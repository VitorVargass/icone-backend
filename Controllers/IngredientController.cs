using icone_backend.Dtos.Ingredient.Requests;
using icone_backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace icone_backend.Controllers
{

    [ApiController]
    [Route("ingredients")]
    [Authorize]
    public class IngredientController : ControllerBase
    {

        private readonly IIngredientInterface _ingredientService;

        public IngredientController(IIngredientInterface ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] Guid? companyId)
        {
            var result = await _ingredientService.GetAllAsync(companyId);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _ingredientService.GetByIdAsync(id);
            if (result == null) return NotFound("Ingredient not find.");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIngredientRequest request, [FromQuery] Guid? companyId)
        {
            var created = await _ingredientService.CreateAsync(request,companyId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIngredientRequest request)
        {
            var updated = await _ingredientService.UpdateAsync(id, request);
            if (updated == null) return NotFound("Ingredient not find.");
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _ingredientService.DeleteAsync(id);
            if (!deleted) return NotFound("Ingredient not find.");
            return NoContent();
        }
    }
}
