#Region "Microsoft.VisualBasic::73e8224f8ce57d39cdffd30897e1f11d, Data_science\Mathematica\Math\Math\test\VectorTest.vb"

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

    ' Module VectorTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module VectorTest

    Sub Main()
        Dim x As Double() = {423, 4, 2, 4, 24, 2, 3, 423, 4, 2, 3, 4, 23, 4, 2, 4, 2, 3, 4, 2, 4, 2}
        Dim y As Vector = Vector.Call(Of Double)(New Func(Of Double, Double, Double)(AddressOf Math.Log), x, 2).ToArray
        Dim z As Vector = Vector.Call(Function(a, b) a / b, x, 1000000)

        Pause()
    End Sub
End Module

