using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class KnowledgeDiscovered : WorldEvent
    {
        public string[] Knowledge { get; set; }
        public bool First { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "knowledge":
                        Knowledge = property.Value.Split(':');
                        break;
                    case "first":
                        First = true;
                        property.Known = true;
                        break;
                }
            HistoricalFigure.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            if (First)
            {
                eventString += " was the first to discover ";
            }
            else
            {
                eventString += " independently discovered ";
            }
            if (Knowledge.Length > 1)
            {
                eventString += " the " + Knowledge[1];
                if (Knowledge.Length > 2)
                {
                    eventString += " (" + Knowledge[2] + ")";
                }
                eventString += " in the field of " + Knowledge[0] + ".";
            }
            return eventString;
        }
    }
}