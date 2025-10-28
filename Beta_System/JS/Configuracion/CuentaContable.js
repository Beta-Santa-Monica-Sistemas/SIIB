function LimparCuentasUsuario() {
    $("#div_cuenta_contable_usuario").html('');
}

function ConsultarCuentasContablesUsuario() {
    var id_usuario = $("#id_usuario_cuenta_contable").val();
    var cuenta_contable_buscar = $("#cuenta_contable_buscar").val();

    //var id_cargo = $("#select_cargo").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarCuentasContablesUsuario",
        data: {
            id_usuario: id_usuario,
            cuenta_contable_buscar: cuenta_contable_buscar
        },
        success: function (response) {
            $("#div_cuenta_contable_usuario").html(response);
            jsRemoveWindowLoad();
            //ValidarCuentasContablesUsuario(id_usuario_cuenta_contable);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CheckAllCuentasCargo(id_cargo) {
    if ($("#check_todas_" + id_cargo + "").is(":checked")) {
        $(".check_cargo_" + id_cargo + "").prop("checked", true);
    }
    else {
        $(".check_cargo_" + id_cargo + "").prop("checked", false);
    }
}



function AsignarCuentasContablesUsuario() {
    var id_cuentas_contables = [];
    var id_usuario = $("#id_usuario_cuenta_contable").val();
    var checks = $("input[name='check_cuenta']:checked");
    var cont = 0;
    checks.each(function () {
        id_cuentas_contables[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_cuentas_contables.length > 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../CONFIGURACION/AsignarCuentasContablesUsuario",
            data: {
                id_usuario: id_usuario,
                id_cuentas_contables: id_cuentas_contables
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Cuentas contables asignados correctamente!',
                    });
                    ConsultarCuentasContablesUsuario();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar las cuentas contables!',
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
            message: '¡Es necesario seleccionar una cuenta!',
        });
    }
}

function AsignarTodoCuentasContableUsuario() {
    var id_cargo = $("#select_cargo").val();
    var id_cuenta_contable = [];
    var id_usuario = $("#id_usuario_cuenta_contable").val();
    var checks = $("input[name='check_cuenta']");
    var cont = 0;
    checks.each(function () {
        id_cuenta_contable[cont] = ($(this).attr('id'));
        cont++;
    });
    if (id_cuenta_contable.length > 0) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 999,
            title: 'ATENCIÓN',
            message: '¿Está seguro de asignar todas las cuentas contables?',
            position: 'center',
            buttons: [
                ['<button><b>Si, asignar todo</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 90000,
                        url: "AsignarTodoCuentasContableUsuario",
                        data: {
                            id_usuario: id_usuario,
                            id_cuenta_contable: id_cuenta_contable
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == "True") {
                                iziToast.success({
                                    title: 'Exito',
                                    message: '¡Cuentas asignadas correctamente!',
                                });
                                ConsultarCuentasContablesUsuario();
                            }
                            else {
                                iziToast.error({
                                    title: 'Error',
                                    message: '¡Ocurrió un problema al asignar las cuentas contables!',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }, true],
                ['<button>No, regresar</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }],
            ],
            onClosing: function (instance, toast, closedBy) {
                console.info('Closing | closedBy: ' + closedBy);
            },
            onClosed: function (instance, toast, closedBy) {
                console.info('Closed | closedBy: ' + closedBy);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Es necesario seleccionar una cuenta contable!',
        });
    }
}

function RemoverCuentaContableUsuario() {
    var id_usuario = $("#id_usuario_cuenta_contable").val();
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de remover todas las cuentas contables del usurio?',
        position: 'center',
        buttons: [
            ['<button><b>Si, remover</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    timeout: 900000,
                    url: "RemoverCuentasContableUsuario",
                    data: {
                        id_usuario: id_usuario
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: '¡Cuentas contables removidas correctamente!',
                            });
                            ConsultarCuentasContablesUsuario();
                        }
                        else {
                            iziToast.warning({
                                title: 'Error',
                                message: '¡Ocurrió un problema al remover las cuentas contables!',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }, true],
            ['<button>No, regresar</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }],
        ],
        onClosing: function (instance, toast, closedBy) {
            console.info('Closing | closedBy: ' + closedBy);
        },
        onClosed: function (instance, toast, closedBy) {
            console.info('Closed | closedBy: ' + closedBy);
        }
    });
}