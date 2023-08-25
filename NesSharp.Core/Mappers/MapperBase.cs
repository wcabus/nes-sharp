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

    public virtual bool CpuMapRead(ushort address, out uint mappedAddress)
    {
        mappedAddress = 0;
        return false;
    }

    public virtual bool CpuMapWrite(ushort address, out uint mappedAddress)
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
}