using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQServer.contracts;
using RabbitMQServer.interfaces;

namespace RabbitMQServer.services;

public class RabbitMQMessageSender : IRabbitMQMessageSender
{
    private IConnection? _conn;

    public void SendMessage<T>(DataServerRabbitMQ<T> data)
    {
        if (ConnectionExists(data))
        {
            using var channel = _conn?.CreateModel();
            channel?.QueueDeclare(queue: data.QueueName, false, false, false, arguments: null);
            byte[] body = GetMessageAsByteArray(data.BaseMessage);
            channel.BasicPublish(
                exchange: "",
                routingKey: data.QueueName,
                basicProperties: null,
                body: body
            );
        }
    }

    private byte[] GetMessageAsByteArray<T>(T message)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(message, options);
        var body = Encoding.UTF8.GetBytes(json);
        return body;
    }

    private void CreateConnection<T>(DataServerRabbitMQ<T> data)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = data.HostName,
                UserName = data.UserName,
                Password = data.Password,
                VirtualHost = data.VirtualHost
            };
            _conn = factory.CreateConnection();
        }
        catch (Exception)
        {
            throw new ApplicationException(
                "Unable to establish a connection to the rabbitmq server."
            );
        }
    }

    private bool ConnectionExists<T>(DataServerRabbitMQ<T> data)
    {
        if (_conn != null)
            return true;
        CreateConnection(data);
        return _conn != null;
    }
}
