#Region "Microsoft.VisualBasic::e837fb36b272bd1c0861e4389cec353b, sciBASIC#\Data_science\Mathematica\Math\test\scriptTest.vb"

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

    '   Total Lines: 16
    '    Code Lines: 8
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 596.00 B


    ' Module scriptTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression

Module scriptTest
    Sub Main()
        Dim tokens2 = New ExpressionTokenIcer("((5) -X33)").GetTokens.ToArray


        'Dim tokens = New ExpressionTokenIcer("(((1+X33 + 9!) ^ 9) * (5+8! %33))").GetTokens.ToArray
        'Dim expression = ExpressionBuilder.BuildExpression(tokens)

        'Console.WriteLine(New ExpressionEngine().SetSymbol("X33", -100).Evaluate("(((1+X33 + 9!) ^ 9) * (5+8! %33))"))
        Console.WriteLine(New ExpressionEngine().Evaluate("1+ abs(-100-99)"))

        Pause()
    End Sub
End Module
