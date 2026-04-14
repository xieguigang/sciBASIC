' ============================================================================
' CVODEExample.vb - CVODE求解器使用示例
' 包含多个常微分方程组的求解示例
' ============================================================================

Imports Microsoft.VisualBasic.Math.Sundials.CVODE

Namespace CVODEExamples

    ''' <summary>
    ''' CVODE求解器示例程序
    ''' </summary>
    Public Class CVODEExample

        ''' <summary>
        ''' 主入口
        ''' </summary>
        Public Shared Sub Main()
            Console.WriteLine("="c, 60)
            Console.WriteLine("Sundials CVODE 求解器示例程序")
            Console.WriteLine("基于VB.NET实现，不依赖第三方库")
            Console.WriteLine("="c, 60)
            Console.WriteLine()

            ' 示例1：简谐振子
            Example1_HarmonicOscillator()

            ' 示例2：Lorenz系统
            Example2_LorenzSystem()

            ' 示例3：刚性ODE（Robertson问题）
            Example3_RobertsonProblem()

            ' 示例4：Van der Pol振子
            Example4_VanDerPolOscillator()

            Console.WriteLine()
            Console.WriteLine("所有示例运行完成！")
            Console.ReadKey()
        End Sub

#Region "示例1：简谐振子"

        ''' <summary>
        ''' 示例1：简谐振子
        ''' 方程：y'' + y = 0
        ''' 转化为一阶方程组：
        '''   y1' = y2
        '''   y2' = -y1
        ''' 解析解：y1 = cos(t), y2 = -sin(t)
        ''' </summary>
        Public Shared Sub Example1_HarmonicOscillator()
            Console.WriteLine("-"c, 60)
            Console.WriteLine("示例1：简谐振子 (Adams方法)")
            Console.WriteLine("方程：y'' + y = 0")
            Console.WriteLine("初始条件：y(0) = 1, y'(0) = 0")
            Console.WriteLine("解析解：y = cos(t)")
            Console.WriteLine("-"c, 60)

            ' 定义右端函数
            Dim rhsFunc As RHSFunction = Sub(t As Double, y As NVector, ydot As NVector)
                                             ydot(0) = y(1)          ' y1' = y2
                                             ydot(1) = -y(0)         ' y2' = -y1
                                         End Sub

            ' 创建求解器（使用Adams方法）
            Dim options As New CVODEOptions() With {
                .RelativeTolerance = 0.000001,
                .AbsoluteTolerance = 0.00000001,
                .MaxOrder = 5,
                .MaxNewtonIterations = 100
            }

            Using solver As New CVODESolver(CVODEMethod.Adams, rhsFunc, 2, options)
                ' 初始化
                Dim y0 As New NVector(New Double() {1.0, 0.0})
                Dim status As CVODEStatus = solver.Initialize(0.0, y0)

                If status <> CVODEStatus.Success Then
                    Console.WriteLine($"初始化失败: {status}")
                    Return
                End If

                ' 输出表头
                Console.WriteLine()
                Console.WriteLine("{0,8} {1,15} {2,15} {3,15} {4,12}",
                    "时间", "数值解y1", "解析解", "误差", "步数")
                Console.WriteLine("{0,8} {1,15:E6} {2,15:E6} {3,15:E6} {4,12}",
                    0.0, y0(0), Math.Cos(0.0), 0.0, 0)

                ' 积分
                Dim tOut As Double = 0.0
                Dim y As New NVector(2)
                For i As Integer = 1 To 10
                    tOut = i * 0.5
                    status = solver.Integrate(tOut, y)

                    If status <> CVODEStatus.Success Then
                        Console.WriteLine($"积分失败: {status}")
                        Exit For
                    End If

                    Dim exact As Double = Math.Cos(tOut)
                    Dim err As Double = Math.Abs(y(0) - exact)

                    Console.WriteLine("{0,8:F2} {1,15:E6} {2,15:E6} {3,15:E6} {4,12}",
                        tOut, y(0), exact, err, solver.TotalSteps)
                Next

                ' 输出统计信息
                Console.WriteLine()
                Console.WriteLine("统计信息:")
                Console.WriteLine($"  总步数: {solver.TotalSteps}")
                Console.WriteLine($"  右端函数调用次数: {solver.RHSFunctionEvaluations}")
                Console.WriteLine($"  Newton迭代次数: {solver.NewtonIterations}")
            End Using

            Console.WriteLine()
        End Sub

#End Region

