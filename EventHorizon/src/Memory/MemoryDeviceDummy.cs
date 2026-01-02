using System;
using Iot.Device.Mcp23xxx;
 //comment
namespace EventHorizon.src.Memory
{
    public class MemoryDeviceDummy : IMemoryDevice
    {
		private bool IsActive = false;
        private readonly bool _invertOutputs;

        public MemoryDeviceDummy(bool invertOutputs)
        {
            _invertOutputs = invertOutputs;
            IsActive = _invertOutputs ? true : false;
        }

		public bool GetIsActive(int ID)
		{
			return _invertOutputs ? !IsActive : IsActive;
        }

		public void UpdatePin(int Id, bool isActive)
        {
            IsActive = _invertOutputs ? !isActive : isActive;

			Console.WriteLine(Id + " is now: " + IsActive);
        }
    }
}
