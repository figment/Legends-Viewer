using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class DiplomatLost : WorldEvent
    {
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }

        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + "UNKNOWN ENTITY lost a diplomat at " + Site.ToSafeLink(link, pov)
                + ". They suspected the involvement of UNKNOWN ENTITY. ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}