Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public MustInherit Class Optimizer

        Protected Friend mLine As Line
        Protected Friend mPoints As LabeledPoint()
        Protected Friend mIterations As Integer

        Protected Friend mCancelled As Boolean

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            mLine = line.clone()
            mPoints = New LabeledPoint(points.Count - 1) {}
            For i As Integer = 0 To mPoints.Length - 1
                mPoints(i) = points(i).clone()
            Next

            mIterations = iterations
            mCancelled = False
        End Sub

        Public Function optimize() As Line

            Dim result As Line = innerOptimize()

            For Each p As LabeledPoint In mPoints
                p.release()
            Next

            Return result
        End Function

        Protected Friend MustOverride Function innerOptimize() As Line

        Public Sub cancel()
            mCancelled = True
        End Sub
    End Class

End Namespace