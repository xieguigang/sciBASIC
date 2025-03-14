#Region "Microsoft.VisualBasic::79ff25b185528f7b21ffe34836b73d95, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Bohachevsky1.vb"

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

    '   Total Lines: 45
    '    Code Lines: 30 (66.67%)
    ' Comment Lines: 1 (2.22%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (31.11%)
    '     File Size: 1.47 KB


    ' Class Bohachevsky1
    ' 
    '     Function: evaluate, in_place_gradient
    ' 
    '     Sub: Main2
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' https://www.sfu.ca/~ssurjano/boha.html


Public Class Bohachevsky1
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double(), g As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        g(0) = 2 * x1 + 0.3 * Math.Sin(3.0 * Math.PI * x1) * 3.0 * Math.PI
        g(1) = 4 * x2 + 0.4 * Math.Sin(4.0 * Math.PI * x2) * 4.0 * Math.PI
        Return x1 * x1 + 2.0 * x2 * x2 - 0.3 * Math.Cos(3.0 * Math.PI * x1) - 0.4 * Math.Cos(4.0 * Math.PI * x2) + 0.7
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main2(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Bohachevsky1(), New Double() {-100, -50}, New Double() {-100, -100}, New Double() {100, 100})

            Debug.debug("!"c, "RESULT")
            Call Debug.debug("k = " & lbfgsb.k.ToString())
            Debug.debug("x = ", res)
            Call Debug.debug("fx = " & lbfgsb.fx.ToString())
            Debug.debug("grad = ", lbfgsb.m_grad)
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try

    End Sub

End Class
