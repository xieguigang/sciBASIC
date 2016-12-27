# XmlExtensions
_namespace: [Microsoft.VisualBasic](./index.md)_





### Methods

#### CreateObjectFromXmlFragment``1
```csharp
Microsoft.VisualBasic.XmlExtensions.CreateObjectFromXmlFragment``1(System.String)
```
使用一个XML文本内容的一个片段创建一个XML映射对象

|Parameter Name|Remarks|
|--------------|-------|
|Xml|是Xml文件的文件内容而非文件路径|


#### GetXml``1
```csharp
Microsoft.VisualBasic.XmlExtensions.GetXml``1(``0,System.Boolean,Microsoft.VisualBasic.Text.Xml.XmlEncodings)
```
Serialization the target object type into a XML document.(将一个类对象序列化为XML文档)

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|


#### LoadFromXml``1
```csharp
Microsoft.VisualBasic.XmlExtensions.LoadFromXml``1(System.String,System.Boolean)
```
Generate a specific type object from a xml document stream.(使用一个XML文本内容创建一个XML映射对象)

|Parameter Name|Remarks|
|--------------|-------|
|Xml|This parameter value is the document text of the xml file, not the file path of the xml file.(是Xml文件的文件内容而非文件路径)|
|throwEx|Should this program throw the exception when the xml deserialization error happens?
 if False then this function will returns a null value instead of throw exception.
 (在进行Xml反序列化的时候是否抛出错误，默认抛出错误，否则返回一个空对象)|


#### LoadXml
```csharp
Microsoft.VisualBasic.XmlExtensions.LoadXml(System.String,System.Type,System.Text.Encoding,System.Boolean,System.Func{System.String,System.String})
```
从文件之中加载XML之中的数据至一个对象类型之中

|Parameter Name|Remarks|
|--------------|-------|
|XmlFile|XML文件的文件路径|
|ThrowEx|当反序列化出错的时候是否抛出错误？假若不抛出错误，则会返回空值|
|preprocess|Xml文件的预处理操作|


#### LoadXml``1
```csharp
Microsoft.VisualBasic.XmlExtensions.LoadXml``1(System.String,System.Text.Encoding,System.Boolean,System.Func{System.String,System.String})
```
Load class object from the exists Xml document.(从文件之中加载XML之中的数据至一个对象类型之中)

|Parameter Name|Remarks|
|--------------|-------|
|XmlFile|The path of the xml document.(XML文件的文件路径)|
|ThrowEx|
 If the deserialization operation have throw a exception, then this function should process this error automatically or just throw it?
 (当反序列化出错的时候是否抛出错误？假若不抛出错误，则会返回空值)
 |
|preprocess|
 The preprocessing on the xml document text, you can doing the text replacement or some trim operation from here.(Xml文件的预处理操作)
 |


#### SafeLoadXml``1
```csharp
Microsoft.VisualBasic.XmlExtensions.SafeLoadXml``1(System.String,Microsoft.VisualBasic.Text.Encodings,System.Func{System.String,System.String})
```
这个函数主要是用作于Linq里面的Select语句拓展的，这个函数永远也不会报错，只会返回空值

#### SaveAsXml``1
```csharp
Microsoft.VisualBasic.XmlExtensions.SaveAsXml``1(``0,System.String,System.Boolean,System.Text.Encoding,System.String)
```
Save the object as the XML document.

|Parameter Name|Remarks|
|--------------|-------|
|obj|-|
|saveXml|-|
|throwEx|-|
|encoding|-|



