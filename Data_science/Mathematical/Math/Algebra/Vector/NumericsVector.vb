Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' <see cref="Integer"/>, <see cref="Long"/>, <see cref="ULong"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class NumericsVector(Of T As {IComparable, Structure}) : Inherits GenericVector(Of Double)

        ReadOnly operators As NumericsOperator = NumericsOperator.GetOperators(GetType(T))

        Sub New()
            Dim i As Integer

            Dim o = Me / 3

        End Sub

        Public Overloads Shared Operator +(v As NumericsVector(Of T), o As Object) As Object

        End Operator


        Public Overloads Shared Operator /(v As NumericsVector(Of T), o As IComparable) As Object

        End Operator
    End Class
End Namespace