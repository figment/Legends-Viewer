using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class OccasionEvent : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public int OccasionId { get; set; }
        public int ScheduleId { get; set; }
        public OccasionType OccasionType { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id":
                        Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this);
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this);
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this);
                        break;
                    case "feature_layer_id":
                        UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this);
                        break;
                    case "occasion_id":
                        OccasionId = property.ValueAsInt();
                        break;
                    case "schedule_id":
                        ScheduleId = property.ValueAsInt();
                        break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " held a ";
            eventString += GetOccasionType();
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            //eventString += " as part of UNKNOWN OCCASION (" + OccasionId + ") with UNKNOWN SCHEDULE(" + ScheduleId + ")";
            eventString += ".";
            return eventString;
        }

        protected virtual string GetOccasionType()
        {
            switch (OccasionType)
            {
                case OccasionType.Competition:
                    switch (OccasionId)
                    {
                        case 1: return "foot race";
                    }
                    break;
            }
            return OccasionType.ToString().ToLower();
        }
    }
}