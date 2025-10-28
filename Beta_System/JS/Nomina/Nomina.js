$('#fecha_inicio').val(last_week);
$('#fecha_fin').val(today);


function ConsultarEmpleadosDepartamentoSelect() {
    var id_departamento = $("#id_areas_prenomina_consulta").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        data: {
            id_departamento, id_departamento,
            status: 0
        },
        url: "../CATALOGOS/ConsultarEmpleadosDepartamentoSelect",
        success: function (response) {
            jsRemoveWindowLoad();
            $("#id_empleado").html(response);
            $("#select2-id_empleado-container").text('Selecciona una opción');
            $("#id_empleado").val('');
        },
        error: function (xhr, status, error) {
            console.error(error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error en la solicitud'
            });
        }
    });
}




function ObtenerLunesSabadoActual() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerLunesSabadoActual",
        success: function (response) {
            if (response && !response.Error) {
                $("#fecha_inicio_area").text(response.Lunes);
                $("#fecha_fin_area").text(response.Sabado);
            } else {
                iziToast.warning({
                    title: 'Aviso',
                    message: response.Error || 'Respuesta inesperada del servidor'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error en la solicitud'
            });
        }
    });
}

//#region CARGO NOMINA
function RegistroCargo() {
    $("#cargo_nombre").val("");
    $("#cargo_activo").prop('checked', false);
    $("#lbl_cargo_nomina").text('Registro de cargo');
    $("#m_cargo_nomina").modal("show");
    $("#div_nombre_cargo").removeClass("col-md-10").addClass("col-md-12");
    $("#div_activo_nomina").css("display", 'none');
    $("#cargo_confirmar").attr("onclick", "ModalNominaCargo(0, null)");
}

function ObtenerInformacionCargo(id_cargo_nomina) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerInformacionCargo",
        data: {
            id_cargo_nomina: id_cargo_nomina
        },
        success: function (response) {
            if (response != "") {
                $("#lbl_cargo_nomina").text('Edicion de cargo');
                $("#m_cargo_nomina").modal("show");
                $("#div_nombre_cargo").removeClass("col-md-12").addClass("col-md-10");
                $("#div_activo_nomina").css("display", 'block');

                var data = $.parseJSON(response);
                $("#cargo_nombre").val(data[0].nombre_cargo);
                var activo = data[0].activo;
                if (activo == true) {
                    $("#cargo_activo").prop('checked', true);
                }
                else {
                    $("#cargo_activo").prop('checked', false);
                }


                $("#cargo_confirmar").attr("onclick", "ModalNominaCargo(2, " + data[0].id_cargo_nomina + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalNominaCargo(accion, id_cargo_nomina) {
    var cargo_nombre_blanco = $.trim($("#cargo_nombre").val());
    //Agregar
    if (accion == 0) {
        if (cargo_nombre_blanco != "") {
            var nombre_cargo = $("#cargo_nombre").val();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/AgregarCargoNomina",
                data: {
                    nombre_cargo: nombre_cargo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente el cargo',
                        });
                        LimpiarCamposNomina();
                        ConsultarCargoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al registrar el cargo',
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
                message: 'Favor de ingresar el nombre del cargo',
            });
        }
    }
    //Regresar
    if (accion == 1) {
        $("#m_cargo_nomina").modal('hide');
        $("#div_nombre_cargo").toggleClass("col-md-12");
    }
    //Editar
    if (accion == 2) {
        if (cargo_nombre_blanco != "") {
            var nombre_cargo = $("#cargo_nombre").val();
            var activo = false;
            if ($("#cargo_activo").is(':checked')) {
                activo = true;
            }
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/EditarCargoNomina",
                data: {
                    id_cargo_nomina: id_cargo_nomina,
                    nombre_cargo: nombre_cargo,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se edito correctamente el cargo',
                        });
                        LimpiarCamposNomina();
                        ConsultarCargoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al editar el cargo',
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
                message: 'Favor de ingresar el nombre del cargo',
            });
        }
    }
}

function OnOffCargoNomina(id_nomina, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/OnOffCargoNomina",
        data: {
            id_nomina: id_nomina,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarCargoNominaTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarCargoNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarCargoNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#datatable_cargos_nomina").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_cargos").html(response);
            $('#datatable_cargos_nomina').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function LimpiarCamposNomina() {
    $("#cargo_nombre").val("");
    $("#cargo_activo").prop('checked', false);
    $("#m_cargo_nomina").modal('hide');
}
//#endregion


//#region SECCION NOMINA
function RegistroSeccion() {
    $("#seccion_nombre").val("");
    $("#seccion_activo").prop('checked', false);
    $("#lbl_seccion_nomina").text('Registro de seccion');
    $("#m_seccion_nomina").modal("show");
    $("#div_nombre_seccion").removeClass("col-md-10").addClass("col-md-12");
    $("#div_activo_seccion").css("display", 'none');
    $("#seccion_confirmar").attr("onclick", "ModalNominaSeccion(0, null)");
}

function ObtenerInformacionSeccion(id_seccion_nomina) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerInformacionSeccion",
        data: {
            id_seccion_nomina: id_seccion_nomina
        },
        success: function (response) {
            if (response != "") {
                $("#lbl_seccion_nomina").text('Edicion de cargo');
                $("#m_seccion_nomina").modal("show");
                $("#div_nombre_seccion").removeClass("col-md-12").addClass("col-md-10");
                $("#div_activo_seccion").css("display", 'block');

                var data = $.parseJSON(response);
                $("#seccion_nombre").val(data[0].nombre_seccion);
                var activo = data[0].activo;
                if (activo == true) {
                    $("#seccion_activo").prop('checked', true);
                }
                else {
                    $("#seccion_activo").prop('checked', false);
                }


                $("#seccion_confirmar").attr("onclick", "ModalNominaSeccion(2, " + data[0].id_seccion_nomina + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalNominaSeccion(accion, id_seccion_nomina) {
    var seccion_nombre_blanco = $.trim($("#seccion_nombre").val());
    //Agregar
    if (accion == 0) {
        if (seccion_nombre_blanco != "") {
            var nombre_seccion = $("#seccion_nombre").val();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/AgregarSeccionNomina",
                data: {
                    nombre_seccion: nombre_seccion
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente la seccion',
                        });
                        LimpiarSeccionNomina();
                        ConsultarSeccionNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al registrar la seccion',
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
                message: 'Favor de ingresar el nombre de la seccion',
            });
        }
    }
    //Regresar
    if (accion == 1) {
        $("#m_seccion_nomina").modal('hide');
        $("#div_nombre_seccion").toggleClass("col-md-12");
    }
    //Editar
    if (accion == 2) {
        if (seccion_nombre_blanco != "") {
            var nombre_seccion = $("#seccion_nombre").val();
            var activo = false;
            if ($("#seccion_activo").is(':checked')) {
                activo = true;
            }
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/EditarSeccionNomina",
                data: {
                    id_seccion_nomina: id_seccion_nomina,
                    nombre_seccion: nombre_seccion,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se edito correctamente la seccion',
                        });
                        LimpiarSeccionNomina();
                        ConsultarSeccionNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al editar la seccion',
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
                message: 'Favor de ingresar el nombre a la seccion',
            });
        }
    }
}

function OnOffSeccionNomina(id_seccion_nomina, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/OnOffSeccionNomina",
        data: {
            id_seccion_nomina: id_seccion_nomina,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarSeccionNominaTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarSeccionNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarSeccionNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#datatable_seccion_nomina").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_secciones").html(response);
            $('#datatable_seccion_nomina').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function LimpiarSeccionNomina() {
    $("#seccion_nombre").val("");
    $("#seccion_activo").prop('checked', false);
    $("#m_seccion_nomina").modal('hide');
}
//#endregion


//#region DEPARTAMENTO NOMINA
function RegistroDepartamento() {
    $("#departamento_nombre").val("");
    $("#departamento_activo").prop('checked', false);
    $("#lbl_departamento_nomina").text('Registro del departamento');
    $("#m_departamento_nomina").modal("show");
    $("#div_nombre_departamento").removeClass("col-md-10").addClass("col-md-12");
    $("#div_activo_departamento").css("display", 'none');
    $("#departamento_confirmar").attr("onclick", "ModalNominaDepartamento(0, null)");
}

function ObtenerInformacionDepartamento(id_departamento_nomina) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerInformacionDepartamento",
        data: {
            id_departamento_nomina: id_departamento_nomina
        },
        success: function (response) {
            if (response != "") {
                $("#lbl_departamento_nomina").text('Edicion del departamento');
                $("#m_departamento_nomina").modal("show");
                $("#div_nombre_departamento").removeClass("col-md-12").addClass("col-md-10");
                $("#div_activo_departamento").css("display", 'block');

                var data = $.parseJSON(response);
                $("#departamento_nombre").val(data[0].nombre_departamento);
                var activo = data[0].activo;
                if (activo == true) {
                    $("#departamento_activo").prop('checked', true);
                }
                else {
                    $("#departamento_activo").prop('checked', false);
                }


                $("#departamento_confirmar").attr("onclick", "ModalNominaDepartamento(2, " + data[0].id_departamento_nomina + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalNominaDepartamento(accion, id_departamento_nomina) {
    var departamento_nombre_blanco = $.trim($("#departamento_nombre").val());
    //Agregar
    if (accion == 0) {
        if (departamento_nombre_blanco != "") {
            var nombre_departamento = $("#departamento_nombre").val();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/AgregarDepartamentoNomina",
                data: {
                    nombre_departamento: nombre_departamento
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente el departamento',
                        });
                        LimpiarDepartamentoNomina();
                        ConsultarDepartamentoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al registrar el departamento',
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
                message: 'Favor de ingresar el nombre del departamento',
            });
        }
    }
    //Regresar
    if (accion == 1) {
        $("#m_departamento_nomina").modal('hide');
        $("#div_nombre_departamento").toggleClass("col-md-12");
    }
    //Editar
    if (accion == 2) {
        if (departamento_nombre_blanco != "") {
            var nombre_departamento = $("#departamento_nombre").val();
            var activo = false;
            if ($("#departamento_activo").is(':checked')) {
                activo = true;
            }
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/EditarDepartamentoNomina",
                data: {
                    id_departamento_nomina: id_departamento_nomina,
                    nombre_departamento: nombre_departamento,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se edito correctamente el departamento',
                        });
                        LimpiarDepartamentoNomina();
                        ConsultarDepartamentoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al editar el departamento',
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
                message: 'Favor de ingresar el nombre del departamento',
            });
        }
    }
}

function OnOffDepartamentoNomina(id_departamento_nomina, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/OnOffDepartamentoNomina",
        data: {
            id_departamento_nomina: id_departamento_nomina,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarDepartamentoNominaTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarDepartamentoNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarDepartamentoNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#tabla_departamento").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_departamento").html(response);
            $('#tabla_departamento').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
} 

function LimpiarDepartamentoNomina() {
    $("#departamento_nombre").val("");
    $("#departamento_activo").prop('checked', false);
    $("#m_departamento_nomina").modal('hide');
}
//#endregion


//#region CONCEPTO NOMINA
function RegistroConcepto() {
    $("#concepto_nombre").val("");
    $("#concepto_activo").prop('checked', false);
    $("#lbl_concepto_nomina").text('Registro del concepto');
    $("#m_concepto_nomina").modal("show");
    $("#div_nombre_concepto").removeClass("col-md-8").addClass("col-md-10");
    $("#div_activo_concepto").css("display", 'none');
    $("#concepto_confirmar").attr("onclick", "ModalNominaConcepto(0, null)");
}

function ObtenerInformacionConcepto(id_concepto_nomina) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerInformacionConcepto",
        data: {
            id_concepto_nomina: id_concepto_nomina
        },
        success: function (response) {
            if (response != "") {
                $("#lbl_concepto_nomina").text('Edicion del concepto');
                $("#m_concepto_nomina").modal("show");
                $("#div_nombre_concepto").removeClass("col-md-10").addClass("col-md-8");
                $("#div_activo_concepto").css("display", 'block');

                var data = $.parseJSON(response);
                $("#concepto_nombre").val(data[0].nombre_concepto);
                $("#id_concepto_select").val(data[0].tipo_concepto);
                var activo = data[0].activo;
                if (activo == true) {
                    $("#concepto_activo").prop('checked', true);
                }
                else {
                    $("#concepto_activo").prop('checked', false);
                }


                $("#concepto_confirmar").attr("onclick", "ModalNominaConcepto(2, " + data[0].id_concepto_nomina + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalNominaConcepto(accion, id_concepto_nomina) {
    var concepto_nombre_blanco = $.trim($("#concepto_nombre").val());
    //Agregar
    if (accion == 0) {
        if (concepto_nombre_blanco != "") {
            var nombre_concepto = $("#concepto_nombre").val();
            var tipo_concepto = $("#id_concepto_select").val();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/AgregarConceptoNomina",
                data: {
                    nombre_concepto: nombre_concepto,
                    tipo_concepto: tipo_concepto
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente el concepto',
                        });
                        LimpiarConceptoNomina();
                        ConsultarConceptoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al registrar el concepto',
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
                message: 'Favor de ingresar el nombre del concepto',
            });
        }
    }
    //Regresar
    if (accion == 1) {
        $("#m_concepto_nomina").modal('hide');
        $("#div_nombre_concepto").toggleClass("col-md-10");
    }
    //Editar
    if (accion == 2) {
        if (concepto_nombre_blanco != "") {
            var nombre_concepto = $("#concepto_nombre").val();
            var tipo_concepto = $("#id_concepto_select").val();
            var activo = false;
            if ($("#concepto_activo").is(':checked')) {
                activo = true;
            }
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/EditarConceptoNomina",
                data: {
                    id_concepto_nomina: id_concepto_nomina,
                    nombre_concepto: nombre_concepto,
                    tipo_concepto: tipo_concepto,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se edito correctamente el concepto',
                        });
                        LimpiarConceptoNomina();
                        ConsultarConceptoNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al editar el concepto',
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
                message: 'Favor de ingresar el nombre del concepto',
            });
        }
    }
}

function OnOffConceptoNomina(id_concepto_nomina, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/OnOffConceptoNomina",
        data: {
            id_concepto_nomina: id_concepto_nomina,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarConceptoNominaTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarConceptoNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarConceptoNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#datatable_concepto_nomina").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_concepto").html(response);
            $('#datatable_concepto_nomina').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function LimpiarConceptoNomina() {
    $("#concepto_nombre").val("");
    $("#concepto_activo").prop('checked', false);
    $("#m_concepto_nomina").modal('hide');
}
//#endregion


//#region AREA NOMINA
function RegistroAreas() {
    $("#area_nombre").val("");
    $("#area_activo").prop('checked', false);
    $("#lbl_area_nomina").text('Registro del area');
    $("#m_area_nomina").modal("show");
    $("#div_nombre_area").removeClass("col-md-5").addClass("col-md-7");
    $("#div_activo_area").css("display", 'none');
    $("#area_confirmar").attr("onclick", "ModalNominaArea(0, null)");
}

function ObtenerInformacionArea(id_area_nomina) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerInformacionArea",
        data: {
            id_area_nomina: id_area_nomina
        },
        success: function (response) {
            if (response != "") {
                $("#lbl_area_nomina").text('Edicion del area');
                $("#m_area_nomina").modal("show");
                $("#div_nombre_area").removeClass("col-md-7").addClass("col-md-5");
                $("#div_activo_area").css("display", 'block');

                var data = $.parseJSON(response);
                $("#area_nombre").val(data[0].nombre_area);
                $("#id_area_select").val(data[0].id_patron_empresa);
                var activo = data[0].activo;
                if (activo == true) {
                    $("#area_activo").prop('checked', true);
                }
                else {
                    $("#area_activo").prop('checked', false);
                }


                $("#area_confirmar").attr("onclick", "ModalNominaArea(2, " + data[0].id_area_nomina + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalNominaArea(accion, id_area_nomina) {
    var area_nombre_blanco = $.trim($("#area_nombre").val());
    //Agregar
    if (accion == 0) {
        if (area_nombre_blanco != "") {
            var nombre_area = $("#area_nombre").val();
            var id_patron_empresa = $("#id_area_select").val();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/AgregarAreaNomina",
                data: {
                    nombre_area: nombre_area,
                    id_patron_empresa: id_patron_empresa
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente el area',
                        });
                        LimpiarAreaNomina();
                        ConsultarAreasNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al registrar el area',
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
                message: 'Favor de ingresar el nombre del area',
            });
        }
    }
    //Regresar
    if (accion == 1) {
        $("#m_area_nomina").modal('hide');
        $("#div_nombre_area").toggleClass("col-md-10");
    }
    //Editar
    if (accion == 2) {
        if (area_nombre_blanco != "") {
            var nombre_area = $("#area_nombre").val();
            var id_patron_empresa = $("#id_area_select").val();
            var activo = false;
            if ($("#area_activo").is(':checked')) {
                activo = true;
            }
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/EditarAreaNomina",
                data: {
                    id_area_nomina: id_area_nomina,
                    nombre_area: nombre_area,
                    id_patron_empresa: id_patron_empresa,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se edito correctamente el area',
                        });
                        LimpiarAreaNomina();
                        ConsultarAreasNominaTable();
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al editar el area',
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
                message: 'Favor de ingresar el nombre del area',
            });
        }
    }
}

