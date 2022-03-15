#Region "Microsoft.VisualBasic::ab774272ec46a91216c982d4a4813469, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Monte-Carlo\EstimatesProtocol.vb"

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

    '   Total Lines: 348
    '    Code Lines: 242
    ' Comment Lines: 63
    '   Blank Lines: 43
    '     File Size: 16.42 KB


    '     Module EstimatesProtocol
    ' 
    '         Function: __getRanges, DllParser, GetEigenvector, GetEntityNumbers, GetRandomParameters
    '                   Gety0, (+2 Overloads) Iterations, Run, (+2 Overloads) Sampling
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MonteCarlo

    ''' <summary>
    ''' 使用蒙特卡洛的方法估算出系统的参数，不过这个方法的效率太低了，没有遗传算法的效率好
    ''' </summary>
    Public Module EstimatesProtocol

        ''' <summary>
        ''' 加载dll文件之中的计算模型
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <returns></returns>
        Public Function DllParser(dll As String) As IEnumerable(Of Type)
            Dim assem As Assembly = Assembly.LoadFrom(dll.GetFullPath)
            Dim types As Type() = assem.GetTypes
            Dim model As Type = GetType(Model)

            Return From type As Type
                   In types
                   Where (type.IsInheritsFrom(model, strict:=False) AndAlso
                       Not type.IsAbstract)  ' a bug???
                   Select type
        End Function

        <Extension>
        Public Function Gety0(def As Type) As ValueRange()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.yinit
        End Function

        <Extension>
        Public Function GetRandomParameters(def As Type) As ValueRange()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.params
        End Function

        ''' <summary>
        ''' Sampling method of the y output values.(假若模型定义之中没有定义这个特征向量的构建方法的话，则使用默认的方法：平均数+标准差)
        ''' </summary>
        ''' <param name="def"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetEigenvector(def As Type) As Dictionary(Of String, Eigenvector)
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Dim eigenvectors As Dictionary(Of String, Eigenvector) = model.eigenvector

            If eigenvectors Is Nothing Then
                eigenvectors = Model _
                    .GetVariables(def) _
                    .DefaultEigenvector
            End If

            Return eigenvectors
        End Function

        ''' <summary>
        ''' 加载目标dll之中的计算模型然后提供计算数据
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="k"></param>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Run(dll As String, k As Long, n As Integer, a As Integer, b As Integer) As IEnumerable(Of ODEsOut)
            Dim model As Type = DllParser(dll).First
            Dim y0 = model.Gety0 _
                .Select(Function(v) New NamedValue(Of IValueProvider) With {
                    .Name = v.Name,
                    .Value = AddressOf v.GetValue
                })
            Dim parms = model.GetRandomParameters _
                .Select(Function(v) New NamedValue(Of IValueProvider) With {
                    .Name = v.Name,
                    .Value = AddressOf v.GetValue
                })
            Return model.Bootstrapping(parms, y0, k, n, a, b,,)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="eigenvector"></param>
        ''' <param name="partN"></param>
        ''' <param name="merge">
        ''' + 假若是从文件之中加在的数据，则这个默认参数值不需要进行设置
        ''' + 假若是直接赋值，则需要设置本参数为True进行y0和参数的合并操作
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Sampling(data As IEnumerable(Of ODEsOut),
                                          eigenvector As Dictionary(Of String, Eigenvector),
                                          Optional partN As Integer = 20,
                                          Optional merge As Boolean = False,
                                          Optional echo As Boolean = True) _
                                                         As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double)))
            If echo Then
                Call "Data sampling as eigenvector.....".__DEBUG_ECHO
            End If

            For Each x As ODEsOut In data
                If merge Then
                    Call x.Join()
                End If

                Yield x.Sampling(eigenvector, partN)
            Next

            If echo Then
                Call "Sampling job done!".__DEBUG_ECHO
            End If
        End Function

        Public Const Observation As String = NameOf(Observation)

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="eigenvector"></param>
        ''' <param name="partN"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Sampling(x As ODEsOut,
                                 eigenvector As Dictionary(Of String, Eigenvector),
                                 Optional partN As Integer = 20,
                                 Optional tag As String = Nothing) As VectorTagged(Of Dictionary(Of String, Double))

            Dim vector As New List(Of Double)

            For Each var As String In eigenvector.Keys
                Dim y As Double() = x.y(var).Value
                Dim n As Integer = CInt(y.Length / partN)

                For Each block As Double() In Parallel.Linq.SplitIterator(y, n, echo:=False)
                    vector += eigenvector(var)(block)
                Next
            Next

            Return New VectorTagged(Of Dictionary(Of String, Double)) With {
                .Tag = vector.ToArray,   ' 所提取采样出来的特征向量
                .Value = x.params,       ' 生成原始数据的参数列表
                .TagStr = tag
            }
        End Function

        ''' <summary>
        ''' k是采样的次数， n,a,b 是进行ODEs计算的参数，可以直接从观测数据之中提取出来，<paramref name="expected"/>是期望的cluster数量
        ''' </summary>
        ''' <param name="model">必须是继承自<see cref="Model"/>类型</param>
        ''' <param name="observation">实验观察里面只需要y值列表就足够了，不需要参数信息</param>
        ''' <param name="k"></param>
        ''' <param name="expected"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="work">工作的临时文件夹工作区间，默认使用dll的文件夹</param>
        ''' <param name="outIterates">每一次的迭代结果都会从这里返回</param>
        ''' <returns>函数返回收敛成功了之后的最后一次迭代的参数数据</returns>
        <Extension>
        Public Function Iterations(model As Type, observation As ODEsOut, k&,
                                   Optional expected% = 10,
                                   Optional stop% = -1,
                                   Optional partN% = 20,
                                   Optional cut# = 0.3,
                                   Optional work$ = Nothing,
                                   Optional parallel As Boolean = False,
                                   Optional ByRef outIterates As Dictionary(Of String, Dictionary(Of String, Double)()) = Nothing) _
                                                              As Dictionary(Of String, Double)()

            Dim y0 As New Dictionary(Of NamedValue(Of IValueProvider))(
                model.Gety0 _
                .Select(Function(v) v.GetRandomModel))
            Dim parms As New Dictionary(Of NamedValue(Of IValueProvider))(
                model.GetRandomParameters _
                .Select(Function(v) v.GetRandomModel))
            Dim eigenvectors As Dictionary(Of String, Eigenvector) = model.GetEigenvector
            Dim n As Integer = observation.x.Length
            Dim a As Integer = observation.x.Min
            Dim b As Integer = observation.x.Max

            If work Is Nothing Then
                work = model.Assembly.Location.TrimSuffix & $"-MonteCarlo-{App.PID}/"
            End If

            Dim experimentObservation As VectorTagged(Of Dictionary(Of String, Double)) =
                observation.Sampling(eigenvectors, partN, EstimatesProtocol.Observation)
            Dim i As VBInteger = 0

            Call observation.params.GetJson.__DEBUG_ECHO

            Dim uid As New Uid
            outIterates = New Dictionary(Of String, Dictionary(Of String, Double)())

            Do While True
                Dim randSamples = experimentObservation.Join(
                    model.Bootstrapping(parms.Values,
                                        y0.Values,
                                        k, n, a, b,,
                                        parallel:=parallel) _
                    .Sampling(eigenvectors,
                              partN,
                              merge:=True))
                Dim kmeansResult As Dictionary(Of Double(), NamedValue(Of Dictionary(Of String, Double)())()) =
                    randSamples.KMeans(expected, [stop])
                Dim required As VectorTagged(Of NamedValue(Of Dictionary(Of String, Double)())()) = Nothing

                For Each cluster In kmeansResult
                    For Each x As NamedValue(Of Dictionary(Of String, Double)()) In cluster.Value
                        If Not String.IsNullOrEmpty(x.Name) Then
                            required = New VectorTagged(Of NamedValue(Of Dictionary(Of String, Double)())()) With {
                                .Tag = cluster.Key,
                                .Value = cluster.Value
                            }
                            Exit For
                        End If
                    Next

                    If Not required Is Nothing Then
                        Exit For
                    End If
                Next

                If required.Value.Length = 1 Then
                    ' 只有一个元素的时候，就只是实验观察数据本身，则不修改范围，直接下一次迭代
                    Call "Current iteration is not valid: Required output just one element(observation itself)!".Warning
                    Continue Do
                End If

                Dim total As Integer = GetEntityNumbers(kmeansResult.Values.ToArray)
                Dim requires As Integer = GetEntityNumbers(required.Value)
                Dim out As Dictionary(Of String, Double)() =  ' 请注意，由于在这里是进行实验数据的计算模型的参数拟合，所以观测数据的参数是不需要的，要从output里面去除掉
                    required.Value _
                    .Where(Function(x) Not x.Name = EstimatesProtocol.Observation) _
                    .Select(Function(x) x.Value) _
                    .ToVector
                Dim key$ = New NamedValue(Of String)(
                    (+uid).ToString,
                    randSamples.Count).GetJson

                Call outIterates.Add(key$, out)

                If requires / total >= cut Then  ' 已经满足条件了，准备返回数据
                    Return out
                Else
                    Dim output As Dictionary(Of String, Double()) =
                        out.IteratesALL _
                        .GroupBy(Function(x) x.Key) _
                        .ToDictionary(
                            Function(x) x.Key,
                            Function(x) x.Select(
                            Function(o) o.Value).ToArray)

                    ' 调整y0和参数列表
                    For Each y As String In y0.Keys.ToArray
                        Dim values As Double() = output(y)
                        Dim range As IValueProvider = values.__getRanges
                        y0(y) = New NamedValue(Of IValueProvider)(y, range)
                    Next
                    For Each parm In parms.Keys.ToArray
                        Dim values As Double() = output(parm)
                        Dim range As IValueProvider = values.__getRanges

                        parms(parm) = New NamedValue(Of IValueProvider) With {
                            .Name = parm,
                            .Value = range
                        }
                    Next

                    ' 保存临时数据到工作区间
                    Call output.ToDictionary(
                        Function(x) x.Key,
                        Function(x) x.Value.Average).GetJson.__DEBUG_ECHO
                    Call output.ToDictionary(
                        Function(x) x.Key,
                        Function(x) New DoubleTagged(Of Double()) With {
                            .Tag = x.Value.Average,
                            .Value = x.Value
                        }).GetJson _
                          .SaveTo(work & $"/{FormatZero(++i, "00000")}.json")
                End If
            Loop

            Return Nothing ' 这里永远都不会被执行
        End Function

        ''' <summary>
        ''' k是采样的次数， n,a,b 是进行ODEs计算的参数，可以直接从观测数据之中提取出来，<paramref name="expected"/>是期望的cluster数量
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="observation">实验观察里面只需要y值列表就足够了，不需要参数信息</param>
        ''' <param name="k"></param>
        ''' <param name="expected"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="work">工作的临时文件夹工作区间，默认使用dll的文件夹</param>
        Public Function Iterations(dll As String,
                                   observation As ODEsOut,
                                   k As Long,
                                   expected As Integer,
                                   Optional [stop] As Integer = -1,
                                   Optional partN As Integer = 20,
                                   Optional cut As Double = 0.3,
                                   Optional work As String = Nothing) As Dictionary(Of String, Double)()

            Dim model As Type = DllParser(dll).First

            If model Is Nothing Then  ' 没有从目标程序集之中查找到计算模型的定义
                Dim msg As String =
                    $"Unable found model from assembly {dll}, a calculation model should inherits from type <see cref=""{GetType(Model).FullName}""/>."
                Throw New NotImplementedException(msg)
            End If

            Return model.Iterations(observation, k, expected, [stop], partN, cut, work)
        End Function

        <Extension>
        Private Function __getRanges(values As Double()) As IValueProvider
            Dim low As Double = values.Min
            Dim high As Double = values.Max
            Return RandomRange.GetRandom(low, high,, forceInit:=True)
        End Function

        Public Function GetEntityNumbers(ParamArray data As NamedValue(Of Dictionary(Of String, Double)())()()) As Integer
            Dim array = data _
                .IteratesALL _
                .Select(Function(x) x.Value) _
                .IteratesALL
            Dim value As Integer = array.Count
            Return value
        End Function
    End Module
End Namespace
