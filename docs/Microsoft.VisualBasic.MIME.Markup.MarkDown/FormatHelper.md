# FormatHelper
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_





### Methods

#### #cctor
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.FormatHelper.#cctor
```
In the static constuctor we'll initialize what stays the same across all transforms.

#### handleTrailingParens
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.FormatHelper.handleTrailingParens(System.Text.RegularExpressions.Match)
```
The first group is essentially a negative lookbehind -- if there's a < or a =", we don't touch this.
 We're not using a *real* lookbehind, because of links with in links, like 
 <a href="http://web.archive.org/web/20121130000728/http://www.google.com/">
 With a real lookbehind, the full link would never be matched, and thus the http://www.google.com *would* be matched.
 With the simulated lookbehind, the full link *is* matched (just not handled, because of this early return), causing
 the google link to not be matched again.

|Parameter Name|Remarks|
|--------------|-------|
|match|-|


#### Outdent
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.FormatHelper.Outdent(System.String)
```
Remove one level of line-leading spaces


### Properties

#### _tabWidth
Tabs are automatically converted to spaces as part of the transform 
 this constant determines how "wide" those tabs become in spaces
