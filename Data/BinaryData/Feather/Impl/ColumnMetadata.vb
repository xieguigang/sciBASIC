#Region "Microsoft.VisualBasic::dad9164aa66aae53107dfcef543f7167, Data\BinaryData\Feather\Impl\ColumnMetadata.vb"

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

    '   Total Lines: 72
    '    Code Lines: 53
    ' Comment Lines: 3
    '   Blank Lines: 16
    '     File Size: 3.85 KB


    '     Class ColumnMetadata
    ' 
    '         Properties: Encoding, Length, Levels, Name, NullCount
    '                     Offset, Ordered, TimeZone, TotalBytes, Type
    '                     Unit
    ' 
    '         Sub: CreateMetadata
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.DataStorage.FeatherFormat.Impl.FbsMetadata

Namespace Impl
    Friend Class ColumnMetadata
        Public Property Name As String

        ' values
        Public Property Type As ColumnType
        Public Property Encoding As FbsMetadata.Encoding
        Public Property Offset As Long
        Public Property Length As Long
        Public Property NullCount As Long
        Public Property TotalBytes As Long

        ' metadata
        Public Property Levels As String()                ' set in Category
        Public Property Ordered As Boolean                   ' set in Category
        Public Property Unit As DateTimePrecisionType     ' set in Time and Timestamp
        Public Property TimeZone As String                ' set in Timestamp

        ' not implemented: user_metadata (see: https://github.com/wesm/feather/blob/master/cpp/src/feather/metadata.fbs#L106)

        Public Sub CreateMetadata(builder As FlatBuffers.FlatBufferBuilder, writer As FeatherWriter, <Out> ByRef metadataOffset As Integer, <Out> ByRef metadata As TypeMetadata, <Out> ByRef categoryLevels As FlatBuffers.Offset(Of PrimitiveArray))
            Dim isCategoryType = Type = ColumnType.Category OrElse Type = ColumnType.NullableCategory

            Dim isDateType = Type = ColumnType.Date OrElse Type = ColumnType.NullableDate

            Dim isTimeType = Type = ColumnType.Time_Microsecond OrElse Type = ColumnType.Time_Millisecond OrElse Type = ColumnType.Time_Second OrElse Type = ColumnType.Time_Nanosecond OrElse Type = ColumnType.NullableTime_Microsecond OrElse Type = ColumnType.NullableTime_Millisecond OrElse Type = ColumnType.NullableTime_Second OrElse Type = ColumnType.NullableTime_Nanosecond

            Dim isTimestampType = Type = ColumnType.Timestamp_Microsecond OrElse Type = ColumnType.Timestamp_Millisecond OrElse Type = ColumnType.Timestamp_Second OrElse Type = ColumnType.Timestamp_Nanosecond OrElse Type = ColumnType.NullableTimestamp_Microsecond OrElse Type = ColumnType.NullableTimestamp_Millisecond OrElse Type = ColumnType.NullableTimestamp_Second OrElse Type = ColumnType.NullableTimestamp_Nanosecond

            If isDateType Then
                Throw New InvalidOperationException($"Mapping to a Date on disk doesn't make sense from .NET")
            End If

            If isTimestampType Then
                Dim offsetMeta = TimestampMetadata.CreateTimestampMetadata(builder, Unit.MapToDiskType(), builder.CreateString("GMT"))
                metadataOffset = offsetMeta.Value
                metadata = TypeMetadata.TimestampMetadata
                categoryLevels = Nothing
                Return
            End If

            If isTimeType Then
                Dim offsetMeta = TimeMetadata.CreateTimeMetadata(builder, Unit.MapToDiskType())
                metadataOffset = offsetMeta.Value
                metadata = TypeMetadata.TimeMetadata
                categoryLevels = Nothing
                Return
            End If

            If isCategoryType Then
                Dim startIx As Long
                Dim numBytes As Long
                writer.WriteLevels(Levels, startIx, numBytes)

                categoryLevels = PrimitiveArray.CreatePrimitiveArray(builder, FbsMetadata.Type.UTF8, FbsMetadata.Encoding.PLAIN, startIx, Levels.LongLength, 0, numBytes)

                Dim offsetMeta = CategoryMetadata.CreateCategoryMetadata(builder, categoryLevels, Ordered)

                metadataOffset = offsetMeta.Value
                metadata = TypeMetadata.CategoryMetadata
                Return
            End If

            metadataOffset = 0
            metadata = TypeMetadata.NONE
            categoryLevels = Nothing
        End Sub
    End Class
End Namespace

