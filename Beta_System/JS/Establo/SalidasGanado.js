$('input[type="date"]').val(today);

var count_tabla = 0;

$('#ficha_g').keyup(function (event) {
    if (event.keyCode === 13) {
        if ($("#ficha_g").val() == "") { $(".input_ficha").val('') }
        else { ConsultarInfoFicha(); }
    }
});

function ValidarAreteExistente() {
    var areteIngreso = $("#arete").val().trim().toLowerCase();

    // Validación local tipo C#: ignorar ciertos valores
    if (areteIngreso === "sa" || areteIngreso === "sinarete" || areteIngreso === "sin arete") {
        return false;
    }

    var encontrado = false;

    // Verificar si el arete ya existe en la lista local
    $(".aretes").each(function () {
        var id = $(this).attr("id").split('_')[1];
        var arete = $("#arete_" + id).text().trim().toLowerCase();

        if (arete === areteIngreso) {
            encontrado = true;
            return false;
        }
    });

    if (encontrado) return true;

    // Si no se encontró localmente, validar con el servidor
    var existe = false;
    $.ajax({
        url: "../SALIDASGANADO/ValidarArete",
        async: false,
        type: "POST",
        data: { arete: areteIngreso },
        success: function (result) {
            if (result === true || result === "true" || result === "True") {
                existe = true;
            }
        }
    });

    return existe;
}

function ShowHideTipoSalida(modo) {
    $("#ficha_g").val("");
    LimpiarCaptura();
    if (modo == 1) {
        $(".tipo_salida").css("display", "inline");
        $(".input_ficha").attr("readonly", "true");
    }
    else {
        $(".tipo_salida").css("display", "none");
        $(".input_ficha").removeAttr("readonly");
    }
}


function GenerarFolioEstablo() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        url: "../SALIDASGANADO/GenerarFolioEstablo",
        async: false,
        timeout: 900000,
        type: "POST",
        data: { id_establo: id_establo },
        success: function (result) {
            $("#folio_g").val(result);
        }
    });

}

function ConsultarInfoFicha() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo").val();
    var ficha = $("#ficha_g").val();
    $.ajax({
        url: "../SALIDASGANADO/ConsultarInfoFicha",
        async: true,
        type: "POST",
        data: {
            id_establo: id_establo,
            ficha: ficha
        },
        success: function (result) {
            jsRemoveWindowLoad();
            if (result != "[]" && result != "0" && result != "-1") {
                var jsonData = result.split('|');

                var data = $.parseJSON(jsonData[0]);
                $("#comprador_g").val(data[0].despro);
                $("#peso_1_g").val(data[0].peso1);
                $("#peso_2_g").val(data[0].peso2);
                $("#peso_t_g").val(data[0].peso_t);
                $("#chofer_g").val(data[0].chofer);
                $("#placas_g").val(data[0].placas);
                $("#vehiculo_g").val(data[0].propietario_camion);
                //$("#pesador_g").val(data[0].usuario);
                $("#pesador_g").val(jsonData[1]);

            }
            else if (result == "0") {
                iziToast.error({
                    title: 'La ficha no corresponde a una salida de ganado',
                    message: '',
                });
                LimpiarCaptura();
            }
            else if (result == "-1") {
                iziToast.error({
                    title: 'La ficha ya fue registrada con una salida de ganado',
                    message: '',
                });
                LimpiarCaptura();
            }
            else {
                iziToast.error({
                    title: 'No se encontró ninguna ficha de ganado',
                    message: '',
                });
                LimpiarCaptura();
            }
        }
    });
}

function ConsultarCondicionesClasificacion() {
    var id_clasificacion = $("#ganado_g").val();
    $.ajax({
        url: "../SALIDASGANADO/ConsultarCondicionesClasificacion",
        async: false,
        type: "POST",
        data: { id_clasificacion: id_clasificacion },
        success: function (result) {
            $("#condicion_g").html(result);
        }
    });
}

