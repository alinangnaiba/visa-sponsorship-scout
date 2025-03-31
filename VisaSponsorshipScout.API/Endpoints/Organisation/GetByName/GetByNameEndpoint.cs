using FastEndpoints;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation.GetByName
{
    public class GetByNameEndpoint : Endpoint<GetOrganisationRequest, PagedResult<OrganisationResultModel>>
    {
        private readonly IDataRetriever _dataRetriever;

        public GetByNameEndpoint(IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
        }

        public override void Configure()
        {
            Get("organisations/name/{Keyword}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationRequest req, CancellationToken ct)
        {
            var result = await _dataRetriever.GetOrganisationByNameAsync(req.Keyword, req.Page);

            if (result.Data.Count == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(result.ToModel(), ct);
        }
    }
}