# Resources
_namespace: [Microsoft.VisualBasic.SoftwareToolkits](./index.md)_

Represents a resource manager that provides convenient access to culture-specific
 resources at run time.Security Note: Calling methods in this class with untrusted
 data is a security risk. Call the methods in the class only with trusted data.
 For more information, see Untrusted Data Security Risks.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.SoftwareToolkits.Resources.#ctor(System.Type,System.Reflection.Assembly)
```


|Parameter Name|Remarks|
|--------------|-------|
|my|null|
|assm|-|


#### GetObject
```csharp
Microsoft.VisualBasic.SoftwareToolkits.Resources.GetObject(System.String,System.Globalization.CultureInfo)
```
Gets the value of the specified non-string resource localized for the specified
 culture.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name of the resource to get.|
|culture|The culture for which the resource is localized. If the resource is not localized
 for this culture, the resource manager uses fallback rules to locate an appropriate
 resource.If this value is null, the System.Globalization.CultureInfo object is
 obtained by using the System.Globalization.CultureInfo.CurrentUICulture property.|


_returns: The value of the resource, localized for the specified culture. If an appropriate
 resource set exists but name cannot be found, the method returns null._

#### GetStream
```csharp
Microsoft.VisualBasic.SoftwareToolkits.Resources.GetStream(System.String,System.Globalization.CultureInfo)
```
Returns an unmanaged memory stream object from the specified resource, using
 the specified culture.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name of a resource.|
|culture|An object that specifies the culture to use for the resource lookup. If culture
 is null, the culture for the current thread is used.|


_returns: An unmanaged memory stream object that represents a resource._

#### GetString
```csharp
Microsoft.VisualBasic.SoftwareToolkits.Resources.GetString(System.String,System.Globalization.CultureInfo)
```
Returns the value of the string resource localized for the specified culture.

|Parameter Name|Remarks|
|--------------|-------|
|name|The name of the resource to retrieve.|
|culture|An object that represents the culture for which the resource is localized.|


_returns: The value of the resource localized for the specified culture, or null if name
 cannot be found in a resource set._

#### LoadMy
```csharp
Microsoft.VisualBasic.SoftwareToolkits.Resources.LoadMy
```
从自身的程序集之中加载数据


### Properties

#### FileName
The file path of the resources satellite assembly.
#### MyResource
Returns the cached ResourceManager instance used by this class.
#### Resources
@``T:System.Resources.ResourceManager`` object in the satellite assembly.
