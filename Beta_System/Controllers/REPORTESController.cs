using Antlr.Runtime.Tree;
using Beta_System.Models;
using Beta_System.Models.GastosMensualesMS;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FirebirdSql.Data.FirebirdClient;
using HtmlAgilityPack;
using Irony;
using Irony.Parsing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebGrease.Extensions;
using DataTable = System.Data.DataTable;
using Rectangle = iTextSharp.text.Rectangle;

namespace Beta_System.Controllers
{
    public class REPORTESController : Controller
    {
        private FbConnection conn = new FbConnection();

        private BETA_CORPEntities db = new BETA_CORPEntities();
        private Master_SIIBEntities db_master_sib = new Master_SIIBEntities();
        private comprasEntities db_compras = new comprasEntities();

        REQUISICIONESController REQUISICIONES_CONTROLLER = new REQUISICIONESController();
        CATALOGOSController CATALOGOS_CONTROLLER = new CATALOGOSController();


        public List<object[]> EjecturarQueryFireBird(string query, FbConnection conn)
        {
            var data = new List<object[]>();
            FbCommand readCommand = new FbCommand(query, conn);
            FbDataReader reader = readCommand.ExecuteReader();
            while (reader.Read())
            {
                var columns = new object[reader.FieldCount];
                reader.GetValues(columns);
                data.Add(columns);
            }
            return data;
        }



