using System.Collections.Generic;

namespace EstrattoConto.Models
{
    public class ModelloConto
    {
        public Lavoratore Lavoratore { get; set; } = new Lavoratore();
        public List<ModelloEstrattoConto> Contributi { get; set; } = new List<ModelloEstrattoConto>();
        public List<Lavoratore> TuttiILavoratori { get; set; } = new List<Lavoratore>();
    }
}