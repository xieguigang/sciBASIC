# Dictionary`1
_namespace: [Microsoft.VisualBasic](./index.md)_

Represents a collection of keys and values.To browse the .NET Framework source
 code for this type, see the Reference Source.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Dictionary`1.#ctor(System.Collections.Generic.Dictionary{System.String,`0})
```
Initializes a new instance of the System.Collections.Generic.SortedDictionary`2
 class that contains elements copied from the specified System.Collections.Generic.IDictionary`2
 and uses the default System.Collections.Generic.IComparer`1 implementation for
 the key type.

|Parameter Name|Remarks|
|--------------|-------|
|source|
 The System.Collections.Generic.IDictionary`2 whose elements are copied to the
 new System.Collections.Generic.SortedDictionary`2.
 |


#### Add
```csharp
Microsoft.VisualBasic.Dictionary`1.Add(`0)
```
Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.

|Parameter Name|Remarks|
|--------------|-------|
|item|-|


#### Find
```csharp
Microsoft.VisualBasic.Dictionary`1.Find(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|name|不区分大小写的|


#### op_Addition
```csharp
Microsoft.VisualBasic.Dictionary`1.op_Addition(Microsoft.VisualBasic.Dictionary{`0},`0)
```
Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.

|Parameter Name|Remarks|
|--------------|-------|
|hash|-|
|item|-|


#### op_Exponent
```csharp
Microsoft.VisualBasic.Dictionary`1.op_Exponent(Microsoft.VisualBasic.Dictionary{`0},System.String)
```
Find a variable in the hash table

|Parameter Name|Remarks|
|--------------|-------|
|hash|-|
|uid|-|


#### op_LessThanOrEqual
```csharp
Microsoft.VisualBasic.Dictionary`1.op_LessThanOrEqual(Microsoft.VisualBasic.Dictionary{`0},System.String)
```
Get value by key.

|Parameter Name|Remarks|
|--------------|-------|
|hash|-|
|key|-|


#### Remove
```csharp
Microsoft.VisualBasic.Dictionary`1.Remove(`0)
```
假若目标元素不存在于本字典之中，则会返回False

|Parameter Name|Remarks|
|--------------|-------|
|x|-|


#### SafeGetValue
```csharp
Microsoft.VisualBasic.Dictionary`1.SafeGetValue(System.String,`0@,System.Boolean@)
```
If the value is not found in the hash directionary, then the default value will be returns, and the default value is nothing.

|Parameter Name|Remarks|
|--------------|-------|
|name|-|
|[default]|-|
|success|可能value本身就是空值，所以在这里使用这个参数来判断是否存在|


#### TryGetValue
```csharp
Microsoft.VisualBasic.Dictionary`1.TryGetValue(System.String,System.Boolean@)
```
Gets the value associated with the specified key.

|Parameter Name|Remarks|
|--------------|-------|
|name|The key of the value to get.|
|success|true if the System.Collections.Generic.SortedDictionary`2 contains an element
 with the specified key; otherwise, false.|


_returns: When this method returns, the value associated with the specified key, if the
 key is found; otherwise, the default value for the type of the value parameter._


