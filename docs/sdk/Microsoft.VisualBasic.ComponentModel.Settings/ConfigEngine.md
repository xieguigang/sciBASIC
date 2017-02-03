# ConfigEngine
_namespace: [Microsoft.VisualBasic.ComponentModel.Settings](./index.md)_

只包含有对数据映射目标对象的属性读写，并不包含有文件数据的读写操作



### Methods

#### ExistsNode
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.ConfigEngine.ExistsNode(System.String)
```
大小写不敏感

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|


#### GetSettingsNode
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.ConfigEngine.GetSettingsNode(System.String)
```
大小写不敏感的

|Parameter Name|Remarks|
|--------------|-------|
|Name|-|


#### Set
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.ConfigEngine.Set(System.String,System.String)
```
请注意，**`name`**必须是小写的

|Parameter Name|Remarks|
|--------------|-------|
|Name|The name of the configuration entry should be in lower case.|
|Value|-|


#### View
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.ConfigEngine.View(System.String)
```
假若函数参数**`name`**为空，则函数输出所有的变量的值，请注意，这个函数并不在终端上面显示任何消息

|Parameter Name|Remarks|
|--------------|-------|
|name|假若本参数为空，则函数输出所有的变量的值，大小写不敏感的|



### Properties

#### _SettingsData
所映射的数据源
#### AllItems
List all of the available settings nodes in this profile data session.
 (枚举出当前配置会话之中的所有可用的配置节点)
#### ProfileItemCollection
键名都是小写的
