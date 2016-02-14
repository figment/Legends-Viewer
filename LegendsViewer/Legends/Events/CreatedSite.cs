using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class CreatedSite : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public HistoricalFigure Builder;
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "builder_hfid": Builder = world.GetHistoricalFigure(property.ValueAsInt()); break;
                }
            if (SiteEntity != null)
            {
                SiteEntity.Parent = Civ;
                new OwnerPeriod(Site, SiteEntity, this.Year, "founded");
            }
            else if (Civ != null)
            {
                new OwnerPeriod(Site, Civ, this.Year, "founded");
            }
            else if (Builder != null)
            {
                new OwnerPeriod(Site, Builder, this.Year, "created");
            }
            Site.AddEvent(this);
            SiteEntity.AddEvent(this);
            Civ.AddEvent(this);
            Builder.AddEvent(this);
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (Builder != null)
            {
                eventString += Builder.ToSafeLink(link, pov) + " created " + Site.ToSafeLink(link, pov) + ". ";
            }
            else
            {
                if (SiteEntity != null) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
                eventString += Civ.ToSafeLink(link, pov) + " founded " + Site.ToSafeLink(link, pov) + ". ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}