using Beta_System.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

namespace Beta_System.Controllers
{
    public class ALIMENTACIONController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        CATALOGOSController catalogos = new CATALOGOSController();
        NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();

        #region RECEPCION
        public ActionResult RecepcionFichasBasculaForrajes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8072)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("Recepcion/Index");
        }


        #region RECEPCION DE FICHAS
        public PartialViewResult ConsultarFichasPendientesRecepcion(/*string fecha_inicio, string fecha_fin,*/ int[] id_establo)
        {
            int id_usuario = (int)Session["LoggedId"];
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            //DateTime fecha_i = DateTime.Parse(fecha_inicio);
            //DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var fichas = db.C_bascula_fichas.Where(x => /*x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f &&*/ id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == false && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && x.id_compras_proveedor != null  //ENTRADA - FORRAJES
                                                    && x.peso_1 > x.peso_2 && x.termina == true && x.activo == true).OrderBy(x => x.id_envio_leche_cliente_ms).ToList();
            return PartialView("Recepcion/_FichasPendientesRecepcionTableCheck", fichas);
        }

        public int RecepcionarPendientesRecepcion(int[] id_bascula_fichas /*, int[] id_proveedores*/)
        {

            try
            {
                //if (id_proveedores.Distinct().Count() > 1) { return 1; }  //PROVEEDORES DIFERENTES

                db.C_bascula_fichas.Where(x => id_bascula_fichas.Contains(x.id_bascula_ficha) && x.activo == true && x.recibido == false).ToList().ForEach(x => x.recibido = true);
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


        #region RECEPCIONES FACTURADAS
        public PartialViewResult ConsultarFichasRecepcionadas(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_proveedor)
        {
            int id_usuario = (int)Session["LoggedId"];
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }

            if (id_proveedor.Contains(0)) { id_proveedor = db.C_compras_proveedores.Where(x => x.activo == true && x.disponible_bascula == true).Select(x => x.id_compras_proveedor).ToArray(); }

            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //&& x.id_compras_proveedor != null  //ENTRADA - FORRAJES
                                                    && x.activo == true).ToList();

            var id_fichas = fichas.Select(x => x.id_bascula_ficha).ToArray();
            var fichas_racturadas = db.C_alimentacion_forrajes_facturas_d.Where(x => x.activo == true && id_fichas.Contains((int)x.id_bascula_ficha)).Select(x => x.id_bascula_ficha).ToArray();
            if (fichas_racturadas.Count() > 0) { fichas = fichas.Where(x => !fichas_racturadas.Contains((int)x.id_bascula_ficha)).ToList(); }

            return PartialView("Recepcion/_FichasRecepcionadasTableCheck", fichas);
        }

        public int GenerarFacturaFichasRecepcionadas(string folio_factura, string fecha, decimal toneladas, decimal importe, int id_moneda, decimal tipo_cambio, int id_compras_proveedor,
                                                        int[] id_fichas, decimal[] toneladas_confirm, decimal[] precios_confirm, int[] id_monedas)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                DateTime fecha_factura = DateTime.Parse(fecha);
                C_alimentacion_forrajes_facturas_g factura_g = new C_alimentacion_forrajes_facturas_g();
                factura_g.folio_factura = folio_factura;
                factura_g.id_compras_proveedor = id_compras_proveedor;
                factura_g.fecha_factura = fecha_factura;
                factura_g.toneladas_totales = toneladas;
                factura_g.importe = importe;
                factura_g.id_tipo_moneda = id_moneda;
                factura_g.tipo_cambio = tipo_cambio;
                factura_g.fecha_registro = hoy;
                factura_g.id_usuario_registra = id_usuario;
                factura_g.activo = true;
                db.C_alimentacion_forrajes_facturas_g.Add(factura_g);
                db.SaveChanges();
                int id_factura_g = factura_g.id_alimentacion_factura_forraje_g;
                for (int i = 0; i < id_fichas.Length; i++)
                {
                    C_alimentacion_forrajes_facturas_d factura_d = new C_alimentacion_forrajes_facturas_d();
                    factura_d.id_alimentacion_factura_forraje_g = id_factura_g;
                    factura_d.id_bascula_ficha = id_fichas[i];
                    factura_d.toneladas_ficha = toneladas_confirm[i];
                    factura_d.precio_ficha = precios_confirm[i];
                    factura_d.id_moneda_tipo = id_monedas[i];
                    factura_d.activo = true;
                    db.C_alimentacion_forrajes_facturas_d.Add(factura_d);
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        #endregion


        #region FACTURAS DE FORRAJE
        public PartialViewResult ConsultarFacturasForrajesTable(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_proveedor, int[] id_alimento)
        {

            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            if (id_proveedor.Contains(0)) { id_proveedor = catalogos.ConsultarProveedoresAlimentacionID(); }
            if (id_alimento.Contains(0)) { id_alimento = catalogos.ConsultarArticulosTipoFichasBasculaID(2); }

            var facturas = db.C_alimentacion_forrajes_facturas_d.Where(x => x.activo == true && x.C_alimentacion_forrajes_facturas_g.fecha_factura >= fecha_i && x.C_alimentacion_forrajes_facturas_g.fecha_registro <= fecha_f
                            && id_establo.Contains((int)x.C_bascula_fichas.id_establo) && x.C_alimentacion_forrajes_facturas_g.activo == true
                            && id_proveedor.Contains((int)x.C_alimentacion_forrajes_facturas_g.id_compras_proveedor)
                            && id_alimento.Contains((int)x.C_bascula_fichas.id_articulo_bascula)).Select(x => x.C_alimentacion_forrajes_facturas_g).OrderBy(x => x.fecha_factura).Distinct().ToList();

            return PartialView("Recepcion/_FacturasForrajesTable", facturas);
        }

        public PartialViewResult ConsultarFacturaForrajeDetalle(int id_factura_forraje_g)
        {
            var detalle = db.C_alimentacion_forrajes_facturas_d.Where(x => x.id_alimentacion_factura_forraje_g == id_factura_forraje_g && x.activo == true).ToList();
            return PartialView("Recepcion/_FacturasForrajesDetalle", detalle);
        }

        public bool EliminarFactura(int id_alimentacion_factura_forraje_g)
        {
            try
            {
                var detalle = db.C_alimentacion_forrajes_facturas_d.Where(x => x.id_alimentacion_factura_forraje_g == id_alimentacion_factura_forraje_g).ToList();
                db.C_alimentacion_forrajes_facturas_d.RemoveRange(detalle);
                db.SaveChanges();

                var general = db.C_alimentacion_forrajes_facturas_g.Find(id_alimentacion_factura_forraje_g);
                db.C_alimentacion_forrajes_facturas_g.Remove(general);
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


        #endregion


        #region PROGRAMA DE ALIMENTO
        public ActionResult ProgramaAlimentacion()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8076)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("ProgramaAlimento/Index");
        }

        public PartialViewResult CalcularSemanasNuevoPrograma(string fecha_inicio, int no_semanas)
        {
            DateTime today = DateTime.Parse(fecha_inicio);
            int diaSemana = (int)today.DayOfWeek;
            DateTime inicioSemana = today.AddDays(-(diaSemana == 0 ? 6 : diaSemana - 1));
            //DateTime finSemana = inicioSemana.AddDays(6);

            ViewBag.fecha_inicio = inicioSemana;
            ViewBag.no_semanas = no_semanas;

            return PartialView("ProgramaAlimento/_FormularioNuevoPrograma");
        }

        public int GuardarNuevoProgramaAlimentos(string fecha_inicio, int no_semanas, int id_establo, string referencia_gen_prg, string[] fechas, int[] articulos, decimal[] toneladas)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime HOY = DateTime.Now;
                C_alimentacion_programa_g prog_g = new C_alimentacion_programa_g();
                prog_g.id_establo = id_establo;
                prog_g.fecha_inicio = DateTime.Parse(fecha_inicio);
                prog_g.nombre_programacion = referencia_gen_prg;
                prog_g.id_usuario_registra = id_usuario;
                prog_g.fecha_registro = HOY;
                prog_g.semanas_programa = no_semanas;
                prog_g.activo = true;
                db.C_alimentacion_programa_g.Add(prog_g);
                db.SaveChanges();
                int id_prog_g = prog_g.id_alimentacion_programa_g;
                try
                {
                    for (int i = 0; i < articulos.Length; i++)
                    {
                        C_alimentacion_programa_d prog_d = new C_alimentacion_programa_d();
                        prog_d.id_alimentacion_programa_g = id_prog_g;
                        prog_d.id_articulo = articulos[i];
                        prog_d.toneladas = toneladas[i];
                        prog_d.fecha_programacion = DateTime.Parse(fechas[i]);
                        prog_d.activo = true;
                        db.C_alimentacion_programa_d.Add(prog_d);
                    }
                    db.SaveChanges();
                    return 0;
                }
                catch (Exception)
                {
                    prog_g.activo = false;
                    db.SaveChanges();
                    return -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public PartialViewResult ConsultarProgramasAlimentacionTable(int[] id_establo, string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin);
            var programas = db.C_alimentacion_programa_g.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo) && x.fecha_inicio >= fecha_i && x.fecha_inicio <= fecha_f).ToList();
            return PartialView("ProgramaAlimento/_ProgramasAlimentacionTable", programas);
        }

        public PartialViewResult ConsultarProgramaAlimentacionDetalle(int id_programa_alimentacion_g)
        {
            var detalle = db.C_alimentacion_programa_d.Where(x => x.id_alimentacion_programa_g == id_programa_alimentacion_g && x.activo == true).ToList();
            ViewBag.fecha_inicio = detalle.Select(x => x.fecha_programacion).Min().Value;
            return PartialView("ProgramaAlimento/_DetalleProgramaAlimentacion", detalle);
        }

        #endregion


        #region DIETAS
        public ActionResult AdministrarDietas()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8085)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("Dietas/Index");
        }

        public string ConsultarPrecioAlimento(int id_alimento)
        {
            var precio = db.C_alimentacion_articulos_precios.Where(x => x.id_articulo == id_alimento && x.activo == true).FirstOrDefault();
            if (precio != null) { return precio.precio_articulo.ToString(); }
            return "0.00";
        }

        public string ConsultarUltimoMSAlimento(int id_alimento)
        {
            var MS = db.C_alimentacion_dietas_d.Where(x => x.id_articulo == id_alimento && x.activo == true && x.C_alimentacion_dietas_g.id_dieta_status == 4).OrderByDescending(x => x.id_dieta_d).FirstOrDefault();
            if (MS != null) { return Convert.ToInt32(MS.porcentaje_MS.Value).ToString(); }
            return "0";
        }


        public PartialViewResult ConsultarProyeccionDieta(int id_dieta_g)
        {
            List<C_alimentacion_dietas_g_valores> data = new List<C_alimentacion_dietas_g_valores>();

            //BASE
            var valores_dieta = db.C_alimentacion_dietas_g_valores.Where(x => x.id_dieta_g == id_dieta_g && x.activo == true && x.id_dieta_g_valor_tipo == 1).FirstOrDefault();
            if (valores_dieta != null)
            {
                data.Add(valores_dieta);
            }

            //ACTUAL CON NUEVOS PRECIOS
            if (valores_dieta.C_alimentacion_dietas_g.id_dieta_g_copia != null) { 
                int id_dieta_copia = (int)valores_dieta.C_alimentacion_dietas_g.id_dieta_g_copia;
                var detalle_dieta = db.C_alimentacion_dietas_d.Where(x => x.id_dieta_g == id_dieta_copia && x.activo == true).ToList();
                decimal total_KgMs = 0, total_KgBH = 0, total_CostoBH = 0;

                foreach (var ing in detalle_dieta)
                {
                    decimal precio_art = Math.Round(Convert.ToDecimal(ConsultarPrecioAlimento((int)ing.id_articulo)), 2);
                    total_KgMs += Math.Round((decimal)ing.kilos_MS, 3);
                    total_KgBH += Math.Round((decimal)ing.kilos_BH, 3);
                    decimal valor_convertido = Convert.ToDecimal(ing.kilos_BH.Value.ToString("0.000"));
                    decimal valor_costo_bg = precio_art * valor_convertido;
                    total_CostoBH += valor_costo_bg;
                }
                C_alimentacion_dietas_g_valores valores_actuales = new C_alimentacion_dietas_g_valores();
                valores_actuales.total_kilos_MS = Math.Round(total_KgMs, 3);
                valores_actuales.total_kilos_BH = Math.Round(total_KgBH, 3);
                valores_actuales.total_costo_BH = Math.Round(total_CostoBH, 2);
                valores_actuales.total_costo_kilos_MS = 0;
                if (total_CostoBH > 0)
                {
                    valores_actuales.total_costo_kilos_MS = Math.Round(total_CostoBH / total_KgMs, 2);
                }
                valores_actuales.id_dieta_g = id_dieta_g;
                data.Add(valores_actuales);
            }
            else
            {
                var detalle_dieta = db.C_alimentacion_dietas_d.Where(x => x.id_dieta_g == id_dieta_g && x.activo == true).ToList();
                decimal total_KgMs = 0, total_KgBH = 0, total_CostoBH = 0;

                foreach (var ing in detalle_dieta)
                {
                    decimal precio_art = Convert.ToDecimal(ConsultarPrecioAlimento((int)ing.id_articulo));
                    total_KgMs += Math.Round((decimal)ing.kilos_MS, 3);
                    total_KgBH += Math.Round((decimal)ing.kilos_BH, 3);
                    decimal valor_convertido = Convert.ToDecimal(ing.kilos_BH.Value.ToString("0.000"));
                    decimal valor_costo_bg = precio_art * valor_convertido;
                    total_CostoBH += Convert.ToDecimal(valor_costo_bg.ToString("0.00"));
                }
                C_alimentacion_dietas_g_valores valores_actuales = new C_alimentacion_dietas_g_valores();
                valores_actuales.total_kilos_MS = Math.Round(total_KgMs, 2);
                valores_actuales.total_kilos_BH = Math.Round(total_KgBH, 2);
                valores_actuales.total_costo_BH = Math.Round(total_CostoBH, 2);
                valores_actuales.total_costo_kilos_MS = Math.Round(total_CostoBH / total_KgMs, 2);
                valores_actuales.id_dieta_g = id_dieta_g;
                data.Add(valores_actuales);
            }
            
            // DIETA NUEVA
            var valores_dieta_actual = db.C_alimentacion_dietas_g_valores.Where(x => x.id_dieta_g == id_dieta_g && x.activo == true && x.id_dieta_g_valor_tipo == 2).FirstOrDefault();
            if (valores_dieta_actual == null)
            {
                data.Add(valores_dieta);
            }
            else
            {
                data.Add(valores_dieta_actual);
            }

            //DIFERENCIA
            C_alimentacion_dietas_g_valores diferencias = new C_alimentacion_dietas_g_valores();
            diferencias.total_kilos_BH = (decimal)data[2].total_kilos_BH - data[1].total_kilos_BH;
            diferencias.total_kilos_MS = (decimal)data[2].total_kilos_MS - (decimal)data[1].total_kilos_MS;
            diferencias.total_costo_BH = data[2].total_costo_BH - (decimal)data[1].total_costo_BH;
            diferencias.total_costo_kilos_MS = (decimal)data[2].total_costo_kilos_MS - (decimal)data[1].total_costo_kilos_MS;
            diferencias.id_dieta_g = id_dieta_g;
            data.Add(diferencias);

            return PartialView("Dietas/_ProyeccionValoresDieta", data);
        }

        public PartialViewResult ConsultarDietasEstatusTable(int id_status)
        {
            ViewBag.id_status = id_status;
            var dietas = db.C_alimentacion_dietas_g.Where(x => x.activo == true && x.id_dieta_status == id_status).ToList();
            return PartialView("Dietas/_DietasStatusTable", dietas);
        }

        public PartialViewResult ConsultarDietasGeneralTable(int[] id_status, int[] id_establo)
        {
            int id_usuario = (int)Session["LoggedId"];
            ViewBag.id_status = id_status;
            if (id_status.Contains(0)) { id_status = catalogos.ConsultarEstatusDietasID(); }
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }

            var dietas = db.C_alimentacion_dietas_g.Where(x => x.activo == true && id_status.Contains((int)x.id_dieta_status) && id_establo.Contains((int)x.id_establo)).ToList();
            return PartialView("Dietas/_DietasGeneralTable", dietas);
        }

        public PartialViewResult ConsultarDietaDetalle(int id_dieta_g, int id_status)
        {
            ViewBag.id_status = id_status;
            var detalle = db.C_alimentacion_dietas_d.Where(x => x.activo == true && x.id_dieta_g == id_dieta_g).ToList();
            //if (detalle.Count() > 0) { if (detalle.FirstOrDefault().C_alimentacion_dietas_g.id_dieta_status != id_status) { return PartialView("Dietas/_DietaDetalle", null); } }
            return PartialView("Dietas/_DietaDetalle", detalle);
        }

        public int ActualizarStatusDieta(int id_dieta_g, int id_status)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                //var dieta = new C_alimentacion_dietas_g { id_dieta_g = id_dieta_g };
                //db.C_alimentacion_dietas_g.Attach(dieta);
                //dieta.id_dieta_status = id_status;
                //db.Entry(dieta).Property(x => x.id_dieta_status).IsModified = true;
                var dieta = db.C_alimentacion_dietas_g.Find(id_dieta_g);
                dieta.id_dieta_status = id_status;

                if (id_status == 1) { dieta.id_usuario_autoriza = null; dieta.id_usuario_valida = null; dieta.id_usuario_revisa = null; }

                if (id_status == 2) { dieta.id_usuario_revisa = id_usuario; db.Entry(dieta).Property(x => x.id_usuario_revisa).IsModified = true; }  //REVISADA -> A AUTORIZACION
                if (id_status == 3) { dieta.id_usuario_autoriza = id_usuario; db.Entry(dieta).Property(x => x.id_usuario_autoriza).IsModified = true; } //AUTORIZADA
                if (id_status == 4) { dieta.id_usuario_valida = id_usuario; db.Entry(dieta).Property(x => x.id_usuario_valida).IsModified = true; }  //APLICADA
                //if (id_status == 6) { dieta.id_usuario_revisa = id_usuario; db.Entry(dieta).Property(x => x.id_usuario_revisa).IsModified = true; }  //REVISION

                if (id_status == 2 || id_status == 4) //ENVIADA A AUTORIZACION Y APLICADA
                {
                    string siglas_establo = dieta.C_establos.siglas;
                    string grupo_dieta = dieta.C_alimentacion_dietas_grupos.nombre_grupo;

                    string destinatario = "";
                    string asunto = "";
                    int id_accion_master = 0;

                    string fecha_inicio = dieta.fecha_inicio.Value.ToString("dd-MMM-yyyy", new CultureInfo("es-ES"));
                    string fecha_fin = dieta.fecha_fin.Value.ToString("dd-MMM-yyyy", new CultureInfo("es-ES"));
                    string usuario = "";
                    string status = "";


                    if (id_status == 2)
                    {
                        status = "Registró";
                        usuario = dieta.C_usuarios_corporativo.usuario;
                        id_accion_master = 2012;
                        asunto = "Nueva Dieta a revisión (" + siglas_establo + " - " + grupo_dieta + ")";
                    }
                    if (id_status == 4)
                    {
                        status = "Aplicó";
                        usuario = dieta.C_usuarios_corporativo2.usuario;
                        id_accion_master = 2013;
                        asunto = "Dieta aplicada (" + siglas_establo + " - " + grupo_dieta + ")";
                    }

                    string mensaje = "<h2 style='text-align: left'>" + dieta.descripcion + "</h2>" +
                            "<br />" +
                            "<table style='border:1px solid;'>" +
                                "<tr>" +
                                    "<td>Inicio:</td>" +
                                    "<td>" + fecha_inicio + "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td>Fin planeado:</td>" +
                                    "<td>" + fecha_fin + "</td>" +
                                "</tr>" +
                                "<tr>" +
                                    "<td>" + status + ":</td>" +
                                    "<td>" + usuario + "</td>" +
                                "</tr>" +
                            "</table>" +
                            "<br />" +
                            "<strong>Visita https://siib.beta.com.mx/ALIMENTACION/AdministrarDietas para mas información</strong>";

                    foreach (var item in db.C_usuarios_masters.Where(x => x.activo == true && x.id_usuario_master_accion == id_accion_master))
                    {
                        destinatario += item.correo + ";";
                    }

                    //notificaciones.EnviarCorreoUsuarioReportes(asunto, destinatario, mensaje);
                }


                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public PartialViewResult CopiarInforamcionDietaNewDieta(int id_dieta_g)
        {
            var dieta = db.C_alimentacion_dietas_g.Find(id_dieta_g);
            return PartialView("Dietas/_CopiaDietaExistenteNewDieta", dieta);
        }

        public int GuardarNewDieta(int id_establo, int id_grupo, string fecha_inicio, string fecha_fin, string nombre, string desc, int id_dieta_copia,
            int[] id_articulos, decimal[] porcMs, decimal[] kgMs, decimal[] kgMs_base, decimal[] kgBh, decimal[] precio, decimal[] costoBh, 
            decimal total_kg_ms, decimal total_kg_bh, decimal total_costo_bh, decimal total_costo_kg_bh, string comentario)
        {
            int id_dieta_g = 0;
            C_alimentacion_dietas_g dieta_g = new C_alimentacion_dietas_g();
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin);
                DateTime Hoy = DateTime.Now;

                dieta_g.id_establo = id_establo;
                dieta_g.id_dieta_grupo = id_grupo;
                dieta_g.nombre = nombre;
                dieta_g.descripcion = desc;
                dieta_g.fecha_inicio = fecha_i;
                dieta_g.fecha_fin = fecha_f;
                dieta_g.fecha_registro = Hoy;
                dieta_g.id_usuario_registra = id_usuario;
                dieta_g.id_dieta_status = 1;
                dieta_g.activo = true;
                if (id_dieta_copia > 0) { dieta_g.id_dieta_g_copia = id_dieta_copia; }
                db.C_alimentacion_dietas_g.Add(dieta_g);
                db.SaveChanges();
                id_dieta_g = dieta_g.id_dieta_g;

                if (comentario != "" || comentario.Length > 1)
                {
                    C_alimentacion_dietas_g_comentarios new_dieta_comentario = new C_alimentacion_dietas_g_comentarios();
                    new_dieta_comentario.id_dieta_g = id_dieta_g;
                    new_dieta_comentario.id_usuario_registra = id_usuario;
                    new_dieta_comentario.fecha_registro = Hoy;
                    new_dieta_comentario.comentario = comentario;
                    new_dieta_comentario.activo = true;
                    db.C_alimentacion_dietas_g_comentarios.Add(new_dieta_comentario);
                    //db.SaveChanges();
                }

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    C_alimentacion_dietas_d dieta_d = new C_alimentacion_dietas_d();
                    dieta_d.id_dieta_g = id_dieta_g;
                    dieta_d.id_articulo = id_articulos[i];
                    dieta_d.porcentaje_MS = porcMs[i];
                    dieta_d.kilos_MS = kgMs[i];
                    dieta_d.kilos_MS_base = kgMs_base[i];
                    dieta_d.kilos_BH = kgBh[i];
                    dieta_d.precio = precio[i];
                    dieta_d.costo_BH = costoBh[i];
                    dieta_d.activo = true;
                    db.C_alimentacion_dietas_d.Add(dieta_d);
                }
                db.SaveChanges();

                if (id_dieta_copia > 0)
                {
                    var dieta_base = db.C_alimentacion_dietas_g_valores.Where(x => x.id_dieta_g == id_dieta_copia && x.activo == true && x.id_dieta_g_valor_tipo == 2).FirstOrDefault();
                    if (dieta_base != null)
                    {
                        C_alimentacion_dietas_g_valores dieta_valores_base = new C_alimentacion_dietas_g_valores();
                        dieta_valores_base.id_dieta_g = id_dieta_g;
                        dieta_valores_base.total_kilos_MS = dieta_base.total_kilos_MS;
                        dieta_valores_base.total_kilos_BH = dieta_base.total_kilos_BH;
                        dieta_valores_base.total_costo_BH = dieta_base.total_costo_BH;
                        dieta_valores_base.total_costo_kilos_MS = dieta_base.total_costo_kilos_MS;
                        dieta_valores_base.activo = true;
                        dieta_valores_base.id_dieta_g_valor_tipo = 1; //BASE - COPIA
                        db.C_alimentacion_dietas_g_valores.Add(dieta_valores_base);

                        C_alimentacion_dietas_g_valores dieta_valores = new C_alimentacion_dietas_g_valores();
                        dieta_valores.id_dieta_g = id_dieta_g;
                        dieta_valores.total_kilos_MS = total_kg_ms;
                        dieta_valores.total_kilos_BH = total_kg_bh;
                        dieta_valores.total_costo_BH = total_costo_bh;
                        dieta_valores.total_costo_kilos_MS = total_costo_kg_bh;
                        dieta_valores.activo = true;
                        dieta_valores.id_dieta_g_valor_tipo = 2; //ACTUALIZACION - CREACION
                        db.C_alimentacion_dietas_g_valores.Add(dieta_valores);
                        db.SaveChanges();
                    }
                }
                else
                {
                    C_alimentacion_dietas_g_valores dieta_valores = new C_alimentacion_dietas_g_valores();
                    dieta_valores.id_dieta_g = id_dieta_g;
                    dieta_valores.total_kilos_MS = total_kg_ms;
                    dieta_valores.total_kilos_BH = total_kg_bh;
                    dieta_valores.total_costo_BH = total_costo_bh;
                    dieta_valores.total_costo_kilos_MS = total_costo_kg_bh;
                    dieta_valores.activo = true;
                    dieta_valores.id_dieta_g_valor_tipo = 1; //BASE - CREACION
                    db.C_alimentacion_dietas_g_valores.Add(dieta_valores);

                    C_alimentacion_dietas_g_valores dieta_valores_edit = new C_alimentacion_dietas_g_valores();
                    dieta_valores_edit.id_dieta_g = id_dieta_g;
                    dieta_valores_edit.total_kilos_MS = total_kg_ms;
                    dieta_valores_edit.total_kilos_BH = total_kg_bh;
                    dieta_valores_edit.total_costo_BH = total_costo_bh;
                    dieta_valores_edit.total_costo_kilos_MS = total_costo_kg_bh;
                    dieta_valores_edit.activo = true;
                    dieta_valores_edit.id_dieta_g_valor_tipo = 2; //COPIA
                    db.C_alimentacion_dietas_g_valores.Add(dieta_valores_edit);
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception)
            {
                try
                {
                    dieta_g.activo = false;
                    if (id_dieta_g != 0)
                    {
                        db.C_alimentacion_dietas_d.Where(x => x.id_dieta_g == id_dieta_g).ToList().ForEach(x => x.activo = false);
                    }
                    db.SaveChanges();
                    return 1;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
        }

        public int ActualizarDieta(int id_dieta_g, int[] id_articulos, decimal[] porcMs, decimal[] kgMs, decimal[] kgMs_base, decimal[] kgBh, decimal[] precio, decimal[] costoBh, decimal total_kg_ms, decimal total_kg_bh, decimal total_costo_bh, decimal total_costo_kg_bh)
        {
            try
            {
                db.C_alimentacion_dietas_d.Where(x => x.id_dieta_g == id_dieta_g).ToList().ForEach(x => x.activo = false);
                db.SaveChanges();

                for (int i = 0; i < id_articulos.Length; i++)
                {
                    int id_articulo = id_articulos[i];
                    var valid_detalle = db.C_alimentacion_dietas_d.Where(x => x.id_articulo == id_articulo && x.id_dieta_g == id_dieta_g).FirstOrDefault();
                    if (valid_detalle == null)
                    {
                        C_alimentacion_dietas_d dieta_d = new C_alimentacion_dietas_d();
                        dieta_d.id_dieta_g = id_dieta_g;
                        dieta_d.id_articulo = id_articulos[i];
                        dieta_d.porcentaje_MS = porcMs[i];
                        dieta_d.kilos_MS = kgMs[i];
                        dieta_d.kilos_MS_base = kgMs_base[i];
                        dieta_d.kilos_BH = kgBh[i];
                        dieta_d.precio = precio[i];
                        dieta_d.costo_BH = costoBh[i];
                        dieta_d.activo = true;
                        db.C_alimentacion_dietas_d.Add(dieta_d);
                        db.SaveChanges();
                    }
                    else
                    {
                        valid_detalle.porcentaje_MS = porcMs[i];
                        valid_detalle.kilos_MS = kgMs[i];
                        valid_detalle.kilos_MS_base = kgMs_base[i];
                        valid_detalle.kilos_BH = kgBh[i];
                        valid_detalle.precio = precio[i];
                        valid_detalle.costo_BH = costoBh[i];
                        valid_detalle.activo = true;
                        db.SaveChanges();
                    }
                }

                var valid_valores = db.C_alimentacion_dietas_g_valores.Where(x => x.id_dieta_g == id_dieta_g && x.activo == true && x.id_dieta_g_valor_tipo == 2).FirstOrDefault();
                if (valid_valores == null)
                {
                    C_alimentacion_dietas_g_valores dieta_valores = new C_alimentacion_dietas_g_valores();
                    dieta_valores.id_dieta_g = id_dieta_g;
                    dieta_valores.total_kilos_MS = total_kg_ms;
                    dieta_valores.total_kilos_BH = total_kg_bh;
                    dieta_valores.total_costo_BH = total_costo_bh;
                    dieta_valores.total_costo_kilos_MS = total_costo_kg_bh;
                    dieta_valores.id_dieta_g_valor_tipo = 2; //ACTUAL - EDICION
                    dieta_valores.activo = true;
                    db.C_alimentacion_dietas_g_valores.Add(dieta_valores);
                    db.SaveChanges();
                }
                else
                {
                    valid_valores.total_kilos_MS = total_kg_ms;
                    valid_valores.total_kilos_BH = total_kg_bh;
                    valid_valores.total_costo_BH = total_costo_bh;
                    valid_valores.total_costo_kilos_MS = total_costo_kg_bh;
                    valid_valores.activo = true;
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public int AgregarComentarioDieta(int id_dieta_g, string comentario)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                C_alimentacion_dietas_g_comentarios nuevoComentario = new C_alimentacion_dietas_g_comentarios
                {
                    id_dieta_g = id_dieta_g,
                    comentario = comentario,
                    activo = true,
                    id_usuario_registra = id_usuario,
                    fecha_registro = hoy
                };

                db.C_alimentacion_dietas_g_comentarios.Add(nuevoComentario);
                db.SaveChanges();
                return 0; // Éxito
            }
            catch (Exception)
            {
                return 1; // Error
            }
        }

        public bool ActualizarInformacionDietaGeneral(int id_dieta_g, string nombre, string descripcion, string fecha_inicio, string fecha_fin)
        {
            try
            {
                var dieta = db.C_alimentacion_dietas_g.Find(id_dieta_g);
                dieta.nombre = nombre;
                dieta.descripcion = descripcion;
                dieta.fecha_inicio = DateTime.Parse(fecha_inicio);
                dieta.fecha_fin = DateTime.Parse(fecha_fin);
                db.SaveChanges();
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