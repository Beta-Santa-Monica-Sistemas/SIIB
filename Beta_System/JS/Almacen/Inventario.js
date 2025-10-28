$('input[type="date"]').val(today);

function ConsultarTiposArticulosSelect() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000000,
        url: "../CATALOGOS/ConsultarTiposArticulosSelect",
        data: { },
        success: function (response) {
            jsRemoveWindowLoad();
            $(".id_tipo_articulos_select").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#region --------------- CAPTURA DE MOVIMIENTOS DE INVENTARIO
function ConsultarMovimientosInventarioTable() {
    var id_almacen_g = $("#id_almacen_g_movs").val();
    var fecha_inicio = $("#fecha_inicio_admin").val();
    var fecha_fin = $("#fecha_fin_admin").val();
    var tipo_movimiento = $("#id_almacen_g_tmovs").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarMovimientosInventarioTable",
        data: {
            id_almacen_g: id_almacen_g,
            tipo_movimiento: tipo_movimiento,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_movimientos_inventario").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_movimientos_inventario").html(response);
            $('#tabla_movimientos_inventario').DataTable({
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
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function CambioMensajeCaptura() {
    if ($("#mensaje_captura").text() == "Folio factura:") {
        $("#mensaje_captura").text("Folio remision:");
    }
    else {
        $("#mensaje_captura").text("Folio factura:");
    }
}

function MovimientoInventario() {
    var movimiento = $("#id_tipo_mov_cap_mov").val();
    if (movimiento == '1') {
        $("#div_folio_captura").css("display", "block");
        $("#div_remision_captura").css("display", "block")
        $("#div_movimiento_captura").attr("class", "col-md-3");
        $("#remision").prop("checked", false);
        $("#factura_folio").val('');
        $("#mensaje_captura").text("Folio factura:");

        $("#div_destino").css("display", "none");
        $("#accion_movimiento").attr("onclick", "RegistrarCapturaMovimientoInventario();");
        //$("#almacen_destino_titulo").css("display", "none");
        //$("#id_almacen_g_cap_mov").css("display", "none");
    }
    else {
        //$("#almacen_destino_titulo").css("display", "block");
        //$("#id_almacen_g_cap_mov").css("display", "block");
        $("#div_folio_captura").css("display", "none");
        $("#div_remision_captura").css("display", "none");
        $("#div_movimiento_captura").attr("class", "col-md-4");

        $("#div_destino").css("display", "block");
        $("#accion_movimiento").attr("onclick", "RegistrarTraspasoInventario();");
    }
}
//


function ConsultarArticulosAlmacenCapturaMov() {
    var id_almacen_g = $("#id_almacen_g_cap_mov").val();
    var id_tipo_articulo = $("#id_tipo_articulo_cap_mov").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000000,
        url: "../ALMACEN/ConsultarArticulosAlmacenCapturaMov",
        data: {
            id_almacen_g: id_almacen_g,
            id_tipo_articulo: id_tipo_articulo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_articulos_cap_inv").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_articulos_almacen_captura_mov").html(response);
            $('#datatable_articulos_cap_inv').DataTable({
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function RegistrarTraspasoInventario() {
    var id_almacen_origen = $("#id_almacen_g_cap_mov").val();
    var id_almacen_destino = $("#id_almacen_g_cap_mov_destino").val();

    var id_tipo_mov = $("#id_tipo_mov_cap_mov").val();

    var captura = $(".articulo_cantidad_captura_mov");
    var id_articulos = [];
    var cantidades = [];
    var obs = [];
    var count = 0;
    var valid_existencia = true;
    captura.each(function () {
        var valor = $(this).val();
        var id_articulo = $(this).attr("id").split('_')[1];
        if (valor != "" && valor > 0 && valor != undefined) {
            var existencia = parseFloat($("#exist_" + id_articulo + "").text());

            if (parseFloat(valor) > existencia) { valid_existencia = false; }

            id_articulos[count] = id_articulo;
            cantidades[count] = valor;
            obs[count] = $("#obs_" + id_articulo + "").val();
            count++;
        }
    });

    if (cantidades.length <= 0) {
        iziToast.warning({
            title: 'Ingresa al menos 1 articulo con cantidad mayor a 0',
            message: '',
        });
    }
    else if (id_almacen_destino == id_almacen_origen) {
        iziToast.warning({
            title: 'No es posible hacer traspasos al mismo almacén',
            message: '',
        });
    }
    else if (valid_existencia == false) {
        iziToast.warning({
            title: 'Asegurese de no traspasar mas de la existencia del articulo',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ALMACEN/RegistrarTraspasoInventario",
            data: {
                id_almacen_g: id_almacen_origen,
                id_almacen_destino: id_almacen_destino,
                id_tipo_mov: id_tipo_mov,
                id_articulos: id_articulos,
                cantidades: cantidades,
                obs: obs
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    $("#m_registrar_movimiento").modal("hide");
                    iziToast.success({
                        title: 'Movimiento registrado correctamente!',
                        message: '',
                    });
                    ConsultarMovimientosInventarioTable();
                }
                else {
                    iziToast.warning({
                        title: 'Ocurrió un error al guardar el movimiento',
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
}

function ConsultarDetalleMovimientoInventario(id_mov_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarDetalleMovimientoInventario",
        data: { id_mov_g: id_mov_g },
        success: function (response) {
            $("#m_detalle_movimiento").modal("show");
            jsRemoveWindowLoad();
            //try {
            //    $("#tabla_movimientos_inventario_detalle").dataTable().fnDestroy();
            //} catch (e) { }
            $("#div_movimientos_inventario_detalle").html(response);
            $('#tabla_movimientos_inventario_detalle').DataTable({
                keys: false,
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
                responsive: false
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarRecepcionTraspaso(id_almacen_destino) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ValidarRecepcionTraspaso",
        data: { id_almacen_destino: id_almacen_destino },
        success: function (response) {
            if (response == "True") { $("#btn_recibir_traspaso").css("display", "inline"); }
            else { $("#btn_recibir_traspaso").css("display", "none"); }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function RecibirTraspasoInventario(id_mov_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/RecibirTraspasoInventario",
        data: { id_mov_g: id_mov_g },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Traspaso recibido correctamente!',
                    message: '',
                });
                ConsultarDetalleMovimientoInventario(id_mov_g);
                ConsultarMovimientosInventarioTable();
                ConsultarArticulosAlmacenCapturaMov();
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al recibir el traspaso',
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
//#endregion



//#region ---------------- CAPTURA DE INVENTARIO
function ConsultarCapturaInventarioDiferencias() {
    var id_almacen_g = $("#id_almacen_g_inv_cap").val();
    var id_tipo_inventario = $("#id_tipo_inventario_cap_general").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout:90000000,
        url: "../ALMACEN/ConsultarCapturaInventarioDiferencias",
        data: {
            id_almacen_g: id_almacen_g,
            id_tipo_inventario: id_tipo_inventario
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_inventario_fisico").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_captura_almacen").html(response);
            $('#datatable_inventario_fisico').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
            $(".divEditable").keypress(function (e) {
                if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function CalcularDiferenciaCapInventario(id_articulo, valid_inv_inicial) {
    var sistema = $("#sistema_cap_" + id_articulo + "").text();
    var fisico = $("#fisico_cap_" + id_articulo + "").text();
    var diferencia = parseFloat(fisico) - parseFloat(sistema);

    if (isNaN(diferencia)) { diferencia = 0; }

    if (valid_inv_inicial == false) {
        $("#diferencia_cap_" + id_articulo + "").text(diferencia.toFixed(2));

    }
}

function RegistrarCapturaInventario() {

    var count = 0;
    var id_articulos = [];
    var cantidades_sistema = [];
    var cantidades_fisico = [];
    var diferencias = [];

    var id_tipo_inventario = $("#id_tipo_inventario_cap_general").val();
    var id_almacen_g = $("#id_almacen_g_inv_cap").val();
    //var articulos = $(".articulos_cap_inventario_general");

    var tabla = $('#datatable_inventario_fisico').DataTable();
    tabla.rows().every(function () {
        var data = this.data(); // Obtener los datos de la fila
        var node = this.node(); // Obtener el nodo de la fila
        var input = $(node).find(".articulos_cap_inventario_general");

        var input_sis = $(node).find(".sistema_cap_inventario_general");
        var input_fis = $(node).find(".articulo_cantidad_fisica_cap");
        var input_dif = $(node).find(".diferencias_cap_inventario_general");

        var id = input.attr('id');
        id_articulos[count] = id;

        var cantidad_cap = input_sis.text();
        if (cantidad_cap == "" || isNaN(cantidad_cap) || cantidad_cap == undefined) { cantidad_cap = 0 }
        cantidades_sistema[count] = cantidad_cap;

        var cantidad_fis = input_fis.text();
        if (cantidad_fis == "" || isNaN(cantidad_fis || cantidad_fis == undefined)) { cantidad_fis = 0 }
        cantidades_fisico[count] = cantidad_fis;

        var cantidad_dif = input_dif.text();
        if (cantidad_dif == "" || isNaN(cantidad_dif || cantidad_dif == undefined)) { cantidad_dif = 0; }
        diferencias[count] = parseFloat(cantidad_dif);
        count++;
    });

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ATENCIÓN',
        message: 'Está seguro de guardar el inventario?',
        position: 'center',
        buttons: [
            ['<button><b>Si, guardar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    timeout: 90000000,
                    url: "../ALMACEN/RegistrarCapturaInventario",
                    data: {
                        id_almacen_g: id_almacen_g,
                        id_articulos: id_articulos,
                        cantidades_sistema: cantidades_sistema,
                        cantidades_fisico: cantidades_fisico,
                        diferencias: diferencias,
                        obs: $("#obs_cap_inventario").val(),
                        id_tipo_inventario: id_tipo_inventario
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == 0) {
                            iziToast.success({
                                title: 'Inventario capturado correctamente!',
                                message: '',
                            });
                            ConsultarCapturaInventarioDiferencias();
                        }
                        else {
                            iziToast.warning({
                                title: 'Ocurrió un error al capturar el inventario',
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
            ['<button>NO</button>', function (instance, toast) {
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


//#region ---------------- CONSULTAR INVENTARIO
function ConsultarInventariosCapturados() {
    var id_almacen_g = $("#id_almacen_g_consulta_inv").val();
    var fecha_inicio = $("#fecha_inicio_inventarios").val();
    var fecha_fin = $("#fecha_fin_inventarios").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarInventariosCapturados",
        data: {
            id_almacen_g: id_almacen_g,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_inventarios_capturados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventarios_capturados").html(response);
            $('#tabla_inventarios_capturados').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function ConsultarDetalleCapturaInventario(id_inventario_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarDetalleCapturaInventario",
        data: { id_inventario_g: id_inventario_g },
        success: function (response) {
            $("#m_detalle_inventario_cap").modal("show");
            jsRemoveWindowLoad();
            try {
                $("#tabla_inventario_capturado_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_capturado_detalle").html(response);
            $('#tabla_inventario_capturado_detalle').DataTable({
                keys: false,
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
                responsive: false,
                select: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarInventarioFecha() {
    var id_almacen_g = $("#id_almacen_g_inv_cons").val();
    var id_tipo_articulos = $("#id_tipo_articulos_inventario_cons").val();
    var fecha = $("#fecha_inventario_cons").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9999999,
        url: "../ALMACEN/ConsultarInventarioFecha",
        data: {
            id_almacen_g: id_almacen_g,
            id_tipo_articulos: id_tipo_articulos,
            fecha: fecha
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_inventario_fecha_cons").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_fecha_cons").html(response);
            $('#tabla_inventario_fecha_cons').DataTable({
                keys: false,
                ordering: false,
                paging: false,
                select: true,
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
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function ConsultarDiferenciasInventarios() {
    var id_almacen = $("#id_almacen_g_dif").val();
    var mes_inicio = $("#mes_inicio_dif").val();
    var mes_fin = $("#mes_fin_dif").val();
    var id_articulos_tipo = $("#id_articulos_tipo_dif").val();
    if (mes_inicio == "" || mes_fin == "") {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Ingrese los meses de consulta',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000000,
        url: "../ALMACEN/ConsultarDiferenciasInventarios",
        data: {
            id_almacen: id_almacen,
            mes_inicio: mes_inicio,
            mes_fin: mes_fin,
            id_articulos_tipo: id_articulos_tipo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_inventario_diferencias").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_diferencias").html(response);
            $('#datatable_inventario_diferencias').DataTable({
                keys: false,
                select: true,
                ordering: false,
                paging: true,
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
                responsive: false,
                pageLength: 50,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion


//#region --------RECPECION DE ORDENES DE COMPRA

//#region RECEPCION ALMACEN ALMACEN
function ConsultarOrdenEspecificaEntregar() {
    var idorden = $("#id_orden_compra_buscar").val();
    var almacen = $("#id_almacen_recepcion").val();


    if (idorden.length > 0 || idorden > 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../ALMACEN/ConsultarOrdenEspecificaEntregar",
            data: {
                idorden: idorden,
                almacen: almacen
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#tabla_orden_g").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_ordenes_recepcion").html(response);
                $('#tabla_orden_g').DataTable({
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: !0,
                    scrollY: false
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Es necesario ingresar el numero de orden',
        });
    }
}

function RegistrarCapturaMovimientoInventarioRecepcion(modo, orden_g) {
    var id_almacen_recepcion = $("#id_almacen_recepcion_detalle").val();
    var counts = 0;
    var idordend = [];
    var cantidades = [];
    var id_articulos = [];
    var entregados = [];
    var cantidad = false;
    $(".orden_articuloD").each(function () {
        idordend[counts] = $(this).attr("id"); counts++;
    });
    counts = 0;
    $(".check_entrega_articulo").each(function () {
        if ($(this).prop("checked") == true) {
            entregados[counts] = true;
            cantidad = true;
        }
        else {
            entregados[counts] = false;
        }
        counts++;
    });
    counts = 0;
    $(".orden_articulo").each(function () {
        id_articulos[counts] = $(this).attr("id").split('_')[0]; counts++;
    });
    counts = 0;
    $(".orden_recibido").each(function () {
        cantidades[counts] = $(this).val(); counts++;
    });
    var validacion = true;
    for (var i = 0; i < idordend.length; i++) {
        if (cantidades[i] == 0 && entregados[i] == true || cantidades[i] == "" && entregados[i] == true || cantidades[i] < 0 && entregados[i] == true) { validacion = false; }
    }
    var factura_folio = $("#factura_folio").val();
    var remision = false;
    if ($("#remision_factura").prop("checked") == true) {
        remision = true;
    }

    if (factura_folio != "" || factura_folio != undefined) {
        if (cantidad == true && validacion == true) {
            if (id_almacen_recepcion == 0 || id_almacen_recepcion == undefined) {
                iziToast.error({
                    title: 'Almacén no encontrado',
                    message: 'Contacte a desarrollo para configurar la ubicación a este almácén',
                });
            }
            else {
                jsShowWindowLoad();
                if (factura_folio.trim() != "" && factura_folio.length > 2) {
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeput:9000000,
                        url: "../ALMACEN/RegistrarCapturaMovimientoInventarioRecepcion",
                        data: {
                            modo: modo,
                            id_almacen_recepcion: id_almacen_recepcion,
                            idordend: idordend,
                            entregados: entregados,
                            cantidades: cantidades,
                            id_articulos: id_articulos,
                            orden_g: orden_g,
                            factura_folio: factura_folio,
                            remision: remision
                        },
                        success: function (response) {
                            if (response == "True") {
                                ConsultarArticulosAlmacenCapturaMov();

                                $("#m_registrar_movimiento").modal("hide");
                                iziToast.success({
                                    title: 'Exito',
                                    message: '¡Movimiento registrado correctamente!',
                                });
                                $("#factura_folio").css("border-color", "");
                                $("#remision_factura").prop("checked", false);
                                $(".check_entrega_articulo").prop("checked", false);
                                $("#factura_folio").val('');

                            }
                            else {
                                iziToast.warning({
                                    title: 'Aviso',
                                    message: '¡Ocurrió un error al guardar el movimiento!',
                                });

                            }
                            ConsultarOrdenPorEntregar();
                            $("#m_detalle_recepcion").modal("hide");
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
                        message: 'Favor de ingresar un folio valido',
                    });
                    $("#factura_folio").css("border-color", "red");
                    jsRemoveWindowLoad();
                }
            }

            
        }
        else {
            iziToast.warning({
                title: '¡No se puede recibir 0!',
                message: '',
            });
            jsRemoveWindowLoad();
        }
    }
    else {
        iziToast.warning({
            title: 'Ingresa el folio o remisión',
            message: '',
        });
    }
}

function RegistrarCapturaMovimientoInventario() {
    var counts = 0;
    var cantidades = [];
    var id_articulos = [];

    var id_almacen_g = $("#id_almacen_g_cap_mov").val();
    $(".orden_articulo").each(function () {
        id_articulos[counts] = $(this).attr("id"); counts++;
    });
    counts = 0;
    $(".orden_recibido").each(function () {
        cantidades[counts] = $(this).val(); counts++;
    });

    var factura_folio = $("#factura_folio").val();
    var remision = false;
    if ($("#remision").prop("checked") == true) {
        remision = true;
    }

    if (factura_folio != " " && factura_folio != null & factura_folio.length > 4) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../ALMACEN/RegistrarCapturaMovimientoInventario",
            data: {
                id_almacen_g: id_almacen_g,
                id_articulos: id_articulos,
                cantidades: cantidades,
                factura_folio: factura_folio,
                remision: remision
            },
            success: function (response) {
                jsRemoveWindowLoad();
                ConsultarArticulosAlmacenCapturaMov();
                if (response == "True") {
                    $("#m_registrar_movimiento").modal("hide");
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Movimiento registrado correctamente!',
                    });
                    $("#factura_folio").css("border-color", "");
                    $("#remision_factura").prop("checked", false);
                    $("#factura_folio").val('');
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: '¡Ocurrió un error al guardar el movimiento!',
                    });

                }
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
            message: 'Favor de ingresar un folio valido',
        });
        $("#factura_folio").css("border-color", "red");
    }
}
//#endregion -----------------------


//#region PRECARGA

function ConsultarPrecargasRealizadas(modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../COMPRAS/ConsultarPrecargasRealizadas",
        data: {
            modo: modo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_precargas_realizadas").html(response);
            //try {
            //    $("#datatable_precargas_realizadas").dataTable().fnDestroy();
            //} catch (e) { }
            //$("#div_precargas_realizadas").html(response);
            //$('#datatable_precargas_realizadas').DataTable({
            //    keys: false,
            //    select: true,
            //    ordering: false,
            //    paging: false,
            //    dom: "Bfrtip",
            //    buttons: [],
            //    responsive: false
            //});
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GenerarFormatoPrecargas() {
    var id_precargas = [];
    var count = 0;
    $("input[name='checks_precargas_g']:checked").each(function () {
        id_precargas[count] = $(this).val();
        count++;
    });
    if (id_precargas.length == 0) {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Seleccione al menos 1 precarga para generar el formato',
        });
        return;
    }
    jsShowWindowLoad();
    fetch('../COMPRAS/ConsultarPrecargas', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ id_precargas: id_precargas })
    }).then(response => response.blob())
        .then(blob => {
            jsRemoveWindowLoad();
            var url = URL.createObjectURL(blob);
            window.open(url, '_blank');
        }).catch(error => console.error("Error al generar PDF:", error));
}

function ConsultarPecargasRecepcionesHistorial() {
    var fecha_inicio = $("#fecha_i_recepciones_historial").val();
    var fecha_fin = $("#fecha_f_recepciones_historial").val();
    if (fecha_inicio == '' || fecha_fin == '') {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Asegurese de que las fechas sean válidas',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../COMPRAS/ConsultarPecargasRecepcionesHistorial",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_precargas_historial").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_precargas_historial").html(response);
            $('#datatable_precargas_historial').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
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
                responsive: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function RegistrarCapturaMovimientoInventarioPrecarga() {
    var counts = 0;
    var idordend = [];
    var cantidades = [];
    var id_articulos = [];
    var compra = [];
    var entrega = [];
    var validacion = true;
    var seleccion = false;
    var entregados = [];
    var comentario = $("#comentario_precarga").val();
    var id_empleado = $("#id_empleado_repartidor").val();
    if (id_empleado == '' || id_empleado == undefined) {
        iziToast.warning({
            title: 'Selecciona el empleado encargado de llevar el envío',
            message: '',
        });
        return;
    }
    var modo = $("#modo_orden").text();
    $(".check_entrega_articulo").each(function () {
        if ($(this).prop("checked") == true) {
            entregados[counts] = true;
            seleccion = true;
        }
        else {
            entregados[counts] = false;
        }
        counts++;
    });
    counts = 0;

    var idordeng = $("#id_orden_compra_buscar").val();
    $(".orden_articuloD").each(function () {
        idordend[counts] = $(this).attr("id"); counts++;
    });
    counts = 0;
    $(".orden_recibido").each(function () {
        cantidades[counts] = $(this).val(); counts++;
    });
    counts = 0;
    $(".orden_compra").each(function () {
        compra[counts] = $(this).text(); counts++;
    });
    counts = 0;
    $(".orden_entrega").each(function () {
        entrega[counts] = $(this).text(); counts++;
    });

    for (var i = 0; i < cantidades.length; i++) {
        if (cantidades[i] == 0 && entregados[i] == true || cantidades[i] == "" && entregados[i] == true || cantidades[i] < 0 && entregados[i] == true) { validacion = false; }
    }
    if (seleccion == true) {
        if (validacion == true) {
            jsShowWindowLoad();
            $.ajax({
                type: "POST",
                async: false,
                url: "../COMPRAS/RegistrarCapturaMovimientoInventarioPrecarga",
                data: {
                    idordeng: idordeng,
                    idordend: idordend,
                    cantidades: cantidades,
                    compra: compra,
                    entrega: entrega,
                    modo: modo,
                    id_empleado: id_empleado,
                    comentario: comentario
                },
                success: function (response) {
                    jsRemoveWindowLoad();
                    if (response == 1) {
                        $("#m_registrar_movimiento").modal("hide");
                        iziToast.success({
                            title: '¡Movimiento registrado correctamente!',
                            message: '',
                        });
                    }
                    if (response == 2) {
                        iziToast.warning({
                            title: '¡Se registro el maximo de entrega!',
                            message: '',
                        });
                    }
                    if (response == 3) {
                        iziToast.warning({
                            title: '¡Ocurrió un error al guardar el movimiento!',
                            message: '',
                        });
                    }
                    ConsultarOrdenEspecificaPrecarga();
                    ConsultarPrecargasRealizadas(1);
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                }
            });
        }
        else {
            iziToast.warning({
                title: '¡No se puede precargar 0!',
                message: '',
            });
        }
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Es necesario seleccionar un articulo',
        });
    }
}


function EliminarPrecarga(id_precarga_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../COMPRAS/EliminarPrecarga",
        data: { id_precarga_g: id_precarga_g },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == 0) {
                iziToast.success({
                    title: 'Precarga eliminada correctamente',
                    message: '',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error el eliminar la precarga',
                    message: '',
                });
            }
            ConsultarPrecargasRealizadas(1);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function ConsultarOrdenEspecificaPrecarga() {
    var idorden = $("#id_orden_compra_buscar").val();
    if (idorden.length > 0 || idorden > 0 || idorden.trim() != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../COMPRAS/ConsultarOrdenEspecificaPrecarga",
            data: {
                idorden: idorden
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    if ($.fn.DataTable.isDataTable("#datatable_precargas")) {
                        $("#datatable_precargas").DataTable().destroy();
                    }
                } catch (e) { }

                // Update the HTML content
                $("#div_ordenes_precarga").html(response);

                // Initialize DataTables
                $('#datatable_precargas').DataTable({
                    keys: false,
                    select: true,
                    ordering: false,
                    searching: false,
                    paging: false,
                    dom: "Bfrtip",
                    buttons: [],
                    responsive: false
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Es necesario ingresar el numero de orden',
        });
    }
}

function ValidarEntregaArticuloPrecarga(id_solicitud_d) {
    var cant_solicita = $("#cant_solicita_" + id_solicitud_d + "").text();
    var cant_entrega = $("#cant_entrega_" + id_solicitud_d + "").text();
    var cant_precarga = $("#cant_precarga_" + id_solicitud_d + "").text();
    var cant_recibe = $("#cant_recibido_" + id_solicitud_d + "").val();
    var canti_recib = $("#cant_recibido_" + id_solicitud_d + "");


    var total = parseFloat(cant_entrega) + parseFloat(cant_recibe);
    var diferencia = parseFloat(cant_solicita) - parseFloat(cant_entrega);
    var nada = 0;
    if (total > cant_solicita) {
        canti_recib.val(diferencia.toString().replace(/\n/g, ' '));
    }
    else if (cant_recibe == "") {
        /*canti_recib.text(nada.toString().replace(/\n/g, ' '));*/
    }

}

function ValidarEntregaArticuloEntrega(id_solicitud_d, modo) {
    if (modo == 1) {
        //PRECARGA
        var cant_precarga = $("#cant_precarga_" + id_solicitud_d + "").text();
        var cant_recibe = $("#cant_recibido_" + id_solicitud_d + "").val();
        var canti_recib = $("#cant_recibido_" + id_solicitud_d + "");
        var nada = 0;
        var cantidad = parseFloat(cant_precarga);

        if (parseFloat(cant_recibe) > parseFloat(cant_precarga)) {
            canti_recib.val(cantidad);
        }
        else if (cant_recibe == "") {
            /*canti_recib.text(nada.toString().replace(/\n/g, ' '));*/
        }
    }
    else {
        //DIRECTO
        var cant_precarga = $("#cant_pendiente_" + id_solicitud_d + "").text();
        var cant_recibe = $("#cant_recibido_" + id_solicitud_d + "").val();
        var canti_recib = $("#cant_recibido_" + id_solicitud_d + "");
        var nada = 0;
        var cantidad = parseFloat(cant_precarga);

        if (parseFloat(cant_recibe) > parseFloat(cant_precarga)) {
            canti_recib.val(cantidad);
        }
        else if (cant_recibe == "") {
            /*canti_recib.text(nada.toString().replace(/\n/g, ' '));*/
        }
    }

}

function ActivarEntrega(id_compras_orden_d, modo) {
    var check = $("#check_" + id_compras_orden_d + "");
    var cant_recibido = $("#cant_recibido_" + id_compras_orden_d + "");
    var precarga = $("#cant_precarga_" + id_compras_orden_d + "").text();
    var pendiente = $("#cant_pendiente_" + id_compras_orden_d + "").text();

    if (check.is(":checked")) {
        //cant_recibido.attr("contenteditable", "true");
        cant_recibido.prop("disabled", false);
        cant_recibido.addClass("divEditable");
        if (modo == 1) {
            cant_recibido.val(pendiente);
        }
        else {
            cant_recibido.val(precarga);
        }
        cant_recibido.focus();
    }
    else {
        cant_recibido.prop("disabled", true);
        cant_recibido.val("");
    }
}

function ConsultarOrdenRecepcionPendiente() {
    var almacen = $("#id_almacen_recepcion_pendiente").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALMACEN/ConsultarOrdenRecepcionPendiente",
        data: {
            almacen: almacen
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_orden_g_pendiente_reporte").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_recepcion_pendiente").html(response);
            $('#tabla_orden_g_pendiente_reporte').DataTable({
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
                responsive: false,
                scrollY: "500px",
                select: true,
                keys: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarOrdenPorEntregar() {
    var id_almacen = $("#id_almacen_recepcion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../ALMACEN/ConsultarOrdenPorEntregar",
        data: { id_almacen: id_almacen },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_orden_g_pendiente").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_recepcion").html(response);
            $('#tabla_orden_g_pendiente').DataTable({
                ordering: false,
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
                responsive: false,
                select: true,
                keys: false,
                searching: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function OrdenRecepcionar(id_orden_g, id_almacen) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/OrdenRecepcionar",
        data: {
            id_orden_g: id_orden_g,
            id_almacen: id_almacen
        },
        success: function (response) {
            $("#m_detalle_recepcion").modal("show");
            jsRemoveWindowLoad();
            try {
                $("#tabla_recepcion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_recepcion").html(response);
            $('#tabla_recepcion').DataTable({
                keys: false,
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                select: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}




//#endregion



//#endregion


//#region --------- INVENTARIO MINIMO
function ConsultarInventarioArticulos() {
    var id_almacen_g = $("#id_almacen_g_inv_minimo").val();
    var id_tipo_inventario = $("#id_tipo_inventario_minimo").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarInventarioArticulos",
        data: {
            id_almacen_g: id_almacen_g,
            id_tipo_inventario: id_tipo_inventario
        },
        success: function (response) {
            try {
                $("#tabla_articulos_minimo").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_minimo").html(response);
            $('#tabla_articulos_minimo').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
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

function CalcularPromedio(id_articulo) {
    var cantidadMinima = parseFloat($("#articulomin_" + id_articulo + "").val());
    if (isNaN(cantidadMinima)) {
        cantidadMinima = 0;
    }
    var cantidadMaxima = parseFloat($("#articulomax_" + id_articulo + "").val());
    if (isNaN(cantidadMaxima)) {
        cantidadMaxima = 0;
    }
    if (cantidadMaxima == 0 && cantidadMinima == 0) {
        $("#articuloprom_" + id_articulo + "").val('0');
        $("#articulomin_" + id_articulo).css("border", "");
        $("#articulomax_" + id_articulo).css("border", "");
    }
    else {
        if (cantidadMinima < cantidadMaxima) {
            var cantidadPromedio = (cantidadMinima + cantidadMaxima) / 2;
            cantidadPromedio = Math.round(cantidadPromedio);
            if (isNaN(cantidadMaxima) || isNaN(cantidadMinima)) {
                $("#articuloprom_" + id_articulo + "").val('0');
                $("#articuloprom_" + id_articulo + "").css("color", "black")

                $("#articulomin_" + id_articulo).css("border", "");
                $("#articulomax_" + id_articulo).css("border", "");
            }
            else {
                $("#articuloprom_" + id_articulo + "").val(cantidadPromedio);
                $("#articuloprom_" + id_articulo + "").css("color", "black")
                $("#articulomin_" + id_articulo).css("border", "");
                $("#articulomax_" + id_articulo).css("border", "");
            }
        }
        else {
            $("#articuloprom_" + id_articulo + "").val('FUERA DE RANGO');
            $("#articuloprom_" + id_articulo + "").css("color", "red");
            $("#articulomin_" + id_articulo).css("border", "1px solid red");
            $("#articulomax_" + id_articulo).css("border", "1px solid red");
        }
    }
}

function CalcularMaximo(id_articulo) {
    var cantidad = 0;
    var cantidadMinima = parseFloat($("#articulomin_" + id_articulo + "").val());
    if (isNaN(cantidadMinima)) {
        cantidadMinima = 0;
    }
    var cantidadMaxima = parseFloat($("#articulomax_" + id_articulo + "").val());
    if (isNaN(cantidadMaxima)) {
        cantidadMaxima = 0;
    }
    if (cantidadMinima > cantidadMaxima) {
        cantidad = cantidadMaxima - 2;
        if (cantidad <= 0) {
            cantidad = 0;
            $("#articulomin_" + id_articulo + "").val(cantidad);
        }
        else {
            $("#articulomin_" + id_articulo + "").val(cantidad);
        }
    }

}

function RegistrarMaximoMinimo(id_articulo) {
    var id_articulos = 0;
    var cantidad_minima = $("#articulomin_" + id_articulo).val();
    var cantidad_maxima = $("#articulomax_" + id_articulo).val();
    var cantidad_prom = $("#articuloprom_" + id_articulo).val();
    var id_almacen_g = $("#id_almacen_g_inv_minimo").val();
    if (isNaN(cantidad_prom)) {
        iziToast.warning({
            title: 'AVISO ',
            message: 'El valor minimo debe ser menor al valor maximo',
        });
    } else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../ALMACEN/RegistrarInventarioMinimoMaximo",
            data: {
                id_almacen_g: id_almacen_g,
                id_articulos: id_articulo,
                cantidad_minima: cantidad_minima,
                cantidad_maxima: cantidad_maxima,
                cantidad_prom: cantidad_prom
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 1) {
                    iziToast.success({
                        title: 'Inventario minimo y maximo capturado correctamente!',
                        message: '',
                    });
                    //ConsultarInventarioArticulos();
                }
                else {
                    iziToast.warning({
                        title: 'Ocurrió un error al capturar el inventario minimo  y maximo',
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
}

function RegistrarInventarioMinimo() {

    var id_articulos = [];
    var cantidad_minima = [];
    var cantidad_maxima = [];
    var cantidad_prom = [];
    var id_almacen_g = $("#id_almacen_g_inv_cap").val();
    //var articulosprom = $('#tabla_articulos_minimo .articulos_inventario_cantidad3');



    var listaIDs = [];
    var count = 0;

    var correctos = 0;
    var errores = 0;

    var tabla = $('#tabla_articulos_minimo').DataTable();
    tabla.rows().every(function () {
        var data = this.data(); // Obtener los datos de la fila
        var node = this.node(); // Obtener el nodo de la fila

        var cantidadProm = $(node).find(".articulos_inventario_cantidad3").val();
        var cantidadMINIMA = $(node).find(".articulos_inventario_cantidad1").val();
        var cantidadMAXIMA = $(node).find(".articulos_inventario_cantidad2").val();

        if (!isNaN(cantidadProm)) {
            cantidad_prom[count] = cantidadProm;
            cantidad_minima[count] = cantidadMINIMA;
            cantidad_maxima[count] = cantidadMAXIMA;

            listaIDs.push(data[0]);
            id_articulos[count] = data[0];
            count++;
            correctos++;
        } else {
            $(node).find(".articulos_inventario_cantidad1").css("border", "1px solid red");
            $(node).find(".articulos_inventario_cantidad2").css("border", "1px solid red");
            errores++;
        }
    });

    count = 0;

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de guardar la cantidad de inventario minimo?',
        position: 'center',
        buttons: [
            ['<button><b>Si, guardar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../ALMACEN/RegistrarInventarioMinimo",
                    data: {
                        id_almacen_g: id_almacen_g,
                        id_articulos: id_articulos,
                        cantidad_minima: cantidad_minima,
                        cantidad_maxima: cantidad_maxima,
                        cantidad_prom: cantidad_prom
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == 1) {
                            iziToast.success({
                                title: '',
                                timeout: 4500,
                                overlay: true,
                                displayMode: 'once',
                                position: 'center',
                                message: `<div style="margin-bottom: 10px;" class="col-md-12"><label style="font-size:18px">Se confirmó con éxito: <strong>${correctos}</strong></label><br><label style="font-size:18px">Registros fuera de rango: <strong>${errores}</strong></label></div>`,
                            });

                        }
                        else {
                            iziToast.warning({
                                title: 'Ocurrió un error al capturar el inventario minimo  y maximo',
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
            ['<button>NO</button>', function (instance, toast) {
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

function NotificarMaximoMinimoAlmacen(id_almacen) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/NotificarMaximoMinimoAlmacen",
        data: {
            id_almacen: id_almacen
        },
        success: function (response) {
            if (response == 'True') {
                iziToast.success({
                    title: '¡Exito!',
                    message: '',
                });
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
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



function ConsultarInventarioMinimoMaximo() {
    var id_almacen_g = $("#id_almacen_g_consulta_min").val();
    var id_tipo_inventario = $("#id_tipo_articulos_inventario_min").val();
    var filtro = $("#id_filtro").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarInventarioMaximoMinimo",
        data: {
            id_almacen_g: id_almacen_g,
            id_tipo_inventario: id_tipo_inventario,
            filtro: filtro
        },
        success: function (response) {
            try {
                $("#datatable_inv_maximo_minimo").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventarios_minimo_maximo").html(response);
            $('#datatable_inv_maximo_minimo').DataTable({
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
                responsive: false,
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
    jsRemoveWindowLoad();
}
//#endregion


//#region ------------  AJUSTE DE INVENTARIO

function ShowHideEstablo(modo) {
    var almacen_select = $("#id_almacen_g_ajuste_inventario");
    var div_select = $("#div_cambiar_establo_ajust");
    if (modo == 1) { //CAMBIO DE ESTABLO
        almacen_select.attr("disabled", true);
        div_select.css("display", "block");
    }
    else {
        almacen_select.removeAttr("disabled");
        almacen_select.find('option[value=0]').prop('selected', 'selected');
        div_select.css("display", "none");
        $("#tbody_ajuste_inventario").html("");
        $(".inputs_ajuste").val("");
    }
}

function ConsultarInformacionArticuloAjuste(clave_nombre, existencia) {
    var id_almacen_g = $("#id_almacen_g_ajuste_inventario").val();
    if (existencia == 1) { id_almacen_g = 0; }  //0: CONSULTA EXISTENCIA X ALMACEN    1:CONSULTA SOLO LA INFO DEL ARTICULO
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarInformacionArticuloAjuste",
        data: {
            id_almacen_g: id_almacen_g,
            clave_nombre: clave_nombre,
            existencia: existencia
        },
        success: function (response) {
            if (response == "0") {
                iziToast.warning({
                    title: 'No se encontró el articulo',
                    message: '',
                });
            }
            else {
                var item = $.parseJSON(response);
                $("#existencia_articulo_ajuste").val(item[0]);
                $("#id_articulo_ajuste").val(item[1]);
                $("#cve_articulo_ajuste").val(item[2]);
                $("#nombre_articulo_ajuste").val(item[3]);
                $("#tipo_articulo_ajuste").val(item[4]);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AgregarArticuloAjuste() {
    var existencia_fisica = $("#existencia_fisica_ajuste");
    existencia_fisica.css("border-color", "");
    $("#clave_nombre").css("border-color", "");

    var id_articulo = $("#id_articulo_ajuste").val();
    if (existencia_fisica.val() == "" || existencia_fisica.val() < 0) {
        iziToast.warning({
            title: 'Ingresa una cantidad válida',
            message: '',
        });
        existencia_fisica.css("border-color", "red");
    }
    else if (id_articulo == "") {
        $("#clave_nombre").css("border-color", "red");
        iziToast.warning({
            title: 'Ingresa un articulo',
            message: '',
        });
    }
    else {
        var ajuste_inventario = existencia_fisica.val() - $("#existencia_articulo_ajuste").val();
        if (ajuste_inventario == 0) {
            iziToast.warning({
                title: 'No es posible realizar un ajuste por cantidad 0',
                message: '',
            });
        }
        else {
            var entrada_salida = true; //ENTRADA
            if (ajuste_inventario < 0) { entrada_salida = false; }
            var new_ajuste = [];
            new_ajuste.push({
                clave: $("#cve_articulo_ajuste").val(),
                nombre: $("#nombre_articulo_ajuste").val(),
                familia: $("#tipo_articulo_ajuste").val(),
                exis_sis: parseFloat($("#existencia_articulo_ajuste").val()).toFixed(2),
                exis_fis: existencia_fisica.val(),
                ajuste: ajuste_inventario.toFixed(2),
                entrada_salida: entrada_salida
            });

            $("#trArticuloAjuste_" + id_articulo + "").remove();

            var tbody = $("#tbody_ajuste_inventario");
            for (var i = 0; i < new_ajuste.length; i++) {
                var row = $("<tr class='text-center articulos_ajuste_data' id='trArticuloAjuste_" + id_articulo + "'></tr>");
                for (var key in new_ajuste[i]) {
                    if ([key] == "ajuste") {
                        var color = "green";
                        if (new_ajuste[i][key] < 0) { color = "red"; }
                        row.append("<td id='" + [key] + "_" + id_articulo + "' style='color:" + color + ";'>" + Math.abs(new_ajuste[i][key]) + "</td>");
                    }
                    else if ([key] == "entrada_salida") {
                        var mov = "ENTRADA";
                        var color = "green";
                        if (new_ajuste[i][key] == false) { color = "red"; mov = "SALIDA"; }
                        row.append("<td style='color:" + color + ";'>" + mov + " <label style='display:none;' id='" + [key] + "_" + id_articulo + "' >" + new_ajuste[i][key] + "</label></td>");
                    }

                    else { row.append("<td id='" + [key] + "_" + id_articulo + "'>" + new_ajuste[i][key] + "</td>"); }
                }
                row.append("<td><button class='btn btn_beta_danger' onclick='QuitarArticuloAjuste(" + id_articulo + ")'><i class='fa fa-remove'></i></button></td>");
                tbody.append(row);  // Add the row to the table body
            }
        }
    }
}

function QuitarArticuloAjuste(id_articulo) {
    $("#trArticuloAjuste_" + id_articulo +"").remove();
}

function RegistrarAjusteInventario() {
    var data_ajuste = $(".articulos_ajuste_data");
    if (data_ajuste.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 articulo',
            message: '',
        });
    }
    else {
        var id_almacen = $("#id_almacen_g_ajuste_inventario").val();
        var id_articulos = [];
        var exist_sistema = [];
        var exist_fisica = [];
        var entrada_salida = [];
        var ajuste = [];
        var count = 0;

        var valid_ajuste = false;
        data_ajuste.each(function () {
            var id_articulo = $(this).attr('id').split('_')[1];
            var valor_exist_sistema = $("#exis_sis_" + id_articulo + "").text();
            var valor_exist_fisica = $("#exis_fis_" + id_articulo + "").text();
            var valor_ajuste = $("#ajuste_" + id_articulo + "").text();
            if (valor_ajuste == 0) { valid_ajuste = true; }

            var valor_mov = true;
            if ($("#entrada_salida_" + id_articulo + "").text().toLowerCase() === "false") { valor_mov = false; }
            
            id_articulos[count] = id_articulo;
            exist_sistema[count] = valor_exist_sistema;
            exist_fisica[count] = valor_exist_fisica;
            entrada_salida[count] = valor_mov;
            ajuste[count] = valor_ajuste;
            count++;
        });

        if (valid_ajuste == true) {
            iziToast.warning({
                title: 'No es posible realizar un ajuste por cantidad 0',
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
                title: 'ADVERTENCIA',
                message: '¿Está seguro de generar el ajuste de inventario?',
                position: 'center',
                buttons: [
                    ['<button><b>Si, ajustar</b></button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                        $.ajax({
                            type: "POST",
                            async: false,
                            url: "../ALMACEN/RegistrarAjusteInventario",
                            data: {
                                id_almacen: id_almacen,
                                id_articulos: id_articulos,
                                exist_sistema: exist_sistema,
                                exist_fisica: exist_fisica,
                                ajuste: ajuste,
                                entrada_salida: entrada_salida
                            },
                            success: function (response) {
                                if (response == 1) {
                                    iziToast.warning({
                                        title: 'No es posible realizar un ajuste por cantidad 0',
                                        message: '',
                                    });
                                }
                                else if (response == -1) {
                                    iziToast.error({
                                        title: 'Ocurrió un error al intentar guardar el ajuste',
                                        message: '',
                                    });
                                    ValidaSesion();
                                }
                                else {
                                    iziToast.success({
                                        title: 'Ajuste realizado correctamente',
                                        message: '',
                                    });
                                    $("#tbody_ajuste_inventario").html("");
                                    $(".inputs_ajuste").val("");
                                }
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                                jsRemoveWindowLoad();
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
}

function ConsultarAjustesInventarioHistorial() {
    var id_almacen_g = $("#id_almacen_g_ajuste_consulta").val();
    var fecha_inicio = $("#fecha_inicio_ajuste_consulta").val();
    var fecha_fin = $("#fecha_fin_ajuste_consulta").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarAjustesInventarioHistorial",
        data: {
            id_almacen_g: id_almacen_g,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            $("#div_ajuste_consulta").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarAjusteInventarioDetalle(id_ajuste_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarAjusteInventarioDetalle",
        data: { id_ajuste_g: id_ajuste_g },
        success: function (response) {
            $("#div_ajuste_inventario_consulta").html(response);
            $("#m_ajuste_inventario_consulta").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function EliminarAjusteInventario(id_ajuste_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de eliminar el ajuste de inventario?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ALMACEN/EliminarAjusteInventario",
                    data: { id_ajuste_g: id_ajuste_g },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Ajuste eliminado correctamente',
                                message: '',
                            });
                            ConsultarAjustesInventarioHistorial();
                        }
                        else if (response == 1) {
                            iziToast.error({
                                title: 'Ocurrió un error al eliminar el ajuste',
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


function ImprimirAjusteInventario(id_ajuste_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ImprimirAjusteInventario",
        data: { id_ajuste_g: id_ajuste_g },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_impresion_ajuste_consulta").css("display", "block");
            $("#div_impresion_ajuste_consulta").html(response);
            var HTML_Width = $("#div_impresion_ajuste_consulta").width();
            var HTML_Height = $("#div_impresion_ajuste_consulta").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;
            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;
            html2canvas($("#div_impresion_ajuste_consulta")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/png", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Ajuste de inventario #" + id_ajuste_g + ".pdf");
            });
            $("#div_impresion_ajuste_consulta").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion


//#region ----------- IMPRESIÓN DE CODIGOS QR
function AgregarArticuloCodigos() {
    var id_articulo = $("#id_articulo_ajuste").val();
    var clave = $("#cve_articulo_ajuste").val();
    var nombre = $("#nombre_articulo_ajuste").val();
    var tipo = $("#tipo_articulo_ajuste").val();

    if (id_articulo == "" || clave == "" || nombre == "" || tipo == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Ingresa el nombre o la clave de articulo para buscar.',
        });
    }
    else {
        var new_codigo = [];
        new_codigo.push({
            select: id_articulo,
            id_articulo: id_articulo,
            clave: clave,
            nombre, nombre,
            tipo: tipo
        });

        $("#trArticuloAjuste_" + id_articulo + "").remove();

        var tbody = $("#tbody_codigos");
        for (var i = 0; i < new_codigo.length; i++) {
            var row = $("<tr class='text-center articulos_ajuste_data' id='trArticuloAjuste_" + id_articulo + "'></tr>");
            for (var key in new_codigo[i]) {
                if (key == "id_articulo") { row.append("<td style='display:none;' class='" + [key] + " text-left' id='" + [key] + "_" + id_articulo + "'>" + new_codigo[i][key] + "</td>"); }
                else if (key == "select") { row.append("<td><input style='height:25px;' type='checkbox' class='" + [key] + " form-control' id='" + [key] + "_" + id_articulo + "' checked/></td>"); }
                else if (key == "nombre") { row.append("<td class='text-left'>" + new_codigo[i][key] + "</td>"); }
                else { row.append("<td>" + new_codigo[i][key] + "</td>"); }
            }
            row.append("<td><button class='btn btn_beta_danger' onclick='QuitarArticuloAjuste(" + id_articulo + ")'><i class='fa fa-remove'></i></button></td>");
            tbody.append(row);
        }
        $("#clave_nombre").val("");
        $("#clave_nombre").focus();
    }
}

function GenerarFormatosCodigosArticulos() {
    var modo = $('input[name="modo_codigo"]:checked').val();
    var articulos = $(".id_articulo");
    if (modo == "" || modo == undefined) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Selecciona el modo de impresión de códigos',
        });
    }
    else if (articulos.length <= 0) {
        iziToast.warning({
            title: 'Ingrese artículos para imprimir',
            message: '',
        });
    }
    else {
        var count = 0;
        var id_articulos = [];
        articulos.each(function () {
            var id_articulo = $(this).text();
            if ($("#select_" + id_articulo + "").is(':checked')) {
                id_articulos[count] = id_articulo;
                count++;
            }
        });
        if (id_articulos.length <= 0) {
            iziToast.warning({
                title: 'Selecciona los artículos para imprimir',
                message: '',
            });
            return;
        }
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 9000000,
            url: "../ALMACEN/GenerarFormatosCodigosArticulos",
            data: {
                id_articulos: id_articulos,
                modo: modo
            },
            success: function (result) {
                jsRemoveWindowLoad();
                html2pdf()
                    .from(result)
                    .set({
                        margin: [15, 15, 15, 15],
                        filename: "ETIQUETAS DE CÓDIGOS DE ARTÍCULOS.pdf",
                        image: { type: 'png', quality: 2 },
                        html2canvas: { scale: 1 },
                        jsPDF: { unit: 'pt', format: 'letter', orientation: 'portrait' },
                        pagebreak: { mode: ['avoid-all', 'css', 'legacy'] }
                    })
                    .save()
                    .then(function () {
                        jsRemoveWindowLoad();
                        //$("#tbody_codigos").html('');
                    });
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    
}

function AgregarArticulosTiposCodigos() {
    jsShowWindowLoad();
    var id_tipo_articulo = $("#id_tipo_articulo").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../ALMACEN/AgregarArticulosTiposCodigos",
        data: { id_tipo_articulo: id_tipo_articulo },
        success: function (result) {
            jsRemoveWindowLoad();
            $("#tbody_codigos").html(result);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#endregion


//#region ----------- ARTICULOS CRITICOS ALMACEN
function ShowHideEstabloCriticos(modo) {
    var almacen_select = $("#id_almacen");
    var div_select = $("#div_cambiar_establo_ajust");
    if (modo == 1) { //CAMBIO DE ESTABLO
        almacen_select.attr("disabled", true);
        div_select.css("display", "block");
    }
    else {
        almacen_select.removeAttr("disabled");
        div_select.css("display", "none");
        $("#div_criticidad_articulos").html("");
    }
}
function ConsultarTiposConsumoCriticidadSelect() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarTiposConsumoCriticidadSelect",
        data: {},
        success: function (response) {
            //$(".select_tipos_consumo_criticidad").html(response);
            options_tipos_consumo_criticidad = response;
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ConsultarTiposOperacionCriticidadSelect() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarTiposOperacionCriticidadSelect",
        data: { },
        success: function (response) {
            //$(".select_tipos_operacion_criticidad").html(response);
            options_tipos_operacion_criticidad = response;
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function ConsultarArticulosCriticidadAlmacen() {
    var id_almacen = $("#id_almacen").val();
    var id_articulo_tipo = $("#id_articulo_tipo").val();
    var tipo_vista = $('input[name="tipo_vista"]:checked').val();
    if (tipo_vista == undefined || tipo_vista == "") {
        iziToast.info({
            title: 'Selecciona el tipo de vista',
            message: '',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000,
        url: "../ALMACEN/ConsultarArticulosCriticidadAlmacen",
        data: {
            id_almacen: id_almacen,
            id_articulo_tipo: id_articulo_tipo,
            tipo_vista: tipo_vista
        },
        success: function (response) {
            ShowHideEstabloCriticos(1);
            jsRemoveWindowLoad();
            try {
                $("#datatable_criticidad_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_criticidad_articulos").html(response);
            $('#datatable_criticidad_articulos').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
            AsignarValoresSelectTipos();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function AsignarValoresSelectTipos() {
    var tabla = $("#datatable_criticidad_articulos").DataTable();
    tabla.rows().every(function () {
        var node = this.node(); // Obtener el nodo de la fila
        var id_tipo_consumo = $(node).find(".select_tipos_consumo_criticidad").attr("name");
        if (id_tipo_consumo != "0") { $(node).find(".select_tipos_consumo_criticidad").val(id_tipo_consumo); }

        var id_tipo_operacion = $(node).find(".select_tipos_operacion_criticidad").attr("name");
        if (id_tipo_operacion != "0") { $(node).find(".select_tipos_operacion_criticidad").val(id_tipo_operacion); }
    });
}
function GuardarArticulosCriticosAlmacen() {
    var id_articulos = [];
    var id_t_csm = [];
    var id_t_ope = [];
    var count = 0;

    var id_almacen = $("#id_almacen").val();
    var tabla = $("#datatable_criticidad_articulos").DataTable();
    tabla.rows().every(function () {
        var data = this.data(); // Obtener los datos de la fila
        var node = this.node(); // Obtener el nodo de la fila

        var id_articulo = $(node).find(".select_tipos_consumo_criticidad").attr("id").split("_")[2];
        var id_csm = $(node).find(".select_tipos_consumo_criticidad").val();
        var id_ope = $(node).find(".select_tipos_operacion_criticidad").val();

        id_articulos[count] = id_articulo;
        id_t_csm[count] = id_csm;
        id_t_ope[count] = id_ope;
        count++;
    });

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Atención',
        message: '¿Está seguro de modificar los valores?',
        position: 'center',
        buttons: [
            ['<button><b>Si, actualizar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ALMACEN/GuardarArticulosCriticosAlmacen",
                    data: {
                        id_almacen: id_almacen,
                        id_articulos: id_articulos,
                        id_t_csm: id_t_csm,
                        id_t_ope: id_t_ope
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Cambios guardados correctamente',
                                message: '',
                            });
                            ConsultarArticulosCriticidadAlmacen();
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al guardar los datos',
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
function GuardarArticuloCriticoAlmacen(id_articulos) {
    var id_almacen = $("#id_almacen").val();
    var id_t_csm = $("#tipo_consumo_" + id_articulos + "").val();
    var id_t_ope = $("#tipo_operacion_" + id_articulos + "").val();
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Atención',
        message: '¿Está seguro de modificar los valores?',
        position: 'center',
        buttons: [
            ['<button><b>Si, modificar valores</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ALMACEN/GuardarArticulosCriticosAlmacen",
                    data: {
                        id_almacen: id_almacen,
                        id_articulos: id_articulos,
                        id_t_csm: id_t_csm,
                        id_t_ope: id_t_ope
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Cambios guardados correctamente',
                                message: '',
                            });
                            ConsultarArticulosCriticidadAlmacen();
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al guardar los datos',
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
function ConsultarInventarioCriticoExistencias() {
    var id_almacen = $("#id_almacen_g_critico").val();
    var fecha_existencia = $("#fecha_inventario_critico").val();
    var id_t_csm = $("#id_t_csm").val();
    var id_t_ope = $("#id_t_ope").val();
    if (id_t_csm == null || id_t_csm == undefined) {
        iziToast.warning({
            title: 'Selecciona un tipo de consumo',
            message: '',
        });
        return;
    }
    if (id_t_ope == null || id_t_ope == undefined) {
        iziToast.warning({
            title: 'Selecciona un tipo de operación',
            message: '',
        });
        return;
    }
    if (fecha_existencia == "") {
        iziToast.warning({
            title: 'Ingrese una fecha válida',
            message: '',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarInventarioCriticoExistencias",
        data: {
            id_almacen: id_almacen,
            id_t_csm: id_t_csm,
            id_t_ope: id_t_ope,
            fecha_existencia: fecha_existencia
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_inventario_critico").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_inventario_critico").html(response);
            $('#datatable_inventario_critico').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
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
                responsive: false,
                pageLength: 50,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#endregion







