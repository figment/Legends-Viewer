using System;
using System.Collections.Generic;
using System.Linq;
using LegendsViewer.Controls;

namespace LegendsViewer.Legends
{
    class XMLPlusParser : XMLParser
    {
        public XMLPlusParser(World world, string xmlFile) : base(world, xmlFile)
        {
        }

        protected override Section GetSectionType(string sectionName)
        {
            switch (sectionName)
            {
                case "name": return Section.Name;
                case "altname": return Section.AltName;
                default:
                    return base.GetSectionType(sectionName);
            }
        }

        protected override void ParseSection()
        {
            if (CurrentSection == Section.Name || CurrentSection == Section.AltName)
            {
                AddItemToWorld(new List<Property>(new[] { new Property() { Name = XML.Name, Value = XML.ReadElementString() } }));
            }
            else
            {
                base.ParseSection();
            }
        }

        protected override void AddItemToWorld(List<Property> properties)
        {
            if (CurrentSection == Section.Name)
                World.Name = string.Intern(properties.FirstOrDefault()?.Value);
            else if (CurrentSection == Section.AltName)
                World.AltName = string.Intern(properties.FirstOrDefault()?.Value);
            else
                base.AddItemToWorld(properties);

            //foreach (Property property in properties)
            //{
            //    string section = "";
            //    if (CurrentSection == Section.Events || CurrentSection == Section.EventCollections)
            //        section = eventType;
            //    else
            //        section = CurrentSection.ToString();
            //    if (!property.Known && property.SubProperties.Count == 0)
            //    {
            //        World.ParsingErrors.Report("Unknown " + section + " Property: " + property.Name, property.Value);
            //    }
            //    foreach (Property subProperty in property.SubProperties)
            //    {
            //        if (!subProperty.Known)
            //            World.ParsingErrors.Report("Unknown " + section + " Property: " + property.Name + " - " + subProperty.Name, subProperty.Value);
            //    }
            //}
        }


        // merge objects
        //protected override void AddFromXMLSection(Section section, List<Property> properties)
        //{
        //    var id = properties.Where(x => x.Name == "id").Select(x => new int? (Convert.ToInt32(x.Value))).FirstOrDefault();
        //    if (id.HasValue && id.Value > -1)
        //    {
        //        switch (section)
        //        {
        //            case Section.Regions: World.GetRegion(id.Value)?.Merge(properties, World); break;
        //            case Section.UndergroundRegions: World.GetUndergroundRegion(id.Value)?.Merge(properties, World); break;
        //            case Section.Sites: World.GetSite(id.Value)?.Merge(properties, World); break;
        //            case Section.HistoricalFigures: World.GetHistoricalFigure(id.Value)?.Merge(properties, World); break;
        //            case Section.EntityPopulations: World.GetEntityPopulation(id.Value)?.Merge(properties, World); break;
        //            case Section.Entities: World.GetEntity(id.Value)?.Merge(properties, World); break;
        //            case Section.Eras: World.GetEra(id.Value)?.Merge(properties, World); break;
        //            case Section.Artifacts: World.GetArtifact(id.Value)?.Merge(properties, World); break;
        //            case Section.WorldConstructions: break;
        //            default: World.ParsingErrors.Report("Unknown XML Section: " + section.ToString()); break;
        //        }
        //    }
        //    else
        //    {
        //        World.ParsingErrors.Report("XML Plus Section with non-unique id" + section.ToString());
        //    }
        //}

