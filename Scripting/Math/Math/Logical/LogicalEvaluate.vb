Namespace Logical

    Public Module LogicalEvaluate

        Public ReadOnly Property LogicalCompares As IReadOnlyDictionary(Of String, Func(Of Double, Double, Boolean)) =
            New Dictionary(Of String, Func(Of Double, Double, Boolean)) From {
 _
            {"=", Function(a, b) a = b},
            {"<>", Function(a, b) a <> b},
            {"~=", Function(a, b) Math.Abs((a - b) / a) < 0.1},
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

    End Module
End Namespace