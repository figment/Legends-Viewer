using System.Text;
using LegendsViewer.Legends;

namespace LegendsViewer.Controls
{

    class ArtifactPrinter : HTMLPrinter
    {
        Artifact Artifact;

        public ArtifactPrinter(Artifact artifact)
        {
            Artifact = artifact;
        }

        public override string Print()
        {
            HTML = new StringBuilder();
            if (!string.IsNullOrEmpty(Artifact.Item) && Artifact.Item != Artifact.Name)
                HTML.AppendLine("<h1>" + Artifact.Name + ", \"" + Artifact.Item + "\"</h1><br />");
            else
                HTML.AppendLine("<h1>" + Artifact.Name + "</h1><br />");

            if (!string.IsNullOrEmpty(Artifact.Type) && !string.IsNullOrEmpty(Artifact.Material))
                HTML.AppendFormat("<b>{0}</b> is a {1} made of {2}<br /><br />", Artifact.ToLink(false), Artifact.Type, Artifact.Material);
            else if (!string.IsNullOrEmpty(Artifact.Type))
                HTML.AppendFormat("<b>{0}</b> is a {1}<br /><br />", Artifact.ToLink(false), Artifact.Type);
            HTML.AppendFormat("<br/>");
            if (!string.IsNullOrEmpty(Artifact.Type)) HTML.AppendFormat("<b>Type: </b> {1}<br/>", Artifact.Type);
            if (!string.IsNullOrEmpty(Artifact.SubType)) HTML.AppendFormat("<b>SubType: </b> {1}<br/>", Artifact.SubType);
            if (!string.IsNullOrEmpty(Artifact.Material)) HTML.AppendFormat("<b>Material: </b> {1}<br/>", Artifact.Material);
            if (!string.IsNullOrEmpty(Artifact.Description))
            {
                HTML.AppendLine("<b>Description</b><br/>");
                HTML.AppendLine("<p>" + Formatting.InitCaps(Artifact.Description) + "</p><br />");
            }

            PrintEvents();
            return HTML.ToString();
        }

        public override string GetTitle()
        {
            return Artifact.Name;
        }

        private void PrintEvents()
        {
            HTML.AppendLine("<b>Event Log</b> " + LineBreak);
            foreach (WorldEvent eraEvent in Artifact.Events)
                if (!Artifact.Filters.Contains(eraEvent.Type))
                    HTML.AppendLine(eraEvent.Print(true, Artifact) + "</br></br>");
        }
    }
}
