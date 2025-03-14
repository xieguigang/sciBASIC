#Region "Microsoft.VisualBasic::2f2b536d7802fc39795b762f588d13c9, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Buckin.vb"

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

    '   Total Lines: 40
    '    Code Lines: 25 (62.50%)
    ' Comment Lines: 4 (10.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (27.50%)
    '     File Size: 1.21 KB


    ' Class Buckin
    ' 
    '     Function: evaluate
    ' 
    '     Sub: Main6
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' BUKIN FUNCTION N. 6
' https://www.sfu.ca/~ssurjano/bukin6.html
' f(-10,1) = 0;
Public Class Buckin
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return 100.0 * Math.Sqrt(Math.Abs(x2 - 0.01 * x1 * x1)) + 0.01 * Math.Abs(x1 + 10.0)
    End Function

    Public Shared Sub Main6(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' FAILS

        Try
            Dim res As Double() = lbfgsb.minimize(New Buckin(), New Double() {-10.1, 1}, New Double() {-15, -3}, New Double() {-5, 3})

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
