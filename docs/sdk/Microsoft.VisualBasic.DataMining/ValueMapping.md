# ValueMapping
_namespace: [Microsoft.VisualBasic.DataMining](./index.md)_





### Methods

#### ModalNumber
```csharp
Microsoft.VisualBasic.DataMining.ValueMapping.ModalNumber(System.Int32[])
```
Gets the modal number of the ranking mapping data set.(求取众数)

|Parameter Name|Remarks|
|--------------|-------|
|data|The ranked mapping encoding value.(经过Rank Mapping处理过后的编码值)|

> 
>  当不存在相同的分组元素数目的时候，会直接取第一个元素的值作为众数
>  当存在相同的分组元素数目的时候，会取最大的元素值作为众数
>  


