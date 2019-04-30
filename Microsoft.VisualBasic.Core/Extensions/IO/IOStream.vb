Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.IOHandler
Imports Microsoft.VisualBasic.Text

Namespace FileIO

    Public Class IOStream(Of T)

        Shared ReadOnly save As ISave
        Shared ReadOnly obj As Type = GetType(T)

        Dim data As Object

        Shared Sub New()
            If IOHandler.IsRegister(obj) Then
                save = IOHandler.GetWrite(obj)
            Else
                save = IOHandler.GetWrite(GetType(IEnumerable(Of T)))
            End If
        End Sub

        Sub New(data As IEnumerable(Of T))
            Me.data = data
        End Sub

        Sub New(data As T)
            Me.data = data
        End Sub

        Public Shared Operator >>(stream As IOStream(Of T), handle%) As Boolean
            Return IOStream(Of T).save(stream.data, My.File.GetHandle(handle).FileName, DefaultEncoding)
        End Operator

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