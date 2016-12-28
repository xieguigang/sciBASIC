# VBC
_namespace: [Microsoft.VisualBasic.Emit.CodeDOM_VBC](./index.md)_

Extension wrappers for VisualBasic compiler



### Methods

#### CompileCode
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.VBC.CompileCode(System.String,System.CodeDom.Compiler.CompilerParameters,System.String@)
```
If the code compile failure, then this function will returns nothing.

|Parameter Name|Remarks|
|--------------|-------|
|code|VisualBasic源代码|


_returns: @``T:System.Reflection.Assembly`` from the source **`code`**_

#### CreateParameters
```csharp
Microsoft.VisualBasic.Emit.CodeDOM_VBC.VBC.CreateParameters(System.Collections.Generic.IEnumerable{System.String},System.String,System.Boolean)
```
Construct of the ``vbc.exe`` compiler parameters @``T:System.CodeDom.Compiler.CompilerParameters``.

|Parameter Name|Remarks|
|--------------|-------|
|ref|-|
|SDK|-|
|dll|-|



