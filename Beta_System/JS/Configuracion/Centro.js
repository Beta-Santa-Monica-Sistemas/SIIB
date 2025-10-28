function ConsultarCentros() {    
    var usuario_centro = $("#id_usuario_centro").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarCentros",
        data: {
        },
        success: function (response) {
            $("#div_centro_usuario").html(response);
            jsRemoveWindowLoad();
            ValidarCentroUsuario(usuario_centro);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarCentroUsuario(usuario_centro) {
    var checks = $(".check_centro");
    checks.each(function () {
        var id_centro = $(this).attr("id");
        $.ajax({
            type: "GET",
            async: false,
            url: "../CONFIGURACION/ValidarCentroUsuario",
            data: {
                id_usuario: usuario_centro,
                id_centro: id_centro
            },
            success: function (response) {
                if (response == "True") { $("#" + id_centro + "").prop("checked", true); }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });

    var checks = $(".check_centro");
}

function AsignarCentroUsuario() {
    var id_centro = [];
    var id_usuario = $("#id_usuario_centro").val();
    var checks = $("input[type='checkbox']:checked");
    var cont = 0;
    checks.each(function () {
        id_centro[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_centro.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarCentroUsuario",
            data: {
                id_usuario: id_usuario,
                id_centro: id_centro
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Centro asignados correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar los centros!',
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
            message: '¡Es necesario seleccionar un centro!',
        });
    }
}

function AsignarTodoCentroUsuario() {
    var id_centro = [];
    var id_usuario = $("#id_usuario_centro").val();
    var checks = $("input[type='checkbox']");
    var cont = 0;
    checks.each(function () {
        id_centro[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_centro.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarTodoCentroUsuario",
            data: {
                id_usuario: id_usuario,
                id_centro: id_centro
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Centro asignados correctamente!',
                    });
                    ConsultarCentros();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar los centros!',
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
            message: '¡Es necesario seleccionar un centro!',
        });
    }
}

function RemoverCentroUsuario() {
    var id_usuario = $("#id_usuario_centro").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "RemoverCentroUsuario",
        data: {
            id_usuario: id_usuario
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Centros removidos correctamente!',
                });
                ConsultarCentros();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al remover los centros!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}