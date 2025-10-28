function ConsultarCambioMoneda() {
    $.ajax({
        //url: "https://api.cambio.today/v1/full/USD/json?key=45953|Q4iyUW5qSxhM7r98SL1n",
        url: "../CONTABILIDAD/ConsultarCambioMoneda",
        async: false,
        type: "GET",
        data: {},
        success: function (result) {
            var data = $.parseJSON(result);
            //var cambio_dia = data.result.conversion[90].rate;
            //$("#cambio_dia").text("Cambio al dia: 1 USD = " + cambio_dia + " MXN");
            //$("#tipo_cargo").text(cambio_dia);

            var cambio_dia = data.conversion_rates.MXN;
            $("#cambio_dia").text("Cambio al dia: 1 USD = " + cambio_dia + " MXN");
            $("#tipo_cargo").text(cambio_dia);
        }
    });
}

function ConsultarFacturasPendientesImportacion() {
    var fecha_inicio = $("#fecha_incio_importacion").val();
    var fecha_fin = $("#fecha_fin_importacion").val();
    $.ajax({
        url: "../CONTABILIDAD/ConsultarFacturasPendientesImportacion",
        async: true,
        type: "POST",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (result) {
            try {
                $("#tabla_facturas_pendientes_importacion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_facturas_pendientes_importacion").html(result);
            $('#tabla_facturas_pendientes_importacion').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: false,
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

function GenerarImportacionFacturas() {
    var tipo_cargo = $("#tipo_cargo").text().trim();

    var nombre_importacion = $("#nombre_importacion");
    var id_facturas = [];
    var count = 0;

    var checks = $(".checks_facturas");
    checks.each(function () {
        if ($(this).prop("checked") == true) {
            var id_factura = $(this).attr("id").split('_')[1];
            id_facturas[count] = id_factura
            count++;
        }
    });

    if (id_facturas.length == 0) {
        iziToast.error({
            title: 'Asegurese de selecciona al menos 1 factura para importar',
            message: '',
        });
        nombre_importacion.css("border-color", "");
    }
    else if (nombre_importacion.val() == "") {
        iziToast.warning({
            title: 'Ingrese el nombre de la importación',
            message: '',
        });
        nombre_importacion.css("border-color", "red");
    }
    else if (tipo_cargo == "" || tipo_cargo == undefined) {
        iziToast.warning({
            title: 'Ocurrió un problema al consultar la conversión de moneda',
            message: 'Llame a sistemas',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            url: "../CONTABILIDAD/GenerarImportacionFacturas",
            async: true,
            timeout: 90000,
            type: "POST",
            data: {
                id_facturas: id_facturas,
                nombre_importacion: nombre_importacion.val(),
                tipo_cargo: tipo_cargo
            },
            success: function (result) {
                nombre_importacion.css("border-color", "");
                if (result != "-1") {
                    window.open(result, '_blank');
                    ConsultarFacturasPendientesImportacion();
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un problema al importar las facturas',
                        message: 'Error en el encabezado',
                    });
                }

            }
        });
    }

}

function DescargarPolizaTXT() {
    var path = "//192.168.128.2/inetpub/PolizasContablesTXT/POLIZAS_566.txt";
    $.ajax({
        //url: "https://api.cambio.today/v1/full/USD/json?key=45953|Q4iyUW5qSxhM7r98SL1n",
        url: "../CONTABILIDAD/DescargarPolizaTXT",
        async: false,
        type: "GET",
        data: { path: path },
        success: function (result) {

        }
    });
}

function ConsultarImportacionHistorial() {
    var fecha_inicio = $("#fecha_incio_historial").val();
    var fecha_fin = $("#fecha_fin_historial").val();
    $.ajax({
        url: "../CONTABILIDAD/ConsultarImportacionHistorial",
        async: true,
        type: "POST",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (result) {
            try {
                $("#datatable_historial_importacion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_historial_importacion").html(result);
            $('#datatable_historial_importacion').DataTable({
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

function ConsultarImportacionDetalle(id_importacion_g) {
    $.ajax({
        url: "../CONTABILIDAD/ConsultarImportacionDetalle",
        async: true,
        type: "POST",
        data: { id_importacion_g: id_importacion_g },
        success: function (result) {
            try {
                $("#datatable_detalle_importacion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_importacion").html(result);
            $('#datatable_detalle_importacion').DataTable({
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
            $("#m_detalle_importacion").modal("show");
        }
    });
}

function ConsultarAuditoriaFacturasValoresImportados() {
    var fecha_inicio = $("#fecha_incio_auditoria_valores").val();
    var fecha_fin = $("#fecha_fin_auditoria_valores").val();
    var id_proveedor = $("#id_proveedor_valores").val();

    var iva = false;
    if ($("#check_iva").is(':checked')) { iva = true; }
    var iva_ret = false;
    if ($("#check_iva_ret").is(':checked')) { iva_ret = true; }
    var isr = false;
    if ($("#check_isr").is(':checked')) { isr = true; }
    var ieps = false;
    if ($("#check_ieps").is(':checked')) { ieps = true; }

    if (iva == false && iva_ret == false && isr == false && ieps == false) {
        iziToast.warning({
            title: 'Ingrese los parámetros de filtrado',
            message: '',
        });
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        url: "../CONTABILIDAD/ConsultarAuditoriaFacturasValoresImportados",
        async: true,
        type: "POST",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            iva: iva,
            iva_ret: iva_ret,
            isr: isr,
            ieps: ieps,
            id_proveedor: id_proveedor
        },
        success: function (result) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_auditoria_valores").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_auditoria_valores").html(result);
            $('#datatable_auditoria_valores').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: false,
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
