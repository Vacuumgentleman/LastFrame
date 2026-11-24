namespace Senses2026
{
    public interface IRedHandedInterface
    {
        public int Mode();

        public bool FactionNameWasChanged(FactionNames _initialFactionName, out FactionNames _resultFactionName, out int _modeIndex);

    }
}

