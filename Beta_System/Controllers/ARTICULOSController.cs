using Antlr.Runtime.Misc;
using Beta_System.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;


namespace Beta_System.Controllers
{
    public class ARTICULOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public ActionResult AdministrarArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(2016)) { return View("/Views/Home/Index.cshtml"); }

                return View("../COMPRAS/ARTICULOS/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarArticulosSistema()
        {
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true).OrderBy(x => x.nombre_articulo).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosSelect", articulos);
        }

        public PartialViewResult ConsultarArticulosSistemaRubro(int id_rubro, int almacenable)
        {
            bool[] almacenables = null;
            if (almacenable == 0)
            {
                almacenables = new bool[2];
                almacenables[0] = true;
                almacenables[1] = false;
            }
            else
            {
                almacenables = new bool[1];
                if (almacenable == 1) { almacenables[0] = true; }
                else { almacenables[0] = false; }
            }

            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.id_articulo_tipo_requisicion == id_rubro && almacenables.Contains((bool)x.almacenable)).OrderBy(x => x.nombre_articulo).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosSelect", articulos);
        }



        public string ConsultarPrecioArticulo(int id_articulo)
        {
            var valid = db.C_articulos_precios.Where(x => x.id_articulo == id_articulo).OrderByDescending(x => x.fecha_regitro).FirstOrDefault();
            if (valid != null) { return valid.precio.ToString(); }
            else { return "0.00"; }
        }

        public string ConsultarInformacionArticulo(string id_articulo, string tipo)
        {
            if (tipo != "")
            {
                if (tipo == "3") { tipo = "1"; }
                int tipos = Convert.ToInt32(tipo);
                var articulo = from art in db.C_articulos_catalogo
                               where art.clave == id_articulo && (int)art.id_articulo_tipo_requisicion == tipos && art.activo == true
                               select new
                               {
                                   art.nombre_articulo,
                                   art.C_articulos_tipos.nombre_tipo,
                                   art.C_articulos_clasificaciones.nombre_clasificacion,
                                   art.C_unidades_medidas.unidad_medida,
                                   art.C_articulos_lineas.nombre_linea,
                                   art.id_unidad_medida,
                                   art.id_articulo_clasificacion,
                                   art.id_articulo_tipo,
                                   art.id_articulo_linea,
                                   art.descripcion_articulo,
                                   art.clave,
                                   art.id_articulo
                               };

                if (articulo.ToList().Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var articulo = from art in db.C_articulos_catalogo
                               where art.clave == id_articulo && art.activo == true
                               select new
                               {
                                   art.nombre_articulo,
                                   art.C_articulos_tipos.nombre_tipo,
                                   art.C_articulos_clasificaciones.nombre_clasificacion,
                                   art.C_unidades_medidas.unidad_medida,
                                   art.C_articulos_lineas.nombre_linea,
                                   art.id_unidad_medida,
                                   art.id_articulo_clasificacion,
                                   art.id_articulo_tipo,
                                   art.id_articulo_linea,
                                   art.descripcion_articulo,
                                   art.clave,
                                   art.id_articulo
                               };
                if (articulo.Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
        }

        public string ConsultarInformacionNomArticulo(string nombre_articulo, string tipo)
        {
            if (tipo != "")
            {
                if (tipo == "3") { tipo = "1"; }
                int tipos = Convert.ToInt32(tipo);
                var articulo = from art in db.C_articulos_catalogo
                               where art.nombre_articulo == nombre_articulo && (int)art.id_articulo_tipo_requisicion == tipos && art.activo == true
                               select new
                               {
                                   art.id_articulo,
                                   art.nombre_articulo,
                                   art.C_unidades_medidas.unidad_medida,
                                   art.id_unidad_medida,
                                   art.clave
                               };
                if (articulo.ToList().Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var articulo = from art in db.C_articulos_catalogo
                               where art.nombre_articulo == nombre_articulo && art.activo == true
                               select new
                               {
                                   art.id_articulo,
                                   art.nombre_articulo,
                                   art.C_unidades_medidas.unidad_medida,
                                   art.id_unidad_medida,
                                   art.clave
                               };
                if (articulo != null)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
        }

        //CAMBIO 25/10/2025
        public string ConsultarDtosArticulo(string nombre_articulo)
        {
            var articulo = from art in db.C_articulos_catalogo
                           where art.nombre_articulo == nombre_articulo && art.activo == true
                           select new
                           {
                               art.id_articulo,
                               art.nombre_articulo,
                               art.C_unidades_medidas.unidad_medida,
                               art.id_unidad_medida,
                               art.clave
                           };
            if (articulo != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
            }
            else
            {
                return null;
            }
        }
        //
        public string ConsultarDecimalArticulo(int id_unidad_medida)
        {
            var decimales = from deci in db.C_unidades_medidas
                            where deci.id_unidad_medida == id_unidad_medida
                            select new
                            {
                                deci.acepta_decimal,
                            };
            if (decimales != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(decimales);
            }
            else
            {
                return null;
            }

        }

        public string ConsultarNombreArt(int id_articulo)
        {
            var articulo = from art in db.C_articulos_catalogo
                           where art.id_articulo == id_articulo
                           select new
                           {
                               art.nombre_articulo
                           };

            return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
        }

        public string ConsultarArticulosArray(string search, int[] id_tipo, int almacenable)
        {
            //int busqueda = 0;
            bool[] almacenables = null;
            if (almacenable == 0)
            {
                almacenables = new bool[2];
                almacenables[0] = true;
                almacenables[1] = false;
            }
            else
            {
                almacenables = new bool[1];
                if (almacenable == 1) { almacenables[0] = true; }
                else { almacenables[0] = false; }
            }

            try
            {
                //id_tipo = 1: Compras/Inversion
                //id_tipo = 2: Servicio

                //busqueda = Convert.ToInt32(search);
                var articulos = (from art in db.C_articulos_catalogo
                                 where art.activo == true && art.clave != null && (art.id_articulo.ToString().Contains(search) || art.nombre_articulo.Contains(search))
                                 && id_tipo.Contains((int)art.id_articulo_tipo_requisicion) && almacenables.Contains((bool)art.almacenable)
                                 select new { art.id_articulo, art.nombre_articulo, art.clave }).Take(10);
                return Newtonsoft.Json.JsonConvert.SerializeObject(articulos);
            }
            catch (Exception)
            {

                return null;
            }
        }
        public PartialViewResult ConsultarClasificacionSelect()
        {
            var tipos_art = db.C_articulos_clasificaciones.Where(x => x.activo == true).OrderBy(x => x.nombre_clasificacion).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ClasificacionSelect", tipos_art);
        }

        public PartialViewResult ConsultarLineaSelect()
        {
            var tipos_art = db.C_articulos_lineas.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_LineaSelect", tipos_art);
        }

        public PartialViewResult ConsultarUnidadMedida()
        {
            var unidadmedida = db.C_unidades_medidas.Where(x => x.activo == true).OrderBy(x => x.unidad_medida).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_UnidadMedidaSelect", unidadmedida);
        }

        public PartialViewResult ConsultarArticulo(int id_articulo_tipo)
        {
            var query = from art in db.C_articulos_catalogo
                        join tip in db.C_articulos_tipos on art.id_articulo_tipo equals tip.id_articulo_tipo
                        where art.id_articulo_tipo == id_articulo_tipo
                        select art.id_articulo_tipo;
            var articuloNombre = db.C_articulos_catalogo.Where(x => query.Contains((int)x.id_articulo_tipo)).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosTable", articuloNombre);
        }

        #region REGISTRAR - ELIMINAR ARTICULOS DEL ALMACEN
        public PartialViewResult ConsultarArticuloCompleto(int[] id_clasif_filtro, bool[] almacen, bool[] activo, int[] tipo, int[] marcas_art)
        {
            if (id_clasif_filtro.Contains(0))
            {
                var clasif = from clasifica in db.C_articulos_clasificaciones
                                 //where clasifica.activo == true
                             select clasifica.id_articulo_clasificacion;
                id_clasif_filtro = clasif.ToArray();
            }
            if (tipo.Contains(0))
            {
                var tip = from tipos in db.C_articulos_tipos_requisicion
                              //where tipos.activo == "1"
                          select tipos.id_articulo_tipo_requisicion;
                tipo = tip.ToArray();
            }
            if (marcas_art.Contains(0))
            {
                var marc = from tipos in db.C_articulos_marcas
                               //where tipos.activo == true
                           select tipos.id_marca;
                marcas_art = marc.ToArray();
            }
            var articulos_cfiltro = db.C_articulos_catalogo.Where(x => id_clasif_filtro.Contains((int)x.id_articulo_clasificacion) && tipo.Contains((int)x.id_articulo_tipo_requisicion) && almacen.Contains((bool)x.almacenable) && activo.Contains((bool)x.activo) && marcas_art.Contains((int)x.id_marca)).OrderBy(x => x.nombre_articulo);
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosCompletoTable", articulos_cfiltro);
        }

        public PartialViewResult ConsultarArticuloCompletosFiltrado(string clave, string articulo)
        {
            if (clave == "" && articulo != "")
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.nombre_articulo.Contains(articulo)).OrderBy(x => x.nombre_articulo);
                return PartialView("../CATALOGOS/ARTICULOS/_ArticulosCompletoTable", articulos_cfiltro);
            }
            else if (articulo == "" && clave != "")
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.clave.Contains(clave)).OrderBy(x => x.nombre_articulo);
                return PartialView("../CATALOGOS/ARTICULOS/_ArticulosCompletoTable", articulos_cfiltro);
            }
            else
            {
                var articulos_cfiltro = db.C_articulos_catalogo.Where(x => x.nombre_articulo.Contains(articulo) || x.clave.Contains(clave)).OrderBy(x => x.nombre_articulo);
                return PartialView("../CATALOGOS/ARTICULOS/_ArticulosCompletoTable", articulos_cfiltro);
            }

        }

        public PartialViewResult ConsultarArticuloCompletoSFiltro(int[] id_clasif_filtro)
        {
            if (id_clasif_filtro.Contains(0))
            {
                var clasif = from clasifica in db.C_articulos_clasificaciones
                             where clasifica.activo == true
                             select clasifica.id_articulo_clasificacion;
                id_clasif_filtro = clasif.ToArray();
            }
            var articulos_sfiltro = db.C_articulos_catalogo.Where(x => id_clasif_filtro.Contains((int)x.id_articulo_clasificacion)).OrderBy(x => x.nombre_articulo);
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosCompletoTable", articulos_sfiltro);
        }

        public PartialViewResult ConsultarArticuloAlmacen(int id_almacen, int id_tipo)
        {
            var query = from artalm in db.C_inventario_almacenes_articulos
                        join alm in db.C_almacen_almacenes_g on artalm.id_almacen equals alm.id_almacen_g
                        join art in db.C_articulos_catalogo on artalm.id_articulo equals art.id_articulo
                        where alm.id_almacen_g == id_almacen && art.id_articulo_tipo == id_tipo
                        select art.id_articulo;
            var articuloNombreAlmacen = db.C_articulos_catalogo.Where(x => query.Contains((int)x.id_articulo)).OrderBy(x => x.nombre_articulo).ToList();
            return PartialView("../CATALOGOS/ALMACENES/_ArticulosAlmacen", articuloNombreAlmacen);
        }



        public bool ValidarArticuloAlmacen(int id_articulo, int id_almacen)
        {
            try
            {
                var articuloAlmacen = db.C_inventario_almacenes_articulos.Where(x => x.id_articulo == id_articulo && x.id_almacen == id_almacen).ToList();
                if (articuloAlmacen.Count >= 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return true;
            }
        }

        public bool RemoverArticuloAlmacen(int id_articulo, int id_almacen)
        {
            try
            {
                var removerArticulo = db.C_inventario_almacenes_articulos.Where(x => x.id_articulo == id_articulo && x.id_almacen == id_almacen);
                db.C_inventario_almacenes_articulos.RemoveRange(removerArticulo);
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

        public string ObtenerInfoArticulo(int id_articulo)
        {
            var articulo = from art in db.C_articulos_catalogo
                           where art.id_articulo == id_articulo
                           select new
                           {
                               art.nombre_articulo,
                               art.descripcion_articulo,
                               art.id_unidad_medida,
                               art.id_articulo_tipo,
                               art.id_articulo_clasificacion,
                               art.id_articulo_linea,
                               art.almacenable,
                               art.activo,
                               art.id_articulo_tipo_requisicion,
                               art.id_marca,
                               art.clave,
                               art.disponible_bascula
                           };

            return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
        }

        #region REGISTRO - MODIFICACION ARTICULOS
        public int RegistarArticulo(C_articulos_catalogo c_articulo)
        {
            try
            {
                string clave = c_articulo.clave;
                var valid = db.C_articulos_catalogo.Where(x => x.clave == clave).FirstOrDefault();
                if (valid != null) { return -1; }

                c_articulo.activo = true;
                c_articulo.fecha_registro = DateTime.Now;
                db.C_articulos_catalogo.Add(c_articulo);
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 1;
            }
        }

        public bool ModificarArticulos(int id_articulo, string nombre_articulo, string descripcion, int unidad_medida, int articulo_tipo, int artic_clasif, bool almaenable, bool estatus, int rubro /*, string clave*/, bool disponible_bascula)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                C_articulos_catalogo articulo = new C_articulos_catalogo();

                var art = db.C_articulos_catalogo.Find(id_articulo);
                articulo = art;

                art.nombre_articulo = nombre_articulo;
                art.descripcion_articulo = descripcion;
                art.id_unidad_medida = unidad_medida;
                art.id_articulo_tipo = articulo_tipo;
                art.id_articulo_clasificacion = artic_clasif;
                art.activo = estatus;
                art.almacenable = almaenable;
                art.id_articulo_tipo_requisicion = rubro;
                art.disponible_bascula = disponible_bascula;
                //art.clave = clave;
                db.SaveChanges();

                C_articulos_logs logs_arti = new C_articulos_logs();
                logs_arti.id_usuario_log = id_usuario;
                logs_arti.id_articulo = id_articulo;
                logs_arti.fecha_registro = DateTime.Now;
                logs_arti.descripcion_cambio = "Se modifico el articulo \n";
                logs_arti.activo = true;


                string descripcion_soporte = "";
                if (articulo.id_articulo_tipo != art.id_articulo)
                {
                    logs_arti.valor_anterior = articulo.id_articulo_tipo.ToString();
                    logs_arti.valor_nuevo = art.id_articulo_tipo.ToString();
                }
                if (articulo.activo != art.activo)
                {
                    descripcion_soporte += "Se cambio el estatus: " + articulo.activo + " a " + art.activo + " \n";
                }
                if (articulo.disponible_bascula != art.disponible_bascula)
                {
                    descripcion_soporte += "Se cambio la disponibilidad de bascula: " + articulo.disponible_bascula + " a " + art.disponible_bascula + " \n";
                }
                if (articulo.almacenable != art.almacenable)
                {
                    descripcion_soporte += "Se cambio almacenable: " + articulo.almacenable + " a " + art.almacenable + " \n";
                }
                if (articulo.id_unidad_medida != art.id_unidad_medida)
                {
                    descripcion_soporte += "Se cambio la unidad de medida: " + articulo.C_unidades_medidas.unidad_medida + " a " + art.C_unidades_medidas.unidad_medida;
                }
                logs_arti.descripcion_cambio = descripcion;

                db.C_articulos_logs.Add(logs_arti);
                db.SaveChanges();
                //camvio familia valor nuevo y viejo
                //descripciob
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool OnOffArticulos(int id_articulo, bool modo)
        {
            try
            {
                var art = db.C_articulos_catalogo.Find(id_articulo);
                art.activo = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OnOffAlmacenables(int id_articulo, bool modo)
        {
            try
            {
                var art = db.C_articulos_catalogo.Find(id_articulo);
                art.almacenable = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        public PartialViewResult LupaArticulo(int requi_tipo, int modo)
        {
            ViewBag.articulomodo = modo;
            if (requi_tipo == 1 || requi_tipo == 3)
            {
                requi_tipo = 1;
            }
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.id_articulo_tipo_requisicion == requi_tipo).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_LupaArticulosTable", articulos);
        }
        public PartialViewResult LupaArticuloParametro(int requi_tipo, int modo, int[] parametro)
        {
            if (parametro.Contains(0))
            {
                var clasif = from clasifica in db.C_articulos_clasificaciones
                             where clasifica.activo == true
                             select clasifica.id_articulo_clasificacion;
                parametro = clasif.ToArray();
            }

            ViewBag.articulomodo = modo;
            if (requi_tipo == 1 || requi_tipo == 3)
            {
                requi_tipo = 1;
            }
            var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.id_articulo_tipo_requisicion == requi_tipo && parametro.Contains((int)x.id_articulo_clasificacion)).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_LupaArticulosTable", articulos);
        }

        public string BuscarPrecioArticulo(int id_articulo)
        {
            try
            {
                //PRECIOS SIIB
                var ultimo_precio = db.C_compras_ordenes_d.Where(x => x.id_articulo == id_articulo && x.C_compras_ordenes_g.id_compras_orden_g == x.id_compras_orden_g &&
                x.C_compras_ordenes_g.id_status_orden != 3 && x.activo == true && x.C_compras_ordenes_g.activo == true).OrderByDescending(x => x.id_compras_orden_d).FirstOrDefault();
                if (ultimo_precio != null /*|| ultimo_precio.precio_unitario == 0 || ultimo_precio.precio_unitario == null*/)
                {
                    if (ultimo_precio.precio_unitario == 0)
                    {
                        //PRECIOS TABULADOR
                        var valid_tabulador = db.C_articulos_precios.Where(x => x.id_articulo == id_articulo && x.activo == true).FirstOrDefault();
                        if (valid_tabulador != null) { return valid_tabulador.precio.Value.ToString("N9"); }
                    }
                    var id_moneda = ultimo_precio.id_tipo_moneda;
                    if (id_moneda != 1)
                    {
                        decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
                        return ((decimal)ultimo_precio.precio_unitario * valor_dolar).ToString("N9");
                    }
                    return ultimo_precio.precio_unitario.Value.ToString("N9");
                }
                else
                {
                    //PRECIOS TABULADOR
                    var valid_tabulador = db.C_articulos_precios.Where(x => x.id_articulo == id_articulo && x.activo == true).FirstOrDefault();
                    if (valid_tabulador != null) { return valid_tabulador.precio.Value.ToString("N9"); }
                    //PRECIOS SAB
                    string clave = db.C_articulos_catalogo.Find(id_articulo).clave;
                    comprasEntities db_compras_sab = new comprasEntities();
                    var valid = db_compras_sab.articulo.Where(x => x.art.Trim() == clave /* || x.art.Trim() == string_char || x.art.Trim() == string_char2*/).FirstOrDefault();
                    if (valid != null)
                    {
                        var precio = db_compras_sab.orden_de.Where(x => x.art == valid.art && x.rstatus == "A" && x.precio > 0).OrderByDescending(x => x.rtime).FirstOrDefault();
                        if (precio != null)
                        {
                            return precio.precio.Value.ToString("N9");
                        }
                        else { return "0.000000000"; }
                    }
                    else { return "0.000000000"; }
                }
            }
            catch (Exception)
            {
                return "0.000000000";
            }
        }


        public PartialViewResult ConsultarArticulosNoAlmacenableSistema(int id_almacen, int id_articulo_tipo)
        {
            if (id_articulo_tipo != 0)
            {
                // Obtener los ID de los artículos que existen en el almacén específico
                var existencias = db.C_inventario_almacenes_articulos
                    .Where(x => x.id_almacen == id_almacen && x.C_articulos_catalogo.id_articulo_tipo == id_articulo_tipo)
                    .Select(x => x.id_articulo)
                    .ToArray();

                // Obtener los artículos activos y almacenables que no están en las existencias
                var articulos = db.C_articulos_catalogo
                    .Where(x => x.activo == true && x.almacenable == true && x.id_articulo_tipo == id_articulo_tipo && !existencias.Contains(x.id_articulo))
                    .OrderBy(x => x.nombre_articulo)
                    .ToList();

                return PartialView("../CATALOGOS/ARTICULOS/_ArticulosNoAlmacenableTable", articulos);
            }
            else
            {
                var articulos = db.C_articulos_catalogo.Where(x => x.activo == true && x.almacenable == true && x.id_articulo_tipo == id_articulo_tipo).ToList();
                return PartialView("../CATALOGOS/ARTICULOS/_ArticulosNoAlmacenableTable", articulos);
            }
        }

        public PartialViewResult ConsultarArticulosUbicaionAlmacen(int id_almacen, int id_articulo_tipo, int ubicacion)
        {
            var articulos = db.C_inventario_almacenes_articulos.Where(x => x.activo == true && x.id_almacen == id_almacen && x.C_articulos_catalogo.id_articulo_tipo == id_articulo_tipo && x.id_ubicacion_almacen == ubicacion).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ArticulosUbicaionAlmacenTable", articulos);
        }

        public bool AsignarArticuloAlmacen(int[] id_articulos_pendientes, int id_almacen, int ubicacion)
        {
            try
            {

                for (int i = 0; i < id_articulos_pendientes.Length; i++)
                {
                    C_inventario_almacenes_articulos articuloAlmacenado = new C_inventario_almacenes_articulos();
                    articuloAlmacenado.id_almacen = id_almacen;
                    articuloAlmacenado.id_articulo = id_articulos_pendientes[i];
                    articuloAlmacenado.id_ubicacion_almacen = ubicacion;
                    articuloAlmacenado.activo = true;
                    db.C_inventario_almacenes_articulos.Add(articuloAlmacenado);
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

        public bool RemoverArticulosAlmacen(int[] id_articulos_ubicacion, int id_almacen)
        {
            try
            {
                var articulos_almacen = db.C_inventario_almacenes_articulos.Where(x => x.activo == true && x.id_almacen == id_almacen && id_articulos_ubicacion.Contains((int)x.id_articulo)).ToList();
                db.C_inventario_almacenes_articulos.RemoveRange(articulos_almacen);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string ConsultarConsecutivoArticulo()
        {
            var test = db.C_articulos_catalogo.Where(x => x.id_articulo >= 123898).OrderBy(x => x.clave).Select(x => x.clave).ToArray();
            var soloNumeros = test.Where(x => !string.IsNullOrEmpty(x) && x.All(char.IsDigit)).ToList();
            int result = soloNumeros.Select(s => int.Parse(s)).Max();
            int last_clave = Convert.ToInt32(result) + 1;
            return last_clave.ToString();
        }



        #region Articulos Precio
        public PartialViewResult ArticulosPendientesPrecio(int[] id_clasif_filtro)
        {
            var articulos_precio_asignado = db.C_articulos_precios.Where(x => x.activo == true).Select(x => x.id_articulo).ToArray();

            if (id_clasif_filtro.Contains(0))
            {
                var clasif = from clasifica in db.C_articulos_clasificaciones
                                 //where clasifica.activo == true
                             select clasifica.id_articulo_clasificacion;
                id_clasif_filtro = clasif.ToArray();
            }

            var articulos_cfiltro = db.C_articulos_catalogo.Where(x => id_clasif_filtro.Contains((int)x.id_articulo_clasificacion)
            && !articulos_precio_asignado.Contains(x.id_articulo) && !articulos_precio_asignado.Contains(x.id_articulo)
            ).OrderBy(x => x.nombre_articulo);
            return PartialView("../CATALOGOS/ARTICULOS/_LupaArticuloSinPrecio", articulos_cfiltro);
        }
        public PartialViewResult ArticulosConPrecio()
        {
            var articulos_precio_asignado = db.C_articulos_precios.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_LupaArticuloConPrecio", articulos_precio_asignado);
        }
        public bool RegistrarPrecioArticulo(int id_articulo, decimal precio)
        {
            try
            {
                C_articulos_precios arti_precio = new C_articulos_precios();
                arti_precio.activo = true;
                arti_precio.id_articulo = id_articulo;
                arti_precio.precio = precio;
                arti_precio.fecha_regitro = DateTime.Now;
                db.C_articulos_precios.Add(arti_precio);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }
        public bool ModificarPrecioArticulo(int id_articulo_precio, decimal precio)
        {
            try
            {
                var arti_precio = db.C_articulos_precios.Find(id_articulo_precio);
                arti_precio.precio = precio;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }
        public string CargarInfoArticuloSinPrecio(string id_articulo_precio)
        {
            try
            {
                int articulo_id = Convert.ToInt32(id_articulo_precio);
                var articulo = from art in db.C_articulos_catalogo
                               where art.id_articulo == articulo_id
                               select new
                               {
                                   art.nombre_articulo,
                                   art.clave,
                                   art.id_articulo
                               };
                if (articulo.Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string CargarInfoArticuloSinPrecioClave(string clave)
        {
            try
            {
                var articulo = from art in db.C_articulos_catalogo
                               where art.clave == clave
                               select new
                               {
                                   art.nombre_articulo,
                                   art.clave,
                                   art.id_articulo
                               };
                if (articulo.Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string CargarInfoArticuloSinPrecioNombre(string nombre)
        {
            try
            {
                var articulo = from art in db.C_articulos_catalogo
                               where art.nombre_articulo == nombre
                               select new
                               {
                                   art.nombre_articulo,
                                   art.clave,
                                   art.id_articulo
                               };
                if (articulo.Count() > 0)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(articulo);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string ConsultarArticulosSinPrecioArray(string search)
        {
            try
            {
                var articulos = (from art in db.C_articulos_catalogo
                                 where art.activo == true && art.clave != null && (art.id_articulo.ToString().Contains(search) || art.nombre_articulo.Contains(search))
                                 select new { art.id_articulo, art.nombre_articulo, art.clave }).Take(10);
                return Newtonsoft.Json.JsonConvert.SerializeObject(articulos);
            }
            catch (Exception)
            {

                return null;
            }
        }
        #endregion



        #region CATALOGOS ARTICULOS BASCULA
        public PartialViewResult TipoArticuloBasculaDiv()
        {
            var tipo = db.C_bascula_fichas_tipos.Where(x => x.activo == true).ToList();
            return PartialView("../COMPRAS/ARTICULOS/Bascula/_TipoArticuloBasculaDiv", tipo);
        }

        public PartialViewResult ArticulosBasculaTable()
        {
            ViewBag.articulo_bascula_tipo = db.C_bascula_fichas_tipos.Where(x => x.activo == true).ToList();
            var articulos = db.C_articulos_catalogo.Where(x => x.disponible_bascula == true).ToList();
            return PartialView("../COMPRAS/ARTICULOS/Bascula/_ArticulosBasculaTable", articulos);
        }
        public bool AsociacionArticuloBascula(int[] tipos_ficha, int[] id_articulos)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                db.C_articulos_catalogo_bascula.Where(x => id_articulos.Contains((int)x.id_articulo)).ForEach(x => x.activo = false);
                db.SaveChanges();

                if (!tipos_ficha.Contains(0))
                {
                    foreach (var tipo in tipos_ficha)
                    {
                        foreach (var articulo in id_articulos)
                        {
                            int id_art = articulo;
                            var articulo_tipo = db.C_articulos_catalogo_bascula.Where(x => x.id_articulo == id_art && x.id_ficha_tipo == tipo).FirstOrDefault();
                            if (articulo_tipo != null)
                            {
                                articulo_tipo.id_ficha_tipo = tipo;
                                articulo_tipo.activo = true;
                                db.SaveChanges();
                            }
                            else
                            {
                                C_articulos_catalogo_bascula bascula = new C_articulos_catalogo_bascula
                                {
                                    id_articulo = articulo,
                                    id_ficha_tipo = tipo,
                                    usuario_registra = id_usuario,
                                    fecha_registro = DateTime.Now,
                                    activo = true
                                };
                                db.C_articulos_catalogo_bascula.Add(bascula);
                                db.SaveChanges();
                            }
                        }
                    }
                }


                // Selección de artículos que se desean eliminar
                //var articulos = db.C_articulos_catalogo_bascula.Where(x => id_articulos.Contains((int)x.id_articulo)).ToList();
                //// Inserción de los nuevos artículos
                //db.C_articulos_catalogo_bascula.AddRange(articulo_bascula);
                //db.SaveChanges();
                //// Eliminación de los artículos encontrados
                //db.C_articulos_catalogo_bascula.RemoveRange(articulos);
                //db.SaveChanges();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}