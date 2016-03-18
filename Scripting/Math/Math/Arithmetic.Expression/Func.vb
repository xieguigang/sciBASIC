Imports Microsoft.VisualBasic.Mathematical.Types
Imports Microsoft.VisualBasic.Linq

Public Delegate Function IFuncEvaluate(name As String, args As Double()) As Double

''' <summary>
''' Function object model.
''' </summary>
Public Class Func

    Public ReadOnly Property Name As String
    Public ReadOnly Property Params As New List(Of SimpleExpression)

    ReadOnly __calls As IFuncEvaluate

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Name">The function name</param>
    ''' <param name="evaluate">Engine handle</param>
    Sub New(Name As String, evaluate As IFuncEvaluate)
        Me.Name = Name
        Me.__calls = evaluate
    End Sub

    Public Overrides Function ToString() As String
        Dim args As String() = Params.ToArray(Function(x) x.ToString)
        Return $"{Name}({args.JoinBy(", ")})"
    End Function

    Public Function Evaluate() As Double
        Return __calls(Name, Params.ToArray(Function(x) x.Evaluate))
    End Function
End Class