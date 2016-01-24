using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AddHFHFLink : WorldEvent
    {
        public HistoricalFigure HistoricalFigure, HistoricalFigureTarget;
        public HistoricalFigureLinkType LinkType;
        public bool LinkTypeSet;
        public AddHFHFLink() { LinkType = HistoricalFigureLinkType.Unknown; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hf":
                    case "hfid":
                    case "histfig1":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "histfig2":
                    case "hf_target":
                    case "hfid_target":
                        HistoricalFigureTarget = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "link_type":
                        if (!Enum.TryParse(Formatting.InitCaps(property.Value.Replace("_", " ")).Replace(" ", ""), true, out LinkType))
                        {
                            LinkType = HistoricalFigureLinkType.Unknown;
                            world.ParsingErrors.Report("Unknown HF Link Type: " + property.Value);
                        }
                        LinkTypeSet = true;
                        break;
                }

            //Fill in LinkType by looking at related historical figures.
            if (!LinkTypeSet && LinkType == HistoricalFigureLinkType.Unknown && HistoricalFigure != HistoricalFigure.Unknown && HistoricalFigureTarget != HistoricalFigure.Unknown)
            {
                List<HistoricalFigureLink> historicalFigureToTargetLinks = HistoricalFigure.RelatedHistoricalFigures.Where(link => link.Type != HistoricalFigureLinkType.Child).Where(link => link.HistoricalFigure == HistoricalFigureTarget).ToList();
                HistoricalFigureLink historicalFigureToTargetLink = null;
                if (historicalFigureToTargetLinks.Count <= 1)
                    historicalFigureToTargetLink = historicalFigureToTargetLinks.FirstOrDefault();
                HFAbducted abduction = HistoricalFigureTarget.Events.OfType<HFAbducted>().SingleOrDefault(abduction1 => abduction1.Snatcher == HistoricalFigure);
                if (historicalFigureToTargetLink != null && abduction == null)
                    LinkType = historicalFigureToTargetLink.Type;
                else if (abduction != null)
                    LinkType = HistoricalFigureLinkType.Prisoner;
            }

            if (HistoricalFigure.Race == "Night Creature" || HistoricalFigureTarget.Race == "Night Creature")
            {
                if (LinkType == HistoricalFigureLinkType.Unknown)
                {
                    LinkType = HistoricalFigureLinkType.Spouse;
                }
                HistoricalFigure.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigureTarget, HistoricalFigureLinkType.ExSpouse));
                HistoricalFigureTarget.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigure, HistoricalFigureLinkType.ExSpouse));
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

            eventString += ((pov == HistoricalFigureTarget) ? HistoricalFigureTarget : HistoricalFigure).ToSafeLink(link, pov);
            switch (LinkType)
            {
                case HistoricalFigureLinkType.Apprentice:
                    if (pov == HistoricalFigureTarget)
                        eventString += " began an apprenticeship under ";
                    else
                        eventString += " became the master of ";
                    break;
                case HistoricalFigureLinkType.Master:
                    if (pov == HistoricalFigureTarget)
                        eventString += " became the master of ";
                    else
                        eventString += " began an apprenticeship under ";
                    break;
                case HistoricalFigureLinkType.FormerApprentice:
                    if (pov == HistoricalFigureTarget)
                        eventString += " ceased being the apprentice of ";
                    else
                        eventString += " ceased being the master of ";
                    break;
                case HistoricalFigureLinkType.FormerMaster:
                    if (pov == HistoricalFigureTarget)
                        eventString += " ceased being the master of ";
                    else
                        eventString += " ceased being the apprentice of ";
                    break;
                case HistoricalFigureLinkType.Deity:
                    if (pov == HistoricalFigureTarget)
                        eventString += " received the worship of ";
                    else
                        eventString += " began worshipping ";
                    break;
                case HistoricalFigureLinkType.Lover:
                    eventString += " became romantically involved with ";
                    break;
                case HistoricalFigureLinkType.Prisoner:
                    if (pov == HistoricalFigureTarget)
                        eventString += " was imprisoned by ";
                    else
                        eventString += " imprisoned ";
                    break;
                case HistoricalFigureLinkType.Spouse:
                    eventString += " married ";
                    break;
                case HistoricalFigureLinkType.Unknown:
                    eventString += " linked (UNKNOWN) to ";
                    break;
                default:
                    throw new Exception("Unhandled Link Type in AddHFHFLink: " + LinkType.GetDescription());
            }
            eventString += (pov == HistoricalFigureTarget ? HistoricalFigure : HistoricalFigureTarget).ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}