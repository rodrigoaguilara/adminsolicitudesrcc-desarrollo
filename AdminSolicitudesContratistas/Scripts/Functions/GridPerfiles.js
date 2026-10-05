
(function () {
    'use strict';

    function MainCtrlPerfiles($scope, $http, uiGridConstants) { // Mantenemos $http por si se usa en otro lado, aunque no en la carga inicial.

        // =================================================================
        // CONFIGURACIÓN DE LA GRILLA (UI-GRID)
        // =================================================================
        $scope.gridOptions = {
            enableFiltering: true,
            enableCellEditOnFocus: false,
            enableRowSelection: true,
            enableRowHeaderSelection: false,
            enableHiding: false,
            selectionRowHeaderWidth: 30,
            multiSelect: false,
            showGridFooter: true,
            paginationPageSizes: [25, 50, 100],
            paginationPageSize: 25,
            enableGridMenu: true,
            exporterMenuCsv: false,
            exporterMenuExcel: false,
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            },
            columnDefs: [
                { name: 'CodPerfil', displayName: 'Código', width: '20%' },
                { name: 'DescPerfil', displayName: 'Descripción del Perfil', width: '60%' },
                {
                    name: 'Editar', width: '20%', enableFiltering: false, displayName: 'Editar Menú de Perfiles',
                    cellTemplate: '<div class="ui-grid-cell-contents text-center">' +
                        '<a href="" ng-click="grid.appScope.abrirModalPermisos(row.entity)" title="Editar Permisos">' +
                        '<i class="bi bi-pencil-square"></i></a></div>'
                }
            ]
        };

        // =================================================================
        // LÓGICA DE DATOS
        // =================================================================

        // Función para cargar los datos usando $.ajax para consistencia.
        function cargarPerfiles() {
            $.ajax({
                type: 'POST',
                url: "/Perfiles/JsonListarPerfiles", 
                dataType: "json",
                data: {
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    // Usamos $scope.$apply para notificar a Angular que los datos han cambiado.
                    $scope.$apply(function () {
                        if (datos && datos !== "EMPTY") {
                            $scope.gridOptions.data = datos;
                        } else {
                            $scope.gridOptions.data = [];
                        }
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Error al cargar los perfiles: ", errorThrown);
                    alert("No se pudieron cargar los datos de los perfiles.");
                }
            });
        }

        // Carga inicial de datos.
        cargarPerfiles();

        // =================================================================
        // FUNCIONES EXPUESTAS AL SCOPE
        // =================================================================

        $scope.abrirModalPermisos = function (perfil) {
            // Llama a la función global que estará en PerfilesJS.js
            window.CargarPermisos(perfil);
        };

        // Función para refrescar el grid desde fuera.
        $scope.actualizarGrid = function () {
            cargarPerfiles();
        };
    }

    // =========================================================================
    // COMUNICACIÓN EXTERNA Y DEFINICIÓN DEL MÓDULO
    // =========================================================================

    // Exponemos la función de actualización para que PerfilesJS.js pueda llamarla.
    window.ActualizarTablaPerfiles = function () {
        var scope = angular.element(document.querySelector('[ng-controller="MainCtrlPerfiles as $ctrl"]')).scope();
        if (scope && typeof scope.actualizarGrid === 'function') {
            scope.actualizarGrid();
        }
    };

    // Definimos el módulo de Angular con todas sus dependencias.
    angular.module('gridPerfiles', [
        'ngAnimate',
        'ngTouch',
        'ui.grid',
        'ui.grid.pagination',
        'ui.grid.selection',
        'ui.grid.cellNav',
        'ui.grid.resizeColumns'
    ])
        .controller('MainCtrlPerfiles', MainCtrlPerfiles);

    // Inyectamos las dependencias.
    MainCtrlPerfiles.$inject = ['$scope', '$http', 'uiGridConstants'];

})();