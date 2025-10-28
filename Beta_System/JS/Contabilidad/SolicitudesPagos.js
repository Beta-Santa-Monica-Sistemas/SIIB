function ConsultarPagosProgramados() {
    var fecha_inicio = $("#fecha_inicio_pagos").val();
    var fecha_fin = $("#fecha_fin_pagos").val();
    var clave_fiscal_ba = $("#clave_fiscal_pagos").val();
    jsShowWindowLoad();
    $.ajax({
        url: "../CONTABILIDAD/ConsultarPagosProgramados",
        async: true,
        type: "POST",
        timeout: 90000,
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            clave_fiscal_ba: clave_fiscal_ba
        },
        success: function (result) {
            jsRemoveWindowLoad();
            $("#div_pagos_pendientes").html(result);
            $("#fecha_solicitud_pago").val(today);
        }
    });
}

function GuardarSolicitudPagos() {
    var clave_fiscal_ba = $("#clave_fiscal_pagos").val();
    var fecha_solicitud = $("#fecha_solicitud_pago").val();
    var referencia_solicitud = $("#referencia_solicitud_pago").val();
    var cuenta_solicitud = $("#cuenta_origen_solicitud_pago").val();

    var id_doctos_ba = [];
    var id_doctos_cp = [];

    var conceptos_id = [];
    var conceptos = [];

    var monedas_id = [];
    var monedas_cf = [];

    var benef_id = [];
    var benef = [];

    var cta_bancaria = [];
    var tipo_persona = [];

    var referencias = [];
    var nombre_pf = [];

    var importes = [];
    var descripciones = [];

    var count = 0;
    $(".check_pago").each(function () {
        if ($(this).is(':checked')) {
            var id_docto_ba = $(this).val();
            var id_docto_cp = $(this).attr("name").split('_')[1];

            id_doctos_ba[count] = id_docto_ba;
            id_doctos_cp[count] = id_docto_cp;

            conceptos_id[count] = $("#conceptoID_" + id_docto_ba + "").val();
            conceptos[count] = $("#conceptoID_" + id_docto_ba + "").attr("name").split('_')[1];

            monedas_id[count] = $("#monedaID_" + id_docto_ba + "").val();
            monedas_cf[count] = $("#monedaID_" + id_docto_ba + "").attr("name").split('_')[1];

            benef_id[count] = $("#beneficiarioID_" + id_docto_ba + "").val();
            benef[count] = $("#beneficiarioID_" + id_docto_ba + "").attr("name").split('_')[1];

            cta_bancaria[count] = $("#datosFiscales_" + id_docto_ba + "").val();
            tipo_persona[count] = $("#datosFiscales_" + id_docto_ba + "").attr("name").split('_')[1];

            referencias[count] = $("#datosBA_" + id_docto_ba + "").val();
            nombre_pf[count] = $("#provPF_" + id_docto_ba + "").val();

            importes[count] = $("#importes_" + id_docto_ba + "").val();
            descripciones[count] = $("#importes_" + id_docto_ba + "").attr("name").split('_')[1];
            count++;
        }
    });

    var importe_total = parseFloat($("#lbl_total_importe").text().replace(/[^\d.]/g, ''));

    var sol_g = {};
    sol_g.importe = importe_total;
    sol_g.numero_cuenta = cuenta_solicitud;
    sol_g.moneda_clave_fiscal = clave_fiscal_ba;
    sol_g.referencia = referencia_solicitud;
    sol_g.id_solicitudes_pago_status = 1;  //REGISTRADA
    sol_g.activo = true;

    if (cuenta_solicitud == "") {
        iziToast.warning({
            title: 'Ingrese la cuenta origen de la solicitud',
            message: '',
        });
    }
    else if (referencia_solicitud == "") {
        iziToast.warning({
            title: 'Ingrese la referencia de la solicitud',
            message: '',
        });
    }
    else if (importe_total == "" || isNaN(importe_total) || importe_total == 0) {
        iziToast.warning({
            title: 'Revise el importe total',
            message: '',
        });
    }
    else if (id_doctos_ba.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 pago para registrar la solicitud',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            url: "../CONTABILIDAD/GuardarSolicitudPagos",
            async: true,
            timeout: 90000,
            type: "POST",
            data: {
                sol_g: sol_g,
                fech_sol: fecha_solicitud,
                id_doc_ba: id_doctos_ba,
                id_doc_cp: id_doctos_cp,
                conceptos_id: conceptos_id,
                conceptos: conceptos,
                monedas_id: monedas_id,
                monedas_cf: monedas_cf,
                benef_id: benef_id,
                benef: benef,
                cta_bancaria: cta_bancaria,
                tipo_persona: tipo_persona,
                referencias: referencias,
                nombre_pf: nombre_pf,
                importes: importes,
                descripciones: descripciones
            },
            success: function (result) {
                jsRemoveWindowLoad();
                if (result == 0) {
                    iziToast.success({
                        title: 'Solicitud generada correctamente',
                        message: '',
                    });
                    LimpiarFormularioSolicitud();
                    ConsultarSolicitudesPendientesValidarAutorizar(1, 1);
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error el guardar la solicitud',
                        message: '',
                    });
                }
            },
            error: function (xhr, status, error) {
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Ocurrió un error el guardar la solicitud',
                    message: error,
                });
            }
        });
    }


}

