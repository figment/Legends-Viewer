using System.ComponentModel;

namespace LegendsViewer.Legends.Enums
{
    public enum WrittenContentType
    {
        Unknown = -1,
        Manual,
        Guide,
        Chronicle,
        [Description("Short Story")]
        ShortStory,
        Novel,
        Biography,
        Autobiography,
        Poem,
        Play,
        Letter,
        Essay,
        Dialog,
        [Description("Comparative Biography")]
        ComparativeBiography = 14,
        [Description("Cultural History")]
        CulturalHistory = 18,
        [Description("Cultural Comparison")]
        CulturalComparison =19,
        [Description("Alternate History")]
        AlternateHistory =20,
        [Description("Treatise on Technological Evolution")]
        Treatise=21,
        Dictionary = 22,
        [Description("Star Chart")]
        StarChart = 23,
        Atlas = 25,
    }
}
