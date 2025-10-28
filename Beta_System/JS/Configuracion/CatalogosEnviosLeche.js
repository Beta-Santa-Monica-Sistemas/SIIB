//#region CATALOGO DESTINOS
function LimpiarCatalogoDestino() {
    $("#destino_nombre").val('');
    $("#destino_status").prop("checked", false);
}

function ConsultarDestinosCatalogoBascula() {
    var estatus = $("#id_activo_destino").val();
    jsShowWindowLoad();
    $("#div_tabla_destinos").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarDestinosCatalogoBascula",
        data: { estatus: estatus },
        success: function (response) {
            try {
                $("#tabla_bascula_destino").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_destinos").html(response);
            $('#tabla_bascula_destino').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ModalCatalogoDestinos() {
    LimpiarCatalogoDestino();
    $("#m_bascula_destinos").modal("show");
    $("#destino_status").prop('disabled', true);
    $("#destino_status").prop('checked', true);

    $("#m_bascula_destinos_lbl").text("Registrar destinos");
    $("#bascula_destinos_btn").attr("onclick", "RegistrarDestinosCatalogoBascula()");
}

function RegistrarDestinosCatalogoBascula() {
    var nombre = $('#destino_nombre').val().toUpperCase();
    if ($('#destino_nombre').val().trim() != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CONFIGURACION/RegistrarDestinosCatalogoBascula",
            data: {
                nombre: nombre
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se registro el destino correctamente',
                    });
                    /*LimpiarCatalogoDestino();*/
                    ModalCatalogoAsociacion(nombre);
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el destino',
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
            message: 'Favor de ingresar un nombre del destino'
        });
    }
}

function AsignarInformacionDestinosCatalogoBascula(id_destino_envio_leche) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/AsignarInformacionDestinosCatalogoBascula",
        data: { id_destino_envio_leche: id_destino_envio_leche },
        success: function (response) {
            if (response != "[]") {
                $("#m_bascula_destinos_lbl").text("Modificar destino");
                $("#bascula_destinos_btn").attr("onclick", "ModificarDestinosCatalogoBascula(" + id_destino_envio_leche + ")");

                LimpiarCatalogoDestino();
                $('#destino_status').prop('disabled', false);
                $('#m_bascula_destinos').modal("show");
                var data = $.parseJSON(response);
                $("#destino_nombre").val(data[0].nombre_destino);
                var activo = data[0].activo;
                if (activo == true) { $("#destino_status").prop('checked', true) }
                else { $("#destino_status").prop('checked', false) }
            }
            else {
                iziToast.warning({
                    title: '',
                    message: 'No se encontró informacion',
                });
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarDestinosCatalogoBascula(id_destino_envio_leche) {
    var nombre = $('#destino_nombre').val().toUpperCase();
    var activo = false;
    if ($("#destino_status").is(':checked')) {
        activo = true;
    }

    if ($('#destino_nombre').val().trim() != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CONFIGURACION/ModificarDestinosCatalogoBascula",
            data: {
                id_destino_envio_leche: id_destino_envio_leche,
                nombre: nombre,
                activo: activo
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se modifico el destino correctamente',
                    });
                    LimpiarCatalogoDestino();
                    $('#m_bascula_destinos').modal("hide");
                    $("#bascula_destinos_btn").attr("onclick", "");
                    ConsultarDestinosCatalogoBascula();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al modificar el destino',
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
            message: 'Favor de ingresar un nombre del destino'
        });
    }
}

