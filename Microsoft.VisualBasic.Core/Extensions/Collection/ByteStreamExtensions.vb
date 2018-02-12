Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.IteratorExtensions

Public Module ByteStreamExtensions

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function UTF8String(stream As IEnumerable(Of Byte)) As String
        Return Encoding.UTF8.GetString(stream.ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function UnicodeString(stream As IEnumerable(Of Byte)) As String
        Return Encoding.Unicode.GetString(stream.ToArray)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsciiString(stream As IEnumerable(Of Byte)) As String
        Return Encoding.ASCII.GetString(stream.ToArray)
    End Function
End Module
