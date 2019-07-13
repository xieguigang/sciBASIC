#Region "Microsoft.VisualBasic::dc1422d810f3c786177f5343c73cad8a, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\FuncCaller.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class FuncCaller
    ' 
    '         Properties: Name, Params
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.Types
Imports Microsoft.VisualBasic.Linq

Namespace Scripting

    ''' <summary>
    ''' Function object model.(调用函数的方法)
    ''' </summary>
    Public Class FuncCaller

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
            Dim args As String() = Params.Select(Function(x) x.ToString).ToArray
            Return $"{Name}({args.JoinBy(", ")})"
        End Function

        Public Function Evaluate() As Double
            Return __calls(Name, Params.Select(Function(x) x.Evaluate).ToArray)
        End Function
    End Class

    Public Delegate Function IFuncEvaluate(name As String, args As Double()) As Double
End Namespace
