using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class ChangedCreatureType : WorldEvent
    {
        public HistoricalFigure Changee, Changer;
        public string OldRace, OldCaste, NewRace, NewCaste;
        public ChangedCreatureType() { }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "old_race": OldRace = Formatting.FormatRace(property.Value); break;
                    case "old_caste": OldCaste = property.Value; break;
                    case "new_race": NewRace = Formatting.FormatRace(property.Value); break;
                    case "new_caste": NewCaste = property.Value; break;
                    case "changee_hfid": Changee = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "changer_hfid": Changer = world.GetHistoricalFigure(property.ValueAsInt()); break;
                }
            Changee.PreviousRace = OldRace;
            Changee.AddEvent(this);
            Changer.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Changer.ToSafeLink(link, pov) + " changed " + Changee.ToSafeLink(link, pov) + " from a " + OldRace + " into a " + NewRace + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}