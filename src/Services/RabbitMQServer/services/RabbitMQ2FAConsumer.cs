using System.Text;
using System.Text.Json;
using EmailServices.Contracts;
using EmailServices.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQServer.services;

public class RabbitMQ2FAConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private IConnection _conn;
    private IModel _channel;
    private const string queueName = "2FA-queue";
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMQ2FAConsumer(
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        var factory = new ConnectionFactory
        {
            HostName =
                _configuration.GetSection("RabbitMQServer").GetSection("HostName").Value ?? "",
            UserName =
                _configuration.GetSection("RabbitMQServer").GetSection("Username").Value ?? "",
            Password =
                _configuration.GetSection("RabbitMQServer").GetSection("Password").Value ?? "",
            VirtualHost =
                _configuration.GetSection("RabbitMQServer").GetSection("VirtualHost").Value ?? ""
        };
        _conn = factory.CreateConnection();
        _channel = _conn.CreateModel();
        _channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (chanel, evt) =>
        {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            EmailSendingDetails detailsOfTheEmailToBeSent =
                JsonSerializer.Deserialize<EmailSendingDetails>(content)
                ?? throw new ApplicationException("Email details to be sent.");
            SendEmail(detailsOfTheEmailToBeSent).GetAwaiter().GetResult();
            _channel.BasicAck(evt.DeliveryTag, false);
        };
        _channel.BasicConsume(queueName, false, consumer);
        return Task.CompletedTask;
    }

    private async Task SendEmail(EmailSendingDetails data)
    {
        var username =
            _configuration
                .GetSection("EmailService")
                .GetSection("Credentials")
                .GetSection("Username")
                .Value ?? "";
        var password =
            _configuration
                .GetSection("EmailService")
                .GetSection("Credentials")
                .GetSection("PasswordApp")
                .Value ?? "";

        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var sendEmail = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await sendEmail.SendEmail(username, password, data);
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException(
                "Unable to send email confirmation email. " + ex.Message
            );
        }
    }
}
