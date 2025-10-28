using Beta_System.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;
using Paragraph = iTextSharp.text.Paragraph;

namespace Beta_System.Controllers
{
    public class COMPRASController : Controller
    {

        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        private CATALOGOSController catalogos = new CATALOGOSController();
        private REQUISICIONESController requisiciones = new REQUISICIONESController();

        public ActionResult AsignarArticuloAlmacen()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(1017)) { return View("/Views/Home/Index.cshtml"); }

                return View("../COMPRAS/CATEGORIZACION/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        #region PRESUPUESTO
        public ActionResult AdministraPresupuestos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(2017)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("PRESUPUESTOS/Index");
        }

        public PartialViewResult ConsultarAniosPresupuestoSelect()
        {
            var anios = db.C_presupuestos_anios.Where(x => x.activo == true).ToList();
            return PartialView("PRESUPUESTOS/_AniosSelect", anios);
        }

        public PartialViewResult ConsultarPresupuestosCargo(int[] id_cargo_contable, int id_anio, int tipo_vista)
        {
            if (id_cargo_contable.Contains(0)) { id_cargo_contable = db.C_cargos_contables_g.Where(x => x.activo == true).Select(x => x.id_cargo_contable_g).ToArray(); }

            var cuentas_contables = db.C_cuentas_contables_g.Where(x => id_cargo_contable.Contains((int)x.id_cargo_contable) && x.activo == true).Select(x => x.id_cuenta_contable).ToArray();

            var presupuestos = from cuentas in db.C_cuentas_contables_g
                               join presupu in db.C_presupuestos_cuentas_meses_anios on cuentas.id_cuenta_contable equals presupu.id_cuenta_contable_g into pres
                               from pco in pres.DefaultIfEmpty()
                               where cuentas_contables.Contains(cuentas.id_cuenta_contable) && cuentas.activo == true // && pco.id_anio == id_anio
                               select new PresupuestosCuentas { c_cuentas = cuentas, c_presupuestos = pco };

            Session["MesesPresupuesto"] = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.id_anio = id_anio;
            ViewBag.tipo_vista = tipo_vista;
            return PartialView("PRESUPUESTOS/_PresupuestosAreasAnio", presupuestos);
        }

        public PartialViewResult ConsultarMesesSelet()
        {
            var meses = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            return PartialView("PRESUPUESTOS/_MesesSelect", meses);
        }
        public bool GuardarPresupuestos(int anio, int[] id_cuentas, decimal[] montos, int[] meses)
        {
            try
            {
                for (int i = 0; i < id_cuentas.Length; i++)
                {
                    int id_mes = meses[i];
                    int id_cuenta = id_cuentas[i];
                    decimal monto = montos[i];

                    var valid = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_anio == anio && x.id_mes == id_mes).FirstOrDefault();
                    if (valid != null)
                    {
                        if (valid.valor_presupuesto != monto)
                        {
                            valid.valor_presupuesto = monto;
                            db.SaveChanges();
                        }
                    }

                    else
                    {
                        C_presupuestos_cuentas_meses_anios presupuesto = new C_presupuestos_cuentas_meses_anios();
                        presupuesto.id_anio = anio;
                        presupuesto.id_mes = id_mes;
                        presupuesto.id_cuenta_contable_g = id_cuenta;
                        presupuesto.valor_presupuesto = monto;
                        db.C_presupuestos_cuentas_meses_anios.Add(presupuesto);
                    }
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
        #endregion

        #region PRESUPUESTO ALMACENES
        public ActionResult AdministrarPresupuestosAlmacen()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7038)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("PRESUPUESTOS_ALMACEN/Index");
        }

        public PartialViewResult ConsultarPresupuestosCargoAlmacen(int id_almacen, int id_cargo_contable, int id_anio)
        {
            var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.id_cargo_contable == id_cargo_contable && x.activo == true).Select(x => x.id_cuenta_contable).ToArray();
            var presupuestos = from cuentas in db.C_cuentas_contables_g
                               join presupu in db.C_presupuestos_almacenes_cuentas_meses_anios on cuentas.id_cuenta_contable equals presupu.id_cuenta_contable_g into pres
                               from pco in pres.DefaultIfEmpty()
                               where cuentas_contables.Contains(cuentas.id_cuenta_contable) &&
                                     (pco == null || pco.id_almacen == id_almacen)
                               select new PresupuestosCuentasAlmacen { c_cuentas = cuentas, c_presupuestos = pco };

            Session["MesesPresupuestoAlmacen"] = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.id_anio = id_anio;

