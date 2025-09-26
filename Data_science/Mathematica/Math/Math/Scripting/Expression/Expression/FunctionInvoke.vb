#Region "Microsoft.VisualBasic::e274d51f60449a8c9465aa46e27f65cb, Data_science\Mathematica\Math\Math\Scripting\Expression\Expression\FunctionInvoke.vb"

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

    '   Total Lines: 38
    '    Code Lines: 27 (71.05%)
    ' Comment Lines: 3 (7.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.29 KB


    '     Class FunctionInvoke
    ' 
    '         Properties: funcName, parameters
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetVariableSymbols, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Scripting.MathExpression.Impl

    ''' <summary>
    ''' f(x)
    ''' </summary>
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

        Public Overrides Iterator Function GetVariableSymbols() As IEnumerable(Of String)
            For Each arg As Expression In parameters.SafeQuery
                For Each name As String In arg.GetVariableSymbols
                    Yield name
                Next
            Next
        End Function
    End Class
End Namespace