#Region "示例2：Lorenz系统"

        ''' <summary>
        ''' 示例2：Lorenz系统（混沌系统）
        ''' 方程：
        '''   dx/dt = sigma * (y - x)
        '''   dy/dt = x * (rho - z) - y
        '''   dz/dt = x * y - beta * z
        ''' 经典参数：sigma=10, rho=28, beta=8/3
        ''' </summary>
        Public Shared Sub Example2_LorenzSystem()
            Console.WriteLine("-"c, 60)
            Console.WriteLine("示例2：Lorenz系统 (BDF方法)")
            Console.WriteLine("混沌系统的数值求解")
            Console.WriteLine("-"c, 60)

            ' Lorenz参数
            Const sigma As Double = 10.0
            Const rho As Double = 28.0
            Const beta As Double = 8.0 / 3.0

            ' 定义右端函数
            Dim rhsFunc As RHSFunction = Sub(t As Double, y As NVector, ydot As NVector)
                                             ydot(0) = sigma * (y(1) - y(0))
                                             ydot(1) = y(0) * (rho - y(2)) - y(1)
                                             ydot(2) = y(0) * y(1) - beta * y(2)
                                         End Sub

            ' 创建求解器
            Dim options As New CVODEOptions() With {
                .RelativeTolerance = 0.000001,
                .AbsoluteTolerance = 0.00000001,
                .MaxOrder = 5
            }

            Using solver As New CVODESolver(CVODEMethod.BDF, rhsFunc, 3, options)
                ' 初始条件
                Dim y0 As New NVector(New Double() {1.0, 1.0, 1.0})
                Dim status As CVODEStatus = solver.Initialize(0.0, y0)

                If status <> CVODEStatus.Success Then
                    Console.WriteLine($"初始化失败: {status}")
                    Return
                End If

                ' 输出表头
                Console.WriteLine()
                Console.WriteLine("{0,8} {1,15} {2,15} {3,15} {4,10}",
                    "时间", "x", "y", "z", "步数")
                Console.WriteLine("{0,8:F2} {1,15:E6} {2,15:E6} {3,15:E6} {4,10}",
                    0.0, y0(0), y0(1), y0(2), 0)

                ' 积分
                Dim tOut As Double = 0.0
                Dim y As New NVector(3)
                For i As Integer = 1 To 20
                    tOut = i * 0.5
                    status = solver.Integrate(tOut, y)

                    If status <> CVODEStatus.Success Then
                        Console.WriteLine($"积分失败: {status}")
                        Exit For
                    End If

                    Console.WriteLine("{0,8:F2} {1,15:E6} {2,15:E6} {3,15:E6} {4,10}",
                        tOut, y(0), y(1), y(2), solver.TotalSteps)
                Next

                ' 输出统计信息
                Console.WriteLine()
                Console.WriteLine("统计信息:")
                Console.WriteLine($"  总步数: {solver.TotalSteps}")
                Console.WriteLine($"  右端函数调用次数: {solver.RHSFunctionEvaluations}")
            End Using

            Console.WriteLine()
        End Sub

#End Region

#Region "示例3：Robertson问题（刚性ODE）"

        ''' <summary>
        ''' 示例3：Robertson问题（经典刚性ODE测试问题）
        ''' 方程：
        '''   y1' = -0.04*y1 + 1.0E4*y2*y3
        '''   y2' = 0.04*y1 - 1.0E4*y2*y3 - 3.0E7*y2^2
        '''   y3' = 3.0E7*y2^2
        ''' 初始条件：y1(0)=1, y2(0)=0, y3(0)=0
        ''' </summary>
        Public Shared Sub Example3_RobertsonProblem()
            Console.WriteLine("-"c, 60)
            Console.WriteLine("示例3：Robertson问题 (刚性ODE，BDF方法)")
            Console.WriteLine("经典刚性ODE测试问题")
            Console.WriteLine("-"c, 60)

            ' 定义右端函数
            Dim rhsFunc As RHSFunction = Sub(t As Double, y As NVector, ydot As NVector)
                                             Dim y1 As Double = y(0)
                                             Dim y2 As Double = y(1)
                                             Dim y3 As Double = y(2)

                                             ydot(0) = -0.04 * y1 + 10000.0 * y2 * y3
                                             ydot(1) = 0.04 * y1 - 10000.0 * y2 * y3 - 30000000.0 * y2 * y2
                                             ydot(2) = 30000000.0 * y2 * y2
                                         End Sub

            ' 创建求解器（使用BDF方法处理刚性）
            Dim options As New CVODEOptions() With {
                .RelativeTolerance = 0.0001,
                .AbsoluteTolerance = 0.00000001,
                .MaxOrder = 5,
                .MaxSteps = 100000
            }

            Using solver As New CVODESolver(CVODEMethod.BDF, rhsFunc, 3, options)
                ' 设置分量绝对误差（y2需要更小的容差）
                solver.SetAbsoluteTolerance(New NVector(New Double() {0.000001, 0.0000000001, 0.000001}))

                ' 初始条件
                Dim y0 As New NVector(New Double() {1.0, 0.0, 0.0})
                Dim status As CVODEStatus = solver.Initialize(0.0, y0)

                If status <> CVODEStatus.Success Then
                    Console.WriteLine($"初始化失败: {status}")
                    Return
                End If

                ' 输出表头
                Console.WriteLine()
                Console.WriteLine("{0,12} {1,15} {2,15} {3,15} {4,10}",
                    "时间", "y1", "y2", "y3", "步数")
                Console.WriteLine("{0,12:E2} {1,15:E6} {2,15:E6} {3,15:E6} {4,10}",
                    0.0, y0(0), y0(1), y0(2), 0)

                ' 积分（使用对数时间步）
                Dim y As New NVector(3)
                Dim tValues As Double() = {0.0, 0.4, 4.0, 40.0, 400.0, 4000.0, 40000.0, 400000.0, 4000000.0, 40000000.0, 400000000.0, 4000000000.0, 40000000000.0}

                For i As Integer = 1 To tValues.Length - 1
                    Dim tOut As Double = tValues(i)
                    status = solver.Integrate(tOut, y)

                    If status <> CVODEStatus.Success Then
                        Console.WriteLine($"积分失败: {status}")
                        Exit For
                    End If

                    Console.WriteLine("{0,12:E2} {1,15:E6} {2,15:E6} {3,15:E6} {4,10}",
                        tOut, y(0), y(1), y(2), solver.TotalSteps)
                Next

                ' 输出统计信息
                Console.WriteLine()
                Console.WriteLine("统计信息:")
                Console.WriteLine($"  总步数: {solver.TotalSteps}")
                Console.WriteLine($"  右端函数调用次数: {solver.RHSFunctionEvaluations}")
                Console.WriteLine($"  Newton迭代次数: {solver.NewtonIterations}")
            End Using

            Console.WriteLine()
        End Sub

