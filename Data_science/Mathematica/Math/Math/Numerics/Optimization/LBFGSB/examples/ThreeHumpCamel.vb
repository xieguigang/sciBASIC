#Region "Microsoft.VisualBasic::59ad921a786655fb5c1f7ea8a2430a8f, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\ThreeHumpCamel.vb"

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

    '   Total Lines: 54
    '    Code Lines: 32 (59.26%)
    ' Comment Lines: 7 (12.96%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (27.78%)
    '     File Size: 1.72 KB


    ' Class ThreeHumpCamel
    ' 
    '     Function: evaluate, in_place_gradient
    ' 
    '     Sub: Main16
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON


' THREE-HUMP CAMEL FUNCTION
' https://www.sfu.ca/~ssurjano/camel3.html
' Global minimum: f(0,0)=0 


Public NotInheritable Class ThreeHumpCamel
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double(), grad As Double()) As Double
        Dim x = [in](0)
        Dim x2 = x * x
        Dim x4 = x2 * x2
        Dim y = [in](1)

        grad(0) = 4 * x - 4.2 * x2 * x + x4 * x + y
        grad(1) = x + 2 * y

        Return 2 * x2 - 1.05 * x4 + x4 * x2 / 6.0 + x * y + y * y
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main16(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            '			double[] res = lbfgsb.minimize(new ThreeHumpCamel(), new double[] { -5, 5 }, new double[] { -5, -5 },
            '					new double[] { 5, 5 });
            Dim res As Double() = lbfgsb.minimize(New ThreeHumpCamel(), New Double() {2, 2}, New Double() {0, -5}, New Double() {1, 1})

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
