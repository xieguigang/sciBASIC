# MarkdownHTML
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_

Markdown is a text-to-HTML conversion tool for web writers. 
 Markdown allows you to write using an easy-to-read, easy-to-write plain text format, 
 then convert it to structurally valid XHTML (or HTML).



### Methods

#### #ctor
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.#ctor(Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownOptions)
```
Create a new Markdown instance and set the options from the MarkdownOptions object.

#### AddExtension
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.AddExtension(Microsoft.VisualBasic.MIME.Markup.MarkDown.ExtensionTransform)
```
Public @``T:System.Delegate`` Function ExtensionTransform(text As @``T:System.String``) As @``T:System.String``

|Parameter Name|Remarks|
|--------------|-------|
|ext|-|


#### DoAnchors
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoAnchors(System.String)
```
Turn Markdown link shortcuts into HTML anchor tags
> 
>  [link text](url "title") 
>  [link text][id] 
>  [id] 
>  

#### DoAutoLinks
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoAutoLinks(System.String)
```
Turn angle-delimited URLs into HTML anchor tags
> 
>  <http://www.example.com>
>  

#### DoBlockQuotes
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoBlockQuotes(System.String)
```
Turn Markdown > quoted blocks into HTML blockquote blocks

#### DoCodeBlocks
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoCodeBlocks(System.String)
```
/// Turn Markdown 4-space indented code into HTML pre code blocks

#### DoCodeSpans
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoCodeSpans(System.String)
```
Turn Markdown `code spans` into HTML code tags

#### DoHardBreaks
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoHardBreaks(System.String)
```
Turn markdown line breaks (two space at end of line) into HTML break tags

#### DoHeaders
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoHeaders(System.String)
```
Turn Markdown headers into HTML header tags
> 
>  Header 1  
>  ========  
>  
>  Header 2  
>  --------  
>  
>  # Header 1  
>  ## Header 2  
>  ## Header 2 with closing hashes ##  
>  ...  
>  ###### Header 6  
>  

#### DoHorizontalRules
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoHorizontalRules(System.String)
```
Turn Markdown horizontal rules into HTML hr tags
> 
>  ***  
>  * * *  
>  ---
>  - - -
>  

#### DoImages
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoImages(System.String)
```
Turn Markdown image shortcuts into HTML img tags.
> 
>  ![alt text][id]
>  ![alt text](url "optional title")
>  

#### DoItalicsAndBold
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoItalicsAndBold(System.String)
```
Turn Markdown *italics* and **bold** into HTML strong and em tags

#### DoLists
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.DoLists(System.String)
```
Turn Markdown lists into HTML ul and ol and li tags

#### EncodeAmpsAndAngles
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EncodeAmpsAndAngles(System.String)
```
Encode any ampersands (that aren't part of an HTML entity) and left or right angle brackets

#### EncodeCode
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EncodeCode(System.String)
```
Encode/escape certain Markdown characters inside code blocks and spans where they are literals

#### EncodeEmailAddress
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EncodeEmailAddress(System.String)
```
encodes email address randomly 
 roughly 10% raw, 45% hex, 45% dec 
 note that @ is always encoded and : never is

#### EscapeBackslashes
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EscapeBackslashes(System.String)
```
Encodes any escaped characters such as \`, \*, \[ etc

#### EscapeBoldItalic
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EscapeBoldItalic(System.String)
```
escapes Bold [ * ] and Italic [ _ ] characters

#### EscapeSpecialCharsWithinTagAttributes
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.EscapeSpecialCharsWithinTagAttributes(System.String)
```
Within tags -- meaning between < and > -- encode [\ ` * _] so they 
 don't conflict with their use in Markdown for code, italics and strong. 
 We're replacing each such character with its corresponding hash 
 value; this is likely overkill, but it should prevent us from colliding 
 with the escape values by accident.

#### FormParagraphs
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.FormParagraphs(System.String,System.Boolean,System.Boolean)
```
splits on two or more newlines, to form "paragraphs"; 
 each paragraph is then unhashed (if it is a hash and unhashing isn't turned off) or wrapped in HTML p tag

#### GetBlockPattern
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.GetBlockPattern
```
derived pretty much verbatim from PHP Markdown

#### GetNestedBracketsPattern
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.GetNestedBracketsPattern
```
Reusable pattern to match balanced [brackets]. See Friedl's 
 "Mastering Regular Expressions", 2nd Ed., pp. 328-331.

#### GetNestedParensPattern
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.GetNestedParensPattern
```
Reusable pattern to match balanced (parens). See Friedl's 
 "Mastering Regular Expressions", 2nd Ed., pp. 328-331.

#### HashHTMLBlocks
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.HashHTMLBlocks(System.String)
```
replaces any block-level HTML blocks with hash entries

#### ProcessListItems
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.ProcessListItems(System.String,System.String)
```
Process the contents of a single ordered or unordered list, splitting it
 into individual list items.

#### RunBlockGamut
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.RunBlockGamut(System.String,System.Boolean,System.Boolean)
```
Perform transformations that form block-level tags like paragraphs, headers, and list items.

#### RunSpanGamut
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.RunSpanGamut(System.String)
```
Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.

#### StripLinkDefinitions
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.StripLinkDefinitions(System.String)
```
Strips link definitions from text, stores the URLs and titles in hash references.
> 
>  ^[id]: url "optional title"
>  

#### Transform
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.Transform(System.String)
```
Transforms the provided Markdown-formatted text to HTML; 
 see http://en.wikipedia.org/wiki/Markdown
 (好像这个并不支持代码高亮的格式化)
> 
>  The order in which other subs are called here is
>  essential. Link and image substitutions need to happen before
>  EscapeSpecialChars(), so that any *'s or _'s in the a
>  and img tags get encoded.
>  

#### Unescape
```csharp
Microsoft.VisualBasic.MIME.Markup.MarkDown.MarkdownHTML.Unescape(System.String)
```
swap back in all the special characters we've hidden


### Properties

#### AsteriskIntraWordEmphasis
when true, asterisks may be used for intraword emphasis
 this does nothing if StrictBoldItalic is false
#### AutoHyperlink
when true, (most) bare plain URLs are auto-hyperlinked 
 WARNING: this is a significant deviation from the markdown spec
#### AutoNewLines
when true, RETURN becomes a literal newline 
 WARNING: this is a significant deviation from the markdown spec
#### DisableHeaders
Disable header parser
#### DisableHr
Disable hr parser
#### DisableImages
Disable image parser
#### EmptyElementSuffix
use ">" for HTML output, or " />" for XHTML output
#### LinkEmails
when false, email addresses will never be auto-linked 
 WARNING: this is a significant deviation from the markdown spec
#### QuoteSingleLine
Don't grab next lines
#### StrictBoldItalic
when true, bold and italic require non-word characters on either side 
 WARNING: this is a significant deviation from the markdown spec
