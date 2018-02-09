#Region "Microsoft.VisualBasic::278270daee8d57d9e2d94f54d4b29866, ..\sciBASIC#\mime\text%html\HTML\CSS\Parser\Enums\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

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
    End Module
End Namespace
