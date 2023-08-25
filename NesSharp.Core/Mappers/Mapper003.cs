namespace NesSharp.Core.Mappers;

public sealed class Mapper003 : MapperBase
{
    private byte _chrBankSelect;

    public Mapper003(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }

    public override bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (uint)(address & (PrgBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress, byte data = 0)
    {
        mappedAddress = 0;

        if (address >= 0x8000)
        {
            _chrBankSelect = (byte)(data & 0x03);
            mappedAddress = address;
        }

        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            mappedAddress = (uint)(_chrBankSelect * 0x2000 + address);
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;
        return false;
    }

    public override void Reset()
    {
        _chrBankSelect = 0;
    }
}