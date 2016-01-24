using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class SiteTributeForced : WorldEvent
    {
        public Entity Attacker { get; set; }
        public Entity Defender { get; set; }
        public Entity SiteEntity { get; set; }
        public Site Site { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "attacker_civ_id":
                        Attacker = world.GetEntity(property.ValueAsInt());
                        break;
                    case "defender_civ_id":
                        Defender = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_civ_id":
                        SiteEntity = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                }
            }

            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " secured tribute from " + SiteEntity.ToSafeLink(link, pov);
            if (Defender != null)
            {
                eventString += " of " + Defender.ToSafeLink(link, pov);
            }
            eventString += ", to be delivered from " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection();
            return eventString;
        }
    }
}