# Parameter
_namespace: [Microsoft.VisualBasic.Scripting.MetaData](./index.md)_

You Cann assign the parameter value using the parameter's alias name in the scripting using this attribute.
 (你可以使用本属性将函数的参数名进行重命名，这样子你就可以使用本属性得到一个书写更加漂亮的编程脚本文件了)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Scripting.MetaData.Parameter.#ctor(System.String,System.String)
```
You can using this attribute to customize your API interface.

|Parameter Name|Remarks|
|--------------|-------|
|Alias|The alias name of this function parameter in the scripting.(当前脚本函数的这个参数的别名)|
|MyDescription|The description information in the scripting help system.(这个信息会显示在脚本环境的帮助系统之中)|


#### GetAliasNameView
```csharp
Microsoft.VisualBasic.Scripting.MetaData.Parameter.GetAliasNameView(System.Reflection.ParameterInfo)
```
当没有定义属性的时候，会返回参数名

|Parameter Name|Remarks|
|--------------|-------|
|pInfo|-|



### Properties

#### Alias
The alias name of this function parameter in the scripting.(脚本函数的参数的别名)
#### Description
The description information in the scripting help system.(在帮助信息里面进行显示的本参数的简要的描述信息)
#### ParameterInfo
请使用这个方法@``M:Microsoft.VisualBasic.Scripting.MetaData.Parameter.GetParameterNameAlias(System.Reflection.ParameterInfo,Microsoft.VisualBasic.Scripting.MetaData.Parameter)``来获取参数信息
