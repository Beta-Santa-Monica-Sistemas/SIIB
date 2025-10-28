using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Beta_System.Controllers
{
    public class ROLESController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public PartialViewResult ConsultarRoles(int id_status)
        {
            List<C_usuarios_roles> roles = null;
            //1: ACTIVOS
            if (id_status == 1)
            {
                roles = db.C_usuarios_roles.Where(x => x.estatus == true).OrderBy(x=>x.nombre_rol).ToList();
            }
            //2: INACTIVOS

            //0: TODOS

            return PartialView("../CATALOGOS/ROLES/_RolesSistemaSelect", roles);
        }


        public PartialViewResult ConsultarRolesSistema()
        {
            var roles = db.C_usuarios_roles.OrderBy(x=>x.nombre_rol).ToList();
            return PartialView("../CATALOGOS/ROLES/_RolesSistemaTable", roles);
        }

        public PartialViewResult VerUsuariosRolTable(int id_rol)
        {
            var usuarios = db.C_usuarios_corporativo.Where(x => x.id_rol == id_rol).OrderBy(x=>x.usuario).ToList();
            return PartialView("../CATALOGOS/ROLES/_UsuariosRolesTable", usuarios);
        }

        public PartialViewResult ConsultarTipoUsuarios()
        {
            var tipos = db.C_usuarios_tipo.ToList();
            return PartialView("../CATALOGOS/ROLES/_TiposUsuarioSelect", tipos);
        }

        public bool RegistrarRol(string nombre_rol, int id_usuario_tipo)
        {
            try
            {
                C_usuarios_roles rol = new C_usuarios_roles();
                rol.id_usuario_tipo = id_usuario_tipo;
                rol.nombre_rol = nombre_rol;
                rol.estatus = true;
                db.C_usuarios_roles.Add(rol);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ActualizarRol(int id_rol, string nombre_rol, int id_tipo_usuario)
        {
            try
            {
                var rol = db.C_usuarios_roles.Find(id_rol);
                rol.nombre_rol = nombre_rol;
                rol.id_usuario_tipo = id_tipo_usuario;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



    }
}