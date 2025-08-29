#Region "Microsoft.VisualBasic::b40ab42a33cd4ea4760fe03c53836ee4, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\examples\Michalewicz.vb"

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

'   Total Lines: 55
'    Code Lines: 41 (74.55%)
' Comment Lines: 2 (3.64%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 12 (21.82%)
'     File Size: 1.66 KB


' Class Michalewicz
' 
'     Constructor: (+3 Overloads) Sub New
' 
'     Function: evaluate
' 
'     Sub: Main11
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Michalewicz
    Inherits IGradFunction
    Public m2 As Double
    Public d As Integer

    Public Sub New(d As Integer, m As Double)
        m2 = 2.0 * m
        Me.d = d
    End Sub

    Public Sub New(d As Integer)
        Me.New(d, 10.0)
    End Sub

    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim res = 0.0
        For i = 1 To d
            Dim x = [in](i - 1)
            res += Math.Sin(x) * Math.Pow(Math.Sin(i * x * x / Math.PI), m2)
        Next
        Return -res
    End Function


    Public Shared Sub Main11(args As String())

        ' Debugger.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Dim pi = Math.PI

        ' converges to global minimum
        Try
            '	double[] res = lbfgsb.minimize(new Michalewicz(), new double[] { 2,2,2,2,2 }, new double[] { 0,0,0,0,0 }, new double[] { pi,pi,pi,pi,pi });
            Dim res As Double() = lbfgsb.minimize(New Michalewicz(2), New Double() {2, 2}, New Double() {0, 0}, New Double() {pi, pi})

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
