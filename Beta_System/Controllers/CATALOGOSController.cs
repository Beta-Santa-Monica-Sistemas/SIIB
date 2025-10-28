using Beta_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Beta_System.Models.ConexionSab;

namespace Beta_System.Controllers
{
    public class CATALOGOSController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();
        private TRACKER_LOGEntities db_tracker = new TRACKER_LOGEntities();

        #region MEDICOS
        public PartialViewResult ConsultarMedicosSelect()
        {            
            var medicos = db.C_salida_ganado_medicos.Where(x=>x.activo == true).ToList();
            return PartialView("../ESTABLOS/SalidasGanado/_MedicosSelect", medicos);
        }
        #endregion

        #region FORMACION ACADEMICA
        public PartialViewResult ConsultarEscolaridadSelect()
        {
            var escolaridad = db.C_rh_formacion_escolaridades.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/FORMACION_ACADEMICA/_EscolaridadSelect", escolaridad);
        }

        public PartialViewResult ConsultarCarreraSelect()
        {
            var escolaridad = db.C_rh_formacion_estudio.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/FORMACION_ACADEMICA/_CarreraSelect", escolaridad);
        }

        public PartialViewResult ConsultarInstitucionSelect()
        {
            var escolaridad = db.C_rh_formacion_universidades.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/FORMACION_ACADEMICA/_InstitucionSelect", escolaridad);
        }

        public PartialViewResult ConsultarEscolaridadStatusSelect()
        {
            var escolaridad = db.C_rh_formacion_status.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/FORMACION_ACADEMICA/_EscolaridadStatusSelect", escolaridad);
        }

        #endregion

        #region UNDEFINED
        public string AlmacenUbicacionArticuloSelectCargado(int id_almacen)
        {
            var ubicacion = from ubi in db.C_almacen_ubicaciones_articulos
                            where ubi.id_almacen == id_almacen && ubi.activo == true
                            select new
                            {
                                ubi.id_ubicacion_almacen,
                                ubi.nombre_ubicacion
                            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(ubicacion.OrderBy(x => x.nombre_ubicacion));
        }
        #endregion

        #region ESTABLOS
        public PartialViewResult ConsultarEstablosUsuarioSelect(int id_usuario)
        {
            if (id_usuario == 0) { id_usuario = (int)Session["LoggedId"]; }
            var establos = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true).ToList().Select(x => x.C_establos);
            return PartialView("../ESTABLOS/SalidasGanado/_EstablosSelect", establos);
        }
        #endregion

        #region BASCULA
        public PartialViewResult ConsultarPozosSelect(int id_establo)
        {
            var pozos = db.C_bascula_no_pozos.Where(x => x.activo == true && x.id_establo == id_establo).OrderBy(x => x.no_pozo).ToList();
            return PartialView("../CATALOGOS/BASCULA/_NumeroPozos", pozos);
        }

        public PartialViewResult ConsultarTiposMovimientoSelect()
        {
            var proveedores = db.C_bascula_tipos_movimientos.Where(x => x.activo == true).OrderBy(x => x.nombre_movimiento).ToList();
            return PartialView("../CATALOGOS/BASCULA/_TiposMovimientosSelect", proveedores);
        }


        public PartialViewResult ConsultarClavesMovimientosSelect()
        {
            var codigos_movimiento = db.C_bascula_codigos_movimientos.Where(x => x.activo == true).OrderBy(x => x.codigo_mov).ToList();
            return PartialView("../CATALOGOS/BASCULA/_CveMovimientosSelect", codigos_movimiento);
        }

        public PartialViewResult ConsultarClavesMovimientosEstabloSelect(int id_ficha_tipo)
        {
            var codigos_movimiento = db.C_bascula_establos_codigos_movimientos.Where(x => x.activo == true && x.id_ficha_tipo == id_ficha_tipo).OrderBy(x => x.C_bascula_codigos_movimientos.codigo_mov).ToList();
            return PartialView("../CATALOGOS/BASCULA/_CveMovimientosEstabloSelect", codigos_movimiento);
        }

        public PartialViewResult ConsultarLineasTrasnportistasSelect()
        {
            var linea_transportista = db.C_bascula_lineas_transportistas.Where(x => x.activo == true).OrderBy(x => x.nombre_linea).ToList();
            return PartialView("../CATALOGOS/BASCULA/_LineasTransportistasSelect", linea_transportista);
        }

        public PartialViewResult ConsultarTipoFichasSelect()
        {
            var tipo_ficha = db.C_bascula_fichas_tipos.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/BASCULA/_ConsultarTipoFichasSelect", tipo_ficha);
        }


        public PartialViewResult ConsultarTipoFichasEstabloSelect(int id_establo)
        {
            var tipo_ficha = db.C_bascula_establos_fichas_tipos.Where(x => x.activo == true && x.id_establo == id_establo).ToList();
            return PartialView("../CATALOGOS/BASCULA/_ConsultarTipoFichasEstabloSelect", tipo_ficha);
        }

        public PartialViewResult ConsultarProveedoresTipoBasculaSelect(int tipo_ficha)
        {
            //ENVIO LECHE Y REGULAR CLIENTE
            if (tipo_ficha == 1 || tipo_ficha == 4)
            {
                var proveedor = db.C_envios_leche_clientes.Where(x => x.activo == true).ToList();
                return PartialView("../CATALOGOS/LECHE/_ProveedorLecheSelect", proveedor);
            }
            //FORRAJE Y REGULAR PROVEEDOR
            else
            {
                var proveedor = db.C_compras_proveedores.Where(x => x.activo == true && x.disponible_bascula == true).ToList();
                return PartialView("../CATALOGOS/BASCULA/_ProveedorRegularSelect", proveedor);
            }
        }

