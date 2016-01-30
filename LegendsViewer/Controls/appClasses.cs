﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LegendsViewer.Legends;
using LegendsViewer.Legends.EventCollections;

namespace LegendsViewer
{
    public static class AppHelpers
    {
        public static string[][] EventInfo = new string[][]{
        new string[] {"add hf entity link",           "Historical Figure - Entity Link",          "(Incomplete) Enemy / Prisoner / Member / General / King / Queen / Etc."},
        new string[] {"add hf hf link",               "Historical Figure Links",                  "(Incomplete)  Marriage / Imprisonment / Worship"},
        new string[] {"attacked site",                "Site Attacked",                            ""},
        new string[] {"body abused",                  "Historical Figure Body Abused",            "(Incomplete) Mutilation / Impalement / Hanging"},
        new string[] {"change hf job",                "Historical Figure Change Job",             "(Incomplete)"},
        new string[] {"change hf state",              "Historical Figure Change State",           "Scouting / Wandering / Snatcher / Thief / Refugee"},
        new string[] {"change creature type",         "Historical Figure Tranformed",             "Tranformed into race / caste of abducter"},
        new string[] {"changed creature type",        "Historical Figure Tranformed",             "Tranformed into race / caste of abducter"},
        new string[] {"create entity position",       "Entity Position Created",                  "(Incomplete)"},
        new string[] {"created site",                 "Site Founded",                             ""},
        new string[] {"created world construction",   "Entity Construction Created",              "(Incomplete) Road / Bridges / Tunnels connecting two sites"},
        new string[] {"creature devoured",            "Historical Figure Eaten",                  "(Incomplete)",},
        new string[] {"destroyed site",               "Site Destroyed",                           "Site Attacked and Destroyed"},
        new string[] {"field battle",                 "Entity Battle",                            "Battle between 2 Civs."},
        new string[] {"hf abducted",                  "Historical Figure Abduction",              ""},
        new string[] {"hist figure died",             "Historical Figure Death",                  ""},
        new string[] {"hf died",                      "Historical Figure Death",                  ""},
        new string[] {"hist figure new pet",          "Historical Figure Tamed Creatures",        "(Incomplete) Tamed creatures in region"},
        new string[] {"hf new pet",                   "Historical Figure Tamed Creatures",        "(Incomplete) Tamed creatures in region"},
        new string[] {"hist figure reunion",          "Historical Figure Reunion",                ""},
        new string[] {"hf reunion",                   "Historical Figure Reunion",                ""},
        new string[] {"hf simple battle event",       "Historical Figure Fight",                  "Multiple Outcomes / Subtypes"},
        new string[] {"hist figure travel",           "Historical Figure Travel",                 ""},
        new string[] {"hf travel",                    "Historical Figure Travel",                 ""},
        new string[] {"hist figure wounded",          "Historical Figure Wounded",                "(Incomplete)"},
        new string[] {"hf wounded",                   "Historical Figure Wounded",                "(Incomplete)"},
        new string[] {"impersonate hf",               "Historical Figure Impersonation",          "Deity is impersonated, fooling Deity's associated civilization"},
        new string[] {"item stolen",                  "Historical Figure Theft",                  "(Incomplete)"},
        new string[] {"new site leader",              "Site Taken Over / New Leader",             "Site Attacked and taken over. New Government and Leader installed."},
        new string[] {"war peace accepted",           "Entity Accepted Peace",                    "(Incomplete)"},
        new string[] {"peace accepted",               "Entity Accepted Peace",                    "(Incomplete)"},
        new string[] {"war peace rejected",           "Entity Rejected Peace",                    "(Incomplete)"},
        new string[] {"peace rejected",               "Entity Rejected Peace",                    "(Incomplete)"},
        new string[] {"plundered site",               "Site Pillaged",                            "Site attacked and plundered"},
        new string[] {"reclaim site",                 "Site Reclaimed",                           ""},
        new string[] {"remove hf entity link",        "Historical Figure - Entity Link Removed",  "(Incomplete) No longer in leader position / escaped prisons"},
        new string[] {"artifact created",             "Historical Figure Artifact Created",       ""},
        new string[] {"artifact destroyed",           "Historical Figure Artifact Destroyed",     ""},
        new string[] {"diplomat lost",                "DF Mode - Diplomat Lost",                  ""},
        new string[] {"entity action",                "Entity Action",                            ""},
        new string[] {"entity created",               "Entity Created",                           ""},
        new string[] {"hf revived",                   "DF Mode - Historical Figure Became Ghost", ""},
        new string[] {"masterpiece arch design",      "DF Mode - Masterpiece Arch. Designed",     ""},
        new string[] {"masterpiece arch constructed", "DF Mode - Masterpiece Arch. Constructed",  ""},
        new string[] {"masterpiece engraving",        "DF Mode - Masterpiece Engraving",          ""},
        new string[] {"masterpiece food",             "DF Mode - Masterpiece Food Cooked",        ""},
        new string[] {"masterpiece dye",              "DF Mode - Masterpiece Dye Made",           ""},
        new string[] {"masterpiece item",             "DF Mode - Masterpiece Item Made",          ""},
        new string[] {"masterpiece item improvement", "DF Mode - Masterpiece Item Improvement",   ""},
        new string[] {"masterpiece lost",             "DF Mode - Masterpiece Item Lost",          ""},
        new string[] {"merchant",                     "DF Mode - Merchants Arrived",              ""},
        new string[] {"first contact",                "DF Mode - First Contact",                  ""},
        new string[] {"site abandoned",               "DF Mode - Site Abandoned",                 ""},
        new string[] {"site died",                    "DF Mode - Site Withered",                  ""},
        new string[] {"site retired",                 "DF Mode - Site Retired",                   ""},
        new string[] {"add hf site link",             "Historical Figure - Site Link",            "(Incomplete) Historical Figure started living at site"},
        new string[] {"created building",             "Site Structure Created",                   "(Incomplete) Some sort of structure created"},
        new string[] {"created structure",            "Site Structure Created",                   "(Incomplete) Some sort of structure created"},
        new string[] {"hf razed structure",           "Site Structure Razed",                     "(Incomplete)"},
        new string[] {"remove hf site link",          "Historical Figure - Site Link Removed",    "(Incomplete) Historical Figure moved out of site"},
        new string[] {"replaced building",            "Site Structure Replaced",                  "(Incomplete) Housing replaced with biggger housing"},
        new string[] {"replaced structure",           "Site Structure Replaced",                  "(Incomplete) Housing replaced with biggger housing"},
        new string[] {"site taken over",              "Site Taken Over",                          ""},
        new string[] {"entity relocate",              "Entity Relocated",                         ""},
        new string[] {"hf gains secret goal",         "Historical Figure Gained Secret Goal",     ""},
        new string[] {"hf profaned structure",        "Historical Figure Profaned structure",     ""},
        new string[] {"hf disturbed structure",       "Historical Figure Disturbed structure",    ""},
        new string[] {"hf does interaction",          "Historical Figure Did Interaction",        ""},
        new string[] {"entity primary criminals",     "Entity Became Primary Criminals",          ""},
        new string[] {"hf confronted",                "Historical Figure Confronted",             ""},
        new string[] {"assume identity",              "Historical Figure Assumed Identity",       ""},
        new string[] {"entity law",                   "Entity Law Change",                        ""},
        new string[] {"change hf body state",         "Historical Figure Body State Changed",     ""},
        new string[] {"razed structure",              "Entity Razed Structure",                   ""},
        new string[] {"hf learns secret",             "Historical Figure Learned Secret",         ""},
        new string[] {"artifact stored",              "Historical Figure Stored Artifact",        ""},
        new string[] {"artifact possessed",           "Historical Figure Obtained Artifact",      ""},
        new string[] {"artifact transformed",         "Historical Figure Transformed Artifact",   ""},
        new string[] {"agreement made",               "Entity Agreement Made",                    ""},
        new string[] {"artifact lost",                "Artifact Lost",                            ""},
        new string[] {"site dispute",                 "Site Dispute",                             ""},
        new string[] {"hf attacked site",             "Historical Figure Attacked Site",          ""},
        new string[] {"hf destroyed site",            "Historical Figure Destroyed Site",         ""},
        new string[] {"agreement formed",             "Agreement Formed",                         ""},
        new string[] {"agreement concluded",          "Agreement Concluded",                      ""},
        new string[] {"site tribute forced",          "Site Tribute Forced",                      ""},
        new string[] {"insurrection started",         "Insurrection Started",                     ""},
        new string[] {"hf act on building",           "Historical Figure Act on Building",        ""},

        new string[] {"hf reach summit",              "Historical Figure Reach Summit",           ""},
        // new 0.42.XX events
        new string[] { "procession",                  "Procession",                               ""},
        new string[] { "ceremony",                    "Ceremony",                                 ""},
        new string[] { "performance",                 "Performance",                              ""},
        new string[] { "competition",                 "Competition",                              ""},
        new string[] { "written content composed",    "Written Content Composed",                 ""},
        new string[] { "knowledge discovered",        "Knowledge Discovered",                     ""},
        new string[] { "hf relationship denied",      "Historical Figure Relationship Denied",    ""},
        new string[] { "poetic form created",         "Poetic Form Created",                      ""},
        new string[] { "musical form created",        "Musical Form Created",                     ""},
        new string[] { "dance form created",          "Dance Form Created",                       ""},
        new string[] { "regionpop incorporated into entity", "Regionpop Incorporated Into Entity",""},
        new string[] {"INVALID",                      "INVALID EVENT",                            ""}
    };

