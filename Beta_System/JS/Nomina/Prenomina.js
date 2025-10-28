function ConsultarDepartamentosUsuarioSelect() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarDepartamentosUsuarioSelect",
        data: {},
        success: function (response) {
            $(".select_areas_nominas").html(response);

            $("#id_areas_prenomina_consulta").html(response);
            $("#id_areas_prenomina_usuario").html(response);
            var newOption = $('<option></option>').val('0').text('Todas');
            $("#id_areas_prenomina_consulta").prepend(newOption);
            $("#id_areas_prenomina_consulta").val("0")
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarDepartamentosPrenominaSelect() {
    var id_prenomina_g = $("#id_prenomina_g_consulta").val();
    if (id_prenomina_g == undefined || id_prenomina_g == '') {
        iziToast.warning({
            title: 'Seleccione una prenomina para consultar las áreas',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ConsultarDepartamentosPrenominaSelect",
            data: { id_prenomina_g: id_prenomina_g },
            success: function (response) {
                $("#id_areas_prenomina_consulta").html(response);
                var newOption = $('<option></option>').val('0').text('Todas');
                $("#id_areas_prenomina_consulta").prepend(newOption);
                $("#id_areas_prenomina_consulta").val("0")
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    
}


function ConsultarNominasGeneradasSelect() {
    var id_anio = $("#id_anio_prenomina_consulta").val();
    var id_establo = $("#id_establo_prenomina_consulta").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ConsultarNominasGeneradasSelect",
        data: {
            id_anio: id_anio,
            id_establo: id_establo
        },
        success: function (response) {
            $("#id_prenomina_g_consulta").html(response);
            jsRemoveWindowLoad();

            ConsultarDepartamentosPrenominaSelect();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarNominasAreasTable(id_nomina_g) {
    var id_nomina_g = $("#id_prenomina_g_consulta").val();
    var id_areas = $("#id_areas_prenomina_consulta").val();
    if (id_nomina_g == "" || id_nomina_g == undefined || id_nomina_g == null) {
        iziToast.warning({
            title: 'Seleccione una prenómina para consultar',
            message: '',
        });
        $("#div_nominas_areas").html("");
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 1200000,
            url: "../NOMINA/ConsultarNominasAreasTable",
            data: {
                id_nomina_g: id_nomina_g,
                id_areas: id_areas
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#div_nominas_areas").html(response);
                $('#m_conceptos_empleado').on('hidden.bs.modal', function (e) {
                    if (id_nomina_d_area_semaforo != 0) {
                        ConsultarNominasAreaTable(id_nomina_d_area_semaforo);
                    }
                });

            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }


}

function ConsultarConceptosEmpleadoNomina(id_nomina_d_area_empleado, id_nomina_g, id_nomina_d_area, id_empleado, id_status) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ConsultarConceptosEmpleadoNomina",
        data: { id_nomina_d_area_empleado: id_nomina_d_area_empleado },
        success: function (response) {
            $("#id_concepto_nuevo").val('0');
            $("#btn_agregar_concepto").attr("onclick", "AgregarConceptoNominaEmpleado(" + id_nomina_d_area_empleado + ", " + id_nomina_g + ", " + id_nomina_d_area +")");
            $("#div_conceptos_empleado").html(response);
            $("#div_conceptos_empleado").css("display", "none");
            $("#div_conceptos_empleado").toggle("slide");
            $("#m_conceptos_empleado").modal("show");
            try {
                if (id_empleado != undefined) {
                    //MostrarInasistenciaEmpleadoTable(id_empleado, id_nomina_g, 2);
                    ConsultarChecadorEmpleadoNomina(id_empleado, id_nomina_g, 2);
                    if (id_status == 3) {
                        $(".btn_valid_eliminar_inasistencia").css("display", "none");
                    }
                    else {
                        $(".btn_valid_eliminar_inasistencia").css("display", "block");
                    }
                }
            } catch (e) {

            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarChecadorEmpleadoNomina(id_empleado, id_nomina_g) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../NOMINA/ConsultarChecadorEmpleadoNomina",
        data: {
            id_empleado: id_empleado,
            id_nomina_g: id_nomina_g
        },
        success: function (response) {
            $("#div_faltas_empleado_concepto").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function EliminarConceptoEmpleado(id_concepto_empleado, id_nomina_d_area_empleado, id_nomina_g, id_nomina_d_area) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de eliminar el concepto?',
        position: 'center',
        buttons: [
            ['<button><b>Si, eliminar concepto</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/EliminarConceptoEmpleado",
                    data: { id_concepto_empleado: id_concepto_empleado },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Concepto eliminado correctamente',
                                message: '',
                            });
                            RecalcularNominaEmpleado(id_nomina_d_area_empleado);
                            ConsultarConceptosEmpleadoNomina(id_nomina_d_area_empleado, id_nomina_g, id_nomina_d_area);
                            id_nomina_d_area_semaforo = id_nomina_d_area;
                        }
                        if (response == -1) {
                            iziToast.error({
                                title: 'Ocurrió un error inesperado al eliminar el concepto',
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

function RecalcularNominaEmpleado(id_nomina_d_area_empleado) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/RecalcularNominaEmpleado",
        data: { id_nomina_d_area_empleado: id_nomina_d_area_empleado },
        success: function (response) {
            if (response == "False") {
                iziToast.error({
                    title: 'Ocurrió un error al recalcular el importe del empleado',
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

function AgregarConceptoNominaEmpleado(id_nomina_d_area_empleado, id_nomina_g, id_nomina_d_area) {
    var id_concepto = $("#id_concepto_nuevo").val();
    var valor = $("#valor_concepto_nuevo").val();
    var comentarios = $("#comentarios_concepto_nuevo").val();
    if (id_concepto == 0 || id_concepto == undefined) {
        iziToast.error({
            title: 'Seleccione un concepto',
            message: '',
        });
    }
    else if (valor <= 0 || valor == undefined || valor == "") {
        iziToast.error({
            title: 'Ingrese un valor valido',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/AgregarConceptoNominaEmpleado",
            data: {
                id_nomina_d_area_empleado: id_nomina_d_area_empleado,
                id_nomina_g: id_nomina_g,
                id_concepto: id_concepto,
                valor: valor,
                comentarios: comentarios
            },
            success: function (response) {
                if (response == 0) {
                    $("#comentarios_concepto_nuevo").val("");
                    $("#valor_concepto_nuevo").val("");
                    iziToast.success({
                        title: 'Concepto registrado correctamente',
                        message: '',
                    });
                    RecalcularNominaEmpleado(id_nomina_d_area_empleado);
                    ConsultarConceptosEmpleadoNomina(id_nomina_d_area_empleado, id_nomina_g);
                    id_nomina_d_area_semaforo = id_nomina_d_area;
                }
                else if (response == -1) {
                    iziToast.warning({
                        title: 'El empleado ya contiene este concepto',
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

function AsignarConceptoMasivoPrenomina(id_nomina_g) {
    $("#m_conceptos_masivos").modal("show");
    $("#id_nomina_g_masivo").val(id_nomina_g);
    $("input[name='radio_tipo_busqueda']").prop('checked', false);
    $(".formulario_salario").css("display", "none");
    $(".formulario_puesto").css("display", "none");

    $(".formulario_conceptos").css("display", "none");
    $("#div_empleados_conceptos_masivos").html("");
}

function MostrarFiltrosConceptosMasivos() {
    var tipo_filtro = $("input[name='radio_tipo_busqueda']:checked").val();
    //-------PUESTO
    if (tipo_filtro == 1) {
        $(".formulario_salario").css("display", "none");
        $(".formulario_puesto").css("display", "block");
    }
    //-------SALARIO
    else if (tipo_filtro == 2) {
        $(".formulario_puesto").css("display", "none");
        $(".formulario_salario").css("display", "block");
    }
    $(".formulario_conceptos").css("display", "none");
    $(".formulario_nuevo_salario").css("display", "none");
    $("#salario_diario_conceptos_masivos").val("");
    $("#div_empleados_cambio_salario_empleados").html("");
}

function ConsultarEmpleadosMasivos(modo) {
    var id_nomina_g = $("#id_nomina_g_masivo").val();

    var id_puesto = 0;
    var salario_diario = 0;
    if (modo == 1) {
        id_puesto = $("#id_puesto_conceptos_masivos").val();
        salario_diario = 0;
    }
    else if (modo == 2) {
        id_puesto = 0
        salario_diario = $("#salario_diario_conceptos_masivos").val();
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../NOMINA/ConsultarEmpleadosMasivos",
        data: {
            id_puesto: id_puesto,
            salario_diario: salario_diario,
            id_nomina_g: id_nomina_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_empleados_masivos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_empleados_conceptos_masivos").html(response);
            $("#datatable_empleados_masivos").DataTable({
                keys: false,
                ordering: true,
                paging: false,
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



function ConsultarNominasAreaTable(id_nomina_d_area) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ConsultarNominasAreaTable",
        data: { id_nomina_d_area: id_nomina_d_area },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_" + id_nomina_d_area + "").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_nomina_area_" + id_nomina_d_area + "").html(response);
            $("#datatable_" + id_nomina_d_area + "").DataTable({
                keys: false,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
            id_nomina_d_area_semaforo = 0;
            $("#btn_refresh_importe").css("display", "block");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GenerarTXTExcepciones(id_nomina_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/GenerarTXTExcepciones",
        data: {
            id_nomina_g: id_nomina_g
        },
        success: function (response) {
            if (response == "-1") {
                iziToast.warning({
                    title: 'Ocurrió un error el generar el TXT de excepciones',
                    message: '',
                });
            }
            else { window.open(response, '_blank'); }            
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GenerarPrenominaBeta() {
    var checkboxes = document.querySelectorAll('input[name="checkout_area_check"]:checked');
    var areas = [];
    checkboxes.forEach((checkbox) => {
        areas.push(checkbox.value);
    });

    var id_establo = $("#id_establo_prenomina_consulta").val();
    if (id_establo == "" || id_establo == undefined) {
        iziToast.warning({
            title: 'No se detectaron establos asignados',
            message: '',
        });
    }
    else if (areas.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 área para generar la prenómina',
            message: '',
        });
    }

    else {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 80000,
            url: "../NOMINA/ValidarGenerarPrenominaBeta",
            data: {
                id_establo: id_establo,
                areas: areas
            },
            success: function (response) {
                if (response == 1) {
                    iziToast.error({
                        title: 'Ocurrió un error al validar la nomina',
                        message: '',
                    });
                }
                else if (response == -1) {
                    iziToast.warning({
                        title: 'No se encontró el año actual registrado',
                        message: '',
                    });
                }
                else if (response == -2) {
                    iziToast.warning({
                        title: 'Ocurrió un error al calcular las fechas',
                        message: '',
                    });
                }
                else if (response == -4) {
                    iziToast.warning({
                        title: 'No se detectaron áreas asignadas al usuario',
                        message: '',
                    });
                }
                else if (response == -3) {
                    iziToast.error({
                        title: 'Para generar una nueva prenomina cierre la ultima prenomina de este establo',
                        message: '',
                    });
                }
                else if (response == -5) {
                    iziToast.info({
                        title: 'Se detectaron prenóminas con las áreas seleccionadas ya generadas',
                        message: '',
                    });
                }
                else if (response == 0) {
                    var msj = "";
                    var title = "";
                    if (response == 0) {
                        title = "ADVERTENCIA"
                        msj = "¿Está seguro de generar una nueva prenomina? Esto tardará unos minutos";
                    }
                    else {
                        title = "ADVERTENCIA. ¿Está seguro de recalcular la prenómina?";
                        msj = "Añadirá las nuevas areas asignadas del usuario en caso de existir. Esto tardará unos minutos"
                    }
                    iziToast.question({
                        timeout: 20000,
                        close: false,
                        overlay: true,
                        displayMode: 'once',
                        id: 'question',
                        zindex: 999999,
                        title: title,
                        message: msj,
                        position: 'center',
                        buttons: [
                            ['<button><b>Si, generar</b></button>', function (instance, toast) {
                                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                                jsShowWindowLoad();
                                $.ajax({
                                    type: "POST",
                                    async: true,
                                    timeout: 900000,
                                    url: "../NOMINA/GenerarPrenominaBeta",
                                    data: {
                                        id_establo: id_establo,
                                        areas: areas
                                    },
                                    success: function (response) {
                                        var response_msj = response.split('|');

                                        $("#m_generar_prenomina").modal("hide");
                                        jsRemoveWindowLoad();
                                        if (response_msj[0] == -1) {
                                            iziToast.warning({
                                                title: 'No se encontró el año actual registrado',
                                                message: '',
                                            });
                                        }
                                        else if (response_msj[0] == -2) {
                                            iziToast.warning({
                                                title: 'Ocurrió un error al calcular las fechas de la prenómina',
                                                message: '',
                                            });
                                        }
                                        else if (response_msj[0] == -3) {
                                            iziToast.warning({
                                                title: 'La prenómina actual ya fue cerrada',
                                                message: 'Eliminela para generar una nueva',
                                            });
                                        }
                                        else if (response_msj[0] == 0) {
                                            iziToast.error({
                                                title: 'Erorr inesperado al generar la prenómina',
                                                message: '',
                                            });
                                        }
                                        else if (response_msj[0] > 0) {

                                            if (response_msj[1] > 0) {
                                                iziToast.error({
                                                    title: 'Prenomina incompleta error en el empleado',
                                                    message: "Empleado No. :" + response_msj[1] + "",
                                                });
                                                ConsultarNominasGeneradasSelect();
                                                $("#id_prenomina_g_consulta").val(response_msj[0]);
                                                ConsultarNominasAreasTable(response_msj[0]);
                                            }
                                            else {
                                                iziToast.success({
                                                    title: 'Prenómina generada correctamente',
                                                    message: '',
                                                });
                                                ConsultarNominasGeneradasSelect();
                                                $("#id_prenomina_g_consulta").val(response_msj[0]);
                                                ConsultarNominasAreasTable(response_msj[0]);
                                            }

                                        }
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        jsRemoveWindowLoad();
                                        if (textStatus === 'timeout') {
                                            iziToast.error({
                                                title: 'Se excedió el tiempo de ejcución.',
                                                message: 'Favor de contactar a desarrollo',
                                            });
                                        } else {
                                            alert('Ocurrió un problema:', textStatus, errorThrown);
                                        }
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
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    
}


function FinalizarNóminaBeta(id_nomina_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de que desea finalizar la prenómina? No podrá generar una nueva hasta la proxima semana',
        position: 'center',
        buttons: [
            ['<button><b>Si, finalizar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../NOMINA/FinalizarNóminaBeta",
                    data: {
                        id_nomina_g: id_nomina_g
                    },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response == 0) {
                            iziToast.success({
                                title: 'Prenómina finalizada correctamente',
                                message: '',
                            });
                            ConsultarNominasAreasTable();
                            //ConsultarNominasFinalizadasSelect();
                        }
                        else if (response == 1) {
                            iziToast.warning({
                                title: 'La Prenómina no tiene el status de registrada',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al finalizar la Prenómina',
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


function ConsultarNominasFinalizadasSelect() {
    var no_anio = $("#id_anio_prenomina_finalizadas").find('option:selected').text();
    var no_mes = $("#id_mes_prenomina_finalizadas").val();
    var id_establo = $("#id_establo_prenomina_finalizadas").val();

    if (id_establo == "" || id_establo == undefined) {
        iziToast.warning({
            title: 'No se detectaron establos asignados',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ConsultarNominasFinalizadasSelect",
            data: {
                no_anio: no_anio,
                no_mes: no_mes,
                id_establo: id_establo
            },
            success: function (response) {
                $("#id_prenomina_finalizadas").html(response);
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}

function ConsultarNominasFinalizadas() { 

    var id_nomina_g = $("#id_prenomina_finalizadas").val();
    if (id_nomina_g == "" || id_nomina_g == undefined || id_nomina_g == null) {
        iziToast.warning({
            title: 'Seleccione una prenómina para consultar',
            message: '',
        });
        $("#div_nominas_terminadas").html("");
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeout: 1200000,
            url: "../NOMINA/ConsultarNominasFinalizadas",
            data: {
                id_nomina_g: id_nomina_g
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#div_nominas_terminadas").html(response);
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });

    }
}

function GenerarFormatoNominaConceptos(id_nomina_g, id_departamento, modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../NOMINA/GenerarFormatoNominaConceptos",
        data: {
            id_nomina_g: id_nomina_g,
            id_departamento: id_departamento,
            modo: modo
        },
        success: function (response) {
            try {
                if (modo == 1) {
                    html2pdf()
                        .from(response)
                        .set({
                            margin: [15, 15, 15, 15],
                            filename: "Prenomina.pdf",
                            image: { type: 'png', quality: 2 },
                            html2canvas: { scale: 1.1 },
                            jsPDF: { unit: 'pt', format: [612, 936], orientation: 'landscape' }, // Oficio: 8.5 x 13 pulgadas
                            pagebreak: { mode: ['avoid-all', 'css', 'legacy'] }
                        })
                        .save()
                        .then(function () {
                            jsRemoveWindowLoad();
                        });
                }
                else {
                    html2pdf()
                        .from(response)
                        .set({
                            margin: [15, 15, 15, 15],
                            filename: "PRENOMINA: " + id_nomina_g +".pdf",
                            image: { type: 'png', quality: 2 },
                            html2canvas: { scale: 1.1 },
                            jsPDF: { unit: 'pt', format: [612, 936], orientation: 'landscape' }, // Oficio: 8.5 x 13 pulgadas
                        })
                        .save()
                        .then(function () {
                            jsRemoveWindowLoad();
                        });
                }

            } catch (e) {
                jsRemoveWindowLoad();
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function EliminarPrenominaBeta(id_nomina_g) {
    iziToast.question({
        timeout: null,
        close: false,
        overlay: true,
        titleSize: '20px',
        messageSize: '18px',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de eliminar la prenómina?',
        position: 'center',
        color: "red",
        maxWidth: 1000,
        icon: 'icon-contacts',
        displayMode: 2,
        class: 'custom1',
        balloon: false,
        //transitionIn: 'flipInX',
        //transitionOut: 'flipOutX',
        progressBarColor: 'rgb(0, 255, 184)',
        image: '/Content/img_layout/Logo_beta_icono.png',
        imageWidth: 70,
        layout: 2,

        drag: false,
        inputs: [
            ["<strong>MOTIVO: </strong><input type='text' id='motivo_rechazo' style='width:500px;'>", "keyup", function (instance, toast, input, e) {
                console.info(input.value);
            }, true]
        ],
        buttons: [
            ['<button><b>Si, eliminar prenómina</b></button>', function (instance, toast) {
                var motivo = $("#motivo_rechazo").val();
                if (motivo == "" || motivo == undefined) {
                    iziToast.error({
                        title: 'Ingrese el motivo de la eliminación de la prenómina',
                        message: '',
                    });
                }
                else if (motivo.length <= 20) {
                    iziToast.info({
                        title: 'Ingrese un motivo de al menos 20 caracteres',
                        message: '',
                    });
                }
                else if (motivo.length > 500) {
                    iziToast.info({
                        title: 'Ingresa un motivo de no mas de 200 caracteres.',
                        message: '',
                    });
                }
                else {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 12000000,
                        url: "../NOMINA/EliminarPrenominaBeta",
                        data: {
                            id_nomina_g: id_nomina_g,
                            motivo: motivo
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Prenomina eliminada correctamente',
                                    message: '',
                                });
                                $("#div_nominas_areas").html("");
                                ConsultarNominasGeneradasSelect();
                                ConsultarNominasFinalizadasSelect();
                            }
                            else if (response == 1) {
                                iziToast.error({
                                    title: 'Ocurrió un problema al eliminar la prenómina',
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


function ValidarTipoConceptoLabel() {
    var id_concepto = $("#id_concepto_nuevo").val();
    var valores = ["53628", "55446", "53628"];

    if (valores.includes(id_concepto)) {
        $("#lblTipoConcepto").text("Dia(s):");
    }
    else {
        $("#lblTipoConcepto").text("Valor ($):");
    }
}

function ValidarTipoConceptoMasivoLabel() {
    var id_concepto = $("#id_concepto_nuevo_masivo").val();
    var valores = ["53628", "55446", "53628"];

    if (valores.includes(id_concepto)) {
        $("#lblTipoConceptoMasivo").text("Dia(s):");
    }
    else {
        $("#lblTipoConceptoMasivo").text("Valor ($):");
    }
}

function EliminarInasistenciaPrenomina(id_inasistencia, id_nomina_g, empleado_id, id_nomina_d_area_empleado) {
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
                    message: 'Se elimino con exito la inasistencia'
                });
                ConsultarConceptosEmpleadoNomina(id_nomina_d_area_empleado, id_nomina_g);
                MostrarInasistenciaEmpleadoTable(empleado_id, id_nomina_g, 2);
            }
            if (response == 1) {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'La nomina esta cerrada, no se puede eliminar inasistencias'
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
                    message: 'Ocurrio un problema al realizar los calculos de inasistencia'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarImporteTotalPrenomina(id_nomina_g) {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 12000000,
        url: "../NOMINA/ConsultarImporteTotalPrenomina",
        data: { id_nomina_g: id_nomina_g },
        success: function (response) {
            $(".importe_total_prenomina").text("$"+response);
            $("#btn_refresh_importe").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CheckOutAreasPrenominaGenerada() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo_prenomina_consulta").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 12000000,
        url: "../NOMINA/CheckOutAreasPrenominaGenerada",
        data: { id_establo: id_establo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_generar_prenomina").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AgregarConceptoMasivoNominaEmpleado() {
    var id_nomina_g = $("#id_nomina_g_masivo").val();

    var id_concepto = $("#id_concepto_nuevo_masivo").val();
    var valor = $("#valor_concepto_nuevo_masivo").val();
    var comentarios = $("#comentarios_concepto_nuevo_masivo").val();

    var checkboxes = document.querySelectorAll('input[name="check_nomina_d_area_empleado_masivo"]:checked');
    var id_nomina_d_area_empleado = [];
    checkboxes.forEach((checkbox) => {
        id_nomina_d_area_empleado.push(checkbox.value);
    });

    if (id_nomina_d_area_empleado.length <= 0) {
        iziToast.error({
            title: 'Seleccione al menos 1 empleado para aplicar el concepto',
            message: '',
        });
    }
    else if (id_concepto <= 0 || id_concepto == undefined) {
        iziToast.error({
            title: 'Seleccione un concepto',
            message: '',
        });
    }
    else if (valor <= 0 || valor == undefined || valor == "") {
        iziToast.error({
            title: 'Ingrese un valor valido',
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
            message: '¿Está seguro de aplicar el concepto a los empleados seleccionados?',
            position: 'center',
            buttons: [
                ['<button><b>Si, aplicar concepto</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 999999,
                        url: "../NOMINA/AgregarConceptoMasivoNominaEmpleados",
                        data: {
                            id_nomina_d_area_empleado: id_nomina_d_area_empleado,
                            id_nomina_g: id_nomina_g,
                            id_concepto: id_concepto,
                            valor: valor,
                            comentarios: comentarios
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                $("#comentarios_concepto_nuevo_masivo").val("");
                                $("#valor_concepto_nuevo_masivo").val("");
                                iziToast.success({
                                    title: 'Conceptos masivos registrados correctamente',
                                    message: '',
                                });
                                $(".check_nomina_d_area_empleado_masivo").prop('checked', false);
                                refesh_nomina = true;
                            }
                            else if (response == -1) {
                                iziToast.warning({
                                    title: 'Ocurrió un error al aplicar los conceptos',
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

function LimpiarPrenominaFont() {
    $("#div_nominas_areas").html("");
}

function CheckUncheckConceptosMasivos() {
    if ($("#check_uncheck_conceptos_masivos").is(':checked')) {
        $(".check_nomina_d_area_empleado_masivo").prop("checked", true);
    }
    else {
        $(".check_nomina_d_area_empleado_masivo").prop("checked", false);
    }
}


function DescargarFormatoNominaConceptosPDF(id_nomina_g, id_departamento, modo) {
    jsShowWindowLoad();

    fetch('../NOMINA/DescargarFormatoNominaConceptosPDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            id_nomina_g: id_nomina_g,
            id_departamento: id_departamento,
            modo: modo
        })
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok.');

            return response.blob().then(blob => {
                let disposition = response.headers.get('Content-Disposition');
                let filename = 'Reporte_Fuera_de_Rango.pdf';

                if (disposition && disposition.indexOf('attachment') !== -1) {
                    let matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }

                let link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            });
        })
        .catch(error => {
            console.error('Error:', error);
            iziToast.error({
                title: 'Error',
                message: 'Ocurrió un error al descargar el PDF.'
            });
        })
        .finally(() => {
            jsRemoveWindowLoad();
        });
}



function ConsultarJornadaArea() {
    var id_departamento = $("#id_areas_prenomina_usuario").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9999999,
        url: "../NOMINA/ConsultarJornadaArea",
        data: { id_departamento: id_departamento },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_jornada_laboral").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CambiarColorCeldaConceptoDia(identificador) {
    var celda = $("#cell_" + identificador + "");

    var id_concepto_real = $("#select_conceptos_reales_" + identificador + "").val();
    var id_concepto = $("#select_conceptos_" + identificador + "").val();
    if (id_concepto == 2) { celda.css("background-color", "#eb3c3c"); celda.css("color", "white"); }
    else { celda.css("background-color", "#FFC900"); celda.css("color", "black"); }

    if (id_concepto == id_concepto_real && id_concepto != 2) { celda.css("background-color", ""); celda.css("color", "#2e87d7"); }
    //else { celda.css("background-color", ""); celda.css("color", "#2e87d7"); }
}

function GuardarControlAsistencias() {
    var concepto_real = [];
    var id_empleados = [];
    var conceptos = [];
    var fechas = [];
    var count = 0;

    var id_empleados_valores = [];
    var HR_pagar = [];
    var HR_no = [];
    var HR_monto = [];
    var EE_pagar = [];
    var EE_no = [];
    var EE_monto = [];
    var fechas_valores = [];
    var count_valores = 0;

    var empleados = $(".empleados");
    empleados.each(function () {
        var id_empleado = $(this).val();
        for (var i = 1; i <= 7; i++) {
            var id_concepto = $("#select_conceptos_" + id_empleado + "_" + i + "").val();
            var id_concepto_real = $("#select_conceptos_reales_" + id_empleado + "_" + i + "").val();
            var fecha = $("#fecha_" + id_empleado + "_" + i + "").val();


            id_empleados[count] = id_empleado;
            conceptos[count] = id_concepto;
            concepto_real[count] = id_concepto_real;
            fechas[count] = fecha;
            count++;
        }

        var hra_extra_monto = $("#hrs_extra_monto_valores_" + id_empleado + "").val();
        var hra_extra_no = $("#hrs_extra_no_valores_" + id_empleado + "").val();
        if (hra_extra_no != undefined && hra_extra_monto != undefined) {
            id_empleados_valores[count_valores] = id_empleado;
            fechas_valores[count_valores] = $("#fecha_" + id_empleado + "_1").val();

            HR_no[count_valores] = hra_extra_no;
            HR_monto[count_valores] = hra_extra_monto;
            if ($("#hrs_extra_pagar_valores_" + id_empleado + "").is(':checked')) { HR_pagar[count_valores] = true; }
            else { HR_pagar[count_valores] = false; }

            var estimulo_extra_monto = $("#estimulo_monto_valores_" + id_empleado + "").val();
            var estimulo_extra_no = $("#estimulo_no_valores_" + id_empleado + "").val();
            if (estimulo_extra_no != undefined && estimulo_extra_monto != undefined) {
                EE_no[count_valores] = estimulo_extra_no;
                EE_monto[count_valores] = estimulo_extra_monto;
                if ($("#estimulo_pagar_valores_" + id_empleado + "").is(':checked')) { EE_pagar[count_valores] = true; }
                else { EE_pagar[count_valores] = false; }
                count_valores++;
            }
            else {
                EE_no[count_valores] = 0;
                EE_monto[count_valores] = 0;
                EE_pagar[count_valores] = false;
                count_valores++;
            }
        }
    });

    //alert(HR_pagar);
    //alert(HR_no);
    //alert(HR_monto);

    //alert(EE_pagar);
    //alert(EE_no);
    //alert(EE_monto);



    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9999999,
        url: "../NOMINA/GuardarCAHEAsistenciasFaltas",
        data: {
            id_empleados: id_empleados,
            conceptos: conceptos,
            concepto_real: concepto_real,
            fechas: fechas
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Asistencias y faltas capturadas correctamente',
                    message: '',
                });
                $.ajax({
                    type: "POST",
                    async: true,
                    timeout: 9999999,
                    url: "../NOMINA/GuardarCAHEHorasExtrasEstimulos",
                    data: {
                        id_empleados: id_empleados_valores,
                        HR_pagar: HR_pagar,
                        HR_no: HR_no,
                        HR_monto: HR_monto,
                        EE_pagar: EE_pagar,
                        EE_no: EE_no,
                        EE_monto: EE_monto,
                        fechas: fechas_valores
                    },
                    success: function (response_estimulos) {
                        if (response_estimulos == "True") {
                            jsRemoveWindowLoad();
                            iziToast.success({
                                title: 'Horas extras y estimulos capturados correctamente',
                                message: '',
                            });
                            //ConsultarJornadaArea();
                        }
                        else {
                            jsRemoveWindowLoad();
                            iziToast.error({
                                title: 'Ocurrió un error al guardar las faltas y asistencias',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                        iziToast.error({
                            title: 'Ocurrió un error al guardar las faltas y asistencias',
                            message: '',
                        });
                    }
                });
            }
            else {
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Ocurrió un error al guardar las faltas y asistencias',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            iziToast.error({
                title: 'Ocurrió un error al guardar las faltas y asistencias',
                message: '',
            });
        }
    });
}







