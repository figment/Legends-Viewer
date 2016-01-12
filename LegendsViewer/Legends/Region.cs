﻿using LegendsViewer.Controls.HTML.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsViewer.Legends
{
    public class WorldRegion : WorldObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Deaths
        {
            get
            {
                List<string> deaths = new List<string>();
                deaths.AddRange(NotableDeaths.Select(death => death.Race));
                foreach (Battle.Squad squad in Battles.SelectMany(battle => battle.AttackerSquads.Concat(battle.DefenderSquads)))
                    for (int i = 0; i < squad.Deaths; i++)
                        deaths.Add(squad.Race);
                return deaths;
            }
            set { }
        }
        public List<HistoricalFigure> NotableDeaths { get { return Events.OfType<HFDied>().Select(death => death.HistoricalFigure).ToList(); } set { } }
        public List<Battle> Battles { get; set; }
        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }
        public List<Location> Locations { get; set; }

        public WorldRegion()
        {
            Name = "INVALID REGION"; Type = "INVALID";
            Battles = new List<Battle>();
        }
        public WorldRegion(List<Property> properties, World world)
            : base(properties, world)
        {
            Battles = new List<Battle>();
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "name": Name = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "type": Type = String.Intern(property.Value); property.Known = true; break;
                    case "coords": Locations = property.Value.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(Formatting.ConvertToLocation).ToList(); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string ToString() { return this.Name; }
        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                if (pov != this)
                {
                    string title = Type + " | Events: " + Events.Count;
                    return "<a href = \"region#" + this.ID + "\" title=\"" + title + "\">" + this.Name + "</a>";
                }
                else
                    return HTMLStyleUtil.CurrentDwarfObject(Name);
            }
            else
                return this.Name;
        }
    }
}
