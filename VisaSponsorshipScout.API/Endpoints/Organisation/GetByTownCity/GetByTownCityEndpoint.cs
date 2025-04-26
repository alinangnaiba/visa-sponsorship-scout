using FastEndpoints;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation.GetByTownCity
{
    public class GetByTownCityEndpoint : Endpoint<GetOrganisationRequest, PagedResult<OrganisationResultModel>>
    {
        private readonly IDataRetriever _dataRetriever;
        private readonly ILogger<GetByTownCityEndpoint> _logger;

        public GetByTownCityEndpoint(ILogger<GetByTownCityEndpoint> logger, IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("organisations/towncity/{Keyword}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _dataRetriever.GetOrganisationByTownCityAsync(req.Keyword, req.Page);

                if (result.Data.Count == 0)
                {
                    await SendNotFoundAsync(ct);
                    return;
                }

                await SendOkAsync(result.ToModel(), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organisations for town/city '{TownCity}'", req.Keyword);
                throw;
            }
        }
    }
}