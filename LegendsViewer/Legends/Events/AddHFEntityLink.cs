using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AddHFEntityLink : WorldEvent
    {
        public Entity Entity;
        public HistoricalFigure HistoricalFigure;
        public HfEntityLinkType LinkType;
        public string Position;

        public AddHFEntityLink()
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
                    case "civ_id":
                        Entity = world.GetEntity(property.ValueAsInt());
                        Entity.AddEvent(this);
                        break;
                    case "histfig":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        HistoricalFigure.AddEvent(this);
                        break;
                    case "position":
                        Position = string.Intern(Formatting.InitCaps(property.Value));
                        break;
                    case "link_type":
                        if (!Enum.TryParse(property.Value, true, out LinkType))
                        {
                            world.ParsingErrors.Report("Unknown HfEntityLinkType: " + property.Value);
                            LinkType = HfEntityLinkType.Unknown;
                        }
                        break;
                }
            }
        }
        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case HfEntityLinkType.Prisoner: eventString += " was imprisoned by "; break;
                case HfEntityLinkType.Slave: eventString += " was enslaved by "; break;
                case HfEntityLinkType.Enemy: eventString += " became an enemy of "; break;
                case HfEntityLinkType.Member: eventString += " became a member of "; break;
                case HfEntityLinkType.FormerMember:
                    eventString += " became a former member of ";
                    break;
                case HfEntityLinkType.Squad:
                case HfEntityLinkType.Position: eventString += " became the " + Position + " of "; break;
                default: eventString += " linked to "; break;
            }
            eventString += Entity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


}