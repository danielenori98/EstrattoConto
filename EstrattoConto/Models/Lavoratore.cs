using System;

namespace EstrattoConto.Models
{
    public class Lavoratore
    {
        public string CF { get; set; } = "";
        public string Cognome { get; set; } = "";
        public string Nome { get; set; } = "";
        public DateTime DataNascita { get; set; }
    }
}