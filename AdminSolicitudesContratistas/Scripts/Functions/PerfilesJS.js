// Scripts/Functions/PerfilesJS.js
(function ($) {
    'use strict';

    $(document).ready(function () {
        $('#btnGuardarPermisos').on('click', GuardarPermisos);
    });

    function CargarPermisos(perfil) {
        var codPerfil = perfil.CodPerfil;
        $('#modalPermisosLabel').text('Permisos para el Perfil: ' + perfil.DescPerfil);
        $('#hiddenCodPerfil').val(codPerfil);

        var $tablaBody = $('#tablaPermisos tbody');
        $tablaBody.html('<tr><td colspan="3" class="text-center">Cargando...</td></tr>');
        $('#modalPermisos').modal('show');

        $.ajax({
            type: 'POST',
            url: '/Perfiles/JsonListarOpcionesPorPerfil',
            data: { codPerfil: codPerfil, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (opciones) {
                $tablaBody.empty();
                if (opciones && opciones.length > 0) {
                    $.each(opciones, function (i, opcion) {
                        var isChecked = opcion.Asignado ? 'checked' : '';
                        var fila = '<tr>' +
                            '<td><input type="checkbox" class="form-check-input permiso-check" ' + isChecked +
                            ' data-opcion-id="' + opcion.OpcionMenuID + '" data-id-asignacion="' + opcion.IdOpcionMenuPerfil + '"></td>' +
                            '<td>' + opcion.DescMenu + '</td>' +
                            '<td>' + opcion.DescOpcion + '</td>' +
                            '</tr>';
                        $tablaBody.append(fila);
                    });
                } else {
                    $tablaBody.html('<tr><td colspan="3" class="text-center">No hay opciones de menú disponibles.</td></tr>');
                }
            }
        });
    }

    function GuardarPermisos() {
        var permisos = [];
        $('#tablaPermisos tbody tr').each(function () {
            var $check = $(this).find('.permiso-check');
            if ($check.length > 0) {
                permisos.push({
                    OpcionMenuID: $check.data('opcion-id'),
                    Asignado: $check.is(':checked')
                });
            }
        });

        var codPerfil = $('#hiddenCodPerfil').val();

        $('#btnGuardarPermisos').prop('disabled', true);

        $.ajax({
            type: 'POST',
            url: '/Perfiles/JsonGuardarPermisos',
            data: {
                codPerfil: codPerfil,
                // Convertimos el array de permisos a un string JSON para enviarlo como un solo campo.
                permisosJson: JSON.stringify(permisos),
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            },

            success: function (response) {
                alert(response.message);
                if (response.success) {
                    $('#modalPermisos').modal('hide');
                }
            },
            complete: function () {
                $('#btnGuardarPermisos').prop('disabled', false);
            }
        });
    }

    // Exponer la función para que Angular la pueda llamar
    window.CargarPermisos = CargarPermisos;

})(jQuery);