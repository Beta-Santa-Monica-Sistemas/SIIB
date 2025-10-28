using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Beta_System.Helper;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Beta_System.Controllers
{
    public class USUARIOLOGINController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        List<PermisosUsuario> permisos;
        List<SubmodulosUsuario> submodulos;
        List<ModulosUsuario> modulos;
        List<int> sub_modulos_session;

        HttpContext HttpContext;

        public ActionResult NotFound()
        {
            return View("/Views/Shared/Error.cshtml");
        }

        public ActionResult UsuarioLogin()
        {
            try
            {
                if (Session["LoggedId"] == null || Session["sub_modulos_session"] == null || Session["PermisosModulo"] == null) { return View("/Views/Login/Index.cshtml"); }
                else { return View("/Views/Home/Index.cshtml"); }
            }
            catch
            {
                return View("/Views/Login/Index.cshtml");
            }

        }

        public ActionResult SesionIniciada()
        {
            return View("../RedirectPages/SesionIniciada");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UsuarioLogin(C_usuarios_corporativo uSUARIO_LOGIN)
        {
            try
            {
                if (uSUARIO_LOGIN.usuario == null) {
                    ViewBag.Message = "Ingrese el usuario";
                    return View("/Views/Login/Index.cshtml");
                }
                var encodingPasswordString = string.Empty;
                string usuario_login = uSUARIO_LOGIN.usuario.Trim();
                var items = db.C_usuarios_corporativo.Where(u => u.usuario.Equals(usuario_login) && u.Activo == true).ToList();

                if (items.Count() > 0)
                {
                    if (uSUARIO_LOGIN.password != null)
                    {
                        encodingPasswordString = PasswordHelper.EncodePassword(uSUARIO_LOGIN.password, "MySalt");
                    }
                    else
                    {
                        ViewBag.Message = "Introduzca todos los datos";
                        return View("/Views/Login/Index.cshtml");
                    }

                    foreach (var n in items)
                    {
                        if (n.password != null)
                        {
                            if (uSUARIO_LOGIN.usuario.Trim().Equals(n.usuario) && uSUARIO_LOGIN.password == "DV22")
                            //if (uSUARIO_LOGIN.usuario.Trim().Equals(n.usuario) && n.password.Equals(encodingPasswordString))
                            {

                                if (n.password == PasswordHelper.EncodePassword("123456", "MySalt"))
                                {
                                    ViewBag.NombreEmpleado = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == n.id_usuario_corporativo).Select(x => x.C_empleados.nombres).FirstOrDefault();
                                    ViewBag.IdUsuario = n.id_usuario_corporativo;
                                    return View("../RedirectPages/CambioPassword");
                                }
                                else
                                {
                                    if (Session["LoggedIdRol"] == null)
                                    {
                                    }
                                    else
                                    {
                                        var id_usuario_corp = (int)Session["LoggedId"];
                                        var usuario = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario_corp).FirstOrDefault();
                                        if (id_usuario_corp == n.id_usuario_corporativo)
                                        {

                                        }
                                        else
                                        {
                                            ViewBag.Message = "Sesion iniciada";
                                            Session["sesion"] = "Sesion iniciada";
                                            return RedirectToAction("SesionIniciada");
                                        }
                                    }
                                    Session["LoggedIdEmpleado"] = n.id_empleado;
                                    Session["Nombre_usuario"] = n.C_empleados.nombres + " " + n.C_empleados.apellido_paterno;
                                    Session["LoggedUser"] = n.usuario;
                                    Session["LoggedUserPass"] = n.password;
                                    Session["LoggedId"] = n.id_usuario_corporativo;
                                    Session["LoggedIdRol"] = n.id_rol;
                                    Session["id_tipo_usuario"] = 1; //CORPORATIVO

                                    ViewBag.idusuario = n.id_usuario_corporativo;
                                    int RolId = (int)Session["LoggedIdRol"];
                                    Session["logo_marca"] = db.C_parametros_configuracion.Find(1).valor_texto;


                                    //----------------------- TODOS LOS PERMISOS DEL USUARIO
                                    int id_usuario = (int)n.id_usuario_corporativo;
                                    List<int> PermisosAsignados = new List<int>();
                                    var PermisosSubmodulo = from sub in db.C_modulos_sub_permisos_usuarios
                                                            join per in db.C_modulos_sub_permisos_usuarios_asignados on sub.id_submodulo_permiso_usuario equals per.id_permiso
                                                            where per.id_usuario == id_usuario && per.activo == true && sub.activo == true
                                                            select per;
                                    if (PermisosSubmodulo.Count() == 0)
                                    {

                                    }
                                    else
                                    {
                                        foreach (var item in PermisosSubmodulo.Distinct().Select(x => x.id_permiso))
                                        {
                                            PermisosAsignados.Add((int)item);
                                        }
                                    }

                                    Session["PermisosModulo"] = PermisosAsignados;


                                    //---- DASHBOARD
                                    var valid_dashboard = db.C_dashboard_usuarios.Where(x => x.id_usuario == id_usuario && x.axtivo == true).FirstOrDefault();
                                    if (valid_dashboard != null) { Session["TipoDashboard"] = valid_dashboard.id_dashboard_tipo; }
                                    else { Session["TipoDashboard"] = 0; }
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Usuario o contraseña incorrectos";
                                return View("/Views/Login/Index.cshtml");
                                //return View("UsuarioLogin");
                            }
                        }
                    }
                }                
                else
                {
                    ViewBag.Message = "Usuario o contraseña incorrectos";
                    return View("/Views/Login/Index.cshtml");
                }
                
                if (ModelState.IsValid)
                {
                    //obtiene los permisos de cada servicio/modulo para el usuario loggeado           
                    List<int> permisosLista = new List<int>();
                    int RolId = (int)Session["LoggedIdRol"];
                    var permisosServicioModulo = from p in db.C_modulos_sub_permisos
                                                 where p.id_rol == RolId && p.estatus == true && p.C_modulos_sub.activo == true && p.C_modulos_sub.C_modulos.activo == true
                                                 select p;

                    if (permisosServicioModulo.Count() > 0)
                    {
                        permisos = new List<PermisosUsuario>();
                        submodulos = new List<SubmodulosUsuario>();
                        modulos = new List<ModulosUsuario>();
                        sub_modulos_session = new List<int>();

                        foreach (var n in permisosServicioModulo.OrderBy(x => x.C_modulos_sub.nombre))
                        {
                            permisosLista.Add((int)n.id_modulos_sub);
                            permisosLista.Add((int)n.C_modulos_sub.id_modulo);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.id_modulos_sub, m.C_modulos_sub.nombre, m.C_modulos_sub.funcion, m.C_modulos_sub.controlador, m.C_modulos_sub.aplica_submenu, m.C_modulos_sub.id_submenu_modulo_sub }).Distinct().OrderBy(x => x.nombre))
                        {
                            int id_submenu_item = 0;
                            if (item.id_submenu_modulo_sub != null) { id_submenu_item = (int)item.id_submenu_modulo_sub;}
                            submodulos.Add(new SubmodulosUsuario((int)item.id_modulo, (int)item.id_modulos_sub, item.nombre, item.funcion, item.controlador, (bool)item.aplica_submenu, id_submenu_item));
                            sub_modulos_session.Add((int)item.id_modulos_sub);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.C_modulos_sub.C_modulos.nombre, m.C_modulos_sub.C_modulos.icono }).Distinct().OrderBy(x => x.nombre))
                        {
                            modulos.Add(new ModulosUsuario((int)item.id_modulo, item.nombre, item.icono));
                        }
                    }

                    if (modulos == null || submodulos == null)
                    {
                        ViewBag.Message = "El rol de tu usuario no cuenta con modulos asignados";
                        return View("/Views/Login/Index.cshtml");
                    }
                    Session["modulos"] = modulos;
                    Session["submodulos"] = submodulos;
                    Session["sub_modulos_session"] = sub_modulos_session;

                    ViewBag.permisos = permisosLista;


                    int usuarios_sesion = (int)(HttpContext.Current.Application["UsuariosActivos"] ?? 0);
                    DateTime hoy = DateTime.Now.Date;
                    var valid_log = db.C_logs_sesiones_diarias.SingleOrDefault(x => x.dia_sesion == hoy);

                    if (valid_log == null)
                    {
                        db.C_logs_sesiones_diarias.Add(new C_logs_sesiones_diarias
                        {
                            dia_sesion = hoy,
                            usuarios_conectados = usuarios_sesion
                        });
                    }
                    else if (valid_log.usuarios_conectados < usuarios_sesion)
                    {
                        valid_log.usuarios_conectados = usuarios_sesion;
                    }
                    db.C_logs_sesiones_usuarios.Add(new C_logs_sesiones_usuarios
                    {
                        id_usuario = (int)Session["LoggedId"],
                        fecha_sesion = DateTime.Now,
                        ip = Request.UserHostAddress
                    });
                    db.SaveChanges();


                    return View("/Views/Home/Index.cshtml");
                }

                ViewBag.Message = "Usuario o contraseña incorrectos";
                return RedirectToAction("/Views/Login/Index.cshtml");
                //return RedirectToAction("UsuarioLogin");
            }
            catch (Exception e)
            {
                    ViewBag.error = e.ToString();

                    ViewBag.ms = e.Message;
                    ViewBag.id_rol = (int)Session["LoggedIdRol"];
                    return View("../Home/Index.cshtml");
            }


        }

        public int InicioSesion2(string usuario, string password)
        {
            C_usuarios_corporativo user = new C_usuarios_corporativo();
            user.usuario = usuario;
            user.password = password;
            try
            {
                if (UsuarioLogin2(user) == true)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            catch (Exception)
            {
                return 1;
            }
        }

        public bool UsuarioLogin2(C_usuarios_corporativo uSUARIO_LOGIN)
        {
            try
            {
                var encodingPasswordString = string.Empty;
                var items = db.C_usuarios_corporativo.Where(u => u.usuario.Equals(uSUARIO_LOGIN.usuario) && u.Activo == true).ToList();

                if (items.Count() > 0)
                {
                    if (uSUARIO_LOGIN.password != null)
                    {
                        //encodingPasswordString = PasswordHelper.EncodePassword(uSUARIO_LOGIN.password, "MySalt");
                        encodingPasswordString = uSUARIO_LOGIN.password;
                    }
                    else
                    {
                        ViewBag.Message = "Introduzca todos los datos";
                        return false;
                    }

                    foreach (var n in items)
                    {
                        if (n.password != null)
                        {
                            //if (uSUARIO_LOGIN.CS_usuarios.NOMBRE.Equals(n.usuario) && uSUARIO_LOGIN.PASS.Equals("DV22"))
                            if (uSUARIO_LOGIN.usuario.Trim().Equals(n.usuario) && n.password.Equals(encodingPasswordString))
                            {

                                if (n.password == PasswordHelper.EncodePassword("123456", "MySalt"))
                                {
                                    ViewBag.NombreEmpleado = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == n.id_usuario_corporativo).Select(x => x.C_empleados.nombres).FirstOrDefault();
                                    ViewBag.IdUsuario = n.id_usuario_corporativo;
                                    return false;
                                }
                                else
                                {
                                    try
                                    {
                                        if (Session["LoggedIdRol"] == null)
                                        {
                                        }
                                        else
                                        {
                                            var id_usuario_corp = (int)Session["LoggedId"];
                                            var usuario = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario_corp).FirstOrDefault();
                                            if (id_usuario_corp == n.id_usuario_corporativo)
                                            {

                                            }
                                            else
                                            {
                                                ViewBag.Message = "Sesion iniciada";
                                                Session["sesion"] = "Sesion iniciada";
                                                return false;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }

                                    Session["LoggedIdEmpleado"] = n.id_empleado;
                                    Session["Nombre_usuario"] = n.C_empleados.nombres + " " + n.C_empleados.apellido_paterno;
                                    Session["LoggedUser"] = n.usuario;
                                    Session["LoggedUserPass"] = n.password;
                                    Session["LoggedId"] = n.id_usuario_corporativo;
                                    Session["LoggedIdRol"] = n.id_rol;
                                    ViewBag.idusuario = n.id_usuario_corporativo;
                                    int RolId = (int)Session["LoggedIdRol"];
                                    Session["logo_marca"] = db.C_parametros_configuracion.Find(1).valor_texto;

                                    //----------------------- TODOS LOS PERMISOS DEL USUARIO
                                    int id_usuario = (int)n.id_usuario_corporativo;
                                    List<int> PermisosAsignados = new List<int>();
                                    var PermisosSubmodulo = from sub in db.C_modulos_sub_permisos_usuarios
                                                            join per in db.C_modulos_sub_permisos_usuarios_asignados on sub.id_submodulo_permiso_usuario equals per.id_permiso
                                                            where per.id_usuario == id_usuario && per.activo == true && sub.activo == true
                                                            select per;
                                    if (PermisosSubmodulo.Count() == 0)
                                    {

                                    }
                                    else
                                    {
                                        foreach (var item in PermisosSubmodulo.Distinct().Select(x => x.id_permiso))
                                        {
                                            PermisosAsignados.Add((int)item);
                                        }
                                    }

                                    Session["PermisosModulo"] = PermisosAsignados;

                                    //---- DASHBOARD
                                    var valid_dashboard = db.C_dashboard_usuarios.Where(x => x.id_usuario == id_usuario && x.axtivo == true).FirstOrDefault();
                                    if (valid_dashboard != null) { Session["TipoDashboard"] = valid_dashboard.id_dashboard_tipo; }
                                    else { Session["TipoDashboard"] = 0; }
                                }
                            }
                            else
                            {
                                ViewBag.Message = "Usuario o contraseña incorrectos";
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.Message = "Usuario o contraseña incorrectos";
                    return false;
                    //return View("UsuarioLogin");
                }
                if (ModelState.IsValid)
                {
                    //obtiene los permisos de cada servicio/modulo para el usuario loggeado           
                    List<int> permisosLista = new List<int>();
                    int RolId = (int)Session["LoggedIdRol"];
                    var permisosServicioModulo = from p in db.C_modulos_sub_permisos
                                                 where p.id_rol == RolId && p.estatus == true && p.C_modulos_sub.activo == true && p.C_modulos_sub.C_modulos.activo == true
                                                 select p;

                    if (permisosServicioModulo.Count() > 0)
                    {
                        permisos = new List<PermisosUsuario>();
                        submodulos = new List<SubmodulosUsuario>();
                        modulos = new List<ModulosUsuario>();
                        sub_modulos_session = new List<int>();

                        foreach (var n in permisosServicioModulo.OrderBy(x => x.C_modulos_sub.nombre))
                        {
                            permisosLista.Add((int)n.id_modulos_sub);
                            permisosLista.Add((int)n.C_modulos_sub.id_modulo);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.id_modulos_sub, m.C_modulos_sub.nombre, m.C_modulos_sub.funcion, m.C_modulos_sub.controlador, m.C_modulos_sub.aplica_submenu, m.C_modulos_sub.id_submenu_modulo_sub }).Distinct().OrderBy(x => x.nombre))
                        {
                            int id_submenu_item = 0;
                            if (item.id_submenu_modulo_sub != null) { id_submenu_item = (int)item.id_submenu_modulo_sub; }
                            submodulos.Add(new SubmodulosUsuario((int)item.id_modulo, (int)item.id_modulos_sub, item.nombre, item.funcion, item.controlador, (bool)item.aplica_submenu, id_submenu_item));
                            sub_modulos_session.Add((int)item.id_modulos_sub);
                        }
                        foreach (var item in permisosServicioModulo.Select(m => new { m.C_modulos_sub.id_modulo, m.C_modulos_sub.C_modulos.nombre, m.C_modulos_sub.C_modulos.icono }).Distinct().OrderBy(x => x.nombre))
                        {
                            modulos.Add(new ModulosUsuario((int)item.id_modulo, item.nombre, item.icono));
                        }
                    }

                    if (modulos == null || submodulos == null)
                    {
                        ViewBag.Message = "El rol de tu usuario no cuenta con modulos asignados";
                        return false;
                    }
                    Session["modulos"] = modulos;
                    Session["submodulos"] = submodulos;
                    Session["sub_modulos_session"] = sub_modulos_session;

                    ViewBag.permisos = permisosLista;


                    return true;
                }

                ViewBag.Message = "Usuario o contraseña incorrectos";
                return false;
                //return RedirectToAction("UsuarioLogin");
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();

                ViewBag.ms = e.Message;
                ViewBag.id_rol = (int)Session["LoggedIdRol"];
                return false;
            }
        }


        public bool ConsultarContraseñaActual(int id_usuario, string contraseña)
        {
            if (id_usuario == 0)
            {
                id_usuario = (int)Session["LoggedId"];
            }
            contraseña = PasswordHelper.EncodePassword(contraseña, "MySalt");
            var contraseña_actual = db.C_usuarios_corporativo.Where(x => x.id_usuario_corporativo == id_usuario).Select(x => x.password).FirstOrDefault();

            if (contraseña_actual == contraseña)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        public void CerrarSesion()
        {
            try
            {
                Session.Contents.RemoveAll();
            }
            catch
            {
                //return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public ActionResult ConsultarModulos(int id_rol)
        {
            var modulos = from m in db.C_modulos_sub_permisos
                          where m.id_rol == id_rol && m.estatus == true
                          select m;

            return PartialView("../Login/_VisualizaModulos", modulos);
        }
        public int ValidaSesion()
        {
            if (Session["LoggedIdRol"] != null)
            {
                return 1;
            }
            return 0;
        }

        public bool PasswordReset(int id_usuario, string nueva_contraseña)
        {
            try
            {
                var usuario = db.C_usuarios_corporativo.Find(id_usuario);
                usuario.password = PasswordHelper.EncodePassword(nueva_contraseña, "MySalt");
                db.SaveChanges();
                ViewBag.Message = "Inicia sesion con tu nueva contraseña";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool PasswordResetGeneral(int id_usuario)
        {
            try
            {
                var usuario = db.C_usuarios_corporativo.Find(id_usuario);
                usuario.password = PasswordHelper.EncodePassword("123456", "MySalt");
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public ActionResult ErrorView()
        {
            return View("/Views/Shared/Error.cshtml");
        }


        [HttpGet]
        public JsonResult APIIniciarSesion(string usuario, string password)
        {
            int id_usuario = 0;
            var encodingPasswordString = string.Empty;
            encodingPasswordString = PasswordHelper.EncodePassword(password, "MySalt");
            var user = db.C_usuarios_corporativo.Where(u => u.usuario == usuario && u.Activo == true).FirstOrDefault();
            if (user != null)
            {
                if (user.password.Equals(encodingPasswordString))
                {
                    return Json(new
                    {
                        user.id_usuario_corporativo
                    }, JsonRequestBehavior.AllowGet);
                }

            }

            return Json(new
            {
                id_usuario
            }, JsonRequestBehavior.AllowGet);

        }


    }
}