function GenerarOrdenCompraPDF(id_orden_g, token_orden) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ORDENES_COMPRA/GenerarOrdenCompraPDF",
        data: {
            id_orden_g: id_orden_g,
            token_orden: token_orden
        },
        success: function (response) {
            $("#div_pdf_orden_compra").css("display", "block");
            $("#div_pdf_orden_compra").html(response);
            var HTML_Width = $("#div_pdf_orden_compra").width();
            var HTML_Height = $("#div_pdf_orden_compra").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#div_pdf_orden_compra")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/jpeg", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Orden de compra: " + id_orden_g + ".pdf");
            });
            $("#div_pdf_orden_compra").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}







