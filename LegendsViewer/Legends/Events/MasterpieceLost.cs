using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class MasterpieceLost : WorldEvent
    {
        public MasterpieceLost() { }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public int MethodID { get; set; }
        public MasterpieceItem CreationEvent { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "creation_event": CreationEvent = world.GetEvent(property.ValueAsInt()) as MasterpieceItem; break;
                    case "method": MethodID = property.ValueAsInt(); break;
                }
            }
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += "the masterful ";
            if (CreationEvent != null)
            {
                eventString += !string.IsNullOrWhiteSpace(CreationEvent.Material) ? CreationEvent.Material + " " : "";
                if (!string.IsNullOrWhiteSpace(CreationEvent.ItemSubType) && CreationEvent.ItemSubType != "-1")
                {
                    eventString += CreationEvent.ItemSubType;
                }
                else
                {
                    eventString += !string.IsNullOrWhiteSpace(CreationEvent.ItemType) ? CreationEvent.ItemType : "UNKNOWN ITEM";
                }
                eventString += " created by ";
                eventString += CreationEvent.Maker.ToSafeLink(link, pov);
                eventString += " for ";
                eventString += CreationEvent.MakerEntity.ToSafeLink(link, pov);
                eventString += " at ";
                eventString += CreationEvent.Site.ToSafeLink(link, pov);
                eventString += " ";
                eventString += CreationEvent.GetYearTime();
            }
            else
            {
                eventString += "UNKNOWN ITEM";
            }
            eventString += " was destroyed by ";
            eventString += HistoricalFigure != null ? HistoricalFigure.ToSafeLink(link, pov) : "an unknown creature";
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }
}