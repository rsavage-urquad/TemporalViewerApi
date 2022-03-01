using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TemporalViewerApi.Models;
using TemporalViewerApi.Repository;

namespace TemporalViewerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SchemaController : Controller
    {

        private readonly ILogger<SchemaController> _logger;
        private readonly ISchemaRepository _repo;

        public SchemaController(ILogger<SchemaController> logger, ISchemaRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        /// <summary>
        /// GetTemporalTables() - Retrieve list of Temporal Tables (Base and History)
        /// </summary>
        /// <returns>Results in JSON format.</returns>
        [HttpGet]
        [Route("TemporalTables")]
        public ActionResult<List<TemporalTable>> GetTemporalTables()
        {
            Schema results = _repo.GetTemporalTables();
            return Ok(results.TemporalTables);
        }

        /// <summary>
        /// GetTemporalTableByName() - Get Temporal Table info (Base and History) by
        /// Base table schema and name.
        /// </summary>
        /// <param name="schema">Schema name</param>
        /// <param name="table">Table Name</param>
        /// <returns>Results in JSON format.</returns>
        [HttpGet]
        [Route("TemporalTableByName")]
        public ActionResult<TemporalTable> GetTemporalTableByName(string schema, string table)
        {
            TemporalTable results = _repo.GetTemporalTableByName(schema, table);
            return Ok(results);
        }

        /// <summary>
        /// GetPrimaryKeys() - Get list of Primary Key field info for requested table
        /// </summary>
        /// <param name="schema">Schema name</param>
        /// <param name="table">Table Name</param>
        /// <returns>Results in JSON format.</returns>
        [HttpGet]
        [Route("PrimaryKeys")]
        public ActionResult<List<PrimaryKeyColumn>> GetPrimaryKeys(string schema, string table)
        {
            Schema results = _repo.GetPrimaryKeys(schema, table);
            return Ok(results.PrimaryKeys);
        }

        /// <summary>
        /// TableColumnsById() - Get list of Column info for a requested Table Object Id
        /// </summary>
        /// <param name="tableObjectId">Table Object Id</param>
        /// <returns>Results in JSON format.</returns>
        [HttpGet]
        [Route("TableColumnsById")]
        public ActionResult<List<PrimaryKeyColumn>> GetTableColumnsById(int tableObjectId)
        {
            Schema results = _repo.GetTableColumns(tableObjectId);
            return Ok(results.TableColumns);
        }

        /// <summary>
        /// GetTableColumnsByName() - Get list of Column info for a requested Table schema and name.
        /// </summary>
        /// <param name="schema">Schema name</param>
        /// <param name="table">Table Name</param>
        /// <returns></returns>
        [HttpGet]
        [Route("TableColumnsByName")]
        public ActionResult<List<PrimaryKeyColumn>> GetTableColumnsByName(string schema, string table)
        {
            Schema results = _repo.GetTableColumns(schema, table);
            return Ok(results.TableColumns);
        }
    }
}
