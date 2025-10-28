$('#fecha_inicio').val(last_week);
$('#fecha_fin').val(today);

$('.fecha_inicio').val(last_week);
$('.fecha_fin').val(today);


//#region REPORTES
function ConsultarKardexArticulo() {
    var fecha_inicio = $("#fecha_inicio_kardex").val();
    var fecha_fin = $("#fecha_fin_kardex").val();
    var id_articulo = $("#id_articulo_kardex").val();
    var id_tipo_mov = $("#id_tipo_mov_kardex").val();
    var id_almacen = $("#id_almacen_kardex").val();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../ALMACEN/ConsultarKardexArticulo",
        data: {
            id_articulo: id_articulo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_tipo_mov: id_tipo_mov,
            id_almacen: id_almacen
        },
        success: function (response) {
            var total = 0;
            jsRemoveWindowLoad();
            try {
                $("#tabla_kardex_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_kardex_articulos").html(response);
            $('#tabla_kardex_articulos').DataTable({
                ordering: false,
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
                responsive: false,
                select: true,
                keys: false
            });
            $(".cantidad_surtida_kardex").each(function () {
                total += parseFloat($(this).text().replace(/[^\d.]/g, ''));
            })

            var formato_total = total.toLocaleString('en-US', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
            var totales = "<tr>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td><strong>TOTAL CANTIDAD</strong></td>" +
                "<td><strong>" + formato_total + "</strong></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "<td></td>" +
                "</tr >";
            $("#tabla_kardex_articulos").append(totales);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });

}


function ShowHideTipoConsultaAnalisisConsumos(modo) {
    var div_fechas = $("#div_fechas_consumos");
    var div_anios = $("#div_anio_consumos");

    if (modo == 0) {  // FECHAS
        div_anios.css("display", "none");
        div_fechas.css("display", "inline");
    }
    else {  // AÑOS
        div_anios.css("display", "inline");
        div_fechas.css("display", "none");
    }
}

function ConsultarAnalisisConsumos() {
    var id_almacen = $("#id_almacen_consumos").val();
    var fecha_inicio = $("#fecha_inicio_consumos").val();
    var fecha_fin = $("#fecha_fin_consumos").val();
    var id_tipos_articulos = $("#id_tipo_articulos_consumos").val();

    var tipo_consulta = $('input[name="RadioTipoConsulta"]:checked').val();

    if (tipo_consulta == 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeput: 9000000,
            url: "../ALMACEN/ConsultarAnalisisConsumos",
            data: {
                id_almacen: id_almacen,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_tipos_articulos: id_tipos_articulos
            },
            success: function (response) {
                jsRemoveWindowLoad();
                var total = 0;
                try {
                    $("#tabla_consumos_articulos").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_consumos_articulos").html(response);
                $('#tabla_consumos_articulos').DataTable({
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
                    responsive: false,
                    pageLength: 20,
                    scrollY: false,
                    scrollX: true
                });

                //$(".cantidad_surtida_consumos").each(function () {
                //    total += parseFloat($(this).text());
                //})

                //var totales = "<tr>" +
                //    "<td></td>" +
                //    "<td></td>" +
                //    "<td></td>" +
                //    "<td></td>" +
                //    "<td></td>" +
                //    "<td></td>" +
                //    "<td><strong>TOTAL CANTIDAD</strong></td>" +
                //    "<td><strong>" + total.toFixed(2) + "</strong></td>" +
                //    "</tr >";
                //$("#tabla_consumos_articulos").append(totales);
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }  // RANGO DE FECHAS

    else if (tipo_consulta == 1) {  // ANUAL
        var anio = $('#anio_consumos option:selected').text();

        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeput: 9000000,
            url: "../ALMACEN/ConsultarAnalisisConsumosAnual",
            data: {
                id_almacen: id_almacen,
                anio: anio,
                id_tipo_articulos: id_tipos_articulos
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#tabla_consumos_articulos").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_consumos_articulos").html(response);
                $('#tabla_consumos_articulos').DataTable({
                    keys: false,
                    ordering: false,
                    paging: false,
                    dom: "Bfrtip",
                    select: true,
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
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Seleccione el tipo de consulta',
            message: '',
        });
    }
}

function ConsultarQuickViewArticulo() {
    var id_articulo = $("#id_articulo_quick_view").val();
    var id_almacen = $("#id_almacen_g").val();

    if (id_almacen == undefined || id_almacen == "") {
        iziToast.warning({
            title: 'No se detectaron almacenes asignados',
            message: '',
        });
    }

    else if (id_articulo == undefined || id_articulo == "") {
        iziToast.warning({
            title: 'No se detectó un articulo para consultar',
            message: 'Busque nuevamente y seleccione el articulo',
        });
    }

    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 9000000,
            url: "../ALMACEN/ConsultarQuickViewArticulo",
            data: {
                id_articulo: id_articulo,
                id_almacen: id_almacen
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#m_quick_view_articulo").modal("show");
                $("#div_quick_view_articulo").html(response);
            },
            error: function (xhr, status, error) {
                alert(error);
            }
        });
    }
}

function ConsultarReporteSolicitudesMercancia() {
    var id_almacen = $("#id_almacen_solicitudes_mercancia").val();
    var fecha_inicio = $("#fecha_inicio_solicitudes_mercancia").val();
    var fecha_fin = $("#fecha_fin_solicitudes_mercancia").val();
    var id_solicitante = $("#id_solicitante_solicitudes_mercancia").val();
    var id_departamento = $("#id_departamento_solicitudes_mercancia").val();
    var id_equipo = $("#id_equipo_solicitudes_mercancia").val();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../ALMACEN/ConsultarReporteSolicitudesMercancia",
        data: {
            id_almacen: id_almacen,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_solicitante: id_solicitante,
            id_departamento: id_departamento,
            id_equipo: id_equipo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_reporte_solicitud_mercancia").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_solicitud_mercancia").html(response);
            $('#datatable_reporte_solicitud_mercancia').DataTable({
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
                responsive: false,
                select: true,
                keys: false,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function ConsultarEntradasVsRecepcionOrdenesExcel() {
    var id_almacen = $("#id_almacen").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    //var url = "https://localhost:44371//REPORTES/DescargarExcelFormatoEntradasVsRecepcionOrdenes?id_almacen=" + id_almacen + "&fecha_inicio=" + fecha_inicio + "&fecha_fin=" + fecha_fin + "&modo=2&formato=1&tamano=10";
    var url = "https://siib.beta.com.mx/REPORTES/DescargarExcelFormatoEntradasVsRecepcionOrdenes?id_almacen=" + id_almacen + "&fecha_inicio=" + fecha_inicio + "&fecha_fin=" + fecha_fin + "&modo=2&formato=1&tamano=10";
    window.open(url, '_blank');
}

function ConsultarEntradasVsRecepcionOrdenesTable() {
    var id_almacen = $("#id_almacen").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarEntradasVsRecepcionOrdenesTable",
        data: {
            id_almacen: id_almacen,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#datatable_entrada_ordenes").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_entradas_vs_ordenes").html(response);
            $('#datatable_entrada_ordenes').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
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
                responsive: !0
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarReporteRequisicionesOrdenesCompraTable() {
    var id_centro = $("#id_centro").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();


    $("#ref_excel").prop("href", "https://siib.beta.com.mx/REPORTES/DescargarExcelFormatoReporteRequisicionesOrdenesCompra?id_centro=" + id_centro + "&fecha_inicio=" + fecha_inicio + "&fecha_fin=" + fecha_fin + "&modo=1&formato=1&tamano=21");
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarReporteRequisicionesOrdenesCompraTable",
        data: {
            id_centro: id_centro,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#datatable_entrada_ordenes").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_entradas_vs_ordenes").html(response);
            $('#datatable_entrada_ordenes').DataTable({
                keys: false,
                select: true,
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm",
                    bom: true
                }, {
                    extend: "excel",
                    className: "btn-sm",
                    bom: true
                }, {
                    extend: "pdf",
                    className: "btn-sm"
                }, {
                    extend: "print",
                    className: "btn-sm"
                }],
                responsive: false,
                seraching: true
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#endregion





//-----------POLIZAS CONTABLES ALMACEN
function GenerarPolizasContablesAlmacenTXT() {
    var id_almacen = $("#id_almacen_g_poliza").val();
    var fecha_inicio = $("#fecha_inicio_poliza").val();
    var fecha_fin = $("#fecha_fin_poliza").val();

    if (id_almacen == undefined || id_almacen == '' || id_almacen.length > 1) {
        iziToast.warning({
            title: 'Seleccione un almacén',
            message: '',
        });
        return;
    }
    else if (fecha_inicio == '' || fecha_fin == '') {
        iziToast.warning({
            title: 'Seleccione el campo de fecha inicio y fecha fin',
            message: '',
        });
        return;
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../ALMACEN/GenerarPolizasContablesAlmacenTXT",
            data: {
                id_almacen: id_almacen,
                fecha_i: fecha_inicio,
                fecha_f: fecha_fin
            },
            success: function (result) {
                jsRemoveWindowLoad();
                if (result.length > 5) {
                    try {
                        window.open(result, '_blank');
                        iziToast.success({
                            title: 'TXT Polizas Almacen correctamente!',
                            message: '',
                        });
                    } catch (e) {
                        iziToast.error({
                            title: 'Ocurrió un error al generar TXT',
                            message: 'ERROR EN DESCARGA AUTOMATICA',
                        });
                    }

                }
                else {
                    if (result == "1") {
                        iziToast.error({
                            title: 'Ocurrió un error al generar TXT',
                            message: 'ERROR EN ALGORITMO DE DATOS',
                        });
                    }
                    else if (result == "-1") {
                        iziToast.error({
                            title: 'ERROR AL GENERAR EL CONSECUTIVO DE REFERENCIA DE LA POLIZA',
                            message: '',
                        });
                    }
                    else if (result == "-2") {
                        iziToast.error({
                            title: 'ERROR AL FORMATEAR LAS FECHAS',
                            message: '',
                        });
                    }
                    else if (result == "-3") {
                        iziToast.error({
                            title: 'ERROR AL CONSULTAR EL TIPO DE POLIZA',
                            message: '',
                        });
                    }
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    
}



//#region POLIZA SEMANAL
function GenerarPolizasAlmacenSemanal() {
    jsShowWindowLoad();
    var almacen = $("#id_almacen_g_poliza").val();
    var fecha1 = $("#fecha_inicio_poliza").val();
    var fecha2 = $("#fecha_fin_poliza").val();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000,
        url: "../ALMACEN/GenerarPolizasAlmacenSemanal",
        data: {
            id_almacen_g_poliza: almacen,
            fecha_i: fecha1,
            fecha_f: fecha2
        },
        success: function (response) {
            try {
                $("#tabla_poliza").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_poliza_semanal").html(response);
            $('#tabla_poliza').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                select: true,
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
                }]
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function GenerarPolizasAlmacenSemanalDetalle() {
    jsShowWindowLoad();
    var almacen = $("#id_almacen_g_poliza").val();
    var fecha1 = $("#fecha_inicio_poliza").val();
    var fecha2 = $("#fecha_fin_poliza").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALMACEN/GenerarPolizasAlmacenSemanalDetalle",
        data: {
            id_almacen_g_poliza: almacen,
            fecha_i: fecha1,
            fecha_f: fecha2
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_poliza_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_poliza_semanal_detalle").html(response);
            $('#tabla_poliza_detalle').DataTable({
                keys: false,
                ordering: false,
                select: true,
                searching: true,
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
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ObtenerLunesSemanaActual(dia) {
    dia = new Date(dia);
    var dias = dia.getDay(),
        diferencia = dia.getDate() - dias + (dias == 0 ? -6 : 1);
    return new Date(dia.setDate(diferencia));
}

function ObtenerDomingoSemanaActual(dia) {
    dia = new Date(dia);
    var dias = dia.getDay(),
        diferencia = dia.getDate() - dias + 7;
    return new Date(dia.setDate(diferencia));
}

function FormaterFecha(fecha) {
    var d = new Date(fecha),
        mes = '' + (d.getMonth() + 1),
        dia = '' + d.getDate(),
        ano = d.getFullYear();

    if (mes.length < 2) mes = '0' + mes;
    if (dia.length < 2) dia = '0' + dia;

    return [ano, mes, dia].join('-');
}
function EstablecerDias() {
    var Hoy = new Date();
    var Lunes = ObtenerLunesSemanaActual(Hoy);
    var Domingo = ObtenerDomingoSemanaActual(Hoy);

    $('#fecha_inicio').val(FormaterFecha(Lunes));
    $('#fecha_fin').val(FormaterFecha(Domingo));
}
//#endregion

//-----------POLIZAS CONTABLES ALMACEN

