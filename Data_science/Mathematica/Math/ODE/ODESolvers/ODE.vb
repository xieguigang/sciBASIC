#Region "Microsoft.VisualBasic::9160e040ebbed1a91b64f569685cfeac, Data_science\Mathematica\Math\ODE\ODESolvers\ODE.vb"

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

' Class ODE
' 
'     Properties: df, ID, y0
' 
'     Function: ToString
' 
' Class ODEOutput
' 
'     Properties: Description, ID, X, xrange, Y
'                 y0
' 
'     Function: ToString
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

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

Public Class ODEOutput : Implements INamedValue

    <XmlAttribute>
    Public Property ID As String Implements INamedValue.Key
    Public Property X As Sequence
    Public Property Y As NumericVector

    <XmlText>
    Public Property Description As String

    Public ReadOnly Property y0 As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return Y.vector.First
        End Get
    End Property

    Public ReadOnly Property xrange As DoubleRange
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return New DoubleRange(X.range)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Y.vector.GetJson
    End Function

    Public Iterator Function GetPointsData() As IEnumerable(Of PointF)
        Yield New PointF(X.range.Min, y0)

        For Each xi In X.ToArray.Skip(1).SeqIterator
            Yield New PointF(xi.value, Y(xi))
        Next
    End Function
End Class
