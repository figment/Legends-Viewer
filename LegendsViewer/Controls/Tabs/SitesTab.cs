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
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof (IDesigner))]
    public partial class SitesTab : BaseSearchTab
    {
        private SitesList siteSearch;

        public SitesTab()
        {
            InitializeComponent();

        }


        internal override void InitializeTab()
        {

        }

        internal override void AfterLoad(World world)
        {
            base.AfterLoad(world);
            siteSearch = new SitesList(World);


            var sites = from site in World.Sites
                        orderby site.Type
                        group site by site.Type into sitetype
                        select sitetype;

            var populationTypes = from population in World.SitePopulations
                                  orderby population.Race
                                  group population by population.Race into type
                                  select type;

            cmbSiteType.Items.Add("All"); cmbSiteType.SelectedIndex = 0;
            foreach (var site in sites)
                cmbSiteType.Items.Add(site.Key);

            cmbSitePopulation.Items.Add("All"); cmbSitePopulation.SelectedIndex = 0;
            foreach (var populationType in populationTypes)
                cmbSitePopulation.Items.Add(populationType.Key);

            var siteEvents = from eventType in World.Sites.SelectMany(site => site.Events)
                             group eventType by eventType.Type into type
                             select type.Key;

            TabEvents.Clear();
            TabEvents.Add(siteEvents.ToList());
        }

        internal override void ResetTab()
        {

            lblSiteList.Text = "All";
            lblSiteList.ForeColor = DefaultForeColor;
            lblSiteList.Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);

            txtSiteSearch.Clear();
            listSiteSearch.Items.Clear();
            cmbSiteType.Items.Clear();
            cmbSitePopulation.Items.Clear();
            radSiteNone.Checked = true;
        }



        private void searchSiteList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                if (sender == cmbSitePopulation && !radSiteSortPopulation.Checked) radSiteSortPopulation.Checked = true;
                else
                {
                    siteSearch.name = txtSiteSearch.Text;
                    siteSearch.type = cmbSiteType.SelectedItem.ToString();
                    siteSearch.PopulationType = cmbSitePopulation.SelectedItem.ToString();
                    siteSearch.sortOwners = radSiteOwners.Checked;
                    siteSearch.sortEvents = radSiteSortEvents.Checked;
                    siteSearch.sortFiltered = radSiteSortFiltered.Checked;
                    siteSearch.sortWarfare = radSiteSortWarfare.Checked;
                    siteSearch.SortPopulation = radSiteSortPopulation.Checked;
                    siteSearch.SortConnections = radSortConnections.Checked;
                    siteSearch.SortDeaths = radSiteSortDeaths.Checked;
                    siteSearch.SortBeastAttacks = radSiteBeastAttacks.Checked;
                    IEnumerable<Site> list = siteSearch.getList();
                    listSiteSearch.Items.Clear();
                    listSiteSearch.Items.AddRange(list.ToArray());
                }
            }
        }

        public void ChangeSiteBaseList(List<Site> list, string listName)
        {
            FileLoader.Working = true;
            lblSiteList.Text = listName;
            lblSiteList.ForeColor = Color.Blue;
            lblSiteList.Font = new Font(lblSiteList.Font.FontFamily, lblSiteList.Font.Size, FontStyle.Bold);
            siteSearch.BaseList = list;
            txtSiteSearch.Clear();
            cmbSiteType.SelectedIndex = 0;
            cmbSitePopulation.SelectedIndex = 0;
            radSiteNone.Checked = true;
            //tcWorld.SelectedTab = tpSites;
            tcSites.SelectedTab = tpSiteSearch;
            searchSiteList(null, null);
            FileLoader.Working = false;
        }

        public void ResetSiteBaseList(object sender, EventArgs e)
        {
            if (!FileLoader.Working && World != null)
            {
                lblSiteList.Text = "All";
                lblSiteList.ForeColor = Control.DefaultForeColor;
                lblSiteList.Font = new Font(lblSiteList.Font.FontFamily, lblSiteList.Font.Size, FontStyle.Regular);
                siteSearch.BaseList = World.Sites;
                searchSiteList(null, null);
            }
        }

        private void listSiteSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            listSearch_SelectedIndexChanged(sender, e);
        }
    }
}
