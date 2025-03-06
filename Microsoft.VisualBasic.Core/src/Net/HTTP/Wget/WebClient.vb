
Imports System.IO

Namespace Net.WebClient

    Public MustInherit Class WebClient

        Public Event DownloadProgressChanged As EventHandler(Of ProgressChangedEventArgs)
        Public Event DownloadCompleted As EventHandler

        Public MustOverride ReadOnly Property LocalSaveFile As String
        Public MustOverride Async Function DownloadFileAsync() As Task

        ''' <summary>
        ''' open save stream
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function OpenSaveStream() As Stream

        Protected Sub ProgressUpdate(args As ProgressChangedEventArgs)
            RaiseEvent DownloadProgressChanged(Me, args)
        End Sub

        Protected Sub ProgressFinished()
            RaiseEvent DownloadCompleted(Me, EventArgs.Empty)
        End Sub

    End Class

    Public Class ProgressChangedEventArgs
        Inherits EventArgs

        Public ReadOnly Property ProgressPercentage As Integer
        Public ReadOnly Property BytesReceived As Long
        Public ReadOnly Property TotalBytes As Long

        Friend Sub New(bytesReceived As Long, totalBytes As Long)
            Me.TotalBytes = totalBytes
            Me.BytesReceived = bytesReceived

            If totalBytes > 0 Then
                Me.ProgressPercentage = CInt((bytesReceived * 100) / totalBytes)
            End If
        End Sub
    End Class
End Namespace