#Region "Microsoft.VisualBasic::87dc741afa2b50e2cc0ef72750fda598, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Models\Locks.vb"

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

    '     Class Locks
    ' 
    '         Properties: isEmpty
    ' 
    '         Sub: add, apply, clear
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts.Cola

    ''' <summary>
    ''' Descent respects a collection of locks over nodes that should not move
    ''' </summary>
    Public Class Locks

        Public locks As Dictionary(Of Integer, Double())

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>false if no locks exist</returns>
        Public ReadOnly Property isEmpty() As Boolean
            Get
                Return locks.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' add a lock on the node at index id
        ''' </summary>
        ''' <param name="id">index of node to be locked</param>
        ''' <param name="x">required position for node</param>
        Public Sub add(id%, x As Double())
            Me.locks(id) = x
        End Sub

        ''' <summary>
        ''' clear all locks
        ''' </summary>
        Public Sub clear()
            Me.locks = New Dictionary(Of Integer, Double())()
        End Sub

        ''' <summary>
        ''' perform an operation on each lock
        ''' </summary>
        ''' <param name="f"></param>
        Public Sub apply(f As Action(Of Integer, Double()))
            For Each l In Me.locks.Keys
                f(l, Me.locks(l))
            Next
        End Sub
    End Class
End Namespace
