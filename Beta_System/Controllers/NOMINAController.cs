using Beta_System.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using FirebirdSql.Data.FirebirdClient;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebGrease.Css.Extensions;
using ZXing;
using Calendar = System.Globalization.Calendar;
using Font = iTextSharp.text.Font;

namespace Beta_System.Controllers
{
    public class NOMINAController : Controller
    {
        CATALOGOSController catalogos = new CATALOGOSController();
        NOTIFICACIONESController notificacion = new NOTIFICACIONESController();
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private ChecadorEntities dbc = new ChecadorEntities();
        private REPORTESController reportes = new REPORTESController();

        private FbConnection conn = new FbConnection();

        public ActionResult CatalogoNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8040)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/Catalogos/CatalogosNomina");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }


        #region MOSTRAR INFORMACION CATALOGOS NOMINA
        public string MostrarInfoPuesto(int id_puesto)
        {
            try
            {
                var nomina = from nom in db.C_nomina_puestos
                             where nom.id_puesto_empleado == id_puesto
                             select new
                             {
                                 nom.id_puesto_empleado,
                                 nom.nombre_puesto,
                                 nom.salario_diario,
                                 nom.salario_diario_max
                             };
                return Newtonsoft.Json.JsonConvert.SerializeObject(nomina);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "";
            }
        }

        public string MostrarInfoDepartamento(int id_departamento)
        {
            try
            {
                var nomina = from nom in db.C_nomina_departamentos
                             where nom.id_departamento_empleado == id_departamento
                             select new
                             {
                                 nom.id_departamento_empleado,
                                 nom.nombre_departamento
                             };
                return Newtonsoft.Json.JsonConvert.SerializeObject(nomina);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "";
            }
        }

        public string MostrarInfoRegimenPatronal(int id_regimen)
        {
            try
            {
                var nomina = from nom in db.C_nomina_regimen_patronal
                             where nom.id_regimen_patronal == id_regimen
                             select new
                             {
                                 nom.id_regimen_patronal,
                                 nom.numero_regimen_partonal,
                                 nom.descripcion,
                                 nom.clase_riesgo_sat,
                                 nom.prima_riesgo,
                                 nom.id_ciudad,
                                 nom.id_tipo_isn
                             };
                return Newtonsoft.Json.JsonConvert.SerializeObject(nomina);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "";
            }
        }

        public string MostrarInfoEmpleado(int id_empleado)
        {
            var empleado = from emp in db.C_nomina_empleados
                           where emp.Empleado_id == id_empleado
                           select new
                           {
                               emp.Empleado_id,
                               emp.Numero,
                               emp.Nombre_completo,
                               emp.Apellido_paterno,
                               emp.Apellido_materno,
                               emp.Nombres,
                               emp.Puesto_no_id,
                               emp.Depto_no_id,
                               emp.Frepag_id,
                               emp.Reg_patronal_id,
                               emp.Es_jefe,
                               emp.Forma_pago,
                               emp.Contrato,
                               emp.Dias_hrs_jsr,
                               emp.Turno,
                               emp.Jornada,
                               emp.Regimen_fiscal,
                               emp.Contrato_sat,
                               emp.Jornada_sat,
                               emp.Es_sindicalizado,
                               emp.Fecha_ingreso,
                               emp.Estatus,
                               emp.Zona_salmin,
                               emp.Tabla_antig_id,
                               emp.Tipo_salario,
                               emp.Pctje_integ,
                               emp.Salario_diario,
                               emp.Salario_hora,
                               emp.Salario_integ,
                               emp.Es_dir_admr_gte_gral,
                               emp.PTU,
                               emp.IMSS,
                               emp.CAS,
                               emp.Deshab_imptos,
                               emp.Calc_isr_anual,
                               emp.Calle,
                               emp.Nombre_calle,
                               emp.Num_exterior,
                               emp.Colonia,
                               emp.Poblacion,
                               emp.Referencia,
                               emp.Ciudad_id,
                               emp.Codigo_postal,
                               emp.Telefono1,
                               emp.Email,
                               emp.Sexo,
                               emp.Fecha_nacimiento,
                               emp.Ciudad_nacimiento_id,
                               emp.Estado_civil,
                               emp.Num_hijos,
                               emp.Nombre_padre,
                               emp.Nombre_madre,
                               emp.RFC,
                               emp.CURP,
                               emp.Reg_imss,
                               emp.Grupo_pago_elect_id,
                               emp.Tipo_ctaban_pago_elect,
                               emp.Num_ctaban_pago_elect,
                               emp.Salario_diario_establo,
                               emp.Salario_hora_establo,
                               emp.Path_constancia_fiscal,
                               formacion = emp.C_rh_formacion_academica_g.id_formacion_escolaridad,
                               formacion_g = emp.id_formacion_academica_g
                           };
            return Newtonsoft.Json.JsonConvert.SerializeObject(empleado);
        }
        #endregion

        public string MostrarChecadorEmpleado(int id_empleado)
        {
            try
            {
                var checador = db.C_nomina_empleados_checador.Where(x => x.Empleado_id == id_empleado).FirstOrDefault();
                var numero = dbc.hr_employee.Where(x => x.id == checador.id_empleado_checador).Select(x => x.emp_pin).FirstOrDefault();
                string datos = checador.id_empleado_checador + "-" + checador.C_nomina_empleados.Nombres + " " + checador.C_nomina_empleados.Apellido_paterno + "-" + numero;
                return datos;
            }
            catch (Exception)
            {
                return "0-No se encontro al empleado";
            }
        }

        #region EMPLEADO - CHECADOR
        //dbc - Data Base Check

        public PartialViewResult EmpleadosSinChecadorTable(int modo, int area)
        {
            ViewBag.modo = modo;
            if (area == 0)
            {
                var empleados_checador = db.C_nomina_empleados_checador.Select(a => a.Empleado_id).ToArray();
                var empleadosNomina = db.C_nomina_empleados.Where(x => !empleados_checador.Contains(x.Empleado_id) && x.Estatus == "A").ToList();
                return PartialView("../CATALOGOS/NOMINA/_EmpleadoNominaTable", empleadosNomina);
            }
            else
            {
                var empleados_checador = db.C_nomina_empleados_checador.Select(a => a.Empleado_id).ToArray();
                var empleadosNomina = db.C_nomina_empleados.Where(x => !empleados_checador.Contains(x.Empleado_id) && x.Depto_no_id == area && x.Estatus == "A").ToList();
                return PartialView("../CATALOGOS/NOMINA/_EmpleadoNominaTable", empleadosNomina);
            }
        }

        public PartialViewResult EmpleadoChecadorNominaTable(int area)
        {
            List<EmpleadoNChecador> List_empleadoNC = new List<EmpleadoNChecador>();

            string query;
            if (area == 0)
            {
                query = @"SELECT
                    t1.Empleado_id AS EmpleadoIdNomina,
                    t1.Nombres AS NombreEmpleadoNomina,
                    t1.Apellido_paterno AS ApellidoEmpleadoNomina,
                    t3.id_empleado_nomina_checador AS IdEmpleadoNChecador,
                    t2.emp_pin AS IdEmpleadoChecador,
                    t2.emp_firstname AS NombreCheca,
                    t2.emp_lastname AS ApellidoCheca
                  FROM Beta_Corp.dbo.C_nomina_empleados t1
                  INNER JOIN C_nomina_empleados_checador t3 ON t1.Empleado_id = t3.Empleado_id
                  INNER JOIN checador.dbo.hr_employee t2 ON t3.id_empleado_checador = t2.id";
            }
            else
            {
                query = @"SELECT
                    t1.Empleado_id AS EmpleadoIdNomina,
                    t1.Nombres AS NombreEmpleadoNomina,
                    t1.Apellido_paterno AS ApellidoEmpleadoNomina,
                    t3.id_empleado_nomina_checador AS IdEmpleadoNChecador,
                    t2.emp_pin AS IdEmpleadoChecador,
                    t2.emp_firstname AS NombreCheca,
                    t2.emp_lastname AS ApellidoCheca
                  FROM Beta_Corp.dbo.C_nomina_empleados t1
                  INNER JOIN C_nomina_empleados_checador t3 ON t1.Empleado_id = t3.Empleado_id
                  INNER JOIN checador.dbo.hr_employee t2 ON t3.id_empleado_checador = t2.id
                  INNER JOIN Beta_Corp.dbo.C_nomina_empleados t4 ON t1.Empleado_id = t4.Empleado_id
                  WHERE t4.Depto_no_id = @area";
            }

            using (SqlConnection connection = new SqlConnection("Server=192.168.128.2; Initial Catalog=BETA_Corp; User ID=sa; Password=12345;"))
            {
                SqlCommand command = new SqlCommand(query, connection);
                if (area != 0)
                {
                    command.Parameters.AddWithValue("@area", area);
                }

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    EmpleadoNChecador empleado = new EmpleadoNChecador
                    {
                        EmpleadoIdNomina = Convert.ToInt32(reader["EmpleadoIdNomina"]),
                        NombreEmpleadoNomina = reader["NombreEmpleadoNomina"].ToString(),
                        ApellidoEmpleadoNomina = reader["ApellidoEmpleadoNomina"].ToString(),
                        IdEmpleadoNChecador = Convert.ToInt32(reader["IdEmpleadoNChecador"]),
                        IdEmpleadoChecador = Convert.ToInt32(reader["IdEmpleadoChecador"]),
                        NombreCheca = reader["NombreCheca"].ToString(),
                        ApellidoCheca = reader["ApellidoCheca"].ToString()
                    };

                    List_empleadoNC.Add(empleado);
                }
            }

