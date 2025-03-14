#Region "Microsoft.VisualBasic::3c34d1c5989013cb66a1f74c4611b661, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\DropWave.vb"

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

    '   Total Lines: 42
    '    Code Lines: 26 (61.90%)
    ' Comment Lines: 3 (7.14%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 13 (30.95%)
    '     File Size: 1.19 KB


    ' Class DropWave
    ' 
    '     Function: evaluate
    ' 
    '     Sub: Main8
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB



' DROP-WAVE
' https://www.sfu.ca/~ssurjano/drop.html
' f(0,0)=-1


Public Class DropWave
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim v = x1 * x1 + x2 * x2
        Return -(1.0 + Math.Cos(12.0 * v)) / (0.5 * v + 2.0)
    End Function

    Public Shared Sub Main8(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New DropWave(), New Double() {-0.1, 0.1}, New Double() {-5, -5}, New Double() {5, 5})

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
