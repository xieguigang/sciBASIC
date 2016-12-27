# Set
_namespace: [Microsoft.VisualBasic.ComponentModel.DataStructures](./index.md)_

Represents an unordered grouping of unique hetrogenous members.
 (这个对象的功能和List类似，但是这个对象的主要的作用是进行一些集合运算：使用AND求交集以及使用OR求并集的)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.#ctor(Microsoft.VisualBasic.ComponentModel.DataStructures.Set[])
```
Constructor called when the source data is an array of @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Sets]. They will
 be unioned together, with addition exceptions quietly eaten.

|Parameter Name|Remarks|
|--------------|-------|
|sources|The source array of @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] objects.|


#### Add
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Add(System.Object)
```
Method to add an @``T:System.Object``[Object] to the set. The new member 
 must be unique.

|Parameter Name|Remarks|
|--------------|-------|
|member|@``T:System.Object``[Object] to add.|


#### Clear
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Clear
```
Empty the set of all members.

#### Contains
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Contains(System.Object)
```
Method to determine if a given object is a member of the set.

|Parameter Name|Remarks|
|--------------|-------|
|target|The object to look for in the set.|


_returns: True if it is a member of the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set], false if not._

#### Dispose
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Dispose
```
Performs cleanup tasks on the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] object.

#### Equals
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Equals(System.Object)
```
Determines whether two @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] instances are equal.

|Parameter Name|Remarks|
|--------------|-------|
|obj|The @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] to compare to the current Set.|


_returns: true if the specified @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] is equal to the current 
 Set; otherwise, false._

#### GetHashCode
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.GetHashCode
```
Serves as a hash function for a particular type, suitable for use in hashing 
 algorithms and data structures like a hash table.

_returns: A hash code for the current @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set]._

#### IEnumerable_GetEnumerator
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.IEnumerable_GetEnumerator
```
Returns an enumerator that can iterate through a collection.

_returns: An @``T:System.Collections.IEnumerator``[IEnumerator] that can be 
 used to iterate through the collection._

#### IsEmpty
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.IsEmpty
```
A method to determine whether the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] has members.

_returns: True is there are members, false if there are 0 members._

#### op_Addition
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_Addition(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
求两个集合的并集，将两个集合之中的所有元素都合并在一起，这个操作符会忽略掉重复出现的元素

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|


#### op_BitwiseAnd
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_BitwiseAnd(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
Performs an intersection of two sets.(求交集)

|Parameter Name|Remarks|
|--------------|-------|
|s1|Any set.|
|s2|Any set.|


_returns: A new @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] object that contains the members
 that were common to both of the input sets._

#### op_BitwiseOr
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_BitwiseOr(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
Performs a union of two sets.(求并集)

|Parameter Name|Remarks|
|--------------|-------|
|s1|Any set.|
|s2|Any set.|


_returns: A new @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] object that contains all of the
 members of each of the input sets._

#### op_Equality
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_Equality(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
Overloaded == operator to determine if 2 sets are equal.

|Parameter Name|Remarks|
|--------------|-------|
|s1|Any set.|
|s2|Any set.|


_returns: True if the two comparison sets have the same number of elements, and
 all of the elements of set s1 are contained in s2._

#### op_Explicit
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_Explicit(System.Array)~Microsoft.VisualBasic.ComponentModel.DataStructures.Set
```
If the Set is created by casting an array to it, add the members of
 the array through the Add method, so if the array has dupes an error
 will occur.

|Parameter Name|Remarks|
|--------------|-------|
|array|The array with the objects to initialize the array.|


_returns: A new Set object based on the members of the array._

#### op_Inequality
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_Inequality(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
Overloaded != operator to determine if 2 sets are unequal.

|Parameter Name|Remarks|
|--------------|-------|
|s1|A benchmark set.|
|s2|The set to compare against the benchmark.|


_returns: True if the two comparison sets fail the equality (==) test,
 false if the pass the equality test._

#### op_Subtraction
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.op_Subtraction(Microsoft.VisualBasic.ComponentModel.DataStructures.Set,Microsoft.VisualBasic.ComponentModel.DataStructures.Set)
```
except(差集)集合运算：先将其中完全重复的数据行删除，再返回只在第一个集合中出现，在第二个集合中不出现的所有行。

|Parameter Name|Remarks|
|--------------|-------|
|s1|-|
|s2|-|


#### Remove
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.Remove(System.Object)
```
Remove a member from the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set].

|Parameter Name|Remarks|
|--------------|-------|
|target|The member to remove.|


_returns: True if a member was removed, false if nothing was found that 
 was removed._

#### ToArray
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.ToArray
```
Copies the members of the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set] to an array of 
 @``T:System.Object``[Objects].

_returns: An @``T:System.Object``[Object] array copies of the 
 elements of the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set]_

#### ToArray``1
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.ToArray``1
```
DirectCast

#### ToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataStructures.Set.ToString
```
Returns a @``T:System.String``[String] that represents the current
 @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set].

_returns: A @``T:System.String``[String] that represents the current
 @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set]._


### Properties

#### Item
Public accessor for the members of the @``T:Microsoft.VisualBasic.ComponentModel.DataStructures.Set``[Set].
#### Length
The number of members of the set.
