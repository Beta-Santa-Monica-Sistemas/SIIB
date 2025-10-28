//#region REPORTES

function ConsultarReporteFichas() {
    var id_establo = $("#id_establo_reporte").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarReporteFichas",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#table_fichas_reporte").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_reporte").html(response);
            $('#table_fichas_reporte').DataTable({
                keys: true,
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
                language: {
                    encoding: "UTF-8"
                },
                responsive: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ExportarFichasBasculaExcel() {
    var id_establo = $("#id_establo_reporte").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../REPORTES/DescargarFichasBasculaExcel', {
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

//#endregion

//#region PESO BASCULA
function ObtenerPesoBascula(pesada) {
    var id_establo = 0;
    //PRIMERA PESAS
    if (pesada == 1) {
        var id_establo = $("#id_establo").val();
    }
    //SEGUNDA PESADA
    else {
        var id_establo = $("#id_establo_sp").val();
    }
    if (id_establo != 0) {
        var peso = 0;
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/ObtenerPesoBascula",
            data: { id_establo: id_establo },
            success: function (response) {
                if (response != "") {
                    if (response != "No se pudo abrir el puerto") {
                        //PRIMERA PESADA
                        if (pesada == 1) {
                            $("#peso_bascula").val(response);
                            peso = response;
                            $("#confirmar_primera_pesada").attr("onclick", "GuardarPrimeraPesada(" + peso + ");");
                        }
                        //SEGUNDA PESADA
                        else {
                            $("#peso_bascula_sp").val(response);
                            peso = response;
                            CalcularPesoTotal(peso);
                            var peso1 = $("#pesot_sp").val();
                            var total_litros = Math.ceil(peso1 / 1.02920);
                            $("#litros_sp").val(total_litros);
                        }
                    }
                    else {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'Favor de revisar el puerto de la bascula',
                        });
                    }
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'Favor de revisar la conexicon con la bascula',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'No se encontro el establo'
        });
    }
}
function CalcularPesoTotal(peso) {
    var peso1 = parseFloat($("#peso1_sp").val());
    var peso2 = peso;
    pesoT = Math.abs(peso1 - peso2);
    $("#pesot_sp").val(pesoT);
    $("#confirmar_segunda_pesada").attr("onclick", "GuardarSegundaPesada('" + peso + "','" + pesoT + "');");
}
//#endregion

//#region LIMPIEZA BASCULA
function LimpiarCamposFicha(pesada) {
    let elementos;
    if (pesada == 1) {
        elementos = $(".limpiar_sp");
        $(".borders").css("border-color", "");
        $("#confirmar_primera_pesada").attr("onclick", "AdvertenciaPesada(1);");

        $("#peso_bascula").val('');
    } else {
        elementos = $(".limpiar_sp");
    }

    elementos.each(function () {
        if ($(this).is('input:text') || $(this).is('textarea')) {
            $(this).val('');
        }
        else if ($(this).is('select')) {
            $(this).prop("selectedIndex", 0);
            $(this).trigger("change");
        }
        else if ($(this).is('input:checkbox')) {
            $(this).prop('checked', false);
        }
    });
}
//#endregion

//#region PRIMERA PESADA
function GuardarPrimeraPesada(primera_pesada) {
    //TIPO 1 - LECHE, TIPO 2 FORRAJE, TIPO 3 REGULAR PROVEEDOR, TIPO 4 REGULAR CLIENTE
    var tipo_ficha = $("#id_tipo_ficha").val();
    var peso_1 = primera_pesada;

    //VALIDAR PESO
    if (peso_1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario el peso para registrar la ficha'
        });
        return;
    }
    //VALIDAR PUERTO
    if (peso_1 == "No se pudo abrir el puerto") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de verificar el puerto',
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

    jsShowWindowLoad();

    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/GuardarPrimeraPesada",
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
                $("#confirmar_primera_pesada").attr("onclick", "AdvertenciaPesada(1);");
                ConsultarUltimaFicha();
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
//#endregion

