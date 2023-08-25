using NesSharp.Core;

namespace NesSharp.Console;

internal class Program
{
    static async Task Main(string[] args)
    {
        var bus = new Bus();

        var cartridge = await Cartridge.FromFile("E:\\temp\\NES-ROMS\\nestest.nes");
        bus.InsertCartridge(cartridge);

        bus.Reset();

        while (true)
        {
            bus.Clock();
        }
    }
}