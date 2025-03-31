using FastEndpoints;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class GetAllEndpoint : Endpoint<GetOrganisationPagedRequest, PagedResult<OrganisationResultModel>>
    {
        private readonly IDataRetriever _dataRetriever;

        public GetAllEndpoint(IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
        }

        public override void Configure()
        {
            Get("organisations");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationPagedRequest req, CancellationToken ct)
        {
            var result = await _dataRetriever.GetOrganisationListAsync(req.Page);

            if (result.Data.Count == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(result.ToModel(), ct);
        }
    }
}