namespace NesSharp.Core.Mappers;

public sealed class Mapper002 : MapperBase
{
    private byte _prgBankSelectLo;
    private byte _prgBankSelectHi;

    public Mapper002(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }

    public override bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        if (address is >= 0x8000 and <= 0xBFFF)
        {
            mappedAddress = (uint)(_prgBankSelectLo * 0x4000 + (address & 0x3FFF));
            return true;
        }

        if (address >= 0xC000)
        {
            mappedAddress = (uint)(_prgBankSelectHi * 0x4000 + (address & 0x3FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress, byte data = 0)
    {
        if (address >= 0x8000)
        {
            _prgBankSelectLo = (byte)(data & 0x0F);
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        if (address <= 0x1FFF)
        {
            mappedAddress = address;
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
                mappedAddress = address;
                return true;
            }
        }

        mappedAddress = 0;
        return false;
    }

    public override void Reset()
    {
        _prgBankSelectLo = 0;
        _prgBankSelectHi = (byte)(PrgBanks - 1);
    }
}