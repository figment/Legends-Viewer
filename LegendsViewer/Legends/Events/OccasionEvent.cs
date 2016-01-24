using System;
using System.Collections.Generic;
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id":
                        Civ = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
                    case "feature_layer_id":
                        UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt());
                        break;
                    case "occasion_id":
                        OccasionId = property.ValueAsInt();
                        break;
                    case "schedule_id":
                        ScheduleId = property.ValueAsInt();
                        break;
                }
            Civ.AddEvent(this);
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
            string eventString = GetYearTime();
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " held a ";
            eventString += OccasionType.ToString().ToLower();
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            //eventString += " as part of UNKNOWN OCCASION (" + OccasionId + ") with UNKNOWN SCHEDULE(" + ScheduleId + ")";
            eventString += ".";
            return eventString;
        }
    }
}