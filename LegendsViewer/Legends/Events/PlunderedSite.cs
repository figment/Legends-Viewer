using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class PlunderedSite : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity;
        public Site Site;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                }
            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            if (Defender != SiteEntity) SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && Defender != SiteEntity) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Defender.ToSafeLink(link, pov) + " and pillaged " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}