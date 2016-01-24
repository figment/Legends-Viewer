using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class CreatureDevoured : WorldEvent
    {
        public string Race { get; set; }
        public string Caste { get; set; }

        public HistoricalFigure Eater, Victim;
        public Entity Entity { get; set; }
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "eater": Eater = world.GetHistoricalFigure(property.ValueAsInt()); Eater.AddEvent(this); break;
                    case "race": Race = Formatting.InitCaps(property.Value.Replace("_", " ")); break;
                    case "caste": Caste = Formatting.InitCaps(property.Value.Replace("_", " ")); break;
                    case "victim": Victim = world.GetHistoricalFigure((property.ValueAsInt())); Victim.AddEvent(this); break;
                    case "entity": Entity = world.GetEntity((property.ValueAsInt())); Entity.AddEvent(this); break;
                        //case "entity": Victim = world.GetHistoricalFigure(property.ValueAsInt()); Victim.AddEvent(this); break;
                }



        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += Eater.ToSafeLink(link, pov);
            eventString += " devoured ";
            if (Victim != null)
            {
                eventString += Victim.ToSafeLink(link, pov);
            }
            else if (!string.IsNullOrWhiteSpace(Race))
            {
                eventString += " a ";
                if (!string.IsNullOrWhiteSpace(Caste))
                {
                    eventString += Caste + " ";
                }
                eventString += Race;
            }
            else
            {
                eventString += "UNKNOWN HISTORICAL FIGURE";
            }
            eventString += " in ";
            if (Site != null)
            {
                eventString += Site.ToSafeLink(link, pov);
            }
            else if (Region != null)
            {
                eventString += Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += UndergroundRegion.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}