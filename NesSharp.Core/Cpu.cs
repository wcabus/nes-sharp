namespace NesSharp.Core;

/// <summary>
/// Class representing the 6502 CPU
/// </summary>
public sealed class Cpu
{
    private byte _flags;
    private Bus? _bus;

    private byte _fetchedData;
    private ushort _absoluteAddress;
    private ushort _relativeAddress;
    private byte _opCode;
    private byte _cycles;

    private readonly Instruction[] _instructionSet;

    public Cpu()
    {
        _instructionSet = new Instruction[] {
            new("BRK", BRK, IMM, AddressingMode.IMM, 7), new("ORA", ORA, IZX, AddressingMode.IZX, 6), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 3), new("ORA", ORA, ZP0, AddressingMode.ZP0, 3), new("ASL", ASL, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("PHP", PHP, IMP, AddressingMode.IMP, 3), new("ORA", ORA, IMM, AddressingMode.IMM, 2), new("ASL", ASL, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", NOP, IMP, AddressingMode.IMP, 4), new("ORA", ORA, ABS, AddressingMode.ABS, 4), new("ASL", ASL, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BPL", BPL, REL, AddressingMode.REL, 2), new("ORA", ORA, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("ORA", ORA, ZPX, AddressingMode.ZPX, 4), new("ASL", ASL, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("CLC", CLC, IMP, AddressingMode.IMP, 2), new("ORA", ORA, ABY, AddressingMode.ABY, 4), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("ORA", ORA, ABX, AddressingMode.ABX, 4), new("ASL", ASL, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
            new("JSR", JSR, ABS, AddressingMode.ABS, 6), new("AND", AND, IZX, AddressingMode.IZX, 6), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("BIT", BIT, ZP0, AddressingMode.ZP0, 3), new("AND", AND, ZP0, AddressingMode.ZP0, 3), new("ROL", ROL, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("PLP", PLP, IMP, AddressingMode.IMP, 4), new("AND", AND, IMM, AddressingMode.IMM, 2), new("ROL", ROL, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("BIT", BIT, ABS, AddressingMode.ABS, 4), new("AND", AND, ABS, AddressingMode.ABS, 4), new("ROL", ROL, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BMI", BMI, REL, AddressingMode.REL, 2), new("AND", AND, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("AND", AND, ZPX, AddressingMode.ZPX, 4), new("ROL", ROL, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("SEC", SEC, IMP, AddressingMode.IMP, 2), new("AND", AND, ABY, AddressingMode.ABY, 4), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("AND", AND, ABX, AddressingMode.ABX, 4), new("ROL", ROL, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
            new("RTI", RTI, IMP, AddressingMode.IMP, 6), new("EOR", EOR, IZX, AddressingMode.IZX, 6), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 3), new("EOR", EOR, ZP0, AddressingMode.ZP0, 3), new("LSR", LSR, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("PHA", PHA, IMP, AddressingMode.IMP, 3), new("EOR", EOR, IMM, AddressingMode.IMM, 2), new("LSR", LSR, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("JMP", JMP, ABS, AddressingMode.ABS, 3), new("EOR", EOR, ABS, AddressingMode.ABS, 4), new("LSR", LSR, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BVC", BVC, REL, AddressingMode.REL, 2), new("EOR", EOR, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("EOR", EOR, ZPX, AddressingMode.ZPX, 4), new("LSR", LSR, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("CLI", CLI, IMP, AddressingMode.IMP, 2), new("EOR", EOR, ABY, AddressingMode.ABY, 4), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("EOR", EOR, ABX, AddressingMode.ABX, 4), new("LSR", LSR, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
            new("RTS", RTS, IMP, AddressingMode.IMP, 6), new("ADC", ADC, IZX, AddressingMode.IZX, 6), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 3), new("ADC", ADC, ZP0, AddressingMode.ZP0, 3), new("ROR", ROR, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("PLA", PLA, IMP, AddressingMode.IMP, 4), new("ADC", ADC, IMM, AddressingMode.IMM, 2), new("ROR", ROR, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("JMP", JMP, IND, AddressingMode.IND, 5), new("ADC", ADC, ABS, AddressingMode.ABS, 4), new("ROR", ROR, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BVS", BVS, REL, AddressingMode.REL, 2), new("ADC", ADC, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("ADC", ADC, ZPX, AddressingMode.ZPX, 4), new("ROR", ROR, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("SEI", SEI, IMP, AddressingMode.IMP, 2), new("ADC", ADC, ABY, AddressingMode.ABY, 4), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("ADC", ADC, ABX, AddressingMode.ABX, 4), new("ROR", ROR, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
            new("???", NOP, IMP, AddressingMode.IMP, 2), new("STA", STA, IZX, AddressingMode.IZX, 6), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 6), new("STY", STY, ZP0, AddressingMode.ZP0, 3), new("STA", STA, ZP0, AddressingMode.ZP0, 3), new("STX", STX, ZP0, AddressingMode.ZP0, 3), new("???", XXX, IMP, AddressingMode.IMP, 3), new("DEY", DEY, IMP, AddressingMode.IMP, 2), new("???", NOP, IMP, AddressingMode.IMP, 2), new("TXA", TXA, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("STY", STY, ABS, AddressingMode.ABS, 4), new("STA", STA, ABS, AddressingMode.ABS, 4), new("STX", STX, ABS, AddressingMode.ABS, 4), new("???", XXX, IMP, AddressingMode.IMP, 4),
            new("BCC", BCC, REL, AddressingMode.REL, 2), new("STA", STA, IZY, AddressingMode.IZY, 6), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 6), new("STY", STY, ZPX, AddressingMode.ZPX, 4), new("STA", STA, ZPX, AddressingMode.ZPX, 4), new("STX", STX, ZPY, AddressingMode.ZPX, 4), new("???", XXX, IMP, AddressingMode.IMP, 4), new("TYA", TYA, IMP, AddressingMode.IMP, 2), new("STA", STA, ABY, AddressingMode.ABY, 5), new("TXS", TXS, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 5), new("???", NOP, IMP, AddressingMode.IMP, 5), new("STA", STA, ABX, AddressingMode.ABX, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("???", XXX, IMP, AddressingMode.IMP, 5),
            new("LDY", LDY, IMM, AddressingMode.IMM, 2), new("LDA", LDA, IZX, AddressingMode.IZX, 6), new("LDX", LDX, IMM, AddressingMode.IMM, 2), new("???", XXX, IMP, AddressingMode.IMP, 6), new("LDY", LDY, ZP0, AddressingMode.ZP0, 3), new("LDA", LDA, ZP0, AddressingMode.ZP0, 3), new("LDX", LDX, ZP0, AddressingMode.ZP0, 3), new("???", XXX, IMP, AddressingMode.IMP, 3), new("TAY", TAY, IMP, AddressingMode.IMP, 2), new("LDA", LDA, IMM, AddressingMode.IMM, 2), new("TAX", TAX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("LDY", LDY, ABS, AddressingMode.ABS, 4), new("LDA", LDA, ABS, AddressingMode.ABS, 4), new("LDX", LDX, ABS, AddressingMode.ABS, 4), new("???", XXX, IMP, AddressingMode.IMP, 4),
            new("BCS", BCS, REL, AddressingMode.REL, 2), new("LDA", LDA, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 5), new("LDY", LDY, ZPX, AddressingMode.ZPX, 4), new("LDA", LDA, ZPX, AddressingMode.ZPX, 4), new("LDX", LDX, ZPY, AddressingMode.ZPX, 4), new("???", XXX, IMP, AddressingMode.IMP, 4), new("CLV", CLV, IMP, AddressingMode.IMP, 2), new("LDA", LDA, ABY, AddressingMode.ABY, 4), new("TSX", TSX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 4), new("LDY", LDY, ABX, AddressingMode.ABX, 4), new("LDA", LDA, ABX, AddressingMode.ABX, 4), new("LDX", LDX, ABY, AddressingMode.ABY, 4), new("???", XXX, IMP, AddressingMode.IMP, 4),
            new("CPY", CPY, IMM, AddressingMode.IMM, 2), new("CMP", CMP, IZX, AddressingMode.IZX, 6), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("CPY", CPY, ZP0, AddressingMode.ZP0, 3), new("CMP", CMP, ZP0, AddressingMode.ZP0, 3), new("DEC", DEC, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("INY", INY, IMP, AddressingMode.IMP, 2), new("CMP", CMP, IMM, AddressingMode.IMM, 2), new("DEX", DEX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 2), new("CPY", CPY, ABS, AddressingMode.ABS, 4), new("CMP", CMP, ABS, AddressingMode.ABS, 4), new("DEC", DEC, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BNE", BNE, REL, AddressingMode.REL, 2), new("CMP", CMP, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("CMP", CMP, ZPX, AddressingMode.ZPX, 4), new("DEC", DEC, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("CLD", CLD, IMP, AddressingMode.IMP, 2), new("CMP", CMP, ABY, AddressingMode.ABY, 4), new("NOP", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("CMP", CMP, ABX, AddressingMode.ABX, 4), new("DEC", DEC, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
            new("CPX", CPX, IMM, AddressingMode.IMM, 2), new("SBC", SBC, IZX, AddressingMode.IZX, 6), new("???", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("CPX", CPX, ZP0, AddressingMode.ZP0, 3), new("SBC", SBC, ZP0, AddressingMode.ZP0, 3), new("INC", INC, ZP0, AddressingMode.ZP0, 5), new("???", XXX, IMP, AddressingMode.IMP, 5), new("INX", INX, IMP, AddressingMode.IMP, 2), new("SBC", SBC, IMM, AddressingMode.IMM, 2), new("NOP", NOP, IMP, AddressingMode.IMP, 2), new("???", SBC, IMP, AddressingMode.IMP, 2), new("CPX", CPX, ABS, AddressingMode.ABS, 4), new("SBC", SBC, ABS, AddressingMode.ABS, 4), new("INC", INC, ABS, AddressingMode.ABS, 6), new("???", XXX, IMP, AddressingMode.IMP, 6),
            new("BEQ", BEQ, REL, AddressingMode.REL, 2), new("SBC", SBC, IZY, AddressingMode.IZY, 5), new("???", XXX, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 8), new("???", NOP, IMP, AddressingMode.IMP, 4), new("SBC", SBC, ZPX, AddressingMode.ZPX, 4), new("INC", INC, ZPX, AddressingMode.ZPX, 6), new("???", XXX, IMP, AddressingMode.IMP, 6), new("SED", SED, IMP, AddressingMode.IMP, 2), new("SBC", SBC, ABY, AddressingMode.ABY, 4), new("NOP", NOP, IMP, AddressingMode.IMP, 2), new("???", XXX, IMP, AddressingMode.IMP, 7), new("???", NOP, IMP, AddressingMode.IMP, 4), new("SBC", SBC, ABX, AddressingMode.ABX, 4), new("INC", INC, ABX, AddressingMode.ABX, 7), new("???", XXX, IMP, AddressingMode.IMP, 7),
        };
    }

    public void ConnectBus(Bus bus)
    {
        _bus = bus;
        _bus.Initialize();
    }
        
    // Registers

    /// <summary>
    /// Program Counter
    /// </summary>
    public ushort PC { get; set; }

    /// <summary>
    /// Stack Pointer
    /// </summary>
    public byte SP { get; set; }

    /// <summary>
    /// Accumulator
    /// </summary>
    public byte A { get; set; }

    /// <summary>
    /// Index Register X
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Index Register Y
    /// </summary>
    public byte Y { get; set; }

    // Flags
    public bool CarryFlag
    {
        get => (_flags & 0b00000001) == 0b00000001;
        set => _flags = (byte)(value ? _flags | 0b00000001 : _flags & 0b11111110);
    }

    public bool ZeroFlag
    {
        get => (_flags & 0b00000010) == 0b00000010;
        set => _flags = (byte)(value ? _flags | 0b00000010 : _flags & 0b11111101);
    }

    public bool InterruptDisableFlag
    {
        get => (_flags & 0b00000100) == 0b00000100;
        set => _flags = (byte)(value ? _flags | 0b00000100 : _flags & 0b11111011);
    }

    public bool DecimalModeFlag
    {
        get => (_flags & 0b00001000) == 0b00001000;
        set => _flags = (byte)(value ? _flags | 0b00001000 : _flags & 0b11110111);
    }

    public bool BreakCommandFlag
    {
        get => (_flags & 0b00010000) == 0b00010000;
        set => _flags = (byte)(value ? _flags | 0b00010000 : _flags & 0b11101111);
    }

    public bool UnusedFlag
    {
        get => (_flags & 0b00100000) == 0b00100000;
        set => _flags = (byte)(value ? _flags | 0b00100000 : _flags & 0b11011111);
    }

    public bool OverflowFlag
    {
        get => (_flags & 0b01000000) == 0b01000000;
        set => _flags = (byte)(value ? _flags | 0b01000000 : _flags & 0b10111111);
    }

    public bool NegativeFlag
    {
        get => (_flags & 0b10000000) == 0b10000000;
        set => _flags = (byte)(value ? _flags | 0b10000000 : _flags & 0b01111111);
    }

    private void WriteByte(ushort address, byte data)
    {
        _bus!.CpuWrite(address, data);
    }

    private byte ReadByte(ushort address)
    {
        return _bus!.CpuRead(address);
    }

    // Signals

    public void Reset()
    {
        _absoluteAddress = 0xFFFC;
        ushort lo = ReadByte(_absoluteAddress);
        ushort hi = ReadByte((ushort)(_absoluteAddress + 1));

        PC = (ushort)((hi << 8) | lo);

        A = X = Y = 0;
        SP = 0xFD;

        _flags = 0;
        UnusedFlag = true;

        _absoluteAddress = 0;
        _relativeAddress = 0;
        _fetchedData = 0;

        _cycles = 8;
    }

    /// <summary>
    /// Interrupt Request
    /// </summary>
    /// <remarks>These kinds of interrupts can be ignored if the interrupt enable flag is not set.</remarks>
    public void IRQ()
    {
        if (InterruptDisableFlag)
        {
            return;
        }

        WriteByte((ushort)(0x0100 + SP), (byte)((PC >>> 8) & 0x00FF));
        SP--;
        WriteByte((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
        SP--;

        BreakCommandFlag = false;
        UnusedFlag = true;
        InterruptDisableFlag = true;
        WriteByte((ushort)(0x0100 + SP), _flags);
        SP--;

        _absoluteAddress = 0xFFFE;
        ushort lo = ReadByte(_absoluteAddress);
        ushort hi = ReadByte((ushort)(_absoluteAddress + 1));
        PC = (ushort)((hi << 8) | lo);

        _cycles = 7;
    }

    /// <summary>
    /// Non-Maskable Interrupt
    /// </summary>
    /// <remarks>These interrupts cannot be ignored.</remarks>
    public void NMI()
    {
        WriteByte((ushort)(0x0100 + SP), (byte)((PC >>> 8) & 0x00FF));
        SP--;
        WriteByte((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
        SP--;

        BreakCommandFlag = false;
        UnusedFlag = true;
        InterruptDisableFlag = true;
        WriteByte((ushort)(0x0100 + SP), _flags);
        SP--;

        _absoluteAddress = 0xFFFA;
        ushort lo = ReadByte(_absoluteAddress);
        ushort hi = ReadByte((ushort)(_absoluteAddress + 1));
        PC = (ushort)((hi << 8) | lo);

        _cycles = 8;
    }

    public void Clock()
    {
        if (_cycles == 0)
        {
            _opCode = ReadByte(PC);
            UnusedFlag = true;
            PC++;

            var instruction = _instructionSet[_opCode];
            _cycles = instruction.Cycles;

            var additionalCycle1 = instruction.AddressingMode();
            var additionalCycle2 = instruction.OpCode();

            if (additionalCycle1 && additionalCycle2)
            {
                _cycles++;
            }
            
            UnusedFlag = true;
        }

        _cycles--;
    }

    // Addressing modes
    // ReSharper disable InconsistentNaming
    public bool IMP()
    {
        _fetchedData = A;
        return false;
    }

    public bool IMM()
    {
        _absoluteAddress = PC++;
        return false;
    }

    public bool ZP0()
    {
        _absoluteAddress = ReadByte(PC++);
        _absoluteAddress &= 0x00FF;
        return false;
    }

    public bool ZPX()
    {
        _absoluteAddress = (ushort)(ReadByte(PC++) + X);
        _absoluteAddress &= 0x00FF;
        return false;
    }

    public bool ZPY()
    {
        _absoluteAddress = (ushort)(ReadByte(PC++) + Y);
        _absoluteAddress &= 0x00FF;
        return false;
    }

    public bool REL()
    {
        _relativeAddress = ReadByte(PC++);

        if ((_relativeAddress & 0x80) != 0)
        {
            _relativeAddress |= 0xFF00;
        }

        return false;
    }

    public bool ABS()
    {
        ushort lo = ReadByte(PC++);
        ushort hi = ReadByte(PC++);

        _absoluteAddress = (ushort)((hi << 8) | lo);
        return false;
    }

    public bool ABX()
    {
        ushort lo = ReadByte(PC++);
        ushort hi = ReadByte(PC++);

        _absoluteAddress = (ushort)((hi << 8) | lo);
        _absoluteAddress += X;

        // Needs an additional clock cycle if the page boundary is crossed
        return (_absoluteAddress & 0xFF00) != (hi << 8);
    }

    public bool ABY()
    {
        ushort lo = ReadByte(PC++);
        ushort hi = ReadByte(PC++);

        _absoluteAddress = (ushort)((hi << 8) | lo);
        _absoluteAddress += Y;

        // Needs an additional clock cycle if the page boundary is crossed
        return (_absoluteAddress & 0xFF00) != (hi << 8);
    }

    public bool IND()
    {
        ushort ptr_lo = ReadByte(PC++);
        ushort ptr_hi = ReadByte(PC++);

        var ptr = (ushort)((ptr_hi << 8) | ptr_lo);
        if (ptr_lo == 0x00FF) // Simulate page boundary hardware bug
        {
            _absoluteAddress = (ushort)((ReadByte((ushort)(ptr & 0xFF00)) << 8) | ReadByte(ptr));
        }
        else // Behave normally
        {
            _absoluteAddress = (ushort)((ReadByte((ushort)(ptr + 1)) << 8) | ReadByte(ptr));
        }

        return false;
    }

    public bool IZX()
    {
        ushort t = ReadByte(PC++);
        ushort lo = ReadByte((ushort)((t + X) & 0x00FF));
        ushort hi = ReadByte((ushort)((t + X + 1) & 0x00FF));
        
        _absoluteAddress = (ushort)((hi << 8) | lo);
        
        return false;
    }

    public bool IZY()
    {
        ushort t = ReadByte(PC++);
        ushort lo = ReadByte((ushort)(t & 0x00FF));
        ushort hi = ReadByte((ushort)((t + 1) & 0x00FF));

        _absoluteAddress = (ushort)((hi << 8) | lo);
        _absoluteAddress += Y;

        // Needs an additional clock cycle if the page boundary is crossed
        return (_absoluteAddress & 0xFF00) != (hi << 8);
    }
    // ReSharper restore InconsistentNaming

    private byte Fetch()
    {
        if (_instructionSet[_opCode].Mode != AddressingMode.IMP)
        {
            _fetchedData = ReadByte(_absoluteAddress);
        }

        return _fetchedData;
    }

    // OpCodes
    // ReSharper disable InconsistentNaming
    public bool ADC() 
    {
        Fetch();
        
        var temp = (ushort)(A + _fetchedData + (CarryFlag ? 1 : 0));

        CarryFlag = temp > 255;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;

        OverflowFlag = (~(A ^ _fetchedData) & (A ^ temp) & 0x80) != 0;
        
        A = (byte)(temp & 0x00FF);
        return true;
    }

    public bool AND() 
    {
        Fetch();
        A &= _fetchedData;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return true;
    }

    public bool ASL() 
    {
        Fetch();
        var temp = (ushort)(_fetchedData << 1);
        CarryFlag = (temp & 0xFF00) > 0;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;

        if (_instructionSet[_opCode].Mode == AddressingMode.IMP)
        {
            A = (byte)(temp & 0x00FF);
        }
        else
        {
            WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        }

        return false;
    }
   
    public bool BCC() 
    {
        if (!CarryFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }
    
    public bool BCS()
    {
        if (CarryFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }
    
    public bool BEQ() 
    {
        if (ZeroFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool BIT() 
    {
        Fetch();
        var temp = (ushort)(A & _fetchedData);
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        OverflowFlag = (temp & 0x40) != 0;
        return false;
    }

    public bool BMI() 
    {
        if (NegativeFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool BNE() 
    {
        if (!ZeroFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool BPL() 
    {
        if (!NegativeFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool BRK()
    {
        PC++;

        InterruptDisableFlag = true;
        WriteByte((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
        SP--;
        WriteByte((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
        SP--;

        BreakCommandFlag = true;
        WriteByte((ushort)(0x0100 + SP), _flags);
        SP--;
        BreakCommandFlag = false;

        PC = (ushort)(ReadByte(0xFFFE) | (ReadByte(0xFFFF) << 8));
        return false;
    }

    public bool BVC()
    {
        if (!OverflowFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool BVS() 
    {
        if (OverflowFlag)
        {
            _cycles++;
            _absoluteAddress = (ushort)(PC + _relativeAddress);
            if ((_absoluteAddress & 0xFF00) != (PC & 0xFF00))
            {
                _cycles++;
            }

            PC = _absoluteAddress;
        }

        return false;
    }

    public bool CLC() 
    {
        CarryFlag = false;
        return false;
    }

    public bool CLD() 
    {
        DecimalModeFlag = false;
        return false;
    }

    public bool CLI()
    {
        InterruptDisableFlag = false;
        return false;
    }

    public bool CLV()
    {
        OverflowFlag = false;
        return false;
    }

    public bool CMP() 
    {
        Fetch();
        var temp = A - _fetchedData;
        CarryFlag = A >= _fetchedData;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        return true;
    }

    public bool CPX() 
    {
        Fetch();
        var temp = (ushort)(X - _fetchedData);
        CarryFlag = X >= _fetchedData;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        return false;
    }

    public bool CPY() 
    {
        Fetch();
        var temp = (ushort)(Y - _fetchedData);
        CarryFlag = Y >= _fetchedData;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        return false;
    }

    public bool DEC() 
    {
        Fetch();
        var temp = (ushort)(_fetchedData - 1);
        WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        return false;
    }

    public bool DEX() 
    {
        X--;
        ZeroFlag = X == 0;
        NegativeFlag = (X & 0x80) != 0;
        return false;
    }

    public bool DEY() 
    {
        Y--;
        ZeroFlag = Y == 0;
        NegativeFlag = (Y & 0x80) != 0;
        return false;
    }

    public bool EOR() 
    {
        Fetch();
        A ^= _fetchedData;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return true;
    }

    public bool INC()
    {
        Fetch();
        var temp = (ushort)(_fetchedData + 1);
        WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        return false;
    }

    public bool INX() 
    {
        X++;
        ZeroFlag = X == 0;
        NegativeFlag = (X & 0x80) != 0;
        return false;
    }

    public bool INY()
    {
        Y++;
        ZeroFlag = Y == 0;
        NegativeFlag = (Y & 0x80) != 0;
        return false;
    }

    public bool JMP() 
    {
        PC = _absoluteAddress;
        return false;
    }

    public bool JSR()
    {
        PC--;

        WriteByte((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
        SP--;
        WriteByte((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
        SP--;

        PC = _absoluteAddress;
        return false;
    }

    public bool LDA() 
    {
        Fetch();
        A = _fetchedData;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return true;
    }

    public bool LDX() 
    {
        Fetch();
        X = _fetchedData;
        ZeroFlag = X == 0;
        NegativeFlag = (X & 0x80) != 0;
        return true;
    }

    public bool LDY() 
    {
        Fetch();
        Y = _fetchedData;
        ZeroFlag = Y == 0;
        NegativeFlag = (Y & 0x80) != 0;
        return true;
    }

    public bool LSR() 
    {
        Fetch();
        CarryFlag = (_fetchedData & 0x0001) != 0;
        var temp = (ushort)(_fetchedData >>> 1);
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        if (_instructionSet[_opCode].Mode == AddressingMode.IMP)
        {
            A = (byte)(temp & 0x00FF);
        }
        else
        {
            WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        }

        return false;
    }

    public bool NOP() 
    {
        switch (_opCode)
        {
            case 0x1C:
            case 0x3C:
            case 0x5C:
            case 0x7C:
            case 0xDC:
            case 0xFC:
                return true;
        }

        return false;
    }

    public bool ORA() 
    {
        Fetch();
        A |= _fetchedData;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return true;
    }
    
    public bool PHA() 
    {
        WriteByte((ushort)(0x0100 + SP), A);
        SP--;
        return false;
    }
    
    public bool PHP() 
    {
        BreakCommandFlag = true;
        UnusedFlag = true;
        WriteByte((ushort)(0x0100 + SP), _flags);
        BreakCommandFlag = false;
        UnusedFlag = false;

        SP--;
        return false;
    }

    public bool PLA() 
    {
        SP++;
        A = ReadByte((ushort)(0x0100 + SP));
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return false;
    }

    public bool PLP() 
    {
        SP++;
        _flags = ReadByte((ushort)(0x0100 + SP));
        UnusedFlag = true;

        return false;
    }

    public bool ROL()
    {
        Fetch();
        var temp = (ushort)(_fetchedData << 1) | (CarryFlag ? 1 : 0);
        CarryFlag = (temp & 0xFF00) != 0;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        if (_instructionSet[_opCode].Mode == AddressingMode.IMP)
        {
            A = (byte)(temp & 0x00FF);
        }
        else
        {
            WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        }

        return false;
    }

    public bool ROR()
    {
        Fetch();
        var temp = (ushort)(CarryFlag ? 0x80 : 0) | (_fetchedData >>> 1);
        CarryFlag = (_fetchedData & 0x01) != 0;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;
        if (_instructionSet[_opCode].Mode == AddressingMode.IMP)
        {
            A = (byte)(temp & 0x00FF);
        }
        else
        {
            WriteByte(_absoluteAddress, (byte)(temp & 0x00FF));
        }

        return false;
    }
    
    public bool RTI()
    {
        SP++;
        _flags = ReadByte((ushort)(0x0100 + SP));
        BreakCommandFlag = false;
        UnusedFlag = false;
        
        SP++;
        PC = ReadByte((ushort)(0x0100 + SP));
        
        SP++;
        PC |= (ushort)(ReadByte((ushort)(0x0100 + SP)) << 8);
        
        return false;
    }
    
    public bool RTS() 
    {
        SP++;
        PC = ReadByte((ushort)(0x0100 + SP));
        
        SP++;
        PC |= (ushort)(ReadByte((ushort)(0x0100 + SP)) << 8);
        
        PC++;
        return false;
    }

    public bool SBC() 
    {
        Fetch();

        var value = (ushort)(_fetchedData ^ 0x00FF);
        var temp = (ushort)(A + value + (CarryFlag ? 1 : 0));

        CarryFlag = temp > 255;
        ZeroFlag = (temp & 0x00FF) == 0;
        NegativeFlag = (temp & 0x80) != 0;

        OverflowFlag = ((temp ^ A) & (temp ^ value) & 0x80) != 0;

        A = (byte)(temp & 0x00FF);
        return true;
    }

    public bool SEC() 
    {
        CarryFlag = true;
        return false;
    }

    public bool SED() 
    {
        DecimalModeFlag = true;
        return false;
    }

    public bool SEI() 
    {
        InterruptDisableFlag = true;
        return false;
    }

    public bool STA() 
    {
        WriteByte(_absoluteAddress, A);
        return false;
    }

    public bool STX() 
    {
        WriteByte(_absoluteAddress, X);
        return false;
    }

    public bool STY() 
    {
        WriteByte(_absoluteAddress, Y);
        return false;
    }

    public bool TAX() 
    {
        X = A;
        ZeroFlag = X == 0;
        NegativeFlag = (X & 0x80) != 0;
        return false;
    }

    public bool TAY() 
    {
        Y = A;
        ZeroFlag = Y == 0;
        NegativeFlag = (Y & 0x80) != 0;
        return false;
    }

    public bool TSX() 
    {
        X = SP;
        ZeroFlag = X == 0;
        NegativeFlag = (X & 0x80) != 0;
        return false;
    }

    public bool TXA() 
    {
        A = X;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return false;
    }

    public bool TXS() 
    {
        SP = X;
        return false;
    }

    public bool TYA() 
    {
        A = Y;
        ZeroFlag = A == 0;
        NegativeFlag = (A & 0x80) != 0;
        return false;
    }

    /// <summary>
    /// Invalid opcode
    /// </summary>
    public bool XXX() 
    {
        return false;
    }
    // ReSharper restore InconsistentNaming
    
    private readonly record struct Instruction(string Name, Func<bool> OpCode, Func<bool> AddressingMode, AddressingMode Mode, byte Cycles = 0);
    private enum AddressingMode
    {
        IMP,
        IMM,
        ZP0,
        ZPX,
        ZPY,
        REL,
        ABS,
        ABX,
        ABY,
        IND,
        IZX,
        IZY
    }
}