using VisaSponsorshipScout.Application.Dto;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System.Linq.Expressions;
using VisaSponsorshipScout.Core.Models;
using VisaSponsorshipScout.Core.Entities;

namespace VisaSponsorshipScout.Application.Services
{
    public interface IDataRetriever
    {
        Task<PagedResult<DuplicteResponse>> GetDuplicates();
        Task<PagedResult<Organisation>> GetOrganisationListAsync(int page);
        Task<PagedResult<Organisation>> GetOrganisationByCountyAsync(string county, int page);
        Task<PagedResult<Organisation>> GetOrganisationByTownCityAsync(string townOrCity, int page);
        Task<PagedResult<Organisation>> GetOrganisationByNameAsync(string townOrCity, int page);
    }

    public class DataRetriever : IDataRetriever
    {
        const int _pageSize = 20;
        private readonly IAsyncDocumentSession _session;

        public DataRetriever(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<PagedResult<DuplicteResponse>> GetDuplicates()
        {
            var result = new PagedResult<DuplicteResponse>();
            var nameGroups = await _session.Query<Organisation>()
            .GroupBy(x => x.Name)
            .Select(g => new DuplicteResponse
            {
                Name = g.Key,
                Count = g.Count()
            }).ToListAsync();
            var data = nameGroups
            .Where(x => x.Count > 1)
            .ToList();
            result.Data = data;
            result.TotalResult = result.Data.Count;
            result.PageSize = _pageSize;
            result.CurrentPage = 1;
            result.TotalPages = (int)Math.Ceiling((double)result.TotalResult / _pageSize);
            return result;
        }

        public async Task<PagedResult<Organisation>> GetOrganisationByCountyAsync(string county, int page = 1)
        {
            return await FindByPagedAsync(org => org.County == county, page);
        }

        public async Task<PagedResult<Organisation>> GetOrganisationByNameAsync(string name, int page = 1)
        {
            return await SearchAsync(name, page);
        }

        public async Task<PagedResult<Organisation>> GetOrganisationByTownCityAsync(string townOrCity, int page = 1)
        {
            return await FindByPagedAsync(org => org.TownCities.Contains(townOrCity), page);
        }

        public async Task<PagedResult<Organisation>> GetOrganisationListAsync(int page = 1)
        {
            return await FindByPagedAsync(null, page);
        }

        private async Task<PagedResult<Organisation>> FindByPagedAsync(Expression<Func<Organisation, bool>>? predicate, int page)
        {
            var result = new PagedResult<Organisation>();
            var query = _session.Query<Organisation>();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            result.TotalResult = await query.CountAsync();
            result.Data = await query
                .Statistics(out var stats)
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .OrderBy(org => org.Name)
                .ToListAsync();

            result.PageSize = _pageSize;
            result.CurrentPage = page;
            result.TotalPages = (int)Math.Ceiling((double)result.TotalResult / _pageSize);

            return result;
        }

        private async Task<PagedResult<Organisation>> SearchAsync(string keyword, int page)
        {
            var result = new PagedResult<Organisation>();
            var query = _session.Query<Organisation>();
            query = query.Search(org => org.Name, keyword);
            result.TotalResult = await query.CountAsync();
            result.Data = await query
                .Statistics(out var stats)
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .OrderBy(org => org.Name)
                .ToListAsync();

            result.PageSize = _pageSize;
            result.CurrentPage = page;
            result.TotalPages = (int)Math.Ceiling((double)result.TotalResult / _pageSize);

            return result;
        }
    }
}
