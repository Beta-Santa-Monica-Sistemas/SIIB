function ConsultarSubModulosModuloSelect() {
    var id_modulo = $("#id_modulo").val();
    jsShowWindowLoad();
    $.ajax({
        type: "GET",
        async: false,
        url: "../MODULOS/ConsultarSubModulosModuloSelect",
        data: { id_modulo: id_modulo },
        success: function (response) {
            $("#id_submodulo").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarModulosRol() {
    var id_rol = $("#id_rol_select").val();
    jsShowWindowLoad();
    $.ajax({
        type: "GET",
        async: true,
        url: "ConsultarSubmodulos",
        data: { id_rol: id_rol },
        success: function (response) {
            $("#div_modulos_submodulos").html(response);
            ValidarModulosRol(id_rol);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarModulosRol(id_rol) {
    var checks = $(".check_submodulo");
    checks.each(function () {
        var id_modulo_sub = $(this).attr("id");
        $.ajax({
            type: "GET",
            async: false,
            url: "ValidarModulosAsignadosRol",
            data: {
                id_rol: id_rol,
                id_modulos_sub: id_modulo_sub
            },
            success: function (response) {
                if (response == "True") { $("#" + id_modulo_sub + "").prop("checked", true); }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });
}

function AsignarModulosRol() {
    var id_sub_modulos = [];
    var id_rol = $("#id_rol_select").val();
    var checks = $("input[type='checkbox']:checked");
    checks.each(function () {
        id_sub_modulos.push($(this).attr('id'));
    });
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "AsignarModulosRol",
        data: {
            id_rol: id_rol,
            id_sub_modulos: id_sub_modulos
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Submodulos asignados correctamente!',
                });
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al asignar los submodulos',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}