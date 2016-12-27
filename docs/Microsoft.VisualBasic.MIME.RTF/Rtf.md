# Rtf
_namespace: [Microsoft.VisualBasic.MIME.RTF](./index.md)_

Rich text format document object model.(带有格式描述信息的文本文档的对象模型)



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.#ctor(System.String,System.Int32,System.Drawing.Color)
```


|Parameter Name|Remarks|
|--------------|-------|
|FontName|Font family name.(字体名称)|
|Size|Font size.(字体大小)|
|Color|Font Color.(字体颜色)|


#### __getMetaDataStr
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.__getMetaDataStr
```

> 
>  {
>    \rtf1\ansi\ansicpg936\deff0\deflang1033\deflangfe2052
>    {
>      \fonttbl
>      {
>         \f0\fnil\fcharset0 Cambria;
>      }
>    }
>    {
>      \colortbl ;%cl_meta%;
>    }
>    {
>      \*\generator Msftedit 5.41.21.2510;
>    }
>  

#### __toRTF
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.__toRTF
```
生成一个含有格式描述的文本文件，即将模型数据保存为rtf文档

#### AppendLine
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.AppendLine(System.String,Microsoft.VisualBasic.MIME.RTF.Font)
```
向文档末尾追加一行带有格式标记的文本

|Parameter Name|Remarks|
|--------------|-------|
|text|-|
|Format|-|


#### AppendText
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.AppendText(System.String,Microsoft.VisualBasic.MIME.RTF.Font)
```
Append the target text value on to the last region of the document with the specific text format.(向文档末尾追加一段带有格式标记的文本)

|Parameter Name|Remarks|
|--------------|-------|
|text|-|
|Format|-|


#### SetFormat
```csharp
Microsoft.VisualBasic.MIME.RTF.Rtf.SetFormat(System.Int32,System.Int32,Microsoft.VisualBasic.MIME.RTF.Font)
```
Set format value to a selected region in the text document. the previous format which was exists on the target 
 document region will be covered by the newly format **`FontStyle`**.
 (先前的格式会被后面设置的格式所覆盖)

|Parameter Name|Remarks|
|--------------|-------|
|start|The region start location.|
|selectLength|The formated region text length.|
|FontStyle|The format will be applied on the target selected region.|

> 
>  目标区域**`start`** -> **`selectLength`**可能与某一个设置了字体的区域重合一部分，也可能完全包含有其他的多个区域
>  


### Properties

#### FontToken
``{\f1\fnil\fcharset0 %font.name%;}``