        //protected virtual void AddEvent(string type, List<Property> properties)
        //{
        //    switch (type)
        //    {
        //        case "add hf entity link": World.Events.Add(new AddHFEntityLink(properties, World)); break;
        //        case "add hf hf link": World.Events.Add(new AddHFHFLink(properties, World)); break;
        //        case "attacked site": World.Events.Add(new AttackedSite(properties, World)); break;
        //        case "body abused": World.Events.Add(new BodyAbused(properties, World)); break;
        //        case "change hf job": World.Events.Add(new ChangeHFJob(properties, World)); break;
        //        case "change hf state": World.Events.Add(new ChangeHFState(properties, World)); break;
        //        case "changed creature type": World.Events.Add(new ChangedCreatureType(properties, World)); break;
        //        case "create entity position": World.Events.Add(new CreateEntityPosition(properties, World)); break;
        //        case "created site": World.Events.Add(new CreatedSite(properties, World)); break;
        //        case "created world construction": World.Events.Add(new CreatedWorldConstruction(properties, World)); break;
        //        case "creature devoured": World.Events.Add(new CreatureDevoured(properties, World)); break;
        //        case "destroyed site": World.Events.Add(new DestroyedSite(properties, World)); break;
        //        case "field battle": World.Events.Add(new FieldBattle(properties, World)); break;
        //        case "hf abducted": World.Events.Add(new HFAbducted(properties, World)); break;
        //        case "hf died": World.Events.Add(new HFDied(properties, World)); break;
        //        case "hf new pet": World.Events.Add(new HFNewPet(properties, World)); break;
        //        case "hf reunion": World.Events.Add(new HFReunion(properties, World)); break;
        //        case "hf simple battle event": World.Events.Add(new HFSimpleBattleEvent(properties, World)); break;
        //        case "hf travel": World.Events.Add(new HFTravel(properties, World)); break;
        //        case "hf wounded": World.Events.Add(new HFWounded(properties, World)); break;
        //        case "impersonate hf": World.Events.Add(new ImpersonateHF(properties, World)); break;
        //        case "item stolen": World.Events.Add(new ItemStolen(properties, World)); break;
        //        case "new site leader": World.Events.Add(new NewSiteLeader(properties, World)); break;
        //        case "peace accepted": World.Events.Add(new PeaceAccepted(properties, World)); break;
        //        case "peace rejected": World.Events.Add(new PeaceRejected(properties, World)); break;
        //        case "plundered site": World.Events.Add(new PlunderedSite(properties, World)); break;
        //        case "reclaim site": World.Events.Add(new ReclaimSite(properties, World)); break;
        //        case "remove hf entity link": World.Events.Add(new RemoveHFEntityLink(properties, World)); break;
        //        case "artifact created": World.Events.Add(new ArtifactCreated(properties, World)); break;
        //        case "diplomat lost": World.Events.Add(new DiplomatLost(properties, World)); break;
        //        case "entity created": World.Events.Add(new EntityCreated(properties, World)); break;
        //        case "hf revived": World.Events.Add(new HFRevived(properties, World)); break;
        //        case "masterpiece arch design": World.Events.Add(new MasterpieceArchDesign(properties, World)); break;
        //        case "masterpiece arch constructed": World.Events.Add(new MasterpieceArchConstructed(properties, World)); break;
        //        case "masterpiece engraving": World.Events.Add(new MasterpieceEngraving(properties, World)); break;
        //        case "masterpiece food": World.Events.Add(new MasterpieceFood(properties, World)); break;
        //        case "masterpiece lost": World.Events.Add(new MasterpieceLost(properties, World)); break;
        //        case "masterpiece item": World.Events.Add(new MasterpieceItem(properties, World)); break;
        //        case "masterpiece item improvement": World.Events.Add(new MasterpieceItemImprovement(properties, World)); break;
        //        case "merchant": World.Events.Add(new Merchant(properties, World)); break;
        //        case "site abandoned": World.Events.Add(new SiteAbandoned(properties, World)); break;
        //        case "site died": World.Events.Add(new SiteDied(properties, World)); break;
        //        case "add hf site link": World.Events.Add(new AddHFSiteLink(properties, World)); break;
        //        case "created structure": World.Events.Add(new CreatedStructure(properties, World)); break;
        //        case "hf razed structure": World.Events.Add(new HFRazedStructure(properties, World)); break;
        //        case "remove hf site link": World.Events.Add(new RemoveHFSiteLink(properties, World)); break;
        //        case "replaced structure": World.Events.Add(new ReplacedStructure(properties, World)); break;
        //        case "site taken over": World.Events.Add(new SiteTakenOver(properties, World)); break;
        //        case "entity relocate": World.Events.Add(new EntityRelocate(properties, World)); break;
        //        case "hf gains secret goal": World.Events.Add(new HFGainsSecretGoal(properties, World)); break;
        //        case "hf profaned structure": World.Events.Add(new HFProfanedStructure(properties, World)); break;
        //        case "hf does interaction": World.Events.Add(new HFDoesInteraction(properties, World)); break;
        //        case "entity primary criminals": World.Events.Add(new EntityPrimaryCriminals(properties, World)); break;
        //        case "hf confronted": World.Events.Add(new HFConfronted(properties, World)); break;
        //        case "assume identity": World.Events.Add(new AssumeIdentity(properties, World)); break;
        //        case "entity law": World.Events.Add(new EntityLaw(properties, World)); break;
        //        case "change hf body state": World.Events.Add(new ChangeHFBodyState(properties, World)); break;
        //        case "razed structure": World.Events.Add(new RazedStructure(properties, World)); break;
        //        case "hf learns secret": World.Events.Add(new HFLearnsSecret(properties, World)); break;
        //        case "artifact stored": World.Events.Add(new ArtifactStored(properties, World)); break;
        //        case "artifact possessed": World.Events.Add(new ArtifactPossessed(properties, World)); break;
        //        case "agreement made": World.Events.Add(new AgreementMade(properties, World)); break;
        //        case "artifact lost": World.Events.Add(new ArtifactLost(properties, World)); break;
        //        case "site dispute": World.Events.Add(new SiteDispute(properties, World)); break;
        //        case "hf attacked site": World.Events.Add(new HfAttackedSite(properties, World)); break;
        //        case "hf destroyed site": World.Events.Add(new HfDestroyedSite(properties, World)); break;
        //        case "agreement formed": World.Events.Add(new AgreementFormed(properties, World)); break;
        //        case "site tribute forced": World.Events.Add(new SiteTributeForced(properties, World)); break;
        //        case "insurrection started": World.Events.Add(new InsurrectionStarted(properties, World)); break;
        //        case "procession": World.Events.Add(new Procession(properties, World)); break;
        //        case "ceremony": World.Events.Add(new Ceremony(properties, World)); break;
        //        case "performance": World.Events.Add(new Performance(properties, World)); break;
        //        case "competition": World.Events.Add(new Competition(properties, World)); break;
        //        case "written content composed": World.Events.Add(new WrittenContentComposed(properties, World)); break;
        //        case "poetic form created": World.Events.Add(new PoeticFormCreated(properties, World)); break;
        //        case "musical form created": World.Events.Add(new MusicalFormCreated(properties, World)); break;
        //        case "dance form created": World.Events.Add(new DanceFormCreated(properties, World)); break;
        //        case "knowledge discovered": World.Events.Add(new KnowledgeDiscovered(properties, World)); break;
        //        case "hf relationship denied": World.Events.Add(new HFRelationShipDenied(properties, World)); break;
        //        case "regionpop incorporated into entity": World.Events.Add(new RegionpopIncorporatedIntoEntity(properties, World)); break;
        //        case "artifact destroyed": World.Events.Add(new ArtifactDestroyed(properties, World)); break;
        //        case "hf disturbed structure":
        //            break;
        //        default:
        //            World.ParsingErrors.Report("Unknown Event: " + type);
        //            break;
        //    }
        //}

