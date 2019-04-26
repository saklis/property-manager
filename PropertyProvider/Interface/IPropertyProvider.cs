using System.Collections.Generic;

namespace PropertyManager.PropertyProvider.Interface
{
    /// <summary>
    ///     Provides list of property entries.
    /// </summary>
    public interface IPropertyProvider
    {
        /// <summary>
        ///     Get list of property entries.
        /// </summary>
        /// <returns>List of property entries.</returns>
        List<PropertyEntry> GetPropertyEntries();
    }
}
