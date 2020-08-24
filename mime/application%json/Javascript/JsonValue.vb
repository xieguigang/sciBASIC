#Region "Microsoft.VisualBasic::7b49e9b86bd42020c163758950923f56, mime\application%json\Javascript\JsonValue.vb"

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

    '     Class JsonValue
    ' 
    '         Properties: BSONValue, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: BuildJsonString, GetStripString, Literal, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Javascript

    ''' <summary>
    ''' The primitive value type in javascript.
    ''' 
    ''' (请注意，假若是字符串的话，值是未经过处理的原始字符串，可能会含有转义字符，
    ''' 则这个时候还需要使用<see cref="GetStripString"/>得到最终的字符串)
    ''' </summary>
    Public Class JsonValue : Inherits JsonElement

        Public Overloads Property value As Object

        Public ReadOnly Property BSONValue As BSONValue
            Get
                Return BSONValue.FromValue(value)
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub New(obj As Object)
            value = obj
        End Sub

        Public Function Literal(typeOfT As Type) As Object
            Select Case typeOfT
                Case GetType(String)
                    Return GetStripString()
                Case GetType(Date)
                    Return Casting.CastDate(GetStripString)
                Case Else
                    Return Scripting.CTypeDynamic(GetStripString, typeOfT)
            End Select
        End Function

        ''' <summary>
        ''' 处理转义等特殊字符串
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStripString() As String
            Dim s$ = Scripting _
                .ToString(value, "null") _
                .GetString
            s = JsonParser.StripString(s)
            Return s
        End Function

        Public Overrides Function ToString() As String
            Return GetStripString()
        End Function
    End Class
End Namespace
