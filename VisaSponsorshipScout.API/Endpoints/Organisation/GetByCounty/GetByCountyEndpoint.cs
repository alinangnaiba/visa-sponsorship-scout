using FastEndpoints;
using VisaSponsorshipScout.API.Common;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation.GetByCounty
{
    public class GetByCountyEndpoint : Endpoint<GetOrganisationRequest, ApiResponse<PagedResult<OrganisationResultModel>>>
    {
        private readonly IDataRetriever _dataRetriever;
        private readonly ILogger<GetByCountyEndpoint> _logger;

        public GetByCountyEndpoint(ILogger<GetByCountyEndpoint> logger, IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("organisations/county/{Keyword}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _dataRetriever.GetOrganisationByCountyAsync(req.Keyword, req.Page);

                if (result.Data.Count == 0)
                {
                    await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("No organisation found"), StatusCodes.Status404NotFound, ct);
                    return;
                }

                await SendOkAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Ok(result.ToModel()), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organisations for county '{County}'", req.Keyword);
                await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("Cannot complete request. Try again later"), StatusCodes.Status500InternalServerError, ct);
            }
        }
    }
}