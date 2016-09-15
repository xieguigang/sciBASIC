#Region "Microsoft.VisualBasic::c6e4399318607803ed06605774e24bb6, ..\visualbasic_App\Scripting\Math\Math\BasicR\Matrix\Matrix.vb"

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

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Matlab里常用的矩阵运算函数  
    ''' %假设矩阵为A
    ''' det(A)求矩阵行列式
    ''' eig(A)求矩阵特征值或特征向量
    ''' inv(A)矩阵A求逆
    ''' pinv(A)矩阵A求伪逆
    ''' rank(A)求矩阵A的秩
    ''' svd(A)求矩阵A的奇异值或进行奇异值分解
    ''' gsvd(A)求矩阵A的广义奇异值
    ''' trace(A)求矩阵A的迹
    ''' schur(A)对矩阵A进行Schur分解
    ''' hess(A)求矩阵A的Hessenburg标准型
    ''' cond(A)求矩阵A的范数
    ''' chol(A)对矩阵A进行Cholesky分解
    ''' lu(A)对矩阵A进行lu分解
    ''' qr(A)对矩阵A进行QR分解
    ''' poly(A)求矩阵A的特征多项式
    ''' </remarks>
    Public Class MATRIX

        Public Dim1 As Integer, Dim2 As Integer
        Public Ele As Double(,)

        ''' <summary>
        ''' 获取矩阵行数
        ''' </summary>
        ''' <returns>函数将返回矩阵的行数</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property GetSize() As Integer
            Get
                Return Ele.Length \ Ele.GetLength(0)
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                Return Ele.Length
            End Get
        End Property

        Public Sub New(m As Integer, n As Integer)
            Dim1 = m : Dim2 = n

            '用二维数组构造数学意义下的矩阵
            '矩阵元素保存于对象ele中
            Ele = New Double(m, n) {}
        End Sub

        Protected Friend Sub New()
        End Sub

        Public Shared Widening Operator CType(Array As Double(,)) As MATRIX
            Return New MATRIX With {.Ele = Array, .Dim1 = Array.Length, .Dim2 = Array.GetLength(0)}
        End Operator

        Public Shared Widening Operator CType(n As Double) As MATRIX
            Dim MAT As MATRIX = New MATRIX(0, 0)
            MAT(0, 0) = n
            Return MAT
        End Operator

        ''' <summary>
        ''' 调整矩阵的大小，并保留原有的数据
        ''' </summary>
        ''' <param name="m"></param>
        ''' <param name="n"></param>
        ''' <remarks></remarks>
        Public Sub Resize(m As Integer, n As Integer)
            Me.Dim1 = m
            Me.Dim2 = n
            ReDim Preserve Ele(m - 1, n - 1)
        End Sub

        ''' <summary>
        ''' 获取仅包含有一个元素的矩阵对象
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Number() As MATRIX
            Return New MATRIX(0, 0)
        End Function

        ''' <summary>
        ''' 两个矩阵加法算符重载，矩阵元素分别相加
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a3 As New MATRIX(m, n)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a3.Ele(i, j) = a1.Ele(i, j) + a2.Ele(i, j)
                Next
            Next

            Return a3
        End Operator

        ''' <summary>
        ''' 两个矩阵减法算符重载，矩阵元素分别相减
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a3 As New MATRIX(m, n)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a3.Ele(i, j) = a1.Ele(i, j) - a2.Ele(i, j)
                Next
            Next

            Return a3
        End Operator

        ''' <summary>
        ''' 两个矩阵乘法算符重载，矩阵元素分别相乘，相当于MATLAB中的   .*，要求两个矩阵维数相同，矩阵类不进行个数判断
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a3 As New MATRIX(m, n)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a3.Ele(i, j) = a1.Ele(i, j) * a2.Ele(i, j)
                Next
            Next

            Return a3
        End Operator

        ''' <summary>
        ''' 两个矩阵除法算符重载，矩阵元素分别相除，相当于MATLAB中的   ./，要求两个矩阵维数相同，矩阵类不进行个数判断
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a3 As New MATRIX(m, n)
            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a3.Ele(i, j) = a1.Ele(i, j) / a2.Ele(i, j)
                Next
            Next

            Return a3
        End Operator

        ''' <summary>
        ''' 矩阵加实数算符重载，各分量分别加实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(a1 As MATRIX, x As Double) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) + x
                Next
            Next


            Return a2
        End Operator


        ''' <summary>
        ''' 矩阵减实数算符重载，各分量分别减实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(a1 As MATRIX, x As Double) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) - x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        ''' 矩阵乘以实数算符重载，各分量分别乘以实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(a1 As MATRIX, x As Double) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) * x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        ''' 矩阵除以实数算符重载，各分量分别除以实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(a1 As MATRIX, x As Double) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) / x
                Next
            Next

            Return a2
        End Operator



        ''' <summary>
        ''' 实数加矩阵算符重载，各分量分别加实数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(x As Double, a1 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) + x
                Next
            Next


            Return a2
        End Operator



        ''' <summary>
        ''' 实数减矩阵算符重载，各分量分别减实数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(x As Double, a1 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) - x
                Next
            Next

            Return a2
        End Operator


        ''' <summary>
        ''' 实数乘矩阵算符重载，各分量分别乘以实数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(x As Double, a1 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = a1.Dim1
            n = a1.Dim2

            Dim a2 As New MATRIX(m, n)

            For i As Integer = 0 To m - 1
                For j As Integer = 0 To n - 1
                    a2.Ele(i, j) = a1.Ele(i, j) * x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        ''' 数学上的矩阵相乘
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Or(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer, n As Integer, p As Integer, q As Integer

            m = a1.Dim1
            n = a1.Dim2

            p = a2.Dim1
            q = a2.Dim2

            If n <> p Then
                System.Console.WriteLine("Inner matrix dimensions must agree！")
            End If
            '如果矩阵维数不匹配给出告警信息

            '新矩阵，用于存放结果
            Dim a3 As New MATRIX(m, q)

            Dim i As Integer, j As Integer


            For i = 0 To m - 1
                For j = 0 To q - 1
                    a3.Ele(i, j) = 0.0
                    For k As Integer = 0 To n - 1
                        a3.Ele(i, j) = a3.Ele(i, j) + a1.Ele(i, k) * a2.Ele(k, j)
                    Next
                Next
            Next

            Return a3
        End Operator


        ''' <summary>
        ''' 矩阵乘以向量(线性变换），即 b=Ax
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator Or(A As MATRIX, x As Vector) As Vector
            Dim m As Integer, n As Integer, p As Integer
            m = A.Dim1
            n = A.Dim2

            p = x.[Dim]

            If n <> p Then
                System.Console.WriteLine("Inner matrix dimensions must agree！")
            End If
            '如果矩阵维数不匹配，给出告警信息

            Dim b As New Vector(m)

            For i As Integer = 0 To m - 1
                b(i) = 0.0

                For k As Integer = 0 To n - 1
                    b(i) = b(i) + A.Ele(i, k) * x(k)
                Next
            Next

            Return b
        End Operator

        ''' <summary>
        ''' 矩阵转置操作
        ''' </summary>
        ''' <param name="A"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Transpose(A As MATRIX) As MATRIX
            Dim m As Integer, n As Integer
            m = A.Dim1
            n = A.Dim2

            Dim TA As New MATRIX(n, m)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    TA.Ele(i, j) = A.Ele(j, i)
                Next
            Next

            Return TA
        End Function

        ''' <summary>
        ''' 获取当前的矩阵对象的转置矩阵
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Transpose() As MATRIX
            Dim m As Integer, n As Integer
            m = Dim1
            n = Dim2

            Dim TA As New MATRIX(n, m)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    TA.Ele(i, j) = Ele(j, i)
                Next
            Next

            Return TA
        End Function

        Public Shared Narrowing Operator CType(MAT As MATRIX) As Double(,)
            Return MAT.Ele
        End Operator

        Default Public Property Item(index1 As Integer, index2 As Integer) As Double
            Get
                Return Ele(index1, index2)
            End Get
            Set(value As Double)
                Ele(index1, index2) = value
            End Set
        End Property
    End Class
End Namespace
