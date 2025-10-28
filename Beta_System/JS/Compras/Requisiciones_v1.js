$('input[type="date"]').val(today);
function SiguienteDia() {
    var hoy = new Date();
    var mañana = new Date(hoy);
    mañana.setDate(hoy.getDate() + 1);
    var actualizacion = mañana.toISOString().split('T')[0];
    $("#fecha_vigencia").val(actualizacion);
}
$('#requi_fecha_inicial').val(last_week);


//------------------ GENERACION DE REQUISICIONES
//#region GENERAR REQUISICION
var modos_requi = 1;  //1 BUSQUEDA CON FILTRO | 2 BUSQUEDA AUTOMATICA
var count_tabla = 1;
var articuloreal;
function LimpiarFormularioRequi() {
    $("#div_formulario_requi").css("display", "none");
    $("#id_tipo_requisicion").find('option[value=0]').prop('selected', 'selected');
    $("#id_solicitante").find('option[value=0]').prop('selected', 'selected');
    $("#select2-id_solicitante-container").text("Selecciona el solicitante");
    $("#unidad_medida").text('');
    $("#id_centro").val('selecte_centro');
    $("#concepto").val('');
    $("#id_prioridad").val('1');
    $("#id_articulo").val('');
    $("#id_articulos").val('');
    $("#id_unidad_medida").val('');
    $("#cantidad").val('');
    $("#precio").val('$0.00');
    $("#observacion").val('');
    $("#select_cargo").val('select_cargo');
    $("#select_cuenta").val('selecte_cuenta');
    $("#thead_requi_articulos").html('');
    $("#concepto").css("border", "");
    $("#codigo_articulo").text('');
    $("#folio_inversion").val("");
    try { ConsultarRequiGenerada(); } catch (e) { }

    $(".input_conceptos").val("");
    $(".input_valid").each(function () {
        $(this).css("border", "");
    });
    $(".input_validos").each(function () {
        $(this).css("border", "");
    });
    $("#select2-id_solicitante-container").css("border", "");

    $("#id_articulos").text('');
    $("#cuentas").text('');
    $("#div_cotizaciones_proveedor").html("");


    $("#precio").val('0');
    CalcularImporteRequisicion();
}


function ShowHideNewCotizacion(modo) {
    if (modo == 1) {  //REGISTRO
        var input_precio = $("#div_new_cotizacion");
        id_cotizacion_servicio = 0;
        if ($("#check_new_cotizacion").is(':checked')) {
            input_precio.css("display", "block");
            $("#div_cotizaciones_proveedor").html("");
        }
        else {
            ConsultarCotizacionesArticuloProveedor(1);
            input_precio.css("display", "none");
        }
    }
    else {  //EDICIÓN
        $("#precio_new_cotizacion_serv").val('');
        var input_precio = $("#div_new_cotizacion_serv");
        id_cotizacion_servicio_serv = 0;
        if ($("#check_new_cotizacion_serv").is(':checked')) {
            input_precio.css("display", "block");
            $("#div_cotizaciones_proveedor_serv").html("");
        }
        else {
            ConsultarCotizacionesArticuloProveedor(2);
            input_precio.css("display", "none");
        }
    }
}

function ShowHideNewCotizacionEditar() {
    id_cotizacion_servicio_serv = 0;
    $("#div_cotizaciones_proveedor_serv").html("");
    if ($("#check_cambiar_cotizacion_serv").is(':checked')) {
        $("#div_formulario_cotizacion_serv").css("display", "inline");
        $("#id_proveedor_serv").val('');
        $("#clave_proveedor_serv").val('');
        $("#check_new_cotizacion_serv").prop("checked", false);
        ShowHideNewCotizacion(2)
    }
    else {
        $("#div_formulario_cotizacion_serv").css("display","none");
    }
}

function SetCotizacionServicio(id_cotizacion, modo) {
    if (modo == 1) { id_cotizacion_servicio = id_cotizacion; }  //REGISTRO (NUEVO)
    else if (modo == 2) { id_cotizacion_servicio_serv = id_cotizacion; }  //EDITAR
}

