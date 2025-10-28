using Beta_System.Helper;
using Beta_System.Models;
using DocumentFormat.OpenXml.Office2013.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class REQUISICIONESController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        NOTIFICACIONESController notificacion = new NOTIFICACIONESController();
        CATALOGOSController catalogos = new CATALOGOSController();

        List<PermisosUsuario> permisos;
        List<SubmodulosUsuario> submodulos;
        List<ModulosUsuario> modulos;
        List<int> sub_modulos_session;

        #region------------------ GENERACION DE REQUISICIONES
        public ActionResult RequisicionIndex()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4016)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("../COMPRAS/REQUISICIONES/GENERAR_REQUI/Index");
        }

        public void CambiarStatusRequisicion(int id_requi, int id_status)
        {
            try
            {
                var requi = db.C_compras_requi_g.Find(id_requi);
                if (requi.id_status_requi != id_status) { requi.id_status_requi = id_status; db.SaveChanges(); }
                return;
            }
            catch (Exception)
            {
                return;
            }
        }


        public PartialViewResult ConsultarTrackingRequisicion(int id_requisicion_g)
        {
            try
            {
                ViewBag.id_requi_g = id_requisicion_g;

                var requisicion = db.C_compras_requi_g.Find(id_requisicion_g);

                TrackingRequisiciones tracking = new TrackingRequisiciones();
                tracking.Registrada = true;
                tracking.fecha_registro = (DateTime)requisicion.fecha_registro;

                if (requisicion.aut_1_fecha != null) { tracking.Firma_1 = true; tracking.fecha_firma_1 = (DateTime)requisicion.aut_1_fecha; }

                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true);
                tracking.cotizaciones = cotizaciones.ToList();

                tracking.confirmaciones = cotizaciones.Where(x => x.confirmada == true && x.fecha_confirmacion != null).ToList();

                tracking.parcialidades = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.id_status_cotizacion_confirmada != 3
                                            && x.activo == true).ToList();

                if (requisicion.aut_5_fecha != null) { tracking.Firma_2 = true; tracking.fecha_firma_2 = (DateTime)requisicion.aut_5_fecha; }

                tracking.ordenes_compra = db.C_compras_ordenes_g.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true).ToList();
                tracking.no_ordenes_compra = tracking.ordenes_compra.Count();



                return PartialView("../COMPRAS/REQUISICIONES/_TrackingRequisicion", tracking);
            }
            catch (Exception)
            {
                return PartialView("../COMPRAS/REQUISICIONES/_TrackingRequisicion", null);
            }
        }

        public PartialViewResult ConsularTrackingLogRequisicion(int id_requi_g)
        {
            try
            {
                var logs = db.C_compras_requisiciones_logs.Where(x => x.id_requisicion == id_requi_g && x.activo == true && x.id_tipo_log == 6).OrderBy(x => x.fecha_registro).ToList();
                return PartialView("../COMPRAS/REQUISICIONES/_TrackingLogRequisicion", logs);
            }
            catch (Exception)
            {
                return PartialView("../COMPRAS/REQUISICIONES/_TrackingLogRequisicion", null);
            }
        }

        public PartialViewResult ConsultarTrackingOrdenes(int id_requi_g)
        {
            var ordenes = db.C_compras_ordenes_g.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.activo == true).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/_TrackingOrdenesRequisicion", ordenes);
        }



        public bool RegistarArticulo(C_compras_requi_g c_requi, C_compras_requi_d c_requi_d)
        {
            try
            {
                #region requi_g
                //Articulo
                c_requi.id_status_requi = 1;
                c_requi.id_usuario_registro = Convert.ToInt32(Session["LoggedId"]);
                c_requi.id_status_requi = 1;

                c_requi.aut_1_status = null;
                c_requi.aut_1_fecha = null;
                c_requi.aut_1_usuario = null;

                c_requi.aut_2_status = null;
                c_requi.aut_2_fecha = null;
                c_requi.aut_2_usuario = null;

                c_requi.aut_3_status = null;
                c_requi.aut_3_fecha = null;
                c_requi.aut_3_usuario = null;

                c_requi.aut_4_status = null;
                c_requi.aut_4_fecha = null;
                c_requi.aut_4_usuario = null;

                c_requi.aut_5_status = null;
                c_requi.aut_5_fecha = null;
                c_requi.aut_5_usuario = null;

                c_requi.cotizada = null;
                c_requi.activo = true;
                db.SaveChanges();
                #endregion

                #region requi_d
                c_requi_d.id_requisicion_articulo_d = c_requi.id_requisicion_articulo_g;
                db.SaveChanges();
                #endregion

                #region REGISTRO
                //Registro
                //C_articulos_logs registo = new C_articulos_logs();
                //registo.id_usuario_log = Convert.ToInt32(Session["LoggedId"]);
                //registo.id_articulo = c_articulo.id_articulo;
                //registo.valor_anterior = "0";
                //registo.valor_nuevo = precio.ToString();
                ////registo.fecha_log = DateTime.Now;
                //registo.descripcion_cambio = "Regristo de articulo";
                //registo.activo = true;
                //db.C_articulos_logs.Add(registo);
                //db.SaveChanges();
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public PartialViewResult RegistrarArtRequi(int count_tabla, string id_articulo, string articulo, string cantidad, string precio, string unidad_medida, int id_unidad_medida,
            string observacion, int id_centro_g, string cargo_contable, int id_cargo_contable, int id_cuenta_contable_g, bool new_cotizacion, int id_tipo_requi, string cve_prov, int id_cotizacion, int id_tipo_moneda, int id_articulo_original)
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
                        id_tipo_moneda = (int)proveedor.id_tipo_moneda;
                    }
                }
            }





            ViewBag.count_tabla = count_tabla;
            ViewBag.id_articulo = id_articulo;
            ViewBag.articulo = articulo;
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
            ViewBag.id_articulo_original = id_articulo_original;

            return PartialView("../COMPRAS/REQUISICIONES/GENERAR_REQUI/_ArticuloRequi");
        }

        public string GenerarRequiGeneral(C_compras_requi_g c_requi_g, string folio)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                c_requi_g.concepto = c_requi_g.concepto.ToUpper();
                c_requi_g.id_usuario_registro = id_usuario;
                c_requi_g.fecha_registro = DateTime.Now;
                c_requi_g.id_status_requi = 1;
                c_requi_g.cotizada = null;
                c_requi_g.activo = true;

                c_requi_g.aut_1_status = null;
                c_requi_g.aut_1_fecha = null;
                c_requi_g.aut_1_usuario = null;

                c_requi_g.aut_2_status = null;
                c_requi_g.aut_2_fecha = null;
                c_requi_g.aut_2_usuario = null;

                c_requi_g.aut_3_status = null;
                c_requi_g.aut_3_fecha = null;
                c_requi_g.aut_3_usuario = null;

                c_requi_g.aut_4_status = null;
                c_requi_g.aut_4_fecha = null;
                c_requi_g.aut_4_usuario = null;

                c_requi_g.aut_5_status = null;
                c_requi_g.aut_5_fecha = null;
                c_requi_g.aut_5_usuario = null;
                c_requi_g.importe_total = 0;
                db.C_compras_requi_g.Add(c_requi_g);
                db.SaveChanges();

                var id_requi_g = c_requi_g.id_requisicion_articulo_g;
                var id_centro = db.C_centros_g.Find(c_requi_g.id_centro_g);
                string folio_requi = id_centro.siglas + id_requi_g;
                var idFolio = id_requi_g + "|" + folio_requi;

                //var requisicion = db.C_compras_requi_g.Find(id_requi_g);
                c_requi_g.folio = folio_requi;
                db.SaveChanges();

                return idFolio;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return null;
            }
        }


        public int GenerarRequiDetallada(int id_requi_g, int[] id_articulos, string[] nombre_art, decimal[] cantidads, decimal[] precios, int[] id_unidad_medidas, string[] observacions, int[] id_centros_g, int[] id_cargos_contable, int[] id_cuentas_contable_g)
        {
            try
            {
                if (cantidads.Contains(0))
                {
                    var requi_g = db.C_compras_requi_g.Find(id_requi_g);
                    if (requi_g != null)
                    {
                        requi_g.activo = false;
                        requi_g.id_status_requi = 2008;
                    }
                    return -1;
                }

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_compras_requi_d detalle = new C_compras_requi_d();
                    detalle.id_requisicion_articulo_g = id_requi_g;
                    detalle.id_articulo = id_articulos[i];
                    detalle.nombre_art = nombre_art[i];
                    detalle.cantidad = cantidads[i];
                    detalle.precio = precios[i];
                    detalle.id_unidad_medida = id_unidad_medidas[i];
                    if (observacions[i] != null) { detalle.observacion = observacions[i].ToUpper(); }
                    else { detalle.observacion = ""; }
                    detalle.id_centro_g = id_centros_g[i];
                    detalle.id_cargo_contable = id_cargos_contable[i];
                    detalle.id_cuenta_contable_g = id_cuentas_contable_g[i];
                    detalle.cantidad_cotizada = 0;
                    db.C_compras_requi_d.Add(detalle);
                }
                db.SaveChanges();
                return id_requi_g;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public bool ProcesarRequisicionServicio(int id_requi_g, int[] id_cotizaciones, int[] id_proveedores, int[] id_tipos_moneda, decimal[] precios, int[] id_articulos, int[] id_unidad_medidas)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = 2109;

                var requi_g = db.C_compras_requi_g.Find(id_requi_g);
                requi_g.id_status_requi = 6;
                requi_g.cotizada = true;

                requi_g.aut_1_status = true;
                requi_g.aut_1_fecha = hoy;
                requi_g.aut_1_usuario = id_usuario;

                requi_g.aut_2_status = true;
                requi_g.aut_2_fecha = hoy;
                requi_g.aut_2_usuario = id_usuario;

                requi_g.aut_3_status = true;
                requi_g.aut_3_fecha = hoy;
                requi_g.aut_3_usuario = id_usuario;

                var articulos_requi = requi_g.C_compras_requi_d.OrderBy(x => x.id_requisicion_articulo_d).ToArray();
                if (articulos_requi.Length != id_cotizaciones.Length)
                {
                    return false;
                }

                List<RequiServicio> ServicioRequi = new List<RequiServicio>();
                for (int i = 0; i < id_cotizaciones.Length; i++)
                {
                    int id_articulo_cotizacion = id_articulos[i];
                    int id_articulo_requisicion = (int)articulos_requi[i].id_articulo;
                    if (id_articulo_cotizacion != id_articulo_requisicion)
                    {
                        return false;
                    }
                    RequiServicio requisicion = new RequiServicio();
                    requisicion.IdProveedores = id_proveedores[i];
                    requisicion.IdArticulos = id_articulo_cotizacion;
                    requisicion.Id_Requi_d = articulos_requi[i].id_requisicion_articulo_d;
                    ServicioRequi.Add(requisicion);
                }

                for (int i = 0; i < id_cotizaciones.Length; i++)
                {
                    int id_articulo_cotizacion = id_articulos[i];
                    int id_articulo_detalle = articulos_requi[i].id_requisicion_articulo_d;

                    int id_proveedor = id_proveedores[i];

                    int id_historico = id_cotizaciones[i];
                    if (id_historico == 0)
                    {
                        C_compras_cotizaciones_articulos historico = new C_compras_cotizaciones_articulos();
                        historico.id_articulo = id_articulo_cotizacion;
                        historico.id_compras_proveedor = id_proveedor;
                        historico.precio_unitario = precios[i];
                        historico.precio_final = precios[i];
                        historico.porcentaje_descuento = 0;
                        historico.id_unidad_medida = id_unidad_medidas[i];
                        historico.fecha_cotizacion = hoy;
                        historico.fecha_vigencia = hoy.AddMonths(3);
                        historico.id_usuario_cotiza = id_usuario;
                        historico.id_tipo_moneda = id_tipos_moneda[i];
                        historico.activo = true;
                        historico.fecha_actualizacion = hoy;
                        historico.dias_entrega = 7;
                        db.C_compras_cotizaciones_articulos.Add(historico);
                        db.SaveChanges();
                        id_historico = historico.id_compra_articulo_cotizacion;
                    }

                    var detalle_requi = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.id_articulo == id_articulo_cotizacion
                                                                                                                && x.id_requisicion_articulo_d == id_articulo_detalle).ToList();
                    foreach (var item in detalle_requi)
                    {
                        C_compras_cotizaciones_requisiciones coti_requi = new C_compras_cotizaciones_requisiciones();
                        coti_requi.id_requisicion_articulo_g = id_requi_g;
                        coti_requi.id_compra_articulo_cotizacion = id_historico;
                        coti_requi.dias_entrega = 3;
                        coti_requi.fecha_registro = hoy;
                        coti_requi.fecha_confirmacion = hoy;
                        coti_requi.comentarios = "";
                        coti_requi.confirmada = true;
                        coti_requi.id_usuario_registra = id_usuario;
                        coti_requi.id_articulo = id_articulo_cotizacion;
                        coti_requi.activo = true;
                        coti_requi.id_requisicion_articulo_d = item.id_requisicion_articulo_d;
                        coti_requi.id_usuario_actualizo = id_usuario;
                        coti_requi.agrupada = true;
                        coti_requi.cantidad_surtir = item.cantidad;
                        db.C_compras_cotizaciones_requisiciones.Add(coti_requi);
                        item.cotizado = true;
                        item.cantidad_cotizada = item.cantidad;
                        db.SaveChanges();
                    }
                }

                foreach (var proveedor in ServicioRequi.GroupBy(x => x.IdProveedores))
                {
                    int id_proveedor = proveedor.FirstOrDefault().IdProveedores;
                    string correo_prov = db.C_compras_proveedores.Find(id_proveedor).contacto_correo_1;
                    C_compras_cotizaciones_confirmadas_g confirmadas_g = new C_compras_cotizaciones_confirmadas_g();
                    confirmadas_g.id_requisicion_articulo_g = id_requi_g;
                    confirmadas_g.fecha_registro = hoy;
                    confirmadas_g.id_usuario_registra = id_usuario;
                    confirmadas_g.activo = true;
                    confirmadas_g.confirmada = true;
                    confirmadas_g.nombre_suborden = requi_g.concepto;
                    confirmadas_g.id_status_cotizacion_confirmada = 1; //PENDIENTE
                    confirmadas_g.solicita_autorizacion = false;
                    confirmadas_g.id_compras_ubicacion_entrega = 9;
                    confirmadas_g.correo_proveedor_suborden = correo_prov;
                    db.C_compras_cotizaciones_confirmadas_g.Add(confirmadas_g);
                    db.SaveChanges();

                    foreach (var item in proveedor)
                    {
                        var id_cotizacion_requi = db.C_compras_cotizaciones_requisiciones.Where(x => x.activo == true && x.id_articulo == item.IdArticulos && x.id_requisicion_articulo_g == id_requi_g
                                                    && x.id_requisicion_articulo_d == item.Id_Requi_d).Select(x => x.id_cotizacion_requisicion).ToList();

                        foreach (var confirmada_d in id_cotizacion_requi)
                        {
                            int id_confirmacion_g = confirmadas_g.id_cotizacion_confirmada_g;
                            C_compras_cotizaciones_confirmadas_d confirmadas_d = new C_compras_cotizaciones_confirmadas_d();
                            confirmadas_d.id_cotizacion_confirmada_g = id_confirmacion_g;
                            confirmadas_d.id_cotizacion_requisicion = confirmada_d;
                            confirmadas_d.activo = true;
                            db.C_compras_cotizaciones_confirmadas_d.Add(confirmadas_d);
                            db.SaveChanges();
                        }
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }


        public int CambiarArticuloRequisicionServicio(int id_requi_d, int id_requi_g, int id_cotizacion, string cve_prov, decimal precio, decimal cantidad, int id_articulo, string observacion, int id_cargo, int id_cuenta, bool cambiar_cotizacion)
        {
            try
            {
                var detalle = db.C_compras_requi_d.Find(id_requi_d);
                if (detalle.C_compras_requi_g.id_requisicion_tipo != 2) { if (detalle.C_compras_requi_g.aut_1_status != null) { return 1; } }
                if (detalle.C_compras_requi_g.activo == false) { return 2; }

                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                var cotizacion_requi = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == id_requi_d && x.activo == true && x.confirmada == true && x.agrupada == true).FirstOrDefault();
                if (cotizacion_requi != null)
                {
                    if (cotizacion_requi.id_articulo != id_articulo && cambiar_cotizacion == false)  //NO SE PUEDE CAMBIAR EL ARTÍCULO Y DEJAR LA MISMA COTIZACION
                    {
                        return 3;
                    }

                    detalle.id_articulo = id_articulo;
                    detalle.cantidad = cantidad;
                    detalle.cantidad_cotizada = cantidad;
                    detalle.observacion = observacion.ToUpper();
                    detalle.id_cargo_contable = id_cargo;
                    detalle.id_cuenta_contable_g = id_cuenta;

                    if (cotizacion_requi.C_compras_cotizaciones_articulos.id_compra_articulo_cotizacion != id_cotizacion && cambiar_cotizacion == true) //SI ES PROVEEDOR DIFERENTE ES NUEVA COTIZACIÓN
                    {
                        if (id_cotizacion == 0 || cotizacion_requi.id_articulo != id_articulo)
                        {
                            var info_prov = db.C_compras_proveedores.Where(x => x.cve == cve_prov && x.activo == true).FirstOrDefault();
                            if (info_prov == null) { return 4; }
                            C_compras_cotizaciones_articulos historico = new C_compras_cotizaciones_articulos();
                            historico.id_articulo = id_articulo;
                            historico.id_compras_proveedor = info_prov.id_compras_proveedor; ;  //ES EL ID NO LA CLAVE
                            historico.precio_unitario = precio;
                            historico.precio_final = precio;
                            historico.porcentaje_descuento = 0;
                            historico.id_unidad_medida = 10;  //SERV
                            historico.fecha_cotizacion = hoy;
                            historico.fecha_vigencia = hoy.AddMonths(3);
                            historico.id_usuario_cotiza = id_usuario;
                            historico.id_tipo_moneda = info_prov.id_tipo_moneda;
                            historico.activo = true;
                            historico.fecha_actualizacion = hoy;
                            historico.dias_entrega = 7;
                            detalle.precio = precio;

                            cotizacion_requi.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true).FirstOrDefault().C_compras_cotizaciones_confirmadas_g.correo_proveedor_suborden = info_prov.contacto_correo_1;

                            db.C_compras_cotizaciones_articulos.Add(historico);
                            db.SaveChanges();
                            id_cotizacion = historico.id_compra_articulo_cotizacion;
                        }
                        cotizacion_requi.id_compra_articulo_cotizacion = id_cotizacion;
                    }

                    cotizacion_requi.cantidad_surtir = cantidad;
                    cotizacion_requi.id_articulo = id_articulo;
                    cotizacion_requi.id_usuario_actualizo = id_usuario;

                    

                    db.SaveChanges();
                    return 0;
                }
                return 5;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int AgregarArticuloRequisicionServicio(int id_requi_g, int id_cotizacion, string cve_prov, decimal precio, decimal cantidad, int id_articulo, string observacion, int id_cargo, int id_cuenta)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var requi_g = db.C_compras_requi_g.Find(id_requi_g);
                var info_prov = db.C_compras_proveedores.Where(x => x.cve == cve_prov && x.activo == true).FirstOrDefault();
                if (info_prov == null) { return 4; }
                var info_art = db.C_articulos_catalogo.Find(id_articulo);

                if (requi_g.id_requisicion_tipo != 2) { if (requi_g.aut_1_status != null) { return 1; } }
                if (requi_g.activo == false) { return 2; }

                int id_confirmacion = 0;
                var confirmacion = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.activo == true && x.id_status_cotizacion_confirmada == 1);
                foreach (var item in confirmacion)
                {
                    int[] cotizaciones_parcialidad = item.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true).Select(x => (int)x.id_cotizacion_requisicion).ToArray();
                    int[] id_proveedores = db.C_compras_cotizaciones_requisiciones.Where(x => cotizaciones_parcialidad.Contains((int)x.id_cotizacion_requisicion)).Select(x => (int)x.C_compras_cotizaciones_articulos.id_compras_proveedor).Distinct().ToArray();
                    if (id_proveedores.Count() > 1) { return 3; }
                    if (id_proveedores.Contains(info_prov.id_compras_proveedor)) { id_confirmacion = item.id_cotizacion_confirmada_g; }
                }

                DateTime hoy = DateTime.Now;
                C_compras_requi_d new_requi = new C_compras_requi_d();
                new_requi.id_requisicion_articulo_g = id_requi_g;
                new_requi.id_articulo = id_articulo;
                new_requi.nombre_art = info_art.nombre_articulo;
                new_requi.cantidad = cantidad;
                new_requi.precio = precio;
                new_requi.id_unidad_medida = info_art.id_unidad_medida;
                new_requi.observacion = observacion.ToUpper();
                new_requi.id_centro_g = requi_g.id_centro_g;
                new_requi.id_cargo_contable = id_cargo;
                new_requi.id_cuenta_contable_g = id_cuenta;
                new_requi.cotizado = true;
                new_requi.cantidad_cotizada = cantidad;
                db.C_compras_requi_d.Add(new_requi);
                db.SaveChanges();
                int id_requi_d = new_requi.id_requisicion_articulo_d;

                if (id_cotizacion == 0)
                {
                    C_compras_cotizaciones_articulos historico = new C_compras_cotizaciones_articulos();
                    historico.id_articulo = id_articulo;
                    historico.id_compras_proveedor = info_prov.id_compras_proveedor;
                    historico.precio_unitario = precio;
                    historico.precio_final = precio;
                    historico.porcentaje_descuento = 0;
                    historico.id_unidad_medida = info_art.id_unidad_medida;  //SERV
                    historico.fecha_cotizacion = hoy;
                    historico.fecha_vigencia = hoy.AddMonths(3);
                    historico.id_usuario_cotiza = id_usuario;
                    historico.id_tipo_moneda = info_prov.id_tipo_moneda;
                    historico.activo = true;
                    historico.fecha_actualizacion = hoy;
                    historico.dias_entrega = 7;
                    db.C_compras_cotizaciones_articulos.Add(historico);
                    db.SaveChanges();
                    id_cotizacion = historico.id_compra_articulo_cotizacion;
                }
                C_compras_cotizaciones_requisiciones coti_requi = new C_compras_cotizaciones_requisiciones();
                coti_requi.id_requisicion_articulo_g = id_requi_g;
                coti_requi.id_compra_articulo_cotizacion = id_cotizacion;
                coti_requi.dias_entrega = 3;
                coti_requi.fecha_registro = hoy;
                coti_requi.fecha_confirmacion = hoy;
                coti_requi.comentarios = "";
                coti_requi.confirmada = true;
                coti_requi.id_usuario_registra = id_usuario;
                coti_requi.id_articulo = id_articulo;
                coti_requi.activo = true;
                coti_requi.id_requisicion_articulo_d = id_requi_d;
                coti_requi.id_usuario_actualizo = id_usuario;
                coti_requi.agrupada = true;
                coti_requi.cantidad_surtir = cantidad;
                db.C_compras_cotizaciones_requisiciones.Add(coti_requi);
                db.SaveChanges();
                int id_cotizacion_requisicion = coti_requi.id_cotizacion_requisicion;

                //var confirmacion = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.activo == true).FirstOrDefault();
                if (id_confirmacion == 0) {
                    C_compras_cotizaciones_confirmadas_g confirmadas_g = new C_compras_cotizaciones_confirmadas_g();
                    confirmadas_g.id_requisicion_articulo_g = id_requi_g;
                    confirmadas_g.fecha_registro = hoy;
                    confirmadas_g.id_usuario_registra = id_usuario;
                    confirmadas_g.activo = true;
                    confirmadas_g.confirmada = true;
                    confirmadas_g.nombre_suborden = requi_g.concepto;
                    confirmadas_g.id_status_cotizacion_confirmada = 1; //PENDIENTE
                    confirmadas_g.solicita_autorizacion = false;
                    confirmadas_g.id_compras_ubicacion_entrega = 9;
                    confirmadas_g.correo_proveedor_suborden = info_prov.contacto_correo_1;
                    db.C_compras_cotizaciones_confirmadas_g.Add(confirmadas_g);
                    db.SaveChanges();
                    id_confirmacion = confirmadas_g.id_cotizacion_confirmada_g;
                }
                

                C_compras_cotizaciones_confirmadas_d confirmadas_d = new C_compras_cotizaciones_confirmadas_d();
                confirmadas_d.id_cotizacion_confirmada_g = id_confirmacion;
                confirmadas_d.id_cotizacion_requisicion = id_cotizacion_requisicion;
                confirmadas_d.activo = true;
                db.C_compras_cotizaciones_confirmadas_d.Add(confirmadas_d);
                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public int EliminarArticuloRequisicionServicio(int id_requi_d, int id_requi_g, int id_articulo)
        {
            try
            {
                var valid = db.C_compras_requi_g.Find(id_requi_g);
                if (valid.C_compras_requi_d.Count() == 1)
                {
                    return 1; //NO SE PUEDE ELIMINAR EL ÚNICO ARTÍCULO DE LA REQUISICIÓN
                }

                var detalle = db.C_compras_requi_d.Find(id_requi_d);
                detalle.id_requisicion_articulo_g = null;
                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == id_requi_d).ToList();
                cotizaciones.ForEach(x => x.activo = false);
                int[] cotizaciones_ids = cotizaciones.Select(x => (int)x.id_cotizacion_requisicion).ToArray();
                db.C_compras_cotizaciones_confirmadas_d.Where(x => cotizaciones_ids.Contains((int)x.id_cotizacion_requisicion)).ToList().ForEach(x => x.activo = false);
                db.SaveChanges();
                ValidarParcialidadesVacias(id_requi_g);
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public void ValidarParcialidadesVacias(int id_requi_g)
        {
            var confirmaciones = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.activo == true).ToList();
            foreach (var confirmacion in confirmaciones)
            {
                int[] cotizaciones_parcialidad = confirmacion.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true).Select(x => (int)x.id_cotizacion_requisicion).ToArray();
                if (cotizaciones_parcialidad.Count() == 0)
                {
                    confirmacion.activo = false;
                    confirmacion.id_status_cotizacion_confirmada = 3; //CANCELADA
                    db.SaveChanges();
                }
            }
        }

        public int ValidarFolioInversion(string folio)
        {
            try
            {
                DateTime hoy = DateTime.Today;
                //int folio_inversion = Convert.ToInt32(folio);
                var valid_inversion = db.C_compras_inversiones_programadas.Where(x => x.folio_inversion_requi == folio && x.activo == true).FirstOrDefault();
                if (valid_inversion == null) { return -1; }
                if (valid_inversion.aplicado == true)
                {
                    return -2; //Este folio ya fue usado en una requisición
                }

                if (valid_inversion.fecha_aplicacion > hoy)
                {
                    return -3;  //Inversion programada para otra fecha
                }

                return 0;
            }
            catch (Exception)
            {
                return -1;  //No se encontró el folio
            }
        }

        public bool ProcesarRequisicionInversion(int id_requi_g, string folio_inversion)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];

                //int folio_inver = Convert.ToInt32(folio_inversion);
                var folio = db.C_compras_inversiones_programadas.Where(x => x.folio_inversion_requi == folio_inversion && x.activo == true && x.aplicado == false).FirstOrDefault();
                folio.aplicado = true;
                folio.id_requisicion_articulo_g = id_requi_g;
                folio.id_usuario_aplica = id_usuario;
                folio.fecha_aplica = hoy;
                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }



        #endregion


        #region//-----------AUTORIZACIONES DE REQUISICINOES
        public ActionResult RequisicionesRevision()
        {
            try
            {
                int id_rol = (int)Session["LoggedIdRol"];
                int id_usuario = (int)Session["LoggedId"];

                var valid_master = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == id_usuario && (x.id_usuario_master_accion == 1 || x.id_usuario_master_accion == 2009 || x.id_usuario_master_accion == 2011) && x.activo == true).ToList();
                if (valid_master.Count() > 0)
                {
                    if (valid_master.Select(x => x.id_usuario_master_accion).Contains(2011)) { ViewBag.modo = 4; } //USUARIO ESPECIAL QUE FUNGE COMO MASTER Y NORMAL AFECTANDO PTTO
                    if (valid_master.Select(x => x.id_usuario_master_accion).Contains(2009)) { ViewBag.modo = 3; } //USUARIO ESPECIAL QUE FUNGE COMO MASTER Y NORMAL SIN AFECTAR PTTO
                    else if (valid_master.Select(x => x.id_usuario_master_accion).Contains(1)) { ViewBag.modo = 2; }  //USUARIO UNICAMENTE MASTER
                }
                else
                {
                    ViewBag.modo = 1; //USUARIO NORMAL
                }

                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4017)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            //return View("../COMPRAS/REQUISICIONES/AUTORIZACIONES/AutRevision/Index");
            return View("../COMPRAS/REQUISICIONES/FIRMAS/Index");
        }

        public PartialViewResult ConsultarRequisicionesRevision(int[] id_centro, string requi)
        {
            List<C_compras_requi_g> requis = null;
            int id_usuario = (int)Session["LoggedId"];
            if (id_centro.Contains(0))
            {
                if (id_centro.Contains(0)) { id_centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }
            }
            if (requi != "")
            {
                requis = db.C_compras_requi_g.Where(x => x.id_status_requi == 1 && x.activo == true && id_centro.Contains((int)x.id_centro_g) && x.folio == requi).OrderBy(x => x.fecha_registro).ToList();
            }
            else
            {
                requis = db.C_compras_requi_g.Where(x => x.id_status_requi == 1 && x.activo == true && id_centro.Contains((int)x.id_centro_g)).OrderBy(x => x.fecha_registro).ToList();
            }

            return PartialView("../COMPRAS/REQUISICIONES/AUTORIZACIONES/AutRevision/_RequisicionesRevision", requis);
        }
        public PartialViewResult ConsultarCotizacionesArticuloProveedor(string cve_proveedor, string id_articulo, string modo)
        {
            List<C_compras_cotizaciones_articulos> cotizaciones = null;
            try
            {
                //REGISTRO CON SET COTIZACION MODO = 1;  
                //EDICION CON SET COTIZACION MODO 2
                ViewBag.modo = 1; 
                if (modo != null) { ViewBag.modo = modo; }
                int id_proveedor = (int)db.C_compras_proveedores.Where(x => x.cve == cve_proveedor).FirstOrDefault().id_compras_proveedor;
                DateTime hoy = DateTime.Today;
                cotizaciones = db.C_compras_cotizaciones_articulos.Where(x => x.id_compras_proveedor == id_proveedor && x.C_articulos_catalogo.clave == id_articulo && x.activo == true
                                && x.fecha_vigencia >= hoy).Take(5).ToList();
                if (cotizaciones.Count() == 0) {
                    try
                    {
                        int id_art = Convert.ToInt32(id_articulo);
                        cotizaciones = db.C_compras_cotizaciones_articulos.Where(x => x.id_compras_proveedor == id_proveedor && x.id_articulo == id_art && x.activo == true
                                && x.fecha_vigencia >= hoy).Take(5).ToList();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
                cotizaciones = null;
            }
            return PartialView("../COMPRAS/REQUISICIONES/GENERAR_REQUI/_CotizacionesProveedorArticuloTable", cotizaciones);
        }


        public PartialViewResult ConsultarInformacionRequisicionHeader(int id_requisicion)
        {
            var informacion_requision = db.C_compras_requi_g.Find(id_requisicion);
            return PartialView("../COMPRAS/REQUISICIONES/_RequisicionInfoHeader", informacion_requision);
        }

        public PartialViewResult ConsultarRequisGenerada(int[] id_centro)
        {
            List<C_compras_requi_g> requis = null;
            int id_usuario = (int)Session["LoggedId"];

            if (id_centro.Contains(0)) { id_centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }

            requis = db.C_compras_requi_g.Where(x => x.activo == true && id_centro.Contains((int)x.id_centro_g)).OrderBy(x => x.fecha_registro).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/GENERAR_REQUI/_RequisicionInfo", requis);
        }
        public PartialViewResult ConsultarRequisGeneradaFiltro(int[] id_centro, string fecha_inicial, string fecha_final, int[] id_status, int[] id_tipo_requi, string folio)
        {
            DateTime FechaInicial = DateTime.Parse(fecha_inicial + " 00:00:00");
            DateTime FechaFinal = DateTime.Parse(fecha_final + " 23:59:59");
            List<bool> activo = new List<bool>();
            activo.Add(true);

            List<C_compras_requi_g> requis = null;
            int id_usuario = (int)Session["LoggedId"];
            if (id_centro.Contains(0)) { id_centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }
            if (id_tipo_requi.Contains(0)) { id_tipo_requi = catalogos.ConsultarTiposRequisicionesID(); }


            if (id_status.Contains(0)) { id_status = db.C_compras_requisiciones_status.Where(x => x.activo == true).Select(x => x.id_status_requi).ToArray(); }

            if (id_status.Contains(1008)) { activo.Add(false); }
            if (id_status.Contains(2008)) { activo.Add(false); }

            if (folio != "")
            {
                if (!id_centro.Contains(0)) { id_centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }
                requis = db.C_compras_requi_g.Where(x => id_centro.Contains((int)x.id_centro_g) && x.folio.Contains(folio)).ToList();
            }
            else
            {
                requis = db.C_compras_requi_g.Where(x => id_centro.Contains((int)x.id_centro_g) && x.fecha_registro >= FechaInicial && x.fecha_registro <= FechaFinal &&
            id_status.Contains((int)x.id_status_requi) && id_tipo_requi.Contains((int)x.id_requisicion_tipo) && activo.Contains((bool)x.activo)).ToList();
            }


            return PartialView("../COMPRAS/REQUISICIONES/GENERAR_REQUI/_RequisicionInfo", requis);
        }
        public PartialViewResult ConsultarDetalleRequi(int id_requi_g, int firma)
        {
            var personal = (int)Session["LoggedId"];
            decimal monto_usuario = 0;
            var monto = db.C_firmas_usuarios_montos.Where(x => x.id_usuario_corporativo == personal).FirstOrDefault();
            if (monto == null) { monto_usuario = 0; }
            else { monto_usuario = Convert.ToDecimal(monto.montos); }
            ViewBag.monto_usuario = monto_usuario;
            ViewBag.firmas = 1;
            var autorizacion = 1;
            //Requisisciones detalle
            var detalle = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requi_g).ToList();
            //Firmas del usuario

            int id_tipo_requi = (int)detalle.FirstOrDefault().C_compras_requi_g.id_requisicion_tipo;
            if (id_tipo_requi == 3)
            {
                var valid = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == personal && x.id_usuario_master_accion == 2003 && x.activo == true).FirstOrDefault();
                if (valid == null)
                {
                    ViewBag.firmas = 5;
                    return PartialView("../COMPRAS/REQUISICIONES/_DetalleRequisicion", detalle);
                }
            }

            //SE DEJA EL TRU SIEMPRE PARA FUTURAS MODIFICACIONES EN LA AUT 1
            if (autorizacion == 1)
            {
                var ctas_usuario = catalogos.CuentasContableUsuariosID(personal);
                switch (firma)
                {
                    //REVISAR
                    case 1:
                        foreach (var requi_detalle in detalle)
                        {
                            requi_detalle.recotizado = true;
                            if (!ctas_usuario.Contains((int)requi_detalle.id_cuenta_contable_g))
                            {
                                requi_detalle.recotizado = false;
                                autorizacion = 0;
                                ViewBag.firmas = 6;
                            }
                        }
                        break;
                        //VALIDAR-AUTORIZAR

                        //---------PRODCESO QUE FIRMABA POR LOS CARGOS CONTABLES DEL EN BASE AL PERMISO C_firmas_cargo_permisos & C_firmas_usuarios
                        ////case 2:
                        ////    foreach (var requi_detalle in detalle)
                        ////    {
                        ////        var datos_firma = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == personal && x.id_firma_cargo_permiso == firma && x.id_cargo_contable_g == requi_detalle.id_cargo_contable).ToList();
                        ////        if (datos_firma.Count == 0)
                        ////        {
                        ////            autorizacion = 0;
                        ////        }
                        ////    }
                        ////    if (autorizacion == 1)
                        ////    {
                        ////        ViewBag.firmas = 2;
                        ////    }
                        ////    else
                        ////    {
                        ////        ViewBag.firmas = 0;
                        ////    }
                        ////    break;
                        //////VALIDAR-COMPRA
                        ////case 3:
                        ////    ViewBag.firmas = 3;
                        ////    break;
                        //////AUTORIZAR-COMPRA
                        ////case 4:
                        ////    ViewBag.firmas = 4;
                        ////    break;
                        ////default:
                        ////    ViewBag.firmas = 0;
                        ////    break;
                }
            }
            return PartialView("../COMPRAS/REQUISICIONES/_DetalleRequisicion", detalle);
        }

        public int ActualizarArticuloRequisicionAuth(int id_requi_d, decimal cantidad, string observacion, int id_cargo, int id_cuenta, int id_articulo)
        {
            try
            {
                var detalle = db.C_compras_requi_d.Find(id_requi_d);
                if (detalle.C_compras_requi_g.id_requisicion_tipo != 2) { if (detalle.C_compras_requi_g.aut_1_status != null) { return -1; } }
                if (detalle.C_compras_requi_g.activo == false) { return -2; }
                detalle.id_articulo = id_articulo;
                detalle.cantidad = cantidad;
                detalle.observacion = observacion.ToUpper();
                detalle.id_cargo_contable = id_cargo;
                detalle.id_cuenta_contable_g = id_cuenta;
                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }



        public PartialViewResult ConsultarDetalleRequiGeneral(int id_requi_g)
        {
            ViewBag.id_usuario = 0;
            try
            {
                ViewBag.id_usuario = (int)Session["LoggedId"];
            }
            catch (Exception)
            {
            }
            var detalle = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requi_g).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/GENERAR_REQUI/_DetallesRequisicion", detalle);
        }

        public PartialViewResult ConsultarResumenPresupuestoCuentaRequi(int id_requi)
        {
            ViewBag.no_firma = 1;
            try
            {
                List<ResumenPresupuestoCuentaRequi> resumen = new List<ResumenPresupuestoCuentaRequi>();

                var requi = db.C_compras_requi_g.Find(id_requi);
                int id_usuario = (int)Session["LoggedId"];
                var id_cuentas_auth = catalogos.CuentasContableUsuariosID(id_usuario);

                foreach (var item in requi.C_compras_requi_d.GroupBy(x => x.id_cuenta_contable_g))
                {
                    int id_cuenta = (int)item.FirstOrDefault().id_cuenta_contable_g;
                    //OBTENER PRESUPUESTO DEL MES EN EL QUE SE HIZO LA REQUI
                    decimal valor_presupuesto;
                    DateTime fecha_requi = (DateTime)requi.fecha_registro;
                    int mes = fecha_requi.Month;
                    int anio = db.C_presupuestos_anios.Where(x => x.anio == fecha_requi.Year.ToString()).FirstOrDefault().id_anio_presupuesto;
                    var pres = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_mes == mes && x.id_anio == anio && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();
                    if (pres == null) { valor_presupuesto = 0; }
                    else { valor_presupuesto = (decimal)pres.valor_presupuesto; }

                    //OBTENER EL VALOR EJERCIDO
                    decimal valor_ejercido = ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 1);
                    valor_ejercido += ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 39118);

                    //OBTENER EL TOTAL DE LA REQUI DE LA CUENTA
                    decimal valor_requi_cuenta = 0;
                    foreach (var articulo in item)
                    {
                        valor_requi_cuenta += (decimal)articulo.cantidad * (decimal)articulo.precio;
                    }

                    ResumenPresupuestoCuentaRequi info_cuenta = new ResumenPresupuestoCuentaRequi();
                    info_cuenta.Id_cuenta = id_cuenta;
                    info_cuenta.Cta_auth = false;
                    if (id_cuentas_auth.Contains(id_cuenta)) { info_cuenta.Cta_auth = true; }

                    info_cuenta.Id_requi_g = id_requi;
                    info_cuenta.Id_tipo_requisicion = (int)requi.id_requisicion_tipo;
                    info_cuenta.Cuenta = item.FirstOrDefault().C_cuentas_contables_g.cuenta;
                    info_cuenta.Nombre_cuenta = item.FirstOrDefault().C_cuentas_contables_g.nombre_cuenta;
                    info_cuenta.Presupuesto = valor_presupuesto;
                    info_cuenta.Ejercido = valor_ejercido;
                    info_cuenta.Proceso = 0;
                    info_cuenta.Disponible = valor_presupuesto - valor_ejercido;
                    info_cuenta.Requisicion = valor_requi_cuenta;
                    info_cuenta.Resultado = info_cuenta.Disponible - info_cuenta.Requisicion;
                    info_cuenta.Pendiente = 0;
                    info_cuenta.Diferencia = info_cuenta.Resultado - info_cuenta.Pendiente;
                    resumen.Add(info_cuenta);
                }

                return PartialView("../COMPRAS/REQUISICIONES/_ResumenPresupuestoCuentaRequi", resumen);
            }
            catch (Exception)
            {
                return PartialView("../COMPRAS/REQUISICIONES/_ResumenPresupuestoCuentaRequi", null);
            }
        }

        public decimal ObtenerMontoEjercidoCuentaMes(int anio, int mes, int id_cuenta, int id_moneda)
        {
            try
            {
                anio = Convert.ToInt32(db.C_presupuestos_anios.Find(anio).anio);
                DateTime fecha_inicio = new DateTime(anio, mes, 1);
                DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
                decimal ejercido = 0;
                try
                {
                    ejercido = (decimal)db.C_compras_ordenes_d.Where(x => x.C_compras_ordenes_g.activo == true && x.id_cuenta_contable == id_cuenta
                    && x.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g.solicita_autorizacion == false  //NO AUTORIZADAS POR DIRECCION
                    && x.C_compras_ordenes_g.fecha_registro >= fecha_inicio && x.C_compras_ordenes_g.fecha_registro <= fecha_fin && x.C_compras_ordenes_g.id_status_orden != 3  //3 = CANCELADA 
                    && x.activo == true && x.id_tipo_moneda == id_moneda && x.C_compras_ordenes_g.C_compras_requisiciones_tipos.id_requisicion_tipo != 3)  //3= INVERSION  
                        .Select(x => x.cantidad_compra * x.precio_unitario).Sum();

                    if (id_moneda == 39118)
                    {
                        if (ejercido > 0)
                        {
                            decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                            return valor_dolar * ejercido;
                        }
                    }

                }
                catch (Exception) { return ejercido; }
                return ejercido;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool AutorizarRechazarRequi(int id_requi_g, bool modo, int id_status_requi, string comentarios)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;

                var requi = db.C_compras_requi_g.Find(id_requi_g);

                if (id_status_requi == 2)
                {
                    //REVISION DE NECESIDAD
                    requi.aut_1_status = modo;
                    requi.aut_1_fecha = hoy;
                    requi.aut_1_usuario = id_usuario;



                    //AÑADIDO NUEVO 09-12-2023////
                    //VALIDA NECESIDAD
                    requi.aut_2_status = modo;
                    requi.aut_2_fecha = hoy;
                    requi.aut_2_usuario = id_usuario;
                    requi.aut_2_usuario = id_usuario;
                    //AUTORIZA NECESIDAD
                    requi.aut_3_status = modo;
                    requi.aut_3_fecha = hoy;
                    requi.aut_3_usuario = id_usuario;
                    requi.aut_3_usuario = id_usuario;
                    requi.comentarios_firma_1 = comentarios;
                    //////////////////////////////
                }
                else if (id_status_requi == 3)
                {
                    //VALIDA NECESIDAD
                    requi.aut_2_status = modo;
                    requi.aut_2_fecha = hoy;
                    requi.aut_2_usuario = id_usuario;
                    requi.aut_2_usuario = id_usuario;
                    //AUTORIZA NECESIDAD
                    requi.aut_3_status = modo;
                    requi.aut_3_fecha = hoy;
                    requi.aut_3_usuario = id_usuario;
                    requi.aut_3_usuario = id_usuario;
                    requi.comentarios_firma_1 = comentarios;
                }
                else if (id_status_requi == 5)
                {

                }


                if (modo == true)
                {
                    requi.id_status_requi = id_status_requi;
                }
                else
                {
                    requi.id_status_requi = id_status_requi;
                }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }



        public bool ValidarFirmaRequisicion(int id_requisicion, int firma, int[] cargos)
        {
            var datos_requi = db.C_compras_requi_g.Where(x => x.id_requisicion_articulo_g == id_requisicion).ToList();
            var personal = (int)Session["LoggedId"];
            var datos_firma = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == personal).ToList();
            bool validacion = true;
            int contador = 0;
            switch (firma)
            {
                case 1:
                    foreach (var firmas_autorizacion in datos_firma)
                    {
                        if (firmas_autorizacion.id_cargo_contable_g != cargos[contador])
                        {
                            validacion = false;
                        }
                        contador++;
                    }
                    break;
                case 2:
                    break;
            }
            return validacion;
        }

        #endregion


        #region//REQUISICION A VALIDAR-REVISAR
        public ActionResult RequisicionesValidarAutorizar()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4017)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/AUTORIZACIONES/ValAutorizar/Index");
        }

        public PartialViewResult ConsultarRequisicionesAutValidar(int[] id_centro, string requi)
        {
            List<C_compras_requi_g> requis = null;
            int id_usuario = (int)Session["LoggedId"];
            if (id_centro.Contains(0)) { id_centro = catalogos.ConsultarCentrosUsuarioID(id_usuario); }
            if (requi != "")
            {
                requis = db.C_compras_requi_g.Where(x => x.activo == true && x.id_status_requi == 2 && id_centro.Contains((int)x.id_centro_g) && x.folio == requi).OrderBy(x => x.fecha_registro).ToList();
            }
            else
            {
                requis = db.C_compras_requi_g.Where(x => x.activo == true && x.id_status_requi == 2 && id_centro.Contains((int)x.id_centro_g)).OrderBy(x => x.fecha_registro).ToList();
            }

            return PartialView("../COMPRAS/REQUISICIONES/AUTORIZACIONES/ValAutorizar/_RequisicionesAutValidar", requis);
        }


        #endregion


        #region FIRMAS REQUISICION
        public PartialViewResult ConsultarRequisiciones(int[] id_cargo, string requi, int status)
        {
            IQueryable<C_compras_requi_g> requis = null;
            int id_usuario = (int)Session["LoggedId"];
            //Session["usuario_firmas"] = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == id_usuario).ToList();
            ViewBag.usuario_firmas = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == id_usuario).ToList();
            if (id_cargo.Contains(0)) { id_cargo = catalogos.CargoContableUsuariosID(id_usuario); }

            var ctas_usuario = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario && x.activo == true
                                    && id_cargo.Contains((int)x.C_cuentas_contables_g.id_cargo_contable)).Select(x => (int)x.id_cuenta_contable_g).ToArray();

            switch (status)
            {
                //REQUISICION GENERADA - PENDIENTE REVISAR
                case 1:
                    if (requi != "")
                    {
                        //requis = db.C_compras_requi_g.Where(x => x.id_status_requi == 1 && x.activo == true && id_centro.Contains((int)x.id_centro_g) && x.folio == requi).OrderBy(x => x.fecha_registro).ToList();
                        requis = from requi_g in db.C_compras_requi_g
                                 join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                                 where requi_g.id_status_requi == 1 && requi_g.activo == true && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g) && requi_g.folio == requi
                                 orderby requi_g.fecha_registro
                                 select requi_g;
                    }
                    else
                    {
                        requis = from requi_g in db.C_compras_requi_g
                                 join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                                 where requi_g.id_status_requi == 1 && requi_g.activo == true && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g)
                                 orderby requi_g.fecha_registro
                                 select requi_g;
                    }

                    return PartialView("../COMPRAS/REQUISICIONES/AUTORIZACIONES/AutRevision/_RequisicionesRevision", requis.Distinct());
                //REQUISICION REVISADA - PENDIETE VALIDAR Y AUTORIZAR
                case 2:
                    if (requi != "")
                    {
                        //requis = db.C_compras_requi_g.Where(x => x.activo == true && x.id_status_requi == 2 && id_centro.Contains((int)x.id_centro_g) && x.folio == requi).OrderBy(x => x.fecha_registro).ToList();
                        requis = from requi_g in db.C_compras_requi_g
                                 join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                                 where requi_g.id_status_requi == 2 && requi_g.activo == true && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g) && requi_g.folio == requi
                                 orderby requi_g.fecha_registro
                                 select requi_g;
                    }
                    else
                    {
                        //requis = db.C_compras_requi_g.Where(x => x.activo == true && x.id_status_requi == 2 && id_centro.Contains((int)x.id_centro_g)).OrderBy(x => x.fecha_registro).ToList();
                        requis = from requi_g in db.C_compras_requi_g
                                 join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                                 where requi_g.id_status_requi == 1 && requi_g.activo == true && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g)
                                 orderby requi_g.fecha_registro
                                 select requi_g;
                    }
                    return PartialView("../COMPRAS/REQUISICIONES/AUTORIZACIONES/ValAutorizar/_RequisicionesAutValidar", requis.Distinct());
                default:
                    return null;
            }
        }
        #endregion


        #region COTIZACION DE ARTICULOS REQUISICION
        public ActionResult CotizacionArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(5018)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/COTIZACION/Index");
        }

        public PartialViewResult ConsultarRequiCotizacion(int[] id_centro)
        {
            List<C_compras_requi_g> requis = null;
            requis = db.C_compras_requi_g.Where(x => x.activo == true && x.id_status_requi == 2 && id_centro.Contains((int)x.id_centro_g)).OrderBy(x => x.fecha_registro).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/AUTORIZACIONES/ValAutorizar/_RequisicionesAutValidar", requis);
        }
        #endregion





        #region //------------------REQUI CON ARTICULO COTIZAR
        public PartialViewResult ConsultarArticulosRequi(int[] centro, int[] clasificacion)
        {
            int id_usuario = (int)Session["LoggedId"];
            if (centro.Contains(0))
            {
                centro = db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => (int)x.id_centro).ToArray();
            }
            if (clasificacion.Contains(0))
            {
                clasificacion = db.C_articulos_clasificaciones.Where(x => x.activo == true).Select(x => (int)x.id_articulo_clasificacion).ToArray();
            }
            var requis = from requig in db.C_compras_requi_g
                         join requid in db.C_compras_requi_d on requig.id_requisicion_articulo_g equals requid.id_requisicion_articulo_g
                         where clasificacion.Contains((int)requid.C_articulos_catalogo.id_articulo_clasificacion) && requig.activo == true
                         && requig.aut_3_status == true && requig.aut_4_status == null && centro.Contains((int)requig.id_centro_g) && requid.cotizado != true &&
                         requig.id_requisicion_tipo != 2
                         select requig;
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_ArticulosCotizar", requis.Distinct());
        }
        //REQUID MODAL ARTICULOS
        public PartialViewResult ConsultarArticulosPorRequi(int id_requisicion)
        {
            var requis = from requid in db.C_compras_requi_d
                         where requid.id_requisicion_articulo_g == id_requisicion
                         select requid;
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_ArticulosPorCotizar", requis);
        }
        //REQUID ARTICULOS
        public PartialViewResult ConsultarArticulosCoti(int requid, string articulo)
        {
            var requis = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == requid && x.C_articulos_catalogo.clave == articulo && x.fecha_confirmacion == null && x.activo == true).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_ArticulosRequid", requis);
        }
        //REQUID ARTICULOS COTIZACIONES
        public PartialViewResult ConsultarArticulosHist(string id_articulo)
        {
            var requis = from requid in db.C_compras_cotizaciones_articulos
                         where requid.C_articulos_catalogo.clave == id_articulo && requid.fecha_vigencia > DateTime.Now
                         select requid;
            ViewBag.articulo = from articulos in db.C_articulos_catalogo where articulos.clave == id_articulo select new { articulos.nombre_articulo };
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_HistorialCotizacion", requis.OrderByDescending(x => x.fecha_cotizacion));
        }
        //REQUID ARTICULOS INFORMACION
        public PartialViewResult ConsultarArticulosInf(int requid)
        {
            var requis = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_d == requid).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_ArticuloProveedor", requis);
        }
        //REGISTRAR COTIZACION ARTICULO
        public bool RegistrarCotizacionArticulo(C_compras_cotizaciones_articulos c_requi_coti, C_compras_cotizaciones_requisiciones c_coti_requi, int id_requi_g, string fecha_vigencia)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_vig = DateTime.Parse(fecha_vigencia).AddHours(23).AddHours(59);

            c_requi_coti.id_usuario_cotiza = id_usuario;
            c_requi_coti.activo = true;
            c_requi_coti.fecha_vigencia = fecha_vig;
            c_requi_coti.fecha_cotizacion = DateTime.Now;
            c_requi_coti.fecha_actualizacion = DateTime.Now;
            db.C_compras_cotizaciones_articulos.Add(c_requi_coti);
            db.SaveChanges();
            var id_coti = c_requi_coti.id_compra_articulo_cotizacion;


            int requisicion_id = id_requi_g;
            c_coti_requi.id_compra_articulo_cotizacion = id_coti;
            c_coti_requi.id_requisicion_articulo_g = requisicion_id;
            c_coti_requi.activo = true;
            c_coti_requi.confirmada = false;
            c_coti_requi.fecha_registro = DateTime.Now;
            c_coti_requi.id_usuario_registra = id_usuario;
            c_coti_requi.cantidad_surtir = 0;
            c_coti_requi.agrupada = false;
            db.C_compras_cotizaciones_requisiciones.Add(c_coti_requi);
            db.SaveChanges();
            CambiarStatusRequisicion(id_requi_g, 5);
            return true;
        }

        public bool AsignarCotizacionArticulo(C_compras_cotizaciones_requisiciones c_coti_requi, int requid)
        {
            int id_usuario = (int)Session["LoggedId"];

            var requisd = db.C_compras_requi_d.Find(requid);
            var coti_articulo = db.C_compras_cotizaciones_articulos.Find(c_coti_requi.id_compra_articulo_cotizacion);
            c_coti_requi.id_requisicion_articulo_g = requisd.id_requisicion_articulo_g;
            c_coti_requi.cantidad_surtir = 0;
            c_coti_requi.dias_entrega = coti_articulo.dias_entrega;
            c_coti_requi.fecha_registro = DateTime.Now;
            c_coti_requi.comentarios = "Ninguno";
            c_coti_requi.confirmada = false;
            c_coti_requi.id_usuario_registra = id_usuario;
            c_coti_requi.id_articulo = coti_articulo.id_articulo;
            c_coti_requi.activo = true;
            c_coti_requi.id_requisicion_articulo_d = requid;
            c_coti_requi.agrupada = false;

            db.C_compras_cotizaciones_requisiciones.Add(c_coti_requi);
            db.SaveChanges();
            CambiarStatusRequisicion((int)requisd.id_requisicion_articulo_g, 5);
            return true;
        }

        public int VerificarCotizacionVR(int requid, int id_cotizacion, int articulo, int proveedor)
        {
            if (id_cotizacion == 0)
            {
                try
                {
                    var valid = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == requid && x.activo == true && x.C_compras_cotizaciones_articulos.id_compras_proveedor == proveedor && x.fecha_confirmacion == null)
                        .Select(x => x.C_compras_cotizaciones_articulos.id_compras_proveedor).Distinct().Count();

                    //if (valid > 0) { 
                    //    return 0; 
                    //}
                    //else { 
                    //    return 1; 
                    //}
                    return 1;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
            else
            {
                try
                {
                    //var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => 
                    //x.id_requisicion_articulo_d == requid && x.id_articulo == articulo && x.activo == true && x.C_compras_cotizaciones_articulos.id_compras_proveedor == proveedor && x.fecha_confirmacion==null).ToList();
                    //if (cotizaciones.Count > 0)
                    //{
                    //    return 0;
                    //}
                    //else
                    //{
                    //    return 1;
                    //}
                    return 1;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public bool RemoverArticuloCotizacion(int cotizacion)
        {
            var cotizaciones = db.C_compras_cotizaciones_requisiciones.Find(cotizacion);
            cotizaciones.activo = false;
            db.SaveChanges();
            return true;
        }

        public bool LimiteCotizacion(int requid, int articulo)
        {
            try
            {
                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == requid && x.id_articulo == articulo && x.activo == true && x.fecha_confirmacion == null).ToList();
                if (cotizaciones.Count < 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ModificarVigenciaProveedor(int cotizacion)
        {
            var requis = db.C_compras_cotizaciones_articulos.Where(x => x.id_compra_articulo_cotizacion == cotizacion);
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACION/_VigenciaProveedor", requis);
        }
        public bool ActualizarVigenciaProveedor(int cotizacion, DateTime fecha)
        {
            var requis = db.C_compras_cotizaciones_articulos.Find(cotizacion);
            requis.fecha_vigencia = fecha;
            requis.fecha_actualizacion = DateTime.Now;
            db.SaveChanges();
            return true;
        }

        #endregion


        #region//-------------CONFIRMACION DE COTIZACIONES DE COMPRA
        public ActionResult ConfirmacionCotizacionesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(5021)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/CONFIRMACION_COTIZACIONES/Index");
        }

        public PartialViewResult ConsultarArticulosCotizadosPendientesConfirmacion(int[] id_clasificaciones, string folio_requi)
        {
            IQueryable<C_compras_cotizaciones_requisiciones> articulos_cotizados = null;
            try
            {
                if (id_clasificaciones.Contains(0)) { id_clasificaciones = db.C_articulos_clasificaciones.Where(x => x.activo == true).Select(x => x.id_articulo_clasificacion).ToArray(); }

                if (folio_requi != "")
                {
                    //articulos_cotizados = db.C_compras_cotizaciones_requisiciones.Where(x => x.C_compras_requi_g.folio == folio_requi && x.confirmada == false && x.activo == true);

                    articulos_cotizados = from req_d in db.C_compras_requi_d
                                          join req_g in db.C_compras_requi_g on req_d.id_requisicion_articulo_g equals req_g.id_requisicion_articulo_g
                                          join req in db.C_compras_cotizaciones_requisiciones on req_d.id_requisicion_articulo_d equals req.id_requisicion_articulo_d
                                          join art in db.C_articulos_catalogo on req.id_articulo equals art.id_articulo

                                          where /*req.confirmada == false &&*/ id_clasificaciones.Contains((int)art.id_articulo_clasificacion)
                                          && req.activo == true && req_d.cotizado == null && req_g.activo == true && req.fecha_confirmacion == null
                                          && req.C_compras_requi_g.folio == folio_requi
                                          select req;
                }
                else
                {
                    articulos_cotizados = from req_d in db.C_compras_requi_d
                                          join req_g in db.C_compras_requi_g on req_d.id_requisicion_articulo_g equals req_g.id_requisicion_articulo_g
                                          join req in db.C_compras_cotizaciones_requisiciones on req_d.id_requisicion_articulo_d equals req.id_requisicion_articulo_d
                                          join art in db.C_articulos_catalogo on req.id_articulo equals art.id_articulo

                                          where /*req.confirmada == false &&*/ id_clasificaciones.Contains((int)art.id_articulo_clasificacion)
                                          && req.activo == true && req_d.cotizado == null && req_g.activo == true && req.fecha_confirmacion == null
                                          select req;
                }
            }
            catch (Exception)
            {
                articulos_cotizados = null;
            }

            return PartialView("../COMPRAS/REQUISICIONES/CONFIRMACION_COTIZACIONES/_ArticulosCotizadosPendientes", articulos_cotizados);
        }

        public PartialViewResult ConsultarCotizacionesArticuloRequi(int id_cotizacion_d)
        {
            IEnumerable<C_compras_cotizaciones_requisiciones> cotizaciones = null;
            try
            {
                cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == id_cotizacion_d && x.activo == true && x.fecha_confirmacion == null && x.C_compras_requi_g.activo == true).ToList();
            }
            catch (Exception)
            {
                cotizaciones = null;
            }
            return PartialView("../COMPRAS/REQUISICIONES/CONFIRMACION_COTIZACIONES/_CotizacionesArticuloRequi", cotizaciones);
        }

        public int GuardarConfirmacionCotizacionArticuloRequisicion(int[] id_cotizaciones_articulo, decimal[] cant_surtir, int[] dias_entrega, string[] comentarios, bool[] confirmada, int id_requi_g, int id_requi_d, decimal total_cantidad)
        {
            try
            {
                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_d == id_requi_d && x.activo == true && x.agrupada == false && x.fecha_confirmacion == null).ToList();

                int id_usuario = (int)Session["LoggedId"];
                DateTime Hoy = DateTime.Now;

                for (int i = 0; i < id_cotizaciones_articulo.Count(); i++)
                {
                    int id_cotizacion = id_cotizaciones_articulo[i];
                    var cotizacion_articulo = db.C_compras_cotizaciones_requisiciones.Find(id_cotizacion);
                    bool confirmacion = confirmada[i];
                    if (confirmacion == false)
                    {
                        cotizacion_articulo.confirmada = confirmacion;
                    }
                    else
                    {
                        cotizacion_articulo.confirmada = true;
                        cotizacion_articulo.cantidad_surtir = cant_surtir[i];
                        cotizacion_articulo.dias_entrega = dias_entrega[i];
                        cotizacion_articulo.comentarios = comentarios[i];
                    }
                    cotizacion_articulo.fecha_confirmacion = Hoy;
                    cotizacion_articulo.id_usuario_actualizo = id_usuario;

                    db.SaveChanges();
                }
                var articulo_requi = db.C_compras_requi_d.Find(id_requi_d);
                articulo_requi.cantidad_cotizada = articulo_requi.cantidad_cotizada + total_cantidad;
                if (articulo_requi.cantidad == articulo_requi.cantidad_cotizada)
                {
                    articulo_requi.cotizado = true;
                    db.SaveChanges();
                }
                else
                {
                    foreach (var item in cotizaciones)
                    {
                        C_compras_cotizaciones_requisiciones coti = new C_compras_cotizaciones_requisiciones();
                        coti.id_requisicion_articulo_g = item.id_requisicion_articulo_g;
                        coti.id_requisicion_articulo_d = item.id_requisicion_articulo_d;
                        coti.id_compra_articulo_cotizacion = item.id_compra_articulo_cotizacion;
                        coti.dias_entrega = item.dias_entrega;
                        coti.fecha_registro = item.fecha_registro;
                        coti.id_articulo = item.id_articulo;
                        coti.confirmada = false;
                        coti.activo = item.activo;
                        coti.dias_entrega = item.dias_entrega;
                        coti.id_usuario_registra = item.id_usuario_registra;
                        coti.agrupada = item.agrupada;
                        db.C_compras_cotizaciones_requisiciones.Add(coti);
                        db.SaveChanges();
                    }
                    //db.SaveChanges();
                }

                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }


        }

        public int CerrarCotiRequi(int id_requi)
        {
            try
            {
                var articulo_requi = db.C_compras_requi_d.Find(id_requi);
                articulo_requi.cotizado = true;
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }


        }



        #endregion


        #region//------------ PARCIALIDADES
        public ActionResult RequisicionesConfirmadas()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6021)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/COTIZACIONES_CONFIRMADAS/Index");
        }

        public PartialViewResult ConsultarRequisicionesConfirmadas(int[] id_centros)
        {
            IQueryable<C_compras_requi_g> cotizaciones_pendientes = null;
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                if (id_centros.Contains(0)) { id_centros = catalogos.ConsultarCentrosUsuarioID(id_usuario); }
                cotizaciones_pendientes = from req_d in db.C_compras_requi_d
                                          join req_g in db.C_compras_requi_g on req_d.id_requisicion_articulo_g equals req_g.id_requisicion_articulo_g
                                          join req in db.C_compras_cotizaciones_requisiciones on req_d.id_requisicion_articulo_d equals req.id_requisicion_articulo_d

                                          where id_centros.Contains((int)req_g.id_centro_g) && req_g.activo == true
                                          && req.activo == true && req.confirmada == true && req.agrupada == false //&& req.fecha_confirmacion != null
                                          select req_g;
            }
            catch (Exception)
            {
                cotizaciones_pendientes = null;
            }

            return PartialView("../COMPRAS/REQUISICIONES/COTIZACIONES_CONFIRMADAS/_CotizacionesPendientesConfirmar", cotizaciones_pendientes);
        }

        public PartialViewResult ConsultarCotizacionesConfirmadas(int id_requi_g)
        {
            var cotizaciones_confirmadas = db.C_compras_cotizaciones_requisiciones.Where(x => x.C_compras_requi_g.activo == true && x.confirmada == true && x.fecha_confirmacion != null && x.activo == true
            && x.agrupada == false && x.id_requisicion_articulo_g == id_requi_g).ToList();
            if (cotizaciones_confirmadas.Count() == 0)
            {
                return null;
            }

            return PartialView("../COMPRAS/REQUISICIONES/COTIZACIONES_CONFIRMADAS/_CotizacionesConfirmadas", cotizaciones_confirmadas);
        }

        public bool EliminarCotizacionConfirmada(int id_requisicion_cotizacion)
        {
            try
            {
                var confirmacion = db.C_compras_cotizaciones_requisiciones.Find(id_requisicion_cotizacion);
                int id_requi_d = (int)confirmacion.id_requisicion_articulo_d;
                decimal cantidad_cotizada = (decimal)confirmacion.cantidad_surtir;

                var requi_d = db.C_compras_requi_d.Find(id_requi_d);

                confirmacion.activo = false;
                confirmacion.confirmada = false;

                requi_d.cantidad_cotizada = requi_d.cantidad_cotizada - cantidad_cotizada;
                requi_d.cotizado = null;

                db.SaveChanges();
                CambiarStatusRequisicion((int)requi_d.id_requisicion_articulo_g, 5);
                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        public string ConsultarLugaresEntregaString(int id_ubicacion_entrega_g)
        {
            try
            {
                var lugares = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_compras_ubicacion_entrega == id_ubicacion_entrega_g && x.activo == true).Select(x => x.C_almacen_almacenes_g.nombre_almacen).Distinct().ToList();
                if (lugares.Count() > 0) { return JsonConvert.SerializeObject(lugares); }
                return "Esta ubicación no tiene un almacén ligado, se recepcionará en oficina/establo";
            }
            catch (Exception)
            {
                return "Error el consultar los lugares de recepción";
            }
        }


        public int GeneracionSolicitudCompra(string nombre_solicitud, int[] id_cotizaciones, int id_requi_g, string correo_proveedor, int id_ubicacion_entrega)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var cotizaciones_requi = db.C_compras_cotizaciones_requisiciones.Where(x => x.activo == true && id_cotizaciones.Contains((int)x.id_cotizacion_requisicion)).Select(x => x.id_compra_articulo_cotizacion).ToArray();
                var id_proveedores = db.C_compras_cotizaciones_articulos.Where(x => cotizaciones_requi.Contains((int)x.id_compra_articulo_cotizacion) && x.activo == true).Select(x => x.id_compras_proveedor).Distinct();
                if (id_proveedores.Count() > 1) { return 1; }
                else
                {
                    foreach (var prov in id_proveedores)
                    {
                        try
                        {
                            var valid_cuenta_proveedor = db.C_compras_proveedores.Find(prov);
                            if (valid_cuenta_proveedor.cuenta_cxp == null || valid_cuenta_proveedor.cuenta_cxp == "" || valid_cuenta_proveedor.cuenta_cxp.Length <= 4)
                            {
                                return 2;
                            }
                        }
                        catch (Exception)
                        {
                            return 3;
                        }
                    }
                }


                C_compras_cotizaciones_confirmadas_g confirmacion_g = new C_compras_cotizaciones_confirmadas_g();
                confirmacion_g.nombre_suborden = nombre_solicitud.ToUpper();
                confirmacion_g.activo = true;
                confirmacion_g.id_requisicion_articulo_g = id_requi_g;
                confirmacion_g.confirmada = true;
                confirmacion_g.id_usuario_registra = id_usuario;
                confirmacion_g.fecha_registro = DateTime.Now;
                confirmacion_g.solicita_autorizacion = false;
                confirmacion_g.id_status_cotizacion_confirmada = 1; //PENDIENTE DE FIRMAR
                confirmacion_g.id_compras_ubicacion_entrega = id_ubicacion_entrega;
                confirmacion_g.correo_proveedor_suborden = correo_proveedor;
                db.C_compras_cotizaciones_confirmadas_g.Add(confirmacion_g);
                db.SaveChanges();

                int id_confirmacion_g = confirmacion_g.id_cotizacion_confirmada_g;
                for (int i = 0; i < id_cotizaciones.Length; i++)
                {
                    int id_cotizacion_requi = id_cotizaciones[i];
                    C_compras_cotizaciones_confirmadas_d confirmadas_d = new C_compras_cotizaciones_confirmadas_d();
                    confirmadas_d.id_cotizacion_confirmada_g = id_confirmacion_g;
                    confirmadas_d.activo = true;
                    confirmadas_d.id_cotizacion_requisicion = id_cotizacion_requi;
                    db.C_compras_cotizaciones_confirmadas_d.Add(confirmadas_d);
                    var coti = db.C_compras_cotizaciones_requisiciones.Find(id_cotizacion_requi);
                    coti.agrupada = true;
                    db.SaveChanges();
                }
                CambiarStatusRequisicion(id_requi_g, 6);
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public PartialViewResult ConsultarSolicitudesCompraAgrupadas(int id_requi_g)
        {
            //var solicitudes_compra = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requi_g && x.activo == true).ToList();
            var solicitudes_compra = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.C_compras_cotizaciones_confirmadas_g.id_requisicion_articulo_g == id_requi_g
            && x.activo == true && x.C_compras_cotizaciones_confirmadas_g.activo == true).OrderBy(x => x.C_compras_cotizaciones_confirmadas_g.id_status_cotizacion_confirmada).ToList();

            return PartialView("../COMPRAS/REQUISICIONES/COTIZACIONES_CONFIRMADAS/_SolicitudesCompraAgrupadas", solicitudes_compra);
        }

        public PartialViewResult ConsultarSolicitudCompraAgrupadaDetalle(int id_cotizacion_confirmada_g)
        {
            var solicitudes_compra = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == id_cotizacion_confirmada_g
            && x.activo == true && x.C_compras_cotizaciones_confirmadas_g.activo == true).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/COTIZACIONES_CONFIRMADAS/_SolicitudCompraAgrupadaDetalle", solicitudes_compra);
        }

        public bool EliminarSolicitudCompra(int[] id_cotizacion_confirmada_g)
        {
            try
            {
                for (int i = 0; i < id_cotizacion_confirmada_g.Length; i++)
                {
                    int id_confirmacion_g = id_cotizacion_confirmada_g[i];

                    var valid = db.C_compras_cotizaciones_confirmadas_g.Find(id_confirmacion_g);
                    if (valid.id_status_cotizacion_confirmada == 2 /*|| valid.solicita_autorizacion == true*/) { return false; }

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


        #endregion


        #region ORDEN DE COMPRA TOTAL
        public ActionResult OrdenesCompraTotal()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(5021)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/ORDENES/ConfirmacionMaster");
        }




        public PartialViewResult ConsultarAgrupamientosGeneral()
        {
            var agrupamientos = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/ORDENES/_RequisicionesAutorizar", agrupamientos);
        }
        public PartialViewResult ConsultarAgrupamientosEspecifico(int requi_g)
        {
            var agrupamientos = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.activo == true && x.id_requisicion_articulo_g == requi_g).ToList();
            return PartialView("../COMPRAS/REQUISICIONES/ORDENES/_DetalleRequisiciones", agrupamientos);
        }

        #endregion



        #region FIRMA AUTORIZACION FINAL
        public PartialViewResult ConsultarRequisicionesAutFinal(int modo, int[] id_cargo)
        {
            ViewBag.modo = modo;
            int id_usuario = (int)Session["LoggedId"];
            if (id_cargo.Contains(0)) { id_cargo = catalogos.CargoContableUsuariosID(id_usuario); }

            var ctas_usuario = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario && x.activo == true 
                                    && id_cargo.Contains((int)x.C_cuentas_contables_g.id_cargo_contable)).Select(x => (int)x.id_cuenta_contable_g).ToArray();

            IQueryable<C_compras_requi_g> requis = null;
            if (modo == 1 || modo == 4)  //1: USUARIO NORMAL    4: USUARIO ESPECIAL QUE FUNGE COMO MASTER Y NORMAL AFECTANDO PTTO (ADOLFO)
            {
                requis = from requi_g in db.C_compras_requi_g
                         join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                         join cotizaciones in db.C_compras_cotizaciones_requisiciones on requi_d.id_requisicion_articulo_d equals cotizaciones.id_requisicion_articulo_d
                         join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on requi_g.id_requisicion_articulo_g equals agrupadas_g.id_requisicion_articulo_g
                         join agrupadas_d in db.C_compras_cotizaciones_confirmadas_d on cotizaciones.id_cotizacion_requisicion equals agrupadas_d.id_cotizacion_requisicion
                         where requi_g.activo == true && cotizaciones.activo == true && cotizaciones.activo == true && agrupadas_g.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1
                         && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g) && agrupadas_g.solicita_autorizacion == false
                         select requi_g;
            }
            else if (modo == 3)  //USUARIO QUE FUNGE COMO MASTER Y NORMAL SIN AFECTAR PTTO (HORACIO)
            {
                requis = from requi_g in db.C_compras_requi_g
                         join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                         join cotizaciones in db.C_compras_cotizaciones_requisiciones on requi_d.id_requisicion_articulo_d equals cotizaciones.id_requisicion_articulo_d
                         join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on requi_g.id_requisicion_articulo_g equals agrupadas_g.id_requisicion_articulo_g
                         join agrupadas_d in db.C_compras_cotizaciones_confirmadas_d on cotizaciones.id_cotizacion_requisicion equals agrupadas_d.id_cotizacion_requisicion
                         where requi_g.activo == true && cotizaciones.activo == true && cotizaciones.activo == true && agrupadas_g.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1
                         && ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g) //&& agrupadas_g.solicita_autorizacion == false
                         select requi_g;
            }
            else //USUARIO MASTER
            {
                requis = from requi_g in db.C_compras_requi_g
                         join requi_d in db.C_compras_requi_d on requi_g.id_requisicion_articulo_g equals requi_d.id_requisicion_articulo_g
                         join cotizaciones in db.C_compras_cotizaciones_requisiciones on requi_d.id_requisicion_articulo_d equals cotizaciones.id_requisicion_articulo_d
                         join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on requi_g.id_requisicion_articulo_g equals agrupadas_g.id_requisicion_articulo_g
                         join agrupadas_d in db.C_compras_cotizaciones_confirmadas_d on cotizaciones.id_cotizacion_requisicion equals agrupadas_d.id_cotizacion_requisicion
                         where requi_g.activo == true && cotizaciones.activo == true && cotizaciones.activo == true && agrupadas_g.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1
                         /*&& ctas_usuario.Contains((int)requi_d.id_cuenta_contable_g)*/ && agrupadas_g.solicita_autorizacion == true
                         select requi_g;
            }

            return PartialView("../COMPRAS/REQUISICIONES/FIRMAS/AutFinal/_RequisicionesPorFirmar", requis.Distinct());
        }

        public PartialViewResult ConsultarRequisicionesAutFinalDetalle(int id_requi_g, int modo)
        {
            var personal = (int)Session["LoggedId"];
            var monto_usuario = db.C_firmas_usuarios_montos.Where(x => x.id_usuario_corporativo == personal).FirstOrDefault();
            if (monto_usuario == null) { ViewBag.monto_usuario = 0; }
            else { ViewBag.monto_usuario = monto_usuario.montos; }

            ViewBag.valor_dolar = "1.00";
            var info_requi = db.C_compras_requi_g.Find(id_requi_g);
            //ViewBag.id_tipo_requi = info_requi.id_requisicion_tipo;

            ViewBag.valid_firmar = true;
            if (info_requi.id_requisicion_tipo == 3)  //INVERSION
            {
                var valid_firmar = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == personal && x.id_usuario_master_accion == 2003 && x.activo == true).FirstOrDefault();
                if (valid_firmar != null) { ViewBag.valid_firmar = true; }
                else { ViewBag.valid_firmar = false; }
            }

            ViewBag.modo = modo;
            List<bool> solicita_aut = new List<bool>();
            if (modo == 1 || modo == 4)  //4: USUARIO QUE FUNGE CON AMBOS PERMISOS DE FIRMA AFECTANDO AL PPTO
            {
                solicita_aut.Add(false);
            }
            else if (modo == 2) {  //USUARIO MASTER
                solicita_aut.Add(true); 
            }
            else              //USUARIO QUE FUNGE CON AMBOS PERMISOS DE FIRMA SIN AFECTAR PPTO
            {
                solicita_aut.Add(true);
                solicita_aut.Add(false);
            }

            var detalle = from requi_d in db.C_compras_requi_d
                          join cotizaciones in db.C_compras_cotizaciones_requisiciones on requi_d.id_requisicion_articulo_d equals cotizaciones.id_requisicion_articulo_d
                          join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on id_requi_g equals agrupadas_g.id_requisicion_articulo_g
                          join agrupadas_d in db.C_compras_cotizaciones_confirmadas_d on agrupadas_g.id_cotizacion_confirmada_g equals agrupadas_d.id_cotizacion_confirmada_g
                          where agrupadas_g.activo == true && cotizaciones.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1 && cotizaciones.id_requisicion_articulo_g == id_requi_g
                          && cotizaciones.fecha_confirmacion != null && cotizaciones.confirmada == true && agrupadas_g.confirmada == true && cotizaciones.agrupada == true && cotizaciones.orden_generada == null
                          && solicita_aut.Contains((bool)agrupadas_g.solicita_autorizacion)
                          select requi_d;

            int count_detalle = detalle.Distinct().Count();
            foreach (var articulo in detalle.Distinct())
            {
                decimal total_articulo = 0;
                int id_requi_d = (int)articulo.id_requisicion_articulo_d;

                var confirmaciones_g = from cotis in db.C_compras_cotizaciones_requisiciones
                                   join confirmadas_g in db.C_compras_cotizaciones_confirmadas_g on cotis.id_requisicion_articulo_g equals confirmadas_g.id_requisicion_articulo_g
                                   
                                   where cotis.activo == true && confirmadas_g.activo == true && confirmadas_g.id_status_cotizacion_confirmada == 1
                                   && cotis.fecha_confirmacion != null && cotis.confirmada == true && cotis.id_requisicion_articulo_d == id_requi_d 
                                   && confirmadas_g.confirmada == true && cotis.agrupada == true && cotis.orden_generada == null 
                                   && cotis.id_requisicion_articulo_g == id_requi_g && confirmadas_g.id_requisicion_articulo_g == id_requi_g
                                   && solicita_aut.Contains((bool)confirmadas_g.solicita_autorizacion)
                                   //select cotis;
                                   select confirmadas_g;

                int count_coti = confirmaciones_g.Distinct().Count();
                foreach (var confirm_g in confirmaciones_g.Distinct())
                {
                    int id_confirmacion_g = (int)confirm_g.id_cotizacion_confirmada_g;
                    var confirm_d = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == id_confirmacion_g && x.activo == true && x.C_compras_cotizaciones_requisiciones.id_requisicion_articulo_d == id_requi_d).ToList();

                    foreach (var confirmacion_d in confirm_d.Distinct())
                    {
                        decimal cantidad_surtir = (decimal)confirmacion_d.C_compras_cotizaciones_requisiciones.cantidad_surtir;
                        decimal precio_final = (decimal)confirmacion_d.C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.precio_final;

                        try
                        {
                            int id_moneda = (int)confirmacion_d.C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.id_tipo_moneda;
                            if (id_moneda != 1)
                            {
                                decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                                precio_final = (decimal)confirmacion_d.C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.precio_final * valor_dolar;
                                ViewBag.valor_dolar = valor_dolar;
                            }
                        }
                        catch (Exception){ }
                        total_articulo += cantidad_surtir * precio_final;
                    }
                }
                var item = detalle.Where(x => x.id_requisicion_articulo_d == id_requi_d).FirstOrDefault();
                item.precio = total_articulo;
            }

            return PartialView("../COMPRAS/REQUISICIONES/FIRMAS/AutFinal/_RequisicionesPorFirmarDetalle", detalle.Distinct());
        }

        public PartialViewResult ConsultarResumenPresupuestoCuentasAutFinal(int id_requi)
        {
            ViewBag.no_firma = 2;
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                decimal total_disponibles = 0;
                decimal total_requi = 0;

                List<ResumenPresupuestoCuentaRequi> resumen = new List<ResumenPresupuestoCuentaRequi>();
                var requi = db.C_compras_requi_g.Find(id_requi);

                decimal porcentaje_tolerancia = 0;
                try { porcentaje_tolerancia = (decimal)db.C_parametros_configuracion.Find(15).valor_numerico; }
                catch (Exception) { }

                var id_cuentas_auth = catalogos.CuentasContableUsuariosID(id_usuario);

                var detalle = from agrupadas_d in db.C_compras_cotizaciones_confirmadas_d
                              join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on agrupadas_d.id_cotizacion_confirmada_g equals agrupadas_g.id_cotizacion_confirmada_g
                              join cotizaciones in db.C_compras_cotizaciones_requisiciones on agrupadas_d.id_cotizacion_requisicion equals cotizaciones.id_cotizacion_requisicion
                              where agrupadas_d.activo == true && agrupadas_g.activo == true && cotizaciones.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1 && cotizaciones.orden_generada == null
                              && cotizaciones.confirmada == true && cotizaciones.fecha_confirmacion != null && cotizaciones.id_requisicion_articulo_g == id_requi
                              select cotizaciones.C_compras_requi_d;

                foreach (var item in detalle.Distinct().GroupBy(x => x.id_cuenta_contable_g))
                {
                    ResumenPresupuestoCuentaRequi info_cuenta = new ResumenPresupuestoCuentaRequi();

                    int id_cuenta = (int)item.FirstOrDefault().id_cuenta_contable_g;
                    info_cuenta.Id_cuenta = id_cuenta;
                    //VALIDA QUE EL USUARIO TENGA ASIGNADA LA CUENTA CONTABLE
                    info_cuenta.Cta_auth = false;
                    if (id_cuentas_auth.Contains(id_cuenta)) { info_cuenta.Cta_auth = true; }

                    //OBTENER PRESUPUESTO DEL MES EN EL QUE SE HIZO LA REQUI
                    decimal valor_presupuesto;
                    DateTime fecha_requi = (DateTime)requi.fecha_registro;
                    int mes = fecha_requi.Month;
                    int anio = db.C_presupuestos_anios.Where(x => x.anio == fecha_requi.Year.ToString()).FirstOrDefault().id_anio_presupuesto;
                    var pres = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_mes == mes && x.id_anio == anio && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();
                    if (pres == null) { valor_presupuesto = 0; }
                    else { valor_presupuesto = (decimal)pres.valor_presupuesto; }

                    //OBTENER EL VALOR EJERCIDO
                    decimal valor_ejercido = ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 1);
                    valor_ejercido += ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 39118);
                    //OBTENER EL TOTAL DE LA REQUI DE LA CUENTA
                    decimal valor_requi_cuenta = 0;
                    //-------INVERSION
                    if (requi.id_requisicion_tipo == 3)
                    {
                        valor_requi_cuenta = 0;
                    }
                    else
                    {
                        foreach (var articulo in detalle.Distinct())
                        {
                            int id_requi_d = (int)articulo.id_requisicion_articulo_d;
                            var cotizaciones = from confirmadas_g in db.C_compras_cotizaciones_confirmadas_g
                                               join requi_g in db.C_compras_requi_g on confirmadas_g.id_requisicion_articulo_g equals requi_g.id_requisicion_articulo_g
                                               join coti_requi in db.C_compras_cotizaciones_requisiciones on confirmadas_g.id_requisicion_articulo_g equals coti_requi.id_requisicion_articulo_g
                                               where coti_requi.activo == true && confirmadas_g.activo == true && confirmadas_g.id_status_cotizacion_confirmada == 1 && coti_requi.id_requisicion_articulo_d == id_requi_d
                                               && coti_requi.fecha_registro != null && coti_requi.confirmada == true && coti_requi.agrupada == true && coti_requi.orden_generada == null
                                               && coti_requi.C_compras_requi_d.id_cuenta_contable_g == id_cuenta
                                               select coti_requi;


                            foreach (var coti in cotizaciones.Distinct())
                            {
                                decimal cantidad_surtir = (decimal)coti.cantidad_surtir;
                                decimal precio_final = 0;
                                var cotizacion = db.C_compras_cotizaciones_articulos.Find(coti.id_compra_articulo_cotizacion);
                                int id_moneda = (int)cotizacion.id_tipo_moneda;
                                if (id_moneda == 1)
                                {
                                    precio_final = (decimal)cotizacion.precio_final;
                                    info_cuenta.Valor_dolar = 1;
                                    info_cuenta.Clave_fiscal = "MXN";
                                }
                                else
                                {
                                    decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                                    precio_final = (decimal)cotizacion.precio_final * valor_dolar;
                                    info_cuenta.Valor_dolar = valor_dolar;
                                    info_cuenta.Clave_fiscal = "USD";
                                }
                                 
                                valor_requi_cuenta += cantidad_surtir * precio_final;
                            }
                        }
                    }

                    info_cuenta.Id_requi_g = id_requi;
                    info_cuenta.Id_tipo_requisicion = (int)requi.id_requisicion_tipo;
                    info_cuenta.Cargo_contable = item.FirstOrDefault().C_cargos_contables_g.nombre_cargo;
                    info_cuenta.Cuenta = item.FirstOrDefault().C_cuentas_contables_g.cuenta;
                    info_cuenta.Nombre_cuenta = item.FirstOrDefault().C_cuentas_contables_g.nombre_cuenta;
                    info_cuenta.Presupuesto = valor_presupuesto;
                    info_cuenta.Ejercido = valor_ejercido;
                    info_cuenta.Proceso = 0;
                    info_cuenta.Disponible = valor_presupuesto - valor_ejercido;
                    info_cuenta.Requisicion = valor_requi_cuenta;

                    decimal monto_tolerancia = 0;
                    try
                    {
                        monto_tolerancia = (porcentaje_tolerancia * info_cuenta.Presupuesto) / 100;
                    }
                    catch (Exception) { }
                    info_cuenta.Resultado = info_cuenta.Disponible - info_cuenta.Requisicion + monto_tolerancia;
                    info_cuenta.Pendiente = 0;
                    info_cuenta.Diferencia = info_cuenta.Resultado - info_cuenta.Pendiente;
                    info_cuenta.Cargo = item.FirstOrDefault().C_cuentas_contables_g.id_cargo_contable.ToString();

                    info_cuenta.Porcentaje_tolerancia = porcentaje_tolerancia;
                    ViewBag.porc_tolerancia = porcentaje_tolerancia;
                    info_cuenta.Monto_tolerancia = monto_tolerancia;

                    total_requi += valor_requi_cuenta;
                    total_disponibles += info_cuenta.Disponible;

                    resumen.Add(info_cuenta);
                }

                ViewBag.total_requi = total_requi;
                ViewBag.total_disponibles = total_disponibles;
                return PartialView("../COMPRAS/REQUISICIONES/_ResumenPresupuestoCuentaRequi", resumen);
            }
            catch (Exception)
            {
                return PartialView("../COMPRAS/REQUISICIONES/_ResumenPresupuestoCuentaRequi", null);
            }
        }

        public decimal ConsultarDisponiblePresupuestoCuentasRequisicion(int id_requi)
        {
            decimal total_resultado = 0;
            var requi = db.C_compras_requi_g.Find(id_requi);

            decimal porcentaje_tolerancia = 0;
            try { porcentaje_tolerancia = (decimal)db.C_parametros_configuracion.Find(15).valor_numerico; }
            catch (Exception) { }

            var detalle = from agrupadas_d in db.C_compras_cotizaciones_confirmadas_d
                          join agrupadas_g in db.C_compras_cotizaciones_confirmadas_g on agrupadas_d.id_cotizacion_confirmada_g equals agrupadas_g.id_cotizacion_confirmada_g
                          join cotizaciones in db.C_compras_cotizaciones_requisiciones on agrupadas_d.id_cotizacion_requisicion equals cotizaciones.id_cotizacion_requisicion
                          where agrupadas_d.activo == true && agrupadas_g.activo == true && cotizaciones.activo == true && agrupadas_g.id_status_cotizacion_confirmada == 1 && cotizaciones.orden_generada == null
                          && cotizaciones.confirmada == true && cotizaciones.fecha_confirmacion != null && cotizaciones.id_requisicion_articulo_g == id_requi
                          select cotizaciones.C_compras_requi_d;

            foreach (var item in detalle.Distinct().GroupBy(x => x.id_cuenta_contable_g))
            {
                int id_cuenta = (int)item.FirstOrDefault().id_cuenta_contable_g;
                decimal valor_presupuesto;
                DateTime fecha_requi = (DateTime)requi.fecha_registro;
                int mes = fecha_requi.Month;
                int anio = db.C_presupuestos_anios.Where(x => x.anio == fecha_requi.Year.ToString()).FirstOrDefault().id_anio_presupuesto;
                var pres = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_mes == mes && x.id_anio == anio && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();
                if (pres == null) { valor_presupuesto = 0; }
                else { valor_presupuesto = (decimal)pres.valor_presupuesto; }

                decimal valor_ejercido = ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 1);
                valor_ejercido += ObtenerMontoEjercidoCuentaMes(anio, mes, (int)item.FirstOrDefault().id_cuenta_contable_g, 39118);
                decimal valor_requi_cuenta = 0;
                if (requi.id_requisicion_tipo == 3)
                {
                    valor_requi_cuenta = 0;
                }
                else
                {
                    foreach (var articulo in detalle.Distinct())
                    {
                        int id_requi_d = (int)articulo.id_requisicion_articulo_d;
                        var cotizaciones = from confirmadas_g in db.C_compras_cotizaciones_confirmadas_g
                                           join requi_g in db.C_compras_requi_g on confirmadas_g.id_requisicion_articulo_g equals requi_g.id_requisicion_articulo_g
                                           join coti_requi in db.C_compras_cotizaciones_requisiciones on confirmadas_g.id_requisicion_articulo_g equals coti_requi.id_requisicion_articulo_g
                                           where coti_requi.activo == true && confirmadas_g.activo == true && confirmadas_g.id_status_cotizacion_confirmada == 1 && coti_requi.id_requisicion_articulo_d == id_requi_d
                                           && coti_requi.fecha_registro != null && coti_requi.confirmada == true && coti_requi.agrupada == true && coti_requi.orden_generada == null
                                           && coti_requi.C_compras_requi_d.id_cuenta_contable_g == id_cuenta
                                           select coti_requi;


                        foreach (var coti in cotizaciones.Distinct())
                        {
                            decimal cantidad_surtir = (decimal)coti.cantidad_surtir;
                            decimal precio_final = 0;
                            var cotizacion = db.C_compras_cotizaciones_articulos.Find(coti.id_compra_articulo_cotizacion);
                            int id_moneda = (int)cotizacion.id_tipo_moneda;
                            if (id_moneda == 1)
                            {
                                precio_final = (decimal)cotizacion.precio_final;
                            }
                            else
                            {
                                decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                                precio_final = (decimal)cotizacion.precio_final * valor_dolar;
                            }

                            valor_requi_cuenta += cantidad_surtir * precio_final;
                        }
                    }
                }
                decimal disponible = valor_presupuesto - valor_ejercido;
                decimal monto_tolerancia = 0;
                try
                {
                    monto_tolerancia = (porcentaje_tolerancia * valor_presupuesto) / 100;
                }
                catch (Exception) { }
                total_resultado += disponible - valor_requi_cuenta + monto_tolerancia;
            }
            return total_resultado;
        }

        public string CalcularTotalRequi(int[] requis)
        {
            decimal total = 0;
            var requisList = requis.ToList();

            var articulo_requi = from requisiciones in db.C_compras_cotizaciones_requisiciones
                                 join articulos in db.C_compras_cotizaciones_articulos
                                 on requisiciones.id_compra_articulo_cotizacion equals articulos.id_compra_articulo_cotizacion
                                 where requisiciones.confirmada == true && requisList.Contains((int)requisiciones.id_requisicion_articulo_g) && requisiciones.activo == true && articulos.id_tipo_moneda == 1 
                                 select requisiciones;
            foreach (var item in articulo_requi)
            {
                total = total + (((decimal)item.cantidad_surtir) * ((decimal)item.C_compras_cotizaciones_articulos.precio_final));
            }


            var articulo_requi_dls = from requisiciones in db.C_compras_cotizaciones_requisiciones
                                 join articulos in db.C_compras_cotizaciones_articulos
                                 on requisiciones.id_compra_articulo_cotizacion equals articulos.id_compra_articulo_cotizacion
                                 where requisiciones.confirmada == true && requisList.Contains((int)requisiciones.id_requisicion_articulo_g) && requisiciones.activo == true && articulos.id_tipo_moneda == 39118 
                                 select requisiciones;
            foreach (var item in articulo_requi_dls)
            {
                decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                decimal precio_final = (decimal)item.C_compras_cotizaciones_articulos.precio_final * valor_dolar;
                total = total + ((decimal)item.cantidad_surtir * precio_final);
            }


            return total.ToString("N2");
        }

        // 1= Solicita y Cancelar   2= Autorizar y Cancelar     3 = NO TIENES PERMITIDO NADA
        public int ValidarCondiciones(int[] id_cuentas_contables, string[] resultado_orden, decimal monto_requi, int id_tipo_requi)
        {
            var personal = (int)Session["LoggedId"];

            if (id_tipo_requi == 3)  //INVERSION
            {
                var valid_firmar = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == personal && x.id_usuario_master_accion == 2003 && x.activo == true).FirstOrDefault();
                if (valid_firmar != null) { ViewBag.valid_firmar = true; return 5; }
                else { return 4; ; }
            }

            //var firma_cargo = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == personal && cargo_contable.Contains(x.id_cargo_contable_g.ToString()) && x.id_firma_cargo_permiso == 4).ToList();
            //if (firma_cargo.Count() <= 0)
            //{
            //    return 3;
            //}

            var ctas_usuario = catalogos.CuentasContableUsuariosID(personal);
            for (int i = 0; i < id_cuentas_contables.Length; i++)
            {
                if (!ctas_usuario.Contains(id_cuentas_contables[i])) { return 3; }
            }

            var monto_usuario = db.C_firmas_usuarios_montos.Where(x => x.id_usuario_corporativo == personal).FirstOrDefault().montos;
            decimal monto = (decimal)monto_usuario;
            bool valid_negativo = true;
            int validar = 0;
            if (monto < monto_requi)
            {
                validar = 1;
                return validar;
            }
            else
            {
                for (int i = 0; i < resultado_orden.Length; i++)
                {
                    decimal resultado = Convert.ToDecimal(resultado_orden[i]);
                    if (resultado < 0)
                    {
                        validar = 1;
                        valid_negativo = false;
                        return validar;
                    }
                    else
                    {
                        validar = 2;
                        //return validar;
                    }
                }

                if (valid_negativo == false) { return 1; }

                var valid_permiso_especial = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == personal && x.id_usuario_master_accion == 2009 && x.activo == true).FirstOrDefault();
                if (valid_permiso_especial != null) { return 6; }

                return validar;
            }

        }


        //GENERAR ORDEN DE COMPRA
        //MODO 2 - MASTER     MODO 1 - USUARIO
        public int AutorizarRequisicionesFinal(int[] id_requis, int modo)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                string usuario = Session["LoggedUser"].ToString();

                DateTime hoy = DateTime.Now;
                for (int i = 0; i < id_requis.Length; i++)
                {
                    int id_requisicion_g = id_requis[i];
                    var requi_info = db.C_compras_requi_g.Find(id_requisicion_g);
                    int id_proveedor = 0;

                    var agrupadas = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true && x.id_status_cotizacion_confirmada == 1).ToList();
                    foreach (var confirmada in agrupadas)
                    {
                        confirmada.id_status_cotizacion_confirmada = 2; //FIRMADA

                        if (modo == 4)  //NO AFECTA EL PRESUPUESTO
                        {
                            confirmada.solicita_autorizacion = false;
                        }

                        int id_confirmacion_g = confirmada.id_cotizacion_confirmada_g;
                        var confirmadas_d = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.id_cotizacion_confirmada_g == id_confirmacion_g && x.activo == true
                        && x.C_compras_cotizaciones_requisiciones.agrupada == true && x.C_compras_cotizaciones_requisiciones.confirmada == true && x.C_compras_cotizaciones_requisiciones.orden_generada == null).ToList();

                        confirmadas_d.ForEach(x => x.C_compras_cotizaciones_requisiciones.orden_generada = true);

                        id_proveedor = (int)confirmadas_d.FirstOrDefault().C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.id_compras_proveedor;

                        C_compras_ordenes_g orden_g = new C_compras_ordenes_g();
                        orden_g.id_requisicion_articulo_g = id_requisicion_g;
                        orden_g.id_proveedor = id_proveedor;
                        orden_g.id_cotizacion_confirmada_g = id_confirmacion_g;
                        orden_g.fecha_registro = hoy;
                        orden_g.id_usuario_genera = id_usuario;
                        orden_g.id_centro = requi_info.id_centro_g;
                        orden_g.id_tipo_orden = requi_info.id_requisicion_tipo;
                        orden_g.id_status_orden = 1; //GENERADA
                        orden_g.token_orden = notificacion.GenerarTokenLongitud(25);
                        orden_g.id_compras_ubicacion_entrega = confirmada.id_compras_ubicacion_entrega;
                        orden_g.alta_proveedor = false;
                        while (db.C_compras_ordenes_g.Where(x => x.token_orden == orden_g.token_orden).FirstOrDefault() != null)
                        {
                            orden_g.token_orden = notificacion.GenerarTokenLongitud(25);
                        }

                        orden_g.activo = true;
                        orden_g.entregado = false;
                        db.C_compras_ordenes_g.Add(orden_g);
                        db.SaveChanges();
                        int id_orden_g = orden_g.id_compras_orden_g;

                        foreach (var cotizaciones in confirmadas_d)
                        {
                            decimal cantidad_total = 0;
                            decimal precio_total = 0;

                            var coti_requisicion = cotizaciones.C_compras_cotizaciones_requisiciones;
                            cantidad_total = (decimal)cotizaciones.C_compras_cotizaciones_requisiciones.cantidad_surtir;

                            //cantidad_total += (decimal)cotizaciones.Sum(x => x.C_compras_cotizaciones_requisiciones.cantidad_surtir);


                            //foreach (var cotizacion in cotizaciones)
                            //{
                            //    precio_total += (decimal)cotizacion.C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.precio_final; //* (decimal)historial.cantidad_surtir;
                            //}
                            //precio_total += (decimal)confirmadas_d.FirstOrDefault().C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.precio_final;
                            precio_total = (decimal)cotizaciones.C_compras_cotizaciones_requisiciones.C_compras_cotizaciones_articulos.precio_final;


                            var info_requi_d_ = cotizaciones.C_compras_cotizaciones_requisiciones.C_compras_requi_d;
                            C_compras_ordenes_d orden_d = new C_compras_ordenes_d();
                            orden_d.id_compras_orden_g = id_orden_g;
                            orden_d.id_articulo = coti_requisicion.id_articulo;
                            orden_d.cantidad_compra = cantidad_total;
                            orden_d.precio_unitario = precio_total;
                            orden_d.id_tipo_moneda = coti_requisicion.C_compras_cotizaciones_articulos.id_tipo_moneda;
                            orden_d.id_cargo_contable = info_requi_d_.id_cargo_contable;
                            orden_d.id_cuenta_contable = info_requi_d_.id_cuenta_contable_g;
                            orden_d.nombre_articulo = info_requi_d_.C_articulos_catalogo.nombre_articulo;
                            orden_d.id_unidad_medida = info_requi_d_.C_articulos_catalogo.id_unidad_medida;
                            orden_d.cantidad_entregada = 0;
                            orden_d.cantidad_precarga = 0;
                            orden_d.entregado = false;
                            orden_d.activo = true;
                            db.C_compras_ordenes_d.Add(orden_d);
                        }
                        confirmada.id_status_cotizacion_confirmada = 2; //FIRMADA
                        db.SaveChanges();

                        var links = db.C_compras_requisiciones_links_autorizacion.Where(x => x.id_requisicion == id_requisicion_g).ToList();
                        if (links.Count() > 0)
                        {
                            links.ForEach(x => x.activo = false);
                            db.SaveChanges();
                        }

                        try
                        {
                            string nombre_comprador = "";
                            string correo_comprador = "";
                            string correo_proveedor = "";
                            string razon_social = "";
                            try
                            {
                                nombre_comprador = confirmada.C_usuarios_corporativo.C_empleados.nombres + " " + confirmada.C_usuarios_corporativo.C_empleados.apellido_paterno;
                                correo_comprador = confirmada.C_usuarios_corporativo.C_empleados.correo;

                                correo_proveedor = confirmada.correo_proveedor_suborden;
                                if (orden_g.id_tipo_orden != 2) { correo_proveedor = confirmada.correo_proveedor_suborden + "; " + correo_comprador; }
                                razon_social = db.C_compras_proveedores.Find(id_proveedor).razon_social;
                            }
                            catch (Exception) { }

                            //PROVEEDOR
                            string mensaje = "<strong>Buen día.</strong><br />" +
                                    "<br /><label>BETA SANTA MONICA solicita dar trámite a esta orden de compra: </label>" +
                                    "<br /><strong>Requisición: " + requi_info.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                                    "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                                    "<br /><strong style='color: red;'>FAVOR DE CONFIRMAR DE RECIBIDO</strong>" +

                                    "<br /><strong>Proveedor: </strong><label>"+ razon_social + "</ label>" +

                                    "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                                    "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                            notificacion.EnviarCorreoUsuario("ORDEN DE COMPRA PARA BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_proveedor /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);

                            ///QUIEN SOLICITA
                            string correo_solicita = requi_info.C_usuarios_corporativo5.C_empleados.correo;
                            mensaje = "<strong>Buen día.</strong><br />" +
                                    "<br /><label>Su orden de compra fue autorizada por el usuario: " + usuario + " </label>" +
                                    "<br /><strong>Requisición: " + requi_info.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                                    "<br /><strong>Orden de compra enviada al proveedor: " + razon_social + " " +
                                    "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                                    "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                                    "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                            notificacion.EnviarCorreoUsuario("ORDEN DE COMPRA PARA BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_solicita /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);


                            //if (orden_g.id_tipo_orden == 2)
                            //{
                            //    //PERSONAL DE COMPRAS
                            //    mensaje = "<strong>Buen día departamento de compras</strong><br />" +
                            //            "<br /><label>La orden de compra fue autorizada por el usuario: " + usuario + " </label>" +
                            //            "<br /><strong>Requisición: " + requi_info.folio + "</strong><br /><strong>Orden de compra: #" + id_orden_g + "</strong>" +
                            //            "<br /><strong>Orden de compra enviada al proveedor: " + orden_g.C_compras_proveedores.razon_social + " al correo: " + confirmada.correo_proveedor_suborden + "</strong><br />" +
                            //            "<br /><strong>Comprador: " + nombre_comprador + "</strong>" +
                            //            "<br /><a href='https://siib.beta.com.mx/PORTAL/GenerarOrdenCompraPDF?id_orden_g=" + id_orden_g + "&&token_orden=" + orden_g.token_orden + "'>CLIC AQUI PARA VER LA ORDEN DE COMPRA</a>" +
                            //            "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                            //    notificacion.EnviarCorreoUsuario("ORDEN DE COMPRA PARA BETA SANTA MONICA: #" + orden_g.id_compras_orden_g + "", correo_comprador /*orden_g.C_compras_proveedores.correo_prov*/, mensaje);
                            //}

                        }
                        catch (Exception) { }
                    }
                    CambiarStatusRequisicion(id_requisicion_g, 7);
                }
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }


        public int RecotizarRequisicionCancelarAutFinal(int id_requisicion, string justificacion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var requisicion = db.C_compras_requi_g.Find(id_requisicion);
                var confirmaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_g == id_requisicion && x.activo == true && x.agrupada == true
                && x.confirmada == true && x.orden_generada == null).ToList();
                confirmaciones.ForEach(x => x.activo = false);
                db.SaveChanges();
                var confirmacion_detalle = confirmaciones.Select(z => z.id_requisicion_articulo_d);
                var detalle = db.C_compras_requi_d.Where(x => confirmacion_detalle.Contains(x.id_requisicion_articulo_d)).ToList().Distinct();
                foreach (var item in detalle)
                {
                    item.cantidad_cotizada = 0;
                    item.cotizado = null;
                    item.recotizado = true;
                    db.SaveChanges();
                }
                //EN COTIAZACION
                requisicion.id_status_requi = 5;
                var agrupadas = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion && x.activo == true && x.id_status_cotizacion_confirmada == 1).Select(x => x.id_cotizacion_confirmada_g).ToArray();
                EliminarSolicitudCompra(agrupadas);
                if (requisicion.id_requisicion_tipo == 2) //SERVICIO
                {
                    requisicion.id_status_requi = 1008; //RECHAZADA
                    requisicion.activo = false;
                    db.SaveChanges();
                    try
                    {
                        var usuario_rechaza = db.C_usuarios_corporativo.Find(id_usuario).C_empleados;
                        string correo_registro_requi = requisicion.C_usuarios_corporativo5.C_empleados.correo;
                        string usaurio_registro_regui = requisicion.C_usuarios_corporativo5.C_empleados.nombres + " " + requisicion.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        string usuario_rechaza_requi = usuario_rechaza.nombres + " " + usuario_rechaza.apellido_paterno;
                        string mensaje = "<strong>Estimado: " + usaurio_registro_regui + "</strong>" +
                            "<br /><br />" +
                            "<label>Su requisición fue rechazada por el siguiente motivo: </label><br />" +
                            "<strong>" + usuario_rechaza_requi + ": </strong><label>" + justificacion + "</label>" +
                            "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta.png' width='200'/>";
                        notificacion.EnviarCorreoUsuario("Tu requisición : #" + requisicion.folio + " fue rechazada", correo_registro_requi, mensaje);
                    }
                    catch (Exception) { }
                }
                else
                {
                    C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                    log.id_requisicion = id_requisicion;
                    log.id_requisicion_articulo_g = id_requisicion;
                    log.id_tipo_log = 4;  //RECOTIZA
                    log.fecha_registro = DateTime.Now;
                    log.activo = true;
                    log.id_usuario_registra = id_usuario;
                    log.justificacion = justificacion;
                    db.C_compras_requisiciones_logs.Add(log);
                    db.SaveChanges();
                }
                    return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int SolicitarAutorizacionDireccion(int id_requisicion, string justificacion, int modo)
        {
            int id_usuario = (int)Session["LoggedId"];
            var info_usuario_solicita = db.C_usuarios_corporativo.Find(id_usuario);

            var requi = db.C_compras_requi_g.Find(id_requisicion);

            var usuarios_master = db.C_usuarios_masters.Where(x => x.activo == true && (x.id_usuario_master_accion == 1 || x.id_usuario_master_accion == 2009)).ToList();
            foreach (var item in usuarios_master)
            {
                string token_aut = notificacion.GenerarTokenLongitud(15);
                C_compras_requisiciones_links_autorizacion token = new C_compras_requisiciones_links_autorizacion();
                token.id_requisicion = id_requisicion;
                token.token = token_aut;
                token.id_usuario_acceso = item.id_usuario_corporativo;
                token.activo = true;
                db.C_compras_requisiciones_links_autorizacion.Add(token);
                db.SaveChanges();
                if (modo == 4 || modo == 3) {  //3: ADOLFO, NICTE U HORACIO - 4: HORACIO
                    notificacion.EnviarCorreoNotificaAutorizacionEspecial(id_requisicion, justificacion, token_aut, (int)item.id_usuario_corporativo, item.C_usuarios_corporativo.C_empleados.correo, info_usuario_solicita.usuario, requi.C_centros_g.siglas, requi.concepto);
                }
                else
                {
                    notificacion.EnviarCorreoSolicitaAutorizacion(id_requisicion, justificacion, token_aut, (int)item.id_usuario_corporativo, item.C_usuarios_corporativo.C_empleados.correo, info_usuario_solicita.usuario, requi.C_centros_g.siglas, requi.concepto);
                }
            }

            try
            {
                //COPIA A CARLOS
                string token_aut = notificacion.GenerarTokenLongitud(15);
                C_compras_requisiciones_links_autorizacion token = new C_compras_requisiciones_links_autorizacion();
                token.id_requisicion = id_requisicion;
                token.token = token_aut;
                token.id_usuario_acceso = 4;
                token.activo = true;
                db.C_compras_requisiciones_links_autorizacion.Add(token);
                db.SaveChanges();
                if (modo == 4 || modo == 3){ notificacion.EnviarCorreoNotificaAutorizacionEspecial(id_requisicion, justificacion, token_aut, 4, "cmartinez@beta.com.mx", info_usuario_solicita.usuario, requi.C_centros_g.siglas, requi.concepto); }
                else { notificacion.EnviarCorreoSolicitaAutorizacion(id_requisicion, justificacion, token_aut, 4, "cmartinez@beta.com.mx", info_usuario_solicita.usuario, requi.C_centros_g.siglas, requi.concepto);  }
            }
            catch (Exception){}
            
            var requisiciones = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion /*&& x.solicita_autorizacion == false*/).ToList();
            requisiciones.ForEach(x => x.solicita_autorizacion = true);
            db.SaveChanges();


            C_compras_requi_g_solicita_autorizacion solicita_autorizacion = new C_compras_requi_g_solicita_autorizacion();
            solicita_autorizacion.id_usuario_registra = id_usuario;
            solicita_autorizacion.id_requisicion_articulo_g = id_requisicion;
            solicita_autorizacion.justificacion = justificacion;
            solicita_autorizacion.fecha_registro = DateTime.Now;
            solicita_autorizacion.activo = true;
            db.C_compras_requi_g_solicita_autorizacion.Add(solicita_autorizacion);
            db.SaveChanges();

            try
            {
                C_compras_requisiciones_logs log = new C_compras_requisiciones_logs();
                log.id_requisicion = id_requisicion;
                log.id_requisicion_articulo_g = id_requisicion;
                log.id_tipo_log = 3;  //SOLICITA AUT
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


        public ActionResult RequisicionMasterView(string token, int id_usuario_aut, int id_requisicion)
        {
            USUARIOLOGINController login = new USUARIOLOGINController();
            login.CerrarSesion();

            if (token == null) { RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }

            try
            {
                var valid_master = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == id_usuario_aut && (x.id_usuario_master_accion == 1 || x.id_usuario_master_accion == 2009) && x.activo == true).ToList();
                if (valid_master == null) { return RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }

                C_usuarios_corporativo usuario = new C_usuarios_corporativo();
                if (valid_master.Select(x => x.id_usuario_master_accion).Contains(2009)) { ViewBag.modo = 3; usuario = valid_master.Where(x => x.id_usuario_master_accion == 2009).FirstOrDefault().C_usuarios_corporativo; } //USUARIO ESPECIAL QUE FUNGE COMO MASTER Y NORMAL
                else if (valid_master.Select(x => x.id_usuario_master_accion).Contains(1)) { ViewBag.modo = 2; usuario = valid_master.Where(x => x.id_usuario_master_accion == 1).FirstOrDefault().C_usuarios_corporativo; }  //USUARIO UNICAMENTE MASTER
                else { ViewBag.modo = 1; } //USUARIO NORMAL

                var valid_token = db.C_compras_requisiciones_links_autorizacion.Where(x => x.id_requisicion == id_requisicion && x.token == token && x.id_usuario_acceso == id_usuario_aut).FirstOrDefault();
                if (valid_token == null) { return RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }


                UsuarioLogin2(usuario);

                var requisicion = db.C_compras_requi_g.Find(id_requisicion);
                ViewBag.folio = requisicion.folio;
                ViewBag.id_requisicion = id_requisicion;
                
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/REQUISICIONES/FIRMAS/AutFinalMasterView/AutorizacionRequisicionIndividual");
        }


        public bool UsuarioLogin2(C_usuarios_corporativo uSUARIO_LOGIN)
        {
            try
            {
                var encodingPasswordString = string.Empty;
                var items = db.C_usuarios_corporativo.Where(u => u.usuario.Equals(uSUARIO_LOGIN.usuario) && u.Activo == true).ToList();

                if (items.Count() > 0)
                {
                    if (uSUARIO_LOGIN.password != null)
                    {
                        //encodingPasswordString = PasswordHelper.EncodePassword(uSUARIO_LOGIN.password, "MySalt");
                        encodingPasswordString = uSUARIO_LOGIN.password;
                    }
                    else
                    {
                        ViewBag.Message = "Introduzca todos los datos";
                        return false;
                    }

                    foreach (var n in items)
                    {
                        if (n.password != null)
                        {
                            //if (uSUARIO_LOGIN.CS_usuarios.NOMBRE.Equals(n.usuario) && uSUARIO_LOGIN.PASS.Equals("DV22"))
                            if (uSUARIO_LOGIN.usuario.Equals(n.usuario) && n.password.Equals(encodingPasswordString))
                            {

                                if (n.password == PasswordHelper.EncodePassword("123456", "MySalt"))
                                {
                                    ViewBag.NombreEmpleado = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == n.id_usuario_corporativo).Select(x => x.C_empleados.nombres).FirstOrDefault();
                                    ViewBag.IdUsuario = n.id_usuario_corporativo;
                                    return false;
                                }
                                else
                                {
                                    try
                                    {
                                        if (Session["LoggedIdRol"] == null)
                                        {
                                        }
                                        else
                                        {
                                            var id_usuario_corp = (int)Session["LoggedId"];
                                            var usuario = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario_corp).FirstOrDefault();
                                            if (id_usuario_corp == n.id_usuario_corporativo)
                                            {

                                            }
                                            else
                                            {
                                                ViewBag.Message = "Sesion iniciada";
                                                Session["sesion"] = "Sesion iniciada";
                                                return false;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    Session["LoggedIdEmpleado"] = n.id_empleado;
                                    Session["Nombre_usuario"] = n.C_empleados.nombres + " " + n.C_empleados.apellido_paterno;
                                    Session["LoggedUser"] = n.usuario;
                                    Session["LoggedUserPass"] = n.password;
                                    Session["LoggedId"] = n.id_usuario_corporativo;
                                    Session["LoggedIdRol"] = n.id_rol;
                                    Session["id_tipo_usuario"] = 1; //CORPORATIVO
                                    ViewBag.idusuario = n.id_usuario_corporativo;
                                    int RolId = (int)Session["LoggedIdRol"];
                                    Session["logo_marca"] = db.C_parametros_configuracion.Find(1).valor_texto;

                                    // ----DASHBOARD
                                    int id_usuario = n.id_usuario_corporativo;
                                    var valid_dashboard = db.C_dashboard_usuarios.Where(x => x.id_usuario == id_usuario && x.axtivo == true).FirstOrDefault();
                                    if (valid_dashboard != null) { Session["TipoDashboard"] = valid_dashboard.id_dashboard_tipo; }
                                    else { Session["TipoDashboard"] = 0; }
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Usuario o contraseña incorrectos";
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Message = "Usuario o contraseña incorrectos";
                    return false;
                    //return View("UsuarioLogin");
                }
                if (ModelState.IsValid)
                {
                    //obtiene los permisos de cada servicio/modulo para el usuario loggeado           
                    List<int> permisosLista = new List<int>();
                    int RolId = (int)Session["LoggedIdRol"];
                    var permisosServicioModulo = from p in db.C_modulos_sub_permisos
                                                 where p.id_rol == RolId && p.estatus == true && p.C_modulos_sub.activo == true && p.C_modulos_sub.C_modulos.activo == true
                                                 select p;

                    if (permisosServicioModulo.Count() > 0)
                    {
                        permisos = new List<PermisosUsuario>();
                        submodulos = new List<SubmodulosUsuario>();
                        modulos = new List<ModulosUsuario>();
                        sub_modulos_session = new List<int>();

                        foreach (var n in permisosServicioModulo.OrderBy(x => x.C_modulos_sub.nombre))
                        {
                            permisosLista.Add((int)n.id_modulos_sub);
                            permisosLista.Add((int)n.C_modulos_sub.id_modulo);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.id_modulos_sub, m.C_modulos_sub.nombre, m.C_modulos_sub.funcion, m.C_modulos_sub.controlador, m.C_modulos_sub.aplica_submenu, m.C_modulos_sub.id_submenu_modulo_sub }).Distinct().OrderBy(x => x.nombre))
                        {
                            int id_submenu_item = 0;
                            if (item.id_submenu_modulo_sub != null) { id_submenu_item = (int)item.id_submenu_modulo_sub; }
                            submodulos.Add(new SubmodulosUsuario((int)item.id_modulo, (int)item.id_modulos_sub, item.nombre, item.funcion, item.controlador, (bool)item.aplica_submenu, id_submenu_item));
                            sub_modulos_session.Add((int)item.id_modulos_sub);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.C_modulos_sub.C_modulos.nombre, m.C_modulos_sub.C_modulos.icono }).Distinct().OrderBy(x => x.nombre))
                        {
                            modulos.Add(new ModulosUsuario((int)item.id_modulo, item.nombre, item.icono));
                        }
                    }

                    if (modulos == null || submodulos == null)
                    {
                        ViewBag.Message = "El rol de tu usuario no cuenta con modulos asignados";
                        return false;
                    }
                    Session["modulos"] = modulos;
                    Session["submodulos"] = submodulos;

                    ViewBag.permisos = permisosLista;


                    return true;
                }

                ViewBag.Message = "Usuario o contraseña incorrectos";
                return false;
                //return RedirectToAction("UsuarioLogin");
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();

                ViewBag.ms = e.Message;
                ViewBag.id_rol = (int)Session["LoggedIdRol"];
                return false;
            }
        }

        #endregion



        
    }
}