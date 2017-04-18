Imports Microsoft.VisualBasic.DataMining.SVM.Model

Namespace Method

    ''' <summary>
    ''' @author Ralf Wondratschek
    ''' </summary>
    Public MustInherit Class Optimizer

        Protected Friend _line As Line
        Protected Friend _points As LabeledPoint()
        Protected Friend _iterations As Integer
        Protected Friend _cancelled As Boolean

        Public Sub New(line As Line, points As IList(Of LabeledPoint), iterations As Integer)
            _line = line.Clone()
            _points = New LabeledPoint(points.Count - 1) {}
            For i As Integer = 0 To _points.Length - 1
                _points(i) = points(i).Clone()
            Next

            _iterations = iterations
            _cancelled = False
        End Sub

        Public Function Optimize() As Line
            Dim result As Line = innerOptimize()
            Return result
        End Function

        Protected Friend MustOverride Function innerOptimize() As Line

        Public Sub Cancel()
            _cancelled = True
        End Sub
    End Class
End Namespace