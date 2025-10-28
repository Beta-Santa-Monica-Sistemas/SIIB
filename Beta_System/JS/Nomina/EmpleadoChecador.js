function LupaChecadorEmpleados(modo) {
    $("#m_lupa_checador").modal('show');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/LupaChecadorEmpleados",
        data: {
            modo: modo
        },
        success: function (response) {

            try {
                $("#tabla_lupa_checador").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_lupa_checador_empleados").html(response);
            $('#tabla_lupa_checador').DataTable({
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
    jsRemoveWindowLoad();
}

function EmpleadosSinChecadorTable() {
    var area = $("#area_emp").val();
    if (area == "" || area == undefined) {
        area = $("#id_areas_prenomina_consulta").val();
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/EmpleadosSinChecadorTable",
        data: { modo: 1, area: area },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_Nomina").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_Tabla_Nomina").html(response);
            $("#tabla_Nomina").DataTable({
                keys: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });
        }
    });
}

function EmpleadoChecadorNominaTable() {
    var area = $("#area_emp_todos").val();
    if (area == "" || area == undefined) {
        area = $("#id_areas_prenomina_consulta").val();
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/EmpleadoChecadorNominaTable",
        data: {
            area: area
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_NChecador").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_EmpleadoNC").html(response);
            $("#tabla_NChecador").DataTable({
                keys: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 6
            });
        }
    });
}

