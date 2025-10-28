function ConsultarReporteConceptosNomina() {
    var id_prenomina = $("#id_prenomina_finalizadas").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../REPORTES/ConsultarReporteConceptosNomina",
        data: { id_prenomina: id_prenomina },
        success: function (response) {
            $("#div_reporte_conceptos_nomina").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}