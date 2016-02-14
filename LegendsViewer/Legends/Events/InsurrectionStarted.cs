using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class InsurrectionStarted : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public InsurrectionOutcome Outcome { get; set; }
        public Boolean ActualStart { get; set; }
        private string unknownOutcome;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            ActualStart = false;

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "target_civ_id":
                        Civ = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "outcome":
                        switch (property.Value)
                        {
                            case "leadership overthrown":
                                Outcome = InsurrectionOutcome.LeadershipOverthrown;
                                break;
                            case "population gone":
                                Outcome = InsurrectionOutcome.PopulationGone;
                                break;
                            default:
                                Outcome = InsurrectionOutcome.Unknown;
                                unknownOutcome = property.Value;
                                world.ParsingErrors.Report("Unknown Insurrection Outcome: " + unknownOutcome);
                                break;
                        }
                        break;
                }
            }

            Civ.AddEvent(this);
            Site.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (ActualStart)
            {
                eventString += "an insurrection against " + Civ.ToSafeLink(link, pov) + " began in " + Site.ToSafeLink(link, pov) + ". ";
            }
            else
            {
                eventString += "the insurrection in " + Site.ToSafeLink(link, pov);
                switch (Outcome)
                {
                    case InsurrectionOutcome.LeadershipOverthrown:
                        eventString += " concluded with " + Civ.ToSafeLink(link, pov) + " overthrowing leadership. ";
                        break;
                    case InsurrectionOutcome.PopulationGone:
                        eventString += " ended with the disappearance of the rebelling population. ";
                        break;
                    default:
                        eventString += " against " + Civ.ToSafeLink(link, pov) + " concluded with (" + unknownOutcome + "). ";
                        break;
                }
            }

            eventString += PrintParentCollection();
            return eventString;
        }
    }
}