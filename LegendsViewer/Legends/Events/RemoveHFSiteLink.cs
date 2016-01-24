using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class RemoveHFSiteLink : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; } // TODO
        public Entity Civ { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public SiteLinkType LinkType { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "histfig":
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "link_type":
                        switch (property.Value.Replace("_", " "))
                        {
                            case "lair": LinkType = SiteLinkType.Lair; break;
                            case "hangout": LinkType = SiteLinkType.Hangout; break;
                            case "home_site_building":
                            case "home site building": LinkType = SiteLinkType.HomeSiteBuilding; break;
                            case "home site underground": LinkType = SiteLinkType.HomeSiteUnderground; break;
                            case "home structure": LinkType = SiteLinkType.HomeStructure; break;
                            case "seat of power": LinkType = SiteLinkType.SeatOfPower; break;
                            case "occupation": LinkType = SiteLinkType.Occupation; break;
                            case "home site realization building": LinkType = SiteLinkType.HomeSiteRealizationBuilding; break;
                            default:
                                LinkType = SiteLinkType.Unknown;
                                world.ParsingErrors.Report("Unknown Site Link Type: " + property.Value.Replace("_", " "));
                                break;
                        }
                        break;
                }
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {

            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case SiteLinkType.HomeSiteRealizationBuilding:
                    eventString += " moved out of ";
                    break;
                case SiteLinkType.Hangout:
                    eventString += " stopped ruling from ";
                    break;
                default:
                    eventString += " UNKNOWN LINKTYPE (" + LinkType + ") ";
                    break;
            }
            eventString += Structure.ToSafeLink(link, pov);
            if (Civ != null)
                eventString += " of " + Civ.ToSafeLink(link, pov);
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}