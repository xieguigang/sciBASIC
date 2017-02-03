# ConfigurationMappings
_namespace: [Microsoft.VisualBasic.Serialization](./index.md)_

最基本的思想是将属性值按照同名属性名称在A和B两个对象类型之间进行映射，即A与B两个对象之间必须要具备相同的属性名称，才可以产生映射，请注意在本对象之中仅能够映射最基本的值类型的数据类型
 对于一些自定义的映射操作，请在目标数据模型之中定义自定义的映射函数，要求为函数只有一个参数，参数类型和返回值类型分别为映射的两个节点的数据类型，程序会使用反射自动查找



### Methods

#### GetNodeMapping``2
```csharp
Microsoft.VisualBasic.Serialization.ConfigurationMappings.GetNodeMapping``2(System.Object)
```
获取从源映射至数据模型的映射过程

#### LoadMapping``2
```csharp
Microsoft.VisualBasic.Serialization.ConfigurationMappings.LoadMapping``2(``1)
```
从源江基本的值类型映射到数据模型，以将配置数据读取出来并进行加载

#### WriteMapping``2
```csharp
Microsoft.VisualBasic.Serialization.ConfigurationMappings.WriteMapping``2(``0)
```
从数据模型将值类型数据映射回源，以将配置数据写入文件

|Parameter Name|Remarks|
|--------------|-------|
|Model|-|



