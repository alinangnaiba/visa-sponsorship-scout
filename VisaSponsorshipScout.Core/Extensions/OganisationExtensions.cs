using VisaSponsorshipScout.Core.Entities;

namespace VisaSponsorshipScout.Core.Extensions
{
    public static class OganisationExtensions
    {
        public static bool HasUpdateFrom(this Organisation self, Organisation other)
        {
            if (self == null || other == null)
            {
                return false;
            }
            
            var hasUpdate = self.Name == other.Name &&
                self.TypeAndRatings == other.TypeAndRatings &&
                self.County == other.County &&
                self.Routes.Matches(other.Routes) &&
                self.TownCities.Matches(other.TownCities);
            return hasUpdate;
        }

        private static bool Matches(this List<string> self, List<string> other)
        {
            if (self.Count != other.Count)
            {
                return false;
            }
            for (int i = 0; i < self.Count; i++)
            {
                if (self[i] == other[i])
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
