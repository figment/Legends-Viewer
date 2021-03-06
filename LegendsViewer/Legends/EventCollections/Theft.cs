﻿using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.EventCollections
{
    public class Theft : EventCollection
    {
        public string Ordinal;
        Location Coordinates;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Site Site;
        public Entity Attacker, Defender;
        public List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return AllEvents.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ordinal": Ordinal = String.Intern(property.Value); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "parent_eventcol": ParentCollection = world.GetEventCollection(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "attacking_enid": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defending_enid": Defender = world.GetEntity(property.ValueAsInt()); break;
                }
            foreach (ItemStolen theft in Collection.OfType<ItemStolen>())
            {
                theft.Site = Site;
                Site.AddEvent(theft);
                Site.Events = Site.Events.OrderBy(ev => ev.ID).ToList();
                if (Attacker.SiteHistory.Count == 1)
                {
                    theft.ReturnSite = Attacker.SiteHistory.First().Site;
                    theft.ReturnSite.AddEvent(theft);
                    theft.ReturnSite.Events = theft.ReturnSite.Events.OrderBy(ev => ev.ID).ToList();
                }
            }
        }
        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            return "a theft";
        }
    }
}
