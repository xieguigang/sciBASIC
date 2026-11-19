' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning
'  Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\Elu.vb
' /********************************************************************************/

Imports System
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports std = System.Math

Namespace ComponentModel.Activations

    ''' <summary>
    ''' Exponential linear unit (ELU): <c>x</c> for <c>x &gt;= 0</c>,
    ''' otherwise <c>alpha * (exp(x) - 1)</c>.
    ''' </summary>
    <Serializable>
    Public Class Elu : Inherits IActivationFunction

        ''' <summary>Negative half axis scale; must be positive.</summary>
        Public Property Alpha As Double = 1.0

        ''' <summary>XML serializable data model of this activation function.</summary>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .name = NameOf(Elu),
                    .Arguments = {
                        New NamedValue With {.name = "alpha", .text = Alpha}
                    }
                }
            End Get
        End Property

        ''' <summary>Create a standard ELU function with <c>alpha = 1</c>.</summary>
        Public Sub New()
        End Sub

        ''' <summary>Create an ELU function with a custom alpha.</summary>
        Public Sub New(alpha As Double)
            If alpha <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(alpha), "ELU 的 alpha 必须为正数")
            End If
            Me.Alpha = alpha
        End Sub

        Public Overrides Function [Function](x As Double) As Double
            If x >= 0 Then
                Return x
            End If
            Return Alpha * (std.Exp(x) - 1.0)
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            If x >= 0 Then
                Return 1.0
            End If
            Return Alpha * std.Exp(x)
        End Function

        Public Overrides Function ToString() As String
            Return $"Elu(alpha:={Alpha})"
        End Function

    End Class

End Namespace