function AgregarVacaTabla() {
    var peso_neto = $("#peso_neto").val();
    var cantidad = $("#cantidad").val();
    var p_unitario = $("#p_unitario").val();
    var condicion = $("#condicion_g").find('option:selected').text();

    if (peso_neto == "" || cantidad == "" || condicion == "") {
        iziToast.error({
            title: 'Ingrese los datos de la salida',
            message: '',
        });
        $(".input_valid").each(function () {
            if ($(this).val() == "") { $(this).css("border-color", "red"); }
            else { $(this).css("border", ""); }
        });
    }
    else if (cantidad <= 0) {
        iziToast.error({
            title: 'Ingrese una cantidad mayor a 0',
            message: '',
        });
        $(".input_valid").each(function () {
            $(this).css("border", "");
        });
        $("#cantidad").css("border-color", "red")
    }
    else {
        if (p_unitario == "") {
            p_unitario = 0;
        }

        var arete = $("#arete").val();
        var clasif = $("#ganado_g").find('option:selected').text();
        var causa = $("#causa").val();
        var sala = $("#sala").val();
        var edad = $("#edad").val();
        var importe = parseFloat(peso_neto * p_unitario).toFixed(2);


        if (ValidarAreteExistente() == true) {
            iziToast.warning({
                title: 'Aviso',
                message: 'Ya se esta utilizando el arete',
            });
            return;
        }


        $.ajax({
            url: "../SALIDASGANADO/AgregarVacaTabla",
            async: false,
            type: "POST",
            data: {
                count_tabla: count_tabla,
                arete: arete,
                clasif: clasif,
                causa: causa,
                sala: sala,
                edad: edad,
                peso_neto: peso_neto,
                cantidad: cantidad,
                p_unitario: p_unitario,
                importe: importe,
                condicion: condicion
            },
            success: function (result) {
                $(".input_detalle").val('');
                $("#thead_ganado").append(result);
                count_tabla++;

                $(".input_valid").each(function () {
                    $(this).css("border", "");
                });
                CalcularTotales();
            }
        });
    }
}

function EliminarFilaSalida(count) {
    $("#row_" + count + "").remove();
    CalcularTotales();
}

function GenerarSalidaGanado() {
    var tipo_salida = $("input[name='tipo_salida']:checked").val();
    var peso_t = $("#peso_t_g").val();
    var peso_total = $("#kilos_total").val();

    var count = 0;
    var aretes = [];
    var clasificaciones = [];
    var causas = [];
    var salas = [];
    var condiciones = [];
    var edades = [];
    var pesos_neto = [];
    var cantidades = [];
    var p_unitarios = [];
    var importes = [];

    var arete = $(".aretes");
    if (arete.length == 0) {
        iziToast.error({
            title: 'Ingrese el detalle de la salida',
            message: '',
        });
    }
    else if (tipo_salida == undefined || isNaN(tipo_salida)) {
        iziToast.error({
            title: 'Seleccione el tipo de salida',
            message: '',
        });
    }
    else if (parseFloat(peso_t) != parseFloat(peso_total) && tipo_salida == 1) {
        iziToast.error({
            title: 'El total de kilos no coincide con el peso de la Tara de la ficha',
            message: 'Verifique los pesos',
        });
    }
    else {
        jsShowWindowLoad();
        arete.each(function () {
            var id = $(this).attr("id").split('_')[1];
            var p_unita = $("#p_unitario_" + id + "").text();
            if (p_unita == "") {
                p_unita = 0;
            }

            aretes[count] = $("#arete_" + id + "").text();
            clasificaciones[count] = $("#clasif_" + id + "").text();
            causas[count] = $("#causa_" + id + "").text();
            condiciones[count] = $("#condicion_" + id + "").text();
            salas[count] = $("#sala_" + id + "").text();
            edades[count] = $("#edad_" + id + "").text();;
            cantidades[count] = $("#cantidad_" + id + "").text();
            pesos_neto[count] = $("#peso_" + id + "").text();
            p_unitarios[count] = p_unita;
            importes[count] = $("#importe_" + id + "").text();
            count++;
        });

        var salida = {};
        salida.id_establo = $("#id_establo").val();
        salida.folio = $("#folio_g").val();
        salida.ficha = $("#ficha_g").val();
        salida.ganado = $("#ganado_g").find('option:selected').text();
        salida.condicion = $("#condicion_g").find('option:selected').text();
        salida.peso1 = $("#peso_1_g").val();
        salida.peso2 = $("#peso_2_g").val();
        salida.peso_t = $("#peso_t_g").val();
        salida.pesador = $("#pesador_g").val();
        salida.chofer = $("#chofer_g").val();
        salida.placas = $("#placas_g").val();
        salida.vehiculo = $("#vehiculo_g").val();
        salida.comprador = $("#comprador_g").val();
        salida.importe = $("#importes_total").val();
        $.ajax({
            url: "../SALIDASGANADO/GenerarSalidaGanado",
            async: false,
            timeout: 900000,
            type: "POST",
            data: { salida: salida, tipo_salida: tipo_salida },
            success: function (id_salida_g) {
                jsShowWindowLoad();
                if (id_salida_g != 0 && id_salida_g != -1 && id_salida_g != -2) {
                    $.ajax({
                        url: "../SALIDASGANADO/GenerarSalidaGanadoDetalle",
                        async: false,
                        timeout: 900000,
                        type: "POST",
                        data: {
                            folio_g: $("#folio_g").val(),
                            id_salida_g: id_salida_g,
                            aretes: aretes,
                            clasificaciones: clasificaciones,
                            causas: causas,
                            condiciones: condiciones,
                            salas: salas,
                            edades: edades,
                            cantidades: cantidades,
                            pesos_neto: pesos_neto,
                            p_unitarios: p_unitarios,
                            importes: importes
                        },
                        success: function (result) {
                            jsRemoveWindowLoad();
                            if (result != "") {
                                iziToast.success({
                                    title: 'Exito!',
                                    message: 'Salida de ganado registrada correctamente',
                                });
                                LimpiarCaptura();
                                GenerarFolioEstablo();
                                $(".input_valid").each(function () {
                                    $(this).css("border", "");
                                });
                                GenerarPDFSalidaGanado(id_salida_g, salida.folio);
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un problema al guardar el detalle de la salida',
                                    message: '',
                                });
                            }
                        }
                    });
                }
                else {
                    GenerarFolioEstablo();
                    jsRemoveWindowLoad();
                    if (id_salida_g == 0) {
                        iziToast.error({
                            title: 'Ocurrió un problema al guardar la información general',
                            message: '',
                        });
                    }
                    if (id_salida_g == -1) {
                        iziToast.error({
                            title: 'Error.',
                            message: 'Ya existen registros con este Folio',
                        });
                    }
                    if (id_salida_g == -2) {
                        iziToast.error({
                            title: 'Error al buscar la fecha de la ficha.',
                            message: '',
                        });
                    }
                }
            }
        });

    }
}

