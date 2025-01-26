using Migrator.Core.Entities;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Application.Services
{
    public interface IDataRetriever
    {
        Task<List<Organisation>> GetOrganisationListAsync();
        Task<List<Organisation>> GetOrganisationByCountyAsync(string county);
        Task<List<Organisation>> GetOrganisationByTownCityAsync(string townOrCity);
        Task<List<Organisation>> GetOrganisationByNameAsync(string townOrCity);
    }

    public class DataRetriever : IDataRetriever
    {
        private readonly IAsyncDocumentSession _session;

        public DataRetriever(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<List<Organisation>> GetOrganisationByCountyAsync(string county)
        {
            return await FindByAsync(org => org.County == county);
        }

        public async Task<List<Organisation>> GetOrganisationByNameAsync(string name)
        {
            return await _session.Query<Organisation>()
                .Search(org => org.OrganisationName, name)
                .ToListAsync();
        }

        public async Task<List<Organisation>> GetOrganisationByTownCityAsync(string townOrCity)
        {
            return await FindByAsync(org => org.TownCity == townOrCity);
        }

        public async Task<List<Organisation>> GetOrganisationListAsync()
        {
            return await _session.Query<Organisation>().ToListAsync();
        }

        private async Task<List<Organisation>> FindByAsync(Expression<Func<Organisation, bool>> predicate)
        {
            return await _session.Query<Organisation>()
                .Where(predicate)
                .ToListAsync();
        }
    }
}
