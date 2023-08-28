Imports System.Text

Namespace ConsoleApp1.data
    ''' <summary>
    ''' Created by danielp on 1/27/17.
    ''' </summary>
    Public Class TrainResult

        Friend fwd_time, bwd_time As Long
        Friend l2_decay_loss, l1_decay_loss, cost_loss, softmax_loss, lossField As Double

        Public Sub New(fwd_time As Long, bwd_time As Long, l1_decay_loss As Double, l2_decay_loss As Double, cost_loss As Double, softmax_loss As Double, loss As Double)
            Me.fwd_time = fwd_time
            Me.bwd_time = bwd_time
            Me.l1_decay_loss = l1_decay_loss
            Me.l2_decay_loss = l2_decay_loss
            Me.cost_loss = cost_loss
            Me.softmax_loss = softmax_loss
            lossField = loss
        End Sub

        Public Overridable ReadOnly Property Loss As Double
            Get
                Return lossField
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("loss: ")
            sb.Append(cost_loss)
            Return sb.ToString()
        End Function
    End Class

End Namespace
