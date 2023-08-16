Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Math

    Public Function reduce_sum(v As Vector) As Double
        Return v.Sum
    End Function

    Public Function reduce_mean(v As Vector) As Double
        Return v.Average
    End Function

    Public Function clip_by_value(v As Vector, min As Double, max As Double) As Vector
        v(v < min) = Vector.Scalar(min)
        v(v > max) = Vector.Scalar(max)
        Return v
    End Function

    Public Function square(v As Vector) As Vector
        Return v ^ 2
    End Function

    Public Function exp(v As Vector) As Vector
        Return v.Exp
    End Function

    Public Function log(v As Vector) As Vector
        Return v.Log
    End Function
End Module
