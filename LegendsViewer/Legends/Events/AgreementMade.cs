using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AgreementMade : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }
        public AgreementTopic Topic { get; set; }

        
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "topic":
                        switch (property.Value)
                        {
                            case "treequota": Topic = AgreementTopic.TreeQuota; break;
                            case "becomelandholder": Topic = AgreementTopic.BecomeLandHolder; break;
                            case "promotelandholder": Topic = AgreementTopic.PromoteLandHolder; break;
                            default:
                                Topic = AgreementTopic.Unknown;
                                world.ParsingErrors.Report("Unknown Agreement Topic: " + property.Value);
                                break;
                        }
                        break;
                    case "source": Source = world.GetEntity(property.ValueAsInt()); Source.AddEvent(this); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); Destination.AddEvent(this); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            switch (Topic)
            {
                case AgreementTopic.TreeQuota:
                    eventString += "a lumber agreement proposed by ";
                    break;
                case AgreementTopic.BecomeLandHolder:
                    eventString += "the establishment of landed nobility proposed by ";
                    break;
                case AgreementTopic.PromoteLandHolder:
                    eventString += "the elevation of the landed nobility proposed by ";
                    break;
                default:
                    eventString += "UNKNOWN AGREEMENT";
                    break;
            }
            eventString += " proposed by ";
            eventString += Source.ToSafeLink(link, pov);
            eventString += " was accepted by ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}