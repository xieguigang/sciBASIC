#Region "Microsoft.VisualBasic::61dafd366352290527e7d5873799581c, Data\BinaryData\HDF5\device\datasetReader\VariableLengthDatasetReader.vb"

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

    '   Total Lines: 126
    '    Code Lines: 79 (62.70%)
    ' Comment Lines: 27 (21.43%)
    '    - Xml Docs: 70.37%
    ' 
    '   Blank Lines: 20 (15.87%)
    '     File Size: 5.08 KB


    '     Module VariableLengthDatasetReader
    ' 
    '         Function: getGlobalHeapIds, readDataSet, totalPoints
    ' 
    '         Sub: fillData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

Namespace device

    ''' <summary>
    ''' Read helpers of <see cref="VariableLength"/>
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/jamesmudd/jhdf
    ''' </remarks>
    Module VariableLengthDatasetReader

        ''' <summary>
        ''' 读取一个字符串或者一个字符串数组
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="dimensions%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function readDataSet(type As VariableLength, dimensions%(), sb As Superblock, source As [Variant](Of Long, MemoryReader)) As Object
            Dim data As Array
            Dim isScalar As Boolean

            If dimensions.Length = 0 Then
                isScalar = True
                dimensions = {1}
                data = {""}
            Else
                isScalar = False

                If dimensions.Length = 1 Then
                    data = Array.CreateInstance(GetType(String), dimensions)
                Else
                    data = RectangularArray.Matrix(Of String)(dimensions(Scan0), dimensions(1))
                End If
            End If

            Dim charset As Encoding = type.encoding
            Dim objects As New List(Of String)

            ' 在这里不能够使用linq方法，必须立即读取完，因为reader是公用的
            ' 否则会产生读取bug
            For Each globalHeapId As GlobalHeapId In getGlobalHeapIds(sb, type.size, dimensions.totalPoints, source).ToArray
                Dim heap As GlobalHeap = sb.globalHeaps.ComputeIfAbsent(globalHeapId.heapAddress, Function(address) New GlobalHeap(sb, address))
                Dim cache = heap.objects(globalHeapId.index)
                Dim element As String = charset.GetString(cache.data)

                objects.Add(element)
            Next

            Dim objEnumerates = objects.GetEnumerator

            Call objEnumerates.MoveNext()
            ' Make the output array
            Call fillData(data, dimensions, objEnumerates)

            If isScalar Then
                Return data.GetValue(Scan0)
            Else
                Return data
            End If
        End Function

        ''' <summary>
        ''' 因为reader是共享的，所以在这里因为使用的是迭代器，所以在外层需要使用ToArray立即读取完，否则会因为读取其他数据而导致下一个<see cref="GlobalHeapId"/>出问题
        ''' </summary>
        ''' <param name="sb"></param>
        ''' <param name="length">数据块的总大小</param>
        ''' <param name="datasetTotalSize%"></param>
        ''' <returns></returns>
        Private Iterator Function getGlobalHeapIds(sb As Superblock, length%, datasetTotalSize%, source As [Variant](Of Long, MemoryReader)) As IEnumerable(Of GlobalHeapId)
            ' For variable length datasets the actual data is in the global heap so need to
            ' resolve that then build the buffer.

            ' final int skipBytes = length - hdfFc.getSizeOfOffsets() - 4;
            Dim skipBytes As Integer = length - sb.sizeOfOffsets - 4
            Dim buffer As BinaryReader

            If source Like GetType(Long) Then
                buffer = sb.FileReader(source.TryCast(Of Long))
            Else
                buffer = source.TryCast(Of MemoryReader)
            End If

            Call buffer.Mark()

            ' id=4
            While buffer.deltaSize < length * datasetTotalSize
                ' Move past the skipped bytes. TODO figure out what this is for
                buffer.offset += skipBytes

                Dim heapAddress As Long = device.readO(buffer, sb)
                Dim index As Integer = buffer.readInt
                Dim globalHeapId As New GlobalHeapId(heapAddress, index)

                Yield globalHeapId
            End While
        End Function

        <Extension>
        Private Sub fillData(data As Array, dims%(), objects As IEnumerator(Of String))
            If dims.Length > 1 Then
                For i As Integer = 0 To dims(Scan0) - 1
                    Call DirectCast(data.GetValue(i), Array).fillData(dims.Skip(1).ToArray, objects)
                Next
            Else
                For i As Integer = 0 To dims(Scan0) - 1
                    Call data.SetValue(objects.Current, i)
                    Call objects.MoveNext()
                Next
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function totalPoints(dimensions As Integer()) As Integer
            Return dimensions.ProductALL
        End Function
    End Module
End Namespace
