﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace LegendsViewer.Legends
{
    public class WorldEvent : IComparable<WorldEvent>
    {
        public int ID { get; set; }
        public int Year { get; set; }
        public int Seconds72 { get; set; }
        public string Type { get; set; }
        public EventCollection ParentCollection { get; set; }
        public World World;
        public WorldEvent(List<Property> properties, World world)
        {
            World = world;
            InternalMerge(properties, world);
        }
        public WorldEvent() { ID = -1; Year = -1; Seconds72 = -1; Type = "INVALID"; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "id": this.ID = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "year": this.Year = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "seconds72": this.Seconds72 = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "type": this.Type = String.Intern(property.Value.Replace('_',' ')); property.Known = true; break;
                    default: break;
                }
        }

        public virtual void Merge(List<Property> properties, World world)
        {
            InternalMerge(properties, world);
        }

        public virtual string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + this.Type;
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

        public int Compare(WorldEvent worldEvent)
        {
            return this.ID.CompareTo(worldEvent.ID);
        }

        public virtual string GetYearTime()
        {
            if (this.Year == -1) return "In a time before time, ";
            string yearTime = "In " + this.Year + ", ";
            if (this.Seconds72 == -1)
                return yearTime;

            int month = this.Seconds72 % 100800;
            if (month <= 33600) yearTime += "early ";
            else if (month <= 67200) yearTime += "mid";
            else if (month <= 100800) yearTime += "late ";

            int season = this.Seconds72 % 403200;
            if (season < 100800) yearTime += "spring, ";
            else if (season < 201600) yearTime += "summer, ";
            else if (season < 302400) yearTime += "autumn, ";
            else if (season < 403200) yearTime += "winter, ";

            int monthIndex = this.Seconds72 / (28 * 1200);
            string[] monthNames = { "Granite", "Slate", "Felsite", "Hematite", "Malachite", "Galena", "Limestone", "Sandstone", "Timber", "Moonstone", "Opal", "Obsidian" };
            string monthName = monthNames[monthIndex];
            int dayIndex = 1 + (this.Seconds72 % (28 * 1200)) / 1200;

            return yearTime + " (" + monthName + ", " + dayIndex.ToString() + ") ";
        }
        public string PrintParentCollection(bool link = true, DwarfObject pov = null)
        {
            EventCollection parent = ParentCollection;
            string collectionString = "";
            while (parent != null)
            {
                if (collectionString.Length > 0) collectionString += " as part of ";
                collectionString += parent.ToLink(link, pov);
                parent = parent.ParentCollection;
            }

            if (collectionString.Length > 0)
                return "In " + collectionString + ". ";
            else
                return collectionString;
        }

        public int CompareTo(object obj)
        {
            return this.ID.CompareTo(obj);
        }

        public int CompareTo(WorldEvent other)
        {
            return this.ID.CompareTo(other.ID);
        }
    }

    public class AddHFEntityLink : WorldEvent
    {
        public Entity Entity;
        public HistoricalFigure HistoricalFigure;
        public string LinkType;
        public string Position;
        public AddHFEntityLink() { }
        public AddHFEntityLink(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); Entity.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "link_type": LinkType = string.Intern(property.Value); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + " ";
            if (HistoricalFigure != null) eventString += HistoricalFigure.ToLink(link, pov);
            else eventString += "UNKNOWN HISTORICAL FIGURE";
            if (LinkType == "imprison") eventString += " was imprisoned by ";
            else if (LinkType == "enemy") eventString += " became an enemy of ";
            else if (LinkType == "position") { eventString += " became a " + Position + " of ";}
            else eventString += " linked to ";
            eventString += Entity.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class AddHFHFLink : WorldEvent
    {
        public HistoricalFigure HistoricalFigure, HistoricalFigureTarget;
        public HistoricalFigureLinkType LinkType;
        public bool LinkTypeSet;
        public AddHFHFLink() { }
        public AddHFHFLink(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hf":
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "hf_target":
                    case "hfid_target": HistoricalFigureTarget = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "link_type":
                        if (!Enum.TryParse(property.Value, true, out LinkType))
                            LinkType = HistoricalFigureLinkType.Unknown;
                        LinkTypeSet = property.Known = true;
                        break;
                }
            if (!LinkTypeSet || LinkType == HistoricalFigureLinkType.Unknown)
            { 
                //Fill in LinkType by looking at related historical figures.
                LinkType = HistoricalFigureLinkType.Unknown;
                if (HistoricalFigure != HistoricalFigure.Unknown && HistoricalFigureTarget != HistoricalFigure.Unknown)
                {
                    List<HistoricalFigureLink> historicalFigureToTargetLinks = HistoricalFigure.RelatedHistoricalFigures.Where(link => link.Type != HistoricalFigureLinkType.Child).Where(link => link.HistoricalFigure == HistoricalFigureTarget).ToList();
                    HistoricalFigureLink historicalFigureToTargetLink = null;
                    if (historicalFigureToTargetLinks.Count <= 1)
                        historicalFigureToTargetLink = historicalFigureToTargetLinks.FirstOrDefault();
                    HFAbducted abduction = HistoricalFigureTarget.Events.OfType<HFAbducted>().SingleOrDefault(abduction1 => abduction1.Snatcher == HistoricalFigure);
                    if (historicalFigureToTargetLink != null && abduction == null)
                        LinkType = historicalFigureToTargetLink.Type;
                    else if (abduction != null)
                        LinkType = HistoricalFigureLinkType.Prisoner;
                    else if (HistoricalFigure.Race == "Night Creature" || HistoricalFigureTarget.Race == "Night Creature")
                    {
                        LinkType = HistoricalFigureLinkType.Spouse;
                        HistoricalFigure.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigureTarget, HistoricalFigureLinkType.ExSpouse));
                        HistoricalFigureTarget.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigure, HistoricalFigureLinkType.ExSpouse));
                    }
                }
            }

            HistoricalFigure.AddEvent(this);
            HistoricalFigureTarget.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();

            if (pov == HistoricalFigureTarget)
                eventString += HistoricalFigureTarget.ToLink(link, pov);
            else
                eventString += HistoricalFigure.ToLink(link, pov);

            switch (LinkType)
            {
                case HistoricalFigureLinkType.Apprentice:
                    if (pov == HistoricalFigureTarget)
                        eventString += " began an apprenticeship under ";
                    else
                        eventString += " became the master of ";
                    break;
                case HistoricalFigureLinkType.Master:
                    if (pov == HistoricalFigureTarget)
                        eventString += " became the master of ";
                    else
                        eventString += " began an apprenticeship under ";
                    break;
                case HistoricalFigureLinkType.FormerApprentice:
                    if (pov == HistoricalFigureTarget)
                        eventString += " ceased being the apprentice of ";
                    else
                        eventString += " ceased being the master of ";
                    break;
                case HistoricalFigureLinkType.FormerMaster:
                    if (pov == HistoricalFigureTarget)
                        eventString += " ceased being the master of ";
                    else
                        eventString += " ceased being the apprentice of ";
                    break;
                case HistoricalFigureLinkType.Deity:
                    if (pov == HistoricalFigureTarget)
                        eventString += " received the worship of ";
                    else
                        eventString += " began worshipping ";
                    break;
                case HistoricalFigureLinkType.Lover:
                    eventString += " became romantically involved with ";
                    break;
                case HistoricalFigureLinkType.Prisoner:
                    if (pov == HistoricalFigureTarget)
                        eventString += " was imprisoned by ";
                    else
                        eventString += " imprisoned ";
                    break;
                case HistoricalFigureLinkType.Spouse:
                    eventString += " married ";
                    break;
                case HistoricalFigureLinkType.Unknown:
                    eventString += " linked (UNKNOWN) to ";
                    break;
                default:
                    throw new Exception("Unhandled Link Type in AddHFHFLink: " + LinkType.GetDescription());
            }

            if (pov == HistoricalFigureTarget)
                eventString += HistoricalFigure.ToLink(link, pov);
            else
                eventString += HistoricalFigureTarget.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ArtifactLost : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public Site Site { get; set; }
        public ArtifactLost() { }
        public ArtifactLost(List<Property> properties, World world) : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact":
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); Artifact.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Artifact.ToLink(link, pov) + " was lost in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ArtifactPossessed : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public int UnitID { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }

        public ArtifactPossessed() { }
        public ArtifactPossessed(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); break;
                    case "unit_id": UnitID = Convert.ToInt32(property.Value); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            Artifact.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Artifact.ToLink(link, pov) + " was claimed";
            if (Site != null)
                eventString += " in " + Site.ToLink(link, pov);
            eventString += " by " + HistoricalFigure.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ArtifactStored : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public int UnitID { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }

        public ArtifactStored() { }
        public ArtifactStored(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); Artifact.AddEvent(this); break;
                    case "unit_id": UnitID = Convert.ToInt32(property.Value); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Artifact.ToLink(link, pov) + " was stored in " + Site.ToLink(link, pov) + " by " + HistoricalFigure.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ArtifactDestroyed : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public Site Site { get; set; }
        public HistoricalFigure Destroyer { get; set; }

        public ArtifactDestroyed() { }
        public ArtifactDestroyed(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); Artifact.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "destroyer_enid": Destroyer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Destroyer.AddEvent(this); break;
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
            eventString += Artifact.ToLink(link, pov);
            eventString += " was destroyed by ";
            eventString += Destroyer.ToLink(link, pov);
            eventString += " in ";
            eventString += Site.ToLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

    public class AssumeIdentity : WorldEvent
    {
        public HistoricalFigure Trickster { get; set; }
        public HistoricalFigure Identity { get; set; }
        public Entity Target { get; set; }

        public AssumeIdentity() { }
        public AssumeIdentity(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Trickster.AddEvent(this); break;
                    case "identity_id": property.Known = true; Identity = HistoricalFigure.Unknown; Identity.AddEvent(this); break; //Bad ID, so unknown for now.
                    case "target_enid": Target = world.GetEntity(Convert.ToInt32(property.Value)); Target.AddEvent(this); break;
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
            string eventString = GetYearTime() + Trickster.ToLink(link, pov) + " fooled " + Target.ToLink(link, pov) + " into believing " + Trickster.CasteNoun() + " was " + Identity.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class AttackedSite : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity;
        public Site Site;
        public HistoricalFigure AttackerGeneral, DefenderGeneral;
        public AttackedSite() { }
        public AttackedSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); Attacker.AddEvent(this); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); Defender.AddEvent(this); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); SiteEntity.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); AttackerGeneral.AddEvent(this); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); DefenderGeneral.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.PrintEntity(true, pov) + " attacked ";
            if (SiteEntity != null) eventString += SiteEntity.PrintEntity(true, pov);
            else eventString += Defender.PrintEntity(true, pov);
            eventString += " at " + Site.ToLink(link, pov) + ". ";
            if (AttackerGeneral != null)
                eventString += AttackerGeneral.ToLink(link, pov) + " led the attack";
            if (DefenderGeneral != null)
                eventString += ", and the defenders were led by " + DefenderGeneral.ToLink(link, pov);
            else eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class BodyAbused : WorldEvent
    {
        public Entity Abuser;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        public Entity Bodies;
        public Entity Civ;
        public HistoricalFigure HistoricalFigure;
        public int AbuseType, PropsPileType;
        public string PropItemType, PropItemSubItem;

        public BodyAbused() { }
        public BodyAbused(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "bodies": Bodies = world.GetEntity(Convert.ToInt32(property.Value)); Bodies.AddEvent(this); break;
                    case "civ": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "abuse_type": AbuseType = Convert.ToInt32(property.Value); break;
                    case "props_item_type": PropItemType = string.Intern(property.Value); break;
                    case "props_item_subtype": PropItemSubItem = string.Intern(property.Value); break;
                    case "props_item_mat": property.Known = true; break;
                    case "props_item_mat_type": property.Known = true; break;
                    case "props_item_mat_index": property.Known = true; break;
                    case "props_pile_type": PropsPileType = Convert.ToInt32(property.Value); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string body = Bodies != null ? Bodies.ToLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
            string histFig = HistoricalFigure != null ? HistoricalFigure.ToLink(link, pov) : "UNKNOWN ENTITY";
            string eventString = this.GetYearTime() + body + "'s body was abused by " + histFig;
            if (Site != null) eventString += " in " + this.Site.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ChangeHFBodyState : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public BodyState BodyState { get; set; }
        public Site Site { get; set; }
        public int BuildingID { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        private string UnknownBodyState;

        public ChangeHFBodyState() { }
        public ChangeHFBodyState(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "body_state":
                        switch (property.Value)
                        {
                            case "entombed at site": BodyState = BodyState.EntombedAtSite; break;
                            default:
                                BodyState = BodyState.Unknown;
                                UnknownBodyState = property.Value;
                                world.ParsingErrors.Report("Unknown HF Body State: " + UnknownBodyState);
                                break;
                        }
                        break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "building_id": BuildingID = Convert.ToInt32(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov) + " ";
            string stateString = "";
            switch (BodyState)
            {
                case BodyState.EntombedAtSite: stateString = "was entombed"; break;
                case BodyState.Unknown: stateString = "(" + UnknownBodyState + ")"; break;
            }
            eventString += stateString;
            if (Region != null)
                eventString += " in " + Region.ToLink(link, pov);
            if (Site != null)
                eventString += " at " + Site.ToLink(link, pov);
            if (BuildingID != -1)
                eventString += " within (" + BuildingID + ")";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum BodyState
    {
        EntombedAtSite,
        Unknown
    }

    public class ChangeHFJob : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public string OldJob, NewJob;

        public ChangeHFJob() { }
        public ChangeHFJob(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "old_job": OldJob = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                    case "new_job": NewJob = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov);
            if (!string.IsNullOrEmpty(NewJob))
            {
                if (!string.IsNullOrEmpty(OldJob))
                    eventString += $" changed jobs from {OldJob} to {NewJob}";
                else
                    eventString += $" became a {NewJob}";
            }
            else if (!string.IsNullOrEmpty(OldJob))
                eventString += " stopped working as a " + OldJob;
            else 
                eventString += " became a UNKNOWN JOB";
            if (Site != null)
            {
                eventString += " in " + Site.ToLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum HFState : byte
    {
        None,
        Settled,
        Wandering,
        Scouting,
        Snatcher,
        Refugee,
        Thief,
        Hunting,
        Visiting,
        Unknown
    }
    public class ChangeHFState : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        public HFState State;
        private string UnknownState;

        public ChangeHFState() { }
        public ChangeHFState(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "state":
                        switch (property.Value)
                        {
                            case "settled": State = HFState.Settled; break;
                            case "wandering": State = HFState.Wandering; break;
                            case "scouting": State = HFState.Scouting; break;
                            case "snatcher": State = HFState.Snatcher; break;
                            case "refugee": State = HFState.Refugee; break;
                            case "thief": State = HFState.Thief; break;
                            case "hunting": State = HFState.Hunting; break;
                            case "visiting": State = HFState.Visiting; break;
                            default: State = HFState.Unknown; UnknownState = property.Value; world.ParsingErrors.Report("Unknown HF State: " + UnknownState); break;
                        }
                        break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "hfid":
                        HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                        if (HistoricalFigure != null && HistoricalFigure.AddEvent(this))
                            HistoricalFigure.States.Add(new HistoricalFigure.State(State, Year));
                        break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                }
            if (HistoricalFigure != null)
            {
                HistoricalFigure.State lastState = HistoricalFigure.States.LastOrDefault();
                if (lastState != null) lastState.EndYear = Year;
                HistoricalFigure.CurrentState = State;
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov);
            if (State == HFState.Settled) eventString += " settled in ";
            else if (State == HFState.Refugee || State == HFState.Snatcher || State == HFState.Thief) eventString += " became a " + State.ToString().ToLower() + " in ";
            else if (State == HFState.Wandering) eventString += " began wandering ";
            else if (State == HFState.Scouting) eventString += " began scouting the area around ";
            else if (State == HFState.Hunting) eventString += " began hunting great beasts in ";
            else
            {
                eventString += " " + UnknownState + " in ";
            }
            if (Site != null) eventString += Site.ToLink(link, pov);
            else if (Region != null) eventString += Region.ToLink(link, pov);
            else if (UndergroundRegion != null) eventString += UndergroundRegion.ToLink(link, pov);
            else eventString += "the wilds";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ChangedCreatureType : WorldEvent
    {
        public HistoricalFigure Changee, Changer;
        public string OldRace, OldCaste, NewRace, NewCaste;
        public ChangedCreatureType() { }
        public ChangedCreatureType(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "old_race": OldRace = Formatting.FormatRace(property.Value); break;
                    case "old_caste": OldCaste = property.Value; break;
                    case "new_race": NewRace = Formatting.FormatRace(property.Value); break;
                    case "new_caste": NewCaste = property.Value; break;
                    case "changee_hfid": Changee = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "changer_hfid": Changer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
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
            string eventString = this.GetYearTime() + Changer.ToLink(link, pov) + " changed " + Changee.ToLink(link, pov) + " from a " + OldRace + " into a " + NewRace + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class CreateEntityPosition : WorldEvent
    {
        public Entity Civ, SiteCiv;
        public HistoricalFigure HistoricalFigure;
        public int Reason;
        public string Position;

        public CreateEntityPosition() { }
        public CreateEntityPosition(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); break;
                    case "site_civ": SiteCiv = world.GetEntity(Convert.ToInt32(property.Value)); SiteCiv.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "reason": Reason = Convert.ToInt32(property.Value); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            if (!string.IsNullOrEmpty(Position))
            { 
                string eventString = this.GetYearTime() + " ";
                eventString += $"Position {Position} was created for ";
                eventString += (HistoricalFigure != null) ? HistoricalFigure.ToLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
                eventString += $" of {Civ.ToLink(link, pov)}. ";
                eventString += PrintParentCollection(link, pov);
                return eventString;
            }
            else
            {
                return base.Print(link, pov);
            }
        }
    }

    public class CreatedSite : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public HistoricalFigure Builder;
        public CreatedSite() { }
        public CreatedSite(List<Property> properties, World world)
            : base(properties, world)
        { 
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "builder_hfid": Builder = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                }
            if (SiteEntity != null)
            {
                SiteEntity.Parent = Civ;
                new OwnerPeriod(Site, SiteEntity, this.Year, "founded");
            }
            else if (Civ != null)
            {
                new OwnerPeriod(Site, Civ, this.Year, "founded");
            }
            else if (Builder != null)
            {
                new OwnerPeriod(Site, Builder, this.Year, "created");
            }
            Site.AddEvent(this);
            SiteEntity.AddEvent(this);
            Civ.AddEvent(this);
            Builder.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (Builder != null)
            {
                eventString += Builder.ToLink(link, pov) + " created " + Site.ToLink(link, pov) + ". ";
            }
            else
            {
                if (SiteEntity != null) eventString += SiteEntity.ToLink(link, pov) + " of ";
                eventString += Civ.ToLink(link, pov) + " founded " + Site.ToLink(link, pov) + ". ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class CreatedWorldConstruction : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site1, Site2;
        public int WorldConstructionID, MasterWorldConstructionID;

        public CreatedWorldConstruction() { }
        public CreatedWorldConstruction(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id1": Site1 = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "site_id2": Site2 = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "wcid": WorldConstructionID = Convert.ToInt32(property.Value); break;
                    case "master_wcid": MasterWorldConstructionID = Convert.ToInt32(property.Value); break;
                }
            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site1.AddEvent(this);
            Site2.AddEvent(this);

            Site1.AddConnection(Site2);
            Site2.AddConnection(Site1);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + SiteEntity.ToLink(link, pov) + " of " + Civ.ToLink(link, pov) + " finished the construction of UNKNOWN CONSTRUCTION connecting " + Site1.ToLink(link, pov) + " and " + Site2.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class CreatureDevoured : WorldEvent
    {
        public HistoricalFigure Eater, Victim;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public string Race, Caste;

        public CreatureDevoured() { }
        public CreatureDevoured(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "victim": property.Known = true; break;
                    case "entity": Victim = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Victim.AddEvent(this); break;
                    case "eater": Eater = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Eater.AddEvent(this); break;
                    case "race": Race = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "caste": Caste = Formatting.InitCaps(property.Value); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eater = Eater != null ? Eater.ToLink(link, pov) : Race ?? "UNKNOWN HISTORICAL FIGURE";
            string victim = Victim != null ? Victim.ToLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
            string eventString = this.GetYearTime();
            eventString += $"{eater} devoured {victim} in ";
            if (Site != null) eventString += Site.ToLink(link, pov);
            else if (Region != null) eventString += Region.ToLink(link, pov);
            else if (UndergroundRegion != null) eventString += UndergroundRegion.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

    }
    public class DestroyedSite : WorldEvent
    {
        public Site Site;
        public Entity SiteEntity, Attacker, Defender;
        public DestroyedSite() { }
        public DestroyedSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                }

            if (Site.OwnerHistory.Count == 0)
                if (SiteEntity != null && SiteEntity != Defender)
                {
                    SiteEntity.Parent = Defender;
                    new OwnerPeriod(Site, SiteEntity, 1, "UNKNOWN");
                }
                else
                    new OwnerPeriod(Site, Defender, 1, "UNKNOWN");

            Site.OwnerHistory.Last().EndCause = "destroyed";
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().Ender = Attacker;

            Site.AddEvent(this);
            if (SiteEntity != Defender)
                SiteEntity.AddEvent(this);
            Attacker.AddEvent(this);
            Defender.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += Defender.ToLink(link, pov) + " and destroyed " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class FieldBattle : WorldEvent
    {
        public Entity Attacker, Defender;
        public WorldRegion Region;
        public HistoricalFigure AttackerGeneral, DefenderGeneral;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        public FieldBattle() { }
        public FieldBattle(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            AttackerGeneral.AddEvent(this);
            DefenderGeneral.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " attacked " + Defender.ToLink(link, pov) + " in " + Region.ToLink(link, pov) + ". " +
                AttackerGeneral.ToLink(link, pov) + " led the attack, and the defenders were led by " + DefenderGeneral.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class HFAbducted : WorldEvent
    {
        public HistoricalFigure Target { get; set; }
        public HistoricalFigure Snatcher { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public HFAbducted() { }
        public HFAbducted(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "target_hfid": Target = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "snatcher_hfid": Snatcher = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            Target.AddEvent(this);
            Snatcher.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (Snatcher != null)
                eventString += Snatcher.ToLink(link, pov);
            else
                eventString += "(UNKNOWN HISTORICAL FIGURE)";
            eventString += " abducted " + Target.ToLink(link, pov) + " from " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFConfronted : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public ConfrontSituation Situation { get; set; }
        public List<ConfrontReason> Reasons { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        private string UnknownSituation;
        private List<string> UnknownReasons;

        public HFConfronted() { }
        public HFConfronted(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            Reasons = new List<ConfrontReason>();
            UnknownReasons = new List<string>();
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "situation":
                        switch (property.Value)
                        {
                            case "general suspicion": Situation = ConfrontSituation.GeneralSuspicion; break;
                            default:
                                Situation = ConfrontSituation.Unknown;
                                UnknownSituation = property.Value;
                                world.ParsingErrors.Report("Unknown HF Confronted Situation: " + UnknownSituation);
                                break;
                        }
                        break;
                    case "reason":
                        switch (property.Value)
                        {
                            case "murder": Reasons.Add(ConfrontReason.Murder); break;
                            case "ageless": Reasons.Add(ConfrontReason.Ageless); break;
                            default:
                                Reasons.Add(ConfrontReason.Unknown);
                                UnknownReasons.Add(property.Value);
                                world.ParsingErrors.Report("Unknown HF Confronted Reason: " + property.Value);
                                break;
                        }
                        break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                }
            }

            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov);
            string situationString = "";
            switch (Situation)
            {
                case ConfrontSituation.GeneralSuspicion: situationString = "aroused general suspicion"; break;
                case ConfrontSituation.Unknown: situationString = "(" + UnknownSituation + ")"; break;
            }
            eventString += " " + situationString;

            if (Region != null)
                eventString += " in " + Region.ToLink(link, pov);

            if (Site != null)
                eventString += " at " + Site.ToLink(link, pov);

            string reasonString = "after ";
            int unknownReasonIndex = 0;
            foreach (ConfrontReason reason in Reasons)
            {
                switch (reason)
                {
                    case ConfrontReason.Murder: reasonString += "a murder"; break;
                    case ConfrontReason.Ageless: reasonString += "appearing not to age"; break;
                    case ConfrontReason.Unknown:
                        reasonString += "(" + UnknownReasons[unknownReasonIndex++] + ")";
                        break;
                }

                if (reason != Reasons.Last() && Reasons.Count > 2)
                    reasonString += ",";
                reasonString += " ";
                if (Reasons.Count > 1 && reason == Reasons[Reasons.Count - 2])
                    reasonString += "and ";
            }
            eventString += " " + reasonString + ". ";

            eventString += PrintParentCollection(link, pov);

            return eventString;
        }
    }

    public enum ConfrontSituation
    {
        GeneralSuspicion,
        Unknown
    }

    public enum ConfrontReason
    {
        Ageless,
        Murder,
        Unknown
    }

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
        public string ItemType, ItemSubType, ItemMaterial;

        public HFDied() { }
        public HFDied(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            SlayerItemID = -1;
            SlayerShooterItemID = -1;
            SlayerRace = "UNKNOWN";
            SlayerCaste = "UNKNOWN";
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "item":
                    case "slayer_item_id": SlayerItemID = Convert.ToInt32(property.Value); break;
                    case "slayer_shooter_item_id": SlayerShooterItemID = Convert.ToInt32(property.Value); break;
                    case "death_cause":
                    case "cause":
                        switch (property.Value)
                        {
                            case "hunger": Cause = DeathCause.Starved; break;
                            case "struck": Cause = DeathCause.Struck; break;
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
                            case "exec fed to beasts": Cause = DeathCause.ExecutedFedToBeasts; break;
                            case "exec burned alive": Cause = DeathCause.ExecutedBurnedAlive; break;
                            case "exec crucified": Cause = DeathCause.ExecutedCrucified; break;
                            case "exec drowned": Cause = DeathCause.ExecutedDrowned; break;
                            case "exec hacked to pieces": Cause = DeathCause.ExecutedHackedToPieces; break;
                            case "exec buried alive": Cause = DeathCause.ExecutedBuriedAlive; break;
                            case "exec beheaded": Cause = DeathCause.ExecutedBeheaded; break;
                            case "blood drained": Cause = DeathCause.DrainedBlood; break;
                            case "collapsed": Cause = DeathCause.Collapsed; break;
                            case "scared to death": Cause = DeathCause.ScaredToDeath; break;
                            case "scuttled": Cause = DeathCause.Scuttled; break;
                            default: Cause = DeathCause.Unknown; UnknownCause = property.Value; world.ParsingErrors.Report("Unknown Death Cause: " + UnknownCause); break;
                        }
                        break;
                    case "slayer_race": SlayerRace = Formatting.FormatRace(property.Value); break;
                    case "slayer_caste": SlayerCaste = property.Value; break;
                    case "victim_hf":
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "slayer_hf":
                    case "slayer_hfid": Slayer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                    case "item_type": ItemType = property.Value; break;
                    case "item_subtype": ItemType = property.Value; break;
                    case "mat": 
                    case "item_material": ItemMaterial = property.Value; break;
                }
            HistoricalFigure.AddEvent(this);
            if (HistoricalFigure.DeathCause == DeathCause.None)
                HistoricalFigure.DeathCause = Cause;
            if (Slayer != null)
            {
                Slayer.AddEvent(this);
                Slayer.NotableKills.Add(this);
            }
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " ";
            string deathString = "";
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
            else if (Cause == DeathCause.Unknown) deathString = "died (" + UnknownCause + ")";

            if (Slayer != null || SlayerRace != "UNKNOWN")
            {
                string slayerString;
                if (Slayer == null) slayerString = " a " + SlayerRace.ToLower();
                else slayerString = Slayer.ToLink(link, pov);

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
                else deathString += ", slain by " + slayerString;
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

            if (Site != null) eventString += " in " + Site.ToLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToLink(link, pov);
            else if (UndergroundRegion != null) eventString += " in " + UndergroundRegion.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

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

        public HFDoesInteraction() { }
        public HFDoesInteraction(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "doer":
                    case "doer_hfid": Doer = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Doer.AddEvent(this); break;
                    case "target":
                    case "target_hfid": Target = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Target.AddEvent(this); break;
                    case "interaction": Interaction = property.Value; break;
                    case "interaction_action": InteractionAction = property.Value; break;
                    case "interaction_string": InteractionString = property.Value; break;
                    case "source": property.Known = true; break;
                    case "site": Site = world.GetSite(Convert.ToInt32(property.Value)); property.Known = true; Site.AddEvent(this); break;
                    case "region": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
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
        }


        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Doer.ToLink(link, pov) + InteractionDescription + " on " + Target.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFGainsSecretGoal : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public SecretGoal Goal { get; set; }
        private string UnknownGoal;

        public HFGainsSecretGoal() { }
        public HFGainsSecretGoal(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "secret_goal":
                        switch (property.Value)
                        {
                            case "immortality": Goal = SecretGoal.Immortality; break;
                            default:
                                Goal = SecretGoal.Unknown;
                                UnknownGoal = property.Value;
                                world.ParsingErrors.Report("Unknown Secret Goal: " + UnknownGoal);
                                break;
                        }
                        break;
                }
            }

            HistoricalFigure.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov);
            string goalString = "";
            switch (Goal)
            {
                case SecretGoal.Immortality: goalString = " became obsessed with " + HistoricalFigure.CasteNoun(true) + " own mortality and sought to extend " + HistoricalFigure.CasteNoun(true) + " life by any means"; break;
                case SecretGoal.Unknown: goalString = " gained secret goal (" + UnknownGoal + ")"; break;
            }
            eventString += goalString + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum SecretGoal
    {
        Immortality,
        Unknown
    }

    public class HFLearnsSecret : WorldEvent
    {
        public HistoricalFigure Student { get; set; }
        public HistoricalFigure Teacher { get; set; }
        public Artifact Artifact { get; set; }
        public string Interaction { get; set; }
        public string SecretText { get; set; }
        public string InteractionDescription { get; set; }

        public HFLearnsSecret() { }
        public HFLearnsSecret(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "student":
                    case "student_hfid": Student = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Student.AddEvent(this); break;
                    case "teacher":
                    case "teacher_hfid": Teacher = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Teacher.AddEvent(this); break;
                    case "artifact":
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); Artifact.AddEvent(this); break;
                    case "interaction": Interaction = property.Value; break;
                    case "secret_text": SecretText = property.Value; break;
                }
            }
            InteractionDescription = !string.IsNullOrEmpty(SecretText) ? Formatting.ExtractInteractionString(SecretText) : Interaction;
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();

            if (Teacher != null)
                eventString += Teacher.ToLink(link, pov) + " taught " + Student.ToLink(link, pov) + " " + InteractionDescription;
            else
                eventString += Student.ToLink(link, pov) + $" learned {InteractionDescription} from " + Artifact.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFNewPet : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;
        public string PetType;

        public HFNewPet() { PetType = "UNKNOWN"; }
        public HFNewPet(List<Property> properties, World world)
            : base(properties, world)
        {
            PetType = "UNKNOWN";
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "group":
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); property.Known = true; Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "pets": PetType = Formatting.InitCaps(property.Value); property.Known = true; break;
                }            
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " tamed " + PetType;
            if (Site != null) eventString += " at " + Site.ToLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToLink(link, pov);
            else if (UndergroundRegion != null) eventString += " in " + UndergroundRegion.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFProfanedStructure : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        
        public HFProfanedStructure() { }
        public HFProfanedStructure(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "histfig":
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "action": property.Known = true; break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov) + " profaned at ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFReunion : WorldEvent
    {
        public HistoricalFigure HistoricalFigure1, HistoricalFigure2;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        public HFReunion() { }
        public HFReunion(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure1.AddEvent(this);
            HistoricalFigure2.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + " " + HistoricalFigure1.ToLink(link, pov) + " was reunited with " + HistoricalFigure2.ToLink(link, pov);
            if (Site != null) eventString += " in " + Site.ToLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum HFSimpleBattleType : byte
    {
        [Description("2nd HF Lost After Giving Wounds")]
        HF2LostAfterGivingWounds,
        [Description("2nd HF Lost After Mutual Wounds")]
        HF2LostAfterMutualWounds,
        [Description("2nd HF Lost After Recieving Wounds")]
        HF2LostAfterReceivingWounds,
        Attacked,
        Scuffle,
        Confronted,
        Ambushed,
        [Description("Happened Upon")]
        HappenedUpon,
        Cornered,
        Surprised,
        Unknown
    }

    public class HFSimpleBattleEvent : WorldEvent
    {
        public HFSimpleBattleType SubType;
        public string UnknownSubType;
        public HistoricalFigure HistoricalFigure1, HistoricalFigure2;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public HFSimpleBattleEvent() { }
        public HFSimpleBattleEvent(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
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
                    case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure1.AddEvent(this);
            HistoricalFigure2.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure1.ToLink(link, pov);
            if (SubType == HFSimpleBattleType.HF2LostAfterGivingWounds) eventString = this.GetYearTime() + HistoricalFigure2.ToLink(link, pov) + " was forced to retreat from "
                + HistoricalFigure1.ToLink(link, pov) + " despite the latter's wounds";
            else if (SubType == HFSimpleBattleType.HF2LostAfterMutualWounds) eventString += " eventually prevailed and " + HistoricalFigure2.ToLink(link, pov)
                + " was forced to make a hasty escape";
            else if (SubType == HFSimpleBattleType.HF2LostAfterReceivingWounds) eventString = this.GetYearTime() + HistoricalFigure2.ToLink(link, pov) + " managed to escape from "
                + HistoricalFigure1.ToLink(link, pov) + "'s onslaught";
            else if (SubType == HFSimpleBattleType.Scuffle) eventString += " fought with " + HistoricalFigure2.ToLink(link, pov) + ". While defeated, the latter escaped unscathed";
            else if (SubType == HFSimpleBattleType.Attacked) eventString += " attacked " + HistoricalFigure2.ToLink(link, pov);
            else if (SubType == HFSimpleBattleType.Confronted) eventString += " confronted " + HistoricalFigure2.ToLink(link, pov);
            else if (SubType == HFSimpleBattleType.HappenedUpon) eventString += " happened upon " + HistoricalFigure2.ToLink();
            else if (SubType == HFSimpleBattleType.Ambushed) eventString += " ambushed " + HistoricalFigure2.ToLink();
            else if (SubType == HFSimpleBattleType.Cornered) eventString += " cornered " + HistoricalFigure2.ToLink();
            else if (SubType == HFSimpleBattleType.Surprised) eventString += " suprised " + HistoricalFigure2.ToLink();
            else eventString += " fought (" + UnknownSubType + ") " + HistoricalFigure2.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

    }
    public class HFTravel : WorldEvent
    {
        public Location Coordinates;
        public bool Escaped, Returned;
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public HFTravel() { }
        public HFTravel(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "escape": Escaped = true; property.Known = true; break;
                    case "return": Returned = true; property.Known = true; break;
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov);
            if (Escaped) return this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " escaped from the " + UndergroundRegion.ToLink(link, pov);
            else if (Returned) eventString += " returned to ";
            else eventString += " made a journey to ";

            if (UndergroundRegion != null) eventString += UndergroundRegion.ToLink(link, pov);
            else if (Site != null) eventString += Site.ToLink(link, pov);
            else if (Region != null) eventString += Region.ToLink(link, pov);

            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class HFWounded : WorldEvent
    {
        public HistoricalFigure Woundee, Wounder;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public string WoundeeRace, WoundeeCaste;
        public string BodyPart, InjuryType, PartLost;

        public HFWounded() { }
        public HFWounded(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "woundee":
                    case "woundee_hfid": Woundee = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Woundee.AddEvent(this); break;
                    case "wounder":
                    case "wounder_hfid": Wounder = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Wounder.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); UndergroundRegion.AddEvent(this); break;
                    case "woundee_race": WoundeeRace = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "woundee_caste": WoundeeCaste = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "body_part": BodyPart = string.Intern(property.Value); property.Known = true; break;
                    case "injury_type": InjuryType = string.Intern(property.Value); property.Known = true; break;
                    case "part_lost": PartLost = string.Intern(property.Value); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (Woundee != null)
                eventString += Woundee.ToLink(link, pov);
            else
                eventString += "(UNKNOWN HISTORICAL FIGURE)";
            eventString += " was wounded (UNKNOWN) by ";
            if (Wounder != null)
                eventString += Wounder.ToLink(link, pov);
            else
                eventString += "(UNKNOWN HISTORICAL FIGURE)";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ImpersonateHF : WorldEvent
    {
        public HistoricalFigure Trickster, Cover;
        public Entity Target;

        public ImpersonateHF() { }
        public ImpersonateHF(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "cover_hfid": Cover = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "target_enid": Target = world.GetEntity(Convert.ToInt32(property.Value)); break;
                }
            Trickster.AddEvent(this);
            Cover.AddEvent(this);
            Target.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Trickster.ToLink(link, pov) + " fooled " + Target.ToLink(link, pov)
                + " into believing he/she was a manifestation of the deity " + Cover.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ItemStolen : WorldEvent
    {
        public HistoricalFigure Thief;
        public Site Site, ReturnSite;
        public string ItemType, Material;
        public int SubType;
        public Entity Entity;
        public int StructureID;
        public Structure Structure;

        public ItemStolen() { }
        public ItemStolen(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "item_type": ItemType = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                    case "mat": Material = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                    case "mattype": property.Known = true; break;
                    case "matindex": property.Known = true; break;
                    case "item_subtype": SubType = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "histfig": Thief = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Thief.AddEvent(this); break;
                    case "entity": Entity = world.GetEntity(Convert.ToInt32(property.Value)); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool path = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            string itemType = ItemType ?? "UNKNOWN ITEM";
            string site = Site != null ? Site.ToLink(path, pov) : "UNKNOWN SITE";
            string thief = Thief != null ? Thief.ToLink(path, pov) : "UNKNOWN HISTORICAL FIGURE";

            eventString += $" a {itemType} was stolen from {site} by {thief}";
            if (Entity != null) eventString += " from " + Entity.ToLink(path, pov);
            if (ReturnSite != null) eventString += "and brought to " + ReturnSite.ToLink();
            eventString += ". ";
            eventString += PrintParentCollection(path, pov);
            return eventString;
        }
    }

    public class NewSiteLeader : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity, NewSiteEntity;
        public Site Site;
        public HistoricalFigure NewLeader;

        public NewSiteLeader() { }
        public NewSiteLeader(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "new_site_civ_id": NewSiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "new_leader_hfid": NewLeader = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                }

            if (Site.OwnerHistory.Count == 0)
                if (SiteEntity != null && SiteEntity != Defender)
                {
                    SiteEntity.Parent = Defender;
                    new OwnerPeriod(Site, SiteEntity, 1, "UNKNOWN");
                }
                else
                    new OwnerPeriod(Site, Defender, 1, "UNKNOWN");

            Site.OwnerHistory.Last().EndCause = "taken over";
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().Ender = Attacker;
            NewSiteEntity.Parent = Attacker;
            new OwnerPeriod(Site, NewSiteEntity, this.Year, "took over");

            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            if (SiteEntity != Defender)
                SiteEntity.AddEvent(this);
            Site.AddEvent(this);
            NewSiteEntity.AddEvent(this);
            NewLeader.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += Defender.ToLink(link, pov) + " and placed " + NewLeader.ToLink(link, pov) + " in charge of " + Site.ToLink(link, pov) + ". The new government was called " + NewSiteEntity.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class PeaceAccepted : WorldEvent
    {
        public Site Site;
        public string Topic;
        public Entity Source, Destination;

        public PeaceAccepted() { }
        public PeaceAccepted(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); property.Known = true; break;
                    case "topic": Topic = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "source": Source = world.GetEntity(Convert.ToInt32(property.Value)); Source.AddEvent(this); property.Known = true; break;
                    case "destination": Destination = world.GetEntity(Convert.ToInt32(property.Value)); Source.AddEvent(this); property.Known = true; break;
                }
            Site.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            if (Source != null && Destination != null && Site != null)
                return $"{GetYearTime()}Peace accepted by {Destination.ToLink(link, pov)} in offer from {Source.ToLink(link, pov)} at {Site.ToLink(link, pov)}.";
            return $"{GetYearTime()}Peace accepted in {ParentCollection.ToLink(link, pov)}.";
        }
    }
    public class PeaceRejected : WorldEvent
    {
        public Site Site;
        public string Topic;
        public Entity Source, Destination;

        public PeaceRejected() { }
        public PeaceRejected(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); property.Known = true; break;
                    case "topic": Topic = Formatting.InitCaps(property.Value); property.Known = true; break;
                    case "source": Source = world.GetEntity(Convert.ToInt32(property.Value)); Source.AddEvent(this); property.Known = true; break;
                    case "destination": Destination = world.GetEntity(Convert.ToInt32(property.Value)); Source.AddEvent(this); property.Known = true; break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            if (Source != null && Destination != null && Site != null)
                return $"{GetYearTime()}Peace rejected by {Destination.ToLink(link, pov)} in offer from {Source.ToLink(link, pov)} at {Site.ToLink(link, pov)}.";
            return $"{GetYearTime()}Peace rejected in {ParentCollection.ToLink(link, pov)}.";
        }
    }
    public class PlunderedSite : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity;
        public Site Site;
        public PlunderedSite() { }
        public PlunderedSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            if (Defender != SiteEntity) SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " defeated ";
            if (SiteEntity != null && Defender != SiteEntity) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += Defender.ToLink(link, pov) + " and pillaged " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class RazedStructure : WorldEvent
    {
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        public RazedStructure() { }
        public RazedStructure(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;

                }
            }

            Entity.AddEvent(this);
            Site.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Entity.ToLink(link, pov) + " razed ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ReclaimSite : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public ReclaimSite() { }
        public ReclaimSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            if (SiteEntity != null && SiteEntity != Civ)
                SiteEntity.Parent = Civ;

            //Make sure period was lost by an event, otherwise unknown loss
            if (Site.OwnerHistory.Count == 0)
                new OwnerPeriod(Site, null, 1, "UNKNOWN");
            if (Site.OwnerHistory.Last().EndYear == -1)
            {
                Site.OwnerHistory.Last().EndCause = "UNKNOWN";
                Site.OwnerHistory.Last().EndYear = this.Year - 1;
            }
            new OwnerPeriod(Site, SiteEntity, this.Year, "reclaimed");

            Civ.AddEvent(this);
            if (SiteEntity != Civ)
                SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += Civ.ToLink(link, pov) + " launched an expedition to reclaim " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class RemoveHFEntityLink : WorldEvent
    {
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public string LinkType, Position;

        public RemoveHFEntityLink() { }
        public RemoveHFEntityLink(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); property.Known = true; break;
                    case "link_type": LinkType = string.Intern(property.Value); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                }

        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string histfig = HistoricalFigure == null ? "UNKNOWN HISTORICAL FIGURE" : HistoricalFigure.ToLink(link, pov);
            string eventString = this.GetYearTime() + histfig + " removed link with " + Civ.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


    //dwarf mode eventsList

    public class ArtifactCreated : WorldEvent
    {
        public int UnitID;
        public Artifact Artifact;
        public bool RecievedName;
        public HistoricalFigure HistoricalFigure;
        public Site Site;

        public ArtifactCreated() { }
        public ArtifactCreated(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "unit_id": UnitID = Convert.ToInt32(property.Value); property.Known = true; break;
                    case "artifact_id": Artifact = world.GetArtifact(Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "hfid":
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "site": 
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "name_only": RecievedName = true; property.Known = true; break;
                }
            Artifact.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Artifact.ToLink(link, pov);
            if (RecievedName)
                eventString += " recieved its name";
            else
                eventString += " was created";
            if (Site != null)
                eventString += " in " + Site.ToLink(link, pov);
            if (RecievedName)
                eventString += " from ";
            else
                eventString += " by ";
            eventString += HistoricalFigure.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

    }

    public class DiplomatLost : WorldEvent
    {
        public Site Site;
        public DiplomatLost() { }
        public DiplomatLost(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + "UNKNOWN ENTITY lost a diplomat at " + Site.ToLink(link, pov)
                + ". They suspected the involvement of UNKNOWN ENTITY. ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class EntityAction 
    {
        public static void DelegateUpsertEvent(List<WorldEvent> events, List<Property> properties, World world)
        {
            var action = properties.Where(x => x.Name == "action").Select(x => new int?(System.Convert.ToInt32(x.Value))).FirstOrDefault();
            if (action.HasValue && action.Value > -1)
            {
                switch (action)
                {
                    case 0: World.UpsertEvent<EntityPrimaryCriminals>(events, properties, world); break;
                    case 1: World.UpsertEvent<EntityRelocate>(events, properties, world); break;
                }
            }
        }
    }

    public class HFActOnBuilding
    {
        public static void DelegateUpsertEvent(List<WorldEvent> events, List<Property> properties, World world)
        {
            var action = properties.Where(x => x.Name == "action").Select(x => new int?(System.Convert.ToInt32(x.Value))).FirstOrDefault();
            if (action.HasValue && action.Value > -1)
            {
                switch (action)
                {
                    case 0: World.UpsertEvent<HFProfanedStructure>(events, properties, world); break;
                    //case 1: World.UpsertEvent<HFDisturbedStructure>(events, properties, world); break;
                }
            }
        }
    }

    public class EntityCreated : WorldEvent
    {
        public Entity Entity;
        public Site Site;
        public EntityCreated() { }
        public EntityCreated(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;

                    //Unhandled Events
                    case "structure_id": property.Known = true; break;
                }
            Entity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += Entity.ToLink(link, pov) + " formed in ";
            eventString += (Site != null ? Site.ToLink(link, pov) : "UNKNOWN SITE") + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class EntityLaw : WorldEvent
    {
        public Entity Entity { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public EntityLawType Law { get; set; }
        public bool LawLaid { get; set; }
        private string UnknownLawType;

        public EntityLaw() { }
        public EntityLaw(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "law_add":
                    case "law_remove":
                        switch (property.Value)
                        {
                            case "harsh": Law = EntityLawType.Harsh; break;
                            default:
                                Law = EntityLawType.Unknown;
                                UnknownLawType = property.Value;
                                world.ParsingErrors.Report("Unknown Law Type: " + UnknownLawType);
                                break;
                        }
                        LawLaid = property.Name == "law_add";
                        break;
                }
            }

            Entity.AddEvent(this);
            HistoricalFigure.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToLink(link, pov);
            if (LawLaid)
                eventString += " laid a series of ";
            else
                eventString += " lifted numerous ";
            switch (Law)
            {
                case EntityLawType.Harsh: eventString += "oppresive"; break;
                case EntityLawType.Unknown: eventString += "(" + UnknownLawType + ")"; break;
            }
            if (LawLaid)
                eventString += " edicts upon ";
            else
                eventString += " laws from ";
            eventString += Entity.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum EntityLawType
    {
        Harsh,
        Unknown
    }

    public class EntityPrimaryCriminals : WorldEvent
    {
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        public EntityPrimaryCriminals() { }
        public EntityPrimaryCriminals(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity":
                    case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); Entity.AddEvent(this); property.Known = true; break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); property.Known = true; break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "action": property.Known = true; break;
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
            string eventString = GetYearTime() + Entity.ToLink(link, pov) + " became the primary criminal organization in " + Site.ToLink();
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class EntityRelocate : WorldEvent
    {
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        public EntityRelocate() { }
        public EntityRelocate(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity":
                    case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); Entity.AddEvent(this); property.Known = true; break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); property.Known = true; break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "action": property.Known = true; break;
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
            string eventString = GetYearTime() + Entity.ToLink(link, pov) + " moved to ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFRevived : WorldEvent
    {
        private string Ghost;
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public HFRevived() { }
        public HFRevived(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ghost": Ghost = property.Value; break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " came back from the dead as a " + Ghost + " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceArchDesign : WorldEvent
    {
        public int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceArchDesign() { }
        public MasterpieceArchDesign(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " constructed a masterful (UNKNOWN) for " + Civ.ToLink(link, pov) +
                " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceArchConstructed : WorldEvent
    {
        public int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceArchConstructed() { }
        public MasterpieceArchConstructed(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " designed a masterful (UNKNOWN) for " + Civ.ToLink(link, pov) +
                " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }

    }

    public class MasterpieceEngraving : WorldEvent
    {
        private int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceEngraving() { }
        public MasterpieceEngraving(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + "created a masterful engraving for" + Civ.ToLink(link, pov) +
                " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceFood : WorldEvent
    {
        private int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceFood() { }
        public MasterpieceFood(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " prepared a masterful (UNKNOWN) for " + Civ.ToLink(link, pov) +
                " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceItem : WorldEvent
    {
        private int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceItem() { }
        public MasterpieceItem(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " created a masterful (UNKNOWN) for " + Civ.ToLink(link, pov) +
                " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceItemImprovement : WorldEvent
    {
        private int SkillAtTime;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public Site Site;
        public MasterpieceItemImprovement() { }
        public MasterpieceItemImprovement(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = Convert.ToInt32(property.Value); break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "entity_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " added masterful (UNKNOWN) to a (UNKNOWN) for "
                + Civ.ToLink(link, pov) + " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceLost : WorldEvent
    {
        public MasterpieceLost() { }
        public MasterpieceLost(List<Property> properties, World world)
            : base(properties, world)
        {
        }
    }

    public class Merchant : WorldEvent
    {
        public Merchant() { }
        public Merchant(List<Property> properties, World world)
            : base(properties, world)
        {
        }
    }

    public class SiteAbandoned : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public SiteAbandoned() { }
        public SiteAbandoned(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().EndCause = "abandoned";
            if (SiteEntity != null)
            {
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";
            }
            Civ.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
            Civ.SiteHistory.Last(s => s.Site == Site).EndCause = "abandoned";

            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += Civ.ToLink(link, pov) + " abandoned the settlement at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class SiteDied : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public Boolean Abandoned;
        public SiteDied() { }
        public SiteDied(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "abandoned":
                        property.Known = true;
                        Abandoned = true;
                        break;
                }

            string endCause = "withered";
            if (Abandoned)
                endCause = "abandoned";

            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().EndCause = endCause;
            if (SiteEntity != null)
            {
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
                SiteEntity.SiteHistory.Last(s => s.Site == Site).EndCause = endCause;
            }
            Civ.SiteHistory.Last(s => s.Site == Site).EndYear = this.Year;
            Civ.SiteHistory.Last(s => s.Site == Site).EndCause = endCause;

            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + SiteEntity.PrintEntity(link, pov);
            if (Abandoned)
            {
                eventString += "abandoned the settlement of " + Site.ToLink(link, pov) + ". ";
            }
            else
            {
                eventString += " settlement of " + Site.ToLink(link, pov) + " withered. ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


    public class AddHFSiteLink : WorldEvent
    {
        public Site Site;
        public int StructureID;
        public Structure Structure;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public string LinkType;

        public AddHFSiteLink() { }
        public AddHFSiteLink(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); property.Known = true; Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); HistoricalFigure.AddEvent(this); property.Known = true; break;
                    case "civ": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); property.Known = true; break;
                    case "link_type": LinkType = string.Intern(Formatting.InitCaps(property.Value)); property.Known = true; break;
                }

        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            if (HistoricalFigure != null)
            {
                if (Structure.Name != null && Site.Name != null)
                {
                    string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " linked to " +
                                         Structure.Name + " in " + Site.ToLink(link, pov) + ". ";
                    eventString += PrintParentCollection(link, pov);
                    return eventString;
                }
                else
                {
                    string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " linked to " + Site.ToLink(link, pov) + ". ";
                    eventString += PrintParentCollection(link, pov);
                    return eventString;
                }
            }
            else
            {
                string eventString = this.GetYearTime() + "UNKNOWN HISTORICAL FIGURE linked to " + Site.ToLink(link, pov) + ". ";
                eventString += PrintParentCollection(link, pov);
                return eventString;
            }
        }
    }

    public class AgreementMade : WorldEvent
    {
        Site Site;
        public AgreementMade() { }
        public AgreementMade(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            Site.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + "UNKNOWN AGREEMENT proposed by UNKNOWN ENTITY was accepted by UNKNOWN ENTITY at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class CreatedStructure : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public Entity Civ, SiteEntity;
        public Site Site;
        public HistoricalFigure Builder;

        public CreatedStructure() { }
        public CreatedStructure(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); break;
                    case "site_civ": 
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); SiteEntity.AddEvent(this); break;
                    case "site": 
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "builder_hf": 
                    case "builder_hfid": Builder = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); Builder.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (Builder != null)
            {
                eventString += Builder.ToLink(link, pov) + ", thrust a spire of slade up from the underworld, naming it (UNKNOWN), and established a gateway between worlds in "
                    + Site.ToLink(link, pov) + ". ";
            }
            else
            {
                if (SiteEntity != null) eventString += SiteEntity.ToLink(link, pov) + " of ";
                eventString += Civ.ToLink(link, pov) + " constructed ";
                eventString += Structure != null ? Structure.Name : $"({StructureID})";
                eventString += " in " + Site.ToLink(link, pov) + ". ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFRazedStructure : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public HFRazedStructure() { }
        public HFRazedStructure(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToLink(link, pov) + " razed a ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class RemoveHFSiteLink : WorldEvent
    {
        public Site Site;
        public Structure Structure;
        public int StructureID;
        public HistoricalFigure HistoricalFigure;
        public Entity Civ;
        public string LinkType;

        public RemoveHFSiteLink() { }
        public RemoveHFSiteLink(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = Convert.ToInt32(property.Value)); property.Known = true; break;
                    case "histfig":
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); property.Known = true; HistoricalFigure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); property.Known = true; break;
                    case "link_type": LinkType = Formatting.InitCaps(property.Value); property.Known = true; break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string hf = HistoricalFigure != null ? HistoricalFigure.ToLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
            string type = LinkType ?? "link";
            string eventString = $"{GetYearTime()}{hf} removed {type} to ";
            if (Structure != null) eventString += Structure.Name + " at ";
            eventString += Site.ToLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ReplacedStructure : WorldEvent
    {
        public int OldABID, NewABID;
        public Structure OldStructure, NewStructure;
        public Entity Civ, SiteEntity;
        public Site Site;
        public ReplacedStructure() { }
        public ReplacedStructure(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "old_structure":
                    case "old_ab_id": OldStructure = Site?.GetStructure(OldABID = Convert.ToInt32(property.Value)); break;
                    case "new_structure":
                    case "new_ab_id": NewStructure = Site?.GetStructure(NewABID = Convert.ToInt32(property.Value)); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); Civ.AddEvent(this); break;
                    case "site_civ":
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); SiteEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string oldstruct = OldStructure == null ? $"({OldABID})" : OldStructure.Name;
            string newstruct = NewStructure == null ? $"({NewABID})" : NewStructure.Name;
            string eventString = this.GetYearTime();
            if (SiteEntity != null) eventString += SiteEntity.ToLink(link, pov) + " of ";
            eventString += $"{Civ.ToLink(link, pov)} replaced a {oldstruct} in {Site.ToLink(link, pov)} with a {newstruct} ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class SiteTakenOver : WorldEvent
    {
        public Entity Attacker, Defender, NewSiteEntity, SiteEntity;
        public Site Site;
        public SiteTakenOver() { }
        public SiteTakenOver(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "new_site_civ_id": NewSiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }

            if (Site.OwnerHistory.Count == 0)
                if (SiteEntity != null && SiteEntity != Defender)
                {
                    SiteEntity.Parent = Defender;
                    new OwnerPeriod(Site, SiteEntity, 1, "UNKNOWN");
                }
                else
                    new OwnerPeriod(Site, Defender, 1, "UNKNOWN");

            Site.OwnerHistory.Last().EndCause = "taken over";
            Site.OwnerHistory.Last().EndYear = this.Year;
            Site.OwnerHistory.Last().Ender = Attacker;
            NewSiteEntity.Parent = Attacker;
            new OwnerPeriod(Site, NewSiteEntity, this.Year, "took over");

            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            NewSiteEntity.AddEvent(this);
            if (SiteEntity != Defender)
                SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToLink(link, pov) + " of ";
            if (Defender == null)
            {
                eventString += "UNKNOWN";
            }
            else {
                eventString += Defender.ToLink(link, pov);
            }
            eventString += " and took over " + Site.ToLink(link, pov) +
                ". The new government was called " + NewSiteEntity.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum Dispute
    {
        FishingRights,
        GrazingRights,
        LivestockOwnership,
        RightsOfWay,
        Territory,
        WaterRights,
        Unknown
    }

    public class SiteDispute : WorldEvent
    {
        public Dispute Dispute { get; set; }
        public Entity Entity1 { get; set; }
        public Entity Entity2 { get; set; }
        public Site Site1 { get; set; }
        public Site Site2 { get; set; }
        private string unknownDispute;

        public SiteDispute() { }
        public SiteDispute(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "dispute":
                        switch (property.Value)
                        {
                            case "fishing rights": Dispute = Dispute.FishingRights; break;
                            case "grazing rights": Dispute = Dispute.GrazingRights; break;
                            case "livestock ownership": Dispute = Dispute.LivestockOwnership; break;
                            case "territory": Dispute = Dispute.Territory; break;
                            case "water rights": Dispute = Dispute.WaterRights; break;
                            case "rights-of-way": Dispute = Dispute.RightsOfWay; break;
                            default:
                                Dispute = Dispute.Unknown;
                                unknownDispute = property.Value;
                                world.ParsingErrors.Report("Unknown Site Dispute: " + unknownDispute);
                                break;
                        }
                        break;
                    case "entity_id_1": Entity1 = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "entity_id_2": Entity2 = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id_1": Site1 = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "site_id_2": Site2 = world.GetSite(Convert.ToInt32(property.Value)); break;
                }

            Entity1.AddEvent(this);
            Entity2.AddEvent(this);
            Site1.AddEvent(this);
            Site2.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String dispute = unknownDispute;
            switch (Dispute)
            {
                case Dispute.FishingRights: dispute = "fishing rights"; break;
                case Dispute.GrazingRights: dispute = "grazing rights"; break;
                case Dispute.LivestockOwnership: dispute = "livestock ownership"; break;
                case Dispute.Territory: dispute = "territory"; break;
                case Dispute.WaterRights: dispute = "water rights"; break;
                case Dispute.RightsOfWay: dispute = "rights of way"; break;
            }

            String eventString = this.GetYearTime() + Entity1.ToLink(link, pov) + " of "
                 + Site1.ToLink(link, pov) + " and " + Entity2.ToLink(link, pov)
                 + " of " + Site2.ToLink(link, pov) + " became embroiled in a dispute over "
                 + dispute + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HfAttackedSite : WorldEvent
    {
        HistoricalFigure Attacker { get; set; }
        Entity DefenderCiv { get; set; }
        Entity SiteCiv { get; set; }
        Site Site { get; set; }

        public HfAttackedSite() { }
        public HfAttackedSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {

                    case "attacker_hfid": Attacker = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": DefenderCiv = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteCiv = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }

            Attacker.AddEvent(this);
            DefenderCiv.AddEvent(this);
            SiteCiv.AddEvent(this);
            Site.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " attacked "
                + SiteCiv.ToLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToLink(link, pov);
            }
            eventString += " at " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HfDestroyedSite : WorldEvent
    {
        HistoricalFigure Attacker { get; set; }
        Entity DefenderCiv { get; set; }
        Entity SiteCiv { get; set; }
        Site Site { get; set; }

        public HfDestroyedSite() { }
        public HfDestroyedSite(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {

                    case "attacker_hfid": Attacker = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": DefenderCiv = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteCiv = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }

            Attacker.AddEvent(this);
            DefenderCiv.AddEvent(this);
            SiteCiv.AddEvent(this);
            Site.AddEvent(this);

            OwnerPeriod lastSiteOwnerPeriod = Site.OwnerHistory.LastOrDefault();
            if (lastSiteOwnerPeriod != null)
            {
                lastSiteOwnerPeriod.EndYear = this.Year;
                lastSiteOwnerPeriod.EndCause = "destroyed";
                lastSiteOwnerPeriod.Ender = Attacker;
            }
            if (DefenderCiv != null)
            {
                OwnerPeriod lastDefenderCivOwnerPeriod = DefenderCiv.SiteHistory.LastOrDefault(s => s.Site == Site);
                if (lastDefenderCivOwnerPeriod != null)
                {
                    lastDefenderCivOwnerPeriod.EndYear = this.Year;
                    lastDefenderCivOwnerPeriod.EndCause = "destroyed";
                    lastDefenderCivOwnerPeriod.Ender = Attacker;
                }
            }
            OwnerPeriod lastSiteCiveOwnerPeriod = SiteCiv.SiteHistory.LastOrDefault(s => s.Site == Site);
            if (lastSiteCiveOwnerPeriod != null)
            {
                lastSiteCiveOwnerPeriod.EndYear = this.Year;
                lastSiteCiveOwnerPeriod.EndCause = "destroyed";
                lastSiteCiveOwnerPeriod.Ender = Attacker;
            }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " routed "
                + SiteCiv.ToLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToLink(link, pov);
            }
            eventString += " and destroyed " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class AgreementFormed : WorldEvent
    {
        String AgreementId { get; set; }
        public AgreementFormed() { }
        public AgreementFormed(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "agreement_id": AgreementId = property.Value; break;
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
            String eventString = this.GetYearTime() + " an unknown agreement was formed (" + AgreementId + "). ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class SiteTributeForced : WorldEvent
    {
        public Entity Attacker { get; set; }
        public Entity Defender { get; set; }
        public Entity SiteEntity { get; set; }
        public Site Site { get; set; }

        public SiteTributeForced() { }
        public SiteTributeForced(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "defender_civ_id": Defender = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                }
            }

            Attacker.AddEvent(this);
            Defender.AddEvent(this);
            SiteEntity.AddEvent(this);
            Site.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Attacker.ToLink(link, pov) + " secured tribute from " + SiteEntity.ToLink(link, pov);
            if (Defender != null)
            {
                eventString += " of " + Defender.ToLink(link, pov);
            }
            eventString += ", to be delivered from " + Site.ToLink(link, pov) + ". ";
            eventString += PrintParentCollection();
            return eventString;
        }
    }


    public enum InsurrectionOutcome
    {
        LeadershipOverthrown,
        PopulationGone,
        Unknown
    }

    public class InsurrectionStarted : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public InsurrectionOutcome Outcome { get; set; }
        public Boolean ActualStart { get; set; }
        private string unknownOutcome;

        public InsurrectionStarted() { }
        public InsurrectionStarted(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            ActualStart = false;

            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "target_civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
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

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            if (ActualStart)
            {
                eventString += "an insurrection against " + Civ.ToLink(link, pov)
                               + " began in " + Site.ToLink(link, pov) + ". ";
            }
            else
            {
                eventString += "the insurrection in " + Site.ToLink(link, pov);
                switch (Outcome)
                {
                    case InsurrectionOutcome.LeadershipOverthrown:
                        eventString += " concluded with " + Civ.ToLink(link, pov) + " overthrowing leadership. ";
                        break;
                    case InsurrectionOutcome.PopulationGone:
                        eventString += " ended with the disappearance of the rebelling population. ";
                        break;
                    default:
                        eventString += " against " + Civ.ToLink(link, pov) + " concluded with (" + unknownOutcome + "). ";
                        break;
                }
            }

            eventString += PrintParentCollection();
            return eventString;
        }
    }

    // new 0.42.XX events

    public enum OccasionType
    {
        Procession,
        Ceremony,
        Performance,
        Competition
    }

    public class OccasionEvent : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public int OccasionId { get; set; }
        public int ScheduleId { get; set; }
        public OccasionType OccasionType { get; set; }

        public OccasionEvent() { }
        public OccasionEvent(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                    case "occasion_id": OccasionId = Convert.ToInt32(property.Value); break;
                    case "schedule_id": ScheduleId = Convert.ToInt32(property.Value); break;
                }
            Civ.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Civ.ToLink(link, pov);
            eventString += " held a " + OccasionType.ToString().ToLower() + " in ";
            eventString += Site.ToLink(link, pov);
            eventString += " as part of UNKNOWN OCCASION (" + OccasionId + ") with UNKNOWN SCHEDULE(" + ScheduleId + ")";
            eventString += ".";
            return eventString;
        }
    }

    public class Procession : OccasionEvent
    {
        public Procession() { }
        public Procession(List<Property> properties, World world)
            : base(properties, world)
        {
            OccasionType = OccasionType.Procession;
        }
    }

    public class Ceremony : OccasionEvent
    {
        public Ceremony() { }
        public Ceremony(List<Property> properties, World world)
            : base(properties, world)
        {
            OccasionType = OccasionType.Ceremony;
        }
    }

    public class Performance : OccasionEvent
    {
        public Performance() { }
        public Performance(List<Property> properties, World world)
            : base(properties, world)
        {
            OccasionType = OccasionType.Performance;
        }
    }

    public class Competition : OccasionEvent
    {
        HistoricalFigure Winner { get; set; }
        List<HistoricalFigure> Competitors { get; set; }
        public Competition() { }
        public Competition(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            OccasionType = OccasionType.Competition;
            Competitors = new List<HistoricalFigure>();
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "winner_hfid": Winner = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "competitor_hfid": Competitors.Add(world.GetHistoricalFigure(Convert.ToInt32(property.Value))); break;
                }
            Winner.AddEvent(this);
            Competitors.ForEach(competitor => competitor.AddEvent(this));
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = base.Print(link, pov);
            if (Competitors.Any())
            {
                eventString += "</br>";
                eventString += "Competing were ";
                for (int i = 0; i < Competitors.Count; i++)
                {
                    HistoricalFigure competitor = Competitors.ElementAt(i);
                    if (i == 0)
                    {
                        eventString += competitor.ToLink(link, pov);
                    }
                    else if (i == Competitors.Count - 1)
                    {
                        eventString += " and " + competitor.ToLink(link, pov);
                    }
                    else
                    {
                        eventString += ", " + competitor.ToLink(link, pov);
                    }
                }
                eventString += ". ";
            }
            if (Winner != null)
            {
                eventString += "The winner was ";
                eventString += Winner.ToLink(link, pov);
                eventString += ".";
            }
            return eventString;
        }
    }


    public enum FormType
    {
        Musical,
        Poetic,
        Dance
    }

    public class FormCreatedEvent : WorldEvent
    {
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public string FormId { get; set; }
        public string Reason { get; set; }
        public int ReasonId { get; set; }
        public HistoricalFigure GlorifiedHF { get; set; }
        public HistoricalFigure PrayToHF { get; set; }
        public string Circumstance { get; set; }
        public int CircumstanceId { get; set; }
        public FormType FormType { get; set; }

        public FormCreatedEvent() { }
        public FormCreatedEvent(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "form_id": FormId = property.Value; break;
                    case "reason": Reason = property.Value; break;
                    case "reason_id": ReasonId = Convert.ToInt32(property.Value); break;
                    case "circumstance": Circumstance = property.Value; break;
                    case "circumstance_id": CircumstanceId = Convert.ToInt32(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                }
            Site.AddEvent(this);
            Region.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            if (Reason == "glorify hf")
            {
                GlorifiedHF = world.GetHistoricalFigure(ReasonId);
                GlorifiedHF.AddEvent(this);
            }
            if (Circumstance == "pray to hf")
            {
                PrayToHF = world.GetHistoricalFigure(CircumstanceId);
                PrayToHF.AddEvent(this);
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += "UNKNOWN";
            switch (FormType)
            {
                case FormType.Musical:
                    eventString += " MUSICAL FORM ";
                    break;
                case FormType.Poetic:
                    eventString += " POETIC FORM ";
                    break;
                case FormType.Dance:
                    eventString += " DANCE FORM ";
                    break;
                default:
                    eventString += " FORM ";
                    break;
            }
            eventString += " was created by ";
            eventString += HistoricalFigure.ToLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToLink(link, pov);
            }
            if (GlorifiedHF != null)
            {
                eventString += " in order to glorify " + GlorifiedHF.ToLink(link, pov);
            }
            if (!string.IsNullOrWhiteSpace(Circumstance))
            {
                if (PrayToHF != null)
                {
                    eventString += " after praying to " + PrayToHF.ToLink(link, pov);
                }
                else
                {
                    eventString += " after a " + Circumstance;
                }
            }
            eventString += ".";
            return eventString;
        }
    }

    public class PoeticFormCreated : FormCreatedEvent
    {
        public PoeticFormCreated() { }
        public PoeticFormCreated(List<Property> properties, World world)
            : base(properties, world)
        {
            FormType = FormType.Poetic;
        }
    }

    public class MusicalFormCreated : FormCreatedEvent
    {
        public MusicalFormCreated() { }
        public MusicalFormCreated(List<Property> properties, World world)
            : base(properties, world)
        {
            FormType = FormType.Musical;
        }
    }

    public class DanceFormCreated : FormCreatedEvent
    {
        public DanceFormCreated() { }
        public DanceFormCreated(List<Property> properties, World world)
            : base(properties, world)
        {
            FormType = FormType.Dance;
        }
    }

    public class WrittenContentComposed : WorldEvent
    {
        public Entity Civ { get; set; }
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public string WrittenContent { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public string Reason { get; set; }
        public int ReasonId { get; set; }
        public HistoricalFigure GlorifiedHF { get; set; }
        public HistoricalFigure CircumstanceHF { get; set; }
        public string Circumstance { get; set; }
        public int CircumstanceId { get; set; }

        public WrittenContentComposed() { }
        public WrittenContentComposed(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "wc_id": WrittenContent = property.Value; break;
                    case "reason": Reason = property.Value; break;
                    case "reason_id": ReasonId = Convert.ToInt32(property.Value); break;
                    case "circumstance": Circumstance = property.Value; break;
                    case "circumstance_id": CircumstanceId = Convert.ToInt32(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                }
            Civ.AddEvent(this);
            Site.AddEvent(this);
            Region.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            if (Reason == "glorify hf")
            {
                GlorifiedHF = world.GetHistoricalFigure(ReasonId);
                GlorifiedHF.AddEvent(this);
            }
            if (Circumstance == "pray to hf" ||
                Circumstance == "dream about hf")
            {
                CircumstanceHF = world.GetHistoricalFigure(CircumstanceId);
                CircumstanceHF.AddEvent(this);
            }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += "UNKNOWN WRITTEN CONTENT";
            eventString += " was authored by ";
            eventString += HistoricalFigure.ToLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToLink(link, pov);
            }
            if (GlorifiedHF != null)
            {
                eventString += " in order to glorify " + GlorifiedHF.ToLink(link, pov);
            }
            if (!string.IsNullOrWhiteSpace(Circumstance))
            {
                if (CircumstanceHF != null)
                {
                    switch (Circumstance)
                    {
                        case "pray to hf":
                            eventString += " after praying to " + CircumstanceHF.ToLink(link, pov);
                            break;
                        case "dream about hf":
                            eventString += " after dreaming of " + CircumstanceHF.ToLink(link, pov);
                            break;
                    }
                }
                else
                {
                    eventString += " after a " + Circumstance;
                }
            }
            eventString += ".";
            return eventString;
        }
    }


    public class KnowledgeDiscovered : WorldEvent
    {
        public string[] Knowledge { get; set; }
        public bool First { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }

        public KnowledgeDiscovered() { }
        public KnowledgeDiscovered(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "knowledge": Knowledge = property.Value.Split(':'); break;
                    case "first": First = true; property.Known = true; break;
                }
            HistoricalFigure.AddEvent(this);
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToLink(link, pov);
            if (First)
            {
                eventString += " was the first to discover ";
            }
            else
            {
                eventString += " independently discovered ";
            }
            if (Knowledge.Length > 1)
            {
                eventString += " the " + Knowledge[1];
                if (Knowledge.Length > 2)
                {
                    eventString += " (" + Knowledge[2] + ")";
                }
                eventString += " in the field of " + Knowledge[0] + ".";
            }
            return eventString;
        }
    }

    public class HFRelationShipDenied : WorldEvent
    {
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public HistoricalFigure Seeker { get; set; }
        public HistoricalFigure Target { get; set; }
        public string Relationship { get; set; }
        public string Reason { get; set; }
        public HistoricalFigure ReasonHF { get; set; }

        public HFRelationShipDenied() { }
        public HFRelationShipDenied(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
                    case "seeker_hfid": Seeker = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "target_hfid": Target = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                    case "relationship": Relationship = property.Value; break;
                    case "reason": Reason = property.Value; break;
                    case "reason_id": ReasonHF = world.GetHistoricalFigure(Convert.ToInt32(property.Value)); break;
                }
            Site.AddEvent(this);
            Region.AddEvent(this);
            UndergroundRegion.AddEvent(this);
            Seeker.AddEvent(this);
            Target.AddEvent(this);
            if (ReasonHF != null && !ReasonHF.Equals(Seeker) && !ReasonHF.Equals(Target))
            {
                ReasonHF.AddEvent(this);
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
            eventString += Seeker.ToLink(link, pov);
            eventString += " was denied ";
            switch (Relationship)
            {
                case "apprentice":
                    eventString += "an apprenticeship under";
                    break;
                default:
                    break;
            }
            eventString += Target.ToLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToLink(link, pov);
            }
            if (ReasonHF != null)
            {
                switch (Reason)
                {
                    case "jealousy":
                        eventString += " due to jealousy of " + ReasonHF.ToLink(link, pov);
                        break;
                    case "prefers working alone":
                        eventString += " as "+ ReasonHF.ToLink(link, pov) + " prefers to work alone";
                        break;
                    default:
                        break;
                }
            }
            eventString += ".";
            return eventString;
        }
    }

    public class RegionpopIncorporatedIntoEntity : WorldEvent
    {
        public Site Site { get; set; }
        public Entity JoinEntity { get; set; }
        public string PopRace { get; set; }
        public int PopNumberMoved { get; set; }
        public WorldRegion PopSourceRegion { get; set; }
        public string PopFlId { get; set; }

        public RegionpopIncorporatedIntoEntity() { }
        public RegionpopIncorporatedIntoEntity(List<Property> properties, World world)
            : base(properties, world)
        {
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "join_entity_id": JoinEntity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                    case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                    case "pop_race": PopRace = property.Value; break;
                    case "pop_number_moved": PopNumberMoved = Convert.ToInt32(property.Value); break;
                    case "pop_srid": PopSourceRegion = world.GetRegion(Convert.ToInt32(property.Value)); break;
                    case "pop_flid": PopFlId = property.Value; break;
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
                eventString += " hundreds of";
            }
            else if (PopNumberMoved > 24)
            {
                eventString += " dozens of";
            }
            else
            {
                eventString += " several";
            }
            eventString += " UNKNOWN RACE from ";
            eventString += PopSourceRegion.ToLink(link, pov);
            eventString += " joined with ";
            eventString += JoinEntity.ToLink(link, pov);
            eventString += " at ";
            eventString += Site.ToLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }
}
