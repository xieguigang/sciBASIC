#Region "Microsoft.VisualBasic::63d9ee372e640979d7879ed48a573262, Data\BinaryData\Feather\Impl\ColumnSpec.vb"

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

    '   Total Lines: 626
    '    Code Lines: 459 (73.32%)
    ' Comment Lines: 20 (3.19%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 147 (23.48%)
    '     File Size: 27.61 KB


    '     Enum DateTimePrecisionType
    ' 
    '         Microsecond, Millisecond, Nanosecond, Second
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum CategoryEnumMapType
    ' 
    '         ByName, ByOrdinal
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum ColumnType
    ' 
    '         [Date], [Double], [String], Binary, Bool
    '         Category, Float, Int16, Int32, Int64
    '         Int8, NullableBinary, NullableBool, NullableCategory, NullableDate
    '         NullableDouble, NullableFloat, NullableInt16, NullableInt32, NullableInt64
    '         NullableInt8, NullableString, NullableTime_Microsecond, NullableTime_Millisecond, NullableTime_Nanosecond
    '         NullableTime_Second, NullableTimestamp_Microsecond, NullableTimestamp_Millisecond, NullableTimestamp_Nanosecond, NullableTimestamp_Second
    '         NullableUint16, NullableUint32, NullableUint64, NullableUint8, Time_Microsecond
    '         Time_Millisecond, Time_Nanosecond, Time_Second, Timestamp_Microsecond, Timestamp_Millisecond
    '         Timestamp_Nanosecond, Timestamp_Second, Uint16, Uint32, Uint64
    '         Uint8
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module ColumnTypeExtensionMethods
    ' 
    '         Function: CanMapTo, GetAlignment, GetMapType, MapToFeatherEnum, TryCategoriesMapToEnum
    ' 
    '     Class ColumnSpec
    ' 
    '         Properties: CategoryEnumMap, CategoryLevels, DataOffset, Length, MappedType
    '                     Name, NullBitmaskOffset, Type
    ' 
    '         Function: CanMapTo, GetCategoryEnumMap
    ' 
    '     Module DateTimePrecisionTypeExtensionMethods
    ' 
    '         Function: MapToDiskType
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl.FbsMetadata

