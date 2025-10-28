var count_beneficiario = 0;

function ConsultarOrdenesPago() {
    $.ajax({
        url: "../CONTABILIDAD/ConsultarOrdenesPago",
        async: true,
        type: "POST",
        data: {},
        success: function (response) {
            try {
                $("#datatable_ordenes_pago").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_pago").html(response);
            $('#datatable_ordenes_pago').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm"
                }, {
                    extend: "excel",
                    className: "btn-sm"
                }, {
                    extend: "pdf",
                    className: "btn-sm"
                }, {
                    extend: "print",
                    className: "btn-sm"
                }],
                responsive: false
            });
        }
    });
}

function ConsultarOrdenPago(id_contabilidad_orden_pago_g) {
    $.ajax({
        url: "../CONTABILIDAD/ConsultarOrdenPago",
        async: true,
        type: "POST",
        data: { id_contabilidad_orden_pago_g: id_contabilidad_orden_pago_g },
        success: function (response) {
            try {
                $("#datatable_orden_pago_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_orden_pago_detalle").html(response);
            $('#datatable_orden_pago_detalle').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm"
                }, {
                    extend: "excel",
                    className: "btn-sm"
                }, {
                    extend: "pdf",
                    className: "btn-sm"
                }, {
                    extend: "print",
                    className: "btn-sm"
                }],
                responsive: false
            });
            $("#m_detalle_orden").modal("show");
        }
    });
}





function AgregarBeneficiarioOrden() {
    $(".input_valid_det").css("border-color", "");
    $(".div_valid_det").css("border", "");

    var orden_tipo = $("input[name='orden_pago_tipo_transaccion']:checked").val();
    if (orden_tipo == "" || orden_tipo == undefined) {
        iziToast.warning({
            title: 'Ingrese el tipo de la cuenta origen',
            message: '',
        });
        $(".div_valid_det").css("border", "1px solid red");
    }
    else {
        var nombre_orden_tipo = $("#lbl_" + orden_tipo + "").text();
        var nombre_beneficiario = $("#nombre_beneficiario_orden_pago").val();
        var sucursal_destino = $("#sucursal_destino_orden_pago").val();
        var importe = $("#importe_orden_pago").val();
        var instrucciones = $("#instrucciones_orden_pago").val();
        var remitente = $("#remitente_orden_pago").val();
        if (nombre_orden_tipo == "" || nombre_orden_tipo == undefined || nombre_beneficiario == "" || nombre_beneficiario == undefined ||
            sucursal_destino == "" || sucursal_destino == undefined || importe == "" || importe == undefined || remitente == "" || remitente == undefined) {
            iziToast.warning({
                title: 'Ingrese todos los datos del beneficiario',
                message: '',
            });

            $(".input_valid_det").each(function () {
                if ($(this).val() == "" || $(this).val() == undefined) {
                    $(this).css("border-color","red");
                }
            });
        }
        else {
            if (sucursal_destino.length > 4) {
                iziToast.warning({
                    title: 'La sucursal destino no debe ser mayor a 4 digitos',
                    message: '',
                });
                $("#sucursal_destino_orden_pago").css("border", "1px solid red");
            }
            else {
                var tbody_detalle = $("#tbody_orden_pago");
                var row = $("<tr class='text-center beneficiarios_detalle' id='trBeneficiario_" + count_beneficiario + "'></tr>");

                row.append("<td style='display:none;' id='ordenes_tipo_" + count_beneficiario + "' >" + orden_tipo + "</td>");

                var importe_format = Number(importe).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                row.append("<td id='nombre_tipo_" + count_beneficiario + "' >" + nombre_orden_tipo + "</td>");
                row.append("<td id='nombre_beneficiario_" + count_beneficiario + "' >" + nombre_beneficiario + "</td>");
                row.append("<td id='instrucciones_" + count_beneficiario + "' >" + instrucciones + "</td>");
                row.append("<td id='sucursal_destino_" + count_beneficiario + "' >" + sucursal_destino + "</td>");

                row.append("<td style='display:none;' id='importe_" + count_beneficiario + "'>" + importe + "</td>");
                row.append("<td>$" + importe_format + "</td>");

                row.append("<td id='remitente_" + count_beneficiario + "' >" + remitente + "</td>");
                row.append("<td><button class='btn btn_beta_danger' onclick='QuitarBeneficiarioOrden(" + count_beneficiario + ");'><i class='fa fa-remove'></i></button></td>");
                tbody_detalle.append(row);

                count_beneficiario = count_beneficiario + 1;
                $(".input_valid_det").val('');
                $("#instrucciones_orden_pago").val('');
            }
        }
    }
}

function QuitarBeneficiarioOrden(count_beneficiario_remove) {
    $("#trBeneficiario_" + count_beneficiario_remove + "").remove();
}

