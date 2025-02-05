namespace Migrator.API.ResultModels
{
    public class OrganisationResultModel
    {
        public string Name { get; set; }
        public List<string> TownCities { get; set; }
        public string County { get; set; }
        public List<string> TypeAndRatings { get; set; }
        public List<string> Routes { get; set; }
    }
}
