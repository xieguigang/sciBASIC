Imports System
Imports System.ComponentModel
Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.MyServices.Internal
    Friend Class WebClientCopy
        ' Methods
        Public Sub New(client As WebClient, dialog As ProgressDialog)
            Me.m_WebClient = client
            Me.m_ProgressDialog = dialog
        End Sub

        Private Sub CloseProgressDialog()
            If (Not Me.m_ProgressDialog Is Nothing) Then
                Me.m_ProgressDialog.IndicateClosing()

                If Me.m_ProgressDialog.IsHandleCreated Then
                    Me.m_ProgressDialog.BeginInvoke(New MethodInvoker(AddressOf Me.m_ProgressDialog.CloseDialog))
                Else
                    Me.m_ProgressDialog.Close()
                End If
            End If
        End Sub

        Public Sub DownloadFile(address As Uri, destinationFileName As String)
            If (Not Me.m_ProgressDialog Is Nothing) Then
                Me.m_WebClient.DownloadFileAsync(address, destinationFileName)
                Me.m_ProgressDialog.ShowProgressDialog()
            Else
                Me.m_WebClient.DownloadFile(address, destinationFileName)
            End If
            If ((Not Me.m_ExceptionEncounteredDuringFileTransfer Is Nothing) AndAlso ((Me.m_ProgressDialog Is Nothing) OrElse Not Me.m_ProgressDialog.UserCanceledTheDialog)) Then
                Throw Me.m_ExceptionEncounteredDuringFileTransfer
            End If
        End Sub

        Private Sub InvokeIncrement(progressPercentage As Integer)
            If ((Not Me.m_ProgressDialog Is Nothing) AndAlso Me.m_ProgressDialog.IsHandleCreated) Then
                Dim num As Integer = (progressPercentage - Me.m_Percentage)
                Me.m_Percentage = progressPercentage
                If (num > 0) Then
                    Dim args As Object() = New Object() {num}
                    Me.m_ProgressDialog.BeginInvoke(New DoIncrement(AddressOf Me.m_ProgressDialog.Increment), args)
                End If
            End If
        End Sub

        Private Sub m_ProgressDialog_UserCancelledEvent()
            Me.m_WebClient.CancelAsync()
        End Sub

        Private Sub m_WebClient_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs)
            Try
                If (Not e.Error Is Nothing) Then
                    Me.m_ExceptionEncounteredDuringFileTransfer = e.Error
                End If
                If (Not e.Cancelled AndAlso (e.Error Is Nothing)) Then
                    Me.InvokeIncrement(100)
                End If
            Finally
                Me.CloseProgressDialog()
            End Try
        End Sub

        Private Sub m_WebClient_DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
            Me.InvokeIncrement(e.ProgressPercentage)
        End Sub

        Private Sub m_WebClient_UploadFileCompleted(sender As Object, e As UploadFileCompletedEventArgs)
            Try
                If (Not e.Error Is Nothing) Then
                    Me.m_ExceptionEncounteredDuringFileTransfer = e.Error
                End If
                If (Not e.Cancelled AndAlso (e.Error Is Nothing)) Then
                    Me.InvokeIncrement(100)
                End If
            Finally
                Me.CloseProgressDialog()
            End Try
        End Sub

        Private Sub m_WebClient_UploadProgressChanged(sender As Object, e As UploadProgressChangedEventArgs)
            Dim num As Long = ((e.BytesSent * 100) / e.TotalBytesToSend)
            Me.InvokeIncrement(CInt(num))
        End Sub

        Public Sub UploadFile(sourceFileName As String, address As Uri)
            If (Not Me.m_ProgressDialog Is Nothing) Then
                Me.m_WebClient.UploadFileAsync(address, sourceFileName)
                Me.m_ProgressDialog.ShowProgressDialog()
            Else
                Me.m_WebClient.UploadFile(address, sourceFileName)
            End If
            If ((Not Me.m_ExceptionEncounteredDuringFileTransfer Is Nothing) AndAlso ((Me.m_ProgressDialog Is Nothing) OrElse Not Me.m_ProgressDialog.UserCanceledTheDialog)) Then
                Throw Me.m_ExceptionEncounteredDuringFileTransfer
            End If
        End Sub


        ' Properties
        Private Overridable Property m_ProgressDialog As ProgressDialog
            <CompilerGenerated>
            Get
                Return Me._m_ProgressDialog
            End Get
            <MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated>
            Set(WithEventsValue As ProgressDialog)
                Dim handler As UserHitCancelEventHandler = New UserHitCancelEventHandler(AddressOf Me.m_ProgressDialog_UserCancelledEvent)
                Dim dialog As ProgressDialog = Me._m_ProgressDialog
                If (Not dialog Is Nothing) Then
                    RemoveHandler dialog.UserHitCancel, handler
                End If
                Me._m_ProgressDialog = WithEventsValue
                dialog = Me._m_ProgressDialog
                If (Not dialog Is Nothing) Then
                    AddHandler dialog.UserHitCancel, handler
                End If
            End Set
        End Property

        Private Overridable Property m_WebClient As WebClient
            <CompilerGenerated>
            Get
                Return Me._m_WebClient
            End Get
            <MethodImpl(MethodImplOptions.Synchronized), CompilerGenerated>
            Set(WithEventsValue As WebClient)
                Dim handler As AsyncCompletedEventHandler = New AsyncCompletedEventHandler(AddressOf Me.m_WebClient_DownloadFileCompleted)
                Dim handler2 As DownloadProgressChangedEventHandler = New DownloadProgressChangedEventHandler(AddressOf Me.m_WebClient_DownloadProgressChanged)
                Dim handler3 As UploadFileCompletedEventHandler = New UploadFileCompletedEventHandler(AddressOf Me.m_WebClient_UploadFileCompleted)
                Dim handler4 As UploadProgressChangedEventHandler = New UploadProgressChangedEventHandler(AddressOf Me.m_WebClient_UploadProgressChanged)
                Dim client As WebClient = Me._m_WebClient
                If (Not client Is Nothing) Then
                    RemoveHandler client.DownloadFileCompleted, handler
                    RemoveHandler client.DownloadProgressChanged, handler2
                    RemoveHandler client.UploadFileCompleted, handler3
                    RemoveHandler client.UploadProgressChanged, handler4
                End If
                Me._m_WebClient = WithEventsValue
                client = Me._m_WebClient
                If (Not client Is Nothing) Then
                    AddHandler client.DownloadFileCompleted, handler
                    AddHandler client.DownloadProgressChanged, handler2
                    AddHandler client.UploadFileCompleted, handler3
                    AddHandler client.UploadProgressChanged, handler4
                End If
            End Set
        End Property


        ' Fields
        <CompilerGenerated, AccessedThroughProperty("m_ProgressDialog")>
        Private _m_ProgressDialog As ProgressDialog
        <CompilerGenerated, AccessedThroughProperty("m_WebClient")>
        Private _m_WebClient As WebClient
        Private m_ExceptionEncounteredDuringFileTransfer As Exception
        Private m_Percentage As Integer = 0

        ' Nested Types
        Private Delegate Sub DoIncrement(Increment As Integer)
    End Class
End Namespace

