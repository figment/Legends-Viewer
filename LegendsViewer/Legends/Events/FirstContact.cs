using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class FirstContact : WorldEvent
    {
        public Site Site;
        public Entity Contactor;
        public Entity Contacted;

        
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "contactor_enid": Contactor = world.GetEntity(property.ValueAsInt()); break;
                    case "contacted_enid": Contacted = world.GetEntity(property.ValueAsInt()); break;
                }
            }
            Site.AddEvent(this);
            Contactor.AddEvent(this);
            Contacted.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Contactor.ToSafeLink(link, pov);
            eventString += " made contact with ";
            eventString += Contacted.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            return eventString;
        }
    }
}