//#region FICHAS PENDIENTES SEGUNDA PESADA
function ConsultarFichasPendientes() {
    $("#m_pendientes").modal("show");
    var id_establo = $("#id_establo_sp").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarFichasPendientes",
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
//#endregion

//#region SEGUNDA PESADA
function ConsultarInfoFicha(ficha_bascula) {
    var ficha = "";
    if (ficha_bascula == undefined || ficha_bascula == "") {
        ficha = $("#id_ficha_g_sp").val();
    }
    else {
        ficha = ficha_bascula;
    }

    if (ficha != "") {
        var id_establo = $("#id_establo_sp").val();
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/ConsultarInfoFicha",
            data: {
                ficha: ficha,
                id_establo: id_establo
            },
            success: function (response) {
                if (response != "[]") {
                    $("#m_pendientes").modal("hide");
                    var data = $.parseJSON(response);
                    var tipo_ficha = data[0].idtipoficha;
                    ConsultarFormatoFichaBascula(tipo_ficha, 2);


                    $("#tipo_ficha_pesada").text(tipo_ficha);

                    $("#id_tipo_ficha_sp").val(data[0].tipoficha);

                    $("#no_pozo_sp").val(data[0].no_pozo);


                    if (tipo_ficha == 1) {
                        $("#select_proveedor_sp").val(data[0].leche);

                        $("#placas_pipa_sp").val(data[0].placas_pipa);
                    }
                    else {
                        $("#select_proveedor_sp").val(data[0].regular);
                    }

                    $("#select_articulo_sp").val(data[0].nombre_articulo);

                    $("#tmov_sp").val(data[0].tmov);
                    $("#cvemov_sp").val(data[0].cvemo);

                    $("#folio_sp").val(data[0].folio);


                    $("#sucursal_sp").val(data[0].sucursal);
                    $("#destino_sp").val(data[0].destino);
                    $("#linea_trasp_sp").val(data[0].linea_trasp);

                    $("#conductor_sp").val(data[0].conductor);
                    $("#placas_sp").val(data[0].placas);
                    $("#maquilador_sp").val(data[0].maquilador);
                    $("#predio_cliente_sp").val(data[0].predio_cliente);

                    var agrupada = data[0].agrupada;
                    if (agrupada == true) { $("#agrupada").prop('checked', true) }
                    else { $("#agrupada").prop('checked', false) }

                    $("#tabla_p1_sp").val(data[0].tabla);
                    $("#variedad_p1_sp").val(data[0].variedad);
                    $("#corte_p1_sp").val(data[0].corte);
                    $("#pacas_p1_sp").val(data[0].pacas);
                    $("#ensilador_p1_sp").val(data[0].ensilador);

                    $("#obs_sp").val(data[0].observaciones);
                    $("#peso_origen_sp").val(data[0].peso_origen);
                    $("#mat_seca_sp").val(data[0].peso_materia_seca);
                    $("#peso1_sp").val(data[0].peso_1);

                    $("#id_ficha_g_sp").val(ficha);

                    AspectoSegundaPesadaBoton(1);

                    $("#fecha_pesada_2").val(data[0].fecha_registo);
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'No se encontró la ficha o ya se registro la segunda pesada',
                    });
                    $(".input_clear").val('');
                }

            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una ficha',
        });
        $("#id_ficha_g_seg").focus();
    }
}