function GuardarOrdenPago() {
    $(".input_valid_form").css("border-color", "");

    var nombre_orden = $("#nombre_orden_pago").val();
    var numero_sucursal = $("#numero_sucursal_orden_pago").val();
    var numero_cuenta = $("#numero_cuenta_orden_pago").val();

    var fecha = $("#fecha_orden_pago").val();
    var hra = $("#hra_orden_pago").val();
    var min = $("#minutos_orden_pago").val();

    var orden_g = {};
    orden_g.nombre_orden_pago = nombre_orden;
    orden_g.numero_sucursal = numero_sucursal;
    orden_g.numero_cuenta = numero_cuenta;

    if (numero_sucursal.length > 4) {
        iziToast.warning({
            title: 'EL numero de sucursal origen debe ser menor a 4 digitos',
            message: '',
        });
        $("#numero_sucursal_orden_pago").css("border","1px solid red");
    }
    else if (numero_cuenta.length > 20) {
        iziToast.warning({
            title: 'El numero de cuenta debe ser menor a 20 digitos',
            message: '',
        });
        $("#numero_cuenta_orden_pago").css("border", "1px solid red");
    }
    else if (hra == "" || hra == undefined || min == "" || min == undefined) {
        iziToast.warning({
            title: 'Ingrese la hora de aplicación',
            message: '',
        });
    }
    else if (fecha == "" || fecha == undefined) {
        iziToast.warning({
            title: 'Ingrese la fecha de aplicación',
            message: '',
        });
    }
    else if (nombre_orden == "" || nombre_orden == undefined || numero_sucursal == "" || numero_sucursal == undefined || numero_cuenta == "" || numero_cuenta == undefined) {
        iziToast.warning({
            title: 'Ingrese todos los datos generales',
            message: '',
        });
        $(".input_valid_form").each(function () {
            if ($(this).val() == "" || $(this).val() == undefined) {
                $(this).css("border-color", "red");
            }
        });
    }
    else {
        var valid_input = false;
        var detalle = $(".beneficiarios_detalle");
        if (detalle.length <= 0) {
            iziToast.warning({
                title: 'Ingrese el detalle de la orden',
                message: '',
            });
        }
        else {
            var count = 0;
            var ordenes_tipos = [];
            var nombre_bene = [];
            var sucs_destino = [];
            var importes = [];
            var instru = [];
            var remit = [];

            detalle.each(function () {
                var count_bene = $(this).attr("id").split('_')[1];
                var ordenes_tipo = $("#ordenes_tipo_" + count_bene + "").text();
                var nombre_beneficiario = $("#nombre_beneficiario_" + count_bene + "").text();
                var sucursal_destino = $("#sucursal_destino_" + count_bene + "").text();
                var importe = $("#importe_" + count_bene + "").text();
                var instrucciones = $("#instrucciones_" + count_bene + "").text();
                var remitente = $("#remitente_" + count_bene + "").text();

                if (ordenes_tipo == "" || ordenes_tipo == undefined || nombre_beneficiario == "" || nombre_beneficiario == undefined ||
                    sucursal_destino == "" || sucursal_destino == undefined || importe == "" || importe == undefined || remitente == "" || remitente == undefined) {
                    valid_input = true;
                }
                ordenes_tipos[count] = ordenes_tipo;
                nombre_bene[count] = nombre_beneficiario;
                sucs_destino[count] = sucursal_destino;
                importes[count] = importe;
                instru[count] = instrucciones;
                remit[count] = remitente;
                count++;
            });

            if (valid_input == true) {
                iziToast.error({
                    title: 'Se detectaron campos vacios en el detalle de la orden',
                    message: '',
                });
            }
            else {
                $.ajax({
                    url: "../CONTABILIDAD/AgregarBeneficiarioOrden",
                    async: true,
                    type: "POST",
                    data: {
                        orden_g: orden_g,
                        fecha_aplicacion: fecha,
                        hra: hra,
                        minutos: min,
                        ordenes_tipo: ordenes_tipos,
                        nombre_bene: nombre_bene,
                        sucursal_destino: sucs_destino,
                        importes: importes,
                        instrucciones: instru,
                        remitente: remit
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Orden generada correctamente',
                                message: '',
                            });
                            $(".input_valid_form").val('');
                            $("#tbody_orden_pago").html("");
                            count_beneficiario = 0;
                            ConsultarOrdenesPago();
                        }
                        else if (response == 1) {
                            iziToast.error({
                                title: 'Error al guardar el detalle de la orden',
                                message: '',
                            });
                        }
                        else{
                            iziToast.error({
                                title: 'Ocurrió un problema al guardar la orden de compra',
                                message: '',
                            });
                        }
                    }
                });
            }
        }
        


    }
    



}

function EliminarBeneficiarioOrden(id_contabilidad_orden_pago_g, id_contabilidad_orden_pago_d) {
    $.ajax({
        url: "../CONTABILIDAD/EliminarBeneficiarioOrden",
        async: true,
        type: "POST",
        data: { id_contabilidad_orden_pago_d: id_contabilidad_orden_pago_d },
        success: function (response) {
            if (response == 0) {
                iziToast.success({
                    title: 'Beneficiario eliminado correctamente',
                    message: '',
                });
                ConsultarOrdenPago(id_contabilidad_orden_pago_g);
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un problema al eliminar el beneficiario',
                    message: '',
                });
            }
        }
    });
}

function GenerarTXTOrdenPago(id_contabilidad_orden_pago_g) {
    $.ajax({
        url: "../CONTABILIDAD/GenerarTXTOrdenPago",
        async: false,
        type: "POST",
        data: {
            id_contabilidad_orden_pago_g: id_contabilidad_orden_pago_g
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
            else if (result == 5) {
                iziToast.info({
                    title: 'NO HAY PAGOS A TERCEROS',
                    message: '',
                });
            }
            else {
                iziToast.info({
                    title: 'Ocurrió un problema al generar el TXT',
                    message: '',
                });
            }
        }
    });
}
