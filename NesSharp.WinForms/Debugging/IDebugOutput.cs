using NesSharp.Core;

namespace NesSharp.WinForms.Debugging
{
    public interface IDebugOutput
    {
        void SetPpu(Ppu ppu);
        void DebugUpdate();
    }
}