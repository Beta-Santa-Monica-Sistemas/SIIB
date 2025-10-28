function ConsultarDietasGeneralTable() {
    var id_status = $("#id_status_consultar_dietas").val();
    var id_establo = $("#id_establo_consultar_dietas").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarDietasGeneralTable",
        data: {
            id_status: id_status,
            id_establo: id_establo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatableDietasGeneral").dataTable().fnDestroy();
            } catch (e) { }
            $("#datatable_dietas_autorizadas").html(response);
            $('#datatableDietasGeneral').DataTable({
                keys: false,
                ordering: true,
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

function ConsultarPrecioAlimento(modo) {
    var id_alimento;
    if (modo == 1) { id_alimento = $("#new_nombre_alimento").val(); }
    if (modo == 2) { id_alimento = $("#edit_nombre_alimento").val(); }
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarPrecioAlimento",
        data: { id_alimento: id_alimento },
        success: function (response) {
            ClearNewDieta();
            if (modo == 1) {
                $("#new_precio").text(response);
                CalcularKgBH(modo);
            }
            if (modo == 2) {
                $("#edit_precio").text(response);
                CalcularKgBH(modo);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarUltimoMSAlimento(modo) {
    var id_alimento;
    if (modo == 1) { id_alimento = $("#new_nombre_alimento").val(); }
    if (modo == 2) { id_alimento = $("#edit_nombre_alimento").val(); }
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarUltimoMSAlimento",
        data: { id_alimento: id_alimento },
        success: function (response) {
            //ClearNewDieta();
            if (modo == 1) {
                $("#new_porc_ms").val(response);
                CalcularKgBH(modo);
            }
            if (modo == 2) {
                $("#edit_porc_ms").val(response);
                CalcularKgBH(modo);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ClearNewDieta() {
    $("#new_porc_ms").val("");
    $("#new_kg_ms").val("");
    $("#new_costo_bh").text("");
    $("#new_kg_bh").text("");

    $("#edit_porc_ms").val("");
    $("#edit_kg_ms").val("");
    $("#edit_costo_bh").text("");
    $("#edit_kg_bh").text("");
}

function ElliminarIngredienteNewDieta(id_alimento) {
    $("#trAlimento_" + id_alimento + "").remove();
    CalcularTotalesNewDieta();
}

function AgregarNewIngredienteDieta() {
    $(".input_valid_new").css("border-color", "");

    var id_alimento = $("#new_nombre_alimento").val();
    var nombre_alimento = $("#new_nombre_alimento option:selected").text();
    var porc_ms = $("#new_porc_ms").val();
    var kg_ms = parseFloat($("#new_kg_ms").val()).toFixed(3);
    var kg_bh = parseFloat($("#new_kg_bh").text().replace(/[^\d.]/g, ''));
    var precio = parseFloat($("#new_precio").text().replace(/[^\d.]/g, ''));
    var costo_bh = parseFloat($("#new_costo_bh").text().replace(/[^\d.]/g, ''));

    var msOriginal = parseFloat($("#msOriginal_" + id_alimento + "").text().replace(/[^\d.]/g, ''));
    if (msOriginal == "" || msOriginal == undefined || isNaN(msOriginal)) { msOriginal = 0; }

    var color = "";
    var diferencia = kg_ms - msOriginal;
    if (msOriginal != kg_ms && msOriginal != 0) {
        color = "#ffeb9c";
    }

    if (porc_ms > 100 || porc_ms <= 0) {
        iziToast.error({
            title: 'El porcentaje no puede ser mayor a 100 ni menor a 1',
            message: '',
        });
        $("#new_porc_ms").css("border-color", "red");
        return;
    }
    if (porc_ms == undefined || porc_ms == "" || kg_ms == undefined || kg_ms == "" || isNaN(kg_ms) || id_alimento == undefined || id_alimento == "" || kg_ms < 0) {
        iziToast.error({
            title: 'Ingresa valores validos',
            message: '',
        });
        $("#new_kg_ms").css("border-color", "red");
        return;
    }

    var formato_kg_sm = porc_ms.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_kg_ms = kg_ms.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_kg_bh = kg_bh.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_precio = precio.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_costo_bh = costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_diferencia = FormatoDecimalConSigno(diferencia);

    var ingrediente = [];
    ingrediente.push({
        idAlimento: id_alimento,
        nombreAlimento: nombre_alimento,
        porcMs: formato_kg_sm,
        kgMs: formato_kg_ms,
        kgBh: formato_kg_bh,
        precio: formato_precio,
        costoBh: formato_costo_bh,
        msOriginal: msOriginal.toFixed(3),
        diferencia: formato_diferencia
    });

    $("#trAlimento_" + id_alimento + "").remove();
    var tbody = $("#tbody_new_dieta");
    for (var i = 0; i < ingrediente.length; i++) {

        var row = $("<tr class='text-center alimentos_dieta' id='trAlimento_" + id_alimento + "'></tr>");
        for (var key in ingrediente[i]) {
            if ([key] == "idAlimento") { row.append("<td style='display:none;' id='idAlimento_" + id_alimento + "'>" + ingrediente[i][key] + "</td>"); }
            else if ([key] == "kgMs") { row.append("<td style='background-color: " + color + ";' id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'>" + ingrediente[i][key] + "</td>"); }
            else { row.append("<td id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'>" + ingrediente[i][key] + "</td>"); }

        }
        row.append("<td><button class='btn btn_beta_lineal_danger btn-sm' onclick='ElliminarIngredienteNewDieta(" + id_alimento + ");'><i class='fa fa-remove'></i></button></td>");
        tbody.append(row); 
    }
    CalcularTotalesNewDieta();
    ClearNewDieta();
}

function CopiarInforamcionDietaNewDieta() {
    var id_dieta_g = $("#id_dieta_new_copia").val();
    if (id_dieta_g == 0) {
        iziToast.warning({
            title: 'Selecciona una dieta valida',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/CopiarInforamcionDietaNewDieta",
        data: { id_dieta_g: id_dieta_g },
        success: function (response) {
            $("#tbody_new_dieta").html(response);
            CalcularTotalesNewDieta();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ShowHideCopiarInformacionDieta(modo) {
    if (modo == 1) { //COPIAR
        $("#btn_cancelar_copia_dieta").css("display", "block");
        $("#btn_aceptar_copia_dieta").css("display", "none");
        $("#id_dieta_new_copia").prop("disabled", true);
    }
    else {
        $("#btn_aceptar_copia_dieta").css("display", "block");
        $("#btn_cancelar_copia_dieta").css("display", "none");
        $("#id_dieta_new_copia").prop("disabled", false);
        $("#tbody_new_dieta").html("");
        CalcularTotalesNewDieta();
    }
}

function CalcularKgBH(modo) {
    var porc_ms;
    var kg_ms;
    var precio;
    if (modo == 1) {  //NUEVA DIETA
        porc_ms = $("#new_porc_ms").val().replace(/[^\d.]/g, '');
        kg_ms = $("#new_kg_ms").val().replace(/[^\d.]/g, '');
        precio = $("#new_precio").text().replace(/[^\d.]/g, '');
    }
    if (modo == 2) { //EDITAR
        porc_ms = $("#edit_porc_ms").val().replace(/[^\d.]/g, '');
        kg_ms = $("#edit_kg_ms").val().replace(/[^\d.]/g, '');
        precio = $("#edit_precio").text().replace(/[^\d.]/g, '');
    }

    if (porc_ms < 0  || kg_ms < 0) {
        iziToast.error({
            title: 'Ingresa valores validos',
            message: '',
        });
        return;
    }
    if (porc_ms == 0 /*|| kg_ms == 0*/) {
        return;
    }

    if (porc_ms == "" || porc_ms == undefined) { porc_ms = 0; }
    if (kg_ms == "" || kg_ms == undefined) { kg_ms = 0; }
    if (precio == "" || precio == undefined) { precio = 0; }

    porc_ms = parseFloat(porc_ms);
    kg_ms = parseFloat(kg_ms).toFixed(3);
    var kg_bh = parseFloat(kg_ms / (porc_ms / 100)).toFixed(3);
    var costo_bh = parseFloat(precio * kg_bh);

    var formato_kg_bh = kg_bh.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_costo_bh = costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    if (modo == 1) {
        $("#new_kg_bh").text(formato_kg_bh);
        $("#new_costo_bh").text(formato_costo_bh);
    }
    if (modo == 2) {
        $("#edit_kg_bh").text(formato_kg_bh);
        $("#edit_costo_bh").text(formato_costo_bh);
    }

}

function CalcularKgBHEdit(id_alimento) {
    $("#kgMsEdit_" + id_alimento + "").css("background-color", "");
    $(".input_valid_edit").css("border-color", "");

    var porc_ms = $("#porcMsEdit_" + id_alimento + "").val();
    var kg_ms = $("#kgMsEdit_" + id_alimento + "").val();
    var kg_ms_original = parseFloat($("#kgMsOriginalEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '')); 
    var kg_ms_base = parseFloat($("#msOriginalEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '')).toFixed(3);
    var precio = $("#precioEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '');
    if (porc_ms <= 0 || kg_ms < 0 || porc_ms == "" || kg_ms == "") {
        //iziToast.error({
        //    title: 'Ingresa valores validos',
        //    message: '',
        //});
        if (porc_ms == "" || porc_ms <= 0) { $("#porcMsEdit_" + id_alimento + "").css("border-color", "red"); }
        if (kg_ms == "" || kg_ms < 0) { $("#kgMsEdit_" + id_alimento + "").css("border-color", "red"); }

        $("#kgBhEdit_" + id_alimento + "").text("");
        $("#costoBhEdit_" + id_alimento + "").text("");
        CalcularTotalesEditDieta();
        return;
    }

    if (porc_ms == "" || porc_ms == undefined) { porc_ms = 0; }
    if (kg_ms == "" || kg_ms == undefined) { kg_ms = 0; }
    if (precio == "" || precio == undefined) { precio = 0; }

    porc_ms = parseFloat(porc_ms);
    kg_ms = parseFloat(kg_ms).toFixed(3);
    var kg_bh = kg_ms / (porc_ms / 100);
    var costo_bh = parseFloat(precio * kg_bh.toFixed(2));

    var diferencia = kg_ms_base - kg_ms;
    if (kg_ms_original != kg_ms) {
        $("#kgMsEdit_" + id_alimento + "").css("background-color", "#ffeb9c");
    }
    else {
        $("#kgMsEdit_" + id_alimento + "").css("background-color", "");
    }

    var formato_kg_bh = kg_bh.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_costo_bh = costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    $("#kgBhEdit_" + id_alimento + "").text(formato_kg_bh);
    $("#costoBhEdit_" + id_alimento + "").text(formato_costo_bh);
    if (diferencia != 0) { $("#diferenciaEdit_" + id_alimento + "").text(diferencia.toFixed(3)); }
    else { $("#diferenciaEdit_" + id_alimento + "").text(""); }
    CalcularTotalesEditDieta();
}

function CalcularTotalesNewDieta() {
    var total_kg_ms = 0;
    var total_bh = 0;
    var total_costo_bh = 0;

    $(".kgMs").each(function () {
        total_kg_ms += parseFloat($(this).text().replace(/[^\d.]/g, ''));
    });
    $(".kgBh").each(function () {
        total_bh += parseFloat($(this).text().replace(/[^\d.]/g, ''));
    });
    $(".costoBh").each(function () {
        total_costo_bh += parseFloat($(this).text().replace(/[^\d.]/g, ''));
    });

    var total_costo_kg_bh = total_costo_bh / total_kg_ms;
    if (isNaN(total_costo_kg_bh)) { total_costo_kg_bh = 0; }

    var formato_total_kg_ms = total_kg_ms.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_total_bh = total_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_total_costo_bh = total_costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    $("#total_kg_ms").text(formato_total_kg_ms);
    $("#total_kg_bh").text(formato_total_bh);
    $("#total_costo_bh").text(formato_total_costo_bh);

    var formato_total_costo_kg_bh = total_costo_kg_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    $("#total_costo_kg_bh").text(formato_total_costo_kg_bh);
}

function GuardarNewDieta() {
    var comentario = $("#comentario_new_dieta").val().trim();

    $(".input_clear").css("border-color", "");

    var id_establo = $("#id_establo_new_dieta").val();
    var id_grupo = $("#grupo_new_dieta").val();
    var fecha_inicio = $("#inicio_new_dieta").val();
    var fecha_fin = $("#fin_new_dieta").val();
    var nombre = $("#nombre_new_dieta").val();
    var desc = $("#desc_new_dieta").val();
    if (id_establo == undefined || id_grupo == undefined || fecha_inicio == "" || fecha_fin == "" || nombre == "" || desc == "") {
        iziToast.error({
            title: 'Ingresa todos los datos de la dieta',
            message: '',
        });
        $(".input_clear").each(function () {
            if ($(this).text() == "") { $(this).css("border-color", "red"); }
        });

        $('html, body').animate({
            scrollTop: $("#div_registro_dieta_header").offset().top
        }, 500); // 500ms animation speed
        return;
    }

    var id_articulos = [];
    var porcMs = [];
    var kgMs = [];
    var kgBh = [];
    var kgMs_base = [];
    var precio = [];
    var costoBh = [];
    var count = 0;

    var alimentos = $(".alimentos_dieta");
    alimentos.each(function () {
        var id_alimento = $(this).attr("id").split('_')[1];
        var _porcMs = $("#porcMs_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _kgMs = $("#kgMs_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _kgMs_base = $("#msOriginal_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _kgBh = $("#kgBh_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _precio = $("#precio_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _costoBh = $("#costoBh_" + id_alimento + "").text().replace(/[^\d.]/g, '');

        id_articulos[count] = id_alimento;
        porcMs[count] = _porcMs;
        kgMs[count] = _kgMs;
        kgMs_base[count] = _kgMs_base;
        kgBh[count] = _kgBh;
        precio[count] = _precio;
        costoBh[count] = _costoBh;
        count++;
    });
    var total_kg_ms = $("#total_kg_ms").text().replace(/[^\d.]/g, '');
    var total_kg_bh = $("#total_kg_bh").text().replace(/[^\d.]/g, '');
    var total_costo_bh = $("#total_costo_bh").text().replace(/[^\d.]/g, '');
    var total_costo_kg_bh = $("#total_costo_kg_bh").text().replace(/[^\d.]/g, '');

    if (id_articulos.length <= 0) {
        iziToast.info({
            title: 'Ingresa el detalle de la dieta',
            message: '',
        });
    }
    else if (isNaN(total_kg_ms) || isNaN(total_kg_bh) || isNaN(total_costo_bh) || isNaN(total_costo_kg_bh)) {
        iziToast.info({
            title: 'Error en los cálculos',
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
            title: 'ADVERTENCIA',
            message: '¿Está seguro de guardar la dieta?',
            position: 'center',
            buttons: [
                ['<button><b>Si, guardar dieta</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../ALIMENTACION/GuardarNewDieta",
                        data: {
                            id_establo: id_establo,
                            id_grupo: id_grupo,
                            fecha_inicio: fecha_inicio,
                            fecha_fin: fecha_fin,
                            nombre: nombre,
                            desc: desc,
                            id_dieta_copia: $("#id_dieta_new_copia").val(),
                            id_articulos: id_articulos,
                            porcMs: porcMs,
                            kgMs: kgMs,
                            kgMs_base: kgMs_base,
                            kgBh: kgBh,
                            precio: precio,
                            costoBh: costoBh,
                            total_kg_ms: total_kg_ms,
                            total_kg_bh: total_kg_bh,
                            total_costo_bh: total_costo_bh,
                            total_costo_kg_bh: total_costo_kg_bh,
                            comentario: comentario
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                ClearNewDieta();
                                $("#comentario_new_dieta").val("");
                                $(".input_clear").val("");
                                $("#tbody_new_dieta").html("");
                                iziToast.success({
                                    title: 'Dieta guardada correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(1);
                                ShowHideCopiarInformacionDieta(2);
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar la dieta',
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

function ConsultarDietaDetalle(id_dieta_g, id_status) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarDietaDetalle",
        data: {
            id_dieta_g: id_dieta_g,
            id_status: id_status
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_detalle_dieta").html(response);
            $("#m_detalle_dieta").modal("show");
            ConsultarPrecioAlimento(2);
            CalcularTotalesEditDieta();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#region EDITAR DIETA
function ElliminarIngredienteEditDieta(id_alimento) {
    $("#trAlimentoEdit_" + id_alimento + "").remove();
    CalcularTotalesEditDieta();
}

function AgregarNewIngredienteDietaEdit() {
    var id_alimento = $("#edit_nombre_alimento").val();
    var nombre_alimento = $("#edit_nombre_alimento option:selected").text();
    var porc_ms = $("#edit_porc_ms").val();
    var kg_ms = parseFloat($("#edit_kg_ms").val()).toFixed(3);
    var kg_bh = parseFloat($("#edit_kg_bh").text().replace(/[^\d.]/g, ''));
    var precio = parseFloat($("#edit_precio").text().replace(/[^\d.]/g, ''));
    var costo_bh = parseFloat($("#edit_costo_bh").text().replace(/[^\d.]/g, ''));

    var msOriginal = parseFloat($("#msOriginalEdit_" + id_alimento + "").text().replace(/[^\d.]/g, ''));
    if (msOriginal == "" || msOriginal == undefined || isNaN(msOriginal)) { msOriginal = 0; }
    var diferencia = kg_ms - msOriginal;

    if (porc_ms == undefined || porc_ms == "" || kg_ms == undefined || kg_ms == "" || isNaN(kg_ms) || id_alimento == undefined || id_alimento == "" || porc_ms <= 0 || kg_ms < 0) {
        iziToast.error({
            title: 'Ingresa valores validos',
            message: '',
        });
        return;
    }

    var formato_kg_sm = porc_ms.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_kg_ms = kg_ms.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_kg_bh = kg_bh.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_precio = precio.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_costo_bh = costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_ms_original = msOriginal.toLocaleString('en-US', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
    var formato_diferencia = FormatoDecimalConSigno(diferencia);

    var ingrediente = [];
    ingrediente.push({
        idAlimentoEdit: id_alimento,
        nombreAlimentoEdit: nombre_alimento,
        porcMsEdit: formato_kg_sm,
        kgMsEdit: formato_kg_ms,
        kgBhEdit: formato_kg_bh,
        precioEdit: formato_precio,
        costoBhEdit: formato_costo_bh,
        msOriginalEdit: formato_ms_original,
        diferenciaEdit: formato_diferencia
    });

    $("#trAlimentoEdit_" + id_alimento + "").remove();
    var tbody = $("#tbody_edit_dieta");
    for (var i = 0; i < ingrediente.length; i++) {

        var row = $("<tr class='text-center alimentos_dieta_edit' id='trAlimentoEdit_" + id_alimento + "'></tr>");
        for (var key in ingrediente[i]) {
            if ([key] == "idAlimentoEdit") { row.append("<td style='display:none;' id='idAlimentoEdit_" + id_alimento + "'>" + ingrediente[i][key] + "</td>"); }
            else if ([key] == "porcMsEdit" || [key] == "kgMsEdit") {
                row.append("<td ><input type='number' onkeyup='CalcularKgBHEdit(" + id_alimento + ");' onchange='CalcularKgBHEdit(" + id_alimento + ");' class='form-control input_valid_edit " + [key] + "' id='" + [key] + "_" + id_alimento + "' value='" + ingrediente[i][key] + "' /></td>");
            }
            else if ([key] == "diferenciaEdit") {
                if (ingrediente[i][key] == "" || ingrediente[i][key] == "+0.000") { row.append("<td id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'></td>");  }
                else { row.append("<td id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'>" + ingrediente[i][key] + "</td>"); }
            }
            else if ([key] == "nombreAlimentoEdit") {
                row.append("<td class='text-left' id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'>" + ingrediente[i][key] + "</td>"); 
            }
            else { row.append("<td id='" + [key] + "_" + id_alimento + "' class='" + [key] + "'>" + ingrediente[i][key] + "</td>"); }
        }
        row.append("<td><button class='btn btn_beta_lineal_danger btn-sm' onclick='ElliminarIngredienteEditDieta(" + id_alimento + ");'><i class='fa fa-remove'></i></button></td>");
        tbody.prepend(row);
    }
    CalcularTotalesEditDieta();
    ClearNewDieta();
}

function CalcularTotalesEditDieta() {
    var total_kg_ms = 0;
    var total_bh = 0;
    var total_costo_bh = 0;

    $(".kgMsEdit").each(function () {
        var valor_kg_ms = $(this).val();
        if (valor_kg_ms == "") { valor_kg_ms = 0; }
        total_kg_ms += parseFloat(valor_kg_ms);
    });
    $(".kgBhEdit").each(function () {
        var valor_kgBh = parseFloat($(this).text().replace(/[^\d.]/g, ''));
        if (isNaN(valor_kgBh)) { valor_kgBh = 0; }
        total_bh += valor_kgBh;
    });
    $(".costoBhEdit").each(function () {
        var valor_costoBh = parseFloat($(this).text().replace(/[^\d.]/g, ''));
        if (isNaN(valor_costoBh)) { valor_costoBh = 0; }
        total_costo_bh += valor_costoBh;
    });

    var total_costo_kg_bh = total_costo_bh / total_kg_ms;
    if (isNaN(total_costo_kg_bh)) { total_costo_kg_bh = 0; }

    var formato_total_kg_ms = total_kg_ms.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_total_bh = total_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    var formato_total_costo_bh = total_costo_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    $("#total_kg_msEdit").text(formato_total_kg_ms);
    $("#total_kg_bhEdit").text(formato_total_bh);
    $("#total_costo_bhEdit").text(formato_total_costo_bh);

    var formato_total_costo_kg_bh = total_costo_kg_bh.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    $("#total_costo_kg_bhEdit").text(formato_total_costo_kg_bh);
}


function ActualizarDieta(id_dieta_g, id_status) {
    var id_articulos = [];
    var porcMs = [];
    var kgMs = [];
    var kgMs_base = [];
    var kgBh = [];
    var precio = [];
    var costoBh = [];
    var count = 0;

    $(".input_valid_edit").css("border-color","");
    var valid_inputs = false;
    var alimentos = $(".alimentos_dieta_edit");
    alimentos.each(function () {
        var id_alimento = $(this).attr("id").split('_')[1];
        var _porcMs = $("#porcMsEdit_" + id_alimento + "").val();
        var _kgMs = $("#kgMsEdit_" + id_alimento + "").val();
        var _kgMs_base = $("#msOriginalEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _kgBh = $("#kgBhEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _precio = $("#precioEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '');
        var _costoBh = $("#costoBhEdit_" + id_alimento + "").text().replace(/[^\d.]/g, '');

        if (_porcMs == undefined || _porcMs == "" || _porcMs <= 0) {
            valid_inputs = true;
            $("#porcMsEdit_" + id_alimento + "").css("border-color", "red");
        }
        //if (_kgMs == undefined || _kgMs == "" || _kgMs <= 0) {
        //    valid_inputs = true;
        //    $("#kgMsEdit_" + id_alimento + "").css("border-color", "red");
        //}

        id_articulos[count] = id_alimento;
        porcMs[count] = _porcMs;
        kgMs[count] = _kgMs;
        kgMs_base[count] = _kgMs_base;
        kgBh[count] = _kgBh;
        precio[count] = _precio;
        costoBh[count] = _costoBh;
        count++;
    });
    var total_kg_ms = $("#total_kg_msEdit").text().replace(/[^\d.]/g, '');
    var total_kg_bh = $("#total_kg_bhEdit").text().replace(/[^\d.]/g, '');
    var total_costo_bh = $("#total_costo_bhEdit").text().replace(/[^\d.]/g, '');
    var total_costo_kg_bh = $("#total_costo_kg_bhEdit").text().replace(/[^\d.]/g, '');

    if (id_articulos.length <= 0) {
        iziToast.info({
            title: 'Ingresa el detalle de la dieta',
            message: '',
        });
    }
    else if (isNaN(total_kg_ms) || isNaN(total_kg_bh) || isNaN(total_costo_bh) || isNaN(total_costo_kg_bh)) {
        iziToast.info({
            title: 'Error en los cálculos',
            message: '',
        });
    }
    else if (valid_inputs == true) {
        iziToast.info({
            title: 'Ingrese valores validos',
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
            zindex: 999999,
            title: 'ADVERTENCIA',
            message: '¿Está seguro de guardar la dieta?',
            position: 'center',
            buttons: [
                ['<button><b>Si, guardar dieta</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../ALIMENTACION/ActualizarDieta",
                        data: {
                            id_dieta_g: id_dieta_g,
                            id_articulos: id_articulos,
                            porcMs: porcMs,
                            kgMs: kgMs,
                            kgMs_base: kgMs_base,
                            kgBh: kgBh,
                            precio: precio,
                            costoBh: costoBh,
                            total_kg_ms: total_kg_ms,
                            total_kg_bh: total_kg_bh,
                            total_costo_bh: total_costo_bh,
                            total_costo_kg_bh: total_costo_kg_bh
                        },
                        success: function (response) {
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Dieta actualizada correctamente',
                                    message: '',
                                });
                                ConsultarDietaDetalle(id_dieta_g, id_status);
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un error al actualizar la receta',
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

function AgregarComentarioDieta(id_dieta_g, id_status) {
    var comentario = $("#comentario_edit_dieta").val().trim();
    if (comentario.length <= 10) {
        iziToast.info({
            title: 'Ingresa un comentario de al menos 10 caracteres',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../ALIMENTACION/AgregarComentarioDieta",
            data: {
                id_dieta_g: id_dieta_g,
                comentario: comentario
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Comentario guardado correctamente',
                        message: '',
                    });
                    ConsultarDietaDetalle(id_dieta_g, id_status);
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al guardar el comentario',
                        message: 'Intente nuevamente',
                    });
                    ValidaSesion();
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}

function ActualizarStatusDieta(id_dieta_g, id_status) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de realizar esta operación?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    timeout: 900000,
                    url: "../ALIMENTACION/ActualizarStatusDieta",
                    data: {
                        id_dieta_g: id_dieta_g,
                        id_status: id_status
                    },
                    success: function (response) {
                        if (response == 0) {
                            if (id_status == 1) { //RECHAZADA Y ENVIADA A EDICION
                                iziToast.success({
                                    title: 'Dieta rechazada correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(1);
                                ConsultarDietasEstatusTable(2);
                            }
                            if (id_status == 2) { //ENVIADA A AUTORIZACION
                                iziToast.success({
                                    title: 'Dieta enviada a autorización correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(6);
                                ConsultarDietasEstatusTable(2);
                            }
                            if (id_status == 3) { //AUTORIZADA
                                iziToast.success({
                                    title: 'Dieta autorizada correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(2);
                                ConsultarDietasEstatusTable(3);
                            }
                            if (id_status == 4) { //APLICADA
                                iziToast.success({
                                    title: 'Dieta aplicada correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(3);
                                ConsultarDietasGeneralTable();
                                ConsultarDietasAutorizadasSelect(4);
                            }

                            if (id_status == 6) { //REVISION
                                iziToast.success({
                                    title: 'Dieta enviada a revisión correctamente',
                                    message: '',
                                });
                                ConsultarDietasEstatusTable(1);
                                ConsultarDietasEstatusTable(6);
                            }
                            $("#m_detalle_dieta").modal("hide");
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al guardar el comentario',
                                message: 'Intente nuevamente',
                            });
                            ValidaSesion();
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

function ConsultarDietasEstatusTable(id_status) {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ConsultarDietasEstatusTable",
        data: { id_status: id_status },
        success: function (response) {
            if (id_status == 1) {
                $("#datatable_dietas_edicion").html(response);
            }
            else if (id_status == 2) {
                $("#datatable_dietas_autorizacion").html(response);
            }
            else if (id_status == 3) {
                $("#datatable_dietas_aplicar").html(response);
            }
            else if (id_status == 4) {
                $("#datatable_dietas_autorizadas").html(response);
            }
            else if (id_status == 6) {
                $("#datatable_dietas_revision").html(response);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarDietasAutorizadasSelect(id_status) {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../CATALOGOS/ConsultarDietasAutorizadasSelect",
        data: { id_status: id_status },
        success: function (response) {
            if (id_status == 4) {
                $("#id_dieta_new_copia").html(response);
                var todos_option = $('<option>', { value: '0', text: 'Ninguna' });
                $("#id_dieta_new_copia").prepend(todos_option);
                $("#id_dieta_new_copia").val(0);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    
}

function FormatoDecimalConSigno(numero) {
    if (numero > 0) {
        return `+${numero.toFixed(3)}`;
    } else if (numero < 0) {
        return numero.toFixed(3); // ya incluye el "-"
    } else {
        return "+0.000"; // o "0.00" si prefieres sin signo
    }
}


function EditarInfoGeneralDieta(id_dieta_g, desc, nombre, fecha_inicio, fecha_fin, id_status) {
    let partes = fecha_inicio.split("/");
    let fechaFormateada = `${partes[2]}-${partes[1]}-${partes[0]}`;

    let partes2 = fecha_fin.split("/");
    let fechaFormateada2 = `${partes2[2]}-${partes2[1]}-${partes2[0]}`;

    $("#nombre_edit_dieta").val(nombre);
    $("#desc_edit_dieta").val(desc);
    $("#inicio_edit_dieta").val(fechaFormateada);
    $("#fin_edit_dieta").val(fechaFormateada2);
    $("#btn_actualizar_dieta_general").attr("onclick", "ActualizarInformacionDietaGeneral(" + id_dieta_g + ", " + id_status + ");");
    $("#m_editar_info_dieta_general").modal("show");
}

function ActualizarInformacionDietaGeneral(id_dieta_g, id_status) {
    var nombre = $("#nombre_edit_dieta").val();
    var descripcion = $("#desc_edit_dieta").val();
    var fecha_inicio = $("#inicio_edit_dieta").val();
    var fecha_fin = $("#fin_edit_dieta").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALIMENTACION/ActualizarInformacionDietaGeneral",
        data: {
            id_dieta_g: id_dieta_g,
            nombre: nombre,
            descripcion: descripcion,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            $("#m_editar_info_dieta_general").modal("hide");
            if (response == "True") {
                iziToast.success({
                    title: 'Dieta actualizada correctamente',
                    message: '',
                });
                ConsultarDietaDetalle(id_dieta_g, id_status);
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al actualizar la información de la fieta',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


