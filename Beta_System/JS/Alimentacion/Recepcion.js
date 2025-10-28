
//#region RECEPCIONES

function ConsultarFichasPendientesRecepcion() {
    //var fecha_inicio = $("#fecha_inicio_recepcion_pendiente").val();
    //var fecha_fin = $("#fecha_fin_recepcion_pendiente").val();
    var id_establo = $("#id_establo_recepcion_pendiente").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarFichasPendientesRecepcion",
        data: {
            //fecha_inicio: fecha_inicio,
            //fecha_fin: fecha_fin,
            id_establo: id_establo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_fichas_pendientes_recepcion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_pendientes_recepcion").html(response);
            $('#datatable_fichas_pendientes_recepcion').DataTable({
                keys: false,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function RecepcionarPendientesRecepcion() {
    var id_bascula_fichas = [];
    //var id_proveedores = [];
    var count = 0;
    $('input[name="seleccion_recepcion_pendiente"]:checked').each(function () {
        var id_ficha = $(this).val();
        var id_proveedor = $("#prov_ficha_recepcion_" + id_ficha + "").val();
        id_bascula_fichas[count] = id_ficha;
        //id_proveedores[count] = id_proveedor;
        count++;
    });

    if (id_bascula_fichas.length == 0) {
        iziToast.error({
            title: 'Seleccione al menos 1 ficha para recepcionar',
            message: '',
        });
    }
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 999,
            title: 'Advertencia',
            message: '¿Está seguro de recepcionar las fichas seleccionadas?',
            position: 'center',
            buttons: [
                ['<button><b>Si, recepcionar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../ALIMENTACION/RecepcionarPendientesRecepcion",
                        data: {
                            id_bascula_fichas: id_bascula_fichas
                            //id_proveedores: id_proveedores
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 1) {
                                iziToast.error({
                                    title: 'Las fichas no pueden ser recepcionadas de diferentes proveedores',
                                    message: '',
                                });
                            }
                            else if (response == 0) {
                                iziToast.success({
                                    title: 'Fichas recepcionadas correctamente',
                                    message: '',
                                });
                                ConsultarFichasPendientesRecepcion();
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error al recepcionar las fichas seleccionadas',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
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

}


//#endregion


//#region RECEPCIONES FACTURADAS
function ConsultarFichasRecepcionadas() {
    var fecha_inicio = $("#fecha_inicio_recepcion_factura").val();
    var fecha_fin = $("#fecha_fin_recepcion_factura").val();
    var id_establo = $("#id_establo_recepcion_factura").val();
    var id_proveedor = $("#proveedor_recepcion_factura").val();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarFichasRecepcionadas",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_establo: id_establo,
            id_proveedor: id_proveedor
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_fichas_recepcion_facturas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_recepcion_facturas").html(response);
            $('#datatable_fichas_recepcion_facturas').DataTable({
                keys: false,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AgruparfacturasRecepcion() {
    var fichas = [];
    var id_monedas = [];
    var id_proveedores = [];
    var importe_total = 0;
    var importe_toneladas = 0;
    var importe_precio = 0;
    var valid_toneladas = true;
    var valid_precio = true;
    $(".toneladas_ficha_facturas").css("border", "");
    $(".precios_ficha_facturas").css("border", "");
    $(".input_valid").css("border", "");
    $('input[name="seleccion_recepcion_factura"]:checked').each(function () {
        var id_ficha = $(this).val();

        var val_tons = 0;
        val_tons = $("#toneladas_facturas_" + id_ficha + "").val();
        if (val_tons == undefined || val_tons == "" || val_tons <= 0) { val_tons = 0; $("#toneladas_facturas_" + id_ficha + "").css("border", "2px solid red"); }

        var val_precio = 0;
        val_precio = $("#precios_facturas_" + id_ficha + "").val();
        if (val_precio == undefined || val_precio == "" || val_precio <= 0) { $("#precios_facturas_" + id_ficha + "").css("border", "2px solid red"); }

        var tons = parseFloat(val_tons);
        var precio = parseFloat(val_precio);
        var importe = tons * precio;
        importe_total = importe_total + importe;
        importe_toneladas = importe_toneladas + tons;
        importe_precio = importe_precio + precio;

        if (precio <= 0) { valid_precio = false; }
        if (tons <= 0) { valid_toneladas = false; }

        var formato_toneladas = tons.toLocaleString('en-US', {
            minimumFractionDigits: 3,
            maximumFractionDigits: 3
        });
        var formato_precio = precio.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        var formato_totales = importe.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        id_monedas.push($("#id_moneda_facturas_" + id_ficha + "").val());
        id_proveedores.push($("#prov_ficha_recepcion_factura_" + id_ficha + "").val());
        fichas.push({
            establo: $("#establo_facturas_" + id_ficha + "").text(),
            ficha: $("#id_ficha_facturas_" + id_ficha + "").text(),
            fecha: $("#fecha_facturas_" + id_ficha + "").text(),
            proveedor: $("#prov_facturas_" + id_ficha + "").text(),
            articulo: $("#art_facturas_" + id_ficha + "").text(),
            obs: $("#obs_facturas_" + id_ficha + "").text(),
            toneladas: formato_toneladas,
            precios: formato_precio,
            importes: formato_totales,
            monedas: $("#id_moneda_facturas_" + id_ficha + " option:selected").text(),
            id_moneda: $("#id_moneda_facturas_" + id_ficha + "").val(),
        });
    });
    var valid_moneda = [...new Set(id_monedas)];
    var valid_proveedor = [...new Set(id_proveedores)];

    if (valid_moneda.length >= 2) {
        iziToast.warning({
            title: 'Asegurese de que las monedas sean iguales',
            message: '',
        });
    }
    else if (valid_proveedor.length >= 2) {
        iziToast.warning({
            title: 'Asegurese de que las fichas sean del mismo proveedor',
            message: '',
        });
    }
    else if (valid_precio == false) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese valores de precios validos',
        });
    }
    else if (valid_toneladas == false) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese valores de toneladas validos',
        });
    }
    else if (fichas.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 ficha para generar la factura',
            message: '',
        });
    }
    else {
        var tbody = $("#tbody_recepcion_factura");
        tbody.html("");
        for (var i = 0; i < fichas.length; i++) {
            var row = $("<tr class='text-center'></tr>");
            for (var key in fichas[i]) {
                if ([key] == "id_moneda") { row.append("<td style='display:none;'>" + fichas[i][key] + "</td>"); }
                else { row.append("<td>" + fichas[i][key] + "</td>"); }
            }
            tbody.append(row);  // Add the row to the table body
        }

        var formato_toneladas = importe_toneladas.toLocaleString('en-US', {
            minimumFractionDigits: 3,
            maximumFractionDigits: 3
        });
        var formato_precio = importe_precio.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        var formato_total = importe_total.toLocaleString('en-US', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });

        var row = $("<tr class='bg-beta text-center'></tr>");
        row.append("<td></td>");
        row.append("<td></td>");
        row.append("<td></td>");
        row.append("<td></td>");
        row.append("<td></td>");
        row.append("<td><strong>TOTALES</strong></td>");
        row.append("<td><strong>" + formato_toneladas + "</strong></td>");
        row.append("<td><strong>" + formato_precio + "</strong></td>");
        row.append("<td><strong>" + formato_total + "</strong></td>");
        row.append("<td></td>");
        tbody.append(row);
        $("#total_toneladas_recepcion_factura").val(importe_toneladas.toFixed(3));
        $("#total_importe_recepcion_factura").val(importe_total.toFixed(2));

        $("#m_recepcion_factura").modal("show");
    }
}

function GenerarFacturaFichasRecepcionadas() {
    $(".input_valid").css("border", "");
    var folio_factura = $("#folio_recepcion_factura").val();
    var fecha = $("#fecha_recepcion_factura").val();
    var total_toneladas = $("#total_toneladas_recepcion_factura").val();
    var total_importe = $("#total_importe_recepcion_factura").val();
    var id_moneda = $("#id_moneda_recepcion_factura").val();
    var tipo_cambio = $("#tipo_cambio_recepcion_factura").val();
    var id_compras_proveedor = 0;

    var id_fichas = [];
    var toneladas_confirm = [];
    var precios_confirm = [];
    var id_monedas = [];
    var importe_total = 0;
    var importe_toneladas = 0;
    var importe_precio = 0;
    var valid_precio = true;
    var valid_toneladas = true;
    var count = 0;
    $('input[name="seleccion_recepcion_factura"]:checked').each(function () {
        var id_ficha = $(this).val();
        id_compras_proveedor = $("#prov_ficha_recepcion_factura_" + id_ficha + "").val();

        id_fichas[count] = id_ficha;

        var val_tons = 0;
        val_tons = $("#toneladas_facturas_" + id_ficha + "").val();
        if (val_tons == undefined || val_tons == "" || val_tons <= 0) { val_tons = 0; }

        var val_precio = 0;
        val_precio = $("#precios_facturas_" + id_ficha + "").val();
        if (val_precio == undefined || val_precio == "" || valid_precio <= 0) { val_precio = 0; }


        if (val_precio == 0) { valid_precio = false; }
        if (val_tons == 0) { valid_toneladas = false; }

        var tons = parseFloat(val_tons);
        var precio = parseFloat(val_precio);

        toneladas_confirm[count] = tons;
        precios_confirm[count] = precio;
        id_monedas[count] = $("#id_moneda_facturas_" + id_ficha + "").val();

        var importe = tons * precio;
        importe_total = importe_total + importe;
        importe_toneladas = importe_toneladas + tons;
        importe_precio = importe_precio + precio;

        count++;
    });


    if (id_compras_proveedor == "" || id_compras_proveedor == null || id_compras_proveedor == 0) {
        iziToast.error({
            title: 'Proveedor no detectado',
            message: 'Cierre este recuadro e invente nuevamente',
        });
    }
    else if (valid_precio == false) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese valores de precios validos',
        });
    }
    else if (valid_toneladas == false) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese valores de toneladas validos',
        });
    }
    else if (folio_factura == "" || folio_factura == undefined) {
        $("#folio_recepcion_factura").css("border", "2px solid red");
        iziToast.error({
            title: 'Ingrese el folio de la factura',
            message: '',
        });
    }
    else if (fecha == "" || fecha == undefined) {
        iziToast.error({
            title: 'Ingrese la fecha de la factura',
            message: '',
        });
    }
    else if (total_toneladas == "" || total_toneladas == undefined) {
        $("#total_toneladas_recepcion_factura").css("border", "2px solid red");
        iziToast.error({
            title: 'Ingrese las toneladas totales de la factura',
            message: '',
        });
    }
    else if (total_importe == "" || total_importe == undefined) {
        $("#total_importe_recepcion_factura").css("border", "2px solid red");
        iziToast.error({
            title: 'Atención',
            message: 'Ingrese el importe total de la factura',
        });
    }
    else if (id_moneda == undefined || id_moneda == "") {
        iziToast.error({
            title: 'Seleccione una moneda para la factura',
            message: '',
        });
    }
    else if (tipo_cambio == "" || tipo_cambio == 0 || tipo_cambio == undefined) {
        iziToast.warning({
            title: 'El tipo de cambio no es valido',
            message: '',
        });
    }
    else if (id_fichas.length <= 0) {
        iziToast.error({
            title: 'Selecciona al menos 1 ficha para generar la factura',
            message: '',
        });
    }
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 9999999,
            title: 'Advertencia',
            message: '¿Está seguro de generar la factura?',
            position: 'center',
            buttons: [
                ['<button><b>Si, generar ahora</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: false,
                        timeout: 900000,
                        url: "../ALIMENTACION/GenerarFacturaFichasRecepcionadas",
                        data: {
                            folio_factura: folio_factura,
                            fecha: fecha,
                            toneladas: total_toneladas,
                            importe: total_importe,
                            id_moneda: id_moneda,
                            tipo_cambio: tipo_cambio,
                            id_compras_proveedor: id_compras_proveedor,
                            id_fichas: id_fichas,
                            toneladas_confirm: toneladas_confirm,
                            precios_confirm: precios_confirm,
                            id_monedas: id_monedas
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Factura generada correctamente',
                                    message: '',
                                });
                                $("#folio_recepcion_factura").val("");
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error al generar la factura',
                                    message: '',
                                });
                            }
                            ConsultarFichasRecepcionadas();
                            $("#m_recepcion_factura").modal("hide");
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
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



}


