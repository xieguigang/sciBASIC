#Region "Microsoft.VisualBasic::1eadcdd71d7a85a3b0a6688567f34e2a, sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\FunctionInvoke.vb"

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
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 881 B


    '     Class FunctionInvoke
    ' 
    '         Properties: funcName, parameters
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Scripting.MathExpression.Impl

    Public Class FunctionInvoke : Inherits Expression

        Public Property funcName As String
        Public Property parameters As Expression()

        Sub New(name As String, parameters As Expression())
            Me.funcName = name
            Me.parameters = parameters
        End Sub

        Public Overrides Function Evaluate(env As ExpressionEngine) As Double
            Dim func As Func(Of Double(), Double) = env.GetFunction(funcName)
            Dim parameters As Double() = Me.parameters.Select(Function(x) x.Evaluate(env)).ToArray
            Dim result As Double = func(parameters)

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"{funcName}({parameters.JoinBy(", ")})"
        End Function
    End Class
End Namespace
