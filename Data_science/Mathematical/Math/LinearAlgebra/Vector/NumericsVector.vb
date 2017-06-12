Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' <see cref="Integer"/>, <see cref="Long"/>, <see cref="ULong"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class NumericsVector(Of T As {IComparable, Structure}) : Inherits GenericVector(Of Double)

       

        Sub New()

        End Sub
    End Class
End Namespace