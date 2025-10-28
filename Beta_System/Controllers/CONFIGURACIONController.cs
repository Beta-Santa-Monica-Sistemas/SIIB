using Beta_System.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class CONFIGURACIONController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        private NOTIFICACIONESController notificaciones = new NOTIFICACIONESController();

        #region MODULO CONFIGURACION - ESTABLOS USUARIO
        public ActionResult ConfiguraUsuariosEstablos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(13)) { return View("/Views/Home/Index.cshtml"); }
                return View("../CONFIGURACION/ESTABLOS/AsignadorEstablosUsuario/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarUsuariosEstablos(int id_rol)
        {
            var usuario = from us in db.C_usuarios_corporativo
                          join us_es in db.C_usuarios_establos on us.id_usuario_corporativo equals us_es.id_usuario
                          select us_es;

            Session["establos_list"] = db.C_establos.Where(x => x.activo == true).OrderBy(x => x.nombre_establo).ToList();
            Session["establos_usuarios"] = db.C_usuarios_corporativo.Where(x => x.Activo == true && x.id_rol == id_rol).OrderBy(x => x.usuario).ToList();
            return PartialView("../CONFIGURACION/ESTABLOS/AsignadorEstablosUsuario/_UsuariosEstablos", usuario);
        }

        public bool GuardarUsuariosEstablos(int[] id_usuarios, int[] id_establos, bool[] status)
        {
            try
            {
                for (int i = 0; i < id_establos.Length; i++)
                {
                    int id_usuario = id_usuarios[i];
                    int id_establo = id_establos[i];

                    var valid = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.id_establo == id_establo).FirstOrDefault();
                    if (valid != null)
                    {
                        valid.activo = status[i];
                        db.SaveChanges();
                    }
                    else
                    {
                        C_usuarios_establos est = new C_usuarios_establos();
                        est.id_establo = id_establo;
                        est.id_usuario = id_usuario;
                        est.activo = status[i];
                        db.C_usuarios_establos.Add(est);
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

        #region MODULO CONFIGURACION - ALMACENES USUARIO 
        public ActionResult ConfiguraUsuariosAlmacenes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(3016)) { return View("/Views/Home/Index.cshtml"); }
                return View("ALMACENES/UsuariosAlmacen/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarAlmacenesUsuario(int id_usuario_almacen)
        {
            ViewBag.id_usuario = id_usuario_almacen;
            var almacenes_asignados = from alma in db.C_almacen_almacenes_g
                                      join alus in db.C_almacen_almacenes_usuarios on alma.id_almacen_g equals alus.id_almacen_g into almus
                                      from almacenes_usuarios in almus.DefaultIfEmpty()
                                          //where almacenes_usuarios.id_usuario == id_usuario_almacen
                                      select new UsuariosAlmacenes { c_almacenes = alma, c_almacen_almacenes_usuarios = almacenes_usuarios };
            return PartialView("ALMACENES/UsuariosAlmacen/_AlmacenesUsuario", almacenes_asignados);
        }

        public bool GuardarAlmacenesUsuario(int id_usuario_almacen, int[] id_almacenes)
        {
            try
            {
                var almacenes_asig = db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario_almacen).ToList();
                db.C_almacen_almacenes_usuarios.RemoveRange(almacenes_asig);
                db.SaveChanges();

                for (int i = 0; i < id_almacenes.Length; i++)
                {
                    C_almacen_almacenes_usuarios new_almacen = new C_almacen_almacenes_usuarios();
                    new_almacen.id_usuario = id_usuario_almacen;
                    new_almacen.id_almacen_g = id_almacenes[i];
                    new_almacen.activo = true;
                    db.C_almacen_almacenes_usuarios.Add(new_almacen);
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoverAlmacenesUsuario(int id_usuario_almacen)
        {
            try
            {
                var almacenes_asig = db.C_almacen_almacenes_usuarios.Where(x => x.id_usuario == id_usuario_almacen).ToList();
                db.C_almacen_almacenes_usuarios.RemoveRange(almacenes_asig);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region MODULO CONFIGURACION - CENTROS USUARIO
        public ActionResult ConfigurarUsuariosCentros()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4018)) { return View("/Views/Home/Index.cshtml"); }

                return View("CENTROS/UsuariosCentro/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarCentros()
        {
            var centros = db.C_centros_g.Where(x => x.activo == true).OrderBy(x => x.nombre_centro).ToList();
            return PartialView("../CONFIGURACION/CENTROS/UsuariosCentro/_CentrosUsuario", centros);
        }

        public bool ValidarCentroUsuario(int id_usuario, int id_centro)
        {
            var Validar = db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario && x.id_centro == id_centro).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AsignarCentroUsuario(int id_usuario, int[] id_centro)
        {
            try
            {
                var centro_usuario = db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_centros.RemoveRange(centro_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_centro.Length; i++)
                {
                    C_usuarios_centros centusu = new C_usuarios_centros();
                    centusu.id_usuario = id_usuario;
                    centusu.id_centro = id_centro[i];
                    centusu.activo = true;
                    db.C_usuarios_centros.Add(centusu);
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

        public bool AsignarTodoCentroUsuario(int id_usuario, int[] id_centro)
        {
            try
            {
                var centro_usuario = db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_centros.RemoveRange(centro_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_centro.Length; i++)
                {
                    C_usuarios_centros centusu = new C_usuarios_centros();
                    centusu.id_usuario = id_usuario;
                    centusu.id_centro = id_centro[i];
                    centusu.activo = true;
                    db.C_usuarios_centros.Add(centusu);
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

        public bool RemoverCentroUsuario(int id_usuario)
        {
            try
            {
                var centro_usuario = db.C_usuarios_centros.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_centros.RemoveRange(centro_usuario);
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

        #region MODULO CONFIGURACION - FIRMAS REQUISICION
        public ActionResult FirmasArea()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(5020)) { return View("/Views/Home/Index.cshtml"); }


                return View("../CONFIGURACION/FIRMASAREA/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        public PartialViewResult ConsultarCargo(int id_usuario_firma)
        {
            var centros = db.C_cargos_contables_g.Where(x => x.activo == true).OrderBy(x => x.nombre_cargo).ToList();
            ViewBag.firmas_permiso = db.C_firmas_cargo_permisos.Where(x => x.activo == true).ToList();
            ViewBag.id_usuario = id_usuario_firma;
            return PartialView("../CONFIGURACION/FIRMASAREA/_CargosUsuario", centros);
        }
        public bool ValidarFirmasUsuario(int id_usuario, int centros_id, int permiso_id)
        {
            var Validar = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == id_usuario && x.id_cargo_contable_g == centros_id && x.id_firma_cargo_permiso == permiso_id).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool AsignarFirmasUsuario(int id_usuario, string[] id_permiso)
        {
            try
            {
                //int controls = id_permiso.Length/id_cargo.Length;
                var firmas_usuario = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == id_usuario);
                db.C_firmas_usuarios.RemoveRange(firmas_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_permiso.Length; i++)
                {
                    var datos = id_permiso[i];
                    var splitData = datos.Split(' ');
                    int centros_id = Convert.ToInt32(splitData[0]);
                    int permiso_id = Convert.ToInt32(splitData[1]);
                    C_firmas_usuarios firma_usuario = new C_firmas_usuarios();
                    firma_usuario.id_usuario_corporativo = id_usuario;
                    firma_usuario.id_cargo_contable_g = centros_id;
                    firma_usuario.id_firma_cargo_permiso = permiso_id;
                    db.C_firmas_usuarios.Add(firma_usuario);
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

        public bool RemoverFirmasUsuario(int id_usuario)
        {
            try
            {
                var cargo_usuario = db.C_firmas_usuarios.Where(x => x.id_usuario_corporativo == id_usuario);
                db.C_firmas_usuarios.RemoveRange(cargo_usuario);
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

        #region MODULO CONFIGURACION - MONTOS USUARIO
        public bool AsignarMontosUsuario(int id_usuario, float monto)
        {
            try
            {
                C_firmas_usuarios_montos montos_usuario = new C_firmas_usuarios_montos();
                montos_usuario.id_usuario_corporativo = id_usuario;
                montos_usuario.montos = monto;
                montos_usuario.fecha_registros = DateTime.Now;
                montos_usuario.monto_ilimitados = false;
                db.C_firmas_usuarios_montos.Add(montos_usuario);
                db.SaveChanges();

                try
                {
                    int id_usuarioRegistro = (int)Session["LoggedId"];
                    UTILERIASController utileria = new UTILERIASController();
                    utileria.RegistroLogsSoporteGral(id_usuarioRegistro, "Se registro el monto de autorizacion de: " + montos_usuario.C_usuarios_corporativo.C_empleados.nombres + " " + montos_usuario.C_usuarios_corporativo.C_empleados.apellido_paterno + " a " + monto, "Log automatico");
                    db.SaveChanges();
                }
                catch (Exception) { }

                
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string ConsultarMontoUsuario(int id_usuario)
        {
            var montos = from mont in db.C_firmas_usuarios_montos
                         where mont.id_usuario_corporativo == id_usuario
                         select new
                         {
                             mont.id_usuario_montos,
                             mont.montos
                         };
            return Newtonsoft.Json.JsonConvert.SerializeObject(montos);
        }

        public bool MofificarMontoUsuario(int id_usuario_montos, float monto)
        {
            try
            {
                var mont = db.C_firmas_usuarios_montos.Find(id_usuario_montos);
                var monto_ori = mont.montos;
                mont.montos = monto;
                mont.fecha_actualizacions = DateTime.Now;
                db.SaveChanges();

                try
                {
                    int id_usuarioRegistro = (int)Session["LoggedId"];
                    UTILERIASController utileria = new UTILERIASController();
                    utileria.RegistroLogsSoporteGral(id_usuarioRegistro, "Se modifico el monto de autorizacion de: " + mont.C_usuarios_corporativo.C_empleados.nombres + " " + mont.C_usuarios_corporativo.C_empleados.apellido_paterno +" de " + monto_ori + " a " + monto, "Log automatico");
                    db.SaveChanges();
                }
                catch (Exception) { }

                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }
        #endregion

        #region MODULO CONFIGURACION - CUENTA CONTABLE USUARIO
        public ActionResult ConfigurarUsuariosCuentaContable()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(10106)) { return View("/Views/Home/Index.cshtml"); }

                return View("CUENTACONTABLE/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarCuentasContablesUsuario(int id_usuario, string cuenta_contable_buscar)
        {
            //if (id_cargo.Contains(0))
            //{
            //    CATALOGOSController catalogos = new CATALOGOSController();
            //    id_cargo = catalogos.CargoContableUsuariosID(0);
            //}

            //if (cuenta_contable_buscar.Trim().Length > 0)
            //{
            //    //var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.activo == true && cargos.Contains((int)x.id_cargo_contable) && x.nombre_cuenta.Contains(cuenta_contable_buscar) && x.C_cargos_contables_g.activo == true).OrderBy(x => x.nombre_cuenta).ToList();
            //    var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.activo == true && x.nombre_cuenta.Contains(cuenta_contable_buscar) && x.C_cargos_contables_g.activo == true).OrderBy(x => x.nombre_cuenta).ToList();

            //    return PartialView("../CONFIGURACION/CUENTACONTABLE/_CuentaContableUsuario", cuentas_contables);
            //}
            //else
            //{
            //    //var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.activo == true && cargos.Contains((int)x.id_cargo_contable) && x.C_cargos_contables_g.activo == true).OrderBy(x => x.nombre_cuenta).ToList();
            //    var cuentas_contables = db.C_cuentas_contables_g.Where(x => x.activo == true && id_cargo.Contains((int)x.id_cargo_contable) && x.C_cargos_contables_g.activo == true).OrderBy(x => x.nombre_cuenta).ToList();
            //    return PartialView("../CONFIGURACION/CUENTACONTABLE/_CuentaContableUsuario", cuentas_contables);
            //}


            var data = from cta in db.C_cuentas_contables_g
                       join us_cta in db.C_usuarios_cuentas_contables on cta.id_cuenta_contable equals us_cta.id_cuenta_contable_g into usuario_ctas
                       from cuentas_asig in usuario_ctas.Where(x => x.activo == true && x.id_usuario == id_usuario).DefaultIfEmpty()
                       where cta.activo == true && (cuenta_contable_buscar.Trim().Length > 0 ? cta.nombre_cuenta.Contains(cuenta_contable_buscar) : 1 == 1)
                       select new CuentasUsuario
                       {
                           Cuentas = cta,
                           Cta_Usuario = cuentas_asig
                       };
            return PartialView("../CONFIGURACION/CUENTACONTABLE/_CuentaContableUsuario", data);
        }

        public bool ValidarCuentasContablesUsuario(int id_usuario, int id_cuenta)
        {
            var Validar = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario && x.id_cuenta_contable_g == id_cuenta).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AsignarCuentasContablesUsuario(int id_usuario, int[] id_cuentas_contables)
        {
            try
            {
                var cuentas_asignadas = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario).ToList();
                cuentas_asignadas.ForEach(x => x.activo = false);
                db.SaveChanges();
                for (int i = 0; i < id_cuentas_contables.Length; i++)
                {
                    if (cuentas_asignadas.Select(x => (int)x.id_cuenta_contable_g).ToArray().Contains(id_cuentas_contables[i]))
                    {
                        cuentas_asignadas.Where(x => x.id_cuenta_contable_g == id_cuentas_contables[i]).FirstOrDefault().activo = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        C_usuarios_cuentas_contables cueta_usu = new C_usuarios_cuentas_contables();
                        cueta_usu.id_usuario = id_usuario;
                        cueta_usu.id_cuenta_contable_g = id_cuentas_contables[i];
                        cueta_usu.activo = true;
                        db.C_usuarios_cuentas_contables.Add(cueta_usu);
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool AsignarTodoCuentasContableUsuario(int id_usuario, int[] id_cuenta_contable)
        {
            try
            {
                var ctas_usuario = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario).ToList();
                ctas_usuario.ForEach(x => x.activo = false);
                db.SaveChanges();
                for (int i = 0; i < id_cuenta_contable.Length; i++)
                {
                    if (ctas_usuario.Where(x => x.id_cuenta_contable_g == id_cuenta_contable[i]).FirstOrDefault() != null)
                    {
                        ctas_usuario.Where(x => x.id_cuenta_contable_g == id_cuenta_contable[i]).FirstOrDefault().activo = true;
                        db.SaveChanges();
                    }
                    else
                    {
                        C_usuarios_cuentas_contables cuentausu = new C_usuarios_cuentas_contables();
                        cuentausu.id_usuario = id_usuario;
                        cuentausu.id_cuenta_contable_g = id_cuenta_contable[i];
                        cuentausu.activo = true;
                        db.C_usuarios_cuentas_contables.Add(cuentausu);
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool RemoverCuentasContableUsuario(int id_usuario)
        {
            try
            {
                db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario).ToList().ForEach(x => x.activo = false);
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

        #region MODULO CONFIGURACION - CARGO CONTABLE USUARIO
        public ActionResult ConfigurarUsuariosCargoContable()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4018)) { return View("/Views/Home/Index.cshtml"); }

                return View("CARGOCONTABLE/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarCargosContables()
        {
            var cargoscontables = db.C_cargos_contables_g.Where(x => x.activo == true).OrderBy(x => x.nombre_cargo).ToList();
            return PartialView("../CONFIGURACION/CARGOCONTABLE/_CargoContableUsuario", cargoscontables);
        }

        public bool ValidarCargosContablesUsuario(int id_usuario, int id_cargocontable)
        {
            var Validar = db.C_usuarios_cargos_contables.Where(x => x.id_usuario == id_usuario && x.id_cargo_contable_g == id_cargocontable).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AsignarCargosContablesUsuario(int id_usuario, int[] id_cargocontable)
        {
            try
            {
                var cargocontable_usuario = db.C_usuarios_cargos_contables.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_cargos_contables.RemoveRange(cargocontable_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_cargocontable.Length; i++)
                {
                    C_usuarios_cargos_contables cargo_usu = new C_usuarios_cargos_contables();
                    cargo_usu.id_usuario = id_usuario;
                    cargo_usu.id_cargo_contable_g = id_cargocontable[i];
                    cargo_usu.activo = true;
                    db.C_usuarios_cargos_contables.Add(cargo_usu);
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

        public bool AsignarTodoCargoContableUsuario(int id_usuario, int[] id_cargocontable)
        {
            try
            {
                var cargocontable_usuario = db.C_usuarios_cargos_contables.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_cargos_contables.RemoveRange(cargocontable_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_cargocontable.Length; i++)
                {
                    C_usuarios_cargos_contables cargousu = new C_usuarios_cargos_contables();
                    cargousu.id_usuario = id_usuario;
                    cargousu.id_cargo_contable_g = id_cargocontable[i];
                    cargousu.activo = true;
                    db.C_usuarios_cargos_contables.Add(cargousu);
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

        public bool RemoverCargoContableUsuario(int id_usuario)
        {
            try
            {
                var cargocontable_usuario = db.C_usuarios_cargos_contables.Where(x => x.id_usuario == id_usuario);
                db.C_usuarios_cargos_contables.RemoveRange(cargocontable_usuario);
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

        public ActionResult EmpleadosChecador()
        {
            return View("../CONFIGURACION/EMPLEADOSCHECADOR/Index");
        }

        #region MODULO CONFIGURACION - AREAS NOMINA USUARIO
        public ActionResult UsuariosAreasNomina()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(4018)) { return View("/Views/Home/Index.cshtml"); }

                return View("AREAS/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarAreasNomina()
        {
            var areas_nomina = from emp in db.C_nomina_empleados
                               join dep in db.C_nomina_departamentos on emp.Depto_no_id equals dep.id_departamento_empleado
                               where dep.activo == true && emp.Estatus == "A"
                               orderby dep.nombre_departamento
                               select dep;
            return PartialView("../CONFIGURACION/AREAS/_AreasUsuario", areas_nomina.Distinct());
        }

        public bool ValidarAreasNominaUsuario(int id_usuario, int id_area_nomina)
        {
            var Validar = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.id_departamento_empleado == id_area_nomina).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AsignarAreasNominaUsuario(int id_usuario, int[] id_area_nomina)
        {
            try
            {
                var areas_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario);
                db.C_usuarios_areas_nomina.RemoveRange(areas_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_area_nomina.Length; i++)
                {
                    C_usuarios_areas_nomina areas = new C_usuarios_areas_nomina();
                    areas.id_usuario_corporativo = id_usuario;
                    areas.id_departamento_empleado = id_area_nomina[i];
                    areas.activo = true;
                    db.C_usuarios_areas_nomina.Add(areas);
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

        public bool AsignarTodoAreasNominaUsuario(int id_usuario, int[] id_area_nomina)
        {
            try
            {
                var areas_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario);
                db.C_usuarios_areas_nomina.RemoveRange(areas_usuario);
                db.SaveChanges();
                for (int i = 0; i < id_area_nomina.Length; i++)
                {
                    C_usuarios_areas_nomina areausu = new C_usuarios_areas_nomina();
                    areausu.id_usuario_corporativo = id_usuario;
                    areausu.id_departamento_empleado = id_area_nomina[i];
                    areausu.activo = true;
                    db.C_usuarios_areas_nomina.Add(areausu);
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

        public bool RemoverAreasNominaUsuario(int id_usuario)
        {
            try
            {
                var area_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario);
                db.C_usuarios_areas_nomina.RemoveRange(area_usuario);
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

        #region MODULO CONFIGURACION - UBICACION ENTREGA ALMACEN
        public ActionResult ConfiguracionEntregaAlmacen()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8053)) { return View("/Views/Home/Index.cshtml"); }

                return View("ALMACENES/UbicacionAlmacen/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarUbicacionAlmacen()
        {
            var ubicacion = db.C_compras_ordenes_ubicaciones_entrega.Where(x => x.activo == true).ToList().OrderBy(x => x.nombre_ubicacion);
            return PartialView("ALMACENES/UbicacionAlmacen/_UbicacionesEntregaAlmacen", ubicacion);
        }

        public bool AsignarUbicacionAlmacenEntrega(int id_almacen, int[] id_ubicacion_entrega)
        {
            try
            {
                var ubicaciones_entrega = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.activo == true && x.id_almacen_g == id_almacen);
                db.C_almacen_almacenes_ubicaciones_entrega.RemoveRange(ubicaciones_entrega);
                db.SaveChanges();
                for (int i = 0; i < id_ubicacion_entrega.Length; i++)
                {
                    C_almacen_almacenes_ubicaciones_entrega ubicacion = new C_almacen_almacenes_ubicaciones_entrega();
                    ubicacion.id_almacen_g = id_almacen;
                    ubicacion.id_compras_ubicacion_entrega = id_ubicacion_entrega[i];
                    ubicacion.id_usuario_registra = (int)Session["LoggedId"];
                    ubicacion.fecha_registro = DateTime.Now;
                    ubicacion.activo = true;
                    db.C_almacen_almacenes_ubicaciones_entrega.Add(ubicacion);
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

        public bool AsignarTodoUbicacionAlmacen(int id_almacen, int[] id_ubicacion_entrega)
        {
            try
            {
                var ubicaciones_entrega = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.activo == true && x.id_almacen_g == id_almacen);
                db.C_almacen_almacenes_ubicaciones_entrega.RemoveRange(ubicaciones_entrega);
                db.SaveChanges();
                for (int i = 0; i < id_ubicacion_entrega.Length; i++)
                {
                    C_almacen_almacenes_ubicaciones_entrega ubicacion = new C_almacen_almacenes_ubicaciones_entrega();
                    ubicacion.id_almacen_g = id_almacen;
                    ubicacion.id_compras_ubicacion_entrega = id_ubicacion_entrega[i];
                    ubicacion.id_usuario_registra = (int)Session["LoggedId"];
                    ubicacion.fecha_registro = DateTime.Now;
                    ubicacion.activo = true;
                    db.C_almacen_almacenes_ubicaciones_entrega.Add(ubicacion);
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

        public bool RemoverUbicacionesAlmacen(int id_almacen)
        {
            try
            {
                var ubicaciones = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_almacen_g == id_almacen).ToList();
                db.C_almacen_almacenes_ubicaciones_entrega.RemoveRange(ubicaciones);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public JsonResult ValidarUbicacionEntrega(int id_almacen, int[] id_ubicacion)
        {
            var Validar = db.C_almacen_almacenes_ubicaciones_entrega.Where(x => x.id_almacen_g == id_almacen && id_ubicacion.
            Contains((int)x.id_compras_ubicacion_entrega)).Select(x => x.id_compras_ubicacion_entrega).ToList();
            return Json(Validar);
        }
        #endregion

        #region MODULO CONFIGURACION - CATALOGOS BASCULA
        public ActionResult ConfiguraCatalogosBascula()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8068)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/BASCULA/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }


        #endregion



        #region MODULO CONFIGURACION - CATALOGOS ENVIOS LECHE

        public ActionResult ConfiguraCatalogosEnviosLeche()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8073)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/ENVIOS_LECHE/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        #region CLIENTES

        public PartialViewResult ConsultaClientesEnvioLecheTable(int estatus)
        {
            bool activo = false; if (estatus == 1) { activo = true; }

            if (estatus == 0)
            {
                var clientes = db.C_envios_leche_clientes.ToList();
                return PartialView("../CATALOGOS/LECHE/Clientes/_ClientesEnvioLecheTable", clientes);
            }
            else
            {
                var clientes = db.C_envios_leche_clientes.Where(x => x.activo == activo).ToList();
                return PartialView("../CATALOGOS/LECHE/Clientes/_ClientesEnvioLecheTable", clientes);
            }
        }

        public bool OnOffClientesEnvioLeche(int id_envio_leche_cliente_ms, bool modo)
        {
            try
            {
                var cliente = db.C_envios_leche_clientes.Find(id_envio_leche_cliente_ms);
                cliente.activo = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string AsignarInformacionClientesEnvioLeche(int id_envio_leche_cliente_ms)
        {
            var cliente = from clie in db.C_envios_leche_clientes
                          where clie.id_envio_leche_cliente_ms == id_envio_leche_cliente_ms
                          select new
                          {
                              clie.nombre_comercial,
                              clie.activo
                          };
            return Newtonsoft.Json.JsonConvert.SerializeObject(cliente);
        }

        public bool ModificarClientesEnvioLeche(int id_envio_leche_cliente_ms, string nombre_comercial, bool activo)
        {
            try
            {
                var cliente = db.C_envios_leche_clientes.Find(id_envio_leche_cliente_ms);
                cliente.activo = activo;
                cliente.nombre_comercial = nombre_comercial;
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }
        #endregion

        #region DESTINOS

        public PartialViewResult ConsultarDestinosCatalogoBascula(int estatus)
        {
            bool activo = false; if (estatus == 1) { activo = true; }
            if (estatus == 0)
            {
                var Destinos = db.C_envios_leche_destinos.OrderBy(x => x.nombre_destino).ToList();
                return PartialView("../CONFIGURACION/BASCULA/Catalogos/_DestinosCatalogoBasculaTable", Destinos);
            }
            else
            {
                var Destinos = db.C_envios_leche_destinos.Where(x => x.activo == activo).OrderBy(x => x.nombre_destino).ToList();
                return PartialView("../CONFIGURACION/BASCULA/Catalogos/_DestinosCatalogoBasculaTable", Destinos);
            }
        }

        public bool RegistrarDestinosCatalogoBascula(string nombre)
        {
            try
            {
                C_envios_leche_destinos destinos = new C_envios_leche_destinos();
                destinos.activo = true;
                destinos.nombre_destino = nombre;
                db.C_envios_leche_destinos.Add(destinos);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public string AsignarInformacionDestinosCatalogoBascula(int id_destino_envio_leche)
        {
            var destinos = from dest in db.C_envios_leche_destinos
                           where dest.id_destino_envio_leche == id_destino_envio_leche
                           select new
                           {
                               dest.nombre_destino,
                               dest.activo
                           };
            return Newtonsoft.Json.JsonConvert.SerializeObject(destinos);
        }

        public bool ModificarDestinosCatalogoBascula(int id_destino_envio_leche, string nombre, bool activo)
        {
            try
            {
                var destinos = db.C_envios_leche_destinos.Find(id_destino_envio_leche);
                destinos.activo = activo;
                destinos.nombre_destino = nombre;
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool OnOffDestinosCatalogoBascula(int id_destino_envio_leche, bool modo)
        {
            try
            {
                var destinos = db.C_envios_leche_destinos.Find(id_destino_envio_leche);
                destinos.activo = modo;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region ASOCIACIONES - CLIENTES -> DESTINOS

        public PartialViewResult ConsultarClienteDestinosBascula()
        {
            var asociacion = db.C_envios_leche_cliente_destinos.Where(x => x.activo == true && x.C_envios_leche_clientes.activo==true && x.C_envios_leche_destinos.activo == true).OrderBy(x => x.C_envios_leche_clientes.nombre_comercial).ToList();
            return PartialView("../CONFIGURACION/ENVIOS_LECHE/Asociacion/_ClienteDestinosTable", asociacion);
        }

        public PartialViewResult ConsultarClienteAsociacion()
        {
            var Clientes = db.C_envios_leche_clientes.Where(x => x.activo == true).OrderBy(x => x.nombre_comercial).ToList();
            return PartialView("../CONFIGURACION/ENVIOS_LECHE/Asociacion/_ClienteAsociacion", Clientes);
        }
        public PartialViewResult ConsultarDestinosAsociacion()
        {
            var Destinos = db.C_envios_leche_destinos.Where(x => x.activo == true).OrderBy(x => x.nombre_destino).ToList();
            return PartialView("../CONFIGURACION/ENVIOS_LECHE/Asociacion/_DestinoAsociacion", Destinos);
        }

        public bool EliminarAsociarClienteDestino(int id_cliente_destino)
        {
            try
            {
                var remover = db.C_envios_leche_cliente_destinos.Where(x => x.id_cliente_destino == id_cliente_destino);
                db.C_envios_leche_cliente_destinos.RemoveRange(remover);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool ValidarClienteDestino(int id_envio_leche_cliente_ms, int id_destino_envio_leche)
        {
            try
            {
                var asociacion = db.C_envios_leche_cliente_destinos.Where(x => x.activo == true && x.id_destino_envio_leche == id_destino_envio_leche && x.id_envio_leche_cliente_ms == id_envio_leche_cliente_ms).Count();
                if (asociacion > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return true; }
        }

        public bool RegistrarClienteDestino(int id_envio_leche_cliente_ms, int id_destino_envio_leche)
        {
            try
            {
                C_envios_leche_cliente_destinos cliente_destino = new C_envios_leche_cliente_destinos();
                cliente_destino.activo = true;
                cliente_destino.id_envio_leche_cliente_ms = id_envio_leche_cliente_ms;
                cliente_destino.id_destino_envio_leche = id_destino_envio_leche;
                db.C_envios_leche_cliente_destinos.Add(cliente_destino);
                db.SaveChanges();
                return true;
            }
            catch { return false; }
        }




        #endregion

        #region PRECIOS DE LECHE DESTINOS
        public PartialViewResult ConsultarPreciosLecheDestinosTable()
        {
            var data = from dest in db.C_envios_leche_destinos
                       join cli_dest in db.C_envios_leche_cliente_destinos on dest.id_destino_envio_leche equals cli_dest.id_destino_envio_leche
                       join precio in db.C_envios_leche_precios
                       on dest.id_destino_envio_leche equals precio.id_destino_envio_leche into leche_precios
                       from precios in leche_precios.DefaultIfEmpty()
                       where dest.activo == true && precios.activo == true && cli_dest.activo == true
                       select new PreciosLecheDestinos
                       {
                           Destinos = dest,
                           Precios = precios,
                           Clientes_destinos = cli_dest
                       };

            return PartialView("ENVIOS_LECHE/PreciosLeche/_PreciosLecheTable", data);
        }

        public int GuardarPreciosLecheDestinos(int[] id_destinos, decimal[] precios)
        {
            int id_usuario = (int)Session["LoggedId"];
            string usuario_logged = Session["LoggedUser"].ToString();
            DateTime hoy = DateTime.Now;
            bool enviar_correo = false;
            try
            {
                for (int i = 0; i < id_destinos.Length; i++)
                {
                    int id_destino = id_destinos[i];
                    var valid = db.C_envios_leche_precios.Where(x => x.id_destino_envio_leche == id_destino).FirstOrDefault();
                    if (valid != null)
                    {
                        if (valid.precio_leche != precios[i])
                        {
                            valid.id_usuario_ultima_actualizacion = id_usuario;
                            valid.fecha_ultima_actualizacion = hoy;
                            valid.precio_leche = precios[i];
                            db.SaveChanges();
                            enviar_correo = true;
                        }
                    }
                    else
                    {
                        C_envios_leche_precios new_art_precio = new C_envios_leche_precios();
                        new_art_precio.id_destino_envio_leche = id_destino;
                        new_art_precio.precio_leche = precios[i];
                        new_art_precio.id_usuario_registra = id_usuario;
                        new_art_precio.id_usuario_ultima_actualizacion = id_usuario;
                        new_art_precio.fecha_registro = hoy;
                        new_art_precio.fecha_ultima_actualizacion = hoy;
                        new_art_precio.activo = true;
                        db.C_envios_leche_precios.Add(new_art_precio);
                        db.SaveChanges();
                    }
                }
                if (enviar_correo == true)
                {
                    string copia_correos = "";
                    var correos_notificacion = db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == 2007 && x.activo == true).Select(x => x.correo).ToList();
                    foreach (var correo in correos_notificacion)
                    {
                        copia_correos += correo + ";";
                    }

                    var obj = from dest in db.C_envios_leche_destinos
                               join cli_dest in db.C_envios_leche_cliente_destinos on dest.id_destino_envio_leche equals cli_dest.id_destino_envio_leche
                               join precio in db.C_envios_leche_precios
                               on dest.id_destino_envio_leche equals precio.id_destino_envio_leche into leche_precios
                               from precios_leche in leche_precios.DefaultIfEmpty()
                               where dest.activo == true && precios_leche.activo == true && cli_dest.activo == true
                               select new PreciosLecheDestinos { Destinos = dest, Precios = precios_leche, Clientes_destinos = cli_dest };
                    string tbody_data = "";
                    foreach (var data in obj.OrderBy(x => x.Clientes_destinos.C_envios_leche_clientes.razon_social))
                    {
                        tbody_data += "<tr style='border:1px solid black; text-align:center;'>";
                        tbody_data += "<td padding:2px;>" + data.Clientes_destinos.C_envios_leche_clientes.razon_social + "</td>";
                        tbody_data += "<td padding:2px;>" + data.Destinos.nombre_destino + "</td>";
                        if (data.Precios == null)
                        {
                            tbody_data += "<td padding:2px;>N/A</td>";
                            tbody_data += "<td padding:2px;>N/A</td>";
                        }
                        else
                        {
                            tbody_data += "<td padding:2px;>"+ data.Precios.precio_leche +"</td>";
                            tbody_data += "<td padding:2px;>"+ data.Precios.C_usuarios_corporativo.C_empleados.nombres + " " + data.Precios.C_usuarios_corporativo.C_empleados.apellido_paterno + "</td>";
                        }
                        tbody_data += "</tr>";
                    }
                    string mensaje = "<strong>Precios actualizados por el usuario: "+ usuario_logged + "</strong><hr />";
                    mensaje += "<table style='width: 100%; font-family: Arial, Helvetica, sans-serif; border:1px solid black;'>" +
                                       "<tr style='text-align:center; border:1px solid black'>" +
                                       "<th style='padding-top:12px; padding-bottom:12px; text-align:center; background-color:#2e87d7; color:white; padding:2px;'>Cliente</th>" +
                                       "<th style='padding-top:12px; padding-bottom:12px; text-align:center; background-color:#2e87d7; color:white; padding:2px;'>Destino</th>" +
                                       "<th style='padding-top:12px; padding-bottom:12px; text-align:center; background-color:#2e87d7; color:white; padding:2px;'>Precio leche</th>" +
                                       "<th style='padding-top:12px; padding-bottom:12px; text-align:center; background-color:#2e87d7; color:white; padding:2px;'>Usuario registra</th>" +
                                       "</tr>"
                                       + tbody_data +
                                       "</table>";
                    mensaje += "<hr /><img src='https://siib.beta.com.mx/Content/img_layout/logo_beta_new.png' width='200'/>";
                    notificaciones.EnviarCorreoUsuarioReportes("ACTUALIZACIÓN DE PRECIOS DE LECHE: "+ string.Format(hoy.ToShortDateString(), "dd/MM/yyyy") +"", copia_correos, mensaje);
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


        #endregion


        #region MODULO CONFIGURACION - CATALOGOS ALIMENTACION
        public ActionResult ConfiguraCatalogosForrajes()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8074)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/ALIMENTACION/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarPreciosArticulosForrajesTable()
        {
            try
            {
                var data = from art in db.C_articulos_catalogo
                           join art_basc in db.C_articulos_catalogo_bascula on art.id_articulo equals art_basc.id_articulo
                           join art_alim in db.C_alimentacion_articulos_precios on art.id_articulo equals art_alim.id_articulo into art_precios
                           from precios in art_precios.DefaultIfEmpty()
                           where art.activo == true && art_basc.activo == true && art_basc.id_ficha_tipo == 2
                           select new ArticulosPreciosForrajes
                           {
                               Articulos = art,
                               Precios = precios
                           };

                //var data = db.C_articulos_catalogo_bascula.Where(x => x.id_ficha_tipo == 2);
                return PartialView("ALIMENTACION/PreciosForrajes/_PreciosForrajesTable", data);
            }
            catch (Exception)
            {
                return PartialView("", null);
            }
        }

        public int GuardarPreciosArticulosForrajes(int[] id_articulos, decimal[] precios)
        {
            int id_usuario = (int)Session["LoggedId"];
            DateTime hoy = DateTime.Now;
            try
            {
                for (int i = 0; i < id_articulos.Length; i++)
                {
                    int id_articulo = id_articulos[i];
                    var valid = db.C_alimentacion_articulos_precios.Where(x => x.id_articulo == id_articulo).FirstOrDefault();
                    if (valid != null)
                    {
                        if (valid.precio_articulo != precios[i])
                        {
                            valid.id_usuario_ultima_actualizacion = id_usuario;
                            valid.fecha_ultima_actualizacion = hoy;
                            valid.precio_articulo = precios[i];
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        C_alimentacion_articulos_precios new_art_precio = new C_alimentacion_articulos_precios();
                        new_art_precio.id_articulo = id_articulo;
                        new_art_precio.precio_articulo = precios[i];
                        new_art_precio.id_usuario_registra = id_usuario;
                        new_art_precio.id_usuario_ultima_actualizacion = id_usuario;
                        new_art_precio.fecha_registro = hoy;
                        new_art_precio.fecha_ultima_actualizacion = hoy;
                        new_art_precio.activo = true;
                        db.C_alimentacion_articulos_precios.Add(new_art_precio);
                        db.SaveChanges();
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

        #region MODULO CONFIGURACION - ENVIOS LECHE CUMPLIMIENTO
        public ActionResult AdministracionCumplimientos()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8086)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../CONFIGURACION/ENVIOS_LECHE_CUMPLIMIENTO/Index");
        }

        public bool ValidarCargaCompromisoCliente(int idCliente, int idAnio)
        {
            try
            {
                var compromiso = db.C_envios_leche_cumplimiento_mensual.Where(x => x.activo == true && x.id_envio_leche_cliente_ms == idCliente && x.id_anio_presupuesto == idAnio).ToList();
                if (compromiso.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception) { return true; }
        }

        public PartialViewResult PreCargaCompromisoCliente(int compromisoCliente, int count_tabla, int anio)
        {
            var cliente = db.C_envios_leche_clientes.Find(compromisoCliente);
            var meses = db.C_presupuestos_meses.Where(x => x.activo == true).ToList();
            ViewBag.compromisoIDCliente = cliente.id_envio_leche_cliente_ms;
            ViewBag.compromisoCliente = cliente.nombre_comercial;
            ViewBag.count_tabla = count_tabla;
            ViewBag.compromisoMesPresupuesto = meses;
            ViewBag.compromisoAnio = anio;
            return PartialView("../CONFIGURACION/ENVIOS_LECHE_CUMPLIMIENTO/_CompromisoClienteTable");
        }

        public string[] ObtenerSemanasCompromiso(int month, int year)
        {
            System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            List<string> weeksNames = new List<string>();
            List<string> DiasInicioSemana = new List<string>();
            List<string> DiasFinSemana = new List<string>();

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
            int lastWeekOfMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek);

            // Ajustar el número de semanas correctamente
            DateTime firstDayOfFirstWeek = firstDayOfMonth;
            if (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Friday || firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Saturday || firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Sunday)
            {
                while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
                }
            }
            else { firstWeekOfMonth += 1; }

            // Contar cuántos días de la primera semana pertenecen al mes actual
            int daysInFirstWeek = 0;
            for (int i = 0; i < 7; i++)
            {
                if (firstDayOfFirstWeek.AddDays(i).Month == month)
                {
                    daysInFirstWeek++;
                }
            }

            int startingWeek = firstWeekOfMonth;
            if (daysInFirstWeek < 4)
            {
                startingWeek++; // Si el mes no tiene al menos 4 días en la primera semana, se la asignamos al mes anterior
            }

            int numberOfWeeksInMonth = lastWeekOfMonth - startingWeek + 1;


            for (int i = 0; i < numberOfWeeksInMonth; i++)
            {
                weeksNames.Add($"{startingWeek + i}");
            }

            var mesSemanas = new
            {
                SemanasNumero = weeksNames
            };

            return weeksNames.ToArray();
        }

        public string[] ObtenerSemanasCompromisoMensual(int month, int year)
        {
            bool semanaIncompleta = false;
            System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            // Lista para almacenar el inicio y fin de cada semana
            List<string> weeksNames = new List<string>();

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
            int numberOfWeeksInMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek) - firstWeekOfMonth + 1;

            DateTime firstDayOfFirstWeek = firstDayOfMonth;

            //VALIDACION SI LA SEMANA INICIA CON 4 DIAS PRINCIPALES (LUNES, MARTES, MIERCOLES, JUEVES)
            if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Monday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Tuesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Wednesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Thursday)
            {
                while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
                }
            }
            else if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Friday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Saturday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Sunday) { semanaIncompleta = true; }
            //VALIDACION SI LA ULTIMA SEMANA INICIA CON LOS PRIMEROS TRES DIAS (LUNES, MARTES, MIERCOLES)
            if (lastDayOfMonth.DayOfWeek == DayOfWeek.Monday || lastDayOfMonth.DayOfWeek == DayOfWeek.Tuesday || lastDayOfMonth.DayOfWeek == DayOfWeek.Wednesday)
            {
                numberOfWeeksInMonth -= 1;
            }

            if (semanaIncompleta == true)
            {
                for (int i = 0; i < numberOfWeeksInMonth - 1; i++)
                {
                    weeksNames.Add($"{firstWeekOfMonth + 1 + i}");
                }
            }
            else
            {
                for (int i = 0; i < numberOfWeeksInMonth; i++)
                {
                    weeksNames.Add($"{firstWeekOfMonth + i}");
                }
            }

            return weeksNames.ToArray();
        }

        public List<(int Semana, string FechaInicio)> ObtenerSemanasCompromisoMensualFechas(int month, int year)
        {
            bool semanaIncompleta = false;
            System.Globalization.Calendar calendar = CultureInfo.InvariantCulture.Calendar;
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            List<(int Semana, string FechaInicio)> semanas = new List<(int, string)>();

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            int firstWeekOfMonth = calendar.GetWeekOfYear(firstDayOfMonth, weekRule, firstDayOfWeek);
            int numberOfWeeksInMonth = calendar.GetWeekOfYear(lastDayOfMonth, weekRule, firstDayOfWeek) - firstWeekOfMonth + 1;

            DateTime firstDayOfFirstWeek = firstDayOfMonth;

            // Ajustar el primer día de la semana al lunes anterior si es necesario
            if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Monday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Tuesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Wednesday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Thursday)
            {
                while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                {
                    firstDayOfFirstWeek = firstDayOfFirstWeek.AddDays(-1);
                }
            }
            else if (firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Friday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Saturday || firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Sunday)
            {
                semanaIncompleta = true;
            }

            // Ajustar si la última semana comienza con lunes, martes o miércoles
            if (lastDayOfMonth.DayOfWeek == DayOfWeek.Monday || lastDayOfMonth.DayOfWeek == DayOfWeek.Tuesday || lastDayOfMonth.DayOfWeek == DayOfWeek.Wednesday)
            {
                numberOfWeeksInMonth -= 1;
            }

            if (semanaIncompleta)
            {
                int diferencia = 1;
                for (int i = 0; i < numberOfWeeksInMonth - 1; i++)
                {
                    DateTime inicioSemana = firstDayOfFirstWeek.AddDays(i * 7);
                    while (firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Monday && firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Tuesday && firstDayOfFirstWeek.DayOfWeek != DayOfWeek.Wednesday && firstDayOfFirstWeek.DayOfWeek == DayOfWeek.Thursday)
                    {
                        inicioSemana = firstDayOfFirstWeek.AddDays(i * 7 + diferencia);
                        diferencia++;
                    }
                    semanas.Add((firstWeekOfMonth + 1 + i, inicioSemana.ToString("dd/MM/yyyy")));
                }
            }
            else
            {
                for (int i = 0; i < numberOfWeeksInMonth; i++)
                {
                    DateTime inicioSemana = firstDayOfFirstWeek.AddDays(i * 7);
                    semanas.Add((firstWeekOfMonth + i, inicioSemana.ToString("dd/MM/yyyy")));
                }
            }

            return semanas;
        }




        public bool CargaCompromisoCliente(int idCliente, int idAnio, int[] mes, decimal[] litros_mensual, decimal[] litros_dia, decimal[] full_mensual)
        {
            try
            {
                for (int i = 0; i < mes.Length; i++)
                {
                    //REGISTRO DE COMRPOMISO MENSUAL
                    C_envios_leche_cumplimiento_mensual mensual = new C_envios_leche_cumplimiento_mensual();
                    mensual.id_envio_leche_cliente_ms = idCliente;
                    mensual.id_anio_presupuesto = idAnio;
                    mensual.id_mes_presupuesto = mes[i];
                    mensual.litros_totales = litros_mensual[i];
                    mensual.litros_dia = litros_dia[i];
                    mensual.full_semana = full_mensual[i];
                    mensual.activo = true;
                    db.C_envios_leche_cumplimiento_mensual.Add(mensual);
                    db.SaveChanges();

                    int anioCompromiso = Convert.ToInt32(db.C_presupuestos_anios.Find(idAnio).anio);
                    //REGISTRO DE COMPROMISO SEMANAL
                    foreach (var item in ObtenerSemanasCompromisoMensual(mes[i], anioCompromiso))
                    {
                        C_envios_leche_cumplimiento_semanal semanal = new C_envios_leche_cumplimiento_semanal();
                        semanal.id_cumplimiento_mensual = mensual.id_cumplimiento_mensual;
                        semanal.no_semana = Convert.ToInt32(item);
                        semanal.activo = true;
                        db.C_envios_leche_cumplimiento_semanal.Add(semanal);
                    }
                    db.SaveChanges();

                    var semanas_mensual = ObtenerSemanasCompromisoMensual(mes[i], anioCompromiso);
                    var semanas_mensual_fechas = ObtenerSemanasCompromisoMensualFechas(mes[i], anioCompromiso);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CargaCompromisoClienteEdit(int compromisoCliente, int idAnio, int[] mes, decimal[] litros_mensual, decimal[] litros_dia, decimal[] full_mensual)
        {
            var compromisos = db.C_envios_leche_cumplimiento_mensual.Where(x => x.id_envio_leche_cliente_ms == compromisoCliente && x.id_anio_presupuesto == idAnio && x.activo == true).ToList();
            try
            {
                compromisos.ForEach(x => x.activo = false);
                for (int i = 0; i < mes.Length; i++)
                {
                    C_envios_leche_cumplimiento_mensual mensual = new C_envios_leche_cumplimiento_mensual();
                    mensual.id_envio_leche_cliente_ms = compromisoCliente;
                    mensual.id_anio_presupuesto = idAnio;
                    mensual.id_mes_presupuesto = mes[i];
                    mensual.litros_totales = litros_mensual[i];
                    mensual.litros_dia = litros_dia[i];
                    mensual.full_semana = full_mensual[i];
                    mensual.activo = true;
                    db.C_envios_leche_cumplimiento_mensual.Add(mensual);
                }
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public PartialViewResult TablaCompromisoClientes(int idAnio)
        {
            if (idAnio == 0)
            {
                var cliente = db.C_envios_leche_cumplimiento_mensual.Where(x => x.activo == true).ToList();
                return PartialView("../CONFIGURACION/ENVIOS_LECHE_CUMPLIMIENTO/_CompromisosClienteTable", cliente);
            }
            else
            {
                var cliente = db.C_envios_leche_cumplimiento_mensual.Where(x => x.id_anio_presupuesto == idAnio && x.activo == true).ToList();
                return PartialView("../CONFIGURACION/ENVIOS_LECHE_CUMPLIMIENTO/_CompromisosClienteTable", cliente);
            }
        }

        public PartialViewResult MostrarCompromisoCliente(int idCliente, int idAnio)
        {
            var compromiso = db.C_envios_leche_cumplimiento_mensual.Where(x => x.id_envio_leche_cliente_ms == idCliente && x.id_anio_presupuesto == idAnio && x.activo == true).ToList();
            return PartialView("../CONFIGURACION/ENVIOS_LECHE_CUMPLIMIENTO/_MostrarCompromisoCliente", compromiso);
        }
        #endregion

        #region MODULO CONFIGURACION - USUARIOS MASTERS 

        public ActionResult ConfiguracionUMaster()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8098)) { return View("/Views/Home/Index.cshtml"); }

                return View("USUARIOS_MASTERS/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }


        }

        public PartialViewResult ConsultarUsuarioMaster()
        {
            var Data = db.C_usuarios_masters_acciones.Where(x => x.activo == true).ToList();

            return PartialView("../CATALOGOS/USUARIOS/_UsuariosMasterAccionesSelect", Data);
        }

        public PartialViewResult ConsultarUsuarioMasterAccionTable(int id_accion)
        {
            var DataUser = db.C_usuarios_masters.Where(x => x.id_usuario_master_accion == id_accion)
                .OrderBy(x => x.C_usuarios_corporativo.usuario).ToList();

            return PartialView("USUARIOS_MASTERS/_UsuariosMastersTable", DataUser);
        }

        public int OnOffUsuarioMaster(int id_usuario_master, bool activo)
        {
            try
            {
                var usuario = db.C_usuarios_masters.Find(id_usuario_master);
                usuario.activo = activo;
                db.SaveChanges();
                return 1;
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public int RegistrarUsuarioMaster(int id_usuario_master, int usuario_accion, string correo_usuario)
        {
            try
            {
                var valid = db.C_usuarios_masters.Where(x => x.id_usuario_master == id_usuario_master && x.id_usuario_master_accion == usuario_accion).FirstOrDefault();
                if (valid != null)
                {
                    valid.activo = true;
                    db.SaveChanges();
                    return 0; //Ya existe el usuario master con esa accion
                }
                else
                {
                    C_usuarios_masters new_usuario_master = new C_usuarios_masters();
                    if (id_usuario_master != 0)
                    {
                        new_usuario_master.id_usuario_corporativo = id_usuario_master;
                    }
                    new_usuario_master.id_usuario_master_accion = usuario_accion;
                    new_usuario_master.correo = correo_usuario;
                    new_usuario_master.activo = true;
                    db.C_usuarios_masters.Add(new_usuario_master);
                    db.SaveChanges();
                    return 0; //Registro exitoso
                }
            }
            catch (Exception)
            {

                return -1; //Error al registrar el usuario master
            }

        }

        #endregion
    }
}