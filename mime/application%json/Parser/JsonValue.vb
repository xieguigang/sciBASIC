#Region "Microsoft.VisualBasic::217569b4a407d4b61239094337511c86, ..\sciBASIC#\mime\application%json\Parser\JsonValue.vb"

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

Namespace Parser

    ''' <summary>
    ''' Primitive value.
    ''' (请注意，假若是字符串的话，值是未经过处理的原始字符串，可能会含有转义字符，
    ''' 则这个时候还需要使用<see cref="GetStripString"/>得到最终的字符串)
    ''' </summary>
    Public Class JsonValue : Inherits JsonElement

        Public Overloads Property Value As Object

        Public Sub New()
        End Sub

        Public Sub New(obj As Object)
            Value = obj
        End Sub

        ''' <summary>
        ''' 处理转义等特殊字符串
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStripString() As String
            Dim s$ = Scripting _
                .ToString(Value, "null") _
                .GetString
            s = JsonParser.StripString(s)
            Return s
        End Function

        Public Overrides Function BuildJsonString() As String
            Return Scripting.ToString(Value, "null")
        End Function

        Public Overrides Function ToString() As String
            Return GetStripString()
        End Function
    End Class
End Namespace
