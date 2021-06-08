﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Extracts the text from a PDF document
    /// </summary>
    public class PdfTextService
    {
        private readonly IPdfTextExtractor _pdfTextExtractor;
        private readonly IMediaFileSystem _mediaFileSystem;
        private readonly ILogger _logger;

        [Obsolete]
        public PdfTextService(
            IPdfTextExtractor pdfTextExtractor,
            IMediaFileSystem mediaFileSystem)
        {
            _pdfTextExtractor = pdfTextExtractor;
            _mediaFileSystem = mediaFileSystem;
            _logger = Current.Logger;
        }

        public PdfTextService(
            IPdfTextExtractor pdfTextExtractor,
            IMediaFileSystem mediaFileSystem,
            ILogger logger)
        {
            _pdfTextExtractor = pdfTextExtractor;
            _mediaFileSystem = mediaFileSystem;
            _logger = logger;
        }

        /// <summary>
        /// Extract text from a PDF file at the given path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string ExtractText(string filePath)
        {
            using (var fs = _mediaFileSystem.OpenFile(filePath))
            {
                if (fs != null)
                {
                    return ExceptChars(_pdfTextExtractor.GetTextFromPdf(fs), UnsupportedRange.Value, ReplaceWithSpace);
                }
                else
                {
                    _logger.Error(this.GetType(), new Exception($"Unable to open PDF file {filePath}"));
                    return null;
                }
            }
        }

        /// <summary>
        /// Stores the unsupported range of character
        /// </summary>
        /// <remarks>
        /// used as a reference:
        /// http://www.tamasoft.co.jp/en/general-info/unicode.html
        /// http://en.wikipedia.org/wiki/Summary_of_Unicode_character_assignments
        /// http://en.wikipedia.org/wiki/Unicode
        /// http://en.wikipedia.org/wiki/Basic_Multilingual_Plane
        /// </remarks>
        private static readonly Lazy<HashSet<char>> UnsupportedRange = new Lazy<HashSet<char>>(() =>
        {
            var unsupportedRange = new HashSet<char>();
            // Create collection with chars to skip
            foreach (var c in Enumerable.Range(0x0000, 0x001F))
            {
                unsupportedRange.Add((char)c);
            }
            unsupportedRange.Add((char)0x1F);

            //Allow tabs
            unsupportedRange.Remove((char)0x9);

            // Remove replace chars from collection
            foreach (var c in ReplaceWithSpace)
            {
                unsupportedRange.Remove(c);
            }
            return unsupportedRange;
        });

        private static readonly HashSet<char> ReplaceWithSpace = new HashSet<char>(Environment.NewLine);

        /// <summary>
        /// Remove all toExclude chars from string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="toExclude"></param>
        /// <param name="replaceWithSpace"></param>
        /// <returns></returns>
        private static string ExceptChars(string str, HashSet<char> toExclude, HashSet<char> replaceWithSpace)
        {
            var sb = new StringBuilder(str.Length);
            foreach (var c in str)
            {
                if (toExclude.Contains(c) == false)
                {
                    if (replaceWithSpace.Contains(c))
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
