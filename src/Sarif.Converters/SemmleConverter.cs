﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts a log file from the Semmle format to the SARIF format.
    /// </summary>
    internal class SemmleConverter : ToolFileConverterBase
    {
        // Semmle logs are CSV files.
        private static readonly string[] s_delimiters = new[] { "," };

        // The fields are as follows:
        private enum FieldIndex
        {
            QueryName,
            QueryDescription,
            Severity,
            Message,
            RelativePath,
            Path,
            StartLine,
            StartColumn,
            EndLine,
            EndColumn
        }

        private TextFieldParser _parser;
        private List<Notification> _toolNotifications;

        /// <summary>
        /// Converts a Semmle log file in CSV format to a SARIF log file.
        /// </summary>
        /// <param name="input">
        /// Input stream from which to read the Semmle log.
        /// </param>
        /// <param name="output">
        /// Output string to which to write the SARIF log.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are null.
        /// </exception>
        public override void Convert(Stream input, IResultLogWriter output)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            _toolNotifications = new List<Notification>();

            var results = GetResultsFromStream(input);

            var tool = new Tool
            {
                Name = "Semmle"
            };

            output.Initialize(id: null, correlationId: null);

            output.WriteTool(tool);

            output.OpenResults();
            output.WriteResults(results);
            output.CloseResults();

            if (_toolNotifications.Any())
            {
                output.WriteToolNotifications(_toolNotifications);
            }
        }

        private Result[] GetResultsFromStream(Stream input)
        {
            var results = new List<Result>();
            using (_parser = new TextFieldParser(input)
            {
                TextFieldType = FieldType.Delimited,
                Delimiters = s_delimiters
            })
            {
                while (!_parser.EndOfData)
                {
                    results.Add(ParseResult());
                }
            }

            return results.ToArray();
        }

        private Result ParseResult()
        {
            string[] fields = _parser.ReadFields();

            Region region = MakeRegion(fields);
            var result = new Result
            {
                Message = fields[(int)FieldIndex.Message],
                Locations = new Location[]
                {
                    new Location
                    {
                        ResultFile = new PhysicalLocation
                        {
                            Uri = new Uri(GetString(fields, FieldIndex.RelativePath), UriKind.Relative),
                            Region = region
                        }
                    }
                }
            };

            ResultLevel level = ResultLevelFromSemmleSeverity(GetString(fields, FieldIndex.Severity));
            if (level != ResultLevel.Warning)
            {
                result.Level = level;
            }

            return result;
        }

        /// <summary>
        /// Create a Region object that contains only those properties required by the
        /// SARIF spec.
        /// </summary>
        /// <param name="fields">
        /// Array of fields from a CSV record.
        /// </param>
        /// <returns>
        /// A Region object that contains only those properties required by the SARIF spec.
        /// </returns>
        private Region MakeRegion(string[] fields)
        {
            Region region = new Region
            {
                StartLine = GetInteger(fields, FieldIndex.StartLine),
                StartColumn = GetInteger(fields, FieldIndex.StartColumn),
            };

            int endLine = GetInteger(fields, FieldIndex.EndLine);
            int endColumn = GetInteger(fields, FieldIndex.EndColumn);
            if (endLine != region.StartLine)
            {
                region.EndLine = endLine;
                region.EndColumn = endColumn;
            }
            else
            {
                if (endColumn != region.StartColumn)
                {
                    region.EndColumn = endColumn;
                }
            }

            return region;
        }

        private static string GetString(string[] fields, FieldIndex fieldIndex)
        {
            return fields[(int)fieldIndex];
        }

        private int GetInteger(string[] fields, FieldIndex fieldIndex)
        {
            string field = GetString(fields, fieldIndex);
            int value;
            if (!int.TryParse(field, out value))
            {
                value = 0;
                AddToolNotification(
                    "InvalidInteger",
                    NotificationLevel.Error,
                    ConverterResources.SemmleInvalidInteger,
                    field,
                    fieldIndex);
            }

            return value;
        }

        private ResultLevel ResultLevelFromSemmleSeverity(string semmleSeverity)
        {
            switch (semmleSeverity)
            {
                case "error":
                    return ResultLevel.Error;

                case "warning":
                    return ResultLevel.Warning;

                case "recommendation":
                    return ResultLevel.Note;

                default:
                    AddToolNotification(
                        "UnknownSeverity",
                        NotificationLevel.Error,
                        ConverterResources.SemmleUnknownSeverity,
                        semmleSeverity);
                    return ResultLevel.Warning;
            }
        }

        private void AddToolNotification(
            string id,
            NotificationLevel level,
            string messageFormat,
            params object[] args)
        {
            string message = string.Format(CultureInfo.CurrentCulture, messageFormat, args);

            // When the parser read the offending line, it incremented the line number,
            // so report the previous line.
            long lineNumber = _parser.LineNumber - 1;
            string messageWithLineNumber = string.Format(
                CultureInfo.CurrentCulture,
                ConverterResources.SemmleNotificationFormat,
                lineNumber,
                message);

            _toolNotifications.Add(new Notification
            {
                Id = id,
                Time = DateTime.Now,
                Level = level,
                Message = messageWithLineNumber
            });
        }
    }
}