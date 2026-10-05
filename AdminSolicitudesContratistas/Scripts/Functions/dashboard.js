var DashboardMOP = (function ($) {
    "use strict";

    // Función para obtener el Token
    var getToken = function () {
        return $('input[name="__RequestVerificationToken"]').val();
    };

    var normalizar = function (texto) {
        if (!texto) return "";
        return texto.toString().toLowerCase()
            .replace(/región de /g, "").replace(/región del /g, "").replace(/región /g, "")
            .normalize("NFD").replace(/[\u0300-\u036f]/g, "").trim();
    };

    var cargarMapaChile = function () {
        try {
            Chart.register(ChartGeo.ChoroplethController, ChartGeo.ProjectionScale, ChartGeo.ColorScale, ChartGeo.GeoFeature);
        } catch (e) { }

        $.getJSON("/Scripts/chile_regiones.json", function (geoJson) {

            // --- CIRUGÍA DE GEOMETRÍA ---
            geoJson.features.forEach(f => {
                // Solo procesamos si es MultiPolygon (como Valparaíso o Magallanes)
                if (f.geometry.type === "MultiPolygon") {
                    // Filtramos los polígonos internos de la región
                    f.geometry.coordinates = f.geometry.coordinates.filter(polygon => {
                        // polygon[0][0][0] es la longitud de uno de los puntos
                        var longitud = polygon[0][0][0];
                        // Solo dejamos los pedazos de tierra al este del meridiano -80
                        return longitud > -80;
                    });
                } else if (f.geometry.type === "Polygon") {
                    // Si fuera un polígono simple (raro en este JSON), verificamos su longitud
                    var longitud = f.geometry.coordinates[0][0][0];
                    if (longitud < -80) {
                        // Si es una isla simple, la "vaciamos"
                        f.geometry.coordinates = [];
                    }
                }
            });

            // Aseguramos que solo queden las 16 regiones administrativas
            geoJson.features = geoJson.features.filter(f => {
                const cod = parseInt(f.properties.codregion);
                return cod >= 1 && cod <= 16;
            });

            $.ajax({
                url: "/Dashboard/GetProveedoresPorRegion",
                type: "POST",
                data: { __RequestVerificationToken: getToken() },
                success: function (res) {
                    if (res.success) {
                        renderMap(geoJson, res.data, 'codregion');
                    }
                }
            });
        });
    };

    var renderMap = function (geoJson, dbData, propCodigo) {
        const ctx = document.getElementById('chartMapaChile').getContext('2d');

        let chartStatus = Chart.getChart("chartMapaChile");
        if (chartStatus !== undefined) { chartStatus.destroy(); }

        new Chart(ctx, {
            type: 'choropleth',
            data: {
                labels: geoJson.features.map(d => d.properties[propCodigo].toString()),
                datasets: [{
                    label: 'Proveedores',
                    outline: geoJson.features,
                    data: geoJson.features.map(d => {
                        const codigoJson = d.properties[propCodigo].toString();
                        const registroDb = dbData.find(x => x.CodRegion.toString() === codigoJson);

                        return {
                            feature: d,
                            value: registroDb ? registroDb.Total : 0,
                            info: {
                                nombre: registroDb ? registroDb.RegionNombre : ("Región " + codigoJson),
                                total: registroDb ? registroDb.Total : 0,
                                cma: registroDb ? registroDb.CMA : 0,
                                cme: registroDb ? registroDb.CME : 0,
                                cns: registroDb ? registroDb.CNS : 0
                            }
                        };
                    })
                }]
            },
            options: {
                maintainAspectRatio: false,
                showOutline: true,
                scales: {
                    projection: {
                        axis: 'x',
                        projection: 'mercator'
                    },
                    color: {
                        axis: 'x',
                        display: true,
                        interpolate: 'blues',
                        legend: {
                            position: 'bottom-right',
                            align: 'right',
                            length: 350, // Ajustamos largo
                            width: 20,   // Ajustamos grosor
                        }
                    }
                },
                layout: {
                    // El padding negativo a la izquierda ayuda a centrar Chile continental
                    padding: { left: -50, right: 30, top: 10, bottom: 10 }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const d = context.raw.info;
                                return [
                                    `Región: ${d.nombre}`,
                                    `Total: ${d.total}`,
                                    `--------------------------`,
                                    //`CMA: ${d.cma} | CME: ${d.cme} | CNS: ${d.cns}`
                                    `Obra Mayor: ${d.cma} | Obra Menor: ${d.cme} | Consultores: ${d.cns}`
                                ];
                            }
                        }
                    }
                }
            }
        });
    };


    var cargarSolicitudesRegion = function () {
        $.ajax({
            url: "/Dashboard/GetSolicitudesPorRegion",
            type: "POST",
            data: { __RequestVerificationToken: getToken() },
            success: function (res) {
                if (res.success) {
                    // FILTRO: Removemos la fila de 'TOTALES' que viene del SP
                    var dataFiltrada = res.data.filter(x => x.RegionNombre !== 'TOTALES');
                    renderStackedSolicitudes(dataFiltrada);
                }
            }
        });
    };

    var renderStackedSolicitudes = function (dbData) {
        const ctx = document.getElementById('chartSolicitudesRegion').getContext('2d');

        const labels = [...new Set(dbData.map(x => x.RegionNombre))];
        const tipos = [...new Set(dbData.map(x => x.TipoSolicitud))];

        const datasets = tipos.map((tipo, index) => {
            const colores = ['#f6c23e', '#4e73df', '#1cc88a', '#e74a3b', '#36b9cc'];
            return {
                label: tipo,
                data: labels.map(reg => {
                    const item = dbData.find(d => d.RegionNombre === reg && d.TipoSolicitud === tipo);
                    return item ? item.Cantidad : 0;
                }),
                backgroundColor: colores[index % colores.length],
                barThickness: 15
            };
        });

        new Chart(ctx, {
            type: 'bar',
            data: { labels: labels, datasets: datasets },
            options: {
                indexAxis: 'y',
                maintainAspectRatio: false,

                interaction: {
                    mode: 'index',     // Captura todos los elementos del mismo índice (región)
                    intersect: false,  // Permite que el tooltip aparezca aunque no toques exactamente la barra
                    axis: 'y'          // ¡IMPORTANTE! Fuerza a buscar solo por posición vertical
                },

                scales: {
                    x: {
                        type: 'logarithmic',
                        stacked: true,
                        min: 1,
                        title: { display: true, text: 'Cantidad de Solicitudes (Escala Logarítmica)' },
                        ticks: {
                            callback: value => Number(value).toLocaleString()
                        }
                    },
                    y: {
                        stacked: true,
                        ticks: { font: { size: 10 } }
                    }
                },
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {                        
                        callbacks: {
                            label: function (context) {
                                return `${context.dataset.label}: ${context.raw}`;
                            }
                        }
                    },
                    subtitle: {
                        display: true,
                        text: '* Escala logarítmica para visualizar estados de bajo volumen',
                        color: '#858796',
                        font: {
                            size: 11,
                            style: 'italic'
                        },
                        padding: { bottom: 10 }
                    }
                }
            }
        });


    };


    // --- GRÁFICO 1: PROVEEDORES ---
    var cargarGraficoProveedores = function () {
        $.ajax({
            url: "/Dashboard/GetProveedoresVigentesPorTipo",
            type: "POST",
            data: { __RequestVerificationToken: getToken() },
            success: function (res) {
                if (res.success) renderPieChart(res.data);
            }
        });
    };

    var renderPieChart = function (data) {
        var ctx = document.getElementById('chartProveedoresTipo').getContext('2d');
        new Chart(ctx, {
            type: 'pie',
            data: {
                labels: data.map(e => e.Etiqueta),
                datasets: [{
                    data: data.map(e => e.Cantidad),
                    backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc']
                }]
            },
            options: { maintainAspectRatio: false }
        });
    };

    // --- GRÁFICO 2: SOLICITUDES ---
    var cargarGraficoSolicitudes = function () {
        $.ajax({
            url: "/Dashboard/GetSolicitudesPorEstado",
            type: "POST",
            data: { __RequestVerificationToken: getToken() },
            success: function (res) {
                if (res.success) renderBarChart(res.data);
            }
        });
    };

    var renderBarChart = function (data) {
        var ctx = document.getElementById('chartSolicitudesEstado').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(e => e.Etiqueta),
                datasets: [{
                    label: 'Cant. Solicitudes',
                    data: data.map(e => e.Cantidad),
                    backgroundColor: '#f6c23e',
                    borderColor: '#f4b619',
                    borderWidth: 1
                }]
            },
            options: {
                maintainAspectRatio: false,
                scales: {
                    y: {
                        type: 'logarithmic', // Escala Logarítmica
                        beginAtZero: false,
                        min: 1, // Evita errores con valores en 0
                        title: {
                            display: true,
                            text: 'Cantidad (Escala Log)', // <--- TEXTO EN EL EJE
                            font: { weight: 'bold' }
                        },
                        ticks: {
                            callback: value => Number(value).toLocaleString()
                        }
                    }
                },
                plugins: {
                    legend: { display: false },
                    // --- SUBTÍTULO INTERNO DEL GRÁFICO ---
                    subtitle: {
                        display: true,
                        text: '* Escala logarítmica para visualizar estados de bajo volumen',
                        color: '#858796',
                        font: {
                            size: 11,
                            style: 'italic'
                        },
                        padding: { bottom: 10 }
                    }
                }
            }
        });
    };

    // Inicializador público
    return {
        init: function () {
            cargarGraficoProveedores();
            cargarGraficoSolicitudes();
            cargarMapaChile();
            cargarSolicitudesRegion();
        }
    };

})(jQuery);