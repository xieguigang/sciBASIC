#Region "Microsoft.VisualBasic::99f875f0df13c8cba402ca06852ec117, Microsoft.VisualBasic.Core\src\ComponentModel\File\XmlAssembly\XmlStyleProcessor.vb"

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

    '   Total Lines: 55
    '    Code Lines: 47 (85.45%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (14.55%)
    '     File Size: 2.14 KB


    '     Class XmlStyleProcessor
    ' 
    '         Properties: alternate, href, media, title, type
    ' 
    '         Function: getAttributes, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    Public Class XmlStyleProcessor

        Public Property href As String
        Public Property alternate As Boolean
        Public Property title As String
        Public Property media As String

        Public ReadOnly Property type As String
            Get
                Select Case href.ExtensionSuffix.ToLower
                    Case "css" : Return "text/css"
                    Case "xsl" : Return ""
                    Case Else
                        Throw New InvalidDataException("Unknown file type!")
                End Select
            End Get
        End Property

        Private Iterator Function getAttributes() As IEnumerable(Of NamedValue(Of String))
            If href.StringEmpty Then
                Throw New EntryPointNotFoundException("No style file was specific!")
            Else
                Yield New NamedValue(Of String) With {.Name = NameOf(href), .Value = href}
            End If
            If Not title.StringEmpty Then
                Yield New NamedValue(Of String) With {.Name = NameOf(title), .Value = title}
            End If
            If Not media.StringEmpty Then
                Yield New NamedValue(Of String) With {.Name = NameOf(media), .Value = media}
            End If

            Yield New NamedValue(Of String) With {.Name = NameOf(type), .Value = type}
            Yield New NamedValue(Of String) With {
                    .Name = NameOf(alternate),
                    .Value = "no" Or "yes".When(alternate)
                }
        End Function

        Public Overrides Function ToString() As String
            Dim attrs As NamedValue(Of String)() = getAttributes.ToArray
            Dim attrVals$() = attrs _
                    .Select(Function(a) $"{a.Name}=""{a.Value}""") _
                    .ToArray
            Dim declares$ = $"<?xml-stylesheet {attrVals.JoinBy(" ")} ?>"

            Return declares
        End Function
    End Class
End Namespace
