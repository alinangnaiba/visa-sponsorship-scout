using CsvHelper.Configuration;
using VisaSponsorshipScout.Application.Dto;

namespace VisaSponsorshipScout.Application.Mapping
{
    internal class OrganisationMap : ClassMap<OrganisationDto>
    {
        internal OrganisationMap()
        {
            Map(o => o.Name).Name("Organisation Name");
            Map(o => o.TownCity).Name("Town/City");
            Map(o => o.County).Name("County");
            Map(o => o.TypeAndRating).Name("Type & Rating");
            Map(o => o.Route).Name("Route");
        }
    }
}
