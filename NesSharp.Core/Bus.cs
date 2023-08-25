namespace NesSharp.Core;

public sealed class Bus
{
    private readonly Cpu _cpu = new();
    private readonly Ppu _ppu = new();
    private Cartridge? _cartridge;
    
    private readonly byte[] _cpuRam = new byte[2 * 1024];
    
    private byte _dmaPage;
    private byte _dmaAddress;
    private byte _dmaData;
    private bool _dmaTransfer;
    private bool _dmaDummy = true;

    private readonly byte[] _controller = new byte[2];
    private readonly byte[] _controllerState = new byte[2];

    private uint _systemClockCounter;

    public Bus()
    {
        _cpu.ConnectBus(this);
    }
        
    public void Initialize()
    {
        Array.Clear(_cpuRam, 0, 2 * 1024);
    }

    public byte CpuRead(ushort address, bool readOnly = false)
    {
        if (_cartridge != null && _cartridge.CpuRead(address, out var data))
        {
            return data;
        }

        switch (address)
        {
            case <= 0x1FFF:
                return _cpuRam[address & 0x07FF];
            case <= 0x3FFF:
                return _ppu.CpuRead((ushort)(address & 0x0007), readOnly);
            case >= 0x4016 and <= 0x4017:
                data = (_controllerState[address & 0x0001] & 0x80) > 0 ? (byte)1 : (byte)0;
                _controllerState[address & 0x0001] <<= 1;
                return data;
            //case >= 0x8000:
            //    return _cartridge!.CpuRead(address, readOnly);
            default:
                return 0;
        }
    }

    public void CpuWrite(ushort address, byte data)
    {
        if (_cartridge != null && _cartridge.CpuWrite(address, data))
        {
            return;
        }

        if (address <= 0x1FFF)
        {
            _cpuRam[address & 0x07FF] = data;
        }
        else if (address <= 0x3FFF)
        {
            _ppu.CpuWrite((ushort)(address & 0x0007), data);
        }
        else if (address == 0x4014)
        {
            _dmaPage = data;
            _dmaAddress = 0x00;
            _dmaTransfer = true;
        }
        else if (address is >= 0x4016 and <= 0x4017)
        {
            _controllerState[address & 0x0001] = _controller[address & 0x0001];
        }
        //else if (address >= 0x8000)
        //{
        //    _cpu.CpuWrite(address, data);
        //}
    }

    public void InsertCartridge(Cartridge cartridge)
    {
        _cartridge = cartridge;
        _ppu.ConnectCartridge(cartridge);
    }

    public void Reset()
    {
        _cartridge?.Reset();
        _cpu.Reset();
        _ppu.Reset();
        _systemClockCounter = 0;
    }

    public void Stop()
    {
        _cpu.Stop();
        _ppu.Stop();
    }

    public void Clock()
    {
        _ppu.Clock();

        if (_systemClockCounter % 3 == 0)
        {
            if (_dmaTransfer)
            {
                if (_dmaDummy)
                {
                    if (_systemClockCounter % 2 == 1)
                    {
                        _dmaDummy = false;
                    }
                }
                else
                {
                    if (_systemClockCounter % 2 == 0)
                    {
                        _dmaData = CpuRead((ushort)((_dmaPage << 8) | _dmaAddress));
                    }
                    else
                    {
                        _ppu.OAMData[_dmaAddress] = _dmaData;
                        _dmaAddress++;
                        if (_dmaAddress == 0x00)
                        {
                            _dmaTransfer = false;
                            _dmaDummy = true;
                        }
                    }
                }
            }
            else
            {
                _cpu.Clock();
            }
        }
        
        if (_ppu.IsNMISet)
        {
            _ppu.IsNMISet = false;
            _cpu.NMI();
        }
        
        _systemClockCounter++;
    }

    public void SetControllerState(int player, bool up, bool down, bool left, bool right, bool start, bool select, bool btnA, bool btnB)
    {
        byte data = 0;
        data |= (byte)(btnA ? 0x80 : 0);
        data |= (byte)(btnB ? 0x40 : 0);
        data |= (byte)(select ? 0x20 : 0);
        data |= (byte)(start ? 0x10 : 0);
        data |= (byte)(up ? 0x08 : 0);
        data |= (byte)(down ? 0x04 : 0);
        data |= (byte)(left ? 0x02 : 0);
        data |= (byte)(right ? 0x01 : 0);

        _controller[player] = data;
    }

    public Ppu Ppu => _ppu;
}