$('#buscar_fecha_final').val(today);
$('#confirmacion_fecha2').val(today);
$('#fecha2_lote_leche').val(today);

function MostrarFormularioEnvioLeche() {
    $("#m_confirmar_envios_leche").modal("show");
    $("#radio_leche_1").prop("checked", true);
    TipoConfirmacionEnvioLeche(1);
    LimpiarCamposEnvioLeche();
    $("#btn_leche_1").css("display", "block");
    $("#btn_leche_2").css("display", "block");
    $("#leche_accion").css("display", "block");
    $("#btn_leche_accion").attr("onclick", "PreConfirmacionEnvioLeche();");
    $(".leche_mostrar").removeAttr("disabled");
}

function PreConfirmacionEnvioLeche() {
    var radio_tipo_tanque_id = $(".radio_tipo_tanque:checked").attr('id').split('_')[2];
    var confirmacion1 = $("#confirmacion_ficha1").text();
    var confirmacion2 = $("#confirmacion_ficha2").text();

    if (confirmacion1 == 1) {
        jsShowWindowLoad();

        //#region HEADER PARTE 1
        var id_establo_envio = $("#id_establo_envio").val();
        var id_destino_envio_leche = $("#id_destino_leche").val();
        var id_producto_envio = $("#id_producto_leche").val();

        var litros_totales = $("#litros_totales_leche").val();
        var folio = $("#folio_total_leche").val();
        var remision_cliente = $("#remision_total_leche").val();

        var id_linea_transportista = $("#linea_trasp").val();
        var placas = $("#placas_leche").val();
        var operador = $("#operador_leche").val();
        var kilos_totales = $("#kilos_totales_leche").val();
        var densidad_total = $("#densidad_leche").val();
        var id_proveedor = $("#cliente1_ficha").val();


        var fecha_envio = $("#fecha1_leche_completa").text();
        //#endregion

        var c_ficha_1 = {};
        var c_ficha_2 = {};
        var c_calidad_1 = {};
        var c_calidad_2 = {};
        var c_header = {};

        //#region HEADER PARTE 2
        c_header.id_establo_envio = id_establo_envio;



        c_header.fecha_envio = fecha_envio;


        c_header.id_destino_envio_leche = id_destino_envio_leche;
        c_header.id_producto_envio = id_producto_envio;
        c_header.litros_totales = litros_totales;
        c_header.folio = folio;
        c_header.remision_cliente = remision_cliente;
        c_header.id_linea_transportista = id_linea_transportista;
        c_header.placas = placas;
        c_header.operador = operador;
        c_header.kilos_totales = kilos_totales;
        c_header.densidad_total = densidad_total;
        c_header.id_tipo_tanque_pipa = radio_tipo_tanque_id;
        c_header.id_bascula_proveedor = id_proveedor;
        c_header.id_envio_leche_status = 1;
        //#endregion

        //#region FICHA 1
        var id_ficha_bascula1 = $("#ficha1_leche").val();
        var folio_ficha1 = $("#folio1_leche").val();
        var tanque1 = $("#tanque1_leche").val();
        var kilos_ficha1 = $("#peso1_leche").val();
        var litros_ficha1 = $("#litros1_leche").val();
        var sello1 = $("#sello1_leche").val();
        var temperatura1 = $("#temperatura1_leche").val();
        //#endregion

        //#region CALIDAD FICHA 1
        var muestra_calidad1 = " ";
        var sala1 = " ";

        var antibiotico1 = -1;
        var grasa1 = 0;

        var proteina1 = 0;
        var solidos_no_grasos1 = 0;
        var solidos_totales1 = 0;

        var lactosa1 = 0;
        var caseina1 = 0;
        var acidez1 = 0;

        var crioscopia1 = 0;
        var urea1 = 0;
        var ccs1 = 0;

        var betalactamicos1 = -1;
        var tetraciclina1 = -1;
        var aflatoxinas1 = -1;

        var sulfasimas1 = -1;
        var alcohol_751 = -1;

        c_ficha_1.id_ficha_bascula = id_ficha_bascula1;
        c_ficha_1.folio_ficha = folio_ficha1;
        c_ficha_1.tanque = tanque1 = tanque1;
        c_ficha_1.kilos_ficha = kilos_ficha1;
        c_ficha_1.litros_ficha = litros_ficha1;
        c_ficha_1.sello = sello1;

        c_calidad_1.temperatura = temperatura1;
        c_calidad_1.muestra_calidad = muestra_calidad1;
        c_calidad_1.sala = sala1;
        c_calidad_1.antibiotico = antibiotico1;
        c_calidad_1.grasa = grasa1;
        c_calidad_1.proteina = proteina1;
        c_calidad_1.solidos_no_grasos = solidos_no_grasos1;
        c_calidad_1.solidos_totales = solidos_totales1;
        c_calidad_1.lactosa = lactosa1;
        c_calidad_1.caseina = caseina1;
        c_calidad_1.acidez = acidez1;
        c_calidad_1.crioscopia = crioscopia1;
        c_calidad_1.urea = urea1;
        c_calidad_1.ccs = ccs1;
        c_calidad_1.betalactamicos = betalactamicos1;
        c_calidad_1.tetraciclina = tetraciclina1;
        c_calidad_1.aflatoxinas = aflatoxinas1;
        c_calidad_1.sulfasimas = sulfasimas1;
        c_calidad_1.alcohol_75 = alcohol_751;
        //#endregion

        if (confirmacion2 == 1) {
            //#region FICHA 2
            var id_ficha_bascula2 = $("#ficha2_leche").val();
            var folio_ficha2 = $("#folio2_leche").val();
            var tanque2 = $("#tanque2_leche").val();
            var kilos_ficha2 = $("#peso2_leche").val();
            var litros_ficha2 = $("#litros2_leche").val();
            var sello2 = $("#sello2_leche").val();
            var temperatura2 = $("#temperatura2_leche").val();
            //CALIDAD
            var muestra_calidad2 = " ";
            var sala2 = " ";

            var antibiotico2 = -1;
            var grasa2 = 0;

            var proteina2 = 0;
            var solidos_no_grasos2 = 0;
            var solidos_totales2 = 0;

            var lactosa2 = 0;
            var caseina2 = 0;
            var acidez2 = 0;

            var crioscopia2 = 0;
            var urea2 = 0;
            var ccs2 = 0;

            var betalactamicos2 = -1;
            var tetraciclina2 = -1;
            var aflatoxinas2 = -1;

            var sulfasimas2 = -1;
            var alcohol_752 = -1;

            c_ficha_2.id_ficha_bascula = id_ficha_bascula2;
            c_ficha_2.folio_ficha = folio_ficha2;
            c_ficha_2.tanque = tanque1 = tanque2;
            c_ficha_2.kilos_ficha = kilos_ficha2;
            c_ficha_2.litros_ficha = litros_ficha2;
            c_ficha_2.sello = sello2;

            c_calidad_2.temperatura = temperatura2;
            c_calidad_2.muestra_calidad = muestra_calidad2;
            c_calidad_2.sala = sala2;
            c_calidad_2.antibiotico = antibiotico2;
            c_calidad_2.grasa = grasa2;
            c_calidad_2.proteina = proteina2;
            c_calidad_2.solidos_no_grasos = solidos_no_grasos2;
            c_calidad_2.solidos_totales = solidos_totales2;
            c_calidad_2.lactosa = lactosa2;
            c_calidad_2.caseina = caseina2;
            c_calidad_2.acidez = acidez2;
            c_calidad_2.crioscopia = crioscopia2;
            c_calidad_2.urea = urea2;
            c_calidad_2.ccs = ccs2;
            c_calidad_2.betalactamicos = betalactamicos2;
            c_calidad_2.tetraciclina = tetraciclina2;
            c_calidad_2.aflatoxinas = aflatoxinas2;
            c_calidad_2.sulfasimas = sulfasimas2;
            c_calidad_2.alcohol_75 = alcohol_752;
            //#endregion
        }
        $.ajax({
            type: "POST",
            url: "../ENVIOLECHE/PreConfirmacionEnvioLeche",
            data: {
                c_header: c_header,
                c_ficha_1: c_ficha_1,
                c_ficha_2: c_ficha_2,
                c_calidad_1: c_calidad_1,
                c_calidad_2: c_calidad_2
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 1) {
                    $("#m_confirmar_envios_leche").modal("hide");
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se registro correctamente el envio de leche',
                    });
                    LimpiarCamposEnvioLeche();
                    ConsultarEnviosDeLecheTable();
                }
                else {
                    iziToast.error({
                        title: 'Aviso',
                        message: 'Ocurrio un error al registrar el envio de leche',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una ficha',
        });
    }
}

function ConfirmarCalidadLeche(id_envio_leche, id_ficha_calidad_1, id_ficha_calidad_2) {
    jsShowWindowLoad();
    var c_calidad_1 = {};
    var c_calidad_2 = {};


    //FICHA 1
    var temperatura1 = $("#temperatura1_leche").val();
    var muestra_calidad1 = $("#muestra1_leche").val();
    var sala1 = $("#sala1_leche").val();

    var antibiotico1 = $("#antibiotico1_leche").val();
    var grasa1 = $("#grasa1_leche").val();

    var proteina1 = $("#proteina1_leche").val();
    var solidos_no_grasos1 = $("#solidos_grasos1").val();
    var solidos_totales1 = $("#solidos_totales1").val();

    var lactosa1 = $("#lactosa1_leche").val();
    var caseina1 = $("#caseina1_leche").val();
    var acidez1 = $("#acidez1_leche").val();

    var crioscopia1 = $("#crioscopia1_leche").val();
    var urea1 = $("#urea1_leche").val();
    var ccs1 = $("#css1_leche").val();

    var betalactamicos1 = $("#betalactamicos1_leche").val();
    var tetraciclina1 = $("#tetraciclina1_leche").val();
    var aflatoxinas1 = $("#aflatoxinas1_leche").val();

    var sulfasimas1 = $("#sulfasimas1_leche").val();
    var alcohol_751 = $("#alcohol1_leche").val();

    c_calidad_1.temperatura = temperatura1;
    c_calidad_1.id_envio_leche_d_ficha = id_ficha_calidad_1;
    c_calidad_1.muestra_calidad = muestra_calidad1;
    c_calidad_1.sala = sala1;
    c_calidad_1.antibiotico = antibiotico1;
    c_calidad_1.grasa = grasa1;
    c_calidad_1.proteina = proteina1;
    c_calidad_1.solidos_no_grasos = solidos_no_grasos1;
    c_calidad_1.solidos_totales = solidos_totales1;
    c_calidad_1.lactosa = lactosa1;
    c_calidad_1.caseina = caseina1;
    c_calidad_1.acidez = acidez1;
    c_calidad_1.crioscopia = crioscopia1;
    c_calidad_1.urea = urea1;
    c_calidad_1.ccs = ccs1;
    c_calidad_1.betalactamicos = betalactamicos1;
    c_calidad_1.tetraciclina = tetraciclina1;
    c_calidad_1.aflatoxinas = aflatoxinas1;
    c_calidad_1.sulfasimas = sulfasimas1;
    c_calidad_1.alcohol_75 = alcohol_751;
    c_calidad_1.activo = true;

    //FICHA 2
    if (id_ficha_calidad_2 != undefined && id_ficha_calidad_2 != "") {
        var temperatura2 = $("#temperatura2_leche").val();
        var muestra_calidad2 = $("#muestra2_leche").val();
        var sala2 = $("#sala2_leche").val();

        var antibiotico2 = $("#antibiotico2_leche").val();
        var grasa2 = $("#grasa2_leche").val();

        var proteina2 = $("#proteina2_leche").val();
        var solidos_no_grasos2 = $("#solidos_grasos2").val();
        var solidos_totales2 = $("#solidos_totales2").val();

        var lactosa2 = $("#lactosa2_leche").val();
        var caseina2 = $("#caseina2_leche").val();
        var acidez2 = $("#acidez2_leche").val();

        var crioscopia2 = $("#crioscopia2_leche").val();
        var urea2 = $("#urea2_leche").val();
        var ccs2 = $("#css2_leche").val();

        var betalactamicos2 = $("#betalactamicos2_leche").val();
        var tetraciclina2 = $("#tetraciclina2_leche").val();
        var aflatoxinas2 = $("#aflatoxinas2_leche").val();

        var sulfasimas2 = $("#sulfasimas2_leche").val();
        var alcohol_752 = $("#alcohol2_leche").val();

        c_calidad_2.id_envio_leche_d_ficha = id_ficha_calidad_2;
        c_calidad_2.temperatura = temperatura2;
        c_calidad_2.muestra_calidad = muestra_calidad2;
        c_calidad_2.sala = sala2;
        c_calidad_2.antibiotico = antibiotico2;
        c_calidad_2.grasa = grasa2;
        c_calidad_2.proteina = proteina2;
        c_calidad_2.solidos_no_grasos = solidos_no_grasos2;
        c_calidad_2.solidos_totales = solidos_totales2;
        c_calidad_2.lactosa = lactosa2;
        c_calidad_2.caseina = caseina2;
        c_calidad_2.acidez = acidez2;
        c_calidad_2.crioscopia = crioscopia2;
        c_calidad_2.urea = urea2;
        c_calidad_2.ccs = ccs2;
        c_calidad_2.betalactamicos = betalactamicos2;
        c_calidad_2.tetraciclina = tetraciclina2;
        c_calidad_2.aflatoxinas = aflatoxinas2;
        c_calidad_2.sulfasimas = sulfasimas2;
        c_calidad_2.alcohol_75 = alcohol_752;
        c_calidad_2.activo = true;
    }
    else {
        c_calidad_2.id_envio_leche_d_ficha = 0;
    }
    $.ajax({
        type: "POST",
        url: "../ENVIOLECHE/ConfirmarCalidadLeche",
        data: {
            id_envio_leche: id_envio_leche,
            c_calidad_1: c_calidad_1,
            c_calidad_2: c_calidad_2
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == 0) {
                iziToast.error({
                    title: 'Aviso',
                    message: 'Ocurrio un error al guardar la informacion',
                });
            }
            if (response == -1) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se logró guardar la informacion de calidad',
                });
            }
            if (response == 1) {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se guardo correctamente la informacion',
                });
                $("#m_confirmar_envios_leche").modal("hide");
                LimpiarCamposEnvioLeche();
                ConsultarEnviosDeLecheTable();
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ValidarInformacionFichaLeche(ficha) {
    var id_ficha = "";
    var destino = $("#id_destino_leche").val();
    var establo = $("#id_establo_envio").val();
    //FICHA 1
    if (ficha == 1) {
        id_ficha = $("#ficha1_leche").val().trim();
    }
    //FICHA 2
    if (ficha == 2) {
        id_ficha = $("#ficha2_leche").val().trim();
    }
    return new Promise(function (resolve, reject) {
        if (id_ficha == "" || id_ficha == undefined) {
            resolve(false);
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de ingresar una ficha valida.',
            });
            if (ficha == 1) {
                $("#ficha1_leche").css("border-color", "red");
            }
            else if (ficha == 2) {
                $("#ficha2_leche").css("border-color", "red");
            }
        }
        else {
            $.ajax({
                type: "POST",
                url: "../ENVIOLECHE/ValidarInformacionFichaLeche",
                data: {
                    ficha_bascula: id_ficha,
                    destino: destino,
                    establo: establo,
                },
                success: function (response) {
                    if (response == 1) {
                        resolve(true);
                    }
                    else if (response == -1) {
                        iziToast.error({
                            title: 'Aviso',
                            message: 'Ocurrio un error al validar la informacion',
                        });
                    }
                    else if (response == -2) {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'No se encontro la ficha',
                        });
                    }
                    else if (response == -3) {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'La fecha no coincide con la ficha',
                        });
                    }
                    else if (response == -4) {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'El establo no coincide con la ficha',
                        });
                    }
                    else if (response == -5) {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'El proveedor de la ficha no tiene asociado el destino seleccionado',
                        });
                    }
                    else if (response == -6) {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'Ya se registro la ficha ingresada',
                        });
                    }
                    resolve(false);
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    reject(error);  // Error en la llamada AJAX
                }
            });
        }

    });
}

