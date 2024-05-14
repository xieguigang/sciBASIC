#Region "Microsoft.VisualBasic::ee0d593f3ff828a35ed2958645d2ffe6, mime\application%rdf+xml\Turtle\ttl_property.vb"

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

    '   Total Lines: 49
    '    Code Lines: 37
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.56 KB


    '     Class ttl_property
    ' 
    '         Properties: subject, value
    ' 
    '         Function: LoadTuples, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Text

Namespace Turtle

    ''' <summary>
    ''' A simple key-value pair tuple data
    ''' </summary>
    Public Class ttl_property

        <XmlAttribute>
        Public Property subject As String
        <XmlText>
        Public Property value As String

        Public Overrides Function ToString() As String
            Return $"{subject}: {value}"
        End Function

        Public Shared Iterator Function LoadTuples(file As Stream) As IEnumerable(Of ttl_property)
            Dim line As Value(Of String) = ""
            Dim reader As New StreamReader(file)
            Dim data As NamedValue(Of String)
            Dim subj As String
            Dim value As String

            Do While (line = reader.ReadLine) IsNot Nothing
                If line.Value.Length = 0 OrElse line.StartsWith("@prefix") Then
                    Continue Do
                End If

                data = line.GetTagValue(vbTab, trim:=False)
                subj = data.Name
                data = data.Value.GetTagValue(vbTab)
                value = data.Value.Trim("."c, " "c, ASCII.TAB)

                Yield New ttl_property With {
                    .subject = subj,
                    .value = value
                }
            Loop
        End Function

    End Class
End Namespace
