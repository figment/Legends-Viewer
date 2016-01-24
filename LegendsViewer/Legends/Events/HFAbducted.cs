using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFAbducted : WorldEvent
    {
        public HistoricalFigure Target { get; set; }
        public HistoricalFigure Snatcher { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "target_hfid": Target = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "snatcher_hfid": Snatcher = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                }
            Target.AddEvent(this);
            Snatcher.AddEvent(this);
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
            string eventString = this.GetYearTime();
            if (Snatcher != null)
                eventString += Snatcher.ToSafeLink(link, pov);
            else
                eventString += "(UNKNOWN HISTORICAL FIGURE)";
            eventString += " abducted " + Target.ToSafeLink(link, pov) + " from " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}