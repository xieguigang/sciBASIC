#Region "Microsoft.VisualBasic::add545d86d74eae42b12648d31b9399d, cuda\ILCudaTensor\GPUTensor\SparseCsrCache.vb"

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

    '   Total Lines: 189
    '    Code Lines: 124 (65.61%)
    ' Comment Lines: 32 (16.93%)
    '    - Xml Docs: 40.62%
    ' 
    '   Blank Lines: 33 (17.46%)
    '     File Size: 7.76 KB


    '     Class SparseCsrCache
    ' 
    '         Properties: Bytes, Count
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ByteSizeOf, GetBuffers
    ' 
    '         Sub: Clear, Dispose, Evict
    '         Class Entry
    ' 
    '             Properties: ByteSize, Ticks
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Finalize, Release
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' SparseCsrCache —— CSR 稀疏矩阵的显存驻留缓存（LRU）
'
' 目的：CSR 连接矩阵（rowPtr / colIdx / values 三个数组）在一次仿真中会被反复
' 使用（每个时间步一次 SpMM），若每次都重新上传显存，PCIe 往返会成为瓶颈。
' 这里把它们常驻显存，供 <see cref="CudaTensor.SpMM"/> 直接引用。
'
' 缓存键 = <see cref="SparseCsr"/> 实例的**引用身份** + <see cref="SparseCsr.Version"/>：
'   * SparseCsr 刻意不覆写 Equals/GetHashCode，故 Dictionary 的键就是引用相等；
'   * 宿主就地修改权重（例如归一化）后必须调用 SparseCsr.MarkModified()，
'     版本号变化会触发淘汰并重新上传，避免显存复用旧权重。
'
' 重要：ILCuda 的显存没有终结器，依赖 GC 永远无法回收。因此这里与
' <see cref="DeviceCache(Of T)"/> 采用同样的显式 LRU：
'   * 超出容量时立即 Dispose 被淘汰的条目；
'   * Clear() 在 CudaTensor.Dispose 时逐条释放；
'   * Entry 保留 Finalize() 作为异常路径兜底。
' ---------------------------------------------------------------------------

Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime

