﻿using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailSender
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string message, List<Attachment>? attachments = null)
    {
        try
        {
            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:Port"] ?? ""),
                Credentials = new NetworkCredential(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:SenderEmail"] ?? ""),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.Headers.Add("X-Priority", "3");
            mailMessage.Headers.Add("X-Mailer", "Microsoft Outlook");
            mailMessage.Headers.Add("Return-Path", _config["EmailSettings:SenderEmail"]);

            mailMessage.To.Add(email);

            // Add attachments if provided
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    mailMessage.Attachments.Add(attachment);
                }
            }

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch(Exception ex) {
            Console.WriteLine("Email Sender Error: " + ex.Message);
        }
    }
}
