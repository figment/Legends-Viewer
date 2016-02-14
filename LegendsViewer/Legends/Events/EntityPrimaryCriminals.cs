using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class EntityPrimaryCriminals : WorldEvent
    {
        public int Action { get; set; } // legends_plus.xml
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
                    case "entity":
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "action": Action = property.ValueAsInt(); break;
                }
            }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Entity.ToSafeLink(link, pov) + " became the primary criminal organization in " + Site.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Structure.ToSafeLink(link,pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}