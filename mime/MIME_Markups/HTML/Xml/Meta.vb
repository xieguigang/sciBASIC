#Region "Microsoft.VisualBasic::babcfbeec82ffc780cf5c167eccbf06e, ..\sciBASIC#\mime\MIME_Markups\HTML\Xml\Meta.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace HTML.XmlMeta

    ''' <summary>
    ''' Html之中的CSS样式
    ''' </summary>
    Public Class CSS

        ''' <summary>
        ''' ``text/css``
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property type As String
            Get
                Return "text/css"
            End Get
            Set(value As String)
                ' ReadOnly, Do Nothing
            End Set
        End Property

        <XmlAttribute> Public Property id As String
        ''' <summary>
        ''' 具体的CSS内容
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property style As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function Generator(classId As IEnumerable(Of String), attrs As Dictionary(Of String, String)) As String
            Dim sb As New StringBuilder(String.Join(", ", classId.ToArray(Function(s) "." & s)))

            Call sb.AppendLine("{")
            For Each attr In attrs
                Call sb.AppendLine($"   {attr.Key}: {attr.Value};")
            Next
            Call sb.AppendLine("}")

            Return sb.ToString
        End Function
    End Class
End Namespace
