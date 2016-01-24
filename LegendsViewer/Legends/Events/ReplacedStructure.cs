using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ReplacedStructure : WorldEvent
    {
        public int OldStructureID, NewStructureID;
        public Structure OldStructure, NewStructure;
        public Entity Civ, SiteEntity;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "old_structure":
                    case "old_ab_id": OldStructure = Site?.GetStructure(OldStructureID = property.ValueAsInt()); OldStructure.AddEvent(this); break;
                    case "new_structure":
                    case "new_ab_id": NewStructure = Site?.GetStructure(NewStructureID = property.ValueAsInt()); NewStructure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ":
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
            }
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
            eventString += " replaced ";
            eventString += OldStructure.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " with ";
            eventString += NewStructure.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }
}