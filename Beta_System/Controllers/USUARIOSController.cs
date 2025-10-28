using Beta_System.Helper;
using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace Beta_System.Controllers
{
    public class USUARIOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();


        public ActionResult ConsultarUsuariosCorporativoSelect()
        {
            var usuarios = db.C_usuarios_corporativo.Where(x => x.Activo == true).OrderBy(x=>x.usuario).ToList();
            return PartialView("../CATALOGOS/Usuarios/UsuariosCorporativoSelect", usuarios);
        }

        public PartialViewResult ConsultarUsuariosEmpleado(int id_empleado)
        {
            var usuarios = db.C_usuarios_corporativo.Where(x => x.id_empleado == id_empleado).OrderBy(x=>x.usuario).ToList();
            return PartialView("../CONFIGURACION/USUARIOS/_UsuariosEmpleado", usuarios);
        }

        public int RegistrarUsuario(C_usuarios_corporativo c_usuario)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                var validPermiso = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario && x.id_permiso == 4).FirstOrDefault(); //4: REGISTRA USUARIO
                if (validPermiso == null)
                {
                    return 1;
                }

                c_usuario.password = PasswordHelper.EncodePassword("123456", "MySalt");
                c_usuario.fecha_alta = System.DateTime.Now;
                c_usuario.Activo = true;
                db.C_usuarios_corporativo.Add(c_usuario);
                db.SaveChanges();
                return 0;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return -1;
            }
        }

        public string EditarUsuario(int id_usuario)
        {
            var usuario = from us in db.C_usuarios_corporativo
                           where us.id_usuario_corporativo == id_usuario
                           select new { us.usuario, us.id_rol };

            return Newtonsoft.Json.JsonConvert.SerializeObject(usuario);
        }

        public int ActualizarUsuario(int id_usuario, string usuario, int id_rol)
        {
            try
            {
                int id_usuario_ses = (int)Session["LoggedId"];
                var validPermiso = db.C_modulos_sub_permisos_usuarios_asignados.Where(x => x.id_usuario == id_usuario_ses && x.id_permiso == 10).FirstOrDefault(); //10: ACTUALIZAR USUARIO
                if (validPermiso == null)
                {
                    return 1;
                }

                var usu = db.C_usuarios_corporativo.Find(id_usuario);
                usu.usuario = usuario;
                usu.id_rol = id_rol;
                db.SaveChanges(); 
                return 0;
            }
            catch (Exception ex)
            {
                string mjs = ex.ToString();
                return -1;
            }
        }

        public bool ResetearPassword(int id_usuario)
        {
            try
            {
                var usu = db.C_usuarios_corporativo.Find(id_usuario);
                usu.password = PasswordHelper.EncodePassword("123456", "MySalt");
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OnOffUsuario(int id_usuario, bool modo)
        {
            try
            {
                var usu = db.C_usuarios_corporativo.Find(id_usuario);
                usu.Activo = modo;
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