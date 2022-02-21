using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Core.Utilities
{
	public static class Email
	{
		#region Connections

		private static MailMessage _mail;
		private static SmtpClient _client;
		static Email()
		{
			_mail = new MailMessage();
			_client = new SmtpClient();
		}

		#endregion

		#region Methods

		public static void SendEmail(string sendTo, string subjectText, string bodyText)
		{
			try
			{
				_mail.From = new MailAddress("amirtahakazemtabarzahraei@gmail.com");
				_mail.To.Add(sendTo);
				_mail.Subject = subjectText;
				_mail.Body = bodyText;
				_mail.IsBodyHtml = true;

				_client.Host = "smtp.gmail.com";
				_client.Port = 587;
				_client.EnableSsl = true;
				_client.UseDefaultCredentials = false;
				_client.DeliveryMethod = SmtpDeliveryMethod.Network;
				_client.Credentials = new NetworkCredential("amirtahakazemtabarzahraei@gmail.com", "amirtahafilm");
				_client.Send(_mail);

			}
			catch
			{
				throw new Exception("There is a problem to connect server");
			}
			finally
			{
				_mail.Dispose();
				_client.Dispose();
			}
		}

		#endregion
	}
}
