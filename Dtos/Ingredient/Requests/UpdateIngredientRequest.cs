namespace icone_backend.Dtos.Ingredient.Requests
{
    public class UpdateIngredientRequest : CreateIngredientRequest
    {
        public bool IsActive { get; set; } = true;

    }
}
