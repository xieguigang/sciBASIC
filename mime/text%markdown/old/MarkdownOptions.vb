#Region "Microsoft.VisualBasic::701f1dceb57a3ac1185081181915fe42, sciBASIC#\mime\text%markdown\MarkdownOptions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 83
    '    Code Lines: 28
    ' Comment Lines: 41
    '   Blank Lines: 14
    '     File Size: 2.64 KB


    ' Class MarkdownOptions
    ' 
    '     Properties: AllowEmptyLinkText, AsteriskIntraWordEmphasis, AutoHyperlink, AutoNewlines, DisableHeaders
    '                 DisableHr, DisableImages, EmptyElementSuffix, LinkEmails, QuoteSingleLine
    '                 StrictBoldItalic
    ' 
    '     Function: DefaultOption, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Serialization.JSON

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

    Public Shared Function DefaultOption() As [Default](Of MarkdownOptions)
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
