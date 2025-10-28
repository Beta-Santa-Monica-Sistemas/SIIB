using Antlr.Runtime.Misc;
using Beta_System.Helper;
using Beta_System.Models;
using FirebirdSql.Data.FirebirdClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;
using System.Security.Policy;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Utilities;

namespace Beta_System.Controllers
{
    public class PROVEEDORESController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        private PERMISOSController permiso = new PERMISOSController();
        NOTIFICACIONESController noti = new NOTIFICACIONESController();

        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(5019)) { return View("/Views/Home/Index.cshtml"); }

                return View("../COMPRAS/PROVEEDORES/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarEstatus()
        {
            var tipos_art = db.C_compras_proveedores_status.Where(x => x.activo == true).OrderBy(x=>x.status_proveedor).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_EstatusSelect", tipos_art);
        }

        public PartialViewResult ConsultarBanco()
        {
            var tipos_art = db.C_compras_bancos.Where(x => x.activo == true).OrderBy(x=>x.nombre_banco).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_BancosSelect", tipos_art);
        }

        public PartialViewResult GiroEmpresa()
        {
            var tipos_art = db.C_compras_proveedores_giros.Where(x => x.activo == true).OrderBy(x => x.nombre_giro).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_GiroEmpresaSelect", tipos_art);
        }

        public PartialViewResult ConsultarProveedores(int id_estatus)
        {
            if(id_estatus == 0)
            {
                var proveedores = db.C_compras_proveedores.OrderBy(x => x.nombre_prov).ToList();
                return PartialView("../COMPRAS/PROVEEDORES/_ProveedoresTable", proveedores);
            }
            else
            {
                var proveedores = db.C_compras_proveedores.Where(x => x.activo == true && x.id_proveedor_status == id_estatus).OrderBy(x => x.nombre_prov).ToList();
                return PartialView("../COMPRAS/PROVEEDORES/_ProveedoresTable", proveedores);
            }
        }

        public int ValidaClaveProveedor(string clave)
        {
            var clave_proveedor = db.C_compras_proveedores.Where(x=>x.cve == clave).ToList();
            if(clave_proveedor.Count == 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public PartialViewResult ConsultarProveedoresBloq(int id_proveedor)
        {
            if (id_proveedor == -1)
            {
                var proveedores = db.C_compras_proveedores_bloqueos.Where(x => x.activo == true).OrderBy(x => x.fecha_registro).ToList();
                return PartialView("../COMPRAS/PROVEEDORES/_ProveedoresBloqueoTable", proveedores);
            }
            else
            {
                var proveedores = db.C_compras_proveedores_bloqueos.Where(x => x.activo == true && x.id_compras_proveedor == id_proveedor).OrderBy(x => x.fecha_registro).ToList();
                return PartialView("../COMPRAS/PROVEEDORES/_ProveedoresBloqueoTable", proveedores);
            }
        }

        public bool RegistarProveedores(C_compras_proveedores c_proveedor /*decimal precio*/)
        {
            try
            {
                c_proveedor.id_proveedor_status = 1;
                c_proveedor.activo = true;
                c_proveedor.fecha_registro = DateTime.Now;
                db.C_compras_proveedores.Add(c_proveedor);
                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string ObtenerInfProveedor(int id_compras_proveedor)
        {
            var proveedor = from prov in db.C_compras_proveedores
                            join moneda in db.C_tipos_moneda on prov.id_tipo_moneda equals moneda.id_moneda
                            where prov.id_compras_proveedor == id_compras_proveedor
                            select new
                            {
                                prov.cve,
                                prov.nombre_prov,
                                prov.razon_social,
                                prov.RFC,
                                prov.direccion_1_prov,
                                prov.direccion_2_prov,
                                prov.tel_prov,
                                prov.correo_prov,
                                prov.dias_pago,
                                prov.contacto_nombre_1,
                                prov.contacto_correo_1,
                                prov.contacto_tel_1,
                                prov.contacto_nombre_2,
                                prov.contacto_correo_2,
                                prov.contacto_tel_2,
                                prov.id_giro_proveedor,
                                prov.giro_desciprcion,
                                prov.cta_banco_1,
                                moneda.nombre,
                                moneda.clave_fiscal,
                                prov.cta_banco_2,
                                prov.cta_banco_tipo_moneda_2,
                                prov.id_proveedor_status,
                                prov.prov_alimentacion,
                                prov.cuenta_cxp,
                                prov.banco,
                                prov.disponible_bascula,
                                prov.alias_bascula
                            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(proveedor);
        }

        public string ObtenerObsProveedor(int id_compras_proveedor)
        {
            var proveedor = from prov in db.C_compras_proveedores_bloqueos
                            where prov.id_compras_proveedor == id_compras_proveedor orderby prov.fecha_registro descending
                            select new
                            {
                                prov.motivo
                            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(proveedor);
        }

        public bool ModificarProveedor(int id_compras_proveedor, C_compras_proveedores c_proveedor)
        {
            try
            {
                var prov = db.C_compras_proveedores.Find(id_compras_proveedor);
                prov.nombre_prov = c_proveedor.nombre_prov;
                prov.id_proveedor_status = c_proveedor.id_proveedor_status;
                prov.contacto_nombre_1 = c_proveedor.contacto_nombre_1;
                prov.contacto_correo_1 = c_proveedor.contacto_correo_1;
                prov.contacto_tel_1 = c_proveedor.contacto_tel_1;
                prov.cta_banco_1 = c_proveedor.cta_banco_1;
                prov.giro_desciprcion = c_proveedor.giro_desciprcion;
                prov.id_giro_proveedor = c_proveedor.id_giro_proveedor;
                prov.cta_banco_tipo_moneda_1 = c_proveedor.cta_banco_tipo_moneda_1;
                prov.prov_alimentacion = c_proveedor.prov_alimentacion;
                prov.disponible_bascula = c_proveedor.disponible_bascula;
                prov.alias_bascula = c_proveedor.alias_bascula;

                if (c_proveedor.id_proveedor_status == 1) { prov.activo = true; }
                else { prov.activo = false; }

                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool BloqueoLogProveedor(int id_compras_proveedor,string motivo)
        {
            try
            {
                C_compras_proveedores_bloqueos c_bloqueos = new C_compras_proveedores_bloqueos();
                c_bloqueos.id_compras_proveedor = id_compras_proveedor;
                c_bloqueos.motivo = motivo;
                c_bloqueos.activo = true;
                c_bloqueos.fecha_registro = DateTime.Now;
                c_bloqueos.id_usuario_bloquea = (int)Session["LoggedId"];
                db.C_compras_proveedores_bloqueos.Add(c_bloqueos);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool OnOffProveedores(int id_compras_proveedor, int modo)
        {
            try
            {
                var prov = db.C_compras_proveedores.Find(id_compras_proveedor);
                prov.id_proveedor_status = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ConsultarProveedoresArray(string search)
        {
            int busqueda = 0;
            try
            {
                busqueda = Convert.ToInt32(search);
                var articulos = (from prov in db.C_compras_proveedores
                                 where prov.activo == true && prov.cve.ToString().Contains(search)
                                 select new { prov.id_compras_proveedor, prov.razon_social, prov.C_tipos_moneda.clave_fiscal, prov.contacto_nombre_1 }).Take(15);
                return Newtonsoft.Json.JsonConvert.SerializeObject(articulos);
            }
            catch (Exception) 
            {
                var articulos = (from prov in db.C_compras_proveedores
                                 where prov.activo == true && (prov.RFC.ToString().Contains(search) || prov.razon_social.ToString().Contains(search) || prov.contacto_nombre_1.ToString().Contains(search))
                                 select new { prov.id_compras_proveedor, prov.razon_social, prov.C_tipos_moneda.clave_fiscal, prov.contacto_nombre_1 }).Take(15);
                return Newtonsoft.Json.JsonConvert.SerializeObject(articulos);
            }
        }

        public string ConsultarInformacionNomProv(string id_proveedor)
        {
            var provedor = from prov in db.C_compras_proveedores
                           where prov.cve == id_proveedor && prov.id_proveedor_status == 1
                           select new
                           {
                               prov.id_compras_proveedor,
                               prov.nombre_prov,
                               prov.razon_social,
                               prov.direccion_1_prov,
                               prov.contacto_nombre_1,
                               prov.tel_prov,
                               prov.contacto_tel_1,
                               prov.correo_prov,
                               prov.dias_pago,
                               prov.C_tipos_moneda.clave_fiscal,
                               prov.id_tipo_moneda
                           };
            if (provedor != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(provedor);
            }
            else
            {
                return null;
            }

        }
        public string ConsultarInfoClave(string id_proveedor)
        {
            var provedor = from prov in db.C_compras_proveedores
                           where prov.razon_social == id_proveedor
                           select new
                           {
                               prov.cve,
                               prov.razon_social,
                               prov.C_tipos_moneda.clave_fiscal,
                               prov.id_tipo_moneda,
                               prov.correo_prov
                           };
            if (provedor != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(provedor);
            }
            else
            {
                return null;
            }

        }

        public PartialViewResult LupaProveedor(int accion)
        {
            ViewBag.proveedores = accion;
            var proveedores = db.C_compras_proveedores.Where(x => x.activo == true && x.id_proveedor_status == 1).OrderBy(x => x.nombre_prov).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_LupaProveedoresTable", proveedores);
        }

        #region MICROSIP PROVEEDORES
        public ActionResult ConfiguraUsuariosProveedores()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6029)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/PROVEEDORES/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        //1 - EXITO, 2- FALLO, 3-YA EXISTE
        public int RegistrarProveedorCuenta(string usuario, string contrasena, int id_proveedor)
        {
            try
            {
                var existe_proveedor = db.C_usuarios_proveedores.Where(x => x.usuario_rfc == usuario).Count();
                var existe_cuentas = db.C_usuarios_proveedores.Where(x => x.id_compras_proveedor == id_proveedor).Count();
                if (existe_proveedor < 1 && existe_cuentas < 1)
                {
                    C_usuarios_proveedores proveedor = new C_usuarios_proveedores();
                    proveedor.usuario_rfc = usuario;
                    proveedor.password = PasswordHelper.EncodePassword(contrasena, "MySalt");
                    proveedor.pass = contrasena;
                    proveedor.id_compras_proveedor = id_proveedor;
                    proveedor.activo = true;
                    //proveedor.id_rol = 1005;
                    db.C_usuarios_proveedores.Add(proveedor);
                    db.SaveChanges();
                    return 1;
                }
                else
                {
                    return 3;
                }
            }
            catch (Exception)
            {
                return 2;
            };
        }
        public PartialViewResult MostrarProveedoresTable()
        {
            var proveedores = db.C_usuarios_proveedores.ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_ProveedoresCuentasTable", proveedores);
        }
        public bool OnOffProveedor(int id_proveedor, bool modo)
        {
            try
            {
                var proveedor = db.C_usuarios_proveedores.Find(id_proveedor);
                proveedor.activo = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            };
        }
        public int ConsultarProvNombre(string nombre)
        {
            try
            {
                var proveedor = db.C_compras_proveedores.Where(x => x.activo == true && x.razon_social == nombre).FirstOrDefault();
                return proveedor.id_compras_proveedor;
            }
            catch (Exception)
            {
                return 0;
            };
        }
        public PartialViewResult MostrarProveedoresFacturas(int idproveedor)
        {
            var proveedores = db.C_compras_facturas_proveedores.Where(x => x.id_compras_proveedor == idproveedor).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/_FacturasProveedorTable", proveedores);
        }
        #endregion


        //------------- SOLICITUDES DE PROVEEDORES
        public ActionResult SolicitudesRegistroProveedor()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7040)) { return View("/Views/Home/Index.cshtml"); }

                return View("../COMPRAS/PROVEEDORES/SolicitudesRegistro/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        public PartialViewResult ConsultarSolicitudesRegistroProveedor(int id_status)
        {
            List<C_compras_proveedores_portal_solicitudes> solicitudes = null;
            //if (id_status == 2) //VALIDADA/AUTORIZADA
            //{
            //    solicitudes = db.C_compras_proveedores_portal_solicitudes.Where(x => x.id_proveedor_solicitud_status == id_status || x.id_proveedor_solicitud_status == 3).ToList();
            //}
            //else
            //{
            //    solicitudes = db.C_compras_proveedores_portal_solicitudes.Where(x => x.id_proveedor_solicitud_status == id_status).ToList();
            //}
            solicitudes = db.C_compras_proveedores_portal_solicitudes.Where(x => x.id_proveedor_solicitud_status == id_status).ToList();
            ViewBag.id_status = id_status;
            return PartialView("../COMPRAS/PROVEEDORES/SolicitudesRegistro/_SolicitudesEjercidasTable", solicitudes);
        }

        public PartialViewResult ConsultarDetalleSolicitud(int id_solicitud)
        {
            var detalle = db.C_compras_proveedores_portal_solicitudes.Find(id_solicitud);
            return PartialView("../COMPRAS/PROVEEDORES/SolicitudesRegistro/_DetalleSolicitudTable", detalle);
        }

        public string MostrarDocumentoSolciitudProveedor(int id_solicitud, string doc)
        {
            string path_doc = $"{doc}_{id_solicitud}.pdf";
            string basePath = $"\\\\192.168.128.2\\inetpub\\SolicitudesProveedores";
            string[] existingFiles = Directory.GetFiles(basePath, path_doc);

            if (existingFiles.Length == 1)
            {
                return $"http://192.168.128.2:92/{path_doc}";
            }
            return "1";
        }


        public PartialViewResult ConsultarTokensGenerados()
        {
            var tokens = db.C_compras_proveedores_portal_tokens.Where(x => x.activo == true).ToList();
            return PartialView("../COMPRAS/PROVEEDORES/SolicitudesRegistro/_TokensGenerados", tokens);
        }

        public int AutorizarRechazarSolicitudRegistroProveedor(int id_solicitud, int id_status, string cta_cxp)
        {
            try
            {
                var valid = db.C_compras_proveedores_portal_solicitudes.Find(id_solicitud);
                if (valid.id_proveedor_solicitud_status == 2) { return -2; }     //LA SOLICITUD YA FUE AUTORIZADA
                if (valid.id_proveedor_solicitud_status == 3) { return -3; }     //LA SOLICITUD ESTA RECHAZADA
                if (valid.id_proveedor_solicitud_status == 4) { return -4; }     //LA SOLICITUD YA FUE SINCRONIZADA
                valid.id_proveedor_solicitud_status = id_status;                        
                if (id_status != 2 && cta_cxp == "") { db.SaveChanges(); }
                else
                {  //AUTORIZADA
                    int ResultImportacion = ImportarProveedorMS(valid, cta_cxp);
                    if (ResultImportacion > 0)
                    {
                        valid.id_proveedor_solicitud_status = 4; //IMPORTADA 
                        db.SaveChanges();

                        var accesos = db.C_usuarios_proveedores.Where(x => x.id_compras_proveedor == ResultImportacion).FirstOrDefault();
                        string usuario_prov = accesos.usuario_rfc;
                        string contrasena = accesos.pass;

                        string mensaje = "<div style='width: 100%;'>" +
                                             "<img src='https://siib.beta.com.mx/Content/img_layout/portal_proveeodres_wallpaper.jpg' style='width: 100%;' />" +
                                             "<div style='background-color: #E8E8E8; padding:2rem;'>" +
                                                 "<h1 style='color:#000A65; margin: 10px;'>Estimado proveedor</h2>" +
                                                 "<h3 style='color: black; margin: 10px;'>Le informamos que su solicitud de registro ha sido aprobada exitosamente. A continuación, le proporcionamos sus credenciales de acceso al portal:</h4>" +
                                                 "<br /><br />" +
                                                     "<div style='text-align: center; color: #000A65;'>" +
                                                         "<label style='font-weight: 800; font-size: 16px;'>Usuario: </label><strong>" + accesos.usuario_rfc + "</strong><br />" +
                                                         "<label style='font-weight: 800; font-size: 16px;'>Contraseña: </label><strong>" + accesos.pass + "</strong>" +
                                                     "</div>" +
                                             "</div>" +
                                             "<div style='width:100%; text-align:center;'><a style='display: inline-block; padding: 10px 20px; background-color: #000A65; color: #ffffff; border-radius: 45px; transition: all 0.3s; border: none; margin-top: 2rem; padding: 1.2rem;' href='https://portaldeproveedores.beta.com.mx/'>CLIC AQUI PARA ENTRAR AL PORTAL</a></div>" +
                                       "</div>";

                        noti.EnviarCorreoUsuarioReportes("Confirmación de Registro y Acceso al Portal", valid.correo, mensaje);
                    }
                    else if (ResultImportacion == -1) { return -5; }  //CUENTA EXISTENTE
                    else if (ResultImportacion == -2) { return -6; }  //OCURRIÓ UN ERROR AL GUARDAR LA INFORMACIÓN EN MICROSIP
                    else { return -2; }  //ERROR AL SINCORNIZAR LA INFORMACION
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int ImportarProveedorMS(C_compras_proveedores_portal_solicitudes solicitud, string cta_cxp)
        {
            try
            {
                DateTime Hoy = DateTime.Now;
                string fecha_registro = Hoy.ToString("dd.MM.yyyy");

                FbConnection conn = new FbConnection();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();

                if (ValidarCxpExistente(cta_cxp, conn) == false) { return -1; }  //Cuenta existente

                int id_proveedor_ms = 0;
                int id_clave_proveedor = 0;
                int id_cuenta = 0;
                int id_cuenta_padre = ConsultarIDCuentaPadreCxP(cta_cxp, conn);

                using (FbCommand command = new FbCommand("SELECT GEN_ID( ID_CATALOGOS, 1) FROM RDB$DATABASE;", conn)) { id_proveedor_ms = Convert.ToInt32(command.ExecuteScalar()); }

                using (FbCommand command = new FbCommand("SELECT GEN_ID( ID_CATALOGOS, 1) FROM RDB$DATABASE;", conn)) { id_clave_proveedor = Convert.ToInt32(command.ExecuteScalar()); }

                using (FbCommand command = new FbCommand("SELECT GEN_ID( ID_CATALOGOS, 1) FROM RDB$DATABASE;", conn)) { id_cuenta = Convert.ToInt32(command.ExecuteScalar()); }

                if (id_proveedor_ms != 0 && id_clave_proveedor != 0 && id_cuenta != 0 && id_cuenta_padre != 0)
                {
                    string[] partes = cta_cxp.Split('.');
                    string resultado = string.Join(".", partes.Select(p => p.PadLeft(9, '0')));

                    string sub_cuenta = cta_cxp.Split('.')[2];

                    string QueryCtaCo = "INSERT INTO CUENTAS_CO (CUENTA_ID, CUENTA_PADRE_ID, CUENTA_PT, CUENTA_JT, SUBCUENTA, NOMBRE, OCULTA, ES_PRORRATEO, " +
                        " USUARIO_CREADOR, FECHA_HORA_CREACION, USUARIO_ULT_MODIF, FECHA_HORA_ULT_MODIF)" +
                        " VALUES (@CUENTA_ID, @CUENTA_PADRE_ID, @CUENTA_PT, @CUENTA_JT, @SUBCUENTA, @NOMBRE, @OCULTA, @ES_PRORRATEO, " +
                        " @USUARIO_CREADOR, @FECHA_HORA_CREACION, @USUARIO_ULT_MODIF, @FECHA_HORA_ULT_MODIF)";
                    using (FbCommand commandCtaCo = new FbCommand(QueryCtaCo, conn))
                    {
                        commandCtaCo.Parameters.Add("@CUENTA_ID", FbDbType.VarChar).Value = id_cuenta;
                        commandCtaCo.Parameters.Add("@CUENTA_PADRE_ID", FbDbType.VarChar).Value = id_cuenta_padre;
                        commandCtaCo.Parameters.Add("@CUENTA_PT", FbDbType.Integer).Value = cta_cxp;
                        commandCtaCo.Parameters.Add("@CUENTA_JT", FbDbType.VarChar).Value = resultado;
                        commandCtaCo.Parameters.Add("@SUBCUENTA", FbDbType.VarChar).Value = sub_cuenta;
                        commandCtaCo.Parameters.Add("@NOMBRE", FbDbType.VarChar).Value = solicitud.razon_social;
                        commandCtaCo.Parameters.Add("@OCULTA", FbDbType.VarChar).Value = "N";
                        commandCtaCo.Parameters.Add("@ES_PRORRATEO", FbDbType.VarChar).Value = "N";
                        commandCtaCo.Parameters.Add("@USUARIO_CREADOR", FbDbType.VarChar).Value = "SYSDBA";
                        commandCtaCo.Parameters.Add("@FECHA_HORA_CREACION", FbDbType.VarChar).Value = fecha_registro;
                        commandCtaCo.Parameters.Add("@USUARIO_ULT_MODIF", FbDbType.VarChar).Value = "SYSDBA";
                        commandCtaCo.Parameters.Add("@FECHA_HORA_ULT_MODIF", FbDbType.VarChar).Value = fecha_registro;
                        commandCtaCo.ExecuteNonQuery();
                    }


                    string QueryProveedores = "INSERT INTO PROVEEDORES (PROVEEDOR_ID, NOMBRE, CONTACTO1, CALLE, NOMBRE_CALLE, NUM_EXTERIOR, COLONIA, CIUDAD_ID, ESTADO_ID, CODIGO_POSTAL, PAIS_ID, " +
                        " TELEFONO1, EMAIL, RFC_CURP, ESTATUS, CARGA_IMPUESTOS, RETENER_IMPUESTOS, SUJETO_IEPS, EXTRANJERO, LIMITE_CREDITO, ORDEN_MINIMA,  " +
                        " MONEDA_ID, COND_PAGO_ID, TIPO_PROV_ID, ACTIVIDAD_PRINCIPAL, USUARIO_CREADOR, FECHA_HORA_CREACION, USUARIO_ULT_MODIF, FECHA_HORA_ULT_MODIF, CUENTA_CXP) " +
                        "  VALUES" +
                       " (@PROVEEDOR_ID, @NOMBRE, @CONTACTO1, @CALLE, @NOMBRE_CALLE, @NUM_EXTERIOR, @COLONIA, @CIUDAD_ID, @ESTADO_ID, @CODIGO_POSTAL, @PAIS_ID, " +
                       " @TELEFONO1, @EMAIL, @RFC_CURP, @ESTATUS, @CARGA_IMPUESTOS, @RETENER_IMPUESTOS, @SUJETO_IEPS, @EXTRANJERO, @LIMITE_CREDITO, @ORDEN_MINIMA, " +
                       "  @MONEDA_ID, @COND_PAGO_ID, @TIPO_PROV_ID, @ACTIVIDAD_PRINCIPAL, @USUARIO_CREADOR, @FECHA_HORA_CREACION, @USUARIO_ULT_MODIF, @FECHA_HORA_ULT_MODIF, @CUENTA_CXP)";   //CUENTA_CXP (pend),
                    using (FbCommand commandProv = new FbCommand(QueryProveedores, conn))
                    {
                        commandProv.Parameters.Add("@PROVEEDOR_ID", FbDbType.VarChar).Value = id_proveedor_ms;
                        commandProv.Parameters.Add("@NOMBRE", FbDbType.VarChar).Value = solicitud.razon_social;
                        commandProv.Parameters.Add("@CONTACTO1", FbDbType.VarChar).Value = solicitud.nombre_contacto;
                        commandProv.Parameters.Add("@CALLE", FbDbType.VarChar).Value = solicitud.calle;
                        commandProv.Parameters.Add("@NOMBRE_CALLE", FbDbType.VarChar).Value = solicitud.calle;
                        commandProv.Parameters.Add("@NUM_EXTERIOR", FbDbType.VarChar).Value = solicitud.no_exterior;
                        commandProv.Parameters.Add("@COLONIA", FbDbType.VarChar).Value = solicitud.colonia;
                        commandProv.Parameters.Add("@CIUDAD_ID", FbDbType.VarChar).Value = solicitud.id_ciudad;
                        commandProv.Parameters.Add("@ESTADO_ID", FbDbType.VarChar).Value = solicitud.id_estado;
                        commandProv.Parameters.Add("@CODIGO_POSTAL", FbDbType.VarChar).Value = solicitud.codigo_postal;
                        commandProv.Parameters.Add("@PAIS_ID", FbDbType.VarChar).Value = solicitud.id_pais;
                        commandProv.Parameters.Add("@TELEFONO1", FbDbType.VarChar).Value = solicitud.telefono;
                        commandProv.Parameters.Add("@EMAIL", FbDbType.VarChar).Value = solicitud.correo;
                        commandProv.Parameters.Add("@RFC_CURP", FbDbType.VarChar).Value = solicitud.RFC;
                        commandProv.Parameters.Add("@ESTATUS", FbDbType.VarChar).Value = "N";
                        commandProv.Parameters.Add("@CARGA_IMPUESTOS", FbDbType.VarChar).Value = "S";
                        commandProv.Parameters.Add("@RETENER_IMPUESTOS", FbDbType.VarChar).Value = "N";
                        commandProv.Parameters.Add("@SUJETO_IEPS", FbDbType.VarChar).Value = "N";
                        commandProv.Parameters.Add("@EXTRANJERO", FbDbType.VarChar).Value = "N";
                        commandProv.Parameters.Add("@LIMITE_CREDITO", FbDbType.VarChar).Value = "0";
                        commandProv.Parameters.Add("@ORDEN_MINIMA", FbDbType.VarChar).Value = "0.00";
                        commandProv.Parameters.Add("@MONEDA_ID", FbDbType.VarChar).Value = solicitud.id_moneda;
                        commandProv.Parameters.Add("@COND_PAGO_ID", FbDbType.VarChar).Value = "45216";  //FALTA
                        commandProv.Parameters.Add("@TIPO_PROV_ID", FbDbType.VarChar).Value = solicitud.id_tipo_proveedor_giro;
                        commandProv.Parameters.Add("@CUENTA_CXP", FbDbType.VarChar).Value = cta_cxp;
                        commandProv.Parameters.Add("@ACTIVIDAD_PRINCIPAL", FbDbType.VarChar).Value = "85";
                        commandProv.Parameters.Add("@USUARIO_CREADOR", FbDbType.VarChar).Value = "SYSTEM";
                        commandProv.Parameters.Add("@FECHA_HORA_CREACION", FbDbType.VarChar).Value = fecha_registro;
                        commandProv.Parameters.Add("@USUARIO_ULT_MODIF", FbDbType.VarChar).Value = "SYSTEM";
                        commandProv.Parameters.Add("@FECHA_HORA_ULT_MODIF", FbDbType.VarChar).Value = fecha_registro;
                        //30
                        commandProv.ExecuteNonQuery();
                    }


                    string QueryLibres = "INSERT INTO LIBRES_PROVEEDOR (PROVEEDOR_ID, CLAVE_INTERBANCARIA, NOMBRE_PERSONA_FISICA, TIPO_DE_PERSONA, BANCO, MONTO_MAXIMO)" +
                        " VALUES (@PROVEEDOR_ID, @CLAVE_INTERBANCARIA, @NOMBRE_PERSONA_FISICA, @TIPO_DE_PERSONA, @BANCO, @MONTO_MAXIMO)";
                    using (FbCommand commandLibres = new FbCommand(QueryLibres, conn))
                    {
                        commandLibres.Parameters.Add("@PROVEEDOR_ID", FbDbType.VarChar).Value = id_proveedor_ms;
                        commandLibres.Parameters.Add("@CLAVE_INTERBANCARIA", FbDbType.VarChar).Value = solicitud.clave_interbancaria;
                        commandLibres.Parameters.Add("@NOMBRE_PERSONA_FISICA", FbDbType.VarChar).Value = solicitud.nombre_persona_fisica;
                        commandLibres.Parameters.Add("@TIPO_DE_PERSONA", FbDbType.VarChar).Value = solicitud.id_tipo_persona;
                        commandLibres.Parameters.Add("@BANCO", FbDbType.VarChar).Value = solicitud.banco;
                        commandLibres.Parameters.Add("@MONTO_MAXIMO", FbDbType.VarChar).Value = "0";
                        commandLibres.ExecuteNonQuery();
                    }


                    string QueryClave = "INSERT INTO CLAVES_PROVEEDORES (CLAVE_PROV_ID, CLAVE_PROV, PROVEEDOR_ID, ROL_CLAVE_PROV_ID)" +
                        " VALUES (@CLAVE_PROV_ID ,@CLAVE_PROV, @PROVEEDOR_ID, @ROL_CLAVE_PROV_ID)";
                    using (FbCommand commandClaves = new FbCommand(QueryClave, conn))
                    {
                        commandClaves.Parameters.Add("@CLAVE_PROV_ID", FbDbType.VarChar).Value = id_clave_proveedor;
                        commandClaves.Parameters.Add("@CLAVE_PROV", FbDbType.VarChar).Value = cta_cxp;
                        commandClaves.Parameters.Add("@PROVEEDOR_ID", FbDbType.Integer).Value = id_proveedor_ms;
                        commandClaves.Parameters.Add("@ROL_CLAVE_PROV_ID", FbDbType.VarChar).Value = "49";
                        commandClaves.ExecuteNonQuery();
                    }

                    try
                    {
                        using (FbTransaction transaction = conn.BeginTransaction())
                        {
                            transaction.Commit();
                        }
                        solicitud.id_proveedor_solicitud_status = 4;
                        solicitud.importada_MS = true;
                        solicitud.id_proveedor_ms = id_proveedor_ms;
                        var token = db.C_compras_proveedores_portal_tokens.Find(solicitud.id_codigo_proveedor);
                        token.fecha_ejercido = Hoy;

                        C_compras_proveedores c_proveedor = new C_compras_proveedores();
                        c_proveedor.cve = id_proveedor_ms.ToString();
                        c_proveedor.id_proveedor_microsip = id_proveedor_ms;
                        c_proveedor.nombre_prov = solicitud.nombre_proveedor;
                        c_proveedor.razon_social = solicitud.razon_social;
                        c_proveedor.RFC = solicitud.RFC;
                        c_proveedor.direccion_1_prov = solicitud.calle + " " + solicitud.no_exterior + " " + solicitud.colonia + " " + solicitud.C_direcciones_ciudades.nombre_ciudad + ", " + solicitud.C_direcciones_estados.nombre_estado + " " + solicitud.C_direcciones_paises.nombre_pais;
                        c_proveedor.tel_prov = solicitud.telefono;
                        c_proveedor.correo_prov = solicitud.correo;
                        c_proveedor.dias_pago = 30;
                        c_proveedor.fecha_registro = Hoy;
                        c_proveedor.contacto_nombre_1 = solicitud.nombre_contacto;
                        c_proveedor.contacto_correo_1 = solicitud.correo;
                        c_proveedor.contacto_tel_1 = solicitud.telefono;
                        c_proveedor.autorizado = true;
                        c_proveedor.id_proveedor_status = 1; //AUTORIZADO
                        c_proveedor.activo = true;
                        c_proveedor.cta_banco_1 = solicitud.clave_interbancaria;
                        c_proveedor.prov_alimentacion = false;
                        c_proveedor.cuenta_cxp = cta_cxp;
                        if (solicitud.id_tipo_persona == 2856270) { c_proveedor.tipo_persona = "FISICA"; }
                        else { c_proveedor.tipo_persona = "MORAL"; }
                        c_proveedor.banco = solicitud.banco;
                        c_proveedor.id_tipo_moneda = solicitud.id_moneda;
                        c_proveedor.disponible_bascula = false;
                        c_proveedor.alias_bascula = solicitud.nombre_proveedor;

                        db.C_compras_proveedores.Add(c_proveedor);
                        db.SaveChanges();

                        solicitud.id_compras_proveedor = c_proveedor.id_compras_proveedor;
                        Random rand = new Random();
                        string contrasena = rand.Next(100000, 1000000).ToString();
                        C_usuarios_proveedores proveedor = new C_usuarios_proveedores();
                        proveedor.usuario_rfc = c_proveedor.RFC.Trim();
                        proveedor.password = PasswordHelper.EncodePassword(contrasena, "MySalt");
                        proveedor.pass = contrasena;
                        proveedor.id_compras_proveedor = c_proveedor.id_compras_proveedor;
                        proveedor.activo = true;
                        db.C_usuarios_proveedores.Add(proveedor);
                        db.SaveChanges();

                        return c_proveedor.id_compras_proveedor;
                    }
                    catch (Exception)
                    {
                        solicitud.id_proveedor_solicitud = 1;
                        solicitud.importada_MS = false;
                        db.SaveChanges();

                        var prov = db.C_compras_proveedores.Where(x => x.id_proveedor_microsip == id_proveedor_ms).FirstOrDefault();
                        if (prov != null) {
                            prov.activo = false;
                            prov.id_proveedor_status = 2;
                            db.SaveChanges();
                        }

                        return -2;
                    }

                }

                return -3;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -2;
            }
        }

        public int GenerarTokenRegistro(string nombre_token)
        {
            NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();
            try
            {
                try
                {
                    if ((int)Session["ses_count_token_portal_proveedor"] >= 3)
                    {
                        return -1;
                    }
                }
                catch (Exception) { Session["ses_count_token_portal_proveedor"] = 0; }

                DateTime Hoy = DateTime.Now;
                int id_usuario = (int)Session["LoggedId"];

                //var valid_token = db.C_compras_proveedores_portal_tokens.OrderBy(x => x.fecha_registro).FirstOrDefault();
                //if (valid_token != null) {
                //    TimeSpan diferencia = valid_token.fecha_registro.Value - Hoy;
                //    if (diferencia.TotalSeconds >= 1 || diferencia.TotalSeconds <= 43200)  // DE 1 SEGUNDO A 12 HORAS
                //    {

                //    }
                //}

                C_compras_proveedores_portal_tokens token = new C_compras_proveedores_portal_tokens();
                token.nombre_proveedor = nombre_token;
                token.id_usuario_registro = id_usuario;
                token.codigo_token = notificaciones.GenerarTokenSegmentado3x3();
                token.activo = true;
                token.ejercido = false;
                token.fecha_registro = Hoy;
                token.fecha_ejercido = null;
                db.C_compras_proveedores_portal_tokens.Add(token);

                try
                {
                    Session["ses_count_token_portal_proveedor"] = (int)Session["ses_count_token_portal_proveedor"] + 1;
                    db.SaveChanges();
                }
                catch (Exception) { Session["ses_count_token_portal_proveedor"] = 1; }

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public async Task<string> SYNC_EMPLEADO(string id_prov_ms)
        {
            using (var client = new HttpClient())
            {
                var baseUrl = "http://192.168.128.2:84/api/[SYNC_SYNC_PROVEEDOR_MS_SIIB_Controller]/POST";
                var parameters = new { id_prov_ms = id_prov_ms };
                var json = JsonConvert.SerializeObject(parameters);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JObject.Parse(responseContent);

                    var message = responseObject["Message"].ToString();
                    return JsonConvert.SerializeObject(responseObject);
                    //if (message.Contains("violation of FOREIGN KEY constraint"))
                    //{
                    //    var errorMessage = "ERROR AL REGISTRAR EN MICROSIP";
                    //    return errorMessage;
                    //}
                    //else
                    //{
                    //    return message;
                    //}
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
        }


        public int CambiarAccesosProveedor(int id_compras_proveedor, string pass)
        {
            try
            {
                DateTime hoy = DateTime.Now;
                var existe_proveedor = db.C_compras_proveedores.Find(id_compras_proveedor);
                if (existe_proveedor == null) { return -1; }
                var existe_cuentas = db.C_usuarios_proveedores.Where(x => x.id_compras_proveedor == id_compras_proveedor).FirstOrDefault();
                if (existe_cuentas == null)
                {
                    C_usuarios_proveedores proveedor = new C_usuarios_proveedores();
                    proveedor.usuario_rfc = existe_proveedor.RFC;
                    proveedor.password = PasswordHelper.EncodePassword(pass, "MySalt");
                    proveedor.pass = pass;
                    proveedor.id_compras_proveedor = id_compras_proveedor;
                    proveedor.activo = true;
                    db.C_usuarios_proveedores.Add(proveedor);
                    db.SaveChanges();
                }
                else
                {
                    existe_cuentas.usuario_rfc = existe_proveedor.RFC;
                    existe_cuentas.password = PasswordHelper.EncodePassword(pass, "MySalt");
                    existe_cuentas.pass = pass;
                    db.SaveChanges();
                }
                return 0;
            }
            catch (Exception)
            {
                return 2;
            };
        }


        public string ConsultarConsecutivoCxP(string razon_social, int id_solicitud)
        {
            FbConnection conn = new FbConnection();
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();
            if (string.IsNullOrEmpty(razon_social)) { return ""; }

            int id_moneda = (int)db.C_compras_proveedores_portal_solicitudes.Find(id_solicitud).id_moneda;
            string new_cta_cxp = "230.1";
            int count = 1;
            while (ValidarCxpExistente(new_cta_cxp, conn) == false)
            {
                string inicial = razon_social.Trim().Substring(0, 1).ToUpper();
                if (inicial.Length > 1 || inicial.Length == 0) { return ""; }

                string no_inicial = "";
                if (id_moneda != 1)
                {
                    no_inicial = "28";
                }
                else
                {
                    switch (inicial)
                    {
                        case "A":
                            no_inicial = "1";
                            break;
                        case "B":
                            no_inicial = "2";
                            break;
                        case "C":
                            no_inicial = "3";
                            break;
                        case "D":
                            no_inicial = "4";
                            break;
                        case "E":
                            no_inicial = "5";
                            break;
                        case "F":
                            no_inicial = "6";
                            break;
                        case "G":
                            no_inicial = "7";
                            break;
                        case "H":
                            no_inicial = "8";
                            break;
                        case "I":
                            no_inicial = "9";
                            break;
                        case "J":
                            no_inicial = "10";
                            break;
                        case "K":
                            no_inicial = "11";
                            break;
                        case "L":
                            no_inicial = "12";
                            break;
                        case "M":
                            no_inicial = "13";
                            break;
                        case "N":
                            no_inicial = "14";
                            break;
                        case "Ñ":
                            no_inicial = "15";
                            break;
                        case "O":
                            no_inicial = "16";
                            break;
                        case "P":
                            no_inicial = "17";
                            break;
                        case "Q":
                            no_inicial = "18";
                            break;
                        case "R":
                            no_inicial = "19";
                            break;
                        case "S":
                            no_inicial = "20";
                            break;
                        case "T":
                            no_inicial = "21";
                            break;
                        case "U":
                            no_inicial = "22";
                            break;
                        case "V":
                            no_inicial = "23";
                            break;
                        case "W":
                            no_inicial = "24";
                            break;
                        case "X":
                            no_inicial = "25";
                            break;
                        case "Y":
                            no_inicial = "26";
                            break;
                        case "Z":
                            no_inicial = "27";
                            break;

                        default:
                            break;
                    }
                }
                
                string cta_consecutivo = "230." + no_inicial + ".";
                var data_cta = new List<object[]>();

                string query = "SELECT FIRST 1 CUENTA_PT FROM CUENTAS_CO WHERE CUENTA_PT LIKE '%"+ cta_consecutivo +"%'  ORDER BY CUENTA_ID DESC";
                FbCommand readCommand = new FbCommand(query, conn);
                FbDataReader reader = readCommand.ExecuteReader();
                while (reader.Read())
                {
                    var columns = new object[reader.FieldCount];
                    reader.GetValues(columns);
                    data_cta.Add(columns);
                }

                try
                {
                    int consecutivo = Convert.ToInt32(data_cta[0][0].ToString().Split('.')[2]) + count;
                    string new_cxp = cta_consecutivo + consecutivo.ToString();
                    new_cta_cxp = new_cxp;
                }
                catch (Exception)
                {
                    int consecutivo = 1;
                    string new_cxp = cta_consecutivo + consecutivo.ToString();
                    new_cta_cxp = new_cxp;
                }
                count++;
            }

            return new_cta_cxp;
        }

        public bool ValidarCxpExistente(string new_cta_cxp, FbConnection conn)
        {
            try
            {
                var data_cta = new List<object[]>();

                string query = "SELECT * FROM CUENTAS_CO WHERE CUENTA_PT = '"+ new_cta_cxp + "' ";
                FbCommand readCommand = new FbCommand(query, conn);
                FbDataReader reader = readCommand.ExecuteReader();
                while (reader.Read())
                {
                    var columns = new object[reader.FieldCount];
                    reader.GetValues(columns);
                    data_cta.Add(columns);
                }

                string query_cve = "SELECT * FROM CLAVES_PROVEEDORES WHERE CLAVE_PROV = '" + new_cta_cxp + "' ";
                FbCommand readCommand_cve = new FbCommand(query_cve, conn);
                FbDataReader reader_cve = readCommand_cve.ExecuteReader();
                while (reader_cve.Read())
                {
                    var columns = new object[reader_cve.FieldCount];
                    reader_cve.GetValues(columns);
                    data_cta.Add(columns);
                }
                if (data_cta.Count > 0) { return false; }

                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        private int ConsultarIDCuentaPadreCxP(string new_cta_cxp, FbConnection conn)
        {
            try
            {
                string cta_inicial = "230." + new_cta_cxp.Split('.')[1]; // "230.24.226  ->  230.24"
                var data_cta = new List<object[]>();
                string query = "SELECT FIRST 1 CUENTA_ID FROM CUENTAS_CO WHERE CUENTA_PT = '" + cta_inicial + "'  ORDER BY CUENTA_ID DESC";
                FbCommand readCommand = new FbCommand(query, conn);
                FbDataReader reader = readCommand.ExecuteReader();
                while (reader.Read())
                {
                    var columns = new object[reader.FieldCount];
                    reader.GetValues(columns);
                    data_cta.Add(columns);
                }
                return Convert.ToInt32(data_cta[0][0]);
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public PartialViewResult EditarSolicitudRegistroProveedor(int id_solicitud)
        {
            var data = db.C_compras_proveedores_portal_solicitudes.Find(id_solicitud);
            return PartialView("../COMPRAS/PROVEEDORES/SolicitudesRegistro/_DetalleSolicitudEdit", data);
        }

        public int ActualizarSolicitudRegistroProveedor(int id_solicitud, string nombre, string razon_social, string rfc, string correo, string contacto, string telefono, string clave_inter)
        {
            try
            {
                var solicitud = db.C_compras_proveedores_portal_solicitudes.Find(id_solicitud);
                if (solicitud == null) { return -1; }
                solicitud.nombre_proveedor = nombre;
                solicitud.razon_social = razon_social;
                solicitud.RFC = rfc;
                solicitud.correo = correo;
                solicitud.nombre_contacto = contacto;
                solicitud.telefono = telefono;
                solicitud.clave_interbancaria = clave_inter;
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

    }
}