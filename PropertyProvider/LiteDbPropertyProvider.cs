using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiteDB;
using PropertyManager.PropertyProvider.Interface;

namespace PropertyManager.PropertyProvider {
    public class LiteDbPropertyProvider : IPropertyProvider {
        /// <summary>
        /// Path to LiteDB file.
        /// </summary>
        public string Path { get; }

        public string Collection { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LiteDBPropertyProvider" /> class.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="collection"></param>
        public LiteDbPropertyProvider(string path, string collection) {
            Path = path;
            Collection = collection;
        }

        /// <summary>
        ///     Get list of property entries.
        /// </summary>
        /// <returns>List of property entries.</returns>
        public List<PropertyEntry> GetPropertyEntries() {
            List<PropertyEntry> entries = new List<PropertyEntry>();
            using (LiteDatabase db = new LiteDatabase(Path)) {
                LiteCollection<Configuration> col = db.GetCollection<Configuration>(Collection);
                IEnumerable<Configuration> configs = col.FindAll();

                foreach (Configuration config in configs) {
                    PropertyEntry entry = new PropertyEntry {
                                                                Property = config.PropertyName,
                                                                IsStatic = config.IsStatic,
                                                                IsField  = config.IsField
                                                            };
                    
                    // parse value and type
                    switch (config.PropertyValue) {
                        case int intValue:
                            entry.PropertyType = typeof(int);
                            entry.Value        = intValue;
                            break;
                        case float floatValue:
                            entry.PropertyType = typeof(float);
                            entry.Value        = floatValue;
                            break;
                        case bool boolValue:
                            entry.PropertyType = typeof(bool);
                            entry.Value        = boolValue;
                            break;
                        case string stringValue:
                            entry.PropertyType = typeof(string);
                            entry.Value        = stringValue;
                            break;
                    }

                    entries.Add(entry);
                }
            }

            return entries;
        }
    }

    /// <summary>
    /// Model class to map db collection to.
    /// </summary>
    internal class Configuration {
        public LiteDB.ObjectId Id { get; set; }
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public bool IsStatic { get; set; }
        public bool IsField { get; set; }
    }
}