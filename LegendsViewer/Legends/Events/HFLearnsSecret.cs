using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFLearnsSecret : WorldEvent
    {
        public HistoricalFigure Student { get; set; }
        public HistoricalFigure Teacher { get; set; }
        public Artifact Artifact { get; set; }
        public string Interaction { get; set; }
        public string SecretText { get; set; }
        public string InteractionDescription { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "student":
                    case "student_hfid": Student = world.GetHistoricalFigure(property.ValueAsInt()); Student.AddEvent(this); break;
                    case "teacher":
                    case "teacher_hfid": Teacher = world.GetHistoricalFigure(property.ValueAsInt()); Teacher.AddEvent(this); break;
                    case "artifact":
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "interaction": Interaction = property.Value; break;
                    case "secret_text": SecretText = property.Value.Replace("[IS_NAME:", "").Replace("]", ""); break;
                }
            }
            InteractionDescription = !string.IsNullOrEmpty(SecretText) ? Formatting.ExtractInteractionString(SecretText) : Interaction;
        }

        
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();

            if (Teacher != null)
            {
                eventString += Teacher.ToSafeLink(link, pov);
                eventString += " taught ";
                eventString += Student.ToSafeLink(link, pov);
                eventString += " ";
                eventString += !string.IsNullOrWhiteSpace(SecretText) ? SecretText : "(" + Interaction + ")";
            }
            else
            {
                eventString += Student.ToSafeLink(link, pov);
                eventString += " learned ";
                eventString += !string.IsNullOrWhiteSpace(SecretText) ? SecretText : "(" + Interaction + ")";
                eventString += " from ";
                eventString += Artifact.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}