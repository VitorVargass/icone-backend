using icone_backend.Dtos.Auth;
using icone_backend.Dtos.Ingridient;
using icone_backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace icone_backend.Controllers
{

    [ApiController]
    [Route("ingredient")]
    public class IngredientController : ControllerBase
    {

        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _ingredientService.GetAllAsync();
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
        public async Task<IActionResult> Create([FromBody] CreateIngredientRequest request)
        {
            var created = await _ingredientService.CreateAsync(request);
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
