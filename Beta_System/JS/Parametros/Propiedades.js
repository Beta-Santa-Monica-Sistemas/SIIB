//VALIDAR CARACTERES (INPUTS)
function CaracteresControl() {
    //SOLO LETRAS A-Z ESPACIO Y NUMERO 0-9 = SIN PUNTO DECIMAL
    function SoloLetrasNumerosEspacio(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const formattedValue = inputValue.replace(/[^0-9A-Za-z ]/g, '');
            $(this).val(formattedValue);
        });
    }

    $(".solo-letras-numeros-espacio").each(function () {
        SoloLetrasNumerosEspacio($(this));
    });
    //SOLO LETRAS A-Z Y NUMEROS 0-9 SIN ESPACIO Y SIN DECIMAL
    function SoloLetrasNumerosSinEspacioDecimal(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const formattedValue = inputValue.replace(/[^0-9A-Za-z]/g, '');
            $(this).val(formattedValue);
        });
    }

    $(".solo-letras-numeros-s-espacio-decimal").each(function () {
        SoloLetrasNumerosSinEspacioDecimal($(this));
    });

    //SOLO LETRAS A-Z Y NUMEROS 0-9
    function SoloLetrasNumeros(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const formattedValue = inputValue.replace(/[^0-9A-Za-z .]/g, '');
            const dotIndex = formattedValue.indexOf('.');
            const integerPart = dotIndex !== -1 ? formattedValue.slice(0, dotIndex).replace(/[^0-9A-Za-z ]/g, '') : formattedValue;
            const decimalPart = dotIndex !== -1 ? '.' + formattedValue.slice(dotIndex + 1).replace(/[^0-9A-Za-z ]/g, '') : '';
            const resultValue = decimalPart.length > 0 ? integerPart + decimalPart : integerPart;
            $(this).val(resultValue);
        });
    }
    $(".solo-letras-numeros").each(function () {
        SoloLetrasNumeros($(this));
    });
    //SOLO LETRAS A-Z
    function SoloLetras(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const formattedValue = inputValue.replace(/[^A-Za-z ]/g, '');

            $(this).val(formattedValue);
        });
    }
    $(".solo-letras").each(function () {
        SoloLetras($(this));
    });
    //SOLO NUMEROS 0-9
    function SoloNumeros(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const numericDotValue = inputValue.replace(/[^0-9.]/g, '');
            const parts = numericDotValue.split('.');
            const integerPart = parts[0].replace(/[^0-9]/g, '');
            const decimalPart = parts.length > 1 ? '.' + parts[1].replace(/[^0-9]/g, '') : '';
            var formattedValue = decimalPart.length > 0 ? integerPart + decimalPart : integerPart;

            if (formattedValue.length > 12) {
                formattedValue = formattedValue.slice(0, -1);
            }
            $(this).val(formattedValue);
        });
    }
    $(".solo-numeros").each(function () {
        SoloNumeros($(this));
    });
    //SOLO NUMEROS 0-9 SIN .
    function SoloNumerosSinPunto(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const numericValue = inputValue.replace(/[^0-9]/g, '');
            $(this).val(numericValue);
        });
    }
    $(".solo-numeros-spunto").each(function () {
        SoloNumerosSinPunto($(this));
    });

    function SoloNumerosTabla(element) {
        element.on("input", function () {
            const inputValue = $(this).val();
            const numericDotValue = inputValue.replace(/[^0-9.]/g, '');
            const parts = numericDotValue.split('.');
            const integerPart = parts[0].replace(/[^0-9]/g, '');
            const decimalPart = parts.length > 1 ? '.' + parts[1].replace(/[^0-9]/g, '') : '';
            const formattedValue = decimalPart.length > 0 ? integerPart + decimalPart : integerPart;
            $(this).val(formattedValue);
        });
    }

    $(".solo-numeros-tabla").each(function () {
        SoloNumerosTabla($(this));
    });

}
//VALIDAR CARACTERES DATETABLE (INPUTS)
function TablaDecimal(element) {
    const inputValue = $(element).text();
    const cursorPosition = getCaretPosition(element);
    // Reemplazar todos los caracteres excepto dígitos y punto
    const numericDotValue = inputValue.replace(/[^0-9.]/g, '');
    // Limitar a solo un punto decimal
    const parts = numericDotValue.split('.');
    const integerPart = parts[0].replace(/[^0-9]/g, '');
    let decimalPart = '';
    if (parts.length > 1) {
        // Si hay más de un punto decimal, ignorar los puntos adicionales
        decimalPart = '.' + parts[1].replace(/[^0-9]/g, '');
    }
    const formattedValue = decimalPart.length > 0 ? integerPart + decimalPart : integerPart;
    $(element).text(formattedValue);
    // Ajustar la posición del cursor solo si no se ha eliminado todo el contenido
    const newCursorPosition = cursorPosition > formattedValue.length ? formattedValue.length : cursorPosition;
    setCaretPosition(element, newCursorPosition);
}

