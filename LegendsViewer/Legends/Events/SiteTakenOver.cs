using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class SiteTakenOver : WorldEvent
    {
        public Entity Attacker, Defender, NewSiteEntity, SiteEntity;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "new_site_civ_id": NewSiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                }

            if (Site.OwnerHistory.Count == 0)
                if (SiteEntity != null && SiteEntity != Defender)
                {
                    SiteEntity.Parent = Defender;
                    new OwnerPeriod(Site, SiteEntity, 1, "UNKNOWN");
                }
                else
                    new OwnerPeriod(Site, Defender, 1, "UNKNOWN");

            Site.OwnerHistory.Last().EndCause = "taken over";
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().Ender = Attacker;
            NewSiteEntity.Parent = Attacker;
            new OwnerPeriod(Site, NewSiteEntity, this.Year, "took over");

            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            NewSiteEntity.AddEvent(this);
            if (SiteEntity != Defender)
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            Defender.ToSafeLink(link, pov);
            eventString += " and took over " + Site.ToSafeLink(link, pov) + ". The new government was called " + NewSiteEntity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}