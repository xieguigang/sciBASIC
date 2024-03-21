
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel

    ''' <summary>
    ''' compress the in-memory buffer data
    ''' </summary>
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

        ''' <summary>
        ''' get data in zip-compressed stream
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' just returns one block of the data, this function works based on the <see cref="Read"/> function.
        ''' </remarks>
        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield Read()
        End Function

        ''' <summary>
        ''' get data in zip-compressed stream
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Read() As Byte()
            Using s As New MemoryStream(data)
                Return ZipStreamExtensions.Zip(s).ToArray
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="zip">the zip data should has the magic header</param>
        ''' <returns></returns>
        Public Shared Function UncompressBuffer(zip As Byte()) As Byte()
            Return ZipStreamExtensions _
                .UnZipStream(zip, noMagic:=False) _
                .ToArray
        End Function

    End Class
End Namespace