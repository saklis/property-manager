using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PropertyManager.PropertyProvider.Interface;

namespace PropertyManager.PropertyProvider {
    /// <summary>
    ///     Parse file and build list of property entries.
    /// </summary>
    public class FileEditablePropertyProvider : IEditablePropertyProvider {
        /// <summary>
        ///     All lines read from the file.
        /// </summary>
        private string[] _allLines;

        /// <summary>
        ///     Create new instance of class.
        /// </summary>
        /// <param name="filePath">File with values.</param>
        /// <param name="encoding">
        ///     Encoding of file. Default to null. If null is provided, UTF8 will be used.
        /// </param>
        /// <exception cref="FileNotFoundException">
        ///     Thrown when file does not exists or is not accessible.
        /// </exception>
        public FileEditablePropertyProvider(string filePath) {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found or inaccessible.", filePath);

            FilePath = filePath;
        }

        /// <summary>
        ///     Character uses in the file as the marker for comments. Must be first non-whitespace
        ///     character to be recognized.
        /// </summary>
        public string CommentSign { get; set; } = "#";

        /// <summary>
        ///     File's encoding used while reading file.
        /// </summary>
        private Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     Path to file.
        /// </summary>
        private string FilePath { get; }

        /// <summary>
        ///     Get list of property entries.
        /// </summary>
        /// <returns>List of property entries.</returns>
        public List<PropertyEntry> GetPropertyEntries() {
            _allLines = File.ReadAllLines(FilePath, Encoding);

            List<PropertyEntry> lines = new List<PropertyEntry>(_allLines.Length);
            foreach (string line in _allLines) {
                string       trimmedLine = line.Trim();
                PropertyLine newLine     = new PropertyLine();

                if (trimmedLine.StartsWith(CommentSign) || string.IsNullOrWhiteSpace(trimmedLine)) {
                    newLine.IsCommentOrEmpty = true;
                    newLine.Source           = trimmedLine;
                } else {
                    newLine.Source = trimmedLine;

                    // separate key and value
                    string[] strings    = trimmedLine.Split(new[] {'='}, 2);
                    string   properties = strings[0].Trim();
                    string   value      = strings[1].Trim();

                    // parse property name and arguments
                    string[] arguments = properties.Split(' ');
                    if (arguments.Length > 1) {
                        if (arguments.Contains("static")) newLine.IsStatic = true;
                        if (arguments.Contains("field")) newLine.IsField   = true;
                    }

                    newLine.Property = arguments[arguments.Length - 1];

                    // parse value and type
                    if (int.TryParse(value, out int intValue)) {
                        newLine.PropertyType = typeof(int);
                        newLine.Value        = intValue;
                    } else if (float.TryParse(value, out float floatValue)) {
                        newLine.PropertyType = typeof(float);
                        newLine.Value        = floatValue;
                    } else if (bool.TryParse(value, out bool boolValue)) {
                        newLine.PropertyType = typeof(bool);
                        newLine.Value        = boolValue;
                    } else {
                        newLine.PropertyType = typeof(string);
                        newLine.Value        = value.Replace("\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    }
                }

                lines.Add(newLine);
            }

            return lines;
        }

        /// <summary>
        ///     Save lines to source.
        /// </summary>
        /// <param name="lines">Lines to save.</param>
        public void Save(List<PropertyEntry> lines) {
            List<string> lineList = new List<string>();

            foreach (PropertyEntry entry in lines)
                if (entry is PropertyLine line) {
                    if (line.IsCommentOrEmpty) {
                        lineList.Add(line.Source);
                    } else {
                        string outLine = $"{(line.IsStatic ? "static" : "")}";
                        outLine += $"{(line.IsField ? (line.IsStatic ? " " : "") + "field" : "")}";
                        outLine += $"{(line.IsStatic || line.IsField ? " " : "")}{line.Property}";
                        string value = string.Format(CultureInfo.InvariantCulture, "{0}", line.Value);
                        value   =  value.Replace(Environment.NewLine, "\\n");
                        outLine += $" = {value}";

                        lineList.Add(outLine);
                    }
                } else {
                    throw new InvalidCastException("Unable to cast entry to PropertyLine type. This should have never happen. Something went wrong while creating the Manager, perhaps?");
                }

            File.WriteAllLines(FilePath, lineList.ToArray(), Encoding);
        }
    }
}