$('#fecha_inicio_admin').val(last_week);
$('#fecha_fin_admin').val(today);

//#region ------------ ENTREGAS DE VALE
function CargoContableVale() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGO/CargosContablesSelect",
        data: {
        },
        success: function (response) {
            $("#select_cuenta").html(response);
            ConsultarCuentasContableSelect();
            $("#select_cargo").focus();
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarSolicitudesAutorizadas() {
    var id_almacen_g = $("#id_almacen_g_atender").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarSolicitudesAutorizadas",
        data: { id_almacen_g: id_almacen_g },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_autorizadas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_solicitudes_autorizadas").html(response);
            $('#tabla_solicitudes_autorizadas').DataTable({
                
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
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarSolicitudDetalleAutorizar(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarSolicitudDetalleAutorizar",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_solicitud").html(response);
            $('#tabla_solicitudes_detalle').DataTable({
                
                searching: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{
                    extend: "",
                }],
                responsive: !0
            });
            ConsultarCambiosArticuloSolicitud(id_solicitud_g);
            $("#m_detalle_solicitud").modal("show");
            //ConsultarSolicitudesAutorizadas();
            $("#btn_entregar_vale").css("display", "block");
            CaracteresControl();

            //$(".divEditable").keypress(function (e) {
            //    if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
            //});


            //$('.divEditable').on('input', function () {
            //    var content = $(this).text();
            //    var sanitizedContent = content.replace(/\s+/g, '');
            //    $(this).text(sanitizedContent);
            //});
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarCambiosArticuloSolicitud(id_solicitud_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarCambiosArticuloSolicitud",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            try {
                $("#tabla_cambios_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_cambios_solicitud").html(response);
            $('#tabla_cambios_articulos').DataTable({
                
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{
                    extend: "",
                }],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



function CambiarArticuloEntrega(id_solicitud_d, nombre, id_articulo_viejo, id_solicitud_g) {
    $("#clave_articulo_viejo").text("Clave: " + id_articulo_viejo);
    $("#nombre_articulo_viejo").text("Descripción: " + nombre);
    $("#btn_cambio_articulo").attr("onclick", "CambiarArticuloSolicitud(" + id_solicitud_d + ", '" + id_articulo_viejo + "', " + id_solicitud_g +" )");
    $("#m_cambio_articulo").modal("show");

    $("#id_articulo_nuevo_cambio").val('');
    $("#cantidad_cambio").val('');
    $("#observaciones_cambio").val('');

    $("#cantidad_cambio").keypress(function (e) {
        if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
    });
}

function CambiarArticuloSolicitud(id_solicitud_d, id_articulo_viejo, id_solicitud_g) {
    var id_articulo_nuevo = $("#id_articulo_nuevo_cambio").val().split('|')[0];
    var observaciones = $("#observaciones_cambio").val();
    var cantidad_nueva = $("#cantidad_cambio").val();

    if (id_articulo_nuevo == "" || observaciones == "" || cantidad_nueva == "") {
        iziToast.error({
            title: 'Asegurese de ingresar todos los campos',
            message: '',
        });
    }
    //else if (id_articulo_nuevo == id_articulo_viejo) {
    //    iziToast.warning({
    //        title: 'No se puede cambiar por el mismo articulo',
    //        message: '',
    //    });
    //}
    else if (observaciones.length <= 10) {
        iziToast.warning({
            title: 'Agregue una observacion de mas de 10 caracteres',
            message: '',
        });
    }
    else if (cantidad_nueva == "" || cantidad_nueva == undefined || isNaN(cantidad_nueva) || parseFloat(cantidad_nueva) <= 0) {
        iziToast.warning({
            title: 'Ingrese una cantidad valida',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ALMACEN/CambiarArticuloSolicitud",
            data: {
                id_solicitud_g: id_solicitud_g,
                id_solicitud_d: id_solicitud_d,
                id_articulo_nuevo: id_articulo_nuevo,
                id_articulo_viejo: id_articulo_viejo,
                observaciones: observaciones,
                cantidad_nueva: cantidad_nueva
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "0") {
                    $("#m_cambio_articulo").modal("hide");
                    $("#id_articulo_nuevo_cambio").val("");
                    $("#observaciones_cambio").val("");
                    $("#cantidad_cambio").val("");
                    ConsultarSolicitudDetalleAutorizar(id_solicitud_g);
                    iziToast.success({
                        title: 'Cambio realizado correctamente',
                        message: '',
                    });
                }
                else if (response == "1") {
                    iziToast.error({
                        title: 'El articulo ya tuvo una entrega parcial',
                        message: '',
                    });
                }
                else if (response == "2") {
                    iziToast.error({
                        title: 'El articulo ya existe en la solicitud',
                        message: 'Registre otro o cambie la cantidad del existente',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un problema al registrar el cambio',
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
    
}

function EntregarVale(id_solicitud_g) {
    var count = 0;
    var entregados = [];
    var id_solicitudes_d = [];
    var cant_entrega = [];
    var valid_negativos = false;
    var valid_vacios = false;
    var valid_mayores = false;

    $(".check_entrega_articulo").each(function () {
        var id_solicitud_d = $(this).attr("name");
        id_solicitudes_d[count] = id_solicitud_d;
        var valor = $("#cant_entrega_" + id_solicitud_d + "").val();
        var solicitado = $("#cant_solicitada_" + id_solicitud_d + "").text();

        if ($(this).prop("checked") == true) {
            entregados[count] = true;
            if (valor == '') {
                valid_vacios = true;
            }
            if (valor <= 0) {
                valid_negativos = true;
            }
            if (valor > parseFloat(solicitado)) {
                valid_mayores = true;
            }
        }
        else {
            entregados[count] = false;
        }

        cant_entrega[count] = valor;
        count++;
    });

    if (valid_vacios == true) {
        iziToast.warning({
            title: 'Asegurese de no ingresar valores vacíos',
            message: '',
        });
    }
    else if (valid_negativos == true) {
        iziToast.warning({
            title: 'Ingrese solo valores positivos',
            message: '',
        });
    }
    else if (valid_mayores == true) {
        iziToast.warning({
            title: 'No puede surtir cantidades mayores a lo solicitado',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../ALMACEN/EntregarVale",
            data: {
                id_solicitud_g: id_solicitud_g,
                id_solicitudes_d: id_solicitudes_d,
                entregados: entregados,
                cant_entrega: cant_entrega
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    $("#m_detalle_solicitud").modal("hide");
                    ConsultarSolicitudesAutorizadas();
                    iziToast.success({
                        title: 'Vale entregado correctamente',
                        message: '',
                    });
                }
                else if (response == 1) {
                    iziToast.warning({
                        title: 'Seleccione al menos 1 articulo para entregar el vale',
                        message: '',
                    });
                }
                else if (response == 2) {
                    ConsultarSolicitudesAutorizadas();
                    ConsultarSolicitudDetalleAutorizar(id_solicitud_g);
                    iziToast.warning({
                        title: 'La cantidad a entregar supera la cantidad solicitada en el vale',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un problema al registrar la entrega',
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
}
function CerrarVale(id_solicitud_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 1999,
        title: 'ATENCION',
        message: '¿Está seguro de cerrar el vale?',
        position: 'center',
        buttons: [
            ['<button><b>Si, confirmar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../ALMACEN/CerrarVale",
                    data: {
                        id_solicitud_g: id_solicitud_g
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        $("#m_detalle_solicitud").modal("hide");
                        if (response == 0) {                            
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se ha cerrado correctamente el vale',
                            });                            
                        }
                        else if (response == 1) {
                            iziToast.warning({
                                title: 'Aviso',
                                message: '¡Solo se puede cerrar el vale al entregar minimo un articulo!',
                            });
                        }
                        else {
                            iziToast.warning({
                                title: 'Aviso',
                                message: '¡Ocurrio un problema al cerrar el vale!',
                            });
                        }
                        ConsultarSolicitudesAutorizadas();
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                    }
                });
            }, true],
            ['<button>No, cancelar operacion</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }],
        ]
    });
}
function CancelarVale(id_solicitud_g,modo) {
    if (modo == 1) {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 1999,
            title: 'ATENCION',
            message: '¿Está seguro de cancelar el vale?',
            position: 'center',
            buttons: [
                ['<button><b>Si, confirmar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../ALMACEN/CancelarVale",
                        data: {
                            id_solicitud_g: id_solicitud_g
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == "0") {
                                $("#m_detalle_solicitud").modal("hide");
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se ha cancelado correctamente el vale',
                                });
                                ConsultarSolicitudesAutorizadas();
                            }
                            else {
                                $("#m_detalle_solicitud").modal("hide");
                                iziToast.warning({
                                    title: 'Aviso',
                                    message: 'No se ha cancelado el vale, ya se entrego un articulo',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                        }
                    });
                }, true],
                ['<button>No, cancelar operacion</button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                }],
            ]
        });
    }
    else {
        jsRemoveWindowLoad();
        iziToast.warning({
            title: 'Aviso',
            message: 'No se puede cancelar el vale, ya se realizarón entregas',
        });
    }
}

function CambiarCuentaVale(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/CambiarCuentaVale",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_cambio_cuenta").modal("show");
            $("#div_cambio_cuenta").html(response);
            CargarCargoCuenta();    
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CargarCargoCuenta() {
    $(".cargos").each(function () {
        var selectCargos = $(this);
        // Obtener el ID único para este conjunto de selects
        var uniqueId = selectCargos.attr("id").split("_")[1];

        $.ajax({
            type: "POST",
            url: "../CATALOGOS/ConsultarCargosCuentas",
            data: {},
            success: function (response) {
                // Llenar el dropdown de cargos
                $.each(response.cargos, function (index, cargo) {
                    selectCargos.append($('<option>', {
                        value: cargo.id_cargo_contable_g,
                        text: cargo.nombre_cargo
                    }));
                });
                // Manejar el cambio en el dropdown de cargos
                selectCargos.on('change', function () {
                    var selectedCargoId = $(this).val();
                    // Filtrar las cuentas basadas en el cargo seleccionado
                    var filteredCuentas = response.cuentas.filter(function (cuenta) {
                        return cuenta.id_cargo_contable == selectedCargoId;
                    });
                    // Obtener el ID único para el dropdown de cuentas
                    var selectCuentas = $("#selectCuentas_" + uniqueId);
                    selectCuentas.empty();
                    // Limpiar y llenar el dropdown de cuentas con las cuentas filtradas
                    $.each(filteredCuentas, function (index, cuenta) {
                        selectCuentas.append($('<option>', {
                            value: cuenta.id_cuenta_contable,
                            text: cuenta.nombre_cuenta
                        }));
                    });
                });
                CargarCargoAuto();
                CargarCuentaAuto();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    });
}

function CargarCargoAuto() {
    $(".cargoReal").each(function () {
        var uniqueId = $(this).attr("id").split("_")[1];
        var cargoActual = $("#selectRCargo_" + uniqueId).text();
        $("#selectCargos_" + uniqueId).val(cargoActual).trigger('change');
    });
}

function CargarCuentaAuto() {
    $(".cuentaReal").each(function () {
        var uniqueId = $(this).attr("id").split("_")[1];
        var cargoActual = $("#selectRCuenta_" + uniqueId).text();
        $("#selectCuentas_" + uniqueId).val(cargoActual).trigger('change');
    });
}

function ConfirmarCambioCuentaVale() {
    var vale_d = [];
    var cargo = [];
    var cuenta = [];
    var cargocuenta = [];
    var contador = 0;

    $(".cargos").each(function () {
        var selectCargos = $(this);
        vale_d[contador] = selectCargos.attr("id").split("_")[1];
        cargo[contador] = $("#selectCargos_" + vale_d[contador]).val();
        cuenta[contador] = $("#selectCuentas_" + vale_d[contador]).val();

        var valeId = vale_d[contador];
        var selecCuenta = $(`#selectCuentas_${valeId} option:selected`);
        var selecCargo = $(`#selectCargos_${valeId} option:selected`);
        var cuentaLimpia = selecCuenta.text().replace(/\s{2,}/g, ' ');
        var cargoLimpio = selecCargo.text().replace(/\s{2,}/g, ' ');

        cargocuenta[contador] = `${cuentaLimpia} .${cargoLimpio}`;

        contador++;
    });

    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConfirmarCambioCuentaVale",
        data: {
            vale_d: vale_d,
            cargo: cargo,
            cuenta: cuenta,
            cargocuenta: cargocuenta
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: '¡Éxito!',
                    message: 'Se cambiaron correctamente las cuentas del vale',
                });
            }
            else if (response == "0") {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se cambiaron correctamente las cuentas del vale,  un vale fue entregado',
                });
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se cambiaron correctamente las cuentas del vale',
                });
                console.log(response);
            }
            $("#m_cambio_cuenta").modal("hide");
            var vale_id = $("#vale_g_id").text();
            ConsultarSolicitudDetalleAutorizar(vale_id);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarValeDetalle(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarSolicitudDetalleAutorizar",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_solicitud").html(response);
            $('#tabla_solicitudes_detalle').DataTable({
                
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
            ConsultarCambiosArticuloSolicitud(id_solicitud_g);
            $("#m_detalle_solicitud").modal("show");
            ConsultarSolicitudesAutorizadas();
            $("#btn_entregar_vale").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarExistenciaArticuloAlmacen(id_solicitud_mercancia_d, id_almacen_g, id_articulo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarExistenciaArticuloAlmacen",
        data: {
            id_almacen_g: id_almacen_g,
            id_articulo: id_articulo
        },
        success: function (response) {
            $("#cant_existencia_" + id_solicitud_mercancia_d + "").text(response);

            var div_existencia = $("#div_existencia_" + id_solicitud_mercancia_d + "");
            if (response == "0") {
                $("#check_" + id_solicitud_mercancia_d + "").attr("disabled", "true");
                div_existencia.css("background-color", "#f93939");
            }
            else {
                div_existencia.css("background-color", "#2ee629");
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function RellenarEntregaArticulo(id_solicitud_d) {
    var check = $("#check_" + id_solicitud_d + "");
    var cant_entregar = $("#cant_entrega_" + id_solicitud_d + "");
    var cant_entregado = $("#cant_entregado_" + id_solicitud_d + "").text();
    var cant_solicitada = $("#cant_solicitada_" + id_solicitud_d + "").text();
    var cant_existencia = $("#cant_existencia_" + id_solicitud_d + "").text();
    if (isNaN(cant_entregado) || cant_entregado == '') { cant_entregado = '0'; }
    var total = parseFloat(cant_solicitada) - parseFloat(cant_entregado);
    if (check.is(":checked")) {
        if (parseFloat(cant_existencia) <= parseFloat(total)) {
            cant_entregar.val(cant_existencia);
        }
        else {
            cant_entregar.val(total);
        }
        cant_entregar.removeAttr('disabled');
    }
    else {
        cant_entregar.val("0");
        cant_entregar.attr('disabled','true');
    }
}

function ValidarEntregaArticulo(id_solicitud_d) {
    var cant_entregar = $("#cant_entrega_" + id_solicitud_d + "");
    var cant_entregado = $("#cant_entregado_" + id_solicitud_d + "").text();
    var cant_solicitada = $("#cant_solicitada_" + id_solicitud_d + "").text();
    var cant_existencia = $("#cant_existencia_" + id_solicitud_d + "").text();
    var total = parseFloat(cant_solicitada) - parseFloat(cant_entregado);

    if (parseFloat(cant_entregar.val()) > total || parseFloat(cant_entregar.val()) == 0 ) {
        if (parseFloat(cant_existencia) <= parseFloat(total)) {
            cant_entregar.val(cant_existencia.replace(/\n/g, ' '));
        }
        else {
            cant_entregar.val(total.toString().replace(/\n/g, ' '));
        }
    }

    if (parseFloat(cant_entregar.val()) > parseFloat(cant_existencia)) {
        if (parseFloat(cant_existencia) <= parseFloat(total.toString().replace(/\n/g, ' '))) {
            cant_entregar.val(cant_existencia.replace(/\n/g, ' '));
        }
        else {
            cant_entregar.val(total.toString().replace(/\n/g, ' '));
        }
    }
}

function GenerarValePDF(id_solicitud_g, id_div) {
    jsShowWindowLoad();
    $("#" + id_div + "").css("display", "block");
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/GenerarValePDF",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#" + id_div + "").html(response);
            var HTML_Width = $("#" + id_div + "").width();
            var HTML_Height = $("#" + id_div + "").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#" + id_div + "")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/png", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'PNG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'PNG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Solicitud Mercancia.pdf");
            });
            $("#" + id_div + "").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//#endregion

//#region ------- HISTORIAL DE VALES
function ConsultarHistorialValesTable() {
    var id_almacen_g = $("#id_almacen_g_historial").val();
    var fecha_inicio = $("#fecha_inicio_admin").val();
    var fecha_fin = $("#fecha_fin_admin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarHistorialValesTable",
        data: {
            id_almacen_g: id_almacen_g,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_vales_historial").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_historial_vales").html(response);
            $('#tabla_vales_historial').DataTable({

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
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarSolicitudDetalleHistorial(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarSolicitudDetalleHistorial",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_solicitud").html(response);
            $('#tabla_solicitudes_detalle').DataTable({

                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
            ConsultarCambiosArticuloSolicitud(id_solicitud_g);
            $("#m_detalle_solicitud").modal("show");
            ConsultarSolicitudesAutorizadas();
            $("#btn_entregar_vale").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion

//#region -------- DEVOLUCION DE VALES
function ConsultarValeDevolucion() {
    var id_almacen_g = $("#id_almacen_g_devolucion").val();
    var id_vale = $("#id_vale_devolucion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarValeDevolucion",
        data: {
            id_almacen_g: id_almacen_g,
            id_vale: id_vale
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_vale_devolucion").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DevolucionVale(id_solicitud_mercancia_d_log) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de devolver el artículo?',
        position: 'center',
        buttons: [
            ['<button><b>Si, realizar devolución</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../ALMACEN/DevolucionVale",
                    data: { id_solicitud_mercancia_d_log: id_solicitud_mercancia_d_log },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == 0) {
                            iziToast.success({
                                title: 'Devolución realizada correctamente',
                                message: '',
                            });
                            ConsultarValeDevolucion();
                        }
                        else if (response == 1) {
                            iziToast.error({
                                title: 'El artículo ya fué devuelto',
                                message: '',
                            });
                            ConsultarValeDevolucion();
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al realizar la devolución',
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

//#endregion


//#region -------- CONSULTAR DEVOLUCIONES
function ConsultarDevolucionesTable() {
    var id_almacen_g = $("#id_almacen_g_historial").val();
    var fecha_inicio = $("#fecha_inicio_admin").val();
    var fecha_fin = $("#fecha_fin_admin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarDevolucionesTable",
        data: {
            id_almacen_g: id_almacen_g,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_vales_historial").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_historial_vales").html(response);
            $('#tabla_vales_historial').DataTable({

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
                responsive: false,
                select: true,
                keys: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion

