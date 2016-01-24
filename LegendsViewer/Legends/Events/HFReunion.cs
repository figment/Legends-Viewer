using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFReunion : WorldEvent
    {
        public HistoricalFigure HistoricalFigure1, HistoricalFigure2;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                }
            HistoricalFigure1.AddEvent(this);
            HistoricalFigure2.AddEvent(this);
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
            string eventString = this.GetYearTime() + " " + HistoricalFigure1.ToSafeLink(link, pov) + " was reunited with " + HistoricalFigure2.ToSafeLink(link, pov);
            if (Site != null) eventString += " in " + Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}