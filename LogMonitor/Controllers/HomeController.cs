using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace LogMonitor.Controllers
{
    public class LogRiga
    {
        public int ID { get; set; }
        public string Operazione { get; set; } = "";
        public DateTime DataLog { get; set; }
        public int IdRecord { get; set; }
        public string CfLavoratore { get; set; } = "";
        public string DatoreLavoro { get; set; } = "";
        public decimal Retribuzione { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly string stringaOracle = "User Id=SYSTEM;Password=Esercizi_123;Data Source=localhost:1521/FREE;";

        public async Task<IActionResult> Index()
        {
            var listaLog = new List<LogRiga>();

            try
            {
                using (var conn = new OracleConnection(stringaOracle))
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID, OPERAZIONE, DATA_LOG, ID_RECORD, CF_LAVORATORE, DATORE_LAVORO, RETRIBUZIONE FROM LOGMONITOR ORDER BY DATA_LOG DESC";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                listaLog.Add(new LogRiga
                                {
                                    ID = reader.GetInt32(0),
                                    Operazione = reader.GetString(1).Trim(),
                                    DataLog = reader.GetDateTime(2),
                                    IdRecord = reader.GetInt32(3),
                                    CfLavoratore = reader.GetString(4).Trim(),
                                    DatoreLavoro = reader.GetString(5).Trim(),
                                    Retribuzione = reader.GetDecimal(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Errore = $"Errore lettura Oracle: {ex.Message}";
            }

            return View(listaLog);
        }
    }
}