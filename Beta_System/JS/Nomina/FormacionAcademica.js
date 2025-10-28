function ValidacionTituloCarrera(modo) {
    var tituloCarrera = $("#selectForma_titulo").val();
    var opciones = $("#selectForma_titulo").find("option");

    //REGISTRAR UN NUEVO TITULO O CARRERA
    if (tituloCarrera == -1 && modo == 1) {
        $(".regCarrera").css("display", "block");
        $(".regTitulo").css("display", "none");
    } else {
        //VALIDA QUE SOLO EXISTA REGISTRAR, SIENDO ASI, OBLIGA A REALIZAR 1 REGISTRO
        if (opciones.length === 1 && opciones.first().val() === "-1") {
            $(".regCarrera").css("display", "block");
            $(".regTitulo").css("display", "none");
            //SI EXISTEN MAS OPCIONES, SELECCIONA LA SIGUIENTE OPCION
        } else if (modo == 2) {
            let opcionSeleccionada = opciones.filter(function () {
                return $(this).val() !== "-1";
            }).first(); // Selecciona la primera opción después del -1

            if (opcionSeleccionada.length) {
                $("#selectForma_titulo").val(opcionSeleccionada.val());
                $(".regCarrera").css("display", "none");
                $(".regTitulo").css("display", "block");
            }
        }
    }
}

function ValidacionInstitucion(modo) {
    var intitucion = $("#selectForma_institucion").val();
    var opciones = $("#selectForma_institucion").find("option");

    //REGISTRAR UN NUEVO TITULO O CARRERA
    if (intitucion == -1 && modo == 1) {
        $(".regEducativa").css("display", "block");
        $(".regInstitucion").css("display", "none");
    } else {
        //VALIDAR SI EXISTEN MAS OPCIONES QUE 'REGISTRAR'
        if (opciones.length === 1 && opciones.first().val() === "-1") {
            $(".regEducativa").css("display", "block");
            $(".regInstitucion").css("display", "none");
            //SI EXISTEN MAS OPCIONES, SELECCIONA LA SIGUIENTE OPCION
        } else if (modo == 2) {
            let opcionSeleccionada = opciones.filter(function () {
                return $(this).val() !== "-1";
            }).first(); // Selecciona la primera opción después del -1

            if (opcionSeleccionada.length) {
                $("#selectForma_institucion").val(opcionSeleccionada.val());
                $(".regEducativa").css("display", "none");
                $(".regInstitucion").css("display", "block");
            }
        }
    }
}

function NuevoRegistroFormacionAcademica(modo) {
    //REGISTRAR
    if (modo == 1) {
        $("#div_registro_academico").css("display", "block");
        $("#btn_formacion_academica").attr("onclick", "NuevoRegistroFormacionAcademica(" + 2 + ")");
        $("#btn_formacion_academica").text("Cancelar registro");
        $("#btn_formacion_academica").removeClass("btn_beta").addClass("btn_beta_danger");
        LimpiarFormacionAcademica();
    }
    //CANCELAR
    else {
        $("#div_registro_academico").css("display", "none");
        $("#btn_formacion_academica").attr("onclick", "NuevoRegistroFormacionAcademica(" + 1 + ")");
        $("#btn_formacion_academica").text("Nuevo registro");
        $("#btn_formacion_academica").removeClass("btn_beta_danger").addClass("btn_beta");
    }
}


