using Microsoft.AspNetCore.Mvc;
using IBM.Data.Db2;
using EstrattoConto.Models;

namespace EstrattoConto.Controllers
{
    public class ControllerConto : Controller
    {
        private string stringaConnessione = "Server=localhost:50000;Database=conto;UID=db2admin;PWD=Esercizi_123;";

        public async Task<IActionResult> Index(string cf)
        {
            var conto = new ModelloConto();

            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();
                    string sqlTutti = "SELECT CF, COGNOME, NOME FROM LAVORATORI ORDER BY COGNOME ASC";
                    using (var cmd = new DB2Command(sqlTutti, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                conto.TuttiILavoratori.Add(new Lavoratore
                                {
                                    CF = reader.GetString(0).Trim(),
                                    Cognome = reader.GetString(1).Trim(),
                                    Nome = reader.GetString(2).Trim()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErroreLavoratore = $"Errore elenco lavoratori: {ex.Message}";
                return View(conto);
            }

            if (conto.TuttiILavoratori.Count == 0)
            {
                ViewBag.ErroreLavoratore = "Nessun lavoratore trovato nel database. Popola il DB con il file .bat.";
                return View(conto);
            }

            if (string.IsNullOrEmpty(cf))
            {
                cf = conto.TuttiILavoratori[0].CF;
            }

            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();
                    string sqlLavoratore = "SELECT CF, COGNOME, NOME, DATA_NASCITA FROM LAVORATORI WHERE CF = ?";
                    using (var cmd = new DB2Command(sqlLavoratore, conn))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@CF", DB2Type.Char)).Value = cf;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                conto.Lavoratore = new Lavoratore
                                {
                                    CF = reader.GetString(0).Trim(),
                                    Cognome = reader.GetString(1).Trim(),
                                    Nome = reader.GetString(2).Trim(),
                                    DataNascita = reader.GetDateTime(3)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErroreLavoratore = $"Errore lettura utente: {ex.Message}";
            }

            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();
                    string sqlConto = "SELECT ID, CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE FROM ESTRATTO_CONTO WHERE CF_LAVORATORE = ? ORDER BY PERIODO_DAL ASC";
                    using (var cmd = new DB2Command(sqlConto, conn))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@CF", DB2Type.Char)).Value = cf;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                conto.Contributi.Add(new ModelloEstrattoConto
                                {
                                    ID = reader.GetInt32(0),
                                    CfLavoratore = reader.GetString(1).Trim(),
                                    PeriodoDal = reader.GetDateTime(2),
                                    PeriodoAl = reader.GetDateTime(3),
                                    TipoContribuzione = reader.GetString(4).Trim(),
                                    SettimaneUtili = reader.GetInt32(5),
                                    Retribuzione = reader.GetDecimal(6),
                                    DatoreLavoro = reader.GetString(7).Trim(),
                                    Note = reader.IsDBNull(8) ? "" : reader.GetString(8).Trim()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErroreConto = $"Errore lettura estratto conto: {ex.Message}";
            }

            return View(conto);
        }
    }
}