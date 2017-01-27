# Settings`1
_namespace: [Microsoft.VisualBasic.ComponentModel.Settings](./index.md)_





### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Settings`1.#ctor(`0)
```
从配置数据的实例对象创建配置映射

|Parameter Name|Remarks|
|--------------|-------|
|config|-|


#### Load
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Settings`1.Load(`0)
```
使用@``T:Microsoft.VisualBasic.ComponentModel.Settings.ProfileItem``来标记想要作为变量的属性

|Parameter Name|Remarks|
|--------------|-------|
|Data|-|


#### LoadFile
```csharp
Microsoft.VisualBasic.ComponentModel.Settings.Settings`1.LoadFile(System.String,System.Action{`0,System.String})
```


|Parameter Name|Remarks|
|--------------|-------|
|XmlFile|目标配置文件的Xml文件的文件名|


_returns: 可以调用的配置项的数目，解析失败则返回0_


### Properties

#### SettingsData
The target object instance that provides the data source for this config engine.
