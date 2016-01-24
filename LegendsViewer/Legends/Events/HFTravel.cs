using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFTravel : WorldEvent
    {
        public Location Coordinates;
        public bool Escaped, Returned;
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "escape": Escaped = true; property.Known = true; break;
                    case "return": Returned = true; property.Known = true; break;
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                }
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (Escaped) return this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " escaped from the " + UndergroundRegion.ToSafeLink(link, pov);
            else if (Returned) eventString += " returned to ";
            else eventString += " made a journey to ";

            if (UndergroundRegion != null) eventString += UndergroundRegion.ToSafeLink(link, pov);
            else if (Site != null) eventString += Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += Region.ToSafeLink(link, pov);

            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}