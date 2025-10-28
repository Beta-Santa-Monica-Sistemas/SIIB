//#region Requisiciones
function LimpiarUtileriaRequisicion() {
    $("#div_informacion_requisicion").html("");
    $("#tr_update_header_requi").css("display", "none");

    $("#txt_tipo_requi_editar").text('');
    $("#id_tipo_requisicion").val('');
    $("#txt_concepto_requi_editar").val('');
    $("#id_centro_g_editar").val('');


    $("#select_centro_edit").find("option[value='selecte_centro']").prop('selected', 'selected');
    $("#select_prioridad_edit").find("option[value='1']").prop('selected', 'selected');
}

function CancelarRequisicionUtilerias() {
    var id_requisicion_g = $("#folio_requi_buscar").val();
    if (id_requisicion_g == "" && id_requisicion_g.trim() == "") {
        iziToast.warning({
            title: '¡Es necesario ingresar la requisicion a elimiar!',
            message: '',
        });
        return;
    }

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de cancelar la requisicion?',
        position: 'center',
        buttons: [
            ['<button><b>Si, cancelar la requisicion</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../UTILERIAS/CancelarRequisicionUtilerias",
                    data: {
                        id_requisicion_g: id_requisicion_g
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == -1) {
                            iziToast.warning({
                                title: 'Aviso',
                                message: "La requisicion tiene orden de compra",
                            });
                        }
                        else if (response == 1) {
                            iziToast.success({
                                title: 'Exito',
                                message: "Se cancelo correctamente la requisicion",
                            });
                            BuscarInformacionRequisicionUtileria();
                        }
                        else if (response == 0) {
                            iziToast.error({
                                title: 'Aviso',
                                message: "Ocurrio un fallo al cancelar la requisicion, favor de intentarlo en otro momento",
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                        iziToast.error({
                            title: 'Aviso',
                            message: "Ocurrio un fallo al cancelar la requisicion, favor de intentarlo en otro momento",
                        });
                    }
                });

            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {

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

function ConsultarCuentasContablesCargosSelectUtileria() {
    var id_cargo_contable = $("#select_cargo_editar").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarCuentasContablesCargosSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            $(".select_cuenta").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function BuscarInformacionRequisicionUtileria() {
    var folio_requisicion = $("#folio_requi_buscar").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000,
        url: "../UTILERIAS/BuscarInformacionRequisicionUtileria",
        data: { folio_requisicion: folio_requisicion },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response != "") {
                $("#div_informacion_requisicion").html(response);
                $("#tr_update_header_requi").css("display", "table-row");


                $("#div_cancelar_requi").css("display", "inline");

            }
            else {
                iziToast.warning({
                    title: 'REQUISICIÓN NO ENCONTRADA',
                    message: '',
                });
                $("#div_cancelar_requi").css("display", "none");
                LimpiarUtileriaRequisicion();
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AgregarArticuloDetalleUtileria(id_requi_d, count_tabla, id_articulo, articulo, cantidad, precio, unidad_medida, id_unidad_medida, observacion, id_centro_g, cargo_contable,
    id_cargo_contable, id_cuenta_contable_g, new_cotizacion, id_tipo_requi, cve_prov, id_cotizacion, id_tipo_moneda, cuenta_contable, clave_articulo, no_cta) {
    cargo_contable = cargo_contable + "  |  " + cuenta_contable + " " + no_cta;
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/AgregarArticuloDetalleUtileria",
        data: {
            id_requi_d: id_requi_d,
            count_tabla: count_tabla,
            id_articulo: id_articulo,
            articulo: articulo,
            cantidad: cantidad,
            precio: precio,
            unidad_medida: unidad_medida,
            id_unidad_medida: id_unidad_medida,
            observacion: observacion,
            id_centro_g: id_centro_g,
            cargo_contable: cargo_contable,
            id_cargo_contable: id_cargo_contable,
            id_cuenta_contable_g: id_cuenta_contable_g,
            new_cotizacion: new_cotizacion,
            id_tipo_requi: id_tipo_requi,
            cve_prov: cve_prov,
            id_cotizacion: id_cotizacion,
            id_tipo_moneda: id_tipo_moneda,
            clave_articulo: clave_articulo
        },
        success: function (result) {
            count_tabla++;
            $(".input_detalle").val('');
            $("#thead_requi_articulos").append(result);

            $("#unidad_medida").text('');
            $("#id_articulos").text('');
            $(".input_valid").each(function () {
                $(this).css("border", "");
            });
            $(".input_validos").each(function () {
                $(this).css("border", "");
            });
            //$("#cuentas").text('');
            $("#unidad_medida").text('');
            id_cotizacion_servicio = 0;
            $("#check_new_cotizacion").prop("checked", false);
            $("#div_cotizaciones_proveedor").html("");
            ShowHideNewCotizacion();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function EditarArticuloRequisicionUtileria(id_requi_d, id_cargo, id_cuenta, cantidad, observacion, nombre_articulo) {
    $("#btn_actualizar_articulo_editar").attr('onclick', "ActualizarArticuloRequisicion(" + id_requi_d + ")");

    $("#select_cargo_editar").find('option[value=' + id_cargo + ']').prop('selected', 'selected');
    ConsultarCuentasContablesCargosSelectUtileria();

    $("#select_cuenta_editar").find('option[value=' + id_cuenta + ']').prop('selected', 'selected');
    $("#cantidad_articulo_editar").val(cantidad);
    $("#obsercacion_articulo_editar").val(observacion);
    $("#txt_nombre_articulo_ediar").text(nombre_articulo);

    $("#m_editar_articulo_utileria").modal("show");
}

function ActualizarArticuloRequisicion(id_requi_d) {
    var cantidad = $("#cantidad_articulo_editar").val();
    var id_cargo = $("#select_cargo_editar").val();
    var id_cuenta = $("#select_cuenta_editar").val();
    var obs = $("#obsercacion_articulo_editar").val();

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
                        async: false,
                        url: "../UTILERIAS/ActualizarArticuloRequisicion",
                        data: {
                            id_requi_d: id_requi_d,
                            cantidad: cantidad,
                            id_cargo: id_cargo,
                            id_cuenta: id_cuenta,
                            obs: obs
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == -1) {
                                iziToast.warning({
                                    title: 'EL ARTICULO YA FUE COTIZADO EN SU TOTALIDAD',
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: 'LA CANTIDAD NO PUEDE SER MENOR A LA COTIZADA',
                                    message: '',
                                });
                            }
                            else if (response == 0) {
                                iziToast.success({
                                    title: 'INFORMACIÓN ACTUALIZADA CORRECTAMENTE',
                                    message: '',
                                });
                                BuscarInformacionRequisicionUtileria();
                                $("#m_editar_articulo_utileria").modal("hide");
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                            iziToast.warning({
                                title: 'AVISO',
                                message: 'OCURRIO UN ERROR AL REALIZAR EL PROCESO',
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

function ActualizarHeaderRequisicion(id_requi_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de modificar esta información?',
        position: 'center',
        buttons: [
            ['<button><b>Si, modificar ahora</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                var concepto = $("#txt_concepto_requi_editar").val();
                var id_prioridad = $("#select_prioridad_edit").val();

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
                                    async: false,
                                    url: "../UTILERIAS/ActualizarHeaderRequisicion",
                                    data: {
                                        id_requi_g: id_requi_g,
                                        concepto: concepto,
                                        id_prioridad: id_prioridad
                                    },
                                    success: function (response) {
                                        jsRemoveWindowLoad();
                                        if (response == -1) {
                                            iziToast.warning({
                                                title: 'OCURRIÓ UN PROBLEMA AL GUARDAR',
                                                message: '',
                                            });
                                        }
                                        else if (response == -2) {
                                            iziToast.warning({
                                                title: 'REQUISICIÓN NO ENCONTRADA',
                                                message: '',
                                            });
                                        }
                                        else if (response == 0) {
                                            iziToast.success({
                                                title: 'INFORMACIÓN ACTUALIZADA CORRECTAMENTE',
                                                message: '',
                                            });
                                        }
                                    },
                                    error: function (xhr, status, error) {
                                        console.error(error);
                                        jsRemoveWindowLoad();
                                        iziToast.warning({
                                            title: 'AVISO',
                                            message: 'OCURRIO UN PROBLEMA AL REALIZAR EL PROCESO',
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
            ['<button>No, cancelar operación</button>', function (instance, toast) {
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

function RegistrarArticuloDetalleUtileria(id_requi_g) {
    $(".input_valid").each(function () {
        if ($(this).val() == "") { $(this).css("border-color", "red"); }
        else { $(this).css("border", ""); }
    });

    var cantidads = $("#cantidad").val();
    var id_articulos = $("#id_articulos").val();
    var nombre_articulo = $("#id_articulo").val();
    var precios = $("#precio").val();
    var id_unidad_medidas = $("#id_unidad_medida").val().split('|')[0];
    var observacions = $("#observacion").val();
    var id_centros_g = $("#id_centro_g_editar").val();
    var id_cargos_contable = $("#select_cargo").val();
    var id_cuentas_contable_g = $("#select_cuenta").val();

    var existe = false;
    $('#articulos_table_solicitud td:first-child').each(function () { if ($(this).text() === id_articulos) { existe = true; } });

    if (existe == true) {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Asegurate de solicitar solo 1 vez el articulo o modifica su cantidad',
        });
    }
    else if (id_articulos == undefined || id_articulos == '') {
        iziToast.warning({
            title: '¡Seleccione un articulo para registrar!',
            message: '',
        });
        $("#id_articulos").css("border-color", "red");
    }
    else if (id_cargos_contable == 'select_cargo') {
        iziToast.warning({
            title: '¡El cargo contable es un campo obligatorio!',
            message: '',
        });
        $("#select_cargo").css("border-color", "red");
    }
    else if (id_cuentas_contable_g == 'selecte_cuenta') {
        iziToast.warning({
            title: '¡La cuenta contable es un campo obligatorio!',
            message: '',
        });
        $("#select_cuenta").css("border-color", "red");
    }
    else if (cantidads == "" || isNaN(cantidads) || cantidads == undefined || cantidads <= 0) {
        iziToast.warning({
            title: '¡Ingrese bien la cantidad del articulo!',
            message: 'Asegurese de que sea una cantidad valida',
        });
        $("#cantidad").css("border-color", "red");
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
            message: '¿Está seguro de agregar este articulo a la requisición?',
            position: 'center',
            buttons: [
                ['<button><b>Si, registrar articulo a requisición</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../REQUISICIONES/GenerarRequiDetallada",
                        data: {
                            id_requi_g: id_requi_g,
                            id_articulos: id_articulos,
                            nombre_art: nombre_articulo,
                            cantidads: cantidads,
                            precios: precios,
                            id_unidad_medidas: id_unidad_medidas,
                            observacions: observacions,
                            id_centros_g: id_centros_g,
                            id_cargos_contable: id_cargos_contable,
                            id_cuentas_contable_g: id_cuentas_contable_g
                        },
                        success: function (response) {
                            if (response != -1) {
                                iziToast.success({
                                    title: 'ARTICULO AGREGADO CORRECTAMENTE REQUISICIÓN',
                                    message: '',
                                });
                                LimpiarFormularioRequi();
                                jsRemoveWindowLoad();
                            }
                            else {
                                iziToast.error({
                                    title: 'OCURRIÓ UN PROBLEMA AL AGREGAR EL ARTICULO A LA REQUISICIÓN',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }, true],
                ['<button>No, cancelar operación</button>', function (instance, toast) {
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

//#region ALMACEN/ENTRADAS
function EliminarRecepcionAlmacenOrdenCompra(id_orden_g, id_mov_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de eliminar la entrada al almacén?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../UTILERIAS/EliminarRecepcionAlmacenOrdenCompra",
                    data: {
                        id_orden_g: id_orden_g,
                        id_mov_g: id_mov_g
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se canceló correctamente la recepcion',
                            });
                            BuscarInformacionRequisicionUtileria();
                        }
                        else if (response = -1) {
                            iziToast.warning({
                                title: 'El proveedor ya subió la factura al portal de proveedores',
                                message: '',
                            });
                        }
                        else if (response = -2) {
                            iziToast.warning({
                                title: 'La orden está desactivada',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al eliminar la recepción',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }, true],
            ['<button>No, regrear</button>', function (instance, toast) {
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


//#region  ORDENES DE COMPRA
function EditarOrdenCompraUtileria(id_orden_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/EditarOrdenCompraUtileria",
        data: { id_orden_g: id_orden_g },
        success: function (response) {
            $("#justificacion_editar_oc").val("");
            $("#btn_actualizar_oc_editar").attr("onclick", "ActualizarOrdenCompraUtileria(" + id_orden_g + ")");
            $("#txt_id_orden_compra_oc").text(id_orden_g);
            $("#div_detalle_oc_editar").html(response);
            $("#m_editar_orden_compra").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function CalcularImporteDetalleOC(id_orden_d) {
    var cantidad = $("#art_oc_cant_" + id_orden_d + "").val();
    var precio = $("#art_oc_prec_" + id_orden_d + "").val();
    if (isNaN(cantidad)) { cantidad = 0; }
    if (isNaN(precio)) { precio = 0; }

    var importe = parseFloat(cantidad) * parseFloat(precio);
    importe = new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 9,
        maximumFractionDigits: 9
    }).format(importe);
    if (parseFloat(importe) <= 0) {
        $("#art_oc_importe_" + id_orden_d + "").text("$0.00");
    }
    else {
        $("#art_oc_importe_" + id_orden_d + "").text("$" + importe);
    }


    var total = 0;
    $(".importes_oc").each(function () {
        total += parseFloat($(this).text().replace(/[^\d.-]/g, ''));
    });

    total = new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 9,
        maximumFractionDigits: 9
    }).format(total);

    if (!isNaN(total) || parseFloat(total) > 0) { $("#art_oc_subtotal").text("Subtotal: $" + total); }
    else { $("#art_oc_subtotal").text("Subtotal: $0.00"); }
}

function ActualizarOrdenCompraUtileria(id_orden_g) {
    var id_proveedor = $("#id_proveedor_editar_oc").val().split('|')[0];
    var nombre_proveedor = $("#id_proveedor_editar_oc").val().split('|')[1];
    var id_ubicacion_entrega = $("#ubicacion_entrega_editar_oc").val();
    var justificacion = $("#justificacion_editar_oc").val();
    var id_articulos = [];
    var cantidades = [];
    var precios = [];
    var count = 0;

    var valid_cantidad = true;
    var valid_precios = true;

    $(".id_articulos_compra").each(function () {
        id_articulos[count] = $(this).attr('id').split('_')[1];
        count++;
    });
    count = 0;
    $(".cantidades_compra").each(function () {
        var valor = $(this).val();
        if (valor == undefined || valor <= 0 || isNaN(valor)) { valid_cantidad = false; }
        else {
            cantidades[count] = $(this).val();
            count++;
        }
    });
    count = 0;
    $(".precios_compra").each(function () {
        var valor = $(this).val();
        if (valor == undefined || valor <= 0 || isNaN(valor)) { valid_precios = false; }
        else {
            precios[count] = $(this).val();
            count++;
        }
    });

    if (id_proveedor == '' || id_proveedor == undefined || nombre_proveedor == undefined) {
        iziToast.warning({
            title: 'INGRESE UN PROVEEDOR VALIDO',
            message: '',
        });
    }
    else if (id_articulos.length <= 0) {
        iziToast.warning({
            title: 'NO SE DETECTARON ARTICULOS PARA ACTUALIZAR',
            message: '',
        });
    }
    else if (valid_cantidad == false) {
        iziToast.warning({
            title: 'ASEGURESE DE QUE TODOS LAS CANTIDADES SEAN VALIDAS',
            message: '',
        });
    }
    else if (valid_precios == false) {
        iziToast.warning({
            title: 'ASEGURESE DE QUE TODOS LOS PRECIOS SEAN VALIDOS',
            message: '',
        });
    }
    else if (cantidades.length == 0) {
        iziToast.warning({
            title: 'CAPTURE TODAS LAS CANTIDADES',
            message: '',
        });
    }
    else if (precios.length == 0) {
        iziToast.warning({
            title: 'CAPTURE TODOS LOS PRECIOS',
            message: '',
        });
    }
    else if (justificacion == "" || justificacion.length <= 10) {
        iziToast.warning({
            title: 'INGRESE UNA JUSTIFICACIÓN DE MAS DE 10 CARACTERES',
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
            zindex: 1050,
            title: 'ADVERTENCIA',
            message: '¿Está seguro de modificar la orden: ' + id_orden_g + ' con el proveedor: ' + nombre_proveedor + '?',
            position: 'center',
            buttons: [
                ['<button><b>Si, modificar orden de compra</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../UTILERIAS/ActualizarOrdenCompraUtileria",
                        data: {
                            id_orden_g: id_orden_g,
                            id_proveedor: id_proveedor,
                            id_ubicacion_entrega: id_ubicacion_entrega,
                            id_articulos: id_articulos,
                            cantidades: cantidades,
                            precios: precios,
                            justificacion: justificacion
                        },
                        success: function (response) {
                            if (response == 0) {
                                iziToast.success({
                                    title: 'ORDEN MODIFICADA CORRECTAMENTE',
                                    message: '',
                                });
                                BuscarInformacionRequisicionUtileria(id_orden_g);
                                $("#m_editar_orden_compra").modal("hide");
                                $("#justificacion_editar_oc").val("");
                            }
                            else if (response == -1) {
                                iziToast.warning({
                                    title: 'LA ORDEN YA FUE IMPORTADA O CANCELADA',
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: 'EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL',
                                    message: '',
                                });
                            }
                            else if (response == -3) {
                                iziToast.warning({
                                    title: 'LA ORDEN DE COMPRA ESTA INACTIVA',
                                    message: '',
                                });
                            }
                            else if (response == -4) {
                                iziToast.warning({
                                    title: 'LA CANTIDAD NO PUEDE SER MAYOR A LA CANTIDAD YA RECIBIDA',
                                    message: '',
                                });
                            }
                            else if (response == -5) {
                                iziToast.warning({
                                    title: 'LA CANTIDAD NO PUEDE SER MAYOR A LA CANTIDAD DE LA ORDEN DE COMPRA',
                                    message: '(Solicite la accion como usuario master para modificarla)',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });

                }, true],
                ['<button>No, cancelar operación</button>', function (instance, toast) {
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

function OnOffArticuloOrdenCompraUtileria(id_orden_g, id_orden_d, modo) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 1050,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de cambiar el estatus del articulo de la orden de compra?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar acción</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../UTILERIAS/OnOffArticuloOrdenCompraUtileria",
                    data: {
                        id_orden_g: id_orden_g,
                        id_orden_d: id_orden_d,
                        modo: modo
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'ORDEN MODIFICADA CORRECTAMENTE',
                                message: '',
                            });
                            BuscarInformacionRequisicionUtileria(id_orden_g);
                            $("#m_editar_orden_compra").modal("hide");
                        }
                        else if (response == -1) {
                            iziToast.warning({
                                title: 'LA ORDEN YA FUE IMPORTADA O CANCELADA',
                                message: '',
                            });
                        }
                        else if (response == -2) {
                            iziToast.warning({
                                title: 'EL PROVEEDOR YA SUBIO LA FACTURA AL PORTAL',
                                message: '',
                            });
                        }
                        else if (response == -3) {
                            iziToast.warning({
                                title: 'LA ORDEN DE COMPRA ESTA INACTIVA',
                                message: '',
                            });
                        }
                        else if (response == -4) {
                            iziToast.warning({
                                title: 'EL ARTICULO YA CUENTA CON UNA RECEPCION',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {
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

function ConsultarCuentasContablesCargosSelectUtileriaOrden() {
    var id_cargo_contable = $("#select_cargo_editar_orden").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarCuentasContablesCargosSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            $("#select_cuenta_editar_orden").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function EditarCargoCuentaArticuloOrdenUtileria(id_orden_d, id_cargo, id_cuenta, nombre_art, id_orden_g) {
    $("#btn_actualizar_articulo_editar_orden").attr('onclick', "ActualizarCargoCuentaArticuloOrdenUtileria(" + id_orden_d + ", " + id_orden_g + ")");
    $("#select_cargo_editar_orden").find('option[value=' + id_cargo + ']').prop('selected', 'selected');
    ConsultarCuentasContablesCargosSelectUtileriaOrden();
    $("#select_cuenta_editar_orden").val(id_cuenta);
    $("#txt_nombre_articulo_ediar_cargo_cta").text(nombre_art);
    $("#m_editar_orden_carg_cta").modal("show");
}
function ActualizarCargoCuentaArticuloOrdenUtileria(id_orden_d, id_orden_g) {
    var id_cargo = $("#select_cargo_editar_orden").val();
    var id_cuenta = $("#select_cuenta_editar_orden").val();
    var justificacion = $("#justificacion_articulo_editar_orden").val().trim();
    if (justificacion.length <= 20) {
        iziToast.warning({
            title: 'Ingrese una justificación de al menos 20 caracteres',
            message: '',
        });
        return;
    }
    if (id_cargo == undefined || id_cuenta == undefined) {
        iziToast.warning({
            title: 'No se ´detectó la nueva cuenta/cargo',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ActualizarCargoCuentaArticuloOrdenUtileria",
        data: {
            id_orden_d: id_orden_d,
            id_cargo: id_cargo,
            id_cuenta: id_cuenta,
            justificacion: justificacion
        },
        success: function (response) {
            if (response == 0) {
                iziToast.success({
                    title: 'Cambio de cuentas correctamente',
                    message: '',
                });
                BuscarInformacionRequisicionUtileria();
                $("#m_editar_orden_carg_cta").modal("hide");
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al modificar la cuenta y cargo',
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


function CancelarRecepcionOrdenCompraUtileria(id_orden_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/CancelarRecepcionOrdenCompraUtileria",
        data: { id_orden_g: id_orden_g },
        success: function (response) {
            if (response == 'True' || response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se cancelo correctamente la recepcion del articulo',
                });
                BuscarInformacionRequisicionUtileria();
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarOrdenDividir(id_orden_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarOrdenDividir",
        data: { id_orden_g: id_orden_g },
        success: function (response) {
            $("#m_dividir_orden_compra").modal("show");
            $("#div_orden_dividir").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function CalcularTotalesOrdenDividir() {
    var total_original = 0;
    $(".precios_orden_dividir_original").each(function () {
        var id_orden_d = $(this).attr("id").split('_')[1];
        var precio = $("#precio_" + id_orden_d + "").text();
        var cantidad_original = parseFloat($("#cant_" + id_orden_d + "").text());
        total_original = total_original + (precio * cantidad_original);
    });
    $("#total_orden_dividir").text(total_original.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' }));

    var total_new = 0;
    $(".precios_orden_dividir_new").each(function () {
        var id_orden_d = $(this).attr("id").split('_')[1];
        var precio = $("#precioNew_" + id_orden_d + "").text();
        var cantidad_new = parseFloat($("#cantNew_" + id_orden_d + "").text())
        total_new = total_new + (precio * cantidad_new);
    });
    $("#total_orden_dividir_new").text(total_new.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' }));

}

function SetControlCaracteresOrdenDividir(acepta_decimal) {
    var input_cantidad = $("#cantidad_orden_dividir");
    input_cantidad.removeClass("solo-numeros");
    input_cantidad.removeClass("solo-letras-numeros-s-espacio-decimal");

    if (acepta_decimal == 1) {
        input_cantidad.addClass("solo-numeros");
    }
    if (acepta_decimal == 0) {
        input_cantidad.addClass("solo-letras-numeros-s-espacio-decimal");
    }
    input_cantidad.val('');
    CaracteresControl();
}

function MoverArticuloDividirOrden() {
    var cantidad_new = $("#cantidad_orden_dividir").val();
    var id_orden_d = $("input[name='radio_articulo_dividir_orden']:checked").val();
    if (id_orden_d == undefined) {
        iziToast.warning({
            title: 'Selecciona al menos 1 artículo para la nueva orden',
            message: '',
        });
        return;
    }
    if (cantidad_new == "" || cantidad_new == undefined) {
        iziToast.warning({
            title: 'Ingresa la cantidad de la nueva orden',
            message: '',
        });
        return;
    }
    var valid_art_repetidos = $("#claveNew_" + id_orden_d + "");
    if (valid_art_repetidos.text() != '') {
        iziToast.warning({
            title: 'El artículo ya está en la nueva orden',
            message: 'Regresélo para actualizar la cantidad',
        });
        return;
    }

    var clave_ori = $("#clave_" + id_orden_d + "").text();
    var articulo_ori = $("#art_" + id_orden_d + "").text();
    var unidad_ori = $("#unid_" + id_orden_d + "").text();
    var precio_ori = $("#precio_" + id_orden_d + "").text();
    var cantidad_ori = $("#cant_" + id_orden_d + "").text().replace(/[^\d.]/g, '');
    if (parseFloat(cantidad_new) > parseFloat(cantidad_ori)) {
        iziToast.warning({
            title: 'Ingrese una cantidad menor a la de la orden original',
            message: '',
        });
        return;
    }

    var cantidad_restante = parseFloat(cantidad_ori) - parseFloat(cantidad_new);
    if (cantidad_restante <= 0) { $("#trArticuloDividirOriginal_" + id_orden_d + "").remove(); }
    else {
        $("#cant_" + id_orden_d + "").text(cantidad_restante.toLocaleString('es-MX'));
        $("#trArticuloDividirOriginal_" + id_orden_d + "").css("background-color", "#ffeb9c");
    }

    var cantidad_add = 0;
    var cantidad_nueva = $("#cantNew_" + id_orden_d + "").text();
    if (cantidad_nueva != "") { cantidad_add = cantidad_add + parseFloat(cantidad_nueva); }
    cantidad_add = cantidad_add + parseFloat(cantidad_new);

    var row = $("<tr class='text-center' id='trArticuloDividir_" + id_orden_d + "'></tr>");
    row.append("<td id='claveNew_" + id_orden_d + "'>" + clave_ori + "</td>");
    row.append("<td id='artNew_" + id_orden_d + "'>" + articulo_ori + "</td>");
    row.append("<td id='unidNew_" + id_orden_d + "'>" + unidad_ori + "</td>");
    row.append("<td id='precioNew_" + id_orden_d + "' class='precios_orden_dividir_new'>" + precio_ori + "</td>");
    row.append("<td id='cantNew_" + id_orden_d + "'>" + cantidad_add.toLocaleString('es-MX') + "</td>");
    var btn_quitar_art = $("<button>").addClass("btn btn btn_beta_lineal_danger").html("<i class='fa fa-remove'></i>").on("click", function () { RegresarArticuloOrdenDividir(id_orden_d, clave_ori, articulo_ori, unidad_ori, precio_ori); });
    row.append($("<td>").attr("id", "regresarNew_" + id_orden_d).append(btn_quitar_art));

    var tbody = $("#tbody_orden_dividir");
    tbody.append(row);

    CalcularTotalesOrdenDividir();
    $("#cantidad_orden_dividir").val('');
}

function RegresarArticuloOrdenDividir(id_orden_d, clave_ori, articulo_ori, unidad_ori, precio_ori) {
    var cantidad_regreso = parseFloat($("#cantNew_" + id_orden_d + "").text());
    var td_cantidad_original = $("#cant_" + id_orden_d + "");

    if (td_cantidad_original.text() != "") {
        $("#trArticuloDividir_" + id_orden_d + "").remove();
        $("#trArticuloDividirOriginal_" + id_orden_d + "").css("background-color", "");
        cantidad_regreso = cantidad_regreso + parseFloat(td_cantidad_original.text());
        td_cantidad_original.text(cantidad_regreso.toFixed(2));
        CalcularTotalesOrdenDividir();
    }
    else {
        $("#trArticuloDividirOriginal_" + id_orden_d + "").remove();
        $("#trArticuloDividir_" + id_orden_d + "").remove();

        td_cantidad_original.text(cantidad_regreso.toFixed(2));
        var tbody = $("#tbody_orden_dividir_original");
        var row = $("<tr class='text-center' id='trArticuloDividirOriginal_" + id_orden_d + "'></tr>");
        row.append("<td><input type='radio' value='" + id_orden_d + "' name='radio_articulo_dividir_orden' class='form-control' style='height: 20px;' /></td>");
        row.append("<td id='clave_" + id_orden_d + "'>" + clave_ori + "</td>");
        row.append("<td id='art_" + id_orden_d + "'>" + articulo_ori + "</td>");
        row.append("<td id='unid_" + id_orden_d + "'>" + unidad_ori + "</td>");
        row.append("<td id='precio_" + id_orden_d + "' class='precios_orden_dividir_original'>" + precio_ori + "</td>");
        row.append("<td id='cant_" + id_orden_d + "'>" + cantidad_regreso.toFixed(2) + "</td>");
        tbody.append(row);
        CalcularTotalesOrdenDividir();
    }
}

function GenerarNuevaOrdenDividir(id_orden_g) {
    var id_orden_d_original = [];
    var id_orden_d_new = [];

    var cantidades_original = [];
    var cantidades_new = [];

    var count = 0;
    $(".precios_orden_dividir_original").each(function () {
        var id_orden_d = $(this).attr("id").split('_')[1];
        id_orden_d_original[count] = id_orden_d;

        var cantidad = $("#cant_" + id_orden_d + "").text().replace(/[^\d.]/g, '');
        if (cantidad != '') { cantidades_original[count] = cantidad; }
        count++;
    });
    if (id_orden_d_original.length == 0) {
        iziToast.warning({
            title: "La orden original no puede estar vacía",
            message: '',
        });
        return;
    }

    count = 0;
    $(".precios_orden_dividir_new").each(function () {
        var id_orden_d = $(this).attr("id").split('_')[1];
        id_orden_d_new[count] = id_orden_d;
        var cantidad_new = $("#cantNew_" + id_orden_d + "").text().replace(/[^\d.]/g, '');
        if (cantidad_new != '') { cantidades_new[count] = cantidad_new; }
        count++;
    });
    if (id_orden_d_new.length == 0) {
        iziToast.warning({
            title: "Ingrese el detalle de la nueva orden",
            message: '',
        });
        return;
    }
    if (id_orden_d_new.length != cantidades_new.length) {
        iziToast.warning({
            title: "Ingrese el detalle de la nueva orden",
            message: '',
        });
        return;
    }
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999999999,
        title: 'ASVERTENCIA',
        message: '¿Está seguro de generar una orden nueva?',
        position: 'center',
        buttons: [
            ['<button><b>Si, generar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../UTILERIAS/GenerarNuevaOrdenDividir",
                    data: {
                        id_orden_g: id_orden_g,
                        id_orden_d_original: id_orden_d_original,
                        cantidades_original: cantidades_original,
                        id_orden_d_new: id_orden_d_new,
                        cantidades_new: cantidades_new
                    },
                    success: function (response) {
                        if (response > 0) {
                            $("#m_dividir_orden_compra").modal("hide");
                            iziToast.success({
                                title: "Orden generada correctamente: #" + response + "",
                                message: '',
                            });
                            BuscarInformacionRequisicionUtileria();
                        }
                        else if (response == -1) {
                            iziToast.error({
                                title: "Ocurrió un error al generar la nueva orden",
                                message: '',
                            });
                        }
                        else if (response == -2) {
                            iziToast.error({
                                title: "La orden ya fue importada o cancelada",
                                message: '',
                            });
                        }
                        else if (response == -3) {
                            iziToast.error({
                                title: "El proveedor ya subió la factura al portal",
                                message: '',
                            });
                        }
                        else if (response == -4) {
                            iziToast.error({
                                title: "La orden de compra está desactivada",
                                message: '',
                            });
                        }
                        else if (response == -5) {
                            iziToast.error({
                                title: "La cantidad de los artículos nuevos no coincide con los artículos nuevos",
                                message: '',
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


//#region MOVER ARTÍCULOS

function ConsultarOrdenMoverArticulos(id_requisicion_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarOrdenMoverArticulos",
        data: { id_requisicion_g: id_requisicion_g },
        success: function (response) {
            $("#div_orden_mover_art").html(response);
            $("#m_div_orden_mover_art").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarArticulosOrdenChecksMover(modo) {
    var id_orden_g = 0;
    if (modo == 1) { id_orden_g = $("#select_mover_orden_1").val(); }
    if (modo == 2) { id_orden_g = $("#select_mover_orden_2").val(); }

    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarArticulosOrdenChecksMover",
        data: {
            id_orden_g: id_orden_g,
            modo: modo
        },
        success: function (response) {
            if (modo == 1) { $("#div_mover_orden_1").html(response); }
            if (modo == 2) { $("#div_mover_orden_2").html(response); }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function MoverArticulosOrden(modo, id_requisicion_g) {
    var id_orden_d_1 = $("#select_mover_orden_1").val();
    var id_orden_d_2 = $("#select_mover_orden_2").val();
    if (id_orden_d_1 == id_orden_d_2) {
        iziToast.warning({
            title: "Las ordenes no pueden ser iguales",
            message: '',
        });
        return;
    }
    var id_orden_afectar = 0;
    var id_orden_base = 0;
    var id_ordenes_d = [];
    var count = 0;
    var checks_orden;
    if (modo == 1) { checks_orden = "check_mover_orden_1"; id_orden_afectar = id_orden_d_2; id_orden_base = id_orden_d_1 }
    if (modo == 2) { checks_orden = "check_mover_orden_2"; id_orden_afectar = id_orden_d_1; id_orden_base = id_orden_d_2; }
    $("input[name='" + checks_orden + "']:checked").each(function () {
        id_ordenes_d[count] = $(this).val();
        count++;
    });
    if (id_ordenes_d.length == 0) {
        iziToast.warning({
            title: "Selecciona al menos 1 artículo para mover",
            message: '',
        });
        return;
    }
    if (id_orden_afectar == 0) {
        iziToast.warning({
            title: "Selecciona la orden a la cual se moverán los artículos",
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/MoverArticulosOrden",
        data: {
            id_orden_afectar: id_orden_afectar,
            id_ordenes_d: id_ordenes_d,
            id_orden_base: id_orden_base
        },
        success: function (response) {
            if (response == 0) {
                BuscarInformacionRequisicionUtileria(id_requisicion_g);
                iziToast.success({
                    title: "Artículos movidos exitosamente",
                    message: '',
                });
                $("#m_div_orden_mover_art").modal("hide");
            }
            else {
                iziToast.error({
                    title: "Ocurrió un error al mover los artículos",
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            jsRemoveWindowLoad();
            console.error(error);
        }
    });

}

//#endregion

function ValidarArticuloNuevoEditarOrdenCompra() {
    if ($("#articulo_editar_oc_articulo").val().trim().length == 0) { $("#id_articulo_editar_oc_articulo").val(''); }
}

function ConsultarCambiarArticuloOrdenCompra(id_articulo, id_orden_d, nombre_art) {
    $("#id_articulo_editar_oc_articulo").val();
    $("#articulo_editar_oc_articulo").val();

    $("#txt_articulo_orden_compra_cambiar").text(nombre_art);
    $("#btn_editar_oc_articulo").attr("onclick", "CambiarCambiarArticuloOrdenCompra(" + id_orden_d + ", " + id_articulo + ");");
    $("#m_editar_orden_compra_articulo").modal("show");
}

function CambiarCambiarArticuloOrdenCompra(id_orden_d, id_articulo_original) {
    var justificacion = $("#justificacion_editar_oc_articulo").val().trim();

    var id_articulo_nuevo = $("#id_articulo_editar_oc_articulo").val();
    if (id_articulo_nuevo == undefined) {
        iziToast.warning({
            title: "Ingrese un artículo para cambiar",
            message: '',
        });
        return;
    }
    if (id_articulo_original == '' || id_articulo_original == undefined) {
        iziToast.warning({
            title: "No se detectó el artículo original",
            message: '',
        });
        return;
    }
    if (id_articulo_nuevo == '' || id_articulo_nuevo == undefined) {
        iziToast.warning({
            title: "No se detectó el artículo nuevo",
            message: '',
        });
        return;
    }
    if (id_articulo_original == id_articulo_nuevo) {
        iziToast.warning({
            title: "El artículo no puede ser el mismo",
            message: 'FRONTEND',
        });
        return;
    }
    if (justificacion.length <= 20) {
        iziToast.warning({
            title: "Ingrese una justificación de más de 20 caracteres",
            message: '',
        });
        return;
    }

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
                        async: false,
                        url: "../UTILERIAS/CambiarCambiarArticuloOrdenCompra",
                        data: {
                            id_orden_d: id_orden_d,
                            id_articulo_original: id_articulo_original,
                            id_articulo_nuevo: id_articulo_nuevo,
                            justificacion: justificacion
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: "Cambio de artículo realizado correctamente",
                                    message: '',
                                });
                                $("#m_editar_orden_compra_articulo").modal("hide");
                                BuscarInformacionRequisicionUtileria();
                            }
                            else if (response == -1) {
                                iziToast.warning({
                                    title: "No se encontró el detalle de la orden",
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: "El artículo no puede ser el mismo",
                                    message: 'BACKEND',
                                });
                            }
                            else if (response == -3) {
                                iziToast.warning({
                                    title: "El proveedor ya subió la factura al portal",
                                    message: '',
                                });
                            }
                            else if (response == -4) {
                                iziToast.warning({
                                    title: "El artículo ya fue recepcionado",
                                    message: '',
                                });
                            }
                            else if (response == -5) {
                                iziToast.warning({
                                    title: "La orden ya fue importada o cancelada",
                                    message: '',
                                });
                            }
                            else if (response == -6) {
                                iziToast.warning({
                                    title: "La orden de compra está inactiva",
                                    message: '',
                                });
                            }
                            else if (response == -7) {
                                iziToast.warning({
                                    title: "El artículo cuenta con recepciones",
                                    message: '',
                                });
                            }
                            else if (response == -8) {
                                iziToast.warning({
                                    title: "El artículo nuevo no fué encontrado",
                                    message: '',
                                });
                            }
                            else {
                                iziToast.error({
                                    title: "Ocurrió un error al realizar el cambio de artículo",
                                    message: '',
                                });
                            }


                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                            iziToast.error({
                                title: "Ocurrió un error al realizar el cambio de artículo",
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


//#endregion

