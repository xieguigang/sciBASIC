# OrderSelector`1
_namespace: [Microsoft.VisualBasic.ComponentModel.Ranges](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.OrderSelector`1.#ctor(System.Collections.Generic.IEnumerable{`0},System.Boolean)
```


|Parameter Name|Remarks|
|--------------|-------|
|source|-|
|asc|
 当这个参数为真的时候为升序排序
 |


#### FirstGreaterThan
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.OrderSelector`1.FirstGreaterThan(`0)
```
遍历整个列表直到找到第一个大于**`o`**的元素，然后函数会返回这第一个元素的index

|Parameter Name|Remarks|
|--------------|-------|
|o|-|


_returns: 
 返回-1表示这个列表之中没有任何元素是大于输入的参数**`o`**的
 _

#### SelectUntilGreaterThan
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.OrderSelector`1.SelectUntilGreaterThan(`0)
```
直到当前元素大于指定值

|Parameter Name|Remarks|
|--------------|-------|
|n|-|


#### SelectUntilLessThan
```csharp
Microsoft.VisualBasic.ComponentModel.Ranges.OrderSelector`1.SelectUntilLessThan(`0)
```
直到当前元素小于指定值

|Parameter Name|Remarks|
|--------------|-------|
|n|-|



### Properties

#### Desc
是否为降序排序?
