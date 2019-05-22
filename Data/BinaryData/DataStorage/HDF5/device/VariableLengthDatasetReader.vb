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
        ''' <param name="buffer"></param>
        ''' <param name="dimensions%"></param>
        ''' <returns></returns>
        <Extension>
        Public Function readDataSet(type As VariableLength, buffer As BinaryReader, dimensions%(), sb As Superblock) As Object
            Dim data As Object
            Dim isScalar As Boolean

            If dimensions.Length = 0 Then
                isScalar = True
                dimensions = {1}
            Else
                isScalar = False
            End If

            Dim charset As Encoding = type.encoding
            Dim objects As New List(Of Object)

            For Each globalHeapId As GlobalHeapId In getGlobalHeapIds(buffer, type.size, dimensions.totalPoints)
                Dim heap As New LocalHeap(buffer, sb, globalHeapId.heapAddress)

            Next
        End Function

        Private Iterator Function getGlobalHeapIds(buffer As BinaryReader, length%, datasetTotalSize%) As IEnumerable(Of GlobalHeapId)

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