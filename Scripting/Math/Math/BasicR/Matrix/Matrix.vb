Imports System.Text
Imports int = System.Int32

Namespace BasicR

    Public Class MATRIX

        ''' <summary>
        ''' 数组维数
        ''' </summary>
        ''' <remarks></remarks>
        Dim dim1, dim2 As Integer
        ''' <summary>
        ''' 用二维数组构造数学意义下的矩阵
        ''' 矩阵元素保存于对象ele中
        ''' </summary>
        ''' <remarks></remarks>
        Dim Ele As Double(,)

        Public Sub New(m As Integer, n As Integer)
            dim1 = m
            dim2 = n
            Ele = New Double(m, n)
        End Sub

        Public Shared Operator +(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As int = a1.dim1, n As int = a1.dim2

            Dim a3 As MATRIX = New MATRIX(m, n)
            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) + a2(i, j)
                Next
            Next
            Return a3
        End Operator

        Public Shared Operator -(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m As Integer = a1.dim2, n As Integer = a1.dim2

            Dim a3 As MATRIX = New MATRIX(m, n)
            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) - a2(i, j)
                Next
            Next
            Return a3
        End Operator


        ''' <summary>
        '''     //-------两个矩阵乘法算符重载
        '''   //------矩阵元素分别相乘，相当于MATLAB中的   .*
        '''   // 要求两个矩阵维数相同，矩阵类不进行个数判断
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Operator *(a1 As MATRIX, a2 As MATRIX) As MATRIX

            Dim m = a1.dim1, n = a1.dim2

            Dim a3 As MATRIX = New MATRIX(m, n)
            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) * a2(i, j)
                Next
            Next
            Return a3
        End Operator

        ''' <summary>
        ''' 
        '''    //-------两个矩阵除法算符重载
        '''    //------矩阵元素分别相除，相当于MATLAB中的   ./
        '''    // 要求两个矩阵维数相同，矩阵类不进行个数判断
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="a2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(a1 As MATRIX, a2 As MATRIX) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a3 As MATRIX = New MATRIX(m, n)
            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) / a2(i, j)
                Next
            Next
            Return a3
        End Operator

        ''' <summary>
        ''' 
        '''   //矩阵加实数算符重载
        ''' //各分量分别加实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(a1 As MATRIX, x As Double) As MATRIX

            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) + x
                Next
            Next

            Return a2

        End Operator



        ''' <summary>
        '''     //矩阵减实数算符重载
        '''   //各分量分别减实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(a1 As MATRIX, x As Double) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) - x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        ''' 
        ''' //矩阵乘以实数算符重载
        '''  //各分量分别乘以实数
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Operator *(a1 As MATRIX, x As Double) As MATRIX
   Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) * x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        ''' 
        ''' //矩阵除以实数算符重载
        '''  //各分量分别除以实数
        ''' </summary>
        ''' <param name="a1"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator /(a1 As MATRIX, x As Double) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) / x
                Next
            Next

            Return a2
        End Operator



        ''' <summary>
        '''     //实数加矩阵算符重载
        ''' //各分量分别加实数
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator +(x As Double, a1 As MATRIX) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = a1(i, j) + x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        '''     //实数减矩阵算符重载
        '''   //各分量分别减实数 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator -(x As Double, a1 As MATRIX) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = x - a1(i, j)
                    'a3(i, j) = a1(i, j) - x
                Next
            Next

            Return a2
        End Operator

        ''' <summary>
        '''   //实数乘矩阵算符重载
        '''    //各分量分别乘以实数
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="a1"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(x As Double, a1 As MATRIX) As MATRIX
            Dim m = a1.dim1, n = a1.dim2

            Dim a2 As MATRIX = New MATRIX(m, n)

            For i As int = 0 To m
                For j As Integer = 0 To n
                    a3(i, j) = x * a1(i, j)
                Next
            Next

            Return a2
        End Operator



        ''' <summary>
        '''   //数学上的矩阵相乘
        ''' </summary>
        ''' <param name="MAT"></param>
        ''' <param name="MAT"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
    public  Shared  operator |(MAT a1, MAT a2) As MATRIX 
            Dim m, n, p, q As int

            m = a1.dim1
            n = a1.dim2

            p = a2.dim1
            q = a2.dim2

            If n <> p Then System.Console.WriteLine("Inner matrix dimensions must agree！")'      //如果矩阵维数不匹配给出告警信息

            Dim a3 As MATRIX = New MATrix(m, q)

            For i As Integer = 0 To m
                For j As int = 0 To q
                    For k As int = 0 To n
                        a3(i, j) = a3(i, j) + a1(i, k) * a2(k, j)
                    Next
                Next
            Next

            Return a3
        End Operator


        ''' <summary>
        ''' 
        '''  //矩阵乘以向量(线性变换）
        ''' //即 b=Ax
        ''' </summary>
        ''' <param name="A"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator *(A As MATRIX, x As Vector) As Vector

            Dim m, n, p As int
            m = A.dim1
            n = A.dim2

            p = x.dim

            If n <> p Then System.Console.WriteLine("Inner matrix dimensions must agree！") '    //如果矩阵维数不匹配，给出告警信息

            Dim b As Vector = New Vector(m)

            For i As int = 0 To m
                For k As int = 0 To n
                    b(i) = b(i) + A(i, k) * x(k)
                Next

                Return b
        End Operator

        ''' <summary>
        ''' 矩阵转置
        ''' </summary>
        ''' <param name="A"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function T(A As MATRIX) As MATRIX
            Dim m As int = A.dim1, n As int = A.dim2

            Dim TA As MATRIX = New MATRIX(n, m)
            For i As int = 0 To n
                For j As int = 0 To m
                    TA(i, j) = A(j, i)
                Next
            Next
            Return TA
        End Function
    End Class
End Namespace