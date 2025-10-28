using Beta_System.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Beta_System.Controllers
{
    public class ORDENES_COMPRAController : Controller
    {
        private static BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        private CATALOGOSController catalogos = new CATALOGOSController();
        private static string valor_dolar = db.C_parametros_configuracion.Find(1015).valor_numerico.ToString();


        NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();
        public ActionResult OrdenesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6022)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("../COMPRAS/ORDENES_COMPRA/ADMINISTRAR_ORDENES/Index");
        }

        public PartialViewResult ConsultarOrdenesCompra(string fecha_inicio, string fecha_fin, int[] centro, int[] tipo_orden, int[] proveedor, int recepcionada)
        {
            bool[] recepcionadas;
            if (recepcionada == 2) { recepcionadas = new bool[] { true, false }; }
            else if (recepcionada == 1) { recepcionadas = new bool[] { true }; }
            else { recepcionadas = new bool[] { false }; }

            List<C_compras_ordenes_g> ordenes = null;
            int id_usuario = (int)Session["LoggedId"];
            try
            {
                if (centro.Contains(0)) { centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }

                if (tipo_orden.Contains(0)) { tipo_orden = catalogos.ConsultarTiposRequisicionesID(); }

                if (proveedor.Contains(0)) { proveedor = catalogos.ConsultarProveedoresID(); }

                if (fecha_inicio != null || fecha_fin != null)
                {
                    DateTime fecha_i = DateTime.Parse(fecha_inicio);
                    DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                    ordenes = db.C_compras_ordenes_g.Where(x => x.activo == true && centro.Contains((int)x.id_centro) && proveedor.Contains((int)x.id_proveedor) && tipo_orden.Contains((int)x.id_tipo_orden)
                    && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && recepcionadas.Contains((bool)x.entregado)).OrderBy(x => x.id_compras_orden_g).ToList();
                }
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                ordenes = null;
            }
            ViewBag.valor_dolar = valor_dolar; //db.C_parametros_configuracion.Find(1015).valor_numerico.ToString();
            return PartialView("../COMPRAS/ORDENES_COMPRA/ADMINISTRAR_ORDENES/_OrdenesCompraGeneradasTable", ordenes);
        }

        public PartialViewResult ConsultarOrdenCompraDetalle(int id_orden_g)
        {
            var detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g).ToList();
            ViewBag.valor_dolar = valor_dolar;
            return PartialView("../COMPRAS/ORDENES_COMPRA/ADMINISTRAR_ORDENES/_OrdenCompraDetalleTable", detalle);
        }

        public bool ReenviarOrdenCompra(int id_orden_g, string token_orden, string correo_destino, string folio_orden)
        {
            try
            {
                string correo_comprador = "";
                try
                {
                    var parcialidad = db.C_compras_ordenes_g.Find(id_orden_g).C_compras_cotizaciones_confirmadas_g.C_usuarios_corporativo;
                    correo_comprador = parcialidad.C_empleados.nombres + " " + parcialidad.C_empleados.apellido_paterno;
                }
                catch (Exception)
                {
                }

                string mensaje = "<strong>Buen día.</strong><br />" +
                                   "<br /><label>BETA SANTA MONICA solicita una orden de compra: </label>" +
                                   "<br /><strong>Requisición: " + folio_orden + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                                   "<br /><strong>Comprador: " + correo_comprador + "</strong>" +
                                   "<br /><br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                                   "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new' width='200'/>";
                notificaciones.EnviarCorreoUsuario("ORDEN DE COMPRA PARA BETA SANTA MONICA: #" + id_orden_g + "", correo_destino, mensaje);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //1- Ya se entrego
        //2- Exito
        //3- Error
        public int CancelarOrdenCompra(int id_orden_g, string justificacion)
        {
            try
            {
                var orden_g = db.C_compras_ordenes_g.Find(id_orden_g);

                //ORDEN D
                var ordenes_compra_d = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_g == id_orden_g).ToList();
                foreach (var orden in ordenes_compra_d)
                {
                    //var cada_orden_d = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_d == orden.id_compras_orden_d && x.cantidad_entregada > 0 && x.entregado == true).FirstOrDefault();
                    var cada_orden_d = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_d == orden.id_compras_orden_d && x.cantidad_entregada > 0).FirstOrDefault();
                    if (cada_orden_d != null)
                    {
                        return 1;
                    }
                }

                //ORDEN G
                orden_g.id_status_orden = 3; //Cancelada
                orden_g.activo = false;
                db.SaveChanges();

                //Solicitud Master
                if (orden_g.C_compras_cotizaciones_confirmadas_g.solicita_autorizacion == false)
                {
                    var ordenes_d = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_g == id_orden_g).ToList();
                    foreach (var item in ordenes_d)
                    {
                        item.activo = false;
                    }
                    db.SaveChanges();
                }

                //PARCIALIDAD
                var id_cotizaciones_requis = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == orden_g.id_cotizacion_confirmada_g).Select(x => x.id_cotizacion_requisicion).ToArray();
                //db.C_compras_cotizaciones_requisiciones.Where(x => id_cotizaciones_requis.Contains(x.id_cotizacion_requisicion)).ToList().ForEach(x => { x.agrupada = false; x.orden_generada = null; });
                db.C_compras_cotizaciones_requisiciones.Where(x => id_cotizaciones_requis.Contains(x.id_cotizacion_requisicion)).ToList().ForEach(x => { x.agrupada = false; x.orden_generada = null; x.confirmada = false; x.activo = false; x.fecha_confirmacion = null; });
                db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == orden_g.id_cotizacion_confirmada_g).ToList().ForEach(x => x.activo = false);
                var cotizacion_confirmada = db.C_compras_cotizaciones_confirmadas_g.Find(orden_g.id_cotizacion_confirmada_g);
                cotizacion_confirmada.activo = false;
                cotizacion_confirmada.id_status_cotizacion_confirmada = 3;
                db.SaveChanges();

                //var requid = db.C_compras_requi_d.Where(x=>x.id_requisicion_articulo_g == orden_g.id_requisicion_articulo_g).ToList();
                //foreach (var item in requid)
                //{
                //    var articulo_cantidades = db.C_compras_cotizaciones_requisiciones.Where(x=>x.id_articulo == item.id_articulo && x.activo == true && x.cantidad_surtir != null).Sum(x=>x.cantidad_surtir);

                //    if(articulo_cantidades == null)
                //    {
                //        item.cantidad_cotizada = 0;
                //    }
                //    else
                //    {
                //        item.cantidad_cotizada = articulo_cantidades;
                //    }

                //    if(item.cantidad_cotizada != item.cantidad)
                //    {
                //        item.cotizado = null;
                //    }
                //}

                db.SaveChanges();

                //CORREOS
                try
                {
                    string nombre_comprador = "";
                    string correo_comprador = "";
                    string correo_proveedor = "";
                    try
                    {
                        var parcialidad = db.C_compras_ordenes_g.Find(id_orden_g).C_compras_cotizaciones_confirmadas_g;
                        nombre_comprador = parcialidad.C_usuarios_corporativo.C_empleados.nombres + " " + parcialidad.C_usuarios_corporativo.C_empleados.apellido_paterno;
                        correo_comprador = parcialidad.C_usuarios_corporativo.C_empleados.correo;
                        correo_proveedor = parcialidad.correo_proveedor_suborden;
                    }
                    catch (Exception) { }

                    string usuario = Session["LoggedUser"].ToString();
                    //PROVEEDOR
                    string mensaje = "<strong>Buen día.</strong><br />" +
                            "<br /><label>BETA SANTA MONICA cancelo la orden de compra: </label>" +
                            "<br /><strong>Requisición: " + orden_g.C_compras_requi_g.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                            "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                            "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                            "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='100'/>";
                    notificaciones.EnviarCorreoUsuario("ORDEN DE COMPRA CANCELADA DE BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_proveedor, mensaje);

                    //USUARIO
                    string correo_solicita = orden_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.correo;
                    mensaje = "<strong>Buen día.</strong><br />" +
                            "<br /><label>Su orden de compra fue cancelada por el usuario: " + usuario + " </label>" +
                            "<br /><strong>Requisición: " + orden_g.C_compras_requi_g.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                            "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                            "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                            "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='100'/>";
                    notificaciones.EnviarCorreoUsuario("ORDEN DE COMPRA CANCELADA DE BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_solicita /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);

                    //COMPRAS
                    mensaje = "<strong>Buen día departamento de compras</strong><br />" +
                            "<br /><label>La orden de compra fue cancelada por el usuario: " + usuario + " </label>" +
                            "<br /><strong>Requisición: " + orden_g.C_compras_requi_g.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                            "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                            "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                            "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='100'/>";
                    notificaciones.EnviarCorreoUsuario("ORDEN DE COMPRA CANCELADA DE BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_comprador /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);
                }
                catch (Exception) { }

                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    UTILERIASController utileria = new UTILERIASController();
                    utileria.RegistroLogsSoporteGral(id_usuario, "Se cancelo la orden de compra: " + id_orden_g, justificacion);
                }
                catch (Exception) { }

                return 2;
            }
            catch (Exception e)
            {
                return 3;
            }
        }

        public bool EliminarSolicitudCompra(int[] id_cotizacion_confirmada_g)
        {
            try
            {
                for (int i = 0; i < id_cotizacion_confirmada_g.Length; i++)
                {
                    int id_confirmacion_g = id_cotizacion_confirmada_g[i];

                    var valid = db.C_compras_cotizaciones_confirmadas_g.Find(id_confirmacion_g);
                    if (valid.id_status_cotizacion_confirmada == 2 || valid.solicita_autorizacion == true) { return false; }

                    var id_cotizaciones_requis = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == id_confirmacion_g).Select(x => x.id_cotizacion_requisicion).ToArray();

                    db.C_compras_cotizaciones_requisiciones.Where(x => id_cotizaciones_requis.Contains(x.id_cotizacion_requisicion)).ToList().ForEach(x => x.agrupada = false);

                    db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == id_confirmacion_g).ToList().ForEach(x => x.activo = false);

                    var cotizacion_confirmada = db.C_compras_cotizaciones_confirmadas_g.Find(id_confirmacion_g);
                    cotizacion_confirmada.activo = false;
                    cotizacion_confirmada.id_status_cotizacion_confirmada = 3; //ELIMINADA

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public string MostrarOrdenCompraPDFProveedor(int id_orden)
        {
            string basePath = $"\\\\192.168.128.2\\inetpub\\PortalProvFilesSIIB";
            string[] existingFiles = Directory.GetFiles(basePath, $"OC{id_orden}.pdf");

            if (existingFiles.Length == 1)
            {
                return $"http://192.168.128.2:91/OC{id_orden}.pdf";
            }
            return "1";
        }

        public string MostrarOrdenCompraXMLProveedor(int id_orden)
        {
            string basePath = $"\\\\192.168.128.2\\inetpub\\PortalProvFilesSIIB";
            string[] existingFiles = Directory.GetFiles(basePath, $"OC{id_orden}.xml");

            if (existingFiles.Length == 1)
            {
                return $"http://192.168.128.2:91/OC{id_orden}.xml";
            }
            return "1";
        }
    }
}