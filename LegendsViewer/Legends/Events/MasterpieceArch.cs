using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class MasterpieceArch : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public string BuildingType { get; set; }
        public string BuildingSubType { get; set; }
        public int BuildingCustom { get; set; }
        public string Process { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = property.ValueAsInt(); break;
                    case "maker":
                    case "hfid": Maker = world.GetHistoricalFigure(property.ValueAsInt()); Maker.AddEvent(this); break;
                    case "maker_entity":
                    case "entity_id": MakerEntity = world.GetEntity(property.ValueAsInt()); MakerEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "building_type": BuildingType = property.Value; break;
                    case "building_subtype": BuildingSubType = property.Value; break;
                    case "building_custom": BuildingCustom = property.ValueAsInt(); break;
                }
            }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " ";
            eventString += Process;
            eventString += " a masterful ";
            if (!string.IsNullOrWhiteSpace(BuildingSubType) && BuildingSubType != "-1")
            {
                eventString += BuildingSubType;
            }
            else
            {
                eventString += !string.IsNullOrWhiteSpace(BuildingType) ? BuildingType : "UNKNOWN BUILDING";
            }
            eventString += " for ";
            eventString += MakerEntity.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}