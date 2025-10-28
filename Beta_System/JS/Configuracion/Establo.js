function ConsultarUsuariosEstablos() {
    var id_rol = $("#id_rol_select").val();
    jsShowWindowLoad();
    $.ajax({
        type: "GET",
        async: true,
        url: "../CONFIGURACION/ConsultarUsuariosEstablos",
        data: { id_rol: id_rol },
        success: function (response) {
            $("#div_usuarios_establos").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GuardarUsuariosEstablos() {
    var count = 0;
    var id_usuarios = [];
    var id_establos = [];
    var status = [];

    var checks = $(".check_us");
    checks.each(function () {
        var valor = $(this).attr('id').split('|')
        id_usuarios[count] = valor[0];
        id_establos[count] = valor[1];

        if ($(this).is(':checked')) {
            status[count] = true;
        } else {
            status[count] = false;
        }
        count++;
    });
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/GuardarUsuariosEstablos",
        data: {
            id_usuarios: id_usuarios,
            id_establos: id_establos,
            status: status
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambios aplicados correctamente!',
                });
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema aplicar los cambios',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });

}