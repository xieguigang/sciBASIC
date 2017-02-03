# ChangeLog
_namespace: [Microsoft.VisualBasic.SoftwareToolkits](./index.md)_

Tools for generate the program change log document.



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.SoftwareToolkits.ChangeLog.#ctor(System.String,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|Path|ChangeLog的文件路径|
|ApplyOn|目标程序的主程序的文件路径|


#### AppendChangeInformation
```csharp
Microsoft.VisualBasic.SoftwareToolkits.ChangeLog.AppendChangeInformation(System.Collections.Generic.IEnumerable{System.String},System.Version,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|Changes|-|
|version|假若为空的话，会自动的根据上一次版本的号码叠加1|



