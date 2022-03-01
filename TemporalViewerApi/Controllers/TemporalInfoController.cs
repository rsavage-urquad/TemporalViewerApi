using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TemporalViewerApi.Models;
using TemporalViewerApi.Repository;

namespace TemporalViewerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TemporalInfoController : Controller
    {

        private readonly ILogger<TemporalInfoController> _logger;
        private readonly ISchemaRepository _repo;
        private readonly ITemporalViewerProcessor _procesor;

        public TemporalInfoController(ILogger<TemporalInfoController> logger, ISchemaRepository repo, ITemporalViewerProcessor processor)
        {
            _logger = logger;
            _repo = repo;
            _procesor = processor;
        }

        /// <summary>
        /// GetData() - Get Temporal information for a given Primary Key in Temporal base and history tables
        /// </summary>
        /// <param name="input">Input format that includes Schema, Table and Primary Key value(s)</param>
        /// <returns>Results object (TemporalViewerResults)</returns>
        [HttpPost]
        [Route("GetData")]
        public ActionResult<TemporalViewerResults> GetData(TemporalViewerRequest input)
        {
            TemporalViewerResults resp = _procesor.Process(input);
            if (!resp.isValid)
            {
                return BadRequest(resp);
            }

            return Ok(resp);
        }

        /// <summary>
        /// GetDelta() - Gets changed column and old/new values for a given Primary Key in Temporal base and history tables
        /// </summary>
        /// <param name="input">Input format that includes Schema, Table and Primary Key value(s)</param>
        /// <returns>Results object (TemporalViewerDeltaResults)</returns>
        [HttpPost]
        [Route("GetDelta")]
        public ActionResult<TemporalViewerDeltaResults> GetDelta(TemporalViewerRequest input)
        {
            TemporalViewerResults resp = _procesor.Process(input);
            TemporalViewerDeltaResults deltaResp = new TemporalViewerDeltaResults(resp);
            if (!deltaResp.isValid)
            {
                return BadRequest(deltaResp);
            }
           
            return Ok(deltaResp);
        }

        /// <summary>
        /// GetColumnHistory() - Gets old/new values for a specific column for a given Primary Key in Temporal base 
        /// and history tables
        /// </summary>
        /// <param name="input">Input format that includes Schema, Table and Primary Key value(s)</param>
        /// <param name="columnName">Column Name to retrieve</param>
        /// <returns>Results object (TemporalViewerColumnHistoryResults)</returns>
        [HttpPost]
        [Route("GetColumnHistory")]
        public ActionResult<TemporalViewerColumnHistoryResults> GetColumnHistory(TemporalViewerRequest input, string columnName)
        {
            TemporalViewerResults resp = _procesor.Process(input);
            TemporalViewerColumnHistoryResults colhistResp = new TemporalViewerColumnHistoryResults(resp, columnName);
            if (!colhistResp.isValid)
            {
                return BadRequest(colhistResp);
            }

            return Ok(colhistResp);
        }

    }
}
