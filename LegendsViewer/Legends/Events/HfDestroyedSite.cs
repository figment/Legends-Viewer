using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HfDestroyedSite : WorldEvent
    {
        internal HistoricalFigure Attacker { get; set; }
        private Entity DefenderCiv { get; set; }
        private Entity SiteCiv { get; set; }
        private Site Site { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_hfid":
                        Attacker = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "defender_civ_id":
                        DefenderCiv = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_civ_id":
                        SiteCiv = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                }

            Attacker.AddEvent(this);
            DefenderCiv.AddEvent(this);
            SiteCiv.AddEvent(this);
            Site.AddEvent(this);

            OwnerPeriod lastSiteOwnerPeriod = Site.OwnerHistory.LastOrDefault();
            if (lastSiteOwnerPeriod != null)
            {
                lastSiteOwnerPeriod.EndYear = this.Year;
                lastSiteOwnerPeriod.EndCause = "destroyed";
                lastSiteOwnerPeriod.Ender = Attacker;
            }
            if (DefenderCiv != null)
            {
                OwnerPeriod lastDefenderCivOwnerPeriod = DefenderCiv.SiteHistory.LastOrDefault(s => s.Site == Site);
                if (lastDefenderCivOwnerPeriod != null)
                {
                    lastDefenderCivOwnerPeriod.EndYear = this.Year;
                    lastDefenderCivOwnerPeriod.EndCause = "destroyed";
                    lastDefenderCivOwnerPeriod.Ender = Attacker;
                }
            }
            OwnerPeriod lastSiteCiveOwnerPeriod = SiteCiv.SiteHistory.LastOrDefault(s => s.Site == Site);
            if (lastSiteCiveOwnerPeriod != null)
            {
                lastSiteCiveOwnerPeriod.EndYear = this.Year;
                lastSiteCiveOwnerPeriod.EndCause = "destroyed";
                lastSiteCiveOwnerPeriod.Ender = Attacker;
            }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " routed " + SiteCiv.ToSafeLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToSafeLink(link, pov);
            }
            eventString += " and destroyed " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}