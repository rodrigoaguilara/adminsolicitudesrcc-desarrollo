




function MainCtrlCapacidadEconomicaHistorica($scope, $http, $q, $interval, uiGridConstants) {
    //i18nService.setCurrentLang('es');

    var self = this;
    //DEFINICIÓN DE COLUMNAS DE LA GRILLA
    $scope.gridOptions = {
        enableFiltering: true,
        enableCellEditOnFocus: false,
        enableRowSelection: true,
        enableRowHeaderSelection: false,
        enableHiding: false,
        selectionRowHeaderWidth: 30,
        multiSelect: false,
        showGridFooter: true,
        enablePagination: false,
        onRegisterApi: function (gridApi) {
            self.gridApi = gridApi;
            $scope.gridApi = gridApi;
        }

    };



    $scope.gridOptions.columnDefs = [
        //{ name: 'IdSol', displayName: 'N°', type: 'number', width: '4%' },
        //{
        //    name: 'ID', displayName: 'N° Sol', type: 'number', width: '5%',
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "'" + "{{row.entity.ID}}" + "'" + ')">  {{row.entity.ID}} </a>  </center>'
        //    cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(row.entity)">  {{row.entity.ID}} </a>  </center>'
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "{{row.entity.ID}}" + ')">  {{row.entity.ID}} </a> </center> '
        //},
        //{ name: 'IdSolPadre', displayName: 'Sol Padre', width: '5%' },
        { name: 'RutProveedor', displayName: 'RUT', width: '6%' },
        { name: 'RazonSocial', displayName: 'Razón Social', width: '33%' },
        { name: 'CapacidadEconomica', displayName: 'Cap. Económica', type: 'number', width: '9%' },
        { name: 'FechaEstadoFinanciero', displayName: 'Fecha Estado Financiero', type: 'date', width: '10%' },
        { name: 'EstadoProveedor', displayName: 'Estado Proveedor',width: '9%' },
        { name: 'FechaRenovacion', displayName: 'Fecha Renovación', type: 'date', width: '9%' },
        { name: 'Vigencia', displayName: 'Vigencia Estado Financiero', width: '11%' },
        { name: 'TipoRegistro', displayName: 'Tipo de Registro', width: '15%' }
    ];

    $scope.uiGridConstants = uiGridConstants;


    self.$scope = $scope;

    $.ajax({
        type: 'POST',
        url: "/ReporteCapacidadEconomicaHistorica/JsonBuscarReporteCapacidadEconomicaHistorica",
        dataType: "json",
        data: {
            __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
        },
        success: function (datos) {
            $scope.gridOptions.data = datos;
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

            $("#divCargando").css('display', 'none');

        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });


    self.DownloadExcel = function () {
        DownloadExcel($scope);
    };

}



function DownloadExcel($scope) {
    //$('#btnDownloadExcel').css('display', 'none');
    //$('#btnProcesandoExcel').css('display', 'block');
    var lista = [];
    //console.log('asdfdsfasdf');
    var filteredRows = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid);
    //var filteredRows = $scope.gridOptions.data;
    //console.log(filteredRows);

    for (var i in filteredRows) {

        var obj = filteredRows[i].entity;
        //var obj = filteredRows[i];

        var reporte = newReporte([
            obj.RutProveedor,
            obj.RazonSocial,
            obj.CapacidadEconomica,            
            obj.FechaEstadoFinanciero,
            obj.EstadoProveedor,
            obj.FechaRenovacion,
            obj.Vigencia,
            obj.TipoRegistro
        ]);
        lista.push(reporte);
    }
    var json = JSON.stringify(lista);
    var args = {
        file: 'Capacidad Economica Historica',
        //base64: btoa(json)
        stringDatos: json
    };
    var form = $('<form>', {
        "id": "downloadReporteCapEconomicaHist",
        "method": "post",
        "html": '<textarea id="data" name="data" style="display:none;">#1</textarea>'.replace('#1', JSON.stringify(args)),
        "action": /*window.siteRoot +*/ '/ReporteCapacidadEconomicaHistorica/ReporteCapacidadEconomicaHistorica'
    }).appendTo($('#modalForm')).submit();
    form.remove();

    //console.log(lista);

    //$('#btnDownloadExcel').css('display', 'block');
    //$('#btnProcesandoExcel').css('display', 'none');

}


function newReporte(obj = undefined) {
    return {
        'RutProveedor': obj?.length > 0 ? obj[0] : undefined,
        'RazonSocial': obj?.length > 0 ? obj[1] : undefined,
        'CapacidadEconomica': obj?.length > 0 ? obj[2] : undefined,
        'FechaEstadoFinanciero': obj?.length > 0 ? obj[3] : undefined,
        'EstadoProveedor': obj?.length > 0 ? obj[4] : undefined,
        'FechaRenovacion': obj?.length > 0 ? obj[5] : undefined,
        'Vigencia': obj?.length > 0 ? obj[6] : undefined,
        'TipoRegistro': obj?.length > 0 ? obj[7] : undefined        
    }
}

function decode_utf8(s) {
    try {
        return decodeURIComponent(escape(s));
    }
    catch (err) {
        return s;
    }
}

var app = angular.module('gridRepCapacidadEconomicaHistorica',
    [
        //'ngAria',
        'ngAnimate',
        'ngTouch',
        'ui.grid',
        /*'ui.grid.edit',*/
        'ui.grid.selection',
        'ui.grid.cellNav',
        'ui.grid.resizeColumns'//,
        //'ui.grid.pagination'
    ]).controller('MainCtrlCapacidadEconomicaHistorica', MainCtrlCapacidadEconomicaHistorica);





MainCtrlCapacidadEconomicaHistorica.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];





function DateFormat(value, row, index) {
    //moment.locale('es');


    if (value == null) {
        //value == '---';
        return '---';
    } else {
        return moment(value).format('DD/MM/YYYY');
    }
}

function DateTimeFormat(value, row, index) {
    //moment.locale('es');

    if (value == null) {
        //value == '---';
        return '---';
    } else {
        //return moment(value).format('DD/MM/YYYY hh:mm:ss');
        return moment(value).format('DD/MM/YYYY hh:mm:ss a');
    }
}
