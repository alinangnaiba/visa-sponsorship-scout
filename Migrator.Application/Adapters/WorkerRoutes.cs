using Migrator.Core.Enums;

namespace Migrator.Application.Adapters
{
    public static class WorkerRoutes
    {
        private static  Dictionary<int, string> _routes = new Dictionary<int, string>
        {
            { (int)RouteEnum.CharityWorker, "Charity Worker" },
            { (int)RouteEnum.CreativeWorker, "Creative Worker" },
            { (int)RouteEnum.GlobalBusinessMobility_GraduateTrainee, "Global Business Mobility: Graduate Trainee" },
            { (int)RouteEnum.GlobalBusinessMobility_SecondmentWorker, "Global Business Mobility: Secondment Worker" },
            { (int)RouteEnum.GlobalBusinessMobility_SeniorOrSpecialistWorker, "Global Business Mobility: Senior or Specialist Worker" },
            { (int)RouteEnum.GlobalBusinessMobility_ServiceSupplier, "Global Business Mobility: Service Supplier" },
            { (int)RouteEnum.GlobalBusinessMobility_UKExpansionWorker, "Global Business Mobility: UK Expansion Worker" },
            { (int)RouteEnum.GovernmentAuthorisedExchange, "Government Authorised Exchange" },
            { (int)RouteEnum.InternationalAgreement, "International Agreement" },
            { (int)RouteEnum.InternationalSportsperson, "International Sportsperson" },
            { (int)RouteEnum.IntraCompanyTransfers_ICT, "Intra Company Transfers (ICT)" },
            { (int)RouteEnum.IntraCompanyRoutes, "Intra-company Routes" },
            { (int)RouteEnum.ReligiousWorker, "Religious Worker" },
            { (int)RouteEnum.ScaleUp, "Scale-up" },
            { (int)RouteEnum.SeasonalWorker, "Seasonal Worker" },
            { (int)RouteEnum.SkilledWorker, "Skilled Worker" },
            { (int)RouteEnum.Tier2MinistersOfReligion, "Tier 2 Ministers of Religion" }
        };

        public static Dictionary<int, string> GetRoutes()
        {
            return _routes;
        }

        public static bool TryGetValue(RouteEnum route, out string value)
        {
            value = string.Empty;
            if (!Enum.IsDefined(typeof(RouteEnum), (int)route))
            {
                return false;
            }
            var hasValue = _routes.TryGetValue((int)route, out var val);
            value = val ?? value;
            return hasValue;
        }
    }
}
