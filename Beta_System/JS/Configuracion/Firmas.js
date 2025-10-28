function ConsultarCargo() {
    var id_usuario_firma = $("#id_usuario_cargo").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarCargo",
        data: {
            id_usuario_firma: id_usuario_firma
        },
        success: function (response) {
            $("#div_cargo_usuario").html(response);
            //ValidarFirmasUsuario(usuario_firma);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarFirmasUsuario(usuario_firma) {
    var checks = $(".check_cargo");
    checks.each(function () {
        var id_centro = $(this).attr("name");
        var centros_id = id_centro.split(' ')[0];
        var permiso_id = id_centro.split(' ')[1];
        $.ajax({
            type: "GET",
            async: false,
            url: "../CONFIGURACION/ValidarFirmasUsuario",
            data: {
                id_usuario: usuario_firma,
                centros_id: centros_id,
                permiso_id: permiso_id
            },
            success: function (response) {
                if (response == "True") { $("#" + centros_id + "\\ " + permiso_id + "").prop("checked", true); }
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    });

    var checks = $(".check_centro");
}

function AsignarFirmasUsuario() {
    var id_cargo = [];
    var id_permiso = [];
    var id_usuario = $("#id_usuario_cargo").val();
    
    var cont = 0;
    var checks = $(".check_cargo");
    checks.each(function () {
        if ($(this).prop("checked") == true) {
            id_permiso[cont] = $(this).attr("name");
            cont++;
        }
    });
    if (id_permiso.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarFirmasUsuario",
            data: {
                id_usuario: id_usuario,
                id_permiso: id_permiso
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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Es necesario seleccionar un permiso!',
        });
    }
}

function AsignarTodoFirmasUsuario() {
    var id_cargo = [];
    var id_permiso = [];
    var id_usuario = $("#id_usuario_cargo").val();

    var cont = 0;
    var checks = $(".check_cargo");
    checks.each(function () {
        id_permiso[cont] = $(this).attr("name");
        cont++;
    });
    if (id_permiso.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarFirmasUsuario",
            data: {
                id_usuario: id_usuario,
                id_permiso: id_permiso
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Permisos asignados correctamente!',
                    });
                    ConsultarCargo();
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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Es necesario seleccionar un permiso!',
        });
    }
}

function RemoverFirmasUsuario() {
    var id_usuario = $("#id_usuario_cargo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "RemoverFirmasUsuario",
        data: {
            id_usuario: id_usuario
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Permisos removidos correctamente!',
                });
                ConsultarCargo();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: '¡Ocurrió un problema al remover los permisos!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AsignarMontoUsuario() {
    jsShowWindowLoad();
    var id_monto = $('#id_monto').val();
    var id_usuario = $("#id_usuario_cargo").val();
    //var monto = $("#monto_usuario_1").val();
    var monto = parseFloat($("#monto_usuario_1").val());

    if (id_monto == 0 && !isNaN(monto) && monto > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "AsignarMontosUsuario",
            data: {
                id_usuario: id_usuario,
                monto: monto
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Monto asignado correctamente!',
                    });
                    ConsultarMontoUsuario();
                    jsRemoveWindowLoad();
                    $('#monto_confirmacion').css('display', 'none');
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar el monto!',
                    });
                    jsRemoveWindowLoad();
                    $('#monto_confirmacion').css('display', 'none');
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                $('#monto_confirmacion').css('display', 'none');
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: '¡El monto debe ser mayor a 0!',
        });
        $('#monto_confirmacion').css('display', 'none');
        jsRemoveWindowLoad();
    }
}
var monto_inicial = 0;
function ConsultarMontoUsuario() {
    accion_monto = 0;
    var id_usuario = $("#id_usuario_cargo").val();
    $("#monto_usuario_1").text('');
    $("#monto_usuario_1").text('');
    $.ajax({
        type: "GET",
        async: false,
        url: "../CONFIGURACION/ConsultarMontoUsuario",
        data: {
            id_usuario: id_usuario
        },
        success: function (response) {
            var data = $.parseJSON(response);
            if (data.length > 0) {
                $('#id_monto').val(data[0].id_usuario_montos);
                $("#monto_usuario_1").val(data[0].montos);
                monto_inicial = $("#monto_usuario_1").val();
                $("#monto_usuario_2").val(data[0].montos);                
            }
            else {
                $('#id_monto').val('0');
                $("#monto_usuario_1").val('0');
                $("#monto_usuario_2").val('0');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarMontoUsuario() {
    jsShowWindowLoad();
    var id_usuario_montos = $("#id_monto").val();
    var monto = $("#monto_usuario_1").val();
    if (id_usuario_montos > 0 && !isNaN(monto) && monto > 0) {
        if (monto_inicial != monto) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../CONFIGURACION/MofificarMontoUsuario",
                data: {
                    id_usuario_montos: id_usuario_montos,
                    monto: monto
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Monto actualizado corretamente!',
                        });
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al modificar el monto',
                        });
                    }
                    $('#monto_confirmacion').css('display', 'none');
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                    $('#monto_confirmacion').css('display', 'none');
                }
            });
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: '¡El monto es el mismo!',
            });
            $('#monto_confirmacion').css('display', 'none');
            jsRemoveWindowLoad();
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: '¡El monto debe ser mayor a 0!',
        });
        $('#monto_confirmacion').css('display', 'none');
        jsRemoveWindowLoad();
    }
    
}
var accion_monto = 0;
function ValidarMontoAut() {
    var id_monto = parseFloat($("#id_monto").val());
    var cantidad = parseFloat($("#monto_usuario_1").val());

    //AGREGAR
    if (id_monto == 0 && !isNaN(cantidad) && cantidad > 0) {
        $('#monto_confirmacion').css('display', 'block');
        accion_monto = 1;
    }
    //MODIFICAR
    else if (id_monto > 0 && !isNaN(cantidad) && cantidad > 0) {
        $('#monto_confirmacion').css('display', 'block');
        accion_monto = 2;
    }
    else {
        $('#monto_confirmacion').css('display', 'none');
    }
}

function AccionMontoRealiza() {
    if (accion_monto == 1) {
        AsignarMontoUsuario();
    }
    else if (accion_monto == 2) {
        ModificarMontoUsuario();
    }
    else { }
}