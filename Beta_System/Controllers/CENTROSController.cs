using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class CENTROSController : Controller
    {

        private PERMISOSController permiso = new PERMISOSController();
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public ActionResult Index()
        {
            try
            {
                List<int> permisos = Session["sub_modulos_session"] as List<int>;
                if (!permisos.Contains(6025)) { return View("/Views/Home/Index.cshtml"); }
            }
            catch (Exception)
            {
                RedirectToAction("UsuarioLogin", "USUARIOLOGIN");
            }

            return View("../CATALOGOS/CENTROS/Index");
        }

        public PartialViewResult ConsultarCentrosTable()
        {
            var centros = db.C_centros_g.OrderBy(x => x.nombre_centro).ToList();
            return PartialView("../CATALOGOS/CENTROS/_CentrosTable", centros);
        }

        public bool AgregarNuevoCentro(C_centros_g C_centros_g)
        {
            try
            {
                C_centros_g.activo = true;
                db.C_centros_g.Add(C_centros_g);
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ActualizarCentro(int id_centro, string siglas, string nombre)
        {
            try
            {
                var centro = db.C_centros_g.Find(id_centro);
                centro.siglas = siglas;
                centro.nombre_centro = nombre;
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool OnOffCentro(int id_centro, bool modo)
        {
            try
            {
                var centro = db.C_centros_g.Find(id_centro);
                centro.activo = modo;
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