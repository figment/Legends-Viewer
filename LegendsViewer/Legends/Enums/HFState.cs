namespace LegendsViewer.Legends.Enums
{
    public enum HFState : byte
    {
        Unknown = 255,
        None = 254,
        Visiting = 0,
        Settled = 1,
        Wandering,
        Refugee,
        Scouting,
        Snatcher,
        Thief,
        Hunting,
    }
    public enum HFSubState : byte
    {
        Unknown = 255,
        Wandering = 0,
        Fled = 1,
    }
}