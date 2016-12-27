# HandledList`1
_namespace: [Microsoft.VisualBasic.ComponentModel](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.#ctor(System.Int32)
```
Construct a new list object

|Parameter Name|Remarks|
|--------------|-------|
|Capacity|The initialize size of this list object, Optional parameter, default value is 2048|


#### Append
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.Append(`0@)
```
Add a disposable object instance element into this list object and return its object handle value in this list object

|Parameter Name|Remarks|
|--------------|-------|
|e|Object instance that will be store in this list object|


_returns: Object handle in this list object instance_

#### AppendRange
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.AppendRange(System.Collections.Generic.IEnumerable{`0}@)
```
Append a list of object instance

|Parameter Name|Remarks|
|--------------|-------|
|list|-|


#### Exists
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.Exists(`0)
```
Know that a specify object instance exists in this list object or not? 
 (判断某一个指定的对象实例是否存在于列表对象之中)

|Parameter Name|Remarks|
|--------------|-------|
|e|Target object instance(目标要进行查找的对象实例)|


#### Flush
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.Flush
```
Clear all of the data in this list object instance.
 (清除本列表对象中的所有数据)

#### Remove
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.Remove(`0)
```
Remove a specify object in this list object using its hashcode and return its handle value.
 (使用对象的哈希值来查找目标对象并对其进行移除，之后返回其句柄值)

|Parameter Name|Remarks|
|--------------|-------|
|e|-|


#### RemoveAt
```csharp
Microsoft.VisualBasic.ComponentModel.HandledList`1.RemoveAt(System.Int64)
```
Remove a object instance element in this list object that have a specify handler

|Parameter Name|Remarks|
|--------------|-------|
|Handle|Object handle value that specify the target object|



### Properties

#### _EmptyListStack
Stack list that store the empty pointer
#### _HandleList
Exists handle that store in this list
#### _ListData
Object instances data physical storage position, element may be null after 
 remove a specify object handle. 
 (列表中的元素对象实例的实际存储位置，当对象元素从列表之中被移除了之后，其将会被销毁)
#### Count
Get the logical list length
#### Item
Get or set a object instance data that has specify handle value
#### ListData
Get the logical list of the data store in this list object instance.
 (获取逻辑形式的列表数据)
