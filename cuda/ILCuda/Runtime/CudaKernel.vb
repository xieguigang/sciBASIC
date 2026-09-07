Imports System.Runtime.InteropServices

Namespace Runtime

    ''' <summary>
    ''' 一个 CUDA 内核函数
    ''' </summary>
    Public NotInheritable Class CudaKernel

        Private ReadOnly _function As IntPtr

        Friend Sub New(name As String, handle As IntPtr)
            Me.Name = name
            _function = handle
        End Sub

        Public ReadOnly Property Name As String

        ''' <summary>内核函数句柄（CUfunction）</summary>
        Public ReadOnly Property Handle As IntPtr
            Get
                Return _function
            End Get
        End Property

        ''' <summary>在 NULL 流上启动内核（二维 grid / 二维 block）</summary>
        Public Sub Launch(gridX As Integer, gridY As Integer,
                         blockX As Integer, blockY As Integer,
                         sharedMemBytes As Integer,
                         ParamArray args As Object())
            LaunchCore(IntPtr.Zero, gridX, gridY, blockX, blockY, sharedMemBytes, args)
        End Sub

        ''' <summary>在指定流上启动内核（二维 grid / 二维 block）</summary>
        Public Sub Launch(stream As CudaStream,
                         gridX As Integer, gridY As Integer,
                         blockX As Integer, blockY As Integer,
                         sharedMemBytes As Integer,
                         ParamArray args As Object())
            LaunchCore(StreamHandle(stream), gridX, gridY, blockX, blockY, sharedMemBytes, args)
        End Sub

        ''' <summary>在 NULL 流上按启动配置启动内核</summary>
        Public Sub Launch(config As LaunchConfig, ParamArray args As Object())
            LaunchCore(IntPtr.Zero, config.GridX, config.GridY, config.BlockX, config.BlockY,
                       config.SharedMemBytes, args)
        End Sub

        ''' <summary>在指定流上按启动配置启动内核</summary>
        Public Sub Launch(stream As CudaStream, config As LaunchConfig, ParamArray args As Object())
            LaunchCore(StreamHandle(stream), config.GridX, config.GridY, config.BlockX, config.BlockY,
                       config.SharedMemBytes, args)
        End Sub

        ''' <summary>一维启动的便捷重载（NULL 流）</summary>
        Public Sub Launch1D(totalElements As Integer, blockSize As Integer,
                           sharedMemBytes As Integer, ParamArray args As Object())
            Dim config = LaunchPlanner.For1D(totalElements, blockSize)
            LaunchCore(IntPtr.Zero, config.GridX, config.GridY, config.BlockX, config.BlockY,
                       sharedMemBytes, args)
        End Sub

        ''' <summary>一维启动的便捷重载（指定流）</summary>
        Public Sub Launch1D(stream As CudaStream, totalElements As Integer, blockSize As Integer,
                           sharedMemBytes As Integer, ParamArray args As Object())
            Dim config = LaunchPlanner.For1D(totalElements, blockSize)
            LaunchCore(StreamHandle(stream), config.GridX, config.GridY, config.BlockX, config.BlockY,
                       sharedMemBytes, args)
        End Sub

        Private Shared Function StreamHandle(stream As CudaStream) As IntPtr
            Return If(stream Is Nothing, IntPtr.Zero, stream.Handle)
        End Function

        Private Sub LaunchCore(streamHandle As IntPtr,
                               gridX As Integer, gridY As Integer,
                               blockX As Integer, blockY As Integer,
                               sharedMemBytes As Integer,
                               args As Object())
            If gridX <= 0 OrElse gridY <= 0 Then Throw New ArgumentOutOfRangeException("grid")
            If blockX <= 0 OrElse blockY <= 0 Then Throw New ArgumentOutOfRangeException("block")

            Dim slot As IntPtr = IntPtr.Zero
            Dim params As IntPtr() = Nothing

            If args IsNot Nothing AndAlso args.Length > 0 Then
                slot = Marshal.AllocHGlobal(IntPtr.Size * args.Length)
                params = New IntPtr(args.Length - 1) {}

                For i As Integer = 0 To args.Length - 1
                    WriteArgument(slot, i, args(i))
                    params(i) = IntPtr.Add(slot, i * IntPtr.Size)
                Next
            End If

            Try
                CudaDriverApi.Check(
                    CudaDriverApi.cuLaunchKernel(_function,
                                                 CUInt(gridX), CUInt(gridY), 1UI,
                                                 CUInt(blockX), CUInt(blockY), 1UI,
                                                 CUInt(sharedMemBytes),
                                                 streamHandle,
                                                 params,
                                                 Nothing), $"cuLaunchKernel({Name})")
            Finally
                If slot <> IntPtr.Zero Then Marshal.FreeHGlobal(slot)
            End Try
        End Sub

        ''' <summary>
        ''' 把托管参数写入参数槽。
        ''' 内核参数本质上是一组 8 字节槽位，指针写 8 字节，
        ''' 标量按自身长度写入（小端机器上低 4 字节即 float 的位模式）。
        ''' </summary>
        Private Shared Sub WriteArgument(slot As IntPtr, index As Integer, value As Object)
            Dim offset = index * IntPtr.Size

            If value Is Nothing Then
                Marshal.WriteIntPtr(slot, offset, IntPtr.Zero)

            ElseIf TypeOf value Is DeviceMemory Then
                Dim devicePtr = DirectCast(value, DeviceMemory).Pointer
                Marshal.WriteIntPtr(slot, offset, New IntPtr(CLng(devicePtr)))

            ElseIf TypeOf value Is PinnedHostBuffer(Of Single) Then
                Marshal.WriteIntPtr(slot, offset, DirectCast(value, PinnedHostBuffer(Of Single)).Pointer)

            ElseIf TypeOf value Is Single Then
                Marshal.WriteInt32(slot, offset, BitConverter.SingleToInt32Bits(CSng(value)))

            ElseIf TypeOf value Is Double Then
                Marshal.WriteInt64(slot, offset, BitConverter.DoubleToInt64Bits(CDbl(value)))

            ElseIf TypeOf value Is Integer Then
                Marshal.WriteInt32(slot, offset, CInt(value))

            ElseIf TypeOf value Is Long Then
                Marshal.WriteInt64(slot, offset, CLng(value))

            ElseIf TypeOf value Is IntPtr Then
                Marshal.WriteIntPtr(slot, offset, DirectCast(value, IntPtr))

            Else
                Throw New ArgumentException(
                    $"内核参数不支持类型 {value.GetType().FullName}，请使用 DeviceBuffer/Single/Integer/Long/IntPtr")
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace
