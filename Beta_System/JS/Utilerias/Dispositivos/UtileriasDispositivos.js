function FechaParametros() {
    const hoy = new Date();
    const haceUnaSemana = new Date();
    haceUnaSemana.setDate(hoy.getDate() - 7);

    const formato = d => d.toISOString().slice(0, 10);

    $('#fecha_i').val(formato(haceUnaSemana));
    $('#fecha_f').val(formato(hoy));
}

function OnOffBloqueoDispositivo(id_dispositivo, estatus) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/OnOffBloqueoDispositivo",
        data: {
            id_dispositivo: id_dispositivo,
            estatus: estatus
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Cambio realizado correctamente!',
                });
                DispositivosActivosTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al realizar el cambio',
                });
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function OnOffEstatusDispositivo(id_dispositivo, estatus) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/OnOffEstatusDispositivo",
        data: {
            id_dispositivo: id_dispositivo,
            estatus: estatus
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Cambio realizado correctamente!',
                });
                DispositivosActivosTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al realizar el cambio',
                });
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function FalloDispositivo(id_dispositivo) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Aviso',
        message: '¿Estas seguro de reiniciar el dispositivo?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../UTILERIAS/FalloDispositivo",
                    data: {
                        id_dispositivo: id_dispositivo
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: '¡Cambio realizado correctamente!',
                            });
                            DispositivosActivosTable();
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al realizar el cambio',
                            });
                        }
                        jsRemoveWindowLoad();
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                    }
                });
                jsRemoveWindowLoad();
            }, true],
            ['<button>NO</button>', function (instance, toast) {
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
    jsRemoveWindowLoad();
}

function DispositivosActivosTable() {
    var id_tipo_dispositivo = $("#id_tipo_dispositivo").val();
    var id_establo = $("#id_establo").val();
    var id_estatus = $("#id_estatus").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/DispositivosActivosTable",
        data: {
            id_tipo_dispositivo: id_tipo_dispositivo,
            id_establo: id_establo,
            id_estatus: id_estatus
        },
        success: function (response) {
            try {
                $("#dispositivos_activos_table").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_dispositivos_activos_table").html(response);
            $('#dispositivos_activos_table').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DispositivosGlobalesTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/DispositivosGlobalesTable",
        data: {
        },
        success: function (response) {
            $("#div_dispositivos_comandos_table").html(response);

            $('.dispositivos_lista_table').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });

            $('#dispositivos_lista_comandos_table').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });

            $('#dispositivos_lista_ejecutados_table').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });



            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ModalNuevoComando() {
    //$("#m_nuevo_comando").modal("hide");
    $("#m_nuevo_comando").modal("show");
}

function EjecutarComandoGlobal() {
    var id_comando = $("#id_comando").val();
    var id_comando_tipo = $("#id_comando_tipo").val();
    var comando_mac = $("#comando_mac").val();
    var comando_parametro = $("#comando_parametro").val();
    var perpetuo = false;
    if ($("#perpetuo").is(':checked')) { perpetuo = true; }
    var general = false;
    if ($("#general").is(':checked')) { general = true; }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/EjecutarComandoGlobal",
        data: {
            id_comando: id_comando,
            id_comando_tipo: id_comando_tipo,
            comando_mac: comando_mac,
            perpetuo: perpetuo,
            general: general,
            comando_parametro: comando_parametro
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Comando pendiente por ejecutar!',
                });
                LimpiarRegistroComando();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema registrar el comando',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function LimpiarRegistroComando() {
    $("#comando_mac").val('');
    $("#comando_parametro").val('');

    $("#id_comando").prop("selectedIndex", 0);
    $("#id_comando_tipo").prop("selectedIndex", 0);

    $("#perpetuo").prop('checked', false);
    $("#general").prop('checked', false);
}

function DispositivosLogsTable() {
    var logs_mac = $("#logs_mac").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();

    if (fecha_inicio == "") {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Favor de seleccionar una fecha inicio valida!',
        });
        return;
    }

    if (fecha_fin == "") {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Favor de seleccionar una fecha fin valida!',
        });
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/DispositivosLogsTable",
        data: {
            logs_mac: logs_mac,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#dispositivos_logs_table").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_dispositivos_logs_table").html(response);
            $('#dispositivos_logs_table').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ModalDesactivarComando() {
    $("#m_desactivar_comando").modal("show");
    DispositivosAdmComandosTable();
}

function DispositivosAdmComandosTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/DispositivosAdmComandosTable",
        data: {

        },
        success: function (response) {
            try {
                $("#dispositivos_desactivar_comandos_table").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_desactivar_table").html(response);
            $('#dispositivos_desactivar_comandos_table').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function OnOffComandoDispositivo(id_comando, estatus) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/OnOffComandoDispositivo",
        data: {
            id_comando: id_comando,
            estatus: estatus
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Cambio realizado correctamente!',
                });
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al realizar el cambio',
                });
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
