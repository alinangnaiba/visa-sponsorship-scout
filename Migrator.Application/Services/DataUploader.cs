using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Migrator.Application.Dto;
using Migrator.Application.Mapping;
using Migrator.Core.Entities;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Collections.Concurrent;
using System.Globalization;

namespace Migrator.Application.Services
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

            var readTask = Task.Run(() => ReadCsv(file, recordQueue));
                        
            var tasks = new List<Task>();
            while (!readTask.IsCompleted || !recordQueue.IsEmpty)
            {
                if (recordQueue.Count >= _batchSize || (readTask.IsCompleted && !recordQueue.IsEmpty))
                {
                    await semaphore.WaitAsync();
                    tasks.Add(ProcessBatchAsync(recordQueue, semaphore, () => Interlocked.Increment(ref total)));
                }
            }

            await Task.WhenAll(tasks);
            return total;
        }

        private async Task ProcessBatchAsync(ConcurrentQueue<Organisation> recordQueue, SemaphoreSlim semaphore, Func<int> increment)
        {
            try
            {
                var batch = new List<Organisation>();
                
                while (batch.Count < _batchSize && recordQueue.TryDequeue(out var org))
                {
                    batch.Add(org);
                    increment();
                }
                await CheckAndUpsertAsync(batch);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task CheckAndUpsertAsync(List<Organisation> batch)
        {
            var names = batch.Select(x => x.Name.Trim()).ToList();
            var session = _documentStore.OpenAsyncSession();
            var currentData = await session.Query<Organisation>()
                .Where(org => org.Name.In(names))
                .ToListAsync();
            var currentDataDictionary = currentData.ToDictionary(data => $"{data.Name.Trim()}", data => data);
            var bulkInsert = _documentStore.BulkInsert();

            foreach (var org in batch)
            {
                var key = $"{org.Name.Trim()}";
                if (currentDataDictionary.TryGetValue(key, out var existingData))
                {
                    existingData.TownCities = org.TownCities;
                    existingData.County = org.County.Trim();
                    existingData.TypeAndRatings = org.TypeAndRatings;
                    existingData.Routes = org.Routes;
                }
                else
                {
                    org.Id = Guid.NewGuid().ToString();
                    await bulkInsert.StoreAsync(org);
                }
            }
            await session.SaveChangesAsync();
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
