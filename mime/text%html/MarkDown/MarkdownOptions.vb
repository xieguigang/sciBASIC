#Region "Microsoft.VisualBasic::85ef0d4441bb38829f676c101e0e1c0d, mime\text%html\MarkDown\MarkdownOptions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class MarkdownOptions
    ' 
    '         Properties: AllowEmptyLinkText, AsteriskIntraWordEmphasis, AutoHyperlink, AutoNewlines, DisableHeaders
    '                     DisableHr, DisableImages, EmptyElementSuffix, LinkEmails, QuoteSingleLine
    '                     StrictBoldItalic
    ' 
    '         Function: DefaultOption, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MarkDown

    ''' <summary>
    ''' The markdown document generate options.
    ''' </summary>
    Public Class MarkdownOptions

        ''' <summary>
        ''' when true, text link may be empty
        ''' </summary>
        Public Property AllowEmptyLinkText() As Boolean

        ''' <summary>
        ''' when true, hr parser disabled
        ''' </summary>
        Public Property DisableHr() As Boolean

        ''' <summary>
        ''' when true, header parser disabled
        ''' </summary>
        Public Property DisableHeaders() As Boolean

        ''' <summary>
        ''' when true, image parser disabled
        ''' </summary>
        Public Property DisableImages() As Boolean

        ''' <summary>
        ''' when true, quote dont grab next lines
        ''' </summary>
        Public Property QuoteSingleLine() As Boolean

        ''' <summary>
        ''' when true, (most) bare plain URLs are auto-hyperlinked  
        ''' WARNING: this is a significant deviation from the markdown spec
        ''' </summary>
        Public Property AutoHyperlink() As Boolean

        ''' <summary>
        ''' when true, RETURN becomes a literal newline  
        ''' WARNING: this is a significant deviation from the markdown spec
        ''' </summary>
        Public Property AutoNewlines() As Boolean

        ''' <summary>
        ''' use ">" for HTML output, or " />" for XHTML output
        ''' </summary>
        Public Property EmptyElementSuffix() As String

        ''' <summary>
        ''' when false, email addresses will never be auto-linked  
        ''' WARNING: this is a significant deviation from the markdown spec
        ''' </summary>
        Public Property LinkEmails() As Boolean

        ''' <summary>
        ''' when true, bold and italic require non-word characters on either side  
        ''' WARNING: this is a significant deviation from the markdown spec
        ''' </summary>
        Public Property StrictBoldItalic() As Boolean

        ''' <summary>
        ''' when true, asterisks may be used for intraword emphasis
        ''' this does nothing if StrictBoldItalic is false
        ''' </summary>
        Public Property AsteriskIntraWordEmphasis() As Boolean

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function DefaultOption() As [Default](Of  MarkdownOptions)
            Return New MarkdownOptions With {
                .AllowEmptyLinkText = True,
                .AutoHyperlink = True,
                .DisableHr = False,
                .AutoNewlines = True,
                .StrictBoldItalic = True,
                .DisableImages = False
            }
        End Function
    End Class
End Namespace
