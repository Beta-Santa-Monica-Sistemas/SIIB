using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Core.Metadata.Edm;
using Newtonsoft.Json;


namespace Beta_System.Controllers
{
    public class DASHBOARDController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        ENVIOLECHEController env_leche = new ENVIOLECHEController();

        public PartialViewResult ConsultarRequisicionesGeneradasUsuario()
        {
            List<List<string>> data = new List<List<string>>();
            DateTime hoy = DateTime.Today;
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_GeneralRequisicionesGeneradasUsuario", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_usuario", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                    cmd.Parameters["@id_usuario"].Value = id_usuario.ToString();
                    cmd.Parameters["@fecha_inicio"].Value = hoy.AddDays(-30).ToString("yyyy-MM-dd");
                    cmd.Parameters["@fecha_fin"].Value = hoy.ToString("yyyy-MM-dd");

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<string> valores = new List<string>();
                        valores.Add(string.Format(DateTime.Parse(reader["fecha_registro"].ToString()).ToShortDateString(), "dd-MM-yyyy") );
                        valores.Add(reader["tipo_requisicion"].ToString());
                        valores.Add(reader["siglas"].ToString());
                        valores.Add(reader["id_requisicion_articulo_g"].ToString());
                        valores.Add(reader["nombre_status"].ToString());
                        valores.Add(reader["color"].ToString());
                        valores.Add(reader["concepto"].ToString());
                        valores.Add(reader["color_tipo_requi"].ToString());
                        data.Add(valores);
                    }
                    reader.Close();
                    conn.Close();
                }
                return PartialView("_GeneralRequisicionesGeneradas", data);
            }
            catch (Exception ex)
            {
                return PartialView("_GeneralRequisicionesGeneradas", null);
            }
        }

        #region DASHBOARD COMPRAS
        public string ConsultarCotizacionesComprasAnual(string id_rol)
        {
            List<GraficasReporteLine> data = new List<GraficasReporteLine>();
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_ComprasCotizacionesComprasAnual", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_rol", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@anio", SqlDbType.Int));
                    cmd.Parameters["@id_rol"].Value = id_rol;
                    cmd.Parameters["@anio"].Value = Convert.ToInt32(DateTime.Now.Year);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<object> valores = new List<object>();
                        valores.Add(Convert.ToDecimal(reader["Enero"]));
                        valores.Add(Convert.ToDecimal(reader["Febrero"]));
                        valores.Add(Convert.ToDecimal(reader["Marzo"]));
                        valores.Add(Convert.ToDecimal(reader["Abril"]));
                        valores.Add(Convert.ToDecimal(reader["Mayo"]));
                        valores.Add(Convert.ToDecimal(reader["Junio"]));
                        valores.Add(Convert.ToDecimal(reader["Julio"]));
                        valores.Add(Convert.ToDecimal(reader["Agosto"]));
                        valores.Add(Convert.ToDecimal(reader["Septiembre"]));
                        valores.Add(Convert.ToDecimal(reader["Octubre"]));
                        valores.Add(Convert.ToDecimal(reader["Noviembre"]));
                        valores.Add(Convert.ToDecimal(reader["Diciembre"]));

                        GraficasReporteLine obj = new GraficasReporteLine();
                        obj.name = reader["usuario"].ToString();
                        obj.value = valores;
                        data.Add(obj);
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
            }

            object[] datos = new object[data.Count() + 1];
            datos[0] = new object[] { "Meses", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            foreach (var item in data)
            {
                object[] res = new object[13];
                res[0] = item.name.ToString();
                for (int i = 1; i < item.value.Count() + 1; i++)
                {
                    res[i] = item.value[i - 1];
                }
                datos[data.IndexOf(item) + 1] = res;
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
        }


        public string ConsultarComprasPreciosArticuloMes(string fecha_inicio, string fecha_fin, string clave)  //int id_articulo
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            List<GraficasReporteLine> data = new List<GraficasReporteLine>();
            try
            {
                int id_articulo = (int)db.C_articulos_catalogo.Where(x => x.clave == clave).Select(x => x.id_articulo).FirstOrDefault();
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_ComprasPreciosArticuloMes", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@id_articulo", SqlDbType.Int));
                    cmd.Parameters["@fecha_inicio"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                    cmd.Parameters["@fecha_fin"].Value = fecha_f.ToString("yyyy-MM-dd HH:mm");
                    cmd.Parameters["@id_articulo"].Value = id_articulo;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<object> valores = new List<object>();
                        valores.Add(reader["Fecha"].ToString());
                        valores.Add(Convert.ToDecimal(reader["Valor"]));

                        GraficasReporteLine obj = new GraficasReporteLine();
                        obj.name = reader["Fecha_format"].ToString();
                        obj.value = valores;
                        data.Add(obj);
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }


        public string ConsultarComprasDirectasMensualesPorAnio(string anio)
        {
            if (anio == "" || anio == null) { anio = DateTime.Today.Year.ToString(); }
            List<GraficasReportePie> data = new List<GraficasReportePie>();
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    //-- COMPRAS
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_ComprasMensualesTipos", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@anio", SqlDbType.Int));
                    cmd.Parameters.Add(new SqlParameter("@id_tipos_orden", SqlDbType.VarChar)); 
                    cmd.Parameters["@anio"].Value = anio;
                    cmd.Parameters["@id_tipos_orden"].Value = "1";
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        GraficasReportePie obj = new GraficasReportePie();
                        obj.ID = 1;
                        obj.Nombre = reader["Mes"].ToString();
                        obj.Valor = Convert.ToDecimal(reader["Importe_Total"].ToString());
                        data.Add(obj);
                    }
                    reader.Close();

                    //----- SERVICIOS
                    SqlCommand cmd2 = new SqlCommand("usp_4_DASHBOARD_ComprasMensualesTipos", conn);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.Add(new SqlParameter("@anio", SqlDbType.Int));
                    cmd2.Parameters.Add(new SqlParameter("@id_tipos_orden", SqlDbType.VarChar));
                    cmd2.Parameters["@anio"].Value = anio;
                    cmd2.Parameters["@id_tipos_orden"].Value = "2";
                    SqlDataReader reader2 = cmd2.ExecuteReader();
                    while (reader2.Read())
                    {
                        GraficasReportePie obj = new GraficasReportePie();
                        obj.ID = 2;
                        obj.Nombre = reader2["Mes"].ToString();
                        obj.Valor = Convert.ToDecimal(reader2["Importe_Total"].ToString());
                        data.Add(obj);
                    }
                    reader2.Close();

                    //----- INVESIONES
                    SqlCommand cmd3 = new SqlCommand("usp_4_DASHBOARD_ComprasMensualesTipos", conn);
                    cmd3.CommandType = CommandType.StoredProcedure;
                    cmd3.Parameters.Add(new SqlParameter("@anio", SqlDbType.Int));
                    cmd3.Parameters.Add(new SqlParameter("@id_tipos_orden", SqlDbType.VarChar));
                    cmd3.Parameters["@anio"].Value = anio;
                    cmd3.Parameters["@id_tipos_orden"].Value = "3";
                    SqlDataReader reader3 = cmd3.ExecuteReader();
                    while (reader3.Read())
                    {
                        GraficasReportePie obj = new GraficasReportePie();
                        obj.ID = 3;
                        obj.Nombre = reader3["Mes"].ToString();
                        obj.Valor = Convert.ToDecimal(reader3["Importe_Total"].ToString());
                        data.Add(obj);
                    }
                    reader3.Close();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
            }

            object[] datos = new object[data.Select(x => x.ID).Distinct().Count() + 1];
            datos[0] = new object[] { "Meses", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

            var data_compras = data.Where(x => x.ID == 1).ToArray();
            object[] res = new object[data_compras.Length +1];
            res[0] = "COMPRAS DIRECTAS";
            for (int i = 0; i < data_compras.Length; i++)
            {
                res[i + 1] = data_compras[i].Valor;
            }
            datos[1] = res;

            var data_servicios = data.Where(x => x.ID == 2).ToArray();
            object[] res2 = new object[data_servicios.Length + 1];
            res2[0] = "SERVICIOS";
            for (int i = 0; i < data_servicios.Length; i++)
            {
                res2[i + 1] = data_servicios[i].Valor;
            }
            datos[2] = res2;

            var data_inversiones = data.Where(x => x.ID == 3).ToArray();
            object[] res3 = new object[data_inversiones.Length + 1];
            res3[0] = "INVERSIONES";
            for (int i = 0; i < data_inversiones.Length; i++)
            {
                res3[i + 1] = data_inversiones[i].Valor;
            }
            datos[3] = res3;

            return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
        }

        public PartialViewResult ConsultarTopProveedoresCompras(string fecha_inicio, string fecha_fin)
        {

            DateTime fecha_i = DateTime.MinValue, fecha_f = DateTime.MinValue;
            DateTime hoy = DateTime.Now;
            int ultimoDia = DateTime.DaysInMonth(hoy.Year, hoy.Month);
            DateTime ultimoDiaDelMes = new DateTime(hoy.Year, hoy.Month, ultimoDia);

            int anio = Convert.ToInt32(hoy.Year);

            if (fecha_inicio == "" || fecha_inicio == null) { fecha_i = new DateTime(anio, hoy.Month, 01); }
            else { fecha_i = DateTime.Parse(fecha_inicio); }

            if (fecha_fin == "" || fecha_fin == null) { fecha_f = ultimoDiaDelMes.AddHours(23).AddMinutes(59); }
            else { fecha_f = DateTime.Parse(fecha_fin); }

                List<GraficasReportePie> data = new List<GraficasReportePie>();
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_ComprasProveedoresTOPImporte", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                    cmd.Parameters["@fecha_inicio"].Value = fecha_i.ToString("yyyy-MM-dd HH:mm");
                    cmd.Parameters["@fecha_fin"].Value = fecha_f.ToString("yyyy-MM-dd HH:mm");
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        GraficasReportePie obj = new GraficasReportePie();
                        obj.Nombre = reader["razon_social"].ToString();
                        obj.Valor = Convert.ToDecimal(reader["importe_total"]);
                        data.Add(obj);
                    }
                    reader.Close();
                    conn.Close();
                }
                return PartialView("_TopImportesComprasProveedores", data);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return PartialView("_TopImportesComprasProveedores", data);
            }
        }

        #endregion


        #region DASHBOARD SISTEMAS
        public string ConsultarLogSemanalGrafica(string fecha_inicio)
        {
            List<int> data = new List<int>();
            try
            {
                DateTime hoy = DateTime.Now.Date;
                if (fecha_inicio != "" && fecha_inicio != null) { hoy = DateTime.Parse(fecha_inicio); }
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_DEV_LogSemanal", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@fecha_semana", SqlDbType.VarChar));
                    cmd.Parameters["@fecha_semana"].Value = hoy.ToString("yyyy-MM-dd");
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        data.Add(Convert.ToInt32(reader["TotalLogs"].ToString()));
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public string ConsultarLogSemanalCorreosGrafica(string fecha_inicio)
        {
            List<int> data = new List<int>();
            try
            {
                DateTime hoy = DateTime.Now.Date;
                if (fecha_inicio != "" && fecha_inicio != null) { hoy = DateTime.Parse(fecha_inicio); }
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_DEV_LogSemanalCorreos", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@fecha_semana", SqlDbType.VarChar));
                    cmd.Parameters["@fecha_semana"].Value = hoy.ToString("yyyy-MM-dd");
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        data.Add(Convert.ToInt32(reader["TotalLogs"].ToString()));
                    }
                    reader.Close();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public PartialViewResult ConsultarLogsProcesos()
        {
            List<List<string>> data = new List<List<string>>();
            try
            {
                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_DEV_LogsProcesos", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<string> valores = new List<string>();
                        valores.Add(reader["nombre_log"].ToString());
                        valores.Add(reader["diferencia"].ToString());
                        valores.Add(reader["check_item"].ToString());
                        data.Add(valores);
                    }
                    reader.Close();
                    conn.Close();
                }
                return PartialView("_DEVLogProcesos", data);
            }
            catch (Exception ex)
            {
                return PartialView("_DEVLogProcesos", null);
            }
            
        }

        public PartialViewResult Consultar_KPIS_Sistemas()
        {
            try
            {
                JsonResult result = env_leche.ObtenerSemanaActual();
                var datos = result.Data;
                var data_inside = new { NumeroSemana = "", PrimerDiaSemana = "", UltimoDiaSemana = "" };
                var usuario = JsonConvert.DeserializeAnonymousType(JsonConvert.SerializeObject(datos), data_inside);

                DateTime fecha_inicio = DateTime.Parse(usuario.PrimerDiaSemana);
                DateTime fecha_fin = DateTime.Parse(usuario.UltimoDiaSemana).AddHours(23).AddMinutes(59);

                int usuarios_conectados = (int)(HttpContext.Application["UsuariosActivos"] ?? 0);
                List<List<string>> data = new List<List<string>>();

                List<string> usuarios = new List<string>();
                usuarios.Add("USUARIOS CONECTADOS");
                usuarios.Add(usuarios_conectados.ToString());
                data.Add(usuarios);

                List<string> cambio_dia = new List<string>();
                cambio_dia.Add("CAMBIO USD AL DÍA");
                cambio_dia.Add("$" + db.C_parametros_configuracion.Find(1015).valor_numerico.Value.ToString("N2"));
                data.Add(cambio_dia);

                int no_facturas = db.C_contabilidad_importacion_facturas_d.Where(x => x.id_status == 3
                                        && x.C_contabilidad_importacion_facturas_g.fecha_registro >= fecha_inicio && x.C_contabilidad_importacion_facturas_g.fecha_registro <= fecha_fin).Distinct().Count();
                List<string> facturas_rechazadas = new List<string>();
                facturas_rechazadas.Add("FACTURAS RECHAZADAS");
                facturas_rechazadas.Add(no_facturas.ToString());
                data.Add(facturas_rechazadas);
                return PartialView("_DEVCardsKPI", data);
            }
            catch (Exception)
            {
                return PartialView("_DEVCardsKPI", null);
            }
        }

        #endregion


        #region DASHBOARD ALMACEN
        public PartialViewResult ConsultarValesPendientesAlmacen()
        {
            List<List<string>> data = new List<List<string>>();
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                string id_almacenes = string.Join(",", db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => x.id_almacen_g));

                DateTime hoy = DateTime.Now.Date;

                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_4_DASHBOARD_AlmacenValesPendientes", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_almacen", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar));
                    cmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar));
                    cmd.Parameters["@id_almacen"].Value = id_almacenes;
                    cmd.Parameters["@fecha_inicio"].Value = hoy.AddDays(-20).ToString("yyyy-MM-dd");
                    cmd.Parameters["@fecha_fin"].Value = hoy.ToString("yyyy-MM-dd");
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<string> valores = new List<string>();
                        valores.Add(reader["tiempo"].ToString());
                        valores.Add(reader["no_vales"].ToString());
                        data.Add(valores);
                    }
                    reader.Close();
                    conn.Close();
                }
                ViewBag.Titulo_Header = "Vales pendientes de surtir";
                return PartialView("_ValesPendientesCardsKPI", data);
            }
            catch (Exception ex)
            {
                return PartialView("_ValesPendientesCardsKPI", null);
            }

        }


        public PartialViewResult ConsultarOrdenesPendientesRecepcion()
        {
            try
            {
                ViewBag.modo = 2;
                int id_usuario = (int)Session["LoggedId"];
                string id_almacenes = string.Join(",", db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => x.id_almacen_g));
                DateTime hoy = DateTime.Now.Date;
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
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = hoy.AddMonths(-3).ToString("yyyy/MM/dd");
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = hoy.ToString("yyyy/MM/dd");
                        sqlCmd.Parameters.Add(new SqlParameter("@id_almacenes", SqlDbType.VarChar)).Value = id_almacenes;
                        sqlCmd.Parameters.Add(new SqlParameter("@modo", SqlDbType.VarChar)).Value = 2;  //SOLO LOS PENDIENTES
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
                ViewBag.Titulo_Header = "Vales pendientes de surtir";
                return PartialView("../REPORTES/ReporteEntradaOrdenes/_ConsultarEntradasVsRecepcionOrdenesTable", dt);
            }
            catch (Exception ex)
            {
                return PartialView("../REPORTES/ReporteEntradaOrdenes/_ConsultarEntradasVsRecepcionOrdenesTable", null);
            }

        }



        #endregion




    }
}