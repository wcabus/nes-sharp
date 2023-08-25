namespace NesSharp.Core.Mappers;

public class Mapper000 : MapperBase
{
    public Mapper000(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }

    public override bool CpuMapRead(ushort address, out uint mappedAddress)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (uint)(address & (PrgBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (uint)(address & (PrgBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            mappedAddress = (uint)address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            if (ChrBanks == 0)
            {
                // Treat as RAM
                mappedAddress = (uint)address;
                return true;
            }
        }

        mappedAddress = 0;
        return false;
    }
}