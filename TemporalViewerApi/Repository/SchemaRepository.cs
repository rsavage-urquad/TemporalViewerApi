using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using TemporalViewerApi.Models;
using Dapper;

namespace TemporalViewerApi.Repository
{
    /// <summary>
    /// SchemaRepository - Repository Class for accessing DB Schema information
    /// </summary>
    public class SchemaRepository : ISchemaRepository
    {
        private readonly IConfiguration _configuration;
        public SchemaRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// GetTemporalTables() - Retireve list of all Temporal tables in the DB
        /// </summary>
        /// <returns>Schema object with the List of TemporalTable objects populated</returns>
        public Schema GetTemporalTables()
        {
            Schema schema = new Schema();
            string sql = @"SELECT
s.name as BaseSchemaName,
t.name as BaseTableName,
t.object_id as BaseTableObjectId,
hs.name as HistorySchemaName,
ht.name as HistoryTableName,
t.history_table_id as HistoryTableObjectId
FROM sys.tables t with(nolock)
INNER JOIN sys.schemas s with(nolock) on t.schema_id = s.schema_id
INNER JOIN sys.tables ht with(nolock) on t.history_table_id = ht.object_id
INNER JOIN sys.schemas hs with(nolock) on ht.schema_id = hs.schema_id
WHERE t.temporal_type = 2
AND t.type = 'U';";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DbConnStr")))
            {
                schema.TemporalTables = db.Query<TemporalTable>(sql).ToList();
            }

            return schema;
        }

        /// <summary>
        /// GetTemporalTableByName() - Gets Temporal Table information for a given Temporal Base table
        /// </summary>
        /// <param name="schemaName">Schema Name</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Populated TemporalTable object (null if not found)</returns>
        public TemporalTable GetTemporalTableByName(string schemaName, string tableName)
        {
            TemporalTable tableInfo = new TemporalTable();
            string sql = @"SELECT
s.name as BaseSchemaName,
t.name as BaseTableName,
t.object_id as BaseTableObjectId,
hs.name as HistorySchemaName,
ht.name as HistoryTableName,
t.history_table_id as HistoryTableObjectId
FROM sys.tables t with(nolock)
INNER JOIN sys.schemas s with(nolock) on t.schema_id = s.schema_id
INNER JOIN sys.tables ht with(nolock) on t.history_table_id = ht.object_id
INNER JOIN sys.schemas hs with(nolock) on ht.schema_id = hs.schema_id
WHERE t.temporal_type = 2
AND t.type = 'U'
AND s.name = @SchemaName
AND t.name = @TableName;";

            var parameters = new DynamicParameters();
            parameters.Add("SchemaName", schemaName);
            parameters.Add("TableName", tableName);

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DbConnStr")))
            {
                tableInfo = db.QueryFirstOrDefault<TemporalTable>(sql, parameters);
            }

            return tableInfo;
        }

        /// <summary>
        /// GetPrimaryKeys() - Retrieves Primary Key column info for a Table
        /// </summary>
        /// <param name="schemaName">Schema Name</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Schema object with the List of PrimaryKeyColumn objects populated</returns>
        public Schema GetPrimaryKeys(string schemaName, string tableName)
        {
            Schema schema = new Schema();
            string sql = @"SELECT 
cc.COLUMN_NAME as ColumnName,
ty.name as ColumnType
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cc  ON tc.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
INNER JOIN sys.schemas s with(nolock) on tc.TABLE_SCHEMA = s.name
INNER JOIN sys.tables t with(nolock) on 
	s.schema_id = t.schema_id AND
	tc.TABLE_NAME = t.name
INNER JOIN sys.columns c with(nolock) on
	t.object_id = c.object_id AND
	cc.COLUMN_NAME = c.name
INNER JOIN sys.types ty with(nolock) on c.user_type_id = ty.user_type_id
WHERE cc.TABLE_NAME = @TableName  
AND cc.TABLE_SCHEMA = @SchemaName 
AND tc.CONSTRAINT_TYPE='PRIMARY KEY';";

            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DbConnStr")))
            {
                schema.PrimaryKeys = db.Query<PrimaryKeyColumn>(sql, new { SchemaName = schemaName, TableName = tableName }).ToList();
            }

            return schema;
        }

        /// <summary>
        /// GetTableColumns() - Retrieves a list of columns for a Table by the table's Object Id.
        /// </summary>
        /// <param name="tableObjectId">Table's Object Id</param>
        /// <returns>Schema object with the List of TableColumn objects populated</returns>
        public Schema GetTableColumns(int tableObjectId)
        {
            string filterLogic = "WHERE c.object_id = @TableObjectId";

            var parameters = new DynamicParameters();
            parameters.Add("TableObjectId", tableObjectId);

            return RetrieveTableColumns(filterLogic, parameters);
        }

        /// <summary>
        /// GetTableColumns() - Retrieves a list of columns for a Table by the table's Schema and Table names.
        /// </summary>
        /// <param name="schemaName">Schema Name</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Schema object with the List of TableColumn objects populated</returns>
        public Schema GetTableColumns(string schemaName, string tableName)
        {
            string filterLogic = @"INNER JOIN sys.tables t with(nolock) on c.object_id = t.object_id
INNER JOIN sys.schemas s with(nolock) on t.schema_id = s.schema_id
WHERE s.name = @SchemaName
AND t.name = @TableName";

            var parameters = new DynamicParameters();
            parameters.Add("SchemaName", schemaName);
            parameters.Add("TableName", tableName);

            return RetrieveTableColumns(filterLogic, parameters);
        }

        /// <summary>
        /// RetrieveTableColumns() - Retrieve's column information for a DB Table
        /// </summary>
        /// <param name="filterLogic">Filter Logic (appropriate WHERE Clause)</param>
        /// <param name="parameters">Parameters for query</param>
        /// <returns>Schema object with the List of TableColumn objects populated</returns>
        protected Schema RetrieveTableColumns(string filterLogic, DynamicParameters parameters)
        {
            Schema schema = new Schema();
            string sql = @"SELECT 
c.name as ColumnName,
c.column_id as ColumnId,
ty.name as ColumnTypeName,
c.max_length as MaxLen,
c.precision as Precision,
c.scale as Scale,
c.is_nullable as IsNullable,
c.is_identity as IsIdentity,
c.generated_always_type as GeneratedType
FROM sys.columns c with(nolock)
INNER JOIN sys.types ty with(nolock) on c.user_type_id = ty.user_type_id
{{Filter}}
ORDER BY c.column_id;";

            sql = sql.Replace("{{Filter}}", filterLogic);
            using (IDbConnection db = new SqlConnection(_configuration.GetConnectionString("DbConnStr")))
            {
                schema.TableColumns = db.Query<TableColumn>(sql, parameters).ToList();
            }
            return schema;
        }

    }
}
