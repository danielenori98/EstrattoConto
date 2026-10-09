using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Oracle.ManagedDataAccess.Client;

namespace LogMonitor.Services
{
    public class AscoltoRabbit : BackgroundService
    {
        private const string HostName = "localhost";
        private const string NomeCoda = "CodaAssicurativaLog";
        private readonly string stringaOracle = "User Id=SYSTEM;Password=Esercizi_123;Data Source=localhost:1521/FREE;";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory { HostName = HostName };
            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: NomeCoda,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;

                        string operazione = root.GetProperty("Operazione").GetString() ?? "";
                        string dataLogStr = root.GetProperty("DataLog").GetString() ?? "";
                        var datiElement = root.GetProperty("Dati");

                        int idRecord = datiElement.GetProperty("ID").GetInt32();
                        string cf = datiElement.GetProperty("CfLavoratore").GetString() ?? "";
                        string datore = datiElement.GetProperty("DatoreLavoro").GetString() ?? "";
                        decimal retribuzione = datiElement.GetProperty("Retribuzione").GetDecimal();

                        using (var conn = new OracleConnection(stringaOracle))
                        {
                            await conn.OpenAsync();

                            string sql = "INSERT INTO LOGMONITOR (OPERAZIONE, DATA_LOG, ID_RECORD, CF_LAVORATORE, DATORE_LAVORO, RETRIBUZIONE) VALUES (:operazione, TO_TIMESTAMP(:dataLog, 'YYYY-MM-DD HH24:MI:SS'), :idRecord, :cf, :datore, :retribuzione)";
                            using (var cmd = new OracleCommand(sql, conn))
                            {
                                // Forza il motore Oracle ad associare i parametri per NOME e non per posizione
                                cmd.BindByName = true;

                                // Definiamo i parametri specificando il tipo di dato esatto di Oracle per evitare conflitti nascosti
                                cmd.Parameters.Add(new OracleParameter("operazione", OracleDbType.Varchar2) { Value = operazione });
                                cmd.Parameters.Add(new OracleParameter("dataLog", OracleDbType.Varchar2) { Value = dataLogStr });
                                cmd.Parameters.Add(new OracleParameter("idRecord", OracleDbType.Int32) { Value = idRecord });
                                cmd.Parameters.Add(new OracleParameter("cf", OracleDbType.Char) { Value = cf });
                                cmd.Parameters.Add(new OracleParameter("datore", OracleDbType.Varchar2) { Value = datore });
                                cmd.Parameters.Add(new OracleParameter("retribuzione", OracleDbType.Decimal) { Value = retribuzione });

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
                catch
                {
                    // Resta silente senza bloccare l'app
                }

                await Task.CompletedTask;
            };

            // Utilizziamo autoAck a true per svuotare la coda istantaneamente
            await channel.BasicConsumeAsync(
                queue: NomeCoda,
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}