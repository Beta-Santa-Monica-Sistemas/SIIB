function ConsultarSalidaGanadoEditar() {
    var id_establo = $("#id_establo_utileria").val();
    var folio = $("#folio_utileria").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        url: "../UTILERIAS/ConsultarSalidaGanadoEditar",
        data: {
            folio: folio,
            id_establo: id_establo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_utileria_salida_ganado").html(response);
            CalcularTotales();
        },
        error: function (xhr, status, error) {

        }
    });
}

function EliminarDetalleSalida(no_detalle) {
    $("#tbody_row_" + no_detalle + "").remove();
}

function AgregarVacaTablaUtileria(last_index) {
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

        var detalle = [];
        detalle.push({
            arete: $("#arete").val(),
            clasif: $("#ganado_g").find('option:selected').text(),
            causa: $("#causa").val(),
            condicion: condicion,
            sala: $("#sala").val(),
            edad: $("#edad").val(),
            cantidad: cantidad,
            peso_neto: peso_neto,
            last_index: last_index
        });
        for (var i = 0; i < detalle.length; i++) {
            var row = $("<tr class='text-center' id='tbody_row_" + last_index + "'></tr>");
            for (var key in detalle[i]) {
                if ([key] == "last_index") { row.append("<td><button class='btn btn_beta_danger' onclick='EliminarDetalleSalida(" + detalle[i][key] + ");'><i class='fa fa-remove'></i></button></td>"); }
                else { row.append("<td><input type='text' value='" + detalle[i][key] + "'  class='form-control " + [key] + "' id='" + [key] + "_" + last_index + "'/></td>"); }
            }
            $("#thead_ganado").append(row);
        }
        $(".input_detalle").val('');
        $(".input_valid").each(function () {
            $(this).css("border", "");
        });
        var next_index = last_index + 1;
        $("#btn_agregar_vaca").attr("onclick", "AgregarVacaTablaUtileria(" + next_index + ")");
        CalcularTotales();
    }

}

function CalcularTotales() {
    var sum_cantidades = 0;
    var sum_kilos = 0;
    $(".cantidad").each(function () {
        sum_cantidades += parseFloat($(this).val());
    });
    $(".peso_neto").each(function () {
        sum_kilos += parseFloat($(this).val());
    });

    $("#cantidad_total").val(sum_cantidades);
    $("#kilos_total").val(sum_kilos.toFixed(2));
}

function ActualizarSalidaGanado(id_salida_g, folio) {
    var tipo_salida = $("input[name='tipo_salida']:checked").val();
    var peso_t = $("#peso_t_g").val();
    var peso_total = $("#kilos_total").val();

    var count = 0;
    var id_salida_d = [];
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

    var arete = $(".arete");
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
        arete.each(function () {
            var id = $(this).attr("id").split('_')[1];
            id_salida_d[count] = id;
            aretes[count] = $(this).val();
            clasificaciones[count] = $("#clasif_" + id + "").val();
            causas[count] = $("#causa_" + id + "").val();
            condiciones[count] = $("#condicion_" + id + "").val();
            salas[count] = $("#sala_" + id + "").val();
            edades[count] = $("#edad_" + id + "").val();;
            cantidades[count] = $("#cantidad_" + id + "").val();
            pesos_neto[count] = $("#peso_neto_" + id + "").val();
            p_unitarios[count] = 0;
            importes[count] = $("#importe_" + id + "").val();
            count++;
        });

        var salida = {};
        salida.id_salida_gan_g = id_salida_g;
        salida.folio = folio;
        salida.ficha = $("#ficha_g").text();
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


        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'questionSoporte',
            zindex: 999,
            title: 'Justificacion',
            drag: false,
            message: `
                <div style="margin-top:10px; width: 100%;">
                    <input type="text" id="justificacionInput"
                           style="width: 100%; padding: 10px; font-size: 16px; box-sizing: border-box;"
                           placeholder="Escribe tu justificación del soporte...">
                </div>
                <div style="margin-top: 20px; display: flex; justify-content: center; gap: 20px;">                                 
                    <button id="cancelBtn" style="padding: 10px 20px;" class = "btn btn_beta_danger">Cancelar</button>
                    <button id="confirmBtn" style="padding: 10px 20px;" class = "btn btn_beta"><b>Confirmar</b></button>
                </div>
            `,
            position: 'center',
            buttons: [],
            onOpening: function (instance, toast) {
                toast.style.width = '60%';
                $(".iziToast-body").removeAttr("style");
                $(".iziToast-capsule").css("width", "100%");
                $(".iziToast-icon").css("display", "none");
                $("#questionSoporte").css("width", "80%");
                $(".iziToast-texts").css("width", "100%");
                $(".iziToast-message").css("width", "100%");

                setTimeout(() => {
                    document.getElementById('confirmBtn').addEventListener('click', function () {


                        var justificacion_soporte = $("#justificacionInput").val();
                        if (justificacion_soporte.length < 10) {
                            iziToast.warning({
                                title: 'Aviso',
                                message: 'El tamaño minimo de la justificacion es de 10 caracteres',
                            });
                            return;
                        }
                        iziToast.hide({}, toast);
                        jsShowWindowLoad();

                        $.ajax({
                            url: "../UTILERIAS/ActualizarSalidaGanado",
                            async: false,
                            timeout: 900000,
                            type: "POST",
                            data: {
                                salida: salida,
                                id_salida_d: id_salida_d,
                                aretes: aretes,
                                clasificaciones: clasificaciones,
                                causas: causas,
                                condiciones: condiciones,
                                salas: salas,
                                edades: edades,
                                cantidades: cantidades,
                                pesos_neto: pesos_neto,
                                p_unitarios: p_unitarios,
                                importes: importes,
                                justificacion: justificacion_soporte
                            },
                            success: function (result) {
                                jsRemoveWindowLoad();
                                if (result != "") {
                                    iziToast.success({
                                        title: 'Exito!',
                                        message: 'Salida de ganado actualizada correctamente',
                                    });
                                    LimpiarCaptura();
                                    //$("#folio_utileria").val('');
                                    $(".input_valid").each(function () {
                                        $(this).css("border", "");
                                    });
                                    //$("#div_utileria_salida_ganado").html("");
                                    ConsultarSalidaGanadoEditar();
                                }
                                else {
                                    iziToast.error({
                                        title: 'Ocurrió un problema al guardar el detalle de la salida',
                                        message: '',
                                    });
                                }
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                                jsRemoveWindowLoad();
                                iziToast.error({
                                    title: 'Ocurrió un problema al guardar el detalle de la salida',
                                    message: '',
                                });
                            }
                        });


                    });

                    document.getElementById('cancelBtn').addEventListener('click', function () {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'Soporte no realizado',
                        });
                        iziToast.hide({}, toast);
                    });
                }, 100);
            },
            onClosing: function (instance, toast, closedBy) { },
            onClosed: function (instance, toast, closedBy) { }
        });

    }
}

