#Region "Microsoft.VisualBasic::80107e9227b3aeddbd1e8132bce5e2b1, mime\application%json\Javascript\JsonValue.vb"

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

    '   Total Lines: 152
    '    Code Lines: 106 (69.74%)
    ' Comment Lines: 27 (17.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (12.50%)
    '     File Size: 5.07 KB


    '     Class JsonValue
    ' 
    '         Properties: BSONValue, IsEmptyString, IsLiteralNull, NULL, UnderlyingType
    '                     value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetStripString, (+2 Overloads) Literal, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.json.BSON
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports any = Microsoft.VisualBasic.Scripting

Namespace Javascript

    ''' <summary>
    ''' The primitive value type in javascript.
    ''' </summary>
    ''' <remarks>
    ''' (请注意，假若是字符串的话，值是未经过处理的原始字符串，可能会含有转义字符，
    ''' 则这个时候还需要使用<see cref="GetStripString"/>得到最终的字符串)
    ''' </remarks>
    Public Class JsonValue : Inherits JsonElement

        Public Overloads Property value As Object

        Public ReadOnly Property BSONValue As BSONValue
            Get
                Return BSONValue.FromValue(value)
            End Get
        End Property

        Public ReadOnly Property UnderlyingType As Type
            Get
                If value Is Nothing Then
                    Return GetType(Object)
                Else
                    Return value.GetType
                End If
            End Get
        End Property

        Public Shared ReadOnly Property NULL As JsonValue
            Get
                Return New JsonValue(Nothing)
            End Get
        End Property

        Public ReadOnly Property IsLiteralNull As Boolean
            Get
                Return value Is Nothing OrElse any.ToString(value).TextEquals("null")
            End Get
        End Property

        Public ReadOnly Property IsEmptyString As Boolean
            Get
                Return IsLiteralNull OrElse any.ToString(value).StringEmpty
            End Get
        End Property

        Public Sub New()
        End Sub

        ''' <summary>
        ''' create based on the value literal data
        ''' </summary>
        ''' <param name="obj"></param>
        Public Sub New(obj As Object)
            value = obj
        End Sub

        ''' <summary>
        ''' get literal value
        ''' </summary>
        ''' <returns></returns>
        Public Function Literal() As Object
            If value Is Nothing Then
                Return Nothing
            ElseIf TypeOf value Is String Then
                Return GetStripString(True)
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' get literal value with type try cast action.
        ''' </summary>
        ''' <param name="typeOfT"></param>
        ''' <param name="decodeMetachar"></param>
        ''' <returns></returns>
        Public Function Literal(typeOfT As Type, decodeMetachar As Boolean) As Object
            Dim str As String = GetStripString(decodeMetachar)

            Select Case typeOfT
                Case GetType(String)
                    Return str
                Case GetType(Date)
                    Return Casting.CastDate(str)
                Case GetType(Boolean)
                    Return str.ParseBoolean
                Case Else
                    If typeOfT.IsEnum Then
                        Return [Enum].Parse(typeOfT, str)
                    Else
                        Return Scripting.CTypeDynamic(str, typeOfT)
                    End If
            End Select
        End Function

        ''' <summary>
        ''' 处理转义等特殊字符串
        ''' </summary>
        ''' <returns>
        ''' this function will removes the warpping of quot symbol.
        ''' </returns>
        Public Function GetStripString(decodeMetachar As Boolean) As String
            Dim s$ = Scripting _
                .ToString(value, "null") _
                .GetString
            s = JsonParser.StripString(s, decodeMetachar)
            Return s
        End Function

        Public Overrides Function ToString() As String
            Return GetStripString(decodeMetachar:=True)
        End Function

        Public Overloads Shared Narrowing Operator CType(value As JsonValue) As String
            If value Is Nothing Then
                Return Nothing
            Else
                Return value.GetStripString(decodeMetachar:=True)
            End If
        End Operator

        Public Overloads Shared Narrowing Operator CType(value As JsonValue) As Boolean
            If value Is Nothing Then
                Return False
            Else
                Return CType(value, String).ParseBoolean
            End If
        End Operator

        Public Overloads Shared Narrowing Operator CType(value As JsonValue) As Double
            If value Is Nothing Then
                Return .0
            Else
                Return CType(value, String).ParseDouble
            End If
        End Operator

        Public Overloads Shared Narrowing Operator CType(value As JsonValue) As Integer
            If value Is Nothing Then
                Return 0
            Else
                Return CType(value, String).ParseInteger
            End If
        End Operator
    End Class
End Namespace
