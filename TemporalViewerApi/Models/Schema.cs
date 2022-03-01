using System.Collections.Generic;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// Schema - Class to contain DB Schema information
    /// </summary>
    public class Schema
    {
        public List<TemporalTable> TemporalTables { get; set; }
        public List<PrimaryKeyColumn> PrimaryKeys { get; set; }
        public List<TableColumn> TableColumns { get; set; }

        /// <summary>
        /// Schema() - Default constructor
        /// </summary>
        public Schema()
        {
            TemporalTables = new List<TemporalTable>();
            PrimaryKeys = new List<PrimaryKeyColumn>();
            TableColumns = new List<TableColumn>();
        }
    }
}
