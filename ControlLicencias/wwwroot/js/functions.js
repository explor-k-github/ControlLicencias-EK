function checkRut(rut) {
    // Despejar Puntos
    var valor = rut.value.replace('.', '');
    // Despejar Guión
    valor = valor.replace('-', '');

    // Aislar Cuerpo y Dígito Verificador
    cuerpo = valor.slice(0, -1);
    dv = valor.slice(-1).toUpperCase();

    // Formatear RUN
    //rut.value = cuerpo + dv;
   

    // Si no cumple con el mínimo ej. (n.nnn.nnn)
    if (cuerpo.length < 7) { rut.setCustomValidity("RUT Incompleto"); return false; }

    // Calcular Dígito Verificador
    suma = 0;
    multiplo = 2;

    // Para cada dígito del Cuerpo
    for (i = 1; i <= cuerpo.length; i++) {

        // Obtener su Producto con el Múltiplo Correspondiente
        index = multiplo * valor.charAt(cuerpo.length - i);

        // Sumar al Contador General
        suma = suma + index;

        // Consolidar Múltiplo dentro del rango [2,7]
        if (multiplo < 7) { multiplo = multiplo + 1; } else { multiplo = 2; }

    }

    // Calcular Dígito Verificador en base al Módulo 11
    dvEsperado = 11 - (suma % 11);

    // Casos Especiales (0 y K)
    dv = (dv == 'K') ? 10 : dv;
    dv = (dv == 0) ? 11 : dv;

    // Validar que el Cuerpo coincide con su Dígito Verificador
    if (dvEsperado != dv) { rut.setCustomValidity("RUT Inválido"); return false; }

    // Si todo sale bien, eliminar errores (decretar que es válido)
    rut.setCustomValidity('');
}

function consultaRut() {
    M.toast({ html: 'Buscando Rut...' });
    var rut = $("#rut").val();
    var chor = $("#chore").val();
    var settings = {
        "async": true,
        "crossDomain": true,
        "url": "../api/License/FillForm",
        "method": "POST",
        "headers": {
            "Content-Type": "application/json",
            "User-Agent": "PostmanRuntime/7.15.2",
            "Accept": "*/*",
            "Cache-Control": "no-cache",
            "Postman-Token": "1277ee3c-9a85-4a31-8adc-a57a237bce70,febf430a-22bb-4a1a-affe-b8dcea0a1df1",
            "Host": "localhost:44382",
            "Accept-Encoding": "gzip, deflate",
            "Content-Length": "23",
            "Connection": "keep-alive",
            "cache-control": "no-cache"
        },
        "processData": false,
        "data": "{\"content\":\"" + rut + "\", \"chore\": \""+ chor +"\"}"
    }

    $.ajax(settings).done(function (response) {
        console.log(response);


        var driver = JSON.parse(response);
        if (driver.error) {
            M.toast({ html: 'Error al Consultar el Rut' });
        } else {
            M.toast({ html: 'Datos Encontrados' });
            var conv = buildDate(driver.ExpLicense + "");
            var explic = moment(conv, "DD-MM-YYYY").format("YYYY-MM-DD");
            $("#name").val(driver.FullNames);
            $("#lastname").val(driver.LastNames);
            document.getElementById("licexpdate").value = explic;
            //$("#licexpdate").val(explic);
            $("#anglolicense").val(driver.LicenseType);
            $("#license").val(driver.LicenseType);
            var cnv = driver.AccDate + "";
            var wdate = moment(cnv, "DD-MM-YYYY").format("YYYY-MM-DD");
            document.getElementById("wcdate").value = wdate;

        }



    });
}

function convertDate(date) {
    var month = date.split('-')[1];
    switch (month) {
        case "ene":
            return "01";
        case "feb":
            return "02";
        case "mar":
            return "03";
        case "abr":
            return "04";
        case "may":
            return "05";
        case "jun":
            return "06";
        case "jul":
            return "07";
        case "ago":
            return "08";
        case "sep":
            return "09";
        case "oct":
            return "10";
        case "nov":
            return "11";
        case "dic":
            return "12";
        default: return "";
        //break;
    }



}

function buildDate(date) {
    var day = date.split('-')[0];
    var month = convertDate(date);
    var year = date.split('-')[2];
    return day + "-" + month + "-" + year;
}