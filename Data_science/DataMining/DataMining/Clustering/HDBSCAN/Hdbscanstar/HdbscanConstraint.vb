Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' A clustering constraint (either a must-link or cannot-link constraint between two points).
    ''' </summary>
    Public Class HdbscanConstraint
        Private ReadOnly _constraintType As HdbscanConstraintType
        Private ReadOnly _pointA As Integer
        Private ReadOnly _pointB As Integer

        ''' <summary>
        ''' Creates a new constraint.
        ''' </summary>
        ''' <param name="pointA">The first point involved in the constraint</param>
        ''' <param name="pointB">The second point involved in the constraint</param>
        ''' <param name="type">The constraint type</param>
        Public Sub New(pointA As Integer, pointB As Integer, type As HdbscanConstraintType)
            _pointA = pointA
            _pointB = pointB
            _constraintType = type
        End Sub

        Public Function GetPointA() As Integer
            Return _pointA
        End Function

        Public Function GetPointB() As Integer
            Return _pointB
        End Function

        Public Function GetConstraintType() As HdbscanConstraintType
            Return _constraintType
        End Function
    End Class
End Namespace
