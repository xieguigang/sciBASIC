Imports System

Namespace SuperSimpleTcp
    ''' <summary>
    ''' SimpleTcp client events.
    ''' </summary>
    Public Class SimpleTcpClientEvents
#Region "Public-Members"

        ''' <summary>
        ''' Event to call when the connection is established.
        ''' </summary>
        Public Event Connected As EventHandler(Of ConnectionEventArgs)

        ''' <summary>
        ''' Event to call when the connection is destroyed.
        ''' </summary>
        Public Event Disconnected As EventHandler(Of ConnectionEventArgs)

        ''' <summary>
        ''' Event to call when byte data has become available from the server.
        ''' </summary>
        Public Event DataReceived As EventHandler(Of DataReceivedEventArgs)

        ''' <summary>
        ''' Event to call when byte data has been sent to the server.
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

        Friend Sub HandleConnected(sender As Object, args As ConnectionEventArgs)
            RaiseEvent Connected(sender, args)
        End Sub

        Friend Sub HandleClientDisconnected(sender As Object, args As ConnectionEventArgs)
            RaiseEvent Disconnected(sender, args)
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
