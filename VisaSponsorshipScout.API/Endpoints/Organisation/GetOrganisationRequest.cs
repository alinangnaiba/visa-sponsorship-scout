using FastEndpoints;

namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class GetOrganisationRequest : GetOrganisationPagedRequest
    {
        [RouteParam]
        public string Keyword { get; set; }
    }
}