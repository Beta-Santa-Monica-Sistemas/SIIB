using Antlr.Runtime.Tree;
using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class EQUIPOS_ESTABLOSController : Controller
    {
        private PERMISOSController permiso = new PERMISOSController();
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6026)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../CATALOGOS/EQUIPOS_ESTABLOS/Index");
        }

        public PartialViewResult ConsultarEquiposEstabloTable()
        {
            var equipos = db.C_establos_equipos.OrderBy(x => x.nombre_equipo).ToList();
            return PartialView("../CATALOGOS/EQUIPOS_ESTABLOS/_EquiposEstabloTable", equipos);
        }

        public bool ValidarClaveUnicaEquipo(string clave)
        {
            var valid = db.C_establos_equipos.Where(x => x.clave_equipo == clave).FirstOrDefault();
            if (valid == null) { return true; }
            return false;
        }

        public bool RegistrarEquipoEstablo(C_establos_equipos C_establos_equipos)
        {
            try
            {
                C_establos_equipos.activo = true;
                db.C_establos_equipos.Add(C_establos_equipos);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ActualizarEquipoEstablo(int id_equipo, string nombre_equipo, string obs)
        {
            try
            {
                var equipo = db.C_establos_equipos.Find(id_equipo);
                equipo.nombre_equipo = nombre_equipo;
                equipo.observaciones = obs;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public bool OnOffEquipoEstablo(int id_equipo, bool modo)
        {
            try
            {
                var equipos = db.C_establos_equipos.Find(id_equipo);
                equipos.activo = modo;
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