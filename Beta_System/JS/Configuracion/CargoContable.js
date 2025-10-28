function ConsultarCargosContables() {
    var usuario_cargocontable = $("#id_usuario_cargocontable").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarCargosContables",
        data: {
        },
        success: function (response) {
            $("#div_cargocontable_usuario").html(response);
            jsRemoveWindowLoad();
            ValidarCargosContablesUsuario(usuario_cargocontable);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarCargosContablesUsuario(usuario_cargocontable) {
    var checks = $(".check_cargo");
    checks.each(function () {
        var id_cargocontable = $(this).attr("id");
        $.ajax({
            type: "GET",
            async: false,
            url: "../CONFIGURACION/ValidarCargosContablesUsuario",
            data: {
                id_usuario: usuario_cargocontable,
                id_cargocontable: id_cargocontable
            },
            success: function (response) {
                if (response == "True") { $("#" + id_cargocontable + "").prop("checked", true); }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });

    var checks = $(".check_cargo");
}

function AsignarCargosContablesUsuario() {
    var id_cargocontable = [];
    var id_usuario = $("#id_usuario_cargocontable").val();
    var checks = $("input[type='checkbox']:checked");
    var cont = 0;
    checks.each(function () {
        id_cargocontable[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_cargocontable.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarCargosContablesUsuario",
            data: {
                id_usuario: id_usuario,
                id_cargocontable: id_cargocontable
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Cargos contables asignados correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar los cargos contables!',
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

function AsignarTodoCargoContableUsuario() {
    var id_cargocontable = [];
    var id_usuario = $("#id_usuario_cargocontable").val();
    var checks = $("input[type='checkbox']");
    var cont = 0;
    checks.each(function () {
        id_cargocontable[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_cargocontable.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarTodoCargoContableUsuario",
            data: {
                id_usuario: id_usuario,
                id_cargocontable: id_cargocontable
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Centro asignados correctamente!',
                    });
                    ConsultarCargosContables();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar los cargos contables!',
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
            message: '¡Es necesario seleccionar un cargo contable!',
        });
    }
}

function RemoverCargoContableUsuario() {
    var id_usuario = $("#id_usuario_cargocontable").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "RemoverCargoContableUsuario",
        data: {
            id_usuario: id_usuario
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Cargos contables removidos correctamente!',
                });
                ConsultarCargosContables();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al remover los cargos contables!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}