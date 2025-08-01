﻿namespace VisaSponsorshipScout.Core.Entities
{
    public class Organisation
    {
        public string County { get; set; }
        public string Id { get; set; } // RavenDB uses a string ID
        public string Name { get; set; }
        public List<string> Routes { get; set; }
        public List<string> TownCities { get; set; }
        public List<string> TypeAndRatings { get; set; }
    }
}