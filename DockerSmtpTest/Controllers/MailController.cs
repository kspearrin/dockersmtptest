using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DockerSmtpTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ILogger<MailController> _logger;
        private readonly IConfiguration _configuration;

        public MailController(ILogger<MailController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<object> Get()
        {
            try
            {
                var client = new SmtpClient(_configuration.GetValue<string>("smtp:host"),
                    _configuration.GetValue<int>("smtp:port"))
                {
                    EnableSsl = _configuration.GetValue<bool>("smtp:ssl"),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(_configuration.GetValue<string>("smtp:username"),
                        _configuration.GetValue<string>("smtp:password"))
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_configuration.GetValue<string>("smtp:from")),
                    Subject = "Testing SMTP Email",
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8,
                    BodyTransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable,
                    Body = "<p>Hi, This is a test email.</p>",
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(_configuration.GetValue<string>("smtp:to")));
                client.SendCompleted += (sender, e) =>
                {
                    if(e.Error != null)
                    {
                        _logger.LogError(e.Error, "SendCompleted Error");
                    }

                    if(e.Cancelled)
                    {
                        _logger.LogError("SendCompleted Cancelled");
                    }

                    message.Dispose();
                    client.Dispose();
                };

                client.SendAsync(message, null);
                return "Mail sent.";
            }
            catch(Exception e)
            {
                _logger.LogError(e, "There was an exception.");
                return "There was an exception.";
            }
        }
    }
}
