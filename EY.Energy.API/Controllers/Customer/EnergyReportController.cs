using EY.Energy.Application.DTO;
using EY.Energy.Application.Services.Answers;
using Microsoft.AspNetCore.Mvc;

namespace EY.Energy.API.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyReportController : ControllerBase
    {
      /*private readonly EnergyReportService _energyService;

        public EnergyReportController(EnergyReportService energyService)
        {
            _energyService = energyService;
        }

        [HttpPost("generate-report")]
        public IActionResult GenerateReport([FromBody] EnergyConsumptionData consumptionData)
        {
            var pdfFile = _energyService.GeneratePdfReport(
                consumptionData.ElectricityConsumptionLastYears,
                consumptionData.FuelConsumptionLastYears,
                consumptionData.SurfaceConsumptionLastYears,
                consumptionData.MachineConsumptionLastYears,
                consumptionData.EmployeeConsumptionLastYears,
                consumptionData.VehicleConsumptionLastYears,
                consumptionData.ProjectedElectricityConsumption,
                consumptionData.ProjectedFuelConsumption,
                consumptionData.ProjectedSurfaceConsumption,
                consumptionData.ProjectedMachineConsumption,
                consumptionData.ProjectedEmployeeConsumption,
                consumptionData.ProjectedVehicleConsumption);
            return File(pdfFile, "application/pdf", "EnergyReport.pdf");
        }*/
    }
}
