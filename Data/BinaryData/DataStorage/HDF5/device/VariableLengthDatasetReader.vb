Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports Microsoft.VisualBasic.Math

Namespace HDF5.device

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
        Public Function readDataSet(type As VariableLength, dimensions%(), sb As Superblock) As Object
            Dim data As String() = New String() {}
            Dim isScalar As Boolean

            If dimensions.Length = 0 Then
                isScalar = True
                dimensions = {1}
            Else
                isScalar = False
            End If

            Dim charset As Encoding = type.encoding
            Dim objects As New List(Of String)

            For Each globalHeapId As GlobalHeapId In getGlobalHeapIds(sb, type.size, dimensions.totalPoints)
                Dim heap As GlobalHeap = sb.globalHeaps.ComputeIfAbsent(globalHeapId.heapAddress, Function(address) New GlobalHeap(sb, address))
                Dim cache = heap.objects(globalHeapId.index)
                Dim element As String = charset.GetString(cache.data)

                objects.Add(element)
            Next

            ' Make the output array
            fillData(data, dimensions, objects.GetEnumerator())

            If isScalar Then
                Return data.GetValue(Scan0)
            Else
                Return data
            End If
        End Function

        Private Iterator Function getGlobalHeapIds(sb As Superblock, length%, datasetTotalSize%) As IEnumerable(Of GlobalHeapId)
            ' For variable length datasets the actual data is in the global heap so need to
            ' resolve that then build the buffer.

            ' final int skipBytes = length - hdfFc.getSizeOfOffsets() - 4;
            Dim skipBytes As Integer = length - sb.sizeOfOffsets - 4
            Dim buffer = sb.file.reader
            Dim size As Integer = 0

            ' id=4
            While size <= length
                ' Move past the skipped bytes. TODO figure out what this is for
                buffer.offset += skipBytes
                size += skipBytes

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