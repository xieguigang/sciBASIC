# Iterator
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel](./index.md)_

Implements for the @``T:System.Collections.Generic.IEnumerable`1``, Supports a simple iteration over a non-generic collection.
 (这个迭代器对象主要是用在远程数据源之中的，对于本地的数据源而言，使用这个迭代器的效率太低了，但是对于远程数据源而言，由于存在网络延迟，所以这个迭代器的效率影响将可以被忽略不计)



### Methods

#### GetEnumerator
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Iterator.GetEnumerator
```
Exposes an enumerator, which supports a simple iteration over a non-generic collection.To
 browse the .NET Framework source code for this type, see the Reference Source.

#### MoveNext
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Iterator.MoveNext
```
Advances the enumerator to the next element of the collection.

_returns: 
 true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
 _

#### Read
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Iterator.Read
```
Returns current and then automatically move to next position

#### Reset
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.Iterator.Reset
```
Sets the enumerator to its initial position, which is before the first element in the collection.


### Properties

#### Current
Gets the current element in the collection.
#### ReadDone
Indicates that there are no more characters in the string and tokenizer is finished.
