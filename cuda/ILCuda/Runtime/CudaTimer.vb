#Region "Microsoft.VisualBasic::3744505969714b2fbbfec83087208d04, cuda\ILCuda\Runtime\CudaTimer.vb"

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

    '   Total Lines: 56
    '    Code Lines: 38 (67.86%)
    ' Comment Lines: 7 (12.50%)
    '    - Xml Docs: 57.14%
    ' 
    '   Blank Lines: 11 (19.64%)
    '     File Size: 2.05 KB


    '     Class CudaTimer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Finish
    ' 
    '         Sub: Dispose, Start
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 基于 CUDA event 的 GPU 侧计时器
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>
    ''' 使用 cuEvent 精确测量一段 GPU 任务（内核序列）的执行耗时
    ''' </summary>
    Public NotInheritable Class CudaTimer
        Implements IDisposable

        Private ReadOnly _start As IntPtr
        Private ReadOnly _end As IntPtr
        Private _disposed As Boolean

        Public Sub New()
            Dim a As IntPtr = IntPtr.Zero
            Dim b As IntPtr = IntPtr.Zero

            CudaDriverApi.Check(CudaDriverApi.cuEventCreate(a, 0UI), "cuEventCreate")
            CudaDriverApi.Check(CudaDriverApi.cuEventCreate(b, 0UI), "cuEventCreate")

            _start = a
            _end = b
        End Sub

        Public Sub Start()
            CudaDriverApi.Check(CudaDriverApi.cuEventRecord(_start, IntPtr.Zero), "cuEventRecord")
        End Sub

        ''' <summary>记录结束事件并等待其完成，返回毫秒数</summary>
        Public Function Finish() As Double
            CudaDriverApi.Check(CudaDriverApi.cuEventRecord(_end, IntPtr.Zero), "cuEventRecord")
            CudaDriverApi.Check(CudaDriverApi.cuEventSynchronize(_end), "cuEventSynchronize")

            Dim ms As Single = 0
            CudaDriverApi.Check(CudaDriverApi.cuEventElapsedTime(ms, _start, _end), "cuEventElapsedTime")
            Return CDbl(ms)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True

            Try
                CudaDriverApi.Check(CudaDriverApi.cuEventDestroy_v2(_start), "cuEventDestroy_v2")
            Catch
            End Try
            Try
                CudaDriverApi.Check(CudaDriverApi.cuEventDestroy_v2(_end), "cuEventDestroy_v2")
            Catch
            End Try
        End Sub
    End Class
End Namespace