Namespace Impl
    Friend Enum DateTimePrecisionType
        NONE = 0

        Nanosecond
        Microsecond
        Millisecond
        Second
    End Enum

    Friend Enum CategoryEnumMapType As Byte
        NONE = 0

        ByName
        ByOrdinal
    End Enum

    Friend Enum ColumnType As Byte
        NONE = 0

        Bool

        Int8
        Int16
        Int32
        Int64

        Uint8
        Uint16
        Uint32
        Uint64

        Float
        [Double]

        Binary

        Category

        Timestamp_Second
        Timestamp_Millisecond
        Timestamp_Microsecond
        Timestamp_Nanosecond

        Time_Second
        Time_Millisecond
        Time_Microsecond
        Time_Nanosecond

        [Date]

        [String]

        ' Nullable!
        NullableBool

        NullableInt8
        NullableInt16
        NullableInt32
        NullableInt64

        NullableUint8
        NullableUint16
        NullableUint32
        NullableUint64

        NullableFloat
        NullableDouble

        NullableBinary

        NullableCategory

        NullableTimestamp_Second
        NullableTimestamp_Millisecond
        NullableTimestamp_Microsecond
        NullableTimestamp_Nanosecond

        NullableTime_Second
        NullableTime_Millisecond
        NullableTime_Microsecond
        NullableTime_Nanosecond

        NullableDate

        NullableString
    End Enum

    Friend Module ColumnTypeExtensionMethods
        <Extension()>
        Public Function MapToFeatherEnum(onDiskType As ColumnType) As FbsMetadata.Type
            Select Case onDiskType
                Case ColumnType.Binary, ColumnType.NullableBinary
                    Return FbsMetadata.Type.BINARY

                Case ColumnType.Bool, ColumnType.NullableBool
                    Return FbsMetadata.Type.BOOL

                Case ColumnType.Category, ColumnType.NullableCategory
                    'return FbsMetadata.Type.CATEGORY;
                    ' note: even though there is a CATEGORY type, R produces Int32 backed
                    '   columns.  Possibly a bug?
                    Return FbsMetadata.Type.INT32

                Case ColumnType.Date, ColumnType.NullableDate
                    Return FbsMetadata.Type.DATE

                Case ColumnType.Double, ColumnType.NullableDouble
                    Return FbsMetadata.Type.DOUBLE

                Case ColumnType.Float, ColumnType.NullableFloat
                    Return FbsMetadata.Type.FLOAT

                Case ColumnType.Int16, ColumnType.NullableInt16
                    Return FbsMetadata.Type.INT16

                Case ColumnType.Int32, ColumnType.NullableInt32
                    Return FbsMetadata.Type.INT32

                Case ColumnType.Int64, ColumnType.NullableInt64
                    Return FbsMetadata.Type.INT64

                Case ColumnType.Int8, ColumnType.NullableInt8
                    Return FbsMetadata.Type.INT8

                Case ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Second, ColumnType.Timestamp_Nanosecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Second, ColumnType.NullableTimestamp_Nanosecond
                    'return FbsMetadata.Type.TIMESTAMP;
                    ' note: even though there is a TIMESTAMP type, the actual primitive's the R library produces
                    '  is an int64
                    Return FbsMetadata.Type.INT64

                Case ColumnType.Time_Microsecond, ColumnType.Time_Millisecond, ColumnType.Time_Nanosecond, ColumnType.Time_Second, ColumnType.NullableTime_Microsecond, ColumnType.NullableTime_Millisecond, ColumnType.NullableTime_Nanosecond, ColumnType.NullableTime_Second
                    'return FbsMetadata.Type.TIME;
                    ' note: even though there is a TIME type, the actual primitive's the R library produces
                    '  is an int64
                    Return FbsMetadata.Type.INT64

                Case ColumnType.Uint16, ColumnType.NullableUint16
                    Return FbsMetadata.Type.UINT16

                Case ColumnType.Uint32, ColumnType.NullableUint32
                    Return FbsMetadata.Type.UINT32

                Case ColumnType.Uint64, ColumnType.NullableUint64
                    Return FbsMetadata.Type.UINT64

                Case ColumnType.Uint8, ColumnType.NullableUint8
                    Return FbsMetadata.Type.UINT8

                Case ColumnType.String, ColumnType.NullableString
                    Return FbsMetadata.Type.UTF8
                Case Else

                    Throw New Exception($"Unexpected ColumnType {onDiskType}")
            End Select
        End Function

        <Extension()>
        Public Function GetAlignment(onDiskType As ColumnType) As Byte
            Select Case onDiskType
                Case ColumnType.Bool, ColumnType.NullableBool
                    Return HeapSizeOf.byte

                Case ColumnType.Category
                    Return 4

                ' TODO (Binary)
                Case ColumnType.NullableBinary, ColumnType.Binary
                    Throw New NotImplementedException()
                ' END TODO

                Case ColumnType.Date
                    Return 4

                Case ColumnType.NullableDouble, ColumnType.Double
                    Return HeapSizeOf.double

                Case ColumnType.NullableFloat, ColumnType.Float
                    Return HeapSizeOf.float

                Case ColumnType.NullableInt16, ColumnType.Int16
                    Return HeapSizeOf.short

                Case ColumnType.NullableUint16, ColumnType.Uint16
                    Return HeapSizeOf.ushort

                Case ColumnType.NullableInt32, ColumnType.Int32
                    Return HeapSizeOf.int

                Case ColumnType.NullableUint32, ColumnType.Uint32
                    Return HeapSizeOf.uint

                Case ColumnType.NullableInt64, ColumnType.Int64
                    Return HeapSizeOf.long

                Case ColumnType.NullableUint64, ColumnType.Uint64
                    Return HeapSizeOf.ulong

                Case ColumnType.NullableInt8, ColumnType.Int8
                    Return HeapSizeOf.sbyte

                Case ColumnType.NullableUint8, ColumnType.Uint8
                    Return HeapSizeOf.byte

                Case ColumnType.String, ColumnType.NullableString
                    ' variable length array composed of int32_t offsets followed by uint8_t's
                    Return 4

                Case ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Nanosecond, ColumnType.NullableTimestamp_Second, ColumnType.NullableTime_Microsecond, ColumnType.NullableTime_Millisecond, ColumnType.NullableTime_Nanosecond, ColumnType.NullableTime_Second, ColumnType.Time_Microsecond, ColumnType.Time_Millisecond, ColumnType.Time_Nanosecond, ColumnType.Time_Second, ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Nanosecond, ColumnType.Timestamp_Second
                    Return 8
                Case Else

                    Throw New InvalidOperationException($"Unexpected ColumnType {onDiskType}")
            End Select
        End Function

        <Extension()>
        Public Function CanMapTo(fromType As ColumnType, toType As System.Type, categoryMetadata As String()) As Boolean
            If toType Is GetType(Value) Then Return True

            Dim hasCategoryMetadata = categoryMetadata IsNot Nothing

            If toType Is GetType(String) AndAlso hasCategoryMetadata Then
                If fromType = ColumnType.Int32 OrElse fromType = ColumnType.NullableInt32 Then Return True
            End If

            Dim isToTypeEnum = toType.IsEnum
            Dim isToTypeNullable = Nullable.GetUnderlyingType(toType) IsNot Nothing
            Dim isToTypeNullableEnum = isToTypeNullable AndAlso Nullable.GetUnderlyingType(toType).IsEnum

            If isToTypeEnum OrElse isToTypeNullableEnum Then
                Dim enumType = If(isToTypeEnum, toType, Nullable.GetUnderlyingType(toType))

                If isToTypeEnum Then
                    Dim __ As CategoryEnumMapType
                    If fromType = ColumnType.Int32 AndAlso TryCategoriesMapToEnum(enumType, categoryMetadata, __) Then Return True
                End If

                If isToTypeNullableEnum Then
                    Dim __ As CategoryEnumMapType
                    If (fromType = ColumnType.Int32 OrElse fromType = ColumnType.NullableInt32) AndAlso TryCategoriesMapToEnum(enumType, categoryMetadata, __) Then Return True
                End If
            End If

            Select Case fromType
                Case ColumnType.NullableBinary, ColumnType.Binary
                    Return toType Is GetType(Byte())

                Case ColumnType.Bool
                    Return toType Is GetType(Boolean) OrElse toType Is GetType(Boolean?)

                Case ColumnType.Category
                    ' categories are handled above
                    Return False

                Case ColumnType.Date
                    Return toType Is GetType(Date) OrElse toType Is GetType(Date?) OrElse toType Is GetType(DateTimeOffset) OrElse toType Is GetType(DateTimeOffset?)

                Case ColumnType.Double
                    Return toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Float
                    Return toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Int16
                    Return toType Is GetType(Long) OrElse toType Is GetType(Long?) OrElse toType Is GetType(Integer) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Short) OrElse toType Is GetType(Short?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Uint16
                    Return toType Is GetType(ULong) OrElse toType Is GetType(ULong?) OrElse toType Is GetType(UInteger) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(UShort) OrElse toType Is GetType(UShort?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Int32
                    Return toType Is GetType(Long) OrElse toType Is GetType(Long?) OrElse toType Is GetType(Integer) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Uint32
                    Return toType Is GetType(ULong) OrElse toType Is GetType(ULong?) OrElse toType Is GetType(UInteger) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Int64
                    Return toType Is GetType(Long) OrElse toType Is GetType(Long?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Uint64
                    Return toType Is GetType(ULong) OrElse toType Is GetType(ULong?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Int8
                    Return toType Is GetType(SByte) OrElse toType Is GetType(SByte?) OrElse toType Is GetType(Long) OrElse toType Is GetType(Long?) OrElse toType Is GetType(Integer) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Short) OrElse toType Is GetType(Short?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.Uint8
                    Return toType Is GetType(Byte) OrElse toType Is GetType(Byte?) OrElse toType Is GetType(ULong) OrElse toType Is GetType(ULong?) OrElse toType Is GetType(UInteger) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(UShort) OrElse toType Is GetType(UShort?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableBool
                    Return toType Is GetType(Boolean?)

                Case ColumnType.NullableCategory
                    ' categories are handled above
                    Return False

                Case ColumnType.NullableDate
                    Return toType Is GetType(Date?) OrElse toType Is GetType(DateTimeOffset?)

                Case ColumnType.NullableDouble
                    Return toType Is GetType(Double?)

                Case ColumnType.NullableFloat
                    Return toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableInt16
                    Return toType Is GetType(Long?) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Short?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableUint16
                    Return toType Is GetType(ULong?) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(UShort?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableInt32
                    Return toType Is GetType(Long?) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableUint32
                    Return toType Is GetType(ULong?) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableInt64
                    Return toType Is GetType(Long?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableUint64
                    Return toType Is GetType(ULong?) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableInt8
                    Return toType Is GetType(SByte?) OrElse toType Is GetType(Long?) OrElse toType Is GetType(Integer?) OrElse toType Is GetType(Short?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.NullableUint8
                    Return toType Is GetType(Byte?) OrElse toType Is GetType(ULong?) OrElse toType Is GetType(UInteger?) OrElse toType Is GetType(UShort?) OrElse toType Is GetType(Single) OrElse toType Is GetType(Single?) OrElse toType Is GetType(Double) OrElse toType Is GetType(Double?)

                Case ColumnType.String, ColumnType.NullableString
                    Return toType Is GetType(String)

                Case ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Nanosecond, ColumnType.Timestamp_Second
                    Return toType Is GetType(Date) OrElse toType Is GetType(Date?) OrElse toType Is GetType(DateTimeOffset) OrElse toType Is GetType(DateTimeOffset?)

                Case ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Nanosecond, ColumnType.NullableTimestamp_Second
                    Return toType Is GetType(Date?) OrElse toType Is GetType(DateTimeOffset?)

                Case ColumnType.Time_Microsecond, ColumnType.Time_Millisecond, ColumnType.Time_Nanosecond, ColumnType.Time_Second
                    Return toType Is GetType(TimeSpan) OrElse toType Is GetType(TimeSpan?)

                Case ColumnType.NullableTime_Microsecond, ColumnType.NullableTime_Millisecond, ColumnType.NullableTime_Nanosecond, ColumnType.NullableTime_Second
                    Return toType Is GetType(TimeSpan?)
                Case Else

                    Throw New Exception($"Unexpected ColumnType {fromType}")
            End Select
        End Function

        <Extension()>
        Public Function GetMapType(type As ColumnType) As System.Type
            Select Case type
                Case ColumnType.NullableBinary, ColumnType.Binary
                    Return GetType(Byte())

                Case ColumnType.Bool
                    Return GetType(Boolean)

                Case ColumnType.Category
                    Return GetType([Enum])

                Case ColumnType.Date
                    Return GetType(Date)
                Case ColumnType.Double
                    Return GetType(Double)
                Case ColumnType.Float
                    Return GetType(Single)
                Case ColumnType.Int16
                    Return GetType(Short)
                Case ColumnType.Int32
                    Return GetType(Integer)
                Case ColumnType.Int64
                    Return GetType(Long)
                Case ColumnType.Int8
                    Return GetType(SByte)

                Case ColumnType.Uint16
                    Return GetType(UShort)
                Case ColumnType.Uint32
                    Return GetType(UInteger)
                Case ColumnType.Uint64
                    Return GetType(ULong)
                Case ColumnType.Uint8
                    Return GetType(Byte)

                Case ColumnType.Timestamp_Microsecond, ColumnType.Timestamp_Millisecond, ColumnType.Timestamp_Nanosecond, ColumnType.Timestamp_Second
                    Return GetType(Date)

                Case ColumnType.NullableBool
                    Return GetType(Boolean?)

                Case ColumnType.NullableCategory
                    Return GetType([Enum])

                Case ColumnType.NullableDate
                    Return GetType(Date?)
                Case ColumnType.NullableDouble
                    Return GetType(Double?)
                Case ColumnType.NullableFloat
                    Return GetType(Single?)
                Case ColumnType.NullableInt16
                    Return GetType(Short?)
                Case ColumnType.NullableInt32
                    Return GetType(Integer?)
                Case ColumnType.NullableInt64
                    Return GetType(Long?)
                Case ColumnType.NullableInt8
                    Return GetType(SByte?)

                Case ColumnType.NullableUint16
                    Return GetType(UShort?)
                Case ColumnType.NullableUint32
                    Return GetType(UInteger?)
                Case ColumnType.NullableUint64
                    Return GetType(ULong?)
                Case ColumnType.NullableUint8
                    Return GetType(Byte?)

                Case ColumnType.String, ColumnType.NullableString
                    Return GetType(String)

                Case ColumnType.NullableTimestamp_Microsecond, ColumnType.NullableTimestamp_Millisecond, ColumnType.NullableTimestamp_Nanosecond, ColumnType.NullableTimestamp_Second
                    Return GetType(Date?)

                Case ColumnType.Time_Microsecond, ColumnType.Time_Millisecond, ColumnType.Time_Nanosecond, ColumnType.Time_Second
                    Return GetType(TimeSpan)

                Case ColumnType.NullableTime_Microsecond, ColumnType.NullableTime_Millisecond, ColumnType.NullableTime_Nanosecond, ColumnType.NullableTime_Second
                    Return GetType(TimeSpan?)
                Case Else

                    Throw New Exception($"Unexpected ColumnType {type}")

            End Select
        End Function

        Friend Function TryCategoriesMapToEnum(enumType As System.Type, categories As String(), <Out> ByRef mapType As CategoryEnumMapType) As Boolean
            ' Operating under the assumption that the
            '   the number of entries and categories
            '   is normally small so avoiding a
            '   HashSet alloc is the smart choice.
            ' Could easily cache this result 
            Dim allNames = [Enum].GetNames(enumType)

            If allNames.Length = 0 Then
                mapType = Nothing
                Return False
            End If

            If allNames.Length < categories.Length Then
                mapType = Nothing
                Return False
            End If

            Dim exactNameMatch = True

            For i = 0 To categories.Length - 1
                Dim category = categories(i)

                Dim found = False
                For j = 0 To allNames.Length - 1
                    Dim enumName = allNames(j)
                    If enumName.Equals(category, StringComparison.InvariantCultureIgnoreCase) Then
                        found = True
                        Exit For
                    End If
                Next

                If Not found Then
                    exactNameMatch = False
                End If
            Next

            If exactNameMatch Then
                mapType = CategoryEnumMapType.ByName
                Return True
            End If

            Dim enumIsByte = [Enum].GetUnderlyingType(enumType) Is GetType(Byte)
            Dim enumIsSbyte = [Enum].GetUnderlyingType(enumType) Is GetType(SByte)
            Dim enumIsShort = [Enum].GetUnderlyingType(enumType) Is GetType(Short)
            Dim enumIsUshort = [Enum].GetUnderlyingType(enumType) Is GetType(UShort)
            Dim enumIsInt = [Enum].GetUnderlyingType(enumType) Is GetType(Integer)
            Dim enumIsUint = [Enum].GetUnderlyingType(enumType) Is GetType(UInteger)
            Dim enumIsLong = [Enum].GetUnderlyingType(enumType) Is GetType(Long)
            Dim enumIsUlong = [Enum].GetUnderlyingType(enumType) Is GetType(ULong)

            Dim ordinalMatch = True
            For i = 0 To categories.Length - 1
                Dim ordinal = i
                If enumIsByte Then
                    If Not [Enum].IsDefined(enumType, CByte(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsSbyte Then
                    If Not [Enum].IsDefined(enumType, CSByte(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsShort Then
                    If Not [Enum].IsDefined(enumType, CShort(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsUshort Then
                    If Not [Enum].IsDefined(enumType, CUShort(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsInt Then
                    If Not [Enum].IsDefined(enumType, CInt(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsUint Then
                    If Not [Enum].IsDefined(enumType, CUInt(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsLong Then
                    If Not [Enum].IsDefined(enumType, CLng(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                If enumIsUlong Then
                    If Not [Enum].IsDefined(enumType, CULng(i)) Then
                        ordinalMatch = False
                        Exit For
                    End If
                    Continue For
                End If

                Throw New Exception($"Couldn't make sense of enum type {enumType.Name} to map it")
            Next

            mapType = CategoryEnumMapType.ByOrdinal
            Return ordinalMatch
        End Function
    End Module

    Friend Class ColumnSpec
        Public Property CategoryEnumMap As Dictionary(Of System.Type, CategoryEnumMapType)

        Public Property Name As String
        Public Property Length As Long
        Public Property Type As ColumnType
        Public ReadOnly Property MappedType As System.Type
            Get
                Return Type.GetMapType()
            End Get
        End Property

        Public Property NullBitmaskOffset As Long
        Public Property DataOffset As Long

        Public Property CategoryLevels As String()

        Public Function CanMapTo(type As System.Type) As Boolean
            Return Me.Type.CanMapTo(type, CategoryLevels)
        End Function

        Public Function GetCategoryEnumMap(Of TEnumType)() As CategoryEnumMapType
            If CategoryEnumMap Is Nothing Then Return CategoryEnumMapType.NONE

            Dim enumType As System.Type = GetType(TEnumType)
            enumType = If(Nullable.GetUnderlyingType(enumType), enumType)

            If Not enumType.IsEnum Then Return CategoryEnumMapType.NONE

            SyncLock CategoryEnumMap
                Dim ret As CategoryEnumMapType
                If CategoryEnumMap.TryGetValue(enumType, ret) Then Return ret

                If Not TryCategoriesMapToEnum(enumType, CategoryLevels, ret) Then
                    CategoryEnumMap(enumType) = ret
                End If

                Return ret
            End SyncLock
        End Function
    End Class

    Friend Module DateTimePrecisionTypeExtensionMethods
        <Extension()>
        Public Function MapToDiskType(type As DateTimePrecisionType) As TimeUnit
            Select Case type
                Case DateTimePrecisionType.Microsecond
                    Return TimeUnit.MICROSECOND
                Case DateTimePrecisionType.Millisecond
                    Return TimeUnit.MILLISECOND
                Case DateTimePrecisionType.Nanosecond
                    Return TimeUnit.NANOSECOND
                Case DateTimePrecisionType.Second
                    Return TimeUnit.SECOND
                Case Else
                    Throw New InvalidOperationException($"Unexpected {NameOf(DateTimePrecisionType)}: {type}")
            End Select
        End Function
    End Module
End Namespace
