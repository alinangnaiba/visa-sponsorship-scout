using Migrator.Core.Entities;
using Raven.Client.Documents.Indexes;

namespace Migrator.Infrastructure.Configuration
{
    internal class Organisation_ByName : AbstractIndexCreationTask<Organisation>
    {
        public Organisation_ByName()
        {
            Map = organisations => from org in organisations
                                   select new
                                   {
                                       OrganisationName = org.OrganisationName,
                                   };

            Indexes.Add(x => x.OrganisationName, FieldIndexing.Search);
        }
    }
}
