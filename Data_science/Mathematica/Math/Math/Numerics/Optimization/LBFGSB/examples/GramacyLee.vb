#Region "Microsoft.VisualBasic::1bab71198546e120a409c09a2a25f02f, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\GramacyLee.vb"

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
    '    Code Lines: 27 (65.85%)
    ' Comment Lines: 4 (9.76%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (24.39%)
    '     File Size: 1.32 KB


    ' Class GramacyLee
    ' 
    '     Function: evaluate
    ' 
    '     Sub: Main10
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON
' Gramacy & Lee (2012)
' https://www.sfu.ca/~ssurjano/grlee12.html


Public Class GramacyLee
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x = [in](0)

        Dim v = (x - 1) * (x - 1)
        Return Math.Sin(10.0 * Math.PI * x) / (2.0 * x) + v * v
    End Function

    Public Shared Sub Main10(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        param.weak_wolfe = True
        param.max_linesearch = 1000
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' a lot of fails in line search

        Try
            Dim res As Double() = lbfgsb.minimize(New GramacyLee(), New Double() {0.55}, New Double() {0.5}, New Double() {2.5})

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
