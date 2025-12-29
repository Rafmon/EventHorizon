using System;
using System.Collections;
using Iot.Device.Mcp23xxx;

namespace EventHorizon.src.Memory
{
    public class MemoryDevice : IMemoryDevice
    {
        private Mcp23017 Dev;
        private BitArray Values = new BitArray(16);
        private readonly bool _invertOutputs;

        public MemoryDevice(Mcp23017 device, bool invertOutputs)
        {
            Dev = device;
            _invertOutputs = invertOutputs;
            Values.SetAll(GetHardwareState(false));
           
        }
        public void UpdatePin(int Id, bool IsActive)
        {
            Values.Set(Id % 16, GetHardwareState(IsActive));
            updatePins();
        }

        private void updatePins()
        {
            Dev.WriteUInt16(Register.GPIO, convertValuesToUshort());
        }

        private ushort convertValuesToUshort()
        {
            if (Values.Length > 16)
                throw new ArgumentException("Argument length shall be at most 16 bits.");

            int[] array = new int[1];
            Values.CopyTo(array, 0);
            return (UInt16)array[0];
        }

		public bool GetIsActive(int ID)
		{
            return GetLogicalState(Values.Get(ID % 16));

		}

        private bool GetHardwareState(bool isActive)
        {
            return _invertOutputs ? !isActive : isActive;
        }

        private bool GetLogicalState(bool isActive)
        {
            return _invertOutputs ? !isActive : isActive;
        }
	}
}
