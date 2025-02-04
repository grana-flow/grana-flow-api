using RabbitMQServer.contracts;

namespace RabbitMQServer.interfaces;

public interface IRabbitMQMessageSender
{
    public void SendMessage<T>(DataServerRabbitMQ<T> data);
}
