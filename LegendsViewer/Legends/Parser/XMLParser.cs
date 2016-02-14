using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using LegendsViewer.Legends.EventCollections;
using LegendsViewer.Legends.Events;

namespace LegendsViewer.Legends.Parser
{
    class XMLParser
    {
        protected World World;
        protected XmlTextReader XML;
        protected string CurrentSectionName = "";
        protected Section CurrentSection = Section.Unknown;
        protected string CurrentItemName = "";


        protected XMLParser(string xmlFile)
        {
            StreamReader reader = new StreamReader(xmlFile, Encoding.GetEncoding("windows-1252"));
            XML = new XmlTextReader(reader);
            XML.WhitespaceHandling = WhitespaceHandling.Significant;
        }

        public XMLParser(World world, string xmlFile) : this(xmlFile)
        {
            World = world;
        }

        public static string SafeXMLFile(string xmlFile)
        {
            DialogResult response = MessageBox.Show("There was an error loading this XML file! Do you wish to attempt a repair?", "Error loading XML", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (response == DialogResult.Yes)
            {
                string currentLine = String.Empty;
                string safeFile = Path.GetTempFileName();
                using (FileStream inputStream = File.OpenRead(xmlFile))
                {
                    using (StreamReader inputReader = new StreamReader(inputStream))
                    {
                        using (StreamWriter outputWriter = File.AppendText(safeFile))
                        {
                            while (null != (currentLine = inputReader.ReadLine()))
                            {
                                outputWriter.WriteLine(Regex.Replace(currentLine, "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]", string.Empty));
                            }
                        }
                    }
                }
                DialogResult overwrite = MessageBox.Show("Repair completed. Would you like to overwrite the original file with the repaired version? (Note: No effect if opened from an archive)", "Repair Completed", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (overwrite == DialogResult.Yes)
                {
                    File.Delete(xmlFile);
                    File.Copy(safeFile, xmlFile);
                    return xmlFile;
                }
                return safeFile;
            }
            return null;

        }

        public void Parse()
        {
            while (!XML.EOF)
            {
                CurrentSection = GetSectionType(XML.Name);
                if (CurrentSection == Section.Junk)
                {
                    XML.Read();
                }
                else if (CurrentSection == Section.Unknown)
                    SkipSection();
                else
                    ParseSection();
            }
            XML.Close();
        }

        protected virtual void GetSectionStart()
        {
            CurrentSectionName = "";
            if (XML.NodeType == XmlNodeType.Element)
                CurrentSectionName = XML.Name;
            CurrentSection = GetSectionType(CurrentSectionName);
        }

        protected virtual Section GetSectionType(string sectionName)
        {
            switch (sectionName)
            {
                case "artifacts": return Section.Artifacts;
                case "entities": return Section.Entities;
                case "entity_populations": return Section.EntityPopulations;
                case "historical_eras": return Section.Eras;
                case "historical_event_collections": return Section.EventCollections;
                case "historical_events": return Section.Events;
                case "historical_figures": return Section.HistoricalFigures;
                case "regions": return Section.Regions;
                case "sites": return Section.Sites;
                case "underground_regions": return Section.UndergroundRegions;
                case "world_constructions": return Section.WorldConstructions;
                case "poetic_forms": return Section.PoeticForms;
                case "musical_forms": return Section.MusicalForms;
                case "dance_forms": return Section.DanceForms;
                case "written_contents": return Section.WrittenContent;
                case "landmasses": return Section.Landmasses;
                case "mountain_peaks": return Section.MountainPeaks;
                case "xml":
                case "":
                case "df_world": return Section.Junk;
                case "name": return Section.Name;
                case "altname": return Section.AltName;
                default: World.ParsingErrors.Report("Unknown XML Section: " + sectionName); return Section.Unknown;
            }
        }

        protected virtual void ParseSection()
        {
            XML.ReadStartElement();
            while (XML.NodeType != XmlNodeType.EndElement)
            {
                List<Property> item = ParseItem();
                AddItemToWorld(item);
            }
            ProcessXMLSection(CurrentSection); //Done with section, do post processing
            XML.ReadEndElement();
        }

        protected virtual void SkipSection()
        {
            string currentSectionName = XML.Name;
            XML.ReadStartElement();
            while (!(XML.NodeType == XmlNodeType.EndElement && XML.Name == currentSectionName))
            {
                XML.Read();
            }
            XML.ReadEndElement();
        }

        public virtual List<Property> ParseItem()
        {
            CurrentItemName = XML.Name;
            if (XML.NodeType == XmlNodeType.EndElement)
                return null;

            XML.ReadStartElement();
            List<Property> properties = new List<Property>();
            while (XML.NodeType != XmlNodeType.EndElement && XML.Name != CurrentItemName)
            {
                properties.Add(ParseProperty());
            }
            XML.ReadEndElement();
            return properties;
        }

        protected virtual Property ParseProperty()
        {
            Property property = new Property();

            if (string.IsNullOrWhiteSpace(XML.Name))
            {
                return null;
            }

            if (XML.IsEmptyElement) //Need this for bugged XML properties that only have and end element like "</deity>" for historical figures.
            {
                property.Name = XML.Name;
                XML.ReadStartElement();
                return property;
            }

            property.Name = XML.Name;
            XML.ReadStartElement();

            if (XML.NodeType == XmlNodeType.Text)
            {
                property.Value = XML.Value;
                XML.Read();
            }
            else if (XML.NodeType == XmlNodeType.Element)
            {
                while (XML.NodeType != XmlNodeType.EndElement)
                {
                    property.SubProperties.Add(ParseProperty());
                }
            }

            XML.ReadEndElement();
            return property;

        }

        protected virtual void AddItemToWorld(List<Property> properties)
        {
            string eventType = "";
            if (CurrentSection == Section.Events || CurrentSection == Section.EventCollections)
                eventType = properties.First(property => property.Name == "type").Value.Replace('_', ' ');

            if (CurrentSection == Section.EventCollections)
            {
                AddEventCollection(eventType, properties);
            }
            else if (CurrentSection == Section.Events)
            {
                AddEvent(eventType, properties);
            }
            else
            {
                AddFromXMLSection(CurrentSection, properties);
            }

            foreach (Property property in properties)
            {
                string section = "";
                if (CurrentSection == Section.Events || CurrentSection == Section.EventCollections)
                    section = eventType;
                else
                    section = CurrentSection.ToString();

                if (!property.Known && property.SubProperties.Count == 0)
                {
                    World.ParsingErrors.Report("Unknown " + section + " Property: " + property.Name, property.Value);
                }
                foreach (Property subProperty in property.SubProperties)
                {
                    if (!subProperty.Known)
                        World.ParsingErrors.Report("Unknown " + section + " Property: " + property.Name + " - " + subProperty.Name, subProperty.Value);
                }
            }
        }


        protected virtual void AddFromXMLSection(Section section, List<Property> properties)
        {
            try
            {
            switch (section)
            {
                case Section.Regions: World.UpsertWorldObject(World.Regions, properties, World); break;
                case Section.UndergroundRegions: World.UpsertWorldObject(World.UndergroundRegions, properties, World); break;
                case Section.Sites: World.UpsertWorldObject(World.Sites, properties, World); break;
                case Section.HistoricalFigures: World.UpsertWorldObject(World.HistoricalFigures, properties, World); break;
                case Section.EntityPopulations: World.UpsertWorldObject(World.EntityPopulations, properties, World); break;
                case Section.Entities: World.UpsertWorldObject(World.Entities, properties, World); break;
                case Section.Eras: World.Eras.Add(new Era(properties, World)); break;
                case Section.Artifacts: World.UpsertWorldObject(World.Artifacts, properties, World); break;
                case Section.WorldConstructions: World.UpsertWorldObject(World.WorldConstructions, properties, World); break;
                case Section.PoeticForms: World.UpsertWorldObject(World.PoeticForms,properties, World); break;
                case Section.MusicalForms: World.UpsertWorldObject(World.MusicalForms,properties, World); break;
                case Section.DanceForms: World.UpsertWorldObject(World.DanceForms, properties, World); break;
                case Section.WrittenContent: World.UpsertWorldObject(World.WrittenContents, properties, World); break;
                case Section.Landmasses: World.UpsertWorldObject(World.Landmasses, properties, World); break;
                case Section.MountainPeaks: World.UpsertWorldObject(World.MountainPeaks, properties, World); break;
                default: World.ParsingErrors.Report("Unknown XML Section: " + section.ToString()); break;
            }
        }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                World.ParsingErrors.Report("ERROR Parsing Section: " + section);
            }
        }

        protected virtual void AddEvent(string type, List<Property> properties)
        {
            try
            {
            switch (type)
            {
                case "add hf entity link": World.UpsertEvent<AddHFEntityLink>(World.Events, properties, World); break;
                case "add hf hf link": World.UpsertEvent<AddHFHFLink>(World.Events, properties, World); break;
                case "attacked site": World.UpsertEvent<AttackedSite>(World.Events, properties, World); break;
                case "body abused": World.UpsertEvent<BodyAbused>(World.Events, properties, World); break;
                case "change hf job": World.UpsertEvent<ChangeHFJob>(World.Events, properties, World); break;
                case "change hf state": World.UpsertEvent<ChangeHFState>(World.Events, properties, World); break;
                case "change creature type":
                case "changed creature type": World.UpsertEvent<ChangedCreatureType>(World.Events, properties, World); break;
                case "create entity position": World.UpsertEvent<CreateEntityPosition>(World.Events, properties, World); break;
                case "created site": World.UpsertEvent<CreatedSite>(World.Events, properties, World); break;
                case "created world construction": World.UpsertEvent<CreatedWorldConstruction>(World.Events, properties, World); break;
                case "creature devoured": World.UpsertEvent<CreatureDevoured>(World.Events, properties, World); break;
                case "destroyed site": World.UpsertEvent<DestroyedSite>(World.Events, properties, World); break;
                case "field battle": World.UpsertEvent<FieldBattle>(World.Events, properties, World); break;
                case "hf abducted": World.UpsertEvent<HFAbducted>(World.Events, properties, World); break;
                case "hist figure died": 
                case "hf died": World.UpsertEvent<HFDied>(World.Events, properties, World); break;
                case "hist figure new pet":
                case "hf new pet": World.UpsertEvent<HFNewPet>(World.Events, properties, World); break;
                case "hist figure reunion":
                case "hf reunion": World.UpsertEvent<HFReunion>(World.Events, properties, World); break;
                case "hf simple battle event": World.UpsertEvent<HFSimpleBattleEvent>(World.Events, properties, World); break;
                case "hist figure travel":
                case "hf travel": World.UpsertEvent<HFTravel>(World.Events, properties, World); break;
                case "hist figure wounded":
                case "hf wounded": World.UpsertEvent<HFWounded>(World.Events, properties, World); break;
                case "impersonate hf": World.UpsertEvent<ImpersonateHF>(World.Events, properties, World); break;
                case "item stolen": World.UpsertEvent<ItemStolen>(World.Events, properties, World); break;
                case "new site leader": World.UpsertEvent<NewSiteLeader>(World.Events, properties, World); break;
                case "war peace accepted":
                case "peace accepted": World.UpsertEvent<PeaceAccepted>(World.Events, properties, World); break;
                case "war peace rejected":
                case "peace rejected": World.UpsertEvent<PeaceRejected>(World.Events, properties, World); break;
                case "plundered site": World.UpsertEvent<PlunderedSite>(World.Events, properties, World); break;
                case "reclaim site": World.UpsertEvent<ReclaimSite>(World.Events, properties, World); break;
                case "remove hf entity link": World.UpsertEvent<RemoveHFEntityLink>(World.Events, properties, World); break;
                case "artifact created": World.UpsertEvent<ArtifactCreated>(World.Events, properties, World); break;
                case "diplomat lost": World.UpsertEvent<DiplomatLost>(World.Events, properties, World); break;
                case "entity created": World.UpsertEvent<EntityCreated>(World.Events, properties, World); break;
                case "hf revived": World.UpsertEvent<HFRevived>(World.Events, properties, World); break;
                case "masterpiece arch design": World.UpsertEvent<MasterpieceArchDesign>(World.Events, properties, World); break;
                case "masterpiece arch constructed": World.UpsertEvent<MasterpieceArchConstructed>(World.Events, properties, World); break;
                case "masterpiece engraving": World.UpsertEvent<MasterpieceEngraving>(World.Events, properties, World); break;
                case "masterpiece food": World.UpsertEvent<MasterpieceFood>(World.Events, properties, World); break;
                case "masterpiece lost": World.UpsertEvent<MasterpieceLost>(World.Events, properties, World); break;
                case "masterpiece item": World.UpsertEvent<MasterpieceItem>(World.Events, properties, World); break;
                case "masterpiece item improvement": World.UpsertEvent<MasterpieceItemImprovement>(World.Events, properties, World); break;
                case "merchant": World.UpsertEvent<Merchant>(World.Events, properties, World); break;
                case "site abandoned": World.UpsertEvent<SiteAbandoned>(World.Events, properties, World); break;
                case "site died": World.UpsertEvent<SiteDied>(World.Events, properties, World); break;
                case "add hf site link": World.UpsertEvent<AddHFSiteLink>(World.Events, properties, World); break;
                case "created building":
                case "created structure": World.UpsertEvent<CreatedStructure>(World.Events, properties, World); break;
                case "hf razed structure": World.UpsertEvent<HFRazedStructure>(World.Events, properties, World); break;
                case "remove hf site link": World.UpsertEvent<RemoveHFSiteLink>(World.Events, properties, World); break;
                case "replaced building":
                case "replaced structure": World.UpsertEvent<ReplacedStructure>(World.Events, properties, World); break;
                case "site taken over": World.UpsertEvent<SiteTakenOver>(World.Events, properties, World); break;
                case "entity relocate": World.UpsertEvent<EntityRelocate>(World.Events, properties, World); break;
	            case "hf gains secret goal": World.UpsertEvent<HFGainsSecretGoal>(World.Events, properties, World); break;
	            case "hf profaned structure": World.UpsertEvent<HFProfanedStructure>(World.Events, properties, World); break;
	            case "hf does interaction": World.UpsertEvent<HFDoesInteraction>(World.Events, properties, World); break;
	            case "entity primary criminals": World.UpsertEvent<EntityPrimaryCriminals>(World.Events, properties, World); break;
                case "hf confronted": World.UpsertEvent<HFConfronted>(World.Events, properties, World); break;
                case "assume identity": World.UpsertEvent<AssumeIdentity>(World.Events, properties, World); break;
	            case "entity law": World.UpsertEvent<EntityLaw>(World.Events, properties, World); break;
	            case "change hf body state": World.UpsertEvent<ChangeHFBodyState>(World.Events, properties, World); break;
                case "razed structure": World.UpsertEvent<RazedStructure>(World.Events, properties, World); break;
                case "hf learns secret": World.UpsertEvent<HFLearnsSecret>(World.Events, properties, World); break;
                case "artifact stored": World.UpsertEvent<ArtifactStored>(World.Events, properties, World); break;
                case "artifact possessed": World.UpsertEvent<ArtifactPossessed>(World.Events, properties, World); break;
                case "agreement made": World.UpsertEvent<AgreementMade>(World.Events, properties, World); break;
                case "artifact lost": World.UpsertEvent<ArtifactLost>(World.Events, properties, World); break;
                case "site dispute": World.UpsertEvent<SiteDispute>(World.Events, properties, World); break;
                case "hf attacked site": World.UpsertEvent<HfAttackedSite>(World.Events, properties, World); break;
                case "hf destroyed site": World.UpsertEvent<HfDestroyedSite>(World.Events, properties, World); break;
                case "agreement formed": World.UpsertEvent<AgreementFormed>(World.Events, properties, World); break;
                case "site tribute forced": World.UpsertEvent<SiteTributeForced>(World.Events, properties, World); break;
                case "insurrection started": World.UpsertEvent<InsurrectionStarted>(World.Events, properties, World); break;
                case "procession": World.UpsertEvent<Procession>(World.Events, properties, World); break;
                case "ceremony": World.UpsertEvent<Ceremony>(World.Events, properties, World); break;
                case "performance": World.UpsertEvent<Performance>(World.Events, properties, World); break;
                case "competition": World.UpsertEvent<Competition>(World.Events, properties, World); break;
                case "written content composed": World.UpsertEvent<WrittenContentComposed>(World.Events, properties, World); break;
                case "poetic form created": World.UpsertEvent<PoeticFormCreated>(World.Events, properties, World); break;
                case "musical form created": World.UpsertEvent<MusicalFormCreated>(World.Events, properties, World); break;
                case "dance form created": World.UpsertEvent<DanceFormCreated>(World.Events, properties, World); break;
                case "knowledge discovered": World.UpsertEvent<KnowledgeDiscovered>(World.Events, properties, World); break;
                case "hf relationship denied": World.UpsertEvent<HFRelationShipDenied>(World.Events, properties, World); break;
                case "regionpop incorporated into entity": World.UpsertEvent<RegionpopIncorporatedIntoEntity>(World.Events, properties, World); break;
                case "artifact destroyed": World.UpsertEvent<ArtifactDestroyed>(World.Events, properties, World); break;
                case "entity action": EntityAction.DelegateUpsertEvent(World.Events, properties, World); break;
                case "hf act on building": HFActOnBuilding.DelegateUpsertEvent(World.Events, properties, World); break;
                case "hf disturbed structure": World.UpsertEvent<HFDisturbedStructure>(World.Events, properties, World); break;
                default:
                    World.ParsingErrors.Report("Unknown Event: " + type);
                break;
            }
        }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                World.ParsingErrors.Report("ERROR Parsing Event: " + type);
            }
        }

