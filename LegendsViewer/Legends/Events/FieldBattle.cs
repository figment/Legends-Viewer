using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class FieldBattle : WorldEvent
    {
        public Entity Attacker, Defender;
        public WorldRegion Region;
        public HistoricalFigure AttackerGeneral, DefenderGeneral;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                }
            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            AttackerGeneral.AddEvent(this);
            DefenderGeneral.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " attacked " + Defender.ToSafeLink(link, pov) + " in " + Region.ToSafeLink(link, pov) + ". " +
                AttackerGeneral.ToSafeLink(link, pov) + " led the attack, and the defenders were led by " + DefenderGeneral.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}