using System.ComponentModel.DataAnnotations;

namespace icone_backend.Models
{
    public class IngredientModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;

        // ---- Nutritional composition ----
        public double WaterPct { get; set; }      
        public double FatPct { get; set; }        
        public double ProteinPct { get; set; }    
        public double SugarPct { get; set; }      
        public double LactosePct { get; set; }    
        public double FiberPct { get; set; }      
        public double CarbsPct { get; set; }      
        public double AlcoholPct { get; set; }    

        // ---- Calculated / technological parameters ----
        public double TotalSolidsPct { get; set; } 
        public double Pac { get; set; }            
        public double Pod { get; set; }            
        public double KcalPer100g { get; set; }    

        // Optional extra nutritional fields (se quiser manter no banco)
        public double SodiumMg { get; set; }       
        public double PotassiumMg { get; set; }    
        public double CholesterolMg { get; set; }  

             
    }
}
