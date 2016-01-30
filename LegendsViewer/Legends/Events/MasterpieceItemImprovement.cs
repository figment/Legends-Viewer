using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class MasterpieceItemImprovement : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Improver { get; set; }
        public Entity ImproverEntity { get; set; }
        public Site Site { get; set; }
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }
        public string Material { get; set; }
        public string ImprovementType { get; set; }
        public string ImprovementSubType { get; set; }
        public string ImprovementMaterial { get; set; }
        public int ArtID { get; set; }
        public int ArtSubID { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = property.ValueAsInt(); break;
                    case "maker":
                    case "hfid": Improver = world.GetHistoricalFigure(property.ValueAsInt()); Improver.AddEvent(this); break;
                    case "maker_entity":
                    case "entity_id": ImproverEntity = world.GetEntity(property.ValueAsInt()); ImproverEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "skill_used": SkillAtTime = property.ValueAsInt(); break;
                    case "item_type": ItemType = property.Value.Replace("_", " "); break;
                    case "item_subtype": ItemSubType = property.Value.Replace("_", " "); break;
                    case "mat": Material = property.Value.Replace("_", " "); break;
                    case "improvement_type": ImprovementType = property.Value.Replace("_", " "); break;
                    case "improvement_subtype": ImprovementSubType = property.Value.Replace("_", " "); break;
                    case "imp_mat": ImprovementMaterial = property.Value.Replace("_", " "); break;
                    case "art_id": ArtID = property.ValueAsInt(); break;
                    case "art_subid": ArtSubID = property.ValueAsInt(); break;
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
            eventString += Improver.ToSafeLink(link, pov);
            switch (ImprovementType)
            {
                case "art image":
                    eventString += " added a masterful image";
                    break;
                case "covered":
                    eventString += " added a masterful covering";
                    break;
                default:
                    eventString += " added masterful ";
                    if (!string.IsNullOrWhiteSpace(ImprovementSubType) && ImprovementSubType != "-1")
                    {
                        eventString += ImprovementSubType;
                    }
                    else
                    {
                        eventString += !string.IsNullOrWhiteSpace(ImprovementType) ? ImprovementType : "UNKNOWN ITEM";
                    }
                    break;
            }
            eventString += " in ";
            eventString += !string.IsNullOrWhiteSpace(ImprovementMaterial) ? ImprovementMaterial + " " : "";
            eventString += " to a ";
            eventString += !string.IsNullOrWhiteSpace(Material) ? Material + " " : "";
            if (!string.IsNullOrWhiteSpace(ItemSubType) && ItemSubType != "-1")
            {
                eventString += ItemSubType;
            }
            else
            {
                eventString += !string.IsNullOrWhiteSpace(ItemType) ? ItemType : "UNKNOWN ITEM";
            }
            eventString += " for ";
            eventString += ImproverEntity.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}