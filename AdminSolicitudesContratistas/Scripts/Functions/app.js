var app = angular.module('app', ['ngTouch', 'ui.grid']);

app.controller('MainCtrl', ['$scope', '$http', '$q', function ($scope, $http, $q) {
    $scope.gridOptions = {
    };

    $scope.gridOptions.columnDefs = [
        { name: 'ID' },
        { name: 'Rut' },
        { name: 'RazonSocial' },
        { name: 'NombreContacto' }
    ];

    var canceler = $q.defer();
    //$http.get ("/Proveedores/JsonTraerProveedores", { timeout: canceler.promise })
    //    .then(function (response) {
    //        $scope.gridOptions.data = response; //response.data
    //    });

    var rutProveedor = "";
    var razonSocial = "";
    var codEstadoMOP = "";
    var region = 0;
    var tipoRegistro = "";

    $.ajax({
        type: 'POST',
        url: "/Proveedores/JsonTraerProveedores",
        dataType: "json",
        data: { rutProveedor: rutProveedor, razonSocial: razonSocial, codEstadoMOP: codEstadoMOP, region: region, tipoRegistro: tipoRegistro, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
        success: function (datos) {

            if (datos === "EMPTY") {
                //MostrarInfoTablaSinDatos('SIN DATOS PARA ESTA OPCIÓN');
                //TablaHistorialSolicitudesVacia();
            } else {

                $scope.gridOptions.data = datos;
                //$scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

            }
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });

    $scope.$on('$destroy', function () {
        canceler.resolve();  // Aborts the $http request if it isn't finished.
    });
}]);