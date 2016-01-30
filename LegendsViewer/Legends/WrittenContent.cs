using LegendsViewer.Controls.HTML.Utilities;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Events;
using LegendsViewer.Legends.Parser;
using System;
using LegendsViewer.Legends.Enums;

namespace LegendsViewer.Legends
{
    public class WrittenContent : WorldObject
    {
        public string Name { get; set; } // legends_plus.xml
        public int PageStart { get; set; } // legends_plus.xml
        public int PageEnd { get; set; } // legends_plus.xml
        public WrittenContentType Type { get; set; } // legends_plus.xml
        public HistoricalFigure Author { get; set; } // legends_plus.xml
        public List<string> Styles { get; set; } // legends_plus.xml
        public List<Reference> References { get; set; } // legends_plus.xml

        public static string Icon = "<i class=\"fa fa-fw fa-book\"></i>";

        public static List<string> Filters;
        public override List<WorldEvent> FilteredEvents
        {
            get { return Events.Where(dwarfEvent => !Filters.Contains(dwarfEvent.Type)).ToList(); }
        }

        public WrittenContent()
        {
            Name = "Untitled";
            Styles = new List<string>();
            References = new List<Reference>();
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "title": Name = Formatting.InitCaps(property.Value); break;
                    case "page_start": PageStart = property.ValueAsInt(); break;
                    case "page_end": PageEnd = property.ValueAsInt(); break;
                    case "reference": References.Add(new Reference(property.SubProperties, world)); break;
                    case "type":
                        switch (property.Value)
                        {
                            case "Autobiography": Type = WrittenContentType.Autobiography; break;
                            case "Biography": Type = WrittenContentType.Biography; break;
                            case "Chronicle": Type = WrittenContentType.Chronicle; break;
                            case "Dialog": Type = WrittenContentType.Dialog; break;
                            case "Essay": Type = WrittenContentType.Essay; break;
                            case "Guide": Type = WrittenContentType.Guide; break;
                            case "Letter": Type = WrittenContentType.Letter; break;
                            case "Manual": Type = WrittenContentType.Manual; break;
                            case "Novel": Type = WrittenContentType.Novel; break;
                            case "Play": Type = WrittenContentType.Play; break;
                            case "Poem": Type = WrittenContentType.Poem; break;
                            case "ShortStory": Type = WrittenContentType.ShortStory; break;
                            default:
                                int typeID;
                                if (int.TryParse(property.Value, out typeID))
                                {
                                    Type = (WrittenContentType) typeID;
                                }
                                else
                                {
                                    Type = WrittenContentType.Unknown;
                                    world.ParsingErrors.Report("Unknown WrittenContent WrittenContentType: " + property.Value);
                                }
                                break;
                        }
                        break;
                    case "author": Author = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "style": Styles.Add(property.Value); break;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override string ToLink(bool link = true, DwarfObject pov = null)
        {
            if (link)
            {
                string type = null;
                if (Type != WrittenContentType.Unknown)
                {
                    type = Type.GetDescription();
                }
                string title = "Written Content";
                title += string.IsNullOrWhiteSpace(type) ? "" : ", " + type;
                title += "&#13";
                title += "Events: " + Events.Count;

                string linkedString = "";
                if (pov != this)
                {
                    linkedString = Icon + "<a href = \"writtencontent#" + ID + "\" title=\"" + title + "\">" + Name + "</a>";
                }
                else
                {
                    linkedString = Icon + "<a title=\"" + title + "\">" + HTMLStyleUtil.CurrentDwarfObject(Name) + "</a>";
                }
                return linkedString;
            }
            else
            {
                return Name;
            }
        }
    }
}
