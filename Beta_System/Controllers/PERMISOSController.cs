using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class PERMISOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public ActionResult AsignarPermisosUsuario()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(8)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/PERMISOS/Index");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }
        
        
        public PartialViewResult ConsultarPermisosUsuario(int id_submodulo)
        {
            var permisos = db.C_modulos_sub_permisos_usuarios.Where(x => x.id_submodulo == id_submodulo && x.activo == true).OrderBy(x => x.nombre_permiso).ToList();
            return PartialView("../CONFIGURACION/PERMISOS/_PermisosSubmoduloTable", permisos);
        }

        public bool ValidarPermisoUsuario(int id_usuario, int id_permiso)
        {
            var Validar = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario && x.id_permiso == id_permiso).FirstOrDefault();
            if (Validar == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool AsignarPermisoUsuario(int id_usuario, int[] id_permiso, int id_submodulo)
        {
            try
            {
                var permiso_submodulos = db.C_modulos_sub_permisos_usuarios.Where(x => x.id_submodulo == id_submodulo).Select(x => x.id_submodulo_permiso_usuario).ToArray();
                var permisos_asignados = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario && permiso_submodulos.Contains((int)x.id_permiso)).ToList();
                //var permisos_asignados = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario);
                db.C_modulos_sub_permisos_usuarios_asignados.RemoveRange(permisos_asignados);
                db.SaveChanges();
                if (id_permiso != null)
                {
                    for (int i = 0; i < id_permiso.Length; i++)
                    {
                        C_modulos_sub_permisos_usuarios_asignados subperm = new C_modulos_sub_permisos_usuarios_asignados();
                        subperm.id_usuario = id_usuario;
                        subperm.id_permiso = id_permiso[i];
                        subperm.activo = true;
                        db.C_modulos_sub_permisos_usuarios_asignados.Add(subperm);
                    }
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public bool RemoverPermisoUsuario(int id_usuario, int id_submodulo)
        {
            try
            {
                var permiso_submodulos = db.C_modulos_sub_permisos_usuarios.Where(x => x.id_submodulo == id_submodulo).Select(x => x.id_submodulo_permiso_usuario).ToArray();
                var permisos_asignados = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario && permiso_submodulos.Contains((int)x.id_permiso)).ToList();
                //var permisos_asignados = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario);
                db.C_modulos_sub_permisos_usuarios_asignados.RemoveRange(permisos_asignados);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }



    }
}