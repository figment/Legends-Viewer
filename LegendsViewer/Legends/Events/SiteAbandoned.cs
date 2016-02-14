using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class SiteAbandoned : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                }
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().EndCause = "abandoned";
            if (SiteEntity != null)
            {
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";
            }
            Civ.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
            Civ.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";

            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Civ.ToSafeLink(link, pov) + " abandoned the settlement at " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}