function AsignarInformacionFichaBascula(ficha) {

    var id_ficha = "";
    //FICHA 1
    if (ficha == 1) {
        id_ficha = $("#ficha1_leche").val().trim();
    }
    //FICHA 2
    if (ficha == 2) {
        id_ficha = $("#ficha2_leche").val().trim();
    }
    if (id_ficha != "" && id_ficha != undefined) {

        if ($("#ficha2_leche").val().trim() != $("#ficha1_leche").val().trim()) {
            ValidarInformacionFichaLeche(ficha).then(function (isValid) {
                if (isValid) {
                    $.ajax({
                        type: "POST",
                        url: "../ENVIOLECHE/AsignarInformacionFichaBascula",
                        data: {
                            ficha_bascula: id_ficha
                        },
                        success: function (response) {
                            if (response != "[]") {
                                var data = $.parseJSON(response);
                                var hora = data[0].fecha_segunda_pesada.split('T')[1];
                                //FICHA 1
                                if (ficha == 1) {

                                    $("#cliente1_ficha").val(data[0].id_bascula_proveedor);
                                    $("#folio1_leche").val(data[0].folio);
                                    $("#tanque1_leche").val(data[0].observaciones);
                                    $("#peso1_leche").val(data[0].peso_t);

                                    $("#litros1_leche").val((data[0].peso_t / 1.0292).toFixed(3));
                                    $("#fecha1_leche").val(hora);
                                    $("#fecha1_leche_completa").text(data[0].fecha_segunda_pesada);
                                    $("#ficha1_leche").css("border-color", "");

                                    $("#linea_trasp").val(data[0].id_linea_transportista);
                                    $("#placas_leche").val(data[0].placas);
                                    $("#operador_leche").val(data[0].chofer);
                                    $("#densidad_leche").val('1.02920');


                                    CalcularKilosLitrosTotalesLeche();
                                    $("#btn_leche_1").attr("onclick", "CancelarFichaEnvioLeche(1);");
                                    $("#ficha1_leche").attr("disabled", "true");
                                    $("#icon_leche_1").removeClass("fa-search");
                                    $("#icon_leche_1").addClass("fa-remove");
                                    $("#btn_leche_1").removeClass("btn_beta_icon");
                                    $("#btn_leche_1").addClass("btn_beta_danger");

                                    $("#confirmacion_ficha1").text('1');


                                    $("#id_destino_leche").attr("disabled", "true");
                                    $("#id_establo_envio").attr("disabled", "true");
                                    $("#fecha_leche_inicial_registro").attr("disabled", "true");
                                }
                                //FICHA 2
                                else if (ficha == 2) {
                                    $("#cliente2_ficha").val(data[0].id_bascula_proveedor);
                                    $("#ficha2_leche").css("border-color", "");
                                    $("#folio2_leche").val(data[0].folio);
                                    $("#tanque2_leche").val(data[0].observaciones);
                                    $("#litros2_leche").val((data[0].peso_t / 1.0292).toFixed(3));
                                    $("#peso2_leche").val(data[0].peso_t);
                                    $("#fecha2_leche").val(hora);
                                    $("#fecha2_leche_completa").text(data[0].fecha_segunda_pesada);

                                    CalcularKilosLitrosTotalesLeche();
                                    $("#btn_leche_2").attr("onclick", "CancelarFichaEnvioLeche(2);");
                                    $("#ficha2_leche").attr("disabled", "true");
                                    $("#icon_leche_2").removeClass("fa-search");
                                    $("#icon_leche_2").addClass("fa-remove");
                                    $("#btn_leche_2").removeClass("btn_beta_icon");
                                    $("#btn_leche_2").addClass("btn_beta_danger");

                                    $("#confirmacion_ficha2").text('1');
                                }
                            }
                            else {
                                iziToast.warning({
                                    title: '',
                                    message: 'No se encontró informacion',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }
            }).catch(function (error) {
                console.error('Error en la validación: ', error);
            });
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'La ficha no puede ser la misma',
            });
        }
    }
    else {
        if (ficha == 1) {
            $("#ficha1_leche").css("border-color", "red");
        }
        else if (ficha == 2) {
            $("#ficha2_leche").css("border-color", "red");
        }
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una ficha valida.',
        });
    }
}

function CancelarFichaEnvioLeche(ficha) {
    if (ficha == 1) {
        $("#btn_leche_1").attr("onclick", "AsignarInformacionFichaBascula(1);");
        $("#ficha1_leche").removeAttr("disabled");
        $("#icon_leche_1").removeClass("fa-remove");
        $("#icon_leche_1").addClass("fa-search");
        $("#btn_leche_1").removeClass("btn_beta_danger");
        $("#btn_leche_1").addClass("btn_beta_icon");


        $("#ficha1_leche").val('');
        $("#folio1_leche").val('');
        $("#peso1_leche").val('');
        $("#litros1_leche").val('');
        $("#fecha1_leche").val('');
        $("#tanque1_leche").val('');
        $("#sello1_leche").val('');

        $("#confirmacion_ficha1").text('0');




        $("#linea_trasp").val(-1);
        $("#placas_leche").val('');
        $("#operador_leche").val('');
        $("#densidad_leche").val('');
        $("#kilos_totales_leche").val('');
        $("#litros_totales_leche").val('');



        $("#id_establo_envio").removeAttr("disabled");
        $("#fecha_leche_inicial_registro").removeAttr("disabled");
        $("#id_destino_leche").removeAttr("disabled");

    }
    else {
        $("#btn_leche_2").attr("onclick", "AsignarInformacionFichaBascula(2);");
        $("#ficha2_leche").removeAttr("disabled");
        $("#icon_leche_2").removeClass("fa-remove");
        $("#icon_leche_2").addClass("fa-search");
        $("#btn_leche_2").removeClass("btn_beta_danger");
        $("#btn_leche_2").addClass("btn_beta_icon");


        $("#ficha2_leche").val('');
        $("#folio2_leche").val('');
        $("#peso2_leche").val('');
        $("#litros2_leche").val('');
        $("#fecha2_leche").val('');
        $("#tanque2_leche").val('');
        $("#sello2_leche").val('');

        $("#confirmacion_ficha2").text('0');
    }
}

function CalcularKilosLitrosTotalesLeche() {
    var leche = $("#litros2_leche").val();
    if (leche == "") {
        $("#kilos_totales_leche").val(parseFloat($("#peso1_leche").val()).toFixed(3));
        $("#litros_totales_leche").val(parseFloat($("#litros1_leche").val()).toFixed(3));
    } else {
        var lecheT = parseFloat($("#litros1_leche").val()) + parseFloat($("#litros2_leche").val());
        var pesoT = parseFloat($("#peso1_leche").val()) + parseFloat($("#peso2_leche").val());

        $("#kilos_totales_leche").val(pesoT.toFixed(3));
        $("#litros_totales_leche").val(lecheT.toFixed(3));
    }

}

function TipoConfirmacionEnvioLeche(envio) {
    if (envio == 1) {
        $(".ficha_leche_2").hide();
        $(".ficha_leche_1").show();
        $("#radio_leche_1").prop("checked", true);
        $("#cliente2_ficha").val(-1);
        CancelarFichaEnvioLeche(2);
        $(".limpiar_input2").val('');
        $(".limpiar_select2").val(-1);
    }
    if (envio == 2) {
        $(".ficha_leche_2").show();
        $(".ficha_leche_1").show();
        $("#radio_leche_2").prop("checked", true);
    }
}

function LimpiarCamposEnvioLeche() {
    $(".limpiar_input").val('');
    $(".limpiar_input2").val('');
    $(".limpiar_select").val(-1);
    $(".limpiar_select2").val(-1);
    $("#confirmacion_ficha1").text('0');
    $("#confirmacion_ficha2").text('0');
    $("#cliente1_ficha").val(-1);
    $("#cliente2_ficha").val(-1);
    CancelarFichaEnvioLeche(1);
    CancelarFichaEnvioLeche(2);
}

function ConsultarEnviosDeLecheTable() {
    var fecha_inicio = $("#buscar_fecha_inicial").val();
    var fecha_fin = $("#buscar_fecha_final").val();
    var establo = $("#buscar_establo").val();
    var folio = $("#buscar_folio").val();
    var cliente = $("#buscar_cliente").val();
    var destino = $("#buscar_destino").val();
    if (fecha_inicio != undefined && fecha_inicio != "" && fecha_fin != undefined && fecha_fin != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ConsultarEnviosDeLecheTable",
            data: {
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                establo: establo,
                folio: folio,
                cliente: cliente,
                destino: destino
            },
            success: function (response) {
                $("#div_envios_leche").html(response);
                $('#tabla_envio_leche').DataTable({
                    ordering: false,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: false,
                    select: true,
                    keys: false,
                    pageLength: 20
                });
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
            message: 'Favor de seleccionar un rango de fechas',
        });
    }
}

function AsignarConfirmacionCalidadLeche(id_envio_leche, id_ficha_calidad_1, id_ficha_calidad_2) {
    ObtenerInformacionEnvioLeche(id_envio_leche);
    $("#btn_leche_accion").attr("onclick", "ConfirmarCalidadLeche(" + id_envio_leche + "," + id_ficha_calidad_1 + "," + id_ficha_calidad_2 + ")");
}

function ObtenerInformacionEnvioLeche(id_envio_leche) {
    $.ajax({
        type: "POST",
        url: "../ENVIOLECHE/ObtenerInformacionEnvioLeche",
        data: {
            id_envio_leche: id_envio_leche
        },
        success: function (response) {
            if (response != "[]") {
                $("#m_confirmar_envios_leche").modal("show");
                var data = $.parseJSON(response);
                if (data.length > 1) {
                    TipoConfirmacionEnvioLeche(2);

                    //#region FICHA 2
                    $("#ficha2_leche").val(data[1].id_ficha_bascula);
                    $("#folio2_leche").val(data[1].folio_ficha);
                    $("#tanque2_leche").val(data[1].tanque);
                    $("#peso2_leche").val(data[1].kilos_ficha);
                    $("#litros2_leche").val(data[1].litros_ficha);
                    $("#sello2_leche").val(data[1].sello);
                    //#endregion

                    $("#muestra2_leche").val(data[1].muestra_calidad);
                    $("#temperatura2_leche").val(data[1].temperatura);
                    $("#sala2_leche").val(data[1].sala);
                    $("#proteina2_leche").val(data[1].proteina);
                    $("#lactosa2_leche").val(data[1].lactosa);
                    $("#crioscopia2_leche").val(data[1].crioscopia);
                    $("#solidos_grasos2").val(data[1].solidos_no_grasos);
                    $("#caseina2_leche").val(data[1].caseina);
                    $("#urea2_leche").val(data[1].urea);
                    $("#grasa2_leche").val(data[1].grasa);
                    $("#solidos_totales2").val(data[1].solidos_totales);
                    $("#acidez2_leche").val(data[1].acidez);
                    $("#css2_leche").val(data[1].ccs);
                    $("#antibiotico2_leche").val(data[1].antibiotico);
                    $("#betalactamicos2_leche").val(data[1].betalactamicos);
                    $("#sulfasimas2_leche").val(data[1].sulfasimas);
                    $("#tetraciclina2_leche").val(data[1].tetraciclina);
                    $("#alcohol2_leche").val(data[1].alcohol_75);
                    $("#aflatoxinas2_leche").val(data[1].aflatoxinas);

                    /* $("#fecha2_leche").val(hora);*/
                    $("#fecha2_leche").val(data[1].fecha_segunda_pesada);
                }
                else {
                    TipoConfirmacionEnvioLeche(1);
                }
                //#region HEADER
                $("#id_establo_envio").val(data[0].id_establo_envio);
                $("#fecha_leche_inicial_registro").val(data[0].fecha_envio);
                $("#id_destino_leche").val(data[0].id_destino_envio_leche);
                $("#id_producto_leche").val(data[0].id_producto_envio);
                $("#folio_total_leche").val(data[0].folio);
                $("#remision_total_leche").val(data[0].remision_cliente);
                $("#operador_leche").val(data[0].operador);
                $("#linea_trasp").val(data[0].id_linea_transportista);
                $("#placas_leche").val(data[0].placas);
                $("#kilos_totales_leche").val(data[0].kilos_totales);
                $("#litros_totales_leche").val(data[0].litros_totales);
                $("#densidad_leche").val(data[0].densidad_total);
                $("#cliente1_ficha").val(data[0].id_envio_leche_cliente_ms);
                $("#cliente2_ficha").val(data[0].id_envio_leche_cliente_ms);
                //#endregion

                //#region FICHA 1
                $("#ficha1_leche").val(data[0].id_ficha_bascula);
                $("#folio1_leche").val(data[0].folio_ficha);
                $("#tanque1_leche").val(data[0].tanque);
                $("#peso1_leche").val(data[0].kilos_ficha);
                $("#litros1_leche").val(data[0].litros_ficha);
                $("#sello1_leche").val(data[0].sello);


                $("#fecha1_leche").val(data[0].fecha_segunda_pesada);
                //#endregion

                $("#muestra1_leche").val(data[0].muestra_calidad);
                $("#temperatura1_leche").val(data[0].temperatura);
                $("#sala1_leche").val(data[0].sala);
                $("#proteina1_leche").val(data[0].proteina);
                $("#lactosa1_leche").val(data[0].lactosa);
                $("#crioscopia1_leche").val(data[0].crioscopia);
                $("#solidos_grasos1").val(data[0].solidos_no_grasos);
                $("#caseina1_leche").val(data[0].caseina);
                $("#urea1_leche").val(data[0].urea);
                $("#grasa1_leche").val(data[0].grasa);
                $("#solidos_totales1").val(data[0].solidos_totales);
                $("#acidez1_leche").val(data[0].acidez);
                $("#css1_leche").val(data[0].ccs);
                $("#antibiotico1_leche").val(data[0].antibiotico);
                $("#betalactamicos1_leche").val(data[0].betalactamicos);
                $("#sulfasimas1_leche").val(data[0].sulfasimas);
                $("#tetraciclina1_leche").val(data[0].tetraciclina);
                $("#alcohol1_leche").val(data[0].alcohol_75);
                $("#aflatoxinas1_leche").val(data[0].aflatoxinas);
                $(".leche_mostrar").attr("disabled", "true");
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//#region DETERMINADOR DE SEMANAS Y FECHAS





function ObtenerNumeroSemanasEstimacion(modo) {
    var contador = 0;
    var year = 0;
    var month = 0;
    //CONSULTAR ESTIMACION
    if (modo == 1) {
        year = $("#ano_consulta_estimacion option:selected").text();
        month = $("#mes_consulta_estimacion").val();
    }
    //REGISTRAR ESTIMACION
    else if (modo == 2) {
        year = $("#id_anio_estimacion_diaria option:selected").text();
        month = $("#id_meses").val();
    }
    //PROGRAMACION SEMANAL
    else if (modo == 3) {
        year = $("#id_anio_programacion option:selected").text();
        month = $("#id_meses_programacion").val();
    }
    //CONSULTA PROGRAMACION SEMANAL
    else if (modo == 4) {
        year = $("#ano_consulta_programacion option:selected").text();
        month = $("#mes_consulta_programacion").val();
    }
    if (year != 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ObtenerSemanasEstimacion",
            data: {
                month: month,
                year: year
            },
            dataType: "json",
            success: function (data) {
                console.log("Respuesta del servidor:", data);
                jsRemoveWindowLoad();
                //CONSULTAR ESTIMACION
                if (modo == 1) {
                    $("#semana_consulta_estimacion").empty();
                    $("#semana_consulta_estimacion").prepend(new Option("TODAS", 0));
                }
                //REGISTRAR ESTIMACION
                else if (modo == 2) { $("#id_semana_mensual").empty(); }
                //PROGRAMACION SEMANAL
                else if (modo == 3) { $("#id_semana_programacion").empty(); }
                //CONSULTA PROGRAMACION SEMANAL
                else if (modo == 4) { $("#semana_consulta_programacion").empty(); }

                if (data.SemanasNumero) {
                    data.SemanasNumero.forEach(function (semana) {
                        //CONSULTAR ESTIMACION
                        if (modo == 1) {
                            $("#semana_consulta_estimacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                        }
                        //REGISTRAR ESTIMACION
                        else if (modo == 2) { $("#id_semana_mensual").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana)); }
                        //PROGRAMACION SEMANAL
                        else if (modo == 3) {
                            $("#id_semana_programacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                            ConsultaProduccionEstimadaSemanaTable();
                            //ValidarExistenciaProgramaLeche();
                        }
                        contador++;
                    });
                    contador = 0;
                    //CONSULTA PROGRAMACION SEMANAL
                    if (modo == 4) {
                        $("#semana_consulta_programacion").empty();
                        $("#semana_consulta_programacion").append(new Option("TODAS", 0));

                        data.SemanasNumero.forEach(function (semana) {
                            $("#semana_consulta_programacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                            contador++;
                        });


                    }

                    //REGISTRAR ESTIMACION
                    if (modo == 2) {
                        ObtenerFechaSemanasEstimacionTable();
                    }

                } else {
                    console.error("La estructura de la respuesta no es la esperada.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", error);
                console.error("Detalles del error:", xhr.responseText);
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
}
function ObtenerFechaSemanasEstimacion() {
    var semana = $("#id_semana_mensual").val();
    var year = $("#id_anio_estimacion_diaria option:selected").text();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerFechaSemanas",
        data: {
            semana: semana,
            year: year
        },
        success: function (response) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ENVIOLECHE/ConsultarDiasEstimacionProduccion",
                data: {
                    dia_inicial: response
                },
                success: function (response) {
                    jsRemoveWindowLoad();
                    $("#produccion").html(response);
                },
                error: function (xhr, status, error) {
                    console.error("Error en la solicitud AJAX:", error);
                    console.error("Detalles del error:", xhr.responseText);
                    jsRemoveWindowLoad();
                }
            });
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", error);
            console.error("Detalles del error:", xhr.responseText);
            jsRemoveWindowLoad();
        }
    });
}
//#endregion

//#region SIN USAR (PENDIENTE POR UTILIZAR)
function ObtenerFechaSemanaActual() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ObtenerFechaSemanaActual",
        success: function (response) {
            jsRemoveWindowLoad();
            if (response != "" && response != undefined) {
                $("#semana_dia_inicial").text("Primer día de la semana: " + response.PrimerDiaSemana);
                $("#semana_actual").text("Semana actual: " + response.SemanaActual);
            }
            else {
                iziToast.error({
                    title: 'Aviso',
                    message: 'No se logro determinar la fecha actual',
                });
            }

        },
        error: function (xhr, status, error) {
            jsRemoveWindowLoad();
        }
    });
}
//#endregion

//#region ESTIMACION DIARIA
function MostrarInformacionEstimacionDiaria(id_establo, id_programacion_diaria_g, ano, semana, idano, idmes) {
    $("#establo_produccion_diaria").prop("disabled", true);
    $("#id_anio_estimacion_diaria").prop("disabled", true);
    $("#id_meses").prop("disabled", true);

    $("#id_anio_estimacion_diaria").val(idano);
    $("#id_meses").val(idmes);


    $("#establo_produccion_diaria").val(id_establo);
    $("#m_produccion_diaria").modal("show");


    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ObtenerSemanaEdicion",
        data: {
            year: ano,
            semana: semana
        },
        dataType: "json",
        success: function (response) {
            $("#id_semana_mensual").empty();
            $("#id_semana_mensual").append(new Option("Semana " + response.NoSemana + " del " + response.PrimerDiaSemana + " al " + response.UltimoDiaSemana, response.NoSemana));
            $.ajax({
                type: "POST",
                async: false,
                url: "../ENVIOLECHE/ConsultarDiasEstimacionProduccionInfoTable",
                data: {
                    dia_inicial: response.PrimerDiaSemana,
                    id_programacion_diaria_g: id_programacion_diaria_g
                },
                success: function (response) {
                    $("#div_tabla_estimacion_diaria").html(response);
                    $('#datatable_estimacion_diaria').DataTable({
                        ordering: false,
                        paging: false,
                        dom: "Bfrtip",
                        buttons: [{}],
                        responsive: false,
                        select: false,
                        keys: false,
                        pageLength: 20,
                        searching: false,
                        info: false
                    });
                    CalcularTotalEstimacionSemanal(4);
                    $("#btn_estimacion_semanal").attr("onclick", "ConfirmacionEdicionEstimacionDiaria(" + id_programacion_diaria_g + ");");
                    $("#btn_estimacion_semanal").text("Actualizar estimacion");
                    $("#btn_icon_estimacion_semanal").removeClass("fa-plus");
                    $("#btn_icon_estimacion_semanal").addClass("fa-refresh");
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                }
            });
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", error);
            console.error("Detalles del error:", xhr.responseText);
            jsRemoveWindowLoad();
        }
    });
}

function ObtenernNumeroSemanasActual(modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ENVIOLECHE/ObtenerSemanaActual",
        data: {},
        dataType: "json",
        success: function (data) {
            jsRemoveWindowLoad();
            //CONSULTAR ESTIMACION
            if (modo == 1) {
                $("#semana_consulta_estimacion").empty();
                $("#semana_consulta_estimacion").prepend(new Option("TODAS", 0));
            }
            //REGISTRAR ESTIMACION
            else if (modo == 2) { $("#id_semana_mensual").empty(); }
            //PROGRAMACION SEMANAL
            else if (modo == 3) { $("#id_semana_programacion").empty(); }
            //CONSULTA PROGRAMACION SEMANAL
            else if (modo == 4) { $("#semana_consulta_programacion").empty(); }

            if (modo == 1) {
                $("#semana_consulta_estimacion").append(new Option("Semana " + data.NumeroSemana + " del " + data.PrimerDiaSemana + " al " + data.UltimoDiaSemana, data.NumeroSemana));
            }
            //REGISTRAR ESTIMACION
            else if (modo == 2) { $("#id_semana_mensual").append(new Option("Semana " + data.NumeroSemana + " del " + data.PrimerDiaSemana + " al " + data.UltimoDiaSemana, data.NumeroSemana)); }
            //PROGRAMACION SEMANAL
            else if (modo == 3) {
                $("#id_semana_programacion").append(new Option("Semana " + data.NumeroSemana + " del " + data.PrimerDiaSemana + " al " + data.UltimoDiaSemana, data.NumeroSemana));
                ConsultaProduccionEstimadaSemanaTable();
                ValidarExistenciaProgramaLeche();
            }
            //CONSULTA PROGRAMACION SEMANAL
            else if (modo == 4) {
                $("#semana_consulta_programacion").append(new Option("Semana " + data.NumeroSemana + " del " + data.PrimerDiaSemana + " al " + data.UltimoDiaSemana, data.NumeroSemana));
                ConsultaProgramacionSemanalTable();
            }
            //REGISTRAR ESTIMACION
            if (modo == 2) {
                ObtenerFechaSemanasEstimacionTable();
            }
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", error);
            console.error("Detalles del error:", xhr.responseText);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function ValidarEstimacionDiariaExistente() {
    var establo = $('#establo_produccion_diaria').val();
    var anio = $("#id_anio_estimacion_diaria").val();
    var semana = $("#id_semana_mensual").val();
    return new Promise(function (resolve, reject) {
        $.ajax({
            type: "POST",
            url: "../ENVIOLECHE/ValidarEstimacionDiariaExistente",
            data: {
                establo: establo,
                anio: anio,
                semana: semana
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'Ya existe una programacion diaria con la semana seleccionada',
                    });
                    resolve(false);
                } else {
                    resolve(true);
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                reject(error);
            }
        });
    });
}

function AsignacionEstimacionDiariaSemanal(parametro) {
    const selectores = {
        1: { diario: ".produccion_diario", semanal: "#produccion_semanal_diario" },
        2: { diario: ".sobrante_diario", semanal: "#sobrante_semanal_diario" },
        3: { diario: ".faltante_diario", semanal: "#faltante_semanal_diario" }
    };

    const seleccion = selectores[parametro];
    if (!seleccion) return;

    const valorSemanal = $(seleccion.semanal).val();
    $(seleccion.diario).val(valorSemanal);
}

function ConfirmacionEstimacionDiaria() {
    ValidarEstimacionDiariaExistente().then(function (isValid) {
        if (isValid) {
            var produccion_total = 0;
            var sobrante_total = 0;
            var faltante_total = 0;
            var cantidad = 0;
            var produccion = [];
            var sobrante = [];
            var faltante = [];
            var id_dias = [];
            var fecha = [];
            var count = 0;
            var id_establo = $("#establo_produccion_diaria").val();
            var semana = $("#id_semana_mensual").val();
            var ano = $("#id_anio_estimacion_diaria").val();
            var programacion_diaria = {};
            programacion_diaria.id_establo = id_establo;
            programacion_diaria.no_semana = semana;
            programacion_diaria.id_anio_presupuesto = ano;
            $(".estimacion_fecha").each(function () {
                fecha[count] = $(this).text();
                count++;
            });
            count = 0;
            $(".estimacion_diario_dia").each(function () {
                id_dias[count] = $(this).attr("id").split('_')[1];
                count++;
            });
            count = 0;

            $(".produccion_diario").each(function () {
                cantidad = $(this).text().trim();
                if (cantidad == undefined || cantidad == "") { cantidad = 0; }
                produccion[count] = cantidad;
                count++;

                produccion_total += cantidad;
            });
            count = 0;
            $(".sobrante_diario").each(function () {
                cantidad = $(this).text().trim();
                if (cantidad == undefined || cantidad == "") { cantidad = 0; }
                sobrante[count] = cantidad;
                count++;

                sobrante_total += cantidad;
            });
            count = 0;
            $(".faltante_diario").each(function () {
                cantidad = $(this).text().trim();
                if (cantidad == undefined || cantidad == "") { cantidad = 0; }
                faltante[count] = cantidad;
                count++;

                faltante_total += cantidad;
            });

            if (produccion_total != 0) {
                iziToast.question({
                    timeout: 20000,
                    close: false,
                    overlay: true,
                    displayMode: 'once',
                    id: 'question',
                    zindex: 99999,
                    title: 'Confirmacion',
                    message: 'Estas seguro de confirmar la estimacion diaria?',
                    position: 'center',
                    buttons: [
                        ['<button><b>SI</b></button>', function (instance, toast) {
                            jsShowWindowLoad();
                            $.ajax({
                                type: "POST",
                                async: true,
                                url: "../ENVIOLECHE/ConfirmacionEstimacionDiaria",
                                data: {
                                    programacion_diaria: programacion_diaria,
                                    id_dias: id_dias,
                                    fecha: fecha,
                                    produccion: produccion,
                                    sobrante: sobrante,
                                    faltante: faltante
                                },
                                success: function (response) {
                                    if (response == -1) {
                                        iziToast.error({
                                            title: 'Ocurrió un error al guardar',
                                            message: 'Contacte a desarrollo',
                                        });
                                    }
                                    else {
                                        iziToast.success({
                                            title: 'Exito',
                                            message: 'Se confirmo correctamente la estimacion diaria',
                                        });
                                        $("#m_produccion_diaria").modal('hide');
                                        ConsultaEstimacionDiariaTable();
                                    }
                                },
                                error: function (xhr, status, error) {
                                    console.error(error);
                                }
                            });
                            jsRemoveWindowLoad();
                            instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                        }, true],
                        ['<button>NO</button>', function (instance, toast) {

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
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se puede realizar una estimacion diaria vacia',
                });
            }
        }
    }).catch(function (error) {
        console.error('Error en la validación: ', error);
    });
}

function ConfirmacionEdicionEstimacionDiaria(id_programacion_diaria_g) {
    var produccion_total = 0;
    var sobrante_total = 0;
    var faltante_total = 0;
    var cantidad = 0;
    var produccion = [];
    var sobrante = [];
    var faltante = [];
    var count = 0;

    $(".produccion_diario").each(function () {
        cantidad = $(this).text().trim();
        if (cantidad == undefined || cantidad == "") { cantidad = 0; }
        produccion[count] = cantidad;
        count++;

        produccion_total += cantidad;
    });
    count = 0;
    $(".sobrante_diario").each(function () {
        cantidad = $(this).text().trim();
        if (cantidad == undefined || cantidad == "") { cantidad = 0; }
        sobrante[count] = cantidad;
        count++;

        sobrante_total += cantidad;
    });
    count = 0;
    $(".faltante_diario").each(function () {
        cantidad = $(this).text().trim();
        if (cantidad == undefined || cantidad == "") { cantidad = 0; }
        faltante[count] = cantidad;
        count++;

        faltante_total += cantidad;
    });

    if (produccion_total != 0) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Confirmacion',
            message: 'Estas seguro de confirmar la estimacion diaria?',
            position: 'center',
            buttons: [
                ['<button><b>SI</b></button>', function (instance, toast) {
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../ENVIOLECHE/ConfirmacionEdicionEstimacionDiaria",
                        data: {
                            id_programacion_diaria_g: id_programacion_diaria_g,
                            produccion: produccion,
                            sobrante: sobrante,
                            faltante: faltante
                        },
                        success: function (response) {
                            if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar',
                                    message: 'Contacte a desarrollo',
                                });
                            }
                            else {
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se confirmo correctamente la estimacion diaria',
                                });
                                $("#m_produccion_diaria").modal('hide');
                                ConsultaEstimacionDiariaTable();
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                    jsRemoveWindowLoad();
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                }, true],
                ['<button>NO</button>', function (instance, toast) {

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
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'No se puede realizar una estimacion diaria vacia',
        });
    }
}

function CalcularTotalEstimacionSemanal(indicador) {
    var total = 0;
    //PRODUCCION
    if (indicador == 1) {
        $(".produccion_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }

            total = total + parseFloat(cantidad);
        });
        $("#produccion_diario_total").text(total);
    }
    //SOBRANTE
    else if (indicador == 2) {
        $(".sobrante_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            total = total + parseFloat(cantidad);
        });
        $("#sobrante_diario_total").text(total);
    }
    //FALTANTE
    else if (indicador == 3) {
        $(".faltante_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            total = total + parseFloat(cantidad);
        });
        $("#faltante_diario_total").text(total);
    }
    else if (indicador == 4) {
        $(".produccion_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }

            total = total + parseFloat(cantidad);
        });
        $("#produccion_diario_total").text(total);
        total = 0;
        $(".sobrante_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            total = total + parseFloat(cantidad);
        });
        $("#sobrante_diario_total").text(total);
        total = 0;
        $(".faltante_diario").each(function () {
            cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            total = total + parseFloat(cantidad);
        });
        $("#faltante_diario_total").text(total);
    }
    else { }
}

