using Beta_System.HUBS;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Beta_System.Models;

namespace Beta_System.Controllers
{
    public class ADMIN_SIIBController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        #region NOTIFICACIONES SIIB

        //public ActionResult NotificacionesGlobales()
        //{
        //    try
        //    {
        //        List<int> permisos = Session["sub_modulos_session"] as List<int>;
        //        if (!permisos.Contains(10107)) { return View("/Views/Home/Index.cshtml"); }
        //    }
        //    catch (Exception)
        //    {
        //        RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
        //    }

        //    return View("Notificaciones/Index");
        //}

        //public ActionResult NotificarAvisoGlobal(string mensaje)
        //{
        //    var contexto = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();
        //    contexto.Clients.All.NotificarAvisoGlobal(mensaje);
        //    return Content("Evento lanzado");
        //}



        #endregion


        #region ADMINISTRAR MENU SIIB
        public ActionResult AdministrarMenu()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(10108)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("AdminMenu/Index");
        }

        public PartialViewResult ConsultarMenuSIIBAmin()
        {
            return PartialView("AdminMenu/_MenuSIIBTree", db.C_modulos);
        }

        public int AgregarActualizarModuloSIIB(int id_modulo, string nombre, string icono)
        {
            try
            {
                if (id_modulo == 0)
                {
                    C_modulos new_modulo = new C_modulos();
                    new_modulo.nombre = nombre;
                    new_modulo.icono = icono;
                    new_modulo.activo = true;
                    db.C_modulos.Add(new_modulo);
                }
                else
                {
                    var modulo = db.C_modulos.Find(id_modulo);
                    modulo.nombre = nombre;
                    modulo.icono = icono;
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public int ActualizarSubModulo(int id_submodulo, int id_modulo, string nombre, string controlador, string funcion, int comportamiento, int id_submenu_modulo_sub)
        {
            try
            {
                if (id_submodulo == 0)
                {
                    C_modulos_sub new_submodulo = new C_modulos_sub();
                    new_submodulo.id_modulo = id_modulo;
                    new_submodulo.nombre = nombre;
                    new_submodulo.controlador = controlador;
                    new_submodulo.funcion = funcion;
                    new_submodulo.aplica_submenu = comportamiento == 2 ? true : false;
                    new_submodulo.id_submenu_modulo_sub = id_submenu_modulo_sub == 0 ? (int?)null : id_submenu_modulo_sub;
                    new_submodulo.activo = true;
                    db.C_modulos_sub.Add(new_submodulo);
                }
                else
                {
                    var submodulo = db.C_modulos_sub.Find(id_submodulo);
                    submodulo.id_modulo = id_modulo;
                    submodulo.nombre = nombre;
                    submodulo.controlador = controlador;
                    submodulo.funcion = funcion;
                    submodulo.aplica_submenu = comportamiento == 2 ? true : false;
                    submodulo.id_submenu_modulo_sub = id_submenu_modulo_sub == 0 ? (int?)null : id_submenu_modulo_sub;
                }
                db.SaveChanges();
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }



        #endregion




    }
}