Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' SimpleTcp server events.
    ''' </summary>
    Public Class SimpleTcpServerEvents
#Region "Public-Members"

        ''' <summary>
        ''' Event to call when a client connects.
        ''' </summary>
        Public Event ClientConnected As EventHandler(Of ConnectionEventArgs)

        ''' <summary>
        ''' Event to call when a client disconnects.
        ''' </summary>
        Public Event ClientDisconnected As EventHandler(Of ConnectionEventArgs)

        ''' <summary>
        ''' Event to call when byte data has become available from the client.
        ''' </summary>
        Public Event DataReceived As EventHandler(Of DataReceivedEventArgs)

        ''' <summary>
        ''' Event to call when byte data has been sent to a client.
        ''' </summary>
        Public Event DataSent As EventHandler(Of DataSentEventArgs)

#End Region

#Region "Constructors-and-Factories"

        ''' <summary>
        ''' Instantiate the object.
        ''' </summary>
        Public Sub New()

        End Sub

#End Region

#Region "Public-Methods"

        Friend Sub HandleClientConnected(sender As Object, args As ConnectionEventArgs)
            RaiseEvent ClientConnected(sender, args)
        End Sub

        Friend Sub HandleClientDisconnected(sender As Object, args As ConnectionEventArgs)
            RaiseEvent ClientDisconnected(sender, args)
        End Sub

        Friend Sub HandleDataReceived(sender As Object, args As DataReceivedEventArgs)
            RaiseEvent DataReceived(sender, args)
        End Sub

        Friend Sub HandleDataSent(sender As Object, args As DataSentEventArgs)
            RaiseEvent DataSent(sender, args)
        End Sub

#End Region
    End Class
End Namespace