function CalcularTotalEstimacionDiaria(id_dia_presupuesto) {
    var total = 0;
    var produccion = parseFloat($("#produccion_dia_" + id_dia_presupuesto).text()) || 0;
    var sobrante = parseFloat($("#sobrante_dia_" + id_dia_presupuesto).text()) || 0;
    var faltante = parseFloat($("#faltante_dia_" + id_dia_presupuesto).text()) || 0;
    total = produccion + sobrante - faltante;
    $("#produccion_dia_total_" + id_dia_presupuesto).text(total);
}



//#endregion

//#region PROGRAMACION SEMANAL

function ConfirmacionEdicionProgramacionDestino() {
    var id_ano = $("#id_anio_programacion").val();
    var contador = 0;
    var id_cantidad_dias = [];
    $(".id_dias_estimacion").each(function () {
        id_cantidad_dias[contador] = $(this).text();
        contador++;
    });

    var no_semana = $("#id_semana_programacion").val();
    var informacion_tabla = [];
    var fila_actual = [];

    $(".informacion_guardar").each(function (index) {
        var valor = $(this).text().trim();
        if (valor == "") { valor = "0"; }
        fila_actual.push(valor);

        if (fila_actual.length === 12) {
            informacion_tabla.push(fila_actual);
            fila_actual = [];
        }
    });

    if (fila_actual.length > 0) {
        informacion_tabla.push(fila_actual);
    }

    var fecha_inicio = $(".encabezado_nombre_estimacion_dia:first").text();

    var informacion_tabla_serializada = informacion_tabla.map(fila => fila.join(','));
    if (informacion_tabla.length > 0) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Confirmacion',
            message: 'Estas seguro de confirmar la programacion semanal?',
            position: 'center',
            buttons: [
                ['<button><b>SI</b></button>', function (instance, toast) {
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../ENVIOLECHE/ConfirmacionEdicionProgramacionDestino",
                        data: {
                            id_ano: id_ano,
                            no_semana: no_semana,
                            fecha_inicio: fecha_inicio,
                            id_cantidad_dias: id_cantidad_dias,
                            informacion_tabla: informacion_tabla_serializada,
                        },
                        success: function (response) {
                            if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar',
                                    message: 'Favor de intentarlo mas tarde',
                                });
                            }
                            else {
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se editó correctamente la programaión semanal',
                                });
                                $("#m_produccion_semanal").modal('hide');
                                ConsultaProgramacionSemanalTable();
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                    jsRemoveWindowLoad();
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                }, true],
                ['<button>NO</button>', function (instance, toast) {

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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario programar minimo 1 envio',
        });
    }
}

