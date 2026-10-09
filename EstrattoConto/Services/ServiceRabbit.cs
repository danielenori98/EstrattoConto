using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using EstrattoConto.Models;

namespace EstrattoConto.Services
{
    public class ServiceRabbit
    {
        private const string HostName = "localhost";
        private const string NomeCoda = "CodaAssicurativaLog";
        public static async Task InviaNotifica(ModelloEstrattoConto record, string operazione)
        {
            var factory = new ConnectionFactory() { HostName = HostName };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: NomeCoda,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var notifica = new
            {
                Operazione = operazione,
                DataLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Dati = record
            };

            string json = JsonSerializer.Serialize(notifica);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties();
            properties.Persistent = true;

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: NomeCoda,
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
    }
}