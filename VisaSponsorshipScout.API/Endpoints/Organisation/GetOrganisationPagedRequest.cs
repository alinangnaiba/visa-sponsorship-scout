using FastEndpoints;

namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class GetOrganisationPagedRequest
    {
        [QueryParam]
        public int Page { get; set; }
    }
}