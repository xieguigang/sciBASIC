# CSSFont
_namespace: [Microsoft.VisualBasic.MIME.Markup.HTML.CSS](./index.md)_

```CSS
 font-style: style; font-size: size; font-family: Name;
 ```
 
 这个简写属性用于一次设置元素字体的两个或更多方面。使用 ``icon`` 等关键字可以适当地设置元素的字体，使之与用户计算机环境中的某个方面一致。
 注意，如果没有使用这些关键词，至少要指定字体大小和字体系列。
 可以按顺序设置如下属性：

 + font-style
 + font-variant
 + font-weight
 + font-size/line-height
 + font-family

 可以不设置其中的某个值，比如 ``font:100% verdana;`` 也是允许的。未设置的属性会使用其默认值。



### Methods

#### TryParse
```csharp
Microsoft.VisualBasic.MIME.Markup.HTML.CSS.CSSFont.TryParse(System.String,Microsoft.VisualBasic.MIME.Markup.HTML.CSS.CSSFont)
```
Parsing font style data from the css expression string.

|Parameter Name|Remarks|
|--------------|-------|
|css|-|
|[default]|On failure return this default value|



### Properties

#### family
A string representation of the System.Drawing.FontFamily for the new System.Drawing.Font.
#### GDIObject
Initializes a new @``T:System.Drawing.Font`` using a specified size and style.
#### style
The System.Drawing.FontStyle of the new font.
