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

namespace Beta_System.Controllers
{
    public class BASCULAController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();

        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(9)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../Establos/Bascula/Index");
        }

        public PartialViewResult ConsultarEstablosUsuarios(int id_usuario)
        {
            var establos = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).ToList();
            return PartialView("../Establos/Bascula/_EstablosUsuarioSelect", establos);
        }

        #region PRIMERA Y SEGUNDA PESADA

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
                    bascula.Close();
                    return pesada;
                }
                else
                {
                    return new Random().Next(1000,9999).ToString();
                    //return "No se pudo abrir el puerto";
                }

            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return msj;
            }
        }


        #endregion

        public PartialViewResult ConsultarReporteFichas(int[] id_establo, DateTime fecha_inicio, DateTime fecha_fin)
        {
            int id_usuario = (int)Session["LoggedId"];

            if (id_establo.Contains(0))
            {
                id_establo = db.C_usuarios_establos.Where(x => x.activo == true && x.id_usuario == id_usuario).Select(x => (int)x.id_establo).ToArray();
            }
            fecha_fin = fecha_fin.AddHours(23).AddMinutes(59).AddSeconds(59);

            var fichas_bascula = db.C_bascula_fichas.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo) && x.fecha_registo > fecha_inicio && x.fecha_registo < fecha_fin).ToList();
            return PartialView("../Establos/Bascula/_FichasReporte", fichas_bascula);

        }

        public PartialViewResult ConsultarFichasPendientes(int id_establo)
        {
            var fichas_bascula = db.C_bascula_fichas.Where(x => x.activo == true && x.id_establo == id_establo && x.termina == false).ToList();
            return PartialView("../Establos/Bascula/_FichasPendientes", fichas_bascula);
        }

        public PartialViewResult ImpresionFichaPDF(int id_bascula_ficha, int id_establo)
        {
            var impresion_ficha = db.C_bascula_fichas.Where(x => x.activo == true && x.id_bascula_ficha == id_bascula_ficha && x.id_establo == id_establo).ToList();
            if (impresion_ficha.Count() > 0)
            {
                return PartialView("../Establos/Bascula/_PDFBascula", impresion_ficha);
            }
            else { return PartialView("../Establos/Bascula/_PDFBascula", null); }
        }

        #region PRIMERA PESADA
        public int ConsultarUltimaFicha()
        {
            try
            {
                var ficha = db.C_bascula_fichas.OrderByDescending(x => x.id_bascula_ficha).FirstOrDefault();
                if (ficha != null) { return ficha.id_bascula_ficha + 1; }
                else { return 1; }
            }
            catch (Exception) { return -1; }
        }

        public bool GuardarPrimeraPesada(C_bascula_fichas c_ficha)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
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
        #endregion

        #region SEGUNDA PESADA
        public string ConsultarInfoFicha(int ficha, int id_establo)
        {
            try
            {
                var ficha_bascula = db.C_bascula_fichas.Where(x => x.id_bascula_ficha == ficha && x.id_establo == id_establo).FirstOrDefault().id_ficha_tipo;
                //ENVIO DE LECHE
                if (ficha_bascula == 1)
                {
                    var datos = from fich in db.C_bascula_fichas
                                join tipoficha in db.C_bascula_fichas_tipos on fich.id_ficha_tipo equals tipoficha.id_ficha_tipo
                                join provlech in db.C_envios_leche_clientes on fich.id_envio_leche_cliente_ms equals provlech.id_envio_leche_cliente_ms
                                join art in db.C_articulos_catalogo on fich.id_articulo_bascula equals art.id_articulo
                                join lin_tra in db.C_bascula_lineas_transportistas on fich.id_linea_transportista equals lin_tra.id_bascula_linea_transp
                                join tmov in db.C_bascula_tipos_movimientos on fich.id_tipo_movimiento equals tmov.id_tipo_movimiento
                                join cvemo in db.C_bascula_codigos_movimientos on fich.id_codigo_movimiento equals cvemo.id_bascula_codigo_movimiento
                                where fich.id_bascula_ficha == ficha && fich.termina == false && fich.id_establo == id_establo
                                select new
                                {
                                    idtipoficha = tipoficha.id_ficha_tipo,
                                    tipoficha = tipoficha.tipo_ficha,
                                    leche = provlech.razon_social,
                                    art.nombre_articulo,
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
                                    fich.agrupada,

                                    fich.tabla,
                                    fich.variedad,
                                    fich.corte,
                                    fich.pacas,
                                    fich.ensilador,

                                    fich.observaciones,
                                    fich.peso_origen,
                                    fich.peso_materia_seca,

                                    fich.peso_1

                                };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                }
                //FORRAJE
                else if(ficha_bascula == 2)
                {
                    var datos = from fich in db.C_bascula_fichas
                                join tipoficha in db.C_bascula_fichas_tipos on fich.id_ficha_tipo equals tipoficha.id_ficha_tipo
                                join provregul in db.C_compras_proveedores on fich.id_compras_proveedor equals provregul.id_compras_proveedor
                                join art in db.C_articulos_catalogo on fich.id_articulo_bascula equals art.id_articulo
                                join lin_tra in db.C_bascula_lineas_transportistas on fich.id_linea_transportista equals lin_tra.id_bascula_linea_transp
                                join tmov in db.C_bascula_tipos_movimientos on fich.id_tipo_movimiento equals tmov.id_tipo_movimiento
                                join cvemo in db.C_bascula_codigos_movimientos on fich.id_codigo_movimiento equals cvemo.id_bascula_codigo_movimiento
                                join pozo in db.C_bascula_no_pozos on fich.id_no_pozo equals pozo.id_no_pozo
                                where fich.id_bascula_ficha == ficha && fich.termina == false && fich.id_establo == id_establo
                                select new
                                {
                                    idtipoficha = tipoficha.id_ficha_tipo,
                                    tipoficha = tipoficha.tipo_ficha,
                                    pozo.no_pozo,
                                    regular = provregul.alias_bascula,
                                    art.nombre_articulo,
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
                                    fich.agrupada,

                                    fich.tabla,
                                    fich.variedad,
                                    fich.corte,
                                    fich.pacas,
                                    fich.ensilador,

                                    fich.observaciones,
                                    fich.peso_origen,
                                    fich.peso_materia_seca,

                                    fich.peso_1

                                };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                }
                //REGULAR PROVEEDOR
                else if (ficha_bascula == 3)
                {
                    var datos = from fich in db.C_bascula_fichas
                                join tipoficha in db.C_bascula_fichas_tipos on fich.id_ficha_tipo equals tipoficha.id_ficha_tipo
                                join provregul in db.C_compras_proveedores on fich.id_compras_proveedor equals provregul.id_compras_proveedor
                                join art in db.C_articulos_catalogo on fich.id_articulo_bascula equals art.id_articulo
                                join tmov in db.C_bascula_tipos_movimientos on fich.id_tipo_movimiento equals tmov.id_tipo_movimiento
                                join cvemo in db.C_bascula_codigos_movimientos on fich.id_codigo_movimiento equals cvemo.id_bascula_codigo_movimiento
                                join pozo in db.C_bascula_no_pozos on fich.id_no_pozo equals pozo.id_no_pozo
                                where fich.id_bascula_ficha == ficha && fich.termina == false && fich.id_establo == id_establo
                                select new
                                {
                                    idtipoficha = tipoficha.id_ficha_tipo,
                                    tipoficha = tipoficha.tipo_ficha,
                                    pozo.no_pozo,
                                    regular = provregul.alias_bascula,
                                    art.nombre_articulo,
                                    tmov = tmov.nombre_movimiento,
                                    cvemo = cvemo.codigo_mov + " - " + cvemo.descripcion,

                                    fich.folio,
                                    fich.sucursal,
                                    fich.destino,

                                    conductor = fich.chofer,
                                    fich.placas,
                                    fich.maquilador,
                                    fich.predio_cliente,
                                    fich.agrupada,

                                    fich.tabla,
                                    fich.variedad,
                                    fich.corte,
                                    fich.pacas,
                                    fich.ensilador,

                                    fich.observaciones,
                                    fich.peso_origen,
                                    fich.peso_materia_seca,

                                    fich.peso_1

                                };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                }
                //REGULAR CLIENTE
                else
                {
                    var datos = from fich in db.C_bascula_fichas
                                join tipoficha in db.C_bascula_fichas_tipos on fich.id_ficha_tipo equals tipoficha.id_ficha_tipo
                                join provlech in db.C_envios_leche_clientes on fich.id_envio_leche_cliente_ms equals provlech.id_envio_leche_cliente_ms
                                join art in db.C_articulos_catalogo on fich.id_articulo_bascula equals art.id_articulo
                                join tmov in db.C_bascula_tipos_movimientos on fich.id_tipo_movimiento equals tmov.id_tipo_movimiento
                                join cvemo in db.C_bascula_codigos_movimientos on fich.id_codigo_movimiento equals cvemo.id_bascula_codigo_movimiento
                                join pozo in db.C_bascula_no_pozos on fich.id_no_pozo equals pozo.id_no_pozo
                                where fich.id_bascula_ficha == ficha && fich.termina == false && fich.id_establo == id_establo
                                select new
                                {
                                    idtipoficha = tipoficha.id_ficha_tipo,
                                    tipoficha = tipoficha.tipo_ficha,
                                    pozo.no_pozo,
                                    regular = provlech.razon_social,
                                    art.nombre_articulo,
                                    tmov = tmov.nombre_movimiento,
                                    cvemo = cvemo.codigo_mov + " - " + cvemo.descripcion,

                                    fich.folio,
                                    fich.sucursal,
                                    fich.destino,

                                    conductor = fich.chofer,
                                    fich.placas,
                                    fich.maquilador,
                                    fich.predio_cliente,
                                    fich.agrupada,

                                    fich.tabla,
                                    fich.variedad,
                                    fich.corte,
                                    fich.pacas,
                                    fich.ensilador,

                                    fich.observaciones,
                                    fich.peso_origen,
                                    fich.peso_materia_seca,

                                    fich.peso_1

                                };
                    return Newtonsoft.Json.JsonConvert.SerializeObject(datos);
                }
            }
            catch (Exception)
            {
                return "[]";
            }

        }

        public PartialViewResult ConsultarFormatoFichaBascula(int idtipoficha, int pesada)
        {
            //FICHA DE LECHE
            if (idtipoficha == 1)
            {
                if (pesada == 1)
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaLechePp");
                }
                else
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaLecheSp");
                }
            }
            //FICHA FORRAJE
            else if (idtipoficha == 2)
            {
                if (pesada == 1)
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaForrajePp");
                }
                else
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaForrajeSp");
                }
            }
            //FICHA REGULAR
            else
            {
                if (pesada == 1)
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaRegularPp");
                }
                else
                {
                    return PartialView("../ESTABLOS/Bascula/TipoFicha/_FichaRegularSp");
                }
            }
        }

        public int GuardarSegundaPesada(int ficha, string peso_2, C_envios_leche_d_fichas envio_leche, C_envios_leche_d_calidad calidad_leche, int tipo_ficha)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];

                //SEGUNDA PESADA
                string valor_ficha = new string(peso_2.Where(char.IsDigit).ToArray());
                int valor_peso_2 = Convert.ToInt32(valor_ficha);
                var ficha_reg = db.C_bascula_fichas.Find(ficha);

                decimal peso_t = (decimal)ficha_reg.peso_1 - valor_peso_2;
                ficha_reg.fecha_segunda_pesada = DateTime.Now;
                ficha_reg.peso_2 = valor_peso_2;
                ficha_reg.peso_t = Math.Abs(peso_t);
                ficha_reg.termina = true;
                db.SaveChanges();
                if (tipo_ficha == 1)
                {
                    //GENERAR ENVIO DE LECHE_D
                    envio_leche.activo = true;
                    db.C_envios_leche_d_fichas.Add(envio_leche);
                    db.SaveChanges();
                    //GENERAR CALIDAD LECHE
                    calidad_leche.activo = true;
                    calidad_leche.id_envio_leche_d_ficha = envio_leche.id_envio_leche_d;
                    db.C_envios_leche_d_calidad.Add(calidad_leche);
                    db.SaveChanges();
                }

                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        #endregion

        private string FolioEnvioLeche(int id_establo)
        {
            int consecuivo;
            int generacion = 20;

            string siglas = "";
            string folio = "";
            if (id_establo == 1) { siglas = "SG"; }
            else if (id_establo == 2) { siglas = "CR"; }
            else if (id_establo == 3) { siglas = "SM"; }
            else { siglas = "TN"; }

            var valid = db.C_envios_leche_g.Where(x => x.id_establo_envio == id_establo).OrderByDescending(x => x.id_envio_leche_g).FirstOrDefault();
            if (valid == null) { consecuivo = 0001; generacion = 20; }
            else
            {
                if (valid.folio.Split('-').Count() > 1)
                {
                    consecuivo = Convert.ToInt32(valid.folio.Split('-')[2]) + 1;
                    generacion = Convert.ToInt32(valid.folio.Split('-')[1]);
                }
                else
                {
                    consecuivo = 0001; generacion = 20;
                }

            }

            if (consecuivo == 10000) { generacion = generacion++; consecuivo = 01; }

            folio = siglas + "-" + generacion.ToString().PadLeft(2, '0') + "-" + consecuivo.ToString().PadLeft(4, '0');
            return folio;
        }

        public PartialViewResult AgrupacionFichasLeche(int[] id_envio_leche_d)
        {
            var envio_leche = db.C_envios_leche_d_fichas.Where(x => x.activo == true && id_envio_leche_d.Contains(x.id_envio_leche_d)).ToList();
            var c_ficha = envio_leche.FirstOrDefault().C_bascula_fichas;



            ViewBag.FolioEnvLeche = FolioEnvioLeche((int)c_ficha.id_establo);
            return PartialView("../Establos/Bascula/EnvioLeche/_AgrupacionFichasLeche", envio_leche);
        }

        public bool ConfirmarAgrupacionFichasLeche(int[] id_envio_leche_d, C_envios_leche_g ficha_leche_g)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                int tipo_tanque = 1;
                if (id_envio_leche_d.Length > 1) { tipo_tanque = 2; }

                ficha_leche_g.id_tipo_tanque_pipa = tipo_tanque;
                ficha_leche_g.id_usuario_registra = id_usuario;
                ficha_leche_g.fecha_registro = DateTime.Now;
                ficha_leche_g.id_envio_leche_status = 1;
                ficha_leche_g.activo = true;
                db.C_envios_leche_g.Add(ficha_leche_g);
                db.SaveChanges();

                string folio = FolioEnvioLeche((int)ficha_leche_g.id_establo_envio);

                foreach (var item in id_envio_leche_d)
                {
                    var ficha_d = db.C_envios_leche_d_fichas.Find(item);
                    var ficha_b = db.C_bascula_fichas.Find(ficha_d.id_ficha_bascula);
                    int id_ficha_g = ficha_leche_g.id_envio_leche_g;
                    ficha_d.id_envio_leche_g = id_ficha_g;
                    ficha_d.folio_ficha = folio;
                    ficha_b.folio = folio;
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}