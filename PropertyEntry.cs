using System;
using System.Reflection;

namespace PropertyManager {
    /// <summary>
    ///     Used by <see cref="PropertyManager" /> to store and apply values.
    /// </summary>
    public class PropertyEntry {
        /// <summary>
        ///     Property name. Can be a call chain separated by '.' sign.
        /// </summary>
        public string Property = "";

        /// <summary>
        ///     Type of property value.
        /// </summary>
        public Type PropertyType = null;

        /// <summary>
        ///     Value that will be assigned.
        /// </summary>
        public object Value = null;

        /// <summary>
        ///     Entry is a field.
        /// </summary>
        public bool IsField { get; set; }

        /// <summary>
        ///     Entry is static.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        ///     Apply this entry to context type. Can only be used with static entries.
        /// </summary>
        /// <param name="context">Type that is a context for entries.</param>
        /// <exception cref="ArgumentException">Thrown when entry is not marked as static.</exception>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if no context was provided.</exception>
        public void Apply(Type context) {
            if (!IsStatic) throw new ArgumentException("This override cannot be used for entries that are not static.");
            if (context is null) throw new ArgumentNullException(nameof(context), "Context cannot be empty.");

            ApplyValue(Property.Split('.'), context, null);
        }

        /// <summary>
        ///     Apply this entry to context object.
        /// </summary>
        /// <param name="context">Object to apply value to.</param>
        /// <exception cref="ArgumentNullException">Thrown if no context was provided.</exception>
        /// <exception cref="ApplicationException">
        ///     Thrown when value in file cannot be accessed from provided context.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element can't be accessed by setter.
        /// </exception>
        public void Apply(object context) {
            if (context is null) throw new ArgumentNullException(nameof(context), "Context cannot be empty.");

            ApplyValue(Property.Split('.'), context.GetType(), context);
        }

        /// <summary>
        ///     Apply actual values
        /// </summary>
        /// <param name="elements">Array of strings representing call path</param>
        /// <param name="targetType">Starting type</param>
        /// <param name="target">Starting object</param>
        /// <exception cref="ApplicationException">
        ///     Thrown when element could not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when element could not be accessed.
        /// </exception>
        private void ApplyValue(string[] elements, Type targetType, object target) {
            for (int i = 0; i < elements.Length; i++)
                if (i < elements.Length - 1) {
                    if (IsStatic && i == 0) {
                        string type                        = $"{targetType.Namespace}.{elements[0]}, {targetType.Assembly.GetName().Name}";
                        Type   staticType                  = Type.GetType(type);
                        if (staticType != null) targetType = staticType;
                    } else {
                        PropertyInfo propertyInfo = targetType.GetProperty(elements[i], (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) ^ BindingFlags.DeclaredOnly);
                        if (propertyInfo != null) {
                            target     = propertyInfo.GetValue(target);
                            targetType = target.GetType();
                        } else {
                            FieldInfo fieldInfo = targetType.GetField(elements[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                            if (fieldInfo != null) {
                                target     = fieldInfo.GetValue(target);
                                targetType = target.GetType();
                            } else {
                                throw new ApplicationException($"Element {Property} don't appear to exist in supplied context or one of the elements of call chain is private.");
                            }
                        }
                    }
                } else {
                    if (IsField) {
                        FieldInfo field = targetType.GetField(elements[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (field is null) throw new InvalidOperationException($"Entry {Property} cannot be accessed.");
                        field.SetValue(IsStatic ? null : target, Value);
                    } else {
                        PropertyInfo property = targetType.GetProperty(elements[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (property is null) throw new InvalidOperationException($"Entry {Property} cannot be accessed.");
                        property.SetValue(IsStatic ? null : target, Value);
                    }
                }
        }
    }
}