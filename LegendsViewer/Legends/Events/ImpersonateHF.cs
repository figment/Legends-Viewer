using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ImpersonateHF : WorldEvent
    {
        public HistoricalFigure Trickster, Cover;
        public Entity Target;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "cover_hfid": Cover = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "target_enid": Target = world.GetEntity(property.ValueAsInt()); break;
                }
            Trickster.AddEvent(this);
            Cover.AddEvent(this);
            Target.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Trickster.ToSafeLink(link, pov) + " fooled " + Target.ToSafeLink(link, pov)
                + " into believing he/she was a manifestation of the deity " + Cover.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}