function ValidarExistenciaProgramaLeche() {
    var semana = $("#id_semana_programacion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ValidarExistenciaProgramaLeche",
        data: {
            semana: semana
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'Ya se registro un programa leche de la semana seleccionada.',
                });
                $("#disponible_programacion_semanal").text('1');
            }
            else {
                $("#disponible_programacion_semanal").text('0');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}



function ValidarDestinoProgramacion(id_destino, id_establo) {
    var existe = false;
    $('#programacion_semanal_detalle tr').each(function () {
        var $tds = $(this).find('td');
        if ($tds.eq(0).text() === id_establo && $tds.eq(3).text() === id_destino) {
            existe = true;
            return existe;
        }
    });
    return existe;
}

function EliminarFilaDetalle(count) {
    $("#row_" + count + "").remove();
}

function CalcularTotalProgramacionSemanal(id_cliente, lista_dias, id_establo_programacion, nombre_establo, id_proveedor, nombre_proveedor, id_destino_programacion) {
    var total = 0;
    //ENVIOS PROGRAMADOS TOTAL
    $(".produccion_cliente_" + id_cliente + "_" + id_establo_programacion + "_" + id_destino_programacion).each(function () {
        var cantidad = $(this).text().trim();
        if (cantidad == undefined || cantidad == "") { cantidad = 0; }

        total += parseFloat(cantidad);
    });
    $("#programacion_produccion_t_" + id_cliente + "_" + id_establo_programacion + "_" + id_destino_programacion).text(total);

    var saldo_destino_establo = new Array(lista_dias.length).fill(0);
    var saldo_estimacion_establo = new Array(lista_dias.length).fill(0);
    var resumen_cliente_programacion = new Array(lista_dias.length).fill(0);
    lista_dias.forEach(function (dia, index) {
        $(".produccion_dia_cliente_" + dia + "_" + id_establo_programacion + "_").each(function () {
            var cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            var cantidad_produccion = parseFloat(cantidad);
            saldo_destino_establo[index] += cantidad_produccion;
        });

        $(".estimacion_dia_" + id_establo_programacion + "_" + dia).each(function () {
            var cantidad = $(this).text().trim();
            saldo_estimacion_establo[index] = cantidad;
        });

        $(".resumen_" + id_cliente + "_" + dia).each(function () {
            var cantidad = $(this).text().trim();
            if (cantidad == undefined || cantidad == "") { cantidad = 0; }
            var cantidadNumerica = parseFloat(cantidad);
            resumen_cliente_programacion[index] += cantidadNumerica;
        });
    });

    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/AgregarDetalleEstabloProgramacion",
        data: {
            count_tabla: count_tabla,
            id_establo_programacion: id_establo_programacion,
            nombre_establo: nombre_establo,
            saldo_cantidad_dias: lista_dias,
            saldo_destino_establo: saldo_destino_establo,
            saldo_estimacion_establo: saldo_estimacion_establo
        },
        success: function (result) {
            $("#programacion_saldo_detalle tr").filter(function () {
                return $(this).find('td:eq(0)').text().includes(id_establo_programacion);
            }).remove();

            $("#thead_saldo_establo").append(result);
            count_tabla++;

            $.ajax({
                type: "POST",
                async: false,
                url: "../ENVIOLECHE/AgregarDetalleResumenProgramacion",
                data: {
                    count_tabla: count_tabla,
                    id_proveedor: id_proveedor,
                    nombre_proveedor: nombre_proveedor,
                    resumen_cliente_programacion: resumen_cliente_programacion
                },
                success: function (result) {
                    $("#programacion_resumen_detalle tr").filter(function () {
                        return $(this).find('td:eq(0)').text().includes(id_proveedor);
                    }).remove();

                    $("#thead_resumen_detalle").append(result);
                    count_tabla++;
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

    //#region VALIDACION EXISTENCIA DE CLIENTE O ESTABLO
    var existe_establo = false;
    var existe_cliente = false;
    $("#programacion_semanal_detalle tr").filter(function () {
        if ($(this).find('td:eq(1)').text().includes(id_cliente)) {
            existe_cliente = true;
        }
        if ($(this).find('td:eq(0)').text().includes(id_establo_programacion)) {
            existe_establo = true;
        }
    });
    if (!existe_cliente) {
        $("#programacion_resumen_detalle tr").filter(function () {
            return $(this).find('td:eq(0)').text().includes(id_proveedor);
        }).remove();
    }
    if (!existe_establo) {
        $("#programacion_saldo_detalle tr").filter(function () {
            return $(this).find('td:eq(0)').text().includes(id_establo_programacion);
        }).remove();
    }
    //#endregion
}



function ConfirmacionProgramacionDestino() {
    ValidarExistenciaProgramaLeche();

    if ($("#disponible_programacion_semanal").text() == 1) {
        return;
    }
    var id_ano = $("#id_anio_programacion").val();
    var contador = 0;
    var id_cantidad_dias = [];
    $(".id_dias_estimacion").each(function () {
        id_cantidad_dias[contador] = $(this).text();
        contador++;
    });

    var no_semana = $("#id_semana_programacion").val();
    var informacion_tabla = [];
    var fila_actual = [];

    $(".informacion_guardar").each(function (index) {
        var valor = $(this).text().trim();
        if (valor == "") { valor = "0"; }
        fila_actual.push(valor);

        if (fila_actual.length === 12) {
            informacion_tabla.push(fila_actual);
            fila_actual = [];
        }
    });

    if (fila_actual.length > 0) {
        informacion_tabla.push(fila_actual);
    }

    var fecha_inicio = $(".encabezado_nombre_estimacion_dia:first").text();

    var informacion_tabla_serializada = informacion_tabla.map(fila => fila.join(','));
    if (informacion_tabla.length > 0) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Confirmacion',
            message: 'Estas seguro de confirmar la programacion semanal?',
            position: 'center',
            buttons: [
                ['<button><b>SI</b></button>', function (instance, toast) {
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../ENVIOLECHE/ConfirmacionProgramacionDestino",
                        data: {
                            id_ano: id_ano,
                            no_semana: no_semana,
                            fecha_inicio: fecha_inicio,
                            id_cantidad_dias: id_cantidad_dias,
                            informacion_tabla: informacion_tabla_serializada,
                        },
                        success: function (response) {
                            if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al guardar',
                                    message: 'Contacte a desarrollo',
                                });
                            }
                            else {
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se confirmo correctamente la programacion semanal',
                                });
                                $("#m_produccion_semanal").modal('hide');
                                ConsultaProgramacionSemanalTable();
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                    jsRemoveWindowLoad();
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                }, true],
                ['<button>NO</button>', function (instance, toast) {

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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario programar minimo 1 envio',
        });
    }
}






//#endregion

//#region CONFIRMACION DE ENVIO DE LECHE
function ConsultaConfirmacionLecheTable() {
    var fecha_inicio = $("#confirmacion_fecha1").val();
    var fecha_fin = $("#confirmacion_fecha2").val();
    var establo = $("#confirmacion_establo").val();
    var destino = $("#confirmacion_destino").val();
    var cliente = $("#confirmacion_cliente").val();
    if (fecha_inicio != undefined && fecha_inicio != "" && fecha_fin != undefined && fecha_fin != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ConsultaConfirmacionLecheTable",
            data: {
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                establo: establo,
                destino: destino,
                cliente: cliente
            },
            success: function (response) {
                $("#div_tabla_confirmacion_leche").html(response);
                $('#tabla_confirmacion_leche').DataTable({
                    ordering: false,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: false,
                    select: true,
                    keys: false,
                    pageLength: 20
                });
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
            message: 'Favor de seleccionar un rango de fechas',
        });
    }
}