            return PartialView("PRESUPUESTOS_ALMACEN/_PresupuestosAreasAnioAlmacen", presupuestos);
        }

        public PartialViewResult ReporteConsumosAlmacen(int id_almacen, int id_cargo_contable, int id_anio)
        {
            List<PresupuestosOrdenCompra> data = new List<PresupuestosOrdenCompra>();
            List<(decimal, decimal)> consumos = new List<(decimal, decimal)>();

            var meses_presupuesto = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            var cuenta_contable = db.C_cuentas_contables_g.Where(x => x.id_cargo_contable == id_cargo_contable && x.activo == true).ToList();
            decimal presupuesto_asignado = 0;
            foreach (var item in cuenta_contable)
            {
                PresupuestosOrdenCompra reportes = new PresupuestosOrdenCompra();
                int id_cuenta = item.id_cuenta_contable;
                reportes.cuenta_string = item.cuenta;
                reportes.cuenta = item.id_cuenta_contable;
                reportes.descripcion = item.nombre_cuenta;
                decimal totales = 0;
                foreach (var meses in meses_presupuesto)
                {
                    var presupuesto_almacen_cuenta = db.C_presupuestos_almacenes_cuentas_meses_anios.Where(x => x.id_anio == id_anio && x.id_mes == meses.id_mes_presupuesto && x.id_cuenta_contable_g == id_cuenta && x.id_almacen == id_almacen).FirstOrDefault();

                    if (presupuesto_almacen_cuenta != null)
                    {
                        presupuesto_asignado = (decimal)presupuesto_almacen_cuenta.valor_presupuesto;

                        var años = db.C_presupuestos_anios.Find(id_anio);
                        int mes_presupuesto = (int)presupuesto_almacen_cuenta.id_mes;

                        DateTime primerDiaDelMes = new DateTime(Convert.ToInt32(años.anio), (int)presupuesto_almacen_cuenta.id_mes, 1);
                        int primerDia = primerDiaDelMes.Day;
                        int ultimoDia = DateTime.DaysInMonth(Convert.ToInt32(años.anio), (int)presupuesto_almacen_cuenta.id_mes);


                        DateTime fecha_1 = new DateTime(Convert.ToInt32(años.anio), mes_presupuesto, primerDia);
                        DateTime fecha_2 = new DateTime(Convert.ToInt32(años.anio), mes_presupuesto, ultimoDia);


                        var consumo_mensual = (from reporte_g in db.C_almacen_solicitudes_mercancia_g
                                               join reporte_d in db.C_almacen_solicitudes_mercancia_d
                                               on reporte_g.id_solicitud_mercancia_g equals reporte_d.id_solicitud_mercancia_g
                                               join movimiento_g in db.C_inventario_almacen_mov_g
                                               on reporte_g.id_solicitud_mercancia_g equals movimiento_g.id_transaccion_solicitud_mercancia_g
                                               join movimientos_d in db.C_inventario_almacen_mov_d
                                               on movimiento_g.id_inventario_almacen_mov_g equals movimientos_d.id_inventario_almacen_mov_g
                                               where reporte_g.activo == true && reporte_d.activo == true && movimientos_d.fecha_registro >= fecha_1 && movimientos_d.fecha_registro <= fecha_2 && reporte_d.id_cuenta_contable_g == id_cuenta && reporte_g.id_almacen_g == id_almacen
                                               group new { reporte_g, reporte_d, movimientos_d } by new { reporte_g.id_solicitud_mercancia_g } into g
                                               select new ReporteMensualCuentaAlmacen
                                               {
                                                   id_almacen_g = g.Key.id_solicitud_mercancia_g,
                                                   fecha = (DateTime)(g.Select(x => x.movimientos_d.fecha_registro).FirstOrDefault()),
                                                   importe = (decimal)(g.Sum(x => x.movimientos_d.cantidad) * g.FirstOrDefault().reporte_d.costo),
                                                   concepto = g.FirstOrDefault().reporte_g.cargo_desc
                                               }).Distinct();

                        foreach (var itemsa in consumo_mensual)
                        {
                            totales += itemsa.importe;
                        }
                        if (consumo_mensual != null)
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

                        var años = db.C_presupuestos_anios.Find(id_anio);
                        int mes_presupuesto = meses.id_mes_presupuesto;

                        DateTime primerDiaDelMes = new DateTime(Convert.ToInt32(años.anio), mes_presupuesto, 1);
                        int primerDia = primerDiaDelMes.Day;
                        int ultimoDia = DateTime.DaysInMonth(Convert.ToInt32(años.anio), mes_presupuesto);


                        DateTime fecha_1 = new DateTime(Convert.ToInt32(años.anio), mes_presupuesto, primerDia);
                        DateTime fecha_2 = new DateTime(Convert.ToInt32(años.anio), mes_presupuesto, ultimoDia);


                        var consumo_mensual = (from reporte_g in db.C_almacen_solicitudes_mercancia_g
                                               join reporte_d in db.C_almacen_solicitudes_mercancia_d
                                               on reporte_g.id_solicitud_mercancia_g equals reporte_d.id_solicitud_mercancia_g
                                               join movimiento_g in db.C_inventario_almacen_mov_g
                                               on reporte_g.id_solicitud_mercancia_g equals movimiento_g.id_transaccion_solicitud_mercancia_g
                                               join movimientos_d in db.C_inventario_almacen_mov_d
                                               on movimiento_g.id_inventario_almacen_mov_g equals movimientos_d.id_inventario_almacen_mov_g
                                               where reporte_g.activo == true && reporte_d.activo == true && movimientos_d.fecha_registro >= fecha_1 && movimientos_d.fecha_registro <= fecha_2 && reporte_d.id_cuenta_contable_g == id_cuenta && reporte_g.id_almacen_g == id_almacen
                                               group new { reporte_g, reporte_d, movimientos_d } by new { reporte_g.id_solicitud_mercancia_g } into g
                                               select new ReporteMensualCuentaAlmacen
                                               {
                                                   id_almacen_g = g.Key.id_solicitud_mercancia_g,
                                                   fecha = (DateTime)(g.Select(x => x.movimientos_d.fecha_registro).FirstOrDefault()),
                                                   importe = (decimal)(g.Sum(x => x.movimientos_d.cantidad) * g.FirstOrDefault().reporte_d.costo),
                                                   concepto = g.FirstOrDefault().reporte_g.cargo_desc
                                               }).Distinct();



                        foreach (var itemsa in consumo_mensual)
                        {
                            totales += itemsa.importe;
                        }
                        if (consumo_mensual != null)
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

            Session["ReporteMesesPresupuestoAlmacen"] = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.id_anio_reporte = id_anio;


            return PartialView("PRESUPUESTOS_ALMACEN/_ReportePresupuestoAlmacen", data);
        }

        public bool GuardarPresupuestosAlmacen(int anio, int id_almacen, int[] id_cuentas, decimal[] montos, int[] meses)
        {
            try
            {
                for (int i = 0; i < id_cuentas.Length; i++)
                {
                    int id_mes = meses[i];
                    int id_cuenta = id_cuentas[i];
                    decimal monto = montos[i];

                    var valid = db.C_presupuestos_almacenes_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_anio == anio && x.id_mes == id_mes && x.id_almacen == id_almacen).FirstOrDefault();
                    if (valid != null)
                    {
                        if (valid.valor_presupuesto != monto)
                        {
                            valid.valor_presupuesto = monto;
                            db.SaveChanges();
                        }
                    }

                    else
                    {
                        C_presupuestos_almacenes_cuentas_meses_anios presupuesto = new C_presupuestos_almacenes_cuentas_meses_anios();
                        presupuesto.id_anio = anio;
                        presupuesto.id_mes = id_mes;
                        presupuesto.id_cuenta_contable_g = id_cuenta;
                        presupuesto.valor_presupuesto = monto;
                        presupuesto.id_almacen = id_almacen;
                        db.C_presupuestos_almacenes_cuentas_meses_anios.Add(presupuesto);
                    }
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


        public PartialViewResult ReporteConsumosMensualAlmacen(int id_almacen, int cuenta, int mes, int id_ano, decimal total)
        {
            var años = db.C_presupuestos_anios.Find(id_ano);
            DateTime primerDiaDelMes = new DateTime(Convert.ToInt32(años.anio), mes, 1);
            int primerDia = primerDiaDelMes.Day;
            int ultimoDia = DateTime.DaysInMonth(Convert.ToInt32(años.anio), mes);


            DateTime fecha_1 = new DateTime(Convert.ToInt32(años.anio), mes, primerDia, 0, 0, 0);
            DateTime fecha_2 = new DateTime(Convert.ToInt32(años.anio), mes, ultimoDia, 23, 59, 59);


            var reporte_mensual = db.C_almacen_solicitudes_mercancia_d.Where(x => x.activo == true &&
            x.C_almacen_solicitudes_mercancia_g.activo == true && x.id_cuenta_contable_g == cuenta && x.cantidad_entregada > 0);


            List<ReporteMensualCuentaAlmacen> reportes = new List<ReporteMensualCuentaAlmacen>();
            var reporte_completo = (from reporte_g in db.C_almacen_solicitudes_mercancia_g
                                    join reporte_d in db.C_almacen_solicitudes_mercancia_d
                                    on reporte_g.id_solicitud_mercancia_g equals reporte_d.id_solicitud_mercancia_g
                                    join movimiento_g in db.C_inventario_almacen_mov_g
                                    on reporte_g.id_solicitud_mercancia_g equals movimiento_g.id_transaccion_solicitud_mercancia_g
                                    join movimientos_d in db.C_inventario_almacen_mov_d
                                    on movimiento_g.id_inventario_almacen_mov_g equals movimientos_d.id_inventario_almacen_mov_g
                                    where reporte_g.activo == true && reporte_d.activo == true && movimientos_d.fecha_registro >= fecha_1 && movimientos_d.fecha_registro <= fecha_2 && reporte_d.id_cuenta_contable_g == cuenta && reporte_g.id_almacen_g == id_almacen
                                    group new { reporte_g, reporte_d, movimientos_d } by new { reporte_g.id_solicitud_mercancia_g } into g
                                    select new ReporteMensualCuentaAlmacen
                                    {
                                        id_almacen_g = g.Key.id_solicitud_mercancia_g,
                                        fecha = (DateTime)(g.Select(x => x.movimientos_d.fecha_registro).FirstOrDefault()),
                                        importe = (decimal)(g.Sum(x => x.movimientos_d.cantidad) * g.FirstOrDefault().reporte_d.costo),
                                        concepto = g.FirstOrDefault().reporte_g.cargo_desc
                                    }).Distinct();
            var meses = db.C_presupuestos_meses.Find(mes);
            var cuentas = db.C_cuentas_contables_g.Find(cuenta);
            ViewBag.PTTOMes = meses.mes;
            ViewBag.CuentaAlmacen = cuentas.cuenta;
            ViewBag.DescAlmacen = cuentas.nombre_cuenta;
            ViewBag.TotalPTTO = total;
            return PartialView("PRESUPUESTOS_ALMACEN/_ReportePTTOMensual", reporte_completo);
        }



        public string ImportarPresupuestos()
        {
            List<string> Cuentas = new List<string>();
            List<string> Ene = new List<string>();
            List<string> Feb = new List<string>();
            List<string> Mar = new List<string>();
            List<string> Abr = new List<string>();
            List<string> May = new List<string>();
            List<string> Jun = new List<string>();
            List<string> Jul = new List<string>();
            List<string> Ago = new List<string>();
            List<string> Sep = new List<string>();
            List<string> Oct = new List<string>();
            List<string> Nov = new List<string>();
            List<string> Dic = new List<string>();
            try
            {
                int count_success = 0;

                string filePath = "C:\\Users\\aleja\\Contacts\\PPTO_2025.xlsx";
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
                            int EneIndex = -1;
                            int FebIndex = -1;
                            int MarIndex = -1;
                            int AbrIndex = -1;
                            int MayIndex = -1;
                            int JunIndex = -1;
                            int JulIndex = -1;
                            int AgoIndex = -1;
                            int SepIndex = -1;
                            int OctIndex = -1;
                            int NovIndex = -1;
                            int DicIndex = -1;

                            for (int col = 1; col <= columnCount; col++)
                            {
                                if (worksheet.Cells[1, col].Value?.ToString() == "CUENTA")
                                {
                                    ClaveColumnIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "ENERO")
                                {
                                    EneIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "FEBRERO")
                                {
                                    FebIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "MARZO")
                                {
                                    MarIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "ABRIL")
                                {
                                    AbrIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "MAYO")
                                {
                                    MayIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "JUNIO")
                                {
                                    JunIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "JULIO")
                                {
                                    JulIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "AGOSTO")
                                {
                                    AgoIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "SEPTIEMBRE")
                                {
                                    SepIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "OCTUBRE")
                                {
                                    OctIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "NOVIEMBRE")
                                {
                                    NovIndex = col;
                                }
                                if (worksheet.Cells[1, col].Value?.ToString() == "DICIEMBRE")
                                {
                                    DicIndex = col;
                                    break;
                                }
                            }


                            if (ClaveColumnIndex != -1)
                            {
                                // Llena los datos de la columna "CLAVE"
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, ClaveColumnIndex].Value?.ToString().Trim();
                                    Cuentas.Add(cellValue);
                                }

                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, EneIndex].Value?.ToString();
                                    Ene.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, FebIndex].Value?.ToString();
                                    Feb.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, MarIndex].Value?.ToString();
                                    Mar.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, AbrIndex].Value?.ToString();
                                    Abr.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, MayIndex].Value?.ToString();
                                    May.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, JunIndex].Value?.ToString();
                                    Jun.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, JulIndex].Value?.ToString();
                                    Jul.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, AgoIndex].Value?.ToString();
                                    Ago.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, SepIndex].Value?.ToString();
                                    Sep.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, OctIndex].Value?.ToString();
                                    Oct.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, NovIndex].Value?.ToString();
                                    Nov.Add(cellValue);
                                }
                                for (int row = 2; row <= rowCount; row++) // Start from row 2 (data rows)
                                {
                                    string cellValue = worksheet.Cells[row, DicIndex].Value?.ToString();
                                    Dic.Add(cellValue);
                                }


                                int count_Ene = Ene.Count();
                                int count_Feb = Feb.Count();
                                int count_Mar = Mar.Count();
                                int count_Abr = Abr.Count();
                                int count_May = May.Count();
                                int count_Jun = Jun.Count();
                                int count_Jul = Jul.Count();
                                int count_Ago = Ago.Count();
                                int count_Sep = Sep.Count();
                                int count_Nov = Nov.Count();
                                int count_Dic = Dic.Count();

                                int count_total = Cuentas.Distinct().Count();
                                int count_cuentas = db.C_cuentas_contables_g.Where(x => x.activo == true && Cuentas.Contains(x.cuenta)).Count();

                                //var cuentas_beta = db.C_cuentas_contables_g.Where(x => x.activo == true && Cuentas.Contains(x.cuenta)).Select(x => x.cuenta).ToList();
                                //List<string> cuentas_not_found = Cuentas.Except(cuentas_beta).ToList();
                                //return Newtonsoft.Json.JsonConvert.SerializeObject(cuentas_not_found);

                                for (int i = 0; i < Cuentas.Count; i++)
                                {
                                    string cuenta = Cuentas[i].Trim();
                                    var valid_cuenta = db.C_cuentas_contables_g.Where(x => x.cuenta == cuenta).FirstOrDefault();
                                    if (valid_cuenta != null)
                                    {
                                        int id_cuenta = valid_cuenta.id_cuenta_contable;

                                        var ene = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 1 && x.id_anio == 3).FirstOrDefault();
                                        if (ene == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto.id_anio = 3;
                                            new_ppto.id_mes = 1;
                                            new_ppto.id_cuenta_contable_g = id_cuenta;
                                            new_ppto.valor_presupuesto = Convert.ToDecimal(Ene[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            ene.valor_presupuesto = Convert.ToDecimal(Ene[i]);
                                            db.SaveChanges();
                                        }

                                        var feb = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 2 && x.id_anio == 3).FirstOrDefault();
                                        if (feb == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_feb = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_feb.id_anio = 3;
                                            new_ppto_feb.id_mes = 2;
                                            new_ppto_feb.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_feb.valor_presupuesto = Convert.ToDecimal(Feb[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_feb);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            feb.valor_presupuesto = Convert.ToDecimal(Feb[i]);
                                            db.SaveChanges();
                                        }

                                        var mar = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 3 && x.id_anio == 3).FirstOrDefault();
                                        if (mar == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_mar = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_mar.id_anio = 3;
                                            new_ppto_mar.id_mes = 3;
                                            new_ppto_mar.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_mar.valor_presupuesto = Convert.ToDecimal(Mar[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_mar);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            mar.valor_presupuesto = Convert.ToDecimal(Mar[i]);
                                            db.SaveChanges();
                                        }


                                        var abr = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 4 && x.id_anio == 3).FirstOrDefault();
                                        if (abr == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_abr = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_abr.id_anio = 3;
                                            new_ppto_abr.id_mes = 4;
                                            new_ppto_abr.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_abr.valor_presupuesto = Convert.ToDecimal(Abr[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_abr);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            abr.valor_presupuesto = Convert.ToDecimal(Abr[i]);
                                            db.SaveChanges();
                                        }

                                        var may = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 5 && x.id_anio == 3).FirstOrDefault();
                                        if (may == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_may = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_may.id_anio = 3;
                                            new_ppto_may.id_mes = 5;
                                            new_ppto_may.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_may.valor_presupuesto = Convert.ToDecimal(May[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_may);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            may.valor_presupuesto = Convert.ToDecimal(May[i]);
                                            db.SaveChanges();
                                        }

                                        var jun = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 6 && x.id_anio == 3).FirstOrDefault();
                                        if (jun == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_jun = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_jun.id_anio = 3;
                                            new_ppto_jun.id_mes = 6;
                                            new_ppto_jun.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_jun.valor_presupuesto = Convert.ToDecimal(Jun[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_jun);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            jun.valor_presupuesto = Convert.ToDecimal(Jun[i]);
                                            db.SaveChanges();
                                        }

                                        var jul = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 7 && x.id_anio == 3).FirstOrDefault();
                                        if (jul == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_jul = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_jul.id_anio = 3;
                                            new_ppto_jul.id_mes = 7;
                                            new_ppto_jul.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_jul.valor_presupuesto = Convert.ToDecimal(Jul[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_jul);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            jul.valor_presupuesto = Convert.ToDecimal(Jul[i]);
                                            db.SaveChanges();
                                        }

                                        var ago = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 8 && x.id_anio == 3).FirstOrDefault();
                                        if (ago == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_ago = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_ago.id_anio = 3;
                                            new_ppto_ago.id_mes = 8;
                                            new_ppto_ago.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_ago.valor_presupuesto = Convert.ToDecimal(Ago[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_ago);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            ago.valor_presupuesto = Convert.ToDecimal(Ago[i]);
                                            db.SaveChanges();
                                        }

                                        var sep = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 9 && x.id_anio == 3).FirstOrDefault();
                                        if (sep == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_sep = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_sep.id_anio = 3;
                                            new_ppto_sep.id_mes = 9;
                                            new_ppto_sep.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_sep.valor_presupuesto = Convert.ToDecimal(Sep[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_sep);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            sep.valor_presupuesto = Convert.ToDecimal(Sep[i]);
                                            db.SaveChanges();
                                        }

                                        var oct = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 10 && x.id_anio == 3).FirstOrDefault();
                                        if (oct == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_oct = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_oct.id_anio = 3;
                                            new_ppto_oct.id_mes = 10;
                                            new_ppto_oct.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_oct.valor_presupuesto = Convert.ToDecimal(Oct[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_oct);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            oct.valor_presupuesto = Convert.ToDecimal(Oct[i]);
                                            db.SaveChanges();
                                        }

                                        var nov = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 11 && x.id_anio == 3).FirstOrDefault();
                                        if (nov == null)
                                        {
                                            C_presupuestos_cuentas_meses_anios new_ppto_nov = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_nov.id_anio = 3;
                                            new_ppto_nov.id_mes = 11;
                                            new_ppto_nov.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_nov.valor_presupuesto = Convert.ToDecimal(Nov[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_nov);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            nov.valor_presupuesto = Convert.ToDecimal(Nov[i]);
                                            db.SaveChanges();
                                        }

                                        var dic = db.C_presupuestos_cuentas_meses_anios.Where(x => x.id_cuenta_contable_g == id_cuenta && x.id_mes == 12 && x.id_anio == 3).FirstOrDefault();
                                        if (dic == null)
                                        {

                                            C_presupuestos_cuentas_meses_anios new_ppto_dic = new C_presupuestos_cuentas_meses_anios();
                                            new_ppto_dic.id_anio = 3;
                                            new_ppto_dic.id_mes = 12;
                                            new_ppto_dic.id_cuenta_contable_g = id_cuenta;
                                            new_ppto_dic.valor_presupuesto = Convert.ToDecimal(Dic[i]);
                                            db.C_presupuestos_cuentas_meses_anios.Add(new_ppto_dic);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            dic.valor_presupuesto = Convert.ToDecimal(Dic[i]);
                                            db.SaveChanges();
                                        }
                                    }


                                }


                                return count_cuentas.ToString();
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
        #endregion


        public string ConvertirNumeroALetras(decimal numero, string clave_fiscal)
        {
            double valorIngresado = Convert.ToDouble(numero);
            string parteEnteraEnLetras = ConvertirNumerosALetras(valorIngresado);

            // Construir el resultado final
            string resultado = parteEnteraEnLetras + " " + clave_fiscal;

            return resultado;
        }

        #region NUMEROS A LETRAS 
        public static string ConvertirNumerosALetras(double numero)
        {
            // Variables
            string res, dec = "";
            Int64 entero;
            int decimales;
            bool negativo = false;

            // Validar longitud (Double 16 dígitos).
            if (numero > 9999999999999999)
            {
                return "Número fuera de rango.";
            }
            // Validar negativos
            if (numero < 0)
            {
                numero = numero * -1;
                negativo = true;
            }

            // Obtener la parte entera y decimal.
            entero = Convert.ToInt64(Math.Truncate(numero));
            decimales = Convert.ToInt32(Math.Round((numero - entero) * 100, 2));
            // Convertir parte decimal.
            if (decimales > 0)
            {
                dec = " " + decimales.ToString() + " /100";
            }
            // Convertir parte entera y unirla con la parte decimal.
            res = CovertirValor(Convert.ToDouble(entero)) + dec;

            // Acentos.
            if (res.IndexOf("DIECISEIS") != -1)
            {
                res = res.Replace("DIECISEIS", "DIECISÉIS");
            }
            // Acentos.
            if (res.IndexOf("VEINTIDOS") != -1)
            {
                res = res.Replace("VEINTIDOS", "VEINTIDÓS");
            }
            // Acentos.
            if (res.IndexOf("VEINTITRES") != -1)
            {
                res = res.Replace("VEINTITRES", "VEINTITRÉS");
            }
            // Acentos.
            if (res.IndexOf("VEINTISEIS") != -1)
            {
                res = res.Replace("VEINTISEIS", "VEINTISÉIS");
            }

            // Excepción VEINTIÚN.
            var contador = res.Split(' ').Count(t => t == "VEINTIUNO");
            for (int i = 0; i < contador; i++)
            {
                var index = res.IndexOf("VEINTIUNO");
                if (index != res.Count() - 9)
                {
                    res = Reemplazar(res, "VEINTIUNO", "VEINTIÚN");
                }
            }

            if (negativo)
                res = "MENOS " + res;

            return res;
        }

        private static string CovertirValor(double value)
        {
            string resultado = "";
            value = Math.Truncate(value);
            if (value == 0) resultado = "CERO";
            else if (value == 1) resultado = "UNO";
            else if (value == 2) resultado = "DOS";
            else if (value == 3) resultado = "TRES";
            else if (value == 4)
                resultado = "CUATRO";
            else if (value == 5) resultado = "CINCO";
            else if (value == 6) resultado = "SEIS";
            else if (value == 7) resultado = "SIETE";
            else if (value == 8) resultado = "OCHO";
            else if (value == 9) resultado = "NUEVE";
            else if (value == 10) resultado = "DIEZ";
            else if (value == 11) resultado = "ONCE";
            else if (value == 12) resultado = "DOCE";
            else if (value == 13) resultado = "TRECE";
            else if (value == 14) resultado = "CATORCE";
            else if (value == 15) resultado = "QUINCE";
            else if (value < 20) resultado = "DIECI" + CovertirValor(value - 10);
            else if (value == 20) resultado = "VEINTE";
            else if (value < 30) resultado = "VEINTI" + CovertirValor(value - 20);
            else if (value == 30) resultado = "TREINTA";
            else if (value == 40) resultado = "CUARENTA";
            else if (value == 50) resultado = "CINCUENTA";
            else if (value == 60) resultado = "SESENTA";
            else if (value == 70) resultado = "SETENTA";
            else if (value == 80) resultado = "OCHENTA";
            else if (value == 90) resultado = "NOVENTA";
            else if (value < 100) resultado = CovertirValor(Math.Truncate(value / 10) * 10) + " Y " + CovertirValor(value % 10);
            else if (value == 100) resultado = "CIEN";
            else if (value < 200) resultado = "CIENTO " + CovertirValor(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) resultado = CovertirValor(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) resultado = "QUINIENTOS";
            else if (value == 700) resultado = "SETECIENTOS";
            else if (value == 900) resultado = "NOVECIENTOS";
            else if (value < 1000) resultado = CovertirValor(Math.Truncate(value / 100) * 100) + " " + CovertirValor(value % 100);
            else if (value == 1000) resultado = "MIL";
            else if (value < 2000) resultado = "MIL " + CovertirValor(value % 1000);
            else if (value < 1000000)
            {
                resultado = CovertirValor(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0) resultado = resultado + " " + CovertirValor(value % 1000);
            }

            else if (value == 1000000) resultado = "UN MILLÓN";
            else if (value < 2000000) resultado = "UN MILLÓN " + CovertirValor(value % 1000000);
            else if (value < 1000000000000)
            {
                resultado = CovertirValor(Math.Truncate(value / 1000000)) + " MILLONES";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) resultado = resultado + " " + CovertirValor(value - Math.Truncate(value / 1000000) * 1000000);
            }

            else if (value == 1000000000000) resultado = "UN BILLÓN";
            else if (value < 2000000000000) resultado = "UN BILLÓN " + CovertirValor(value - Math.Truncate(value / 1000000000000) * 1000000000000);

            else
            {
                resultado = CovertirValor(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) resultado = resultado + " " + CovertirValor(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }
            return resultado;

        }

        public static string Reemplazar(string cadena, string encuentra, string reemplazo)
        {
            int index = cadena.IndexOf(encuentra);
            string resultado = cadena.Remove(index, encuentra.Length).Insert(index, reemplazo);
            return resultado;
        }
        #endregion

        #region PRECARGA
        public ActionResult IndexPrecarga()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6024)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../COMPRAS/PRECARGA/Index");
        }

        public PartialViewResult ConsultarOrdenEspecificaPrecarga(int idorden)
        {
            ViewBag.comentario = "";

            //REGISTRO
            var precarga_g = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == idorden && x.recibido == false && x.activo == true /*&& x.fecha_registro > oneDayAgo*/).ToList();

            if (precarga_g.Count() == 0)
            {
                var ordenesCompra = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == idorden && x.entregado == false && x.activo == true && x.C_compras_ordenes_g.id_tipo_orden == 1);
                ViewBag.modo_recepcion = 1;
                return PartialView("../COMPRAS/PRECARGA/_OrdenCompraPrecargaTable", ordenesCompra);
            }
            else
            {
                ViewBag.comentario = precarga_g.FirstOrDefault().comentario;
                var ordenesCompra = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == idorden && x.entregado == false && x.activo == true && x.cantidad_precarga >= 0 && x.C_compras_ordenes_g.id_tipo_orden == 1);
                ViewBag.modo_recepcion = 2;
                return PartialView("../COMPRAS/PRECARGA/_OrdenCompraPrecargaTable", ordenesCompra);
            }
        }
        /*
            1 = INGRESO CORRECTO
            2 = YA SE INGRESO EL MAXIMO
            3 = HA OCURRIDO UN ERROR
        */
        //MODO 1 - REGISTRO
        //MODO 2 - EDICION
        public int RegistrarCapturaMovimientoInventarioPrecarga(int idordeng, int[] idordend, decimal[] cantidades, decimal[] compra, decimal[] entrega, int modo, int id_empleado, string comentario)
        {
            int regreso = 1;
            var id_precarga_g = 0;
            DateTime oneDayAgo = DateTime.Now.AddDays(-1);
            try
            {
                var precarga_original = from precargag in db.C_compras_ordenes_precarga_g
                                        join precargad in db.C_compras_ordenes_precarga_d
                                        on precargag.id_precarga_g equals precargad.id_precarga_g
                                        where precargag.id_compras_orden_g == idordeng /*&& precargag.fecha_registro > oneDayAgo*/ && precargag.fecha_recepcion == null && precargag.recibido == false
                                        && precargag.activo == true
                                        select precargag;

                #region REGISTRO ORDEN-PRECARGA-G (SI NO EXISTE)
                if (precarga_original.Count() == 0)
                {
                    C_compras_ordenes_precarga_g precarga_g = new C_compras_ordenes_precarga_g();
                    precarga_g.fecha_registro = DateTime.Now;
                    //precarga_g.fecha_edicion = DateTime.Now;
                    //precarga_g.fecha_recepcion = DateTime.Now;
                    precarga_g.id_usuario_registra = Convert.ToInt32(Session["LoggedId"]);
                    //precarga_g.id_usuario_edita = Convert.ToInt32(Session["LoggedId"]);
                    precarga_g.id_compras_orden_g = idordeng;
                    precarga_g.recibido = false;
                    precarga_g.activo = true;
                    precarga_g.comentario = comentario;
                    precarga_g.id_empleado_repartidor = id_empleado;
                    db.C_compras_ordenes_precarga_g.Add(precarga_g);
                    db.SaveChanges();

                    id_precarga_g = precarga_g.id_precarga_g;
                }
                else
                {
                    id_precarga_g = precarga_original.FirstOrDefault().id_precarga_g;
                }
                #endregion

                #region REGISTRAR ORDEN-PRECARGA-D (SI NO EXISTE)
                if (modo == 1)
                {
                    //REGISTRO O EDICION PARA CADA ARTICULO (ORDEN D)
                    for (int w = 0; w < idordend.Length; w++)
                    {
                        var cantidad_g = cantidades[w];
                        var precargas = idordend[w];
                        var precarga = db.C_compras_ordenes_precarga_d.Where(x => x.id_compras_orden_d == precargas && x.id_precarga_g == id_precarga_g).ToList();
                        //REGISTRO
                        if (precarga.Count() == 0)
                        {
                            if (cantidades[w] > -1)
                            {
                                C_compras_ordenes_precarga_d precarga_d = new C_compras_ordenes_precarga_d();
                                precarga_d.cantidad_compra = compra[w];
                                precarga_d.cantidad_entrega = entrega[w];
                                precarga_d.cantidad_precarga = cantidades[w];
                                precarga_d.id_precarga_g = id_precarga_g;
                                precarga_d.activo = true;
                                precarga_d.id_compras_orden_d = idordend[w];
                                db.C_compras_ordenes_precarga_d.Add(precarga_d);
                                db.SaveChanges();
                            }
                        }
                        //EDICION
                        else
                        {
                            precarga.FirstOrDefault().cantidad_entrega = entrega[w];
                            precarga.FirstOrDefault().cantidad_compra = compra[w];
                            precarga.FirstOrDefault().cantidad_precarga = cantidad_g;
                            precarga.FirstOrDefault().id_compras_orden_d = idordend[w];
                            db.SaveChanges();
                        }

                        var orden_d = db.C_compras_ordenes_d.Find(idordend[w]);
                        orden_d.cantidad_precarga = cantidades[w];
                        var cantidades_total = orden_d.cantidad_precarga + cantidades[w];
                        //if (cantidades_total == orden_d.cantidad_compra)
                        //{
                        //    regreso = 2;
                        //}
                        db.SaveChanges();
                    }
                }
                #endregion

                #region MODIFICAR ORDEN PRECARGA-D (DEBE EXISTIR)
                else
                {
                    for (int w = 0; w < idordend.Length; w++)
                    {
                        if (cantidades[w] > 0)
                        {
                            var id_orden = idordend[w];
                            var orden_d_precarga = db.C_compras_ordenes_precarga_d.Where(x => x.id_compras_orden_d == id_orden && x.activo == true).OrderByDescending(x => x.id_precarga_d).FirstOrDefault();
                            orden_d_precarga.cantidad_precarga = cantidades[w];
                            db.SaveChanges();

                            var orden_d = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_d == id_orden && x.entregado == false).FirstOrDefault();
                            orden_d.cantidad_precarga = cantidades[w];
                            db.SaveChanges();

                            var orden_g = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == idordeng && x.activo == true && x.fecha_recepcion == null).OrderByDescending(x => x.fecha_registro).FirstOrDefault();
                            orden_g.fecha_edicion = DateTime.Now;
                            orden_g.id_usuario_edita = Convert.ToInt32(Session["LoggedId"]);
                            orden_g.comentario = comentario;
                            orden_g.id_empleado_repartidor = id_empleado;
                            db.SaveChanges();
                        }
                    }
                }
                #endregion
                
                if (regreso != 1)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception e)
            {
                return 3;
            }
        }

        public int EliminarPrecarga(int id_precarga_g)
        {
            try
            {
                db.C_compras_ordenes_precarga_g.Find(id_precarga_g).activo = false;
                db.C_compras_ordenes_precarga_d.Where(x => x.id_precarga_g == id_precarga_g).ToList().ForEach(x => x.activo = false);
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public PartialViewResult ConsultarPrecargasRealizadas(int modo)
        {
            var data = new List<C_compras_ordenes_precarga_g>();
            if (modo == 1) //PENDIENTES
            {
                data = db.C_compras_ordenes_precarga_g.Where(x => x.recibido == false && x.activo == true).OrderByDescending(x => x.fecha_registro).ToList();
            }
            else if(modo == 2)  //REALIZADAS
            {
                data = db.C_compras_ordenes_precarga_g.Where(x => x.recibido == true && x.activo == true).OrderByDescending(x => x.fecha_registro).ToList();
            }
            return PartialView("../COMPRAS/PRECARGA/_PrecargasRealizadasTable", data);
        }

        public PartialViewResult GenerarFormatoPrecargas(int[] id_precargas)
        {
            var precargas = db.C_compras_ordenes_precarga_g.Where(x => id_precargas.Contains((int)x.id_precarga_g)).OrderBy(x => x.C_compras_ordenes_g.C_compras_ordenes_ubicaciones_entrega.nombre_ubicacion);
            return PartialView("../COMPRAS/PRECARGA/_FormatoPrecargasPDF", precargas);
        }

        public ActionResult ConsultarPrecargas(int[] id_precargas)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Crear documento en memoria
                Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var precargas = db.C_compras_ordenes_precarga_g.Where(x => id_precargas.Contains((int)x.id_precarga_g) && x.C_compras_ordenes_precarga_d.Where(z => z.activo == true).Count() > 0).GroupBy(x => x.C_compras_ordenes_g.C_compras_requi_g.id_usuario_registro);
                foreach (var item in precargas)
                {
                    // ---------------- HEADER ----------------
                    PdfPTable headerTable = new PdfPTable(3); // 3 columnas: imagen - titulo - fecha
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 2f, 12f, 2f }); // proporción de ancho

                    // Imagen izquierda
                    string rutaImagen = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
                    Image logo = Image.GetInstance(rutaImagen);
                    logo.ScaleToFit(120f, 120f);
                    PdfPCell imgCell = new PdfPCell(logo);
                    imgCell.Border = Rectangle.NO_BORDER;
                    imgCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    headerTable.AddCell(imgCell);

                    // Título central
                    PdfPCell tituloCell = new PdfPCell(new Phrase("Envío de ordenes de compra", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                    tituloCell.Border = Rectangle.NO_BORDER;
                    tituloCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    tituloCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tituloCell.PaddingLeft = 20f;
                    headerTable.AddCell(tituloCell);

                    // Fecha derecha
                    PdfPCell fechaCell = new PdfPCell(new Phrase("Fecha de impresión: " + DateTime.Now.ToString("dd/MM/yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 10)));
                    fechaCell.Border = Rectangle.NO_BORDER;
                    fechaCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    fechaCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    headerTable.AddCell(fechaCell);
                    doc.Add(headerTable);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable headerTableDestinatario = new PdfPTable(1);
                    string nombre_destinatario = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                    PdfPCell destinatarioCell = new PdfPCell(new Phrase("ENTREGAR A: " + nombre_destinatario.ToUpper() + "", FontFactory.GetFont(FontFactory.HELVETICA, 14)));
                    destinatarioCell.Border = Rectangle.NO_BORDER;
                    destinatarioCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    destinatarioCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    headerTableDestinatario.AddCell(destinatarioCell);
                    doc.Add(headerTableDestinatario);

                    doc.Add(new Paragraph("\n"));

                    //Detalle de cada hoja
                    PdfPTable tabla1 = new PdfPTable(7);
                    tabla1.WidthPercentage = 100;

                    float[] widths = { 7f, 7f, 7f, 7f, 7f, 7f, 18f };
                    tabla1.SetWidths(widths);

                    BaseColor colorFondo = new BaseColor(0x2e, 0x87, 0xd7); // #2e87d7
                    string[] headers = { "No. Orden", "Requisición", "Artículos", "Entregados", "Pendientes", "A recibir", "Obs" };
                    foreach (var header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE)));
                        cell.BackgroundColor = colorFondo;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 2f;
                        tabla1.AddCell(cell);
                    }

                    
                    foreach (var precarga in item)
                    {
                        PdfPCell cell_table = new PdfPCell(new Phrase(precarga.id_compras_orden_g.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK)));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_g.C_compras_requi_g.C_centros_g.siglas.ToString() + " " + precarga.C_compras_ordenes_g.C_compras_requi_g.id_requisicion_articulo_g.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        //cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_g.C_compras_ordenes_ubicaciones_entrega.nombre_ubicacion.ToString());
                        //cell_table.Phrase.Font = FontFactory.GetFont(FontFactory.HELVETICA, 2);
                        //tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_g.C_compras_ordenes_d.Where(x => x.activo == true).Count().ToString("N2"), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_g.C_compras_ordenes_d.Where(x => x.activo == true && x.entregado == true).Count().ToString("N2"), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_g.C_compras_ordenes_d.Where(x => x.activo == true && x.entregado == false).Count().ToString("N2"), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.C_compras_ordenes_precarga_d.Where(x => x.activo == true).Sum(x => x.cantidad_precarga).Value.ToString("N2"), FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);

                        cell_table.Phrase = new Phrase(precarga.comentario, FontFactory.GetFont(FontFactory.HELVETICA, 6, BaseColor.BLACK));
                        cell_table.HorizontalAlignment = Element.ALIGN_LEFT;
                        cell_table.Padding = 4f;
                        tabla1.AddCell(cell_table);
                    }
                    doc.Add(tabla1);

                    PdfContentByte cb = writer.DirectContent;
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("Firma de recibido:"), 250, 10, 0);
                    doc.Add(new Paragraph("\n"));
                    ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("___________________"), 230, 30, 0);
                    

                    doc.NewPage();
                }

                

                doc.Close();

                // Devolver PDF al navegador
                byte[] file = ms.ToArray();
                return File(file, "application/pdf", "VistaPrevia.pdf");
                // 🔹 si quieres inline sin descargar:
                // return File(file, "application/pdf");
            }
        }



        public PartialViewResult ConsultarPecargasRecepcionesHistorial(string fecha_inicio, string fecha_fin)
        {
            DateTime fecha_i, fecha_f;
            try
            {
                fecha_i = DateTime.Parse(fecha_inicio);
                fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            }
            catch (Exception)
            {
                return PartialView("../COMPRAS/PRECARGA/_RecepcionesHistorial", null);
            }

            var precargas = db.C_compras_ordenes_precarga_d.Where(x => x.C_compras_ordenes_precarga_g.fecha_registro >= fecha_i && x.C_compras_ordenes_precarga_g.fecha_registro <= fecha_f 
                                                && x.activo == true && x.C_compras_ordenes_precarga_g.activo == true);
            //var recepciones = db.C_compras_ordenes_recepciones_g.Where(x => x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.activo == true);
            return PartialView("../COMPRAS/PRECARGA/_RecepcionesHistorial", precargas);
        }

        #endregion


        #region RECEPECION ORDENES DE COMPRA

        public ActionResult RecepcionOrdenesCompra()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7035)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception) { RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }
            return View("RECEPCION_OC/Index");
        }

        public PartialViewResult ConsultarOrdenesPendientesRecepcion(int[] id_centros)
        {
            try
            {
                var ubicaciones = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.activo == true && x.id_almacen_g != null).Select(x => x.id_compras_ubicacion_entrega).ToArray();

                int id_usuario = (int)Session["LoggedId"];
                if (id_centros.Contains(0)) { id_centros = catalogos.ConsultarCentrosUsuarioID(id_usuario); }

                var ordenes = db.C_compras_ordenes_g.Where(x => x.entregado == false && x.alta_proveedor == false && id_centros.Contains((int)x.id_centro) && x.activo == true
                                && x.entregado == false && !ubicaciones.Contains(x.id_compras_ubicacion_entrega) && x.id_status_orden != 3)  //TODAS LAS UBICACIONES QUE NO TENGAL ALMACEN
                                .ToList();

                return PartialView("RECEPCION_OC/_OrdenesPendientesRecepcion", ordenes);
            }
            catch (Exception)
            {
                return PartialView("RECEPCION_OC/_OrdenesPendientesRecepcion", null);
            }
        }

        public PartialViewResult ConsultarDetalleOrdenPendienteRecepcion(int id_orden_g)
        {
            try
            {
                //var detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true && x.entregado == false).ToList();

                //return PartialView("RECEPCION_OC/_DetalleOrdenPendienteRecepcion", detalle);

                var ordenRecepcionar = db.C_compras_ordenes_d.Where(x => x.entregado == false && x.activo == true && x.cantidad_entregada < x.cantidad_compra &&
                                                                         x.C_compras_ordenes_g.id_compras_orden_g == id_orden_g).ToList();

                var precarga = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == id_orden_g && x.recibido == false && x.activo == true).ToList().Count();
                if (precarga > 0)
                {
                    //PRECARGA
                    ViewBag.modo = 1;
                }
                else
                {
                    //DIRECTO
                    ViewBag.modo = 2;
                }
                return PartialView("RECEPCION_OC/_DetalleOrdenPendienteRecepcion", ordenRecepcionar);

            }
            catch (Exception)
            {
                return PartialView("RECEPCION_OC/_DetalleOrdenPendienteRecepcion", null);
            }
        }

        public int RecepcionarOrdenCompra(int id_orden_g, int[] id_orden_d, decimal[] cantidad, string folio_factura_remision)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;

                C_compras_ordenes_recepciones_g rep_g = new C_compras_ordenes_recepciones_g();
                rep_g.id_compras_orden_g = id_orden_g;
                rep_g.id_usuario_registra = id_usuario;
                rep_g.fecha_registro = hoy;
                rep_g.activo = true;
                rep_g.factura_remision_folio = folio_factura_remision;
                if (folio_factura_remision == null) {
                    rep_g.factura_remision_folio = "N/A";
                    rep_g.factura_remision = false;
                }
                else
                {
                    rep_g.factura_remision_folio = folio_factura_remision;
                    rep_g.factura_remision = true;
                }
                db.C_compras_ordenes_recepciones_g.Add(rep_g);
                db.SaveChanges();
                int id_recepcion_g = rep_g.id_compras_orden_recepcion_g;

                //VALIDO QUE EXISTA UNA PRECARGA
                var valid_precarga = db.C_compras_ordenes_precarga_g.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true && x.recibido == false).OrderByDescending(x => x.fecha_registro).FirstOrDefault();

                for (int i = 0; i < id_orden_d.Length; i++)
                {
                    int id_detalle = id_orden_d[i];
                    var detalle = db.C_compras_ordenes_d.Find(id_detalle);
                    if (detalle.entregado == true)
                    {

                    }
                    else
                    {
                        detalle.cantidad_entregada = detalle.cantidad_entregada + cantidad[i];
                        if (detalle.cantidad_entregada >= detalle.cantidad_compra) { detalle.entregado = true; }
                        db.SaveChanges();

                        if (valid_precarga != null)
                        {
                            var precarga = valid_precarga.C_compras_ordenes_precarga_d.Where(x => x.id_compras_orden_d == id_detalle && x.activo == true && x.C_compras_ordenes_precarga_g.recibido == false).FirstOrDefault();
                            if (precarga != null)
                            {
                                if (precarga.cantidad_entrega < detalle.cantidad_entregada)
                                {
                                    precarga.cantidad_entrega = precarga.cantidad_entrega + cantidad[i];
                                }
                            }
                        }
                    }

                    C_compras_ordenes_recepciones_d rep_d = new C_compras_ordenes_recepciones_d();
                    rep_d.id_compras_orden_recepcion_g = id_recepcion_g;
                    rep_d.id_compras_orden_d = id_detalle;
                    rep_d.cantidad_recepcion = cantidad[i];
                    rep_d.activo = true;
                    db.C_compras_ordenes_recepciones_d.Add(rep_d);
                    db.SaveChanges();

                    if (valid_precarga != null)
                    {
                        var valid_d = valid_precarga.C_compras_ordenes_precarga_d.Where(x => x.id_compras_orden_d == id_detalle && x.activo == true).FirstOrDefault();
                        if (valid_d != null)
                        {
                            valid_d.id_compras_orden_recepcion_d = rep_d.id_compras_orden_recepcion_d;
                            db.SaveChanges();
                        }
                    }

                }

                //ACTUALIZO LA PRECARGA COMO FINALIZADA (ENTREGADA) AUNQUE SEAN PARCIALES
                if (valid_precarga != null)
                {
                    valid_precarga.recibido = true;
                    valid_precarga.fecha_recepcion = hoy;
                    valid_precarga.id_compras_orden_recepcion_g = id_recepcion_g;
                    db.SaveChanges();
                }

                var articulos = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true).Select(x => x.entregado).ToArray();
                if (!articulos.Contains(false))
                {
                    var orden = db.C_compras_ordenes_g.Find(id_orden_g);
                    orden.entregado = true;
                    orden.id_usuario_recepciona = id_usuario;
                    orden.fecha_recepcion = hoy;
                    db.SaveChanges();
                    requisiciones.CambiarStatusRequisicion((int)orden.id_requisicion_articulo_g, 8);
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        #endregion

        #region TRANSITO DE ARTICULOS
        public ActionResult TransitoArticulos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8058)) { return View("/Views/Home/Index.cshtml"); }

                return View("../COMPRAS/TRANSITO/Articulo/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        public PartialViewResult ConsultaArticuloTransitoTable(string clave, int tipo)
        {
            //BUSQUEDA POR CLAVE DEL ARTICULO
            if (clave != "")
            {
                try
                {
                    List<TransitoArticulo> TransitoArticuloLista = new List<TransitoArticulo>();

                    //PENDIENTE POR PRIMERA FIRMA
                    var requis_pendientes = db.C_compras_requi_g.Where(x => x.aut_1_status != true).Select(x => x.id_requisicion_articulo_g).ToArray();
                    var requis_pendientes_d = db.C_compras_requi_d.Where(x => x.C_articulos_catalogo.clave == clave && requis_pendientes.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.almacenable == true).ToList();
                    foreach (var item in requis_pendientes_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.id_requisicion_articulo_g;
                        transito.cotizacion = (decimal)item.cantidad;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = 0;

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);

                    }
                    //REQUISICION AUTORIZADA
                    var requis_autorizada = db.C_compras_requi_g.Where(x => x.aut_1_status == true).Select(x => x.id_requisicion_articulo_g).ToArray();
                    var requis_autorizada_d = db.C_compras_requi_d.Where(x => x.C_articulos_catalogo.clave == clave && requis_autorizada.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.almacenable == true).Select(x => (int)x.id_requisicion_articulo_g).Distinct().ToArray();


                    //ORDEN DE COMPRA
                    var orden_g = db.C_compras_ordenes_g.Where(x => x.activo == true && requis_autorizada_d.Contains((int)x.id_requisicion_articulo_g)).Select(x => x.id_compras_orden_g).ToArray();
                    var orden_g_requi = db.C_compras_ordenes_g.Where(x => x.activo == true && requis_autorizada_d.Contains((int)x.id_requisicion_articulo_g) && x.id_status_orden != 3).Select(x => (int)x.id_requisicion_articulo_g).ToArray();
                    var orden_d = db.C_compras_ordenes_d.Where(x => x.activo == true && orden_g.Contains((int)x.id_compras_orden_g) && x.entregado == false && x.cantidad_entregada < x.cantidad_compra && x.C_articulos_catalogo.clave == clave).ToList();
                    //ORDEN DE COMPRA ARTICULOS
                    foreach (var item in orden_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.C_compras_ordenes_g.id_requisicion_articulo_g;
                        transito.cotizacion = 0;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = Convert.ToDecimal(item.cantidad_compra - item.cantidad_entregada);

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_ordenes_g.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_compras_ordenes_g.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);
                    }


                    var requis_sin_orden = requis_autorizada_d.Except(orden_g_requi).ToArray();

                    //CONFIRMACION - PARCIALIDAD
                    var cotiza_g = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.activo == true && requis_sin_orden.Contains((int)x.id_requisicion_articulo_g)).Select(x => x.id_cotizacion_confirmada_g).ToArray();
                    var cotiza_g_requi = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.activo == true && requis_sin_orden.Contains((int)x.id_requisicion_articulo_g)).Select(x => (int)x.id_requisicion_articulo_g).ToArray();
                    var cotiza_d = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true && cotiza_g.Contains((int)x.id_cotizacion_confirmada_g) && x.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.clave == clave
                     && x.C_compras_cotizaciones_requisiciones.orden_generada == false).ToList();

                    //PARCIALIDAD ARTICULOS (PENDIENTE DE AUTORIZAR)
                    foreach (var item in cotiza_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.clave;
                        transito.articulo = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.C_compras_cotizaciones_confirmadas_g.id_requisicion_articulo_g;
                        transito.cotizacion = 0;
                        transito.confirmacion = 0;
                        transito.parcialidad = (decimal)item.C_compras_cotizaciones_requisiciones.cantidad_surtir;
                        transito.precepcion = 0;

                        transito.familia = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.fecha_registro;
                        transito.cargo = "";
                        transito.cuenta = "";
                        transito.centro = "";
                        TransitoArticuloLista.Add(transito);
                    }
                    var requis_sin_cotizacion = requis_sin_orden.Except(cotiza_g_requi).ToArray();

                    //COTIZACION
                    var requis_d = db.C_compras_requi_d.Where(x => requis_sin_cotizacion.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.clave == clave).ToList();
                    foreach (var item in requis_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.id_requisicion_articulo_g;
                        transito.cotizacion = (decimal)item.cantidad;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = 0;

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);
                    }

                    ViewBag.transitomodo = 1;
                    return PartialView("../COMPRAS/TRANSITO/Articulo/_TransitoArticulo", TransitoArticuloLista);
                }
                catch (Exception)
                {
                    return PartialView("../COMPRAS/TRANSITO/Articulo/_TransitoArticulo", null);
                }
            }
            //BUSQUEDA POR TIPO DE ARTICULO
            else
            {
                try
                {
                    List<TransitoArticulo> TransitoArticuloLista = new List<TransitoArticulo>();
                    //PENDIENTE POR PRIMERA FIRMA
                    var requis_pendientes = db.C_compras_requi_g.Where(x => x.aut_1_status != true).Select(x => x.id_requisicion_articulo_g).ToArray();
                    var requis_pendientes_d = db.C_compras_requi_d.Where(x => x.C_articulos_catalogo.id_articulo_tipo == tipo && requis_pendientes.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.almacenable == true).ToList();
                    foreach (var item in requis_pendientes_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.id_requisicion_articulo_g;
                        transito.cotizacion = (decimal)item.cantidad;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = 0;

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);

                    }
                    //REQUISICION AUTORIZADA
                    var requis_autorizada = db.C_compras_requi_g.Where(x => x.aut_1_status == true).Select(x => x.id_requisicion_articulo_g).ToArray();
                    var requis_autorizada_d = db.C_compras_requi_d.Where(x => x.C_articulos_catalogo.id_articulo_tipo == tipo && requis_autorizada.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.almacenable == true).Select(x => (int)x.id_requisicion_articulo_g).Distinct().ToArray();


                    //ORDEN DE COMPRA
                    var orden_g = db.C_compras_ordenes_g.Where(x => x.activo == true && requis_autorizada_d.Contains((int)x.id_requisicion_articulo_g)).Select(x => x.id_compras_orden_g).ToArray();
                    var orden_g_requi = db.C_compras_ordenes_g.Where(x => x.activo == true && requis_autorizada_d.Contains((int)x.id_requisicion_articulo_g) && x.id_status_orden != 3).Select(x => (int)x.id_requisicion_articulo_g).ToArray();
                    var orden_d = db.C_compras_ordenes_d.Where(x => x.activo == true && orden_g.Contains((int)x.id_compras_orden_g) && x.entregado == false && x.cantidad_entregada < x.cantidad_compra && x.C_articulos_catalogo.id_articulo_tipo == tipo && x.C_articulos_catalogo.almacenable == true).ToList();
                    //ORDEN DE COMPRA ARTICULOS
                    foreach (var item in orden_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.C_compras_ordenes_g.id_requisicion_articulo_g;
                        transito.cotizacion = 0;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = Convert.ToDecimal(item.cantidad_compra - item.cantidad_entregada);

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_ordenes_g.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_compras_ordenes_g.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);
                    }


                    var requis_sin_orden = requis_autorizada_d.Except(orden_g_requi).ToArray();

                    //CONFIRMACION - PARCIALIDAD
                    var cotiza_g = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.activo == true && requis_sin_orden.Contains((int)x.id_requisicion_articulo_g)).Select(x => x.id_cotizacion_confirmada_g).ToArray();
                    var cotiza_g_requi = db.C_compras_cotizaciones_confirmadas_g.Where(x => x.activo == true && requis_sin_orden.Contains((int)x.id_requisicion_articulo_g)).Select(x => (int)x.id_requisicion_articulo_g).ToArray();
                    var cotiza_d = db.C_compras_cotizaciones_confirmadas_d.Where(x => x.activo == true && cotiza_g.Contains((int)x.id_cotizacion_confirmada_g) && x.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.id_articulo_tipo == tipo
                     && x.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.almacenable == true && x.C_compras_cotizaciones_requisiciones.orden_generada == false).ToList();

                    //PARCIALIDAD ARTICULOS (PENDIENTE DE AUTORIZAR)
                    foreach (var item in cotiza_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.clave;
                        transito.articulo = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.C_compras_cotizaciones_confirmadas_g.id_requisicion_articulo_g;
                        transito.cotizacion = 0;
                        transito.confirmacion = 0;
                        transito.parcialidad = (decimal)item.C_compras_cotizaciones_requisiciones.cantidad_surtir;
                        transito.precepcion = 0;

                        transito.familia = item.C_compras_cotizaciones_requisiciones.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.fecha_registro;
                        transito.cargo = "";
                        transito.cuenta = "";
                        transito.centro = "";
                        TransitoArticuloLista.Add(transito);
                    }
                    var requis_sin_cotizacion = requis_sin_orden.Except(cotiza_g_requi).ToArray();

                    //COTIZACION
                    var requis_d = db.C_compras_requi_d.Where(x => requis_sin_cotizacion.Contains((int)x.id_requisicion_articulo_g) && x.C_articulos_catalogo.id_articulo_tipo == tipo && x.C_articulos_catalogo.almacenable == true).ToList();
                    foreach (var item in requis_d)
                    {
                        TransitoArticulo transito = new TransitoArticulo();
                        transito.clave = item.C_articulos_catalogo.clave;
                        transito.articulo = item.C_articulos_catalogo.nombre_articulo;
                        transito.requisicion = (int)item.id_requisicion_articulo_g;
                        transito.cotizacion = (decimal)item.cantidad;
                        transito.confirmacion = 0;
                        transito.parcialidad = 0;
                        transito.precepcion = 0;

                        transito.familia = item.C_articulos_catalogo.C_articulos_tipos.nombre_tipo;
                        transito.registrado = item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres + " " + item.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno;
                        transito.fecha = (DateTime)item.C_compras_requi_g.fecha_registro;
                        transito.cargo = item.C_cargos_contables_g.nombre_cargo;
                        transito.cuenta = item.C_cuentas_contables_g.cuenta + " " + item.C_cuentas_contables_g.nombre_cuenta;
                        transito.centro = item.C_centros_g.nombre_centro;
                        TransitoArticuloLista.Add(transito);
                    }

                    ViewBag.transitomodo = 2;
                    return PartialView("../COMPRAS/TRANSITO/Articulo/_TransitoArticulo", TransitoArticuloLista);
                }
                catch (Exception)
                {
                    return PartialView("../COMPRAS/TRANSITO/Articulo/_TransitoArticulo", null);
                }
            }
        }
        #endregion

        #region  MODULO HISTORIAL DE COTIZACIONES
        public ActionResult HistorialCotizacion()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8062)) { return View("/Views/Home/Index.cshtml"); }

                return View("../REPORTES/HistorialCotizacion/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        public PartialViewResult ConsultaHistorialCotizacionTable(string clave, string fecha_inicio, string fecha_fin)
        {
            DateTime fecha_i = DateTime.Today.AddMonths(-3);
            DateTime fecha_f = DateTime.Today.AddDays(1);

            if (fecha_inicio != "" && fecha_fin != "" && fecha_inicio != null && fecha_fin != null)
            {
                fecha_i = DateTime.Parse(fecha_inicio);
                fecha_f = DateTime.Parse(fecha_fin);
            }

            ViewBag.periodo = fecha_f - fecha_i;

            var orden_d = db.C_compras_ordenes_d.Where(x => x.activo == true && x.C_articulos_catalogo.clave == clave && x.C_compras_ordenes_g.fecha_registro >= fecha_i && x.C_compras_ordenes_g.fecha_registro <= fecha_f).OrderByDescending(x => x.C_compras_ordenes_g.fecha_registro).ToList();
            return PartialView("../REPORTES/HistorialCotizacion/_HistorialCotizacionesTable", orden_d);
        }

        public PartialViewResult ConsultarHistorialCotizacionesGeneralTable(string clave)
        {
            DateTime hoy = DateTime.Today.AddHours(23).AddMinutes(59);
            var orden_d = db.C_compras_cotizaciones_articulos.Where(x => x.activo == true && x.C_articulos_catalogo.clave == clave && x.fecha_vigencia >= hoy).ToList();

            if (orden_d.Count == 0)
            {
                var articulo = db.C_articulos_catalogo.Where(x => x.clave == clave).FirstOrDefault();
                if (articulo != null)
                {
                    ViewBag.histarticulo = articulo.nombre_articulo;
                    ViewBag.histunidadmedida = articulo.C_unidades_medidas.unidad_medida;
                    ViewBag.histidunidadmedida = articulo.id_unidad_medida;
                    ViewBag.histidarticulo = articulo.id_articulo;
                }
            }


            return PartialView("../REPORTES/HistorialCotizacion/_HistorialCotizacionesGeneralTable", orden_d);
        }

        public int RegistrarCotizacionProveedor(int id_articulo, int id_compras_proveedor, decimal precio_unitario, decimal porcentaje_descuento, decimal precio_final, int id_unidad_medida, string marca,
                                                string presentacion, string fecha_vigencia, int dias_entrega, int id_tipo_moneda)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                DateTime fecha_vig = DateTime.Parse(fecha_vigencia);
                int id_usuario = (int)Session["LoggedId"];
                C_compras_cotizaciones_articulos cotizacion = new C_compras_cotizaciones_articulos();
                cotizacion.id_articulo = id_articulo;
                cotizacion.id_compras_proveedor = id_compras_proveedor;
                cotizacion.precio_unitario = precio_unitario;
                cotizacion.porcentaje_descuento = porcentaje_descuento;
                cotizacion.precio_final = precio_final;
                cotizacion.id_unidad_medida = id_unidad_medida;
                cotizacion.marca = marca;
                cotizacion.presentacion = presentacion;
                cotizacion.fecha_cotizacion = hoy;
                cotizacion.fecha_vigencia = fecha_vig;
                cotizacion.id_usuario_cotiza = id_usuario;
                cotizacion.id_tipo_moneda = id_tipo_moneda;
                cotizacion.activo = true;
                cotizacion.fecha_actualizacion = hoy;
                cotizacion.dias_entrega = dias_entrega;
                db.C_compras_cotizaciones_articulos.Add(cotizacion);
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }

        }

        public int ActualizarVigenciaCotizacion(int id_compra_articulo_cotizacion, string fecha_vigencia)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                DateTime fecha_vig = DateTime.Parse(fecha_vigencia);
                var cotizacion = db.C_compras_cotizaciones_articulos.Find(id_compra_articulo_cotizacion);

                if (fecha_vig <= DateTime.Today) { return -2; }

                cotizacion.fecha_vigencia = fecha_vig;
                cotizacion.fecha_actualizacion = hoy;
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

        #region INVERSIONES
        public ActionResult AdministrarInversiones()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8082)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("../COMPRAS/INVERSIONES/Index");
        }


        public PartialViewResult ConsultaInversiones(int anio)
        {
            var inversiones = db.C_compras_inversiones_programadas.Where(x => x.fecha_aplicacion.Value.Year == anio && x.activo == true).ToList();
            return PartialView("../COMPRAS/INVERSIONES/_InversionesTable", inversiones);
        }

        public bool ConfirmacionInversion(C_compras_inversiones_programadas c_inversion)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                c_inversion.fecha_registro = DateTime.Now;
                c_inversion.id_usuario_registra = id_usuario;
                c_inversion.aplicado = false;
                c_inversion.activo = true;
                db.C_compras_inversiones_programadas.Add(c_inversion);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string InfoInversion(int id_inversion)
        {
            var inversion = from invers in db.C_compras_inversiones_programadas
                            where invers.id_compra_inversion_programada == id_inversion
                            select new
                            {
                                invers.id_compra_inversion_programada,
                                invers.folio_inversion_requi,
                                invers.presupuesto,
                                invers.fecha_aplicacion,
                                invers.concepto
                            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(inversion);
        }

        public bool EdicionInversion(C_compras_inversiones_programadas c_inversion)
        {
            try
            {
                var inversion = db.C_compras_inversiones_programadas.Find(c_inversion.id_compra_inversion_programada);
                inversion.folio_inversion_requi = c_inversion.folio_inversion_requi;
                inversion.presupuesto = c_inversion.presupuesto;
                inversion.fecha_aplicacion = c_inversion.fecha_aplicacion;
                inversion.concepto = c_inversion.concepto;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PartialViewResult ConsultaInversionesReporte(int anio)
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
            return PartialView("../COMPRAS/INVERSIONES/_ReporteInversionTable", Inversion);
        }

        public void ReporteInversionTablePDF(int anio)
        {
            #region INVERSIONES
            //DateTime anio_nuevo = anio.AddYears(1).AddSeconds(-1);

            //var ordenes_compra = db.C_compras_ordenes_d.Where(x => x.C_compras_ordenes_g.fecha_registro >= anio && x.C_compras_ordenes_g.fecha_registro <= anio_nuevo && x.C_compras_ordenes_g.entregado == true && x.C_compras_ordenes_g.id_tipo_orden == 3).ToList();
            //var inversiones = db.C_compras_inversiones_programadas.Where(x => x.fecha_aplicacion >= anio && x.fecha_aplicacion <= anio_nuevo).ToList();

            //List<ReporteInversiones> Inversion = new List<ReporteInversiones>();
            //foreach (var item in ordenes_compra.GroupBy(x => x.C_compras_ordenes_g.id_requisicion_articulo_g))
            //{
            //    ReporteInversiones reporte = new ReporteInversiones();
            //    try
            //    {
            //        reporte.Justificacion = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_compras_requi_g_solicita_autorizacion.FirstOrDefault().justificacion;
            //    }
            //    catch (Exception)
            //    {
            //        reporte.Justificacion = "";
            //    }
            //    reporte.Fecha = item.FirstOrDefault().C_compras_ordenes_g.fecha_registro.Value.ToShortDateString();
            //    try
            //    {
            //        reporte.Concepto = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_compras_inversiones_programadas.FirstOrDefault().concepto;
            //    }
            //    catch (Exception)
            //    {
            //        reporte.Concepto = "";
            //    }
            //    try
            //    {
            //        reporte.Folio = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_compras_inversiones_programadas.FirstOrDefault().folio_inversion_requi;
            //    }
            //    catch (Exception)
            //    {
            //        reporte.Folio = "";
            //    }
            //    reporte.Requisicion = (int)item.FirstOrDefault().C_compras_ordenes_g.id_requisicion_articulo_g;
            //    reporte.Centro = item.FirstOrDefault().C_compras_ordenes_g.C_centros_g.siglas;
            //    reporte.Usuario_registra = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_usuarios_corporativo5.usuario;
            //    reporte.Usuario_autoriza = item.FirstOrDefault().C_compras_ordenes_g.C_usuarios_corporativo.usuario;
            //    try
            //    {
            //        reporte.Monto_inversion = (decimal)item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_compras_inversiones_programadas.FirstOrDefault().presupuesto;
            //        reporte.Monto_orden = (decimal)item.Select(x => x.cantidad_entregada * x.precio_unitario).Sum();
            //        reporte.Diferencia = reporte.Monto_inversion - reporte.Monto_orden;
            //    }
            //    catch
            //    {
            //        reporte.Monto_inversion = 0;
            //        reporte.Monto_orden = (decimal)item.Select(x => x.cantidad_entregada * x.precio_unitario).Sum();
            //        reporte.Diferencia = reporte.Monto_inversion - reporte.Monto_orden;
            //    }
            //    Inversion.Add(reporte);
            //}

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
                    };

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
            #endregion

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate(), 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuentes y Tipografia
            var FuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
            var FuenteSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var FuenteGeneral = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var FuenteNegritas = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var FuenteNegritasColor = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
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
            PdfPCell titleCell = new PdfPCell(new Phrase("REPORTE COMPARATIVO DE INVERSIONES DEL AÑO", FuenteTitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado.AddCell(titleCell);

            PdfPTable encabezado_fechas = new PdfPTable(1) { WidthPercentage = 100 };
            // fecha a la derecha
            PdfPCell fecha1 = new PdfPCell(new Phrase("Fecha de impresion", FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            PdfPCell fecha2 = new PdfPCell(new Phrase("" + DateTime.Now, FuenteSubtitulo))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            encabezado_fechas.AddCell(fecha1);
            encabezado_fechas.AddCell(fecha2);
            encabezado_fechas.DefaultCell.Border = Rectangle.NO_BORDER;
            encabezado.AddCell(encabezado_fechas);
            #endregion




            #region CUERPO
            PdfPTable cuerpo_inversion = new PdfPTable(12) { WidthPercentage = 100 };
            cuerpo_inversion.SetWidths(new float[] { 0.08f, 0.12f, 0.2f, 0.05f, 0.06f, 0.051f, 0.051f, 0.07f, 0.07f, 0.08f, 0.08f, 0.08f, });
            string[] conceptos = { "Fecha Compra", "Justificacion", "Inversion", "Folio Inversion", "Orden de compra", "Requisicion", "Centro", "Usuario registra", "Usuario autoriza", "Monto inversion", "Monto orden", "Diferencia" };
            foreach (var header in conceptos)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, FuenteNegritasColor))
                {
                    //Border = Rectangle.NO_BORDER
                    BackgroundColor = new BaseColor(46, 135, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cell);
            }
            foreach (var item in Inversion)
            {
                PdfPCell cellcuerpo = new PdfPCell(new Phrase("" + item.Fecha, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Justificacion, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                };
                cuerpo_inversion.AddCell(cellcuerpo);

                cellcuerpo = new PdfPCell(new Phrase("" + item.Concepto, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Folio, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);

                cellcuerpo = new PdfPCell(new Phrase("" + item.OrdenCompra, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);

                cellcuerpo = new PdfPCell(new Phrase("" + item.Requisicion, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Centro, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Usuario_registra, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Usuario_autoriza, FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Monto_inversion.ToString("N3"), FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    //HorizontalAlignment = Element.ALIGN_RIGHT
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Monto_orden.ToString("N3"), FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    //HorizontalAlignment = Element.ALIGN_RIGHT
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
                cellcuerpo = new PdfPCell(new Phrase("" + item.Diferencia.ToString("N3"), FuenteGeneral))
                {
                    //Border = Rectangle.NO_BORDER
                    //HorizontalAlignment = Element.ALIGN_RIGHT
                    VerticalAlignment = Element.ALIGN_CENTER
                };
                cuerpo_inversion.AddCell(cellcuerpo);
            }

            #endregion


            document.Add(new iTextSharp.text.Paragraph("\n"));
            document.Add(encabezado);
            document.Add(new iTextSharp.text.Paragraph("\n"));
            document.Add(cuerpo_inversion);


            document.Close();
            byte[] pdfBytes = ms.ToArray();
            ms.Close();
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", $"attachment;filename=Reporte comparativo de inversiones.pdf");
            Response.Buffer = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.BinaryWrite(pdfBytes);
            Response.End();
        }



        #endregion

        
    }
}