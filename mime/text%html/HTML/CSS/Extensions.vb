#Region "Microsoft.VisualBasic::dec09f7ac5c73e4de15d22c8bfd657cf, mime\text%html\HTML\CSS\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: CSSInlineStyle, CSSValue, GetTagValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace HTML.CSS

    Public Module Extensions

        ReadOnly tags As Dictionary(Of String, HtmlTags) =
            Enums(Of HtmlTags)() _
            .ToDictionary(Function(t) t.Description.ToLower)

        <Extension>
        Public Function GetTagValue(str As String) As HtmlTags
            With Strings.Trim(str).ToLower
                If tags.ContainsKey(.ByRef) Then
                    Return tags(.ByRef)
                Else
                    Return HtmlTags.NA
                End If
            End With
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CSSValue(style As FontStyle) As String
            Return CSSFont.ToString(style)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CSSInlineStyle(font As CSSFont) As String
            Return $"{font.style.CSSValue} {font.size}px {font.family}"
        End Function
    End Module
End Namespace
