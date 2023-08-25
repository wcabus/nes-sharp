namespace NesSharp.Core;

public sealed class Ppu
{
    private int _cycle;
    private int _scanLine;

    private readonly byte[][] _nameTable = new byte[2][];
    private readonly byte[] _palette = new byte[32];
    private readonly byte[][] _patternTable = new byte[2][];

    private byte _oamAddress;
    private readonly byte[] _oamData = new byte[64*4];
    
    private readonly Sprite[] _spriteScanLine = new Sprite[8];
    private byte _spriteCount;
    private readonly byte[] _spriteShifterPatternLo = new byte[8];
    private readonly byte[] _spriteShifterPatternHi = new byte[8];

    private bool _spriteZeroHitPossible;
    private bool _spriteZeroBeingRendered;

    private readonly PpuStatusRegister _status = new();
    private readonly PpuMaskRegister _mask = new();
    private readonly PpuControlRegister _control = new();

    private byte _addressLatch;
    private byte _ppuDataBuffer;

    private readonly LoopyRegister _vramAddress = new();
    private readonly LoopyRegister _tramAddress = new();
    private byte _fineX;

    private byte _bgNextTileId;
    private byte _bgNextTileAttribute;
    private byte _bgNextTileLsb;
    private byte _bgNextTileMsb;

    private ushort _bgShifterPatternLo;
    private ushort _bgShifterPatternHi;
    private ushort _bgShifterAttributeLo;
    private ushort _bgShifterAttributeHi;

    private readonly RgbColor[] _screen = new RgbColor[256 * 240];

    private Cartridge? _cartridge;
    private bool _isHalted;

