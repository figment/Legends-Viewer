using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class PeaceEfforts : WorldEvent
    {
        public string Decision { get; set; }
        public string Topic { get; set; }
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "topic": Topic = Formatting.InitCaps(property.Value); break;
                    case "source": Source = world.GetEntity(property.ValueAsInt()); Source.AddEvent(this); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); Destination.AddEvent(this); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            if (Source != null && Destination != null)
            {
                eventString += Destination.ToSafeLink(link, pov) + " " + Decision + " an offer of peace from " + Source.ToSafeLink(link, pov) + " in " + ParentCollection.ToSafeLink(link, pov) + ".";
            }
            else
            {
                eventString += "Peace " + Decision + " in " + ParentCollection.ToSafeLink(link, pov) + ".";
            }
            return eventString;
        }
    }
}