function GuardarEmpleadoChecador() {
    var ValorChecador = $('input[name=id_RadiosChecador]:checked').val();
    var ValorNomina = $('input[name=radio_nomina]:checked').val();

    if (ValorChecador == "" || ValorChecador == undefined || ValorNomina == "" || ValorNomina == undefined) {

        iziToast.error({
            title: 'Error',
            message: 'Selecciona correctamente',
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
            title: 'Advertencia',
            message: '¿Estas seguro de guardar este empleado?',
            position: 'center',
            buttons: [
                ['<button><b>Sí, guardar.</b></button>', function (instance, toast) {

                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');


                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../NOMINA/GuardarEmpleado",
                        data: {
                            id_RadiosChecador: ValorChecador,
                            radio_nomina: ValorNomina,
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response = true) {
                                $("#tr_Checador_" + ValorChecador + "").remove();
                                $("#tr_Nomina_" + ValorNomina + "").remove();


                                iziToast.success({
                                    title: 'Correcto',
                                    message: 'Empleado guardado con éxito',
                                });

                                LupaChecadorEmpleados(2);
                                EmpleadosSinChecadorTable();
                                EmpleadoChecadorNominaTable();

                            } else {

                                iziToast.error({
                                    title: 'Error',
                                    message: 'Empleado no guardado',
                                });
                            }
                        }
                    });
                }, true],
                ['<button>No guardar.</button>', function (instance, toast) {

                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    iziToast.warning({
                        title: 'Atención',
                        message: 'Empleado no guardado',
                    });

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

function EliminarEmpleadoLigado(idEmpleadoNChecador) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'Atención',
        message: '¿Estas seguro de querer eliminar este empleado?',
        position: 'center',
        buttons: [
            ['<button><b>Sí, eliminar</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                jsShowWindowLoad();

                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../NOMINA/EliminarEmpleadoLigado",
                    data: { idEmpleadoNChecador: idEmpleadoNChecador },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        if (response = true) {
                            iziToast.success({
                                title: 'Correcto',
                                message: 'Empleado eliminado',
                            });

                            LupaChecadorEmpleados(2);
                            EmpleadosSinChecadorTable();
                            EmpleadoChecadorNominaTable();
                        } else {

                            iziToast.error({
                                title: 'Error',
                                message: 'El empleado no se eliminó',
                            });
                        }
                    }
                });

            }, true],
            ['<button>No eliminar</button>', function (instance, toast) {

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

function ObtenerNumeroSemanas() {
    var year = $("#id_anio_prenomina_consulta option:selected").text();
    var month = $("#id_meses").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerSemanas",
        data: {
            month: month,
            year: year
        },
        dataType: "json",
        success: function (data) {
            console.log("Respuesta del servidor:", data); // Depuración: Verifica la respuesta del servidor
            jsRemoveWindowLoad();
            $("#id_semana_mensual").empty();
            if (data.SemanasNumero) {
                data.SemanasNumero.forEach(function (semana) {
                    $("#id_semana_mensual").append(new Option("Semana " + semana, semana));
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
}
function ObtenerFechaSemanas() {
    var semana = $("#id_semana_mensual option:selected").val();
    var year = $("#id_anio_prenomina_consulta option:selected").text();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../NOMINA/ObtenerFechaSemanasLunesDomingo",
        data: {
            semana: semana,
            year: year
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#semana_dia_inicial").text(response);
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", error);
            console.error("Detalles del error:", xhr.responseText);
            jsRemoveWindowLoad();
        }
    });
}





function ReporteJornadaLaboral() {
    var fecha_semana = $("#semana_dia_inicial").text().split('-')[0].trim();
    var id_departamento = $("#id_areas_prenomina_consulta").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../NOMINA/ReporteJornadaLaboral",
        data: {
            fecha_semana: fecha_semana,
            id_departamento: id_departamento
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_jornada_laboral").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_jornada_laboral").html(response);
            $('#datatable_jornada_laboral').DataTable({
                keys: false,
                select: true,
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

function AccionChecador(accion, id_checador, empleado, dia, entrada, salida) {
    $("#m_accion_checador").modal("show");
    $("#fecha_checador").text(dia);

    //ACTUALIZAR ENTRADA
    if (accion == 1) {
        var partes = salida.split(" ");
        var horaMinutosSegundos = partes[1].split(":");
        var horas = parseInt(horaMinutosSegundos[0]);
        var minutos = horaMinutosSegundos[1];
        var usoHorario = partes[2].toUpperCase();
        $("#salida_hora_oculto").text(horas);
        $("#salida_minuto_oculto").text(minutos);
        $("#salida_zona_oculto").text(usoHorario.includes('A.') ? 'AM' : 'PM');


        var partes2 = entrada.split(" ");
        var horaMinutosSegundos2 = partes2[1].split(":");
        var horas2 = parseInt(horaMinutosSegundos2[0]);
        var minutos2 = horaMinutosSegundos2[1];
        var usoHorario2 = partes2[2].toUpperCase();
        $("#hora").val(horas2);
        $("#minutos").val(minutos2);
        $("#horario").val(usoHorario2.includes('A.') ? 'AM' : 'PM');
        $("#accion_checador").attr("onclick", "ActualizarHorasChecadorEmpleado(" + empleado + "," + accion + "," + id_checador + ")");
    }

    //ACTUALIZAR SALIDA
    if (accion == 2) {
        var partes = entrada.split(" ");
        var horaMinutosSegundos = partes[1].split(":");
        var horas = parseInt(horaMinutosSegundos[0]);
        var minutos = horaMinutosSegundos[1];
        var usoHorario = partes[2].toUpperCase();
        $("#entrada_hora_oculto").text(horas);
        $("#entrada_minuto_oculto").text(minutos);
        $("#entrada_zona_oculto").text(usoHorario.includes('A.') ? 'AM' : 'PM');

        var partes2 = salida.split(" ");
        var horaMinutosSegundos2 = partes2[1].split(":");
        var horas2 = parseInt(horaMinutosSegundos2[0]);
        var minutos2 = horaMinutosSegundos2[1];
        var usoHorario2 = partes2[2].toUpperCase();
        $("#hora").val(horas2);
        $("#minutos").val(minutos2);
        $("#horario").val(usoHorario2.includes('A.') ? 'AM' : 'PM');
        $("#accion_checador").attr("onclick", "ActualizarHorasChecadorEmpleado(" + empleado + "," + accion + "," + id_checador + ")");
    }

    //REGISTRAR ENTRADA
    if (accion == 3) {
        var partes = salida.split(" ");
        var horaMinutosSegundos = partes[1].split(":");
        var horas = parseInt(horaMinutosSegundos[0]);
        var minutos = horaMinutosSegundos[1];
        var usoHorario = partes[2].toUpperCase();
        $("#salida_hora_oculto").text(horas);
        $("#salida_minuto_oculto").text(minutos);
        $("#salida_zona_oculto").text(usoHorario.includes('A.') ? 'AM' : 'PM');

        $("#hora").val(1);
        $("#minutos").val('00');
        $("#horario").val('AM');
        $("#accion_checador").attr("onclick", "RegistrarHorasChecadorEmpleado(" + empleado + "," + accion + ")");
    }

    //REGISTRAR SALIDA
    if (accion == 4) {
        var partes = entrada.split(" ");
        var horaMinutosSegundos = partes[1].split(":");
        var horas = parseInt(horaMinutosSegundos[0]);
        var minutos = horaMinutosSegundos[1];
        var usoHorario = partes[2].toUpperCase();
        $("#entrada_hora_oculto").text(horas);
        $("#entrada_minuto_oculto").text(minutos);
        $("#entrada_zona_oculto").text(usoHorario.includes('A.') ? 'AM' : 'PM');

        $("#hora").val(1);
        $("#minutos").val('00');
        $("#horario").val('AM');
        $("#accion_checador").attr("onclick", "RegistrarHorasChecadorEmpleado(" + empleado + "," + accion + ")");
    }

}

function RegistrarHorasChecadorEmpleado(empleado, modo) {
    var fecha = $("#fecha_checador").text();
    var horas = $("#hora").val();
    var minutos = $("#minutos").val();
    var usohorario = $("#horario").val();
    var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;
    if (ValidarTiempos(tiempo, modo, usohorario) == true) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/RegistrarDiaChecador",
            data: {
                empleado: empleado,
                tiempo: tiempo,
                modo: modo
            },
            success: function (response) {
                if (response == true || response == "True") {
                    jsRemoveWindowLoad();
                    ReporteJornadaLaboral();
                    $("#m_accion_checador").modal('hide');
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Se realizo con extio el proceso!',
                    });
                }
                else {
                    jsRemoveWindowLoad();
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'No se realizo el proceso, favor de intentarlo nuevamente',
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

function ActualizarHorasChecadorEmpleado(empleado, modo, idpunches) {
    //empleado, dia, hora
    var fecha = $("#fecha_checador").text();
    var horas = $("#hora").val();
    var minutos = $("#minutos").val();
    var usohorario = $("#horario").val();
    var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;
    if (ValidarTiempos(tiempo, modo, usohorario) == true) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../NOMINA/ActualizarDiaChecador",
            data: {
                empleado: empleado,
                tiempo: tiempo,
                modo: modo,
                id_punches: idpunches
            },
            success: function (response) {
                if (response == true || response == "True") {
                    jsRemoveWindowLoad();
                    ReporteJornadaLaboral();
                    $("#m_accion_checador").modal('hide');
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Se realizo con extio el proceso!',
                    });
                }
                else {
                    jsRemoveWindowLoad();
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'No se realizo el proceso, favor de intentarlo nuevamente',
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


function FormatoStringDate(fechaHora) {
    var partes = fechaHora.split(" ");
    const [fecha, horaAMPM] = fechaHora.split(' ');
    const [dia, mes, anio] = fecha.split('/');
    const [hora, minutoAMPM] = horaAMPM.split(':');
    const minuto = minutoAMPM.slice(0, 2);
    const ampm = partes[2].toUpperCase();;
    let hora24 = parseInt(hora);
    if (ampm === 'PM' && hora24 !== 12) {
        hora24 += 12;
    } else if (ampm === 'AM' && hora24 === 12) {
        hora24 = 0;
    }
    return new Date(anio, mes - 1, dia, hora24, minuto);
}
function ValidarTiempos(tiempo_real, modo, horario) {
    //ACTUALIZAR ENTRADA
    if (modo == 1) {
        var fecha = $("#fecha_checador").text();
        var horas = $("#salida_hora_oculto").text();
        var minutos = $("#salida_minuto_oculto").text();
        var usohorario = $("#salida_zona_oculto").text();
        var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;

        const fechaTiempoReal = FormatoStringDate(tiempo_real);
        const fechaTiempo = FormatoStringDate(tiempo);
        if (fechaTiempoReal.getTime() == fechaTiempo.getTime() || fechaTiempo.getTime() < fechaTiempoReal.getTime()) {

            iziToast.warning({
                title: 'Aviso',
                message: 'La hora de entrada debe ser menor a la hora de salida',
            });
            return false;
        }
        else {
            return true;
        }
    }
    //ACTUALIZAR SALIDA
    if (modo == 2) {
        var fecha = $("#fecha_checador").text();
        var horas = $("#entrada_hora_oculto").text();
        var minutos = $("#entrada_minuto_oculto").text();
        var usohorario = $("#entrada_zona_oculto").text();
        var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;

        const fechaTiempoReal = FormatoStringDate(tiempo_real);
        const fechaTiempo = FormatoStringDate(tiempo);
        if (fechaTiempoReal.getTime() == fechaTiempo.getTime() || fechaTiempoReal.getTime() > fechaTiempo.getTime()) {
            iziToast.warning({
                title: 'Aviso',
                message: 'La hora de salida debe ser mayor a la hora de entrada',
            });
            return false;
        }
        else {
            return true;
        }
    }
    //REGISTRAR ENTRADA
    if (modo == 3) {
        var fecha = $("#fecha_checador").text();
        var horas = $("#salida_hora_oculto").text();
        var minutos = $("#salida_minuto_oculto").text();
        var usohorario = $("#salida_zona_oculto").text();
        var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;
        const fechaTiempoReal = FormatoStringDate(tiempo_real);
        const fechaTiempo = FormatoStringDate(tiempo);
        if (fechaTiempoReal.getTime() == fechaTiempo.getTime() || fechaTiempoReal.getTime() > fechaTiempo.getTime()) {
            iziToast.warning({
                title: 'Aviso',
                message: 'La hora de entrada debe ser menor a la hora de salida',
            });
            return false;
        }
        else {
            return true;
        }
    }
    //REGISTRAR SALIDA
    if (modo == 4) {
        var fecha = $("#fecha_checador").text();
        var horas = $("#entrada_hora_oculto").text();
        var minutos = $("#entrada_minuto_oculto").text();
        var usohorario = $("#entrada_zona_oculto").text();
        var tiempo = fecha + " " + horas + ":" + minutos + " " + usohorario;
        const fechaTiempoReal = FormatoStringDate(tiempo_real);
        const fechaTiempo = FormatoStringDate(tiempo);
        if (fechaTiempoReal.getTime() == fechaTiempo.getTime() || fechaTiempoReal.getTime() < fechaTiempo.getTime()) {
            iziToast.warning({
                title: 'Aviso',
                message: 'La hora de salida debe ser mayor a la hora de entrada',
            });
            return false;
        }
        else {
            return true;
        }
    }
}



//#region REPORTE CHECADOR MENSUAL
function ConsultarReporteChecadorMensual() {
    var mes_inicio = $("#mes_rep_mensual").val();
    var id_departamento = $("#id_area_rep_mensual").val();
    if (id_departamento == "" || id_departamento == undefined) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Ingrese una área válida',
        });
        return;
    }
    if (mes_inicio == undefined || mes_inicio == '') {
        iziToast.warning({
            title: 'Aviso',
            message: 'Ingrese un mes válido',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../NOMINA/ConsultarReporteChecadorMensual",
        data: {
            mes_inicio: mes_inicio,
            id_departamento: id_departamento
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_reporte_mensual").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_mensual").html(response);
            $('#datatable_reporte_mensual').DataTable({
                keys: false,
                select: true,
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





