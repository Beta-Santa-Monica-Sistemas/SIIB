
function ConsultarMovimientosClientesEnviosLeche() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    //var id_destino = $("#id_alimento_reporte").val();
    var facturado = $("#facturado_reporte").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarMovimientosClientesEnviosLeche",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_cliente: id_cliente,
            id_establo: id_establo,
            id_destino: 0,
            facturado: facturado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $(".datatable_reporte_movimientos_cliente").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_movimientos_cliente").html(response);
            $('.datatable_reporte_movimientos_cliente').DataTable({
                keys: false,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarMovimientosClientesEnviosLecheExcel() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_destino = 0;
    var facturado = $("#facturado_reporte").val();

    //var url = "https://siib.beta.com.mx//REPORTES/ConsultarMovimientosClientesEnviosLecheExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_cliente=" + id_cliente + "&&id_establo=" + id_establo + "&&id_destino=" + id_destino + "&&facturado=" + facturado + "";
    var url = "https://localhost:44371//REPORTES/ConsultarMovimientosClientesEnviosLecheExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_cliente=" + id_cliente + "&&id_establo=" + id_establo + "&&id_destino=" + id_destino + "&&facturado=" + facturado + "";
    window.open(url, '_blank');
}

function ConsultarMovimientosClientesEnviosLechePDF() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_destino = 0;
    var facturado = $("#facturado_reporte").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarMovimientosClientesEnviosLechePDF",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_cliente: id_cliente,
            id_establo: id_establo,
            id_destino: 0,
            facturado: facturado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            //$("#div_reporte_movimientos_proveedor").html(response);

            var newTab = window.open('', '_blank');
            newTab.document.open();
            newTab.document.write(response);
            newTab.document.close();
            newTab.onload = function () {
                newTab.print();
                newTab.close();
            };


            html2pdf()
                .from(response)
                .set({
                    margin: [15, 15, 15, 15],
                    filename: "Reporte de movimientos por cliente.pdf",
                    image: { type: 'png', quality: 2 },
                    html2canvas: { scale: 1.1 },
                    jsPDF: { unit: 'pt', format: [612, 936], orientation: 'landscape' },  // Oficio: 8.5 x 13 pulgadas
                    pagebreak: { mode: ['avoid-all', 'css', 'legacy'] }
                })
                .save()
                .then(function () {
                    jsRemoveWindowLoad();
                });

            //$("#div_reporte_movimientos_proveedor_pdf").css("display", "block");
            //$("#div_reporte_movimientos_proveedor_pdf").html(response);
            //var HTML_Width = $("#div_reporte_movimientos_proveedor_pdf").width();
            //var HTML_Height = $("#div_reporte_movimientos_proveedor_pdf").height();
            //var top_left_margin = 15;
            //var PDF_Width = HTML_Width + (top_left_margin * 2);
            //var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            //var canvas_image_width = HTML_Width;
            //var canvas_image_height = HTML_Height;

            //var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            //html2canvas($("#div_reporte_movimientos_proveedor_pdf")[0]).then(function (canvas) {
            //    var imgData = canvas.toDataURL("image/jpeg", 1.0);
            //    var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
            //    pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
            //    for (var i = 1; i <= totalPDFPages; i++) {
            //        pdf.addPage(PDF_Width, PDF_Height);
            //        pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
            //    }
            //    pdf.save("Reporte de movimientos por proveedor.pdf");
            //});
            //$("#div_reporte_movimientos_proveedor_pdf").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#region CALIDAD LECHE
function ConsultarCalidadEnviosLeche() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_destino = $("#id_destino_reporte").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarCalidadEnviosLeche",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_cliente: id_cliente,
            id_establo: id_establo,
            id_destino: id_destino
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $(".datatable_calidad_leche").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte").html(response);
            $('.datatable_calidad_leche').DataTable({
                keys: false,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: true,
                info: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DescargarCalidadEnviosLecheExcel() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../REPORTES/DescargarCalidadEnviosLecheExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_cliente: id_cliente,
                id_establo: id_establo,
                id_destino: 0
            })
        })
            .then(response => {
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
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}

function DescargarCalidadEnviosLechePDF() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_cliente = $("#id_cliente_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_destino = $("#id_destino_reporte").val();

    if (fecha_inicio && fecha_fin) {
        jsShowWindowLoad();

        fetch('../REPORTES/GenerarReporteCalidadEnviosLechePDF', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_cliente: id_cliente,
                id_establo: id_establo,
                id_destino: id_destino
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
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}

//#endregion

//#region CUMPLIMIENTO LECHE
function ConsultarCumplimientoEnviosLeche() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarCumplimientoEnviosLeche",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $(".datatable_cumplimiento_leche").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte").html(response);
            $('.datatable_cumplimiento_leche').DataTable({
                keys: false,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: true,
                info: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function DescargarCumplimientoEnviosLecheExcel() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../REPORTES/DescargarCumplimientoEnviosLecheExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
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
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}

function DescargarCumplimientoEnviosLechePDF() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();

    if (fecha_inicio && fecha_fin) {
        jsShowWindowLoad();

        fetch('../REPORTES/GenerarCumplimientoEnviosLechePDF', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
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
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}
//#endregion



function DescargarFormatoEnviosLechePDF(id_envio_leche) {
    fetch('../REPORTES/GenerarFormatoEnvioLechePDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            id_envio_leche: id_envio_leche
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

function DescargarCumplimientoEnviosLecheDetalladoPDF(fecha1, fecha2) {
    fetch('../REPORTES/GenerarCumplimientoEnviosLecheDetalladoPDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            anio: fecha1,
            nosemana: fecha2
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

function GenerarCumplimientoEnviosLecheCompletoPDF(fecha1, fecha2) {
    fetch('../REPORTES/GenerarCumplimientoEnviosLecheCompletoPDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            anio: fecha1,
            nosemana: fecha2
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
function GenerarCumplimientoEnviosLecheGeneralPDF(fecha1, fecha2, mes) {
    fetch('../REPORTES/GenerarCumplimientoEnviosLecheGeneralPDF', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            anio: fecha1,
            nosemana: fecha2,
            mes: mes
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


function DescargarDetalleProgramaDiaExcel(fecha) {

    var anio = $("#id_anio_programacion").val();
    var nosemana = $("#id_semana_programacion").val();

    fetch('../REPORTES/DescargarDetalleProgramaDiaExcel', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            anio: anio,
            nosemana: nosemana,
            fecha: fecha
        })
    })
        .then(response => {
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