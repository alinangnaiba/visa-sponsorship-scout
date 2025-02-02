using Microsoft.AspNetCore.Mvc;
using Migrator.Application.Services;

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

        [HttpGet("county/{county}")]
        public async Task<IActionResult> GetByCounty(string county)
        {
            var result = await _dataRetriever.GetOrganisationByCountyAsync(county);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(new {Data = result, Total = result.Count, Successful = true });
        }

        [HttpGet("town-or-city/{townOrCity}")]
        public async Task<IActionResult> GetByTownOrCity(string townOrCity)
        {
            var result = await _dataRetriever.GetOrganisationByTownCityAsync(townOrCity);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(new { Data = result, Total = result.Count, Successful = true });
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var result = await _dataRetriever.GetOrganisationByNameAsync(name);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(new { Data = result, Total = result.Count, Successful = true });
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
