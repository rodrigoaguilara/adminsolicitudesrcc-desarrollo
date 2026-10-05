// Scripts/Functions/LayoutJS.js
(function ($) {
    'use strict';

    // =========================================================================
    // 1. CÓDIGO DE INICIALIZACIÓN
    // =========================================================================
    $(document).ready(function () {
        var usr = $("#hiddenUsernameAD").val();
        if (usr) {
            CargarMenu(usr);
        }

        // Lógica del temporizador de sesión
        //iniciarTemporizadorSesion();
    });

    // =========================================================================
    // 2. DEFINICIONES DE FUNCIONES
    // =========================================================================

    function CargarMenu(username) {
        var $listMenu = $("#listMenu");
        $listMenu.empty();

        console.log('inicio de CargarMenu');

        $.ajax({
            type: 'POST',
            url: "/Menu/JsonTraerMenuCompleto", // <-- Llamada al nuevo endpoint consolidado
            dataType: "json",
            // NOTA: Añadimos el AntiforgeryToken que ahora espera el controlador
            data: { username: username, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (menuCompleto) {
                // Añadimos el enlace a la Página Principal, que siempre está presente.
                $listMenu.append(
                    '<li class="nav-item">' +
                    '  <a href="/Home/Index" class="nav-link">Página Principal</a>' +
                    '</li>'
                );

                if (menuCompleto && menuCompleto.length > 0) {
                    // Iteramos sobre los menús principales (Reportes, Mantenedores, etc.)
                    $.each(menuCompleto, function (i, menu) {
                        var opcionesHtml = '';

                        // Iteramos sobre las sub-opciones de este menú
                        $.each(menu.Opciones, function (j, opcion) {
                            opcionesHtml += '<li><a class="dropdown-item" href="/' + opcion.Controlador + '/Index">' + opcion.DescOpcion + '</a></li>';
                        });

                        // Construimos el HTML completo para este dropdown del menú.
                        var menuDropdownHtml =
                            '<li class="nav-item dropdown">' +
                            '  <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">' +
                            menu.DescMenu +
                            '  </a>' +
                            '  <ul class="dropdown-menu">' +
                            opcionesHtml +
                            '  </ul>' +
                            '</li>';

                        $listMenu.append(menuDropdownHtml);
                    });
                }
            },
            error: function () {
                $listMenu.append('<li class="nav-item"><span class="nav-link text-danger">Error al cargar el menú</span></li>');
            }
        });
    }

    // Lógica del temporizador de sesión
    //function iniciarTemporizadorSesion() {
    //    var sessionTimeoutMilliseconds = parseInt($("#hiddenMinutosTimeoutms").val());
    //    if (isNaN(sessionTimeoutMilliseconds) || sessionTimeoutMilliseconds <= 0) {
    //        return;
    //    }

    //    setTimeout(function () {
    //        window.location.href = '/Home/Login';
    //    }, sessionTimeoutMilliseconds);

    //    var sessionTimeoutSeconds = sessionTimeoutMilliseconds / 1000;
    //    var countdownInterval = setInterval(function () {
    //        sessionTimeoutSeconds--;
    //        if (sessionTimeoutSeconds <= 0) {
    //            clearInterval(countdownInterval);
    //        }
    //        var tiempoFormateado = formatTime(sessionTimeoutSeconds);
    //        $("#lblTiempoSesion").text("Fin de sesión en " + tiempoFormateado);
    //    }, 1000);
    //}

    //function formatTime(seconds) {
    //    if (seconds < 0) seconds = 0;
    //    var minutes = Math.floor(seconds / 60);
    //    var remainingSeconds = seconds % 60;
    //    // Simplificado para no mostrar horas si no es necesario
    //    return String(minutes).padStart(2, '0') + ":" + String(remainingSeconds).padStart(2, '0');
    //}

})(jQuery);