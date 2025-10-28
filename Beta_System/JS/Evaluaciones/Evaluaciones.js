
//#region DEPARTAMENTOS
function AddEditDepartamento(modo, id_departamento, nombre_departamento) {
    var boton = $("#btn_add_edit_departamento");
    if (modo == 1) {
        id_departamento = 0;
        boton.text("GUARDAR");
        boton.attr("onclick", "RegistrarActualizarDepartamentoEvaluacion(" + id_departamento + ");");
    }
    else {
        boton.text("ACTUALIZAR");
        boton.attr("onclick", "RegistrarActualizarDepartamentoEvaluacion(" + id_departamento + ");");
        $("#nombre_departamento").val(nombre_departamento);
    }
    $("#m_departamentos").modal("show");
}
function ConsultarDepartamentosEvaluacionTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../EVALUACIONES/ConsultarDepartamentosEvaluacionTable",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_departamentos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_departamentos").html(response);
            $('#datatable_departamentos').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                searching: true,
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
                responsive: false
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function RegistrarActualizarDepartamentoEvaluacion(id_departmaneto) {
    var nombre_departamento = $("#nombre_departamento").val().trim();
    if (nombre_departamento == "" || nombre_departamento == null) {
        iziToast.warning({
            title: 'Ingresa el nombre del departamento',
            message: '',
        });
    }
    else {
        var departamento = {};
        departamento.id_evaluacion_departamento = id_departmaneto;
        departamento.nombre_departamento = nombre_departamento;
        $.ajax({
            type: "POST",
            async: true,
            url: "../EVALUACIONES/RegistrarActualizarDepartamentoEvaluacion",
            data: {
                departamento: departamento
            },
            success: function (response) {
                $("#nombre_departamento").val('');
                $("#m_departamentos").modal("hide");
                ConsultarDepartamentosEvaluacionTable();
                if (response == 1) {
                    iziToast.success({
                        title: 'Departamento registrado correctamente',
                        message: '',
                    });
                }
                else if (response == 2) {
                    iziToast.success({
                        title: 'Departamento actualizado correctamente',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al ejecutar la operación',
                        message: '',
                    });
                }
                ConsultarDepartamentosEmpleadosTable();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}
function OnOffDepartamento(id_departamento, status) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Atención',
        message: '¿Está seguro de realizar esta operación?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../EVALUACIONES/OnOffDepartamento",
                    data: {
                        id_departamento: id_departamento,
                        status: status
                    },
                    success: function (response) {
                        if (response == "True" && status == true) {
                            iziToast.info({
                                title: 'Departamento activado correctamente',
                                message: '',
                            });
                        }
                        else if (response == "True" && status == false) {
                            iziToast.info({
                                title: 'Departamento desactivado correctamente',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error',
                                message: '',
                            });
                        }
                        ConsultarDepartamentosEvaluacionTable();
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

//#region DEPARTAMENTOS EMPLEADOS
function ConsultarEmpleadosSinDepartamento() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarEmpleadosSinDepartamento",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_empleados_sin_depto").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_empleados_sin_depto").html(response);
            $('#datatable_empleados_sin_depto').DataTable({
                keys: false,
                select: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarDepartamentosEmpleadosTable(id_departamento, nombre_departamento) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../EVALUACIONES/ConsultarDepartamentosEmpleadosTable",
        data: { id_departamento: id_departamento },
        success: function (response) {
            jsRemoveWindowLoad();

            $("#div_departamentos_empleados").html(response);
            $("#btn_ligar_departamentos_empleados").attr("onclick", "AsociarDepartamentoEmpleados(" + id_departamento + ");");
            if (nombre_departamento != undefined || nombre_departamento != "") { $("#nombre_departamento_empleados").text("DEPARTAMENTO: " + nombre_departamento); }
            $("#m_departamentos_empleados").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function AsociarDepartamentoEmpleados(id_departamento) {
    var count = 0;
    var id_empleados = [];
    var tabla = $('#datatable_empleados_sin_depto').DataTable();
    tabla.rows().every(function () {
        var data = this.data(); // Obtener los datos de la fila
        var node = this.node(); // Obtener el nodo de la fila
        var input = $(node).find(".id_empelado_departamento_new");
        if (input.is(':checked')) {
            id_empleados[count] = input.val();
            count++;
        } else {
        }
    });

    if (id_empleados.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 empleado para asociar',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../EVALUACIONES/AsociarDepartamentoEmpleados",
            data: {
                id_departamento: id_departamento,
                id_empleados: id_empleados
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Departamento actualizado correctamente',
                        message: '',
                    });
                    ConsultarEmpleadosSinDepartamento();
                    ConsultarDepartamentosEmpleadosTable(id_departamento, "");
                    ConsultarDepartamentosEvaluacionTable();
                }
                else {
                    iziToast.warning({
                        title: 'Ocurrió un error al guardar',
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
function EliminarLigaEmpleadoDepartamento(id_departamento_empleado, id_departamento) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/EliminarLigaEmpleadoDepartamento",
        data: { id_departamento_empleado: id_departamento_empleado },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Asociacion elimiada correctamente',
                    message: '',
                });
                ConsultarEmpleadosSinDepartamento();
                ConsultarDepartamentosEmpleadosTable(id_departamento, "");
                ConsultarDepartamentosEvaluacionTable();
            }
            else {
                iziToast.warning({
                    title: 'Ocurrió un error al eliminar la asociación',
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
//#endregion


//#region DEPARTAMENTOS USUARIOS
function ConsultarUsuariosCheckTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../EVALUACIONES/ConsultarUsuariosCheckTable",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_usuarios_check").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_usuarios").html(response);
            $('#datatable_usuarios_check').DataTable({
                keys: false,
                select: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarDepartamentosUsuariosTable(id_departamento, nombre_departamento) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../EVALUACIONES/ConsultarDepartamentosUsuariosTable",
        data: { id_departamento: id_departamento },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_departamentos_usuarios").html(response);
            $("#btn_ligar_departamentos_usuarios").attr("onclick", "AsociarDepartamentoUsuarios(" + id_departamento + ");");
            if (nombre_departamento != undefined || nombre_departamento != "") { $("#nombre_departamento_usuarios").text("DEPARTAMENTO: " + nombre_departamento); }
            $("#m_departamentos_usuarios").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AsociarDepartamentoUsuarios(id_departamento) {
    var count = 0;
    var id_usuarios = [];
    var tabla = $('#datatable_usuarios_check').DataTable();
    tabla.rows().every(function () {
        var data = this.data(); // Obtener los datos de la fila
        var node = this.node(); // Obtener el nodo de la fila
        var input = $(node).find(".id_usuario_departamento_new");
        if (input.is(':checked')) {
            id_usuarios[count] = input.val();
            count++;
        } else {
        }
    });

    if (id_usuarios.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 empleado para asociar',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../EVALUACIONES/AsociarDepartamentoUsuarios",
            data: {
                id_departamento: id_departamento,
                id_usuarios: id_usuarios
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Departamento actualizado correctamente',
                        message: '',
                    });
                    ConsultarUsuariosCheckTable();
                    ConsultarDepartamentosUsuariosTable(id_departamento, "");
                    ConsultarDepartamentosEvaluacionTable();
                }
                else {
                    iziToast.warning({
                        title: 'Ocurrió un error al guardar',
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
function EliminarLigaUsuarioDepartamento(id_liga_departamento_usuario, id_departamento) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 9000000,
        url: "../EVALUACIONES/EliminarLigaUsuarioDepartamento",
        data: { id_liga_departamento_usuario: id_liga_departamento_usuario },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Asociacion elimiada correctamente',
                    message: '',
                });
                ConsultarUsuariosCheckTable();
                ConsultarDepartamentosUsuariosTable(id_departamento, "");
                ConsultarDepartamentosEvaluacionTable();
            }
            else {
                iziToast.warning({
                    title: 'Ocurrió un error al eliminar la asociación',
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
//#endregion



//#region RUBROS / CONCEPTOS
function ConsultarRubrosEvaluacionCards() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarRubrosEvaluacionCards",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_rubros_cards").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarConceptosRubroArray(id_rubro) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarConceptosRubroArray",
        data: { id_rubro: id_rubro },
        success: function (response) {
            var data = $.parseJSON(response);
            if (data.length > 0) {
                var tr_conceptos = $("#tr_conceptos");
                for (var i = 0; i < data.length; i++) {
                    var row = $("<tr class='text-center conceptos_rubros' id='concepto_" + data[i].id_evaluacion_concepto_rubro + "'></tr>");

                    row.append("<td id='nombre_concepto_" + data[i].id_evaluacion_concepto_rubro + "' >" + data[i].nombre_concepto + "</td>");
                    row.append("<td class='porcentajes_conceptos' id='porcentaje_concepto_" + data[i].id_evaluacion_concepto_rubro + "' >" + data[i].porcentaje + "</td>");

                    row.append("<td><button class='btn btn_beta_danger' onclick='QuitarConceptoRubro(" + data[i].id_evaluacion_concepto_rubro + ");'><i class='fa fa-remove'></i></button></td>");
                    tr_conceptos.append(row);
                }

                $(".valid_concepto").val('');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AddEditRubro(modo, id_rubro, nombre_rubro, porcentaje_rubro) {
    $("#tr_conceptos").html("");
    var boton = $("#btn_add_edit_rubro");
    boton.attr("onclick", "RegistrarActualizarRubroEvaluacion(" + id_rubro + ");");
    if (modo == 1) {
        boton.text("GUARDAR RUBRO");
        $("#lbl_rubro_title").text('NUEVO RUBRO');
    }
    else {
        boton.text("ACTUALIZAR RUBRO");
        $("#nombre_rubro").val(nombre_rubro);
        $("#porcentaje_rubro").val(porcentaje_rubro);
        $("#lbl_rubro_title").text('ACTUALIZAR RUBRO');
        ConsultarConceptosRubroArray(id_rubro);
    }
    $("#m_rubros_conceptos").modal("show");
}

function AgregarConceptoRubro() {
    var nombre = $("#nombre_concepto").val().trim();
    var porcentaje = parseFloat($("#porcentaje_concepto").val());
    var random = Math.floor(Math.random() * 1001);

    if (nombre == "" || isNaN(porcentaje)) {
        iziToast.warning({
            title: 'Ingresa ambos campos del concepto',
            message: '',
        });
    }
    else {
        if (porcentaje <= 0) {
            iziToast.warning({
                title: 'El porcentaje debe ser mayor a 0',
                message: '',
            });
            return false;
        }

        var valid_decimal = /^-?\d+\.\d+$/.test(porcentaje);
        if (valid_decimal == true) {
            iziToast.warning({
                title: 'El porcentaje no debe llevar deicimales',
                message: '',
            });
        }
        else {
            var suma_porcentaje = 0;
            $(".porcentajes_conceptos").each(function () {
                suma_porcentaje = suma_porcentaje + parseFloat($(this).text());
            });

            suma_porcentaje = suma_porcentaje + porcentaje
            if (suma_porcentaje > 100) {
                iziToast.warning({
                    title: 'El porcentaje de los conceptos del rubro no deben ser mayores de 100%',
                    message: '',
                });
            }
            else {
                var tr_conceptos = $("#tr_conceptos");
                var row = $("<tr class='text-center conceptos_rubros' id='concepto_" + random + "'></tr>");

                row.append("<td id='nombre_concepto_" + random + "' >" + nombre + "</td>");
                row.append("<td class='porcentajes_conceptos' id='porcentaje_concepto_" + random + "' >" + porcentaje + "</td>");

                row.append("<td><button class='btn btn_beta_danger' onclick='QuitarConceptoRubro(" + random + ");'><i class='fa fa-remove'></i></button></td>");
                tr_conceptos.append(row);
                $(".valid_concepto").val('');
                $("#nombre_concepto").focus();
            }
        }
        
    }
}

function QuitarConceptoRubro(id_concepto_rubro) {
    $("#concepto_" + id_concepto_rubro + "").remove();
}

function OnOffRubro(id_rubro, status) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Atención',
        message: '¿Está seguro de realizar esta operación?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../EVALUACIONES/OnOffRubro",
                    data: {
                        id_rubro: id_rubro,
                        status: status
                    },
                    success: function (response) {
                        if (response == "True" && status == true) {
                            iziToast.info({
                                title: 'Rubro activado correctamente',
                                message: '',
                            });
                        }
                        else if (response == "True" && status == false) {
                            iziToast.info({
                                title: 'Rubro desactivado correctamente',
                                message: '',
                            });
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error',
                                message: '',
                            });
                        }
                        ConsultarRubrosEvaluacionCards();
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

function RegistrarActualizarRubroEvaluacion(id_rubro) {
    var nombre_rubro = $("#nombre_rubro").val();
    var porcentaje_rubro = parseFloat($("#porcentaje_rubro").val());
    var valid_porcentaje = 0;

    if ($(".conceptos_rubros").length == 0) {
        iziToast.warning({
            title: 'Ingresa los conceptos del rubro',
            message: '',
        });
    }
    else if (nombre_rubro == "" || nombre_rubro == undefined || porcentaje_rubro == "" || porcentaje_rubro == undefined) {
        iziToast.warning({
            title: 'Ingresa los datos del rubro',
            message: '',
        });
    }
    else if (porcentaje_rubro > 100 || porcentaje_rubro <= 0) {
        iziToast.warning({
            title: 'El porcentaje del rubro debe ser entre 1 - 100',
            message: '',
        });
    }
    else {
        var count = 0;
        var conceptos = [];
        var porcentajes = [];
        $(".conceptos_rubros").each(function () {
            var count_concepto = $(this).attr('id').split('_')[1];
            conceptos[count] = $("#nombre_concepto_" + count_concepto + "").text();
            porcentajes[count] = $("#porcentaje_concepto_" + count_concepto + "").text();

            valid_porcentaje = valid_porcentaje + parseFloat($("#porcentaje_concepto_" + count_concepto + "").text());
            count++;
        });

        if (valid_porcentaje != 100) {
            iziToast.warning({
                title: 'El porcentaje de los conceptos debe sumar 100',
                message: '',
            });
        }
        else {
            var rubro = {};
            rubro.nombre_rubro = nombre_rubro;
            rubro.porcentaje = porcentaje_rubro;
            rubro.id_evaluacion_rubro = id_rubro;

            iziToast.question({
                timeout: 20000,
                close: false,
                overlay: true,
                displayMode: 'once',
                id: 'question',
                zindex: 99999999,
                title: 'Atención',
                message: '¿Desea guardar el rubro?',
                position: 'center',
                buttons: [
                    ['<button><b>Si, guardar</b></button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                        $.ajax({
                            type: "POST",
                            async: true,
                            url: "../EVALUACIONES/RegistrarActualizarRubroEvaluacion",
                            data: {
                                rubro: rubro,
                                conceptos: conceptos,
                                porcentajes: porcentajes
                            },
                            success: function (response) {
                                if (response == 1) {
                                    iziToast.success({
                                        title: 'Rubro registrado correctamente',
                                        message: '',
                                    });
                                    $("#m_rubros_conceptos").modal("hide");
                                    ConsultarRubrosEvaluacionCards();
                                }
                                else if (response == 2) {
                                    iziToast.success({
                                        title: 'Rubro actualizado correctamente',
                                        message: '',
                                    });
                                    $("#m_rubros_conceptos").modal("hide");
                                    ConsultarRubrosEvaluacionCards();
                                }
                                else {
                                    iziToast.error({
                                        title: 'Ocurrió un error al guardar el rubro',
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
}

//#endregion


//#region ASOCIACION DE RUBROS ADEPARTAMENTO


function ConsultarDepartamentosRubrosCards() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarDepartamentosRubrosCards",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_rubros_departamentos_cards").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarRubrosDepartamentoCard(id_depa, nombre_depa) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarRubrosDepartamentoCard",
        data: { id_depa: id_depa },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_rubros_departamentos_table").html(response);
            $("#btn_add_edit_rubros_depa").attr("onclick", "GuardarRubrosDepartamento(" + id_depa + ");");
            $("#lbl_depa_title").text(nombre_depa);
            CalcularTotalRubrosDepartamentos();
            $("#m_rubros_depas").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function CalcularTotalRubrosDepartamentos() {
    var total = 0;
    $('.id_rubro_depa:checked').each(function () {
        total = total + parseInt($(this).prop("name"));
    });
    return $("#lbl_total_rubros_depa").text(total + "%");
}

function GuardarRubrosDepartamento(id_depa) {
    var count = 0;
    var id_rubros = [];
    $('.id_rubro_depa:checked').each(function () {
        id_rubros[count] = $(this).val();
        count++;
    });

    if (id_rubros.length <= 0) {
        iziToast.warning({
            title: 'Selecciona al menos 1 rubro para asignar al departamento',
            message: '',
        });
    }
    else {
        var valid_total = 0;
        $('.id_rubro_depa:checked').each(function () {
            valid_total = valid_total + parseFloat($(this).prop("name"));
        });
        if (valid_total != 100) {
            iziToast.warning({
                title: 'El porcentaje total de los rubros debe ser 100%',
                message: '',
            });
        }
        else {
            $.ajax({
                type: "POST",
                async: true,
                url: "../EVALUACIONES/GuardarRubrosDepartamento",
                data: {
                    id_departamento: id_depa,
                    id_rubros: id_rubros
                },
                success: function (response) {
                    if (response == 0) {
                        iziToast.success({
                            title: 'Rubros del departamento actualizados correctamente',
                            message: '',
                        });
                        ConsultarDepartamentosRubrosCards(id_depa);
                        $("#m_rubros_depas").modal("hide");
                    }
                    else {
                        iziToast.error({
                            title: 'Ocurrió un problema al actualizar los rubros del departamento',
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

}


//#endregion


//#region EVALUACIONES

function ConsultarEvaluacionesSelect(modo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarEvaluacionesSelect",
        data: { modo: modo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#id_evaluacion_g_consulta").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GenerarAperturaEvaluaciones() {
    var descripcion = $("#descripcion_new").val();
    var fecha_apertura = $("#fecha_apertura_new").val();
    var fecha_cierre = $("#fecha_cierre_new").val();
    if (descripcion == "") {
        iziToast.warning({
            title: 'Ingresa el concepto de la evaluación',
            message: '',
        });
    }
    else if (fecha_apertura == "" || fecha_apertura == undefined || fecha_cierre == "" || fecha_cierre == undefined) {
        iziToast.warning({
            title: 'Selecciona las fechas de apertura y cierre',
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
            zindex: 9999999,
            title: 'ATENCIÓN',
            message: '¿Está seguro de aperturar las evaluaciones?',
            position: 'center',
            buttons: [
                ['<button><b>Si, generar plantillas de evaluación</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../EVALUACIONES/GenerarAperturaEvaluaciones",
                        data: {
                            descripcion: descripcion,
                            fecha_apertura: fecha_apertura,
                            fecha_cierre: fecha_cierre
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == -1) {
                                iziToast.warning({
                                    title: 'Se detectaron evaluaciones abiertas',
                                    message: 'Cierre la evaluación para generar una nueva',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: 'No hay un año registrado',
                                    message: '',
                                });
                            }
                            else if (response == -3) {
                                iziToast.warning({
                                    title: 'No hay empleados asignados a los departamentos',
                                    message: '',
                                });
                            }
                            else if (response == -4) {
                                iziToast.warning({
                                    title: 'No hay configuración de rubros a departamentos',
                                    message: '',
                                });
                            }
                            else if (response == -5) {
                                iziToast.warning({
                                    title: 'La fecha de apertura no puede ser igual o mayor a la fecha de cierre',
                                    message: '',
                                });
                            }
                            else if (response > 0) {
                                iziToast.info({
                                    title: "Plantilla generada con: " + response + " errores",
                                    message: '',
                                });
                            }
                            else {
                                iziToast.success({
                                    title: 'Plantilla generada correctamente',
                                    message: '',
                                });
                                $("#m_generar_evaluacion").modal("hide");
                                $("#descripcion_new").val('');
                                ConsultarEvaluacionesSelect(1);
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                        }
                    });

                }, true],
                ['<button>No. regresar</button>', function (instance, toast) {
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

function ConsultarEvaluacionesPendientes() {
    var id_evaluacion_g = $("#id_evaluacion_g_consulta").val();
    var id_departamento = $("#id_departamento_consulta").val();
    if (id_evaluacion_g == undefined || id_evaluacion_g == "") {
        iziToast.warning({
            title: 'Selecciona una evaluación para evaluar',
            message: '',
        });
    }
    else if (id_departamento == undefined || id_departamento == "") {
        iziToast.warning({
            title: 'Selecciona un departamento para evaluar',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../EVALUACIONES/ConsultarEvaluacionesPendientes",
            data: {
                id_evaluacion_g: id_evaluacion_g,
                id_departamento: id_departamento
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datatable_evaluaciones").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_evaluaciones").html(response);
                $('#datatable_evaluaciones').DataTable({
                    keys: false,
                    select: true,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [],
                    responsive: false,
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
}

function ConsultarRubrosEvaluacionEmpleado(id_evaluacion_g, id_evaluacion_d_empleado) {
    $("#m_departamentos_pendientes").modal("hide");
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarRubrosEvaluacionEmpleado",
        data: {
            id_evaluacion_g: id_evaluacion_g,
            id_evaluacion_d_empleado: id_evaluacion_d_empleado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response != "") {
                $("#div_evaluacion_empleado").html(response);
                $("#m_evaluacion_empleado").modal("show");
            }
            else {
                iziToast.warning({
                    title: 'La evaluación ya fue finalizada por dirección',
                    message: '',
                });
                ConsultarEvaluacionesPendientes();
            }
            
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CalularTotalesRubros() {
    $(".rubros_id_headers").each(function () {
        var calificacion_total = 0;

        var id_rubro = $(this).attr('id').split('_')[1];
        $(".porcentajes_" + id_rubro + "").each(function () {
            var calificacion = parseFloat($(this).text().replace("%", ""));
            if (isNaN(calificacion)) {
                calificacion = 0;
            }
            calificacion_total += Math.round(parseFloat(calificacion));
        });

        $("#CalificacionFinal_" + id_rubro + "").text(calificacion_total + "%");
    });

    var totales_rubro = 0;
    var calificaciones_finales = $(".calificaciones_finales");
    calificaciones_finales.each(function () {
        var id_rubro = $(this).attr('id').split('_')[1];

        var porcentaje_maximo_rubro = parseFloat($(this).attr('name'));
        var calificacion_rubro = parseFloat($(this).text().replace('%', ''));
        if (isNaN(calificacion_rubro)) { calificacion_rubro = 0; }

        var equivalente = Math.round((porcentaje_maximo_rubro * calificacion_rubro) / 100);
        $("#PorcentajeEquivalente_" + id_rubro + "").text(equivalente + "%" + "/" + Math.round(porcentaje_maximo_rubro) + "%");
        totales_rubro = totales_rubro + equivalente;
    });
    totales_rubro = parseInt(Math.round(totales_rubro));
    $("#calificacion_final_empleado").text(" - " + totales_rubro + "%");
}

function ValidarCalificacionMaxima(id_concepto, valor_maximo, id_rubro) {
    var valor = $("#calificacion_" + id_concepto + "").val();
    var tieneDecimal = /^\d+\.\d+$/.test(valor);
    if (tieneDecimal) { $("#calificacion_" + id_concepto + "").val(Math.round(parseFloat(valor))); }

    if (valor <= 0 || valor == undefined) { $("#calificacion_" + id_concepto + "").val(''); }


    if (valor > 100) {
        valor = 100;
        $("#calificacion_" + id_concepto + "").val(valor);
    }

    var porcentaje_calificacion = (valor * valor_maximo) / 100;
    $("#porcentaje_calificacion_" + id_concepto + "").text(Math.round(porcentaje_calificacion) + "%");

    var calificacion_total = 0;
    $(".porcentajes_" + id_rubro + "").each(function () {
        var calificacion = $(this).text();
        if (calificacion == "") {
            calificacion = 0;
        }
        calificacion_total += Math.round(parseFloat(calificacion));
    });

    $("#CalificacionFinal_" + id_rubro + "").text(calificacion_total + "%");

    var totales_rubro = 0;
    var calificaciones_finales = $(".calificaciones_finales");
    calificaciones_finales.each(function () {
        var id_rubro = $(this).attr('id').split('_')[1];

        var porcentaje_maximo_rubro = parseFloat($(this).attr('name'));
        var calificacion_rubro = parseFloat($(this).text().replace('%', ''));
        if (isNaN(calificacion_rubro)) { calificacion_rubro = 0; }

        var equivalente = Math.round((porcentaje_maximo_rubro * calificacion_rubro) / 100);
        $("#PorcentajeEquivalente_" + id_rubro + "").text(equivalente + "%" + "/" + Math.round(porcentaje_maximo_rubro) + "%");
        totales_rubro = totales_rubro + equivalente;
    });
    totales_rubro = parseInt(Math.round(totales_rubro));
    $("#calificacion_final_empleado").text(" - " + totales_rubro + "%");
}

function GuardarEvaluacionEmpleado(id_evaluacion_d_empleado, id_evaluacion_g, id_status) {
    var valid_vacios = false;
    var valid_mayores_porcentaje = false;
    var calificaciones = [];
    var porcentajes = [];
    var comentarios = [];
    var id_conceptos = [];
    var count = 0;
    $(".calificaciones_inputs").each(function () {
        $(this).css("border-color", "");

        var id_concepto = $(this).attr("id").split('_')[1];
        id_conceptos[count] = id_concepto;

        //var porcentaje_mayor = parseFloat($("#porcentaje_maximo_" + id_concepto + "").text());
        var calificacion = $("#porcentaje_calificacion_" + id_concepto + "").text().replace('%', '');

        var valor = $(this).val();
        if (valor == "" || valor <= 0) {
            valid_vacios = true;
            $(this).css("border-color","red");
        }
        if (valor > 100 || valor <= 0) {
            valid_mayores_porcentaje = true;
            $(this).css("border-color", "red");

            var $input = $(this);
            var container = $("#div_scroll_evaluaciones");

            container.animate({
                scrollTop: $input.position().top - container.scrollTop()
            }, 100); // Adjust the duration as needed
        }
        calificaciones[count] = valor;
        porcentajes[count] = calificacion;
        comentarios[count] = $("#obs_" + id_concepto + "").val();
        count++;
    });

    if (valid_vacios == true) {
        iziToast.warning({
            title: 'Se detectaron campos vacíos o negativos',
            message: '',
        });
    }
    else if (valid_mayores_porcentaje == true) {
        iziToast.warning({
            title: 'Se detectaron valores mayores al porcentaje máximo',
            message: '',
        });
    }
    else {
        if (id_status == 2) {
            iziToast.info({
                timeout: 20000,
                overlay: true,
                displayMode: 'once',
                id: 'inputs',
                zindex: 9999999,
                title: 'Atención',
                message: '¿Está seguro de guardar la evaluación?',
                position: 'center',
                transitionIn: 'flipInX',
                transitionOut: 'flipOutX',
                drag: false,
                inputs: [
                    ['<input type="radio" name="radio_direccion" value="' + id_status +'"><b>Evaluar</b>', 'change', function (instance, toast, input, e) {
                    }],
                    ['<input type="radio" name="radio_direccion" value="3"><b>Finalizar evaluación</b>', 'change', function (instance, toast, input, e) {
                    }]
                ],
                buttons: [
                    ['<button><b>Evaluar</b></button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                        var valor_direccion = $('input[name="radio_direccion"]:checked').val();
                        if (valor_direccion == undefined || valor_direccion == "") {
                            iziToast.info({
                                title: 'Selecciona una opción para guardar la evaluación',
                                message: '',
                            });
                        }
                        else {
                            GuardarEvaluacion(id_evaluacion_d_empleado, id_evaluacion_g, valor_direccion, id_conceptos, calificaciones, comentarios, porcentajes);
                            FinalizarEvaluacionGeneral();
                        }

                    }, true],
                    ['<button>No, regresar</button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    }],
                ],
            });
        }
        else {
            iziToast.question({
                timeout: 20000,
                close: false,
                overlay: true,
                displayMode: 'once',
                id: 'question',
                zindex: 9999999,
                title: 'Advertencia',
                message: '¿Está seguro de guardar la evaluación?',
                position: 'center',
                buttons: [
                    ['<button><b>Si, guardar</b></button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                        GuardarEvaluacion(id_evaluacion_d_empleado, id_evaluacion_g, id_status, id_conceptos, calificaciones, comentarios, porcentajes);

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
}

function GuardarEvaluacion(id_evaluacion_d_empleado, id_evaluacion_g, id_status, id_conceptos, calificaciones, comentarios, porcentajes) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/GuardarEvaluacionEmpleado",
        data: {
            id_evaluacion_d_empleado: id_evaluacion_d_empleado,
            id_evaluacion_g: id_evaluacion_g,
            id_status: id_status,
            id_conceptos: id_conceptos,
            calificaciones: calificaciones,
            comentarios: comentarios,
            porcentajes: porcentajes
        },
        success: function (response) {
            if (response == 0) {
                var fortalezas = $("#txt_fortalezas").val();
                var areas_oportunidad = $("#txt_areasOportunidad").val();
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../EVALUACIONES/GuardarEvaluacionComentarios",
                    data: {
                        id_evaluacion_g: id_evaluacion_g,
                        id_evaluacion_d_empleado: id_evaluacion_d_empleado,
                        fortalezas: fortalezas,
                        areas_oportunidad: areas_oportunidad
                    },
                    success: function (response) {
                        if (response == 0) {
                            jsRemoveWindowLoad();
                            iziToast.success({
                                title: 'Evaluación guardada correctamente',
                                message: '',
                            });
                            $("#m_evaluacion_empleado").modal("hide");
                            ConsultarEvaluacionesPendientes();
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al guardar la evaluación',
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
            else {
                iziToast.error({
                    title: 'Ocurrió un error al guardar la evaluación',
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

function FinalizarEvaluacionGeneral() {
    var id_evaluacion = $("#id_evaluacion_g_consulta").val();
    if (id_evaluacion == undefined || id_evaluacion == "") {
        iziToast.warning({
            title: 'Seleccione una evaluación para finalizar',
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
            message: '¿Está seguro de finalizar la evaluación?',
            position: 'center',
            buttons: [
                ['<button><b>Si, fanalizar</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../EVALUACIONES/FinalizarEvaluacionGeneral",
                        data: { id_evaluacion: id_evaluacion },
                        success: function (response) {
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Evaluación finalizada correctamente',
                                    message: '',
                                });
                                ConsultarEvaluacionesSelect(1);
                            }
                            else if (response == 1) {
                                iziToast.error({
                                    title: 'Evaluación no encontrada',
                                    message: '',
                                });
                            }
                            else if (response == 2) {
                                iziToast.error({
                                    title: 'La evaluación se encuentra cerrada',
                                    message: '',
                                });
                            }
                            else if (response == 3) {
                                iziToast.info({
                                    title: 'Hay evaluaciones pendientes de cerrar',
                                    message: '',
                                });
                                ConsultarDepartamentosPendientesEvaluar(id_evaluacion);
                            }
                            else if (response == 4) {
                                iziToast.error({
                                    title: 'La evaluación está desactivada',
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

function ConsultarDepartamentosPendientesEvaluar(id_evaluacion) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarDepartamentosPendientesEvaluar",
        data: { id_evaluacion: id_evaluacion },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_departamentos_pendientes").html(response);
            $("#m_departamentos_pendientes").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#endregion


//#region HISTORIAL DE EVALUACIONES
function ConsultarEvaluacionesDepartamentoAcordeon() {
    var id_evaluacion_g = $("#id_evaluacion_g_consulta").val();
    var id_departamento = $("#id_departamento_consulta").val();
    if (id_evaluacion_g == undefined || id_evaluacion_g == "") {
        iziToast.warning({
            title: 'Selecciona una evaluación para evaluar',
            message: '',
        });
    }
    else if (id_departamento == undefined || id_departamento == "") {
        iziToast.warning({
            title: 'Selecciona un departamento para evaluar',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../EVALUACIONES/ConsultarEvaluacionesDepartamentoAcordeon",
            data: {
                id_evaluacion_g: id_evaluacion_g,
                id_departamento: id_departamento
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datatable_evaluaciones").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_evaluaciones").html(response);
                $('#datatable_evaluaciones').DataTable({
                    keys: false,
                    select: true,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [],
                    responsive: false,
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
}

function ConsultarEvaluacionesEmpleado(id_evaluacion_g, id_evaluacion_d_empleado) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarEvaluacionesEmpleado",
        data: {
            id_evaluacion_g: id_evaluacion_g,
            id_evaluacion_d_empleado: id_evaluacion_d_empleado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_evaluaciones_empleado").html(response);
            $("#m_evaluaciones_empleado").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#endregion


//#region ADMINISTRACIÓN DE BONOS
function ConsultarEmpleadosBonosTable() {
    var id_departamento = $("#id_departamento").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarEmpleadosBonosTable",
        data: { id_departamento: id_departamento },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_bonos_empleados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_empleados_bonos").html(response);
            $('#datatable_bonos_empleados').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                pageLength: 10,
                scrollY: true,
                scrollX: true
            });
            CaracteresControl();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ActualizarBonosEmpleados() {
    var id_empleados = [];
    var montos = [];
    var count = 0;
    var valid_vacios = false;

    var inputs = $(".bonos_empleados");
    inputs.css("border-color", "");

    inputs.each(function () {
        var valor = $(this).val();
        if (valor == "" || valor == 0 || valor == undefined) {
            $(this).css("border-color", "red");
            valid_vacios = true;
        }
        montos[count] = valor.replace(/,/g, '');
        id_empleados[count] = $(this).attr("id").split('_')[1];
        count++;
    });

    if (valid_vacios == true) {
        iziToast.warning({
            title: 'Se detectaron montos vacíos',
            message: '',
        });
    }
    else if (id_empleados.length <= 0) {
        iziToast.warning({
            title: 'No hay empleados para actualizar',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 90000,
            url: "../EVALUACIONES/ActualizarBonosEmpleados",
            data: {
                id_empleados: id_empleados,
                montos: montos
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Bonos actualizados correctamente',
                        message: '',
                    });
                    ConsultarEmpleadosBonosTable();
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al guardar los datos',
                        message: 'Intente nuevamente',
                    });
                    ValidaSesion();
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}


//#endregion



//#region PAGO DE BONOS
function ConsultarBonosEvaluacion() {
    var id_evaluacion = $("#id_evaluacion_g_consulta").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EVALUACIONES/ConsultarBonosEvaluacion",
        data: {
            id_evaluacion: id_evaluacion
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_bonos_evaluacion_empleados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_bonos_evaluacion").html(response);
            $('#datatable_bonos_evaluacion_empleados').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                pageLength: 10
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function GenerarExcelBonosEvaluacion() {
    var id_evaluacion = $("#id_evaluacion_g_consulta").val();
    window.open("http://192.168.128.2:90/EVALUACIONES/GenerarExcelBonosEvaluacion?id_evaluacion=" + id_evaluacion + "", "_blank");
    //window.open("https://localhost:44371/EVALUACIONES/GenerarExcelBonosEvaluacion?id_evaluacion=" + id_evaluacion + "", "_blank");
}

//#endregion

function GenerarExcelEvaluacionesDepartamento() {
    var id_evaluacion_g = $("#id_evaluacion_g_consulta").val();
    var id_departamento = $("#id_departamento_consulta").val();
    fetch('../REPORTES/EvaluacionesExcel', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            id_evaluacion_g: id_evaluacion_g,
            id_departamento: id_departamento
        })
    }).then(response => {
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
}

