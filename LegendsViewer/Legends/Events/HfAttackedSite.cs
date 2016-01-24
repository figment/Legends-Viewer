using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HfAttackedSite : WorldEvent
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
                    case "attacker_hfid": Attacker = world.GetHistoricalFigure(property.ValueAsInt()); Attacker.AddEvent(this); break;
                    case "defender_civ_id": DefenderCiv = world.GetEntity(property.ValueAsInt()); DefenderCiv.AddEvent(this); break;
                    case "site_civ_id": SiteCiv = world.GetEntity(property.ValueAsInt()); SiteCiv.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " attacked " + SiteCiv.ToSafeLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToSafeLink(link, pov);
            }
            eventString += " at " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}