using System.Text;
using LegendsViewer.Legends;

namespace LegendsViewer.Controls
{
    public class WorldConstructionPrinter : HTMLPrinter
    {
        WorldConstruction _worldConstruction;

        public WorldConstructionPrinter(WorldConstruction worldConstruction)
        {
            _worldConstruction = worldConstruction;
        }

        public override string Print()
        {
            HTML = new StringBuilder();
            HTML.AppendLine("<h1>" + _worldConstruction.Name + "</h1><br />");

            PrintEventLog(_worldConstruction.Events, WorldConstruction.Filters, _worldConstruction);
            return HTML.ToString();
        }

        public override string GetTitle()
        {
            return _worldConstruction.Name;
        }
    }
}
