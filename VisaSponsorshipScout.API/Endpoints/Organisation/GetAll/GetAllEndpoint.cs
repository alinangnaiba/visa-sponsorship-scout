using FastEndpoints;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class GetAllEndpoint : Endpoint<GetOrganisationPagedRequest, PagedResult<OrganisationResultModel>>
    {
        private readonly IDataRetriever _dataRetriever;
        private readonly ILogger<GetAllEndpoint> _logger;

        public GetAllEndpoint(ILogger<GetAllEndpoint> logger, IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("organisations");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationPagedRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _dataRetriever.GetOrganisationListAsync(req.Page);

                if (result.Data.Count == 0)
                {
                    await SendNotFoundAsync(ct);
                    return;
                }

                await SendOkAsync(result.ToModel(), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all organisations");
                throw;
            }
        }
    }
}