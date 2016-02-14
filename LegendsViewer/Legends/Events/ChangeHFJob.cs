using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ChangeHFJob : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public string OldJob, NewJob;

        public ChangeHFJob()
        {
            NewJob = "UNKNOWN JOB";
            OldJob = "UNKNOWN JOB";
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "old_job": OldJob = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " "))); break;
                    case "new_job": NewJob = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " "))); break;
                }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (OldJob != "standard" && NewJob != "standard")
            {
                eventString += " gave up being a " + OldJob + " to become a " + NewJob;
            }
            else if (NewJob != "standard")
            {
                eventString += " became a " + NewJob;
            }
            else if (OldJob != "standard")
            {
                eventString += " stopped being a " + OldJob;
            }
            else
            {
                eventString += " became a peasant";
            }
            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}