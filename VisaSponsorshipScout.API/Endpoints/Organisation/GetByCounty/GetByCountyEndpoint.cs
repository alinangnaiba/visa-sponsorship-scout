using FastEndpoints;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation.GetByCounty
{
    public class GetByCountyEndpoint : Endpoint<GetOrganisationRequest, PagedResult<OrganisationResultModel>>
    {
        private readonly IDataRetriever _dataRetriever;

        public GetByCountyEndpoint(IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
        }

        public override void Configure()
        {
            Get("organisations/county/{Keyword}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationRequest req, CancellationToken ct)
        {
            var result = await _dataRetriever.GetOrganisationByCountyAsync(req.Keyword, req.Page);

            if (result.Data.Count == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(result.ToModel(), ct);
        }
    }
}