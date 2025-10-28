using Beta_System.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using FirebirdSql.Data.FirebirdClient;
using HtmlAgilityPack;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Windows.Media.Animation;
using WebGrease.Css.Extensions;
using static System.Net.WebRequestMethods;

namespace Beta_System.Controllers
{
    public class CONTABILIDADController : Controller
    {
        private PERMISOSController permiso = new PERMISOSController();
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private Master_SIIBEntities db_master = new Master_SIIBEntities();
        private FbConnection conn = new FbConnection();
        private NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();

        public string ConsultarCambioMoneda()
        {
            //string apiUrl = "https://api.cambio.today/v1/full/USD/json";
            //string parameter1 = "45953|Q4iyUW5qSxhM7r98SL1n";
            //string fullUrl = $"{apiUrl}?key={parameter1}";

            string fullUrl = "https://v6.exchangerate-api.com/v6/7b270486eb05e55b72583651/latest/USD";
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(fullUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string response_api = response.Content.ReadAsStringAsync().Result;

                    try
                    {
                        JObject jsonResponse = JObject.Parse(response_api);
                        string clave_fiscal_mx = jsonResponse["conversion_rates"]["MXN"].ToString();
                        var valor_dolar = db.C_parametros_configuracion.Find(1015);
                        valor_dolar.valor_numerico = Convert.ToDecimal(clave_fiscal_mx);
                        db.SaveChanges();
                    }
                    catch (Exception){}

                    return response_api; //Newtonsoft.Json.JsonConvert.SerializeObject(response_api);
                }
                else
                {
                    return "0";
                }
            }
        }