function LimpiarCaptura() {
    $("#thead_ganado").html("");
    $(".input_clean").val("");
    CalcularTotales();
}

function ConsultarSalidasEstablo() {
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    var id_establo = $("#id_establo_consulta").val();
    jsShowWindowLoad();
    $.ajax({
        url: "../SALIDASGANADO/ConsultarSalidasEstablo",
        async: true,
        type: "POST",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (result) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_salidas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_salidas").html(result);
            $('#tabla_salidas').DataTable({
                keys: false,
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
                scrollCollapse: true,
                scrollY: '350px',
                responsive: !0
            });

        }
    });
}

function ExportarSalidasGanadoExcel() {
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    var id_establo = $("#id_establo_consulta").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../SALIDASGANADO/ExportarSalidasGanadoExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                id_establo: id_establo,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok.');
                }
                return response.blob().then(blob => {
                    var disposition = response.headers.get('Content-Disposition');
                    var filename = 'Reporte_Fuera_de_Rango.xlsx';

                    if (disposition && disposition.indexOf('attachment') !== -1) {
                        var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                        var matches = filenameRegex.exec(disposition);
                        if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                    }

                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = filename;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                });
            })
            .catch(error => {
                console.error('Error:', error);
            })
            .finally(() => {
                jsRemoveWindowLoad();
            });
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}

function CalcularTotales() {
    var sum_cantidades = 0;
    var sum_kilos = 0;
    var sum_importes = 0;
    $(".cantidades").each(function () {
        sum_cantidades += parseFloat($(this).text());
    });
    $(".pesos").each(function () {
        sum_kilos += parseFloat($(this).text());
    });
    $(".importes_d").each(function () {
        sum_importes += parseFloat($(this).text());
    });

    $("#cantidad_total").val(sum_cantidades);
    $("#kilos_total").val(sum_kilos.toFixed(2));
    $("#importes_total").val(sum_importes.toFixed(2));
}

function GenerarPDFSalidaGanado(id_salida_g, folio) {
    jsShowWindowLoad();
    $.ajax({
        url: "../SALIDASGANADO/GenerarPDFSalidaGanado",
        async: false,
        timeout: 900000,
        type: "POST",
        data: { id_salida_g: id_salida_g },
        success: function (result) {
            jsRemoveWindowLoad();
            $("#div_pdf_salida").css("display", "block");
            $("#div_pdf_salida").html(result);
            var HTML_Width = $("#div_pdf_salida").width();
            var HTML_Height = $("#div_pdf_salida").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#div_pdf_salida")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/png", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Salida de ganado #" + folio + ".pdf");
            });
            $("#div_pdf_salida").css("display", "none");
        }
    });
}


