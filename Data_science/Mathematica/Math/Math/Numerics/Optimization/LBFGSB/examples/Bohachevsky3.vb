#Region "Microsoft.VisualBasic::8e0e3da6f9fd5bc1bbdfad19965ed040, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Bohachevsky3.vb"

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

    '   Total Lines: 44
    '    Code Lines: 30 (68.18%)
    ' Comment Lines: 2 (4.55%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (27.27%)
    '     File Size: 1.54 KB


    ' Class Bohachevsky3
    ' 
    '     Function: evaluate, in_place_gradient
    ' 
    '     Sub: Main4
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

' https://www.sfu.ca/~ssurjano/boha.html

Public Class Bohachevsky3
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double(), g As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        g(0) = 2 * x1 + 0.3 * Math.Sin(3.0 * Math.PI * x1 + 4.0 * Math.PI * x2) * 3.0 * Math.PI
        g(1) = 4 * x2 + 0.4 * Math.Sin(3.0 * Math.PI * x1 + 4.0 * Math.PI * x2) * 4.0 * Math.PI
        Return x1 * x1 + 2.0 * x2 * x2 - 0.3 * Math.Cos(3.0 * Math.PI * x1 + 4.0 * Math.PI * x2) + 0.3
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main4(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Bohachevsky3(), New Double() {0, 0.1}, New Double() {-100, -100}, New Double() {100, 100})

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
