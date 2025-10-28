using Beta_System.Helper;
using Beta_System.Models;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit.Security;
using Microsoft.Ajax.Utilities;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Beta_System.Controllers
{
    public class NOTIFICACIONESController : Controller
    {
        private static BETA_CORPEntities db = new BETA_CORPEntities();
        private static string correo_sib = db.C_parametros_configuracion.Find(5).valor_texto;
        private static string pass_correo_sib = db.C_parametros_configuracion.Find(6).valor_texto;

        private static string correo_sib_repores = db.C_parametros_configuracion.Find(1017).valor_texto;
        private static string pass_correo_sib_reportes = db.C_parametros_configuracion.Find(1018).valor_texto;

        private static int puerto_correo = Convert.ToInt32(db.C_parametros_configuracion.Find(1019).valor_numerico);

        private static string Credencials_User = db.C_parametros_configuracion.Find(1020).valor_texto;
        private static string Credencials_Pass = db.C_parametros_configuracion.Find(1021).valor_texto;
        private static string Host_correo = db.C_parametros_configuracion.Find(1022).valor_texto;


        public int EnviarCorreoUsuario(string asunto, string destinatario, string mensaje)
        {
            try
            {
                var remitente = correo_sib;
                var pass = pass_correo_sib;

                var correo = new MimeMessage();
                correo.From.Add(new MailboxAddress("Ordenes de Compra", remitente));

                // Separar los destinatarios
                if (destinatario.Contains(";"))
                {
                    var cc_correo = destinatario.Split(';').ToArray();
                    correo.To.Add(MailboxAddress.Parse(cc_correo[0]));

                    for (int i = 1; i < cc_correo.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cc_correo[i]))
                        {
                            correo.Cc.Add(MailboxAddress.Parse(cc_correo[i].Trim()));
                        }
                    }
                }
                else
                {
                    correo.To.Add(MailboxAddress.Parse(destinatario));
                }

                correo.Subject = asunto;
                correo.Body = new TextPart("html")
                {
                    Text = mensaje
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate(remitente, pass);
                    smtp.Send(correo);
                    smtp.Disconnect(true);
                }
                RegistrarCorreoLog();
                return 1;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine("Error al enviar correo: " + ex.Message);
                return 0;
            }
        }

        public int EnviarCorreoUsuarioReportes(string asunto, string destinatario, string mensaje)
        {
            try
            {
                var remitente = correo_sib_repores;
                var pass = pass_correo_sib_reportes;

                var correo = new MimeMessage();
                correo.From.Add(new MailboxAddress("Reportes Beta", remitente));

                // Separar los destinatarios
                if (destinatario.Contains(";"))
                {
                    var cc_correo = destinatario.Split(';').ToArray();
                    correo.To.Add(MailboxAddress.Parse(cc_correo[0]));

                    for (int i = 1; i < cc_correo.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cc_correo[i]))
                        {
                            correo.Cc.Add(MailboxAddress.Parse(cc_correo[i].Trim()));
                        }
                    }
                }
                else
                {
                    correo.To.Add(MailboxAddress.Parse(destinatario));
                }

                correo.Subject = asunto;
                correo.Body = new TextPart("html")
                {
                    Text = mensaje
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate(remitente, pass);
                    smtp.Send(correo);
                    smtp.Disconnect(true);
                }
                RegistrarCorreoLog();
                return 1;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public int EnviarCorreoSolicitaAutorizacion(int id_requisicion, string justificacion, string token,int id_usuario,string correo_usuario,string usuario, string centro, string concepto)
        {
            try
            {
                var remitente = correo_sib;
                var pass = pass_correo_sib;
                var correo = new MimeMessage();
                correo.From.Add(new MailboxAddress("Reportes Beta", remitente));

                if (correo_usuario.Contains(";"))
                {
                    var cc_correo = correo_usuario.Split(';').ToArray();
                    correo.To.Add(MailboxAddress.Parse(cc_correo[0]));

                    for (int i = 1; i < cc_correo.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cc_correo[i]))
                        {
                            correo.Cc.Add(MailboxAddress.Parse(cc_correo[i].Trim()));
                        }
                    }
                }
                else
                {
                    correo.To.Add(MailboxAddress.Parse(correo_usuario));
                }

                string link_token = "https://siib.beta.com.mx///REQUISICIONES/RequisicionMasterView?token=" + token + "&&id_usuario_aut=" + id_usuario + "&&id_requisicion=" + id_requisicion + "";

                string mensaje = "<label>El usuario: " + usuario + " solicita la autorización de la requisición: <strong>" + centro + " "+ id_requisicion +"</strong> para su compra</label><br />" +
                    "<br /><label>Concepto: </label><strong>" + concepto + "</strong>" +
                    "<br /><label>Justificación: </label><strong>" + justificacion + "</strong>" +
                    "<br /><hr /><strong>Visita </strong><a href='" + link_token + "'>Requisiciones a autorización por presupuesto </a><strong> para mas información</strong>" +
                    "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";

                correo.Subject = "Solicitud de autorización para la requisición: " + centro + " " + id_requisicion +"";
                correo.Body = new TextPart("html")
                {
                    Text = mensaje
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate(remitente, pass);
                    smtp.Send(correo);
                    smtp.Disconnect(true);
                }
                RegistrarCorreoLog();
                return 1;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public int EnviarCorreoNotificaAutorizacionEspecial(int id_requisicion, string justificacion, string token, int id_usuario, string correo_usuario, string usuario, string centro, string concepto)
        {
            try
            {
                var remitente = correo_sib;
                var pass = pass_correo_sib;
                var correo = new MimeMessage();
                correo.From.Add(new MailboxAddress("Reportes Beta", remitente));

                if (correo_usuario.Contains(";"))
                {
                    var cc_correo = correo_usuario.Split(';').ToArray();
                    correo.To.Add(MailboxAddress.Parse(cc_correo[0]));

                    for (int i = 1; i < cc_correo.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(cc_correo[i]))
                        {
                            correo.Cc.Add(MailboxAddress.Parse(cc_correo[i].Trim()));
                        }
                    }
                }
                else
                {
                    correo.To.Add(MailboxAddress.Parse(correo_usuario));
                }

                string link_token = "https://siib.beta.com.mx///REQUISICIONES/RequisicionMasterView?token=" + token + "&&id_usuario_aut=" + id_usuario + "&&id_requisicion=" + id_requisicion + "";

                string mensaje = "<label>El usuario: <strong>" + usuario + "</strong> autorizó la requisición: <strong>" + centro +" "+ id_requisicion +"</strong></label><br />" +
                    "<br /><label>Concepto: </label><strong>" + concepto + "</strong>" +
                    "<br /><label>Justificación: </label><strong>" + justificacion + "</strong>" +
                    "<br /><hr /><strong>Visita </strong><a href='" + link_token + "'>Requisiciones a autorización por presupuesto </a><strong> para mas información</strong>" +
                    "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";

                correo.Subject = "AVISO DE AUTORIZACION SIN PRESUPUESTO DE LA REQUISICIÓN: " + centro + " " + id_requisicion +"";
                correo.Body = new TextPart("html")
                {
                    Text = mensaje
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate(remitente, pass);
                    smtp.Send(correo);
                    smtp.Disconnect(true);
                }
                RegistrarCorreoLog();
                return 1;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public int TestCorreo()
        {
            try
            {
                var remitente = correo_sib;
                var pass = pass_correo_sib;
                var correo = new MimeMessage();
                correo.From.Add(new MailboxAddress("Reportes Beta", remitente));
                correo.To.Add(MailboxAddress.Parse("eruizgalindo@beta.com.mx"));
                correo.Cc.Add(MailboxAddress.Parse("vmascorro@beta.com.mx"));
                correo.Subject = "CORREO DE PRUEBA";
                correo.Body = new TextPart("html")
                {
                    Text = ""
                };

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                    smtp.Authenticate(remitente, pass);
                    smtp.Send(correo);
                    smtp.Disconnect(true);
                }
                RegistrarCorreoLog();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public string GenerarTokenLongitud(int longitud)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";
            Random random = new Random();
            char[] randomArray = new char[longitud];
            for (int i = 0; i < longitud; i++) { randomArray[i] = chars[random.Next(chars.Length)]; }
            return new string(randomArray);
        }

        public string GenerarTokenSegmentado3x3()
        {
            Random random = new Random();
            int part1 = random.Next(1000);
            int part2 = random.Next(1000);
            string formattedPart1 = part1.ToString("000");
            string formattedPart2 = part2.ToString("000");
            string token = $"{formattedPart1}-{formattedPart2}";
            return token;
        }

        private string GenerarUsuariosProveedores()
        {
            int count_success = 0;
            var proveedores = db.C_compras_proveedores.Where(x => x.activo == true).ToList();
            foreach (var prov in proveedores)
            {
                int id_prov = prov.id_compras_proveedor;
                var valid = db.C_usuarios_proveedores.Where(x => x.id_compras_proveedor == id_prov).FirstOrDefault();
                if (valid == null)
                {
                    C_usuarios_proveedores usuario_new = new C_usuarios_proveedores();
                    usuario_new.id_compras_proveedor = id_prov;
                    usuario_new.pass = GenerarPassLongitud(6);
                    usuario_new.usuario_rfc = prov.RFC;
                    usuario_new.password = PasswordHelper.EncodePassword(usuario_new.pass, "MySalt");
                    usuario_new.activo = true;
                    db.C_usuarios_proveedores.Add(usuario_new);
                    db.SaveChanges();
                }
                else
                {
                    valid.usuario_rfc = prov.RFC;
                    db.SaveChanges();
                }
                count_success++;
            }
            return count_success.ToString();
        }

        private string GenerarPassLongitud(int longitud)
        {
            const string chars = "0123456789";
            Random random = new Random();
            char[] randomArray = new char[longitud];
            for (int i = 0; i < longitud; i++) { randomArray[i] = chars[random.Next(chars.Length)]; }
            return new string(randomArray);
        }

        public int EnviarCorreoPreRegistro(string usuario, string nombre, string rfc, string puesto, string nss, string folio_solicitud)
        {
            try
            {
                var remitente = correo_sib_repores;
                var pass = pass_correo_sib_reportes;

                var empleados_notificacion = db.C_usuarios_masters.Where(x=> x.activo == true && x.id_usuario_master_accion == 2004).ToList();
                foreach (var item in empleados_notificacion)
                {
                    string mensaje = "<label>El usuario " + usuario + " solicita la revisión de una solicitud de alta de empleado para nómina.</label><br />" +
                        "<br /><label>Folio de la solicitud: </label><strong>" + folio_solicitud + "</strong>" +
                        "<br /><label>Nombre: </label><strong>" + nombre + "</strong>" +
                        "<br /><label>RFC: </label><strong>" + rfc + "</strong>" +
                        "<br /><label>Puesto: </label><strong>" + puesto + "</strong>" +
                        "<br /><label>NSS: </label><strong>" + nss + "</strong>" +
                        "<br /><hr />" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";

                    var correo = new MimeMessage();
                    correo.From.Add(new MailboxAddress("Reportes Beta", remitente));
                    correo.Subject = "SOLICITUD DE ALTA DE EMPLEADO PARA NÓMINA";
                    correo.Body = new TextPart("html")
                    {
                        Text = mensaje
                    };

                    using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                    {
                        smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                        smtp.Authenticate(remitente, pass);
                        smtp.Send(correo);
                        smtp.Disconnect(true);
                    }
                    RegistrarCorreoLog();
                }

                return 1;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public int EnviarCorreoValidarPreRegistro(string usuario, string nombre, string rfc, string nss, string folio_solicitud)
        {
            try
            {
                var remitente = correo_sib_repores;
                var pass = pass_correo_sib_reportes;

                var empleados_notificacion = db.C_usuarios_masters.Where(x => x.activo == true && x.id_usuario_master_accion == 2005).ToList();
                foreach (var item in empleados_notificacion)
                {
                    string mensaje = "<label>El usuario " + usuario + " autorizó su solciitud de alta de empleado.</label><br />" +
                        "<br /><label>Folio de la solicitud: </label><strong>" + folio_solicitud + "</strong>" +
                        "<br /><label>Nombre: </label><strong>" + nombre.ToUpper() + "</strong>" +
                        "<br /><label>RFC: </label><strong>" + rfc + "</strong>" +
                        "<br /><label>NSS: </label><strong>" + nss + "</strong>" +
                        "<br /><hr />" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";

                    var correo = new MimeMessage();
                    correo.From.Add(new MailboxAddress("Reportes Beta", remitente));
                    correo.To.Add(MailboxAddress.Parse(item.correo));

                    correo.Subject = "SOLICITUD DE ALTA DE EMPLEADO AUTORIZADA";
                    correo.From.Add(new MailboxAddress("Reportes Beta", remitente));
                    correo.Body = new TextPart("html")
                    {
                        Text = mensaje
                    };

                    using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                    {
                        smtp.Connect(Host_correo, puerto_correo, SecureSocketOptions.SslOnConnect);
                        smtp.Authenticate(remitente, pass);
                        smtp.Send(correo);
                        smtp.Disconnect(true);
                    }
                    RegistrarCorreoLog();

                }

                return 1;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public (DateTime PrimerDiaSemana, DateTime UltimoDiaSemana) ObtenerSemanaCompromisoEdicion(int year, int semana)
        {
            System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            // Iniciar en el primer día del año especificado
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            DateTime startOfWeek = firstDayOfYear;

            // Mover a la primera semana completa si el primer día del año no cae en el primer lunes
            int currentWeek = calendar.GetWeekOfYear(startOfWeek, weekRule, firstDayOfWeek);

            // Encontrar el inicio de la semana solicitada
            while (currentWeek < semana)
            {
                startOfWeek = startOfWeek.AddDays(1);

                if (startOfWeek.DayOfWeek == DayOfWeek.Monday)
                {
                    currentWeek = calendar.GetWeekOfYear(startOfWeek, weekRule, firstDayOfWeek);
                }
            }

            // Ajustar al lunes de la semana especificada si no es lunes ya
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            // Calcular el último día de la semana (domingo)
            DateTime endOfWeek = startOfWeek.AddDays(6);

            // Retornar las fechas directamente
            return (startOfWeek, endOfWeek);
        }

        public bool CompromisosClienteLeche(int anio, int nosemana, int mes)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();

            var fechas = ObtenerSemanaCompromisoEdicion(Convert.ToInt32(cliente.FirstOrDefault().C_envios_leche_programacion_semanal_g.C_presupuestos_anios.anio), nosemana);

            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fechas.PrimerDiaSemana && x.fecha_envio <= fechas.UltimoDiaSemana && x.activo == true).ToList();

            var produccion_semanal = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true &&
            x.C_envios_leche_programacion_diaria_g.no_semana == nosemana && x.C_envios_leche_programacion_diaria_g.activo == true && x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto == anio).ToList();

            var clientes_cumplimiento = cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).Distinct().ToArray();
            var compromiso_semanal = db.C_envios_leche_cumplimiento_semanal.Where(x => x.no_semana == nosemana && x.activo == true && x.C_envios_leche_cumplimiento_mensual.activo == true && x.C_envios_leche_cumplimiento_mensual.id_anio_presupuesto == anio && clientes_cumplimiento.Contains(x.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms)).ToList();


            int mes_ppto = 0;
            if (compromiso_semanal.Count() > 0)
            {
                mes_ppto = (int)compromiso_semanal.FirstOrDefault().C_envios_leche_cumplimiento_mensual.id_mes_presupuesto;
            }
            else
            {
                mes_ppto = mes;
            }
            int semenas_mensual = db.C_envios_leche_cumplimiento_mensual.Where(x => x.id_mes_presupuesto == mes_ppto && x.id_anio_presupuesto == anio).Count();

            List<CompromisoEntregaLeche> lista_compromisos = new List<CompromisoEntregaLeche>();


            //ITERACION POR CLIENTE
            foreach (var item in cliente.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche))
            {
                CompromisoEntregaLeche compromiso = new CompromisoEntregaLeche();
                int idClienteMS = (int)item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche;
                var compromiso_mensual = compromiso_semanal.Where(x => x.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms == idClienteMS).ToList();
                var cliente_leche = item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche;

                //VALIDAR SI EXISTE COMPROMISO MENSUAL
                if (compromiso_mensual.Count() > 0)
                {
                    var idCompromisoMensual = compromiso_mensual.Select(x => x.id_cumplimiento_mensual).ToArray();

                    var observaciones = db.C_envios_leche_cumplimiento_semanal_comentario.Where(x => x.activo == true && idCompromisoMensual.Contains(x.C_envios_leche_cumplimiento_semanal.C_envios_leche_cumplimiento_mensual.id_cumplimiento_mensual)).ToList();

                    decimal fullCompromisoSemanal = ((decimal)compromiso_mensual.FirstOrDefault().C_envios_leche_cumplimiento_mensual.litros_totales / 55000) / semenas_mensual;

                    decimal fullProgramaSemanal = (decimal)cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente_leche).Sum(x => x.cantidad_litros) / 55000;

                    decimal total_estimacion_cumplimiento = fullProgramaSemanal - fullCompromisoSemanal;

                    compromiso.idCumplimientoSemanal = compromiso_mensual.FirstOrDefault().id_cumplimiento_semanal;
                    compromiso.Cliente = item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial;
                    compromiso.fullCompromisoSemanal = fullCompromisoSemanal;
                    compromiso.fullProgramaSemanal = fullProgramaSemanal;

                    if (observaciones.Count > 0)
                    {
                        List<string[]> comentarios = new List<string[]>();
                        //P1-idCompromisoSemanal, P2-idObservacion, P3-Cliente, P4-Usuario, P5-Comentarios
                        foreach (var obs in observaciones.Where(x => x.C_envios_leche_cumplimiento_semanal.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms == cliente_leche))
                        {
                            comentarios.Add(new string[]
                            {
                            obs.id_cumplimiento_semanal.ToString(),
                            obs.id_cumplimiento_semanal_comentario.ToString(),
                            obs.C_envios_leche_cumplimiento_semanal.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms.ToString(),
                            obs.C_usuarios_corporativo.usuario,
                            obs.comentario,
                            obs.fecha_registro.ToString()
                            }); ;
                        }
                        compromiso.observaciones = comentarios.ToArray();
                    }
                    else
                    {
                        compromiso.observaciones = null;
                    }

                    lista_compromisos.Add(compromiso);
                }
                //SI NO EXISTE
                else
                {
                    decimal estimacion_establo = (decimal)cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente_leche).Sum(x => x.cantidad_litros) / 55000;

                    compromiso.idCumplimientoSemanal = 0;
                    compromiso.Cliente = item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial;
                    compromiso.fullCompromisoSemanal = 0;
                    compromiso.fullProgramaSemanal = estimacion_establo;
                    compromiso.observaciones = null;

                    lista_compromisos.Add(compromiso);
                }
            }
            string comentario_general = "";
            if (compromiso_semanal.Count() > 0)
            {
                comentario_general = compromiso_semanal.FirstOrDefault().comentario;
            }

            ViewBag.MesCompromiso = mes;
            ViewBag.AnioCompromiso = anio;
            ViewBag.NoSemanaCompromiso = nosemana;

            bool exito_correo = true;
            var usuario_notificacion = db.C_usuarios_masters.Where(x => x.activo == true && x.id_usuario_master_accion == 2010).Distinct().ToList();
            foreach (var usuarionoti in usuario_notificacion)
            {
                string articulosMensaje = "<br /><strong>Articulos:</strong><br />";
                articulosMensaje += "<table border='0.5' style= \"border-color:black\"> ";
                articulosMensaje += "<tr style=\"border: 2px solid black; background-color: rgb(46, 135, 215); color:white;\">" +
                    "<th style=\"padding-left:10px; padding-right:10px;\">Cliente</th>" +
                    "<th style=\"padding-left:10px; padding-right:10px;\">Full por semana</th>" +
                    "<th style=\"padding-left:10px; padding-right:10px;\">Full programado</th>" +
                    "<th style=\"padding-left:10px; padding-right:10px;\">Diferencia</th>" +
                    "<th style=\"padding-left:10px; padding-right:10px;\">Observacion</th>" +
                    "</tr>";

                foreach (var compromiso in lista_compromisos)
                {
                    string comentario = "";
                    decimal diferencia = compromiso.fullProgramaSemanal - compromiso.fullCompromisoSemanal;
                    articulosMensaje += "<tr style=\"border: 2px solid black; background-color: rgb(236, 244, 251);\">";
                    articulosMensaje += $"<td style=\"padding-left:10px; padding-right:10px;\">{compromiso.Cliente}</td>";
                    articulosMensaje += $"<td style=\"padding-left:10px; padding-right:10px;\">{compromiso.fullCompromisoSemanal.ToString("F2")}</td>";
                    articulosMensaje += $"<td style=\"padding-left:10px; padding-right:10px;\">{compromiso.fullProgramaSemanal.ToString("F2")}</td>";
                    articulosMensaje += $"<td style=\"padding-left:10px; padding-right:10px;\">{diferencia.ToString("F2")}</td>";
                    if (compromiso.observaciones != null)
                    {
                        foreach (var item in compromiso.observaciones)
                        {
                            comentario = comentario + item[3] + ": " + item[4] + "<br />" + item[5] + "<br />";
                        }
                    }
                    articulosMensaje += $"<td style=\"padding-left:10px; padding-right:10px;\">{comentario}</td>";
                    articulosMensaje += "</tr>";
                }
                articulosMensaje += "</table>";
                string mensaje =
                "<strong>Compromiso de la semana " + nosemana + "</strong>" +
                "<br /><label>" + comentario_general.Replace("\n", "<br/>") + "</label>" +
                "<br />" +
                articulosMensaje +
                "<br />" +
                "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='170'/>";

                if (EnviarCorreoUsuarioReportes("Compromisos envio de leche", usuarionoti.correo, mensaje) == 0)
                {
                    exito_correo = false;
                    return exito_correo;
                }

            }
            return exito_correo;
        }


        public bool RegistrarCorreoLog()
        {
            try
            {
                C_logs_correos_enviados log = new C_logs_correos_enviados();
                log.fecha_envio = DateTime.Now;
                try
                {
                    log.IP = Request.UserHostAddress;
                    log.id_usuario = (int)Session["LoggedId"];
                    
                }
                catch (Exception)
                {
                    log.id_usuario = null;
                    log.IP = null;
                }
                db.C_logs_correos_enviados.Add(log);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}