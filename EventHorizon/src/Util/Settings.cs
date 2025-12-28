using System.ComponentModel.DataAnnotations;

namespace EventHorizon.src.Util
{
    public class Settings
    {
        [Key]
        public int Id { get; set; } = 1;
        public bool SimulateI2CDevices { get; set; } = false;
        public bool InvertMemoryOutputs { get; set; } = false;
        public int TimelineDuration { get; set; } = 180;
    }
}
