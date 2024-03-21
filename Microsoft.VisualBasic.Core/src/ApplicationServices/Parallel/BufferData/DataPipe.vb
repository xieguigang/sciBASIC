Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel

    ''' <summary>
    ''' a <see cref="MemoryStream"/> liked in-memory data
    ''' </summary>
    Public Class DataPipe : Inherits BufferPipe
        Implements IDisposable

        ''' <summary>
        ''' the in-memory data
        ''' </summary>
        Protected data As Byte()

        Dim disposedValue As Boolean

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Byte))
            Me.data = data.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(str As String)
            Me.data = Encoding.UTF8.GetBytes(str)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Double))
            Me.data = data _
                .Select(Function(mzi) BitConverter.GetBytes(mzi)) _
                .IteratesALL _
                .ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As RawStream)
            Call Me.New(data.Serialize)
        End Sub

        ''' <summary>
        ''' extract all bytes data from the input <see cref="MemoryStream"/> for construct a new data package
        ''' </summary>
        ''' <param name="data"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As MemoryStream)
            Call Me.New(data.ToArray)
        End Sub

        Public Overrides Function ToString() As String
            Return $"buffer_data({StringFormats.Lanudry(bytes:=data.Length)})"
        End Function

        ''' <summary>
        ''' get data in block formats
        ''' </summary>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield data
        End Function

        ''' <summary>
        ''' get all data
        ''' </summary>
        ''' <returns></returns>
        <DebuggerStepThrough>
        Public Overrides Function Read() As Byte()
            Return data
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Erase data
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并替代终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace