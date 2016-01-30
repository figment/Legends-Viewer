using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class DiplomatLost : WorldEvent
    {
        public Entity Entity { get; set; }
        public Entity InvolvedEntity { get; set; }
        public Site Site { get; set; }


        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "site": if (Site == null) { Site = world.GetSite(property.ValueAsInt()); } else property.Known = true; break;
                    case "entity": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "involved": InvolvedEntity = world.GetEntity(property.ValueAsInt()); break;
                }

            Entity.AddEvent(this);
            InvolvedEntity.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Entity.ToSafeLink(link, pov);
            eventString += " lost a diplomat at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". They suspected the involvement of ";
            eventString += InvolvedEntity.ToLink(link, pov);
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}