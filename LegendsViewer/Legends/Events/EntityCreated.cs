using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;
using System.Linq;

namespace LegendsViewer.Legends.Events
{
    public class EntityCreated : WorldEvent
    {
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;

                    //Unhandled Events
                    case "structure_id":
                        Structure = Site?.GetStructure(StructureID = property.ValueAsInt());
                        Structure.AddEvent(this);
                        property.Known = true;
                        break;
                }
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Entity.ToSafeLink(link, pov) + " formed";
            if (Structure != null)
            {
                eventString += " in ";
                eventString += Structure.ToSafeLink(link, pov);
            }
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}