            return PartialView("../CATALOGOS/NOMINA/_EmpleadoChecadorNominaTable", List_empleadoNC);
        }

        public bool EliminarEmpleadoLigado(int idEmpleadoNChecador)
        {
            try
            {
                C_nomina_empleados_checador empleado_checador = db.C_nomina_empleados_checador.Find(idEmpleadoNChecador);
                db.C_nomina_empleados_checador.Remove(empleado_checador);
                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public bool GuardarEmpleado(int id_RadiosChecador, int radio_nomina)
        {
            try
            {
                int id_usuario = (int)Session["loggedId"];

                C_nomina_empleados_checador new_empleado = new C_nomina_empleados_checador();
                new_empleado.id_usuario_registro_liga = id_usuario;
                new_empleado.fecha_registro = DateTime.Now;
                new_empleado.id_empleado_checador = id_RadiosChecador;
                new_empleado.Empleado_id = radio_nomina;
                db.C_nomina_empleados_checador.Add(new_empleado);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        #endregion

        #region VALIDACIONES CATALOGOS NOMINA
        public PartialViewResult ModalEmpleado()
        {
            return PartialView("../NOMINA/Catalogos/_EditarEmpleado");
        }
        public bool ValidarNumero(int numero)
        {
            try
            {
                var numero_empleado = db.C_nomina_empleados.Where(x => x.Numero == numero).ToList();
                if (numero_empleado.Count() != 0)
                {
                    return false;
                }
                else { return true; }

            }
            catch
            {
                return false;
            }
        }

        public bool ValidarRFC(string rfc)
        {
            try
            {
                var rfc_empleado = db.C_nomina_empleados.Where(x => x.RFC == rfc).ToList();
                if (rfc_empleado.Count() != 0)
                {
                    return false;
                }
                else { return true; }

            }
            catch
            {
                return false;
            }
        }

        public int ObtenerUltimoNumero()
        {
            var ultimo_numero = db.C_nomina_empleados.OrderByDescending(x => x.Numero).Select(x => x.Numero).FirstOrDefault();
            if ((int)ultimo_numero != null)
            {
                return (int)ultimo_numero + 1;
            }
            else
            {
                return 0;
            }
        }

        public PartialViewResult LupaChecadorEmpleados(int modo)
        {
            ViewBag.modo_checador = modo;
            var empleadosligados = db.C_nomina_empleados_checador.Select(x => x.id_empleado_checador).ToArray();
            var empleadoschecador = dbc.hr_employee.Where(x => !empleadosligados.Contains(x.id)).ToList();
            return PartialView("../CATALOGOS/NOMINA/_LupaChecadorTable", empleadoschecador);
        }

        public ActionResult MovimientosNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8044)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/Movimientos/MovimientosNomina");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarIncidenciasNominaTable()
        {
            DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);

            DateTime hoy = DateTime.Today;
            DateTime Lunes = hoy.AddDays(-(int)hoy.DayOfWeek + (int)firstDayOfWeek).Date;
            DateTime Domingo = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Sunday + 8).Date.AddSeconds(-1);
            var incidencia = db.C_nomina_incidencias.Where(x => x.Fecha >= Lunes && x.Fecha <= Domingo).OrderBy(x => x.id_incidencia_nomina).ToList();
            return PartialView("../CATALOGOS/NOMINA/_IncidenciasTable", incidencia);
        }



        public PartialViewResult LupaEmpleados(int departamento, int modo)
        {
            ViewBag.modo_movimiento = modo;
            if (modo == 1)
            {
                if (departamento == 0)
                {
                    var empleado = db.C_nomina_empleados.OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
                else
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Depto_no_id == departamento).OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
            }
            else
            {
                if (departamento == 0)
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Estatus == "A").OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
                else
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Depto_no_id == departamento && x.Estatus == "A").OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
            }
        }

        public PartialViewResult LupaEmpleadosFiltros(int departamento, string estatus)
        {
            ViewBag.modo_movimiento = 1;
            if (departamento == 0)
            {
                if (estatus == "0")
                {
                    var empleado = db.C_nomina_empleados.OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
                else
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Estatus == estatus).OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
            }
            else
            {
                if (estatus == "0")
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Depto_no_id == departamento).OrderBy(x => x.Nombre_completo).ToList();
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }
                else
                {
                    var empleado = db.C_nomina_empleados.Where(x => x.Depto_no_id == departamento && x.Estatus == estatus).OrderBy(x => x.Nombre_completo);
                    return PartialView("../CATALOGOS/NOMINA/_LupaEmpleadoTable", empleado);
                }

            }
        }

        public decimal PrimerAñoPorcentajeIntegracion()
        {
            return (decimal)db.C_nomina_parametros_porcentajes_integracion.Select(x => x.porcentaje_integracion).FirstOrDefault();
        }

        public decimal DeterminarPorcentajeIntegracion(int id_empleado)
        {
            int años_diferencia = 0;
            DateTime fecha_actual = DateTime.Now;
            var empleado = db.C_nomina_empleados.Where(x => x.Empleado_id == id_empleado).FirstOrDefault();
            var baja = db.C_nomina_incidencias.Where(x => x.Empleado_id == id_empleado && x.id_tipo_incidencia == 1)
                .OrderByDescending(x => x.Fecha).FirstOrDefault();
            if (baja != null)
            {
                var reingreso = db.C_nomina_incidencias.Where(x => x.Empleado_id == id_empleado && x.id_tipo_incidencia == 3)
                    .OrderByDescending(x => x.Fecha).FirstOrDefault();

                if (reingreso != null)
                {
                    DateTime fecha_reingreso = (DateTime)reingreso.Fecha;
                    años_diferencia = fecha_actual.Year - fecha_reingreso.Year;
                    if (fecha_actual < fecha_reingreso.AddYears(años_diferencia))
                    {
                        años_diferencia--;
                    }
                }
            }
            else
            {
                DateTime fecha_ingreso = (DateTime)empleado.Fecha_ingreso;
                años_diferencia = fecha_actual.Year - fecha_ingreso.Year;
                if (fecha_actual < fecha_ingreso.AddYears(años_diferencia))
                {
                    años_diferencia--;
                }
            }
            if (años_diferencia <= 0)
            {
                años_diferencia = 1;
            }
            if (años_diferencia > 35)
            {
                años_diferencia = 34;
            }
            var porcentaje = db.C_nomina_parametros_porcentajes_integracion.Where(x => x.activo == true && años_diferencia <= x.tipo_maximo && años_diferencia >= x.tiempo_minimo).OrderByDescending(x => x.id_nomina_parametro_porcentaje_integracion).FirstOrDefault();
            return (decimal)porcentaje.porcentaje_integracion;
        }

        public async Task<string> RegistrarIncidenciaMS(Incidencias_MS incidencias)
        {
            try
            {
                incidencias.ID_USUARIO_REGISTRA = (int)Session["LoggedId"];
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_REGISTRAR_INCIDENCIA/POST";
                //var baseUrl = "http://localhost:52534/api/SYNC_REGISTRAR_INCIDENCIA/POST";

                var json = JsonConvert.SerializeObject(incidencias);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        //if (incidencias.INCIDENCIA_ID == 1 || incidencias.INCIDENCIA_ID == 3)
                        //{
                        //    string tipos = "";
                        //    if (incidencias.INCIDENCIA_ID == 3) { tipos = "A"; }
                        //    if (incidencias.INCIDENCIA_ID == 1) { tipos = "B"; }
                        //    var empleado = db.C_nomina_empleados.Where(x => x.Empleado_id == incidencias.EMPELADO_ID).FirstOrDefault();
                        //    empleado.Estatus = tipos;
                        //    db.SaveChanges();
                        //}


                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();
                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }

        public string MostrarSalarioTipo(int id_empleado)
        {
            string salario_tipo = "";
            var empleado = db.C_nomina_empleados.Where(x => x.Empleado_id == id_empleado).Select(x => x.Tipo_salario).FirstOrDefault();
            if (empleado == 0)
            {
                salario_tipo = "Fijo";
            }
            if (empleado == 1)
            {
                salario_tipo = "Variable";
            }
            if (empleado == 2)
            {
                salario_tipo = "Mixto";
            }
            return salario_tipo;
        }

        public string MostrarInfoIncidencia(int id_incidencia)
        {
            try
            {
                var nomina = from nom in db.C_nomina_incidencias
                             join emp in db.C_nomina_empleados on nom.Empleado_id equals emp.Empleado_id
                             where nom.id_incidencia_nomina == id_incidencia
                             select new
                             {
                                 nom.id_incidencia_nomina,
                                 nom.Empleado_id,
                                 nom.Nuevo_Reg_patronal_id,
                                 nom.Reg_patronal_id,
                                 nom.Fecha,
                                 nom.Salario_diario,
                                 nom.Salario_hora,
                                 nom.Salario_integ,
                                 nom.Pctje_integ,
                                 nom.id_tipo_incidencia,
                                 nom.observaciones,
                                 nom.id_departamento_actual,
                                 nom.id_puesto_actual,
                                 nom.id_departamento_nuevo,
                                 nom.id_puesto_nuevo,
                                 emp.Nombre_completo,
                                 diario_anterior = emp.Salario_diario,
                                 hora_anterior = emp.Salario_hora,
                                 integ_anterior = emp.Salario_integ,
                                 nom.Lista_negra
                             };
                return Newtonsoft.Json.JsonConvert.SerializeObject(nomina);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "";
            }
        }

        [HttpPost]
        public string ConfirmarConstanciaFiscal()
        {
            DateTime hoy = DateTime.Now;
            string path_pdf = "";
            HttpPostedFileBase file = null;
            HttpFileCollectionBase files = Request.Files;
            var jsonEmpleado = Request.Form["empleado"];
            C_nomina_empleados empleado = JsonConvert.DeserializeObject<C_nomina_empleados>(jsonEmpleado);
            if (files.Count > 0)
            {
                for (int z = 0; z < files.Count; z++)
                {
                    file = files[z];
                    var file_upload = Request.Files[z];
                    if (!file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return "-2";
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    file = files[i];
                    try
                    {
                        var file_upload = Request.Files[i];
                        if (file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var InputFileName = "Const_Fisc_" + empleado.Nombres.Replace(" ", "") + empleado.Apellido_paterno.Replace(" ", "") + '_' + empleado.RFC.Replace("-", "") + ".pdf";
                            string fname = Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER"
                                ? file.FileName.Split(new char[] { '\\' }).Last()
                                : file.FileName;
                            var ServerSavePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\EmpleadosDocs\\ConstanciasFiscales\\{InputFileName}";
                            file.SaveAs(ServerSavePath);
                            path_pdf = "http://192.168.128.2/ConstanciasFiscales/" + InputFileName;
                        }
                        else
                        {
                            file.InputStream.Close();
                            file.InputStream.Dispose();
                            System.IO.File.Delete(path_pdf);
                            return "-3";
                        }
                    }
                    catch (Exception)
                    {
                        file.InputStream.Close();
                        file.InputStream.Dispose();
                        System.IO.File.Delete(path_pdf);
                        return "-3";
                    }
                }
                try
                {
                    file.InputStream.Close();
                    file.InputStream.Dispose();

                    var empleado_path = db.C_nomina_empleados.Find(empleado.Empleado_id);
                    empleado_path.Path_constancia_fiscal = path_pdf;
                    db.SaveChanges();
                    return "0";
                }
                catch (Exception)
                {
                    System.IO.File.Delete(path_pdf);
                    return "-5";
                }
            }
            else
            {
                return "-4";
            }
        }

        #endregion

        #region EMPLEADO MICROSIP - SIIB
        public async Task<string> RegistrarEmpleado(Empleados_MS empleado, int id_empleado_checador)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var nombre_empleado = db.C_usuarios_corporativo.Where(x => x.id_empleado == id_usuario).FirstOrDefault();

                empleado.USUARIO_CREADOR = "SYSDBA";
                empleado.FECHA_HORA_CREACION = DateTime.Now;
                empleado.FECHA_INGRESO = DateTime.Now;

                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_REGISTRAR_EMPLEADO/POST";
                //var baseUrl = "http://localhost:52534/api/SYNC_ACTUALIZAR_EMPLEADO/POST";

                var json = JsonConvert.SerializeObject(empleado);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var empleadoId = (int)responseObject["data"][0]["EMPLEADO_ID"];
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        if (id_empleado_checador != 0)
                        {
                            C_nomina_empleados_checador checador = new C_nomina_empleados_checador();
                            checador.fecha_registro = DateTime.Now;
                            checador.Empleado_id = empleadoId;
                            checador.id_usuario_registro_liga = id_usuario;
                            checador.id_empleado_checador = id_empleado_checador;
                            db.C_nomina_empleados_checador.Add(checador);
                            db.SaveChanges();
                        }
                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();
                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }

        public async Task<string> ActualizarEmpleadoMS(Empleados_MS empleado, int id_empleado_checador)
        {
            try
            {
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_ACTUALIZAR_EMPLEADO/POST";
                var json = JsonConvert.SerializeObject(empleado);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        var checador = db.C_nomina_empleados_checador.Where(x => x.Empleado_id == empleado.EMPLEADO_ID).FirstOrDefault();

                        if (checador != null)
                        {
                            checador.id_empleado_checador = id_empleado_checador;
                            db.SaveChanges();
                        }
                        else
                        {
                            int id_usuario = (int)Session["LoggedId"];
                            C_nomina_empleados_checador checadors = new C_nomina_empleados_checador();
                            checadors.fecha_registro = DateTime.Now;
                            checadors.Empleado_id = empleado.EMPLEADO_ID;
                            checadors.id_usuario_registro_liga = id_usuario;
                            checadors.id_empleado_checador = id_empleado_checador;
                            db.C_nomina_empleados_checador.Add(checadors);
                            db.SaveChanges();
                        }

                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }
        #endregion

        #region DEPARTAMENTO MICROSIP - SIIB
        public async Task<string> RegistrarDepartamentoMS(Departamentos_MS departamento)
        {
            try
            {
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_REGISTRAR_DEPARTAMENTO/POST";
                var json = JsonConvert.SerializeObject(departamento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();
                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }

        public async Task<string> ActualizarDepartamentoMS(Departamentos_MS departamento)
        {
            try
            {
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_ACTUALIZAR_DEPARTAMENTO/POST";
                var json = JsonConvert.SerializeObject(departamento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }
        #endregion

        #region PUESTO MICROSIP - SIIB
        public async Task<string> RegistrarPuestoMS(Puestos_MS puesto)
        {
            try
            {
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_REGISTRAR_PUESTO/POST";
                var json = JsonConvert.SerializeObject(puesto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();
                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }

        public async Task<string> ActualizarPuestoMS(Puestos_MS puesto)
        {
            try
            {
                var client = new HttpClient();
                var baseUrl = "http://192.168.128.2:84/api/SYNC_ACTUALIZAR_PUESTO_SIIB/POST";
                var json = JsonConvert.SerializeObject(puesto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);
                    var message = responseObject["Message"].ToString();

                    if (message.Contains("violation of FOREIGN KEY constraint"))
                    {
                        var errorMessage = "ERROR AL ACTUALIZA EN MICROSIP";
                        return errorMessage;
                    }
                    else
                    {
                        return message;
                    }
                }
            }
            catch
            {
                return "Favor de revisar si esta activa la API";
            }
        }
        #endregion

        #region CAMBIO SALARIO MASIVO
        public PartialViewResult ConsultarEmpleadosCambioSalarioMasivo(int id_puesto, decimal salario_diario)
        {
            List<C_nomina_empleados> nomina_empleados = null;
            if (id_puesto != 0)
            {
                nomina_empleados = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.Puesto_no_id == id_puesto).ToList();
            }
            else if (salario_diario != 0)
            {
                nomina_empleados = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.Salario_diario_establo == salario_diario).ToList();
            }

            return PartialView("../NOMINA/Movimientos/_EmpleadosCambioSalarioTable", nomina_empleados);
        }

        public int CambiarSalariosMasivosEmpleados(int[] id_empleados, decimal salario_diario, decimal salario_hora)
        {
            int id_empleado = 0;
            int count = 0;
            try
            {
                for (int i = 0; i < id_empleados.Length; i++)
                {
                    id_empleado = id_empleados[i];
                    try
                    {
                        var empleado = db.C_nomina_empleados.Find(id_empleado);
                        empleado.Salario_diario_establo = salario_diario;
                        empleado.Salario_hora_establo = salario_hora;
                        db.SaveChanges();
                        count++;
                    }
                    catch (Exception)
                    {

                    }
                }

                if (count != id_empleados.Count()) { return -1; }

                else { return count; }

            }
            catch (Exception)
            {
                return -2;
            }

        }

        #endregion


        #region PRENOMINA EMPLEADOS
        public ActionResult PrenominaBeta()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8045)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/Prenomina/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarDepartamentosPrenominaSelect(int id_prenomina_g)
        {
            int id_usuario = (int)Session["LoggedId"];
            var departamentos_usuario = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_prenomina_g && x.activo == true /*&& x.C_nomina_departamentos.C_usuarios_areas_nomina*/)
                                        .Select(x => x.C_nomina_departamentos).Distinct().OrderBy(x => x.nombre_departamento).ToList();
            return PartialView("../CATALOGOS/NOMINA/_DepartamentoSelect", departamentos_usuario);
        }

        public PartialViewResult ConsultarEmpleadosMasivos(int id_puesto, decimal salario_diario, int id_nomina_g)
        {
            List<C_nomina_beta_d_areas_empleados> nomina_empleados = null;
            nomina_empleados = db.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true).ToList();
            if (id_puesto != 0)
            {
                nomina_empleados = nomina_empleados.Where(x => x.C_nomina_empleados.Puesto_no_id == id_puesto && x.C_nomina_empleados.Estatus == "A").ToList();
            }
            else if (salario_diario != 0)
            {
                nomina_empleados = nomina_empleados.Where(x => x.C_nomina_empleados.Salario_diario_establo == salario_diario && x.C_nomina_empleados.Estatus == "A").ToList();
            }

            return PartialView("../NOMINA/Prenomina/_EmpleadosFiltroMasivoTable", nomina_empleados);
        }

        public PartialViewResult ConsultarNominasGeneradasSelect(int id_anio, int id_establo)
        {
            var nominas = db.C_nomina_beta_g.Where(x => x.id_anio == id_anio && x.activo == true && x.id_nomina_status != 4 && x.id_establo == id_establo).OrderBy(x => x.nombre_nomina).ToList();
            return PartialView("../NOMINA/Prenomina/_NominasGeneradasSelect", nominas);
        }

        public PartialViewResult ConsultarNominasAreasTable(int id_nomina_g, int[] id_areas)
        {
            ViewBag.modo = 1; //EDICION DE NOMINA
            if (id_areas.Contains(0))
            {
                int id_usuario = (int)Session["LoggedId"];
                id_areas = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => (int)x.id_departamento_empleado).ToArray();
            }

            var nominas_areas = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true && id_areas.Contains((int)x.id_departamento) && x.C_nomina_beta_g.id_nomina_status != 4)
                .OrderBy(x => x.C_nomina_departamentos.nombre_departamento).ToList();
            return PartialView("../NOMINA/Prenomina/_NominasAreasTable", nominas_areas);
        }

        public PartialViewResult ConsultarNominasAreaTable(int id_nomina_d_area)
        {
            ViewBag.modo = 1;
            var nominas = db.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_d_area == id_nomina_d_area && x.activo == true).ToList();
            return PartialView("../NOMINA/Prenomina/_PrenominaAreaTable", nominas);
        }


        public PartialViewResult ConsultarConceptosEmpleadoNomina(int id_nomina_d_area_empleado)
        {
            var conceptos = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_d_area_empleado == id_nomina_d_area_empleado && x.activo == true).OrderBy(x => x.C_nomina_conceptos.nombre_concepto).ToList();
            var nomina_empleado = db.C_nomina_beta_d_areas_empleados.Find(id_nomina_d_area_empleado);
            ViewBag.salario_diario = nomina_empleado.salario_diario;
            ViewBag.importe = nomina_empleado.importe_final;
            ViewBag.id_nomina_g = nomina_empleado.C_nomina_beta_d_areas.id_nomina_g;

            return PartialView("../NOMINA/Prenomina/_ConceptosNominaEmpleadoTable", conceptos);
        }

        public PartialViewResult ConsultarChecadorEmpleadoNomina(int id_empleado, int id_nomina_g)
        {
            var info_nomina = db.C_nomina_beta_g.Find(id_nomina_g);
            DateTime[] fechas_nomina = ConsultarFechasSemanaNominaArray((DateTime)info_nomina.fecha_inicio);
            var checador = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == id_empleado && fechas_nomina.Contains((DateTime)x.fecha_accion) && x.activo == true).ToList();
            return PartialView("../NOMINA/ControlFaltasHorasExtra/_ChecadorNominaEmpleado", checador);
        }

        public int AgregarConceptoNominaEmpleado(int id_nomina_d_area_empleado, int id_nomina_g, int id_concepto, decimal valor, string comentarios)
        {
            try
            {
                var info_nomina_empleado = db.C_nomina_beta_d_areas_empleados.Find(id_nomina_d_area_empleado);
                var info_concepto = db.C_nomina_conceptos.Find(id_concepto);
                if (info_concepto.clave_concepto == "est")
                {
                    DateTime[] fechas_nomina = ConsultarFechasSemanaNominaArray((DateTime)db.C_nomina_beta_g.Find(id_nomina_g).fecha_inicio);
                    var valid_estimulo_extra = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == info_nomina_empleado.id_empleado && x.activo == true && fechas_nomina.Contains((DateTime)x.fecha_accion)).FirstOrDefault();
                    if (valid_estimulo_extra != null) { if (valid_estimulo_extra.pagar_extimulo == true) { valor += (decimal)valid_estimulo_extra.estimulo_extra_monto; } }
                }

                //VALIDAR SI YA TIENE ASIGNADO EL CONCEPTO (pendiente)
                var valid_concepto = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_d_area_empleado == id_nomina_d_area_empleado &&
                                                                                        x.id_concepto_nomina == id_concepto && x.id_nomina_g == id_nomina_g && x.activo == true).FirstOrDefault();
                if (valid_concepto != null)
                {
                    valid_concepto.valor = valor;
                    //if (valid_concepto.comentarios == null || valid_concepto.comentarios == "") { valid_concepto.comentarios = comentarios; }
                    valid_concepto.comentarios = comentarios;
                    db.SaveChanges();
                    return 0;
                }
                else
                {
                    C_nomina_beta_d_areas_empleados_conceptos new_concepto = new C_nomina_beta_d_areas_empleados_conceptos();
                    new_concepto.id_nomina_g = id_nomina_g;
                    new_concepto.id_nomina_d_area_empleado = id_nomina_d_area_empleado;
                    new_concepto.id_concepto_nomina = id_concepto;
                    new_concepto.tipo_calculo = "P";
                    new_concepto.valor = valor;
                    new_concepto.activo = true;
                    new_concepto.comentarios = comentarios;
                    db.C_nomina_beta_d_areas_empleados_conceptos.Add(new_concepto);
                    db.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 1;
            }

        }

        public int AgregarConceptoMasivoNominaEmpleados(int[] id_nomina_d_area_empleado, int id_nomina_g, int id_concepto, decimal valor, string comentarios)
        {
            for (int i = 0; i < id_nomina_d_area_empleado.Length; i++)
            {
                try
                {
                    int id_nomina_empleado_area = id_nomina_d_area_empleado[i];
                    //VALIDAR SI YA TIENE ASIGNADO EL CONCEPTO (pendiente)
                    var valid_concepto = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_d_area_empleado == id_nomina_empleado_area &&
                                                                                            x.id_concepto_nomina == id_concepto && x.id_nomina_g == id_nomina_g && x.activo == true).FirstOrDefault();
                    if (valid_concepto != null)
                    {
                        valid_concepto.valor = valor;
                        if (valid_concepto.comentarios == null || valid_concepto.comentarios == "") { valid_concepto.comentarios = comentarios; }
                        db.SaveChanges();
                    }
                    else
                    {
                        C_nomina_beta_d_areas_empleados_conceptos new_concepto = new C_nomina_beta_d_areas_empleados_conceptos();
                        new_concepto.id_nomina_g = id_nomina_g;
                        new_concepto.id_nomina_d_area_empleado = id_nomina_empleado_area;
                        new_concepto.id_concepto_nomina = id_concepto;
                        new_concepto.tipo_calculo = "P";
                        new_concepto.valor = valor;
                        new_concepto.activo = true;
                        new_concepto.comentarios = comentarios;
                        db.C_nomina_beta_d_areas_empleados_conceptos.Add(new_concepto);
                        db.SaveChanges();
                    }
                    RecalcularNominaEmpleado(id_nomina_empleado_area);
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                    return 1;
                }
            }
            return 0;
        }


        public int EliminarConceptoEmpleado(int id_concepto_empleado)
        {
            try
            {
                var concepto = db.C_nomina_beta_d_areas_empleados_conceptos.Find(id_concepto_empleado);
                concepto.activo = false;
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool RecalcularNominaEmpleado(int id_nomina_d_area_empleado)
        {
            try
            {
                var nomina_empleado = db.C_nomina_beta_d_areas_empleados.Find(id_nomina_d_area_empleado);
                string clave_concepto;
                decimal importe = 0;
                decimal salario_diario = (decimal)nomina_empleado.salario_diario;
                int dias_laborados = (int)nomina_empleado.dias_laborados;
                decimal porcentaje_septimo_dia = (dias_laborados * 100) / 6;
                decimal salario_hora = salario_diario / 6;
                var conceptos = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_d_area_empleado == id_nomina_d_area_empleado && x.activo == true).ToList();
                foreach (var concepto in conceptos)
                {
                    clave_concepto = concepto.C_nomina_conceptos.clave_concepto;
                    //----------------------------SUELDO NORMAL
                    if (clave_concepto == "101")
                    {
                        importe += salario_diario * dias_laborados;
                    }
                    //----------------------------SEPTIMO DIA
                    if (clave_concepto == "102")
                    {
                        decimal valor_septimo_dia = (salario_diario * porcentaje_septimo_dia) / 100;
                        importe += valor_septimo_dia;
                    }
                    //----------------------------HORAS EXTRAS
                    if (clave_concepto == "HE")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------DIA FESTIVO
                    if (clave_concepto == "DIA")
                    {
                        importe += salario_diario * (decimal)concepto.valor;
                    }
                    //----------------------------DESCANSO TRABAJADO
                    if (clave_concepto == "des")
                    {
                        importe += salario_diario * (decimal)concepto.valor;
                    }
                    //----------------------------ESTIMULOS
                    if (clave_concepto == "est")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------DESPENSA
                    if (clave_concepto == "1de")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------PRIMA DOMINICAL
                    if (clave_concepto == "1pd")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------COMISION POR VIAJES
                    if (clave_concepto == "comis")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------RETROACTIVO
                    if (clave_concepto == "1re")
                    {
                        importe += (decimal)concepto.valor;
                    }
                    //----------------------------DESCUENTO
                    if (clave_concepto == "203")
                    {
                        importe -= (decimal)concepto.valor;
                    }
                    //----------------------------PRIMA VACACIONAL
                    if (clave_concepto == "1pv")
                    {
                        importe += (decimal)concepto.valor;
                    }
                }
                nomina_empleado.importe_final = importe;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public int ValidarGenerarPrenominaBeta(int id_establo, int[] areas)
        {
            try
            {
                DateTime Hoy = DateTime.Now;
                CultureInfo latinCulture = new CultureInfo("es-ES");

                var anio = db.C_presupuestos_anios.Where(x => x.anio == Hoy.Year.ToString()).FirstOrDefault();
                if (anio == null) { return -1; }  //NO SE ENCONTRÓ UN AÑO PARA REGISTRAR LA NOMINA
                int id_anio = anio.id_anio_presupuesto;


                int semana_actual = 0;
                DateTime Lunes_semana_actual, Domingo_semana_actual;
                try
                {
                    DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                    semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                    Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                    Domingo_semana_actual = Lunes_semana_actual.AddDays(6);
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                    return -2;  //ERROR AL CALCULAR LAS FECHAS DE INICIO Y FIN DE LA NOMINA
                }

                int id_usuario = (int)Session["LoggedId"];
                var departamentos_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => x.id_departamento_empleado).ToArray();
                if (departamentos_usuario.Count() <= 0) { return -4; }

                var prenominas_semana = db.C_nomina_beta_d_areas.Where(x => x.C_nomina_beta_g.no_semana >= semana_actual && x.C_nomina_beta_g.id_anio == id_anio && x.activo == true && x.C_nomina_beta_g.id_nomina_status != 4).ToList();
                if (prenominas_semana.Count() > 0)
                {
                    foreach (var prenomina in prenominas_semana)
                    {
                        if (areas.Contains((int)prenomina.id_departamento))
                        {
                            return -5;
                        }
                    }
                }

                var valid_nomina = db.C_nomina_beta_g.Where(x => /*x.no_semana == semana_actual && x.id_anio == id_anio &&*/ x.activo == true && x.id_nomina_status != 4 && x.id_establo == id_establo).FirstOrDefault();
                if (valid_nomina == null) { return 0; }  //NOMINA NUEVA (SEMANA NUEVA)
                else
                {
                    if (valid_nomina.id_nomina_status != 3) { return -3; }  //NOMINA NO ESTÁ CERRADA
                    else if (valid_nomina.id_nomina_status == 3) { return 0; }
                    else { return -3; }
                }

            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 1;
            }
        }

        public string GenerarPrenominaBeta(int id_establo, int[] areas)
        {
            int id_usuario = (int)Session["LoggedId"];
            int errores_conceptos = 0;

            var departamentos_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => x.id_departamento_empleado).ToArray();
            if (departamentos_usuario.Count() <= 0) { return "-4|"; }

            C_nomina_empleados empleado_error = null;
            int id_nomina_g = 0;
            try
            {

                int count_no_empleados = 0;
                decimal importe_nomina_empleado = 0;

                DateTime Hoy = DateTime.Now;
                CultureInfo latinCulture = new CultureInfo("es-ES");

                var anio = db.C_presupuestos_anios.Where(x => x.anio == Hoy.Year.ToString() && x.activo == true).FirstOrDefault();
                if (anio == null) { return "-1|0"; }  //NO SE ENCONTRÓ UN AÑO PARA REGISTRAR LA NOMINA
                int id_anio = anio.id_anio_presupuesto;


                int semana_actual = 0;
                DateTime Lunes_semana_actual, Domingo_semana_actual;
                DateTime[] fechas_nomina = ConsultarFechasSemanaNominaArray(Hoy);
                try
                {
                    DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                    semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    if (semana_actual > 1) { semana_actual = semana_actual - 1; }

                    Lunes_semana_actual = fechas_nomina.Min();
                    Domingo_semana_actual = fechas_nomina.Max().AddHours(23).AddMinutes(59);
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                    return "-2|0";  //ERROR AL CALCULAR LAS FECHAS DE INICIO Y FIN DE LA NOMINA
                }

                var valid_nomina = db.C_nomina_beta_g.Where(x => x.no_semana == semana_actual && x.id_anio == id_anio && x.activo == true && x.id_nomina_status != 4 && x.id_establo == id_establo).FirstOrDefault();

                if (valid_nomina != null)
                {
                    if (valid_nomina.id_nomina_status == 3) { return "-3|0"; }  //NOMINA CERRADA
                    id_nomina_g = valid_nomina.id_nomina_g;
                }
                else
                {
                    string siglas_establo = db.C_establos.Find(id_establo).siglas;
                    C_nomina_beta_g nomina_g = new C_nomina_beta_g();
                    nomina_g.id_anio = id_anio;
                    nomina_g.no_semana = semana_actual;
                    nomina_g.nombre_nomina = "PRENOMINA SEMANA: #" + semana_actual.ToString() + " " + siglas_establo.ToUpper();
                    nomina_g.fecha_registro = Hoy;
                    nomina_g.fecha_inicio = Lunes_semana_actual;
                    nomina_g.fecha_fin = Domingo_semana_actual;
                    nomina_g.id_nomina_status = 1;      //1: REGISTRADA
                    nomina_g.id_usuario_creador = id_usuario;
                    nomina_g.activo = true;
                    nomina_g.id_establo = id_establo;
                    db.C_nomina_beta_g.Add(nomina_g);
                    db.SaveChanges();
                    id_nomina_g = nomina_g.id_nomina_g;
                }
                if (id_nomina_g > 0)
                {
                    //----------DEPARTAMENTOS
                    var departamentos_activos = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true && x.C_nomina_departamentos.activo == true && x.Frepag_id == 410 &&
                                                                                areas.Contains((int)x.Depto_no_id)).Select(x => x.C_nomina_departamentos).ToList().Distinct();
                    foreach (var area in departamentos_activos)
                    {
                        count_no_empleados = 0;
                        int id_departamento = area.id_departamento_empleado;

                        int id_nomina_departamento = 0;
                        var valid_nomina_area = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.id_departamento == id_departamento && x.activo == true).FirstOrDefault();

                        C_nomina_beta_d_areas nomina_d_area = new C_nomina_beta_d_areas();
                        if (valid_nomina_area != null) { id_nomina_departamento = valid_nomina_area.id_nomina_d_area; }
                        else
                        {
                            nomina_d_area.id_nomina_g = id_nomina_g;
                            nomina_d_area.id_departamento = id_departamento;
                            nomina_d_area.no_empleados = 0;
                            nomina_d_area.activo = true;
                            db.C_nomina_beta_d_areas.Add(nomina_d_area);
                            db.SaveChanges();
                            id_nomina_departamento = nomina_d_area.id_nomina_d_area;
                        }

                        if (id_nomina_departamento > 0)
                        {
                            //--------EMPLEADOS
                            foreach (var empleado in db.C_nomina_empleados.Where(x => x.Depto_no_id == id_departamento && x.Estatus == "A" && x.Frepag_id == 410).ToList())
                            {
                                var valid_CAHE_empleado = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == empleado.Empleado_id && x.activo == true && fechas_nomina.Contains((DateTime)x.fecha_accion)).ToList();
                                if (valid_CAHE_empleado.Count() == 0) {
                                    var valid_liga_empleado = db.C_nomina_empleados_checador.Where(x => x.Empleado_id == empleado.Empleado_id).FirstOrDefault();
                                    if (valid_liga_empleado != null)
                                    {
                                        ProcesarCAHEEmpleado(empleado.Empleado_id, (int)valid_liga_empleado.id_empleado_checador);
                                    }
                                }

                                importe_nomina_empleado = 0;
                                empleado_error = empleado;
                                C_nomina_beta_d_areas_empleados nomina_d_area_empleado = new C_nomina_beta_d_areas_empleados();
                                decimal salario_diario = 0;
                                int id_nomina_d_area_empleado = 0;

                                int count_faltas = ConsultarInasistenciasSemanaEmpleado(empleado.Empleado_id, Hoy);
                                if (count_faltas != 0)
                                {
                                    string hola = "";
                                }

                                int dias_laborados = 6 - count_faltas;
                                if (dias_laborados < 0) { dias_laborados = 0; }
                                var valid_empleado = db.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_d_area == id_nomina_departamento && x.id_empleado == empleado.Empleado_id && x.activo == true).FirstOrDefault();
                                if (valid_empleado != null)
                                {
                                    id_nomina_d_area_empleado = valid_empleado.id_nomina_d_area_empleado;
                                    salario_diario = (decimal)empleado.Salario_diario;
                                }
                                else
                                {
                                    nomina_d_area_empleado.id_nomina_d_area = id_nomina_departamento;
                                    nomina_d_area_empleado.id_empleado = empleado.Empleado_id;
                                    nomina_d_area_empleado.salario_diario = empleado.Salario_diario;
                                    nomina_d_area_empleado.cuenta_bancaria = empleado.Num_ctaban_pago_elect;
                                    nomina_d_area_empleado.id_puesto = empleado.Puesto_no_id;
                                    nomina_d_area_empleado.nss = empleado.Reg_imss;
                                    nomina_d_area_empleado.importe_final = 0;
                                    nomina_d_area_empleado.activo = true;
                                    nomina_d_area_empleado.dias_laborados = dias_laborados;
                                    nomina_d_area_empleado.id_nomina_g = id_nomina_g;
                                    db.C_nomina_beta_d_areas_empleados.Add(nomina_d_area_empleado);
                                    db.SaveChanges();
                                    id_nomina_d_area_empleado = nomina_d_area_empleado.id_nomina_d_area_empleado;
                                    salario_diario = (decimal)empleado.Salario_diario;
                                }

                                if (id_nomina_d_area_empleado > 0 && salario_diario > 0)
                                {
                                    //----------------- CONCEPTOS
                                    var valid_concepto_1 = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g && x.id_nomina_d_area_empleado == id_nomina_d_area_empleado &&
                                                                                                                x.id_concepto_nomina == 104 && x.activo == true).FirstOrDefault();

                                    C_nomina_beta_d_areas_empleados_conceptos nomina_d_concepto_sueldo_normal = new C_nomina_beta_d_areas_empleados_conceptos();
                                    C_nomina_beta_d_areas_empleados_conceptos nomina_d_concepto_septimo_dia = new C_nomina_beta_d_areas_empleados_conceptos();
                                    if (valid_concepto_1 == null)
                                    {
                                        //var incidencias = db.C_nomina_incidencias.Where(x => x.Empleado_id == empleado.Empleado_id && x.id_tipo_incidencia == );
                                        nomina_d_concepto_sueldo_normal.id_nomina_g = id_nomina_g;
                                        nomina_d_concepto_sueldo_normal.id_nomina_d_area_empleado = id_nomina_d_area_empleado;
                                        nomina_d_concepto_sueldo_normal.id_concepto_nomina = 104;  //Sueldo normal
                                        nomina_d_concepto_sueldo_normal.valor = dias_laborados;
                                        nomina_d_concepto_sueldo_normal.tipo_calculo = "P";
                                        nomina_d_concepto_sueldo_normal.activo = true;
                                        db.C_nomina_beta_d_areas_empleados_conceptos.Add(nomina_d_concepto_sueldo_normal);
                                    }
                                    try
                                    {
                                        decimal total_sueldo_normal = 0;
                                        decimal total_septimo_dia = 0;
                                        if (valid_concepto_1 != null) { total_sueldo_normal = salario_diario * (decimal)valid_concepto_1.valor; }
                                        else { total_sueldo_normal = salario_diario * (decimal)nomina_d_concepto_sueldo_normal.valor; }
                                        //total_septimo_dia = total_sueldo_normal / 6;

                                        decimal porcentaje_septimo_dia = (dias_laborados * 100) / 6;
                                        total_septimo_dia = (salario_diario * porcentaje_septimo_dia) / 100;

                                        var valid_concepto_2 = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g && x.id_nomina_d_area_empleado == id_nomina_d_area_empleado &&
                                                                                                                x.id_concepto_nomina == 105 && x.activo == true).FirstOrDefault();
                                        if (valid_concepto_2 == null)
                                        {
                                            nomina_d_concepto_septimo_dia.id_nomina_g = id_nomina_g;
                                            nomina_d_concepto_septimo_dia.id_nomina_d_area_empleado = id_nomina_d_area_empleado;
                                            nomina_d_concepto_septimo_dia.id_concepto_nomina = 105;  //Septimo dia
                                            nomina_d_concepto_septimo_dia.valor = total_septimo_dia;
                                            nomina_d_concepto_septimo_dia.tipo_calculo = "P";
                                            nomina_d_concepto_septimo_dia.activo = true;
                                            db.C_nomina_beta_d_areas_empleados_conceptos.Add(nomina_d_concepto_septimo_dia);
                                        }

                                        //---------PROCESO DONDE ENTRA EL CAHE
                                        C_nomina_beta_d_areas_empleados_conceptos nomina_d_concepto_hra_extra = new C_nomina_beta_d_areas_empleados_conceptos();
                                        C_nomina_beta_d_areas_empleados_conceptos nomina_d_concepto_estimulo_extra = new C_nomina_beta_d_areas_empleados_conceptos();
                                        var valid_HR = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == nomina_d_area_empleado.id_empleado && x.activo == true && fechas_nomina.Contains((DateTime)x.fecha_accion)).FirstOrDefault();
                                        if (valid_HR != null) {
                                            if (valid_HR.pagar_hrs == true)
                                            {
                                                var valid_concepto_HE = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g && x.id_nomina_d_area_empleado == id_nomina_d_area_empleado &&
                                                                                                                x.id_concepto_nomina == 107 && x.activo == true).FirstOrDefault();
                                                if (valid_concepto_HE == null)
                                                {
                                                    nomina_d_concepto_hra_extra.id_nomina_g = id_nomina_g;
                                                    nomina_d_concepto_hra_extra.id_nomina_d_area_empleado = id_nomina_d_area_empleado;
                                                    nomina_d_concepto_hra_extra.id_concepto_nomina = 107;  //Horas extras
                                                    nomina_d_concepto_hra_extra.valor = valid_HR.hrs_extra_monto;
                                                    nomina_d_concepto_hra_extra.tipo_calculo = "P";
                                                    nomina_d_concepto_hra_extra.activo = true;
                                                    db.C_nomina_beta_d_areas_empleados_conceptos.Add(nomina_d_concepto_hra_extra);
                                                    importe_nomina_empleado += (decimal)valid_HR.hrs_extra_monto;
                                                }
                                            }
                                            if (valid_HR.pagar_extimulo == true)
                                            {
                                                var valid_concepto_HE = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g && x.id_nomina_d_area_empleado == id_nomina_d_area_empleado &&
                                                                                                                x.id_concepto_nomina == 49378 && x.activo == true).FirstOrDefault();
                                                if (valid_concepto_HE == null)
                                                {
                                                    nomina_d_concepto_estimulo_extra.id_nomina_g = id_nomina_g;
                                                    nomina_d_concepto_estimulo_extra.id_nomina_d_area_empleado = id_nomina_d_area_empleado;
                                                    nomina_d_concepto_estimulo_extra.id_concepto_nomina = 49378;  //Estimulo extra
                                                    nomina_d_concepto_estimulo_extra.valor = valid_HR.estimulo_extra_monto;
                                                    nomina_d_concepto_estimulo_extra.tipo_calculo = "P";
                                                    nomina_d_concepto_estimulo_extra.activo = true;
                                                    db.C_nomina_beta_d_areas_empleados_conceptos.Add(nomina_d_concepto_estimulo_extra);
                                                    importe_nomina_empleado += (decimal)valid_HR.estimulo_extra_monto;
                                                }
                                            }
                                        }
                                        db.SaveChanges();

                                        importe_nomina_empleado = importe_nomina_empleado + total_sueldo_normal + total_septimo_dia;
                                        if (valid_empleado != null)
                                        {
                                            valid_empleado.importe_final = importe_nomina_empleado;
                                            count_no_empleados++;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            nomina_d_area_empleado.importe_final = importe_nomina_empleado;
                                            count_no_empleados++;
                                            db.SaveChanges();
                                        }
                                        importe_nomina_empleado = 0;
                                    }
                                    catch (Exception ex)
                                    {
                                        importe_nomina_empleado = 0;

                                        string msj = ex.ToString();
                                        errores_conceptos++;
                                        var nomina_proceso = db.C_nomina_beta_g.Find(id_nomina_g);
                                        nomina_proceso.activo = false;
                                        nomina_proceso.id_nomina_status = 4;
                                        db.SaveChanges();

                                        return id_nomina_g.ToString() + "|" + empleado.Numero;
                                    }
                                }

                            }
                        }

                        if (valid_nomina_area != null) { valid_nomina_area.no_empleados = count_no_empleados; }
                        else { nomina_d_area.no_empleados = count_no_empleados; }
                        db.SaveChanges();
                    }
                }

                return id_nomina_g.ToString() + "|0";
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                errores_conceptos++;
                var nomina_proceso = db.C_nomina_beta_g.Find(id_nomina_g);
                nomina_proceso.activo = false;
                nomina_proceso.id_nomina_status = 4;
                db.SaveChanges();

                return "0|" + empleado_error.Numero;

                //return "0";
            }

        }

        public int ConsultarInasistenciasSemanaEmpleado(int id_empleado, DateTime fecha)
        {
            DateTime[] dias_nomina = ConsultarFechasSemanaNominaArray(fecha);

            int count_faltas = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == id_empleado && x.activo == true
                                                    && dias_nomina.Contains((DateTime)x.fecha_accion) && x.id_nomina_checador_concepto == 2).Count();
            return count_faltas;
        }

        public DateTime[] ConsultarFechasInasistenciasSemanaEmpleado(int id_empleado, DateTime fecha_nomina)
        {
            DateTime[] fechas_nomina = ConsultarFechasSemanaNominaArray(fecha_nomina);
            DateTime[] fechas_inasistencia = null;
            try
            {
                fechas_inasistencia = db.C_nomina_checador_prenomina.Where(x => fechas_nomina.Contains((DateTime)x.fecha_accion) && x.id_nomina_checador_concepto == 2 && x.activo == true)
                                        .Select(x => (DateTime)x.fecha_accion).ToArray();
            }
            catch (Exception)
            {
                fechas_inasistencia = null;
            }

            return null;
        }


        public DateTime ObtenerPrimerDiaSemana(int year, int weekNum)
        {
            DayOfWeek startOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);

            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)startOfWeek - (int)jan1.DayOfWeek;

            // Ajuste para manejar el inicio de semana en viernes
            DateTime firstWeekDay = jan1.AddDays(daysOffset);

            // Si el primer día calculado está en el año anterior, ajustamos al próximo viernes
            if (firstWeekDay.Year < year)
            {
                firstWeekDay = firstWeekDay.AddDays(7);
            }

            // Determinamos si la primera semana es la semana 1
            int firstWeek = new GregorianCalendar().GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, startOfWeek);
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            // Devolvemos el primer día de la semana calculado
            return firstWeekDay.AddDays(weekNum * 7);
        }
        public DateTime ObtenerPrimerDiaSemanaLunesDomingo(int year, int weekNum)
        {
            DayOfWeek startOfWeek = DayOfWeek.Monday;

            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)startOfWeek - (int)jan1.DayOfWeek;

            // Ajuste para manejar el inicio de semana en viernes
            DateTime firstWeekDay = jan1.AddDays(daysOffset);

            // Si el primer día calculado está en el año anterior, ajustamos al próximo viernes
            if (firstWeekDay.Year < year)
            {
                firstWeekDay = firstWeekDay.AddDays(7);
            }

            // Determinamos si la primera semana es la semana 1
            int firstWeek = new GregorianCalendar().GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, startOfWeek);
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            // Devolvemos el primer día de la semana calculado
            return firstWeekDay.AddDays(weekNum * 7);
        }

        public string GenerarTXTExcepciones(int id_nomina_g)
        {
            string Header_Line;
            string Detail_Line;
            string filePath = "\\\\192.168.128.2\\inetpub\\NominaFiles\\Excepciones_TXT\\EXCEPCIONES_NOMINA_" + id_nomina_g + ".txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var nomina_empleado = db.C_nomina_beta_d_areas_empleados.Where(x => x.activo == true && x.C_nomina_beta_d_areas.id_nomina_g == id_nomina_g && x.C_nomina_empleados.Frepag_id == 410).GroupBy(x => x.id_empleado).ToList();
                foreach (var nomina_emp in nomina_empleado)
                {
                    try
                    {
                        var empleado = nomina_emp.FirstOrDefault().C_nomina_empleados;
                        decimal saladio_diario = (decimal)nomina_emp.FirstOrDefault().salario_diario;

                        var tipos_conceptos = nomina_emp.FirstOrDefault().C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true).Select(x => x.C_nomina_conceptos.fijo_excepcion).Distinct().ToList();
                        if (tipos_conceptos.Contains(true))
                        {
                            int no_faltas = ConsultarInasistenciasSemanaEmpleado(empleado.Empleado_id, (DateTime)nomina_emp.FirstOrDefault().C_nomina_beta_g.fecha_registro);
                            if (no_faltas == 0) { Header_Line = "|1|" + empleado.Numero + ",6,00000,0,0"; }
                            else
                            {
                                int faltas_a7 = no_faltas * 1000;
                                string no_faltas_total = faltas_a7.ToString().Trim().PadLeft(5, '0');
                                Header_Line = "|1|" + empleado.Numero + ",6," + no_faltas_total + ",0,0";
                            }
                            writer.WriteLine(Header_Line);


                            //---------- IMPORTO LOS CONCEPTOS AL TXT
                            foreach (var conceptos in nomina_emp.FirstOrDefault().C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.C_nomina_conceptos.fijo_excepcion == true && x.activo == true && x.C_nomina_beta_d_areas_empleados.id_empleado == empleado.Empleado_id).ToList())
                            {
                                string importe = "";
                                string clave_concepto = conceptos.C_nomina_conceptos.clave_concepto;
                                decimal valor_salario = (decimal)conceptos.valor * saladio_diario;

                                if (clave_concepto == "102")  //Septimo día
                                { importe = conceptos.valor.ToString(); }

                                else if (clave_concepto == "101")  //Salario normal
                                { importe = valor_salario.ToString(); }

                                else if (clave_concepto == "DIA")  //Dia festivo
                                { importe = valor_salario.ToString(); }

                                else if (clave_concepto == "des")  //Descanso trabajado
                                { importe = valor_salario.ToString(); }

                                else if (clave_concepto == "HE")  //Horas extras (PROCESO DEL CHECADOR)
                                { importe = conceptos.valor.Value.ToString(); }

                                else { importe = conceptos.valor.Value.ToString(); }

                                Detail_Line = "|1.1|" + conceptos.C_nomina_conceptos.clave_concepto + "," + Convert.ToDecimal(importe).ToString("0.00") + ", 000000000000";
                                writer.WriteLine(Detail_Line);
                            }

                            //---------- IMPORTO LAS FALTAS AL TXT
                            if (no_faltas > 0)
                            {
                                var fechas_faltas = ConsultarFechasInasistenciasSemanaEmpleado(empleado.Empleado_id, (DateTime)nomina_emp.FirstOrDefault().C_nomina_beta_g.fecha_registro);
                                if (fechas_faltas != null)
                                {
                                    for (int i = 0; i < fechas_faltas.Length; i++)
                                    {
                                        string fecha_falta_formato = string.Format(fechas_faltas[i].ToShortDateString(), "dd/MM/yyyy");
                                        Detail_Line = "|1.2|" + fecha_falta_formato;
                                        writer.WriteLine(Detail_Line);
                                    }
                                }
                            }

                        }
                        else { string hola = ""; }
                    }
                    catch (Exception)
                    {
                        writer.Close();
                        writer.Dispose();
                        System.IO.File.Delete(filePath);
                        return "-1";
                    }

                }
            }

            //return "https://localhost:44371/NOMINA/DescargarExcepcionesTXT?path=" + filePath.Replace("\\", "/");
            return "https://siib.beta.com.mx/NOMINA/DescargarExcepcionesTXT?path=" + filePath.Replace("\\", "/");
        }

        public ActionResult DescargarExcepcionesTXT(string path)
        {

            // Check if the file exists
            if (System.IO.File.Exists(path))
            {
                // Set the content type and headers
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("Content-Disposition", "attachment;filename=\"" + System.IO.Path.GetFileName(path) + "\"");
                Response.TransmitFile(path);
                Response.End();
            }
            else
            {
                // Handle the case where the file does not exist
                return HttpNotFound("File not found.");
            }

            // Return an empty result to avoid rendering a view
            return new EmptyResult();
        }


        public int FinalizarNóminaBeta(int id_nomina_g)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                var valid_status = db.C_nomina_beta_g.Find(id_nomina_g);
                if (valid_status.id_nomina_status != 1) { return 1; }

                valid_status.id_nomina_status = 3;  //CERRADA
                valid_status.fecha_cierre = DateTime.Now;
                valid_status.id_usuario_cierre = id_usuario;
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }

        }


        public PartialViewResult ConsultarNominasFinalizadasSelect(string no_anio, int no_mes, int[] id_establo)
        {
            int id_usuario = (int)Session["LoggedId"];
            ViewBag.modo = 2; //REVISION DE NOMINA TERMINADA
            DateTime fecha_inicio = new DateTime(Convert.ToInt32(no_anio), no_mes, 01);
            DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            var nominas = db.C_nomina_beta_g.Where(x => x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin && x.activo == true && x.id_nomina_status == 3 && id_establo.Contains((int)x.id_establo) ).OrderBy(x => x.nombre_nomina).ToList();
            return PartialView("../NOMINA/Prenomina/_NominasGeneradasSelect", nominas);
        }

        public PartialViewResult ConsultarNominasFinalizadas(int id_nomina_g)
        {
            ViewBag.modo = 2;
            var nominas_areas = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true).OrderBy(x => x.C_nomina_departamentos.nombre_departamento).ToList();
            return PartialView("../NOMINA/Prenomina/_NominasAreasTable", nominas_areas);
        }


        public PartialViewResult GenerarFormatoNominaConceptos(int id_nomina_g, int[] id_departamento, int modo)
        {
            ViewBag.modo = modo;
            if (id_departamento.Contains(0)) { id_departamento = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true).Select(x => x.C_nomina_departamentos.id_departamento_empleado).ToArray(); }

            var valid = db.C_nomina_beta_g.Find(id_nomina_g);
            if (valid.id_nomina_status != 3) { }  //NO ESTÁ CERRADA

            Session["ConceptosNominaFormato"] = db.C_nomina_conceptos.Where(x => x.activo == true && x.formato_nomina == true).ToList();
            var nomina = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true && id_departamento.Contains((int)x.id_departamento)).ToList();
            return PartialView("../NOMINA/Prenomina/_FormatoNominaConceptos", nomina);

        }


        public int EliminarPrenominaBeta(int id_nomina_g, string motivo)
        {
            try
            {
                var nomina = db.C_nomina_beta_g.Find(id_nomina_g);
                string numero_nomina = nomina.no_semana.ToString();

                nomina.id_nomina_status = 4;  //ELIMINADA
                nomina.activo = false;
                db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g).ToList().ForEach(x => x.activo = false);
                db.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_g == id_nomina_g).ToList().ForEach(x => x.activo = false);
                db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g).ToList().ForEach(x => x.activo = false);
                //db.C_nomina_beta_d_areas_empleados_conceptos.RemoveRange(db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.id_nomina_g == id_nomina_g));
                try
                {
                    string nombre_usuario_logeado = Session["Nombre_usuario"].ToString();
                    string mensaje = "<strong>El usuario: " + nombre_usuario_logeado + " ah eliminado la prenómina de la semana: #" + numero_nomina + "</strong>";
                    mensaje += "<br />";
                    mensaje += "<label>Motivo:" + motivo + "<label>" +
                        "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta.png' width='200'/>";

                    NOTIFICACIONESController notificacion = new NOTIFICACIONESController();
                    foreach (var item in db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == 2002 && x.activo == true).ToList())
                    {
                        notificacion.EnviarCorreoUsuario("PRENOMINA ELIMINADA", item.C_usuarios_corporativo.email, mensaje);
                    }
                }
                catch (Exception)
                {

                }


                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public string ConsultarImporteTotalPrenomina(int id_nomina_g)
        {
            try
            {
                return db.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true).Sum(x => x.importe_final).Value.ToString("#,##0.00");
            }
            catch (Exception)
            {
                return "0";
            }
        }


        public PartialViewResult CheckOutAreasPrenominaGenerada(int id_establo)
        {
            try
            {
                DateTime Hoy = DateTime.Now;
                CultureInfo latinCulture = new CultureInfo("es-ES");

                var anio = db.C_presupuestos_anios.Where(x => x.anio == Hoy.Year.ToString()).FirstOrDefault();
                if (anio == null) { return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", null); }  //NO SE ENCONTRÓ UN AÑO PARA REGISTRAR LA NOMINA
                int id_anio = anio.id_anio_presupuesto;


                int semana_actual = 0;
                DateTime Lunes_semana_actual, Domingo_semana_actual;
                try
                {
                    DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                    semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                    Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                    Domingo_semana_actual = Lunes_semana_actual.AddDays(6);
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                    return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", null);  //ERROR AL CALCULAR LAS FECHAS DE INICIO Y FIN DE LA NOMINA
                }

                int id_usuario = (int)Session["LoggedId"];
                var departamentos_activos = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true && x.C_nomina_departamentos.activo == true && x.Frepag_id == 410)
                                                                    .Select(x => x.C_nomina_departamentos.id_departamento_empleado).Distinct().ToArray();
                Session["AreasUsuarioSesion"] = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true && departamentos_activos.Contains((int)x.id_departamento_empleado)).Distinct().ToList();

                try
                {
                    ViewBag.nombre_establo = db.C_establos.Find(id_establo).nombre_establo;
                    ViewBag.fecha_inicio = Lunes_semana_actual;
                    ViewBag.fecha_fin = Domingo_semana_actual;
                    ViewBag.no_semana = semana_actual;
                }
                catch (Exception)
                {
                    ViewBag.nombre_establo = "ERROR";
                    ViewBag.fecha_inicio = Lunes_semana_actual;
                    ViewBag.fecha_fin = Domingo_semana_actual;
                    ViewBag.no_semana = semana_actual;
                }

                var valid_nomina = db.C_nomina_beta_g.Where(x => x.no_semana == semana_actual && x.id_anio == id_anio && x.activo == true && x.id_nomina_status != 4 && x.id_establo == id_establo).FirstOrDefault();
                if (valid_nomina == null)
                {   //NOMINA NUEVA (SEMANA NUEVA)
                    ViewBag.status = 1;
                    return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", null);
                }
                else
                {
                    if (valid_nomina.id_nomina_status != 3 && valid_nomina.id_nomina_status != 4)
                    {  //NOMINA ABIERTA
                        ViewBag.status = 2;
                        return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", valid_nomina.C_nomina_beta_d_areas);
                    }
                    else
                    {    //ESTÁ CERRADA
                        ViewBag.status = 3;
                        return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", null);
                    }
                }


            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return PartialView("../NOMINA/Prenomina/_CheckOutAreasPrenomina", null);
            }
        }


        public ActionResult DescargarExcelFormatoNominaConceptos(int id_nomina_g, int[] id_departamento, int modo, int formato, int tamano)
        {
            string encabezado = "PRENOMINA BETA SANTA MONICA";
            ViewBag.modo = modo;
            if (id_departamento.Contains(0)) { id_departamento = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true).Select(x => x.C_nomina_departamentos.id_departamento_empleado).ToArray(); }

            var valid = db.C_nomina_beta_g.Find(id_nomina_g);
            if (valid.id_nomina_status != 3) { }  //NO ESTÁ CERRADA

            Session["ConceptosNominaFormato"] = db.C_nomina_conceptos.Where(x => x.activo == true && x.formato_nomina == true).ToList();
            var data = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true && id_departamento.Contains((int)x.id_departamento)).ToList();

            string htmlContent = RenderPartialViewToString(data, "../NOMINA/Prenomina/_FormatoNominaConceptosExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, formato, tamano, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PRENOMINA " + id_nomina_g.ToString() + ".xlsx");
        }

        private string RenderPartialViewToString(object model, string path_partial_view)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, path_partial_view /*"_ExcelImportacionesFacturas"*/);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        private byte[] ConvertHtmlTableToExcel(string htmlTable, int modo, int tamano, string encabezado)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Parse HTML table content
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlTable);

            // Convert HTML table to Excel
            ConvertHtmlTableToWorksheet(worksheet, doc.DocumentNode.SelectSingleNode("//table"), tamano, modo, encabezado);

            // Save the workbook to a memory stream
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        private void ConvertHtmlTableToWorksheet(IXLWorksheet worksheet, HtmlNode tableNode, int lenght_row_header, int modo, string encabezado)
        {
            if (tableNode != null)
            {
                if (modo == 1)
                {
                    // Modo 1: Procesamiento estándar
                    ProcessTableForWorksheet(worksheet, tableNode, lenght_row_header, encabezado);
                }
                else if (modo == 2)
                {
                    // Modo 2: Separar en hojas por cada headerDepartamento
                    IXLWorksheet currentSheet = null;
                    string currentDepartamento = null;
                    int rowNumber = 4;

                    foreach (var rowNode in tableNode.SelectNodes("tr"))
                    {
                        var colNumber = 1;
                        foreach (var cellNode in rowNode.SelectNodes("th|td"))
                        {
                            var decodedText = System.Web.HttpUtility.HtmlDecode(cellNode.InnerHtml);

                            if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerDepartamento"))
                            {
                                // Si es un nuevo departamento, crear una nueva hoja
                                currentDepartamento = decodedText;

                                // Limpiar el nombre de la hoja de caracteres inválidos
                                currentDepartamento = CleanSheetName(currentDepartamento);

                                currentSheet = worksheet.Workbook.AddWorksheet(currentDepartamento);

                                // Insertar el encabezado y el logo en la nueva hoja
                                InsertHeaderAndLogo(currentSheet, lenght_row_header, encabezado);

                                // Ajustar el número de fila para la nueva hoja
                                rowNumber = 4;

                                currentSheet.Range(rowNumber, 1, rowNumber, lenght_row_header).Merge().Value = decodedText;
                                currentSheet.Cell(rowNumber, colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                currentSheet.Cell(rowNumber, colNumber).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                currentSheet.Cell(rowNumber, colNumber).Style.Font.Bold = true;
                                currentSheet.Cell(rowNumber, colNumber).Style.Font.FontColor = XLColor.White;
                                currentSheet.Cell(rowNumber, colNumber).Style.Fill.BackgroundColor = XLColor.Black;
                            }
                            else
                            {
                                if (currentSheet != null)
                                {
                                    if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerConcepto"))
                                    {
                                        currentSheet.Cell(rowNumber, colNumber).Value = decodedText;
                                        currentSheet.Cell(rowNumber, colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        currentSheet.Cell(rowNumber, colNumber).Style.Fill.BackgroundColor = XLColor.FromArgb(38, 109, 173);
                                        currentSheet.Cell(rowNumber, colNumber).Style.Font.Bold = true;
                                        currentSheet.Cell(rowNumber, colNumber).Style.Font.FontColor = XLColor.White;
                                        currentSheet.Cell(rowNumber, colNumber).Style.Font.FontSize = 12;
                                    }
                                    else
                                    {
                                        currentSheet.Cell(rowNumber, colNumber).Value = decodedText;
                                    }
                                }
                            }
                            colNumber++;
                        }
                        rowNumber++;
                    }

                    // Ajuste automático del ancho de las columnas y bordes para todas las hojas creadas en modo 2
                    foreach (var sheet in worksheet.Workbook.Worksheets)
                    {
                        if (sheet.Name != worksheet.Name) // Excluir la hoja original si aún existe
                        {
                            var tableRange = sheet.RangeUsed();
                            if (tableRange != null)
                            {
                                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            }
                            sheet.Columns().AdjustToContents();
                        }
                    }

                    // Eliminar la hoja original si está vacía
                    if (worksheet.CellsUsed().Count() == 0)
                    {
                        worksheet.Workbook.Worksheets.Delete(worksheet.Name);
                    }
                }
            }
        }

        // Método auxiliar para procesar la tabla en modo 1 (procesamiento estándar)
        private void ProcessTableForWorksheet(IXLWorksheet worksheet, HtmlNode tableNode, int lenght_row_header, string encabezado)
        {
            var currentRow = 1;
            var rowNumber = 4;

            InsertHeaderAndLogo(worksheet, lenght_row_header, encabezado);

            foreach (var rowNode in tableNode.SelectNodes("tr"))
            {
                var colNumber = 1;
                foreach (var cellNode in rowNode.SelectNodes("th|td"))
                {
                    var decodedText = System.Web.HttpUtility.HtmlDecode(cellNode.InnerHtml);
                    if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerDepartamento"))
                    {
                        worksheet.Range(rowNumber, 1, rowNumber, lenght_row_header).Merge().Value = decodedText;
                        worksheet.Cell(rowNumber, colNumber).Value = decodedText;
                        worksheet.Cell(rowNumber, colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(rowNumber, colNumber).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(rowNumber, colNumber).Style.Font.Bold = true;
                        worksheet.Cell(rowNumber, colNumber).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(rowNumber, colNumber).Style.Fill.BackgroundColor = XLColor.Black;
                    }
                    else if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerConcepto"))
                    {
                        worksheet.Cell(rowNumber, colNumber).Value = decodedText;
                        worksheet.Cell(rowNumber, colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(rowNumber, colNumber).Style.Fill.BackgroundColor = XLColor.FromArgb(38, 109, 173);
                        worksheet.Cell(rowNumber, colNumber).Style.Font.Bold = true;
                        worksheet.Cell(rowNumber, colNumber).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(rowNumber, colNumber).Style.Font.FontSize = 12;
                    }
                    else
                    {
                        worksheet.Cell(rowNumber, colNumber).Value = decodedText;
                    }
                    colNumber++;
                }
                rowNumber++;
            }

            var tableRange = worksheet.Range(1, 1, rowNumber - 1, lenght_row_header);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Columns().AdjustToContents();
        }

        // Método auxiliar para limpiar el nombre de la hoja de caracteres inválidos
        private string CleanSheetName(string sheetName)
        {
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']', '\'' };
            foreach (var ch in invalidChars)
            {
                sheetName = sheetName.Replace(ch, '_');
            }

            return sheetName.Length > 31 ? sheetName.Substring(0, 31) : sheetName;
        }

        // Método auxiliar para insertar el encabezado y el logo
        private void InsertHeaderAndLogo(IXLWorksheet worksheet, int lenght_row_header, string encabezado)
        {
            var currentRow = 1;
            var headerRange = worksheet.Range(currentRow, 1, currentRow + 2, lenght_row_header);

            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var image = worksheet.AddPicture(stream)
                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                     .Scale(0.2);  // Ajustar el tamaño de la imagen si es necesario
            }

            // Insertar el texto centrado en la cabecera
            headerRange.Merge();
            worksheet.Cell(currentRow, 1).Value = encabezado;
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 24;
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Black;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.White;

            // Ajustar la altura de las filas si es necesario
            worksheet.Row(currentRow).Height = 16;
            worksheet.Row(currentRow + 1).Height = 16;
            worksheet.Row(currentRow + 2).Height = 16;
        }


        #endregion

        #region INASISTENCIA
        public PartialViewResult MostrarDiasInasistencia()
        {
            DateTime Hoy = DateTime.Now;
            CultureInfo latinCulture = new CultureInfo("es-ES");
            int semana_actual = 0;
            DateTime Lunes_actual;
            try
            {
                DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                List<DateTime> Dias_Inasistencia = new List<DateTime>();
                semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                Lunes_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                for (int i = 0; i < 7; i++)
                {
                    Dias_Inasistencia.Add(Lunes_actual.AddDays(i));
                }
                return PartialView("../NOMINA/Inasistencias/_DiasInasistenciaCheck", Dias_Inasistencia);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public PartialViewResult MostrarInasistenciaEmpleadoTable(int id_empleado, int id_nomina_g, int modo)
        {
            try
            {
                ViewBag.modo = modo;
                ViewBag.id_nomina_g = id_nomina_g;
                ViewBag.id_nomina_d_area_empleado = 0;
                List<C_nomina_inasistencias> empleado = null;
                if (id_nomina_g == 0)
                {
                    DateTime Hoy = DateTime.Now;
                    CultureInfo latinCulture = new CultureInfo("es-ES");
                    int semana_actual = 0;
                    DateTime Lunes_semana_actual, Domingo_semana_actual;
                    DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                    semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                    if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                    Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                    Domingo_semana_actual = Lunes_semana_actual.AddDays(6);
                    empleado = db.C_nomina_inasistencias.Where(x => x.activo == true && x.Empleado_id == id_empleado && x.fecha_inasistencia >= Lunes_semana_actual && x.fecha_inasistencia <= Domingo_semana_actual).ToList();
                }
                else
                {
                    var data_nomina = db.C_nomina_beta_g.Find(id_nomina_g);
                    ViewBag.id_nomina_d_area_empleado = data_nomina.C_nomina_beta_d_areas_empleados.Where(x => x.id_nomina_g == id_nomina_g && x.id_empleado == id_empleado).FirstOrDefault().id_nomina_d_area_empleado;
                    empleado = db.C_nomina_inasistencias.Where(x => x.activo == true && x.Empleado_id == id_empleado && x.fecha_inasistencia >= data_nomina.fecha_inicio && x.fecha_inasistencia <= data_nomina.fecha_fin).ToList();
                }
                return PartialView("../NOMINA/Inasistencias/_DiasInasistenciaEmpleadoTable", empleado);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return null;
            }
        }



        /*
        1- YA ESTA CERRADA LA NOMINA
        2- EXITO, SE REGISTRO CORRECTAMENTE LAS INASISTENCIAS
        3- OCURRIO UN PROBLEMA AL BUSCAR AL EMPLEADO
        4- OCURRIO UN ERROR AL CALCULAR LOS DIAS TRABAJADOS
        5- NO SE ENCONTRÓ UNA NÓMINA
         */


        public int ConfirmarInasistencias(string[] dias_inasistencias, int empleado_id, string obs)
        {
            try
            {
                CultureInfo latinCulture = new CultureInfo("es-ES");
                int id_usuario = (int)Session["LoggedId"];
                int semana_actual = 0;
                DateTime fecha_registro = DateTime.Now;
                DateTime Lunes_semana_actual, Domingo_semana_actual;
                DateTime Hoy = DateTime.Now;
                DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                Domingo_semana_actual = Lunes_semana_actual.AddDays(6);
                //var nomina_actual = db.C_nomina_beta_g.Where(x => x.activo == true && x.id_nomina_status != 4 && x.fecha_inicio >= Lunes_semana_actual && x.fecha_fin <= Domingo_semana_actual).FirstOrDefault();
                var nomina_actual = db.C_nomina_beta_d_areas_empleados.Where(x => x.activo == true && x.C_nomina_beta_g.activo == true &&
                                                x.C_nomina_beta_g.id_nomina_status != 4 && x.id_empleado == empleado_id
                                                && x.C_nomina_beta_g.fecha_inicio >= Lunes_semana_actual && x.C_nomina_beta_g.fecha_fin <= Domingo_semana_actual).Select(x => x.C_nomina_beta_g).FirstOrDefault();
                if (nomina_actual == null)
                {
                    foreach (var item in dias_inasistencias)
                    {
                        var inasistencia = new C_nomina_inasistencias
                        {
                            activo = true,
                            id_usuario_registra = id_usuario,
                            fecha_registro = fecha_registro,
                            fecha_inasistencia = DateTime.Parse(item),
                            Empleado_id = empleado_id,
                            observaciones = obs
                        };
                        db.C_nomina_inasistencias.Add(inasistencia);
                    }
                    db.SaveChanges();
                    return 2;
                }
                else
                {
                    if (nomina_actual.id_nomina_status == 3) { return 1; }
                    if (nomina_actual.id_nomina_status == 1)
                    {
                        foreach (var item in dias_inasistencias)
                        {
                            var inasistencia = new C_nomina_inasistencias
                            {
                                activo = true,
                                id_usuario_registra = id_usuario,
                                fecha_registro = fecha_registro,
                                fecha_inasistencia = DateTime.Parse(item),
                                Empleado_id = empleado_id,
                                observaciones = obs
                            };
                            db.C_nomina_inasistencias.Add(inasistencia);
                        }
                        db.SaveChanges();
                    }
                }
                var nomina_empleado = db.C_nomina_beta_d_areas_empleados.Where(x => x.id_empleado == empleado_id && x.id_nomina_g == nomina_actual.id_nomina_g && x.activo == true).FirstOrDefault();
                if (nomina_empleado == null) return 3;
                int[] concepto = { 104, 105 };


                var nomina_empleado_concepto = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true && x.id_nomina_d_area_empleado == nomina_empleado.id_nomina_d_area_empleado && x.id_nomina_g == nomina_actual.id_nomina_g && concepto.Contains((int)x.id_concepto_nomina)).OrderBy(x => x.id_concepto_nomina).ToList();
                for (int i = 0; i < nomina_empleado_concepto.Count(); i++)
                {
                    if (nomina_empleado_concepto[i].C_nomina_conceptos.id_concepto_nomina == 104)
                    {
                        nomina_empleado.dias_laborados = nomina_empleado.dias_laborados - dias_inasistencias.Count();
                        if (nomina_empleado.dias_laborados < 0)
                        {
                            nomina_empleado.dias_laborados = 0;
                        }

                        nomina_empleado_concepto[i].valor = nomina_empleado.dias_laborados;
                    }
                    else if (nomina_empleado_concepto[i].C_nomina_conceptos.id_concepto_nomina == 105)
                    {
                        //nomina_empleado.dias_laborados = nomina_empleado.dias_laborados - dias_inasistencias.Count();
                        decimal porcentaje_septimo_dia = ((decimal)nomina_empleado.dias_laborados * 100) / 6;
                        decimal total_septimo_dia = ((decimal)nomina_empleado.salario_diario * porcentaje_septimo_dia) / 100;
                        nomina_empleado_concepto[i].valor = total_septimo_dia;
                    }
                }
                nomina_empleado.dias_laborados = (int)nomina_empleado.dias_laborados;
                db.SaveChanges();
                RecalcularNominaEmpleado(nomina_empleado.id_nomina_d_area_empleado);
                return 2;
            }
            catch (Exception ex)
            {
                return 4;
            }
        }

        public PartialViewResult MostrarInasistenciaEmpleadoGeneralTable(int area, DateTime fecha1, DateTime fecha2)
        {
            if (area == 0)
            {
                var empleado = db.C_nomina_inasistencias.Where(x => x.activo == true && x.fecha_inasistencia >= fecha1 && x.fecha_inasistencia <= fecha2).ToList();
                return PartialView("../NOMINA/Inasistencias/_InasistenciasEmpleadosTable", empleado);
            }
            else
            {
                var empleado = db.C_nomina_inasistencias.Where(x => x.activo == true && x.fecha_inasistencia >= fecha1 && x.fecha_inasistencia <= fecha2 && x.C_nomina_empleados.Depto_no_id == area).ToList();
                return PartialView("../NOMINA/Inasistencias/_InasistenciasEmpleadosTable", empleado);
            }
        }

        public int EliminarInasistencia(int id_inasistencia, int empleado_id)
        {
            try
            {
                CultureInfo latinCulture = new CultureInfo("es-ES");
                int semana_actual = 0;
                DateTime fecha_registro = DateTime.Now;
                DateTime Lunes_semana_actual, Domingo_semana_actual;
                DateTime Hoy = DateTime.Now;
                DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                Domingo_semana_actual = Lunes_semana_actual.AddDays(6);


                var nomina_actual = db.C_nomina_beta_g.Where(x => x.activo == true && x.id_nomina_status == 1 && x.fecha_inicio >= Lunes_semana_actual && x.fecha_fin <= Domingo_semana_actual).FirstOrDefault();
                if (nomina_actual == null)
                {
                    var nomina_existente = db.C_nomina_beta_g.Where(x => x.activo == true && x.id_nomina_status == 3 && x.fecha_inicio >= Lunes_semana_actual && x.fecha_fin <= Domingo_semana_actual).FirstOrDefault();
                    if (nomina_existente != null)
                    {
                        return 1;
                    }
                    else
                    {
                        var inasistencia = db.C_nomina_inasistencias.Find(id_inasistencia);
                        inasistencia.activo = false;
                        db.SaveChanges();
                        return 2;
                    }
                }
                else
                {
                    int[] concepto = { 104, 105 };
                    var inasistencia = db.C_nomina_inasistencias.Find(id_inasistencia);
                    inasistencia.activo = false;
                    var nomina_empleado = db.C_nomina_beta_d_areas_empleados.Where(x => x.id_empleado == empleado_id && x.id_nomina_g == nomina_actual.id_nomina_g && x.activo == true).FirstOrDefault();
                    if (nomina_empleado == null)
                        return 3;
                    int dias_trabajados = (int)nomina_empleado.dias_laborados + 1;
                    if (dias_trabajados > 6)
                        dias_trabajados = 6;
                    nomina_empleado.dias_laborados = dias_trabajados;
                    db.SaveChanges();
                    var nomina_empleado_concepto = db.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true && x.id_nomina_d_area_empleado == nomina_empleado.id_nomina_d_area_empleado && x.id_nomina_g == nomina_actual.id_nomina_g && concepto.Contains((int)x.id_concepto_nomina)).OrderBy(x => x.id_concepto_nomina).ToList();
                    for (int i = 0; i < nomina_empleado_concepto.Count(); i++)
                    {
                        if (nomina_empleado_concepto[i].C_nomina_conceptos.id_concepto_nomina == 104)
                        {
                            if (dias_trabajados < 0)
                                dias_trabajados = 0;

                            nomina_empleado_concepto[i].valor = dias_trabajados;
                        }
                        else if (nomina_empleado_concepto[i].C_nomina_conceptos.id_concepto_nomina == 105)
                            nomina_empleado_concepto[i].valor = (dias_trabajados * nomina_empleado.salario_diario) / 6;
                    }
                    nomina_empleado.dias_laborados = (int)dias_trabajados;
                    db.SaveChanges();
                    RecalcularNominaEmpleado(nomina_empleado.id_nomina_d_area_empleado);
                    return 2;
                }

            }
            catch (Exception ex)
            {
                return 4;
            }
        }


        public PartialViewResult MostrarDetalleInasistencias(int empleado_id, DateTime fecha1, DateTime fecha2)
        {
            ViewBag.fd1 = fecha1;
            ViewBag.fd2 = fecha2;

            CultureInfo latinCulture = new CultureInfo("es-ES");
            int semana_actual = 0;
            DateTime fecha_registro = DateTime.Now;
            DateTime Lunes_semana_actual, Domingo_semana_actual;
            DateTime Hoy = DateTime.Now;
            DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
            semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
            if (semana_actual > 1) { semana_actual = semana_actual - 1; }
            Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
            Domingo_semana_actual = Lunes_semana_actual.AddDays(6);


            var nomina_actual = db.C_nomina_beta_g.Where(x => x.activo == true && x.id_nomina_status != 4 && x.fecha_inicio >= Lunes_semana_actual && x.fecha_fin <= Domingo_semana_actual).FirstOrDefault();
            if (nomina_actual != null)
            {
                ViewBag.estatus_faltas = nomina_actual.id_nomina_status;
            }
            else
            {
                ViewBag.estatus_faltas = 0;
            }


            var empleado = db.C_nomina_inasistencias.Where(x => x.activo == true && x.Empleado_id == empleado_id && x.fecha_inasistencia >= fecha1 && x.fecha_inasistencia <= fecha2).ToList();
            return PartialView("../NOMINA/Inasistencias/_DetalleInasistenciaEmpleado", empleado);
        }


        public JsonResult ObtenerLunesSabadoActual()
        {
            try
            {
                CultureInfo latinCulture = new CultureInfo("es-ES");
                int semana_actual = 0;
                DateTime Hoy = DateTime.Now;
                DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
                semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
                if (semana_actual > 1) { semana_actual = semana_actual - 1; }
                DateTime Lunes_semana_actual = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual);
                DateTime Sabado_semana_actual = Lunes_semana_actual.AddDays(6);

                var LunesSabado = new
                {
                    Lunes = Lunes_semana_actual.ToString("yyyy-MM-dd"),
                    Sabado = Sabado_semana_actual.ToString("yyyy-MM-dd")
                };

                return Json(LunesSabado);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        public PartialViewResult ReporteFaltasEmpleado(int area, DateTime fecha1, DateTime fecha2)
        {
            fecha1 = fecha1.Date;
            fecha2 = fecha2.Date.AddDays(1).AddTicks(-1);
            if (area == 0)
            {
                var inasistencias = db.C_nomina_inasistencias.Where(x => x.activo == true && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2).ToList();
                return PartialView("../NOMINA/Inasistencias/_ReporteInasistencia", inasistencias);
            }
            else
            {
                var inasistencias = db.C_nomina_inasistencias.Where(x => x.activo == true && x.C_nomina_empleados.Depto_no_id == area && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2).ToList();
                return PartialView("../NOMINA/Inasistencias/_ReporteInasistencia", inasistencias);
            }
        }

        public PartialViewResult GenerarFormatoReporte(int area, DateTime fecha1, DateTime fecha2)
        {
            fecha1 = fecha1.Date;
            fecha2 = fecha2.Date.AddDays(1).AddTicks(-1);

            ViewBag.fecha_falta_1 = fecha1;
            ViewBag.fecha_falta_2 = fecha2;
            if (area == 0)
            {
                var inasistencias = db.C_nomina_inasistencias.Where(x => x.activo == true && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2).ToList();
                return PartialView("../NOMINA/Inasistencias/_PDFReporteFaltas", inasistencias);
            }
            else
            {
                var inasistencias = db.C_nomina_inasistencias.Where(x => x.activo == true && x.C_nomina_empleados.Depto_no_id == area && x.fecha_registro >= fecha1 && x.fecha_registro <= fecha2).ToList();
                return PartialView("../NOMINA/Inasistencias/_PDFReporteFaltas", inasistencias);
            }
        }
        #endregion

        #region CHECADOR
        public int ObtenerNumeroSemanaActual()
        {
            // Definir el día de inicio de la semana (jueves)
            DayOfWeek diaInicioSemana = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);

            // Obtener la fecha actual
            DateTime fechaActual = DateTime.Now;

            // Ajustar la fecha actual para que corresponda al rango de jueves a viernes
            int diferenciaDias = (7 + (fechaActual.DayOfWeek - diaInicioSemana)) % 7;

            // Restar los días para obtener el "jueves" de la semana actual
            DateTime juevesDeLaSemana = fechaActual.AddDays(-diferenciaDias);

            // Usar la cultura invariante para evitar problemas con el formato regional
            CultureInfo cultura = CultureInfo.InvariantCulture;

            // Obtener el número de la semana basado en el jueves como referencia
            return cultura.Calendar.GetWeekOfYear(juevesDeLaSemana, CalendarWeekRule.FirstFourDayWeek, diaInicioSemana);
        }

        public int ObtenerNumeroSemanas(int year, int month)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            DateTime lastDayOfMonth = new DateTime(year, month, daysInMonth);
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            CalendarWeekRule calendarWeekRule = CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int firstWeek = calendar.GetWeekOfYear(firstDayOfMonth, calendarWeekRule, firstDayOfWeek);
            int lastWeek = calendar.GetWeekOfYear(lastDayOfMonth, calendarWeekRule, firstDayOfWeek);
            // Si el primer y último día del mes caen en semanas diferentes
            if (firstWeek != lastWeek)
            {
                return lastWeek - firstWeek + 1;
            }
            else
            {
                return 1; // Cuando ambos días caen en la misma semana
            }
        }

        public JsonResult ObtenerSemanas(int month, int year)
        {
            DateTime firstDayOfMonth = new DateTime(year, month, 1);

            Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);

            int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
            //int firstWeekOfYear = calendar.GetWeekOfYear(firstDayOfYear, weekRule, firstDayOfWeek);

            //DateTime firstDayOfFirstWeek = firstDayOfMonth;
            //while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
            //{
            //    firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
            //}

            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int lastWeekOfMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek);

            int numberOfWeeksInMonth = lastWeekOfMonth - firstWeekOfMonth;

            List<string> weeksNames = new List<string>();
            for (int i = 0; i < numberOfWeeksInMonth; i++)
            {
                weeksNames.Add($"{firstWeekOfMonth + i}");
            }

            var mesSemanas = new MesSemanas
            {
                PrimerDiaSemana = ObtenerPrimerDiaSemana(year, ObtenerNumeroSemanaActual()).ToShortDateString(),
                NumeroSemanas = numberOfWeeksInMonth,
                SemanasNumero = weeksNames
            };

            return Json(mesSemanas);
        }

        public JsonResult ObtenerFechaSemanas(int semana, int year)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);

            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
            
            DateTime firstDayOfWeekDate = firstDayOfYear.AddDays((semana - 1) * 7);
            while (firstDayOfWeekDate.DayOfWeek != firstDayOfWeek)
            {
                firstDayOfWeekDate = firstDayOfWeekDate.AddDays(-1);
            }

            return Json(firstDayOfWeekDate.ToShortDateString());
        }
        public JsonResult ObtenerFechaSemanasLunesDomingo(int semana, int year)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);

            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            DateTime firstDayOfWeekDate = firstDayOfYear.AddDays((semana - 1) * 7);
            while (firstDayOfWeekDate.DayOfWeek != firstDayOfWeek)
            {
                firstDayOfWeekDate = firstDayOfWeekDate.AddDays(-1);
            }

            return Json(firstDayOfWeekDate.ToShortDateString() + " - " + firstDayOfWeekDate.AddDays(6).ToShortDateString());
        }

        public bool RegistrarDiaChecador(int empleado, DateTime tiempo, int modo)
        {
            if (modo == 1 || modo == 3) { modo = 0; }
            if (modo == 2 || modo == 4) { modo = 1; }
            try
            {
                att_punches punches = new att_punches();
                punches.emp_id = empleado;
                punches.punch_time = tiempo;
                punches.workstate = "" + modo + "";
                punches.workcode = "0";
                punches.terminal_id = 2;
                punches.punch_type = "0";
                punches.iuser1 = 0;
                punches.iuser2 = 0;
                punches.iuser3 = 0;
                punches.IsSelect = 0;
                dbc.att_punches.Add(punches);
                dbc.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public bool ActualizarDiaChecador(int empleado, DateTime tiempo, int modo, int id_punches)
        {
            string modos = "0";
            if (modo == 1 || modo == 3) { modos = "0"; }
            if (modo == 2 || modo == 4) { modos = "1"; }

            try
            {
                var punches = dbc.att_punches.Where(x => x.id == id_punches && x.workstate == modos).FirstOrDefault();
                if (punches != null)
                {
                    punches.punch_time = tiempo;
                    dbc.SaveChanges();
                    return true;
                }
                else
                {
                    att_punches punches2 = new att_punches();
                    punches2.emp_id = empleado;
                    punches2.punch_time = tiempo;
                    punches2.workstate = "" + modo + "";
                    punches2.workcode = "0";
                    punches2.terminal_id = 2;
                    punches2.punch_type = "0";
                    punches2.iuser1 = 0;
                    punches2.iuser2 = 0;
                    punches2.iuser3 = 0;
                    punches2.IsSelect = 0;
                    dbc.att_punches.Add(punches2);
                    dbc.SaveChanges();
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }

        public ActionResult ReporteChecador()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8048)) { return View("/Views/Home/Index.cshtml"); }

                return View("Checador/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }



        public PartialViewResult ReporteJornadaLaboral(string fecha_semana, int[] id_departamento)
        {
            CultureInfo cultura = new CultureInfo("es-ES");
            Calendar calendario = cultura.Calendar;

            int id_usuario = (int)Session["LoggedId"];
            if (id_departamento.Contains(0))
            {
                id_departamento = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => (int)x.id_departamento_empleado).ToArray();
            }

            List<ReporteChecadorNomina> data = new List<ReporteChecadorNomina>();

            DateTime fecha_jornada = DateTime.Parse(fecha_semana).Date;
            DateTime[] fechas_semana = ConsultarFechasSemanaNominaArrayLunesDomingo(fecha_jornada);

            var empleados_ligados = db.C_nomina_empleados_checador.Where(x => id_departamento.Contains((int)x.C_nomina_empleados.Depto_no_id) && x.C_nomina_empleados.Estatus == "A").Distinct().ToList();
            int[] id_empleados_ligados = empleados_ligados.Select(x => (int)x.id_empleado_checador).ToArray();

            DateTime fecha_i = fechas_semana.Min().Date;
            DateTime fecha_f = fechas_semana.Max().Date.AddHours(23).AddMinutes(59);

            var checadas = dbc.att_punches.Where(x => id_empleados_ligados.Contains(x.emp_id) && x.punch_time >= fecha_i && x.punch_time <= fecha_f).ToList();

            TimeSpan horas_al_dia = new TimeSpan(9, 00, 00);
            foreach (var item in empleados_ligados)
            {
                var valid_concepto = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true);
                var valid_extras = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true).ToList();

                decimal valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;


                if (item.C_nomina_empleados.Nombre_completo == "LARA/GUERRERO,ADOLFO ")
                {
                    string hola = "";
                }

                for (int i = 0; i < fechas_semana.Length; i++)
                {
                    ReporteChecadorNomina obj = new ReporteChecadorNomina();
                    DateTime fecha_inicio = fechas_semana[i];
                    DateTime fecha_fin = fechas_semana[i].AddHours(23).AddMinutes(59);
                    obj.id_empleado = item.C_nomina_empleados.Empleado_id;
                    obj.nombre_empleado = item.C_nomina_empleados.Nombre_completo;
                    obj.nombre_dia = fechas_semana[i].ToString("dddd", cultura);

                    obj.no_dia = ((int)fechas_semana[i].DayOfWeek == 0) ? 7 : (int)fechas_semana[i].DayOfWeek;

                    obj.fecha_punch = fecha_inicio;
                    obj.id_departamento = (int)item.C_nomina_empleados.Depto_no_id;
                    obj.nombre_departamento = item.C_nomina_empleados.C_nomina_departamentos.nombre_departamento;
                    obj.falta = false;
                    obj.id_checador_concepto = 0;
                    obj.valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;

                    //ENTRADA
                    var punches_e = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin && x.workcode == "0").ToList();
                    if (punches_e.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_e = 0;
                    }
                    else
                    {
                        obj.fecha_punch_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().punch_time;
                        obj.id_punches_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().id;
                    }

                    //SALIDA
                    var punches_s = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin).ToList();
                    if (punches_s.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_s = 0;
                    }
                    else
                    {
                        if (punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time == obj.fecha_punch_e)
                        {
                            obj.falta = true;
                            obj.id_punches_e = 0;
                        }
                        else
                        {
                            obj.fecha_punch_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time;
                            obj.id_punches_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().id;
                        }
                    }

                    if (obj.no_dia == 7)
                    {
                        obj.nombre_concepto = "DESCANSO";
                        obj.concepto_modificado = false;
                        obj.id_checador_concepto = 6;
                    }
                    else
                    {
                        if (obj.falta == true) { obj.id_checador_concepto = 2; obj.nombre_concepto = "FALTA"; obj.concepto_modificado = false; }  //FALTA POR DEFAULT
                        else { obj.id_checador_concepto = 1; obj.nombre_concepto = "ASISTENCIA"; obj.concepto_modificado = false; }  //ASISTENCIA
                    }

                    //BUSCAR CONCEPTO
                    var concepto = valid_concepto.Where(x => x.fecha_accion == fecha_inicio).FirstOrDefault();
                    if (concepto != null)
                    {
                        if (obj.id_checador_concepto != concepto.id_nomina_checador_concepto) { obj.concepto_modificado = true; }
                        obj.id_checador_concepto = (int)concepto.id_nomina_checador_concepto;
                        obj.nombre_concepto = concepto.C_nomina_checador_conceptos.nombre_concepto;
                    }

                    //CALCULAR HRS TOTALES, EXTRA Y ESTIMULO
                    if (obj.falta == false)
                    {
                        TimeSpan hrs_contabilizadas = obj.fecha_punch_s - obj.fecha_punch_e;
                        obj.hrs_totales = hrs_contabilizadas;

                        hrs_contabilizadas = hrs_contabilizadas - horas_al_dia;
                        if (hrs_contabilizadas.Hours > 0)
                        {
                            obj.hrs_extra = hrs_contabilizadas.Hours;

                            if (hrs_contabilizadas.Minutes >= 50) { obj.hrs_extra = obj.hrs_extra + 1; } //TOLERANCIA 50 MIN = 1 HRA
                            obj.hrs_extra_valor = hrs_contabilizadas.Hours * valor_hra;
                        }
                    }

                    data.Add(obj);
                }

                int hrs_totales = data.Where(x => x.id_empleado == item.Empleado_id).Sum(x => x.hrs_extra);
                if (hrs_totales > 9)
                {
                    int hrs_diferencia = hrs_totales - 9;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_extra = 9);

                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo = hrs_diferencia);

                    decimal valor_estimulo = hrs_diferencia * valor_hra;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo_valor = valor_estimulo);

                }
            }

            ViewBag.fechas_semana = fechas_semana;
            //Session["lista_conceptos"] = db.C_nomina_checador_conceptos.Where(x => x.activo == true).ToList();
            return PartialView("Checador/_ReporteChecador", data);
        }

        public PartialViewResult ConsultarReporteChecadorMensual(string mes_inicio, int[] id_departamento )
        {
            string mes = mes_inicio + "-01";
            DateTime fecha_i = DateTime.Parse(mes);
            DateTime fecha_f = fecha_i.AddMonths(1); //.AddDays(-1).AddHours(23).AddMinutes(59);

            CultureInfo cultura = new CultureInfo("es-ES");
            Calendar calendario = cultura.Calendar;

            if (id_departamento.Contains(0))
            {
                int id_usuario = (int)Session["LoggedId"];
                id_departamento = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => (int)x.id_departamento_empleado).ToArray();
            }

            List<ReporteChecadorNomina> data = new List<ReporteChecadorNomina>();

            //DateTime fecha_jornada = DateTime.Parse(fecha_semana).Date;
            //DateTime[] fechas_semana = ConsultarFechasSemanaNominaArrayLunesDomingo(fecha_jornada);


            var empleados_ligados = db.C_nomina_empleados_checador.Where(x => id_departamento.Contains((int)x.C_nomina_empleados.Depto_no_id) && x.C_nomina_empleados.Estatus == "A").Distinct().ToList();
            int[] id_empleados_ligados = empleados_ligados.Select(x => (int)x.id_empleado_checador).ToArray();


            var checadas = dbc.att_punches.Where(x => id_empleados_ligados.Contains(x.emp_id) && x.punch_time >= fecha_i && x.punch_time <= fecha_f).ToList();

            TimeSpan horas_al_dia = new TimeSpan(9, 00, 00);
            foreach (var item in empleados_ligados)
            {
                fecha_i = DateTime.Parse(mes);
                var valid_concepto = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true);
                var valid_extras = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true).ToList();

                decimal valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;


                if (item.C_nomina_empleados.Nombre_completo == "LARA/GUERRERO,ADOLFO ")
                {
                    string hola = "";
                }

                while (fecha_i != fecha_f)
                {
                    ReporteChecadorNomina obj = new ReporteChecadorNomina();
                    DateTime fecha_inicio = fecha_i;
                    DateTime fecha_fin = fecha_i.AddHours(23).AddMinutes(59);
                    obj.id_empleado = item.C_nomina_empleados.Empleado_id;
                    obj.nombre_empleado = item.C_nomina_empleados.Nombre_completo;
                    obj.nombre_dia = fecha_i.ToString("dddd", cultura);

                    obj.no_dia = ((int)fecha_i.DayOfWeek == 0) ? 7 : (int)fecha_i.DayOfWeek;

                    obj.fecha_punch = fecha_inicio;
                    obj.id_departamento = (int)item.C_nomina_empleados.Depto_no_id;
                    obj.nombre_departamento = item.C_nomina_empleados.C_nomina_departamentos.nombre_departamento;
                    obj.falta = false;
                    obj.id_checador_concepto = 0;
                    obj.valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;

                    //ENTRADA
                    var punches_e = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin && x.workcode == "0").ToList();
                    if (punches_e.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_e = 0;
                    }
                    else
                    {
                        obj.fecha_punch_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().punch_time;
                        obj.id_punches_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().id;
                    }

                    //SALIDA
                    var punches_s = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin).ToList();
                    if (punches_s.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_s = 0;
                    }
                    else
                    {
                        if (punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time == obj.fecha_punch_e)
                        {
                            obj.falta = true;
                            obj.id_punches_e = 0;
                        }
                        else
                        {
                            obj.fecha_punch_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time;
                            obj.id_punches_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().id;
                        }
                    }

                    if (obj.no_dia == 7)
                    {
                        obj.nombre_concepto = "DESCANSO";
                        obj.concepto_modificado = false;
                        obj.id_checador_concepto = 6;
                    }
                    else
                    {
                        if (obj.falta == true) { obj.id_checador_concepto = 2; obj.nombre_concepto = "FALTA"; obj.concepto_modificado = false; }  //FALTA POR DEFAULT
                        else { obj.id_checador_concepto = 1; obj.nombre_concepto = "ASISTENCIA"; obj.concepto_modificado = false; }  //ASISTENCIA
                    }

                    //BUSCAR CONCEPTO
                    var concepto = valid_concepto.Where(x => x.fecha_accion == fecha_inicio).FirstOrDefault();
                    if (concepto != null)
                    {
                        if (obj.id_checador_concepto != concepto.id_nomina_checador_concepto) { obj.concepto_modificado = true; }
                        obj.id_checador_concepto = (int)concepto.id_nomina_checador_concepto;
                        obj.nombre_concepto = concepto.C_nomina_checador_conceptos.nombre_concepto;
                    }

                    //CALCULAR HRS TOTALES, EXTRA Y ESTIMULO
                    if (obj.falta == false)
                    {
                        TimeSpan hrs_contabilizadas = obj.fecha_punch_s - obj.fecha_punch_e;
                        obj.hrs_totales = hrs_contabilizadas;

                        hrs_contabilizadas = hrs_contabilizadas - horas_al_dia;
                        if (hrs_contabilizadas.Hours > 0)
                        {
                            obj.hrs_extra = hrs_contabilizadas.Hours;

                            if (hrs_contabilizadas.Minutes >= 50) { obj.hrs_extra = obj.hrs_extra + 1; } //TOLERANCIA 50 MIN = 1 HRA
                            obj.hrs_extra_valor = hrs_contabilizadas.Hours * valor_hra;
                        }
                    }

                    data.Add(obj);
                    fecha_i = fecha_i.AddDays(1);
                }

                int hrs_totales = data.Where(x => x.id_empleado == item.Empleado_id).Sum(x => x.hrs_extra);
                if (hrs_totales > 9)
                {
                    int hrs_diferencia = hrs_totales - 9;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_extra = 9);

                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo = hrs_diferencia);

                    decimal valor_estimulo = hrs_diferencia * valor_hra;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo_valor = valor_estimulo);

                }
            }
            return PartialView("Checador/_ReporteChecadorMensual", data);
        }


        #endregion



        #region SOLICITUDES ALTA DE EMPLEADOS
        public ActionResult SolicitudesAltaEmpleadosNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8065)) { return View("/Views/Home/Index.cshtml"); }

                return View("SolicitudesAltaEmpleados/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarFormatoSolicitud()
        {
            return PartialView("SolicitudesAltaEmpleados/_FormatoSolicitud");
        }

        public PartialViewResult ConsultarSoliciudesRevision(int modo)
        {
            List<C_nomina_empleados_solicitudes> solicitudes = null;
            if (modo == 1)
            {
                solicitudes = db.C_nomina_empleados_solicitudes.Where(x => x.activo == true && x.id_empleado_solicitud_status == 1).ToList();
            }
            else
            {
                solicitudes = db.C_nomina_empleados_solicitudes.Where(x => x.activo == true && x.id_empleado_solicitud_status == 2).ToList();
            }

            ViewBag.modo = modo;
            return PartialView("SolicitudesAltaEmpleados/_SolicitudesRevision", solicitudes);
        }


        public string ConsultarSolciitudAltaEmpleado(int id_empleado_solicitud, int modo)
        {
            var empleado = from emp in db.C_nomina_empleados_solicitudes
                           where emp.id_empleado_solicitud == id_empleado_solicitud
                           select new
                           {
                               //emp.Empleado_id,
                               //emp.Numero,
                               emp.path,
                               emp.Nombre_completo,
                               emp.Apellido_paterno,
                               emp.Apellido_materno,
                               emp.Nombres,
                               emp.Puesto_no_id,
                               emp.Depto_no_id,
                               emp.Frepag_id,
                               emp.Reg_patronal_id,
                               emp.Es_jefe,
                               emp.Forma_pago,
                               emp.Contrato,
                               emp.Dias_hrs_jsr,
                               emp.Turno,
                               emp.Jornada,
                               emp.Regimen_fiscal,
                               emp.Contrato_sat,
                               emp.Jornada_sat,
                               emp.Es_sindicalizado,
                               emp.Fecha_ingreso,
                               emp.Estatus,
                               emp.Zona_salmin,
                               emp.Tabla_antig_id,
                               emp.Tipo_salario,
                               emp.Pctje_integ,
                               emp.Salario_diario,
                               emp.Salario_hora,
                               emp.Salario_integ,
                               emp.Es_dir_admr_gte_gral,
                               emp.PTU,
                               emp.IMSS,
                               emp.CAS,
                               emp.Deshab_imptos,
                               emp.Calc_isr_anual,
                               emp.Calle,
                               emp.Nombre_calle,
                               emp.Num_exterior,
                               emp.Colonia,
                               emp.Poblacion,
                               emp.Referencia,
                               emp.Ciudad_id,
                               emp.Codigo_postal,
                               emp.Telefono1,
                               emp.Email,
                               emp.Sexo,
                               emp.Fecha_nacimiento,
                               emp.Ciudad_nacimiento_id,
                               emp.Estado_civil,
                               emp.Num_hijos,
                               emp.Nombre_padre,
                               emp.Nombre_madre,
                               emp.RFC,
                               emp.CURP,
                               emp.Reg_imss,
                               emp.Grupo_pago_elect_id,
                               emp.Tipo_ctaban_pago_elect,
                               emp.Num_ctaban_pago_elect,
                               emp.Salario_diario_establo,
                               emp.Salario_hora_establo
                           };
            return Newtonsoft.Json.JsonConvert.SerializeObject(empleado);
        }


        [HttpPost]
        public string FirmarSolicitudAltaEmpleado()
        {
            string path_pdf = "";
            HttpPostedFileBase file = null;
            HttpFileCollectionBase files = Request.Files;
            var jsonEmpleado = Request.Form["empleado"];
            int id_solicitud_g = Convert.ToInt32(Request.Form["id_solicitud_g"]);
            int modo = Convert.ToInt32(Request.Form["modo"]);
            C_nomina_empleados_solicitudes empleado = JsonConvert.DeserializeObject<C_nomina_empleados_solicitudes>(jsonEmpleado);

            var solicitud = db.C_nomina_empleados_solicitudes.Find(id_solicitud_g);
            solicitud.Nombre_completo = empleado.Nombre_completo;
            solicitud.Apellido_paterno = empleado.Apellido_paterno;
            solicitud.Apellido_materno = empleado.Apellido_materno;
            solicitud.Nombres = empleado.Nombres;
            solicitud.Regimen = empleado.Regimen;
            solicitud.Puesto_no_id = empleado.Puesto_no_id;
            solicitud.Depto_no_id = empleado.Depto_no_id;
            solicitud.Frepag_id = empleado.Frepag_id;
            solicitud.Reg_patronal_id = empleado.Reg_patronal_id;
            solicitud.Es_jefe = empleado.Es_jefe;
            solicitud.Forma_pago = empleado.Forma_pago;
            solicitud.Contrato = empleado.Contrato;
            solicitud.Dias_hrs_jsr = empleado.Dias_hrs_jsr;
            solicitud.Turno = empleado.Turno;
            solicitud.Jornada = empleado.Jornada;
            solicitud.Regimen_fiscal = empleado.Regimen_fiscal;
            solicitud.Contrato_sat = empleado.Contrato_sat;
            solicitud.Jornada_sat = empleado.Jornada_sat;
            solicitud.Es_sindicalizado = empleado.Es_sindicalizado;
            solicitud.Fecha_ingreso = empleado.Fecha_ingreso;
            solicitud.Estatus = empleado.Estatus;
            solicitud.Zona_salmin = empleado.Zona_salmin;
            solicitud.Tabla_antig_id = empleado.Tabla_antig_id;
            solicitud.Tipo_salario = empleado.Tipo_salario;
            solicitud.Pctje_integ = empleado.Pctje_integ;
            solicitud.Salario_diario = empleado.Salario_diario;
            solicitud.Salario_hora = empleado.Salario_hora;
            solicitud.Salario_integ = empleado.Salario_integ;
            solicitud.Es_dir_admr_gte_gral = empleado.Es_dir_admr_gte_gral;
            solicitud.PTU = empleado.PTU;
            solicitud.IMSS = empleado.IMSS;
            solicitud.CAS = empleado.CAS;
            solicitud.Pensionado = empleado.Pensionado;
            solicitud.Deshab_imptos = empleado.Deshab_imptos;
            solicitud.Calc_isr_anual = empleado.Calc_isr_anual;
            solicitud.Calle = empleado.Calle;
            solicitud.Nombre_calle = empleado.Nombre_calle;
            solicitud.Num_exterior = empleado.Num_exterior;
            solicitud.Colonia = empleado.Colonia;
            solicitud.Poblacion = empleado.Poblacion;
            solicitud.Referencia = empleado.Referencia;
            solicitud.Ciudad_id = empleado.Ciudad_id;
            solicitud.Codigo_postal = empleado.Codigo_postal;
            solicitud.Telefono1 = empleado.Telefono1;
            solicitud.Email = empleado.Email;
            solicitud.Sexo = empleado.Sexo;
            solicitud.Fecha_nacimiento = empleado.Fecha_nacimiento;
            solicitud.Ciudad_nacimiento_id = empleado.Ciudad_nacimiento_id;
            solicitud.Estado_civil = empleado.Estado_civil;
            solicitud.Num_hijos = empleado.Num_hijos;
            solicitud.Nombre_padre = empleado.Nombre_padre;
            solicitud.Nombre_madre = empleado.Nombre_madre;
            solicitud.RFC = empleado.RFC;
            solicitud.CURP = empleado.CURP;
            solicitud.Reg_imss = empleado.Reg_imss;
            solicitud.Grupo_pago_elect_id = empleado.Grupo_pago_elect_id;
            solicitud.Tipo_ctaban_pago_elect = empleado.Tipo_ctaban_pago_elect;
            solicitud.Num_ctaban_pago_elect = empleado.Num_ctaban_pago_elect;
            solicitud.fecha_registro = empleado.Fecha_ingreso;
            solicitud.Salario_diario_establo = empleado.Salario_diario_establo;
            solicitud.Salario_hora_establo = empleado.Salario_hora_establo;
            solicitud.activo = true;

            for (int z = 0; z < files.Count; z++)
            {
                file = files[z];
                var file_upload = Request.Files[z];
                if (!file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return "-3";
                }
            }
            for (int i = 0; i < files.Count; i++)
            {
                file = files[i];
                try
                {
                    var file_upload = Request.Files[i];
                    if (file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        var InputFileName = "Const_Fisc_" + empleado.Nombres.Replace(" ", "") + empleado.Apellido_paterno.Replace(" ", "") + '_' + empleado.RFC.Replace(" ", "") + ".pdf";
                        string fname = Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER"
                            ? file.FileName.Split(new char[] { '\\' }).Last()
                            : file.FileName;
                        var ServerSavePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\EmpleadosDocs\\ConstanciasFiscales\\{InputFileName}";
                        file.SaveAs(ServerSavePath);
                        path_pdf = "http://192.168.128.2/ConstanciasFiscales/" + InputFileName;
                        solicitud.path = path_pdf;
                    }
                    else
                    {
                        file.InputStream.Close();
                        file.InputStream.Dispose();
                        System.IO.File.Delete(path_pdf);
                        return "-2";
                    }
                }
                catch (Exception ex)
                {
                    file.InputStream.Close();
                    file.InputStream.Dispose();
                    System.IO.File.Delete(path_pdf);
                    return "-2";
                }
            }
            int id_usuario = (int)Session["LoggedId"];
            //--------- ANA
            if (modo == 1)
            {
                solicitud.id_usuario_valida = id_usuario;
                solicitud.fecha_valida = DateTime.Now;
                solicitud.id_empleado_solicitud_status = 2;
                db.SaveChanges();
                return "0";
            }

            //-----------VALERIA
            else if (modo == 2)
            {
                solicitud.id_usuario_autoriza = id_usuario;
                solicitud.fecha_autoriza = DateTime.Now;
                solicitud.id_empleado_solicitud_status = 3;
                try
                {
                    Empleados_MS empleado_ms = new Empleados_MS();
                    empleado_ms.NUMERO = ObtenerUltimoNumero();
                    empleado_ms.NOMBRE_COMPLETO = empleado.Nombre_completo;
                    empleado_ms.APELLIDO_PATERNO = empleado.Apellido_paterno;
                    empleado_ms.APELLIDO_MATERNO = empleado.Apellido_materno;
                    empleado_ms.NOMBRES = empleado.Nombres;
                    empleado_ms.REGIMEN = empleado.Regimen;
                    empleado_ms.PUESTO_NO_ID = (int)empleado.Puesto_no_id;
                    empleado_ms.DEPTO_NO_ID = (int)empleado.Depto_no_id;
                    empleado_ms.FREPAG_ID = (int)empleado.Frepag_id;
                    empleado_ms.REG_PATRONAL_ID = (int)empleado.Reg_patronal_id;
                    empleado_ms.ES_JEFE = (bool)empleado.Es_jefe;
                    empleado_ms.FORMA_PAGO = Convert.ToChar(empleado.Forma_pago);
                    empleado_ms.CONTRATO = Convert.ToChar(empleado.Contrato);
                    empleado_ms.DIAS_HRS_JSR = empleado.Dias_hrs_jsr;
                    empleado_ms.TURNO = Convert.ToChar(empleado.Turno);
                    empleado_ms.JORNADA = Convert.ToDecimal(empleado.Jornada);
                    empleado_ms.REGIMEN_FISCAL = (int)empleado.Regimen_fiscal;
                    empleado_ms.CONTRATO_SAT = empleado.Contrato_sat;
                    empleado_ms.JORNADA_SAT = empleado.Jornada_sat;
                    empleado_ms.ES_SINDICALIZADO = Convert.ToChar(empleado.Es_sindicalizado);
                    empleado_ms.FECHA_INGRESO = (DateTime)empleado.Fecha_ingreso;
                    empleado_ms.ESTATUS = empleado.Estatus;
                    empleado_ms.ZONA_SALMIN = Convert.ToChar(empleado.Zona_salmin);
                    empleado_ms.TABLA_ANTIG_ID = (int)empleado.Tabla_antig_id;
                    empleado_ms.TIPO_SALARIO = (int)empleado.Tipo_salario;
                    empleado_ms.PCTJE_INTEG = (decimal)empleado.Pctje_integ;

                    empleado_ms.SALARIO_DIARIO = (decimal)empleado.Salario_diario;
                    empleado_ms.SALARIO_HORA = (decimal)empleado.Salario_hora;

                    empleado_ms.SALARIO_DIARIO_ESTABLO = (decimal)empleado.Salario_diario_establo;
                    empleado_ms.SALARIO_HORA_ESTABLO = (decimal)empleado.Salario_hora_establo;

                    empleado_ms.SALARIO_INTEG = (decimal)empleado.Salario_integ;
                    empleado_ms.ES_DIR_ADMR_GTE_GRAL = (bool)empleado.Es_dir_admr_gte_gral;
                    empleado_ms.PTU = (bool)empleado.PTU;
                    empleado_ms.IMSS = (bool)empleado.IMSS;
                    empleado_ms.CAS = (bool)empleado.CAS;
                    empleado_ms.PENSIONADO = (bool)empleado.Pensionado;
                    empleado_ms.DESHAB_IMPTOS = (bool)empleado.Deshab_imptos;
                    empleado_ms.CALC_ISR_ANUAL = (bool)empleado.Calc_isr_anual;
                    empleado_ms.CALLE = empleado.Calle;
                    empleado_ms.NOMBRE_CALLE = empleado.Nombre_calle;
                    empleado_ms.NUM_EXTERIOR = empleado.Num_exterior;
                    empleado_ms.COLONIA = empleado.Colonia;
                    empleado_ms.POBLACION = empleado.Poblacion;
                    empleado_ms.REFERENCIA = empleado.Referencia;
                    empleado_ms.CIUDAD_ID = (int)empleado.Ciudad_id;
                    empleado_ms.CODIGO_POSTAL = empleado.Codigo_postal;
                    empleado_ms.TELEFONO1 = empleado.Telefono1;
                    empleado_ms.EMAIL = empleado.Email;
                    empleado_ms.SEXO = Convert.ToChar(empleado.Sexo);
                    empleado_ms.FECHA_NACIMIENTO = (DateTime)empleado.Fecha_nacimiento;
                    empleado_ms.CIUDAD_NACIMIENTO_ID = (int)empleado.Ciudad_nacimiento_id;
                    empleado_ms.ESTADO_CIVIL = empleado.Estado_civil;
                    empleado_ms.NUM_HIJOS = (short)empleado.Num_hijos;
                    empleado_ms.NOMBRE_PADRE = empleado.Nombre_padre;
                    empleado_ms.NOMBRE_MADRE = empleado.Nombre_madre;
                    empleado_ms.RFC = empleado.RFC;
                    empleado_ms.CURP = empleado.CURP;
                    empleado_ms.REG_IMSS = empleado.Reg_imss;
                    empleado_ms.GRUPO_PAGO_ELECT_ID = Convert.ToChar(empleado.Grupo_pago_elect_id);
                    empleado_ms.TIPO_CTABAN_PAGO_ELECT = Convert.ToChar(empleado.Tipo_ctaban_pago_elect);
                    empleado_ms.NUM_CTABAN_PAGO_ELECT = empleado.Num_ctaban_pago_elect;
                    empleado_ms.USUARIO_CREADOR = "SYSDBA";
                    empleado_ms.PATH_CONSTANCIA_FISCAL = solicitud.path;
                    empleado_ms.FECHA_HORA_CREACION = (DateTime)solicitud.Fecha_hora_creacion;

                    //var result_registrar_empleado = RegistrarEmpleado(empleado_ms, 1);

                    var baseUrl = "http://192.168.128.2:84/api/SYNC_REGISTRAR_EMPLEADO/POST";
                    //var baseUrl = "http://localhost:52534/api/SYNC_REGISTRAR_EMPLEADO/POST";

                    string result_registrar_empleado = "";
                    var json = JsonConvert.SerializeObject(empleado_ms);
                    var data = new StringContent(json, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        HttpResponseMessage response = client.PostAsync(baseUrl, data).Result;


                        if (response.IsSuccessStatusCode)
                        {
                            string result = response.Content.ReadAsStringAsync().Result;
                            var jsonResponse = JObject.Parse(result);
                            result_registrar_empleado = jsonResponse["Message"].ToString();
                        }
                        else
                        {
                            Console.WriteLine("Error en la llamada a la API: " + response.StatusCode);
                        }
                    }



                    if (result_registrar_empleado.ToString() == "REGISTRO EXITOSO")
                    {
                        if (path_pdf != "")
                        {
                            solicitud.path = path_pdf;
                        }
                        db.SaveChanges();

                        try
                        {
                            string nombre_completo = empleado.Nombres + " " + empleado.Apellido_paterno + " " + empleado.Apellido_materno;
                            var usuario_autoriza = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario).FirstOrDefault().usuario;

                            notificacion.EnviarCorreoValidarPreRegistro(usuario_autoriza, nombre_completo, solicitud.RFC, solicitud.Reg_imss, id_solicitud_g.ToString());
                        }
                        catch (Exception) { }

                        return "0";
                    }
                    else
                    {
                        return "-4";
                    }
                }
                catch (Exception ex)
                {
                    string mjs = ex.ToString();
                    return "-4";
                }
            }
            return "-5";

        }



        public bool RechazarSolicitudAltaEmpleado(int id_empleado_solicitud)
        {
            try
            {
                var solicitud = db.C_nomina_empleados_solicitudes.Find(id_empleado_solicitud);
                solicitud.activo = false;
                solicitud.id_empleado_solicitud_status = 4;
                db.SaveChanges();


                int usuario = (int)Session["LoggedId"];

                var empleado = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == usuario).FirstOrDefault();
                string asunto = "Alta de empleado rechazada!";
                string mensaje = "<label>Tu solicitiud fue rechazada por el usuario: " + empleado.usuario + "</label><br />" +
                "<br /><label>El cantidato " + solicitud.Nombres + " " + solicitud.Apellido_paterno + " " + solicitud.Apellido_materno + " no cumple los criterios necesarios</label><strong></strong>" +
                "<br />" +
                "<br /><label>Saludos Cordiales</label>" +
                "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                notificacion.EnviarCorreoUsuario(asunto, solicitud.C_usuarios_corporativo2.C_empleados.correo, mensaje);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion



        #region PRE REGISTRO EMPLEADO
        public ActionResult PreRegistroEmpleadosNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8064)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/Preregistro_Empleados/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public bool PreRegistrarEmpleados(C_nomina_empleados_solicitudes empleado)
        {
            DateTime hoy = DateTime.Now;
            try
            {
                empleado.id_usuario_registra = (int)Session["LoggedId"];
                empleado.id_empleado_solicitud_status = 1;
                empleado.fecha_registro = hoy;
                empleado.Fecha_ingreso = hoy;
                empleado.activo = true;
                empleado.Usuario_creador = "SYSDBA";
                empleado.Fecha_hora_creacion = hoy;
                empleado.Referencia = "";
                empleado.Telefono2 = "";
                db.C_nomina_empleados_solicitudes.Add(empleado);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ConsultarSolicitudesEmpleadosRegistradas(int estatus, DateTime fecha_inicio, DateTime fecha_fin)
        {
            if (estatus == 0)
            {
                var preregistro = db.C_nomina_empleados_solicitudes.Where(x => x.activo == true && x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin).ToList();
                return PartialView("../NOMINA/Preregistro_Empleados/_PreregistroEmpTable", preregistro);
            }
            else
            {
                var preregistro = db.C_nomina_empleados_solicitudes.Where(x => x.activo == true && x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin).ToList();
                return PartialView("../NOMINA/Preregistro_Empleados/_PreregistroEmpTable", preregistro);
            }
        }

        [HttpPost]
        public string GuardarSolicitudAltaEmpleado()
        {
            DateTime hoy = DateTime.Now;
            string path_pdf = "";
            HttpPostedFileBase file = null;
            HttpFileCollectionBase files = Request.Files;
            var jsonEmpleado = Request.Form["empleado"];
            C_nomina_empleados_solicitudes empleado = JsonConvert.DeserializeObject<C_nomina_empleados_solicitudes>(jsonEmpleado);

            if (files.Count > 0)
            {
                for (int z = 0; z < files.Count; z++)
                {
                    file = files[z];
                    var file_upload = Request.Files[z];
                    if (!file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return "-2";
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    file = files[i];
                    try
                    {
                        var file_upload = Request.Files[i];
                        if (file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            var InputFileName = "Const_Fisc_" + empleado.Nombres.Replace(" ", "") + empleado.Apellido_paterno.Replace(" ", "") + '_' + empleado.RFC.Replace("-", "") + ".pdf";
                            string fname = Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER"
                                ? file.FileName.Split(new char[] { '\\' }).Last()
                                : file.FileName;
                            var ServerSavePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\EmpleadosDocs\\ConstanciasFiscales\\{InputFileName}";
                            file.SaveAs(ServerSavePath);
                            path_pdf = "http://192.168.128.2/ConstanciasFiscales/" + InputFileName;
                        }
                        else
                        {
                            file.InputStream.Close();
                            file.InputStream.Dispose();
                            System.IO.File.Delete(path_pdf);
                            return "-3";
                        }
                    }
                    catch (Exception ex)
                    {
                        file.InputStream.Close();
                        file.InputStream.Dispose();
                        System.IO.File.Delete(path_pdf);
                        return "-3";
                    }
                }
                try
                {
                    file.InputStream.Close();
                    file.InputStream.Dispose();
                    var id_usuario = (int)Session["LoggedId"];
                    empleado.id_usuario_registra = id_usuario;
                    empleado.Usuario_creador = "SYSDBA";
                    empleado.id_empleado_solicitud_status = 1;
                    empleado.fecha_registro = hoy;
                    empleado.Fecha_ingreso = hoy;
                    empleado.Fecha_hora_creacion = hoy;
                    empleado.activo = true;
                    empleado.path = path_pdf;
                    db.C_nomina_empleados_solicitudes.Add(empleado);
                    db.SaveChanges();

                    string nombre_completo = empleado.Nombres + " " + empleado.Apellido_paterno + " " + empleado.Apellido_materno;
                    var rh_empleado = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario).FirstOrDefault();
                    var puesto = db.C_nomina_empleados_solicitudes.Find(empleado.id_empleado_solicitud).Puesto_no_id;
                    string puesto_nombre = db.C_nomina_puestos.Where(x => x.id_puesto_empleado == puesto).FirstOrDefault().nombre_puesto;

                    notificacion.EnviarCorreoPreRegistro(rh_empleado.usuario, nombre_completo, empleado.RFC, puesto_nombre, empleado.Reg_imss, empleado.id_empleado_solicitud.ToString());


                    return "0";
                }
                catch (Exception)
                {
                    System.IO.File.Delete(path_pdf);
                    return "-5";
                }
            }
            else
            {
                return "-4";
            }
        }


        public string MostrarSolicitudPreRegistroEmpleadosNomina(int id_solicitud)
        {
            var empleado = from emp in db.C_nomina_empleados_solicitudes
                           where emp.id_empleado_solicitud == id_solicitud
                           select new
                           {
                               emp.Nombre_completo,
                               emp.Apellido_paterno,
                               emp.Apellido_materno,
                               emp.Nombres,
                               emp.Puesto_no_id,
                               emp.Depto_no_id,
                               emp.Frepag_id,
                               emp.Reg_patronal_id,
                               emp.Es_jefe,
                               emp.Forma_pago,
                               emp.Contrato,
                               emp.Dias_hrs_jsr,
                               emp.Turno,
                               emp.Jornada,
                               emp.Regimen_fiscal,
                               emp.Contrato_sat,
                               emp.Jornada_sat,
                               emp.Es_sindicalizado,
                               emp.Fecha_ingreso,
                               emp.Estatus,
                               emp.Zona_salmin,
                               emp.Tabla_antig_id,
                               emp.Tipo_salario,
                               emp.Pctje_integ,
                               emp.Salario_diario,
                               emp.Salario_hora,
                               emp.Salario_integ,
                               emp.Es_dir_admr_gte_gral,
                               emp.PTU,
                               emp.IMSS,
                               emp.CAS,
                               emp.Deshab_imptos,
                               emp.Calc_isr_anual,
                               emp.Calle,
                               emp.Nombre_calle,
                               emp.Num_exterior,
                               emp.Colonia,
                               emp.Poblacion,
                               emp.Referencia,
                               emp.Ciudad_id,
                               emp.Codigo_postal,
                               emp.Telefono1,
                               emp.Email,
                               emp.Sexo,
                               emp.Fecha_nacimiento,
                               emp.Ciudad_nacimiento_id,
                               emp.Estado_civil,
                               emp.Num_hijos,
                               emp.Nombre_padre,
                               emp.Nombre_madre,
                               emp.RFC,
                               emp.CURP,
                               emp.Reg_imss,
                               emp.Grupo_pago_elect_id,
                               emp.Tipo_ctaban_pago_elect,
                               emp.Num_ctaban_pago_elect,
                               emp.Salario_diario_establo,
                               emp.Salario_hora_establo
                           };
            return Newtonsoft.Json.JsonConvert.SerializeObject(empleado);
        }

        public decimal DeterminarPorcentajeIntegracionInicial()
        {
            var porcentaje = db.C_nomina_parametros_porcentajes_integracion.Where(x => x.activo == true && 1 <= x.tipo_maximo && 1 >= x.tiempo_minimo).OrderByDescending(x => x.id_nomina_parametro_porcentaje_integracion).FirstOrDefault();
            return (decimal)porcentaje.porcentaje_integracion;
        }


        #endregion


        #region REPORTE INCIDENCIAS
        public PartialViewResult ConsultarReporteIncidencias(int tipo_incidencia, DateTime fecha_inicio, DateTime fecha_fin)
        {
            fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (tipo_incidencia == 0)
            {
                var preregistro = db.C_nomina_incidencias.Where(x => x.Fecha >= fecha_inicio && x.Fecha <= fecha_fin).ToList();
                return PartialView("../NOMINA/Movimientos/_IncidenciasReporteTable", preregistro);
            }
            else
            {
                var preregistro = db.C_nomina_incidencias.Where(x => x.id_tipo_incidencia == tipo_incidencia && x.Fecha >= fecha_inicio && x.Fecha <= fecha_fin).ToList();
                return PartialView("../NOMINA/Movimientos/_IncidenciasReporteTable", preregistro);
            }
        }

        public ActionResult ExportarIncidenciasExcel(int tipo_incidencia, DateTime fecha_inicio, DateTime fecha_fin)
        {
            try
            {
                fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);
                var incidencias = tipo_incidencia == 0
                    ? db.C_nomina_incidencias.Where(x => x.Fecha >= fecha_inicio && x.Fecha <= fecha_fin).ToList()
                    : db.C_nomina_incidencias.Where(x => x.id_tipo_incidencia == tipo_incidencia && x.Fecha >= fecha_inicio && x.Fecha <= fecha_fin).ToList();
                var groupedIncidencias = incidencias.GroupBy(i => i.id_tipo_incidencia).ToList();
                using (var workbook = new XLWorkbook())
                {
                    if (groupedIncidencias.Any())
                    {
                        foreach (var group in groupedIncidencias)
                        {
                            var worksheet = workbook.Worksheets.Add($"{group.First().C_nomina_incidencias_tipos.nombre_tipo_incidencia}");
                            var currentRow = 1;
                            var columnCount = 5;
                            var tipoIncidencia = group.Key;
                            // Ajustar el tamaño de 'headerRange' según el tipo de incidencia
                            var headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                            switch (tipoIncidencia)
                            {
                                //BAJA
                                case 1:
                                    columnCount = 10;
                                    headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                                    break;
                                //CAMBIO DE PUESTO, SALARIO, REINGRESO
                                case 2:
                                case 3:
                                case 5:
                                    columnCount = 18;
                                    headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                                    break;
                            }
                            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                var image = worksheet.AddPicture(stream)
                                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                                     .Scale(0.2);  // Ajustar el tamaño de la imagen si es necesario
                            }
                            // Insertar el texto centrado en la cabecera
                            headerRange.Merge();
                            worksheet.Cell(currentRow, 1).Value = "Reporte de incidencias de Nomina"; // Colocar el texto en la primera celda del rango
                            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 24;
                            worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Black; // Asegurarse de que el color de la fuente sea diferente al fondo
                            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
                            // Ajustar la altura de las filas si es necesario
                            worksheet.Row(currentRow).Height = 16;
                            worksheet.Row(currentRow + 1).Height = 16;
                            worksheet.Row(currentRow + 2).Height = 16;
                            currentRow += 3;
                            // Configuración del título de reporte según el tipo de incidencia
                            var titleRange = worksheet.Range(currentRow, 1, currentRow, columnCount);
                            switch (tipoIncidencia)
                            {
                                //BAJA
                                case 1:
                                    titleRange.Merge();
                                    titleRange.Value = $"{group.First().C_nomina_incidencias_tipos.nombre_tipo_incidencia}S - {fecha_inicio.ToShortDateString()} AL {fecha_fin.ToShortDateString()}";
                                    break;
                                //CAMBIO DE PUESTO, SALARIO, REINGRESO
                                case 2:
                                case 3:
                                case 5:
                                    titleRange.Merge();
                                    titleRange.Value = $"{group.First().C_nomina_incidencias_tipos.nombre_tipo_incidencia}S - {fecha_inicio.ToShortDateString()} AL {fecha_fin.ToShortDateString()}";
                                    break;
                                default:
                                    titleRange.Value = $"POR DEFINIR";
                                    break;
                            }
                            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.FontSize = 14;
                            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(38, 38, 38);
                            titleRange.Style.Font.FontColor = XLColor.White;
                            currentRow++;
                            // Cabeceras de columna
                            var headers = new[] { "Fecha de registro", "N°", "Nombre del empleado", "Area", "Puesto", "RFC", "NSS" };
                            switch (tipoIncidencia)
                            {
                                //BAJA
                                case 1:
                                    headers = headers.Concat(new[] { "Regimen patronal", "Causa de la baja", "Apto para reingreso" }).ToArray();
                                    break;
                                //CAMBIO DE PUESTO, SALARIO, REINGRESO
                                case 2:
                                case 3:
                                case 5:
                                    headers = headers.Concat(new[] { "Tipo de salario", "Porcentaje de integracion", "Regimen patronal", "Area Nueva", "Puesto Nuevo", "Salario diario anterior", "Salario hora anterior", "Salario integrado anterior", "Salario diario nuevo", "Salario hora nuevo", "Salario integrado nuevo" }).ToArray();
                                    break;
                            }
                            for (int i = 0; i < headers.Length; i++)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = headers[i];
                                worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(38, 109, 173);
                                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, i + 1).Style.Font.FontColor = XLColor.White;
                                worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 12;
                            }
                            int startRow = currentRow;
                            foreach (var item in group)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = item.Fecha?.ToShortDateString();
                                worksheet.Cell(currentRow, 2).Value = item.C_nomina_empleados.Numero;
                                worksheet.Cell(currentRow, 3).Value = $"{item.C_nomina_empleados.Nombres} {item.C_nomina_empleados.Apellido_paterno} {item.C_nomina_empleados.Apellido_materno}";
                                worksheet.Cell(currentRow, 6).Value = item.C_nomina_empleados.RFC;
                                worksheet.Cell(currentRow, 7).Value = item.C_nomina_empleados.Reg_imss;
                                switch (tipoIncidencia)
                                {
                                    //BAJA
                                    case 1:
                                        worksheet.Cell(currentRow, 4).Value = item.C_nomina_empleados.C_nomina_departamentos.nombre_departamento;
                                        worksheet.Cell(currentRow, 5).Value = item.C_nomina_empleados.C_nomina_puestos.nombre_puesto;
                                        worksheet.Cell(currentRow, 8).Value = item.C_nomina_regimen_patronal.numero_regimen_partonal;
                                        worksheet.Cell(currentRow, 9).Value = item.observaciones;
                                        if (item.Lista_negra == true)
                                        {
                                            worksheet.Cell(currentRow, 10).Value = "SI";
                                        }
                                        else
                                        {
                                            worksheet.Cell(currentRow, 10).Value = "NO";
                                        }
                                        break;
                                    //CAMBIO DE PUESTO, SALARIO, REINGRESO
                                    case 2:
                                    case 3:
                                    case 5:
                                        worksheet.Cell(currentRow, 4).Value = item.C_nomina_departamentos1.nombre_departamento;
                                        worksheet.Cell(currentRow, 5).Value = item.C_nomina_puestos1.nombre_puesto;
                                        if (item.C_nomina_empleados.Forma_pago == "T")
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "Transferencia";
                                        }
                                        else if (item.C_nomina_empleados.Forma_pago == "E")
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "Efectivo";
                                        }
                                        else if (item.C_nomina_empleados.Forma_pago == "C")
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "Cheque";
                                        }
                                        else
                                        {
                                            worksheet.Cell(currentRow, 8).Value = "NA";
                                        }
                                        worksheet.Cell(currentRow, 9).Value = item.Pctje_integ;
                                        worksheet.Cell(currentRow, 10).Value = item.C_nomina_regimen_patronal.numero_regimen_partonal;
                                        worksheet.Cell(currentRow, 11).Value = item.C_nomina_departamentos1.nombre_departamento;
                                        worksheet.Cell(currentRow, 12).Value = item.C_nomina_puestos1.nombre_puesto;
                                        worksheet.Cell(currentRow, 13).Value = item.Salario_diario;
                                        worksheet.Cell(currentRow, 13).Style.NumberFormat.Format = "$ #,##0.00";
                                        worksheet.Cell(currentRow, 14).Value = item.Salario_hora;
                                        worksheet.Cell(currentRow, 14).Style.NumberFormat.Format = "$ #,##0.00";
                                        worksheet.Cell(currentRow, 15).Value = item.Salario_integ;
                                        worksheet.Cell(currentRow, 15).Style.NumberFormat.Format = "$ #,##0.00";
                                        worksheet.Cell(currentRow, 16).Value = item.Salario_diario_nuevo;
                                        worksheet.Cell(currentRow, 16).Style.NumberFormat.Format = "$ #,##0.00";
                                        worksheet.Cell(currentRow, 17).Value = item.Salario_hora_nuevo;
                                        worksheet.Cell(currentRow, 17).Style.NumberFormat.Format = "$ #,##0.00";
                                        worksheet.Cell(currentRow, 18).Value = item.Salario_integ_nuevo;
                                        worksheet.Cell(currentRow, 18).Style.NumberFormat.Format = "$ #,##0.00";
                                        break;
                                }
                            }
                            var endRow = currentRow;
                            var tableRange = worksheet.Range(startRow, 1, endRow, columnCount);
                            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                            // Ajuste automático del ancho de las columnas
                            worksheet.Columns().AdjustToContents();
                        }
                    }
                    else
                    {
                        // Crear una hoja de trabajo vacía con un mensaje si no hay datos
                        var worksheet = workbook.Worksheets.Add("REPORTE");
                        worksheet.Cell(1, 1).Value = "No se encontraron incidencias para los criterios seleccionados.";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Row(1).Height = 30;
                        worksheet.Columns().AdjustToContents();
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE_INCIDENCIAS_BSM_" + fecha_inicio.ToShortDateString() + "_AL_" + fecha_fin.ToShortDateString() + ".xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Registrar el error o retornar un mensaje más específico
                // LogError(ex); // Asumiendo que tienes una función para registrar errores
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public ActionResult ExportarPreRegistroExcel(int estatus, DateTime fecha_inicio, DateTime fecha_fin)
        {
            try
            {
                fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);

                var incidencias = estatus == 0
                    ? db.C_nomina_empleados_solicitudes.Where(x => x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin).ToList()
                    : db.C_nomina_empleados_solicitudes.Where(x => x.id_empleado_solicitud_status == estatus && x.fecha_registro >= fecha_inicio && x.fecha_registro <= fecha_fin).ToList();

                var groupedIncidencias = incidencias.GroupBy(i => i.id_empleado_solicitud_status).ToList();

                using (var workbook = new XLWorkbook())
                {
                    if (groupedIncidencias.Any())
                    {
                        foreach (var group in groupedIncidencias)
                        {
                            var worksheet = workbook.Worksheets.Add($"{group.First().C_nomina_empleados_solicitudes_status.nombre_status}S");
                            var currentRow = 1;
                            var columnCount = 5;

                            var tipoIncidencia = group.Key;

                            // Ajustar el tamaño de 'headerRange' según el tipo de incidencia
                            var headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                            switch (tipoIncidencia)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    columnCount = 13;
                                    headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                                    break;
                            }

                            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                var image = worksheet.AddPicture(stream)
                                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                                     .Scale(0.2);  // Ajustar el tamaño de la imagen si es necesario
                            }

                            // Insertar el texto centrado en la cabecera
                            headerRange.Merge();
                            worksheet.Cell(currentRow, 1).Value = "Reporte de pre-alta de empleados"; // Colocar el texto en la primera celda del rango
                            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 24;
                            worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Black; // Asegurarse de que el color de la fuente sea diferente al fondo
                            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);

                            // Ajustar la altura de las filas si es necesario
                            worksheet.Row(currentRow).Height = 16;
                            worksheet.Row(currentRow + 1).Height = 16;
                            worksheet.Row(currentRow + 2).Height = 16;

                            currentRow += 3;

                            // Configuración del título de reporte según el tipo de incidencia
                            var titleRange = worksheet.Range(currentRow, 1, currentRow, columnCount);
                            switch (tipoIncidencia)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    titleRange.Merge();
                                    titleRange.Value = $"SOLICITUDES: {group.First().C_nomina_empleados_solicitudes_status.nombre_status}S - {fecha_inicio.ToShortDateString()} AL {fecha_fin.ToShortDateString()}";
                                    break;
                                default:
                                    titleRange.Value = $"REPORTE POR DEFINIR";
                                    break;
                            }

                            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.FontSize = 14;
                            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(38, 38, 38);
                            titleRange.Style.Font.FontColor = XLColor.White;

                            currentRow++;

                            // Cabeceras de columna
                            var headers = new[] { "Fecha de registro", "Nombre del empleado", "Area", "Puesto", "RFC", "NSS", "Sueldo diario oficina", "Sueldo diario establo" };

                            switch (tipoIncidencia)
                            {
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    headers = headers.Concat(new[] { "Calle", "No. exterior", "Colonia", "Ciudad", "C.P" }).ToArray();
                                    break;
                            }

                            for (int i = 0; i < headers.Length; i++)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = headers[i];
                                worksheet.Cell(currentRow, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                worksheet.Cell(currentRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(38, 109, 173);
                                worksheet.Cell(currentRow, i + 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, i + 1).Style.Font.FontColor = XLColor.White;
                                worksheet.Cell(currentRow, i + 1).Style.Font.FontSize = 12;
                            }

                            int startRow = currentRow;
                            foreach (var item in group)
                            {
                                currentRow++;
                                worksheet.Cell(currentRow, 1).Value = item.fecha_registro?.ToShortDateString();
                                worksheet.Cell(currentRow, 2).Value = $"{item.Nombres} {item.Apellido_paterno} {item.Apellido_materno}";
                                worksheet.Cell(currentRow, 3).Value = item.C_nomina_departamentos.nombre_departamento;
                                worksheet.Cell(currentRow, 4).Value = item.C_nomina_puestos.nombre_puesto;
                                worksheet.Cell(currentRow, 5).Value = item.RFC;
                                worksheet.Cell(currentRow, 6).Value = item.Reg_imss;
                                worksheet.Cell(currentRow, 7).Value = item.Salario_diario;
                                worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "$ #,##0.00";
                                worksheet.Cell(currentRow, 8).Value = item.Salario_diario_establo;
                                worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "$ #,##0.00";

                                switch (tipoIncidencia)
                                {
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        worksheet.Cell(currentRow, 9).Value = item.Calle;
                                        worksheet.Cell(currentRow, 10).Value = item.Num_exterior;
                                        worksheet.Cell(currentRow, 11).Value = item.Colonia;
                                        worksheet.Cell(currentRow, 12).Value = item.C_direcciones_ciudades1.nombre_ciudad;
                                        worksheet.Cell(currentRow, 13).Value = item.Codigo_postal;
                                        break;
                                }
                            }

                            var endRow = currentRow;
                            var tableRange = worksheet.Range(startRow, 1, endRow, columnCount);
                            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            // Ajuste automático del ancho de las columnas
                            worksheet.Columns().AdjustToContents();
                        }
                    }
                    else
                    {
                        // Crear una hoja de trabajo vacía con un mensaje si no hay datos
                        var worksheet = workbook.Worksheets.Add("REPORTE");
                        worksheet.Cell(1, 1).Value = "No se encontraron incidencias para los criterios seleccionados.";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Row(1).Height = 30;
                        worksheet.Columns().AdjustToContents();
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE_PREALTAS_BSM_" + fecha_inicio.ToShortDateString() + "_AL_" + fecha_fin.ToShortDateString() + ".xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Registrar el error o retornar un mensaje más específico
                // LogError(ex); // Asumiendo que tienes una función para registrar errores
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion


        #region CONTROL DE FALTAS Y HORAS EXTRA (CAHE)
        public ActionResult ControlFaltasHorasExtra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8071)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/ControlFaltasHorasExtra/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public string ConsultarFechaInicioFinNomina()
        {
            DateTime[] fechas = ConsultarFechasSemanaNominaArray(DateTime.Today);
            return "DEL " + fechas.Min().ToShortDateString() + " AL " + fechas.Max().ToShortDateString();
        }

        public DateTime[] ConsultarFechasSemanaNominaArray(DateTime Hoy)
        {
            DateTime[] fechas = new DateTime[7];
            CultureInfo latinCulture = new CultureInfo("es-ES");
            int semana_actual = 0;

            DayOfWeek firstDayOfWeek = (DayOfWeek)Convert.ToInt32(db.C_parametros_configuracion.Find(1016).valor_numerico);
            semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
            if (semana_actual > 1) { semana_actual = semana_actual - 1; }
            DateTime fecha_inicio = ObtenerPrimerDiaSemana(Hoy.Year, semana_actual).Date;
            for (int i = 0; i < fechas.Length; i++)
            {
                fechas[i] = fecha_inicio;
                fecha_inicio = fecha_inicio.AddDays(1);
            }
            return fechas;
        }
        public DateTime[] ConsultarFechasSemanaNominaArrayLunesDomingo(DateTime Hoy)
        {
            DateTime[] fechas = new DateTime[7];
            CultureInfo latinCulture = new CultureInfo("es-ES");
            int semana_actual = 0;

            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;
            semana_actual = latinCulture.Calendar.GetWeekOfYear(Hoy, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
            if (semana_actual > 1) { semana_actual = semana_actual - 1; }
            DateTime fecha_inicio = ObtenerPrimerDiaSemanaLunesDomingo(Hoy.Year, semana_actual).Date;
            for (int i = 0; i < fechas.Length; i++)
            {
                fechas[i] = fecha_inicio;
                fecha_inicio = fecha_inicio.AddDays(1);
            }
            return fechas;
        }

        public PartialViewResult ConsultarJornadaArea(int[] id_departamento)
        {
            CultureInfo cultura = new CultureInfo("es-ES");
            Calendar calendario = cultura.Calendar;

            int id_usuario = (int)Session["LoggedId"];
            if (id_departamento.Contains(0))
            {
                id_departamento = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => (int)x.id_departamento_empleado).ToArray();
            }

            List<ReporteChecadorNomina> data = new List<ReporteChecadorNomina>();

            DateTime[] fechas_semana = ConsultarFechasSemanaNominaArray(DateTime.Today);
            //DateTime[] fechas_semana = ConsultarFechasSemanaNominaArray(new DateTime(2024, 11, 13));

            var empleados_ligados = db.C_nomina_empleados_checador.Where(x => id_departamento.Contains((int)x.C_nomina_empleados.Depto_no_id) && x.C_nomina_empleados.Estatus == "A").Distinct().ToList();
            int[] id_empleados_ligados = empleados_ligados.Select(x => (int)x.id_empleado_checador).ToArray();

            DateTime fecha_i = fechas_semana.Min().Date;
            DateTime fecha_f = fechas_semana.Max().Date.AddHours(23).AddMinutes(59);

            var checadas = dbc.att_punches.Where(x => id_empleados_ligados.Contains(x.emp_id) && x.punch_time >= fecha_i && x.punch_time <= fecha_f).ToList();

            TimeSpan horas_al_dia = new TimeSpan(9, 00, 00);
            foreach (var item in empleados_ligados)
            {
                var valid_concepto = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true);
                var valid_extras = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == item.Empleado_id && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true).ToList();

                decimal valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;


                if (item.C_nomina_empleados.Nombre_completo == "LARA/GUERRERO,ADOLFO ")
                {
                    string hola = "";
                }

                for (int i = 0; i < fechas_semana.Length; i++)
                {
                    ReporteChecadorNomina obj = new ReporteChecadorNomina();
                    DateTime fecha_inicio = fechas_semana[i];
                    DateTime fecha_fin = fechas_semana[i].AddHours(23).AddMinutes(59);
                    obj.id_empleado = item.C_nomina_empleados.Empleado_id;
                    obj.nombre_empleado = item.C_nomina_empleados.Nombre_completo;
                    obj.nombre_dia = fechas_semana[i].ToString("dddd", cultura);

                    obj.no_dia = ((int)fechas_semana[i].DayOfWeek == 0) ? 7 : (int)fechas_semana[i].DayOfWeek;

                    obj.fecha_punch = fecha_inicio;
                    obj.id_departamento = (int)item.C_nomina_empleados.Depto_no_id;
                    obj.nombre_departamento = item.C_nomina_empleados.C_nomina_departamentos.nombre_departamento;
                    obj.falta = false;
                    obj.id_checador_concepto = 0;
                    obj.valor_hra = (decimal)item.C_nomina_empleados.Salario_hora;

                    //ENTRADA
                    var punches_e = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin && x.workcode == "0").ToList();
                    if (punches_e.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_e = 0;
                    }
                    else
                    {
                        obj.fecha_punch_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().punch_time;
                        obj.id_punches_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().id;
                    }

                    //SALIDA
                    var punches_s = checadas.Where(x => x.emp_id == item.id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin).ToList();
                    if (punches_s.Count() == 0)
                    {
                        obj.falta = true;
                        obj.id_punches_s = 0;
                    }
                    else
                    {
                        if (punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time == obj.fecha_punch_e)
                        {
                            obj.falta = true;
                            obj.id_punches_e = 0;
                        }
                        else
                        {
                            obj.fecha_punch_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time;
                            obj.id_punches_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().id;
                        }
                    }

                    if (obj.no_dia == 7)
                    {
                        obj.nombre_concepto = "DESCANSO"; 
                        obj.concepto_modificado = false;
                        obj.id_checador_concepto = 6;
                    }
                    else
                    {
                        if (obj.falta == true) { obj.id_checador_concepto = 2; obj.nombre_concepto = "FALTA"; obj.concepto_modificado = false; }  //FALTA POR DEFAULT
                        else { obj.id_checador_concepto = 1; obj.nombre_concepto = "ASISTENCIA"; obj.concepto_modificado = false; }  //ASISTENCIA
                    }
                    
                    //BUSCAR CONCEPTO
                    var concepto = valid_concepto.Where(x => x.fecha_accion == fecha_inicio).FirstOrDefault();
                    if (concepto != null)
                    {
                        if (obj.id_checador_concepto != concepto.id_nomina_checador_concepto) { obj.concepto_modificado = true; }
                        obj.id_checador_concepto = (int)concepto.id_nomina_checador_concepto;
                        obj.nombre_concepto = concepto.C_nomina_checador_conceptos.nombre_concepto;
                    }

                    //CALCULAR HRS TOTALES, EXTRA Y ESTIMULO
                    if (obj.falta == false)
                    {
                        TimeSpan hrs_contabilizadas = obj.fecha_punch_s - obj.fecha_punch_e;
                        obj.hrs_totales = hrs_contabilizadas;

                        hrs_contabilizadas = hrs_contabilizadas - horas_al_dia;
                        if (hrs_contabilizadas.Hours > 0)
                        {
                            obj.hrs_extra = hrs_contabilizadas.Hours;

                            if (hrs_contabilizadas.Minutes >= 50) { obj.hrs_extra = obj.hrs_extra + 1; } //TOLERANCIA 50 MIN = 1 HRA
                            obj.hrs_extra_valor = hrs_contabilizadas.Hours * valor_hra;
                        }
                    }

                    //BUSCAR HRS EXTRAS Y ESTIMULOS
                    if (valid_extras.Count() > 0)
                    {
                        obj.pagar_hrs_extra = (bool)valid_extras.FirstOrDefault().pagar_hrs;
                        obj.pagar_hrs_estimulo = (bool)valid_extras.FirstOrDefault().pagar_extimulo;
                    }
                    else
                    {
                        obj.pagar_hrs_extra = false;
                        obj.pagar_hrs_estimulo = false;
                    }
                        data.Add(obj);
                }

                int hrs_totales = data.Where(x => x.id_empleado == item.Empleado_id).Sum(x => x.hrs_extra);
                if (hrs_totales > 9)
                {
                    int hrs_diferencia = hrs_totales - 9;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_extra = 9);

                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo = hrs_diferencia);

                    decimal valor_estimulo = hrs_diferencia * valor_hra;
                    data.Where(x => x.id_empleado == item.Empleado_id).ToList().ForEach(x => x.hrs_estimulo_valor = valor_estimulo);

                }
            }

            ViewBag.fechas_semana = fechas_semana;
            Session["lista_conceptos"] = db.C_nomina_checador_conceptos.Where(x => x.activo == true).ToList();
            return PartialView("../NOMINA/ControlFaltasHorasExtra/_ReporteChecadorPrenomina", data);
        }


        public bool ProcesarCAHEEmpleado(int id_empleado_nomina, int id_empleado_checador)
        {
            try
            {
                var data = ConsultarCAHEEmpleado(id_empleado_nomina, id_empleado_checador, DateTime.Today, 2);
                var conceptosCAHE = data.OrderBy(x => x.fecha_punch).Select(x => new { x.fecha_punch, x.id_checador_concepto }).ToList();

                DateTime hoy = DateTime.Now;
                int id_usuario = 2109; //-------- SYS AUTOMATICO

                var fechas = conceptosCAHE.Select(x => x.fecha_punch).ToList();
                var validEntries = db.C_nomina_checador_prenomina
                    .Where(x => x.activo == true && x.Empleado_id == id_empleado_nomina && fechas.Contains((DateTime)x.fecha_accion))
                    .ToList();

                foreach (var item in conceptosCAHE)
                {
                    var valid = validEntries.FirstOrDefault(x => x.fecha_accion == item.fecha_punch);
                    if (valid == null)
                    {
                        C_nomina_checador_prenomina new_conceptos = new C_nomina_checador_prenomina
                        {
                            Empleado_id = id_empleado_nomina,
                            fecha_accion = item.fecha_punch,
                            id_nomina_checador_concepto = item.id_checador_concepto,
                            id_nomina_checador_concepto_real = item.id_checador_concepto,
                            id_usuario_registra = id_usuario,
                            fecha_registro = hoy,
                            activo = true
                        };
                        db.C_nomina_checador_prenomina.Add(new_conceptos);
                    }
                    else
                    {
                        valid.id_nomina_checador_concepto = item.id_checador_concepto;
                        db.SaveChanges();
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

        public List<ReporteChecadorNomina> ConsultarCAHEEmpleado(int id_empleado_nomina, int id_empleado_checador, DateTime fecha_cache, int modo)
        {
            //---- MODO 1 = PROCESO DE CAPTURA DEL CAHE
            //---- MODO 2 = PROCESO AUTOMATICO DEL CAHE

            CultureInfo cultura = new CultureInfo("es-ES");
            Calendar calendario = cultura.Calendar;

            List<ReporteChecadorNomina> data = new List<ReporteChecadorNomina>();
            DateTime[] fechas_semana = ConsultarFechasSemanaNominaArray(fecha_cache);
            DateTime fecha_i = fechas_semana.Min().Date;
            DateTime fecha_f = fechas_semana.Max().Date.AddHours(23).AddMinutes(59);

            var info_empelado = db.C_nomina_empleados.Find(id_empleado_nomina);
            var checadas = dbc.att_punches.Where(x => x.emp_id == id_empleado_checador && x.punch_time >= fecha_i && x.punch_time <= fecha_f).ToList();

            TimeSpan horas_al_dia = new TimeSpan(9, 00, 00);

            var valid_concepto = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == id_empleado_nomina && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true);
            var valid_extras = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == id_empleado_nomina && x.fecha_accion >= fecha_i && x.fecha_accion <= fecha_f && x.activo == true).ToList();

            decimal valor_hra = (decimal)info_empelado.Salario_hora;


            if (info_empelado.Nombre_completo == "LARA/GUERRERO,ADOLFO ")
            {
                string hola = "";
            }

            for (int i = 0; i < fechas_semana.Length; i++)
            {
                ReporteChecadorNomina obj = new ReporteChecadorNomina();
                DateTime fecha_inicio = fechas_semana[i];
                DateTime fecha_fin = fechas_semana[i].AddHours(23).AddMinutes(59);
                obj.id_empleado = info_empelado.Empleado_id;
                obj.nombre_empleado = info_empelado.Nombre_completo;
                obj.nombre_dia = fechas_semana[i].ToString("dddd", cultura);

                obj.no_dia = ((int)fechas_semana[i].DayOfWeek == 0) ? 7 : (int)fechas_semana[i].DayOfWeek;

                obj.fecha_punch = fecha_inicio;
                obj.id_departamento = (int)info_empelado.Depto_no_id;
                obj.nombre_departamento = info_empelado.C_nomina_departamentos.nombre_departamento;
                obj.falta = false;
                obj.id_checador_concepto = 0;
                obj.valor_hra = (decimal)info_empelado.Salario_hora;

                //ENTRADA
                var punches_e = checadas.Where(x => x.emp_id == id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin && x.workcode == "0").ToList();
                if (punches_e.Count() == 0)
                {
                    obj.falta = true;
                    obj.id_punches_e = 0;
                }
                else
                {
                    obj.fecha_punch_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().punch_time;
                    obj.id_punches_e = punches_e.OrderBy(x => x.punch_time).FirstOrDefault().id;
                }

                //SALIDA
                var punches_s = checadas.Where(x => x.emp_id == id_empleado_checador && x.punch_time >= fecha_inicio && x.punch_time <= fecha_fin).ToList();
                if (punches_s.Count() == 0)
                {
                    obj.falta = true;
                    obj.id_punches_s = 0;
                }
                else
                {
                    if (punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time == obj.fecha_punch_e)
                    {
                        obj.falta = true;
                        obj.id_punches_e = 0;
                    }
                    else
                    {
                        obj.fecha_punch_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().punch_time;
                        obj.id_punches_s = punches_s.OrderByDescending(x => x.punch_time).FirstOrDefault().id;
                    }
                }

                if (obj.no_dia == 7)
                {
                    obj.nombre_concepto = "DESCANSO";
                    obj.concepto_modificado = false;
                    obj.id_checador_concepto = 6;
                }
                else
                {
                    if (obj.falta == true) { obj.id_checador_concepto = 2; obj.nombre_concepto = "FALTA"; obj.concepto_modificado = false; }  //FALTA POR DEFAULT
                    else { obj.id_checador_concepto = 1; obj.nombre_concepto = "ASISTENCIA"; obj.concepto_modificado = false; }  //ASISTENCIA
                }

                //BUSCAR CONCEPTO
                if (modo == 1)
                {
                    var concepto = valid_concepto.Where(x => x.fecha_accion == fecha_inicio).FirstOrDefault();
                    if (concepto != null)
                    {
                        if (obj.id_checador_concepto != concepto.id_nomina_checador_concepto) { obj.concepto_modificado = true; }
                        obj.id_checador_concepto = (int)concepto.id_nomina_checador_concepto;
                        obj.nombre_concepto = concepto.C_nomina_checador_conceptos.nombre_concepto;
                    }
                }

                //CALCULAR HRS TOTALES, EXTRA Y ESTIMULO
                if (obj.falta == false)
                {
                    TimeSpan hrs_contabilizadas = obj.fecha_punch_s - obj.fecha_punch_e;
                    obj.hrs_totales = hrs_contabilizadas;

                    hrs_contabilizadas = hrs_contabilizadas - horas_al_dia;
                    if (hrs_contabilizadas.Hours > 0)
                    {
                        obj.hrs_extra = hrs_contabilizadas.Hours;

                        if (hrs_contabilizadas.Minutes >= 50) { obj.hrs_extra = obj.hrs_extra + 1; } //TOLERANCIA 50 MIN = 1 HRA
                        obj.hrs_extra_valor = hrs_contabilizadas.Hours * valor_hra;
                    }
                }

                //BUSCAR HRS EXTRAS Y ESTIMULOS
                if (valid_extras.Count() > 0)
                {
                    obj.pagar_hrs_extra = (bool)valid_extras.FirstOrDefault().pagar_hrs;
                    obj.pagar_hrs_estimulo = (bool)valid_extras.FirstOrDefault().pagar_extimulo;
                }
                else
                {
                    obj.pagar_hrs_extra = false;
                    obj.pagar_hrs_estimulo = false;
                }
                data.Add(obj);
            }
            int hrs_totales = data.Where(x => x.id_empleado == id_empleado_nomina).Sum(x => x.hrs_extra);
            if (hrs_totales > 9)
            {
                int hrs_diferencia = hrs_totales - 9;
                data.Where(x => x.id_empleado == id_empleado_nomina).ToList().ForEach(x => x.hrs_extra = 9);

                data.Where(x => x.id_empleado == id_empleado_nomina).ToList().ForEach(x => x.hrs_estimulo = hrs_diferencia);

                decimal valor_estimulo = hrs_diferencia * valor_hra;
                data.Where(x => x.id_empleado == id_empleado_nomina).ToList().ForEach(x => x.hrs_estimulo_valor = valor_estimulo);
            }

            return data;
        }



        public int GuardarPrenominaChecador(int[] id_emp, DateTime[] fechas, int[] id_concep)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime hoy = DateTime.Now;

            DateTime fecha_inicio = fechas.Min();
            DateTime fecha_fin = fechas.Max();

            db.C_nomina_checador_prenomina.Where(x => id_emp.Contains((int)x.Empleado_id) && fechas.Contains((DateTime)x.fecha_accion)).ToList().ForEach(x => x.activo = false);
            db.SaveChanges();

            for (int i = 0; i < id_emp.Length; i++)
            {
                try
                {
                    int id_empleado = id_emp[i];
                    DateTime fecha = fechas[i];
                    int id_concepto = id_concep[i];

                    var valid_concepto = db.C_nomina_checador_prenomina.Where(x => x.Empleado_id == id_empleado && x.fecha_accion == fecha && x.id_nomina_checador_concepto == id_concepto && x.activo == true).FirstOrDefault();
                    if (valid_concepto == null)
                    {
                        C_nomina_checador_prenomina new_concepto = new C_nomina_checador_prenomina();
                        new_concepto.Empleado_id = id_empleado;
                        new_concepto.fecha_accion = fecha;
                        new_concepto.id_nomina_checador_concepto = id_concepto;
                        new_concepto.fecha_registro = hoy;
                        new_concepto.id_usuario_registra = id_usuario;
                        new_concepto.activo = true;
                        db.C_nomina_checador_prenomina.Add(new_concepto);
                        db.SaveChanges();
                    }
                    else
                    {
                        valid_concepto.activo = true;
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {

                }
            }
            return 0;
        }

        public bool GuardarCAHEAsistenciasFaltas(int[] id_empleados, int[] concepto_real, int[] conceptos, string[] fechas)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];
                for (int i = 0; i < id_empleados.Length; i++)
                {
                    int id_empleado = id_empleados[i];
                    DateTime fecha = DateTime.Parse(fechas[i]);
                    var valid = db.C_nomina_checador_prenomina.Where(x => x.activo == true && x.Empleado_id == id_empleado && x.fecha_accion == fecha).FirstOrDefault();
                    if (valid == null)
                    {
                        C_nomina_checador_prenomina new_conceptos = new C_nomina_checador_prenomina();
                        new_conceptos.Empleado_id = id_empleado;
                        new_conceptos.fecha_accion = fecha;
                        new_conceptos.id_nomina_checador_concepto = conceptos[i];
                        new_conceptos.id_nomina_checador_concepto_real = concepto_real[i];
                        new_conceptos.id_usuario_registra = id_usuario;
                        new_conceptos.fecha_registro = hoy;
                        new_conceptos.activo = true;
                        db.C_nomina_checador_prenomina.Add(new_conceptos);
                        db.SaveChanges();
                    }
                    else
                    {
                        valid.id_nomina_checador_concepto = conceptos[i];
                        db.SaveChanges();
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

        public bool GuardarCAHEHorasExtrasEstimulos(int[] id_empleados, bool[] HR_pagar, int[] HR_no, decimal[] HR_monto, bool[] EE_pagar, int[] EE_no, decimal[] EE_monto, string[] fechas)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];
                for (int i = 0; i < id_empleados.Length; i++)
                {
                    int id_empleado = id_empleados[i];
                    DateTime fecha = DateTime.Parse(fechas[i]);

                    var valid = db.C_nomina_checador_hrs_extras_estimulos.Where(x => x.Empleado_id == id_empleado && x.fecha_accion == fecha && x.activo == true).FirstOrDefault();
                    if (valid == null)
                    {
                        C_nomina_checador_hrs_extras_estimulos new_estimulo = new C_nomina_checador_hrs_extras_estimulos();
                        new_estimulo.Empleado_id = id_empleado;
                        new_estimulo.fecha_accion = fecha;
                        new_estimulo.fecha_registro = hoy;
                        new_estimulo.id_usuario_registra = id_usuario;
                        new_estimulo.pagar_hrs = HR_pagar[i];
                        new_estimulo.hrs_extra_no = HR_no[i];
                        new_estimulo.hrs_extra_monto = HR_monto[i];
                        new_estimulo.pagar_extimulo = EE_pagar[i];
                        new_estimulo.estimulo_extra_no = EE_no[i];
                        new_estimulo.estimulo_extra_monto = EE_monto[i];
                        new_estimulo.activo = true;
                        db.C_nomina_checador_hrs_extras_estimulos.Add(new_estimulo);
                        db.SaveChanges();
                    }
                    else
                    {
                        valid.pagar_hrs = HR_pagar[i];
                        valid.hrs_extra_no = HR_no[i];
                        valid.hrs_extra_monto = HR_monto[i];
                        valid.pagar_extimulo = EE_pagar[i];
                        valid.estimulo_extra_no = EE_no[i];
                        valid.estimulo_extra_monto = EE_monto[i];
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region REPORTE PDF NOMINA

        private PdfPTable GenerarEncabezadoReporte(string encabezado, iTextSharp.text.Font titleFont, iTextSharp.text.Font cellFont, DateTime fecha_i, DateTime fecha_f)
        {
            PdfPTable headerTable = new PdfPTable(3) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 1f, 3f, 1f });

            // Agregar logo en la primera celda
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(60f, 60f);
                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                headerTable.AddCell(logoCell);
            }

            // Título de la tabla en el centro
            PdfPCell titleCell = new PdfPCell(new Phrase(encabezado, titleFont))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(titleCell);

            // Fechas a la derecha
            PdfPTable dateTable = new PdfPTable(1);
            dateTable.AddCell(new PdfPCell(new Phrase($"Fecha Inicial: {fecha_i.ToShortDateString()}", cellFont))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
            dateTable.AddCell(new PdfPCell(new Phrase($"Fecha Final: {fecha_f.ToShortDateString()}", cellFont))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });
            PdfPCell dateCell = new PdfPCell(dateTable)
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            headerTable.AddCell(dateCell);

            return headerTable;
        }
        private PdfPTable GenerarEncabezadoNominaReporte(string[] portada_prenomina)
        {
            // Define the color
            var customColor = new BaseColor(54, 139, 216);

            // Create a table for the header with a single column
            PdfPTable headerTable = new PdfPTable(1) { WidthPercentage = 100 };

            // Add the logo centered and larger
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(600f, 600f); // Adjust logo size
                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(logoCell);
            }

            // Create a paragraph with line breaks
            Paragraph paragraph = new Paragraph("\n\n\n\n");
            PdfPCell emptyCell = new PdfPCell(paragraph)
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER
            };
            headerTable.AddCell(emptyCell);

            // Create the secondary table of three columns with information
            PdfPTable infoTable = new PdfPTable(3) { WidthPercentage = 100 };
            infoTable.SetWidths(new float[] { .34f, .32f, .34f });

            // Cell 1
            PdfPCell cell1 = new PdfPCell(new Phrase(
                "Sistema Integral de Informacion BETA\n",
                new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL))) // Apply color
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            cell1.Phrase.Add(new Chunk("Modulo de administracion de recursos humanos\n", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            // Add dates with specified color
            cell1.Phrase.Add(new Chunk("Nomina aplicada del ", FontFactory.GetFont("HELVETICA", 12)));
            cell1.Phrase.Add(new Chunk(portada_prenomina[0], new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            cell1.Phrase.Add(" al ");
            cell1.Phrase.Add(new Chunk(portada_prenomina[1], new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            infoTable.AddCell(cell1);

            // Cell 2
            PdfPCell cell2 = new PdfPCell(new Phrase(
                "Fecha de registro: ", FontFactory.GetFont("HELVETICA", 12, Font.NORMAL)))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            // Add other phrases with color
            cell2.Phrase.Add(new Chunk(portada_prenomina[2] + "\n", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            cell2.Phrase.Add(new Chunk("Total de empleados: \n", FontFactory.GetFont("HELVETICA", 12)));
            cell2.Phrase.Add(new Chunk(portada_prenomina[3], new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            cell2.Phrase.Add(new Chunk("\nTotal de departamentos: \n", FontFactory.GetFont("HELVETICA", 12)));
            cell2.Phrase.Add(new Chunk(portada_prenomina[6], new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            infoTable.AddCell(cell2);

            // Cell 3
            PdfPCell cell3 = new PdfPCell(new Phrase(
                "Del ", FontFactory.GetFont("HELVETICA", 12, Font.NORMAL)))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            // Add date and generated-by details with color
            cell3.Phrase.Add(new Chunk(portada_prenomina[0] + " al " + portada_prenomina[1] + "\n", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            cell3.Phrase.Add(new Chunk("Generada por: ", FontFactory.GetFont("HELVETICA", 12)));
            cell3.Phrase.Add(new Chunk(portada_prenomina[4] + "\n", new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));
            cell3.Phrase.Add(new Chunk("Importe total de la nomina: \n", FontFactory.GetFont("HELVETICA", 12)));
            decimal importeTotal = Convert.ToDecimal(portada_prenomina[5]);
            string importe = importeTotal.ToString("#,##0.00");

            // Add the formatted amount with the specified color
            cell3.Phrase.Add(new Chunk("$" + importe, new Font(Font.FontFamily.HELVETICA, 12, Font.NORMAL, customColor)));

            infoTable.AddCell(cell3);

            // Add the infoTable as a cell of headerTable
            PdfPCell infoCell = new PdfPCell(infoTable)
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                PaddingTop = 20f,
                VerticalAlignment = Element.ALIGN_BOTTOM
            };
            headerTable.AddCell(infoCell);

            // Configure to occupy the entire page width
            headerTable.TotalWidth = 550; // Page width
            headerTable.LockedWidth = true;

            return headerTable;
        }
        public void GenerarReportePDF(string htmlContent, string encabezado, DateTime fecha_i, DateTime fecha_f, int orientacion_hoja, int tipo_hoja, string[] tamano, int modo, string[] portada_prenomina)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            HtmlNode tableNode = doc.DocumentNode.SelectSingleNode("//table");

            // Configuración para el documento PDF
            //VERTICAL
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
            //HORIZONTAL
            if (orientacion_hoja == 1)
            {
                document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            }
            switch (tipo_hoja)
            {
                case 1: // Tamaño Carta
                    document.SetPageSize(iTextSharp.text.PageSize.LETTER);
                    break;
                case 2: // Tamaño A0
                    document.SetPageSize(iTextSharp.text.PageSize.A0);
                    break;
                case 3: // Tamaño A2
                    document.SetPageSize(iTextSharp.text.PageSize.A2);
                    break;
                case 4: // Tamaño A3
                    document.SetPageSize(iTextSharp.text.PageSize.A3);
                    break;
                case 5: // Tamaño B0
                    document.SetPageSize(iTextSharp.text.PageSize.B0);
                    break;
                case 6: // Tamaño B2
                    document.SetPageSize(iTextSharp.text.PageSize.B2);
                    break;
                case 7: // Tamaño B4
                    document.SetPageSize(iTextSharp.text.PageSize.B4);
                    break;

                default:
                    // Tamano A4
                    break;
            }
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuentes para el título, encabezado y contenido
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.WHITE);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 6);

            // Agregar encabezado de reporte con logo, título y fechas
            if (modo == 2) { document.Add(GenerarEncabezadoReporte(encabezado, titleFont, cellFont, fecha_i, fecha_f)); }
            else
            {

                PdfPTable portadaTable = GenerarEncabezadoNominaReporte(portada_prenomina);

                // Ajustar la posición y el tamaño para ocupar toda la hoja
                portadaTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                portadaTable.LockedWidth = true;

                // Calcular la posición para centrar verticalmente
                float tableHeight = portadaTable.TotalHeight;
                float centerY = (document.PageSize.Height / 2) + (tableHeight / 2);

                // Posiciona la tabla en el centro de la página
                portadaTable.WriteSelectedRows(0, -1, document.LeftMargin, centerY, writer.DirectContent);
                document.NewPage();
            }

            bool primera_tabla = true;
            int totalHeaders = 0;
            int totalHeadersMayor = 0;
            foreach (var rowNode in tableNode.SelectNodes("tr"))
            {
                foreach (var cellNode in rowNode.SelectNodes("th|td"))
                {
                    if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headers"))
                    {
                        totalHeaders++;
                    }
                    if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerMayor"))
                    {
                        totalHeadersMayor++;
                    }
                }
            }
            if (totalHeadersMayor != 0)
            {
                totalHeaders = totalHeaders / totalHeadersMayor;
            }

            PdfPTable currentTable = null;
            int currentHeadersCount = 0;

            foreach (var rowNode in tableNode.SelectNodes("tr"))
            {
                foreach (var cellNode in rowNode.SelectNodes("th|td"))
                {
                    var decodedText = System.Web.HttpUtility.HtmlDecode(cellNode.InnerHtml);

                    if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headerMayor"))
                    {
                        if (modo == 1 && primera_tabla == true) { document.Add(GenerarEncabezadoReporte(encabezado, titleFont, cellFont, fecha_i, fecha_f)); }
                        // Añadimos la tabla previa al documento, si existe
                        if (currentTable != null)
                        {
                            document.Add(currentTable);
                        }

                        // Reinicia la tabla y el contador de headers
                        currentHeadersCount = 0;
                        currentTable = null;

                        // Agrega un salto de página antes de agregar el nuevo `headerMayor`
                        if (primera_tabla != true) { document.NewPage(); document.Add(GenerarEncabezadoReporte(encabezado, titleFont, cellFont, fecha_i, fecha_f)); }
                        else { primera_tabla = false; }

                        // Crear y agregar el `headerMayor` como una tabla independiente
                        PdfPCell tituloCell = new PdfPCell(new Phrase(decodedText, headerFont))
                        {
                            BackgroundColor = new BaseColor(46, 135, 215),
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Padding = 5f
                        };
                        PdfPTable tituloTable = new PdfPTable(1) { WidthPercentage = 100 };
                        tituloTable.AddCell(tituloCell);
                        document.Add(tituloTable);
                    }
                    else if (cellNode.Attributes["class"] != null && cellNode.Attributes["class"].Value.Contains("headers"))
                    {
                        // Si es la primera vez que se encuentra un header, se crea la nueva tabla con el conteo actual de headers
                        if (currentTable == null)
                        {
                            currentHeadersCount++;
                            currentTable = new PdfPTable(totalHeaders) { WidthPercentage = 100 };

                            // Convertir el array de strings `tamano` en un array de floats para `SetWidths`
                            float[] columnWidths = new float[totalHeaders];
                            float totalFixedWidth = 0;
                            int autoColumnCount = 0;

                            for (int i = 0; i < tamano.Length; i++)
                            {
                                if (float.TryParse(tamano[i].Replace("f", ""), out float width))
                                {
                                    columnWidths[i] = width;
                                    totalFixedWidth += width;
                                }
                                else
                                {
                                    autoColumnCount++;
                                }
                            }

                            // Calcular el tamaño automático para columnas que no tienen tamaño especificado
                            float autoWidth = (1 - totalFixedWidth) / autoColumnCount;
                            for (int i = 0; i < columnWidths.Length; i++)
                            {
                                if (columnWidths[i] == 0)
                                {
                                    columnWidths[i] = autoWidth;
                                }
                            }

                            // Establecer los anchos de las columnas en la tabla
                            currentTable.SetWidths(columnWidths);
                        }

                        // Añadir header a la tabla actual
                        PdfPCell headerCell = new PdfPCell(new Phrase(decodedText, headerFont))
                        {
                            BackgroundColor = new BaseColor(52, 58, 64),
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        currentTable.AddCell(headerCell);
                    }
                    else if (currentTable != null)
                    {
                        // Añadir datos a la tabla actual
                        PdfPCell dataCell = new PdfPCell(new Phrase(decodedText, cellFont))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        currentTable.AddCell(dataCell);
                    }
                }
            }

            // Añadir la última tabla al documento si existe
            if (currentTable != null)
            {
                document.Add(currentTable);
            }

            document.Close();
            writer.Close();

            // Enviar PDF al navegador
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Reporte.pdf");
            Response.BinaryWrite(ms.ToArray());
            Response.End();
        }
        public void DescargarFormatoNominaConceptosPDF(int id_nomina_g, int[] id_departamento, int modo)
        {


            string encabezado = "PRENOMINA BETA SANTA MONICA";
            ViewBag.modo = modo;
            if (id_departamento.Contains(0)) { id_departamento = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true).Select(x => x.C_nomina_departamentos.id_departamento_empleado).ToArray(); }

            var valid = db.C_nomina_beta_g.Find(id_nomina_g);
            if (valid.id_nomina_status != 3) { }  //NO ESTÁ CERRADA

            Session["ConceptosNominaFormato"] = db.C_nomina_conceptos.Where(x => x.activo == true && x.formato_nomina == true).ToList();
            var data = db.C_nomina_beta_d_areas.Where(x => x.id_nomina_g == id_nomina_g && x.activo == true && id_departamento.Contains((int)x.id_departamento)).ToList();

            var nomina_g = data.FirstOrDefault().C_nomina_beta_g;
            string fecha_inicial = nomina_g.fecha_inicio.Value.ToShortDateString();
            string fecha_final = nomina_g.fecha_fin.Value.ToShortDateString();
            string fecha_registro = nomina_g.fecha_registro.Value.ToShortDateString();
            string no_empleado = data.Sum(x => x.no_empleados).ToString();
            string generada_por = nomina_g.C_usuarios_corporativo.C_empleados.nombres + " " + nomina_g.C_usuarios_corporativo.C_empleados.apellido_paterno;
            decimal importe_final_nomina = 0;
            foreach (var area in data)
            {
                decimal importe_area = (decimal)area.C_nomina_beta_d_areas_empleados.Sum(x => x.importe_final);
                importe_final_nomina += importe_area;
            }
            string total_departamento = data.Select(x => x.id_departamento).Distinct().Count().ToString();
            string importe_final = importe_final_nomina.ToString();
            string[] portada_prenomina = { fecha_inicial, fecha_final, fecha_registro, no_empleado, generada_por, importe_final, total_departamento };

            DateTime fecha_i = (DateTime)nomina_g.fecha_inicio;
            DateTime fecha_f = (DateTime)nomina_g.fecha_fin;


            string htmlContent = RenderPartialViewToString(data, "../NOMINA/Prenomina/_FormatoNominaConceptosPDF");
            string[] tamano = { "", ".14", ".12", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ".05" };
            GenerarReportePDF(htmlContent, encabezado, fecha_i, fecha_f, 1, 0, tamano, modo, portada_prenomina);
        }

        #endregion



        #region RECIBOS DE NOMINA
        public ActionResult RecibosNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8099)) { return View("/Views/Home/Index.cshtml"); }

                return View("../NOMINA/RecibosNomina/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarNominasMesAnioSelect(int id_anio, int id_mes, int id_frecuencia_pago)
        {
            List<GraficasReportePie> data = new List<GraficasReportePie>();

            DateTime fecha_inicio = new DateTime(id_anio, id_mes, 1);
            DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            string query = "SELECT NOMINA_ID, FECHA_PAGO, TIPO_NOM FROM NOMINAS WHERE FECHA >= '"+ fecha_inicio.ToString("dd.MM.yyyy") + "' AND FECHA <= '"+ fecha_fin.ToString("dd.MM.yyyy") + "' AND FREPAG_ID = "+ id_frecuencia_pago + "";
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();
            var data_nominas = reportes.EjecturarQueryFireBird(query, conn);

            for (int i = 0; i < data_nominas.Count; i++)
            {
                GraficasReportePie obj = new GraficasReportePie();
                obj.ID = (int)data_nominas[i][0];

                DateTime fecha_pago = DateTime.Parse(data_nominas[i][1].ToString());
                string format_sp = fecha_pago.ToString("dddd dd MMM yyyy", new System.Globalization.CultureInfo("es-MX"));

                string tipo_nomina = data_nominas[i][2].ToString();
                if (tipo_nomina == "N"){ tipo_nomina = "Ordinaria"; }
                else if (tipo_nomina == "L") { tipo_nomina = "Liquidación"; }
                else if (tipo_nomina == "E") { tipo_nomina = "Extraordinaria"; }

                obj.Nombre = format_sp.ToUpper() + " ("+ tipo_nomina + ")";
                data.Add(obj);
            }

            return PartialView("RecibosNomina/_NominasGeneradasSelect", data);

        }

        public PartialViewResult ConsultarReciboNominaEmpleado(int id_nomina, int id_empleado)
        {
            int[] param = { 1024, 7, 11, 8, 9 };
            var parametros = db.C_parametros_configuracion.Where(x => param.Contains((int)x.id_parametro_configuracion)).ToList();
            ViewBag.IMSS = parametros.Where(x => x.id_parametro_configuracion == 1024).FirstOrDefault().valor_texto;
            ViewBag.NOMBRE_EMPRESA = parametros.Where(x => x.id_parametro_configuracion == 7).FirstOrDefault().valor_texto;
            ViewBag.RFC = parametros.Where(x => x.id_parametro_configuracion == 11).FirstOrDefault().valor_texto;

            ViewBag.DIRECCION = parametros.Where(x => x.id_parametro_configuracion == 8).FirstOrDefault().valor_texto + " " + parametros.Where(x => x.id_parametro_configuracion == 9).FirstOrDefault().valor_texto;

            List<ReciboNominaEmpleado> data = new List<ReciboNominaEmpleado>();
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();
            string query = "SELECT PAGOS_NOMINA_DET.PAGO_NOMINA_DET_ID, PAGOS_NOMINA_DET.PAGO_NOMINA_ID, EMPLEADOS.NOMBRE_COMPLETO, PUESTOS_NO.NOMBRE, DEPTOS_NO.NOMBRE, " +
                " EMPLEADOS.RFC, EMPLEADOS.CURP, FRECUENCIAS_PAGO.NOMBRE, NOMINAS.FECHA_PAGO, EMPLEADOS.REG_IMSS, PAGOS_NOMINA.TIPO_SALARIO, EMPLEADOS.SALARIO_INTEG, PAGOS_NOMINA.JORNADA, " +
                " EMPLEADOS.FECHA_INGRESO, CONCEPTOS_NO.NATURALEZA, CONCEPTOS_NO.NOMBRE, PAGOS_NOMINA_DET.IMPORTE_GRAVABLE, PAGOS_NOMINA_DET.UNIDADES, " +
                " PAGOS_NOMINA.TOTAL_PERCEP, PAGOS_NOMINA.TOTAL_RETEN, NOMINAS.FECHA_INICIAL, NOMINAS.FECHA_FINAL, PAGOS_NOMINA.FORMA_PAGO, PAGOS_NOMINA_DET.IMPORTE_EXENTO " +
                " FROM PAGOS_NOMINA" +
                " JOIN PAGOS_NOMINA_DET on PAGOS_NOMINA.PAGO_NOMINA_ID = PAGOS_NOMINA_DET.PAGO_NOMINA_ID" +
                " JOIN CONCEPTOS_NO on PAGOS_NOMINA_DET.CONCEPTO_NO_ID = CONCEPTOS_NO.CONCEPTO_NO_ID " +
                " JOIN EMPLEADOS on PAGOS_NOMINA.EMPLEADO_ID = EMPLEADOS.EMPLEADO_ID" +
                " JOIN PUESTOS_NO on EMPLEADOS.PUESTO_NO_ID = PUESTOS_NO.PUESTO_NO_ID" +
                " JOIN DEPTOS_NO ON EMPLEADOS.DEPTO_NO_ID = DEPTOS_NO.DEPTO_NO_ID" +
                " JOIN NOMINAS ON PAGOS_NOMINA.NOMINA_ID = NOMINAS.NOMINA_ID" +
                " JOIN FRECUENCIAS_PAGO ON NOMINAS.FREPAG_ID = FRECUENCIAS_PAGO.FREPAG_ID" +
                " WHERE PAGOS_NOMINA.NOMINA_ID = " + id_nomina + " AND PAGOS_NOMINA.EMPLEADO_ID = " + id_empleado + " ";
            var data_recibo = reportes.EjecturarQueryFireBird(query, conn);
            if (data_recibo.Count() == 0) { return null; }
            for (int i = 0; i < data_recibo.Count; i++)
            {
                try
                {
                    decimal importe_final = 0;
                    decimal importe_exento = 0;
                    decimal importe_gravable = 0;
                    if (data_recibo[i][23] != null && data_recibo[i][23].ToString() != "")
                    {
                        importe_gravable = Convert.ToDecimal(data_recibo[i][16].ToString());  //-- DE LEY
                        importe_exento = Convert.ToDecimal(data_recibo[i][23].ToString());  //DESPENSAS, ETC
                        //if (importe_gravable == 0) { importe_final = importe_exento; }
                        //else { importe_final = importe_gravable; } //-- TOTAL A PAGAR
                        if (importe_exento > 0)
                        {
                            importe_gravable += importe_exento;
                        }
                        importe_final = importe_gravable;
                    }

                    ReciboNominaEmpleado obj = new ReciboNominaEmpleado();
                    obj.id_nomina_det = (int)data_recibo[i][0];
                    obj.id_nomina = (int)data_recibo[i][1];
                    obj.nombre = data_recibo[i][2].ToString();
                    obj.puesto = data_recibo[i][3].ToString();
                    obj.departamento = data_recibo[i][4].ToString();
                    obj.RFC = data_recibo[i][5].ToString();
                    obj.CURP = data_recibo[i][6].ToString();
                    obj.frec_pago = data_recibo[i][7].ToString();

                    DateTime fecha_pago = DateTime.MinValue;
                    DateTime fecha_ingreso = DateTime.MinValue;
                    DateTime fecha_inicio = DateTime.MinValue;
                    DateTime fecha_fin = DateTime.MinValue;
                    try
                    {
                        fecha_ingreso = DateTime.Parse(data_recibo[i][13].ToString());
                        fecha_pago = DateTime.Parse(data_recibo[i][8].ToString());
                        fecha_inicio = DateTime.Parse(data_recibo[i][20].ToString());
                        fecha_fin = DateTime.Parse(data_recibo[i][21].ToString());

                        DayOfWeek diaSemana = fecha_pago.DayOfWeek;
                        int diferenciaLunes = ((int)diaSemana - (int)DayOfWeek.Monday + 7) % 7;
                        DateTime lunes = fecha_pago.AddDays(-diferenciaLunes);

                        DateTime domingo = lunes.AddDays(6);

                        obj.fecha_periodo = lunes.ToString("dd/MM/yyyy") + " AL " + domingo.ToString("dd/MM/yyyy");

                        Console.WriteLine("Fecha: " + fecha_pago.ToShortDateString());
                        Console.WriteLine("Lunes: " + lunes.ToShortDateString());
                        Console.WriteLine("Domingo: " + domingo.ToShortDateString());

                    }
                    catch (Exception)
                    {
                    }

                    obj.fecha_pago = fecha_pago.AddDays(1).ToString("dd/MM/yyyy");
                    obj.registro_nss = data_recibo[i][9].ToString();
                    int tipo_salario = Convert.ToInt32(data_recibo[i][10].ToString());
                    if (tipo_salario == 0)
                    {
                        obj.tipo_salario = "Fijo";
                    }
                    if (tipo_salario == 1)
                    {
                        obj.tipo_salario = "Variable";
                    }
                    if (tipo_salario == 2)
                    {
                        obj.tipo_salario = "Mixto";
                    }

                    obj.salario_integrado = Convert.ToDecimal(data_recibo[i][11].ToString());
                    obj.jornada = data_recibo[i][12].ToString();
                    obj.fecha_ingreso = fecha_ingreso.ToString("dd/MM/yyyy");
                    obj.naturaleza_concepto = data_recibo[i][14].ToString().Trim();
                    obj.concepto = data_recibo[i][15].ToString();
                    obj.monto = importe_final;
                    obj.unidades = Convert.ToDecimal(data_recibo[i][17].ToString());
                    obj.total_percepciones = Convert.ToDecimal(data_recibo[i][18].ToString());
                    obj.total_retenciones = Convert.ToDecimal(data_recibo[i][19].ToString());
                    string forma_pago = data_recibo[i][22].ToString().Trim();
                    if (forma_pago == "T") { forma_pago = "Transferencia"; }
                    else { forma_pago = "Efectivo"; }
                    obj.forma_pago = forma_pago;

                    obj.fecha_inicio = fecha_inicio.ToString("dd/MM/yyyy");
                    obj.fecha_fin = fecha_fin.ToString("dd/MM/yyyy");
                    obj.UUID = "";
                    data.Add(obj);
                }
                catch (Exception ex)
                {
                    string hola = ex.ToString();
                }
            }

            string XML = "";
            if (data.Count() > 0)
            {
                var info_recibo = data.FirstOrDefault();
                string query_cfdi = "SELECT XML, FECHA_HORA_TIMBRADO, UUID, CFDI_ID FROM USOS_FOLIOS_FISCALES WHERE DOCTO_ID = " + info_recibo.id_nomina + "";
                using (var command = new FbCommand(query_cfdi, conn))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Obtener el stream del BLOB directamente
                        using (Stream blobStream = reader.GetStream(0)) // 0 = índice de columna
                        using (StreamReader sr = new StreamReader(blobStream)) // se asume que es texto
                        {
                            XML = sr.ReadToEnd();
                        }

                        data.ForEach(x => x.XML = XML);
                        //DateTime fecha_certificacion = DateTime.MinValue;
                        //try { fecha_certificacion = DateTime.Parse(reader.GetString(1).ToString()); } catch (Exception) { }

                        int id_cfdi = Convert.ToInt32(reader.GetString(3).ToString());
                        data.ForEach(x => x.fecha_certificacion = reader.GetString(1).ToString());
                        data.ForEach(x => x.fecha_emision_sat = reader.GetString(1).ToString());
                        data.ForEach(x => x.UUID = reader.GetString(2).ToString());
                        data.ForEach(x => x.id_cfdi = id_cfdi);
                        
                        string query_fiscal = "SELECT RFC, LUGAR_EXPEDICION, FOLIO, XML  FROM REPOSITORIO_CFDI where CFDI_ID = " + id_cfdi + "";
                        var data_fiscal = reportes.EjecturarQueryFireBird(query_fiscal, conn);
                        for (int d = 0; d < data_fiscal.Count; d++)
                        {
                            data.ForEach(x => x.lugar_sat = data_fiscal[d][1].ToString());
                            data.ForEach(x => x.folio_sat = data_fiscal[d][2].ToString());
                        }

                        if (XML != "")
                        {
                            string sello = Regex.Match(XML, @"Sello\s*=\s*""([^""]+)""").Groups[1].Value;
                            string certificado = Regex.Match(XML, @"Certificado\s*=\s*""([^""]+)""").Groups[1].Value;
                            string RfcProv = Regex.Match(XML, @"RfcProvCertif\s*=\s*""([^""]+)""").Groups[1].Value;
                            string SelloCFD = Regex.Match(XML, @"SelloCFD\s*=\s*""([^""]+)""").Groups[1].Value;
                            string SelloSAT = Regex.Match(XML, @"SelloSAT\s*=\s*""([^""]+)""").Groups[1].Value;
                            string NoCertificadoSAT = Regex.Match(XML, @"NoCertificadoSAT\s*=\s*""([^""]+)""").Groups[1].Value;

                            data.ForEach(x => x.certificado_cfdi = certificado);
                            data.ForEach(x => x.sello_cfdi = sello);
                            data.ForEach(x => x.noCertificado_cfdi = NoCertificadoSAT);

                            string cadena_sat = "||1.1|" + reader.GetString(2) + "|" + reader.GetString(1) + "|" + RfcProv + "|" + SelloCFD;
                            data.ForEach(x => x.cadena_cfdi = cadena_sat);
                            data.ForEach(x => x.sello_digital_cfdi = SelloCFD);
                            data.ForEach(x => x.sello_digital_sat = SelloSAT);

                            BarcodeWriter barcodeWriter = new BarcodeWriter()
                            {
                                Format = BarcodeFormat.QR_CODE,
                                Options = new ZXing.Common.EncodingOptions
                                {
                                    Width = 500,
                                    Height = 350,
                                    Margin = 1
                                }
                            };
                            Bitmap codigo_qr = barcodeWriter.Write("https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx?");
                            using (var ms = new MemoryStream())
                            {
                                codigo_qr.Save(ms, ImageFormat.Png);
                                byte[] imageBytes = ms.ToArray();
                                string base64String = Convert.ToBase64String(imageBytes);
                                data.ForEach(x => x.codigo_qr = $"data:image/png;base64,{base64String}");
                            }
                        }

                    }
                }
            }

            return PartialView("RecibosNomina/_ReciboNominaEmpleado", data);

        }

        #endregion



    }
}