function TablaEntero(element) {
    const inputValue = $(element).text();
    const cursorPosition = getCaretPosition(element);  // Guarda la posición del cursor
    const numericDotValue = inputValue.replace(/[^0-9]/g, ''); // Solo permite números
    $(element).text(numericDotValue);
    setCaretPosition(element, cursorPosition);  // Restaura la posición del cursor
}

// Función para obtener la posición actual del cursor
function getCaretPosition(element) {
    const range = window.getSelection().getRangeAt(0);
    const preCaretRange = range.cloneRange();
    preCaretRange.selectNodeContents(element);
    preCaretRange.setEnd(range.endContainer, range.endOffset);
    return preCaretRange.toString().length;
}

// Función para restaurar la posición del cursor
function setCaretPosition(element, position) {
    const range = document.createRange();
    const sel = window.getSelection();
    range.setStart(element.childNodes[0], position);
    range.collapse(true);
    sel.removeAllRanges();
    sel.addRange(range);
}

//Funcion para establecer la fecha actual de hace 1 mes
function setFechaInicialMesAtras(elemento) {
    const fechaActual = new Date();
    fechaActual.setMonth(fechaActual.getMonth() - 1);

    const año = fechaActual.getFullYear();
    const mes = String(fechaActual.getMonth() + 1).padStart(2, '0');
    const dia = String(fechaActual.getDate()).padStart(2, '0');

    const fechaFormateada = `${año}-${mes}-${dia}`;
    elemento.val(fechaFormateada);
}





var fotosTemporal = {};
var fotoIndex = 0;

let streamActivo = null;
let camaraActual = null;

function HabilitarCamaraFull(fotografia) {
    camaraActual = fotografia;

    // Limpiar imagen anterior
    $(`#foto_${fotografia}`).attr("src", '').hide();
    $(`#imagen_base64_${fotografia}`).val('');
    $(`#btn_camara_${fotografia}`).prop("disabled", true);

    $("#btn_tomar_foto").off("click").on("click", function () {
        TomarCapturaFull(fotografia);
    });

    document.getElementById("modoCamaraFull").style.display = "flex";

    const video = document.getElementById("video_full");

    navigator.mediaDevices.getUserMedia({ video: { facingMode: { exact: "environment" } } })
        .then(stream => {
            streamActivo = stream;
            video.srcObject = stream;
        })
        .catch(() => {
            // Si no pudo usar la trasera, intenta con la delantera
            navigator.mediaDevices.getUserMedia({ video: { facingMode: "user" } })
                .then(stream => {
                    streamActivo = stream;
                    video.srcObject = stream;
                })
                .catch(() => {
                    document.getElementById("modoCamaraFull").style.display = "none";

                    iziToast.error({
                        title: '❌ Permiso denegado o cámara no disponible',
                        message: 'Por favor habilita el acceso a la cámara para tomar fotos.',
                        timeout: 6000
                    });

                    $(`#btn_camara_${fotografia}`).prop("disabled", false);
                });
        });
}

function TomarCapturaFull(fotografia) {
    const video = document.getElementById("video_full");
    const canvas = document.createElement("canvas");

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext("2d").drawImage(video, 0, 0, canvas.width, canvas.height);

    //const dataUrl = canvas.toDataURL("image/png", 1.0);
    const dataUrl = canvas.toDataURL("image/jpeg", 0.85);

    // Mostrar imagen y guardar
    // 📌 Generar clave temporal
    var clave = "foto_" + (++fotoIndex);
    fotosTemporal[clave] = dataUrl;

    // Guardar solo la clave
    $(`#imagen_base64_${fotografia}`).val(clave);

    // Mostrar imagen (esto sigue igual con base64 solo en <img>)
    $(`#foto_${fotografia}`).attr("src", dataUrl).show();

    $(`#btn_camara_${fotografia}`).prop("disabled", false).text("Activar cámara");

    DeshabilitarCamara();
    document.getElementById("modoCamaraFull").style.display = "none";
}

function TomarCapturaFullLigero(fotografia) {
    const video = document.getElementById("video_full");
    const canvas = document.createElement("canvas");

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    canvas.getContext("2d").drawImage(video, 0, 0, canvas.width, canvas.height);

    const dataUrl = canvas.toDataURL("image/png", 1.0);

    // Mostrar imagen y guardar
    $(`#imagen_base64_${fotografia}`).val(dataUrl);
    $(`#foto_${fotografia}`).attr("src", dataUrl).show();
    $(`#btn_camara_${fotografia}`).prop("disabled", false).text("Activar cámara");

    DeshabilitarCamara();
    document.getElementById("modoCamaraFull").style.display = "none";
}

function DeshabilitarCamara() {
    if (streamActivo) {
        streamActivo.getTracks().forEach(track => track.stop());
        streamActivo = null;
    }
}