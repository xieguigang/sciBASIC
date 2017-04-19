Imports Microsoft.VisualBasic.DataMining.SVM.Model

Public Class State

    Friend ReadOnly outerInstance As CartesianCoordinateSystem

    Public Property Line As Line
    Public ReadOnly Property Points As List(Of LabeledPoint)

    Sub New(outerInstance As CartesianCoordinateSystem)
        Me.New(outerInstance, New List(Of LabeledPoint)(), Nothing)
    End Sub

    Sub New(outerInstance As CartesianCoordinateSystem, points As IList(Of LabeledPoint), line As Line)
        Me.outerInstance = outerInstance
        Me.Points = New List(Of LabeledPoint)(points.Count)

        For Each p As LabeledPoint In points
            Me.Points.Add(p.Clone())
        Next

        If line IsNot Nothing Then
            Me.Line = line.Clone()
        End If
    End Sub

    Sub AddPoint(point As LabeledPoint)
        Points.Add(point)
    End Sub

    Sub RemovePoint(point As LabeledPoint)
        Points.Remove(point)
    End Sub

    Sub ClearPoints()
        Points.Clear()
    End Sub

    Public Overrides Function Equals(o As Object) As Boolean
        If TypeOf o Is State Then

            Dim state As State = CType(o, State)

            If Not LabeledPoint.ListEqual(state.Points, Points) Then Return False
            If Me.Line Is Nothing AndAlso state.Line Is Nothing Then Return True
            If Me.Line IsNot Nothing AndAlso state.Line IsNot Nothing Then
                Return state.Line.Equals(Me.Line)
            End If

            Return False
        End If

        Return MyBase.Equals(o)
    End Function
End Class
