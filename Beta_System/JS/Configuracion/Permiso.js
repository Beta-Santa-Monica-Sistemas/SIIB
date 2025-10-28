function ConsultarPermisosUsuario() {
    var id_submodulo = $("#id_submodulo").val();
    var id_usuario = $("#id_usuario_select").val();
    $.ajax({
        type: "GET",
        async: true,
        url: "../PERMISOS/ConsultarPermisosUsuario",
        data: { id_submodulo: id_submodulo },
        success: function (response) {
            $("#div_permisos_usuario").html(response);
            ValidarPermisosUsuario(id_usuario, id_submodulo);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ValidarPermisosUsuario(id_usuario) {
    var checks = $(".check_submodulo");
    checks.each(function () {
        var id_permiso = $(this).attr("id");
        $.ajax({
            type: "GET",
            async: false,
            url: "../PERMISOS/ValidarPermisoUsuario",
            data: {
                id_usuario: id_usuario,
                id_permiso: id_permiso
            },
            success: function (response) {
                if (response == "True") { $("#" + id_permiso + "").prop("checked", true); }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    });
}

function AsignarPermisosUsuario() {
    var id_permiso = [];
    var id_usuario = $("#id_usuario_select").val();
    var id_submodulo = $("#id_submodulo").val();
    var checks = $("input[type='checkbox']:checked");
    var cont = 0;
    checks.each(function () {
        id_permiso[cont] = ($(this).attr('id'));
        cont++;
    });
    $.ajax({
        type: "POST",
        async: false,
        url: "AsignarPermisoUsuario",
        data: {
            id_usuario: id_usuario,
            id_permiso: id_permiso,
            id_submodulo: id_submodulo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Permisos asignados correctamente!',
                });
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al asignar los permisos!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RemoverPermisosUsuario() {
    var id_usuario = $("#id_usuario_select").val();
    var id_submodulo = $("#id_submodulo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "RemoverPermisoUsuario",
        data: {
            id_usuario: id_usuario,
            id_submodulo: id_submodulo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Permisos removidos correctamente!',
                });
                ConsultarPermisosUsuario();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al asignar los permisos!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}