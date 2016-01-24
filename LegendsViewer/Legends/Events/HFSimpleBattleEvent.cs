using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFSimpleBattleEvent : WorldEvent
    {
        public HFSimpleBattleType SubType;
        public string UnknownSubType;
        public HistoricalFigure HistoricalFigure1, HistoricalFigure2;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "subtype":
                        switch (property.Value)
                        {
                            case "attacked": SubType = HFSimpleBattleType.Attacked; break;
                            case "scuffle": SubType = HFSimpleBattleType.Scuffle; break;
                            case "confront": SubType = HFSimpleBattleType.Confronted; break;
                            case "2 lost after receiving wounds": SubType = HFSimpleBattleType.HF2LostAfterReceivingWounds; break;
                            case "2 lost after giving wounds": SubType = HFSimpleBattleType.HF2LostAfterGivingWounds; break;
                            case "2 lost after mutual wounds": SubType = HFSimpleBattleType.HF2LostAfterMutualWounds; break;
                            case "happen upon": SubType = HFSimpleBattleType.HappenedUpon; break;
                            case "ambushed": SubType = HFSimpleBattleType.Ambushed; break;
                            case "corner": SubType = HFSimpleBattleType.Cornered; break;
                            case "surprised": SubType = HFSimpleBattleType.Surprised; break;
                            default: SubType = HFSimpleBattleType.Unknown; UnknownSubType = property.Value; world.ParsingErrors.Report("Unknown HF Battle SubType: " + UnknownSubType); break;
                        }
                        break;
                    case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure1.AddEvent(this); break;
                    case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure2.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure1.ToSafeLink(link, pov);
            if (SubType == HFSimpleBattleType.HF2LostAfterGivingWounds)
                eventString = this.GetYearTime() + HistoricalFigure2.ToSafeLink(link, pov) + " was forced to retreat from "
+ HistoricalFigure1.ToSafeLink(link, pov) + " despite the latter's wounds";
            else if (SubType == HFSimpleBattleType.HF2LostAfterMutualWounds)
                eventString += " eventually prevailed and " + HistoricalFigure2.ToSafeLink(link, pov)
+ " was forced to make a hasty escape";
            else if (SubType == HFSimpleBattleType.HF2LostAfterReceivingWounds)
                eventString = this.GetYearTime() + HistoricalFigure2.ToSafeLink(link, pov) + " managed to escape from "
+ HistoricalFigure1.ToSafeLink(link, pov) + "'s onslaught";
            else if (SubType == HFSimpleBattleType.Scuffle) eventString += " fought with " + HistoricalFigure2.ToSafeLink(link, pov) + ". While defeated, the latter escaped unscathed";
            else if (SubType == HFSimpleBattleType.Attacked) eventString += " attacked " + HistoricalFigure2.ToSafeLink(link, pov);
            else if (SubType == HFSimpleBattleType.Confronted) eventString += " confronted " + HistoricalFigure2.ToSafeLink(link, pov);
            else if (SubType == HFSimpleBattleType.HappenedUpon) eventString += " happened upon " + HistoricalFigure2.ToSafeLink();
            else if (SubType == HFSimpleBattleType.Ambushed) eventString += " ambushed " + HistoricalFigure2.ToSafeLink();
            else if (SubType == HFSimpleBattleType.Cornered) eventString += " cornered " + HistoricalFigure2.ToSafeLink();
            else if (SubType == HFSimpleBattleType.Surprised) eventString += " suprised " + HistoricalFigure2.ToSafeLink();
            else eventString += " fought (" + UnknownSubType + ") " + HistoricalFigure2.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

    }
}