function ConsultarFormatoFichaBascula(idtipoficha, pesada) {
    if (idtipoficha == undefined) { idtipoficha = $("#id_tipo_ficha").val(); pesada = 1; }
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarFormatoFichaBascula",
        data: {
            idtipoficha: idtipoficha,
            pesada: pesada
        },
        success: function (response) {
            if (pesada == 1) {
                $("#div_tipo_ficha_pp").html(response);
                $("#peso_bascula").val('');
                SelectProveedorBascula(); SelectArticuloBascula();
                ConsultarPozosEstablo();
                ConsultarClavesMovimientosEstabloSelect();
            }
            else {
                $("#div_tipo_ficha_sp").html(response);
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AspectoSegundaPesadaBoton(modo) {
    //CONSULTAR FICHA
    if (modo == 1) {
        //BOTONES BUSCAR FICHA PENDIENTES
        $("#btn_p2_pendientes").attr("onclick", "");
        $("#btn_p2_consultar").attr("onclick", "AspectoSegundaPesadaBoton(2);");
        //INPUT FICHA
        $("#btn_p2_pendientes").attr("disabled", true);
        $("#id_ficha_g_sp").attr("disabled", true);
        //ICON
        $("#icon_p2_consultar").removeClass("fa-search");
        $("#icon_p2_consultar").addClass("fa-remove");
        //COLOR BUTTON
        $("#btn_p2_consultar").removeClass("btn_beta_icon");
        $("#btn_p2_consultar").addClass("btn_beta_danger");
        //SELECT ESTABLO
        $("#id_establo_sp").attr("disabled", true);
        //BOTON OBTENER PESO
        $("#btn_segunda_pesada_peso").attr("disabled", false);
    }
    //CANCELAR CONSULTAR FICHA
    else {
        LimpiarCamposFicha(2);
        //BOTON OBTENER PESO
        $("#btn_segunda_pesada_peso").attr("disabled", true);
        //BOTONES BUSCAR FICHA PENDIENTES
        $("#btn_p2_pendientes").attr("onclick", "ConsultarFichasPendientes();");
        $("#btn_p2_consultar").attr("onclick", "ConsultarInfoFicha();");
        //INPUT FICHA
        $("#btn_p2_pendientes").attr("disabled", false);
        $("#id_ficha_g_sp").attr("disabled", false);
        //ICON
        $("#icon_p2_consultar").removeClass("fa-remove");
        $("#icon_p2_consultar").addClass("fa-search");
        //COLOR BUTTON
        $("#btn_p2_consultar").removeClass("btn_beta_danger");
        $("#btn_p2_consultar").addClass("btn_beta_icon");

        $("#div_tipo_ficha_sp").html('');

        $("#tipo_ficha_pesada").text('');
        //SELECT ESTABLO
        $("#id_establo_sp").attr("disabled", false);
    }
}

function GuardarSegundaPesada(segunda_pesada, peso_total) {
    var ficha = $("#id_ficha_g_sp").val();
    var peso_2 = segunda_pesada;
    var tipo_ficha = 0;
    if (ficha != "") {
        if (peso_2 != "" && peso_total != "") {
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
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de asegurarse de que el PESO 2 tenga un valor',
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de buscar la ficha',
        });
    }
}
//#endregion

//#region IMPRESION DE FICHA
function ImpresionFichaPDF(ficha) {
    var establo = $("#id_establo_sp").val();
    jsShowWindowLoad();
    var newTab = window.open('', '_blank');

    $.ajax({
        type: "POST",
        url: "../BASCULA/ImpresionFichaPDF",
        data: {
            id_bascula_ficha: ficha,
            id_establo: establo
        },
        success: function (response) {
            if (response != "") {
                jsRemoveWindowLoad();
                newTab.document.open();
                newTab.document.write(response);
                newTab.document.close();
                newTab.onload = function () {
                    newTab.print();
                    newTab.close();
                };
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se encontro la ficha asociada al numero ingresado',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ReImpresionFichaPDF(modo) {
    if (modo == 1) {
        $("#m_reimpresion").modal("show");
    }
    else {
        var ficha = $("#reimp_ficha").val();
        var establo = $("#id_establo_reimp").val();
        if (ficha != undefined && ficha != "") {
            jsShowWindowLoad();
            $.ajax({
                type: "POST",
                url: "../BASCULA/ImpresionFichaPDF",
                data: {
                    id_bascula_ficha: ficha,
                    id_establo: establo
                },
                success: function (response) {
                    if (response != "") {
                        jsRemoveWindowLoad();
                        var newTab = window.open('', '_blank');
                        newTab.document.open();
                        newTab.document.write(response);
                        newTab.document.close();
                        newTab.onload = function () {
                            newTab.print();
                            newTab.close();
                        };
                    }
                    else {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'No se encontro informacion con la ficha ingresada',
                        });
                        jsRemoveWindowLoad();
                    }
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                }
            });
            jsRemoveWindowLoad();
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de ingresar un numero de ficha',
            });
        }
    }

}
//#endregion

//#region BASCULA CATALOGOS

//SELECT
function SelectProveedorBascula() {
    var tipo_ficha = $("#id_tipo_ficha").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarProveedoresTipoBasculaSelect",
        data: {
            tipo_ficha: tipo_ficha
        },
        success: function (response) {
            $("#select_proveedor").empty();
            $("#select_proveedor").append('<option value="0">Selecciona un proveedor</option>');
            $("#select_proveedor").append(response);
            $("#select_proveedor").val("0").change();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function SelectArticuloBascula() {
    var tipo_ficha = $("#id_tipo_ficha").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarArticulosBasculaTiposSelect",
        data: {
            tipo_ficha: tipo_ficha
        },
        success: function (response) {
            $("#select_articulo").empty();
            $("#select_articulo").append('<option value="0">Selecciona un articulo</option>');
            $("#select_articulo").append(response);
            $("#select_articulo").val("0").change();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

//ADVERTENCIAS PRIMERA Y SEGUNDA PESADA
function AdvertenciaPesada(pesada) {
    if (pesada == 1) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar la primera pesada',
        });
    }
    else {
        if ($("#btn_p2_consultar").hasClass("btn_beta_danger")) {
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de ingresar la segunda pesada',
            });
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de ingresar la ficha para la segunda pesada',
            });
        }

    }
}