        #region DESCARGAR AUTOMATICA EXCEL
        private string RenderPartialViewToString(object model, string path_vista_parcial)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, path_vista_parcial /*"_ExcelImportacionesFacturas"*/);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                return sw.GetStringBuilder().ToString();
            }
        }

        private byte[] ConvertHtmlTableToExcel(string htmlTable, int modo)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Parse HTML table content
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlTable);

            // Convert HTML table to Excel
            if (modo == 1)
            {
                ConvertHtmlTableToWorksheetComparativoPresupuestoOC(worksheet, doc.DocumentNode.SelectSingleNode("//table"));
            }
            else
            {
                ConvertHtmlTableToWorksheet(worksheet, doc.DocumentNode.SelectSingleNode("//table"));
            }

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
                        worksheet.Cell(rowNumber, colNumber).Value = cellNode.InnerHtml;
                        colNumber++;
                    }
                    rowNumber++;
                }
            }
        }

        private void ConvertHtmlTableToWorksheetComparativoPresupuestoOC(IXLWorksheet worksheet, HtmlNode tableNode)
        {
            var array_meses = db.C_presupuestos_meses.Select(x => x.mes).ToArray();
            if (tableNode != null)
            {
                var rowNumber = 1;
                foreach (var rowNode in tableNode.SelectNodes("tr"))
                {
                    var colNumber = 1;
                    foreach (var cellNode in rowNode.SelectNodes("th|td"))
                    {
                        try
                        {
                            if (array_meses.Contains(cellNode.InnerHtml.ToString().Trim()))
                            {
                                // Combinar las celdas de la fila actual en las columnas actuales
                                worksheet.Range(rowNumber, colNumber, rowNumber, colNumber + 2).Merge().Value = cellNode.InnerHtml;
                                worksheet.Range(rowNumber, colNumber, rowNumber, colNumber + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                worksheet.Range(rowNumber, colNumber, rowNumber, colNumber + 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                colNumber += 2; // Incrementar en 2 porque hemos combinado tres celdas (colNumber, colNumber + 1, colNumber + 2)
                            }
                            else
                            {
                                worksheet.Cell(rowNumber, colNumber).Value = cellNode.InnerHtml;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Manejo de excepción
                            Console.WriteLine("Error: " + ex.Message);
                        }

                        colNumber++;
                    }
                    rowNumber++;
                }
            }
        }


        #endregion

        #region ORDENES DE COMPRA VS PRESUPUESTO
        public ActionResult ComparativoPresupuestoOrdenesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8054)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("ComparativoPresupuestoOrdenesCompra/Index");
        }

        public PartialViewResult ConsultarComparativoCompraPresupuesto(int id_anio, int[] id_cargos_contable)
        {
            List<PresupuestosOrdenCompra> data = new List<PresupuestosOrdenCompra>();
            List<(decimal, decimal)> consumos = new List<(decimal, decimal)>();

            decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
            var anos = db.C_presupuestos_anios.Find(id_anio);

            if (id_cargos_contable.Contains(0))
            {
                id_cargos_contable = db.C_cargos_contables_g.Where(x => x.activo == true).Select(x => x.id_cargo_contable_g).ToArray();
            }
            foreach (var id_cargo_contable in id_cargos_contable)
            {
                var cargo_info = db.C_cargos_contables_g.Find(id_cargo_contable);
                var meses_presupuesto = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
                var cuenta_contable = db.C_cuentas_contables_g.Where(x => x.id_cargo_contable == id_cargo_contable && x.activo == true).ToList();
                decimal presupuesto_asignado = 0;
                foreach (var item in cuenta_contable)
                {
                    PresupuestosOrdenCompra reportes = new PresupuestosOrdenCompra();
                    reportes.id_cargo_contable = cargo_info.id_cargo_contable_g;
                    reportes.nombre_cargo = cargo_info.nombre_cargo;

                    int id_cuenta = item.id_cuenta_contable;
                    reportes.cuenta_string = item.cuenta;
                    reportes.cuenta = item.id_cuenta_contable;
                    reportes.descripcion = item.nombre_cuenta;
                    decimal totales = 0;

                    var consumo_mensual = db.C_compras_ordenes_d.Where(x => x.activo == true && x.C_compras_ordenes_g.activo == true && x.id_cuenta_contable == id_cuenta && x.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g.solicita_autorizacion == false).ToList();
                    //int id_mes = 1;
                    foreach (var meses in meses_presupuesto)
                    {
                        var presupuesto_almacen_cuenta = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_anio == id_anio && x.id_mes == meses.id_mes_presupuesto && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();

                        if (presupuesto_almacen_cuenta != null)
                        {
                            //id_mes = (int)presupuesto_almacen_cuenta.id_mes;
                            presupuesto_asignado = (decimal)presupuesto_almacen_cuenta.valor_presupuesto;

                            if (id_cuenta == 1158)
                            {
                                string hola = "";
                            }

                            int mes_presupuesto = (int)presupuesto_almacen_cuenta.id_mes;

                            DateTime fecha_1 = new DateTime(Convert.ToInt32(anos.anio), (int)presupuesto_almacen_cuenta.id_mes, 1);
                            DateTime fecha_2 = fecha_1.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda == 1).Select(x => (decimal)x.precio_unitario * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }
                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda != 1).Select(x => (valor_dolar * (decimal)x.precio_unitario) * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }

                            if (totales > 0)
                            {
                                consumos.Add((presupuesto_asignado, totales));
                            }
                            else
                            {
                                consumos.Add((presupuesto_asignado, 0));
                            }
                        }
                        else
                        {
                            presupuesto_asignado = 0;
                            int mes_presupuesto = meses.id_mes_presupuesto;
                            //id_mes = id_mes + 1;
                            DateTime fecha_1 = new DateTime(Convert.ToInt32(anos.anio), mes_presupuesto, 1);
                            DateTime fecha_2 = fecha_1.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda == 1).Select(x => (decimal)x.precio_unitario * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }
                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda != 1).Select(x => (valor_dolar * (decimal)x.precio_unitario) * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }


                            if (totales > 0)
                            {
                                consumos.Add((presupuesto_asignado, totales));
                            }
                            else
                            {
                                consumos.Add((presupuesto_asignado, 0));
                            }
                        }
                        totales = 0;
                    }

                    decimal consumo_asignado = 0;
                    decimal consumo_total = 0;



                    var consumo_agrupado = consumos[0];
                    var consumo_principal = consumo_agrupado.Item1;
                    var consumo_secundario = consumo_agrupado.Item2;
                    reportes.ene_asignado = consumo_principal;
                    reportes.ene_consumido = consumo_secundario;
                    reportes.ene_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;


                    consumo_agrupado = consumos[1];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.feb_asignado = consumo_principal;
                    reportes.feb_consumido = consumo_secundario;
                    reportes.feb_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[2];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.mar_asignado = consumo_principal;
                    reportes.mar_consumido = consumo_secundario;
                    reportes.mar_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[3];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.abr_asignado = consumo_principal;
                    reportes.abr_consumido = consumo_secundario;
                    reportes.abr_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[4];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.mayo_asignado = consumo_principal;
                    reportes.mayo_consumido = consumo_secundario;
                    reportes.mayo_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[5];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.jun_asignado = consumo_principal;
                    reportes.jun_consumido = consumo_secundario;
                    reportes.jun_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[6];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.jul_asignado = consumo_principal;
                    reportes.jul_consumido = consumo_secundario;
                    reportes.jul_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[7];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.ago_asignado = consumo_principal;
                    reportes.ago_consumido = consumo_secundario;
                    reportes.ago_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[8];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.sep_asignado = consumo_principal;
                    reportes.sep_consumido = consumo_secundario;
                    reportes.sep_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[9];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.oct_asignado = consumo_principal;
                    reportes.oct_consumido = consumo_secundario;
                    reportes.oct_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[10];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.nov_asignado = consumo_principal;
                    reportes.nov_consumido = consumo_secundario;
                    reportes.nov_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[11];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.dic_asignado = consumo_principal;
                    reportes.dic_consumido = consumo_secundario;
                    reportes.dic_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    reportes.total_asignado = consumo_asignado;
                    reportes.total_consumido = consumo_total;

                    consumo_total = 0;
                    consumo_asignado = 0;

                    data.Add(reportes);

                    consumos.Clear();
                }
            }




            Session["ReporteMesesPresupuestoAlmacen"] = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.id_anio_reporte = id_anio;
            ViewBag.anio_numero = anos.anio;


            return PartialView("ComparativoPresupuestoOrdenesCompra/_ComparativoPresupuestoOrdenesCompra", data);
        }

        public PartialViewResult ConsultarOrdenesEjericdasMesDetalle(int id_cuenta, int no_mes, decimal total_ejercido, decimal total_presupuesto, int anio_numero, string numero_cuenta, string nombre_cuenta, string nombre_cargo)
        {
            DateTime fecha_inicio = new DateTime(anio_numero, no_mes, 01);
            DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
            decimal tipo_cambio = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;

            ViewBag.total_ejercido = total_ejercido;
            ViewBag.total_presupuesto = total_presupuesto;
            ViewBag.anio_numero = anio_numero;
            ViewBag.nombre_cuenta = nombre_cuenta;
            ViewBag.fecha_inicio = fecha_inicio;
            ViewBag.fecha_fin = fecha_fin;
            ViewBag.numero_cuenta = numero_cuenta;
            ViewBag.nombre_cargo = nombre_cargo;

            var ordenes = db.C_compras_ordenes_d.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_inicio && x.C_compras_ordenes_g.fecha_registro <= fecha_fin
            && x.activo == true && x.C_compras_ordenes_g.activo == true && x.id_cuenta_contable == id_cuenta && x.C_compras_ordenes_g.id_usuario_genera != 1).ToList();

            foreach (var orden in ordenes)
            {
                if (orden.id_tipo_moneda != 1)
                {
                    orden.precio_unitario = orden.precio_unitario * tipo_cambio;
                    orden.C_tipos_moneda.clave_fiscal = "MXN-USD";
                }
            }


            string cuenta = db.C_cuentas_contables_g.Find(id_cuenta).cuenta;
            try
            {
                List<C_compras_ordenes_d> OrdenesSab = ConsultarOrdenesEjericdasMesDetalleSab(cuenta, no_mes, anio_numero);
                ordenes.AddRange(OrdenesSab);
            }
            catch (Exception)
            {
            }


            return PartialView("ComparativoPresupuestoOrdenesCompra/_OrdenesEjericdasMesDetalle", ordenes);
        }

        public List<C_compras_ordenes_d> ConsultarOrdenesEjericdasMesDetalleSab(string cuenta, int no_mes, int anio_numero)
        {
            DateTime fecha_inicio = new DateTime(anio_numero, no_mes, 01);
            DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
            decimal tipo_cambio = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;

            List<C_compras_ordenes_d> ordenes_sab = new List<C_compras_ordenes_d>();
            try
            {
                var ordenes_g = db_compras.orden_ge.Where(x => x.rstatus == "A" && x.fecha >= fecha_inicio && x.fecha <= fecha_fin).ToList();
                foreach (var orden_g in ordenes_g)
                {
                    var ordenes_d = db_compras.orden_de.Where(x => x.rstatus == "A" && x.cuenta.Trim() == cuenta.Trim() && x.orden == orden_g.orden).ToList();

                    foreach (var orden in ordenes_d)
                    {
                        C_compras_ordenes_d new_orden = new C_compras_ordenes_d();
                        var info_orden_g = db_compras.orden_ge.Where(x => x.orden == orden.orden).FirstOrDefault();

                        new_orden.C_compras_ordenes_g = new C_compras_ordenes_g();
                        new_orden.C_compras_ordenes_g.C_compras_requi_g = new C_compras_requi_g();
                        new_orden.C_compras_ordenes_g.C_compras_requisiciones_tipos = new C_compras_requisiciones_tipos();
                        new_orden.C_articulos_catalogo = new C_articulos_catalogo();
                        new_orden.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5 = new C_usuarios_corporativo();
                        new_orden.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo = new C_usuarios_corporativo();
                        new_orden.C_compras_ordenes_g.C_usuarios_corporativo = new C_usuarios_corporativo();
                        new_orden.C_compras_ordenes_g.C_compras_requi_g.C_centros_g = new C_centros_g();
                        new_orden.C_compras_ordenes_g.C_compras_proveedores = new C_compras_proveedores();
                        new_orden.C_tipos_moneda = new C_tipos_moneda();
                        new_orden.C_unidades_medidas = new C_unidades_medidas();
                        new_orden.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g = new C_compras_cotizaciones_confirmadas_g();
                        new_orden.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g.C_usuarios_corporativo = new C_usuarios_corporativo();


                        new_orden.C_compras_ordenes_g.fecha_registro = info_orden_g.fecha;
                        new_orden.C_compras_ordenes_g.id_compras_orden_g = Convert.ToInt32(orden.orden);
                        new_orden.C_compras_ordenes_g.C_compras_requi_g.id_requisicion_articulo_g = Convert.ToInt32(info_orden_g.requi);

                        new_orden.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.usuario = "SAB";
                        new_orden.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo.usuario = "SAB";
                        new_orden.C_compras_ordenes_g.C_usuarios_corporativo.usuario = "SAB";
                        new_orden.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g.C_usuarios_corporativo.usuario = "SAB";

                        if (orden.t_mon != "MXN ")
                        {
                            orden.precio = orden.precio * tipo_cambio;
                            orden.t_mon = "MXN-USD";
                        }

                        new_orden.precio_unitario = orden.precio;
                        new_orden.cantidad_compra = orden.cantidad;

                        var info_proveedor = db_compras.proveedo_sab.Where(x => x.prov == info_orden_g.prov).FirstOrDefault();
                        if (info_proveedor != null) { new_orden.C_compras_ordenes_g.C_compras_proveedores.razon_social = info_proveedor.des_prov.Trim(); }
                        else { new_orden.C_compras_ordenes_g.C_compras_proveedores.razon_social = "NO ENCONTRADO"; }

                        string centro_sab = info_orden_g.id_centro.Trim();
                        var centro_sib = db.C_centros_g.Where(x => x.siglas == centro_sab).FirstOrDefault();
                        if (centro_sab != null) { new_orden.C_compras_ordenes_g.C_compras_requi_g.C_centros_g.siglas = centro_sib.siglas; }
                        else { new_orden.C_compras_ordenes_g.C_compras_requi_g.C_centros_g.siglas = "NO ENCONTRADO"; }

                        new_orden.C_compras_ordenes_g.C_compras_cotizaciones_confirmadas_g.solicita_autorizacion = false;


                        string tipo_requi = "";
                        string color_tipo = "";
                        if (info_orden_g.servicio == 0)
                        {
                            tipo_requi = "COMPRA";
                            color_tipo = "blue";
                        }
                        else if (info_orden_g.servicio == 1)
                        {
                            tipo_requi = "SERVICIO";
                            color_tipo = "orange";
                        }
                        else if (info_orden_g.servicio == 4)
                        {
                            tipo_requi = "INVERSION";
                            color_tipo = "green";
                        }
                        else
                        {
                            tipo_requi = "CAJA CHICA";
                            color_tipo = "grey";
                        }
                        new_orden.C_compras_ordenes_g.C_compras_requisiciones_tipos.color = color_tipo;
                        new_orden.C_compras_ordenes_g.C_compras_requisiciones_tipos.tipo_requisicion = tipo_requi;
                        new_orden.id_compras_orden_g = Convert.ToInt32(info_orden_g.orden);
                        new_orden.C_articulos_catalogo.clave = orden.art;
                        new_orden.C_articulos_catalogo.nombre_articulo = orden.des1_art;

                        new_orden.C_unidades_medidas.unidad_medida = orden.uni_art;
                        new_orden.C_tipos_moneda.clave_fiscal = orden.t_mon;
                        ordenes_sab.Add(new_orden);
                    }
                }

                return ordenes_sab;
            }
            catch (Exception)
            {
                return ordenes_sab;
            }
        }

        public string GetLinkDescargarConsolidadoExcel(int id_anio, int id_cargos_contable)
        {
            //return "https://localhost:44371//REPORTES/DescargarConsolidadoExcel?id_anio=" + id_anio + "&&id_cargos_contable=" + id_cargos_contable + "";
            return "https://siib.beta.com.mx/REPORTES/DescargarConsolidadoExcel?id_anio=" + id_anio + "&&id_cargos_contable=" + id_cargos_contable + "";
        }

        public ActionResult DescargarConsolidadoExcel(int id_anio, int[] id_cargos_contable)
        {
            List<PresupuestosOrdenCompra> data = new List<PresupuestosOrdenCompra>();
            List<(decimal, decimal)> consumos = new List<(decimal, decimal)>();

            decimal valor_dolar = (decimal)db.C_parametros_configuracion.Find(1015).valor_numerico;
            var anos = db.C_presupuestos_anios.Find(id_anio);
            if (id_cargos_contable.Contains(0)) { id_cargos_contable = db.C_cargos_contables_g.Where(x => x.activo == true).Select(x => x.id_cargo_contable_g).ToArray(); }

            foreach (var id_cargo_contable in id_cargos_contable)
            {
                var cargo_info = db.C_cargos_contables_g.Find(id_cargo_contable);
                var meses_presupuesto = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
                var cuenta_contable = db.C_cuentas_contables_g.Where(x => x.id_cargo_contable == id_cargo_contable && x.activo == true).ToList();
                decimal presupuesto_asignado = 0;
                foreach (var item in cuenta_contable)
                {
                    PresupuestosOrdenCompra reportes = new PresupuestosOrdenCompra();
                    reportes.id_cargo_contable = cargo_info.id_cargo_contable_g;
                    reportes.nombre_cargo = cargo_info.nombre_cargo;

                    int id_cuenta = item.id_cuenta_contable;
                    reportes.cuenta_string = item.cuenta;
                    reportes.cuenta = item.id_cuenta_contable;
                    reportes.descripcion = item.nombre_cuenta;
                    decimal totales = 0;
                    var consumo_mensual = db.C_compras_ordenes_d.Where(x => x.activo == true && x.C_compras_ordenes_g.activo == true && x.id_cuenta_contable == id_cuenta /*&& x.id_tipo_moneda == 1*/).ToList();
                    //int id_mes = 1;
                    foreach (var meses in meses_presupuesto)
                    {
                        var presupuesto_almacen_cuenta = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_anio == id_anio && x.id_mes == meses.id_mes_presupuesto && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();

                        if (presupuesto_almacen_cuenta != null)
                        {
                            //id_mes = (int)presupuesto_almacen_cuenta.id_mes;
                            presupuesto_asignado = (decimal)presupuesto_almacen_cuenta.valor_presupuesto;

                            if (id_cuenta == 1158)
                            {
                                string hola = "";
                            }

                            int mes_presupuesto = (int)presupuesto_almacen_cuenta.id_mes;

                            DateTime fecha_1 = new DateTime(Convert.ToInt32(anos.anio), (int)presupuesto_almacen_cuenta.id_mes, 1);
                            DateTime fecha_2 = fecha_1.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda == 1).Select(x => (decimal)x.precio_unitario * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }
                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda != 1).Select(x => (valor_dolar * (decimal)x.precio_unitario) * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }

                            if (totales > 0)
                            {
                                consumos.Add((presupuesto_asignado, totales));
                            }
                            else
                            {
                                consumos.Add((presupuesto_asignado, 0));
                            }
                        }
                        else
                        {
                            presupuesto_asignado = 0;
                            int mes_presupuesto = meses.id_mes_presupuesto;
                            //id_mes = id_mes + 1;
                            DateTime fecha_1 = new DateTime(Convert.ToInt32(anos.anio), mes_presupuesto, 1);
                            DateTime fecha_2 = fecha_1.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda == 1).Select(x => (decimal)x.precio_unitario * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }
                            try { totales += consumo_mensual.Where(x => x.C_compras_ordenes_g.fecha_registro >= fecha_1 && x.C_compras_ordenes_g.fecha_registro <= fecha_2 && x.id_tipo_moneda != 1).Select(x => (valor_dolar * (decimal)x.precio_unitario) * (decimal)x.cantidad_compra).Sum(); }
                            catch (Exception) { }


                            if (totales > 0)
                            {
                                consumos.Add((presupuesto_asignado, totales));
                            }
                            else
                            {
                                consumos.Add((presupuesto_asignado, 0));
                            }
                        }
                        totales = 0;
                    }

                    decimal consumo_asignado = 0;
                    decimal consumo_total = 0;



                    var consumo_agrupado = consumos[0];
                    var consumo_principal = consumo_agrupado.Item1;
                    var consumo_secundario = consumo_agrupado.Item2;
                    reportes.ene_asignado = consumo_principal;
                    reportes.ene_consumido = consumo_secundario;
                    reportes.ene_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;


                    consumo_agrupado = consumos[1];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.feb_asignado = consumo_principal;
                    reportes.feb_consumido = consumo_secundario;
                    reportes.feb_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[2];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.mar_asignado = consumo_principal;
                    reportes.mar_consumido = consumo_secundario;
                    reportes.mar_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[3];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.abr_asignado = consumo_principal;
                    reportes.abr_consumido = consumo_secundario;
                    reportes.abr_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[4];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.mayo_asignado = consumo_principal;
                    reportes.mayo_consumido = consumo_secundario;
                    reportes.mayo_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[5];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.jun_asignado = consumo_principal;
                    reportes.jun_consumido = consumo_secundario;
                    reportes.jun_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[6];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.jul_asignado = consumo_principal;
                    reportes.jul_consumido = consumo_secundario;
                    reportes.jul_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[7];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.ago_asignado = consumo_principal;
                    reportes.ago_consumido = consumo_secundario;
                    reportes.ago_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[8];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.sep_asignado = consumo_principal;
                    reportes.sep_consumido = consumo_secundario;
                    reportes.sep_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[9];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.oct_asignado = consumo_principal;
                    reportes.oct_consumido = consumo_secundario;
                    reportes.oct_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[10];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.nov_asignado = consumo_principal;
                    reportes.nov_consumido = consumo_secundario;
                    reportes.nov_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    consumo_agrupado = consumos[11];
                    consumo_principal = consumo_agrupado.Item1;
                    consumo_secundario = consumo_agrupado.Item2;
                    reportes.dic_asignado = consumo_principal;
                    reportes.dic_consumido = consumo_secundario;
                    reportes.dic_disponible = consumo_principal - consumo_secundario;
                    consumo_asignado += consumo_principal;
                    consumo_total += consumo_secundario;

                    reportes.total_asignado = consumo_asignado;
                    reportes.total_consumido = consumo_total;

                    consumo_total = 0;
                    consumo_asignado = 0;

                    data.Add(reportes);

                    consumos.Clear();
                }
            }




            Session["ReporteMesesPresupuestoAlmacen"] = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.id_anio_reporte = id_anio;
            ViewBag.anio_numero = anos.anio;

            string htmlContent = RenderPartialViewToString(data, "ComparativoPresupuestoOrdenesCompra/_ComparativoPresupuestoOrdenesCompraConsolidado");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1);
            // Return the Excel file as a download
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CONSOLIDADO PRESUPUESTO VS REQUISICIONES.xlsx");
        }


        #endregion

        #region REPORTE MENSUAL DE GASTOS COMPRAS/CONSUMO
        public ActionResult ReporteMensualGastosCompraConsumo()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8056)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("GastosMensualesCompraConsumido/Index");
        }

        public PartialViewResult ConsultarGastosMensualesCompraConsumo(int id_cargo_contable, int id_anio, int id_mes, bool anual)
        {
            List<ReportePolizasAlmacenCuentasContables> reportePolizas = new List<ReportePolizasAlmacenCuentasContables>();
            List<DOCTOS_CP_IMPORTES_PROVEEDORES> doctos_ms = new List<DOCTOS_CP_IMPORTES_PROVEEDORES>();

            int no_anio = Convert.ToInt32(db.C_presupuestos_anios.Find(id_anio).anio);
            int no_mes = Convert.ToInt32(db.C_presupuestos_meses.Find(id_mes).no_mes);
            DateTime fecha_inicio = new DateTime(no_anio, no_mes, 01);
            DateTime fecha_fin = fecha_inicio.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            int total_no_cuentas = 0;
            decimal total_consumos = 0;
            decimal total_compras = 0;
            decimal total_concentrado = 0;

            if (anual == true)
            {
                fecha_inicio = new DateTime(no_anio, 01, 01);
                fecha_fin = fecha_inicio.AddYears(1).AddDays(-1).AddHours(23).AddMinutes(59);
            }

            try
            {
                //--------------------- VALES DE SALIDA (SIIB)
                var salidas = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true &&
                           x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_inicio && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_fin &&
                           x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                           x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002 && x.id_cargo_contable_g == id_cargo_contable).ToList();

                //-------------------- ENTRADAS/COMPRAS DIRECTAS (MS)
                var data_doctos = new List<object[]>();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();
                string fecha_inicio_format = fecha_inicio.ToString("dd.MM.yyyy");
                string fecha_fin_format = fecha_fin.ToString("dd.MM.yyyy");

                string query_co_ba = "SELECT DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, " +
                                        " SUM(DOCTOS_CO_DET.IMPORTE) IMPORTE, DOCTOS_CO.TIPO_CAMBIO FROM DOCTOS_CO " +
                                        " INNER JOIN DOCTOS_CO_DET ON DOCTOS_CO_DET.DOCTO_CO_ID = DOCTOS_CO.DOCTO_CO_ID " +
                                        " INNER JOIN CUENTAS_CO ON DOCTOS_CO_DET.CUENTA_ID = CUENTAS_CO.CUENTA_ID " +
                                        " WHERE DOCTOS_CO.FECHA >= @fecha_inicio AND DOCTOS_CO.FECHA <= @fecha_fin AND " +
                                        " DOCTOS_CO_DET.TIPO_ASIENTO = 'C' AND DOCTOS_CO.DESCRIPCION LIKE '%O-%' " +
                                        " GROUP BY DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, DOCTOS_CO.TIPO_CAMBIO;";

                FbCommand readCommand_co = new FbCommand(query_co_ba, conn);
                readCommand_co.Parameters.AddWithValue("@fecha_inicio", fecha_inicio_format);
                readCommand_co.Parameters.AddWithValue("@fecha_fin", fecha_fin_format);
                FbDataReader reader_co = readCommand_co.ExecuteReader();
                while (reader_co.Read())
                {
                    var columns = new object[reader_co.FieldCount];
                    reader_co.GetValues(columns);
                    data_doctos.Add(columns);
                }
                conn.Close();

                for (int i = 0; i < data_doctos.Count(); i++)
                {
                    DOCTOS_CP_IMPORTES_PROVEEDORES docto = new DOCTOS_CP_IMPORTES_PROVEEDORES();
                    docto.Docto_Cp_Id = Convert.ToInt32(data_doctos[i][0].ToString().Trim());
                    docto.Folio = data_doctos[i][1].ToString().Trim();
                    docto.Fecha = DateTime.Parse(data_doctos[i][2].ToString().Trim());
                    //docto.Proveedor_Id = Convert.ToInt32(data_doctos[i][3].ToString().Trim());
                    //docto.Clave_Prov = data_doctos[i][4].ToString().Trim();
                    //docto.Nombre_Proveedor = data_doctos[i][5].ToString().Trim();
                    docto.Descripcion = data_doctos[i][3].ToString().Trim();
                    docto.Cuenta = data_doctos[i][4].ToString().Trim();
                    docto.Cuenta_Concepto = data_doctos[i][5].ToString().Trim();
                    docto.Importe = Convert.ToDecimal(data_doctos[i][6].ToString().Trim());
                    docto.Tipo_Cambio = Convert.ToDecimal(data_doctos[i][7].ToString().Trim());

                    try
                    {
                        if (docto.Tipo_Cambio > 1)
                        {
                            docto.Importe = docto.Importe * docto.Tipo_Cambio;
                        }
                    }
                    catch (Exception) { }

                    doctos_ms.Add(docto);
                }

                var cuentas_contables_g = db.C_cuentas_contables_g.Where(x => x.activo == true && x.id_cargo_contable == id_cargo_contable).OrderBy(x => x.cuenta);
                var cuentas_contables = cuentas_contables_g.Select(x => x.cuenta.Trim()).ToList();

                var cuentas_ms = doctos_ms.Select(z => z.Cuenta).Distinct().ToArray();
                doctos_ms = doctos_ms.Where(x => cuentas_contables.Contains(x.Cuenta)).ToList();

                foreach (var item in cuentas_contables_g.Distinct())
                {
                    try
                    {
                        int inicial_cuenta = Convert.ToInt32(item.cuenta.Split('.')[0].ToString());
                        if (inicial_cuenta > 200)
                        {
                            decimal total_salida = 0;
                            decimal total_entrada = 0;
                            int id_cuenta_contable_g = item.id_cuenta_contable;

                            ReportePolizasAlmacenCuentasContables entrada = new ReportePolizasAlmacenCuentasContables();
                            entrada.Id_cuenta_contable = item.id_cuenta_contable;
                            entrada.Cuenta = item.cuenta;
                            entrada.CuentaContable = item.nombre_cuenta;

                            entrada.Id_cargo_contable = item.C_cargos_contables_g.id_cargo_contable_g;
                            entrada.CargoContable = item.C_cargos_contables_g.nombre_cargo;

                            if (item.cuenta == "615.22.1")
                            {
                                string hola = "";
                            }

                            //----------SALIDA SIIB
                            try
                            {
                                entrada.Importe_salida = (decimal)salidas.Where(x => x.id_cuenta_contable_g == item.id_cuenta_contable && x.id_cargo_contable_g == item.id_cargo_contable)
                                    .Select(x => x.cantidad_entregada * x.costo).Sum();
                            }
                            catch (Exception) { entrada.Importe_salida = 0; }


                            //-----------ENTRADA MICROSIP
                            try
                            {
                                total_entrada = (decimal)doctos_ms.Where(x => x.Cuenta.Trim() == item.cuenta.Trim()).Select(x => x.Importe).Sum();
                                entrada.Importe_entrada = total_entrada;
                            }
                            catch (Exception) { entrada.Importe_entrada = 0; }
                            entrada.Importe_total = entrada.Importe_entrada + entrada.Importe_salida;

                            var valid_presupuesto = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta_contable_g && x.id_mes == id_mes && x.id_anio == id_anio).FirstOrDefault();
                            entrada.Presupuesto = 0;
                            if (valid_presupuesto != null) { entrada.Presupuesto = (decimal)valid_presupuesto.valor_presupuesto; }

                            entrada.Disponible = entrada.Presupuesto - entrada.Importe_total;

                            total_no_cuentas = total_no_cuentas + 1;
                            total_consumos += entrada.Importe_salida;
                            total_compras += entrada.Importe_entrada;
                            total_concentrado += entrada.Importe_total;

                            reportePolizas.Add(entrada);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                ReportePolizasAlmacenCuentasContables totales = new ReportePolizasAlmacenCuentasContables();
                totales.Id_cuenta_contable = total_no_cuentas;
                totales.Cuenta = "TOTAL:";
                totales.CuentaContable = total_no_cuentas.ToString() + " CUENTAS";

                totales.Id_cargo_contable = 0;
                totales.CargoContable = "";

                totales.Importe_salida = total_consumos;
                totales.Importe_entrada = total_compras;
                totales.Importe_total = total_concentrado;
                reportePolizas.Add(totales);


                var fechas = salidas.Select(x => (DateTime)x.C_almacen_solicitudes_mercancia_g.fecha_registro).Union(doctos_ms.Select(x => x.Fecha)).Distinct().OrderByDescending(x => x.Date).FirstOrDefault();
                ViewBag.fecha_inicio = fecha_inicio.ToShortDateString();
                ViewBag.fecha_fin = fecha_fin.ToShortDateString();
                if (fechas != null)
                {
                    ViewBag.fecha_ultimo_registro = fechas.ToShortDateString();
                }
                else
                {
                    ViewBag.fecha_ultimo_registro = fecha_fin.ToShortDateString();
                }

                ViewBag.nombre_cargo = db.C_cargos_contables_g.Find(id_cargo_contable).nombre_cargo;
                return PartialView("GastosMensualesCompraConsumido/_ReporteGastosMensualesCompraConsumido", reportePolizas);
            }
            catch (Exception)
            {
                return PartialView("GastosMensualesCompraConsumido/_ReporteGastosMensualesCompraConsumido", null);
            }


        }

        public PartialViewResult ConsultarDetalleComprasDirectasCuentaContable(int id_cuenta_contable, string fecha_inicio, string fecha_fin)
        {
            //-------------------- ENTRADAS/COMPRAS DIRECTAS (MS)
            try
            {
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                string fecha_inicio_format = fecha_i.ToString("dd.MM.yyyy");
                string fecha_fin_format = fecha_f.ToString("dd.MM.yyyy");

                var data_doctos = new List<object[]>();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();

                string cuenta = db.C_cuentas_contables_g.Find(id_cuenta_contable).cuenta;
                string query_co_ba = "SELECT DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, " +
                                        " SUM(DOCTOS_CO_DET.IMPORTE), DOCTOS_CO.TIPO_CAMBIO FROM DOCTOS_CO_DET " +
                                        " INNER JOIN DOCTOS_CO ON DOCTOS_CO_DET.DOCTO_CO_ID = DOCTOS_CO.DOCTO_CO_ID " +
                                        " INNER JOIN CUENTAS_CO ON DOCTOS_CO_DET.CUENTA_ID = CUENTAS_CO.CUENTA_ID " +
                                        " WHERE DOCTOS_CO.FECHA >= @fecha_inicio AND DOCTOS_CO.FECHA <= @fecha_fin AND" +
                                        " DOCTOS_CO_DET.TIPO_ASIENTO = 'C' AND CUENTAS_CO.CUENTA_PT = @cuenta AND DOCTOS_CO.DESCRIPCION LIKE '%O-%' " +
                                        " GROUP BY DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, DOCTOS_CO.TIPO_CAMBIO;";

                FbCommand readCommand_co = new FbCommand(query_co_ba, conn);
                readCommand_co.Parameters.AddWithValue("@fecha_inicio", fecha_inicio_format);
                readCommand_co.Parameters.AddWithValue("@fecha_fin", fecha_fin_format);
                readCommand_co.Parameters.AddWithValue("@cuenta", cuenta);
                FbDataReader reader_co = readCommand_co.ExecuteReader();
                while (reader_co.Read())
                {
                    var columns = new object[reader_co.FieldCount];
                    reader_co.GetValues(columns);
                    data_doctos.Add(columns);
                }
                conn.Close();
                List<DOCTOS_CP_IMPORTES_PROVEEDORES> doctos_ms = new List<DOCTOS_CP_IMPORTES_PROVEEDORES>();
                for (int i = 0; i < data_doctos.Count(); i++)
                {
                    DOCTOS_CP_IMPORTES_PROVEEDORES docto = new DOCTOS_CP_IMPORTES_PROVEEDORES();
                    docto.Docto_Cp_Id = Convert.ToInt32(data_doctos[i][0].ToString().Trim());
                    docto.Folio = data_doctos[i][1].ToString().Trim();
                    docto.Fecha = DateTime.Parse(data_doctos[i][2].ToString().Trim());
                    //docto.Proveedor_Id = Convert.ToInt32(data_doctos[i][3].ToString().Trim());
                    //docto.Clave_Prov = data_doctos[i][4].ToString().Trim();
                    //docto.Nombre_Proveedor = data_doctos[i][5].ToString().Trim();
                    docto.Descripcion = data_doctos[i][3].ToString().Trim();
                    docto.Cuenta = data_doctos[i][4].ToString().Trim();
                    docto.Cuenta_Concepto = data_doctos[i][5].ToString().Trim();
                    docto.Importe = Convert.ToDecimal(data_doctos[i][6].ToString().Trim());
                    docto.Tipo_Cambio = Convert.ToDecimal(data_doctos[i][7].ToString().Trim());

                    try
                    {
                        string input = docto.Descripcion;
                        string pattern = @"O-(\d+)";
                        Match match = Regex.Match(input, pattern);
                        if (match.Success)
                        {
                            try
                            {
                                string extractedNumber = match.Groups[1].Value;
                                int id_compra_orden_g = Convert.ToInt32(extractedNumber);

                                var orden = db.C_compras_ordenes_g.Find(id_compra_orden_g);
                                //var articulos = orden.C_compras_ordenes_d.Where(x => x.activo == true).Select(x => x.id_articulo).ToArray();

                                int id_requisicion_g = (int)orden.id_requisicion_articulo_g;
                                var obs_art = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requisicion_g /*&& articulos.Contains((int)x.id_articulo)*/ && x.id_cuenta_contable_g == id_cuenta_contable);

                                string observaciones_requi = "";
                                foreach (var item in obs_art.Select(x => x.observacion).ToList())
                                {
                                    if (item != "") { observaciones_requi += item + ", "; }
                                }

                                docto.folio_requisicion = orden.C_centros_g.siglas.ToUpper() + "-" + id_requisicion_g.ToString();
                                docto.fecha_requisicion = (DateTime)orden.C_compras_requi_g.fecha_registro;
                                docto.observaciones = observaciones_requi;
                                docto.Clave_Prov = orden.C_compras_proveedores.cuenta_cxp;
                                docto.Nombre_Proveedor = orden.C_compras_proveedores.razon_social;

                                //docto.Importe = (decimal)obs_art.Select(x => x.precio * x.cantidad).Sum();
                            }
                            catch (Exception)
                            {
                                docto.folio_requisicion = "N/A";
                                docto.observaciones = "N/A";
                            }
                        }
                        else
                        {
                            docto.folio_requisicion = "N/A";
                            docto.observaciones = "N/A";
                        }
                    }
                    catch (Exception)
                    {
                        docto.folio_requisicion = "N/A";
                        docto.observaciones = "N/A";
                    }


                    try
                    {
                        if (docto.Tipo_Cambio > 1)
                        {
                            docto.Importe = docto.Importe * docto.Tipo_Cambio;
                        }
                    }
                    catch (Exception) { }

                    doctos_ms.Add(docto);
                }
                conn.Close();

                //var cuentas_ms = doctos_ms.Select(z => z.Cuenta).Distinct().ToArray();

                ViewBag.nombre_cargo = db.C_cuentas_contables_g.Find(id_cuenta_contable).C_cargos_contables_g.nombre_cargo;
                return PartialView("GastosMensualesCompraConsumido/_DetalleComprasDirectasMicrosip", doctos_ms);
            }
            catch (Exception)
            {
                return PartialView("GastosMensualesCompraConsumido/_DetalleComprasDirectasMicrosip", null);
            }
        }

        public PartialViewResult ConsultarDetalleConsumoAlmacenCuentaContable(int id_cuenta_contable, string fecha_inicio, string fecha_fin, int id_cargo_contable)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            var consumos = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true &&
                            x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_i && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_f &&
                            x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                            x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002 && x.id_cuenta_contable_g == id_cuenta_contable && x.id_cargo_contable_g == id_cargo_contable).ToList();
            return PartialView("GastosMensualesCompraConsumido/_DetalleConsumoAlmacen", consumos);
        }

        public PartialViewResult ConsultarDetalleConsolidadoCuentaContable(int id_cuenta_contable, string fecha_inicio, string fecha_fin, int id_cargo_contable)
        {
            try
            {
                //---------- COMPRA DIRECTA
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                string fecha_inicio_format = fecha_i.ToString("dd.MM.yyyy");
                string fecha_fin_format = fecha_f.ToString("dd.MM.yyyy");

                var data_doctos = new List<object[]>();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();

                //string query_co_ba = "SELECT DOCTOS_CP.DOCTO_CP_ID, DOCTOS_CP.FOLIO, DOCTOS_CP.FECHA, DOCTOS_CP.PROVEEDOR_ID, DOCTOS_CP.CLAVE_PROV, PROVEEDORES.NOMBRE, DOCTOS_CP.DESCRIPCION, " +
                //                        "DOCTOS_CP.CUENTA_CONCEPTO, CUENTAS_CO.NOMBRE, IMPORTES_DOCTOS_CP.IMPORTE, DOCTOS_CP.TIPO_CAMBIO " +
                //                                " FROM DOCTOS_CP " +
                //                                " INNER JOIN IMPORTES_DOCTOS_CP ON DOCTOS_CP.DOCTO_CP_ID = IMPORTES_DOCTOS_CP.DOCTO_CP_ID " +
                //                                " INNER JOIN PROVEEDORES ON DOCTOS_CP.PROVEEDOR_ID = PROVEEDORES.PROVEEDOR_ID " +
                //                                " INNER JOIN CUENTAS_CO ON CUENTAS_CO.CUENTA_PT = DOCTOS_CP.CUENTA_CONCEPTO " +
                //                                " WHERE  DOCTOS_CP.FECHA >= @fecha_inicio AND DOCTOS_CP.FECHA <= @fecha_fin AND DOCTOS_CP.NATURALEZA_CONCEPTO = 'C' AND DOCTOS_CP.DESCRIPCION LIKE '%O-%';";

                string cuenta = db.C_cuentas_contables_g.Find(id_cuenta_contable).cuenta;
                string query_co_ba = "SELECT DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER FOLIO, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, " +
                                        " SUM(DOCTOS_CO_DET.IMPORTE) IMPORTE, DOCTOS_CO.TIPO_CAMBIO FROM DOCTOS_CO_DET " +
                                        " INNER JOIN DOCTOS_CO ON DOCTOS_CO_DET.DOCTO_CO_ID = DOCTOS_CO.DOCTO_CO_ID " +
                                        " INNER JOIN CUENTAS_CO ON DOCTOS_CO_DET.CUENTA_ID = CUENTAS_CO.CUENTA_ID " +
                                        " WHERE DOCTOS_CO.FECHA >= @fecha_inicio AND DOCTOS_CO.FECHA <= @fecha_fin AND" +
                                        " DOCTOS_CO_DET.TIPO_ASIENTO = 'C' AND CUENTAS_CO.CUENTA_PT = @cuenta AND DOCTOS_CO.DESCRIPCION LIKE '%O-%' " +
                                        " GROUP BY DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, DOCTOS_CO.TIPO_CAMBIO;";

                FbCommand readCommand_co = new FbCommand(query_co_ba, conn);
                readCommand_co.Parameters.AddWithValue("@fecha_inicio", fecha_inicio_format);
                readCommand_co.Parameters.AddWithValue("@fecha_fin", fecha_fin_format);
                readCommand_co.Parameters.AddWithValue("@cuenta", cuenta);
                FbDataReader reader_co = readCommand_co.ExecuteReader();
                while (reader_co.Read())
                {
                    var columns = new object[reader_co.FieldCount];
                    reader_co.GetValues(columns);
                    data_doctos.Add(columns);
                }
                conn.Close();
                List<GastosMensualesConsolidadoConsumoCompra> doctos_ms = new List<GastosMensualesConsolidadoConsumoCompra>();
                for (int i = 0; i < data_doctos.Count(); i++)
                {
                    GastosMensualesConsolidadoConsumoCompra docto = new GastosMensualesConsolidadoConsumoCompra();
                    docto.Docto_Cp_Id = Convert.ToInt32(data_doctos[i][0].ToString().Trim());
                    docto.Folio = data_doctos[i][1].ToString().Trim();
                    docto.Fecha = DateTime.Parse(data_doctos[i][2].ToString().Trim());
                    //docto.Proveedor_Id = Convert.ToInt32(data_doctos[i][3].ToString().Trim());
                    //docto.Clave_Prov = data_doctos[i][4].ToString().Trim();
                    //docto.Nombre_Proveedor = data_doctos[i][5].ToString().Trim();
                    docto.Descripcion = data_doctos[i][3].ToString().Trim();
                    docto.Cuenta = data_doctos[i][4].ToString().Trim();
                    docto.Cuenta_Concepto = data_doctos[i][5].ToString().Trim();
                    docto.Importe = Convert.ToDecimal(data_doctos[i][6].ToString().Trim());
                    docto.Tipo_Cambio = Convert.ToDecimal(data_doctos[i][7].ToString().Trim());
                    try
                    {
                        string input = docto.Descripcion;
                        string pattern = @"O-(\d+)";
                        Match match = Regex.Match(input, pattern);
                        if (match.Success)
                        {
                            try
                            {
                                string extractedNumber = match.Groups[1].Value;
                                int id_compra_orden_g = Convert.ToInt32(extractedNumber);
                                if (id_compra_orden_g == 7526)
                                {
                                    string hola = "";
                                }

                                var orden = db.C_compras_ordenes_g.Find(id_compra_orden_g);

                                int id_requisicion_g = (int)orden.id_requisicion_articulo_g;
                                var obs_art = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.id_cuenta_contable_g == id_cuenta_contable);

                                string observaciones_requi = "";
                                foreach (var item in obs_art.Select(x => x.observacion).ToList())
                                {
                                    if (item != "") { observaciones_requi += item + ", "; }
                                }
                                docto.Usuario_registra = orden.C_usuarios_corporativo.C_empleados.nombres.ToUpper() + " " + orden.C_usuarios_corporativo.C_empleados.apellido_paterno.ToUpper();
                                docto.Folio_requisicion = orden.C_centros_g.siglas.ToUpper() + "-" + id_requisicion_g.ToString();
                                docto.Fecha_requisicion = (DateTime)orden.C_compras_requi_g.fecha_registro;
                                docto.Observaciones = observaciones_requi;
                                //docto.Importe = (decimal)obs_art.Select(x => x.precio * x.cantidad).Sum();

                                docto.Clave_Prov = orden.C_compras_proveedores.cuenta_cxp;
                                docto.Nombre_Proveedor = orden.C_compras_proveedores.razon_social;
                            }
                            catch (Exception)
                            {
                                docto.Folio_requisicion = "N/A";
                                docto.Observaciones = "N/A";
                            }
                        }
                        else
                        {
                            docto.Folio_requisicion = "N/A";
                            docto.Observaciones = "N/A";
                        }
                    }
                    catch (Exception)
                    {
                        docto.Folio_requisicion = "N/A";
                        docto.Observaciones = "N/A";
                    }


                    try
                    {
                        if (docto.Tipo_Cambio > 1)
                        {
                            docto.Importe = docto.Importe * docto.Tipo_Cambio;
                        }
                    }
                    catch (Exception) { }

                    doctos_ms.Add(docto);
                }
                conn.Close();

                //var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.id_cuenta_contable == id_cuenta_contable).Select(x => x.cuenta.Trim()).Distinct().ToArray();
                //doctos_ms = doctos_ms.Where(x => cuentas_contables.Contains(x.Cuenta.Trim())).ToList();


                //------------------- CONSUMO DE ALMACÉN

                var consumos = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true &&
                                x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_i && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_f &&
                                x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                                x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002 && x.id_cuenta_contable_g == id_cuenta_contable && x.id_cargo_contable_g == id_cargo_contable).ToList();
                foreach (var item in consumos)
                {
                    GastosMensualesConsolidadoConsumoCompra salida = new GastosMensualesConsolidadoConsumoCompra();
                    salida.Docto_Cp_Id = 0;
                    salida.CargoContable = item.C_cargos_contables_g.nombre_cargo;
                    salida.Concepto = item.C_almacen_solicitudes_mercancia_g.cargo_desc;
                    salida.Folio = item.id_solicitud_mercancia_g.ToString();
                    salida.Clave_Art = item.C_articulos_catalogo.clave;
                    salida.Articulo = item.C_articulos_catalogo.nombre_articulo;
                    salida.Cantidad = (decimal)item.cantidad_entregada;
                    salida.Costo = (decimal)item.costo;
                    salida.Importe_articulo = salida.Cantidad * salida.Costo;
                    salida.Descripcion = item.C_almacen_solicitudes_mercancia_g.cargo_desc;
                    salida.Cuenta = item.C_cuentas_contables_g.cuenta;
                    salida.Cuenta_Concepto = item.C_cuentas_contables_g.nombre_cuenta;
                    salida.Fecha = (DateTime)item.C_almacen_solicitudes_mercancia_g.fecha_registro;
                    salida.Almacen = item.C_almacen_solicitudes_mercancia_g.C_almacen_almacenes_g.siglas;
                    salida.Usuario_registra = item.C_almacen_solicitudes_mercancia_g.C_usuarios_corporativo.C_empleados.nombres.ToUpper() + " " + item.C_almacen_solicitudes_mercancia_g.C_usuarios_corporativo.C_empleados.apellido_paterno;
                    doctos_ms.Add(salida);
                }


                ViewBag.nombre_cargo = db.C_cuentas_contables_g.Find(id_cuenta_contable).C_cargos_contables_g.nombre_cargo;



                return PartialView("GastosMensualesCompraConsumido/_DetalleConsolidadoAlmacenCompras", doctos_ms.OrderBy(x => x.Docto_Cp_Id).ToList());
            }

            catch (Exception)
            {
                return PartialView("GastosMensualesCompraConsumido/_DetalleConsolidadoAlmacenCompras", null);
            }
        }

        public ActionResult DescargarExcelConsolidadoComprasDirectasCargoContable(int id_cargo_contable, int id_anio, int id_mes, bool anual)
        {
            try
            {
                List<GastosMensualesConsolidadoConsumoCompra> doctos_ms = new List<GastosMensualesConsolidadoConsumoCompra>();

                int no_anio = Convert.ToInt32(db.C_presupuestos_anios.Find(id_anio).anio);
                int no_mes = Convert.ToInt32(db.C_presupuestos_meses.Find(id_mes).no_mes);
                DateTime fecha_i = new DateTime(no_anio, no_mes, 01);
                DateTime fecha_f = fecha_i.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
                if (anual == true)
                {
                    fecha_i = new DateTime(no_anio, 01, 01);
                    fecha_f = fecha_i.AddYears(1).AddDays(-1).AddHours(23).AddMinutes(59);
                }

                //---------- COMPRA DIRECTA
                string fecha_inicio_format = fecha_i.ToString("dd.MM.yyyy");
                string fecha_fin_format = fecha_f.ToString("dd.MM.yyyy");

                var data_doctos = new List<object[]>();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();
                var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.id_cargo_contable == id_cargo_contable && x.activo == true).Distinct().ToList();
                string result = "'" + string.Join("','", cuentas_contables.Select(x => x.cuenta).ToList()) + "'";


                var id_cuentas_contables = cuentas_contables.Select(z => (int)z.id_cuenta_contable).ToArray();
                string query_co_ba = "SELECT DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER FOLIO, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, " +
                                        " SUM(DOCTOS_CO_DET.IMPORTE) IMPORTE, DOCTOS_CO.TIPO_CAMBIO FROM DOCTOS_CO_DET " +
                                        " INNER JOIN DOCTOS_CO ON DOCTOS_CO_DET.DOCTO_CO_ID = DOCTOS_CO.DOCTO_CO_ID " +
                                        " INNER JOIN CUENTAS_CO ON DOCTOS_CO_DET.CUENTA_ID = CUENTAS_CO.CUENTA_ID " +
                                        " WHERE DOCTOS_CO.FECHA >= @fecha_inicio AND DOCTOS_CO.FECHA <= @fecha_fin AND" +
                                        " DOCTOS_CO_DET.TIPO_ASIENTO = 'C' AND CUENTAS_CO.CUENTA_PT IN (" + result + ") " +
                                        " AND DOCTOS_CO.DESCRIPCION LIKE '%O-%' " +
                                        " GROUP BY DOCTOS_CO.DOCTO_CO_ID, DOCTOS_CO_DET.REFER, DOCTOS_CO.FECHA, DOCTOS_CO.DESCRIPCION, CUENTAS_CO.CUENTA_PT, CUENTAS_CO.NOMBRE, DOCTOS_CO.TIPO_CAMBIO;";

                FbCommand readCommand_co = new FbCommand(query_co_ba, conn);
                readCommand_co.Parameters.AddWithValue("@fecha_inicio", fecha_inicio_format);
                readCommand_co.Parameters.AddWithValue("@fecha_fin", fecha_fin_format);
                //readCommand_co.Parameters.AddWithValue("@cuenta", result);
                FbDataReader reader_co = readCommand_co.ExecuteReader();
                while (reader_co.Read())
                {
                    var columns = new object[reader_co.FieldCount];
                    reader_co.GetValues(columns);
                    data_doctos.Add(columns);
                }
                for (int i = 0; i < data_doctos.Count(); i++)
                {
                    GastosMensualesConsolidadoConsumoCompra docto = new GastosMensualesConsolidadoConsumoCompra();
                    docto.Docto_Cp_Id = Convert.ToInt32(data_doctos[i][0].ToString().Trim());
                    docto.Folio = data_doctos[i][1].ToString().Trim();
                    docto.Fecha = DateTime.Parse(data_doctos[i][2].ToString().Trim());
                    //docto.Proveedor_Id = Convert.ToInt32(data_doctos[i][3].ToString().Trim());
                    //docto.Clave_Prov = data_doctos[i][4].ToString().Trim();
                    //docto.Nombre_Proveedor = data_doctos[i][5].ToString().Trim();
                    docto.Descripcion = data_doctos[i][3].ToString().Trim();
                    docto.Cuenta = data_doctos[i][4].ToString().Trim();
                    docto.Cuenta_Concepto = data_doctos[i][5].ToString().Trim();
                    docto.Importe = Convert.ToDecimal(data_doctos[i][6].ToString().Trim());
                    docto.Tipo_Cambio = Convert.ToDecimal(data_doctos[i][7].ToString().Trim());

                    try
                    {
                        string input = docto.Descripcion;
                        string pattern = @"O-(\d+)";
                        Match match = Regex.Match(input, pattern);
                        if (match.Success)
                        {
                            try
                            {
                                string extractedNumber = match.Groups[1].Value;
                                int id_compra_orden_g = Convert.ToInt32(extractedNumber);
                                if (id_compra_orden_g == 7526)
                                {
                                    string hola = "";
                                }

                                var orden = db.C_compras_ordenes_g.Find(id_compra_orden_g);

                                int id_requisicion_g = (int)orden.id_requisicion_articulo_g;
                                var obs_art = db.C_compras_requi_d.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && id_cuentas_contables.Contains((int)x.id_cuenta_contable_g));

                                string observaciones_requi = "";
                                foreach (var item in obs_art.Select(x => x.observacion).ToList())
                                {
                                    if (item != "") { observaciones_requi += item + ", "; }
                                }

                                docto.Folio_requisicion = orden.C_centros_g.siglas.ToUpper() + "-" + id_requisicion_g.ToString();
                                docto.Fecha_requisicion = (DateTime)orden.C_compras_requi_g.fecha_registro;
                                docto.Observaciones = observaciones_requi;
                                //docto.Importe = (decimal)obs_art.Select(x => x.precio * x.cantidad).Sum();

                                docto.Clave_Prov = orden.C_compras_proveedores.cuenta_cxp;
                                docto.Nombre_Proveedor = orden.C_compras_proveedores.razon_social;
                            }
                            catch (Exception)
                            {
                                docto.Folio_requisicion = "N/A";
                                docto.Observaciones = "N/A";
                            }
                        }
                        else
                        {
                            docto.Folio_requisicion = "N/A";
                            docto.Observaciones = "N/A";
                        }
                    }
                    catch (Exception)
                    {
                        docto.Folio_requisicion = "N/A";
                        docto.Observaciones = "N/A";
                    }


                    try
                    {
                        if (docto.Tipo_Cambio > 1)
                        {
                            docto.Importe = docto.Importe * docto.Tipo_Cambio;
                        }
                    }
                    catch (Exception) { }

                    doctos_ms.Add(docto);
                }


                //------------------- CONSUMO DE ALMACÉN
                var consumos = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true &&
                                x.C_almacen_solicitudes_mercancia_g.fecha_registro >= fecha_i && x.C_almacen_solicitudes_mercancia_g.fecha_registro <= fecha_f &&
                                x.C_articulos_catalogo.C_articulos_tipos.cuenta != "" && x.cantidad_entregada > 0 && x.C_almacen_solicitudes_mercancia_g.activo == true &&
                                x.C_almacen_solicitudes_mercancia_g.id_status_solicitud != 1002 && id_cuentas_contables.Contains((int)x.id_cuenta_contable_g) && x.id_cargo_contable_g == id_cargo_contable).ToList();
                foreach (var item in consumos)
                {
                    GastosMensualesConsolidadoConsumoCompra salida = new GastosMensualesConsolidadoConsumoCompra();
                    salida.Docto_Cp_Id = 0;
                    salida.CargoContable = item.C_cargos_contables_g.nombre_cargo;
                    salida.Concepto = item.C_almacen_solicitudes_mercancia_g.cargo_desc;
                    salida.Folio = item.id_solicitud_mercancia_g.ToString();
                    salida.Clave_Art = item.C_articulos_catalogo.clave;
                    salida.Articulo = item.C_articulos_catalogo.nombre_articulo;
                    salida.Cantidad = (decimal)item.cantidad_entregada;
                    salida.Costo = (decimal)item.costo;
                    salida.Importe_articulo = salida.Cantidad * salida.Costo;
                    salida.Descripcion = item.C_almacen_solicitudes_mercancia_g.cargo_desc;
                    salida.Cuenta = item.C_cuentas_contables_g.cuenta;
                    salida.Cuenta_Concepto = item.C_cuentas_contables_g.nombre_cuenta;
                    salida.Fecha = (DateTime)item.C_almacen_solicitudes_mercancia_g.fecha_registro;
                    salida.Almacen = item.C_almacen_solicitudes_mercancia_g.C_almacen_almacenes_g.siglas;

                    doctos_ms.Add(salida);
                }
                conn.Close();

                ViewBag.modo = 2;
                string nombre_cargo = db.C_cargos_contables_g.Find(id_cargo_contable).nombre_cargo.ToUpper();
                string encabezado = "REPORTE DE GASTOS MENSUALES - " + nombre_cargo + "";
                string htmlContent = RenderPartialViewToStringObj(doctos_ms, "GastosMensualesCompraConsumido/_ExcelConsolidadoComprasDirectasCargoContable");
                byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 14, encabezado);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GASTOS MENSUALES " + nombre_cargo + ".xlsx");
            }

            catch (Exception)
            {
                ViewBag.modo = 2;
                string nombre_cargo = db.C_cargos_contables_g.Find(id_cargo_contable).nombre_cargo.ToUpper();
                string encabezado = "REPORTE DE GASTOS MENSUALES - " + nombre_cargo + "";
                string htmlContent = RenderPartialViewToStringObj(null, "GastosMensualesCompraConsumido/_ExcelConsolidadoComprasDirectasCargoContable");
                byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 14, encabezado);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GASTOS MENSUALES " + nombre_cargo + ".xlsx");
            }
        }


        #endregion

        #region REPORTE ENTRADA VS RECEPCION
        public ActionResult EntradasVsRecepcionOrdenes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8066)) { return View("/Views/Home/Index.cshtml"); }

                return View("../REPORTES/ReporteEntradaOrdenes/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarEntradasVsRecepcionOrdenesTable(int id_almacen, DateTime fecha_inicio, DateTime fecha_fin)
        {
            ViewBag.modo = 1;
            string f1 = fecha_inicio.Date.ToString("yyyy/MM/dd");
            string f2 = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59).ToString("yyyy/MM/dd HH:mm:ss");
            DataTable dt = new DataTable();
            using (SqlCommand sqlCmd = new SqlCommand())
            {
                try
                {
                    SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                    //string cadena = @"Data Source=192.168.128.2;Initial Catalog=BETA_CORP;Persist Security Info=True;User ID=sa;Password=12345";
                    //SqlConnection mSqlCnn = new SqlConnection(cadena);
                    sqlCmd.CommandText = "usp_1_ALMACEN_ReporteEntradas_vs_RecepcionOrdenes";
                    sqlCmd.Connection = mSqlCnn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 300000;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = f1;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = f2;
                    sqlCmd.Parameters.Add(new SqlParameter("@id_almacenes", SqlDbType.VarChar)).Value = id_almacen;
                    sqlCmd.Parameters.Add(new SqlParameter("@modo", SqlDbType.VarChar)).Value = 1;  //TODOS
                    //mSqlCnn.Open();
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    mSqlCnn.Close();
                }
                catch (Exception ex)
                {
                    dt = null;
                }
            }
            return PartialView("../REPORTES/ReporteEntradaOrdenes/_ConsultarEntradasVsRecepcionOrdenesTable", dt);
        }

        public ActionResult DescargarExcelFormatoEntradasVsRecepcionOrdenes(int id_almacen, string fecha_inicio, string fecha_fin, int modo, int formato, int tamano)
        {

            int id_usuario = (int)Session["LoggedId"];
            string encabezado = "REPORTE ENTRADA VS RECEPCION ORDENES";
            string almacen = "";
            if (id_almacen == 0)
            {
                foreach (var item in db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario).Select(x => x.id_almacen_g).ToList())
                {
                    almacen += item.Value.ToString() + ",";
                }
            }
            else
            {
                almacen = id_almacen.ToString();
            }




            ViewBag.modo = modo;
            DataTable dt = new DataTable();
            using (SqlCommand sqlCmd = new SqlCommand())
            {
                try
                {
                    SqlConnection mSqlCnn = ConexionSIIB.Conectar(); 
                    //string cadena = @"Data Source=192.168.128.2;Initial Catalog=BETA_CORP;Persist Security Info=True;User ID=sa;Password=12345";
                    //SqlConnection mSqlCnn = new SqlConnection(cadena);
                    sqlCmd.CommandText = "usp_1_ALMACEN_ReporteEntradas_vs_RecepcionOrdenes";
                    sqlCmd.Connection = mSqlCnn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 300000;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = fecha_inicio;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = fecha_fin;
                    sqlCmd.Parameters.Add(new SqlParameter("@id_almacenes", SqlDbType.VarChar)).Value = almacen;
                    sqlCmd.Parameters.Add(new SqlParameter("@modo", SqlDbType.VarChar)).Value = 1;  //TODOS
                    //mSqlCnn.Open();
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    mSqlCnn.Close();
                }
                catch (Exception ex)
                {
                    dt = null;
                }
            }


            string htmlContent = RenderPartialViewToStringObj(dt, "../REPORTES/ReporteEntradaOrdenes/_ReporteEntradaRecepcionOrdenesExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, formato, tamano, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Entrada_VS_Recepcion.xlsx");
        }
        #endregion

        #region REPORTE DE REQUISICIONES
        public ActionResult ReporteRequisicionesOrdenesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8067)) { return View("/Views/Home/Index.cshtml"); }

                return View("../REPORTES/ReporteRequisicionesOrdenesCompra/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarReporteRequisicionesOrdenesCompraTable(int id_centro, DateTime fecha_inicio, DateTime fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            string centro = "";
            if (id_centro == 0)
            {
                foreach (var item in db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario).Select(x => x.id_centro).ToList())
                {
                    centro += item.Value.ToString() + ",";
                }
            }

            string f1 = fecha_inicio.Date.ToString("yyyy/MM/dd HH:mm:ss");
            string f2 = fecha_fin.AddHours(23).AddMinutes(59).ToString("yyyy/MM/dd HH:mm:ss"); ;

            DataTable dt = new DataTable();
            using (SqlCommand sqlCmd = new SqlCommand())
            {
                try
                {
                    SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                    //string cadena = @"Data Source=192.168.128.2;Initial Catalog=BETA_CORP;Persist Security Info=True;User ID=sa;Password=12345";
                    //SqlConnection mSqlCnn = new SqlConnection(cadena);
                    sqlCmd.CommandText = "usp_2_REQUISICIONES_ReporteRequisicionesOrdenesCompra";
                    sqlCmd.Connection = mSqlCnn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 300000;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = f1;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = f2;
                    sqlCmd.Parameters.Add(new SqlParameter("@id_centro", SqlDbType.VarChar)).Value = centro;
                    //mSqlCnn.Open();
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    mSqlCnn.Close();
                }
                catch (Exception ex)
                {
                    dt = null;
                }
            }
            return PartialView("../REPORTES/ReporteRequisicionesOrdenesCompra/_ConsultarReporteRequisicionesOrdenesCompraTable", dt);
        }

        public ActionResult DescargarExcelFormatoReporteRequisicionesOrdenesCompra(int id_centro, string fecha_inicio, string fecha_fin, int modo, int formato, int tamano)
        {

            int id_usuario = (int)Session["LoggedId"];
            string encabezado = "REPORTE DE REQUISICIONES Y ORDENES DE COMPRA";
            string centro = "";
            if (id_centro == 0)
            {
                foreach (var item in db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario).Select(x => x.id_centro).ToList())
                {
                    centro += item.Value.ToString() + ",";
                }
            }

            ViewBag.modo = modo;
            DataTable dt = new DataTable();
            using (SqlCommand sqlCmd = new SqlCommand())
            {
                try
                {
                    //string cadena = @"Data Source=192.168.128.2;Initial Catalog=BETA_CORP;Persist Security Info=True;User ID=sa;Password=12345";
                    //SqlConnection mSqlCnn = new SqlConnection(cadena);

                    SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                    sqlCmd.CommandText = "usp_2_REQUISICIONES_ReporteRequisicionesOrdenesCompra";
                    sqlCmd.Connection = mSqlCnn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 300000;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = fecha_inicio;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = fecha_fin;
                    sqlCmd.Parameters.Add(new SqlParameter("@id_centro", SqlDbType.VarChar)).Value = centro;
                    //mSqlCnn.Open();
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    mSqlCnn.Close();
                }
                catch (Exception ex)
                {
                    dt = null;
                }
            }


            string htmlContent = RenderPartialViewToStringObj(dt, "../REPORTES/ReporteRequisicionesOrdenesCompra/_ReporteRequisicionesOrdenesCompraExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, formato, tamano, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte_Requi_Orden.xlsx");
        }
        #endregion


        #region REPORTE ORDENES DE COMPRA
        public ActionResult ReporteOrdenesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(9106)) { return View("/Views/Home/Index.cshtml"); }

                return View("../REPORTES/ReporteOrdenesCompra/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarReporteOrdenesCompraTable(int[] id_clasificaciones, DateTime fecha_inicio, DateTime fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            string clasificaciones = string.Join(",", id_clasificaciones);

            string f1 = fecha_inicio.Date.ToString("yyyy/MM/dd HH:mm:ss");
            string f2 = fecha_fin.AddHours(23).AddMinutes(59).ToString("yyyy/MM/dd HH:mm:ss"); ;

            DataTable dt = new DataTable();
            using (SqlCommand sqlCmd = new SqlCommand())
            {
                try
                {
                    SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                    sqlCmd.CommandText = "usp_2_REQUISICIONES_OrdenesCompra";
                    sqlCmd.Connection = mSqlCnn;
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 300000;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = f1;
                    sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = f2;
                    sqlCmd.Parameters.Add(new SqlParameter("@id_clasificaciones", SqlDbType.VarChar)).Value = clasificaciones;
                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    mSqlCnn.Close();
                }
                catch (Exception ex)
                {
                    dt = null;
                }
            }
            return PartialView("../REPORTES/ReporteOrdenesCompra/_ConsultarReporteOrdenesCompraTable", dt);
        }

        #endregion





        #region CREACION DE EXCEL MEDIANTE VISTA PARCIAL
        private string RenderPartialViewToStringObj(object model, string path_partial_view)
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
            worksheet.Columns().AdjustToContents();
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

        #region REPORTE DE SEGUIMIENTO DE REQUISICIONES

        public ActionResult ReporteSeguimientoRequisiciones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8069)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("SeguimientoRequisiciones/Index");
        }

        private List<ReporteSeguimientoRequisiciones> ConsultarSeguimientoHeader(int[] id_usuario, DateTime  fecha_i, DateTime fecha_f)
        {
            var requisiciones = db.C_compras_requi_g.Where(x => id_usuario.Contains((int)x.id_usuario_registro) && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f).ToList();

            List<ReporteSeguimientoRequisiciones> data = new List<ReporteSeguimientoRequisiciones>();
            foreach (var item in requisiciones)
            {
                int id_requisicion_g = (int)item.id_requisicion_articulo_g;
                ReporteSeguimientoRequisiciones obj = new ReporteSeguimientoRequisiciones();
                if (item.activo == false) { obj.estatus_requisicion = "CANCELADA"; }

                obj.id_requisicion = id_requisicion_g;
                obj.id_centro = (int)item.id_centro_g;
                obj.centro = item.C_centros_g.siglas;
                obj.concepto = item.concepto;
                obj.tipo_requisicion = item.C_compras_requisiciones_tipos.tipo_requisicion;
                obj.id_tipo_requisicion = (int)item.id_requisicion_tipo;
                obj.color_tipo_requisicion = item.C_compras_requisiciones_tipos.color;
                obj.fecha_registro = (DateTime)item.fecha_registro;

                obj.id_usuario_registra = (int)item.id_usuario_registro;
                obj.usuario_registra = item.C_usuarios_corporativo5.usuario;

                obj.fecha_orden = DateTime.MinValue;
                obj.fecha_recepcion = DateTime.MinValue;
                obj.fecha_cotiza = DateTime.MinValue;

                //------------ VALIDA (1 FIRMA)
                if (item.C_usuarios_corporativo != null)
                {
                    obj.id_usuario_revisa = (int)item.aut_2_usuario;
                    obj.usuario_revisa = item.C_usuarios_corporativo.usuario;
                    obj.fecha_revisa = (DateTime)item.aut_2_fecha;
                    obj.tiempo_revisa = (DateTime)item.aut_2_fecha - (DateTime)item.fecha_registro;
                    if (item.activo == true) { obj.estatus_requisicion = "1RA FIRMA"; }
                }
                else
                {
                    obj.id_usuario_revisa = 0;
                    obj.usuario_revisa = "N/A";
                    if (item.activo == true) { obj.estatus_requisicion = "REGISTRADA"; }
                }

                //------------ COTIZACIONES
                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true).Distinct().ToList();
                if (cotizaciones.Count() > 0)
                {
                    obj.no_cotizaciones = cotizaciones.Count();
                    if (item.activo == true) { obj.estatus_requisicion = "COTIZADA"; }
                }
                else { obj.no_cotizaciones = 0; }

                //------------ PARCIALIDADES 
                var parcialidades = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true && x.confirmada == true).Distinct().ToList();
                if (parcialidades.Count() > 0)
                {
                    obj.no_parcialidades = parcialidades.Count();

                    if (parcialidades.Select(x => x.solicita_autorizacion).Contains(true)) { obj.solicita_autorizacion = true; }
                    else { obj.solicita_autorizacion = false; }

                    obj.id_usuario_cotiza = (int)parcialidades.FirstOrDefault().C_usuarios_corporativo.id_usuario_corporativo;
                    obj.usuario_cotiza = parcialidades.FirstOrDefault().C_usuarios_corporativo.usuario;
                    obj.fecha_cotiza = (DateTime)parcialidades.FirstOrDefault().fecha_registro;

                    obj.tiempo_cotizacion = obj.fecha_cotiza - obj.fecha_revisa;

                    if (item.activo == true && obj.solicita_autorizacion == true) { obj.estatus_requisicion = "AUT. DIRECCION"; }
                    else if (item.activo == true && obj.solicita_autorizacion == false) { obj.estatus_requisicion = "2DA FIRMA"; }
                }
                else
                {
                    obj.no_parcialidades = 0;
                    obj.solicita_autorizacion = false;
                    obj.id_usuario_cotiza = 0;
                    obj.fecha_cotiza = DateTime.MinValue;
                    obj.tiempo_cotizacion = TimeSpan.MinValue;
                    obj.usuario_cotiza = "N/A";
                    if (item.activo == true) { obj.estatus_requisicion = "POR PARCIALIZAR"; }
                }

                //------------ ORDENES DE COMPRA
                if (item.C_compras_ordenes_g.Where(x => x.activo == true).ToList().Count() > 0)
                {
                    string ordenes_string = "";
                    string facturas_string = "";
                    string proveedores_ordenes = "";
                    foreach (var ordenes in item.C_compras_ordenes_g.Where(x => x.activo == true).ToList())
                    {
                        ordenes_string += ordenes.id_compras_orden_g + ", ";
                        if (!proveedores_ordenes.Contains(ordenes.C_compras_proveedores.razon_social))
                        {
                            proveedores_ordenes += ordenes.C_compras_proveedores.razon_social + ",";
                        }

                        if (ordenes.C_compras_facturas_proveedores.Where(x => x.activo == true).Count() > 0)
                        {
                            foreach (var factura in ordenes.C_compras_facturas_proveedores.Where(x => x.activo == true).ToList())
                            {
                                facturas_string += factura.folio_factura + ", ";
                                if (factura.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true).Count() > 0) { obj.importada = true; }
                                else { obj.importada = false; }
                            }
                        }
                        obj.fecha_orden = (DateTime)ordenes.fecha_registro;

                        if (ordenes.fecha_recepcion != null) {
                            obj.fecha_recepcion = (DateTime)ordenes.fecha_recepcion;
                            if (ordenes.id_usuario_recepciona != null) { obj.usuario_recepcion = ordenes.C_usuarios_corporativo1.usuario; }
                        }
                        else
                        {
                            var valid_recepcion_almacen = db.C_inventario_almacen_mov_g.Where(x => x.id_transaccion_orden_g == ordenes.id_compras_orden_g && x.id_inventario_mov_status == 2).FirstOrDefault();
                            if (valid_recepcion_almacen != null) { 
                                obj.fecha_recepcion = (DateTime)valid_recepcion_almacen.C_inventario_almacen_mov_d.FirstOrDefault().fecha_registro;
                                obj.usuario_recepcion = valid_recepcion_almacen.C_usuarios_corporativo.usuario;
                            }
                        }
                    }

                    obj.nombre_proveedor = proveedores_ordenes;
                    obj.ordenes_string = ordenes_string;
                    obj.facturas_string = facturas_string;

                    obj.no_ordenes = item.C_compras_ordenes_g.Where(x => x.activo == true).ToList().Count();
                    obj.orden_generada = true;

                    obj.id_usuario_autoriza = (int)item.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().id_usuario_genera;
                    obj.usuario_autoriza = item.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().C_usuarios_corporativo.usuario;
                    obj.fecha_autoriza = (DateTime)item.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().fecha_registro;
                    obj.tiempo_autoriza = (DateTime)item.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().fecha_registro - (DateTime)item.fecha_registro;


                    if (item.C_compras_ordenes_g.Where(x => x.activo == true).Select(x => x.entregado).Contains(true)) { obj.recepcionada = true; }
                    else { obj.recepcionada = false; }

                    if (item.activo == true && obj.recepcionada == true) { obj.estatus_requisicion = "RECEPCIONADA"; }
                    else if (item.activo == true && obj.recepcionada == false) { obj.estatus_requisicion = "PENDIENTE RECEPCION"; }
                    if (item.activo == true && item.C_compras_ordenes_g.Where(x => x.activo == true).Select(x => x.alta_proveedor).Contains(true))
                    {
                        obj.estatus_requisicion = "IMPORTADA MICROSIP";
                    }
                }
                else
                {
                    obj.no_ordenes = 0;
                    obj.orden_generada = false;

                    obj.id_usuario_autoriza = 0;
                    obj.usuario_autoriza = "N/A";
                    obj.recepcionada = false;

                    if (item.activo == true && obj.solicita_autorizacion == true) { obj.estatus_requisicion = "AUT. DIRECCION"; }
                    else if (item.activo == true && obj.solicita_autorizacion == false) { obj.estatus_requisicion = "2DA FIRMA"; }
                }

                try
                {
                    int[] array_requi = new int[] { (int)item.id_requisicion_articulo_g };
                    obj.importe = Convert.ToDecimal(REQUISICIONES_CONTROLLER.CalcularTotalRequi(array_requi));
                }
                catch (Exception)
                {
                    obj.importe = 0;
                }
                data.Add(obj);
            }
            return data;
        }
        private List<ReporteSeguimientoRequisiciones> ConsultarSeguimientoDetalle(int[] id_usuario, DateTime fecha_i, DateTime fecha_f)
        {
            var requisiciones = db.C_compras_requi_d.Where(x => id_usuario.Contains((int)x.C_compras_requi_g.id_usuario_registro) && x.C_compras_requi_g.fecha_registro >= fecha_i && x.C_compras_requi_g.fecha_registro <= fecha_f).ToList();
            List<ReporteSeguimientoRequisiciones> data = new List<ReporteSeguimientoRequisiciones>();
            foreach (var item in requisiciones)
            {
                int id_requisicion_g = (int)item.id_requisicion_articulo_g;
                int id_requisicion_d = (int)item.id_requisicion_articulo_d;
                int id_articulo = (int)item.id_articulo;

                ReporteSeguimientoRequisiciones obj = new ReporteSeguimientoRequisiciones();
                if (item.C_compras_requi_g.activo == false) { obj.estatus_requisicion = "CANCELADA"; }


                obj.anomalia = false;
                obj.id_requisicion = id_requisicion_g;
                obj.id_centro = (int)item.id_centro_g;
                obj.centro = item.C_centros_g.siglas;
                obj.concepto = item.C_compras_requi_g.concepto;
                obj.tipo_requisicion = item.C_compras_requi_g.C_compras_requisiciones_tipos.tipo_requisicion;
                obj.color_tipo_requisicion = item.C_compras_requi_g.C_compras_requisiciones_tipos.color;
                obj.fecha_registro = (DateTime)item.C_compras_requi_g.fecha_registro;

                obj.id_usuario_registra = (int)item.C_compras_requi_g.id_usuario_registro;
                obj.usuario_registra = item.C_compras_requi_g.C_usuarios_corporativo5.usuario;

                obj.clave = item.C_articulos_catalogo.clave;
                obj.articulo = item.C_articulos_catalogo.nombre_articulo;
                obj.observaciones = item.observacion;
                obj.unidad = item.C_unidades_medidas.unidad_medida;

                obj.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                obj.id_cuenta = (int)item.id_cuenta_contable_g;

                obj.fecha_revisa = DateTime.MinValue;
                obj.fecha_autoriza = DateTime.MinValue;
                obj.fecha_cotiza = DateTime.MinValue;
                obj.fecha_orden = DateTime.MinValue;
                obj.fecha_recepcion = DateTime.MinValue;

                //------------ VALIDA (1 FIRMA)
                if (item.C_compras_requi_g.C_usuarios_corporativo != null)
                {
                    obj.id_usuario_revisa = (int)item.C_compras_requi_g.aut_2_usuario;
                    obj.usuario_revisa = item.C_compras_requi_g.C_usuarios_corporativo.usuario;
                    obj.fecha_revisa = (DateTime)item.C_compras_requi_g.aut_2_fecha;
                    obj.tiempo_revisa = (DateTime)item.C_compras_requi_g.aut_2_fecha - (DateTime)item.C_compras_requi_g.fecha_registro;
                    if (item.C_compras_requi_g.activo == true) { obj.estatus_requisicion = "1RA FIRMA"; }
                }
                else
                {
                    obj.id_usuario_revisa = 0;
                    obj.usuario_revisa = "N/A";
                    if (item.C_compras_requi_g.activo == true) { obj.estatus_requisicion = "REGISTRADA"; }
                }

                //------------ COTIZACIONES
                var cotizaciones = db.C_compras_cotizaciones_requisiciones.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true).Distinct().ToList();
                if (cotizaciones.Count() > 0)
                {
                    obj.no_cotizaciones = cotizaciones.Count();
                    if (item.C_compras_requi_g.activo == true) { obj.estatus_requisicion = "COTIZADA"; }
                }
                else { obj.no_cotizaciones = 0; }

                //------------ PARCIALIDADES 
                var parcialidades = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.id_requisicion_articulo_g == id_requisicion_g && x.activo == true && x.confirmada == true).Distinct().ToList();
                if (parcialidades.Count() > 0)
                {
                    obj.no_parcialidades = parcialidades.Count();

                    if (parcialidades.Select(x => x.solicita_autorizacion).Contains(true)) { obj.solicita_autorizacion = true; }
                    else { obj.solicita_autorizacion = false; }

                    obj.id_usuario_cotiza = (int)parcialidades.FirstOrDefault().C_usuarios_corporativo.id_usuario_corporativo;
                    obj.usuario_cotiza = parcialidades.FirstOrDefault().C_usuarios_corporativo.usuario;

                    if (item.C_compras_requi_g.activo == true && obj.solicita_autorizacion == true) { obj.estatus_requisicion = "AUT. DIRECCION"; }
                    else if (item.C_compras_requi_g.activo == true && obj.solicita_autorizacion == false) { obj.estatus_requisicion = "2DA FIRMA"; }

                    obj.fecha_cotiza = (DateTime)parcialidades.FirstOrDefault().fecha_registro;
                    obj.tiempo_cotizacion = obj.fecha_cotiza - obj.fecha_revisa;
                }
                else
                {
                    obj.no_parcialidades = 0;
                    obj.solicita_autorizacion = false;
                    obj.tiempo_cotizacion = TimeSpan.MinValue;
                    obj.id_usuario_cotiza = 0;
                    obj.usuario_cotiza = "N/A";
                    if (item.C_compras_requi_g.activo == true) { obj.estatus_requisicion = "POR PARCIALIZAR"; }
                }

                if (obj.id_requisicion == 26480)
                {
                    string hola = "";
                }
                else
                {
                    string test = "";
                }

                //------------ ORDENES DE COMPRA
                var valid_orden = db.C_compras_ordenes_d.Where(x => x.C_compras_ordenes_g.id_requisicion_articulo_g == id_requisicion_g && x.activo == true && x.C_compras_ordenes_g.activo == true).ToList();
                if (valid_orden.Count() > 0)
                {
                    string ordenes_string = "";
                    string facturas_string = "";
                    string proveedores_ordenes = "";

                    if (valid_orden.Where(x => x.id_articulo == id_articulo).FirstOrDefault() == null)
                    {
                        foreach (var ordenes in valid_orden.GroupBy(x => x.id_compras_orden_g))
                        {
                            ordenes_string += ordenes.FirstOrDefault().id_compras_orden_g + ", ";
                            if (!proveedores_ordenes.Contains(ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_proveedores.razon_social))
                            {
                                proveedores_ordenes += ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_proveedores.razon_social + ",";
                            }

                            if (ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_facturas_proveedores.Where(x => x.activo == true).Count() > 0)
                            {
                                foreach (var factura in ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_facturas_proveedores.Where(x => x.activo == true).ToList())
                                {
                                    facturas_string += factura.folio_factura + ", ";
                                    if (factura.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true).Count() > 0) { obj.importada = true; }
                                    else { obj.importada = false; }
                                }
                            }
                            obj.cantidad = 0;
                            obj.precio = 0;
                            obj.total = 0;
                            obj.fecha_orden = (DateTime)ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_registro;
                            obj.anomalia = true;
                            //VALIDAR RECEPCION EN ALMACEN
                            try
                            {
                                if (ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_recepcion != null)
                                {
                                    obj.fecha_recepcion = (DateTime)ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_recepcion;
                                    if (ordenes.FirstOrDefault().C_compras_ordenes_g.id_usuario_recepciona != null) { obj.usuario_recepcion = ordenes.FirstOrDefault().C_compras_ordenes_g.C_usuarios_corporativo1.usuario; }
                                }
                                else
                                {
                                    var valid_recepcion_almacen = db.C_inventario_almacen_mov_g.Where(x => x.id_transaccion_orden_g == ordenes.FirstOrDefault().C_compras_ordenes_g.id_compras_orden_g && x.id_inventario_mov_status == 2).FirstOrDefault();
                                    if (valid_recepcion_almacen != null)
                                    {
                                        obj.fecha_recepcion = (DateTime)valid_recepcion_almacen.C_inventario_almacen_mov_d.FirstOrDefault().fecha_registro;
                                        obj.usuario_recepcion = valid_recepcion_almacen.C_usuarios_corporativo.usuario;
                                    }
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    else
                    {
                        var ord_d = valid_orden.Where(x => x.id_articulo == id_articulo).GroupBy(x => x.id_articulo);

                        foreach (var ordenes in ord_d)
                        {
                            ordenes_string += ordenes.FirstOrDefault().id_compras_orden_g + ", ";
                            if (!proveedores_ordenes.Contains(ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_proveedores.razon_social))
                            {
                                proveedores_ordenes += ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_proveedores.razon_social + ",";
                            }

                            if (ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_facturas_proveedores.Where(x => x.activo == true).Count() > 0)
                            {
                                foreach (var factura in ordenes.FirstOrDefault().C_compras_ordenes_g.C_compras_facturas_proveedores.Where(x => x.activo == true).ToList())
                                {
                                    facturas_string += factura.folio_factura + ", ";
                                    if (factura.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true).Count() > 0) { obj.importada = true; }
                                    else { obj.importada = false; }
                                }
                            }

                            if (ord_d.FirstOrDefault().Count() > 1)
                            {
                                decimal importe_total = 0;


                                obj.cantidad = (decimal)ordenes.Where(x => x.id_cuenta_contable == obj.id_cuenta).Sum(x => x.cantidad_compra);
                                obj.precio = (decimal)ordenes.Where(x => x.id_cuenta_contable == obj.id_cuenta).Sum(x => x.precio_unitario);
                                obj.total = obj.cantidad * obj.precio;
                            }
                            else
                            {
                                obj.cantidad = (decimal)ordenes.Sum(x => x.cantidad_compra);
                                obj.precio = (decimal)ordenes.Sum(x => x.precio_unitario);
                                obj.total = obj.cantidad * obj.precio;
                            }
                            obj.anomalia = false;
                            obj.fecha_orden = (DateTime)ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_registro;

                            //VALIDAR RECEPCION EN ALMACEN
                            try
                            {
                                if (ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_recepcion != null)
                                {
                                    obj.fecha_recepcion = (DateTime)ordenes.FirstOrDefault().C_compras_ordenes_g.fecha_recepcion;
                                    if (ordenes.FirstOrDefault().C_compras_ordenes_g.id_usuario_recepciona != null) { obj.usuario_recepcion = ordenes.FirstOrDefault().C_compras_ordenes_g.C_usuarios_corporativo1.usuario; }
                                }
                                else
                                {
                                    var valid_recepcion_almacen = db.C_inventario_almacen_mov_g.Where(x => x.id_transaccion_orden_g == ordenes.FirstOrDefault().C_compras_ordenes_g.id_compras_orden_g && x.id_inventario_mov_status == 2).FirstOrDefault();
                                    if (valid_recepcion_almacen != null)
                                    {
                                        obj.fecha_recepcion = (DateTime)valid_recepcion_almacen.C_inventario_almacen_mov_d.FirstOrDefault().fecha_registro;
                                        obj.usuario_recepcion = valid_recepcion_almacen.C_usuarios_corporativo.usuario;
                                    }
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                    
                    obj.nombre_proveedor = proveedores_ordenes;
                    obj.ordenes_string = ordenes_string;
                    obj.facturas_string = facturas_string;

                    obj.no_ordenes = item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).ToList().Count();
                    obj.orden_generada = true;

                    obj.id_usuario_autoriza = (int)item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().id_usuario_genera;
                    obj.usuario_autoriza = item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().C_usuarios_corporativo.usuario;
                    obj.fecha_autoriza = (DateTime)item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().fecha_registro;
                    obj.tiempo_autoriza = (DateTime)item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).FirstOrDefault().fecha_registro - (DateTime)item.C_compras_requi_g.fecha_registro;


                    if (item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true).Select(x => x.entregado).Contains(true)) { obj.recepcionada = true; }
                    else { obj.recepcionada = false; }

                    if (item.C_compras_requi_g.activo == true && obj.recepcionada == true) { obj.estatus_requisicion = "RECEPCIONADA"; }
                    else if (item.C_compras_requi_g.activo == true && obj.recepcionada == false) { obj.estatus_requisicion = "PENDIENTE RECEPCION"; }

                }
                else
                {
                    obj.no_ordenes = 0;
                    obj.orden_generada = false;

                    obj.id_usuario_autoriza = 0;
                    obj.usuario_autoriza = "N/A";
                    obj.recepcionada = false;

                    obj.cantidad = 0;
                    obj.precio = 0;
                    obj.total = 0;

                    if (item.C_compras_requi_g.activo == true && obj.solicita_autorizacion == true) { obj.estatus_requisicion = "AUT. DIRECCION"; }
                    else if (item.C_compras_requi_g.activo == true && obj.solicita_autorizacion == false) { obj.estatus_requisicion = "2DA FIRMA"; }
                }
                data.Add(obj);
            }
            return data;
        }




        public PartialViewResult ConsultarRequisicionesSeguimiento(int[] id_usuario, string fecha_inicio, string fecha_fin)
        {
            if (id_usuario.Contains(0)) { id_usuario = db.C_usuarios_corporativo.Select(x => x.id_usuario_corporativo).ToArray(); }

            ViewBag.modo = 1;
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            List<ReporteSeguimientoRequisiciones> data = ConsultarSeguimientoHeader(id_usuario, fecha_i, fecha_f);
            return PartialView("SeguimientoRequisiciones/_Seguimiento_requisiciones_table", data);
        }

        public ActionResult DescargarExcelRequisicionesSeguimiento(int[] id_usuario, string fecha_inicio, string fecha_fin)
        {
            if (id_usuario.Contains(0)) { id_usuario = db.C_usuarios_corporativo.Select(x => x.id_usuario_corporativo).ToArray(); }

            string encabezado = "REPORTE DE SEGUIMIENTO DE REQUISICIONES";
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            List<ReporteSeguimientoRequisiciones> data = ConsultarSeguimientoHeader(id_usuario, fecha_i, fecha_f);

            ViewBag.modo = 2;
            string htmlContent = RenderPartialViewToStringObj(data, "SeguimientoRequisiciones/_Seguimiento_requisiciones_table");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 26, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SEGUIMIENTO DE REQUISICIONES.xlsx");
        }


        public PartialViewResult ConsultarRequisicionesSeguimientoDetalle(int[] id_usuario, string fecha_inicio, string fecha_fin)
        {
            if (id_usuario.Contains(0)) { id_usuario = db.C_usuarios_corporativo.Select(x => x.id_usuario_corporativo).ToArray(); }

            ViewBag.modo = 1;
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            List<ReporteSeguimientoRequisiciones> data = ConsultarSeguimientoDetalle(id_usuario, fecha_i, fecha_f);

            return PartialView("SeguimientoRequisiciones/_Seguimiento_requisiciones_detalle_table", data);
        }

        public ActionResult DescargarExcelRequisicionesSeguimientoDetalle(int[] id_usuario, string fecha_inicio, string fecha_fin)
        {
            if (id_usuario.Contains(0)) { id_usuario = db.C_usuarios_corporativo.Select(x => x.id_usuario_corporativo).ToArray(); }

            string encabezado = "REPORTE DE SEGUIMIENTO DE REQUISICIONES DETALLE";
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            List<ReporteSeguimientoRequisiciones> data = ConsultarSeguimientoDetalle(id_usuario, fecha_i, fecha_f);

            ViewBag.modo = 2;
            string htmlContent = RenderPartialViewToStringObj(data, "SeguimientoRequisiciones/_Seguimiento_requisiciones_detalle_table");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 32, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DETALLE SEGUIMIENTO DE REQUISICIONES.xlsx");
        }




        #endregion

        #region REPORTE CASETA
        public ActionResult ConsultarBitacoraCasetaExcel(string fecha_inicio, string fecha_fin, int id_establo)
        {
            int modo = 1;
            int formato = 1;
            int tamano = 11;
            string encabezado = "REPORTE DE CASETA";
            ViewBag.modo = modo;
            string fe1 = fecha_inicio;

            DateTime inicio = DateTime.Parse(fecha_inicio);
            DateTime fin = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var data = db.C_establos_caseta_logs_g.Where(x => x.fecha_entrada >= inicio && x.fecha_entrada <= fin && x.activo == true && x.id_establo == id_establo).ToList();

            string htmlContent = RenderPartialViewToString(data, "../ESTABLOS/Casetas/BitacoraCaseta/_ReporteBitacoraFechasExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, formato, tamano, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE_CASETA_" + inicio.ToShortDateString() + "_AL_" + fin.ToShortDateString() + ".xlsx");
        }
        #endregion

        #region REPORTE FICHA BASCULA
        public ActionResult DescargarExportarFichasBasculaExcel(int[] id_establo, DateTime fecha_inicio, DateTime fecha_fin)
        {
            string encabezado = "REPORTE DE FICHAS DE BASCULA";
            int id_usuario = (int)Session["LoggedId"];

            if (id_establo.Contains(0))
            {
                id_establo = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).Select(x => (int)x.id_establo).ToArray();
            }
            fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);

            var fichas_bascula = db.C_bascula_fichas.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo) && x.fecha_registo > fecha_inicio && x.fecha_registo < fecha_fin).ToList();

            string htmlContent = RenderPartialViewToStringObj(fichas_bascula, "../Establos/Bascula/_ExcelReporteBascula");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 24, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE DE FICHAS DE BASCULA.xlsx");
        }

        public ActionResult DescargarFichasBasculaExcel(int[] id_establo, DateTime fecha_inicio, DateTime fecha_fin)
        {
            try
            {
                string encabezado = "REPORTE DE FICHAS DE BASCULA";
                int id_usuario = (int)Session["LoggedId"];

                if (id_establo.Contains(0))
                {
                    id_establo = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).Select(x => (int)x.id_establo).ToArray();
                }
                fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);

                var fichas_bascula = db.C_bascula_fichas.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo) && x.fecha_registo > fecha_inicio && x.fecha_registo < fecha_fin && x.id_ficha_tipo != 1).ToList();



                DateTime Fecha_inicio = Convert.ToDateTime(fecha_inicio);
                DateTime Fecha_final = Convert.ToDateTime(fecha_fin);
                Fecha_final = Fecha_final.AddHours(23).AddMinutes(59).AddSeconds(59);

                var separacionPorHoja = fichas_bascula.GroupBy(x => x.id_establo).ToList();

                using (var workbook = new XLWorkbook())
                {
                    if (separacionPorHoja.Any())
                    {
                        foreach (var group in separacionPorHoja)
                        {
                            var worksheet = workbook.Worksheets.Add($"{group.First().C_establos.nombre_establo}");
                            var currentRow = 1;
                            var columnCount = 5;

                            var tipoIndicador = group.Key;

                            // Ajustar el tamaño de 'headerRange' según el tipo de parametro de 'Hoja'
                            var headerRange = worksheet.Range(currentRow, 1, currentRow + 2, columnCount);
                            columnCount = 25;
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
                            worksheet.Cell(currentRow, 1).Value = "Reporte de fichas bascula"; // Colocar el texto en la primera celda del rango
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

                            // Configuración del título de reporte según el tipo de indicador
                            var titleRange = worksheet.Range(currentRow, 1, currentRow, columnCount);
                            titleRange.Merge();
                            titleRange.Value = $"{group.First().C_establos.nombre_establo} - {Fecha_inicio.ToShortDateString()} AL {Fecha_final.ToShortDateString()}";

                            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            titleRange.Style.Font.Bold = true;
                            titleRange.Style.Font.FontSize = 14;
                            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(38, 38, 38);
                            titleRange.Style.Font.FontColor = XLColor.White;

                            currentRow++;

                            // Cabeceras de columna
                            var headers = new[] { "Establo", "Ficha" };
                            headers = headers.Concat(new[] { "Folio", "Articulo", "Cliente", "Fecha 1", "Fecha 2", "Linea", "Chofer", "Placas", "P1", "P2", "PT", "Materia",
                                    "Maquilador", "Pozo", "Observacion", "Movimiento", "TMovimiento", "Origen", "Tabla", "Variedad", "Corte", "Pacas", "Ensilador"}).ToArray();

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
                                worksheet.Cell(currentRow, 1).Value = item.C_establos.nombre_establo;
                                worksheet.Cell(currentRow, 2).Value = item.id_bascula_ficha;
                                worksheet.Cell(currentRow, 3).Value = item.folio;
                                worksheet.Cell(currentRow, 4).Value = item.C_articulos_catalogo.nombre_articulo;
                                if(item.id_ficha_tipo == 1 || item.id_ficha_tipo == 4)
                                {
                                    worksheet.Cell(currentRow, 5).Value = item.C_envios_leche_clientes.nombre_comercial;
                                }
                                else {
                                    worksheet.Cell(currentRow, 5).Value = item.C_compras_proveedores.alias_bascula;
                                }
                                worksheet.Cell(currentRow, 6).Value = item.fecha_registo;
                                worksheet.Cell(currentRow, 7).Value = item.fecha_segunda_pesada;
                                worksheet.Cell(currentRow, 8).Value = item.C_bascula_lineas_transportistas.nombre_linea;
                                worksheet.Cell(currentRow, 9).Value = item.chofer;
                                worksheet.Cell(currentRow, 10).Value = item.placas;
                                worksheet.Cell(currentRow, 11).Value = item.peso_1;
                                worksheet.Cell(currentRow, 12).Value = item.peso_2;
                                worksheet.Cell(currentRow, 13).Value = item.peso_t;
                                worksheet.Cell(currentRow, 14).Value = item.peso_materia_seca;
                                worksheet.Cell(currentRow, 15).Value = item.maquilador;
                                worksheet.Cell(currentRow, 16).Value = item.C_bascula_no_pozos.no_pozo;
                                worksheet.Cell(currentRow, 17).Value = item.observaciones;
                                worksheet.Cell(currentRow, 18).Value = item.C_bascula_codigos_movimientos.descripcion;
                                worksheet.Cell(currentRow, 19).Value = item.C_bascula_tipos_movimientos.nombre_movimiento;
                                worksheet.Cell(currentRow, 20).Value = item.peso_origen;
                                worksheet.Cell(currentRow, 21).Value = item.tabla;
                                worksheet.Cell(currentRow, 22).Value = item.variedad;
                                worksheet.Cell(currentRow, 23).Value = item.corte;
                                worksheet.Cell(currentRow, 24).Value = item.pacas;
                                worksheet.Cell(currentRow, 25).Value = item.ensilador;
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
                        worksheet.Cell(1, 1).Value = "No se encontraron fichas para los criterios seleccionados.";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Row(1).Height = 30;
                        worksheet.Columns().AdjustToContents();
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE_FICHAS_BASCULA_BSM_" + Fecha_inicio.ToShortDateString() + "_AL_" + Fecha_final.ToShortDateString() + ".xlsx");
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

        #region REPORTES ALIMENTACION
        public ActionResult ReporteMovimientosPorProveedor()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8075)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("Alimentacion/MovimientosPorProveedor/Index");
        }

        public PartialViewResult ConsultarMovimientosProveedorAlimentacion(string fecha_inicio, string fecha_fin, int[] id_proveedor, int[] id_establo, int[] id_alimento, int facturado)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
                if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
                if (id_proveedor.Contains(0)) { id_proveedor = CATALOGOS_CONTROLLER.ConsultarProveedoresAlimentacionID(); }
                if (id_alimento.Contains(0)) { id_alimento = CATALOGOS_CONTROLLER.ConsultarArticulosTipoFichasBasculaID(2); }

                List<C_bascula_fichas> fichas;
                string hola = "";
                if (facturado == 2)
                {
                    fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //ENTRADA - FORRAJES
                                                    && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                    && x.activo == true && x.C_alimentacion_forrajes_facturas_d.FirstOrDefault() == null).ToList();
                }
                else if (facturado == 0)
                {
                    fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //&& x.id_compras_proveedor != null  //ENTRADA - FORRAJES
                                                    && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                    && x.activo == true).ToList();
                }
                else
                {
                    var fichas_fact = from fich in db.C_bascula_fichas
                                      join fact in db.C_alimentacion_forrajes_facturas_d on fich.id_bascula_ficha equals fact.id_bascula_ficha
                                      where fich.fecha_registo >= fecha_i && fich.fecha_registo <= fecha_f && id_establo.Contains((int)fich.id_establo) &&
                                      fich.recibido == true && fich.id_tipo_movimiento == 1 && fich.id_ficha_tipo == 2 && id_proveedor.Contains((int)fich.id_compras_proveedor) &&
                                      fich.activo == true && fact.activo == true && id_alimento.Contains((int)fich.id_articulo_bascula)
                                      select fich;
                    fichas = fichas_fact.Distinct().ToList();
                }
                return PartialView("Alimentacion/MovimientosPorProveedor/_ReporteMovimientosProveedor", fichas);
            }
            catch (Exception)
            {
                return PartialView("Alimentacion/MovimientosPorProveedor/_ReporteMovimientosProveedor", null);
            }
        }

        public PartialViewResult ConsultarMovimientosProveedorAlimentacionPDF(string fecha_inicio, string fecha_fin, int[] id_proveedor, int[] id_establo, int[] id_alimento, int facturado)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
                if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
                if (id_proveedor.Contains(0)) { id_proveedor = CATALOGOS_CONTROLLER.ConsultarProveedoresAlimentacionID(); }
                if (id_alimento.Contains(0)) { id_alimento = CATALOGOS_CONTROLLER.ConsultarArticulosTipoFichasBasculaID(2); }

                List<C_bascula_fichas> fichas;
                string hola = "";
                if (facturado == 2)
                {
                    fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //ENTRADA - FORRAJES
                                                    && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                    && x.activo == true && x.C_alimentacion_forrajes_facturas_d.FirstOrDefault() == null).ToList();
                }
                else if (facturado == 0)
                {
                    fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                    && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //&& x.id_compras_proveedor != null  //ENTRADA - FORRAJES
                                                    && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                    && x.activo == true).ToList();
                }
                else
                {
                    var fichas_fact = from fich in db.C_bascula_fichas
                                      join fact in db.C_alimentacion_forrajes_facturas_d on fich.id_bascula_ficha equals fact.id_bascula_ficha
                                      where fich.fecha_registo >= fecha_i && fich.fecha_registo <= fecha_f && id_establo.Contains((int)fich.id_establo) &&
                                      fich.recibido == true && fich.id_tipo_movimiento == 1 && fich.id_ficha_tipo == 2 && id_proveedor.Contains((int)fich.id_compras_proveedor) &&
                                      fich.activo == true && fact.activo == true && id_alimento.Contains((int)fich.id_articulo_bascula)
                                      select fich;
                    fichas = fichas_fact.Distinct().ToList();
                }
                ViewBag.fecha_inicio = string.Format(fecha_i.ToShortDateString(), "dd/MM/yyyy");
                ViewBag.fecha_fin = string.Format(fecha_f.ToShortDateString(), "dd/MM/yyyy");
                return PartialView("Alimentacion/MovimientosPorProveedor/_ReporteMovimientosProveedorPDF", fichas);
            }
            catch (Exception)
            {
                return PartialView("Alimentacion/MovimientosPorProveedor/_ReporteMovimientosProveedorPDF", null);
            }
        }

        public ActionResult ConsultarMovimientosProveedorAlimentacionExcel(string fecha_inicio, string fecha_fin, int[] id_proveedor, int[] id_establo, int[] id_alimento, int facturado)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_proveedor.Contains(0)) { id_proveedor = CATALOGOS_CONTROLLER.ConsultarProveedoresAlimentacionID(); }
            if (id_alimento.Contains(0)) { id_alimento = CATALOGOS_CONTROLLER.ConsultarArticulosTipoFichasBasculaID(2); }

            List<C_bascula_fichas> fichas;
            string hola = "";
            if (facturado == 2)
            {
                fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //ENTRADA - FORRAJES
                                                && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                && x.activo == true && x.C_alimentacion_forrajes_facturas_d.FirstOrDefault() == null).OrderBy(x => x.fecha_registo).ToList();
            }
            else if (facturado == 0)
            {
                fichas = db.C_bascula_fichas.Where(x => x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && id_establo.Contains((int)x.id_establo)
                                                && x.recibido == true && x.id_tipo_movimiento == 1 && x.id_ficha_tipo == 2 && id_proveedor.Contains((int)x.id_compras_proveedor) //&& x.id_compras_proveedor != null  //ENTRADA - FORRAJES
                                                && x.id_compras_proveedor != null && id_alimento.Contains((int)x.id_articulo_bascula)
                                                && x.activo == true).OrderBy(x => x.fecha_registo).ToList();
            }
            else
            {
                var fichas_fact = from fich in db.C_bascula_fichas
                                  join fact in db.C_alimentacion_forrajes_facturas_d on fich.id_bascula_ficha equals fact.id_bascula_ficha
                                  where fich.fecha_registo >= fecha_i && fich.fecha_registo <= fecha_f && id_establo.Contains((int)fich.id_establo) &&
                                  fich.recibido == true && fich.id_tipo_movimiento == 1 && fich.id_ficha_tipo == 2 && id_proveedor.Contains((int)fich.id_compras_proveedor) &&
                                  fich.activo == true && fact.activo == true && id_alimento.Contains((int)fich.id_articulo_bascula)
                                  select fich;
                fichas = fichas_fact.OrderBy(x => x.fecha_registo).Distinct().ToList();
            }

            string encabezado = "ALIMENTACIÓN - MOVIMIENTOS POR PROVEEDOR";
            string htmlContent = RenderPartialViewToStringObj(fichas, "Alimentacion/MovimientosPorProveedor/_ReporteMovimientosProveedorEXCEL");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 10, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteMovimientosPorProveedor.xlsx");
        }


        #endregion

        #region REPORTES ENVIOS DE LECHE

        #region Reporte Formato Envio de Leche

        public void GenerarFormatoEnvioLechePDF(int id_envio_leche)
        {
            var fichas_leche = db.C_envios_leche_d_calidad.Where(x => x.C_envios_leche_d_fichas.id_envio_leche_g == id_envio_leche).ToList();

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
            PdfPCell titleCell = new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.C_establos.nombre_establo.ToUpper(), FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado.AddCell(titleCell);

            // Folio a la derecha
            PdfPCell folioTabla = new PdfPCell(new Phrase("Folio " + fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.folio, FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado.AddCell(folioTabla);
            #endregion

            #region CUERPO
            PdfPTable cuerpoP1 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP2 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP3 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP4 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP5 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP6 = new PdfPTable(1) { WidthPercentage = 100 };

            string[] conceptos1 = { "Cliente", "Destino", "Fecha" };
            foreach (var header in conceptos1)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritas))
                {
                    Border = Rectangle.NO_BORDER
                };
                cuerpoP1.AddCell(cell);
            }

            cuerpoP2.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.C_envios_leche_clientes.nombre_comercial, FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP2.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.C_envios_leche_destinos.nombre_destino, FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP2.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.fecha_envio.Value.ToShortDateString(), FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });

            string[] conceptos2 = { "Pipa", "Placas", "Operador" };
            foreach (var header in conceptos2)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritas))
                {
                    Border = Rectangle.NO_BORDER
                };
                cuerpoP3.AddCell(cell);
            }

            cuerpoP4.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.C_bascula_lineas_transportistas.nombre_linea, FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP4.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.placas, FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP4.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.operador, FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });

            string[] conceptos3 = { "Densidad", "Kgs. Totales", "Litros Totales" };
            foreach (var header in conceptos3)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritas))
                {
                    Border = Rectangle.NO_BORDER
                };
                cuerpoP5.AddCell(cell);
            }
            cuerpoP6.AddCell(new PdfPCell(new Phrase("1.02920", FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP6.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.kilos_totales.Value.ToString("N3"), FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoP6.AddCell(new PdfPCell(new Phrase(fichas_leche.FirstOrDefault().C_envios_leche_d_fichas.C_envios_leche_g.litros_totales.Value.ToString("N3"), FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });

            PdfPTable cuerpoCompleto1 = new PdfPTable(8) { WidthPercentage = 100 };
            cuerpoCompleto1.SetWidths(new float[] { 0.1f, 0.29f, 0.01f, 0.1f, 0.29f, 0.01f, 0.1f, 0.1f });

            PdfPTable SaltoLinea = new PdfPTable(1) { WidthPercentage = 100 };
            SaltoLinea.AddCell(new PdfPCell(new Phrase("\n", FuenteGeneral)) { Border = Rectangle.NO_BORDER });

            PdfPTable cuerpoP7 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP8 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP9 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP10 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP11 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP12 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP13 = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable cuerpoP14 = new PdfPTable(1) { WidthPercentage = 100 };

            string[] conceptos7 = { "Ficha", "Tanque", "1a Pesada", "Fecha y Hora", "2a Pesada", "Fecha y Hora", "Peso KG", "Litros", "Sellos" };
            foreach (var header in conceptos7)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritas))
                {
                    Border = Rectangle.NO_BORDER
                };
                cuerpoP7.AddCell(cell);
                cuerpoP11.AddCell(cell);
            }
            string[] conceptos9 = { "Antibiotico", "Temperatura", "Grasa", "Acidez", "Crioscopia", "Solidos Totales", "Betalactamicos", "Tetraciclina", "Aflatoxinas" };
            foreach (var header in conceptos9)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritas))
                {
                    Border = Rectangle.NO_BORDER
                };
                cuerpoP9.AddCell(cell);
                cuerpoP13.AddCell(cell);
            }
            var fichas_envio = fichas_leche.Select(x => x.id_envio_leche_d_ficha).ToArray();

            foreach (var ficha in fichas_leche.Where(x => x.id_envio_leche_d_ficha == fichas_envio[0]))
            {
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.id_envio_leche_d_ficha.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.tanque.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.peso_1.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.fecha_registo.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.peso_2.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.fecha_segunda_pesada.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.kilos_ficha.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.litros_ficha.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP8.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.sello.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });

                if (ficha.antibiotico.Value == -1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else if (ficha.antibiotico.Value == 1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                cuerpoP10.AddCell(new PdfPCell(new Phrase(ficha.temperatura.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP10.AddCell(new PdfPCell(new Phrase(ficha.grasa.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP10.AddCell(new PdfPCell(new Phrase(ficha.acidez.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP10.AddCell(new PdfPCell(new Phrase(ficha.crioscopia.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP10.AddCell(new PdfPCell(new Phrase(ficha.solidos_totales.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });

                if (ficha.betalactamicos.Value == -1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else if (ficha.betalactamicos.Value == 1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                if (ficha.tetraciclina.Value == -1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else if (ficha.tetraciclina.Value == 1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                if (ficha.aflatoxinas.Value == -1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else if (ficha.aflatoxinas.Value == 1)
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
                else
                {
                    cuerpoP10.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                }
            }

            if (fichas_envio.Length > 1)
            {
                foreach (var ficha in fichas_leche.Where(x => x.id_envio_leche_d_ficha == fichas_envio[1]))
                {
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.id_envio_leche_d_ficha.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.tanque.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.peso_1.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.fecha_registo.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.peso_2.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.C_bascula_fichas.fecha_segunda_pesada.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.kilos_ficha.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.litros_ficha.Value.ToString("N3"), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP12.AddCell(new PdfPCell(new Phrase(ficha.C_envios_leche_d_fichas.sello.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });

                    if (ficha.antibiotico.Value == -1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else if (ficha.antibiotico.Value == 1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    cuerpoP14.AddCell(new PdfPCell(new Phrase(ficha.temperatura.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP14.AddCell(new PdfPCell(new Phrase(ficha.grasa.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP14.AddCell(new PdfPCell(new Phrase(ficha.acidez.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP14.AddCell(new PdfPCell(new Phrase(ficha.crioscopia.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    cuerpoP14.AddCell(new PdfPCell(new Phrase(ficha.solidos_totales.ToString(), FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });


                    if (ficha.betalactamicos.Value == -1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else if (ficha.betalactamicos.Value == 1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    if (ficha.tetraciclina.Value == -1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else if (ficha.tetraciclina.Value == 1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    if (ficha.aflatoxinas.Value == -1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else if (ficha.aflatoxinas.Value == 1)
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("POSITIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                    else
                    {
                        cuerpoP14.AddCell(new PdfPCell(new Phrase("NEGATIVO", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                    }
                }
            }
            else
            {
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP12.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });

                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
                cuerpoP14.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { HorizontalAlignment = Element.ALIGN_RIGHT, Border = Rectangle.BOTTOM_BORDER });
            }




            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP1) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP2) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP3) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP4) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP5) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(cuerpoP6) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto1.AddCell(new PdfPCell(SaltoLinea) { Border = Rectangle.NO_BORDER });

            PdfPTable cuerpoCompleto2 = new PdfPTable(11) { WidthPercentage = 100 };
            cuerpoCompleto2.SetWidths(new float[] { 0.1f, 0.19f, 0.01f, 0.12f, 0.08f, 0.01f, 0.1f, 0.19f, 0.01f, 0.12f, 0.08f });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP7) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP8) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP9) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP10) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP11) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP12) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP13) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(cuerpoP14) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(SaltoLinea) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(SaltoLinea) { Border = Rectangle.NO_BORDER });
            cuerpoCompleto2.AddCell(new PdfPCell(SaltoLinea) { Border = Rectangle.NO_BORDER });

            PdfPTable cuerpoCompleto3 = new PdfPTable(5) { WidthPercentage = 100 };
            cuerpoCompleto3.SetWidths(new float[] { 1, 0.1f, 1, 0.1f, 1 });

            // Primera fila con líneas separadas
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase(" ", FuenteGeneral)) { Border = Rectangle.BOTTOM_BORDER });

            // Segunda fila con textos centrados
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase("FIRMA CHOFER", FuenteGeneral))
            { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase("FIRMA SEGURIDAD", FuenteGeneral))
            { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER });
            cuerpoCompleto3.AddCell(new PdfPCell(new Phrase("FIRMA ESTABLO", FuenteGeneral))
            { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });

            #endregion

            document.Add(new iTextSharp.text.Paragraph("\n"));
            document.Add(encabezado);
            document.Add(new iTextSharp.text.Paragraph("\n"));
            document.Add(cuerpoCompleto1);
            document.Add(new iTextSharp.text.Paragraph("\n"));
            document.Add(cuerpoCompleto2);
            document.Add(new iTextSharp.text.Paragraph("\n\n\n\n"));
            document.Add(cuerpoCompleto3);


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

        #region Reporte de Movimientos por Cliente
        public ActionResult ReporteMovimientosClienteEnviosLeche()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8068)) { return View("/Views/Home/Index.cshtml"); }

                return View("EnviosLeche/MovimientosClienteEnviosLeche/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarMovimientosClientesEnviosLeche(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_cliente, int[] id_destino, int facturado)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }

            List<C_envios_leche_g> envios;
            if (facturado == 0) // TODOS
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)).ToList();
            }
            else if (facturado == 1) // SI
            {
                //envios = db.C_envios_leche_lote_d.Where(x => x.activo == true && id_establo.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_establo_envio)
                //        && id_cliente.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_cliente_ms)
                //        && id_destino.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_destino_envio_leche)
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio <= fecha_f
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.activo == true && x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_status == 3).
                //        Select(x => x.C_envios_leche_confirmacion.C_envios_leche_g).Distinct().ToList();
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && x.id_envio_leche_status == 3).ToList();
            }
            else  // NO
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && (x.id_envio_leche_status == 2 || x.id_envio_leche_status == 1)).ToList();
            }
            return PartialView("EnviosLeche/MovimientosClienteEnviosLeche/_ReporteMovimientosClientesEnviosLeche", envios);
        }

        public ActionResult ConsultarMovimientosClientesEnviosLecheExcel(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_cliente, int[] id_destino, int facturado)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }

            List<C_envios_leche_g> envios;
            if (facturado == 0) // TODOS
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)).ToList();
            }
            else if (facturado == 1) // SI
            {
                //envios = db.C_envios_leche_lote_d.Where(x => x.activo == true && id_establo.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_establo_envio)
                //        && id_cliente.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_cliente_ms)
                //        && id_destino.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_destino_envio_leche)
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio <= fecha_f
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.activo == true && x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_status == 3).
                //        Select(x => x.C_envios_leche_confirmacion.C_envios_leche_g).Distinct().ToList();
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && x.id_envio_leche_status == 3).ToList();
            }
            else  // NO
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && (x.id_envio_leche_status == 2 || x.id_envio_leche_status == 1)).ToList();
            }
            string encabezado = "ENVIOS DE LECHE - MOVIMIENTOS POR CLIENTE";
            string htmlContent = RenderPartialViewToStringObj(envios, "EnviosLeche/MovimientosClienteEnviosLeche/_ReporteMovimientosClientesEnviosLecheExcel");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 18, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteMovimientosPorCliente.xlsx");
        }


        public PartialViewResult ConsultarMovimientosClientesEnviosLechePDF(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_cliente, int[] id_destino, int facturado)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }

            List<C_envios_leche_g> envios;
            if (facturado == 0) // TODOS
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)).ToList();
            }
            else if (facturado == 1) // SI
            {
                //envios = db.C_envios_leche_lote_d.Where(x => x.activo == true && id_establo.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_establo_envio)
                //        && id_cliente.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_cliente_ms)
                //        && id_destino.Contains((int)x.C_envios_leche_confirmacion.C_envios_leche_g.id_destino_envio_leche)
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_confirmacion.C_envios_leche_g.fecha_envio <= fecha_f
                //        && x.C_envios_leche_confirmacion.C_envios_leche_g.activo == true && x.C_envios_leche_confirmacion.C_envios_leche_g.id_envio_leche_status == 3).
                //        Select(x => x.C_envios_leche_confirmacion.C_envios_leche_g).Distinct().ToList();
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && x.id_envio_leche_status == 3).ToList();
            }
            else  // NO
            {
                envios = db.C_envios_leche_g.Where(x => id_establo.Contains((int)x.id_establo_envio) && x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f && x.activo == true
                                                    && id_cliente.Contains((int)x.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.id_destino_envio_leche)
                                                    && (x.id_envio_leche_status == 2 || x.id_envio_leche_status == 1)).ToList();
            }
            ViewBag.fecha_inicio = string.Format(fecha_i.ToShortDateString(), "dd/MM/yyyy");
            ViewBag.fecha_fin = string.Format(fecha_f.ToShortDateString(), "dd/MM/yyyy");
            return PartialView("EnviosLeche/MovimientosClienteEnviosLeche/_ReporteMovimientosClientesEnviosLechePDF", envios);
        }
        #endregion

        #region Reporte de Calidad de Envios de Leche
        public ActionResult ReporteCalidadEnviosLeche()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8077)) { return View("/Views/Home/Index.cshtml"); }

                return View("EnviosLeche/CalidadEnviosLeche/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarCalidadEnviosLeche(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_cliente, int[] id_destino)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;
            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }


            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_g.fecha_envio <= fecha_f &&
            id_cliente.Contains((int)x.C_envios_leche_g.id_envio_leche_cliente_ms) && id_destino.Contains((int)x.C_envios_leche_g.id_destino_envio_leche)).ToList();


            return PartialView("EnviosLeche/CalidadEnviosLeche/_ReporteCalidadEnviosLeche", envio_leche);
        }

        public ActionResult DescargarCalidadEnviosLecheExcel(string fecha_inicio, string fecha_fin, int[] id_establo, int[] id_cliente, int[] id_destino)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }

            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_g.fecha_envio <= fecha_f &&
                id_cliente.Contains((int)x.C_envios_leche_g.id_envio_leche_cliente_ms) &&
                id_destino.Contains((int)x.C_envios_leche_g.id_destino_envio_leche)).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte Calidad Envíos Leche");

                // Insertar logo
                var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var image = worksheet.AddPicture(stream)
                                        .MoveTo(worksheet.Cell(1, 1))
                                        .Scale(0.2);
                }

                // Encabezado principal
                var encabezado = "Reporte de Calidad de Envíos de Leche";
                worksheet.Cell(1, 1).Value = encabezado;
                worksheet.Range(1, 1, 3, 26).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold()
                    .Font.SetFontSize(22)
                    .Font.SetFontColor(XLColor.Black)
                    .Fill.SetBackgroundColor(XLColor.White);

                int currentRow = 4;

                foreach (var clienteGroup in envio_leche.GroupBy(x => x.C_envios_leche_g.id_envio_leche_cliente_ms))
                {
                    //HEADER CLIENTE
                    var cliente = clienteGroup.FirstOrDefault();
                    worksheet.Cell(currentRow, 1).Value = "Cliente " + cliente.C_envios_leche_g.C_envios_leche_clientes.nombre_comercial;
                    worksheet.Range(currentRow, 1, currentRow, 26).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Font.SetBold()
                        .Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromArgb(38, 109, 173));
                    currentRow++;

                    // ENCABEZADO ENVIO Y RESULTADOS DE CALIDAD EN LA MISMA FILA
                    worksheet.Cell(currentRow, 1).Value = "ENVIO";
                    worksheet.Range(currentRow, 1, currentRow, 7).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Red);

                    worksheet.Cell(currentRow, 8).Value = "RESULTADOS DE CALIDAD";
                    worksheet.Range(currentRow, 8, currentRow, 26).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Green);

                    currentRow++;

                    // Agregar encabezado de columnas
                    var headers = new[] { "Fecha", "Establo", "Folio Basc.", "Folio Conf.", "Destino", "Litros", "Ficha", "Tanque", "Muestra", "Sala", "ATB.", "Temp.", "Grasa", "Proteina", "SNG", "ST", "Lactosa", "Caseina", "Acidez", "Criosc.", "Urea", "Betalac.", "Tetrac.", "Aflatox", "Sulfas.", "Alcohol 75%" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(currentRow, i + 1).Value = headers[i];
                        worksheet.Cell(currentRow, i + 1).Style
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                            .Font.SetBold()
                            .Font.SetFontColor(XLColor.White)
                            .Fill.SetBackgroundColor(XLColor.Black);
                    }
                    currentRow++;

                    // Datos del cliente
                    foreach (var envio in clienteGroup)
                    {

                        worksheet.Cell(currentRow, 1).Value = envio.C_envios_leche_g.fecha_envio;
                        worksheet.Cell(currentRow, 2).Value = envio.C_envios_leche_g.C_establos.nombre_establo;
                        worksheet.Cell(currentRow, 3).Value = envio.C_envios_leche_g.folio;
                        worksheet.Cell(currentRow, 4).Value = envio.C_envios_leche_g.C_envios_leche_d_fichas.FirstOrDefault()?.folio_confirmacion;
                        worksheet.Cell(currentRow, 5).Value = envio.C_envios_leche_g.C_envios_leche_destinos.nombre_destino;
                        worksheet.Cell(currentRow, 6).Value = envio.C_envios_leche_g.litros_totales;

                        int fichaCol = 7;
                        worksheet.Cell(currentRow, fichaCol).Value = envio.id_ficha_bascula;
                        worksheet.Cell(currentRow, fichaCol + 1).Value = envio.tanque;
                        worksheet.Cell(currentRow, fichaCol + 2).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.muestra_calidad;
                        worksheet.Cell(currentRow, fichaCol + 3).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.sala;
                        worksheet.Cell(currentRow, fichaCol + 4).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.antibiotico == 1 ? "Positivo" : "Negativo";
                        worksheet.Cell(currentRow, fichaCol + 5).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.temperatura;
                        worksheet.Cell(currentRow, fichaCol + 6).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.grasa;
                        worksheet.Cell(currentRow, fichaCol + 7).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.proteina;
                        worksheet.Cell(currentRow, fichaCol + 8).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.solidos_no_grasos;
                        worksheet.Cell(currentRow, fichaCol + 9).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.solidos_totales;
                        worksheet.Cell(currentRow, fichaCol + 10).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.lactosa;
                        worksheet.Cell(currentRow, fichaCol + 11).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.caseina;
                        worksheet.Cell(currentRow, fichaCol + 12).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.acidez;
                        worksheet.Cell(currentRow, fichaCol + 13).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.crioscopia;
                        worksheet.Cell(currentRow, fichaCol + 14).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.urea;
                        worksheet.Cell(currentRow, fichaCol + 15).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.betalactamicos == 1 ? "Positivo" : "Negativo";
                        worksheet.Cell(currentRow, fichaCol + 16).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.tetraciclina == 1 ? "Positivo" : "Negativo";
                        worksheet.Cell(currentRow, fichaCol + 17).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.aflatoxinas == 1 ? "Positivo" : "Negativo";
                        worksheet.Cell(currentRow, fichaCol + 18).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.sulfasimas == 1 ? "Positivo" : "Negativo";
                        worksheet.Cell(currentRow, fichaCol + 19).Value = envio.C_envios_leche_d_calidad.FirstOrDefault()?.alcohol_75 == 1 ? "Positivo" : "Negativo";
                        currentRow++;

                    }
                }

                var dataRange = worksheet.Range(1, 1, currentRow - 1, 26);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    var fechas = fecha_i.ToShortDateString() + " AL " + fecha_f.ToShortDateString();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE DE CALIDAD LECHE " + fechas + ".xlsx");
                }
            }
        }

        public void GenerarReporteCalidadEnviosLechePDF(string fecha_inicio, string fecha_fin, int[] id_cliente, int[] id_establo, int[] id_destino)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            if (id_establo.Contains(0)) { id_establo = CATALOGOS_CONTROLLER.EstablosUsuariosID(id_usuario); }
            if (id_cliente.Contains(0)) { id_cliente = CATALOGOS_CONTROLLER.ConsultarClientesEnviosLecheID(); }
            if (id_destino.Contains(0)) { id_destino = CATALOGOS_CONTROLLER.ConsultarDestinosClienteID(0); }

            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.C_envios_leche_g.fecha_envio >= fecha_i && x.C_envios_leche_g.fecha_envio <= fecha_f &&
                id_cliente.Contains((int)x.C_envios_leche_g.id_envio_leche_cliente_ms) &&
                id_destino.Contains((int)x.C_envios_leche_g.id_destino_envio_leche)).ToList();

            if (!envio_leche.Any())
            {
                return;
            }

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuente para el título, encabezado, y contenido
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.WHITE);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 6);



            // Configuración de la tabla principal con tres columnas
            PdfPTable headerTable = new PdfPTable(3) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 1f, 3f, 1f }); // Proporciones de cada columna

            // Agregar logo en la primera celda, alineado a la izquierda
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(60f, 60f); // Tamaño del logo
                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                headerTable.AddCell(logoCell);
            }

            // Agregar título centrado en la segunda celda
            PdfPCell titleCell = new PdfPCell(new Phrase("Reporte de Calidad de Envíos de Leche", titleFont))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(titleCell);

            // Agregar fechas en la tercera celda, cada una en su propia fila y alineada a la derecha
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

            // Agregar la tabla de encabezado al documento
            document.Add(headerTable);

            foreach (var cliente in envio_leche.GroupBy(x => x.C_envios_leche_g.id_envio_leche_cliente_ms))
            {
                // Línea horizontal antes de cada cliente
                document.Add(new iTextSharp.text.Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator())));
                // Salto de línea
                document.Add(new iTextSharp.text.Paragraph("\n"));

                // Título del cliente con tamaño de fuente reducido
                var clientName = cliente.FirstOrDefault()?.C_envios_leche_g?.C_envios_leche_clientes?.nombre_comercial ?? "Cliente desconocido";
                PdfPCell clientCell = new PdfPCell(new Phrase($"Cliente: {clientName}", headerFont))
                {
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5f  // Opcional: agrega un poco de espacio alrededor del texto
                };

                // Crea una tabla con una sola columna para añadir la celda
                PdfPTable clientTable = new PdfPTable(1) { WidthPercentage = 100 };
                clientTable.AddCell(clientCell);

                // Agrega la tabla con la celda azul al documento
                document.Add(clientTable);



                PdfPTable table = new PdfPTable(26);  // Número de columnas de la tabla
                table.WidthPercentage = 100;

                // Encabezados superiores "ENVÍO" y "RESULTADO DE CALIDAD"
                PdfPCell envioHeader = new PdfPCell(new Phrase("ENVÍO", headerFont))
                {
                    Colspan = 8,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = new BaseColor(231, 76, 60) // Color de fondo opcional para resaltar
                };
                table.AddCell(envioHeader);

                PdfPCell resultadoCalidadHeader = new PdfPCell(new Phrase("RESULTADO DE CALIDAD", headerFont))
                {
                    Colspan = 18,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = new BaseColor(26, 187, 156) // Color de fondo opcional para resaltar
                };
                table.AddCell(resultadoCalidadHeader);


                // Encabezado de columnas
                string[] headers = {
                    "Fecha", "Establo", "Folio Basc.","Folio Conf.", "Destino", "Litros", "Ficha", "Tanque",
                    "Muestra", "Sala", "ATB.", "Temp.", "Grasa", "Proteina", "SNG", "ST",
                    "Lactosa", "Caseina", "Acidez", "Criosc.", "Urea", "Betalac.",
                    "Tetrac.", "Aflatox", "Sulfas.", "Alcohol 75%"
                };

                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(52, 58, 64),
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }

                // Agregar filas con datos
                foreach (var itemGroup in cliente.GroupBy(x => x.id_envio_leche_g))
                {
                    var envio = itemGroup.FirstOrDefault()?.C_envios_leche_g;

                    // Encabezado del envío
                    table.AddCell(new PdfPCell(new Phrase(envio?.fecha_envio.ToString() ?? "", cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(envio?.C_establos?.nombre_establo ?? "", cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(envio?.folio ?? "", cellFont)));

                    string folios = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_g.C_envios_leche_d_fichas.FirstOrDefault()?.folio_confirmacion?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(folios, cellFont)));

                    table.AddCell(new PdfPCell(new Phrase(envio?.C_envios_leche_destinos?.nombre_destino ?? "", cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(envio?.litros_totales.ToString() ?? "", cellFont)));

                    // Concatenación de valores por cada campo
                    string fichas_concatenadas = string.Join("\n", itemGroup.Select(f => f.id_ficha_bascula.ToString()));
                    table.AddCell(new PdfPCell(new Phrase(fichas_concatenadas, cellFont)));

                    string tanques_concatenados = string.Join("\n", itemGroup.Select(f => f.tanque.ToString()));
                    table.AddCell(new PdfPCell(new Phrase(tanques_concatenados, cellFont)));

                    string muestras_concatenadas = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.muestra_calidad ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(muestras_concatenadas, cellFont)));

                    string salas_concatenadas = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.sala ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(salas_concatenadas, cellFont)));

                    string antibioticos_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.antibiotico;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(antibioticos_concatenados + "\n", cellFont)));

                    string temperaturas_concatenadas = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.temperatura?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(temperaturas_concatenadas, cellFont)));

                    string grasas_concatenadas = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.grasa?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(grasas_concatenadas, cellFont)));

                    string proteinas_concatenadas = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.proteina?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(proteinas_concatenadas, cellFont)));

                    string sng_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.solidos_no_grasos?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(sng_concatenados, cellFont)));

                    string st_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.solidos_totales?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(st_concatenados, cellFont)));

                    string lactosa_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.lactosa?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(lactosa_concatenados, cellFont)));

                    string caseina_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.caseina?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(caseina_concatenados, cellFont)));

                    string acidez_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.acidez?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(acidez_concatenados, cellFont)));

                    string crioscopia_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.crioscopia?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(crioscopia_concatenados, cellFont)));

                    string urea_concatenados = string.Join("\n", itemGroup.Select(f => f.C_envios_leche_d_calidad.FirstOrDefault()?.urea?.ToString() ?? ""));
                    table.AddCell(new PdfPCell(new Phrase(urea_concatenados, cellFont)));


                    string betalactamicos_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.betalactamicos;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(betalactamicos_concatenados, cellFont)));

                    string tetraciclinas_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.tetraciclina;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(tetraciclinas_concatenados, cellFont)));

                    string aflatoxinas_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.aflatoxinas;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(aflatoxinas_concatenados, cellFont)));

                    string sulfas_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.sulfasimas;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(sulfas_concatenados, cellFont)));


                    string alcohol75_concatenados = string.Join("\n", itemGroup.Select(f =>
                    {
                        int? antibiotico = f.C_envios_leche_d_calidad.FirstOrDefault()?.alcohol_75;
                        if (antibiotico == -1) return "Ninguno";
                        if (antibiotico == 0) return "Negativo";
                        if (antibiotico == 1) return "Positivo";
                        return "";
                    }));
                    table.AddCell(new PdfPCell(new Phrase(alcohol75_concatenados, cellFont)));
                }
                document.Add(table);
                document.Add(new iTextSharp.text.Paragraph("\n"));
            }

            // Cerrar y retornar el archivo PDF
            document.Close();
            writer.Close();

            // Enviar PDF al navegador
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Reporte_Calidad_Envios_Leche.pdf");
            Response.BinaryWrite(ms.ToArray());
            Response.End();
        }


        #endregion


        #region Reporte de Cumplimiento de Envio de Leche
        public ActionResult ReporteCumplimientoEnviosLeche()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8078)) { return View("/Views/Home/Index.cshtml"); }

                return View("EnviosLeche/CumplimientoProgramaEnvioLeche/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarCumplimientoEnviosLeche(string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;

            //PPTO
            var presupuesto = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x =>
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_inicio >= fecha_i &&
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_fin <= fecha_f).ToList();

            //REAL
            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f).ToList();

            var clientes_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).ToArray();
            var clientes_2 = envio_leche.Select(x => x.id_envio_leche_cliente_ms).ToArray();
            var clientes_total = clientes_1.Union(clientes_2).ToArray();

            var establos_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).ToArray();
            var establos_2 = envio_leche.Select(x => x.id_establo_envio).ToArray();
            var establos_total = establos_1.Union(establos_2).ToArray();



            List<ReporteCumplimientoEnviosLecheData> cumplimiento_general = new List<ReporteCumplimientoEnviosLecheData>();

            foreach (var cliente in clientes_total)
            {
                foreach (var establo in establos_total)
                {
                    var destino_envio = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).Select(x => x.id_destino_envio_leche).Distinct().ToArray();
                    var destino_ppto = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche).Distinct().ToArray();
                    var destino_leche = destino_envio.Union(destino_ppto).ToArray();
                    foreach (var destino in destino_leche)
                    {
                        //  && x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche
                        ReporteCumplimientoEnviosLecheData cumplimiento = new ReporteCumplimientoEnviosLecheData();

                        cumplimiento.Id_cliente = (int)cliente;
                        //PPTO ENVIO
                        cumplimiento.Ppto_envio = (decimal)presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Sum(x => x.cantidad_litros);
                        //REAL ENVIO
                        cumplimiento.Real_envio = (decimal)envio_leche.Where(x => x.id_establo_envio == establo && x.id_destino_envio_leche == destino && x.id_envio_leche_cliente_ms == cliente).Sum(x => x.litros_totales);



                        //CUMPLIMIENTO POR ENVIO
                        try
                        {
                            cumplimiento.Cumplimiento_envio = Convert.ToInt32(cumplimiento.Real_envio / (cumplimiento.Ppto_envio / 100));
                        }
                        catch (Exception)
                        {
                            cumplimiento.Cumplimiento_envio = 0;
                        }
                        try
                        {
                            cumplimiento.Establo = envio_leche.Where(x => x.id_establo_envio == establo).FirstOrDefault().C_establos.nombre_establo;
                            cumplimiento.Destino = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo && x.id_destino_envio_leche == destino).FirstOrDefault().C_envios_leche_destinos.nombre_destino;
                            cumplimiento.Cliente = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).FirstOrDefault().C_envios_leche_clientes.nombre_comercial;
                        }
                        catch (Exception)
                        {
                            cumplimiento.Establo = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo;
                            cumplimiento.Cliente = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo && x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche == destino).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial;
                            cumplimiento.Destino = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo && x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche == destino).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_destinos.nombre_destino;
                        }

                        cumplimiento_general.Add(cumplimiento);
                    }
                }
            }

            return PartialView("EnviosLeche/CumplimientoProgramaEnvioLeche/_ReporteCumplimientoEnviosLeche", cumplimiento_general);
        }

        public ActionResult DescargarCumplimientoEnviosLecheExcel(string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;

            //PPTO
            var presupuesto = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x =>
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_inicio >= fecha_i &&
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_fin <= fecha_f).ToList();

            //REAL
            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f).ToList();

            var clientes_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).ToArray();
            var clientes_2 = envio_leche.Select(x => x.id_envio_leche_cliente_ms).ToArray();
            var clientes_total = clientes_1.Union(clientes_2).ToArray();

            var establos_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).ToArray();
            var establos_2 = envio_leche.Select(x => x.id_establo_envio).ToArray();
            var establos_total = establos_1.Union(establos_2).ToArray();



            List<ReporteCumplimientoEnviosLecheData> cumplimiento_general = new List<ReporteCumplimientoEnviosLecheData>();

            foreach (var cliente in clientes_total)
            {
                foreach (var establo in establos_total)
                {
                    var destino_envio = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).Select(x => x.id_destino_envio_leche).Distinct().ToArray();
                    var destino_ppto = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche).Distinct().ToArray();
                    var destino_leche = destino_envio.Union(destino_ppto).ToArray();
                    foreach (var destino in destino_leche)
                    {
                        //  && x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche
                        ReporteCumplimientoEnviosLecheData cumplimiento = new ReporteCumplimientoEnviosLecheData();

                        cumplimiento.Id_cliente = (int)cliente;
                        //PPTO ENVIO
                        cumplimiento.Ppto_envio = (decimal)presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Sum(x => x.cantidad_litros);
                        //REAL ENVIO
                        cumplimiento.Real_envio = (decimal)envio_leche.Where(x => x.id_establo_envio == establo && x.id_destino_envio_leche == destino && x.id_envio_leche_cliente_ms == cliente).Sum(x => x.litros_totales);



                        //CUMPLIMIENTO POR ENVIO
                        try
                        {
                            cumplimiento.Cumplimiento_envio = Convert.ToInt32(cumplimiento.Real_envio / (cumplimiento.Ppto_envio / 100));
                        }
                        catch (Exception)
                        {
                            cumplimiento.Cumplimiento_envio = 0;
                        }
                        try
                        {
                            cumplimiento.Establo = envio_leche.Where(x => x.id_establo_envio == establo).FirstOrDefault().C_establos.nombre_establo;
                            cumplimiento.Destino = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo && x.id_destino_envio_leche == destino).FirstOrDefault().C_envios_leche_destinos.nombre_destino;
                            cumplimiento.Cliente = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).FirstOrDefault().C_envios_leche_clientes.nombre_comercial;
                        }
                        catch (Exception)
                        {
                            cumplimiento.Establo = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo;
                            cumplimiento.Cliente = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial;
                            cumplimiento.Destino = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_destinos.nombre_destino;
                        }

                        cumplimiento_general.Add(cumplimiento);
                    }
                }
            }


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte Cumplimiento");

                // Insertar logo
                var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var image = worksheet.AddPicture(stream)
                                        .MoveTo(worksheet.Cell(1, 1))
                                        .Scale(0.2);
                }

                // Encabezado principal
                var encabezado = "CUMPLIMIENTO DE PROGRAMA DE ENVIOS DE LECHE";
                worksheet.Cell(1, 2).Value = encabezado;
                worksheet.Range(1, 1, 3, 1).Merge();
                worksheet.Range(1, 2, 3, 6).Merge().Style
                      .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                      .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                      .Font.SetBold()
                      .Font.SetFontSize(18)
                      .Font.SetFontColor(XLColor.Black)
                      .Fill.SetBackgroundColor(XLColor.White)
                      .Alignment.WrapText = true;
                int currentRow = 4;
                var dataRange1 = worksheet.Range(1, 2, currentRow - 1, 6);
                dataRange1.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                //CUMPLIMIENTO POR ENVIO
                #region CUMPLIMIENTO ENVIO
                var primera_celda = currentRow;
                worksheet.Cell(currentRow, 1).Value = "CUMPLIMIENTO POR ENVIOS";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Font.SetBold()
                    .Font.SetFontSize(12)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(38, 109, 173));
                currentRow++;

                // Agregar encabezado de columnas
                var headers = new[] { "Cliente", "Destino", "Establo", "Ppto", "Real", "Cumplimiento" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i];
                    worksheet.Cell(currentRow, i + 1).Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Black);
                }
                currentRow++;

                // Datos del cliente
                foreach (var envios in cumplimiento_general)
                {
                    worksheet.Cell(currentRow, 1).Value = envios.Cliente;
                    worksheet.Cell(currentRow, 2).Value = envios.Destino;
                    worksheet.Cell(currentRow, 3).Value = envios.Establo;
                    worksheet.Cell(currentRow, 4).Value = envios.Ppto_envio;
                    worksheet.Cell(currentRow, 5).Value = envios.Real_envio;
                    worksheet.Cell(currentRow, 6).Value = envios.Cumplimiento_envio;
                    currentRow++;

                }

                var dataRange2 = worksheet.Range(primera_celda, 1, currentRow - 1, 6);
                dataRange2.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange2.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                #endregion
                currentRow++;
                #region CUMPLIMIENTO CLIENTE
                primera_celda = currentRow;
                worksheet.Cell(currentRow, 1).Value = "CUMPLIMIENTO POR CLIENTE";
                worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Font.SetBold()
                    .Font.SetFontSize(12)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(38, 109, 173));
                currentRow++;

                headers = new[] { "Cliente", "", "Ppto", "Real", "Cumplimiento" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i];
                    worksheet.Cell(currentRow, i + 1).Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Black);
                }
                currentRow++;

                foreach (var envios in cumplimiento_general.GroupBy(x => x.Id_cliente))
                {
                    worksheet.Cell(currentRow, 1).Value = envios.FirstOrDefault().Cliente;
                    worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = envios.Sum(x => x.Ppto_envio);
                    worksheet.Cell(currentRow, 4).Value = envios.Sum(x => x.Real_envio);
                    decimal total = 0;
                    try
                    {
                        total = envios.Sum(x => x.Real_envio) / (envios.Sum(x => x.Ppto_envio) / 100);
                    }
                    catch (Exception)
                    {
                        total = 0;
                    }
                    worksheet.Cell(currentRow, 5).Value = total;
                    currentRow++;

                }
                var dataRange3 = worksheet.Range(primera_celda, 1, currentRow - 1, 5);
                dataRange3.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange3.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                #endregion
                currentRow++;
                #region CUMPLIMIENTO ESTABLO
                primera_celda = currentRow;
                worksheet.Cell(currentRow, 1).Value = "CUMPLIMIENTO POR ESTABLO";
                worksheet.Range(currentRow, 1, currentRow, 5).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Font.SetBold()
                    .Font.SetFontSize(12)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromArgb(38, 109, 173));
                currentRow++;

                headers = new[] { "Establo", "", "Ppto", "Real", "Cumplimiento" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = headers[i];
                    worksheet.Cell(currentRow, i + 1).Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Black);
                }
                currentRow++;

                foreach (var envios in cumplimiento_general.GroupBy(x => x.Establo))
                {
                    worksheet.Cell(currentRow, 1).Value = envios.FirstOrDefault().Establo;
                    worksheet.Range(currentRow, 1, currentRow, 2).Merge();
                    worksheet.Cell(currentRow, 3).Value = envios.Sum(x => x.Ppto_envio);
                    worksheet.Cell(currentRow, 4).Value = envios.Sum(x => x.Real_envio);
                    decimal total = 0;
                    try
                    {
                        total = envios.Sum(x => x.Real_envio) / (envios.Sum(x => x.Ppto_envio) / 100);
                    }
                    catch (Exception)
                    {
                        total = 0;
                    }
                    worksheet.Cell(currentRow, 5).Value = total;
                    currentRow++;

                }
                var dataRange4 = worksheet.Range(primera_celda, 1, currentRow - 1, 5);
                dataRange4.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange4.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                #endregion

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    var fechas = fecha_i.ToShortDateString() + " AL " + fecha_f.ToShortDateString();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE DE CALIDAD LECHE " + fechas + ".xlsx");
                }
            }
        }

        public void GenerarCumplimientoEnviosLechePDF(string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59); ;

            //PPTO
            var presupuesto = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x =>
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_inicio >= fecha_i &&
            x.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_programacion_semanal_g.fecha_fin <= fecha_f).ToList();

            //REAL
            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fecha_i && x.fecha_envio <= fecha_f).ToList();

            var clientes_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).ToArray();
            var clientes_2 = envio_leche.Select(x => x.id_envio_leche_cliente_ms).ToArray();
            var clientes_total = clientes_1.Union(clientes_2).ToArray();

            var establos_1 = presupuesto.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).ToArray();
            var establos_2 = envio_leche.Select(x => x.id_establo_envio).ToArray();
            var establos_total = establos_1.Union(establos_2).ToArray();


            #region LLENADO DEL OBJETO
            List<ReporteCumplimientoEnviosLecheData> cumplimiento_general = new List<ReporteCumplimientoEnviosLecheData>();

            foreach (var cliente in clientes_total)
            {
                foreach (var establo in establos_total)
                {
                    var destino_envio = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).Select(x => x.id_destino_envio_leche).Distinct().ToArray();
                    var destino_ppto = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche).Distinct().ToArray();
                    var destino_leche = destino_envio.Union(destino_ppto).ToArray();
                    foreach (var destino in destino_leche)
                    {
                        //  && x.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche
                        ReporteCumplimientoEnviosLecheData cumplimiento = new ReporteCumplimientoEnviosLecheData();

                        cumplimiento.Id_cliente = (int)cliente;
                        //PPTO ENVIO
                        cumplimiento.Ppto_envio = (decimal)presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Sum(x => x.cantidad_litros);
                        //REAL ENVIO
                        cumplimiento.Real_envio = (decimal)envio_leche.Where(x => x.id_establo_envio == establo && x.id_destino_envio_leche == destino && x.id_envio_leche_cliente_ms == cliente).Sum(x => x.litros_totales);

                        //CUMPLIMIENTO POR ENVIO
                        try
                        {
                            cumplimiento.Cumplimiento_envio = Convert.ToInt32(cumplimiento.Real_envio / (cumplimiento.Ppto_envio / 100));
                        }
                        catch (Exception)
                        {
                            cumplimiento.Cumplimiento_envio = 0;
                        }
                        try
                        {
                            cumplimiento.Establo = envio_leche.Where(x => x.id_establo_envio == establo).FirstOrDefault().C_establos.nombre_establo;
                            cumplimiento.Destino = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo && x.id_destino_envio_leche == destino).FirstOrDefault().C_envios_leche_destinos.nombre_destino;
                            cumplimiento.Cliente = envio_leche.Where(x => x.id_envio_leche_cliente_ms == cliente && x.id_establo_envio == establo).FirstOrDefault().C_envios_leche_clientes.nombre_comercial;
                        }
                        catch (Exception)
                        {
                            cumplimiento.Establo = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo;
                            cumplimiento.Cliente = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial;
                            cumplimiento.Destino = presupuesto.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_destinos.nombre_destino;
                        }
                        cumplimiento_general.Add(cumplimiento);
                    }
                }
            }
            #endregion

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuentes para el título, encabezado y contenido
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 7, BaseColor.WHITE);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 6);

            // Agregar encabezado de reporte con logo, título y fechas
            document.Add(GenerarEncabezadoReporte(titleFont, cellFont, fecha_i, fecha_f));

            // Generar la primera tabla (CUMPLIMIENTO POR ENVIOS)
            document.Add(new iTextSharp.text.Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator())));
            document.Add(new iTextSharp.text.Paragraph("\n"));
            GenerarTablaCumplimientoEnvios(document, cumplimiento_general, headerFont, cellFont);

            // Generar la segunda tabla (CUMPLIMIENTO POR CLIENTE)
            //document.Add(new iTextSharp.text.Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator())));
            document.Add(new iTextSharp.text.Paragraph("\n"));
            GenerarTablaCumplimientoCliente(document, cumplimiento_general, headerFont, cellFont);

            // Generar la tercera tabla (CUMPLIMIENTO POR ESTABLO)
            //document.Add(new iTextSharp.text.Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator())));
            document.Add(new iTextSharp.text.Paragraph("\n"));
            GenerarTablaCumplimientoEstablo(document, cumplimiento_general, headerFont, cellFont);

            document.Close();
            writer.Close();

            // Enviar PDF al navegador
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=Reporte_Cumplimiento_Envios_Leche.pdf");
            Response.BinaryWrite(ms.ToArray());
            Response.End();

        }

        private PdfPTable GenerarEncabezadoReporte(iTextSharp.text.Font titleFont, iTextSharp.text.Font cellFont, DateTime fecha_i, DateTime fecha_f)
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
            PdfPCell titleCell = new PdfPCell(new Phrase("Reporte de Cumplimiento de Envíos de Leche", titleFont))
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
        private void GenerarTablaCumplimientoEnvios(iTextSharp.text.Document document, List<ReporteCumplimientoEnviosLecheData> datos, iTextSharp.text.Font headerFont, iTextSharp.text.Font cellFont)
        {
            var titulo_tabla = "CUMPLIMIENTO POR ENVIOS";
            PdfPCell tituloCell = new PdfPCell(new Phrase($"{titulo_tabla}", headerFont))
            {
                BackgroundColor = new BaseColor(46, 135, 215),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f
            };
            // Crea una tabla con una sola columna para añadir la celda
            PdfPTable tituloTable = new PdfPTable(1) { WidthPercentage = 100 };
            tituloTable.AddCell(tituloCell);
            document.Add(tituloTable);

            PdfPTable table = new PdfPTable(6);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 0.3f, 0.25f, 0.15f, 0.1f, 0.1f, 0.1f });

            // Encabezado
            string[] headers = { "Cliente", "Destino", "Establo", "Ppto", "Real", "Cumplimiento" };
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(52, 58, 64),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }

            // Filas de datos
            foreach (var item in datos)
            {
                table.AddCell(new PdfPCell(new Phrase(item.Cliente, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Destino, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Establo, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Ppto_envio.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Real_envio.ToString(), cellFont)));
                int cumplimiento = Convert.ToInt32(item.Cumplimiento_envio);
                table.AddCell(new PdfPCell(new Phrase(cumplimiento.ToString() + "%", cellFont)));
            }

            document.Add(table);
        }
        private void GenerarTablaCumplimientoCliente(iTextSharp.text.Document document, List<ReporteCumplimientoEnviosLecheData> datos, iTextSharp.text.Font headerFont, iTextSharp.text.Font cellFont)
        {
            var titulo_tabla = "CUMPLIMIENTO POR CLIENTE";
            PdfPCell tituloCell = new PdfPCell(new Phrase($"{titulo_tabla}", headerFont))
            {
                BackgroundColor = new BaseColor(46, 135, 215),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f
            };
            // Crea una tabla con una sola columna para añadir la celda
            PdfPTable tituloTable = new PdfPTable(1) { WidthPercentage = 100 };
            tituloTable.AddCell(tituloCell);
            document.Add(tituloTable);
            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 0.4f, 0.2f, 0.2f, 0.2f });

            // Encabezado
            string[] headers = { "Cliente", "Ppto", "Real", "Cumplimiento" };
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(52, 58, 64),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }

            // Filas de datos
            foreach (var item in datos.GroupBy(x => x.Id_cliente))
            {
                table.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().Cliente, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.Ppto_envio).ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.Real_envio).ToString(), cellFont)));
                decimal total = 0;
                try
                {
                    total = item.Sum(x => x.Real_envio) / (item.Sum(x => x.Ppto_envio) / 100);
                }
                catch (Exception)
                {
                    total = 0;
                }
                int cumplimiento = Convert.ToInt32(total);
                table.AddCell(new PdfPCell(new Phrase(cumplimiento.ToString() + "%", cellFont)));
            }

            document.Add(table);
        }
        private void GenerarTablaCumplimientoEstablo(iTextSharp.text.Document document, List<ReporteCumplimientoEnviosLecheData> datos, iTextSharp.text.Font headerFont, iTextSharp.text.Font cellFont)
        {
            var titulo_tabla = "CUMPLIMIENTO POR ESTABLO";
            PdfPCell tituloCell = new PdfPCell(new Phrase($"{titulo_tabla}", headerFont))
            {
                BackgroundColor = new BaseColor(46, 135, 215),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5f
            };
            // Crea una tabla con una sola columna para añadir la celda
            PdfPTable tituloTable = new PdfPTable(1) { WidthPercentage = 100 };
            tituloTable.AddCell(tituloCell);
            document.Add(tituloTable);

            PdfPTable table = new PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 0.4f, 0.2f, 0.2f, 0.2f });

            // Encabezado
            string[] headers = { "Establo", "Ppto", "Real", "Cumplimiento" };
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(52, 58, 64),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(cell);
            }

            // Filas de datos
            foreach (var item in datos.GroupBy(x => x.Establo))
            {
                table.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().Establo, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.Ppto_envio).ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.Real_envio).ToString(), cellFont)));
                decimal total = 0;
                try
                {
                    total = item.Sum(x => x.Real_envio) / (item.Sum(x => x.Ppto_envio) / 100);
                }
                catch (Exception)
                {
                    total = 0;
                }
                int cumplimiento = Convert.ToInt32(total);
                table.AddCell(new PdfPCell(new Phrase(cumplimiento.ToString() + "%", cellFont)));
            }

            document.Add(table);
        }
        #endregion

        #region Reporte de Cumplimiento de Envio de Leche Detallado
        public (DateTime PrimerDiaSemana, DateTime UltimoDiaSemana) ObtenerSemanaEdicion(int year, int semana)
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


        //GenerarCumplimientoEnviosLecheCompletoPDF
        public void GenerarCumplimientoEnviosLecheDetalladoPDF(int anio, int nosemana)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            var fechas = ObtenerSemanaEdicion(Convert.ToInt32(cliente.FirstOrDefault().C_envios_leche_programacion_semanal_g.C_presupuestos_anios.anio), nosemana);
            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fechas.PrimerDiaSemana && x.fecha_envio <= fechas.UltimoDiaSemana && x.activo == true).ToList();


            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuentes y Tipografia
            var FuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var FuenteTituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            var FuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var FuenteSubtituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var FuenteGeneral = FontFactory.GetFont(FontFactory.HELVETICA, 6);
            var FuenteNegritasMayor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var FuenteNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6);
            var FuenteNegritasColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, BaseColor.WHITE);
            var FuenteSubrayada = FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteSubrayadaNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteError = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40);
            FuenteError.Color = BaseColor.RED;

            #region ENCABEZADO
            PdfPTable encabezadoG = new PdfPTable(2) { WidthPercentage = 100 };
            encabezadoG.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            encabezadoG.SetWidths(new float[] { 0.15f, 0.85f });



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
                encabezadoG.AddCell(logoCell);
            }
            PdfPTable encabezadoD = new PdfPTable(1) { WidthPercentage = 100 };
            encabezadoD.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            // Título de la tabla
            PdfPCell celdaTitulo1 = new PdfPCell(new Phrase("Sistema de Informacion Integral Beta", FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo1);
            PdfPCell celdaTitulo2 = new PdfPCell(new Phrase("CUMPLIMIENTO DE PROGRAMA DE ENVIOS DE LECHE DEL " + fechas.PrimerDiaSemana.ToShortDateString() + " AL " + fechas.UltimoDiaSemana.ToShortDateString(), FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo2);
            encabezadoG.AddCell(encabezadoD);
            document.Add(encabezadoG);

            document.Add(new iTextSharp.text.Paragraph("\n"));
            #endregion

            #region CUERPO
            //CUMPLIMIENTO
            PdfPTable cumplimientoG = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell celdaEncabezado = new PdfPCell(new Phrase("CUMPLIMIENTO POR ENVIO", FuenteNegritasMayor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            cumplimientoG.AddCell(celdaEncabezado);
            document.Add(cumplimientoG);
            //ENCABEZADOS
            PdfPTable cumplimientoDEncabezados = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 3) { WidthPercentage = 100 };
            cumplimientoDEncabezados.SetWidths(new float[] { 0.232f, 0.08925f, 0.08925f, 0.08925f, 0.08925f, 0.08925f, 0.08925f, 0.08925f, 0.08925f, 0.054f });
            PdfPCell general = new PdfPCell(new Phrase(" ", FuenteNegritasColor))
            {
                BackgroundColor = new BaseColor(0, 0, 0),
                //Border = Rectangle.NO_BORDER
            };
            cumplimientoDEncabezados.AddCell(general);

            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(0, 0, 0),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimientoDEncabezados.AddCell(celda);
            }
            PdfPCell general1 = new PdfPCell(new Phrase("Total", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(0, 0, 0),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cumplimientoDEncabezados.AddCell(general1);
            PdfPCell general2 = new PdfPCell(new Phrase(" ", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(0, 0, 0),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cumplimientoDEncabezados.AddCell(general2);
            document.Add(cumplimientoDEncabezados);
            //CUERPO
            PdfPTable cumplimientoDCuerpo = new PdfPTable((cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() * 2) + 5) { WidthPercentage = 100 };
            cumplimientoDCuerpo.SetWidths(new float[] { 0.2f, 0.032f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.054f });
            PdfPCell cuerpo2 = new PdfPCell(new Phrase("Destino", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            cumplimientoDCuerpo.AddCell(cuerpo2);
            PdfPCell cuerpo3 = new PdfPCell(new Phrase("Establo", FuenteNegritasColor))
            {
                //Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            cumplimientoDCuerpo.AddCell(cuerpo3);

            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase("Ppto", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimientoDCuerpo.AddCell(celda);
                celda = new PdfPCell(new Phrase("Real", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimientoDCuerpo.AddCell(celda);
            }
            PdfPCell cuerpo4 = new PdfPCell(new Phrase("Ppto", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cumplimientoDCuerpo.AddCell(cuerpo4);
            PdfPCell cuerpo5 = new PdfPCell(new Phrase("Real", FuenteNegritasColor))
            {
                //  Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cumplimientoDCuerpo.AddCell(cuerpo5);
            PdfPCell cuerpo6 = new PdfPCell(new Phrase("Cumplimiento", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cumplimientoDCuerpo.AddCell(cuerpo6);

            foreach (var id_programacion in cliente.Select(x => x.id_programacion_semanal_cliente_g).Distinct())
            {
                PdfPCell cuerpo7 = new PdfPCell(new Phrase(cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_destinos.nombre_destino, FuenteGeneral))
                {
                    // Border = iTextSharp.text.Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                cumplimientoDCuerpo.AddCell(cuerpo7);
                PdfPCell cuerpo8 = new PdfPCell(new Phrase(cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas, FuenteGeneral))
                {
                    //  Border = iTextSharp.text.Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimientoDCuerpo.AddCell(cuerpo8);
                foreach (var item in cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion))
                {
                    decimal total_ppto_dias = (decimal)item.cantidad_litros;
                    PdfPCell cuerpo9 = new PdfPCell(new Phrase("" + total_ppto_dias.ToString("N0"), FuenteGeneral))
                    {
                        // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_RIGHT

                    };
                    cumplimientoDCuerpo.AddCell(cuerpo9);
                    try
                    {
                        DateTime fecha_real = item.fecha_dia.Value.AddHours(12).AddMinutes(59).AddSeconds(59);
                        decimal total_real_dias = Convert.ToDecimal(envio_leche.Where(x => x.fecha_envio < fecha_real && fecha_real > item.fecha_dia.Value && x.id_destino_envio_leche == item.C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche && x.id_envio_leche_cliente_ms == item.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche && x.id_establo_envio == item.C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum());
                        PdfPCell cuerpo10 = new PdfPCell(new Phrase("" + total_real_dias.ToString("N0"), FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimientoDCuerpo.AddCell(cuerpo10);
                    }
                    catch
                    {
                        PdfPCell cuerpo11 = new PdfPCell(new Phrase("0", FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimientoDCuerpo.AddCell(cuerpo11);
                    }
                }
                decimal total_ppto = Convert.ToDecimal(cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion).Select(x => x.cantidad_litros).Sum());
                PdfPCell cuerpo12 = new PdfPCell(new Phrase("" + total_ppto.ToString("N0"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimientoDCuerpo.AddCell(cuerpo12);

                var cliente_leche = cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion);
                decimal total_real = Convert.ToDecimal(envio_leche.Where(x => x.id_destino_envio_leche == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche && x.id_envio_leche_cliente_ms == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche && x.id_establo_envio == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum());
                PdfPCell cuerpo13 = new PdfPCell(new Phrase(" " + total_real.ToString("N0"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimientoDCuerpo.AddCell(cuerpo13);

                decimal total = 0;
                try
                {
                    total = Convert.ToDecimal(envio_leche.Where(x => x.id_destino_envio_leche == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_destino_envio_leche && x.id_envio_leche_cliente_ms == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche && x.id_establo_envio == cliente_leche.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum()) / Convert.ToDecimal(cliente.Where(x => x.id_programacion_semanal_cliente_g == id_programacion).Select(x => x.cantidad_litros).Sum());
                }
                catch (Exception) { }
                PdfPCell cuerpo14 = new PdfPCell(new Phrase("" + total.ToString("P"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimientoDCuerpo.AddCell(cuerpo14);
            }

            document.Add(cumplimientoDCuerpo);
            #endregion
            document.Add(new iTextSharp.text.Paragraph("\n"));
            #region CUMPLIMIENTO POR CLIENTE
            PdfPTable c_cliente = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell c_encabezado_cliente = new PdfPCell(new Phrase("CUMPLIMIENTO POR CLIENTE", FuenteNegritasMayor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            c_cliente.AddCell(c_encabezado_cliente);
            document.Add(c_cliente);
            document.Add(cumplimientoDEncabezados);


            PdfPTable cumplimiento_cliente = new PdfPTable((cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() * 2) + 4) { WidthPercentage = 100 };
            cumplimiento_cliente.SetWidths(new float[] { 0.232f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.054f });
            PdfPCell cliente_cumplimiento = new PdfPCell(new Phrase("Cliente", FuenteNegritasColor))
            {
                // Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            cumplimiento_cliente.AddCell(cliente_cumplimiento);

            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase("Ppto", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_cliente.AddCell(celda);
                celda = new PdfPCell(new Phrase("Real", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_cliente.AddCell(celda);
            }
            cumplimiento_cliente.AddCell(cuerpo4);
            cumplimiento_cliente.AddCell(cuerpo5);
            cumplimiento_cliente.AddCell(cuerpo6);

            foreach (var g_cliente in cliente.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche))
            {
                PdfPCell cuerpo7 = new PdfPCell(new Phrase(g_cliente.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial, FuenteGeneral))
                {
                    // Border = iTextSharp.text.Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                cumplimiento_cliente.AddCell(cuerpo7);
                var dias_cumplimiento_cliente = g_cliente.Select(x => x.fecha_dia).Distinct();

                foreach (var item in dias_cumplimiento_cliente)
                {
                    DateTime cliente_dia = item.Value.AddHours(12).AddMinutes(59).AddSeconds(59);
                    decimal total_ppto_dias = (decimal)g_cliente.Where(x => x.fecha_dia < cliente_dia && x.fecha_dia > item.Value).Select(x => x.cantidad_litros).Sum();
                    PdfPCell cuerpo9 = new PdfPCell(new Phrase("" + total_ppto_dias.ToString("N0"), FuenteGeneral))
                    {
                        // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_RIGHT

                    };
                    cumplimiento_cliente.AddCell(cuerpo9);
                    try
                    {
                        decimal total_real_dias = Convert.ToDecimal(envio_leche.Where(x => x.fecha_envio < cliente_dia && x.fecha_envio > item.Value && x.id_envio_leche_cliente_ms == g_cliente.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).Select(x => x.litros_totales).Sum());
                        PdfPCell cuerpo10 = new PdfPCell(new Phrase("" + total_real_dias.ToString("N0"), FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimiento_cliente.AddCell(cuerpo10);
                    }
                    catch
                    {
                        PdfPCell cuerpo11 = new PdfPCell(new Phrase("0", FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimiento_cliente.AddCell(cuerpo11);
                    }
                }
                decimal total_ppto = (decimal)g_cliente.Select(x => x.cantidad_litros).Sum();
                PdfPCell cuerpo12 = new PdfPCell(new Phrase("" + total_ppto.ToString("N0"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimiento_cliente.AddCell(cuerpo12);

                decimal total_real = (decimal)envio_leche.Where(x => x.id_envio_leche_cliente_ms == g_cliente.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).Select(x => x.litros_totales).Sum();
                PdfPCell cuerpo13 = new PdfPCell(new Phrase(" " + total_real.ToString("N0"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimiento_cliente.AddCell(cuerpo13);

                decimal total = 0;
                try
                {
                    total = ((decimal)envio_leche.Where(x => x.id_envio_leche_cliente_ms == g_cliente.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).Select(x => x.litros_totales).Sum()) / ((decimal)g_cliente.Select(x => x.cantidad_litros).Sum());
                }
                catch (Exception) { }
                PdfPCell cuerpo14 = new PdfPCell(new Phrase("" + total.ToString("P"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_cliente.AddCell(cuerpo14);
            }
            document.Add(cumplimiento_cliente);

            #endregion
            document.Add(new iTextSharp.text.Paragraph("\n"));
            #region CUMPLIMIENTO ESTABLO
            PdfPTable c_establo = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell c_encabezado_establo = new PdfPCell(new Phrase("CUMPLIMIENTO POR ESTABLO", FuenteNegritasMayor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            c_establo.AddCell(c_encabezado_establo);
            document.Add(c_establo);
            document.Add(cumplimientoDEncabezados);


            PdfPTable cumplimiento_establo = new PdfPTable((cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() * 2) + 4) { WidthPercentage = 100 };
            cumplimiento_establo.SetWidths(new float[] { 0.232f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.054f });
            cumplimiento_establo.AddCell(cuerpo3);

            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase("Ppto", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_establo.AddCell(celda);
                celda = new PdfPCell(new Phrase("Real", FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_establo.AddCell(celda);
            }
            cumplimiento_establo.AddCell(cuerpo4);
            cumplimiento_establo.AddCell(cuerpo5);
            cumplimiento_establo.AddCell(cuerpo6);

            foreach (var g_establo in cliente.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo))
            {
                var dias_cumplimiento_establo = g_establo.Select(x => x.fecha_dia).Distinct();
                PdfPCell establo_cumplimiento = new PdfPCell(new Phrase("" + g_establo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo, FuenteGeneral))
                {
                    // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE

                };
                cumplimiento_establo.AddCell(establo_cumplimiento);
                foreach (var item in dias_cumplimiento_establo)
                {
                    DateTime cliente_dia = item.Value.AddHours(12).AddMinutes(59).AddSeconds(59);
                    decimal total_ppto_dias = (decimal)g_establo.Where(x => x.fecha_dia < cliente_dia && x.fecha_dia > item.Value).Select(x => x.cantidad_litros).Sum();
                    PdfPCell cuerpo9 = new PdfPCell(new Phrase("" + total_ppto_dias.ToString("N0"), FuenteGeneral))
                    {
                        // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_RIGHT

                    };
                    cumplimiento_establo.AddCell(cuerpo9);
                    try
                    {
                        decimal total_real_dias = Convert.ToDecimal(envio_leche.Where(x => x.fecha_envio < cliente_dia && x.fecha_envio > item.Value && x.id_establo_envio == g_establo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum());
                        PdfPCell cuerpo10 = new PdfPCell(new Phrase("" + total_real_dias.ToString("N0"), FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimiento_establo.AddCell(cuerpo10);
                    }
                    catch
                    {
                        PdfPCell cuerpo11 = new PdfPCell(new Phrase("0", FuenteGeneral))
                        {
                            // Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        cumplimiento_establo.AddCell(cuerpo11);
                    }
                }
                decimal total_ppto = (decimal)g_establo.Select(x => x.cantidad_litros).Sum();
                PdfPCell cuerpo12 = new PdfPCell(new Phrase("" + total_ppto.ToString("N0"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimiento_establo.AddCell(cuerpo12);

                decimal total_real = (decimal)envio_leche.Where(x => x.id_establo_envio == g_establo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum();
                PdfPCell cuerpo13 = new PdfPCell(new Phrase(" " + total_real.ToString("N"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                cumplimiento_establo.AddCell(cuerpo13);

                decimal total = 0;
                try
                {
                    total = ((decimal)envio_leche.Where(x => x.id_establo_envio == g_establo.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_establo).Select(x => x.litros_totales).Sum()) / ((decimal)g_establo.Select(x => x.cantidad_litros).Sum());
                }
                catch (Exception) { }
                PdfPCell cuerpo14 = new PdfPCell(new Phrase("" + total.ToString("P"), FuenteGeneral))
                {
                    //Border = iTextSharp.text.Rectangle.LEFT_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                cumplimiento_establo.AddCell(cuerpo14);
            }
            document.Add(cumplimiento_establo);

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

        public void GenerarCumplimientoEnviosLecheCompletoPDF(int anio, int nosemana)
        {

            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            var fechas = ObtenerSemanaEdicion(Convert.ToInt32(cliente.FirstOrDefault().C_envios_leche_programacion_semanal_g.C_presupuestos_anios.anio), nosemana);
            var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fechas.PrimerDiaSemana && x.fecha_envio <= fechas.UltimoDiaSemana && x.activo == true).ToList();

            var produccion_semanal = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true &&
            x.C_envios_leche_programacion_diaria_g.no_semana == nosemana && x.C_envios_leche_programacion_diaria_g.activo == true && x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto == anio).ToList();

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            #region FUENTE Y TIPOGRAFIA
            var FuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var FuenteTituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            var FuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var FuenteSubtituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var FuenteGeneral = FontFactory.GetFont(FontFactory.HELVETICA, 6);
            var FuenteNegritasMayor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var FuenteNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6);
            var FuenteNegritasColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, BaseColor.WHITE);
            var FuenteSubrayada = FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteSubrayadaNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteError = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40);
            FuenteError.Color = BaseColor.RED;
            #endregion

            #region TITULO DEL DOCUMENTO
            PdfPTable encabezadoG = new PdfPTable(2) { WidthPercentage = 100 };
            encabezadoG.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            encabezadoG.SetWidths(new float[] { 0.15f, 0.85f });

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
                encabezadoG.AddCell(logoCell);
            }
            PdfPTable encabezadoD = new PdfPTable(1) { WidthPercentage = 100 };
            encabezadoD.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            // Título de la tabla
            PdfPCell celdaTitulo1 = new PdfPCell(new Phrase("Sistema de Informacion Integral Beta", FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo1);
            PdfPCell celdaTitulo2 = new PdfPCell(new Phrase("PROGRAMAS DE ENTREGA DE LECHE DEL " + fechas.PrimerDiaSemana.ToShortDateString() + " AL " + fechas.UltimoDiaSemana.ToShortDateString(), FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo2);
            encabezadoG.AddCell(encabezadoD);
            document.Add(encabezadoG);

            document.Add(new iTextSharp.text.Paragraph("\n"));
            #endregion

            PdfPTable tablaSalto = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell celdaSalto = new PdfPCell(new Phrase("\n", FuenteNegritas)) { Border = Rectangle.NO_BORDER };
            tablaSalto.AddCell(celdaSalto);

            #region TABLA PRODUCCION DIARIA ESTABLOS SM Y SG
            PdfPTable tablaProduccionEstablos = new PdfPTable(3) { WidthPercentage = 100 };
            tablaProduccionEstablos.SetWidths(new float[] { 0.15f, 0.56875f, 0.28125f });

            PdfPCell celdaSaltoAzul = new PdfPCell(new Phrase("\n", FuenteNegritas))
            {
                BackgroundColor = new BaseColor(46, 135, 215),
                Border = Rectangle.NO_BORDER
            };

            tablaProduccionEstablos.AddCell(celdaSalto);
            PdfPCell C_Produccion = new PdfPCell(new Phrase("Producción diaria", FuenteNegritasColor))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            tablaProduccionEstablos.AddCell(C_Produccion);
            tablaProduccionEstablos.AddCell(celdaSalto);
            #endregion

            document.Add(tablaProduccionEstablos);

            #region TABLA PRODUCCION DIARIA ESTABLOS SM Y SG - DETALLE
            PdfPTable tablaProduccionEstablosDetalle = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 4) { WidthPercentage = 100 };
            //tablaProduccionEstablosDetalle.SetWidths(new float[] { 0.25f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.1f });
            tablaProduccionEstablosDetalle.SetWidths(new float[] { 0.15f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.1f, 0.1f });

            tablaProduccionEstablosDetalle.AddCell(celdaSaltoAzul);
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                tablaProduccionEstablosDetalle.AddCell(celda);
            }
            PdfPCell tablaProduccionEstablosDetalleTotal = new PdfPCell(new Phrase("Total lts", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEstablosDetalle.AddCell(tablaProduccionEstablosDetalleTotal);
            PdfPCell tablaProduccionEstablosDetalleUnidades = new PdfPCell(new Phrase("Cantidad de unidades", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEstablosDetalle.AddCell(tablaProduccionEstablosDetalleUnidades);

            tablaProduccionEstablosDetalle.AddCell(celdaSalto);
            #endregion

            document.Add(tablaProduccionEstablosDetalle);

            #region TABLA PRODUCCION DIARIA ESTABLOS SM Y SG - DETALLE INFO
            PdfPTable tablaProduccionEstablosDetalleInfo = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 5) { WidthPercentage = 100 };
            //tablaProduccionEstablosDetalleInfo.SetWidths(new float[] { 0.25f,0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f,0.05f, 0.05f });
            tablaProduccionEstablosDetalleInfo.SetWidths(new float[] { 0.15f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });
            PdfPCell tablaProduccionEstablosDetalleInfoProgramado = new PdfPCell(new Phrase("Lts programados de producción", FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
            };
            tablaProduccionEstablosDetalleInfo.AddCell(tablaProduccionEstablosDetalleInfoProgramado);

            int dias = 0;
            int lts_totales = 0;
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                dias++;
                DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                decimal produccion = (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.produccion) +
               (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.sobrante) -
               (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.faltante);

                PdfPCell litros_dia = new PdfPCell(new Phrase(produccion.ToString("N2"), FuenteGeneral))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = new BaseColor(255, 255, 208)
                };

                lts_totales += Convert.ToInt32(produccion);
                tablaProduccionEstablosDetalleInfo.AddCell(litros_dia);
            }
            PdfPCell tablaProduccionEstablosDetalleInfoLtrs = new PdfPCell(new Phrase(lts_totales.ToString("N2"), FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEstablosDetalleInfo.AddCell(tablaProduccionEstablosDetalleInfoLtrs);
            double unidades = lts_totales / 55000;
            PdfPCell tablaProduccionEstablosDetalleInfoUnidades = new PdfPCell(new Phrase(unidades.ToString("N3"), FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEstablosDetalleInfo.AddCell(tablaProduccionEstablosDetalleInfoUnidades);
            PdfPCell tablaProduccionEstablosDetalleInfoFull = new PdfPCell(new Phrase("Fulles", FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEstablosDetalleInfo.AddCell(tablaProduccionEstablosDetalleInfoFull);

            tablaProduccionEstablosDetalleInfo.AddCell(celdaSalto);
            #endregion

            document.Add(tablaProduccionEstablosDetalleInfo);

            document.Add(tablaSalto);

            PdfPCell celdaCliente = new PdfPCell(new Phrase("Cliente", FuenteNegritasColor))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            PdfPCell celdaEstabo = new PdfPCell(new Phrase("Establo", FuenteNegritasColor))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celdaDias = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
            }
            PdfPCell celdaTotalLts = new PdfPCell(new Phrase("Total lts", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            PdfPCell celdaCantidadUnidades = new PdfPCell(new Phrase("Cantidad de unidades", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            #region TABLA CUMPLOMIENTO POR CLIENTE
            PdfPTable tablaCumplimientoCliente = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 5) { WidthPercentage = 100 };
            tablaCumplimientoCliente.SetWidths(new float[] { 0.1f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.1f, 0.1f });
            PdfPCell celdaTransportista = new PdfPCell(new Phrase("Transportista", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            tablaCumplimientoCliente.AddCell(celdaCliente);
            tablaCumplimientoCliente.AddCell(celdaEstabo);
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celdaDias = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                tablaCumplimientoCliente.AddCell(celdaDias);
            }
            tablaCumplimientoCliente.AddCell(celdaTotalLts);
            tablaCumplimientoCliente.AddCell(celdaCantidadUnidades);
            tablaCumplimientoCliente.AddCell(celdaTransportista);
            #endregion

            document.Add(tablaCumplimientoCliente);

            #region TABLA CUMPLOMIENTO POR CLIENTE - DETALLE
            PdfPTable tablaCumplimientoClienteDetalle = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 6) { WidthPercentage = 100 };
            tablaCumplimientoClienteDetalle.SetWidths(new float[] { 0.1f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });
            foreach (var programa in cliente.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche))
            {
                PdfPCell datos_cliente_celda = new PdfPCell(new Phrase(programa.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial, FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE };
                tablaCumplimientoClienteDetalle.AddCell(datos_cliente_celda);

                string establos = "";
                foreach (var item in programa.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas).Distinct())
                {
                    establos += item + ",";
                }
                PdfPCell datos_establo_celda = new PdfPCell(new Phrase(establos, FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaCumplimientoClienteDetalle.AddCell(datos_establo_celda);

                int contador = 0;
                decimal cliente_litros = 0;
                foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
                {
                    DateTime f1 = fechas.PrimerDiaSemana.AddDays(contador);
                    contador++;
                    DateTime f2 = fechas.PrimerDiaSemana.AddDays(contador).AddSeconds(-1);
                    decimal litros_establo_cliente = (decimal)programa.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);
                    PdfPCell litros_dia = new PdfPCell(new Phrase(litros_establo_cliente.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                    cliente_litros += (decimal)programa.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);
                    tablaCumplimientoClienteDetalle.AddCell(litros_dia);
                }

                PdfPCell litros_totales = new PdfPCell(new Phrase(cliente_litros.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaCumplimientoClienteDetalle.AddCell(litros_totales);

                decimal full_cliente = 0;
                full_cliente = cliente_litros / 55000;
                PdfPCell envios_litros_totales = new PdfPCell(new Phrase(full_cliente.ToString("N3"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaCumplimientoClienteDetalle.AddCell(envios_litros_totales);

                PdfPCell envios_litros_totales_full = new PdfPCell(new Phrase("Fulles", FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaCumplimientoClienteDetalle.AddCell(envios_litros_totales_full);

                PdfPCell linea_transportista = new PdfPCell(new Phrase("Por definir", FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaCumplimientoClienteDetalle.AddCell(linea_transportista);
            }
            #endregion

            document.Add(tablaCumplimientoClienteDetalle);

            document.Add(tablaSalto);

            #region TABLA LITROS PROGRAMADOS PARA EMBARQUE

            #region EMBARQUE
            PdfPTable tablaProduccionEmbarque = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 5) { WidthPercentage = 100 };
            //tablaProduccionEmbarque.SetWidths(new float[] { 0.25f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.05f, 0.05f });
            tablaProduccionEmbarque.SetWidths(new float[] { 0.15f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });
            PdfPCell tablaProduccionEmbarqueProgramado = new PdfPCell(new Phrase("Lts programados para embarque", FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
            };
            tablaProduccionEmbarque.AddCell(tablaProduccionEmbarqueProgramado);

            dias = 0;
            decimal lts_totalesEmbarque = 0;
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                dias++;
                DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                decimal produccion = (decimal)cliente.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);

                PdfPCell litros_dia = new PdfPCell(new Phrase(produccion.ToString("N2"), FuenteGeneral))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    BackgroundColor = new BaseColor(255, 255, 208)
                };

                lts_totalesEmbarque += Convert.ToInt32(produccion);
                tablaProduccionEmbarque.AddCell(litros_dia);
            }
            PdfPCell tablaProduccionEmbarqueLtrs = new PdfPCell(new Phrase(lts_totalesEmbarque.ToString("N2"), FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEmbarque.AddCell(tablaProduccionEmbarqueLtrs);
            decimal unidadesEmbarque = lts_totalesEmbarque / 55000;
            PdfPCell tablaProduccionEmbarqueUnidades = new PdfPCell(new Phrase(unidadesEmbarque.ToString("N3"), FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEmbarque.AddCell(tablaProduccionEmbarqueUnidades);
            PdfPCell tablaProduccionEmbarqueFull = new PdfPCell(new Phrase("Fulles", FuenteNegritas))
            {
                BackgroundColor = new BaseColor(255, 255, 208),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEmbarque.AddCell(tablaProduccionEmbarqueFull);

            tablaProduccionEmbarque.AddCell(celdaSalto);
            #endregion

            dias = 0;

            #region EXCEDENTES
            PdfPCell celdaExcedebte = new PdfPCell(new Phrase("Excedentes", FuenteNegritas))
            {

            };
            tablaProduccionEmbarque.AddCell(celdaExcedebte);

            double lts_programacion = 0;
            double lts_establo = 0;
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                dias++;
                DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                decimal prorgrama_produccion = (decimal)cliente.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);

                decimal produccion = (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.produccion) +
               (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.sobrante) -
               (decimal)produccion_semanal.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.faltante);

                decimal total = prorgrama_produccion - produccion;

                PdfPCell litros_dia = new PdfPCell(new Phrase(total.ToString("N2"), FuenteGeneral))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                lts_programacion += Convert.ToInt32(prorgrama_produccion);
                lts_establo += Convert.ToInt32(produccion);

                tablaProduccionEmbarque.AddCell(litros_dia);
            }
            double total_programacion = lts_programacion - lts_establo;
            PdfPCell celdaTotalProgramacion = new PdfPCell(new Phrase(total_programacion.ToString("N2"), FuenteNegritas))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEmbarque.AddCell(celdaTotalProgramacion);

            double celdaCantidadEmbarque = total_programacion / 55000;
            PdfPCell celdaEmbarque = new PdfPCell(new Phrase(celdaCantidadEmbarque.ToString("N3"), FuenteNegritas))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaProduccionEmbarque.AddCell(celdaSalto);
            tablaProduccionEmbarque.AddCell(celdaSalto);
            tablaProduccionEmbarque.AddCell(celdaSalto);
            #endregion

            #endregion

            document.Add(tablaProduccionEmbarque);

            document.Add(tablaSalto);

            #region TABLA CUMPLIMIENTO POR ESTABLO
            PdfPTable tablaCumplimientoEstablo = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 5) { WidthPercentage = 100 };
            //tablaCumplimientoEstablo.SetWidths(new float[] { 0.2f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.1f });
            tablaCumplimientoEstablo.SetWidths(new float[] { 0.1f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.1f, 0.1f });

            tablaCumplimientoEstablo.AddCell(celdaCliente);
            tablaCumplimientoEstablo.AddCell(celdaEstabo);
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celdaDias = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    Border = Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                tablaCumplimientoEstablo.AddCell(celdaDias);
            }
            tablaCumplimientoEstablo.AddCell(celdaTotalLts);
            tablaCumplimientoEstablo.AddCell(celdaCantidadUnidades);
            tablaCumplimientoEstablo.AddCell(celdaSalto);
            #endregion

            document.Add(tablaCumplimientoEstablo);

            #region TABLA CUMPLIMIENTO POR ESTABLO - DETALLE
            PdfPTable tablaCumplimientoEstabloDetalle = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 6) { WidthPercentage = 100 };
            //tablaCumplimientoEstabloDetalle.SetWidths(new float[] { 0.2f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f });
            tablaCumplimientoEstabloDetalle.SetWidths(new float[] { 0.1f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });

            //tablaCumplimientoEstabloDetalle.SetWidths(new float[] { 0.2f, 0.032f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.044625f, 0.054f });            

            foreach (var establo in cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).Distinct())
            {
                foreach (var programa in cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche))
                {
                    PdfPCell datos_cliente_celda = new PdfPCell(new Phrase(programa.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial, FuenteGeneral))
                    {
                        //Border = iTextSharp.text.Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaCumplimientoEstabloDetalle.AddCell(datos_cliente_celda);

                    PdfPCell datos_establo_celda = new PdfPCell(new Phrase(programa.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas, FuenteGeneral))
                    {
                        //Border = iTextSharp.text.Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaCumplimientoEstabloDetalle.AddCell(datos_establo_celda);
                    int contador = 0;
                    decimal cantidad_litros_totales = 0;
                    foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
                    {
                        DateTime f1 = fechas.PrimerDiaSemana.AddDays(contador);
                        contador++;
                        DateTime f2 = fechas.PrimerDiaSemana.AddDays(contador).AddSeconds(-1);
                        decimal establo_litros_cliente = (decimal)programa.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);
                        PdfPCell litros_dia = new PdfPCell(new Phrase(establo_litros_cliente.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                        cantidad_litros_totales += (decimal)programa.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);
                        tablaCumplimientoEstabloDetalle.AddCell(litros_dia);
                    }
                    PdfPCell litros_totales = new PdfPCell(new Phrase(cantidad_litros_totales.ToString("N2"), FuenteGeneral))
                    {
                        //Border = iTextSharp.text.Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaCumplimientoEstabloDetalle.AddCell(litros_totales);

                    decimal full_envio = 0;
                    full_envio = cantidad_litros_totales / 55000;
                    PdfPCell envios_litros_totales = new PdfPCell(new Phrase(full_envio.ToString("N3"), FuenteGeneral))
                    {
                        //Border = iTextSharp.text.Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaCumplimientoEstabloDetalle.AddCell(envios_litros_totales);
                    PdfPCell envios_litros_totales_full = new PdfPCell(new Phrase("Fulles", FuenteGeneral))
                    {
                        //Border = iTextSharp.text.Rectangle.NO_BORDER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    tablaCumplimientoEstabloDetalle.AddCell(envios_litros_totales_full);
                    tablaCumplimientoEstabloDetalle.AddCell(celdaSalto);

                }
            }
            #endregion

            document.Add(tablaCumplimientoEstabloDetalle);

            document.Add(tablaSalto);

            #region TABLA LITROS POR ESTABLO
            PdfPTable tablaLitros = new PdfPTable(3) { WidthPercentage = 100 };
            tablaLitros.SetWidths(new float[] { 0.15f, 0.75f, 0.1f });
            tablaLitros.AddCell(celdaSalto);

            PdfPCell celdaLts = new PdfPCell(new Phrase("LTS POR ESTABLO", FuenteNegritasColor))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            tablaLitros.AddCell(celdaLts);
            tablaLitros.AddCell(celdaSalto);
            #endregion

            document.Add(tablaLitros);

            #region TABLA LITROS POR ESTABLO - DETALLE
            PdfPTable tablaLtsDetalle = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 4) { WidthPercentage = 100 };
            tablaLtsDetalle.SetWidths(new float[] { 0.15f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.1f, 0.1f });

            tablaLtsDetalle.AddCell(celdaSaltoAzul);
            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                PdfPCell celda = new PdfPCell(new Phrase(item + " " + Convert.ToDateTime(cliente.Where(x => x.C_presupuestos_dias.dia == item).FirstOrDefault().fecha_dia.Value).Day, FuenteNegritasColor))
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    BackgroundColor = new BaseColor(46, 135, 215),
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                tablaLtsDetalle.AddCell(celda);
            }
            PdfPCell tablaLtsDetalleTotal = new PdfPCell(new Phrase("Total lts", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaLtsDetalle.AddCell(tablaLtsDetalleTotal);
            PdfPCell tablaLtsDetalleUnidades = new PdfPCell(new Phrase("Cantidad de unidades", FuenteNegritasColor))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(46, 135, 215),
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            tablaLtsDetalle.AddCell(tablaLtsDetalleUnidades);

            tablaLtsDetalle.AddCell(celdaSalto);
            #endregion

            document.Add(tablaLtsDetalle);

            #region TABLA PRODUCCION DIARIA ESTABLOS SM Y SG - DETALLE INFO
            PdfPTable tablaLtsDetalleInfo = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 5) { WidthPercentage = 100 };
            //tablaLtsDetalleInfo.SetWidths(new float[] { 0.25f,0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f, 0.0875f,0.05f, 0.05f });
            tablaLtsDetalleInfo.SetWidths(new float[] { 0.15f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });
            PdfPCell tablaLtsDetalleInfoProgramado = new PdfPCell(new Phrase("Lts programados para embarque total", FuenteNegritas))
            {
            };
            tablaLtsDetalleInfo.AddCell(tablaLtsDetalleInfoProgramado);

            dias = 0;
            decimal lts_establo_embarque = 0;
            decimal full_establo_embarque = 0;

            foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
            {
                DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                dias++;
                DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                decimal produccion = (decimal)cliente.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2).Sum(x => x.cantidad_litros);

                PdfPCell litros_dia = new PdfPCell(new Phrase(produccion.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, };
                lts_establo_embarque += produccion;
                tablaLtsDetalleInfo.AddCell(litros_dia);
            }

            PdfPCell tablaLtsDetalleInfoLtrs = new PdfPCell(new Phrase(lts_establo_embarque.ToString("N2"), FuenteNegritas)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };

            tablaLtsDetalleInfo.AddCell(tablaLtsDetalleInfoLtrs);
            full_establo_embarque = lts_establo_embarque / 55000;

            PdfPCell tablaLtsDetalleInfoUnidades = new PdfPCell(new Phrase(full_establo_embarque.ToString("N3"), FuenteNegritas)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, };

            tablaLtsDetalleInfo.AddCell(tablaLtsDetalleInfoUnidades);

            PdfPCell tablaLtsDetalleInfoFull = new PdfPCell(new Phrase("Fulles", FuenteNegritas)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, };

            tablaLtsDetalleInfo.AddCell(tablaLtsDetalleInfoFull);
            tablaLtsDetalleInfo.AddCell(celdaSalto);

            foreach (var establo in cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).Distinct())
            {
                dias = 0;
                lts_establo_embarque = 0;
                full_establo_embarque = 0;
                PdfPCell celdaLtsEstablo = new PdfPCell(new Phrase("Lts programados para embarque " + cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas, FuenteNegritas)) { };
                tablaLtsDetalleInfo.AddCell(celdaLtsEstablo);

                foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
                {
                    DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                    dias++;
                    DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                    decimal produccion = (decimal)cliente.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2 && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Sum(x => x.cantidad_litros);

                    PdfPCell litros_dia = new PdfPCell(new Phrase(produccion.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };

                    lts_establo_embarque += Convert.ToInt32(produccion);
                    tablaLtsDetalleInfo.AddCell(litros_dia);
                }

                PdfPCell celdaTotalLtsEstablo = new PdfPCell(new Phrase(lts_establo_embarque.ToString("N2"), FuenteNegritas)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, };
                tablaLtsDetalleInfo.AddCell(celdaTotalLtsEstablo);

                full_establo_embarque = lts_establo_embarque / 55000;
                PdfPCell celdaUnidades = new PdfPCell(new Phrase(full_establo_embarque.ToString("N3"), FuenteNegritas)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER, };
                tablaLtsDetalleInfo.AddCell(celdaUnidades);
                tablaLtsDetalleInfo.AddCell(tablaLtsDetalleInfoFull);
                tablaLtsDetalleInfo.AddCell(celdaSalto);
            }
            document.Add(tablaLtsDetalleInfo);
            #endregion

            document.Add(tablaSalto);

            #region TABLAS SOBRANTE
            PdfPTable tablaSobranteEmbarque = new PdfPTable(cliente.Select(x => x.id_dia_presupuesto).Distinct().Count() + 6) { WidthPercentage = 100 };
            tablaSobranteEmbarque.SetWidths(new float[] { 0.1f, 0.05f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.08125f, 0.05f, 0.05f, 0.1f });
            PdfPCell celdaSobrante = new PdfPCell(new Phrase("SOBRANTE", FuenteNegritas)) { };
            foreach (var establo in cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo).Distinct())
            {
                dias = 0;
                lts_totales = 0;

                tablaSobranteEmbarque.AddCell(celdaSobrante);
                PdfPCell celdaEstablo = new PdfPCell(new Phrase("" + cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_establos.siglas, FuenteNegritas)) { };
                tablaSobranteEmbarque.AddCell(celdaEstablo);
                foreach (var item in cliente.Select(x => x.C_presupuestos_dias.dia).Distinct())
                {

                    DateTime f1 = fechas.PrimerDiaSemana.AddDays(dias);
                    dias++;
                    DateTime f2 = fechas.PrimerDiaSemana.AddDays(dias).AddSeconds(-1);
                    decimal produccion = (decimal)cliente.Where(x => x.fecha_dia >= f1 && x.fecha_dia <= f2 && x.C_envios_leche_programacion_semanal_cliente_d.id_establo == establo).Sum(x => x.cantidad_litros) -
                        (decimal)cliente.Where(x => x.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.id_establo == establo && x.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.C_envios_leche_programacion_diaria_d.Where(z => z.fecha_dia >= f1) != null && x.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.C_envios_leche_programacion_diaria_d.Where(z => z.fecha_dia <= f2) != null).Sum(x => x.C_envios_leche_programacion_semanal_g.C_envios_leche_programacion_diaria_g.C_envios_leche_programacion_diaria_d.Sum(y => y.produccion + y.sobrante - y.faltante));
                    PdfPCell litros_dia = new PdfPCell(new Phrase(produccion.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                    lts_totales += Convert.ToInt32(produccion);
                    tablaSobranteEmbarque.AddCell(litros_dia);
                }
                PdfPCell celdaTotalSobranteEstablo = new PdfPCell(new Phrase(lts_totales.ToString("N2"), FuenteGeneral)) { VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_CENTER };
                tablaSobranteEmbarque.AddCell(celdaTotalSobranteEstablo);

                tablaSobranteEmbarque.AddCell(celdaSalto);
                tablaSobranteEmbarque.AddCell(celdaSalto);
                tablaSobranteEmbarque.AddCell(celdaSalto);
            }
            document.Add(tablaSobranteEmbarque);
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

        public void GenerarCumplimientoEnviosLecheGeneralPDF(int anio, int nosemana, int mes)
        {
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            var fechas = ObtenerSemanaEdicion(Convert.ToInt32(cliente.FirstOrDefault().C_envios_leche_programacion_semanal_g.C_presupuestos_anios.anio), nosemana);
            //var envio_leche = db.C_envios_leche_g.Where(x => x.fecha_envio >= fechas.PrimerDiaSemana && x.fecha_envio <= fechas.UltimoDiaSemana && x.activo == true).ToList();

            var produccion_semanal = db.C_envios_leche_programacion_diaria_d.Where(x => x.activo == true &&
            x.C_envios_leche_programacion_diaria_g.no_semana == nosemana && x.C_envios_leche_programacion_diaria_g.activo == true && x.C_envios_leche_programacion_diaria_g.id_anio_presupuesto == anio).ToList();

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            #region FUENTE Y TIPOGRAFIA
            var FuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var FuenteTituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
            var FuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var FuenteSubtituloColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            var FuenteGeneral = FontFactory.GetFont(FontFactory.HELVETICA, 6);
            var FuenteNegritasMayor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var FuenteNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6);
            var FuenteNegritasColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, BaseColor.WHITE);
            var FuenteSubrayada = FontFactory.GetFont(FontFactory.HELVETICA, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteSubrayadaNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6, iTextSharp.text.Font.UNDERLINE);
            var FuenteError = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40);
            FuenteError.Color = BaseColor.RED;
            #endregion

            #region TITULO DEL DOCUMENTO
            PdfPTable encabezadoG = new PdfPTable(2) { WidthPercentage = 100 };
            encabezadoG.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            encabezadoG.SetWidths(new float[] { 0.15f, 0.85f });

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
                encabezadoG.AddCell(logoCell);
            }
            PdfPTable encabezadoD = new PdfPTable(1) { WidthPercentage = 100 };
            encabezadoD.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            // Título de la tabla
            PdfPCell celdaTitulo1 = new PdfPCell(new Phrase("Sistema de Informacion Integral Beta", FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo1);
            PdfPCell celdaTitulo2 = new PdfPCell(new Phrase("COMPROMISO DE ENTREGA DE LECHE DEL " + fechas.PrimerDiaSemana.ToShortDateString() + " AL " + fechas.UltimoDiaSemana.ToShortDateString(), FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezadoD.AddCell(celdaTitulo2);
            encabezadoG.AddCell(encabezadoD);
            document.Add(encabezadoG);

            document.Add(new iTextSharp.text.Paragraph("\n"));
            #endregion

            PdfPTable tablaSalto = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell celdaSalto = new PdfPCell(new Phrase("\n", FuenteNegritas)) { Border = Rectangle.NO_BORDER };
            tablaSalto.AddCell(celdaSalto);


            var clientes_cumplimiento = cliente.Select(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche).Distinct().ToArray();
            var compromiso_semanal = db.C_envios_leche_cumplimiento_semanal.Where(x => x.no_semana == nosemana && x.activo == true && x.C_envios_leche_cumplimiento_mensual.activo == true && x.C_envios_leche_cumplimiento_mensual.id_anio_presupuesto == anio && clientes_cumplimiento.Contains(x.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms)).ToList();

            string comentarioOriginal = "";
            if (compromiso_semanal.Count() > 0)
            {
                comentarioOriginal = compromiso_semanal.FirstOrDefault().comentario;
            }

           

            PdfPTable tablaComentario = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell celdaComentarioG = new PdfPCell(new Phrase("Comentario", FuenteNegritas)) { Border = Rectangle.NO_BORDER };
            PdfPCell celdaComentarioD = new PdfPCell(new Phrase(comentarioOriginal, FuenteGeneral)) { Border = Rectangle.NO_BORDER };
            tablaComentario.AddCell(celdaComentarioG);
            tablaComentario.AddCell(celdaComentarioD);
            tablaComentario.AddCell(celdaSalto);
            document.Add(tablaComentario);

            



            #region TABLA PRODUCCION DIARIA ESTABLOS SM Y SG
            string mes_header = fechas.UltimoDiaSemana.ToString("MMMM", new CultureInfo("es-MX"));
            string[] headers = { "Cliente", "Fulles compromiso por semana " + mes_header + " " + fechas.UltimoDiaSemana.Year, "Fulles programados semana " + nosemana, "Diferencia", "Observacion" };

            PdfPTable tablaCompromisos = new PdfPTable(headers.Length) { WidthPercentage = 100 };
            //tablaCompromisos.SetWidths(new float[] { });            
            foreach (var item in headers)
            {
                PdfPCell celdaHeaders = new PdfPCell(new Phrase(item, FuenteNegritasColor))
                {
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                tablaCompromisos.AddCell(celdaHeaders);
            }

           
            int id_mes = 0;

            if (compromiso_semanal.Count() > 0)
            {
                id_mes = (int)compromiso_semanal.FirstOrDefault().C_envios_leche_cumplimiento_mensual.id_mes_presupuesto;
            }
            else
            {
                id_mes = mes;
            }

            int semenas_mensual = db.C_envios_leche_cumplimiento_mensual.Where(x => x.id_mes_presupuesto == id_mes && x.id_anio_presupuesto == anio).Count();

            foreach (var item in cliente.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche))
            {
                int idClienteMS = (int)item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche;

                PdfPCell celdaCliente = new PdfPCell(new Phrase(item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial, FuenteGeneral))
                { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                tablaCompromisos.AddCell(celdaCliente);

                var compromiso_mensual = compromiso_semanal.Where(x => x.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms == idClienteMS).ToList();

                var cliente_leche = item.FirstOrDefault().C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche;
                if (compromiso_mensual.Count() > 0)
                {
                    var idCompromisoMensual = compromiso_mensual.Select(x => x.id_cumplimiento_mensual).Distinct().ToArray();
                    var observaciones = db.C_envios_leche_cumplimiento_semanal_comentario.Where(x => x.activo == true && idCompromisoMensual.Contains(x.C_envios_leche_cumplimiento_semanal.C_envios_leche_cumplimiento_mensual.id_cumplimiento_mensual)).ToList();

                    decimal cumplimiento_mensual_litros = ((decimal)compromiso_mensual.FirstOrDefault().C_envios_leche_cumplimiento_mensual.litros_totales / 55000) / semenas_mensual;
                    PdfPCell celdaFullCumplimiento = new PdfPCell(new Phrase(cumplimiento_mensual_litros.ToString("N3"), FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullCumplimiento);

                    decimal estimacion_establo = (decimal)cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente_leche).Sum(x => x.cantidad_litros) / 55000;
                    PdfPCell celdaFullProgramado = new PdfPCell(new Phrase(estimacion_establo.ToString("N3"), FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullProgramado);

                    decimal total_estimacion_cumplimiento = estimacion_establo - cumplimiento_mensual_litros;
                    PdfPCell celdaFullDiferencia = new PdfPCell(new Phrase(total_estimacion_cumplimiento.ToString("N3"), FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullDiferencia);

                    PdfPTable tablaObservaciones = new PdfPTable(1) { WidthPercentage = 100 };
                    tablaObservaciones.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    tablaObservaciones.HorizontalAlignment = Element.ALIGN_LEFT;

                    foreach (var obs in observaciones.Where(x => x.C_envios_leche_cumplimiento_semanal.C_envios_leche_cumplimiento_mensual.id_envio_leche_cliente_ms == cliente_leche))
                    {
                        PdfPCell celdaObs = new PdfPCell(new Phrase(obs.C_usuarios_corporativo.C_empleados.nombres + ": " + obs.comentario, FuenteGeneral))
                        { HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_CENTER, Border = iTextSharp.text.Rectangle.NO_BORDER };
                        tablaObservaciones.AddCell(celdaObs);
                    }
                    tablaCompromisos.AddCell(tablaObservaciones);
                }
                else
                {
                    PdfPCell celdaFullCumplimiento = new PdfPCell(new Phrase("0.000", FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullCumplimiento);

                    decimal estimacion_establo = (decimal)cliente.Where(x => x.C_envios_leche_programacion_semanal_cliente_d.id_cliente_envio_leche == cliente_leche).Sum(x => x.cantidad_litros) / 55000;
                    PdfPCell celdaFullProgramado = new PdfPCell(new Phrase(estimacion_establo.ToString("N3"), FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullProgramado);

                    decimal total_estimacion_cumplimiento = estimacion_establo - 0;
                    PdfPCell celdaFullDiferencia = new PdfPCell(new Phrase(total_estimacion_cumplimiento.ToString("N3"), FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaFullDiferencia);

                    PdfPCell celdaObs = new PdfPCell(new Phrase("", FuenteGeneral))
                    { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
                    tablaCompromisos.AddCell(celdaObs);
                }
            }
            document.Add(tablaCompromisos);
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

        #region Reporte del dia del programa de Envio de leche
        public ActionResult DescargarDetalleProgramaDiaExcel(int anio, int nosemana, DateTime fecha)
        {
            int id_usuario = (int)Session["LoggedId"];
            var cliente = db.C_envios_leche_programacion_semanal_cliente_d_dias.Where(x => x.C_envios_leche_programacion_semanal_g.no_semana == nosemana && x.C_envios_leche_programacion_semanal_g.id_anio == anio && x.activo == true).ToList();
            int[] id_cliente_d = cliente.Select(x => x.id_programacion_semanal_cliente_d_dia).ToArray();
            var detalle_programa = db.C_envios_leche_programacion_semanal_cliente_d_dias_horas.Where(x => id_cliente_d.Contains((int)x.id_programacion_semanal_cliente_d_dia) && x.cantidad_litros > 0 && x.activo == true).ToList();

            int itereacionGeneral = 0;
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("PROGRAMA DE CARGAS");

                // Insertar logo
                var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var image = worksheet.AddPicture(stream)
                                        .MoveTo(worksheet.Cell(1, 1))
                                        .Scale(0.2);
                }

                var encabezado = "PROGRAMA DE CARGAS POR ESTABLO";
                worksheet.Cell(1, 1).Value = encabezado;
                worksheet.Range(1, 1, 3, 35).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                    .Font.SetBold()
                    .Font.SetFontSize(22)
                    .Font.SetFontColor(XLColor.Black)
                    .Fill.SetBackgroundColor(XLColor.White);

                int currentRow = 4;
                int celdaMaxima = 0;


                foreach (var establo in detalle_programa.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.id_establo))
                {
                    int id_establo = (int)establo.Select(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.id_establo).FirstOrDefault();
                    string nombre_establo = detalle_programa.Where(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.id_establo == id_establo).Select(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.C_establos.nombre_establo).FirstOrDefault().ToUpper();

                    worksheet.Cell(currentRow, 1).Value = nombre_establo;
                    worksheet.Range(currentRow, 1, currentRow, 35).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                        .Font.SetBold()
                        .Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.FromArgb(38, 109, 173));
                    currentRow++;

                    int longitud_1 = 1;
                    int longitud_2 = 5;

                    #region ENCABEZADO DIAS
                    for (int i = 0; i < 7; i++)
                    {
                        string fechaLarga = fecha.AddDays(i).ToString("D", new CultureInfo("es-MX"));

                        worksheet.Cell(currentRow, longitud_1).Value = fechaLarga;
                        worksheet.Range(currentRow, longitud_1, currentRow, longitud_2).Merge().Style
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                            .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                            .Font.SetBold()
                            .Font.SetFontColor(XLColor.Black)
                            .Fill.SetBackgroundColor(XLColor.White);

                        longitud_1 = longitud_2 + 1;
                        longitud_2 = longitud_2 + 5;
                    }
                    #endregion
                    currentRow++;

                    longitud_1 = 1;
                    longitud_2 = 5;

                    #region ENCABEZADO - HEADER TABLA
                    for (int y = 0; y < 7; y++)
                    {
                        var headers = new[] { "HORA DE CARGA", "UNIDAD", "LITROS", "DESTINO", "LINEA" };
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet.Cell(currentRow, longitud_1 + i).Value = headers[i];
                            worksheet.Cell(currentRow, longitud_1 + i).Style
                                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                                .Font.SetBold()
                                .Font.SetFontColor(XLColor.White)
                                .Fill.SetBackgroundColor(XLColor.LightBlue);
                        }
                        longitud_1 = longitud_2 + 1;
                        longitud_2 = longitud_2 + 5;
                    }
                    #endregion
                    currentRow++;

                    int diaContador = 0;
                    int celdaNumero = 1;
                    foreach (var programado in establo.GroupBy(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.fecha_dia).ToList())
                    {
                        int celdaParcial = 0;
                        itereacionGeneral++;

                        //RENGLON PRINCIPAL
                        int currentRowReal = currentRow;

                        //POSICION CELDA INCREMENTABLE


                        //POSICION INICIAL MV,BXBV DE LA CELDA
                        int celdaNumero_original = 0;

                        //DETERMINAR EL REGISTRO DIARIO
                        int registroDiario = 0;
                        while (registroDiario != programado.Count())
                        {
                            DateTime fechaDiaIteracion = fecha.AddDays(diaContador);
                            int exite_programa = programado.Where(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.fecha_dia == fechaDiaIteracion).Count();
                            if (exite_programa > 0)
                            {
                                foreach (var dia in programado.Where(x => x.C_envios_leche_programacion_semanal_cliente_d_dias.fecha_dia == fechaDiaIteracion))
                                {
                                    celdaNumero_original = celdaNumero;
                                    worksheet.Cell(currentRow, celdaNumero).Value = dia.hora;
                                    worksheet.Cell(currentRow, celdaNumero).Style.DateFormat.Format = "hh:mm:ss AM/PM";
                                    celdaNumero++;
                                    worksheet.Cell(currentRow, celdaNumero).Value = dia.C_envios_leche_tipos_tanques.tipo_tanque;
                                    celdaNumero++;
                                    worksheet.Cell(currentRow, celdaNumero).Value = dia.cantidad_litros;
                                    celdaNumero++;
                                    worksheet.Cell(currentRow, celdaNumero).Value = dia.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_clientes.nombre_comercial + " " + dia.C_envios_leche_programacion_semanal_cliente_d_dias.C_envios_leche_programacion_semanal_cliente_d.C_envios_leche_destinos.nombre_destino;
                                    celdaNumero++;
                                    worksheet.Cell(currentRow, celdaNumero).Value = dia.C_bascula_lineas_transportistas.nombre_linea;

                                    currentRow++;
                                    registroDiario++;

                                    celdaNumero = celdaNumero_original;
                                    celdaParcial++;
                                }
                            }
                            else
                            {
                                //SE AVANZA AL SIGUIENTE DIA EN POSICION DE CELDA
                                celdaNumero = celdaNumero + 5;
                                currentRow = currentRowReal;
                            }
                            diaContador++;
                        }
                        currentRow = currentRowReal;
                        if (registroDiario == programado.Count())
                        {
                            celdaNumero = celdaNumero + 5;
                            currentRow = currentRowReal;
                        }

                        if (celdaParcial >= celdaMaxima)
                        {
                            celdaMaxima = celdaParcial;
                        }

                    }
                    currentRow = currentRow + celdaMaxima;
                    celdaMaxima = 0;
                }

                var dataRange = worksheet.Range(1, 1, currentRow - 1, 35);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                int itereacionGeneral2 = itereacionGeneral;
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProgramaCargas.xlsx");
                }
            }
        }
        #endregion

        #endregion

        #region CONFIGURACION Y GENERACION DE REPORTE PDF MEDIANTE VISTA PARCIAL
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
        public void GenerarReportePDF(string htmlContent, string encabezado, DateTime fecha_i, DateTime fecha_f, int orientacion_hoja, int tipo_hoja, string[] tamano)
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
            document.Add(GenerarEncabezadoReporte(encabezado, titleFont, cellFont, fecha_i, fecha_f));

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

        #endregion

        #region INVERSIONES
        public ActionResult DescargarExcelReporteInversionTable(int anio)
        {
            var inversiones = db.C_compras_inversiones_programadas.Where(x => x.activo == true && x.fecha_aplicacion.Value.Year == anio && x.id_requisicion_articulo_g != null && x.C_compras_requi_g.C_compras_ordenes_g.Count() > 0).ToList();

            List<ReporteInversiones> Inversion = new List<ReporteInversiones>();
            foreach (var item in inversiones)
            {
                ReporteInversiones reporte = new ReporteInversiones();
                try
                {
                    reporte.Justificacion = item.C_compras_requi_g.C_compras_requi_g_solicita_autorizacion.FirstOrDefault().justificacion;
                }
                catch (Exception)
                {
                    reporte.Justificacion = "";
                }
                reporte.Fecha = string.Format(item.C_compras_requi_g.C_compras_ordenes_g.FirstOrDefault().fecha_registro.Value.ToShortDateString(), "dd/MM/yyyy");
                try
                {
                    reporte.Concepto = item.concepto;
                }
                catch (Exception)
                {
                    reporte.Concepto = "";
                }
                try
                {
                    reporte.Folio = item.folio_inversion_requi;
                }
                catch (Exception)
                {
                    reporte.Folio = "";
                }
                reporte.OrdenCompra = item.C_compras_requi_g.C_compras_ordenes_g.FirstOrDefault().id_compras_orden_g;
                reporte.Requisicion = (int)item.id_requisicion_articulo_g;
                reporte.Centro = item.C_compras_requi_g.C_compras_ordenes_g.FirstOrDefault().C_centros_g.siglas;
                reporte.Usuario_registra = item.C_compras_requi_g.C_usuarios_corporativo5.usuario;
                reporte.Usuario_autoriza = item.C_compras_requi_g.C_compras_ordenes_g.FirstOrDefault().C_usuarios_corporativo.usuario;
                decimal monto_orden = 0;
                try
                {
                    var ordenes = item.C_compras_requi_g.C_compras_ordenes_g.Where(x => x.activo == true);
                    foreach (var detalle in ordenes)
                    {
                        monto_orden += (decimal)detalle.C_compras_ordenes_d.Where(X => X.activo == true).Select(y => y.cantidad_compra * y.precio_unitario).Sum();
                    }
                    ;

                    reporte.Monto_inversion = (decimal)item.presupuesto;
                    reporte.Monto_orden = monto_orden;
                    reporte.Diferencia = reporte.Monto_inversion - reporte.Monto_orden;
                }
                catch
                {
                    reporte.Monto_inversion = 0;
                    reporte.Monto_orden = monto_orden;
                    reporte.Diferencia = reporte.Monto_inversion - reporte.Monto_orden;
                }
                Inversion.Add(reporte);
            }
            string encabezado = "REPORTE COMPARATIVO DE INVERSIONES DEL " + new DateTime(anio, 01, 01).ToShortDateString() + " AL " + new DateTime(anio, 12, 31).ToShortDateString();
            string htmlContent = RenderPartialViewToStringObj(Inversion, "../COMPRAS/INVERSIONES/_ExcelReporteInversionTable");
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent, 1, 12, encabezado);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "REPORTE DE INVERSIONES.xlsx");
        }
        #endregion

        #region REPORTES DE EVALUACIONES

        public ActionResult EvaluacionesExcel(int id_evaluacion_g, int id_departamento)
        {
            var listaEmpleadosEvaluacion = db.C_evaluaciones_d_empleados.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.activo == true && x.id_evaluacion_status != 4 && x.id_departamento == id_departamento).OrderBy(x => x.C_nomina_empleados.Apellido_paterno).ToList();

            var listaEmpleadoEvaluados = db.C_evaluaciones_d_conceptos.Where(x => x.id_evaluacion_g == id_evaluacion_g && x.activo == true && x.id_evaluacion_status != 4 && x.C_evaluaciones_d_empleados.id_departamento == id_departamento).OrderBy(x => x.C_evaluaciones_d_empleados.C_nomina_empleados.Apellido_paterno).ToList();

            try
            {
                var grupoEmpleado = listaEmpleadoEvaluados.GroupBy(x => x.id_evaluacion_d_empleado);

                using (var workbook = new XLWorkbook())
                {
                    //SE VALIDA QUE EXISTA ALMENOS 1 EMPLEADO
                    if (grupoEmpleado.Any())
                    {
                        //ITERACION POR CADA EMPLEADO
                        bool registroEvaluador = true;
                        foreach (var item in grupoEmpleado)
                        {
                            registroEvaluador = false;

                            //NOMBRE DEL SHEET/HOJA
                            string hojaEmpleado = item.FirstOrDefault().C_evaluaciones_d_empleados.C_nomina_empleados.Nombres.ToUpper() + " " + item.FirstOrDefault().C_evaluaciones_d_empleados.C_nomina_empleados.Apellido_paterno.ToUpper();

                            var worksheet = workbook.Worksheets.Add(hojaEmpleado);
                            var currentRow = 1;
                            var columnCount = 5;

                            //SE OBTIENE UN LISTADO DE QUIEN EVALUO
                            var listaEvaluadores = item.Where(x => x.activo == true).GroupBy(x => x.id_usuario_registra);
                            string departamento = item.FirstOrDefault().C_evaluaciones_d_empleados.C_evaluaciones_departamentos.nombre_departamento.ToUpper();
                            string titulo = "EVALUACION - " + departamento;

                            columnCount = (listaEvaluadores.Distinct().Count() * 2) + 2;

                            ExcelEncabezadoTituloImagen(worksheet, titulo, currentRow, 1, 3, columnCount, true);
                            currentRow += 3;

                            var headers = new[] { "CONCEPTO", "PONDERACION" }.ToArray();
                            int headerCell = 0;

                            //ENCABEZADOS PRINCIPALES
                            for (int i = 0; i < headers.Length; i++)
                            {
                                ExcelEstiloEncabezado(worksheet.Cell(currentRow, i + 1), headers[i], XLColor.FromArgb(38, 109, 173), 1, true);
                                headerCell++;
                            }

                            //SE GUARDA LA POSICION PARA LOS ENCABEZADOS DINAMICOS
                            int rowHeader = currentRow;
                            int celdaHeader = headerCell;

                            currentRow++;
                            headerCell++;

                            //RUBRO
                            foreach (var rubro in item.GroupBy(x => x.C_evaluaciones_conceptos_rubros.id_evaluacion_rubro))
                            {
                                ExcelEstiloEncabezado(worksheet.Cell(currentRow, 1), rubro.FirstOrDefault().C_evaluaciones_conceptos_rubros.C_evaluaciones_rubros.nombre_rubro, XLColor.FromArgb(0, 0, 0), 0, true);

                                int rubroRow = currentRow;
                                currentRow++;

                                var rubrosConceptos = rubro.OrderBy(x => x.C_evaluaciones_conceptos_rubros.nombre_concepto).Select(x => new { NombreConcepto = x.C_evaluaciones_conceptos_rubros.nombre_concepto, idConcepto = x.C_evaluaciones_conceptos_rubros.id_evaluacion_concepto_rubro }).Distinct().ToArray();

                                int rowActual = currentRow;
                                int celdaActual = 1;

                                //CONCEPTO
                                for (int i = 0; i < rubrosConceptos.Count(); i++)
                                {
                                    ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual), rubrosConceptos[i].NombreConcepto.ToUpper(), 0, true);

                                    //PONDERACION DEL CONCEPTO                                    
                                    int idConceptoRubro = rubrosConceptos[i].idConcepto;
                                    ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual), "100 pts / " + Convert.ToInt32(rubro.Where(x => x.C_evaluaciones_conceptos_rubros.id_evaluacion_concepto_rubro == idConceptoRubro).Select(x => x.C_evaluaciones_conceptos_rubros.porcentaje).FirstOrDefault().Value) + "%", 1, true);
                                    currentRow++;
                                }
                                ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual), "TOTAL", 2, true);
                                ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual + 1), "100%", 1, true);

                                celdaActual++;

                                int rowFinal = currentRow;

                                currentRow = rowActual;
                                celdaActual++;

                                //EVALUADOR
                                foreach (var evaluacionEvaluador in listaEvaluadores)
                                {
                                    if (registroEvaluador == false)
                                    {
                                        ExcelEstiloEncabezado(worksheet.Cell(rowHeader, headerCell), evaluacionEvaluador.FirstOrDefault().C_usuarios_corporativo.C_empleados.nombres.ToUpper() + ' ' + evaluacionEvaluador.FirstOrDefault().C_usuarios_corporativo.C_empleados.apellido_paterno.ToUpper(), ExceclObtenerColorAleatorio(), 1, true);
                                        ExcelUnirCeldas(worksheet, rowHeader, headerCell, rowHeader, headerCell + 1, false);
                                        headerCell = headerCell + 2;
                                    }
                                    currentRow = rowActual;
                                    //CONCEPTO
                                    foreach (var evaluacion in evaluacionEvaluador.Where(x => x.C_evaluaciones_conceptos_rubros.id_evaluacion_rubro == rubro.FirstOrDefault().C_evaluaciones_conceptos_rubros.id_evaluacion_rubro).OrderBy(x => x.C_evaluaciones_conceptos_rubros.nombre_concepto))
                                    {
                                        ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual), Convert.ToInt32(evaluacion.calificacion) + " pts" + " / " + Convert.ToInt32(evaluacion.porcentaje_calificacion) + "%", 1, true);
                                        celdaActual++;
                                        ExcelEstiloTextoAjustado(worksheet.Cell(currentRow, celdaActual), evaluacion.comentarios, 1, true);
                                        currentRow++;
                                        celdaActual = celdaActual - 1;
                                    }

                                    ExcelEstiloCelda(worksheet.Cell(currentRow, celdaActual), Convert.ToInt32(evaluacionEvaluador.Where(x => x.C_evaluaciones_conceptos_rubros.id_evaluacion_rubro == rubro.FirstOrDefault().C_evaluaciones_conceptos_rubros.id_evaluacion_rubro).Sum(x => x.porcentaje_calificacion)) + "%", 1, true);
                                    celdaActual = celdaActual + 2;
                                }
                                currentRow = rowFinal;
                                currentRow++;
                                //RUBRO (LINEA NEGRA)
                                ExcelUnirCeldas(worksheet, rubroRow, 1, rubroRow, celdaActual - 1, true);
                                registroEvaluador = true;
                            }
                            var endRow = currentRow;
                            //BORDES DE LA TABLA
                            ExcelBordesRangoCelda(worksheet, 1, 1, endRow - 1, columnCount);
                            currentRow++;
                            currentRow++;
                            int bordeYCalificacion = currentRow;
                            ExcelEstiloEncabezado(worksheet.Cell(currentRow, 1), "EVALUADOR", XLColor.FromArgb(0, 0, 0), 0, true);
                            ExcelUnirCeldas(worksheet, currentRow, 1, currentRow, 2, true);
                            ExcelEstiloEncabezado(worksheet.Cell(currentRow, 3), "FORTALEZA", XLColor.FromArgb(46, 135, 215), 1, true);
                            ExcelUnirCeldas(worksheet, currentRow, 3, currentRow, 4, true);
                            ExcelEstiloEncabezado(worksheet.Cell(currentRow, 5), "AREAS DE OPORTUNIDAD", XLColor.FromArgb(250, 99, 92), 1, true);
                            ExcelUnirCeldas(worksheet, currentRow, 5, currentRow, 6, true);

                            currentRow++;

                            var idEvaluadores = item.Where(x => x.activo == true).Select(x => x.id_usuario_registra).Distinct();
                            int rowCalificaciones = currentRow + (idEvaluadores.Count() * 3) + 2;
                            int contadorEvaluador = 1;

                            //TABLA EVALUADOR
                            //CADA EVALUADOR
                            foreach (var evaluacionEvaluador in idEvaluadores)
                            {
                                //EVALUADOR
                                var evaluador = item.Where(x => x.activo == true && x.id_usuario_registra == evaluacionEvaluador && x.id_evaluacion_d_empleado == item.FirstOrDefault().id_evaluacion_d_empleado).FirstOrDefault();
                                string nombreEvaluadro = evaluador.C_usuarios_corporativo.C_empleados.nombres.ToUpper() + ' ' + evaluador.C_usuarios_corporativo.C_empleados.apellido_paterno.ToUpper();
                                ExcelEstiloCelda(worksheet.Cell(currentRow, 1), nombreEvaluadro, 0, true);
                                ExcelUnirCeldas(worksheet, currentRow, 1, currentRow + 2, 2, true);
                                //FORTALEZA
                                string fortaleza = evaluador.C_evaluaciones_g.C_evaluaciones_d_calificaciones.Where(x => x.id_usuario_califica == evaluacionEvaluador && x.id_evaluacion_d_empleado == item.FirstOrDefault().id_evaluacion_d_empleado).FirstOrDefault().fortalezas;
                                ExcelEstiloTextoAjustado(worksheet.Cell(currentRow, 3), fortaleza, 0, true);
                                ExcelUnirCeldas(worksheet, currentRow, 3, currentRow + 2, 4, true);
                                //AREAS DE OPORTUNIDAD
                                string oportunidad = evaluador.C_evaluaciones_g.C_evaluaciones_d_calificaciones.Where(x => x.id_usuario_califica == evaluacionEvaluador && x.id_evaluacion_d_empleado == item.FirstOrDefault().id_evaluacion_d_empleado).FirstOrDefault().area_oportunidad;
                                ExcelEstiloTextoAjustado(worksheet.Cell(currentRow, 5), oportunidad, 0, true);
                                ExcelUnirCeldas(worksheet, currentRow, 5, currentRow + 2, 6, true);
                                currentRow = currentRow + 3;

                                //CALIFICACIONES
                                int calificacionFinal = 0;
                                try
                                {
                                    calificacionFinal = Convert.ToInt32(evaluador.C_evaluaciones_g.C_evaluaciones_d_calificaciones.Where(x => x.id_usuario_califica == evaluacionEvaluador && x.id_evaluacion_d_empleado == item.FirstOrDefault().id_evaluacion_d_empleado).FirstOrDefault().calificacion_final);
                                }
                                catch (Exception) { }
                                int nomass = currentRow + idEvaluadores.Count() + 2;
                                ExcelEstiloCelda(worksheet.Cell(rowCalificaciones + 1 + contadorEvaluador - 1, 1), nombreEvaluadro, 0, true);
                                ExcelUnirCeldas(worksheet, rowCalificaciones + 1 + contadorEvaluador - 1, 1, rowCalificaciones + 1 + contadorEvaluador - 1, 2, true);
                                ExcelEstiloCelda(worksheet.Cell(rowCalificaciones + 1 + contadorEvaluador - 1, 3), calificacionFinal.ToString() + "%", 1, true);
                                contadorEvaluador++;
                            }
                            currentRow = currentRow + 2;

                            ExcelEstiloEncabezado(worksheet.Cell(currentRow, 1), "CALIFICACIONES", XLColor.FromArgb(0, 0, 0), 0, true);
                            ExcelUnirCeldas(worksheet, currentRow, 1, currentRow, 3, true);

                            ExcelBordesRangoCelda(worksheet, bordeYCalificacion, 1, bordeYCalificacion + (idEvaluadores.Count() * 3), 6);

                            ExcelBordesRangoCelda(worksheet, bordeYCalificacion + (idEvaluadores.Count() * 3) + 3, 1, bordeYCalificacion + (idEvaluadores.Count() * 3) + 3 + idEvaluadores.Count(), 3);
                            worksheet.Columns().AdjustToContents();
                        }
                    }
                    else
                    {
                        // Crear una hoja de trabajo vacía con un mensaje si no hay datos
                        var worksheet = workbook.Worksheets.Add("REPORTE");
                        worksheet.Cell(1, 1).Value = "No se encontraron fichas para los criterios seleccionados.";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Row(1).Height = 30;
                        worksheet.Columns().AdjustToContents();
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EVALUACIONES.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        #endregion


        #region REPORTES DE NOMINA
        public ActionResult ReporteConceptosNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8092)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("Nomina/ReporteConceptosNomina/ReporteConceptosNomina");
        }

        public PartialViewResult ConsultarReporteConceptosNomina(int id_prenomina)
        {
            try
            {
                var nomina = db.C_nomina_beta_g.Find(id_prenomina);
                List<ReporteConceptosNomina> data = new List<ReporteConceptosNomina>();
                ViewBag.lista_conceptos = nomina.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true).Select(x => x.C_nomina_conceptos).Distinct().ToList();

                decimal total_nomina = 0;
                foreach (var concepto in nomina.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true /*&& x.C_nomina_conceptos.fijo_excepcion == true*/).Select(x => x.C_nomina_conceptos).Distinct().ToList())
                {

                    ReporteConceptosNomina reporte = new ReporteConceptosNomina();
                    reporte.id_nomina_g = (int)nomina.id_nomina_g;
                    reporte.nombre_nomina = nomina.nombre_nomina;
                    reporte.id_conceptos = new List<int>();
                    reporte.nombres_conceptos = new List<string>();
                    reporte.valores_conceptos = new List<decimal>();

                    reporte.nombres_conceptos.Add(concepto.nombre_concepto);
                    reporte.id_conceptos.Add(concepto.id_concepto_nomina);
                    decimal valor = 0;
                    foreach (var nominas_empleados in nomina.C_nomina_beta_d_areas_empleados_conceptos.Where(x => x.activo == true && x.id_concepto_nomina == concepto.id_concepto_nomina))
                    {
                        if (concepto.calculo_valor_dia == false || concepto.clave_concepto == "101")  // CALULO X DIA
                        {
                            decimal sueldo_diario = (decimal)nominas_empleados.C_nomina_beta_d_areas_empleados.salario_diario;
                            valor += (decimal)nominas_empleados.valor * sueldo_diario;
                            total_nomina += valor;
                        }
                        else if (concepto.clave_concepto == "203" && concepto.calculo_valor_dia == true) {
                            valor += (decimal)nominas_empleados.valor;
                            total_nomina = total_nomina - valor;
                        }
                        else
                        {
                            valor += (decimal)nominas_empleados.valor;
                            total_nomina += valor;
                        }
                    }
                    reporte.valores_conceptos.Add(valor);
                    data.Add(reporte);
                }
                ViewBag.total_nomina = total_nomina;
                return PartialView("Nomina/ReporteConceptosNomina/_ReporteConceptosNomina", data);
            }
            catch (Exception)
            {
                return PartialView("Nomina/ReporteConceptosNomina/_ReporteConceptosNomina",null);
            }
        }

        #endregion

        #region ESTRUCTURA EXCEL
        private void ExcelEncabezadoTituloImagen(IXLWorksheet worksheet, string titulo, int Y1, int X1, int Y2, int X2, bool margen)
        {
            var headerRange = worksheet.Range(Y1, X1, Y2, X2);
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");

            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var image = worksheet.AddPicture(stream).MoveTo(worksheet.Cell(Y1, 1)).Scale(0.2);
            }

            worksheet.Cell(Y1, 1).Value = titulo;
            worksheet.Cell(Y1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(Y1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell(Y1, 1).Style.Font.Bold = true;
            worksheet.Cell(Y1, 1).Style.Font.FontSize = 24;
            worksheet.Cell(Y1, 1).Style.Font.FontColor = XLColor.Black;
            worksheet.Cell(Y1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 255);
            worksheet.Row(Y1).Height = 16;
            worksheet.Row(Y1 + 1).Height = 16;
            worksheet.Row(Y1 + 2).Height = 16;

            headerRange.Merge();


            if (margen == true)
            {
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void ExcelEstiloEncabezado(IXLCell celda, string texto, XLColor colorFondo, int alineacion, bool margen)
        {
            celda.Value = texto;
            switch (alineacion)
            {
                case 0:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    break;
                case 1:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    break;
                default:
                    break;
            }
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Font.FontSize = 14;
            celda.Style.Fill.BackgroundColor = colorFondo;

            if (margen == true)
            {
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                celda.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void ExcelEstiloCelda(IXLCell celda, string texto, int alineacion, bool margen)
        {
            celda.Value = texto;
            switch (alineacion)
            {
                case 0:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    break;
                case 1:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    break;
                case 2:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    break;
                default:
                    break;
            }
            celda.Style.Font.FontSize = 12;

            if (margen == true)
            {
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                celda.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void ExcelEstiloTextoAjustado(IXLCell celda, string texto, int alineacion, bool margen)
        {
            celda.Value = texto;
            switch (alineacion)
            {
                case 0:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    celda.Style.Alignment.WrapText = true;
                    break;
                case 1:
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    celda.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    celda.Style.Alignment.WrapText = true;
                    break;
                default:
                    break;
            }
            celda.Style.Font.FontSize = 12;
            if (texto.Length > 100)
            {
                celda.WorksheetRow().Height = 50;
            }

            if (margen == true)
            {
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                celda.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void ExcelUnirCeldas(IXLWorksheet worksheet, int Y1, int X1, int Y2, int X2, bool margen)
        {
            var celdaMerge = worksheet.Range(Y1, X1, Y2, X2);
            celdaMerge.Merge();
            if (margen == true)
            {
                var celdaBordes = worksheet.Range(Y1, X1, Y2, X2);
                celdaBordes.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                celdaBordes.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }
        }

        private void ExcelBordesRangoCelda(IXLWorksheet worksheet, int Y1, int X1, int Y2, int X2)
        {
            var celdaBordes = worksheet.Range(Y1, X1, Y2, X2);
            celdaBordes.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            celdaBordes.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        private static Random nAleatorio = new Random();

        private XLColor ExceclObtenerColorAleatorio()
        {
            List<XLColor> coloresPastel = new List<XLColor>{
                XLColor.FromArgb(204, 204, 255), // Azul lavanda
                XLColor.FromArgb(204, 255, 204), // Verde claro
                XLColor.FromArgb(255, 255, 204), // Amarillo pastel
                XLColor.FromArgb(229, 204, 255), // Morado pastel
                XLColor.FromArgb(250, 99, 92),   // Rojo pastel
                XLColor.FromArgb(255, 204, 153), // Naranja pastel
                XLColor.FromArgb(153, 204, 255), // Azul claro
                XLColor.FromArgb(255, 153, 204), // Rosa pastel
                XLColor.FromArgb(204, 255, 255), // Cian pastel
                XLColor.FromArgb(204, 229, 255)  // Azul cielo pastel
            };
            return coloresPastel[nAleatorio.Next(coloresPastel.Count)];
        }

        private void ExcelEstiloPersonalizado(IXLCell celda, string texto, int alineacion, Action<IXLCell, string, int> estiloFuncion)
        {
            XLColor colorFondo = ExceclObtenerColorAleatorio();
            estiloFuncion(celda, texto, alineacion);
            celda.Style.Fill.BackgroundColor = colorFondo;
        }

        #endregion

    }
}