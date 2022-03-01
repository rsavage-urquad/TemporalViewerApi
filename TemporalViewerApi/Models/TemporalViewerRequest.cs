using System.Collections.Generic;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalViewerRequest -Class to represent input for processing
    /// </summary>
    public class TemporalViewerRequest
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public List<PrimaryKeyColumnInput> LookupInfo { get; set; }

        /// <summary>
        /// TemporalViewerRequest() - Default Constructor
        /// </summary>
        public TemporalViewerRequest()
        {
            SchemaName = "";
            TableName = "";
            LookupInfo = new List<PrimaryKeyColumnInput>();
        }
    }
}
