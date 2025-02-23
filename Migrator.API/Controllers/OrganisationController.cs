using Microsoft.AspNetCore.Mvc;
using Migrator.API.Extensions;
using Migrator.Application.Adapters;
using Migrator.Application.Services;
using Migrator.Core.Enums;

namespace Migrator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganisationController : ControllerBase
    {
        private readonly IDataRetriever _dataRetriever;
        private readonly IDataUploader _dataUploader;

        public OrganisationController(IDataRetriever dataRetriever, IDataUploader dataUploader)
        {
            _dataRetriever = dataRetriever;
            _dataUploader = dataUploader;
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

        [HttpGet("county/{county}/{page}")]
        public async Task<IActionResult> GetByCountyPaged(string county, int page)
        {
            var result = await _dataRetriever.GetOrganisationByCountyAsync(county, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            return Ok(result.ToModel());
        }

        [HttpGet("town-or-city/{townOrCity}/{page}")]
        public async Task<IActionResult> GetByTownOrCityPaged(string townOrCity, int page)
        {
            var result = await _dataRetriever.GetOrganisationByTownCityAsync(townOrCity, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            var resultModel = result.ToModel();
            return Ok(resultModel);
        }

        [HttpGet("name/{name}/{page}")]
        public async Task<IActionResult> GetByNamePaged(string name, int page)
        {
            var result = await _dataRetriever.GetOrganisationByNameAsync(name, page);
            if (result.Data.Count == 0)
            {
                return NotFound(result.ToModel());
            }
            return Ok(result.ToModel());
        }

        [HttpGet("routes")]
        public IActionResult GetRoutes()
        {
            return Ok(WorkerRoutes.GetRoutes());
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

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (file == null || !Path.GetExtension(file.FileName).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
            {
                return BadRequest("Invalid file uploaded.");
            }

            var totalChanges = await _dataUploader.SaveAsync(file);
            return Ok($"Total changes: {totalChanges}");
        }
    }
}