#End Region

#Region "示例4：Van der Pol振子"

        ''' <summary>
        ''' 示例4：Van der Pol振子（中等刚性）
        ''' 方程：y'' - mu*(1-y^2)*y' + y = 0
        ''' 转化为一阶方程组：
        '''   y1' = y2
        '''   y2' = mu*(1-y1^2)*y2 - y1
        ''' </summary>
        Public Shared Sub Example4_VanDerPolOscillator()
            Console.WriteLine("-"c, 60)
            Console.WriteLine("示例4：Van der Pol振子 (BDF方法)")
            Console.WriteLine("方程：y'' - mu*(1-y^2)*y' + y = 0")
            Console.WriteLine("参数：mu = 1000 (刚性)")
            Console.WriteLine("-"c, 60)

            ' Van der Pol参数
            Const mu As Double = 1000.0

            ' 定义右端函数
            Dim rhsFunc As RHSFunction = Sub(t As Double, y As NVector, ydot As NVector)
                                             ydot(0) = y(1)
                                             ydot(1) = mu * (1.0 - y(0) * y(0)) * y(1) - y(0)
                                         End Sub

            ' 定义Jacobian函数（可选，提高效率）
            Dim jacFunc As JacobianFunction = Sub(t As Double, y As NVector, fy As NVector, J As DenseMatrix)
                                                  J(0, 0) = 0.0
                                                  J(0, 1) = 1.0
                                                  J(1, 0) = -2.0 * mu * y(0) * y(1) - 1.0
                                                  J(1, 1) = mu * (1.0 - y(0) * y(0))
                                              End Sub

            ' 创建求解器
            Dim options As New CVODEOptions() With {
                .RelativeTolerance = 0.000001,
                .AbsoluteTolerance = 0.000001,
                .MaxOrder = 5,
                .MaxSteps = 50000
            }

            Using solver As New CVODESolver(CVODEMethod.BDF, rhsFunc, 2, options)
                ' 设置Jacobian函数
                solver.SetJacobianFunction(jacFunc)

                ' 初始条件
                Dim y0 As New NVector(New Double() {2.0, 0.0})
                Dim status As CVODEStatus = solver.Initialize(0.0, y0)

                If status <> CVODEStatus.Success Then
                    Console.WriteLine($"初始化失败: {status}")
                    Return
                End If

                ' 输出表头
                Console.WriteLine()
                Console.WriteLine("{0,12} {1,15} {2,15} {3,12}",
                    "时间", "y1", "y2", "步数")
                Console.WriteLine("{0,12:E2} {1,15:E6} {2,15:E6} {3,12}",
                    0.0, y0(0), y0(1), 0)

                ' 积分
                Dim y As New NVector(2)
                Dim tOut As Double = 0.0
                For i As Integer = 1 To 10
                    tOut = i * 300.0
                    status = solver.Integrate(tOut, y)

                    If status <> CVODEStatus.Success Then
                        Console.WriteLine($"积分失败: {status}")
                        Exit For
                    End If

                    Console.WriteLine("{0,12:E2} {1,15:E6} {2,15:E6} {3,12}",
                        tOut, y(0), y(1), solver.TotalSteps)
                Next

                ' 输出统计信息
                Console.WriteLine()
                Console.WriteLine("统计信息:")
                Console.WriteLine($"  总步数: {solver.TotalSteps}")
                Console.WriteLine($"  右端函数调用次数: {solver.RHSFunctionEvaluations}")
                Console.WriteLine($"  Newton迭代次数: {solver.NewtonIterations}")
                Console.WriteLine($"  线性求解次数: {solver.LinearSolves}")
            End Using

            Console.WriteLine()
        End Sub

#End Region

    End Class

End Namespace
