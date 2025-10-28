
//----------------- USUARIOS
function AddEditUsuario(modo) {
    $("#usuario").css("border-color", "");
    //EDITAR
    if (modo == 1) {
        $("#lbl_add_edit_usuario").text("EDITAR USUARIO");
        $("#btn_add_edit_usuario").text("ACTUALIZAR");
        $("#btn_add_edit_usuario_cancel").css("display", "block");
    }

    //AGREGAR
    else {
        $("#lbl_add_edit_usuario").text("REGISTRAR EMPLEADO");
        $("#btn_add_edit_usuario").text("REGISTRAR");
        //$("#btn_add_edit_usuario").attr("onclick", "RegistrarUsuario();");
        $(".input_clear_emp").val('');
        $("#btn_add_edit_usuario_cancel").css("display", "none");
    }

    $("#m_add_edit_usuario").modal("show");
}

function ConsultarUsuariosEmpleado(id_empleado) {
    $("#usuario").css("border-color", "");
    $.ajax({
        type: "POST",
        async: false,
        url: "../USUARIOS/ConsultarUsuariosEmpleado",
        data: {
            id_empleado: id_empleado
        },
        success: function (response) {
            $("#table_usuarios_admin").html(response);
            $("#m_add_edit_usuario").modal("show");
            $("#btn_add_edit_usuario").attr("onclick", "RegistrarUsuario(" + id_empleado + ")");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegistrarUsuario(id_empleado) {
    var c_usuario = {};
    var usuario = $("#usuario").val().trim();
    c_usuario.usuario = $("#usuario").val();
    c_usuario.id_rol = $("#id_rol").val();
    c_usuario.id_empleado = id_empleado;
    if (usuario.length > 4 && usuario.replace(/\s/g, '').length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../USUARIOS/RegistrarUsuario",
            data: {
                c_usuario: c_usuario
            },
            success: function (response) {
                if (response == 0) {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Usuario registrado corretamente!',
                    });
                    ConsultarUsuariosEmpleado(id_empleado);
                    ConsultarEmpleadosSistema();
                    $("#usuario").val('');
                    $("#usuario").css("border-color", "");
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Tu usuario no cuenta con el permiso para realizar esta accion',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el usuario',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#usuario").css("border-color", "red");
        iziToast.warning({
            title: 'Ingrese un usuario valido',
            message: 'Asegurese de que el usuario tenga mas de 8 letras, (no introducir caracteres especiales)',
        });
    }
}

function EditarUsuario(id_usuario, usuario, id_rol) {
    $("#usuario").css("border-color", "");
    $("#id_rol").find('option[value="' + id_rol + '"]').prop('selected', true);
    $("#usuario").val(usuario);
    $("#btn_add_edit_usuario").attr("onclick", "ActualizarUsuario(" + id_usuario + ")");
}

function ActualizarUsuario(id_usuario) {
    var usuarios = $("#usuario").val().trim();
    var usuario = $("#usuario").val();
    var id_rol = $("#id_rol").val();
    if (usuarios.length > 4 && usuarios.replace(/\s/g, '').length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../USUARIOS/ActualizarUsuario",
            data: {
                id_usuario: id_usuario,
                usuario: usuario,
                id_rol: id_rol
            },
            success: function (response) {
                if (response == 0) {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Usuario actualizado corretamente!',
                    });
                    $("#m_add_edit_usuario").modal("hide");
                    $("#usuario").val('');
                    $("#usuario").css("border-color", "");
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Tu usuario no cuenta con el permiso para realizar esta accion',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el usuario',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#usuario").css("border-color", "red");
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar un usuario valido',
        });
    }
}

function OnOffUsuario(id_usuario, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../USUARIOS/OnOffUsuario",
        data: {
            id_usuario: id_usuario,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de status correctamente',
                });
                $("#m_add_edit_usuario").modal("hide");
                $("#m_usuarios_rol").modal("hide");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el status',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
function LimpiarKeyPressUSU() {
    $("#usuario").css("border-color", "");
}