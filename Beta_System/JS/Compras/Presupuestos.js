function ConsultarPresupuestosCargo() {
    var id_cargo_contable = $("#id_cargo_contable").val();
    var id_anio = $("#id_anio_presupuesto").val();
    var tipo_vista = $("input[name='tipo_vista_presupuestos']:checked").val();
    if (tipo_vista == undefined || tipo_vista == "") {
        iziToast.warning({
            title: 'Seleccione el tipo de vista',
            message: '',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../COMPRAS/ConsultarPresupuestosCargo",
        data: {
            id_cargo_contable: id_cargo_contable,
            id_anio: id_anio,
            tipo_vista: tipo_vista
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_presupuestos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_presupuestos").html(response);
            $('#datatable_presupuestos').DataTable({
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
            });


            $(".divEditable").keypress(function (e) {
                // Permitir números y el punto decimal (código 46)
                if (e.which !== 46 && isNaN(String.fromCharCode(e.which))) {
                    e.preventDefault();
                }
            });

            var inputField = $('.divEditable');
            inputField.on('input', function () {
                var inputValue = $(this).val();
                var newValue = inputValue.replace(/[^\d.]/g, '');
                newValue = newValue.replace(/(\..*)\./g, '$1');
                $(this).val(newValue);
            });
            CalcularTotalesPorMes();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CalcularTotalPresupuestoCuenta(id_cuenta, count_format) {
    var total = 0;
    var valores = $(".valor_cuenta_" + id_cuenta + "");
    valores.each(function () {
        if ($(this).text() != '' || $(this.text() != undefined)) {
            if ($(this).text() == "2,218,642.62") {
                var hola = "";
            }
            var value = $(this).text().replace(/,/g, '');
            total = total + parseFloat(value);
        }
    });
    var formattedNumber = parseFloat(total.toFixed(2)).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    $("#lbl_total_" + id_cuenta + "").text(formattedNumber);
}

function CalcularTotalesPorMes() {
    var totalGeneral = 0;
    for (var i = 1; i <= 12; i++) {
        var totalMes = 0;
        var valores = $(".mes_" + i);
        valores.each(function () {
            if ($(this).text() != '' || $(this.text() != undefined)) {
                totalMes = totalMes + parseFloat($(this).text().replace(/,/g, ''));
            }
        });
        var formattedNumber = parseFloat(totalMes.toFixed(2)).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        $("#m_" + i).text(formattedNumber);
        totalGeneral += totalMes;
    }
    var formattedNumberTotal = parseFloat(totalGeneral.toFixed(2)).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    $("#mtotal_ptto").text(formattedNumberTotal);
}

function CalcultarTotalGeneral() {
    var total = 0;
    $('#datatable_presupuestos tbody tr').each(function () {
        var precio = parseFloat($(this).find('td:eq(3)').text());
        if (!isNaN(precio)) {
            total += precio;
        }
    });
    $("#total_ptto").val(total);
}

function GuardarPresupuestos() {
    var count = 0;
    var montos = [];
    var id_cuentas = [];
    var meses = [];
    var anio;

    var valores = $(".divEditable");
    valores.each(function () {
        var mes = $(this).attr('id').split('_')[1];
        var id_cuenta = $(this).attr("class").split(' ')[1].split('_')[2];
        var valor = parseFloat($(this).text());

        if (valor != '' || valor != undefined) {
            if (isNaN(valor)) { valor = 0; }
            else { valor = parseFloat($(this).text()); }
        }
        anio = $(this).attr('name');;
        meses[count] = mes;
        id_cuentas[count] = id_cuenta;
        montos[count] = valor;
        count++;
    });

    if (anio == undefined || meses.length == 0 || id_cuentas.length == 0 || montos.length == 0) {
        iziToast.warning({
            title: 'Asegurate de capturar los datos',
            message: 'Presiona el botón de busqueda',
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
            message: '¿Está seguro de guardar los cambios?',
            position: 'center',
            buttons: [
                ['<button><b>Si, Guardar presupuestos</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../COMPRAS/GuardarPresupuestos",
                        data: {
                            anio: anio,
                            id_cuentas: id_cuentas,
                            montos: montos,
                            meses: meses
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == "True") {
                                ConsultarPresupuestosCargo();
                                iziToast.success({
                                    title: 'Presupuestos guardados correctamente',
                                    message: '',
                                });
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un problema al guardar los cambios',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });

                }, true],
                ['<button>No, cancelar opreación</button>', function (instance, toast) {
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

function ConsultarPresupuestosCargoAlmacen() {
    jsShowWindowLoad();
    var id_almacen = $("#id_almacen_presupuesto").val();
    var id_cargo_contable = $("#id_cargo_contable").val();
    var id_anio = $("#id_anio_presupuesto").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/ConsultarPresupuestosCargoAlmacen",
        data: {
            id_almacen: id_almacen,
            id_cargo_contable: id_cargo_contable,
            id_anio: id_anio
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_presupuestos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_presupuestos").html(response);
            $('#datatable_presupuestos').DataTable({
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}]
            });

            $(".divEditable").keypress(function (e) {
                if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
            });

            var inputField = $('.divEditable');
            inputField.on('.divEditable', function () {
                var inputValue = $(this).val();
                var newValue = inputValue.replace(/\s/g, '');
                $(this).val(newValue);
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GuardarPresupuestosAlmacen() {
    var count = 0;
    var montos = [];
    var id_cuentas = [];
    var meses = [];
    var anio;
    var id_almacen = $("#id_almacen_presupuesto").val();
    var valores = $(".divEditable");
    valores.each(function () {
        var mes = $(this).attr('id').split('_')[1];
        var id_cuenta = $(this).attr("class").split(' ')[1].split('_')[2];
        var valor = parseFloat($(this).text());

        if (valor != '' || valor != undefined) {
            if (isNaN(valor)) { valor = 0; }
            else { valor = parseFloat($(this).text()); }
        }
        anio = $(this).attr('name');;
        meses[count] = mes;
        id_cuentas[count] = id_cuenta;
        montos[count] = valor;
        count++;
    });

    if (anio == undefined || meses.length == 0 || id_cuentas.length == 0 || montos.length == 0) {
        iziToast.warning({
            title: 'Asegurate de capturar los datos',
            message: 'Presiona el botón de busqueda',
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
            message: '¿Está seguro de guardar los cambios?',
            position: 'center',
            buttons: [
                ['<button><b>Si, Guardar presupuestos</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../COMPRAS/GuardarPresupuestosAlmacen",
                        data: {
                            id_almacen: id_almacen,
                            anio: anio,
                            id_cuentas: id_cuentas,
                            montos: montos,
                            meses: meses
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == "True") {
                                ConsultarPresupuestosCargoAlmacen();
                                iziToast.success({
                                    title: 'Presupuestos guardados correctamente',
                                    message: '',
                                });
                            }
                            else {
                                iziToast.error({
                                    title: 'Ocurrió un problema al guardar los cambios',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });

                }, true],
                ['<button>No, cancelar opreación</button>', function (instance, toast) {
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

function ReportePresupuestosCargoAlmacen() {
    jsShowWindowLoad();
    var id_almacen = $("#id_almacen_presupuesto").val();
    var id_cargo_contable = $("#id_cargo_contable").val();
    var id_anio = $("#id_anio_presupuesto").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/ReporteConsumosAlmacen",
        data: {
            id_almacen: id_almacen,
            id_cargo_contable: id_cargo_contable,
            id_anio: id_anio
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_presupuestos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_presupuestos").html(response);
            $('#datatable_presupuestos').DataTable({
                keys: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsumoMensualAlman(cuenta, mes,total) {
    var id_almacen = $("#id_almacen_presupuesto").val();
    var id_ano = $("#id_anio_presupuesto").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/ReporteConsumosMensualAlmacen",
        data: {
            id_almacen: id_almacen,
            cuenta: cuenta,
            mes: mes,
            id_ano: id_ano,
            total: total
        },
        success: function (response) {            
            $("#m_cuenta_mensual").modal("show");
            $("#div_cuenta_mensual").html(response);
            $('#tabla_ppto_mensual').DataTable({
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

//#region INVERSIONES
function RegistroInversion() {
    $(".invers_limp").val('');
    $("#m_inversion").modal("show");
    $("#btn_accion_inversion").attr("onclick", "ConfirmacionInversion();");
    $("#lbl_inversion").text("REGISTRO DE INVERSION");
    var hoy = new Date().toISOString().split("T")[0];
    $("#inversion_periodo").attr("min", hoy);
    $("#inversion_periodo").on("change", function () {
        var valorSeleccionado = $(this).val();

        if (valorSeleccionado < fecha) {
            iziToast.warning({
                title: 'Aviso',
                message: 'No puedes seleccionar una fecha anterior a la permitida: ' + hoy,
            });
            $(this).val(fecha);
        }
    });
}

function ConfirmacionInversion() {
    var validador = 0;
    var c_inversion = {};
    var folio = $("#inversion_folio").val();
    var presupuesto = $("#inversion_ppto").val();
    var periodo = $("#inversion_periodo").val();
    var concepto = $("#inversion_concepto").val();

    var hoy = new Date().toISOString().split("T")[0];

    if (!periodo || periodo < hoy) {
        iziToast.warning({
            title: 'Aviso',
            message: 'La fecha debe ser válida y mayor o igual a hoy.',
        });
        $("#inversion_periodo").css("border-color", "red");
        return;
    }

    if (folio.trim() == undefined || folio.length == 0) {
        $("#inversion_folio").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_folio").css("border-color", ""); }

    if (presupuesto.trim() == undefined || presupuesto.length == 0) {
        $("#inversion_ppto").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_ppto").css("border-color", ""); }

    if (concepto.trim() == undefined || concepto.length == 0) {
        $("#inversion_concepto").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_concepto").css("border-color", ""); }

    if (validador == 1) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar informacion en los campos obligatorios',
        });
        $("#inversion_periodo").css("border-color", "");
        return;
    }

    c_inversion.folio_inversion_requi = folio;
    c_inversion.presupuesto = presupuesto;
    c_inversion.fecha_aplicacion = periodo;
    c_inversion.concepto = concepto;

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../COMPRAS/ConfirmacionInversion",
        data: {
            c_inversion: c_inversion,
            periodo: periodo
        },
        success: function (response) {
            if (response == true || response == "True") {
                $("#lbl_inversion").text("CONTROL DE INVERSION")
                $(".invers_limp").val('');
                $(".invers_limp").css("border-color", "");
                $("#inversion_periodo").css("border-color", "");
                ConsultaInversiones();
                iziToast.success({
                    title: 'Exito',
                    message: 'Se registro correctamente la inversion',
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al registrar la inversion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ConsultaInversiones() {
    var anio = $("#anio_inversion_select option:selected").text();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/ConsultaInversiones",
        data: { anio: anio },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_inversiones").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_inversiones").html(response);
            $('#datatable_inversiones').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}]
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function InfoInversion(id_inversion) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/InfoInversion",
        data: { id_inversion: id_inversion },
        success: function (response) {
            if (response != "[]") {
                $("#m_inversion").modal("show");
                $("#lbl_inversion").text("EDICION DE INVERSION");
                var data = $.parseJSON(response);
                if (data.length >= 1) {
                    $("#inversion_folio").val(data[0].folio_inversion_requi);
                    $("#inversion_ppto").val(data[0].presupuesto);
                    var fecha = data[0].fecha_aplicacion.split("T")[0];
                    $("#inversion_periodo").val(fecha);
                    $("#inversion_concepto").val(data[0].concepto);
                    $("#inversion_periodo").attr("min", fecha);
                    $("#inversion_periodo").on("change", function () {
                        var valorSeleccionado = $(this).val();

                        if (valorSeleccionado < fecha) {
                            iziToast.warning({
                                title: 'Aviso',
                                message: 'No puedes seleccionar una fecha anterior a la permitida: ' + fecha,
                            });
                            $(this).val(fecha);
                        }
                    });
                    $("#btn_accion_inversion").attr("onclick", "EdicionInversion('" + data[0].id_compra_inversion_programada + "');");
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: '¡No se encontro informacion!',
                    });
                }
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: '¡No se encontro informacion!',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function EdicionInversion(id_compra_inversion_programada) {
    var validador = 0;
    var c_inversion = {};
    var folio = $("#inversion_folio").val();
    var presupuesto = $("#inversion_ppto").val();
    var periodo = $("#inversion_periodo").val();
    var concepto = $("#inversion_concepto").val();

    var hoy = new Date().toISOString().split("T")[0];

    if (!periodo || periodo < hoy) {
        iziToast.warning({
            title: 'Aviso',
            message: 'La fecha debe ser válida y mayor o igual a hoy.',
        });
        $("#inversion_periodo").css("border-color", "red");
        return;
    }

    if (folio.trim() == undefined || folio.length == 0) {
        $("#inversion_folio").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_folio").css("border-color", ""); }

    if (presupuesto.trim() == undefined || presupuesto.length == 0) {
        $("#inversion_ppto").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_ppto").css("border-color", ""); }

    if (concepto.trim() == undefined || concepto.length == 0) {
        $("#inversion_concepto").css("border-color", "red");
        validador = 1;
    }
    else { $("#inversion_concepto").css("border-color", ""); }

    if (validador == 1) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar informacion en los campos obligatorios',
        });
        $("#inversion_periodo").css("border-color", "");
        return;
    }
    c_inversion.id_compra_inversion_programada = id_compra_inversion_programada;
    c_inversion.folio_inversion_requi = folio;
    c_inversion.presupuesto = presupuesto;
    c_inversion.fecha_aplicacion = periodo;
    c_inversion.concepto = concepto;

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../COMPRAS/EdicionInversion",
        data: {
            c_inversion: c_inversion,
            periodo: periodo
        },
        success: function (response) {
            if (response == true || response == "True") {
                $("#lbl_inversion").text("CONTROL DE INVERSION")
                $(".invers_limp").val('');
                $(".invers_limp").css("border-color", "");
                $("#inversion_periodo").css("border-color", "");
                ConsultaInversiones();
                iziToast.success({
                    title: 'Exito',
                    message: 'Se edito correctamente la inversion',
                });
                $("#m_inversion").modal("hide");
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al editar la inversion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}


function ConsultaInversionesReporte() {
    var anio = $("#anio_inversion_reporte_select option:selected").text();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../COMPRAS/ConsultaInversionesReporte",
        data: { anio: anio },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_inversiones_reporte").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_inversiones_reporte").html(response);
            $('#datatable_inversiones_reporte').DataTable({
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}]
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
//#endregion