function EnvioConfirmadoPorAgrupar(id_envio_leche_g) {
    $("#m_confirmacion_leche").modal('show');
    $("#div_fichas_confirmacion").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ENVIOLECHE/EnvioConfirmadoPorAgrupar",
        data: {
            id_envio_leche_g: id_envio_leche_g
        },
        success: function (response) {
            $("#div_fichas_confirmacion").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function ConfirmacionEnvioLeche(ficha1, ficha2) {
    var recepcion = $("#fecha_confirmacion_cliente").val();
    c_ficha_1 = {};
    c_ficha_2 = {};

    c_ficha_1.id_envio_leche_d = ficha1;
    if ($("#confirma_litros_ficha_" + ficha1 + "").val().trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio ingresar la cantidad de litros',
        });
        return;
    }
    if ($("#confirma_folio_ficha_" + ficha1 + "").val().trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es obligatorio ingresar el folio de la ficha',
        });
        return;
    }

    c_ficha_1.litros_confirmacion = $("#confirma_litros_ficha_" + ficha1 + "").val();
    c_ficha_1.folio_confirmacion = $("#confirma_folio_ficha_" + ficha1 + "").val();

    if (ficha2 != 0) {
        if ($("#confirma_litros_ficha_" + ficha2 + "").val().trim() == "") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es obligatorio ingresar la cantidad de litros',
            });
            return;
        }
        if ($("#confirma_folio_ficha_" + ficha2 + "").val().trim() == "") {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es obligatorio ingresar el folio de la ficha',
            });
            return;
        }
        c_ficha_2.id_envio_leche_d = ficha2;
        c_ficha_2.litros_confirmacion = $("#confirma_litros_ficha_" + ficha2 + "").val();
        c_ficha_2.folio_confirmacion = $("#confirma_folio_ficha_" + ficha2 + "").val();
    }
    else { c_ficha_2.id_envio_leche_d = 0; }

    var fecha = $("#fecha_confirmacion_cliente").val();
    if (fecha != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ConfirmacionEnvioLeche",
            data: {
                recepcion: recepcion,
                c_ficha_1: c_ficha_1,
                c_ficha_2: c_ficha_2
            },
            success: function (response) {
                $("#m_confirmacion_leche").modal('hide');
                jsRemoveWindowLoad();
                iziToast.success({
                    title: 'Exito',
                    message: 'Se confirmo correctamente la recepcion de leche',
                });
                ConsultaConfirmacionLecheTable();
            },
            error: function (xhr, status, error) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al guardar la informacion',
                });
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar la fecha de recepcion',
        });
    }
}
//#endregion

//#region LOTE ENVIO LECHE
function ConsultaLoteAgrupacionLecheTable() {
    var cliente = $("#id_cliente_lote_leche").val();
    var estatus = $("#estatus_lote_leche").val();
    var fecha_inicio = $("#fecha1_lote_leche").val();
    var fecha_fin = $("#fecha2_lote_leche").val();

    if (fecha_inicio != undefined && fecha_inicio != "" && fecha_fin != undefined && fecha_fin != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ConsultaLoteAgrupacionLecheTable",
            data: {
                cliente: cliente,
                estatus: estatus,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            },
            success: function (response) {
                $("#div_tabla_lote_leche").html(response);
                $('#tabla_lote_leche').DataTable({
                    ordering: false,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{}],
                    responsive: false,
                    select: true,
                    keys: false,
                    pageLength: 20
                });
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
            message: 'Favor de seleccionar un rango de fechas',
        });
    }
}

