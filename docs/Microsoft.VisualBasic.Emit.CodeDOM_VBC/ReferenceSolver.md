# ReferenceSolver
_namespace: [Microsoft.VisualBasic.Emit.CodeDOM_VBC](./index.md)_

Assembly references solver



### Methods

#### __getReferences
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.ReferenceSolver.__getReferences(System.Reflection.Assembly,System.Int32,Microsoft.VisualBasic.Language.List{System.String}@)
```
有一些会出现循环引用的情况？？？？？

|Parameter Name|Remarks|
|--------------|-------|
|assembly|-|
|i|-|
|refList|-|


#### GetReferences
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.ReferenceSolver.GetReferences(System.String,System.Int32,Microsoft.VisualBasic.Language.List{System.String}@)
```


|Parameter Name|Remarks|
|--------------|-------|
|url|+特殊符号存在于这个字符串之中的话，函数会出错|
|i|-|


#### IsSystemAssembly
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.ReferenceSolver.IsSystemAssembly(System.String,System.Boolean)
```
放在C:\WINDOWS\Microsoft.Net\这个文件夹下面的所有的引用都是本地编译的，哈希值已经不对了

|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|strict|-|



### Properties

#### ExecutingReferences
获取当前所执行的应用程序的所有引用dll模块的文件路径列表
