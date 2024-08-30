using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.DTO
{
    public class EnergyConsumptionData
    {
        public double[] ElectricityConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] FuelConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] SurfaceConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] MachineConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] EmployeeConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] VehicleConsumptionLastYears { get; set; } = Array.Empty<double>();
        public double[] ProjectedElectricityConsumption { get; set; } = Array.Empty<double>();
        public double[] ProjectedFuelConsumption { get; set; } = Array.Empty<double>();
        public double[] ProjectedSurfaceConsumption { get; set; } = Array.Empty<double>();
        public double[] ProjectedMachineConsumption { get; set; } = Array.Empty<double>();
        public double[] ProjectedEmployeeConsumption { get; set; } = Array.Empty<double>();
        public double[] ProjectedVehicleConsumption { get; set; } = Array.Empty<double>();
    }
}
