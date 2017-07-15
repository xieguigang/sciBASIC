Imports System.Linq.Expressions

Namespace Scripting.TokenIcer

    Module OperatorExpression

        Public ReadOnly Property Linq2Name As New Dictionary(Of ExpressionType, String)

        Sub New()
            With Linq2Name
                Call .Add(ExpressionType.Add, "+")
                Call .Add(ExpressionType.AddAssign, "+=")
                Call .Add(ExpressionType.AddAssignChecked, "+=")
                Call .Add(ExpressionType.AddChecked, "+")
                Call .Add(ExpressionType.And, "And")
                Call .Add(ExpressionType.AndAlso, "AndAlso")
                Call .Add(ExpressionType.Assign, "=")
                Call .Add(ExpressionType.Convert, "CType")
                Call .Add(ExpressionType.ConvertChecked, "CType")
                Call .Add(ExpressionType.Decrement, "-")
                Call .Add(ExpressionType.Divide, "/")
                Call .Add(ExpressionType.DivideAssign, "/=")
                Call .Add(ExpressionType.Equal, "=")
                Call .Add(ExpressionType.ExclusiveOr, "Xor")
                Call .Add(ExpressionType.GreaterThan, ">")
                Call .Add(ExpressionType.GreaterThanOrEqual, ">=")
                Call .Add(ExpressionType.Increment, "+")
                Call .Add(ExpressionType.IsFalse, "IsFalse")
                Call .Add(ExpressionType.IsTrue, "IsTrue")
                Call .Add(ExpressionType.LeftShift, "<<")
                Call .Add(ExpressionType.LeftShiftAssign, "<<=")
                Call .Add(ExpressionType.LessThan, "<")
                Call .Add(ExpressionType.LessThanOrEqual, "<=")
                Call .Add(ExpressionType.Modulo, "Mod")
                Call .Add(ExpressionType.Multiply, "*")
                Call .Add(ExpressionType.MultiplyAssign, "*=")
                Call .Add(ExpressionType.MultiplyAssignChecked, "*=")
                Call .Add(ExpressionType.MultiplyChecked, "*")
                Call .Add(ExpressionType.Negate, "-")
                Call .Add(ExpressionType.NegateChecked, "-")
                Call .Add(ExpressionType.Not, "Not")
                Call .Add(ExpressionType.NotEqual, "<>")
                Call .Add(ExpressionType.Or, "Or")
                Call .Add(ExpressionType.OrElse, "OrElse")
                Call .Add(ExpressionType.Power, "^")
                Call .Add(ExpressionType.PowerAssign, "^=")
                Call .Add(ExpressionType.RightShift, ">>")
                Call .Add(ExpressionType.RightShiftAssign, ">>=")
                Call .Add(ExpressionType.Subtract, "-")
                Call .Add(ExpressionType.SubtractAssign, "-=")
                Call .Add(ExpressionType.SubtractAssignChecked, "-=")
                Call .Add(ExpressionType.SubtractChecked, "-")
                Call .Add(ExpressionType.TypeAs, "TryCast")
                Call .Add(ExpressionType.UnaryPlus, "+")
            End With
        End Sub

    End Module
End Namespace