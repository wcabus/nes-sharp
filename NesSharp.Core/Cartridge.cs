using NesSharp.Core.Mappers;

namespace NesSharp.Core;

public class Cartridge
{
    private readonly List<byte> _prgMemory = new();
    private readonly List<byte> _chrMemory = new();

    private int _mapperId = 0;
    private MirrorMode _mirrorMode = MirrorMode.Horizontal;
    private int _prgBanks = 0;
    private int _chrBanks = 0;
    
    private MapperBase? _mapper = null;

    public static async Task<Cartridge> FromFile(string fileName)
    {
        var cartridge = new Cartridge();

        await using var fs = File.OpenRead(fileName);
        using var br = new BinaryReader(fs);

        var header = new NesHeader
        {
            Name = br.ReadChars(4),
            PrgRomChunks = br.ReadByte(),
            ChrRomChunks = br.ReadByte(),
            Mapper1 = br.ReadByte(),
            Mapper2 = br.ReadByte(),
            PrgRamSize = br.ReadByte(),
            TvSystem1 = br.ReadByte(),
            TvSystem2 = br.ReadByte(),
            Unused = br.ReadBytes(5)
        };

        Nes2Header? header2 = null;

        // Discover File Format
        var fileType = 1;
        if ((header.Mapper1 & 0x01) != 0)
        {
            fileType = 2;
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            header2 = new Nes2Header
            {
                Name = br.ReadChars(4),
                PrgRomSizeLsb = br.ReadByte(),
                ChrRomSizeLsb = br.ReadByte(),
                Mapper1 = br.ReadByte(),
                Mapper2 = br.ReadByte(),
                Mapper3 = br.ReadByte(),
                PrgCharRomSizeMsb = br.ReadByte(),
                PrgRamSize = br.ReadByte(),
                ChrRamSize = br.ReadByte(),
                CpuPpuTiming = br.ReadByte(),
                TvSystem = br.ReadByte(),
                MiscRoms = br.ReadByte(),
                ExpansionDevice = br.ReadByte()
            };
        }

        if ((header.Mapper1 & 0x04) != 0)
        {
            // skip trainer data
            br.BaseStream.Seek(512, SeekOrigin.Current);
        }

        // Determine Mapper ID
        cartridge._mapperId = ((header.Mapper2 >>> 4) << 4) | (header.Mapper1 >>> 4);
        cartridge._mirrorMode = (header.Mapper1 & 0x01) != 0 ? MirrorMode.Vertical : MirrorMode.Horizontal;
               

        if (fileType == 0)
        {

        }

        if (fileType == 1)
        {
            cartridge._prgBanks = header.PrgRomChunks;
            cartridge._prgMemory.AddRange(br.ReadBytes(cartridge._prgBanks * 16 * 1024));

            cartridge._chrBanks = header.ChrRomChunks;
            
            // Create RAM if CHR ROM is not present
            cartridge._chrMemory.AddRange(cartridge._chrBanks == 0 ? new byte[8 * 1024] : br.ReadBytes(cartridge._chrBanks * 8 * 1024));
        }

        if (fileType == 2 && header2.HasValue)
        {
            // Determine the amount of PRG ROM banks based on the NES 2.0 header2
            var prgRomSize = ((header2.Value.PrgCharRomSizeMsb & 0x0f) << 8) | header2.Value.PrgRomSizeLsb;
            cartridge._prgBanks = prgRomSize;
            cartridge._prgMemory.AddRange(br.ReadBytes(cartridge._prgBanks * 16 * 1024));

            var chrRomSize = header2.Value.PrgCharRomSizeMsb << 8 | header2.Value.ChrRomSizeLsb;
            cartridge._chrBanks = chrRomSize;

            // Create RAM if CHR ROM is not present
            cartridge._chrMemory.AddRange(cartridge._chrBanks == 0 ? new byte[8 * 1024] : br.ReadBytes(cartridge._chrBanks * 8 * 1024));
        }

        switch (cartridge._mapperId)
        {
            case 0:
                cartridge._mapper = new Mapper000(cartridge._prgBanks, cartridge._chrBanks);
                break;
        }

        return cartridge;
    }

    public bool CpuRead(ushort address, out byte data)
    {
        if (_mapper is not null && _mapper.CpuMapRead(address, out var mappedAddress))
        {
            data = _prgMemory[(int)mappedAddress];
            return true;
        }

        data = 0;
        return false;
    }

    public bool CpuWrite(ushort address, byte data)
    {
        if (_mapper is not null && _mapper.CpuMapWrite(address, out var mappedAddress))
        {
            _prgMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

    public bool PpuRead(ushort address, out byte data)
    {
        if (_mapper is not null && _mapper.PpuMapRead(address, out var mappedAddress))
        {
            data = _chrMemory[(int)mappedAddress];
            return true;
        }

        data = 0;
        return false;
    }

    public bool PpuWrite(ushort address, byte data)
    {
        if (_mapper is not null && _mapper.PpuMapWrite(address, out var mappedAddress))
        {
            _chrMemory[(int)mappedAddress] = data;
            return true;
        }

        return false;
    }

    public void Reset()
    {
        _mapper?.Reset();
    }

    public MirrorMode MirrorMode => _mirrorMode;

    private readonly record struct NesHeader(char[] Name, byte PrgRomChunks, byte ChrRomChunks, byte Mapper1, byte Mapper2, byte PrgRamSize, byte TvSystem1, byte TvSystem2, byte[] Unused);
    private readonly record struct Nes2Header(char[] Name, byte PrgRomSizeLsb, byte ChrRomSizeLsb, byte Mapper1, byte Mapper2, byte Mapper3, byte PrgCharRomSizeMsb, byte PrgRamSize, byte ChrRamSize, byte CpuPpuTiming, byte TvSystem, byte MiscRoms, byte ExpansionDevice);
}