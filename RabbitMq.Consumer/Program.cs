using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            
            await channel.QueueDeclareAsync(
                queue: "message", 
                durable: true, 
                exclusive: false, 
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Waiting for messages...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                // Getting the message from the queue
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Receveid: {message}");

                // Now we must aknowledge the broker that we consumed the message
                // This way, the message will be removed from the queue

               await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                // setting multiple to false means that we only going to ack the current message
            };

            // Actually consuming the messages
            await channel.BasicConsumeAsync("message", autoAck: false, consumer: consumer);

            // only to not close the terminal
            Console.ReadLine();
        }
    }
}
