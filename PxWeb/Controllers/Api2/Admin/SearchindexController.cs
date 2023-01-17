﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Px.Abstractions.Interfaces;
using Px.Search;
using PxWeb.Api2.Server.Models;
using PxWeb.Config.Api2;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PxWeb.Controllers.Api2.Admin
{
    [ApiController]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class SearchindexController : ControllerBase
    {
        private readonly IDataSource _dataSource;
        private readonly ISearchBackend _backend;
        private readonly IPxApiConfigurationService _pxApiConfigurationService;

        public SearchindexController(IDataSource dataSource, ISearchBackend backend, IPxApiConfigurationService pxApiConfigurationService)
        {
            _dataSource = dataSource;
            _backend = backend; 
            _pxApiConfigurationService = pxApiConfigurationService; 
        }

        /// <summary>
        /// Index the whole database in all languages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/v2/admin/searchindex")]
        [SwaggerOperation("IndexDatabase")]
        [SwaggerResponse(statusCode: 200, description: "Success")]
        [SwaggerResponse(statusCode: 401, description: "Unauthorized")]
        public IActionResult IndexDatabase()
        {
            List<string> languages = new List<string>();

            var config = _pxApiConfigurationService.GetConfiguration();

            if (config.Languages.Count == 0)
            {
                throw new System.Exception("No languages configured for PxApi");
            }

            foreach (var lang in config.Languages)
            {
                languages.Add(lang.Id);
            }

            Indexer indexer = new Indexer(_dataSource, _backend);
            indexer.IndexDatabase(languages);

            return Ok();
        }

        /// <summary>
        /// Update index for the specified tables
        /// </summary>
        /// <param name="tables"></param>
        /// <returns>Comma separated list of table ids that should be updated in the index</returns>
        [HttpPatch]
        [Route("/api/v2/admin/searchindex")]
        [SwaggerOperation("IndexDatabase")]
        [SwaggerResponse(statusCode: 200, description: "Success")]
        [SwaggerResponse(statusCode: 401, description: "Unauthorized")]
        public IActionResult IndexDatabase([FromQuery(Name = "tables"), Required] string tables)
        {
            List<string> languages = new List<string>();
            List<string> tableList = tables.Split(',').ToList();

            if (tableList.Count == 0)
            {
                throw new System.Exception("No tables specified for index update");
            }

            var config = _pxApiConfigurationService.GetConfiguration();

            if (config.Languages.Count == 0)
            {
                throw new System.Exception("No languages configured for PxApi");
            }

            foreach (var lang in config.Languages)
            {
                languages.Add(lang.Id);
            }

            Indexer indexer = new Indexer(_dataSource, _backend);
            indexer.UpdateTableEntries(tableList, languages);

            return Ok();
        }

    }
}