function OnOffDestinosCatalogoBascula(id_destino_envio_leche, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/OnOffDestinosCatalogoBascula",
        data: {
            id_destino_envio_leche: id_destino_envio_leche,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarDestinosCatalogoBascula();
                ConsultarClienteDestinosBascula();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion

//#region CATALOGO ASOCIACIONES
function ModalCatalogoAsociacion(buscar) {
    $("#m_bascula_asociacion").modal("show");
    ConsultarClienteAsociacion();
    ConsultarDestinosAsociacion(buscar);
}

function ConsultarClienteDestinosBascula() {
    jsShowWindowLoad();
    $("#div_tabla_asociacion").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarClienteDestinosBascula",
        data: {},
        success: function (response) {
            try {
                $("#tabla_asociacion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_asociacion").html(response);
            $('#tabla_asociacion').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ConsultarClienteAsociacion() {
    jsShowWindowLoad();
    $("#div_tabla_asociacion_cliente").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarClienteAsociacion",
        data: {},
        success: function (response) {
            try {
                $("#tabla_asociacion_cliente").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_asociacion_cliente").html(response);
            $('#tabla_asociacion_cliente').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ConsultarDestinosAsociacion(buscar) {
    jsShowWindowLoad();
    $("#div_tabla_asociacion_destino").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarDestinosAsociacion",
        data: {},
        success: function (response) {
            try {
                $("#tabla_asociacion_destino").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_asociacion_destino").html(response);
            $('#tabla_asociacion_destino').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });

            if (buscar != "" || buscar != undefined) {
                var table = $('#tabla_asociacion_destino').DataTable();
                table.search(buscar).draw();
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ValidarAsociarClienteDestino() {
    var id_envio_leche_cliente_ms = $('input[name=radio_cliente]:checked').val();
    var id_destino_envio_leche = $('input[name=radio_destino]:checked').val();
    return new Promise(function (resolve, reject) {
        $.ajax({
            type: "POST",
            url: "../CONFIGURACION/ValidarClienteDestino",
            data: {
                id_envio_leche_cliente_ms: id_envio_leche_cliente_ms,
                id_destino_envio_leche: id_destino_envio_leche
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'El cliente ya esta asociado con el destino seleccionado!',
                    });
                    $('input[name=radio_cliente]:checked').prop('checked', false);
                    $('input[name=radio_destino]:checked').prop('checked', false);
                    resolve(false);  // La clave no está disponible
                } else {
                    resolve(true);  // La clave está disponible
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                reject(error);  // Error en la llamada AJAX
            }
        });
    });
}

function AsociarClienteDestino() {
    var id_envio_leche_cliente_ms = $('input[name=radio_cliente]:checked').val();
    var id_destino_envio_leche = $('input[name=radio_destino]:checked').val();

    if (id_envio_leche_cliente_ms != undefined && id_destino_envio_leche != undefined) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Advertencia',
            message: '¿Estas seguro de asociar el cliente con el destino?',
            position: 'center',
            buttons: [
                ['<button><b>Sí, guardar.</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                    ValidarAsociarClienteDestino().then(function (isValid) {
                        if (isValid) {
                            jsShowWindowLoad();
                            $.ajax({
                                type: "POST",
                                async: false,
                                url: "../CONFIGURACION/RegistrarClienteDestino",
                                data: {
                                    id_envio_leche_cliente_ms: id_envio_leche_cliente_ms,
                                    id_destino_envio_leche: id_destino_envio_leche
                                },
                                success: function (response) {
                                    if (response == "True") {
                                        iziToast.success({
                                            title: 'Exito',
                                            message: 'Se asocio correctamente el proveedor con el destino',
                                        });
                                        $('input[name=radio_cliente]:checked').prop('checked', false);
                                        $('input[name=radio_destino]:checked').prop('checked', false);
                                    }
                                    else {
                                        iziToast.error({
                                            title: 'Error',
                                            message: 'Ocurrió un problema al asociar el cliente con el destinoo',
                                        });
                                    }
                                },
                                error: function (xhr, status, error) {
                                    console.error(error);
                                }
                            });
                            jsRemoveWindowLoad();
                        }
                        else {
                            instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                        }

                    }).catch(function (error) {
                        console.error('Error en la validación: ', error);
                    });

                }, true],
                ['<button>No guardar.</button>', function (instance, toast) {

                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                    $('input[name=radio_cliente]:checked').prop('checked', false);
                    $('input[name=radio_destino]:checked').prop('checked', false);
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

function EliminarAsociarClienteDestino(id_cliente_destino) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/EliminarAsociarClienteDestino",
        data: {
            id_cliente_destino: id_cliente_destino
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se elimino la asociacion del cliente con el destino',
                });
                ConsultarClienteDestinosBascula();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al eliminar la asociacion del cliente con el destino',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion

//#region CLIENTES ENVIO DE LECHE
function LimpiarClientesEnvioLeche() {
    $("#cliente_nombre").val('');
    $("#cliente_status").prop("checked", false);
}

function ConsultaClientesEnvioLecheTable() {
    var estatus = $("#id_activo_cliente").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ConsultaClientesEnvioLecheTable",
        data: { estatus: estatus },
        success: function (response) {
            try {
                $("#datatable_cliente_leche").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_clientes").html(response);
            $('#datatable_cliente_leche').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pageLength: 20,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function OnOffClientesEnvioLeche(id_envio_leche_cliente_ms, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/OnOffClientesEnvioLeche",
        data: {
            id_envio_leche_cliente_ms: id_envio_leche_cliente_ms,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultaClientesEnvioLecheTable();
                ConsultarClienteDestinosBascula();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AsignarInformacionClientesEnvioLeche(id_envio_leche_cliente_ms) {
    LimpiarClientesEnvioLeche();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/AsignarInformacionClientesEnvioLeche",
        data: { id_envio_leche_cliente_ms: id_envio_leche_cliente_ms },
        success: function (response) {
            if (response != "[]") {
                $('#m_cliente').modal("show");
                var data = $.parseJSON(response);
                $("#cliente_nombre").val(data[0].nombre_comercial);
                var activo = data[0].activo;
                if (activo == true) { $("#cliente_status").prop('checked', true) }
                else { $("#cliente_status").prop('checked', false) }
                $("#btn_accion_cliente").attr("onclick", "ModificarClientesEnvioLeche(" + id_envio_leche_cliente_ms + ")");
            }
            else {
                iziToast.warning({
                    title: '',
                    message: 'No se encontró informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarClientesEnvioLeche(id_envio_leche_cliente_ms) {
    var nombre_comercial = $('#cliente_nombre').val().toUpperCase();
    var activo = false;
    if ($("#cliente_status").is(':checked')) {
        activo = true;
    }

    if ($('#cliente_nombre').val().trim() != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CONFIGURACION/ModificarClientesEnvioLeche",
            data: {
                id_envio_leche_cliente_ms: id_envio_leche_cliente_ms,
                nombre_comercial: nombre_comercial,
                activo: activo
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se modifico el cliente correctamente',
                    });
                    LimpiarClientesEnvioLeche();
                    $('#m_cliente').modal("hide");
                    $("#btn_accion_cliente").attr("onclick", "");
                    ConsultaClientesEnvioLecheTable();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al modificar al cliente',
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
            message: 'Favor de ingresar un nombre comercial al cliente'
        });
    }
}

//#endregion

//#region PRECIOS LECHE DESTINOS
function ConsultarPreciosLecheDestinosTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../CONFIGURACION/ConsultarPreciosLecheDestinosTable",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_precios_leche").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_precios_leche").html(response);
            $('#datatable_precios_leche').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function GuardarPreciosLecheDestinos() {
    var id_destinos = [];
    var precios = [];
    var count = 0;
    var valid_negativos = false;
    $(".id_destinos_leche_precios").css("border", "");
    $(".id_destinos_leche_precios").each(function () {
        var id_destino = $(this).attr("id").split("_")[1];
        id_destinos[count] = id_destino;

        var precio = $("#LecheDestinoPrecio_" + id_destino + "").val();
        if (precio == "" || precio == undefined) { precio = 0; }
        if (precio < 0) { valid_negativos = true; $("#LecheDestinoPrecio_" + id_destino + "").css("border", "1px solid red"); }
        precios[count] = precio;
        count++;
    });
    if (id_destinos.length == 0 || precios.length == 0) {
        iziToast.warning({
            title: 'No se detectaron precios a actualizar',
            message: '',
        });
    }
    else if (valid_negativos == true) {
        iziToast.warning({
            title: 'Asegurese de ingresar solo valores positivos',
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
            message: '¿Está seguro de actualizar los precios?',
            position: 'center',
            buttons: [
                ['<button><b>Si, actualizar ahora</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../CONFIGURACION/GuardarPreciosLecheDestinos",
                        data: {
                            id_destinos: id_destinos,
                            precios: precios
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Precios actualizados correctamente',
                                    message: '',
                                });
                                ConsultarPreciosLecheDestinosTable();
                            }
                            else if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al actualizar los precios',
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
}


//#endregion

var count_tabla = 1;

//#region CUMPLIMIENTO ENVIO DE LECHE
function ModalCompromiso() {
    $("#modalCompromiso").modal("show");
    $("#selectAnio").prop('disabled', false);
    $("#selectCliente").prop('disabled', false);
    $("#btnCompromisoBuscar").prop('disabled', false);
    $("#rowRegistroCompromiso").css("display", "block");
    $("#rowMostrarCompromiso").css("display", "none");
}

function PreCargaCompromisoCliente() {
    var compromisoCliente = $("#selectCliente").val();
    var anio = $("#selectAnio  option:selected").text();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/PreCargaCompromisoCliente",
        data: {
            compromisoCliente: compromisoCliente,
            count_tabla: count_tabla,
            anio: anio
        },
        success: function (response) {
            $("#div_tablas_compromiso").append(response);
            count_tabla = count_tabla + 1;
            $("#selectAnio").prop('disabled', true);
            $("#selectCliente").prop('disabled', true);
            $("#btnCompromisoBuscar").prop('disabled', true);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function CalculoLitrosCompromiso(idCliente, idMes, diasMes) {
    var litrosMensual = $("#litros_mensual_" + idMes + "_" + idCliente).val();
    var litrosDiario = litrosMensual / diasMes;
    var fullMensual = litrosMensual / 55000;

    $("#litros_diario_" + idMes + "_" + idCliente).val(litrosDiario.toFixed(3));
    $("#litros_full_" + idMes + "_" + idCliente).val(fullMensual.toFixed(3));

    var litrosMensualTotal = 0;
    var litrosDiarioTotal = 0;
    var fullTotal = 0;

    $(".litros_mensual").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            litrosMensualTotal += parseFloat(litros);
        }
    });
    $(".litros_diario").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            litrosDiarioTotal += parseFloat(litros);
        }
    });
    $(".full_mensual").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            fullTotal += parseFloat(litros);
        }
    });

    $("#litros_total_mensual").val(litrosMensualTotal.toFixed(3));
    $("#litros_total_diario").val(litrosDiarioTotal.toFixed(3));
    $("#litros_total_full").val(fullTotal.toFixed(3));
}


function ValidarCargaCompromisoCliente() {
    var compromisoCliente = $("#selectCliente").val();
    var idAnio = $("#selectAnio").val();
    if (compromisoCliente == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar un cliente',
        });
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ValidarCargaCompromisoCliente",
        data: {
            idCliente: compromisoCliente,
            idAnio: idAnio
        },
        success: function (response) {
            if (response == "False") {
                PreCargaCompromisoCliente();
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ya se tiene registro del cumplimiento del cliente',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function CargaCompromisoCliente() {
    iziToast.question({
        timeout: 10000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999999,
        title: 'Confirmacion',
        message: '¿Estas seguro de registrar el compromiso?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                var compromisoCliente = $("#selectCliente").val();
                var idAnio = $("#selectAnio").val();
                var contador = 0;

                var mes = [];
                var litros_mensual = [];
                var litros_dia = [];
                var full_mensual = [];

                $(".compromisoIDMes").each(function () {
                    var mesCompromiso = $(this).text().trim();
                    mes[contador] = mesCompromiso;
                    var lts_mensual = $("#litros_mensual_" + mesCompromiso + "_" + compromisoCliente).val().trim();
                    var lts_dia = $("#litros_diario_" + mesCompromiso + "_" + compromisoCliente).val().trim();
                    var f_mensual = $("#litros_full_" + mesCompromiso + "_" + compromisoCliente).val().trim();

                    if (lts_mensual != "") {
                        litros_mensual[contador] = lts_mensual;
                    }
                    else {
                        litros_mensual[contador] = 0;
                    }

                    if (lts_dia != "") {
                        litros_dia[contador] = lts_dia;
                    }
                    else {
                        litros_dia[contador] = 0;
                    }

                    if (f_mensual != "") {
                        full_mensual[contador] = f_mensual;
                    }
                    else {
                        full_mensual[contador] = 0;
                    }
                    contador++;
                });

                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CONFIGURACION/CargaCompromisoCliente",
                    data: {
                        idCliente: compromisoCliente,
                        idAnio: idAnio,
                        mes: mes,
                        litros_mensual: litros_mensual,
                        litros_dia: litros_dia,
                        full_mensual: full_mensual
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se registro el compromiso',
                            });
                            $("#div_tablas_compromiso").html('');
                            $("#selectAnio").prop('disabled', false);
                            $("#selectCliente").prop('disabled', false);
                            $("#btnCompromisoBuscar").prop('disabled', false);
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al registrar el compromiso',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
                jsRemoveWindowLoad();
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

function CancelarCargaCompromisoCliente() {
    $("#selectAnio").prop('disabled', false);
    $("#selectCliente").prop('disabled', false);
    $("#btnCompromisoBuscar").prop('disabled', false);
    $("#div_tablas_compromiso").html('');
}

function TablaCompromisoClientes() {
    var idAnio = $("#selectAnioTabla").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/TablaCompromisoClientes",
        data: {
            idAnio: idAnio
        },
        success: function (response) {
            $("#div_compromiso_cliente").html(response);
            $('#tabla_compromisos_cliente').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function MostrarCompromisoCliente(idCliente, idAnio) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/MostrarCompromisoCliente",
        data: {
            idCliente: idCliente,
            idAnio: idAnio
        },
        success: function (response) {
            $("#modalCompromiso").modal("show");
            $("#rowRegistroCompromiso").css("display", "none");
            $("#rowMostrarCompromiso").css("display", "block");
            $("#div_tabla_mostrar_compromiso").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function CalculoLitrosCompromisoEdit(idCompromiso, idMes, diasMes) {
    var litrosMensual = $("#litros_mensual_edit_" + idCompromiso + "_" + idMes).val();
    var litrosDiario = litrosMensual / diasMes;
    var fullMensual = litrosMensual / 55000;

    $("#litros_diario_edit_" + idCompromiso + "_" + idMes).val(litrosDiario.toFixed(3));
    $("#litros_full_edit_" + idCompromiso + "_" + idMes).val(fullMensual.toFixed(3));

    var litrosMensualTotal = 0;
    var litrosDiarioTotal = 0;
    var fullTotal = 0;

    $(".litros_mensualEdit").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            litrosMensualTotal += parseFloat(litros);
        }
    });
    $(".litros_diarioEdit").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            litrosDiarioTotal += parseFloat(litros);
        }
    });
    $(".full_mensualEdit").each(function () {
        var litros = $(this).val().trim();
        if (litros != "") {
            fullTotal += parseFloat(litros);
        }
    });

    $("#litros_total_mensual_edit").val(litrosMensualTotal.toFixed(3));
    $("#litros_total_diario_edit").val(litrosDiarioTotal.toFixed(3));
    $("#litros_total_full_edit").val(fullTotal.toFixed(3));
}

function CargaCompromisoClienteEdit(idCompromiso, idAnio, compromisoCliente) {
    iziToast.question({
        timeout: 10000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999999,
        title: 'Confirmacion',
        message: '¿Estas seguro de guardar el compromiso?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                var contador = 0;

                var mes = [];
                var litros_mensual = [];
                var litros_dia = [];
                var full_mensual = [];

                $(".compromisoIDMesEdit").each(function () {
                    var mesCompromiso = $(this).text().trim();
                    mes[contador] = mesCompromiso;
                    var lts_mensual = $("#litros_mensual_edit_" + idCompromiso + "_" + mesCompromiso).val().trim();
                    var lts_dia = $("#litros_diario_edit_" + idCompromiso + "_" + mesCompromiso).val().trim();
                    var f_mensual = $("#litros_full_edit_" + idCompromiso + "_" + mesCompromiso).val().trim();

                    if (lts_mensual != "") {
                        litros_mensual[contador] = lts_mensual;
                    }
                    else {
                        litros_mensual[contador] = 0;
                    }

                    if (lts_dia != "") {
                        litros_dia[contador] = lts_dia;
                    }
                    else {
                        litros_dia[contador] = 0;
                    }

                    if (f_mensual != "") {
                        full_mensual[contador] = f_mensual;
                    }
                    else {
                        full_mensual[contador] = 0;
                    }
                    contador++;
                });

                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CONFIGURACION/CargaCompromisoClienteEdit",
                    data: {
                        compromisoCliente: compromisoCliente,
                        idAnio: idAnio,
                        mes: mes,
                        litros_mensual: litros_mensual,
                        litros_dia: litros_dia,
                        full_mensual: full_mensual
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se modifico el compromiso correctamente',
                            });
                            $("#div_tablas_compromiso").html('');
                            $("#selectAnio").prop('disabled', false);
                            $("#selectCliente").prop('disabled', false);
                            $("#btnCompromisoBuscar").prop('disabled', false);
                            $("#modalCompromiso").modal("hide");
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al registrar el compromiso',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                    }
                });
                jsRemoveWindowLoad();

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