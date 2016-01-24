using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class BodyAbused : WorldEvent
    {
        // TODO
        public string ItemType { get; set; } // legends_plus.xml
        public string ItemSubType { get; set; } // legends_plus.xml
        public string Material { get; set; } // legends_plus.xml
        public int PileTypeID { get; set; } // legends_plus.xml
        public int MaterialTypeID { get; set; } // legends_plus.xml
        public int MaterialIndex { get; set; } // legends_plus.xml
        public AbuseType AbuseType { get; set; } // legends_plus.xml

        public Entity Abuser { get; set; } // legends_plus.xml
        public HistoricalFigure Body { get; set; } // legends_plus.xml
        public HistoricalFigure HistoricalFigure { get; set; } // legends_plus.xml
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }

        public BodyAbused()
        {
            AbuseType = AbuseType.Unknown;
        }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "civ": Abuser = world.GetEntity(property.ValueAsInt()); Abuser.AddEvent(this); break;
                    case "bodies": Body = world.GetHistoricalFigure(property.ValueAsInt()); Body.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "abuse_type": AbuseType = (AbuseType)property.ValueAsInt(); break;
                    case "props_pile_type": PileTypeID = property.ValueAsInt(); break;
                    case "props_item_type": ItemType = string.Intern(property.Value); break;
                    case "props_item_subtype": ItemSubType = string.Intern(property.Value); break;
                    case "props_item_mat": property.Known = true; break;
                    case "props_item_mat_type": MaterialTypeID = property.ValueAsInt(); break;
                    case "props_item_mat_index": MaterialIndex = property.ValueAsInt(); break;
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
            eventString += Body.ToSafeLink(link, pov);

            if (AbuseType == AbuseType.Unknown)
                eventString += "'s body was abused by ";
            else
                eventString += "'s body was "+ AbuseType +" by ";
            eventString += Abuser.ToSafeLink(link, pov);
            if (ItemType != null)
                eventString += " with a " + ItemType;
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}