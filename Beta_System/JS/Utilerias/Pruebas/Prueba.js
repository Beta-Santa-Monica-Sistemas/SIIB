$('input[type="date"]').val(today);
var count_tabla = 0;


function ConsultarBitacoraCarga(id_establo, fecha_inicio, fecha_fin, id_tipo_salida) {
    var id_establo = $("#id_establo_bitacora").val();
    var fecha_inicio = $("#fecha_inicio_bitacora").val();
    var fecha_fin = $("#fecha_fin_bitacora").val();
    var id_tipo_salida = $("#id_tipo_salida_bitacora").val();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../PRUEBAS/ConsultarBitacoraCarga",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_tipo_salida: id_tipo_salida
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_datatable_bitacora").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ActualizarSalidaGanado(id_salida_g, folio) {

    var count = 0;
    var id_salida_d = [];
    var aretes = [];
    var clasificaciones = [];
    var causas = [];
    var condiciones = [];
    var salas = [];
    var edades = [];

    if ($("#thead_ganado_edit tr").length === 0) {
        iziToast.error({
            title: 'Es necesario al menos un registro para confirmar la edicion',
            message: '',
        });
        return;
    }

    var validacion = ValidacionAreteActualizacionGanado(id_salida_g, folio);

    if (validacion == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Hay aretes que ya se utilizaron',
        });
        return;
    }
    else if (validacion == -1) {
        return;
    }

    jsShowWindowLoad();

    var arete = $(".arete_edit");
    if (arete.length == 0) {
        iziToast.error({
            title: 'Ingrese el detalle de la salida',
            message: '',
        });
    }
    arete.each(function () {
        var id = $(this).attr("id").split('_')[1];
        id_salida_d[count] = id;
        aretes[count] = $("#IdDetalleSalida_" + id + "").val();
        clasificaciones[count] = $("#clasif_" + id + "").val();
        causas[count] = $("#causa_" + id + "").val();
        condiciones[count] = $("#condicion_" + id + "").val();
        salas[count] = $("#sala_" + id + "").val();
        edades[count] = $("#edad_" + id + "").val();
        count++;
    });

    $.ajax({
        url: "../PRUEBAS/ActualizarSalidaGanado",
        async: false,
        timeout: 900000,
        type: "POST",
        data: {
            id_salida_g: id_salida_g,
            id_salida_d: id_salida_d,
            aretes: aretes,
            clasificaciones: clasificaciones,
            causas: causas,
            condiciones: condiciones,
            salas: salas,
            edades: edades
        },
        success: function (result) {
            jsRemoveWindowLoad();
            if (result == 1) {
                iziToast.success({
                    title: 'Exito!',
                    message: 'Salida de ganado actualizada correctamente',
                });
                $("#div_datatable_edicion").html("");
                $("#folio_edicion").val('');
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

function ValidacionAreteActualizacionGanado(id_salida_g, folio) {
    var count = 0;
    var aretes = [];

    var arete = $(".arete_edit");
    if (arete.length == 0) {
        iziToast.error({
            title: 'Ingrese el detalle de la salida',
            message: '',
        });
    }
    arete.each(function () {
        var id = $(this).attr("id").split('_')[1];
        aretes[count] = $("#IdDetalleSalida_" + id + "").val();
        count++;
    });

    // Validar duplicados
    if (new Set(aretes).size !== aretes.length) {
        iziToast.warning({
            title: 'Aviso',
            message: 'No se puede repetir el arete',
        });
        return -1;
    }

    var resultado = 0;

    $.ajax({
        url: "../PRUEBAS/ValidacionAreteActualizacionGanado",
        async: false, // sincrónico
        type: "POST",
        data: {
            id_salida_g: id_salida_g,
            folio: folio,
            aretes: aretes
        },
        success: function (res) {
            resultado = res;
        },
        error: function () {
            resultado = 0;
        }
    });

    return resultado
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
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    url: "../UTILERIAS/EliminarSalidaGanado",
                    data: { id_salida_ganado: id_salida_ganado },
                    success: function (response) {
                        if (response == 0) {
                            jsRemoveWindowLoad();
                            iziToast.success({
                                title: 'Salida de ganado eliminada correctamente',
                                message: '',
                            });
                            $("#div_datatable_edicion").html("");
                            $("#folio_edicion").val('');
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un problema al eliminar la salida de ganado',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {

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

function EliminarDetalleSalidaGanado(no_detalle) {
    $("#tbody_row_ganado_" + no_detalle + "").remove();
}

function ConsultarSalidaGanadoModificar() {
    var id_establo = $("#id_establo_edicion").val();
    var folio = $("#folio_edicion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        url: "../PRUEBAS/ConsultarSalidaGanadoModificar",
        data: {
            folio: folio,
            id_establo: id_establo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_datatable_edicion").html(response);
            CalcularTotales();
        },
        error: function (xhr, status, error) {

        }
    });
}


function SelectSalaEstablo() {
    var id_establo = $("#id_establo_carga").val();
    $.ajax({
        url: "../PRUEBAS/SelectSalaEstablo",
        async: false,
        type: "POST",
        data: { id_establo: id_establo },
        success: function (result) {
            $("#sala").html(result);
        }
    });
}

function TipoGanado() {
    var tipo_ganado = $("#tipo_salida").val();

    if (tipo_ganado == 1) {
        $("#tipo_salida").val();
        $("#tipo_salida").val();
    }
    else {
        $("#tipo_salida").val();
        $("#tipo_salida").val();
    }

}
function ConsultarInformacionArete() {
    var arete = $("#arete").val();
    var siniiga = $("#siniiga").val();
    jsShowWindowLoad();
    $.ajax({
        url: "/PRUEBAS/ConsultarInformacionArete",
        type: "POST",
        data: {
            arete: arete,
            siniiga: siniiga
        },
        success: function (result) {
            if (result.Estado === "-1") {
                $("#arete_estado").val("NA");
                $("#arete_ginecologico").val("NA");
                $("#edad").val("NA");
            } else {
                $("#arete_estado").val(result.Estado);
                $("#arete_ginecologico").val(result.EstadoGinecologico);
                $("#edad").val(result.Edad);

                if (arete == "" || arete.trim() == "") {
                    $("#arete").val(result.Vaca);
                }
                if (siniiga == "" || siniiga.trim() == "") {
                    $("#siniiga").val(result.Siniiga);
                }
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            jsRemoveWindowLoad();
            console.error("Error al consultar el arete:", error);
            iziToast.error({
                title: 'Aviso',
                message: 'Ocurrió un error al consultar la información',
            });
        }
    });
    jsRemoveWindowLoad();
}

function CambioAreteInput() {
    var arete = $("#arete");
    var boton = $("#btn_cambio_arete_input");

    if (arete.prop("readonly")) {
        arete.prop("readonly", false);
        boton.text("Deshabilitar arete");
        $("#arete").val('');
    } else {
        arete.prop("readonly", true);
        boton.text("Habilitar arete");
        $("#arete").val('');
    }
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

//#region CARGA DE GANADO (MEDICOS)
function CargarDGanadoTabla() {
    var condicion = $("#condicion_g").find('option:selected').text();
    var imagen_base64 = $("#imagen_base64_1").val();
    var imagen_base64_2 = $("#imagen_base64_2").val();
    var id_causa_muerte = $("#id_causa_muerte").val();
    var tipo_salida = $("#id_tipo_causa_muerte").find('option:selected').text();
    var causa_muerte = $("#id_causa_muerte").find('option:selected').text();

    var arete = $("#arete").val();
    var clasif = $("#ganado_g").find('option:selected').text();
    var causa = $("#causa").val();
    var sala = $("#sala").val();
    var edad = $("#edad").val();

    var medico = $("#id_medico_select").val();
    var estado = $("#arete_estado").val();
    var ginecologico = $("#arete_ginecologico").val();
    var arete_manual = false;
    var siniiga = $("#siniiga").val();


    const claveFoto1 = imagen_base64;   // <-- NUEVO
    const claveFoto2 = imagen_base64_2; // <-- NUEVO

    if (edad == "" || edad.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de consultar arete',
        });
        return;
    }


    if (!$("#arete").prop("readonly")) {
        arete_manual = true;
    }


    if (medico == "" || medico.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio seleccionar el medico.',
        });
        return;
    }
    if ((arete == "" || arete.trim() == "") && (siniiga == "" || siniiga.trim() == "")) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio ingresar el ARETE o el SINIIGA.',
        });
        return;
    }
    if (causa_muerte == "" || causa_muerte.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio seleccionar la causa de muerte.',
        });
        return;
    }
    if (imagen_base64 == '' || $("#foto_1").attr("src") == '') {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio tomar la fotografia.',
        });
        return;
    }
    if (imagen_base64_2 == '' || $("#foto_2").attr("src") == '') {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio tomar la fotografia.',
        });
        return;
    }

    if (arete == "NA" && siniiga == "NA") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un arete o siiniga valido',
        });
        return;
    }

    if (ValidarAreteExistente() == true) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Ya se esta utilizando el arete',
        });
        return;
    }
    else {
        edad = $("#edad").val();
        estado = $("#arete_estado").val();
        ginecologico = $("#arete_ginecologico").val();
        siniiga = $("#siniiga").val();

    }

    jsShowWindowLoad();
    $.ajax({
        url: "../PRUEBAS/CargarDGanadoTabla",
        async: false,
        type: "POST",
        data: {
            count_tabla: count_tabla,
            arete: arete,
            siniiga: siniiga,
            clasif: clasif,
            tipo_salida: tipo_salida,
            causa: causa,
            sala: sala,
            edad: edad,
            condicion: condicion,
            imagen_base64: imagen_base64,
            imagen_base64_2: imagen_base64_2,
            id_causa_muerte: id_causa_muerte,
            causa_muerte: causa_muerte,
            medico: medico,
            estado: estado,
            ginecologico: ginecologico,
            arete_manual: arete_manual
        },
        success: function (result) {
            jsRemoveWindowLoad();
            $(".input_detalle").val('');
            $("#thead_ganado_medicos").append(result);


            // 📌 Recuperar índice de la fila recién agregada
            var idFila = count_tabla;
            // Forzar que la fila tenga sus claves
            $("#imagen_base64_" + idFila).val(claveFoto1);
            $("#imagen_base64_2_" + idFila).val(claveFoto2);

            // Pintar usando esas claves
            $("#img_vaca_" + idFila).attr("src", fotosTemporal[claveFoto1] || "");
            $("#img_vaca_2_" + idFila).attr("src", fotosTemporal[claveFoto2] || "");


            count_tabla++;
            $(".input_valid").each(function () {
                $(this).css("border", "");
            });
            $("#foto_1").attr("src", "");
            $("#imagen_base64_1").val('');
            $("#foto_2").attr("src", "");
            $("#imagen_base64_2").val('');

            $("#lbl_precarga_cantidad").text($("#thead_ganado_medicos tr").length);
        }
    });
    jsRemoveWindowLoad();
}

function ConfirmarGGanadoTabla() {
    var count = 0;
    var aretes = [];
    var siniigas = [];
    var clasificaciones = [];
    var causas = [];
    var salas = [];
    var condiciones = [];
    var edades = [];

    var fotos1 = [];
    var fotos2 = [];

    var id_causa_muerte = [];
    var tipo_salida = $("#tipo_salida").val();

    var medico = [];
    var estado = [];
    var ginecologico = [];
    var arete_manual = [];

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
    else {
        iziToast.question({
            timeout: 200000000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Confirmacion',
            message: 'Estas seguro de confirmar la precarga de arete?',
            position: 'center',
            class: 'custom-question',
            buttons: [
                ['<button><b>SI</b></button>', function (instance, toast) {
                    arete.each(function () {
                        var id = $(this).attr("id").split('_')[1];
                        aretes[count] = $("#arete_" + id + "").text();
                        siniigas[count] = $("#siniiga_" + id + "").text();
                        clasificaciones[count] = $("#clasif_" + id + "").text();
                        causas[count] = $("#causa_" + id + "").text();
                        condiciones[count] = $("#condicion_" + id + "").text();
                        salas[count] = $("#sala_" + id + "").text();
                        edades[count] = $("#edad_" + id + "").text();


                        // 🔑 Claves (si existen)
                        var idx1 = $("#imagen_base64_" + id).val();
                        var idx2 = $("#imagen_base64_2_" + id).val();

                        // 🖼️ Fallback desde <img> de la fila (por si los hiddens quedaron vacíos)
                        var src1 = $("#img_vaca_" + id).attr("src") || "";
                        var src2 = $("#img_vaca_2_" + id).attr("src") || "";

                        // 📦 Prioridad: 1) fotosTemporal[clave]  2) dataURL en <img>  3) vacío
                        var b64_1 = (idx1 && fotosTemporal[idx1]) ? fotosTemporal[idx1]
                            : (src1.indexOf(",") > -1 ? src1 : "");
                        var b64_2 = (idx2 && fotosTemporal[idx2]) ? fotosTemporal[idx2]
                            : (src2.indexOf(",") > -1 ? src2 : "");

                        fotos1[count] = b64_1;
                        fotos2[count] = b64_2;



                        id_causa_muerte[count] = $("#causa_baja_vaca_" + id + "").text();

                        medico[count] = $("#id_arete_medico_" + id + "").text();
                        estado[count] = $("#estado_medico_" + id + "").text();
                        ginecologico[count] = $("#estado_ginecologico_" + id + "").text();
                        arete_manual[count] = $("#arete_manual_" + id + "").text();
                        count++;
                    });

                    var salida = {};
                    salida.id_establo = $("#id_establo_carga").val();
                    salida.folio = $("#folio_g").val();

                    salida.ganado = $("#ganado_g option:selected").text();
                    salida.condicion = $("#condicion_g option:selected").text();
                    $.ajax({
                        url: "../PRUEBAS/ConfirmarGGanadoTabla",
                        async: false,
                        timeout: 900000,
                        type: "POST",
                        data: { salida: salida, tipo_salida: tipo_salida },
                        success: function (id_salida_g) {
                            if (id_salida_g != 0 && id_salida_g != -1 && id_salida_g != -2) {

                                const fotos1_norm = fotos1.map(s => {
                                    if (!s) return "";
                                    const body = s.indexOf(",") > -1 ? s.split(",")[1] : s;
                                    return encodeURIComponent(body); // evita que '+' y '/' se alteren
                                });
                                const fotos2_norm = fotos2.map(s => {
                                    if (!s) return "";
                                    const body = s.indexOf(",") > -1 ? s.split(",")[1] : s;
                                    return encodeURIComponent(body);
                                });

                                $.ajax({
                                    url: "../PRUEBAS/ConfirmarDGanadoTabla",
                                    async: false,
                                    timeout: 900000,
                                    type: "POST",
                                    data: {
                                        folio_g: $("#folio_g").val(),
                                        id_salida_g: id_salida_g,
                                        aretes: aretes,
                                        siniigas: siniigas,
                                        clasificaciones: clasificaciones,
                                        causas: causas,
                                        condiciones: condiciones,
                                        salas: salas,
                                        edades: edades,
                                        imagen_base64: fotos1_norm,
                                        imagen_base64_2: fotos2_norm,
                                        id_causa_muerte: id_causa_muerte,
                                        medico: medico,
                                        estado: estado,
                                        ginecologico: ginecologico,
                                        arete_manual: arete_manual
                                    },
                                    traditional: true,
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
                                            DeshabilitarCamara();

                                            $("#thead_ganado_medicos").html('');

                                            $("#imagen_base64").val("");
                                            $("#foto").attr("src", "");
                                            $("#lbl_precarga_cantidad").text(0);
                                        }
                                        else {
                                            jsRemoveWindowLoad();
                                            iziToast.error({
                                                title: 'Ocurrió un problema al guardar el detalle de la salida',
                                                message: '',
                                            });
                                        }
                                    }
                                });
                            }
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
                    });
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                }, true],
                ['<button>NO</button>', function (instance, toast) {
                    jsRemoveWindowLoad();
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
        jsRemoveWindowLoad();
    }
}

function GenerarFolioEstablo() {
    var id_establo = $("#id_establo_carga").val();
    $.ajax({
        url: "../PRUEBAS/GenerarFolioEstablo",
        async: false,
        timeout: 900000,
        type: "POST",
        data: { id_establo: id_establo },
        success: function (result) {
            $("#folio_g").val(result);
        }
    });

}


function ValidarAreteExistente() {
    var areteIngreso = $("#arete").val().trim().toLowerCase();
    var siniigaIngreso = $("#siniiga").val().trim();

    if (areteIngreso === "sa" || areteIngreso === "sinarete" || areteIngreso === "sin arete") {
        $("#arete_estado").val("NA");
        $("#arete_ginecologico").val("NA");
        $("#edad").val("NA");
        $("#siniiga").val("NA")
        return false;
    }

    var encontradoEnTabla = false;
    $(".aretes").each(function () {
        var id = $(this).attr("id").split('_')[1];
        var areteEnTabla = $("#arete_" + id).text();
        var siniigaEnTabla = $("#siniiga_" + id).text();

        if (areteEnTabla === areteIngreso) {
            encontradoEnTabla = true;
            return false;
        }
        if (siniigaIngreso && siniigaIngreso !== "NA" && siniigaEnTabla === siniigaIngreso) {
            encontradoEnTabla = true;
            return false;
        }
    });
    if (encontradoEnTabla) {
        return true;
    }

    $.ajax({
        url: "../PRUEBAS/ValidarArete",
        async: false,
        type: "POST",
        data: {
            arete: areteIngreso,
            siniiga: siniigaIngreso
        },
        success: function (result) {
            if (result === true || result === "true" || result === "True") {
                existe = true;
            }
            else {
                $.ajax({
                    url: "/PRUEBAS/ConsultarInformacionArete",
                    type: "POST",
                    data: {
                        arete: areteIngreso,
                        siniiga: siniigaIngreso
                    },
                    success: function (result) {
                        if (result.Estado === "-1") {
                            $("#arete_estado").val("NA");
                            $("#arete_ginecologico").val("NA");
                            $("#edad").val("NA");
                        } else {
                            $("#arete_estado").val(result.Estado);
                            $("#arete_ginecologico").val(result.EstadoGinecologico);
                            $("#edad").val(result.Edad);

                            if (arete == "" || arete.trim() == "") {
                                $("#arete").val(result.Vaca);
                            }
                            if (siniiga == "" || siniiga.trim() == "") {
                                $("#siniiga").val(result.Siniiga);
                            }
                        }
                        return false;

                    },
                    error: function (xhr, status, error) {
                        return true;
                    }
                });
            }
        }
    });
}


function SelectCausaMuerte() {
    var id_tipo_causa_muerte = $("#id_tipo_causa_muerte").val();
    $.ajax({
        url: "../PRUEBAS/SelectCausaMuerte",
        async: false,
        type: "POST",
        data: { id_tipo_causa_muerte: id_tipo_causa_muerte },
        success: function (result) {
            $("#id_causa_muerte").html(result);
        }
    });
}

function TiposSalidasClasificacionesSelect() {
    var tipoSalida = $("#tipo_salida").val();
    $.ajax({
        url: "../PRUEBAS/TiposSalidasClasificacionesSelect",
        async: false,
        type: "POST",
        data: { tipoSalida: tipoSalida },
        success: function (result) {
            $("#ganado_g").html(result);
            ConsultarCondicionesClasificacion();
        }
    });
}

function ConsultarCondicionesClasificacion() {
    var id_clasificacion = $("#ganado_g").val();
    $.ajax({
        url: "../PRUEBAS/ConsultarCondicionesClasificacion",
        async: false,
        type: "POST",
        data: { id_clasificacion: id_clasificacion },
        success: function (result) {
            $("#condicion_g").html(result);
        }
    });
}



//#endregion

//#region CONFIRMACION SALIDA DE GANADO
function ConsultarGanadoPendiente() {
    var id_establo = $("#id_establo").val();
    jsShowWindowLoad();
    $.ajax({
        url: "../PRUEBAS/ConsultarGanadoPendiente",
        async: false,
        type: "POST",
        data: { id_establo: id_establo },
        success: function (result) {
            $("#m_ganado_pendiente").modal("show");
            $("#div_ganado_pendiente").html(result);
            $('#tabla_ganado_pendiente').DataTable({
                ordering: false,
                paging: true,
                searching: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
        }
    });
    jsRemoveWindowLoad();
}

function AsignarFolioGanadoPendiente(folio) {
    $("#folio_busqueda").val(folio);
    $("#m_ganado_pendiente").modal("hide");
    ConsultarSalidasAreteEstablo();
    ConsultarInfoFicha();
}


$('#ficha_g').keydown(function (event) {
    //13 ENTER. 9 TAB
    if (event.keyCode === 13 || event.keyCode === 9) {
        if ($("#ficha_g").val() == "" || $("#ficha_g").val().trim() == "") {
            $(".input_ficha").val('');
            var total = $(".peso_vaca_total").length;
            if (total > 0) {
                $(".peso_vaca_total").each(function () {
                    $(this).text('0');
                });
                $("#kilos_total").val("0.00");
            }
        }
        else {
            ConsultarInfoFicha();
        }
    }
});


$('#folio_busqueda').keydown(function (event) {
    //13 ENTER. 9 TAB
    if (event.keyCode === 13 || event.keyCode === 9) {
        event.preventDefault(); // Opcional: evita que TAB cambie el foco
        if ($("#folio_busqueda").val() === "") {
            $("#div_aretes_salidas").html('');
        } else {
            ConsultarSalidasAreteEstablo();
        }
    }
});


//function ConsultarInfoFicha() {

//    var id_establo = $("#id_establo").val();
//    var ficha = $("#ficha_g").val();

//    if (ficha.trim() == "" || ficha == undefined) {
//        iziToast.warning({ title: 'Favor de ingresar una ficha valida', message: '' });
//        $("#ficha_g").val('');
//        $("#id_existe_ficha").text('0');
//        return;
//    }
//    jsShowWindowLoad();
//    $.ajax({
//        url: "../PRUEBAS/ConsultarInfoFicha",
//        async: true,
//        type: "POST",
//        data: {
//            id_establo: id_establo,
//            ficha: ficha
//        },
//        success: function (result) {

//            jsRemoveWindowLoad();
//            if (result != "[]" && result != "0" && result != "-1" && result != "-2" && result != "-3" && result != "-4") {
//                var data = $.parseJSON(result);

//                $("#comprador_g").val(data.proveedor);
//                $("#peso_1_g").val(data.peso1);
//                $("#peso_2_g").val(data.peso2);
//                $("#peso_t_g").val(data.pesoT);
//                $("#chofer_g").val(data.chofer);
//                $("#placas_g").val(data.placas);
//                $("#vehiculo_g").val(data.propietarioCamion);
//                $("#pesador_g").val(data.pesador);

//                var total = $(".peso_vaca_total").length;
//                var porunidad = data.pesoT / total;
//                $(".peso_vaca_total").each(function () {
//                    $(this).text(porunidad);
//                });
//                $("#kilos_total").val(data.pesoT);

//                $("#id_existe_ficha").text("1");
//            }
//            else if (result == "0") {
//                iziToast.warning({ title: 'La ficha no corresponde a una salida de ganado', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//            else if (result == "-1") {
//                iziToast.warning({ title: 'La ficha ya fue registrada con una salida de ganado', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//            else if (result == "-2") {
//                iziToast.warning({ title: 'La ficha pertenece a otro establo', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//            else if (result == "-3") {
//                iziToast.warning({ title: 'No se encontró ninguna ficha de ganado', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//            else if (result == "-4") {
//                iziToast.warning({ title: 'La ficha no esta cerrada', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//            else {
//                iziToast.warning({ title: 'Error desconocido', message: '' });
//                $("#id_existe_ficha").text('0');
//                LimpiarCaptura();
//                LimpiarInfoFicha();
//            }
//        }
//    });
//}
function ConsultarInfoFicha() {
    var total = $(".peso_vaca_total").length;
    var id_establo = $("#id_establo").val();
    var ficha = $("#ficha_g").val();

    if (ficha.trim() == "" || ficha == undefined) {
        iziToast.warning({ title: 'Favor de ingresar una ficha valida', message: '' });
        $("#ficha_g").val('');
        $("#id_existe_ficha").text('0');

        if (total > 0) {
            $(".peso_vaca_total").each(function () {
                $(this).text('0');
            });
        }
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        url: "../PRUEBAS/ConsultarInfoFicha",
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

                $("#id_existe_ficha").text("1");





                var peso_t = $("#peso_t_g").val();


                if (total > 0) {
                    var porunidad = peso_t / total;
                    $(".peso_vaca_total").each(function () {
                        $(this).text(porunidad);
                    });
                    $("#kilos_total").val(peso_t);
                }


            }
            else if (result == "0") {
                iziToast.error({
                    title: 'La ficha no corresponde a una salida de ganado',
                    message: '',
                });
                LimpiarCaptura();
                LimpiarInfoFicha();
            }
            else if (result == "-1") {
                iziToast.error({
                    title: 'La ficha ya fue registrada con una salida de ganado',
                    message: '',
                });
                LimpiarCaptura();
                LimpiarInfoFicha();
            }
            else {
                iziToast.error({
                    title: 'No se encontró ninguna ficha de ganado',
                    message: '',
                });
                LimpiarCaptura();
                LimpiarInfoFicha();
            }
        }
    });
}
function LimpiarInfoFicha() {
    $("#comprador_g").val('');
    $("#peso_1_g").val('');
    $("#peso_2_g").val('');
    $("#peso_t_g").val('');
    $("#chofer_g").val('');
    $("#placas_g").val('');
    $("#vehiculo_g").val('');
    $("#pesador_g").val('');

    var total = $(".peso_vaca_total").length;
    $(".peso_vaca_total").each(function () {
        $(this).text('');
    });
    $("#kilos_total").val('0.00');
}

function GenerarSGanado() {
    ConsultarInfoFicha();
    var existe_ficha = $("#id_existe_ficha").text();
    if (existe_ficha == 0) {
        LimpiarInfoFicha();
        return;
    }
    if ($("#folio_busqueda").val() === "") {
        $("#div_aretes_salidas").html('');
    } else {
        ConsultarSalidasAreteEstablo();
    }

    var salida = {};
    salida.id_salida_gan_g = $("#id_salida_ganado_bascula_g").text();
    salida.ficha = $("#ficha_g").val();
    salida.peso1 = $("#peso_1_g").val();
    salida.peso2 = $("#peso_2_g").val();
    salida.peso_t = $("#peso_t_g").val();
    salida.pesador = $("#pesador_g").val();
    salida.chofer = $("#chofer_g").val();
    salida.placas = $("#placas_g").val();
    salida.vehiculo = $("#vehiculo_g").val();
    salida.comprador = $("#comprador_g").val();
    salida.importe = 0;

    var total = $(".peso_vaca_total").length;
    if (total == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un folio con ganado',
        });
        return;
    }

    $.ajax({
        url: "../PRUEBAS/GenerarSGanado",
        async: false,
        timeout: 900000,
        type: "POST",
        data: { salida: salida },
        success: function (id_salida_g) {
            jsShowWindowLoad();
            if (id_salida_g != 0 && id_salida_g != -1 && id_salida_g != -2) {
                iziToast.success({
                    title: 'Exito!',
                    message: 'Salida de ganado registrada correctamente',
                });
                LimpiarCaptura();
                $(".input_valid").each(function () {
                    $(this).css("border", "");
                });
                $("#folio_busqueda").val('');
                $("#div_aretes_salidas").html('');

                GenerarPDFSalidaGanado(id_salida_g, salida.folio);

                jsRemoveWindowLoad();
            }

            else if (id_salida_g == 0) {
                iziToast.error({
                    title: 'Ocurrió un problema al guardar la información general',
                    message: '',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un problema al realizar el proceso',
                    message: '',
                });
            }
        }
    });
    jsRemoveWindowLoad();
}

function ConsultarSalidasAreteEstablo() {
    var folio_g = $("#folio_busqueda").val();
    var id_establo = $("#id_establo").val();
    jsShowWindowLoad();
    $.ajax({
        url: "../PRUEBAS/ValidarConsultarSalidasAreteEstablo",
        async: false,
        type: "POST",
        data: {
            folio_g: folio_g,
            id_establo: id_establo
        },
        success: function (result) {
            if (result == "0") {
                $.ajax({
                    url: "../PRUEBAS/ConsultarSalidasAreteEstablo",
                    async: false,
                    type: "POST",
                    data: {
                        folio_g: folio_g,
                        id_establo: id_establo
                    },
                    success: function (result) {
                        try {
                            $("#tabla_aretes_establo").dataTable().fnDestroy();
                        } catch (e) { }
                        $("#div_aretes_salidas").html(result);
                        $('#tabla_aretes_establo').DataTable({
                            dom: 't',               // solo la tabla, sin buscador, sin info, sin paginación
                            keys: false,
                            ordering: true,
                            paging: false,
                            searching: false,
                            info: false,
                            lengthChange: false,
                            responsive: true
                        });
                        jsRemoveWindowLoad();

                        var peso_t = $("#peso_t_g").val();
                        var total = $(".peso_vaca_total").length;
                        var porunidad = peso_t / total;
                        $(".peso_vaca_total").each(function () {
                            $(this).text(porunidad);
                        });
                        $("#kilos_total").val(peso_t);
                    }
                });
            }
            if (result == "1") {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se encontró informacion o pertenece a otro establo.',
                });
                $("#div_aretes_salidas").html('');
            }
            if (result == "2") {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ya se utilizó el folio ingresado',
                });
                $("#div_aretes_salidas").html('');
            }
        }
    });
    jsRemoveWindowLoad();

}
//#endregion




