namespace icone_backend.Dtos.Ingridient
{
    public class UpdateIngredientRequest : CreateIngredientRequest
    {
        public bool IsActive { get; set; } = true;

    }
}
