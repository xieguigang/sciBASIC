# HtmlStrips
_namespace: [Microsoft.VisualBasic.Text.HtmlParser](./index.md)_





### Methods

#### GetValue
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.GetValue(System.String)
```
获取两个尖括号之间的内容

|Parameter Name|Remarks|
|--------------|-------|
|html|-|


#### href
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.href(System.String)
```
Gets the link text in the html fragement text.

|Parameter Name|Remarks|
|--------------|-------|
|html|A string that contains the url string pattern like: href="url_text"|


#### HTMLTitle
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.HTMLTitle(System.String)
```
Parsing the title text from the html inputs.

|Parameter Name|Remarks|
|--------------|-------|
|html|-|


#### ImageSource
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.ImageSource(System.String)
```
Parsing image source url from the img html tag.

|Parameter Name|Remarks|
|--------------|-------|
|str|-|


#### StripHTMLTags
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.StripHTMLTags(System.String)
```
Removes the html tags from the text string.

|Parameter Name|Remarks|
|--------------|-------|
|s|-|


#### TrimResponseTail
```csharp
Microsoft.VisualBasic.Text.HtmlParser.HtmlStrips.TrimResponseTail(System.String)
```
有些时候后面可能会存在多余的vbCrLf，则使用这个函数去除

|Parameter Name|Remarks|
|--------------|-------|
|value|-|



