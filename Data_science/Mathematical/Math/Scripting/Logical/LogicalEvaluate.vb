#Region "Microsoft.VisualBasic::280266813cb99ef71505fbc524dba56c, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Logical\LogicalEvaluate.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Namespace Scripting.Logical

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

        Public Function ExpressionParser(s As String)
            Throw New NotSupportedException
        End Function

    End Module
End Namespace
