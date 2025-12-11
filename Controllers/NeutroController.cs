using icone_backend.Dtos.Ingredient.Responses;
using icone_backend.Dtos.Neutral.Requests;
using icone_backend.Dtos.Neutral.Responses;
using icone_backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace icone_backend.Controllers
{
    [ApiController]
    [Route("neutral")]
    [Authorize]
    public class NeutralsController : ControllerBase
    {
        private readonly INeutral _neutralService;

        public NeutralsController(INeutral neutralService)
        {
            _neutralService = neutralService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NeutralResponse>>> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
        {
            var neutrals = await _neutralService.GetAllAsync(companyId, ct);
            return Ok(neutrals);
        }

       
        [HttpGet("{id:int}")]
        public async Task<ActionResult<NeutralResponse>> GetById(int id, CancellationToken ct)
        {
            var neutral = await _neutralService.GetByIdAsync(id, ct);
            if (neutral == null)
                return NotFound(new { message = "Neutral not found" });

            return Ok(neutral);
        }
        
        [HttpPost]
        public async Task<ActionResult<NeutralResponse>> Create( [FromBody] CreateNeutralRequest request, [FromQuery] Guid? companyId, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid neutral payload" });

            var created = await _neutralService.CreateAsync(request, companyId, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

       
        [HttpPut("{id:int}")]
        public async Task<ActionResult<NeutralResponse>> Update(
            int id,
            [FromBody] CreateNeutralRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid neutral payload" });

            var updated = await _neutralService.UpdateAsync(id, request, ct);
            if (updated == null)
                return NotFound(new { message = "Neutral not found" });

            return Ok(updated);
        }

        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken ct)
        {
            var deleted = await _neutralService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(new { message = "Neutral not found" });

            return NoContent();
        }

        [HttpPost("analyze-draft")]
        public async Task<ActionResult<AdditiveScoresDto>> AnalyzeDraft([FromBody] CreateNeutralRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid neutral payload" });

            var scores = await _neutralService.AnalyzeDraftAsync(request, ct);

            return Ok(scores);
        }
    }
}