function ConsultarUltimaFicha() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarUltimaFicha",
        data: {
        },
        success: function (response) {
            if (response != 0) {
                $("#id_ficha_g").val(response);
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

function ConsultarPozosEstablo() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarPozosSelect",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#no_pozo").html(response);
            $("#no_pozo_editar").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarTipoFichasEstabloSelect() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarTipoFichasEstabloSelect",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#id_tipo_ficha").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarClavesMovimientosEstabloSelect() {
    var id_ficha_tipo = $("#id_tipo_ficha").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarClavesMovimientosEstabloSelect",
        data: { id_ficha_tipo: id_ficha_tipo },
        success: function (response) {
            $("#cvemov").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultaGeneralSelectEstablo() {
    ConsultarTipoFichasEstabloSelect();
    ConsultarPozosEstablo();
    ConsultarClavesMovimientosEstabloSelect();
}

//#endregion

//#region ENVIO LECHE
function ModalAgrupacionEnvioLeche() {
    $("#m_envio_leche").modal("show");
    ConsultarEstabloLecheUsuarioSelect();
    EnvioLecheFichasTable();
    IndicadorTipoEnvioLeche();
}

function EnvioLecheFichasTable() {
    $("#btn_agrupar_ficha_leche").css("display", "none");
    $("#div_agrupacion_leche_filtro").css("display", "inline");
    var id_establo = $("#id_establo_fichas_leche").val();
    $("#div_agrupacion_ficha").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/EnvioLecheFichasTable",
        data: { id_establo: id_establo },
        success: function (response) {
            try {
                $("#datatable_envio_fichas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_envio_fichas").html(response);
            $('#datatable_envio_fichas').DataTable({
                keys: false,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                select: true,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers',
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function IndicadorTipoEnvioLeche() {
    var id_envio_leche_d = $(".check_envio:checked").map(function () {
        return $(this).attr("id").split('_')[2];
    }).get();

    switch (id_envio_leche_d.length) {
        case 0:
            $("#btn_agrupar_ficha_leche").css("display", "none");
            $("#btn_agrupar_ficha_leche").text("AGRUPAR ENVIO");
            break;
        case 1:
            $("#btn_agrupar_ficha_leche").css("display", "inline");
            $("#btn_agrupar_ficha_leche").text("ENVIO SENCILLO");
            break;
        case 2:
            $("#btn_agrupar_ficha_leche").text("ENVIO FULL");
            $("#btn_agrupar_ficha_leche").css("display", "inline");
            break;
        default:
            $("#btn_agrupar_ficha_leche").text("FUERA DE RANGO");
            $("#btn_agrupar_ficha_leche").css("display", "none");
    }
}


function AgrupacionFichasLeche() {
    var count = 0;
    var id_envio_leche_d = [];
    var clientes = [];
    var id_cliente = 0;
    $(".check_envio").each(function () {
        if ($(this).is(':checked')) {
            var id_envio = $(this).attr("id").split('_')[2];
            id_cliente = $(this).attr("id").split('_')[3];
            clientes[count] = $(this).attr("id").split('_')[3];
            id_envio_leche_d[count] = id_envio
            count++;
        }

    });
    if (id_envio_leche_d.length > 0 && id_envio_leche_d.length <= 2) {
        if (id_envio_leche_d.length == 2) {
            if (clientes[0] != clientes[1]) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'El cliente debe ser el mismo'
                });
                return;
            }
        }


        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/AgrupacionFichasLeche",
            data: {
                id_envio_leche_d: id_envio_leche_d
            },
            success: function (response) {
                $("#div_agrupacion_leche_filtro").css("display", "none");
                $("#div_envio_fichas").html('');
                $("#div_agrupacion_ficha").html(response);

                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CATALOGOS/ConsultarDestinosClienteLecheSelect",
                    data: {
                        id_cliente: id_cliente
                    },
                    success: function (response) {
                        $("#select_destino_agrupacion").empty();
                        $("#select_destino_agrupacion").append(response);
                        $('#select_destino_agrupacion').prop('selectedIndex', 0);
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
        jsRemoveWindowLoad();
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario seleccionar una ficha.'
        });
    }
}

function ConfirmarAgrupacionFichasLeche() {
    var count = 0;
    var id_envio_leche_d = [];
    var ficha_leche_g = {};

    $(".id_ficha_d_leche").each(function () {
        id_envio_leche_d[count] = $(this).text();
        count++;
    });
    var destino = $("#select_destino_agrupacion").val();
    var remision = $("#remision_agrupar_ficha").val();
    var folio = $("#folio_agrupar_ficha").val();
    if (destino == undefined || destino.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar un destino'
        });
        return;
    }
    if (folio.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'El folio es obligatorio'
        });
        return;
    }
    if (remision == undefined || remision.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'El n° de remision es obligatorio'
        });
        return;
    }



    var id_establo = $("#id_establo_fichas_leche").val();

    var operador = $("#chofer_ficha_leche").text();
    var placas = $("#placas_ficha_leche").text();
    var cliente = $("#id_cliente_ficha_leche").text();
    var producto = $("#id_producto_ficha_leche").text();
    var linea_transportista = $("#id_linea_ficha_leche").text();
    var kilos_totales = $("#kilos_ficha_leche_agrupa").text().replace(/,/g, "");
    var litros_totales = $("#litros_ficha_leche_agrupa").text().replace(/,/g, "");
    var densidad_total = $("#densidad_ficha_leche_agrupa").text();
    var fecha = $("#fecha_ficha_leche").text();




    ficha_leche_g.id_establo_envio = id_establo;
    ficha_leche_g.folio = folio;
    ficha_leche_g.remision_cliente = remision;
    ficha_leche_g.id_destino_envio_leche = destino;
    ficha_leche_g.operador = operador;
    ficha_leche_g.placas = placas;
    ficha_leche_g.id_envio_leche_cliente_ms = cliente;
    ficha_leche_g.id_producto_envio = producto;
    ficha_leche_g.id_linea_transportista = linea_transportista;
    ficha_leche_g.kilos_totales = kilos_totales;
    ficha_leche_g.litros_totales = litros_totales;
    ficha_leche_g.densidad_total = densidad_total;
    ficha_leche_g.fecha_envio = fecha;

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConfirmarAgrupacionFichasLeche",
        data: {
            id_envio_leche_d: id_envio_leche_d,
            ficha_leche_g: ficha_leche_g,
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se agrupo correctamente la ficha de leche'
                });
                EnvioLecheFichasTable();
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al agruppar la ficha de leche'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}
//#endregion