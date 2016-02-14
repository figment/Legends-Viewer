using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFDoesInteraction : WorldEvent
    {
        public HistoricalFigure Doer { get; set; }
        public HistoricalFigure Target { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public string Interaction { get; set; }
        public string InteractionAction { get; set; }
        public string InteractionString { get; set; }
        public string InteractionDescription { get; set; }
        public string Source { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "doer":
                    case "doer_hfid": Doer = world.GetHistoricalFigure(property.ValueAsInt()); Doer.AddEvent(this); break;
                    case "target":
                    case "target_hfid": Target = world.GetHistoricalFigure(property.ValueAsInt()); Target.AddEvent(this); break;
                    case "interaction": Interaction = property.Value; break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "region": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "interaction_action": InteractionAction = property.Value.Replace("[IS_HIST_STRING_1:", "").Replace("[IS_HIST_STRING_2:", "").Replace("]", ""); break;
                    case "interaction_string": InteractionString = property.Value.Replace("[IS_HIST_STRING_2:", "").Replace("[I_TARGET:A:CREATURE", "").Replace("]", ""); break;
                    case "source": Source = property.Value; break;
                }
            }
            InteractionDescription = Interaction;
            if (!string.IsNullOrEmpty(InteractionAction) && !string.IsNullOrEmpty(InteractionString))
            {
                InteractionDescription = Formatting.ExtractInteractionString(InteractionAction) + " " +
                                         Formatting.ExtractInteractionString(InteractionString);
            }
            else
            {
                InteractionDescription = $"({Interaction})";
            }
            if (Target != null && !string.IsNullOrWhiteSpace(Interaction) && !Target.ActiveInteractions.Contains(Interaction))
                Target.ActiveInteractions.Add(Interaction);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Doer.ToSafeLink(link, pov);
            if (InteractionString == "")
            {
                eventString += " bit ";
                eventString += Target.ToSafeLink(link, pov);
                eventString += !string.IsNullOrWhiteSpace(InteractionAction) ? InteractionAction : ", passing on the " + Interaction + " ";
            }
            else
            {
                eventString += !string.IsNullOrWhiteSpace(InteractionAction) ? InteractionAction : " put " + Interaction + " on ";
                eventString += Target.ToSafeLink(link, pov);
                eventString += !string.IsNullOrWhiteSpace(InteractionString) ? InteractionString : "";
            }
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}