﻿#Region "Microsoft.VisualBasic::03ad8ecb598d6d321ebf20fafef6b20a, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/Math//Scripting/Expression/Expression/Expression.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 25
    '    Code Lines: 14
    ' Comment Lines: 5
    '   Blank Lines: 6
    '     File Size: 812 B


    '     Class Expression
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' the abstract math expression
    ''' </summary>
    Public MustInherit Class Expression

        Public MustOverride Function Evaluate(env As ExpressionEngine) As Double

        Public Shared Operator =(expr As Expression, literal As Literal) As Boolean
            ' test expression type is literal?
            If expr Is Nothing OrElse Not TypeOf expr Is Literal Then
                Return False
            End If

            ' test the literal value
            Return DirectCast(expr, Literal).number = literal.number
        End Operator

        Public Shared Operator <>(expr As Expression, literal As Literal) As Boolean
            Return Not expr = literal
        End Operator

    End Class
End Namespace
