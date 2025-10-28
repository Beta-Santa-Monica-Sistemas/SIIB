using Beta_System.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZXing;

namespace Beta_System.Controllers
{
    public class ACCESSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public class MacRegistroDto
        {
            public string mac { get; set; }
            public string dispositivo { get; set; }
        }

        // Obtiene los comandos pendientes por ejecutar para un dispositivo
        [HttpGet]
        public JsonResult ComandosPorEjecutar(string mac, int tipoTablet)
        {
            var comandos = (from g in db.C_dispositivos_comandos_globales
                            join cat in db.C_dispositivos_comandos_catalogo on g.id_comando equals cat.id_comando
                            where cat.activo == true
                                  && g.activo == true
                                  && (g.para_todos == true
                                      || g.para_mac == mac
                                      || g.para_tipo == tipoTablet)
                                  && (g.es_perpetuo == true
                                      || !db.C_dispositivos_comandos_ejecutados
                                          .Any(e => e.id_comando_global == g.id_comando_global && e.mac_address == mac))
                            select new
                            {
                                id_comando_global = g.id_comando_global,
                                tipo = cat.nombre_comando,
                                parametro = g.parametro,
                                perpetuo = g.es_perpetuo
                            }).ToList();

            return Json(new { comandos = comandos }, JsonRequestBehavior.AllowGet);
        }

        #region VALIDACION DISPOSITIVO
        // Revisa si el dispositivo necesita actualizar la aplicación y devuelve la última versión y URL
        [HttpGet]
        public JsonResult CheckUpdate(string version, string androidId, string mode)
        {
            string ultimaVersion = db.C_parametros_configuracion.Find(1025).valor_texto;
            if (ultimaVersion == "" || ultimaVersion == null) { ultimaVersion = "2.0.0"; }

            string apkUrl = mode == "kiosk"
                ? "http://192.168.128.2/apks/kiosk-release.apk"
                : "http://192.168.128.2/apks/normal-release.apk";

            // Lógica de comparación
            bool needsUpdate = !string.Equals(version, ultimaVersion, StringComparison.OrdinalIgnoreCase);

            return Json(new
            {
                needsUpdate = needsUpdate,
                latest = ultimaVersion,
                apkUrl = apkUrl
            }, JsonRequestBehavior.AllowGet);
        }

