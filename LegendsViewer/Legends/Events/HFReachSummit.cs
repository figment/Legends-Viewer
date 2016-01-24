using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFReachSummit : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Site Site { get; set; }
        public Location Coordinates;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "group":
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                }
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            eventString += " reached the summit";
            if (Region != null)
            {
                eventString += ", which rises above ";
                eventString += Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += ", in the depths of ";
                eventString += UndergroundRegion.ToSafeLink(link, pov);
            }
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            eventString += ".";
            return eventString;
        }
    }
}