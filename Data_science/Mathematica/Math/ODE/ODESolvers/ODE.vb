#Region "Microsoft.VisualBasic::48989c74a7761dd42d8a802636df5b28, Data_science\Mathematica\Math\ODE\ODESolvers\ODE.vb"

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

    '   Total Lines: 43
    '    Code Lines: 16 (37.21%)
    ' Comment Lines: 22 (51.16%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.63%)
    '     File Size: 1.34 KB


    ' Class ODE
    ' 
    '     Properties: df, ID, y0
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

''' <summary>
''' Ordinary differential equation(ODE).(常微分方程的模型)
''' </summary>
Public Class ODE : Implements INamedValue

    ''' <summary>
    ''' The tag value
    ''' </summary>
    ''' <returns></returns>
    Public Property ID As String Implements INamedValue.Key

    ''' <summary>
    ''' Public <see cref="System.Delegate"/> Function df(x As <see cref="Double"/>, y As <see cref="Double"/>) As <see cref="Double"/>
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
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return _df(xi, yi)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return df.ToString
    End Function
End Class