        protected virtual void AddEventCollection(string type, List<Property> properties)
        {
            try
            {
            switch (type)
            {
                case "abduction": World.UpsertEventCol<Abduction>(World.EventCollections, properties, World); break;
                case "battle": World.UpsertEventCol<Battle>(World.EventCollections, properties, World); break;
                case "beast attack": World.UpsertEventCol<BeastAttack>(World.EventCollections, properties, World); break;
                case "duel": World.UpsertEventCol<Duel>(World.EventCollections, properties, World); break;
                case "journey": World.UpsertEventCol<Journey>(World.EventCollections, properties, World); break;
                case "site conquered": World.UpsertEventCol<SiteConquered>(World.EventCollections, properties, World); break;
                case "theft": World.UpsertEventCol<Theft>(World.EventCollections, properties, World); break;
                case "war": World.UpsertEventCol<War>(World.EventCollections, properties, World); break;
                case "insurrection": World.UpsertEventCol<Insurrection>(World.EventCollections, properties, World); break;
                case "occasion": World.UpsertEventCol<Occasion>(World.EventCollections, properties, World); break;
                case "procession": World.UpsertEventCol<ProcessionCollection>(World.EventCollections, properties, World); break;
                case "ceremony": World.UpsertEventCol<CeremonyCollection>(World.EventCollections, properties, World); break;
                case "performance": World.UpsertEventCol<PerformanceCollection>(World.EventCollections, properties, World); break;
                case "competition": World.UpsertEventCol<CompetitionCollection>(World.EventCollections, properties, World); break;
                case "purge": World.UpsertEventCol<PurgeCollection>(World.EventCollections, properties, World); break;
                default: World.ParsingErrors.Report("Unknown Event Collection: " + type); break;
            }
        }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                World.ParsingErrors.Report("ERROR Parsing Event Collection: " + type);
            }
        }

