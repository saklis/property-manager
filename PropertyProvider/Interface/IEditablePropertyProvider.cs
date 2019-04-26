using System.Collections.Generic;

namespace PropertyManager.PropertyProvider.Interface {
    /// <summary>
    ///     Provides list of lines in property file.
    /// </summary>
    public interface IEditablePropertyProvider : IPropertyProvider {
        /// <summary>
        ///     Save lines to source.
        /// </summary>
        /// <param name="lines">Lines to save.</param>
        void Save(List<PropertyEntry> lines);
    }
}