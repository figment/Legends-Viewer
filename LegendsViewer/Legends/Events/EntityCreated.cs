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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;

                    //Unhandled Events
                    case "structure_id":
                        Structure = Site?.GetStructure(StructureID = property.ValueAsInt());
                        Structure.AddEvent(this);
                        property.Known = true;
                        break;
                }
            }
            if (Site != null)
            {
                Structure = Site.Structures.FirstOrDefault(structure => structure.ID == StructureID);
            }
            Entity.AddEvent(this);
            Site.AddEvent(this);
            Structure.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
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