//#region  INDICADORES
function ConsultarGraficasReporteSalidasGanado() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo_indicadores").val();
    var fecha_inicio = $("#fecha_inicio_indicadores").val();
    var fecha_fin = $("#fecha_fin_indicadores").val();
    var id_tipo_salida = $("#id_tipo_salida_indicadores").val();
    var id_tipo_muerte = $("#tipo_muerte_indicadores").val();
    try {
        ConsultarReporteTiposMuerte(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte);
        ConsultarReporteCausasMuerte(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte);
        ConsultarReporteIndicadoresTable(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte);

    } catch (e) {

    }
}

function ConsultarReporteTiposMuerte(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../SALIDASGANADO/ConsultarReporteTiposMuerte",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_tipo_salida: id_tipo_salida,
            id_tipo_muerte: id_tipo_muerte
        },
        success: function (response) {
            jsRemoveWindowLoad();
            var data = $.parseJSON(response);
            var obj = [];
            for (var i = 0; i < data.length; i++) {
                obj.push({ value: data[i].Valor, name: data[i].Nombre });
            }

            var fecha_i = new Date(fecha_inicio +"T00:00:00");
            var opciones_i = { day: '2-digit', month: 'short', year: 'numeric' };
            var fechaFormateada_i = fecha_i.toLocaleDateString('es-ES', opciones_i);

            var fecha_f = new Date(fecha_fin +"T00:00:00");
            var opciones_f = { day: '2-digit', month: 'short', year: 'numeric' };
            var fechaFormateada_f = fecha_f.toLocaleDateString('es-ES', opciones_f);

            var chartDom = document.getElementById('div_chart_tipos_muerte');
            var myChart = echarts.init(chartDom);
            var option = {
                title: {
                    text: 'CAUSA DE SALIDA',
                    subtext: fechaFormateada_i + " - " + fechaFormateada_f,
                    left: 'center'
                },
                tooltip: {
                    trigger: 'item'
                },
                legend: {
                    orient: 'vertical',
                    left: 'left'
                },
                series: [
                    {
                        name: '',
                        type: 'pie',
                        radius: '50%',
                        data: obj,
                        emphasis: {
                            itemStyle: {
                                shadowBlur: 10,
                                shadowOffsetX: 0,
                                shadowColor: 'rgba(0, 0, 0, 0.5)'
                            }
                        }
                    }
                ]
            };
            option && myChart.setOption(option);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarReporteCausasMuerte(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../SALIDASGANADO/ConsultarReporteCausasMuerte",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_tipo_salida: id_tipo_salida,
            id_tipo_muerte: id_tipo_muerte
        },
        success: function (response) {
            jsRemoveWindowLoad();
            var data = $.parseJSON(response);
            var obj = [];
            for (var i = 0; i < data.length; i++) {
                obj.push({ value: data[i].Valor, name: data[i].Nombre });
            }

            var chartDom = document.getElementById('div_chart_causas_muerte');
            var myChart = echarts.init(chartDom);

            var fecha_i = new Date(fecha_inicio + "T00:00:00");
            var opciones_i = { day: '2-digit', month: 'short', year: 'numeric' };
            var fechaFormateada_i = fecha_i.toLocaleDateString('es-ES', opciones_i);

            var fecha_f = new Date(fecha_fin + "T00:00:00");
            var opciones_f = { day: '2-digit', month: 'short', year: 'numeric' };
            var fechaFormateada_f = fecha_f.toLocaleDateString('es-ES', opciones_f);

            var option = {
                title: {
                    text: 'DETALLE',
                    subtext: fechaFormateada_i + " - " + fechaFormateada_f,
                    left: 'center'
                },
                tooltip: {
                    trigger: 'item'
                },
                legend: {
                    orient: 'vertical',
                    left: 'left'
                },
                series: [
                    {
                        name: '',
                        type: 'pie',
                        radius: '50%',
                        data: obj,
                        emphasis: {
                            itemStyle: {
                                shadowBlur: 10,
                                shadowOffsetX: 0,
                                shadowColor: 'rgba(0, 0, 0, 0.5)'
                            }
                        }
                    }
                ]
            };
            option && myChart.setOption(option);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarReporteIndicadoresTable(id_establo, fecha_inicio, fecha_fin, id_tipo_salida, id_tipo_muerte) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../SALIDASGANADO/ConsultarReporteIndicadoresTable",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_tipo_salida: id_tipo_salida,
            id_tipo_muerte: id_tipo_muerte
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_indicadores").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_datatable_indicadores").html(response);
            $('#datatable_indicadores').DataTable({
                keys: false,
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
                scrollCollapse: true,
                scrollY: '350px',
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion