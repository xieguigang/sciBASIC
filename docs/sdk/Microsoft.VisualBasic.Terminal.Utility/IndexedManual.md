# IndexedManual
_namespace: [Microsoft.VisualBasic.Terminal.Utility](./index.md)_

有显示标题的



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.Terminal.Utility.IndexedManual.#ctor(System.String[],System.String)
```
与@``T:Microsoft.VisualBasic.Terminal.Utility.ManualPages``所不同的是，本对象之中的这个字符串数组表示的是一页帮助，而不是一行帮助信息

|Parameter Name|Remarks|
|--------------|-------|
|Pages|-|


#### PrintPrompted
```csharp
Microsoft.VisualBasic.Terminal.Utility.IndexedManual.PrintPrompted(System.Int32,System.Int32,System.Int32)
```


|Parameter Name|Remarks|
|--------------|-------|
|p|Current page index|


#### ShowManual
```csharp
Microsoft.VisualBasic.Terminal.Utility.IndexedManual.ShowManual(System.Int32,System.Int32)
```
使用[Enter][Down_arrow][pagedown]翻下一页[Up_arrow][Pageup]翻上一页，[q]或者[esc]结束，[home]第一页[end]最后一页

|Parameter Name|Remarks|
|--------------|-------|
|initLines|无用的参数|
|printLines|无用的参数|



