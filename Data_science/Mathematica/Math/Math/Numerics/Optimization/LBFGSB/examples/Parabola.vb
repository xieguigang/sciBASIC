#Region "Microsoft.VisualBasic::e99516a3274d3fdc75ebf210b7a3b5e9, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Parabola.vb"

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

    '   Total Lines: 41
    '    Code Lines: 28 (68.29%)
    ' Comment Lines: 3 (7.32%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (24.39%)
    '     File Size: 1.26 KB


    ' Class Parabola
    ' 
    '     Function: evaluate, in_place_gradient
    ' 
    '     Sub: Main12
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON



' Parabola, f(x)=2x^2-x+3
' Global minimum: f(0.25) = 2.875
Public Class Parabola
    Inherits IGradFunction

    Public Overrides Function evaluate(x As Double(), grad As Double()) As Double
        Dim xx = x(0)
        grad(0) = 4 * xx - 1
        Return 2 * xx * xx - xx + 3
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main12(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Parabola(), New Double() {-2}, New Double() {-5}, New Double() {5})
            Call ("!RESULT").debug
            Call ("k = " & lbfgsb.k.ToString()).debug
            Call ("x = " & res.GetJson).debug
            Call ("fx = " & lbfgsb.fx.ToString()).debug
            Call ("grad = " & lbfgsb.m_grad.GetJson).debug
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try
    End Sub

End Class
