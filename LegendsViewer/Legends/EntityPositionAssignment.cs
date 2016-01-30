using LegendsViewer.Legends.Parser;
using System;
using System.Collections.Generic;

namespace LegendsViewer.Legends
{
    public class EntityPositionAssignment
    {
        public int ID { get; set; }
        public HistoricalFigure HistoricalFigure { get; set; }
        public int PositionID { get; set; }
        public int SquadID { get; set; }

        public EntityPositionAssignment(List<Property> properties, World world)
        {
            ID = -1;
            foreach (Property property in properties)
            {
                switch (property.Name)
                {
                    case "id": ID = property.ValueAsInt(); break;
                    case "histfig": HistoricalFigure = world.GetHistoricalFigure(property.ValueAsInt()); break;
                    case "position_id": PositionID = property.ValueAsInt(); break;
                    case "squad_id": SquadID = property.ValueAsInt(); break;
                }
            }

        }
    }
}
