
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter

Namespace Layouts.Cola

    ''' <summary>
    ''' The layout process fires three events:
    ''' 
    ''' > - ``start``: layout iterations started
    ''' > - ``tick``: fired once per iteration, listen to this to animate
    ''' > - ``end``: layout converged, you might like to zoom-to-fit or 
    ''' >       something at notification of this event
    ''' </summary>
    Public Enum EventType
        ''' <summary>
        ''' layout iterations started
        ''' </summary>
        start
        ''' <summary>
        ''' fired once per iteration, listen to this to animate
        ''' </summary>
        tick
        ''' <summary>
        ''' layout converged, you might like to zoom-to-fit or something 
        ''' at notification of this event
        ''' </summary>
        [end]
    End Enum

    Public Class [Event]

        Public type As EventType
        Public alpha As Double
        Public stress As Double
        Public listener As Action
        Public s As Segment
        Public pos As Double

        Default Public Property Accessor(name As String) As Object
            Get
                Select Case name
                    Case NameOf(type)
                        Return type
                    Case NameOf(alpha)
                        Return alpha
                    Case NameOf(stress)
                        Return stress
                    Case NameOf(listener)
                        Return listener
                    Case Else
                        Throw New NotImplementedException(name)
                End Select
            End Get
            Set(value As Object)
                Select Case name
                    Case NameOf(type)
                        type = value
                    Case NameOf(alpha)
                        alpha = value
                    Case NameOf(stress)
                        stress = value
                    Case NameOf(listener)
                        listener = value
                    Case Else
                        Throw New NotImplementedException(name)
                End Select
            End Set
        End Property
    End Class
End Namespace