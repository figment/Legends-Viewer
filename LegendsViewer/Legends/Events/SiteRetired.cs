using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class SiteRetired : WorldEvent
    {
        public Site Site { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteCiv { get; set; }
        public string First { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteCiv = world.GetEntity(property.ValueAsInt()); break;
                    case "first": First = property.Value; break;
                }
            }
            Site.AddEvent(this);
            Civ.AddEvent(this);
            SiteCiv.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += SiteCiv.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " at the settlement of ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " regained their senses after an initial period of questionable judgment.";
            return eventString;
        }
    }
}