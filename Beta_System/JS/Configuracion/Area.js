function ConsultarAreasNomina() {
    var usuario_area = $("#id_usuario_area").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarAreasNomina",
        data: {
        },
        success: function (response) {
            $("#div_areas_usuario").html(response);
            jsRemoveWindowLoad();
            ValidarAreasNominaUsuario(usuario_area);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarAreasNominaUsuario(usuario_area) {
    var checks = $(".check_area");
    checks.each(function () {
        var id_area_nomina = $(this).attr("id");
        $.ajax({
            type: "GET",
            async: false,
            url: "../CONFIGURACION/ValidarAreasNominaUsuario",
            data: {
                id_usuario: usuario_area,
                id_area_nomina: id_area_nomina
            },
            success: function (response) {
                if (response == "True") { $("#" + id_area_nomina + "").prop("checked", true); }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });
    var checks = $(".check_area");
}

function AsignarAreasNominaUsuario() {
    var id_area_nomina = [];
    var id_usuario = $("#id_usuario_area").val();
    var checks = $("input[type='checkbox']:checked");
    var cont = 0;
    checks.each(function () {
        id_area_nomina[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_area_nomina.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarAreasNominaUsuario",
            data: {
                id_usuario: id_usuario,
                id_area_nomina: id_area_nomina
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Area asignadas correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar las areas!',
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
            message: '¡Es necesario seleccionar una area!',
        });
    }
}

function AsignarTodoAreasNominaUsuario() {
    var id_area_nomina = [];
    var id_usuario = $("#id_usuario_area").val();
    var checks = $("input[type='checkbox']");
    var cont = 0;
    checks.each(function () {
        id_area_nomina[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_area_nomina.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarTodoAreasNominaUsuario",
            data: {
                id_usuario: id_usuario,
                id_area_nomina: id_area_nomina
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Areas asignadas correctamente!',
                    });
                    ConsultarAreasNomina();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar las areas!',
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
            message: '¡Es necesario seleccionar un area!',
        });
    }
}

function RemoverAreasNominaUsuario() {
    var id_usuario = $("#id_usuario_area").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "RemoverAreasNominaUsuario",
        data: {
            id_usuario: id_usuario
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Areas removidas correctamente!',
                });
                ConsultarAreasNomina();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al remover las areas!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}