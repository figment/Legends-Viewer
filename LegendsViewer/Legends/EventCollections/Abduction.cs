﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class Abduction : EventCollection
    {
        public int Ordinal;
        public Location Coordinates;
        public HistoricalFigure Abductee;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Site Site;
        public Entity Attacker, Defender;
        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public Abduction(){}

        public Abduction(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ordinal": Ordinal = Convert.ToInt32(property.Value); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "parent_eventcol": ParentCollection = world.GetEventCollection(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "attacking_enid": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defending_enid": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return "an abduction";
            /*string colString = this.GetYearTime(true) + "The " + ordinals[numeral] + " abduction of ";
            if (abductee != null) colString += abductee.ToLink(path, pov);
            else colString += "UNKNOWN FIGURE";
                             return colString + " in " + period.ToLink(path, pov) + " ocurred";*/
        }
    }
}
