#Region "Microsoft.VisualBasic::1ecf7b7d63403b5e623e1ec526440a76, ..\visualbasic_App\Data_science\Bootstrapping\Monte-Carlo\AnalysisProtocol.vb"

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

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MonteCarlo

    Public Module AnalysisProtocol

        ''' <summary>
        ''' 加载dll文件之中的计算模型
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <returns></returns>
        Public Function DllParser(dll As String) As Type
            Dim assem As Assembly = Assembly.LoadFrom(dll.GetFullPath)
            Dim types As Type() = assem.GetTypes
            Dim model As Type = GetType(Model)

            Return LinqAPI.DefaultFirst(Of Type) <= From type As Type
                                                    In types
                                                    Where (type.IsInheritsFrom(model, strict:=False) AndAlso
                                                        Not type.IsAbstract)  ' a bug???
                                                    Select type
        End Function

        <Extension>
        Public Function Gety0(def As Type) As VariableModel()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.yinit
        End Function

        <Extension>
        Public Function GetRandomParameters(def As Type) As VariableModel()
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
            Dim model As Type = DllParser(dll)
            Dim y0 = model.Gety0
            Dim parms = model.GetRandomParameters
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
                Dim y As Double() = x.y(var).x
                Dim n As Integer = y.Length / partN

                For Each block As Double() In Parallel.Linq.SplitIterator(y, n, echo:=False)
                    vector += eigenvector(var)(block)
                Next
            Next

            Return New VectorTagged(Of Dictionary(Of String, Double)) With {
                .Tag = vector.ToArray,   ' 所提取采样出来的特征向量
                .value = x.params,       ' 生成原始数据的参数列表
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

            Dim y0 As New Dictionary(Of NamedValue(Of INextRandomNumber))(model.Gety0)
            Dim parms As New Dictionary(Of NamedValue(Of INextRandomNumber))(model.GetRandomParameters)
            Dim eigenvectors As Dictionary(Of String, Eigenvector) = model.GetEigenvector
            Dim n As Integer = observation.x.Length
            Dim a As Integer = observation.x.Min
            Dim b As Integer = observation.x.Max

            If work Is Nothing Then
                work = model.Assembly.Location.TrimSuffix & $"-MonteCarlo-{App.PID}/"
            End If

            Dim experimentObservation As VectorTagged(Of Dictionary(Of String, Double)) =
                observation.Sampling(eigenvectors, partN, AnalysisProtocol.Observation)
            Dim i As int = 0

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
                    For Each x In cluster.Value
                        If Not String.IsNullOrEmpty(x.Name) Then
                            required = New VectorTagged(Of NamedValue(Of Dictionary(Of String, Double)())()) With {
                                .Tag = cluster.Key,
                                .value = cluster.Value
                            }
                            Exit For
                        End If
                    Next

                    If Not required Is Nothing Then
                        Exit For
                    End If
                Next

                If required.value.Length = 1 Then
                    ' 只有一个元素的时候，就只是实验观察数据本身，则不修改范围，直接下一次迭代
                    Call "Current iteration is not valid: Required output just one element(observation itself)!".Warning
                    Continue Do
                End If

                Dim total As Integer = GetEntityNumbers(kmeansResult.Values.ToArray)
                Dim requires As Integer = GetEntityNumbers(required)
                Dim out As Dictionary(Of String, Double)() =  ' 请注意，由于在这里是进行实验数据的计算模型的参数拟合，所以观测数据的参数是不需要的，要从output里面去除掉
                    required.value _
                    .Where(Function(x) Not x.Name = AnalysisProtocol.Observation) _
                    .Select(Function(x) x.x) _
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
                            Function(x) x.ToArray(
                            Function(o) o.Value))

                    ' 调整y0和参数列表
                    For Each y In y0.Keys.ToArray
                        Dim values As Double() = output(y)
                        Dim range As INextRandomNumber = values.__getRanges
                        y0(y) = New NamedValue(Of INextRandomNumber)(y, range)
                    Next
                    For Each parm In parms.Keys.ToArray
                        Dim values As Double() = output(parm)
                        Dim range As INextRandomNumber = values.__getRanges

                        parms(parm) = New NamedValue(Of INextRandomNumber) With {
                            .Name = parm,
                            .x = range
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
                            .value = x.Value
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

            Dim model As Type = DllParser(dll)

            If model Is Nothing Then  ' 没有从目标程序集之中查找到计算模型的定义
                Dim msg As String =
                    $"Unable found model from assembly {dll}, a calculation model should inherits from type <see cref=""{GetType(Model).FullName}""/>."
                Throw New NotImplementedException(msg)
            End If

            Return model.Iterations(observation, k, expected, [stop], partN, cut, work)
        End Function

        <Extension>
        Private Function __getRanges(values As Double()) As INextRandomNumber
            Dim low As Double = values.Min
            Dim high As Double = values.Max
            Return RandomRange.GetRandom(low, high,, forceInit:=True)
        End Function

        Public Function GetEntityNumbers(ParamArray data As NamedValue(Of Dictionary(Of String, Double)())()()) As Integer
            Dim array = data _
                .IteratesALL _
                .Select(Function(x) x.x) _
                .IteratesALL
            Dim value As Integer = array.Count
            Return value
        End Function
    End Module
End Namespace
