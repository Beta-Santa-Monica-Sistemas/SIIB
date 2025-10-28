function ConsultarMovimientosProveedorAlimentacion() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_proveedor = $("#id_proveedor_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_alimento = $("#id_alimento_reporte").val();
    var facturado = $("#facturado_reporte").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarMovimientosProveedorAlimentacion",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_proveedor: id_proveedor,
            id_establo: id_establo,
            id_alimento: id_alimento,
            facturado: facturado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_reporte_moviemintos_proveedor").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_reporte_movimientos_proveedor").html(response);
            $('#datatable_reporte_moviemintos_proveedor').DataTable({
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

function ConsultarMovimientosProveedorAlimentacionExcel() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_proveedor = $("#id_proveedor_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_alimento = $("#id_alimento_reporte").val();
    var facturado = $("#facturado_reporte").val();

    //var url = "https://siib.beta.com.mx//REPORTES/ConsultarMovimientosProveedorAlimentacionExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_proveedor=" + id_proveedor + "&&id_establo=" + id_establo + "&&id_alimento=" + id_alimento + "&&facturado=" + facturado + "";
    var url = "https://localhost:44371//REPORTES/ConsultarMovimientosProveedorAlimentacionExcel?fecha_inicio=" + fecha_inicio + "&&fecha_fin=" + fecha_fin + "&&id_proveedor=" + id_proveedor + "&&id_establo=" + id_establo + "&&id_alimento=" + id_alimento + "&&facturado=" + facturado + "";
    window.open(url, '_blank');
}

function ConsultarMovimientosProveedorAlimentacionPDF() {
    var fecha_inicio = $("#fecha_inicio_reporte").val();
    var fecha_fin = $("#fecha_fin_reporte").val();
    var id_proveedor = $("#id_proveedor_reporte").val();
    var id_establo = $("#id_establo_reporte").val();
    var id_alimento = $("#id_alimento_reporte").val();
    var facturado = $("#facturado_reporte").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../REPORTES/ConsultarMovimientosProveedorAlimentacionPDF",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_proveedor: id_proveedor,
            id_establo: id_establo,
            id_alimento: id_alimento,
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
                    filename: "Reporte de movimientos por proveedor.pdf",
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


