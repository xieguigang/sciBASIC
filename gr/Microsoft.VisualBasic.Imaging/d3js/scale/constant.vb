Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace d3js.scale

    Public Class ConstantScale : Inherits LinearScale

        ReadOnly [const] As Double

        Default Public Overrides ReadOnly Property Value(term As String) As Double
            Get
                Return [const]
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(vector As Array) As Double()
            Get
                Return (From null In vector.AsQueryable Select [const]).ToArray
            End Get
        End Property

        Default Public Overrides ReadOnly Property Value(x As Double) As Double
            Get
                Return [const]
            End Get
        End Property

        Sub New(const_val As Double)
            [const] = const_val
        End Sub

        Public Overrides Function ToString() As String
            Return [const].ToString
        End Function

    End Class
End Namespace