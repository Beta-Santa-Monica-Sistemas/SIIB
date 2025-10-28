function ConsultarOrdenesDisponibles() {
    jsShowWindowLoad();
    $.ajax({
        url: "../PORTAL_PROVEEDORES/ConsultarOrdenesDisponibles",
        async: true,
        type: "POST",
        data: {},
        success: function (result) {
            jsRemoveWindowLoad();
            try {
                $("#ordenes_disponibles_datatable").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_disponibles").html(result);
            $('#ordenes_disponibles_datatable').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                scrollCollapse: true,
                scrollY: '40vh',
                dom: "Bfrtip",
                language: {

                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json',
                },
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarFacturasRevision() {
    jsShowWindowLoad();
    $.ajax({
        url: "../PORTAL_PROVEEDORES/ConsultarFacturasGral",
        async: true,
        type: "POST",
        data: { modo: false },
        success: function (result) {
            jsRemoveWindowLoad();
            try {
                $("#facturas_revision_datatable").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_revision_datatable").html(result);
            $('#facturas_revision_datatable').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                scrollColapse: true,
                scrollY: '40vh',
                dom: "Bfrtip",
                language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json', },
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarFacturasPendientesPago() {
    jsShowWindowLoad();
    $.ajax({
        url: "../PORTAL_PROVEEDORES/ConsultarFacturasGral",
        async: true,
        type: "POST",
        data: { modo: true },
        success: function (result) {
            jsRemoveWindowLoad();
            try {
                $("#facturas_pendientes_pago_datatable").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_pendientes_pago_datatable").html(result);
            $('#facturas_pendientes_pago_datatable').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                scrollColapse: true,
                scrollY: '40vh',
                dom: "Bfrtip",
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json',
                },
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarFacturasPagadasUltimoMes() {

    $.ajax({
        url: "../PORTAL_PROVEEDORES/ConsultarFacturasPagadasUltimoMes",
        async: true,
        type: "POST",
        data: {
        },
        success: function (result) {
            try {
                $("#ultimas_facturas_datatable").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_pagos_ultimo_mes").html(result);
            $('#ultimas_facturas_datatable').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                scrollColapse: true,
                scrollY: '40vh',
                dom: "Bfrtip",
                language: {

                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json',
                },
                buttons: [{
                    //Botón para Excel
                    extend: 'excel',
                    footer: true,
                    title: 'Facturas del ultimo mes',
                    filename: 'Facturas_ultimo_mes',

                    //Aquí es donde generas el botón personalizado
                    text: '<button class="btn btn-success">Exportar a Excel <i class="fas fa-file-excel"></i></button>'
                },
                //Botón para PDF
                {
                    extend: 'pdf',
                    footer: true,
                    title: 'Facturas del ultimo mes',
                    filename: 'Facturas_ultimo_mes',
                    text: '<button class="btn btn-success">Exportar a PDF <i class="far fa-file-pdf"></i></button>'
                }],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarHistorialPagos() {
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();

    $.ajax({
        url: "../PORTAL_PROVEEDORES/ConsultarHistorialPagos",
        async: true,
        type: "POST",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (result) {
            try {
                $("#historial_pagos_datatable").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_historial_pagos").html(result);
            $('#historial_pagos_datatable').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                scrollColapse: true,
                scrollY: '40vh',
                dom: "Bfrtip",
                language: {

                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json',
                },
                buttons: [{
                    //Botón para Excel
                    extend: 'excel',
                    footer: true,
                    title: 'Historial de pagos',
                    filename: 'Historial',

                    //Aquí es donde generas el botón personalizado
                    text: '<button class="btn btn-success">Exportar a Excel <i class="fas fa-file-excel"></i></button>'
                },
                //Botón para PDF
                {
                    extend: 'pdf',
                    footer: true,
                    title: 'Historial de pagos',
                    filename: 'Historial',
                    text: '<button class="btn btn-success">Exportar a PDF <i class="far fa-file-pdf"></i></button>'
                }],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function UploadFilesOrden() {
    var id_orden = $("#no_orden_upload").val();
    if (id_orden == "" || id_orden == undefined || isNaN(id_orden)) {
        iziToast.error({
            title: 'Alerta',
            message: 'ESCOJA UNA ORDEN.'
        });
    }
    else {
        jsShowWindowLoad();
        var fileData = new FormData();
        var fileUpload = $("#xml_upload").get(0);
        var files = fileUpload.files;
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        var fileUpload2 = $("#pdf_upload").get(0);
        var files2 = fileUpload2.files;
        for (var i = 0; i < files2.length; i++) {
            fileData.append(files2[i].name, files2[i]);
        }
        fileData.append('id_orden', id_orden);

        $.ajax({
            url: '../PORTAL_PROVEEDORES/UploadFilesOrden',
            type: "POST",
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            data: fileData,
            success: function (result) {
                //alert(result);
                jsRemoveWindowLoad();
                if (result == "-1") {
                    iziToast.warning({
                        title: 'Atención!',
                        message: 'NO SE DETECTARON ARCHIVOS.'
                    });
                }
                else if (result == "-2") {
                    iziToast.warning({
                        title: 'Alerta!',
                        message: 'REVISE EL FORMATO DE SU DOCUMENTO'
                    });
                }
                else if (result == "-3") {
                    iziToast.error({
                        title: 'Alerta!',
                        message: 'ERROR EN EL PROCESAMIENTO DE ARCHIVOS (LECTURA XML)'
                    });
                }
                else if (result == "-4") {
                    iziToast.error({
                        title: 'Alerta!',
                        message: 'EL RFC RECEPTOR NO COINCIDE CON EL DE BETA: BSM941005111'
                    });
                }
                else if (result == "-5") {
                    iziToast.error({
                        title: 'Atención!',
                        message: 'LOS SUBTOTALES NO COINCIDEN, REVISE'
                    });
                }
                else if (result == "-6") {
                    iziToast.error({
                        title: 'Alerta!',
                        message: 'LA ORDEN NO ESTA DISPONIBLE'
                    });
                }
                else if (result == "-7") {
                    iziToast.warning({
                        title: 'Alerta!',
                        message: 'LA MONEDA NO CONCIDE CON LA ORDEN DE COMPRA'
                    });
                }
                else if (result == "-8") {
                    iziToast.warning({
                        title: 'Alerta!',
                        message: 'LA ORDEN NO EXISTE'
                    });
                }
                else if (result == "-9") {
                    iziToast.error({
                        title: 'Alerta!',
                        message: 'LA ORDEN SELECCIONADA YA FUE IMPORTADA'
                    });
                }
                else if (result == "-10") {
                    iziToast.warning({
                        title: 'Atención!',
                        message: 'EL PROVEEDOR DE LA SESION ES DIFERENTE AL DE LA ORDEN'
                    });
                }
                else if (result == "-11") {
                    iziToast.warning({
                        title: 'Atención!',
                        message: 'LA SESIÓN CADUCÓ, INICIE SESIÓN NUEVAMENTE'
                    });
                }
                else {
                    //var data = $.parseJSON(result);
                    //console.log(data);
                    //$("#subtotal_fac").text("Subtotal: " + data.RecFacturaSubtotal);
                    //$("#moneda_fac").text("Moneda: " + data.RecFacturaMoneda);

                    //$("#iva_fac").text("IVA GRAL: " + data.RecFacturaIVA);
                    //$("#iva_base_fac").text("IVA_base: " + data.RecFacturaIVA_RetBase);
                    //$("#iva_ret_fac").text("IVA_retenido: " + data.RecFacturaIVA_Retenido);
                    //$("#iva_porc_fac").text("IVA_porcentaje: " + data.RecFacturaIVA_RetPorc);

                    //$("#isr_base_fac").text("ISR_base: " + data.RecFacturaISR_RetBase);
                    //$("#isr_ret_fac").text("ISR_retenido: " + data.RecFacturaISR_Retenido);
                    //$("#isr_porc_fac").text("ISR_porcentaje: " + data.RecFacturaISR_RetPorc);

                    //$("#fecha_fac").text("Fecha factura: " + data.RecFacturaFecha);
                    ConsultarOrdenesDisponibles();
                    ConsultarFacturasRevision();
                    iziToast.success({
                        title: 'Exito!',
                        message: 'Factura registrada correctamente'
                    });
                }




            },
            error: function (err) {
                jsRemoveWindowLoad();
            }
        });
    }
}
