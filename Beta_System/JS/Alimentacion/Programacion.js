function CalcularSemanasNuevoPrograma() {
    var fecha_inicio = $("#fecha_incio_gen_prg").val();
    var no_semanas = $("#no_semanas_gen_prg").val();

    if (no_semanas <= 0) {
        iziToast.warning({
            title: 'Atención',
            message: 'El numero de semanas debe ser positivo',
        });
    }
    else if (no_semanas == "" || no_semanas == undefined || no_semanas > 5) {
        iziToast.warning({
            title: 'Atención',
            message: 'El numero de semanas debe ser máximo 5',
        });
    }
    else if (fecha_inicio == "" || fecha_inicio == undefined) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese la fecha de inicio',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            timeout: 900000,
            url: "../ALIMENTACION/CalcularSemanasNuevoPrograma",
            data: {
                fecha_inicio: fecha_inicio,
                no_semanas: no_semanas
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#div_programacion_nueva").html(response);
                BloquearCamposPrg(1);
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}

function BloquearCamposPrg(modo) {
    if (modo == 1) {
        $(".form-secure").attr("readonly", "true");
        $(".form-secure").attr("disabled", "true");
        $("#btn_secure_prg").css("display", "block");
    }
    else {
        $(".form-secure").removeAttr("readonly");
        $(".form-secure").removeAttr("disabled");
        $("#btn_secure_prg").css("display", "none");
        $("#div_programacion_nueva").html("");
    }
}

function AgregarAlimentoNuevoPrograma(no_semana) {
    var nombre_articulo = $("#id_alimento_" + no_semana + " option:selected").text();
    var id_articulo = $("#id_alimento_" + no_semana + "").val();
    var lunes = $("#Lunes_" + no_semana + "").val();
    var martes = $("#Martes_" + no_semana + "").val();
    var miercoles = $("#Miercoles_" + no_semana + "").val();
    var jueves = $("#Jueves_" + no_semana + "").val();
    var viernes = $("#Viernes_" + no_semana + "").val();
    var sabado = $("#Sabado_" + no_semana + "").val();
    var domingo = $("#Domingo_" + no_semana + "").val();

    var fecha_lunes = $("#Lunes_" + no_semana + "").attr('name');
    var fecha_martes = $("#Martes_" + no_semana + "").attr('name');
    var fecha_miercoles = $("#Miercoles_" + no_semana + "").attr('name');
    var fecha_jueves = $("#Jueves_" + no_semana + "").attr('name');
    var fecha_viernes = $("#Viernes_" + no_semana + "").attr('name');
    var fecha_sabado = $("#Sabado_" + no_semana + "").attr('name');
    var fecha_domingo = $("#Domingo_" + no_semana + "").attr('name');

    if (lunes == "") { lunes = 0; }
    if (martes == "") { martes = 0; }
    if (miercoles == "") { miercoles = 0; }
    if (jueves == "") { jueves = 0; }
    if (viernes == "") { viernes = 0; }
    if (sabado == "") { sabado = 0; }
    if (domingo == "") { domingo = 0; }

    if (lunes < 0 || martes < 0 || miercoles < 0 || jueves < 0 || viernes < 0 || sabado < 0 || domingo < 0) {
        iziToast.warning({
            title: 'Asegurese de ingresar solo valores positivos',
            message: '',
        });
    }
    else {

        $("#IdArticuloNuevaPrg_" + id_articulo + "_" + no_semana + "").closest('tr').remove();

        var total = parseFloat(lunes) + parseFloat(martes) + parseFloat(miercoles) + parseFloat(jueves) + parseFloat(viernes) + parseFloat(sabado) + parseFloat(domingo);
        var count_tr = Math.floor(Math.random() * 99999) + 1;
        var tbody = $("#tbody_prg_" + no_semana + "");
        var row = $("<tr class='text-center' id='tr_semana_" + no_semana + "_" + count_tr + "'></tr>");

        row.append("<td style='display:none;' id='IdArticuloNuevaPrg_" + id_articulo + "_" + no_semana + "'>" + id_articulo + "</td>");
        row.append("<td class='NombresArticulosNuevoPrg' id='ArtNombre_" + id_articulo + "'>" + nombre_articulo + "</td>");

        row.append("<td class='ValoresProgramacion LunVal_" + id_articulo + "' id='prg_" + fecha_lunes + "_" + id_articulo + "'        name='Lun_" + id_articulo + "'>" + lunes + "</td>");
        row.append("<td class='ValoresProgramacion MarVal_" + id_articulo + "' id='prg_" + fecha_martes + "_" + id_articulo + "'       name='Mar_" + id_articulo + "'>" + martes + "</td>");
        row.append("<td class='ValoresProgramacion MieVal_" + id_articulo + "' id='prg_" + fecha_miercoles + "_" + id_articulo + "'    name='Mie_" + id_articulo + "'>" + miercoles + "</td>");
        row.append("<td class='ValoresProgramacion JueVal_" + id_articulo + "' id='prg_" + fecha_jueves + "_" + id_articulo + "'       name='Jue_" + id_articulo + "'>" + jueves + "</td>");
        row.append("<td class='ValoresProgramacion VieVal_" + id_articulo + "' id='prg_" + fecha_viernes + "_" + id_articulo + "'      name='Vie_" + id_articulo + "'>" + viernes + "</td>");
        row.append("<td class='ValoresProgramacion SabVal_" + id_articulo + "' id='prg_" + fecha_sabado + "_" + id_articulo + "'       name='Sab_" + id_articulo + "'>" + sabado + "</td>");
        row.append("<td class='ValoresProgramacion DomVal_" + id_articulo + "' id='prg_" + fecha_domingo + "_" + id_articulo + "'      name='Dom_" + id_articulo + "'>" + domingo + "</td>");
        row.append("<td class='totales_semana'>" + parseFloat(total).toFixed(2) + "</td>");

        row.append("<td><button class='btn btn-danger btn-xs' onclick='EliminarAlimentoNuevoPrograma(" + no_semana + "," + count_tr + ");'><i class='fa fa-remove'></i></button></td>");
        tbody.append(row);
        $(".valores_prg_" + no_semana + "").val('0');
        $("#total_art_" + no_semana + "").text('0');

        GenerarResumenProgramaNuevoAlimentos();
    }
}

function EliminarAlimentoNuevoPrograma(no_semana, count_tr) {
    $("#tr_semana_" + no_semana + "_" + count_tr + "").remove();
    GenerarResumenProgramaNuevoAlimentos();
}

function CalcularTotalSemanaAlimento(no_semana) {
    var lunes = $("#Lunes_" + no_semana + "").val();
    var martes = $("#Martes_" + no_semana + "").val();
    var miercoles = $("#Miercoles_" + no_semana + "").val();
    var jueves = $("#Jueves_" + no_semana + "").val();
    var viernes = $("#Viernes_" + no_semana + "").val();
    var sabado = $("#Sabado_" + no_semana + "").val();
    var domingo = $("#Domingo_" + no_semana + "").val();

    if (lunes == "") { lunes = 0; }
    if (martes == "") { martes = 0; }
    if (miercoles == "") { miercoles = 0; }
    if (jueves == "") { jueves = 0; }
    if (viernes == "") { viernes = 0; }
    if (sabado == "") { sabado = 0; }
    if (domingo == "") { domingo = 0; }

    var total = parseFloat(lunes) + parseFloat(martes) + parseFloat(miercoles) + parseFloat(jueves) + parseFloat(viernes) + parseFloat(sabado) + parseFloat(domingo);
    $("#total_art_" + no_semana + "").text(total);
}

function GenerarResumenProgramaNuevoAlimentos() {
    var tbody = $("#tbody_resumen");
    tbody.html("");

    var articulos = [];
    var id_arts = [];
    $(".NombresArticulosNuevoPrg").each(function () {
        var text = $(this).text().trim();
        var id_art = $(this).attr('id').split('_')[1];
        if ($.inArray(text, articulos) === -1) {
            articulos.push(text);
            id_arts.push(id_art);
        }
    });
    for (var i = 0; i < id_arts.length; i++) {
        var nombre_articulo = articulos[i];
        var id_articulo = id_arts[i];
        var total_lunes = 0;
        var total_martes = 0;
        var total_miercoles = 0;
        var total_jueves = 0;
        var total_viernes = 0;
        var total_sabado = 0;
        var total_domingo = 0;

        $(".LunVal_" + id_articulo + "").each(function () {
            total_lunes = total_lunes + parseFloat($(this).text());
        }); $(".MarVal_" + id_articulo + "").each(function () {
            total_martes = total_martes + parseFloat($(this).text());
        }); $(".MieVal_" + id_articulo + "").each(function () {
            total_miercoles = total_miercoles + parseFloat($(this).text());
        }); $(".JueVal_" + id_articulo + "").each(function () {
            total_jueves = total_jueves + parseFloat($(this).text());
        }); $(".VieVal_" + id_articulo + "").each(function () {
            total_viernes = total_viernes + parseFloat($(this).text());
        }); $(".SabVal_" + id_articulo + "").each(function () {
            total_sabado = total_sabado + parseFloat($(this).text());
        }); $(".DomVal_" + id_articulo + "").each(function () {
            total_domingo = total_domingo + parseFloat($(this).text());
        });

        var total = total_lunes + total_martes + total_miercoles + total_jueves + total_viernes + total_sabado + total_domingo;

        var row = $("<tr class='text-center'></tr>");

        row.append("<td>" + nombre_articulo + "</td>");

        if (total_lunes > 0) { row.append("<td >" + total_lunes + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_martes > 0) { row.append("<td>" + total_martes + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_miercoles > 0) { row.append("<td>" + total_miercoles + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_jueves) { row.append("<td>" + total_jueves + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_viernes) { row.append("<td>" + total_viernes + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_sabado) { row.append("<td>" + total_sabado + "</td>"); }
        else { row.append("<td >0</td>"); }
        if (total_domingo) { row.append("<td>" + total_domingo + "</td>"); }
        else { row.append("<td >0</td>"); }
        row.append("<td class='totales_semana'>" + parseFloat(total).toFixed(2) + "</td>");

        tbody.append(row);

    }

}



function GuardarNuevoProgramaAlimentos() {
    var fecha_inicio = $("#fecha_incio_gen_prg").val();
    var no_semanas = $("#no_semanas_gen_prg").val();
    var id_establo = $("#id_establo_gen_prg").val();
    var referencia_gen_prg = $("#referencia_gen_prg").val();

    var articulos = [];
    var toneladas = [];
    var fechas = [];
    var count = 0;
    $(".ValoresProgramacion").each(function () {
        var fecha = $(this).attr('id').split('_')[1];
        var id_articulo = $(this).attr('id').split('_')[2];
        var valor_tons = parseFloat($(this).text());

        fechas[count] = fecha;
        articulos[count] = id_articulo;
        toneladas[count] = valor_tons;
        count++;
    });

    if (articulos.length <= 0) {
        iziToast.warning({
            title: '',
            message: 'Ingrese al menos 1 alimento a la programación',
        });
    }
    else if (no_semanas <= 0) {
        iziToast.warning({
            title: 'Atención',
            message: 'El numero de semanas debe ser positivo',
        });
    }
    else if (no_semanas == "" || no_semanas == undefined || no_semanas > 5) {
        iziToast.warning({
            title: 'Atención',
            message: 'El numero de semanas debe ser máximo 5',
        });
    }
    else if (fecha_inicio == "" || fecha_inicio == undefined) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese la fecha de inicio',
        });
    }
    else if (referencia_gen_prg == "" || referencia_gen_prg == undefined) {
        iziToast.warning({
            title: 'Atención',
            message: 'Ingrese la referencia de la programación',
        });
    }
    else if (id_establo == undefined || id_establo == "") {
        iziToast.warning({
            title: 'Atención',
            message: 'Seleccione el establo de la programación',
        });
    }
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 9999,
            title: 'ADVERTENCIA',
            message: '¿Está seguro de guardar la programación capturada?',
            position: 'center',
            buttons: [
                ['<button><b>Si, guardar</b></button>', function (instance, toast) {
                    jsShowWindowLoad();
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../ALIMENTACION/GuardarNuevoProgramaAlimentos",
                        data: {
                            fecha_inicio: fecha_inicio,
                            no_semanas: no_semanas,
                            id_establo: id_establo,
                            referencia_gen_prg: referencia_gen_prg,
                            fechas: fechas,
                            articulos: articulos,
                            toneladas: toneladas
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Programación guardada correctamente',
                                    message: '',
                                });
                                $("#div_programacion_nueva").html("");
                                BloquearCamposPrg(2);
                                $("#no_semanas_gen_prg").val('');
                                $("#referencia_gen_prg").val('');
                            }
                            else if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar la programación',
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
                ['<button>No, regresar a la programación</button>', function (instance, toast) {
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


function ConsultarProgramasAlimentacionTable() {
    var id_establo = $("#id_establo_consulta_prg").val();
    var fecha_inicio = $("#fecha_inicio_consulta_prg").val();
    var fecha_fin = $("#fecha_fin_consulta_prg").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarProgramasAlimentacionTable",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_consulta_prg").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_consulta_prg").html(response);
            $('#datatable_consulta_prg').DataTable({
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

function ConsultarProgramaAlimentacionDetalle(id_programa_alimentacion_g) {
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarProgramaAlimentacionDetalle",
        data: { id_programa_alimentacion_g: id_programa_alimentacion_g },
        success: function (response) {
            $("#div_detalle_programacion").html(response);
            $("#m_detalle_programacion").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