        protected virtual void ProcessXMLSection(Section section)
        {
            if (section == Section.Events)
            {
                //Calculate Historical Figure Ages.
                int lastYear = World.Events.Last().Year;
                foreach (HistoricalFigure hf in World.HistoricalFigures)
                {
                    if (hf.DeathYear > 0)
                        hf.Age = hf.DeathYear - hf.BirthYear;
                    else
                        hf.Age = lastYear - hf.BirthYear;
                }
            }

            if (section == Section.EventCollections)
            {
                ProcessCollections();
            }

            //Create sorted Historical Figures so they can be binary searched by name, needed for parsing History file
            if (section == Section.HistoricalFigures)
            {
                World.HistoricalFiguresByName = new List<HistoricalFigure>(World.HistoricalFigures);
                World.HistoricalFiguresByName.Sort((a, b) => String.Compare(a.Name, b.Name));
                World.ProcessHFtoHFLinks();
                World.ProcessHFCurrentIdentities();
                World.ProcessHFUsedIdentities();
            }

            //Create sorted entities so they can be binary searched by name, needed for History/sites files
            if (section == Section.Entities)
            {
                World.EntitiesByName = new List<Entity>(World.Entities);
                World.EntitiesByName.Sort((a, b) => String.Compare(a.Name, b.Name));
                World.ProcessReputations();
                World.ProcessHFtoSiteLinks();
                World.ProcessEntityEntityLinks();
            }

            //Calculate end years for eras and add list of wars during era.
            if (section == Section.Eras)
            {
                World.Eras.Last().EndYear = World.Events.Last().Year;
                for (int i = 0; i < World.Eras.Count - 1; i++)
                    World.Eras[i].EndYear = World.Eras[i + 1].StartYear - 1;
                foreach (Era era in World.Eras)
                {
                    era.Events = World.Events.Where(events => events.Year >= era.StartYear && events.Year <= era.EndYear).OrderBy(events => events.Year).ToList();
                    era.Wars = World.EventCollections.OfType<War>().Where(war => (war.StartYear >= era.StartYear && war.EndYear <= era.EndYear && war.EndYear != -1) //entire war between
                                                                                                    || (war.StartYear >= era.StartYear && war.StartYear <= era.EndYear) //war started before & ended
                                                                                                    || (war.EndYear >= era.StartYear && war.EndYear <= era.EndYear && war.EndYear != -1) //war started during
                                                                                                    || (war.StartYear <= era.StartYear && war.EndYear >= era.EndYear) //war started before & ended after
                                                                                                    || (war.StartYear <= era.StartYear && war.EndYear == -1)).ToList();
                }
                
            }
        }

