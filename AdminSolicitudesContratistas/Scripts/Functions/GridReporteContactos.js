// Scripts/Functions/GridReporteContactos.js

// Usamos una IIFE para proteger el ámbito de nuestro código.
(function () {
    'use strict';

    // Definimos el controlador de Angular para nuestro reporte.
    function MainCtrlReporteContactos($scope, $http, uiGridConstants) {
        var self = this; // Usamos 'self' para referirnos al controlador dentro de las funciones.

        // Configuración inicial de la tabla (ui-grid).
        $scope.gridOptions = {
            enableFiltering: true, // Habilita los filtros en cada columna.
            enableRowSelection: true,
            enableRowHeaderSelection: false,
            multiSelect: false,
            showGridFooter: true,
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            }
        };

        // Definición de las columnas que se mostrarán en la tabla.
        $scope.gridOptions.columnDefs = [
            { name: 'Rut', displayName: 'RUT', width: '10%' },
            { name: 'RazonSocial', displayName: 'Razón Social', width: '20%' },
            { name: 'EstadoProveedor', displayName: 'Estado', width: '10%' },
            { name: 'TipoRegistro', displayName: 'Tipo Registro', width: '12%' },
            { name: 'Email', displayName: 'Email', width: '15%' },
            { name: 'TelefonoContacto', displayName: 'Teléfono', width: '10%' },
            { name: 'CelularContacto', displayName: 'Celular', width: '10%' },
            // Columnas extras que están disponibles pero ocultas por defecto.
            { name: 'Direccion', displayName: 'Dirección', width: '15%', visible: false },
            { name: 'Comuna', displayName: 'Comuna', width: '10%', visible: false },
            { name: 'Ciudad', displayName: 'Ciudad', width: '10%', visible: false },
            { name: 'Region', displayName: 'Región', width: '12%' }
        ];

        // Función para cargar los datos del reporte desde el servidor.
        function cargarDatos() {
            // Usamos $.ajax, igual que en tu otro reporte que funciona.
            $.ajax({
                type: 'POST',
                url: "/ReporteContactosProveedores/JsonBuscarReporteContactos",
                dataType: "json",
                // Enviamos el token en el cuerpo de los datos, que es lo que ASP.NET MVC espera.
                data: {
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    // Usamos $scope.$apply para notificar a Angular que estamos cambiando datos
                    // desde fuera de su ciclo de vida (desde un callback de jQuery).
                    $scope.$apply(function () {
                        if (datos && datos !== "EMPTY") {
                            $scope.gridOptions.data = datos;
                        } else {
                            $scope.gridOptions.data = [];
                        }
                    });

                    // Ocultamos el mensaje de "cargando".
                    $("#divCargando").hide();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Manejamos el error
                    $("#divCargando").text('Error al cargar los datos del reporte.').addClass('alert-danger');
                }
            });
        }



        // Llamamos a la función para cargar los datos cuando se inicia el controlador.
        cargarDatos();

        // Hacemos que la función de descarga esté disponible en la vista (a través de $ctrl.DownloadExcel).
        self.DownloadExcel = function () {
            DownloadExcel($scope);
        };
    }

    // Función para preparar y enviar los datos para la exportación a Excel.
    function DownloadExcel($scope) {
        // Obtenemos solo las filas visibles/filtradas de la tabla.
        var listaParaExportar = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid).map(function (row) {
            return row.entity;
        });

        if (listaParaExportar.length === 0) {
            alert("No hay datos para exportar.");
            return;
        }

        // Creamos el objeto que enviaremos al controlador.
        var args = {
            file: 'Contactos de Proveedores',
            stringDatos: JSON.stringify(listaParaExportar)
        };

        // Creamos un formulario dinámico para enviar los datos y provocar la descarga.
        var form = $('<form>', {
            "method": "post",
            "html": '<textarea name="data" style="display:none;">' + JSON.stringify(args) + '</textarea>',
            "action": '/ReporteContactosProveedores/ExportarExcel'
        }).appendTo('body').submit();

        // Eliminamos el formulario después de enviarlo.
        form.remove();
    }

    // Definimos el módulo de Angular y registramos nuestro controlador.
    var app = angular.module('gridReporteContactos', ['ngAnimate', 'ngTouch', 'ui.grid'])
        .controller('MainCtrlReporteContactos', MainCtrlReporteContactos);

    // Inyectamos las dependencias necesarias al controlador.
    MainCtrlReporteContactos.$inject = ['$scope', '$http', 'uiGridConstants'];

})();