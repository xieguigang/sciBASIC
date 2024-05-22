#Region "Microsoft.VisualBasic::94b26c1192c484a2d9df7ad7f7d9a368, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\MathHelpers.vb"

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

    '   Total Lines: 49
    '    Code Lines: 34 (69.39%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (30.61%)
    '     File Size: 1.45 KB


    '     Class DoubleFunction
    ' 
    ' 
    ' 
    '     Class DoubleDoubleFunction
    ' 
    ' 
    ' 
    '     Module Helpers
    ' 
    '         Function: (+2 Overloads) assign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace math

    Public MustInherit Class DoubleFunction

        Public MustOverride Function apply(x As Double) As Double

    End Class

    Public MustInherit Class DoubleDoubleFunction

        Public MustOverride Function apply(x As Double, y As Double) As Double

    End Class

    Module Helpers

        <Extension>
        Public Function assign(m As GeneralMatrix, f As DoubleFunction) As NumericMatrix
            Dim rows As New List(Of Double())

            For Each row As Vector In m.RowVectors
                rows.Add(row.Select(AddressOf f.apply).ToArray)
            Next

            Return New NumericMatrix(rows)
        End Function

        <Extension>
        Public Function assign(m1 As GeneralMatrix, m2 As GeneralMatrix, f As DoubleDoubleFunction) As NumericMatrix
            Dim rows As New List(Of Double())
            Dim a = m1.RowVectors.ToArray
            Dim b = m2.RowVectors.ToArray
            Dim x, y As Vector

            For i As Integer = 0 To a.Length - 1
                x = a(i)
                y = b(i)
                rows.Add(x.Select(Function(xi, offset) f.apply(xi, y(offset))).ToArray)
            Next

            Return New NumericMatrix(rows)
        End Function

    End Module
End Namespace
