/*
 * PxApi
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 2.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using PxWeb.Attributes.Api2;
using PxWeb.Api2.Server.Models;
using PCAxis.Paxiom;
using Px.Abstractions.Interfaces;
using PxWeb.Helper.Api2;
using PxWeb.Mappers;
using Px.Search;
using System.Linq;
using Lucene.Net.Util;
using PxWeb.Code.Api2.Serialization;
using PCAxis.Serializers;

namespace PxWeb.Controllers.Api2
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class TableApiController : PxWeb.Api2.Server.Controllers.TableApiController
    {
        private readonly IDataSource _dataSource;
        private readonly ILanguageHelper _languageHelper;
        private readonly IResponseMapper _responseMapper;
        private readonly ISearchBackend _backend;
        private readonly ISerializeManager _serializeManager;

        public TableApiController(IDataSource dataSource, ILanguageHelper languageHelper, IResponseMapper responseMapper, ISearchBackend backend, ISerializeManager serializeManager)
        {
            _dataSource = dataSource;
            _languageHelper = languageHelper;
            _responseMapper = responseMapper;
            _backend = backend;
            _serializeManager = serializeManager;
        }


        public override IActionResult GetMetadataById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }


        public override IActionResult GetTableById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            lang = _languageHelper.HandleLanguage(lang);
            IPXModelBuilder? builder = _dataSource.CreateBuilder(id, lang);

            if (builder != null)
            {
                try
                {
                    builder.BuildForSelection();
                    var model = builder.Model;

                    Table t = new Table();
                    t.Id = id;
                    t.Label = model.Meta.Title;
                    return new ObjectResult(t);
                }
                catch (Exception)
                {
                    return NotFound();
                }
            }
            else
            {
                return new BadRequestObjectResult("No such table id " + id);
            }
        }

        public override IActionResult GetTableCodeListById([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get table data
        /// HttpGet
        /// Route /api/v2/tables/{id}/data
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="lang">The language if the default is not what you want.</param>
        /// <param name="valuecodes"></param>
        /// <param name="codelist"></param>
        /// <param name="outputvalues"></param>
        /// <response code="200">Success</response>
        /// <response code="400">Error respsone for 400</response>
        /// <response code="403">Error respsone for 403</response>
        /// <response code="404">Error respsone for 404</response>
        /// <response code="429">Error respsone for 429</response>
        public override IActionResult GetTableData([FromRoute(Name = "id"), Required] string id, [FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "valuecodes")] Dictionary<string, List<string>>? valuecodes, [FromQuery(Name = "codelist")] Dictionary<string, string>? codelist, [FromQuery(Name = "outputvalues")] Dictionary<string, CodeListOutputValuesStyle>? outputvalues)
        {
            //TODO check that no selection paramaters is given
            lang = _languageHelper.HandleLanguage(lang);
            PXModel model;
            //if no parameters given
            var builder = _dataSource.CreateBuilder(id, lang);
            if (builder == null)
            {
                throw new Exception("Missing datasource");
            }

            builder.BuildForSelection();
            var selection = GetDefaultTable(builder.Model);

            builder.BuildForPresentation(selection);
            model = builder.Model;
            //else
            //    TODO create model from selection
            //    selection = GetSelectionFromQuery(...)
            
            //serialize output
            //TODO check if given in url param otherwise take the format from appsettings
            string outputFormat = "px";
            var serializer = _serializeManager.GetSerializer(outputFormat);
            serializer.Serialize(model, Response);

            return Ok();
           
        }
        
        private Selection[] GetDefaultTable(PXModel model)
        {
            //TODO implement the correct algorithm

            var selections = new List<Selection>();

            foreach (var variable in model.Meta.Variables)
            {
                var selection = new Selection(variable.Code);
                //Takes the first 4 values for each variable if variable has less values it takes all of its values.
                var codes = variable.Values.Take(4).Select(value => value.Code).ToArray();
                selection.ValueCodes.AddRange(codes);
                selections.Add(selection);
            }

            return selections.ToArray();
        }

        public override IActionResult ListAllTables([FromQuery(Name = "lang")] string? lang, [FromQuery(Name = "query")] string? query, [FromQuery(Name = "pastDays")] int? pastDays, [FromQuery(Name = "includeDiscontinued")] bool? includeDiscontinued, [FromQuery(Name = "pageNumber")] int? pageNumber, [FromQuery(Name = "pageSize")] int? pageSize)
        {
            Searcher searcher = new Searcher(_dataSource, _backend);

            lang = _languageHelper.HandleLanguage(lang);
            //TODO: H�mta default v�rden f�r pageSize fr�n config
            if (pageNumber == null || pageNumber <= 0)
                pageNumber = 1;

            if (pageSize == null || pageSize <= 0)
                pageSize = 20;

            if (query != null)
                return Ok(searcher.Find(query, lang, pageSize.Value, pageNumber.Value));

            return Ok();
        }
    }

}
