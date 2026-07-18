#Region "Microsoft.VisualBasic::6173baf499397aef48d1513a13c9164b, Data\BinaryData\netCDF\Data\DataReader.vb"

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

    '   Total Lines: 105
    '    Code Lines: 55 (52.38%)
    ' Comment Lines: 36 (34.29%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 14 (13.33%)
    '     File Size: 4.82 KB


    '     Module DataReader
    ' 
    '         Function: CreateArray, dimProduct, nonRecord, record
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.netCDF.Components
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Data

    ''' <summary>
    ''' Data reader methods for a given variable data value.
    ''' (在这个模块之中读取<see cref="variable.value"/>数据变量的值)
    ''' </summary>
    Module DataReader

        <Extension>
        Private Function CreateArray(x As variable, size As Integer) As Array
            Select Case x.type
                Case CDFDataTypes.BOOLEAN : Return New Boolean(size - 1) {}
                Case CDFDataTypes.NC_BYTE, CDFDataTypes.NC_UBYTE : Return New Byte(size - 1) {}
                Case CDFDataTypes.NC_CHAR, CDFDataTypes.NC_STRING : Return New Char(size - 1) {}
                Case CDFDataTypes.NC_DOUBLE : Return New Double(size - 1) {}
                Case CDFDataTypes.NC_FLOAT : Return New Single(size - 1) {}
                Case CDFDataTypes.NC_INT, CDFDataTypes.NC_USHORT : Return New Integer(size - 1) {}
                Case CDFDataTypes.NC_INT64, CDFDataTypes.NC_UINT, CDFDataTypes.NC_UINT64 : Return New Long(size - 1) {}
                Case CDFDataTypes.NC_SHORT : Return New Short(size - 1) {}
                Case Else
                    Throw New InvalidDataException("invalid data type!")
            End Select
        End Function

        ''' <summary>
        ''' Product of the sizes of the given dimension ids.
        ''' </summary>
        Private Function dimProduct(dimensions As Dimension(), ids As Integer()) As Long
            Dim p As Long = 1

            For Each id As Integer In ids
                p *= dimensions(id).size
            Next

            Return p
        End Function

        ''' <summary>
        ''' Read data for the given non-record variable
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <param name="variable">Variable metadata</param>
        ''' <returns>Data of the element</returns>
        ''' <remarks>
        ''' 非记录类型则是一个数组
        ''' </remarks>
        Public Function nonRecord(buffer As BinaryDataReader, variable As variable, dimensions As Dimension()) As Array
            ' Real element count = product of all dimension sizes.
            ' For numeric types this equals varSize / elementSize; for
            ' char/byte variables the stored vsize may include 4-byte
            ' padding, so we must use the dimension product to avoid
            ' reading the padding bytes.
            Dim realCount As Integer = CInt(dimProduct(dimensions, variable.dimensions))

            If realCount <= 0 Then
                realCount = CInt(variable.size \ sizeof(variable.type))
            End If

            Return buffer.readVector(realCount, variable.type)
        End Function

        ''' <summary>
        ''' Read data for the given record variable
        ''' (读取结构体数据?)
        ''' </summary>
        ''' <param name="buffer">Buffer for the file data</param>
        ''' <param name="variable">Variable metadata</param>
        ''' <param name="recordDimension">Record dimension metadata</param>
        ''' <returns>Data of the element</returns>
        ''' <remarks>
        ''' 记录类型的数据可能是一个矩阵类型
        ''' </remarks>
        Public Function record(buffer As BinaryDataReader, variable As variable, recordDimension As recordDimension, dimensions As Dimension()) As Array
            ' Elements per record slab = product of the non-record
            ' dimension sizes (all dimensions except the leading record dim).
            Dim perRecordElems As Integer = 1

            For i As Integer = 1 To variable.dimensions.Length - 1
                perRecordElems *= CInt(dimensions(variable.dimensions(i)).size)
            Next

            Dim numRecords As Integer = CInt(recordDimension.length)
            Dim total As Integer = perRecordElems * numRecords
            Dim data As Array = CreateArray(variable, total)
            Dim [step] As Long = recordDimension.recordStep

            ' Record variables are stored interleaved in the file: the r-th
            ' record of this variable starts at (offset + r * recordStep).
            For r As Integer = 0 To numRecords - 1
                buffer.Seek(variable.offset + r * [step], SeekOrigin.Begin)
                Dim slab As Array = buffer.readVector(perRecordElems, variable.type)
                Array.Copy(slab, 0, data, r * perRecordElems, perRecordElems)
            Next

            Return data
        End Function
    End Module
End Namespace
