Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl
Imports std = System.Math

''' <summary>
''' Represents a value in a dataframe cell.
''' 
''' Depending on the underlying type, can be coerced to:
'''   - (s)byte(?)
'''   - (u)short(?)
'''   - (u)int(?)
'''   - (u)long(?)
'''   - float(?)
'''   - double(?)
'''   - bool(?)
'''   - string
'''   - DateTime(?)
'''   - DateTimeOffset(?)
'''   - TimeSpan(?)
'''   
''' Conversions can be performed with casts (which throw if the conversion is invalid) or the TryConvert methods which
''' return false if the conversion is invalid.
''' 
''' Conversions are done lazily.  Values just represent offsets into a dataframe.
''' 
''' Enumerations are a special case.  If the underyling type is a category, enums can be used if:
'''   - the names of each enum field match a category name
'''   - the valyues of each enum field match a category index
'''   
''' However, no cast operators can be provided for enumerations.  Conversion must go through either an (int) cast or
''' a TryConvert call.
''' </summary>
Public Class Value
    Implements IEquatable(Of Value), IEquatable(Of Byte), IEquatable(Of Byte?), IEquatable(Of SByte), IEquatable(Of SByte?), IEquatable(Of Short), IEquatable(Of Short?), IEquatable(Of UShort), IEquatable(Of UShort?), IEquatable(Of Integer), IEquatable(Of Integer?), IEquatable(Of UInteger), IEquatable(Of UInteger?), IEquatable(Of Long), IEquatable(Of Long?), IEquatable(Of ULong), IEquatable(Of ULong?), IEquatable(Of Single), IEquatable(Of Single?), IEquatable(Of Double), IEquatable(Of Double?), IEquatable(Of Boolean), IEquatable(Of Boolean?), IEquatable(Of String), IEquatable(Of Date), IEquatable(Of Date?), IEquatable(Of DateTimeOffset), IEquatable(Of DateTimeOffset?), IEquatable(Of TimeSpan), IEquatable(Of TimeSpan?)
    Private Parent As DataFrame

    Private TranslatedRowIndex As Long
    Friend TranslatedColumnIndex As Long

    ''' <summary>
    ''' The row index, in the containing dataframe's basis, of this Value.
    ''' </summary>
    Public ReadOnly Property RowIndex As Long
        Get
            Return Parent.UntranslateIndex(TranslatedRowIndex)
        End Get
    End Property
    ''' <summary>
    ''' The column index, in the containing dataframe's basis, of this Value.
    ''' </summary>
    Public ReadOnly Property ColumnIndex As Long
        Get
            Return Parent.UntranslateIndex(TranslatedColumnIndex)
        End Get
    End Property
    ''' <summary>
    ''' The .NET type that best matches the underlying value of this Value.
    ''' 
    ''' Categories (which best mapped to enums) have a Type of System.Enum.
    ''' </summary>
    Public ReadOnly Property Type As Type
        Get
            Return OnDiskType.GetMapType()
        End Get
    End Property

    ''' <summary>
    ''' The row that contains this value.
    ''' </summary>
    Public ReadOnly Property Row As Row
        Get
            Dim lRow As Row = Nothing
            If Not Parent.TryGetRowTranslated(TranslatedRowIndex, lRow) Then
                Throw New ArgumentOutOfRangeException($"Row index unexpectedly out of bounds {TranslatedRowIndex}")
            End If

            Return lRow
        End Get
    End Property

    ''' <summary>
    ''' The column that contains this value.
    ''' </summary>
    Public ReadOnly Property Column As Column
        Get
            Dim col As Column = Nothing
            If Not Parent.TryGetColumnTranslated(TranslatedColumnIndex, col) Then
                Throw New ArgumentOutOfRangeException($"Column index unexpectedly out of bounds {TranslatedColumnIndex}")
            End If

            Return col
        End Get
    End Property

    Friend ReadOnly Property OnDiskType As ColumnType
        Get
            Return Parent.Metadata.Columns(TranslatedColumnIndex).Type
        End Get
    End Property
    Friend ReadOnly Property IsCategory As Boolean
        Get
            Return (OnDiskType = ColumnType.Int32 OrElse OnDiskType = ColumnType.NullableInt32) AndAlso Categories IsNot Nothing
        End Get
    End Property
    Friend ReadOnly Property Categories As String()
        Get
            Return Parent.Metadata.Columns(TranslatedColumnIndex).CategoryLevels
        End Get
    End Property

    Friend Sub New(translatedRowIndex As Long, translatedColumnIndex As Long, parent As DataFrame)
        Me.Parent = parent
        Me.TranslatedRowIndex = translatedRowIndex
        Me.TranslatedColumnIndex = translatedColumnIndex
    End Sub

    Public Function [TryCast](type As Type) As Object
        Select Case type
            Case GetType(Boolean)
                Dim val As Boolean = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Integer)

                Dim val As Integer = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Single)

                Dim val As Single = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Long)

                Dim val As Long = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Double)

                Dim val As Double = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(String)

                Dim val As String = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(UInteger)

                Dim val As UInteger = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(ULong)

                Dim val As ULong = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Date)

                Dim val As Date = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(TimeSpan)

                Dim val As TimeSpan = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Byte)

                Dim val As Byte = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(SByte)

                Dim val As SByte = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(Short)

                Dim val As Short = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case GetType(UShort)

                Dim val As UShort = Nothing

                If TryConvert(val) Then
                    Return val
                Else
                    Return Nothing
                End If

            Case Else
                Throw New NotImplementedException(type.FullName)
        End Select
    End Function

    Public Overloads Function Equals(other As Value) As Boolean Implements IEquatable(Of Value).Equals
        Select Case other.OnDiskType
                ' TODO (Binary)
                                                ' END TODO

            Case ColumnType.NullableBinary, ColumnType.Binary, ColumnType.Category
                Return CategoryEquals(Me, other)
            Case ColumnType.NullableCategory
                Return NullableCategoryEquals(Me, other)

            Case ColumnType.Date, ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Nanosecond, ColumnType.Timestamp_Second
                Return Equals(CDate(other))

            Case ColumnType.Time_Microsecond, ColumnType.Time_Millisecond, ColumnType.Time_Nanosecond, ColumnType.Time_Second
                Return Equals(CType(other, TimeSpan))

            Case ColumnType.NullableDate, ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Nanosecond, ColumnType.NullableTimestamp_Second
                Return Equals(CType(other, Date?))

            Case ColumnType.NullableTime_Microsecond, ColumnType.NullableTime_Millisecond, ColumnType.NullableTime_Nanosecond, ColumnType.NullableTime_Second
                Return Equals(CType(other, TimeSpan?))

            Case ColumnType.Bool
                Return Equals(CBool(other))
            Case ColumnType.NullableBool
                Return Equals(CType(other, Boolean?))
            Case ColumnType.Double
                Return Equals(CDbl(other))
            Case ColumnType.NullableDouble
                Return Equals(CType(other, Double?))
            Case ColumnType.Float
                Return Equals(CSng(other))
            Case ColumnType.NullableFloat
                Return Equals(CType(other, Single?))
            Case ColumnType.Int16
                Return Equals(CShort(other))
            Case ColumnType.NullableInt16
                Return Equals(CType(other, Short?))
            Case ColumnType.Int32
                Return Equals(CInt(other))
            Case ColumnType.NullableInt32
                Return Equals(CType(other, Integer?))
            Case ColumnType.Int64
                Return Equals(CLng(other))
            Case ColumnType.NullableInt64
                Return Equals(CType(other, Long?))
            Case ColumnType.Int8
                Return Equals(CSByte(other))
            Case ColumnType.NullableInt8
                Return Equals(CType(other, SByte?))

            Case ColumnType.NullableString, ColumnType.String
                Return Equals(CStr(other))

            Case ColumnType.Uint16
                Return Equals(CUShort(other))
            Case ColumnType.NullableUint16
                Return Equals(CType(other, UShort?))
            Case ColumnType.Uint32
                Return Equals(CUInt(other))
            Case ColumnType.NullableUint32
                Return Equals(CType(other, UInteger?))
            Case ColumnType.Uint64
                Return Equals(CLng(other))
            Case ColumnType.NullableUint64
                Return Equals(CType(other, ULong?))
            Case ColumnType.Uint8
                Return Equals(CByte(other))
            Case ColumnType.NullableUint8
                Return Equals(CType(other, Byte?))
            Case Else

                Throw New Exception($"Unexpected ColumnType {other.OnDiskType}")
        End Select
    End Function

    Public Overloads Function Equals(b As Boolean) As Boolean Implements IEquatable(Of Boolean).Equals
        Dim other As Boolean
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(b As Boolean?) As Boolean Implements IEquatable(Of Boolean?).Equals
        Dim other As Boolean?
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(b As Byte) As Boolean Implements IEquatable(Of Byte).Equals
        Dim other As Byte
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(b As Byte?) As Boolean Implements IEquatable(Of Byte?).Equals
        Dim other As Byte?
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(b As SByte) As Boolean Implements IEquatable(Of SByte).Equals
        Dim other As SByte
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(b As SByte?) As Boolean Implements IEquatable(Of SByte?).Equals
        Dim other As SByte?
        If Not TryConvert(other) Then Return False

        Return b = other
    End Function

    Public Overloads Function Equals(s As Short) As Boolean Implements IEquatable(Of Short).Equals
        Dim other As Short
        If Not TryConvert(other) Then Return False

        Return s = other
    End Function

    Public Overloads Function Equals(s As Short?) As Boolean Implements IEquatable(Of Short?).Equals
        Dim other As Short?
        If Not TryConvert(other) Then Return False

        Return s = other
    End Function

    Public Overloads Function Equals(s As UShort) As Boolean Implements IEquatable(Of UShort).Equals
        Dim other As UShort
        If Not TryConvert(other) Then Return False

        Return s = other
    End Function

    Public Overloads Function Equals(s As UShort?) As Boolean Implements IEquatable(Of UShort?).Equals
        Dim other As UShort?
        If Not TryConvert(other) Then Return False

        Return s = other
    End Function

    Public Overloads Function Equals(i As Integer) As Boolean Implements IEquatable(Of Integer).Equals
        Dim other As Integer
        If Not TryConvert(other) Then Return False

        Return i = other
    End Function

    Public Overloads Function Equals(i As Integer?) As Boolean Implements IEquatable(Of Integer?).Equals
        Dim other As Integer?
        If Not TryConvert(other) Then Return False

        Return i = other
    End Function

    Public Overloads Function Equals(i As UInteger) As Boolean Implements IEquatable(Of UInteger).Equals
        Dim other As UInteger
        If Not TryConvert(other) Then Return False

        Return i = other
    End Function

    Public Overloads Function Equals(i As UInteger?) As Boolean Implements IEquatable(Of UInteger?).Equals
        Dim other As UInteger?
        If Not TryConvert(other) Then Return False

        Return i = other
    End Function

    Public Overloads Function Equals(l As Long) As Boolean Implements IEquatable(Of Long).Equals
        Dim other As Long
        If Not TryConvert(other) Then Return False

        Return l = other
    End Function

    Public Overloads Function Equals(l As Long?) As Boolean Implements IEquatable(Of Long?).Equals
        Dim other As Long?
        If Not TryConvert(other) Then Return False

        Return l = other
    End Function

    Public Overloads Function Equals(l As ULong) As Boolean Implements IEquatable(Of ULong).Equals
        Dim other As ULong
        If Not TryConvert(other) Then Return False

        Return l = other
    End Function

    Public Overloads Function Equals(l As ULong?) As Boolean Implements IEquatable(Of ULong?).Equals
        Dim other As ULong?
        If Not TryConvert(other) Then Return False

        Return l = other
    End Function

    Public Overloads Function Equals(d As Double) As Boolean Implements IEquatable(Of Double).Equals
        Dim other As Double
        If Not TryConvert(other) Then Return False

        Return d = other
    End Function

    Public Overloads Function Equals(d As Double?) As Boolean Implements IEquatable(Of Double?).Equals
        Dim other As Double?
        If Not TryConvert(other) Then Return False

        Return d = other
    End Function

    Public Overloads Function Equals(f As Single) As Boolean Implements IEquatable(Of Single).Equals
        Dim other As Single
        If Not TryConvert(other) Then Return False

        Return f = other
    End Function

    Public Overloads Function Equals(f As Single?) As Boolean Implements IEquatable(Of Single?).Equals
        Dim other As Single?
        If Not TryConvert(other) Then Return False

        Return f = other
    End Function

    Public Overloads Function Equals(str As String) As Boolean Implements IEquatable(Of String).Equals
        Dim other As String = Nothing
        If Not TryConvert(other) Then Return False

        Return String.Equals(str, other)
    End Function

    Public Overloads Function Equals([date] As Date) As Boolean Implements IEquatable(Of Date).Equals
        Dim other As Date
        If Not TryConvert(other) Then Return False

        Return [date] = other
    End Function

    Public Overloads Function Equals([date] As Date?) As Boolean Implements IEquatable(Of Date?).Equals
        Dim other As Date?
        If Not TryConvert(other) Then Return False

        Return [date] = other
    End Function

    Public Overloads Function Equals([date] As DateTimeOffset) As Boolean Implements IEquatable(Of DateTimeOffset).Equals
        Dim other As DateTimeOffset
        If Not TryConvert(other) Then Return False

        Return [date] = other
    End Function

    Public Overloads Function Equals([date] As DateTimeOffset?) As Boolean Implements IEquatable(Of DateTimeOffset?).Equals
        Dim other As DateTimeOffset?
        If Not TryConvert(other) Then Return False

        Return [date] = other
    End Function

    Public Overloads Function Equals(time As TimeSpan) As Boolean Implements IEquatable(Of TimeSpan).Equals
        Dim other As TimeSpan
        If Not TryConvert(other) Then Return False

        Return time = other
    End Function

    Public Overloads Function Equals(time As TimeSpan?) As Boolean Implements IEquatable(Of TimeSpan?).Equals
        Dim other As TimeSpan?
        If Not TryConvert(other) Then Return False

        Return time = other
    End Function

    Public Function TryConvert(<Out> ByRef value As Double) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Double OrElse type = ColumnType.Float OrElse type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Uint8 OrElse type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableDouble OrElse type = ColumnType.NullableFloat OrElse type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8 OrElse type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Double.NaN
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Double.NaN
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Double, ColumnType.NullableDouble
                value = Parent.ReadDouble(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Float, ColumnType.NullableFloat
                value = Parent.ReadFloat(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Double?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Double OrElse type = ColumnType.Float OrElse type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Int8 OrElse type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableDouble OrElse type = ColumnType.NullableFloat OrElse type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8 OrElse type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Double, ColumnType.NullableDouble
                value = Parent.ReadDouble(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Float, ColumnType.NullableFloat
                value = Parent.ReadFloat(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = CByte(Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex))
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Double
        Dim ret As Double
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to double, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Double?
        Dim ret As Double?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to double?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Single) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Float OrElse type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Int8 OrElse type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableFloat OrElse type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8 OrElse type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Single.NaN
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Single.NaN
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Float, ColumnType.NullableFloat
                value = Parent.ReadFloat(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Single?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Float OrElse type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Int8 OrElse type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableFloat OrElse type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8 OrElse type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Float, ColumnType.NullableFloat
                value = Parent.ReadFloat(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = CByte(Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex))
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Single
        Dim ret As Single
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to float, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Single?
        Dim ret As Single?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to float?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Byte) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Byte?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Byte
        Dim ret As Byte
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to byte, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Byte?
        Dim ret As Byte?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to byte?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As SByte) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As SByte?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As SByte
        Dim ret As SByte
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to sbyte, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As SByte?
        Dim ret As SByte?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to sbyte?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Short) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Short?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Short
        Dim ret As Short
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to short, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Short?
        Dim ret As Short?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to short?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As UShort) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As UShort?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As UShort
        Dim ret As UShort
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to ushort, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As UShort?
        Dim ret As UShort?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to ushort?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Integer) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Integer?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Integer
        Dim ret As Integer
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to int, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Integer?
        Dim ret As Integer?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to int?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As UInteger) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As UInteger?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As UInteger
        Dim ret As UInteger
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to uint, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As UInteger?
        Dim ret As UInteger?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to uint?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Long) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Long?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Int16 OrElse type = ColumnType.Int32 OrElse type = ColumnType.Int64 OrElse type = ColumnType.Int8

        Dim canConvertNullable = type = ColumnType.NullableInt16 OrElse type = ColumnType.NullableInt32 OrElse type = ColumnType.NullableInt64 OrElse type = ColumnType.NullableInt8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Int16, ColumnType.NullableInt16
                value = Parent.ReadInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int32, ColumnType.NullableInt32
                value = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int64, ColumnType.NullableInt64
                value = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Int8, ColumnType.NullableInt8
                value = Parent.ReadInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As Long
        Dim ret As Long
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to long, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Long?
        Dim ret As Long?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to long?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As ULong) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = 0
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = 0
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As ULong?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Uint16 OrElse type = ColumnType.Uint32 OrElse type = ColumnType.Uint64 OrElse type = ColumnType.Uint8

        Dim canConvertNullable = type = ColumnType.NullableUint16 OrElse type = ColumnType.NullableUint32 OrElse type = ColumnType.NullableUint64 OrElse type = ColumnType.NullableUint8

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Uint16, ColumnType.NullableUint16
                value = Parent.ReadUInt16(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint32, ColumnType.NullableUint32
                value = Parent.ReadUInt32(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint64, ColumnType.NullableUint64
                value = Parent.ReadUInt64(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case ColumnType.Uint8, ColumnType.NullableUint8
                value = Parent.ReadUInt8(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As ULong
        Dim ret As ULong
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to ulong, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As ULong?
        Dim ret As ULong?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to ulong?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As String) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.String

        Dim canConvertNullable = type = ColumnType.NullableString

        Dim canConvertCategory = IsCategory

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable OrElse canConvertCategory

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertCategory Then
            Dim isNullable = OnDiskType = ColumnType.NullableInt32
            Dim categories = Me.Categories

            If isNullable Then
                Dim asIndex = CType(Me, Integer?)
                If asIndex Is Nothing Then
                    value = Nothing
                    Return True
                End If

                If asIndex.Value < 0 OrElse asIndex.Value >= categories.Length Then
                    value = Nothing
                    Return False
                End If

                value = categories(asIndex.Value)
            Else
                Dim asIndex = CInt(Me)
                If asIndex < 0 OrElse asIndex >= categories.Length Then
                    value = Nothing
                    Return False
                End If

                value = categories(asIndex)
            End If

            Return True
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.String
                value = If(Parent.ReadString(TranslatedRowIndex, TranslatedColumnIndex), "")    ' non-nullable string should never return null
                Return True
            Case ColumnType.NullableString
                value = Parent.ReadString(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Shared Widening Operator CType(value As Value) As String
        Dim ret As String = Nothing
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to string, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Boolean
        Dim ret As Boolean
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to bool, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Boolean?
        Dim ret As Boolean?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to bool?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As Boolean) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Bool

        Dim canConvertNullable = type = ColumnType.NullableBool

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = False
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = False
                Return False
            End If
        End If

        Select Case type
            Case ColumnType.Bool, ColumnType.NullableBool
                value = Parent.ReadBool(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As Boolean?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Bool

        Dim canConvertNullable = type = ColumnType.NullableBool

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Select Case type
            Case ColumnType.Bool, ColumnType.NullableBool
                value = Parent.ReadBool(TranslatedRowIndex, TranslatedColumnIndex)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function


    Const NANOSECONDS_PER_TICK As Long = 100
    Const TICKS_PER_NANOSECOND As Double = 1.0 / NANOSECONDS_PER_TICK
    Const TICKS_PER_MICROSECOND As Long = TICKS_PER_NANOSECOND * 1000
    Const TICKS_PER_MILLISECOND As Long = TICKS_PER_MICROSECOND * 1000
    Const TICKS_PER_SECOND As Long = TICKS_PER_MILLISECOND * 1000

    Public Shared Widening Operator CType(value As Value) As Date
        Dim ret As Date
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to DateTime, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As Date?
        Dim ret As Date?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to DateTime?, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As DateTimeOffset
        Return CDate(value)
    End Operator
    Public Shared Widening Operator CType(value As Value) As DateTimeOffset?
        Return CType(value, Date?)
    End Operator

    Public Function TryConvert(<Out> ByRef value As Date) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Timestamp_Microsecond OrElse type = ColumnType.Timestamp_Millisecond OrElse type = ColumnType.Timestamp_Nanosecond OrElse type = ColumnType.Timestamp_Second OrElse type = ColumnType.Date

        Dim canConvertNullable = type = ColumnType.NullableTimestamp_Microsecond OrElse type = ColumnType.NullableTimestamp_Millisecond OrElse type = ColumnType.NullableTimestamp_Nanosecond OrElse type = ColumnType.NullableTimestamp_Second OrElse type = ColumnType.NullableDate

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return False
            End If
        End If

        Dim isDate = type = ColumnType.Date OrElse type = ColumnType.NullableDate

        If isDate Then
            Dim val = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)

            value = DATETIME_EPOCH + TimeSpan.FromDays(val)
            Return True
        Else
            Dim val = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)

            Select Case type
                Case ColumnType.Timestamp_Microsecond, ColumnType.NullableTimestamp_Microsecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_MICROSECOND)
                    Return True

                Case ColumnType.Timestamp_Millisecond, ColumnType.NullableTimestamp_Millisecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_MILLISECOND)
                    Return True

                Case ColumnType.Timestamp_Nanosecond, ColumnType.NullableTimestamp_Nanosecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(std.Round(val * TICKS_PER_NANOSECOND))
                    Return True

                Case ColumnType.Timestamp_Second, ColumnType.NullableTimestamp_Second
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_SECOND)
                    Return True
                Case Else

                    Throw New InvalidOperationException($"Unexpected column type {type}")
            End Select
        End If
    End Function

    Public Function TryConvert(<Out> ByRef value As Date?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Timestamp_Microsecond OrElse type = ColumnType.Timestamp_Millisecond OrElse type = ColumnType.Timestamp_Nanosecond OrElse type = ColumnType.Timestamp_Second OrElse type = ColumnType.Date

        Dim canConvertNullable = type = ColumnType.NullableTimestamp_Microsecond OrElse type = ColumnType.NullableTimestamp_Millisecond OrElse type = ColumnType.NullableTimestamp_Nanosecond OrElse type = ColumnType.NullableTimestamp_Second OrElse type = ColumnType.NullableDate

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Dim isDate = type = ColumnType.Date OrElse type = ColumnType.NullableDate

        If isDate Then
            Dim val = Parent.ReadInt32(TranslatedRowIndex, TranslatedColumnIndex)

            value = DATETIME_EPOCH + TimeSpan.FromDays(val)
            Return True
        Else
            Dim val = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)

            Select Case type
                Case ColumnType.Timestamp_Microsecond, ColumnType.NullableTimestamp_Microsecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_MICROSECOND)
                    Return True

                Case ColumnType.Timestamp_Millisecond, ColumnType.NullableTimestamp_Millisecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_MILLISECOND)
                    Return True

                Case ColumnType.Timestamp_Nanosecond, ColumnType.NullableTimestamp_Nanosecond
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(std.Round(val * TICKS_PER_NANOSECOND))
                    Return True

                Case ColumnType.Timestamp_Second, ColumnType.NullableTimestamp_Second
                    value = DATETIME_EPOCH + TimeSpan.FromTicks(val * TICKS_PER_SECOND)
                    Return True
                Case Else

                    Throw New InvalidOperationException($"Unexpected column type {type}")
            End Select
        End If
    End Function

    Public Function TryConvert(<Out> ByRef value As DateTimeOffset) As Boolean
        Dim dt As Date
        If Not TryConvert(dt) Then
            value = Nothing
            Return False
        End If

        value = dt
        Return True
    End Function

    Public Function TryConvert(<Out> ByRef value As DateTimeOffset?) As Boolean
        Dim dt As Date?
        If Not TryConvert(dt) Then
            value = Nothing
            Return False
        End If

        value = dt
        Return True
    End Function

    Public Shared Widening Operator CType(value As Value) As TimeSpan
        Dim ret As TimeSpan
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to TimeSpan, underlying type is {value.OnDiskType}")
    End Operator

    Public Shared Widening Operator CType(value As Value) As TimeSpan?
        Dim ret As TimeSpan?
        If value.TryConvert(ret) Then
            Return ret
        End If

        Throw New InvalidCastException($"Could not convert value to TimeSpan?, underlying type is {value.OnDiskType}")
    End Operator

    Public Function TryConvert(<Out> ByRef value As TimeSpan) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Time_Microsecond OrElse type = ColumnType.Time_Millisecond OrElse type = ColumnType.Time_Nanosecond OrElse type = ColumnType.Time_Second

        Dim canConvertNullable = type = ColumnType.NullableTime_Microsecond OrElse type = ColumnType.NullableTime_Millisecond OrElse type = ColumnType.NullableTime_Nanosecond OrElse type = ColumnType.NullableTime_Second

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            ' null isn't castable, freak out
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return False
            End If
        End If

        Dim val = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)

        Select Case type
            Case ColumnType.NullableTime_Microsecond, ColumnType.Time_Microsecond
                value = TimeSpan.FromTicks(val * TICKS_PER_MICROSECOND)
                Return True

            Case ColumnType.NullableTime_Millisecond, ColumnType.Time_Millisecond
                value = TimeSpan.FromTicks(val * TICKS_PER_MILLISECOND)
                Return True

            Case ColumnType.NullableTime_Nanosecond, ColumnType.Time_Nanosecond
                value = TimeSpan.FromTicks(std.Round(val * TICKS_PER_NANOSECOND))
                Return True

            Case ColumnType.NullableTime_Second, ColumnType.Time_Second
                value = TimeSpan.FromTicks(val * TICKS_PER_SECOND)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Function TryConvert(<Out> ByRef value As TimeSpan?) As Boolean
        Dim type = OnDiskType

        Dim canConvertNonNullable = type = ColumnType.Time_Microsecond OrElse type = ColumnType.Time_Millisecond OrElse type = ColumnType.Time_Nanosecond OrElse type = ColumnType.Time_Second

        Dim canConvertNullable = type = ColumnType.NullableTime_Microsecond OrElse type = ColumnType.NullableTime_Millisecond OrElse type = ColumnType.NullableTime_Nanosecond OrElse type = ColumnType.NullableTime_Second

        Dim canConvert = canConvertNonNullable OrElse canConvertNullable

        If Not canConvert Then
            value = Nothing
            Return False
        End If

        If canConvertNullable Then
            If Parent.IsNullTranslated(TranslatedRowIndex, TranslatedColumnIndex) Then
                value = Nothing
                Return True
            End If
        End If

        Dim val = Parent.ReadInt64(TranslatedRowIndex, TranslatedColumnIndex)

        Select Case type
            Case ColumnType.NullableTime_Microsecond, ColumnType.Time_Microsecond
                value = TimeSpan.FromTicks(val * TICKS_PER_MICROSECOND)
                Return True

            Case ColumnType.NullableTime_Millisecond, ColumnType.Time_Millisecond
                value = TimeSpan.FromTicks(val * TICKS_PER_MILLISECOND)
                Return True

            Case ColumnType.NullableTime_Nanosecond, ColumnType.Time_Nanosecond
                value = TimeSpan.FromTicks(std.Round(val * TICKS_PER_NANOSECOND))
                Return True

            Case ColumnType.NullableTime_Second, ColumnType.Time_Second
                value = TimeSpan.FromTicks(val * TICKS_PER_SECOND)
                Return True
            Case Else

                Throw New InvalidOperationException($"Unexpected column type {type}")
        End Select
    End Function

    Public Overrides Function ToString() As String
        Dim valStr As String
        Select Case OnDiskType
                ' TODO (Binary)
            Case ColumnType.Binary, ColumnType.NullableBinary
                Throw New NotImplementedException()
                ' END TODO

            Case ColumnType.Category, ColumnType.NullableCategory, ColumnType.String
                valStr = """" & ToString() & """"
            Case ColumnType.NullableString
                Dim asStr = CStr(Me)
                valStr = If(Not asStr Is Nothing, """" & asStr & """", "null")

            Case ColumnType.Bool
                valStr = CBool(Me).ToString()
            Case ColumnType.NullableBool
                Dim asBool = CType(Me, Boolean?)
                valStr = If(asBool IsNot Nothing, asBool.ToString(), "null")

            Case ColumnType.Date, ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Nanosecond, ColumnType.Timestamp_Second
                valStr = CDate(Me).ToString()

            Case ColumnType.NullableDate, ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Nanosecond, ColumnType.NullableTimestamp_Second
                Dim asDatetime = CType(Me, Date?)
                valStr = If(asDatetime IsNot Nothing, asDatetime.ToString(), "null")

            Case ColumnType.Time_Microsecond, ColumnType.Time_Millisecond
                valStr = CType(Me, TimeSpan).ToString()

            Case ColumnType.Time_Nanosecond, ColumnType.Time_Second
                Dim asTimeSpan = CType(Me, TimeSpan?)
                valStr = If(asTimeSpan IsNot Nothing, asTimeSpan.ToString(), "null")

            Case ColumnType.Double
                valStr = CDbl(Me).ToString()
            Case ColumnType.NullableDouble
                Dim asNullableDouble = CType(Me, Double?)
                valStr = If(asNullableDouble.HasValue, asNullableDouble.ToString(), "null")

            Case ColumnType.Float
                valStr = CSng(Me).ToString()
            Case ColumnType.NullableFloat
                Dim asNullableFloat = CType(Me, Single?)
                valStr = If(asNullableFloat.HasValue, asNullableFloat.ToString(), "null")

            Case ColumnType.Int16
                valStr = CShort(Me).ToString()
            Case ColumnType.NullableInt16
                Dim asNullableShort = CType(Me, Short?)
                valStr = If(asNullableShort.HasValue, asNullableShort.ToString(), "null")

            Case ColumnType.Uint16
                valStr = CUShort(Me).ToString()
            Case ColumnType.NullableUint16
                Dim asNullableUShort = CType(Me, UShort?)
                valStr = If(asNullableUShort.HasValue, asNullableUShort.ToString(), "null")

            Case ColumnType.Int32
                valStr = CInt(Me).ToString()
            Case ColumnType.NullableInt32
                Dim asNullableInt = CType(Me, Integer?)
                valStr = If(asNullableInt.HasValue, asNullableInt.ToString(), "null")

            Case ColumnType.Uint32
                valStr = CUInt(Me).ToString()
            Case ColumnType.NullableUint32
                Dim asNullableUint = CType(Me, UInteger?)
                valStr = If(asNullableUint.HasValue, asNullableUint.ToString(), "null")

            Case ColumnType.Int64
                valStr = CLng(Me).ToString()
            Case ColumnType.NullableInt64
                Dim asNullableLong = CType(Me, Long?)
                valStr = If(asNullableLong.HasValue, asNullableLong.ToString(), "null")

            Case ColumnType.Uint64
                valStr = CULng(Me).ToString()
            Case ColumnType.NullableUint64
                Dim asNullableUlong = CType(Me, ULong?)
                valStr = If(asNullableUlong.HasValue, asNullableUlong.ToString(), "null")

            Case ColumnType.Int8
                valStr = CSByte(Me).ToString()
            Case ColumnType.NullableInt8
                Dim asNullableSbyte = CType(Me, SByte?)
                valStr = If(asNullableSbyte.HasValue, asNullableSbyte.ToString(), "null")

            Case ColumnType.Uint8
                valStr = CByte(Me).ToString()
            Case ColumnType.NullableUint8
                Dim asNullableByte = CType(Me, Byte?)
                valStr = If(asNullableByte.HasValue, asNullableByte.ToString(), "null")
            Case Else

                Throw New InvalidOperationException($"Unexpected ColumnType {Type}")
        End Select

        Return $"Value Type={OnDiskType}, Row={Parent.UntranslateIndex(TranslatedRowIndex)}, Column={Parent.UntranslateIndex(TranslatedColumnIndex)}, Value={valStr}"
    End Function

    ' unsafe 'cause it'll explode messily if the conversion isn't valid
    Friend Function UnsafeCast(Of T)(categories As CategoryEnumMapType) As T
        Return ValueCaster(Of T).Cast(Me, categories)
    End Function

    Friend Shared Function ConvertEnum(Of T As Structure)(value As Value, categories As CategoryEnumMapType) As T
        Select Case categories
            Case CategoryEnumMapType.ByName
                Return [Enum].Parse(GetType(T), value, ignoreCase:=True)
            Case CategoryEnumMapType.ByOrdinal
                Dim asInt = CInt(value)
                Return EnumMapper(Of T).Map(asInt)
            Case Else

                Throw New Exception($"Unexpected {NameOf(CategoryEnumMapType)} {categories}")
        End Select
    End Function

    Friend Shared Function ConvertNullableEnum(Of T As Structure)(value As Value, categories As CategoryEnumMapType) As T?
        Dim isNullable = value.OnDiskType = ColumnType.NullableInt32 OrElse value.OnDiskType = ColumnType.NullableString

        If isNullable Then
            If value.Parent.IsNullTranslated(value.TranslatedRowIndex, value.TranslatedColumnIndex) Then
                Return Nothing
            End If
        End If

        Select Case categories
            Case CategoryEnumMapType.ByName
                Return CType([Enum].Parse(GetType(T), value, ignoreCase:=True), T)
            Case CategoryEnumMapType.ByOrdinal
                Dim asInt = CInt(value)
                Return EnumMapper(Of T).Map(asInt)
            Case Else

                Throw New Exception($"Unexpected {NameOf(CategoryEnumMapType)} {categories}")
        End Select
    End Function

    Private Shared Function CategoryEquals(a As Value, b As Value) As Boolean
        If Not a.IsCategory AndAlso Not b.IsCategory Then Throw New Exception($"Shouldn't be possible, tried to compare two non-category types")
        If a.IsCategory AndAlso Not b.IsCategory Then Return False
        If b.IsCategory AndAlso Not a.IsCategory Then Return False

        ' these are often zero-allocation, so might as well
        Dim aAsStr = CStr(a)
        Dim bAsStr = CStr(b)

        Return String.Equals(aAsStr, bAsStr)
    End Function

    Private Shared Function NullableCategoryEquals(a As Value, b As Value) As Boolean
        Return CategoryEquals(a, b)
    End Function
End Class
