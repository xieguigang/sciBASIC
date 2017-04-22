#Region "Microsoft.VisualBasic::b62f295c5bf766da4bb320963b7b3224, ..\sciBASIC#\Data_science\Mathematical\Math\Test\Program.vb"

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

Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Quantile
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Module Program

    Sub Main()

        Call (0#, 100000.0#).DoubleRange.rand(2000).Summary.EchoLine
        Call {0#, 569.0#, 63.0#, 59, 345.0#, 456, 423}.Summary.EchoLine

        Pause()



        Call Test(1, 2, 3, z:="a+b")

        Pause()

        Call "x <- 123+3^3!".Evaluate
        ' Call "log((x+699)*9/3!)".Evaluate
        Call "x <- log((x+699)*9/3!)+sin(99)".Evaluate
        Call "x!".Evaluate
        Call "(1+2)!/5".Evaluate

        Pause()
    End Sub

    Sub Test(a!, b&, c#, Optional x$ = "(A + b^2)! * 100", Optional y$ = "(cos(x/33)+1)^2 -3", Optional z$ = "log(-Y) + 9")
        'Dim params As Dictionary(Of String, Double) = ParameterExpression.Evaluate(Function() {a, b, x, y, z})
        'Dim json$ = params.GetJson(True)

        Dim before = {a, b, c, x, y, z}

        Call ParameterExpression.Apply(Function() {a, b, x, y, z})

        Dim after = {a, b, c, x, y, z}

        'Call json.__DEBUG_ECHO
    End Sub
End Module
