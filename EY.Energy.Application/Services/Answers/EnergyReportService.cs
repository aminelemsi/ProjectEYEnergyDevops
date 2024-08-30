using DinkToPdf;
using DinkToPdf.Contracts;

using System.Globalization;

namespace EY.Energy.Application.Services.Answers
{
    public class EnergyReportService
    {
      /* private readonly IConverter _pdfConverter;
        private const double DefaultEfficiencyImprovementPercentage = 10.0;
        private const double DefaultCostPerUnit = 0.12;
        private const double DefaultInvestmentCost = 5000;

        public EnergyReportService(IConverter pdfConverter)
        {
            _pdfConverter = pdfConverter;
        }

        public double CalculateTotalEnergyConsumption(double annualElectricityConsumption, double annualFuelConsumption, double officeSurface, double numberOfMachines, double numberOfEmployees, double numberOfVehicles, double fuelConsumptionPerVehicle, double workingHoursPerDay, double workingDaysPerYear)
        {
            double totalFuelConsumption = annualFuelConsumption + (numberOfVehicles * fuelConsumptionPerVehicle * workingDaysPerYear);
            double totalElectricityConsumption = annualElectricityConsumption + (numberOfEmployees * workingHoursPerDay * workingDaysPerYear * 0.05);
            double totalSurfaceConsumption = officeSurface * 0.02;
            double totalMachineConsumption = numberOfMachines * 0.05;
            return totalFuelConsumption + totalElectricityConsumption + totalSurfaceConsumption + totalMachineConsumption;
        }

        public double CalculateGHGEmissions(double totalEnergyConsumption)
        {
            const double EmissionFactor = 0.233; // en kgCO2/kWh
            return totalEnergyConsumption * EmissionFactor;
        }

        public double CalculateEnergySavings(double currentConsumption)
        {
            return currentConsumption * (DefaultEfficiencyImprovementPercentage / 100);
        }

        public double CalculateCostSavings(double energySavings)
        {
            return energySavings * DefaultCostPerUnit;
        }

        public double CalculateROI(double energySavings)
        {
            return (energySavings - DefaultInvestmentCost) / DefaultInvestmentCost;
        }

        private string GenerateEnergyConsumptionChart(string title, double[] electricityConsumption, double[] fuelConsumption, double[] surfaceConsumption, double[] machineConsumption, double[] employeeConsumption, double[] vehicleConsumption, string[] years)
        {
            var model = new PlotModel { Title = title };
            model.Axes.Add(new CategoryAxis { Position = AxisPosition.Bottom, Key = "Years", ItemsSource = years });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0 });

            var electricitySeries = new LineSeries { Title = "Électricité", MarkerType = MarkerType.Circle };
            var fuelSeries = new LineSeries { Title = "Carburant", MarkerType = MarkerType.Circle };
            var surfaceSeries = new LineSeries { Title = "Surface", MarkerType = MarkerType.Circle };
            var machineSeries = new LineSeries { Title = "Machines", MarkerType = MarkerType.Circle };
            var employeeSeries = new LineSeries { Title = "Employés", MarkerType = MarkerType.Circle };
            var vehicleSeries = new LineSeries { Title = "Véhicules", MarkerType = MarkerType.Circle };

            for (int i = 0; i < years.Length; i++)
            {
                electricitySeries.Points.Add(new DataPoint(i, electricityConsumption[i]));
                fuelSeries.Points.Add(new DataPoint(i, fuelConsumption[i]));
                surfaceSeries.Points.Add(new DataPoint(i, surfaceConsumption[i]));
                machineSeries.Points.Add(new DataPoint(i, machineConsumption[i]));
                employeeSeries.Points.Add(new DataPoint(i, employeeConsumption[i]));
                vehicleSeries.Points.Add(new DataPoint(i, vehicleConsumption[i]));
            }

            model.Series.Add(electricitySeries);
            model.Series.Add(fuelSeries);
            model.Series.Add(surfaceSeries);
            model.Series.Add(machineSeries);
            model.Series.Add(employeeSeries);
            model.Series.Add(vehicleSeries);

            string filePath = Path.Combine(Path.GetTempPath(), $"{title.Replace(" ", "_")}.png");
            using (var stream = File.Create(filePath))
            {
                var exporter = new PngExporter(800, 600);
                exporter.Export(model, stream);
            }

            return filePath;
        }

        public byte[] GeneratePdfReport(
            double[] electricityConsumptionLastYears, double[] fuelConsumptionLastYears, double[] surfaceConsumptionLastYears, double[] machineConsumptionLastYears, double[] employeeConsumptionLastYears, double[] vehicleConsumptionLastYears,
            double[] projectedElectricityConsumption, double[] projectedFuelConsumption, double[] projectedSurfaceConsumption, double[] projectedMachineConsumption, double[] projectedEmployeeConsumption, double[] projectedVehicleConsumption)
        {
            var chartPathLastYears = GenerateEnergyConsumptionChart(
                "Répartition de la Consommation Énergétique Annuelle (3 dernières années)",
                electricityConsumptionLastYears, fuelConsumptionLastYears, surfaceConsumptionLastYears, machineConsumptionLastYears, employeeConsumptionLastYears, vehicleConsumptionLastYears,
                new[] { "2022", "2023", "2024" });

            var chartPathNextYear = GenerateEnergyConsumptionChart(
                "Projection de la Consommation Énergétique pour 2025",
                projectedElectricityConsumption, projectedFuelConsumption, projectedSurfaceConsumption, projectedMachineConsumption, projectedEmployeeConsumption, projectedVehicleConsumption,
                new[] { "2025" });

            var totalEnergyConsumption = CalculateTotalEnergyConsumption(
                projectedElectricityConsumption[0], projectedFuelConsumption[0], projectedSurfaceConsumption[0], projectedMachineConsumption[0], projectedEmployeeConsumption[0], projectedVehicleConsumption[0], 0, 0, 0);

            var ghgEmissions = CalculateGHGEmissions(totalEnergyConsumption);
            var predictedEnergySavings = CalculateEnergySavings(totalEnergyConsumption);
            var predictedCostSavings = CalculateCostSavings(predictedEnergySavings);
            var predictedROI = CalculateROI(predictedCostSavings);

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "Energy Report"
            };

            string recommendations = GenerateRecommendations(predictedEnergySavings, ghgEmissions);

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = $@"
            <html lang='fr'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .header {{ text-align: center; font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
                    .section {{ margin-bottom: 20px; }}
                    .label {{ font-weight: bold; }}
                    .chart {{ text-align: center; margin-bottom: 20px; }}
                </style>
            </head>
            <body>
                <div class='header'>Rapport Énergétique</div>
                <div class='chart'><img src='{chartPathLastYears}' alt='Energy Consumption Chart' width='800' height='600'></div>
                <div class='chart'><img src='{chartPathNextYear}' alt='Projected Energy Consumption Chart' width='800' height='600'></div>
                <div class='section'><div class='label'>Prédiction des économies d'énergie:</div><div>{predictedEnergySavings.ToString("N2", CultureInfo.InvariantCulture)} kWh</div></div>
                <div class='section'><div class='label'>Prédiction des économies de coûts:</div><div>{predictedCostSavings.ToString("N2", CultureInfo.InvariantCulture)} €</div></div>
                <div class='section'><div class='label'>Retour sur investissement (ROI):</div><div>{predictedROI.ToString("P2", CultureInfo.InvariantCulture)}</div></div>
                <div class='section'><div class='label'>Émissions de GES:</div><div>{ghgEmissions.ToString("N2", CultureInfo.InvariantCulture)} kgCO2</div></div>
                <div class='section'><div class='label'>Recommandations:</div><div>{recommendations}</div></div>
            </body>
            </html>",
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Rapport généré le [date]" }
            };

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return _pdfConverter.Convert(pdf);
        }

        private string GenerateRecommendations(double energySavings, double ghgEmissions)
        {
            if (energySavings > 1000 && ghgEmissions > 500)
            {
                return "Votre consommation énergétique est élevée. Nous vous recommandons de :<br>- Installer des systèmes de gestion de l'énergie.<br>- Utiliser des sources d'énergie renouvelable.<br>- Améliorer l'efficacité énergétique des bâtiments.";
            }
            if (energySavings > 500 && ghgEmissions > 200)
            {
                return "Votre consommation énergétique est moyenne. Nous vous recommandons de :<br>- Optimiser l'utilisation des machines et des véhicules.<br>- Améliorer l'isolation des bâtiments.";
            }
            else
            {
                return "Votre consommation énergétique est bonne. Continuez vos efforts actuels pour maintenir cette performance.";
            }
        }*/
    }

}
