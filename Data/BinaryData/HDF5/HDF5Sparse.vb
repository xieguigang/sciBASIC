

' 稀疏感知的 HDF5 读取辅助：在基础 HDF5 模块之上提供通用的流式分块读取
' 与 COO 三元组 -> SparseMatrix 的封装，供上层（如 STRaid）按 10x 语义组合使用。

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Microsoft.VisualBasic.Data.IO.HDF5

    ''' <summary>
    ''' 稀疏感知的 HDF5 读取辅助集合。所有方法都避免将整个数据集一次性解压到内存：
    ''' 分块数据集按 chunk 流式枚举，COO 三元组直接构造 <see cref="SparseMatrix"/>。
    ''' </summary>
    Public Module HDF5Sparse

        ''' <summary>
        ''' 按全路径打开一个分块数据集对象（支持 <c>group/subgroup/dataset</c> 形式的深层路径）。
        ''' </summary>
        ''' <param name="file">已打开的 HDF5 文件。</param>
        ''' <param name="path">数据集全路径，例如 <c>feature_slices/0/row</c>。</param>
        ''' <returns>对应的 <see cref="ChunkedDatasetV3"/>，若路径不存在或不是分块数据集则返回空值。</returns>
        <Extension>
        Public Function OpenChunkedDataset(file As HDF5File, path As String) As ChunkedDatasetV3
            Dim reader As HDF5Reader = file.GetObject(path)

            If reader Is Nothing OrElse reader.dataset Is Nothing Then
                Return Nothing
            End If

            If TypeOf reader.dataset Is ChunkedDatasetV3 Then
                Return DirectCast(reader.dataset, ChunkedDatasetV3)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 以 chunk 为单位流式枚举分块数据集的一维数组，内存峰值维持在单 chunk 量级。
        ''' </summary>
        ''' <typeparam name="T">目标元素 .NET 类型，需与 HDF5 元素类型在字节布局上兼容（如 uint32 -> UInteger）。</typeparam>
        ''' <param name="file">已打开的 HDF5 文件。</param>
        ''' <param name="path">数据集全路径。</param>
        ''' <returns>按 chunk 顺序返回的每个 chunk 解压后的一维数组。</returns>
        <Extension>
        Public Iterator Function EnumerateChunkArrays(Of T)(file As HDF5File, path As String) As IEnumerable(Of T())
            Dim ds As ChunkedDatasetV3 = file.OpenChunkedDataset(path)

            If ds Is Nothing Then
                Throw New InvalidOperationException($"dataset '{path}' is not a chunked dataset or does not exist.")
            End If

            Dim reader As HDF5Reader = file.GetObject(path)
            Dim sb As Superblock = reader.superblock

            For Each chunk As T() In ds.EnumerateChunkArrays(Of T)(sb)
                Yield chunk
            Next
        End Function

        ''' <summary>
        ''' 将三个 COO 三元组数据集（row / col / data）按 chunk 流式读取后一次性构造为
        ''' <see cref="SparseMatrix"/>。三个数据集必须等长且同为分块布局。row 与 col 使用相同的索引类型。
        ''' </summary>
        ''' <typeparam name="TIndex">行/列索引的 .NET 类型（需与 HDF5 索引元素类型匹配，如 uint32 -> UInteger）。</typeparam>
        ''' <typeparam name="TValue">值元素的 .NET 类型（需与 HDF5 值元素类型匹配）。</typeparam>
        ''' <param name="file">已打开的 HDF5 文件。</param>
        ''' <param name="rowPath">行索引数据集路径。</param>
        ''' <param name="colPath">列索引数据集路径。</param>
        ''' <param name="dataPath">值数据集路径（计数或表达量）。</param>
        ''' <param name="indexCast">将原始索引类型转换为 <see cref="Integer"/> 的回调（默认直接 CInt，会校验 32 位范围）。</param>
        ''' <param name="valueCast">将原始值类型转换为 <see cref="Double"/> 的回调（默认直接 CDbl）。</param>
        ''' <param name="nrows">可选的行维度上界；缺省时由最大行索引 + 1 推断。</param>
        ''' <param name="ncols">可选的列维度上界；缺省时由最大列索引 + 1 推断。</param>
        ''' <returns>COO 三元组构造的稀疏矩阵。</returns>
        Public Function GetSparseMatrixFromTriplets(Of TIndex, TValue)(
            file As HDF5File,
            rowPath As String,
            colPath As String,
            dataPath As String,
            Optional indexCast As Func(Of TIndex, Integer) = Nothing,
            Optional valueCast As Func(Of TValue, Double) = Nothing,
            Optional nrows As Integer = -1,
            Optional ncols As Integer = -1) As SparseMatrix

            If indexCast Is Nothing Then
                indexCast = Function(v As TIndex)
                                Dim l As Long = CLng(CObj(v))
                                If l < 0 OrElse l > Integer.MaxValue Then
                                    Throw New OverflowException($"index value {l} exceeds 32-bit range.")
                                End If
                                Return CInt(l)
                            End Function
            End If

            If valueCast Is Nothing Then
                valueCast = Function(v) CDbl(CObj(v))
            End If

            Dim rowEnum As IEnumerable(Of TIndex()) = file.EnumerateChunkArrays(Of TIndex)(rowPath)
            Dim colEnum As IEnumerable(Of TIndex()) = file.EnumerateChunkArrays(Of TIndex)(colPath)
            Dim dataEnum As IEnumerable(Of TValue()) = file.EnumerateChunkArrays(Of TValue)(dataPath)

            Dim rows As New List(Of Integer)
            Dim cols As New List(Of Integer)
            Dim vals As New List(Of Double)

            Using rowIter = rowEnum.GetEnumerator()
                Using colIter = colEnum.GetEnumerator()
                    Using dataIter = dataEnum.GetEnumerator()

                        Do While rowIter.MoveNext() AndAlso colIter.MoveNext() AndAlso dataIter.MoveNext()
                            Dim r As TIndex() = rowIter.Current
                            Dim c As TIndex() = colIter.Current
                            Dim d As TValue() = dataIter.Current

                            If r.Length <> c.Length OrElse r.Length <> d.Length Then
                                Throw New InvalidOperationException("COO triplet datasets have mismatched chunk lengths.")
                            End If

                            For i As Integer = 0 To r.Length - 1
                                rows.Add(indexCast(r(i)))
                                cols.Add(indexCast(c(i)))
                                vals.Add(valueCast(d(i)))
                            Next
                        Loop
                    End Using
                End Using
            End Using

            Return New SparseMatrix(rows.ToArray, cols.ToArray, vals.ToArray, nrows, ncols)
        End Function
    End Module
End Namespace
