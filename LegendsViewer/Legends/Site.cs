using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Controls.HTML.Utilities;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;
using LegendsViewer.Legends.Enums;

namespace LegendsViewer.Legends
{
    public class Site : WorldObject
    {
        public string Icon = "<i class=\"fa fa-fw fa-home\"></i>";

        public string Type { get; set; }
        public SiteType SiteType { get; set; }
        public string Name { get; set; }
        public string UntranslatedName { get; set; }
        public Location Coordinates { get; set; }
        public bool HasStructures { get; set; }
        public List<Structure> Structures { get; set; }
        public List<EventCollection> Warfare { get; set; }
        public List<Battle> Battles { get { return Warfare.OfType<Battle>().ToList(); } set { } }
        public List<SiteConquered> Conquerings { get { return Warfare.OfType<SiteConquered>().ToList(); } set { } }
        public List<OwnerPeriod> OwnerHistory { get; set; }
        public static List<string> Filters;
        public DwarfObject CurrentOwner
        {
            get
            {
                if (OwnerHistory.Count(site => site.EndYear == -1) > 0)
                    return OwnerHistory.First(site => site.EndYear == -1).Owner;
                else
                    return null;
            }
            set { }
        }
        public List<DwarfObject> PreviousOwners { get { return OwnerHistory.Where(site => site.EndYear >= 0).Select(site => site.Owner).ToList(); } set { } }
        public List<Site> Connections { get; set; }
        public List<Population> Populations { get; set; }

        public List<Official> Officials { get; set; }
        public List<string> Deaths
        {
            get
            {
                List<string> deaths = new List<string>();
                deaths.AddRange(NotableDeaths.Select(death => death.Race));

                foreach (Battle.Squad squad in Battles.SelectMany(battle => battle.AttackerSquads.Concat(battle.DefenderSquads)).ToList())
                    for (int i = 0; i < squad.Deaths; i++)
                        deaths.Add(squad.Race);
                return deaths;
            }
            set { }
        }
        public List<HistoricalFigure> NotableDeaths { get { return Events.OfType<HFDied>().Select(death => death.HistoricalFigure).ToList(); } set { } }
        public List<BeastAttack> BeastAttacks { get; set; }
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }
        public class Official
        {
            public HistoricalFigure HistoricalFigure;
            public string Position;

            public Official(HistoricalFigure historicalFigure, string position)
            {
                HistoricalFigure = historicalFigure;
                Position = position;
        }
        }

        public Site()
        {
            ID = -1;
            Type = "INVALID";
            Name = "INVALID SITE";
            UntranslatedName = "";
            Warfare = new List<EventCollection>();
            OwnerHistory = new List<OwnerPeriod>();
            Connections = new List<Site>();
            Populations = new List<Population>();
            Officials = new List<Official>();
            BeastAttacks = new List<BeastAttack>();
            Structures = new List<Structure>(1);
        }
        
