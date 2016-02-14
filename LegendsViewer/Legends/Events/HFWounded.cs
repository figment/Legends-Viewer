using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFWounded : WorldEvent
    {
        public int WoundeeRace { get; set; }
        public int WoundeeCaste { get; set; }

        // TODO
        public int BodyPart { get; set; } // legends_plus.xml
        public int InjuryType { get; set; } // legends_plus.xml
        public int PartLost { get; set; } // legends_plus.xml

        public HistoricalFigure Woundee, Wounder;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "woundee":
                    case "woundee_hfid": Woundee = world.GetHistoricalFigure(property.ValueAsInt()); Woundee.AddEvent(this); break;
                    case "wounder":
                    case "wounder_hfid": Wounder = world.GetHistoricalFigure(property.ValueAsInt()); Wounder.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "woundee_race": WoundeeRace = property.ValueAsInt(); break;
                    case "woundee_caste": WoundeeCaste = property.ValueAsInt(); break;
                    case "body_part": BodyPart = property.ValueAsInt(); break;
                    case "injury_type": InjuryType = property.ValueAsInt(); break;
                    case "part_lost": PartLost = property.ValueAsInt(); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Woundee.ToSafeLink(link, pov);
            eventString += " was wounded by ";
            eventString += Wounder.ToSafeLink(link, pov);

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