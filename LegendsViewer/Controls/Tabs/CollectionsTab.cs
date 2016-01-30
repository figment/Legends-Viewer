﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using LegendsViewer.Legends;

namespace LegendsViewer.Controls.Tabs
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class CollectionsTab : BaseSearchTab
    {
        private ArtifactsList artifactSearch;
        private WrittenContentList writtenContentSearch;
        private WorldConstructionsList worldConstructionSearch;
        private StructuresList structureSearch;

        public CollectionsTab()
        {
            InitializeComponent();

        }


        internal override void InitializeTab()
        {
            EventTabs = new TabPage[] { tpArtifactsEvents, tpEraEvents, tpStructureEvents, tpWorldConstructionEvents, tpWrittenContentEvents };
            EventTabTypes = new Type[] { typeof(Artifact), typeof(Era), typeof(Structure), typeof(WorldConstruction), typeof(WrittenContent) };

            listArtifactSearch.AllColumns.Add(new OLVColumn
            {
                IsVisible = false, Text = "Creator", TextAlign = HorizontalAlignment.Left,
                AspectGetter = obj => ((Artifact)obj).Creator.Name
            });
            listArtifactSearch.AllColumns.Add(new OLVColumn { AspectName = "SubType", IsVisible = false, Text = "SubType", TextAlign = HorizontalAlignment.Left });
            listArtifactSearch.AllColumns.Add(new OLVColumn { AspectName = "Material", IsVisible = false, Text = "Material", TextAlign = HorizontalAlignment.Left });
            listArtifactSearch.AllColumns.Add(new OLVColumn { AspectName = "Page Count", IsVisible = false, Text = "PageCount", TextAlign = HorizontalAlignment.Right });
            listArtifactSearch.ShowGroups = false;
        }

        internal override void AfterLoad(World world)
        {
            base.AfterLoad(world);



            artifactSearch = new ArtifactsList(World);
            writtenContentSearch = new WrittenContentList(World);
            worldConstructionSearch = new WorldConstructionsList(World);
            structureSearch = new StructuresList(World);

            var structures = from structure in World.Structures
                             orderby structure.Type.GetDescription()
                             group structure by structure.Type.GetDescription() into structuretype
                             select structuretype;
            var worldconstructions = from construction in World.WorldContructions
                                     orderby construction.Type.GetDescription()
                                     group construction by construction.Type.GetDescription() into constructiontype
                                     select constructiontype;
            var writtencontents = from writtenContent in World.WrittenContents
                                  orderby writtenContent.Type.GetDescription()
                                  group writtenContent by writtenContent.Type.GetDescription() into writtenContentType
                                  select writtenContentType;

            var artifactTypes = World.Artifacts.Select(x => x.Type).SkipWhile(string.IsNullOrEmpty).Distinct().OrderBy(x => x);

            var artifactMaterials = World.Artifacts.Select(x => x.Material).SkipWhile(string.IsNullOrEmpty).Distinct().OrderBy(x => x);

            foreach (Era era in World.Eras)
                listEras.Items.Add(era);


            cmbStructureType.Items.Add("All"); cmbStructureType.SelectedIndex = 0;
            foreach (var structure in structures)
                cmbStructureType.Items.Add(structure.Key);
            cmbConstructionType.Items.Add("All"); cmbConstructionType.SelectedIndex = 0;
            foreach (var construction in worldconstructions)
                cmbConstructionType.Items.Add(construction.Key);
            cmbWrittenContentType.Items.Add("All"); cmbWrittenContentType.SelectedIndex = 0;
            foreach (var writtencontent in writtencontents)
                cmbWrittenContentType.Items.Add(writtencontent.Key);

            cbmArtTypeFilter.Items.Add("All"); cbmArtTypeFilter.SelectedIndex = 0;
            cbmArtTypeFilter.Items.AddRange(artifactTypes.ToArray());
            lblArtTypeFilter.Visible = cbmArtTypeFilter.Visible = artifactTypes.Any();

            cbmArtMatFilter.Items.Add("All"); cbmArtMatFilter.SelectedIndex = 0;
            cbmArtMatFilter.Items.AddRange(artifactMaterials.ToArray());
            lblArtMatFilter.Visible = cbmArtMatFilter.Visible = artifactMaterials.Any();

            numStart.Maximum = numEraEnd.Value = numEraEnd.Maximum = World.Events.Last().Year;

            Coordinator.Form.DontRefreshBrowserPages = true;
            foreach (CheckBox eraCheck in tpEraEvents.Controls.OfType<CheckBox>())
                eraCheck.Checked = false;
            Coordinator.Form.DontRefreshBrowserPages = false;


            var artifactEvents = from eventType in World.Artifacts.SelectMany(artifact => artifact.Events)
                                 group eventType by eventType.Type into type
                                 select type.Key;

            var writtenContentEvents = from eventType in World.WrittenContents.SelectMany(element => element.Events)
                                       group eventType by eventType.Type into type
                                       select type.Key;

            var worldConstructionEvents = from eventType in World.WorldContructions.SelectMany(element => element.Events)
                                          group eventType by eventType.Type into type
                                          select type.Key;

            var structureEvents = from eventType in World.Structures.SelectMany(element => element.Events)
                                  group eventType by eventType.Type into type
                                  select type.Key;

            var eventTypes = from eventType in World.Events
                             group eventType by eventType.Type into type
                             select type.Key;

            TabEvents.Clear();
            TabEvents.Add(artifactEvents.ToList());
            TabEvents.Add(eventTypes.ToList());
            TabEvents.Add(structureEvents.ToList());
            TabEvents.Add(worldConstructionEvents.ToList());
            TabEvents.Add(writtenContentEvents.ToList());
        }

        internal override void ResetTab()
        {


            Coordinator.Form.DontRefreshBrowserPages = true;
            foreach (CheckBox eraCheck in tpEraEvents.Controls.OfType<CheckBox>())
                eraCheck.Checked = false;
            Coordinator.Form.DontRefreshBrowserPages = false;

            txtArtifactSearch.Clear();
            listArtifactSearch.SetObjects(new object[0]);
            radArtifactSortNone.Checked = true;

            cbmArtMatFilter.Items.Clear();
            cbmArtTypeFilter.Items.Clear();

            txtWrittenContentSearch.Clear();
            cmbWrittenContentType.Items.Clear();
            listWrittenContentSearch.Items.Clear();
            radWrittenContentSortNone.Checked = true;

            txtWorldConstructionsSearch.Clear();
            cmbConstructionType.Items.Clear();
            listWorldConstructionsSearch.Items.Clear();
            radWorldConstructionsSortNone.Checked = true;

            txtStructuresSearch.Clear();
            cmbStructureType.Items.Clear();
            listStructuresSearch.Items.Clear();
            radStructuresSortNone.Checked = true;

            listEras.Items.Clear();
            numStart.Value = -1;
            numEraEnd.Value = 0;
        }

        private void searchArtifactList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                artifactSearch.Name = txtArtifactSearch.Text;
                artifactSearch.SortEvents = radArtifactSortEvents.Checked;
                artifactSearch.SortFiltered = radArtifactSortFiltered.Checked;
                artifactSearch.Type = cbmArtTypeFilter.SelectedIndex == 0 ? null : cbmArtTypeFilter.SelectedItem.ToString();
                artifactSearch.Material = cbmArtMatFilter.SelectedIndex == 0 ? null : cbmArtMatFilter.SelectedItem.ToString();
                IEnumerable<Artifact> list = artifactSearch.GetList();
                listArtifactSearch.SetObjects(list.ToArray());
                listArtifactSearch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }


        private void btnEraShow_Click(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                Browser.Navigate(ControlOption.HTML, new Era(Convert.ToInt32(numStart.Value), Convert.ToInt32(numEraEnd.Value), World));
            }
        }

        private void ResetArtifactBaseList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                lblArtifactList.Text = "All";
                lblArtifactList.ForeColor = Control.DefaultForeColor;
                lblArtifactList.Font = new Font(lblArtifactList.Font.FontFamily, lblArtifactList.Font.Size, FontStyle.Regular);
                artifactSearch.BaseList = World.Artifacts;
                cbmArtTypeFilter.SelectedIndex = 0;
                cbmArtMatFilter.SelectedIndex = 0;
                searchArtifactList(null, null);
            }
        }

        private void searchWrittenContentList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                writtenContentSearch.Name = txtWrittenContentSearch.Text;
                writtenContentSearch.Type = cmbWrittenContentType.SelectedItem.ToString();
                writtenContentSearch.SortEvents = radWrittenContentSortEvents.Checked;
                writtenContentSearch.SortFiltered = radWrittenContentSortFiltered.Checked;
                IEnumerable<WrittenContent> list = writtenContentSearch.GetList();
                listWrittenContentSearch.Items.Clear();
                listWrittenContentSearch.Items.AddRange(list.ToArray());
            }
        }

        private void searchWorldConstructionList(object sender, EventArgs e)
        {

        }

        private void searchStructureList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                structureSearch.Name = txtStructuresSearch.Text;
                structureSearch.Type = cmbStructureType.SelectedItem.ToString();
                structureSearch.SortEvents = radStructuresSortEvents.Checked;
                structureSearch.SortFiltered = radStructuresSortFiltered.Checked;
                IEnumerable<Structure> list = structureSearch.GetList();
                listStructuresSearch.Items.Clear();
                listStructuresSearch.Items.AddRange(list.ToArray());
            }
        }

        private void listEras_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }

        private void listArtifactSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }

        private void listWrittenContentSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }

        private void listWorldConstructionsSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }

        private void listStructuresSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }
    }
}
