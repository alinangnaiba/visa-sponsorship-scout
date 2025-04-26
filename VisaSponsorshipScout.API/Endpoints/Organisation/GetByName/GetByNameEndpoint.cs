using FastEndpoints;
using VisaSponsorshipScout.API.Common;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Models;

namespace VisaSponsorshipScout.API.Endpoints.Organisation.GetByName
{
    public class GetByNameEndpoint : Endpoint<GetOrganisationRequest, ApiResponse<PagedResult<OrganisationResultModel>>>
    {
        private readonly IDataRetriever _dataRetriever;
        private readonly ILogger<GetByNameEndpoint> _logger;

        public GetByNameEndpoint(ILogger<GetByNameEndpoint> logger, IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("organisations/name/{Keyword}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetOrganisationRequest req, CancellationToken ct)
        {
            try
            {
                var result = await _dataRetriever.GetOrganisationByNameAsync(req.Keyword, req.Page);
                if (result.Data.Count == 0)
                {
                    await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("No organisation found"), StatusCodes.Status404NotFound, ct);
                    return;
                }

                await SendOkAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Ok(result.ToModel()), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for organisations with name containing '{Keyword}'", req.Keyword);
                await SendAsync(ApiResponse<PagedResult<OrganisationResultModel>>.Fail("Cannot complete request. Try again later"), StatusCodes.Status500InternalServerError, ct);
            }
        }
    }
}