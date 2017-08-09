#Region "Microsoft.VisualBasic::71c82a7d57c2c442314ba2b2dbddfee4, ..\sciBASIC#\Data_science\Mathematica\Math\ODE\ODESolvers\ODE.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Ordinary differential equation(ODE).(常微分方程的模型)
''' </summary>
Public Class ODE
    Implements INamedValue

#Region "Output results"

    Public Property x As Double()
    Public Property y As Double()
#End Region

    ''' <summary>
    ''' Public Delegate Function df(x As Double, y As Double) As Double
    ''' (从这里传递自定义的函数指针)
    ''' </summary>
    ''' <returns></returns>
    Public Property df As ODESolver.df
    ''' <summary>
    ''' ``x=a``的时候的y初始值
    ''' </summary>
    ''' <returns></returns>
    Public Property y0 As Double

    ''' <summary>
    ''' 计算函数值
    ''' </summary>
    ''' <param name="xi"></param>
    ''' <param name="yi"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property GetValue(xi As Double, yi As Double) As Double
        Get
            Return _df(xi, yi)
        End Get
    End Property

    Public ReadOnly Property xrange As DoubleRange
        Get
            Return New DoubleRange(x.First, x.Last)
        End Get
    End Property

    Public Property Id As String Implements INamedValue.Key

    Public Overrides Function ToString() As String
        Return x.GetJson & " --> " & y.GetJson
    End Function
End Class