        private class ColumnBinding
        {
            public string PropertyName { get; set; }
            public string HeaderText { get; set; }
            public ColumnType Type { get; set; }

            public ColumnBinding(string property, string header, ColumnType type)
            {
                PropertyName = property;
                HeaderText = header;
                Type = ColumnType.Text;
            }

            public ColumnBinding(string property, string header) : this(property, header, ColumnType.Text) { }
            public ColumnBinding(string property, ColumnType type) : this(property, property, type) { }
            public ColumnBinding(string property) : this(property, property, ColumnType.Text) { }

        }

        private enum ColumnType
        {
            Text
        }

        public static List<DataGridViewColumn> GetColumns(Type dataType)
        {
            if (dataType.IsGenericType)
                dataType = dataType.GetGenericArguments()[0];
            List<DataGridViewColumn> columns = new List<DataGridViewColumn>();
            List<ColumnBinding> bindings = new List<ColumnBinding>();
            if (dataType == typeof(HistoricalFigure))
            {
                bindings  = new List<ColumnBinding> {   new ColumnBinding ( "Name" ), 
                                                        new ColumnBinding ( "Race" ), 
                                                        new ColumnBinding ( "Caste" ),
                                                        new ColumnBinding ( "AssociatedType", "Associated Type"),
                                                        new ColumnBinding ( "Age"),
                                                        new ColumnBinding ( "CurrentState", "State"),
                                                        new ColumnBinding ( "Kills"),
                                                        new ColumnBinding ( "Battles"),
                                                        new ColumnBinding ( "Abductions"),
                                                        new ColumnBinding ( "Abducted"),
                                                        new ColumnBinding ( "BeastAttacks", "Beast Attacks") };
            }
            else if (dataType == typeof(Entity))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("CurrentSites", "Sites"),
                                                       new ColumnBinding("LostSites", "Lost Sites"),
                                                       new ColumnBinding("Population"),
                                                       new ColumnBinding("Wars"),
                                                       new ColumnBinding("WarVictoryRatio", "Wins : Losses"),
                                                       new ColumnBinding("WarKillDeathRatio", "Kills : Deaths") };
            }
            else if (dataType == typeof(Site))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("Type"), 
                                                       new ColumnBinding("CurrentOwner", "Owner"),
                                                       new ColumnBinding("Warfare"),
                                                       new ColumnBinding("PreviousOwners", "Previous Owners"),
                                                       new ColumnBinding("Population"),
                                                       new ColumnBinding("Deaths"),
                                                       new ColumnBinding("BeastAttacks", "Beast Attacks") };
            }
            else if (dataType == typeof(LegendsViewer.Legends.WorldRegion))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("Type"),
                                                       new ColumnBinding("Deaths"),
                                                       new ColumnBinding("Battles") };
            }
            else if (dataType == typeof(UndergroundRegion))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Type"),
                                                       new ColumnBinding("Depth")};         
            }
            else if (dataType == typeof(War))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                        new ColumnBinding("Length"),
                                                        new ColumnBinding("Attacker"),
                                                        new ColumnBinding("Defender"),
                                                        //new ColumnBinding("Battles"),
                                                        //new ColumnBinding("DeathCount", "Deaths"),  
                                                        new ColumnBinding("AttackerToDefenderVictories", "Victories"),
                                                        new ColumnBinding("AttackerToDefenderKills", "Kills"),
                                                        new ColumnBinding("SitesLost", "Sites Lost") };

            }
            else if (dataType == typeof(Battle))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("StartYear", "Year"),
                                                       new ColumnBinding("Deaths"),
                                                       new ColumnBinding("Attacker"),
                                                       new ColumnBinding("Defender"),
                                                       new ColumnBinding("AttackersToDefenders", "Combatants"),
                                                       new ColumnBinding("AttackersToDefendersRemaining", "Remaining"),
                                                       new ColumnBinding("Outcome"),
                                                       new ColumnBinding("Conquering") };
            }
            else if (dataType == typeof(SiteConquered))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("StartYear", "Year"),
                                                       new ColumnBinding("Deaths") };
            }
            else if (dataType == typeof(BeastAttack))
            {
                bindings = new List<ColumnBinding>() { new ColumnBinding("Name"),
                                                       new ColumnBinding("Deaths"),
                                                       new ColumnBinding("StartYear", "Year") };
            }
            else if (dataType == typeof(Artifact))
            {
                bindings = new List<ColumnBinding>() {new ColumnBinding("Name"),
                                                      new ColumnBinding("Item") };
            }

            if (dataType.BaseType == typeof(WorldObject))
                bindings.Add(new ColumnBinding("Events"));
            if (dataType.BaseType == typeof(EventCollection))
                bindings.Add(new ColumnBinding("AllEvents", "Events"));

            foreach (ColumnBinding binding in bindings)
            {
                DataGridViewColumn propertyColumn;
                switch (binding.Type)
                {
                    case ColumnType.Text: propertyColumn = new DataGridViewTextBoxColumn(); break;
                    default: propertyColumn = new DataGridViewTextBoxColumn(); break;
                }

                propertyColumn.DataPropertyName = binding.PropertyName;
                propertyColumn.HeaderText = binding.HeaderText;
                columns.Add(propertyColumn);
            }

            return columns;
        }

        public static List<object> ToObjectList(this IQueryable results)  //No longer used?
        {
            List<object> list = new List<object>();
            System.Collections.IEnumerator get = results.GetEnumerator();
            while (get.MoveNext())
                list.Add(get.Current);
            return list;
        }

        public static double AverageOrZero(this IEnumerable<double> values)
        {
            if (values.Any()) return values.Average();
            else return 0;
        }

        public static string GetDescription(this object enumerationValue)
        {
            Type type = enumerationValue.GetType();
            if (type == typeof(double))
            {
                return (enumerationValue as Nullable<double>).Value.ToString("R");
            }
            if (!type.IsEnum)
            {
                return enumerationValue.ToString();
                //throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            System.Reflection.MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((System.ComponentModel.DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();

        }
    }
}
 
