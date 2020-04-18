using System.Text.Json.Serialization;

namespace ISNAP_4.Models.PythonSolver
{
    public class Response
    {
        [JsonPropertyName("x")]
        public double[] X { get; set; }
        
        [JsonPropertyName("fun")]
        public double Fun { get; set; }
    }
}