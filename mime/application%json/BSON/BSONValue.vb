
Imports System.Text

Public Class BSONValue


    Private mValueType As ValueType
    Private _double As [Double]
    Private _string As String
    Private _binary As Byte()
    Private _bool As Boolean
    Private _dateTime As DateTime
    Private _int32 As Int32
    Private _int64 As Int64

    '
    '		protected static BSONValue convert(object obj) {
    '			if (obj as BSONValue != null)
    '				return obj as BSONValue;
    '
    '			if (obj is Int32)
    '				return new BSONValue (obj as Int32);
    '			if (obj is Int64)
    '				return new BSONValue (obj as Int64);
    '			if (obj is byte[])
    '				return new BSONValue (obj as byte[]);
    '			if (obj is DateTime)
    '				return new BSONValue (obj as DateTime);
    '			if (obj is string)
    '				return new BSONValue (obj as string);
    '			if (obj is bool)
    '				return new BSONValue (obj as bool);
    '			if (obj is double)
    '				return new BSONValue (obj as double);
    '
    '			throw new InvalidCastException();
    '		}
    '		


    ''' Properties
    Public ReadOnly Property valueType() As ValueType
        Get
            Return mValueType
        End Get
    End Property
    Public ReadOnly Property doubleValue() As [Double]
        Get
            Select Case mValueType
                Case ValueType.Int32
                    Return CDbl(_int32)
                Case ValueType.Int64
                    Return CDbl(_int64)
                Case ValueType.[Double]
                    Return _double
                Case ValueType.None
                    Return Single.NaN
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to double", mValueType))
        End Get
    End Property
    Public ReadOnly Property int32Value() As Int32
        Get
            Select Case mValueType
                Case ValueType.Int32
                    Return CType(_int32, Int32)
                Case ValueType.Int64
                    Return CType(_int64, Int32)
                Case ValueType.[Double]
                    Return CType(Math.Truncate(_double), Int32)
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to Int32", mValueType))
        End Get
    End Property
    Public ReadOnly Property int64Value() As Int64
        Get
            Select Case mValueType
                Case ValueType.Int32
                    Return CType(_int32, Int64)
                Case ValueType.Int64
                    Return CType(_int64, Int64)
                Case ValueType.[Double]
                    Return CType(Math.Truncate(_double), Int64)
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to Int64", mValueType))
        End Get
    End Property
    Public ReadOnly Property binaryValue() As Byte()
        Get
            Select Case mValueType
                Case ValueType.Binary
                    Return _binary
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to binary", mValueType))
        End Get
    End Property
    Public ReadOnly Property dateTimeValue() As DateTime
        Get
            Select Case mValueType
                Case ValueType.UTCDateTime
                    Return _dateTime
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to DateTime", mValueType))
        End Get
    End Property
    Public ReadOnly Property stringValue() As [String]
        Get
            Select Case mValueType
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

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to string", mValueType))
        End Get
    End Property
    Public ReadOnly Property boolValue() As Boolean
        Get
            Select Case mValueType
                Case ValueType.[Boolean]
                    Return _bool
            End Select

            Throw New Exception(String.Format("Original type is {0}. Cannot convert from {0} to bool", mValueType))
        End Get
    End Property
    Public ReadOnly Property isNone() As Boolean
        Get
            Return mValueType = ValueType.None
        End Get
    End Property

    Default Public Overridable Property Item(key As String) As BSONValue
        Get
            Return Nothing
        End Get
        Set
        End Set
    End Property
    Default Public Overridable Property Item(index As Integer) As BSONValue
        Get
            Return Nothing
        End Get
        Set
        End Set
    End Property
    Public Overridable Sub Clear()
    End Sub
    Public Overridable Sub Add(key As String, value As BSONValue)
    End Sub
    Public Overridable Sub Add(value As BSONValue)
    End Sub
    Public Overridable Function Contains(v As BSONValue) As Boolean
        Return False
    End Function
    Public Overridable Function ContainsKey(key As String) As Boolean
        Return False
    End Function

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
        mValueType = valueType
    End Sub

    Public Sub New()
        mValueType = ValueType.None
    End Sub

    Public Sub New(v As Double)
        mValueType = ValueType.[Double]
        _double = v
    End Sub

    Public Sub New(v As [String])
        mValueType = ValueType.[String]
        _string = v
    End Sub

    Public Sub New(v As Byte())
        mValueType = ValueType.Binary
        _binary = v
    End Sub

    Public Sub New(v As Boolean)
        mValueType = ValueType.[Boolean]
        _bool = v
    End Sub

    Public Sub New(dt As DateTime)
        mValueType = ValueType.UTCDateTime
        _dateTime = dt
    End Sub

    Public Sub New(v As Int32)
        mValueType = ValueType.Int32
        _int32 = v
    End Sub

    Public Sub New(v As Int64)
        mValueType = ValueType.Int64
        _int64 = v
    End Sub


    Public Shared Operator =(a As BSONValue, b As Object) As Boolean
        Return System.[Object].ReferenceEquals(a, b)
    End Operator

    Public Shared Operator <>(a As BSONValue, b As Object) As Boolean
        Return Not (a = b)
    End Operator
End Class

