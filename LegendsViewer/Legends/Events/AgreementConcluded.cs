using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class AgreementConcluded : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }
        public AgreementTopic Topic { get; set; }
        public int Result { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
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
                    case "result": Result = property.ValueAsInt(); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            switch (Topic)
            {
                case AgreementTopic.TreeQuota:
                    eventString += "a lumber agreement between ";
                    break;
                case AgreementTopic.BecomeLandHolder:
                    eventString += "the establishment of landed nobility agreement between ";
                    break;
                case AgreementTopic.PromoteLandHolder:
                    eventString += "the elevation of the landed nobility agreement between ";
                    break;
                default:
                    eventString += "UNKNOWN AGREEMENT";
                    break;
            }
            eventString += Source.ToSafeLink(link, pov);
            eventString += " and ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " concluded";
            switch (Result)
            {
                case -3:
                    eventString += "  with miserable outcome";
                    break;
                case -2:
                    eventString += " with a strong negative outcome";
                    break;
                case -1:
                    eventString += " in an unsatisfactory fashion";
                    break;
                case 0:
                    eventString += " fairly";
                    break;
                case 1:
                    eventString += " with a positive outcome";
                    break;
                case 2:
                    eventString += ", cementing bonds of mutual trust";
                    break;
                case 3:
                    eventString += " with a very strong positive outcome";
                    break;
                default:
                    eventString += " with an unknown outcome";
                    break;
            }
            eventString += ".";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}