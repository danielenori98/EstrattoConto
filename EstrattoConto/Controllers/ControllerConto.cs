using EstrattoConto.Models;
using EstrattoConto.Services;
using IBM.Data.Db2;
using Microsoft.AspNetCore.Mvc;

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
                ViewBag.ErroreLavoratore = "Nessun lavoratore trovato nel database.";
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

        public IActionResult Nuovo(string cf)
        {
            var nuovoRecord = new ModelloEstrattoConto { CfLavoratore = cf };
            return View(nuovoRecord);
        }

        [HttpPost]
        public async Task<IActionResult> Nuovo(ModelloEstrattoConto record)
        {
            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();

                    bool lavoratoreEsiste = false;
                    string sqlCheck = "SELECT COUNT(*) FROM LAVORATORI WHERE CF = ?";
                    using (var cmdCheck = new DB2Command(sqlCheck, conn))
                    {
                        cmdCheck.Parameters.Add(new DB2Parameter("@CF", DB2Type.VarChar)).Value = record.CfLavoratore.Trim();
                        int conto = Convert.ToInt32(await cmdCheck.ExecuteScalarAsync());
                        if (conto > 0) lavoratoreEsiste = true;
                    }

                    if (!lavoratoreEsiste)
                    {
                        string sqlInsLavoratore = "INSERT INTO LAVORATORI (CF, COGNOME, NOME, DATA_NASCITA) VALUES (?, 'CognomeNuovo', 'NomeNuovo', '2000-01-01')";
                        using (var cmdInsLav = new DB2Command(sqlInsLavoratore, conn))
                        {
                            cmdInsLav.Parameters.Add(new DB2Parameter("@CF", DB2Type.VarChar)).Value = record.CfLavoratore.Trim();
                            await cmdInsLav.ExecuteNonQueryAsync();
                        }
                    }

                    string sql = "INSERT INTO ESTRATTO_CONTO (CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
                    using (var cmd = new DB2Command(sql, conn))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@CF", DB2Type.VarChar)).Value = record.CfLavoratore.Trim();
                        cmd.Parameters.Add(new DB2Parameter("@DAL", DB2Type.Date)).Value = record.PeriodoDal;
                        cmd.Parameters.Add(new DB2Parameter("@AL", DB2Type.Date)).Value = record.PeriodoAl;
                        cmd.Parameters.Add(new DB2Parameter("@TIPO", DB2Type.VarChar)).Value = record.TipoContribuzione;
                        cmd.Parameters.Add(new DB2Parameter("@SETT", DB2Type.Integer)).Value = record.SettimaneUtili;
                        cmd.Parameters.Add(new DB2Parameter("@RETR", DB2Type.Decimal)).Value = record.Retribuzione;
                        cmd.Parameters.Add(new DB2Parameter("@DITTA", DB2Type.VarChar)).Value = record.DatoreLavoro;
                        cmd.Parameters.Add(new DB2Parameter("@NOTE", DB2Type.VarChar)).Value = record.Note ?? "";

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await ServiceRabbit.InviaNotifica(record, "inserito");

                return RedirectToAction("Index", new { cf = record.CfLavoratore.Trim() });
            }
            catch (Exception ex)
            {
                ViewBag.Errore = $"Errore salvataggio: {ex.Message}";
                return View(record);
            }
        }
        public async Task<IActionResult> Modifica(int id)
        {
            var record = new ModelloEstrattoConto();
            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();
                    string sql = "SELECT ID, CF_LAVORATORE, PERIODO_DAL, PERIODO_AL, TIPO_CONTRIBUZIONE, SETTIMANE_UTILI, RETRIBUZIONE, DATORE_LAVORO, NOTE FROM ESTRATTO_CONTO WHERE ID = ?";
                    using (var cmd = new DB2Command(sql, conn))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@ID", DB2Type.Integer)).Value = id;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                record.ID = reader.GetInt32(0);
                                record.CfLavoratore = reader.GetString(1).Trim();
                                record.PeriodoDal = reader.GetDateTime(2);
                                record.PeriodoAl = reader.GetDateTime(3);
                                record.TipoContribuzione = reader.GetString(4).Trim();
                                record.SettimaneUtili = reader.GetInt32(5);
                                record.Retribuzione = reader.GetDecimal(6);
                                record.DatoreLavoro = reader.GetString(7).Trim();
                                record.Note = reader.IsDBNull(8) ? "" : reader.GetString(8).Trim();
                            }
                        }
                    }
                }
                return View(record);
            }
            catch (Exception ex)
            {
                return Content($"Errore recupero record: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Modifica(ModelloEstrattoConto record)
        {
            try
            {
                using (var conn = new DB2Connection(stringaConnessione))
                {
                    await conn.OpenAsync();
                    string sql = "UPDATE ESTRATTO_CONTO SET PERIODO_DAL = ?, PERIODO_AL = ?, TIPO_CONTRIBUZIONE = ?, SETTIMANE_UTILI = ?, RETRIBUZIONE = ?, DATORE_LAVORO = ?, NOTE = ? WHERE ID = ?";
                    using (var cmd = new DB2Command(sql, conn))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@DAL", DB2Type.Date)).Value = record.PeriodoDal;
                        cmd.Parameters.Add(new DB2Parameter("@AL", DB2Type.Date)).Value = record.PeriodoAl;
                        cmd.Parameters.Add(new DB2Parameter("@TIPO", DB2Type.VarChar)).Value = record.TipoContribuzione;
                        cmd.Parameters.Add(new DB2Parameter("@SETT", DB2Type.Integer)).Value = record.SettimaneUtili;
                        cmd.Parameters.Add(new DB2Parameter("@RETR", DB2Type.Decimal)).Value = record.Retribuzione;
                        cmd.Parameters.Add(new DB2Parameter("@DITTA", DB2Type.VarChar)).Value = record.DatoreLavoro;
                        cmd.Parameters.Add(new DB2Parameter("@NOTE", DB2Type.VarChar)).Value = record.Note ?? "";
                        cmd.Parameters.Add(new DB2Parameter("@ID", DB2Type.Integer)).Value = record.ID;
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                await ServiceRabbit.InviaNotifica(record, "modificato");
                return RedirectToAction("Index", new { cf = record.CfLavoratore });
            }
            catch (Exception ex)
            {
                ViewBag.Errore = $"Errore aggiornamento: {ex.Message}";
                return View(record);
            }
        }
    }
}