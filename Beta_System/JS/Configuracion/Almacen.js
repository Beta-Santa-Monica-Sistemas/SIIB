function ConsultarAlmacenesUsuario() {
    jsShowWindowLoad();
    var id_usuario_almacen = $("#id_usuario_almacen").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarAlmacenesUsuario",
        data: {
            id_usuario_almacen: id_usuario_almacen
        },
        success: function (response) {
            $("#div_almacenes_usuario").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GuardarAlmacenesUsuario() {
    var count = 0;
    var id_almacenes = [];
    var id_usuario_almacen = $("#id_usuario_almacen").val();
    var checks = $(".check_almacen");
    checks.each(function () {
        if ($(this).prop("checked") == true) {
            id_almacenes[count] = $(this).attr("name");
            count++;
        }
    });
    if (id_almacenes.length <= 0) {
        iziToast.error({
            title: 'Selecciona al menos 1 almacen',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../CONFIGURACION/GuardarAlmacenesUsuario",
            data: {
                id_usuario_almacen: id_usuario_almacen,
                id_almacenes: id_almacenes
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Almacen asignado corretamente!',
                    });
                    ConsultarAlmacenesUsuario();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema asignar el almacen',
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

function RemoverAlmacenesUsuario() {
    var id_usuario_almacen = $("#id_usuario_almacen").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/RemoverAlmacenesUsuario",
        data: {
            id_usuario_almacen: id_usuario_almacen
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Almacén removido del usuario correctamente!',
                });
                ConsultarAlmacenesUsuario();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al remover el almacen del usuario',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#region UBICACION DE ENTREGA DE ALMACEN
function ConsultarUbicacionesEntrega() {
    jsShowWindowLoad();
    var id_almacen = $("#id_almacen").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ConsultarUbicacionAlmacen",
        data: {
            id_almacen: id_almacen
        },
        success: function (response) {
            $("#div_ubicaciones_entrega").html(response);
            jsRemoveWindowLoad();
            ValidarUbicacionEntrega(id_almacen);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AsignarUbicacionAlmacenEntrega(modo) {
    var count = 0;
    var id_ubicacion_entrega = [];
    var id_almacen = $("#id_almacen").val();
    var checks = $(".check_almacen");
    if (modo == 1) {
        checks.each(function () {
            if ($(this).prop("checked") == true) {
                id_ubicacion_entrega[count] = $(this).attr("id");
                count++;
            }
        });
    }
    else if (modo == 2) {
        checks.each(function () {
            id_ubicacion_entrega[count] = $(this).attr("id");
            count++;
        });
    }
    if (id_almacen.length <= 0) {
        iziToast.error({
            title: 'Selecciona al menos 1 almacen',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../CONFIGURACION/AsignarUbicacionAlmacenEntrega",
            data: {
                id_almacen: id_almacen,
                id_ubicacion_entrega: id_ubicacion_entrega
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Ubicaciones de entrega asignados corretamente!',
                    });
                    if (modo == 2) {
                        ConsultarUbicacionesEntrega();
                    }
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema asignar las ubicaciones de entrega',
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

function ValidarUbicacionEntrega(id_almacen) {
    var checks = $(".check_almacen");
    var id_ubicacion = [];
    var contador = 0;
    checks.each(function () {
        id_ubicacion[contador] = $(this).attr("id");
        contador++;
    });
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ValidarUbicacionEntrega",
        data: {
            id_almacen: id_almacen,
            id_ubicacion: id_ubicacion
        },
        success: function (response) {
            if (response != "" && response != null) {
                response.forEach(function (id_ubicacion) {
                    $("#" + id_ubicacion).prop("checked", true);
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion