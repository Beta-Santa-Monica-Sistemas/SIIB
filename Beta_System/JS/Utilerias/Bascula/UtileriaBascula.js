function AsignadorUtileriasBascula() {
    $("#confirmar_primera_pesada").attr("onclick", "GuardarPrimeraPesadaSistemas();");
}
function AsignadorUtileriasBasculaCp() {
    $("#confirmar_primera_pesada").attr("onclick", "GuardarPrimeraPesadaSistemas();");
}

function ConsultarFichasPendientesSistemas() {
    $("#m_pendientes").modal("show");
    var id_establo = $("#id_establo_sp").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarFichasPendientesSistemas",
        data: {
            id_establo: id_establo
        },
        success: function (response) {
            $("#div_ficha_pendiente").html(response);
            $('#table_fichas_pendientes').DataTable({
                ordering: false,
                paging: true,
                searching: true,
                dom: "Bfrtip",
                buttons: [{}],
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

function FichaPendienteSistemas(ficha_bascula) {
    $("#id_ficha_g_sp").val(ficha_bascula);
    $("#m_pendientes").modal("hide");
}

//#region PESADA
function GuardarPrimeraPesadaSistemas() {
    //TIPO 1 - LECHE, TIPO 2 FORRAJE, TIPO 3 REGULAR
    var tipo_ficha = $("#id_tipo_ficha").val();
    var peso_1 = $("#peso_bascula").val();
    var fecha1 = $("#fecha_pesada_1").val();
    //VALIDAR PESO
    if (peso_1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario el peso para registrar la ficha'
        });
        return;
    }
    //VALIDAR FECHA 1
    if (fecha1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una fecha valida',
        });
        return;
    }

    //#region DATOS GENERALES PARA LOS TIPOS DE FICHA
    //ESTABLO
    var id_establo = ($("#id_establo").val().trim() === "") ? " " : $("#id_establo").val().trim();
    //PROVEEDOR
    var select_proveedor = ($("#select_proveedor").val().trim() === "") ? " " : $("#select_proveedor").val().trim();
    //ARTICULO
    var select_articulo = ($("#select_articulo").val().trim() === "") ? " " : $("#select_articulo").val().trim();
    //MOVIMIENTO
    var tmov = ($("#tmov").val().trim() === "") ? " " : $("#tmov").val();
    //TIPO DE MOVIMIENTO
    var cvemov = ($("#cvemov").val() === "") ? " " : $("#cvemov").val().trim();
    //CHOFER
    var chofer = ($("#conductor").val().trim() === "") ? "" : $("#conductor").val().trim();
    //PLACAS
    var placas = ($("#placas").val().trim() === "") ? "" : $("#placas").val().trim();
    //OBSERVACIONES PARA TIPO FORRAJE Y REGULAR, EN ENVIO DE LECHE ES TANQUE
    var obs = ($("#obs").val().trim() === "") ? " " : $("#obs").val().trim();
    //#endregion

    var c_ficha = {};


    //#region VALIDACIONES GENERALES
    //VALIDACION PROVEEDOR Y ARTICULO
    if (select_proveedor == 0 && select_articulo == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un proveedor y un artículo para continuar',
        });
        return;
    }
    //VALIDACION ARTICULO
    if (select_articulo == 0 && select_proveedor != 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un artículo para continuar',
        });
        return;
    }
    //VALIDACION PROVEEDOR
    if (select_proveedor == 0 && select_articulo != 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un proveedor para continuar',
        });
        return;
    }
    //VALIDACION CHOFER Y PLACAS
    if (chofer == undefined || chofer == "" || placas == undefined || placas == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario rellenar todos los campos obligatorios para continuar',
        });
        $(".borders").each(function () {
            var value = $(this).val().trim();
            if (value === "") {
                $(this).css("border-color", "red");
            } else {
                $(this).css("border-color", "");
            }
        });
        return;
    }
    //#endregion

    var linea_trasp;
    var agrupada;
    var folio;
    var no_pozo;
    var peso_origen;
    var mat_seca;
    var tabla;
    var variedad;
    var corte;
    var pacas;
    var ensilador;
    var maquilador;
    var predio_cliente;
    var sucursal;
    var destino;
    var placas_pipa;

    //ENVIO DE LECHE
    if (tipo_ficha == 1) {
        //LINEA TRANSPORTISTA
        linea_trasp = ($("#linea_trasp").val().trim() === "") ? " " : $("#linea_trasp").val().trim();
        //PLACAS PIPA
        placas_pipa = ($("#placas_pipa").val().trim() === "") ? " " : $("#placas_pipa").val().trim();
        if (placas_pipa == undefined || placas_pipa == "" || placas_pipa == " ") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario indicar las placas de la pipa',
            });
            return;
        }
        agrupada = false;
        folio = " ";
        //SG = 1, SM = 3
        if (id_establo == 1) { no_pozo = 1; }
        if (id_establo == 3) { no_pozo = 5; }
        peso_origen = 0;
        mat_seca = 0;
        tabla = "NA";
        variedad = " ";
        corte = "0";
        pacas = "0";
        ensilador = " ";
        maquilador = " ";
        predio_cliente = " ";
        sucursal = " ";
        destino = " ";


    }
    //FORRAJE
    else if (tipo_ficha == 2) {
        linea_trasp = ($("#linea_trasp").val().trim() === "") ? " " : $("#linea_trasp").val().trim();
        agrupada = ($("#agrupada").is(':checked')) ? true : false;
        folio = ($("#folio").val().trim() === "") ? " " : $("#folio").val().trim();
        no_pozo = ($("#no_pozo").val().trim() === "") ? " " : $("#no_pozo").val().trim();
        peso_origen = ($("#peso_origen").val().trim() === "") ? 0 : $("#peso_origen").val().trim();
        mat_seca = ($("#mat_seca").val().trim() === "") ? 0 : $("#mat_seca").val().trim();
        tabla = ($("#tabla_p1").val().trim() === "") ? " " : $("#tabla_p1").val().trim();
        variedad = ($("#variedad_p1").val().trim() === "") ? " " : $("#variedad_p1").val().trim();
        corte = ($("#corte_p1").val().trim() === "") ? " " : $("#corte_p1").val().trim();
        pacas = ($("#pacas_p1").val().trim() === "") ? " " : $("#pacas_p1").val().trim();
        ensilador = ($("#ensilador_p1").val().trim() === "") ? " " : $("#ensilador_p1").val().trim();
        maquilador = ($("#maquilador").val().trim() === "") ? " " : $("#maquilador").val().trim();
        predio_cliente = ($("#predio_cliente").val().trim() === "") ? " " : $("#predio_cliente").val().trim();
        sucursal = ($("#sucursal").val().trim() === "") ? " " : $("#sucursal").val().trim();
        destino = ($("#destino").val().trim() === "") ? " " : $("#destino").val().trim();
        placas_pipa = " ";
        if (tabla == undefined || tabla == "" || tabla == " ") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario indicar la tabla',
            });
            return;
        }
        if (no_pozo == null || no_pozo == undefined) {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario seleccionar el no.pozo/almacen',
            });
            return;
        }
    }
    //REGULAR
    else {
        linea_trasp = 1;
        no_pozo = ($("#no_pozo").val().trim() === "") ? " " : $("#no_pozo").val().trim();
        folio = ($("#folio").val().trim() === "") ? " " : $("#folio").val().trim();
        sucursal = ($("#sucursal").val().trim() === "") ? " " : $("#sucursal").val().trim();
        agrupada = ($("#agrupada").is(':checked')) ? true : false;
        destino = ($("#destino").val().trim() === "") ? " " : $("#destino").val().trim();
        peso_origen = ($("#peso_origen").val().trim() === "") ? 0 : $("#peso_origen").val().trim();

        mat_seca = 0;
        tabla = "NA";
        variedad = " ";
        corte = "0";
        pacas = "0";
        ensilador = " ";
        maquilador = " ";
        predio_cliente = " ";
        placas_pipa = " ";
    }


    //CLIENTE ENVIO DE LECHE
    if (tipo_ficha == 1 || tipo_ficha == 4) { c_ficha.id_envio_leche_cliente_ms = select_proveedor; }
    //PROVEEDOR/CLIENTE FORRAJE Y REGULAR
    else { c_ficha.id_compras_proveedor = select_proveedor; }


    c_ficha.placas_pipa = placas_pipa;
    c_ficha.id_no_pozo = no_pozo;
    c_ficha.sucursal = sucursal;
    c_ficha.destino = destino;
    c_ficha.maquilador = maquilador;
    c_ficha.predio_cliente = predio_cliente;
    c_ficha.tabla = tabla;
    c_ficha.variedad = variedad;
    c_ficha.corte = corte;
    c_ficha.pacas = pacas;
    c_ficha.ensilador = ensilador;
    c_ficha.peso_origen = peso_origen;
    c_ficha.peso_materia_seca = mat_seca;
    c_ficha.folio = folio;
    c_ficha.agrupada = agrupada;
    c_ficha.peso_1 = peso_1;
    c_ficha.id_ficha_tipo = tipo_ficha;
    c_ficha.id_establo = id_establo;
    c_ficha.id_articulo_bascula = select_articulo;
    c_ficha.id_codigo_movimiento = cvemov;
    c_ficha.id_tipo_movimiento = tmov;
    c_ficha.id_linea_transportista = linea_trasp;
    c_ficha.chofer = chofer;
    c_ficha.placas = placas;
    c_ficha.observaciones = obs;
    c_ficha.recibido = false;
    c_ficha.termina = false;
    c_ficha.confirmada_envio = false;
    c_ficha.fecha_registo = fecha1;

    jsShowWindowLoad();

    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/GuardarPrimeraPesadaSistemas",
        data: {
            c_ficha: c_ficha
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Ficha registrada correctamente!',
                });
                $("#peso_bascula").val('');
                ConsultarUltimaFichaSistemas();
                $(".borders").css("border-color", "");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al registrar la ficha',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function GuardarSegundaPesadaSistemas() {
    var ficha = $("#id_ficha_g_sp").val();
    var peso_2 = $("#peso_bascula_sp").val();
    var tipo_ficha = 0;
    var peso_total = $("#pesot_sp").val();
    if (ficha == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de buscar la ficha',
        });
        return;
    }

    if (peso_2 == "" || peso_total == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de asegurarse de que el PESO 2 tenga un valor',
        });
        return;
    }


    tipo_ficha = $("#tipo_ficha_pesada").text();
    var envio_leche = {};
    var c_calidad_1 = {};
    var tanque = $("#obs_sp").val();
    var kilos_ficha = $("#pesot_sp").val();
    var litros_ficha = $("#litros_sp").val();
    var sello = $("#sello_sp").val();
    var temperatura = $("#temperatura_sp").val();

    if (tipo_ficha == 1) {
        if (sello == undefined || sello.trim() == "" || temperatura == undefined || temperatura.trim() == "") {
            iziToast.warning({
                title: 'Aviso',
                message: 'La temperatura y sello son campos obligatorios',
            });
            return;
        }
        envio_leche.id_ficha_bascula = ficha;
        envio_leche.tanque = tanque;
        envio_leche.kilos_ficha = kilos_ficha;
        envio_leche.litros_ficha = litros_ficha;
        envio_leche.sello = sello;

        c_calidad_1.muestra_calidad = " ";
        c_calidad_1.sala = " ";
        c_calidad_1.antibiotico = -1;
        c_calidad_1.grasa = 0;
        c_calidad_1.proteina = 0;
        c_calidad_1.solidos_no_grasos = 0;
        c_calidad_1.solidos_totales = 0;
        c_calidad_1.lactosa = 0;
        c_calidad_1.caseina = 0;
        c_calidad_1.acidez = 0;
        c_calidad_1.crioscopia = 0;
        c_calidad_1.urea = 0;
        c_calidad_1.ccs = 0;
        c_calidad_1.betalactamicos = -1;
        c_calidad_1.tetraciclina = -1;
        c_calidad_1.aflatoxinas = -1;
        c_calidad_1.sulfasimas = -1;
        c_calidad_1.alcohol_75 = -1;
        c_calidad_1.temperatura = temperatura;
    }
    else {
        tipo_ficha = 0;
    }



    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/GuardarSegundaPesada",
        data: {
            ficha: ficha,
            peso_2: peso_2,
            envio_leche: envio_leche,
            calidad_leche: c_calidad_1,
            tipo_ficha: tipo_ficha
        },
        success: function (response) {
            if (response == 1) {
                iziToast.success({
                    title: 'Exito',
                    message: 'Segunda pesada generada correctamente',
                });
                $("#id_ficha_g_sp").val('');
                $("#confirmar_segunda_pesada").attr("onclick", "AdvertenciaPesada(2);");
                AspectoSegundaPesadaBoton(2);
                ImpresionFichaPDF(ficha);
                LimpiarCamposFicha(2);
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al confirmar la segunda pesada',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion

//#region BASCULA CATALOGOS

//SELECT
function ConsultarUltimaFichaCp() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarUltimaFicha",
        data: {
        },
        success: function (response) {
            if (response != 0) {
                $("#id_ficha_g_completa").val(response);
            }
            else {
                iziToast.error({
                    title: 'Aviso',
                    message: 'Ocurrió un problema al consultar la ultima ficha',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function SelectProveedorBasculaCp() {
    var tipo_ficha = $("#id_tipo_ficha_completa").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarProveedoresTipoBasculaSelect",
        data: {
            tipo_ficha: tipo_ficha
        },
        success: function (response) {
            $("#select_proveedor_cp").empty();
            $("#select_proveedor_cp").append('<option value="0">Selecciona un proveedor</option>');
            $("#select_proveedor_cp").append(response);
            $("#select_proveedor_cp").val("0").change();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function SelectArticuloBasculaCp() {
    var tipo_ficha = $("#id_tipo_ficha_completa").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarArticulosBasculaTiposSelect",
        data: {
            tipo_ficha: tipo_ficha
        },
        success: function (response) {
            $("#select_articulo_cp").empty();
            $("#select_articulo_cp").append('<option value="0">Selecciona un articulo</option>');
            $("#select_articulo_cp").append(response);
            $("#select_articulo_cp").val("0").change();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ConsultarPozosEstabloCp() {
    var id_establo = $("#id_establo_completa").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarPozosSelect",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#no_pozo_cp").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarTipoFichasEstabloSelectCp() {
    var id_establo = $("#id_establo_completa").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarTipoFichasEstabloSelect",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#id_tipo_ficha_completa").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarClavesMovimientosEstabloSelectCp() {
    var id_ficha_tipo = $("#id_tipo_ficha_completa").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarClavesMovimientosEstabloSelect",
        data: { id_ficha_tipo: id_ficha_tipo },
        success: function (response) {
            $("#cvemov_cp").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultaGeneralSelectEstabloCp() {
    ConsultarTipoFichasEstabloSelectCp();
    ConsultarPozosEstabloCp();
    ConsultarClavesMovimientosEstabloSelectCp();
}

function ConsultarFormatoFichaBasculaCp() {
    var idtipoficha = $("#id_tipo_ficha_completa").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarFormatoFichaBasculaCp",
        data: {
            idtipoficha: idtipoficha
        },
        success: function (response) {
            $("#div_tipo_ficha_completo").html(response);
            $("#peso_bascula").val('');
            SelectProveedorBasculaCp(); SelectArticuloBasculaCp();
            ConsultarPozosEstabloCp();
            ConsultarClavesMovimientosEstabloSelectCp();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//#endregion


function CalcularPesoTcp() {
    var peso1 = parseFloat($("#peso_bascula1_completa").val()) || 0;
    var peso2 = parseFloat($("#peso_bascula2_completa").val()) || 0;
    var peso3 = Math.abs(peso1 - peso2);

    $("#peso_basculat_completa").val(peso3);
}
function CalcularPesoTsp() {
    var peso1 = parseFloat($("#peso1_sp").val()) || 0;
    var peso2 = parseFloat($("#peso_bascula_sp").val()) || 0;
    var peso3 = Math.abs(peso1 - peso2);

    $("#pesot_sp").val(peso3);
}


function GuardarFichaCompletaSistemas() {
    //TIPO 1 - LECHE, TIPO 2 FORRAJE, TIPO 3 REGULAR PROVEEDOR, TIPO 4 REGULAR CLIENTE
    var tipo_ficha = $("#id_tipo_ficha_completa").val();
    var peso_1 = $("#peso_bascula1_completa").val();
    var peso_2 = $("#peso_bascula2_completa").val();
    var peso_t = $("#peso_basculat_completa").val();
    var fecha1 = $("#fecha_pesada_1_completa").val();
    var fecha2 = $("#fecha_pesada_2_completa").val();
    //VALIDAR PESO
    if (peso_1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario el peso 1 para registrar la ficha'
        });
        return;
    }
    if (peso_2 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario el peso 2 para registrar la ficha'
        });
        return;
    }
    //VALIDAR FECHAS
    if (fecha1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una fecha valida (dia, mes, año, horas, minutos) en Fecha primera pesada',
        });
        return;
    }
    if (fecha2 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una fecha valida (dia, mes, año, horas, minutos) en Fecha segunda pesada',
        });
        return;
    }


    //#region DATOS GENERALES PARA LOS TIPOS DE FICHA
    //ESTABLO
    var id_establo = $("#id_establo_completa").val();
    //PROVEEDOR
    var select_proveedor = $("#select_proveedor_cp").val();
    //ARTICULO
    var select_articulo = $("#select_articulo_cp").val();
    //MOVIMIENTO
    var tmov = $("#tmov_cp").val();
    //TIPO DE MOVIMIENTO
    var cvemov = $("#cvemov_cp").val();
    //CHOFER
    var chofer = ($("#conductor_cp").val().trim() === "") ? "" : $("#conductor_cp").val().trim();
    //PLACAS
    var placas = ($("#placas_cp").val().trim() === "") ? "" : $("#placas_cp").val().trim();
    //OBSERVACIONES PARA TIPO FORRAJE Y REGULAR, EN ENVIO DE LECHE ES TANQUE
    var obs = ($("#obs_cp").val().trim() === "") ? " " : $("#obs_cp").val().trim();
    //#endregion

    var c_ficha = {};

    //CLIENTE ENVIO DE LECHE
    if (tipo_ficha == 1 || tipo_ficha == 4) { c_ficha.id_envio_leche_cliente_ms = select_proveedor; }
    //PROVEEDOR/CLIENTE FORRAJE Y REGULAR
    else { c_ficha.id_compras_proveedor = select_proveedor; }

    //#region VALIDACIONES GENERALES
    //VALIDACION PROVEEDOR Y ARTICULO
    if (select_proveedor == 0 && select_articulo == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un proveedor y un artículo para continuar',
        });
        return;
    }
    //VALIDACION ARTICULO
    if (select_articulo == 0 && select_proveedor != 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un artículo para continuar',
        });
        return;
    }
    //VALIDACION PROVEEDOR
    if (select_proveedor == 0 && select_articulo != 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar un proveedor para continuar',
        });
        return;
    }
    //VALIDACION CHOFER Y PLACAS
    if (chofer == undefined || chofer == "" || placas == undefined || placas == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario rellenar todos los campos obligatorios para continuar',
        });
        $(".borders").each(function () {
            var value = $(this).val().trim();
            if (value === "") {
                $(this).css("border-color", "red");
            } else {
                $(this).css("border-color", "");
            }
        });
        return;
    }
    //#endregion

    var linea_trasp;
    var agrupada;
    var folio;
    var no_pozo;
    var peso_origen;
    var mat_seca;
    var tabla;
    var variedad;
    var corte;
    var pacas;
    var ensilador;
    var maquilador;
    var predio_cliente;
    var sucursal;
    var destino;
    var placas_pipa;

    //ENVIO DE LECHE
    if (tipo_ficha == 1) {
        //LINEA TRANSPORTISTA
        linea_trasp = $("#linea_trasp_cp").val();
        //PLACAS PIPA
        placas_pipa = ($("#placas_pipa_cp").val().trim() === "") ? " " : $("#placas_pipa_cp").val().trim();
        if (placas_pipa == undefined || placas_pipa == "" || placas_pipa == " ") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario indicar las placas de la pipa',
            });
            return;
        }
        agrupada = false;
        folio = " ";
        //SG = 1, SM = 3
        if (id_establo == 1) { no_pozo = 1; }
        if (id_establo == 3) { no_pozo = 5; }
        peso_origen = 0;
        mat_seca = 0;
        tabla = "NA";
        variedad = " ";
        corte = "0";
        pacas = "0";
        ensilador = " ";
        maquilador = " ";
        predio_cliente = " ";
        sucursal = " ";
        destino = " ";


    }
    //FORRAJE
    else if (tipo_ficha == 2) {
        linea_trasp = ($("#linea_trasp_cp").val().trim() === "") ? " " : $("#linea_trasp_cp").val().trim();
        agrupada = ($("#agrupada_cp").is(':checked')) ? true : false;
        folio = ($("#folio_cp").val().trim() === "") ? " " : $("#folio_cp").val().trim();
        no_pozo = ($("#no_pozo_cp").val().trim() === "") ? " " : $("#no_pozo_cp").val().trim();
        peso_origen = ($("#peso_origen_cp").val().trim() === "") ? 0 : $("#peso_origen_cp").val().trim();
        mat_seca = ($("#mat_seca_cp").val().trim() === "") ? 0 : $("#mat_seca_cp").val().trim();
        tabla = ($("#tabla_p1_cp").val().trim() === "") ? " " : $("#tabla_p1_cp").val().trim();
        variedad = ($("#variedad_p1_cp").val().trim() === "") ? " " : $("#variedad_p1_cp").val().trim();
        corte = ($("#corte_p1_cp").val().trim() === "") ? " " : $("#corte_p1_cp").val().trim();
        pacas = ($("#pacas_p1_cp").val().trim() === "") ? " " : $("#pacas_p1_cp").val().trim();
        ensilador = ($("#ensilador_p1_cp").val().trim() === "") ? " " : $("#ensilador_p1_cp").val().trim();
        maquilador = ($("#maquilador_cp").val().trim() === "") ? " " : $("#maquilador_cp").val().trim();
        predio_cliente = ($("#predio_cliente_cp").val().trim() === "") ? " " : $("#predio_cliente_cp").val().trim();
        sucursal = ($("#sucursal_cp").val().trim() === "") ? " " : $("#sucursal_cp").val().trim();
        destino = ($("#destino_cp").val().trim() === "") ? " " : $("#destino_cp").val().trim();
        placas_pipa = " ";
        if (tabla == undefined || tabla == "" || tabla == " ") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario indicar la tabla',
            });
            return;
        }
        if (no_pozo == null || no_pozo == undefined) {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario seleccionar el no.pozo/almacen',
            });
            return;
        }
    }
    //REGULAR
    else {
        linea_trasp = 1;
        no_pozo = ($("#no_pozo_cp").val().trim() === "") ? " " : $("#no_pozo_cp").val().trim();
        folio = ($("#folio_cp").val().trim() === "") ? " " : $("#folio_cp").val().trim();
        sucursal = ($("#sucursal_cp").val().trim() === "") ? " " : $("#sucursal_cp").val().trim();
        agrupada = ($("#agrupada_cp").is(':checked')) ? true : false;
        destino = ($("#destino_cp").val().trim() === "") ? " " : $("#destino_cp").val().trim();
        peso_origen = ($("#peso_origen_cp").val().trim() === "") ? 0 : $("#peso_origen_cp").val().trim();

        mat_seca = 0;
        tabla = "NA";
        variedad = " ";
        corte = "0";
        pacas = "0";
        ensilador = " ";
        maquilador = " ";
        predio_cliente = " ";
        placas_pipa = " ";
    }

    c_ficha.placas_pipa = placas_pipa;
    c_ficha.id_no_pozo = no_pozo;
    c_ficha.sucursal = sucursal;
    c_ficha.destino = destino;
    c_ficha.maquilador = maquilador;
    c_ficha.predio_cliente = predio_cliente;
    c_ficha.tabla = tabla;
    c_ficha.variedad = variedad;
    c_ficha.corte = corte;
    c_ficha.pacas = pacas;
    c_ficha.ensilador = ensilador;
    c_ficha.peso_origen = peso_origen;
    c_ficha.peso_materia_seca = mat_seca;
    c_ficha.folio = folio;
    c_ficha.agrupada = agrupada;
    c_ficha.peso_1 = peso_1;
    c_ficha.peso_2 = peso_2;
    c_ficha.peso_t = peso_t;
    c_ficha.id_ficha_tipo = tipo_ficha;
    c_ficha.id_establo = id_establo;
    c_ficha.id_articulo_bascula = select_articulo;
    c_ficha.id_codigo_movimiento = cvemov;
    c_ficha.id_tipo_movimiento = tmov;
    c_ficha.id_linea_transportista = linea_trasp;
    c_ficha.chofer = chofer;
    c_ficha.placas = placas;
    c_ficha.observaciones = obs;
    c_ficha.recibido = false;
    c_ficha.termina = true;
    c_ficha.confirmada_envio = false;
    c_ficha.fecha_registo = fecha1;
    c_ficha.fecha_segunda_pesada = fecha2;

    jsShowWindowLoad();

    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/GuardarFichaCompletaSistemas",
        data: {
            c_ficha: c_ficha
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Ficha registrada correctamente!',
                });
                $("#peso_bascula").val('');
                ConsultarUltimaFichaSistemas();
                $(".borders").css("border-color", "");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al registrar la ficha',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}