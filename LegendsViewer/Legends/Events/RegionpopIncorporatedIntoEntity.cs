using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    //dwarf mode eventsList


    // new 0.42.XX events


    public class RegionpopIncorporatedIntoEntity : WorldEvent
    {
        public Site Site { get; set; }
        public Entity JoinEntity { get; set; }
        public string PopRace { get; set; }
        public int PopNumberMoved { get; set; }
        public WorldRegion PopSourceRegion { get; set; }
        public string PopFlId { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "join_entity_id":
                        JoinEntity = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "pop_race":
                        PopRace = property.Value;
                        break;
                    case "pop_number_moved":
                        PopNumberMoved = property.ValueAsInt();
                        break;
                    case "pop_srid":
                        PopSourceRegion = world.GetRegion(property.ValueAsInt());
                        break;
                    case "pop_flid":
                        PopFlId = property.Value;
                        break;
                }
            Site.AddEvent(this);
            JoinEntity.AddEvent(this);
            PopSourceRegion.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            if (PopNumberMoved > 200)
            {
                eventString += " hundreds of ";
            }
            else if (PopNumberMoved > 24)
            {
                eventString += " dozens of ";
            }
            else
            {
                eventString += " several ";
            }
            eventString += "UNKNOWN RACE";
            eventString += " from ";
            eventString += PopSourceRegion.ToSafeLink(link, pov);
            eventString += " joined with ";
            eventString += JoinEntity.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

}
