Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.IOHandler
Imports Microsoft.VisualBasic.Text

Namespace FileIO

    Public Class IOStream(Of T)

        Shared ReadOnly save As ISave

        Dim data As IEnumerable(Of T)

        Shared Sub New()

        End Sub

        Sub New(data As IEnumerable(Of T))
            Me.data = data
        End Sub

    End Class

    Public Module IOStreamExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsIOStream(Of T)(data As IEnumerable(Of T)) As IOStream(Of T)
            Return New IOStream(Of T)(data)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function FileOpen(path As String, Optional encoding As Encodings = Encodings.UTF8) As Integer
            Return My.File.OpenHandle(path, encoding)
        End Function
    End Module
End Namespace