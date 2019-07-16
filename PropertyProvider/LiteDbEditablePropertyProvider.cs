using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using PropertyManager.PropertyProvider.Interface;

namespace PropertyManager.PropertyProvider {
    public class LiteDbEditablePropertyProvider : IEditablePropertyProvider {
        /// <summary>Initializes a new instance of the <see cref="T:LiteDbEditablePropertyProvider" /> class.</summary>
        public LiteDbEditablePropertyProvider(string path, string collection) {
            Path       = path;
            Collection = collection;
        }

        public string Collection { get; }

        /// <summary>
        ///     Path to LiteDB file.
        /// </summary>
        public string Path { get; }

        private IEnumerable<Configuration> _configs;

        /// <summary>
        ///     Get list of property entries.
        /// </summary>
        /// <returns>List of property entries.</returns>
        public List<PropertyEntry> GetPropertyEntries() {
            List<PropertyEntry> entries = new List<PropertyEntry>();
            using (LiteDatabase db = new LiteDatabase(Path)) {
                LiteCollection<Configuration> col     = db.GetCollection<Configuration>(Collection);
                _configs = col.FindAll();

                foreach (Configuration config in _configs) {
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

        /// <summary>
        ///     Save lines to source.
        /// </summary>
        /// <param name="lines">Lines to save.</param>
        public void Save(List<PropertyEntry> lines) {
            using (LiteDatabase db = new LiteDatabase(Path)) {
                LiteCollection<Configuration> col = db.GetCollection<Configuration>(Collection);

                foreach (PropertyEntry entry in lines) {
                    List<Configuration> found = col.Find(e => e.PropertyName == entry.Property)?.ToList();
                    if (found != null) {
                        if (found.Count == 1) {
                            found[0].IsField       = entry.IsField;
                            found[0].IsStatic      = entry.IsStatic;
                            found[0].PropertyValue = entry.Value;
                            if (col.Update(found) != 1) throw new InvalidOperationException($"Update operation of object with Id:{found[0].Id} failed.");
                        } else if (found.Count == 0) {
                            Configuration newConfig = new Configuration {
                                                                            IsField       = entry.IsField,
                                                                            IsStatic      = entry.IsStatic,
                                                                            PropertyName  = entry.Property,
                                                                            PropertyValue = entry.Value
                                                                        };
                            if (!col.Insert(newConfig).IsObjectId) throw new InvalidOperationException($"Error while trying to insert object with Property Name:[{entry.Property}] into db.");
                        } else
                            throw new InvalidOperationException($"Unable to find distinctive element with Property Name:[{entry.Property}] in db.");
                    } else
                        throw new InvalidOperationException($"Unspecified error while looking for element with Property Name:[{entry.Property}] in db.");
                }
            }
        }
    }
}