        protected virtual void ProcessCollections()
        {
            World.Wars = World.EventCollections.OfType<War>().ToList();
            World.Battles = World.EventCollections.OfType<Battle>().ToList();
            World.BeastAttacks = World.EventCollections.OfType<BeastAttack>().ToList();

            foreach (EventCollection eventCollection in World.EventCollections)
            {
                //Sub Event Collections aren't created until after the main collection
                //So only IDs are stored in the main collection until here now that all collections have been created
                //and can now be added to their Parent collection
                foreach (int collectionID in eventCollection.CollectionIDs)
                    eventCollection.Collections.Add(World.GetEventCollection(collectionID));
            }

            //Attempt at calculating beast historical figure for beast attacks.
            //Find beast by looking at eventsList and fill in some event properties from the beast attacks's properties
            //Calculated here so it can look in Duel collections contained in beast attacks
            foreach (BeastAttack beastAttack in World.EventCollections.OfType<BeastAttack>())
            {
                if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfAttackedSite>().Any())
                {
                    beastAttack.Beast = beastAttack.GetSubEvents().OfType<HfAttackedSite>().First().Attacker;
                }
                if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HfDestroyedSite>().Any())
                {
                    beastAttack.Beast = beastAttack.GetSubEvents().OfType<HfDestroyedSite>().First().Attacker;
                }

