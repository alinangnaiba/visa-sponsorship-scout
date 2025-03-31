using Microsoft.AspNetCore.Mvc;
using VisaSponsorshipScout.API.Extensions;
using VisaSponsorshipScout.Application.Adapters;
using VisaSponsorshipScout.Application.Services;
using VisaSponsorshipScout.Core.Enums;

namespace VisaSponsorshipScout.API.Controllers
{
    //TODO: DELETE THIS FILE
    //No longer used since the introduction of FastEndpoints
    //keeping it for now for reference
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationController : ControllerBase
    {
        private readonly IDataRetriever _dataRetriever;

        public OrganisationController(IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
        }

        [HttpGet("{page}")]
        public async Task<IActionResult> GetAll(int page)
        {
            var result = await _dataRetriever.GetOrganisationListAsync(page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            return Ok(result.ToModel());
        }

        [HttpGet("county/{county}")]
        public async Task<IActionResult> GetByCountyPaged(string county, [FromQuery] int page)
        {
            var result = await _dataRetriever.GetOrganisationByCountyAsync(county, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            return Ok(result.ToModel());
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByNamePaged(string name, [FromQuery] int page)
        {
            var result = await _dataRetriever.GetOrganisationByNameAsync(name, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            return Ok(result.ToModel());
        }

        [HttpGet("town-or-city/{townOrCity}")]
        public async Task<IActionResult> GetByTownOrCityPaged(string townOrCity, [FromQuery] int page)
        {
            var result = await _dataRetriever.GetOrganisationByTownCityAsync(townOrCity, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            var resultModel = result.ToModel();
            return Ok(resultModel);
        }

        [HttpGet("routes/{id}")]
        public IActionResult GetRouteById(int id)
        {
            if (!WorkerRoutes.TryGetValue((RouteEnum)id, out string value))
            {
                return BadRequest("Invalid worker route.");
            }

            return Ok(new { Data = value, Success = true });
        }

        [HttpGet("routes")]
        public IActionResult GetRoutes()
        {
            return Ok(WorkerRoutes.GetRoutes());
        }
    }
}