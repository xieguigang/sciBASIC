#Region "Microsoft.VisualBasic::fceb752b00cc54e5bffa70b32fee407b, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Booth.vb"

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

    '   Total Lines: 50
    '    Code Lines: 32 (64.00%)
    ' Comment Lines: 4 (8.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (28.00%)
    '     File Size: 1.53 KB


    ' Class Booth
    ' 
    '     Function: evaluate, in_place_gradient
    ' 
    '     Sub: Main5
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON


' https://www.sfu.ca/~ssurjano/booth.html
' f(1,3) = 0


Public Class Booth
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double(), grad As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim a = x1 + 2.0 * x2 - 7.0
        Dim b = 2.0 * x1 + x2 - 5.0

        grad(0) = 2.0 * a + 4.0 * b
        grad(1) = 4.0 * a + 2.0 * b

        Return a * a + b * b
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main5(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            '		double[] res = lbfgsb.minimize(new Booth(), new double[] { -10,0.1 }, new double[] { -10, -10 },
            '				new double[] { 10, 10 });
            Dim res As Double() = lbfgsb.minimize(New Booth(), New Double() {-1, -10}, New Double() {-10, 3}, New Double() {10, 3.1})
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
