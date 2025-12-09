using icone_backend.Dtos.Additives;
using icone_backend.Dtos.Additives.Requests;
using icone_backend.Dtos.Additives.Responses;
using icone_backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace icone_backend.Controllers
{
    [ApiController]
    [Route("additives")]
    [Authorize]
    public class AdditivesController : ControllerBase
    {
        private readonly IAdditiveInterface _service;

        public AdditivesController(IAdditiveInterface service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdditiveResponse>>> GetAll([FromQuery] Guid? companyId)
        {
            var result = await _service.GetAllAsync(companyId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdditiveResponse>> GetById(int id)
        {
            var additive = await _service.GetByIdAsync(id);
            if (additive == null)
                return NotFound(new { message = "Additive not found" });

            return Ok(additive);
        }

        [HttpPost]
        public async Task<ActionResult<AdditiveResponse>> Create([FromBody] AdditiveRequest request, [FromQuery] Guid? companyId)
        {
            var created = await _service.CreateAsync(request, companyId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AdditiveResponse>> Update(int id, [FromBody] AdditiveRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);
            if (updated == null)
                return NotFound(new { message = "Additive not found" });

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Additive not found" });

            return NoContent();
        }
    }
}
