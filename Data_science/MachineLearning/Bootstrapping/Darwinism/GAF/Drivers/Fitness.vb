#Region "Microsoft.VisualBasic::ab4e7a5c6fec659103ec9c36088b442f, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Darwinism\GAF\Drivers\Fitness.vb"

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

    '   Total Lines: 157
    '    Code Lines: 89
    ' Comment Lines: 47
    '   Blank Lines: 21
    '     File Size: 5.51 KB


    '     Delegate Function
    ' 
    ' 
    '     Class GAFFitness
    ' 
    '         Properties: Cacheable, Ignores, Model, modelVariables, observation
    '                     weights, y0
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Calculate, RunTest
    ' 
    '         Sub: __init
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping.Darwinism.GAF.ODEs
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Darwinism.GAF.Driver

    ''' <summary>
    ''' 计算当前的最好的参数的fitness
    ''' </summary>
    ''' <param name="best">当前代之中的最好的参数</param>
    ''' <param name="fit"></param>
    ''' <returns></returns>
    Public Delegate Function FitnessCompute(best As ParameterVector, fit As GAFFitness) As Double

    Public Class GAFFitness : Implements Fitness(Of ParameterVector)

        ''' <summary>
        ''' 真实的实验观察数据
        ''' </summary>
        Public ReadOnly Property observation As ODEsOut
        ''' <summary>
        ''' 具体的计算模型
        ''' </summary>
        Public ReadOnly Property Model As Type

        ''' <summary>
        ''' 计算的采样数
        ''' </summary>
        Dim samples%
        ''' <summary>
        ''' 样本列表部分计算的参考值
        ''' </summary>
        Dim ref As ODEsOut
        ''' <summary>
        ''' 模型之中所定义的y变量
        ''' </summary>
        Public ReadOnly Property modelVariables As String()

#Region "Friend visit for dump debug module and run test for fitness calc"

        ''' <summary>
        ''' ODEs y0
        ''' </summary>
        Public ReadOnly Property y0 As Dictionary(Of String, Double)
        ''' <summary>
        ''' RK4 parameters
        ''' </summary>
        Friend n%, a#, b#
#End Region

        Public log10Fitness As Boolean
        ''' <summary>
        ''' 被忽略比较的y变量名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Ignores As String()
        Public Property weights As Dictionary(Of String, Double)

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of ParameterVector).Cacheable
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation"></param>
        Sub New(observation As Dictionary(Of String, Double), model As MonteCarlo.Model, n%, a#, b#)
            With Me
                ._observation = model.RunTest(observation, n, a, b)
                ._Model = model.GetType
                .n = n
                .a = a
                .b = b
            End With

            Call __init()
        End Sub

        ''' <summary>
        ''' 初始化一些共同的数据
        ''' </summary>
        Private Sub __init()
            With Me
                .samples = CInt(n / 100)
                ._y0 = observation _
                    .y _
                    .Values _
                    .ToDictionary(Function(v) v.Name,
                                  Function(y) y.Value(0))
                .Ignores = {}
                ._modelVariables = MonteCarlo.Model _
                    .GetVariables(Model) _
                    .ToArray

                Call .Model.FullName.Warning
            End With
        End Sub

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation">只需要其中的<see cref="ODEsOut.y"/>有数据就行了</param>
        Sub New(model As Type, observation As ODEsOut, initOverrides As Dictionary(Of String, Double), isRef As Boolean)
            With Me
                ._observation = observation
                ._Model = model
                .n = observation.x.Length
                .a = observation.x(0)
                .b = observation.x.Last

                If isRef Then
                    ref = observation
                End If
            End With

            Call __init()

            If Not initOverrides.IsNullOrEmpty Then
                For Each k$ In initOverrides.Keys
                    y0(k$) = initOverrides(k$)
                Next

                Call $"Overrides y0: {initOverrides.GetJson}".__DEBUG_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 使用指定的参数测试计算模型的输出
        ''' </summary>
        ''' <param name="parms"></param>
        ''' <returns></returns>
        Public Function RunTest(parms As Dictionary(Of String, Double)) As ODEsOut
            Dim out As ODEsOut = ' y0使用实验观测值，而非突变的随机值
                MonteCarlo.Model.RunTest(Model, y0, parms, n, a, b, ref)  ' 通过拟合的参数得到具体的计算数据
            Return out
        End Function

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector).Calculate
            Dim varsToCompares As String() = modelVariables _
                .Where(Function(v) Array.IndexOf(Ignores, v) = -1) _
                .ToArray

            Return Model.GetFitness(
                chromosome,
                observation,
                varsToCompares,
                y0,
                n, a, b,
                log10Fitness,
                ref, weights)
        End Function
    End Class
End Namespace