        public PartialViewResult ConsultarArticulosBasculaTiposSelect(int tipo_ficha)
        {
            if(tipo_ficha == 4) { tipo_ficha = 3; }

            var art_ings = db.C_articulos_catalogo_bascula.Where(x => x.activo == true && x.C_articulos_catalogo.activo == true && x.id_ficha_tipo == tipo_ficha).Select(x => x.C_articulos_catalogo).Distinct().OrderBy(x => x.nombre_articulo).ToList();
            return PartialView("../CATALOGOS/BASCULA/_TipoArticuloBasculaSelect", art_ings);
        }

        #endregion


        #region ENVIO LECHE
        public PartialViewResult ConsultarTiposTanquesSelect()
        {
            var tipos = db.C_envios_leche_tipos_tanques.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/LECHE/_TiposTanqueSelect", tipos);
        }

        public PartialViewResult ConsultarEstatusLoteSelect()
        {
            var estatus_lote = db.C_envios_leche_lote_status.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/LECHE/_EstatusLoteSelect", estatus_lote);
        }


        public PartialViewResult ConsultarEstablosUsuarioLecheSelect()
        {
            int id_usuario = (int)Session["LoggedId"];
            var tipo_ficha = db.C_bascula_establos_fichas_tipos.Where(x => x.activo == true && x.id_ficha_tipo == 1).Select(x => x.id_establo).ToArray();
            var establos = db.C_usuarios_establos.Where(x => x.id_usuario == id_usuario && x.activo == true && tipo_ficha.Contains(x.id_establo)).ToList();
            return PartialView("../CATALOGOS/LECHE/_EstablosUsuarioLecheSelect", establos);
        }

