Namespace ComponentModel.Evaluation

    Friend Class ChangePoint

        Public Sub New(tp As Integer, fp As Integer, tn As Integer, fn As Integer)
            Me.TP = tp
            Me.FP = fp
            Me.TN = tn
            Me.FN = fn
        End Sub

        Public TP, FP, TN, FN As Integer

        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}:{2}:{3}", TP, FP, TN, FN)
        End Function
    End Class
End Namespace