#Region "Microsoft.VisualBasic::ff3f09d91be1f071d718a962fc2e9314, Data_science\Mathematica\Math\Math\Scripting\Logical\LogicalEvaluate.vb"

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

    '     Module LogicalEvaluate
    ' 
    '         Properties: LogicalCompares, LogicalOperators
    ' 
    '         Function: ExpressionParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports sys = System.Math

Namespace Scripting.Logical

    Public Module LogicalEvaluate

        Public ReadOnly Property LogicalCompares As IReadOnlyDictionary(Of String, Func(Of Double, Double, Boolean)) =
            New Dictionary(Of String, Func(Of Double, Double, Boolean)) From {
 _
            {"=", Function(a, b) a = b},
            {"<>", Function(a, b) a <> b},
            {"~=", Function(a, b) sys.Abs((a - b) / a) < 0.1},
            {"<<", Function(a, b) b / a > 100},
            {"<", Function(a, b) a < b},
            {"<=", Function(a, b) a <= b},
            {">", Function(a, b) a > b},
            {"=>", Function(a, b) a >= b},
            {">>", Function(a, b) a / b > 100}
        }

        Public ReadOnly Property LogicalOperators As IReadOnlyDictionary(Of String, Func(Of Boolean, Boolean, Boolean)) =
            New Dictionary(Of String, Func(Of Boolean, Boolean, Boolean)) From {
 _
            {"and", Function(a, b) a AndAlso b},
            {"or", Function(a, b) a OrElse b},
            {"not", Function(a, b) Not a},
            {"xor", Function(a, b) a Xor b},
            {"nor", Function(a, b) Not (a OrElse b)},
            {"nand", Function(a, b) Not (a AndAlso b)},
            {"is", Function(a, b) CInt(a) = CInt(b)} ' true = true, false = false
        }

        Public Function ExpressionParser(s As String)
            Throw New NotSupportedException
        End Function

    End Module
End Namespace