        private void InternalMerge(List<Property> properties, World world, bool merge = false)
        {
            foreach(Property property in properties)
            {
                switch(property.Name)
                {
                    //case "type": Type = Formatting.InitCaps(property.Value); break;
                    //case "name": Name = Formatting.InitCaps(property.Value); break;
                    case "type":
                        Type = Formatting.InitCaps(property.Value);
                        switch (property.Value)
                        {
                            case "cave": SiteType = SiteType.Cave; break;
                            case "fortress": SiteType = SiteType.Fortress; break;
                            case "forest retreat": SiteType = SiteType.ForestRetreat; break;
                            case "dark fortress": SiteType = SiteType.DarkFortress; break;
                            case "town": SiteType = SiteType.Town; break;
                            case "hamlet": SiteType = SiteType.Hamlet; break;
                            case "vault": SiteType = SiteType.Vault; break;
                            case "dark pits": SiteType = SiteType.DarkPits; break;
                            case "hillocks": SiteType = SiteType.Hillocks; break;
                            case "tomb": SiteType = SiteType.Tomb; break;
                            case "tower": SiteType = SiteType.Tower; break;
                            case "mountain halls": SiteType = SiteType.MountainHalls; break;
                            case "camp": SiteType = SiteType.Camp; break;
                            case "lair": SiteType = SiteType.Lair; break;
                            case "labyrinth": SiteType = SiteType.Labyrinth; break;
                            case "shrine": SiteType = SiteType.Shrine; break;
                            default:
                                world.ParsingErrors.Report("Unknown Site SiteType: " + property.Value);
                                break;
                        }
                        break;
                    case "name": Name = Formatting.InitCaps(property.Value); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "structures":
                        {
                            HasStructures = true; property.Known = true;
                            foreach (var subprop in property.SubProperties) {
                                subprop.Known = true;
                                UpsertStructure(subprop.SubProperties, world);
                            }

                        } break;
                    case "civ_id": property.Known = true; break;
                    case "cur_owner_id": property.Known = true; break;
                }
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world, true);
        }
        string Printidenicon()
        { 
            switch (SiteType)
            {
                case SiteType.Cave:
                    Icon = "<i class=\"fa fa-fw fa-circle\"></i>";
                    break;
                case SiteType.Fortress:
                    Icon = "<i class=\"fa fa-fw fa-fort-awesome\"></i>";
                    break;
                case SiteType.ForestRetreat:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-tree-deciduous\"></i>";
                    break;
                case SiteType.DarkFortress:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-compressed fa-rotate-90\"></i>";
                    break;
                case SiteType.Town:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-home\"></i>";
                    break;
                case SiteType.Hamlet:
                    Icon = "<i class=\"fa fa-fw fa-home\"></i>";
                    break;
                case SiteType.Vault:
                    Icon = "<i class=\"fa fa-fw fa-key\"></i>";
                    break;
                case SiteType.DarkPits:
                    Icon = "<i class=\"fa fa-fw fa-chevron-circle-down\"></i>";
                    break;
                case SiteType.Hillocks:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-grain\"></i>";
                    break;
                case SiteType.Tomb:
                    Icon = "<i class=\"fa fa-fw fa-archive fa-flip-vertical\"></i>";
                    break;
                case SiteType.Tower:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-tower\"></i>";
                    break;
                case SiteType.MountainHalls:
                    Icon = "<i class=\"fa fa-fw fa-gg-circle\"></i>";
                    break;
                case SiteType.Camp:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-tent\"></i>";
                    break;
                case SiteType.Lair:
                    Icon = "<i class=\"fa fa-fw fa-database\"></i>";
                    break;
                case SiteType.Labyrinth:
                    Icon = "<i class=\"fa fa-fw fa-ils fa-rotate-90\"></i>";
                    break;
                case SiteType.Shrine:
                    Icon = "<i class=\"glyphicon fa-fw glyphicon-screenshot\"></i>";
                    break;
            }
            return Icon;
        }

        public void AddConnection(Site connection)
        {
            if (!Connections.Contains(connection)) Connections.Add(connection);
        }

        public Structure GetStructure(int id)
        {
            if (id == -1) return null;
            else
            {
                int min = 0;
                int max = Structures.Count - 1;
                while (min <= max)
                {
                    int mid = min + (max - min) / 2;
                    if (id > Structures[mid].ID)
                        min = mid + 1;
                    else if (id < Structures[mid].ID)
                        max = mid - 1;
                    else
                        return Structures[mid];
                }
                return null;
            }
        }

        private void UpsertStructure(List<Property> properties, World world)
        {
            var id = properties.Where(x => x.Name == "id").Select(x => new int?(System.Convert.ToInt32(x.Value))).FirstOrDefault();
            if (id.HasValue && id.Value > -1)
            {
                var value = new Structure() {ID = id.Value};
                int index = Structures.BinarySearch(0, Structures.Count, value, new LambaComparer<Structure>((x,y) => Comparer<int>.Default.Compare(x.ID,y.ID))  );
                if (index >= 0)
                    value = Structures[index];
                else
                    Structures.Insert(~index, value);
                value.Merge(properties, world);
                value.Site = this;
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
                    return Icon + "<a href = \"site#" + ID + "\" title=\"" + title + "\">" + Name + "</a>";
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
