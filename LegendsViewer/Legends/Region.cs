using LegendsViewer.Controls.HTML.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Interfaces;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends
{
    public class WorldRegion : WorldObject, IHasCoordinates
    {
        public string Icon = "<i class=\"fa fa-fw fa-map-o\"></i>";

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
        public List<Location> Coordinates { get; set; } // legends_plus.xml
        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public WorldRegion()
        {
            Name = "INVALID REGION";
            Type = "INVALID";
            Battles = new List<Battle>();
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "name": Name = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "type": Type = String.Intern(property.Value); property.Known = true; break;
                    case "coords": Coordinates = property.Value.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(Formatting.ConvertToLocation).ToList(); property.Known = true; break;
                }
        }

        public override string ToString() { return this.Name; }
        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string title = Type;
                title += "&#13";
                title += "Events: " + Events.Count;

                if (pov != this)
                {
                    return Icon + "<a href = \"region#" + ID + "\" title=\"" + title + "\">" + Name + "</a>";
                }
                else
                {
                    return Icon + "<a title=\"" + title + "\">" + HTMLStyleUtil.CurrentDwarfObject(Name) + "</a>";
                }
            }
            else
            {
                return Name;
            }
        }
    }
}