Namespace GPUTensor

    Friend NotInheritable Class SparseCsrCache
        Implements IDisposable

        ''' <summary>单条缓存项：CSR 三个数组的显存缓冲 + 上传时的版本 + LRU 时间戳</summary>
        Friend NotInheritable Class Entry

            Public ReadOnly RowPtr As ILCudaRuntime.DeviceBuffer(Of Integer)
            Public ReadOnly ColIdx As ILCudaRuntime.DeviceBuffer(Of Integer)
            Public ReadOnly Values As ILCudaRuntime.DeviceBuffer(Of Double)
            Public ReadOnly Version As Long
            Public Property Ticks As Long

            Public Sub New(rowPtr As ILCudaRuntime.DeviceBuffer(Of Integer),
                           colIdx As ILCudaRuntime.DeviceBuffer(Of Integer),
                           values As ILCudaRuntime.DeviceBuffer(Of Double),
                           version As Long, ticks As Long)
                Me.RowPtr = rowPtr
                Me.ColIdx = colIdx
                Me.Values = values
                Me.Version = version
                Me.Ticks = ticks
            End Sub

            ''' <summary>本条目占用字节数</summary>
            Public ReadOnly Property ByteSize As Long
                Get
                    Return CLng(RowPtr.Count) * RowPtr.ElementSize +
                           CLng(ColIdx.Count) * ColIdx.ElementSize +
                           CLng(Values.Count) * Values.ElementSize
                End Get
            End Property

            ''' <summary>释放三个显存缓冲</summary>
            Public Sub Release()
                RowPtr.Dispose()
                ColIdx.Dispose()
                Values.Dispose()
            End Sub

            ''' <summary>兜底释放（正常情况下都会走显式 Release）</summary>
            Protected Overrides Sub Finalize()
                Try
                    Release()
                Catch
                End Try
            End Sub

        End Class

        Private ReadOnly _map As New Dictionary(Of tfCompute.SparseCsr, Entry)()
        Private ReadOnly _sync As New Object()
        Private ReadOnly _capacity As Long
        Private _bytes As Long
        Private _ticks As Long

        ''' <param name="capacityBytes">显存容量上限（字节）；&lt;= 0 表示不限制</param>
        Public Sub New(capacityBytes As Long)
            _capacity = capacityBytes
        End Sub

        ''' <summary>已驻留的字节数</summary>
        Public ReadOnly Property Bytes As Long
            Get
                Return _bytes
            End Get
        End Property

        ''' <summary>当前驻留的条目个数</summary>
        Public ReadOnly Property Count As Integer
            Get
                SyncLock _sync
                    Return _map.Count
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' 取得 CSR 对应的显存缓冲；版本不一致时淘汰旧条目并重新上传。
        ''' </summary>
        ''' <remarks>调用方须保证 <c>csr.NonZeros &gt; 0</c>（零非零元素无法分配显存缓冲）。</remarks>
        Public Function GetBuffers(engine As ILCudaRuntime.CudaEngine, csr As tfCompute.SparseCsr) As Entry
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
            If csr Is Nothing Then Throw New ArgumentNullException(NameOf(csr))
            If csr.NonZeros <= 0 Then
                Throw New ArgumentException("CSR 非零元素为 0，无法分配显存缓冲", NameOf(csr))
            End If

            SyncLock _sync
                Dim entry As Entry = Nothing

                If _map.TryGetValue(csr, entry) AndAlso entry.Version = csr.Version Then
                    _ticks += 1
                    entry.Ticks = _ticks
                    Return entry
                End If

                ' 版本失效：淘汰旧条目并释放显存
                If entry IsNot Nothing Then
                    _map.Remove(csr)
                    _bytes -= entry.ByteSize
                    entry.Release()
                End If

                Dim need As Long = ByteSizeOf(csr)
                Evict(need)

                Dim rowPtrBuf As New ILCudaRuntime.DeviceBuffer(Of Integer)(csr.RowPointers.Length)
                Dim colIdxBuf As New ILCudaRuntime.DeviceBuffer(Of Integer)(csr.ColumnIndices.Length)
                Dim valuesBuf As New ILCudaRuntime.DeviceBuffer(Of Double)(csr.Values.Length)

                rowPtrBuf.Write(csr.RowPointers)
                colIdxBuf.Write(csr.ColumnIndices)
                valuesBuf.Write(csr.Values)

                _ticks += 1
                Dim created As New Entry(rowPtrBuf, colIdxBuf, valuesBuf, csr.Version, _ticks)
                _map(csr) = created
                _bytes += need

                Return created
            End SyncLock
        End Function

        Private Shared Function ByteSizeOf(csr As tfCompute.SparseCsr) As Long
            Return CLng(csr.RowPointers.Length) * 4L +
                   CLng(csr.ColumnIndices.Length) * 4L +
                   CLng(csr.Values.Length) * 8L
        End Function

        ''' <summary>按 LRU 淘汰直到腾出需要的空间</summary>
        Private Sub Evict(needBytes As Long)
            If _capacity <= 0 Then Return
            If _bytes + needBytes <= _capacity Then Return

            Dim snapshot = _map.ToArray()
            Array.Sort(snapshot, Function(x, y) x.Value.Ticks.CompareTo(y.Value.Ticks))

            For Each pair In snapshot
                If _bytes + needBytes <= _capacity Then Exit For

                _map.Remove(pair.Key)
                _bytes -= pair.Value.ByteSize
                pair.Value.Release()
            Next
        End Sub

        ''' <summary>释放全部驻留的显存缓冲</summary>
        Public Sub Clear()
            SyncLock _sync
                For Each entry In _map.Values
                    entry.Release()
                Next

                _map.Clear()
                _bytes = 0
            End SyncLock
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Clear()
        End Sub

    End Class

End Namespace