                //Find Beast by looking at fights, Beast always engages the first fight in a Beast Attack?
                if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HFSimpleBattleEvent>().Any())
                {
                    beastAttack.Beast = beastAttack.GetSubEvents().OfType<HFSimpleBattleEvent>().First().HistoricalFigure1;
                }
                if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<AddHFEntityLink>().Any())
                {
                    beastAttack.Beast = beastAttack.GetSubEvents().OfType<AddHFEntityLink>().First().HistoricalFigure;
                }
                if (beastAttack.Beast == null && beastAttack.GetSubEvents().OfType<HFDied>().Any())
                {
                    var slayers = beastAttack.GetSubEvents().OfType<HFDied>().GroupBy(death => death.Slayer).Select(hf => new { HF = hf.Key, Count = hf.Count() });
                    if (slayers.Count(slayer => slayer.Count > 1) == 1)
                    {
                        HistoricalFigure beast = slayers.Single(slayer => slayer.Count > 1).HF;
                        beastAttack.Beast = beast;
                    }
                }


                //Fill in some various event info from collections.

                int insertIndex;
                foreach (ItemStolen theft in beastAttack.Collection.OfType<ItemStolen>())
                {
                    if (theft.Site == null)
                    {
                        theft.Site = beastAttack.Site;
                    }
                    else
                    {
                        beastAttack.Site = theft.Site;
                    }
                    if (theft.Thief == null)
                    {
                        theft.Thief = beastAttack.Beast;
                    }
                    else
                    {
                        beastAttack.Beast = theft.Thief;
                    }

                    if (beastAttack.Site != null)
                    {
                        insertIndex = beastAttack.Site.Events.BinarySearch(theft);
                    if (insertIndex < 0)
                        beastAttack.Site.Events.Insert(~insertIndex, theft);
                    else
                    {
                        // merge
                    }
                    if (beastAttack.Beast != null)
                    {
                        insertIndex = beastAttack.Beast.Events.BinarySearch(theft);
                        if (insertIndex < 0)
                            beastAttack.Beast.Events.Insert(~insertIndex, theft);
                    }
                }
                foreach (CreatureDevoured devoured in beastAttack.Collection.OfType<CreatureDevoured>())
                {
                    if (devoured.Eater == null)
                    {
                        devoured.Eater = beastAttack.Beast;
                    }
                    else
                    {
                        beastAttack.Beast = devoured.Eater;
                    }
                    if (beastAttack.Beast != null)
                    {
                        insertIndex = beastAttack.Beast.Events.BinarySearch(devoured);
                        if (insertIndex < 0)
                            beastAttack.Beast.Events.Insert(~insertIndex, devoured);

                    }
                }

                }
                if (beastAttack.Beast != null)
                {
                    if (beastAttack.Beast.BeastAttacks == null)
                    {
                        beastAttack.Beast.BeastAttacks = new List<BeastAttack>();
            }
                    beastAttack.Beast.BeastAttacks.Add(beastAttack);
                }
            }

            //Assign a Conquering Event its corresponding battle
            //Battle = first Battle prior to the conquering?
            foreach (SiteConquered conquer in World.EventCollections.OfType<SiteConquered>())
            {
                for (int i = conquer.ID - 1; i >= 0; i--)
                {
                    EventCollection collection = World.GetEventCollection(i);
                    if (collection == null) continue;
                    if (collection.GetType() == typeof(Battle))
                    {

                        conquer.Battle = collection as Battle;
                        conquer.Battle.Conquering = conquer;
                        if (conquer.Battle.Defender == null && conquer.Defender != null)
                            conquer.Battle.Defender = conquer.Defender;
                        break;
                    }
                }
            } 
        }
    }
}
