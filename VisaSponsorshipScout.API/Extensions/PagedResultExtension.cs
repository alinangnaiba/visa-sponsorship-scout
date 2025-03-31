using VisaSponsorshipScout.API.ResultModels;
using VisaSponsorshipScout.Core.Entities;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Extensions
{
    internal static class PagedResultExtension
    {
        internal static PagedResult<OrganisationResultModel> ToModel(this PagedResult<Organisation> paged)
        {
            return new PagedResult<OrganisationResultModel>
            {
                CurrentPage = paged.CurrentPage,
                Data = paged.Data.ToModel(),
                PageSize = paged.PageSize,
                TotalPages = paged.TotalPages,
                TotalResult = paged.TotalResult
            };
        }

        private static List<OrganisationResultModel> ToModel(this List<Organisation> entities)
        {
            if (entities is null || entities.Count == 0)
            {
                return [];
            }
            return entities.Select(entity => entity.ToModel()).ToList();
        }

        private static OrganisationResultModel ToModel(this Organisation entity)
        {
            return new OrganisationResultModel
            {
                County = entity.County,
                Name = entity.Name,
                Routes = entity.Routes,
                TownCities = entity.TownCities,
                TypeAndRatings = entity.TypeAndRatings
            };
        }
    }
}