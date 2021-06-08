﻿using Examine;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Examine;
using System;
using System.Linq;

namespace UmbracoExamine.PDF
{
    public interface IPdfIndexValueSetBuilder : IValueSetBuilder<IMedia> { }

    /// <summary>
    /// Builds a ValueSet for PDF Documents
    /// </summary>
    public class PdfIndexValueSetBuilder : IPdfIndexValueSetBuilder
    {
        private PdfTextService _pdfTextService;
        private readonly ILogger _logger;

        public PdfIndexValueSetBuilder(PdfTextService pdfTextService, ILogger logger)
        {
            _pdfTextService = pdfTextService;
            _logger = logger;
        }
        public IEnumerable<ValueSet> GetValueSets(params IMedia[] content)
        {
            foreach (var item in content)
            {
                var umbracoFile = item.GetValue<string>(Constants.Conventions.Media.File);
                if (string.IsNullOrWhiteSpace(umbracoFile)) continue;

                string fileTextContent;
                try
                {
                    fileTextContent = ExtractTextFromFile(umbracoFile);
                }
                catch (Exception ex)
                {
                    _logger.Error<PdfIndexValueSetBuilder>(ex, "Could not read the file {MediaFile}", umbracoFile);
                    continue;
                }
                var indexValues = new Dictionary<string, object>
                {
                    ["nodeName"] = item.Name,
                    ["id"] = item.Id,
                    ["path"] =  item.Path,
                    [PdfIndexConstants.PdfContentFieldName] = fileTextContent
                };

                var valueSet = new ValueSet(item.Id.ToString(), PdfIndexConstants.PdfCategory, item.ContentType.Alias, indexValues);

                yield return valueSet;
            }
        }

        private string ExtractTextFromFile(string filePath)
        {
            try
            {
                return _pdfTextService.ExtractText(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error<PdfIndexValueSetBuilder>(ex, "Could not extract text from PDF {PdfFilePath}", filePath);
                return string.Empty;
            }
        }
    }
}
