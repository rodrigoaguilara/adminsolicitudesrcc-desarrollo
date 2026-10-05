// Scripts/Functions/GridUsuariosMOP.js

(function () {
    'use strict';

    // 1. Definimos el controlador de Angular.
    function MainCtrlUsuariosMOP($scope, $http, $q, $interval, uiGridConstants) {

        // =================================================================
        // CONFIGURACIÓN DE LA GRILLA (UI-GRID)
        // Se mantiene tu configuración original completa.
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
            }
        };

        // Definición de las columnas de la grilla.
        $scope.gridOptions.columnDefs = [
            { name: 'UsernameAD', displayName: 'Cuenta', width: '12%' },
            { name: 'Nombre', displayName: 'Usuario', width: '20%' },
            { name: 'Email', displayName: 'e-mail', width: '23%' },
            { name: 'DescPerfil', displayName: 'Perfil', width: '13%' },
            { name: 'TipoReg', displayName: 'Tipo Registro', width: '15%' },
            { name: 'Vigente', displayName: 'Vigente', width: '6%' },
            {
                name: 'Editar',
                displayName: 'Editar',
                width: '10%',
                enableFiltering: false, // No tiene sentido filtrar esta columna
                cellTemplate: '<div class="ui-grid-cell-contents text-center">' +
                    '  <a href="" ng-click="grid.appScope.abrirModalUsuario(row.entity)" title="Editar Usuario">' +
                    '   <i class="bi bi-pencil-square" ></i>' +
                    '  </a>' +
                    '</div>'
            }
        ];

        // =================================================================
        // LÓGICA DE DATOS
        // =================================================================

        // Función para cargar o recargar los datos de la grilla.
        function cargarUsuarios() {
            // Usamos $.ajax para ser consistentes y evitar problemas con AntiForgeryToken.
            $.ajax({
                type: 'POST',
                url: "/UsuariosMOP/JsonTraerUsuariosMOP",
                dataType: "json",
                data: {
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    // Usamos $scope.$apply para notificar a Angular de los cambios.
                    $scope.$apply(function () {
                        if (datos && datos !== "EMPTY") {
                            $scope.gridOptions.data = datos;
                        } else {
                            $scope.gridOptions.data = [];
                        }
                    });
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    // Manejo de error
                    console.error("Error al cargar los usuarios: ", errorThrown);
                    alert("No se pudieron cargar los datos de los usuarios.");
                }
            });
        }

        // Carga inicial de datos al iniciar el controlador.
        cargarUsuarios();

        // =================================================================
        // FUNCIONES EXPUESTAS AL SCOPE (Para ser usadas en la vista HTML)
        // =================================================================

        // Esta función se llama desde el `cellTemplate` de la columna "Editar".
        $scope.abrirModalUsuario = function (usuario) {
            $('#modalUsuarioMOP').modal('show');
            // Llamamos a la función global que está en UsuariosJS.js
            window.CargarDatosUsuarioMOP(usuario.UsernameAD);
        };

        // Esta es la función que será llamada desde fuera para refrescar la grilla.
        $scope.actualizarGrid = function () {
            cargarUsuarios();
        };
    }

    // 2. Exponemos la función de actualización al ámbito global (window).
    // Esto crea un "puente" para que UsuariosJS.js pueda llamar a una función
    // dentro de nuestro controlador de Angular.
    window.ActualizarTablaUsuarios = function () {
        // Obtenemos el scope del controlador de Angular que está en la página.
        var scope = angular.element(document.querySelector('[ng-controller="MainCtrlUsuariosMOP as $ctrl"]')).scope();

        // Verificamos que el scope exista y llamamos a la función.
        if (scope && typeof scope.actualizarGrid === 'function') {
            scope.actualizarGrid();
        }
    };

    // 3. Definimos el módulo de Angular y registramos el controlador.
    var app = angular.module('gridUsuariosMOP', [
        'ngAnimate',
        'ngTouch',
        'ui.grid',
        'ui.grid.selection',
        'ui.grid.cellNav',
        'ui.grid.resizeColumns',
        'ui.grid.pagination'
    ]).controller('MainCtrlUsuariosMOP', MainCtrlUsuariosMOP);

    // 4. Inyectamos las dependencias necesarias al controlador.
    MainCtrlUsuariosMOP.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];

})();