function AgregarVacaTabla() {
    var peso_neto = $("#peso_neto").val();
    var cantidad = $("#cantidad").val();
    var p_unitario = $("#p_unitario").val();
    var condicion = $("#condicion_g").find('option:selected').text();
    var imagen_base64 = $("#imagen_base64").val();
    var id_causa_muerte = $("#id_causa_muerte").val();
    var causa_muerte = $("#id_causa_muerte").find('option:selected').text();
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
        if (arete == "" || arete.trim() == "") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es obligatorio ingresar el arete.',
            });
            return;
        }
        if (causa_muerte == "" || causa_muerte.trim() == "") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es obligatorio seleccionar la causa de muerte.',
            });
            return;
        }
        if (imagen_base64 == '' || $("#foto").attr("src") == '') {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es obligatorio tomar la fotografia.',
            });
            return;
        }
        if (ValidarAreteExistente() == true) {
            iziToast.warning({
                title: 'Aviso',
                message: 'Ya se esta utilizando el arete',
            });
            return;
        }
        $.ajax({
            url: "../PRUEBAS/AgregarVacaTabla",
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
                condicion: condicion,
                imagen_base64: imagen_base64,
                id_causa_muerte: id_causa_muerte,
                causa_muerte: causa_muerte
            },
            success: function (result) {
                $(".input_detalle").val('');
                $("#thead_ganado").append(result);
                count_tabla++;
                $(".input_valid").each(function () {
                    $(this).css("border", "");
                });
                CalcularTotales();
                $("#foto").attr("src", "");
                $("#imagen_base64").val('');
            }
        });
    }
}

