# KeyValuePairExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_





### Methods

#### Add``2
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.Add``2(Microsoft.VisualBasic.Language.List{Microsoft.VisualBasic.ComponentModel.Collection.Generic.KeyValuePairObject{``0,``1}}@,``0,``1)
```
Adds an object to the end of the List`1.

|Parameter Name|Remarks|
|--------------|-------|
|list|-|
|key|-|
|value|-|


#### ContainsKey
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.ContainsKey(System.Collections.Specialized.NameValueCollection,System.String)
```
Determines whether the @``T:System.Collections.Specialized.NameValueCollection`` contains the specified key.

|Parameter Name|Remarks|
|--------------|-------|
|d|-|
|key$|The key to locate in the @``T:System.Collections.Specialized.NameValueCollection``|


_returns: true if the System.Collections.Generic.Dictionary`2 contains an element with
 the specified key; otherwise, false._

#### HaveData``1
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.HaveData``1(System.Collections.Generic.Dictionary{``0,System.String},``0)
```
Data exists and not nothing

|Parameter Name|Remarks|
|--------------|-------|
|d|-|
|key|-|


#### ParserDictionary``1
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.ParserDictionary``1
```
请注意，这里的类型约束只允许枚举类型

#### ReverseMaps``2
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.ReverseMaps``2(System.Collections.Generic.Dictionary{``0,``1},System.Boolean)
```
使用这个函数应该要确保value是没有重复的

|Parameter Name|Remarks|
|--------------|-------|
|d|-|


#### ToDictionary``1
```csharp
Microsoft.VisualBasic.KeyValuePairExtensions.ToDictionary``1(System.Collections.Generic.IEnumerable{``0})
```
Creates a @``T:System.Collections.Generic.Dictionary`2```2 from an @``T:System.Collections.Generic.IEnumerable`1```1
 according to a specified key selector function.

|Parameter Name|Remarks|
|--------------|-------|
|source|-|



