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
    public class FilePropertyProvider : IPropertyProvider {
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
        public FilePropertyProvider(string filePath) {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found or inaccessible.", filePath);

            FilePath = filePath;
        }

        /// <summary>
        ///     Character uses in the file as the marker for comments. Must be first non-whitespace
        ///     character to be recognized.
        /// </summary>
        public string CommentSign { get; set; } = "#";

        /// <summary>
        ///     File's enciding used while reading file.
        /// </summary>
        private Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        ///     Path to file.
        /// </summary>
        private string FilePath { get; }

        /// <summary>
        ///     Parse read file, creating <see cref="PropertyEntry" /> objects.
        /// </summary>
        /// <returns>List of Property entries.</returns>
        public List<PropertyEntry> GetPropertyEntries() {
            _allLines = File.ReadAllLines(FilePath, Encoding);

            List<PropertyEntry> entries = new List<PropertyEntry>(_allLines.Length);

            foreach (string line in _allLines)
                if (line.Trim().Length != 0 && !line.Trim().StartsWith(CommentSign) && line.Contains("=")) {
                    PropertyEntry newEntry = new PropertyEntry();

                    // separate key and value
                    string[] strings    = line.Split(new[] {'='}, 2);
                    string   properties = strings[0].Trim();
                    string   value      = strings[1].Trim();

                    // parse property name and arguments
                    string[] arguments = properties.Split(' ');
                    if (arguments.Length > 1) {
                        if (arguments.Contains("static")) newEntry.IsStatic = true;
                        if (arguments.Contains("field")) newEntry.IsField   = true;
                    }

                    newEntry.Property = arguments[arguments.Length - 1];

                    // parse value and type
                    if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue)) {
                        newEntry.PropertyType = typeof(int);
                        newEntry.Value        = intValue;
                    } else if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue)) {
                        newEntry.PropertyType = typeof(float);
                        newEntry.Value        = floatValue;
                    } else if (bool.TryParse(value, out bool boolValue)) {
                        newEntry.PropertyType = typeof(bool);
                        newEntry.Value        = boolValue;
                    } else {
                        newEntry.PropertyType = typeof(string);
                        newEntry.Value        = value.Replace("\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                    }

                    entries.Add(newEntry);
                }

            return entries;
        }
    }
}