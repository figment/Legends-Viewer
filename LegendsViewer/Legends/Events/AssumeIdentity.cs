using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AssumeIdentity : WorldEvent
    {
        public HistoricalFigure Trickster { get; set; }
        public HistoricalFigure Identity { get; set; }
        public Entity Target { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(property.ValueAsInt()); Trickster.AddEvent(this); break;
                    case "identity_id": Identity = HistoricalFigure.Unknown; Identity.AddEvent(this); break; //Bad ID, so unknown for now.
                    case "target_enid": Target = world.GetEntity(property.ValueAsInt()); Target.AddEvent(this); break;
                    case "trickster": if (Trickster == null) { Trickster = world.GetHistoricalFigure(property.ValueAsInt()); } else property.Known = true; break;
                    case "target": if (Target == null) { Target = world.GetEntity(property.ValueAsInt()); } else property.Known = true; break;
                }
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Trickster.ToSafeLink(link, pov) + " fooled " + Target.ToSafeLink(link, pov) + " into believing " + Trickster.CasteNoun() + " was " + Identity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}