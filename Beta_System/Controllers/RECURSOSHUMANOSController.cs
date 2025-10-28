using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class RECURSOSHUMANOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        [HttpPost]
        public string ConfirmacionFormacionAcademica()
        {
            DateTime hoy = DateTime.Now;
            string path_pdf = "";
            HttpPostedFileBase file = null;
            HttpFileCollectionBase files = Request.Files;

            // Recibir los datos del FormData
            string id_formacion = Request.Form["id_formacion_g"];
            int id_formacion_g = 0;
            try
            {
                id_formacion_g = Convert.ToInt32(id_formacion);
            }
            catch (Exception) { }


            int numero_emp = Convert.ToInt32(Request.Form["numero_emp"]);
            string tituloCarrera = Request.Form["tituloCarrera"];
            string institucion = Request.Form["institucion"];
            string statusCarrera = Request.Form["statusCarrera"];
            string nivelEscolaridad = Request.Form["nivelEscolaridad"];
            string numero_cedula = Request.Form["numero_cedula"];
            string año = Request.Form["año"];

            // Buscar la información del empleado
            var empleadoInfo = db.C_nomina_empleados.FirstOrDefault(x => x.Numero == numero_emp);
            if (empleadoInfo == null)
            {
                return "-5"; // Error: empleado no encontrado
            }
            string NombreEmpleado = empleadoInfo.Nombres.Replace(" ", "") + empleadoInfo.Apellido_paterno.Replace(" ", "") + empleadoInfo.Apellido_materno.Replace(" ", "");



            C_rh_formacion_academica_d formacion_d = new C_rh_formacion_academica_d();
            C_rh_formacion_universidades formacion_universidad = new C_rh_formacion_universidades();
            C_rh_formacion_estudio formacion_estudio = new C_rh_formacion_estudio();

            //Registro del titulo/carrera
            int titulo = 0;
            try
            {
                titulo = Convert.ToInt32(tituloCarrera);
            }
            catch (Exception ex)
            {
                formacion_estudio.activo = true;
                formacion_estudio.estudio = tituloCarrera;
                db.C_rh_formacion_estudio.Add(formacion_estudio);
                db.SaveChanges();

                titulo = formacion_estudio.id_formacion_estudio;
            }
            //Registro de universidad
            int universidad = 0;
            try
            {
                universidad = Convert.ToInt32(institucion);
            }
            catch (Exception ex)
            {
                formacion_universidad.activo = true;
                formacion_universidad.universidad = institucion;
                db.C_rh_formacion_universidades.Add(formacion_universidad);
                db.SaveChanges();

                universidad = formacion_universidad.id_formacion_universidad;
            }

            //REGISTRO NUEVO
            if (id_formacion_g == 0)
            {
                C_rh_formacion_academica_g formacion_g = new C_rh_formacion_academica_g();
                formacion_g.activo = true;
                formacion_g.id_formacion_escolaridad = Convert.ToInt32(nivelEscolaridad);
                db.C_rh_formacion_academica_g.Add(formacion_g);
                db.SaveChanges();

                id_formacion_g = formacion_g.id_formacion_academica_g;

                empleadoInfo.id_formacion_academica_g = id_formacion_g;
                db.SaveChanges();
            }


            formacion_d.activo = true;
            formacion_d.id_formacion_academica_g = id_formacion_g;
            formacion_d.id_formacion_universidad = universidad;
            formacion_d.id_formacion_estudio = titulo;
            formacion_d.id_formacion_status = Convert.ToInt32(statusCarrera);
            if (numero_cedula != "")
            {
                formacion_d.numero_cedula = Convert.ToInt32(numero_cedula);
            }
            if (año != "")
            {
                formacion_d.anio_graduacion = Convert.ToInt32(año);
            }



            string RFC = empleadoInfo.RFC.Replace("-", "");

            // Ruta base donde se guardarán los archivos
            //string basePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\RecursosHumanos\\Empleados\\{RFC}";
            string basePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\EmpleadosDocs\\CedulasProfesionales";

            //// Verificar si la carpeta con el RFC existe, si no, crearla  <---OBSOLETO
            //if (!Directory.Exists(basePath))
            //{
            //    Directory.CreateDirectory(basePath);
            //}

            if (files.Count > 0)
            {
                for (int z = 0; z < files.Count; z++)
                {
                    file = files[z];
                    var file_upload = Request.Files[z];

                    // Validar que el archivo sea un PDF
                    if (!file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return "-2"; // Error: formato incorrecto
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    file = files[i];
                    try//\\192.168.128.2\inetpub\NominaFiles\RecursosHumanos\Empleados  <---OBSOLETO
                    {
                        var file_upload = Request.Files[i];
                        if (file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            // Obtener los archivos existentes en la carpeta que empiezan con "CEDPROF-RFC-"
                            string[] existingFiles = Directory.GetFiles(basePath, $"CedProf_{NombreEmpleado}_{RFC}_*.pdf");
                            // Contar los archivos y sumar 1 para el nuevo archivo
                            int numeroArchivo = existingFiles.Length + 1;
                            // Crear el nombre del archivo con el formato "CedProf_NombreEmpleado_RFC.pdf"
                            //string fileName = $"CEDPROF-{RFC}-{numeroArchivo}.pdf"; <---OBSOLETO
                            string fileName = $"CedProf_{NombreEmpleado}_{RFC}_{numeroArchivo}.pdf";
                            // Ruta completa donde se guardará el archivo
                            string savePath = Path.Combine(basePath, fileName);
                            // Guardar el archivo en la carpeta correspondiente
                            file.SaveAs(savePath);

                            // URL accesible del archivo
                            path_pdf = $"http://192.168.128.2/CedulasProfesionales/{fileName}";
                            formacion_d.path_cedula = path_pdf;
                            db.C_rh_formacion_academica_d.Add(formacion_d);
                            db.SaveChanges();
                        }
                        else
                        {
                            return "-3"; // Error al guardar el archivo
                        }
                    }
                    catch (Exception)
                    {
                        return "-3"; // Error en la operación de guardado
                    }
                }
                return "0"; // Éxito
            }
            else
            {
                db.C_rh_formacion_academica_d.Add(formacion_d);
                db.SaveChanges();
                return "0"; // Éxito
                //return "-4"; // No se detectaron archivos
            }
        }


        public PartialViewResult HistoricoFormacionAcademicaTable(int numero)
        {
            var HistorialEstudios = db.C_rh_formacion_academica_d.Where(x => x.C_rh_formacion_academica_g.C_nomina_empleados.FirstOrDefault().Numero == numero && x.activo == true).ToList();
            return PartialView("../CATALOGOS/FORMACION_ACADEMICA/_HistorialEstudiosEmpleadoTable", HistorialEstudios);
        }

        public int RemoverHistoricoFormacionAcademica(int id_formacion_d)
        {
            try
            {
                var formacion_d = db.C_rh_formacion_academica_d.Find(id_formacion_d);
                formacion_d.activo = false;
                db.SaveChanges();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        [HttpPost]
        public string ActualizarCedulaProfesional()
        {
            HttpPostedFileBase file = null;
            HttpFileCollectionBase files = Request.Files;

            int numero_emp = Convert.ToInt32(Request.Form["numero_emp"]);
            int id_formacion_d = Convert.ToInt32(Request.Form["id_formacion_d"]);

            var formacion_d = db.C_rh_formacion_academica_d.Find(id_formacion_d);

            string path_pdf = "";

            var empleadoInfo = db.C_nomina_empleados.FirstOrDefault(x => x.Numero == numero_emp);
            if (empleadoInfo == null)
            {
                return "-5"; // Error: empleado no encontrado
            }

            string NombreEmpleado = empleadoInfo.Nombres.Replace(" ", "") + empleadoInfo.Apellido_paterno.Replace(" ", "") + empleadoInfo.Apellido_materno.Replace(" ", "");
            string RFC = empleadoInfo.RFC.Replace("-", "");
            string basePath = $"\\\\192.168.128.2\\inetpub\\NominaFiles\\EmpleadosDocs\\CedulasProfesionales";

            if (files.Count > 0)
            {
                for (int z = 0; z < files.Count; z++)
                {
                    file = files[z];
                    var file_upload = Request.Files[z];

                    // Validar que el archivo sea un PDF
                    if (!file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        return "-2"; // Error: formato incorrecto
                    }
                }

                for (int i = 0; i < files.Count; i++)
                {
                    file = files[i];
                    try//\\192.168.128.2\inetpub\NominaFiles\RecursosHumanos\Empleados  <---OBSOLETO
                    {
                        var file_upload = Request.Files[i];
                        if (file_upload.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            // Obtener los archivos existentes en la carpeta que empiezan con "CEDPROF-RFC-"
                            string[] existingFiles = Directory.GetFiles(basePath, $"CedProf_{NombreEmpleado}_{RFC}_*.pdf");
                            // Contar los archivos y sumar 1 para el nuevo archivo
                            int numeroArchivo = existingFiles.Length + 1;
                            // Crear el nombre del archivo con el formato "CedProf_NombreEmpleado_RFC.pdf"
                            //string fileName = $"CEDPROF-{RFC}-{numeroArchivo}.pdf"; <---OBSOLETO
                            string fileName = $"CedProf_{NombreEmpleado}_{RFC}_{numeroArchivo}.pdf";
                            // Ruta completa donde se guardará el archivo
                            string savePath = Path.Combine(basePath, fileName);
                            // Guardar el archivo en la carpeta correspondiente
                            file.SaveAs(savePath);

                            // URL accesible del archivo
                            path_pdf = $"http://192.168.128.2/CedulasProfesionales/{fileName}";
                            formacion_d.path_cedula = path_pdf;
                            db.SaveChanges();
                        }
                        else
                        {
                            return "-3"; // Error al guardar el archivo
                        }
                    }
                    catch (Exception)
                    {
                        return "-3"; // Error en la operación de guardado
                    }
                }
                return "0"; // Éxito
            }
            else
            {
                return "-4"; // No se detectaron archivos
            }
        }
    }
}