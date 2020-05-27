# Property Manager
A configuration tool, which automatically finds and sets any fields or properties based on provided data. It comes bundled with providers to read and write ini files as well as pair of simple interfaces to let you easily write your own.

## Features
* Can set values of fields and properties using Reflection.
* Supported types: bool, int, float, string
* Supports static variables.
* Option to save changes through editable data provider.

## Compatibility
* C# 7.0 and greater

## Installation
With Visual Studio NuGet Package Manager: `PM> Install-Package PropertyManager`

or download library from https://www.nuget.org/packages/PropertyManager/

## Getting started with Property Manager
Most common way of using Property Manager is by invoking static API of `PropertyManager` class. This is used for batch application of all values.

### Applying values through static call
When you get used to it, applying values through static call is quite easy:
```c#
PropertyManager.Apply(this, new PropertyProvider.FilePropertyProvider("fileWithValues.ini"));
```

This one line is a bit deceptive, though, as it does have quite a bit of things happening. Two main topics are Context and providers.

### Providers
Providers are classes implementing `IPropertyProvider` or `IEditablePropertyProvider` interface. The job of a provider is to supply Property Manager with List of `PropertyEntry` objects through implementation of `GetPropertyEntries()` method. Aditionally, `IEditablePropertyProvider` provides `Save()` method, which should be used to saving changes done through Property Manager.

Propery Manager by default contains two providers - `FilePropertyProvider` and `FileEditablePropertyProvider` which implement simple support \*.ini files with line-long comments.

An example of `fileWithValues.ini` file:
```ini
private field id = 42
Name = "Adams"

# Is Adams user of our system?
IsUser = true
```

You may notice that there are two qualificators before `id` variable. Each variable is, by default, treated as public property, so if you're trying to set a private property or a field you need to add proper qualifier.

You can create and configure both providers using an initializer. Here's an example based on `FilePropertyProvider` class:
```c#
var provider = new FilePropertyProvider("fileWithValues.ini") {
  CommentSign = "#",
  Encoding = System.Text.Encoding.UTF8,
  Culture = CultureInfo.InvariantCulture,
  AllValuesAsString = false
};
```

Methods `GetPropertyEntries()` and `Save()` in the providers are for internal use for Property Manager and don't need to be called manualy.

### Context
Context is a starting point for Property Manager from which the system will look for fields and propertis.

Check out this example that expands on static call line from above:
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

The context of the call is userObject object, which means that all lines from the files will be applied to this userObject object. In this example after the calls of `Apply()` method values stored in userObject object will be 42, "Adams" and true for id, Name and IsUser respectively.

This approach, while quick and convinient, is a read-only approach. To make full use out of editable providers you'll need...

### Instance of Property Manager
Creating an editable instance of Property Manager is as strightforward as any other class.

```c#
var manager = new PropertyManager(new PropertyProvider.FileEditablePropertyProvider("fileWithValues.ini"));
```

A Property Manager instance give you access to `Apply()` method that can be used exactly the same way as the static version. Aside of that, you can also use `GetValue()` method or indexer to retrive value from particular key.
```c#
string name = manager.GetValue<string>("Name");
string sameName = manager["Name"];
```

### Editable values
To change value under key you can use `SetValue` method.
```c#
manager.SetValue<string>("Name", "Duglas");
```

And to update source file just call `Save()` method that's provided by the instance.
```c#
manager.Save();
```

## Concurrency
Property Manager is thread-safe in non-editable mode. It's no when editable provider is used.
