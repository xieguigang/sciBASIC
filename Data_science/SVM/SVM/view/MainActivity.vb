Imports System.Linq
Imports Microsoft.VisualBasic.DataMining.SVM.Method
Imports Microsoft.VisualBasic.DataMining.SVM.Model

Public Class MainActivity

    Public Function calculate(CartesianCoordinateSystem As CartesianCoordinateSystem, mNavigationPosition%) As (points As Model.LabeledPoint(), result As Model.Line)
        Dim line As Line = CartesianCoordinateSystem.Line
        If line Is Nothing Then
            line = New Line(New NormalVector(-1, 1), 0)
            CartesianCoordinateSystem.setLine(line)
        End If

        line.NormalVector.W1 = line.NormalVector.W1 / line.NormalVector.W2
        line.NormalVector.W2 = 1

        Dim optimizer As Optimizer
        Select Case mNavigationPosition
            Case 0
                optimizer = New SubGradientDescent(CartesianCoordinateSystem.Line, CartesianCoordinateSystem.Points)
            Case 1
                optimizer = New GradientDescent(CartesianCoordinateSystem.Line, CartesianCoordinateSystem.Points)
            Case 2
                optimizer = New NewtonMethod(CartesianCoordinateSystem.Line, CartesianCoordinateSystem.Points)
            Case Else
                optimizer = Nothing
        End Select

        Dim result = optimizer.Calculate
        CartesianCoordinateSystem.setLine(result)

        Return (CartesianCoordinateSystem.Points.ToArray, result)
    End Function

    Function insertDefault() As CartesianCoordinateSystem
        Dim mCartesianCoordinateSystem As New CartesianCoordinateSystem
        Dim points As IList(Of LabeledPoint) = mCartesianCoordinateSystem.Points
        Dim toAdd As IList(Of LabeledPoint) = New List(Of LabeledPoint)
        toAdd.Add(LabeledPoint.GetInstance(0.4, 0.4, ColorClass.RED))
        toAdd.Add(LabeledPoint.GetInstance(0.7, 0.6, ColorClass.RED))
        toAdd.Add(LabeledPoint.GetInstance(0.2, 0.6, ColorClass.BLUE))
        toAdd.Add(LabeledPoint.GetInstance(0.4, 0.9, ColorClass.BLUE))
        toAdd.Add(Model.LabeledPoint.GetInstance(1, 1, ColorClass.BLUE))
        toAdd.Add(Model.LabeledPoint.GetInstance(1, 0.75, ColorClass.BLUE))
        toAdd.Add(Model.LabeledPoint.GetInstance(0.6, 0.59, ColorClass.BLUE))
        toAdd.Add(Model.LabeledPoint.GetInstance(0.14, 0.5, ColorClass.RED))

        If Not LabeledPoint.ListEqual(points, toAdd) Then
            mCartesianCoordinateSystem.clearPoints()
            For Each p As LabeledPoint In toAdd
                mCartesianCoordinateSystem.addPoint(p)
            Next
        Else
            For Each p As LabeledPoint In toAdd
                p.release()
            Next
        End If

        Dim line As New Line(0, 0.4, 1, 0.9)
        If Not line.Equals(mCartesianCoordinateSystem.Line) Then
            mCartesianCoordinateSystem.setLine(line)
        End If

        Console.WriteLine(mCartesianCoordinateSystem.Line.ToString)

        Return mCartesianCoordinateSystem
    End Function
End Class