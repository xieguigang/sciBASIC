#Region "Microsoft.VisualBasic::cbb8df3f4fb4ea72e5f527d5dbd0c611, mime\application%json\BSON\BSONValue.vb"

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

    '   Total Lines: 301
    '    Code Lines: 259 (86.05%)
    ' Comment Lines: 1 (0.33%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 41 (13.62%)
    '     File Size: 11.02 KB


    '     Class BSONValue
    ' 
    '         Properties: binaryValue, boolValue, dateTimeValue, doubleValue, int32Value
    '                     int64Value, isNone, stringValue, valueType
    ' 
    '         Constructor: (+9 Overloads) Sub New
    '         Function: FromValue, GetObjectValue, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Net.Http
Imports ASCII = Microsoft.VisualBasic.Text.ASCII
Imports std = System.Math

Namespace BSON

    Public Class BSONValue

        Private _double As [Double]
        Private _string As String
        Private _binary As Byte()
        Private _bool As Boolean
        Private _dateTime As DateTime
        Private _int32 As Int32
        Private _int64 As Int64

        Public ReadOnly Property valueType As ValueType

        Public ReadOnly Property doubleValue() As [Double]
            Get
                Select Case valueType
                    Case ValueType.Int32
                        Return CDbl(_int32)
                    Case ValueType.Int64
                        Return CDbl(_int64)
                    Case ValueType.[Double]
                        Return _double
                    Case ValueType.None
                        Return Single.NaN
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to double", valueType))
            End Get
        End Property

        Public ReadOnly Property int32Value() As Int32
            Get
                Select Case valueType
                    Case ValueType.Int32
                        Return CType(_int32, Int32)
                    Case ValueType.Int64
                        Return CType(_int64, Int32)
                    Case ValueType.[Double]
                        Return CType(std.Truncate(_double), Int32)
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to Int32", valueType))
            End Get
        End Property

        Public ReadOnly Property int64Value() As Int64
            Get
                Select Case valueType
                    Case ValueType.Int32
                        Return CType(_int32, Int64)
                    Case ValueType.Int64
                        Return CType(_int64, Int64)
                    Case ValueType.[Double]
                        Return CType(std.Truncate(_double), Int64)
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to Int64", valueType))
            End Get
        End Property

        Public ReadOnly Property binaryValue() As Byte()
            Get
                Select Case valueType
                    Case ValueType.Binary
                        Return _binary
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to binary", valueType))
            End Get
        End Property

        Public ReadOnly Property dateTimeValue() As DateTime
            Get
                Select Case valueType
                    Case ValueType.UTCDateTime
                        Return _dateTime
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to DateTime", valueType))
            End Get
        End Property

        Public ReadOnly Property stringValue() As [String]
            Get
                Select Case valueType
                    Case ValueType.Int32
                        Return Convert.ToString(_int32)
                    Case ValueType.Int64
                        Return Convert.ToString(_int64)
                    Case ValueType.[Double]
                        Return Convert.ToString(_double)
                    Case ValueType.[String]
                        Return If(_string IsNot Nothing, _string.TrimEnd(New Char() {ChrW(0)}), Nothing)
                    Case ValueType.[Boolean]
                        Return If(_bool = True, "true", "false")
                    Case ValueType.Binary
                        Return Encoding.UTF8.GetString(_binary).TrimEnd(New Char() {ChrW(0)})
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to string", valueType))
            End Get
        End Property

        Public ReadOnly Property boolValue() As Boolean
            Get
                Select Case valueType
                    Case ValueType.[Boolean]
                        Return _bool
                End Select

                Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to bool", valueType))
            End Get
        End Property

        Public ReadOnly Property isNone() As Boolean
            Get
                Return valueType = ValueType.None
            End Get
        End Property

        Public Shared Widening Operator CType(v As Double) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As Int32) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As Int64) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As Byte()) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As DateTime) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As String) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As Boolean) As BSONValue
            Return New BSONValue(v)
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As Double
            Return v.doubleValue
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As Int32
            Return v.int32Value
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As Int64
            Return v.int64Value
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As Byte()
            Return v.binaryValue
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As DateTime
            Return v.dateTimeValue
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As String
            Return v.stringValue
        End Operator

        Public Shared Widening Operator CType(v As BSONValue) As Boolean
            Return v.boolValue
        End Operator

        '''
        Protected Sub New(valueType As ValueType)
            valueType = valueType
        End Sub

        Public Sub New()
            valueType = ValueType.None
        End Sub

        Public Sub New(v As Double)
            valueType = ValueType.[Double]
            _double = v
        End Sub

        Public Sub New(v As String)
            valueType = ValueType.String
            _string = Strings.Trim(v).Trim(ASCII.NUL)
        End Sub

        Public Sub New(v As Byte())
            valueType = ValueType.Binary
            _binary = v
        End Sub

        Public Sub New(v As Boolean)
            valueType = ValueType.[Boolean]
            _bool = v
        End Sub

        Public Sub New(dt As DateTime)
            valueType = ValueType.UTCDateTime
            _dateTime = dt
        End Sub

        Public Sub New(v As Int32)
            valueType = ValueType.Int32
            _int32 = v
        End Sub

        Public Sub New(v As Int64)
            valueType = ValueType.Int64
            _int64 = v
        End Sub

        Public Function GetObjectValue() As Object
            Select Case valueType
                Case ValueType.Binary
                    Return binaryValue.ToBase64String
                Case ValueType.Boolean
                    Return boolValue
                Case ValueType.Double
                    Return doubleValue
                Case ValueType.Int32
                    Return int32Value
                Case ValueType.Int64
                    Return int64Value
                Case ValueType.None
                    Return "null"
                Case ValueType.String
                    Return stringValue
                Case ValueType.UTCDateTime
                    Return dateTimeValue
                Case Else
                    Throw New Exception(valueType.Description)
            End Select
        End Function

        Public Overrides Function ToString() As String
            Select Case valueType
                Case ValueType.Binary
                    Return $"""{binaryValue.ToBase64String}"""
                Case ValueType.Boolean
                    Return boolValue.ToString.ToLower
                Case ValueType.Double
                    Return doubleValue
                Case ValueType.Int32
                    Return int32Value
                Case ValueType.Int64
                    Return int64Value
                Case ValueType.None
                    Return "null"
                Case ValueType.String
                    Return $"""{stringValue}"""
                Case ValueType.UTCDateTime
                    Return $"""{dateTimeValue.ToString}"""
                Case Else
                    Throw New Exception(valueType.Description)
            End Select
        End Function

        Public Shared Function FromValue(obj As Object) As BSONValue
            If obj Is Nothing Then
                Return New BSONValue
            End If

            Select Case obj.GetType
                Case GetType(Int32), GetType(Byte), GetType(Short), GetType(UShort)
                    Return New BSONValue(CInt(obj))
                Case GetType(Int64), GetType(UInt32)
                    Return New BSONValue(CType(obj, Int64))
                Case GetType(Byte())
                    Return New BSONValue(DirectCast(obj, Byte()))
                Case GetType(DateTime)
                    Return New BSONValue(CType(obj, DateTime))
                Case GetType(String), GetType(Char)
                    Return New BSONValue(CStr(obj))
                Case GetType(Boolean)
                    Return New BSONValue(CType(obj, Boolean))
                Case GetType(Double), GetType(Single), GetType(Decimal)
                    Return New BSONValue(CType(obj, Double))
                Case GetType(BSONValue)
                    Return obj
                Case GetType(ObjectId)
                    Return New BSONValue(DirectCast(obj, ObjectId).value)
                Case Else
                    Throw New InvalidCastException(obj.GetType.FullName)
            End Select
        End Function

        Public Shared Operator =(a As BSONValue, b As Object) As Boolean
            Return Object.ReferenceEquals(a, b)
        End Operator

        Public Shared Operator <>(a As BSONValue, b As Object) As Boolean
            Return Not (a = b)
        End Operator
    End Class
End Namespace
