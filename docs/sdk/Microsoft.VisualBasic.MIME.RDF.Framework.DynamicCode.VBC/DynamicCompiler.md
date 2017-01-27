# DynamicCompiler
_namespace: [Microsoft.VisualBasic.MIME.RDF.Framework.DynamicCode.VBC](./index.md)_

编译整个LINQ语句的动态代码编译器



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MIME.RDF.Framework.DynamicCode.VBC.DynamicCompiler.#ctor(System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|SDK|.NET Framework Reference Assembly文件夹的位置|


#### Compile
```csharp
Microsoft.VisualBasic.MIME.RDF.Framework.DynamicCode.VBC.DynamicCompiler.Compile(System.CodeDom.CodeCompileUnit,System.String[],System.String)
```
Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)

|Parameter Name|Remarks|
|--------------|-------|
|ObjectModel|CodeDom dynamic code object model.(目标动态代码的对象模型)|
|Reference|Reference assemby file path collection.(用户代码的引用DLL文件列表)|
|CodeStyle|VisualBasic, C#|


#### GenerateCode
```csharp
Microsoft.VisualBasic.MIME.RDF.Framework.DynamicCode.VBC.DynamicCompiler.GenerateCode(System.CodeDom.CodeNamespace,System.String)
```
Generate the source code from the CodeDOM object model.(根据对象模型生成源代码以方便调试程序)

|Parameter Name|Remarks|
|--------------|-------|
|NameSpace|-|
|CodeStyle|VisualBasic, C#|

> 
>  You can easily convert the source code between VisualBasic and C# using this function just by makes change in statement: 
>  CodeDomProvider.GetCompilerInfo("VisualBasic").CreateProvider().GenerateCodeFromNamespace([NameSpace], sWriter, Options)
>  Modify the VisualBasic in to C#
>  


### Properties

#### DotNETReferenceAssembliesDir

