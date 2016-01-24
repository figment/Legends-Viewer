using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class EntityCreated : WorldEvent
    {
        public Entity Entity;
        public Site Site;
        public int StructureID;
        public Structure Structure;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;

                    //Unhandled Events
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                }
            Entity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += Entity.ToSafeLink(link, pov) + " formed in ";
            eventString += (Site.ToSafeLink(link, pov)) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}