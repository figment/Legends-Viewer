using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class RemoveHFEntityLink : WorldEvent
    {
        public Entity Entity;
        public HistoricalFigure HistoricalFigure;
        public HfEntityLinkType LinkType;
        public string Position;

        public RemoveHFEntityLink()
        {
            LinkType = HfEntityLinkType.Unknown;
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "link_type":
                        if (!Enum.TryParse(property.Value, true, out LinkType))
                        {
                            world.ParsingErrors.Report("Unknown HfEntityLinkType: " + property.Value);
                        }
                        break;

                }
            }

            HistoricalFigure.AddEvent(this);
            Entity.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case HfEntityLinkType.Prisoner:
                    eventString += " escaped from the prisons of ";
                    break;
                case HfEntityLinkType.Slave:
                    eventString += " fled from ";
                    break;
                case HfEntityLinkType.Enemy:
                    eventString += " stopped being an enemy of ";
                    break;
                case HfEntityLinkType.Member:
                    eventString += " left ";
                    break;
                case HfEntityLinkType.Squad:
                case HfEntityLinkType.Position:
                    eventString += " stopped being the " + Position + " of ";
                    break;
                default:
                    eventString += " stopped being linked to ";
                    break;
            }

            eventString += Entity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}