function CalcularImporteTotal(modo) {
    var importe_total = 0;
    if (modo == 1) {  //GENERAR SOLICITUD
        $(".check_pago").each(function () {
            if ($(this).prop("checked") == true) {
                var id_docto_ba = $(this).val();
                importe_total += parseFloat($("#importes_" + id_docto_ba + "").val());
            }
        });
        $("#lbl_total_importe").text(importe_total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
    }
    else {  //AUTORIZACIONES (VALIDAR, AUTORIZAR)
        $(".check_aut_valid").each(function () {
            if ($(this).prop("checked") == true) {
                //var id_docto_ba = $(this).attr("id").split('_')[1];
                importe_total += parseFloat($(this).attr('name'));
            }
        });
        $("#lbl_total_importe_aut").text(importe_total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
    }
}


function LimpiarFormularioSolicitud() {
    $("#div_pagos_pendientes").html("");
    //$('input[type="text"]').val('')
    $('.input_clear').val('')
}

function ConsultarSolicitudesPendientesValidarAutorizar(id_status, modo) {
    $.ajax({
        url: "../CONTABILIDAD/ConsultarSolicitudesPendientesValidarAutorizar",
        async: false,
        type: "POST",
        data: {
            id_status: id_status,
            modo: modo
        },
        success: function (result) {
            if (modo == 1) { $("#div_solciitudes_validar").html(result); }
            if (modo == 2) { $("#div_solciitudes_autorizar").html(result); }
            if (modo == 3) { $("#div_solciitudes_pago").html(result); }
        }
    });
}

function ConsultarSolicitudPagoDetalle(modo, id_solicitud_g) {
    $.ajax({
        url: "../CONTABILIDAD/ConsultarSolicitudPagoDetalle",
        async: false,
        type: "POST",
        data: {
            modo: modo,
            id_solicitud_g: id_solicitud_g
        },
        success: function (result) {
            $("#div_solciitudes_validar_autorizar_detalle").html(result);
            $("#m_detalle_solicitud").modal("show");
        }
    });
}

function ValidarAutorizarSolicitud(modo, id_solicitud_g, accion) {
    var importe = parseFloat($("#lbl_total_importe_aut").text());
    var id_solicitud_d = [];
    var valores = [];
    var count = 0;
    $(".check_aut_valid").each(function () {
        id_solicitud_d[count] = $(this).val();
        if (accion == true) {
            if ($(this).is(':checked')) { valores[count] = true; }
            else { valores[count] = false; }
        }
        else {
            if ($(this).is(':checked')) { valores[count] = false; }
            else { valores[count] = true; }
        }
        count++;
    });

    if (id_solicitud_d <= 0 && accion == true) {
        iziToast.warning({
            title: 'Selecciona al menos 1 pago',
            message: '',
        });
        return;
    }
    else if (importe <= 0 && accion == true) {
        iziToast.warning({
            title: 'El importe no puede ser 0 o una cantidad menor',
            message: '',
        });
        return;
    }
    else {
        var msj_autoriza = "";
        if (modo == 1) { msj_autoriza = "¿Está seguro de validar la solicitud?"; }
        if (modo == 2) { msj_autoriza = "¿Está seguro de autorizar la solicitud?"; }
        if (accion == false) { msj_autoriza = "¿Está seguro de rechazar la solicitud?"; }
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 9999,
            title: 'ADVERTENCIA',
            message: '' + msj_autoriza + '',
            position: 'center',
            buttons: [
                ['<button><b>Si, ejecutar acción</b></button>', function (instance, toast) {
                    $.ajax({
                        url: "../CONTABILIDAD/ValidarAutorizarSolicitud",
                        async: false,
                        type: "POST",
                        data: {
                            id_solicitud_g: id_solicitud_g,
                            modo: modo,
                            accion: accion,
                            id_solicitud_d: id_solicitud_d,
                            valores: valores
                        },
                        success: function (result) {
                            if (result == 0) {
                                $("#m_detalle_solicitud").modal("hide");

                                if (modo == 1) {
                                    if (accion == true) {
                                        iziToast.success({
                                            title: 'Los pagos seleccionados se validaron correctamente',
                                            message: 'Los pagos no seleccionados se rechazaron',
                                        });
                                    }
                                    else {
                                        iziToast.success({
                                            title: 'Los pagos seleccionados se rechazaron correctamente',
                                            message: 'Los pagos no seleccionados pasaron a autorización',
                                        });
                                    }
                                    ConsultarSolicitudesPendientesValidarAutorizar(1, 1);
                                    ConsultarSolicitudesPendientesValidarAutorizar(2, 2);
                                }
                                if (modo == 2) {
                                    if (accion == true) {
                                        iziToast.success({
                                            title: 'Los pagos seleccionados se autorizaron correctamente',
                                            message: 'Los pagos no seleccionados se rechazaron',
                                        });
                                    }
                                    else {
                                        iziToast.success({
                                            title: 'Los pagos seleccionados se rechazaron correctamente',
                                            message: 'Los pagos no seleccionados son los autorizados',
                                        });
                                    }
                                    ConsultarSolicitudesPendientesValidarAutorizar(2, 2);
                                    ConsultarSolicitudesPendientesValidarAutorizar(3, 3);
                                }
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error el guardar la solicitud',
                                    message: '',
                                });
                            }
                        }
                    });
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
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
    }
}

function GenerarTXTInterbancarioSolicitudPago(id_solicitud_g) {
    $.ajax({
        url: "../CONTABILIDAD/GenerarTXTInterbancarioSolicitudPago",
        async: false,
        type: "POST",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (result) {
            if (result.length > 5) {
                try {
                    window.open(result, '_blank');
                    iziToast.success({
                        title: 'TXT Interbancario Generado correctamente!',
                        message: '',
                    });
                } catch (e) {
                    iziToast.error({
                        title: 'Ocurrió un error al generar TXT Interbancario',
                        message: 'ERROR EN DESCARGA AUTOMATICA',
                    });
                }

            }
            else {
                if (result == "1") {
                    iziToast.error({
                        title: 'Ocurrió un error al generar TXT Interbancario',
                        message: 'ERROR EN ALGORITMO DE DATOS',
                    });
                }
                else if (result == "-1") {
                    iziToast.error({
                        title: 'NO SE ENCONTRÓ EL DETALLE',
                        message: '',
                    });
                }
                else if (result == "-2") {
                    iziToast.error({
                        title: 'NO HAY PAGOS ACTIVOS',
                        message: '',
                    });
                }
                else if (result == "-3") {
                    iziToast.error({
                        title: 'NO SE ENCONTRÓ EL HEADER DE LA SOLICITUD',
                        message: '',
                    });
                }
                else if (result == "-4") {
                    iziToast.error({
                        title: 'NO HAY PAGOS INTERBANCARIOS, SOLO A TERCEROS',
                        message: '',
                    });
                }
                else if (result == "-5") {
                    iziToast.error({
                        title: 'LA SOLICITUD NO ESTÁ AUTORIZADA',
                        message: '',
                    });
                }
                else if (result == "-6") {
                    iziToast.error({
                        title: 'ERROR EN LA CLAVE INTERBANCARIA',
                        message: 'DIGITOS ERRONEOS',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error no mapeado',
                        message: 'Contacte desarrollo de inmediato',
                    });
                }
            }
        }
    });
}

function GenerarTXTTercerosSolicitudPago(id_solicitud_g) {
    $.ajax({
        url: "../CONTABILIDAD/GenerarTXTTercerosSolicitudPago",
        async: false,
        type: "POST",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (result) {
            if (result.length > 5) {
                try {
                    window.open(result, '_blank');
                    iziToast.success({
                        title: 'TXT Terceros Generado correctamente!',
                        message: '',
                    });
                } catch (e) {
                    iziToast.error({
                        title: 'Ocurrió un error al generar TXT Terceros',
                        message: 'ERROR EN DESCARGA AUTOMATICA',
                    });
                }

            }
            else if (result == "-1") {
                iziToast.error({
                    title: 'NO HAY PAGOS A TERCEROS',
                    message: '',
                });
            }
            else if (result == "-2") {
                iziToast.warning({
                    title: 'NO SE ENCONTRÓ EL DETALLE',
                    message: '',
                });
            }
            else if (result == "-3") {
                iziToast.warning({
                    title: 'NO HAY PAGOS ACTIVOS',
                    message: '',
                });
            }
            else if (result == "-4") {
                iziToast.error({
                    title: 'LA SOLICITUD NO ESTÁ AUTORIZADA',
                    message: '',
                });
            }
            else if (result == "-5") {
                iziToast.error({
                    title: 'ERROR EN LA CLAVE INTERBANCARIA',
                    message: 'DIGITOS ERRONEOS',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al generar TXT Interbancario',
                    message: 'ERROR EN ALGORITMO DE DATOS',
                });
            }
        }
    });
}

function EliminarSolicitudPago(id_solicitud_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Hey',
        message: 'Are you sure about that?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar solicitud</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    url: "../CONTABILIDAD/EliminarSolicitudPago",
                    async: false,
                    type: "POST",
                    data: { id_solicitud_g: id_solicitud_g },
                    success: function (result) {
                        if (result == 0) {
                            iziToast.success({
                                title: 'Solicitud eliminada correctamente',
                                message: '',
                            });
                            ConsultarSolicitudesPendientesValidarAutorizar(3, 3);
                        }
                        else if (result == 1) {
                            iziToast.warning({
                                title: 'La solicitud ya tiene TXT generados',
                                message: 'Contacte a desarrollo para eliminarla manualmente',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al eliminar la solicitud',
                                message: 'Contacte a desarrollo',
                            });
                        }
                    }
                });

            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {

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

