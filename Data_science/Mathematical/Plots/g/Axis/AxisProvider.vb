Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic.Axis

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' ```vbnet
    ''' Dim axis$ = "(min,max),tick=steps,n=parts"
    ''' 
    ''' ' example as
    ''' Dim axis$ = "(0,100),tick=10"
    ''' Dim axis$ = "(100,1000),n=10"
    ''' ```
    ''' </remarks>
    Public Class AxisProvider

        Public Property Range As DoubleRange
        Public Property n As Integer
        Public Property Tick As Double

        Public Function AxisTicks() As Double()
            If n = 0 Then
                Return Range.GetAxisByTick(Tick)
            Else
                Return Range.GetAxisByTick(tick:=Range.Length / n)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return AxisTicks().GetJson
        End Function

        Public Shared Function TryParse(s$) As AxisProvider
            Dim range As String = Regex.Match(s, "\(.+?\)").Value
            Dim tick$ = Regex.Match(s, "tick=[^,]+", RegexICSng).Value
            Dim n$ = Regex.Match(s, "n=[^,]+", RegexICSng).Value

            Return New AxisProvider With {
                .n = n.GetTagValue("=", trim:=True).Value.ParseInteger,
                .Range = DoubleRange.op_Implicit(range),
                .Tick = tick.GetTagValue("=", trim:=True).Value.ParseNumeric
            }
        End Function

        Public Shared Widening Operator CType(expression$) As AxisProvider
            Return TryParse(expression)
        End Operator

        Public Shared Narrowing Operator CType(axis As AxisProvider) As Double()
            Return axis.AxisTicks
        End Operator
    End Class
End Namespace