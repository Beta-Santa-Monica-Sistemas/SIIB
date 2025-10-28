using System.Web;
using System.Web.Optimization;

namespace Beta_System
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();

            //---------------------------NEW FRONT-------------------------
            bundles.Add(new ScriptBundle("~/bundles_MainLayout/js").Include("~/JS/MainLayout.js"));


            //-------------------------------ADMIN PAGE-------------------------------
            bundles.Add(new StyleBundle("~/AdminBundles/CSS_v1").Include(
                "~/Content/production/css/font-awesome.css",
                "~/Content/production/css/font-awesome-animation.min.css",
                "~/Content/production/fonts/css/font-awesome.css",
                "~/Content/css/custom.css",
                "~/Content/css/semantic_v1.min.css",
                "~/Content/production/css/bootstrap-table.css",
                "~/Content/production/css/bootstrap-table.min.css",
                "~/Content/production/css/responsive.bootstrap.min.css",
                //------------< !--jQUERY UI-- >
                "~/Content/documentation/js/jquery-ui-1.13.1/jquery-ui.min.css",
                "~/Content/documentation/js/jquery-ui-1.13.1/jquery-ui.structure.css",
                "~/Content/documentation/js/jquery-ui-1.13.1/jquery-ui.theme.css",

                "~/Content/production/css/datatables/css/jquery.dataTables.css",
                "~/Content/production/css/datatables/css/jquery.dataTables_themeroller",
                "~/Content/production/css/datatables/css/scroller.bootstrap.min.css",
                "~/Content/production/css/datatables/css/buttons.bootstrap.min.css",
                "~/Content/css/bootstrap-multiselect.css"
                ));


            bundles.Add(new ScriptBundle("~/AdminBundles/JS").Include(
                "~/Content/js/utils.js",
                "~/Content/production/js/nprogress.js",
                "~/Content/production/js/pace/pace.min.js",
                "~/Content/production/js/gauge/gauge.min.js",
                "~/Content/production/js/datatables/jquery.dataTables.min.js",
                "~/Content/production/js/datatables/dataTables.bootstrap.js",
                "~/Content/production/js/datatables/dataTables.buttons.min.js",
                "~/Content/production/js/datatables/buttons.bootstrap.min.js",
                "~/Content/production/js/datatables/buttons.html5.min.js",
                "~/Content/production/js/datatables/buttons.print.min.js",
                "~/Content/production/js/datatables/dataTables.fixedHeader.min.js",
                "~/Content/production/js/datatables/dataTables.keyTable.min.js",
                "~/Content/production/js/datatables/dataTables.responsive.min.js",
                "~/Content/production/js/datatables/responsive.bootstrap.min.js",
                "~/Content/production/js/datatables/dataTables.scroller.min.js",
                "~/Content/production/js/progressbar/bootstrap-progressbar.min.js",
                "~/Content/production/js/bootstrap-table.min.js",
                "~/Content/production/js/bootstrap-table-es-MX.min.js",
                "~/Content/production/js/bootstrap-table-contextmenu.js",
                "~/Content/production/js/bootstrap-table.js",
                "~/Content/production/js/nicescroll/jquery.nicescroll.min.js",
                "~/Content/js/bootstrap-multiselect.js"
            ));

            //-------------------PORTAL PUBLICO
            bundles.Add(new ScriptBundle("~/bundles_FormatoOrdenCompra/js").Include("~/JS/Portal/FormatoOrdenCompra.js"));

            //------------- SEGURIDAD
            bundles.Add(new ScriptBundle("~/bundles_Password/js").Include("~/JS/Seguridad/Password.js"));


            //-------------------- MODULO DE ESTABLO
            bundles.Add(new ScriptBundle("~/bundles_Bascula/js").Include("~/JS/Establo/Bascula.js"));




            bundles.Add(new ScriptBundle("~/bundles_SalidasGanado/js").Include("~/JS/Establo/SalidasGanado.js"));
            bundles.Add(new ScriptBundle("~/bundles_BasculaSab/js").Include("~/JS/Establo/BasculaSab.js"));
            bundles.Add(new ScriptBundle("~/bundles_BasculaSistemas/js").Include("~/JS/Establo/BasculaSistemas.js"));
            bundles.Add(new ScriptBundle("~/bundles_Casetas/js").Include("~/JS/Establo/Casetas.js"));

            //-------------------MODULOS DE ENVIOS DE LECHE
            bundles.Add(new ScriptBundle("~/bundles_EnviosLeche/js").Include("~/JS/Establo/EnviosLeche.js"));

            //--------------------ALIMENTACIÓN
            bundles.Add(new ScriptBundle("~/bundles_AlimentacionRecepcion/js").Include("~/JS/Alimentacion/Recepcion.js"));
            bundles.Add(new ScriptBundle("~/bundles_AlimentacionProgramacion/js").Include("~/JS/Alimentacion/Programacion.js"));
            bundles.Add(new ScriptBundle("~/bundles_AlimentacionDietas/js").Include("~/JS/Alimentacion/Dietas.js"));
            bundles.Add(new ScriptBundle("~/bundles_AlimentacionTracker/js").Include("~/JS/Alimentacion/Tracker.js"));

            

            //-------------------- MODULO DE CONFIGURACION
            bundles.Add(new ScriptBundle("~/bundles_Modulo/js").Include("~/JS/Configuracion/Modulo.js"));
            bundles.Add(new ScriptBundle("~/bundles_Permiso/js").Include("~/JS/Configuracion/Permiso.js"));

            bundles.Add(new ScriptBundle("~/bundles_Empleado/js").Include("~/JS/Configuracion/Empleado.js"));
            bundles.Add(new ScriptBundle("~/bundles_Usuario/js").Include("~/JS/Configuracion/Usuario.js"));
            bundles.Add(new ScriptBundle("~/bundles_Rol/js").Include("~/JS/Configuracion/Rol.js"));
            bundles.Add(new ScriptBundle("~/bundles_Establo/js").Include("~/JS/Configuracion/Establo.js"));
            bundles.Add(new ScriptBundle("~/bundles_Almacen/js").Include("~/JS/Configuracion/Almacen.js"));
            bundles.Add(new ScriptBundle("~/bundles_Centro/js").Include("~/JS/Configuracion/Centro.js"));

            bundles.Add(new ScriptBundle("~/bundles_CargoContable/js").Include("~/JS/Configuracion/CargoContable.js"));
            bundles.Add(new ScriptBundle("~/bundles_CuentaContable/js").Include("~/JS/Configuracion/CuentaContable.js"));

            bundles.Add(new ScriptBundle("~/bundles_Firmas/js").Include("~/JS/Configuracion/Firmas.js"));
            bundles.Add(new ScriptBundle("~/bundles_Area/js").Include("~/JS/Configuracion/Area.js"));
            
            //------------------- MODULO ALMACEN
            bundles.Add(new ScriptBundle("~/bundles_SolicitudesMercancia/js").Include("~/JS/Almacen/SolicitudesMercancia.js"));
            bundles.Add(new ScriptBundle("~/bundles_EntregaMercancia/js").Include("~/JS/Almacen/EntregaMercancia.js"));
            bundles.Add(new ScriptBundle("~/bundles_Inventario/js").Include("~/JS/Almacen/Inventario.js"));

            //--------------------- CATALOGOS
            bundles.Add(new ScriptBundle("~/bundles_Articulos/js").Include("~/JS/Catalogos/Articulos.js"));
            bundles.Add(new ScriptBundle("~/bundles_Centros/js").Include("~/JS/Catalogos/Centros.js"));
            bundles.Add(new ScriptBundle("~/bundles_Equipos_Establos/js").Include("~/JS/Catalogos/Equipos_Establos.js"));

            bundles.Add(new ScriptBundle("~/bundles_Catalogos_Bascula/js").Include("~/JS/Configuracion/CatalogosBascula.js"));
            bundles.Add(new ScriptBundle("~/bundles_Catalogos_EnviosLeche/js").Include("~/JS/Configuracion/CatalogosEnviosLeche.js"));
            bundles.Add(new ScriptBundle("~/bundles_Catalogos_Forrajes/js").Include("~/JS/Configuracion/CatalogosForrajes.js"));

            //-------------------MODULO DE UTILERIAS - DISPOSITIVOS
            bundles.Add(new ScriptBundle("~/bundles_UtileriasDispositivos/js").Include("~/JS/UTILERIAS/Dispositivos/UtileriasDispositivos.js"));
            //-------------------MODULO DE UTILERIAS - SOPORTE
            bundles.Add(new ScriptBundle("~/bundles_UtileriaSoporte/js").Include("~/JS/UTILERIAS/Soporte/UtileriaSoporte.js"));

            //------------------- MODULO COMPRAS
            bundles.Add(new ScriptBundle("~/bundles_Presupuestos/js").Include("~/JS/Compras/Presupuestos.js"));
            bundles.Add(new ScriptBundle("~/bundles_Requisiciones/js").Include("~/JS/Compras/Requisiciones_v1.js"));
            bundles.Add(new ScriptBundle("~/bundles_Proveedores/js").Include("~/JS/Compras/Proveedores.js"));
            bundles.Add(new ScriptBundle("~/bundles_OrdenesCompra/js").Include("~/JS/Compras/OrdenesCompra.js"));

            //------------------- MODULO PORTAL DE PROVEEDORES
            bundles.Add(new ScriptBundle("~/bundles_PortalProveedores/js").Include("~/JS/PortalProveedores/Index.js"));

            //------------------- MODULO CONTABILIDAD
            bundles.Add(new ScriptBundle("~/bundles_ImportacionFacturas/js").Include("~/JS/Contabilidad/ImportacionFacturas.js"));
            bundles.Add(new ScriptBundle("~/bundles_SolicitudesPagos/js").Include("~/JS/Contabilidad/SolicitudesPagos.js"));
            bundles.Add(new ScriptBundle("~/bundles_OrdenesPagos/js").Include("~/JS/Contabilidad/OrdenesPagos.js"));

            //-------------------MODULO PARAMETROS
            bundles.Add(new ScriptBundle("~/bundles_Propiedades/js").Include("~/JS/Parametros/Propiedades.js"));



            //-------------------MODULOS DE REPORTES ALMACEN
            bundles.Add(new ScriptBundle("~/bundles_ReportesAlmacen/js").Include("~/JS/REPORTES/ReportesAlmacen.js"));

            //-------------------MODULOS DE REPORTES COMPRAS
            bundles.Add(new ScriptBundle("~/bundles_ReportesCompras/js").Include("~/JS/REPORTES/ReportesCompras.js"));

            //-------------------MODULOS DE REPORTES ALIMENTACION
            bundles.Add(new ScriptBundle("~/bundles_ReportesAlimentacion/js").Include("~/JS/REPORTES/ReportesAlimentacion.js"));

            //-------------------MODULOS DE REPORTES ENVIOS DE LECHE
            bundles.Add(new ScriptBundle("~/bundles_ReportesEnviosLeche/js").Include("~/JS/REPORTES/ReportesEnviosLeche.js"));

            //-------------------MODULOS DE REPORTES NOMINA
            bundles.Add(new ScriptBundle("~/bundles_ReportesNominas/js").Include("~/JS/REPORTES/ReportesNominas.js"));




            //-------------------MODULO DE UTILERIAS
            bundles.Add(new ScriptBundle("~/bundles_UtileriaSalidaGanado/js").Include("~/JS/UTILERIAS/SalidasGanado/UtileriaSalidasGanado.js"));
            bundles.Add(new ScriptBundle("~/bundles_UtileriaRequisicionesV1.1/js").Include("~/JS/UTILERIAS/Requisiciones/UtileriaRequisiciones.js"));
            bundles.Add(new ScriptBundle("~/bundles_UtileriaInventario/js").Include("~/JS/UTILERIAS/Almacen/UtileriaInventario.js"));
            bundles.Add(new ScriptBundle("~/bundles_UtileriaBascula/js").Include("~/JS/UTILERIAS/Bascula/UtileriaBascula.js"));

            //-------------------MODULO DE UTILERIAS - PRUEBA
            bundles.Add(new ScriptBundle("~/bundles_Prueba/js").Include("~/JS/UTILERIAS/Pruebas/Prueba.js"));


            //------------------- MODULO NOMINA
            bundles.Add(new ScriptBundle("~/bundles_Nomina/js").Include("~/JS/Nomina/Nomina.js"));
            //------------------- MODULO NOMINA - FORMACION ACADEMICA
            bundles.Add(new ScriptBundle("~/bundles_FormacionAcademica/js").Include("~/JS/Nomina/FormacionAcademica.js"));


            //------------------- MODULO PRE-NOMINA
            bundles.Add(new ScriptBundle("~/bundles_Prenomina/js").Include("~/JS/Nomina/Prenomina.js"));
            
            
            //------------------- MODULO EMPLEADO CHECADOR
            bundles.Add(new ScriptBundle("~/bundles_EmpleadoChecador/js").Include("~/JS/Nomina/EmpleadoChecador.js"));

            //------------------- MODULO EVALUACIONES SEMESTRALES
            bundles.Add(new ScriptBundle("~/bundles_Evaluaciones/js").Include("~/JS/Evaluaciones/Evaluaciones.js"));


            //------------------- DASHBOARD
            bundles.Add(new ScriptBundle("~/bundles_Dashboard/js").Include("~/JS/Reportes/Dashboard.js"));


            //------------------- MODULO VINCULACION SAB-SIIB
            bundles.Add(new ScriptBundle("~/bundles_VinculoSab/js").Include("~/JS/Utilerias/VinculoSab.js"));


            //------------------- MODULO ADMINISTRACIÓN SIIB
            bundles.Add(new ScriptBundle("~/bundles_admin_siib/js").Include("~/JS/AdminSiib/Admin_siib.js"));



            //-----OPTIMIZACIÓN DE BUNDLES
            //BundleTable.EnableOptimizations = true;
        }
    }
}
