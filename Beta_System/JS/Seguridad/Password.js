function PasswordReset(id_usuario) {
    var antigua_contraseña = $("#contraseña_actual").val();
    var nueva_contraseña = $("#nueva_contraseña").val();
    var nueva_contraseña2 = $("#nueva_contraseña2").val();
    $.ajax({
        url: "../USUARIOLOGIN/ConsultarContraseñaActual",
        data: {
            id_usuario: id_usuario,
            contraseña: antigua_contraseña
        },
        success: function (result) {
            if (result == "True") {
                if (nueva_contraseña == "123456") {
                    iziToast.error({
                        title: 'La contrseña no puede ser la misma',
                        message: 'Intenta con otra',
                    });
                }
                else if (nueva_contraseña.length > 5) {
                    if (nueva_contraseña == nueva_contraseña2 && nueva_contraseña != "") {
                        $.ajax({
                            url: "../USUARIOLOGIN/PasswordReset",
                            data: {
                                id_usuario: id_usuario,
                                nueva_contraseña: nueva_contraseña
                            },
                            success: function (result) {
                                iziToast.show({
                                    icon: 'icon-person',
                                    timeout: false,
                                    title: 'Contraseña actualizada correctamente!',
                                    message: '',
                                    position: 'center', // bottomRight, bottomLeft, topRight, topLeft, topCenter, bottomCenter
                                    progressBarColor: '#2e87d7',
                                    buttons: [
                                        ['<button>Iniciar sesión</button>', function (instance, toast) {
                                            location.reload("../");
                                        }, true]
                                    ]
                                });
                            }
                        });
                    }
                    else {
                        iziToast.error({
                            title: 'Las contraseñas no coinciden.',
                            message: '',
                        });
                        $("#nueva_contraseña").css("border-color", "red");
                        $("#nueva_contraseña2").css("border-color", "red");
                    }
                }
                else {
                    $("#nueva_contraseña").css("border-color", "red");
                    iziToast.error({
                        title: 'La contraseña debe de tener minimo 6 caracteres.',
                        message: '',
                    });
                }
                $("#contraseña_actual").css("border-color", "gray");


            }
            else {
                iziToast.error({
                    title: 'La contraseña actual no es correcta.',
                    message: '',
                });
                $("#contraseña_actual").css("border-color", "red");
            }
        }
    });



}

function PasswordResetGeneral(id_usuario) {
    $.ajax({
        url: "../USUARIOLOGIN/PasswordResetGeneral",
        data: {
            id_usuario: id_usuario
        },
        success: function (result) {
            if (result == "True") {
                iziToast.success({
                    title: 'Reseteo de contraseña exitoso!',
                    message: 'La clave por defecto es: 123456',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un problema al resetear el pasword',
                    message: '',
                });
            }
        }
    });
}

function PasswordResetSencilla(id_usuario) {
    var nueva_contraseña2 = $("#nueva_contraseña2").val();
    var nueva_contraseña = $("#nueva_contraseña").val();
    if (nueva_contraseña.length > 3) {
        if (nueva_contraseña == nueva_contraseña2 && nueva_contraseña != "") {
            $.ajax({
                url: "../USUARIOLOGIN/PasswordReset",
                data: {
                    id_usuario: id_usuario,
                    nueva_contraseña: nueva_contraseña
                },
                success: function (result) {
                    if (result == "True") {
                        swal("", "Contraseña cambiada correctamente", "success");
                        $("#close_cambiar_contraseña").click();
                        $("#nueva_contraseña2").val("");
                        $("#nueva_contraseña").val("");
                    }
                }
            });
        }
        else {
            swal("", "Las contraseñas no coinciden", "warning");
            $("#nueva_contraseña").css("border-color", "red");
            $("#nueva_contraseña2").css("border-color", "red");
        }


    }
    else {
        $("#nueva_contraseña").css("border-color", "red");
        swal("", "La contraseña debe de tener minimo 4 caracteres", "warning");
    }



}