function ConsultarCotizacionesArticuloProveedor(modo) {
    var cve_proveedor;
    var id_articulo;
    if (modo == 1) {  //REGISTRO
        cve_proveedor = $("#clave_proveedor").val();
        id_articulo = $("#codigo_articulo").val();
        if (id_articulo == "") {
            id_articulo = $("#id_articulos").val();
        }
        if (id_articulo == "" || cve_proveedor == '') {
            return;
        }
    }
    else {  //EDITAR
        cve_proveedor = $("#clave_proveedor_serv").val();
        var id_articulo = $("#id_nuevo_articulo_editar_serv").val();
        if (id_articulo == "" || cve_proveedor == '') {
            return;
        }
    }
    
    $("#div_cotizaciones_proveedor").html("");
    $("#div_cotizaciones_proveedor_serv").html("");
    try { $("#datatable_cotizaciones_articulo_proveedor").dataTable().fnDestroy();
    } catch (e) { }
    if (!$("#check_new_cotizacion").is(':checked') && modo == 1) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../REQUISICIONES/ConsultarCotizacionesArticuloProveedor",
            data: {
                cve_proveedor: cve_proveedor,
                id_articulo: id_articulo,
                modo: modo
            },
            success: function (response) {
                $("#div_cotizaciones_proveedor").html(response);
                $('#datatable_cotizaciones_articulo_proveedor').DataTable({
                    ordering: false,
                    paging: false,
                    searching: false,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: !0,
                    scrollY: false,
                    select: true
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    if (!$("#check_new_cotizacion_serv").is(':checked') && modo == 2) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../REQUISICIONES/ConsultarCotizacionesArticuloProveedor",
            data: {
                cve_proveedor: cve_proveedor,
                id_articulo: id_articulo,
                modo: modo
            },
            success: function (response) {
                $("#div_cotizaciones_proveedor_serv").html(response);
                $('#datatable_cotizaciones_articulo_proveedor').DataTable({
                    ordering: false,
                    paging: false,
                    searching: false,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: !0,
                    scrollY: false,
                    select: true
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}
function mostrarfiltroRequi(modo) {
    if (modo == 1) {
        $(".divs_search").css("display", "block");
        //$("#filtros_div_requi_1").css("display", "block");
        //$("#filtros_div_requi_2").css("display", "block");
        //$("#filtros_div_tipo").css("display", "block");
        //$("#filtros_div_status").css("display", "block");
        modos_requi = modo;
    }
    else {
        $(".divs_search").css("display", "none");

        //$("#filtros_div_requi_1").css("display", "none");
        //$("#filtros_div_requi_2").css("display", "none");
        //$("#filtros_div_tipo").css("display", "none");
        //$("#filtros_div_status").css("display", "none");
        modos_requi = modo;
    }
}
function ConsultarRequiGenerada() {
    var id_centro = $("#id_centro_search").val();
    jsShowWindowLoad();
    //BUSCAR CON FILTRO
    if (modos_requi == 1) {
        var fecha_inicial = $("#requi_fecha_inicial").val();
        var fecha_final = $("#requi_fecha_final").val();
        var id_status = $("#requi_id_status").val();
        var id_tipo_requi = $("#requi_id_tipo_requi").val();
        var folio = $("#requi_folio").val();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 90000000,
            url: "../REQUISICIONES/ConsultarRequisGeneradaFiltro",
            data: {
                id_centro: id_centro,
                fecha_inicial: fecha_inicial,
                fecha_final: fecha_final,
                id_status: id_status,
                id_tipo_requi: id_tipo_requi,
                folio: folio
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#div_requis_revision").html(response);
                $('#datatable_requisiciones_general').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{
                        extend: "copy",
                        className: "btn-sm",
                        columns: ':visible'
                    }, {
                        extend: "csv",
                        className: "btn-sm",
                        bom: true,
                        charset: 'utf-8',
                        customize: function (csv) {
                            return "\uFEFF" + csv;
                        }
                    }, {
                        extend: "excel",
                        className: "btn-sm"
                    }, {
                        extend: "pdf",
                        className: "btn-sm"
                    }, {
                        extend: "print",
                        className: "btn-sm"
                    }],
                    responsive: false,
                    select: true,
                    pageLength: 20,
                    scrollY: false,
                    scrollX: true
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    //BUSCAR SIN FILTRO
    else {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 90000000,
            url: "../REQUISICIONES/ConsultarRequisGenerada",
            data: {
                id_centro: id_centro
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#div_requis_revision").html(response);
                $('#datatable_requisiciones_general').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{
                        extend: "copy",
                        className: "btn-sm",
                        columns: ':visible'
                    }, {
                        extend: "csv",
                        className: "btn-sm",
                        bom: true, // Añadir BOM para UTF-8
                        charset: 'utf-8', // Especificar la codificación
                        customize: function (csv) {
                            // Añadir el BOM manualmente y devolver el CSV modificado
                            return "\uFEFF" + csv;
                        }
                    }, {
                        extend: "excel",
                        className: "btn-sm"
                    }, {
                        extend: "pdf",
                        className: "btn-sm"
                    }, {
                        extend: "print",
                        className: "btn-sm"
                    }],
                    responsive: false,
                    select: true,
                    pageLength: 20,
                    scrollY: false,
                    scrollX: true
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}
function ObtenerNombreArticulo() {
    var id_articulo = parseInt($("#codigo_articulo").text());
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarNombreArt",
        data: {
            id_articulo: id_articulo
        },
        success: function (response) {
            var data = $.parseJSON(response);
            $("#id_articulo").text(data[0].nombre_articulo);
            articuloreal = data[0].nombre_articulo;
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ArticuloRequiExiste(articulo) {
    var cargo = $("#select_cargo").val();
    var cuenta = $("#select_cuenta").val();
    var existe = false;

    $('#articulos_table_solicitud tr').each(function () {
        var $tds = $(this).find('td');

        if ($tds.eq(1).text() === articulo &&
            $tds.eq(11).text() === cargo &&
            $tds.eq(12).text() === cuenta) {
            existe = true;
            return existe;
        }
    });

    return existe;
}

function AgregarArticuloDetalle(count_tabla, id_articulo, articulo, cantidad, precio, unidad_medida, id_unidad_medida, observacion, id_centro_g, cargo_contable, id_cargo_contable, id_cuenta_contable_g, new_cotizacion, id_tipo_requi, cve_prov, id_cotizacion, id_tipo_moneda, id_articulo_original) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/RegistrarArtRequi",
        data: {
            count_tabla: count_tabla,
            id_articulo: id_articulo,
            articulo: articulo,
            cantidad: cantidad,
            precio: precio,
            unidad_medida: unidad_medida,
            id_unidad_medida: id_unidad_medida,
            observacion: observacion,
            id_centro_g: id_centro_g,
            cargo_contable: cargo_contable,
            id_cargo_contable: id_cargo_contable,
            id_cuenta_contable_g: id_cuenta_contable_g,
            new_cotizacion: new_cotizacion,
            id_tipo_requi: id_tipo_requi,
            cve_prov: cve_prov,
            id_cotizacion: id_cotizacion,
            id_tipo_moneda: id_tipo_moneda,
            id_articulo_original: id_articulo_original
        },
        success: function (result) {
            $(".input_detalle").val('');
            $("#thead_requi_articulos").append(result);

            $("#unidad_medida").text('');
            $("#id_articulos").text('');
            $(".input_valid").each(function () {
                $(this).css("border", "");
            });
            $(".input_validos").each(function () {
                $(this).css("border", "");
            });
            //$("#cuentas").text('');
            $("#unidad_medida").text('');
            $("#precio").val('0.00');
            id_cotizacion_servicio = 0;
            $("#check_new_cotizacion").prop("checked", false);
            $("#div_cotizaciones_proveedor").html("");
            $("#clave_fiscal_moneda_new_cotizacion").text("");
            $("#moneda_new_cotizacion").text("");
            ShowHideNewCotizacion();
            CalcularImporteRequisicion();
            $("#id_articulos").focus();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function CalcularImporteRequisicion() {
    var importe_total_requisicion = 0
    var total_cantidad = 0;

    $(".importes_requisicion").each(function () {
        var valor = $(this).text();
        var sin_digitos = valor.split(",").join("").replace("$", "");
        importe_total_requisicion += parseFloat(sin_digitos);
    });
    $(".cantidads").each(function () {
        var valor = $(this).text();
        total_cantidad += parseFloat(valor);
    });
    

    importe_total_requisicion = new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 9,
        maximumFractionDigits: 9
    }).format(importe_total_requisicion);
    $("#importe_total_requisicion").text("IMPORTE TOTAL: $" + importe_total_requisicion);

    $("#cantidad_total_requisicion").text("CANTIDAD TOTAL: " + total_cantidad);
}



function RegistrarRequi() {
    var id_tipo_requi = $("#id_tipo_requisicion").val();
    var concepto = $('#concepto').val();
    var valid_concepto = true;
    if (id_tipo_requi == 2) {
        var factura = $("#factura_concepto").val();
        var prov = $("#proveedor_concepto").val();
        var desc = $("#descripcion_concepto").val();
        if (factura == "" || prov == "" || desc == "") {
            valid_concepto = false;
            $(".input_conceptos").each(function () {
                if ($(this).val() == "") { $(this).css("border-color", "red"); }
                else { $(this).css("border", ""); }
            });
        }
        else {
            concepto = factura + " " + desc + " " + prov;
        }
    }

    if (valid_concepto == false) {
        iziToast.warning({
            title: '¡El concepto es un campo obligatorio!',
            message: '',
        });
    }
    else if ($("#id_centro").val() == 'selecte_centro') {
        iziToast.warning({
            title: '¡El centro es un campo obligatorio!',
            message: '',
        });
        $("#id_centro").css("border-color", "red");
    }
    else if (concepto == '') {
        iziToast.warning({
            title: '¡El concepto es un campo obligatorio!',
            message: '',
        });
        $("#concepto").css("border-color", "red");
    }
    else if ($('#articulos_table_solicitud tr').length == 1) {
        iziToast.warning({
            title: 'Agrega articulos a la solicitud',
            message: '',
        });
    }
    else if (id_tipo_requi == 2 && ($("#id_solicitante").val() == "" || $("#id_solicitante").val() == undefined || $("#id_solicitante").val() == 0)) {
        iziToast.warning({
            title: 'Selecciona un solicitante para la requisición de servicio',
            message: '',
        });
        $("#select2-id_solicitante-container").css("border", "1px solid red");
    }
    else {
        var c_requi_g = {};
        c_requi_g.id_requisicion_tipo = id_tipo_requi;
        c_requi_g.id_centro_g = $('#id_centro').val();
        c_requi_g.concepto = concepto;
        c_requi_g.id_prioridad = $('#id_prioridad').val();
        if (id_tipo_requi == 2) { c_requi_g.id_empleado_solicita = $("#id_solicitante").val(); }

        //var folio = $("#id_centro").val();
        var counts = 0;
        var id_articulos = [];
        var nombre_articulo = [];
        var cantidads = [];
        var precios = [];
        var id_unidad_medidas = [];
        var observacions = [];
        var id_centros_g = [];
        var id_cargos_contable = [];
        var id_cuentas_contable_g = [];

        $(".id_articulos_real").each(function () {
            id_articulos[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".nombre_articulo").each(function () {
            nombre_articulo[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".cantidads").each(function () {
            cantidads[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".precios").each(function () {
            precios[counts] = $(this).text().replace('$', '').replace(/,/g, ''); counts++;
        });
        counts = 0;
        $(".id_unidad_medidas").each(function () {
            id_unidad_medidas[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".observacions").each(function () {
            observacions[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".id_centros_g").each(function () {
            id_centros_g[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".id_cargos_contable").each(function () {
            id_cargos_contable[counts] = $(this).text(); counts++;
        });
        counts = 0;
        $(".id_cuentas_contable_g").each(function () {
            id_cuentas_contable_g[counts] = $(this).text(); counts++;
        });
        $.ajax({
            type: "POST",
            async: false,
            url: "../REQUISICIONES/GenerarRequiGeneral",
            data: {
                c_requi_g: c_requi_g
            },
            success: function (response) {
                jsShowWindowLoad();
                var datos = response.split('|');
                var id_requi_g = datos[0];
                var folio = datos[1];
                if (id_requi_g != 0) {
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 9000000,
                        url: "../REQUISICIONES/GenerarRequiDetallada",
                        data: {
                            id_requi_g: id_requi_g,
                            id_articulos: id_articulos,
                            nombre_art: nombre_articulo,
                            cantidads: cantidads,
                            precios: precios,
                            id_unidad_medidas: id_unidad_medidas,
                            observacions: observacions,
                            id_centros_g: id_centros_g,
                            id_cargos_contable: id_cargos_contable,
                            id_cuentas_contable_g: id_cuentas_contable_g
                        },
                        success: function (response) {
                            if (response != -1) {

                                jsRemoveWindowLoad();
                                if (id_tipo_requi == 2) {
                                    ProcesarRequisicionServicio(id_requi_g, precios, id_articulos, id_unidad_medidas);
                                }
                                if (id_tipo_requi == 3) {
                                    var folio_inversion = $("#folio_inversion").val();
                                    ProcesarRequisicionInversion(id_requi_g, folio_inversion);
                                }
                                iziToast.success({
                                    title: '¡Requisicion generada exitosamente!    Requisicion:' + folio,
                                    message: '',
                                    timeout: false
                                });
                                $("#consultar_requi").click();
                                LimpiarFormularioRequi();
                                ShowHideFormularioProveedor(2);
                            }
                            else {
                                iziToast.error({
                                    title: 'Error',
                                    message: 'Ocurrió un problema al registrar el detalle de la solicitud',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el encabezado de la requisicion',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

function ProcesarRequisicionInversion(id_requi_g, folio_inversion) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ProcesarRequisicionInversion",
        data: {
            id_requi_g: id_requi_g,
            folio_inversion: folio_inversion

        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: "Requisición de inversión procesada correctamente con el folio: " + folio_inversion + "",
                    message: '',
                });
            }
            else {
                iziToast.error({
                    title: 'Error al procesar la requisición de inversión',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ProcesarRequisicionServicio(id_requi_g, precios, id_articulos, id_unidad_medidas) {
    var id_cotizaciones = [];
    var id_proveedores = [];
    var id_tipos_moneda = [];
    var counts = 0;
    $(".id_cotizacion").each(function () {
        id_cotizaciones[counts] = $(this).text(); counts++;
    });
    var counts = 0;
    $(".id_proveedor").each(function () {
        id_proveedores[counts] = $(this).text(); counts++;
    });
    var counts = 0;
    $(".id_tipo_moneda").each(function () {
        id_tipos_moneda[counts] = $(this).text(); counts++;
    });
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ProcesarRequisicionServicio",
        data: {
            id_requi_g: id_requi_g,
            id_cotizaciones: id_cotizaciones,
            id_proveedores: id_proveedores,
            id_tipos_moneda: id_tipos_moneda,
            precios: precios,
            id_articulos: id_articulos,
            id_unidad_medidas: id_unidad_medidas
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Requisición de servicio procesada correctamente',
                    message: '',
                });
            }
            else {
                iziToast.error({
                    title: 'Error al procesar la requisición de servicio',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function CambiarArticuloRequisicionServicio(id_requi_d, id_requi_g, cantidad, id_cargo, id_cuenta) {
    var observacion = $("#obsercacion_articulo_editar").val();
    var precio = $("#precio_new_cotizacion_serv").val();
    var cve_prov = $("#clave_proveedor_serv").val();
    var id_articulo = $("#id_nuevo_articulo_editar_serv").val();
    var cambiar_cotizacion = false;
    if ($("#check_cambiar_cotizacion_serv").is(':checked')) { cambiar_cotizacion = true; }
    if ($("#check_new_cotizacion_serv").is(':checked')) { id_cotizacion_servicio_serv = 0; }

    if (cambiar_cotizacion == false) { precio = 0; }
    if (id_cotizacion_servicio_serv > 0) { precio = 0; }

    if (cve_prov == '' && cambiar_cotizacion == false) {
        cve_prov = 0;
    }
    if (cantidad == '' || cantidad <= 0) {
        iziToast.warning({
            title: 'Ingresa una cantidad válida',
            message: '',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv == 0 && cambiar_cotizacion == true && !$("#check_new_cotizacion_serv").is(':checked')) {
        iziToast.warning({
            title: 'Cotización no detectada',
            message: 'Si no existen cotizaciones ingrese el nuevo precio',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv == 0 && cambiar_cotizacion == true && $("#check_new_cotizacion_serv").is(':checked') && (precio == '' || precio <= 0)) {
        iziToast.warning({
            title: 'Ingresa el nuevo precio de la cotización',
            message: '',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv > 0 && cambiar_cotizacion == false) {
        iziToast.warning({
            title: 'Cotización no detectada',
            message: 'Cierre esta ventana emergente e intentelo de nuevo',
        });
        return;
    }
    else if (id_articulo == undefined || id_articulo == '') {
        iziToast.warning({
            title: 'Ingresa un producto válido',
            message: '',
        });
        return;
    }
    else {
        //alert("OK");
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999999,
            title: 'ATENCIÓN',
            message: '¿Está seguro de modificar la requisición de servicio?',
            position: 'center',
            buttons: [
                ['<button><b>Si. modificar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../REQUISICIONES/CambiarArticuloRequisicionServicio",
                        data: {
                            id_requi_d: id_requi_d,
                            id_requi_g: id_requi_g,
                            id_cotizacion: id_cotizacion_servicio_serv,
                            cve_prov: cve_prov,
                            precio: precio,
                            cantidad: cantidad,
                            id_articulo: id_articulo,
                            observacion: observacion,
                            id_cargo: id_cargo,
                            id_cuenta: id_cuenta,
                            cambiar_cotizacion: cambiar_cotizacion
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                $("#m_editar_articulo_requisicion_auth").modal("hide");
                                iziToast.success({
                                    title: 'Requisición de servicio actualizada correctamente',
                                    message: '',
                                });
                                ConsultarDetalleRequiGeneral(id_requi_g);
                            }
                            else if (response == 1) {
                                iziToast.warning({
                                    title: 'La requisicióm ya fué autorizada',
                                    message: '',
                                });
                            }
                            else if (response == 2) {
                                iziToast.warning({
                                    title: 'La requisición fue desactivada',
                                    message: '',
                                });
                            }
                            else if (response == 3) {
                                iziToast.warning({
                                    title: 'No se puede cambiar el artículo y dejar la misma cotización',
                                    message: '',
                                });
                            }
                            else if (response == 4) {
                                iziToast.warning({
                                    title: 'No se encontró información del proveedor',
                                    message: 'Intente nuevamente',
                                });
                            }
                            else if (response == 5) {
                                iziToast.warning({
                                    title: 'El artículo no cuenta con una cotización procesada',
                                    message: '',
                                });
                            } else {
                                iziToast.warning({
                                    title: 'Ocurrió un error al modificar la requisición de servicio',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }, true],
                ['<button>No, regresar</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }],
            ],
            onClosing: function (instance, toast, closedBy) {
                console.info('Closing | closedBy: ' + closedBy);
            },
            onClosed: function (instance, toast, closedBy) {
                console.info('Closed | closedBy: ' + closedBy);
            }
        });
    }
}

function AgregarArticuloRequisicionServicio(id_requi_g, id_rubro) {
    var observacion = $("#obsercacion_articulo_editar").val();
    var id_cargo = $("#select_cargo_editar").val();
    var id_cuenta = $("#select_cuenta_editar").val();
    var precio = $("#precio_new_cotizacion_serv").val();
    var cantidad = $("#cantidad_articulo_editar").val();
    var cve_prov = $("#clave_proveedor_serv").val();
    var id_articulo = $("#id_nuevo_articulo_editar_serv").val();
    var cambiar_cotizacion = false;
    if ($("#check_cambiar_cotizacion_serv").is(':checked')) { cambiar_cotizacion = true; }
    if ($("#check_new_cotizacion_serv").is(':checked')) { id_cotizacion_servicio_serv = 0; }

    if (cambiar_cotizacion == false) { precio = 0; }
    if (id_cotizacion_servicio_serv > 0) { precio = 0; }

    if (cve_prov == '' && cambiar_cotizacion == false) {
        cve_prov = 0;
    }
    if (id_cargo == "select_cargo") {
        iziToast.warning({
            title: 'Ingresa un cargo válido',
            message: '',
        });
        return;
    }
    else if (id_cuenta == null || id_cuenta == undefined) {
        iziToast.warning({
            title: 'Ingresa una cuenta válida',
            message: '',
        });
        return;
    }
    else if (cantidad == '' || cantidad <= 0) {
        iziToast.warning({
            title: 'Ingresa una cantidad válida',
            message: '',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv == 0 && cambiar_cotizacion == true && !$("#check_new_cotizacion_serv").is(':checked')) {
        iziToast.warning({
            title: 'Cotización no detectada',
            message: 'Si no existen cotizaciones ingrese el nuevo precio',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv == 0 && cambiar_cotizacion == true && $("#check_new_cotizacion_serv").is(':checked') && (precio == '' || precio <= 0)) {
        iziToast.warning({
            title: 'Ingresa el nuevo precio de la cotización',
            message: '',
        });
        return;
    }
    else if (id_cotizacion_servicio_serv > 0 && cambiar_cotizacion == false) {
        iziToast.warning({
            title: 'Cotización no detectada',
            message: 'Cierre esta ventana emergente e intentelo de nuevo',
        });
        return;
    }
    else if (id_articulo == undefined || id_articulo == '') {
        iziToast.warning({
            title: 'Ingresa un producto válido',
            message: '',
        });
        return;
    }
    else {
        //alert("OK");
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999999,
            title: 'ATENCIÓN',
            message: '¿Está seguro de modificar la requisición de servicio?',
            position: 'center',
            buttons: [
                ['<button><b>Si. modificar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../REQUISICIONES/AgregarArticuloRequisicionServicio",
                        data: {
                            id_requi_g: id_requi_g,
                            id_cotizacion: id_cotizacion_servicio_serv,
                            cve_prov: cve_prov,
                            precio: precio,
                            cantidad: cantidad,
                            id_articulo: id_articulo,
                            observacion: observacion,
                            id_cargo: id_cargo,
                            id_cuenta: id_cuenta
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            $("#m_editar_articulo_requisicion_auth").modal("hide");
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Servicio agregado a la cotización correctamente',
                                    message: '',
                                });
                                ConsultarDetalleRequiGeneral(id_requi_g);
                            }
                            else if (response == 1) {
                                iziToast.warning({
                                    title: 'La requisicióm ya fué autorizada',
                                    message: '',
                                });
                            }
                            else if (response == 2) {
                                iziToast.warning({
                                    title: 'La requisición fue desactivada',
                                    message: '',
                                });
                            }
                            else if (response == 3) {
                                iziToast.warning({
                                    title: 'El proveedor no puede estar en 2 parcialidades',
                                    message: '',
                                });
                            }
                            else if (response == 4) {
                                iziToast.warning({
                                    title: 'No se encontró información del proveedor',
                                    message: 'Intente nuevamente',
                                });
                            }
                            else if (response == 5) {
                                iziToast.warning({
                                    title: 'El artículo no cuenta con una cotización procesada',
                                    message: '',
                                });
                            } else {
                                iziToast.warning({
                                    title: 'Ocurrió un error al modificar la requisición de servicio',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }, true],
                ['<button>No, regresar</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }],
            ],
            onClosing: function (instance, toast, closedBy) {
                console.info('Closing | closedBy: ' + closedBy);
            },
            onClosed: function (instance, toast, closedBy) {
                console.info('Closed | closedBy: ' + closedBy);
            }
        });
    }
}

function AddArticuloRequisicionServicio(id_requi_g, id_rubro) {
    id_cotizacion_servicio_serv = 0;
    $("#btn_actualizar_articulo_editar").attr('onclick', "AgregarArticuloRequisicionServicio(" + id_requi_g + ", " + id_rubro + ")");
    $("#btn_actualizar_articulo_editar").text("Agregar servicio");
    ShowHideNewCotizacionEditar();
    ConsultarCuentasContablesCargosSelect();

    $("#cantidad_articulo_editar").val('');
    $("#obsercacion_articulo_editar").val('');
    $("#txt_nombre_articulo_ediar").text('');
    ConsultarArticulosSistemaRubro(id_rubro, 0, 0, '');

    if (id_rubro == 2) {
        $("#check_cambiar_cotizacion_serv").prop("checked", true);
        $("#check_cambiar_cotizacion_serv").prop("disabled", true);
        ShowHideNewCotizacionEditar();
    }
    $("#m_editar_articulo_requisicion_auth").modal("show");
}


function EliminarArticuloRequisicionServicio(id_requi_d, id_requi_g, id_articulo) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de eliminar este artículo',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../REQUISICIONES/EliminarArticuloRequisicionServicio",
                    data: {
                        id_requi_d: id_requi_d,
                        id_requi_g: id_requi_g,
                        id_articulo: id_articulo
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == 0) {
                            iziToast.success({
                                title: 'Artículo eliminado de la requisición correctamente',
                                message: '',
                            });
                            ConsultarDetalleRequiGeneral(id_requi_g);
                        }
                        else if (response == 1) {
                            iziToast.error({
                                title: 'No se puede eliminar el único artículo de la requisición',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al eliminar el artículo',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }, true],
            ['<button>No, regresar</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }],
        ],
        onClosing: function (instance, toast, closedBy) {
            console.info('Closing | closedBy: ' + closedBy);
        },
        onClosed: function (instance, toast, closedBy) {
            console.info('Closed | closedBy: ' + closedBy);
        }
    });
}



//#endregion



//#region REVISAR,VALIDAR,AUTORIZAR REQUISICION
function ConsultarRequisiciones(status) {
    var requi = $("#requi_search").val();
    var id_cargo = undefined;
    if (status == 1) {
        id_cargo = $("#id_cargo_search_1").val();
    }
    else {
        id_cargo = $("#id_cargo_search_2").val();
    }

    if (id_cargo == undefined || id_cargo == "") {
        iziToast.warning({
            title: 'No se detectaron centros asignados',
            message: 'Favor de contactar a desarrollo',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../REQUISICIONES/ConsultarRequisiciones",
            data: {
                id_cargo: id_cargo,
                requi: requi,
                status: status
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (status == 1) {
                    $("#div_requis_revision").html(response);
                    $('#datatable_aut_firma_1').DataTable({
                        keys: false,
                        searching: true,
                        ordering: true,
                        paging: false,
                        dom: "Bfrtip",
                        buttons: [{
                            extend: "copy",
                            className: "btn-sm",
                            columns: ':visible'
                        }, {
                            extend: "csv",
                            className: "btn-sm"
                        }, {
                            extend: "excel",
                            className: "btn-sm"
                        }, {
                            extend: "pdf",
                            className: "btn-sm"
                        }, {
                            extend: "print",
                            className: "btn-sm"
                        }],
                        select: true,
                        responsive: false
                    });
                }
                else {
                    $("#div_requis_revision_d").html(response);
                }

            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }


}
function ConsultarDetalleRequi(nombre_modal, nombre_div, id_requi_g, firma) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarDetalleRequi",
        data: {
            id_requi_g: id_requi_g,
            firma: firma
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#" + nombre_modal + "").modal("show");
            $("#" + nombre_div + "").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarArticulosSistemaRubro(id_rubro, almacenable, id_articulo, nombre_articulo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000,
        url: "../ARTICULOS/ConsultarArticulosSistemaRubro",
        data: {
            id_rubro: id_rubro,
            almacenable: almacenable
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#id_nuevo_articulo_editar").html(response);
            $("#id_nuevo_articulo_editar").select2({
                placeholder: 'Selecciona un artículo',
                dropdownParent: $('#m_editar_articulo_requisicion_auth')
            });
            if (id_articulo != 0) {
                $("#id_nuevo_articulo_editar").val(id_articulo);
                $("#select2-id_nuevo_articulo_editar-container").text(nombre_articulo);
            }


            $("#id_nuevo_articulo_editar_serv").html(response);
            $("#id_nuevo_articulo_editar_serv").select2({
                placeholder: 'Selecciona un artículo',
                dropdownParent: $('#m_editar_articulo_requisicion_auth')
            });
            $("#select2-id_nuevo_articulo_editar_serv-container").text('Selecciona un artículo');
            $("#select2-id_nuevo_articulo_editar_serv-container").val('');

            if (id_articulo != 0) {
                $("#id_nuevo_articulo_editar_serv").val(id_articulo);
                $("#select2-id_nuevo_articulo_editar_serv-container").text(nombre_articulo);
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function EditarArticuloRequisicionAuth(id_requi_d, id_cargo, id_cuenta, cantidad, observacion, nombre_articulo, id_rubro, id_articulo, id_requi_g) {
    $("#btn_actualizar_articulo_editar").attr('onclick', "ActualizarArticuloRequisicionAuth(" + id_requi_d + ", " + id_requi_g + ", " + id_rubro + ")");
    $("#btn_actualizar_articulo_editar").text("Actualizar");
    $("#select_cargo_editar").find('option[value=' + id_cargo + ']').prop('selected', 'selected');
    ConsultarCuentasContablesCargosSelect();

    $("#select_cuenta_editar").find('option[value=' + id_cuenta + ']').prop('selected', 'selected');

    $("#cantidad_articulo_editar").val(cantidad);
    $("#obsercacion_articulo_editar").val(observacion);
    $("#txt_nombre_articulo_ediar").text(nombre_articulo);
    ConsultarArticulosSistemaRubro(id_rubro, 0, id_articulo, nombre_articulo);

    $("#check_cambiar_cotizacion_serv").prop("checked", false);
    $("#check_cambiar_cotizacion_serv").prop("disabled", false);
    if (id_rubro == 2) {
        $("#check_cambiar_cotizacion_serv").prop("checked", false);
        ShowHideNewCotizacionEditar();
    }
    $("#m_editar_articulo_requisicion_auth").modal("show");
}

function ActualizarArticuloRequisicionAuth(id_requi_d, id_requi_g, id_rubro) {
    var cantidad = $("#cantidad_articulo_editar").val();
    var observacion = $("#obsercacion_articulo_editar").val();
    var id_cargo = $("#select_cargo_editar").val();
    var id_cuenta = $("#select_cuenta_editar").val();
    var id_articulo = $("#id_nuevo_articulo_editar").val();
    
    if (id_rubro == 2) {
        CambiarArticuloRequisicionServicio(id_requi_d, id_requi_g, cantidad, id_cargo, id_cuenta);
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../REQUISICIONES/ActualizarArticuloRequisicionAuth",
            data: {
                id_requi_d: id_requi_d,
                cantidad: cantidad,
                observacion: observacion,
                id_cargo: id_cargo,
                id_cuenta: id_cuenta,
                id_articulo: id_articulo
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Requisición modificada correctamente',
                        message: '',
                    });
                    ConsultarDetalleRequi('m_detalle_requi_revision', 'div_requis_revision_detalle', id_requi_g, 1);
                    BotonesAutorizar(id_requi_g, 2, 1);
                    $("#m_editar_articulo_requisicion_auth").modal("hide");
                }
                else if (response == -1) {
                    iziToast.warning({
                        title: 'La requisición ya fué autorizada (1ra firma)',
                        message: '',
                    });
                }
                else if (response == -2) {
                    iziToast.error({
                        title: 'La requisición se encuentra cancelada',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un problema al ejecutar la operación',
                        message: '',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}



function ConsultarCuentasContablesCargosSelect() {
    var id_cargo_contable = $("#select_cargo_editar").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarCuentasContablesCargosSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            $(".select_cuenta").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function BotonesAutorizar(id_requi_g, id_status_requi, status) {
    $("#btn_aut_requi_autorizar").attr("onclick", "AutorizarRechazarRequi(" + id_requi_g + ", true, " + id_status_requi + "," + status + ")");
    $("#btn_aut_requi_no").attr("onclick", "AutorizarRechazarRequi(" + id_requi_g + ", false, 1008," + status + ")"); //1008 = RECHAZADA
}


function AutorizarRechazarRequi(id_requi_g, modo, id_status_requi, status) {
    jsShowWindowLoad();
    var comentarios = $("#comentarios_requi_aut").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/AutorizarRechazarRequi",
        data: {
            id_requi_g: id_requi_g,
            modo: modo,
            id_status_requi: id_status_requi,
            comentarios: comentarios
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Requisición procesada con exito',
                    message: '',
                });
                ConsultarRequisiciones(status);
                $("#m_detalle_requi_revision").modal("hide");
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un problema al ejecutar la operación',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarRequisicionesAutValidar() {
    var id_centro = $("#id_centro_search").val();
    var requi = $("#requi_search").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarRequisicionesAutValidar",
        data: {
            id_centro: id_centro,
            requi: requi
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_requis_revision").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ValidarFirmaRequi(id_requisicion) {
    var id_requisicion = id_requisicion;
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ValidarFirmaRequisicion",
        data: {
            id_requisicion: id_requisicion
        },
        success: function (response) {
            jsRemoveWindowLoad();
            //$("#div_requis_revision").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


//#endregion



//#region  PROCESO DE COTIZACION
function ConsultarCuentaCont() {
    var id_cargo_contable = $("#id_cargo_contable").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarCuentContSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            $("#id_cuenta_contable_g").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function EliminarFilaSalida(count) {
    $("#row_" + count + "").remove();
    CalcularImporteRequisicion();
}
function LimpiarCaptura() {
    $("#thead_requi_articulos").html("");
    $(".input_clean").val("");
    $("#unidad_medida").text('');
}
function LimpiarBordes() {
    $(".input_validos").each(function () {
        $(this).css("border", "");
    });
}


//------------------ REQUIS A REVISION
function EnfocarComponente(componente) {
    $("#select_cuenta").focus();
}
//COTIZACION DE ARTICULOS REQUISICION :)
function AccionCotizacion(accion) {
    //Modificar
    if (accion == 1) {
    }
    //Agregar
    else {
    }

    $("#m_articulos_requisicion").modal("show");
    $(".input_valid").each(function () {
        $(this).css("border", "");
    });

}
function ConsultarArticuloRequisicion() {
    var id_centro = $("#id_centro_search").val();
    var clasificacion = $("#id_clasificaciones").val();
    var requi = $("#requi_search").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarArticulosRequi",
        data: {
            centro: id_centro,
            clasificacion: clasificacion
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulos").html(response);
            $('#datatable_articulos').DataTable({
                keys: false,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                select:true,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers',
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ConsultarArticuloPorRequisicion(id_requisicion) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarArticulosPorRequi",
        data: {
            id_requisicion: id_requisicion
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_articulos_requisicion").modal("show");
            $("#div_requi_detalle_articulos").html(response);
            $('#datatable_articulos_detalles').DataTable({
                keys: false,
                ordering: false,
                paging: false,
                searching: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                select: true,
                responsive: !0
            });
            $('#datatable_articulos_detalles tbody').on('click', 'tr', function () {
                var articuloid = $(this).find('td:first').text();
                var nomarticulo = $(this).find('td:eq(1)').text();
                var requid = $(this).find('td:last').attr('id');
                var cantidad = $(this).find('td:eq(3)').text();
                requisiciond = requid;
                articolo_d = articuloid;
                ArticulosCotizacion(requid, articuloid, cantidad);
                $('.historial').show();
                $("#articulo_nombre").text(nomarticulo);

            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ArticulosCotizacion(requid, articulo, cantidad) {
    $("#articulo_prov").attr("onclick", "ConsultarRequiProveedor(" + requid + ");");
    $("#articulo_prov_hist").attr("onclick", "HistoricoCotizacion('" + articulo + "'," + requid + "," + cantidad + ");");
    hist_articulo = articulo;
    hist_requi = requid;
    hist_cantidad = cantidad;
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarArticulosCoti",
        data: {
            requid: requid,
            articulo: articulo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_cotizacion_articulo").html(response);
            $('#datatable_articulos_cotizacion').DataTable({
                keys: false,
                searching: false,
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                select: true,
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function CuentaContable(id_cuenta) {
    var cuenta = id_cuenta;
}
function ActualizarCotizacionesTable() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarArticulosCoti",
        data: {
            requid: requisiciond,
            articulo: articolo_d
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_cotizacion_articulo").html(response);
            $('#datatable_articulos_cotizacion').DataTable({
                keys: false,
                searching: false,
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                select: true,
                responsive: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function HistoricoCotizacion(articuloid, requid, cantidad) {
    $("#m_proveedor_requisicion_hist").modal("show");
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarArticulosHist",
        data: {
            id_articulo: articuloid
        },
        success: function (response) {
            $(".historial").css("diaplay", "block");
            $("#div_cotizacion_articulo_hi").html(response);
            $('#datatable_articulos_coti').DataTable({
                keys: false,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                select: true,
                responsive: false,
                pageLength: 20
            });
            SoloLecturaInputs();
            var articulos_cant = parseFloat(cantidad).toFixed(2);
            var articulo_ceros = parseFloat(articulos_cant);

            $("#cantidad_articulo_hist").text(articulos_cant);
            $("#requi_d_hist").text(requid);
            $('#datatable_articulos_coti tbody').on('click', 'tr', function () {
                $('.historial_asignar').show();
                //var requids = $(this).find('td:first').attr('id');
                //$("#articulo_cotizacion").attr("onclick", "AsignarPorHistorial(" + requids + ");");
                proveedores = $(this).find('td:eq(1)').attr('id');
                //
            });
            $('#datatable_articulos_coti tbody').on('click', 'tr', function () {
                var precio_unit = $(this).find('td:eq(3)').text();
                var cantidad_art = $("#cantidad_articulo_hist").text();

                var precio = parseFloat(precio_unit.replace(/[^\d.]/g, ''));
                var cantidad = parseFloat(cantidad_art);

                var precio_inicial = precio * cantidad;
                var descuento = ($(this).find('td:eq(5)').attr('id') / 100);
                var valor_final = precio_inicial - (precio_inicial * descuento);
                var final = valor_final.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
                $("#precio_final").text(final);
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
var proveedores = 0;
function AsignarPorHistorial(id_cotizacion, proveedors) {
    var requid = $("#requi_d_hist").text();
    var c_coti_requi = {};
    var articulo = $("#articulo_hist").text();
    c_coti_requi.id_compra_articulo_cotizacion = id_cotizacion;
    c_coti_requi.id_articulo = $("#articulo_hist").text();
    var proveedor = proveedors;
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/VerificarCotizacionVR",
        data: {
            requid: requid,
            id_cotizacion: id_cotizacion,
            articulo: articulo,
            proveedor: proveedor
        },
        success: function (result) {
            if (result == 0) {
                iziToast.warning({
                    title: 'Aviso',
                    message: '¡Ya se tiene asignada la cotizacion!',
                });
            }
            else {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../REQUISICIONES/LimiteCotizacion",
                    data: {
                        requid: requid,
                        articulo: articulo
                    },
                    success: function (result) {
                        if (result == 'True') {
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: "../REQUISICIONES/AsignarCotizacionArticulo",
                                data: {
                                    c_coti_requi: c_coti_requi,
                                    requid: requid
                                },
                                success: function (result) {
                                    iziToast.success({
                                        title: 'Exito',
                                        message: '¡Cotizacion asignada!',
                                    });
                                    $("#m_proveedor_requisicion_hist").modal("hide");
                                    $('#m_proveedor_requisicion_hist').on('hidden.bs.modal', function () {
                                        $('#m_articulos_requisicion').focus();
                                        $('#m_articulos_requisicion').css('overflow', 'auto');
                                    });;
                                },
                                error: function (xhr, status, error) {
                                    console.error(error);
                                }
                            });
                        }
                        else {
                            iziToast.warning({
                                title: 'Aviso',
                                message: '¡Solo se puede cotizar 3 veces el articulo por requisicion!',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ConsultarRequiProveedor(requid) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarArticulosInf",
        data: {
            requid: requid
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_proveedor_requisicion").modal("show");
            $("#div_requi_proveedor_articulos").html(response);
            SoloLecturaInputs();

            $("#fecha_vigencia").val(today);
            $("#tipo_moneda").val(1);
            MinimoCero();
            MinimoCeroDias();
            SiguienteDia();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function SoloLecturaInputs() {
    $(".solo_lectura").prop("readonly", true);
}
function CalcularPrecio() {
    var precio_fin_monetario = 0;
    var precio_final = 0;
    var precio_original = parseFloat($("#precio_unitario").val());
    if (precio_original > 0) {
        var descuento = $("#descuento").val();
        if (descuento > 0) {
            precio_final = precio_original - (precio_original * (descuento / 100))
        }
        else {
            precio_final = precio_original;
        }
        //precio_fin_monetario = precio_final.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });

        precio_fin_monetario = new Intl.NumberFormat('en-US', {
            minimumFractionDigits: 9,
            maximumFractionDigits: 9
        }).format(precio_final);

        $("#precio_unitario_iva").val(precio_fin_monetario);
    }
    else {
        $("#descuento").val("");
        $("#precio_unitario").focus();
        $("#precio_unitario_iva").val('0.00000000');
    }
}
function Limitador() {
    if ($("#descuento").val() > 100) {
        $("#descuento").val(100);
        CalcularPrecio();
    }
}
var validacion = true;
function Validadores() {
    var fechaVigencia = $("#fecha_vigencia").val();
    var currentDate = new Date();
    var enteredDate = new Date(fechaVigencia.replace(/-/g, '/'));
    if ($("#clave_proveedor").val() == '') {
        validacion = false;
        $("#mensaje_proveedor").text('¡Es necesario ingresar un proveedor mediante clave o nombre!');
        $("#mensaje_proveedor").css("color", "red");
    }
    if ($("#precio_unitario").val() < 0 || $("#precio_unitario").val() == '') {
        validacion = false;
        $("#mensaje_precio").text('¡Es necesario ingresar un precio mayor que 0!');
        $("#mensaje_precio").css("color", "red");
    }
    if (enteredDate < currentDate) {
        validacion = false;
        $("#mensaje_fecha").text('¡Es necesario ingresar una fecha mayor al dia de hoy!');
        $("#mensaje_fecha").css("color", "red");
    }
    //if ($("#tiempo_entrega").val() <= 0) {
    //    validacion = false;
    //    $("#mensaje_entrega").text('¡Es necesario ingresar un tiempo de entrega!');
    //    $("#mensaje_entrega").css("color", "red");
    //}
}
function ValidarTotal() {
    var fechaVigencia = $("#fecha_vigencia").val();
    var currentDate = new Date();
    var enteredDate = new Date(fechaVigencia.replace(/-/g, '/'));
    if ($("#clave_proveedor").val() != '' && $("#tiempo_entrega").val() >= 0 && enteredDate > currentDate && $("#precio_unitario").val() > 0 || $("#precio_unitario").val() != '') {
        validacion = true;
    }

    if ($("#descuento").val() == 100) {
        iziToast.warning({
            title: 'Aviso',
            message: '!Solo es valido un descuento del 0% al 99%!',
        });
        validacion = false;
    }
    Validadores();
}
function Limpiador() {
    var fechaVigencia = $("#fecha_vigencia").val();
    var currentDate = new Date();
    var enteredDate = new Date(fechaVigencia.replace(/-/g, '/'));
    if ($("#clave_proveedor").val() != '' || $("#id_proveedor").val() != '') {
        $("#mensaje_proveedor").text('');
    }
    if ($("#precio_unitario").val() > 0 || $("#precio_unitario").val() != '') {
        $("#mensaje_precio").text('');
    }
    if (enteredDate > currentDate) {
        $("#mensaje_fecha").text('');
    }
    if ($("#tiempo_entrega").val() > 0) {
        $("#mensaje_entrega").text('');
    }
}
function MinimoCero() {
    var descuentos_requi = $("#descuento").val();
    if (descuentos_requi < 0 || descuentos_requi == "") {
        $("#descuento").val("");
    }
}
function MinimoCeroDias() {
    var dias = $("#tiempo_entrega").val();
    if (dias < -1 || dias == "") {
        $("#tiempo_entrega").val("");
    }
}


function RemoverArticuloCotizacion(id_cotizacion) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/RemoverArticuloCotizacion",
        data: {
            cotizacion: id_cotizacion
        },
        success: function (response) {
            ActualizarCotizacionesTable();
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ModificarVigenciaProveedor(cotizacion) {
    var requid = $("#requi_d_hist").text();
    jsShowWindowLoad();
    $("#m_proveedor_requisicion_modif").modal("show");
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ModificarVigenciaProveedor",
        data: {
            cotizacion: cotizacion
        },
        success: function (response) {
            $("#div_cotizacion_articulo_modif").html(response);
            $("#requi_vigencia_d").text(requid);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });

}
function ActualizarVigenciaProveedor(cotizacion) {
    var fechaVigencia = $("#mod_fecha_final").val();
    var currentDate = new Date();
    var enteredDate = new Date(fechaVigencia.replace(/-/g, '/'));
    if (enteredDate > currentDate) {
        var fecha = $("#mod_fecha_final").val();
        jsShowWindowLoad();
        $("#m_proveedor_requisicion_modif").modal("show");
        $.ajax({
            type: "POST",
            async: true,
            url: "../REQUISICIONES/ActualizarVigenciaProveedor",
            data: {
                cotizacion: cotizacion,
                fecha: fecha
            },
            success: function (response) {

                $("#m_proveedor_requisicion_modif").modal("hide");
                iziToast.success({
                    title: '¡Exito!',
                    message: 'Se modifico la fecha de vigencia',
                });
                var historial_requid = $("#requi_vigencia_d").text();

                HistoricoCotizacion(hist_articulo, hist_requi, hist_cantidad);
                $("#requi_d_hist").text(historial_requid);
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'La fecha de vigencia debe ser mayor a la fecha actual',
        });
    }
}

//-------------CONFIRMACION DE COTIZACIONES DE COMPRA
function ConsultarArticulosCotizadosPendientesConfirmacion() {
    var id_clasificaciones = $("#id_clasificaciones_search").val();
    //var folio_requi = $("#folio_requi_search").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarArticulosCotizadosPendientesConfirmacion",
        data: {
            id_clasificaciones: id_clasificaciones,
            folio_requi: ""
        },
        success: function (response) {
            $("#div_cotizaciones_articulos").html("");
            $("#div_articulos_cotizados_pendientes").html(response);
            $('#datatable_articulos_cotizados_pendientes').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                scrollY: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarCotizacionesArticuloRequi(id_cotizacion_d) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarCotizacionesArticuloRequi",
        data: {
            id_cotizacion_d: id_cotizacion_d
        },
        success: function (response) {
            $("#div_cotizaciones_articulos").html(response);
            $(".divEditable").keypress(function (e) {
                if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
            });


            $('.divEditable').on('input', function () {
                var content = $(this).text();
                var sanitizedContent = content.replace(/\s+/g, '');
                $(this).text(sanitizedContent);
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function SelectCardCotizacion(id_cotizacion) {
    var card = $("#cotizacion_card_" + id_cotizacion + "");
    var radio = $("#radioCoti_" + id_cotizacion + "");
    var inputs = $(".input_coti_" + id_cotizacion + "");
    //var lbl = $("#lbl_check_" + id_cotizacion + "");

    if (radio.is(':checked')) {
        radio.prop("checked", false);
    }
    else {
        radio.prop("checked", true);
    }
    if (radio.is(':checked')) {
        card.css("background-color", "#ECF4FB");
        inputs.removeAttr("disabled");
        //lbl.css("display", "block");
    }
    else {
        card.css("background-color", "lightgray");
        inputs.attr("disabled", "true");
        //lbl.css("display", "none");
    }
}
function ValidarDiasEntrega(id_coti) {
    var input = $("#dias_entrega_" + id_coti + "");
    //if (input.val() == "") { input.val("0"); }
    if (input.val().length > 2) { input.val(""); }
}

function ValidarUnidadMedida() {
    var acepta_decimales = parseFloat($("#acepta_decimales_lbl").text());
    if (acepta_decimales == 1) {  //DECIMALES
        $(".cant_surtir_inputs").removeClass('solo-numeros');
        $(".cant_surtir_inputs").removeClass('solo-numeros-spunto');
        $(".cant_surtir_inputs").addClass('solo-numeros');
    }
    else {  //ENTEROS
        $(".cant_surtir_inputs").removeClass('solo-numeros-spunto');
        $(".cant_surtir_inputs").removeClass('solo-numeros');
        $(".cant_surtir_inputs").addClass('solo-numeros-spunto');
    }
}
function GuardarConfirmacionCotizacionArticuloRequisicion(id_requi_d, id_requi_g, nombre_articulo) {
    var id_cotizaciones_articulo = [];
    var cant_surtir = [];
    var dias_entrega = [];
    var comentarios = [];
    var confirmada = [];
    var count = 0;
    var total_cantidad = 0;
    var cantidad_solicitada = parseFloat($("#cantidad_solicitada_requi").text());

    var valid_vacios = false;
    var valid_negativos = false;
    $(".checks_cotizaciones").each(function () {
        var id_coti = $(this).attr("id").split('_')[1];
        id_cotizaciones_articulo[count] = id_coti;

        if ($(this).is(':checked')) {
            confirmada[count] = true;
            cant_surtir[count] = $("#cant_surtir_" + id_coti + "").val();
            dias_entrega[count] = $("#dias_entrega_" + id_coti + "").val();
            comentarios[count] = $("#comentarios_" + id_coti + "").val();

            if (cant_surtir[count] == "") { valid_vacios = true; }
            if (cant_surtir[count] <= 0) { valid_negativos = true; }

            if (dias_entrega[count] == "") { dias_entrega[count] = 0; }

            total_cantidad = total_cantidad + parseFloat(cant_surtir[count]);
        }
        else {
            confirmada[count] = false;
        }
        count++;
    });

    if (valid_vacios == true) {
        iziToast.warning({
            title: 'Asegurese de no dejar valores vacíos en cantidad a surtir',
            message: '',
        });
        return false;
    }
    if (valid_negativos == true) {
        iziToast.warning({
            title: 'Ingrese solo valores positivos en cantidad a surtir',
            message: '',
        });
        return false;
    }

    if (cant_surtir.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 cotización',
            message: '',
        });
        return false;
    }
    else if (total_cantidad > cantidad_solicitada) {
        iziToast.warning({
            title: 'Ingresa una cantidad menor a la requerida: ' + cantidad_solicitada + '',
            message: '',
        });
    }
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 999,
            title: 'ATENCION',
            message: '¿Está seguro de guardar la cotización de la articulo: ' + nombre_articulo + '?',
            position: 'center',
            buttons: [
                ['<button><b>Si, confirmar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../REQUISICIONES/GuardarConfirmacionCotizacionArticuloRequisicion",
                        data: {
                            id_cotizaciones_articulo: id_cotizaciones_articulo,
                            cant_surtir: cant_surtir,
                            dias_entrega: dias_entrega,
                            comentarios: comentarios,
                            confirmada: confirmada,
                            id_requi_g: id_requi_g,
                            id_requi_d: id_requi_d,
                            total_cantidad: total_cantidad
                        },
                        success: function (response) {
                            if (response == 0) {
                                iziToast.success({
                                    title: '¡Cotización guardada correctamente!',
                                    message: '',
                                });
                                //ConsultarArticulosCotizadosPendientesConfirmacion();
                                $("#tr_articulo_" + id_requi_d + "").remove();
                                $("#div_cotizaciones_articulos").html("");
                            }
                            else if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar',
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: 'Ingresa cantidades validas',
                                    message: '',
                                });
                            }
                            else if (response == -3) {
                                iziToast.warning({
                                    title: 'Asegurate de capturar los días de entrega',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            iziToast.error({
                                title: 'Ocurrió un error al guardar',
                                message: '',
                            });
                        }
                    });
                }, true],
                ['<button>No, cancelar operacion</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                }],
            ]
        });
    }
}

function CerrarCotiRequi(id_requi) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 1999,
        title: 'ATENCION',
        message: '¿Está seguro de cerrar la cotización?',
        position: 'center',
        buttons: [
            ['<button><b>Si, confirmar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../REQUISICIONES/CerrarCotiRequi",
                    data: {
                        id_requi: id_requi
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: '¡Cotización cerrada correctamente!',
                                message: '',
                            });
                            ConsultarArticulosCotizadosPendientesConfirmacion();
                            $("#div_cotizaciones_articulos").html("");
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        iziToast.error({
                            title: 'Ocurrió un error al guardar',
                            message: '',
                        });
                    }
                });
            }, true],
            ['<button>No, cancelar operacion</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

            }],
        ]
    });
}
//---------- COTIZACIONES CONFIRMADAS
function ConsultarRequisicionesConfirmadas() {
    var id_centros = $("#id_centro_search").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarRequisicionesConfirmadas",
        data: { id_centros: id_centros },
        success: function (response) {
            $("#div_requis_confirmadas").html(response);
            $('#datatable_requis_confirmadas').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                scrollY: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarCotizacionesConfirmadas(id_requi_g) {
    var btn_close_modal = $("#btn_close_form_solicitud_compra");
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarCotizacionesConfirmadas",
        data: {
            id_requi_g: id_requi_g
        },
        success: function (response) {
            if (response == "") {
                //$("#m_cotizaciones_confirmadas").modal("hide");
                $("#datatable_cotizaciones_confirmadas").html("");
                $("#div_form_solicitud_compra").html("");
                ConsultarRequisicionesConfirmadas();
                ConsultarSolicitudesCompraAgrupadas(id_requi_g);
                btn_close_modal.text("Confirmar agrupacion");
                btn_close_modal.removeClass("btn_beta_danger");
                btn_close_modal.addClass("btn_beta");
            }
            else {
                $("#div_cotizaciones_confirmadas").html(response);
                $("#m_cotizaciones_confirmadas").modal("show");
                ConsultarSolicitudesCompraAgrupadas(id_requi_g);
                btn_close_modal.text("Regresar");
                btn_close_modal.removeClass("btn_beta");
                btn_close_modal.addClass("btn_beta_danger");
                ConsultarRequisicionesConfirmadas();

                $("input[type='checkbox']").change(function () {
                    var correo = "";
                    var check = $("input[type='checkbox']:checked");
                    var importe_solicitud = 0.000000000;
                    try {
                        check.each(function () {
                            if ($(this).is(':checked')) {
                                var id_cotizacion = $(this).attr("id").split('_')[1];
                                correo = $("#correo_prov_" + id_cotizacion + "").text().trim();
                                var importe = $("#importe_" + id_cotizacion + "").text().trim();
                                importe_solicitud += parseFloat(importe);
                            }
                        });
                    } catch (e) { }
                    $("#correo_proveedor_solicitud").val(correo);
                    importe_solicitud = new Intl.NumberFormat('en-US', {
                        minimumFractionDigits: 9,
                        maximumFractionDigits: 9
                    }).format(importe_solicitud);
                    $("#importe_total_solicitud").text("Importe total: $" + importe_solicitud);
                });

            }

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//function SeleccionarCorreoProveedor(id_cotizacion) {
//    var check = $("input[type='checkbox']:checked");
//    if (check.length > 0) {
//        var correo = $("#correo_prov_" + id_cotizacion + "").text().trim();
//        $("#correo_proveedor_solicitud").val(correo);
//    }
//    else { $("#correo_proveedor_solicitud").val(""); }
//}


function EliminarCotizacionConfirmada(id_requisicion_cotizacion, id_requi_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de elminar la confirmación de cotización?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar confirmación</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../REQUISICIONES/EliminarCotizacionConfirmada",
                    data: { id_requisicion_cotizacion: id_requisicion_cotizacion },
                    success: function (response) {
                        if (response == "True") {
                            ConsultarCotizacionesConfirmadas(id_requi_g);
                            iziToast.success({
                                title: 'Confirmación eliminada correctamente!',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un problema al eliminar la confirmación',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                    }
                });
            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }],
        ],
        onClosing: function (instance, toast, closedBy) {
            console.info('Closing | closedBy: ' + closedBy);
        },
        onClosed: function (instance, toast, closedBy) {
            console.info('Closed | closedBy: ' + closedBy);
        }
    });
}

function ConsultarLugaresEntregaString() {
    var id_ubicacion_entrega_g = $("#ubicacion_entrega_solicitud").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarLugaresEntregaString",
        data: { id_ubicacion_entrega_g: id_ubicacion_entrega_g },
        success: function (response) {
            try {
                var data = $.parseJSON(response);
                var lugares = "<label style='margin:0px;'>Se recepciónará en almacén:</label>";
                lugares += "<ul>";
                for (var i = 0; i < data.length; i++) {
                    lugares += "<li><strong>-" + data[i] + "</strong></li>"
                }
                lugares += "</ul>";
                $("#div_lugares_recepcion_ubicacion").html(lugares);
            } catch (e) {
                $("#div_lugares_recepcion_ubicacion").html(response);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GeneracionSolicitudCompra(id_requi_g) {
    var correo_destino = $("#correo_proveedor_solicitud").val();
    if (correo_destino == "") {
        iziToast.warning({
            title: 'Aseguese de ingresar el campo del correo del proveedor',
            message: '',
        });
        $("#correo_proveedor_solicitud").css("border", "1px solid red");
        $("#correo_proveedor_solicitud").focus();
        return false;
    }

    var id_ubicacion_entrega = $("#ubicacion_entrega_solicitud").val();
    if (id_ubicacion_entrega == 0 || id_ubicacion_entrega == undefined) {
        iziToast.warning({
            title: 'Selecciona la ubicación de entrega',
            message: '',
        });
        $("#ubicacion_entrega_solicitud").css("border", "1px solid red");
        return false;
    }

    var nombre_solicitud = $("#nombre_solicitud").val();
    var id_cotizaciones = [];
    var count = 0;

    $(".check_agrupar").each(function () {
        if ($(this).is(':checked')) {
            id_cotizaciones[count] = $(this).attr("id").split('_')[1];
            count++;
        }
    });

    if (id_cotizaciones.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 cotización para generar la solicitud de compra',
            message: '',
        });
    }
    else if (nombre_solicitud == "") {
        iziToast.warning({
            title: 'Ingrese el nombre de la solicitud de compra',
            message: '',
        });
        $("#nombre_solicitud").css("border", "1px solid red");
        $("#nombre_solicitud").focus();
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../REQUISICIONES/GeneracionSolicitudCompra",
            data: {
                nombre_solicitud: nombre_solicitud,
                id_cotizaciones: id_cotizaciones,
                id_requi_g: id_requi_g,
                correo_proveedor: correo_destino,
                id_ubicacion_entrega: id_ubicacion_entrega
            },
            success: function (response) {
                if (response == -1) {
                    iziToast.error({
                        title: 'Ocurrió un error al guardar',
                        message: 'Contacte a desarrollo',
                    });
                }
                else if (response == 1) {
                    iziToast.warning({
                        title: 'Asegurese de que las cotizaciones sean del mismo proveedor',
                        message: '',
                    });
                }
                else if (response == 2) {
                    iziToast.warning({
                        title: 'El proveedor no tiene una cuenta asignada',
                        message: 'Favor de asignarla en Microsip',
                    });
                }
                else if (response == 3) {
                    iziToast.warning({
                        title: 'Ocurrió in error al validar el proveedor',
                        message: 'Favor de contactar a sistemas',
                    });
                }
                else {
                    $("#ubicacion_entrega_solicitud").css("border", "");
                    $("#correo_proveedor_solicitud").val("");
                    $("#nombre_solicitud").val("");
                    ConsultarCotizacionesConfirmadas(id_requi_g);
                    iziToast.success({
                        title: 'Cotizaciones agrupadas correctamente',
                        message: '',
                    });
                    $("#importe_total_solicitud").val("0.000000000");
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
        //alert("EN DESARROLLO");
    }

}
function ConsultarSolicitudesCompraAgrupadas(id_requi_g) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarSolicitudesCompraAgrupadas",
        data: {
            id_requi_g: id_requi_g
        },
        success: function (response) {
            $("#div_solicitudes_compra_generadas").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ConsultarSolicitudCompraAgrupadaDetalle(id_cotizacion_confirmada_g) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../REQUISICIONES/ConsultarSolicitudCompraAgrupadaDetalle",
        data: {
            id_cotizacion_confirmada_g: id_cotizacion_confirmada_g
        },
        success: function (response) {
            $("#div_cotizaciones_confirmadas_detalle").html(response);
            $("#m_cotizaciones_confirmadas_detalle").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function EliminarSolicitudCompra(id_cotizacion_confirmada_g, nombre_grupo, id_requi_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de elminar la solicitud: ' + nombre_grupo + '?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar agrupación</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../REQUISICIONES/EliminarSolicitudCompra",
                    data: {
                        id_cotizacion_confirmada_g: id_cotizacion_confirmada_g
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: '¡Solicitud de compra eliminada correctamente!',
                                message: '',
                            });
                            ConsultarCotizacionesConfirmadas(id_requi_g);
                            try { BuscarInformacionRequisicionUtileria(); } catch (e) { }
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al intentar eliminar',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });

            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

            }],
        ],
        onClosing: function (instance, toast, closedBy) {
            console.info('Closing | closedBy: ' + closedBy);
        },
        onClosed: function (instance, toast, closedBy) {
            console.info('Closed | closedBy: ' + closedBy);
        }
    });


}
//#endregion 



//#region ORDEN DE COMPRA (FIRMAS)
function ConsultarRequisicionesAutFinal(modo) {
    usuario_modo = modo;
    var id_cargo = $("#id_cargo_search_2").val();
    if (id_cargo == undefined || id_cargo == "") {
        iziToast.warning({
            title: 'No se detectaron centros asignados',
            message: 'Favor de contactar a desarrollo',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../REQUISICIONES/ConsultarRequisicionesAutFinal",
            data: {
                modo: modo,
                id_cargo: id_cargo
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (modo == 2) {
                    $("#div_requisiciones_por_firmar").css("display", "block");
                    $("#div_requisiciones_por_firmar").html(response);
                    $('#datatable_requisiciones_por_firmar').DataTable({
                        keys: false,
                        searching: false,
                        ordering: true,
                        paging: false,
                        dom: "Bfrtip",
                        buttons: [{
                            extend: "copy",
                            className: "btn-sm",
                            columns: ':visible'
                        }, {
                            extend: "csv",
                            className: "btn-sm"
                        }, {
                            extend: "excel",
                            className: "btn-sm"
                        }, {
                            extend: "pdf",
                            className: "btn-sm"
                        }, {
                            extend: "print",
                            className: "btn-sm"
                        }],
                        select: true,
                        responsive: false
                    });
                }
                else {
                    $("#div_requisiciones_por_firmar").html(response);
                    $('#datatable_requisiciones_por_firmar').DataTable({
                        keys: false,
                        searching: true,
                        ordering: true,
                        paging: false,
                        dom: "Bfrtip",
                        buttons: [{
                            extend: "copy",
                            className: "btn-sm",
                            columns: ':visible'
                        }, {
                            extend: "csv",
                            className: "btn-sm"
                        }, {
                            extend: "excel",
                            className: "btn-sm"
                        }, {
                            extend: "pdf",
                            className: "btn-sm"
                        }, {
                            extend: "print",
                            className: "btn-sm"
                        }],
                        select: true,
                        responsive: false
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }


}


//------ AUTORIZAR LA REQUISICION
function AutorizarRequisicionesFinal(id_requis, modo) {
    if (modo == 3) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 9000000,
            url: "../REQUISICIONES/AutorizarRequisicionesFinal",
            data: {
                id_requis: id_requis,
                modo: modo
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Requisición autorizada correctamente',
                        message: 'Orden de compra generada',
                    });
                    ConsultarRequisicionesAutFinal(modo);
                    $("#m_detalle_requi_aut_final").modal("hide");
                    //if (modo == 2) { window.location.replace("https://siib.beta.com.mx/REQUISICIONES/RequisicionesRevision"); }
                    if (modo == 2 || modo == 3) {
                        $(".div_hide_master").css("display", "none");
                        $("#div_requisiciones_detalle").html("");
                        $("#requis_master").css("display", "inline-block");
                    }
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al generar la autorización',
                        message: '',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.question({
            timeout: false,
            close: false,
            overlay: false,
            drag: false,
            displayMode: 'once',
            id: 'question',
            zindex: 9999999,
            title: 'ATENCIÓN',
            message: '¿Está seguro de autorizar la requisición?',
            position: 'center',
            buttons: [
                ['<button><b>Si, autorizar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../REQUISICIONES/AutorizarRequisicionesFinal",
                        data: {
                            id_requis: id_requis,
                            modo: modo
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Requisición autorizada correctamente',
                                    message: 'Orden de compra generada',
                                });
                                ConsultarRequisicionesAutFinal(modo);
                                $("#m_detalle_requi_aut_final").modal("hide");
                                //if (modo == 2) { window.location.replace("https://siib.beta.com.mx/REQUISICIONES/RequisicionesRevision"); }
                                if (modo == 2 || modo == 3 || modo == 4) {
                                    $(".div_hide_master").css("display", "none");
                                    $("#div_requisiciones_detalle").html("");
                                    $("#requis_master").css("display", "inline-block");
                                }
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error al generar la autorización',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }, true],
                ['<button>No, regresar</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }],
            ],
            onClosing: function (instance, toast, closedBy) {
                console.info('Closing | closedBy: ' + closedBy);
            },
            onClosed: function (instance, toast, closedBy) {
                console.info('Closed | closedBy: ' + closedBy);
            }
        });
    }
}
function AutorizarRequisicionesFinalMaster(modo) {
    var counts = 0;
    var id_requis = [];

    var checks = $("input[type='checkbox']:checked");
    checks.each(function () {
        id_requis[counts] = ($(this).attr('id'));
        counts++;
    });
    if (id_requis.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../REQUISICIONES/AutorizarRequisicionesFinal",
            data: {
                id_requis: id_requis,
                modo: modo
            },
            success: function (response) {
                if (response == 0) {
                    iziToast.success({
                        title: 'Requisición autorizada correctamente',
                        message: 'Orden de compra generada',
                    });
                    //ConsultarRequisicionesAutFinal(modo);
                    for (var i = 0; i < id_requis.length; i++) {
                        $("#TrRequi_" + id_requis[i] + "").remove();
                    }
                    CalcularTotalRequis();
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al generar la autorización',
                        message: '',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar una requisicion',
        });
    }
}



//------ RECHAZAR LA REQUISICION CON UNA JUSTIFICACIÓN
function ShowJustificacionRecotizarAutFinal(id_requisicion, modo, concepto_requi) {
    $("justificacion_recotizar_aut_final").val("");
    $("#concepto_recotizar_aut_final").val(concepto_requi);
    $("#btn_rechazar_aut_final").attr("onclick", "RecotizarRequisicionCancelarAutFinal(" + id_requisicion + ", " + modo + ");");
    $("#m_recotizar_aut_final").modal("show");
}
function RecotizarRequisicionCancelarAutFinal(id_requisicion, modo) {
    var justificacion = $("#justificacion_recotizar_aut_final").val();
    if (justificacion == "" || justificacion.length <= 10) {
        iziToast.error({
            title: 'Ingresa una justificación mayor a 10 caracteres',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/RecotizarRequisicionCancelarAutFinal",
        data: {
            id_requisicion: id_requisicion,
            justificacion: justificacion
        },
        success: function (response) {
            if (response == 0) {
                iziToast.success({
                    title: 'Requisicion rechazada correctamente',
                    message: '',
                });
                ConsultarRequisicionesAutFinal(modo);
                $("#m_recotizar_aut_final").modal("hide");
                $("#m_rechazar_aut_final").modal("hide");
                $("#m_detalle_requi_aut_final").modal("hide");
                $("#justificacion_recotizar_aut_final").val("");
                if (modo == 2) { windows.location.replace("https://siib.beta.com.mx//REQUISICIONES/RequisicionesRevision"); }
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al rechazar la requisición',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


//-------SOLICITIUD DE AUTORIZACIÓN A DIRECCIÓN (USUARIO MASTER)
function ShowJustificacionSolicitaAutFinal(id_requisicion, modo, concepto_requi) {
    $("justificacion_solicita_aut_final").val("");
    $("#concepto_solicita_aut_final").val(concepto_requi);
    $("#btn_solicita_aut_final_enviar").attr("onclick", "SolicitarAutorizacionDireccion(" + id_requisicion + ", " + modo + ");");
    $("#m_solicita_aut_final").modal("show");
}
function SolicitarAutorizacionDireccion(id_requisicion, modo) {
    var justificacion = $("#justificacion_solicita_aut_final").val();
    if (justificacion == "") {
        iziToast.error({
            title: 'Ingrese una justificación',
            message: '',
        });
    }
    else if (justificacion.length <= 20) {
        iziToast.error({
            title: 'Ingrese una justificación de mas de 20 caracteres',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../REQUISICIONES/SolicitarAutorizacionDireccion",
            data: {
                id_requisicion: id_requisicion,
                justificacion: justificacion,
                modo: modo
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Solicitud de autorización enviada correctamente',
                        message: '',
                    });
                    ConsultarRequisicionesAutFinal(modo);
                    $("#m_solicita_aut_final").modal('hide');
                    $("#m_detalle_requi_aut_final").modal("hide");
                    $("#justificacion_solicita_aut_final").val("");
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al generar la autorización',
                        message: '',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}




//#endregion