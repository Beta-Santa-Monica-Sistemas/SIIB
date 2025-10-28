using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class CASETAS_ESTABLOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private base_forrajesEntities db_forrajes = new base_forrajesEntities();

        public ActionResult BitacoraCaseta()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8039)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../ESTABLOS/Casetas/BitacoraCaseta/Index");
        }

        public PartialViewResult ConusltarTiposEntradasCaseta()
        {
            var tipos_entradas = db.C_establos_caseta_tipos_entradas.Where(x => /*x.id_establo == id_establo &&*/ x.activo == true).OrderBy(x => x.tipo_entrada).ToList();
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_TiposEntradaCasetaRadio", tipos_entradas);
        }

        public int GuardarLogEntradaCaseta(int id_establo, string nombre, int id_tipo_entrada_caseta, int id_area_caseta, string placas, string asunto)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];

                C_establos_caseta_logs_g log_g = new C_establos_caseta_logs_g();
                log_g.id_establo = id_establo;
                log_g.nombre_registro = nombre;
                log_g.id_tipo_entrada_caseta = id_tipo_entrada_caseta;
                log_g.id_caseta_area = id_area_caseta;
                log_g.fecha_entrada = hoy;
                log_g.id_usuario_registra_entrada = id_usuario;
                log_g.placas = placas;
                log_g.asunto = asunto;
                log_g.activo = true;
                db.C_establos_caseta_logs_g.Add(log_g);
                db.SaveChanges();
                return log_g.id_caseta_log_g;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GuardarLogEntradaCasetaVisitaGeneral(int id_log_g, string observaciones)
        {
            try
            {
                C_establos_caseta_logs_d_visita_general log_d_visita_general = new C_establos_caseta_logs_d_visita_general();
                log_d_visita_general.observaciones = observaciones;
                log_d_visita_general.id_caseta_log_g = id_log_g;
                db.C_establos_caseta_logs_d_visita_general.Add(log_d_visita_general);
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                var log_g = db.C_establos_caseta_logs_g.Find(id_log_g);  //AGREGAR ACTIVO A LOG_G
                return 1;
            }
        }

        public int GuardarLogEntradaCasetaVisitaBascula(int id_log_g, string ficha, string chofer, string proveedor, string producto, string placas, int id_tipo_tante)
        {
            try
            {
                C_establos_caseta_logs_d_visita_bascula log_d_visita_bascula = new C_establos_caseta_logs_d_visita_bascula();
                log_d_visita_bascula.id_caseta_log_g = id_log_g;
                log_d_visita_bascula.ficha = ficha;
                log_d_visita_bascula.nombre_chofer = chofer;
                log_d_visita_bascula.proveedor = proveedor;
                log_d_visita_bascula.producto = producto;
                log_d_visita_bascula.placas = placas;
                log_d_visita_bascula.id_tipo_tanque_pipa = id_tipo_tante;
                log_d_visita_bascula.activo = true;
                db.C_establos_caseta_logs_d_visita_bascula.Add(log_d_visita_bascula);
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                var log_g = db.C_establos_caseta_logs_g.Find(id_log_g);  //AGREGAR ACTIVO A LOG_G

                return 1;
            }
        }

        public PartialViewResult ConsultarEntradasPendientesEstablo(int id_establo)
        {
            var entradas = db.C_establos_caseta_logs_g.Where(x => x.id_establo == id_establo && x.fecha_salida == null && x.activo == true).ToList();
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_EntradasPendientesTable", entradas);
        }

        public PartialViewResult BuscarVisitantePlacas(string placas, int id_establo)
        {
            var info = db.C_establos_caseta_logs_g.Where(x => x.placas == placas && x.id_establo == id_establo && x.fecha_salida == null).FirstOrDefault();
            if (info == null) { return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoEntradaPendiente", null); }
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoEntradaPendiente", info);
        }
        public PartialViewResult BuscarVisitanteID(int id_log_g)
        {
            var info = db.C_establos_caseta_logs_g.Find(id_log_g);
            if (info == null) { return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoEntradaPendiente", null); }

            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoEntradaPendiente", info);
        }

        public PartialViewResult ConsultarBitacoraCaseta(string fecha_inicio, string fecha_fin, int id_establo)
        {
            DateTime inicio = DateTime.Parse(fecha_inicio);
            DateTime fin = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var info = db.C_establos_caseta_logs_g.Where(x => x.fecha_entrada >= inicio && x.fecha_entrada <= fin && x.activo == true && x.id_establo == id_establo).ToList();
            if (info == null) { return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ReporteBitacoraFechas", null); }

            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ReporteBitacoraFechas", info);
        }


        public int GuardarLogSalidaGeneral(int id_log_g)
        {
            try
            {
                DateTime hoy = DateTime.Now.AddHours(2);
                int id_usuario = (int)Session["LoggedId"];

                var log = db.C_establos_caseta_logs_g.Find(id_log_g);
                if (log == null) { return 1; }
                if (log.fecha_salida != null) { return 2; }

                log.fecha_salida = hoy;
                log.tiempo_estancia = (decimal)(hoy - log.fecha_entrada).Value.TotalSeconds;
                log.id_usuario_registra_salida = id_usuario;
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 3;
            }
        }

        public int GuardarLogSalidaBascula(int id_log_g, int id_tipo_tanque, string[] fichas, string[] placas, string[] choferes, string[] productos, string[] proveedores)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];

                var log = db.C_establos_caseta_logs_g.Find(id_log_g);
                if (log == null) { return 1; }
                if (log.fecha_salida != null) { return 2; }

                log.fecha_salida = hoy;
                log.id_usuario_registra_salida = id_usuario;
                log.tiempo_estancia = (decimal)(hoy - log.fecha_entrada).Value.TotalSeconds;

                for (int i = 0; i < fichas.Length; i++)
                {
                    C_establos_caseta_logs_d_visita_bascula log_d = new C_establos_caseta_logs_d_visita_bascula();
                    log_d.id_caseta_log_g = id_log_g;
                    log_d.ficha = fichas[i];
                    log_d.nombre_chofer = choferes[i];
                    log_d.proveedor = proveedores[i];
                    log_d.producto = productos[i];
                    log_d.placas = placas[i];
                    log_d.id_tipo_tanque_pipa = id_tipo_tanque;
                    log_d.activo = true;
                    db.C_establos_caseta_logs_d_visita_bascula.Add(log_d);
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 3;
            }
        }

        public int GuardarLogSalidaVale(int id_log_g, string folio_vale_salida)
        {
            try
            {
                DateTime hoy = DateTime.Now.AddHours(2);
                int id_usuario = (int)Session["LoggedId"];

                var log = db.C_establos_caseta_logs_g.Find(id_log_g);
                if (log == null) { return 1; }
                if (log.fecha_salida != null) { return 2; }

                var valid_folio = db.C_establos_caseta_vales_salida.Where(X => X.folio == folio_vale_salida && X.activo == true && X.registrado == false).FirstOrDefault();
                if (valid_folio == null) { return 3; }
                if (valid_folio.id_establo != log.id_establo) { return 4; }
                valid_folio.registrado = true;
               

                log.fecha_salida = hoy;
                log.tiempo_estancia = (decimal)(hoy - log.fecha_entrada).Value.TotalSeconds;
                log.id_usuario_registra_salida = id_usuario;

                C_establos_caseta_logs_d_visita_taller log_taller = new C_establos_caseta_logs_d_visita_taller();
                log_taller.id_caseta_log_g = id_log_g;
                log_taller.id_caseta_vale = valid_folio.id_caseta_vale;
                log_taller.fecha_registro = hoy;
                log_taller.id_usuario_registro = id_usuario;
                log_taller.entrada_salida = false; //true (0): ENTRADA, false(1): SALIDA
                log_taller.activo = true;
                db.C_establos_caseta_logs_d_visita_taller.Add(log_taller);
                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return 5;
            }
        }


        public PartialViewResult BuscarInformacionFicha(string ficha, int id_establo)
        {
            try
            {
                var salida_ganado = db.C_salida_ganado_g.Where(x => x.folio == ficha.Trim() && x.rstatus == "A" && x.id_establo == id_establo).FirstOrDefault();
                if (salida_ganado != null)
                {
                    List<string> data = new List<string>
                        {
                            salida_ganado.comprador,
                            salida_ganado.ganado,
                            salida_ganado.placas,
                            salida_ganado.chofer,
                            salida_ganado.C_salida_ganado_d.Count() + " " + salida_ganado.ganado,
                        };
                    return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoFichaBusqueda", data);
                }
                else
                {
                    int no_ficha = Convert.ToInt32(ficha);
                    var data_ficha = from fich in db.C_bascula_fichas
                                     join art in db.C_articulos_catalogo on fich.id_articulo_bascula equals art.id_articulo
                                     where fich.id_bascula_ficha == no_ficha && fich.id_establo == id_establo && fich.activo == true
                                     select fich;
                    if (data_ficha.Count() > 0)
                    {
                        List<string> data = new List<string>();
                        try
                        {
                            data.Add(data_ficha.FirstOrDefault().C_compras_proveedores.razon_social);
                        }
                        catch (Exception)
                        {
                            data.Add(data_ficha.FirstOrDefault().C_envios_leche_clientes.nombre_comercial);
                        }

                        data.Add(data_ficha.FirstOrDefault().C_articulos_catalogo.nombre_articulo);
                        data.Add(data_ficha.FirstOrDefault().placas);
                        data.Add(data_ficha.FirstOrDefault().chofer);
                        return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoFichaBusqueda", data);
                    }

                    using (var connection = Models.ConexionSab.Conectar())
                    {
                        ficha = ficha.Trim().PadLeft(25, ' ');
                        //1 SG, 2 SI, 3 SM, 4 TN
                        string query = "";
                        switch (id_establo)
                        {
                            case 1:
                                query = "SELECT prov.despro, art.desart, fichas.placas, fichas.chofer " +
                                    "FROM [base_forrajes].[dbo].[fichas_sg] fichas " +
                                    "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                    "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                                break;
                            case 2:
                                query = "SELECT prov.despro, art.desart, fichas.placas, fichas.chofer " +
                                           "FROM [base_forrajes].[dbo].[fichas_si] fichas " +
                                           "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                           "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                                break;
                            case 3:
                                query = "SELECT prov.despro, art.desart, fichas.placas, fichas.chofer " +
                                           "FROM [base_forrajes].[dbo].[fichas_sm] fichas " +
                                           "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                           "left join [base_forrajes].[dbo].[proveedo_B] prov on fichas.prov_cli = prov.cvepro where ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                                break;
                            case 4:
                                query = "SELECT prov.despro, art.desart, fichas.placas, fichas.chofer " +
                                           "FROM [base_forrajes].[dbo].[fichas_tn] fichas " +
                                           "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                           "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                                break;
                        }
                        SqlCommand command = new SqlCommand(query, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            if (reader.Read())
                            {
                                List<string> data = new List<string>
                                {
                                    reader[0].ToString(),
                                    reader[1].ToString(),
                                    reader[2].ToString(),
                                    reader[3].ToString(),
                                    ficha,
                                };
                                return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_InfoFichaBusqueda", data);
                            }

                            else { return null; }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception){
                return null;
            }
        }


        //----------------- VALES DE SALIDA
        public string GenerarFolioValeSalida(int id_establo)
        {
            try
            {
                int consecuivo;
                int generacion = 1;

                string siglas = db.C_establos.Find(id_establo).siglas;
                var valid = db.C_establos_caseta_vales_salida.Where(X => X.id_establo == id_establo).OrderByDescending(x => x.id_caseta_vale).FirstOrDefault();

                if (valid == null) { consecuivo = 0001; generacion = 01; }
                else
                {
                    consecuivo = Convert.ToInt32(valid.folio.Split('-')[2]) + 1;
                    generacion = Convert.ToInt32(valid.folio.Split('-')[1]);
                }

                if (consecuivo == 10000) { generacion = generacion++; consecuivo = 01; }

                string folio = siglas + "-" + generacion.ToString().PadLeft(2, '0') + "-" + consecuivo.ToString().PadLeft(4, '0');

                return folio;

            }
            catch (Exception)
            {
                return "ERROR AL GENERAR";
            }
        }

        public PartialViewResult ConsultarFormatoValeSalida(int id_establo)
        {
            try
            {
                //ViewBag.folio = GenerarFolioValeSalida(id_establo);
                ViewBag.id_establo = id_establo;
                var tipos_salida = db.C_establos_caseta_vales_tipos_salidas.Where(x => x.activo == true).OrderBy(x => x.nombre_tipo_salida).ToList();
                return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ValeSalidaFormato", tipos_salida);
            }
            catch (Exception)
            {
                return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ValeSalidaFormato", null);
            }
        }

        public PartialViewResult ConsultarValeSalidaImpresion(int id_caseta_vale)
        {
            try
            {
                var vale = db.C_establos_caseta_vales_salida.Find(id_caseta_vale);
                return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ValeSalidaImpresion", vale);
            }
            catch (Exception)
            {
                return null;
                //return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_ValeSalidaImpresion", null);
            }
        }

        public int GenerarValeSalida(int id_establo, string folio, int id_area, string descripcion, int id_tipo_salida, string tipo_salida, string proveedor, string tipo_activo, string id_activo)
        {
            try
            {
                var valid_folio = db.C_establos_caseta_vales_salida.Where(x => x.folio == folio.Trim()).FirstOrDefault();
                if (valid_folio != null) { return -1; }

                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];
                C_establos_caseta_vales_salida vale_salida = new C_establos_caseta_vales_salida();
                vale_salida.id_establo = id_establo;
                vale_salida.folio = folio;
                vale_salida.id_area = id_area;
                vale_salida.fecha_registro = hoy;
                vale_salida.descripcion = descripcion;
                vale_salida.id_usuario_registra = id_usuario;
                vale_salida.id_caseta_tipo_salida = id_tipo_salida;
                vale_salida.tipo_salida = tipo_salida;
                vale_salida.proveedor = proveedor;
                vale_salida.nombre_activo = tipo_activo;
                vale_salida.id_activo = id_activo;
                vale_salida.registrado = false;
                vale_salida.activo = true;
                db.C_establos_caseta_vales_salida.Add(vale_salida);
                db.SaveChanges();
                return vale_salida.id_caseta_vale;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public PartialViewResult ConsultarValesGenerados(string fecha_inicio, string fecha_fin, int id_establo)
        {
            DateTime fecha1 = DateTime.Parse(fecha_inicio);
            DateTime fecha2 = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var vales = db.C_establos_caseta_vales_salida.Where(x => x.id_establo == id_establo && x.activo == true && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2).ToList();
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_HistorialValesGenerados", vales);
        }

        public PartialViewResult ConsultarDetalleValeSalida(int id_vale_salida_g)
        {
            var detalle = db.C_establos_caseta_logs_d_visita_taller.Where(x => x.id_caseta_vale == id_vale_salida_g && x.activo == true).OrderBy(x => x.id_caseta_vale_log).ToList();
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_DetalleValeSalidaTable", detalle);
        }



    }
}