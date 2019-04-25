# Property Manager
Configuration tool, which automatically finds and sets any fields or properties based on provided data. It comes bundled with providers to read and write ini files as well as pair of simple interfaces to let you easily write your own.

## Features
* Can set values of fields and properties using Reflection.
* Supported types: bool, int, float, string
* Supports static variables.
* Option to save changes through editable data provider.

## Compatibility
* C# 7.0 and greater

## Installation
Under construction...

## Getting started with Property Manager
Most common way of using Property Manager is by invoking static API of `PropertyManager` class. This is used for batch application of all values.

### Applying values through static call
When you get used to it, applying values through static call is quite easy:
```c#
PropertyManager.Apply(this, new PropertyProvider.FilePropertyProvider("fileWithValues.ini"));
```

This one line is a bit deceptive, though, as it does have quite a bit of things happening. Two main topics are Context and providers.

### Providers
Providers are classes implementing `IPropertyProvider` or `IEditablePropertyProvider` interface. The job of a provider is to supply Property Manager with List of `PropertyEntry` objects through implementation of `GetPropertyEntries()` method.

### Context
Context is a starting point for Property Manager from which the system will look for fields and propertis.

Check out this example that expands on our static call line:
```c#
class User {
  private int id;
  public string Name { get; set; }
  public bool IsUser { get; set; }
    
  public User() { }
}

internal class Program {
  private static void Main(string[] args) {
    User userObject = new User();
    
    PropertyManager.Apply(userObject, new PropertyProvider.FilePropertyProvider("fileWithValues.ini"));
  }
}
```

And as for `fileWithValues.ini` file, it looks like this:
```ini
private field id = 42
Name = "Adams"
IsUser = true
```

