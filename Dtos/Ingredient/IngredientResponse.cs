namespace icone_backend.Dtos.Ingredient
{
    public class IngredientResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double TotalSolidsPct { get; set; }
        public double Pac { get; set; }
        public double Pod { get; set; }
        public double KcalPer100g { get; set; }
    }
}
