using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;

public static class EmailHandler
{
    public static void SendEmail(string _subject, string _bodyText) {
        MailMessage messageToSend = new MailMessage();
        SmtpClient StmpServer = new SmtpClient("smtp.gmail.com");
        StmpServer.Timeout = 10000;
        StmpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        StmpServer.UseDefaultCredentials = false;
        StmpServer.Port = 587;

        StmpServer.EnableSsl = true;

        messageToSend.From = new MailAddress("maximilianmcdiarmid@gmail.com");
        messageToSend.To.Add(new MailAddress("maximilianmcdiarmid@gmail.com"));

        messageToSend.Subject ="[MoleculeMaker3D] " + _subject;
        messageToSend.Body = _bodyText;

        StmpServer.Credentials = new System.Net.NetworkCredential("maximilianmcdiarmid@gmail.com", "jjbn bihi aglw skdc") as ICredentialsByHost;
        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            return true;
        };

        messageToSend.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        StmpServer.Send(messageToSend);
    }
}