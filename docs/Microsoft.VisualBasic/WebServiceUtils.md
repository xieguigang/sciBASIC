# WebServiceUtils
_namespace: [Microsoft.VisualBasic](./index.md)_

The extension module for web services works.



### Methods

#### BuildReqparm
```csharp
Microsoft.VisualBasic.WebServiceUtils.BuildReqparm(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})
```
Build the request parameters for the HTTP POST

|Parameter Name|Remarks|
|--------------|-------|
|data|-|


#### BuildUrlData
```csharp
Microsoft.VisualBasic.WebServiceUtils.BuildUrlData(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}},System.Boolean)
```
生成URL请求的参数

|Parameter Name|Remarks|
|--------------|-------|
|data|-|
|escaping|是否进行对value部分的字符串数据进行转义|


#### CopyStream
```csharp
Microsoft.VisualBasic.WebServiceUtils.CopyStream(System.IO.Stream)
```
Download stream data from the http response.

|Parameter Name|Remarks|
|--------------|-------|
|stream|
 Create from @``M:Microsoft.VisualBasic.WebServiceUtils.GetRequestRaw(System.String,System.Boolean,System.String)``
 |


#### DownloadFile
```csharp
Microsoft.VisualBasic.WebServiceUtils.DownloadFile(System.String,System.String,System.String,System.String)
```
download the file from **`strUrl`** to ``save``[local file].

|Parameter Name|Remarks|
|--------------|-------|
|strUrl|-|
|save|The file path of the file saved|


#### GenerateDictionary
```csharp
Microsoft.VisualBasic.WebServiceUtils.GenerateDictionary(System.String[],System.Boolean)
```
Create a parameter dictionary from the request parameter tokens.
 (请注意，字典的key默认为转换为小写的形式)

|Parameter Name|Remarks|
|--------------|-------|
|tokens|
 元素的个数必须要大于1，因为从url里面解析出来的元素之中第一个元素是url本身，则不再对url做字典解析
 |


_returns: 
 ###### 2016-11-21
 因为post可能会传递数组数据进来，则这个时候就会出现重复的键名，则已经不再适合字典类型了，这里改为返回@``T:System.Collections.Specialized.NameValueCollection``
 _

#### GET
```csharp
Microsoft.VisualBasic.WebServiceUtils.GET(System.String,System.UInt16,System.Boolean,System.Collections.Generic.Dictionary{System.String,System.String},System.String,System.Boolean,System.String)
```
Get the html page content from a website request or a html file on the local filesystem.(同时支持http位置或者本地文件，失败或者错误会返回空字符串)

|Parameter Name|Remarks|
|--------------|-------|
|url|web http request url or a file path handle|
|retry|发生错误的时候的重试的次数|


_returns: 失败或者错误会返回空字符串_

#### GetDownload
```csharp
Microsoft.VisualBasic.WebServiceUtils.GetDownload(System.String,System.String)
```
使用GET方法下载文件

|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|savePath|-|


#### GetMyIPAddress
```csharp
Microsoft.VisualBasic.WebServiceUtils.GetMyIPAddress
```
获取我的公网IP地址，假若没有连接互联网的话则会返回局域网IP地址

#### GetRequest
```csharp
Microsoft.VisualBasic.WebServiceUtils.GetRequest(System.String,System.Boolean,System.String)
```
GET http request

|Parameter Name|Remarks|
|--------------|-------|
|url|-|


#### GetRequestRaw
```csharp
Microsoft.VisualBasic.WebServiceUtils.GetRequestRaw(System.String,System.Boolean,System.String)
```


|Parameter Name|Remarks|
|--------------|-------|
|url|-|
|https|-|
|userAgent|
 
 fix a bug for github API:
 
 Protocol violation using Github api
 
 You need to set UserAgent like this:
 webRequest.UserAgent = "YourAppName"
 Otherwise it will give The server committed a protocol violation. Section=ResponseStatusLine Error.
 |


#### GetValue
```csharp
Microsoft.VisualBasic.WebServiceUtils.GetValue(System.String)
```
获取两个尖括号之间的内容

|Parameter Name|Remarks|
|--------------|-------|
|html|-|


#### href
```csharp
Microsoft.VisualBasic.WebServiceUtils.href(System.String)
```
Gets the link text in the html fragement text.

|Parameter Name|Remarks|
|--------------|-------|
|html|A string that contains the url string pattern like: href="url_text"|


#### ImageSource
```csharp
Microsoft.VisualBasic.WebServiceUtils.ImageSource(System.String)
```
Parsing image source url from the img html tag.

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### IsSocketPortOccupied
```csharp
Microsoft.VisualBasic.WebServiceUtils.IsSocketPortOccupied(System.Exception)
```
Only one usage of each socket address (protocol/network address/port) Is normally permitted

|Parameter Name|Remarks|
|--------------|-------|
|ex|-|


#### isURL
```csharp
Microsoft.VisualBasic.WebServiceUtils.isURL(System.String)
```
Determine that is this uri string is a network location?
 (判断这个uri字符串是否是一个网络位置)

|Parameter Name|Remarks|
|--------------|-------|
|url|-|


#### PostRequest
```csharp
Microsoft.VisualBasic.WebServiceUtils.PostRequest(System.String,System.Collections.Generic.Dictionary{System.String,System.String[]},System.String,System.String,System.String)
```
POST http request for get html

|Parameter Name|Remarks|
|--------------|-------|
|url$|-|
|data|-|
|Referer$|-|


#### PostUrlDataParser
```csharp
Microsoft.VisualBasic.WebServiceUtils.PostUrlDataParser(System.String,System.Boolean)
```
假若你的数据之中包含有SHA256的加密数据，则非常不推荐使用这个函数进行解析。因为请注意，这个函数会替换掉一些转义字符的，所以会造成一些非常隐蔽的BUG

|Parameter Name|Remarks|
|--------------|-------|
|data|转义的时候大小写无关|


#### RequestParser
```csharp
Microsoft.VisualBasic.WebServiceUtils.RequestParser(System.String,System.Boolean)
```
不像@``M:Microsoft.VisualBasic.WebServiceUtils.PostUrlDataParser(System.String,System.Boolean)``函数，这个函数不会替换掉转义字符，并且所有的Key都已经被默认转换为小写形式的了

|Parameter Name|Remarks|
|--------------|-------|
|argsData|URL parameters|


#### TrimHTMLTag
```csharp
Microsoft.VisualBasic.WebServiceUtils.TrimHTMLTag(System.String)
```
Removes the html tags from the text string.

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### TrimResponseTail
```csharp
Microsoft.VisualBasic.WebServiceUtils.TrimResponseTail(System.String)
```
有些时候后面可能会存在多余的vbCrLf，则使用这个函数去除

|Parameter Name|Remarks|
|--------------|-------|
|value|-|


#### UrlDecode
```csharp
Microsoft.VisualBasic.WebServiceUtils.UrlDecode(System.String,System.Text.Encoding)
```
在服务器端对URL进行解码还原

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|encoding|-|


#### UrlEncode
```csharp
Microsoft.VisualBasic.WebServiceUtils.UrlEncode(System.String,System.Text.Encoding)
```
进行url编码，将特殊字符进行转码

|Parameter Name|Remarks|
|--------------|-------|
|s|-|
|encoding|-|


#### UrlPathEncode
```csharp
Microsoft.VisualBasic.WebServiceUtils.UrlPathEncode(System.String)
```
编码整个URL

|Parameter Name|Remarks|
|--------------|-------|
|s|-|



### Properties

#### MicrosoftDNS
Microsoft DNS Server
#### Protocols
Web protocols enumeration
