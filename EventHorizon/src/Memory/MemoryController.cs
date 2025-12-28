using EventHorizon.src.Memory;
using Microsoft.EntityFrameworkCore;

public class MemoryController
{
    private Dictionary<int, MemoryAddress> Addresses;
    private readonly IServiceScopeFactory _scopeFactory;
    public MemoryController(IServiceScopeFactory scopeFactory)
    {
        Console.WriteLine("Starting Memory");
        _scopeFactory = scopeFactory;

        Addresses = new Dictionary<int, MemoryAddress>(128);
        genrateAddresses();

        Console.WriteLine("finished initializing Memory addresses");
    }

    private void genrateAddresses()
    {
        for (int i = 0; i < 8; i++)
        {
            IMemoryDevice dev = new DynamicMemoryDevice(1, 32 + i, false);
            generateMemoryAddr(i, dev);
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
