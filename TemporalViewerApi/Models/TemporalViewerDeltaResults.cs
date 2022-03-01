using System;
using System.Collections.Generic;
using System.Linq;

namespace TemporalViewerApi.Models
{
    /// <summary>
    /// TemporalViewerDeltaResults Class - Results from Temporal Viewer Api "GetDelta" method
    /// </summary>
    public class TemporalViewerDeltaResults
    {
        public string BaseSchemaName { get; set; }
        public string BaseTableName { get; set; }
        public string HistorySchemaName { get; set; }
        public string HistoryTableName { get; set; }
        public List<TableColumn> TableColumns { get; set; }
        public List<DeltaInfo> Delta { get; set; }
        public List<string> Messages { get; set; }
        public bool isValid { get { return (Messages.Count == 0); } }

        public TemporalViewerDeltaResults()
        {
            BaseSchemaName = "";
            BaseTableName = "";
            HistorySchemaName = "";
            HistoryTableName = "";
            TableColumns = new List<TableColumn>();
            Delta = new List<DeltaInfo>();
            Messages = new List<string>();
        }

        public TemporalViewerDeltaResults(TemporalViewerResults results)
        {
            BaseSchemaName = results.BaseSchemaName;
            BaseTableName = results.BaseTableName;
            HistorySchemaName = results.HistorySchemaName;
            HistoryTableName = results.HistoryTableName;
            TableColumns = results.TableColumns;
            Delta = PopulateDelta(results);
            Messages = results.Messages;
        }

        /// <summary>
        /// PopulateDelta() - Populated the Changed Items
        /// </summary>
        /// <param name="results">Results to act upon</param>
        /// <returns>List of Delta Info items</returns>
        protected List<DeltaInfo> PopulateDelta(TemporalViewerResults results)
        {
            List<DeltaInfo> deltaInfo = new List<DeltaInfo>();
            dynamic newInfo;
            dynamic oldInfo;
            CompareDictionary dict = new CompareDictionary();

            // Iterate over the History Rows to compare the row to the prior state (as rows will be in time descending order)
            for (int i = 0; i < (results.HistoryInfo.Count - 1); i++)
            {
                newInfo = results.HistoryInfo[i];
                oldInfo = results.HistoryInfo[i + 1];

                dict.PopulateCompareDictionary(newInfo, true);
                dict.PopulateCompareDictionary(oldInfo, false);
                DeltaInfo rowDelta = new DeltaInfo();

                // Iterate over the columns to identify changes.
                foreach (var col in results.TableColumns)
                {
                    switch (col.GeneratedType)
                    {
                        case 0:
                            if (dict.CompareDict[col.ColumnName].NewValue != dict.CompareDict[col.ColumnName].OldValue)
                            {
                                DeltaColumn deltaCol = new DeltaColumn(col.ColumnName, dict.CompareDict[col.ColumnName].OldValue, dict.CompareDict[col.ColumnName].NewValue);
                                rowDelta.ChangedColumns.Add(deltaCol);
                            }
                            break;
                        case 1:
                            rowDelta.StartDate = dict.CompareDict[col.ColumnName].NewValue;
                            break;
                        case 2:
                            rowDelta.EndDate = dict.CompareDict[col.ColumnName].NewValue;
                            break;
                    }
                }

                // If a change was identified, add the row to the results
                if (rowDelta.ChangedColumns.Count > 0)
                {
                    deltaInfo.Add(rowDelta);
                }
            }

            // TODO: (Question) Do we want to include initial state?

            return deltaInfo;
        }
    }
}