        #region IMPORTACION DE FACTURAS
        //-------------------- IMPORTACION DE FACTURAS
        public ActionResult ImportacionFacturas()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6030)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("ImportacionFacturas/Index");
        }

        public PartialViewResult ConsultarFacturasPendientesImportacion(string fecha_inicio, string fecha_fin)
        {
            List<C_compras_facturas_proveedores> facturas = null;

            if (fecha_inicio == null || fecha_fin == null) { return PartialView("ImportacionFacturas/_FacturasPendientesImportacionTable", facturas); }
            else
            {
                DateTime fecha_i = DateTime.Parse(fecha_inicio);
                DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);

                facturas = db.C_compras_facturas_proveedores.Where(x => x.activo == true && x.aplicado == false && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f).ToList();
                return PartialView("ImportacionFacturas/_FacturasPendientesImportacionTable", facturas);
            }
        }


        public ActionResult DescargarPolizaTXT(string path)
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


        public string GenerarImportacionFacturas(int[] id_facturas, string nombre_importacion, decimal tipo_cargo)
        {
            try
            {
                Random rand = new Random();
                conn = ConexionMS.GetConexionMS().CrearConexion();
                conn.Open();

                int facturas_aceptadas = 0;
                int facturas_rechazadas = 0;
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;

                C_contabilidad_importacion_facturas_g importacion_g = new C_contabilidad_importacion_facturas_g();
                importacion_g.nombre_importacion = nombre_importacion;
                importacion_g.fecha_registro = hoy;
                importacion_g.id_usuario_registro = id_usuario;
                importacion_g.activo = true;
                db.C_contabilidad_importacion_facturas_g.Add(importacion_g);
                db.SaveChanges();

                int id_importacion_g = importacion_g.id_contabilidad_importacion_g;
                string filePath = "\\\\192.168.128.2\\inetpub\\PolizasContablesTXT\\POLIZAS_FACTURAS\\POLIZAS_" + id_importacion_g + ".txt";

                C_contabilidad_importacion_facturas_d importacion_d = new C_contabilidad_importacion_facturas_d();
                importacion_d.id_contabilidad_importacion_g = id_importacion_g;
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    try
                    {
                        for (int i = 0; i < id_facturas.Length; i++)
                        {
                            int id_factura = id_facturas[i];
                            var info_orden = db.C_compras_facturas_proveedores.Find(id_factura);
                            int id_orden_g = (int)info_orden.id_compras_orden_g;
                            int id_tipo_moneda = 0;

                            if (id_orden_g == 11308)
                            {
                                string hola = "";
                            }

                            //CAMBIO 10-OCT-2024
                            //info_orden.aplicado = true;
                            //db.SaveChanges();

                            importacion_d.id_factura_orden = id_factura;
                            importacion_d.id_status = 1;
                            importacion_d.activo = true;

                            string folio_cut = info_orden.folio_factura.Length > 10 ? info_orden.folio_factura.Substring(0, 10) : info_orden.folio_factura;

                            if (folio_cut.Trim() == "FL00002")
                            {
                                folio_cut = rand.Next(100, 999999).ToString();
                            }
                            string concepto = info_orden.C_compras_ordenes_g.C_compras_requi_g.concepto;
                            if (concepto.Length > 150)
                            {
                                concepto = concepto.Substring(0, 150);
                            }

                            string descripcion = "O-" + info_orden.id_compras_orden_g + " R-" + info_orden.C_compras_ordenes_g.C_centros_g.siglas + " " + folio_cut + " " + concepto;
                            string header_poliza = "|1|\"C\"," + String.Format(info_orden.fecha_registro.Value.ToShortDateString(), "dd/mm/yyyy") + ",\"1\",\"1\",\"" + descripcion + "\" ";    //ENCABEZADO DE LA POLIZA
                            writer.WriteLine(header_poliza);

                            decimal subtotal_detalle = 0;
                            var cuentas_detalle = db.C_compras_ordenes_d.Where(x => x.id_compras_orden_g == id_orden_g && x.activo == true).GroupBy(x => x.id_cuenta_contable).ToList();
                            foreach (var cargo in cuentas_detalle)
                            {
                                decimal subtotal_cuenta = 0;
                                id_tipo_moneda = (int)cargo.FirstOrDefault().id_tipo_moneda;

                                foreach (var articulos_cuenta in cargo)
                                {
                                    if (id_tipo_moneda == 1)
                                    {
                                        subtotal_cuenta += (decimal)articulos_cuenta.cantidad_compra * (decimal)articulos_cuenta.precio_unitario;
                                    }
                                    else
                                    {
                                        subtotal_cuenta += (decimal)articulos_cuenta.cantidad_compra * ((decimal)articulos_cuenta.precio_unitario * tipo_cargo);
                                    }
                                }

                                string asiento_poliza = "|1.1|\"" + cargo.FirstOrDefault().C_cuentas_contables_g.cuenta.Trim() + "\",\"GRAL\",\"C\"," + subtotal_cuenta.ToString("0.00") + ",\"" + folio_cut + "\",\"\"";   //SUBTOTAL DE LA CUENTA
                                writer.WriteLine(asiento_poliza);
                                subtotal_detalle += subtotal_cuenta;
                            }
                            importacion_d.concepto = descripcion;

                            decimal iva_factura = (decimal)info_orden.iva_monto;
                            decimal iva_ret_factura = (decimal)info_orden.iva_retenido_monto;
                            decimal isr_factura = (decimal)info_orden.isr_monto;
                            decimal total_factura = (decimal)info_orden.total;

                            importacion_d.tipo_cambio = 1;
                            importacion_d.id_moneda = 1;
                            if (id_tipo_moneda != 1)  //DOLAR
                            {
                                iva_factura = iva_factura * tipo_cargo;
                                iva_ret_factura = iva_ret_factura * tipo_cargo;
                                isr_factura = isr_factura * tipo_cargo;
                                total_factura = total_factura * tipo_cargo;

                                importacion_d.tipo_cambio = tipo_cargo;
                                importacion_d.id_moneda = 39118;
                            }

                            //------ VM: MODIFICACIÓN 16 OCT 2025 (SE SUMA EL IEPS AL SUBTOTAL DEL TXT)
                            if (info_orden.ieps_monto > 0)
                            {
                                subtotal_detalle += Convert.ToDecimal(info_orden.ieps_monto.Value.ToString("0.00"));
                            }

                            //------ VM: MODIFICACIÓN 09 SEP 2025 (POLIZAS DESCUADRADAS POR DECIMALES, SE AGREGÓ EL REDONDEO A 2 Y SE PARSEA)
                            decimal pago_proveedor = Convert.ToDecimal(subtotal_detalle.ToString("0.00")) + Convert.ToDecimal(iva_factura.ToString("0.00")) - Convert.ToDecimal(iva_ret_factura.ToString("0.00")) - Convert.ToDecimal(isr_factura.ToString("0.00"));

                            if (iva_factura > 0)
                            {
                                string iva_poliza = "|1.1|\"121.2\",\"GRAL\",\"C\"," + iva_factura.ToString("0.00") + ",\"" + folio_cut + "\",\"\"";            //IVA ( 16 % )
                                writer.WriteLine(iva_poliza);
                            }

                            if (iva_ret_factura > 0)
                            {
                                string iva_ret_poliza = "|1.1|\"250.9\",\"GRAL\",\"A\"," + iva_ret_factura.ToString("0.00") + ",\"" + folio_cut + "\",\"\"";    //IVA RETENIDO ( 4,5,10 % )
                                writer.WriteLine(iva_ret_poliza);
                            }

                            if (isr_factura > 0)
                            {
                                string isr_poliza = "|1.1|\"250.4\",\"GRAL\",\"A\"," + isr_factura.ToString("0.00") + ",\"" + folio_cut + "\",\"\"";            //ISR ( 0.125 % )
                                writer.WriteLine(isr_poliza);
                            }

                            if (pago_proveedor > 0)
                            {
                                string abono_proveedor = "|1.1|\"" + info_orden.C_compras_proveedores.cuenta_cxp.Trim() + "\",\"GRAL\",\"A\"," + pago_proveedor.ToString("0.00") + ",\"" + folio_cut + "\",\"\"";       //PAGO AL PROVEEDOR
                                writer.WriteLine(abono_proveedor);
                            }


                            int result_importacion = 0;
                            var cuenta_cargo = info_orden.C_compras_ordenes_g.C_compras_ordenes_d.FirstOrDefault().C_cuentas_contables_g;
                            if (id_tipo_moneda == 1)
                            {
                                result_importacion = ImportarFacturaMicrosip(id_factura, "1", conn, descripcion, folio_cut, (DateTime)info_orden.fecha_registro.Value, cuenta_cargo.cuenta, subtotal_detalle.ToString(),
                                iva_factura.ToString(), iva_ret_factura.ToString(), isr_factura.ToString());
                            }
                            else
                            {
                                try { iva_factura = iva_factura / tipo_cargo; }
                                catch (Exception) { iva_factura = 0; }

                                try { iva_ret_factura = iva_ret_factura / tipo_cargo; }
                                catch (Exception) { iva_ret_factura = 0; }

                                try { isr_factura = isr_factura / tipo_cargo; }
                                catch (Exception) { isr_factura = 0; }

                                try { total_factura = total_factura / tipo_cargo; }
                                catch (Exception) { total_factura = 0; }

                                try { subtotal_detalle = subtotal_detalle / tipo_cargo; }
                                catch (Exception) { subtotal_detalle = 0; }

                                result_importacion = ImportarFacturaMicrosip(id_factura, tipo_cargo.ToString(), conn, descripcion, folio_cut, (DateTime)info_orden.fecha_registro.Value, cuenta_cargo.cuenta, subtotal_detalle.ToString(),
                                iva_factura.ToString(), iva_ret_factura.ToString(), isr_factura.ToString());
                            }


                            importacion_d.id_cuenta_contable = cuenta_cargo.id_cuenta_contable;
                            importacion_d.id_status = 1;
                            importacion_d.id_docto_cp_ms = null;
                            if (result_importacion == 0)
                            {
                                importacion_d.id_status = 2; //RECHAZADA
                                importacion_d.motivo_rechazo = "Datos existentes en otro cargo (Folio ó Proveedor)";
                                importacion_d.id_docto_cp_ms = null;
                                facturas_rechazadas++;
                            }
                            else
                            {
                                importacion_d.id_docto_cp_ms = result_importacion;
                                facturas_aceptadas++;
                            }
                            db.C_contabilidad_importacion_facturas_d.Add(importacion_d);
                            db.SaveChanges();

                            info_orden.aplicado = true;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        importacion_d.id_status = 2;
                        importacion_d.id_docto_cp_ms = null;
                        importacion_d.motivo_rechazo = "Datos existentes en otro cargo (Folio ó Proveedor)";
                        db.C_contabilidad_importacion_facturas_d.Add(importacion_d);
                        db.SaveChanges();
                    }
                }

                try
                {
                    importacion_g.facturas_importadas = id_facturas.Count();
                    importacion_g.facturas_aceptadas = facturas_aceptadas;
                    importacion_g.facturas_rechazadas = facturas_rechazadas;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                }

                string intArrayAsString = string.Join(",", id_facturas);
                string mensaje = "<strong>IMPORTACION: " + String.Format(hoy.ToShortDateString(), "dd/mm/yyyy") + "</strong><br/>" +
                    "<label>El usuario: " + db.C_usuarios_corporativo.Find(id_usuario).usuario + " importó las facturas registradas en el portal de proveedores </label>" +
                    "<br/><br/><strong><a href=https://siib.beta.com.mx/CONTABILIDAD/DownloadExcel?id_facturas=" + intArrayAsString + " > DESCARGAR EXCEL </ a ></strong>" +
                    "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";

                var correos_importaciones = db.C_usuarios_masters.Where(x => x.activo == true && x.id_usuario_master_accion == 2).Select(x => x.correo).ToList();
                string correos = string.Join(";", correos_importaciones);
                notificaciones.EnviarCorreoUsuario("IMPORTACION DE FACTURAS", correos, mensaje);
                return "https://siib.beta.com.mx/CONTABILIDAD/DescargarPolizaTXT?path=" + filePath.Replace("\\", "/");
            }
            catch (Exception)
            {
                return "-1";
            }
        }


        private int ImportarFacturaMicrosip(int id_factura, string tipo_cargo, FbConnection conn, string descripcion, string folio, DateTime fecha_factura, string cuenta_cargo, string importe, string impuesto,
                                            string iva_retenido, string isr_retenido)
        {
            try
            {
                int dias_credito = 1; 
                var factura = db.C_compras_facturas_proveedores.Find(id_factura);
                string id_prveedor_microsip = factura.id_proveedor_microsip.ToString();
                var info_proveedor = factura.C_compras_proveedores;

                string id_condicion_pago = "55185";
                string condicion_pago = factura.C_compras_proveedores.dias_pago.ToString();
                if (condicion_pago == "30")  //MENSUAL
                {
                    id_condicion_pago = "39745";
                    dias_credito = 30;
                }
                else if (condicion_pago == "15") //QUINCENAL
                {
                    id_condicion_pago = "45216";
                    dias_credito = 15;
                }
                else if (condicion_pago == "7")  //SEMANAL
                {
                    id_condicion_pago = "55185";
                    dias_credito = 7;
                }
                else if (condicion_pago == "0")  //CONTADO
                {
                    id_condicion_pago = "75159";
                    dias_credito = 1;
                }

                string id_docto_cp_det = "";
                string id_docto_cp = "";
                string id_docto_cp_libre = "";
                using (FbCommand command = new FbCommand("SELECT GEN_ID( ID_DOCTOS, 1 ) FROM RDB$DATABASE;", conn))
                {
                    int consecutivo = Convert.ToInt32(command.ExecuteScalar());
                    id_docto_cp = (consecutivo + 1).ToString();
                    id_docto_cp_det = (consecutivo + 2).ToString();
                    id_docto_cp_libre = (consecutivo + 3).ToString();
                }

                string insertQuery = "INSERT INTO DOCTOS_CP (DOCTO_CP_ID, CONCEPTO_CP_ID, SUCURSAL_ID, FOLIO, NATURALEZA_CONCEPTO, FECHA, CLAVE_PROV, PROVEEDOR_ID, TIPO_CAMBIO, CANCELADO, APLICADO," +
                                                            " DESCRIPCION, CUENTA_CONCEPTO, FORMA_EMITIDA, CONTABILIZADO, CONTABILIZADO_GYP, COND_PAGO_ID, SISTEMA_ORIGEN) VALUES " +
                                                            " (@DOCTO_CP_ID, @CONCEPTO_CP_ID, @SUCURSAL_ID, @FOLIO, @NATURALEZA_CONCEPTO, @FECHA, @CLAVE_PROV, @PROVEEDOR_ID, @TIPO_CAMBIO, @CANCELADO, @APLICADO," +
                                                            " @DESCRIPCION, @CUENTA_CONCEPTO, @FORMA_EMITIDA, @CONTABILIZADO, @CONTABILIZADO_GYP, @COND_PAGO_ID, @SISTEMA_ORIGEN)";
                using (FbCommand command = new FbCommand(insertQuery, conn))
                {
                    command.Parameters.Add("@DOCTO_CP_ID", FbDbType.VarChar).Value = id_docto_cp;
                    command.Parameters.Add("@CONCEPTO_CP_ID", FbDbType.VarChar).Value = "51";
                    command.Parameters.Add("@SUCURSAL_ID", FbDbType.VarChar).Value = "81556";
                    command.Parameters.Add("@FOLIO", FbDbType.VarChar).Value = folio;
                    command.Parameters.Add("@NATURALEZA_CONCEPTO", FbDbType.VarChar).Value = "C";
                    command.Parameters.Add("@FECHA", FbDbType.VarChar).Value = fecha_factura.ToString("dd.MM.yyyy"); 
                    command.Parameters.Add("@CLAVE_PROV", FbDbType.VarChar).Value = info_proveedor.cuenta_cxp;
                    command.Parameters.Add("@PROVEEDOR_ID", FbDbType.VarChar).Value = info_proveedor.id_proveedor_microsip;
                    command.Parameters.Add("@TIPO_CAMBIO", FbDbType.VarChar).Value = tipo_cargo;
                    command.Parameters.Add("@CANCELADO", FbDbType.VarChar).Value = "N";
                    command.Parameters.Add("@APLICADO", FbDbType.VarChar).Value = "S";
                    command.Parameters.Add("@DESCRIPCION", FbDbType.VarChar).Value = descripcion;
                    //command.Parameters.Add("@DESCRIPCION", FbDbType.VarChar).Value = "TESTTTTT VICTOR MASCORRO";
                    command.Parameters.Add("@CUENTA_CONCEPTO", FbDbType.VarChar).Value = cuenta_cargo;
                    command.Parameters.Add("@FORMA_EMITIDA", FbDbType.VarChar).Value = "N";
                    command.Parameters.Add("@CONTABILIZADO", FbDbType.VarChar).Value = "N";
                    command.Parameters.Add("@CONTABILIZADO_GYP", FbDbType.VarChar).Value = "N";
                    command.Parameters.Add("@COND_PAGO_ID", FbDbType.VarChar).Value = id_condicion_pago;
                    command.Parameters.Add("@SISTEMA_ORIGEN", FbDbType.VarChar).Value = "CP";
                    command.ExecuteNonQuery();
                }

                string insertQuery2 = "INSERT INTO IMPORTES_DOCTOS_CP (IMPTE_DOCTO_CP_ID, DOCTO_CP_ID,CANCELADO,APLICADO,TIPO_IMPTE,DOCTO_CP_ACR_ID,IMPORTE,IMPUESTO,IVA_RETENIDO,ISR_RETENIDO,DSCTO_PPAG) VALUES" +
                        " (@IMPTE_DOCTO_CP_ID, @DOCTO_CP_ID, @CANCELADO, @APLICADO, @TIPO_IMPTE, @DOCTO_CP_ACR_ID, @IMPORTE, @IMPUESTO, @IVA_RETENIDO, @ISR_RETENIDO, @DSCTO_PPAG)";
                using (FbCommand command2 = new FbCommand(insertQuery2, conn))
                {
                    command2.Parameters.Add("@IMPTE_DOCTO_CP_ID", FbDbType.VarChar).Value = id_docto_cp_det;
                    command2.Parameters.Add("@DOCTO_CP_ID", FbDbType.VarChar).Value = id_docto_cp;
                    command2.Parameters.Add("@CANCELADO", FbDbType.VarChar).Value = "N";
                    command2.Parameters.Add("@APLICADO", FbDbType.VarChar).Value = "S";
                    command2.Parameters.Add("@TIPO_IMPTE", FbDbType.VarChar).Value = "C";
                    command2.Parameters.Add("@DOCTO_CP_ACR_ID", FbDbType.VarChar).Value = id_docto_cp;
                    command2.Parameters.Add("@IMPORTE", FbDbType.VarChar).Value = importe;
                    command2.Parameters.Add("@IMPUESTO", FbDbType.VarChar).Value = impuesto;
                    command2.Parameters.Add("@IVA_RETENIDO", FbDbType.VarChar).Value = iva_retenido;
                    command2.Parameters.Add("@ISR_RETENIDO", FbDbType.VarChar).Value = isr_retenido;
                    command2.Parameters.Add("@DSCTO_PPAG", FbDbType.VarChar).Value = "0.00";
                    command2.ExecuteNonQuery();
                }

                string insertQuery3 = "INSERT INTO LIBRES_CARGOS_CP (DOCTO_CP_ID, ORDEN_DE_COMPRA_2) VALUES" +
                       " (@DOCTO_CP_ID, @ORDEN_DE_COMPRA_2)";
                using (FbCommand command3 = new FbCommand(insertQuery3, conn))
                {
                    command3.Parameters.Add("@DOCTO_CP_ID", FbDbType.VarChar).Value = id_docto_cp;
                    command3.Parameters.Add("@ORDEN_DE_COMPRA_2", FbDbType.VarChar).Value = factura.id_compras_orden_g.ToString();
                    command3.ExecuteNonQuery();
                }


                string insertQuery4 = "INSERT INTO VENCIMIENTOS_CARGOS_CP (DOCTO_CP_ID, FECHA_VENCIMIENTO, PCTJE_VEN) VALUES" +
                       " (@DOCTO_CP_ID, @FECHA_VENCIMIENTO, @PCTJE_VEN)";
                DateTime fecha_credito = fecha_factura.AddDays(dias_credito);
                using (FbCommand command4 = new FbCommand(insertQuery4, conn))
                {
                    command4.Parameters.Add("@DOCTO_CP_ID", FbDbType.VarChar).Value = id_docto_cp;
                    command4.Parameters.Add("@FECHA_VENCIMIENTO", FbDbType.VarChar).Value = fecha_credito.ToString("dd.MM.yyyy");
                    command4.Parameters.Add("@PCTJE_VEN", FbDbType.VarChar).Value = "100";
                    command4.ExecuteNonQuery();
                }

                using (FbTransaction transaction = conn.BeginTransaction())
                {
                    transaction.Commit();
                }


                return Convert.ToInt32(id_docto_cp);
            }
            catch (Exception ex)
            {
                string ms = ex.ToString();
                return 0;
            }
        }


        public int UnirficarDoctosCPFacturasSIIB()
        {
            int count_modificaciones = 0;
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();

            var facturas = db.C_contabilidad_importacion_facturas_d.Where(x => x.C_compras_facturas_proveedores.activo == true && x.C_compras_facturas_proveedores.aplicado == true
                                                                            && x.id_docto_cp_ms == null && x.activo == true).ToList();
            foreach (var item in facturas)
            {
                //string concepto = item.concepto.Trim();
                string concepto = "O-" + item.C_compras_facturas_proveedores.id_compras_orden_g + " R-"; //+ item.C_compras_facturas_proveedores.C_compras_ordenes_g.C_centros_g.siglas +" "+ item.C_compras_facturas_proveedores.folio_factura +"";
                //string folio_factura = item.C_compras_facturas_proveedores.folio_factura.ToString();
                //string id_proveedor = item.C_compras_facturas_proveedores.id_proveedor_microsip.ToString();
                string fecha_factura = item.C_compras_facturas_proveedores.fecha_registro.Value.ToString("dd.MM.yyyy");
                try
                {
                    var data_factura = new List<object[]>();
                    string query_facturas = "select DOCTO_CP_ID from DOCTOS_CP WHERE DESCRIPCION LIKE '%"+ concepto + "%' AND NATURALEZA_CONCEPTO = 'C'  AND FECHA = @fecha_factura;";  // and FOLIO = '@folio_factura'
                    FbCommand readCommand_co = new FbCommand(query_facturas, conn);
                    //readCommand_co.Parameters.AddWithValue("@descripcion", concepto);
                    //readCommand_co.Parameters.AddWithValue("@id_proveedor", id_proveedor);
                    readCommand_co.Parameters.AddWithValue("@fecha_factura", fecha_factura);
                    //readCommand_co.Parameters.AddWithValue("@folio_factura", folio_factura);
                    FbDataReader reader_co = readCommand_co.ExecuteReader();
                    while (reader_co.Read())
                    {
                        var columns = new object[reader_co.FieldCount];
                        reader_co.GetValues(columns);
                        data_factura.Add(columns);
                    }
                    if (data_factura.Count() > 0)
                    {
                        for (int y = 0; y < data_factura.Count(); y++)
                        {
                            int id_docto = Convert.ToInt32(data_factura[y][0]);
                            item.id_docto_cp_ms = id_docto;
                            db.SaveChanges();
                            count_modificaciones++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                }
            }

            return count_modificaciones;
        }


        public int UnirficarDoctosCOFacturasSIIB()
        {
            int count_modificaciones = 0;
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();

            var facturas = db.C_contabilidad_importacion_facturas_d.Where(x => x.C_compras_facturas_proveedores.activo == true && x.C_compras_facturas_proveedores.aplicado == true
                                                                            && x.id_docto_co_ms == null && x.activo == true).ToList();
            foreach (var item in facturas)
            {
                //string concepto = item.concepto.Trim();
                string concepto = "O-" + item.C_compras_facturas_proveedores.id_compras_orden_g + " R-"; //+ item.C_compras_facturas_proveedores.C_compras_ordenes_g.C_centros_g.siglas +" "+ item.C_compras_facturas_proveedores.folio_factura +"";
                string fecha_factura = item.C_compras_facturas_proveedores.fecha_registro.Value.ToString("dd.MM.yyyy");
                try
                {
                    var data_factura = new List<object[]>();
                    string query_facturas = "SELECT DOCTOS_CO.DOCTO_CO_ID FROM DOCTOS_CO_DET " +
                        " JOIN DOCTOS_CO ON DOCTOS_CO.DOCTO_CO_ID = DOCTOS_CO_DET.DOCTO_CO_ID" +
                        " WHERE DOCTOS_CO.DESCRIPCION LIKE '%" + concepto + "%' AND DOCTOS_CO_DET.TIPO_ASIENTO = 'A' AND DOCTOS_CO.FECHA = @fecha_factura;";  // and FOLIO = '@folio_factura'
                    FbCommand readCommand_co = new FbCommand(query_facturas, conn);
                    readCommand_co.Parameters.AddWithValue("@fecha_factura", fecha_factura);
                    FbDataReader reader_co = readCommand_co.ExecuteReader();
                    while (reader_co.Read())
                    {
                        var columns = new object[reader_co.FieldCount];
                        reader_co.GetValues(columns);
                        data_factura.Add(columns);
                    }
                    if (data_factura.Count() > 0)
                    {
                        for (int y = 0; y < data_factura.Count(); y++)
                        {
                            int id_docto = Convert.ToInt32(data_factura[y][0]);
                            item.id_docto_co_ms = id_docto;
                            db.SaveChanges();
                            count_modificaciones++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msj = ex.ToString();
                }
            }

            UnirficarDoctosCPFacturasSIIB();
            return count_modificaciones;
        }


        public PartialViewResult ConsultarAuditoriaFacturasValoresImportados(string fecha_inicio, string fecha_fin, bool iva, bool iva_ret, bool isr, bool ieps, int[] id_proveedor)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(59);
            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();

            List<AuditoriaFacturasValoresImportacion> data = new List<AuditoriaFacturasValoresImportacion>();

            var facturas_all = db.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true &&
                 x.id_docto_co_ms != null &&
                 x.id_docto_cp_ms != null &&
                 x.C_compras_facturas_proveedores.activo == true &&
                 x.C_compras_facturas_proveedores.aplicado == true &&
                 x.C_compras_facturas_proveedores.fecha_registro >= fecha_i &&
                 x.C_compras_facturas_proveedores.fecha_registro <= fecha_f);
            int count_all = facturas_all.Distinct().Count();

            if (!id_proveedor.Contains(0))
            {
                facturas_all = facturas_all.Where(x => id_proveedor.Contains((int)x.C_compras_facturas_proveedores.id_compras_proveedor));
            }
            int count_prov = facturas_all.Distinct().Count();

            if (iva || iva_ret || isr || ieps)
            {
                facturas_all = facturas_all.Where(x => (iva && x.C_compras_facturas_proveedores.iva_monto > 0) || 
                    (iva_ret && x.C_compras_facturas_proveedores.iva_retenido_monto > 0) ||
                    (isr && x.C_compras_facturas_proveedores.isr_monto > 0) ||
                    (ieps && x.C_compras_facturas_proveedores.ieps_monto > 0) );
            }
            int count_imptos = facturas_all.Distinct().Count();

            foreach (var factura in facturas_all.Distinct())
            {
                try
                {
                    AuditoriaFacturasValoresImportacion obj = new AuditoriaFacturasValoresImportacion();
                    int id_docto_cp = (int)factura.id_docto_cp_ms;
                    obj.id_docto_cp_ms = null;
                    obj.estatus = "IMPORTADA";
                    obj.color = "green";
                    obj.fecha_registro = (DateTime)factura.C_compras_facturas_proveedores.fecha_registro;
                    obj.proveedor = factura.C_compras_facturas_proveedores.C_compras_proveedores.razon_social;
                    obj.cve_proveedor = factura.C_compras_facturas_proveedores.C_compras_proveedores.cve;
                    obj.moneda = factura.C_compras_facturas_proveedores.C_compras_ordenes_g.C_compras_ordenes_d.FirstOrDefault().C_tipos_moneda.clave_fiscal;

                    obj.no_orden = factura.C_compras_facturas_proveedores.id_compras_orden_g.ToString();
                    obj.subtotal = (decimal)factura.C_compras_facturas_proveedores.subtotal;
                    obj.descuento = (decimal)factura.C_compras_facturas_proveedores.descuento;

                    obj.iva_monto = (decimal)factura.C_compras_facturas_proveedores.iva_monto;
                    obj.iva_retenido_monto = (decimal)factura.C_compras_facturas_proveedores.iva_retenido_monto;
                    obj.isr_monto = (decimal)factura.C_compras_facturas_proveedores.isr_monto;
                    obj.ieps_monto = (decimal)factura.C_compras_facturas_proveedores.ieps_monto;
                    obj.total_sib = (decimal)factura.C_compras_facturas_proveedores.total_calculado;
                    if (factura.id_moneda != 1){ obj.total_sib = obj.total_sib * (decimal)factura.tipo_cambio; }  //USD

                    if (factura.activo == false || factura.id_status == 2)
                    {
                        obj.estatus = "RECHAZADA";
                        obj.color = "red";
                    }
                    
                    var data_factura = new List<object[]>();
                    string query_facturas = "select DESCRIPCION,IMPORTE,IMPUESTO,IVA_RETENIDO,ISR_RETENIDO,FOLIO from DOCTOS_CP " +
                        " JOIN IMPORTES_DOCTOS_CP on DOCTOS_CP.DOCTO_CP_ID = IMPORTES_DOCTOS_CP.DOCTO_CP_ID" +
                        " WHERE DOCTOS_CP.DOCTO_CP_ID = @docto_cp_id;";
                    FbCommand readCommand_co = new FbCommand(query_facturas, conn);
                    readCommand_co.Parameters.AddWithValue("@docto_cp_id", id_docto_cp.ToString());
                    FbDataReader reader_co = readCommand_co.ExecuteReader();
                    while (reader_co.Read())
                    {
                        var columns = new object[reader_co.FieldCount];
                        reader_co.GetValues(columns);
                        data_factura.Add(columns);
                    }
                    if (data_factura.Count() > 0)
                    {
                        for (int y = 0; y < data_factura.Count(); y++)
                        {
                            obj.id_docto_cp_ms = id_docto_cp.ToString();
                            obj.concepto_ms = data_factura[y][0].ToString();
                            obj.importe_ms = Convert.ToDecimal(data_factura[y][1]);
                            obj.impuesto_ms = Convert.ToDecimal(data_factura[y][2]);
                            obj.iva_retenido_ms = Convert.ToDecimal(data_factura[y][3]);
                            obj.isr_retenido_ms = Convert.ToDecimal(data_factura[y][4]);
                            obj.folio_ms = data_factura[y][5].ToString();
                            obj.total_ms = obj.importe_ms + obj.impuesto_ms - obj.iva_retenido_ms - obj.isr_retenido_ms;
                            if (factura.id_moneda != 1) { obj.total_ms = obj.total_ms * (decimal)factura.tipo_cambio; } //USD
                        }
                    }

                    try
                    {
                        var data_contabilidad = new List<object[]>();
                        string query_conta = "select IMPORTE from DOCTOS_CO_DET WHERE DOCTO_CO_ID = " + factura.id_docto_co_ms +" AND TIPO_ASIENTO = 'A'";
                        FbCommand readCommand_conta = new FbCommand(query_conta, conn);
                        //readCommand_conta.Parameters.AddWithValue("@docto_cp_id", id_docto_cp.ToString());
                        FbDataReader reader_conta = readCommand_conta.ExecuteReader();
                        while (reader_conta.Read())
                        {
                            var columns = new object[reader_conta.FieldCount];
                            reader_conta.GetValues(columns);
                            data_contabilidad.Add(columns);
                        }
                        if (data_contabilidad.Count() > 0)
                        {
                            for (int i = 0; i < data_contabilidad.Count(); i++)
                            {
                                obj.total_co_ms += Convert.ToDecimal(data_contabilidad[i][0]);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        obj.total_co_ms = 0;
                    }

                    data.Add(obj);
                }
                catch (Exception ex)
                {
                    string mjs = ex.ToString();
                }
            }
            return PartialView("ImportacionFacturas/_AuditoriaFacturasValoresTable", data);
        }



        public ActionResult DownloadExcel(/*string fecha_inicio, string fecha_fin*/ string id_facturas)
        {
            //DateTime fecha_i = DateTime.Parse(fecha_inicio);
            //DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(55);
            //var data = db.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true && x.C_contabilidad_importacion_facturas_g.fecha_registro >= fecha_i && 
            //                                                        x.C_contabilidad_importacion_facturas_g.fecha_registro <= fecha_f).ToList();
            string[] facturas = id_facturas.Split(',');
            int[] id_facturas_generadas = Array.ConvertAll(facturas, int.Parse);

            var data = db.C_contabilidad_importacion_facturas_d.Where(x => x.activo == true && x.C_contabilidad_importacion_facturas_g.activo == true
                                                                        && id_facturas_generadas.Contains((int)x.id_factura_orden)).ToList();
            string htmlContent = RenderPartialViewToString(data);
            byte[] excelBytes = ConvertHtmlTableToExcel(htmlContent);

            Random rand = new Random();
            // Return the Excel file as a download
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IMPORTACION_FACTURAS.xlsx");
        }

        private string RenderPartialViewToString(object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, "ImportacionFacturas/_ExcelDetalleImportacion" /*"_ExcelImportacionesFacturas"*/);
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

        //--------------------------------------------------------------
        #endregion


        #region HISTORIAL DE FACTURAS
        //---------------- HISTORIAL DE FACTURAS
        public PartialViewResult ConsultarImportacionHistorial(string fecha_inicio, string fecha_fin)
        {
            DateTime fecha_i = DateTime.Parse(fecha_inicio);
            DateTime fecha_f = DateTime.Parse(fecha_fin).AddHours(23).AddMinutes(55);
            var importaciones = db.C_contabilidad_importacion_facturas_g.Where(x => x.activo == true && x.fecha_registro >= fecha_i && x.fecha_registro <= fecha_f).ToList();
            return PartialView("ImportacionFacturas/_ImportacionesHistorialTable", importaciones);
        }

        public PartialViewResult ConsultarImportacionDetalle(int id_importacion_g)
        {
            var detalle_importacion = db.C_contabilidad_importacion_facturas_d.Where(x => x.id_contabilidad_importacion_g == id_importacion_g && x.activo == true).ToList();
            return PartialView("ImportacionFacturas/_ImportacionDetalle", detalle_importacion);
        }
        //--------------------------------------------------------------
        #endregion



        #region SOLICITUDES DE PAGO
        //-------------------- SOLICITUDES DE PAGO
        public ActionResult SolicitudesPago()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(7036)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception) { RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }

            return View("SolicitudesPago/Index");
        }

        public PartialViewResult ConsultarPagosProgramados(string fecha_inicio, string fecha_fin, string clave_fiscal_ba)
        {
            List<SolicitudPagoDetalleMS> DetallePago = new List<SolicitudPagoDetalleMS>();

            conn = ConexionMS.GetConexionMS().CrearConexion();
            conn.Open();

            var ultima_cuenta = db.C_contabilidad_solicitudes_pago_g.Where(x => x.activo == true).OrderByDescending(x => x.id_solicitud_pago_g).FirstOrDefault();
            ViewBag.ultima_cuenta = ultima_cuenta != null ? ultima_cuenta.numero_cuenta : "";
            ViewBag.referencia = ultima_cuenta != null ? ultima_cuenta.referencia : "";

            var data_doctos_ba = new List<object[]>();
            var data_conceptos_ba = new List<object[]>();
            var data_monedas = new List<object[]>();
            var data_beneficiarios = new List<object[]>();
            var data_doctos_entre_sis = new List<object[]>();
            var data_doctos_cp = new List<object[]>();
            var data_proveedores = new List<object[]>();
            try
            {
                string query_docto_ba = "select DOCTO_BA_ID, CONCEPTO_BA_ID, MONEDA_ID, BENEFICIARIO_ID, FECHA, REFER, IMPORTE, DESCRIPCION from doctos_ba where tipo_movto='R' and estatus='P' and cancelado='N' and fecha>= @fecha_inicio AND fecha <= @fecha_fin order by docto_ba_id";
                FbCommand readCommand_ba = new FbCommand(query_docto_ba, conn);
                readCommand_ba.Parameters.AddWithValue("@fecha_inicio", fecha_inicio);
                readCommand_ba.Parameters.AddWithValue("@fecha_fin", fecha_fin);
                FbDataReader reader_ba = readCommand_ba.ExecuteReader();
                while (reader_ba.Read())
                {
                    var columns = new object[reader_ba.FieldCount];
                    reader_ba.GetValues(columns);
                    data_doctos_ba.Add(columns);
                }
                if (data_doctos_ba.Count() > 0)
                {
                    for (int i = 0; i < data_doctos_ba.Count(); i++)
                    {
                        data_conceptos_ba.Clear();
                        data_monedas.Clear();
                        data_beneficiarios.Clear();
                        data_doctos_entre_sis.Clear();
                        data_doctos_cp.Clear();
                        data_proveedores.Clear();


                        int id_docto_ba = Convert.ToInt32(data_doctos_ba[i][0]);
                        int id_concepto_ba = Convert.ToInt32(data_doctos_ba[i][1]);
                        int id_moneda_ba = Convert.ToInt32(data_doctos_ba[i][2]);
                        int id_beneficiario_ba = Convert.ToInt32(data_doctos_ba[i][3]);

                        string fecha_ba = data_doctos_ba[i][4].ToString().Trim();
                        string referecia = data_doctos_ba[i][5].ToString().Trim();
                        string importe = data_doctos_ba[i][6].ToString().Trim();
                        string descripcion = data_doctos_ba[i][7].ToString().Trim();

                        bool valid_docto = true;
                        int id_docto_cp = 0;
                        string nombre_concepto = "";
                        string nombre_moneda = "";
                        string nombre_beneficiario = "";
                        string cuenta_interbancaria = "";
                        string tipo_persona = "";
                        string nombre_pf = "";

                        //VALIDAR QUE EL id_docto_ba NO EXISTA EN LA TABLA (C_contabilidad_solicitudes_pago_d) QUE NO ESTÉ ACTIVO NI AUTORIZADO
                        //var solicitud_pago_d = db_master.SolPagoDetalle.Where(x => x.SolPagoDetalleDocto_Ba_Id == id_docto_ba && x.SolPagoDetalleRstatus != "E" &&
                        //(x.SolPagoDetalleAuto1 < 3 || x.SolPagoDetalleAuto2 < 3)).FirstOrDefault();
                        //if (solicitud_pago_d != null) { valid_docto = false; }

                        var solicitud_pago_d = db.C_contabilidad_solicitudes_pago_d.Where(x => x.activo == true && x.C_contabilidad_solicitudes_pago_g.activo == true &&
                                                            x.id_docto_ba == id_docto_ba).FirstOrDefault();
                        if (solicitud_pago_d != null) { valid_docto = false; }

                        if (valid_docto == true)
                        {
                            //--------VALIDO QUE EL CONCEPTO SEA "TRASNFERENCIA"
                            bool valid_concepto = false;
                            string query_co_ba = "select CONCEPTO_BA_ID, NOMBRE, NOMBRE_ABREV from conceptos_ba Where concepto_ba_id = @id_concepto_ba";
                            FbCommand readCommand_co = new FbCommand(query_co_ba, conn);
                            readCommand_co.Parameters.AddWithValue("@id_concepto_ba", id_concepto_ba);
                            FbDataReader reader_co = readCommand_co.ExecuteReader();
                            while (reader_co.Read())
                            {
                                var columns = new object[reader_co.FieldCount];
                                reader_co.GetValues(columns);
                                data_conceptos_ba.Add(columns);
                            }
                            if (data_conceptos_ba.Count() > 0)
                            {
                                for (int y = 0; y < data_conceptos_ba.Count(); y++)
                                {
                                    int id_concepto = Convert.ToInt32(data_conceptos_ba[y][0]);
                                    nombre_concepto = data_conceptos_ba[y][1].ToString().Trim();
                                    if (id_concepto == id_concepto_ba)
                                    {
                                        if (nombre_concepto.Contains("TRANSFE") || nombre_concepto.Contains("Transfe")) { valid_concepto = true; }
                                    }
                                }
                            }

                            //--------- VALIDO QUE LA MONEDA SEA IGUAL A LA DEL PARAMETRO
                            bool valid_moneda = false;
                            string query_moneda = "select MONEDA_ID, CLAVE_FISCAL, NOMBRE from monedas Where moneda_id= @id_moneda";
                            FbCommand readCommand_mon = new FbCommand(query_moneda, conn);
                            readCommand_mon.Parameters.AddWithValue("@id_moneda", id_moneda_ba);
                            FbDataReader reader_mon = readCommand_mon.ExecuteReader();
                            while (reader_mon.Read())
                            {
                                var columns = new object[reader_mon.FieldCount];
                                reader_mon.GetValues(columns);
                                data_monedas.Add(columns);
                            }
                            if (data_monedas.Count() > 0)
                            {
                                for (int z = 0; z < data_monedas.Count(); z++)
                                {
                                    int id_moneda = Convert.ToInt32(data_monedas[z][0]);
                                    if (id_moneda == id_moneda_ba)
                                    {
                                        string clave_fiscal = data_monedas[z][1].ToString().Trim();
                                        if (clave_fiscal == clave_fiscal_ba)
                                        {
                                            nombre_moneda = data_monedas[z][2].ToString().Trim();
                                            valid_moneda = true;
                                        }
                                    }
                                }
                            }

                            //PROCEDO A IR POR LOS DATOS DE: BENEFICIARIO Y PROVEEDOR
                            if (valid_concepto == true && valid_moneda == true)
                            {
                                string query_beneficiario = "select NOMBRE from beneficiarios where beneficiario_id= @id_beneficiario";
                                FbCommand readCommand_bene = new FbCommand(query_beneficiario, conn);
                                readCommand_bene.Parameters.AddWithValue("@id_beneficiario", id_beneficiario_ba);
                                FbDataReader reader_bene = readCommand_bene.ExecuteReader();
                                while (reader_bene.Read())
                                {
                                    var columns = new object[reader_bene.FieldCount];
                                    reader_bene.GetValues(columns);
                                    data_beneficiarios.Add(columns);
                                }
                                if (data_beneficiarios.Count() > 0) { nombre_beneficiario = data_beneficiarios[0][0].ToString().Trim(); }

                                string query_docto_entre_sis = "select DOCTO_FTE_ID from doctos_entre_sis Where docto_dest_id= @id_docto";
                                FbCommand readCommand_docto = new FbCommand(query_docto_entre_sis, conn);
                                readCommand_docto.Parameters.AddWithValue("@id_docto", id_docto_ba);
                                FbDataReader reader_docto = readCommand_docto.ExecuteReader();
                                while (reader_docto.Read())
                                {
                                    var columns = new object[reader_docto.FieldCount];
                                    reader_docto.GetValues(columns);
                                    data_doctos_entre_sis.Add(columns);
                                }
                                if (data_doctos_entre_sis.Count() > 0)
                                {
                                    id_docto_cp = Convert.ToInt32(data_doctos_entre_sis[0][0]);
                                    string query_docto = "select PROVEEDOR_ID, FOLIO from doctos_cp Where docto_cp_id= @id_docto";
                                    FbCommand readCommand_docto_cp = new FbCommand(query_docto, conn);
                                    readCommand_docto_cp.Parameters.AddWithValue("@id_docto", id_docto_cp);
                                    FbDataReader reader_docto_cp = readCommand_docto_cp.ExecuteReader();
                                    while (reader_docto_cp.Read())
                                    {
                                        var columns = new object[reader_docto_cp.FieldCount];
                                        reader_docto_cp.GetValues(columns);
                                        data_doctos_cp.Add(columns);
                                    }
                                    if (data_doctos_cp.Count() > 0)
                                    {
                                        int id_proveedor = Convert.ToInt32(data_doctos_cp[0][0]);
                                        referecia = data_doctos_cp[0][1].ToString().Trim();

                                        string query_proveedor = "select CLAVE_INTERBANCARIA, TIPO_DE_PERSONA, NOMBRE_PERSONA_FISICA from libres_proveedor Where proveedor_id= @id_proveedor";
                                        FbCommand readCommand_proveedor = new FbCommand(query_proveedor, conn);
                                        readCommand_proveedor.Parameters.AddWithValue("@id_proveedor", id_proveedor);
                                        FbDataReader reader_proveedor = readCommand_proveedor.ExecuteReader();
                                        while (reader_proveedor.Read())
                                        {
                                            var columns = new object[reader_proveedor.FieldCount];
                                            reader_proveedor.GetValues(columns);
                                            data_proveedores.Add(columns);
                                        }
                                        if (data_proveedores.Count() > 0)
                                        {
                                            string tipo_persona_libre = data_proveedores[0][1].ToString().Trim();
                                            if (tipo_persona_libre == "2856270") { tipo_persona = "Fisica"; }
                                            else if (tipo_persona_libre == "2856271") { tipo_persona = "Moral"; }
                                            cuenta_interbancaria = data_proveedores[0][0].ToString().Trim();
                                            nombre_pf = data_proveedores[0][2].ToString().Trim();
                                        }

                                    }
                                }
                                SolicitudPagoDetalleMS solicitud = new SolicitudPagoDetalleMS();
                                solicitud.id_docto_ba = id_docto_ba.ToString().Trim();
                                solicitud.id_concepto_ba = id_concepto_ba.ToString().Trim();
                                solicitud.nombre_concepto = nombre_concepto;
                                solicitud.fecha_ba = fecha_ba;
                                solicitud.referencia = referecia;
                                solicitud.importe = importe;
                                solicitud.moneda_id = id_moneda_ba.ToString();
                                solicitud.moneda_nombre = nombre_moneda;
                                solicitud.moneda_clave_fiscal = clave_fiscal_ba;
                                solicitud.descripcion = descripcion;
                                solicitud.beneficiario_id = id_beneficiario_ba.ToString();
                                solicitud.beneficiario_nombre = nombre_beneficiario;

                                solicitud.prov_cuenta_bancaria = cuenta_interbancaria;
                                solicitud.prov_tipo_persona = tipo_persona;
                                solicitud.prov_nombre_pf = nombre_pf;

                                if (data_doctos_cp.Count() > 0 && id_docto_cp != 0) { solicitud.id_docto_cp = id_docto_cp.ToString(); }
                                DetallePago.Add(solicitud);
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return PartialView("SolicitudesPago/_PagosPendientesMS", null);
            }

            return PartialView("SolicitudesPago/_PagosPendientesMS", DetallePago.OrderBy(x => x.beneficiario_nombre).ToList());

        }

        public int GuardarSolicitudPagos(C_contabilidad_solicitudes_pago_g sol_g, string fech_sol, int[] id_doc_ba, int[] id_doc_cp, int[] conceptos_id, string[] conceptos, int[] monedas_id,
        string[] monedas_cf, int[] benef_id, string[] benef, string[] cta_bancaria, string[] tipo_persona, string[] referencias, string[] nombre_pf, decimal[] importes, string[] descripciones)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                DateTime hoy = DateTime.Now;
                DateTime fecha_solicitud = DateTime.Parse(fech_sol);

                sol_g.fecha_registro = hoy;
                sol_g.fecha_solicitud = fecha_solicitud;
                sol_g.id_usuario_registra = id_usuario;
                sol_g.generacion_txt_interbancario = 0;
                sol_g.numero_cuenta = sol_g.numero_cuenta.Trim();
                db.C_contabilidad_solicitudes_pago_g.Add(sol_g);
                db.SaveChanges();
                int id_solciitud_g = sol_g.id_solicitud_pago_g;

                for (int i = 0; i < id_doc_ba.Length; i++)
                {
                    C_contabilidad_solicitudes_pago_d detalle = new C_contabilidad_solicitudes_pago_d();
                    detalle.id_solicitud_pago_g = id_solciitud_g;
                    detalle.id_docto_ba = id_doc_ba[i];
                    detalle.id_docto_cp = id_doc_cp[i];
                    detalle.fecha_registro = hoy;
                    detalle.id_concepto = conceptos_id[i];
                    detalle.concepto_nombre = conceptos[i];
                    detalle.referencia = referencias[i];
                    detalle.importe_detalle = importes[i];

                    var clave_fical = monedas_cf[i];
                    if (clave_fical == "MXN") { detalle.moneda_nombre = "Moneda nacional"; }
                    else { detalle.moneda_nombre = "Dolares"; }
                    detalle.id_moneda = monedas_id[i];
                    detalle.moneda_clave_fiscal = clave_fical;

                    detalle.id_beneficiario = benef_id[i];
                    detalle.beneficiario_nombre = benef[i];
                    detalle.cuenta_bancaria = cta_bancaria[i];
                    detalle.nombre_pf = nombre_pf[i];
                    detalle.tipo_persona = tipo_persona[i];
                    detalle.descripcion = descripciones[i];
                    detalle.activo = true;
                    db.C_contabilidad_solicitudes_pago_d.Add(detalle);
                }

                try { db.SaveChanges(); }
                catch (Exception)
                {
                    sol_g.activo = false;
                    db.SaveChanges();
                }



                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public PartialViewResult ConsultarSolicitudesPendientesValidarAutorizar(int modo, int id_status)
        {
            var solicitudes = db.C_contabilidad_solicitudes_pago_g.Where(x => x.id_solicitudes_pago_status == id_status && x.activo == true && x.C_contabilidad_solicitudes_pago_d.Count() > 0).OrderByDescending(x => x.id_solicitud_pago_g).ToList();
            if (id_status == 3)
            {
                DateTime fecha_inicio = DateTime.Today.AddMonths(-3);
                solicitudes = db.C_contabilidad_solicitudes_pago_g.Where(x => x.id_solicitudes_pago_status == id_status && x.activo == true && x.C_contabilidad_solicitudes_pago_d.Count() > 0 && x.fecha_registro >= fecha_inicio).OrderByDescending(x => x.id_solicitud_pago_g).ToList();
            }
            ViewBag.modo = modo;

            return PartialView("SolicitudesPago/_SolicitudesPendientesFirmar", solicitudes);

        }

        public PartialViewResult ConsultarSolicitudPagoDetalle(int modo, int id_solicitud_g)
        {
            ViewBag.modo = modo;
            var detalle = db.C_contabilidad_solicitudes_pago_d.Where(x => x.id_solicitud_pago_g == id_solicitud_g).ToList();
            return PartialView("SolicitudesPago/_DetalleSolicitudPago", detalle);
        }

        public int ValidarAutorizarSolicitud(int id_solicitud_g, int modo, bool accion, int[] id_solicitud_d, bool[] valores)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var solicitud = db.C_contabilidad_solicitudes_pago_g.Find(id_solicitud_g);
                if (accion == false)
                {
                    solicitud.id_solicitudes_pago_status = 4; //ELIMINADA
                    solicitud.activo = false;
                    db.C_contabilidad_solicitudes_pago_d.Where(x => x.id_solicitud_pago_g == id_solicitud_g).ToList().ForEach(x => x.activo = false);
                    db.SaveChanges();
                    return 0;
                }
                else
                {
                    decimal importe = 0;
                    for (int i = 0; i < id_solicitud_d.Length; i++)
                    {
                        int id_solicitud = id_solicitud_d[i];
                        bool activo = valores[i];
                        var soli_d = db.C_contabilidad_solicitudes_pago_d.Find(id_solicitud);
                        soli_d.activo = activo;
                        if (activo == true) { importe += (decimal)soli_d.importe_detalle; }

                        db.SaveChanges();
                    }

                    //VALIDAR
                    if (modo == 1)
                    {
                        solicitud.id_solicitudes_pago_status = 2;
                        solicitud.id_usuario_aut1 = id_usuario;
                    }

                    //AUTORIZAR
                    else
                    {
                        solicitud.id_solicitudes_pago_status = 3;
                        solicitud.id_usuario_aut2 = id_usuario;
                    }
                    solicitud.importe = importe;
                }
                db.SaveChanges();

                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public string GenerarTXTInterbancarioSolicitudPago(int id_solicitud_g)
        {
            try
            {
                var solicitud_d = db.C_contabilidad_solicitudes_pago_d.Where(x => x.id_solicitud_pago_g == id_solicitud_g && x.activo == true).ToList();
                if (solicitud_d == null) { return "-1"; }  //NO SE ENCONTRÓ EL DETALLE
                if (solicitud_d.Count() == 0) { return "-2"; }  //NO HAY PAGOS ACTIVOS
                if (solicitud_d.FirstOrDefault().C_contabilidad_solicitudes_pago_g == null) { return "-3"; } //NO SE ENCONTRÓ EL HEADER DE LA SOLICITUD

                var valid_exst = solicitud_d.Where(x => !x.cuenta_bancaria.Contains("/"));  //SI NO CONTIENE "/" ES PAGO A NUESTRO BANCO NO A TERCEROS
                if (valid_exst.Count() == 0) { return "-4"; }  //NO HAY PAGOS A NUESTRO BANCO


                var solicitud_g = db.C_contabilidad_solicitudes_pago_g.Find(id_solicitud_g);
                if (solicitud_g.id_solicitudes_pago_status != 3) { return "-5"; }

                string cuenta_origen = solicitud_g.numero_cuenta;
                DateTime fecha_solicitud = (DateTime)solicitud_g.fecha_solicitud;
                string dia_soli = fecha_solicitud.Day.ToString().Trim().PadLeft(2, '0');
                string mes_soli = fecha_solicitud.Month.ToString().Trim().PadLeft(2, '0');
                string anio_soli = fecha_solicitud.Year.ToString().Trim().Substring(2);
                string referencia = "0" + dia_soli + mes_soli + anio_soli;

                string inicial_cta = cuenta_origen.Substring(0, 3);
                string restante_cta = "";
                try
                {
                    restante_cta = cuenta_origen.Substring(3).PadLeft(20, '0');
                }
                catch (Exception)
                {
                    return "-6";  //ERROR EN LA CLAVE INTERBANCARIA (DIGITOS ERRONEOS)
                }
                Random rand = new Random();
                int random_number = rand.Next(0, 999);
                int generacion_consecutivo = (int)solicitud_g.generacion_txt_interbancario++;
                string filePath = "\\\\192.168.128.2\\inetpub\\SolicitudesPago\\TXT_Interbancarios\\IPrg-" + referencia + "-" + generacion_consecutivo.ToString().PadLeft(2, '0') + "-" + random_number + ".txt";


                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    foreach (var cuenta in solicitud_d)
                    {
                        string nombre_bene = "";
                        try
                        {
                            if (cuenta.cuenta_bancaria.Contains("/")) { }
                            else
                            {
                                int entero_importe = (int)cuenta.importe_detalle;
                                decimal decimales = ((decimal)cuenta.importe_detalle - entero_importe) * 100;
                                int entero_decimal = Convert.ToInt32(decimales);
                                string importe_total = entero_importe.ToString().Trim().PadLeft(12, '0') + entero_decimal.ToString().Trim().PadLeft(2, '0');
                                string cta_destino = cuenta.cuenta_bancaria.Trim().PadLeft(18, '0');

                                string nombre_beneficiario = "";
                                int length_nombre_beneficiario = cuenta.beneficiario_nombre.Trim().Length;

                                nombre_bene = cuenta.beneficiario_nombre.Trim();
                                if (cuenta.beneficiario_nombre == "ASESORIA Y SERVICIOS AGROPECUARIOS DE LA LAGUNA S.A. DE C.V.") {
                                    string stop1 = "";
                                }

                                if (cuenta.tipo_persona == "Fisica")
                                {
                                    if (cuenta.nombre_pf.Trim().Length > 0) { nombre_beneficiario = cuenta.nombre_pf.ToUpper(); }
                                    else { nombre_beneficiario = cuenta.beneficiario_nombre; }
                                    length_nombre_beneficiario = nombre_beneficiario.Length;
                                    if (length_nombre_beneficiario < 55)
                                    {
                                        int relleno = 55 - length_nombre_beneficiario;
                                        nombre_beneficiario = nombre_beneficiario + new string(' ', relleno);
                                    }
                                    if (length_nombre_beneficiario > 55)
                                    {
                                        nombre_beneficiario.Substring(1, 55);
                                    }
                                }
                                else
                                {
                                    for (int z = 0; z < length_nombre_beneficiario; z++)
                                    {
                                        if (cuenta.beneficiario_nombre.Substring(z, 1) != "." && cuenta.beneficiario_nombre.Substring(z, 1) != ",")
                                        {
                                            if (cuenta.beneficiario_nombre.Substring(z, 1) == "Ñ" || cuenta.beneficiario_nombre.Substring(z, 1) == "ñ")
                                            {
                                                nombre_beneficiario = nombre_beneficiario + "N";
                                            }
                                            else
                                            {
                                                nombre_beneficiario = nombre_beneficiario + cuenta.beneficiario_nombre.Substring(z, 1);
                                            }
                                        }
                                        else
                                        {
                                            nombre_beneficiario = nombre_beneficiario + "*";
                                        }
                                    }
                                    length_nombre_beneficiario = nombre_beneficiario.Length;
                                    length_nombre_beneficiario++;
                                    if (length_nombre_beneficiario < 54)
                                    {
                                        int relleno = 54 - length_nombre_beneficiario;
                                        nombre_beneficiario += "/";
                                        nombre_beneficiario = nombre_beneficiario + new string(' ', relleno);
                                    }
                                    if (length_nombre_beneficiario > 54)
                                    {
                                        nombre_beneficiario = nombre_beneficiario.Substring(0, 53) + "/";
                                    }
                                }
                                string cadena = "";
                                cadena += "09010";
                                cadena += inicial_cta;
                                cadena += restante_cta;
                                cadena += importe_total;
                                cadena += "0014000";
                                cadena += cta_destino;
                                cadena += "PAGO A PROVEEDOR                        ";  //40 CARACTERES
                                cadena += referencia;
                                if (cuenta.tipo_persona == "Moral") { cadena += ","; }
                                cadena += nombre_beneficiario;
                                cadena += "00";
                                cadena += "              ";
                                cadena += "0000000000000";
                                cadena += cuenta.cuenta_bancaria.Trim().Substring(0, 3);
                                cadena += "0000000000";
                                writer.WriteLine(cadena);
                            }
                        }
                        catch (Exception)
                        {
                            string bene_error = nombre_bene;
                        }
                    }
                    db.SaveChanges();
                }
                return "https://siib.beta.com.mx/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");

                //return "https://localhost:44371/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");
            }
            catch (Exception ex)
            {
                string nombre_bene;
                string msj = ex.ToString();
                return "1";
            }
        }

        public string GenerarTXTTercerosSolicitudPago(int id_solicitud_g)
        {
            try
            {
                var solicitud_d = db.C_contabilidad_solicitudes_pago_d.Where(x => x.id_solicitud_pago_g == id_solicitud_g && x.activo == true).ToList();
                var valid_exst = solicitud_d.Where(x => x.cuenta_bancaria.Contains("/"));
                if (valid_exst.Count() == 0) { return "-1"; }  //NO HAY PAGOS A TERCEROS

                if (solicitud_d == null) { return "-2"; }  //NO SE ENCONTRÓ EL DETALLE
                if (solicitud_d.Count() == 0) { return "-3"; }  //NO HAY PAGOS ACTIVOS

                var solicitud_g = db.C_contabilidad_solicitudes_pago_g.Find(id_solicitud_g);
                if (solicitud_g.id_solicitudes_pago_status != 3) { return "-4"; }  //LA SOLICITUD NO ESTÁ AUTORIZADA

                string cuenta_origen = solicitud_g.numero_cuenta;
                DateTime fecha_solicitud = (DateTime)solicitud_g.fecha_solicitud;
                string dia_soli = fecha_solicitud.Day.ToString().Trim().PadLeft(2, '0');
                string mes_soli = fecha_solicitud.Month.ToString().Trim().PadLeft(2, '0');
                string anio_soli = fecha_solicitud.Year.ToString().Trim().Substring(2);
                string referencia = "0" + dia_soli + mes_soli + anio_soli;

                string inicial_cta = cuenta_origen.Substring(0, 3);
                string restante_cta = "";
                try
                {
                    restante_cta = cuenta_origen.Substring(3).PadLeft(20, '0');
                }
                catch (Exception)
                {
                    return "-5";  //ERROR EN LA CLAVE INTERBANCARIA (DIGITOS ERRONEOS)
                }
                Random rand = new Random();
                int random_number = rand.Next(0, 999);
                int generacion_consecutivo = (int)solicitud_g.generacion_txt_interbancario++;
                solicitud_g.generacion_txt_interbancario = generacion_consecutivo;

                string filePath = "\\\\192.168.128.2\\inetpub\\SolicitudesPago\\TXT_Terceros\\TPrg-" + referencia + "-" + generacion_consecutivo.ToString().PadLeft(2, '0') + "-" + random_number + ".txt";
                
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    foreach (var cuenta in solicitud_d)
                    {
                        string nombre_bene = "";
                        try
                        {
                            if (cuenta.cuenta_bancaria.Contains("/"))
                            {
                                int entero_importe = (int)cuenta.importe_detalle;
                                decimal decimales = ((decimal)cuenta.importe_detalle - entero_importe) * 100;
                                int entero_decimal = Convert.ToInt32(decimales);
                                string importe_total = entero_importe.ToString().Trim().PadLeft(12, '0') + entero_decimal.ToString().Trim().PadLeft(2, '0');

                                //CUENTA SUCURSAL
                                int count_z = 0;
                                int cta_destino_suc_lenght = cuenta.cuenta_bancaria.Trim().Length;
                                string cta_destino_suc = "";
                                string cta_destino = "";
                                for (int z = 0; z < cta_destino_suc_lenght; z++)
                                {
                                    if (cuenta.cuenta_bancaria.Trim().Substring(z, 1) != "/") { cta_destino_suc += cuenta.cuenta_bancaria.Trim().Substring(z, 1); count_z++; }
                                    else { z = cta_destino_suc_lenght; } //FIN DEL FOR, SOLO NECESITO LOS PRIMEROS DIGITOS ANTES DEL "/"
                                }
                                cta_destino_suc = cta_destino_suc.Trim().PadLeft(4, '0');
                                if (count_z < cta_destino_suc_lenght)
                                {
                                    count_z++;
                                    cta_destino = cuenta.cuenta_bancaria.Substring(count_z).Trim().PadLeft(20, '0');
                                }
                                else { cta_destino = cuenta.cuenta_bancaria.Substring(count_z).Trim().PadLeft(20, '0'); }


                                //BENEFICIARIO
                                nombre_bene = cuenta.beneficiario_nombre.Trim();
                                if (cuenta.beneficiario_nombre == "LUIS ZAVALA FLORES")
                                {
                                    string stop1 = "";
                                }
                                string nombre_beneficiario = "";
                                int length_nombre_beneficiario = cuenta.beneficiario_nombre.Trim().Length;
                                for (int z = 0; z < length_nombre_beneficiario; z++)
                                {
                                    if (cuenta.beneficiario_nombre.Substring(z, 1) != "." && cuenta.beneficiario_nombre.Substring(z, 1) != ",")
                                    {
                                        if (cuenta.beneficiario_nombre.Substring(z, 1) == "Ñ" || cuenta.beneficiario_nombre.Substring(z, 1) == "ñ")
                                        {
                                            nombre_beneficiario = nombre_beneficiario + "N";
                                        }
                                        else
                                        {
                                            nombre_beneficiario = nombre_beneficiario + cuenta.beneficiario_nombre.Substring(z, 1);
                                        }
                                    }
                                    else
                                    {
                                        nombre_beneficiario = nombre_beneficiario + "*";
                                    }
                                }
                                length_nombre_beneficiario = nombre_beneficiario.Length;
                                length_nombre_beneficiario++;
                                if (length_nombre_beneficiario < 20)
                                {
                                    int relleno = 21 - length_nombre_beneficiario;
                                    nombre_beneficiario = nombre_beneficiario + new string(' ', relleno);
                                }
                                if (length_nombre_beneficiario == 20)
                                {
                                    int relleno = 21 - length_nombre_beneficiario;
                                    nombre_beneficiario = nombre_beneficiario + new string(' ', relleno);
                                    nombre_beneficiario = nombre_beneficiario.Substring(0, 20);
                                }
                                if (length_nombre_beneficiario > 20)
                                {
                                    nombre_beneficiario = nombre_beneficiario.Substring(0, 20);
                                }

                                string cadena = "";
                                cadena += "03010";
                                cadena += inicial_cta;
                                cadena += restante_cta;
                                cadena += "01";
                                cadena += cta_destino_suc;
                                cadena += cta_destino;
                                cadena += importe_total;
                                cadena += "001";
                                cadena += nombre_beneficiario;
                                cadena += "    ";
                                cadena += "PAGO A PROVEEDOR                  ";  //34 CARACTERES
                                cadena += "000";
                                cadena += referencia;
                                cadena += "0000000000000";
                                writer.WriteLine(cadena);
                            }
                            else { string hola = ""; }
                        }
                        catch (Exception ex)
                        {
                            string msj = ex.ToString();
                            string bene_error = nombre_bene;
                        }
                    }
                    db.SaveChanges();
                }
                return "https://siib.beta.com.mx/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");

                //return "https://localhost:44371/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return "-1";
            }
        }


        public ActionResult DescargarTXT_Path(string path)
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

        public int EliminarSolicitudPago(int id_solicitud_g)
        {
            try
            {
                var soli = db.C_contabilidad_solicitudes_pago_g.Find(id_solicitud_g);
                if (soli.generacion_txt_interbancario == 0)
                {
                    soli.activo = false;
                    soli.id_solicitudes_pago_status = 3; //ELIMINADA
                    db.C_contabilidad_solicitudes_pago_d.Where(x => x.id_solicitud_pago_g == id_solicitud_g).ToList().ForEach(z => z.activo = false);
                    db.SaveChanges();
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        #endregion


        #region ORDENES DE PAGO
        public ActionResult OrdenesPagoCheques()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8081)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception) { RedirectToAction("UsuarioLogin", "USUARIOLOGIN"); }

            return View("OrdenesPago/Index");
        }

        public PartialViewResult ConsultarOrdenesPago()
        {
            DateTime hoy = DateTime.Today.AddMonths(-3);
            var ordenes = db.C_contabilidad_ordenes_pago_g.Where(x => x.activo == true && x.fecha_registro >= hoy).OrderBy(x => x.fecha_registro).ToList();
            return PartialView("OrdenesPago/_OrdenesPagoGeneradasTable", ordenes);
        }

        public PartialViewResult ConsultarOrdenPago(int id_contabilidad_orden_pago_g)
        {
            var data = db.C_contabilidad_ordenes_pago_d.Where(x => x.activo == true && x.id_contabilidad_orden_pago_g == id_contabilidad_orden_pago_g).ToList();
            return PartialView("OrdenesPago/_OrdenPagoDetalle", data);
        }

        public int AgregarBeneficiarioOrden(C_contabilidad_ordenes_pago_g orden_g, string fecha_aplicacion, string hra, string minutos, 
            string[] ordenes_tipo, string[] nombre_bene, string[] sucursal_destino, decimal[] importes, string[] instrucciones, string[] remitente)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                DateTime hoy = DateTime.Now;
                DateTime fecha_apli = DateTime.Parse(fecha_aplicacion);
                string hra_format = hra.PadLeft(2, '0');
                string min_format = minutos.PadLeft(2, '0');

                orden_g.fecha_aplicacion = fecha_apli;
                orden_g.hora_aplicacion = hra_format + min_format;
                orden_g.id_usuario_registra = id_usuario;
                orden_g.fecha_registro = hoy;
                orden_g.activo = true;
                db.C_contabilidad_ordenes_pago_g.Add(orden_g);
                db.SaveChanges();
                int id_orden_g = orden_g.id_contabilidad_orden_pago_g;
                for (int i = 0; i < ordenes_tipo.Length; i++)
                {
                    try
                    {
                        C_contabilidad_ordenes_pago_d orden_d = new C_contabilidad_ordenes_pago_d();
                        orden_d.id_contabilidad_orden_pago_g = id_orden_g;
                        orden_d.tipo_transaccion = "05";  //ORDEN DE PAGO
                        orden_d.tipo_cuenta_origen = ordenes_tipo[i];
                        orden_d.sucursal_destino = sucursal_destino[i];
                        orden_d.importe = importes[i];
                        orden_d.tipo_moneda = "001";
                        orden_d.nombre_beneficiario = nombre_bene[i];
                        orden_d.remitente = remitente[i];
                        orden_d.instrucciones = instrucciones[i];
                        orden_d.activo = true;
                        db.C_contabilidad_ordenes_pago_d.Add(orden_d);
                    }
                    catch (Exception)
                    {
                        orden_g.activo = false;
                        db.C_contabilidad_ordenes_pago_d.Where(x => x.id_contabilidad_orden_pago_g == id_orden_g).ToList().ForEach(x => x.activo = false);
                        db.SaveChanges();
                        return 2; 
                    }
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public int EliminarBeneficiarioOrden(int id_contabilidad_orden_pago_d)
        {
            try
            {
                var detalle = db.C_contabilidad_ordenes_pago_d.Find(id_contabilidad_orden_pago_d);
                detalle.activo = false;
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }


        public string GenerarTXTOrdenPago(int id_contabilidad_orden_pago_g)
        {
            var orden_g = db.C_contabilidad_ordenes_pago_g.Find(id_contabilidad_orden_pago_g);

            string dia_soli = orden_g.fecha_aplicacion.Value.Day.ToString().Trim().PadLeft(2, '0');
            string mes_soli = orden_g.fecha_aplicacion.Value.Month.ToString().Trim().PadLeft(2, '0');
            string anio_soli = orden_g.fecha_aplicacion.Value.Year.ToString().Trim().Substring(2);
            string fecha_aplicacion = dia_soli + mes_soli + anio_soli;

            Random rand = new Random();
            int random_number = rand.Next(0, 999);
            string filePath = "\\\\192.168.128.2\\inetpub\\OrdenesPagoTXT\\OrdenPago-" + fecha_aplicacion + "" + orden_g.id_contabilidad_orden_pago_g + "-" + random_number + ".txt";

            var solicitud_d = db.C_contabilidad_ordenes_pago_d.Where(x => x.id_contabilidad_orden_pago_g == id_contabilidad_orden_pago_g && x.activo == true).ToList();
            if (solicitud_d.Count() == 0) { return "0"; }

            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                foreach (var cuenta in solicitud_d)
                {
                    string nombre_bene = "";
                    try
                    {
                        int entero_importe = (int)cuenta.importe;
                        decimal decimales = ((decimal)cuenta.importe - entero_importe) * 100;
                        int entero_decimal = Convert.ToInt32(decimales);
                        string importe_total = entero_importe.ToString().Trim().PadLeft(12, '0') + entero_decimal.ToString().Trim().PadLeft(2, '0');

                        string nombre_beneficiario = "";
                        int length_nombre_beneficiario = cuenta.nombre_beneficiario.Trim().Length;
                        for (int z = 0; z < length_nombre_beneficiario; z++)
                        {
                            if (cuenta.nombre_beneficiario.Substring(z, 1) != "." && cuenta.nombre_beneficiario.Substring(z, 1) != ",")
                            {
                                if (cuenta.nombre_beneficiario.Substring(z, 1) == "Ñ" || cuenta.nombre_beneficiario.Substring(z, 1) == "ñ")
                                {
                                    nombre_beneficiario = nombre_beneficiario + "N";
                                }
                                else
                                {
                                    nombre_beneficiario = nombre_beneficiario + cuenta.nombre_beneficiario.Substring(z, 1);
                                }
                            }
                            else
                            {
                                nombre_beneficiario = nombre_beneficiario + "*";
                            }
                        }
                        length_nombre_beneficiario = nombre_beneficiario.Length;
                        length_nombre_beneficiario++;
                        if (length_nombre_beneficiario < 54)
                        {
                            int relleno = 54 - length_nombre_beneficiario;
                            nombre_beneficiario += "/";
                            nombre_beneficiario = nombre_beneficiario + new string(' ', relleno);
                        }
                        if (length_nombre_beneficiario > 54)
                        {
                            nombre_beneficiario = nombre_beneficiario.Substring(0, 53) + "/";
                        }

                        string cadena = cuenta.tipo_transaccion;
                        cadena += cuenta.tipo_cuenta_origen;
                        cadena += orden_g.numero_sucursal.PadLeft(4, '0');
                        cadena += orden_g.numero_cuenta.PadLeft(20, '0');
                        cadena += cuenta.sucursal_destino.PadLeft(4, '0');
                        cadena += importe_total;
                        cadena += cuenta.tipo_moneda;
                        cadena += nombre_beneficiario;
                        cadena += cuenta.remitente.PadRight(33,' ');

                        cadena += cuenta.instrucciones.PadRight(40, ' ');
                        cadena += fecha_aplicacion;
                        cadena += orden_g.hora_aplicacion;

                        writer.WriteLine(cadena);
                    }
                    catch (Exception)
                    {
                        string bene_error = nombre_bene;
                    }
                }
            }
            return "https://siib.beta.com.mx/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");

            //return "http://192.168.128.2:90/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");

            //return "https://localhost:44371/CONTABILIDAD/DescargarTXT_Path?path=" + filePath.Replace("\\", "/");
        }


        #endregion





    }
}