function ConfirmacionFormacionAcademica() {
    var formacion_empleado_g = $("#formacion_empleado_g").text();

    var numero_emp = $("#numero_emp").val();
    var tituloCarrera = $("#selectForma_titulo").val();
    var institucion = $("#selectForma_institucion").val();
    var statusCarrera = $("#selectForma_estatus").val();
    var año = $("#inputForma_anio").val();
    var nivelEscolaridad = $("#selectNivelEscolaridad").val();
    var numero_cedula = $("#numero_cedula").val();

    var fileData = new FormData();

    // Agregar archivos al FormData
    var fileUpload = $("#uploadForma_pdf").get(0);
    var files = fileUpload.files;
    for (var i = 0; i < files.length; i++) {
        fileData.append('file', files[i]);
    }

    // Si el título o la institución son "nuevos", tomar el valor del input de texto
    if (tituloCarrera == -1) {
        tituloCarrera = $("#inputForma_titulo").val();

        if ($("#inputForma_titulo").val().trim() == "") {
            iziToast.warning({ title: 'Aviso', message: 'Favor de ingresar un Titulo/Carrera' });
            return;
        }
    }
    if (institucion == -1) {
        institucion = $("#inputForma_institucion").val();
        if ($("#inputForma_institucion").val().trim() == "") {
            iziToast.warning({ title: 'Aviso', message: 'Favor de ingresar una Institucion' });
            return;
        }
    }

    // Agregar los demás datos al FormData
    fileData.append('id_formacion_g', formacion_empleado_g);

    fileData.append('numero_emp', numero_emp);
    fileData.append('tituloCarrera', tituloCarrera);
    fileData.append('institucion', institucion);
    fileData.append('statusCarrera', statusCarrera);
    fileData.append('año', año);
    fileData.append('nivelEscolaridad', nivelEscolaridad);
    fileData.append('numero_cedula', numero_cedula);


    iziToast.question({
        timeout: 20000,
        async: false,
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
                jsShowWindowLoad();
                $.ajax({
                    url: '../RECURSOSHUMANOS/ConfirmacionFormacionAcademica',
                    type: "POST",
                    contentType: false,
                    processData: false,
                    async: false,
                    data: fileData,
                    success: function (result) {
                        jsRemoveWindowLoad();
                        if (result == "-2") {
                            iziToast.warning({ title: 'EL ARCHIVO DEBE TENER FORMATO PDF', message: '' });
                        } else if (result == "-3") {
                            iziToast.error({ title: 'OCURRIÓ UN PROBLEMA AL GUARDAR EL ARCHIVO', message: '' });
                        } else if (result == "-4") {
                            iziToast.info({ title: 'NO SE DETECTARON ARCHIVOS', message: '' });
                        } else if (result == "-5") {
                            iziToast.warning({ title: 'OCURRIÓ UN ERROR AL REGISTRAR LA SOLICITUD', message: '' });
                        } else if (result == "0") {
                            iziToast.success({ title: 'SE REGISTRÓ CORRECTAMENTE LA INFORMACIÓN', message: '' });
                            ConsultarInstitucionSelect();
                            ConsultarCarreraSelect();
                            LimpiarFormacionAcademica();
                            HistoricoFormacionAcademicaTable();
                        }
                    },
                    error: function (err) {
                        jsRemoveWindowLoad();
                        iziToast.error({ title: 'Error en la solicitud', message: 'No se pudo enviar la información.' });
                    }
                });

            }, true],
            ['<button>NO</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
            }],
        ],
    });
    jsRemoveWindowLoad();
}

