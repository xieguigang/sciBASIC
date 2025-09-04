#Region "Microsoft.VisualBasic::30d9ca27b9d2c939e3e28ca796ef3be5, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Eggholder.vb"

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
    '    Code Lines: 26 (63.41%)
    ' Comment Lines: 4 (9.76%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (26.83%)
    '     File Size: 1.32 KB


    ' Class Eggholder
    ' 
    '     Function: evaluate
    ' 
    '     Sub: Main9
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

' EGGHOLDER
' https://www.sfu.ca/~ssurjano/egg.html
' f(512,404.2319) = -959.6407


Public Class Eggholder
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim x2_47 = x2 + 47.0
        Return -x2_47 * Math.Sin(Math.Sqrt(Math.Abs(x2_47 + x1 / 2.0))) - x1 * Math.Sin(Math.Sqrt(Math.Abs(x1 - x2_47)))
    End Function

    Public Shared Sub Main9(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Eggholder(), New Double() {500, 350}, New Double() {-512, -512}, New Double() {512, 512})

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
