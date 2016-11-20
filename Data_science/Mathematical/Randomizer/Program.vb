Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports MAT = Microsoft.VisualBasic.Mathematical.LinearAlgebra.Matrix

''' <summary>
''' 正态分布随机数
''' </summary>
Public Module crandn

    ''' <summary>
    ''' 生成标准正态分布随机向量
    ''' </summary>
    ''' <param name="m">向量维数</param>
    ''' <param name="seed">混合同余种子</param>
    ''' <returns></returns>
    Public Function randn(m As Integer, seed As Integer) As Vector
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 21:55:12         
        '    ---------------------------------------------------
        '    Desciption  : 生成标准正态分布随机数向量
        '     *
        '    Post Script :
        '     *
        '    paramtegers :
        '     *   m--------向量维数
        '     *   seed-----混合同余种子
        '    -------------------------------------------------

        Dim gauss As New Vector(m)
        Dim tmp1 As Double

        If m Mod 2 = 0 Then
            '如果是偶数
            '同时用两个均匀分布随机数
            '产生两个高斯分布随机数

            '生成均匀分布向量
            Dim u As Vector = rand(m, seed)
            Dim k As Integer = 0

            While k < m
                tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(k)))
                gauss(k) = tmp1 * Math.Cos(2 * Math.PI * u(k + 1))
                gauss(k + 1) = tmp1 * Math.Sin(2 * Math.PI * u(k + 1))

                k = k + 2
            End While
        Else
            '生成均匀分布向量
            '如果是奇数，则多生成一个均匀数，

            Dim u As Vector = rand(m + 1, seed)
            Dim k As Integer = 0

            While k < m - 1
                tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(k)))
                gauss(k) = tmp1 * Math.Cos(2 * Math.PI * u(k + 1))
                gauss(k + 1) = tmp1 * Math.Sin(2 * Math.PI * u(k + 1))

                k = k + 2
            End While

            tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(m - 1)))
            gauss(m - 1) = tmp1 * Math.Cos(2 * Math.PI * u(m))
        End If

        Return gauss
    End Function

    ''' <summary>
    ''' 生成标准正态分布随机矩阵
    ''' </summary>
    ''' <param name="m">矩阵维数</param>
    ''' <param name="n">矩阵维数</param>
    ''' <param name="seed">混合同余种子</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 由于矩阵的行和列都可能为奇数，所以先判断总元素
    ''' 的奇偶性，如果是偶数，先生成偶数随机向量，后把
    ''' 随机向量赋值给随机矩阵。
    ''' 如果总元素为奇数，按照VEC rand中类似的方法先
    ''' 生成随机向量，后把向量复制给随机矩阵。
    ''' </remarks>
    Public Function randn(m As Integer, n As Integer, seed As Integer) As MAT
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 21:57:36         
        '    ---------------------------------------------------
        '    Desciption  :  标准正态分布随机矩阵发生器
        '     *
        '    Post Script :
        '     *    由于矩阵的行和列都可能为奇数，所以先判断总元素
        '     *    的奇偶性，如果是偶数，先生成偶数随机向量，后把
        '     *    随机向量赋值给随机矩阵。
        '     *    如果总元素为奇数，按照VEC rand中类似的方法先
        '     *    生成随机向量，后把向量复制给随机矩阵。
        '     *
        '    paramtegers :
        '     *  [m,n]----矩阵维数
        '     *  seed-----混合同余种子
        '     *
        '    -------------------------------------------------

        Dim p As Integer = m * n
        Dim gauss As New Vector(p)
        Dim tmp1 As Double

        If p Mod 2 = 0 Then
            '生成均匀分布向量
            Dim u As Vector = rand(p, seed)
            Dim k As Integer = 0

            While k < p
                tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(k)))
                gauss(k) = tmp1 * Math.Cos(2 * Math.PI * u(k + 1))
                gauss(k + 1) = tmp1 * Math.Sin(2 * Math.PI * u(k + 1))

                k = k + 2
            End While
        Else
            '生成均匀分布向量
            '如果是奇数，则多生成一个均匀数，
            Dim u As Vector = rand(p + 1, seed)
            Dim k As Integer = 0

            While k < p - 1
                tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(k)))
                gauss(k) = tmp1 * Math.Cos(2 * Math.PI * u(k + 1))
                gauss(k + 1) = tmp1 * Math.Sin(2 * Math.PI * u(k + 1))

                k = k + 2
            End While

            tmp1 = Math.Sqrt(-2 * Math.Log(1 - u(m)))
            gauss(m) = tmp1 * Math.Cos(2 * Math.PI * u(m + 1))
        End If

        Dim goal As New MAT(m, n)

        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                goal(i, j) = gauss(i * n + j)
            Next
        Next

        Return goal
    End Function

    ''' <summary>
    ''' 混合同余法产生[0，1]均匀分布随机数向量
    ''' </summary>
    ''' <param name="m">向量维数</param>
    ''' <param name="seed">种子</param>
    ''' <returns></returns>
    Public Function rand(m As Integer, seed As Integer) As Vector
        '------------------------------------------ comment
        '    Author      : syz 
        '    Date        : 2011-07-12 19:39:36         
        '    ---------------------------------------------------
        '    Desciption  : 混合同余法产生[0，1]均匀分布随机数向量
        '     *
        '    Post Script :
        '     *       
        '    paramtegers :
        '     * m--------向量维数
        '     * seed-----种子
        '    -------------------------------------------------

        Const M__2 As Long = 2147483648UI
        Const namda As Long = 314159269
        Const C As Long = 453806245

        '结果
        Dim goal As New Vector(m)
        Dim x0 As Long = seed
        Dim x1 As Long

        For k As Integer = 0 To m - 1
            x1 = (x0 * namda + C) Mod M__2
            '得到余数

            '通过乘以1.0把整数运算变为浮点数运算，否则结果都为0
            goal(k) = x1 * 1.0 / M__2

            '下一个起点以之前的余数为种子
            x0 = x1
        Next

        Return goal
    End Function
End Module