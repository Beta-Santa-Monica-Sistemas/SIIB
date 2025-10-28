//#region COMPRAS VS PRESUPUESTO
//--------------COMPRAS VS PRESUPUESTO

function ConsultarComparativoCompraPresupuesto() {
    var div_reporte = $("#div_reporte");
    div_reporte.html("");
    jsShowWindowLoad();
    var id_anio = $("#id_anio_presupuesto").val();
    var id_cargos_contable = $("#id_cargo_contable").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999999,
        url: "../REPORTES/ConsultarComparativoCompraPresupuesto",
        data: {
            id_anio: id_anio,
            id_cargos_contable: id_cargos_contable
        },
        success: function (response) {
            div_reporte.html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarOrdenesEjericdasMesDetalle(id_cuenta, no_mes, total_ejercido, total_presupuesto, anio_numero, numero_cuenta, nombre_cuenta, nombre_cargo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarOrdenesEjericdasMesDetalle",
        data: {
            id_cuenta: id_cuenta,
            no_mes: no_mes,
            total_ejercido: total_ejercido,
            total_presupuesto: total_presupuesto,
            anio_numero: anio_numero,
            numero_cuenta: numero_cuenta,
            nombre_cuenta: nombre_cuenta,
            nombre_cargo: nombre_cargo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_detalle_ordenes_mes").dataTable().fnDestroy();
            } catch (e) { }

            try {
                $("#datatable_detalle_ordenes_mes_consolidado").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_detalle_ordenes_mes").html(response);
            $('#datatable_detalle_ordenes_mes').DataTable({
                keys: false,
                select: true,
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
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


            $('#datatable_detalle_ordenes_mes_consolidado').DataTable({
                keys: false,
                ordering: false,
                paging: false,
                select: true,
                dom: "Bfrtip",
                scrollCollapse: false,
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




            $("#m_detalle_ordenes_mes").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GetLinkDescargarConsolidadoExcel() {
    var id_anio = $("#id_anio_presupuesto").val();
    var id_cargos_contable = $("#id_cargo_contable").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/GetLinkDescargarConsolidadoExcel",
        data: {
            id_anio: id_anio,
            id_cargos_contable: id_cargos_contable
        },
        success: function (response) {
            window.open(response, '_blank');

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion


//#region GASTOS MENSUALES

//---------------GASTOS MENSUALES
function ConsultarGastosMensualesCompraConsumo() {
    var id_cargo_contable = $("#id_cargo_contable").val();
    var id_anio = $("#id_anio").val();
    var id_mes = $("#id_mes").val();
    var anual = false;
    if ($('#anual').is(':checked')) { anual = true; }


    if (id_cargo_contable == undefined || id_cargo_contable == '') {
        iziToast.warning({
            title: 'Seleccione un cargo contable',
            message: '',
        });
    }
    else if (id_mes == '' || id_mes == undefined) {
        iziToast.warning({
            title: 'Seleccione el campo mes',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 99999999999,
            url: "../REPORTES/ConsultarGastosMensualesCompraConsumo",
            data: {
                id_cargo_contable: id_cargo_contable,
                id_anio: id_anio,
                id_mes: id_mes,
                anual: anual
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datable_gastos_mensuales_compra_consumo").dataTable().fnDestroy();
                } catch (e) { }

                $("#div_gastos_mensuales_compra_consumo").html(response);
                $('#datable_gastos_mensuales_compra_consumo').DataTable({
                    keys: false,
                    ordering: true,
                    searching: true,
                    paging: false,
                    select: true,
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

            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}

function ConsultarDetalleConsumoAlmacenCuentaContable(fecha_inicio, fecha_fin, id_cuenta_contable, id_cargo_contable) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarDetalleConsumoAlmacenCuentaContable",
        data: {
            id_cuenta_contable: id_cuenta_contable,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_consumo_almacen_detalle").modal("show");

            try {
                $("#datatable_consumo_almacen_detalle").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_consumo_almacen_detalle").html(response);
            $('#datatable_consumo_almacen_detalle').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                select: true,
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

function ConsultarDetalleComprasDirectasCuentaContable(fecha_inicio, fecha_fin, id_cuenta_contable) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarDetalleComprasDirectasCuentaContable",
        data: {
            id_cuenta_contable: id_cuenta_contable,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_compras_directas_detalle").modal("show");

            try {
                $("#datatable_compras_directas_detalle").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_compras_directas_detalle").html(response);
            $('#datatable_compras_directas_detalle').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                select: true,
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


function ConsultarDetalleConsolidadoCuentaContable(fecha_inicio, fecha_fin, id_cuenta_contable, id_cargo_contable) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 99999999999,
        url: "../REPORTES/ConsultarDetalleConsolidadoCuentaContable",
        data: {
            id_cuenta_contable: id_cuenta_contable,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_consolidado_detalle").modal("show");

            try {
                $("#datatable_consolidado_detalle").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_consolidado_detalle").html(response);
            $('#datatable_consolidado_detalle').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                select: true,
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

function DescargarExcelConsolidadoComprasDirectasCargoContable() {
    var id_cargo_contable = $("#id_cargo_contable").val();
    var id_anio = $("#id_anio").val();
    var id_mes = $("#id_mes").val();
    var anual = false;
    if ($('#anual').is(':checked')) { anual = true; }


    if (id_cargo_contable == undefined || id_cargo_contable == '') {
        iziToast.warning({
            title: 'Seleccione un cargo contable',
            message: '',
        });
    }
    else if (id_mes == '' || id_mes == undefined) {
        iziToast.warning({
            title: 'Seleccione el campo mes',
            message: '',
        });
    }
    else {
        var url = "https://siib.beta.com.mx/REPORTES/DescargarExcelConsolidadoComprasDirectasCargoContable?id_cargo_contable=" + id_cargo_contable + "&&id_anio=" + id_anio + "&&id_mes=" + id_mes + "&&anual=" + anual +"";
        //var url = "https://localhost:44371//REPORTES/DescargarExcelConsolidadoComprasDirectasCargoContable?id_cargo_contable=" + id_cargo_contable + "&&id_anio=" + id_anio + "&&id_mes=" + id_mes + "&&anual=" + anual +"";
        window.open(url, '_blank');
    }
    
}



//#endregion


//#region SEGUIMIENTO DE REQUISICIONES

function ConsultarRequisicionesSeguimiento() {

    var id_usuario = $("#id_usuario").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        clearTimeout: 9000000,
        url: "../REPORTES/ConsultarRequisicionesSeguimiento",
        data: {
            id_usuario: id_usuario,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_seguimiento_requisiciones").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_seguimiento_requisiciones").html(response);
            $('#datatable_seguimiento_requisiciones').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                select: true,
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

function DescargarExcelRequisicionesSeguimiento() {
    var id_usuario = $("#id_usuario").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    var url = "https://siib.beta.com.mx/REPORTES/DescargarExcelRequisicionesSeguimiento?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "";
    //var url = "http://192.168.128.2:90/REPORTES/DescargarExcelRequisicionesSeguimiento?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin +"";
    //var url = "https://localhost:44371/REPORTES/DescargarExcelRequisicionesSeguimiento?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "";
    window.open(url, '_blank');
}

function ConsultarRequisicionesSeguimientoDetalle() {

    var id_usuario = $("#id_usuario_det").val();
    var fecha_inicio = $("#fecha_inicio_det").val();
    var fecha_fin = $("#fecha_fin_det").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        clearTimeout: 9000000,
        url: "../REPORTES/ConsultarRequisicionesSeguimientoDetalle",
        data: {
            id_usuario: id_usuario,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_seguimiento_requisiciones_detalle").dataTable().fnDestroy();
            } catch (e) { }

            $("#div_seguimiento_requisiciones_detalle").html(response);
            $('#datatable_seguimiento_requisiciones_detalle').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                select: true,
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
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DescargarExcelRequisicionesSeguimientoDetalle() {
    var id_usuario = $("#id_usuario_det").val();
    var fecha_inicio = $("#fecha_inicio_det").val();
    var fecha_fin = $("#fecha_fin_det").val();
    //var url = "https://localhost:44371//REPORTES/DescargarExcelRequisicionesSeguimientoDetalle?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "";
    var url = "https://siib.beta.com.mx/REPORTES/DescargarExcelRequisicionesSeguimientoDetalle?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "";
    //var url = "https://localhost:44371/REPORTES/DescargarExcelRequisicionesSeguimientoDetalle?id_usuario=" + id_usuario + "&&fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "";
    
    window.open(url, '_blank');
}


//#endregion


//#region INVERSIONES

function ReporteInversionTablePDF() {
    var anio = $("#anio_inversion_select option:selected").text();
    fetch('../COMPRAS/ReporteInversionTablePDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            anio: anio
        })
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok.');

            return response.blob().then(blob => {
                let disposition = response.headers.get('Content-Disposition');
                let filename = 'Reporte_Fuera_de_Rango.pdf';

                if (disposition && disposition.indexOf('attachment') !== -1) {
                    let matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }

                let link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            });
        })
        .catch(error => {
            console.error('Error:', error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error al descargar el PDF.'
            });
        })
        .finally(() => {
            jsRemoveWindowLoad();
        });
}

function DescargarExcelReporteInversionTable() {
    var anio = $("#anio_inversion_select option:selected").text();
    var url = "https://siib.beta.com.mx//REPORTES/DescargarExcelReporteInversionTable?anio=" + anio + "";
    //var url = "https://localhost:44371//REPORTES/DescargarExcelReporteInversionTable?anio=" + anio + "";
    //var url = "http://192.168.128.2:90//REPORTES/DescargarExcelReporteInversionTable?anio=" + anio + "";

    window.open(url, '_blank');
}

//#endregion


//#region REPORTES ORDENES DE COMPRA
function ConsultarReporteOrdenesCompraTable() {
    var id_clasificaciones = $("#id_clasificaciones").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    if (id_clasificaciones == undefined || id_clasificaciones == '') {
        iziToast.warning({
            title: 'Seleccione una clasificación',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: true,
        timeout: 99999999,
        url: "../REPORTES/ConsultarReporteOrdenesCompraTable",
        data: {
            id_clasificaciones: id_clasificaciones,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_reporte_ordenes_compra").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_ordenes_compra").html(response);
            $('#datatable_reporte_ordenes_compra').DataTable({
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
                pageLength: 30,
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

//#endregion


