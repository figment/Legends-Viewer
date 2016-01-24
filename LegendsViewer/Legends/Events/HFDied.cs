using System;
using System.Collections.Generic;
using LegendsViewer.Legends.Enums;
using LegendsViewer.Legends.Parser;

namespace LegendsViewer.Legends.Events
{
    public class HFDied : WorldEvent
    {
        public HistoricalFigure Slayer { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public DeathCause Cause { get; set; }
        private string UnknownCause { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public int SlayerItemID { get; set; }
        public int SlayerShooterItemID { get; set; }
        public string SlayerRace { get; set; }
        public string SlayerCaste { get; set; }

        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }
        public string ItemMaterial { get; set; }
        public Artifact Artifact { get; set; }

        public int ShooterItemID { get; set; }
        public string ShooterItemType { get; set; }
        public string ShooterItemSubType { get; set; }
        public string ShooterItemMaterial { get; set; }
        public Artifact ShooterArtifact { get; set; }

        public HFDied()
        {
            ItemID = -1;
            ShooterItemID = -1;
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            SlayerItemID = -1;
            SlayerShooterItemID = -1;
            SlayerRace = "UNKNOWN";
            SlayerCaste = "UNKNOWN";
            Cause = DeathCause.Unknown;
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "item":
                    case "slayer_item_id": SlayerItemID = property.ValueAsInt(); break;
                    case "slayer_shooter_item_id": SlayerShooterItemID = property.ValueAsInt(); break;
                    case "death_cause":
                    case "cause":
                        switch (property.Value.Replace("_"," "))
                        {
                            case "hunger": Cause = DeathCause.Starved; break;
                            case "struck": Cause = DeathCause.Struck; break;
                            case "struck down": Cause = DeathCause.Struck; break;
                            case "murder":
                            case "murdered": Cause = DeathCause.Murdered; break;
                            case "old age": Cause = DeathCause.OldAge; break;
                            case "dragonfire": Cause = DeathCause.DragonsFire; break;
                            case "shot": Cause = DeathCause.Shot; break;
                            case "fire": Cause = DeathCause.Burned; break;
                            case "thirst": Cause = DeathCause.Thirst; break;
                            case "air": Cause = DeathCause.Suffocated; break;
                            case "blood": Cause = DeathCause.Bled; break;
                            case "cold": Cause = DeathCause.Cold; break;
                            case "crushed bridge": Cause = DeathCause.CrushedByABridge; break;
                            case "drown": Cause = DeathCause.Drowned; break;
                            case "infection": Cause = DeathCause.Infection; break;
                            case "obstacle": Cause = DeathCause.CollidedWithAnObstacle; break;
                            case "put to rest": Cause = DeathCause.PutToRest; break;
                            case "quitdead": Cause = DeathCause.StarvedQuit; break;
                            case "trap": Cause = DeathCause.Trap; break;
                            case "crushed": Cause = DeathCause.CaveIn; break;
                            case "cage blasted": Cause = DeathCause.InACage; break;
                            case "freezing water": Cause = DeathCause.FrozenInWater; break;
                            case "feed to beasts":
                            case "exec fed to beasts": Cause = DeathCause.ExecutedFedToBeasts; break;
                            case "burn alive":
                            case "exec burned alive": Cause = DeathCause.ExecutedBurnedAlive; break;
                            case "exec crucified": Cause = DeathCause.ExecutedCrucified; break;
                            case "drown alt":
                            case "exec drowned": Cause = DeathCause.ExecutedDrowned; break;
                            case "hack to pieces":
                            case "exec hacked to pieces": Cause = DeathCause.ExecutedHackedToPieces; break;
                            case "bury alive":
                            case "exec buried alive": Cause = DeathCause.ExecutedBuriedAlive; break;
                            case "behead":
                            case "exec beheaded": Cause = DeathCause.ExecutedBeheaded; break;
                            case "blood drained": Cause = DeathCause.DrainedBlood; break;
                            case "collapsed": Cause = DeathCause.Collapsed; break;
                            case "scared to death": Cause = DeathCause.ScaredToDeath; break;
                            case "scuttled": Cause = DeathCause.Scuttled; break;
                            case "flying object": Cause = DeathCause.FlyingObject; break;
                            case "slaughtered": Cause = DeathCause.Slaughtered; break;
                            case "melt": Cause = DeathCause.Melted; break;
                            default: Cause = DeathCause.Unknown; UnknownCause = property.Value; world.ParsingErrors.Report("Unknown Death Cause: " + UnknownCause); break;
                        }
                        break;
                    case "slayer_race": SlayerRace = Formatting.FormatRace(property.Value); break;
                    case "slayer_caste": SlayerCaste = property.Value; break;
                    case "victim_hf":
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "slayer_hf":
                    case "slayer_hfid": Slayer = world.GetHistoricalFigure(property.ValueAsInt()); Slayer.AddEvent(this); if (Slayer != null) Slayer.NotableKills.Add(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "item_type": ItemType = property.Value; break;
                    case "item_subtype": ItemType = property.Value; break;
                    case "mat":
                    case "item_material": ItemMaterial = property.Value; break;
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "shooter_item": ShooterItemID = property.ValueAsInt(); break;
                    case "shooter_item_type": ShooterItemType = property.Value; break;
                    case "shooter_item_subtype": ShooterItemSubType = property.Value; break;
                    case "shooter_mat": ShooterItemMaterial = property.Value; break;
                    case "shooter_artifact_id": ShooterArtifact = world.GetArtifact(property.ValueAsInt()); break;
                }
            if (HistoricalFigure.DeathCause == DeathCause.None)
                HistoricalFigure.DeathCause = Cause;
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " ";
            string deathString = "";

            if (Slayer != null || (SlayerRace != "UNKNOWN" && SlayerRace != "-1"))
            {
                string slayerString;
                if (Slayer == null) slayerString = " a " + SlayerRace.ToLower();
                else slayerString = Slayer.ToSafeLink(link, pov);

                if (Cause == DeathCause.DragonsFire) deathString = "burned up in " + slayerString + "'s dragon fire";
                else if (Cause == DeathCause.Burned) deathString = "was burned to death by " + slayerString + "'s fire";
                else if (Cause == DeathCause.Murdered) deathString = "was murdered by " + slayerString;
                else if (Cause == DeathCause.Shot) deathString = "was shot and killed by " + slayerString;
                else if (Cause == DeathCause.Struck) deathString = "was struck down by " + slayerString;
                else if (Cause == DeathCause.ExecutedBuriedAlive) deathString = "was buried alive by " + slayerString;
                else if (Cause == DeathCause.ExecutedBurnedAlive) deathString = "was burned alive by " + slayerString;
                else if (Cause == DeathCause.ExecutedCrucified) deathString = "was crucified by " + slayerString;
                else if (Cause == DeathCause.ExecutedDrowned) deathString = "was drowned by " + slayerString;
                else if (Cause == DeathCause.ExecutedFedToBeasts) deathString = "was fed to beasts by " + slayerString;
                else if (Cause == DeathCause.ExecutedHackedToPieces) deathString = "was hacked to pieces by " + slayerString;
                else if (Cause == DeathCause.ExecutedBeheaded) deathString = "was beheaded by " + slayerString;
                else if (Cause == DeathCause.DrainedBlood) deathString = "was drained of blood by " + slayerString;
                else if (Cause == DeathCause.Collapsed) deathString = "collapsed, struck down by " + slayerString;
                else if (Cause == DeathCause.ScaredToDeath) deathString = " was scared to death by " + slayerString;
                else if (Cause == DeathCause.Bled) deathString = " bled to death, slain by " + slayerString;
                else deathString += ", slain by " + slayerString;
            }
            else
            {
                if (Cause == DeathCause.Thirst) deathString = "died of thirst";
                else if (Cause == DeathCause.OldAge) deathString = "died of old age";
                else if (Cause == DeathCause.Suffocated) deathString = "suffocated";
                else if (Cause == DeathCause.Bled) deathString = "bled to death";
                else if (Cause == DeathCause.Cold) deathString = "froze to death";
                else if (Cause == DeathCause.CrushedByABridge) deathString = "was crushed by a drawbridge";
                else if (Cause == DeathCause.Drowned) deathString = "drowned";
                else if (Cause == DeathCause.Starved) deathString = "starved to death";
                else if (Cause == DeathCause.Infection) deathString = "succumbed to infection";
                else if (Cause == DeathCause.CollidedWithAnObstacle) deathString = "died after colliding with an obstacle";
                else if (Cause == DeathCause.PutToRest) deathString = "was put to rest";
                else if (Cause == DeathCause.StarvedQuit) deathString = "starved";
                else if (Cause == DeathCause.Trap) deathString = "was killed by a trap";
                else if (Cause == DeathCause.CaveIn) deathString = "was crushed under a collapsing ceiling";
                else if (Cause == DeathCause.InACage) deathString = "died in a cage";
                else if (Cause == DeathCause.FrozenInWater) deathString = "was incased in ice";
                else if (Cause == DeathCause.Scuttled) deathString = "was scuttled";
                else if (Cause == DeathCause.Slaughtered) deathString = "was slaughtered";
                else if (Cause == DeathCause.FlyingObject) deathString = "was killed by a flying object";
                else if (Cause == DeathCause.ExecutedBuriedAlive) deathString = "was buried alive";
                else if (Cause == DeathCause.ExecutedBurnedAlive) deathString = "was burned alive";
                else if (Cause == DeathCause.ExecutedCrucified) deathString = "was crucified";
                else if (Cause == DeathCause.ExecutedDrowned) deathString = "was drowned";
                else if (Cause == DeathCause.ExecutedFedToBeasts) deathString = "was fed to beasts";
                else if (Cause == DeathCause.ExecutedHackedToPieces) deathString = "was hacked to pieces";
                else if (Cause == DeathCause.ExecutedBeheaded) deathString = "was beheaded";
                else if (Cause == DeathCause.Melted) deathString = "melted";
                else if (Cause == DeathCause.Unknown) deathString = "died (" + UnknownCause + ")";
            }

            eventString += deathString;

            string slayeritem = null;
            if (!string.IsNullOrEmpty(ItemSubType))
                slayeritem = !string.IsNullOrEmpty(ItemMaterial) ? ItemMaterial + " " + ItemSubType : ItemSubType;
            else if (!string.IsNullOrEmpty(ItemType))
                slayeritem = !string.IsNullOrEmpty(ItemMaterial) ? ItemMaterial + " " + ItemType : ItemType;
            else if (SlayerItemID >= 0)
                slayeritem = "(" + SlayerItemID + ")";
            if (slayeritem != null) eventString += " with a " + slayeritem;
            else if (SlayerShooterItemID >= 0) eventString += " with a (shot) (" + SlayerShooterItemID + ")";
            if (ItemID >= 0)
            {
                if (Artifact != null)
                {
                    eventString += " with " + Artifact.ToSafeLink(link, pov);
                }
                else if (!string.IsNullOrWhiteSpace(ItemType) || !string.IsNullOrWhiteSpace(ItemSubType))
                {
                    eventString += " with a ";
                    eventString += !string.IsNullOrWhiteSpace(ItemMaterial) ? ItemMaterial + " " : " ";
                    eventString += !string.IsNullOrWhiteSpace(ItemSubType) ? ItemSubType : ItemType;
                }
            }
            else if (ShooterItemID >= 0)
            {
                if (ShooterArtifact != null)
                {
                    eventString += " (shot) with " + ShooterArtifact.ToSafeLink(link, pov);
                }
                else if (!string.IsNullOrWhiteSpace(ShooterItemType) || !string.IsNullOrWhiteSpace(ShooterItemSubType))
                {
                    eventString += " (shot) with a ";
                    eventString += !string.IsNullOrWhiteSpace(ShooterItemMaterial) ? ShooterItemMaterial + " " : " ";
                    eventString += !string.IsNullOrWhiteSpace(ShooterItemSubType) ? ShooterItemSubType : ShooterItemType;
                }
            }
            else if (SlayerItemID >= 0) eventString += " with a (" + SlayerItemID + ")";
            else if (SlayerShooterItemID >= 0) eventString += " (shot) with a (" + SlayerShooterItemID + ")";

            if (Site != null) eventString += " in " + Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToSafeLink(link, pov);
            else if (UndergroundRegion != null) eventString += " in " + UndergroundRegion.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
}