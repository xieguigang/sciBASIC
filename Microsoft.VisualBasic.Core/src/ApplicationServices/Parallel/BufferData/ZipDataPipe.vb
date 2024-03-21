
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel

    Public Class ZipDataPipe : Inherits DataPipe

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Byte))
            Call MyBase.New(data)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(str As String)
            Call MyBase.New(str)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As IEnumerable(Of Double))
            Call MyBase.New(data)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As RawStream)
            Call MyBase.New(data.Serialize)
        End Sub

        ''' <summary>
        ''' extract all bytes data from the input <see cref="MemoryStream"/> for construct a new data package
        ''' </summary>
        ''' <param name="data"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Sub New(data As MemoryStream)
            Call MyBase.New(data.ToArray)
        End Sub

    End Class
End Namespace