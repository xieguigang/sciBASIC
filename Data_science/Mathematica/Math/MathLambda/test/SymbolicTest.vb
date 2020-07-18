#Region "Microsoft.VisualBasic::18abcfc2b2ac49d5d2f1a543d7b8ca20, Data_science\Mathematica\Math\MathLambda\test\SymbolicTest.vb"

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

    ' Module SymbolicTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Lambda
Imports Microsoft.VisualBasic.Math.Scripting

Module SymbolicTest

    Sub Main()
        Dim symbols = ScriptEngine.ParseExpression("(5+5) * (2*x + x / 5 + x ^ 3)")
        Dim result = symbols.DoCall(AddressOf Symbolic.Simplify)

        Console.WriteLine($"{symbols} -> {result}")

        ScriptEngine.SetVariable("x", 33)

        Console.WriteLine($"{symbols} = {symbols.Evaluate(ScriptEngine.Expression)}")
        Console.WriteLine($"{result} = {result.Evaluate(ScriptEngine.Expression)}")

        Pause()
    End Sub
End Module

