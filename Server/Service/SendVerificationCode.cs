using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
using Rolling.ViewModels;
using Server.Hub;

namespace Server.Service;

public class SendVerificationCode
{
    private const string FromPassword = "uvos umpw nqqh lxpk";
    private const string FromAddress = "shaihnurov3@gmail.com";

    public static async Task<string> SendVerification(string name, string email)
    {
        string code = GenerateCode();
        var fromAddress = new MailAddress(FromAddress, "Rolling");
        var toAddress = new MailAddress(email);

        const string subject = "Registering an account with Rolling";
        string body = $"{name}, your confirmation code - {code}";

        var smtp = new SmtpClient()
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, FromPassword)
        };

        try
        {
            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            await smtp.SendMailAsync(message);
        }
        catch (SmtpException ex)
        {
            throw new SmtpException($"Error sending confirmation code to mail {email}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("There was an internal error: " + ex.Message, ex);
        }

        return code;
    }
    private static string GenerateCode()
    {
        byte[] data = new byte[6];
        RandomNumberGenerator.Fill(data);
        return BitConverter.ToString(data).Replace("-", "").Substring(0, 6);
    }
}