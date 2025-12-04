namespace icone_backend.Dtos.Ingridient
{
    public class CreateIngredientRequest
    {
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public double WaterPct { get; set; }
        public double FatPct { get; set; }
        public double ProteinPct { get; set; }
        public double SugarPct { get; set; }
        public double FiberPct { get; set; }
        public double LactosePct { get; set; }
        public double CarbsPct { get; set; }
        public double AlcoholPct { get; set; }
        public double Pac { get; set; }
        public double Pod { get; set; }
        public double KcalPer100g { get; set; }
        public double SodioMg { get; set; }
        public double PotassioMg { get; set; }
        public double ColesteroloMg { get; set; }
    }
}
