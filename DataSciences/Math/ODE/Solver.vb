Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Solving the Ordinary differential equation(ODE) by using trapezoidal method.(使用梯形法求解常微分方程)
''' </summary>
''' <remarks>http://www.oschina.net/code/snippet_76_4433</remarks>
Public Module ODESolver

    ''' <summary>
    ''' df函数指针，微分方程 ``df = f(x,y)``
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Delegate Function df(x As Double, y As Double) As Double

    ''' <summary>
    ''' 欧拉法解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0,y0)，ODE的结果会从x和y这两个数组指针返回
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Sub Eluer(ByRef df As ODE, n As Integer, a As Double, b As Double)
        Dim h As Double = (b - a) / n

        df.x = New Double(n - 1) {}
        df.y = New Double(n - 1) {}
        df.x(Scan0) = a
        df.y(Scan0) = df.y0

        Dim x = df.x, y = df.y

        For i As Integer = 1 To n - 1
            x(i) = a + h * i
            y(i) = x(i - 1) + h * df(x(i - 1), y(i - 1))
        Next
    End Sub

    ''' <summary>
    ''' 二阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    <Extension>
    Public Sub RK2(ByRef df As ODE, n As Integer, a As Double, b As Double)
        Dim h As Double = (b - a) / n

        df.x = New Double(n - 1) {}
        df.y = New Double(n - 1) {}
        df.x(Scan0) = a
        df.y(Scan0) = df.y0

        Dim x = df.x, y = df.y
        Dim k1 As Double, k2 As Double

        For i As Integer = 1 To n - 1
            x(i) = a + h * i
            k1 = df(x(i - 1), y(i - 1))
            k2 = df(x(i - 1) + h, y(i - 1) + h * k1)
            y(i) = y(i - 1) + h / 2 * (k1 + k2)
        Next
    End Sub

    ''' <summary>
    ''' 四阶龙格库塔法解解微分方程，分块数量为n, 解的区间为[a,b], 解向量为(x,y),方程初值为(x0, y0)
    ''' 参考http://blog.sina.com.cn/s/blog_698c6a6f0100lp4x.html 和维基百科
    ''' </summary>
    ''' <param name="df"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' 
    <Extension>
    Public Sub RK4(ByRef df As ODE, n As Integer, a As Double, b As Double)
        Dim h As Double = (b - a) / n

        df.x = New Double(n - 1) {}
        df.y = New Double(n - 1) {}
        df.x(Scan0) = a
        df.y(Scan0) = df.y0

        Dim x = df.x, y = df.y
        Dim k1, k2, k3, k4 As Double

        For i As Integer = 1 To n - 1
            x(i) = a + h * i
            k1 = df(x(i - 1), y(i - 1))
            k2 = df(x(i - 1) + 0.5 * h, y(i - 1) + 0.5 * h * k1)
            k3 = df(x(i - 1) + 0.5 * h, y(i - 1) + 0.5 * h * k2)
            k4 = df(x(i - 1) + h, y(i - 1) + h * k3)
            y(i) = y(i - 1) + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4)
        Next
    End Sub
End Module

