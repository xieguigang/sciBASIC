#Region "Microsoft.VisualBasic::6ea108cf947d574f856a1bdda1177ead, Data_science\Mathematica\Math\Math\Spline\CubicSpline\CubicEval.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20 (74.07%)
    ' Comment Lines: 1 (3.70%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 749 B


    '     Structure Cubic
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Eval, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Interpolation

    Public Structure Cubic

        Public ReadOnly a, b, c, d As Double

        Public Sub New(p0 As Double, d2 As Double, e As Double, f As Double)
            d = p0
            c = d2
            b = e
            a = f
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Eval(u As Double) As Double
            ' equals a*x^3 + b*x^2 + c*x + d
            Return (((a * u) + b) * u + c) * u + d
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
