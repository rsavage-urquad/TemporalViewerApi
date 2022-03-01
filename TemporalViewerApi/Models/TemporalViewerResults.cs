using System.Collections.Generic;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalViewerResults Class - Results from Temporal Viewer Api "GetData" method
    /// </summary>
    public class TemporalViewerResults
    {
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public string HistorySchemaName { get; set; }
        public string HistoryTableName { get; set; }
        public List<TableColumn> TableColumns { get; set; }
        public List<dynamic> HistoryInfo { get; set; }
        public List<List<bool>> DiffInds { get; set; }
        public List<string> Messages { get; set; }
        public bool isValid { get { return (Messages.Count == 0); } }

        /// <summary>
        /// TemporalViewerResults() - Default Constructor
        /// </summary>
        public TemporalViewerResults()
        {
            BaseSchemaName = "";
            BaseTableName = "";
            HistorySchemaName = "";
            HistoryTableName = "";
            TableColumns = new List<TableColumn>();
            HistoryInfo = new List<dynamic>();
            DiffInds = new List<List<bool>>();
            Messages = new List<string>();
        }
    }
}