function ConsultarTipoCambio() {
    var id_moneda = $("#id_moneda_recepcion_factura").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../CATALOGOS/ConsultarTipoCambio",
        data: { id_moneda: id_moneda },
        success: function (response) {
            $("#tipo_cambio_recepcion_factura").prop("disabled", false);
            $("#tipo_cambio_recepcion_factura").val(response);
            if (response == "1") { $("#tipo_cambio_recepcion_factura").prop("disabled", true); }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function SeleccionarFilaFacturas(id_row) {
    if ($("#check_seleccion_factura_" + id_row + "").is(':checked')) {
        $("#id_row_seleccion_factura_" + id_row + "").css("background-color", "rgb(153, 255, 204)");
    }
    else {
        $("#id_row_seleccion_factura_" + id_row + "").css("background-color", "");
    }
}

//#endregion


//#region FACTURAS DE FORRAJE
function ConsultarFacturasForrajesTable() {
    var fecha_inicio = $("#fecha_inicio_factura").val();
    var fecha_fin = $("#fecha_fin_factura").val();
    var id_establo = $("#id_establo_factura").val();
    var id_proveedor = $("#proveedor_factura").val();
    var id_alimento = $("#alimento_factura").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarFacturasForrajesTable",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_establo: id_establo,
            id_proveedor: id_proveedor,
            id_alimento: id_alimento
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_fichas_facturas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_facturas").html(response);
            $('#datatable_fichas_facturas').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarFacturaForrajeDetalle(id_factura_forraje_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarFacturaForrajeDetalle",
        data: { id_factura_forraje_g: id_factura_forraje_g },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_factura_detalle").html(response);
            $("#m_factura_detalle").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion