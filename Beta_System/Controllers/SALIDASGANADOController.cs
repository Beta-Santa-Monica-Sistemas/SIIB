using Beta_System.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace Beta_System.Controllers
{
    public class SALIDASGANADOController : Controller
    {
        private BETA_CORPEntities db_master = new BETA_CORPEntities();
        private base_forrajesEntities db_forra = new base_forrajesEntities();
        private CATALOGOSController catalogos = new CATALOGOSController();
        public ActionResult GenerarSalida()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(12)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception ex)
            {
                return RedirectToAction("usuariologin", "USUARIOLOGIN");
            }

            return View("../ESTABLOS/SalidasGanado/GenerarSalida");
        }

        public ActionResult ConsultarSalidas()
        {
            try
            {
                if (Session["LoggedId"] != null)
                {
                }
                else { return RedirectToAction("usuariologin", "USUARIOLOGIN"); }
            }
            catch (Exception ex)
            {
                return RedirectToAction("InicioSesion", "LOGIN");
            }

            return View("../ESTABLOS/SalidaGanado/ReporteSalidaDetalle/Reportes/ReporteSalidas");
        }


        public PartialViewResult ConsultarSalidasEstablo(int id_establo, string fecha_inicio, string fecha_fin)
        {
            DateTime fi = DateTime.Parse(fecha_inicio);
            DateTime ff = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            IQueryable<C_salida_ganado_d> salida = null;
            if (id_establo == 0)
            {
                salida = from sal_g in db_master.C_salida_ganado_g
                         join sal_d in db_master.C_salida_ganado_d on sal_g.id_salida_gan_g equals sal_d.id_salida_gan_g
                         where sal_g.fecha >= fi && sal_g.fecha <= ff && sal_g.rstatus == "A" && sal_d.rstatus == "A" && sal_d.confirmado == true
                         select sal_d;
            }
            else
            {
                salida = from sal_g in db_master.C_salida_ganado_g
                         join sal_d in db_master.C_salida_ganado_d on sal_g.id_salida_gan_g equals sal_d.id_salida_gan_g
                         where sal_g.fecha >= fi && sal_g.fecha <= ff && sal_g.rstatus == "A" && sal_g.id_establo == id_establo && sal_d.rstatus == "A" && sal_d.confirmado == true
                         select sal_d;
            }

            return PartialView("../ESTABLOS/SalidasGanado/ReporteSalidaDetalle/_SalidasGanadoEstablo", salida);
        }

        public ActionResult ExportarSalidasGanadoExcel(int id_establo, string fecha_inicio, string fecha_fin)
        {
            try
            {
                DateTime fi = DateTime.Parse(fecha_inicio);
                DateTime ff = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

                IQueryable<C_salida_ganado_d> salida = null;
                if (id_establo == 0)
                {
                    salida = from sal_g in db_master.C_salida_ganado_g
                             join sal_d in db_master.C_salida_ganado_d on sal_g.id_salida_gan_g equals sal_d.id_salida_gan_g
                             where sal_g.fecha >= fi && sal_g.fecha <= ff && sal_g.rstatus == "A" && sal_d.rstatus == "A" && sal_d.confirmado == true
                             select sal_d;
                }
                else
                {
                    salida = from sal_g in db_master.C_salida_ganado_g
                             join sal_d in db_master.C_salida_ganado_d on sal_g.id_salida_gan_g equals sal_d.id_salida_gan_g
                             where sal_g.fecha >= fi && sal_g.fecha <= ff && sal_g.rstatus == "A" && sal_g.id_establo == id_establo && sal_d.rstatus == "A" && sal_d.confirmado == true
                             select sal_d;
                }

                var groupedIncidencias = salida.GroupBy(i => i.C_salida_ganado_g.id_establo).ToList();

                using (var workbook = new XLWorkbook())
                {
                    if (groupedIncidencias.Any())
                    {
                        foreach (var group in groupedIncidencias)
                        {
                            var worksheet = workbook.Worksheets.Add($"{group.First().C_salida_ganado_g.C_salida_ganado_establos.establo}");
                            var currentRow = 1;
                            var columnCount = 5;

                            var tipoIncidencia = group.Key;

                            // Ajustar el tamaño de 'headerRange' según el tipo de incidencia
                            var headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                            columnCount = 16;
                            headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);

                            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                var image = worksheet.AddPicture(stream)
                                                     .MoveTo(worksheet.Cell(currentRow, 1))
                                                     .Scale(0.2);  // Ajustar el tamaño de la imagen si es necesario
                            }

                            // Insertar el texto centrado en la cabecera
                            headerRange.Merge();
                            worksheet.Cell(currentRow, 1).Value = "Reporte de salidas de ganado"; // Colocar el texto en la primera celda del rango
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
                                case 2:
                                case 3:
                                case 4:
                                    titleRange.Merge();
                                    titleRange.Value = $"{group.First().C_salida_ganado_g.C_salida_ganado_establos.establo} - {fi.ToShortDateString()} AL {ff.ToShortDateString()}";
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
                            var headers = new[] { "Folio", "Fecha" };
                            headers = headers.Concat(new[] { "Hora", "Arete", "Clasificacion", "Causa Baja", "Condición", "Edad", "Cantidad", "Peso",
                                 "Chofer", "Vehículo", "Placas", "Comprador", "Sala","Salida"}).ToArray();

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
                                worksheet.Cell(currentRow, 1).Value = item.folio;
                                worksheet.Cell(currentRow, 2).Value = item.C_salida_ganado_g.fecha.Value.ToShortDateString();
                                worksheet.Cell(currentRow, 3).Value = item.C_salida_ganado_g.hora;
                                worksheet.Cell(currentRow, 4).Value = item.arete;
                                worksheet.Cell(currentRow, 5).Value = item.clasifica;
                                worksheet.Cell(currentRow, 6).Value = item.causa_b;
                                worksheet.Cell(currentRow, 7).Value = item.cond_ind;
                                worksheet.Cell(currentRow, 8).Value = item.edad;
                                worksheet.Cell(currentRow, 9).Value = item.cantidad;
                                worksheet.Cell(currentRow, 10).Value = item.peso;
                                worksheet.Cell(currentRow, 11).Value = item.C_salida_ganado_g.chofer;
                                worksheet.Cell(currentRow, 12).Value = item.C_salida_ganado_g.vehiculo;
                                worksheet.Cell(currentRow, 13).Value = item.C_salida_ganado_g.placas;
                                worksheet.Cell(currentRow, 14).Value = item.C_salida_ganado_g.comprador;
                                worksheet.Cell(currentRow, 15).Value = item.sala;
                                worksheet.Cell(currentRow, 16).Value = item.C_salida_ganado_g.C_salida_ganado_tipos_salidas.nombre_salida;

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
                        worksheet.Cell(1, 1).Value = "No se encontraron salidas de ganado para los criterios seleccionados.";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Row(1).Height = 30;
                        worksheet.Columns().AdjustToContents();
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE_SALIDAS_GANADO_BSM_" + fi.ToShortDateString() + "_AL_" + ff.ToShortDateString() + ".xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        public PartialViewResult ConsultarEstablosSelect()
        {
            int id_usuario = (int)Session["LoggedId"];
            var establos_id = db_master.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true).ToList().Select(x => x.id_establo).ToArray();

            var establos = db_master.C_salida_ganado_establos.Where(x => establos_id.Contains((int)x.id_establo)).OrderBy(x => x.establo).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_EstablosSelect", establos);
        }

        public PartialViewResult ConsultarClasificacionesGanado()
        {
            var clasif = db_master.C_salida_ganado_clasificaciones_ganado.Where(x => x.activo == true).OrderBy(x => x.clasificacion).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_ClasificacionesGanado", clasif);
        }

        public PartialViewResult ConsultarCondicionesClasificacion(int id_clasificacion)
        {
            var condiciones = db_master.C_salida_ganado_condiciones_ganado.Where(x => x.activo == true && x.id_clasificacion == id_clasificacion).OrderBy(x => x.condicion).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_CondicionesGanado", condiciones);
        }

        public PartialViewResult AgregarVacaTabla(int count_tabla, string arete, string clasif, string causa, string sala, string edad,
    decimal peso_neto, decimal cantidad, decimal p_unitario, decimal importe, string condicion)
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

            return PartialView("../ESTABLOS/SalidasGanado/_TheadSalida", detalle);
        }



        public PartialViewResult AnadirVacaTabla(int count_tabla, string arete, string clasif, string causa, string sala, string edad, string condicion, string imagen_base64, int id_causa_muerte, string causa_muerte)
        {
            C_salida_ganado_d detalle = new C_salida_ganado_d();
            detalle.arete = arete;
            detalle.clasifica = clasif;
            detalle.causa_b = causa;
            detalle.sala = sala;
            detalle.cond_ind = condicion;
            detalle.edad = edad.ToString();

            detalle.id_causa_muerte = id_causa_muerte;

            ViewBag.causa_muerte = causa_muerte;
            ViewBag.count = count_tabla;
            ViewBag.imagen_base64 = imagen_base64;

            return PartialView("../ESTABLOS/SalidasGanado/_TheadSalida", detalle);
        }







        #region CARGA DE GANADO (MEDICOS)

        public string GenerarFolioEstablo(int id_establo)
        {
            try
            {
                int consecuivo;
                int generacion = 20;

                string siglas = "";
                if (id_establo == 1) { siglas = "SG"; }

                else if (id_establo == 2) { siglas = "CR"; }

                else if (id_establo == 3) { siglas = "SM"; }

                else { siglas = "TN"; }


                var valid = db_master.C_salida_ganado_g.Where(X => X.id_establo == id_establo).OrderByDescending(x => x.id_salida_gan_g).FirstOrDefault();

                if (valid == null) { consecuivo = 0001; generacion = 20; }
                else
                {
                    consecuivo = Convert.ToInt32(valid.folio.Split('-')[2]) + 1;
                    generacion = Convert.ToInt32(valid.folio.Split('-')[1]);
                }

                if (consecuivo == 10000) { generacion = generacion++; consecuivo = 01; }

                string folio = siglas + "-" + generacion.ToString().PadLeft(2, '0') + "-" + consecuivo.ToString().PadLeft(4, '0');

                return folio;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "";
            }

        }

        public PartialViewResult CargarDGanadoTabla(int count_tabla, string arete, string clasif, string causa, string sala, string edad, string condicion, string imagen_base64, int id_causa_muerte, string causa_muerte)
        {
            C_salida_ganado_d detalle = new C_salida_ganado_d();
            detalle.arete = arete;
            detalle.clasifica = clasif;
            detalle.causa_b = causa;
            detalle.sala = sala;
            detalle.cond_ind = condicion;
            detalle.edad = edad.ToString();
            detalle.peso = null;
            detalle.cantidad = 1;
            detalle.p_unitario = 0;
            detalle.id_causa_muerte = id_causa_muerte;

            ViewBag.causa_muerte = causa_muerte;
            ViewBag.importe = 0;
            ViewBag.count = count_tabla;
            ViewBag.imagen_base64 = imagen_base64;

            return PartialView("../ESTABLOS/SalidasGanado/_CargarDGanadoTabla", detalle);
        }

        public int ConfirmarGGanadoTabla(C_salida_ganado_g salida, int tipo_salida)
        {
            try
            {
                var validFolio = db_master.C_salida_ganado_g.Where(x => x.folio == salida.folio).FirstOrDefault();
                if (validFolio != null) { return -1; }

                int id_establo = (int)salida.id_establo;
                DateTime fecha_ficha = DateTime.Now;

                salida.tregistro = fecha_ficha;
                salida.rtime = fecha_ficha;
                salida.fecha = fecha_ficha;
                salida.hora = fecha_ficha.ToShortTimeString().Split(' ')[0];
                salida.cancelada = false;
                salida.rstatus = "A";
                salida.id_tipo_salida = tipo_salida;

                

                db_master.C_salida_ganado_g.Add(salida);
                db_master.SaveChanges();

                return salida.id_salida_gan_g;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public string ConfirmarDGanadoTabla(string folio_g, int id_salida_g, string[] aretes, string[] clasificaciones, string[] causas, string[] condiciones, string[] salas, string[] edades, string[] imagen_base64, int[] id_causa_muerte)
        {
            try
            {
                string path_publico = "";
                DateTime Hoy = DateTime.Now;
                for (int i = 0; i < aretes.Length; i++)
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
                    salida_d.cantidad = 1;

                    //salida_d.peso = pesos_neto[i];

                    salida_d.p_unitario = 0;
                    salida_d.valor = 0;

                    salida_d.rstatus = "A";
                    salida_d.rtime = Hoy;
                    salida_d.tregistro = Hoy;
                    salida_d.id_causa_muerte = id_causa_muerte[i];
                    salida_d.confirmado = false;
                    db_master.C_salida_ganado_d.Add(salida_d);
                }
                db_master.SaveChanges();

                //GUARDADO DE LA FOTOGRAFIA Y PATH
                var salidas_d = db_master.C_salida_ganado_d.Where(x => x.id_salida_gan_g == id_salida_g).ToList();
                int contador = 0;
                foreach (var item in salidas_d)
                {
                    //SUBIR ARCHIVO base64 AL SERVIDOR
                    try
                    {
                        // Extraer la parte de datos base64 (eliminando el prefijo si existe)
                        var base64Data = imagen_base64[contador].Contains(",") ? imagen_base64[contador].Split(',')[1] : imagen_base64[contador];
                        byte[] imageBytes = Convert.FromBase64String(base64Data);
                        // Generar un nombre de archivo
                        string fileName = $"SG{item.id_saliga_gan_d}.jpg";
                        // Ruta de guardado en red
                        string basePath = @"\\192.168.128.2\inetpub\NominaFiles\EmpleadosDocs\SalidasGanado";
                        string savePath = Path.Combine(basePath, fileName);

                        // Guardar la imagen en disco
                        try
                        {
                            System.IO.File.WriteAllBytes(savePath, imageBytes);
                            // Ruta pública para acceder a la imagen
                            path_publico = $"http://192.168.128.2/SalidasGanado/{fileName}";
                        }
                        catch (Exception)
                        {
                            path_publico = "No se pudo guardar la imagen";
                        }
                    }
                    catch (Exception)
                    {
                        path_publico = "No se pudo convertir la imagen";
                    }
                    item.path = path_publico;
                    db_master.SaveChanges();
                    contador++;
                }
                return folio_g;
            }
            catch (Exception EX)
            {
                string msj = EX.ToString();
                return "";
            }
        }
        #endregion

        #region CONFIRMACION SALIDA DE GANADO
        public string ConsultarInfoFicha(int id_establo, string ficha)
        {
            var valid_pesada = db_master.C_salida_ganado_g.Where(x => x.ficha == ficha.Trim()).FirstOrDefault();
            if (valid_pesada != null) { return "-1"; }

            string usuario = "";
            ficha = ficha.Trim().PadLeft(25, ' ');
            if (id_establo == 1)  //SAN GABIREL
            {
                var result = from fich in db_forra.fichas_sg
                             join prov in db_forra.proveedo on fich.prov_cli equals prov.cvepro
                             //join user in db_forra.usuarios on fich.user_id equals user.user_id
                             where fich.ficha == ficha
                             select new { prov.despro, fich.peso1, fich.peso2, fich.peso_t, fich.chofer, fich.propietario_camion, fich.placas, fich.cvemov, fich.user_id };
                if (result != null)
                {
                    try
                    {
                        int id_usuario = Convert.ToInt32(result.FirstOrDefault().user_id);
                        var usuario_db = db_master.C_usuarios_corporativo.Find(id_usuario).C_empleados;
                        usuario = usuario_db.nombres + " " + usuario_db.apellido_paterno;
                    }
                    catch (Exception) { usuario = result.FirstOrDefault().user_id; }
                }

                var res = result.FirstOrDefault();
                if (res == null) { return "[]"; }
                if (!result.FirstOrDefault().cvemov.Contains("5")) { return "0"; }

                return Newtonsoft.Json.JsonConvert.SerializeObject(result) + "|" + usuario;
            }
            else if (id_establo == 2)  //SAN IGNACIO
            {
                var result = from fich in db_forra.fichas_si
                             join prov in db_forra.proveedo on fich.prov_cli equals prov.cvepro
                             //join user in db_forra.usuarios on fich.user_id equals user.user_id
                             where fich.ficha == ficha
                             select new { prov.despro, fich.peso1, fich.peso2, fich.peso_t, fich.chofer, fich.propietario_camion, fich.placas, fich.cvemov, fich.user_id };
                if (result != null)
                {
                    try
                    {
                        int id_usuario = Convert.ToInt32(result.FirstOrDefault().user_id);
                        var usuario_db = db_master.C_usuarios_corporativo.Find(id_usuario).C_empleados;
                        usuario = usuario_db.nombres + " " + usuario_db.apellido_paterno;
                    }
                    catch (Exception) { usuario = result.FirstOrDefault().user_id; }
                }

                var res = result.FirstOrDefault();
                if (res == null) { return "[]"; }
                if (!result.FirstOrDefault().cvemov.Contains("5")) { return "0"; }

                return Newtonsoft.Json.JsonConvert.SerializeObject(result) + "|" + usuario;
            }

            else  //SANTA MONICA (3)
            {
                var result = from fich in db_forra.fichas_sm
                             join prov in db_forra.Proveedo_B on fich.prov_cli equals prov.cvepro
                             //join user in db_forra.usuarios on fich.user_id equals user.user_id
                             where fich.ficha == ficha
                             select new { prov.despro, fich.peso1, fich.peso2, fich.peso_t, fich.chofer, fich.propietario_camion, fich.placas, fich.cvemov, fich.user_id };
                if (result != null)
                {
                    try
                    {
                        int id_usuario = Convert.ToInt32(result.FirstOrDefault().user_id);
                        var usuario_db = db_master.C_usuarios_corporativo.Find(id_usuario).C_empleados;
                        usuario = usuario_db.nombres + " " + usuario_db.apellido_paterno;
                    }
                    catch (Exception) { usuario = result.FirstOrDefault().user_id; }
                }


                var res = result.FirstOrDefault();
                if (res == null) { return "[]"; }
                if (!result.FirstOrDefault().cvemov.Contains("5")) { return "0"; }

                return Newtonsoft.Json.JsonConvert.SerializeObject(result) + "|" + usuario;
            }
        }

        public string ValidarConsultarSalidasAreteEstablo(string folio_g, int id_establo)
        {
            var aretes = db_master.C_salida_ganado_d.Where(x => x.folio == folio_g && x.C_salida_ganado_g.id_establo == id_establo).ToList();
            if (aretes.Count() == 0)
            {
                return "1";
            }
            else if (aretes.FirstOrDefault().confirmado == true)
            {
                return "2";
            }
            else
            {
                return "0";
            }


        }
        public PartialViewResult ConsultarSalidasAreteEstablo(string folio_g, int id_establo)
        {
            var aretes = db_master.C_salida_ganado_d.Where(x => x.folio == folio_g && x.C_salida_ganado_g.id_establo == id_establo).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_TheadArete", aretes);
        }
        public int GenerarSGanado(C_salida_ganado_g salida)
        {
            try
            {
                var sganado = db_master.C_salida_ganado_g.Find(salida.id_salida_gan_g);
                sganado.ficha = salida.ficha;
                sganado.peso1 = salida.peso1;
                sganado.peso2 = salida.peso2;
                sganado.peso_t = salida.peso_t;
                sganado.pesador = salida.pesador;
                sganado.chofer = salida.chofer;
                sganado.placas = salida.placas;
                sganado.vehiculo = salida.vehiculo;
                sganado.comprador = salida.comprador;
                sganado.importe = "0";
                db_master.SaveChanges();

                var sganadoD = db_master.C_salida_ganado_d.Where(x => x.id_salida_gan_g == salida.id_salida_gan_g).ToList();

                decimal totalPorGanado = Convert.ToDecimal(sganado.peso_t) / sganadoD.Count();
                foreach (var item in sganadoD)
                {
                    item.peso = totalPorGanado;
                    item.confirmado = true;
                }
                db_master.SaveChanges();

                return salida.id_salida_gan_g;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion






        public int GenerarSalidaGanado(C_salida_ganado_g salida, int tipo_salida)
        {
            try
            {
                var validFolio = db_master.C_salida_ganado_g.Where(x => x.folio == salida.folio).FirstOrDefault();
                if (validFolio != null) { return -1; }

                int id_establo = (int)salida.id_establo;
                DateTime fecha_ficha = DateTime.Now;
                if (tipo_salida == 1) //LOTE
                {
                    string ficha = salida.ficha;
                    if (id_establo == 1) //SAN GABRIEL
                    {
                        var ficha_info = db_forra.fichas_sg.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                        if (ficha_info == null) { return -2; }
                        if (ficha_info.rtime_p2 == null || !ficha_info.rtime_p2.HasValue) { return -2; }
                        fecha_ficha = (DateTime)ficha_info.rtime_p2;
                    }
                    if (id_establo == 2) //SAN IGNACIO
                    {
                        var ficha_info = db_forra.fichas_si.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                        if (ficha_info == null) { return -2; }
                        if (ficha_info.rtime_p2 == null || !ficha_info.rtime_p2.HasValue) { return -2; }
                        fecha_ficha = (DateTime)ficha_info.rtime_p2;
                    }
                    if (id_establo == 3) //SANTA MONICA
                    {
                        var ficha_info = db_forra.fichas_sm.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                        if (ficha_info == null) { return -2; }
                        if (ficha_info.rtime_p2 == null || !ficha_info.rtime_p2.HasValue) { return -2; }
                        fecha_ficha = (DateTime)ficha_info.rtime_p2;
                    }
                }

                salida.tregistro = fecha_ficha;
                salida.rtime = fecha_ficha;
                salida.fecha = fecha_ficha;
                salida.hora = fecha_ficha.ToShortTimeString().Split(' ')[0];
                salida.cancelada = false;
                salida.rstatus = "A";
                salida.id_tipo_salida = tipo_salida;
                db_master.C_salida_ganado_g.Add(salida);
                db_master.SaveChanges();
                return salida.id_salida_gan_g;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public string GenerarSalidaGanadoDetalle(string folio_g, int id_salida_g, string[] aretes, string[] clasificaciones, string[] causas, string[] condiciones, string[] salas, string[] edades,
            decimal[] cantidades, decimal[] pesos_neto, decimal[] p_unitarios, decimal[] importes)
        {
            try
            {
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
                    salida_d.confirmado = true;
                    db_master.C_salida_ganado_d.Add(salida_d);
                }
                db_master.SaveChanges();
                return folio_g;
            }
            catch (Exception EX)
            {
                string msj = EX.ToString();
                return "";
            }
        }


        public PartialViewResult GenerarPDFSalidaGanado(int id_salida_g)
        {
            var salida = db_master.C_salida_ganado_d.Where(x => x.id_salida_gan_g == id_salida_g && x.rstatus == "A").OrderBy(x => x.tregistro).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/ReporteSalidaDetalle/_PDFSalida", salida);
        }

        public PartialViewResult ConsultarInfoFichaPDF(string ficha, int id_establo)
        {
            fichas_sg fich_obj = new fichas_sg();
            string nombre_pesador = "";
            string nombre_proveedor = "";
            if (id_establo == 1) //SAN GABRIEL
            {
                var ficha_info = db_forra.fichas_sg.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                fich_obj.peso1 = ficha_info.peso1;
                fich_obj.peso2 = ficha_info.peso2;
                fich_obj.peso_t = ficha_info.peso_t;
                var usuario = db_forra.usuarios.Where(x => x.user_id.Contains(ficha_info.user_id)).FirstOrDefault();
                if (usuario != null) { nombre_pesador = usuario.usuario; }
                fich_obj.maquilador = nombre_pesador;
                fich_obj.chofer = ficha_info.chofer;
                fich_obj.propietario_camion = ficha_info.propietario_camion;
                fich_obj.placas = ficha_info.placas;

                var prov = db_forra.proveedo.Where(x => x.cvepro == ficha_info.prov_cli).FirstOrDefault();
                if (prov != null) { nombre_proveedor = prov.despro; }
                fich_obj.obs_p2 = nombre_proveedor;
            }
            else if (id_establo == 2) //SAN IGNACIO
            {
                var ficha_info = db_forra.fichas_si.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                fich_obj.peso1 = ficha_info.peso1;
                fich_obj.peso2 = ficha_info.peso2;
                fich_obj.peso_t = ficha_info.peso_t;
                var usuario = db_forra.usuarios.Where(x => x.user_id.Contains(ficha_info.user_id)).FirstOrDefault();
                if (usuario != null) { nombre_pesador = usuario.usuario; }
                fich_obj.maquilador = nombre_pesador.ToString();
                fich_obj.chofer = ficha_info.chofer;
                fich_obj.propietario_camion = ficha_info.propietario_camion;
                fich_obj.placas = ficha_info.placas;

                var prov = db_forra.proveedo.Where(x => x.cvepro == ficha_info.prov_cli).FirstOrDefault();
                if (prov != null) { nombre_proveedor = prov.despro; }
                fich_obj.obs_p2 = nombre_proveedor;
            }
            else //SANTA MONICA
            {
                var ficha_info = db_forra.fichas_sm.Where(x => x.ficha.Contains(ficha)).FirstOrDefault();
                fich_obj.peso1 = ficha_info.peso1;
                fich_obj.peso2 = ficha_info.peso2;
                fich_obj.peso_t = ficha_info.peso_t;
                var usuario = db_forra.usuarios.Where(x => x.user_id.Contains(ficha_info.user_id)).FirstOrDefault();
                if (usuario != null) { nombre_pesador = usuario.usuario; }
                fich_obj.maquilador = nombre_pesador;
                fich_obj.chofer = ficha_info.chofer;
                fich_obj.propietario_camion = ficha_info.propietario_camion;
                fich_obj.placas = ficha_info.placas;

                var prov = db_forra.proveedo.Where(x => x.cvepro == ficha_info.prov_cli).FirstOrDefault();
                if (prov != null) { nombre_proveedor = prov.despro; }
                fich_obj.obs_p2 = nombre_proveedor;
            }
            return PartialView("../ESTABLOS/SalidasGanado/ReporteSalidaDetalle/_InfoFicha", fich_obj);

        }

        public PartialViewResult ConsultarGanadoPendiente(int id_establo)
        {
            var salida = db_master.C_salida_ganado_d.Where(x => x.C_salida_ganado_g.id_establo == id_establo && x.confirmado == false).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_GanadoPendiente", salida);
        }

        #region FOTOGRAFIA - ARETES VACA
        [HttpPost]
        public ActionResult SubirFotos(List<string> imagenesBase64, List<string> ids)
        {
            if (imagenesBase64 == null || imagenesBase64.Count == 0)
            {
                return Content("⚠️ No se recibieron imágenes.");
            }

            var client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes("jaleman:jaleman140923");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            List<string> resultados = new List<string>();

            for (int i = 0; i < imagenesBase64.Count; i++)
            {
                try
                {
                    // Extraer la parte de datos base64 (eliminando el prefijo si existe)
                    var base64Data = imagenesBase64[i].Split(',')[1];
                    byte[] imageBytes = Convert.FromBase64String(base64Data);
                    // Generar un nombre de archivo único si no se proporcionan IDs
                    string fileName = ids != null && ids.Count > i ? $"{ids[i]}.jpg" : $"foto_{Guid.NewGuid()}.jpg";
                    // Crear el contenido para subir
                    var content = new ByteArrayContent(imageBytes);
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Ajustar según el tipo real de imagen

                    var uploadUrl = $"https://cloud.beta.com.mx/remote.php/webdav/Fotos_SIIB/{fileName}";
                    var response = client.PutAsync(uploadUrl, content).Result;

                    if (response.IsSuccessStatusCode)
                        resultados.Add($"✅ {fileName} subida correctamente");
                    else
                        resultados.Add($"❌ Error al subir {fileName}: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    resultados.Add($"❌ Error procesando imagen {i + 1}: {ex.Message}");
                }
            }

            return Content(string.Join("<br/>", resultados));
        }

        #endregion


        public PartialViewResult SelectTiposCausaMuerte()
        {
            var tiposCausaMuerte = db_master.C_salida_ganado_causas_muerte_tipos.Where(x => x.activo == true).OrderBy(x => x.tipo_muerte).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_SelectTiposCausaMuerte", tiposCausaMuerte);
        }

        public PartialViewResult SelectCausaMuerte(int id_tipo_causa_muerte)
        {
            var causaMuerte = db_master.C_salida_ganado_causas_muerte.Where(x => x.activo == true && x.id_causa_muerte_tipo == id_tipo_causa_muerte);
            return PartialView("../ESTABLOS/SalidasGanado/_SelectCausaMuerte", causaMuerte);
        }

        public PartialViewResult ConsultarTiposSalidaSelect()
        {
            var data = db_master.C_salida_ganado_tipos_salidas.Where(x => x.activo == true).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_SelectTiposSalida", data);
        }

        public bool ValidarArete(string arete)
        {
            string areteTrimmed = arete.Trim().ToLowerInvariant();

            if (areteTrimmed == "sa" || areteTrimmed == "sinarete" || areteTrimmed == "sin arete")
            {
                return false;
            }


            var validacion = db_master.C_salida_ganado_d.Where(x => x.arete == arete.Trim() && x.rstatus == "A").Count();
            //ARETE SIN UTILIZAR
            if (validacion == 0)
            {
                return false;
            }
            //ARETE UTILIZADO
            return true;
        }


        #region INDICADORES
        public string ConsultarReporteTiposMuerte(int[] id_establo, string fecha_inicio, string fecha_fin, int[] id_tipo_salida, int[] id_tipo_muerte)
        {
            List<GraficasReportePie> data = new List<GraficasReportePie>();
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            int id_usuario = (int)Session["LoggedId"];

            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            if (id_tipo_muerte.Contains(0)) { id_tipo_muerte = catalogos.ConsultarTiposCausaMuerteSalidaGanado(); }
            if (id_tipo_salida.Contains(0)) { id_tipo_salida = catalogos.ConsultarTiposSalidaGanado(); }

            string establos = string.Join(",", id_establo);
            string tipo_muerte = string.Join(",", id_tipo_muerte);
            string tipos_salida = string.Join(",", id_tipo_salida);

            using (var conn = ConexionSIIB.Conectar())
            {
                SqlCommand cmd = new SqlCommand("usp_3_SALIDASGANADO_ReporteTiposMuerte", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_establo", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_tipo_salida", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_tipo_muerte", SqlDbType.VarChar));
                cmd.Parameters["@fecha_inicio"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                cmd.Parameters["@fecha_fin"].Value = fecha_f.ToString("yyyy-MM-dd HH:mm");
                cmd.Parameters["@id_establo"].Value = establos;
                cmd.Parameters["@id_tipo_salida"].Value = tipos_salida;
                cmd.Parameters["@id_tipo_muerte"].Value = tipo_muerte;
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GraficasReportePie salida = new GraficasReportePie();
                    salida.Nombre = reader["Tipo"].ToString();
                    salida.Valor = Convert.ToDecimal(reader["Valor"]);
                    data.Add(salida);
                }
                reader.Close();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public string ConsultarReporteCausasMuerte(int[] id_establo, string fecha_inicio, string fecha_fin, int[] id_tipo_salida, int[] id_tipo_muerte)
        {
            List<GraficasReportePie> data = new List<GraficasReportePie>();
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            int id_usuario = (int)Session["LoggedId"];

            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            if (id_tipo_muerte.Contains(0)) { id_tipo_muerte = catalogos.ConsultarTiposCausaMuerteSalidaGanado(); }
            if (id_tipo_salida.Contains(0)) { id_tipo_salida = catalogos.ConsultarTiposSalidaGanado(); }

            string establos = string.Join(",", id_establo);
            string tipo_muerte = string.Join(",", id_tipo_muerte);
            string tipos_salida = string.Join(",", id_tipo_salida);

            using (var conn = ConexionSIIB.Conectar())
            {
                SqlCommand cmd = new SqlCommand("usp_3_SALIDASGANADO_ReporteCausasMuerteTipoSalida", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters
                cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_establo", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_tipo_salida", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@id_tipo_muerte", SqlDbType.VarChar));
                cmd.Parameters["@fecha_inicio"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                cmd.Parameters["@fecha_fin"].Value = fecha_f.ToString("yyyy-MM-dd HH:mm");
                cmd.Parameters["@id_establo"].Value = establos;
                cmd.Parameters["@id_tipo_salida"].Value = tipos_salida;
                cmd.Parameters["@id_tipo_muerte"].Value = tipo_muerte;
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    GraficasReportePie salida = new GraficasReportePie();
                    salida.Nombre = reader["Causa"].ToString();
                    salida.Valor = Convert.ToDecimal(reader["Valor"]);
                    data.Add(salida);
                }
                reader.Close();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public PartialViewResult ConsultarReporteIndicadoresTable(int[] id_establo, string fecha_inicio, string fecha_fin, int[] id_tipo_salida, int[] id_tipo_muerte)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            int id_usuario = (int)Session["LoggedId"];

            if (id_establo.Contains(0)) { id_establo = catalogos.EstablosUsuariosID(id_usuario); }
            if (id_tipo_muerte.Contains(0)) { id_tipo_muerte = catalogos.ConsultarTiposCausaMuerteSalidaGanado(); }
            if (id_tipo_salida.Contains(0)) { id_tipo_salida = catalogos.ConsultarTiposSalidaGanado(); }

            string establos = string.Join(",", id_establo);
            string tipo_muerte = string.Join(",", id_tipo_muerte);
            string tipos_salida = string.Join(",", id_tipo_salida);

            DataTable dt = new DataTable();
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_3_SALIDASGANADO_ReporteBajasVaca", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    cmd.Parameters.Add(new SqlParameter("@Fecha_inicio", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@Fecha_fin", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@Establo", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@Tipo_salida", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@Tipo_muerte", SqlDbType.VarChar));
                    cmd.Parameters["@Fecha_inicio"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                    cmd.Parameters["@Fecha_fin"].Value = fecha_f.ToString("yyyy-MM-dd HH:mm");
                    cmd.Parameters["@Establo"].Value = establos;
                    cmd.Parameters["@Tipo_salida"].Value = tipos_salida;
                    cmd.Parameters["@Tipo_muerte"].Value = tipo_muerte;
                    SqlDataReader reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    reader.Close();
                }
                return PartialView("../ESTABLOS/SalidasGanado/_ReporteIndicadoresTable", dt);
            }
            catch (Exception)
            {
                return PartialView("../ESTABLOS/SalidasGanado/_ReporteIndicadoresTable", null);
            }
        }

        #endregion




    }
}