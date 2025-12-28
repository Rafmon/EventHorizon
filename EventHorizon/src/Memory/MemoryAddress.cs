using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventHorizon.src.Memory;

public class MemoryAddress

{
    [Key]
    public int Address { get; protected set; }
    public String Name { get; set; }
    
    [NotMapped]
    public IMemoryDevice Device;
    [NotMapped]
    public bool IsActive { get; set; }
    [NotMapped]
    public bool IsEditing { get; set; } = false;
    [NotMapped]
    public bool IsDeviceConnected { get; private set; }
    [NotMapped]
    public bool IsDeviceSimulated { get; private set; }

    /// <summary>
    /// constructor for EF not for normal use.
    /// </summary>
    protected MemoryAddress()
    {
    }

    public MemoryAddress(int addr, IMemoryDevice dev)
    {
        Address = addr;
        Device = dev;
        Name = Device.GetType().Name + "" + addr.ToString();
        IsActive = Device.GetIsActive(Address);
        RefreshDeviceStatus();
    }

    public void Update(bool isActive)
    {
        this.IsActive = isActive;
        Device.UpdatePin(Address, isActive);
        RefreshDeviceStatus();
    }

    public bool GetActivationStatus()
    {
        RefreshDeviceStatus();
        return Device.GetIsActive(Address);
    }

    public void RefreshDeviceStatus()
    {
        IsDeviceConnected = Device.IsConnected;
        IsDeviceSimulated = Device.IsSimulated;
    }

}

