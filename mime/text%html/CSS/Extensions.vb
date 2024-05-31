#Region "Microsoft.VisualBasic::bb2357679c0af15ab490d9325e2efd09, mime\text%html\CSS\Extensions.vb"

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

    '   Total Lines: 38
    '    Code Lines: 32 (84.21%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (15.79%)
    '     File Size: 1.23 KB


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
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Namespace CSS

    <HideModuleName>
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
