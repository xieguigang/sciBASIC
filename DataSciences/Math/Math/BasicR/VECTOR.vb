#Region "Microsoft.VisualBasic::6d39267484e7244a085acd98e22465aa, ..\visualbasic_App\Scripting\Math\Math\BasicR\VECTOR.vb"

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

Namespace BasicR

    Public Class VEC ' : Inherits Double()

        ''' <summary>
        ''' 向量维数
        ''' </summary>
        ''' <remarks></remarks>
        Public [Dim] As Integer
        Public Ele As Double()

        Public Sub New(m As Integer)
            [Dim] = m
            Ele = New Double([Dim] - 1) {}
        End Sub

        ''' <summary>
        ''' 两个向量加法算符重载，分量分别相加
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(v1 As VEC, v2 As VEC) As VEC
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v3 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v3.Ele(j) = v1.Ele(j) + v2.Ele(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量减法算符重载，分量分别想减
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(v1 As VEC, v2 As VEC) As VEC
            Dim N0 As Integer
            '获取变量维数
            N0 = v1.[Dim]

            Dim v3 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v3.Ele(j) = v1.Ele(j) - v2.Ele(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量乘法算符重载，分量分别相乘，相当于MATLAB中的  .*算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(v1 As VEC, v2 As VEC) As VEC

            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v3 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v3.Ele(j) = v1.Ele(j) * v2.Ele(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量除法算符重载，分量分别相除，相当于MATLAB中的   ./算符
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(v1 As VEC, v2 As VEC) As VEC
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v3 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v3.Ele(j) = v1.Ele(j) / v2.Ele(j)
            Next
            Return v3
        End Operator

        ''' <summary>
        ''' 向量减加实数，各分量分别加实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(v1 As VEC, a As Double) As VEC
            '向量数加算符重载
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) + a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量减实数，各分量分别减实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(v1 As VEC, a As Double) As VEC
            '向量数加算符重载
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) - a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数乘，各分量分别乘以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(v1 As VEC, a As Double) As VEC
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) * a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数除，各分量分别除以实数
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="a"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(v1 As VEC, a As Double) As VEC
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) / a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 实数加向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(a As Double, v1 As VEC) As VEC
            '向量数加算符重载
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) + a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 实数减向量
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(a As Double, v1 As VEC) As VEC
            '向量数加算符重载
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) - a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量 数乘
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(a As Double, v1 As VEC) As VEC
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim v2 As New VEC(N0)

            Dim j As Integer
            For j = 0 To N0 - 1
                v2.Ele(j) = v1.Ele(j) * a
            Next
            Return v2
        End Operator

        ''' <summary>
        ''' 向量内积
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Or(v1 As VEC, v2 As VEC) As Double
            Dim N0 As Integer, M0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            M0 = v2.[Dim]

            If N0 <> M0 Then
                System.Console.WriteLine("Inner vector dimensions must agree！")
            End If
            '如果向量维数不匹配，给出告警信息

            Dim sum As Double
            sum = 0.0

            Dim j As Integer
            For j = 0 To N0 - 1
                sum = sum + v1.Ele(j) * v2.Ele(j)
            Next
            Return sum
        End Operator

        ''' <summary>
        ''' 向量外积（相当于列向量，乘以横向量）
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Xor(v1 As VEC, v2 As VEC) As MATRIX
            Dim N0 As Integer, M0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            M0 = v2.[Dim]

            If N0 <> M0 Then
                System.Console.WriteLine("Inner vector dimensions must agree！")
            End If
            '如果向量维数不匹配，给出告警信息

            Dim vvmat As New MATRIX(N0, N0)

            For i As Integer = 0 To N0 - 1
                For j As Integer = 0 To N0 - 1
                    vvmat(i, j) = v1(i) * v2(j)
                Next
            Next

            '返回外积矩阵

            Return vvmat
        End Operator

        ''' <summary>
        ''' 向量模的平方
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Not(v1 As VEC) As Double
            Dim N0 As Integer

            '获取变量维数
            N0 = v1.[Dim]

            Dim sum As Double
            sum = 0.0

            Dim j As Integer
            For j = 0 To N0 - 1
                sum = sum + v1.Ele(j) * v1.Ele(j)
            Next
            Return sum
        End Operator

        ''' <summary>
        ''' 负向量 
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(v1 As VEC) As VEC
            Dim N0 As Integer = v1.[Dim]

            Dim v2 As New VEC(N0)

            For i As Integer = 0 To N0 - 1
                v2.Ele(i) = -v1.Ele(i)
            Next

            Return v2
        End Operator

        Default Public Property Item(index As Integer) As Double
            Get
                Return Ele(index)
            End Get
            Set(value As Double)
                Ele(index) = value
            End Set
        End Property
    End Class
End Namespace
