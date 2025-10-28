using Beta_System.Models;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class ENVIOLECHEController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        NOTIFICACIONESController notificacion = new NOTIFICACIONESController();



        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7034)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../ESTABLOS/EnviosLeche/Index");
        }

        #region CALIDAD LECHE LABORATORIO
        public PartialViewResult SiloCalidadLecheTable(int count_tabla_lab, int id_silo, string silo)
        {
            ViewBag.countLab = count_tabla_lab;
            ViewBag.idsiloLab = id_silo;
            ViewBag.silo = silo;

            return PartialView("../ESTABLOS/EnviosLeche/Laboratorio/_SiloCalidadLecheTable");
        }

        public int RegistroCalidadLecheLab(C_envios_leche_calidad c_calidad, int[] id_silos)
        {
            C_envios_leche_calidad calidad = new C_envios_leche_calidad();
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                calidad.muestra_calidad = c_calidad.muestra_calidad;
                calidad.temperatura = c_calidad.temperatura;
                calidad.sala = c_calidad.sala;
                calidad.proteina = c_calidad.proteina;
                calidad.lactosa = c_calidad.lactosa;
                calidad.crioscopia = c_calidad.crioscopia;
                calidad.solidos_no_grasos = c_calidad.solidos_no_grasos;
                calidad.caseina = c_calidad.caseina;
                calidad.urea = c_calidad.urea;
                calidad.grasa = c_calidad.grasa;
                calidad.solidos_totales = c_calidad.solidos_totales;
                calidad.acidez = c_calidad.acidez;
                calidad.ccs = c_calidad.ccs;
                calidad.antibiotico = c_calidad.antibiotico;
                calidad.betalactamicos = c_calidad.betalactamicos;
                calidad.sulfasimas = c_calidad.sulfasimas;
                calidad.tetraciclina = c_calidad.tetraciclina;
                calidad.alcohol_75 = c_calidad.alcohol_75;
                calidad.aflatoxinas = c_calidad.aflatoxinas;
                calidad.activo = true;
                calidad.id_usuario_registra = id_usuario;
                calidad.fecha_registro = DateTime.Now;
                calidad.id_establo = c_calidad.id_establo;
                db.C_envios_leche_calidad.Add(calidad);
                db.SaveChanges();

                foreach (var item in id_silos)
                {
                    C_envios_leche_calidad_silos calidad_silos = new C_envios_leche_calidad_silos();
                    calidad_silos.activo = true;
                    calidad_silos.id_envio_leche_calidad = calidad.id_envio_leche_calidad; ;
                    calidad_silos.id_envio_leche_silo = item;
                    db.C_envios_leche_calidad_silos.Add(calidad_silos);
                }
                db.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public PartialViewResult CalidadLecheTable(DateTime fecha_inicio, DateTime fecha_fin)
        {
            DateTime fecha_final = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);
            var calidad = db.C_envios_leche_calidad.Where(x => x.activo == true && x.fecha_registro > fecha_inicio && x.fecha_registro < fecha_final).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/Laboratorio/_CalidadLecheTable", calidad);
        }

        //ConsultarInfoCalidadLecheLab
        public string ConsultarInfoCalidadLecheLab(int id_envio_leche_calidad)
        {
            var calidad = from calid in db.C_envios_leche_calidad
                          where calid.id_envio_leche_calidad == id_envio_leche_calidad
                          select new
                          {
                              calid.id_envio_leche_calidad,
                              calid.muestra_calidad,
                              calid.temperatura,
                              calid.sala,
                              calid.proteina,
                              calid.lactosa,
                              calid.crioscopia,
                              calid.solidos_no_grasos,
                              calid.caseina,
                              calid.urea,
                              calid.grasa,
                              calid.solidos_totales,
                              calid.acidez,
                              calid.ccs,
                              calid.antibiotico,
                              calid.betalactamicos,
                              calid.sulfasimas,
                              calid.tetraciclina,
                              calid.alcohol_75,
                              calid.aflatoxinas
                          };
            return Newtonsoft.Json.JsonConvert.SerializeObject(calidad);
        }


        public int ConfirmacionCalidadLecheLab(int id_envio_leche_calidad, C_envios_leche_calidad c_calidad)
        {
            var calidad = db.C_envios_leche_calidad.Find(id_envio_leche_calidad);
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                calidad.muestra_calidad = c_calidad.muestra_calidad;
                calidad.temperatura = c_calidad.temperatura;
                calidad.sala = c_calidad.sala;
                calidad.proteina = c_calidad.proteina;
                calidad.lactosa = c_calidad.lactosa;
                calidad.crioscopia = c_calidad.crioscopia;
                calidad.solidos_no_grasos = c_calidad.solidos_no_grasos;
                calidad.caseina = c_calidad.caseina;
                calidad.urea = c_calidad.urea;
                calidad.grasa = c_calidad.grasa;
                calidad.solidos_totales = c_calidad.solidos_totales;
                calidad.acidez = c_calidad.acidez;
                calidad.ccs = c_calidad.ccs;
                calidad.antibiotico = c_calidad.antibiotico;
                calidad.betalactamicos = c_calidad.betalactamicos;
                calidad.sulfasimas = c_calidad.sulfasimas;
                calidad.tetraciclina = c_calidad.tetraciclina;
                calidad.alcohol_75 = c_calidad.alcohol_75;
                calidad.aflatoxinas = c_calidad.aflatoxinas;
                calidad.activo = true;
                calidad.id_usuario_registra = id_usuario;
                calidad.fecha_registro = DateTime.Now;
                db.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool AsignarUltimaCalidad(int id_envio_leche)
        {
            try
            {
                var envio_leche = db.C_envios_leche_d_calidad.Where(x => x.C_envios_leche_d_fichas.C_envios_leche_g.id_envio_leche_g == id_envio_leche).ToList();
                var ultima_calidad = db.C_envios_leche_calidad.OrderByDescending(x => x.fecha_registro).FirstOrDefault();
                foreach (var registro_reciente in envio_leche)
                {
                    registro_reciente.muestra_calidad = ultima_calidad.muestra_calidad;
                    registro_reciente.sala = ultima_calidad.sala;
                    registro_reciente.proteina = ultima_calidad.proteina;
                    registro_reciente.lactosa = ultima_calidad.lactosa;
                    registro_reciente.crioscopia = ultima_calidad.crioscopia;
                    registro_reciente.solidos_no_grasos = ultima_calidad.solidos_no_grasos;
                    registro_reciente.caseina = ultima_calidad.caseina;
                    registro_reciente.urea = ultima_calidad.urea;
                    registro_reciente.grasa = ultima_calidad.grasa;
                    registro_reciente.solidos_totales = ultima_calidad.solidos_totales;
                    registro_reciente.acidez = ultima_calidad.acidez;
                    registro_reciente.ccs = ultima_calidad.ccs;
                    //antibiotico
                    if (ultima_calidad.antibiotico == true)
                    {
                        registro_reciente.antibiotico = 1;
                    }
                    else
                    {
                        registro_reciente.antibiotico = 0;
                    }
                    //betalactamicos
                    if (ultima_calidad.betalactamicos == true)
                    {
                        registro_reciente.betalactamicos = 1;
                    }
                    else
                    {
                        registro_reciente.betalactamicos = 0;
                    }
                    //sulfasimas
                    if (ultima_calidad.sulfasimas == true)
                    {
                        registro_reciente.sulfasimas = 1;
                    }
                    else
                    {
                        registro_reciente.sulfasimas = 0;
                    }
                    //tetraciclina
                    if (ultima_calidad.tetraciclina == true)
                    {
                        registro_reciente.tetraciclina = 1;
                    }
                    else
                    {
                        registro_reciente.tetraciclina = 0;
                    }
                    //alcohol_75
                    if (ultima_calidad.alcohol_75 == true)
                    {
                        registro_reciente.alcohol_75 = 1;
                    }
                    else
                    {
                        registro_reciente.alcohol_75 = 0;
                    }
                    //aflatoxinas
                    if (ultima_calidad.aflatoxinas == true)
                    {
                        registro_reciente.aflatoxinas = 1;
                    }
                    else
                    {
                        registro_reciente.aflatoxinas = 0;
                    }
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        public PartialViewResult EnvioLecheFichasTable(int id_establo)
        {
            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && x.id_envio_leche_g == null && x.C_bascula_fichas.id_establo == id_establo).ToList();
            return PartialView("../CATALOGOS/LECHE/_EnvioLecheFichasTable", envio_leche);
        }

        public int PreConfirmacionEnvioLeche(C_envios_leche_g c_header, C_envios_leche_d_fichas c_ficha_1, C_envios_leche_d_fichas c_ficha_2, C_envios_leche_d_calidad c_calidad_1, C_envios_leche_d_calidad c_calidad_2)
        {
            try
            {
                //HEADER ENVIO DE LECHE
                int id_usuario = (int)Session["LoggedId"];
                c_header.id_usuario_registra = id_usuario;
                c_header.fecha_registro = DateTime.Now;
                c_header.activo = true;
                c_header.id_envio_leche_status = 1;
                db.C_envios_leche_g.Add(c_header);
                db.SaveChanges();

                #region FICHA 1
                //ENVIO LECHE FICHA 1
                c_ficha_1.id_envio_leche_g = c_header.id_envio_leche_g;
                c_ficha_1.activo = true;
                db.C_envios_leche_d_fichas.Add(c_ficha_1);
                db.SaveChanges();

                //HEADER CALIDAD LECHE FICHA 1
                c_calidad_1.id_envio_leche_d_ficha = c_ficha_1.id_envio_leche_d;
                c_calidad_1.activo = true;
                db.C_envios_leche_d_calidad.Add(c_calidad_1);
                db.SaveChanges();
                #endregion

                #region FICHA 2
                if (c_ficha_2.id_ficha_bascula != null)
                {
                    //ENVIO LECHE FICHA 2
                    c_ficha_2.id_envio_leche_g = c_header.id_envio_leche_g;
                    c_ficha_2.activo = true;
                    db.C_envios_leche_d_fichas.Add(c_ficha_2);
                    db.SaveChanges();

                    //HEADER CALIDAD LECHE FICHA 2
                    c_calidad_2.id_envio_leche_d_ficha = c_ficha_2.id_envio_leche_d;
                    c_calidad_2.activo = true;
                    db.C_envios_leche_d_calidad.Add(c_calidad_2);
                    db.SaveChanges();
                }
                #endregion
                return 1;
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();
                return 0;
            }
        }

        public int ConfirmarCalidadLeche(int id_envio_leche, C_envios_leche_d_calidad c_calidad_1, C_envios_leche_d_calidad c_calidad_2)
        {
            try
            {
                var calidad1 = db.C_envios_leche_d_calidad.Where(x => x.id_envio_leche_d_ficha == c_calidad_1.id_envio_leche_d_ficha).FirstOrDefault();
                if (calidad1 != null)
                {
                    #region FICHA 1
                    calidad1.muestra_calidad = c_calidad_1.muestra_calidad;
                    calidad1.temperatura = c_calidad_1.temperatura;
                    calidad1.sala = c_calidad_1.sala;
                    calidad1.proteina = c_calidad_1.proteina;
                    calidad1.lactosa = c_calidad_1.lactosa;
                    calidad1.crioscopia = c_calidad_1.crioscopia;
                    calidad1.solidos_no_grasos = c_calidad_1.solidos_no_grasos;
                    calidad1.caseina = c_calidad_1.caseina;
                    calidad1.urea = c_calidad_1.urea;
                    calidad1.grasa = c_calidad_1.grasa;
                    calidad1.solidos_totales = c_calidad_1.solidos_totales;
                    calidad1.acidez = c_calidad_1.acidez;
                    calidad1.ccs = c_calidad_1.ccs;
                    calidad1.antibiotico = c_calidad_1.antibiotico;
                    calidad1.betalactamicos = c_calidad_1.betalactamicos;
                    calidad1.sulfasimas = c_calidad_1.sulfasimas;
                    calidad1.tetraciclina = c_calidad_1.tetraciclina;
                    calidad1.alcohol_75 = c_calidad_1.alcohol_75;
                    calidad1.aflatoxinas = c_calidad_1.aflatoxinas;
                    #endregion

                    #region FICHA 2
                    if (c_calidad_2.id_envio_leche_d_ficha != 0)
                    {
                        var calidad2 = db.C_envios_leche_d_calidad.Where(x => x.id_envio_leche_d_ficha == c_calidad_2.id_envio_leche_d_ficha).FirstOrDefault();
                        if (calidad2 != null)
                        {
                            calidad2.muestra_calidad = c_calidad_2.muestra_calidad;
                            calidad2.temperatura = c_calidad_2.temperatura;
                            calidad2.sala = c_calidad_2.sala;
                            calidad2.proteina = c_calidad_2.proteina;
                            calidad2.lactosa = c_calidad_2.lactosa;
                            calidad2.crioscopia = c_calidad_2.crioscopia;
                            calidad2.solidos_no_grasos = c_calidad_2.solidos_no_grasos;
                            calidad2.caseina = c_calidad_2.caseina;
                            calidad2.urea = c_calidad_2.urea;
                            calidad2.grasa = c_calidad_2.grasa;
                            calidad2.solidos_totales = c_calidad_2.solidos_totales;
                            calidad2.acidez = c_calidad_2.acidez;
                            calidad2.ccs = c_calidad_2.ccs;
                            calidad2.antibiotico = c_calidad_2.antibiotico;
                            calidad2.betalactamicos = c_calidad_2.betalactamicos;
                            calidad2.sulfasimas = c_calidad_2.sulfasimas;
                            calidad2.tetraciclina = c_calidad_2.tetraciclina;
                            calidad2.alcohol_75 = c_calidad_2.alcohol_75;
                            calidad2.aflatoxinas = c_calidad_2.aflatoxinas;
                        }
                    }
                    #endregion

                    var envio_leche = db.C_envios_leche_g.Find(id_envio_leche);
                    if (envio_leche.id_envio_leche_status == 1)
                    {
                        envio_leche.id_envio_leche_status = 2;
                        db.SaveChanges();
                    }

                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public string AsignarInformacionFichaBascula(int ficha_bascula)
        {
            var info_ficha = from ficha in db.C_bascula_fichas
                             where ficha.id_bascula_ficha == ficha_bascula
                             select new
                             {
                                 //ficha.id_bascula_proveedor,
                                 ficha.folio,
                                 ficha.observaciones,
                                 ficha.peso_t,
                                 ficha.fecha_segunda_pesada,
                                 ficha.id_linea_transportista,
                                 ficha.placas,
                                 ficha.chofer
                             };
            return Newtonsoft.Json.JsonConvert.SerializeObject(info_ficha);
        }

        public PartialViewResult ConsultarEnviosDeLecheTable(DateTime fecha_inicio, DateTime fecha_fin, int[] establo, string folio, int[] cliente, int[] destino)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_final = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);

            if (folio != "")
            {
                var envios_leche = db.C_envios_leche_g.Where(x => x.folio == folio && x.activo == true).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/_EnviosDeLecheTable", envios_leche);
            }
            if (establo.Contains(0))
            {
                establo = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => (int)x.id_establo).ToArray();
            }
            if (cliente.Contains(0))
            {
                cliente = db.C_envios_leche_clientes.Where(x => x.activo == true).Select(x => (int)x.id_envio_leche_cliente_ms).ToArray();
            }
            if (destino.Contains(0))
            {
                destino = db.C_envios_leche_destinos.Where(x => x.activo == true).Select(x => (int)x.id_destino_envio_leche).ToArray();
            }
            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && x.C_envios_leche_g.fecha_registro > fecha_inicio &&
            x.C_envios_leche_g.fecha_registro < fecha_final && establo.Contains((int)x.C_envios_leche_g.id_establo_envio) &&
            destino.Contains((int)x.C_envios_leche_g.id_destino_envio_leche) && cliente.Contains((int)x.C_envios_leche_g.id_envio_leche_cliente_ms)).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/_EnviosDeLecheTable", envio_leche);
        }

        public string ObtenerInformacionEnvioLeche(int id_envio_leche)
        {
            var envio_leche = from leche in db.C_envios_leche_g
                              join ficha in db.C_envios_leche_d_fichas on leche.id_envio_leche_g equals ficha.id_envio_leche_g
                              join calidad in db.C_envios_leche_d_calidad on ficha.id_envio_leche_d equals calidad.id_envio_leche_d_ficha
                              join bascula in db.C_bascula_fichas on ficha.id_ficha_bascula equals bascula.id_bascula_ficha
                              where leche.id_envio_leche_g == id_envio_leche
                              select new
                              {
                                  leche.id_tipo_tanque_pipa,
                                  leche.id_establo_envio,
                                  leche.fecha_envio,
                                  leche.id_envio_leche_cliente_ms,
                                  leche.id_destino_envio_leche,
                                  leche.id_producto_envio,
                                  leche.folio,
                                  leche.remision_cliente,
                                  leche.operador,
                                  leche.id_linea_transportista,
                                  leche.placas,
                                  leche.kilos_totales,
                                  leche.litros_totales,
                                  leche.densidad_total,
                                  ficha.id_ficha_bascula,
                                  ficha.folio_ficha,
                                  ficha.tanque,
                                  ficha.kilos_ficha,
                                  ficha.litros_ficha,
                                  bascula.fecha_segunda_pesada,
                                  ficha.sello,
                                  calidad.muestra_calidad,
                                  calidad.temperatura,
                                  calidad.sala,
                                  calidad.proteina,
                                  calidad.lactosa,
                                  calidad.crioscopia,
                                  calidad.solidos_no_grasos,
                                  calidad.caseina,
                                  calidad.urea,
                                  calidad.grasa,
                                  calidad.solidos_totales,
                                  calidad.acidez,
                                  calidad.ccs,
                                  calidad.antibiotico,
                                  calidad.betalactamicos,
                                  calidad.sulfasimas,
                                  calidad.tetraciclina,
                                  calidad.alcohol_75,
                                  calidad.aflatoxinas
                              };

            var resultados = envio_leche.ToList();
            var datosFormateados = resultados.Select(leche => new
            {
                leche.id_tipo_tanque_pipa,
                leche.id_establo_envio,
                leche.fecha_envio,
                leche.id_envio_leche_cliente_ms,
                leche.id_destino_envio_leche,
                leche.id_producto_envio,
                leche.folio,
                leche.remision_cliente,
                leche.operador,
                leche.id_linea_transportista,
                leche.placas,
                kilos_totales = string.Format("{0:N3}", leche.kilos_totales),
                litros_totales = string.Format("{0:N3}", leche.litros_totales),
                leche.densidad_total,
                leche.id_ficha_bascula,
                leche.folio_ficha,
                leche.tanque,
                kilos_ficha = string.Format("{0:N3}", leche.kilos_ficha),
                litros_ficha = string.Format("{0:N3}", leche.litros_ficha),
                fecha_segunda_pesada = leche.fecha_segunda_pesada?.ToString("HH:mm:ss"),
                leche.sello,
                leche.muestra_calidad,
                leche.temperatura,
                leche.sala,
                leche.proteina,
                leche.lactosa,
                leche.crioscopia,
                leche.solidos_no_grasos,
                leche.caseina,
                leche.urea,
                leche.grasa,
                leche.solidos_totales,
                leche.acidez,
                leche.ccs,
                leche.antibiotico,
                leche.betalactamicos,
                leche.sulfasimas,
                leche.tetraciclina,
                leche.alcohol_75,
                leche.aflatoxinas
            });

            return Newtonsoft.Json.JsonConvert.SerializeObject(datosFormateados);
        }

        #region DETERMINADOR DE SEMANAS DEL ANO

        public JsonResult ObtenerSemanasFechaMensual(int month, int year)
        {

            List<string> semana = new List<string>();
            List<string> inicioSemanaFecha = new List<string>();
            List<string> terminoSemanaFecha = new List<string>();

            if (month == 0)
            {
                DateTime firstDayOfYear = new DateTime(year, 1, 1);
                DateTime lastDayOfYear = new DateTime(year, 12, 31);
                DateTime currentWeekStart = firstDayOfYear;

                while (currentWeekStart.DayOfWeek != DayOfWeek.Monday)
                {
                    currentWeekStart = currentWeekStart.AddDays(-1);
                }

                for (int i = 0; i < 52; i++)
                {
                    if (currentWeekStart > lastDayOfYear) break;

                    semana.Add((i + 1).ToString());
                    inicioSemanaFecha.Add(currentWeekStart.ToString("dd/MM/yyyy"));
                    terminoSemanaFecha.Add(currentWeekStart.AddDays(6).ToString("dd/MM/yyyy"));
                    currentWeekStart = currentWeekStart.AddDays(7);
                }
            }
            else
            {
                bool semanaIncompleta = false;
                System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
                CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
                DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

                List<(int Semana, string FechaInicio)> semanas = new List<(int, string)>();

                DateTime firstDayOfMonth = new DateTime(year, month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
                int diasAnual = 1;
                //Se valida que la primera semana no sea 53 o 54, si es asi, se ajusta para que sea como semana inicial 1.
                if (firstWeekOfMonth == 53 || firstWeekOfMonth == 54) { firstWeekOfMonth = 1; diasAnual = 2; }
                else if (firstDayOfMonth.Month == 1 && firstWeekOfMonth == 52) { firstWeekOfMonth = 1; diasAnual = 2; }
                int numberOfWeeksInMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek) - firstWeekOfMonth + diasAnual;

                DateTime firstDayOfFirstWeek = firstDayOfMonth;

                // Ajustar el primer día de la semana al lunes anterior si es necesario
                if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Monday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Tuesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Wednesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Thursday)
                {
                    while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                    {
                        firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
                    }
                }
                //si la semana inicio como viernes, sabado o domingo (3 dias), se considera como una semana incompleta
                else if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Friday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Saturday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Sunday)
                {
                    semanaIncompleta = true;
                }

                // Ajustar si la ultima semana comienza con lunes, martes o miércoles
                if (lastDayOfMonth.DayOfWeek == DayOfWeek.Monday || lastDayOfMonth.DayOfWeek == DayOfWeek.Tuesday || lastDayOfMonth.DayOfWeek == DayOfWeek.Wednesday)
                {
                    numberOfWeeksInMonth -= 1;
                }


                //VALIDAR SI LA SEMANA INICIO CON 3 DIAS O MENOS
                if (semanaIncompleta)
                {
                    for (int i = 0; i < numberOfWeeksInMonth - 1; i++)
                    {
                        DateTime inicioSemana = firstDayOfFirstWeek.AddDays(i * 7);
                        //DETERMINAR DIA INICIAL DE LA SEMANA LUNES
                        while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                        {
                            inicioSemana = firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(1);
                        }
                        //SI EL AÑO INICIA COMO LA SEMANA 52,53,54, LA SEMANA REAL DEBE SER 1
                        if (firstWeekOfMonth == 1) { firstWeekOfMonth = 0; }

                        if ((firstWeekOfMonth + 1 + i) > 52) { }
                        else
                        {
                            semana.Add((firstWeekOfMonth + 1 + i).ToString());
                            inicioSemanaFecha.Add(inicioSemana.ToString("dd/MM/yyyy"));
                            terminoSemanaFecha.Add(inicioSemana.AddDays(6).ToString("dd/MM/yyyy"));
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfWeeksInMonth; i++)
                    {
                        if ((firstWeekOfMonth + i) > 52) { }
                        else
                        {
                            semana.Add((firstWeekOfMonth + i).ToString());
                            inicioSemanaFecha.Add(firstDayOfFirstWeek.AddDays(i * 7).ToString("dd/MM/yyyy"));
                            terminoSemanaFecha.Add(firstDayOfFirstWeek.AddDays(i * 7).AddDays(6).ToString("dd/MM/yyyy"));
                        }
                    }
                }
            }
            //llenamos el json
            var mesSemanas = new
            {
                PrimerDiaSemana = inicioSemanaFecha,
                NumeroSemanas = semana.Count,
                SemanasNumero = semana,
                UltimoDiaSemana = terminoSemanaFecha
            };
            return Json(mesSemanas);
        }








        //public JsonResult ObtenerSemanasFechaMensual(int month, int year)
        //{
        //    System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
        //    CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
        //    DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

        //    HashSet<int> semanasUnicas = new HashSet<int>(); // Evita contar semanas duplicadas
        //    List<string> inicioSemanaFecha = new List<string>();
        //    List<string> terminoSemanaFecha = new List<string>();
        //    List<string> semana = new List<string>();

        //    DateTime firstDayOfMonth = new DateTime(year, month, 1);
        //    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        //    DateTime currentDay = firstDayOfMonth;
        //    while (currentDay <= lastDayOfMonth)
        //    {
        //        int weekNumber = calendar.GetWeekOfYear(currentDay, weekRule, firstDayOfWeek);

        //        // Solo agregar si la semana no está ya registrada
        //        if (!semanasUnicas.Contains(weekNumber))
        //        {
        //            semanasUnicas.Add(weekNumber);
        //            semana.Add(weekNumber.ToString());

        //            // Encontrar el lunes de la semana actual
        //            DateTime inicioSemana = currentDay;
        //            while (inicioSemana.DayOfWeek != DayOfWeek.Monday)
        //            {
        //                inicioSemana = inicioSemana.AddDays(-1);
        //            }

        //            // Calcular fin de semana (domingo)
        //            DateTime finSemana = inicioSemana.AddDays(6);

        //            inicioSemanaFecha.Add(inicioSemana.ToString("dd/MM/yyyy"));
        //            terminoSemanaFecha.Add(finSemana.ToString("dd/MM/yyyy"));
        //        }

        //        currentDay = currentDay.AddDays(1);
        //    }

        //    var mesSemanas = new
        //    {
        //        PrimerDiaSemana = inicioSemanaFecha,
        //        NumeroSemanas = semanasUnicas.Count,
        //        SemanasNumero = semana,
        //        UltimoDiaSemana = terminoSemanaFecha
        //    };

        //    return Json(mesSemanas);
        //}
















        public JsonResult ObtenerSemanaEdicion(int year, int semana)
        {
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
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

            // Crear el objeto de respuesta
            var semanaActual = new
            {
                NoSemana = semana,
                PrimerDiaSemana = startOfWeek.ToShortDateString(),
                UltimoDiaSemana = endOfWeek.ToShortDateString()
            };

            return Json(semanaActual);
        }

        public JsonResult ObtenerSemanasEstimacionPorFechas(DateTime Fecha1, DateTime Fecha2)
        {
            if (Fecha1 > Fecha2)
            {
                DateTime temp = Fecha1;
                Fecha1 = Fecha2;
                Fecha2 = temp;
            }

            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            int weekStart = calendar.GetWeekOfYear(Fecha1, weekRule, firstDayOfWeek);
            int weekEnd = calendar.GetWeekOfYear(Fecha2, weekRule, firstDayOfWeek);

            List<int> semanas = new List<int>();

            if (Fecha1.Year != Fecha2.Year)
            {
                int lastWeekOfYear = calendar.GetWeekOfYear(new DateTime(Fecha1.Year, 12, 31), weekRule, firstDayOfWeek);
                for (int i = weekStart; i <= lastWeekOfYear; i++)
                {
                    semanas.Add(i);
                }
                for (int i = 1; i <= weekEnd; i++)
                {
                    semanas.Add(i);
                }
            }
            else
            {
                for (int i = weekStart; i <= weekEnd; i++)
                {
                    semanas.Add(i);
                }
            }
            return Json(semanas.ToArray());
        }

        public JsonResult ObtenerSemanasEstimacion(int month, int year)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            DateTime lastDayOfYear = new DateTime(year, 12, 31);

            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            // Lista para almacenar el inicio y fin de cada semana
            List<string> weeksNames = new List<string>();
            List<string> DiasInicioSemana = new List<string>();
            List<string> DiasFinSemana = new List<string>();

            if (month == 0) // Si el mes es 0, calcular semanas del año completo
            {
                int firstWeekOfYear = calendar.GetWeekOfYear(firstDayOfYear, weekRule, firstDayOfWeek);
                DateTime startOfWeek = firstDayOfYear;

                // Ajusta la primera semana para que comience en lunes
                while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    startOfWeek = startOfWeek.AddDays(-1);
                }

                int weekNumber = firstWeekOfYear;
                DateTime currentWeekStart = startOfWeek;

                while (currentWeekStart <= lastDayOfYear)
                {
                    DateTime currentWeekEnd = currentWeekStart.AddDays(6);

                    weeksNames.Add($"{weekNumber}");
                    DiasInicioSemana.Add(currentWeekStart.ToShortDateString());
                    DiasFinSemana.Add(currentWeekEnd > lastDayOfYear ? lastDayOfYear.ToShortDateString() : currentWeekEnd.ToShortDateString());

                    currentWeekStart = currentWeekStart.AddDays(7);
                    weekNumber++;
                }
            }
            else // Si el mes no es 0, calcular semanas de ese mes específico
            {
                DateTime firstDayOfMonth = new DateTime(year, month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
                int numberOfWeeksInMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek) - firstWeekOfMonth + 1;

                DateTime firstDayOfFirstWeek = firstDayOfMonth;
                while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
                }

                int dias_semana = 0;
                for (int i = 0; i < numberOfWeeksInMonth; i++)
                {
                    weeksNames.Add($"{firstWeekOfMonth + i}");
                    DiasInicioSemana.Add(firstDayOfFirstWeek.AddDays(dias_semana).ToShortDateString());
                    DiasFinSemana.Add(firstDayOfFirstWeek.AddDays(dias_semana + 6).ToShortDateString());
                    dias_semana += 7;
                }
            }

            var mesSemanas = new
            {
                PrimerDiaSemana = DiasInicioSemana,
                NumeroSemanas = weeksNames.Count,
                SemanasNumero = weeksNames,
                UltimoDiaSemana = DiasFinSemana
            };

            return Json(mesSemanas);
        }

        public JsonResult ObtenerSemanaActual()
        {
            DateTime today = DateTime.Today;
            DateTime firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);

            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            int currentWeekNumber = calendar.GetWeekOfYear(today, weekRule, firstDayOfWeek);
            DateTime startOfWeek = today;

            // Encuentra el lunes de la semana actual
            while (startOfWeek.DayOfWeek != DayOfWeek.Monday)
            {
                startOfWeek = startOfWeek.AddDays(-1);
            }

            DateTime endOfWeek = startOfWeek.AddDays(6);

            var semanaActual = new
            {
                NumeroSemana = currentWeekNumber,
                PrimerDiaSemana = startOfWeek.ToShortDateString(),
                UltimoDiaSemana = endOfWeek.ToShortDateString()
            };

            return Json(semanaActual);
        }

        public JsonResult ObtenerFechaSemanas(int semana, int year)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);

            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            DateTime firstDayOfWeekDate = firstDayOfYear.AddDays((semana - 1) * 7);
            while (firstDayOfWeekDate.DayOfWeek != firstDayOfWeek)
            {
                firstDayOfWeekDate = firstDayOfWeekDate.AddDays(-1);
            }

            return Json(firstDayOfWeekDate.ToShortDateString());
        }

        public JsonResult ObtenerFechaSemanaActual()
        {
            DateTime today = DateTime.Now;
            int year = today.Year;
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;
            int currentWeek = calendar.GetWeekOfYear(today, weekRule, firstDayOfWeek);
            DateTime firstDayOfWeekDate = today;
            while (firstDayOfWeekDate.DayOfWeek != firstDayOfWeek)
            {
                firstDayOfWeekDate = firstDayOfWeekDate.AddDays(-1);
            }
            var resultado = new
            {
                PrimerDiaSemana = firstDayOfWeekDate.ToShortDateString(),
                SemanaActual = currentWeek
            };
            return Json(resultado, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region ESTIMACION DIARIA

        public PartialViewResult ConsultarDiasEstimacionProduccionTable(DateTime dia_inicial)
        {
            int id_usuario = (int)Session["LoggedId"];
            ViewBag.FechaProduccion = dia_inicial;
            var dias = db.C_presupuestos_dias.Where(x => x.activo == true).ToList();

            return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_EstimacionDiariaTable", dias);
        }

        public PartialViewResult ConsultarDiasEstimacionProduccionInfoTable(string dia_inicial, int id_programacion_diaria_g)
        {
            int id_usuario = (int)Session["LoggedId"];
            ViewBag.FechaProduccion = dia_inicial;
            var dias = db.C_presupuestos_dias.Where(x => x.activo == true).ToList();

            var estimacion = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == id_programacion_diaria_g).ToList();
            ViewBag.EstimacionEstabloDias = estimacion;

            return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_EstimacionDiariaInfoTable", dias);
        }

        public PartialViewResult ConsultarDiasEstimacionProduccion(string dia_inicial)
        {
            ViewBag.FechaProduccion = dia_inicial;
            var dias = db.C_presupuestos_dias.Where(x => x.activo == true).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_EstimacionProduccion", dias);
        }
        public bool ConfirmacionEstimacionDiaria(C_envios_leche_programacion_diaria_g programacion_diaria, int[] id_dias, DateTime[] fecha, decimal[] produccion, decimal[] sobrante, decimal[] faltante)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                programacion_diaria.fecha_registro = DateTime.Now;
                programacion_diaria.activo = true;
                programacion_diaria.id_usuario_registra = id_usuario;
                db.C_envios_leche_programacion_diaria_g.Add(programacion_diaria);
                db.SaveChanges();
                for (int i = 0; i < id_dias.Length; i++)
                {
                    C_envios_leche_programacion_diaria_d detalle = new C_envios_leche_programacion_diaria_d();
                    detalle.id_programacion_diaria_g = programacion_diaria.id_programacion_diaria_g;
                    detalle.id_dia_presupuesto = id_dias[i];
                    detalle.fecha_dia = fecha[i];
                    detalle.produccion = produccion[i];
                    detalle.sobrante = sobrante[i];
                    detalle.faltante = faltante[i];
                    detalle.activo = true;
                    db.C_envios_leche_programacion_diaria_d.Add(detalle);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ConfirmacionEdicionEstimacionDiaria(int id_programacion_diaria_g, decimal[] produccion, decimal[] sobrante, decimal[] faltante)
        {
            try
            {
                int contador = 0;
                var estimacion_d_general = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == id_programacion_diaria_g).ToList();
                foreach (var item in estimacion_d_general)
                {
                    var estimacion_d = db.C_envios_leche_programacion_diaria_d.Find(item.id_programacion_diaria_d);
                    estimacion_d.produccion = produccion[contador];
                    estimacion_d.sobrante = sobrante[contador];
                    estimacion_d.faltante = faltante[contador];
                    contador++;
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidarEstimacionDiariaExistente(int establo, int anio, int semana)
        {
            try
            {
                int existe_programacio = db.C_envios_leche_programacion_diaria_g.Where(x => x.activo == true && x.id_anio_presupuesto == anio && x.no_semana == semana && x.id_establo == establo).Count(); ;
                if (existe_programacio == 0) { return false; }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ConsultaEstimacionDiariaTable(int[] establo, int[] semana, int[] mes, int[] ano)
        {
            if (establo.Contains(0))
            {
                int id_usuario = (int)Session["LoggedId"];
                establo = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).Select(x => (int)x.id_establo).ToArray();
            }

            //AÑO MES SEMANA
            if (ano.Contains(0) && mes.Contains(0) && semana.Contains(0))
            {
                var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && establo.Contains((int)x.C_envios_leche_programacion_diaria_g.id_establo)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_ConsultaEstimacionDiariaTable", estimacion_diaria);
            }
            //AÑO MES
            else if (ano.Contains(0) && mes.Contains(0))
            {
                var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && establo.Contains((int)x.C_envios_leche_programacion_diaria_g.id_establo) &&
                semana.Contains((int)x.C_envios_leche_programacion_diaria_g.no_semana)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_ConsultaEstimacionDiariaTable", estimacion_diaria);
            }
            //MES SEMANA
            else if (mes.Contains(0) && semana.Contains(0))
            {
                semana = ObtenerSemanasEstimacionArray(0, ano[0]);
                var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && establo.Contains((int)x.C_envios_leche_programacion_diaria_g.id_establo) &&
                ano.Contains((int)x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto) && semana.Contains((int)x.C_envios_leche_programacion_diaria_g.no_semana)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_ConsultaEstimacionDiariaTable", estimacion_diaria);
            }
            //SEMANA
            else if (semana.Contains(0))
            {
                semana = ObtenerSemanasEstimacionArray(mes[0], ano[0]);
                var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && establo.Contains((int)x.C_envios_leche_programacion_diaria_g.id_establo) &&
                ano.Contains((int)x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto) && semana.Contains((int)x.C_envios_leche_programacion_diaria_g.no_semana)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_ConsultaEstimacionDiariaTable", estimacion_diaria);
            }
            else
            {
                var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && establo.Contains((int)x.C_envios_leche_programacion_diaria_g.id_establo) &&
                ano.Contains((int)x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto) && semana.Contains((int)x.C_envios_leche_programacion_diaria_g.no_semana)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/EstimacionDiaria/_ConsultaEstimacionDiariaTable", estimacion_diaria);
            }

        }

        public int[] ObtenerSemanasEstimacionArray(int month, int year)
        {
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            if (year == 0 && month > 0) // Caso: todas las semanas del mes especificado, sin importar el año
            {
                DateTime firstDayOfMonth = new DateTime(DateTime.Now.Year, month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
                int lastWeekOfMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek);

                int numberOfWeeksInMonth = lastWeekOfMonth - firstWeekOfMonth + 1;
                int[] semanasArray = new int[numberOfWeeksInMonth];

                for (int i = 0; i < numberOfWeeksInMonth; i++)
                {
                    semanasArray[i] = firstWeekOfMonth + i;
                }

                return semanasArray;
            }
            else if (month == 0 && year > 0) // Caso: todas las semanas del año especificado
            {
                DateTime firstDayOfYear = new DateTime(year, 1, 1);
                DateTime lastDayOfYear = new DateTime(year, 12, 31);

                int firstWeekOfYear = calendar.GetWeekOfYear(firstDayOfYear, weekRule, firstDayOfWeek);
                int lastWeekOfYear = calendar.GetWeekOfYear(lastDayOfYear, weekRule, firstDayOfWeek);

                int numberOfWeeksInYear = lastWeekOfYear - firstWeekOfYear + 1;
                int[] semanasArray = new int[numberOfWeeksInYear];

                for (int i = 0; i < numberOfWeeksInYear; i++)
                {
                    semanasArray[i] = firstWeekOfYear + i;
                }

                return semanasArray;
            }
            else if (year > 0 && month > 0) // Caso: semanas de un mes específico en un año específico
            {
                DateTime firstDayOfMonth = new DateTime(year, month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
                int lastWeekOfMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek);

                int numberOfWeeksInMonth = lastWeekOfMonth - firstWeekOfMonth + 1;
                int[] semanasArray = new int[numberOfWeeksInMonth];

                for (int i = 0; i < numberOfWeeksInMonth; i++)
                {
                    semanasArray[i] = firstWeekOfMonth + i;
                }

                return semanasArray;
            }
            else if (year == 0 && month == 0)
            {
                DateTime firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime lastDayOfYear = new DateTime(DateTime.Now.Year, 12, 31);

                int firstWeekOfYear = calendar.GetWeekOfYear(firstDayOfYear, weekRule, firstDayOfWeek);
                int lastWeekOfYear = calendar.GetWeekOfYear(lastDayOfYear, weekRule, firstDayOfWeek);

                int numberOfWeeksInYear = lastWeekOfYear - firstWeekOfYear + 1;
                int[] semanasArray = new int[numberOfWeeksInYear];

                for (int i = 0; i < numberOfWeeksInYear; i++)
                {
                    semanasArray[i] = firstWeekOfYear + i;
                }

                return semanasArray;
            }

            return new int[0]; // Caso: entrada inválida
        }



        #endregion

        #region PROGRAMACION SEMANAL

        public bool ValidarExistenciaProgramaLeche(int semana)
        {
            try
            {
                string ano = DateTime.Now.Year.ToString();
                var programa_leche = db.C_envios_leche_programacion_semanal_g.Where(x => x.activo == true && x.no_semana == semana && x.C_presupuestos_anios.anio == ano).Count();
                if (programa_leche > 0) { return true; }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        //ENVIOS PROGRAMADOS
        public PartialViewResult AgregarDetalleProgramacion(int count_tabla, int id_establo, string nombre_establo, int id_cliente_destino, int id_destino, string no_semana, int id_anio, int id_estimacion)
        {
            var destino_prov = db.C_envios_leche_cliente_destinos.Find(id_cliente_destino);
            if (destino_prov == null)
            {
                destino_prov = db.C_envios_leche_cliente_destinos.Where(x => x.activo == true && x.id_envio_leche_cliente_ms == id_cliente_destino && x.id_destino_envio_leche == id_destino).FirstOrDefault();
                ViewBag.id_cliente_destino_programacion = destino_prov.id_cliente_destino;
            }
            else
            {
                ViewBag.id_cliente_destino_programacion = id_cliente_destino;
            }

            ViewBag.count_tabla = count_tabla;
            ViewBag.nombre_establo_programacion = nombre_establo;
            ViewBag.id_establo_programacion = id_establo;

            ViewBag.nombre_cliente_programacion = destino_prov.C_envios_leche_clientes.nombre_comercial;
            ViewBag.nombre_destino_programacion = destino_prov.C_envios_leche_destinos.nombre_destino;
            ViewBag.id_cliente_programacion = destino_prov.id_envio_leche_cliente_ms;
            ViewBag.id_destino_programacion = destino_prov.id_destino_envio_leche;
            ViewBag.id_estimacion_semanal = id_estimacion;

            var cantidad_dias = db.C_presupuestos_dias.Where(x => x.activo == true).Select(x => x.id_dia_presupuesto).ToArray();

            ViewBag.cantidad_dias_programacion = cantidad_dias;

            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionDetalle");
        }

        //SALDO
        public PartialViewResult AgregarDetalleEstabloProgramacion(int count_tabla, int id_establo_programacion, string nombre_establo, int[] saldo_cantidad_dias, decimal[] saldo_destino_establo, decimal[] saldo_estimacion_establo)
        {
            ViewBag.count_tabla = count_tabla;
            ViewBag.saldo_id_establo = id_establo_programacion;
            ViewBag.saldo_nombre_establo = nombre_establo;
            ViewBag.saldo_cantidad_dias = saldo_cantidad_dias;
            ViewBag.saldo_destino_establo = saldo_destino_establo;
            ViewBag.saldo_establo_estimacion = saldo_estimacion_establo;
            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionClienteDetalle");
        }

        public PartialViewResult AgregarDetalleEstabloInicioProgramacion(int count_tabla, decimal[] saldo_estimacion_establo)
        {
            ViewBag.count_tabla = count_tabla;
            ViewBag.saldo_establo_estimacion = saldo_estimacion_establo;
            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionClienteDetalle");
        }

        //RESUMEN
        public PartialViewResult AgregarDetalleResumenProgramacion(int count_tabla, int id_proveedor, string nombre_proveedor, decimal[] resumen_cliente_programacion)
        {
            ViewBag.count_tabla = count_tabla;
            ViewBag.resumen_id_proveedor = id_proveedor;
            ViewBag.resumen_nombre_proveedor = nombre_proveedor;
            ViewBag.resumen_cliente_programacion = resumen_cliente_programacion;

            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionResumenDetalle");
        }


        public PartialViewResult ConsultaProduccionEstimadaSemanaTable(int semana, int year, int modo)
        {
            var programa_leche = db.C_envios_leche_programacion_semanal_g.Where(x => x.activo == true && x.no_semana == semana && x.id_anio == year).Count();
            var estimacion_diaria = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true && x.C_envios_leche_programacion_diaria_g.no_semana == semana && x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto == year).ToList();
            var dias = db.C_presupuestos_dias.Where(x => x.activo == true).ToList();

            if (modo == 1)
            {
                if (programa_leche > 0)
                {
                    ViewBag.ProduccionEstatus = 1;
                }
                else
                {
                    ViewBag.ProduccionEstatus = 0;
                }
            }
            else
            {
                ViewBag.ProduccionEstatus = 0;
            }


            ViewBag.NombreProduccionEstimada = estimacion_diaria.Select(x => ((DateTime)x.fecha_dia).ToShortDateString()).Distinct().ToList();
            ViewBag.DiasProduccionEstimada = dias.Select(x => x.dia).ToList();
            ViewBag.IdDiasProduccionEstimada = dias.Select(x => x.id_dia_presupuesto).ToList();

            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ConsultaProduccionEstimadaSemanaTable", estimacion_diaria);



        }


        public bool ConfirmacionProgramacionDestino(int id_ano, int no_semana, DateTime fecha_inicio, int[] id_cantidad_dias, string[] informacion_tabla)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                foreach (var fila in informacion_tabla)
                {
                    var valores = fila.Split(',');
                    C_envios_leche_programacion_semanal_g semana = new C_envios_leche_programacion_semanal_g();
                    semana.id_programacion_diaria_g = Convert.ToInt32(valores[4]); //ID Estimacion Semanal
                    semana.no_semana = no_semana; //ID Estimacion Semanal

                    int id_estimacion = Convert.ToInt32(valores[4]);

                    semana.fecha_inicio = fecha_inicio;
                    semana.fecha_fin = fecha_inicio.AddDays(id_cantidad_dias.Length - 1);
                    semana.id_anio = id_ano;
                    semana.id_usuario_registra = id_usuario;
                    semana.fecha_registro = DateTime.Now;
                    semana.activo = true;
                    db.C_envios_leche_programacion_semanal_g.Add(semana);
                    db.SaveChanges();

                    int id_cliente = Convert.ToInt32(valores[1]);
                    int id_destino = Convert.ToInt32(valores[2]);
                    C_envios_leche_programacion_semanal_cliente_d envio_cliente = new C_envios_leche_programacion_semanal_cliente_d();
                    envio_cliente.id_programacion_semanal_cliente_g = semana.id_programacion_semanal_g;
                    envio_cliente.id_cliente_envio_leche = id_cliente;
                    envio_cliente.id_destino_envio_leche = id_destino;
                    envio_cliente.id_establo = Convert.ToInt32(valores[0]);
                    envio_cliente.activo = true;
                    db.C_envios_leche_programacion_semanal_cliente_d.Add(envio_cliente);
                    db.SaveChanges();

                    var contador = 0;

                    List<C_envios_leche_programacion_semanal_cliente_d_dias> dias_envio_cliente = new List<C_envios_leche_programacion_semanal_cliente_d_dias>();
                    for (int i = 5; i < valores.Length; i++)
                    {
                        C_envios_leche_programacion_semanal_cliente_d_dias envio_cliente_dias = new C_envios_leche_programacion_semanal_cliente_d_dias();
                        decimal litros = 0;
                        litros = Convert.ToInt32(valores[i]);

                        envio_cliente_dias.id_programacion_semanal_cliente_g = envio_cliente.id_programacion_semanal_cliente_d;
                        envio_cliente_dias.id_programacion_semanal_g = semana.id_programacion_semanal_g;
                        envio_cliente_dias.id_dia_presupuesto = id_cantidad_dias[contador];
                        envio_cliente_dias.fecha_dia = fecha_inicio.AddDays(contador);
                        envio_cliente_dias.cantidad_litros = litros;
                        envio_cliente_dias.activo = true;
                        dias_envio_cliente.Add(envio_cliente_dias);
                        contador++;
                    }
                    db.C_envios_leche_programacion_semanal_cliente_d_dias.AddRange(dias_envio_cliente);
                    db.SaveChanges();

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ConsultaProgramacionSemanalTable(int[] id_ano, int mes, int[] no_semana)
        {
            List<ProgramacionSemanalLeche> programacion_leche = new List<ProgramacionSemanalLeche>();


            if (id_ano[0] == 0)
            {
                id_ano = db.C_presupuestos_anios.Where(x => x.activo == true).Select(x => x.id_anio_presupuesto).ToArray();
            }
            if (mes == 0 && no_semana.Contains(0) && id_ano.Length == 1)
            {
                no_semana = ObtenerSemanasEstimacionArray(mes, id_ano[0]);
            }
            else if (no_semana.Contains(0))
            {
                no_semana = Enumerable.Range(1, 52).ToArray();

            }

            var programacion = db.C_envios_leche_programacion_semanal_cliente_d_dias
                                .Where(x => x.activo == true && id_ano.Contains((int)x.C_envios_leche_programacion_semanal_g.id_anio) &&
                                no_semana.Contains((int)x.C_envios_leche_programacion_semanal_g.no_semana) && x.C_envios_leche_programacion_semanal_g.activo == true).ToList();

            var programacion_diaria = programacion.Select(x => x.C_envios_leche_programacion_semanal_g.id_programacion_diaria_g).ToArray();

            var establos = db.C_envios_leche_programacion_diaria_g.Where(x => x.activo == true && programacion_diaria.Contains(x.id_programacion_diaria_g)).ToList();
            foreach (var ano in programacion.GroupBy(x => x.C_envios_leche_programacion_semanal_g.id_anio).Distinct())
            {
                foreach (var item in ano.GroupBy(x => x.C_envios_leche_programacion_semanal_g.no_semana).Distinct())
                {
                    int semanas = (int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.no_semana;
                    ProgramacionSemanalLeche leche = new ProgramacionSemanalLeche();

                    leche.FechaInicial.Add(Convert.ToDateTime(item.FirstOrDefault().C_envios_leche_programacion_semanal_g.fecha_inicio));
                    leche.FechaFinal.Add(Convert.ToDateTime(item.FirstOrDefault().C_envios_leche_programacion_semanal_g.fecha_fin));
                    leche.TotalLitros.Add(Convert.ToDecimal(item.Sum(x => x.cantidad_litros)));

                    var id_estimacion = item.FirstOrDefault().C_envios_leche_programacion_semanal_g.id_programacion_diaria_g;
                    var estimacion = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == id_estimacion &&
                    x.C_envios_leche_programacion_diaria_g.no_semana == semanas).ToList();

                    decimal total = Convert.ToDecimal(estimacion.Sum(x => x.produccion)) +
                                    Convert.ToDecimal(estimacion.Sum(x => x.sobrante)) -
                                    Convert.ToDecimal(estimacion.Sum(x => x.faltante));

                    var est_sm = establos.FirstOrDefault();
                    var est_sg = establos.FirstOrDefault();
                    try
                    {
                        est_sm = establos.Where(x => x.id_establo == 3 && x.id_programacion_diaria_g == item.Where(y => y.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.id_establo == 3).FirstOrDefault().C_envios_leche_programacion_semanal_g.id_programacion_diaria_g).FirstOrDefault();
                    }
                    catch (Exception)
                    {
                        est_sm = null;
                    }
                    try
                    {
                        est_sg = establos.Where(x => x.id_establo == 1 && x.id_programacion_diaria_g == item.Where(y => y.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.id_establo == 1).FirstOrDefault().C_envios_leche_programacion_semanal_g.id_programacion_diaria_g).FirstOrDefault();
                    }
                    catch (Exception)
                    {
                        est_sg = null;
                    }


                    if (est_sm != null && est_sg != null)
                    {
                        var total_sm = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == est_sm.id_programacion_diaria_g).ToList();
                        var total_sg = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == est_sg.id_programacion_diaria_g).ToList();
                        leche.Anio.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.id_anio);
                        leche.NoSemana.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.no_semana);
                        leche.ProgramadoSM.Add(Convert.ToDecimal((total_sm.Sum(x => x.produccion) + total_sm.Sum(x => x.sobrante)) - total_sm.Sum(x => x.faltante)));
                        leche.ProgramadoSG.Add(Convert.ToDecimal((total_sg.Sum(x => x.produccion) + total_sg.Sum(x => x.sobrante)) - total_sg.Sum(x => x.faltante)));
                        leche.Mes.Add(mes);
                        programacion_leche.Add(leche);
                    }
                    else if (est_sm != null && est_sg == null)
                    {
                        var total_sm = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == est_sm.id_programacion_diaria_g).ToList();
                        leche.Anio.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.id_anio);
                        leche.NoSemana.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.no_semana);
                        leche.ProgramadoSM.Add(Convert.ToDecimal((total_sm.Sum(x => x.produccion) + total_sm.Sum(x => x.sobrante)) - total_sm.Sum(x => x.faltante)));
                        leche.ProgramadoSG.Add(0);
                        leche.Mes.Add(mes);
                        programacion_leche.Add(leche);
                    }
                    else if (est_sm == null && est_sg != null)
                    {
                        var total_sg = db.C_envios_leche_programacion_diaria_d.Where(x => x.id_programacion_diaria_g == est_sg.id_programacion_diaria_g).ToList();
                        leche.Anio.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.id_anio);
                        leche.NoSemana.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.no_semana);
                        leche.ProgramadoSM.Add(0);
                        leche.ProgramadoSG.Add(Convert.ToDecimal((total_sg.Sum(x => x.produccion) + total_sg.Sum(x => x.sobrante)) - total_sg.Sum(x => x.faltante)));
                        leche.Mes.Add(mes);
                        programacion_leche.Add(leche);
                    }
                    else
                    {
                        leche.Anio.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.id_anio);
                        leche.NoSemana.Add((int)item.FirstOrDefault().C_envios_leche_programacion_semanal_g.no_semana);
                        leche.ProgramadoSG.Add(0);
                        leche.ProgramadoSM.Add(0);
                        leche.Mes.Add(mes);
                        programacion_leche.Add(leche);
                    }
                }
            }



            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ConsultaProgramacionSemanalTable", programacion_leche);
        }

        public PartialViewResult MostrarInformacionProgramaSemanal(int anio, int nosemana)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionMostrar", cliente);
        }


        public (int Mes, int Semana) ObtenerMesDeSemana(int anio, int nosemana)
        {
            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday; // Primer día de la semana

            // Obtener el primer día del año
            DateTime firstDayOfYear = new DateTime(anio, 1, 1);

            // Ajustar para encontrar el primer lunes del año (inicio de la primera semana)
            while (firstDayOfYear.DayOfWeek != firstDayOfWeek)
            {
                firstDayOfYear = firstDayOfYear.AddDays(1);
            }

            // Calcular el inicio de la semana solicitada
            DateTime inicioSemana = firstDayOfYear.AddDays((nosemana - 1) * 7);

            // Determinar el mes correspondiente
            int mes = inicioSemana.Month;

            return (Mes: mes, Semana: nosemana);
        }



        public JsonResult EditarProgramaSemanal(int anio, int nosemana)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x =>
            x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();

            int mes = cliente.GroupBy(x => x.fecha_dia.Value.Month).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            int anio_junto = cliente.GroupBy(x => x.fecha_dia.Value.Year).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            //var mesSemana = ObtenerMesDeSemana(anio, nosemana);
            //int mes = mesSemana.Mes;
            //int semana = mesSemana.Semana;

            var lista = cliente.GroupBy(x => x.id_programacion_semanal_g)
                .Select(grupo => new
                {
                    Ano = anio,
                    AnoCompleto = anio_junto,
                    Mes = mes,
                    Semana = nosemana,
                    IdEstablo = grupo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo,
                    NombreDestino = grupo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial,
                    NombreEstablo = grupo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo,
                    IdClienteEnvioLeche = grupo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche,
                    IdDestinoEnvioLeche = grupo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche,
                    CantidadesLitros = grupo.Select(item => new
                    {
                        item.cantidad_litros
                    }).ToList()
                })
                .ToList();


            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public bool ConfirmacionEdicionProgramacionDestino(int id_ano, int no_semana, DateTime fecha_inicio, int[] id_cantidad_dias, string[] informacion_tabla)
        {
            try
            {
                var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x =>
                x.C_envios_leche_programacion_semanal_g.no_semana == no_semana && x.C_envios_leche_programacion_semanal_g.id_anio == id_ano).ToList();

                var s_clientes_d = cliente.Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray();
                var s_clientes = cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_programacion_semanal_cliente_d).Distinct().ToArray();
                var s_completa = cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.id_programacion_semanal_g).Distinct().ToArray();

                db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => s_clientes_d.Contains(x.id_programacion_semanal_cliente_d_dia)).ToList().ForEach(x => { x.activo = false; });
                db.C_envios_leche_programacion_semanal_cliente_d.Where(x => s_clientes.Contains(x.id_programacion_semanal_cliente_d)).ToList().ForEach(x => { x.activo = false; });
                db.C_envios_leche_programacion_semanal_g.Where(x => s_completa.Contains(x.id_programacion_semanal_g)).ToList().ForEach(x => { x.activo = false; });

                db.SaveChanges();

                int id_usuario = (int)Session["LoggedId"];
                foreach (var fila in informacion_tabla)
                {
                    var valores = fila.Split(',');
                    C_envios_leche_programacion_semanal_g semana = new C_envios_leche_programacion_semanal_g();
                    semana.id_programacion_diaria_g = Convert.ToInt32(valores[4]); //ID Estimacion Semanal
                    semana.no_semana = no_semana; //ID Estimacion Semanal

                    int id_estimacion = Convert.ToInt32(valores[4]);

                    semana.fecha_inicio = fecha_inicio;
                    semana.fecha_fin = fecha_inicio.AddDays(id_cantidad_dias.Length - 1);
                    semana.id_anio = id_ano;
                    semana.id_usuario_registra = id_usuario;
                    semana.fecha_registro = DateTime.Now;
                    semana.activo = true;

                    db.C_envios_leche_programacion_semanal_g.Add(semana);
                    db.SaveChanges();

                    int id_cliente = Convert.ToInt32(valores[1]);
                    int id_destino = Convert.ToInt32(valores[2]);
                    C_envios_leche_programacion_semanal_cliente_d envio_cliente = new C_envios_leche_programacion_semanal_cliente_d();
                    envio_cliente.id_programacion_semanal_cliente_g = semana.id_programacion_semanal_g;
                    envio_cliente.id_cliente_envio_leche = id_cliente;
                    envio_cliente.id_destino_envio_leche = id_destino;
                    envio_cliente.id_establo = Convert.ToInt32(valores[0]);
                    envio_cliente.activo = true;

                    db.C_envios_leche_programacion_semanal_cliente_d.Add(envio_cliente);
                    db.SaveChanges();

                    var contador = 0;
                    List<C_envios_leche_programacion_semanal_cliente_d_dias> dias_envio_cliente = new List<C_envios_leche_programacion_semanal_cliente_d_dias>();
                    for (int i = 5; i < valores.Length; i++)
                    {
                        C_envios_leche_programacion_semanal_cliente_d_dias envio_cliente_dias = new C_envios_leche_programacion_semanal_cliente_d_dias();
                        decimal litros = 0;
                        litros = Convert.ToInt32(valores[i]);

                        envio_cliente_dias.id_programacion_semanal_cliente_g = envio_cliente.id_programacion_semanal_cliente_d;
                        envio_cliente_dias.id_programacion_semanal_g = semana.id_programacion_semanal_g;
                        envio_cliente_dias.id_dia_presupuesto = id_cantidad_dias[contador];
                        envio_cliente_dias.fecha_dia = fecha_inicio.AddDays(contador);
                        envio_cliente_dias.cantidad_litros = litros;
                        envio_cliente_dias.activo = true;
                        dias_envio_cliente.Add(envio_cliente_dias);
                        contador++;
                    }

                    db.C_envios_leche_programacion_semanal_cliente_d_dias.AddRange(dias_envio_cliente);
                    db.SaveChanges();

                }
                return true;
            }
            catch (Exception)
            {
                return false;
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
        public PartialViewResult CompromisosClienteLeche(int anio, int nosemana, int mes)
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
            if (compromiso_semanal.Count() > 0)
            {
                ViewBag.comentarioGeneral = compromiso_semanal.FirstOrDefault().comentario;
            }
            else
            {
                ViewBag.comentarioGeneral = "";
            }

            ViewBag.MesCompromiso = mes;
            ViewBag.AnioCompromiso = anio;
            ViewBag.NoSemanaCompromiso = nosemana;

            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_CompromisosClienteLeche", lista_compromisos);
        }

        public bool ConfirmacionComentariosCompromiso(int idCumplimientoSemanal, string observacion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                C_envios_leche_cumplimiento_semanal_comentario comentario = new C_envios_leche_cumplimiento_semanal_comentario();
                comentario.activo = true;
                comentario.comentario = observacion;
                comentario.id_usuario_corporativo = id_usuario;
                comentario.id_cumplimiento_semanal = idCumplimientoSemanal;
                db.C_envios_leche_cumplimiento_semanal_comentario.Add(comentario);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ComentarioGeneralCompromiso(int id_semanal, string comentario)
        {
            try
            {
                if (id_semanal == 0)
                {
                    return false;
                }

                var compromisoSemanal = db.C_envios_leche_cumplimiento_semanal.Find(id_semanal);
                compromisoSemanal.comentario = comentario;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region CONFIRMACION ENVIO LECHE
        public PartialViewResult ConsultaConfirmacionLecheTable(DateTime fecha_inicio, DateTime fecha_fin, int[] establo, int[] destino, int[] cliente)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_final = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (establo.Contains(0))
            {
                establo = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => (int)x.id_establo).ToArray();
            }
            if (cliente.Contains(0))
            {
                cliente = db.C_envios_leche_clientes.Where(x => x.activo == true).Select(x => (int)x.id_envio_leche_cliente_ms).ToArray();
            }
            if (destino.Contains(0))
            {
                destino = db.C_envios_leche_destinos.Where(x => x.activo == true).Select(x => (int)x.id_destino_envio_leche).ToArray();
            }
            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && x.C_envios_leche_g.fecha_registro > fecha_inicio &&
            x.C_envios_leche_g.fecha_registro < fecha_final && establo.Contains((int)x.C_envios_leche_g.id_establo_envio) &&
            destino.Contains((int)x.C_envios_leche_g.id_destino_envio_leche) && cliente.Contains((int)x.C_envios_leche_g.id_envio_leche_cliente_ms) &&
            x.C_bascula_fichas.recibido == false).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/ConfirmacionLeche/_ConsultaConfirmacionLecheTable", envio_leche);
        }

        public PartialViewResult EnvioConfirmadoPorAgrupar(int id_envio_leche_g)
        {
            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && x.id_envio_leche_g == id_envio_leche_g).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/ConfirmacionLeche/_EnvioConfirmadoPorAgruparTable", envio_leche);
        }


        public bool ConfirmacionEnvioLeche(DateTime recepcion, C_envios_leche_d_fichas c_ficha_1, C_envios_leche_d_fichas c_ficha_2)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var ficha1 = db.C_envios_leche_d_fichas.Find(c_ficha_1.id_envio_leche_d);
                ficha1.litros_confirmacion = c_ficha_1.litros_confirmacion;
                ficha1.folio_confirmacion = c_ficha_1.folio_confirmacion;
                var bascula_ficha_1 = db.C_bascula_fichas.Where(x => x.id_bascula_ficha == ficha1.id_ficha_bascula).FirstOrDefault();
                bascula_ficha_1.recibido = true;
                if (c_ficha_2.id_envio_leche_d != 0)
                {
                    var ficha2 = db.C_envios_leche_d_fichas.Find(c_ficha_2.id_envio_leche_d);
                    ficha2.litros_confirmacion = c_ficha_2.litros_confirmacion;
                    ficha2.folio_confirmacion = c_ficha_2.folio_confirmacion;
                    var bascula_ficha_2 = db.C_bascula_fichas.Where(x => x.id_bascula_ficha == ficha2.id_ficha_bascula).FirstOrDefault();
                    bascula_ficha_2.recibido = true;
                }
                C_envios_leche_confirmacion confirmacion = new C_envios_leche_confirmacion();
                confirmacion.id_envio_leche_g = ficha1.id_envio_leche_g;
                confirmacion.fecha_recepcion = recepcion;
                confirmacion.id_envio_leche_confirmacion_status = 1;
                confirmacion.id_usuario_registra = id_usuario;
                confirmacion.fecha_registro = DateTime.Now;
                confirmacion.activo = true;
                db.C_envios_leche_confirmacion.Add(confirmacion);
                db.SaveChanges();


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region LOTES ENVIO DE LECHE
        public PartialViewResult ConsultaLoteAgrupacionLecheTable(int[] cliente, int[] estatus, DateTime fecha_inicio, DateTime fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_final = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (cliente.Contains(0))
            {
                cliente = db.C_envios_leche_clientes.Where(x => x.activo == true).Select(x => (int)x.id_envio_leche_cliente_ms).ToArray();
            }
            if (estatus.Contains(0))
            {
                estatus = db.C_envios_leche_lote_status.Where(x => x.activo == true).Select(x => (int)x.id_envios_leche_lote_status).ToArray();
            }
            var lote = db.C_envios_leche_lote_d.Where(x => x.activo == true &&
            x.C_envios_leche_lote_g.fecha_registro > fecha_inicio && x.C_envios_leche_lote_g.fecha_registro < fecha_final &&
            estatus.Contains((int)x.C_envios_leche_lote_g.id_envio_leche_lote_status) &&
            cliente.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_cliente_ms)).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/LoteLeche/_ConsultaLoteAgrupacionLecheTable", lote);
        }

        public PartialViewResult ConsultaEnviosLecheAgrupacion(int[] cliente)
        {
            if (cliente.Contains(0))
            {
                var confirmacion = db.C_envios_leche_confirmacion.Where(x => x.activo == true && x.id_envio_leche_confirmacion_status == 1).Select(x => x.id_envio_leche_g).ToArray();
                var ficha_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && confirmacion.Contains(x.id_envio_leche_g)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/LoteLeche/_AgrupacionLeche", ficha_leche);
            }
            else
            {
                var confirmacion = db.C_envios_leche_confirmacion.Where(x => x.activo == true && x.id_envio_leche_confirmacion_status == 1).Select(x => x.id_envio_leche_g).ToArray();
                var ficha_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && confirmacion.Contains(x.id_envio_leche_g) && cliente.Contains((int)x.C_bascula_fichas.id_envio_leche_cliente_ms)).ToList();
                return PartialView("../ESTABLOS/EnviosLeche/LoteLeche/_AgrupacionLeche", ficha_leche);
            }
        }

        public bool ConfirmacionLoteEnvioLeche(int[] leche_g, decimal[] precio_leche, decimal[] precio_flete)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                C_envios_leche_lote_g lote_g = new C_envios_leche_lote_g();
                //lote_g.nombre_lote = nombre_lote;
                lote_g.id_envio_leche_lote_status = 2;
                lote_g.usuario_registra = id_usuario;
                lote_g.fecha_registro = DateTime.Now;
                lote_g.activo = true;
                db.C_envios_leche_lote_g.Add(lote_g);
                db.SaveChanges();

                List<C_envios_leche_lote_d> lote_d = new List<C_envios_leche_lote_d>();
                for (int i = 0; i < leche_g.Length; i++)
                {
                    C_envios_leche_lote_d lote = new C_envios_leche_lote_d();
                    lote.id_envio_leche_lote_g = lote_g.id_envio_leche_lote_g;
                    lote.id_envio_leche_confirmacion = leche_g[i];
                    lote.precio_leche = precio_leche[i];
                    lote.precio_flete = precio_flete[i];
                    lote.activo = true;
                    lote_d.Add(lote);
                }
                db.C_envios_leche_lote_d.AddRange(lote_d);
                db.SaveChanges();

                for (int i = 0; i < leche_g.Length; i++)
                {
                    var confirmacion = db.C_envios_leche_confirmacion.Find(leche_g[i]);
                    confirmacion.id_envio_leche_confirmacion_status = 2;
                    db.SaveChanges();
                    var envio_g = db.C_envios_leche_g.Find(confirmacion.id_envio_leche_g);
                    envio_g.id_envio_leche_status = 3;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception) { return false; }
        }


        public PartialViewResult ConsultaConfirmacionFacturaLote(int id_lote_g)
        {
            var lote_d = db.C_envios_leche_lote_d.Where(x => x.id_envio_leche_lote_g == id_lote_g).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/LoteLeche/_ConsultaLoteFacturaTable", lote_d);
        }

        public async Task<ActionResult> ConsultarFacturaLote(string folio_factura)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var client = new HttpClient();
                var baseUrl = $"http://192.168.128.2:84/api/GET_INFORMACION_FACTURA_LECHE/GET?folio_factura_leche={folio_factura}";

                var response = await client.GetAsync(baseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var responseCode = responseObject["ResponseCode"].ToString();

                    if (responseCode == "Ok")
                    {
                        var dataArray = responseObject["data"].ToString();
                        var factura = JsonConvert.DeserializeObject<List<FacturaLecheVenta_MS>>(dataArray).ToList();
                        return Json(JsonConvert.SerializeObject(factura), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { error = true, message = "No se encontró la factura." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { error = true, message = "El folio ingresado no es válido." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (HttpRequestException ex)
            {
                return Json(new { error = true, message = $"Error en la solicitud HTTP: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = $"Ocurrió un error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> ConfirmacionFacturaLoteMs(int id_lote_g, string folio_factura, int cliente_ms, decimal precio_total, decimal litros_totales)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var client = new HttpClient();
                var baseUrl = $"http://192.168.128.2:84/api/GET_INFORMACION_FACTURA_LECHE/GET?folio_factura_leche={folio_factura}";

                var response = await client.GetAsync(baseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var responseCode = responseObject["ResponseCode"].ToString();

                    if (responseCode == "Ok")
                    {
                        var dataArray = responseObject["data"].ToString();
                        var factura = JsonConvert.DeserializeObject<List<FacturaLecheVenta_MS>>(dataArray).FirstOrDefault();

                        if (factura.CLIENTE_ID == cliente_ms)
                        {
                            if (factura.UNIDADES == litros_totales)
                            {
                                decimal ajuste_total = factura.PRECIO_TOTAL_NETO - 0.5m;
                                if (factura.PRECIO_TOTAL_NETO == precio_total || (precio_total > ajuste_total && precio_total <= factura.PRECIO_TOTAL_NETO))
                                {
                                    try
                                    {
                                        var lote_g = db.C_envios_leche_lote_g.Find(id_lote_g);
                                        lote_g.factura = folio_factura;
                                        lote_g.id_envio_leche_lote_status = 3;
                                        lote_g.docto_microsip = factura.DOCTO_VE_ID.ToString();
                                        lote_g.fecha_factura = Convert.ToDateTime(factura.FECHA);
                                        lote_g.usuario_factura = factura.USUARIO_CREADOR;
                                        db.SaveChanges();
                                        var lote_d = db.C_envios_leche_lote_d.Where(x => x.id_envio_leche_lote_g == id_lote_g).Select(x => x.id_envio_leche_confirmacion).ToArray();
                                        foreach (var item in lote_d)
                                        {
                                            var confirmacion = db.C_envios_leche_confirmacion.Find(item);
                                            confirmacion.id_envio_leche_confirmacion_status = 3;
                                            db.SaveChanges();
                                        }
                                        return Json(new { sucess = true, message = "Se vinculó la factura correctamente." }, JsonRequestBehavior.AllowGet);
                                    }
                                    catch (Exception)
                                    {
                                        return Json(new { error = true, message = "Ocurrio un problema al vincular la factura." }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    return Json(new { error = true, message = "El importe total no coincide, favor de revisar." }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                return Json(new { error = true, message = "Los litros totales no coinciden, favor de revisar." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new { error = true, message = "El folio de la factura no coincide con el cliente, favor de revisar." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { error = true, message = "No se encontró la factura." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { error = true, message = "El folio ingresado no es valido." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error en la solicitud HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurrió un error: {ex.Message}");
            }
        }

        public bool CancelacionLote(int id_lote_g)
        {
            try
            {
                var envio_leche = db.C_envios_leche_lote_g.Find(id_lote_g);
                envio_leche.id_envio_leche_lote_status = 4;
                envio_leche.activo = false;
                db.SaveChanges();
                var lote_d = db.C_envios_leche_lote_d.Where(x => x.id_envio_leche_lote_g == id_lote_g).Select(x => x.id_envio_leche_confirmacion).ToArray();
                foreach (var item in lote_d)
                {
                    var confirmacion = db.C_envios_leche_confirmacion.Find(item);
                    confirmacion.id_envio_leche_confirmacion_status = 1;
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool CancelacionEnviosLecheAgrupacion(int[] leche_g)
        {
            try
            {
                for (int i = 0; i < leche_g.Length; i++)
                {
                    var confirmacion = db.C_envios_leche_confirmacion.Find(leche_g[i]);
                    confirmacion.id_envio_leche_confirmacion_status = 4;
                    confirmacion.activo = false;
                    db.SaveChanges();
                    var env_leche_g = db.C_envios_leche_g.Find(confirmacion.id_envio_leche_g);
                    var env_leche_d = db.C_envios_leche_d_fichas.Where(x => x.activo == true && x.id_envio_leche_g == env_leche_g.id_envio_leche_g).Select(x => x.id_envio_leche_d).ToArray();
                    var calidad = db.C_envios_leche_d_calidad.Where(x => x.activo == true && env_leche_d.Contains((int)x.id_envio_leche_d_ficha)).ToList();
                    foreach (var cal in calidad)
                    {
                        if (cal.muestra_calidad.Trim().Length == 0)
                        {
                            env_leche_g.id_envio_leche_status = 1;
                        }
                        else
                        {
                            env_leche_g.id_envio_leche_status = 2;
                        }
                    }

                    db.SaveChanges();
                    var envio_leche_d = db.C_envios_leche_d_fichas.Where(x => x.id_envio_leche_g == confirmacion.id_envio_leche_g).Select(x => x.id_ficha_bascula).ToArray();
                    foreach (var item in envio_leche_d)
                    {
                        var ficha = db.C_bascula_fichas.Find(item);
                        ficha.recibido = false;
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception) { return false; }
        }

        public PartialViewResult MostrarLoteFacturaLeche(int id_lote_g)
        {
            var lote_d = db.C_envios_leche_lote_d.Where(x => x.activo == true && x.C_envios_leche_lote_g.id_envio_leche_lote_status == 3 && x.C_envios_leche_lote_g.activo == true && x.id_envio_leche_lote_g == id_lote_g).ToList();
            return PartialView("../ESTABLOS/EnviosLeche/LoteLeche/_MostrarLoteFacturaLeche", lote_d);
        }

        #endregion




        public PartialViewResult ProgramaSemanalDetalleDia(DateTime fecha)
        {
            ViewBag.ProgramaFecha = fecha;
            ViewBag.ListaDias = db.C_presupuestos_dias.Where(x => x.activo == true).ToList();

            try
            {
                int[] establos = db.C_bascula_establos_fichas_tipos.Where(x => x.activo == true && x.id_ficha_tipo == 1).Select(x => (int)x.id_establo).Distinct().ToArray();
                string[] nombre_establos = db.C_bascula_establos_fichas_tipos.Where(x => x.activo == true && x.id_ficha_tipo == 1).Select(x => x.C_establos.nombre_establo).Distinct().ToArray();
                ViewBag.ProgramaEstabloFecha = establos;
                ViewBag.ProgramanNombreEstabloFecha = nombre_establos;
            }
            catch
            {
                ViewBag.ProgramaEstabloFecha = 0;
            }


            return PartialView("../ESTABLOS/EnviosLeche/ProgramacionSemanal/_ProgramacionDetalleDias", null);
        }

        public JsonResult ClientesDetalle(int anio, int nosemana)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();

            var cliente_select = new
            {
                id_Cliente = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray(),
                Cliente = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial).ToArray(),
                Fecha = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.fecha_dia.GetValueOrDefault().ToString("dddd", new CultureInfo("es-MX")).ToUpper()).ToArray(),
                Dia = cliente.Where(x => x.cantidad_litros > 0).Select(x => (int)x.fecha_dia.GetValueOrDefault().DayOfWeek == 0 ? 7 : (int)x.fecha_dia.GetValueOrDefault().DayOfWeek).ToArray(),
                Litros = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.cantidad_litros).ToArray(),
                Establo = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas).ToArray(),
                IDEstablo = cliente.Where(x => x.cantidad_litros > 0).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).ToArray()
            };

            return Json(cliente_select);
        }

        public JsonResult ConsultarProgramaSemanalDetalleDia(int anio, int nosemana)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            int[] id_cliente_d = cliente.Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray();
            var detalle_programa = db.C_envios_leche_programacion_semanal_cliente_d_dias_horas.Where(x => id_cliente_d.Contains((int)x.id_programacion_semanal_cliente_d_dia) && x.cantidad_litros > 0 && x.activo == true).ToList();


            var programaDetalle = new
            {
                id_programacion_d = detalle_programa.Select(x => x.id_programacion_diaria_cliente_d).ToArray(),

                idestablo = detalle_programa.Select(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.id_establo).ToArray(),
                establo = detalle_programa.Select(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas).ToArray(),

                id_programacion_dia = detalle_programa.Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray(),
                destino = detalle_programa.Select(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial).ToArray(),

                cantidad_litros = detalle_programa.Select(x => x.cantidad_litros).ToArray(),

                Dia = detalle_programa.Select(x => (int)x.C_envios_leche_programacion_semanal_cliente_d_dias.fecha_dia.GetValueOrDefault().DayOfWeek == 0 ? 7 : (int)x.C_envios_leche_programacion_semanal_cliente_d_dias.fecha_dia.GetValueOrDefault().DayOfWeek).ToArray(),
                hora = detalle_programa.Select(x => x.hora.Value.ToString()).ToArray(),

                id_linea_transp = detalle_programa.Select(x => x.id_bascula_linea_transp).ToArray(),
                linea_transp = detalle_programa.Select(x => x.C_bascula_lineas_transportistas.nombre_linea).ToArray(),

                id_tanque_pipa = detalle_programa.Select(x => x.id_tipo_tanque_pipa).ToArray(),
                tanque_pipa = detalle_programa.Select(x => x.C_envios_leche_tipos_tanques.tipo_tanque).ToArray(),
            };

            return Json(programaDetalle);
        }

        public bool ConfirmacionDetalleProgramaDia(int anio, int nosemana, int[] id_programacion, int[] id_pipa, decimal[] cantidad_litros, string[] hora, int[] id_linea)
        {
            try
            {
                var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
                int[] id_cliente_d = cliente.Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray();
                var detalle_programa = db.C_envios_leche_programacion_semanal_cliente_d_dias_horas.Where(x => id_cliente_d.Contains((int)x.id_programacion_semanal_cliente_d_dia) && x.cantidad_litros > 0).ToList();

                detalle_programa.ForEach(x => x.activo = false);
                db.SaveChanges();

                int id_usuario = (int)Session["LoggedId"];

                if (id_programacion == null)
                {
                    return true;
                }

                var programa_dia = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => id_programacion.Contains((int)x.id_programacion_semanal_cliente_d_dia)).ToList();
                int contador = 0;
                List<C_envios_leche_programacion_semanal_cliente_d_dias_horas> programa_hora_lista = new List<C_envios_leche_programacion_semanal_cliente_d_dias_horas>();
                foreach (var item in id_programacion)
                {
                    DateTime fecha = programa_dia.Where(x => x.id_programacion_semanal_cliente_d_dia == item).Select(x => (DateTime)x.fecha_dia).FirstOrDefault();
                    string hora_registro = hora[contador];
                    DateTime fechaHora = DateTime.ParseExact(fecha.ToString("yyyy-MM-dd") + " " + hora_registro, "yyyy-MM-dd HH:mm", null);
                    TimeSpan hora_programa = fechaHora.TimeOfDay;

                    C_envios_leche_programacion_semanal_cliente_d_dias_horas programa_hora = new C_envios_leche_programacion_semanal_cliente_d_dias_horas();
                    programa_hora.id_programacion_semanal_cliente_d_dia = item;
                    programa_hora.fecha_registro = DateTime.Now;
                    programa_hora.id_usuario_registra = id_usuario;
                    programa_hora.id_tipo_tanque_pipa = id_pipa[contador];
                    programa_hora.cantidad_litros = cantidad_litros[contador];
                    programa_hora.hora = hora_programa;
                    programa_hora.id_bascula_linea_transp = id_linea[contador];
                    programa_hora.activo = true;

                    programa_hora_lista.Add(programa_hora);
                    contador++;
                }
                db.C_envios_leche_programacion_semanal_cliente_d_dias_horas.AddRange(programa_hora_lista);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public ActionResult InfoFacturaLeche()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8091)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../ESTABLOS/EnviosLeche/InfoFacturaLeche/Index");
        }


        public PartialViewResult HistoricoFacturaLeche(string factura)
        {
            try
            {
                var historico = from fich in db.C_envios_leche_d_fichas
                                join bascula in db.C_bascula_fichas on fich.id_ficha_bascula equals bascula.id_bascula_ficha
                                join lechG in db.C_envios_leche_g on fich.id_envio_leche_g equals lechG.id_envio_leche_g
                                join destino in db.C_envios_leche_destinos on lechG.id_destino_envio_leche equals destino.id_destino_envio_leche
                                join cliente in db.C_envios_leche_clientes on lechG.id_envio_leche_cliente_ms equals cliente.id_envio_leche_cliente_ms
                                join establo in db.C_establos on lechG.id_establo_envio equals establo.id_establo
                                join confirma in db.C_envios_leche_confirmacion on lechG.id_envio_leche_g equals confirma.id_envio_leche_g
                                join loteD in db.C_envios_leche_lote_d on confirma.id_envio_leche_confirmacion equals loteD.id_envio_leche_confirmacion
                                join loteG in db.C_envios_leche_lote_g on loteD.id_envio_leche_lote_g equals loteG.id_envio_leche_lote_g
                                join usuario in db.C_usuarios_corporativo on loteG.usuario_registra equals usuario.id_usuario_corporativo
                                where loteG.factura == factura
                                select new
                                {
                                    establo = establo.siglas,
                                    destino = cliente.nombre_comercial + ", " + destino.nombre_destino,
                                    fecha = lechG.fecha_envio,
                                    folio = fich.folio_ficha,
                                    ficha = fich.id_ficha_bascula,
                                    tanque = bascula.observaciones,
                                    litros_envio = fich.litros_ficha,
                                    lote = loteD.id_envio_leche_lote_d,
                                    litros_recibido = fich.litros_confirmacion,
                                    precio_litro = loteD.precio_leche,
                                    precio_flete = loteD.precio_flete,
                                    importe = loteD.precio_leche * fich.litros_confirmacion,
                                    registro = usuario.usuario
                                };
                var Listestablo = historico.Select(x => x.establo).ToList();
                var Listdestino = historico.Select(x => x.destino).ToList();
                var Listfecha = historico.Select(x => (DateTime)x.fecha).ToList();
                var Listfolio = historico.Select(x => x.folio).ToList();
                var Listficha = historico.Select(x => (int)x.ficha).ToList();
                var Listtanque = historico.Select(x => x.tanque).ToList();
                var Listlitros_envio = historico.Select(x => (decimal)x.litros_envio).ToList();
                var Listlote = historico.Select(x => x.lote).ToList();
                var Listlitros_recibido = historico.Select(x => (decimal)x.litros_recibido).ToList();
                var Listprecio_litro = historico.Select(x => (decimal)x.precio_litro).ToList();
                var Listprecio_flete = historico.Select(x => (decimal)x.precio_flete).ToList();
                var Listimporte = historico.Select(x => (decimal)x.importe).ToList();
                var Listregistro = historico.Select(x => x.registro).ToList();

                HistoricoFacturasLeche ListaHistorico = new HistoricoFacturasLeche();
                ListaHistorico.Establo = Listestablo;
                ListaHistorico.Destino = Listdestino;
                ListaHistorico.Fecha = Listfecha;
                ListaHistorico.Folio = Listfolio;
                ListaHistorico.Ficha = Listficha;
                ListaHistorico.Tanque = Listtanque;
                ListaHistorico.Litros_envio = Listlitros_envio;
                ListaHistorico.Lote = Listlote;
                ListaHistorico.Litros_recibido = Listlitros_recibido;
                ListaHistorico.Precio_litro = Listprecio_litro;
                ListaHistorico.Precio_flete = Listprecio_flete;
                ListaHistorico.Importe = Listimporte;
                ListaHistorico.Registro = Listregistro;

                return PartialView("../ESTABLOS/EnviosLeche/InfoFacturaLeche/_HistoricoFacturaLecheTable", ListaHistorico);
            }
            catch
            {
                return PartialView("../ESTABLOS/EnviosLeche/InfoFacturaLeche/_HistoricoFacturaLecheTable", null);
            }
        }

    }
}