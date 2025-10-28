//BITACORA DE ENTRADA - SALIDA
function LimpiarFormularioEntrada() {
    $(".input_clear").val("");
    $('input[name="radio_tipo_entrada"]').prop('checked', false);
    $("#id_area_tipo_entrada").html("");
    $("#id_area_tipo_entrada").append("<option value=''>Selecciona un tipo de entrada</option>");
    $("#div_formulario_header").css("display", "none");
    $("#btn_registrar_entrada").css("display", "none");
}


function ConusltarAreasTiposEntradasCaseta() {
    var id_establo = $("#id_establo").val();
    var id_tipo_entrada = $("input[name='radio_tipo_entrada']:checked").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConusltarAreasTiposEntradasCaseta",
        data: {
            id_establo: id_establo,
            id_tipo_entrada: id_tipo_entrada
        },
        success: function (response) {
            $("#id_area_tipo_entrada").html(response);
            $("#div_area").css("display", "block");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarTipoFormulario() {
    $("#div_formulario_header").css("display", "block");
    var id_tipo_entrada = $("input[name='radio_tipo_entrada']:checked").val();
    if (id_tipo_entrada == 2) {  //VISITA GENERAL
        //$("#div_formulario_general").css("display", "block");
        $("#div_formulario_bascula").css("display", "none");
        $("#div_formulario_empleado").css("display", "none");
    }
    if (id_tipo_entrada == 3) {  //VISITA BASCULA
        //$("#div_formulario_general").css("display", "none");
        $("#div_formulario_bascula").css("display", "block");
        $("#div_formulario_empleado").css("display", "none");
    }
    if (id_tipo_entrada == 1) {
        //$("#div_formulario_general").css("display", "none");
        $("#div_formulario_bascula").css("display", "none");
        $("#div_formulario_empleado").css("display", "block");
    }
    $("#btn_registrar_entrada").css("display", "block");

    ConusltarAreasTiposEntradasCaseta();
}

//function ConsultarTiposTanquesRadio() {
//    $.ajax({
//        type: "POST",
//        async: false,
//        url: "../CATALOGOS/ConsultarTiposTanquesRadio",
//        data: {
//        },
//        success: function (response) {
//            $("#div_tipos_tanques").html(response);
//        },
//        error: function (xhr, status, error) {
//            console.error(error);
//        }
//    });
//}

function ConsultarFormularioBascula(no_pipas) {
    $("#btn_salida_bascula").css("display", "none");
    $("#div_info_ficha_1").html("");
    $("#div_info_ficha_2").html("");
    $(".ficha_input").val('');
    if (no_pipas == 1) {
        $("#div_formulario_tanque_sencillo").css("display", "block");
        $("#div_formulario_tanque_full").css("display", "none");
    }
    else {
        $("#div_formulario_tanque_sencillo").css("display", "block");
        $("#div_formulario_tanque_full").css("display", "block");
    }
}

function GuardarLogEntradaCaseta() {
    $(".input_valid").css("border-color", "");
    var id_establo = $("#id_establo").val();
    var nombre = $("#nombre_ingresante").val();
    var placas = $("#placas_ingresante").val();
    var asunto = $("#asunto_ingresante").val();

    var id_area_caseta = $("#id_area_tipo_entrada").val();
    var id_tipo_entrada_caseta = $("input[name='radio_tipo_entrada']:checked").val();
    if (id_tipo_entrada_caseta == undefined || id_tipo_entrada_caseta == "") {
        iziToast.warning({
            title: 'Seleccione un tipo de entrada',
            message: '',
        });
    }
    else if (id_area_caseta == "" || id_area_caseta == undefined || id_area_caseta == null) {
        iziToast.warning({
            title: 'Seleccione un area valida',
            message: '',
        });
    }
    else if (nombre == "" || placas == "") {
        iziToast.warning({
            title: 'Los campos "Nombre" y "Placas" son obligatorios',
            message: '',
        });
        $(".input_valid").each(function () {
            if ($(this).val() == '') { $(this).css("border-color", "red"); }
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CASETAS_ESTABLOS/GuardarLogEntradaCaseta",
            data: {
                id_establo: id_establo,
                nombre: nombre,
                id_tipo_entrada_caseta: id_tipo_entrada_caseta,
                id_area_caseta: id_area_caseta,
                placas: placas,
                asunto: asunto
            },
            success: function (response) {
                if (response > 0) {
                    if (id_tipo_entrada_caseta == 1) {  //EMPLEADO
                        iziToast.success({
                            title: 'Entrada Empleado/Taller registrada correctamente',
                            message: '',
                        });
                    }

                    if (id_tipo_entrada_caseta == 2) {  //VISITA GENERAL
                        //GuardarLogEntradaCasetaVisitaGeneral(response);
                        iziToast.success({
                            title: 'Entrada general registrada correctamente',
                            message: '',
                        });
                    }

                    if (id_tipo_entrada_caseta == 3) {  // VISITA BASCULA
                        iziToast.success({
                            title: 'Entrada a bascula registrada correctamente',
                            message: '',
                        });
                    }
                    LimpiarFormularioEntrada();
                    ConsultarEntradasPendientesEstablo();
                }
                else {

                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

function GuardarLogEntradaCasetaVisitaGeneral(id_log_g) {
    var observaciones = $("#obs_visita_general").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/GuardarLogEntradaCasetaVisitaGeneral",
        data: {
            id_log_g: id_log_g,
            observaciones: observaciones
        },
        success: function (response) {
            if (response == 0) {
                iziToast.success({
                    title: 'Entrada general registrada correctamente',
                    message: '',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al registrar la entrada general',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarEntradasPendientesEstablo() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarEntradasPendientesEstablo",
        data: { id_establo: id_establo },
        success: function (response) {
            try {
                $("#entradas_pendientes_table").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_entradas_pendientes").html(response);
            $('#entradas_pendientes_table').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function BuscarVisitantePlacas(placas) {
    var id_establo = $("#id_establo").val();
    if (placas == undefined || placas == "") {
        placas = $("#placas_busqueda").val();
    }
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/BuscarVisitantePlacas",
        data: {
            placas: placas,
            id_establo: id_establo
        },
        success: function (response) {
            $("#div_informacion_busqueda").html(response);
            $("#m_generar_salida").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function BuscarVisitanteID(id_log_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/BuscarVisitanteID",
        data: {
            id_log_g: id_log_g
        },
        success: function (response) {
            $("#div_informacion_busqueda").html(response);
            $("#m_generar_salida").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GuardarLogSalidaHeader(tipo_registro, id_log_g) {
    if (tipo_registro == 1) {
        var folio_vale_salida = $("#folio_vale_salida_input").val();
        $.ajax({
            type: "POST",
            async: false,
            url: "../CASETAS_ESTABLOS/GuardarLogSalidaVale",
            data: {
                id_log_g: id_log_g,
                folio_vale_salida: folio_vale_salida
            },
            success: function (response) {
                if (response == 0) {
                    iziToast.success({
                        title: 'Salida registrada correctamente',
                        message: '',
                    });
                    $("#m_generar_salida").modal("hide");
                    ConsultarEntradasPendientesEstablo();
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Registro de entrada no encontrado',
                        message: '',
                    });
                }
                else if (response == 2) {
                    iziToast.success({
                        title: 'Este registro ya tiene una salida',
                        message: '',
                    });
                }
                else if (response == 3) {
                    iziToast.error({
                        title: 'Folio invalido',
                        message: '',
                    });
                }
                else if (response == 4) {
                    iziToast.error({
                        title: 'Folio perteneciente a otro establo',
                        message: 'Asegurese de que se haya generado el vale para este establo',
                    });
                }
                else if (response == 5) {
                    iziToast.error({
                        title: 'Ocurrió un error al registrar la salida',
                        message: 'Intente nuevamente',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }

    else if (tipo_registro == 2) {  //VISITA GENERAL
        $.ajax({
            type: "POST",
            async: false,
            url: "../CASETAS_ESTABLOS/GuardarLogSalidaGeneral",
            data: {
                id_log_g: id_log_g
            },
            success: function (response) {
                if (response == 0) {
                    iziToast.success({
                        title: 'Salida registrada correctamente',
                        message: '',
                    });
                    $("#m_generar_salida").modal("hide");
                    ConsultarEntradasPendientesEstablo();
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Registro de entrada no encontrado',
                        message: '',
                    });
                }
                else if (response == 2) {
                    iziToast.success({
                        title: 'Este registro ya tiene una salida',
                        message: '',
                    });
                }
                else if (response == 3) {
                    iziToast.error({
                        title: 'Ocurrió un error al registrar la salida',
                        message: 'Intente nuevamente',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }

    else if (tipo_registro == 3) {  //VISITA BASCULA
        var valid_duplicada = true;
        var id_tipo_tanque = $("input[name='radio_tipo_tanque']:checked").val();
        if (id_tipo_tanque == 1) {
            if ($("#ficha_1").val() == $("#ficha_2").val()) { valid_duplicada = false; }
        }
        else if (id_tipo_tanque == 2) {
            if ($("#ficha_2").val() == $("#ficha_1").val()) { valid_duplicada = false; }
        }

        if (valid_duplicada == false) {
            iziToast.warning({
                title: 'Fichas repetidas revise bien',
                message: '',
            });
        }
        else {
            var fichas = [];
            var placas = [];
            var choferes = []
            var productos = [];
            var proveedores = [];
            var count = 0;
            if (id_tipo_tanque == 1) {  //SENCILLO
                fichas[0] = $("#ficha_1").val();
            }
            else {  //FULL
                fichas[0] = $("#ficha_1").val();
                fichas[1] = $("#ficha_2").val();
            }
            $(".bascula_proveedor").each(function () {
                proveedores[count] = $(this).text();
                count++;
            });
            count = 0;
            $(".bascula_producto").each(function () {
                productos[count] = $(this).text();
                count++;
            });
            count = 0;
            $(".bascula_placas").each(function () {
                placas[count] = $(this).text();
                count++;
            });
            count = 0;
            $(".bascula_chofer").each(function () {
                choferes[count] = $(this).text();
                count++;
            });
            count = 0;

            $.ajax({
                type: "POST",
                async: false,
                url: "../CASETAS_ESTABLOS/GuardarLogSalidaBascula",
                data: {
                    id_log_g: id_log_g,
                    id_tipo_tanque: id_tipo_tanque,
                    fichas: fichas,
                    placas: placas,
                    choferes: choferes,
                    productos: productos,
                    proveedores: proveedores
                },
                success: function (response) {
                    if (response == 0) {
                        iziToast.success({
                            title: 'Salida registrada correctamente',
                            message: '',
                        });
                        $("#m_generar_salida").modal("hide");
                        ConsultarEntradasPendientesEstablo();
                    }
                    else if (response == 1) {
                        iziToast.error({
                            title: 'Registro de entrada no encontrado',
                            message: '',
                        });
                    }
                    else if (response == 2) {
                        iziToast.error({
                            title: 'Este registro ya tiene una salida',
                            message: '',
                        });
                    }
                    else if (response == 3) {
                        iziToast.error({
                            title: 'Ocurrió un error al registrar la salida',
                            message: 'Intente nuevamente',
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        }
    }

    else {
        alert("TIPO DE ENTRADA DESCONOCIDO");
    }
}

function BuscarInformacionFicha(modo) {
    var no_pipa = $("input[name='radio_tipo_tanque']:checked").val();
    var valid_duplicada = true;
    var ficha = "";
    if (modo == 1) {
        ficha = $("#ficha_1").val();
        if (ficha == $("#ficha_2").val()) { valid_duplicada = false; }
    }
    else if (modo == 2) {
        ficha = $("#ficha_2").val();
        if (ficha == $("#ficha_1").val()) { valid_duplicada = false; }
    }

    if (valid_duplicada == false) {
        iziToast.warning({
            title: 'Fichas repetidas',
            message: '',
        });
    }
    else {
        var id_establo = $("#id_establo").val();
        $.ajax({
            type: "POST",
            async: false,
            url: "../CASETAS_ESTABLOS/BuscarInformacionFicha",
            data: {
                ficha: ficha,
                id_establo: id_establo
            },
            success: function (response) {
                if (response == "") {
                    iziToast.warning({
                        title: 'Ficha no encontrada',
                        message: '',
                    });
                }
                else {
                    if (modo == 1) {
                        $("#div_info_ficha_1").html(response);
                    }
                    else if (modo == 2) {
                        $("#div_info_ficha_2").html(response);
                    }

                    if (no_pipa == 1) {
                        if ($("#ficha_1").val() != "") {
                            $("#btn_salida_bascula").css("display", "block");
                        }
                        else { $("#btn_salida_bascula").css("display", "none"); }
                    }
                    else if (no_pipa == 2) {
                        if ($("#ficha_1").val() != "" && $("#ficha_2").val() != "") {
                            $("#btn_salida_bascula").css("display", "block");
                        }
                        else { $("#btn_salida_bascula").css("display", "none"); }
                    }
                }

            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }

}


function ConsultarBitacoraCaseta() {
    var id_establo = $("#id_establo_bitacora").val();
    var fecha_inicio = $("#fecha_inicio_bitacora").val();
    var fecha_fin = $("#fecha_fin_bitacora").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarBitacoraCaseta",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_establo: id_establo
        },
        success: function (response) {
            try {
                $("#reporte_bitacora__table").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_bitacora").html(response);
            $('#reporte_bitacora__table').DataTable({
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

function ConsultarBitacoraCasetaExcel() {
    var id_establo = $("#id_establo_bitacora").val();
    var fecha_inicio = $("#fecha_inicio_bitacora").val();
    var fecha_fin = $("#fecha_fin_bitacora").val();
    //var url = "https://localhost:44371//REPORTES/ConsultarBitacoraCasetaExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_establo=" + id_establo + "";
    var url = "https://siib.beta.com.mx/REPORTES/ConsultarBitacoraCasetaExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_establo=" + id_establo + "";
    window.open(url, '_blank');
}


//END   BITACORA DE ENTRADA - SALIDA


//----------------------- VALES DE SALIDA

function ConsultarFormatoValeSalida() {
    var id_establo = $("#id_establo_vale").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarFormatoValeSalida",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#div_vale_formato").html(response);
            GenerarFolioValeSalida();

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GenerarFolioValeSalida() {
    var id_establo = $("#id_establo_vale").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/GenerarFolioValeSalida",
        data: {
            id_establo: id_establo
        },
        success: function (response) {
            $("#folio_vale_salida").text(response);

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function MostrarValorSalida(id_tipo_salida, nombre_tipo_salida) {
    var tr_valor_tipo_salida = $("#tr_valor_tipo_salida");
    var input_valor_tipo_salida = $("#nombre_tipo_salida_vale_salida");

    if (id_tipo_salida == 11) {   //OTRO
        tr_valor_tipo_salida.css("display", "contents");
        input_valor_tipo_salida.val('');
    }
    else {
        tr_valor_tipo_salida.css("display", "none");
        input_valor_tipo_salida.val(nombre_tipo_salida);
    }
}

function GenerarValeSalida() {
    var id_establo = $("#id_establo_vale").val();
    var folio = $("#folio_vale_salida").text();
    var id_area = $("#id_area_vale").val();
    var descripcion = $("#descripcion_vale_salida").val();
    var id_tipo_salida = $("input[name='tipo_salida_vale_salida']:checked").val();
    var tipo_salida = $("#nombre_tipo_salida_vale_salida").val();

    var proveedor = $("#proveedor_vale_salida").val();
    var id_activo = $("#id_activo_vale_salida").val();
    var tipo_activo = $("#tipo_activo_vale_salida").val();


    if (folio == "ERROR AL GENERAR" || folio == "") {
        iziToast.warning({
            title: 'Error en el folio. Revise',
            message: '',
        });
    }
    else if (id_area == "" || id_area == "0" || id_area == null) {
        iziToast.warning({
            title: 'Selecciona una area',
            message: '',
        });
    }
    else if (descripcion == "" || descripcion.length < 20) {
        iziToast.warning({
            title: 'Ingrese una descripción de mas de 20 caractéres',
            message: '',
        });
    }
    else if (tipo_salida == "") {
        iziToast.warning({
            title: 'Selecciona un tipo de salida',
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
            message: '¿Está seguro de generar el vale de salida?',
            position: 'center',
            buttons: [
                ['<button><b>Si, generar vale</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../CASETAS_ESTABLOS/GenerarValeSalida",
                        data: {
                            id_establo: id_establo,
                            folio: folio,
                            id_area: id_area,
                            descripcion: descripcion,
                            id_tipo_salida: id_tipo_salida,
                            tipo_salida: tipo_salida,
                            proveedor: proveedor,
                            tipo_activo: tipo_activo,
                            id_activo: id_activo
                        },
                        success: function (response) {
                            if (response > 0) {
                                iziToast.success({
                                    title: 'Vale generado correctamente',
                                    message: 'Intente nuevamente',
                                });
                                ConsultarFormatoValeSalida();
                                ConsultarValeSalidaImpresion(response);
                            }
                            else if (response == -1) {
                                GenerarFolioValeSalida();
                                iziToast.error({
                                    title: 'Folio ya registrado',
                                    message: 'Intente nuevamente',
                                });
                            }
                            else {
                                GenerarFolioValeSalida();
                                iziToast.error({
                                    title: 'Ocurrió un error al registrar el vale de salida',
                                    message: 'Intente nuevamente',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
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

function ConsultarValesGenerados() {
    var fecha_inicio = $("#fecha_inicio_historial").val();
    var fecha_fin = $("#fecha_fin_historial").val();
    var id_establo = $("#id_establo_historial").val();

    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarValesGenerados",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_establo: id_establo
        },
        success: function (response) {
            try {
                $("#datatable_historial_vales").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_historial_vales").html(response);
            $('#datatable_historial_vales').DataTable({
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

function ConsultarDetalleValeSalida(id_vale_salida_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarDetalleValeSalida",
        data: { id_vale_salida_g: id_vale_salida_g },
        success: function (response) {
            $("#m_detalle_vale_salida").modal("show");
            $("#div_detalle_vale_salida").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarValeSalidaImpresion(id_caseta_vale) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CASETAS_ESTABLOS/ConsultarValeSalidaImpresion",
        data: { id_caseta_vale: id_caseta_vale },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response != "") {
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
                //newTab.close();
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se encontró el vale',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConusltarAreasTiposEntradasCasetaValeSalida() {
    var id_establo = $("#id_establo_vale").val();
    var id_tipo_entrada = 1;
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConusltarAreasTiposEntradasCaseta",
        data: {
            id_establo: id_establo,
            id_tipo_entrada: id_tipo_entrada
        },
        success: function (response) {
            $("#id_area_vale").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//----------------------- END VALES DE SALIDA

