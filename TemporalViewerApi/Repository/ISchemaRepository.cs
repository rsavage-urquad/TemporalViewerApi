using TemporalViewerApi.Models;

namespace TemporalViewerApi.Repository
{
    public interface ISchemaRepository
    {
        Schema GetTemporalTables();
        TemporalTable GetTemporalTableByName(string schemsName, string columnName);
        Schema GetPrimaryKeys(string schema, string table);
        Schema GetTableColumns(int tableObjectId);
        Schema GetTableColumns(string schemsName, string columnName);

    }
}
