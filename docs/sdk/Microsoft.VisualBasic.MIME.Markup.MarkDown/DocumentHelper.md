# DocumentHelper
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_





### Methods

#### Normalize
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.DocumentHelper.Normalize(System.String)
```
convert all tabs to _tabWidth spaces; 
 standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
 makes sure text ends with a couple of newlines; 
 removes any blank lines (only spaces) in the text

#### TokenizeHTML
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.DocumentHelper.TokenizeHTML(System.String)
```
returns an array of HTML tokens comprising the input string. Each token is 
 either a tag (possibly with nested, tags contained therein, such 
 as <a href="<MTFoo>">, or a run of text between tags. Each element of the 
 array is a two-element array; the first is either 'tag' or 'text'; the second is 
 the actual value.


### Properties

#### _nestDepth
maximum nested depth of [] and () supported by the transform; implementation detail