function EliminarSalidaGanado(id_salida_ganado) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de eliminar la salida de ganado?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                iziToast.question({
                    timeout: 20000,
                    close: false,
                    overlay: true,
                    displayMode: 'once',
                    id: 'questionSoporte',
                    zindex: 999,
                    title: 'Justificacion',
                    drag: false,
                    message: `
                <div style="margin-top:10px; width: 100%;">
                    <input type="text" id="justificacionInput"
                           style="width: 100%; padding: 10px; font-size: 16px; box-sizing: border-box;"
                           placeholder="Escribe tu justificación del soporte...">
                </div>
                <div style="margin-top: 20px; display: flex; justify-content: center; gap: 20px;">                                 
                    <button id="cancelBtn" style="padding: 10px 20px;" class = "btn btn_beta_danger">Cancelar</button>
                    <button id="confirmBtn" style="padding: 10px 20px;" class = "btn btn_beta"><b>Confirmar</b></button>
                </div>
            `,
                    position: 'center',
                    buttons: [],
                    onOpening: function (instance, toast) {
                        toast.style.width = '60%';
                        $(".iziToast-body").removeAttr("style");
                        $(".iziToast-capsule").css("width", "100%");
                        $(".iziToast-icon").css("display", "none");
                        $("#questionSoporte").css("width", "80%");
                        $(".iziToast-texts").css("width", "100%");
                        $(".iziToast-message").css("width", "100%");

                        setTimeout(() => {
                            document.getElementById('confirmBtn').addEventListener('click', function () {


                                var justificacion_soporte = $("#justificacionInput").val();
                                if (justificacion_soporte.length < 10) {
                                    iziToast.warning({
                                        title: 'Aviso',
                                        message: 'El tamaño minimo de la justificacion es de 10 caracteres',
                                    });
                                    return;
                                }
                                iziToast.hide({}, toast);
                                jsShowWindowLoad();

                                $.ajax({
                                    type: "POST",
                                    url: "../UTILERIAS/EliminarSalidaGanado",
                                    data: {
                                        id_salida_ganado: id_salida_ganado,
                                        justificacion: justificacion_soporte
                                    },
                                    success: function (response) {
                                        if (response == 0) {
                                            jsRemoveWindowLoad();
                                            iziToast.success({
                                                title: 'Salida de ganado eliminada correctamente',
                                                message: '',
                                            });
                                            $("#div_utileria_salida_ganado").html("");
                                            $("#folio_utileria").val('');
                                        }
                                        else {
                                            iziToast.error({
                                                title: 'Ocurrió un problema al eliminar la salida de ganado',
                                                message: '',
                                            });
                                        }
                                    },
                                    error: function (xhr, status, error) {
                                        jsRemoveWindowLoad();
                                        console.error(error);
                                        iziToast.error({
                                            title: 'Ocurrió un problema al eliminar la salida de ganado',
                                            message: '',
                                        });
                                    }
                                });


                            });

                            document.getElementById('cancelBtn').addEventListener('click', function () {
                                iziToast.warning({
                                    title: 'Aviso',
                                    message: 'Soporte no realizado',
                                });
                                iziToast.hide({}, toast);
                            });
                        }, 100);
                    },
                    onClosing: function (instance, toast, closedBy) { },
                    onClosed: function (instance, toast, closedBy) { }
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

