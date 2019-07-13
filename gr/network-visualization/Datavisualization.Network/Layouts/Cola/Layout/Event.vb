#Region "Microsoft.VisualBasic::0cb6025690528504570ed4f1051f7198, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout\Event.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum EventType
    ' 
    '         [end], start, tick
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class [Event]
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Structure Comparer
    ' 
    '             Function: Compare
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.My.JavaScript

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

    Public Class [Event] : Inherits JavaScriptObject

        Public type As EventType
        Public alpha As Double
        Public stress As Double
        Public listener As Action
        Public s As Segment
        Public pos As Double
        Public isOpen As Boolean
        Public v As Node

        Sub New(isOpen As Boolean, v As Node, pos As Double)
            Me.isOpen = isOpen
            Me.v = v
            Me.pos = pos
        End Sub

        Sub New()
        End Sub

        Public Structure Comparer : Implements IComparer(Of [Event])

            Public Function Compare(a As [Event], b As [Event]) As Integer Implements IComparer(Of [Event]).Compare
                Return a.pos - b.pos + a.type - b.type
            End Function
        End Structure
    End Class
End Namespace
