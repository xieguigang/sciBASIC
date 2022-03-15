#Region "Microsoft.VisualBasic::220fb2fc26c4801f852da9f89216fe3e, sciBASIC#\mime\application%xml\XmlGeneric\XmlElement.vb"

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

    '   Total Lines: 52
    '    Code Lines: 31
    ' Comment Lines: 13
    '   Blank Lines: 8
    '     File Size: 1.56 KB


    ' Class XmlElement
    ' 
    '     Properties: [namespace], attributes, elements, id, name
    '                 text
    ' 
    '     Function: getElementById, getElementsByTagName, ParseXmlText, ToString
    ' 
    ' /********************************************************************************/

#End Region

Public Class XmlElement

    ''' <summary>
    ''' the xml tag name
    ''' </summary>
    ''' <returns></returns>
    Public Property name As String
    Public Property [namespace] As String
    Public Property attributes As Dictionary(Of String, String)
    Public Property elements As XmlElement()

    ''' <summary>
    ''' the content value of the current xml node
    ''' </summary>
    ''' <returns></returns>
    Public Property text As String

    Public ReadOnly Property id As String
        Get
            Return attributes _
                .Where(Function(a) a.Key = "id") _
                .FirstOrDefault _
                .Value
        End Get
    End Property

    Public Function getElementById(id As String) As XmlElement
        Return elements.Where(Function(a) a.id = id).FirstOrDefault
    End Function

    Public Iterator Function getElementsByTagName(name As String) As IEnumerable(Of XmlElement)
        For Each element As XmlElement In elements
            If element.name = name Then
                Yield element
            End If
        Next
    End Function

    Public Overrides Function ToString() As String
        Return $"{[namespace]}::{name}"
    End Function

    ''' <summary>
    ''' parse the xml document text
    ''' </summary>
    ''' <param name="xmlText">the xml dcument text</param>
    ''' <returns></returns>
    Public Shared Function ParseXmlText(xmlText As String) As XmlElement
        Return XmlParser.ParseXml(xmlText)
    End Function

End Class
