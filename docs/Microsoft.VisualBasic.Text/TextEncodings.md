# TextEncodings
_namespace: [Microsoft.VisualBasic.Text](./index.md)_





### Methods

#### Assertion
```csharp
Microsoft.VisualBasic.Text.TextEncodings.Assertion(System.Text.Encoding)
```
Default value or user specific?

|Parameter Name|Remarks|
|--------------|-------|
|encoding|-|


#### GetEncodings
```csharp
Microsoft.VisualBasic.Text.TextEncodings.GetEncodings(Microsoft.VisualBasic.Text.Encodings)
```
Get text file save encodings instance

|Parameter Name|Remarks|
|--------------|-------|
|value|-|


#### ParseEncodingsName
```csharp
Microsoft.VisualBasic.Text.TextEncodings.ParseEncodingsName(System.String,Microsoft.VisualBasic.Text.Encodings)
```
从字符串名称之中解析出编码格式的枚举值

|Parameter Name|Remarks|
|--------------|-------|
|encoding$|-|
|onFailure|-|


#### TransEncoding
```csharp
Microsoft.VisualBasic.Text.TextEncodings.TransEncoding(System.String,Microsoft.VisualBasic.Text.Encodings,System.Text.Encoding)
```
有时候有些软件对文本的编码是有要求的，则可以使用这个函数进行文本编码的转换
 例如R程序默认是读取ASCII，而。NET的默认编码是UTF8，则可以使用这个函数将目标文本文件转换为ASCII编码的文本文件

|Parameter Name|Remarks|
|--------------|-------|
|path|-|
|encoding|-|
|from|-|



