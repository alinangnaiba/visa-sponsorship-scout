using CsvHelper.Configuration;
using Migrator.Core.Entities;

namespace Migrator.Application.Mapping
{
    internal class OrganisationMap : ClassMap<Organisation>
    {
        internal OrganisationMap()
        {
            Map(o => o.OrganisationName).Name("Organisation Name");
            Map(o => o.TownCity).Name("Town/City");
            Map(o => o.County).Name("County");
            Map(o => o.TypeAndRating).Name("Type & Rating");
            Map(o => o.Route).Name("Route");
        }
    }
}