    private static readonly RgbColor[] PalScreen =
    {
        new(0x62, 0x62, 0x62), new(0x00, 0x1F, 0xB2), new(0x24, 0x04, 0xC8), new(0x52, 0x00, 0xB2), new(0x73, 0x00, 0x76), new(0x80, 0x00, 0x24), new(0x73, 0x0B, 0x00), new(0x52, 0x28, 0x00), new(0x24, 0x44, 0x00), new(0x00, 0x57, 0x00), new(0x00, 0x5C, 0x00), new(0x00, 0x53, 0x24), new(0x00, 0x3C, 0x76), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00),
        new(0xAB, 0xAB, 0xAB), new(0x0D, 0x57, 0xFF), new(0x4B, 0x30, 0xFF), new(0x8A, 0x13, 0xFF), new(0xBC, 0x08, 0xD6), new(0xD2, 0x12, 0x69), new(0xC7, 0x2E, 0x00), new(0x9D, 0x54, 0x00), new(0x60, 0x7B, 0x00), new(0x20, 0x98, 0x00), new(0x00, 0xA3, 0x00), new(0x00, 0x99, 0x42), new(0x00, 0x7D, 0xB4), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00),
        new(0xFF, 0xFF, 0xFF), new(0x53, 0xAE, 0xFF), new(0x90, 0x85, 0xFF), new(0xD3, 0x65, 0xFF), new(0xFF, 0x57, 0xFF), new(0xFF, 0x5D, 0xCF), new(0xFF, 0x77, 0x57), new(0xFA, 0x9E, 0x00), new(0xBD, 0xC7, 0x00), new(0x7A, 0xE7, 0x00), new(0x43, 0xF6, 0x11), new(0x26, 0xEF, 0x7E), new(0x2C, 0xD5, 0xF6), new(0x4E, 0x4E, 0x4E), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00),
        new(0xFF, 0xFF, 0xFF), new(0xB6, 0xE1, 0xFF), new(0xCE, 0xD1, 0xFF), new(0xE9, 0xC3, 0xFF), new(0xFF, 0xBC, 0xFF), new(0xFF, 0xBD, 0xF4), new(0xFF, 0xC6, 0xC3), new(0xFF, 0xD5, 0x9A), new(0xE9, 0xE6, 0x81), new(0xCE, 0xF4, 0x81), new(0xB6, 0xFB, 0x9A), new(0xA9, 0xFA, 0xC3), new(0xA9, 0xF0, 0xF4), new(0xB8, 0xB8, 0xB8), new(0x00, 0x00, 0x00), new(0x00, 0x00, 0x00)
    };    

    public Ppu()
    {
        _nameTable[0] = new byte[1024];
        _nameTable[1] = new byte[1024];

        _patternTable[0] = new byte[4096];
        _patternTable[1] = new byte[4096];
    }

    public bool IsNMISet { get; set; }
    public byte[] OAMData => _oamData;

    public void ConnectCartridge(Cartridge cartridge)
    {
        _cartridge = cartridge;
    }

    public void Reset()
    {
        _cycle = 0;
        _scanLine = -1;

        Array.Clear(_nameTable[0], 0, 1024);
        Array.Clear(_nameTable[1], 0, 1024);
        Array.Clear(_palette, 0, 32);
        Array.Clear(_patternTable[0], 0, 4096);
        Array.Clear(_patternTable[1], 0, 4096);

        _oamAddress = 0;
        Array.Clear(_oamData, 0, 64 * 4);

        Array.Clear(_spriteScanLine, 0, 8);
        _spriteCount = 0;
        Array.Clear(_spriteShifterPatternLo, 0, 8);
        Array.Clear(_spriteShifterPatternHi, 0, 8);

        _spriteZeroHitPossible = false;
        _spriteZeroBeingRendered = false;

        _status.Register = 0;
        _mask.Register = 0;
        _control.Register = 0;

        _addressLatch = 0;
        _ppuDataBuffer = 0;

        _vramAddress.Register = 0;
        _tramAddress.Register = 0;
        _fineX = 0;

        _bgNextTileId = 0;
        _bgNextTileAttribute = 0;
        _bgNextTileLsb = 0;
        _bgNextTileMsb = 0;
        _bgShifterPatternLo = 0;
        _bgShifterPatternHi = 0;
        _bgShifterAttributeLo = 0;
        _bgShifterAttributeHi = 0;
        
        Array.Clear(_screen, 0, 256 * 240);

        IsNMISet = false;
        FrameComplete = false;

        _isHalted = false;
    }

    public void Stop()
    {
        _isHalted = true;
    }

    public void Clock()
    {
        if (_isHalted)
        {
            return;
        }

        void IncrementScrollX()
        {
            if (_mask.ShowBackground || _mask.ShowSprites)
            {
                if (_vramAddress.CoarseX == 31)
                {
                    _vramAddress.CoarseX = 0;
                    _vramAddress.NameTableX = !_vramAddress.NameTableX;
                }
                else
                {
                    _vramAddress.CoarseX++;
                }
            }
        }

        void IncrementScrollY()
        {
            if (_mask.ShowBackground || _mask.ShowSprites)
            {
                if (_vramAddress.FineY < 7)
                {
                    _vramAddress.FineY++;
                }
                else
                {
                    _vramAddress.FineY = 0;

                    if (_vramAddress.CoarseY == 29)
                    {
                        _vramAddress.CoarseY = 0;
                        _vramAddress.NameTableY = !_vramAddress.NameTableY;
                    }
                    else if (_vramAddress.CoarseY == 31)
                    {
                        _vramAddress.CoarseY = 0;
                    }
                    else
                    {
                        _vramAddress.CoarseY++;
                    }
                }
            }
        }

        void TransferAddressX()
        {
            if (_mask.ShowBackground || _mask.ShowSprites)
            {
                _vramAddress.NameTableX = _tramAddress.NameTableX;
                _vramAddress.CoarseX = _tramAddress.CoarseX;
            }
        }

        void TransferAddressY()
        {
            if (_mask.ShowBackground || _mask.ShowSprites)
            {
                _vramAddress.FineY = _tramAddress.FineY;
                _vramAddress.NameTableY = _tramAddress.NameTableY;
                _vramAddress.CoarseY = _tramAddress.CoarseY;
            }
        }

        void LoadBackgroundShifters()
        {
            _bgShifterPatternLo = (ushort)((_bgShifterPatternLo & 0xFF00) | _bgNextTileLsb);
            _bgShifterPatternHi = (ushort)((_bgShifterPatternHi & 0xFF00) | _bgNextTileMsb);

            _bgShifterAttributeLo = (ushort)((_bgShifterAttributeLo & 0xFF00) | ((_bgNextTileAttribute & 0b01) != 0 ? 0xFF : 0x00));
            _bgShifterAttributeHi = (ushort)((_bgShifterAttributeHi & 0xFF00) | ((_bgNextTileAttribute & 0b10) != 0 ? 0xFF : 0x00));
        }

        void UpdateShifters()
        {
            if (_mask.ShowBackground)
            {
                _bgShifterPatternLo <<= 1;
                _bgShifterPatternHi <<= 1;

                _bgShifterAttributeLo <<= 1;
                _bgShifterAttributeHi <<= 1;
            }

            if (_mask.ShowSprites && (_cycle is >= 1 and < 258))
            {
                for (var i = 0; i < _spriteCount; i++)
                {
                    if (_spriteScanLine[i].X > 0)
                    {
                        _spriteScanLine[i].X--;
                    }
                    else
                    {
                        _spriteShifterPatternLo[i] <<= 1;
                        _spriteShifterPatternHi[i] <<= 1;
                    }
                }
            }
        }

        if (_scanLine is >= -1 and < 240)
        {
            if (_scanLine == 0 && _cycle == 0)
            {
                // "Odd Frame" cycle skip
                _cycle = 1;
            }

            if (_scanLine == -1 && _cycle == 1)
            {
                _status.VerticalBlank = false;
                _status.SpriteZeroHit = false;
                _status.SpriteOverflow = false;
                Array.Clear(_spriteShifterPatternHi, 0, _spriteShifterPatternHi.Length);
                Array.Clear(_spriteShifterPatternLo, 0, _spriteShifterPatternLo.Length);
            }

            if (_cycle is >= 2 and < 258 || _cycle is >= 321 and < 338)
            {
                UpdateShifters();

                switch ((_cycle - 1) % 8)
                {
                    case 0:
                        {
                            LoadBackgroundShifters();
                            var address = (ushort)(0x2000 | (_vramAddress.Register & 0x0FFF));
                            _bgNextTileId = PpuRead(address);
                        }
                        break;

                    case 2:
                        {
                            var address = (ushort)(
                                0x23C0
                                | (_vramAddress.NameTableY ? 0x800 : 0x000)
                                | (_vramAddress.NameTableX ? 0x400 : 0x000)
                                | ((_vramAddress.CoarseY >>> 2) << 3)
                                | (_vramAddress.CoarseX >>> 2));

                            _bgNextTileAttribute = PpuRead(address);

                            if ((_vramAddress.CoarseY & 0x02) != 0)
                            {
                                _bgNextTileAttribute >>>= 4;
                            }

                            if ((_vramAddress.CoarseX & 0x02) != 0)
                            {
                                _bgNextTileAttribute >>>= 2;
                            }

                            _bgNextTileAttribute &= 0x03;
                        }
                        break;

                    case 4:
                        {
                            var address = (ushort)((_control.PatternBackground ? 0x1000 : 0x0000)
                                                   + (_bgNextTileId << 4)
                                                   + _vramAddress.FineY + 0);
                            _bgNextTileLsb = PpuRead(address);
                        }
                        break;

                    case 6:
                        {
                            var address = (ushort)((_control.PatternBackground ? 0x1000 : 0x0000)
                                                   + (_bgNextTileId << 4)
                                                   + _vramAddress.FineY + 8);
                            _bgNextTileMsb = PpuRead(address);
                        }
                        break;

                    case 7:
                        IncrementScrollX();
                        break;
                }
            }

            if (_cycle == 256)
            {
                IncrementScrollY();
            }

            if (_cycle == 257)
            {
                LoadBackgroundShifters();
                TransferAddressX();
            }
                        
            // Superfluous reads of tile id at end of scanline
            if (_cycle is 338 or 340)
            {
                _bgNextTileId = PpuRead((ushort)(0x2000 | (_vramAddress.Register & 0x0FFF)));
            }

            if (_scanLine == -1 && _cycle is >= 280 and < 305)
            {
                TransferAddressY();
            }

            // Foreground rendering (AKA sprites)
            if (_cycle == 257 && _scanLine >= 0)
            {
                Array.Fill(_spriteScanLine, new Sprite(0xFF, 0xFF, 0xFF, 0xFF));
                Array.Clear(_spriteShifterPatternHi, 0, _spriteShifterPatternHi.Length);
                Array.Clear(_spriteShifterPatternLo, 0, _spriteShifterPatternLo.Length);

                _spriteCount = 0;
                _spriteZeroHitPossible = false;

                for (var i = 0; i < 64 && _spriteCount < 9; i++)
                {
                    var diff = (short)((short)_scanLine - _oamData[i * 4]); // sprite's Y coordinate
                    if (diff >= 0 && diff < (_control.SpriteSize ? 16 : 8))
                    {
                        if (_spriteCount < 8)
                        {
                            if (i == 0)
                            {
                                _spriteZeroHitPossible = true;
                            }

                            _spriteScanLine[_spriteCount] = Sprite.FromBytes(_oamData, i * 4);
                            _spriteCount++;
                        }
                        else
                        {
                            _status.SpriteOverflow = true;
                        }
                    }
                }
            }

            if (_cycle == 340)
            {
                for (var i = 0; i < _spriteCount; i++)
                {
                    var sprite = _spriteScanLine[i];
                    ushort spritePatternAddressLo;

                    if (!_control.SpriteSize)
                    {
                        // 8x8 Sprite Mode
                        if ((sprite.Attributes & 0x80) == 0)
                        {
                            // Sprite is NOT flipped vertically, i.e. normal
                            spritePatternAddressLo = (ushort)((_control.PatternSprite ? 0x1000 : 0x0000)
                                                                | (sprite.TileIndex << 4)
                                                                | (_scanLine - sprite.Y));
                        }
                        else
                        {
                            // Sprite is flipped vertically, i.e. upside down
                            spritePatternAddressLo = (ushort)((_control.PatternSprite ? 0x1000 : 0x0000)
                                                              | (sprite.TileIndex << 4)
                                                              | (7 - (_scanLine - sprite.Y)));
                        }
                    }
                    else
                    {
                        // 8x16 Sprite Mode
                        if ((sprite.Attributes & 0x80) == 0)
                        {
                            // Sprite is NOT flipped vertically, i.e. normal
                            if (_scanLine - sprite.Y < 8)
                            {
                                // Reading Top half Tile
                                spritePatternAddressLo = (ushort)(((sprite.TileIndex & 0x01) << 12)
                                                                  | ((sprite.TileIndex & 0xFE) << 4)
                                                                  | ((_scanLine - sprite.Y) & 0x07));
                            }
                            else 
                            {
                                // Reading Bottom Half Tile
                                spritePatternAddressLo = (ushort)(((sprite.TileIndex & 0x01) << 12)
                                                                  | (((sprite.TileIndex & 0xFE) + 1) << 4)
                                                                  | ((_scanLine - sprite.Y) & 0x07));
                            }
                        }
                        else
                        {
                            // Sprite is flipped vertically, i.e. upside down
                            if (_scanLine - sprite.Y < 8)
                            {
                                // Reading Top half Tile
                                spritePatternAddressLo = (ushort)(((sprite.TileIndex & 0x01) << 12)
                                                                  | ((sprite.TileIndex & 0xFE) << 4)
                                                                  | (7 - (_scanLine - sprite.Y) & 0x07));
                            }
                            else
                            {
                                // Reading Bottom Half Tile
                                spritePatternAddressLo = (ushort)(((sprite.TileIndex & 0x01) << 12)
                                                                  | (((sprite.TileIndex & 0xFE) + 1) << 4)
                                                                  | (7 - (_scanLine - sprite.Y) & 0x07));
                            }
                        }
                    }

                    var spritePatternAddressHi = (ushort)(spritePatternAddressLo + 8);
                    var spritePatternLo = PpuRead(spritePatternAddressLo);
                    var spritePatternHi = PpuRead(spritePatternAddressHi);

                    if ((sprite.Attributes & 0x40) != 0)
                    {
                        // Sprite is flipped horizontally
                        spritePatternLo = spritePatternLo.FlipByte();
                        spritePatternHi = spritePatternHi.FlipByte();
                    }

                    _spriteShifterPatternLo[i] = spritePatternLo;
                    _spriteShifterPatternHi[i] = spritePatternHi;
                }
            }
        }
        
        if (_scanLine == 240)
        {
            // Do nothing
        }

        if (_scanLine is >= 241 and < 261)
        {
            if (_scanLine == 241 && _cycle == 1)
            {
                _status.VerticalBlank = true;
                if (_control.EnableNMI)
                {
                    IsNMISet = true;
                }
            }
        }

        byte bgPixel = 0x00;
        byte bgPalette = 0x00;

        if (_mask.ShowBackground)
        {
            var bitMux = (ushort)(0x8000 >>> _fineX);

            var p0Pixel = (byte)((_bgShifterPatternLo & bitMux) > 0 ? 1 : 0);
            var p1Pixel = (byte)((_bgShifterPatternHi & bitMux) > 0 ? 1 : 0);
            bgPixel = (byte)((p1Pixel << 1) | p0Pixel);

            var bgPal0 = (byte)((_bgShifterAttributeLo & bitMux) > 0 ? 1 : 0);
            var bgPal1 = (byte)((_bgShifterAttributeHi & bitMux) > 0 ? 1 : 0);
            bgPalette = (byte)((bgPal1 << 1) | bgPal0);
        }

        byte fgPixel = 0x00;
        byte fgPalette = 0x00;
        byte fgPriority = 0x00;

        if (_mask.ShowSprites)
        {
            _spriteZeroBeingRendered = false;

            for (var i = 0; i < _spriteCount; i++)
            {
                if (_spriteScanLine[i].X == 0)
                {
                    var fgPixelLo = (byte)((_spriteShifterPatternLo[i] & 0x80) > 0 ? 1 : 0);
                    var fgPixelHi = (byte)((_spriteShifterPatternHi[i] & 0x80) > 0 ? 1 : 0);
                    fgPixel = (byte)((fgPixelHi << 1) | fgPixelLo);

                    fgPalette = (byte)((_spriteScanLine[i].Attributes & 0x03) + 0x04);
                    fgPriority = (byte)((_spriteScanLine[i].Attributes & 0x20) == 0 ? 1 : 0);

                    if (fgPixel != 0)
                    {
                        if (i == 0)
                        {
                            _spriteZeroBeingRendered = true;
                        }

                        break;
                    }
                }
            }
        }

        byte pixel = 0x00;
        byte palette = 0x00;

        if (bgPixel == 0 && fgPixel == 0)
        {
            // pixel and palette are 0, draw the default background color
        }
        else if (bgPixel == 0 && fgPixel > 0)
        {
            pixel = fgPixel;
            palette = fgPalette;
        }
        else if (bgPixel > 0 && fgPixel == 0)
        {
            pixel = bgPixel;
            palette = bgPalette;
        }
        else 
        {
            // Both background and foreground have a pixel, need to decide priority
            if (fgPriority == 1)
            {
                pixel = fgPixel;
                palette = fgPalette;
            }
            else
            {
                pixel = bgPixel;
                palette = bgPalette;
            }

            if (_spriteZeroHitPossible && _spriteZeroBeingRendered)
            {
                if (_mask.ShowBackground && _mask.ShowSprites)
                {
                    if (!(_mask.BackgroundMask || _mask.SpriteMask))
                    {
                        if (_cycle is >= 9 and < 258)
                        {
                            _status.SpriteZeroHit = true;
                        }
                    }
                    else
                    {
                        if (_cycle is >= 1 and < 258)
                        {
                            _status.SpriteZeroHit = true;
                        }
                    }
                }
            }
        }

        var x = _cycle - 1;
        var y = _scanLine;
        if (x is >= 0 and < 256 && y is >= 0 and < 240)
        {
            var rgb = GetColorFromPalette(palette, pixel);
            _screen[y * 256 + x] = rgb;
        }

        _cycle++;
        if (_mask.ShowBackground || _mask.ShowSprites)
        {
            if (_cycle == 260 && _scanLine < 240)
            {
                _cartridge!.Mapper!.ScanLine();
            }
        }

        if (_cycle >= 341)
        {
            _cycle = 0;
            _scanLine++;

            if (_scanLine >= 261)
            {
                _scanLine = -1;
                FrameComplete = true;
            }
        }
    }

    public bool FrameComplete { get; set; }

    public RgbColor[] Screen => _screen;

    public byte CpuRead(ushort address, bool readOnly = false)
    {
        byte data = 0;

        switch (address)
        {
            case 0x0000: // Control
                break;

            case 0x0001: // Mask
                break;

            case 0x0002: // Status
                data = (byte)((_status.Register & 0xE0) | (_ppuDataBuffer & 0x1F));
                _status.VerticalBlank = false;
                _addressLatch = 0;
                break;

            case 0x0003: // OAM Address
                break;

            case 0x0004: // OAM Data
                data = _oamData[_oamAddress];
                break;

            case 0x0005: // Scroll
                break;

            case 0x0006: // PPU Address
                break;

            case 0x0007: // PPU Data
                data = _ppuDataBuffer;
                _ppuDataBuffer = PpuRead(_vramAddress.Register);

                if (_vramAddress.Register >= 0x3F00)
                {
                    data = _ppuDataBuffer;
                }
                _vramAddress.Register += (ushort)(_control.IncrementMode ? 32 : 1);
                break;
        }

        return data;
    }

    public void CpuWrite(ushort address, byte data)
    {
        switch (address)
        {
            case 0x0000: // Control
                _control.Register = data;
                _tramAddress.NameTableX = _control.NameTableX;
                _tramAddress.NameTableY = _control.NameTableY;
                break;

            case 0x0001: // Mask
                _mask.Register = data;
                break;

            case 0x0002: // Status
                break;

            case 0x0003: // OAM Address
                _oamAddress = data;
                break;

            case 0x0004: // OAM Data
                _oamData[_oamAddress] = data;
                break;

            case 0x0005: // Scroll
                if (_addressLatch == 0)
                {
                    _fineX = (byte)(data & 0x07);
                    _tramAddress.CoarseX = (byte)(data >>> 3);
                    _addressLatch = 1;
                }
                else
                {
                    _tramAddress.FineY = (byte)(data & 0x07);
                    _tramAddress.CoarseY = (byte)(data >>> 3);
                    _addressLatch = 0;
                }
                break;

            case 0x0006: // PPU Address
                if (_addressLatch == 0)
                {
                    _tramAddress.Register = (ushort)(((data & 0x3F) << 8) | (_tramAddress.Register & 0x00FF));
                    _addressLatch = 1;
                }
                else
                {
                    _tramAddress.Register = (ushort)((_tramAddress.Register & 0xFF00) | data);
                    _vramAddress.Register = _tramAddress.Register;
                    _addressLatch = 0;
                }
                break;

            case 0x0007: // PPU Data
                PpuWrite(_vramAddress.Register, data);
                _vramAddress.Register += (ushort)(_control.IncrementMode ? 32 : 1);
                break;
        }
    }

    public byte PpuRead(ushort address, bool readOnly = false)
    {
        byte data = 0;
        address &= 0x3FFF;

        if (_cartridge != null && _cartridge.PpuRead(address, out data))
        {

        }
        else if (address <= 0x1FFF)
        {
            data = _patternTable[(address & 0x1000) >>> 12][address & 0x0FFF];
        }
        else if (address <= 0x3EFF)
        {
            address &= 0x0FFF;

            if (_cartridge!.MirrorMode == MirrorMode.Vertical)
            {
                if (address <= 0x03FF)
                {
                    data = _nameTable[0][address & 0x03FF];
                }
                if (address is >= 0x0400 and <= 0x07FF)
                {
                    data = _nameTable[1][address & 0x03FF];
                }
                if (address is >= 0x0800 and <= 0x0BFF)
                {
                    data = _nameTable[0][address & 0x03FF];
                }
                if (address is >= 0x0C00 and <= 0x0FFF)
                {
                    data = _nameTable[1][address & 0x03FF];
                }
            }
            else if (_cartridge.MirrorMode == MirrorMode.Horizontal)
            {
                if (address <= 0x03FF)
                {
                    data = _nameTable[0][address & 0x03FF];
                }
                if (address is >= 0x0400 and <= 0x07FF)
                {
                    data = _nameTable[0][address & 0x03FF];
                }
                if (address is >= 0x0800 and <= 0x0BFF)
                {
                    data = _nameTable[1][address & 0x03FF];
                }
                if (address is >= 0x0C00 and <= 0x0FFF)
                {
                    data = _nameTable[1][address & 0x03FF];
                }
            }
        }
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x001F;
            if (address == 0x0010)
            {
                address = 0x0000;
            }
            if (address == 0x0014)
            {
                address = 0x0004;
            }
            if (address == 0x0018)
            {
                address = 0x0008;
            }
            if (address == 0x001C)
            {
                address = 0x000C;
            }
            data = (byte)(_palette[address] & (_mask.Grayscale ? 0x30 : 0x3F));
        }

        return data;
    }

    public void PpuWrite(ushort address, byte data)
    {
        address &= 0x3FFF;

        if (_cartridge != null && _cartridge.PpuWrite(address, data))
        {
            
        }
        else if (address <= 0x1FFF)
        {
            _patternTable[(address & 0x1000) >>> 12][address & 0x0FFF] = data;
        }
        else if (address <= 0x3EFF)
        {
            address &= 0x0FFF;

            if (_cartridge!.MirrorMode == MirrorMode.Vertical)
            {
                if (address <= 0x03FF)
                {
                    _nameTable[0][address & 0x03FF] = data;
                }
                if (address is >= 0x0400 and <= 0x07FF)
                {
                    _nameTable[1][address & 0x03FF] = data;
                }
                if (address is >= 0x0800 and <= 0x0BFF)
                {
                    _nameTable[0][address & 0x03FF] = data;
                }
                if (address is >= 0x0C00 and <= 0x0FFF)
                {
                    _nameTable[1][address & 0x03FF] = data;
                }
            }
            else if (_cartridge.MirrorMode == MirrorMode.Horizontal)
            {
                if (address <= 0x03FF)
                {
                    _nameTable[0][address & 0x03FF] = data;
                }
                if (address is >= 0x0400 and <= 0x07FF)
                {
                    _nameTable[0][address & 0x03FF] = data;
                }
                if (address is >= 0x0800 and <= 0x0BFF)
                {
                    _nameTable[1][address & 0x03FF] = data;
                }
                if (address is >= 0x0C00 and <= 0x0FFF)
                {
                    _nameTable[1][address & 0x03FF] = data;
                }
            }
        }
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x001F;
            if (address == 0x0010)
            {
                address = 0x0000;
            }
            if (address == 0x0014)
            {
                address = 0x0004;
            }
            if (address == 0x0018)
            {
                address = 0x0008;
            }
            if (address == 0x001C)
            {
                address = 0x000C;
            }
            _palette[address] = data;
        }
    }

    public RgbColor[] GetPatternTable(RgbColor[] patternTable, byte index = 0, byte palette = 0)
    {
        // Clamp the index to 0x00 or 0x01
        if (index > 1)
        {
            index = 1;
        }

        for (var tileY = 0; tileY < 16; tileY++)
        {
            for (var tileX = 0; tileX < 16; tileX++)
            {
                // Convert the 2D tile coordinate into a 1D offset into the pattern
                // table memory.
                var offset = tileY * 256 + tileX * 16;

                // Now loop through 8 rows of 8 pixels
                for (var row = 0; row < 8; row++)
                {
                    var tileLsb = PpuRead((ushort)(index * 0x1000 + offset + row + 0));
                    var tileMsb = PpuRead((ushort)(index * 0x1000 + offset + row + 8));

                    for (var col = 0; col < 8; col++)
                    {
                        var pixel = (byte)(((tileLsb & 0x01) << 1) | (tileMsb & 0x01));

                        tileLsb >>= 1;
                        tileMsb >>= 1;

                        var x = tileX * 8 + (7 - col); 
                        var y = tileY * 8 + row;
                        patternTable[y * 128 + x] = GetColorFromPalette(palette, pixel);
                    }
                }
            }
        }

        return patternTable;
    }

    public RgbColor GetColorFromPalette(byte palette, byte pixel)
    {
        var paletteIndex = PpuRead((ushort)(0x3F00 + (palette << 2) + pixel)) & 0x3F;
        return PalScreen[paletteIndex];
    }

    private sealed class PpuStatusRegister
    {
        public byte Unused
        {
            get => (byte)(Register & 0b0001_1111);
            set => Register = (byte)((Register & 0b1110_0000) | (value & 0b0001_1111));
        }

        public bool SpriteOverflow
        {
            get => (Register & 0b0010_0000) > 0;
            set => Register = (byte)((Register & 0b1101_1111) | (value ? 0b0010_0000 : 0));
        }

        public bool SpriteZeroHit
        {
            get => (Register & 0b0100_0000) > 0;
            set => Register = (byte)((Register & 0b1011_1111) | (value ? 0b0100_0000 : 0));
        }

        public bool VerticalBlank
        {
            get => (Register & 0b1000_0000) > 0;
            set => Register = (byte)((Register & 0b0111_1111) | (value ? 0b1000_0000 : 0));
        }

        public byte Register { get; set; }
    }

    private sealed class PpuMaskRegister
    {
        public bool Grayscale
        {
            get => (Register & 0b0000_0001) > 0;
            set => Register = (byte)((Register & 0b1111_1110) | (value ? 0b0000_0001 : 0));
        }

        public bool BackgroundMask
        {
            get => (Register & 0b0000_0010) > 0;
            set => Register = (byte)((Register & 0b1111_1101) | (value ? 0b0000_0010 : 0));
        }

        public bool SpriteMask
        {
            get => (Register & 0b0000_0100) > 0;
            set => Register = (byte)((Register & 0b1111_1011) | (value ? 0b0000_0100 : 0));
        }

        public bool ShowBackground
        {
            get => (Register & 0b0000_1000) > 0;
            set => Register = (byte)((Register & 0b1111_0111) | (value ? 0b0000_1000 : 0));
        }

        public bool ShowSprites
        {
            get => (Register & 0b0001_0000) > 0;
            set => Register = (byte)((Register & 0b1110_1111) | (value ? 0b0001_0000 : 0));
        }

        public bool EmphasizeRed
        {
            get => (Register & 0b0010_0000) > 0;
            set => Register = (byte)((Register & 0b1101_1111) | (value ? 0b0010_0000 : 0));
        }

        public bool EmphasizeGreen
        {
            get => (Register & 0b0100_0000) > 0;
            set => Register = (byte)((Register & 0b1011_1111) | (value ? 0b0100_0000 : 0));
        }

        public bool EmphasizeBlue
        {
            get => (Register & 0b1000_0000) > 0;
            set => Register = (byte)((Register & 0b0111_1111) | (value ? 0b1000_0000 : 0));
        }

        public byte Register { get; set; }
    }

    private sealed class PpuControlRegister
    {
        public bool NameTableX
        {
            get => (Register & 0b0000_0001) > 0;
            set => Register = (byte)((Register & 0b1111_1110) | (value ? 0b0000_0001 : 0));
        }

        public bool NameTableY
        {
            get => (Register & 0b0000_0010) > 0;
            set => Register = (byte)((Register & 0b1111_1101) | (value ? 0b0000_0010 : 0));
        }

        public bool IncrementMode
        {
            get => (Register & 0b0000_0100) > 0;
            set => Register = (byte)((Register & 0b1111_1011) | (value ? 0b0000_0100 : 0));
        }

        public bool PatternSprite
        {
            get => (Register & 0b0000_1000) > 0;
            set => Register = (byte)((Register & 0b1111_0111) | (value ? 0b0000_1000 : 0));
        }

        public bool PatternBackground
        {
            get => (Register & 0b0001_0000) > 0;
            set => Register = (byte)((Register & 0b1110_1111) | (value ? 0b0001_0000 : 0));
        }

        public bool SpriteSize
        {
            get => (Register & 0b0010_0000) > 0;
            set => Register = (byte)((Register & 0b1101_1111) | (value ? 0b0010_0000 : 0));
        }

        public bool SlaveMode
        {
            get => (Register & 0b0100_0000) > 0;
            set => Register = (byte)((Register & 0b1011_1111) | (value ? 0b0100_0000 : 0));
        }

        public bool EnableNMI
        {
            get => (Register & 0b1000_0000) > 0;
            set => Register = (byte)((Register & 0b0111_1111) | (value ? 0b1000_0000 : 0));
        }

        public byte Register { get; set; }
    }

    private sealed class LoopyRegister
    {
        /* Implementation of the NES "loopy" register. This is an unsigned 16-bit register which stores:
         * CoarseX (5-bits) - Current X scroll offset (0-31)
         * CoarseY (5-bits) - Current Y scroll offset (0-31)
         * NametableX (1-bit) - Current nametable X offset (0-1)
         * NametableY (1-bit) - Current nametable Y offset (0-1)
         * FineY (3-bits) - Current fine Y scroll offset (0-7)
         * Unused (1-bit) - Unused
         */

        // CoarseX (5-bits) - Current X scroll offset (0-31)
        public byte CoarseX
        {
            get => (byte)(Register & 0b0000_0000_0001_1111);
            set => Register = (ushort)((Register & 0b1111_1111_1110_0000) | (value & 0b0001_1111));
        }

        // CoarseY (5-bits) - Current Y scroll offset (0-31)
        public byte CoarseY
        {
            get => (byte)((Register & 0b0000_0011_1110_0000) >>> 5);
            set => Register = (ushort)((Register & 0b1111_1100_0001_1111) | ((value & 0b0001_1111) << 5));
        }

        // NametableX (1-bit) - Current nametable X offset (0-1)
        public bool NameTableX
        {
            get => (Register & 0b0000_0100_0000_0000) > 0;
            set => Register = (ushort)((Register & 0b1111_1011_1111_1111) | (value ? 0b0000_0100_0000_0000 : 0));
        }

        // NametableY (1-bit) - Current nametable Y offset (0-1)
        public bool NameTableY
        {
            get => (Register & 0b0000_1000_0000_0000) > 0;
            set => Register = (ushort)((Register & 0b1111_0111_1111_1111) | (value ? 0b0000_1000_0000_0000 : 0));
        }

        // FineY (3-bits) - Current fine Y scroll offset (0-7)
        public byte FineY
        {
            get => (byte)((Register & 0b0111_0000_0000_0000) >>> 12);
            set => Register = (ushort)((Register & 0b1000_1111_1111_1111) | ((value & 0b0000_0111) << 12));
        }

        public bool Unused
        {
            get => (Register & 0b1000_0000_0000_0000) > 0;
            set => Register = (ushort)((Register & 0b0111_1111_1111_1111) | (value ? 0b1000_0000_0000_0000 : 0));
        }

        public ushort Register { get; set; }
    }

    private record struct Sprite(byte Y, byte TileIndex, byte Attributes, byte X)
    {
        public static Sprite FromBytes(IReadOnlyList<byte> buffer, int offset)
        {
            return new Sprite(buffer[offset + 0], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
        }

        public void WriteToBuffer(IList<byte> buffer, int offset)
        {
            buffer[offset + 0] = Y;
            buffer[offset + 1] = TileIndex;
            buffer[offset + 2] = Attributes;
            buffer[offset + 3] = X;
        }
    }
}