using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Beta_System.Controllers
{
    public class CLASIFICACIONESController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities(); //Conexion DB

        //Metodo
        public ActionResult Index()
        {
            return View("../CATALOGOS/ARTICULOS_CLASIFICACIONES/Index");
        }


        public PartialViewResult MostrarClasificaciones()
        {
            //Mostrar
            var Clasificaciones = db.C_articulos_clasificaciones.ToList();
            return PartialView("../CATALOGOS/ARTICULOS_CLASIFICACIONES/_ClasificacionesTable", Clasificaciones);
        }

        public bool GuardarClasificaciones(string ParametroBK)
        {
            try
            {
                //Intanciar Obj.
                C_articulos_clasificaciones newClasificacion = new C_articulos_clasificaciones();

                //Setear Propiedades
                newClasificacion.nombre_clasificacion = ParametroBK;
                newClasificacion.activo = true;

                //Guardar Obj.
                db.C_articulos_clasificaciones.Add(newClasificacion);
                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public string MostrarInformacionClasificacion(int ParametroItemIdBK)
        {
            try
            {
                var clasificacionModificar = db.C_articulos_clasificaciones.Find(ParametroItemIdBK);
                return clasificacionModificar.nombre_clasificacion;
            }
            catch (Exception e)
            {
                return e.ToString();

            }
        }

        public bool ActualizarClasificacion(string ParametroNomBK, int ParametroItemIdBK)
        {
            try
            {

                C_articulos_clasificaciones actualizacionClasificaciones = db.C_articulos_clasificaciones.Find(ParametroItemIdBK);


                if (actualizacionClasificaciones.nombre_clasificacion != ParametroNomBK)
                {
                    //Setear Propiedades
                    actualizacionClasificaciones.nombre_clasificacion = ParametroNomBK;
                    db.SaveChanges();

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public bool OnOffActivo(bool ModoBK, int ParametroIdOnOffBK)
        {
            try
            {
                var OnOffClasificacion = db.C_articulos_clasificaciones.Find(ParametroIdOnOffBK);

                OnOffClasificacion.activo = ModoBK;

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
