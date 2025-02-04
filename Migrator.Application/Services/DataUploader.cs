using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Migrator.Application.Dto;
using Migrator.Application.Mapping;
using Migrator.Core.Entities;
using Raven.Client.Documents;
using System.Collections.Concurrent;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        //will insert duplicates. 
        //TODO: find a way to check the db and only insert if it's not a duplicate, otherwise just update
        public async Task<int> SaveAsync(IFormFile file)
        {
            var recordQueue = new ConcurrentQueue<OrganisationDto>();
            var semaphore = new SemaphoreSlim(_maxParallel);
            int total = 0;

            var readTask = Task.Run(() => ReadCsv(file, recordQueue));
            var updateSession = _documentStore.OpenAsyncSession();
            var currentData = await updateSession.Query<Organisation>().ToListAsync();
            var currentDataDictionary = currentData.ToDictionary(data => $"{data.Name.Trim()}{data.Route.Trim()}", data => data);
            var tasks = new List<Task>();
            while (!readTask.IsCompleted || !recordQueue.IsEmpty)
            {
                if (recordQueue.Count >= _batchSize || (readTask.IsCompleted && !recordQueue.IsEmpty))
                {
                    await semaphore.WaitAsync();
                    tasks.Add(ProcessBatchAsync(currentDataDictionary, recordQueue, semaphore, () => Interlocked.Increment(ref total)));
                }
            }

            await Task.WhenAll(tasks);
            return total;
        }

        private async Task ProcessBatchAsync(Dictionary<string, Organisation> currentDataDictionary, ConcurrentQueue<OrganisationDto> recordQueue, SemaphoreSlim semaphore, Func<int> increment)
        {
            try
            {
                var batch = new List<Organisation>();
                while (batch.Count < _batchSize && recordQueue.TryDequeue(out var dto))
                {
                    var key = $"{dto.Name.Trim()}{dto.Route.Trim()}";
                    if (currentDataDictionary.TryGetValue(key, out var existingData))
                    {
                        existingData.TownCity = dto.TownCity.Trim();
                        existingData.County = dto.County.Trim();
                        existingData.TypeAndRating = dto.TypeAndRating.Trim();
                        existingData.Route = dto.Route.Trim();
                    }
                    else
                    {
                        batch.Add(CreateOrganisationEntity(dto));
                    }
                    increment();
                }

                using var bulkInsert = _documentStore.BulkInsert();
                foreach (var organisation in batch)
                {
                    await bulkInsert.StoreAsync(organisation);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void ReadCsv(IFormFile file, ConcurrentQueue<OrganisationDto> recordQueue)
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

            while (csv.Read())
            {
                var organisation = csv.GetRecord<OrganisationDto>();
                recordQueue.Enqueue(organisation);
            }
        }

        private Organisation CreateOrganisationEntity(OrganisationDto data)
        {
            return new Organisation
            {
                Id = Guid.NewGuid().ToString(),
                Name = data.Name,
                County = data.County,
                Route = data.Route,
                TownCity = data.TownCity,
                TypeAndRating = data.TypeAndRating,
            };
        }
    }
}
