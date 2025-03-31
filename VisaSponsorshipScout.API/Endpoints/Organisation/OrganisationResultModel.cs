namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class OrganisationResultModel
    {
        public string County { get; set; }
        public string Name { get; set; }
        public List<string> Routes { get; set; }
        public List<string> TownCities { get; set; }
        public List<string> TypeAndRatings { get; set; }
    }
}