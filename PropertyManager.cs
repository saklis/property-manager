using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PropertyManager.PropertyProvider.Interface;

namespace PropertyManager {
    /// <summary>
    ///     Property Manager is a Reflection-based tool for applying values stored in ini configuration
    ///     file to provided objects.
    /// </summary>
    public class PropertyManager {
        /// <summary>
        ///     List of Property entries of local instance.
        /// </summary>
        private List<PropertyEntry> _properties;

        /// <summary>
        ///     Provider used by local instance.
        /// </summary>
        private IPropertyProvider _provider;

        /// <summary>
        ///     Create new instance of PropertyManager.
        /// </summary>
        /// <param name="provider">Provider for property values.</param>
        /// <exception cref="ArgumentNullException">Thrown when provider is empty.</exception>
        public PropertyManager(IPropertyProvider provider) {
            Editable = false;

            _provider   = provider ?? throw new ArgumentNullException(nameof(provider), "Provider cannot be empty.");
            _properties = _provider.GetPropertyEntries();
        }

        /// <summary>
        ///     Create new instance of PropertyManager.
        /// </summary>
        /// <param name="provider">Provider for property values that supports batch data edition.</param>
        /// <exception cref="ArgumentNullException">Thrown when provider is empty.</exception>
        public PropertyManager(IEditablePropertyProvider provider) {
            Editable = true;

            _provider   = provider ?? throw new ArgumentNullException(nameof(provider), "Provider cannot be empty.");
            _properties = _provider.GetPropertyEntries();
        }

        /// <summary>
        ///     Gets if PropertyManager can save changes to source.
        /// </summary>
        public bool Editable { get; }

        /// <summary>
        ///     Get value from entry under key as string.
        /// </summary>
        /// <param name="key">Key for property entry</param>
        public string this[string key] {
            get {
                PropertyEntry property = _properties.SingleOrDefault(p => p.Property == key);
                return property is null ? key : property.Value.ToString();
            }
        }

        /// <summary>
        ///     Apply values from provider to an object.
        /// </summary>
        /// <param name="context">Object to which values will be applied.</param>
        /// <param name="provider">Provider for Property entries' list.</param>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        public static void Apply(object context, IPropertyProvider provider) {
            foreach (PropertyEntry entry in provider.GetPropertyEntries()) entry.Apply(context);
        }

        /// <summary>
        ///     Apply values from provider to static context within a Type. Can only be used with static entries.
        /// </summary>
        /// <param name="context">Type which will be used as context for static values to be applied.</param>
        /// <param name="provider">Provider for Property entries' list.</param>
        /// <exception cref="ArgumentException">Thrown when entry is not marked as static.</exception>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        public static void Apply(Type context, IPropertyProvider provider) {
            foreach (PropertyEntry entry in provider.GetPropertyEntries()) entry.Apply(context);
        }

        /// <summary>
        ///     Get value stored under key in provider.
        /// </summary>
        /// <param name="key">Key for property entry.</param>
        /// <param name="provider">Provider for property values.</param>
        /// <typeparam name="T">Type of value of property.</typeparam>
        /// <returns>Property's value.</returns>
        /// <exception cref="ArgumentException">Thrown when key was not defined in the provider.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when declared and actual type of property don't match.
        /// </exception>
        public static T GetValue<T>(string key, IPropertyProvider provider) {
            PropertyEntry property = provider.GetPropertyEntries().SingleOrDefault(p => p.Property == key);
            if (property is null) throw new ArgumentException($"Key {key} was not found in provider.", nameof(key));
            if (property.PropertyType != typeof(T)) throw new ArgumentException($"Type of property under key {key} is not the same as declared. Property type: {property.PropertyType}; Declared type: {typeof(T)}");
            return (T) property.Value;
        }

        /// <summary>
        ///     Returns if provider contains key.
        /// </summary>
        /// <param name="key">Key for property entry.</param>
        /// <returns>If provider contains key.</returns>
        public bool ContainsKey(string key) {
            return _properties.Any(x => x.Property == key);
        }

        /// <summary>
        ///     Apply values from provider to an object.
        /// </summary>
        /// <param name="context">Object to which values will be applied.</param>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        public void Apply(object context) {
            foreach (PropertyEntry entry in _properties) entry.Apply(context);
        }

        /// <summary>
        ///     Apply values from provider to static context within a Type. Can only be used with static entries.
        /// </summary>
        /// <param name="context">Type which will be used as context for static values to be applied.</param>
        /// <exception cref="ArgumentException">Thrown when entry is not marked as static.</exception>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        public void Apply(Type context) {
            foreach (PropertyEntry entry in _properties) entry.Apply(context);
        }

        /// <summary>
        ///     Get value stored under key.
        /// </summary>
        /// <param name="key">Key for property entry.</param>
        /// <typeparam name="T">Type of value of property.</typeparam>
        /// <returns>Property's value.</returns>
        /// <exception cref="ArgumentException">Thrown when key was not defined in the provider.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when declared and actual type of property don't match.
        /// </exception>
        public T GetValue<T>(string key) {
            PropertyEntry property = _properties.SingleOrDefault(p => p.Property == key);
            if (property is null) throw new ArgumentException($"Key {key} was not found in provider.", nameof(key));
            if (property.PropertyType != typeof(T)) throw new ArgumentException($"Type of property under key {key} is not the same as declared. Property type: {property.PropertyType}; Declared type: {typeof(T)}");
            return (T) property.Value;
        }

        /// <summary>
        ///     Set value stored under key.
        /// </summary>
        /// <typeparam name="T">Type of value of property.</typeparam>
        /// <param name="key">Key for property value.</param>
        /// <param name="value">New value for property.</param>
        /// <exception cref="ArgumentException">Thrown when key was not defined in the provider.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when declared and actual type of property don't match.
        /// </exception>
        public void SetValue<T>(string key, T value) {
            if (!Editable) throw new InvalidOperationException("Manager is not Editable. To be able to save changes use provider of type IEditablePropertyProvider.");
            PropertyEntry property = _properties.SingleOrDefault(p => p.Property == key);
            if (property is null) throw new ArgumentException($"Key {key} was not found in provider.", nameof(key));
            if (property.PropertyType != typeof(T)) throw new ArgumentException($"Type of property under key {key} is not the same as declared. Property type: {property.PropertyType}; Declared type: {typeof(T)}");
            property.Value = value;
        }

        /// <summary>
        ///     Save all properties to source.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when manager is not paired with Editable Provider.</exception>
        /// <exception cref="AmbiguousMatchException">Provider doesn't implement <see cref="IEditablePropertyProvider"/> interface.</exception>
        public void Save() {
            if (!Editable) throw new InvalidOperationException("Manager is not Editable. To be able to save changes use provider of type IEditablePropertyProvider.");
            if (_provider is IEditablePropertyProvider provider)
                provider.Save(_properties);
            else
                throw new AmbiguousMatchException("Provider can't be cast to IEditablePropertyProvider despite manager being market as Editable.");
        }

        /// <summary>
        ///     Reload properties from current provider.
        /// </summary>
        public void ReloadProperties() {
            _properties = _provider.GetPropertyEntries();
        }

        /// <summary>
        ///     Reload properties from new provider.
        /// </summary>
        /// <param name="provider">Provider for property values.</param>
        public void ReloadProperties(IPropertyProvider provider) {
            _provider   = provider;
            _properties = _provider.GetPropertyEntries();
        }
    }
}