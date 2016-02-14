using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFNewPet : WorldEvent
    {
        public string Pet { get; set; }
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;

        public HFNewPet() { Pet = "UNKNOWN"; }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "group":
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "pets": Pet = Formatting.InitCaps(property.Value.Replace("_", " ").Replace("2", "two")); break;
                }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " tamed the creatures named ";
            eventString += !string.IsNullOrWhiteSpace(Pet) ? Pet : "UNKNOWN";
            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
            }
            else if (Region != null)
            {
                eventString += " in " + Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += " in " + UndergroundRegion.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}