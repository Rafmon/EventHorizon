using EventHorizon.src.Memory;
using Iot.Device.Mcp23xxx;
using System.Collections;
using System.Device.I2c;
using System.Runtime.InteropServices;
using static System.Formats.Asn1.AsnWriter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using EventHorizon.src.Util;

public class MemoryController
{
    private Dictionary<int, MemoryAddress> Addresses;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SettingsManager _settings;

    public MemoryController(IServiceScopeFactory scopeFactory, SettingsManager settings)
    {
        Console.WriteLine("Starting Memory");
        _scopeFactory = scopeFactory;
        _settings = settings;

        Addresses = new Dictionary<int, MemoryAddress>(128);

        if (!(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            (RuntimeInformation.ProcessArchitecture == Architecture.Arm ||
            RuntimeInformation.ProcessArchitecture == Architecture.Arm64)))
        {
            Console.WriteLine("GPIO/I2C not supported on this platform. Simulating devices...");
            _settings.SimulateI2CDevices = true;
        }
        genrateAddresses();

        Console.WriteLine("finished initializing Memory addresses");
    }

    private void genrateAddresses()
    {
        for (int i = 0; i < 8; i++)
        {
            try
            {
                IMemoryDevice dev;
                I2cConnectionSettings cset = new I2cConnectionSettings(1, 32 + i);
                I2cDevice idev = I2cDevice.Create(cset);
                Mcp23017 mcp23017 = new Mcp23017(idev);
                mcp23017.WriteByte(Register.IODIR, 0b0000_0000, Port.PortA);
                mcp23017.WriteByte(Register.IODIR, 0b0000_0000, Port.PortB);

                dev = new MemoryDevice(mcp23017);
                generateMemoryAddr(i, dev);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Creating dummy device for address " + (32 + i));
                generateMemoryAddr(i, new MemoryDeviceDummy());


            }
        }
    }

    private void generateMemoryAddr(int i, IMemoryDevice dev)
    {

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var addressRange = Enumerable.Range(16 * i, 16).ToList();
        var existingAddresses = dbContext.MemoryAddresses.AsNoTracking()
                                          .Where(ma => addressRange.Contains(ma.Address))
                                          .ToList();

        for (int a = 0; a < 16; a++)
        {
            int addr = (16 * i) + a;
            var existingAddress = existingAddresses.SingleOrDefault(ma => ma.Address == addr);
            Console.WriteLine($"Generating memory address {addr}");
            if (existingAddress != null)
            {
                existingAddress.Device = dev;
                if (string.IsNullOrWhiteSpace(existingAddress.Name))
                {
                    existingAddress.Name = $"Device {i + 1}";
                }
                Addresses.Add(addr, existingAddress);
            }
            else
            {
                var memoryAddress = new MemoryAddress(addr, dev);
                if (string.IsNullOrWhiteSpace(memoryAddress.Name))
                {
                    memoryAddress.Name = $"Device {i + 1}";
                }
                Addresses.Add(addr, memoryAddress);
            }
        }
    }

    public Dictionary<int, MemoryAddress> GetMemoryAddresses()
    {
        return Addresses;
    }

    public MemoryAddress GetMemoryAddressForIndex(int i)
    {
        return Addresses[i];
    }

    public void SaveMemoryAddress(int address)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var trackedAddress = dbContext.MemoryAddresses.SingleOrDefault(ma => ma.Address == address);
        if (trackedAddress == null)
        {
            dbContext.MemoryAddresses.Add(Addresses[address]);
        }
        else
        {
            dbContext.Entry(trackedAddress).CurrentValues.SetValues(Addresses[address]);
        }
        dbContext.SaveChanges();
    }
}
