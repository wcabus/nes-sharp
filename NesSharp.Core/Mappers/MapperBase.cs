namespace NesSharp.Core.Mappers;

public abstract class MapperBase
{
    protected readonly int PrgBanks;
    protected readonly int ChrBanks;

    protected MapperBase(int prgBanks, int chrBanks)
    {
        PrgBanks = prgBanks;
        ChrBanks = chrBanks;
    }

    public virtual bool CpuMapRead(ushort address, out uint mappedAddress, ref byte data)
    {
        mappedAddress = 0;
        return false;
    }

    public virtual bool CpuMapWrite(ushort address, out uint mappedAddress, byte data = 0)
    {
        mappedAddress = 0;
        return false;
    }

    public virtual bool PpuMapRead(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;
        return false;
    }

    public virtual bool PpuMapWrite(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;
        return false;
    }

    public virtual void Reset()
    {

    }

    public virtual MirrorMode Mirror()
    {
        return MirrorMode.Hardware;
    }
    
    public virtual bool IRQState()
    {
        return false;
    }

    public virtual void IRQClear()
    {

    }

    public virtual void ScanLine()
    {

    }
}