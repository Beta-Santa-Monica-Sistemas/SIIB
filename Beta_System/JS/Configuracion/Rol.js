function AddEditRol(modo) {
    $("#txt_nobre_rol").css("border-color", "");
    //EDITAR
    if (modo == 1) {
        $("#lbl_add_edit_rol").text("EDITAR ROL");
        $("#btn_add_update_rol").text("ACTUALIZAR");
    }

    //AGREGAR
    else {
        $("#lbl_add_edit_rol").text("REGISTRAR ROL");
        $("#btn_add_update_rol").text("GUARDAR");
        $("#btn_add_update_rol").attr("onclick", "RegistrarRol();");
        $(".input_clear_rol").val('');
    }

    $("#m_add_edit_rol").modal("show");
}
function ConsultarRolesSistema() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ROLES/ConsultarRolesSistema",
        data: {},
        success: function (response) {
            //$("#table_roles_sistema").html(response);

            try {
                $("#datatable_roles_sistema").dataTable().fnDestroy();
            } catch (e) { }
            $("#table_roles_sistema").html(response);
            $('#datatable_roles_sistema').DataTable({
                keys: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}]
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarRolesSistemaSelect() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ROLES/ConsultarRoles",
        data: { id_status: 1 },
        success: function (response) {
            $("#id_rol").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function VerUsuariosRolTable(id_rol) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ROLES/VerUsuariosRolTable",
        data: { id_rol: id_rol },
        success: function (response) {
            $("#table_roles_admin").html(response);
            $("#m_usuarios_rol").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegistrarRol() {
    var nombres_rol = $("#txt_nobre_rol").val().trim();
    var nombre_rol = $("#txt_nobre_rol").val();
    var id_usuario_tipo = $("#id_tipo_usuario").val();
    if (nombres_rol.length > 4 && nombres_rol.replace(/\s/g, '').length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ROLES/RegistrarRol",
            data: {
                nombre_rol: nombre_rol,
                id_usuario_tipo: id_usuario_tipo
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'ROL registrado correctamente',
                    });
                    $("#m_add_edit_rol").modal("hide");
                    ConsultarRolesSistema();
                    ConsultarRolesSistemaSelect();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el ROL',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#txt_nobre_rol").css("border-color", "red");
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar un nombre al rol',
        });
    }
}

function EditarRol(id_rol, nombre_rol, id_tupo_usuario) {
    $("#txt_nobre_rol").val(nombre_rol);
    $("#btn_add_update_rol").attr("onclick", "ActualizarRol(" + id_rol + ")");
    $("#m_add_edit_rol").modal("show");
    $("#id_tipo_usuario").find('option[value="' + id_tupo_usuario + '"]').prop('selected', true);
}

function ActualizarRol(id_rol) {
    var nombres_rol = $("#txt_nobre_rol").val().trim();
    var nombre_rol = $("#txt_nobre_rol").val();
    var id_tipo_usuario = $("#id_tipo_usuario").val();
    if (nombres_rol.length > 4 && nombres_rol.replace(/\s/g, '').length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ROLES/ActualizarRol",
            data: {
                id_rol: id_rol,
                nombre_rol: nombre_rol,
                id_tipo_usuario: id_tipo_usuario
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'ROL actualizado corretamente!',
                    });
                    $("#m_add_edit_rol").modal("hide");
                    ConsultarRolesSistema();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al actualizar el ROL',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#txt_nobre_rol").css("border-color", "red");
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar un nombre al rol',
        });
    }
}

function LimpiarKeyPressBorder() {
    $("#txt_nobre_rol").css("border-color", "");
}