function EliminarFilaSalida(count) {
    $("#row_" + count + "").remove();
    CalcularTotales();
    $("#lbl_precarga_cantidad").text($("#thead_ganado_medicos tr").length);
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

    var imagen_base64 = [];
    var id_causa_muerte = [];

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
            imagen_base64[count] = $("#img_vaca_" + id + "").attr("src");
            id_causa_muerte[count] = $("#causa_baja_vaca_" + id + "").text();
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
            url: "../PRUEBAS/GenerarSalidaGanado",
            async: false,
            timeout: 900000,
            type: "POST",
            data: { salida: salida, tipo_salida: tipo_salida },
            success: function (id_salida_g) {
                jsShowWindowLoad();
                if (id_salida_g != 0 && id_salida_g != -1 && id_salida_g != -2) {
                    $.ajax({
                        url: "../PRUEBAS/GenerarSalidaGanadoDetalle",
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
                            importes: importes,
                            imagen_base64: imagen_base64,
                            id_causa_muerte: id_causa_muerte
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
                                DeshabilitarCamara();
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
        url: "../PRUEBAS/ConsultarSalidasEstablo",
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

        fetch('../PRUEBAS/ExportarSalidasGanadoExcel', {
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
        url: "../PRUEBAS/GenerarPDFSalidaGanado",
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


//#region CAMARA
//var streamActivo = null;
//function HabilitarCamara() {
//    $("#foto").attr("src", "");
//    var video = document.getElementById("video");

//    // Intentar primero la cámara trasera
//    navigator.mediaDevices.getUserMedia({
//        video: { facingMode: { exact: "environment" } }
//    })
//        .then(stream => {
//            streamActivo = stream;
//            video.srcObject = stream;
//            $("#video").css("display", "inline");
//            $("#btn_camara").attr("onclick", "TomarCaptura();");
//            $("#btn_camara").text("Tomar captura");
//            $("#foto").css("display", "none");
//        })
//        .catch(error => {
//            iziToast.warning({
//                title: 'Aviso',
//                message: 'No se pudo acceder a la cámara trasera. Intentando la cámara frontal...'
//            });
//            console.warn("", error);

//            // Intentar la cámara frontal si falla la trasera
//            navigator.mediaDevices.getUserMedia({
//                video: { facingMode: "user" }
//            })
//                .then(stream => {
//                    iziToast.warning({
//                        title: 'Aviso',
//                        message: '⚠️ No se pudo usar la cámara trasera, se activó la cámara frontal.'
//                    });

//                    streamActivo = stream;
//                    video.srcObject = stream;
//                    $("#video").css("display", "inline");
//                    $("#btn_camara").attr("onclick", "TomarCaptura();");
//                    $("#btn_camara").text("Tomar captura");
//                    $("#foto").css("display", "none");
//                })
//                .catch(error2 => {
//                    iziToast.warning({
//                        title: '❌ No se pudo acceder a ninguna cámara.',
//                        message: error2
//                    });
//                });
//        });
//}


//function DeshabilitarCamara() {
//    if (streamActivo) {
//        streamActivo.getTracks().forEach(track => track.stop());
//        video.srcObject = null;
//    }
//}

//function TomarCaptura() {
//    var foto = document.getElementById("foto");
//    canvas.getContext("2d").drawImage(video, 0, 0, canvas.width, canvas.height);
//    const dataUrl = canvas.toDataURL("image/png");
//    document.getElementById("imagen_base64").value = dataUrl;
//    $("#imagen_base64").value = dataUrl;
//    foto.src = dataUrl;
//    foto.style.display = 'block';
//    DeshabilitarCamara();
//    $("#video").css("display", "none");

//    $("#btn_camara").attr("onclick", "HabilitarCamara();");
//    $("#btn_camara").text("Activar camara");
//    $("#foto").css("display", "inline");
//}
//#endregion




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
        url: "../PRUEBAS/ConsultarReporteTiposMuerte",
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

            var chartDom = document.getElementById('div_chart_tipos_muerte');
            var myChart = echarts.init(chartDom);
            var option;
            option = {
                title: {
                    text: 'CAUSAS DE MUERTE',
                    subtext: fecha_inicio + " - " + fecha_fin,
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
        url: "../PRUEBAS/ConsultarReporteCausasMuerte",
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
            var option;
            option = {
                title: {
                    text: 'DETALLE',
                    subtext: fecha_inicio + " - " + fecha_fin,
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
        url: "../PRUEBAS/ConsultarReporteIndicadoresTable",
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