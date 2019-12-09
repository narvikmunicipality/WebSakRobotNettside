using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

namespace WebSakFilopplaster.Net_AD
{
    public class MailHelper
    {
        public static void SendMail(string to, string subject, string body)
        {
            var passord = WebConfigurationManager.AppSettings["RobotPassword"];
#if DEBUG
            to = "andreas.jansson@narvik.kommune.no";
#endif
            SmtpClient client = new SmtpClient("smtp.narvik.kommune.no", 25);
            client.Credentials = new NetworkCredential("roboen@narvik.kommune.no", passord);
            client.UseDefaultCredentials = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("info@app-uipath.narvik.kommune.no");
            mail.Sender = mail.From;
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body + StringConstants.MAIL_DONOTREPLY;
            mail.IsBodyHtml = true;

            client.Send(mail);
            Logger.Log(LogFile.MAIL_LOG, $"<{to}> {body}");
        }

        public static void SendDeleteSuperUserMail(string to)
        {
            SendMail(to, "Endring av tilgang", "Din superbrukertilgang på <a href=https://app-uipath.narvik.kommune.no>app-uipath</a> har blitt fjernet.");
        }

        public static void SendAddSuperUserMail(string to)
        {
            SendMail(to, "Endring av tilgang", "Du har fått superbrukertilgang på <a href=https://app-uipath.narvik.kommune.no>app-uipath</a>.");
        }

        public static void SendDeleteRaadmannUserMail(string to)
        {
            SendMail(to, "Endring av tilgang", "Din tilgang til å se status og logger på <a href=https://app-uipath.narvik.kommune.no>app-uipath</a> har blitt fjernet.");
        }

        public static void SendAddRaadmannUserMail(string to)
        {
            SendMail(to, "Endring av tilgang", "Du har fått tilgang til å se status og logger på <a href=https://app-uipath.narvik.kommune.no>app-uipath</a>.");
        }

        public static void SendEsignatureError(string to, string message)
        {
            SendMail(to, "Feil med e-signeringsoppdrag", message);
        }

        public static void SendExceptionMail(string exception)
        {
            foreach (var superUser in AnsattHelper.Superbrukere)
            {
                if (superUser.MottaFeilmeldinger)
                {
                    SendMail(superUser.EPost, "Teknisk feil", exception);
                }
            }
        }
    }
}