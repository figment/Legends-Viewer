using System;
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
        public WorldEvent() { ID = -1; Year = -1; Seconds72 = -1; Type = "INVALID"; }

        private void InternalMerge(List<Property> properties, World world)
        {
            World = world;
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "id": this.ID = property.ValueAsInt(); break;
                    case "year": this.Year = property.ValueAsInt(); break;
                    case "seconds72": this.Seconds72 = property.ValueAsInt(); break;
                    case "type": this.Type = String.Intern(property.Value.Replace('_',' ')); break;
                    default: break;
                }
        }

        public virtual void Merge(List<Property> properties, World world)
        {
            InternalMerge(properties, world);
        }

        public virtual string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Type;
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


    public enum HfEntityLinkType
    {
        Enemy,
        Member,
        Position,
        Prisoner,
        Slave,
        Squad,
        Unknown
    }
    public class AddHFEntityLink : WorldEvent
    {
        public Entity Entity;
        public HistoricalFigure HistoricalFigure;
        public HfEntityLinkType LinkType;
        public string Position;

        public AddHFEntityLink()
        {
            LinkType = HfEntityLinkType.Unknown;
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id":
                        Entity = world.GetEntity(property.ValueAsInt());
                        Entity.AddEvent(this);
                        break;
                    case "histfig":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        HistoricalFigure.AddEvent(this);
                        break;
                    case "position":
                        Position = string.Intern(Formatting.InitCaps(property.Value));
                        break;
                    case "link_type":
                        if (!Enum.TryParse(property.Value, true, out LinkType))
                        {
                            world.ParsingErrors.Report("Unknown HfEntityLinkType: " + property.Value);
                            LinkType = HfEntityLinkType.Unknown;
                        }
                        break;
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
            string eventString = this.GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case HfEntityLinkType.Prisoner: eventString += " was imprisoned by ";   break;
                case HfEntityLinkType.Slave:    eventString += " was enslaved by ";     break;
                case HfEntityLinkType.Enemy:    eventString += " became an enemy of ";  break;
                case HfEntityLinkType.Member:   eventString += " became a member of ";  break;
                case HfEntityLinkType.Squad:
                case HfEntityLinkType.Position: eventString += " became the " + Position + " of ";  break;
                default: eventString += " linked to "; break;
            }
            eventString += Entity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class AddHFHFLink : WorldEvent
    {
        public HistoricalFigure HistoricalFigure, HistoricalFigureTarget;
        public HistoricalFigureLinkType LinkType;
        public bool LinkTypeSet;
        public AddHFHFLink() { LinkType = HistoricalFigureLinkType.Unknown; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hf":
                    case "hfid":
                    case "histfig1":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); 
                        break;
                    case "histfig2":
                    case "hf_target":
                    case "hfid_target":
                        HistoricalFigureTarget = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "link_type":
                        if (!Enum.TryParse(Formatting.InitCaps(property.Value.Replace("_", " ")).Replace(" ", ""), true, out LinkType))
                        {
                            LinkType = HistoricalFigureLinkType.Unknown;
                            world.ParsingErrors.Report("Unknown HF Link Type: " + property.Value);
                        }
                        LinkTypeSet = true;
                        break;
                }

            //Fill in LinkType by looking at related historical figures.
            if (!LinkTypeSet && LinkType == HistoricalFigureLinkType.Unknown && HistoricalFigure != HistoricalFigure.Unknown && HistoricalFigureTarget != HistoricalFigure.Unknown)
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
            }

            if (HistoricalFigure.Race == "Night Creature" || HistoricalFigureTarget.Race == "Night Creature")
            {
                if (LinkType == HistoricalFigureLinkType.Unknown)
                {
                    LinkType = HistoricalFigureLinkType.Spouse;
                }
                HistoricalFigure.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigureTarget, HistoricalFigureLinkType.ExSpouse));
                HistoricalFigureTarget.RelatedHistoricalFigures.Add(new HistoricalFigureLink(HistoricalFigure, HistoricalFigureLinkType.ExSpouse));
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

            eventString += ((pov == HistoricalFigureTarget) ? HistoricalFigureTarget : HistoricalFigure).ToSafeLink(link, pov);
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
            eventString += (pov == HistoricalFigureTarget ? HistoricalFigure : HistoricalFigureTarget).ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ArtifactLost : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public Site Site { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact":
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
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
            eventString += Artifact.ToSafeLink(link, pov);
            eventString += " was lost in ";
            eventString += Site.ToSafeLink(link,pov);
            eventString += ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Artifact.ToSafeLink(link, pov) + " was claimed";
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += " by " + HistoricalFigure.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
            }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Artifact.ToSafeLink(link, pov) + " was stored in " + Site.ToSafeLink(link, pov) + " by " + HistoricalFigure.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ArtifactDestroyed : WorldEvent
    {
        public Artifact Artifact { get; set; }
        public Site Site { get; set; }
        public HistoricalFigure Destroyer { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "destroyer_enid": Destroyer = world.GetHistoricalFigure(property.ValueAsInt()); Destroyer.AddEvent(this); break;
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
            eventString += Artifact.ToSafeLink(link, pov);
            eventString += " was destroyed by ";
            eventString += Destroyer.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

    public class AssumeIdentity : WorldEvent
    {
        public HistoricalFigure Trickster { get; set; }
        public HistoricalFigure Identity { get; set; }
        public Entity Target { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(property.ValueAsInt()); Trickster.AddEvent(this); break;
                    case "identity_id": Identity = HistoricalFigure.Unknown; Identity.AddEvent(this); break; //Bad ID, so unknown for now.
                    case "target_enid": Target = world.GetEntity(property.ValueAsInt()); Target.AddEvent(this); break;
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
            string eventString = GetYearTime() + Trickster.ToSafeLink(link, pov) + " fooled " + Target.ToSafeLink(link, pov) + " into believing " + Trickster.CasteNoun() + " was " + Identity.ToSafeLink(link, pov) + ". ";
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
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); Attacker.AddEvent(this); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); Defender.AddEvent(this); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(property.ValueAsInt()); AttackerGeneral.AddEvent(this); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(property.ValueAsInt()); DefenderGeneral.AddEvent(this); break;
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
            eventString += " at " + Site.ToSafeLink(link, pov) + ". ";
            if (AttackerGeneral != null)
                eventString += AttackerGeneral.ToSafeLink(link, pov) + " led the attack";
            if (DefenderGeneral != null)
                eventString += ", and the defenders were led by " + DefenderGeneral.ToSafeLink(link, pov);
            else eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class BodyAbused : WorldEvent
    {
        // TODO
        public string ItemType { get; set; } // legends_plus.xml
        public string ItemSubType { get; set; } // legends_plus.xml
        public string Material { get; set; } // legends_plus.xml
        public int PileTypeID { get; set; } // legends_plus.xml
        public int MaterialTypeID { get; set; } // legends_plus.xml
        public int MaterialIndex { get; set; } // legends_plus.xml
        public int AbuseTypeID { get; set; } // legends_plus.xml

        public Entity Abuser { get; set; } // legends_plus.xml
        public HistoricalFigure Body { get; set; } // legends_plus.xml
        public HistoricalFigure HistoricalFigure { get; set; } // legends_plus.xml
        public Site Site { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        public string PropItemSubItem;

        public BodyAbused() { }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "civ": Abuser = world.GetEntity(property.ValueAsInt()); Abuser.AddEvent(this); break;
                    case "bodies": Body = world.GetHistoricalFigure(property.ValueAsInt()); Body.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "abuse_type": AbuseTypeID = property.ValueAsInt(); break;
                    case "props_pile_type": PileTypeID = property.ValueAsInt(); break;
                    case "props_item_subtype": PropItemSubItem = string.Intern(property.Value); break;
                    case "props_item_mat": property.Known = true; break;
                    case "props_item_mat_type": MaterialTypeID = property.ValueAsInt(); break;
                    case "props_item_mat_index": MaterialIndex = property.ValueAsInt(); break;
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
            eventString += Body.ToSafeLink(link, pov);
            eventString += "'s body was abused by ";
            eventString += Abuser.ToSafeLink(link, pov);
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
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
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Location Coordinates { get; set; }
        private string UnknownBodyState;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
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
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "building_id": Structure = Site?.GetStructure(property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " ";
            string stateString = "";
            switch (BodyState)
            {
                case BodyState.EntombedAtSite: stateString = "was entombed"; break;
                case BodyState.Unknown: stateString = "(" + UnknownBodyState + ")"; break;
            }
            eventString += stateString;
            if (Region != null)
                eventString += " in " + Region.ToSafeLink(link, pov);
            if (Site != null)
                eventString += " at " + Site.ToSafeLink(link, pov);
            eventString += " within ";
            eventString += Structure != null ? Structure.ToSafeLink(link, pov) : "UNKNOWN STRUCTURES";
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

        public ChangeHFJob()
        {
            NewJob = "UNKNOWN JOB";
            OldJob = "UNKNOWN JOB";
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "old_job": OldJob = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " "))); break;
                    case "new_job": NewJob = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " "))); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            // TODO: Sort out differences
            //string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            //if (!string.IsNullOrEmpty(NewJob))
            //{
            //    if (!string.IsNullOrEmpty(OldJob))
            //        eventString += $" changed jobs from {OldJob} to {NewJob}";
            //    else
            //        eventString += $" became a {NewJob}";
            //}
            //else if (!string.IsNullOrEmpty(OldJob))
            //    eventString += " stopped working as a " + OldJob;
            //else 
            //    eventString += " became a UNKNOWN JOB";
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (OldJob != "standard" && NewJob != "standard")
            {
                eventString += " gave up being a " + OldJob + " to become a " + NewJob;
            }
            else if (NewJob != "standard")
            {
                eventString += " became a " + NewJob;
            }
            else if (OldJob != "standard")
            {
                eventString += " stopped being a " + OldJob;
            }
            else
            {
                eventString += " became a peasant";
            }
            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
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
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        if (HistoricalFigure != null && HistoricalFigure.AddEvent(this))
                            HistoricalFigure.States.Add(new HistoricalFigure.State(State, Year));
                        break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
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
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (State == HFState.Settled) eventString += " settled in ";
            else if (State == HFState.Refugee || State == HFState.Snatcher || State == HFState.Thief) eventString += " became a " + State.ToString().ToLower() + " in ";
            else if (State == HFState.Wandering) eventString += " began wandering ";
            else if (State == HFState.Scouting) eventString += " began scouting the area around ";
            else if (State == HFState.Hunting) eventString += " began hunting great beasts in ";
            else if (State == HFState.Visiting) eventString += " visited ";
            else
            {
                eventString += " " + UnknownState + " in ";
            }
            if (Site != null) eventString += Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += Region.ToSafeLink(link, pov);
            else if (UndergroundRegion != null) eventString += UndergroundRegion.ToSafeLink(link, pov);
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

    public class CreateEntityPosition : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteCiv { get; set; }
        public string Position { get; set; }
        public int Reason { get; set; } // TODO // legends_plus.xml

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ": SiteCiv = world.GetEntity(property.ValueAsInt()); SiteCiv.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "reason": Reason = property.ValueAsInt(); break;
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
            string eventString = GetYearTime();
            switch (Reason)
            {
                case 0:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " through force of argument. ";
                    break;
                case 1:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " compelled the creation of the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " with threats of violence. ";
                    break;
                case 2:
                    eventString += SiteCiv.ToSafeLink(link, pov);
                    eventString += " collaborated to create the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += ". ";
                    break;
                case 3:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += ", pushed by a wave of popular support. ";
                    break;
                case 4:
                    eventString += HistoricalFigure.ToSafeLink(link, pov);
                    eventString += " of ";
                    eventString += Civ.ToSafeLink(link, pov, "CIV");
                    eventString += " created the position of ";
                    eventString += !string.IsNullOrWhiteSpace(Position) ? Position : "UNKNOWN POSITION";
                    eventString += " as a matter of course. ";
                    break;
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class CreatedSite : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public HistoricalFigure Builder;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "builder_hfid": Builder = world.GetHistoricalFigure(property.ValueAsInt()); break;
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
                eventString += Builder.ToSafeLink(link, pov) + " created " + Site.ToSafeLink(link, pov) + ". ";
            }
            else
            {
                if (SiteEntity != null) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
                eventString += Civ.ToSafeLink(link, pov) + " founded " + Site.ToSafeLink(link, pov) + ". ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class CreatedWorldConstruction : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site1, Site2;
        public WorldConstruction WorldConstruction, MasterWorldConstruction;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id1": Site1 = world.GetSite(property.ValueAsInt()); break;
                    case "site_id2": Site2 = world.GetSite(property.ValueAsInt()); break;
                    case "wcid": WorldConstruction = world.GetWorldConstruction(property.ValueAsInt()); break;
                    case "master_wcid": MasterWorldConstruction = world.GetWorldConstruction(property.ValueAsInt()); break;
                }
            }

            Civ.AddEvent(this);
            SiteEntity.AddEvent(this);

            WorldConstruction.AddEvent(this);
            MasterWorldConstruction.AddEvent(this);

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
            string eventString = GetYearTime();
            eventString += SiteEntity.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " constructed ";
            eventString += WorldConstruction.ToSafeLink(link, pov);
            if (MasterWorldConstruction != null)
            {
                eventString += " as part of ";
                eventString += MasterWorldConstruction.ToSafeLink(link, pov);
            }
            eventString += " connecting ";
            eventString += Site1.ToSafeLink(link, pov);
            eventString += " and ";
            eventString += Site2.ToSafeLink(link, pov);
            return eventString;
        }
    }
    public class CreatureDevoured : WorldEvent
    {
        public string Race { get; set; }
        public string Caste { get; set; }

        public HistoricalFigure Eater, Victim;
        public Entity Entity { get; set; }
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "eater": Eater = world.GetHistoricalFigure(property.ValueAsInt()); Eater.AddEvent(this); break;
                    case "race": Race = Formatting.InitCaps(property.Value.Replace("_", " ")); break;
                    case "caste": Caste = Formatting.InitCaps(property.Value.Replace("_", " ")); break;
                    case "victim": Victim = world.GetHistoricalFigure((property.ValueAsInt())); Victim.AddEvent(this); break;
                    case "entity": Entity = world.GetEntity((property.ValueAsInt())); Entity.AddEvent(this); break;
                   //case "entity": Victim = world.GetHistoricalFigure(property.ValueAsInt()); Victim.AddEvent(this); break;
                }



        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eater = Eater != null ? Eater.ToSafeLink(link, pov) : Race ?? "UNKNOWN HISTORICAL FIGURE";
            string victim = Victim != null ? Victim.ToSafeLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
            string eventString = this.GetYearTime();
            eventString += $"{eater} devoured {victim} in ";
            if (Site != null) eventString += Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += Region.ToSafeLink(link, pov);
            else if (UndergroundRegion != null) eventString += UndergroundRegion.ToSafeLink(link, pov);
            if (Eater != null)
            {
                eventString += Eater.ToSafeLink(link, pov);
            }
            else
            {
                eventString += "UNKNOWN HISTORICAL FIGURE";
            }
            eventString += " devoured ";
            if (Victim != null)
            {
                eventString += Victim.ToSafeLink(link, pov);
            }
            else if (!string.IsNullOrWhiteSpace(Race))
            {
                eventString += " a ";
                if (!string.IsNullOrWhiteSpace(Caste))
                {
                    eventString += Caste + " ";
                }
                eventString += Race;
            }
            else
            {
                eventString += "UNKNOWN HISTORICAL FIGURE";
            }
            eventString += " in ";
            if (Site != null)
            {
                eventString += Site.ToSafeLink(link, pov);
            }
            else if (Region != null)
            {
                eventString += Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += UndergroundRegion.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class DestroyedSite : WorldEvent
    {
        public Site Site;
        public Entity SiteEntity, Attacker, Defender;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Defender.ToSafeLink(link, pov) + " and destroyed " + Site.ToSafeLink(link, pov) + ". ";
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
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "attacker_general_hfid": AttackerGeneral = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "defender_general_hfid": DefenderGeneral = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " attacked " + Defender.ToSafeLink(link, pov) + " in " + Region.ToSafeLink(link, pov) + ". " +
                AttackerGeneral.ToSafeLink(link, pov) + " led the attack, and the defenders were led by " + DefenderGeneral.ToSafeLink(link, pov) + ". ";
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
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "target_hfid": Target = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "snatcher_hfid": Snatcher = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
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
                eventString += Snatcher.ToSafeLink(link, pov);
            else
                eventString += "(UNKNOWN HISTORICAL FIGURE)";
            eventString += " abducted " + Target.ToSafeLink(link, pov) + " from " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            Reasons = new List<ConfrontReason>();
            UnknownReasons = new List<string>();
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
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
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            string situationString = "";
            switch (Situation)
            {
                case ConfrontSituation.GeneralSuspicion: situationString = "aroused general suspicion"; break;
                case ConfrontSituation.Unknown: situationString = "(" + UnknownSituation + ")"; break;
            }
            eventString += " " + situationString;

            if (Region != null)
                eventString += " in " + Region.ToSafeLink(link, pov);

            if (Site != null)
                eventString += " at " + Site.ToSafeLink(link, pov);

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
                    case "slayer_hfid": Slayer = world.GetHistoricalFigure(property.ValueAsInt()); Slayer.AddEvent(this); if (Slayer != null) Slayer.NotableKills.Add(this);  break;
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

        private void InternalMerge(List<Property> properties, World world)
        {
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
        }


        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
            if (Target != null && !string.IsNullOrWhiteSpace(Interaction) && !Target.ActiveInteractions.Contains(Interaction))
                Target.ActiveInteractions.Add(Interaction);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            //string eventString = GetYearTime() + Doer.ToSafeLink(link, pov) + InteractionDescription + " on " + Target.ToSafeLink(link, pov) + ". ";
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

    public class HFGainsSecretGoal : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public SecretGoal Goal { get; set; }
        private string UnknownGoal;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
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
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
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

        private void InternalMerge(List<Property> properties, World world)
        {
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

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
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

    public class HFNewPet : WorldEvent
    {
        public string Pet { get; set; }
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        public Location Coordinates;

        public HFNewPet() { Pet = "UNKNOWN"; }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "group":
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "pets": Pet = Formatting.InitCaps(property.Value.Replace("_", " ").Replace("2", "two")); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            //string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " tamed " + PetType;
            //if (Site != null) eventString += " at " + Site.ToSafeLink(link, pov);
            //else if (Region != null) eventString += " in " + Region.ToSafeLink(link, pov);
            //else if (UndergroundRegion != null) eventString += " in " + UndergroundRegion.ToSafeLink(link, pov);
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " tamed the creatures named ";
            eventString += !string.IsNullOrWhiteSpace(Pet) ? Pet : "UNKNOWN";
            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
            }
            else if (Region != null)
            {
                eventString += " in " + Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += " in " + UndergroundRegion.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFProfanedStructure : WorldEvent
    {
        public int Action { get; set; } // legends_plus.xml
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "histfig":
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "action": Action = property.ValueAsInt(); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " profaned ";
            eventString += Structure.ToSafeLink(link, pov);
            eventString += " in " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "group_1_hfid": HistoricalFigure1 = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "group_2_hfid": HistoricalFigure2 = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + " " + HistoricalFigure1.ToSafeLink(link, pov) + " was reunited with " + HistoricalFigure2.ToSafeLink(link, pov);
            if (Site != null) eventString += " in " + Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += " in " + Region.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HFReachSummit : WorldEvent
    {
        public HistoricalFigure HistoricalFigure { get; set; }
        public WorldRegion Region { get; set; }
        public UndergroundRegion UndergroundRegion { get; set; }
        public Site Site { get; set; }
        public Location Coordinates;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "group":
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
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
            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            eventString += " reached the summit";
            if (Region != null)
            {
                eventString += ", which rises above ";
                eventString += Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += ", in the depths of ";
                eventString += UndergroundRegion.ToSafeLink(link, pov);
            }
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            eventString += ".";
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
            if (SubType == HFSimpleBattleType.HF2LostAfterGivingWounds) eventString = this.GetYearTime() + HistoricalFigure2.ToSafeLink(link, pov) + " was forced to retreat from "
                + HistoricalFigure1.ToSafeLink(link, pov) + " despite the latter's wounds";
            else if (SubType == HFSimpleBattleType.HF2LostAfterMutualWounds) eventString += " eventually prevailed and " + HistoricalFigure2.ToSafeLink(link, pov)
                + " was forced to make a hasty escape";
            else if (SubType == HFSimpleBattleType.HF2LostAfterReceivingWounds) eventString = this.GetYearTime() + HistoricalFigure2.ToSafeLink(link, pov) + " managed to escape from "
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
    public class HFTravel : WorldEvent
    {
        public Location Coordinates;
        public bool Escaped, Returned;
        public HistoricalFigure HistoricalFigure;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                    case "escape": Escaped = true; property.Known = true; break;
                    case "return": Returned = true; property.Known = true; break;
                    case "group_hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
            if (Escaped) return this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " escaped from the " + UndergroundRegion.ToSafeLink(link, pov);
            else if (Returned) eventString += " returned to ";
            else eventString += " made a journey to ";

            if (UndergroundRegion != null) eventString += UndergroundRegion.ToSafeLink(link, pov);
            else if (Site != null) eventString += Site.ToSafeLink(link, pov);
            else if (Region != null) eventString += Region.ToSafeLink(link, pov);

            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class HFWounded : WorldEvent
    {
        public int WoundeeRace { get; set; }
        public int WoundeeCaste { get; set; }

        // TODO
        public int BodyPart { get; set; } // legends_plus.xml
        public int InjuryType { get; set; } // legends_plus.xml
        public int PartLost { get; set; } // legends_plus.xml

        public HistoricalFigure Woundee, Wounder;
        public Site Site;
        public WorldRegion Region;
        public UndergroundRegion UndergroundRegion;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "woundee":
                    case "woundee_hfid": Woundee = world.GetHistoricalFigure(property.ValueAsInt()); Woundee.AddEvent(this); break;
                    case "wounder":
                    case "wounder_hfid": Wounder = world.GetHistoricalFigure(property.ValueAsInt()); Wounder.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "subregion_id": Region = world.GetRegion(property.ValueAsInt()); Region.AddEvent(this); break;
                    case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt()); UndergroundRegion.AddEvent(this); break;
                    //case "woundee_race": WoundeeRace = Formatting.InitCaps(property.Value); break;
                    //case "woundee_caste": WoundeeCaste = Formatting.InitCaps(property.Value); break;
                    //case "body_part": BodyPart = string.Intern(property.Value); break;
                    //case "injury_type": InjuryType = string.Intern(property.Value); break;
                    case "part_lost": PartLost = property.ValueAsInt(); break;
                    case "woundee_race": WoundeeRace = property.ValueAsInt(); break;
                    case "woundee_caste": WoundeeCaste = property.ValueAsInt(); break;
                    case "body_part": BodyPart = property.ValueAsInt(); break;
                    case "injury_type": InjuryType = property.ValueAsInt(); break;
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
            eventString += Woundee.ToSafeLink(link, pov);
            eventString += " was wounded by ";
            eventString += Wounder.ToSafeLink(link, pov);

            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
            }
            else if (Region != null)
            {
                eventString += " in " + Region.ToSafeLink(link, pov);
            }
            else if (UndergroundRegion != null)
            {
                eventString += " in " + UndergroundRegion.ToSafeLink(link, pov);
            }

            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class ImpersonateHF : WorldEvent
    {
        public HistoricalFigure Trickster, Cover;
        public Entity Target;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "trickster_hfid": Trickster = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "cover_hfid": Cover = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "target_enid": Target = world.GetEntity(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Trickster.ToSafeLink(link, pov) + " fooled " + Target.ToSafeLink(link, pov)
                + " into believing he/she was a manifestation of the deity " + Cover.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ItemStolen : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public string ItemType { get; set; }
        public int ItemSubType { get; set; }
        public string Material { get; set; }
        public int MaterialTypeID { get; set; }
        public int MaterialIndex { get; set; }
        public HistoricalFigure Thief { get; set; }
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public Site ReturnSite { get; set; }

        public ItemStolen()
        {
            ItemType = "UNKNOWN ITEM";
            Material = "UNKNOWN MATERIAL";
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "item_type": ItemType = string.Intern(Formatting.InitCaps(property.Value.Replace("_", " ")));  break;
                    case "mat": Material = string.Intern(Formatting.InitCaps(property.Value));  break;
                    case "histfig": Thief = world.GetHistoricalFigure(property.ValueAsInt()); Thief.AddEvent(this); break;
                    case "entity": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "item_subtype": ItemSubType = property.ValueAsInt(); break;
                    case "mattype": MaterialIndex = property.ValueAsInt(); break;
                    case "matindex": ItemSubType = property.ValueAsInt(); break;
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
            string itemType = !string.IsNullOrEmpty(ItemType) ? (Material + " " + ItemType) : "UNKNOWN ITEM";
            string site = Site.ToSafeLink(path, pov);
            string thief = Thief.ToSafeLink(path, pov);

            eventString += $" a {itemType} was stolen from {site} by {thief}";
            if (Entity != null) eventString += " from " + Entity.ToSafeLink(path, pov);

            eventString += " a ";
            eventString += Material + " " + ItemType;
            eventString += " was stolen from ";

            if (ReturnSite != null)
            {
                eventString += " and brought to " + ReturnSite.ToSafeLink();
            }
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "new_site_civ_id": NewSiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "new_leader_hfid": NewLeader = world.GetHistoricalFigure(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Defender.ToSafeLink(link, pov) + " and placed " + NewLeader.ToSafeLink(link, pov) + " in charge of " + Site.ToSafeLink(link, pov) + ". The new government was called " + NewSiteEntity.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class PeaceEfforts : WorldEvent
    {
        public string Decision { get; set; }
        public string Topic { get; set; }
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this);  break;
                    case "topic": Topic = Formatting.InitCaps(property.Value);  break;
                    case "source": Source = world.GetEntity(property.ValueAsInt()); Source.AddEvent(this);  break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); Destination.AddEvent(this);  break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            //if (Source != null && Destination != null && Site != null)
            //    return $"{GetYearTime()}Peace accepted by {Destination.ToSafeLink(link, pov)} in offer from {Source.ToSafeLink(link, pov)} at {Site.ToSafeLink(link, pov)}.";
            //return $"{GetYearTime()}Peace {Decision} in {ParentCollection.ToSafeLink(link, pov)}.";
            string eventString = GetYearTime();
            if (Source != null && Destination != null)
            {
                eventString += Destination.ToSafeLink(link, pov) + " " + Decision + " an offer of peace from " + Source.ToSafeLink(link, pov) + " in " + ParentCollection.ToSafeLink(link, pov) + ".";
            }
            else
            {
                eventString += "Peace " + Decision + " in " + ParentCollection.ToSafeLink(link, pov) + ".";
            }
            return eventString;
        }
    }
    public class PeaceAccepted : PeaceEfforts
    {
        public PeaceAccepted() { Decision = "accepted"; }
    }
    public class PeaceRejected : PeaceEfforts
    {
        public PeaceRejected() { Decision = "rejected"; }
    }

    public class PlunderedSite : WorldEvent
    {
        public Entity Attacker, Defender, SiteEntity;
        public Site Site;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && Defender != SiteEntity) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Defender.ToSafeLink(link, pov) + " and pillaged " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;

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
            string eventString = GetYearTime() + Entity.ToSafeLink(link, pov) + " razed ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ReclaimSite : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
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
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Civ.ToSafeLink(link, pov) + " launched an expedition to reclaim " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }
    public class RemoveHFEntityLink : WorldEvent
    {
        public Entity Entity;
        public HistoricalFigure HistoricalFigure;
        public HfEntityLinkType LinkType;
        public string Position;

        public RemoveHFEntityLink()
        {
            LinkType = HfEntityLinkType.Unknown;
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "civ":
                    case "civ_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "position": Position = string.Intern(Formatting.InitCaps(property.Value)); break;
                    case "link_type":
                        if (!Enum.TryParse(property.Value, true, out LinkType))
                        {
                            world.ParsingErrors.Report("Unknown HfEntityLinkType: " + property.Value);
                        }
                        break;

                }
            }

            HistoricalFigure.AddEvent(this);
            Entity.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case HfEntityLinkType.Prisoner:
                    eventString += " escaped from the prisons of ";
                    break;
                case HfEntityLinkType.Slave:
                    eventString += " fled from ";
                    break;
                case HfEntityLinkType.Enemy:
                    eventString += " stopped being an enemy of ";
                    break;
                case HfEntityLinkType.Member:
                    eventString += " left ";
                    break;
                case HfEntityLinkType.Squad:
                case HfEntityLinkType.Position:
                    eventString += " stopped being the " + Position + " of ";
                    break;
                default:
                    eventString += " stopped being linked to ";
                    break;
            }

            eventString += Entity.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "artifact_id": Artifact = world.GetArtifact(property.ValueAsInt()); Artifact.AddEvent(this); break;
                    case "hfid":
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site": 
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "name_only": RecievedName = true; property.Known = true; break;
                }
            if (Artifact != null)
                Artifact.Creator = HistoricalFigure;            
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + Artifact.ToSafeLink(link, pov);
            if (RecievedName)
                eventString += " recieved its name";
            else
                eventString += " was created";
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            if (RecievedName)
                eventString += " from ";
            else
                eventString += " by ";
            eventString += HistoricalFigure.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ArtifactTransformed : WorldEvent
    {
        public int UnitID { get; set; }
        public Artifact NewArtifact { get; set; }
        public Artifact OldArtifact { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "unit_id": UnitID = property.ValueAsInt(); break;
                    case "new_artifact_id": NewArtifact = world.GetArtifact(property.ValueAsInt()); break;
                    case "old_artifact_id": OldArtifact = world.GetArtifact(property.ValueAsInt()); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                }
            NewArtifact.AddEvent(this);
            OldArtifact.AddEvent(this);
            HistoricalFigure.AddEvent(this);
            Site.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += NewArtifact.ToSafeLink(link, pov);
                eventString += ", ";
            if (!string.IsNullOrWhiteSpace(NewArtifact.Material))
            {
                eventString += NewArtifact.Material;
            }
            if (!string.IsNullOrWhiteSpace(NewArtifact.SubType))
            {
                eventString += " ";
                eventString += NewArtifact.SubType;
            }
            else
            {
                eventString += " ";
                eventString += !string.IsNullOrWhiteSpace(NewArtifact.Type) ? NewArtifact.Type : "UNKNOWN TYPE";
            }
            eventString += ", was made from ";
            eventString += OldArtifact.ToSafeLink(link, pov);
            eventString += ", ";
            if (!string.IsNullOrWhiteSpace(OldArtifact.Material))
            {
                eventString += OldArtifact.Material;
            }
            if (!string.IsNullOrWhiteSpace(OldArtifact.SubType))
            {
                eventString += " ";
                eventString += OldArtifact.SubType;
            }
            else
            {
                eventString += " ";
                eventString += !string.IsNullOrWhiteSpace(OldArtifact.Type) ? OldArtifact.Type : "UNKNOWN TYPE";
            }
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += " by ";
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class DiplomatLost : WorldEvent
    {
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
            
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + "UNKNOWN ENTITY lost a diplomat at " + Site.ToSafeLink(link, pov)
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
        public int StructureID;
        public Structure Structure;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;

                    //Unhandled Events
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;
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
            eventString += Entity.ToSafeLink(link, pov) + " formed in ";
            eventString += (Site.ToSafeLink(link, pov)) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); break;
                    case "hist_figure_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
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
            string eventString = GetYearTime() + HistoricalFigure.ToSafeLink(link, pov);
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
            eventString += Entity.ToSafeLink(link, pov);
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
        public int Action { get; set; } // legends_plus.xml
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity":
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "action": Action = property.ValueAsInt(); break;
                }
            }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
            Structure.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime() + Entity.ToSafeLink(link, pov) + " became the primary criminal organization in " + Site.ToSafeLink(link, pov) ;
            eventString += " at ";
            eventString += Structure != null ? Structure.Name : $" at ({StructureID})";
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class EntityRelocate : WorldEvent
    {
        public int Action { get; set; } // legends_plus.xml
        public Entity Entity { get; set; }
        public Site Site { get; set; }
        public int StructureID { get; set; }
        public Structure Structure { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "entity":
                    case "entity_id": Entity = world.GetEntity(property.ValueAsInt()); Entity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;
                    case "action": Action = property.ValueAsInt(); break;
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
            string eventString = GetYearTime() + Entity.ToSafeLink(link, pov) + " moved to ";
            eventString += Structure.ToSafeLink(link, pov);
            eventString += " in " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "ghost": Ghost = property.Value; break;
                    case "hfid": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
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
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " came back from the dead as a " + Ghost + " in " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


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
        
        private void InternalMerge(List<Property> properties, World world)
        {
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
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
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

    public class MasterpieceArchDesign : MasterpieceArch
    {
        public MasterpieceArchDesign() { Process = "designed"; }
    }

    public class MasterpieceArchConstructed : MasterpieceArch
    {
        public MasterpieceArchConstructed() { Process = "constructed"; }
    }


    public class MasterpieceEngraving : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public int ArtID { get; set; }
        public int ArtSubID { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "skill_at_time": SkillAtTime = property.ValueAsInt(); break;
                    case "maker":
                    case "hfid": Maker = world.GetHistoricalFigure(property.ValueAsInt()); Maker.AddEvent(this); break;
                    case "maker_entity":
                    case "entity_id": MakerEntity = world.GetEntity(property.ValueAsInt()); MakerEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "skill_rating": SkillAtTime = property.ValueAsInt(); break;
                    case "art_id": ArtID = property.ValueAsInt(); break;
                    case "art_subid": ArtSubID = property.ValueAsInt(); break;
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
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " created a masterful ";
            eventString += "engraving";
            eventString += " for ";
            eventString += MakerEntity.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceFood : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public int ItemID { get; set; }
        public string ItemSubType { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
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
                    case "item_subtype": ItemSubType = property.Value; break;
                    case "item_id": ItemID = property.ValueAsInt(); break;
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
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " prepared a masterful ";
            switch (ItemSubType)
            {
                case "0": eventString += "biscuits"; break;
                case "1": eventString += "stew"; break;
                case "2": eventString += "roasts"; break;
                default: eventString += "meal"; break;
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

    public class MasterpieceItem : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }
        public string Material { get; set; }
        public int MaterialType { get; set; }
        public int MaterialIndex { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
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
                    case "skill_used": SkillAtTime = property.ValueAsInt(); break;
                    case "item_type": ItemType = property.Value.Replace("_", " "); break;
                    case "item_subtype": ItemSubType = property.Value.Replace("_", " "); break;
                    case "mat": Material = property.Value.Replace("_", " "); break;
                    case "item_id": ItemID = property.ValueAsInt(); break;
                    case "mat_type": MaterialType = property.ValueAsInt(); break;
                    case "mat_index": MaterialIndex = property.ValueAsInt(); break;
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
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " created a masterful ";
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
            eventString += MakerEntity.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class MasterpieceDye : WorldEvent
    {
        private int SkillAtTime { get; set; }
        public HistoricalFigure Maker { get; set; }
        public Entity MakerEntity { get; set; }
        public Site Site { get; set; }
        public string ItemType { get; set; }
        public string ItemSubType { get; set; }
        public int MaterialType { get; set; }
        public int MaterialIndex { get; set; }
        public int DyeMaterialType { get; set; }
        public int DyeMaterialIndex { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
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
                    case "item_type": ItemType = property.Value.Replace("_", " "); break;
                    case "item_subtype": ItemSubType = property.Value.Replace("_", " "); break;
                    case "mat_type": MaterialType = property.ValueAsInt(); break;
                    case "mat_index": MaterialIndex = property.ValueAsInt(); break;
                    case "dye_mat_type": DyeMaterialType = property.ValueAsInt(); break;
                    case "dye_mat_index": DyeMaterialIndex = property.ValueAsInt(); break;
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
            eventString += Maker.ToSafeLink(link, pov);
            eventString += " masterfully dyed a ";
            eventString += "UNKNOWN ITEM";
            eventString += " with ";
            eventString += "UNKNOWN DYE";
            eventString += " for ";
            eventString += MakerEntity.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

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
            eventString += Improver != null ? Improver.ToSafeLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
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

    public class MasterpieceLost : WorldEvent
    {
        public MasterpieceLost() { }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public int MethodID { get; set; }
        public MasterpieceItem CreationEvent { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "creation_event": CreationEvent = world.GetEvent(property.ValueAsInt()) as MasterpieceItem; break;
                    case "method": MethodID = property.ValueAsInt(); break;
                }
            }
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += "the masterful ";
            if (CreationEvent != null)
            {
                eventString += !string.IsNullOrWhiteSpace(CreationEvent.Material) ? CreationEvent.Material + " " : "";
                if (!string.IsNullOrWhiteSpace(CreationEvent.ItemSubType) && CreationEvent.ItemSubType != "-1")
                {
                    eventString += CreationEvent.ItemSubType;
                }
                else
                {
                    eventString += !string.IsNullOrWhiteSpace(CreationEvent.ItemType) ? CreationEvent.ItemType : "UNKNOWN ITEM";
                }
                eventString += " created by ";
                eventString += CreationEvent.Maker.ToSafeLink(link, pov);
                eventString += " for ";
                eventString += CreationEvent.MakerEntity.ToSafeLink(link, pov);
                eventString += " at ";
                eventString += CreationEvent.Site.ToSafeLink(link, pov);
                eventString += " ";
                eventString += CreationEvent.GetYearTime();
            }
            else
            {
                eventString += "UNKNOWN ITEM";
            }
            eventString += " was destroyed by ";
            eventString += HistoricalFigure != null ? HistoricalFigure.ToSafeLink(link, pov) : "an unknown creature";
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

    public class Merchant : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        protected void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "source": Source = world.GetEntity(property.ValueAsInt()); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); break;
                    case "site": Site = world.GetSite(property.ValueAsInt()); break;
                }
            }
            Source.AddEvent(this);
            Destination.AddEvent(this);
            Site.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += "merchants from ";
            eventString += Source.ToSafeLink(link, pov, "CIV");
            eventString += " visited ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

    public class SiteAbandoned : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
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
            if (SiteEntity != null && SiteEntity != Civ) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            eventString += Civ.ToSafeLink(link, pov) + " abandoned the settlement at " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class SiteDied : WorldEvent
    {
        public Entity Civ, SiteEntity;
        public Site Site;
        public Boolean Abandoned;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
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
                eventString += "abandoned the settlement of " + Site.ToSafeLink(link, pov) + ". ";
            }
            else
            {
                eventString += " settlement of " + Site.ToSafeLink(link, pov) + " withered. ";
            }
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


    public class AddHFSiteLink : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; } // TODO
        public Entity Civ { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public SiteLinkType LinkType { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                LinkType = SiteLinkType.Unknown;
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "civ": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "link_type":
                        switch (property.Value.Replace("_", " "))
                        {
                            case "lair": LinkType = SiteLinkType.Lair; break;
                            case "hangout": LinkType = SiteLinkType.Hangout; break;
                            case "home site building": LinkType = SiteLinkType.HomeSiteBuilding; break;
                            case "home site underground": LinkType = SiteLinkType.HomeSiteUnderground; break;
                            case "home structure": LinkType = SiteLinkType.HomeStructure; break;
                            case "seat of power": LinkType = SiteLinkType.SeatOfPower; break;
                            case "occupation": LinkType = SiteLinkType.Occupation; break;
                            case "home site realization building": LinkType = SiteLinkType.HomeSiteRealizationBuilding; break;
                            default:
                                LinkType = SiteLinkType.Unknown;
                                world.ParsingErrors.Report("Unknown Site Link Type: " + property.Value.Replace("_", " "));
                                break;
                        }

                        break;
                }
            }
            HistoricalFigure.AddEvent(this);
            Civ.AddEvent(this);
            Site.AddEvent(this);
            Structure.AddEvent(this);
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case SiteLinkType.HomeSiteRealizationBuilding:
                    eventString += " took up residence in ";
                    break;
                case SiteLinkType.Hangout:
                    eventString += " ruled from ";
                    break;
                default:
                    eventString += " UNKNOWN LINKTYPE (" + LinkType + ") ";
                    break;
            }
            eventString += Structure.ToSafeLink(link, pov);
            if (Civ != null)
            {
                eventString += " of " + Civ.ToSafeLink(link, pov);
            }
            if (Site != null)
            {
                eventString += " in " + Site.ToSafeLink(link, pov);
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }


    public enum AgreementTopic
    {
        TreeQuota,
        BecomeLandHolder,
        PromoteLandHolder,
        Unknown
    }


    public class AgreementMade : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }
        public AgreementTopic Topic { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "topic":
                        switch (property.Value)
                        {
                            case "treequota": Topic = AgreementTopic.TreeQuota; break;
                            case "becomelandholder": Topic = AgreementTopic.BecomeLandHolder; break;
                            case "promotelandholder": Topic = AgreementTopic.PromoteLandHolder; break;
                            default:
                                Topic = AgreementTopic.Unknown;
                                world.ParsingErrors.Report("Unknown Agreement Topic: " + property.Value);
                                break;
                        }
                        break;
                    case "source": Source = world.GetEntity(property.ValueAsInt()); Source.AddEvent(this); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); Destination.AddEvent(this); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            switch (Topic)
            {
                case AgreementTopic.TreeQuota:
                    eventString += "a lumber agreement proposed by ";
                    break;
                case AgreementTopic.BecomeLandHolder:
                    eventString += "the establishment of landed nobility proposed by ";
                    break;
                case AgreementTopic.PromoteLandHolder:
                    eventString += "the elevation of the landed nobility proposed by ";
                    break;
                default:
                    eventString += "UNKNOWN AGREEMENT";
                    break;
            }
            eventString += " proposed by ";
            eventString += Source.ToSafeLink(link, pov);
            eventString += " was accepted by ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += ".";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class AgreementConcluded : WorldEvent
    {
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Site Site { get; set; }
        public AgreementTopic Topic { get; set; }
        public int Result { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "topic":
                        switch (property.Value)
                        {
                            case "treequota": Topic = AgreementTopic.TreeQuota; break;
                            case "becomelandholder": Topic = AgreementTopic.BecomeLandHolder; break;
                            case "promotelandholder": Topic = AgreementTopic.PromoteLandHolder; break;
                            default:
                                Topic = AgreementTopic.Unknown;
                                world.ParsingErrors.Report("Unknown Agreement Topic: " + property.Value);
                                break;
                        }
                        break;
                    case "source": Source = world.GetEntity(property.ValueAsInt()); Source.AddEvent(this); break;
                    case "destination": Destination = world.GetEntity(property.ValueAsInt()); Destination.AddEvent(this); break;
                    case "result": Result = property.ValueAsInt(); break;
                }
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            switch (Topic)
            {
                case AgreementTopic.TreeQuota:
                    eventString += "a lumber agreement between ";
                    break;
                case AgreementTopic.BecomeLandHolder:
                    eventString += "the establishment of landed nobility agreement between ";
                    break;
                case AgreementTopic.PromoteLandHolder:
                    eventString += "the elevation of the landed nobility agreement between ";
                    break;
                default:
                    eventString += "UNKNOWN AGREEMENT";
                    break;
            }
            eventString += Source.ToSafeLink(link, pov);
            eventString += " and ";
            eventString += Destination.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " concluded";
            switch (Result)
            {
                case -3:
                    eventString += "  with miserable outcome";
                    break;
                case -2:
                    eventString += " with a strong negative outcome";
                    break;
                case -1:
                    eventString += " in an unsatisfactory fashion";
                    break;
                case 0:
                    eventString += " fairly";
                    break;
                case 1:
                    eventString += " with a positive outcome";
                    break;
                case 2:
                    eventString += ", cementing bonds of mutual trust";
                    break;
                case 3:
                    eventString += " with a very strong positive outcome";
                    break;
                default:
                    eventString += " with an unknown outcome";
                    break;
            }
            eventString += ".";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public enum AgreementReason
    {
        Unknown,
        Whim,
        ViolentDisagreement,
        ArrivedAtLocation
    }

    public class AgreementFormed : WorldEvent
    {
        private string AgreementId { get; set; }
        public HistoricalFigure Concluder { get; set; }
        private string AgreementSubjectId { get; set; }
        private AgreementReason Reason { get; set; }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "agreement_id": AgreementId = property.Value; break;
                    case "concluder_hfid": Concluder = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "agreement_subject_id": AgreementSubjectId = property.Value; break;
                    case "reason":
                        switch (property.Value)
                        {
                            case "arrived at location": Reason = AgreementReason.ArrivedAtLocation; break;
                            case "violent disagreement": Reason = AgreementReason.ViolentDisagreement; break;
                            case "whim": Reason = AgreementReason.Whim; break;
                            default:
                                Reason = AgreementReason.Unknown;
                                world.ParsingErrors.Report("Unknown Agreement Reason: " + property.Value);
                                break;
                        }
                        break;
                }
            }
            Concluder.AddEvent(this);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Concluder != null ? Concluder.ToSafeLink(link, pov) : "UNKNOWN HISTORICAL FIGURE";
            eventString += " formed an agreement";
            switch (Reason)
            {
                case AgreementReason.Whim:
                    eventString += " on a whim";
                    break;
                case AgreementReason.ViolentDisagreement:
                    eventString += " a violent disagreement";
                    break;
                case AgreementReason.ArrivedAtLocation:
                    eventString += " after arriving at the location";
                    break;
            }
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class CreatedStructure : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteEntity { get; set; }
        public Site Site { get; set; }
        public HistoricalFigure Builder { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ": 
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site": 
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "builder_hf": 
                    case "builder_hfid": Builder = world.GetHistoricalFigure(property.ValueAsInt()); Builder.AddEvent(this); break;
                }

            if (Site != null)
            {
                Structure = Site.Structures.FirstOrDefault(structure => structure.ID == StructureID);
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
            if (Builder != null)
            {
                eventString += Builder.ToSafeLink(link, pov);
                eventString += ", thrust a spire of slade up from the underworld, naming it ";
                eventString += Structure.ToSafeLink(link, pov);
                eventString += ", and established a gateway between worlds in ";
                eventString += Site.ToSafeLink(link, pov);
                eventString += ". ";
            }
            else
            {
                eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
                eventString += Civ.ToSafeLink(link, pov, "CIV");
                eventString += " constructed ";
                eventString += Structure.ToSafeLink(link, pov);
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
                eventString += ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
        }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }

        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = this.GetYearTime() + HistoricalFigure.ToSafeLink(link, pov) + " razed a ";
            eventString += Structure != null ? Structure.Name : $"({StructureID})";
            eventString += " in " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class RemoveHFSiteLink : WorldEvent
    {
        public int StructureID { get; set; }
        public Structure Structure { get; set; } // TODO
        public Entity Civ { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public Site Site { get; set; }
        public SiteLinkType LinkType { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                    case "structure":
                    case "structure_id": Structure = Site?.GetStructure(StructureID = property.ValueAsInt()); Structure.AddEvent(this);break;
                    case "histfig":
                    case "hist_fig_id": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); HistoricalFigure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "link_type":
                        switch (property.Value.Replace("_", " "))
                        {
                            case "lair": LinkType = SiteLinkType.Lair; break;
                            case "hangout": LinkType = SiteLinkType.Hangout; break;
                            case "home_site_building":
                            case "home site building": LinkType = SiteLinkType.HomeSiteBuilding; break;
                            case "home site underground": LinkType = SiteLinkType.HomeSiteUnderground; break;
                            case "home structure": LinkType = SiteLinkType.HomeStructure; break;
                            case "seat of power": LinkType = SiteLinkType.SeatOfPower; break;
                            case "occupation": LinkType = SiteLinkType.Occupation; break;
                            case "home site realization building": LinkType = SiteLinkType.HomeSiteRealizationBuilding; break;
                            default:
                                LinkType = SiteLinkType.Unknown;
                                world.ParsingErrors.Report("Unknown Site Link Type: " + property.Value.Replace("_", " "));
                                break;
                        }
                        break;
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
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            switch (LinkType)
            {
                case SiteLinkType.HomeSiteRealizationBuilding:
                    eventString += " moved out of ";
                    break;
                case SiteLinkType.Hangout:
                    eventString += " stopped ruling from ";
                    break;
                default:
                    eventString += " UNKNOWN LINKTYPE (" + LinkType + ") ";
                    break;
            }
            eventString += Structure.ToSafeLink(link, pov);
            if (Civ != null)
                eventString += " of " + Civ.ToSafeLink(link, pov);
            if (Site != null)
                eventString += " in " + Site.ToSafeLink(link, pov);
            eventString += ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class ReplacedStructure : WorldEvent
    {
        public int OldStructureID, NewStructureID;
        public Structure OldStructure, NewStructure;
        public Entity Civ, SiteEntity;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "old_structure":
                    case "old_ab_id": OldStructure = Site?.GetStructure(OldStructureID = property.ValueAsInt()); OldStructure.AddEvent(this); break;
                    case "new_structure":
                    case "new_ab_id": NewStructure = Site?.GetStructure(NewStructureID = property.ValueAsInt()); NewStructure.AddEvent(this); break;
                    case "civ":
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); Civ.AddEvent(this); break;
                    case "site_civ":
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); SiteEntity.AddEvent(this); break;
                    case "site":
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
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
            eventString += SiteEntity.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " replaced ";
            eventString += OldStructure.ToSafeLink(link, pov);
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " with ";
            eventString += NewStructure.ToSafeLink(link, pov);
            eventString += ".";
            return eventString;
        }
    }

    public class SiteTakenOver : WorldEvent
    {
        public Entity Attacker, Defender, NewSiteEntity, SiteEntity;
        public Site Site;

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_civ_id": Attacker = world.GetEntity(property.ValueAsInt()); break;
                    case "defender_civ_id": Defender = world.GetEntity(property.ValueAsInt()); break;
                    case "new_site_civ_id": NewSiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteEntity = world.GetEntity(property.ValueAsInt()); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " defeated ";
            if (SiteEntity != null && SiteEntity != Defender) eventString += SiteEntity.ToSafeLink(link, pov) + " of ";
            Defender.ToSafeLink(link, pov);
            eventString += " and took over " + Site.ToSafeLink(link, pov) + ". The new government was called " + NewSiteEntity.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "dispute":
                        switch (property.Value)
                        {
                            case "fishing rights":
                                Dispute = Dispute.FishingRights;
                                break;
                            case "grazing rights":
                                Dispute = Dispute.GrazingRights;
                                break;
                            case "livestock ownership":
                                Dispute = Dispute.LivestockOwnership;
                                break;
                            case "territory":
                                Dispute = Dispute.Territory;
                                break;
                            case "water rights":
                                Dispute = Dispute.WaterRights;
                                break;
                            case "rights-of-way":
                                Dispute = Dispute.RightsOfWay;
                                break;
                            default:
                                Dispute = Dispute.Unknown;
                                unknownDispute = property.Value;
                                world.ParsingErrors.Report("Unknown Site Dispute: " + unknownDispute);
                                break;
                        }
                        break;
                    case "entity_id_1":
                        Entity1 = world.GetEntity(property.ValueAsInt());
                        break;
                    case "entity_id_2":
                        Entity2 = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id_1":
                        Site1 = world.GetSite(property.ValueAsInt());
                        break;
                    case "site_id_2":
                        Site2 = world.GetSite(property.ValueAsInt());
                        break;
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
            string dispute = unknownDispute;
            switch (Dispute)
            {
                case Dispute.FishingRights:
                    dispute = "fishing rights";
                    break;
                case Dispute.GrazingRights:
                    dispute = "grazing rights";
                    break;
                case Dispute.LivestockOwnership:
                    dispute = "livestock ownership";
                    break;
                case Dispute.Territory:
                    dispute = "territory";
                    break;
                case Dispute.WaterRights:
                    dispute = "water rights";
                    break;
                case Dispute.RightsOfWay:
                    dispute = "rights of way";
                    break;
            }

            string eventString = GetYearTime();
            eventString += Entity1.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Site1.ToSafeLink(link, pov);
            eventString += " and ";
            eventString += Entity2.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Site2.ToSafeLink(link, pov);
            eventString += " became embroiled in a dispute over " + dispute + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HfAttackedSite : WorldEvent
    {
        private HistoricalFigure Attacker { get; set; }
        private Entity DefenderCiv { get; set; }
        private Entity SiteCiv { get; set; }
        private Site Site { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_hfid": Attacker = world.GetHistoricalFigure(property.ValueAsInt()); Attacker.AddEvent(this);break;
                    case "defender_civ_id": DefenderCiv = world.GetEntity(property.ValueAsInt()); DefenderCiv.AddEvent(this);break;
                    case "site_civ_id": SiteCiv = world.GetEntity(property.ValueAsInt()); SiteCiv.AddEvent(this); break;
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); Site.AddEvent(this); break;
                }
        }

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            String eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " attacked " + SiteCiv.ToSafeLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToSafeLink(link, pov);
            }
            eventString += " at " + Site.ToSafeLink(link, pov) + ". ";
            eventString += PrintParentCollection(link, pov);
            return eventString;
        }
    }

    public class HfDestroyedSite : WorldEvent
    {
        private HistoricalFigure Attacker { get; set; }
        private Entity DefenderCiv { get; set; }
        private Entity SiteCiv { get; set; }
        private Site Site { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "attacker_hfid":
                        Attacker = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "defender_civ_id":
                        DefenderCiv = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_civ_id":
                        SiteCiv = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
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
            String eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " routed " + SiteCiv.ToSafeLink(link, pov);
            if (DefenderCiv != null)
            {
                eventString += " of " + DefenderCiv.ToSafeLink(link, pov);
            }
            eventString += " and destroyed " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "attacker_civ_id":
                        Attacker = world.GetEntity(property.ValueAsInt());
                        break;
                    case "defender_civ_id":
                        Defender = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_civ_id":
                        SiteEntity = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
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
            string eventString = this.GetYearTime() + Attacker.ToSafeLink(link, pov) + " secured tribute from " + SiteEntity.ToSafeLink(link, pov);
            if (Defender != null)
            {
                eventString += " of " + Defender.ToSafeLink(link, pov);
            }
            eventString += ", to be delivered from " + Site.ToSafeLink(link, pov) + ". ";
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

        private void InternalMerge(List<Property> properties, World world)
        {
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

    public class FirstContact : WorldEvent
    {
        public Site Site;
        public Entity Contactor;
        public Entity Contacted;

        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "contactor_enid": Contactor = world.GetEntity(property.ValueAsInt()); break;
                    case "contacted_enid": Contacted = world.GetEntity(property.ValueAsInt()); break;
                }
            }
            Site.AddEvent(this);
            Contactor.AddEvent(this);
            Contacted.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += Contactor.ToSafeLink(link, pov);
            eventString += " made contact with ";
            eventString += Contacted.ToSafeLink(link, pov);
            eventString += " at ";
            eventString += Site.ToSafeLink(link, pov);
            return eventString;
        }
    }

    public class SiteRetired : WorldEvent
    {
        public Site Site { get; set; }
        public Entity Civ { get; set; }
        public Entity SiteCiv { get; set; }
        public string First { get; set; }
        public override void Merge(List<Property> properties, World world)
        {
            base.Merge(properties, world);
            InternalMerge(properties, world);
        }
        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "site_id": Site = world.GetSite(property.ValueAsInt()); break;
                    case "civ_id": Civ = world.GetEntity(property.ValueAsInt()); break;
                    case "site_civ_id": SiteCiv = world.GetEntity(property.ValueAsInt()); break;
                    case "first": First = property.Value; break;
                }
            }
            Site.AddEvent(this);
            Civ.AddEvent(this);
            SiteCiv.AddEvent(this);
        }
        public override string Print(bool link = true, DwarfObject pov = null)
        {
            string eventString = GetYearTime();
            eventString += SiteCiv.ToSafeLink(link, pov);
            eventString += " of ";
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " at the settlement of ";
            eventString += Site.ToSafeLink(link, pov);
            eventString += " regained their senses after an initial period of questionable judgment.";
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id":
                        Civ = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
                    case "feature_layer_id":
                        UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt());
                        break;
                    case "occasion_id":
                        OccasionId = property.ValueAsInt();
                        break;
                    case "schedule_id":
                        ScheduleId = property.ValueAsInt();
                        break;
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
            eventString += Civ.ToSafeLink(link, pov, "CIV");
            eventString += " held a ";
            eventString += OccasionType.ToString().ToLower();
            eventString += " in ";
            eventString += Site.ToSafeLink(link, pov);
            //eventString += " as part of UNKNOWN OCCASION (" + OccasionId + ") with UNKNOWN SCHEDULE(" + ScheduleId + ")";
            eventString += ".";
            return eventString;
        }
    }

    public class Procession : OccasionEvent
    {
        public Procession()
        {
            OccasionType = OccasionType.Procession;
        }
    }

    public class Ceremony : OccasionEvent
    {
        public Ceremony()
        {
            OccasionType = OccasionType.Ceremony;
        }
    }

    public class Performance : OccasionEvent
    {
        public Performance()
        {
            OccasionType = OccasionType.Performance;
        }
    }

    public class Competition : OccasionEvent
    {
        HistoricalFigure Winner { get; set; }
        List<HistoricalFigure> Competitors { get; set; }

        private void InternalMerge(List<Property> properties, World world)
        {
            OccasionType = OccasionType.Competition;
            Competitors = new List<HistoricalFigure>();
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "winner_hfid":
                        Winner = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "competitor_hfid":
                        Competitors.Add(world.GetHistoricalFigure(property.ValueAsInt()));
                        break;
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
                        eventString += competitor.ToSafeLink(link, pov);
                    }
                    else if (i == Competitors.Count - 1)
                    {
                        eventString += " and " + competitor.ToSafeLink(link, pov);
                    }
                    else
                    {
                        eventString += ", " + competitor.ToSafeLink(link, pov);
                    }
                }
                eventString += ". ";
            }
            if (Winner != null)
            {
                eventString += "The winner was ";
                eventString += Winner.ToSafeLink(link, pov);
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hist_figure_id":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "form_id":
                        FormId = property.Value;
                        break;
                    case "reason":
                        Reason = property.Value;
                        break;
                    case "reason_id":
                        ReasonId = property.ValueAsInt();
                        break;
                    case "circumstance":
                        Circumstance = property.Value;
                        break;
                    case "circumstance_id":
                        CircumstanceId = property.ValueAsInt();
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
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
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            if (GlorifiedHF != null)
            {
                eventString += " in order to glorify " + GlorifiedHF.ToSafeLink(link, pov);
            }
            if (!string.IsNullOrWhiteSpace(Circumstance))
            {
                if (PrayToHF != null)
                {
                    eventString += " after praying to " + PrayToHF.ToSafeLink(link, pov);
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
        public PoeticFormCreated()
        {
            FormType = FormType.Poetic;
        }
    }

    public class MusicalFormCreated : FormCreatedEvent
    {
        public MusicalFormCreated()
        {
            FormType = FormType.Musical;
        }
    }

    public class DanceFormCreated : FormCreatedEvent
    {
        public DanceFormCreated()
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "civ_id":
                        Civ = world.GetEntity(property.ValueAsInt());
                        break;
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "hist_figure_id":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "wc_id":
                        WrittenContent = property.Value;
                        break;
                    case "reason":
                        Reason = property.Value;
                        break;
                    case "reason_id":
                        ReasonId = property.ValueAsInt();
                        break;
                    case "circumstance":
                        Circumstance = property.Value;
                        break;
                    case "circumstance_id":
                        CircumstanceId = property.ValueAsInt();
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
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
            if (Circumstance == "pray to hf" || Circumstance == "dream about hf")
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
            eventString += HistoricalFigure.ToSafeLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            if (GlorifiedHF != null)
            {
                eventString += " in order to glorify " + GlorifiedHF.ToSafeLink(link, pov);
            }
            if (!string.IsNullOrWhiteSpace(Circumstance))
            {
                if (CircumstanceHF != null)
                {
                    switch (Circumstance)
                    {
                        case "pray to hf":
                            eventString += " after praying to " + CircumstanceHF.ToSafeLink(link, pov);
                            break;
                        case "dream about hf":
                            eventString += " after dreaming of " + CircumstanceHF.ToSafeLink(link, pov);
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "hfid":
                        HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "knowledge":
                        Knowledge = property.Value.Split(':');
                        break;
                    case "first":
                        First = true;
                        property.Known = true;
                        break;
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
            eventString += HistoricalFigure.ToSafeLink(link, pov);
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

        private void InternalMerge(List<Property> properties, World world)
        {
            foreach (Property property in properties)
                switch (property.Name)
                {
                    case "site_id":
                        Site = world.GetSite(property.ValueAsInt());
                        break;
                    case "subregion_id":
                        Region = world.GetRegion(property.ValueAsInt());
                        break;
                    case "feature_layer_id":
                        UndergroundRegion = world.GetUndergroundRegion(property.ValueAsInt());
                        break;
                    case "seeker_hfid":
                        Seeker = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "target_hfid":
                        Target = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
                    case "relationship":
                        Relationship = property.Value;
                        break;
                    case "reason":
                        Reason = property.Value;
                        break;
                    case "reason_id":
                        ReasonHF = world.GetHistoricalFigure(property.ValueAsInt());
                        break;
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
            eventString += Seeker.ToSafeLink(link, pov);
            eventString += " was denied ";
            switch (Relationship)
            {
                case "apprentice":
                    eventString += "an apprenticeship under";
                    break;
                default:
                    break;
            }
            eventString += Target.ToSafeLink(link, pov);
            if (Site != null)
            {
                eventString += " in ";
                eventString += Site.ToSafeLink(link, pov);
            }
            if (ReasonHF != null)
            {
                switch (Reason)
                {
                    case "jealousy":
                        eventString += " due to jealousy of " + ReasonHF.ToSafeLink(link, pov);
                        break;
                    case "prefers working alone":
                        eventString += " as " + ReasonHF.ToSafeLink(link, pov) + " prefers to work alone";
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
