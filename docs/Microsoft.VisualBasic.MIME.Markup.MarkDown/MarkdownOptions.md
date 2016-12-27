# MarkdownOptions
_namespace: [Microsoft.VisualBasic.MIME.Markup.MarkDown](./index.md)_

The markdown document generate options.




### Properties

#### AllowEmptyLinkText
when true, text link may be empty
#### AsteriskIntraWordEmphasis
when true, asterisks may be used for intraword emphasis
 this does nothing if StrictBoldItalic is false
#### AutoHyperlink
when true, (most) bare plain URLs are auto-hyperlinked 
 WARNING: this is a significant deviation from the markdown spec
#### AutoNewlines
when true, RETURN becomes a literal newline 
 WARNING: this is a significant deviation from the markdown spec
#### DisableHeaders
when true, header parser disabled
#### DisableHr
when true, hr parser disabled
#### DisableImages
when true, image parser disabled
#### EmptyElementSuffix
use ">" for HTML output, or " />" for XHTML output
#### LinkEmails
when false, email addresses will never be auto-linked 
 WARNING: this is a significant deviation from the markdown spec
#### QuoteSingleLine
when true, quote dont grab next lines
#### StrictBoldItalic
when true, bold and italic require non-word characters on either side 
 WARNING: this is a significant deviation from the markdown spec
