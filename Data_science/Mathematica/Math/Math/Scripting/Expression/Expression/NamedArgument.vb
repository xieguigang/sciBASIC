Imports System.Globalization

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' A named argument in a function invoke expression, in the format of ``name:=value``.
    ''' </summary>
    ''' <remarks>
    ''' The node is transparent for evaluation and symbol traversal: both operations are
    ''' delegated to the inner <see cref="value"/> expression.
    ''' </remarks>
    Public Class NamedArgument : Inherits Expression

        ''' <summary>
        ''' The name of this argument.
        ''' </summary>
        Public Property name As String

        ''' <summary>
        ''' The value expression of this argument.
        ''' </summary>
        Public Property value As Expression

        ''' <summary>
        ''' Create a new named argument node.
        ''' </summary>
        ''' <param name="name">The argument name.</param>
        ''' <param name="value">The argument value expression.</param>
        Public Sub New(name As String, value As Expression)
            Me.name = name
            Me.value = value
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Return value.Evaluate(env)
        End Function

        Public Overrides Iterator Function GetVariableSymbols() As IEnumerable(Of String)
            If value IsNot Nothing Then
                For Each symbol As String In value.GetVariableSymbols
                    Yield symbol
                Next
            End If
        End Function

        Public Overrides Function ToString() As String
            Dim text As String

            If TypeOf value Is Literal Then
                text = DirectCast(value, Literal).number.ToString(CultureInfo.InvariantCulture)
            Else
                text = If(value Is Nothing, "", value.ToString)
            End If

            Return $"{name}:={text}"
        End Function
    End Class
End Namespace
