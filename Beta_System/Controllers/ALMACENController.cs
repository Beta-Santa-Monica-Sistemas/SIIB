using Antlr.Runtime.Misc;
using Beta_System.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text.pdf.qrcode;
using Microsoft.Ajax.Utilities;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Windows.Media.Animation;
using WebGrease.Css.Extensions;
using ZXing;
using DataTable = System.Data.DataTable;

namespace Beta_System.Controllers
{
    public class ALMACENController : Controller
    {
        private CATALOGOSController catalogos = new CATALOGOSController();
        private comprasEntities compras = new comprasEntities();
        private NOTIFICACIONESController notificacion = new NOTIFICACIONESController();
        private ARTICULOSController articulos = new ARTICULOSController();
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private REQUISICIONESController requisiciones = new REQUISICIONESController();

        #region SOLICITUDES DE MERCANCIA
        public ActionResult SolicitudesMercancia()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(14)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("SolicitudesMercancia/Index");
        }

        public PartialViewResult AgregarArticuloSolicitud(int count_articulos_detalle, int id_articulo, string articulo, string unidad_medida, int id_unidad_medida, string precio_articulo,
            string importe_articulo, decimal cantidad, string cargo_contable, string obs, int id_cuenta, int id_cargo, string clave)
        {
            ViewBag.count_articulos_detalle = count_articulos_detalle;
            ViewBag.id_articulo = id_articulo;
            ViewBag.articulo = articulo;
            ViewBag.unidad_medida = unidad_medida;
            ViewBag.id_unidad_medida = id_unidad_medida;
            ViewBag.precio_articulo = precio_articulo;
            ViewBag.importe_articulo = importe_articulo;
            ViewBag.cantidad = cantidad;
            ViewBag.cargo_contable = cargo_contable;
            ViewBag.obs = obs;
            ViewBag.id_cuenta = id_cuenta;
            ViewBag.id_cargo = id_cargo;
            ViewBag.clave_mercancia = clave;

            return PartialView("SolicitudesMercancia/_TbodyArticulosSolicitud");
        }

        public int GenerarSolicitudMercancia(C_almacen_solicitudes_mercancia_g c_solicitud_g, int id_productor)
        {
            try
            {
                if (c_solicitud_g.id_empleado_remitente == null) { return -1; }

                int id_empleado = (int)Session["LoggedIdEmpleado"];
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;

                c_solicitud_g.fecha_registro = hoy;
                c_solicitud_g.id_usuario_registra = id_usuario;
                c_solicitud_g.id_empleado_solicita = id_empleado;

                c_solicitud_g.activo = true;
                c_solicitud_g.id_status_solicitud = 1; //POR AUTORIZAR
                if (id_productor > 0) { c_solicitud_g.id_alimentacion_productor = id_productor; }


                //c_solicitud_g.autorizada = false;
                c_solicitud_g.atendida = false;

                db.C_almacen_solicitudes_mercancia_g.Add(c_solicitud_g);
                db.SaveChanges();
                return c_solicitud_g.id_solicitud_mercancia_g;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 0;
            }
        }

        public bool GenerarSolicitudMercanciaDetalle(int id_solicitud_g, int[] id_articulos, int[] id_unidades, decimal[] cantidades, decimal[] precios, decimal[] importes, int[] cargos, int[] cuentas, string[] obs, string[] cargos_contables)
        {
            try
            {
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_almacen_solicitudes_mercancia_d detalle = new C_almacen_solicitudes_mercancia_d();
                    detalle.id_solicitud_mercancia_g = id_solicitud_g;
                    detalle.id_articulo = id_articulos[i];
                    detalle.cantidad = cantidades[i];
                    detalle.cantidad_entregada = 0;
                    detalle.costo = precios[i];
                    detalle.id_cargo_contable_g = cargos[i];
                    detalle.id_cuenta_contable_g = cuentas[i];
                    detalle.cargo_cuenta_contable = cargos_contables[i];
                    detalle.entregado = false;
                    detalle.cambio_articulo = false;
                    detalle.observaciones = obs[i];
                    detalle.activo = true;
                    detalle.id_unidad_medida = id_unidades[i];
                    detalle.importe = importes[i];
                    db.C_almacen_solicitudes_mercancia_d.Add(detalle);
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

        public PartialViewResult ConsultarSolicitudesTable(int[] id_estatus, string fecha_fin, string fecha_inicio, string folio, int[] id_almacen)
        {
            if (id_almacen.Contains(0))
            {
                int id_usuario = (int)Session["LoggedId"];
                id_almacen = db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => (int)x.id_almacen_g).ToArray();
                
                //id_almacen = from alm in db.C_almacen_almacenes_g
                                        //join us in db.C_almacen_almacenes_usuarios on alm.id_almacen_g equals us.id_almacen_g
                                        //where alm.activo == true && us.activo == true && us.id_usuario == id_usuario
                                        //orderby alm.nombre_almacen
                                        //select ;
            }

            if (folio != "")
            {
                var solicitud = db.C_almacen_solicitudes_mercancia_g.Where(x => x.activo == true && id_almacen.Contains((int)x.id_almacen_g) && x.id_solicitud_mercancia_g.ToString() == folio)
                                    .OrderBy(x => x.id_status_solicitud).ToList();
                return PartialView("SolicitudesMercancia/_SolicitudesMercanciaTable", solicitud);
            }
            else
            {
                if (id_estatus.Contains(0))
                {
                    id_estatus = db.C_almacen_solicitudes_status.Select(x => x.id_solicitud_status).ToArray();
                }

                List<C_almacen_solicitudes_mercancia_g> solicitudes = null;
                if (fecha_fin != "" && fecha_inicio != "")
                {
                    DateTime fecha1 = DateTime.Parse(fecha_inicio);
                    DateTime fecha2 = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                    solicitudes = db.C_almacen_solicitudes_mercancia_g.Where(x => x.activo == true && id_almacen.Contains((int)x.id_almacen_g) && id_estatus.Contains((int)x.id_status_solicitud)
                                    && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2)
                                    .OrderBy(x => x.id_status_solicitud).ToList();
                }
                else
                {
                    solicitudes = db.C_almacen_solicitudes_mercancia_g.Where(x => x.activo == true && id_almacen.Contains((int)x.id_almacen_g) && id_estatus.Contains((int)x.id_status_solicitud))
                                    .OrderBy(x => x.id_status_solicitud).ToList();
                }

                return PartialView("SolicitudesMercancia/_SolicitudesMercanciaTable", solicitudes);
            }
        }

        public PartialViewResult ConsultarSolicitudDetalle(int id_solicitud_g)
        {
            var detalle = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g).ToList();
            return PartialView("SolicitudesMercancia/_SolicitudesMercanciaDetalle", detalle);
        }

