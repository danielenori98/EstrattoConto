using System;

namespace EstrattoConto.Models
{
    public class ModelloEstrattoConto
    {
        public int ID { get; set; }
        public string CfLavoratore { get; set; } = "";
        public DateTime PeriodoDal { get; set; }
        public DateTime PeriodoAl { get; set; }
        public string TipoContribuzione { get; set; } = "";
        public int SettimaneUtili { get; set; }
        public decimal Retribuzione { get; set; }
        public string DatoreLavoro { get; set; } = "";
        public string? Note { get; set; }
    }
}