function HistoricoFormacionAcademicaTable() {
    jsShowWindowLoad();
    var numero = $("#numero_emp").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../RECURSOSHUMANOS/HistoricoFormacionAcademicaTable",
        data: {
            numero: numero
        },
        success: function (response) {
            try {
                $("#table_historico_formacion_academica").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_table_historico_formacion_academica").html(response);
            $('#table_historico_formacion_academica').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10,
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

function LimpiarFormacionAcademica() {
    ValidacionTituloCarrera(2);
    ValidacionInstitucion(2);
    $("#selectForma_estatus").prop('selectedIndex', 0);
    $("#inputForma_anio").val('');
    $("#uploadForma_pdf").val('');
    $("#numero_cedula").val('');
}

function ConsultarInstitucionSelect() {
    $.ajax({
        url: '../CATALOGOS/ConsultarInstitucionSelect',
        type: "POST",
        contentType: false,
        processData: false,
        async: false,
        success: function (result) {
            $("#selectForma_institucion").empty().append('<option value="-1">Nuevo registro</option>').append(result);
        },
        error: function (err) {
        }
    });
}

function ConsultarCarreraSelect() {
    $.ajax({
        url: '../CATALOGOS/ConsultarCarreraSelect',
        type: "POST",
        contentType: false,
        processData: false,
        async: false,
        success: function (result) {
            $("#selectForma_titulo").empty().append('<option value="-1">Nuevo registro</option>').append(result);
        },
        error: function (err) {
        }
    });
}

function RemoverHistoricoFormacionAcademica(id_formacion_d) {
    var formacion = id_formacion_d;
    iziToast.question({
        timeout: 20000,
        async: false,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'Aviso',
        message: '¿Estás seguro de remover la formacion academica?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                $.ajax({
                    url: '../RECURSOSHUMANOS/RemoverHistoricoFormacionAcademica',
                    type: "POST",
                    async: false,
                    data: {
                        id_formacion_d: id_formacion_d
                    },
                    success: function (result) {
                        if (result == 1) {
                            HistoricoFormacionAcademicaTable();
                            iziToast.success({ title: 'Exito', message: 'Se removio la información correctamente' });
                        }
                        else {
                            iziToast.warning({ title: 'Error en la solicitud', message: 'No se pudo remover la información.' });
                        }
                    },
                    error: function () {
                        jsRemoveWindowLoad();
                        iziToast.error({ title: 'Error en la solicitud', message: 'No se pudo enviar la información.' });
                    }
                });

            }, true],
            ['<button>NO</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsRemoveWindowLoad();
            }],
        ],
    });
    jsRemoveWindowLoad();
}

function ActualizarCedulaProfesional(input, numero_emp, id_formacion_d) {
    var file = input.files[0];

    if (!file) {
        iziToast.warning({ title: 'Aviso', message: 'Debes seleccionar un archivo PDF' });
        return;
    }

    if (!file.name.endsWith(".pdf")) {
        iziToast.warning({ title: 'Error', message: 'Solo se permiten archivos en formato PDF' });
        return;
    }

    var fileData = new FormData();
    fileData.append('numero_emp', numero_emp);
    fileData.append('id_formacion_d', id_formacion_d);
    fileData.append('archivo', file);

    iziToast.question({
        timeout: 20000,
        async: false,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'Aviso',
        message: '¿Estás seguro de actualizar la cedula profesional?',
        position: 'center',
        buttons: [
            ['<button><b>SI</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

                $.ajax({
                    url: '../RECURSOSHUMANOS/ActualizarCedulaProfesional',
                    type: "POST",
                    contentType: false,
                    processData: false,
                    async: false,
                    data: fileData,
                    success: function (result) {
                        if (result == "-2") {
                            iziToast.warning({ title: 'EL ARCHIVO DEBE TENER FORMATO PDF', message: '' });
                        } else if (result == "-3") {
                            iziToast.error({ title: 'OCURRIÓ UN PROBLEMA AL GUARDAR EL ARCHIVO', message: '' });
                        } else if (result == "-4") {
                            iziToast.info({ title: 'NO SE DETECTARON ARCHIVOS', message: '' });
                        } else if (result == "-5") {
                            iziToast.warning({ title: 'OCURRIÓ UN ERROR AL REGISTRAR LA SOLICITUD', message: '' });
                        } else if (result == "0") {
                            iziToast.success({ title: 'SE ACTUALIZO CORRECTAMENTE LA INFORMACIÓN', message: '' });
                            HistoricoFormacionAcademicaTable();
                        }
                    },
                    error: function () {
                        jsRemoveWindowLoad();
                        iziToast.error({ title: 'Error en la solicitud', message: 'No se pudo enviar la información.' });
                    }
                });

            }, true],
            ['<button>NO</button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsRemoveWindowLoad();
            }],
        ],
    });
    jsRemoveWindowLoad();
}