        //protected virtual void AddEventCollection(string type, List<Property> properties)
        //{
        //    switch (type)
        //    {
        //        case "abduction": World.EventCollections.Add(new Abduction(properties, World)); break;
        //        case "battle": World.EventCollections.Add(new Battle(properties, World)); break;
        //        case "beast attack": World.EventCollections.Add(new BeastAttack(properties, World)); break;
        //        case "duel": World.EventCollections.Add(new Duel(properties, World)); break;
        //        case "journey": World.EventCollections.Add(new Journey(properties, World)); break;
        //        case "site conquered": World.EventCollections.Add(new SiteConquered(properties, World)); break;
        //        case "theft": World.EventCollections.Add(new Theft(properties, World)); break;
        //        case "war": World.EventCollections.Add(new War(properties, World)); break;
        //        case "insurrection": World.EventCollections.Add(new Insurrection(properties, World)); break;
        //        case "occasion": World.EventCollections.Add(new Occasion(properties, World)); break;
        //        case "procession": World.EventCollections.Add(new ProcessionCollection(properties, World)); break;
        //        case "ceremony": World.EventCollections.Add(new CeremonyCollection(properties, World)); break;
        //        case "performance": World.EventCollections.Add(new PerformanceCollection(properties, World)); break;
        //        case "competition": World.EventCollections.Add(new CompetitionCollection(properties, World)); break;
        //        default: World.ParsingErrors.Report("Unknown Event Collection: " + type); break;
        //    }
        //}

        protected override void ProcessXMLSection(Section section)
        {
            if (section == Section.HistoricalFigures)
                return;
            base.ProcessXMLSection(section);
        }

    }
}
