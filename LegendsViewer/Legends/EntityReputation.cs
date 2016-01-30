using LegendsViewer.Legends.Enums;
using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public class EntityReputation
    {
        public Entity Entity { get; set; }
        public int UnsolvedMurders { get; set; }
        public int FirstSuspectedAgelessYear { get; set; }
        public string FirstSuspectedAgelessSeason { get; set; }
        public Dictionary<ReputationType, int> Reputations = new Dictionary<ReputationType, int>();

        public EntityReputation(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "unsolved_murders": UnsolvedMurders = property.ValueAsInt(); break;
                    case "first_ageless_year": FirstSuspectedAgelessYear = property.ValueAsInt(); break;
                    case "first_ageless_season_count": FirstSuspectedAgelessSeason = Formatting.TimeCountToSeason(property.ValueAsInt()); break;
                    case "rep_enemy_fighter": Reputations.Add(ReputationType.EnemyFighter, property.ValueAsInt()); break;
                    case "rep_trade_partner": Reputations.Add(ReputationType.TradePartner, property.ValueAsInt()); break;
                    case "rep_killer": Reputations.Add(ReputationType.Killer, property.ValueAsInt()); break;
                    case "rep_poet": Reputations.Add(ReputationType.Poet, property.ValueAsInt()); break;
                    case "rep_bard": Reputations.Add(ReputationType.Bard, property.ValueAsInt()); break;
                    case "rep_storyteller": Reputations.Add(ReputationType.Storyteller, property.ValueAsInt()); break;
                    case "rep_dancer": Reputations.Add(ReputationType.Dancer, property.ValueAsInt()); break;
                }
            }
        }
    }
}