        // Valida una clave de administrador contra los parámetros de configuración
        [HttpPost]
        public ActionResult ValidarClave(DeviceClaveAdmin input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.clave))
                return new HttpStatusCodeResult(400, "Clave vacía");

            decimal acceso = 0;
            try
            {
                acceso = Convert.ToDecimal(input.clave);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(401, "Clave incorrecta");
            }

            var claveReal = db.C_parametros_configuracion.FirstOrDefault(c => c.valor_numerico == acceso);
            if (claveReal != null) return new HttpStatusCodeResult(200);

            return new HttpStatusCodeResult(401, "Clave incorrecta");
        }

        // Verifica si una MAC está autorizada y activa en la base de datos
        [HttpGet]
        public ActionResult ValidarMACAddress(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return new HttpStatusCodeResult(400, "MAC no proporcionada");

            var existe = db.C_dispositivos_autorizados.Any(x => x.mac_address == mac && x.activo == true);

            return JsonOk(new { autorizado = existe ? 1 : 0 });
        }

        // Valida una MAC y devuelve si está autorizada, si es kiosko y el tipo de dispositivo
        [HttpGet]
        public JsonResult ValidarMacAccess(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                return Json(new { autorizado = false, kiosko = false }, JsonRequestBehavior.AllowGet);
            }

            var registro = db.C_dispositivos_autorizados.FirstOrDefault(x => x.mac_address == mac && x.activo == true);

            if (registro == null)
            {
                return Json(new { autorizado = false, kiosko = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                autorizado = true,
                kiosko = registro.bloqueo,
                id_tipo = registro.id_dispositivo_tipo
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region REGISTROS DISPOSITIVO
        // Registra una nueva MAC en la tabla de dispositivos autorizados
        [HttpPost]
        public ActionResult RegistrarMACAddress(MacRegistroDto data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.mac))
                return new HttpStatusCodeResult(400, "MAC inválida");

            if (db.C_dispositivos_autorizados.Any(x => x.mac_address == data.mac))
                return new HttpStatusCodeResult(409, "MAC ya registrada");

            var nuevo = new C_dispositivos_autorizados
            {
                mac_address = data.mac,
                dispositivo = data.dispositivo ?? "Sin nombre",
                fecha_registro = DateTime.Now,
                activo = true
            };

            db.C_dispositivos_autorizados.Add(nuevo);
            db.SaveChanges();

            return new HttpStatusCodeResult(200);
        }

        // Registra un nuevo dispositivo Android con información adicional
        [HttpPost]
        public ActionResult RegistrarDeviceAccess(DeviceAndroidRegistroDto data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.androidId))
            {
                return new HttpStatusCodeResult(400, "Dispositivo inválido");
            }


            if (db.C_dispositivos_autorizados.Where(x => x.mac_address == data.androidId && x.activo == true).Count() != 0)
            {
                return new HttpStatusCodeResult(409, "Dispositivo ya registrada");
            }

            var nuevo = new C_dispositivos_autorizados
            {
                mac_address = data.androidId,
                dispositivo = data.dispositivo ?? "Sin nombre",
                fecha_registro = DateTime.Now,
                bloqueo = true,
                activo = true,
                id_establo = data.id_establo,
                id_dispositivo_tipo = data.id_tipo
            };

            db.C_dispositivos_autorizados.Add(nuevo);
            db.SaveChanges();

            return new HttpStatusCodeResult(200);
        }
        #endregion

        #region MENUS DISPOSITIVO
        // Obtiene los ítems de menú permitidos para un tipo de dispositivo
        [HttpGet]
        public JsonResult MenuItemsPorTipo(int id_tipo)
        {
            // 1. Obtener módulos permitidos para este tipo
            var modulos = db.C_dispositivos_tipos_modulo.Where(x => x.id_dispositivo_tipo == id_tipo && x.activo == true).Select(x => x.id_modulo).ToList();
            if (!modulos.Any())
            {
                return Json(new
                {
                    success = true,
                    message = "Sin módulos asignados.",
                    menu_ids = new List<string>(),
                    grupos = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }

            // 2. Obtener ítems de esos módulos
            var opciones = db.C_dispositivos_modulos_items.Where(x => modulos.Contains(x.id_modulo) && x.activo == true).Select(x => new
            {
                x.id_modulo,
                x.id_android_menu_item,
                titulo = x.descripcion,
                funcion = x.funcion,
                modulo_nombre = x.C_dispositivos_modulos.nombre_modulo
            }).ToList();

            // 3. Agrupar por módulo
            var agrupado = opciones.GroupBy(x => new { x.id_modulo, x.modulo_nombre }).Select(x => new
            {
                modulo = x.Key.modulo_nombre,
                items = x.Select(y => new
                {
                    id = y.id_android_menu_item,
                    titulo = y.titulo,
                    funcion = y.funcion
                }).ToList()
            }).ToList();

            // 4. Respuesta
            var listaIds = opciones.Select(o => o.id_android_menu_item).Distinct().ToList();
            return Json(new
            {
                success = true,
                message = "Menú generado correctamente.",
                menu_ids = listaIds,
                grupos = agrupado
            }, JsonRequestBehavior.AllowGet);
        }

        // Devuelve la lista de establos activos
        [HttpGet]
        public ActionResult EstablosDispositivos()
        {
            try
            {
                var establos = db.C_establos
                    .Where(x => x.activo == true)
                    .Select(x => new
                    {
                        id_establo = x.id_establo,
                        nombre_establo = x.nombre_establo
                    })
                    .ToList();

                return Json(establos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error en EstablosDispositivos: " + ex.Message);
            }
        }

        // Devuelve la lista de tipos de dispositivos activos
        [HttpGet]
        public ActionResult TiposDispositivos()
        {
            try
            {
                var tipos = db.C_dispositivos_tipos
                    .Where(x => x.activo == true)
                    .Select(x => new
                    {
                        id_tipo = x.id_dispositivo_tipo,
                        nombre_tipo = x.tipo_dispositivo
                    })
                    .ToList();

                return Json(tipos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error en TiposDispositivos: " + ex.Message);
            }
        }
        #endregion

        #region LOGS DISPOSITIVO
        // Registra un latido simple de un dispositivo
        [HttpPost]
        public JsonResult LatidoDispositivo(DeviceLatidoDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.mac))
                return JsonError("MAC vacía", 400);

            db.C_dispositivos_latidos.Add(new C_dispositivos_latidos
            {
                mac_address = dto.mac,
                version_app = dto.version,
                fecha = DateTime.Now
            });
            db.SaveChanges();

            return JsonOk(new { success = true }, 200);
        }

        // Registra latido de un dispositivo y devuelve si está autorizado y en modo kiosko
        [HttpPost]
        public JsonResult LatidoYControl(DeviceLatidoDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.mac))
                return JsonError("MAC vacía", 400);

            var nuevo = new C_dispositivos_latidos
            {
                mac_address = dto.mac,
                version_app = dto.version,
                fecha = DateTime.Now
            };
            db.C_dispositivos_latidos.Add(nuevo);

            var registro = db.C_dispositivos_autorizados
                .FirstOrDefault(x => x.mac_address == dto.mac && x.activo == true);

            if (registro == null)
            {
                db.SaveChanges();
                return JsonOk(new { autorizado = false });
            }
            bool kiosko = registro.bloqueo ?? false;

            db.SaveChanges();

            return JsonOk(new
            {
                autorizado = true,
                kiosko
            });
        }

        // Registra en histórico que un comando fue ejecutado por un dispositivo
        [HttpPost]
        public ActionResult HistoricoComandosEjecutados(int id_comando_global, string mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return new HttpStatusCodeResult(400, "MAC inválida");

            var existe = db.C_dispositivos_comandos_ejecutados.Any(x => x.id_comando_global == id_comando_global && x.mac_address == mac);
            if (!existe)
            {
                db.C_dispositivos_comandos_ejecutados.Add(new C_dispositivos_comandos_ejecutados
                {
                    id_comando_global = id_comando_global,
                    mac_address = mac,
                    fecha_ejecucion = DateTime.Now

                });
                db.SaveChanges();
            }

            return new HttpStatusCodeResult(200);
        }

        // Registra un log individual de un dispositivo
        [HttpPost]
        public ActionResult RegistrarLog(string mac, string tipo_accion, string detalle = "")
        {
            if (string.IsNullOrWhiteSpace(mac) || string.IsNullOrWhiteSpace(tipo_accion))
                return new HttpStatusCodeResult(400, "Parámetros inválidos");

            try
            {
                db.C_dispositivos_logs.Add(new C_dispositivos_logs
                {
                    mac_address = mac,
                    tipo_accion = tipo_accion,
                    detalle = detalle
                });
                db.SaveChanges();
                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                return JsonError("Error al registrar log: " + ex.Message, 500);
            }
        }

        // Registra múltiples logs en lote enviados en formato JSON
        [HttpPost]
        public ActionResult RegistrarLogsLote()
        {
            try
            {
                // 1) Validar Content-Type
                if (string.IsNullOrWhiteSpace(Request.ContentType) ||
                    !Request.ContentType.ToLowerInvariant().Contains("application/json"))
                {
                    // Usa tu
                    //
                    // si ya lo agregaste (JsonError); si no, responde JSON manualmente
                    Response.StatusCode = 415;
                    Response.ContentType = "application/json";
                    return Json(new { success = false, error = "Content-Type inválido. Usa application/json." }, JsonRequestBehavior.AllowGet);
                }

                // 2) Leer todo el body crudo
                string body;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                // 3) Intentar deserializar
                List<DeviceLogDispositivo> logs;
                try
                {
                    logs = JsonConvert.DeserializeObject<List<DeviceLogDispositivo>>(body);
                }
                catch (Exception)
                {
                    Response.StatusCode = 400;
                    Response.ContentType = "application/json";
                    return Json(new { success = false, error = "JSON inválido." }, JsonRequestBehavior.AllowGet);
                }

                if (logs == null || logs.Count == 0)
                {
                    Response.StatusCode = 400;
                    Response.ContentType = "application/json";
                    return Json(new { success = false, error = "No se recibieron logs." }, JsonRequestBehavior.AllowGet);
                }

                // 4) Insertar cada log con fecha segura
                foreach (var log in logs)
                {
                    DateTime fecha;
                    if (!DateTime.TryParse(log.fecha, out fecha))
                        fecha = DateTime.UtcNow;

                    db.C_dispositivos_logs.Add(new C_dispositivos_logs
                    {
                        mac_address = log.mac,
                        tipo_accion = log.tipo_accion,
                        detalle = log.detalle,
                        fecha = fecha
                    });
                }

                db.SaveChanges();

                Response.StatusCode = 200;
                Response.ContentType = "application/json";
                return Json(new { success = true, count = logs.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.ContentType = "application/json";
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region JSONS DISPOSITIVO
        private JsonResult JsonOk(object data, int status = 200)
        {
            Response.StatusCode = status;
            Response.ContentType = "application/json";
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private JsonResult JsonError(string message, int status = 500)
        {
            Response.StatusCode = status;
            Response.ContentType = "application/json";
            return Json(new { success = false, error = message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpGet]
        public JsonResult ObtenerNombreDispositivo(string mac)
        {
            string nombre = db.C_dispositivos_autorizados.Where(x=>x.activo == true && x.mac_address == mac).FirstOrDefault().dispositivo;

            return Json(new
            {
                nombre
            }, JsonRequestBehavior.AllowGet);
        }

    }
}