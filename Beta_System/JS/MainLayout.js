function CerrarSesion() {
    $.ajax({
        url: "../USUARIOLOGIN/CerrarSesion",
        async: true,
        type: "POST",
        success: function (result) {
            window.location.href = "../";
        }

    });
}

function jsRemoveWindowLoad() {
    // eliminamos el div que bloquea pantalla
    $("#WindowLoad").remove();

}

function jsShowWindowLoad(mensaje) {
    //eliminamos si existe un div ya bloqueando
    jsRemoveWindowLoad();

    //si no enviamos mensaje se pondra este por defecto
    if (mensaje === undefined) mensaje = "Un momento porfavor...";

    //centrar imagen gif
    height = 20;//El div del titulo, para que se vea mas arriba (H)
    var ancho = 0;
    var alto = 0;

    //obtenemos el ancho y alto de la ventana de nuestro navegador, compatible con todos los navegadores
    if (window.innerWidth == undefined) ancho = window.screen.width;
    else ancho = window.innerWidth;
    if (window.innerHeight == undefined) alto = window.screen.height;
    else alto = window.innerHeight;

    //operación necesaria para centrar el div que muestra el mensaje
    var heightdivsito = ((alto / 2) - parseInt(height)) / 2;//Se utiliza en el margen superior, para centrar

    //imagen que aparece mientras nuestro div es mostrado y da apariencia de cargando
    //imgCentro = "<div style='text-align:center;height:" + alto + "px;'><div  style='color:#000;margin-top:" + heightdivsito + "px; font-size:20px;font-weight:bold'>" + mensaje + "</div><img  src='/Content/production/images/gifLoading4.gif'></div>";
    imgCentro = "<div style='text-align:center;height:" + alto + "px;'><div  style='color:white;margin-top:" + heightdivsito + "px; font-size:24px;font-weight:bold'>" + mensaje + "</div><img  src='/Content/img_layout/cowgif.gif'></div>";

    //creamos el div que bloquea grande------------------------------------------
    div = document.createElement("div");
    div.id = "WindowLoad"
    div.style.width = ancho + "px";
    div.style.height = alto + "px";
    div.style.background = "rgba(0, 0, 0, 0.8)";
    div.style.opacity = 0.8
    $("body").append(div);

    //creamos un input text para que el foco se plasme en este y el usuario no pueda escribir en nada de atras
    input = document.createElement("input");
    input.id = "focusInput";
    input.type = "text"

    //asignamos el div que bloquea
    $("#WindowLoad").append(input);

    //asignamos el foco y ocultamos el input text
    $("#focusInput").focus();
    $("#focusInput").hide();

    //centramos el div del texto
    $("#WindowLoad").html(imgCentro);
    return true;
}


function ConsultarAlmacenesUsuarioSelect(id_usuario) {
    $.ajax({
        url: "../CATALOGOS/ConsultarAlmacenesUsuarioSelect",
        async: false,
        type: "POST",
        data: { id_usuario: id_usuario },
        success: function (result) {
            $(".id_almacen_select").html(result);
        }
    });
}


function ConsultarEstablosUsuarioSelect(id_usuario) {
    $.ajax({
        url: "../CATALOGOS/ConsultarEstablosUsuarioSelect",
        async: false,
        type: "POST",
        data: { id_usuario: id_usuario },
        success: function (result) {
            $(".id_establo_select").html(result);
        }
    });
}

function ConsultarEstabloLecheUsuarioSelect() {
    $.ajax({
        url: "../CATALOGOS/ConsultarEstablosUsuarioLecheSelect",
        async: false,
        type: "POST",
        data: {},
        success: function (result) {
            $(".id_establo_leche_select").html(result);
        }
    });
}

function ConsultarEstablosLecheUsuarioSelect() {
    $.ajax({
        url: "../CATALOGOS/ConsultarEstablosUsuarioLecheSelect",
        async: false,
        type: "POST",
        data: {},
        success: function (result) {
            const opciones = '<option value="0">TODOS</option>' + result;
            $(".id_establo_leche_select").html(opciones);
        }
    });
}





function ConsultarDetalleRequiGeneral(id_requi_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarDetalleRequiGeneral",
        data: {
            id_requi_g: id_requi_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_detalle_requi_revision").modal("show");

            try {
                $("#datatable_detalle_requisicion_general").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_requis_revision_detalle").html(response);
            $('#datatable_detalle_requisicion_general').DataTable({
                keys: false,
                ordering: true,
                dom: "Bfrtip",
                select: true,
                buttons: [{}],
                paging: false,
                responsive: false,
                select: true
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsularTrackingLogRequisicion(id_requi_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsularTrackingLogRequisicion",
        data: { id_requi_g: id_requi_g },
        success: function (response) {
            $("#div_actividad_logs_requi").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarTrackingOrdenes(id_requi_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../REQUISICIONES/ConsultarTrackingOrdenes",
        data: { id_requi_g: id_requi_g },
        success: function (response) {
            $("#m_tracking_ordenes").modal("show");
            $("#div_tracking_ordenes").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


function MostrarOrdenCompraPDFProveedor(id_orden) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ORDENES_COMPRA/MostrarOrdenCompraPDFProveedor",
        data: {
            id_orden: id_orden
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "0") {
                iziToast.error({
                    title: 'Error',
                    message: 'No se encontró la orden de compra.',
                });
            }
            else {
                window.open(response, '_blank');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function MostrarOrdenCompraXMLProveedor(id_orden) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ORDENES_COMPRA/MostrarOrdenCompraXMLProveedor",
        data: {
            id_orden: id_orden
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "0") {
                iziToast.error({
                    title: 'Error',
                    message: 'No se encontró la orden de compra.',
                });
            }
            else {
                window.open(response, '_blank');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DescargarOrdenCompraProveedor(id_orden_g, token_orden) {
    jsShowWindowLoad();

    fetch('../PORTAL/GenerarOrdenCompraPDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            id_orden_g: id_orden_g,
            token_orden: token_orden
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