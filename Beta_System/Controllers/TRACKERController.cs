using Beta_System.Models;
using Beta_System.Models.Tracker;
using FirebirdSql.Data.FirebirdClient;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Ast.Selectors;
using static Beta_System.Models.ConexionSab;

namespace Beta_System.Controllers
{
    public class TRACKERController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private TRACKER_LOGEntities db_tracker = new TRACKER_LOGEntities();
        private REPORTESController reportes = new REPORTESController();
        private CATALOGOSController catalogos = new CATALOGOSController();

        #region CENTRALIZADOR
        public ActionResult SincronizadorTracker()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8097)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("Centralizador/Index");
        }

        public List<IngredientesEstablo> GetIngredientesTracker(FbConnection conn)
        {
            List<IngredientesEstablo> data = new List<IngredientesEstablo>();
            try
            {
                //FbCommand readCommand = new FbCommand("select ID, DISPLAY_NAME, DESCRIPTION, IS_ACTIVE from DS_INGREDIENT", conn);
                //FbDataReader reader = readCommand.ExecuteReader();

                var rows = reportes.EjecturarQueryFireBird("select ID, DISPLAY_NAME, DESCRIPTION, IS_ACTIVE from DS_INGREDIENT", conn);
                for (int i = 0; i < rows.Count; i++)
                {
                    IngredientesEstablo obj = new IngredientesEstablo();
                    obj.id = Convert.ToInt32(rows[i][0]);
                    obj.clave = rows[i][1].ToString();
                    obj.descripcion = rows[i][2].ToString();
                    obj.activo = Convert.ToInt32(rows[i][3]);
                    data.Add(obj);
                }
                return data;
            }
            catch (Exception)
            {
                return data;
            }
        }

        public List<Cargas_Tracker> GetCargasMovimientosTracker(DateTime fecha_i, DateTime fecha_f, FbConnection conn, int id_establo)
        {
            List<Cargas_Tracker> data = new List<Cargas_Tracker>();
            try
            {
                string fecha_fin = fecha_f.ToString("dd.MM.yyyy HH:mm");

                string columna_precio = ", cag.TOTAL_PRICE"; // Agregar columna de precio si es necesario
                if (id_establo == 3) { columna_precio = ""; }

                string query = "SELECT cag.ID, carg_desc.ID, ration.id, ration.DISPLAY_NAME, ration.DESCRIPTION, ing.ID , ing.DISPLAY_NAME, ing.DESCRIPTION, cag.HEAD_COUNT, " +
                    "cag.CALL_WEIGHT, cag.LOADED_WEIGHT, (cag.LOADED_WEIGHT - cag.CALL_WEIGHT) \"Dif\", carro.ID, carro.DESCRIPTION, usu.ID, usu.\"NAME\" as \"Operador\", " +
                    " cag.DRYMATTER_PERC \"PMS_P\", cag.MODIFIED  " + columna_precio + "" +
                    " from DS_BATCH carg_desc" +
                    " JOIN DS_BATCH_LOAD cag on carg_desc.ID = cag.BATCH_ID" +
                    " JOIN DS_RATION ration on carg_desc.RATION_ID = ration.ID" +
                    " JOIN DS_USER usu on cag.USER_ID = usu.ID" +
                    " JOIN DS_MIXER carro on cag.MIXER_ID = carro.ID" +
                    " JOIN DS_INGREDIENT ing on cag.INGREDIENT_ID = ing.ID" +
                    //" WHERE carg_desc.ID = 319084", conn);
                    " WHERE cag.MODIFIED >= '" + fecha_i.ToString("dd.MM.yyyy") + "' and  cag.modified <= '" + fecha_fin + "' ORDER BY carg_desc.ID";
                var rows = reportes.EjecturarQueryFireBird(query, conn);
                for (int i = 0; i < rows.Count; i++)
                {
                    Cargas_Tracker new_carga = new Cargas_Tracker();
                    new_carga.cve_mov = "C";
                    new_carga.id_carga = Convert.ToInt32(rows[i][0]);
                    new_carga.id_barch = Convert.ToInt32(rows[i][1]);
                    new_carga.racion_id = Convert.ToInt32(rows[i][2]);
                    new_carga.racion_cve = rows[i][3].ToString();
                    new_carga.racion = rows[i][4].ToString();
                    new_carga.line_status = "S";
                    new_carga.ing_id = Convert.ToInt32(rows[i][5]);
                    new_carga.ing_cve = rows[i][6].ToString();
                    new_carga.ing_nombre = rows[i][7].ToString();
                    new_carga.animales = Convert.ToInt32(rows[i][8]);  //HEAD_COUNT
                    new_carga.peso_cargado = Convert.ToDecimal(rows[i][9]);
                    new_carga.peso_real = Convert.ToDecimal(rows[i][10]);
                    new_carga.deiferencia = Convert.ToDecimal(rows[i][11]);
                    new_carga.id_mezclador = Convert.ToInt32(rows[i][12]);
                    new_carga.nombre_mezclador = rows[i][13].ToString();
                    new_carga.id_operador = Convert.ToInt32(rows[i][14]);
                    new_carga.nombre_operador = rows[i][15].ToString();
                    new_carga.PMS_P = Convert.ToDecimal(rows[i][16]);
                    new_carga.PMS_R = 0; // No se obtiene este dato en la consulta
                    new_carga.fecha_registro = Convert.ToDateTime(rows[i][17]);
                    new_carga.precio = 0;

                    if (columna_precio != "")
                    {
                        try
                        {
                            decimal precio = Convert.ToInt32(rows[i][18]);
                            if (precio > 0)
                            {
                                new_carga.precio = precio / new_carga.peso_cargado;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    data.Add(new_carga);
                }

                return data;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return data;
            }
        }
        public List<Cargas_Tracker> GetDescargasMovimientosTracker(DateTime fecha_i, DateTime fecha_f, FbConnection conn, int id_establo)
        {
            List<Cargas_Tracker> data = new List<Cargas_Tracker>();
            try
            {
                string fecha_fin = fecha_f.ToString("dd.MM.yyyy HH:mm");

                string porc_mat = "cag.DRYMATTER_PERC";
                string columna_precio = ", cag.TOTAL_PRICE"; // Agregar columna de precio si es necesario

                if (id_establo == 3) { 
                    columna_precio = "";
                    porc_mat = "cag.ACTUALDM_PERC";
                }

                string query = "SELECT cag.ID, carg_desc.ID, racion.DISPLAY_NAME as \"Racion_id\", racion.ID \"cve_racion\", racion.DESCRIPTION as \"Racion\", grup.DISPLAY_NAME as \"Corral\", grup.ID AS \"Concepto_id\", " +
                    " grup.DESCRIPTION, tipo.\"NAME\", cag.HEAD_COUNT \"Animales\", cag.CALL_WEIGHT, cag.DELIVERED_WEIGHT, (cag.DELIVERED_WEIGHT - cag.CALL_WEIGHT) \"Dif\", " +
                    " carro.ID, carro.DESCRIPTION, usu.ID, usu.\"NAME\" as \"Operador\", "+ porc_mat +" , cag.ACTUALDM_PERC \"PMS_R\", cag.MODIFIED  "+ columna_precio + "" +
                    " FROM DS_BATCH carg_desc " +
                    " JOIN DS_BATCH_DELIVERY cag on carg_desc.ID = cag.BATCH_ID" +
                    " LEFT JOIN DS_GROUP grup on cag.GROUP_ID = grup.ID " +
                    " LEFT JOIN DS_USER usu on cag.USER_ID = usu.ID" +
                    " LEFT JOIN DS_MIXER carro on cag.MIXER_ID = carro.ID" +
                    " LEFT JOIN DS_RATION racion on carg_desc.RATION_ID = racion.ID" +
                    " LEFT JOIN DS_GROUP_TYPE tipo on grup.GROUP_TYPE = tipo.ID" +
                    " WHERE cag.MODIFIED >= '" + fecha_i.ToString("dd.MM.yyyy") + "' and  cag.modified <= '" + fecha_fin + "' ORDER BY carg_desc.ID";
                var rows = reportes.EjecturarQueryFireBird(query, conn);
                for (int i = 0; i < rows.Count; i++)
                {
                    Cargas_Tracker new_carga = new Cargas_Tracker();
                    new_carga.cve_mov = "D";
                    new_carga.id_carga = Convert.ToInt32(rows[i][0]);
                    new_carga.id_barch = Convert.ToInt32(rows[i][1]);
                    new_carga.racion_cve = rows[i][2].ToString();
                    new_carga.racion_id = Convert.ToInt32(rows[i][3]);
                    new_carga.racion = rows[i][4].ToString();
                    new_carga.line_status = "S";
                    new_carga.corral = rows[i][5].ToString();
                    new_carga.ing_cve = rows[i][6].ToString();
                    new_carga.ing_nombre = rows[i][7].ToString();
                    new_carga.ing_tipo = rows[i][8].ToString();
                    new_carga.animales = Convert.ToInt32(rows[i][9]);  //HEAD_COUNT
                    new_carga.peso_cargado = Convert.ToDecimal(rows[i][10]);
                    new_carga.peso_real = Convert.ToDecimal(rows[i][11]);
                    new_carga.deiferencia = Convert.ToDecimal(rows[i][12]);
                    new_carga.id_mezclador = Convert.ToInt32(rows[i][13]);
                    new_carga.nombre_mezclador = rows[i][14].ToString();
                    new_carga.id_operador = Convert.ToInt32(rows[i][15]);
                    new_carga.nombre_operador = rows[i][16].ToString();
                    new_carga.PMS_P = Convert.ToDecimal(rows[i][17]);
                    new_carga.PMS_R = Convert.ToDecimal(rows[i][18]);
                    new_carga.fecha_registro = Convert.ToDateTime(rows[i][19]);
                    new_carga.precio = 0;

                    if (columna_precio != "")
                    {
                        try
                        {
                            decimal precio = Convert.ToInt32(rows[i][20]);
                            if (precio > 0)
                            {
                                new_carga.precio = precio / new_carga.peso_cargado;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                    data.Add(new_carga);
                }

                return data;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return data;
            }
        }

        public int CentralizarInformacionTrackerEstablo(int id_establo, string fecha_inicio, string fecha_fin)
        {
            try
            {
                List<C_tracker_parametros> parametros = db_tracker.C_tracker_parametros.Where(x => x.id_establo == id_establo).ToList();
                string Servidor = parametros.Where(x => x.id_parametro_tipo == 1).FirstOrDefault().valor;
                string Puerto = parametros.Where(x => x.id_parametro_tipo == 2).FirstOrDefault().valor;
                string Usuario = parametros.Where(x => x.id_parametro_tipo == 3).FirstOrDefault().valor;
                string Clave = parametros.Where(x => x.id_parametro_tipo == 4).FirstOrDefault().valor;
                string BaseDatos = parametros.Where(x => x.id_parametro_tipo == 5).FirstOrDefault().valor;
                FbConnection conn = ConexionDinamicaMS.GetConexionMS(Usuario, Clave, BaseDatos, Servidor, Puerto).CrearConexion(Usuario, Clave, BaseDatos, Servidor, Puerto);
                conn.Open();
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
                DateTime hoy = DateTime.Now;
                List<IngredientesEstablo> IngredientesTracker = GetIngredientesTracker(conn);
                foreach (var ing in IngredientesTracker)
                {
                    var valid = db_tracker.C_tracker_ingredientes_establos.Where(x => x.id_establo == id_establo && x.id_ing == ing.id).FirstOrDefault();
                    if (valid == null)
                    {
                        C_tracker_ingredientes_establos new_ing = new C_tracker_ingredientes_establos();
                        new_ing.id_establo = id_establo;
                        new_ing.id_ing = ing.id;
                        new_ing.clave = ing.clave;
                        new_ing.descripcion = ing.descripcion;
                        new_ing.activo = Convert.ToBoolean(ing.activo);
                        db_tracker.C_tracker_ingredientes_establos.Add(new_ing);
                    }
                    else
                    {
                        valid.clave = ing.clave;
                        valid.descripcion = ing.descripcion;
                        valid.activo = Convert.ToBoolean(ing.activo);
                        db_tracker.SaveChanges();
                    }
                    db_tracker.SaveChanges();
                }

                //--------- Obtener Cargas
                List<Cargas_Tracker> cargas = GetCargasMovimientosTracker(fecha_i, fecha_f, conn, id_establo);
                int[] id_cargas = cargas.Select(x => x.id_carga).ToArray();
                int[] id_cargas_syncs = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && id_cargas.Contains((int)x.id_carg_desc) && x.id_movimiento_tracker == 1).Select(x => (int)x.id_carg_desc).ToArray();
                cargas = cargas.Where(x => !id_cargas_syncs.Contains((int)x.id_carga)).ToList();
                foreach (var carga in cargas)
                {
                    var valid = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && x.id_carg_desc == carga.id_carga && x.id_movimiento_tracker == 1).FirstOrDefault();
                    if (valid == null)
                    {
                        C_tracker_movimientos new_carg = new C_tracker_movimientos();
                        new_carg.id_tracker_establo = id_establo;
                        new_carg.fecha_syync = hoy;
                        new_carg.id_movimiento_tracker = 1;  //CARGAS
                        new_carg.id_barch = carga.id_barch;
                        new_carg.id_carg_desc = carga.id_carga;
                        new_carg.cve_racion = carga.racion_cve;
                        new_carg.racion = carga.racion;
                        new_carg.line_status = carga.line_status;
                        new_carg.id_ing = carga.ing_id;
                        new_carg.ing_cve = carga.ing_cve;
                        new_carg.ing_nombre = carga.ing_nombre;
                        new_carg.ing_tipo = "";
                        new_carg.no_animales = carga.animales;
                        new_carg.peso_cargado = carga.peso_cargado;
                        new_carg.peso_real = carga.peso_real;
                        new_carg.diferencia = carga.deiferencia;
                        new_carg.id_mezclador = carga.id_mezclador;
                        new_carg.nombre_mezclador = carga.nombre_mezclador;
                        new_carg.id_operador = carga.id_operador;
                        new_carg.nombre_operador = carga.nombre_operador;
                        new_carg.PMS_P = carga.PMS_P;
                        new_carg.PMS_R = carga.PMS_R;
                        new_carg.precio = carga.precio;
                        new_carg.fecha_registro = carga.fecha_registro;
                        new_carg.id_tracker_establo = id_establo;
                        new_carg.sobrante = 0;
                        db_tracker.C_tracker_movimientos.Add(new_carg);
                        db_tracker.SaveChanges();
                    }
                    else
                    {
                        valid.fecha_syync = hoy;
                        valid.id_movimiento_tracker = 1;  //CARGAS
                        valid.id_barch = carga.id_barch;
                        valid.id_carg_desc = carga.id_carga;
                        valid.cve_racion = carga.racion_cve;
                        valid.racion = carga.racion;
                        valid.line_status = carga.line_status;
                        valid.id_ing = carga.ing_id;
                        valid.ing_cve = carga.ing_cve;
                        valid.ing_nombre = carga.ing_nombre;
                        valid.ing_tipo = "";
                        valid.no_animales = carga.animales;
                        valid.peso_cargado = carga.peso_cargado;
                        valid.peso_real = carga.peso_real;
                        valid.diferencia = carga.deiferencia;
                        valid.id_mezclador = carga.id_mezclador;
                        valid.nombre_mezclador = carga.nombre_mezclador;
                        valid.id_operador = carga.id_operador;
                        valid.nombre_operador = carga.nombre_operador;
                        valid.PMS_P = carga.PMS_P;
                        valid.PMS_R = carga.PMS_R;
                        valid.precio = carga.precio;
                        valid.fecha_registro = carga.fecha_registro;
                        valid.id_tracker_establo = id_establo;
                        valid.sobrante = 0;
                        db_tracker.SaveChanges();
                    }
                }


                //--------- Obtener Descargas
                List<Cargas_Tracker> descargas = GetDescargasMovimientosTracker(fecha_i, fecha_f, conn, id_establo);
                int[] id_descargas = descargas.Select(x => x.id_carga).ToArray();
                int[] id_descargas_sync = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && id_descargas.Contains((int)x.id_carg_desc) && x.id_movimiento_tracker == 2).Select(x => (int)x.id_carg_desc).ToArray();
                descargas = descargas.Where(x => !id_descargas_sync.Contains((int)x.id_carga)).ToList();
                foreach (var descarga in descargas)
                {
                    var valid = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && x.id_carg_desc == descarga.id_carga && x.id_movimiento_tracker == 2).FirstOrDefault();
                    if (valid == null)
                    {
                        C_tracker_movimientos new_descarg = new C_tracker_movimientos();
                        new_descarg.id_tracker_establo = id_establo;
                        new_descarg.fecha_syync = hoy;
                        new_descarg.id_movimiento_tracker = 2;  //DESCARGAS
                        new_descarg.id_barch = descarga.id_barch;
                        new_descarg.id_carg_desc = descarga.id_carga;
                        new_descarg.cve_racion = descarga.racion_cve;
                        new_descarg.racion = descarga.racion;
                        new_descarg.line_status = descarga.line_status;
                        new_descarg.id_ing = descarga.ing_id;
                        new_descarg.ing_cve = descarga.ing_cve;
                        new_descarg.ing_nombre = descarga.ing_nombre;
                        new_descarg.ing_tipo = descarga.ing_tipo;
                        new_descarg.no_animales = descarga.animales;
                        new_descarg.peso_cargado = descarga.peso_cargado;
                        new_descarg.peso_real = descarga.peso_real;
                        new_descarg.diferencia = descarga.deiferencia;
                        new_descarg.id_mezclador = descarga.id_mezclador;
                        new_descarg.nombre_mezclador = descarga.nombre_mezclador;
                        new_descarg.id_operador = descarga.id_operador;
                        new_descarg.nombre_operador = descarga.nombre_operador;
                        new_descarg.PMS_P = descarga.PMS_P;
                        new_descarg.PMS_R = descarga.PMS_R;
                        new_descarg.precio = descarga.precio;
                        new_descarg.fecha_registro = descarga.fecha_registro;
                        new_descarg.id_tracker_establo = id_establo;
                        new_descarg.sobrante = 0;
                        new_descarg.corral = descarga.corral;
                        db_tracker.C_tracker_movimientos.Add(new_descarg);
                        db_tracker.SaveChanges();
                    }
                    else
                    {
                        valid.fecha_syync = hoy;
                        valid.id_movimiento_tracker = 2;  //DESCARGAS
                        valid.id_barch = descarga.id_barch;
                        valid.id_carg_desc = descarga.id_carga;
                        valid.cve_racion = descarga.racion_cve;
                        valid.racion = descarga.racion;
                        valid.line_status = descarga.line_status;
                        valid.id_ing = descarga.ing_id;
                        valid.ing_cve = descarga.ing_cve;
                        valid.ing_nombre = descarga.ing_nombre;
                        valid.ing_tipo = descarga.ing_tipo;
                        valid.no_animales = descarga.animales;
                        valid.peso_cargado = descarga.peso_cargado;
                        valid.peso_real = descarga.peso_real;
                        valid.diferencia = descarga.deiferencia;
                        valid.id_mezclador = descarga.id_mezclador;
                        valid.nombre_mezclador = descarga.nombre_mezclador;
                        valid.id_operador = descarga.id_operador;
                        valid.nombre_operador = descarga.nombre_operador;
                        valid.PMS_P = descarga.PMS_P;
                        valid.PMS_R = descarga.PMS_R;
                        valid.precio = descarga.precio;
                        valid.fecha_registro = descarga.fecha_registro;
                        valid.id_tracker_establo = id_establo;
                        valid.sobrante = 0;
                        valid.corral = descarga.corral;
                        db_tracker.SaveChanges();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        #endregion


        #region MOVIMIENTOS
        public ActionResult TrackerMovimientos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8095)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("Movimientos/Index");
        }
        public PartialViewResult ConsultarMovimientosTrackerTable(string fecha_inicio, string fecha_fin, int[] id_establo, int id_tipo_mov)
        {
            DataTable dt = new DataTable();
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            string f1 = fecha_i.Date.ToString("yyyy/MM/dd HH:mm:ss");
            string f2 = fecha_f.ToString("yyyy/MM/dd HH:mm:ss"); ;

            if (id_establo.Contains(0))
            {
                int id_usuario = (int)Session["LoggedId"];
                id_establo = catalogos.EstablosUsuariosID(id_usuario);
            }
            string establos = string.Join(",", id_establo);

            if (id_tipo_mov == 1)  //CARGA
            {
                using (SqlCommand sqlCmd = new SqlCommand())
                {
                    try
                    {
                        SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                        sqlCmd.CommandText = "usp_5_TRACKER_ConsultarMovimientosCargas";
                        sqlCmd.Connection = mSqlCnn;
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandTimeout = 300000;
                        sqlCmd.Parameters.Add(new SqlParameter("@id_establo", SqlDbType.VarChar)).Value = establos;
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = f1;
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = f2;
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
                return PartialView("Movimientos/_CargasTrackerDataTable", dt);
            }
            else
            {
                using (SqlCommand sqlCmd = new SqlCommand())
                {
                    try
                    {
                        SqlConnection mSqlCnn = ConexionSIIB.Conectar();
                        sqlCmd.CommandText = "usp_5_TRACKER_ConsultarMovimientosDescargas";
                        sqlCmd.Connection = mSqlCnn;
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandTimeout = 300000;
                        sqlCmd.Parameters.Add(new SqlParameter("@id_establo", SqlDbType.VarChar)).Value = establos;
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_inicio", SqlDbType.VarChar)).Value = f1;
                        sqlCmd.Parameters.Add(new SqlParameter("@fecha_fin", SqlDbType.VarChar)).Value = f2;
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
                return PartialView("Movimientos/_DescargasTrackerDataTable", dt);
            }
        }



        #endregion


        #region CONFIGURACION
        public ActionResult AsociacionArticulosIngredientes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8096)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("ConfigurarIngredientes/Index");
        }

        public PartialViewResult ConsultarArticulosIngredientesTrackerTable(int id_establo)
        {
            //var data = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_establo == id_establo && x.activo == true).ToList();
            //return PartialView("ConfigurarIngredientes/_ArticulosIngredientesTrackerTable", data);
            try
            {
                List<List<string>> data = new List<List<string>>();

                using (var conn = ConexionSIIB.Conectar())
                {
                    SqlCommand cmd = new SqlCommand("usp_5_TRACKER_ConsultarArticulosIngredientesTracker", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@id_establo", SqlDbType.VarChar));
                    cmd.Parameters["@id_establo"].Value = id_establo;
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        List<string> obj = new List<string>();
                        obj.Add(reader["id_ingrediente_articulo"].ToString());
                        obj.Add(reader["nombre_articulo"].ToString());
                        obj.Add(reader["descripcion"].ToString());
                        data.Add(obj);
                    }
                    reader.Close();
                    conn.Close();
                }
                return PartialView("ConfigurarIngredientes/_ArticulosIngredientesTrackerTable", data);
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return PartialView("ConfigurarIngredientes/_ArticulosIngredientesTrackerTable", null);
            }
        }

        public PartialViewResult ConsultarArticulosForrajesTable(int id_establo)
        {
            var articulos = db.C_articulos_catalogo_bascula.Where(x => x.activo == true && x.C_articulos_catalogo.activo == true && x.id_ficha_tipo == 2).Select(x => x.C_articulos_catalogo).Distinct().OrderBy(x => x.nombre_articulo).ToList();
            return PartialView("ConfigurarIngredientes/_ArticulosForrajesTable", articulos);
        }

        public PartialViewResult ConsultarArticulosTrackerTable(int id_establo)
        {
            var ing_asociados = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_establo == id_establo && x.activo == true).Select(x => x.id_ingrediente).ToList();

            var articulos = db_tracker.C_tracker_ingredientes_establos.Where(x => x.id_establo == id_establo && x.activo == true && !ing_asociados.Contains((int)x.id_ing)).OrderBy(x => x.descripcion).ToList();
            return PartialView("ConfigurarIngredientes/_IngredientesTrackerTable", articulos);
        }

        public int AsociarArticuloIngrediente(int id_articulo, int[] id_ingredientes, int id_establo)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                for (int i = 0; i < id_ingredientes.Length; i++)
                {
                    int id_ing = id_ingredientes[i];
                    var valid = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_articulo == id_articulo && x.id_ingrediente == id_ing && x.id_establo == id_establo).FirstOrDefault();
                    if (valid == null)
                    {
                        C_alimentacion_tracker_articulos_ingredientes newAsoc = new C_alimentacion_tracker_articulos_ingredientes
                        {
                            id_articulo = id_articulo,
                            id_ingrediente = id_ing,
                            id_establo = id_establo,
                            fecha_registro = hoy,
                            id_usuario_registra = id_usuario,
                            activo = true
                        };
                        db.C_alimentacion_tracker_articulos_ingredientes.Add(newAsoc);
                    }
                    else
                    {
                        valid.activo = true;
                        db.Entry(valid).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool EliminarAsociacionArticuloIngrediente(int id_ingrediente_articulo)
        {
            try
            {
                var asoc = db.C_alimentacion_tracker_articulos_ingredientes.Find(id_ingrediente_articulo);
                if (asoc != null)
                {
                    asoc.activo = false;
                    db.Entry(asoc).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }


        #endregion


        #region KARDEX DE INGREDIENTES
        public ActionResult KardexIngredientes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8100)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("KardexIngredientes/Index");
        }

        public PartialViewResult ConsultarKardexIngredientesFecha(string fecha_inicio, string fecha_fin, int id_establo, int id_articulo)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            var fichas = db.C_bascula_fichas.Where(x => x.id_establo == id_establo && x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && x.activo == true
                            && x.id_ficha_tipo == 2 && x.id_articulo_bascula == id_articulo && (x.id_codigo_movimiento == 1 || x.id_codigo_movimiento == 3)).ToList(); //COMPRA/TRASNFERENCIA
            List<KardexIngredientes> data = new List<KardexIngredientes>();
            foreach (var item in fichas)
            {
                KardexIngredientes kardex = new KardexIngredientes();
                kardex.Fecha = string.Format(item.fecha_registo.Value.Date.ToShortDateString(), "dd/mm/yyyy");
                if (item.id_codigo_movimiento == 1) { kardex.Compra = (decimal)item.peso_t; }
                else if(item.id_codigo_movimiento == 3) { kardex.Transferencia = (decimal)item.peso_t; }
                data.Add(kardex);
            }

            int[] id_ingredientes = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_articulo == id_articulo && x.id_establo == id_establo && x.activo == true).Select(x => (int)x.id_ingrediente).ToArray();
            var consumos = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && id_ingredientes.Contains((int)x.id_ing) 
                && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 1).ToList();  // CARGAS
            foreach (var item in consumos)
            {
                KardexIngredientes kardex = new KardexIngredientes();
                kardex.Fecha = string.Format(item.fecha_registro.Value.Date.ToShortDateString(), "dd/mm/yyyy");
                kardex.Consumo = (decimal)item.peso_real;
                data.Add(kardex);
            }

            ViewBag.nombre_ing = db.C_articulos_catalogo.Find(id_articulo).nombre_articulo;
            ViewBag.fecha_inicio = string.Format(fecha_i.ToShortDateString(), "dd/mm/yyyy");
            ViewBag.fecha_fin = string.Format(fecha_f.ToShortDateString(), "dd/mm/yyyy");
            return PartialView("KardexIngredientes/_KardexIngredientesFechaTable", data.OrderBy(x => x.Fecha).ToList());
        }

        public PartialViewResult ConsultarKardexIngredientesDetallado(string fecha_inicio, string fecha_fin, int id_establo, int id_articulo)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            var fichas = db.C_bascula_fichas.Where(x => x.id_establo == id_establo && x.fecha_registo >= fecha_i && x.fecha_registo <= fecha_f && x.activo == true
                            && x.id_ficha_tipo == 2 && x.id_articulo_bascula == id_articulo && (x.id_codigo_movimiento == 1 || x.id_codigo_movimiento == 3) && x.termina == true).ToList(); //COMPRA/TRASNFERENCIA
            List<KardexIngredientes> data = new List<KardexIngredientes>();
            foreach (var item in fichas)
            {
                KardexIngredientes kardex = new KardexIngredientes();
                string hra_1 = item.fecha_registo.Value.ToString("hh:mm tt");
                string hra_2 = item.fecha_segunda_pesada.Value.ToString("hh:mm tt");

                kardex.Fecha = string.Format(item.fecha_registo.Value.Date.ToShortDateString(), "dd/mm/yyyy");
                kardex.hra_entrada = hra_1;
                if (item.id_codigo_movimiento == 1) { 
                    kardex.Compra = (decimal)item.peso_t;
                    kardex.tipo = "COMPRA";
                    kardex.concepto = "Ficha " + item.id_bascula_ficha + " Destino " + item.C_compras_proveedores.alias_bascula + " Placas " + item.placas + " Op. " + item.chofer.ToUpper() + " 1er pesada " + hra_1 + " 2da pesada " + hra_2;
                }
                else if (item.id_codigo_movimiento == 3) { 
                    kardex.Transferencia = (decimal)item.peso_t;
                    kardex.tipo = "TRANSFERENCIA";
                    kardex.concepto = "Ficha " + item.id_bascula_ficha + "Prov. " + item.C_compras_proveedores.alias_bascula + " Placas " + item.placas + " Op. " + item.chofer.ToUpper() + " 1er pesada " + hra_1 + " 2da pesada " + hra_2;
                }

                data.Add(kardex);
            }

            int[] id_ingredientes = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_articulo == id_articulo && x.id_establo == id_establo && x.activo == true).Select(x => (int)x.id_ingrediente).ToArray();
            var consumos = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && id_ingredientes.Contains((int)x.id_ing)
                && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 1).ToList();  // CARGAS
            foreach (var item in consumos)
            {
                KardexIngredientes kardex = new KardexIngredientes();
                kardex.Fecha = string.Format(item.fecha_registro.Value.Date.ToShortDateString(), "dd/mm/yyyy");
                kardex.hra_entrada = item.fecha_registro.Value.ToString("hh:mm tt");
                kardex.Consumo = (decimal)item.peso_real;
                kardex.concepto = "Carga ración " + item.racion.ToUpper();
                kardex.tipo = "CONSUMO";
                data.Add(kardex);
            }

            return PartialView("KardexIngredientes/_KardexIngredientesDetalleTable", data.OrderBy(x => x.Fecha).ToList());
        }

        public PartialViewResult ConsultarKardexIngredientesResumen(string fecha_inicio, string fecha_fin, int id_establo, int[] id_articulo)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

            ViewBag.fecha_inicio = string.Format(fecha_i.ToShortDateString(), "dd/mm/yyyy");
            ViewBag.fecha_fin = string.Format(fecha_f.ToShortDateString(), "dd/mm/yyyy");

            List<KardexIngredientes> data = new List<KardexIngredientes>();
            int[] ing_tracker = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_establo == id_establo && x.activo == true && 
                    id_articulo.Contains((int)x.id_articulo)).Select(x => (int)x.id_ingrediente).ToArray();

            var consumos = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo && ing_tracker.Contains((int)x.id_ing)
                            && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 1);

            return PartialView("KardexIngredientes/_KardexIngredientesResumenTable", consumos);

        }


        #endregion


        #region TIRADAS DIARIAS
        public ActionResult TiradasDiariasEstablo()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8101)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
            return View("TiradasDiarias/Index");
        }
        public PartialViewResult ConsultarTiradasDiarias(int id_establo, string fecha)
        {
            //int[] id_ingredientes = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_establo == id_establo && x.activo == true).Select(x => (int)x.id_ingrediente).ToArray();
            
            DateTime fecha_i = DateTime.Parse(fecha).Date;
            DateTime fecha_f = fecha_i.AddHours(23).AddMinutes(59);

            var consumos = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo /*&& id_ingredientes.Contains((int)x.id_ing)*/
                && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 1 
                && !x.racion.Contains("MEZCLA") && !x.racion.Contains("CORRAL") && !x.racion.Contains("HUMED") && !x.racion.Contains("SOB")).ToList();
            List<TiradasDiarias> data = new List<TiradasDiarias>();
            foreach (var item in consumos)
            {
                TiradasDiarias tirada = new TiradasDiarias();
                tirada.id_ing = (int)item.id_ing;
                tirada.clave = item.ing_cve;
                tirada.ingrediente = item.ing_nombre;

                if (item.racion.Contains("ALT") || item.racion.Contains("MEDIA") || item.racion.Contains("FRES")) { tirada.tipo = "PRODUCCION"; }
                else if (item.racion.Contains("SECA")) { tirada.tipo = "SECAS"; }
                else if (item.racion.Contains("RETO")) { tirada.tipo = "RETO"; }
                else { tirada.tipo = "OTRO"; }
                
                tirada.ppto = item.peso_cargado.HasValue ? item.peso_cargado.Value : 0;
                tirada.real = item.peso_real.HasValue ? item.peso_real.Value : 0;

                data.Add(tirada);
            }

            var mezclas = db_tracker.C_tracker_movimientos.Where(x => x.id_tracker_establo == id_establo /*&& id_ingredientes.Contains((int)x.id_ing)*/
                && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 1
                && (x.cve_racion.Contains("MEZRET") || x.cve_racion.Contains("MEREVQ") || x.cve_racion.Contains("MESECA")
                        || x.cve_racion.Contains("MEZSUA") || x.cve_racion.Contains("REMOHU") )).ToList();
            List<TiradasDiarias> data_mezcla = new List<TiradasDiarias>();
            foreach (var item in mezclas)
            {
                TiradasDiarias mezcla = new TiradasDiarias();
                mezcla.id_ing = (int)item.id_ing;
                mezcla.clave = item.ing_cve;
                mezcla.ingrediente = item.ing_nombre;

                //if (item.cve_racion.Contains("MEZRET") ) { tirada.tipo = "PRODUCCION"; }
                //else if (item.racion.Contains("SECA")) { tirada.tipo = "SECAS"; }
                //else if (item.racion.Contains("RETO")) { tirada.tipo = "RETO"; }
                //else { tirada.tipo = "OTRO"; }

                mezcla.tipo = item.cve_racion;
                mezcla.ppto = item.peso_cargado.HasValue ? item.peso_cargado.Value : 0;
                mezcla.real = item.peso_real.HasValue ? item.peso_real.Value : 0;
                data_mezcla.Add(mezcla);
            }

            ViewBag.data_mezcla = data_mezcla;
            return PartialView("TiradasDiarias/_TiradasDiariasTable", data.OrderBy(x => x.ingrediente).ToList());
        }








        #endregion


        #region CONSUMOS POR CORRAL
        public ActionResult ConsumosCorral()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8102)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("ConsumosCorral/Index");
        }

        public PartialViewResult ConsultarConsumosXCorralTracker(int[] id_establo, string fecha_inicio, string fecha_fin)
        {
            if (id_establo.Contains(0)) { int id_usuario = (int)Session["LoggedId"]; id_establo = catalogos.EstablosUsuariosID(id_usuario); }

            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            var consumos = db_tracker.C_tracker_movimientos.Where(x => id_establo.Contains((int)x.id_tracker_establo) 
                            && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f && x.id_movimiento_tracker == 2).GroupBy(x => (int)x.id_tracker_establo).ToList();
            return PartialView("ConsumosCorral/_ConsumosXCorralTrackerTable", consumos);
        }



        #endregion


    }
}