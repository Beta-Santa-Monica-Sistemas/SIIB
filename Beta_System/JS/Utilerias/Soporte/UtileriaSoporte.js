function ModalNuevoSoporte() {
    $("#m_soporte").modal("show");
    ConsultarTipoSoporteSelect();

    $("#soporte_estatus_g").val(3);
    $("#soporte_prioridad").val(2);
    $("#soporte_estatus_d").val(3);

    setFechasSoporte();
}

function FechaParametrosSoporte() {
    const hoy = new Date();
    const haceUnaSemana = new Date();
    haceUnaSemana.setDate(hoy.getDate() - 2);

    const formato = d => d.toISOString().slice(0, 10);

    $('#fecha_soporte_buscar_1').val(formato(haceUnaSemana));
    $('#fecha_soporte_buscar_2').val(formato(hoy));
}


function setFechasSoporte() {
    const ahora = new Date();

    // Clonar fecha y restar 1 hora
    const haceUnaHora = new Date(ahora.getTime() - (60 * 60 * 1000));

    // Función para formatear
    function formatDateTimeLocal(fecha) {
        const year = fecha.getFullYear();
        const month = String(fecha.getMonth() + 1).padStart(2, '0');
        const day = String(fecha.getDate()).padStart(2, '0');
        const hours = String(fecha.getHours()).padStart(2, '0');
        const minutes = String(fecha.getMinutes()).padStart(2, '0');
        return `${year}-${month}-${day}T${hours}:${minutes}`;
    }

    $("#soporte_fecha_1").val(formatDateTimeLocal(haceUnaHora));
    $("#soporte_fecha_2").val(formatDateTimeLocal(ahora));
}


function ConsultarTipoSoporteSelect() {
    var categoria = $("#soporte_categoria").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CATALOGOS/ConsultarTiposSoporteSelect",
        data: { categoria: categoria },
        success: function (response) {
            $("#soporte_tipo").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function RegistrarSoporte() {
    var titulo = $("#soporte_titulo").val();
    var descripcion = $("#soporte_descripcion").val();
    var id_estado_g = $("#soporte_estatus_g").val();
    var id_prioridad = $("#soporte_prioridad").val();
    var id_usuario_solicita = $("#id_soporte_solicita").val();


    var id_categoria = $("#soporte_categoria").val();
    var id_tipo = $("#soporte_tipo").val();
    var solicitud = $("#soporte_solicitud").val();
    var id_estado_d = $("#soporte_estatus_d").val();
    var fecha_inicio = $("#soporte_fecha_1").val();
    var fecha_termino = $("#soporte_fecha_2").val();

    if (fecha_inicio == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de indicar la fecha de inicio',
        });
        return;
    }
    if (fecha_termino == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de indicar la fecha de termino',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/RegistrarSoporte",
        data: {
            titulo: titulo,
            descripcion: descripcion,
            id_estado_g: id_estado_g,
            id_prioridad: id_prioridad,
            id_usuario_solicita: id_usuario_solicita,

            id_categoria: id_categoria,
            id_tipo: id_tipo,
            solicitud: solicitud,
            id_estado_d: id_estado_d,
            fecha_inicio: fecha_inicio,
            fecha_termino: fecha_termino,
        },
        success: function (response) {
            if (response == true || response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Soporte registrado correctamente',
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrio un problema al regisrar el soporte',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarHistorialSoporteTable() {
    var estatus = $("#soporte_buscar_estatus").val();
    var prioridad = $("#soporte_buscar_prioridad").val();
    var fecha1 = $("#fecha_soporte_buscar_1").val();
    var fecha2 = $("#fecha_soporte_buscar_2").val();

    if (fecha1 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de indicar la fecha de inicio',
        });
        return;
    }
    if (fecha2 == "") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de indicar la fecha de termino',
        });
        return;
    }

    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarHistorialSoporteTable",
        data: {
            estatus: estatus,
            prioridad: prioridad,
            fecha1: fecha1,
            fecha2: fecha2
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_historial_soporte").html(response);
            $('#historial_soporte_table').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm",
                    bom: true,
                    charset: 'utf-8',
                    customize: function (csv) {
                        return "\uFEFF" + csv;
                    }
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
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}