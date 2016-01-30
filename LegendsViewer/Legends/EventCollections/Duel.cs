using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.EventCollections
{
    public class Duel : EventCollection
    {
        public int Ordinal;
        public Location Coordinates;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Site Site;
        public HistoricalFigure Attacker, Defender;
        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }
        public Duel() { Ordinal = -1; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ordinal": Ordinal = property.ValueAsInt(); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "parent_eventcol": ParentCollection = world.GetEventCollection(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "attacking_hfid": Attacker = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "defending_hfid": Defender = world.GetHistoricalFigure(property.ValueAsInt()); break;
                }
            //foreach (WorldEvent collectionEvent in Collection) this.AddEvent(collectionEvent);
            if (ParentCollection != null && ParentCollection.GetType() == typeof(Battle))
                foreach (HFDied death in Collection.OfType<HFDied>())
                {
                    Battle battle = ParentCollection as Battle;
                    War parentWar = (battle.ParentCollection as War);
                    if (battle.NotableAttackers.Contains(death.HistoricalFigure))
                    {
                        battle.AttackerDeathCount++;
                        battle.Attackers.Single(squad => squad.Race == death.HistoricalFigure.Race).Deaths++;

                        if (parentWar != null)
                        {
                            parentWar.AttackerDeathCount++;
                        }
                    }
                    else if (battle.NotableDefenders.Contains(death.HistoricalFigure))
                    {
                        battle.DefenderDeathCount++;
                        battle.Defenders.Single(squad => squad.Race == death.HistoricalFigure.Race).Deaths++;
                        if (parentWar != null)
                        {
                            parentWar.DefenderDeathCount++;
                        }
                    }

                    if (parentWar != null)
                    {
                        (ParentCollection.ParentCollection as War).DeathCount++;
                    }
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return "a duel";
        }
    }
}
