# CodeDOMExtension
_namespace: [Microsoft.VisualBasic.Emit.CodeDOM_VBC](./index.md)_





### Methods

#### Compile
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExtension.Compile(System.CodeDom.CodeCompileUnit,System.String[],System.String,System.CodeDom.Compiler.CompilerParameters,System.String)
```
Compile the codedom object model into a binary assembly module file.(将CodeDOM对象模型编译为二进制应用程序文件)

|Parameter Name|Remarks|
|--------------|-------|
|ObjectModel|CodeDom dynamic code object model.(目标动态代码的对象模型)|
|Reference|Reference assemby file path collection.(用户代码的引用DLL文件列表)|
|DotNETReferenceAssembliesDir|.NET Framework SDK|
|CodeStyle|VisualBasic, C#|


#### GenerateCode
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExtension.GenerateCode(System.CodeDom.CodeNamespace,System.String)
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

#### Icon
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.CodeDOMExtension.Icon(System.String)
```
设置所编译的应用程序的图标

|Parameter Name|Remarks|
|--------------|-------|
|iconPath|-|



### Properties

#### DllProfile
.Dll的编译配置文件
#### DotNETFramework
基本的引用集合
#### ExecutableProfile
.exe的编译配置文件
#### net46Default
```
 C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6\
 ```
