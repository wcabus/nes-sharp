namespace NesSharp.Core.Mappers;

public sealed class Mapper066 : MapperBase
{
    private byte _prgBankSelect;
    private byte _chrBankSelect;

    public Mapper066(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }

    public override bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        if (address >= 0x8000)
        {
            mappedAddress = (uint)(_prgBankSelect * 0x8000 + (address & 0x7FFF));
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
            _prgBankSelect = (byte)((data & 0x30) >>> 4);
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
        _prgBankSelect = 0;
        _chrBankSelect = 0;
    }
}