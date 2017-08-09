#Region "Microsoft.VisualBasic::f2cf4b423f069796a0df3d21ee8e8beb, ..\sciBASIC#\Data_science\Mathematica\Math\DataFittings\FitResult.vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class FittedResult

    ''' <summary>
    ''' 拟合后的方程系数，根据阶次获取拟合方程的系数，如getFactor(2),就是获取y=a0+a1*x+a2*x^2+……+apoly_n*x^poly_n中a2的值
    ''' </summary>
    Public Property Factor As New List(Of Double)()
    ''' <summary>
    ''' 回归平方和
    ''' </summary>
    Public Property SSR As Double
    ''' <summary>
    ''' (剩余平方和)
    ''' </summary>
    Public Property SSE As Double
    ''' <summary>
    ''' RMSE均方根误差
    ''' </summary>
    Public Property RMSE As Double
    ''' <summary>
    ''' 保存拟合后的y值，在拟合时可设置为不保存节省内存
    ''' </summary>
    Public Property FitedYlist As New List(Of Double)()

    Sub New()
        Call Factor.Resize(2, 0)
    End Sub

    ''' <summary>
    ''' 根据x获取拟合方程的y值
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function GetY(x As Double) As Double
        Dim ans As Double = 0
        For i As Integer = 0 To Factor.Count - 1
            ans += Factor(i) * Math.Pow(CDbl(x), CInt(i))
        Next
        Return ans
    End Function

    ''' <summary>
    ''' 获取斜率
    ''' </summary>
    ''' <returns>斜率值</returns>
    Public ReadOnly Property Slope() As Double
        Get
            Return Factor(1)
        End Get
    End Property

    ''' <summary>
    ''' 获取截距
    ''' </summary>
    ''' <returns>截距值</returns>
    Public ReadOnly Property Intercept() As Double
        Get
            Return Factor(0)
        End Get
    End Property

    ''' <summary>
    ''' 确定系数，系数是0~1之间的数，是数理上判定拟合优度的一个量
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property R_square() As Double
        Get
            Return 1 - (SSE / (SSR + SSE))
        End Get
    End Property

    ''' <summary>
    ''' 获取拟合方程系数的个数
    ''' </summary>
    ''' <returns>拟合方程系数的个数</returns>
    Public ReadOnly Property FactorSize As Integer
        Get
            Return Factor.Count
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
