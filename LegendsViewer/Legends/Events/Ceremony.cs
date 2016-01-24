using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class Ceremony : OccasionEvent
    {
        public Ceremony()
        {
            OccasionType = OccasionType.Ceremony;
        }
    }
}