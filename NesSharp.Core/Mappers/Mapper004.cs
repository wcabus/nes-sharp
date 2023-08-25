namespace NesSharp.Core.Mappers;

public sealed class Mapper004 : MapperBase
{
    private byte _targetRegister;
    private bool _prgBankMode;
    private bool _chrInversion;

    private uint[] _register = new uint[8];
    private uint[] _chrBank = new uint[8];
    private uint[] _prgBank = new uint[4];

    private bool _irqEnable;
    private bool _irqActive;
    private bool _irqUpdate;
    private ushort _irqCounter;
    private ushort _irqReload;

    private MirrorMode _mirrorMode = MirrorMode.Horizontal;
    private readonly byte[] _staticRAM = new byte[32 * 1024];

    // TODO: Allow saving of SRAM for cartridges using this mapper

    public Mapper004(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }       

    public override bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        mappedAddress = 0;

        if (address is >= 0x6000 and <= 0x7FFF)
        {
            // Write is to static ram on cartridge
            mappedAddress = 0xFFFFFFFF;

            // Write data to RAM
            data = _staticRAM[address & 0x1FFF];

            // Signal mapper has handled request
            return true;
        }


        if (address is >= 0x8000 and <= 0x9FFF)
        {
            mappedAddress = (uint)(_prgBank[0] + (address & 0x1FFF));
            return true;
        }

        if (address is >= 0xA000 and <= 0xBFFF)
        {
            mappedAddress = (uint)(_prgBank[1] + (address & 0x1FFF));
            return true;
        }

        if (address is >= 0xC000 and <= 0xDFFF)
        {
            mappedAddress = (uint)(_prgBank[2] + (address & 0x1FFF));
            return true;
        }

        if (address >= 0xE000)
        {
            mappedAddress = (uint)(_prgBank[3] + (address & 0x1FFF));
            return true;
        }

        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress, byte data = 0)
    {
        mappedAddress = 0;

        if (address is >= 0x6000 and <= 0x7FFF)
        {
            // Write is to static ram on cartridge
            mappedAddress = 0xFFFFFFFF;

            // Write data to RAM
            _staticRAM[address & 0x1FFF] = data;

            // Signal mapper has handled request
            return true;
        }

        if (address is >= 0x8000 and <= 0x9FFF)
        {
            // Bank Select
            if ((address & 0x0001) == 0)
            {
                _targetRegister = (byte)(data & 0x07);
                _prgBankMode = (data & 0x40) != 0;
                _chrInversion = (data & 0x80) != 0;
            }
            else
            {
                // Update target register
                _register[_targetRegister] = data;

                // Update Pointer Table
                if (_chrInversion)
                {
                    _chrBank[0] = _register[2] * 0x0400;
                    _chrBank[1] = _register[3] * 0x0400;
                    _chrBank[2] = _register[4] * 0x0400;
                    _chrBank[3] = _register[5] * 0x0400;
                    _chrBank[4] = (_register[0] & 0xFE) * 0x0400;
                    _chrBank[5] = _register[0] * 0x0400 + 0x0400;
                    _chrBank[6] = (_register[1] & 0xFE) * 0x0400;
                    _chrBank[7] = _register[1] * 0x0400 + 0x0400;
                }
                else
                {
                    _chrBank[0] = (_register[0] & 0xFE) * 0x0400;
                    _chrBank[1] = _register[0] * 0x0400 + 0x0400;
                    _chrBank[2] = (_register[1] & 0xFE) * 0x0400;
                    _chrBank[3] = _register[1] * 0x0400 + 0x0400;
                    _chrBank[4] = _register[2] * 0x0400;
                    _chrBank[5] = _register[3] * 0x0400;
                    _chrBank[6] = _register[4] * 0x0400;
                    _chrBank[7] = _register[5] * 0x0400;
                }

                if (_prgBankMode)
                {
                    _prgBank[2] = (_register[6] & 0x3F) * 0x2000;
                    _prgBank[0] = (uint)((PrgBanks * 2 - 2) * 0x2000);
                }
                else
                {
                    _prgBank[0] = (_register[6] & 0x3F) * 0x2000;
                    _prgBank[2] = (uint)((PrgBanks * 2 - 2) * 0x2000);
                }

                _prgBank[1] = (_register[7] & 0x3F) * 0x2000;
                _prgBank[3] = (uint)((PrgBanks * 2 - 1) * 0x2000);

            }

            return false;
        }

        if (address is >= 0xA000 and <= 0xBFFF)
        {
            if ((address & 0x0001) == 0)
            {
                // Mirroring
                _mirrorMode = (data & 0x01) != 0 ? MirrorMode.Horizontal : MirrorMode.Vertical;
            }
            else
            {
                // PRG Ram Protect
                // TODO:
            }
            return false;
        }

        if (address is >= 0xC000 and <= 0xDFFF)
        {
            if ((address & 0x0001) == 0)
            {
                _irqReload = data;
            }
            else
            {
                _irqCounter = 0x0000;
            }
            return false;
        }

        if (address >= 0xE000)
        {
            if ((address & 0x0001) == 0)
            {
                _irqEnable = false;
                _irqActive = false;
            }
            else
            {
                _irqEnable = true;
            }
            return false;
        }

        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address <= 0x03FF)
        {
            mappedAddress = (uint)(_chrBank[0] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x0400 and <= 0x07FF)
        {
            mappedAddress = (uint)(_chrBank[1] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x0800 and <= 0x0BFF)
        {
            mappedAddress = (uint)(_chrBank[2] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x0C00 and <= 0x0FFF)
        {
            mappedAddress = (uint)(_chrBank[3] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x1000 and <= 0x13FF)
        {
            mappedAddress = (uint)(_chrBank[4] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x1400 and <= 0x17FF)
        {
            mappedAddress = (uint)(_chrBank[5] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x1800 and <= 0x1BFF)
        {
            mappedAddress = (uint)(_chrBank[6] + (address & 0x03FF));
            return true;
        }

        if (address is >= 0x1C00 and <= 0x1FFF)
        {
            mappedAddress = (uint)(_chrBank[7] + (address & 0x03FF));
            return true;
        }

        return false;
    }

    public override bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;
        return false;
    }

    public override void Reset()
    {
        _targetRegister = 0x00;
        _prgBankMode = false;
        _chrInversion = false;
        _mirrorMode = MirrorMode.Horizontal;

        _irqActive = false;
        _irqEnable = false;
        _irqUpdate = false;
        _irqCounter = 0x0000;
        _irqReload = 0x0000;

        Array.Clear(_prgBank, 0, _prgBank.Length);
        Array.Clear(_chrBank, 0, _chrBank.Length);
        Array.Clear(_register, 0, _register.Length);

        _prgBank[0] = 0 * 0x2000;
        _prgBank[1] = 1 * 0x2000;
        _prgBank[2] = (uint)((PrgBanks * 2 - 2) * 0x2000);
        _prgBank[3] = (uint)((PrgBanks * 2 - 1) * 0x2000);
    }

    public override bool IRQState()
    {
        return _irqActive;
    }

    public override void IRQClear()
    {
        _irqActive = false;
    }

    public override void ScanLine()
    {
        if (_irqCounter == 0)
        {
            _irqCounter = _irqReload;
        }
        else
        {
            _irqCounter--;
        }

        if (_irqCounter == 0 && _irqEnable)
        {
            _irqActive = true;
        }
    }

    public override MirrorMode Mirror()
    {
        return _mirrorMode;
    }
}