function ConsultaEnviosLecheAgrupacion(modo) {
    $("#m_lotes_leche").modal("show");
    var cliente = 0;
    if (modo != undefined) {
        cliente = $("#select_cliente_destino_agrupacion").val();
    }
    $("#div_agrupacion_leche").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConsultaEnviosLecheAgrupacion",
        data: { cliente: cliente },
        success: function (response) {
            $("#div_agrupacion_leche").html(response);
            $("#datatable_agrupacion_leche").DataTable({
                searching: false,
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
            });
            ClienteAgrupacionLote(4);
            $("#select_cliente_destino_agrupacion").val(cliente);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function AgrupacionGeneral() {
    var checkbox = $("#check_general_lote");
    var isChecked = checkbox.is(":checked");

    if (isChecked) {
        $(".check_lote").prop("checked", true);
        $(".divEditable").attr("contenteditable", "true");
        $(".divEditable").css("background-color", "rgb(46, 135, 215)");
    }
    else {
        $(".check_lote").prop("checked", false);
        $(".divEditable").attr("contenteditable", "false");
        $(".divEditable").css("background-color", "gray");
        $(".divEditable").text('');
    }
    CalcularTotalLtsImporte();
}

function ClienteAgrupacionLote(modo, id_leche_g) {
    //PRECIO GENERAL
    if (modo == 1) {
        //SELECCIONAR
        if ($("#btn_beta_precios_general").hasClass("btn_beta_lineal")) {
            $("#btn_beta_precios_general").removeClass('btn_beta_lineal');
            $("#btn_beta_precios_general").addClass('btn_beta_lineal_danger');
            var leche = $("#precio_leche_general").val();
            var flete = $("#precio_flete_general").val();
            $(".check_lote").each(function () {
                if ($(this).is(':checked')) {
                    var total = 0;
                    var id_envio_leche_g = $(this).attr("id").split('_')[2];
                    /* $("#precio_leche_ficha_" + id_envio_leche_g + "").text(leche);*/
                    $("#precio_flete_ficha_" + id_envio_leche_g + "").text(flete);
                }
            });
            $("#icon_precios").addClass('fa-remove');
            $("#icon_precios").removeClass('fa-check');
        }
        else {
            $("#btn_beta_precios_general").removeClass('btn_beta_lineal_danger');
            $("#btn_beta_precios_general").addClass('btn_beta_lineal');
            $(".check_lote").each(function () {
                if ($(this).is(':checked')) {
                    var total = 0;
                    var id_envio_leche_g = $(this).attr("id").split('_')[2];
                    /*$("#precio_leche_ficha_" + id_envio_leche_g + "").text(0);*/
                    $("#precio_flete_ficha_" + id_envio_leche_g + "").text(0);
                }
            });
            $("#precio_leche_general").val('');
            $("#precio_flete_general").val('');
            $("#icon_precios").removeClass('fa-remove');
            $("#icon_precios").addClass('fa-check');
        }

        CalcularTotalLtsImporte();
    }
    //CHECK DE FICHA CONFIRMADA
    else if (modo == 2) {
        var checkbox = $("#check_lecha_" + id_leche_g);
        var isChecked = checkbox.is(":checked");

        if (isChecked) {
            // Habilitar campos
            /* $("#precio_leche_ficha_" + id_leche_g).attr("contenteditable", "true");*/
            $("#precio_flete_ficha_" + id_leche_g).attr("contenteditable", "true");

            /* $("#precio_leche_ficha_" + id_leche_g).css("background-color", "rgb(46, 135, 215)");*/
            $("#precio_flete_ficha_" + id_leche_g).css("background-color", "rgb(46, 135, 215)");
        } else {
            // Deshabilitar campos
            /*  $("#precio_leche_ficha_" + id_leche_g).text('');*/
            $("#precio_flete_ficha_" + id_leche_g).text('');

            /*$("#precio_leche_ficha_" + id_leche_g).attr("contenteditable", "false");*/
            $("#precio_flete_ficha_" + id_leche_g).attr("contenteditable", "false");

            /*$("#precio_leche_ficha_" + id_leche_g).css("background-color", "gray");*/
            $("#precio_flete_ficha_" + id_leche_g).css("background-color", "gray");
        }
        CalcularTotalLtsImporte();
    }
    //DISENO AUTOMATICO
    else {
        $(".divEditable").css("background-color", "gray");
    }
}

function CalcularTotalLtsImporte() {
    var litros_totales = 0;
    var precio_total_leche = 0;
    var precio_total_flete = 0;
    $(".check_lote").each(function () {
        if ($(this).is(':checked')) {
            var total_leche = 0;
            var total_flete = 0;
            var id_envio_leche_g = $(this).attr("id").split('_')[2];
            var leche = $("#precio_leche_ficha_" + id_envio_leche_g + "").text();
            var flete = $("#precio_flete_ficha_" + id_envio_leche_g + "").text();
            if (leche == undefined || leche == "") { leche = 0; }
            if (flete == undefined || flete == "") { flete = 0; }
            var litros_confirmados = $("#litros_confirmados_" + id_envio_leche_g + "").text();
            total_leche = parseFloat(leche) * parseFloat(litros_confirmados);
            litros_totales += parseFloat(litros_confirmados);
            precio_total_leche += parseFloat(total_leche);

            total_flete = parseFloat(flete) * parseFloat(litros_confirmados);
            precio_total_flete += parseFloat(total_flete);
        }
    });

    $("#litro_total_lote").text(litros_totales.toLocaleString("es-MX"));
    $("#importe_total_leche").text(precio_total_leche.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' }));
    $("#importe_total_flete").text(precio_total_flete.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' }));

}

function ConfirmacionLoteEnvioLeche() {

    var leche_g = [];
    var precio_leche = [];
    var precio_flete = [];
    var proveedor = [];
    var count = 0;

    var validador = true;
    $(".check_lote").each(function () {
        if ($(this).is(':checked')) {
            var id_envio_leche_g = $(this).attr("id").split('_')[2];
            leche_g[count] = id_envio_leche_g;

            var leche = $("#precio_leche_ficha_" + id_envio_leche_g + "").text();
            var flete = $("#precio_flete_ficha_" + id_envio_leche_g + "").text();

            if (leche == undefined || leche == "") { validador = false; }
            if (flete == undefined || flete == "") { flete = 0; }

            precio_leche[count] = leche;
            precio_flete[count] = flete;

            var id_proveedor = $(this).attr("class").split(' ')[2].split('_')[1];
            proveedor[count] = id_proveedor;

            count++;
        }
    });

    //VALIDAR SOLO MISMO PROVEEDOR POR LOTE
    if (proveedor.length > 0 && new Set(proveedor).size === 1) { }
    else if (proveedor.length > 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar al mismo cliente',
        });
        return;
    }



    if (leche_g.length > 0) {
        if (validador == true) {
            iziToast.question({
                timeout: 20000,
                close: false,
                overlay: true,
                displayMode: 'once',
                id: 'question',
                zindex: 99999,
                title: 'Confirmacion',
                message: 'Estas seguro de confirmar el lote?',
                position: 'center',
                buttons: [
                    ['<button><b>SI</b></button>', function (instance, toast) {
                        jsShowWindowLoad();
                        $.ajax({
                            type: "POST",
                            async: false,
                            url: "../ENVIOLECHE/ConfirmacionLoteEnvioLeche",
                            data: {
                                leche_g: leche_g,
                                precio_leche: precio_leche,
                                precio_flete: precio_flete
                            },
                            success: function (response) {
                                if (response == true || response == "True") {
                                    iziToast.success({
                                        title: 'Exito',
                                        message: 'Se registro el lote correctamente!',
                                    });
                                    ConsultaEnviosLecheAgrupacion();
                                }
                                else {
                                    iziToast.warning({
                                        title: 'Aviso',
                                        message: 'Ocurrio un problema al registrar el lote',
                                    });
                                }
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                            }
                        });
                        jsRemoveWindowLoad();
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    }, true],
                    ['<button>NO</button>', function (instance, toast) {
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
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Favor de ingresar un precio leche valido ',
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar una confirmacion de leche',
        });
    }
}

function ConsultaConfirmacionFacturaLote(id_lote_g) {
    $("#m_lotes_factura").modal("show");
    $("#div_lote_factura_leche").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConsultaConfirmacionFacturaLote",
        data: { id_lote_g: id_lote_g },
        success: function (response) {
            $("#div_lote_factura_leche").html(response);

            $("#datatable_lote_leche").DataTable({
                searching: false,
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}


function ConsultarFacturaLote(modo) {
    var folio_factura = "";
    if (modo == 2) {
        folio_factura = $("#factura_informacion_leche").val();
    }
    else {
        folio_factura = $("#folio_factura_lote").val();
    }
    
    if (folio_factura.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la factura para confirmar el lote',
        });
        $("#folio_factura_lote").val('');
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        url: "../ENVIOLECHE/ConsultarFacturaLote",
        data: {
            folio_factura: folio_factura
        },
        success: function (response) {
            if (response.error) {
                iziToast.warning({
                    title: 'Error',
                    message: response.message,
                });
                $("#div_informacion_factura").css("display", "none");
            } else {
                if (modo == 2) {
                    var total_litros = 0;
                    var total_importe = 0;
                    var contador = 0;
                    var tablaIdMostrar = "#tabla_lote_facturas_mostrar";
                    $(tablaIdMostrar + " td").remove();

                    var data = $.parseJSON(response);
                    data.forEach(function () {
                        var nuevaFila = `
                        <tr>
                            <td>${data[contador].FOLIO}</td>
                            <td>${data[contador].USUARIO_CREADOR}</td>
                            <td>${data[contador].FECHA}</td>
                            <td>${data[contador].CLAVE_CLIENTE}</td>
                            <td>${data[contador].NOMBRE_CLIENTE}</td>
                            <td>${data[contador].CLAVE_FISCAL}</td>
                            <td>${data[contador].ARTICULO_ID}</td>
                            <td>${data[contador].ARTICULO}</td>
                            <td>${data[contador].UNIDADES}</td>
                            <td>${data[contador].PRECIO_UNITARIO}</td>
                            <td>${data[contador].PRECIO_TOTAL_NETO}</td>
                        </tr>
                    `;

                        $(tablaIdMostrar + " tbody").append(nuevaFila);

                        total_litros = total_litros + parseFloat(data[contador].UNIDADES);
                        total_importe = total_importe + parseFloat(data[contador].PRECIO_TOTAL_NETO);

                        contador++;
                    });
                    $("#total_litros_factura_mostrar").text(total_litros);
                    $("#total_importe_factura_mostrar").text(total_importe);
                }
                else {
                    var total_litros = 0;
                    var total_importe = 0;

                    var contador = 0;
                    $("#div_informacion_factura").css("display", "block");

                    var tablaId = "#tabla_lote_facturas";
                    $(tablaId + " td").remove();

                    var data = $.parseJSON(response);
                    data.forEach(function () {
                        var nuevaFila = `
                        <tr>
                            <td>${data[contador].FOLIO}</td>
                            <td>${data[contador].USUARIO_CREADOR}</td>
                            <td>${data[contador].FECHA}</td>
                            <td>${data[contador].CLAVE_CLIENTE}</td>
                            <td>${data[contador].NOMBRE_CLIENTE}</td>
                            <td>${data[contador].CLAVE_FISCAL}</td>
                            <td>${data[contador].ARTICULO_ID}</td>
                            <td>${data[contador].ARTICULO}</td>
                            <td>${data[contador].UNIDADES}</td>
                            <td>${data[contador].PRECIO_UNITARIO}</td>
                            <td>${data[contador].PRECIO_TOTAL_NETO}</td>
                        </tr>
                    `;

                        $(tablaId + " tbody").append(nuevaFila);

                        total_litros = total_litros + parseFloat(data[contador].UNIDADES);
                        total_importe = total_importe + parseFloat(data[contador].PRECIO_TOTAL_NETO);

                        contador++;
                    });
                    $("#total_litros_factura").text(total_litros);
                    $("#total_importe_factura").text(total_importe);
                }
            }
        },
        error: function (xhr, status, error) {
            iziToast.error({
                title: 'Error',
                message: xhr.responseText || 'Error al consultar la factura.',
            });
        },
        complete: function () {
            jsRemoveWindowLoad();
        }
    });
}

function ConfirmacionFacturaLoteMs() {
    var folio_factura = $("#folio_factura_lote").val();
    var cliente_ms = $("#cliente_ms_factura").text();
    var precio_total = $("#importe_factura_lote").val();
    var litros_totales = $("#litros_factura_lota").val();
    var id_lote_g = $("#lote_g_factura_leche").text();
    if (folio_factura.trim() == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la factura para confirmar el lote',
        });
        $("#folio_factura_lote").val('');
        return;
    }

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'Aviso',
        message: 'Estas seguro de confirmar la factura?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ENVIOLECHE/ConfirmacionFacturaLoteMs",
                    data: {
                        id_lote_g: id_lote_g,
                        folio_factura: folio_factura,
                        cliente_ms: cliente_ms,
                        precio_total: precio_total,
                        litros_totales: litros_totales
                    },
                    success: function (response) {

                        if (response.error) {
                            iziToast.warning({
                                title: 'Error',
                                message: response.message,
                            });
                        }
                        else {
                            iziToast.success({
                                title: 'Exito',
                                message: response.message,
                            });
                            $("#m_lotes_factura").modal("hide");
                            $("#div_informacion_factura").css("display", "none");
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
                jsRemoveWindowLoad();

            }, true],
            ['<button>NO</button>', function (instance, toast) {

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


function CancelacionLote(id_lote_g) {
    var lote = 0;
    if (id_lote_g == "" || id_lote_g == undefined) {
        id_lote_g = $("#lote_g_factura_leche").text();
        lote = 1;
    }

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 9999,
        title: 'Aviso',
        message: 'Estas seguro de cancelar el lote?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ENVIOLECHE/CancelacionLote",
                    data: {
                        id_lote_g: id_lote_g
                    },
                    success: function (response) {
                        if (response == "True" || response == true) {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se cancelo correctamente el lote',
                            });
                            $("#m_lotes_factura").modal("hide");
                            if (lote == 0) {
                                ConsultaLoteAgrupacionLecheTable();
                            }
                        }
                        else {
                            iziToast.warning({
                                title: 'Aviso',
                                message: 'Ocurrio un problema al cancelar el lote',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
                jsRemoveWindowLoad();
            }, true],
            ['<button>NO</button>', function (instance, toast) {
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


function CancelacionEnviosLecheAgrupacion() {
    var leche_g = [];
    var count = 0;

    $(".check_lote").each(function () {
        if ($(this).is(':checked')) {
            var id_envio_leche_g = $(this).attr("id").split('_')[2];
            leche_g[count] = id_envio_leche_g;
            count++;
        }
    });

    if (leche_g.length > 0) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 99999,
            title: 'Aviso',
            message: 'Estas seguro de cancelar las confirmaciones seleccionadas?',
            position: 'center',
            buttons: [
                ['<button><b>SI</b></button>', function (instance, toast) {
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../ENVIOLECHE/CancelacionEnviosLecheAgrupacion",
                        data: {
                            leche_g: leche_g
                        },
                        success: function (response) {
                            if (response == true || response == "True") {
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se cancelo correctamente las confirmaciones seleccionadas!',
                                });
                                ConsultaEnviosLecheAgrupacion();
                            }
                            else {
                                iziToast.warning({
                                    title: 'Aviso',
                                    message: 'Ocurrio un problema al cancelar las confirmaciones',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                    jsRemoveWindowLoad();
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }, true],
                ['<button>NO</button>', function (instance, toast) {
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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de seleccionar una confirmacion de leche',
        });
    }
}


function MostrarLoteFacturaLeche(id_lote_g) {
    $("#m_lotes_factura").modal("show");
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/MostrarLoteFacturaLeche",
        data: {
            id_lote_g: id_lote_g
        },
        success: function (response) {
            $("#div_lote_factura_leche").html(response);
            $("#datatable_lote_leche_mostrar").DataTable({
                searching: false,
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
            });

            ConsultarFacturaLote(2);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}
//#endregion

//#region COMPROMISO
function CompromisosClienteLeche(anio, nosemana, mes) {
    $("#m_compromiso_leche").modal("show");
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/CompromisosClienteLeche",
        data: {
            anio: anio,
            nosemana: nosemana,
            mes: mes
        },
        success: function (response) {
            $("#div_compromiso_leche").html(response);
            $('#datatable_compromiso_leche').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                keys: false,
                searching: false
            });
            $("#correo_leche_direccion").attr("onclick", "CorreoCompromisosClienteLeche(" + anio + "," + nosemana + "," + mes + ");");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function CorreoCompromisosClienteLeche(anio, nosemana, mes) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOTIFICACIONES/CompromisosClienteLeche",
        data: {
            anio: anio,
            nosemana: nosemana,
            mes: mes
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Correo enviado correctamente',
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se envio el correo',
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

function ConfirmacionComentariosCompromiso(idCumplimientoSemanal, anio, nosemana, mes) {
    var observacion = $("#compromisoComentario_" + idCumplimientoSemanal).val();
    if (observacion.trim() == "" || observacion.trim() == undefined) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la observacion o comentario',
        });
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConfirmacionComentariosCompromiso",
        data: {
            idCumplimientoSemanal: idCumplimientoSemanal,
            observacion: observacion
        },
        success: function (response) {
            CompromisosClienteLeche(anio, nosemana, mes);
            iziToast.success({
                title: 'Exito',
                message: 'Comentario u/o observacion registrado',
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function ComentarioGeneralCompromiso(id_semanal) {
    var comentario = $("#compromisoComentarioGeneral").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ComentarioGeneralCompromiso",
        data: {
            id_semanal: id_semanal,
            comentario: comentario
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Comentario guardado correctamente',
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se guardo el comentario',
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
//#endregion




//#region AJUSTE ESTIMACION DE PRODUCCION DIARIA

//MODAL
function ModalEstimacionDiaria() {
    $("#m_produccion_diaria").modal("show");
    $("#btn_estimacion_semanal").attr("onclick", "ConfirmacionEstimacionDiaria();");
    $("#btn_estimacion_semanal").text("Confirmar estimacion");
    $("#btn_icon_estimacion_semanal").removeClass("fa-refresh");
    $("#btn_icon_estimacion_semanal").addClass("fa-plus");
    $("#establo_produccion_diaria").prop("disabled", false);
    $("#id_anio_estimacion_diaria").prop("disabled", false);
    $("#id_meses").prop("disabled", false);
    ObtenerSemanasFechaMensualEstimacion();
    /*ObtenerNumeroSemanasEstimacion(2);*/
    //ObtenernNumeroSemanasActual(2);
}
//BUSCAR
function ConsultaEstimacionDiariaTable() {
    var establo = $("#establo_consulta_estimacion").val();
    var semana = $("#semana_consulta_estimacion").val();
    var mes = $("#mes_consulta_estimacion").val();
    var ano = $("#ano_consulta_estimacion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConsultaEstimacionDiariaTable",
        data: {
            establo: establo,
            semana: semana,
            mes: mes,
            ano: ano
        },
        success: function (response) {
            try {
                $("#datatable_consulta_estimacion_diaria").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_estimacion_diaria_consulta").html(response);
            $('#datatable_consulta_estimacion_diaria').DataTable({
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}
//FECHAS INDEX
function ObtenerSemanasFechaMensualIndexEstimacion() {
    var contador = 0;
    var year = $("#ano_consulta_estimacion option:selected").text();
    var month = $("#mes_consulta_estimacion").val();
    if ($("#ano_consulta_estimacion").val() == 0) {
        $("#semana_consulta_estimacion").prop("disabled", true);
        $("#semana_consulta_estimacion").empty();
        $("#semana_consulta_estimacion").append(new Option("TODAS", 0));
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ObtenerSemanasFechaMensual",
            data: {
                month: month,
                year: year
            },
            dataType: "json",
            success: function (data) {
                $("#semana_consulta_estimacion").prop("disabled", false);
                $("#semana_consulta_estimacion").empty();
                if (data.SemanasNumero) {

                    $("#semana_consulta_estimacion").append(new Option("TODAS", 0));
                    data.SemanasNumero.forEach(function (semana) {
                        $("#semana_consulta_estimacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                        contador++;
                    });
                } else {
                    console.error("La estructura de la respuesta no es la esperada.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", error);
                console.error("Detalles del error:", xhr.responseText);
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
}
//SEMANAS CON FECHA MODAL
function ObtenerSemanasFechaMensualEstimacion() {
    var contador = 0;
    year = $("#id_anio_estimacion_diaria option:selected").text();
    month = $("#id_meses").val();
    if (year != 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ObtenerSemanasFechaMensual",
            data: {
                month: month,
                year: year
            },
            dataType: "json",
            success: function (data) {
                $("#id_semana_mensual").empty();
                if (data.SemanasNumero) {
                    data.SemanasNumero.forEach(function (semana) {
                        $("#id_semana_mensual").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                        contador++;
                    });
                    $("#estimacion_inicio_semana").text(data.PrimerDiaSemana[0]);

                    ObtenerFechasPorSemanaEstimacion();
                } else {
                    console.error("La estructura de la respuesta no es la esperada.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", error);
                console.error("Detalles del error:", xhr.responseText);
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
}
//FECHAS LUN-DOMINGO MODAL
function ObtenerFechasPorSemanaEstimacion() {
    var dia_inicial = $("#id_semana_mensual option:selected").text().split(" ")[3];
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ENVIOLECHE/ConsultarDiasEstimacionProduccionTable",
        data: {
            dia_inicial: dia_inicial
        },
        success: function (response) {
            $("#div_tabla_estimacion_diaria").html(response);
            $('#datatable_estimacion_diaria').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
                keys: false,
                pageLength: 20,
                searching: false,
                info: false
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}
//#endregion

//#region AJUSTE PROGRAMA DE LECHE
function ObtenerSemanasFechaMensualIndexPrograma() {
    var contador = 0;
    var year = $("#ano_consulta_programacion option:selected").text();
    var month = $("#mes_consulta_programacion").val();
    if ($("#ano_consulta_programacion").val() == 0) {
        $("#mes_consulta_programacion").val(0);
        $("#mes_consulta_programacion").prop("disabled", true);
        $("#semana_consulta_programacion").prop("disabled", true);
        $("#semana_consulta_programacion").empty();
        $("#semana_consulta_programacion").append(new Option("TODAS", 0));
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ObtenerSemanasFechaMensual",
            data: {
                month: month,
                year: year
            },
            dataType: "json",
            success: function (data) {
                $("#mes_consulta_programacion").prop("disabled", false);
                $("#semana_consulta_programacion").prop("disabled", false);
                $("#semana_consulta_programacion").empty();
                if (data.SemanasNumero) {

                    $("#semana_consulta_programacion").append(new Option("TODAS", 0));
                    data.SemanasNumero.forEach(function (semana) {
                        $("#semana_consulta_programacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                        contador++;
                    });
                } else {
                    console.error("La estructura de la respuesta no es la esperada.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", error);
                console.error("Detalles del error:", xhr.responseText);
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
}

function ConsultaProgramacionSemanalTable() {
    var no_semana = $("#semana_consulta_programacion").val();
    var mes = $("#mes_consulta_programacion").val();
    var id_ano = $("#ano_consulta_programacion").val();
    jsShowWindowLoad();
    $("#div_programacion_semanal_consulta").html('');
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConsultaProgramacionSemanalTable",
        data: {
            id_ano: id_ano,
            mes: mes,
            no_semana: no_semana
        },
        success: function (response) {
            $("#div_programacion_semanal_consulta").html(response);
            $('#datatable_consulta_programacion_semanal').DataTable({
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

function ModalProgramacionSemanal() {
    $("#detalle_programa_diario_div").css("display", "none");

    $("#m_produccion_semanal").modal("show");
    /*ObtenerNumeroSemanasEstimacion(3);*/

    $("#id_anio_programacion").removeAttr("disabled");
    $("#id_meses_programacion").removeAttr("disabled");
    $("#id_semana_programacion").removeAttr("disabled");

    $("#btn_confirmacion_semanal").attr("onclick", "ConfirmacionProgramacionDestino();");
    ObtenerSemanasFechaMensualPrograma();
    ConsultaProduccionEstimadaSemanaTable(1);
    /* ValidarExistenciaProgramaLeche();*/
    //ObtenernNumeroSemanasActual(3);
}

//SEMANAS CON FECHA MODAL
function ObtenerSemanasFechaMensualPrograma() {
    var contador = 0;
    year = $("#id_anio_programacion option:selected").text();
    month = $("#id_meses_programacion").val();
    if (year != 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ENVIOLECHE/ObtenerSemanasFechaMensual",
            data: {
                month: month,
                year: year
            },
            dataType: "json",
            success: function (data) {
                $("#id_semana_programacion").empty();
                if (data.SemanasNumero) {
                    data.SemanasNumero.forEach(function (semana) {
                        $("#id_semana_programacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                        contador++;
                    });
                    ConsultaProduccionEstimadaSemanaTable(1);
                } else {
                    console.error("La estructura de la respuesta no es la esperada.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", error);
                console.error("Detalles del error:", xhr.responseText);
                jsRemoveWindowLoad();
            }
        });
        jsRemoveWindowLoad();
    }
}

function ConsultaProduccionEstimadaSemanaTable(modo) {
    var year = $("#id_anio_programacion").val();
    var semana = $("#id_semana_programacion").val();
    jsShowWindowLoad();
    $("#div_produccion_estimada").html('');
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConsultaProduccionEstimadaSemanaTable",
        data: {
            semana: semana,
            year: year,
            modo: modo
        },
        success: function (response) {
            $("#div_produccion_estimada").html(response);
            $('#datatable_consulta_produccion_estimada').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                scrollY: false,
                select: false,
                keys: false,
                scrollY: false,
                scrollX: true,
                searching: false
            });


            $("#programacion_semanal_detalle td").remove();
            $("#programacion_saldo_detalle td").remove();
            $("#programacion_resumen_detalle td").remove();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}

var count_tabla = 1;
function AgregarDetalleProgramacion() {
    var disponible_programa_leche = $("#existe_programacion_semanal").text();
    var disponible_establo = $("#existe_programacion_semanal_establo").text();
    if (disponible_programa_leche == 1) {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Ya se registro un programa leche de la semana actual.',
        });
        return;
    }
    if (disponible_establo == 0) {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Es necesario seleccionar una semana con programacion para los establos',
        });
        return;
    }

    var existe_produccion = $("#datatable_consulta_produccion_estimada td").length;
    if (existe_produccion > 1) {
        var id_estimacion = 0;
        var id_establo = $("#establo_programacion").val();
        var id_cliente_destino = $("#destino_programacion").val();
        var no_semana = $("#id_semana_programacion").val();
        var id_anio = $("#id_anio_programacion").val();
        var nombre_establo = $("#establo_programacion option:selected").text();
        var id_destino = 0;
        $(".estimacion_establos").each(function () {
            if ($(this).text() == nombre_establo) {
                id_estimacion = $(this).attr("id");
            }
        });

        if (ValidarDestinoProgramacion(id_cliente_destino, id_establo) == false) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ENVIOLECHE/AgregarDetalleProgramacion",
                data: {
                    count_tabla: count_tabla,
                    id_establo: id_establo,
                    nombre_establo: nombre_establo,
                    id_cliente_destino: id_cliente_destino,
                    id_destino: id_destino,
                    no_semana: no_semana,
                    id_anio: id_anio,
                    id_estimacion: id_estimacion
                },
                success: function (result) {
                    $("#tbody_cliente_detalle").append(result);
                    count_tabla = count_tabla + 1;
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        }
        else {
            iziToast.warning({
                title: '¡Aviso!',
                message: 'Asegurate de agregar solo 1 vez el destino por establo',
            });
        }
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Favor de seleccionar una semana con produccion estimada cargada',
        });
    }
}


function EditarProgramaSemanal(anio, nosemana, fecha) {
    $("#m_produccion_semanal").modal("show");
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/EditarProgramaSemanal",
        data: { anio: anio, nosemana: nosemana },
        success: function (response) {
            if (response.length > 0) {
                var ano = response[0].Ano;
                var mes = response[0].Mes;
                var semana = response[0].Semana;
                var AnoCompleto = response[0].AnoCompleto;
                $("#id_anio_programacion").val(ano);
                $("#id_meses_programacion").val(mes);

                var contador = 0;

                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../ENVIOLECHE/ObtenerSemanasEstimacion",
                    data: {
                        month: mes,
                        year: AnoCompleto
                    },
                    dataType: "json",
                    success: function (data) {
                        $("#id_semana_programacion").empty();
                        if (data.SemanasNumero) {
                            data.SemanasNumero.forEach(function (semana) {
                                $("#id_semana_programacion").append(new Option("Semana " + semana + " del " + data.PrimerDiaSemana[contador] + " al " + data.UltimoDiaSemana[contador], semana));
                                contador++;
                            });

                        } else {
                            console.error("La estructura de la respuesta no es la esperada.");
                        }
                        $("#id_semana_programacion").val(semana);
                        ConsultaProduccionEstimadaSemanaTable(2);
                        response.forEach(item => {
                            var nombreClietne = item.NombreDestino;
                            var idEstablo = item.IdEstablo;
                            var nombreEstablo = item.NombreEstablo;
                            var idClienteEnvioLeche = item.IdClienteEnvioLeche;
                            var id_Destino = item.IdDestinoEnvioLeche;
                            var no_semana = semana;
                            var id_anio = ano;
                            var id_estimacion = 0;
                            $(".estimacion_establos").each(function () {
                                if ($(this).text() == nombreEstablo) {
                                    id_estimacion = $(this).attr("id");
                                }
                            });


                            $.ajax({
                                type: "POST",
                                async: false,
                                url: "../ENVIOLECHE/AgregarDetalleProgramacion",
                                data: {
                                    count_tabla: count_tabla,
                                    id_establo: idEstablo,
                                    nombre_establo: nombreEstablo,
                                    id_cliente_destino: idClienteEnvioLeche,
                                    id_destino: id_Destino,
                                    no_semana: no_semana,
                                    id_anio: id_anio,
                                    id_estimacion: id_estimacion
                                },
                                success: function (result) {
                                    $("#tbody_cliente_detalle").append(result);
                                    count_tabla = count_tabla + 1;
                                },
                                error: function (xhr, status, error) {
                                    console.error(error);
                                }
                            });
                            var totalitros = 0;
                            var contador = 1;
                            item.CantidadesLitros.forEach(litro => {
                                $(".produccion_dia_cliente_" + contador + "_" + idEstablo + "_").text(litro.cantidad_litros);
                                contador++;
                                if (contador == 8) { contador = 1; }
                                totalitros += litro.cantidad_litros;
                            });
                            $("#programacion_produccion_t_" + idClienteEnvioLeche + "_" + idEstablo + "_" + id_Destino).text(totalitros);
                            CalcularTotalProgramacionSemanal(idClienteEnvioLeche, [1, 2, 3, 4, 5, 6, 7], idEstablo, nombreEstablo, idClienteEnvioLeche, nombreClietne, id_Destino);


                            $("#btn_detalle_confirmacion").attr("onclick", "ProgramaSemanalDetalleDia(" + anio + "," + nosemana + ");");
                            $("#btn_detalle_confirmacion_excel").attr("onclick", "DescargarDetalleProgramaDiaExcel('" + fecha + "');");
                        });

                    },
                    error: function (xhr, status, error) {
                        console.error("Error en la solicitud AJAX:", error);
                        console.error("Detalles del error:", xhr.responseText);
                    }
                });

                $("#lbl_fecha_produccion_diaria").text(fecha);
            } else {
                console.log("No se encontraron resultados.");
            }
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud:", status, error);
        },
        complete: function () {
            jsRemoveWindowLoad();
        }
    });

    $("#id_anio_programacion").attr("disabled", "true");
    $("#id_meses_programacion").attr("disabled", "true");
    $("#id_semana_programacion").attr("disabled", "true");

    $("#btn_confirmacion_semanal").attr("onclick", "ConfirmacionEdicionProgramacionDestino();");

    $("#detalle_programa_diario_div").css("display", "inline");
    jsRemoveWindowLoad();
}


function MostrarInformacionProgramaSemanal(anio, nosemana) {
    $("#m_produccion_semanal_mostrar").modal("show");

    jsShowWindowLoad();
    $("#div_produccion_mostrar").html('');
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/MostrarInformacionProgramaSemanal",
        data: {
            anio: anio,
            nosemana: nosemana
        },
        success: function (response) {
            $("#div_produccion_mostrar").html(response);
            $('#datatable_envio_produccion').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
                keys: false,
                searching: false,
                info: false
            });
            $('#datatable_envio_cliente').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
                keys: false,
                searching: false,
                info: false
            });
            $('#datatable_envio_saldo').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
                keys: false,
                searching: false,
                info: false
            });
            $('#datatable_envio_resumen').DataTable({
                ordering: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
                keys: false,
                searching: false,
                info: false
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();
}
//#endregion


function ProgramaSemanalDetalleDia(anio, nosemana) {
    $("#m_produccion_semanal_detalle").modal("show");
    var fecha = $("#lbl_fecha_produccion_diaria").text();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ProgramaSemanalDetalleDia",
        data: {
            fecha: fecha
        },
        success: function (response) {
            $("#div_produccion_semanal_detalle").html(response);
            var contador = 0;
            $.ajax({
                type: "POST",
                async: false,
                url: "../ENVIOLECHE/ClientesDetalle",
                data: {
                    anio: anio,
                    nosemana: nosemana
                },
                dataType: "json",
                success: function (data) {
                    data.id_Cliente.forEach(function (cliente) {
                        $("#destino_programacion_cliente").append(new Option(data.Establo[contador] + " | " + data.Fecha[contador] + " | " + data.Cliente[contador] + " | " + data.Litros[contador], cliente + "_" + data.IDEstablo[contador] + "_" + data.Dia[contador]));
                        contador++;
                    });
                    $("#destino_programacion_cliente").select2({
                        placeholder: 'Selecciona un proveedor',
                        //dropdownParent: $("#tabulador")
                    });

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../ENVIOLECHE/ConsultarProgramaSemanalDetalleDia",
                        data: {
                            anio: anio,
                            nosemana: nosemana
                        },
                        dataType: "json",
                        success: function (data) {
                            var info_position = 0;
                            data.id_programacion_d.forEach(function (programa) {
                                var id_destino = data.id_programacion_dia[info_position];
                                var id_establo = data.idestablo[info_position];
                                var id_dia = data.Dia[info_position];

                                var id_linea = data.id_linea_transp[info_position];
                                var id_unidad = data.id_tanque_pipa[info_position];

                                var horas = data.hora[info_position].split(":");
                                var hora = horas[0] + ":" + horas[1];
                                var tanque = data.tanque_pipa[info_position];
                                var litros = data.cantidad_litros[info_position];
                                var destino = data.destino[info_position];
                                var linea = data.linea_transp[info_position];

                                var tablaId = "#datatable_" + id_establo + "_" + id_dia;

                                var nuevaFila = `
                                        <tr>
                                            <td style = "display:none" class = "programa_id_${id_dia}_${id_establo}">${id_destino}</td>
                                            <td style = "display:none" class = "programa_tanque_${id_dia}_${id_establo}">${id_unidad}</td>
                                            <td style = "display:none" class = "programa_linea_${id_dia}_${id_establo}">${id_linea}</td>                    
                                            <td class = "programa_hora_${id_dia}_${id_establo}">${hora}</td>
                                            <td>${tanque}</td>
                                            <td class = "litros_dia_${id_dia}_${id_establo} programa_litros_${id_dia}_${id_establo}">${litros}</td>
                                            <td>${destino}</td>
                                            <td>${linea}</td>
                                            <td><button class="btn btn_beta_danger" onclick="RemoverDetalleProgramaDia(this,${id_dia},${id_establo})">Eliminar<i class="fa fa-remove"></i></button></td>
                                        </tr>
                                    `;

                                $(tablaId + " tbody").append(nuevaFila);


                                var total_litros = 0;
                                $(`.litros_dia_${id_dia}_${id_establo}`).each(function () {
                                    var valor = parseFloat($(this).text()) || 0;
                                    total_litros += valor;
                                });

                                $(`#litros_${id_establo}_${id_dia}`).text(total_litros);

                                info_position++;
                            });
                        },
                        error: function (xhr, status, error) {
                            console.error("Error en la solicitud AJAX:", error);
                            console.error("Detalles del error:", xhr.responseText);
                            jsRemoveWindowLoad();
                        }
                    });


                },
                error: function (xhr, status, error) {
                    console.error("Error en la solicitud AJAX:", error);
                    console.error("Detalles del error:", xhr.responseText);
                    jsRemoveWindowLoad();
                }
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    jsRemoveWindowLoad();




}


var detalleDias = 0;
function AgregarDetalleProgramaDia() {
    var id_c_tabla = 0;

    var destino = $("#destino_programacion_cliente").val().split("_");

    var id_destino = destino[0];
    var id_establo = destino[1];
    var id_dia = destino[2];

    var id_linea = $("#linea_prog_cliente").val();
    var id_unidad = $("#tanque_prog_cliente").val();

    var hora = $("#hora_prog_cliente").val();
    var tanque = $("#tanque_prog_cliente option:selected").text();
    var litros = $("#lts_prog_cliente").val();
    var destino = $("#destino_programacion_cliente option:selected").text().split("|")[2];
    var linea = $("#linea_prog_cliente option:selected").text();

    var establo = $("#establo_prog_cliente").val();


    if (!hora || !tanque || !litros || !destino || !linea) {
        iziToast.warning({
            title: 'Aviso',
            message: '¡Todos los campos son obligatorios"!',
            timeout: 5000,
        });
        return;
    }

    var tablaId = "#datatable_" + id_establo + "_" + id_dia;

    var nuevaFila = `
                <tr>
                    <td style = "display:none" class = "programa_id_${id_dia}_${id_establo}">${id_destino}</td>
                    <td style = "display:none" class = "programa_tanque_${id_dia}_${id_establo}">${id_unidad}</td>
                    <td style = "display:none" class = "programa_linea_${id_dia}_${id_establo}">${id_linea}</td>                    
                    <td class = "programa_hora_${id_dia}_${id_establo}">${hora}</td>
                    <td>${tanque}</td>
                    <td class = "litros_dia_${id_dia}_${id_establo} programa_litros_${id_dia}_${id_establo}">${litros}</td>
                    <td>${destino}</td>
                    <td>${linea}</td>
                    <td class = "text-center" ><button class="btn btn_beta_danger" onclick="RemoverDetalleProgramaDia(this,${id_dia},${id_establo})">Eliminar<i class="fa fa-remove"></i></button></td>
                </tr>
            `;

    // Agregar la fila a la tabla
    $(tablaId + " tbody").append(nuevaFila);

    var total_litros = 0;
    $(`.litros_dia_${id_dia}_${id_establo}`).each(function () {
        var valor = parseFloat($(this).text()) || 0;
        total_litros += valor;
    });

    $(`#litros_${id_establo}_${id_dia}`).text(total_litros);

    iziToast.success({
        title: '',
        message: '¡Se añadio correctamente!',
        timeout: 2000
    });
}
function RemoverDetalleProgramaDia(boton, id_dia, id_establo) {
    $(boton).closest("tr").remove();

    var total_litros = 0;
    $(`.litros_dia_${id_dia}_${id_establo}`).each(function () {
        var valor = parseFloat($(this).text()) || 0;
        total_litros += valor;
    });

    $(`#litros_${id_establo}_${id_dia}`).text(total_litros);
}

function ConfirmacionDetalleProgramaDia(establos, dias) {
    var anio = $("#id_anio_programacion").val();
    var nosemana = $("#id_semana_programacion").val();


    var id_establos = establos.split('_');
    var id_dias = dias.split('_');

    var id_programacion = [];
    var id_pipa = [];
    var cantidad_litros = [];
    var hora = [];
    var id_linea = [];

    id_establos.forEach(function (id_establo) {
        id_dias.forEach(function (id_dia) {
            $(`.programa_id_${id_dia}_${id_establo}`).each(function () {
                id_programacion.push($(this).text());
            });

            $(`.programa_tanque_${id_dia}_${id_establo}`).each(function () {
                id_pipa.push($(this).text());
            });

            $(`.litros_dia_${id_dia}_${id_establo}`).each(function () {
                cantidad_litros.push($(this).text());
            });

            $(`.programa_hora_${id_dia}_${id_establo}`).each(function () {
                hora.push($(this).text());
            });

            $(`.programa_linea_${id_dia}_${id_establo}`).each(function () {
                id_linea.push($(this).text());
            });
        });
    });

    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/ConfirmacionDetalleProgramaDia",
        data: {
            anio: anio,
            nosemana: nosemana,
            id_programacion: id_programacion,
            id_pipa: id_pipa,
            cantidad_litros: cantidad_litros,
            hora: hora,
            id_linea: id_linea
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: '¡Se guardo la informacion correctamente!',
                    timeout: 5000,
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: '¡No guardo la informacion!',
                    timeout: 5000,
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

function HistoricoFacturaLeche() {
    var factura = $("#factura_historico").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ENVIOLECHE/HistoricoFacturaLeche",
        data: { factura: factura },
        success: function (response) {
            $("#div_tabla_historico_factura").html(response);
            $("#datatable_historico_leche").DataTable({
                searching: false,
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: false,
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}