function OnOffAreaNomina(id_area_nomina, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/OnOffAreasNomina",
        data: {
            id_area_nomina: id_area_nomina,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarAreasNominaTable();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarAreasNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarAreasNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#datatable_area_nomina").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_area").html(response);
            $('#datatable_area_nomina').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function LimpiarAreaNomina() {
    $("#area_nombre").val("");
    $("#area_activo").prop('checked', false);
    $("#m_area_nomina").modal('hide');
}
//#endregion


//#region PUESTO NOMINA
function MostrarInfoPuesto(id_puesto) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarInfoPuesto",
        data: {
            id_puesto: id_puesto
        },
        success: function (response) {
            if (response != "") {
                $("#m_puesto_nomina").modal("show");

                var data = $.parseJSON(response);
                $("#puesto_nombre").val(data[0].nombre_puesto);
                $("#salario").val(data[0].salario_diario);
                $("#salario_maximo").val(data[0].salario_diario_max);


                $("#emp_puesto").text("Confirmar");
                $("#emp_puesto").attr("onclick", "ActualizarPuestoMS(" + id_puesto + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegresarPuesto() {
    $("#m_puesto_nomina").modal('hide');
}


function PuestoTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarPuestoNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#tabla_puestos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_puesto").html(response);
            $('#tabla_puestos').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}


function ConsultarPuestosNominaActivosSelect() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarPuestosNominaActivosSelect",
        data: {},
        success: function (response) {
            $(".id_puesto_select").html(response);
            try {
                $(".id_puesto_select").select2({
                    placeholder: 'Selecciona un puesto',
                    //dropdownParent: $("#tabulador")
                });
            }
            catch (ex) { }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//#endregion


//#region DEPARTAMENTO NOMINA
function DepartamentoAccion(modo) {
    //REGISTRAR
    if (modo == 1) {
        LimpiarDepartamentoNomina();
        $("#m_departamento").modal("show");
        $("#emp_departamento").text("Confirmar");
        $("#emp_departamento").attr("onclick", "RegistrarDepartamentoMS()");
    }
}

function RegistrarDepartamentoMS() {
    var departamentos = {};
    var departamento = $("#departamento_nombre").val();
    departamentos.NOMBRE = departamento;
    if (departamento != "" && departamento.trim() != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/RegistrarDepartamentoMS",
            data: {
                departamento: departamentos
            },
            success: function (response) {
                iziToast.success({
                    title: 'Aviso',
                    message: response,
                });
                $("#m_departamento").modal("hide");
                DepartamentoTable();
                LimpiarDepartamento();
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
            message: 'Favor de ingresar un nombre al departamento',
        });
    }
}

function ActualizarDepartamentoMS(clave) {
    var departamentos = {};
    var departamento = $("#departamento_nombre").val();
    departamentos.NOMBRE = departamento;
    departamentos.DEPTO_NO_ID = clave;
    if (departamento != "" && departamento.trim() != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ActualizarDepartamentoMS",
            data: {
                departamento: departamentos
            },
            success: function (response) {
                iziToast.success({
                    title: 'Aviso',
                    message: response,
                });
                $("#m_departamento").modal("hide");
                DepartamentoTable();
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
            message: 'Favor de ingresar un nombre al departamento',
        });
    }
}




function PuestoAccion(modo) {
    //REGISTRAR
    if (modo == 1) {
        LimpiarPuesto();
        $("#m_puesto_nomina").modal("show");
        $("#emp_puesto").text("Confirmar");
        $("#emp_puesto").attr("onclick", "RegistrarPuestoMS()");
    }
}
var validador_puesto = 0;

function ValidarCampoPuesto() {
    validador_puesto = 0;
    $(".obligatorio_puesto").each(function () {
        var texto = $(this).val().trim();
        if (texto === "") {
            $(this).css("border", "1px solid red");
            validador_puesto++;
        }
        else {
            $(this).css("border", "");
        }
    });
}

function LimpiarPuesto() {
    $("#puesto_nombre").val('');
    $("#salario").val('');
    $("#salario_maximo").val('');
    $(".obligatorio_puesto").each(function () {
        $(this).css("border", "");
    });
}
function LimpiarDepartamento() {
    $("#departamento_nombre").val('');
}

function RegistrarPuestoMS() {
    var puestos = {};
    puestos.NOMBRE = $("#puesto_nombre").val();
    puestos.SUELDO_DIARIO = $("#salario").val();
    puestos.SUELDO_DIARIO_MAX = $("#salario_maximo").val();

    ValidarCampoPuesto();


    if (validador_puesto == 0) {
        if ($("#salario").val() <= $("#salario_maximo").val()) {
            jsShowWindowLoad();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/RegistrarPuestoMS",
                data: {
                    puesto: puestos
                },
                success: function (response) {

                    if (response != "YA EXISTE UN PUESTO CON ESE NOMBRE") {
                        iziToast.success({
                            title: 'Aviso',
                            message: response,
                        });
                        $("#m_puesto_nomina").modal("hide");
                        PuestoTable();
                        LimpiarPuesto();
                    }
                    else {
                        iziToast.warning({
                            title: 'Aviso',
                            message: response,
                        });
                    }

                    $(".obligatorio_puesto").each(function () {
                        $(this).css("border", "");
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
                message: 'El salario diario no puede ser mayor al salario maximo',
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar los campos obligatorios',
        });
    }
}

function ActualizarPuestoMS(clave) {
    var puestos = {};
    puestos.PUESTO_NO_ID = clave;
    puestos.NOMBRE = $("#puesto_nombre").val();
    puestos.SUELDO_DIARIO = $("#salario").val();
    puestos.SUELDO_DIARIO_MAX = $("#salario_maximo").val();

    ValidarCampoPuesto();
    if (validador_puesto == 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ActualizarPuestoMS",
            data: {
                puesto: puestos
            },
            success: function (response) {
                iziToast.success({
                    title: 'Aviso',
                    message: response,
                });
                $("#m_puesto_nomina").modal("hide");
                PuestoTable();

                $(".obligatorio_puesto").each(function () {
                    $(this).css("border", "");
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
            message: 'Favor de ingresar los campos obligatorios',
        });
    }
}

function MostrarInfoDepartamento(id_departamento) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarInfoDepartamento",
        data: {
            id_departamento: id_departamento
        },
        success: function (response) {
            if (response != "") {
                $("#m_departamento").modal("show");

                var data = $.parseJSON(response);
                $("#departamento_nombre").val(data[0].nombre_departamento);

                $("#emp_departamento").text("Confirmar");
                $("#emp_departamento").attr("onclick", "ActualizarDepartamentoMS(" + id_departamento + ")");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegresarDepartamentos() {
    $("#m_departamento").modal('hide');
}

function LimpiarDepartamento() {
    $("#departamento_nombre").val("");
    $("#m_departamento").modal('hide');
}

function DepartamentoTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarDepartamentoNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#tabla_departamento").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_departamento").html(response);
            $('#tabla_departamento').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}
//#endregion


//#region REGIMEN PATRONAL
function MostrarInfoRegimenPatronal(id_regimen) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarInfoRegimenPatronal",
        data: {
            id_regimen: id_regimen
        },
        success: function (response) {
            if (response != "") {
                $("#m_regimen_patronal").modal("show");

                var data = $.parseJSON(response);
                $("#clave_rp").val(data[0].id_regimen_patronal);
                $("#numero_rp").val(data[0].numero_regimen_partonal);
                $("#descripcion_rp").val(data[0].descripcion);
                $("#clase_riesgo_sat_rp").val(data[0].clase_riesgo_sat);
                $("#prima_riesgo_rp").val(data[0].prima_riesgo);
                $("#ciudad_rp").val(data[0].id_ciudad);
                $("#isn_rp").val(data[0].id_tipo_isn);
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegresarDepartamento() {
    $("#m_regimen_patronal").modal('hide');
}

function RegimenPatronalTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarRegimenPatronalNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#tabla_regimen_patronal").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_regimen_patronal").html(response);
            $('#tabla_regimen_patronal').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}
//#endregion


//#region EMPLEADO
var numero_edit = 0;
var rfc_edit = "";
function MostrarInfoEmpleado(id_empleado, modo) {
    $("#m_empleados").modal("show");
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarInfoEmpleado",
        data: {
            id_empleado: id_empleado
        },
        success: function (response) {
            ModalEmpleados(modo);
            /*            $("#emp_actualizar").attr("onclick", "ActualizarEmpleadoMS(" + id_empleado + ")");*/
            $("#confirmar_constancia").attr("onclick", "ConfirmarConstanciaFiscal(" + id_empleado + ")");
            //#region CARGAR DATOS
            var data = $.parseJSON(response);

            $("#numero_emp").val(data[0].Numero);
            numero_edit = data[0].Numero;




            if (data[0].formacion == null) {
                $("#selectNivelEscolaridad").val(1);
            }
            else {
                $("#selectNivelEscolaridad").val(data[0].formacion);
            }




            $("#nombre_completo_emp").val(data[0].Nombre_completo);
            $("#apellido_paterno_emp").val(data[0].Apellido_paterno);
            $("#apellido_materno_emp").val(data[0].Apellido_materno);
            $("#nombre_emp").val(data[0].Nombres);
            $("#puesto_emp").val(data[0].Puesto_no_id);
            $("#departamento_emp").val(data[0].Depto_no_id);
            $("#frecuencia_emp").val(data[0].Frepag_id);
            $("#registro_patronal_emp").val(data[0].Reg_patronal_id);

            if (data[0].Es_jefe === true) {
                $("#es_jefe_emp").prop('checked', true);
            } else {
                $("#es_jefe_emp").prop('checked', false);
            }

            $("#forma_pago_emp").val(data[0].Forma_pago);
            $("#contrato_general_emp").val(data[0].Contrato);
            $("#turno_emp").val(data[0].Turno);
            $("#jornada_emp").val(data[0].Jornada);

            /*
            if (data[0].Regimen_fiscal == 2) {
                ("#regimen_emp").val("02");
            }
            */
            $("#contrato_sat_emp").val(data[0].Contrato_sat);
            $("#jornada_sat_emp").val(data[0].Jornada_sat);

            if (data[0].Es_sindicalizado === 'S') {
                $("#sindicalizado_emp").prop('checked', true);
            } else {
                $("#sindicalizado_emp").prop('checked', false);
            }



            $("#estatus_emp").val(data[0].Estatus);
            $("#zona_salario_minimo_emp").val(data[0].Zona_salmin);
            $("#antigüedades_emp").val(data[0].Tabla_antig_id);
            $("#tipo_salario_emp").val(data[0].Tipo_salario);
            $("#porcentaje_integracion_emp").val(data[0].Pctje_integ);
            $("#sueldo_diario_emp").val(data[0].Salario_diario);
            $("#sueldo_hora_emp").val(data[0].Salario_hora);
            $("#sueldo_diario_integrado_emp").val(data[0].Salario_integ);

            if (data[0].Es_dir_admr_gte_gral === true) {
                $("#director_emp").prop('checked', true);
            } else {
                $("#director_emp").prop('checked', false);
            }

            if (data[0].PTU === true) {
                $("#derecho_ptu_emp").prop('checked', true);
            } else {
                $("#derecho_ptu_emp").prop('checked', false);
            }

            if (data[0].IMSS === true) {
                $("#retener_seguro_emp").prop('checked', true);
            } else {
                $("#retener_seguro_emp").prop('checked', false);
            }

            if (data[0].CAS === true) {
                $("#aplicar_subsidio_emp").prop('checked', true);
            } else {
                $("#aplicar_subsidio_emp").prop('checked', false);
            }

            if (data[0].DESHAB_IMPTOS === true) {
                $("#deshabilitar_impuestos_emp").prop('checked', true);
            } else {
                $("#deshabilitar_impuestos_emp").prop('checked', false);
            }

            if (data[0].CALC_ISR_ANUAL === true) {
                $("#calcular_isr_emp").prop('checked', true);
            } else {
                $("#calcular_isr_emp").prop('checked', false);
            }

            $("#direc_domicilio_emp").val(data[0].Calle);
            $("#direc_calle_emp").val(data[0].Nombre_calle);
            $("#direc_no_exterior_emp").val(data[0].Num_exterior);
            $("#direc_colonia_emp").val(data[0].Colonia);
            $("#direc_poblacion_emp").val(data[0].Poblacion);
            $("#direc_referencia_emp").val(data[0].Referencia);
            $("#direc_ciudad_emp").val(data[0].Ciudad_id);
            $("#direc_cp_emp").val(data[0].Codigo_postal);
            $("#direc_telefono1_emp").val(data[0].Telefono1);

            $("#direc_email_emp").val(data[0].Email);
            $("#sexo_emp").val(data[0].Sexo);



            var fecha = new Date(data[0].Fecha_nacimiento);
            var fechaFormatoInput = fecha.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_nacimiento_emp").val(fechaFormatoInput);

            var fechaing = new Date(data[0].Fecha_ingreso);
            var fechaFormatoInputingre = fechaing.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_ingreso_emp").val(fechaFormatoInputingre);

            $("#div_ingreso").css("display", "block");
            $("#estatus_emp").prop("disabled", false);



            $("#lugar_nacimiento_emp").val(data[0].Ciudad_nacimiento_id);
            $("#estado_civil_emp").val(data[0].Estado_civil);
            $("#numero_hijos_emp").val(data[0].Num_hijos);
            $("#nombre_padre_emp").val(data[0].Nombre_padre);
            $("#nombre_madre_emp").val(data[0].Nombre_madre);


            var rfc = data[0].RFC;
            var partesRFC = rfc.split('-');
            if (partesRFC.length > 2) {
                $("#rfc_emp_1").val(partesRFC[0]);
                $("#rfc_emp_2").val(partesRFC[1]);
                $("#rfc_emp_3").val(partesRFC[2]);
            }
            else {
                $("#rfc_emp_1").val(rfc.substring(0, 4));
                $("#rfc_emp_2").val(rfc.substring(4, 10));
                $("#rfc_emp_3").val(rfc.substring(10));
            }

            rfc_edit = data[0].RFC;

            $("#curp_emp").val(data[0].CURP);
            $("#seguridad_social_emp").val(data[0].Reg_imss);
            $("#grupo_pago_emp").val(data[0].Grupo_pago_elect_id);
            $("#tipo_cuenta_emp").val(data[0].Tipo_ctaban_pago_elect);
            $("#numero_cuenta_emp").val(data[0].Num_ctaban_pago_elect);
            //#endregion
            if ($("#forma_pago_emp").val() == "T") {
                $("#grupo_pago_emp").prop("disabled", false);
                $("#tipo_cuenta_emp").prop("disabled", false);
                $("#numero_cuenta_emp").prop("disabled", false);
            }


            $("#sueldo_diario_real_emp").val(data[0].Salario_diario_establo);
            $("#sueldo_hora_real_emp").val(data[0].Salario_hora_establo);


            if (data[0].Salario_diario == data[0].Salario_diario_establo) {
                $("#sueldo_diario_emp").prop("disabled", false);
            }
            else {
                $("#sueldo_diario_emp").prop("disabled", true);
            }
            $("#estatus_emp").prop("disabled", true);
            if (data[0].Path_constancia_fiscal != undefined && data[0].Path_constancia_fiscal != null && data[0].Path_constancia_fiscal != "") {
                $("#link_constancia_fiscal").attr("href", data[0].Path_constancia_fiscal);
                $("#mensaje_contancia").css('display', 'none');
                $("#constancia_existente").css('display', 'block');
            }
            else {
                $("#mensaje_contancia").css('display', 'block');
                $("#constancia_existente").css('display', 'none');
            }


            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/MostrarChecadorEmpleado",
                data: { id_empleado: id_empleado },
                success: function (response) {
                    var checador_datos = response;
                    var partes = checador_datos.split('-');
                    $("#id_checador_emp").text(partes[0]);
                    $("#asosiacion_emp").val(partes[1]);
                    $("#asosiacion_numero_emp").val(partes[2]);
                    $("#div_boton_emp").css("display", "block");
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }


            });
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/DeterminarPorcentajeIntegracion",
                data: { id_empleado: id_empleado },
                success: function (response) {
                    $("#porcentaje_integracion_emp").val(response);
                    CalcularSalarioIntegral();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });

            $("#formacion_empleado_g").text(data[0].formacion_g);

            HistoricoFormacionAcademicaTable();
            NuevoRegistroFormacionAcademica(2);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });

}

function ObtenerUltimoNumero() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerUltimoNumero",
        data: {},
        success: function (response) {
            $("#numero_emp").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModalEmpleados(modo) {    
    $("#m_empleados").modal("show");
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ModalEmpleado",
        data: {},
        success: function (response) {
            if (response != "") {
                $("#m_empleado_nomina").modal('show');
                $("#div_empleado_informacion").html(response);
                CaracteresControl();
                ValidarCamposObligatorios(2);
                $("#porcentaje_integracion_emp").val('1.04931');



                //1-ver , 2-registrar, 3-actualizar
                if (modo == 1) {
                    $("#emp_registrar").css("display", "none");
                    $("#emp_actualizar").css("display", "none");
                    $("#estatus_emp").prop("disabled", true);
                    $("#numero_emp").prop("disabled", false);
                    numero_edit = 0;
                    rfc_edit = "";
                    $("#div_boton_emp").css("display", "none");
                    $("#porcentaje_integracion_emp").prop("disabled", true);


                    $("#constancia_pendiente").css('display','none');
                }
                else if (modo == 2) {
                    $("#constancia_pendiente").css('display', 'block');

                    $("#forma_pago_emp").val('T');
                    $("#emp_registrar").css("display", "block");
                    $("#emp_actualizar").css("display", "none");

                    $("#numero_emp").prop("disabled", true);
                    $("#estatus_emp").prop("disabled", true);
                    $("#div_ingreso").css("display", "none");
                    $("#div_baja").css("display", "none");
                    $("#div_boton_emp").css("display", "block");
                    numero_edit = 0;
                    rfc_edit = "";
                    ObtenerUltimoNumero();
                    $("#jornada_emp").val("8");

                    $("#div_diario_establo").css("display", "none");
                    $("#div_hora_establo").css("display", "none");
                    $("#porcentaje_integracion_emp").prop("disabled", true);

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../NOMINA/PrimerAñoPorcentajeIntegracion",
                        data: {},
                        success: function (response) {
                            $("#porcentaje_integracion_emp").val(response);
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }

                else if (modo == 3) {
                    $("#constancia_pendiente").css('display', 'block');

                    $("#emp_registrar").css("display", "none");
                    $("#emp_actualizar").css("display", "block");

                    $("#div_boton_emp").css("display", "none");
                    $("#numero_emp").prop("disabled", true);
                    $("#div_ingreso").css("display", "show");
                    $("#div_baja").css("display", "none");

                    $("#sueldo_hora_real_emp").prop("disabled", true);
                    $("#sueldo_diario_real_emp").prop("disabled", true);
                    $("#porcentaje_integracion_emp").prop("disabled", true);
                }

                else if(modo == 4) {
                    //$("#emp_registrar").css("display", "none");
                    //$("#emp_actualizar").css("display", "none");
                    //$("#numero_emp").prop("disabled", false);
                }
                ValidarFormaPago();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al mostrar el formulario',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function VincularEmpleadoNomina(id_checador, nombre,numero) {
    $("#id_checador_emp").text(id_checador);
    $("#asosiacion_emp").val(nombre);
    $("#m_lupa_checador").modal('hide');
    $("#asosiacion_numero_emp").val(numero);
}

function CalcularSalarioIntegral() {
    var integral = $("#porcentaje_integracion_emp").val();
    var salario = $("#sueldo_diario_emp").val();
    total = salario * integral;
    total = total.toFixed(2);
    $("#sueldo_diario_integrado_emp").val(total);
}
function CalcularSalarioHora() {
    var salario = $("#sueldo_diario_emp").val();
    var hora = salario / 8;
    $("#sueldo_hora_emp").val(hora);

    var salario = $("#salario_diario_masivo").val();
    var hora = salario / 8;
    $("#salario_hora_masivo").val(hora);

}

function CalcularSalarioHoraReal() {
    var salario = $("#sueldo_diario_real_emp").val();
    var hora = salario / 8;
    $("#sueldo_hora_real_emp").val(hora);
}

function Domicilio() {
    var calle = $("#direc_calle_emp").val();
    var no_ext = $("#direc_no_exterior_emp").val();
    var colonia = $("#direc_colonia_emp").val();
    var poblacion = $("#direc_poblacion_emp").val();

    var direccion = calle + " " + no_ext + ", " + colonia + ", " + poblacion;
    $("#direc_domicilio_emp").val(direccion);
}


function TipoSalarioIntegral() {
    var salario = $("#tipo_salario_emp").val();
    if (salario == 1) {
        $("#porcentaje_integracion_emp").prop("disabled", true);
    }
    else {
        $("#porcentaje_integracion_emp").prop("disabled", false);
    }
    $("#porcentaje_integracion_emp").val('1.04931');
}

function RegresarEmpleado() {
    $("#m_regimen_patronal").modal('hide');
}

function LimpiarDepartamento() {
    $("#clave_rp").val("");
    $("#numero_rp").val("");
    $("#descripcion_rp").val("");
    $("#clase_riesgo_sat_rp").val("");
    $("#prima_riesgo_rp").val("");
    $("#ciudad_rp").val("");
    $("#isn_rp").val("");
    $("#m_regimen_patronal").modal('hide');
}

function EmpleadoTable() {
    jsShowWindowLoad();
    var id_status = $("#estatus_buscar").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/ConsultarEmpleadoNominaTable",
        data: {
            id_status: id_status
        },
        success: function (response) {
            try {
                $("#tabla_nomina_empleados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_empleados").html(response);
            $('#tabla_nomina_empleados').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
    
}


function MensajeDePrueba() {
    var nombre_empleado = $("#nombre_emp").val();
    var sueldo_diario = $("#sueldo_diario_emp").val();
    iziToast.success({
        title: 'Exito',
        message: 'La informacion es' + nombre_empleado,
    });
    iziToast.success({
        title: 'Exito',
        message: 'La informacion es' + sueldo_diario,
    });
}



function limpiarFormulario() {
    var inputsTexto = document.querySelectorAll('.limpiar_input');
    inputsTexto.forEach(function (input) {
        input.value = '';
    });

    var checkboxes = document.querySelectorAll('.limpiar_check');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
    ActualizarDepartamentoMS
    var selects = document.querySelectorAll('.limpiar_select');
    selects.forEach(function (select) {
        select.selectedIndex = 0;
    });

    var datetimeInputs = document.querySelectorAll('.limpiar_datetime');
    var fechaHoy = new Date().toISOString().split('T')[0];
    datetimeInputs.forEach(function (input) {
        input.value = fechaHoy;
    });
    $("#pdf_upload").val('');
    $("#m_empleados").modal('hide');
}

function limpiarFormularioSolicitud() {
    var inputsTexto = document.querySelectorAll('.limpiar_input_s');
    inputsTexto.forEach(function (input) {
        input.value = '';
    });

    var checkboxes = document.querySelectorAll('.limpiar_check_s');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
    ActualizarDepartamentoMS
    var selects = document.querySelectorAll('.limpiar_select_s');
    selects.forEach(function (select) {
        select.selectedIndex = 0;
    });

    var datetimeInputs = document.querySelectorAll('.limpiar_datetime_s');
    var fechaHoy = new Date().toISOString().split('T')[0];
    datetimeInputs.forEach(function (input) {
        input.value = fechaHoy;
    });
    $("#pdf_upload").val('');
    $("#m_empleados").modal('hide');
}

function NombreCompleto() {
    var nombre = $("#nombre_emp").val();
    var paterno = $("#apellido_paterno_emp").val();
    var materno = $("#apellido_materno_emp").val();

    $("#nombre_completo_emp").val(paterno + '/' + materno + ',' + nombre);
}

function RegistrarEmpleadoMS() {

    ValidarCamposObligatorios(1);
    if (campos_obligatorios == 0) {
        ValidarNumero();
        if (validacion_numero == 1) {
            var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
            ValidarRFC();
            if (validacion_rfc == 1) {
                var fecha_nac = $("#fecha_nacimiento_emp").val();
                if (fecha_nac != "") {
                    if ($("#id_checador_emp").text() != '' && $("#asosiacion_emp").val() != '') {
                        //#region DATOS EMPLEADO
                        var empleado = {};
                        empleado.id_empleado = 0;
                        empleado.NUMERO = $("#numero_emp").val();
                        empleado.NOMBRE_COMPLETO = $("#nombre_completo_emp").val();
                        empleado.APELLIDO_PATERNO = $("#apellido_paterno_emp").val();
                        empleado.APELLIDO_MATERNO = $("#apellido_materno_emp").val();
                        empleado.NOMBRES = $("#nombre_emp").val();
                        empleado.REGIMEN = 'S';
                        empleado.PUESTO_NO_ID = $("#puesto_emp").val();
                        empleado.DEPTO_NO_ID = $("#departamento_emp").val();
                        empleado.FREPAG_ID = $("#frecuencia_emp").val();
                        empleado.REG_PATRONAL_ID = $("#registro_patronal_emp").val();

                        if ($("#es_jefe_emp").is(':checked')) {
                            empleado.ES_JEFE = true;
                        }
                        else {
                            empleado.ES_JEFE = false;
                        }

                        empleado.FORMA_PAGO = $("#forma_pago_emp").val();
                        empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();
                        empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                        if ($("#forma_pago_emp").val() == 'T') {
                            //empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();                    
                            //empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                        }
                        else {

                        }

                        empleado.TIPO_CTABAN_PAGO_ELECT = $("#tipo_cuenta_emp").val();
                        empleado.CONTRATO = $("#contrato_general_emp").val();
                        empleado.DIAS_HRS_JSR = 0.00;
                        empleado.TURNO = $("#turno_emp").val();
                        empleado.JORNADA = $("#jornada_emp").val();
                        empleado.REGIMEN_FISCAL = $("#regimen_emp").val();
                        empleado.CONTRATO_SAT = $("#contrato_sat_emp").val();
                        empleado.JORNADA_SAT = $("#jornada_sat_emp").val();

                        if ($("#sindicalizado_emp").is(':checked')) {
                            empleado.ES_SINDICALIZADO = 'S';
                        }
                        else {
                            empleado.ES_SINDICALIZADO = 'N';
                        }

                        /* empleado.FECHA_INGRESO = $("#fecha_ingreso_emp").val();*/
                        empleado.ESTATUS = $("#estatus_emp").val();
                        empleado.ZONA_SALMIN = $("#zona_salario_minimo_emp").val();
                        empleado.TABLA_ANTIG_ID = $("#antigüedades_emp").val();
                        empleado.TIPO_SALARIO = $("#tipo_salario_emp").val();
                        empleado.PCTJE_INTEG = $("#porcentaje_integracion_emp").val();
                        empleado.SALARIO_DIARIO = $("#sueldo_diario_emp").val();
                        empleado.SALARIO_HORA = $("#sueldo_hora_emp").val();
                        empleado.SALARIO_INTEG = $("#sueldo_diario_integrado_emp").val();




                        empleado.SALARIO_DIARIO_ESTABLO = $("#sueldo_diario_emp").val();
                        empleado.SALARIO_HORA_ESTABLO = $("#sueldo_hora_emp").val();




                        if ($("#director_emp").is(':checked')) {
                            empleado.ES_DIR_ADMR_GTE_GRAL = true;
                        }
                        else {
                            empleado.ES_DIR_ADMR_GTE_GRAL = false;
                        }
                        if ($("#derecho_ptu_emp").is(':checked')) {
                            empleado.PTU = true;
                        }
                        else {
                            empleado.PTU = false;
                        }
                        if ($("#retener_seguro_emp").is(':checked')) {
                            empleado.IMSS = true;
                        }
                        else {
                            empleado.IMSS = false;
                        }
                        if ($("#aplicar_subsidio_emp").is(':checked')) {
                            empleado.CAS = true;
                        }
                        else {
                            empleado.CAS = false;
                        }

                        empleado.PENSIONADO = false;

                        if ($("#deshabilitar_impuestos_emp").is(':checked')) {
                            empleado.DESHAB_IMPTOS = true;
                        }
                        else {
                            empleado.DESHAB_IMPTOS = false;
                        }
                        if ($("#calcular_isr_emp").is(':checked')) {
                            empleado.CALC_ISR_ANUAL = true;
                        }
                        else {
                            empleado.CALC_ISR_ANUAL = false;
                        }

                        empleado.CALLE = $("#direc_domicilio_emp").val();
                        empleado.NOMBRE_CALLE = $("#direc_calle_emp").val();
                        empleado.NUM_EXTERIOR = $("#direc_no_exterior_emp").val();
                        empleado.COLONIA = $("#direc_colonia_emp").val();
                        empleado.POBLACION = $("#direc_poblacion_emp").val();
                        empleado.REFERENCIA = $("#direc_referencia_emp").val();
                        empleado.CIUDAD_ID = $("#direc_ciudad_emp").val();
                        empleado.CODIGO_POSTAL = $("#direc_cp_emp").val();
                        empleado.TELEFONO1 = $("#direc_telefono1_emp").val();
                        empleado.EMAIL = $("#direc_email_emp").val();
                        empleado.SEXO = $("#sexo_emp").val();





                        empleado.FECHA_NACIMIENTO = fecha_nac;



                        empleado.CIUDAD_NACIMIENTO_ID = $("#lugar_nacimiento_emp").val();
                        empleado.ESTADO_CIVIL = $("#estado_civil_emp").val();
                        empleado.NUM_HIJOS = $("#numero_hijos_emp").val();
                        empleado.NOMBRE_PADRE = $("#nombre_padre_emp").val();
                        empleado.NOMBRE_MADRE = $("#nombre_madre_emp").val();



                        empleado.RFC = rfc;
                        empleado.CURP = $("#curp_emp").val();
                        empleado.REG_IMSS = $("#seguridad_social_emp").val();

                        //#endregion
                        var id_empleado_checador = $("#id_checador_emp").text();


                        jsShowWindowLoad();
                        $.ajax({
                            type: "POST",
                            async: false,
                            url: "../NOMINA/RegistrarEmpleado",
                            data: {
                                empleado: empleado,
                                id_empleado_checador: id_empleado_checador
                            },
                            success: function (response) {
                                iziToast.success({
                                    title: 'Aviso',
                                    message: response,
                                });
                                validacion_numero = 0;
                                campos_obligatorios = 0;
                                validacion_rfc = 0;

                                $("#estatus_buscar").val("A");


                                EmpleadoTable();
                                $("#m_empleados").modal('hide');
                                $("#id_checador_emp").text('');
                                $("#asosiacion_emp").val('');
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                            }
                        });
                        jsRemoveWindowLoad();
                    }
                    else {
                        iziToast.warning({
                            title: '¡Aviso!',
                            message: 'Es obligatorio asociar el empleado con el checador',
                        });
                    }
                }
                else {
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'La fecha de nacimiento es obligatorio',
                    });
                }
            }
            else {
                $("#rfc_emp_1").css("border", "1px solid red");
                $("#rfc_emp_2").css("border", "1px solid red");
                $("#rfc_emp_3").css("border", "1px solid red");
            }
        }
        else {
            iziToast.warning({
                title: '¡Aviso!',
                message: 'El numero ya esta en uso',
            });
            ObtenerUltimoNumero();
        }
    }
}





function obtenerPrimeraVocal(apellido) {
    var vocales = "aeiouAEIOU";
    for (var i = 1; i < apellido.length; i++) { // Iniciar desde 1 para evitar la primera letra
        if (vocales.includes(apellido[i])) {
            return apellido[i];
        }
    }
    return '';
}

function formatearFecha(fecha) {
    var partesFecha = fecha.split('-');
    var anio = partesFecha[0].slice(2);
    var mes = partesFecha[1].padStart(2, '0');
    var dia = partesFecha[2].padStart(2, '0');
    return anio + mes + dia;
}

function limpiarPalabrasNoClasificables(nombre) {
    var palabrasNoClasificables = ["DE", "DEL", "LAS", "LOS", "Y", "DA", "D", "DES", "DU", "VON", "VAN", "VANDEN", "VANDER"];
    var nombreSinPalabras = nombre.split(' ').filter(function (palabra) {
        return !palabrasNoClasificables.includes(palabra.toUpperCase());
    }).join(' ');
    return nombreSinPalabras;
}

function GenerarRFC() {
    var apellidoPaterno = $("#apellido_paterno_emp").val().trim().toUpperCase();
    var apellidoMaterno = $("#apellido_materno_emp").val().trim().toUpperCase();
    var nombre = $("#nombre_emp").val().trim().toUpperCase();
    var fechaNacimiento = $("#fecha_nacimiento_emp").val().trim();

    // Regla 8
    apellidoPaterno = limpiarPalabrasNoClasificables(apellidoPaterno);
    apellidoMaterno = limpiarPalabrasNoClasificables(apellidoMaterno);
    nombre = limpiarPalabrasNoClasificables(nombre);

    // Regla 1
    var primeraVocal = obtenerPrimeraVocal(apellidoPaterno);
    var primeraLetraMaterno = apellidoMaterno.charAt(0);
    var primeraLetraNombre = nombre.charAt(0);

    if (primeraVocal) {
        var rfc1 = apellidoPaterno.charAt(0) + primeraVocal + primeraLetraMaterno + primeraLetraNombre;
    } else {
        var rfc1 = apellidoPaterno.slice(0, 2) + primeraLetraMaterno + primeraLetraNombre;
    }

    // Regla 5
    var nombrePartido = nombre.split(' ');
    var primerNombre = nombrePartido[0];
    if (primerNombre === "MARIA" || primerNombre === "JOSE") {
        if (nombrePartido.length > 1) {
            primerNombre = nombrePartido[1].charAt(0);
        }
    } else {
        primerNombre = nombre.charAt(0);
    }

    // La primera letra del nombre es correcta
    rfc1 = rfc1.slice(0, 3) + primerNombre;

    // Regla 2
    if (apellidoPaterno.slice(0, 2) === "CH" || apellidoPaterno.slice(0, 2) === "LL") {
        apellidoPaterno = apellidoPaterno.charAt(0);
    }

    // Regla 3
    if (apellidoPaterno.length <= 2) {
        rfc1 = apellidoPaterno + apellidoMaterno.charAt(0) + nombre.slice(0, 2);
    }

    // Regla 4
    if (apellidoPaterno.includes(' ')) {
        apellidoPaterno = apellidoPaterno.split(' ')[0];
    }

    // Regla 7
    if ($("#estado_civil_emp").val() === "CASADA" || $("#estado_civil_emp").val() === "VIUDA") {
        nombre = nombrePartido[0];
    }

    var fechaFormateada = formatearFecha(fechaNacimiento);

    var rfc2 = fechaFormateada;

    var rfc = rfc1 + rfc2;

    // Regla 9
    rfc = rfc.replace(/(LIC|PROF|ING|DR|CNTD)\.?/g, '');

    // Regla 10
    if (rfc.toUpperCase().includes("PUTO")) {
        rfc = rfc.slice(0, -1) + "X";
    }

    $("#rfc_emp_1").val(rfc1);
    $("#rfc_emp_2").val(rfc2);
    //$("#rfc_emp").val(rfc);
}






function ActualizarEmpleadoMS(id_empleado) {
    ValidarCamposObligatorios(1);
    if (campos_obligatorios == 0) {
        var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
        if (rfc_edit == rfc) {
            var fecha_nac = $("#fecha_nacimiento_emp").val();
            if (fecha_nac != "") {


                //#region DATOS EMPLEADO
                var empleado = {};

                empleado.FECHA_INGRESO = $("#fecha_nacimiento_emp").val();

                empleado.EMPLEADO_ID = id_empleado;
                empleado.NOMBRE_COMPLETO = $("#nombre_completo_emp").val();
                empleado.APELLIDO_PATERNO = $("#apellido_paterno_emp").val();
                empleado.APELLIDO_MATERNO = $("#apellido_materno_emp").val();
                empleado.NOMBRES = $("#nombre_emp").val();
                empleado.REGIMEN = 'S';
                empleado.PUESTO_NO_ID = $("#puesto_emp").val();
                empleado.DEPTO_NO_ID = $("#departamento_emp").val();
                empleado.FREPAG_ID = $("#frecuencia_emp").val();
                empleado.REG_PATRONAL_ID = $("#registro_patronal_emp").val();

                if ($("#es_jefe_emp").is(':checked')) {
                    empleado.ES_JEFE = true;
                }
                else {
                    empleado.ES_JEFE = false;
                }


                empleado.FORMA_PAGO = $("#forma_pago_emp").val();
                empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();
                empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                if ($("#forma_pago_emp").val() == 'T') {
                    //empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();                    
                    //empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                }
                else {

                }




                empleado.TURNO = $("#turno_emp").val();
                empleado.TIPO_CTABAN_PAGO_ELECT = $("#tipo_cuenta_emp").val();
                empleado.CONTRATO = $("#contrato_general_emp").val();
                empleado.DIAS_HRS_JSR = 0.00;

                empleado.JORNADA = $("#jornada_emp").val();
                empleado.REGIMEN_FISCAL = $("#regimen_emp").val();
                empleado.CONTRATO_SAT = $("#contrato_sat_emp").val();
                empleado.JORNADA_SAT = $("#jornada_sat_emp").val();

                if ($("#sindicalizado_emp").is(':checked')) {
                    empleado.ES_SINDICALIZADO = 'S';
                }
                else {
                    empleado.ES_SINDICALIZADO = 'N';
                }


                /* empleado.FECHA_INGRESO = $("#fecha_ingreso_emp").val();*/
                empleado.ESTATUS = $("#estatus_emp").val();
                empleado.ZONA_SALMIN = $("#zona_salario_minimo_emp").val();
                empleado.TABLA_ANTIG_ID = $("#antigüedades_emp").val();
                empleado.TIPO_SALARIO = $("#tipo_salario_emp").val();
                empleado.PCTJE_INTEG = $("#porcentaje_integracion_emp").val();
                empleado.SALARIO_DIARIO = $("#sueldo_diario_emp").val();
                empleado.SALARIO_HORA = $("#sueldo_hora_emp").val();
                empleado.SALARIO_INTEG = $("#sueldo_diario_integrado_emp").val();




                empleado.SALARIO_DIARIO_ESTABLO = $("#sueldo_diario_real_emp").val();
                empleado.SALARIO_HORA_ESTABLO = $("#sueldo_hora_real_emp").val();




                if ($("#director_emp").is(':checked')) {
                    empleado.ES_DIR_ADMR_GTE_GRAL = true;
                }
                else {
                    empleado.ES_DIR_ADMR_GTE_GRAL = false;
                }
                if ($("#derecho_ptu_emp").is(':checked')) {
                    empleado.PTU = true;
                }
                else {
                    empleado.PTU = false;
                }
                if ($("#retener_seguro_emp").is(':checked')) {
                    empleado.IMSS = true;
                }
                else {
                    empleado.IMSS = false;
                }
                if ($("#aplicar_subsidio_emp").is(':checked')) {
                    empleado.CAS = true;
                }
                else {
                    empleado.CAS = false;
                }

                empleado.PENSIONADO = false;

                if ($("#deshabilitar_impuestos_emp").is(':checked')) {
                    empleado.DESHAB_IMPTOS = true;
                }
                else {
                    empleado.DESHAB_IMPTOS = false;
                }
                if ($("#calcular_isr_emp").is(':checked')) {
                    empleado.CALC_ISR_ANUAL = true;
                }
                else {
                    empleado.CALC_ISR_ANUAL = false;
                }

                empleado.CALLE = $("#direc_domicilio_emp").val();
                empleado.NOMBRE_CALLE = $("#direc_calle_emp").val();
                empleado.NUM_EXTERIOR = $("#direc_no_exterior_emp").val();
                empleado.COLONIA = $("#direc_colonia_emp").val();
                empleado.POBLACION = $("#direc_poblacion_emp").val();
                empleado.REFERENCIA = $("#direc_referencia_emp").val();
                empleado.CIUDAD_ID = $("#direc_ciudad_emp").val();
                empleado.CODIGO_POSTAL = $("#direc_cp_emp").val();
                empleado.TELEFONO1 = $("#direc_telefono1_emp").val();
                empleado.EMAIL = $("#direc_email_emp").val();
                empleado.SEXO = $("#sexo_emp").val();
                empleado.FECHA_NACIMIENTO = $("#fecha_nacimiento_emp").val();
                empleado.CIUDAD_NACIMIENTO_ID = $("#lugar_nacimiento_emp").val();
                empleado.ESTADO_CIVIL = $("#estado_civil_emp").val();
                empleado.NUM_HIJOS = $("#numero_hijos_emp").val();
                empleado.NOMBRE_PADRE = $("#nombre_padre_emp").val();
                empleado.NOMBRE_MADRE = $("#nombre_madre_emp").val();



                empleado.RFC = rfc;
                empleado.CURP = $("#curp_emp").val();
                empleado.REG_IMSS = $("#seguridad_social_emp").val();

                //#endregion
                var id_empleado_checador = $("#id_checador_emp").text();

                if ($("#asosiacion_emp").val() != "No se encontro al empleado") {

                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../NOMINA/ActualizarEmpleadoMS",
                        data: {
                            empleado: empleado,
                            id_empleado_checador: id_empleado_checador
                        },
                        success: function (response) {
                            iziToast.success({
                                title: 'Aviso',
                                message: response,
                            });
                            validacion_numero = 0;
                            campos_obligatorios = 0;
                            validacion_rfc = 0;
                            EmpleadoTable();
                            numero_edit = 0;
                            rfc_edit = "";
                            $("#m_empleados").modal('hide');
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                    jsRemoveWindowLoad();
                }
                else {
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'Favor de seleccionar una asociacion del checador',
                    });
                }
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'La fecha de nacimiento es obligatorio',
                });
            }
        }
        else {
            ValidarRFC();
            if (validacion_rfc == 1) {
                var fecha_nac = $("#fecha_nacimiento_emp").val();
                if (fecha_nac != "") {
                    //#region DATOS EMPLEADO
                    var empleado = {};

                    empleado.FECHA_INGRESO = $("#fecha_nacimiento_emp").val();

                    empleado.EMPLEADO_ID = id_empleado;
                    empleado.NOMBRE_COMPLETO = $("#nombre_completo_emp").val();
                    empleado.APELLIDO_PATERNO = $("#apellido_paterno_emp").val();
                    empleado.APELLIDO_MATERNO = $("#apellido_materno_emp").val();
                    empleado.NOMBRES = $("#nombre_emp").val();
                    empleado.REGIMEN = 'S';
                    empleado.PUESTO_NO_ID = $("#puesto_emp").val();
                    empleado.DEPTO_NO_ID = $("#departamento_emp").val();
                    empleado.FREPAG_ID = $("#frecuencia_emp").val();
                    empleado.REG_PATRONAL_ID = $("#registro_patronal_emp").val();

                    if ($("#es_jefe_emp").is(':checked')) {
                        empleado.ES_JEFE = true;
                    }
                    else {
                        empleado.ES_JEFE = false;
                    }


                    empleado.FORMA_PAGO = $("#forma_pago_emp").val();
                    empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();
                    empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                    if ($("#forma_pago_emp").val() == 'T') {
                        //empleado.GRUPO_PAGO_ELECT_ID = $("#grupo_pago_emp").val();                    
                        //empleado.NUM_CTABAN_PAGO_ELECT = $("#numero_cuenta_emp").val();
                    }
                    else {

                    }




                    empleado.TURNO = $("#turno_emp").val();
                    empleado.TIPO_CTABAN_PAGO_ELECT = $("#tipo_cuenta_emp").val();
                    empleado.CONTRATO = $("#contrato_general_emp").val();
                    empleado.DIAS_HRS_JSR = 0.00;

                    empleado.JORNADA = $("#jornada_emp").val();
                    empleado.REGIMEN_FISCAL = $("#regimen_emp").val();
                    empleado.CONTRATO_SAT = $("#contrato_sat_emp").val();
                    empleado.JORNADA_SAT = $("#jornada_sat_emp").val();

                    if ($("#sindicalizado_emp").is(':checked')) {
                        empleado.ES_SINDICALIZADO = 'S';
                    }
                    else {
                        empleado.ES_SINDICALIZADO = 'N';
                    }


                    /* empleado.FECHA_INGRESO = $("#fecha_ingreso_emp").val();*/
                    empleado.ESTATUS = $("#estatus_emp").val();
                    empleado.ZONA_SALMIN = $("#zona_salario_minimo_emp").val();
                    empleado.TABLA_ANTIG_ID = $("#antigüedades_emp").val();
                    empleado.TIPO_SALARIO = $("#tipo_salario_emp").val();
                    empleado.PCTJE_INTEG = $("#porcentaje_integracion_emp").val();
                    empleado.SALARIO_DIARIO = $("#sueldo_diario_emp").val();
                    empleado.SALARIO_HORA = $("#sueldo_hora_emp").val();
                    empleado.SALARIO_INTEG = $("#sueldo_diario_integrado_emp").val();

                    if ($("#director_emp").is(':checked')) {
                        empleado.ES_DIR_ADMR_GTE_GRAL = true;
                    }
                    else {
                        empleado.ES_DIR_ADMR_GTE_GRAL = false;
                    }
                    if ($("#derecho_ptu_emp").is(':checked')) {
                        empleado.PTU = true;
                    }
                    else {
                        empleado.PTU = false;
                    }
                    if ($("#retener_seguro_emp").is(':checked')) {
                        empleado.IMSS = true;
                    }
                    else {
                        empleado.IMSS = false;
                    }
                    if ($("#aplicar_subsidio_emp").is(':checked')) {
                        empleado.CAS = true;
                    }
                    else {
                        empleado.CAS = false;
                    }

                    empleado.PENSIONADO = false;

                    if ($("#deshabilitar_impuestos_emp").is(':checked')) {
                        empleado.DESHAB_IMPTOS = true;
                    }
                    else {
                        empleado.DESHAB_IMPTOS = false;
                    }
                    if ($("#calcular_isr_emp").is(':checked')) {
                        empleado.CALC_ISR_ANUAL = true;
                    }
                    else {
                        empleado.CALC_ISR_ANUAL = false;
                    }

                    empleado.CALLE = $("#direc_domicilio_emp").val();
                    empleado.NOMBRE_CALLE = $("#direc_calle_emp").val();
                    empleado.NUM_EXTERIOR = $("#direc_no_exterior_emp").val();
                    empleado.COLONIA = $("#direc_colonia_emp").val();
                    empleado.POBLACION = $("#direc_poblacion_emp").val();
                    empleado.REFERENCIA = $("#direc_referencia_emp").val();
                    empleado.CIUDAD_ID = $("#direc_ciudad_emp").val();
                    empleado.CODIGO_POSTAL = $("#direc_cp_emp").val();
                    empleado.TELEFONO1 = $("#direc_telefono1_emp").val();
                    empleado.EMAIL = $("#direc_email_emp").val();
                    empleado.SEXO = $("#sexo_emp").val();
                    empleado.FECHA_NACIMIENTO = $("#fecha_nacimiento_emp").val();
                    empleado.CIUDAD_NACIMIENTO_ID = $("#lugar_nacimiento_emp").val();
                    empleado.ESTADO_CIVIL = $("#estado_civil_emp").val();
                    empleado.NUM_HIJOS = $("#numero_hijos_emp").val();
                    empleado.NOMBRE_PADRE = $("#nombre_padre_emp").val();
                    empleado.NOMBRE_MADRE = $("#nombre_madre_emp").val();



                    empleado.RFC = rfc;
                    empleado.CURP = $("#curp_emp").val();
                    empleado.REG_IMSS = $("#seguridad_social_emp").val();

                    //#endregion
                    var id_empleado_checador = $("#id_checador_emp").text();

                    if ($("#asosiacion_emp").val() != "No se encontro al empleado") {
                        jsShowWindowLoad();
                        $.ajax({
                            type: "POST",
                            async: false,
                            url: "../NOMINA/ActualizarEmpleadoMS",
                            data: {
                                empleado: empleado,
                                id_empleado_checador: id_empleado_checador
                            },
                            success: function (response) {
                                iziToast.success({
                                    title: 'Aviso',
                                    message: response,
                                });
                                validacion_numero = 0;
                                campos_obligatorios = 0;
                                validacion_rfc = 0;
                                EmpleadoTable();
                                numero_edit = 0;
                                rfc_edit = "";
                                $("#m_empleados").modal('hide');
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                            }
                        });
                        jsRemoveWindowLoad();
                    }
                    else {
                        iziToast.warning({
                            title: '¡Aviso!',
                            message: 'Favor de seleccionar una asociacion del checador',
                        });
                    }

                }
                else {
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'La fecha de nacimiento es obligatorio',
                    });
                }
            }
            else {
                $("#rfc_emp_1").css("border", "1px solid red");
                $("#rfc_emp_2").css("border", "1px solid red");
                $("#rfc_emp_3").css("border", "1px solid red");
            }

        }
    }
}

function ValidarFormaPago() {
    if ($("#forma_pago_emp").val() == "T") {
        $("#grupo_pago_emp").prop("disabled", false);
        $("#tipo_cuenta_emp").prop("disabled", false);
        $("#numero_cuenta_emp").prop("disabled", false);
        $("#numero_cuenta_emp").val('');
    }
    else {

        $("#grupo_pago_emp").prop("disabled", true);
        $("#tipo_cuenta_emp").prop("disabled", true);
        $("#numero_cuenta_emp").prop("disabled", true);
        $("#numero_cuenta_emp").val('');
    }
}


var validacion_numero = 0;
function ValidarNumero() {
    if ($("#numero_emp").val() != '') {
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ValidarNumero",
            data: {
                numero: $("#numero_emp").val()
            },
            success: function (response) {
                if (response == "True" || response == true) {
                    $("#numero_validador").prop("class", "fa fa-check");
                    $("#numero_validador").css("color", "green");
                    validacion_numero = 1;
                }
                else {
                    $("#numero_validador").prop("class", "fa fa-remove");
                    $("#numero_validador").css("color", "red");
                    validacion_numero = 0;
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#numero_validador").prop("class", "fa fa-remove");
        $("#numero_validador").css("color", "red");
    }
}


var validacion_rfc = 0;
function ValidarRFC() {

    var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
    if (rfc_edit == rfc || rfc.length >= 14) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ValidarRFC",
            data: {
                rfc: rfc
            },
            success: function (response) {
                if (response == "True" || response == true) {
                    validacion_rfc = 1;
                }
                else {
                    validacion_rfc = 0;
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'El rfc ingresado ya esta en uso',
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
            title: '¡Aviso!',
            message: 'El rfc debe seguir la estructura xxxx xxxxxx xxx',
        });

        $("#numero_validador").prop("class", "fa fa-remove");
        $("#numero_validador").css("color", "red");
    }
}


var campos_obligatorios = 0;
function ValidarCamposObligatorios(modo) {
    if (modo == 1) {
        campos_obligatorios = 0;
        $(".obligatorio").each(function () {
            var texto = $(this).val().trim();
            if (texto === "") {
                $(this).css("border", "1px solid red");
                campos_obligatorios++;
            }
            else {
                $(this).css("border", "");
            }
        });
        if (campos_obligatorios > 0) {
            iziToast.warning({
                title: '¡Aviso!',
                message: 'Es necesario rellenar los campos obligatorios',
            });
        }
    }
    else {
        campos_obligatorios = 0;
        $(".obligatorio").each(function () {
            $(this).css("border", "");
        });
    }
}
//#endregion


//#region INCIDENCIA
function ConsultarIncidenciasNominaTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ConsultarIncidenciasNominaTable",
        data: {},
        success: function (response) {
            try {
                $("#tabla_incidencias").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_incidencia").html(response);
            $('#tabla_incidencias').DataTable({
                keys: false,
                ordering: false,
                select: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
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
                pagingType: 'simple_numbers',
                pageLength: 10
            });


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ModalIncidencias(modo) {
    if (modo == 1) {
        $("#lista_negra_chk").prop('checked', false);
        $("#empleado_id").text('');
        $("#m_incidencia_nomina").modal("show");
        $("#div_btn_accion").css("display", "block");
        $("#div_empleado_btn").css("display", "block");
        $("#incidencia_emp").prop("disabled", false);
        $("#tipo_salario").val('');
        $("#depta_emp").val('-1');


        $("#tabla_nomina_empleados").dataTable().fnClearTable();
        $("#tabla_nomina_empleados").dataTable().fnDraw();
        $("#tabla_nomina_empleados").dataTable().fnDestroy();
    }
    limpiarFormulario();
    TipoIncidenciaAccion();
}
function TipoIncidenciaAccion() {
    var Incidencia = $("#incidencia_emp").val();
    //CAMBIO DE REGIMEN PATRONAL
    if (Incidencia == 4) {
        $("#div_salarios_incidencias").css("display", "none");
        $("#div_nuevo_patronal").css("display", "block");
        $("#div_empleado").removeClass("col-md-7").addClass("col-md-11");
        $("#div_actual_patronal").removeClass("col-md-4").addClass("col-md-6");
        $("#div_baja_empleado").css("display", "none");
        $("#registro_patronal_emp").prop("disabled", true);
    }
    //BAJA DEL EMPLEADO
    else if (Incidencia == 1) {
        $("#div_baja_empleado").css("display", "block");
        $("#div_salarios_incidencias").css("display", "none");
        $("#div_empleado").removeClass("col-md-11").addClass("col-md-7");
        $("#div_actual_patronal").removeClass("col-md-6").addClass("col-md-4");
        $("#div_nuevo_patronal").css("display", "none");
        $("#registro_patronal_emp").prop("disabled", true);
    }
    //CAMBIO DE SALARIO
    else if (Incidencia == 2) {
        $("#div_salarios_incidencias").css("display", "block");
        $("#div_empleado").removeClass("col-md-11").addClass("col-md-7");
        $("#div_actual_patronal").removeClass("col-md-6").addClass("col-md-4");
        $("#div_nuevo_patronal").css("display", "none");
        $("#registro_patronal_emp").prop("disabled", true);
        $("#div_baja_empleado").css("display", "none");
    }
    //REINGRESO o CAMBIO DE PUESTO
    else if (Incidencia == 3 || Incidencia == 5) {
        $("#div_salarios_incidencias").css("display", "block");
        $("#div_nuevo_patronal").css("display", "block");
        $("#div_empleado").removeClass("col-md-7").addClass("col-md-11");
        $("#div_actual_patronal").removeClass("col-md-4").addClass("col-md-6");
        $("#div_baja_empleado").css("display", "none");
        $("#registro_patronal_emp").prop("disabled", true);
    }
    else {
        $("#registro_patronal_emp").prop("disabled", true);
        $("#div_baja_empleado").css("display", "none");
        $("#div_salarios_incidencias").css("display", "none");
    }
}

function AsignarEstatusEmpleado(estatus) {
    $("#empleado_inasistencia_estatus").text(estatus);
}
function AsignarInfoEmpleado(id_empleado, nombre, puesto, departamento, id_departamento) {

    $("#empleado_inasistencia_id").text(id_empleado);
    $("#empleado_inasistencia_emp").text(nombre);
    $("#empleado_puesto_emp").text(puesto);
    $("#empleado_departamento_emp").text(departamento);
    $("#empleado_depa_inasistencia_id").text(id_departamento);
    $("#m_lupa_empleado").modal("hide");
    MostrarInasistenciaEmpleadoTable(id_empleado, 0, 1);
}
function ValidarExistenciaNomina(id_empleado, id_departamento) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ValidarExistenciaNomina",
        data: {
            id_empleado: id_empleado,
            id_departamento: id_departamento
        },
        success: function (response) {
            jsRemoveWindowLoad();
            return response;
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}





function AsignarIncidenciaEmpleado(id_empleado, nombre, patronal, sueldo_diario, sueldo_hora, sueldo_integral, salario_tipo, puesto, departamento, estatus, lista_negra) {
    if (lista_negra == 0) {
        $("#lista_negra").text(lista_negra);


        $("#empleado_estatus_incidencia").text(estatus);

        $("#empleado_emp").val(nombre);
        $("#empleado_id").text(id_empleado);

        $("#registro_patronal_emp").val(patronal);
        $("#m_lupa_empleado").modal("hide");

        $("#sueldo_diario_actual_emp").val(sueldo_diario);
        $("#sueldo_hora_actual_emp").val(sueldo_hora);
        $("#sueldo_integrado_actual_emp").val(sueldo_integral);

        $("#sueldo_diario_nuevo_emp").val(sueldo_diario);
        $("#sueldo_hora_nuevo_emp").val(sueldo_hora);
        $("#sueldo_integrado_nuevo_emp").val(sueldo_integral);

        if (salario_tipo == '0') {
            $("#tipo_salario").val('Fijo');
        }
        if (salario_tipo == '1') {
            $("#tipo_salario").val('Variable');
        }
        if (salario_tipo == '2') {
            $("#tipo_salario").val('Mixto');
        }


        $("#puesto_emp_anterior").prop("disabled", true);
        $("#departamento_anterior").prop("disabled", true);

        $("#puesto_emp_anterior").val(puesto);
        $("#departamento_anterior").val(departamento);
        $("#puesto_emp_nuevo").val(puesto);
        $("#departamento_emp_nuevo").val(departamento);



        $.ajax({
            type: "POST",
            async: true,
            url: "../NOMINA/DeterminarPorcentajeIntegracion",
            data: {
                id_empleado: id_empleado
            },
            success: function (response) {
                $("#porcentaje_integracion").val(response);
                jsRemoveWindowLoad();
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
            message: "El empleado no puede reincorporarse a la empresa, su reingreso está prohibido.",
        });
    }
}



function CalcularCambioSalarial() {
    var salario_nuevo = $("#sueldo_diario_nuevo_emp").val();

    var integral = $("#porcentaje_integracion").val();
    total = salario_nuevo * integral;
    total = total.toFixed(2);
    $("#sueldo_integrado_nuevo_emp").val(total);
    //HORA
    var hora = salario_nuevo / 8;
    $("#sueldo_hora_nuevo_emp").val(hora);


}

function EmpleadoModal(modo) {
    $("#m_lupa_empleado").modal("show");
    $("#buscador_empleado").attr("onclick", "LupaEmpleados(" + modo + ")");    
    if (modo == 2) {
        $("#filtro_lupa").css('display','none');
    }
}

function AsignarLupaEmpleado() {
    $("#m_lupa_empleado").modal("show");
    $("#buscador_empleado").attr("onclick", "LupaEmpleadosFiltros()");
    $("#filtro_lupa").css('display', 'block');
}

function LupaEmpleados(modo) {
    var departamento = $("#depta_emp").val();
    if (departamento >= 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/LupaEmpleados",
            data: {
                departamento: departamento,
                modo:modo
            },
            success: function (response) {
                /*$("#m_lupa_empleado").modal('show');*/
                try {
                    $("#tabla_nomina_empleados").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_lupa_empleados").html(response);
                $('#tabla_nomina_empleados').DataTable({
                    keys: true,
                    ordering: false,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: !0,
                    pagingType: 'simple_numbers',
                    pageLength: 10
                });
                jsRemoveWindowLoad();
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
            message: "Favor de seleccionar un area",
        });
    }
}
function LupaEmpleadosFiltros() {
    var departamento = $("#depta_emp").val();
    var estatus = $("#estatus_emp").val();
    if (departamento >= 0) {
        if (estatus != '') {
            jsShowWindowLoad();
            $.ajax({
                type: "POST",
                async: false,
                url: "../NOMINA/LupaEmpleadosFiltros",
                data: {
                    departamento: departamento,
                    estatus: estatus
                },
                success: function (response) {
                    jsRemoveWindowLoad();
                    try {
                        $("#tabla_nomina_empleados").dataTable().fnDestroy();
                    } catch (e) { }
                    $("#div_lupa_empleados").html(response);
                    $('#tabla_nomina_empleados').DataTable({
                        keys: true,
                        ordering: false,
                        paging: true,
                        dom: "Bfrtip",
                        scrollCollapse: false,
                        buttons: [{}],
                        responsive: !0,
                        pagingType: 'simple_numbers',
                        pageLength: 10
                    });
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
                message: "Favor de seleccionar un estatus",
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: "Favor de seleccionar un area",
        });
    }
}

function ValidarRegistroIncidencia(estatus) {
    var incidencia_tipo = $("#incidencia_emp").val();
    switch (estatus) {
        case 'B':
            if (incidencia_tipo == 3) {
                return 1;
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: "El empleado no esta activo, solo puede tener incidencia reingreso"
                });
                return 2;
            }
            break;
        case 'I':
            if (incidencia_tipo == 1) {
                return 1;
            }
            iziToast.warning({
                title: 'Aviso',
                message: "El empleado no esta activo, solo puede tener incidencia baja"
            });
            return 2;
            break;
        case 'A':
            return 1;
            break;
        default:
            return 2;
            break;
    }
}
function ConfirmarIncidencia() {
    var negra = $("#lista_negra").text();
    var obligatorio = 0;
    var estatus_empleado = $("#empleado_estatus_incidencia").text();
    var empleado_id = $("#empleado_id").text();
    var tipo = $("#incidencia_emp").val();
    if (empleado_id != "" && empleado_id != " ") {
        if (ValidarRegistroIncidencia(estatus_empleado) == 1) {
            //BAJA
            var incidencias = {};
            if (tipo == 1) {

                incidencias.INCIDENCIA_ID = $("#incidencia_emp").val();
                incidencias.EMPELADO_ID = $("#empleado_id").text();
                incidencias.REG_PATRONAL_ID = $("#registro_patronal_emp").val();
                incidencias.FECHA = $("#fecha_incidencia_emp").val();
                incidencias.TIPO = tipo;
                /*incidencias.CASUSA_BAJA = $("#causa_baja_empleado").val(); */
                incidencias.CASUSA_BAJA = 0;
                incidencias.OBSERVACIONES = $("#causa_baja_empleado option:selected").text() + "- " +$("#observaciones_incidencia_baja").val();


                incidencias.SALARIO_DIARIO = $("#sueldo_diario_actual_emp").val();
                incidencias.SALARIO_HORA = $("#sueldo_hora_actual_emp").val();
                incidencias.SALARIO_INTEG = $("#sueldo_integrado_actual_emp").val();

                incidencias.SALARIO_DIARIO_NUEVO = 0;
                incidencias.SALARIO_HORA_NUEVO = 0;
                incidencias.SALARIO_INTEG_NUEVO = 0;
                incidencias.PCTJE_INTEG = $("#porcentaje_integracion").val();
                incidencias.ID_PUESTO_ACTUAL = $("#puesto_emp_anterior").val();
                incidencias.ID_PUESTO_NUEVO = 0;
                incidencias.ID_DEPARTAMENTO_ACTUAL = $("#departamento_anterior").val();
                incidencias.ID_DEPARTAMENTO_NUEVO = 0;
                incidencias.NUEVO_REG_PATRONAL_ID = 0;


                if ($("#lista_negra_chk").is(':checked')) {
                    incidencias.LISTA_NEGRA = true;
                }
                else {
                    incidencias.LISTA_NEGRA = false;
                }

            }
            //CAMBIO DE SALARIO - REINGRESO - CAMBIO DE PUESTO
            else if (tipo == 2 || tipo == 3 || tipo == 5) {
                if ($("#sueldo_diario_nuevo_emp").val().trim() == "" || $("#sueldo_diario_nuevo_emp").val().trim() == " ") { obligatorio = 1; }

                incidencias.INCIDENCIA_ID = $("#incidencia_emp").val();
                incidencias.EMPELADO_ID = $("#empleado_id").text();
                incidencias.REG_PATRONAL_ID = $("#registro_patronal_emp").val();
                incidencias.FECHA = $("#fecha_incidencia_emp").val();
                incidencias.TIPO = tipo;
                incidencias.CASUSA_BAJA = 0;
                incidencias.OBSERVACIONES = $("#observaciones_incidencia").val();



                incidencias.SALARIO_DIARIO = $("#sueldo_diario_actual_emp").val();
                incidencias.SALARIO_HORA = $("#sueldo_hora_actual_emp").val();
                incidencias.SALARIO_INTEG = $("#sueldo_integrado_actual_emp").val();

                incidencias.SALARIO_DIARIO_NUEVO = $("#sueldo_diario_nuevo_emp").val();
                incidencias.SALARIO_HORA_NUEVO = $("#sueldo_hora_nuevo_emp").val();
                incidencias.SALARIO_INTEG = $("#sueldo_integrado_nuevo_emp").val();
                incidencias.SALARIO_INTEG_NUEVO = $("#porcentaje_integracion").val();
                incidencias.ID_PUESTO_ACTUAL = $("#puesto_emp_anterior").val();
                incidencias.ID_PUESTO_NUEVO = $("#puesto_emp_nuevo").val();
                incidencias.ID_DEPARTAMENTO_ACTUAL = $("#departamento_anterior").val();
                incidencias.ID_DEPARTAMENTO_NUEVO = $("#departamento_emp_nuevo").val();
                incidencias.NUEVO_REG_PATRONAL_ID = $("#nuevo_registro_patronal_emp").val();
                incidencias.NUEVO_REG_PATRONAL_ID = $("#nuevo_registro_patronal_emp").val();
                incidencias.LISTA_NEGRA = false;
            }

            if (obligatorio == 0) {
                if (negra == "0") {
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../NOMINA/RegistrarIncidenciaMS",
                        data: {
                            incidencias: incidencias
                        },
                        success: function (response) {
                            iziToast.success({
                                title: 'Aviso',
                                message: response
                            });
                            $("#m_incidencia_nomina").modal('hide');
                            /*ConsultarIncidenciasNominaTable();*/
                            ConsultarReporteIncidencias();
                            $("#lista_negra_chk").prop('checked', false);
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
                        message: "El empleado no puede reincorporarse a la empresa, su reingreso está prohibido."
                    });
                }
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: "Favor de ingresar una sueldo diario"
                });
            }
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: "Favor de seleccionar un tipo de incidencia valido"
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: "Favor de seleccionar un empleado"
        });
    }
}

function MostrarInfoIncidencia(id_incidencia) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarInfoIncidencia",
        data: {
            id_incidencia: id_incidencia
        },
        success: function (response) {
            if (response != "") {
                limpiarFormulario();
                
                $("#tipo_salario").val('');
                $("#m_incidencia_nomina").modal("show");

                var data = $.parseJSON(response);
                $("#tipo_salario").val('');
                $("#incidencia_emp").val(data[0].id_tipo_incidencia);
                $("#empleado_id").text(data[0].Empleado_id);
                $("#nuevo_registro_patronal_emp").val(data[0].Nuevo_Reg_patronal_id);
                $("#sueldo_diario_nuevo_emp").val(data[0].Salario_diario);
                $("#sueldo_hora_nuevo_emp").val(data[0].Salario_hora);
                $("#sueldo_integrado_nuevo_emp").val(data[0].Salario_integ);
                $("#porcentaje_integracion").val(data[0].Pctje_integ);
                $("#empleado_emp").val(data[0].Nombre_completo);
                $("#div_btn_accion").css("display", "none");
                $("#div_empleado_btn").css("display", "none");
                $("#incidencia_emp").prop("disabled", true);



                $("#puesto_emp_anterior").val(data[0].id_puesto_actual);
                $("#departamento_anterior").val(data[0].id_departamento_actual);
                $("#puesto_emp_nuevo").val(data[0].id_puesto_nuevo);
                $("#departamento_emp_nuevo").val(data[0].id_departamento_nuevo);
                $("#sueldo_diario_actual_emp").val(data[0].diario_anterior);
                $("#sueldo_hora_actual_emp").val(data[0].hora_anterior);
                $("#sueldo_integrado_actual_emp").val(data[0].integ_anterior);

                var observaciones = data[0].observaciones.split('-');
                $("#causa_baja_empleado").val(observaciones[0]);
                $("#observaciones_incidencia_baja").val(observaciones[1]);



                $("#puesto_emp_anterior").val(data[0].observaciones);

                if (data[0].Lista_negra == true) {
                    $("#lista_negra_chk").prop('checked', true);
                }
                else {
                    $("#lista_negra_chk").prop('checked', false);
                }

                TipoIncidenciaAccion();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/MostrarSalarioTipo",
                    data: {
                        id_empleado: data[0].Empleado_id
                    },
                    success: function (response) {
                        $("#tipo_salario").val(response);
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al obtener la informacion',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion


//#region INASISTENCIA
function ModalInasistencia(modo) {
    if (modo == 1) {
        $("#m_inasistencia_nomina").modal("show");
        $('.div_ocultar').each(function () {
            this.style.display = 'none';
        });
        $("#inasistencia_obs").val('');
        $("#tabla_nomina_empleados").dataTable().fnClearTable();
        $("#tabla_nomina_empleados").dataTable().fnDraw();
        $("#tabla_nomina_empleados").dataTable().fnDestroy();
    }
    if (modo == 2) {
        $("#inasistencia_obs").val('');
        $('.div_ocultar').each(function () {
            this.style.display = 'block';
        });
    }
    if (modo == 3) {
        $('.div_ocultar').each(function () {
            this.style.display = 'none';
        });
        $("#inasistencia_obs").val('');
    }
}

function MostrarDetalleInasistencias(empleado_id) {
    var fecha1 = $("#fecha_inicio_area").text();
    var fecha2 = $("#fecha_fin_area").text();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarDetalleInasistencias",
        data: {
            empleado_id: empleado_id,
            fecha1: fecha1,
            fecha2: fecha2
        },
        success: function (response) {

            $("#m_inasistencia_nomina_detalle").modal("show");
            try {
                $("#tabla_inasistencia_empleado_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_inasistencia_general_detalle").html(response);
            $('#tabla_inasistencia_empleado_detalle').DataTable({
                ordering: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [],
                responsive: false,
                paging: false,
                searching: false
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function MostrarDiasInasistencia() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarDiasInasistencia",
        data: {
        },
        success: function (response) {
            $("#div_dias_inasistencia").css('display', 'block');
            $("#div_dias_inasistencia").html(response);
            DeshabilitarDiasInasistencia();
            $("#div_btn_accion_inasistencia").css('display', 'block');
            ModalInasistencia(2);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function MostrarInasistenciaEmpleadoTable(id_empleado, id_nomina_g, modo) {
    if (id_empleado == undefined) { $("#empleado_inasistencia_id").text(); }
    if (id_empleado != '') {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/MostrarInasistenciaEmpleadoTable",
            data: {
                id_empleado: id_empleado,
                id_nomina_g: id_nomina_g,
                modo: modo
            },
            success: function (response) {
                $("#div_empleado_inasistencia").css('display', 'block');
                try {
                    $("#tabla_inasistencia_empleado").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_empleado_inasistencia").html(response);
                $('#tabla_inasistencia_empleado').DataTable({
                    ordering: false,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [],
                    responsive: true,
                    paging: false,
                    searching: false
                });
                $("#div_faltas_empleado_concepto").html(response);
                jsRemoveWindowLoad();
                MostrarDiasInasistencia();

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
            message: "Favor de seleccionar un empleado",
        });
    }
}

function EliminarInasistencia(id_inasistencia) {
    var modo = 0;
    var empleado_id = $("#empleado_inasistencia_id").text();
    if (empleado_id == "" || empleado_id == null) {
        empleado_id = $("#detalle_empleado_inasistencia").text();
        modo = 1;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/EliminarInasistencia",
        data: {
            id_inasistencia: id_inasistencia,
            empleado_id: empleado_id
        },
        success: function (response) {
            if (response == 2) {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se elimino con exito la falta'
                });
                if (modo == 1) {
                    MostrarDetalleInasistencias(empleado_id);
                }
                else {
                    MostrarInasistenciaEmpleadoTable(empleado_id, 0, 1);
                }
            }
            if (response == 1) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'La nomina esta cerrada, no se puede eliminar la falta'
                });
            }
            if (response == 3) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al buscar el empleado, favor de internarlo mas tarde'
                });
            }
            if (response == 4) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al realizar los calculos de las faltas'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ConfirmarInasistencias() {
    if ($("#empleado_inasistencia_estatus").text() == 'A') {
        var obs = $("#inasistencia_obs").val();
        var count = 0;
        var empleado_id = $("#empleado_inasistencia_id").text();
        if (empleado_id != "" && empleado_id != " ") {
            var dias_inasistencias = [];
            var dias_inasistencias_table = [];
            var dias_inasistencias_final = [];
            $(".dias_inasistencia_asignados").each(function () {
                var id_inasig = $(this).text().replace(/\//g, "-");
                dias_inasistencias_table[count] = id_inasig;
                count++;
            });
            count = 0;
            $(".dias_inasistencia").each(function () {
                var id_inasig = $(this).attr("id");
                if ($(this).is(':checked')) {
                    dias_inasistencias[count] = id_inasig;
                    count++;
                }
            });

            for (var i = 0; i < dias_inasistencias.length; i++) {
                var elemento = dias_inasistencias[i];
                if (dias_inasistencias_table.indexOf(elemento) === -1) {
                    dias_inasistencias_final.push(elemento);
                }
            }

            if (dias_inasistencias_final.length > 0) {


                var id_departamento = $("#empleado_depa_inasistencia_id").text();

                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/ConfirmarInasistencias",
                    data: {
                        dias_inasistencias: dias_inasistencias_final,
                        empleado_id: empleado_id,
                        obs: obs,
                        id_departamento: id_departamento
                    },
                    success: function (response) {
                        if (response == 2) {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se registro con exito las faltas'
                            });
                            ModalInasistencia(3);
                            $("#inasistencia_obs").val('');
                            $("#empleado_inasistencia_id").text('');
                        }
                        if (response == 1) {
                            iziToast.info({
                                title: 'Aviso',
                                message: 'El empleado debe estar en una nómina abierta'
                            });
                        }
                        if (response == 3) {
                            iziToast.error({
                                title: 'Aviso',
                                message: 'Ocurrio un problema al buscar el empleado, favor de intentarlo mas tarde'
                            });
                        }
                        if (response == 4) {
                            iziToast.warning({
                                title: 'error',
                                message: 'Ocurrio un problema al realizar los calculos de las faltas'
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
                jsRemoveWindowLoad();
                ModalInasistencia(3);
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: "Favor de seleccionar un dia"
                });
            }
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: "Favor de seleccionar un empleado"
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: "Favor de seleccionar un empleado activo",
        });
    }
}

function DeshabilitarDiasInasistencia() {
    var count = 0;
    var dias_inasistencias = [];
    var dias_inasistencias_activos = [];
    var dias_inasistencias_inactivos = [];
    $(".dias_inasistencia").each(function () {
        var id_ina = $(this).attr("id");
        dias_inasistencias[count] = id_ina;
        count++;
    });
    count = 0;
    $(".dias_inasistencia_asignados").each(function () {
        var id_inasig = $(this).text().replace(/\//g, "-");
        dias_inasistencias_activos[count] = id_inasig;
        count++;
    });

    for (var i = 0; i < dias_inasistencias_activos.length; i++) {
        if (dias_inasistencias.includes(dias_inasistencias_activos[i])) {
            dias_inasistencias_inactivos.push(dias_inasistencias_activos[i]);
        }
    }

    for (var i = 0; i < dias_inasistencias_inactivos.length; i++) {
        $("input[id='" + dias_inasistencias_inactivos[i] + "']").prop("disabled", true);
        $("input[id='" + dias_inasistencias_inactivos[i] + "']").prop('checked', true);
    }

}

function MostrarInasistenciaEmpleadoGeneralTable() {
    var area = $("#area_inasistencia_emp").val();
    var fecha1 = $("#fecha_inicio_area").text();
    var fecha2 = $("#fecha_fin_area").text();
    if (area != '-1') {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/MostrarInasistenciaEmpleadoGeneralTable",
            data: {
                area: area,
                fecha1: fecha1,
                fecha2: fecha2
            },
            success: function (response) {
                try {
                    $("#tabla_inasistencia_empleado_general").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_inasistencia_general").html(response);
                $('#tabla_inasistencia_empleado_general').DataTable({
                    ordering: true,
                    select: true,
                    keys: false,
                    dom: "Bfrtip",
                    scrollCollapse: false,
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
                    paging: true,
                    searching: true
                });
                jsRemoveWindowLoad();

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
            message: "Favor de seleccionar un area",
        });
    }
}

function ReporteFaltasEmpleado() {
    jsShowWindowLoad();
    var area = $("#area_inasistencia_emp_reporte").val();
    var fecha1 = $("#fecha_inicio").val();
    var fecha2 = $("#fecha_fin").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../NOMINA/ReporteFaltasEmpleado",
        data: {
            area: area,
            fecha1: fecha1,
            fecha2: fecha2
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_presupuestos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_empleado_inasistencia").html(response);
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

function GenerarFormatoReporte() {
    var area = $("#area_inasistencia_emp_reporte").val();
    var fecha1 = $("#fecha_inicio").val();
    var fecha2 = $("#fecha_fin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../NOMINA/GenerarFormatoReporte",
        data: {
            area: area,
            fecha1: fecha1,
            fecha2: fecha2
        },
        success: function (response) {
            jsRemoveWindowLoad();

            // Create a temporary div to hold the response HTML
            var tempDiv = document.createElement('div');
            tempDiv.innerHTML = response;

            html2pdf()
                .from(tempDiv)
                .set({
                    margin: [10, 10, 10, 10],
                    filename: "Reporte de Faltas.pdf",
                    image: { type: 'jpeg', quality: 0.98 },
                    html2canvas: { scale: 2 },
                    jsPDF: { unit: 'pt', format: 'letter', orientation: 'portrait' },
                    pagebreak: { mode: ['avoid-all', 'css'] }
                })
                .save()
                .then(function () {
                    jsRemoveWindowLoad();
                });

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}



//#endregion


//#region CAMBIO SALARIO MASIVO
function ConsultarEmpleadosCambioSalarioMasivo(modo) {
    var id_puesto = 0;
    var salario_diario = 0;
    if (modo == undefined) { modo = $("input[name='radio_tipo_busqueda']:checked").val(); }

    if (modo == 1) {
        id_puesto = $("#id_puesto_cambio_salario").val();
        salario_diario = 0;
    }
    else if (modo == 2) {
        id_puesto = 0
        salario_diario = $("#salario_diario_cambio_salario").val();
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../NOMINA/ConsultarEmpleadosCambioSalarioMasivo",
        data: {
            id_puesto: id_puesto,
            salario_diario: salario_diario
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_cambio_salario_empleados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_empleados_cambio_salario_empleados").html(response);
            $("#datatable_cambio_salario_empleados").DataTable({
                keys: false,
                ordering: true,
                paging: false,
                select:true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
            $(".formulario_conceptos").css("display", "inline");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CambiarSalariosMasivosEmpleados() {
    var salario_diario = $("#salario_diario_masivo").val();
    var salario_hora = $("#salario_hora_masivo").val();

    var checkboxes = document.querySelectorAll('input[name="check_cambio_salario_masivo"]:checked');
    var id_empleados = [];
    checkboxes.forEach((checkbox) => {
        id_empleados.push(checkbox.value);
    });

    if (id_empleados.length <= 0) {
        iziToast.error({
            title: 'Seleccione al menos 1 empleado para cambiar el salario',
            message: '',
        });
    }
    else if (salario_diario <= 0 || salario_diario == undefined || salario_hora == "") {
        iziToast.error({
            title: 'Ingrese un salario diario valido',
            message: '',
        });
    }
    else if (salario_hora <= 0 || salario_hora == undefined || salario_hora == "") {
        iziToast.error({
            title: 'Ingrese un salario por hora valido',
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
            message: '¿Está seguro de modificar los salarios a los empleados seleccionados?',
            position: 'center',
            buttons: [
                ['<button><b>Si, cambiar salarios</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 9000000,
                        url: "../NOMINA/CambiarSalariosMasivosEmpleados",
                        data: {
                            id_empleados: id_empleados,
                            salario_diario: salario_diario,
                            salario_hora: salario_hora
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response > 0) {
                                $("#salario_diario_masivo").val("");
                                $("#salario_hora_masivo").val("");
                                iziToast.success({
                                    title: 'Moficiación de salarios aplicada correctamente',
                                    message: '',
                                });
                                ConsultarEmpleadosCambioSalarioMasivo();
                            }
                            else if (response == -1) {
                                iziToast.warning({
                                    title: 'Algunos empleados no se les pudo cambiar el salario',
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.error({
                                    title: 'Ocurrió un error al cambiar los salarios',
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



//#region SOLICITUDES DE ALTA DE EMPLEADOS
function ConfirmarConstanciaFiscal(id_empleado) {
    var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'Aviso',
        message: '¿Estás seguro de confirmar el registro?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                var empleado = {
                    Empleado_id: id_empleado,
                    Nombre_completo: obtenerValorConEspacio("#nombre_completo_emp"),
                    Apellido_paterno: obtenerValorConEspacio("#apellido_paterno_emp"),
                    Apellido_materno: obtenerValorConEspacio("#apellido_materno_emp"),
                    Nombres: obtenerValorConEspacio("#nombre_emp"),
                    RFC: rfc
                };

                var fileData = new FormData();
                var fileUpload = $("#pdf_upload").get(0);
                var files = fileUpload.files;
                for (var i = 0; i < files.length; i++) {
                    fileData.append('file', files[i]);
                }

                fileData.append('empleado', JSON.stringify(empleado));

                $.ajax({
                    url: '../NOMINA/ConfirmarConstanciaFiscal',
                    type: "POST",
                    contentType: false,
                    processData: false,
                    data: fileData,
                    success: function (result) {
                        jsRemoveWindowLoad();

                        if (result == "-2") {
                            iziToast.warning({
                                title: 'EL ARCHIVO DEBE TENER FORMATO PDF',
                                message: ''
                            });
                        }

                        else if (result == "-3") {
                            iziToast.error({
                                title: 'OCURRIÓ UN PROBLEMA AL GUARDAR EL ARCHIVO',
                                message: ''
                            });
                        }

                        else if (result == "-4") {
                            iziToast.info({
                                title: 'NO SE DETECTARON ARCHIVOS',
                                message: ''
                            });
                        }

                        else if (result == "-5") {
                            iziToast.warning({
                                title: 'OCURRIÓ UN ERROR EL REGISTRAR LA SOLICITUD',
                                message: ''
                            });
                        }

                        else if (result == "0") {
                            iziToast.success({
                                title: 'SE REGISTRO CORRECTAMENTE LA CONSTANCIA FISCAL',
                                message: ''
                            });
                            $("#pdf_upload").val('');
                            MostrarBotonConstanciaFiscal();
                            $("#m_empleados").modal("hide");
                        }

                    },
                    error: function (err) {
                        jsRemoveWindowLoad();
                    }
                });

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

function MostrarBotonConstanciaFiscal() {
    if ($("#pdf_upload").val() != "") {
        $("#div_confirmar_constancia").css("display", "block");
    }
    else {
        $("#div_confirmar_constancia").css("display", "none");
    }
}
function ConsultarSoliciudesRevision(modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 80000,
        url: "../NOMINA/ConsultarSoliciudesRevision",
        data: {
            modo: modo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (modo == 1) {
                try {
                    $("#datatable_solicitudes_validar").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_solicitudes_validar").html(response);
                $('#datatable_solicitudes_validar').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: false,
                    pagingType: 'simple_numbers',
                    pageLength: 10
                });
            }
            else {
                try {
                    $("#datatable_solicitudes_autorizar").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_solicitudes_autorizar").html(response);
                $('#datatable_solicitudes_autorizar').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: false,
                    pagingType: 'simple_numbers',
                    pageLength: 10
                });


            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function ConsultarSolciitudAltaEmpleado(id_empleado_solicitud, modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 80000,
        url: "../NOMINA/ConsultarSolciitudAltaEmpleado",
        data: {
            id_empleado_solicitud: id_empleado_solicitud,
            modo: modo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_ver_soliitud").modal("show");

            var data = $.parseJSON(response);
            $("#nombre_completo_emp").val(data[0].Nombre_completo);
            $("#apellido_paterno_emp").val(data[0].Apellido_paterno);
            $("#apellido_materno_emp").val(data[0].Apellido_materno);
            $("#nombre_emp").val(data[0].Nombres);
            $("#puesto_emp").val(data[0].Puesto_no_id);
            $("#departamento_emp").val(data[0].Depto_no_id);
            $("#frecuencia_emp").val(data[0].Frepag_id);
            $("#registro_patronal_emp").val(data[0].Reg_patronal_id);

            if (data[0].Es_jefe === true) {
                $("#es_jefe_emp").prop('checked', true);
            } else {
                $("#es_jefe_emp").prop('checked', false);
            }

            $("#forma_pago_emp").val(data[0].Forma_pago);
            $("#contrato_general_emp").val(data[0].Contrato);
            $("#turno_emp").val(data[0].Turno);
            $("#jornada_emp").val(data[0].Jornada);

            /*
            if (data[0].Regimen_fiscal == 2) {
                ("#regimen_emp").val("02");
            }
            */
            $("#contrato_sat_emp").val(data[0].Contrato_sat);
            $("#jornada_sat_emp").val(data[0].Jornada_sat);

            if (data[0].Es_sindicalizado === 'S') {
                $("#sindicalizado_emp").prop('checked', true);
            } else {
                $("#sindicalizado_emp").prop('checked', false);
            }



            $("#estatus_emp").val(data[0].Estatus);
            $("#zona_salario_minimo_emp").val(data[0].Zona_salmin);
            $("#antigüedades_emp").val(data[0].Tabla_antig_id);
            $("#tipo_salario_emp").val(data[0].Tipo_salario);
            $("#porcentaje_integracion_emp").val(data[0].Pctje_integ);
            $("#sueldo_diario_emp").val(data[0].Salario_diario);
            $("#sueldo_hora_emp").val(data[0].Salario_hora);
            $("#sueldo_diario_integrado_emp").val(data[0].Salario_integ);

            if (data[0].Es_dir_admr_gte_gral === true) {
                $("#director_emp").prop('checked', true);
            } else {
                $("#director_emp").prop('checked', false);
            }

            if (data[0].PTU === true) {
                $("#derecho_ptu_emp").prop('checked', true);
            } else {
                $("#derecho_ptu_emp").prop('checked', false);
            }

            if (data[0].IMSS === true) {
                $("#retener_seguro_emp").prop('checked', true);
            } else {
                $("#retener_seguro_emp").prop('checked', false);
            }

            if (data[0].CAS === true) {
                $("#aplicar_subsidio_emp").prop('checked', true);
            } else {
                $("#aplicar_subsidio_emp").prop('checked', false);
            }

            if (data[0].DESHAB_IMPTOS === true) {
                $("#deshabilitar_impuestos_emp").prop('checked', true);
            } else {
                $("#deshabilitar_impuestos_emp").prop('checked', false);
            }

            if (data[0].CALC_ISR_ANUAL === true) {
                $("#calcular_isr_emp").prop('checked', true);
            } else {
                $("#calcular_isr_emp").prop('checked', false);
            }

            $("#direc_domicilio_emp").val(data[0].Calle);
            $("#direc_calle_emp").val(data[0].Nombre_calle);
            $("#direc_no_exterior_emp").val(data[0].Num_exterior);
            $("#direc_colonia_emp").val(data[0].Colonia);
            $("#direc_poblacion_emp").val(data[0].Poblacion);
            $("#direc_referencia_emp").val(data[0].Referencia);
            $("#direc_ciudad_emp").val(data[0].Ciudad_id);
            $("#direc_cp_emp").val(data[0].Codigo_postal);
            $("#direc_telefono1_emp").val(data[0].Telefono1);

            $("#direc_email_emp").val(data[0].Email);
            $("#sexo_emp").val(data[0].Sexo);



            var fecha = new Date(data[0].Fecha_nacimiento);
            var fechaFormatoInput = fecha.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_nacimiento_emp").val(fechaFormatoInput);

            var fechaing = new Date(data[0].Fecha_ingreso);
            var fechaFormatoInputingre = fechaing.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_ingreso_emp").val(fechaFormatoInputingre);

            $("#div_ingreso").css("display", "block");
            $("#estatus_emp").prop("disabled", false);



            $("#lugar_nacimiento_emp").val(data[0].Ciudad_nacimiento_id);
            $("#estado_civil_emp").val(data[0].Estado_civil);
            $("#numero_hijos_emp").val(data[0].Num_hijos);
            $("#nombre_padre_emp").val(data[0].Nombre_padre);
            $("#nombre_madre_emp").val(data[0].Nombre_madre);


            var rfc = data[0].RFC;
            $("#rfc_registrado").text(rfc);
            var partesRFC = rfc.split('-');
            if (partesRFC.length > 2) {
                $("#rfc_emp_1").val(partesRFC[0]);
                $("#rfc_emp_2").val(partesRFC[1]);
                $("#rfc_emp_3").val(partesRFC[2]);
            }
            else {
                $("#rfc_emp_1").val(rfc.substring(0, 4));
                $("#rfc_emp_2").val(rfc.substring(4, 10));
                $("#rfc_emp_3").val(rfc.substring(10));
            }

            rfc_edit = data[0].RFC;

            $("#curp_emp").val(data[0].CURP);
            $("#seguridad_social_emp").val(data[0].Reg_imss);
            $("#grupo_pago_emp").val(data[0].Grupo_pago_elect_id);
            $("#tipo_cuenta_emp").val(data[0].Tipo_ctaban_pago_elect);
            $("#numero_cuenta_emp").val(data[0].Num_ctaban_pago_elect);

            if ($("#forma_pago_emp").val() == "T") {
                $("#grupo_pago_emp").prop("disabled", false);
                $("#tipo_cuenta_emp").prop("disabled", false);
                $("#numero_cuenta_emp").prop("disabled", false);
            }


            $("#sueldo_diario_real_emp").val(data[0].Salario_diario_establo);
            $("#sueldo_hora_real_emp").val(data[0].Salario_hora_establo);


            if (data[0].Salario_diario == data[0].Salario_diario_establo) {
                $("#sueldo_diario_emp").prop("disabled", false);
            }
            else {
                $("#sueldo_diario_emp").prop("disabled", true);
            }
            $("#estatus_emp").prop("disabled", true);

            $("#link_constancia_fiscal").attr("href", data[0].path);
            $("#btn_firma_solicitud").attr("onclick", "FirmarSolicitudAltaEmpleado(" + id_empleado_solicitud + ", "+ modo +")");
            if (modo == 1) {
                $("#btn_firma_solicitud").text("VALIDAR SOLICITUD");
            }
            else if (modo == 2) {
                $("#btn_firma_solicitud").text("AUTORIZAR SOLICITUD");
            }
            else {
                $("#btn_firma_solicitud").css("display", "none");
            }



            $("#btn_rechazar_solicitud").attr("onclick", "RechazarSolicitudAltaEmpleado(" + id_empleado_solicitud + ")");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}



function FirmarSolicitudAltaEmpleado(id_empleado_solicitud, modo) {
    var rfc_primero = $("#rfc_registrado").text();
    ValidarCamposObligatorios(1);
    if (campos_obligatorios == 0) {
        var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
        var rfc_segundo = $("#rfc_emp_1").val() + $("#rfc_emp_2").val() + $("#rfc_emp_3").val();
        if (rfc_primero == rfc_segundo) { validacion_rfc = 1; }
        else { ValidarRFC(); }        
        if (validacion_rfc == 1) {
            var fecha_nac = $("#fecha_nacimiento_emp").val();
            var fecha_ingreso = $("#fecha_ingreso_emp").val();
            if (fecha_nac != "" && fecha_ingreso != "") {


                iziToast.question({
                    timeout: 20000,
                    close: false,
                    overlay: true,
                    displayMode: 'once',
                    id: 'question',
                    zindex: 25000,
                    title: 'Aviso',
                    message: '¿Estás seguro de confirmar la validacion?',
                    position: 'center',
                    buttons: [
                        ['<button><b>SI</b></button>', function (instance, toast) {
                            instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                            if ($("#numero_hijos_emp").val().trim() == '') {
                                $("#numero_hijos_emp").val('0');
                            }
                            var empleado = {
                                id_empleado_solicitud: id_empleado_solicitud,
                                Nombre_completo: obtenerValorConEspacio("#nombre_completo_emp"),
                                Apellido_paterno: obtenerValorConEspacio("#apellido_paterno_emp"),
                                Apellido_materno: obtenerValorConEspacio("#apellido_materno_emp"),
                                Nombres: obtenerValorConEspacio("#nombre_emp"),
                                Regimen: 'S',
                                Puesto_no_id: obtenerValorConEspacio("#puesto_emp"),
                                Depto_no_id: obtenerValorConEspacio("#departamento_emp"),
                                Frepag_id: obtenerValorConEspacio("#frecuencia_emp"),
                                Reg_patronal_id: obtenerValorConEspacio("#registro_patronal_emp"),
                                Es_jefe: $("#es_jefe_emp").is(':checked'),
                                Forma_pago: obtenerValorConEspacio("#forma_pago_emp"),
                                Grupo_pago_elect_id: obtenerValorConEspacio("#grupo_pago_emp"),
                                Num_ctaban_pago_elect: obtenerValorConEspacio("#numero_cuenta_emp"),
                                Tipo_ctaban_pago_elect: obtenerValorConEspacio("#tipo_cuenta_emp"),
                                Contrato: obtenerValorConEspacio("#contrato_general_emp"),
                                Dias_hrs_jsr: 0.00,
                                Turno: obtenerValorConEspacio("#turno_emp"),
                                Jornada: obtenerValorConEspacio("#jornada_emp"),
                                Regimen_fiscal: obtenerValorConEspacio("#regimen_emp"),
                                Contrato_sat: obtenerValorConEspacio("#contrato_sat_emp"),
                                Jornada_sat: obtenerValorConEspacio("#jornada_sat_emp"),
                                Es_sindicalizado: $("#sindicalizado_emp").is(':checked') ? 'S' : 'N',
                                Estatus: obtenerValorConEspacio("#estatus_emp"),
                                Zona_salmin: obtenerValorConEspacio("#zona_salario_minimo_emp"),
                                Tabla_antig_id: obtenerValorConEspacio("#antigüedades_emp"),
                                Tipo_salario: obtenerValorConEspacio("#tipo_salario_emp"),
                                Pctje_integ: obtenerValorConEspacio("#porcentaje_integracion_emp"),

                                Salario_diario: obtenerValorConEspacio("#sueldo_diario_emp"),
                                Salario_hora: obtenerValorConEspacio("#sueldo_hora_emp"),

                                Salario_diario_establo: obtenerValorConEspacio("#sueldo_diario_real_emp"),
                                Salario_hora_establo: obtenerValorConEspacio("#sueldo_hora_real_emp"),

                                Salario_integ: obtenerValorConEspacio("#sueldo_diario_integrado_emp"),

                                Es_dir_admr_gte_gral: $("#director_emp").is(':checked'),
                                PTU: $("#derecho_ptu_emp").is(':checked'),
                                IMSS: $("#retener_seguro_emp").is(':checked'),
                                CAS: $("#aplicar_subsidio_emp").is(':checked'),
                                Pensionado: false,
                                Deshab_imptos: $("#deshabilitar_impuestos_emp").is(':checked'),
                                Calc_isr_anual: $("#calcular_isr_emp").is(':checked'),
                                Calle: obtenerValorConEspacio("#direc_domicilio_emp"),
                                Nombre_calle: obtenerValorConEspacio("#direc_calle_emp"),
                                Num_exterior: obtenerValorConEspacio("#direc_no_exterior_emp"),
                                Colonia: obtenerValorConEspacio("#direc_colonia_emp"),
                                Poblacion: obtenerValorConEspacio("#direc_poblacion_emp"),
                                Referencia: obtenerValorConEspacio("#direc_referencia_emp"),
                                Ciudad_id: obtenerValorConEspacio("#direc_ciudad_emp"),
                                Codigo_postal: obtenerValorConEspacio("#direc_cp_emp"),
                                Telefono1: obtenerValorConEspacio("#direc_telefono1_emp"),
                                Telefono2: obtenerValorConEspacio("#direc_telefono2_emp"),
                                Fecha_ingreso: fecha_ingreso,
                                Email: obtenerValorConEspacio("#direc_email_emp"),
                                Sexo: obtenerValorConEspacio("#sexo_emp"),
                                Fecha_nacimiento: fecha_nac,
                                Ciudad_nacimiento_id: obtenerValorConEspacio("#lugar_nacimiento_emp"),
                                Estado_civil: obtenerValorConEspacio("#estado_civil_emp"),
                                Num_hijos: obtenerValorConEspacio("#numero_hijos_emp"),
                                Nombre_padre: obtenerValorConEspacio("#nombre_padre_emp"),
                                Nombre_madre: obtenerValorConEspacio("#nombre_madre_emp"),
                                RFC: rfc,
                                CURP: obtenerValorConEspacio("#curp_emp"),
                                Reg_imss: obtenerValorConEspacio("#seguridad_social_emp")
                            };

                            var fileData = new FormData();
                            var fileUpload = $("#pdf_upload").get(0);
                            var files = fileUpload.files;
                            for (var i = 0; i < files.length; i++) {
                                fileData.append('file', files[i]);
                            }

                            fileData.append('empleado', JSON.stringify(empleado));
                            fileData.append('id_solicitud_g', id_empleado_solicitud);
                            fileData.append('modo', modo);

                            $.ajax({
                                url: '../NOMINA/FirmarSolicitudAltaEmpleado',
                                type: "POST",
                                contentType: false,
                                processData: false,
                                data: fileData,
                                success: function (result) {
                                    jsRemoveWindowLoad();
                                    if (result == "-2") {
                                        iziToast.warning({
                                            title: 'Ocurrió un problema al actualizar la constancia',
                                            message: ''
                                        });
                                    }
                                    else {

                                        //-----ANA
                                        if (modo == 1) {
                                            if (result == "0") {
                                                $("#m_ver_soliitud").modal("hide");
                                                iziToast.success({
                                                    title: 'Validación de la solicitud exitosa!',
                                                    message: ''
                                                });
                                                limpiarFormulario();
                                                ValidarFormaPago();
                                                $("#pdf_upload").val('');

                                                ConsultarSoliciudesRevision(1);
                                                ConsultarSoliciudesRevision(2);
                                            }
                                            else {
                                                iziToast.error({
                                                    title: 'Ocurrió un problema al validar',
                                                    message: '.'
                                                });
                                            }
                                        }

                                        //------ VALERIA
                                        else if (modo == 2) {

                                            if (result == "0") {
                                                $("#m_ver_soliitud").modal("hide");
                                                iziToast.success({
                                                    title: 'Solicitud autorizada correctamente',
                                                    message: 'Empleado registrado en Microsip'
                                                });
                                                limpiarFormulario();
                                                ValidarFormaPago();
                                                $("#pdf_upload").val('');

                                                ConsultarSoliciudesRevision(1);
                                                ConsultarSoliciudesRevision(2);
                                            }


                                            else if (result == "-2") {
                                                iziToast.error({
                                                    title: 'OCURRIÓ UN ERROR AL GUARDAR EL ARCHIVO',
                                                    message: ''
                                                });
                                            }

                                            else if (result == "-3") {
                                                iziToast.error({
                                                    title: 'EL ARCHIVO DEBE TENER EL FORMATO PDF',
                                                    message: ''
                                                });
                                            }

                                            else if (result == "-4") {
                                                iziToast.error({
                                                    title: 'OCURRIÓ UN PROBLEMA AL REGISTRAR EL EMPLEADO EN MICROSIP',
                                                    message: ''
                                                });
                                            }

                                            else if (result == "-5") {
                                                iziToast.error({
                                                    title: 'ERROR.',
                                                    message: 'Modo desconocido'
                                                });
                                            }

                                            else {
                                                iziToast.error({
                                                    title: 'ERROR.',
                                                    message: 'Contacte a sistemas'
                                                });
                                            }

                                        }
                                    }
                                },
                                error: function (err) {
                                    jsRemoveWindowLoad();
                                }
                            });

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
            } else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'La fecha de ingreso y nacimiento es obligatoria',
                });
            }
        } else {
            $("#rfc_emp_1, #rfc_emp_2, #rfc_emp_3").css("border", "1px solid red");
        }
    }
}

function RechazarSolicitudAltaEmpleado(id_empleado_solicitud) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 250000,
        title: 'Aviso',
        message: '¿Estás seguro de rechazar la solicitud?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/RechazarSolicitudAltaEmpleado",
                    data: {
                        id_empleado_solicitud: id_empleado_solicitud
                    },
                    success: function (response) {
                        if (response == true || response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se rechazo correctamente la solicitud',
                            });
                            $("#m_ver_soliitud").modal("hide");
                            limpiarFormulario();
                            ValidarFormaPago();
                            $("#pdf_upload").val('');

                            ConsultarSoliciudesRevision(1);
                            ConsultarSoliciudesRevision(2);
                        }
                        else {
                            iziToast.warning({
                                title: 'Aviso',
                                message: 'No se rechazo la solicitud',
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

//#endregion


//#region PREREGISTRO DE EMPLEADO
function PregistrarEmpleadoMS() {

    ValidarCamposObligatorios(1);
    if (campos_obligatorios == 0) {
        var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
        ValidarRFC();
        if (validacion_rfc == 1) {
            var fecha_nac = $("#fecha_nacimiento_emp").val();
            if (fecha_nac != "" || fecha_nac == undefined) {
                //#region DATOS EMPLEADO
                $('#jornada_emp').val('8');
                var empleado = {};
                empleado.Nombre_completo = $("#nombre_completo_emp").val();
                empleado.Apellido_paterno = $("#apellido_paterno_emp").val();
                empleado.Apellido_materno = $("#apellido_materno_emp").val();
                empleado.Nombres = $("#nombre_emp").val();
                empleado.Regimen = 'S';
                empleado.Puesto_no_id = $("#puesto_emp").val();
                empleado.Depto_no_id = $("#departamento_emp").val();
                empleado.Frepag_id = $("#frecuencia_emp").val();
                empleado.Reg_patronal_id = $("#registro_patronal_emp").val();
                if ($("#es_jefe_emp").is(':checked')) { empleado.Es_jefe = true; }
                else { empleado.Es_jefe = false; }
                empleado.Forma_pago = $("#forma_pago_emp").val();
                if ($("#forma_pago_emp").val() == 'T') {
                    empleado.Grupo_pago_elect_id = $("#grupo_pago_emp").val();
                    empleado.Num_ctaban_pago_elect = $("#numero_cuenta_emp").val();
                }
                else { }
                empleado.Tipo_ctaban_pago_elect = $("#tipo_cuenta_emp").val();
                empleado.Contrato = $("#contrato_general_emp").val();
                empleado.Dias_hrs_jsr = 0.00;
                empleado.Turno = $("#turno_emp").val();
                empleado.Jornada = $("#jornada_emp").val();
                empleado.Regimen_fiscal = $("#regimen_emp").val();
                empleado.Contrato_sat = $("#contrato_sat_emp").val();
                empleado.Jornada_sat = $("#jornada_sat_emp").val();
                if ($("#sindicalizado_emp").is(':checked')) {
                    empleado.Es_sindicalizado = 'S';
                }
                else {
                    empleado.Es_sindicalizado = 'N';
                }
                empleado.Estatus = $("#estatus_emp").val();
                empleado.Zona_salmin = $("#zona_salario_minimo_emp").val();
                empleado.Tabla_antig_id = $("#antigüedades_emp").val();
                empleado.Tipo_salario = $("#tipo_salario_emp").val();
                empleado.Pctje_integ = $("#porcentaje_integracion_emp").val();
                empleado.Salario_diario = $("#sueldo_diario_emp").val();
                empleado.Salario_hora = $("#sueldo_hora_emp").val();
                empleado.Salario_integ = $("#sueldo_diario_integrado_emp").val();
                empleado.Salario_diario_establo = $("#sueldo_diario_emp").val();
                empleado.Salario_hora_establo = $("#sueldo_hora_emp").val();
                if ($("#director_emp").is(':checked')) {
                    empleado.Es_dir_admr_gte_gral = true;
                }
                else {
                    empleado.Es_dir_admr_gte_gral = false;
                }
                if ($("#derecho_ptu_emp").is(':checked')) {
                    empleado.PTU = true;
                }
                else {
                    empleado.PTU = false;
                }
                if ($("#retener_seguro_emp").is(':checked')) {
                    empleado.IMSS = true;
                }
                else {
                    empleado.IMSS = false;
                }
                if ($("#aplicar_subsidio_emp").is(':checked')) {
                    empleado.CAS = true;
                }
                else {
                    empleado.CAS = false;
                }
                empleado.Pensionado = false;
                if ($("#deshabilitar_impuestos_emp").is(':checked')) {
                    empleado.Deshab_imptos = true;
                }
                else {
                    empleado.Deshab_imptos = false;
                }
                if ($("#calcular_isr_emp").is(':checked')) {
                    empleado.Calc_isr_anual = true;
                }
                else {
                    empleado.Calc_isr_anual = false;
                }
                empleado.Calle = $("#direc_domicilio_emp").val();
                empleado.Nombre_calle = $("#direc_calle_emp").val();
                empleado.Num_exterior = $("#direc_no_exterior_emp").val();
                empleado.Colonia = $("#direc_colonia_emp").val();
                empleado.Poblacion = $("#direc_poblacion_emp").val();
                empleado.Referencia = $("#direc_referencia_emp").val();
                empleado.Ciudad_id = $("#direc_ciudad_emp").val();
                empleado.Codigo_postal = $("#direc_cp_emp").val();
                empleado.Telefono1 = $("#direc_telefono1_emp").val();
                empleado.Telefono2 = $("#direc_telefono2_emp").val();
                empleado.Email = $("#direc_email_emp").val();
                empleado.Sexo = $("#sexo_emp").val();
                empleado.Fecha_nacimiento = fecha_nac;
                empleado.Ciudad_nacimiento_id = $("#lugar_nacimiento_emp").val();
                empleado.Estado_civil = $("#estado_civil_emp").val();
                empleado.Num_hijos = $("#numero_hijos_emp").val();
                empleado.Nombre_padre = $("#nombre_padre_emp").val();
                empleado.Nombre_madre = $("#nombre_madre_emp").val();
                empleado.RFC = rfc;
                empleado.CURP = $("#curp_emp").val();
                empleado.Reg_imss = $("#seguridad_social_emp").val();
                //#endregion
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/PreRegistrarEmpleado",
                    data: {
                        empleado: empleado
                    },
                    success: function (response) {
                        RenovarConstanciaSolicitud();
                        iziToast.success({
                            title: 'Aviso',
                            message: response,
                        });
                        validacion_numero = 0;
                        campos_obligatorios = 0;
                        validacion_rfc = 0;
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
                jsRemoveWindowLoad();
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'La fecha de nacimiento es obligatorio',
                });
            }
        }
        else {
            $("#rfc_emp_1").css("border", "1px solid red");
            $("#rfc_emp_2").css("border", "1px solid red");
            $("#rfc_emp_3").css("border", "1px solid red");
        }
    }
}

function ConsultarSolicitudesEmpleadosRegistradas() {
    var estatus = $("#id_estatus").val();
    var fecha_inicio = $("#fecha_incidencia_reporte1").val();
    var fecha_fin = $("#fecha_incidencia_reporte2").val();

    if (fecha_inicio != "" && fecha_fin != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ConsultarSolicitudesEmpleadosRegistradas",
            data: {
                estatus: estatus,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            },
            success: function (response) {
                try {
                    $("#tabla_solicitud_empleado").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_solicitud_empleado").html(response);
                $('#tabla_solicitud_empleado').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: false,
                    pagingType: 'simple_numbers',
                    pageLength: 10
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
            message: 'Favor de indicar la fecha inicial y fecha final'
        });
    }
}
function ObtenerLunesDomingoActual() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerLunesSabadoActual",
        success: function (response) {
            if (response && !response.Error) {
                $("#fecha_incidencia_reporte1").val(response.Lunes);
                $("#fecha_incidencia_reporte2").val(response.Sabado);
            } else {
                iziToast.warning({
                    title: 'Aviso',
                    message: response.Error || 'Respuesta inesperada del servidor'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error en la solicitud'
            });
        }
    });
}



function obtenerValorConEspacio(selector) {
    var valor = $(selector).val();
    return valor ? valor : ' ';
}

function GuardarSolicitudAltaEmpleado() {
    ValidarCamposObligatorios(1);
    if (campos_obligatorios == 0) {
        var rfc = $("#rfc_emp_1").val() + "-" + $("#rfc_emp_2").val() + "-" + $("#rfc_emp_3").val();
        ValidarRFC();
        if (validacion_rfc == 1) {
            var fecha_nac = $("#fecha_nacimiento_emp").val();
            if (fecha_nac != "") {

                iziToast.question({
                    timeout: 20000,
                    close: false,
                    overlay: true,
                    displayMode: 'once',
                    id: 'question',
                    zindex: 99999,
                    title: 'Aviso',
                    message: '¿Estás seguro de confirmar el registro?',
                    position: 'center',
                    buttons: [
                        ['<button><b>SI</b></button>', function (instance, toast) {
                            instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                            if ($("#numero_hijos_emp").val().trim() == '') {
                                $("#numero_hijos_emp").val('0');
                            }
                            var empleado = {
                                Nombre_completo: obtenerValorConEspacio("#nombre_completo_emp"),
                                Apellido_paterno: obtenerValorConEspacio("#apellido_paterno_emp"),
                                Apellido_materno: obtenerValorConEspacio("#apellido_materno_emp"),
                                Nombres: obtenerValorConEspacio("#nombre_emp"),
                                Regimen: 'S',
                                Puesto_no_id: obtenerValorConEspacio("#puesto_emp"),
                                Depto_no_id: obtenerValorConEspacio("#departamento_emp"),
                                Frepag_id: obtenerValorConEspacio("#frecuencia_emp"),
                                Reg_patronal_id: obtenerValorConEspacio("#registro_patronal_emp"),
                                Es_jefe: $("#es_jefe_emp").is(':checked'),
                                Forma_pago: obtenerValorConEspacio("#forma_pago_emp"),
                                Grupo_pago_elect_id: obtenerValorConEspacio("#grupo_pago_emp"),
                                Num_ctaban_pago_elect: obtenerValorConEspacio("#numero_cuenta_emp"),
                                Tipo_ctaban_pago_elect: obtenerValorConEspacio("#tipo_cuenta_emp"),
                                Contrato: obtenerValorConEspacio("#contrato_general_emp"),
                                Dias_hrs_jsr: 0.00,
                                Turno: obtenerValorConEspacio("#turno_emp"),
                                Jornada: obtenerValorConEspacio("#jornada_emp"),
                                Regimen_fiscal: obtenerValorConEspacio("#regimen_emp"),
                                Contrato_sat: obtenerValorConEspacio("#contrato_sat_emp"),
                                Jornada_sat: obtenerValorConEspacio("#jornada_sat_emp"),
                                Es_sindicalizado: $("#sindicalizado_emp").is(':checked') ? 'S' : 'N',
                                Estatus: obtenerValorConEspacio("#estatus_emp"),
                                Zona_salmin: obtenerValorConEspacio("#zona_salario_minimo_emp"),
                                Tabla_antig_id: obtenerValorConEspacio("#antigüedades_emp"),
                                Tipo_salario: obtenerValorConEspacio("#tipo_salario_emp"),
                                Pctje_integ: obtenerValorConEspacio("#porcentaje_integracion_emp"),
                                Salario_diario: obtenerValorConEspacio("#sueldo_diario_emp"),
                                Salario_hora: obtenerValorConEspacio("#sueldo_hora_emp"),
                                Salario_integ: obtenerValorConEspacio("#sueldo_diario_integrado_emp"),
                                Salario_diario_establo: obtenerValorConEspacio("#sueldo_diario_emp"),
                                Salario_hora_establo: obtenerValorConEspacio("#sueldo_hora_emp"),
                                Es_dir_admr_gte_gral: $("#director_emp").is(':checked'),
                                PTU: $("#derecho_ptu_emp").is(':checked'),
                                IMSS: $("#retener_seguro_emp").is(':checked'),
                                CAS: $("#aplicar_subsidio_emp").is(':checked'),
                                Pensionado: false,
                                Deshab_imptos: $("#deshabilitar_impuestos_emp").is(':checked'),
                                Calc_isr_anual: $("#calcular_isr_emp").is(':checked'),
                                Calle: obtenerValorConEspacio("#direc_domicilio_emp"),
                                Nombre_calle: obtenerValorConEspacio("#direc_calle_emp"),
                                Num_exterior: obtenerValorConEspacio("#direc_no_exterior_emp"),
                                Colonia: obtenerValorConEspacio("#direc_colonia_emp"),
                                Poblacion: obtenerValorConEspacio("#direc_poblacion_emp"),
                                Referencia: obtenerValorConEspacio("#direc_referencia_emp"),
                                Ciudad_id: obtenerValorConEspacio("#direc_ciudad_emp"),
                                Codigo_postal: obtenerValorConEspacio("#direc_cp_emp"),
                                Telefono1: obtenerValorConEspacio("#direc_telefono1_emp"),
                                Telefono2: obtenerValorConEspacio("#direc_telefono2_emp"),
                                Email: obtenerValorConEspacio("#direc_email_emp"),
                                Sexo: obtenerValorConEspacio("#sexo_emp"),
                                Fecha_nacimiento: fecha_nac,
                                Ciudad_nacimiento_id: obtenerValorConEspacio("#lugar_nacimiento_emp"),
                                Estado_civil: obtenerValorConEspacio("#estado_civil_emp"),
                                Num_hijos: obtenerValorConEspacio("#numero_hijos_emp"),
                                Nombre_padre: obtenerValorConEspacio("#nombre_padre_emp"),
                                Nombre_madre: obtenerValorConEspacio("#nombre_madre_emp"),
                                RFC: rfc,
                                CURP: obtenerValorConEspacio("#curp_emp"),
                                Reg_imss: obtenerValorConEspacio("#seguridad_social_emp")
                            };

                            var fileData = new FormData();
                            var fileUpload = $("#pdf_upload").get(0);
                            var files = fileUpload.files;
                            for (var i = 0; i < files.length; i++) {
                                fileData.append('file', files[i]);
                            }

                            fileData.append('empleado', JSON.stringify(empleado));

                            $.ajax({
                                url: '../NOMINA/GuardarSolicitudAltaEmpleado',
                                type: "POST",
                                contentType: false,
                                processData: false,
                                data: fileData,
                                success: function (result) {
                                    jsRemoveWindowLoad();

                                    if (result == "-2") {
                                        iziToast.warning({
                                            title: 'EL ARCHIVO DEBE TENER FORMATO PDF',
                                            message: ''
                                        });
                                    }

                                    else if (result == "-3") {
                                        iziToast.error({
                                            title: 'OCURRIÓ UN PROBLEMA AL GUARDAR EL ARCHIVO',
                                            message: ''
                                        });
                                    }

                                    else if (result == "-4") {
                                        iziToast.info({
                                            title: 'NO SE DETECTARON ARCHIVOS',
                                            message: ''
                                        });
                                    }

                                    else if (result == "-5") {
                                        iziToast.warning({
                                            title: 'OCURRIÓ UN ERROR EL REGISTRAR LA SOLICITUD',
                                            message: ''
                                        });
                                    }

                                    else if (result == "0") {
                                        iziToast.success({
                                            title: 'SE REGISTRO CORRECTAMENTE AL EMPLEADO',
                                            message: ''
                                        });
                                        limpiarFormularioSolicitud();
                                        limpiarFormulario();
                                        ValidarFormaPago();
                                        $("#pdf_upload").val('');
                                    }

                                },
                                error: function (err) {
                                    jsRemoveWindowLoad();
                                }
                            });

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

            } else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'La fecha de nacimiento es obligatoria',
                });
            }
        } else {
            $("#rfc_emp_1, #rfc_emp_2, #rfc_emp_3").css("border", "1px solid red");
        }
    }
}

function MostrarSolicitudPreRegistroEmpleadosNomina(id_solicitud) {
    limpiarFormulario();
    $("#m_solicitud_registro_empleado").modal("show");
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/MostrarSolicitudPreRegistroEmpleadosNomina",
        data: {
            id_solicitud: id_solicitud
        },
        success: function (response) {
            //#region CARGAR DATOS
            var data = $.parseJSON(response);

            $("#numero_emp_s").val(data[0].Numero);
            numero_edit = data[0].Numero;




            $("#nombre_completo_emp_s").val(data[0].Nombre_completo);
            $("#apellido_paterno_emp_s").val(data[0].Apellido_paterno);
            $("#apellido_materno_emp_s").val(data[0].Apellido_materno);
            $("#nombre_emp_s").val(data[0].Nombres);
            $("#puesto_emp_s").val(data[0].Puesto_no_id);
            $("#departamento_emp_s").val(data[0].Depto_no_id);
            $("#frecuencia_emp_s").val(data[0].Frepag_id);
            $("#registro_patronal_emp_s").val(data[0].Reg_patronal_id);

            if (data[0].Es_jefe === true) {
                $("#es_jefe_emp_s").prop('checked', true);
            } else {
                $("#es_jefe_emp_s").prop('checked', false);
            }

            $("#forma_pago_emp_s").val(data[0].Forma_pago);
            $("#contrato_general_emp_s").val(data[0].Contrato);
            $("#turno_emp_s").val(data[0].Turno);
            $("#jornada_emp_s").val(data[0].Jornada);

            /*
            if (data[0].Regimen_fiscal == 2) {
                ("#regimen_emp").val("02");
            }
            */
            $("#contrato_sat_emp_s").val(data[0].Contrato_sat);
            $("#jornada_sat_emp_s").val(data[0].Jornada_sat);

            if (data[0].Es_sindicalizado === 'S') {
                $("#sindicalizado_emp_s").prop('checked', true);
            } else {
                $("#sindicalizado_emp_s").prop('checked', false);
            }



            $("#estatus_emp_s").val(data[0].Estatus);
            $("#zona_salario_minimo_emp_s").val(data[0].Zona_salmin);
            $("#antigüedades_emp_s").val(data[0].Tabla_antig_id);
            $("#tipo_salario_emp_s").val(data[0].Tipo_salario);
            $("#porcentaje_integracion_emp_s").val(data[0].Pctje_integ);
            $("#sueldo_diario_emp_s").val(data[0].Salario_diario);
            $("#sueldo_hora_emp_s").val(data[0].Salario_hora);
            $("#sueldo_diario_integrado_emp_s").val(data[0].Salario_integ);

            if (data[0].Es_dir_admr_gte_gral === true) {
                $("#director_emp_s").prop('checked', true);
            } else {
                $("#director_emp_s").prop('checked', false);
            }

            if (data[0].PTU === true) {
                $("#derecho_ptu_emp_s").prop('checked', true);
            } else {
                $("#derecho_ptu_emp_s").prop('checked', false);
            }

            if (data[0].IMSS === true) {
                $("#retener_seguro_emp_s").prop('checked', true);
            } else {
                $("#retener_seguro_emp_s").prop('checked', false);
            }

            if (data[0].CAS === true) {
                $("#aplicar_subsidio_emp_s").prop('checked', true);
            } else {
                $("#aplicar_subsidio_emp_s").prop('checked', false);
            }

            if (data[0].DESHAB_IMPTOS === true) {
                $("#deshabilitar_impuestos_emp_s").prop('checked', true);
            } else {
                $("#deshabilitar_impuestos_emp_s").prop('checked', false);
            }

            if (data[0].CALC_ISR_ANUAL === true) {
                $("#calcular_isr_emp_s").prop('checked', true);
            } else {
                $("#calcular_isr_emp_s").prop('checked', false);
            }

            $("#direc_domicilio_emp_s").val(data[0].Calle);
            $("#direc_calle_emp_s").val(data[0].Nombre_calle);
            $("#direc_no_exterior_emp_s").val(data[0].Num_exterior);
            $("#direc_colonia_emp_s").val(data[0].Colonia);
            $("#direc_poblacion_emp_s").val(data[0].Poblacion);
            $("#direc_referencia_emp_s").val(data[0].Referencia);
            $("#direc_ciudad_emp_s").val(data[0].Ciudad_id);
            $("#direc_cp_emp_s").val(data[0].Codigo_postal);
            $("#direc_telefono1_emp_s").val(data[0].Telefono1);

            $("#direc_email_emp_s").val(data[0].Email);
            $("#sexo_emp_s").val(data[0].Sexo);



            var fecha = new Date(data[0].Fecha_nacimiento);
            var fechaFormatoInput = fecha.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_nacimiento_emp_s").val(fechaFormatoInput);

            var fechaing = new Date(data[0].Fecha_ingreso);
            var fechaFormatoInputingre = fechaing.toISOString().substring(0, 10); // Formato YYYY-MM-DD

            $("#fecha_ingreso_emp_s").val(fechaFormatoInputingre);

            $("#div_ingreso_s").css("display", "block");
            $("#estatus_emp_s").prop("disabled", false);



            $("#lugar_nacimiento_emp_s").val(data[0].Ciudad_nacimiento_id);
            $("#estado_civil_emp_s").val(data[0].Estado_civil);
            $("#numero_hijos_emp_s").val(data[0].Num_hijos);
            $("#nombre_padre_emp_s").val(data[0].Nombre_padre);
            $("#nombre_madre_emp_s").val(data[0].Nombre_madre);


            var rfc = data[0].RFC;
            var partesRFC = rfc.split('-');
            if (partesRFC.length > 2) {
                $("#rfc_emp_1_s").val(partesRFC[0]);
                $("#rfc_emp_2_s").val(partesRFC[1]);
                $("#rfc_emp_3_s").val(partesRFC[2]);
            }
            else {
                $("#rfc_emp_1_s").val(rfc.substring(0, 4));
                $("#rfc_emp_2_s").val(rfc.substring(4, 10));
                $("#rfc_emp_3_s").val(rfc.substring(10));
            }

            rfc_edit = data[0].RFC;

            $("#curp_emp_s").val(data[0].CURP);
            $("#seguridad_social_emp_s").val(data[0].Reg_imss);
            $("#grupo_pago_emp_s").val(data[0].Grupo_pago_elect_id);
            $("#tipo_cuenta_emp_s").val(data[0].Tipo_ctaban_pago_elect);
            $("#numero_cuenta_emp_s").val(data[0].Num_ctaban_pago_elect);
            //#endregion
            if ($("#forma_pago_emp_s").val() == "T") {
                $("#grupo_pago_emp_s").prop("disabled", false);
                $("#tipo_cuenta_emp_s").prop("disabled", false);
                $("#numero_cuenta_emp_s").prop("disabled", false);
            }


            $("#sueldo_diario_real_emp_s").val(data[0].Salario_diario_establo);
            $("#sueldo_hora_real_emp_s").val(data[0].Salario_hora_establo);


            if (data[0].Salario_diario == data[0].Salario_diario_establo) {
                $("#sueldo_diario_emp_s").prop("disabled", false);
            }
            else {
                $("#sueldo_diario_emp_s").prop("disabled", true);
            }
            $("#estatus_emp_s").prop("disabled", true);


            $("#grupo_pago_emp_s").prop("disabled", true);
            $("#tipo_cuenta_emp_s").prop("disabled", true);
            $("#numero_cuenta_emp_s").prop("disabled", true);
            $("#forma_pago_emp_s").prop("disabled", true);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });

}

function limpiarFormularioSolicitud() {
    var inputsTexto = document.querySelectorAll('.limpiar_input_s');
    inputsTexto.forEach(function (input) {
        input.value = '';
    });

    var checkboxes = document.querySelectorAll('.limpiar_check_s');
    checkboxes.forEach(function (checkbox) {
        checkbox.checked = false;
    });
    ActualizarDepartamentoMS
    var selects = document.querySelectorAll('.limpiar_select_s');
    selects.forEach(function (select) {
        select.selectedIndex = 0;
    });

    var datetimeInputs = document.querySelectorAll('.limpiar_datetime_s');
    var fechaHoy = new Date().toISOString().split('T')[0];
    datetimeInputs.forEach(function (input) {
        input.value = fechaHoy;
    });
    ValidarFormaPago();
    $("#pdf_upload").val('');
    DeterminarPorcentajeIntegracionInicial();
}


function DeterminarPorcentajeIntegracionInicial() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/DeterminarPorcentajeIntegracionInicial",
        data: {
        },
        success: function (response) {
            $("#porcentaje_integracion_emp").val(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
//#endregion


//#region REPORTE INCIDENCIA
function ConsultarReporteIncidencias() {
    var tipo_incidencia = $("#incidencia_reporte").val();
    var fecha_inicio = $("#fecha_incidencia_reporte1").val();
    var fecha_fin = $("#fecha_incidencia_reporte2").val();

    if (fecha_inicio != "" && fecha_fin != "") {
        $("#div_reporte_empleado_incidencias").html('');
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ConsultarReporteIncidencias",
            data: {
                tipo_incidencia: tipo_incidencia,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            },
            success: function (response) {
                try {
                    $("#tabla_incidencias_reporte").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_reporte_empleado_incidencias").html(response);
                $('#tabla_incidencias_reporte').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: false,
                    pagingType: 'simple_numbers',
                    pageLength: 10
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
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}

function ExportarIncidenciasExcel() {
    var tipo_incidencia = $("#incidencia_reporte").val();
    var fecha_inicio = $("#fecha_incidencia_reporte1").val();
    var fecha_fin = $("#fecha_incidencia_reporte2").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../NOMINA/ExportarIncidenciasExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                tipo_incidencia: tipo_incidencia,
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
                var filename = 'Reporte_Incidencias.xlsx';

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

function ExportarPreRegistroExcel() {
    var estatus = $("#id_estatus").val();
    var fecha_inicio = $("#fecha_incidencia_reporte1").val();
    var fecha_fin = $("#fecha_incidencia_reporte2").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../NOMINA/ExportarPreRegistroExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                estatus: estatus,
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
                    var filename = 'Reporte_Incidencias.xlsx';

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


//#region RECIBOS DE NOMINA
function ConsultarNominasMesAnioSelect() {
    var id_anio = $("#id_anio").val();
    var id_mes = $("#id_mes").val();
    var id_frecuencia_pago = $("#id_frecuencia_pago").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        data: {
            id_anio: id_anio,
            id_mes: id_mes,
            id_frecuencia_pago: id_frecuencia_pago
        },
        url: "../NOMINA/ConsultarNominasMesAnioSelect",
        success: function (response) {
            jsRemoveWindowLoad();
            $("#id_nomina").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error en la solicitud'
            });
        }
    });
}

function ConsultarReciboNominaEmpleado() {
    var id_nomina = $("#id_nomina").val();
    var id_empleado = $("#id_empleado").val();
    if (id_empleado == undefined || id_empleado == '') {
        iziToast.info({
            title: 'Seleccione un empleado',
            message: ''
        });
        return;
    }
    if (id_nomina == undefined || id_nomina == '') {
        iziToast.info({
            title: 'Seleccione una nómina',
            message: ''
        });
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        data: {
            id_nomina: id_nomina,
            id_empleado: id_empleado
        },
        url: "../NOMINA/ConsultarReciboNominaEmpleado",
        success: function (response) {
            jsRemoveWindowLoad();

            if (response == "") {
                iziToast.info({
                    title: 'Recibo no encontrado con el empleado solicitado',
                    message: ''
                });
                return;
            }
            $("#div_formato_codigos").css("display", "block");
            $("#div_formato_codigos").css("width", "75%");
            $("#div_formato_codigos").html(response);
            var HTML_Width = $("#div_formato_codigos").width();
            var HTML_Height = $("#div_formato_codigos").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#div_formato_codigos")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/png", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Recibo Nómina.pdf");
            });
            $("#div_formato_codigos").css("display", "none");

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error en la solicitud'
            });
        }
    });
}




//#endregion


