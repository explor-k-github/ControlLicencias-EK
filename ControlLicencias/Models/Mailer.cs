using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace ControlLicencias.Models {

    public class Mailer {
        //MailAddress addressFrom = "reportes@explor-k.cl";
        //MailAddress addressTo = "vvasquez@explor-k.cl";
        //MailAddress adressCC = "cc@outlook.com";

        public void SendMailInternal(string subject, string request, string requester) {
            // string subject = "Prueba";
            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Reportes", "reportes@explor-k.cl"));
            mailMessage.Sender = new MailboxAddress("Reportes", "reportes@explor-k.cl");
            mailMessage.To.Add(new MailboxAddress("Solicitudes", "solicitudes@explor-k.cl"));
            //mailMessage.To.Add(new MailboxAddress("Tomas Barros", "tomas.barros@angloamerican.com"));
            //mailMessage.Cc.Add(new MailboxAddress("Claudio Asensio", "casensio@explor-k.cl"));
            mailMessage.Subject = subject;
            //mailMessage.ReplyTo.Add(new MailboxAddress(replyToAddress));
            //mailMessage.Subject = subject;
            var builder = new BodyBuilder();
            //builder.TextBody = "Consulta de Patente Ingresada: \n Patente: " + patente + " \n Empresa: " + empresa + "\n Tipo Vehículo: " + type + " \n Tipo de reporte: " + tipo;
            builder.HtmlBody = "Solicitud Ingresada: <br> Ticket:<b>" + request + "<br> Solicitante: " + requester;
            mailMessage.Body = builder.ToMessageBody();
            try {
                using (var smtpClient = new SmtpClient()) {
                    smtpClient.Connect("mail.explor-k.cl", 587, SecureSocketOptions.None);
                    smtpClient.Authenticate("reportes@explor-k.cl", "Explor2019");

                    smtpClient.Send(mailMessage);
                    Console.WriteLine("Enviado");
                }
            } catch (SmtpCommandException ex) {
                Console.WriteLine("SMTP Error");
            } catch (Exception ex) {
                Console.WriteLine("Excepcion CORREO: " + ex);
            }
        }

        public void SendMailExternal(string subject, string request, string rut, string name, string requester, string requestermail) {
            // string subject = "Prueba";
            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Solicitud Licencia", "solicitudes@explor-k.cl"));
            mailMessage.Sender = new MailboxAddress("Solicitud Licencia", "solicitudes@explor-k.cl");
            mailMessage.To.Add(new MailboxAddress(requester, requestermail));
            //mailMessage.To.Add(new MailboxAddress("Tomas Barros", "tomas.barros@angloamerican.com"));
            //mailMessage.Cc.Add(new MailboxAddress("Claudio Asensio", "casensio@explor-k.cl"));
            mailMessage.Subject = subject;
            //mailMessage.ReplyTo.Add(new MailboxAddress(replyToAddress));
            //mailMessage.Subject = subject;
            var builder = new BodyBuilder();
            //builder.TextBody = "Consulta de Patente Ingresada: \n Patente: " + patente + " \n Empresa: " + empresa + "\n Tipo Vehículo: " + type + " \n Tipo de reporte: " + tipo;
            builder.HtmlBody = "Solicitud Ingresada Exitosamente <br> Numero de Seguimiento: <b>" + request + "<br> Rut: " + rut + "<br> Nombre: " + name;
            mailMessage.Body = builder.ToMessageBody();
            try {
                using (var smtpClient = new SmtpClient()) {
                    smtpClient.Connect("mail.explor-k.cl", 587, SecureSocketOptions.None);
                    smtpClient.Authenticate("solicitudes@explor-k.cl", "explor2019");

                    smtpClient.Send(mailMessage);
                    Console.WriteLine("Enviado");
                }
            } catch (SmtpCommandException ex) {
                Console.WriteLine("SMTP Error");
            } catch (Exception ex) {
                Console.WriteLine("Excepcion CORREO: " + ex);
            }
        }


    }
}
