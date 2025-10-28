using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Beta_System.Models;
using ClosedXML.Excel;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using WebGrease.Css.Extensions;


namespace Beta_System.Controllers
{
    public class EVALUACIONESController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private CATALOGOSController catalogos = new CATALOGOSController();
        private NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();


        #region ADMINISTRACION DEPARTAMENTOS

        public ActionResult AdministracionDepartamentos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8079)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("AdministracionDepartamentos/Index");
        }


        //------------CRUD DE DEPARTAMENTOS
        public PartialViewResult ConsultarDepartamentosEvaluacionTable()
        {
            var data = db.C_evaluaciones_departamentos.OrderBy(x => x.nombre_departamento).ToList();
            return PartialView("AdministracionDepartamentos/_DepartamentodTable", data);
        }
        public int RegistrarActualizarDepartamentoEvaluacion(C_evaluaciones_departamentos departamento)
        {
            try
            {
                int id_departamento = departamento.id_evaluacion_departamento;
                if (id_departamento == 0)
                {
                    departamento.activo = true;
                    db.C_evaluaciones_departamentos.Add(departamento);
                    db.SaveChanges();
                    return 1;
                }
                else
                {
                    var depto = db.C_evaluaciones_departamentos.Find(id_departamento);
                    depto.nombre_departamento = departamento.nombre_departamento;
                    db.SaveChanges();
                    return 2;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool OnOffDepartamento(int id_departamento, bool status)
        {
            try
            {
                var data = db.C_evaluaciones_departamentos.Find(id_departamento).activo = status;
                db.SaveChanges();
                return true;    
            }
            catch (Exception)
            {
                return false;
            }
        }



        //------------ASOCIACION DE DEPARTAMENTOS - EMPLEADOS
        public PartialViewResult ConsultarDepartamentosEmpleadosTable(int id_departamento)
        {
            var data = db.C_evaluaciones_departamentos_empleados.Where(x => x.activo == true && x.id_evaluacion_departamento == id_departamento).ToList();
            return PartialView("AdministracionDepartamentos/_DepartamentosEmpleadosTable", data);
        }
        public PartialViewResult ConsultarEmpleadosSinDepartamento()
        {
            List<C_nomina_empleados> empleados;
            int[] id_empleados_ligados = null;
            var empleados_ligados = db.C_evaluaciones_departamentos_empleados.Where(x => x.activo == true).ToList();
            if (empleados_ligados.Count() > 0)
            {
                id_empleados_ligados = empleados_ligados.Select(x => (int)x.id_empleado).ToArray();
                empleados = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.Frepag_id == 410 && !id_empleados_ligados.Contains(x.Empleado_id)).ToList();
            }
            else
            {
                empleados = db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.Frepag_id == 410).ToList();
            }
            return PartialView("AdministracionDepartamentos/_EmpleadosSinDepartamentoCheckTable", empleados);
        }
        public int AsociarDepartamentoEmpleados(int id_departamento, int[] id_empleados)
        {
            try
            {
                for (int i = 0; i < id_empleados.Length; i++)
                {
                    int id_empleado = id_empleados[i];
                    var valid = db.C_evaluaciones_departamentos_empleados.Where(x => x.id_empleado == id_empleado).FirstOrDefault();
                    if (valid != null)
                    {
                        valid.id_evaluacion_departamento = id_departamento;
                        valid.activo = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        C_evaluaciones_departamentos_empleados new_liga = new C_evaluaciones_departamentos_empleados();
                        new_liga.id_evaluacion_departamento = id_departamento;
                        new_liga.id_empleado = id_empleado;
                        new_liga.activo = true;
                        db.C_evaluaciones_departamentos_empleados.Add(new_liga);
                        db.SaveChanges();
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public bool EliminarLigaEmpleadoDepartamento(int id_departamento_empleado)
        {
            try
            {
                db.C_evaluaciones_departamentos_empleados.Find(id_departamento_empleado).activo = false;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        //------------ASOCIACION DE DEPARTAMENTOS - USUARIOS
        public PartialViewResult ConsultarDepartamentosUsuariosTable(int id_departamento)
        {
            var data = db.C_evaluaciones_departamentos_usuarios.Where(x => x.activo == true && x.id_evaluacion_departamento == id_departamento).ToList();
            return PartialView("AdministracionDepartamentos/_DepartamentosUsuariosTable", data);
        }
        public PartialViewResult ConsultarUsuariosCheckTable()
        {
            var empleados = db.C_usuarios_corporativo.Where(x => x.Activo == true).ToList();
            return PartialView("AdministracionDepartamentos/_UsuariosCheckTable", empleados);
        }
        public int AsociarDepartamentoUsuarios(int id_departamento, int[] id_usuarios)
        {
            try
            {
                for (int i = 0; i < id_usuarios.Length; i++)
                {
                    int id_usuario = id_usuarios[i];
                    var valid = db.C_evaluaciones_departamentos_usuarios.Where(x => x.id_usuario == id_usuario && x.id_evaluacion_departamento == id_departamento).FirstOrDefault();
                    if (valid != null)
                    {
                        valid.id_evaluacion_departamento = id_departamento;
                        valid.activo = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        C_evaluaciones_departamentos_usuarios new_liga = new C_evaluaciones_departamentos_usuarios();
                        new_liga.id_evaluacion_departamento = id_departamento;
                        new_liga.id_usuario = id_usuario;
                        new_liga.activo = true;
                        db.C_evaluaciones_departamentos_usuarios.Add(new_liga);
                        db.SaveChanges();
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public bool EliminarLigaUsuarioDepartamento(int id_liga_departamento_usuario)
        {
            try
            {
                db.C_evaluaciones_departamentos_usuarios.Find(id_liga_departamento_usuario).activo = false;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion


        #region ADMINISTRACION RUBROS

        //------------ADMINISTRACIÓN DE RUBROS
        public ActionResult AdministracionRubros()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8080)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("AdministracionRubros/Index");
        }

        public PartialViewResult ConsultarRubrosEvaluacionCards()
        {
            var data = db.C_evaluaciones_rubros.OrderBy(x => x.nombre_rubro).ToList();
            return PartialView("AdministracionRubros/_RubrosConceptosCards", data);
        }

        public string ConsultarConceptosRubroArray(int id_rubro)
        {
            var data = from conceptos in db.C_evaluaciones_conceptos_rubros
                       where conceptos.id_evaluacion_rubro == id_rubro && conceptos.activo == true
                       select new { conceptos.nombre_concepto, conceptos.porcentaje, conceptos.id_evaluacion_concepto_rubro };
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public int RegistrarActualizarRubroEvaluacion(C_evaluaciones_rubros rubro, string[] conceptos, decimal[] porcentajes)
        {
            try
            {
                int id_rubro = rubro.id_evaluacion_rubro;
                if (id_rubro == 0)
                {
                    rubro.nombre_rubro = rubro.nombre_rubro.ToUpper();
                    rubro.activo = true;
                    db.C_evaluaciones_rubros.Add(rubro);
                    db.SaveChanges();
                    id_rubro = rubro.id_evaluacion_rubro;
                    for (int i = 0; i < conceptos.Length; i++)
                    {
                        C_evaluaciones_conceptos_rubros concepto = new C_evaluaciones_conceptos_rubros();
                        concepto.id_evaluacion_rubro = id_rubro;
                        concepto.nombre_concepto = Regex.Replace(conceptos[i].ToUpper(), @"[^a-zA-ZñÑ\s]", string.Empty);
                        concepto.porcentaje = porcentajes[i];
                        concepto.activo = true;
                        db.C_evaluaciones_conceptos_rubros.Add(concepto);
                    }
                    db.SaveChanges();
                    return 1;
                }
                else
                {
                    var rubro_edit = db.C_evaluaciones_rubros.Find(id_rubro);
                    rubro_edit.nombre_rubro = rubro.nombre_rubro.ToUpper();
                    rubro_edit.porcentaje = rubro.porcentaje;

                    db.C_evaluaciones_conceptos_rubros.Where(x => x.id_evaluacion_rubro == id_rubro).ToList().ForEach(x => x.activo = false);
                    db.SaveChanges();

                    for (int i = 0; i < conceptos.Length; i++)
                    {
                        string concepto_mayu = Regex.Replace(conceptos[i].ToUpper(), @"[^a-zA-ZñÑ\s]", string.Empty);

                        var valid = db.C_evaluaciones_conceptos_rubros.Where(x => x.id_evaluacion_rubro == id_rubro && x.nombre_concepto == concepto_mayu ).FirstOrDefault();
                        if (valid == null)
                        {
                            C_evaluaciones_conceptos_rubros concepto = new C_evaluaciones_conceptos_rubros();
                            concepto.id_evaluacion_rubro = id_rubro;
                            concepto.nombre_concepto = concepto_mayu;
                            concepto.porcentaje = porcentajes[i];
                            concepto.activo = true;
                            db.C_evaluaciones_conceptos_rubros.Add(concepto);
                            db.SaveChanges();
                        }
                        else
                        {
                            valid.porcentaje = porcentajes[i];
                            valid.activo = true;
                            db.SaveChanges();
                        }
                    }
                    return 2;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool OnOffRubro(int id_rubro, bool status)
        {
            try
            {
                db.C_evaluaciones_rubros.Find(id_rubro).activo = status;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        //------------ASOCIACION DE RUBROS A DEPARTAMENTOS
        public PartialViewResult ConsultarDepartamentosRubrosCards()
        {
            var data = db.C_evaluaciones_departamentos.Where(x => x.activo == true).ToList();
            return PartialView("AdministracionRubros/_DepartamentosRubrosCards", data);
        }

        public PartialViewResult ConsultarRubrosDepartamentoCard(int id_depa)
        {
            Session["rubros_departamentos_evaluaciones"] = db.C_evaluaciones_departamentos_rubros.Where(x => x.id_evaluacion_departamento == id_depa && x.activo == true).ToList();
            var data = db.C_evaluaciones_rubros.Where(x => x.activo == true).ToList();
            return PartialView("AdministracionRubros/_RubrosDepartamentoCard", data);
        }

        public int GuardarRubrosDepartamento(int id_departamento, int[] id_rubros)
        {
            try
            {
                db.C_evaluaciones_departamentos_rubros.Where(x => x.id_evaluacion_departamento == id_departamento).ToList().ForEach(x => x.activo = false);
                db.SaveChanges();
                for (int i = 0; i < id_rubros.Length; i++)
                {
                    int id_rubro = id_rubros[i];
                    var valid = db.C_evaluaciones_departamentos_rubros.Where(x => x.id_evaluacion_departamento == id_departamento && x.id_evaluacion_rubro == id_rubro).FirstOrDefault();
                    if (valid == null)
                    {
                        C_evaluaciones_departamentos_rubros depa_rubro = new C_evaluaciones_departamentos_rubros();
                        depa_rubro.id_evaluacion_departamento = id_departamento;
                        depa_rubro.id_evaluacion_rubro = id_rubro;
                        depa_rubro.activo = true;
                        db.C_evaluaciones_departamentos_rubros.Add(depa_rubro);
                        db.SaveChanges();
                    }
                    else
                    {
                        valid.activo = true;
                        db.SaveChanges();
                    }
                }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        #endregion


        #region ADMINISTRACION DE EVALUACIONES
        //-------------GENERACIÓN DE PLANTILLAS
        public ActionResult AdministrarEvaluaciones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8083)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("AdministracionEvaluaciones/Index");
        }

        public PartialViewResult ConsultarEvaluacionesSelect(int modo)
        {
            //List<C_evaluaciones_g> data;
            //if (fecha_fin == "" || fecha_inicio == "")
            //{
            //    data = db.C_evaluaciones_g.Where(x => x.activo == true && x.abierta_cerrada == true).ToList();
            //}
            //else
            //{
            //    DateTime fecha_i = DateTime.Parse(fecha_inicio);
            //    DateTime fecha_f = DateTime.Parse(fecha_fin);
            //    data = db.C_evaluaciones_g.Where(x => x.activo == true && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f).ToList();
            //}


            List<C_evaluaciones_g> data = null;
            if (modo == 1)  //SOLO ABIERTAS
            {
                data = db.C_evaluaciones_g.Where(x => x.activo == true && x.abierta_cerrada == true).ToList();
            }
            else if (modo == 2)  //SOLO CERRADAS
            {
                data = db.C_evaluaciones_g.Where(x => x.activo == true && x.abierta_cerrada == false).ToList();
            }
            else
            {
                data = db.C_evaluaciones_g.Where(x => x.activo == true).ToList();
            }
            return PartialView("AdministracionEvaluaciones/_EvaluacionesSelect", data);
        }

        public PartialViewResult ConsultarDepartamentosUsuariosSelect()
        {
            int id_usuario = (int)Session["LoggedId"];
            var depas = db.C_evaluaciones_departamentos_usuarios.Where(x => x.id_usuario == id_usuario && x.activo == true && x.C_evaluaciones_departamentos.activo == true).Select(x => x.C_evaluaciones_departamentos).Distinct().ToList();
            return PartialView("_DepartamentosSelect", depas);
        }

        public PartialViewResult ConsultarEvaluacionesPendientes(int id_evaluacion_g, int id_departamento)
        {
            var data = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.activo == true && x.id_evaluacion_status != 4 && x.id_departamento == id_departamento).OrderBy(x => x.C_nomina_empleados.Apellido_paterno).ToList();
            return PartialView("AdministracionEvaluaciones/_EvaluacionesPendientesTable", data);
        }

        public int GenerarAperturaEvaluaciones(string descripcion, string fecha_apertura, string fecha_cierre)
        {
            DateTime hoy = DateTime.Now;
            int id_usuario = (int)Session["LoggedId"];
            int count_catch = 0;
            int id_anio = 0;
            C_presupuestos_anios anio;

            var valid_evaluaciones = db.C_evaluaciones_g.Where(x => x.activo == true && x.abierta_cerrada == true).FirstOrDefault();
            if (valid_evaluaciones != null) { return -1; }

            try {
                anio = db.C_presupuestos_anios.Where(x => x.anio == hoy.Year.ToString()).FirstOrDefault();
                id_anio = anio.id_anio_presupuesto; }
            catch (Exception) { return -2; }  //NO HAY UN AÑO REGISTRADO

            var valid_empleados = db.C_evaluaciones_departamentos_empleados.Where(x => x.activo == true && x.C_evaluaciones_departamentos.activo == true).ToList();
            if (valid_empleados.Count() == 0)
            {
                return -3; //NO HAY EMPLEADOS ASIGNADOS A DEPARTAMENTOS
            }

            var valid_rubros = db.C_evaluaciones_departamentos_rubros.Where(x => x.activo == true).ToList();
            if (valid_rubros.Count() == 0)
            {
                return -4; //NO HAY CONFIGURACIÓN DE RUBROS A DEPARTAMENTOS
            }

            DateTime fecha_inicio = DateTime.Parse(fecha_apertura);
            DateTime fecha_final = DateTime.Parse(fecha_cierre);

            if (fecha_inicio >= fecha_final) { return -5; }  //LA FECHA FINAL NO PUEDE SER MAYOR A LA APERTURA
            //if (fecha_inicio.Year.ToString() != anio.anio || fecha_final.Year.ToString() != anio.anio) { return -6; }  //LAS FECHAS DE APERTURA Y CIERRE ESTÁN FUERA DE RANGO (DEBEN SER DEL AÑO ACTUAL) 

            C_evaluaciones_g evaluacion_g = new C_evaluaciones_g();
            evaluacion_g.descripcion = descripcion.ToUpper();
            evaluacion_g.fecha_registro = hoy;
            evaluacion_g.id_usuario_registra = id_usuario;
            evaluacion_g.id_anio = id_anio;
            evaluacion_g.abierta_cerrada = true;
            evaluacion_g.fecha_apertura = fecha_inicio;
            evaluacion_g.fecha_cierre = fecha_final;
            evaluacion_g.activo = true;
            db.C_evaluaciones_g.Add(evaluacion_g);
            db.SaveChanges();
            int id_evaluacion_g = (int)evaluacion_g.id_evaluacion_g;

            //var rubros_depa = db.C_evaluaciones_departamentos_rubros.Where(x => x.C_evaluaciones_departamentos.activo == true && x.activo == true).ToList();
            foreach (var depa in valid_empleados.GroupBy(x => x.id_evaluacion_departamento))
            {
                try
                {
                    int id_departamento = (int)depa.FirstOrDefault().id_evaluacion_departamento;
                    foreach (var empleados in depa)
                    {
                        int id_empleado = (int)empleados.id_empleado;
                        C_evaluaciones_d_empleados evaluacion_empleado = new C_evaluaciones_d_empleados();
                        evaluacion_empleado.id_evaluacion_g = id_evaluacion_g;
                        evaluacion_empleado.id_departamento = id_departamento;
                        evaluacion_empleado.id_empleado = id_empleado;
                        evaluacion_empleado.activo = true;
                        evaluacion_empleado.id_evaluacion_status = 1;
                        db.C_evaluaciones_d_empleados.Add(evaluacion_empleado);
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    count_catch++;
                }
            }

            string asunto = "APERTURA DE EVALUACIONES: " + evaluacion_g.descripcion;
            string destinatarios = "";
            var correos_masters = db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == 2008 && x.activo == true);
            try
            {
                var correo_master = correos_masters.Select(x => x.C_usuarios_corporativo.C_empleados.correo).ToArray();
                destinatarios += correo_master[0] + ";";
                for (int i = 1; i < correo_master.Length; i++) { destinatarios += correo_master[i] + ";"; }
            }
            catch (Exception){}

            var id_usuarios_masters = correos_masters.Select(x => x.id_usuario_corporativo);
            var evaluadores = db.C_evaluaciones_departamentos_usuarios.Where(x => x.activo == true && !id_usuarios_masters.Contains(x.id_usuario)).Select(x => x.C_usuarios_corporativo.C_empleados.correo).Distinct().ToArray();
            foreach (var correo_evaluador in evaluadores)
            {
                destinatarios += correo_evaluador + ";";
            }

            string mensaje = "<label>El usuario: " + (string)Session["LoggedUser"] + " aperturó las evaluaciones semestrales con fechas del: <strong>" + string.Format(evaluacion_g.fecha_apertura.Value.ToShortDateString(), "dd/MM/yyyy") + " AL "+ string.Format(evaluacion_g.fecha_cierre.Value.ToShortDateString(), "dd/MM/yyyy") + "</strong></label><br />" +
                   
                   "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
            notificaciones.EnviarCorreoUsuarioReportes(asunto, destinatarios, mensaje);


            return 0;
        }

        public PartialViewResult ConsultarRubrosEvaluacionEmpleado(int id_evaluacion_g, int id_evaluacion_d_empleado)
        {
            try
            {
                var data_evaluacion = db.C_evaluaciones_d_empleados.Find(id_evaluacion_d_empleado);
                if (data_evaluacion.id_evaluacion_status == 3) { return null; }  //EVALUADO

                ViewBag.nombre = data_evaluacion.C_nomina_empleados.Nombres;
                ViewBag.apellido_p = data_evaluacion.C_nomina_empleados.Apellido_paterno;
                ViewBag.apellido_m = data_evaluacion.C_nomina_empleados.Apellido_materno;

                ViewBag.id_evaluacion_g = data_evaluacion.id_evaluacion_g;
                ViewBag.id_evaluacion_d_empleado = id_evaluacion_d_empleado;
                ViewBag.id_evaluacion_status = data_evaluacion.id_evaluacion_status;
                ViewBag.tipo_evaluacion = "EVALUACIÓN GERENCIAL";

                int id_usuario = (int)Session["LoggedId"];
                var valid_aut_final = db.C_usuarios_masters.Where(x => x.id_usuario_corporativo == id_usuario && x.id_usuario_master_accion == 2008 && x.activo == true).FirstOrDefault();
                if (valid_aut_final != null) {  
                    ViewBag.id_evaluacion_status = 2;   //EVALUADA
                    ViewBag.tipo_evaluacion = "EVALUACIÓN POR DIRECCIÓN"; 
                }  
                var valid_evaluacion = db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.activo == true
                                        //&& x.id_usuario_registra == id_usuario
                                        ).ToList();
                if (valid_evaluacion.Count() > 0)
                {
                    ViewBag.id_usuario = id_usuario;
                    ViewBag.nombre_usuario = Session["LoggedUser"];
                    return PartialView("AdministracionEvaluaciones/_EvaluacionEmpleadoCapturada", valid_evaluacion);
                }
                else
                {
                    var evaluacion_empleado = db.C_evaluaciones_d_empleados.Find(id_evaluacion_d_empleado);
                    int id_depa = (int)evaluacion_empleado.id_departamento;
                    var rubros_depa = db.C_evaluaciones_departamentos_rubros.Where(x => x.id_evaluacion_departamento == id_depa && x.activo == true).Select(x => x.id_evaluacion_rubro).ToArray();
                    var conceptos_rubos = db.C_evaluaciones_conceptos_rubros.Where(x => rubros_depa.Contains((int)x.id_evaluacion_rubro) && x.activo == true).ToList();

                    return PartialView("AdministracionEvaluaciones/_EvaluacionEmpleadoNuevaCaptura", conceptos_rubos);
                }
            }
            catch (Exception)
            {
                return PartialView("AdministracionEvaluaciones/_EvaluacionEmpleadoNuevaCaptura", null);
            }
        }

        public int GuardarEvaluacionEmpleado(int id_evaluacion_d_empleado, int id_evaluacion_g, int id_status, int[] id_conceptos, decimal[] calificaciones, string[] comentarios, decimal[] porcentajes)
        {
            int id_usuario = (int)Session["LoggedId"];

            try
            {
                DateTime hoy = DateTime.Now;
                for (int i = 0; i < id_conceptos.Length; i++)
                {
                    int id_concepto = id_conceptos[i];
                    var valid_concepto = db.C_evaluaciones_d_conceptos.Where(x => x.id_usuario_registra == id_usuario && x.activo == true && x.id_evaluacion_concepto_rubro == id_concepto 
                                        && x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_evaluacion_g == id_evaluacion_g).FirstOrDefault();
                    if (valid_concepto != null)
                    {
                        valid_concepto.porcentaje_calificacion = porcentajes[i];
                        valid_concepto.calificacion = calificaciones[i];
                        valid_concepto.comentarios = comentarios[i];
                        valid_concepto.id_evaluacion_status = id_status;
                        valid_concepto.fecha_registro = hoy;
                        db.SaveChanges();
                    }
                    else
                    {
                        C_evaluaciones_d_conceptos concepto_empleado = new C_evaluaciones_d_conceptos();
                        concepto_empleado.id_evaluacion_d_empleado = id_evaluacion_d_empleado;
                        concepto_empleado.id_evaluacion_g = id_evaluacion_g;
                        concepto_empleado.id_evaluacion_concepto_rubro = id_concepto;
                        concepto_empleado.fecha_registro = hoy;
                        concepto_empleado.calificacion = calificaciones[i];
                        concepto_empleado.porcentaje_calificacion = porcentajes[i];
                        concepto_empleado.id_evaluacion_status = id_status;
                        concepto_empleado.id_usuario_registra = id_usuario;
                        concepto_empleado.comentarios = comentarios[i];
                        concepto_empleado.activo = true;
                        db.C_evaluaciones_d_conceptos.Add(concepto_empleado);
                        db.SaveChanges();
                    }
                }

                decimal calificacion_total = 0;

                var conceptos = db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_usuario_registra == id_usuario && x.activo == true)
                    .GroupBy(x => x.C_evaluaciones_conceptos_rubros.C_evaluaciones_rubros.id_evaluacion_rubro).ToList();
                foreach (var rubro in conceptos)
                {
                    var id_concepto = (int)rubro.FirstOrDefault().id_evaluacion_concepto_rubro;
                    decimal valor_rubro = (decimal)db.C_evaluaciones_conceptos_rubros.Find(id_concepto).C_evaluaciones_rubros.porcentaje;
                    decimal valor_calificacion = (decimal)rubro.Sum(x => x.porcentaje_calificacion);

                    calificacion_total += (valor_rubro * valor_calificacion) / 100;
                }

                calificacion_total = Math.Round(calificacion_total);
                var calificacion = db.C_evaluaciones_d_calificaciones.Where(x => x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_usuario_califica == id_usuario && x.activo == true).FirstOrDefault();
                if (calificacion != null)
                {
                    calificacion.calificacion_final = calificacion_total;
                    calificacion.tipo_calificacion = false;
                    if (id_status == 3) { calificacion.tipo_calificacion = true; }
                    db.SaveChanges();
                }
                else
                {
                    C_evaluaciones_d_calificaciones new_calificacion = new C_evaluaciones_d_calificaciones();
                    new_calificacion.id_evaluacion_d_empleado = id_evaluacion_d_empleado;
                    new_calificacion.id_evaluacion_g = id_evaluacion_g;
                    new_calificacion.fecha_calificacion = DateTime.Now;
                    new_calificacion.calificacion_final = calificacion_total;
                    new_calificacion.id_usuario_califica = id_usuario;
                    new_calificacion.activo = true;
                    new_calificacion.tipo_calificacion = false; //NORMAL
                    if (id_status == 3) { new_calificacion.tipo_calificacion = true; }
                    db.C_evaluaciones_d_calificaciones.Add(new_calificacion);
                    db.SaveChanges();
                }

                if (id_status == 3)  //EVALUADA
                {
                    var evaluacion_empleado = db.C_evaluaciones_d_empleados.Find(id_evaluacion_d_empleado);
                    evaluacion_empleado.id_evaluacion_status = id_status;
                    evaluacion_empleado.calificacion_final = calificacion_total;
                    evaluacion_empleado.id_usuario_califica = id_usuario;
                    db.SaveChanges();
                }

                return 0;
            }
            catch (Exception)
            {
                db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_evaluacion_g == id_evaluacion_g && x.id_usuario_registra == id_usuario && x.activo == true).ToList().ForEach(x => x.activo = false);
                db.SaveChanges();
                return -1;
            }

        }

        public int GuardarEvaluacionComentarios(int id_evaluacion_g, int id_evaluacion_d_empleado, string fortalezas, string areas_oportunidad)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var valid_comentarios = db.C_evaluaciones_d_calificaciones.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.id_evaluacion_d_empleado == id_evaluacion_d_empleado &&
                x.id_usuario_califica == id_usuario && x.activo == true).FirstOrDefault();
                if (valid_comentarios == null)
                {
                    C_evaluaciones_d_calificaciones new_comentarios = new C_evaluaciones_d_calificaciones();
                    new_comentarios.fortalezas = fortalezas;
                    new_comentarios.area_oportunidad = areas_oportunidad;
                    db.C_evaluaciones_d_calificaciones.Add(new_comentarios);
                    db.SaveChanges();
                }
                else
                {
                    valid_comentarios.fortalezas = fortalezas;
                    valid_comentarios.area_oportunidad = areas_oportunidad;
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public bool CalcularEvaluacionEmpleado(int id_evaluacion_d_empleado, int id_usuario_evalua, int id_evaluacion_g)
        {
            try
            {
                decimal calificacion_total = 0;

                var conceptos = db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_usuario_registra == id_usuario_evalua && x.activo == true)
                    .GroupBy(x => x.C_evaluaciones_conceptos_rubros.C_evaluaciones_rubros.id_evaluacion_rubro).ToList();
                foreach (var rubro in conceptos)
                {
                    decimal valor_rubro = (decimal)rubro.FirstOrDefault().C_evaluaciones_conceptos_rubros.C_evaluaciones_rubros.porcentaje;
                    decimal valor_calificacion = (decimal)rubro.Sum(x => x.porcentaje_calificacion);

                    calificacion_total += (valor_rubro * valor_calificacion) / 100;
                }

                var calificacion = db.C_evaluaciones_d_calificaciones.Where(x => x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.id_usuario_califica == id_usuario_evalua && x.activo == true).FirstOrDefault();
                if (calificacion != null)
                {
                    calificacion.calificacion_final = calificacion_total;
                    db.SaveChanges();
                }
                else
                {
                    C_evaluaciones_d_calificaciones new_calificacion = new C_evaluaciones_d_calificaciones();
                    new_calificacion.id_evaluacion_d_empleado = id_evaluacion_d_empleado;
                    new_calificacion.id_evaluacion_g = id_evaluacion_g;
                    new_calificacion.fecha_calificacion = DateTime.Now;
                    new_calificacion.calificacion_final = calificacion_total;
                    new_calificacion.id_usuario_califica = id_usuario_evalua;
                    new_calificacion.activo = true;
                    db.C_evaluaciones_d_calificaciones.Add(new_calificacion);
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int FinalizarEvaluacionGeneral(int id_evaluacion)
        {
            try
            {
                var evaluacion = db.C_evaluaciones_g.Find(id_evaluacion);
                if (evaluacion == null) { return 1; }  //EVALUACION NO ENCONTRADA
                if (evaluacion.abierta_cerrada == false) { return 2; }  //EVALUACION YA CERRADA
                var valid_status = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion && x.activo == true).Select(x => x.id_evaluacion_status);
                var valid_calificacion = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion && x.activo == true).Select(x => x.calificacion_final);

                if (valid_status.Contains(1) || valid_status.Contains(2) || valid_calificacion.Contains(null)){ return 3; } //HAY EVALUACIONES DE EMPLEADOS PENDIENTES DE EVALUAR
                if (evaluacion.activo == false) { return 4; }  //EVALUACION ESTA DESACTIVADA

                evaluacion.abierta_cerrada = false;
                db.SaveChanges();


                string asunto = "CLAUSURA DE EVALUACIONES: " + evaluacion.descripcion;
                string destinatarios = "";
                var correos_masters = db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == 2008 && x.activo == true);
                try
                {
                    var correo_master = correos_masters.Select(x => x.C_usuarios_corporativo.C_empleados.correo).ToArray();
                    destinatarios += correo_master[0] + ";";
                    for (int i = 1; i < correo_master.Length; i++) { destinatarios += correo_master[i] + ";"; }
                }
                catch (Exception) { }

                var id_usuarios_masters = correos_masters.Select(x => x.id_usuario_corporativo);
                var evaluadores = db.C_evaluaciones_departamentos_usuarios.Where(x => x.activo == true && !id_usuarios_masters.Contains(x.id_usuario)).Select(x => x.C_usuarios_corporativo.C_empleados.correo).Distinct().ToArray();
                foreach (var correo_evaluador in evaluadores)
                {
                    destinatarios += correo_evaluador + ";";
                }

                string mensaje = "<label>El usuario: " + (string)Session["LoggedUser"] + " finalizó todas las evaluaciones semestrales</strong></label><br />" +

                       "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                notificaciones.EnviarCorreoUsuarioReportes(asunto, destinatarios, mensaje);


                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        public PartialViewResult ConsultarDepartamentosPendientesEvaluar(int id_evaluacion)
        {
            var data = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion && x.activo == true && x.id_evaluacion_status != 3).ToList();
            return PartialView("AdministracionEvaluaciones/_DepartamentosPendientesEvaluacion", data);
        }

        #endregion


        #region HISTORIAL DE EVALUACIONES
        public ActionResult HistorialEvaluaciones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8084)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("HistorialEvaluaciones/Index");
        }

        public PartialViewResult ConsultarEvaluacionesDepartamentoAcordeon(int id_evaluacion_g, int id_departamento)
        {
            var data = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.activo == true && x.id_evaluacion_status != 4 && x.id_departamento == id_departamento).OrderBy(x => x.C_nomina_empleados.Apellido_paterno).ToList();
            return PartialView("HistorialEvaluaciones/_EvaluacionesDepartamentoAcordeon", data);
        }


        public PartialViewResult ConsultarEvaluacionesEmpleado(int id_evaluacion_g, int id_evaluacion_d_empleado)
        {
            var data_evaluacion = db.C_evaluaciones_d_empleados.Find(id_evaluacion_d_empleado);
            ViewBag.nombre = data_evaluacion.C_nomina_empleados.Nombres;
            ViewBag.apellido_p = data_evaluacion.C_nomina_empleados.Apellido_paterno;
            ViewBag.apellido_m = data_evaluacion.C_nomina_empleados.Apellido_materno;

            ViewBag.id_evaluacion_g = data_evaluacion.id_evaluacion_g;
            ViewBag.id_evaluacion_d_empleado = id_evaluacion_d_empleado;
            ViewBag.id_evaluacion_status = data_evaluacion.id_evaluacion_status;
            ViewBag.tipo_evaluacion = "EVALUACIÓN GERENCIAL";
            var valid_evaluacion = db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.id_evaluacion_d_empleado == id_evaluacion_d_empleado && x.activo == true).ToList();

            return PartialView("HistorialEvaluaciones/_EvaluacionesEmpleadoTable", valid_evaluacion);
        }


        public ActionResult GenerarExcelEvaluacionesDepartamento(int id_evaluacion_g, int id_departamento)
        {
            var data = db.C_evaluaciones_d_conceptos.Where(x => x.activo == true && x.id_evaluacion_g == id_evaluacion_g && x.C_evaluaciones_d_empleados.id_departamento == id_departamento && x.C_evaluaciones_d_empleados.activo == true).ToList();

            string htmlContent = RenderPartialViewToString(data, "HistorialEvaluaciones/_EvaluacionesDepartamentoExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent);

            Random rand = new Random();
            // Return the Excel file as a download
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EVALUACIONES_.xlsx");
        }

        public PartialViewResult TestExcel(int id_evaluacion_g, int id_departamento)
        {
            var data = db.C_evaluaciones_d_conceptos.Where(x => x.activo == true && x.id_evaluacion_g == id_evaluacion_g && x.C_evaluaciones_d_empleados.id_departamento == id_departamento && x.C_evaluaciones_d_empleados.activo == true).ToList();
            return PartialView("HistorialEvaluaciones/_EvaluacionesDepartamentoExcel", data);
        }

        private string RenderPartialViewToString(object model, string path_partial_view)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, path_partial_view);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        private byte[] ConvertHtmlTableToExcel(string htmlTable)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Parse HTML table content
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlTable);

            // Convert HTML table to Excel
            ConvertHtmlTableToWorksheet(worksheet, doc.DocumentNode.SelectSingleNode("//table"));

            // Save the workbook to a memory stream
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

        private void ConvertHtmlTableToWorksheet(IXLWorksheet worksheet, HtmlNode tableNode)
        {
            if (tableNode != null)
            {
                var rowNumber = 1;
                foreach (var rowNode in tableNode.SelectNodes("tr"))
                {
                    var colNumber = 1;
                    foreach (var cellNode in rowNode.SelectNodes("th|td"))
                    {
                        var decodedText = System.Web.HttpUtility.HtmlDecode(cellNode.InnerHtml);
                        worksheet.Cell(rowNumber, colNumber).Value = decodedText;
                        colNumber++;
                    }
                    rowNumber++;
                }
            }
        }

        public void GenerarExcel()
        {
            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();


            // Fuentes y Tipografia
            var FuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
            var FuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var FuenteGeneral = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var FuenteNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var FuenteSubrayada = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharp.text.Font.UNDERLINE);
            var FuenteSubrayadaNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, iTextSharp.text.Font.UNDERLINE);
            var FuenteError = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40);
            FuenteError.Color = BaseColor.RED;


            #region ENCABEZADO
            PdfPTable encabezado = new PdfPTable(3) { WidthPercentage = 100 };
            encabezado.SetWidths(new float[] { 1f, 3f, 1f });

            // Agregar logo en la primera celda
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(85f, 85f);
                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                encabezado.AddCell(logoCell);
            }

            // Título de la tabla en el centro
            PdfPCell titleCell = new PdfPCell(new Phrase("", FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado.AddCell(titleCell);

            // Folio a la derecha
            PdfPCell folioTabla = new PdfPCell(new Phrase("Folio ", FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado.AddCell(folioTabla);
            #endregion


            document.Close();
            byte[] pdfBytes = ms.ToArray();
            ms.Close();
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Reporete.pdf");
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.BinaryWrite(pdfBytes);
            Response.End();
        }


        #endregion


        #region ADMINISTRACIÓN DE BONOS
        public ActionResult AdministracionBonos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8087)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("AdministracionBonos/Index");
        }

        public PartialViewResult ConsultarEmpleadosBonosTable(int id_departamento)
        {
            {
                try
                {
                    var data = from emp in db.C_nomina_empleados
                               join depa_emp in db.C_evaluaciones_departamentos_empleados on emp.Empleado_id equals depa_emp.id_empleado
                               join bonos_emp in db.C_evaluaciones_empleados_bonos on depa_emp.id_empleado equals bonos_emp.id_empleado into bonos_empleados
                               from bonos in bonos_empleados.DefaultIfEmpty()
                               where emp.Estatus == "A" && depa_emp.activo == true && depa_emp.id_evaluacion_departamento == id_departamento
                               select new BonosEmpleados
                               {
                                   Bonos = bonos,
                                   Empleados = emp
                               };
                    return PartialView("AdministracionBonos/_BonosEmpleadosTable", data);
                }
                catch (Exception)
                {
                    return PartialView("", null);
                }
            }
        }

        public int ActualizarBonosEmpleados(int[] id_empleados, decimal[] montos)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime hoy = DateTime.Now;

            try
            {
                for (int i = 0; i < id_empleados.Length; i++)
                {
                    int id_empleado = id_empleados[i];
                    decimal monto = montos[i];

                    var valid = db.C_evaluaciones_empleados_bonos.Where(x => x.id_empleado == id_empleado).FirstOrDefault();
                    if (valid != null)
                    {
                        if (monto != valid.monto_bono)
                        {
                            valid.fecha_ultima_actualizacion = hoy;
                            valid.id_usuario_actualiza = id_usuario;
                            valid.activo = true;
                        }
                    }
                    else
                    {
                        C_evaluaciones_empleados_bonos new_bono = new C_evaluaciones_empleados_bonos();
                        new_bono.id_empleado = id_empleado;
                        new_bono.monto_bono = monto;
                        new_bono.id_usuario_registra = id_usuario;
                        new_bono.id_usuario_actualiza = id_usuario;
                        new_bono.fecha_registro = hoy;
                        new_bono.fecha_ultima_actualizacion = hoy;
                        new_bono.activo = true;
                        db.C_evaluaciones_empleados_bonos.Add(new_bono);
                    }
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return 1;
            }
        }


        #endregion


        #region PAGO DE BONOS
        public ActionResult PagoBonosEvaluaciones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8088)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("PagoBonos/Index");
        }

        public PartialViewResult ConsultarBonosEvaluacion(int id_evaluacion)
        {
            ViewBag.modo = 1;
            var evaluacion = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion && x.activo == true && x.id_evaluacion_status == 3).ToList();
            return PartialView("PagoBonos/_BonosEmpleadosTable", evaluacion);
        }


        public ActionResult GenerarExcelBonosEvaluacion(int id_evaluacion)
        {
            ViewBag.modo = 2;
            var evaluacion = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion && x.activo == true && x.id_evaluacion_status == 3).ToList();
            string htmlContent = RenderPartialViewToString(evaluacion, "PagoBonos/_BonosEmpleadosTable");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent);

            Random rand = new Random();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BONOS_EVALUACIONES.xlsx");
        }



        #endregion



    }
}