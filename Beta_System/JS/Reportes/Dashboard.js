


//#region  JEFA COMPPRAS
function ConsultarCotizacionesComprasAnual() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../DASHBOARD/ConsultarCotizacionesComprasAnual",
        data: { id_rol: '3010,3009,3040,3041' },
        success: function (response) {
            jsRemoveWindowLoad();
            var data = $.parseJSON(response);
            var chartDom = document.getElementById('div_grafica_compras_anuales');
            var myChart = echarts.init(chartDom);
            var option;

            option = {
                legend: {
                    text: 'COMPRAS ANUALES',
                    subtext: '',
                    left: 'center'
                },
                tooltip: {
                    trigger: 'axis',
                    showContent: false
                },
                dataset: {
                    source: data
                },
                xAxis: { type: 'category' },
                yAxis: {
                    gridIndex: 0,
                    type: 'value',
                    axisLabel: {
                        formatter: function (value) {
                            return value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                        }
                    }
                },
                grid: { top: '55%', left: '15%' },
                series: [
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'pie',
                        id: 'pie',
                        radius: '35%',
                        center: ['45%', '30%'],
                        emphasis: {
                            focus: 'self'
                        },
                        label: {
                            formatter: '{b}: {@Enero} ({d}%)'
                        },
                        encode: {
                            itemName: 'Meses',
                            value: 'Enero',
                            tooltip: 'Enero'
                        }
                    }
                ]
            };
            myChart.on('updateAxisPointer', function (event) {
                const xAxisInfo = event.axesInfo[0];
                if (xAxisInfo) {
                    const dimension = xAxisInfo.value + 1;
                    myChart.setOption({
                        series: {
                            id: 'pie',
                            label: {
                                formatter: '{b}: {@[' + dimension + ']} ({d}%)'
                            },
                            encode: {
                                value: dimension,
                                tooltip: dimension
                            }
                        }
                    });
                }
            });
            myChart.setOption(option);

            option && myChart.setOption(option);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarComprasPreciosArticuloMes() {
    var fecha_inicio = $("#fechas_inicio_historial").val();
    var fecha_fin = $("#fechas_fin_historial").val();
    var clave = $("#clave_buscar").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarComprasPreciosArticuloMes",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            clave: clave
        },
        success: function (response) {
            jsRemoveWindowLoad();
            var data = $.parseJSON(response);
            var chartDom = document.getElementById('div_grafica_cotizaciones');
            var myChart = echarts.init(chartDom);
            var option = {
                title: {
                    text: 'Dynamic Data & Time Axis'
                },
                tooltip: {
                    trigger: 'axis',
                    formatter: function (params) {
                        params = params[0];
                        var date = new Date(params.name);
                        return (
                            date.getDate() +
                            '/' +
                            (date.getMonth() + 1) +
                            '/' +
                            date.getFullYear() +
                            ' : ' +
                            params.value[1]
                        );
                    },
                    axisPointer: {
                        animation: false
                    }
                },
                xAxis: {
                    type: 'time',
                    splitLine: {
                        show: false
                    }
                },
                yAxis: {
                    type: 'value',
                    boundaryGap: [0, '100%'],
                    splitLine: {
                        show: false
                    }
                },
                series: [
                    {
                        name: 'Fake Data',
                        type: 'line',
                        showSymbol: false,
                        data: data
                    }
                ]
            };
            option && myChart.setOption(option);

        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

function ConsultarComprasDirectasMensualesPorAnio() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../DASHBOARD/ConsultarComprasDirectasMensualesPorAnio",
        data: { anio: new Date().getFullYear() },
        success: function (response) {
            jsRemoveWindowLoad();
            var data = $.parseJSON(response);
            var chartDom = document.getElementById('div_grafica_compras_mensuales_tipos');
            var myChart = echarts.init(chartDom);
            var option;

            option = {
                legend: {
                    text: 'COMPRAS MENSUALES',
                    subtext: '',
                    left: 'center'
                },
                tooltip: {
                    trigger: 'axis',
                    showContent: false
                },
                dataset: {
                    source: data
                },
                xAxis: { type: 'category' },
                yAxis: {
                    gridIndex: 0,
                    type: 'value'
                },
                grid: { top: '60%' },
                series: [
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'line',
                        smooth: true,
                        seriesLayoutBy: 'row',
                        emphasis: { focus: 'series' }
                    },
                    {
                        type: 'pie',
                        id: 'pie',
                        radius: '35%',
                        center: ['50%', '30%'],
                        emphasis: {
                            focus: 'self'
                        },
                        label: {
                            formatter: '{b}: {@Enero} ({d}%)'
                        },
                        encode: {
                            itemName: 'Meses',
                            value: 'Enero',
                            tooltip: 'Enero'
                        }
                    }
                ]
            };
            myChart.on('updateAxisPointer', function (event) {
                const xAxisInfo = event.axesInfo[0];
                if (xAxisInfo) {
                    const dimension = xAxisInfo.value + 1;
                    myChart.setOption({
                        series: {
                            id: 'pie',
                            label: {
                                formatter: '{b}: {@[' + dimension + ']} ({d}%)'
                            },
                            encode: {
                                value: dimension,
                                tooltip: dimension
                            }
                        }
                    });
                }
            });
            myChart.setOption(option);

            option && myChart.setOption(option);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarTopProveedoresCompras() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../DASHBOARD/ConsultarTopProveedoresCompras",
        data: { fecha_inicio: "", fecha_fin : "" },
        success: function (response) {
            $("#div_grafica_compras_proveedores").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion


//#region SISTEMAS
function ConsultarLogSemanalGrafica() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarLogSemanalGrafica",
        data: { fecha_inicio: "" },
        success: function (response) {
            var data = $.parseJSON(response);
            var chartDom = document.getElementById('div_grafica_visitantes_semanales');
            var myChart = echarts.init(chartDom);
            var option;
            option = {
                xAxis: {
                    type: 'category',
                    data: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
                },
                yAxis: {
                    type: 'value'
                },
                title: {
                    text: 'SESIONES DIARIAS',
                    left: 'center'
                },
                legend: {
                    orient: 'vertical',
                    left: 'left'
                },
                series: [
                    {
                        data: data,
                        type: 'bar'
                    }
                ]
            };

            option && myChart.setOption(option);

        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

}

function ConsultarLogsProcesos() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarLogsProcesos",
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_log_procesos").html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

function Consultar_KPIS_Sistemas() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/Consultar_KPIS_Sistemas",
        success: function (response) {
            $("#div_kpis_dev").html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

function ConsultarLogSemanalCorreosGrafica() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarLogSemanalCorreosGrafica",
        data: { fecha_inicio: "" },
        success: function (response) {
            var data = $.parseJSON(response);
            var chartDom = document.getElementById('div_grafica_correos_enviados');
            var myChart = echarts.init(chartDom);
            var option;

            option = {
                xAxis: {
                    type: 'category',
                    data: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']
                },
                yAxis: {
                    type: 'value'
                },
                title: {
                    text: 'CORREOS ENVÍADOS',
                    left: 'center'
                },
                legend: {
                    orient: 'vertical',
                    left: 'left'
                },
                series: [
                    {
                        data: data,
                        type: 'bar'
                    }
                ]
            };

            option && myChart.setOption(option);

        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });

}

//#endregion


//#region ALMACEN
function ConsultarValesPendientesAlmacen() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarValesPendientesAlmacen",
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_vales_pendientes").html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}

function ConsultarRequisicionesGeneradasUsuario() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarRequisicionesGeneradasUsuario",
        success: function (response) {
            $("#div_requisiciones_generadas").html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}


function ConsultarOrdenesPendientesRecepcion() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../DASHBOARD/ConsultarOrdenesPendientesRecepcion",
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_recepciones_pendientes").html(response);
        },
        error: function (xhr, status, error) {
            alert(error);
        }
    });
}


//#endregion

