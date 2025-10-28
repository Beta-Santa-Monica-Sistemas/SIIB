using Beta_System.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Beta_System.Controllers
{
    public class EMPLEADOSController : Controller
    {

        private BETA_CORPEntities db = new BETA_CORPEntities();
        private PERMISOSController permiso = new PERMISOSController();
        private PERMISOSController PermisosController = new PERMISOSController();

        public ActionResult AdministradorEmpleados()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(10)) { return View("/Views/Home/Index.cshtml"); }

                return View("../CONFIGURACION/Empleados/AdministradorEmpleados");
            }
            catch (Exception)
            {
                return RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }
        }

        public PartialViewResult ConsultarEmpleadosSistema(int modo)
        {
            List<C_empleados> empleados = null;
            if (modo == 0)
            {
                empleados = db.C_empleados.OrderBy(x => x.nombres).OrderBy(x=>x.nombres).ToList();
            }

            return PartialView("../CONFIGURACION/Empleados/_EmpleadosTable", empleados);
        }

        public bool RegistrarEmpleado(C_empleados c_empleados)
        {
            try
            {
                c_empleados.id_estado_activo = true;
                c_empleados.fecha_ingreso = DateTime.Now;
                db.C_empleados.Add(c_empleados);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string EditarEmpleado(int id_empleado)
        {
            var empleado = from emp in db.C_empleados
                           where emp.id_empleado == id_empleado
                           select new { emp.nombres, emp.apellido_materno, emp.apellido_paterno, emp.telefono_celular, emp.correo };
            return Newtonsoft.Json.JsonConvert.SerializeObject(empleado);
        }

        public bool ActualizarEmpleado(int id_empleado, string nombres, string ap_pat, string ap_mat, string correo, string telefono_celular)
        {
            try
            {
                var empl = db.C_empleados.Find(id_empleado);
                empl.nombres = nombres;
                empl.apellido_paterno = ap_pat;
                empl.apellido_materno = ap_mat;
                empl.correo = correo;
                empl.telefono_celular = telefono_celular;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
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
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public PartialViewResult ConsultarEmpleadosActivosSelect()
        {
            var empleados = db.C_empleados.Where(x => x.id_estado_activo == true).OrderBy(x=>x.nombres).ToList();
            return PartialView("../CONFIGURACION/EMPLEADOS/_EmpleadosSistemaSelect", empleados);
        }

    }
}