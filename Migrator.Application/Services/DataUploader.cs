using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Migrator.Application.Mapping;
using Migrator.Core.Entities;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Globalization;

namespace Migrator.Application.Services
{
    public interface IDataUploader
    {
        public Task<int> SaveAsync(IFormFile file);
    }

    public class DataUploader : IDataUploader
    {
        private readonly IAsyncDocumentSession _session;

        public DataUploader(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<int> SaveAsync(IFormFile file)
        {
            var csvData = new List<Organisation>();

            using var stream = new StreamReader(file.OpenReadStream());
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",", // Specifies the delimiter
                HasHeaderRecord = true, // Indicates that the CSV contains a header row
                TrimOptions = TrimOptions.Trim, // Trims extra whitespace
                BadDataFound = null, // Suppresses errors for malformed rows
                HeaderValidated = null, // Ignores missing or unexpected headers
                MissingFieldFound = null // Ignores missing fields in a row
            };

            using var csv = new CsvReader(stream, config);
            csv.Context.RegisterClassMap<OrganisationMap>();
            csvData = csv.GetRecords<Organisation>().ToList();

            var csvDataDictionary = csvData.ToDictionaryIgnoringDuplicates(data => $"{data.OrganisationName.Trim()}{data.Route.Trim()}", data => data);

            var currentData = await _session.Query<Organisation>().ToListAsync();
            var currentDataDictionary = currentData.ToDictionary(data => $"{data.OrganisationName.Trim()}{data.Route.Trim()}", data => data);
            var updatedList = new List<Organisation>();
            foreach (var exportData in csvDataDictionary)
            {
                if (currentDataDictionary.TryGetValue(exportData.Key, out var existingData))
                {
                    existingData.TownCity = exportData.Value.TownCity.Trim();
                    existingData.County = exportData.Value.County.Trim();
                    existingData.TypeAndRating = exportData.Value.TypeAndRating.Trim();
                    existingData.Route = exportData.Value.Route.Trim();
                }
                else
                {
                    exportData.Value.Id = Guid.NewGuid().ToString();
                    updatedList.Add(exportData.Value);
                }
            }
            foreach (var organisation in updatedList)
            {
                await _session.StoreAsync(organisation);
            }

            await _session.SaveChangesAsync();
            return updatedList.Count;
        }
    }
}
