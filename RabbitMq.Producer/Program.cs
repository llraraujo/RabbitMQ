using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Producer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // this is a idempotent operation. It will creates a queue if that queue dont exists on the MSB
            await channel.QueueDeclareAsync(
                queue: "message", // name of the queue
                durable: true, // setting to true means that all the messages produced will survive a broker restart
                exclusive: false, // setting to true means that the queue is exclusive to the connection that she was declared
                autoDelete: false, // setting to true means that the queue should be deleted when the last subscriber unsubscribe
                arguments: null);

            for(int i = 0; i < 10; i++)
            {
                var message = $"{DateTime.UtcNow} - {Guid.NewGuid()}";
                var body = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: "message", // the same name of the declared queue
                    mandatory: true, // setting to true means that will be routed to a queue
                    basicProperties: new BasicProperties { Persistent = true }, // setting Persistent to true means that the message must be persisted inside of the queue,
                    body: body
                    );

                Console.WriteLine($"Sent : {message}");

                await Task.Delay(2000);
            }
        }
    }
}
