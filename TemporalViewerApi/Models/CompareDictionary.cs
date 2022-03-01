using System.Collections.Generic;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// CompareDictionary - Class to handle column comparisons
    /// </summary>
    public class CompareDictionary
    {
        public Dictionary<string, ColumnCompare> CompareDict { get; set; }

        /// <summary>
        /// CompareDictionary() - Default constructor.
        /// </summary>
        public CompareDictionary()
        {
            CompareDict = new Dictionary<string, ColumnCompare>();
        }

        /// <summary>
        /// PopulateCompareDictionary() - Populates the Compare Dictionary for the Old or New Column info
        /// </summary>
        /// <param name="rowIinfo">Row Info (dynamic Object)</param>
        /// <param name="isNew">New/Old indicator</param>
        public void PopulateCompareDictionary(dynamic rowIinfo, bool isNew)
        {
            foreach (var col in rowIinfo)
            {
                var x = col.Key;
                var y = col.Value;
                if (!CompareDict.ContainsKey(col.Key))
                {
                    CompareDict.Add(col.Key, new ColumnCompare());
                }

                if (isNew)
                {
                    CompareDict[col.Key].NewValue = col.Value;
                }
                else
                {
                    CompareDict[col.Key].OldValue = col.Value;
                }
            }
        }
    }
}
