namespace Migrator.Core.Entities
{
    public class Organisation
    {
        public string Id { get; set; } // RavenDB uses a string ID
        public string OrganisationName { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string TypeAndRating { get; set; }
        public string Route { get; set; }
    }
}
