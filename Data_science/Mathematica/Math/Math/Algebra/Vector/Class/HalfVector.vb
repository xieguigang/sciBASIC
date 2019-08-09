Imports Microsoft.VisualBasic.Math.Numerics
Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors

Namespace LinearAlgebra

    ''' <summary>
    ''' 精度比较低的半精度向量
    ''' </summary>
    Public Class HalfVector : Inherits GenericVector(Of Half)

        Public ReadOnly Property Sum As Double
            Get
                Dim result As Double

                For Each value As Half In buffer
                    result += CSng(value)
                Next

                Return result
            End Get
        End Property

        Sub New(data As IEnumerable(Of Half))
            Call MyBase.New(data)
        End Sub

        Sub New(values As IEnumerable(Of Single))
            Call MyBase.New(From x As Single In values Select CType(x, Half))
        End Sub

        Sub New(value As Single, n As Integer)
            Call MyBase.New(CType(value, Half).Replicate(n))
        End Sub

        Public Shared Operator +(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x + CSng(add))
        End Operator

        Public Shared Operator +(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x + add)
        End Operator

        Public Shared Operator -(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x - CSng(add))
        End Operator

        Public Shared Operator -(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x - add)
        End Operator

        Public Shared Operator *(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x * CSng(add))
        End Operator

        Public Shared Operator *(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x * add)
        End Operator

        Public Shared Operator /(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x / CSng(add))
        End Operator

        Public Shared Operator /(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x / add)
        End Operator

        Public Shared Operator ^(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(From x As Half In v Select x ^ CSng(add))
        End Operator

        Public Shared Operator ^(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(From x As Half In v Select x ^ add)
        End Operator
    End Class
End Namespace