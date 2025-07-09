using FastEndpoints;
using VisaSponsorshipScout.API.Common;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation
{
    public class GetAllEndpoint : FastEndpoints.Endpoint<GetOrganisationPagedRequest, ApiResponse<PagedResult<OrganisationResultModel>>>
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
                    await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("No organisation found"), StatusCodes.Status200OK, ct);
                    return;
                }

                await SendOkAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Ok(result.ToModel()), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all organisations");
                await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("Cannot complete request. Try again later"), StatusCodes.Status500InternalServerError, ct);
            }
        }
    }
}