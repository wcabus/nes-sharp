namespace NesSharp.Core.Mappers;

public sealed class Mapper001 : MapperBase
{
    private byte _chrBankSelect4Lo;
    private byte _chrBankSelect4Hi;
    private byte _chrBankSelect8;

    private byte _prgBankSelect16Lo;
    private byte _prgBankSelect16Hi;
    private byte _prgBankSelect32;

    private byte _loadRegister;
    private byte _loadRegisterCount;
    private byte _controlRegister;

    private MirrorMode _mirrorMode = MirrorMode.Horizontal;
    private readonly byte[] _staticRAM = new byte[32 * 1024];

    // TODO: Allow saving of SRAM for cartridges using this mapper

    public Mapper001(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
    {
    }

    public override bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        if (address is >= 0x6000 and <= 0x7FFF)
        {
            // Read from static ram on cartridge
            mappedAddress = 0xFFFFFFFF;

            // Read data from RAM
            data = _staticRAM[address & 0x1FFF];

            // Signal mapper has handled request
            return true;
        }

        if (address >= 0x8000)
        {
            if ((_controlRegister & 0b01000) != 0)
            {
                // 16K Mode
                if (address is >= 0x8000 and <= 0xBFFF)
                {
                    mappedAddress = (uint)(_prgBankSelect16Lo * 0x4000 + (address & 0x3FFF));
                    return true;
                }

                mappedAddress = (uint)(_prgBankSelect16Hi * 0x4000 + (address & 0x3FFF));
                return true;
            }

            // 32K Mode
            mappedAddress = (uint)(_prgBankSelect32 * 0x8000 + (address & 0x7FFF));
            return true;
        }

        mappedAddress = 0;
        return false;
    }

    public override bool CpuMapWrite(ushort address, out uint mappedAddress, byte data = 0)
    {
        if (address is >= 0x6000 and <= 0x7FFF)
        {
            // Write is to static ram on cartridge
            mappedAddress = 0xFFFFFFFF;

            // Write data to RAM
            _staticRAM[address & 0x1FFF] = data;

            // Signal mapper has handled request
            return true;
        }

        if (address >= 0x8000)
        {
            if ((data & 0x80) != 0)
            {
                // MSB is set, so reset serial loading
                _loadRegister = 0x00;
                _loadRegisterCount = 0;
                _controlRegister |= 0x0C;
            }
            else
            {
                // Load data in serially into load register
                // It arrives LSB first, so implant this at
                // bit 5. After 5 writes, the register is ready
                _loadRegister >>>= 1;
                _loadRegister |= (byte)((data & 0x01) << 4);
                _loadRegisterCount++;

                if (_loadRegisterCount == 5)
                {
                    // Get Mapper Target Register, by examining
                    // bits 13 & 14 of the address
                    var targetRegister = (byte)((address >>> 13) & 0x03);

                    if (targetRegister == 0) // 0x8000 - 0x9FFF
                    {
                        // Set Control Register
                        _controlRegister = (byte)(_loadRegister & 0x1F);

                        switch (_controlRegister & 0x03)
                        {
                            case 0: _mirrorMode = MirrorMode.OneScreenLow; break;
                            case 1: _mirrorMode = MirrorMode.OneScreenHigh; break;
                            case 2: _mirrorMode = MirrorMode.Vertical; break;
                            case 3: _mirrorMode = MirrorMode.Horizontal; break;
                        }
                    }
                    else if (targetRegister == 1) // 0xA000 - 0xBFFF
                    {
                        // Set CHR Bank Lo
                        if ((_controlRegister & 0b10000) != 0)
                        {
                            // 4K CHR Bank at PPU 0x0000
                            _chrBankSelect4Lo = (byte)(_loadRegister & 0x1F);
                        }
                        else
                        {
                            // 8K CHR Bank at PPU 0x0000
                            _chrBankSelect8 = (byte)(_loadRegister & 0x1E);
                        }
                    }
                    else if (targetRegister == 2) // 0xC000 - 0xDFFF
                    {
                        // Set CHR Bank Hi
                        if ((_controlRegister & 0b10000) != 0)
                        {
                            // 4K CHR Bank at PPU 0x1000
                            _chrBankSelect4Hi = (byte)(_loadRegister & 0x1F);
                        }
                    }
                    else if (targetRegister == 3) // 0xE000 - 0xFFFF
                    {
                        // Configure PRG Banks
                        var prgMode = (byte)((_controlRegister >>> 2) & 0x03);

                        if (prgMode is 0 or 1)
                        {
                            // Set 32K PRG Bank at CPU 0x8000
                            _prgBankSelect32 = (byte)((_loadRegister & 0x0E) >>> 1);
                        }
                        else if (prgMode == 2)
                        {
                            // Fix 16KB PRG Bank at CPU 0x8000 to First Bank
                            _prgBankSelect16Lo = 0;
                            // Set 16KB PRG Bank at CPU 0xC000
                            _prgBankSelect16Hi = (byte)(_loadRegister & 0x0F);
                        }
                        else if (prgMode == 3)
                        {
                            // Set 16KB PRG Bank at CPU 0x8000
                            _prgBankSelect16Lo = (byte)(_loadRegister & 0x0F);
                            // Fix 16KB PRG Bank at CPU 0xC000 to Last Bank
                            _prgBankSelect16Hi = (byte)(PrgBanks - 1);
                        }
                    }

                    // 5 bits were written, and decoded, so
                    // reset load register
                    _loadRegister = 0x00;
                    _loadRegisterCount = 0;
                }
            }
        }

        // Mapper has handled write, but do not update ROMs
        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        if (address < 0x2000)
        {
            if (ChrBanks == 0)
            {
                mappedAddress = address;
                return true;
            }
            else
            {
                if ((_controlRegister & 0b10000) != 0)
                {
                    // 4K CHR Bank Mode
                    if (address <= 0x0FFF)
                    {
                        mappedAddress = (uint)(_chrBankSelect4Lo * 0x1000 + (address & 0x0FFF));
                        return true;
                    }

                    mappedAddress = (uint)(_chrBankSelect4Hi * 0x1000 + (address & 0x0FFF));
                    return true;
                }
            

                // 8K CHR Bank Mode
                mappedAddress = (uint)(_chrBankSelect8 * 0x2000 + (address & 0x1FFF));
                return true;
            }
        }

        mappedAddress = 0;
        return false;
    }

    public override bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;

        if (address < 0x2000)
        {
            if (ChrBanks == 0)
            {
                mappedAddress = address;
                return true;
            }
            
            return true;
        }
        
        return false;
    }

    public override void Reset()
    {
        _controlRegister = 0x1C;
        _loadRegister = 0x00;
        _loadRegisterCount = 0x00;

        _chrBankSelect4Lo = 0;
        _chrBankSelect4Hi = 0;
        _chrBankSelect8 = 0;

        _prgBankSelect32 = 0;
        _prgBankSelect16Lo = 0;
        _prgBankSelect16Hi = (byte)(PrgBanks - 1);
    }

    public override MirrorMode Mirror()
    {
        return _mirrorMode;
    }
}