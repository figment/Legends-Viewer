using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class CreatedWorldConstruction : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site1, Site2;
        public WorldConstruction WorldConstruction, MasterWorldConstruction;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id1": Site1 = world.GetSite(property.ValueAsInt()); break;
                    case "site_id2": Site2 = world.GetSite(property.ValueAsInt()); break;
                    case "wcid": WorldConstruction = world.GetWorldConstruction(property.ValueAsInt()); break;
                    case "master_wcid": MasterWorldConstruction = world.GetWorldConstruction(property.ValueAsInt()); break;
                }
            }

            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);

            WorldConstruction.AddEvent(this);
            MasterWorldConstruction.AddEvent(this);

            Site1.AddEvent(this);
            Site2.AddEvent(this);

            Site1.AddConnection(Site2);
            Site2.AddConnection(Site1);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += SiteEntity.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " constructed ";
            eventString += WorldConstruction.ToSafeLink(link, pov);
            if (MasterWorldConstruction != null)
            {
                eventString += " as part of ";
                eventString += MasterWorldConstruction.ToSafeLink(link, pov);
            }
            eventString += " connecting ";
            eventString += Site1.ToSafeLink(link, pov);
            eventString += " and ";
            eventString += Site2.ToSafeLink(link, pov);
            return eventString;
        }
    }
}