        public bool AutorizarSolicitud(int id_solicitud_g)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var soli = db.C_almacen_solicitudes_mercancia_g.Find(id_solicitud_g);
                soli.autorizada = true;
                soli.fecha_autorizacion = DateTime.Now;
                soli.id_usuario_autoriza = id_usuario;
                soli.id_status_solicitud = 4; //AUTORIZADO
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }

        }
        #endregion


        #region ENTREGA DE VALES
        public ActionResult EntregaValesMercancia()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(15)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("EntregaVales/Index");
        }

        public PartialViewResult ConsultarSolicitudesAutorizadas(int id_almacen_g)
        {
            var solicitudes = db.C_almacen_solicitudes_mercancia_g.Where(x => x.atendida == false && x.activo == true && x.autorizada == true && x.id_almacen_g == id_almacen_g).OrderBy(x => x.fecha_registro).ToList();
            return PartialView("EntregaVales/_SolicitudesAutorizadas", solicitudes);
        }

        public PartialViewResult ConsultarSolicitudDetalleAutorizar(int id_solicitud_g)
        {
            var detalle = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g && x.entregado == false).ToList();
            foreach (var item in detalle)
            {
                item.C_articulos_catalogo.descripcion_articulo = ConsultarExistenciaArticuloAlmacen((int)item.C_almacen_solicitudes_mercancia_g.id_almacen_g, (int)item.id_articulo);
            }
            return PartialView("EntregaVales/_SolicitudesMercanciaDetalleAtender", detalle);
        }

        public PartialViewResult ConsultarSolicitudDetalleHistorial(int id_solicitud_g)
        {
            var detalle = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g).ToList();
            return PartialView("EntregaVales/_SolicitudesMercanciaDetalleHistorial", detalle);
        }

        public int CambiarArticuloSolicitud(int id_solicitud_g, int id_solicitud_d, string id_articulo_nuevo, int id_articulo_viejo, string observaciones, decimal cantidad_nueva)
        {
            try
            {
                var articulo_viejo_mercancia = db.C_articulos_catalogo.Find(id_articulo_viejo);
                var articulo_nuevo_mercancia = db.C_articulos_catalogo.Where(x => x.clave == id_articulo_nuevo && x.activo == true).FirstOrDefault();
                int id_articulo = (int)articulo_nuevo_mercancia.id_articulo;
                var solicitud = db.C_almacen_solicitudes_mercancia_d.Find(id_solicitud_d);
                if (solicitud.cantidad_entregada != 0)
                {
                    return 1;
                }
                var valid_articulo_repetido = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g && x.id_articulo == id_articulo && x.id_solicitud_mercancia_d != id_solicitud_d).Count();
                if (valid_articulo_repetido > 0)
                {
                    return 2;
                }
                int id_unidad_medida;
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                ARTICULOSController articulos = new ARTICULOSController();
                try
                {
                    id_unidad_medida = (int)db.C_articulos_catalogo.Find(id_articulo_nuevo).id_unidad_medida;
                }
                catch (Exception)
                {
                    id_unidad_medida = 4;
                }
                ARTICULOSController compras = new ARTICULOSController();
                decimal precio_art = Convert.ToDecimal(compras.BuscarPrecioArticulo(id_articulo));
                //var ultimo_precio = db.C_compras_ordenes_d.Where(x => x.entregado == true && x.id_articulo == id_articulo).OrderByDescending(x => x.id_compras_orden_d).FirstOrDefault();
                //if (ultimo_precio == null || ultimo_precio.precio_unitario == 0 || ultimo_precio.precio_unitario == null)
                //{
                //    precio_art = 0;
                //}
                //else
                //{
                //    precio_art = Convert.ToDecimal(ultimo_precio.precio_unitario);
                //}
                C_almacen_solicitudes_cambios_articulos cambio = new C_almacen_solicitudes_cambios_articulos();
                cambio.id_solicitud_g = id_solicitud_g;
                cambio.id_solicitud_d = id_solicitud_d;
                cambio.id_usuario_cambio = id_usuario;
                cambio.fecha_cambio = hoy;
                cambio.observaciones = observaciones;
                cambio.activo = true;
                cambio.cantidad_vieja = solicitud.cantidad;
                cambio.cantidad_nueva = cantidad_nueva;
                cambio.id_articulo_nuevo = articulo_nuevo_mercancia.id_articulo;
                cambio.precio = precio_art;
                cambio.id_articulo_viejo = articulo_viejo_mercancia.id_articulo;
                db.C_almacen_solicitudes_cambios_articulos.Add(cambio);
                db.SaveChanges();
                solicitud.id_articulo = articulo_nuevo_mercancia.id_articulo;
                solicitud.cambio_articulo = true;
                solicitud.costo = cambio.precio;
                solicitud.importe = cambio.precio * cantidad_nueva;
                solicitud.id_unidad_medida = id_unidad_medida;
                solicitud.cantidad = cantidad_nueva;
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public PartialViewResult ConsultarCambiosArticuloSolicitud(int id_solicitud_g)
        {
            var cambios = db.C_almacen_solicitudes_cambios_articulos.Where(x => x.id_solicitud_g == id_solicitud_g && x.activo == true).OrderByDescending(x => x.fecha_cambio).ToList();
            return PartialView("EntregaVales/_CambiosArticuloSolicitud", cambios);
        }

        public PartialViewResult ConsultarHistorialValesTable(int id_almacen_g, string fecha_inicio, string fecha_fin)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var vales = db.C_almacen_solicitudes_mercancia_g.Where(x => x.atendida == true && x.activo == true && x.autorizada == true &&
            x.fecha_entrega >= fecha_i && x.fecha_entrega <= fecha_f && x.id_almacen_g == id_almacen_g).ToList();
            return PartialView("EntregaVales/_HistorialValesTable", vales);
        }

        public int EntregarVale(int id_solicitud_g, int[] id_solicitudes_d, bool[] entregados, decimal[] cant_entrega)
        {
            try
            {
                if (entregados.All(element => element == false))
                {
                    return 1;
                }

                DateTime hoy = DateTime.Now;
                var solicitud_g = db.C_almacen_solicitudes_mercancia_g.Find(id_solicitud_g);
                int id_alamcen_g = (int)solicitud_g.id_almacen_g;
                int id_usuario = (int)Session["LoggedId"];
                var info_tipo_mov = db.C_inventario_mov_tipos.Find(2); // 2 =  SALIDA DE MERCANCIA|ENTREGA DE VALE

                int id_mov_g = 0;
                var validSalida = db.C_inventario_almacen_mov_g.Where(x => x.id_transaccion_solicitud_mercancia_g == id_solicitud_g && x.id_inventario_mov_tipo == 2).FirstOrDefault(); //SALIDA (ENTREGA DE VALE)
                if (validSalida == null)
                {
                    C_inventario_almacen_mov_g mov_g = new C_inventario_almacen_mov_g();
                    mov_g.id_almacen_origen = id_alamcen_g;
                    mov_g.id_usuario_registra = id_usuario;
                    mov_g.fecha_registro = hoy;
                    mov_g.id_inventario_mov_tipo = 2; //SALIDA DE MERCANCIA|ENTREGA DE VALE
                    mov_g.descripcion_mov = info_tipo_mov.descripcion;
                    mov_g.id_inventario_mov_status = 2; // 2 = CONCLUIDO
                    mov_g.id_transaccion_solicitud_mercancia_g = id_solicitud_g;
                    db.C_inventario_almacen_mov_g.Add(mov_g);
                    db.SaveChanges();
                    id_mov_g = mov_g.id_inventario_almacen_mov_g;
                }
                else { id_mov_g = validSalida.id_inventario_almacen_mov_g; }


                for (int i = 0; i < id_solicitudes_d.Length; i++)
                {
                    int id_solicitud_d = id_solicitudes_d[i];
                    var soli_d = db.C_almacen_solicitudes_mercancia_d.Find(id_solicitud_d);

                    //CANDADO PARA VALIDAR LOS ENTREGADOS Y EVITAR DUPLICADOS AL ENTREGAR VALE EN 2 COMPUTADORAS
                    if (soli_d.entregado == false) {
                        soli_d.cantidad_entregada = cant_entrega[i] + soli_d.cantidad_entregada;
                        //soli_d.cantidad = soli_d.cantidad - cant_entrega[i];

                        if (soli_d.cantidad == soli_d.cantidad_entregada) { soli_d.entregado = true; }

                        if (soli_d.cantidad_entregada > soli_d.cantidad) { return 2; }

                        if (cant_entrega[i] > 0)
                        {
                            C_inventario_almacen_mov_d mov_d = new C_inventario_almacen_mov_d();
                            mov_d.id_inventario_almacen_mov_g = id_mov_g;
                            mov_d.id_articulo = soli_d.id_articulo;
                            mov_d.cantidad = cant_entrega[i];
                            mov_d.entrada_salida = info_tipo_mov.entrada_salida;
                            mov_d.id_almacen_g = id_alamcen_g;
                            mov_d.id_inventario_mov_status = 2; //CONCLUIDO
                            mov_d.fecha_registro = hoy;
                            db.C_inventario_almacen_mov_d.Add(mov_d);

                            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx//
                            //LOGS VALE_D
                            C_almacen_solicitudes_mercancia_d_logs merca_d_log = new C_almacen_solicitudes_mercancia_d_logs();
                            merca_d_log.fecha_registro = hoy;
                            merca_d_log.id_solicitud_mercancia_d = soli_d.id_solicitud_mercancia_d;
                            merca_d_log.id_solicitud_mercancia_g = id_solicitud_g;
                            merca_d_log.cantidad = cant_entrega[i];
                            merca_d_log.id_usuario_entrega = id_usuario;
                            merca_d_log.activo = true;
                            db.C_almacen_solicitudes_mercancia_d_logs.Add(merca_d_log);
                            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx//
                        }
                        db.SaveChanges();
                    }

                }

                //VALIDO TODOS LOS ARTICULOS ENTREGDOS
                var articulos = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g).Select(x => x.entregado).Distinct();
                if (!articulos.Contains(false))
                {
                    solicitud_g.id_status_solicitud = 3; //ENTREGADO
                    solicitud_g.atendida = true;
                    solicitud_g.id_usuario_entrega = id_usuario;
                    solicitud_g.fecha_entrega = hoy;
                    db.SaveChanges();
                }



                
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }
        public int CerrarVale(int id_solicitud_g)
        {
            try
            {
                var mercancia_d = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true && x.id_solicitud_mercancia_g == id_solicitud_g && x.cantidad_entregada > 0).ToList();
                if (mercancia_d.Count() > 0)
                {
                    DateTime hoy = DateTime.Now;
                    var solicitud_g = db.C_almacen_solicitudes_mercancia_g.Find(id_solicitud_g);
                    int id_usuario = (int)Session["LoggedId"];
                    solicitud_g.id_status_solicitud = 3;
                    solicitud_g.atendida = true;
                    solicitud_g.id_usuario_entrega = id_usuario;
                    solicitud_g.fecha_entrega = hoy;
                    db.SaveChanges();
                    return 0;
                }
                else
                {
                    return 1;
                }



            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }
        public int CancelarVale(int id_solicitud_g)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                var solicitud_g = db.C_almacen_solicitudes_mercancia_g.Find(id_solicitud_g);

                var mercancia_d = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true && x.id_solicitud_mercancia_g == id_solicitud_g && x.cantidad_entregada > 0).ToList();
                if (mercancia_d.Count() != 0)
                {
                    return 1;
                }
                else
                {
                    int id_usuario = (int)Session["LoggedId"];
                    solicitud_g.id_status_solicitud = 1002;
                    solicitud_g.atendida = true;
                    solicitud_g.id_usuario_entrega = id_usuario;
                    solicitud_g.fecha_entrega = hoy;
                    db.SaveChanges();
                    return 0;
                }




            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public PartialViewResult CambiarCuentaVale(int id_solicitud_g)
        {
            var vales = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g && x.cantidad_entregada == 0);
            return PartialView("EntregaVales/_CambiarCuentaVale", vales);
        }

        public string ConfirmarCambioCuentaVale(int[] vale_d, int[] cargo, int[] cuenta, string[] cargocuenta)
        {
            string accion = "True";
            try
            {
                for (int i = 0; i < vale_d.Length; i++)
                {
                    var mercancia_d = db.C_almacen_solicitudes_mercancia_d.Find(vale_d[i]);
                    if (mercancia_d.cantidad_entregada != 0)
                    {
                        accion = "0";
                    }
                    else
                    {
                        mercancia_d.id_cargo_contable_g = cargo[i];
                        mercancia_d.id_cuenta_contable_g = cuenta[i];
                        mercancia_d.cargo_cuenta_contable = cargocuenta[i];
                        db.SaveChanges();
                    }
                }
                return accion;
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public PartialViewResult GenerarValePDF(int id_solicitud_g)
        {
            var detalle = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == id_solicitud_g).ToList();
            return PartialView("EntregaVales/_PDFVale", detalle);
        }

        public string ConsultarExistenciaArticuloAlmacen(int id_almacen_g, int id_articulo)
        {
            try
            {
                bool InvCapturaInventario = false;

                decimal entradas = 0;
                decimal salidas = 0;
                decimal mermas = 0;
                decimal entradas_trapaso = 0;
                decimal salidas_traspaso = 0;

                decimal salidas_ajuste = 0;
                decimal entradas_ajuste = 0;
                decimal entradas_devolucion = 0;

                decimal CantidadCapturada = 0;

                C_inventario_almacen_captura_g valid_inventario_captura = null;

                var articulo_capturado = db.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo && x.activo == true && x.C_inventario_almacen_captura_g.id_almacen == id_almacen_g 
                                        && x.C_inventario_almacen_captura_g.activo == true && x.C_inventario_almacen_captura_g.id_status_inventario == 1).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();
                //var articulo_capturado = db.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo && x.activo == true && x.C_inventario_almacen_captura_g.id_almacen == id_almacen_g).OrderByDescending(x => x.id_inventario_d).FirstOrDefault();
                if (articulo_capturado != null)
                {
                    CantidadCapturada = (decimal)articulo_capturado.cantidad_captura;
                    InvCapturaInventario = true;
                    valid_inventario_captura = articulo_capturado.C_inventario_almacen_captura_g;
                }

                if (InvCapturaInventario == true && valid_inventario_captura != null) // EXISTE UNA CAPTURA DE INVENTARIO
                {
                    var entradas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();   //ENTRADAS POR COMPRA
                    if (entradas_registros.Count() > 0) { entradas = entradas_registros.Sum(x => x.cantidad).Value; }

                    var salidas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();  //ENTREGAS DE VALES
                    if (salidas_registros.Count() > 0) { salidas = salidas_registros.Sum(x => x.cantidad).Value; }

                    var mermas_registros = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();  //MERMAS
                    if (mermas_registros.Count() > 0) { mermas = mermas_registros.Sum(x => x.cantidad).Value; }

                    var entradas_trap = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.id_articulo == id_articulo).ToList();
                    if (entradas_trap.Count() > 0) { entradas_trapaso = entradas_trap.Sum(x => x.cantidad).Value; }

                    var salidas_trap = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.id_articulo == id_articulo).ToList();
                    if (salidas_trap.Count() > 0) { salidas_traspaso = salidas_trap.Sum(x => x.cantidad).Value; }


                    //-------AJUSTES 
                    var entradas_ajust = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1003 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();  
                    if (entradas_ajust.Count() > 0) { entradas_ajuste = entradas_ajust.Sum(x => x.cantidad).Value; }

                    var salidas_ajust = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1004 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                    if (salidas_ajust.Count() > 0) { salidas_ajuste = salidas_ajust.Sum(x => x.cantidad).Value; }

                    //----- DEVOLUCIONES
                    var entradas_devoluciones = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1005 && x.id_articulo == id_articulo && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                    if (entradas_devoluciones.Count() > 0) { entradas_devolucion = entradas_devoluciones.Sum(x => x.cantidad).Value; }


                    decimal existencia = (CantidadCapturada + entradas + entradas_trapaso + entradas_ajuste + entradas_devolucion) - salidas - mermas - salidas_traspaso - salidas_ajuste;
                    return Convert.ToDecimal(existencia).ToString();
                }

                else
                {
                    return "0";
                }
            }
            catch (Exception)
            {
                return "---";
            }

        }

        public PartialViewResult ConsultarExistenciaPorAlmacenArticulo(int clave)
        {
            var almacen = db.C_almacen_almacenes_g.Where(x => x.activo == true);
            List<ExistenciaAlmacenArticulo> existencia = new List<ExistenciaAlmacenArticulo>();
            foreach (var item in almacen)
            {
                int id_almacen = item.id_almacen_g;
                ExistenciaAlmacenArticulo exist = new ExistenciaAlmacenArticulo();
                exist.almacen = item.siglas;
                exist.existencia = Convert.ToDecimal(ConsultarExistenciaArticuloAlmacen(id_almacen, clave));

                var valid = db.C_compras_ordenes_d.Where(x => x.id_articulo == clave && x.activo == true && x.entregado == false);
                if (valid.Count() > 0)
                {
                    foreach (var orden in valid)
                    {
                        var valid_ubicaciones = orden.C_compras_ordenes_g.C_compras_ordenes_ubicaciones_entrega.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_almacen_g == id_almacen);
                        if (valid_ubicaciones.Count() > 0)
                        {
                            foreach (var art_almacen in valid_ubicaciones)
                            {
                                exist.transito += (decimal)orden.cantidad_compra - (decimal)orden.cantidad_entregada;
                            }
                        }
                    }
                }

                existencia.Add(exist);
            }
            return PartialView("../ALMACEN/SolicitudesMercancia/_ExistenciaAlmacenArticulo", existencia);
        }



        #region DEVOLUCION DE VALES
        public PartialViewResult ConsultarValeDevolucion(int id_almacen_g, int id_vale)
        {
            var data_vale = db.C_almacen_solicitudes_mercancia_g.Find(id_vale);
            if (data_vale == null) { return PartialView("EntregaVales/_DevolucionNuevaValeTable", null); }
            if (data_vale.id_almacen_g != id_almacen_g) { return PartialView("EntregaVales/_DevolucionNuevaValeTable", null); }

            return PartialView("EntregaVales/_DevolucionNuevaValeTable", data_vale);
        }

        public int DevolucionVale(int id_solicitud_mercancia_d_log)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var log_salida = db.C_almacen_solicitudes_mercancia_d_logs.Find(id_solicitud_mercancia_d_log);
                if (log_salida.activo == false) { return 1; }
                log_salida.activo = false;
                int id_salid_d = (int)log_salida.id_solicitud_mercancia_d;

                var salida_d = db.C_almacen_solicitudes_mercancia_d.Find(id_salid_d);
                decimal cantidad_original = (decimal)salida_d.cantidad;
                decimal cantidad_devuelta = (decimal)log_salida.cantidad;

                salida_d.cantidad_entregada = salida_d.cantidad_entregada - log_salida.cantidad;
                salida_d.entregado = false;
                db.SaveChanges();

                // VALIDO TODOS LOS ARTICULOS ENTREGDOS
                var solicitud_g = db.C_almacen_solicitudes_mercancia_g.Find((int)log_salida.id_solicitud_mercancia_g);
                var articulos = db.C_almacen_solicitudes_mercancia_d.Where(x => x.id_solicitud_mercancia_g == log_salida.id_solicitud_mercancia_g).Select(x => x.entregado).Distinct();
                if (!articulos.Contains(false))
                {
                    solicitud_g.id_status_solicitud = 3; //ENTREGADO
                    solicitud_g.atendida = true;
                }
                else
                {
                    solicitud_g.id_status_solicitud = 4; //AUTORIZADO
                    solicitud_g.atendida = false;
                }
                db.SaveChanges();

                C_inventario_almacen_mov_g devolucion_g = new C_inventario_almacen_mov_g();
                C_inventario_almacen_mov_d devolucion_d = new C_inventario_almacen_mov_d();
                devolucion_g.id_almacen_origen = salida_d.C_almacen_solicitudes_mercancia_g.id_almacen_g;
                devolucion_g.id_usuario_registra = id_usuario;
                try
                {
                    devolucion_g.fecha_registro = DateTime.Now;
                    devolucion_g.id_inventario_mov_tipo = 1005;
                    devolucion_g.descripcion_mov = "Devolucion";
                    devolucion_g.id_inventario_mov_status = 2;
                    db.C_inventario_almacen_mov_g.Add(devolucion_g);
                    int id_devolucion_g = devolucion_g.id_inventario_almacen_mov_g;

                    devolucion_d.id_inventario_almacen_mov_g = id_devolucion_g;
                    devolucion_d.id_articulo = salida_d.id_articulo;
                    devolucion_d.cantidad = log_salida.cantidad;
                    devolucion_d.entrada_salida = true;
                    devolucion_d.id_almacen_g = devolucion_g.id_almacen_origen;
                    devolucion_d.fecha_registro = devolucion_g.fecha_registro;
                    devolucion_d.id_inventario_mov_status = 2;
                    db.C_inventario_almacen_mov_d.Add(devolucion_d);
                    db.SaveChanges();

                    try
                    {
                        int id_dev_g_log = 0;
                        var valid_log = db.C_inventario_almacen_devoluciones_g.Where(x => x.id_solicitud_mercancia_g == salida_d.id_solicitud_mercancia_g).FirstOrDefault();
                        if (valid_log == null)
                        {
                            C_inventario_almacen_devoluciones_g log_dev_g = new C_inventario_almacen_devoluciones_g();
                            log_dev_g.fecha_registro = DateTime.Now;
                            log_dev_g.id_usuario_registra = id_usuario;
                            log_dev_g.id_almacen = devolucion_g.id_almacen_origen;
                            log_dev_g.id_solicitud_mercancia_g = log_salida.id_solicitud_mercancia_g;
                            log_dev_g.activo = true;
                            db.C_inventario_almacen_devoluciones_g.Add(log_dev_g);
                            db.SaveChanges();
                            id_dev_g_log = (int)log_dev_g.id_inventario_devolucion_g;
                        }
                        else { id_dev_g_log = (int)valid_log.id_inventario_devolucion_g; }

                        if (id_dev_g_log != 0)
                        {
                            C_inventario_almacen_devoluciones_d log_dev_d = new C_inventario_almacen_devoluciones_d();
                            log_dev_d.id_inventario_devolucion_g = id_dev_g_log;
                            log_dev_d.id_solicitud_mercancia_d = salida_d.id_solicitud_mercancia_d;
                            log_dev_d.id_solicitud_mercancia_d_log = id_solicitud_mercancia_d_log;
                            log_dev_d.cantidad_original = cantidad_original;
                            log_dev_d.cantidad_devuelta = cantidad_devuelta;
                            log_dev_d.activo = true;
                            db.C_inventario_almacen_devoluciones_d.Add(log_dev_d);
                            db.SaveChanges();
                        }

                    }
                    catch (Exception)
                    {

                    }

                }
                catch (Exception)
                {
                    try
                    {
                        devolucion_g.id_inventario_mov_status = 3;
                        db.SaveChanges();

                        devolucion_d.id_inventario_mov_status = 3;
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        //public PartialViewResult ConsultarDevolucionesTable(int id_almacen_g, string fecha_inicio, string fecha_fin)
        //{
        //    var devoluciones = db.C_
        //}


        #endregion


        #endregion


        #region CAPTURA DE MOVIMIENTOS
        public ActionResult IndexInventario()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(16)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }


            return View("Inventario/Index");
        }

        public PartialViewResult ConsultarMovimientosInventarioTable(int id_almacen_g, int[] tipo_movimiento, string fecha_inicio, string fecha_fin)
        {
            if (tipo_movimiento.Contains(0))
            {
                var tmovimiento = from mov in db.C_inventario_mov_tipos where mov.activo == true select mov.id_inventario_mov_tipo;
                tipo_movimiento = tmovimiento.ToArray();
            }

            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var movimientos = db.C_inventario_almacen_mov_g.Where(x => x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && tipo_movimiento.Contains((int)x.id_inventario_mov_tipo) && (x.id_almacen_origen == id_almacen_g || x.id_almacen_destino == id_almacen_g)).ToList();
            return PartialView("../ALMACEN/Inventario/MovimientosInventario/_MovimientosInventarioTable", movimientos);
        }

        public PartialViewResult ConsultarDetalleMovimientoInventario(int id_mov_g)
        {
            var detalle = db.C_inventario_almacen_mov_d.Where(x => x.id_inventario_almacen_mov_g == id_mov_g).ToList();
            return PartialView("../ALMACEN/Inventario/MovimientosInventario/_MovimientoInventarioDetalle", detalle);
        }

        public PartialViewResult ConsultarArticulosAlmacenCapturaMov(int id_almacen_g, int id_tipo_articulo)
        {
            //var articulos = db.C_inventario_almacenes_articulos.Where(x => x.activo == true && x.id_almacen == id_almacen_g && x.C_articulos_catalogo.activo == true 
            //&& x.C_articulos_catalogo.id_articulo_tipo == id_tipo_articulo).ToList();
            //var articulos = db.C_articulos_catalogo.Where(x => x.id_articulo_tipo == id_tipo_articulo && x.activo == true && x.id_articulo_tipo_requisicion == 1
            //&& x.almacenable == true);
            //foreach (var articulo in articulos)
            //{
            //    int id_articulo = articulo.id_articulo;
            //    articulo.descripcion_articulo = ConsultarExistenciaArticuloAlmacen(id_almacen_g, id_articulo);
            //}
            var articulos = ConsultarInventarioAlmacenTipoArticulo(id_almacen_g, id_tipo_articulo.ToString(), DateTime.Now, 0); //MODO 0: SIN PRECIOS NI UBICACION
            return PartialView("../ALMACEN/Inventario/MovimientosInventario/_ArticulosAlmacen", articulos);
        }

        public int RegistrarTraspasoInventario(int id_almacen_g, int id_almacen_destino, int id_tipo_mov, int[] id_articulos, decimal[] cantidades, string[] obs)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                var info_tipo_mov = db.C_inventario_mov_tipos.Find(id_tipo_mov);

                int id_usuario = (int)Session["LoggedId"];
                C_inventario_almacen_mov_g mov_g = new C_inventario_almacen_mov_g();
                mov_g.id_almacen_origen = id_almacen_g;
                mov_g.id_almacen_destino = id_almacen_destino;

                mov_g.id_usuario_registra = id_usuario;
                mov_g.fecha_registro = hoy;
                mov_g.id_inventario_mov_tipo = id_tipo_mov;
                mov_g.descripcion_mov = info_tipo_mov.descripcion;
                mov_g.id_inventario_mov_status = 1; //EN PROCESO
                db.C_inventario_almacen_mov_g.Add(mov_g);
                db.SaveChanges();
                int id_movimiento_g = mov_g.id_inventario_almacen_mov_g;

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_inventario_almacen_mov_d mov_d = new C_inventario_almacen_mov_d();
                    mov_d.id_inventario_almacen_mov_g = id_movimiento_g;
                    mov_d.id_articulo = id_articulos[i];
                    mov_d.cantidad = cantidades[i];
                    mov_d.observaciones = obs[i];
                    mov_d.entrada_salida = info_tipo_mov.entrada_salida;
                    mov_d.id_almacen_g = id_almacen_g;
                    mov_d.id_inventario_mov_status = 1; //CONCLUIDO
                    mov_d.fecha_registro = hoy;
                    db.C_inventario_almacen_mov_d.Add(mov_d);
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public bool ValidarRecepcionTraspaso(int id_almacen_destino)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var valid = db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario && x.id_almacen_g == id_almacen_destino && x.activo == true).FirstOrDefault();
                if (valid != null) { return true; }
                return false;
            }
            catch (Exception ex)
            {
                string ms = ex.ToString();
                return false;
            }
        }

        public bool RecibirTraspasoInventario(int id_mov_g)
        {
            try
            {
                db.C_inventario_almacen_mov_g.Find(id_mov_g).id_inventario_mov_status = 2;
                db.C_inventario_almacen_mov_d.Where(x => x.id_inventario_almacen_mov_g == id_mov_g && x.id_inventario_mov_status == 1).ToList().ForEach(x => x.id_inventario_mov_status = 2);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        #endregion


        #region CAPTURA INVENTARIO
        
        #region CAPTURA DE INVENTARIO POR TIPO DE ARTICULO
        public PartialViewResult ConsultarCapturaInventarioDiferencias(int id_almacen_g, string id_tipo_inventario)
        {
            try
            {
                DateTime hoy = DateTime.Today.AddHours(23).AddMinutes(59);
                var captura = ConsultarInventarioAlmacenTipoArticulo(id_almacen_g, id_tipo_inventario, hoy, 1); //MODO 1: CON PRECIOS CON UBICACION
                return PartialView("../ALMACEN/Inventario/CapturaInventario/_CapturaInventarioDiferencias", captura.OrderBy(x => x.Nombre_articulo.Trim()));
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                ViewBag.ex_msj = msj;
                return PartialView("../ALMACEN/Inventario/CapturaInventario/_CapturaInventarioDiferencias", null);
            }
        }

        public int RegistrarCapturaInventario(int id_almacen_g, int[] id_articulos, decimal[] cantidades_sistema, decimal[] cantidades_fisico, decimal[] diferencias, string obs, int id_tipo_inventario)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                C_inventario_almacen_captura_g inv_g = new C_inventario_almacen_captura_g();
                inv_g.id_almacen = id_almacen_g;
                inv_g.fecha_registro = DateTime.Now;
                inv_g.id_usuario_registra = id_usuario;
                inv_g.id_tipo_inventario = 1; //INVENTARIO NORMAL
                inv_g.id_status_inventario = 1; //CONCLUIDO
                inv_g.observaciones = obs;
                inv_g.activo = true;
                inv_g.id_tipo_articulos_inventario = id_tipo_inventario;
                db.C_inventario_almacen_captura_g.Add(inv_g);
                db.SaveChanges();
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_inventario_almacen_captura_d inv_d = new C_inventario_almacen_captura_d();
                    inv_d.id_inventario_g = inv_g.id_inventario_g;
                    inv_d.id_articulo = id_articulos[i];
                    inv_d.cantidad_captura = cantidades_fisico[i];
                    inv_d.cantidad_sistema = cantidades_sistema[i];
                    inv_d.diferencia = inv_d.cantidad_captura - inv_d.cantidad_sistema;
                    inv_d.activo = true;
                    db.C_inventario_almacen_captura_d.Add(inv_d);
                }
                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return -1;
            }

        }

        public string RegistrarCapturaInventarioExcel()
        {
            int id_almacen_g = 3; //TN
            int id_inventario_g = 6269;
            List<string> Claves_erorr = new List<string>();

            List<string> Claves_articulos = new List<string>();
            List<string> Existencia_articulos = new List<string>();
            List<string> Ubicaciones_articulos = new List<string>();
            try
            {
                int count_success = 0;

                string filePath = "C:\\Users\\aleja\\Contacts\\INVENTARIO_TN_4.1.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(); // Get the first worksheet

                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;

                            // Find the index of the column with header name "CATEGORIA"
                            int ClaveColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "CLAVE")
                                {
                                    ClaveColumnIndex = col;
                                    break;
                                }
                            }

                            // Find the index of the column with header name "EXISTENCIA"
                            int ExistenciaColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "EXISTENCIA")
                                {
                                    ExistenciaColumnIndex = col;
                                    break;
                                }
                            }

                            int UbicacionColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "UBICACION")
                                {
                                    UbicacionColumnIndex = col;
                                    break;
                                }
                            }


                            if (ClaveColumnIndex != -1 && ExistenciaColumnIndex != -1 /*&& UbicacionColumnIndex != -1*/)
                            {
                                // Llena los datos de la columna "CLAVE"
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, ClaveColumnIndex].Value?.ToString();
                                    Claves_articulos.Add(cellValue);
                                }

                                // Llena los datos de la columna "EXISTENCIAS"
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, ExistenciaColumnIndex].Value?.ToString();
                                    Existencia_articulos.Add(cellValue);
                                }

                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, UbicacionColumnIndex].Value?.ToString();
                                    Ubicaciones_articulos.Add(cellValue);
                                }

                                var valid_count = db.C_articulos_catalogo.Where(x => Claves_articulos.Contains(x.clave));
                                //List<int> id_tipos = valid_count.Select(x => (int)x.id_articulo_tipo).Distinct().ToList();
                                List<string> found = valid_count.Select(x => x.clave).ToList();
                                List<string> not_fount = Claves_articulos.Except(found).ToList();

                                //var arts = valid_count.Select(x => x.id_articulo);
                                //db.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen_g && arts.Contains((int)x.id_articulo)).ToList().ForEach(x => x.activo = false);
                                //db.SaveChanges();

                                //DateTime hoy = DateTime.Now;
                                //C_inventario_almacen_captura_g cap_g = new C_inventario_almacen_captura_g();
                                //cap_g.id_almacen = id_almacen_g; //CSI
                                //cap_g.fecha_registro = hoy;
                                //cap_g.id_usuario_registra = 1;
                                //cap_g.id_tipo_inventario = 1; //INVENTARIO NORMAL
                                //cap_g.id_status_inventario = 1; //CONCLUIDO
                                //cap_g.observaciones = "CAPTURA DE INVENTARIO POR EXCEL 01-10-2025";
                                //cap_g.activo = true;
                                //cap_g.id_tipo_articulos_inventario = 8; //EQUIPO MED
                                //db.C_inventario_almacen_captura_g.Add(cap_g);
                                //db.SaveChanges();
                                //id_inventario_g = cap_g.id_inventario_g;

                                for (int i = 0; i < Claves_articulos.Count(); i++)
                                {
                                    string clave_articulo = "";
                                    try
                                    {
                                        decimal existencia = Convert.ToDecimal(Existencia_articulos[i]);
                                        clave_articulo = Claves_articulos[i];
                                        var articulo = db.C_articulos_catalogo.Where(x => x.clave.Trim() == clave_articulo.Trim()).FirstOrDefault();
                                        int id_articulo = articulo.id_articulo;

                                        var valid = db.C_inventario_almacen_captura_d.Where(x => x.id_inventario_g == id_inventario_g && x.id_articulo == id_articulo && x.activo == true).FirstOrDefault();
                                        if (valid != null)
                                        {
                                            valid.cantidad_captura = existencia;
                                            valid.diferencia = valid.cantidad_captura - valid.cantidad_sistema;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            C_inventario_almacen_captura_d captura = new C_inventario_almacen_captura_d();
                                            captura.id_inventario_g = id_inventario_g;
                                            captura.id_articulo = id_articulo;
                                            captura.cantidad_captura = existencia;
                                            captura.cantidad_sistema = Convert.ToDecimal(ConsultarExistenciaArticuloAlmacen(id_almacen_g, id_articulo));
                                            captura.diferencia = captura.cantidad_captura - captura.cantidad_sistema;
                                            captura.activo = true;
                                            db.C_inventario_almacen_captura_d.Add(captura);
                                            db.SaveChanges();
                                        }

                                        //string ubicacion_articulo = Ubicaciones_articulos[i].Trim();
                                        //int id_ubicacion_almacen = 0;
                                        //var ubicacion = db.C_almacen_ubicaciones_articulos.Where(x => x.id_almacen == id_almacen_g && x.nombre_ubicacion == ubicacion_articulo && x.activo == true).FirstOrDefault();
                                        //if (ubicacion != null)
                                        //{
                                        //    id_ubicacion_almacen = (int)ubicacion.id_ubicacion_almacen;
                                        //}
                                        //else
                                        //{
                                        //    C_almacen_ubicaciones_articulos new_ubicacion = new C_almacen_ubicaciones_articulos();
                                        //    new_ubicacion.nombre_ubicacion = ubicacion_articulo;
                                        //    new_ubicacion.id_almacen = id_almacen_g;
                                        //    new_ubicacion.activo = true;
                                        //    db.C_almacen_ubicaciones_articulos.Add(new_ubicacion);
                                        //    db.SaveChanges();
                                        //    id_ubicacion_almacen = new_ubicacion.id_ubicacion_almacen;
                                        //}

                                        //try
                                        //{
                                        //    var valid_art_ubi = db.C_inventario_almacenes_articulos.Where(x => x.id_articulo == id_articulo && x.id_almacen == id_almacen_g && x.activo == true).FirstOrDefault();
                                        //    if (valid_art_ubi == null)
                                        //    {
                                        //        C_inventario_almacenes_articulos art_ubi = new C_inventario_almacenes_articulos();
                                        //        art_ubi.id_almacen = id_almacen_g;
                                        //        art_ubi.id_articulo = id_articulo;
                                        //        art_ubi.id_ubicacion_almacen = id_ubicacion_almacen;
                                        //        art_ubi.activo = true;
                                        //        db.C_inventario_almacenes_articulos.Add(art_ubi);
                                        //        db.SaveChanges();
                                        //    }
                                        //    else
                                        //    {
                                        //        valid_art_ubi.id_ubicacion_almacen = id_ubicacion_almacen;
                                        //        valid_art_ubi.activo = true;
                                        //        db.SaveChanges();
                                        //    }
                                        //}
                                        //catch (Exception){}

                                        count_success++;
                                    }
                                    catch (Exception)
                                    {
                                        Claves_erorr.Add(clave_articulo.ToString());
                                    }
                                }

                                //response += "<table>";
                                //foreach (var item in articulos_tipos)
                                //{
                                //    response += "<tr>" +
                                //           "<td style='text-align: center;'>" + item.clave + "</td>" +
                                //           "<td style='text-align: center;'>" + item.nombre_articulo + "</td>" +
                                //           "<td style='text-align: center;'>" + item.C_articulos_tipos.nombre_tipo + "</td>" +
                                //           "</tr>";
                                //}
                                //response += "</table>";
                                //return response;


                                if (Claves_articulos.Count() == count_success)
                                {
                                    return "OK";
                                }
                                else
                                {
                                    return Newtonsoft.Json.JsonConvert.SerializeObject(Claves_erorr);
                                }

                            }
                            else
                            {
                                //NO SE DETECTÓ LA COLUMNA "CLAVE" o "EXISTENCIA"
                                return "NO SE DETECTÓ LA COLUMNA \"CLAVE\" o \"EXISTENCIA";
                            }
                        }
                        else
                        {
                            //NO SE DETECTÓ UNA HOJA PARA LEER
                            return "NO SE DETECTÓ UNA HOJA PARA LEER";
                        }
                    }
                    else
                    {
                        //NO SE DETECTÓ EL ARCHIVO PARA LEER
                        return "NO SE DETECTÓ EL ARCHIVO PARA LEER";
                    }
                }


            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }
        }

        private string SyncProveedoresExcel()
        {
            List<string> Cuentas_prov = new List<string>();
            try
            {
                int count_success = 0;

                string filePath = "C:\\Users\\aleja\\Contacts\\PROVEEDORES.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(); // Get the first worksheet

                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;

                            // Find the index of the column with header name "CATEGORIA"
                            int ClaveColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "CUENTA")
                                {
                                    ClaveColumnIndex = col;
                                    break;
                                }
                            }


                            if (ClaveColumnIndex != -1)
                            {
                                // Llena los datos de la columna "CLAVE"
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, ClaveColumnIndex].Value?.ToString();
                                    Cuentas_prov.Add(cellValue);
                                }

                                var valid_count = db.C_compras_proveedores.Where(x => Cuentas_prov.Contains(x.cuenta_cxp));

                                var found = valid_count.Select(x => x.cuenta_cxp).ToList();
                                List<string> not_fount = Cuentas_prov.Except(found).ToList();

                                //db.C_compras_proveedores.ForEach(x => x.activo = false);
                                //db.C_compras_proveedores.ForEach(x => x.id_proveedor_status = 2);
                                //db.SaveChanges();

                                //valid_count.ForEach(x => x.activo = true);
                                //valid_count.ForEach(x => x.id_proveedor_status = 1);
                                //db.SaveChanges();

                                return valid_count.Count().ToString();

                            }
                            else
                            {
                                //NO SE DETECTÓ LA COLUMNA "CLAVE" o "EXISTENCIA"
                                return "NO SE DETECTÓ LA COLUMNA \"CLAVE\" o \"EXISTENCIA";
                            }
                        }
                        else
                        {
                            //NO SE DETECTÓ UNA HOJA PARA LEER
                            return "NO SE DETECTÓ UNA HOJA PARA LEER";
                        }
                    }
                    else
                    {
                        //NO SE DETECTÓ EL ARCHIVO PARA LEER
                        return "NO SE DETECTÓ EL ARCHIVO PARA LEER";
                    }
                }


            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }
        }

        public string ImportarUbicacionesAlmacen()
        {
            int id_almacen = 3; //TANQUE NUEVO
            List<string> Claves = new List<string>();
            List<string> Not_fount = new List<string>();
            try
            {
                int count_success = 0;

                string filePath = "C:\\Users\\aleja\\Contacts\\UBICACION_ARTICULOS.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(); // Get the first worksheet

                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;
                            int ClaveColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "UBICACION")
                                {
                                    ClaveColumnIndex = col;
                                    break;
                                }
                            }

                            if (ClaveColumnIndex != -1)
                            {
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, ClaveColumnIndex].Value?.ToString();
                                    Claves.Add(cellValue);
                                }

                                int count_ubicaciones = Claves.Count();
                                Claves = Claves.Distinct().ToList();
                                int count_ubicaciones_2 = Claves.Count();

                                DateTime hoy = DateTime.Now;
                                for (int i = 0; i < Claves.Count(); i++)
                                {
                                    string clave = Claves[i].Trim();
                                    try
                                    {
                                        C_almacen_ubicaciones_articulos art = new C_almacen_ubicaciones_articulos();
                                        art.nombre_ubicacion = clave;
                                        art.id_almacen = id_almacen;
                                        art.activo = true;
                                        db.C_almacen_ubicaciones_articulos.Add(art);
                                        db.SaveChanges();
                                    }
                                    catch (Exception)
                                    {
                                        Not_fount.Add(clave);
                                    }
                                }
                                


                                //return response;

                                if (Not_fount.Count() == count_success)
                                {
                                    return Newtonsoft.Json.JsonConvert.SerializeObject(Claves);
                                }
                                else
                                {
                                    return Newtonsoft.Json.JsonConvert.SerializeObject(Not_fount);
                                }

                            }
                            else
                            {
                                //NO SE DETECTÓ LA COLUMNA "CLAVE" o "EXISTENCIA"
                                return "NO SE DETECTÓ LA COLUMNA \"CLAVE\" o \"EXISTENCIA";
                            }
                        }
                        else
                        {
                            //NO SE DETECTÓ UNA HOJA PARA LEER
                            return "NO SE DETECTÓ UNA HOJA PARA LEER";
                        }
                    }
                    else
                    {
                        //NO SE DETECTÓ EL ARCHIVO PARA LEER
                        return "NO SE DETECTÓ EL ARCHIVO PARA LEER";
                    }
                }




            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }
        }
        
        public string ImportarCuentasUsuariosExcel()
        {
            List<string> Cuentas = new List<string>();
            List<int> Usuarios = new List<int>();
            List<string> Cuentas_error = new List<string>();
            try
            {
                int count_success = 0;
                string filePath = "C:\\Users\\aleja\\OneDrive\\Documentos\\BETA\\CUENTAS USUARIOS\\Encargados.xlsx";
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(); // Get the first worksheet
                        if (worksheet != null)
                        {
                            int rowCount = worksheet.Dimension.Rows;
                            int columnCount = worksheet.Dimension.Columns;
                            // Find the index of the column with header name "CATEGORIA"
                            int CuentaColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "CUENTA")
                                {
                                    CuentaColumnIndex = col;
                                    break;
                                }
                            }

                            int IDColumnIndex = -1;
                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "ID")
                                {
                                    IDColumnIndex = col;
                                    break;
                                }
                            }

                            if (CuentaColumnIndex != -1 && IDColumnIndex != -1)
                            {
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    string cellValue = worksheet.Cells[row, CuentaColumnIndex].Value?.ToString();
                                    Cuentas.Add(cellValue);
                                }

                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, IDColumnIndex].Value?.ToString();
                                    Usuarios.Add(Convert.ToInt32(cellValue));
                                }

                                var valid_count = db.C_cuentas_contables_g.Where(x => Cuentas.Contains(x.cuenta) && x.activo == true);
                                for (int i = 0; i < Cuentas.Count(); i++)
                                {
                                    string cuenta = Cuentas[i];
                                    int id_cuenta = (int)valid_count.Where(x => x.cuenta == cuenta).Select(x => x.id_cuenta_contable).FirstOrDefault();
                                    int id_usuario = Usuarios[i];
                                    try
                                    {
                                        var valid = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();
                                        if (valid == null)
                                        {
                                            C_usuarios_cuentas_contables user_cuenta = new C_usuarios_cuentas_contables();
                                            user_cuenta.id_usuario = id_usuario;
                                            user_cuenta.id_cuenta_contable_g = id_cuenta;
                                            user_cuenta.activo = true;
                                            db.C_usuarios_cuentas_contables.Add(user_cuenta);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            valid.activo = true;
                                            db.SaveChanges();
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Cuentas_error.Add(Cuentas[i]);
                                    }
                                }
                                return Newtonsoft.Json.JsonConvert.SerializeObject(Cuentas_error);

                            }
                            else
                            {
                                //NO SE DETECTÓ LA COLUMNA "CLAVE" o "EXISTENCIA"
                                return "NO SE DETECTÓ LA COLUMNA \"CLAVE\" o \"EXISTENCIA";
                            }
                        }
                        else
                        {
                            return "NO SE DETECTÓ UNA HOJA PARA LEER";
                        }
                    }
                    else
                    {
                        //NO SE DETECTÓ EL ARCHIVO PARA LEER
                        return "NO SE DETECTÓ EL ARCHIVO PARA LEER";
                    }
                }
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }
        }




        #endregion


        #region AJUSTE DE INVENTARIO
        public string ConsultarInformacionArticuloAjuste(int id_almacen_g, string clave_nombre, int existencia)
        {
            List<string> data = new List<string>();
            var articulo = db.C_articulos_catalogo.Where(x => x.clave == clave_nombre).FirstOrDefault();
            if (articulo == null) { return "0"; }

            if (existencia == 0) { data.Add(ConsultarExistenciaArticuloAlmacen(id_almacen_g, (int)articulo.id_articulo)); }
            else { data.Add(""); }

            data.Add(articulo.id_articulo.ToString());
            data.Add(articulo.clave);
            data.Add(articulo.nombre_articulo);
            data.Add(articulo.C_articulos_tipos.nombre_tipo);
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public int RegistrarAjusteInventario(int id_almacen, int[] id_articulos, decimal[] exist_sistema, decimal[] exist_fisica, decimal[] ajuste, bool[] entrada_salida)
        {
            try
            {
                if(ajuste.Contains(0)) { return 1; }  //No puede hacer un ajuste por cantidad 0

                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];
                int id_entrada_g = 0;
                int id_salida_g = 0;
                if (entrada_salida.Contains(true)) //ENTRADA
                {
                    C_inventario_almacen_mov_g entrada_g = new C_inventario_almacen_mov_g();
                    entrada_g.id_almacen_origen = id_almacen;
                    entrada_g.id_usuario_registra = id_usuario;
                    entrada_g.fecha_registro = hoy;
                    entrada_g.id_inventario_mov_tipo = 1003; //ENTRADA POR AJUSTE
                    entrada_g.id_inventario_mov_status = 2;
                    entrada_g.descripcion_mov = "Entrada por ajuste";
                    db.C_inventario_almacen_mov_g.Add(entrada_g);
                    db.SaveChanges();
                    id_entrada_g = entrada_g.id_inventario_almacen_mov_g;
                }
                if (entrada_salida.Contains(false)) //SALIDA
                {
                    C_inventario_almacen_mov_g salida_g = new C_inventario_almacen_mov_g();
                    salida_g.id_almacen_origen = id_almacen;
                    salida_g.id_usuario_registra = id_usuario;
                    salida_g.fecha_registro = hoy;
                    salida_g.id_inventario_mov_tipo = 1004; //SALIDA POR AJUSTE
                    salida_g.id_inventario_mov_status = 2;
                    salida_g.descripcion_mov = "Salida por ajuste";
                    db.C_inventario_almacen_mov_g.Add(salida_g);
                    db.SaveChanges();
                    id_salida_g = salida_g.id_inventario_almacen_mov_g;
                }

                List<int> inventario_mov_d = new List<int>();
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    decimal valor_ajuste = ajuste[i];

                    C_inventario_almacen_mov_d entrada = new C_inventario_almacen_mov_d();
                    if (entrada_salida[i] == true) { entrada.id_inventario_almacen_mov_g = id_entrada_g; }
                    if (entrada_salida[i] == false) { entrada.id_inventario_almacen_mov_g = id_salida_g; }
                    entrada.id_articulo = id_articulos[i];
                    entrada.cantidad = Math.Abs(valor_ajuste);
                    entrada.entrada_salida = entrada_salida[i];
                    entrada.id_almacen_g = id_almacen;
                    entrada.id_inventario_mov_status = 2;
                    entrada.fecha_registro = hoy;
                    db.C_inventario_almacen_mov_d.Add(entrada);
                    db.SaveChanges();
                    inventario_mov_d.Add(entrada.id_inventario_almacen_mov_d);
                    
                    //if (valor_ajuste > 0 && id_entrada_g > 0) //ENTRADA
                    //{
                    //    C_inventario_almacen_mov_d entrada = new C_inventario_almacen_mov_d();
                    //    entrada.id_inventario_almacen_mov_g = id_entrada_g;
                    //    entrada.id_articulo = id_articulos[i];
                    //    entrada.cantidad = valor_ajuste;
                    //    entrada.entrada_salida = true;
                    //    entrada.id_almacen_g = id_almacen;
                    //    entrada.id_inventario_mov_status = 2;
                    //    entrada.fecha_registro = hoy;
                    //    db.C_inventario_almacen_mov_d.Add(entrada);
                    //    db.SaveChanges();
                    //    inventario_mov_d.Add(entrada.id_inventario_almacen_mov_d);
                    //}

                    //if (valor_ajuste < 0 && id_salida_g > 0)      //SALIDA
                    //{
                    //    C_inventario_almacen_mov_d salida = new C_inventario_almacen_mov_d();
                    //    salida.id_inventario_almacen_mov_g = id_salida_g;
                    //    salida.id_articulo = id_articulos[i];
                    //    salida.cantidad = Math.Abs(valor_ajuste);
                    //    salida.entrada_salida = false;
                    //    salida.id_almacen_g = id_almacen;
                    //    salida.id_inventario_mov_status = 2;
                    //    salida.fecha_registro = hoy;
                    //    db.C_inventario_almacen_mov_d.Add(salida);
                    //    db.SaveChanges();
                    //    inventario_mov_d.Add(salida.id_inventario_almacen_mov_d);
                    //}
                }

                C_inventario_almacen_ajustes_g ajuste_g = new C_inventario_almacen_ajustes_g();
                ajuste_g.fecha_registro = hoy;
                ajuste_g.id_usuario_registra = id_usuario;
                ajuste_g.id_almacen_ajuste = id_almacen;
                ajuste_g.activo = true;
                db.C_inventario_almacen_ajustes_g.Add(ajuste_g);
                db.SaveChanges();
                int id_ajuste_g = ajuste_g.id_inventario_ajuste_g;

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_inventario_almacen_ajustes_d ajuste_d = new C_inventario_almacen_ajustes_d();
                    ajuste_d.id_inventario_ajuste_g = id_ajuste_g;
                    ajuste_d.id_articulo_ajuste = id_articulos[i];
                    ajuste_d.id_inventario_almacen_mov_d = inventario_mov_d[i];
                    ajuste_d.existencia_sistema = exist_sistema[i];
                    ajuste_d.existencia_fisica = exist_fisica[i];
                    ajuste_d.ajuste = ajuste[i];
                    ajuste_d.entrada_salida = entrada_salida[i];
                    ajuste_d.activo = true;
                    db.C_inventario_almacen_ajustes_d.Add(ajuste_d);
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }

        }
        
        public PartialViewResult ConsultarAjustesInventarioHistorial(int id_almacen_g, string fecha_inicio, string fecha_fin)
        {
            DateTime fecha_1 = DateTime.Parse(fecha_inicio);
            DateTime fecha_2 = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(23);
            var ajustes = db.C_inventario_almacen_ajustes_g.Where(x => x.id_almacen_ajuste == id_almacen_g && x.fecha_registro >= fecha_1 && x.fecha_registro <= fecha_2 && x.activo == true).ToList();
            return PartialView("Inventario/AjusteInventario/_HistorialAjustesTable", ajustes);
        }

        public PartialViewResult ConsultarAjusteInventarioDetalle(int id_ajuste_g)
        {
            var detalle = db.C_inventario_almacen_ajustes_d.Where(x => x.id_inventario_ajuste_g == id_ajuste_g && x.activo == true).ToList();
            return PartialView("Inventario/AjusteInventario/_AjusteInventarioDetalle", detalle);
        }

        public int EliminarAjusteInventario(int id_ajuste_g)
        {
            try
            {
                var ajuste_g = db.C_inventario_almacen_ajustes_g.Find(id_ajuste_g);
                ajuste_g.activo = false;
                ajuste_g.C_inventario_almacen_ajustes_d.ToList().ForEach(x => x.activo = false);

                var id_movimientos_d = ajuste_g.C_inventario_almacen_ajustes_d.Select(x => x.id_inventario_almacen_mov_d).ToArray();
                var movimientos = db.C_inventario_almacen_mov_d.Where(x => id_movimientos_d.Contains(x.id_inventario_almacen_mov_d)).ToList();

                movimientos.ForEach(x => x.id_inventario_mov_status = 3);
                movimientos.Select(x => x.C_inventario_almacen_mov_g).ToList().ForEach(x => x.id_inventario_mov_status = 3);
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public PartialViewResult ImprimirAjusteInventario(int id_ajuste_g)
        {
            var data = db.C_inventario_almacen_ajustes_g.Find(id_ajuste_g);
            return PartialView("Inventario/AjusteInventario/_ImpresionAjusteInventario", data);
        }

        #endregion

        #endregion


        #region CONSULTAR INVENTARIOS
        public PartialViewResult ConsultarInventariosCapturados(int[] id_almacen_g, string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            if (id_almacen_g.Contains(0))
            {
                var almacenes = from alm in db.C_almacen_almacenes_g
                                join us in db.C_almacen_almacenes_usuarios on alm.id_almacen_g equals us.id_almacen_g
                                where alm.activo == true && us.activo == true && us.id_usuario == id_usuario
                                orderby alm.nombre_almacen
                                select alm.id_almacen_g;
                id_almacen_g = almacenes.ToArray();
            }

            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var inventarios = db.C_inventario_almacen_captura_g.Where(x => x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f &&
            id_almacen_g.Contains((int)x.id_almacen) && x.activo == true).ToList();

            return PartialView("../ALMACEN/Inventario/ConsultarInventario/_InventariosCapturadosTable", inventarios);
        }

        public PartialViewResult ConsultarDetalleCapturaInventario(int id_inventario_g)
        {
            var detalle = db.C_inventario_almacen_captura_d.Where(x => x.id_inventario_g == id_inventario_g && x.activo == true).ToList();
            return PartialView("../ALMACEN/Inventario/ConsultarInventario/_InventarioCapturadoDetalleTable", detalle);
        }


        public List<CapturaInventarioDiferencias> ConsultarInventarioAlmacenTipoArticulo(int id_almacen_g, string id_tipo_inventario, DateTime fecha_inventario, int modo)
        {
            int id_tipo = Convert.ToInt32(id_tipo_inventario);
            if (id_almacen_g != 3)
            {
                bool InvCapturaInventario = false;
                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen_g && x.activo == true && x.id_tipo_articulos_inventario == id_tipo
                                                && x.fecha_registro <= fecha_inventario && x.id_status_inventario == 1).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();
                if (valid_inventario_captura != null) { InvCapturaInventario = true; }

                List<C_inventario_almacen_mov_d> entradas = null;
                List<C_inventario_almacen_mov_d> salidas = null;
                List<C_inventario_almacen_mov_d> mermas = null;
                List<C_inventario_almacen_mov_d> entr_trasp = null;
                List<C_inventario_almacen_mov_d> salid_trasp = null;

                List<C_inventario_almacen_mov_d> entradas_devoluciones = null;
                List<C_inventario_almacen_mov_d> entradas_ajuste = null;
                List<C_inventario_almacen_mov_d> salidas_ajuste = null;

                //--------------------- FILTRO DE ARTÍCULOS POR UBICACIÓN
                //IEnumerable<C_articulos_catalogo> articulos_almacen = null;
                //if (id_almacen_g != 3)  //TN
                //{
                //    articulos_almacen = catalogos.ConsultarArticulosAlmacen(id_almacen_g, id_tipo);
                //}
                //else
                //{
                //    articulos_almacen = db.C_articulos_catalogo.Where(x => x.id_articulo_tipo == id_tipo && x.activo == true && x.id_articulo_tipo_requisicion == 1 && x.almacenable == true);
                //}

                var articulos_almacen = catalogos.ConsultarArticulosAlmacen(id_almacen_g, id_tipo);
                var id_articulos = articulos_almacen.Select(x => (int)x.id_articulo).ToArray();

                if (InvCapturaInventario == true) // EXISTE UNA CAPTURA DE INVENTARIO
                {
                    entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 //ENTRADAS POR COMPRA
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 //ENTREGAS DE VALES
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    mermas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 //MERMAS
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    //-------AJUSTES
                    entradas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1003  //ENTRADA POR AJUSTE
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salidas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1004  //SALIDA POR AJUSTE
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    //-----DEVOLUCIONES
                    entradas_devoluciones = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1005   //ENTRADAS POR DEVOLUCION
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    //----TRASPASOS
                    entr_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //ENTRADAS TRASPASO
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salid_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //SALIDAS TRASPASO
                    && id_articulos.Contains((int)x.id_articulo)
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                }

                //OBEJTO QUE ENGLOBA LA CPTURA DEL INVENTARIO CON ENTRADAS Y SALIDAS
                List<CapturaInventarioDiferencias> captura = new List<CapturaInventarioDiferencias>();
                //var articulos_almacen = db.C_articulos_catalogo.Where(x => x.almacenable == true && x.activo == true && x.id_articulo_tipo == id_tipo && x.id_articulo_tipo_requisicion == 1).ToList();

                if (InvCapturaInventario == true)  //EXISTE UNA CAPTURA DE INVENTARIO
                {
                    ViewBag.fecha_corta = String.Format(valid_inventario_captura.fecha_registro.Value.ToShortDateString(), "dd/MM/yyyy");
                    ViewBag.fecha_inventario = String.Format(valid_inventario_captura.fecha_registro.ToString(), "dd/MM/yyyy");
                    ViewBag.inv_inicial = false;
                    foreach (var item in articulos_almacen)
                    {
                        int id_articulo = (int)item.id_articulo;
                        CapturaInventarioDiferencias registro = new CapturaInventarioDiferencias();
                        registro.Clave = item.clave;
                        registro.Id_articulo = id_articulo;
                        registro.Nombre_articulo = item.nombre_articulo;
                        registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                        registro.Acepta_decimales = (int)item.C_unidades_medidas.acepta_decimal;
                        registro.Entradas = (decimal)entradas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salidas = (decimal)salidas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Mermas = (decimal)mermas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        registro.Entradas_trasp = (decimal)entr_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salidas_trasp = (decimal)salid_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        registro.Entradas_ajuste = (decimal)entradas_ajuste.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Salidas_ajuste = (decimal)salidas_ajuste.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                        registro.Entradas_devolucion = (decimal)entradas_devoluciones.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                        decimal CantidadCapturada = 0;
                        var articulo_capturado = valid_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo).FirstOrDefault();
                        if (articulo_capturado != null) { CantidadCapturada = (decimal)articulo_capturado.cantidad_captura; }
                        registro.Ultima_Cap = CantidadCapturada;


                        registro.Cantidad_sistema = (CantidadCapturada + registro.Entradas + registro.Entradas_trasp + registro.Entradas_ajuste + registro.Entradas_devolucion) - registro.Salidas - registro.Salidas_trasp - registro.Mermas - registro.Salidas_ajuste;

                        if (modo == 1)
                        {
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
                            registro.Precio = Convert.ToDecimal(articulos.BuscarPrecioArticulo(registro.Id_articulo));
                            registro.Total = registro.Precio * registro.Cantidad_sistema;
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
                        CapturaInventarioDiferencias registro = new CapturaInventarioDiferencias();
                        registro.Clave = item.clave;

                        registro.Id_articulo = id_articulo;
                        registro.Nombre_articulo = item.nombre_articulo;
                        registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                        registro.Entradas = 0;
                        registro.Salidas = 0;
                        registro.Mermas = 0;

                        registro.Entradas_trasp = 0;
                        registro.Salidas_trasp = 0;
                        registro.Entradas_ajuste = 0;
                        registro.Salidas_ajuste = 0;
                        registro.Entradas_devolucion = 0;

                        registro.Ultima_Cap = 0;
                        registro.Cantidad_sistema = 0;
                        registro.Precio = Convert.ToDecimal(articulos.BuscarPrecioArticulo(registro.Id_articulo));
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
                        registro.Total = 0;
                        captura.Add(registro);
                    }
                }
                return captura;
            }
            else
            {
                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen_g && x.activo == true && x.id_tipo_articulos_inventario == id_tipo
                                                && x.fecha_registro <= fecha_inventario && x.id_status_inventario == 1).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();
                if (valid_inventario_captura != null) { ViewBag.fecha_inventario = String.Format(valid_inventario_captura.fecha_registro.ToString(), "dd/MM/yyyy"); }
                else { ViewBag.fecha_inventario = "NO CAPTURADA"; }

                List<CapturaInventarioDiferencias> captura = new List<CapturaInventarioDiferencias>();
                try
                {
                    using (var conn = ConexionSIIB.Conectar())
                    {
                        SqlCommand cmd = new SqlCommand("usp_1_ALMACEN_InventarioExistenciaArticulosTipo", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@id_almacen", SqlDbType.VarChar));
                        cmd.Parameters.Add(new SqlParameter("@id_tipos_articulo", SqlDbType.VarChar));
                        cmd.Parameters.Add(new SqlParameter("@fecha_existencia", SqlDbType.VarChar));
                        cmd.Parameters.Add(new SqlParameter("@id_articulo_exist", SqlDbType.VarChar));
                        cmd.Parameters["@id_almacen"].Value = id_almacen_g;
                        cmd.Parameters["@id_tipos_articulo"].Value = string.Join(",", id_tipo_inventario);
                        cmd.Parameters["@fecha_existencia"].Value = fecha_inventario.ToString("yyyy-MM-dd hh:mm:ss");
                        cmd.Parameters["@id_articulo_exist"].Value = "0";
                        cmd.CommandTimeout = 600;

                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            CapturaInventarioDiferencias data = new CapturaInventarioDiferencias();
                            data.Id_articulo = Convert.ToInt32(reader["Id_articulo"]);
                            data.Clave = reader["Clave"].ToString();
                            data.Nombre_articulo = reader["Articulo"].ToString();
                            data.Unidad_medida = reader["Unid_medida"].ToString();
                            data.Acepta_decimales = Convert.ToInt32(reader["Acepta_decimal"]);
                            data.Ubicacion = reader["Ubicacion"].ToString();
                            data.Tipo_articulo = reader["Tipo"].ToString();
                            data.Clasificacion_articulo = reader["Clasificacion"].ToString();
                            data.Fecha_ultimo_inv = reader["Ult_inv"].ToString();
                            data.Ultima_Cap = Convert.ToDecimal(reader["Inv_inicial"]);

                            data.Entradas = Convert.ToDecimal(reader["Entradas_compr"]);
                            data.Entradas_ajuste = Convert.ToDecimal(reader["Entradas_ajust"]);
                            data.Entradas_trasp = Convert.ToDecimal(reader["Entradas_trasp"]);
                            data.Entradas_devolucion = Convert.ToDecimal(reader["Devoluciones"]);

                            data.Mermas = Convert.ToDecimal(reader["Mermas"]);
                            data.Salidas = Convert.ToDecimal(reader["Salidas_vale"]);
                            data.Salidas_trasp = Convert.ToDecimal(reader["Salidas_trasp"]);
                            data.Salidas_ajuste = Convert.ToDecimal(reader["Salidas_ajust"]);

                            data.Cantidad_sistema = Convert.ToDecimal(reader["Existencia"]);
                            data.Precio = Convert.ToDecimal(reader["Costo_articulo"]);
                            data.Total = Convert.ToDecimal(reader["Importe"]);
                            captura.Add(data);
                        }
                        reader.Close();
                        conn.Close();
                    }
                    return captura;
                }
                catch (Exception ex)
                {
                    return captura;
                }
            }
        }

        public PartialViewResult ConsultarInventarioFecha(int id_almacen_g, string id_tipo_articulos, string fecha)
        {
            try
            {
                DateTime fecha_consulta = DateTime.Parse(fecha).AddHours(23).AddMinutes(59);

                var captura = ConsultarInventarioAlmacenTipoArticulo(id_almacen_g, id_tipo_articulos, fecha_consulta, 1); //MODO 1: CON PRECIOS CON UBICACION
                return PartialView("../ALMACEN/Inventario/ConsultarInventario/_InventarioFechaTable", captura);
            }
            catch (Exception)
            {
                return PartialView("../ALMACEN/Inventario/ConsultarInventario/_InventarioFechaTable", null);
            }
        }


        public PartialViewResult ConsultarDiferenciasInventarios(int id_almacen, string mes_inicio, string mes_fin, int[] id_articulos_tipo)
        {
            string fecha_inicio = mes_inicio + "-01";
            DateTime fecha_i = DateTime.Parse(fecha_inicio);

            string fecha_fin = mes_fin + "-01";
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            var inventario = db.C_inventario_almacen_captura_d.Where(x => x.C_inventario_almacen_captura_g.id_almacen == id_almacen && x.activo == true
                                && x.C_inventario_almacen_captura_g.activo == true && //x.diferencia != 0 && 
                                id_articulos_tipo.Contains((int)x.C_inventario_almacen_captura_g.id_tipo_articulos_inventario)
                                && x.C_inventario_almacen_captura_g.fecha_registro >= fecha_i && x.C_inventario_almacen_captura_g.fecha_registro <= fecha_f
                                && x.C_inventario_almacen_captura_g.id_tipo_inventario == 1 && x.C_inventario_almacen_captura_g.id_status_inventario == 1);

            foreach (var item in inventario.Select(x => x.id_articulo).Distinct())
            {
                int id_articulo = (int)item;
                inventario.Where(x => x.id_articulo == id_articulo).ToList().ForEach(x => x.C_articulos_catalogo.descripcion_articulo = articulos.BuscarPrecioArticulo(id_articulo));
            }
            return PartialView("../ALMACEN/Inventario/ConsultarInventario/_DiferenciasAnalsisiInventariosTable", inventario);
        }



        public decimal ConsultarExistenciaArticuloAlmacenFecha(int id_almacen_g, int id_articulo, DateTime fecha_inventario)
        {
            decimal inventario_final = 0;
            try
            {
                var valid_inventario_captura = db.C_inventario_almacen_captura_d.Where(x => x.C_inventario_almacen_captura_g.id_almacen == id_almacen_g && x.activo == true 
                                                    && x.id_articulo == id_articulo && x.C_inventario_almacen_captura_g.id_status_inventario == 1 && x.C_inventario_almacen_captura_g.activo == true).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();

                CapturaInventarioDiferencias registro = new CapturaInventarioDiferencias();
                List<C_inventario_almacen_mov_d> entradas = null;
                List<C_inventario_almacen_mov_d> salidas = null;
                List<C_inventario_almacen_mov_d> mermas = null;
                List<C_inventario_almacen_mov_d> entr_trasp = null;
                List<C_inventario_almacen_mov_d> salid_trasp = null;

                List<C_inventario_almacen_mov_d> entradas_ajuste = null;
                List<C_inventario_almacen_mov_d> salidas_ajuste = null;
                List<C_inventario_almacen_mov_d> entradas_devolucion = null;
                if (valid_inventario_captura != null) // EXISTE UNA CAPTURA DE INVENTARIO
                {
                    entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_inventario_mov_status == 2 //ENTREGAS DE VALES
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    mermas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 && x.id_inventario_mov_status == 2 //MERMAS
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    entr_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null && x.id_articulo == id_articulo
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //ENTRADAS TRASPASO
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salid_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null && x.id_articulo == id_articulo
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //SALIDAS TRASPASO
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    //AJUSTES
                    entradas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1003 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    salidas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1004 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                    entradas_devolucion = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1005 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();


                    registro.Entradas = (decimal)entradas.Sum(x => x.cantidad).Value;
                    registro.Salidas = (decimal)salidas.Sum(x => x.cantidad).Value;
                    registro.Mermas = (decimal)mermas.Sum(x => x.cantidad).Value;
                    registro.Entradas_trasp = (decimal)entr_trasp.Sum(x => x.cantidad).Value;
                    registro.Salidas_trasp = (decimal)salid_trasp.Sum(x => x.cantidad).Value;
                    registro.Entradas_ajuste = (decimal)entradas_ajuste.Sum(x => x.cantidad).Value;
                    registro.Salidas_ajuste = (decimal)salidas_ajuste.Sum(x => x.cantidad).Value;
                    registro.Entradas_devolucion = (decimal)entradas_devolucion.Sum(x => x.cantidad).Value;
                    decimal CantidadCapturada = (decimal)valid_inventario_captura.cantidad_captura;
                    registro.Cantidad_sistema = (CantidadCapturada + registro.Entradas + registro.Entradas_trasp + registro.Entradas_ajuste + registro.Entradas_devolucion) - registro.Salidas - registro.Mermas - registro.Salidas_trasp - registro.Salidas_ajuste;
                    inventario_final = (decimal)registro.Cantidad_sistema;
                }
                return inventario_final;
            }
            catch (Exception)
            {
                return inventario_final;
            }
        }


        #endregion


        #region INVENTARIO MINIMO DE ARTICULOS
        public PartialViewResult ConsultarInventarioArticulos(int id_almacen_g, int id_tipo_inventario)
        {
            try
            {
                ViewBag.almacenmin = id_almacen_g;
                var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true && x.id_articulo_tipo_requisicion == 1 && x.id_articulo_tipo == id_tipo_inventario).ToList();
                return PartialView("../ALMACEN/Inventario/InventarioMinimoMaximo/_InventarioMaxMin", articulos);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                ViewBag.exe_msj = msj;
                return PartialView("../ALMACEN/Inventario/InventarioMinimoMaximo/_InventarioMaxMin", null);
            }
        }

        public int RegistrarInventarioMinimoMaximo(int id_almacen_g, int id_articulos, decimal cantidad_minima, decimal cantidad_maxima, decimal cantidad_prom)
        {
            try
            {
                var maximo_minimo_almacen = db.C_inventario_almacen_establo_maximo_minimo.Where(x => x.id_almacen_g == id_almacen_g && x.id_articulo == id_articulos).FirstOrDefault();
                if (maximo_minimo_almacen != null)
                {
                    maximo_minimo_almacen.activo = true;
                    maximo_minimo_almacen.id_almacen_g = id_almacen_g;
                    maximo_minimo_almacen.id_articulo = id_articulos;
                    maximo_minimo_almacen.cantidad_minima = cantidad_minima;
                    maximo_minimo_almacen.cantidad_maxima = cantidad_maxima;
                    maximo_minimo_almacen.cantidad_promedio = cantidad_prom;
                    db.SaveChanges();
                }
                else
                {
                    C_inventario_almacen_establo_maximo_minimo articulos = new C_inventario_almacen_establo_maximo_minimo();
                    articulos.activo = true;
                    articulos.id_almacen_g = id_almacen_g;
                    articulos.id_articulo = id_articulos;
                    articulos.cantidad_minima = cantidad_minima;
                    articulos.cantidad_maxima = cantidad_maxima;
                    articulos.cantidad_promedio = cantidad_prom;
                    db.C_inventario_almacen_establo_maximo_minimo.Add(articulos);
                    db.SaveChanges();
                }
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int RegistrarInventarioMinimo(int id_almacen_g, string[] id_articulos, decimal[] cantidad_minima, decimal[] cantidad_maxima, decimal[] cantidad_prom)
        {
            try
            {
                var maximo_minimo_almacen = db.C_inventario_almacen_establo_maximo_minimo.Where(x => x.id_almacen_g == id_almacen_g && id_articulos.Contains(x.C_articulos_catalogo.clave)).ToList();
                db.C_inventario_almacen_establo_maximo_minimo.RemoveRange(maximo_minimo_almacen);
                db.SaveChanges();
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    var clave = id_articulos[i];
                    var id_articulo = db.C_articulos_catalogo.Where(x => x.clave == clave).Select(x => x.id_articulo).FirstOrDefault();
                    C_inventario_almacen_establo_maximo_minimo articulos = new C_inventario_almacen_establo_maximo_minimo();
                    articulos.activo = true;
                    articulos.id_almacen_g = id_almacen_g;
                    articulos.id_articulo = id_articulo;
                    articulos.cantidad_minima = cantidad_minima[i];
                    articulos.cantidad_maxima = cantidad_maxima[i];
                    articulos.cantidad_promedio = cantidad_prom[i];
                    db.C_inventario_almacen_establo_maximo_minimo.Add(articulos);
                }
                db.SaveChanges();

                return 1;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public bool NotificarMaximoMinimoAlmacen(int id_almacen)
        {
            try
            {
                List<string> NotificacionArticulos = new List<string>();

                string[] Articulos = new string[2];
                var almacenes_beta = db.C_almacen_almacenes_g.Where(x => x.activo == true).ToList();

                //var id_almacen_g = items.id_almacen_g;

                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen && x.activo == true && x.id_status_inventario == 1).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();

                List<C_inventario_almacen_mov_d> entradas = null;
                List<C_inventario_almacen_mov_d> salidas = null;
                List<C_inventario_almacen_mov_d> mermas = null;
                List<C_inventario_almacen_mov_d> entr_trasp = null;
                List<C_inventario_almacen_mov_d> salid_trasp = null;

                //COMPRA
                entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == true
                && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 
                && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                //VALES
                salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false
                && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2
                && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                //MERMA
                mermas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false
                && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3
                && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                //ENTRADA TRASPASO
                entr_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen && x.C_inventario_almacen_mov_g.id_almacen_origen != null
                && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();
                //SALIDA TRASPASO
                salid_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.C_inventario_almacen_mov_g.id_almacen_destino != null
                && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002
                && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                //OBJETO QUE ENGLOBA LA CAPTURA DEL INVENTARIO CON ENTRADAS Y SALIDAS
                var articulos_almacen = db.C_articulos_catalogo.Where(x => x.almacenable == true && x.activo == true && x.id_articulo_tipo_requisicion == 1).ToList();
                foreach (var item in articulos_almacen)
                {
                    int id_articulo = (int)item.id_articulo;
                    CapturaInventarioDiferencias registro = new CapturaInventarioDiferencias();
                    registro.Clave = item.clave;
                    registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                    registro.Id_articulo = id_articulo;
                    registro.Nombre_articulo = item.nombre_articulo;
                    registro.Entradas = (decimal)entradas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Salidas = (decimal)salidas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Mermas = (decimal)mermas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                    registro.Entradas_trasp = (decimal)entr_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Salidas_trasp = (decimal)salid_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                    decimal CantidadCapturada = 0;
                    var articulo_capturado = valid_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo).FirstOrDefault();
                    if (articulo_capturado != null) { CantidadCapturada = (decimal)articulo_capturado.cantidad_captura; }
                    registro.Ultima_Cap = CantidadCapturada;
                    registro.Cantidad_sistema = (CantidadCapturada + registro.Entradas + registro.Entradas_trasp) - registro.Salidas - registro.Salidas_trasp - registro.Mermas;

                    var articulo_id = item.id_articulo;
                    var minimo_maximo = db.C_inventario_almacen_establo_maximo_minimo.Where(x => x.activo == true && x.id_articulo == articulo_id && x.id_almacen_g == id_almacen).ToList();
                    if (minimo_maximo.Count() > 0)
                    {
                        registro.Minimo = (decimal)minimo_maximo.FirstOrDefault().cantidad_minima;
                        registro.Maximo = (decimal)minimo_maximo.FirstOrDefault().cantidad_maxima;
                        registro.Promedio = (decimal)minimo_maximo.FirstOrDefault().cantidad_promedio;
                    }
                    try
                    {
                        var valid_ubicacion = item.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen && x.activo == true).FirstOrDefault();
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


                }
                //foreach (var items in almacenes_beta)
                //{

                //}

                if (NotificacionArticulos.Count() > 0)
                {
                    var mensaje = "";
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ConsultarInventarioMaximoMinimo(int id_almacen_g, int id_tipo_inventario, int filtro)
        {
            try
            {
                bool InvCapturaInventario = false;
                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen_g && x.activo == true 
                                                && x.id_tipo_articulos_inventario == id_tipo_inventario && x.id_status_inventario == 1).OrderByDescending(x => x.id_inventario_g).FirstOrDefault();
                if (valid_inventario_captura != null) { InvCapturaInventario = true; }


                List<C_inventario_almacen_mov_d> entradas = null;
                List<C_inventario_almacen_mov_d> salidas = null;
                List<C_inventario_almacen_mov_d> mermas = null;
                List<C_inventario_almacen_mov_d> entr_trasp = null;
                List<C_inventario_almacen_mov_d> salid_trasp = null;
                if (InvCapturaInventario == true) // EXISTE UNA CAPTURA DE INVENTARIO
                {
                    entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 //ENTRADAS POR COMPRA
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                    salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 //ENTREGAS DE VALES
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                    mermas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_almacen_destino == null
                    && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 //MERMAS
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                    entr_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //ENTRADAS TRASPASO
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                    salid_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null
                    && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //SALIDAS TRASPASO
                    && x.fecha_registro >= (DateTime)valid_inventario_captura.fecha_registro).ToList();

                    //String.Format(inventario.fecha_registro.Value.ToShortDateString(), "dd/mm/aaaa")
                    ViewBag.fechacaptura = valid_inventario_captura.fecha_registro.Value;
                }

                //OBEJTO QUE ENGLOBA LA CPTURA DEL INVENTARIO CON ENTRADAS Y SALIDAS
                List<CapturaInventarioDiferencias> captura = new List<CapturaInventarioDiferencias>();
                var articulos_almacen = db.C_articulos_catalogo.Where(x => x.almacenable == true && x.activo == true && x.id_articulo_tipo == id_tipo_inventario && x.id_articulo_tipo_requisicion == 1).ToList();
                foreach (var item in articulos_almacen)
                {
                    int id_articulo = (int)item.id_articulo;

                    CapturaInventarioDiferencias registro = new CapturaInventarioDiferencias();
                    registro.Clave = item.clave;

                    registro.Id_articulo = id_articulo;
                    registro.Nombre_articulo = item.nombre_articulo;
                    registro.Unidad_medida = item.C_unidades_medidas.unidad_medida;
                    registro.Entradas = (decimal)entradas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Salidas = (decimal)salidas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Mermas = (decimal)mermas.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                    registro.Entradas_trasp = (decimal)entr_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    registro.Salidas_trasp = (decimal)salid_trasp.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                    decimal CantidadCapturada = 0;
                    var articulo_capturado = valid_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo).FirstOrDefault();
                    if (articulo_capturado != null) { CantidadCapturada = (decimal)articulo_capturado.cantidad_captura; }
                    registro.Ultima_Cap = CantidadCapturada;
                    registro.Cantidad_sistema = (CantidadCapturada + registro.Entradas + registro.Entradas_trasp) - registro.Salidas - registro.Salidas_trasp - registro.Mermas;

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

                    var articulo_id = item.id_articulo;
                    var minimo_maximo = db.C_inventario_almacen_establo_maximo_minimo.Where(x => x.activo == true && x.id_articulo == articulo_id && x.id_almacen_g == id_almacen_g).FirstOrDefault();
                    //valid_inventario_captura
                    var id_captura_g = valid_inventario_captura.id_inventario_g;
                    var detalle_captura = db.C_inventario_almacen_captura_d.Where(x => x.id_inventario_g == id_captura_g && x.id_articulo == id_articulo).FirstOrDefault();
                    if (detalle_captura != null)
                    {
                        if (filtro == 0)  //TODOS
                        {
                            if (minimo_maximo != null)
                            {
                                registro.Minimo = (decimal)minimo_maximo.cantidad_minima;
                                registro.Maximo = (decimal)minimo_maximo.cantidad_maxima;
                                registro.Promedio = (decimal)minimo_maximo.cantidad_promedio;
                                if (registro.Cantidad_sistema < registro.Minimo)
                                {
                                    registro.Color = "#eb3c3c";
                                }
                                else if (registro.Cantidad_sistema > registro.Minimo && registro.Cantidad_sistema < registro.Promedio)
                                {
                                    registro.Color = "#F1D06F";
                                }
                                else if (registro.Cantidad_sistema > registro.Promedio)
                                {
                                    registro.Color = "#90EE90";
                                }
                                else
                                {
                                    registro.Color = "#E7E7E7";
                                }
                            }
                            else
                            {
                                registro.Minimo = 0;
                                registro.Maximo = 0;
                                registro.Promedio = 0;
                                registro.Color = "#E7E7E7";
                            }


                            captura.Add(registro);
                        }
                        else if (filtro == 1)  //MAXIMOS
                        {
                            if (minimo_maximo != null)
                            {
                                registro.Minimo = (decimal)minimo_maximo.cantidad_minima;
                                registro.Maximo = (decimal)minimo_maximo.cantidad_maxima;
                                registro.Promedio = (decimal)minimo_maximo.cantidad_promedio;
                                if (registro.Cantidad_sistema < registro.Minimo)
                                {
                                    registro.Color = "#eb3c3c";
                                    captura.Add(registro);
                                }
                            }
                        }
                        else if (filtro == 2)  //PROMEDIO
                        {
                            if (minimo_maximo != null)
                            {
                                registro.Minimo = (decimal)minimo_maximo.cantidad_minima;
                                registro.Maximo = (decimal)minimo_maximo.cantidad_maxima;
                                registro.Promedio = (decimal)minimo_maximo.cantidad_promedio;
                                if (registro.Cantidad_sistema > registro.Minimo && registro.Cantidad_sistema < registro.Promedio)
                                {
                                    registro.Color = "#F1D06F";
                                    captura.Add(registro);
                                }
                            }
                        }
                        else//MINIMO
                        {
                            if (minimo_maximo != null)
                            {
                                registro.Minimo = (decimal)minimo_maximo.cantidad_minima;
                                registro.Maximo = (decimal)minimo_maximo.cantidad_maxima;
                                registro.Promedio = (decimal)minimo_maximo.cantidad_promedio;
                                registro.Color = "#90EE90";
                                captura.Add(registro);
                            }
                        }
                    }

                }
                return PartialView("../ALMACEN/Inventario/InventarioMinimoMaximo/_ReporteInventarioMaximoMinimo", captura);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                ViewBag.ex_msj = msj;
                return PartialView("../ALMACEN/Inventario/InventarioMinimoMaximo/_ReporteInventarioMaximoMinimo", null);
            }
        }
        #endregion


        #region RECEPCION DE REQUISICIONES
        public ActionResult IndexRecepcion()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6023)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("Recepcion/Index");
        }

        public PartialViewResult ConsultarOrdenEspecificaEntregar(int idorden, int almacen)
        {
            //2 SAN GABRIEL
            //1 SANTA MONICA

            if (almacen == 2)
            {
                almacen = 3;
            }
            else
            {
                almacen = 2;
            }
            var ordenesCompra = db.C_compras_ordenes_d
          .Where(x => x.id_compras_orden_g == idorden && x.entregado == false && x.cantidad_precarga > 0 && x.C_compras_ordenes_g.id_tipo_orden == 1 && x.C_compras_ordenes_g.id_compras_ubicacion_entrega == almacen)
          .ToList();
            ViewBag.modo_recepcion = 2;
            return PartialView("../ALMACEN/Recepcion/_OrdenCompraTable", ordenesCompra);
        }

        public PartialViewResult ConsultarOrdenPorEntregar(int id_almacen)
        {
            ViewBag.id_almacen = id_almacen;
            var ubicaciones_almacen = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_almacen_g == id_almacen && x.activo == true).Select(x => x.id_compras_ubicacion_entrega).ToArray();
            var ordenesCompra = db.C_compras_ordenes_g.Where(x => x.id_tipo_orden == 1 && ubicaciones_almacen.Contains((int)x.id_compras_ubicacion_entrega) && x.entregado == false && x.alta_proveedor == false && x.activo == true && x.id_status_orden != 3).Distinct().ToList();
            return PartialView("../ALMACEN/Recepcion/_OrdenCompraRecepcion", ordenesCompra);
        }
        public PartialViewResult OrdenRecepcionar(int id_orden_g, int id_almacen)
        {
            ViewBag.id_almacen = id_almacen;
            var ordenRecepcionar = db.C_compras_ordenes_d.
                Where(x => x.C_compras_ordenes_g.id_tipo_orden == 1 && x.entregado == false && x.activo == true && x.cantidad_entregada < x.cantidad_compra && 
                            x.C_compras_ordenes_g.id_compras_orden_g == id_orden_g).ToList();
            var precarga = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == id_orden_g && x.recibido == false && x.activo == true).ToList().Count();
            if (precarga > 0)
            {
                //PRECARGA
                ViewBag.OrdenModo = 2;
            }
            else
            {
                //DIRECTO
                ViewBag.OrdenModo = 1;
            }
            return PartialView("../ALMACEN/Recepcion/_OrdenCompraEntregaTable", ordenRecepcionar);
        }

        public PartialViewResult ConsultarOrdenRecepcionPendiente(int almacen)
        {
            var ubicaciones = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_almacen_g == almacen && x.activo == true).Select(x => x.id_compras_ubicacion_entrega).ToArray();
            var ordenesCompra = db.C_compras_ordenes_d.Where(x => x.entregado == false && x.C_compras_ordenes_g.id_tipo_orden == 1 && ubicaciones.Contains(x.C_compras_ordenes_g.id_compras_ubicacion_entrega) && x.activo == true && x.C_compras_ordenes_g.activo == true);
            return PartialView("../ALMACEN/Recepcion/_OrdenCompraPendienteTable", ordenesCompra);
        }

        public bool RegistrarCapturaMovimientoInventarioRecepcion(int modo, int id_almacen_recepcion, int[] idordend, bool[] entregados, decimal[] cantidades, int[] id_articulos, int orden_g, string factura_folio, bool remision)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;

                int id_tipo_mov = 1;
                var info_tipo_mov = db.C_inventario_mov_tipos.Find(id_tipo_mov);
                C_inventario_almacen_mov_g mov_g = new C_inventario_almacen_mov_g();
                mov_g.id_almacen_origen = id_almacen_recepcion;
                mov_g.id_usuario_registra = id_usuario;
                mov_g.fecha_registro = hoy;
                mov_g.id_inventario_mov_tipo = id_tipo_mov;
                mov_g.descripcion_mov = info_tipo_mov.descripcion;
                mov_g.id_inventario_mov_status = 2; //CONCLUIDO
                mov_g.id_transaccion_orden_g = orden_g;
                mov_g.remision_factura = remision;
                mov_g.folio_remi_fact = factura_folio;
                db.C_inventario_almacen_mov_g.Add(mov_g);
                db.SaveChanges();

                C_compras_ordenes_recepciones_g rep_g = new C_compras_ordenes_recepciones_g();
                rep_g.id_compras_orden_g = orden_g;
                rep_g.id_usuario_registra = id_usuario;
                rep_g.fecha_registro = hoy;
                rep_g.activo = true;
                rep_g.factura_remision_folio = factura_folio;
                if (factura_folio == null)
                {
                    rep_g.factura_remision_folio = "N/A";
                    rep_g.factura_remision = false;
                }
                else
                {
                    rep_g.factura_remision_folio = factura_folio;
                    rep_g.factura_remision = true;
                }
                db.C_compras_ordenes_recepciones_g.Add(rep_g);
                db.SaveChanges();
                int id_recepcion_g = rep_g.id_compras_orden_recepcion_g;

                //VALIDO QUE EXISTA UNA PRECARGA
                var valid_precarga = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == orden_g && x.recibido == false && x.activo == true).OrderByDescending(x => x.fecha_registro).FirstOrDefault();

                var articulos_cantidad = id_articulos.Length;
                for (int w = 0; w < idordend.Length; w++)
                {

                    int[] articulo_inventario = new int[id_articulos.Length];
                    if (entregados[w] == true)
                    {
                        articulo_inventario[w] = id_articulos[w];

                        var orden_d = db.C_compras_ordenes_d.Find(idordend[w]);
                        orden_d.cantidad_entregada += cantidades[w];

                        if (orden_d.cantidad_entregada >= orden_d.cantidad_compra)
                        {
                            orden_d.entregado = true;
                        }
                        orden_d.cantidad_precarga = 0;
                        db.SaveChanges();


                        //VALIDA RECEPCION TOTAL
                        var orden_g_entrega = db.C_compras_ordenes_d.Where(x => x.entregado == false && x.id_compras_orden_g == orden_g && x.activo == true).Count();
                        if (orden_g_entrega == 0)
                        {
                            var orden_entregado = db.C_compras_ordenes_g.Find(orden_g);
                            orden_entregado.entregado = true;
                            orden_entregado.id_usuario_recepciona = id_usuario;
                            orden_entregado.fecha_recepcion = hoy;
                            db.SaveChanges();
                            requisiciones.CambiarStatusRequisicion((int)orden_entregado.id_requisicion_articulo_g, 8);
                        }

                        for (int i = 0; i < articulo_inventario.Length; i++)
                        {
                            if (articulo_inventario[i] > 0)
                            {
                                C_inventario_almacen_mov_d mov_d = new C_inventario_almacen_mov_d();
                                mov_d.id_inventario_almacen_mov_g = mov_g.id_inventario_almacen_mov_g;
                                mov_d.id_articulo = articulo_inventario[i];
                                mov_d.cantidad = cantidades[i];
                                //mov_d.observaciones = obs[i];
                                mov_d.entrada_salida = info_tipo_mov.entrada_salida;
                                mov_d.id_almacen_g = id_almacen_recepcion;
                                mov_d.id_inventario_mov_status = 2; //CONCLUIDO
                                mov_d.fecha_registro = hoy;
                                db.C_inventario_almacen_mov_d.Add(mov_d);
                                //db.SaveChanges();

                                C_inventario_almacen_mov_d_recepciones_logs log_recepcion = new C_inventario_almacen_mov_d_recepciones_logs();
                                log_recepcion.id_inventario_almacen_mov_g = mov_g.id_inventario_almacen_mov_g;
                                log_recepcion.id_inventario_almacen_mov_d = mov_d.id_inventario_almacen_mov_d;
                                log_recepcion.id_articulo = mov_d.id_articulo;
                                log_recepcion.id_usuario_recepcion = id_usuario;
                                log_recepcion.cantidad_recepcion = cantidades[i];
                                log_recepcion.fecha_recepcion = hoy;
                                log_recepcion.folio_remision_factura = factura_folio;
                                log_recepcion.activo = true;
                                db.C_inventario_almacen_mov_d_recepciones_logs.Add(log_recepcion);
                                //db.SaveChanges();

                                C_compras_ordenes_recepciones_d rep_d = new C_compras_ordenes_recepciones_d();
                                rep_d.id_compras_orden_recepcion_g = id_recepcion_g;
                                rep_d.id_compras_orden_d = orden_d.id_compras_orden_d;
                                rep_d.cantidad_recepcion = cantidades[w];
                                rep_d.activo = true;
                                db.C_compras_ordenes_recepciones_d.Add(rep_d);
                                db.SaveChanges();

                                if (valid_precarga != null)
                                {
                                    var valid_d = valid_precarga.C_compras_ordenes_precarga_d.Where(x => x.id_compras_orden_d == orden_d.id_compras_orden_d && x.activo == true).FirstOrDefault();
                                    if (valid_d != null)
                                    {
                                        valid_d.id_compras_orden_recepcion_d = rep_d.id_compras_orden_recepcion_d;
                                        db.SaveChanges();
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        articulo_inventario[w] = 0;
                    }
                }
                if (modo == 2)  //EXISTE UNA PRECARGA
                {
                    if (valid_precarga != null)
                    {
                        valid_precarga.recibido = true;
                        valid_precarga.fecha_recepcion = hoy;
                        valid_precarga.id_compras_orden_recepcion_g = id_recepcion_g;
                        int id_recepcion = valid_precarga.id_precarga_g;
                        db.SaveChanges();

                        var precarga = db.C_compras_ordenes_precarga_d.Where(x => x.id_precarga_g == id_recepcion && idordend.Contains((int)x.id_compras_orden_d)).ToList();
                        int contador = 0;
                        foreach (var item in precarga)
                        {
                            item.cantidad_entrega = cantidades[contador];
                            contador++;
                            db.SaveChanges();
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool RegistrarCapturaMovimientoInventario(int id_almacen_g, int[] id_articulos, decimal[] cantidades, string factura_folio, bool remision)
        {
            try
            {
                bool validador = false;
                foreach (var item in cantidades)
                {
                    if (item > 0)
                    {
                        validador = true;
                    }
                }
                if (validador == true)
                {
                    int id_tipo_mov = 1;
                    var info_tipo_mov = db.C_inventario_mov_tipos.Find(id_tipo_mov);

                    C_inventario_almacen_mov_g mov_g = new C_inventario_almacen_mov_g();
                    int id_usuario = (int)Session["LoggedId"];
                    mov_g.id_almacen_origen = id_almacen_g;
                    mov_g.id_usuario_registra = id_usuario;
                    mov_g.fecha_registro = DateTime.Now;
                    mov_g.id_inventario_mov_tipo = id_tipo_mov;
                    mov_g.descripcion_mov = info_tipo_mov.descripcion;
                    mov_g.id_inventario_mov_status = 2; //CONCLUIDO
                                                        //mov_g.id_transaccion_orden_g = orden_g;
                    mov_g.remision_factura = remision;
                    mov_g.folio_remi_fact = factura_folio;
                    db.C_inventario_almacen_mov_g.Add(mov_g);
                    db.SaveChanges();

                    for (int i = 0; i < id_articulos.Length; i++)
                    {
                        if (cantidades[i] > 0)
                        {
                            C_inventario_almacen_mov_d mov_d = new C_inventario_almacen_mov_d();
                            mov_d.id_inventario_almacen_mov_g = mov_g.id_inventario_almacen_mov_g;
                            mov_d.id_articulo = id_articulos[i];
                            mov_d.cantidad = cantidades[i];
                            mov_d.entrada_salida = true;
                            mov_d.id_almacen_g = id_almacen_g;
                            mov_d.id_inventario_mov_status = 2;
                            mov_d.fecha_registro = DateTime.Now;
                            db.C_inventario_almacen_mov_d.Add(mov_d);
                        }
                    }
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                return false;
            }
        }


        #endregion


        #region CODIGOS QR DE ARTÍCULOS

        public ActionResult CodigosQRArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8090)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("CodigosQR/Index");
        }

        public PartialViewResult AgregarArticulosTiposCodigos(int id_tipo_articulo)
        {
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true && x.id_articulo_tipo == id_tipo_articulo).ToList();
            return PartialView("CodigosQR/_ArticulosTipoTable", articulos);
        }


        public PartialViewResult GenerarFormatosCodigosArticulos(int[] id_articulos, int modo)
        {
            List<C_articulos_catalogo> codigos_list = new List<C_articulos_catalogo>();
            var claves_articulo = db.C_articulos_catalogo.Where(x => id_articulos.Contains((int)x.id_articulo)).ToList();
            foreach (var art in claves_articulo)
            {
                string claveValida = art.clave.Trim();
                try
                {
                    BarcodeWriter barcodeWriter;
                    if (modo == 0)
                    {
                        barcodeWriter = new BarcodeWriter()
                        {
                            Format = BarcodeFormat.QR_CODE,
                            Options = new ZXing.Common.EncodingOptions
                            {
                                Width = 500,
                                Height = 350,
                                Margin = 3
                            }
                        };
                    }
                    else
                    {
                        barcodeWriter = new BarcodeWriter
                        {
                            Format = BarcodeFormat.CODE_128,
                            Options = new ZXing.Common.EncodingOptions
                            {
                                Width = 500,
                                Height = 350,
                                Margin = 3,
                                PureBarcode = true
                            }
                        };
                    }

                    Bitmap codigo_qr = barcodeWriter.Write(claveValida); // Asegurar que art.clave es válido
                    using (var ms = new MemoryStream())
                    {
                        C_articulos_catalogo new_codigo = new C_articulos_catalogo();

                        codigo_qr.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        new_codigo.descripcion_articulo = $"data:image/png;base64,{base64String}";
                        new_codigo.nombre_articulo = art.nombre_articulo;
                        new_codigo.clave = art.clave;
                        codigos_list.Add(new_codigo);
                    }
                }
                catch (Exception)
                {

                }
            }

            return PartialView("CodigosQR/_CodigosQR", codigos_list.Distinct().ToList());
        }

        #endregion


        #region CRITICIDAD DE ARTICULOS
        public ActionResult ConfigurarCriticidadArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8104)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("CriticidadArticulos/Index");    
        }

        public PartialViewResult ConsultarArticulosCriticidadAlmacen(int id_almacen, int id_articulo_tipo, int tipo_vista)
        {
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true && x.id_articulo_tipo_requisicion == 1 && x.id_articulo_tipo == id_articulo_tipo);
            ViewBag.id_almacen = id_almacen;
            ViewBag.tipo_vista = tipo_vista;
            return PartialView("CriticidadArticulos/_ArticulosCriticidadTable", articulos);
        }

        public int GuardarArticulosCriticosAlmacen(int id_almacen, int[] id_articulos, int[] id_t_csm, int[] id_t_ope)
        {
            try
            {
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    int id_articulo = id_articulos[i];
                    var valid = db.C_almacen_criticidad_articulos.Where(x => x.id_almacen == id_almacen && x.id_articulo == id_articulo).FirstOrDefault();
                    if (valid != null) { 
                        valid.activo = true; db.SaveChanges();
                        valid.id_criticidad_tipo_consumo = id_t_csm[i];
                        valid.id_criticidad_tipo_operacion = id_t_ope[i];
                        db.SaveChanges();
                    }
                    else
                    {
                        C_almacen_criticidad_articulos new_critico = new C_almacen_criticidad_articulos();
                        new_critico.id_almacen = id_almacen;
                        new_critico.id_articulo = id_articulo;
                        new_critico.id_criticidad_tipo_consumo = id_t_csm[i];
                        new_critico.id_criticidad_tipo_operacion = id_t_ope[i];
                        new_critico.activo = true;
                        db.C_almacen_criticidad_articulos.Add(new_critico);
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

        public PartialViewResult ConsultarInventarioCriticoExistencias(int id_almacen, int[] id_t_csm, int[] id_t_ope, string fecha_existencia)
        {
            DateTime fecha_i = DateTime.MinValue;
            if (fecha_existencia == "" || fecha_existencia == null) { fecha_i = DateTime.Today.AddHours(23).AddMinutes(59); }
            else { fecha_i = DateTime.Parse(fecha_existencia).AddHours(23).AddMinutes(59); }
            DataTable dt = new DataTable();

            string id_consumos = string.Join(",", id_t_csm);
            string id_operacion = string.Join(",", id_t_ope);
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_1_ALMACEN_InventarioCriticoExistencias", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_almacen", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@id_tipo_consumo", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@id_tipo_operacion", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_existencia", SqlDbType.VarChar));
                    cmd.Parameters["@id_almacen"].Value = id_almacen;
                    cmd.Parameters["@id_tipo_consumo"].Value = id_consumos;
                    cmd.Parameters["@id_tipo_operacion"].Value = id_operacion;
                    cmd.Parameters["@fecha_existencia"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                    SqlDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    reader.Close();
                    conn.Close();
                }
                return PartialView("Inventario/InventarioCritico/_InventarioCriticoTable", dt);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return PartialView("Inventario/InventarioCritico/_InventarioCriticoTable", null);
            }
        }

        #endregion





        #region REPORTE QUICK VIEW
        public ActionResult QuickViewArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8063)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("QuickViewArticulos/Index");            
        }

        public string ConsultarArticulosQuickView(string clave_nombre, int[] id_tipo_articulos)
        {
            if (id_tipo_articulos.Contains(0)) { id_tipo_articulos = catalogos.ConsultarTiposArticulosID(); }

            var articulos = (from art in db.C_articulos_catalogo
                                where art.activo == true && art.almacenable == true && art.clave != null && 
                                        (art.clave.ToString().Contains(clave_nombre) || art.nombre_articulo.Contains(clave_nombre)) && 
                                        id_tipo_articulos.Contains((int)art.id_articulo_tipo) && art.id_articulo_tipo_requisicion == 1
                             select new { art.id_articulo, art.nombre_articulo, art.clave, nombre_tipo = art.C_articulos_tipos.nombre_tipo }).Take(50).OrderBy(x => x.nombre_articulo);
            return Newtonsoft.Json.JsonConvert.SerializeObject(articulos);
        }

        public PartialViewResult ConsultarQuickViewArticulo(int id_articulo, int id_almacen)
        {
            try
            {
                QuickViewArticulo obj = new QuickViewArticulo();
                obj.articulo_info = db.C_articulos_catalogo.Find(id_articulo);
                obj.Existencia = ConsultarExistenciaArticuloAlmacen(id_almacen, id_articulo);

                //--------ULTIMAS COMPRAS
                var ultimas_compras = db.C_compras_ordenes_d.Where(x => x.id_articulo == id_articulo && x.activo == true && x.C_compras_ordenes_g.activo == true && x.C_compras_ordenes_g.id_status_orden != 3).
                                                                    OrderByDescending(x => x.C_compras_ordenes_g.fecha_registro).Take(10).ToList();
                obj.Ultimas_compras = ultimas_compras;

                //--------UBICACIONES
                var ubicacion = obj.articulo_info.C_inventario_almacenes_articulos.Where(x => x.id_articulo == id_articulo && x.id_almacen == id_almacen && x.activo == true).FirstOrDefault();
                if (ubicacion != null) { obj.Ubicacion = ubicacion.C_almacen_ubicaciones_articulos.nombre_ubicacion; }
                else { obj.Ubicacion = "NO ASIGNADA"; }

                //--------PRECIO ACTUAL
                if (ultimas_compras.Count() > 0) { obj.PrecioActual = (decimal)ultimas_compras.FirstOrDefault().precio_unitario; }
                else { obj.PrecioActual = 0; }

                //--------FECHA ULTIMO INVENTARIO
                var inventario = db.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo && x.C_inventario_almacen_captura_g.id_almacen == id_almacen).OrderByDescending(x => x.C_inventario_almacen_captura_g.fecha_registro).FirstOrDefault();
                if (inventario != null) { obj.Fecha_ult_inventario = string.Format(inventario.C_inventario_almacen_captura_g.fecha_registro.Value.ToShortDateString(), "dd/MM/yyyy"); }
                else { obj.Fecha_ult_inventario = "SIN CAPTURA"; }

                //--------MAXIMOS Y MINIMOS
                var maximo_minimos = obj.articulo_info.C_inventario_almacen_establo_maximo_minimo.Where(x => x.id_almacen_g == id_almacen).FirstOrDefault();
                if (maximo_minimos != null) { obj.Maximos_Minimos = maximo_minimos; }
                else { obj.Maximos_Minimos = null; }

                var requis_coritzadas_pendientes = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_articulo == id_articulo && x.orden_generada == false && x.activo == true).Select(x => x.C_compras_requi_d).Distinct().ToList();
                
                var requis_sin_cotizacion = db.C_compras_requi_d.Where(x => x.id_articulo == id_articulo && x.cotizado == null &&
                                            !requis_coritzadas_pendientes.Select(z => z.id_requisicion_articulo_d).Contains((int)x.id_requisicion_articulo_d));
                List<C_compras_requi_d> requis_pendientes = requis_coritzadas_pendientes.Union(requis_sin_cotizacion).ToList();

                obj.Articulos_Pendientes = requis_pendientes;

                return PartialView("QuickViewArticulos/_QuickViewArticulo", obj);
            }
            catch (Exception)
            {
                return PartialView("QuickViewArticulos/_QuickViewArticulo", null);
            }

        }


        #endregion

        #region REPORTE KARDEX ARTICULOS 
        public ActionResult KardexArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6031)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("KardexArticulos/Index");
        }

        public PartialViewResult ConsultarKardexArticulo(int[] id_articulo, string fecha_inicio, string fecha_fin, int[] id_tipo_mov, int id_almacen)
        {
            try
            {
                if (id_articulo.Contains(0))
                {
                    id_articulo = db.C_articulos_catalogo.Where(x => x.activo == true && x.id_articulo_tipo_requisicion == 1 && x.almacenable == true).Select(x => x.id_articulo).ToArray();
                }

                if (id_tipo_mov.Contains(0)) { id_tipo_mov = db.C_inventario_mov_tipos.Where(x => x.activo == true).Select(x => x.id_inventario_mov_tipo).ToArray(); }

                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                var movimientos = db.C_inventario_almacen_mov_d.Where(x => x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.C_inventario_almacen_mov_g.id_inventario_mov_status == 2 &&
                id_tipo_mov.Contains((int)x.C_inventario_almacen_mov_g.id_inventario_mov_tipo) && id_articulo.Contains((int)x.id_articulo) && x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen
                && x.id_inventario_mov_status == 2).ToList().OrderBy(x => x.fecha_registro);

                return PartialView("KardexArticulos/_MovimientosArticuloTable", movimientos);
            }
            catch (Exception)
            {
                return PartialView("", null);
            }
        }
        #endregion

        #region REPORTE ANALISIS DE CONSUMOS

        public ActionResult AnalisisConsumos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7032)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("AnalisisConsumos/Index");
        }

        public PartialViewResult ConsultarAnalisisConsumos(int id_almacen, string fecha_inicio, string fecha_fin, int id_tipos_articulos)
        {
            try
            {
                //if (id_tipos_articulos.Contains(0)) { id_tipos_articulos = db.C_articulos_tipos.Where(x => x.activo == true).Select(x => x.id_articulo_tipo).ToArray(); }
                List<REPORTE_ALMACEN_AnalisisConsumos> data = new List<REPORTE_ALMACEN_AnalisisConsumos>();
                DateTime fecha_1 = DateTime.Parse(fecha_inicio);
                DateTime fecha_2 = DateTime.Parse(fecha_fin).AddDays(1);
                DateTime hoy = DateTime.Now;

                ViewBag.dias_consumo = (fecha_2 - fecha_1).Days;
                ViewBag.fecha_inicio = string.Format(fecha_1.ToShortDateString(), "dd/MM/yyyy");
                ViewBag.fecha_fin = string.Format(DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59).ToShortDateString(), "dd/MM/yyyy");
                fecha_2 = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

                var valid_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen && x.activo == true && x.id_tipo_articulos_inventario == id_tipos_articulos 
                && x.id_status_inventario == 1 && x.fecha_registro <= fecha_1).OrderByDescending(x => x.fecha_registro).FirstOrDefault();

                var valid_ult_inventario_captura = db.C_inventario_almacen_captura_g.Where(x => x.id_almacen == id_almacen && x.activo == true && x.id_tipo_articulos_inventario == id_tipos_articulos
                && x.id_status_inventario == 1 && x.fecha_registro <= hoy).OrderByDescending(x => x.fecha_registro).FirstOrDefault();

                //-------------- CONSUMOS DEL ULTIMO INVENTARIO DE LA FECHA SELECCIONADA
                DateTime fecha_ultimo_inv = new DateTime(2024,06,20);
                ViewBag.Fecha_ultimo_inv = "N/A";
                if (valid_inventario_captura != null)
                {
                    fecha_ultimo_inv = (DateTime)valid_inventario_captura.fecha_registro.Value.Date;
                    ViewBag.Fecha_ultimo_inv = valid_inventario_captura.fecha_registro.Value.ToShortDateString();
                }

                //FECHA DEL ULTIMO INVENTARIO ACTUAL
                DateTime fecha_ultimo_inv_actual = fecha_ultimo_inv;
                if (valid_ult_inventario_captura != null) { fecha_ultimo_inv_actual = (DateTime)valid_ult_inventario_captura.fecha_registro; }

                //------------------- CONSUMOS EN EL RANGO DE FECHAS
                List<C_inventario_almacen_mov_d> entradas_rango = new List<C_inventario_almacen_mov_d>();
                List<C_inventario_almacen_mov_d> consumos_rango = new List<C_inventario_almacen_mov_d>();
                List<C_inventario_almacen_mov_d> salidas_rango = new List<C_inventario_almacen_mov_d>();
                DateTime fecha_fin_curso = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

                //------------------- FILTRO DE ARTÍCULOS CON UBICACIÓN ASIGNADA
                var articulos_inventario = catalogos.ConsultarArticulosAlmacen(id_almacen, id_tipos_articulos);
                var id_articulos = articulos_inventario.Select(x => (int)x.id_articulo).Distinct().ToArray();


                entradas_rango = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == true  //ENTRADAS POR COMPRA
                    && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos 
                    && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                    && x.fecha_registro >= fecha_1 && x.fecha_registro <= fecha_fin_curso).ToList();

                consumos_rango = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false  //SALIDAS DE VALES
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos
                && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                && x.fecha_registro >= fecha_1 && x.fecha_registro <= fecha_fin_curso).ToList();

                salidas_rango = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false  //SALIDAS QUE NO SON DE VALES
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo != 2 && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos
                && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                && x.fecha_registro >= fecha_1 && x.fecha_registro <= fecha_fin_curso).ToList();

                //------------------- CONSUMOS APARTIR DEL ULTIMO INVENTARIO DE LAS FECHAS SELECCIONADAS
                List<C_inventario_almacen_mov_d> entradas = new List<C_inventario_almacen_mov_d>();
                List<C_inventario_almacen_mov_d> consumos = new List<C_inventario_almacen_mov_d>();
                List<C_inventario_almacen_mov_d> salidas = new List<C_inventario_almacen_mov_d>();
                entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == true  //ENTRADAS POR COMPRA
                && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos
                && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                && x.fecha_registro >= fecha_ultimo_inv && x.fecha_registro <= hoy).ToList();

                consumos = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos //SALIDAS DE VALES
                && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                && x.fecha_registro >= fecha_ultimo_inv && x.fecha_registro <= hoy).ToList();

                salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.entrada_salida == false  //SALIDAS QUE NO SON DE VALES
                && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo != 2 && x.id_inventario_mov_status == 2 && x.C_articulos_catalogo.id_articulo_tipo == id_tipos_articulos
                && id_articulos.Contains(x.C_articulos_catalogo.id_articulo)
                && x.fecha_registro >= fecha_ultimo_inv && x.fecha_registro <= hoy).ToList();


                foreach (var articulo in articulos_inventario)
                {
                    int id_articulo = articulo.id_articulo;

                    decimal cant_entradas_inicio = (decimal)entradas.Where(x => x.id_articulo == id_articulo && x.fecha_registro <= fecha_1).Sum(x => x.cantidad).Value;
                    decimal cant_consumos_inicio = (decimal)consumos.Where(x => x.id_articulo == id_articulo && x.fecha_registro <= fecha_1).Sum(x => x.cantidad).Value;
                    decimal cant_salidas_inicio = (decimal)salidas.Where(x => x.id_articulo == id_articulo && x.fecha_registro <= fecha_1).Sum(x => x.cantidad).Value;

                    decimal CantidadCapturada = 0;
                    decimal CantidadCapturadaActual = 0;
                    if (valid_inventario_captura != null)
                    {
                        var captura = valid_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo && x.activo == true).FirstOrDefault();
                        if (captura != null) { CantidadCapturada = (decimal)captura.cantidad_captura; }
                    }
                    decimal inventario_inicio = CantidadCapturada + cant_entradas_inicio - cant_consumos_inicio - cant_salidas_inicio;

                    REPORTE_ALMACEN_AnalisisConsumos reporte = new REPORTE_ALMACEN_AnalisisConsumos();

                    //-------------- INVENTARIO INICIAL DE LAS FECHAS SELECCIONADAS
                    reporte.id_articulo = id_articulo;
                    reporte.Clave_articulo = articulo.clave;
                    reporte.Nombre_articulo = articulo.nombre_articulo;
                    reporte.Unidad_Medida = articulo.C_unidades_medidas.unidad_medida;

                    reporte.Inventario_inicio = inventario_inicio;

                    decimal cant_entradas_fin = (decimal)entradas_rango.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    decimal cant_consumos_fin = (decimal)consumos_rango.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;
                    decimal cant_salidas_fin = (decimal)salidas_rango.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad).Value;

                    reporte.Entradas = cant_entradas_fin;
                    reporte.Salidas = cant_consumos_fin;
                    reporte.Inventario_final = (reporte.Inventario_inicio + reporte.Entradas) - reporte.Salidas - reporte.Consumo;
                    reporte.Consumo = cant_consumos_fin;

                    //----------------- INVENTARIO ACTUAL AL DÍA DE HOY
                    decimal entradas_totales = (decimal)entradas.Where(x => x.id_articulo == id_articulo && x.fecha_registro >= fecha_ultimo_inv_actual && x.fecha_registro <= hoy).Sum(x => x.cantidad).Value;
                    decimal consumos_totales = (decimal)consumos.Where(x => x.id_articulo == id_articulo && x.fecha_registro >= fecha_ultimo_inv_actual && x.fecha_registro <= hoy).Sum(x => x.cantidad).Value;
                    decimal salidas_totales = (decimal)salidas.Where(x => x.id_articulo == id_articulo && x.fecha_registro >= fecha_ultimo_inv_actual && x.fecha_registro <= hoy).Sum(x => x.cantidad).Value;

                    if (valid_ult_inventario_captura != null)
                    {
                        var captura = valid_ult_inventario_captura.C_inventario_almacen_captura_d.Where(x => x.id_articulo == id_articulo && x.activo == true).FirstOrDefault();
                        if (captura != null) { CantidadCapturadaActual = (decimal)captura.cantidad_captura; }
                    }
                    decimal inventario_totales = CantidadCapturadaActual + entradas_totales - consumos_totales - salidas_totales;
                    reporte.Inventario_real = inventario_totales;

                    //----------------- UBICACIONES
                    try
                    {
                        var valid_ubicacion = articulo.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen && x.activo == true).FirstOrDefault();
                        if (valid_ubicacion != null)
                        {
                            reporte.Ubicacion = valid_ubicacion.C_almacen_ubicaciones_articulos.nombre_ubicacion;
                        }
                        else
                        {
                            reporte.Ubicacion = "NO ASIGNADA";
                        }
                    }
                    catch (Exception)
                    {
                        reporte.Ubicacion = "NO ASIGNADA";
                    }

                    var ultima_compra = db.C_inventario_almacen_mov_d.Where(x => x.id_articulo == id_articulo && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_status == 2
                                        && x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1).OrderByDescending(x => x.fecha_registro).FirstOrDefault();
                    if (ultima_compra != null)
                    {
                        try
                        {
                            reporte.Fecha_compra = String.Format(ultima_compra.C_inventario_almacen_mov_g.C_compras_ordenes_g.fecha_registro.Value.ToShortDateString(), "dd/mm/yyyy");
                            reporte.Clave_Proveedor = (int)ultima_compra.C_inventario_almacen_mov_g.C_compras_ordenes_g.C_compras_proveedores.id_compras_proveedor;
                            reporte.Proveedor = ultima_compra.C_inventario_almacen_mov_g.C_compras_ordenes_g.C_compras_proveedores.razon_social;
                            reporte.Cantidad_Compra = (decimal)ultima_compra.C_inventario_almacen_mov_g.C_compras_ordenes_g.C_compras_ordenes_d.Where(x => x.id_articulo == id_articulo).Sum(x => x.cantidad_compra);
                        }
                        catch (Exception) { }
                    }

                    reporte.Precio = Convert.ToDecimal(articulos.BuscarPrecioArticulo(id_articulo));
                    reporte.Importe = reporte.Precio * reporte.Inventario_final;
                    reporte.Tipo_Articulo = articulo.C_articulos_tipos.nombre_tipo;

                    data.Add(reporte);
                }
                return PartialView("AnalisisConsumos/_AnalisisConsumoReporte", data);
            }
            catch (Exception)
            {
                return PartialView("AnalisisConsumos/_AnalisisConsumoReporte", null);
            }
        }

        public PartialViewResult ConsultarAnalisisConsumosAnual(int id_almacen, int anio, int id_tipo_articulos)
        {
            try
            {
                List<REPORTE_ALMACEN_AnalisisConsumosAnual> data = new List<REPORTE_ALMACEN_AnalisisConsumosAnual>();

                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_1_ALMACEN_ConsumoTipoArticulosAnual", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_almacenes", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@id_tipos_articulo", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@anio", SqlDbType.VarChar));
                    cmd.Parameters["@id_almacenes"].Value = id_almacen;
                    cmd.Parameters["@id_tipos_articulo"].Value = id_tipo_articulos;
                    cmd.Parameters["@anio"].Value = anio;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        REPORTE_ALMACEN_AnalisisConsumosAnual obj = new REPORTE_ALMACEN_AnalisisConsumosAnual();
                        obj.No_mes = Convert.ToInt32(reader["No_mes"].ToString());
                        obj.Mes = reader["Mes"].ToString();
                        obj.Almacen = reader["Almacen"].ToString();
                        obj.Tipo = reader["Tipo"].ToString();
                        obj.Clave = reader["Clave"].ToString();
                        obj.Unidad_medida = reader["Unidad_Medida"].ToString();
                        obj.Ubicacion = reader["Ubicacion"].ToString();
                        obj.id_articulo = Convert.ToInt32(reader["Id_articulo"].ToString());
                        obj.Nombre_articulo = reader["Articulo"].ToString();
                        obj.Cantidad = Convert.ToDecimal(reader["Cantidad"].ToString());
                        data.Add(obj);
                    }
                    reader.Close();
                    conn.Close();
                }
                var grupos = data.GroupBy(x => x.id_articulo);
                foreach (var grupo in grupos)
                {
                    int idArticulo = grupo.Key;
                    var lista = grupo.ToList();

                    if (idArticulo == 110925)
                    {
                        string hol = "";
                    }

                    var existencia = ConsultarExistenciaArticuloAlmacen(id_almacen, idArticulo);
                    var precio = articulos.BuscarPrecioArticulo(idArticulo);

                    int count_meses = lista.Where(x => x.Cantidad > 0).Select(x => x.No_mes).Count();
                    decimal consumo_mensual = lista.Where(x => x.Cantidad > 0).Sum(x => x.Cantidad);
                    decimal promedioMensual = 0;
                    decimal promedioDias = 0;

                    if (count_meses > 0)
                    {
                        promedioMensual = consumo_mensual / count_meses;
                        promedioDias = Convert.ToDecimal(existencia) / (promedioMensual / 30);
                    }

                    foreach (var x in lista)
                    {
                        x.ExistenciaActual = existencia;
                        x.Precio = precio;
                        x.PromedioMensual = promedioMensual;
                        x.DiasInventario = promedioDias;
                    }
                }

                //foreach (var item in data.Select(x => x.id_articulo).Distinct())
                //{
                //    data.Where(x => x.id_articulo == item).ToList().ForEach(x => x.ExistenciaActual = ConsultarExistenciaArticuloAlmacen(id_almacen, item));
                //    data.Where(x => x.id_articulo == item).ToList().ForEach(x => x.Precio = articulos.BuscarPrecioArticulo(item));

                //    int count_meses = data.Where(x => x.id_articulo == item).Select(x => x.No_mes).Count();
                //    decimal consumo_mensual = data.Where(x => x.id_articulo == item && x.Cantidad > 0).Sum(x => x.Cantidad);
                //    data.Where(x => x.id_articulo == item).ToList().ForEach(x => x.PromedioMensual = count_meses / consumo_mensual);
                //}

                return PartialView("AnalisisConsumos/_AnalisisConsumoReporteAnual", data);
            }
            catch (Exception)
            {
                return PartialView("AnalisisConsumos/_AnalisisConsumoReporteAnual", null);
            }
        }



        public string ConsultarPreciosArticulos()
        {
            ARTICULOSController art = new ARTICULOSController();
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true);
            string result = "<table>";
            foreach (var item in articulos)
            {
                result += "<tr>" +
                    "<td>"+ item.clave +"</td>" +
                    "<td>" + item.nombre_articulo + "</td>" +
                    "<td>" + art.BuscarPrecioArticulo(item.id_articulo) + "</td>" +
                    "</tr>";
            }
            return result;
        }


        #endregion

        #region REPORTE PRESUPUESTO ALMACEN
        public ActionResult ReportePresupuestoAlmacen()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7039)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("ReportePresupuestoAlmacen/Index");
        }
        #endregion

        #region REPORTE SOLICITUDES DE MERCANCIA
        public ActionResult ReporteSolicitudesMercancia()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8090)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("ReporteSolicitudesMercancia/Index");
        }

        public PartialViewResult ConsultarReporteSolicitudesMercancia(int[] id_almacen, string fecha_inicio, string fecha_fin, int[] id_solicitante, int[] id_departamento, int[] id_equipo)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                if (id_almacen.Contains(0)) { id_almacen = catalogos.ConsultarAlmacenesUsuarioID(id_usuario); }
                if (id_departamento.Contains(0)) { id_departamento = catalogos.ConsultarDepartamentosValesAlmacenesID(); }
                if (id_solicitante.Contains(0)) { id_solicitante = catalogos.ConsultarEmpleadosNominaID(); }
                if (id_equipo.Contains(0)) { id_equipo = catalogos.ConsultarEquiposEstablosID(); }


                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

                var data = db.C_almacen_solicitudes_mercancia_d.Where(x => x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_i && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_f
                            && id_departamento.Contains((int)x.C_almacen_solicitudes_mercancia_g.id_departamento_g) && id_solicitante.Contains((int)x.C_almacen_solicitudes_mercancia_g.id_empleado_remitente)
                            && x.activo == true && x.C_almacen_solicitudes_mercancia_g.activo == true && x.cantidad_entregada > 0 && id_equipo.Contains((int)x.C_almacen_solicitudes_mercancia_g.id_establo_equipo))
                            .OrderBy(x => x.C_almacen_solicitudes_mercancia_g.fecha_registro);
                return PartialView("ReporteSolicitudesMercancia/_ReporteSolicitudesMercancia", data);
            }
            catch (Exception)
            {
                return PartialView("ReporteSolicitudesMercancia/_ReporteSolicitudesMercancia", null);
            }
        }



        #endregion

        #region POLIZAS ALMACEN
        public ActionResult PolizasContables()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8055)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("PolizasContables/Index");            
        }

        public string GenerarPolizasContablesAlmacenTXT(int id_almacen, string fecha_i, string fecha_f)
        {
            //SM : id_almacen = 2
            //SG : id_almacen = 1

            Random rand = new Random();
            DateTime hoy = DateTime.Today;
            DateTime fecha_inicio, fecha_fin;
            string fecha_actual;

            int no_referencia = rand.Next(380,999);
            try
            {
                fecha_inicio = DateTime.Parse(fecha_i);
                fecha_fin = DateTime.Parse(fecha_f).AddHours(23).AddMinutes(59);
                fecha_actual = string.Format(hoy.ToShortDateString(), "dd/mm/yyyy");
            }
            catch (Exception)
            {
                return "-2";
            }

            string filePath = "\\\\192.168.128.2\\inetpub\\PolizasContablesTXT\\POLIZAS_ALMACEN\\POLIZAS _" + no_referencia.ToString() + ".txt";

            var vales = db.C_almacen_solicitudes_mercancia_d.Where(x => x.C_almacen_solicitudes_mercancia_g.id_almacen_g == id_almacen && x.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_inicio && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_fin &&
                        x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002).ToList();

            string clave_moneda = "1".PadRight(20);

            string concepto_poliza = "C DEL " + string.Format(fecha_inicio.ToShortDateString(), "dd/mm/yyyy HH:mm") + " AL " + string.Format(fecha_fin.ToShortDateString(), "dd/mm/yyyy HH:mm") + "";
            string concepto_pad = concepto_poliza.PadRight(200);
            string tipo_poliza = "";
            if (id_almacen == 1) { tipo_poliza = "A"; }
            else if (id_almacen == 2) { tipo_poliza = "a"; }
            else if (id_almacen == 3) { tipo_poliza = "T"; }
            else
            {
                return "-3";
            }
            string poliza_header = "|1|"+ tipo_poliza +"," + fecha_actual + ",\"" + clave_moneda + "\",\"0000000000000000000\"" + ",\"" + concepto_pad + "\"";

            var cargos_vales = vales.Select(x => x.C_cargos_contables_g).Distinct();
            decimal total = 0;
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var cargos_contables in cargos_vales)
                {
                    writer.WriteLine(poliza_header);

                    //--------- CARGOS (DEBE)
                    foreach (var vale in vales.Where(x => x.id_cargo_contable_g == cargos_contables.id_cargo_contable_g).GroupBy(x => x.id_cuenta_contable_g))
                    {
                        string cuenta = vale.FirstOrDefault().C_cuentas_contables_g.cuenta;
                        decimal importe_cuenta = (decimal)vale.Where(x => x.cantidad_entregada > 0).Select(x => x.cantidad_entregada * x.costo).Sum();

                        var cuenta_split = cuenta.Split('.');
                        string cuenta_index_1 = "0";
                        string cuenta_index_2 = "0";
                        string cuenta_index_3 = "0";

                        cuenta_index_1 = cuenta_split[0];
                        try { cuenta_index_2 = cuenta_split[1]; }
                        catch (Exception) { }

                        try { cuenta_index_3 = cuenta_split[2]; }
                        catch (Exception) { }

                        string cuenta_concat = cuenta_index_1.ToString().PadLeft(9, '0') + "." + cuenta_index_2.ToString().PadLeft(9, '0') + "." + cuenta_index_3.ToString().PadLeft(9, '0');
                        string importe_format = importe_cuenta.ToString("F2").PadLeft(16, '0');

                        string referencia = no_referencia.ToString().PadLeft(10, '0');

                        string poliza_body = $"|1.1|\"{cuenta_concat}\",\"GRAL \",\"C\",{importe_format},\"{referencia}\",\"{concepto_pad}\"";
                        writer.WriteLine(poliza_body);
                    }


                    //-------- ASIENTOS (HABER)
                    foreach (var vale in vales.Where(x => x.id_cargo_contable_g == cargos_contables.id_cargo_contable_g).GroupBy(x => x.C_articulos_catalogo.id_articulo_tipo))
                    {
                        string cuenta = vale.FirstOrDefault().C_articulos_catalogo.C_articulos_tipos.cuenta;
                        if (cuenta != "")
                        {
                            decimal importe_cuenta = (decimal)vale.Where(x => x.cantidad_entregada > 0).Select(x => x.cantidad_entregada * x.costo).Sum();

                            var cuenta_split = cuenta.Split('.');
                            string cuenta_index_1 = "0";
                            string cuenta_index_2 = "0";
                            string cuenta_index_3 = "0";

                            cuenta_index_1 = cuenta_split[0];
                            //SAN GRABIEL
                            if (id_almacen == 2)
                            {
                                int cuenta_1 = Convert.ToInt32(cuenta_index_1) + 1;
                                cuenta_index_1 = cuenta_1.ToString();
                            }
                            if (id_almacen == 3)
                            {
                                int cuenta_1 = Convert.ToInt32(cuenta_index_1) + 2;
                                cuenta_index_1 = cuenta_1.ToString();
                                //AGRICOLA (TN)
                            }

                            if (id_almacen == 3)
                            {
                                try { cuenta_index_2 = cuenta_split[1]; }
                                catch (Exception) { }

                                if (cuenta_index_2 == "11") {  }
                                else { cuenta_index_2 = "5"; }
                            }
                            else {
                                try { cuenta_index_2 = cuenta_split[1]; }
                                catch (Exception) { }

                                try { cuenta_index_3 = cuenta_split[2]; }
                                catch (Exception) { }
                            }

                            string cuenta_concat = cuenta_index_1.ToString().PadLeft(9, '0') + "." + cuenta_index_2.ToString().PadLeft(9, '0') + "." + cuenta_index_3.ToString().PadLeft(9, '0');
                            string importe_format = importe_cuenta.ToString("F2").PadLeft(16, '0');

                            string referencia = no_referencia.ToString().PadLeft(10, '0');

                            string poliza_body = $"|1.1|\"{cuenta_concat}\",\"GRAL \",\"A\",{importe_format},\"{referencia}\",\"{concepto_pad}\"";
                            writer.WriteLine(poliza_body);

                            total += importe_cuenta;
                        }
                        else
                        {
                            string cuenta_not_found = cuenta;
                        }
                    }
                    //writer.WriteLine("TOTAL: " + total);
                }
                no_referencia++;
            }
            return "https://siib.beta.com.mx/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");

            //return "https://localhost:44371/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");
        }

        public PartialViewResult GenerarPolizasAlmacenSemanal(int[] id_almacen_g_poliza, string fecha_i, string fecha_f)
        {
            if (id_almacen_g_poliza.Contains(0)) { return PartialView("../ALMACEN/PolizasContables/_PolizaSemanal", null); }
            DateTime fecha_inicio = DateTime.Parse(fecha_i);
            DateTime fecha_fin = DateTime.Parse(fecha_f).AddHours(23).AddMinutes(59);

            var entrada_ordenes_d = from mov_g in db.C_inventario_almacen_mov_g
                                    join mov_d in db.C_inventario_almacen_mov_d on mov_g.id_inventario_almacen_mov_g equals mov_d.id_inventario_almacen_mov_g
                                    join ord_g in db.C_compras_ordenes_g on mov_g.id_transaccion_orden_g equals ord_g.id_compras_orden_g
                                    join ord_d in db.C_compras_ordenes_d on ord_g.id_compras_orden_g equals ord_d.id_compras_orden_g
                                    where ord_d.activo == true && ord_g.activo == true && ord_d.cantidad_entregada > 0 && id_almacen_g_poliza.Contains((int)mov_g.id_almacen_origen)
                                    && mov_d.fecha_registro >= fecha_inicio && mov_d.fecha_registro <= fecha_fin
                                    select ord_d;


            var salidas = db.C_almacen_solicitudes_mercancia_d.Where(x => id_almacen_g_poliza.Contains((int)x.C_almacen_solicitudes_mercancia_g.id_almacen_g) && x.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_inicio && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_fin &&
                        x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002).ToList();

            var cuentas_contables = salidas.Select(x => x.C_cuentas_contables_g).Union(entrada_ordenes_d.Select(x => x.C_cuentas_contables_g)).Distinct();

            List<ReportePolizasAlmacenCuentasContables> reportePolizas = new List<ReportePolizasAlmacenCuentasContables>();
            decimal total_salida = 0;
            decimal total_entrada = 0;
            foreach (var item in cuentas_contables.OrderBy(x => x.C_cargos_contables_g.nombre_cargo).Distinct())
            {
                try
                {
                    var entrada = new ReportePolizasAlmacenCuentasContables();
                    entrada.Cuenta = item.C_cuentas_contables_g2.cuenta;
                    entrada.CuentaContable = item.C_cuentas_contables_g2.nombre_cuenta;
                    entrada.CargoContable = item.C_cargos_contables_g.nombre_cargo;
                    try
                    {
                        entrada.Importe_salida = total_salida = (decimal)salidas.Where(x => x.id_cuenta_contable_g == item.id_cuenta_contable).Select(x => x.cantidad_entregada * x.costo).Sum();
                    }
                    catch (Exception)
                    {
                        entrada.Importe_salida = 0;
                    }

                    try
                    {
                        entrada.Importe_entrada = total_entrada = (decimal)entrada_ordenes_d.Where(x => x.id_cuenta_contable == item.id_cuenta_contable).Select(x => x.cantidad_entregada * x.precio_unitario).Sum();
                    }
                    catch (Exception)
                    {
                        entrada.Importe_entrada = 0;
                    }
                    entrada.Importe_total = total_salida + total_entrada;
                    reportePolizas.Add(entrada);
                }
                catch (Exception)
                {
                }
            }
            return PartialView("../ALMACEN/PolizasContables/_PolizaSemanal", reportePolizas);
        }

        public PartialViewResult GenerarPolizasAlmacenSemanalDetalle(int[] id_almacen_g_poliza, string fecha_i, string fecha_f)
        {
            if (id_almacen_g_poliza.Contains(0)) { return PartialView("../ALMACEN/PolizasContables/_PolizaSemanal", null); }
            DateTime fecha_inicio = DateTime.Parse(fecha_i);
            DateTime fecha_fin = DateTime.Parse(fecha_f).AddHours(23).AddMinutes(59);
            var entrada_ordenes_d = from mov_g in db.C_inventario_almacen_mov_g
                                    join mov_d in db.C_inventario_almacen_mov_d on mov_g.id_inventario_almacen_mov_g equals mov_d.id_inventario_almacen_mov_g
                                    join ord_g in db.C_compras_ordenes_g on mov_g.id_transaccion_orden_g equals ord_g.id_compras_orden_g
                                    join ord_d in db.C_compras_ordenes_d on ord_g.id_compras_orden_g equals ord_d.id_compras_orden_g
                                    where ord_d.activo == true && ord_g.activo == true && ord_d.cantidad_entregada > 0 && id_almacen_g_poliza.Contains((int)mov_g.id_almacen_origen)
                                    && mov_d.fecha_registro >= fecha_inicio && mov_d.fecha_registro <= fecha_fin
                                    select ord_d;


            var salidas = db.C_almacen_solicitudes_mercancia_d.Where(x => id_almacen_g_poliza.Contains((int)x.C_almacen_solicitudes_mercancia_g.id_almacen_g) && x.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_inicio && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_fin &&
                        x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                        x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002).ToList();

            var cuentas_contables = salidas.Select(x => x.C_cuentas_contables_g).Union(entrada_ordenes_d.Select(x => x.C_cuentas_contables_g));

            var reportePolizas = new List<ReportePolizasAlmacenCuentasContablesDetalle>();


            decimal total_salida = 0;
            decimal total_entrada = 0;


            var pruebas = salidas.Count();
            foreach (var vale in salidas)
            {
                var poliza_detalle = new ReportePolizasAlmacenCuentasContablesDetalle();

                poliza_detalle.Almacen = vale.C_almacen_solicitudes_mercancia_g.C_almacen_almacenes_g.siglas;
                poliza_detalle.Id_cargo_contable = (int)vale.id_cargo_contable_g;
                poliza_detalle.Cargo_Contable = vale.C_cargos_contables_g.nombre_cargo;
                poliza_detalle.Mov = "Salida";
                poliza_detalle.Folio = Convert.ToString(vale.id_solicitud_mercancia_d);
                try
                {
                    poliza_detalle.Fecha = vale.C_almacen_solicitudes_mercancia_g.fecha_entrega.ToString();
                }
                catch (Exception) { poliza_detalle.Fecha = "Pendiente"; }
                poliza_detalle.Clave = vale.C_articulos_catalogo.clave;
                poliza_detalle.Articulo = vale.C_articulos_catalogo.nombre_articulo;
                poliza_detalle.Medida = vale.C_articulos_catalogo.C_unidades_medidas.unidad_medida;
                poliza_detalle.Cantidad = (decimal)vale.cantidad_entregada;
                poliza_detalle.Precio = (decimal)vale.costo;
                poliza_detalle.Importe = (decimal)vale.cantidad_entregada * (decimal)vale.costo;
                poliza_detalle.Cuenta = vale.C_cuentas_contables_g.cuenta;
                poliza_detalle.Cuenta_Contable = vale.C_cuentas_contables_g.nombre_cuenta;
                poliza_detalle.Movimiento = "Consumo";

                poliza_detalle.Requisicion = "";
                poliza_detalle.Remision = "";
                poliza_detalle.Proveedor = "";

                poliza_detalle.Trabajador = vale.C_almacen_solicitudes_mercancia_g.C_empleados.nombres;
                poliza_detalle.Equipo = vale.C_almacen_solicitudes_mercancia_g.C_establos_equipos.nombre_equipo;
                poliza_detalle.Departamento = vale.C_almacen_solicitudes_mercancia_g.C_departamentos_g.nombre_departamento;
                poliza_detalle.Centro = vale.C_almacen_solicitudes_mercancia_g.C_centros_g.nombre_centro;
                reportePolizas.Add(poliza_detalle);
            }
            foreach (var orden in entrada_ordenes_d)
            {
                var poliza_detalle = new ReportePolizasAlmacenCuentasContablesDetalle();
                poliza_detalle.Almacen = orden.C_compras_ordenes_g.C_inventario_almacen_mov_g.FirstOrDefault().C_almacen_almacenes_g.siglas;
                poliza_detalle.Id_cargo_contable = (int)orden.id_cargo_contable;
                poliza_detalle.Cargo_Contable = orden.C_cargos_contables_g.nombre_cargo;
                poliza_detalle.Mov = "Entrada";
                poliza_detalle.Folio = "";
                try
                {
                    poliza_detalle.Fecha = orden.C_compras_ordenes_g.fecha_registro.ToString();
                }
                catch (Exception) { poliza_detalle.Fecha = "Pendiente"; }
                poliza_detalle.Clave = orden.C_articulos_catalogo.clave;
                poliza_detalle.Articulo = orden.C_articulos_catalogo.nombre_articulo;
                poliza_detalle.Medida = orden.C_articulos_catalogo.C_unidades_medidas.unidad_medida;
                poliza_detalle.Cantidad = (decimal)orden.cantidad_entregada;
                poliza_detalle.Precio = (decimal)orden.precio_unitario;
                poliza_detalle.Importe = (decimal)orden.cantidad_entregada * (decimal)orden.precio_unitario;
                poliza_detalle.Cuenta = orden.C_cuentas_contables_g.cuenta;
                poliza_detalle.Cuenta_Contable = orden.C_cuentas_contables_g.nombre_cuenta;
                poliza_detalle.Movimiento = "Compra";


                poliza_detalle.Requisicion = orden.id_compras_orden_g.ToString();
                poliza_detalle.Remision = "";
                poliza_detalle.Proveedor = orden.C_compras_ordenes_g.C_compras_proveedores.razon_social;

                poliza_detalle.Trabajador = orden.C_compras_ordenes_g.C_usuarios_corporativo.C_empleados.nombres + " " + orden.C_compras_ordenes_g.C_usuarios_corporativo.C_empleados.apellido_paterno;
                poliza_detalle.Equipo = "";
                poliza_detalle.Departamento = "";
                poliza_detalle.Centro = orden.C_compras_ordenes_g.C_centros_g.nombre_centro;
                reportePolizas.Add(poliza_detalle);
            }
            return PartialView("../ALMACEN/PolizasContables/_PolizaSemanalDetalle", reportePolizas); ;
        }
        #endregion

        #region REPORTE GLOBAL DE ALMACENES Y SUS EXISTENCIAS
        public PartialViewResult ReporteExistenciaArticulosAlmacenes(int[] id_almacen)
        {
            try
            {
                CultureInfo cultura = new CultureInfo("es-ES");
                Calendar calendario = cultura.Calendar;
                
                ARTICULOSController ArticulosController = new ARTICULOSController();

                var consumo = db.C_almacen_solicitudes_mercancia_d_logs.Where(x => x.activo == true && x.fecha_registro <= DateTime.Today
                && x.C_almacen_solicitudes_mercancia_d.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002).ToList();
                List<DateTime> meses = new List<DateTime>();
                meses.Add(new DateTime(2024, 07, 01));
                meses.Add(new DateTime(2024, 08, 01));
                meses.Add(new DateTime(2024, 09, 01));
                meses.Add(new DateTime(2024, 10, 01));
                meses.Add(new DateTime(2024, 11, 01));
                meses.Add(new DateTime(2024, 12, 01));
                meses.Add(new DateTime(2025, 01, 01));
                meses.Add(new DateTime(2025, 02, 01));
                meses.Add(new DateTime(2025, 03, 01));

                if (id_almacen.Contains(0)) { id_almacen = db.C_almacen_almacenes_g.Where(x => x.activo == true).Select(x => x.id_almacen_g).ToArray(); }

                List<C_almacen_almacenes_g> almacenes = db.C_almacen_almacenes_g.Where(x => x.activo == true && id_almacen.Contains((int)x.id_almacen_g)).ToList();
                List<CapturaInventarioDiferencias> data = new List<CapturaInventarioDiferencias>();
                var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true).OrderBy(x => x.C_articulos_tipos.nombre_tipo).ToList();
                foreach (var articulo in articulos)
                {
                    string precio_articulo = ArticulosController.BuscarPrecioArticulo((int)articulo.id_articulo);
                    var ubicaciones = db.C_inventario_almacenes_articulos.Where(x => x.id_articulo == articulo.id_articulo).ToList();
                    foreach (var almacen in almacenes)
                    {
                        int id_articulo = articulo.id_articulo;
                        CapturaInventarioDiferencias reporte = new CapturaInventarioDiferencias();
                        int id_almacen_g = (int)almacen.id_almacen_g;
                        string existencia = ConsultarExistenciaArticuloAlmacen(id_almacen_g, (int)articulo.id_articulo);

                        reporte.Clave = articulo.clave.Trim();
                        reporte.Nombre_articulo = articulo.nombre_articulo;
                        reporte.Tipo_articulo = articulo.C_articulos_tipos.nombre_tipo;
                        reporte.Almacen = almacen.nombre_almacen;
                        reporte.Cantidad_sistema = Convert.ToDecimal(existencia);
                        reporte.Precio = Convert.ToDecimal(precio_articulo);
                        reporte.Total = reporte.Cantidad_sistema * reporte.Precio;
                        try
                        {
                            reporte.Ubicacion = ubicaciones.Where(x => x.id_almacen == id_almacen_g && x.activo == true).FirstOrDefault().C_almacen_ubicaciones_articulos.nombre_ubicacion;
                        }
                        catch (Exception)
                        {
                            reporte.Ubicacion = "NO ASIGNADA";
                        }

                        try
                        {
                            var valid_maximo = articulo.C_inventario_almacen_establo_maximo_minimo.Where(x => x.id_almacen_g == id_almacen_g && x.activo == true && x.id_articulo == id_articulo).FirstOrDefault();
                            if (valid_maximo != null)
                            {
                                reporte.Maximo = (decimal)valid_maximo.cantidad_maxima;
                                reporte.Minimo = (decimal)valid_maximo.cantidad_minima;
                                reporte.Promedio = (decimal)valid_maximo.cantidad_promedio;
                            }
                            else
                            {
                                reporte.Maximo = -1;
                                reporte.Minimo = -1;
                                reporte.Promedio = -1;
                            }
                        }
                        catch (Exception)
                        {
                            reporte.Maximo = -1;
                            reporte.Minimo = -1;
                            reporte.Promedio = -1;
                        }

                        reporte.Meses_consumo_nombre = new List<string>();
                        reporte.Meses_consumo_valor = new List<decimal>();
                        foreach (var mes in meses)
                        {
                            DateTime fecha_inicio = mes;
                            DateTime fecha_fin = mes.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
                            string nombre_mes = mes.ToString("MMMM", cultura).ToUpper() + " " + mes.Year.ToString();

                            reporte.Meses_consumo_nombre.Add(nombre_mes);
                            
                            decimal consumo_mes = (decimal)consumo.Where(x => x.C_almacen_solicitudes_mercancia_d.id_articulo == id_articulo && x.C_almacen_solicitudes_mercancia_d.C_almacen_solicitudes_mercancia_g.id_almacen_g == id_almacen_g 
                            && x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin).Sum(x => x.cantidad);
                            reporte.Meses_consumo_valor.Add(consumo_mes);
                        }

                        data.Add(reporte);
                    }
                }
                return PartialView("_ReporteExistenciasArticulosAlmacenes", data);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return null;
            }
        }

        public PartialViewResult ReporteExistenciasGlobalAlmacenes(int[] id_almacen)
        {
            DateTime fecha_inventario = new DateTime(2024,12,31);

            CultureInfo cultura = new CultureInfo("es-ES");
            Calendar calendario = cultura.Calendar;

            if (id_almacen.Contains(0)) { id_almacen = db.C_almacen_almacenes_g.Where(x => x.activo == true).Select(x => x.id_almacen_g).ToArray(); }

            List<C_almacen_almacenes_g> almacenes = db.C_almacen_almacenes_g.Where(x => x.activo == true && id_almacen.Contains((int)x.id_almacen_g)).ToList();
            List<CapturaInventarioDiferencias> data = new List<CapturaInventarioDiferencias>();
            var articulos_cat = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true).OrderBy(x => x.C_articulos_tipos.nombre_tipo).ToList();

            //var articulos_cat = db.C_inventario_almacen_captura_d.Select(x => x.C_articulos_catalogo).Distinct().ToList();

            foreach (var articulo in articulos_cat)
            {
                string precio_articulo = articulos.BuscarPrecioArticulo((int)articulo.id_articulo);
                var ubicaciones = db.C_inventario_almacenes_articulos.Where(x => x.id_articulo == articulo.id_articulo).ToList();
                foreach (var almacen in almacenes)
                {
                    int id_articulo = articulo.id_articulo;
                    CapturaInventarioDiferencias reporte = new CapturaInventarioDiferencias();
                    int id_almacen_g = (int)almacen.id_almacen_g;

                    var valid_inventario_captura = db.C_inventario_almacen_captura_d.Where(x => x.C_inventario_almacen_captura_g.id_almacen == id_almacen_g 
                    && x.activo == true && x.id_articulo == id_articulo && x.C_inventario_almacen_captura_g.fecha_registro <= fecha_inventario
                    && x.C_inventario_almacen_captura_g.id_status_inventario == 1 && x.C_inventario_almacen_captura_g.activo == true)
                        .OrderByDescending(x => x.id_inventario_g).FirstOrDefault();

                    List<C_inventario_almacen_mov_d> entradas = null;
                    List<C_inventario_almacen_mov_d> salidas = null;
                    List<C_inventario_almacen_mov_d> mermas = null;
                    List<C_inventario_almacen_mov_d> entr_trasp = null;
                    List<C_inventario_almacen_mov_d> salid_trasp = null;

                    List<C_inventario_almacen_mov_d> entradas_ajuste = null;
                    List<C_inventario_almacen_mov_d> salidas_ajuste = null;
                    List<C_inventario_almacen_mov_d> entradas_devolucion = null;

                    if (valid_inventario_captura != null) // EXISTE UNA CAPTURA DE INVENTARIO
                    {
                        entradas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        salidas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 2 && x.id_inventario_mov_status == 2 //ENTREGAS DE VALES
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        mermas = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == false && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 3 && x.id_inventario_mov_status == 2 //MERMAS
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        entr_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_destino == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_origen != null && x.id_articulo == id_articulo
                        && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //ENTRADAS TRASPASO
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        salid_trasp = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.C_inventario_almacen_mov_g.id_almacen_destino != null && x.id_articulo == id_articulo
                        && x.id_inventario_mov_status == 2 && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1002 //SALIDAS TRASPASO
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        //AJUSTES
                        entradas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1003 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        salidas_ajuste = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1004 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        entradas_devolucion = db.C_inventario_almacen_mov_d.Where(x => x.C_inventario_almacen_mov_g.id_almacen_origen == id_almacen_g && x.entrada_salida == true && x.id_articulo == id_articulo
                        && x.C_inventario_almacen_mov_g.id_inventario_mov_tipo == 1005 && x.id_inventario_mov_status == 2 //ENTRADAS POR COMPRA
                        && x.fecha_registro >= (DateTime)valid_inventario_captura.C_inventario_almacen_captura_g.fecha_registro && x.fecha_registro <= fecha_inventario).ToList();

                        reporte.Entradas = (decimal)entradas.Sum(x => x.cantidad).Value;
                        reporte.Salidas = (decimal)salidas.Sum(x => x.cantidad).Value;
                        reporte.Mermas = (decimal)mermas.Sum(x => x.cantidad).Value;
                        reporte.Entradas_trasp = (decimal)entr_trasp.Sum(x => x.cantidad).Value;
                        reporte.Salidas_trasp = (decimal)salid_trasp.Sum(x => x.cantidad).Value;
                        reporte.Entradas_ajuste = (decimal)entradas_ajuste.Sum(x => x.cantidad).Value;
                        reporte.Salidas_ajuste = (decimal)salidas_ajuste.Sum(x => x.cantidad).Value;
                        reporte.Entradas_devolucion = (decimal)entradas_devolucion.Sum(x => x.cantidad).Value;
                        decimal CantidadCapturada = (decimal)valid_inventario_captura.cantidad_captura;
                        reporte.Ultima_Cap = CantidadCapturada;
                        reporte.Cantidad_sistema = (CantidadCapturada + reporte.Entradas + reporte.Entradas_trasp + reporte.Entradas_ajuste + reporte.Entradas_devolucion) - reporte.Salidas - reporte.Mermas - reporte.Salidas_trasp - reporte.Salidas_ajuste;
                    }
                    else
                    {
                        reporte.Ultima_Cap = 0;
                        reporte.Entradas = 0;
                        reporte.Salidas = 0;
                        reporte.Mermas = 0;
                        reporte.Entradas_trasp = 0;
                        reporte.Salidas_trasp = 0;
                        reporte.Entradas_ajuste = 0;
                        reporte.Salidas_ajuste = 0;
                        reporte.Entradas_devolucion = 0;
                        reporte.Cantidad_sistema = 0;
                    }
                    
                    reporte.Clave = articulo.clave.Trim();
                    reporte.Nombre_articulo = articulo.nombre_articulo;
                    reporte.Tipo_articulo = articulo.C_articulos_tipos.nombre_tipo;
                    reporte.Almacen = almacen.nombre_almacen;
                    reporte.Precio = Convert.ToDecimal(precio_articulo);
                    reporte.Total = reporte.Cantidad_sistema * reporte.Precio;
                    try
                    {
                        reporte.Ubicacion = ubicaciones.Where(x => x.id_almacen == id_almacen_g && x.activo == true).FirstOrDefault().C_almacen_ubicaciones_articulos.nombre_ubicacion;
                    }
                    catch (Exception)
                    {
                        reporte.Ubicacion = "NO ASIGNADA";
                    }


                    data.Add(reporte);
                }
            }
            return PartialView("_ReporteExistenciasGlobalAlmacenes", data);
        }

        #endregion

    }
}