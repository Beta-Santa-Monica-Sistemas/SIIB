using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class MODULOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();

        public ActionResult AsignarModulosRoles()
        {
            List<int> permisos = Session["sub_modulos_session"] as List<int>;
            if (!permisos.Contains(7)) { return View("/Views/Home/Index.cshtml"); }

            return View("../CONFIGURACION/MODULOS/Index");
        }

        public PartialViewResult ConsultarModulosSelect()
        {
            var modulos = db.C_modulos.Where(x => x.activo == true).OrderBy(x=>x.nombre).ToList();
            return PartialView("../CONFIGURACION/MODULOS/_ModulosSelect",modulos);
        }

        public PartialViewResult ConsultarSubModulosModuloSelect(int id_modulo)
        {
            var modulos = db.C_modulos_sub.Where(x => x.activo == true && x.id_modulo == id_modulo).OrderBy(x=>x.nombre).ToList();
            return PartialView("../CONFIGURACION/MODULOS/_SubModulosSelect", modulos);
        }

        public PartialViewResult ConsultarSubModulosPrincipalesSelect(int id_modulo)
        {
            var modulos = db.C_modulos_sub.Where(x => x.activo == true && x.id_modulo == id_modulo && x.aplica_submenu == true).OrderBy(x => x.nombre).ToList();
            return PartialView("../CONFIGURACION/MODULOS/_SubModulosSelect", modulos);
        }

        public PartialViewResult ConsultarSubmodulos()
        {
            var modulos = from mod_g in db.C_modulos
                          join mod_s in db.C_modulos_sub on mod_g.IdModulo equals mod_s.id_modulo

                          //join mod_p in db.C_modulos_sub_permisos on mod_s.id_modulos_sub equals mod_p.id_modulos_sub into ModulosUsuario
                          //from mod_u in ModulosUsuario.DefaultIfEmpty()
                          //where mod_u.id_rol == id_rol
                          //select new SubModulosRol { c_modulos = mod_g, c_modulos_sub = mod_s, c_modulos_sub_perm = mod_u  };
                          select mod_s;
            return PartialView("../CONFIGURACION/MODULOS/_SubModulosRol", modulos.OrderBy(x=>x.nombre).ToList());
        }

        public bool ValidarModulosAsignadosRol(int id_rol, int id_modulos_sub)
        {
            var valid = db.C_modulos_sub_permisos.Where(x => x.id_rol == id_rol && x.id_modulos_sub == id_modulos_sub && x.estatus == true).FirstOrDefault();
            if (valid != null) { return true; }
            else { return false; }
        }

        public bool AsignarModulosRol(int id_rol, int[] id_sub_modulos)
        {
            try
            {
                var modulos_asignados = db.C_modulos_sub_permisos.Where(x => x.id_rol == id_rol).ToList();
                //db.C_modulos_sub_permisos.RemoveRange(modulos_asignados);
                modulos_asignados.ForEach(z => z.estatus = false);
                db.SaveChanges();

                if (id_sub_modulos != null)
                {
                    for (int i = 0; i < id_sub_modulos.Length; i++)
                    {
                        if (modulos_asignados.Select(x => x.id_modulos_sub).Contains(id_sub_modulos[i]))
                        {
                            modulos_asignados.Where(x => x.id_modulos_sub == id_sub_modulos[i]).FirstOrDefault().estatus = true;
                            db.SaveChanges();
                        }
                        else
                        {
                            C_modulos_sub_permisos sub = new C_modulos_sub_permisos();
                            sub.id_modulos_sub = id_sub_modulos[i];
                            sub.id_rol = id_rol;
                            sub.estatus = true;
                            db.C_modulos_sub_permisos.Add(sub);
                            db.SaveChanges();
                        }
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


        public int SyncModulosRoles()
        {
            try
            {
                var roles = db.C_usuarios_roles.Where(x => x.estatus == true).ToList();
                var sub_modulos_g = db.C_modulos_sub.Where(x => x.activo == true && x.aplica_submenu == true).ToList();

                foreach (var sub_modulo in sub_modulos_g)
                {
                    int id_sub_modulo_g = sub_modulo.id_modulos_sub;
                    foreach (var rol in roles)
                    {
                        int id_rol_g = rol.id_rol;
                        var existing = db.C_modulos_sub.Where(x => x.id_submenu_modulo_sub == id_sub_modulo_g && x.activo == true).Select(x => x.id_modulos_sub);
                        if (db.C_modulos_sub_permisos.Where(x => x.id_rol == id_rol_g && existing.Contains((int)x.id_modulos_sub)).Count() > 0)
                        {
                            var valid_modulo = db.C_modulos_sub_permisos.Where(x => x.id_rol == id_rol_g && x.id_modulos_sub == id_sub_modulo_g && x.estatus == true).FirstOrDefault();
                            if (valid_modulo == null)
                            {
                                C_modulos_sub_permisos sub = new C_modulos_sub_permisos();
                                sub.id_modulos_sub = id_sub_modulo_g;
                                sub.id_rol = id_rol_g;
                                sub.estatus = true;
                                db.C_modulos_sub_permisos.Add(sub);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }






    }
}