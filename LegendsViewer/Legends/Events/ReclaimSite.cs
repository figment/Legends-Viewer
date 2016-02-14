using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ReclaimSite : WorldEvent
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
            if (SiteEntity != null && SiteEntity != Civ)
                SiteEntity.Parent = Civ;

            //Make sure period was lost by an event, otherwise unknown loss
            if (Site.OwnerHistory.Count == 0)
                new OwnerPeriod(Site, null, 1, "founded");
            if (Site.OwnerHistory.Last().EndYear == -1)
            {
                Site.OwnerHistory.Last().EndCause = "abandoned";
                Site.OwnerHistory.Last().EndYear = this.Year - 1;
            }
            new OwnerPeriod(Site, SiteEntity, Year, "reclaimed");

            Civ.AddEvent(this);
            if (SiteEntity != Civ)
                SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Civ.ToSafeLink(link, pov) + " launched an expedition to reclaim " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}