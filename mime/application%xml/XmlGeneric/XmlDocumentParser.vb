#Region "Microsoft.VisualBasic::0792e251d133ca3a59141550bbafa5e4, mime\application%xml\XmlGeneric\XmlDocumentParser.vb"

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

    '   Total Lines: 61
    '    Code Lines: 46 (75.41%)
    ' Comment Lines: 5 (8.20%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (16.39%)
    '     File Size: 2.11 KB


    ' Module XmlParser
    ' 
    '     Function: (+2 Overloads) ParseXml, PopulateChilds
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Xml

Public Module XmlParser

    ''' <summary>
    ''' parse the xml document text
    ''' </summary>
    ''' <param name="xml">the xml document text</param>
    ''' <returns></returns>
    Public Function ParseXml(xml As String) As XmlElement
        Dim doc As XDocument = XDocument.Load(New StringReader(xml))
        Dim root As XElement = doc.Root

        Return ParseXml(root)
    End Function

    Private Function ParseXml(root As XElement) As XmlElement
        Dim rootElement As New XmlElement With {
            .name = root.Name.LocalName,
            .[namespace] = root.Name.Namespace.ToString
        }

        If root.HasAttributes Then
            Dim xmlns As String
            Dim attr_name As String

            rootElement.attributes = New Dictionary(Of String, String)

            For Each attr In root.Attributes
                xmlns = attr.Name.Namespace.ToString
                attr_name = If(xmlns.StringEmpty, "", xmlns & "!") & attr.Name.LocalName
                rootElement.attributes.Add(attr_name, attr.Value)
            Next
        End If

        If root.HasElements Then
            rootElement.elements = root.PopulateChilds.ToArray
        Else
            rootElement.text = root.Value
        End If

        Return rootElement
    End Function

    <Extension>
    Private Iterator Function PopulateChilds(root As XElement) As IEnumerable(Of XmlElement)
        For Each child As XNode In root.Nodes
            If child.NodeType = XmlNodeType.Text Then
                Yield New XmlElement With {.text = child.ToString}
            ElseIf child.NodeType = XmlNodeType.Element Then
                Yield ParseXml(root:=CType(child, XElement))
            ElseIf child.NodeType = XmlNodeType.Comment Then
                Yield New XmlElement With {.comment = child.ToString}
            Else
                Throw New NotImplementedException(child.NodeType.ToString)
            End If
        Next
    End Function
End Module
