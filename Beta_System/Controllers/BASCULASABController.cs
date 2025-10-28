using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;
using System.Data.Entity.Infrastructure;
using System.Web.UI;
using System.Data.SqlClient;
using System.Web.Caching;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Beta_System.Models.ConexionSab;
using Newtonsoft.Json;
using Irony.Parsing;
using Microsoft.Ajax.Utilities;
using System.Data;
using System.Collections.ObjectModel;
using ClosedXML.Excel;
using System.IO;
using System.Net;

namespace Beta_System.Controllers
{
    public class BASCULASABController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();

        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6032)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../Establos/BasculaSab/Index");
        }

        public ActionResult IndexSistemas()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6032)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../Establos/BasculaSistemas/Index");
        }

        public PartialViewResult ConsultarEstablosUsuarios(int id_usuario)
        {
            var establos = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).ToList();
            return PartialView("../Establos/BasculaSab/_EstablosUsuarioSelect", establos);
        }


        public PartialViewResult ConsultarPozos(int id_establo)
        {
            var pozos = db.C_bascula_no_pozos.Where(x => x.activo == true && x.id_establo == id_establo).OrderBy(x => x.no_pozo).ToList();
            return PartialView("../Establos/Bascula/_NumeroPozos", pozos);
        }

        public PartialViewResult ConsultarTiposMovimiento()
        {
            var proveedores = db.C_bascula_tipos_movimientos.Where(x => x.activo == true).OrderBy(x => x.nombre_movimiento).ToList();
            return PartialView("../Establos/Bascula/_TiposMovimientosSelect", proveedores);
        }

        public PartialViewResult ConsultarClavesMovimientos()
        {
            var proveedores = db.C_bascula_codigos_movimientos.Where(x => x.activo == true).OrderBy(x => x.codigo_mov).ToList();
            return PartialView("../Establos/Bascula/_CveMovimientosSelect", proveedores);
        }

        public PartialViewResult ConsultarLineasTrasnportistas()
        {
            var proveedores = db.C_bascula_lineas_transportistas.Where(x => x.activo == true).OrderBy(x => x.nombre_linea).ToList();
            return PartialView("../Establos/Bascula/_LineasTransportistasSelect", proveedores);
        }

        public int ConsultarUltimaFicha()
        {
            var ficha = db.C_bascula_fichas.OrderByDescending(x => x.id_bascula_ficha).FirstOrDefault();
            if (ficha != null) { return ficha.id_bascula_ficha + 1; }

            else { return 1; }

        }

        public string ObtenerPesoBascula(int id_establo)
        {
            try
            {
                var parametros = db.C_bascula_establos_codigos_puertos.Where(x => x.id_establo == id_establo).FirstOrDefault();

                var bascula = new BasculaReader();
                //abrir puerto
                var abierto = bascula.Open(parametros.puerto, 9600, 0, 8, 1);

                if (abierto) // se abrió el puerto
                {
                    bascula.SetReadTimeout(500);
                    bascula.SetWriteTimeout(500);
                    bascula.SetHandshake(0);
                    string codigo_string = parametros.codigo;
                    if (codigo_string == "P1\\r") { codigo_string = codigo_string.Replace("\\r", "\r"); }
                    var pesada = bascula.Read((bool)parametros.continua, codigo_string);

                    //LINEA PARA SANTA MONICA
                    //var pesada = bascula.Read(true, "3%Q");

                    //CODIGO PARA SAN IGNACIO
                    //var pesada = bascula.Read(true, "P1\x0D");

                    //LINEA PARA SAN SAN GABRIEL y TANQUE NUEVO
                    //var pesada = bascula.Read(false, "");
                    bascula.Close();
                    return pesada;
                }
                else
                {
                    return "No se pudo abrir el puerto";
                }

            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }

        }

        public string ObtenerPeso(int id_establo)
        {
            try
            {
                SerialPort mySerialPort = new SerialPort();
                mySerialPort.PortName = "COM1";
                mySerialPort.BaudRate = 9600;
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.DataBits = 8;
                mySerialPort.Handshake = Handshake.None;
                mySerialPort.RtsEnable = true;
                mySerialPort.DtrEnable = true;
                mySerialPort.Open();
                string indata = mySerialPort.ReadExisting();
                mySerialPort.Close();
                return indata;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string GetPortsNames()
        {
            string[] ports = SerialPort.GetPortNames();
            return Newtonsoft.Json.JsonConvert.SerializeObject(ports);
        }

        public string GetConectionData(int id_establo)
        {
            //var parametros = db.C_bascula_establos_codigos_puertos.Where(x => x.id_establo == id_establo).FirstOrDefault();
            var data = from param in db.C_bascula_establos_codigos_puertos
                       where param.id_establo == id_establo
                       select new { param.puerto, param.codigo, param.continua };
            return JsonConvert.SerializeObject(data);
        }

        public bool GuardarFicha(C_bascula_fichas c_ficha, string peso_1)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                string valor_ficha = new string(peso_1.Where(char.IsDigit).ToArray());
                c_ficha.peso_1 = Convert.ToInt32(valor_ficha);
                c_ficha.fecha_registo = DateTime.Now;
                c_ficha.activo = true;
                c_ficha.id_usuario = id_usuario;
                db.C_bascula_fichas.Add(c_ficha);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return false;
            }
        }

        //---------- SEGUNDA PESADA
        public string ConsultarInfoFicha(int ficha)
        {
            var datos = from fich in db.C_bascula_fichas
                        join lin_tra in db.C_bascula_lineas_transportistas on fich.id_linea_transportista equals lin_tra.id_bascula_linea_transp
                        join tmov in db.C_bascula_tipos_movimientos on fich.id_tipo_movimiento equals tmov.id_tipo_movimiento
                        join cvemo in db.C_bascula_codigos_movimientos on fich.id_codigo_movimiento equals cvemo.id_bascula_codigo_movimiento
                        join pozo in db.C_bascula_no_pozos on fich.id_no_pozo equals pozo.id_no_pozo
                        where fich.id_bascula_ficha == ficha
                        select new
                        {
                            pozo.no_pozo,
                            ////pro.nombre_prov,
                            //art.nombre_articulo,
                            tmov = tmov.nombre_movimiento,
                            cvemo = cvemo.codigo_mov + " - " + cvemo.descripcion,
                            fich.folio,
                            fich.sucursal,
                            fich.destino,
                            linea_trasp = lin_tra.nombre_linea,
                            conductor = fich.chofer,
                            fich.placas,
                            fich.maquilador,
                            fich.predio_cliente,
                            fich.observaciones,
                            fich.peso_origen,
                            fich.peso_materia_seca,
                            fich.peso_1,
                            fich.peso_2,
                            fich.agrupada
                        };
            return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
        }

        public string GuardarSegundaPesada(int ficha, string peso_2)
        {
            try
            {
                string valor_ficha = new string(peso_2.Where(char.IsDigit).ToArray());
                int valor_peso_2 = Convert.ToInt32(valor_ficha);

                var ficha_reg = db.C_bascula_fichas.Find(ficha);

                decimal peso_t = (decimal)ficha_reg.peso_1 - valor_peso_2;

                ficha_reg.fecha_segunda_pesada = DateTime.Now;
                ficha_reg.peso_2 = valor_peso_2;
                ficha_reg.peso_t = Math.Abs(peso_t);
                ficha_reg.termina = true;
                db.SaveChanges();
                return Math.Abs(peso_t).ToString();
            }
            catch (Exception)
            {
                return "No se encontró la ficha. Asegurese de no modificar el campo una vez se haya buscado";
            }
        }

        public PartialViewResult ConsultarFichasReporte(int id_establo, string fecha_inicio, string fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];
            IQueryable<C_establos> establos = null;
            if (id_establo == 0)
            {
                establos = from usu in db.C_usuarios_corporativo
                           join us_es in db.C_usuarios_establos on usu.id_usuario_corporativo equals us_es.id_usuario
                           join est in db.C_establos on us_es.id_establo equals est.id_establo
                           where usu.id_usuario_corporativo == id_usuario
                           select est;
            }
            else
            {
                establos = db.C_establos.Where(x => x.id_establo == id_establo);
            }

            var establos_array = establos.Select(x => x.id_establo).ToArray();
            var fichas = db.C_bascula_fichas.Where(x => establos_array.Contains((int)x.id_establo) && x.activo == true).ToList();
            return PartialView("../Establos/Bascula/_FichasReporte", fichas);

        }

        #region BASCULA SAB
        public PartialViewResult ObtenerPozos(int establo)
        {
            string query = "SELECT id_no_pozo, no_pozo FROM no_pozos WHERE id_establo = @id_establo";

            using (var connection = Models.ConexionSab.Conectar())
            {
                ConexionSab.No_Pozos pozos = new ConexionSab.No_Pozos();
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_establo", establo);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        pozos.ID.Add(reader.GetInt32(0));
                        pozos.No_pozo.Add(reader.GetString(1));
                    }
                }

                return PartialView("../Establos/BasculaSab/_NumeroPozos", pozos); // Assuming "_NumeroPozos" is your partial view
            }
        }

        public PartialViewResult ObtenerLineaTransportista()
        {
            string query = "SELECT id_linea_transportista, nombre_linea FROM lineas_transportistas WHERE rstatus = 'A'";

            using (var connection = Models.ConexionSab.Conectar())
            {
                ConexionSab.Lineas lineas = new ConexionSab.Lineas();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        lineas.ID_Linea.Add(reader.GetInt32(0));
                        lineas.No_linea.Add(reader.GetString(1));
                    }
                }

                return PartialView("../Establos/BasculaSab/_LineasTransportistasSelect", lineas); // Assuming "_NumeroPozos" is your partial view
            }
        }

        public PartialViewResult ObtenerProveedor(int proveedor)
        {
            string query = "";
            //SANTA MONICA
            if (proveedor == 3)
            {
                query = "SELECT cvepro, despro FROM proveedo_B WHERE rstatus = 'A'";
            }
            //SG, SI, TN
            else
            {
                query = "SELECT cvepro, despro FROM proveedo WHERE rstatus = 'A'";
            }

            using (var connection = Models.ConexionSab.Conectar())
            {
                ConexionSab.Proveedores proveedor_siib = new ConexionSab.Proveedores();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        proveedor_siib.ID_Proveedor.Add(reader.GetString(0));
                        proveedor_siib.No_proveedor.Add(reader.GetString(1));
                    }
                }

                return PartialView("../Establos/BasculaSab/_ProveedoresSelect", proveedor_siib);
            }
        }
        public PartialViewResult ObtenerArticulo()
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                string query = "SELECT cveart, desart FROM articulo WHERE rstatus = 'A'";
                ConexionSab.Articulos_Sab articulos_Siib = new ConexionSab.Articulos_Sab();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        articulos_Siib.ID_Articulo.Add(reader.GetString(0));
                        articulos_Siib.No_articulo.Add(reader.GetString(1));
                    }
                }

                return PartialView("../Establos/BasculaSab/_ArticulosSelect", articulos_Siib);
            }
        }

        public string ObtenerFichaSab(int establo)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                //1 SG, 2 SI, 3 SM, 4 TN
                string query = "";
                switch (establo)
                {
                    case 1:
                        query = "SELECT  TOP (1) ficha FROM base_forrajes.dbo.fichas_sg  WHERE fecha >= '2024-01-01' ORDER BY ficha DESC";
                        break;
                    case 2:
                        query = "SELECT  TOP (1) ficha FROM base_forrajes.dbo.fichas_si  WHERE fecha >= '2024-01-01' ORDER BY ficha DESC";
                        break;
                    case 3:
                        query = "SELECT  TOP (1) ficha FROM base_forrajes.dbo.fichas_sm  WHERE fecha >= '2024-01-01' ORDER BY ficha DESC";
                        break;
                    case 4:
                        query = "SELECT  TOP (1) ficha FROM base_forrajes.dbo.fichas_tn  WHERE fecha >= '2024-01-01' ORDER BY ficha DESC";
                        break;
                }
                ConexionSab.Articulos_Sab articulos_Siib = new ConexionSab.Articulos_Sab();
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandTimeout = 10000;
                SqlDataReader reader = command.ExecuteReader();
                string lastFicha = "";
                string newFicha = "";
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        lastFicha = reader["ficha"].ToString();
                    }
                    else { lastFicha = "0"; }
                    string numericString = new string(lastFicha.Where(char.IsDigit).ToArray());
                    int lastFichaNumber = int.Parse(numericString);
                    int newFichaNumber = lastFichaNumber + 1;
                    newFicha = newFichaNumber.ToString().PadLeft(25, ' ');
                }
                else
                {
                    newFicha = "ERROR";
                }
                return newFicha;
            }
        }

        #region Parametros bascula
        /*
         * 0 id_establo            6 tmov
         * 1 id_ficha_g            7 folio
         * 2 no_pozo               8 sucursal
         * 3 select_proveedor      9 destino
         * 4 select_articulo       10 linea_trasp
         * 5 cvemov                11 chofer
         * 
         * 12 placas               15 obs
         * 13 maquilador           16 peso_origen
         * 14 predio_cliente       17 mat_seca
         * 
         * 18 agrupada             19 peso_bascula
         */
        #endregion

        public bool GuardarFichaSab(string[] primera_pesada)
        {
            var control_siib = db.C_parametros_configuracion.Where(x => x.id_parametro_configuracion == 13).FirstOrDefault();
            //var control_siib = 0;
            string valor_ficha = new string(primera_pesada[19].Where(char.IsDigit).ToArray());
            var usuario = (int)Session["LoggedId"];
            var empleado = db.C_usuarios_corporativo.Find(usuario);
            var nombre = empleado.usuario;

            string tabla_principal = "";




            control_siib.valor_numerico = 1;




            if (control_siib.valor_numerico == 0)
            {
                switch (primera_pesada[0])
                {
                    case "1":
                        tabla_principal = "fichas_sg";
                        break;
                    case "2":
                        tabla_principal = "fichas_si";
                        break;
                    case "3":
                        tabla_principal = "fichas_sm";
                        break;
                    case "4":
                        tabla_principal = "fichas_tn";
                        break;
                }
            }
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    if (primera_pesada[16] == " ")
                    {
                        primera_pesada[16] = "0";
                    }
                    if (primera_pesada[17] == " ")
                    {
                        primera_pesada[17] = "0";
                    }





                    SqlCommand command2;
                    if (control_siib.valor_numerico == 0)
                    {
                        string query = "INSERT INTO [base_forrajes].[dbo].[" + tabla_principal + "] ([ficha],[fecha], [placas], [user_id], [prov_cli], [producto], [chofer], [peso1], [peso2], [peso_t], [obs_p1], [obs_p2], [tmov], [termina], [folio], [remision], [cvemov], [aplica], [agrupada], [observacio], [cesp], [folio_inv], [rstatus], [rtime], [rtime_p1], [noreg], [olor], [matext_lib], [particula], [empaque], [color], [humedad], [caducidad], [pacas], [corte], [repase], [floracion5], [descolorida], [humedad18], [humedad12], [maleza], [prhoja], [d1tab], [d2tab], [d3tab], [d4tab], [d5tab], [d1pac], [d2pac], [d3pac], [d4pac], [d5pac], [peso_ori], [pozo], [p_matsec], [fic_prov], [maquilador], [sucursal],[propietario_camion],[destino],[predio_cliente], [Recibida],[tabla],[variedad],[ensilador]) VALUES (@ficha, @fecha, @placas, @user_id, @prov_cli, @producto, @chofer, @peso1, @peso2, @peso_t, @obs_p1, @obs_p2, @tmov, @termina, @folio, @remision, @cvemov, @aplica, @agrupada, @observacio, @cesp, @folio_inv, @rstatus, @rtime, @rtime_p1, @noreg, @olor, @matext_lib, @particula, @empaque, @color, @humedad, @caducidad, @pacas, @corte, @repase, @floracion5, @descolorida, @humedad18, @humedad12, @maleza, @prhoja, @d1tab, @d2tab, @d3tab, @d4tab, @d5tab, @d1pac, @d2pac, @d3pac, @d4pac, @d5pac, @peso_ori, @pozo, @p_matsec, @fic_prov, @maquilador, @sucursal, @propietario_camion, @destino,@predio_cliente,@recibida,@tabla,@variedad,@ensilador)";
                        command2 = new SqlCommand(query, connection);

                        command2.Parameters.AddWithValue("@recibida", 0);
                    }
                    else
                    {
                        command2 = new SqlCommand("usp_1_BASCULA_InsertFicha", connection);
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@terraplen", " ");
                        command2.Parameters.AddWithValue("@id_establo", primera_pesada[0]);
                        command2.Parameters.AddWithValue("@bunker", " ");
                        command2.Parameters.AddWithValue("@arcina", " ");
                        command2.Parameters.AddWithValue("@consumo", " ");
                        command2.Parameters.AddWithValue("@reimpresion", 0);
                        command2.Parameters.AddWithValue("@imp_p1", 0);
                        command2.Parameters.AddWithValue("@imp_p2", 0);
                        command2.Parameters.AddWithValue("@Recibida", 0);
                    }

                    DateTime fecha = DateTime.Now;
                    string fecha_sab = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                    DateTime fecha2 = DateTime.Now;
                    DateTime fecha_sin_hora = fecha2.Date;
                    string fecha_sab2 = fecha_sin_hora.ToString("yyyy-MM-dd HH:mm:ss");
                    command2.Parameters.AddWithValue("@ficha", primera_pesada[1]);
                    command2.Parameters.AddWithValue("@fecha", fecha_sab2);
                    command2.Parameters.AddWithValue("@placas", primera_pesada[12]);
                    command2.Parameters.AddWithValue("@user_id", usuario);
                    command2.Parameters.AddWithValue("@prov_cli", primera_pesada[3]);
                    command2.Parameters.AddWithValue("@producto", primera_pesada[4]);
                    command2.Parameters.AddWithValue("@chofer", primera_pesada[11]);
                    command2.Parameters.AddWithValue("@peso1", Convert.ToInt32(valor_ficha));
                    command2.Parameters.AddWithValue("@peso2", 0);
                    command2.Parameters.AddWithValue("@peso_t", 0);
                    command2.Parameters.AddWithValue("@obs_p1", primera_pesada[15]);
                    command2.Parameters.AddWithValue("@obs_p2", " ");
                    command2.Parameters.AddWithValue("@tmov", primera_pesada[6]);
                    command2.Parameters.AddWithValue("@termina", 0);
                    command2.Parameters.AddWithValue("@folio", primera_pesada[7]);
                    command2.Parameters.AddWithValue("@remision", " ");
                    command2.Parameters.AddWithValue("@cvemov", primera_pesada[5]);
                    command2.Parameters.AddWithValue("@aplica", 0);
                    command2.Parameters.AddWithValue("@agrupada", primera_pesada[18]);
                    command2.Parameters.AddWithValue("@observacio", " ");
                    command2.Parameters.AddWithValue("@cesp", "000");
                    command2.Parameters.AddWithValue("@folio_inv", " ");
                    command2.Parameters.AddWithValue("@rstatus", "M");
                    command2.Parameters.AddWithValue("@rtime", fecha_sab);
                    command2.Parameters.AddWithValue("@rtime_p1", fecha_sab);
                    command2.Parameters.AddWithValue("@noreg", 100003);
                    command2.Parameters.AddWithValue("@olor", 0);
                    command2.Parameters.AddWithValue("@matext_lib", 0);
                    command2.Parameters.AddWithValue("@particula", " ");
                    command2.Parameters.AddWithValue("@empaque", " ");
                    command2.Parameters.AddWithValue("@color", " ");
                    command2.Parameters.AddWithValue("@humedad", " ");
                    command2.Parameters.AddWithValue("@caducidad", " ");
                    command2.Parameters.AddWithValue("@pacas", primera_pesada[24]);
                    command2.Parameters.AddWithValue("@corte", primera_pesada[23]);
                    command2.Parameters.AddWithValue("@repase", 0);
                    command2.Parameters.AddWithValue("@floracion5", 0);
                    command2.Parameters.AddWithValue("@descolorida", 0);
                    command2.Parameters.AddWithValue("@humedad18", 0);
                    command2.Parameters.AddWithValue("@humedad12", 0);
                    command2.Parameters.AddWithValue("@maleza", 0);
                    command2.Parameters.AddWithValue("@prhoja", 0);
                    command2.Parameters.AddWithValue("@d1tab", " ");
                    command2.Parameters.AddWithValue("@d2tab", " ");
                    command2.Parameters.AddWithValue("@d3tab", " ");
                    command2.Parameters.AddWithValue("@d4tab", " ");
                    command2.Parameters.AddWithValue("@d5tab", " ");
                    command2.Parameters.AddWithValue("@d1pac", 0);
                    command2.Parameters.AddWithValue("@d2pac", 0);
                    command2.Parameters.AddWithValue("@d3pac", 0);
                    command2.Parameters.AddWithValue("@d4pac", 0);
                    command2.Parameters.AddWithValue("@d5pac", 0);

                    command2.Parameters.AddWithValue("@peso_ori", primera_pesada[16]);
                    command2.Parameters.AddWithValue("@p_matsec", primera_pesada[17]);
                    command2.Parameters.AddWithValue("@pozo", primera_pesada[2]);
                    command2.Parameters.AddWithValue("@fic_prov", " ");
                    command2.Parameters.AddWithValue("@maquilador", primera_pesada[13]);
                    command2.Parameters.AddWithValue("@sucursal", primera_pesada[8]);
                    command2.Parameters.AddWithValue("@propietario_camion", primera_pesada[10]);
                    command2.Parameters.AddWithValue("@destino", primera_pesada[9]);
                    command2.Parameters.AddWithValue("@predio_cliente", primera_pesada[14]);

                    command2.Parameters.AddWithValue("@tabla", primera_pesada[20]);
                    command2.Parameters.AddWithValue("@variedad", primera_pesada[21]);
                    command2.Parameters.AddWithValue("@ensilador", primera_pesada[22]);

                    command2.CommandTimeout = 10000000;
                    command2.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return false;
            }
        }

        public bool GuardarFichaSabSistemas(string[] primera_pesada, DateTime fecha_sab, string justificacion)
        {
            var control_siib = db.C_parametros_configuracion.Where(x => x.id_parametro_configuracion == 13).FirstOrDefault();
            //var control_siib = 0;
            string valor_ficha = new string(primera_pesada[19].Where(char.IsDigit).ToArray());
            var usuario = (int)Session["LoggedId"];
            var empleado = db.C_usuarios_corporativo.Find(usuario);
            var nombre = empleado.usuario;

            string tabla_principal = "";




            control_siib.valor_numerico = 1;




            if (control_siib.valor_numerico == 0)
            {
                switch (primera_pesada[0])
                {
                    case "1":
                        tabla_principal = "fichas_sg";
                        break;
                    case "2":
                        tabla_principal = "fichas_si";
                        break;
                    case "3":
                        tabla_principal = "fichas_sm";
                        break;
                    case "4":
                        tabla_principal = "fichas_tn";
                        break;
                }
            }
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    if (primera_pesada[16] == " ")
                    {
                        primera_pesada[16] = "0";
                    }
                    if (primera_pesada[17] == " ")
                    {
                        primera_pesada[17] = "0";
                    }





                    SqlCommand command2;
                    if (control_siib.valor_numerico == 0)
                    {
                        string query = "INSERT INTO [base_forrajes].[dbo].[" + tabla_principal + "] ([ficha],[fecha], [placas], [user_id], [prov_cli], [producto], [chofer], [peso1], [peso2], [peso_t], [obs_p1], [obs_p2], [tmov], [termina], [folio], [remision], [cvemov], [aplica], [agrupada], [observacio], [cesp], [folio_inv], [rstatus], [rtime], [rtime_p1], [noreg], [olor], [matext_lib], [particula], [empaque], [color], [humedad], [caducidad], [pacas], [corte], [repase], [floracion5], [descolorida], [humedad18], [humedad12], [maleza], [prhoja], [d1tab], [d2tab], [d3tab], [d4tab], [d5tab], [d1pac], [d2pac], [d3pac], [d4pac], [d5pac], [peso_ori], [pozo], [p_matsec], [fic_prov], [maquilador], [sucursal],[propietario_camion],[destino],[predio_cliente], [Recibida],[tabla],[variedad],[ensilador]) VALUES (@ficha, @fecha, @placas, @user_id, @prov_cli, @producto, @chofer, @peso1, @peso2, @peso_t, @obs_p1, @obs_p2, @tmov, @termina, @folio, @remision, @cvemov, @aplica, @agrupada, @observacio, @cesp, @folio_inv, @rstatus, @rtime, @rtime_p1, @noreg, @olor, @matext_lib, @particula, @empaque, @color, @humedad, @caducidad, @pacas, @corte, @repase, @floracion5, @descolorida, @humedad18, @humedad12, @maleza, @prhoja, @d1tab, @d2tab, @d3tab, @d4tab, @d5tab, @d1pac, @d2pac, @d3pac, @d4pac, @d5pac, @peso_ori, @pozo, @p_matsec, @fic_prov, @maquilador, @sucursal, @propietario_camion, @destino,@predio_cliente,@recibida,@tabla,@variedad,@ensilador)";
                        command2 = new SqlCommand(query, connection);

                        command2.Parameters.AddWithValue("@recibida", 0);
                    }
                    else
                    {
                        command2 = new SqlCommand("usp_1_BASCULA_InsertFicha", connection);
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@terraplen", " ");
                        command2.Parameters.AddWithValue("@id_establo", primera_pesada[0]);
                        command2.Parameters.AddWithValue("@bunker", " ");
                        command2.Parameters.AddWithValue("@arcina", " ");
                        command2.Parameters.AddWithValue("@consumo", " ");
                        command2.Parameters.AddWithValue("@reimpresion", 0);
                        command2.Parameters.AddWithValue("@imp_p1", 0);
                        command2.Parameters.AddWithValue("@imp_p2", 0);
                        command2.Parameters.AddWithValue("@Recibida", 0);
                    }

                    DateTime fecha = DateTime.Now;
                    //string fecha_sab = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                    DateTime fecha2 = DateTime.Now;
                    DateTime fecha_sin_hora = fecha2.Date;
                    string fecha_sab2 = fecha_sin_hora.ToString("yyyy-MM-dd HH:mm:ss");
                    command2.Parameters.AddWithValue("@ficha", primera_pesada[1]);
                    command2.Parameters.AddWithValue("@fecha", fecha_sab2);
                    command2.Parameters.AddWithValue("@placas", primera_pesada[12]);
                    command2.Parameters.AddWithValue("@user_id", usuario);
                    command2.Parameters.AddWithValue("@prov_cli", primera_pesada[3]);
                    command2.Parameters.AddWithValue("@producto", primera_pesada[4]);
                    command2.Parameters.AddWithValue("@chofer", primera_pesada[11]);
                    command2.Parameters.AddWithValue("@peso1", Convert.ToInt32(valor_ficha));
                    command2.Parameters.AddWithValue("@peso2", 0);
                    command2.Parameters.AddWithValue("@peso_t", 0);
                    command2.Parameters.AddWithValue("@obs_p1", primera_pesada[15]);
                    command2.Parameters.AddWithValue("@obs_p2", " ");
                    command2.Parameters.AddWithValue("@tmov", primera_pesada[6]);
                    command2.Parameters.AddWithValue("@termina", 0);
                    command2.Parameters.AddWithValue("@folio", primera_pesada[7]);
                    command2.Parameters.AddWithValue("@remision", " ");
                    command2.Parameters.AddWithValue("@cvemov", primera_pesada[5]);
                    command2.Parameters.AddWithValue("@aplica", 0);
                    command2.Parameters.AddWithValue("@agrupada", primera_pesada[18]);
                    command2.Parameters.AddWithValue("@observacio", " ");
                    command2.Parameters.AddWithValue("@cesp", "000");
                    command2.Parameters.AddWithValue("@folio_inv", " ");
                    command2.Parameters.AddWithValue("@rstatus", "M");
                    command2.Parameters.AddWithValue("@rtime", fecha_sab);
                    command2.Parameters.AddWithValue("@rtime_p1", fecha_sab);
                    command2.Parameters.AddWithValue("@noreg", 100003);
                    command2.Parameters.AddWithValue("@olor", 0);
                    command2.Parameters.AddWithValue("@matext_lib", 0);
                    command2.Parameters.AddWithValue("@particula", " ");
                    command2.Parameters.AddWithValue("@empaque", " ");
                    command2.Parameters.AddWithValue("@color", " ");
                    command2.Parameters.AddWithValue("@humedad", " ");
                    command2.Parameters.AddWithValue("@caducidad", " ");
                    command2.Parameters.AddWithValue("@pacas", primera_pesada[24]);
                    command2.Parameters.AddWithValue("@corte", primera_pesada[23]);
                    command2.Parameters.AddWithValue("@repase", 0);
                    command2.Parameters.AddWithValue("@floracion5", 0);
                    command2.Parameters.AddWithValue("@descolorida", 0);
                    command2.Parameters.AddWithValue("@humedad18", 0);
                    command2.Parameters.AddWithValue("@humedad12", 0);
                    command2.Parameters.AddWithValue("@maleza", 0);
                    command2.Parameters.AddWithValue("@prhoja", 0);
                    command2.Parameters.AddWithValue("@d1tab", " ");
                    command2.Parameters.AddWithValue("@d2tab", " ");
                    command2.Parameters.AddWithValue("@d3tab", " ");
                    command2.Parameters.AddWithValue("@d4tab", " ");
                    command2.Parameters.AddWithValue("@d5tab", " ");
                    command2.Parameters.AddWithValue("@d1pac", 0);
                    command2.Parameters.AddWithValue("@d2pac", 0);
                    command2.Parameters.AddWithValue("@d3pac", 0);
                    command2.Parameters.AddWithValue("@d4pac", 0);
                    command2.Parameters.AddWithValue("@d5pac", 0);

                    command2.Parameters.AddWithValue("@peso_ori", primera_pesada[16]);
                    command2.Parameters.AddWithValue("@p_matsec", primera_pesada[17]);
                    command2.Parameters.AddWithValue("@pozo", primera_pesada[2]);
                    command2.Parameters.AddWithValue("@fic_prov", " ");
                    command2.Parameters.AddWithValue("@maquilador", primera_pesada[13]);
                    command2.Parameters.AddWithValue("@sucursal", primera_pesada[8]);
                    command2.Parameters.AddWithValue("@propietario_camion", primera_pesada[10]);
                    command2.Parameters.AddWithValue("@destino", primera_pesada[9]);
                    command2.Parameters.AddWithValue("@predio_cliente", primera_pesada[14]);

                    command2.Parameters.AddWithValue("@tabla", primera_pesada[20]);
                    command2.Parameters.AddWithValue("@variedad", primera_pesada[21]);
                    command2.Parameters.AddWithValue("@ensilador", primera_pesada[22]);

                    command2.CommandTimeout = 10000000;
                    command2.ExecuteNonQuery();



                    int id_usuario = (int)Session["LoggedId"];
                    try
                    {
                        UTILERIASController utileria = new UTILERIASController();
                        utileria.RegistroLogsSoporteGral(id_usuario, "Se edito la ficha: " + primera_pesada[1], justificacion);
                    }
                    catch (Exception) { }

                    return true;
                }
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return false;
            }
        }

        public PartialViewResult GenerarFichaPDF(int ficha, int establo, string folio, string placas, string chofer, string producto, decimal pesada1, decimal pesada2, decimal pesadat, string observaciones, string cliente)
        {
            int pesador_id = (int)Session["LoggedId"];
            string nombre = "";
            string fecha_peso1 = "";
            string fecha_peso2 = "";
            try
            {
                var empleado = db.C_usuarios_corporativo.Find(pesador_id);
                nombre = empleado.C_empleados.nombres + " " + empleado.C_empleados.apellido_paterno;

                //OBTENER FECHA
                using (var connection = Models.ConexionSab.Conectar())
                {
                    string fichas = ficha.ToString().PadLeft(25, ' ');
                    //1 SG, 2 SI, 3 SM
                    string query = "";
                    switch (establo)
                    {
                        case 1:
                            query = "SELECT fich.rtime_p1, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_sg] fich where ficha = '" + fichas + "'";
                            break;
                        case 2:
                            query = "SELECT fich.rtime_p1, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_si] fich where ficha = '" + fichas + "'";
                            break;
                        case 3:
                            query = "SELECT fich.rtime_p1, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_sm] fich where ficha = '" + fichas + "'";
                            break;
                        case 4:
                            query = "SELECT fich.rtime_p1, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_tn] fich where ficha = '" + fichas + "'";
                            break;
                    }
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            fecha_peso1 = reader[0].ToString();
                            fecha_peso2 = reader[1].ToString();
                        }
                        else
                        {
                            fecha_peso1 = "";
                            fecha_peso2 = "";
                        }
                    }
                    else
                    {
                        fecha_peso1 = "";
                        fecha_peso2 = "";
                    }
                }
                //


            }
            catch (Exception)
            {

            }
            db.SaveChanges();
            BasculaSab model = new BasculaSab(establo, ficha, folio, fecha_peso1, fecha_peso2, placas, chofer, producto, pesada1, pesada2, pesadat, nombre, observaciones, cliente);
            return PartialView("../Establos/BasculaSab/_PDFBascula", new List<BasculaSab> { model });
        }

        public PartialViewResult ReimprecionFichaPDF(int fichas, int id_establo)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                string ficha = fichas.ToString().PadLeft(25, ' ');
                //1 SG, 2 SI, 3 SM
                string query = "";
                switch (id_establo)
                {
                    case 1:
                        query = "SELECT fich.folio, fich.rtime_p1, fich.placas, fich.chofer, art.desart, peso1, peso2, peso_t, " +
                            "fich.user_id, fich.obs_p1, proved.despro, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_sg] fich " +
                            "join [base_forrajes].[dbo].[articulo] art on fich.producto = art.[cveart] " +
                            "join [base_forrajes].[dbo].[proveedo] proved on fich.prov_cli = proved.cvepro where ficha = '" + ficha + "'";
                        break;
                    case 2:
                        query = "SELECT fich.folio, fich.rtime_p1, fich.placas, fich.chofer, art.desart, peso1, peso2, peso_t, " +
                                                    "fich.user_id, fich.obs_p1, proved.despro, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_si] fich " +
                                                    "join [base_forrajes].[dbo].[articulo] art on fich.producto = art.[cveart] " +
                                                    "join [base_forrajes].[dbo].[proveedo] proved on fich.prov_cli = proved.cvepro where ficha = '" + ficha + "'";
                        break;
                    case 3:
                        query = "SELECT fich.folio, fich.rtime_p1, fich.placas, fich.chofer, art.desart, peso1, peso2, peso_t, " +
                                                    "fich.user_id, fich.obs_p1, proved.despro, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_sm] fich " +
                                                    "join [base_forrajes].[dbo].[articulo] art on fich.producto = art.[cveart] " +
                                                    "join [base_forrajes].[dbo].[Proveedo_B] proved on fich.prov_cli = proved.cvepro where ficha = '" + ficha + "'";
                        break;
                    case 4:
                        query = "SELECT fich.folio, fich.rtime_p1, fich.placas, fich.chofer, art.desart, peso1, peso2, peso_t, " +
                                                    "fich.user_id, fich.obs_p1, proved.despro, fich.rtime_p2 FROM [base_forrajes].[dbo].[fichas_tn] fich " +
                                                    "join [base_forrajes].[dbo].[articulo] art on fich.producto = art.[cveart] " +
                                                    "join [base_forrajes].[dbo].[proveedo] proved on fich.prov_cli = proved.cvepro where ficha = '" + ficha + "'";
                        break;
                }
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        decimal p1 = Convert.ToDecimal(reader[5]);
                        decimal p2 = Convert.ToDecimal(reader[6]);
                        decimal p3 = Convert.ToDecimal(reader[7]);
                        int usuario_id = 0;
                        string nombre = "";
                        try
                        {
                            usuario_id = Convert.ToInt32(reader[8].ToString());
                            var usuario = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == usuario_id).FirstOrDefault();
                            nombre = usuario.C_empleados.nombres.ToString() + " " + usuario.C_empleados.apellido_paterno.ToString();
                        }
                        catch (Exception)
                        {

                        }
                        BasculaSab model = new BasculaSab(id_establo, fichas, reader[0].ToString(), reader[1].ToString(), reader[11].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), p1, p2, p3, nombre, reader[9].ToString(), reader[10].ToString());
                        return PartialView("../Establos/BasculaSab/_PDFBascula", new List<BasculaSab> { model });
                    }
                    else { return null; }
                }
                else
                {
                    return null;
                }
            }
        }


        public string ConsultarInfoFichaSab(int fichas, int id_establo)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                string ficha = fichas.ToString().PadLeft(25, ' ');
                //1 SG, 2 SI, 3 SM
                string query = "";
                switch (id_establo)
                {
                    case 1:
                        query = "SELECT fichas.pozo, prov.despro, art.desart, fichas.cvemov, tmov, folio, sucursal, destino, " +
                            "fichas.propietario_camion, chofer, placas, maquilador, predio_cliente, obs_p1, peso_ori, p_matsec, agrupada, peso1, " +
                            "fichas.tabla, fichas.variedad, fichas.corte, fichas.pacas, fichas.ensilador " +
                            "FROM [base_forrajes].[dbo].[fichas_sg] fichas " +
                            "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                            "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where  termina = 0 and fecha >= '2024-01-01' and " +
                            "ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                        break;
                    case 2:
                        query = "SELECT fichas.pozo, prov.despro, art.desart, fichas.cvemov, tmov, folio, sucursal, destino, " +
                                   "fichas.propietario_camion, chofer, placas, maquilador, predio_cliente, obs_p1, peso_ori, p_matsec, agrupada, peso1, " +
                                   "fichas.tabla, fichas.variedad, fichas.corte, fichas.pacas, fichas.ensilador " +
                                   "FROM [base_forrajes].[dbo].[fichas_si] fichas " +
                                   "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                   "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where  termina = 0 and fecha >= '2024-01-01' and " +
                                   "ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                        break;
                    case 3:
                        query = "SELECT fichas.pozo, prov.despro, art.desart, fichas.cvemov, tmov, folio, sucursal, destino, " +
                                   "fichas.propietario_camion, chofer, placas, maquilador, predio_cliente, obs_p1, peso_ori, p_matsec, agrupada, peso1, " +
                                   "fichas.tabla, fichas.variedad, fichas.corte, fichas.pacas, fichas.ensilador " +
                                   "FROM [base_forrajes].[dbo].[fichas_sm] fichas " +
                                   "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                   "left join [base_forrajes].[dbo].[proveedo_B] prov on fichas.prov_cli = prov.cvepro where  termina = 0 and fecha >= '2024-01-01' and " +
                                   "ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                        break;
                    case 4:
                        query = "SELECT fichas.pozo, prov.despro, art.desart, fichas.cvemov, tmov, folio, sucursal, destino, " +
                                   "fichas.propietario_camion, chofer, placas, maquilador, predio_cliente, obs_p1, peso_ori, p_matsec, agrupada, peso1, " +
                                   "fichas.tabla, fichas.variedad, fichas.corte, fichas.pacas, fichas.ensilador " +
                                   "FROM [base_forrajes].[dbo].[fichas_tn] fichas " +
                                   "left join [base_forrajes].[dbo].[articulo] art on fichas.producto = art.cveart " +
                                   "left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro where  termina = 0 and fecha >= '2024-01-01' and " +
                                   "ficha = '" + ficha + "' and fichas.rstatus != 'E'";
                        break;
                }
                ConexionSab.Articulos_Sab articulos_Siib = new ConexionSab.Articulos_Sab();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        var fichaInfo = new
                        {
                            Pozo = reader[0].ToString(),
                            ProvCli = reader[1].ToString(),
                            Producto = reader[2].ToString(),
                            CveMov = reader[3].ToString(),
                            TMov = reader["tmov"].ToString(),
                            Folio = reader["folio"].ToString(),
                            Sucursal = reader["sucursal"].ToString(),
                            Destino = reader["destino"].ToString(),
                            PropietarioCamion = reader[8].ToString(),
                            Chofer = reader["chofer"].ToString(),
                            Placas = reader["placas"].ToString(),
                            Maquilador = reader["maquilador"].ToString(),
                            PredioCliente = reader["predio_cliente"].ToString(),
                            ObsP1 = reader["obs_p1"].ToString(),
                            PesoOri = reader["peso_ori"].ToString(),
                            PMatSec = reader["p_matsec"].ToString(),
                            Agrupada = reader["agrupada"].ToString(),
                            Peso1 = reader["peso1"].ToString(),
                            Tabla = reader["tabla"].ToString(),
                            Variedad = reader["variedad"].ToString(),
                            Corte = reader["corte"].ToString(),
                            Pacas = reader["pacas"].ToString(),
                            Ensilador = reader["ensilador"].ToString()
                        };
                        string resultJson = JsonConvert.SerializeObject(fichaInfo);
                        return resultJson;
                    }
                    else { return "No se encontro informacion de la ficha"; }
                }
                else
                {
                    return "No se encontro informacion de la ficha";
                }
            }
        }

        public string GuardarSegundaPesadaSab(int fichas, string peso_2, string peso_t, int id_establo)
        {
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    using (var command = new SqlCommand())
                    {
                        string valor_ficha = new string(peso_2.Where(char.IsDigit).ToArray());
                        string ficha = fichas.ToString().PadLeft(25, ' ');
                        DateTime fecha = DateTime.Now;
                        string fecha_sab = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                        command.Connection = connection;
                        switch (id_establo)
                        {
                            case 1:
                                command.CommandText = "UPDATE [dbo].[fichas_sg] SET [peso2] = '" + valor_ficha + "',[peso_t] = '" + peso_t + "', [termina] = '1', [rtime_p2] = '" + fecha_sab + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case 2:
                                command.CommandText = "UPDATE [dbo].[fichas_si] SET [peso2] = '" + valor_ficha + "',[peso_t] = '" + peso_t + "', [termina] = '1', [rtime_p2] = '" + fecha_sab + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case 3:
                                command.CommandText = "UPDATE [dbo].[fichas_sm] SET [peso2] = '" + valor_ficha + "',[peso_t] = '" + peso_t + "', [termina] = '1', [rtime_p2] = '" + fecha_sab + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case 4:
                                command.CommandText = "UPDATE [dbo].[fichas_tn] SET [peso2] = '" + valor_ficha + "',[peso_t] = '" + peso_t + "', [termina] = '1', [rtime_p2] = '" + fecha_sab + "' WHERE ficha = '" + ficha + "'";
                                break;
                        }
                        command.ExecuteNonQuery();
                        return "Exito";
                    }
                }
            }
            catch (Exception)
            {
                return "No se encontró la ficha. Asegurese de no modificar el campo una vez se haya buscado";
            }
        }

        public PartialViewResult ConsultarFichasReporteSAB(int id_establo, string fecha_inicio, string fecha_fin)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                string query = ObtenerQuery(id_establo, fecha_inicio, fecha_fin);

                if (string.IsNullOrEmpty(query))
                {
                    return null;
                }

                SqlCommand command = new SqlCommand(query, connection)
                {
                    CommandTimeout = 100000
                };
                SqlDataReader reader = command.ExecuteReader();

                if (!reader.HasRows)
                {
                    return null;
                }

                var captura = new List<ReporteFichasBascula>();

                int establoIDIndex = reader.GetOrdinal("IDESTABLO");
                int establoIndex = reader.GetOrdinal("Establo");
                int fichaIndex = reader.GetOrdinal("Ficha");
                int folioIndex = reader.GetOrdinal("Folio");
                int articuloIndex = reader.GetOrdinal("Articulo");
                int clienteIndex = reader.GetOrdinal("Cliente");
                int fecha1Index = reader.GetOrdinal("Fecha1");
                int fecha2Index = reader.GetOrdinal("Fecha2");
                int lineaIndex = reader.GetOrdinal("Linea Transportista");
                int choferIndex = reader.GetOrdinal("Chofer");
                int placasIndex = reader.GetOrdinal("Placas");
                int peso1Index = reader.GetOrdinal("Peso1");
                int peso2Index = reader.GetOrdinal("Peso2");
                int pesoTIndex = reader.GetOrdinal("PesoT");
                int materiaIndex = reader.GetOrdinal("Mat. Seca");
                int maquiladorIndex = reader.GetOrdinal("Maquilador");
                int pozoIndex = reader.GetOrdinal("Pozo");
                int observacionIndex = reader.GetOrdinal("Observacion");
                int movimientoIndex = reader.GetOrdinal("Movimiento");
                int tMovimientoIndex = reader.GetOrdinal("Tipo Movimiento");
                int origenIndex = reader.GetOrdinal("Peso Origen");
                int tablaIndex = reader.GetOrdinal("Tabla");
                int variedadIndex = reader.GetOrdinal("Variedad");
                int corteIndex = reader.GetOrdinal("Corte");
                int pacasIndex = reader.GetOrdinal("Pacas");
                int ensiladorIndex = reader.GetOrdinal("Ensilador");

                while (reader.Read())
                {
                    var fichasSab = new ReporteFichasBascula
                    {
                        IDEstablo = Convert.ToInt32(reader[establoIDIndex]),
                        Establo = reader[establoIndex].ToString(),
                        Ficha = reader[fichaIndex].ToString(),
                        Folio = reader[folioIndex].ToString(),
                        Articulo = reader[articuloIndex].ToString(),
                        Cliente = reader[clienteIndex].ToString(),
                        Fecha1 = Convert.ToDateTime(reader[fecha1Index]),
                        Fecha2 = reader.IsDBNull(fecha2Index) ? DateTime.MinValue : Convert.ToDateTime(reader[fecha2Index]),
                        Linea = reader[lineaIndex].ToString(),
                        Chofer = reader[choferIndex].ToString(),
                        Placas = reader[placasIndex].ToString(),
                        P1 = reader.IsDBNull(peso1Index) ? 0 : Convert.ToDecimal(reader[peso1Index]),
                        P2 = reader.IsDBNull(peso2Index) ? 0 : Convert.ToDecimal(reader[peso2Index]),
                        PT = reader.IsDBNull(pesoTIndex) ? 0 : Convert.ToDecimal(reader[pesoTIndex]),
                        Materia = reader.IsDBNull(materiaIndex) ? 0 : Convert.ToDecimal(reader[materiaIndex]),
                        Maquilador = reader[maquiladorIndex].ToString(),
                        Pozo = reader[pozoIndex].ToString(),
                        Observacion = reader[observacionIndex].ToString(),
                        Movimiento = reader[movimientoIndex].ToString(),
                        TMovimiento = reader[tMovimientoIndex].ToString(),
                        Origen = reader.IsDBNull(origenIndex) ? 0 : Convert.ToDecimal(reader[origenIndex]),
                        Tabla = reader[tablaIndex].ToString().Replace(" ", ""),
                        Variedad = reader[variedadIndex].ToString(),
                        Corte = reader.IsDBNull(corteIndex) ? 0 : Convert.ToDecimal(reader[corteIndex]),
                        Pacas = reader.IsDBNull(pacasIndex) ? 0 : Convert.ToDecimal(reader[pacasIndex]),
                        Ensilador = reader[ensiladorIndex].ToString()
                    };

                    captura.Add(fichasSab);
                }

                return PartialView("../Establos/BasculaSab/_FichasReporte", captura);
            }
        }

        public ActionResult ExportarFichasBasculaExcel(int id_establo, string fecha_inicio, string fecha_fin)
        {
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    string query = ObtenerQuery(id_establo, fecha_inicio, fecha_fin);

                    if (string.IsNullOrEmpty(query))
                    {
                        return null;
                    }

                    SqlCommand command = new SqlCommand(query, connection)
                    {
                        CommandTimeout = 100000
                    };
                    SqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        return null;
                    }

                    var captura = new List<ReporteFichasBascula>();

                    int establoIDIndex = reader.GetOrdinal("IDESTABLO");
                    int establoIndex = reader.GetOrdinal("Establo");
                    int fichaIndex = reader.GetOrdinal("Ficha");
                    int folioIndex = reader.GetOrdinal("Folio");
                    int articuloIndex = reader.GetOrdinal("Articulo");
                    int clienteIndex = reader.GetOrdinal("Cliente");
                    int fecha1Index = reader.GetOrdinal("Fecha1");
                    int fecha2Index = reader.GetOrdinal("Fecha2");
                    int lineaIndex = reader.GetOrdinal("Linea Transportista");
                    int choferIndex = reader.GetOrdinal("Chofer");
                    int placasIndex = reader.GetOrdinal("Placas");
                    int peso1Index = reader.GetOrdinal("Peso1");
                    int peso2Index = reader.GetOrdinal("Peso2");
                    int pesoTIndex = reader.GetOrdinal("PesoT");
                    int materiaIndex = reader.GetOrdinal("Mat. Seca");
                    int maquiladorIndex = reader.GetOrdinal("Maquilador");
                    int pozoIndex = reader.GetOrdinal("Pozo");
                    int observacionIndex = reader.GetOrdinal("Observacion");
                    int movimientoIndex = reader.GetOrdinal("Movimiento");
                    int tMovimientoIndex = reader.GetOrdinal("Tipo Movimiento");
                    int origenIndex = reader.GetOrdinal("Peso Origen");
                    int tablaIndex = reader.GetOrdinal("Tabla");
                    int variedadIndex = reader.GetOrdinal("Variedad");
                    int corteIndex = reader.GetOrdinal("Corte");
                    int pacasIndex = reader.GetOrdinal("Pacas");
                    int ensiladorIndex = reader.GetOrdinal("Ensilador");

                    while (reader.Read())
                    {
                        var fichasSab = new ReporteFichasBascula
                        {
                            IDEstablo = Convert.ToInt32(reader[establoIDIndex]),
                            Establo = reader[establoIndex].ToString(),
                            Ficha = reader[fichaIndex].ToString(),
                            Folio = reader[folioIndex].ToString(),
                            Articulo = reader[articuloIndex].ToString(),
                            Cliente = reader[clienteIndex].ToString(),
                            Fecha1 = Convert.ToDateTime(reader[fecha1Index]),
                            Fecha2 = reader.IsDBNull(fecha2Index) ? DateTime.MinValue : Convert.ToDateTime(reader[fecha2Index]),
                            Linea = reader[lineaIndex].ToString(),
                            Chofer = reader[choferIndex].ToString(),
                            Placas = reader[placasIndex].ToString(),
                            P1 = reader.IsDBNull(peso1Index) ? 0 : Convert.ToDecimal(reader[peso1Index]),
                            P2 = reader.IsDBNull(peso2Index) ? 0 : Convert.ToDecimal(reader[peso2Index]),
                            PT = reader.IsDBNull(pesoTIndex) ? 0 : Convert.ToDecimal(reader[pesoTIndex]),
                            Materia = reader.IsDBNull(materiaIndex) ? 0 : Convert.ToDecimal(reader[materiaIndex]),
                            Maquilador = reader[maquiladorIndex].ToString(),
                            Pozo = reader[pozoIndex].ToString(),
                            Observacion = reader[observacionIndex].ToString(),
                            Movimiento = reader[movimientoIndex].ToString(),
                            TMovimiento = reader[tMovimientoIndex].ToString(),
                            Origen = reader.IsDBNull(origenIndex) ? 0 : Convert.ToDecimal(reader[origenIndex]),
                            Tabla = reader[tablaIndex].ToString().Replace(" ", ""),
                            Variedad = reader[variedadIndex].ToString(),
                            Corte = reader.IsDBNull(corteIndex) ? 0 : Convert.ToDecimal(reader[corteIndex]),
                            Pacas = reader.IsDBNull(pacasIndex) ? 0 : Convert.ToDecimal(reader[pacasIndex]),
                            Ensilador = reader[ensiladorIndex].ToString()
                        };

                        captura.Add(fichasSab);
                    }


                    try
                    {
                        DateTime Fecha_inicio = Convert.ToDateTime(fecha_inicio);
                        DateTime Fecha_final = Convert.ToDateTime(fecha_fin);
                        Fecha_final = Fecha_final.AddHours(23).AddMinutes(59).AddSeconds(59);

                        var groupedIncidencias = captura.GroupBy(i => i.IDEstablo).ToList();

                        using (var workbook = new XLWorkbook())
                        {
                            if (groupedIncidencias.Any())
                            {
                                foreach (var group in groupedIncidencias)
                                {
                                    var worksheet = workbook.Worksheets.Add($"{group.First().Establo}");
                                    var currentRow = 1;
                                    var columnCount = 5;

                                    var tipoIncidencia = group.Key;

                                    // Ajustar el tamaño de 'headerRange' según el tipo de incidencia
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
                                            titleRange.Value = $"{group.First().Establo} - {Fecha_inicio.ToShortDateString()} AL {Fecha_final.ToShortDateString()}";
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
                                        worksheet.Cell(currentRow, 1).Value = item.Establo;
                                        worksheet.Cell(currentRow, 2).Value = item.Ficha;
                                        worksheet.Cell(currentRow, 3).Value = item.Folio;
                                        worksheet.Cell(currentRow, 4).Value = item.Articulo;
                                        worksheet.Cell(currentRow, 5).Value = item.Cliente;
                                        worksheet.Cell(currentRow, 6).Value = item.Fecha1;
                                        worksheet.Cell(currentRow, 7).Value = item.Fecha2;
                                        worksheet.Cell(currentRow, 8).Value = item.Linea;
                                        worksheet.Cell(currentRow, 9).Value = item.Chofer;
                                        worksheet.Cell(currentRow, 10).Value = item.Placas;
                                        worksheet.Cell(currentRow, 11).Value = item.P1;
                                        worksheet.Cell(currentRow, 12).Value = item.P2;
                                        worksheet.Cell(currentRow, 13).Value = item.PT;
                                        worksheet.Cell(currentRow, 14).Value = item.Materia;
                                        worksheet.Cell(currentRow, 15).Value = item.Maquilador;
                                        worksheet.Cell(currentRow, 16).Value = item.Pozo;
                                        worksheet.Cell(currentRow, 17).Value = item.Observacion;

                                        if (item.Movimiento == "E")
                                        {
                                            worksheet.Cell(currentRow, 18).Value = "Entrada";
                                        }
                                        else if (item.Movimiento == "S")
                                        {
                                            worksheet.Cell(currentRow, 18).Value = "Salida";
                                        }
                                        else
                                        {
                                            worksheet.Cell(currentRow, 18).Value = "Inventario";
                                        }

                                        switch (item.TMovimiento)
                                        {
                                            case "001":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Compra";
                                                break;
                                            case "002":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Transf.entre establos";
                                                break;
                                            case "003":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Transf.forraje / agricola";
                                                break;
                                            case "004":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Venta(salida de leche)";
                                                break;
                                            case "005":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Venta(salida de ganado)";
                                                break;
                                            case "006":
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "Inventario";
                                                break;
                                            default:
                                                worksheet.Cell(currentRow, 19).Value = item.TMovimiento + " " + "NA";
                                                break;
                                        }





                                        worksheet.Cell(currentRow, 20).Value = item.Origen;
                                        worksheet.Cell(currentRow, 21).Value = item.Tabla;
                                        worksheet.Cell(currentRow, 22).Value = item.Variedad;
                                        worksheet.Cell(currentRow, 23).Value = item.Corte;
                                        worksheet.Cell(currentRow, 24).Value = item.Pacas;
                                        worksheet.Cell(currentRow, 25).Value = item.Ensilador;
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
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        private string ObtenerQuery(int id_establo, string fecha_inicio, string fecha_fin)
        {
            if (id_establo == 0)
            {
                int id_usuario = (int)Session["LoggedId"];
                var establo_usuario = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true).Select(x => x.id_establo).ToArray();
                if (establo_usuario.Count() == 1)
                {
                    id_establo = (int)establo_usuario[0];
                }
            }
            fecha_fin = fecha_fin + " 23:59:59";
            string queryTemplate = "SELECT '{5}' as 'IDESTABLO','{4}' as 'Establo', fichas.ficha as 'Ficha', fichas.folio as 'Folio', art.desart as 'Articulo', prov.despro as 'Cliente', " +
                                   "fichas.rtime_p1 as 'Fecha1', fichas.rtime_p2 as 'Fecha2', fichas.propietario_camion as 'Linea Transportista', " +
                                   "fichas.chofer as 'Chofer', fichas.placas as 'Placas', fichas.peso1 as 'Peso1', fichas.peso2 as 'Peso2', " +
                                   "fichas.peso_t as 'PesoT', fichas.p_matsec as 'Mat. Seca', fichas.maquilador as 'Maquilador', " +
                                   "fichas.pozo as 'Pozo', fichas.obs_p1 as 'Observacion', fichas.tmov as 'Movimiento', fichas.cvemov as 'Tipo Movimiento', " +
                                   "fichas.peso_ori as 'Peso Origen', fichas.tabla as 'Tabla', fichas.variedad as 'Variedad', fichas.corte as 'Corte', fichas.pacas as 'Pacas', fichas.ensilador as 'Ensilador' " +
                                   "FROM {0} fichas " +
                                   "LEFT JOIN [base_forrajes].[dbo].[articulo] art ON fichas.producto = art.cveart " +
                                   "LEFT JOIN {1} prov ON fichas.prov_cli = prov.cvepro " +
                                   "WHERE fichas.rtime_p1 BETWEEN '{2}' AND '{3}' AND fichas.rstatus != 'E' ";

            if (id_establo == 0)
            {
                string query1 = string.Format(queryTemplate, "[base_forrajes].[dbo].[fichas_sg]", "[base_forrajes].[dbo].[proveedo]", fecha_inicio, fecha_fin, "Establo San Gabriel", 1);
                string query2 = string.Format(queryTemplate, "[base_forrajes].[dbo].[fichas_si]", "[base_forrajes].[dbo].[proveedo]", fecha_inicio, fecha_fin, "Crianza San Ignacio", 2);
                string query3 = string.Format(queryTemplate, "[base_forrajes].[dbo].[fichas_sm]", "[base_forrajes].[dbo].[proveedo_B]", fecha_inicio, fecha_fin, "Establo Santa Monica", 3);
                string query4 = string.Format(queryTemplate, "[base_forrajes].[dbo].[fichas_tn]", "[base_forrajes].[dbo].[proveedo]", fecha_inicio, fecha_fin, "Tanque Nuevo", 4);

                return query1 + " UNION ALL " + query2 + " UNION ALL " + query3 + " UNION ALL " + query4 + " ORDER BY 'Ficha' ASC";
            }

            string tableName;
            string providerTable;
            string establo;
            int establo_id;

            switch (id_establo)
            {
                case 1:
                    tableName = "[base_forrajes].[dbo].[fichas_sg]";
                    providerTable = "[base_forrajes].[dbo].[proveedo]";
                    establo = "Establo San Gabriel";
                    establo_id = 1;
                    break;
                case 2:
                    tableName = "[base_forrajes].[dbo].[fichas_si]";
                    providerTable = "[base_forrajes].[dbo].[proveedo]";
                    establo = "Crianza San Ignacio";
                    establo_id = 2;
                    break;
                case 3:
                    tableName = "[base_forrajes].[dbo].[fichas_sm]";
                    providerTable = "[base_forrajes].[dbo].[proveedo_B]";
                    establo = "Establo Santa Monica";
                    establo_id = 3;
                    break;
                case 4:
                    tableName = "[base_forrajes].[dbo].[fichas_tn]";
                    providerTable = "[base_forrajes].[dbo].[proveedo]";
                    establo = "Tanque Nuevo";
                    establo_id = 4;
                    break;
                default:
                    return null;
            }

            return string.Format(queryTemplate, tableName, providerTable, fecha_inicio, fecha_fin, establo, establo_id) + " ORDER BY 'Ficha' ASC";
        }





        public PartialViewResult FichaPendiente(int id_establo)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                //1 SG, 2 SI, 3 SM
                string query = "";
                switch (id_establo)
                {
                    case 1:
                        query = "SELECT fichas.ficha as 'Ficha', fichas.folio as 'Folio', art.desart as 'Articulo', prov.despro as 'Cliente'," +
                            "fichas.propietario_camion as 'Linea Transportista', fichas.chofer as 'Chofer', fichas.placas as 'Placas', " +
                            "fichas.peso1 as 'Peso1',fichas.rtime_p1 as 'Fecha1', fichas.maquilador as 'Maquilador', fichas.pozo as 'Pozo', " +
                            "fichas.obs_p1 as 'Observacion', fichas.tmov as 'Movimiento', fichas.cvemov as 'Tipo Movimiento' " +
                            "FROM [base_forrajes].[dbo].[fichas_sg] fichas left join [base_forrajes].[dbo].[articulo] art on " +
                            "fichas.producto = art.cveart left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro " +
                            "WHERE fichas.termina = '0' and fichas.rtime_p1 > '2024-01-01 00:00:00.000' and fichas.rstatus != 'E' ORDER BY fichas.ficha asc";
                        break;
                    case 2:
                        query = "SELECT fichas.ficha as 'Ficha', fichas.folio as 'Folio', art.desart as 'Articulo', prov.despro as 'Cliente'," +
                            "fichas.propietario_camion as 'Linea Transportista', fichas.chofer as 'Chofer', fichas.placas as 'Placas', " +
                            "fichas.peso1 as 'Peso1',fichas.rtime_p1 as 'Fecha1', fichas.maquilador as 'Maquilador', fichas.pozo as 'Pozo', " +
                            "fichas.obs_p1 as 'Observacion', fichas.tmov as 'Movimiento', fichas.cvemov as 'Tipo Movimiento' " +
                            "FROM [base_forrajes].[dbo].[fichas_si] fichas left join [base_forrajes].[dbo].[articulo] art on " +
                            "fichas.producto = art.cveart left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro " +
                            "WHERE fichas.termina = '0' and fichas.rtime_p1 > '2024-01-01 00:00:00.000' and fichas.rstatus != 'E' ORDER BY fichas.ficha asc";
                        break;
                    case 3:
                        query = "SELECT fichas.ficha as 'Ficha', fichas.folio as 'Folio', art.desart as 'Articulo', prov.despro as 'Cliente'," +
                            "fichas.propietario_camion as 'Linea Transportista', fichas.chofer as 'Chofer', fichas.placas as 'Placas', " +
                            "fichas.peso1 as 'Peso1',fichas.rtime_p1 as 'Fecha1', fichas.maquilador as 'Maquilador', fichas.pozo as 'Pozo', " +
                            "fichas.obs_p1 as 'Observacion', fichas.tmov as 'Movimiento', fichas.cvemov as 'Tipo Movimiento' " +
                            "FROM [base_forrajes].[dbo].[fichas_sm] fichas left join [base_forrajes].[dbo].[articulo] art on " +
                            "fichas.producto = art.cveart left join [base_forrajes].[dbo].[proveedo_B] prov on fichas.prov_cli = prov.cvepro " +
                            "WHERE fichas.termina = '0' and fichas.rtime_p1 > '2024-01-01 00:00:00.000' and fichas.rstatus != 'E' ORDER BY fichas.ficha asc";
                        break;
                    case 4:
                        query = "SELECT fichas.ficha as 'Ficha', fichas.folio as 'Folio', art.desart as 'Articulo', prov.despro as 'Cliente'," +
                            "fichas.propietario_camion as 'Linea Transportista', fichas.chofer as 'Chofer', fichas.placas as 'Placas', " +
                            "fichas.peso1 as 'Peso1',fichas.rtime_p1 as 'Fecha1', fichas.maquilador as 'Maquilador', fichas.pozo as 'Pozo', " +
                            "fichas.obs_p1 as 'Observacion', fichas.tmov as 'Movimiento', fichas.cvemov as 'Tipo Movimiento' " +
                            "FROM [base_forrajes].[dbo].[fichas_tn] fichas left join [base_forrajes].[dbo].[articulo] art on " +
                            "fichas.producto = art.cveart left join [base_forrajes].[dbo].[proveedo] prov on fichas.prov_cli = prov.cvepro " +
                            "WHERE fichas.termina = '0' and fichas.rtime_p1 > '2024-01-01 00:00:00.000' and fichas.rstatus != 'E' ORDER BY fichas.ficha asc";
                        break;
                }
                SqlCommand command = new SqlCommand(query, connection);
                command.CommandTimeout = 100000;
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    FichasPendienteSab fichasSab = new FichasPendienteSab();
                    while (reader.Read())
                    {
                        fichasSab.Fichas.Add(reader[0].ToString());
                        fichasSab.Folios.Add(reader[1].ToString());
                        fichasSab.Articulos.Add(reader[2].ToString());
                        fichasSab.Cliente.Add(reader[3].ToString());
                        fichasSab.LineaTransportista.Add(reader[4].ToString());
                        fichasSab.Chofer.Add(reader[5].ToString());
                        fichasSab.Placas.Add(reader[6].ToString());
                        fichasSab.Peso1.Add(reader[7].ToString());
                        fichasSab.Fecha1.Add(reader[8].ToString());
                        fichasSab.Maquilador.Add(reader[9].ToString());
                        fichasSab.Pozo.Add(reader[10].ToString());
                        fichasSab.Observacion.Add(reader[11].ToString());
                        fichasSab.Movimiento.Add(reader[12].ToString());
                        fichasSab.TipoMovimiento.Add(reader[13].ToString());
                    }
                    string resultJson = JsonConvert.SerializeObject(fichasSab);
                    return PartialView("../Establos/BasculaSab/_FichasPendienteSab", fichasSab);
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public string ConsultarInfoFichaSabEditar(int fichas, int id_establo)
        {
            using (var connection = Models.ConexionSab.Conectar())
            {
                string ficha = fichas.ToString().PadLeft(25, ' ');
                //1 SG, 2 SI, 3 SM
                string query = "";
                switch (id_establo)
                {
                    case 1:
                        query = "SELECT placas, prov_cli, producto ,chofer ,peso1 ,peso2 ,peso_t ,obs_p1 ,tmov ,termina ,folio ,cvemov ,agrupada ,rtime_p1 ,rtime_p2 ,peso_ori ,p_matsec ,pozo ,maquilador ,sucursal ,propietario_camion ,destino ,predio_cliente, tabla, variedad, corte, pacas, ensilador  " +
                            "FROM [base_forrajes].[dbo].[fichas_sg] where ficha = '" + ficha + "';";
                        break;
                    case 2:
                        query = "SELECT placas, prov_cli, producto ,chofer ,peso1 ,peso2 ,peso_t ,obs_p1 ,tmov ,termina ,folio ,cvemov ,agrupada ,rtime_p1 ,rtime_p2 ,peso_ori ,p_matsec ,pozo ,maquilador ,sucursal ,propietario_camion ,destino ,predio_cliente, tabla, variedad, corte, pacas, ensilador " +
                            "FROM [base_forrajes].[dbo].[fichas_si] where ficha = '" + ficha + "';";
                        break;
                    case 3:
                        query = "SELECT placas, prov_cli, producto ,chofer ,peso1 ,peso2 ,peso_t ,obs_p1 ,tmov ,termina ,folio ,cvemov ,agrupada ,rtime_p1 ,rtime_p2 ,peso_ori ,p_matsec ,pozo ,maquilador ,sucursal ,propietario_camion ,destino ,predio_cliente, tabla, variedad, corte, pacas, ensilador " +
                            "FROM [base_forrajes].[dbo].[fichas_sm] where ficha = '" + ficha + "';";
                        break;
                    case 4:
                        query = "SELECT placas, prov_cli, producto ,chofer ,peso1 ,peso2 ,peso_t ,obs_p1 ,tmov ,termina ,folio ,cvemov ,agrupada ,rtime_p1 ,rtime_p2 ,peso_ori ,p_matsec ,pozo ,maquilador ,sucursal ,propietario_camion ,destino ,predio_cliente, tabla, variedad, corte, pacas, ensilador " +
                            "FROM [base_forrajes].[dbo].[fichas_tn] where ficha = '" + ficha + "';";
                        break;
                }
                ConexionSab.Articulos_Sab articulos_Siib = new ConexionSab.Articulos_Sab();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        var fichaInfo = new
                        {
                            Placas = reader[0].ToString(),
                            Prov_cli = reader[1].ToString(),
                            Producto = reader[2].ToString(),
                            Chofer = reader[3].ToString(),
                            Peso1 = reader[4].ToString(),
                            Peso2 = reader[5].ToString(),
                            Peso_t = reader[6].ToString(),
                            Obs_p1 = reader[7].ToString(),
                            Tmov = reader[8].ToString(),
                            Termina = reader[9].ToString(),
                            Folio = reader[10].ToString(),
                            Cvemov = reader[11].ToString(),
                            Agrupada = reader[12].ToString(),
                            Rtime_p1 = reader[13].ToString(),
                            Rtime_p2 = reader[14].ToString(),
                            Peso_ori = reader[15].ToString(),
                            P_matsec = reader[16].ToString(),
                            Pozo = reader[17].ToString(),
                            Maquilador = reader[18].ToString(),
                            Sucursal = reader[19].ToString(),
                            Propietario_camion = reader[20].ToString(),
                            Destino = reader[21].ToString(),
                            Predio_cliente = reader[22].ToString(),
                            Tabla = reader[23].ToString(),
                            Variedad = reader[24].ToString(),
                            Corte = reader[25].ToString(),
                            Pacas = reader[26].ToString(),
                            Ensilador = reader[27].ToString()

                            //, tabla, variedad, corte, pacas, ensilador
                        };
                        string resultJson = JsonConvert.SerializeObject(fichaInfo);
                        return resultJson;
                    }
                    else { return "No se encontro informacion de la ficha"; }
                }
                else
                {
                    return "No se encontro informacion de la ficha";
                }
            }
        }

        public bool GuardarEdicionFichaSab(string[] primera_pesada, string justificacion)
        {
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    using (var command = new SqlCommand())
                    {
                        string ficha = primera_pesada[1].ToString().Trim().PadLeft(25, ' ');
                        command.Connection = connection;

                        string fechas_sin = Convert.ToDateTime(primera_pesada[22]).ToString("yyyy-MM-dd");
                        string fechas_con_1 = Convert.ToDateTime(primera_pesada[22]).ToString("yyyy-MM-dd HH:mm:ss");
                        string fechas_con_2 = "";
                        if (primera_pesada[23] != "" && primera_pesada[23] != null)
                        {
                            fechas_con_2 = Convert.ToDateTime(primera_pesada[23]).ToString("yyyy-MM-dd HH:mm:ss");
                        }

                        int termina = 0;
                        if (primera_pesada[20] != "0" && primera_pesada[20] != "" && primera_pesada[20] != "NaN")
                        {
                            termina = 1;
                        }
                        else
                        {
                            primera_pesada[20] = "0";
                            primera_pesada[21] = "0";
                            fechas_con_2 = "2024-01-01 08:30:00.000";
                        }
                        switch (primera_pesada[0])
                        {
                            case "1":

                                command.CommandText = "update [base_forrajes].[dbo].[fichas_sg] set [pozo] = '" + primera_pesada[2] + "', [prov_cli] = '" + primera_pesada[3] + "', [producto] = '" + primera_pesada[4] + "', " +
                                    "[cvemov] = '" + primera_pesada[5] + "', [tmov] = '" + primera_pesada[6] + "', [folio] = '" + primera_pesada[7] + "', [sucursal] = '" + primera_pesada[8] + "', " +
                                    "[destino] = '" + primera_pesada[9] + "', [propietario_camion] = '" + primera_pesada[10] + "', [chofer] = '" + primera_pesada[11] + "', [placas] = '" + primera_pesada[12] + "', " +
                                    "[maquilador] = '" + primera_pesada[13] + "', predio_cliente = '" + primera_pesada[14] + "', [obs_p1] = '" + primera_pesada[15] + "', [peso_ori] = '" + primera_pesada[16] + "', " +
                                    "[p_matsec] = '" + primera_pesada[17] + "', [agrupada] = '" + primera_pesada[18] + "', [peso1] = '" + primera_pesada[19] + "', [peso2] = '" + primera_pesada[20] + "', " +
                                    "[peso_t] = '" + primera_pesada[21] + "', [rtime] = '" + fechas_sin + "', [rtime_p1] = '" + fechas_con_1 + "', [rtime_p2] = '" + fechas_con_2 + "', " +
                                    "[termina] = '" + termina + "', [tabla] = '" + primera_pesada[24] + "', [variedad] = '" + primera_pesada[25] + "', [ensilador] = '" + primera_pesada[26] + "', [corte] = '" + primera_pesada[27] + "', [pacas] = '" + primera_pesada[28] + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case "2":
                                command.CommandText = "update [base_forrajes].[dbo].[fichas_si] set [pozo] = '" + primera_pesada[2] + "', [prov_cli] = '" + primera_pesada[3] + "', [producto] = '" + primera_pesada[4] + "', " +
                                    "[cvemov] = '" + primera_pesada[5] + "', [tmov] = '" + primera_pesada[6] + "', [folio] = '" + primera_pesada[7] + "', [sucursal] = '" + primera_pesada[8] + "', " +
                                    "[destino] = '" + primera_pesada[9] + "', [propietario_camion] = '" + primera_pesada[10] + "', [chofer] = '" + primera_pesada[11] + "', [placas] = '" + primera_pesada[12] + "', " +
                                    "[maquilador] = '" + primera_pesada[13] + "', predio_cliente = '" + primera_pesada[14] + "', [obs_p1] = '" + primera_pesada[15] + "', [peso_ori] = '" + primera_pesada[16] + "', " +
                                    "[p_matsec] = '" + primera_pesada[17] + "', [agrupada] = '" + primera_pesada[18] + "', [peso1] = '" + primera_pesada[19] + "', [peso2] = '" + primera_pesada[20] + "', " +
                                    "[peso_t] = '" + primera_pesada[21] + "', [rtime] = '" + fechas_sin + "', [rtime_p1] = '" + fechas_con_1 + "', [rtime_p2] = '" + fechas_con_2 + "', " +
                                    "[termina] = '" + termina + "', [tabla] = '" + primera_pesada[24] + "', [variedad] = '" + primera_pesada[25] + "', [ensilador] = '" + primera_pesada[26] + "', [corte] = '" + primera_pesada[27] + "', [pacas] = '" + primera_pesada[28] + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case "3":
                                command.CommandText = "update [base_forrajes].[dbo].[fichas_sm] set [pozo] = '" + primera_pesada[2] + "', [prov_cli] = '" + primera_pesada[3] + "', [producto] = '" + primera_pesada[4] + "', " +
                                    "[cvemov] = '" + primera_pesada[5] + "', [tmov] = '" + primera_pesada[6] + "', [folio] = '" + primera_pesada[7] + "', [sucursal] = '" + primera_pesada[8] + "', " +
                                    "[destino] = '" + primera_pesada[9] + "', [propietario_camion] = '" + primera_pesada[10] + "', [chofer] = '" + primera_pesada[11] + "', [placas] = '" + primera_pesada[12] + "', " +
                                    "[maquilador] = '" + primera_pesada[13] + "', predio_cliente = '" + primera_pesada[14] + "', [obs_p1] = '" + primera_pesada[15] + "', [peso_ori] = '" + primera_pesada[16] + "', " +
                                    "[p_matsec] = '" + primera_pesada[17] + "', [agrupada] = '" + primera_pesada[18] + "', [peso1] = '" + primera_pesada[19] + "', [peso2] = '" + primera_pesada[20] + "', " +
                                    "[peso_t] = '" + primera_pesada[21] + "', [rtime] = '" + fechas_sin + "', [rtime_p1] = '" + fechas_con_1 + "', [rtime_p2] = '" + fechas_con_2 + "', " +
                                    "[termina] = '" + termina + "', [tabla] = '" + primera_pesada[24] + "', [variedad] = '" + primera_pesada[25] + "', [ensilador] = '" + primera_pesada[26] + "', [corte] = '" + primera_pesada[27] + "', [pacas] = '" + primera_pesada[28] + "' WHERE ficha = '" + ficha + "'";
                                break;
                            case "4":
                                command.CommandText = "update [base_forrajes].[dbo].[fichas_tn] set [pozo] = '" + primera_pesada[2] + "', [prov_cli] = '" + primera_pesada[3] + "', [producto] = '" + primera_pesada[4] + "', " +
                                    "[cvemov] = '" + primera_pesada[5] + "', [tmov] = '" + primera_pesada[6] + "', [folio] = '" + primera_pesada[7] + "', [sucursal] = '" + primera_pesada[8] + "', " +
                                    "[destino] = '" + primera_pesada[9] + "', [propietario_camion] = '" + primera_pesada[10] + "', [chofer] = '" + primera_pesada[11] + "', [placas] = '" + primera_pesada[12] + "', " +
                                    "[maquilador] = '" + primera_pesada[13] + "', predio_cliente = '" + primera_pesada[14] + "', [obs_p1] = '" + primera_pesada[15] + "', [peso_ori] = '" + primera_pesada[16] + "', " +
                                    "[p_matsec] = '" + primera_pesada[17] + "', [agrupada] = '" + primera_pesada[18] + "', [peso1] = '" + primera_pesada[19] + "', [peso2] = '" + primera_pesada[20] + "', " +
                                    "[peso_t] = '" + primera_pesada[21] + "', [rtime] = '" + fechas_sin + "', [rtime_p1] = '" + fechas_con_1 + "', [rtime_p2] = '" + fechas_con_2 + "', " +
                                    "[termina] = '" + termina + "', [tabla] = '" + primera_pesada[24] + "', [variedad] = '" + primera_pesada[25] + "', [ensilador] = '" + primera_pesada[26] + "', [corte] = '" + primera_pesada[27] + "', [pacas] = '" + primera_pesada[28] + "' WHERE ficha = '" + ficha + "'";
                                break;
                        }
                        command.ExecuteNonQuery();


                        int id_usuario = (int)Session["LoggedId"];
                        try
                        {
                            UTILERIASController utileria = new UTILERIASController();
                            utileria.RegistroLogsSoporteGral(id_usuario, "Se edito la ficha: " + primera_pesada[1], justificacion);
                        }
                        catch (Exception) { }

                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string CerrarFichaSab(int fichas, int id_establo, string justificacion)
        {
            try
            {
                using (var connection = Models.ConexionSab.Conectar())
                {
                    using (var command = new SqlCommand())
                    {

                        string ficha = fichas.ToString().PadLeft(25, ' ');
                        command.Connection = connection;
                        switch (id_establo)
                        {
                            case 1:
                                command.CommandText = "UPDATE [dbo].[fichas_sg] SET [rstatus] = 'E' WHERE ficha = '" + ficha + "'";
                                break;
                            case 2:
                                command.CommandText = "UPDATE [dbo].[fichas_si] SET [rstatus] = 'E' WHERE ficha = '" + ficha + "'";
                                break;
                            case 3:
                                command.CommandText = "UPDATE [dbo].[fichas_sm] SET [rstatus] = 'E' WHERE ficha = '" + ficha + "'";
                                break;
                            case 4:
                                command.CommandText = "UPDATE [dbo].[fichas_tn] SET [rstatus] = 'E' WHERE ficha = '" + ficha + "'";
                                break;
                        }
                        command.ExecuteNonQuery();

                        int id_usuario = (int)Session["LoggedId"];
                        try
                        {
                            UTILERIASController utileria = new UTILERIASController();
                            utileria.RegistroLogsSoporteGral(id_usuario, "Se cerro la ficha: " + fichas, justificacion);
                        }
                        catch (Exception) { }

                        return "Exito";
                    }
                }
            }
            catch (Exception)
            {
                return "No se encontró la ficha. Asegurese de no modificar el campo una vez se haya buscado";
            }
        }
    }
}