using System;
using System.Collections;
using System.Device.I2c;
using Iot.Device.Mcp23xxx;

namespace EventHorizon.src.Memory
{
    public class DynamicMemoryDevice : IMemoryDevice
    {
        private readonly int _busId;
        private readonly int _deviceAddress;
        private readonly bool _allowHardware;
        private readonly TimeSpan _reconnectDelay;
        private readonly BitArray _values = new BitArray(16);
        private DateTimeOffset _lastReconnectAttempt = DateTimeOffset.MinValue;
        private Mcp23017? _device;
        private I2cDevice? _i2cDevice;

        public DynamicMemoryDevice(int busId, int deviceAddress, bool allowHardware, TimeSpan? reconnectDelay = null)
        {
            _busId = busId;
            _deviceAddress = deviceAddress;
            _allowHardware = allowHardware;
            _reconnectDelay = reconnectDelay ?? TimeSpan.FromSeconds(3);

            if (_allowHardware)
            {
                TryInitializeDevice();
            }
        }

        public bool IsConnected { get; private set; }

        public bool IsSimulated => !IsConnected;

        public void UpdatePin(int id, bool isActive)
        {
            _values.Set(id % 16, isActive);

            if (!_allowHardware)
            {
                return;
            }

            if (!IsConnected)
            {
                TryReconnectIfDue();
                return;
            }

            try
            {
                WriteCurrentValues();
            }
            catch (Exception)
            {
                MarkDisconnected();
            }
        }

        public bool GetIsActive(int id)
        {
            return _values.Get(id % 16);
        }

        private void TryReconnectIfDue()
        {
            var now = DateTimeOffset.UtcNow;
            if (now - _lastReconnectAttempt < _reconnectDelay)
            {
                return;
            }

            _lastReconnectAttempt = now;
            if (TryInitializeDevice())
            {
                try
                {
                    WriteCurrentValues();
                }
                catch (Exception)
                {
                    MarkDisconnected();
                }
            }
        }

        private bool TryInitializeDevice()
        {
            try
            {
                DisposeDevice();
                var settings = new I2cConnectionSettings(_busId, _deviceAddress);
                _i2cDevice = I2cDevice.Create(settings);
                _device = new Mcp23017(_i2cDevice);
                _device.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA);
                _device.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);
                IsConnected = true;
                return true;
            }
            catch (Exception)
            {
                MarkDisconnected();
                return false;
            }
        }

        private void WriteCurrentValues()
        {
            if (_device == null)
            {
                throw new InvalidOperationException("Device is not initialized.");
            }

            _device.WriteUInt16(Register.GPIO, ConvertValuesToUshort());
        }

        private ushort ConvertValuesToUshort()
        {
            if (_values.Length > 16)
            {
                throw new ArgumentException("Argument length shall be at most 16 bits.");
            }

            int[] array = new int[1];
            _values.CopyTo(array, 0);
            return (ushort)array[0];
        }

        private void MarkDisconnected()
        {
            IsConnected = false;
            DisposeDevice();
        }

        private void DisposeDevice()
        {
            _device?.Dispose();
            _i2cDevice?.Dispose();
            _device = null;
            _i2cDevice = null;
        }
    }
}
