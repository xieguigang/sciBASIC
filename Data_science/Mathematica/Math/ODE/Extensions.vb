#Region "Microsoft.VisualBasic::3b81952b1b7e3fda4ea5126f143203e1, Data_science\Mathematica\Math\ODE\Extensions.vb"

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

    '   Total Lines: 102
    '    Code Lines: 69 (67.65%)
    ' Comment Lines: 18 (17.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.71%)
    '     File Size: 3.94 KB


    ' Module Extensions
    ' 
    '     Function: CDF, ECDF, Let, Solve
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Function ECDF(v As IEnumerable(Of Double), range As Integer(),
                         Optional resolution As Integer = 50000,
                         Optional p0 As Double = 0) As Func(Of Double, Double)

        Dim x As Double() = Nothing
        Dim y As Double() = Nothing

        Call CDF(AddressOf New ECDF(v, range).eval, New DoubleRange(range), resolution, p0, x, y)

        Return AddressOf New ECDF(y, x).eval
    End Function

    ''' <summary>
    ''' Cumulative Distribution Function via solve ODE
    ''' </summary>
    ''' <param name="p"></param>
    ''' <param name="range"></param>
    ''' <param name="resolution"></param>
    ''' <param name="p0"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CDF(p As Func(Of Double, Double),
                        range As DoubleRange,
                        Optional resolution As Integer = 50000,
                        Optional p0 As Double = 0,
                        Optional ByRef x As Double() = Nothing,
                        Optional ByRef y As Double() = Nothing) As Double

        Dim ode As New ODE With {
            .ID = p.ToString,
            .y0 = p0,
            .df = Function(xi, yi) p(xi)
        }
        Dim solve = ode.RK4(resolution, range.Min, range.Max)
        Dim sum As Double = solve.Y.vector.Last

        x = solve.X.ToArray
        y = solve.Y.vector

        Return sum - p0
    End Function

    ''' <summary>
    ''' Solve the target ODEs dynamics system by using the RK4 solver.
    ''' </summary>
    ''' <param name="system"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Solve(system As IEnumerable(Of NonlinearVar), dt As (from#, to#, step#)) As ODEsOut
        Dim vector As NonlinearVar() = system.ToArray
        Dim df = Sub(dx#, ByRef dy As Vector)
                     For Each x As NonlinearVar In vector
                         dy(x) = x.deSolve()
                     Next
                 End Sub
        Dim ODEs As New GenericODEs(vector, df)

        With dt
            Dim result As ODEsOut = ODEs.Solve(CInt((.to - .from) / .step), .from, .to)
            Return result
        End With
    End Function

    ''' <summary>
    ''' Create VisualBasic variables.(使用这个函数来进行初始化是为了在赋值的同时还对新创建的对象赋予名称，方便将结果写入数据集)
    ''' </summary>
    ''' <param name="list"></param>
    ''' <returns></returns>
    <Extension> Public Function Let$(list As Expression(Of Func(Of var())))
        Dim unaryExpression As NewArrayExpression = DirectCast(list.Body, NewArrayExpression)
        Dim arrayData = unaryExpression _
            .Expressions _
            .Select(Function(b) DirectCast(b, BinaryExpression)) _
            .ToArray
        Dim var As New Dictionary(Of String, Double)

        For Each expr As BinaryExpression In arrayData
            Dim member = DirectCast(expr.Left, MemberExpression)
            Dim name As String = member.Member.Name.Replace("$VB$Local_", "")
            Dim field As FieldInfo = DirectCast(member.Member, FieldInfo)
            Dim value As Object = DirectCast(expr.Right, ConstantExpression).Value
            Dim obj = DirectCast(member.Expression, ConstantExpression).Value

            Call field.SetValue(obj, New var(name, CDbl(value)))
        Next

        Return var.GetJson
    End Function
End Module
