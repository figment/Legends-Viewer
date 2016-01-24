using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class Merchant : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        protected void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "source": Source = world.GetEntity(property.ValueAsInt()); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); break;
                }
            }
            Source.AddEvent(this);
            Destination.AddEvent(this);
            Site.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += "merchants from ";
            eventString += Source.ToSafeLink(link, pov, "CIV");
            eventString += " visited ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }
}