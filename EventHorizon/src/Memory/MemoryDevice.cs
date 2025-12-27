using System;
using System.Collections;
using Iot.Device.Mcp23xxx;

namespace EventHorizon.src.Memory
{
    public class MemoryDevice : IMemoryDevice
    {
        private Mcp23017 Dev;
        private BitArray Values = new BitArray(16);

        public MemoryDevice(Mcp23017 device)
        {
            Dev = device;
            Values.SetAll(false);
           
        }
        public void UpdatePin(int Id, bool IsActive)
        {
            Values.Set(Id % 16, IsActive);
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
            return Values.Get(ID % 16);

		}

        public bool IsConnected => true;

        public bool IsSimulated => false;
	}
}