        public PartialViewResult ConsultarSilosLecheEstabloSelect(int[] id_establo)
        {
            if (id_establo.Contains(0))
            {
                id_establo = db.C_establos.Where(x => x.activo == true).Select(x => x.id_establo).ToArray();
                var silos = db.C_envios_leche_silos_establo.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo)).ToList().OrderBy(x => x.C_envios_leche_silos.silo).Select(x => x.C_envios_leche_silos).Distinct();
                return PartialView("../CATALOGOS/LECHE/_SilosEstabloSelect", silos);
            }
            else
            {
                var silos = db.C_envios_leche_silos_establo.Where(x => x.activo == true && id_establo.Contains((int)x.id_establo)).ToList().OrderBy(x => x.C_envios_leche_silos.silo).Select(x => x.C_envios_leche_silos).Distinct();
                return PartialView("../CATALOGOS/LECHE/_SilosEstabloSelect", silos);
            }
        }
        #endregion


        #region CENTRO
        public PartialViewResult CentrosUsuariosSelect(int id_usuario)
        {
            var centros = from cent_us in db.C_usuarios_centros
                          join usu in db.C_usuarios_corporativo on cent_us.id_usuario equals usu.id_usuario_corporativo
                          where cent_us.activo == true && cent_us.id_usuario == id_usuario && cent_us.C_centros_g.activo == true orderby cent_us.C_centros_g.siglas 
                          select cent_us.C_centros_g;
            return PartialView("CENTROS/_CentrosSelect", centros);
        }

        public PartialViewResult ConsultarCentrosSelect()
        {
            var centros = db.C_centros_g.Where(x => x.activo == true).OrderBy(x => x.siglas).ToList();
            return PartialView("CENTROS/_CentrosSelect", centros);
        }
        #endregion

        #region CARGOS CONTABLES / CUENTAS CONTABLES
        public PartialViewResult CargosContablesSelect()
        {
            var cargoscontables = db.C_cargos_contables_g.Where(x => x.activo == true).OrderBy(x => x.nombre_cargo);
            return PartialView("CARGOS_CONTABLES/_CargosContablesSelect", cargoscontables);
        }

        public PartialViewResult CargoContableCuentaUsuariosSelect(int id_usuario)
        {
            if (id_usuario == 0) { id_usuario = (int)Session["LoggedId"]; }

            //var cargocuenta = from cuenta_usu in db.C_usuarios_cuentas_contables
            //                  join usu in db.C_usuarios_corporativo on cuenta_usu.id_usuario equals usu.id_usuario_corporativo
            //                  join cuen in db.C_cuentas_contables_g on cuenta_usu.id_cuenta_contable_g equals cuen.id_cuenta_contable
            //                  join carg in db.C_cargos_contables_g on cuen.id_cargo_contable equals carg.id_cargo_contable_g
            //                  where cuenta_usu.activo == true && cuenta_usu.id_usuario == id_usuario && cuenta_usu.C_cuentas_contables_g.C_cargos_contables_g.activo == true && cuenta_usu.C_cuentas_contables_g.activo == true
            //                  orderby cuenta_usu.C_cuentas_contables_g.C_cargos_contables_g.nombre_cargo
            //                  select cuenta_usu.C_cuentas_contables_g.C_cargos_contables_g;

            var cargos_usuario = db.C_usuarios_cuentas_contables.Where(x => x.id_usuario == id_usuario && x.activo == true 
                                && x.C_cuentas_contables_g.C_cargos_contables_g.activo == true).Select(x => x.C_cuentas_contables_g.C_cargos_contables_g).Distinct().OrderBy(x => x.nombre_cargo);
            return PartialView("CARGOS_CONTABLES/_CargosContablesSelect", cargos_usuario);
        }

        public PartialViewResult ConsultarCargosContables()
        {
            var cargos = db.C_cargos_contables_g.Where(x => x.activo == true).OrderBy(x => x.nombre_cargo).ToList();
            return PartialView("CARGOS_CONTABLES/_CargosContablesSelect", cargos);
        }



        public class ConsultaCargosCuentasResult
        {
            public List<C_cargos_contables_g> Cargos { get; set; }
            public List<C_cuentas_contables_g> Cuentas { get; set; }
        }

        public JsonResult ConsultarCargosCuentas()
        {
            var cargos = db.C_cargos_contables_g
                .Where(x => x.activo == true)
                .Select(x => new { x.id_cargo_contable_g, x.nombre_cargo })
                .OrderBy(x => x.nombre_cargo)
                .ToArray();

            var cuentas = db.C_cuentas_contables_g
                .Where(x => x.activo == true)
                .Select(x => new { x.id_cargo_contable, x.id_cuenta_contable, x.nombre_cuenta })
                .OrderBy(x => x.nombre_cuenta)
                .ToArray();

            return Json(new { cargos, cuentas }, JsonRequestBehavior.AllowGet);
        }
        public string ConsultarCuentasContableSelects(int id_cuentas_contable)
        {
            var cuenta = from cuen in db.C_cuentas_contables_g
                         where cuen.id_cuenta_contable == id_cuentas_contable
                         select new
                         {
                             cuen.cuenta
                         };

            return Newtonsoft.Json.JsonConvert.SerializeObject(cuenta);
        }

        public PartialViewResult ConsultarCuentasCargosUsuarioSelect(int id_cargo_contable)
        {


            int id_usuario = (int)Session["LoggedId"];
            var cuentas = db.C_usuarios_cuentas_contables.Where(x => x.activo == true && x.C_cuentas_contables_g.id_cargo_contable == id_cargo_contable && x.id_usuario == id_usuario).OrderBy(x => x.C_cuentas_contables_g.nombre_cuenta).ToList();
            return PartialView("CUENTAS_CONTABLES/_CuentasCargosUsuarioSelect", cuentas);
        }

        public PartialViewResult ConsultarCuentasContablesCargosSelect(int id_cargo_contable)
        {
            var cuentas = db.C_cuentas_contables_g.Where(x => x.activo == true && x.id_cargo_contable == id_cargo_contable).OrderBy(x => x.nombre_cuenta).ToList();
            return PartialView("CUENTAS_CONTABLES/_CuentasContablesSelect", cuentas);
        }

        #endregion

        #region ALMACENES
        public PartialViewResult ConsultarAlmacenesUsuarioSelect(int id_usuario)
        {
            if (id_usuario == 0) { id_usuario = (int)Session["LoggedId"]; }
            var almacenes = from alm in db.C_almacen_almacenes_g
                            join us in db.C_almacen_almacenes_usuarios on alm.id_almacen_g equals us.id_almacen_g
                            where alm.activo == true && us.activo == true && us.id_usuario == id_usuario
                            orderby alm.nombre_almacen
                            select alm;
            return PartialView("ALMACENES/_AlmacenesSelect", almacenes);
        }

        public PartialViewResult ConsultarAlmacenesSelect()
        {
            var almacenes = db.C_almacen_almacenes_g.Where(x => x.activo == true).OrderBy(x => x.nombre_almacen).ToList();
            return PartialView("ALMACENES/_AlmacenesSelect", almacenes);
        }

        public PartialViewResult ConsultarTiposConsumoCriticidadSelect()
        {
            var data = db.C_almacen_criticidad_tipos_consumos.Where(x => x.activo == true).OrderBy(x => x.siglas).ToList();
            return PartialView("ALMACENES/_TiposConsumoCriticidadSelect", data);
        }
        public PartialViewResult ConsultarTiposOperacionCriticidadSelect()
        {
            var data = db.C_almacen_criticidad_tipos_operacion.Where(x => x.activo == true).OrderBy(x => x.siglas).ToList();
            return PartialView("ALMACENES/_TiposOperacionCriticidadSelect", data);
        }



        #endregion

        #region SOLICITUDES DE MERCANCIA CATALOGOS
        public PartialViewResult ConsultarDepartamentosSelect()
        {
            var departamentos = db.C_departamentos_g.Where(x => x.activo == true).OrderBy(x => x.nombre_departamento).ToList();
            return PartialView("DEPARTAMENTOS/_DepartamentosSelect", departamentos);
        }

        public PartialViewResult ConsultarEquiposEstablosSelect()
        {
            var cuentas = db.C_establos_equipos.Where(x => x.activo == true).OrderBy(x => x.nombre_equipo).ToList();
            return PartialView("EQUIPOS_ESTABLOS/_EquiposEstabloSelect", cuentas);
        }

        public PartialViewResult ConsultarEstatusSolicitudesMercanciaSelect()
        {
            var status = db.C_almacen_solicitudes_status.Where(x => x.activo == true).OrderBy(x => x.nombre_status).ToList();
            return PartialView("ESTATUS/_EstatusSolicitudesMercanciaSelect", status);
        }
        #endregion

        #region REQUISICIONES CATALOGOS
        public PartialViewResult ConsultarPrioriedadSelect()
        {
            var prioriedad = db.C_prioridades_g.Where(x => x.activo == true).OrderBy(x => x.nombre_prioridad).ToList();
            return PartialView("PRIORIDADES/_PrioriedadSelect", prioriedad);
        }

        public PartialViewResult ConsultarTiposRequisiciones()
        {
            var tipos = db.C_compras_requisiciones_tipos.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/REQUISICIONES/_TiposRequisicionesSelect", tipos.OrderBy(x => x.tipo_requisicion));
        }

        public PartialViewResult ConsultarStatusRequisicionesSelect()
        {
            var status = db.C_compras_requisiciones_status.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/REQUISICIONES/_StatusRequisicionesSelect", status/*.OrderBy(x => x.nombre_status)*/);
        }

        public PartialViewResult ConsultarUbicacionesEntrega()
        {
            var ubicaciones = db.C_compras_ordenes_ubicaciones_entrega.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/REQUISICIONES/_UbicacionesEntregaSelect", ubicaciones.OrderBy(x => x.nombre_ubicacion));
        }
        public PartialViewResult AlmacenUbicacionArticuloSelect(int id_almacen)
        {
            var ubicacion = db.C_almacen_ubicaciones_articulos.Where(x => x.activo == true && x.id_almacen == id_almacen).ToList().OrderBy(x => x.nombre_ubicacion);
            return PartialView("../CATALOGOS/ALMACENES/_AlmacenUbicacionArticuloSelect", ubicacion);
        }
        public PartialViewResult ConsultarTipoMoneda()
        {
            var cargos = db.C_tipos_moneda.ToList().OrderBy(x => x.nombre);
            return PartialView("../COMPRAS/PROVEEDORES/_TiposMonedaSelect", cargos);
        }
        public PartialViewResult ConsultarTipoArticuloRequi()
        {
            var rubro = db.C_articulos_tipos_requisicion.ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_TipoArticuloRequi", rubro.OrderBy(x => x.tipo_requisicion));
        }

        public string ConsultarTipoCambio(int id_moneda)
        {
            if (id_moneda == 1) { return "1"; }
            else { return db.C_parametros_configuracion.Find(1015).valor_numerico.ToString(); }
        }
        #endregion

        #region INVENTARIO CATALOGO
        public PartialViewResult ConsultarTipoMovInventarioSelect()
        {
            var tipos_mov = db.C_inventario_mov_tipos.Where(x => x.activo == true).ToList();
            return PartialView("INVENTARIO/_TiposMovimientosSelect", tipos_mov);
        }
        #endregion

        #region PROVEEDORES
        public PartialViewResult ConsultarProveedoresSelect()
        {
            var proveedores = db.C_compras_proveedores.Where(x => x.activo == true && x.id_proveedor_status == 1).ToList();
            return PartialView("../CATALOGOS/PROVEEDORES/_ProveedoresSelect", proveedores.OrderBy(x => x.razon_social));
        }
        #endregion

        #region ARTICULOS CATALOGOS
        public IEnumerable<C_articulos_catalogo> ConsultarArticulosAlmacen(int id_almacen, int id_tipo)
        {
            if (id_tipo == 0){
                return db.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen && x.activo == true
                                        && x.C_articulos_catalogo.activo == true && x.C_articulos_catalogo.almacenable == true).Select(x => x.C_articulos_catalogo); }

            return db.C_inventario_almacenes_articulos.Where(x => x.id_almacen == id_almacen && x.activo == true
                                        && x.C_articulos_catalogo.activo == true && x.C_articulos_catalogo.almacenable == true
                                        && x.C_articulos_catalogo.id_articulo_tipo == id_tipo
                                        ).Select(x => x.C_articulos_catalogo);
        }
        
        public PartialViewResult ConsultarTiposArticulosSelect()
        {
            var tipos_art = db.C_articulos_tipos.Where(x => x.activo == true).OrderBy(x => x.nombre_tipo).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_TiposArticulosSelect", tipos_art);
        }
        public PartialViewResult MostrarClasificacionSelect()
        {
            var tipos_art = db.C_articulos_clasificaciones.Where(x => x.activo == true).OrderBy(x => x.nombre_clasificacion).ToList();
            return PartialView("../CATALOGOS/ARTICULOS/_ClasificacionSelect", tipos_art);
        }
        public PartialViewResult MostrarMarcasSelect()
        {
            var marcas = db.C_articulos_marcas.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/ARTICULOS_MARCAS/_MarcasSelect", marcas);
        }
        #endregion


        #region CRUD MARCAS ARTICULOS

        public PartialViewResult MostrarTablaMarcas()
        {
            var marcas = db.C_articulos_marcas.ToList();
            return PartialView("../CATALOGOS/ARTICULOS_MARCAS/_MarcasTable", marcas);
        }
        
        public bool RegistrarMarca(string marca, bool estatus_marca)
        {
            try
            {
                C_articulos_marcas marcas_db = new C_articulos_marcas();
                marcas_db.marca = marca;
                marcas_db.activo = estatus_marca;
                db.C_articulos_marcas.Add(marcas_db);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string MostrarInformacionMarca(int id_marca)
        {
            var marca = from marc in db.C_articulos_marcas
                           where marc.id_marca == id_marca
                        select new
                           {
                            marc.id_marca,
                            marc.marca,
                            marc.activo
                           };

            return Newtonsoft.Json.JsonConvert.SerializeObject(marca);
        }

        public bool ModificarMarca(int id_marca, string marca, bool estatus_marca)
        {
            try
            {
                var marcas_db = db.C_articulos_marcas.Find(id_marca);
                marcas_db.marca = marca;
                marcas_db.activo = estatus_marca;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }
        #endregion

        #region CRUD CLASIFICACION ARTICULOS
        public PartialViewResult MostrarTablaClasificacion()
        {
            var Clasificaciones = db.C_articulos_clasificaciones.ToList().OrderBy(x=>x.nombre_clasificacion) ;
            return PartialView("../CATALOGOS/ARTICULOS_CLASIFICACIONES/_ClasificacionesTable", Clasificaciones);
        }
        public bool RegistrarClasificacion(string clasificacion, bool estatus_clasificacion)
        {
            try
            {
                C_articulos_clasificaciones clasificaciones_db = new C_articulos_clasificaciones();
                clasificaciones_db.nombre_clasificacion = clasificacion;
                clasificaciones_db.activo = estatus_clasificacion;
                db.C_articulos_clasificaciones.Add(clasificaciones_db);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }

        public string MostrarInformacionClasificacion(int id_clasificacion)
        {
            var clasif = from clasi in db.C_articulos_clasificaciones
                        where clasi.id_articulo_clasificacion == id_clasificacion
                         select new
                        {
                            clasi.id_articulo_clasificacion,
                            clasi.nombre_clasificacion,
                            clasi.activo
                        };

            return Newtonsoft.Json.JsonConvert.SerializeObject(clasif);
        }

        public bool ModificarClasificacion(int id_clasificacion, string clasificacion, bool estatus_clasificacion)
        {
            try
            {
                var clasificacion_db = db.C_articulos_clasificaciones.Find(id_clasificacion);
                clasificacion_db.nombre_clasificacion = clasificacion;
                clasificacion_db.activo = estatus_clasificacion;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                string msj = ex.ToString();
                return false;
            }
        }
        #endregion

        #region ENVIOS DE LECHE CATALOGOS
        public PartialViewResult ConsultarClienteEnvioLecheSelect()
        {
            var cliente = db.C_envios_leche_clientes.Where(x => x.activo == true).OrderBy(x => x.nombre_comercial).ToList();
            return PartialView("../CATALOGOS/LECHE/_ClienteSelect", cliente);
        }


        public PartialViewResult ConsultarTiposTanquesRadio()
        {
            var tipos = db.C_envios_leche_tipos_tanques.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/LECHE/_TiposTanqueRadio", tipos);
        }
        public PartialViewResult ConsultarDestinoEnvLecheSelect()
        {
            var destino = db.C_envios_leche_destinos.Where(x => x.activo == true).OrderBy(x => x.nombre_destino).ToList();
            return PartialView("../CATALOGOS/LECHE/_DestinoEnvLecheSelect", destino);
        }
        public PartialViewResult ConsultarTipoEnvioLecheSelect()
        {
            var envio = db.C_envios_leche_tipos_envio.Where(x => x.activo == true).OrderBy(x => x.tipo_envio).ToList();
            return PartialView("../CATALOGOS/LECHE/_TipoEnvioLecheSelect", envio);
        }
        public PartialViewResult ConsultarTiposTanquesRadiorEscala()
        {
            var tipos = db.C_envios_leche_tipos_tanques.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/LECHE/_TiposTanqueRadioEscala", tipos);
        }

        public PartialViewResult ConsultarClienteDestinoLecheSelect(int modo)
        {
            ViewBag.ModoClienteDestino = modo;
            var destinos = db.C_envios_leche_cliente_destinos.Where(x => x.activo == true).OrderBy(x => x.C_envios_leche_clientes.nombre_comercial).ToList();
            return PartialView("../CATALOGOS/LECHE/_ClienteDestinoLecheSelect", destinos);
        }

        public PartialViewResult ConsultarDestinosClienteLecheSelect(int id_cliente)
        {
            List<C_envios_leche_destinos> destinos = null;
            if (id_cliente == 0) {
                destinos = db.C_envios_leche_destinos.Where(x => x.activo == true).ToList();
            }
            else
            {
                destinos = db.C_envios_leche_cliente_destinos.Where(x => x.activo == true && x.id_envio_leche_cliente_ms == id_cliente).Select(x => x.C_envios_leche_destinos).
                            OrderBy(x => x.nombre_destino).Distinct().ToList();
            }
            return PartialView("../CATALOGOS/LECHE/_DestinosClienteLecheSelect", destinos);
        }

        #endregion

        #region NOMINA CATALOGOS
        public PartialViewResult ConsultarPuestoNominaTable()
        {
            var puesto = db.C_nomina_puestos.OrderBy(x=>x.nombre_puesto).ToList();
            return PartialView("../CATALOGOS/NOMINA/_PuestosTable", puesto);
        }

        public PartialViewResult ConsultarDepartamentoNominaTable()
        {
            var departamento = db.C_nomina_departamentos.OrderBy(x=>x.nombre_departamento).ToList();
            return PartialView("../CATALOGOS/NOMINA/_DepartamentoTable", departamento);
        }

        public PartialViewResult ConsultarEmpleadosSelect(string status)
        {
            List<C_nomina_empleados> empleados = new List<C_nomina_empleados>();
            if (status == "0")
            {
                empleados = db.C_nomina_empleados.Where(x => x.Frepag_id == 410).OrderBy(x => x.Nombre_completo).ToList();
            }
            else
            {
                empleados = db.C_nomina_empleados.Where(x => x.Estatus == status && x.Frepag_id == 410).OrderBy(x => x.Nombre_completo).ToList();
            }
            return PartialView("../CATALOGOS/NOMINA/_EmpleadosSelect", empleados);
        }
        public PartialViewResult ConsultarEmpleadosDepartamentoSelect(string status, int id_departamento)
        {
            List<C_nomina_empleados> empleados = new List<C_nomina_empleados>();
            if (status == "0")
            {
                empleados = db.C_nomina_empleados.Where(x => x.Frepag_id == 410 && x.Depto_no_id == id_departamento).OrderBy(x => x.Nombre_completo).ToList();
            }
            else
            {
                empleados = db.C_nomina_empleados.Where(x => x.Estatus == status && x.Frepag_id == 410 && x.Depto_no_id == id_departamento).OrderBy(x => x.Nombre_completo).ToList();
            }
            return PartialView("../CATALOGOS/NOMINA/_EmpleadosSelect", empleados);
        }

        public PartialViewResult ConsultarDepartamentoNominaSelect()
        {
            var departamento = db.C_nomina_departamentos.OrderBy(x=>x.nombre_departamento).ToList();
            return PartialView("../CATALOGOS/NOMINA/_DepartamentoSelect", departamento);
        }
        public PartialViewResult ConsultarDepartamentosUsuarioSelect()
        {
            int id_usuario = (int)Session["LoggedId"];
            var departamentos_usuario = db.C_usuarios_areas_nomina.Where(x => x.id_usuario_corporativo == id_usuario && x.activo == true).Select(x => x.C_nomina_departamentos).Distinct().OrderBy(x => x.nombre_departamento).ToList();
            return PartialView("../CATALOGOS/NOMINA/_DepartamentoSelect", departamentos_usuario);
        }


        public PartialViewResult ConsultarRegimenPatronalNominaTable()
        {
            var regimen = db.C_nomina_regimen_patronal.OrderBy(x=>x.numero_regimen_partonal).ToList();
            return PartialView("../CATALOGOS/NOMINA/_RegimenPatronalTable", regimen);
        }

        public PartialViewResult ConsultarEmpleadoNominaTable(string id_status)
        {
            if(id_status == "T")
            {
                var empleado = db.C_nomina_empleados.Where(x => x.Frepag_id == 410).OrderBy(x=>x.Nombre_completo).ToList();
                return PartialView("../CATALOGOS/NOMINA/_EmpleadosTable", empleado);
            }
            else
            {
                var empleado = db.C_nomina_empleados.Where(x => x.Frepag_id == 410).OrderBy(x=>x.Nombre_completo).Where(x => x.Estatus == id_status).ToList();
                return PartialView("../CATALOGOS/NOMINA/_EmpleadosTable", empleado);
            }
        }
        public PartialViewResult ConsultarConceptosNominaSelect()
        {
            var conceptos = db.C_nomina_conceptos.Where(x => x.activo == true).OrderBy(x => x.nombre_concepto).ToList();
            return PartialView("../CATALOGOS/NOMINA/_ConceptosNominaSelect", conceptos);
        }


        public PartialViewResult ConsultarPuestoNominaSelect()
        {
            var puesto = db.C_nomina_puestos.Where(x => x.activo == true).OrderBy(x=>x.nombre_puesto).ToList();
            return PartialView("../CATALOGOS/NOMINA/_PuestosSelect", puesto);
        }

        public PartialViewResult ConsultarPuestosNominaActivosSelect()
        {
            var puesto = db.C_nomina_empleados.Where(x => x.Estatus == "A").Select(x => x.C_nomina_puestos).Where(z => z.activo == true).Distinct().OrderBy(x => x.nombre_puesto).ToList();
            return PartialView("../CATALOGOS/NOMINA/_PuestosSelect", puesto);
        }

        public PartialViewResult ConsultarFormaPagoNominaSelect()
        {
            var forma_pago = db.C_nomina_forma_pago.Where(x => x.activo == true).OrderBy(x=>x.nombre_forma_pago).ToList();
            return PartialView("../CATALOGOS/NOMINA/_FormaPagoSelect", forma_pago);
        }
        public PartialViewResult ConsultarTipoCuentaBancariaNominaSelect()
        {
            var tipo_cuenta = db.C_nomina_tipo_cuenta_bancaria.Where(x => x.activo == true).OrderBy(x=>x.nombre_tipo_cuenta_bancaria).ToList();
            return PartialView("../CATALOGOS/NOMINA/_TipoCuentaBancariaSelect", tipo_cuenta);
        }
        public PartialViewResult ConsultarRegimenPatronalNominaSelect()
        {
            var regimen = db.C_nomina_regimen_patronal.Where(x => x.activo == true).OrderBy(x=>x.numero_regimen_partonal).ToList();
            return PartialView("../CATALOGOS/NOMINA/_RegimenPatronalSelect", regimen);
        }
        public PartialViewResult ConsultarEstadoCivilNominaSelect()
        {
            var estado_civil = db.C_nomina_estado_civil.Where(x => x.activo == true).OrderBy(x=>x.nombre_estado_civil).ToList();
            return PartialView("../CATALOGOS/NOMINA/_EstadoCivilSelect", estado_civil);
        }

        public PartialViewResult ConsultarGruposPagosNominaSelect()
        {
            var grupos_pago = db.C_nomina_grupos_pagos.Where(x => x.activo == true).OrderBy(x=>x.nombre_grupo_pago).ToList();
            return PartialView("../CATALOGOS/NOMINA/_GruposPagosSelect", grupos_pago);
        }

        public PartialViewResult ConsultarFrecuenciaPagoNominaSelect()
        {
            var frecuencias_pagos = db.C_nomina_frecuencias_pagos.Where(x => x.activo == true).OrderBy(x=>x.nombre_frecuencia_pago).ToList();
            return PartialView("../CATALOGOS/NOMINA/_FrecuenciasPagosSelect", frecuencias_pagos);
        }
        public PartialViewResult ConsultarContratoNominaSelect()
        {
            var contrato = db.C_nomina_contrato.Where(x => x.activo == true).OrderBy(x=>x.nombre_contrato).ToList();
            return PartialView("../CATALOGOS/NOMINA/_ContratosSelect", contrato);
        }

        public PartialViewResult ConsultarCiudadNominaSelect()
        {
            var ciudad = db.C_direcciones_ciudades.Where(x => x.activo == true).OrderBy(x=>x.nombre_ciudad).ToList();
            return PartialView("../CATALOGOS/NOMINA/_CiudadSelect", ciudad);
        }




        public PartialViewResult ConsultarTipoIncidenciaNominaSelect()
        {
            var incidencia = db.C_nomina_incidencias_tipos.Where(x=> x.activo == true).OrderBy(x => x.nombre_tipo_incidencia).ToList();
            return PartialView("../CATALOGOS/NOMINA/_TiposIncidenciasSelect", incidencia);
        }
        public PartialViewResult ConsultarTipoBajaNominaSelect()
        {
            var incidencia = db.C_nomina_incidencias_tipos.OrderBy(x => x.nombre_tipo_incidencia).ToList();
            return PartialView("../CATALOGOS/NOMINA/_TiposIncidenciasSelect", incidencia);
        }
        public PartialViewResult ConsultarEstatusPreaAltaNomina()
        {
            var estatus = db.C_nomina_empleados_solicitudes_status.OrderBy(x => x.nombre_status).ToList();
            return PartialView("../CATALOGOS/NOMINA/_EstatusPreAltaEmpleadoNomina", estatus);
        }
        #endregion

        #region CASETA

        public PartialViewResult ConusltarAreasTiposEntradasCaseta(int id_establo, int id_tipo_entrada)
        {
            var areas_establo_tipo_entrada = db.C_establos_caseta_tipos_entradas_areas.
                Where(x => x.C_establos_areas.id_establo == id_establo && x.id_tipo_entrada_caseta == id_tipo_entrada && x.activo == true).OrderBy(x => x.C_establos_areas.nombre_area).ToList();
            return PartialView("../ESTABLOS/Casetas/BitacoraCaseta/_AreasEstabloTipoEntradaSelect", areas_establo_tipo_entrada);
        }

        #endregion

        #region OBTENER ARREGLOS DE ID PARA CONSULTAS RAPIDAS EN MÉTODOS EXTERNOS
        
        public int[] EstablosUsuariosID(int id_usuario)
        {
            var cargos_usuarios = from establo in db.C_usuarios_establos
                                  where establo.id_usuario == id_usuario && establo.activo == true
                                  select (int)establo.id_establo;
            return cargos_usuarios.Distinct().ToArray();
        }
        public int[] CargoContableUsuariosID(int id_usuario)
        {
            var cargos_usuarios = from carg in db.C_usuarios_cargos_contables
                                  where carg.id_usuario == id_usuario && carg.activo == true
                                  select (int)carg.id_cargo_contable_g;
            return cargos_usuarios.ToArray();
        }

        public int[] CuentasContableUsuariosID(int id_usuario)
        {
            var ctas_usuarios = from cta in db.C_usuarios_cuentas_contables
                                  where cta.id_usuario == id_usuario && cta.activo == true
                                  select (int)cta.id_cuenta_contable_g;
            return ctas_usuarios.ToArray();
        }

        public int[] ConsultarCentrosUsuarioID(int id_usuario)
        {
            var centros = from cent_us in db.C_usuarios_centros
                          join usu in db.C_usuarios_corporativo on cent_us.id_usuario equals usu.id_usuario_corporativo
                          where cent_us.activo == true && cent_us.id_usuario == id_usuario && cent_us.C_centros_g.activo == true
                          select cent_us.C_centros_g.id_centro_g;
            return centros.ToArray();
        }

        public int[] ConsultarTiposRequisicionesID()
        {
            return db.C_compras_requisiciones_tipos.Where(x => x.activo == true).Select(x => x.id_requisicion_tipo).ToArray();
        }

        public int[] ConsultarProveedoresID()
        {
            return db.C_compras_proveedores.Where(x => x.activo == true && x.id_proveedor_status == 1).Select(x => x.id_compras_proveedor).ToArray();
        }

        public int[] ConsultarAlmacenesUsuarioID(int id_usuario)
        {
            return db.C_almacen_almacenes_usuarios.Where(x => x.activo == true && x.id_usuario == id_usuario).Select(x => (int)x.id_almacen_g).ToArray();
        }

        public int[] ConsultarTiposArticulosID()
        {
           return db.C_articulos_tipos.Where(x => x.activo == true).Select(x => (int)x.id_articulo_tipo).ToArray();
        }


        public int[] ConsultarProveedoresAlimentacionID()
        {
            return db.C_compras_proveedores.Where(x => x.activo == true && x.id_proveedor_status == 1 && x.prov_alimentacion == true).Select(x => x.id_compras_proveedor).ToArray();
        }
        public int[] ConsultarArticulosTipoFichasBasculaID(int tipo_ficha)
        {
            return db.C_articulos_catalogo_bascula.Where(x => x.activo == true && x.id_ficha_tipo == tipo_ficha).Select(x => x.C_articulos_catalogo.id_articulo).Distinct().ToArray();
        }


        public int[] ConsultarClientesEnviosLecheID()
        {
            return db.C_envios_leche_clientes.Where(x => x.activo == true).Select(x => x.id_envio_leche_cliente_ms).Distinct().ToArray();
        }

        public int[] ConsultarDestinosClienteID(int id_cliente)
        {
            if (id_cliente == 0) { return db.C_envios_leche_destinos.Where(x => x.activo == true).Select(x => x.id_destino_envio_leche).ToArray(); }

            return db.C_envios_leche_cliente_destinos.Where(x => x.id_envio_leche_cliente_ms == id_cliente && x.activo == true).Select(x => (int)x.id_destino_envio_leche).Distinct().ToArray();
        }


        public int[] ConsultarDepartamentosNominaID()
        {
            return db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.C_nomina_departamentos.activo == true && x.C_nomina_departamentos.activo == true
            && x.Frepag_id == 410).Select(x => x.C_nomina_departamentos.id_departamento_empleado).Distinct().ToArray();
        }

        public int[] ConsultarDepartamentosValesAlmacenesID()
        {
            return db.C_departamentos_g.Where(x => x.activo == true).Select(x => x.id_departamento_g).Distinct().ToArray();
        }
        public int[] ConsultarEmpleadosNominaID()
        {
            return db.C_nomina_empleados.Where(x => x.Estatus == "A" && x.Frepag_id == 410).Select(x => x.Empleado_id).Distinct().ToArray();
        }

        public int[] ConsultarEquiposEstablosID()
        {
            return db.C_establos_equipos.Where(x => x.activo == true).Select(x => x.id_establo_equipo).ToArray();
        }

        public int[] ConsultarEstatusDietasID()
        {
            return db.C_alimentacion_dietas_status.Where(x => x.activo == true).Select(x => x.id_dieta_status).ToArray();
        }

        public int[] ConsultarTiposCausaMuerteSalidaGanado()
        {
            return db.C_salida_ganado_causas_muerte_tipos.Where(x => x.activo == true).Select(x => x.id_causa_muerte_tipo).ToArray();
        }
        public int[] ConsultarTiposSalidaGanado()
        {
            return db.C_salida_ganado_tipos_salidas.Where(x => x.activo == true).Select(x => x.id_tipo_salida_ganado).ToArray();
        }

        #endregion

        #region Productores Alimentacion



        public PartialViewResult ConsultarProductoresAlmacen(int[] id_almacen_g)
        {
            try
            {
                int id_usuario = (int)Session["LoggedId"];
                if (id_almacen_g.Contains(0)) { ConsultarAlmacenesUsuarioID(id_usuario); }
                var productores = db.C_alimentacion_productores.Where(x => id_almacen_g.Contains((int)x.id_almacen_g) && x.activo == true).OrderBy(x => x.nombre_productor).ToList();
                return PartialView("ALIMENTACION/ProductoresAlimento/_ProductoresAlimentoSelect", productores);
            }
            catch (Exception)
            {
                return PartialView("ALIMENTACION/ProductoresAlimento/_ProductoresAlimentoSelect", null);
            }
        }


        #endregion


        #region DIETAS
        public PartialViewResult ConsultarGruposDietasSelect()
        {
            var grupos = db.C_alimentacion_dietas_grupos.Where(x => x.activo == true).OrderBy(x => x.nombre_grupo).ToList();
            return PartialView("ALIMENTACION/Dietas/_GruposDietasSelect", grupos);
        }
        public PartialViewResult ConsultarDietasAutorizadasSelect(int[] id_status)
        {
            var grupos = db.C_alimentacion_dietas_g.Where(x => x.activo == true && id_status.Contains((int)x.id_dieta_status)).OrderBy(x => x.fecha_registro).ToList();  //AUTORIZADA
            return PartialView("ALIMENTACION/Dietas/_DietasAutorizadasSelect", grupos);
        }
        public PartialViewResult ConsultarEstatusDietasSelect()
        {
            var estatus = db.C_alimentacion_dietas_status.Where(x => x.activo == true).OrderBy(x => x.nombre_status).ToList();
            return PartialView("ALIMENTACION/Dietas/_EstatusDietasSelect", estatus);
        }

        #endregion


        #region TRACKER
        public PartialViewResult ConsultarMovimientosTrackerSelect()
        {
            var trackers = db_tracker.C_tracker_movimientos_tipos.OrderBy(x => x.nombre_tipo).ToList();
            return PartialView("ALIMENTACION/Tracker/_TipoMovimientosSelect", trackers);
        }

        public PartialViewResult ConsultarArticulosIngredientesEstablo(int id_establo)  //CONSULTA LOS ARTÍCULOS QUE YA ESTÉN LIGADOS A UN ALIMENTO X ESTABLO
        {
            var art_ings = db.C_alimentacion_tracker_articulos_ingredientes.Where(x => x.id_establo == id_establo && x.activo == true).Select(x => x.C_articulos_catalogo).Distinct().OrderBy(x => x.nombre_articulo);
            return PartialView("../CATALOGOS/BASCULA/_TipoArticuloBasculaSelect", art_ings);
        }


        #endregion

        #region SOPORTE
        public PartialViewResult ConsultarCategoriaSoporteSelect()
        {
            var soporte = db.C_soporte_categorias.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/SOPORTE/_SoporteCategoriaSelect", soporte);
        }
        public PartialViewResult ConsultarEstatusSoporteSelect()
        {
            var soporte = db.C_soporte_estado.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/SOPORTE/_SoporteEstatusSelect", soporte);
        }
        public PartialViewResult ConsultarPrioridadSoporteSelect()
        {
            var soporte = db.C_soporte_prioridad.Where(x => x.activo == true).ToList();
            return PartialView("../CATALOGOS/SOPORTE/_SoportePrioridadSelect", soporte);
        }
        public PartialViewResult ConsultarTiposSoporteSelect(int categoria)
        {
            var soporte = db.C_soporte_tipos.Where(x => x.activo == true && x.id_soporte_categoria == categoria).ToList();
            return PartialView("../CATALOGOS/SOPORTE/_SoporteTiposSelect", soporte);
        }
        #endregion
    }
}