using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using VisaSponsorshipScout.Application.Dto;
using VisaSponsorshipScout.Application.Mapping;
using VisaSponsorshipScout.Core.Extensions;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using VisaSponsorshipScout.Core.Entities;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Session;

namespace VisaSponsorshipScout.Application.Services
{
    public interface IDataUploader
    {
        public Task<int> SaveAsync(IFormFile file);
    }

    public class DataUploader : IDataUploader
    {
        const int _batchSize = 5000;
        const int _maxParallel = 4;
        private readonly IDocumentStore _documentStore;

        public DataUploader(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task<int> SaveAsync(IFormFile file)
        {
            var recordQueue = new ConcurrentQueue<Organisation>();
            var semaphore = new SemaphoreSlim(_maxParallel);
            int total = 0;
            var session = _documentStore.OpenAsyncSession();
            var existingOrgs = (await GetOrganisationsFromStreamAsync(session)).ToConcurrentDictionary(org => org.Name);
            var readTask = Task.Run(() => ReadCsv(file, recordQueue));
                        
            var tasks = new List<Task>();
            while (!readTask.IsCompleted || !recordQueue.IsEmpty)
            {
                if (recordQueue.Count >= _batchSize || (readTask.IsCompleted && !recordQueue.IsEmpty))
                {
                    await semaphore.WaitAsync();
                    tasks.Add(ProcessBatchAsync(recordQueue, existingOrgs, semaphore, () => Interlocked.Increment(ref total)));
                }
            }

            await Task.WhenAll(tasks);
            await session.SaveChangesAsync();
            return total;
        }

        private async Task<List<Organisation>> GetOrganisationsFromStreamAsync(IAsyncDocumentSession session)
        {
            List<Organisation> organisations = new();
            var query = session.Query<Organisation>();
            await using (var stream = await session.Advanced.StreamAsync(query))
            {
                while (await stream.MoveNextAsync())
                {
                    var entity = stream.Current.Document;
                    organisations.Add(entity);
                }
            }
            return organisations;
        }

        private async Task ProcessBatchAsync(ConcurrentQueue<Organisation> recordQueue, ConcurrentDictionary<string, Organisation> existingOrgs, SemaphoreSlim semaphore, Func<int> increment)
        {
            try
            {
                var batch = new List<Organisation>();

                var items = 0;
                while (items < _batchSize && recordQueue.TryDequeue(out var org))
                {
                    if (existingOrgs.TryGetValue(org.Name, out var existingOrg))
                    {
                        if (existingOrg.HasUpdateFrom(org))
                        {
                            existingOrg.TownCities = org.TownCities;
                            existingOrg.County = org.County.Trim();
                            existingOrg.TypeAndRatings = org.TypeAndRatings;
                            existingOrg.Routes = org.Routes;
                        }
                    }
                    else
                    {
                        batch.Add(org);
                    }
                    increment();
                    items++;
                }
                await InsertAsync(batch);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task InsertAsync(List<Organisation> batch)
        {
            if (batch.Count == 0)
            {
                return;
            }
            using BulkInsertOperation bulkInsert = _documentStore.BulkInsert();
            foreach (var org in batch)
            {
                org.Id = Guid.NewGuid().ToString();
                await bulkInsert.StoreAsync(org);
            }
        }

        private static void ReadCsv(IFormFile file, ConcurrentQueue<Organisation> recordQueue)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null
            };
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<OrganisationMap>();
            var dtos = new List<OrganisationDto>();
            while (csv.Read())
            {
                dtos.Add(csv.GetRecord<OrganisationDto>());
            }

            var organisations = dtos
                .GroupBy(org => new { org.Name })
                .Select(group => new Organisation
                {
                    Name = group.Key.Name,
                    TownCities = group.Select(o => o.TownCity).Distinct().ToList(),
                    County = group.First().County,
                    TypeAndRatings = group.Select(o => o.TypeAndRating).Distinct().ToList(),
                    Routes = group.Select(o => o.Route).Distinct().ToList()
                })
                .ToList();

            foreach (var org in organisations)
            {
                recordQueue.Enqueue(org);
            }
        }
    }
}
