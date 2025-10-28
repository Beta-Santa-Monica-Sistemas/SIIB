using Beta_System.Models;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Beta_System.Controllers
{
    public class UTILERIASController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();

        public bool RegistroLogsSoporteGral(int id_usuario_realiza, string accion_realizada, string justificacion)
        {
            try
            {
                C_soporte_logs soporte = new C_soporte_logs();
                soporte.id_usuario_realiza = id_usuario_realiza;
                soporte.fecha_registro = DateTime.Now;
                soporte.accion_realizada = accion_realizada;
                soporte.activo = true;
                soporte.justificacion = justificacion;
                db.C_soporte_logs.Add(soporte);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region UTILERIA REQUISICIONES

        public ActionResult UtileriasRequisiciones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7033)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("REQUISICIONES/EditarRequisiciones/Index");
        }

        public PartialViewResult BuscarInformacionRequisicionUtileria(string folio_requisicion)
        {
            C_compras_requi_g requisicion = new C_compras_requi_g();
            try
            {
                int id_requi = Convert.ToInt32(folio_requisicion);
                requisicion = db.C_compras_requi_g.Where(x => x.id_requisicion_articulo_g == id_requi).FirstOrDefault();
            }
            catch (Exception)
            {
                requisicion = db.C_compras_requi_g.Where(x => x.folio == folio_requisicion).FirstOrDefault();
            }
            if (requisicion != null) { return PartialView("REQUISICIONES/EditarRequisiciones/_InformacionRequisicionEditar", requisicion); }

            return null;
            //return PartialView("REQUISICIONES/EditarRequisiciones/_InformacionRequisicionEditar", null);
        }

        public PartialViewResult AgregarArticuloDetalleUtileria(int id_requi_d, int count_tabla, int id_articulo, string articulo, string cantidad, string precio, string unidad_medida, int id_unidad_medida,
            string observacion, int id_centro_g, string cargo_contable, int id_cargo_contable, int id_cuenta_contable_g, bool new_cotizacion, int id_tipo_requi, string cve_prov, int id_cotizacion, int id_tipo_moneda, string clave_articulo)
        {
            int id_proveedor = 0;
            string nombre_proveedor = "";
            if (id_tipo_requi == 2)
            {
                if (id_cotizacion != 0)
                {
                    var cotizacion = db.C_compras_cotizaciones_articulos.Find(id_cotizacion);
                    precio = cotizacion.precio_unitario.ToString();
                    nombre_proveedor = cotizacion.C_compras_proveedores.razon_social;
                    id_proveedor = (int)cotizacion.id_compras_proveedor;
                    id_tipo_moneda = (int)cotizacion.id_tipo_moneda;
                }
                else
                {
                    if (new_cotizacion == true)
                    {
                        var proveedor = db.C_compras_proveedores.Where(x => x.cve == cve_prov).FirstOrDefault();
                        nombre_proveedor = proveedor.razon_social;
                        id_proveedor = (int)proveedor.id_compras_proveedor;
                    }
                }
            }

            ViewBag.id_requi_d = id_requi_d;
            ViewBag.count_tabla = count_tabla;
            ViewBag.id_articulo = id_articulo;
            ViewBag.articulo = articulo;
            ViewBag.clave_articulo = clave_articulo;
            ViewBag.cantidad = cantidad;
            ViewBag.precio = precio;
            ViewBag.medida = unidad_medida;
            ViewBag.id_unidad_medida = id_unidad_medida;
            ViewBag.observacion = observacion;
            ViewBag.id_centro_g = id_centro_g;
            ViewBag.cargo_contable = cargo_contable;
            ViewBag.id_cargo_contable = id_cargo_contable;
            ViewBag.id_cuenta_contable_g = id_cuenta_contable_g;

            ViewBag.id_tipo_requi = id_tipo_requi;
            ViewBag.nombre_proveedor = nombre_proveedor;
            ViewBag.id_cotizacion = id_cotizacion;
            ViewBag.id_proveedor = id_proveedor;
            ViewBag.id_tipo_moneda = id_tipo_moneda;

            return PartialView("REQUISICIONES/EditarRequisiciones/_RequisicionDetalleEditar");
        }

        public int CancelarRequisicionUtilerias(int id_requisicion_g, string justificacion)
        {
            //-1 La requisicion tiene orden de compra
            //0 Ocurrio un error
            //1 Se cancelo la requisicion correctamente
            try
            {
                var requisicion = db.C_compras_requi_g.Find(id_requisicion_g);
                //ORDEN DE COMPRA
                if (requisicion.C_compras_ordenes_g.Where(x => x.activo == true).Any())
                {
                    return -1;
                }
                //INVERSION
                if (requisicion.id_requisicion_tipo == 3)
                {
                    var inversion = db.C_compras_inversiones_programadas.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true).FirstOrDefault();
                    if (inversion != null)
                    {
                        inversion.aplicado = false;
                        inversion.fecha_aplicacion = null;
                        inversion.id_requisicion_articulo_g = null;
                        inversion.id_usuario_aplica = null;
                        db.SaveChanges();
                    }
                }
                requisicion.id_status_requi = 2008;
                requisicion.activo = false;
                db.SaveChanges();

                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se desactivo la requisicion: " + id_requisicion_g, justificacion);
                }
                catch (Exception) { }

                return 1;
            }
            catch (Exception ex)
            {
                string log = ex.ToString();
                return 0;
            }
        }

        public int ActualizarArticuloRequisicion(int id_requi_d, decimal cantidad, int id_cargo, int id_cuenta, string obs, string justificacion)
        {
            try
            {
                var valid_entregado = db.C_compras_requi_d.Find(id_requi_d);
                if (valid_entregado.cantidad == valid_entregado.cantidad_cotizada) { return -1; }  //EL ARTICULO YA FUE COTIZADO EN SU TOTALIDAD
                if (cantidad < valid_entregado.cantidad_cotizada) { return -2; }  //LA CANTIDAD NO PUEDE SER MENOR A LA COTIZADA

                valid_entregado.cantidad = cantidad;
                valid_entregado.id_cargo_contable = id_cargo;
                valid_entregado.id_cuenta_contable_g = id_cuenta;
                valid_entregado.observacion = obs;

                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    UTILERIASController utileria = new UTILERIASController();
                    utileria.RegistroLogsSoporteGral(id_usuario, "Se modifico el articulo: " + valid_entregado.id_requisicion_articulo_d + " de la requisicion: " + valid_entregado.id_requisicion_articulo_g, justificacion);
                }
                catch (Exception) { }

                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public int ActualizarHeaderRequisicion(int id_requi_g, string concepto, int id_prioridad, string justificacion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var requisicion = db.C_compras_requi_g.Find(id_requi_g);
                if (requisicion == null) { return -2; } //NO ENCONTRADA

                string justificacionInterna = "Se modificó el concepto: '"+ requisicion.concepto +"' A -> '" + concepto + "'.";

                requisicion.concepto = concepto;
                requisicion.id_prioridad = id_prioridad;
                db.SaveChanges();

                C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                log.id_requisicion = id_requi_g;
                log.id_requisicion_articulo_g = id_requi_g;
                log.id_tipo_log = 6;  //MODIFICACION DE ORDEN
                log.fecha_registro = DateTime.Now;
                log.activo = true;
                log.id_usuario_registra = id_usuario;
                log.justificacion = justificacionInterna;
                db.C_compras_requisiciones_logs.Add(log);
                db.SaveChanges();

                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se modifico el header de la requisicion: " + id_requi_g, justificacion);
                }
                catch (Exception) { }

                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public PartialViewResult EditarOrdenCompraUtileria(int id_orden_g)
        {
            var detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g).ToList();
            return PartialView("REQUISICIONES/EditarRequisiciones/_OrdenCompraDetalleEditar", detalle);
        }

        public int ActualizarOrdenCompraUtileria(int id_orden_g, int id_proveedor, int id_ubicacion_entrega, int[] id_articulos, decimal[] cantidades, decimal[] precios, string justificacion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                if (orden.id_status_orden != 1) { return -1; }  //LA ORDEN YA FUE IMPORTADA O CANCELADA (NO GENERADA)
                if (orden.alta_proveedor != false) { return -2; } //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
                if (orden.activo == false) { return -3; } //LA ORDEN DE COMPRA ESTA INACTIVA

                int id_moneda = 0;
                bool cambio_proveedor = false;
                bool cambio_cantidad_precio = false;
                if (orden.id_proveedor != id_proveedor) { 
                    cambio_proveedor = true;
                    id_moneda = (int)db.C_compras_proveedores.Find(id_proveedor).id_tipo_moneda;
                }

                orden.id_proveedor = id_proveedor;
                if (orden.id_tipo_orden == 2) { orden.id_compras_ubicacion_entrega = 9; }  //SERVICIO
                else { orden.id_compras_ubicacion_entrega = id_ubicacion_entrega; } 
                db.SaveChanges();

                bool permiso_cantidades = false;
                if (db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == 2017 && x.id_usuario_corporativo == id_usuario && x.activo == true).FirstOrDefault() != null) { permiso_cantidades = true; }

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    int id_orden_d = id_articulos[i];
                    var detalle = db.C_compras_ordenes_d.Find(id_orden_d);
                    if (detalle.cantidad_compra != cantidades[i] || detalle.precio_unitario != precios[i]) { cambio_cantidad_precio = true; }

                    detalle.precio_unitario = precios[i];
                    if (cantidades[i] > detalle.cantidad_compra && permiso_cantidades == false) { //LA CANTIDAD NUEVA NO PUEDE SER MAYOR A LA DE LA ORDEN DE COMPRA
                        return -5; 
                    }  

                    if (cambio_proveedor == true && id_moneda != 0) { detalle.id_tipo_moneda = id_moneda; }

                    if (cantidades[i] != detalle.cantidad_compra)
                    {
                        // COTIZACIÓN NUEVA EN AUTOMATICO PARA MODIFICACIÓN DE PRECIOS (VM 06-24-2025)
                        try
                        {
                            int id_articulo = (int)detalle.id_articulo;
                            int id_requi_g = (int)detalle.C_compras_ordenes_g.id_requisicion_articulo_g;
                            var cotizacion = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_articulo == id_articulo && 
                            x.id_requisicion_articulo_g == id_requi_g && x.orden_generada == true && x.activo == true).FirstOrDefault();
                            if (cotizacion != null)
                            {
                                C_compras_cotizaciones_articulos new_coti = new C_compras_cotizaciones_articulos();
                                new_coti.id_articulo = id_articulo;
                                new_coti.id_compras_proveedor = cotizacion.C_compras_cotizaciones_articulos.id_compras_proveedor;
                                new_coti.precio_unitario = precios[i];
                                new_coti.porcentaje_descuento = 0;
                                new_coti.precio_final = precios[i];
                                new_coti.id_unidad_medida = cotizacion.C_compras_cotizaciones_articulos.id_unidad_medida;
                                new_coti.marca = cotizacion.C_compras_cotizaciones_articulos.marca;
                                new_coti.presentacion = cotizacion.C_compras_cotizaciones_articulos.presentacion;
                                new_coti.fecha_cotizacion = cotizacion.C_compras_cotizaciones_articulos.fecha_cotizacion;
                                new_coti.fecha_vigencia = cotizacion.C_compras_cotizaciones_articulos.fecha_vigencia;
                                new_coti.id_usuario_cotiza = cotizacion.C_compras_cotizaciones_articulos.id_usuario_cotiza;
                                new_coti.id_tipo_moneda = cotizacion.C_compras_cotizaciones_articulos.id_tipo_moneda;
                                new_coti.activo = cotizacion.C_compras_cotizaciones_articulos.activo;
                                new_coti.fecha_actualizacion = cotizacion.C_compras_cotizaciones_articulos.fecha_actualizacion;
                                new_coti.dias_entrega = cotizacion.C_compras_cotizaciones_articulos.dias_entrega;
                                db.C_compras_cotizaciones_articulos.Add(new_coti);
                                db.SaveChanges();
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }

                    detalle.cantidad_compra = cantidades[i];
                    //VALIDAR ENTREGADO COMPLETO
                    if (detalle.cantidad_entregada > detalle.cantidad_compra)
                    {
                        return -4;   //LA CANTIDAD NO PUEDE SER MAYOR A LA CANTIDAD RECIBIDA
                    }
                    if (detalle.cantidad_compra == detalle.cantidad_entregada)
                    {
                        detalle.entregado = true;
                    }
                    if (detalle.cantidad_entregada < detalle.cantidad_compra)
                    {
                        detalle.entregado = false;
                        detalle.C_compras_ordenes_g.entregado = false;
                        //notificaciones.EnviarCorreoUsuario("LOG UTIELRIAS","vmascorro@beta.com.mx", "MODIFICACION DE CANTIDADES HACIA ARRIBA EN LA ORDEN: "+ detalle.id_compras_orden_g + ", ID_D = "+ detalle.id_compras_orden_d +"");
                    }
                    db.SaveChanges();
                }

                var articulos_orden = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true).Select(x => x.entregado).ToArray();
                if (!articulos_orden.Contains(false)) { orden.entregado = true; db.SaveChanges(); }

                if (cambio_cantidad_precio == true && cambio_proveedor == false)
                {
                    ReenviarCorreoOrdenModificada(id_orden_g);
                }

                try
                {
                    //string justificacion = "Se modificó la orden de compra: #" + id_orden_g + " por el usuario: " + Session["LoggedUser"].ToString() + ".";
                    //if (cambio_proveedor == true)
                    //{
                    //    justificacion += " Se cambió el proveedor a: " + db.C_compras_proveedores.Find(id_proveedor).razon_social + ".";
                    //}
                    //if (cambio_cantidad_precio == true)
                    //{
                    //    justificacion += " Se modificaron las cantidades y precios de los artículos.";
                    //}

                    C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                    log.id_requisicion = orden.id_requisicion_articulo_g;
                    log.id_requisicion_articulo_g = orden.id_requisicion_articulo_g;
                    log.id_tipo_log = 6;  //MODIFICACION DE ORDEN
                    log.fecha_registro = DateTime.Now;
                    log.activo = true;
                    log.id_usuario_registra = id_usuario;
                    log.justificacion = justificacion;
                    db.C_compras_requisiciones_logs.Add(log);
                    db.SaveChanges();
                }
                catch (Exception)
                {

                }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public int ActualizarCargoCuentaArticuloOrdenUtileria(int id_orden_d, int id_cargo, int id_cuenta, string justificacion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var partida = db.C_compras_ordenes_d.Find(id_orden_d);
                if (partida == null) { return -2; } //PARTIDA NO ENCONTRADA
                if (partida.C_compras_ordenes_g.alta_proveedor == true) { return -3; }  //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
                partida.id_cargo_contable = id_cargo;
                partida.id_cuenta_contable = id_cuenta;
                db.SaveChanges();

                C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                log.id_requisicion = partida.C_compras_ordenes_g.id_requisicion_articulo_g;
                log.id_requisicion_articulo_g = partida.C_compras_ordenes_g.id_requisicion_articulo_g;
                log.id_usuario_registra = id_usuario;
                log.id_tipo_log = 7;  //MODIFICACION DE CARGO Y CUENTA
                log.fecha_registro = DateTime.Now;
                log.activo = true;
                if (justificacion == null){ log.justificacion = "NO CAPTURADA (SYS)"; }
                else { log.justificacion = justificacion; }
                db.C_compras_requisiciones_logs.Add(log);
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public int OnOffArticuloOrdenCompraUtileria(int id_orden_g, int id_orden_d, bool modo)
        {
            try
            {
                var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                if (orden.id_status_orden != 1) { return -1; }  //LA ORDEN YA FUE IMPORTADA O CANCELADA (NO GENERADA)
                if (orden.alta_proveedor == true) { return -2; } //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
                if (orden.activo == false) { return -3; } //LA ORDEN DE COMPRA ESTA INACTIVA

                var detalle = db.C_compras_ordenes_d.Find(id_orden_d);
                if (modo == false && detalle.cantidad_entregada > 0) { return -4; }  //EL ARTICULO YA FUE ENTREGADO

                detalle.activo = modo;
                db.SaveChanges();

                var articulos_orden = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true).Select(x => x.entregado).ToArray();
                if (!articulos_orden.Contains(false)) { orden.entregado = true; db.SaveChanges(); }

                ReenviarCorreoOrdenModificada(id_orden_g);

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public bool ReenviarCorreoOrdenModificada(int id_orden_g)
        {
            try
            {
                string usuario = Session["LoggedUser"].ToString();

                var orden_g = db.C_compras_ordenes_g.Find(id_orden_g);
                var requi_info = orden_g.C_compras_requi_g;
                var confirmada = orden_g.C_compras_cotizaciones_confirmadas_g;
                int id_proveedor = (int)orden_g.id_proveedor;

                string nombre_comprador = "";
                string correo_comprador = "";
                string correo_proveedor = "";
                string razon_social = "";
                try
                {
                    nombre_comprador = confirmada.C_usuarios_corporativo.C_empleados.nombres + " " + confirmada.C_usuarios_corporativo.C_empleados.apellido_paterno;
                    correo_comprador = confirmada.C_usuarios_corporativo.C_empleados.correo;
                    correo_proveedor = confirmada.correo_proveedor_suborden;
                    razon_social = db.C_compras_proveedores.Find(id_proveedor).razon_social;
                }
                catch (Exception) { }

                NOTIFICACIONESController notificacion = new NOTIFICACIONESController();
                //PROVEEDOR
                string mensaje = "<strong>Buen día.</strong><br />" +
                        "<br /><label>BETA SANTA MONICA modificó su orden de compra: </label><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                        "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                        "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                notificacion.EnviarCorreoUsuario("MODIFICACIÓN ORDEN DE COMPRA DE BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_proveedor /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);

                ///QUIEN SOLICITA
                string correo_solicita = requi_info.C_usuarios_corporativo5.C_empleados.correo;
                mensaje = "<strong>Buen día.</strong><br />" +
                        "<br /><label>Su orden de compra fué modificada por el usuario: " + usuario + " </label>" +
                        "<br /><strong>Requisición: " + requi_info.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                        "<br /><strong>Orden de compra enviada al proveedor: " + razon_social + " " +
                        "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                        "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                notificacion.EnviarCorreoUsuario("MODIFICACIÓN ORDEN DE COMPRA: #" + orden_g.id_compras_orden_g + "", correo_solicita /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);

                //------ITEREAR EL SPLIT DE ";" EN EL CAMPO "confirmada.correo_proveedor_suborden"
                //PERSONAL DE COMPRAS
                mensaje = "<strong>Buen día departamento de compras</strong><br />" +
                        "<br /><label>La orden de compra fue modificada por el usuario: " + usuario + " </label>" +
                        "<br /><strong>Requisición: " + requi_info.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                        "<br /><strong>Orden de compra enviada al proveedor: " + orden_g.C_compras_proveedores.razon_social + " al correo: " + confirmada.correo_proveedor_suborden + "</strong><br />" +
                        "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                        "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                notificacion.EnviarCorreoUsuario("MODIFICACIÓN ORDEN DE COMPRA: #" + orden_g.id_compras_orden_g + "", correo_comprador /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);

                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool CancelarRecepcionOrdenCompraUtileria(int id_orden_g)
        {
            try
            {
                var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                if (orden.alta_proveedor == true) { return false; }
                orden.entregado = false;
                db.SaveChanges();
                db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g).ForEach(x => { x.entregado = false; x.cantidad_entregada = 0; });
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int EliminarRecepcionAlmacenOrdenCompra(int id_orden_g, int id_mov_g)
        {
            try
            {
                var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                if (orden.alta_proveedor == true) { return -1; } //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
                if (orden.activo == false) { return -2; } //LA ORDEN DE COMPRA ESTA INACTIVA


                orden.entregado = false;
                var movs = db.C_inventario_almacen_mov_g.Find(id_mov_g);
                movs.id_inventario_mov_status = 3; //CANCELADO
                db.SaveChanges();

                foreach (var item in movs.C_inventario_almacen_mov_d)
                {
                    int id_articulo = (int)item.id_articulo;

                    item.id_inventario_mov_status = 3; //CANCELADO
                    var detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true && x.id_articulo == id_articulo).FirstOrDefault();
                    if (detalle != null)
                    {
                        detalle.cantidad_entregada = detalle.cantidad_entregada - item.cantidad;
                        detalle.entregado = false;
                    }
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public PartialViewResult ConsultarOrdenDividir(int id_orden_g)
        {
            try
            {
                var data = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_g == id_orden_g);
                return PartialView("REQUISICIONES/EditarRequisiciones/_DivisionOrden", data);
            }
            catch (Exception)
            {
                return PartialView("REQUISICIONES/EditarRequisiciones/_DivisionOrden", null);
            }
        }

        public int GenerarNuevaOrdenDividir(int id_orden_g, int[] id_orden_d_original, decimal[] cantidades_original, int[] id_orden_d_new, decimal[] cantidades_new)
        {
            try
            {
                var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                if (orden.id_status_orden != 1) { return -2; }  //LA ORDEN YA FUE IMPORTADA O CANCELADA (NO GENERADA)
                if (orden.alta_proveedor == true) { return -3; } //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
                if (orden.activo == false) { return -4; } //LA ORDEN DE COMPRA ESTA INACTIVA
                if (id_orden_d_new.Length != cantidades_new.Length) { return -5; } // //LA CANTIDAD DE ARTICULOS NUEVOS NO COINCIDE CON LOS ARTICULOS NUEVOS

                C_compras_ordenes_g nueva_orden = new C_compras_ordenes_g();
                nueva_orden.id_requisicion_articulo_g = orden.id_requisicion_articulo_g;
                nueva_orden.id_proveedor = orden.id_proveedor;
                nueva_orden.id_cotizacion_confirmada_g = orden.id_cotizacion_confirmada_g;
                nueva_orden.fecha_registro = DateTime.Now;
                nueva_orden.id_usuario_genera = orden.id_usuario_genera;
                nueva_orden.id_centro = orden.id_centro;
                nueva_orden.id_tipo_orden = orden.id_tipo_orden;
                nueva_orden.id_status_orden = 1;
                nueva_orden.entregado = false;
                nueva_orden.activo = true;
                nueva_orden.token_orden = notificaciones.GenerarTokenLongitud(25);
                while (db.C_compras_ordenes_g.Where(x => x.token_orden == nueva_orden.token_orden).FirstOrDefault() != null)
                {
                    nueva_orden.token_orden = notificaciones.GenerarTokenLongitud(25);
                }
                nueva_orden.id_compras_ubicacion_entrega = orden.id_compras_ubicacion_entrega;
                nueva_orden.alta_proveedor = false;
                nueva_orden.id_usuario_recepciona = null;
                nueva_orden.fecha_recepcion = null;
                db.C_compras_ordenes_g.Add(nueva_orden);
                db.SaveChanges();
                int id_orden_g_new = nueva_orden.id_compras_orden_g;

                db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g).ForEach(x => { x.activo = false; });
                db.SaveChanges();

                for (int i = 0; i < id_orden_d_original.Length; i++)
                {
                    int id_orden_d = id_orden_d_original[i];
                    var detalle_original = db.C_compras_ordenes_d.Find(id_orden_d);
                    if (detalle_original == null) { return -4; } //DETALLE DE ORDEN NO ENCONTRADO
                    detalle_original.cantidad_compra = cantidades_original[i];
                    detalle_original.activo = true;
                    db.SaveChanges();
                }

                for (int i = 0; i < id_orden_d_new.Length; i++)
                {
                    int id_orden_d = id_orden_d_new[i];
                    var detalle_original = db.C_compras_ordenes_d.Find(id_orden_d);
                    C_compras_ordenes_d nuevo_detalle = new C_compras_ordenes_d();
                    nuevo_detalle.id_compras_orden_g = id_orden_g_new;
                    nuevo_detalle.id_articulo = detalle_original.id_articulo;
                    nuevo_detalle.cantidad_compra = cantidades_new[i];
                    nuevo_detalle.precio_unitario = detalle_original.precio_unitario;
                    nuevo_detalle.id_tipo_moneda = detalle_original.id_tipo_moneda;
                    nuevo_detalle.id_cargo_contable = detalle_original.id_cargo_contable;
                    nuevo_detalle.id_cuenta_contable = detalle_original.id_cuenta_contable;
                    nuevo_detalle.nombre_articulo = detalle_original.nombre_articulo;
                    nuevo_detalle.id_unidad_medida = detalle_original.id_unidad_medida;
                    nuevo_detalle.cantidad_entregada = 0;
                    nuevo_detalle.entregado = false;
                    nuevo_detalle.cantidad_precarga = 0;
                    nuevo_detalle.activo = true;
                    db.C_compras_ordenes_d.Add(nuevo_detalle);
                    db.SaveChanges();
                }

                return nueva_orden.id_compras_orden_g;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1; //ERROR AL GENERAR LA NUEVA ORDEN
            }


        }


        public PartialViewResult ConsultarOrdenMoverArticulos(int id_requisicion_g)
        {
            //var data = db.C_compras_ordenes_d.Where(x => x.activo == true && x.id_compras_orden_g == id_orden_g);
            var data = db.C_compras_ordenes_d.Where(x => x.C_compras_ordenes_g.id_requisicion_articulo_g == id_requisicion_g && x.activo == true 
                        && x.C_compras_ordenes_g.activo == true && x.C_compras_ordenes_g.entregado == false);
            
            return PartialView("REQUISICIONES/EditarRequisiciones/_MoverArticulosOrden", data);
        }

        public PartialViewResult ConsultarArticulosOrdenChecksMover(int id_orden_g, int modo)
        {
            ViewBag.modo = modo;
            var ordenes = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true);
            return PartialView("REQUISICIONES/EditarRequisiciones/_MoverArticuloOrdenChecks", ordenes);
        }

        public int MoverArticulosOrden(int id_orden_afectar, int[] id_ordenes_d, int id_orden_base)
        {
            try
            {
                for (int i = 0; i < id_ordenes_d.Length; i++)
                {
                    var detalle = db.C_compras_ordenes_d.Find(id_ordenes_d[i]);
                    if (detalle != null) {
                        if (detalle.id_compras_orden_g != id_orden_afectar)
                        {
                            detalle.id_compras_orden_g = id_orden_afectar;
                            db.SaveChanges();
                        }
                    }
                }

                var valid_detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_base && x.activo == true);
                if (valid_detalle.Count() == 0)
                {
                    var orden_base = db.C_compras_ordenes_g.Find(id_orden_base);
                    if (orden_base != null)
                    {
                        orden_base.activo = false; //DESACTIVAR LA ORDEN BASE SI NO TIENE DETALLES
                        orden_base.id_status_orden = 3; //ESTADO CANCELADO
                        orden_base.id_requisicion_articulo_g = null; //QUITAR LA REQUISICION ASIGNADA
                        db.SaveChanges();
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public int CambiarCambiarArticuloOrdenCompra(int id_orden_d, int id_articulo_original, int id_articulo_nuevo, string justificacion)
        {
            int id_usuario = (int)Session["LoggedId"];
            var orden_detalle = db.C_compras_ordenes_d.Find(id_orden_d);
            if (orden_detalle == null) { return -1; } //DETALLE DE ORDEN NO ENCONTRADO
            if (orden_detalle.id_articulo == id_articulo_nuevo) { return -2; } //EL ARTICULO YA ES EL MISMO
            if (orden_detalle.C_compras_ordenes_g.alta_proveedor == true) { return -3; } //EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL
            if (orden_detalle.entregado == true) { return -4; } //EL ARTICULO YA FUE ENTREGADO
            if (orden_detalle.C_compras_ordenes_g.id_status_orden != 1) { return -5; } //LA ORDEN YA FUE IMPORTADA O CANCELADA (NO GENERADA)
            if (orden_detalle.C_compras_ordenes_g.activo == false) { return -6; } //LA ORDEN DE COMPRA ESTA INACTIVA
            if (orden_detalle.cantidad_entregada > 0) { return -7; } //SE ENTREGÓ PARCIALMENTE EL ARTICULO

            try
            {
                var articulo_nuevo = db.C_articulos_catalogo.Find(id_articulo_nuevo);
                if (articulo_nuevo == null) { return -8; } //ARTICULO NUEVO NO ENCONTRADO
                orden_detalle.id_articulo = id_articulo_nuevo;
                orden_detalle.nombre_articulo = articulo_nuevo.nombre_articulo;
                orden_detalle.id_unidad_medida = articulo_nuevo.id_unidad_medida;
                //db.SaveChanges();

                C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                log.id_requisicion = orden_detalle.C_compras_ordenes_g.id_requisicion_articulo_g;
                log.id_requisicion_articulo_g = log.id_requisicion;
                log.id_usuario_registra = id_usuario;
                log.id_tipo_log = 1007;  //CAMBIO DE ARTÍCULO EN LA ORDEN DE COMPRA
                log.fecha_registro = DateTime.Now;
                log.activo = true;
                if (justificacion == null) { log.justificacion = "NO CAPTURADA (SYS)"; }
                else { log.justificacion = justificacion; }
                db.C_compras_requisiciones_logs.Add(log);
                db.SaveChanges();

                try
                {
                    var requisicion_d = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == orden_detalle.C_compras_ordenes_g.id_requisicion_articulo_g
                                    && x.id_articulo == id_articulo_original).FirstOrDefault();
                    if (requisicion_d != null)
                    {
                        requisicion_d.id_articulo = id_articulo_nuevo;
                        requisicion_d.nombre_art = articulo_nuevo.nombre_articulo;
                        requisicion_d.id_unidad_medida = articulo_nuevo.id_unidad_medida;
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {

                }

                //ELIMINAR COTIZACIONES ANTERIORES DEL ARTICULO ORIGINAL
                //var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_articulo == id_articulo_original && x.orden_generada == true && x.activo == true).ToList();
                //foreach (var item in cotizaciones)
                //{
                //    item.id_articulo = id_articulo_nuevo;
                //    db.SaveChanges();
                //}

                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se cambio el articulo: " + id_articulo_original + " por el articulo: " + id_articulo_nuevo, justificacion);
                }
                catch (Exception) { }

                return 0;
            }
            catch (Exception)
            {
                return 1; //ERROR AL CAMBIAR EL ARTICULO
            }

        }

        #endregion


        public bool CalcularIntegracionGeneral()
        {
            bool validacion = true;

            try
            {
                var empleados = db.C_nomina_empleados.ToList();
                DateTime fecha_actual = DateTime.Now;

                foreach (var item in empleados)
                {
                    try
                    {
                        int años_diferencia = 0;
                        var reingreso = db.C_nomina_incidencias
                                          .FirstOrDefault(x => x.Empleado_id == item.Empleado_id && x.id_tipo_incidencia == 3);

                        DateTime? fecha_base = reingreso?.Fecha ?? item.Fecha_ingreso;

                        if (fecha_base.HasValue)
                        {
                            años_diferencia = fecha_actual.Year - fecha_base.Value.Year;

                            if (fecha_actual < fecha_base.Value.AddYears(años_diferencia))
                            {
                                años_diferencia--;
                            }
                        }

                        if (años_diferencia == 0)
                        {
                            años_diferencia = 1;
                        }
                        if (años_diferencia > 35)
                        {
                            años_diferencia = 34;
                        }

                        var porcentaje = db.C_nomina_parametros_porcentajes_integracion
                                           .Where(x => x.activo == true && años_diferencia <= x.tipo_maximo && años_diferencia >= x.tiempo_minimo)
                                           .OrderByDescending(x => x.id_nomina_parametro_porcentaje_integracion)
                                           .FirstOrDefault();

                        if (porcentaje != null)
                        {
                            item.Pctje_integ = porcentaje.porcentaje_integracion;
                        }
                        else
                        {
                             Console.WriteLine($"No se encontró porcentaje de integración para el empleado {item.Empleado_id} con {años_diferencia} años de diferencia.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando empleado {item.Empleado_id}: {ex.Message}");
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                //validacion = false;
            }

            return validacion;
        }




        #region UTILERIA INVENTARIOS

        public ActionResult UtileriasInventarioAlmacen()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8057)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("ALMACEN/Index");
        }

        public PartialViewResult ArticulosUtileriaTable(string clave, string articulo)
        {
            if (clave == "" && articulo != "")
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.nombre_articulo.Contains(articulo)).OrderBy(x => x.nombre_articulo);
                return PartialView("../UTILERIAS/ALMACEN/_ArticulosTable", articulos_cfiltro);
            }
            else if (articulo == "" && clave != "")
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.clave.Contains(clave)).OrderBy(x => x.nombre_articulo);
                return PartialView("../UTILERIAS/ALMACEN/_ArticulosTable", articulos_cfiltro);
            }
            else if (clave != "")
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.clave.Contains(clave)).OrderBy(x => x.nombre_articulo);
                return PartialView("../UTILERIAS/ALMACEN/_ArticulosTable", articulos_cfiltro);
            }
            else
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.nombre_articulo.Contains(articulo) || x.clave.Contains(clave)).OrderBy(x => x.nombre_articulo);
                return PartialView("../UTILERIAS/ALMACEN/_ArticulosTable", articulos_cfiltro);
            }
        }

        public PartialViewResult ExistenciaAlmacenReciente(int articulo, int id_almacen_g, string fecha)
        {
            try
            {
                var articulo_completo = db.C_articulos_catalogo.Where(x => x.id_articulo == articulo && x.activo == true).FirstOrDefault();
                DateTime fecha_consulta = DateTime.Parse(fecha).AddHours(23).AddMinutes(59);

                bool InvCapturaInventario = false;
                var id_tipo = articulo_completo.id_articulo_tipo;
                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen_g && x.activo == true && x.id_tipo_articulos_inventario == id_tipo).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();
                if (valid_inventario_captura != null) { InvCapturaInventario = true; }

                decimal entradas = 0;
                decimal salidas = 0;
                decimal mermas = 0;
                decimal entradas_trapaso = 0;
                decimal salidas_traspaso = 0;

                decimal salidas_ajuste = 0;
                decimal entradas_ajuste = 0;
                decimal entradas_devolucion = 0;

                decimal CantidadCapturada = 0;

                if (InvCapturaInventario == true) // EXISTE UNA CAPTURA DE INVENTARIO
                {
                    var entradas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();   //ENTRADAS POR COMPRA
                    if (entradas_registros.Count() > 0) { entradas = entradas_registros.Sum(x => x.cantidad).Value; }

                    var salidas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();  //ENTREGAS DE VALES
                    if (salidas_registros.Count() > 0) { salidas = salidas_registros.Sum(x => x.cantidad).Value; }

                    var mermas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();  //MERMAS
                    if (mermas_registros.Count() > 0) { mermas = mermas_registros.Sum(x => x.cantidad).Value; }

                    var entradas_trap = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.id_articulo == articulo).ToList();
                    if (entradas_trap.Count() > 0) { entradas_trapaso = entradas_trap.Sum(x => x.cantidad).Value; }

                    var salidas_trap = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.id_articulo == articulo).ToList();
                    if (salidas_trap.Count() > 0) { salidas_traspaso = salidas_trap.Sum(x => x.cantidad).Value; }


                    //-------AJUSTES 
                    var entradas_ajust = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1003 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                    if (entradas_ajust.Count() > 0) { entradas_ajuste = entradas_ajust.Sum(x => x.cantidad).Value; }

                    var salidas_ajust = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1004 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                    if (salidas_ajust.Count() > 0) { salidas_ajuste = salidas_ajust.Sum(x => x.cantidad).Value; }

                    //----- DEVOLUCIONES
                    var entradas_devoluciones = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1005 && x.id_articulo == articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                    if (entradas_devoluciones.Count() > 0) { entradas_devolucion = entradas_devoluciones.Sum(x => x.cantidad).Value; }

                }

                //OBEJTO QUE ENGLOBA LA CPTURA DEL INVENTARIO CON ENTRADAS Y SALIDAS
                List<CapturaInventarioDiferenciasUtileria> captura = new List<CapturaInventarioDiferenciasUtileria>();
                var articulos_almacen = db.C_articulos_catalogo.Where(x => x.almacenable == true && x.id_articulo == articulo && x.activo == true).ToList();
                if (InvCapturaInventario == true)  //EXISTE UNA CAPTURA DE INVENTARIO
                {
                    ViewBag.fecha_corta = String.Format(valid_inventario_captura.fecha_registro.Value.ToShortDateString(), "dd/MM/yyyy");
                    ViewBag.fecha_inventario = String.Format(valid_inventario_captura.fecha_registro.ToString(), "dd/MM/yyyy");
                    ViewBag.inv_inicial = false;
                    foreach (var item in articulos_almacen)
                    {
                        CapturaInventarioDiferenciasUtileria registro = new CapturaInventarioDiferenciasUtileria();
                        int id_articulo = (int)item.id_articulo;
                        int captura_d = 0;
                        try
                        {
                            captura_d = db.C_inventario_almacen_captura_d.Where(x => x.id_inventario_g == valid_inventario_captura.id_inventario_g && x.id_articulo == id_articulo).FirstOrDefault().id_inventario_d;
                            registro.Id_inventario_captura = captura_d;
                            registro.Existe_registro = true;
                        }
                        catch (Exception)
                        {
                            registro.Id_inventario_captura = 0;
                            registro.Existe_registro = false;
                        }
                        registro.Id_tipo_inventario = valid_inventario_captura.id_inventario_g;
                        registro.Clave = item.clave;
                        registro.Id_articulo = id_articulo;
                        registro.Nombre_articulo = item.nombre_articulo;
                        registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                        registro.Entradas = entradas; //(decimal)entradas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salidas = salidas; //(decimal)salidas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        registro.Mermas = mermas; //(decimal)mermas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Entradas_trasp = entradas_trapaso; //(decimal)entr_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salidas_trasp = salidas_traspaso; //(decimal)salid_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        registro.Entrada_ajuste = entradas_ajuste; //(decimal)entradas_ajuste.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salida_ajuste = salidas_ajuste; //(decimal)salidas_ajuste.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Entrada_devolucion = entradas_devolucion; //(decimal)entradas_devoluciones.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        //decimal CantidadCapturada = 0;
                        var articulo_capturado = valid_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo).FirstOrDefault();
                        if (articulo_capturado != null) { CantidadCapturada = (decimal)articulo_capturado.cantidad_captura; }
                        registro.Ultima_Cap = CantidadCapturada;

                        registro.Cantidad_sistema = (CantidadCapturada + registro.Entradas + registro.Entradas_trasp + registro.Entrada_ajuste + registro.Entrada_devolucion) - registro.Salidas - registro.Mermas - registro.Salidas_trasp - registro.Salida_ajuste;

                        try
                        {
                            var valid_ubicacion = item.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen_g && x.activo == true).FirstOrDefault();
                            if (valid_ubicacion != null)
                            {
                                registro.Ubicacion = valid_ubicacion.C_almacen_ubicaciones_articulos.nombre_ubicacion;
                            }
                            else
                            {
                                registro.Ubicacion = "NO ASIGNADA";
                            }
                        }
                        catch (Exception)
                        {
                            registro.Ubicacion = "NO ASIGNADA";
                        }

                        captura.Add(registro);
                    }
                }
                else
                {
                    ViewBag.fecha_corta = "";
                    ViewBag.fecha_inventario = "Sin historial de captura";
                    ViewBag.inv_inicial = true;
                    foreach (var item in articulos_almacen)
                    {
                        int id_articulo = (int)item.id_articulo;

                        CapturaInventarioDiferenciasUtileria registro = new CapturaInventarioDiferenciasUtileria();

                        registro.Id_inventario_captura = 0;
                        registro.Clave = item.clave;
                        registro.Existe_registro = false;
                        registro.Id_articulo = id_articulo;
                        registro.Nombre_articulo = item.nombre_articulo;
                        registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                        registro.Entradas = 0;
                        registro.Salidas = 0;
                        registro.Mermas = 0;
                        registro.Ultima_Cap = 0;
                        registro.Cantidad_sistema = 0;
                        registro.Entrada_ajuste = 0;
                        registro.Salida_ajuste = 0;
                        registro.Entrada_devolucion = 0;
                        try
                        {
                            var valid_ubicacion = item.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen_g && x.activo == true).FirstOrDefault();
                            if (valid_ubicacion != null)
                            {
                                registro.Ubicacion = valid_ubicacion.C_almacen_ubicaciones_articulos.nombre_ubicacion;
                            }
                            else
                            {
                                registro.Ubicacion = "NO ASIGNADA";
                            }
                        }
                        catch (Exception)
                        {
                            registro.Ubicacion = "NO ASIGNADA";
                        }

                        captura.Add(registro);
                    }
                }
                return PartialView("../UTILERIAS/ALMACEN/_InventarioFechaTableEditar", captura);
            }
            catch (Exception)
            {
                return PartialView("../UTILERIAS/ALMACEN/_InventarioFechaTableEditar", null);
            }
        }

        public bool RegistrarCapturaAlmacen(int id_inventario_g, int id_articulo)
        {
            try
            {
                C_inventario_almacen_captura_d captura_d = new C_inventario_almacen_captura_d();
                captura_d.id_inventario_g = id_inventario_g;
                captura_d.id_articulo = id_articulo;
                captura_d.cantidad_captura = 0;
                captura_d.cantidad_sistema = 0;
                captura_d.diferencia = 0;
                captura_d.activo = true;
                db.C_inventario_almacen_captura_d.Add(captura_d);
                db.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
        }

        public string ObtenerInfoCapturaAlmacen(int id_inventario_d)
        {
            var captura = from capt in db.C_inventario_almacen_captura_d
                          where capt.id_inventario_d == id_inventario_d
                          select new
                          {
                              capt.C_articulos_catalogo.nombre_articulo,
                              capt.cantidad_captura,
                              capt.cantidad_sistema,
                              capt.diferencia
                          };

            return Newtonsoft.Json.JsonConvert.SerializeObject(captura);
        }

        public bool ModificarCapturaAlmacen(int id_inventario_d, decimal cantidad_captura, decimal cantidad_sistema, decimal diferencia, string justificacion)
        {
            try
            {
                var captura_d = db.C_inventario_almacen_captura_d.Find(id_inventario_d);
                captura_d.cantidad_captura = cantidad_captura;
                captura_d.cantidad_sistema = cantidad_sistema;
                captura_d.diferencia = diferencia;
                db.SaveChanges();


                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se modifico el inventario id: " + id_inventario_d + " Captura: " + cantidad_captura + " Sistema: " + cantidad_sistema + " Diferencia: " + diferencia, justificacion);
                }
                catch (Exception) { }

                return true;
            }
            catch (Exception) { return false; }
        }


        #endregion


        #region BASCULA SISTEMAS
        public ActionResult UtileriasBascula()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8070)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("../UTILERIAS/BASCULA/Index");
        }

        public bool GuardarPrimeraPesadaSistemas(C_bascula_fichas c_ficha)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                c_ficha.activo = true;
                c_ficha.id_usuario = id_usuario;
                db.C_bascula_fichas.Add(c_ficha);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return false;
            }
        }

        public PartialViewResult ConsultarFichasPendientesSistemas(int id_establo)
        {
            var fichas_bascula = db.C_bascula_fichas.Where(x => x.activo == true && x.id_establo == id_establo && x.termina == false).ToList();
            return PartialView("../UTILERIAS/BASCULA/_FichasPendientes", fichas_bascula);
        }

        public PartialViewResult ConsultarFormatoFichaBasculaCp(int idtipoficha)
        {
            //FICHA DE LECHE
            if (idtipoficha == 1)
            {
                return PartialView("../UTILERIAS/BASCULA/TipoFicha/_FichaLechePp");
            }
            //FICHA FORRAJE
            else if (idtipoficha == 2)
            {
                return PartialView("../UTILERIAS/BASCULA/TipoFicha/_FichaForrajePp");
            }
            //FICHA REGULAR
            else
            {
                return PartialView("../UTILERIAS/BASCULA/TipoFicha/_FichaRegularPp");
            }
        }

        public bool GuardarFichaCompletaSistemas(C_bascula_fichas c_ficha)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                c_ficha.activo = true;
                c_ficha.id_usuario = id_usuario;
                db.C_bascula_fichas.Add(c_ficha);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return false;
            }
        }
        #endregion




        #region SALIDAS DE GANADO
        public ActionResult UtileriaSalidasGanado()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7033)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("SALIDAS_GANADO/Index");
        }

        public PartialViewResult ConsultarSalidaGanadoEditar(string folio, int id_establo)
        {
            var salida = db.C_salida_ganado_d.Where(x => x.folio == folio && x.C_salida_ganado_g.id_establo == id_establo && x.rstatus == "A").ToList();
            if (salida.Count() == 0)
            {
                return PartialView("SALIDAS_GANADO/_InformacionSalidaGanadoEditar", null);
            }
            else
            {
                return PartialView("SALIDAS_GANADO/_InformacionSalidaGanadoEditar", salida);
            }
        }

        public int ActualizarSalidaGanado(C_salida_ganado_g salida, int[] id_salida_d, string[] aretes, string[] clasificaciones, string[] causas, string[] condiciones, string[] salas, string[] edades,
            decimal[] cantidades, decimal[] pesos_neto, decimal[] p_unitarios, string justificacion)
        {
            try
            {
                DateTime Hoy = DateTime.Now;
                int id_salida_g = salida.id_salida_gan_g;
                var salida_g = db.C_salida_ganado_g.Find(id_salida_g);
                salida_g.ficha = salida.ficha;
                salida_g.ganado = salida.ganado;
                salida_g.pesador = salida.pesador;
                salida_g.condicion = salida.condicion;
                salida_g.peso1 = salida.peso1;
                salida_g.peso2 = salida.peso2;
                salida_g.peso_t = salida.peso_t;
                salida_g.chofer = salida.chofer;
                salida_g.placas = salida.placas;
                salida_g.vehiculo = salida.vehiculo;
                salida_g.comprador = salida.comprador;
                db.C_salida_ganado_d.Where(x => x.id_salida_gan_g == id_salida_g).ForEach(x => x.rstatus = "E");
                db.SaveChanges();

                for (int i = 0; i < id_salida_d.Length; i++)
                {
                    int id_salida_detalle = id_salida_d[i];
                    var valid_salida = db.C_salida_ganado_d.Where(x => x.id_salida_gan_g == id_salida_g && x.id_saliga_gan_d == id_salida_detalle).FirstOrDefault();
                    if (valid_salida != null)
                    {
                        valid_salida.arete = aretes[i];
                        valid_salida.clasifica = clasificaciones[i];
                        valid_salida.causa_b = causas[i];
                        valid_salida.cond_ind = condiciones[i];
                        valid_salida.sala = salas[i];
                        valid_salida.edad = edades[i];
                        valid_salida.cantidad = cantidades[i];
                        valid_salida.peso = pesos_neto[i];
                        valid_salida.p_unitario = p_unitarios[i];
                        valid_salida.rstatus = "A";
                        db.SaveChanges();
                    }
                    else
                    {
                        C_salida_ganado_d salida_d = new C_salida_ganado_d();
                        salida_d.folio = salida.folio;
                        salida_d.id_salida_gan_g = id_salida_g;
                        salida_d.arete = aretes[i];
                        salida_d.clasifica = clasificaciones[i];
                        salida_d.causa_b = causas[i];
                        salida_d.cond_ind = condiciones[i];
                        salida_d.sala = salas[i];
                        salida_d.edad = edades[i];
                        salida_d.cantidad = cantidades[i];
                        salida_d.peso = pesos_neto[i];
                        salida_d.p_unitario = p_unitarios[i];
                        salida_d.valor = 0;
                        salida_d.rstatus = "A";
                        salida_d.rtime = Hoy;
                        salida_d.tregistro = Hoy;
                        db.C_salida_ganado_d.Add(salida_d);
                        db.SaveChanges();
                    }
                }

                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se edito la salida de ganado con el folio: " + salida.folio, justificacion);
                }
                catch (Exception) { }

                return salida.id_salida_gan_g;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int EliminarSalidaGanado(int id_salida_ganado, string justificacion)
        {
            try
            {
                var salida = db.C_salida_ganado_g.Find(id_salida_ganado);
                salida.rstatus = "E";
                salida.cancelada = true;
                db.C_salida_ganado_d.Where(x => x.id_salida_gan_g == id_salida_ganado).ToList().ForEach(x => x.rstatus = "E");
                db.SaveChanges();


                int id_usuario = (int)Session["LoggedId"];
                try
                {
                    RegistroLogsSoporteGral(id_usuario, "Se desactivo la salida de ganado con el ID: " + id_salida_ganado, justificacion);
                }
                catch (Exception) { }

                return 0;
            }
            catch (Exception ex)
            {
                string log = ex.ToString();
                return 1;
            }
        }


        #endregion


        #region Pruebas
        public ActionResult IndexTesteo()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8093)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception ex)
            {
                return RedirectToAction("usuariologin", "USUARIOLOGIN");
            }

            return View("../UTILERIAS/PRUEBAS/Index");
        }

        public PartialViewResult AgregarVacaTablaPrueba(int count_tabla, string arete, string clasif, string causa, string sala, string edad,
    decimal peso_neto, decimal cantidad, decimal p_unitario, decimal importe, string condicion, string imagen_base64)
        {
            C_salida_ganado_d detalle = new C_salida_ganado_d();
            detalle.arete = arete;
            detalle.clasifica = clasif;
            detalle.causa_b = causa;
            detalle.sala = sala;
            detalle.cond_ind = condicion;
            detalle.edad = edad.ToString();
            detalle.peso = peso_neto;
            detalle.cantidad = cantidad;
            detalle.p_unitario = p_unitario;

            ViewBag.importe = importe;
            ViewBag.count = count_tabla;
            ViewBag.imagen_base64 = imagen_base64;

            return PartialView("../UTILERIAS/PRUEBAS/_VParcial", detalle);
        }

        public string GenerarSalidaGanadoPrueba(string folio_g, int id_salida_g, string[] aretes, string[] clasificaciones, string[] causas, string[] condiciones, string[] salas, string[] edades,
decimal[] cantidades, decimal[] pesos_neto, decimal[] p_unitarios, decimal[] importes, string[] imagen_base64)
        {
            try
            {
                var client = new HttpClient();
                var byteArray = Encoding.ASCII.GetBytes("jaleman:jaleman140923");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                DateTime Hoy = DateTime.Now;
                for (int i = 0; i < importes.Length; i++)
                {
                    C_salida_ganado_d salida_d = new C_salida_ganado_d();
                    salida_d.folio = folio_g;
                    salida_d.id_salida_gan_g = id_salida_g;
                    salida_d.arete = aretes[i];
                    salida_d.clasifica = clasificaciones[i];
                    salida_d.causa_b = causas[i];
                    salida_d.cond_ind = condiciones[i];
                    salida_d.sala = salas[i];
                    salida_d.edad = edades[i];
                    salida_d.cantidad = cantidades[i];
                    salida_d.peso = pesos_neto[i];
                    salida_d.p_unitario = p_unitarios[i];
                    salida_d.valor = importes[i];
                    salida_d.rstatus = "A";
                    salida_d.rtime = Hoy;
                    salida_d.tregistro = Hoy;

                    #region IMAGEN                    
                    try
                    {
                        // Extraer la parte de datos base64 (eliminando el prefijo si existe)
                        var base64Data = imagen_base64[i].Split(',')[1];
                        byte[] imageBytes = Convert.FromBase64String(base64Data);
                        // Generar un nombre de archivo único si no se proporcionan IDs
                        string fileName = $"SGDO_{id_salida_g}_{aretes[i]}_{i}.jpg";
                        // Crear el contenido para subir
                        var content = new ByteArrayContent(imageBytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Ajustar según el tipo real de imagen

                        var uploadUrl = $"https://cloud.beta.com.mx/remote.php/webdav/Fotos_SIIB/{fileName}";
                        var response = client.PutAsync(uploadUrl, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            salida_d.path = uploadUrl;
                        }
                        else
                        {
                            salida_d.path = "NA";
                        }
                    }
                    catch (Exception)
                    {
                        salida_d.path = "NA_ERROR";
                    }
                    #endregion

                    //db_master.C_salida_ganado_d.Add(salida_d);
                }
                //db_master.SaveChanges();
                return folio_g;
            }
            catch (Exception EX)
            {
                string msj = EX.ToString();
                return "";
            }
        }
        #endregion

        #region ADMINISTRADOR DE DISPOSITIVOS
        public ActionResult AdministradorDispositivos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8105)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("/Views/UTILERIAS/DISPOSITIVOS/Index.cshtml");
        }

        public PartialViewResult DispositivosActivosTable(int[] id_tipo_dispositivo, int[] id_establo, int[] id_estatus)
        {
            if (id_tipo_dispositivo.Contains(0))
            {
                id_tipo_dispositivo = db.C_dispositivos_tipos.Where(x => x.activo == true).Select(x => x.id_dispositivo_tipo).ToArray();
            }
            if (id_establo.Contains(0))
            {
                id_establo = db.C_establos.Where(x => x.activo == true).Select(x => x.id_establo).ToArray();
            }

            var ahoraUtc = DateTime.Now;
            var umbralMin = 10;

            var ultimosLatidos = from latido in db.C_dispositivos_latidos
                                 group latido by latido.mac_address into g
                                 select new { mac = g.Key, ultimo = g.Max(z => z.fecha) };

            var estados = from dispositivos in db.C_dispositivos_autorizados
                          where dispositivos.activo == true
                                && id_tipo_dispositivo.Contains((int)dispositivos.id_dispositivo_tipo)
                                && id_establo.Contains((int)dispositivos.id_establo)
                          join est in db.C_establos on dispositivos.id_establo equals est.id_establo
                          join tip in db.C_dispositivos_tipos on dispositivos.id_dispositivo_tipo equals tip.id_dispositivo_tipo
                          join hora in ultimosLatidos on dispositivos.mac_address equals hora.mac into union
                          from hora in union.DefaultIfEmpty()
                          let minutos = (hora.ultimo == null) ? (int?)null : DbFunctions.DiffMinutes(hora.ultimo, ahoraUtc)
                          let online = (hora.ultimo != null) && minutos <= umbralMin
                          where id_estatus.Contains(0)
                                || (id_estatus.Contains(1) && !online)
                                || (id_estatus.Contains(2) && online)
                          select new DeviceStatus
                          {
                              idAutorizado = dispositivos.id_dispositivo,
                              NombreDispositivo = dispositivos.dispositivo,
                              Mac = dispositivos.mac_address,
                              Tipo = tip.tipo_dispositivo,
                              Establo = est.nombre_establo,
                              UltimoLatido = hora.ultimo,
                              MinDesdeUltimo = minutos,
                              Online = online,
                              Bloqueo = (bool)dispositivos.bloqueo,
                              Activo = (bool)dispositivos.activo
                          };


            var lista = estados
                .OrderByDescending(x => x.Online)
                .ThenByDescending(x => x.UltimoLatido)
                .ToList();

            return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosActivosTable", lista);
        }

        public PartialViewResult DispositivosAutorizadosTable(int estatus)
        {
            IQueryable<C_dispositivos_autorizados> query = db.C_dispositivos_autorizados;
            if (estatus != -1)
            {
                bool activo = estatus == 1;
                query = query.Where(x => x.activo == activo);
            }
            var dispositivos = query.ToList();
            return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosAutorizadosTable", dispositivos);
        }


        public PartialViewResult DispositivosGlobalesTable()
        {
            var lista_dispositivos = db.C_dispositivos_autorizados.Where(x => x.activo == true && x.id_dispositivo_tipo != null && x.id_establo != null).ToList();
            ViewBag.ListaComandosGlobales = db.C_dispositivos_comandos_globales.ToList();
            ViewBag.ListaComandosEjecutados = db.C_dispositivos_comandos_ejecutados.ToList();

            return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosGlobalesTable", lista_dispositivos);
        }


        public PartialViewResult ConsultarTipoDispositivoSelect()
        {
            var tipo_dispositivo = db.C_dispositivos_tipos.Where(x => x.activo == true).ToList();
            return PartialView("../UTILERIAS/DISPOSITIVOS/_TipoDispositivoSelect", tipo_dispositivo);
        }

        public PartialViewResult ConsultarComandosSelect()
        {
            var comandos = db.C_dispositivos_comandos_catalogo.Where(x => x.activo == true).ToList();
            return PartialView("../UTILERIAS/DISPOSITIVOS/_ComandosSelect", comandos);
        }

        public bool EjecutarComandoGlobal(int id_comando, int id_comando_tipo, string comando_mac, bool perpetuo, bool general, string comando_parametro)
        {
            try
            {
                C_dispositivos_comandos_globales global = new C_dispositivos_comandos_globales();
                global.id_comando = id_comando;
                if (id_comando_tipo != 0)
                {
                    global.para_tipo = id_comando_tipo;
                }
                global.para_mac = comando_mac;
                global.es_perpetuo = perpetuo;
                global.para_todos = general;
                global.activo = true;
                if (id_comando == 7)
                {
                    global.parametro = double.TryParse(comando_parametro, out _) ? comando_parametro : "100";
                }
                else
                {
                    global.parametro = comando_parametro;
                }
                global.fecha_creacion = DateTime.Now;
                db.C_dispositivos_comandos_globales.Add(global);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public PartialViewResult DispositivosLogsTable(string logs_mac, DateTime fecha_inicio, DateTime fecha_fin)
        {
            DateTime fecha = fecha_fin.AddDays(1).AddSeconds(-1);
            if (logs_mac.Trim() == "")
            {
                var logs = db.C_dispositivos_logs.Where(x => x.fecha > fecha_inicio && x.fecha < fecha).ToList();
                return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosLogsTable", logs);
            }
            else
            {
                var logs = db.C_dispositivos_logs.Where(x => x.fecha > fecha_inicio && x.fecha < fecha && x.mac_address == logs_mac).ToList();
                return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosLogsTable", logs);
            }

        }

        public bool OnOffBloqueoDispositivo(int id_dispositivo, bool estatus)
        {
            try
            {
                var dispositivo = db.C_dispositivos_autorizados.Find(id_dispositivo);
                dispositivo.bloqueo = estatus;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OnOffEstatusDispositivo(int id_dispositivo, bool estatus)
        {
            try
            {
                var dispositivo = db.C_dispositivos_autorizados.Find(id_dispositivo);
                dispositivo.activo = estatus;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool FalloDispositivo(int id_dispositivo)
        {
            try
            {
                var dispositivo = db.C_dispositivos_autorizados.Find(id_dispositivo);
                dispositivo.dispositivo = "FALLO - " + dispositivo.dispositivo;
                dispositivo.mac_address = "XX-" + dispositivo.mac_address;
                dispositivo.activo = false;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public PartialViewResult DispositivosAdmComandosTable()
        {
            var comandos = db.C_dispositivos_comandos_globales.Where(x => x.activo == true).ToList();
            return PartialView("../UTILERIAS/DISPOSITIVOS/_DispositivosAdmComandosTable", comandos);
        }


        public bool OnOffComandoDispositivo(int id_comando, bool estatus)
        {
            try
            {
                var comando = db.C_dispositivos_comandos_globales.Find(id_comando);
                comando.activo = estatus;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region ADMINISTRADOR DE SOPORTE
        public ActionResult AdministradorSoporte()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8105)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("/Views/UTILERIAS/SOPORTE/Index.cshtml");
        }


        public bool RegistrarSoporte(string titulo, string descripcion, int id_estado_g, int id_prioridad, int id_usuario_solicita, int id_categoria, int id_tipo, string solicitud, int id_estado_d, DateTime fecha_inicio, DateTime fecha_termino)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                C_soporte_tickets_g ticket_g = new C_soporte_tickets_g();
                ticket_g.titulo = titulo;
                ticket_g.descripcion = descripcion;
                ticket_g.id_soporte_estado = id_estado_g;
                ticket_g.id_soporte_prioridad = id_prioridad;
                ticket_g.fecha_registro = DateTime.Now.AddMinutes(-10);
                ticket_g.fecha_cierra = DateTime.Now;
                ticket_g.id_usuario_solicita = id_usuario_solicita;
                ticket_g.activo = true;
                db.C_soporte_tickets_g.Add(ticket_g);
                db.SaveChanges();

                C_soporte_tickets_d ticket_d = new C_soporte_tickets_d();
                ticket_d.id_soporte_ticket_g = ticket_g.id_soporte_ticket_g;
                ticket_d.id_soporte_categoria = id_categoria;
                ticket_d.id_soporte_tipo = id_tipo;
                ticket_d.solicitud = solicitud;
                ticket_d.id_soporte_estado = id_estado_d;
                ticket_d.fecha_inico = fecha_inicio;
                ticket_d.fecha_termino = fecha_termino;
                ticket_d.id_usuario_realiza = id_usuario;
                ticket_d.activo = true;

                db.C_soporte_tickets_d.Add(ticket_d);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public PartialViewResult ConsultarHistorialSoporteTable(int[] estatus, int[] prioridad, DateTime fecha1, DateTime fecha2)
        {
            DateTime fecha = fecha2.AddDays(1).AddSeconds(-1);
            if (estatus.Contains(0))
            {
                estatus = db.C_soporte_estado.Where(x => x.activo == true).Select(x => x.id_soporte_estado).ToArray();
            }
            if (prioridad.Contains(0))
            {
                prioridad = db.C_soporte_prioridad.Where(x => x.activo == true).Select(x => x.id_soporte_prioridad).ToArray();
            }

            var historial = db.C_soporte_tickets_d.Where(x => x.activo == true && estatus.Contains((int)x.id_soporte_estado) && prioridad.Contains((int)x.C_soporte_tickets_g.id_soporte_prioridad) && x.fecha_inico > fecha1 && x.fecha_termino < fecha).ToList();

            return PartialView("../UTILERIAS/SOPORTE/_HistorialSoporteTable", historial);
        }
        #endregion

    }
}