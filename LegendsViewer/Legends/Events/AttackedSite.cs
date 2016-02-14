using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AttackedSite : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity;
        public Site Site;
        public HistoricalFigure AttackerGeneral, DefenderGeneral;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); Attacker.AddEvent(this); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); Defender.AddEvent(this); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(property.ValueAsInt()); AttackerGeneral.AddEvent(this); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(property.ValueAsInt()); DefenderGeneral.AddEvent(this); break;
                }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.PrintEntity(true, pov) + " attacked ";
            if (SiteEntity != null) eventString += SiteEntity.PrintEntity(true, pov);
            else eventString += Defender.PrintEntity(true, pov);
            eventString += " at " + Site.ToSafeLink(link, pov) + ". ";
            if (AttackerGeneral != null)
                eventString += "Leader of the attack was " + AttackerGeneral.ToSafeLink(link, pov);
            if (DefenderGeneral != null)
                eventString += ", and the defenders were led by " + DefenderGeneral.ToSafeLink(link, pov);
            else eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}