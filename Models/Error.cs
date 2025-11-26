namespace icone_backend.Models
{
    
    public class Error
    {
        public string Code { get; set; } = "UNKNOWN_ERROR";
        public string Message { get; set; } = "Ocorreu um erro inesperado.";
        public string? Details { get; set; }
        public string? Field { get; set; }
        public string? TraceId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
