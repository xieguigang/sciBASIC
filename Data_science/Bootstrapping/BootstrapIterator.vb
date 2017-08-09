#Region "Microsoft.VisualBasic::bd9efea74bdab57add63267bfa887040, ..\sciBASIC#\Data_science\Bootstrapping\BootstrapIterator.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Terminal.Utility

''' <summary>
''' 参数估计的过程之中的迭代器，这个模块内的函数主要是用来产生数据源的
''' </summary>
Public Module BootstrapIterator

    ''' <summary>
    ''' 这个更加适合没有任何参数信息的时候的情况
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="range"></param>
    ''' <param name="vars"></param>
    ''' <param name="yinit"></param>
    ''' <param name="k"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="trimNaN"></param>
    ''' <param name="parallel">并行计算模式有极大的内存泄漏的危险</param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function Bootstrapping(Of T As ODEs)(
                                 range As PreciseRandom,
                                  vars As IEnumerable(Of String),
                                 yinit As IEnumerable(Of String),
                                     k As Long,
                                     n As Integer,
                                     a As Double,
                                     b As Double,
                                 Optional trimNaN As Boolean = True,
                                 Optional parallel As Boolean = False) As IEnumerable(Of ODEsOut)

        Dim varList = vars.Select(Function(x) New NamedValue(Of IValueProvider)(x, AddressOf range.NextNumber))
        Dim y0 = yinit.Select(Function(x) New NamedValue(Of IValueProvider)(x, AddressOf range.NextNumber))

        Return GetType(T).Bootstrapping(
            parameters:=varList,
            y0:=y0,
            k:=k,
            n:=n,
            a:=a,
            b:=b,
            trimNaN:=trimNaN,
            parallel:=parallel)
    End Function

    Public Function Bootstrapping(model As Type,
                                   vars As IEnumerable(Of NamedValue(Of PreciseRandom)),
                                  yinit As IEnumerable(Of NamedValue(Of PreciseRandom)),
                                      k As Long,
                                      n As Integer,
                                      a As Double,
                                      b As Double,
                                  Optional trimNaN As Boolean = True,
                                  Optional parallel As Boolean = False) As IEnumerable(Of ODEsOut)
        Return model.Bootstrapping(
            vars.Select(Function(x) New NamedValue(Of IValueProvider) With {
                .Name = x.Name,
                .Value = AddressOf x.Value.NextNumber
            }),
            yinit.Select(Function(x) New NamedValue(Of IValueProvider) With {
                .Name = x.Name,
                .Value = AddressOf x.Value.NextNumber
            }),
            k, n, a, b, trimNaN, parallel)
    End Function

    ''' <summary>
    ''' For populates the random system status
    ''' </summary>
    ''' <param name="model"></param>
    ''' <param name="args"></param>
    ''' <param name="y0"></param>
    ''' <param name="k"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="trimNaN"></param>
    ''' <param name="parallel"></param>
    ''' <param name="echo"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Bootstrapping(model As Type,
                                   args As Dictionary(Of String, Double),
                                     y0 As IEnumerable(Of NamedValue(Of IValueProvider)),
                                      k As Long,
                                      n As Integer,
                                      a As Double,
                                      b As Double,
                                  Optional trimNaN As Boolean = True,
                                  Optional parallel As Boolean = False,
                                  Optional echo As Boolean = True) As IEnumerable(Of ODEsOut)
        Dim vars = args.Select(
            Function(v) New NamedValue(Of IValueProvider) With {
                .Name = v.Key,
                .Value = Function() v.Value  ' 在研究可能的系统状态的时候，参数值是固定不变的，只变化初始状态值y0
            })
        Return model.Bootstrapping(vars, y0, k, n, a, b, trimNaN, parallel, echo)
    End Function

    ''' <summary>
    ''' Bootstrapping 参数估计分析，这个函数用于生成基本的采样数据
    ''' </summary>
    ''' <param name="parameters">各个参数的变化范围</param>
    ''' <param name="model">具体的求解方程组</param>
    ''' <param name="k">重复的次数</param>
    ''' <param name="y0">
    ''' ``Y0``初值，在进行参数估计的时候应该是被固定的，在进行系统状态分布的计算的时候才是随机的
    ''' </param>
    ''' <param name="parallel">并行计算模式有极大的内存泄漏的危险</param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Iterator Function Bootstrapping(model As Type,
                                      parameters As IEnumerable(Of NamedValue(Of IValueProvider)),
                                              y0 As IEnumerable(Of NamedValue(Of IValueProvider)),
                                               k As Long,
                                               n As Integer,
                                               a As Double,
                                               b As Double,
                                           Optional trimNaN As Boolean = True,
                                           Optional parallel As Boolean = False,
                                           Optional echo As Boolean = True) As IEnumerable(Of ODEsOut)

        Dim params As NamedValue(Of IValueProvider)() = parameters.ToArray
        Dim i&
        Dim ps As Dictionary(Of String, Action(Of Object, Double)) =
            params _
            .Select(Function(x) x.Name) _
            .SetParameters(model)
        Dim yinit As NamedValue(Of IValueProvider)() = y0.ToArray
        Dim proc As New EventProc(k, "Bootstrapping Samples")

        If echo Then
            Call "Start bootstrapping data samples...".__DEBUG_ECHO
        End If

        If parallel Then
            proc.Capacity = k * 0.4

            ' memory leaks on linux
            ' 2016-9-28，可能是由于生成csv文件的时候字符串没有被正确的释放所导致内存泄漏，如果只是执行这段代码的话，经过测试没有内存泄漏的危险

            For Each x As ODEsOut In From it As Long ' 进行n次并行的采样计算
                                     In k.SeqIterator.AsParallel
                                     Let odes_Out As ODEsOut = params _
                                         .Iterates(model, yinit, ps, n, a, b)
                                     Let isNaNResult As Boolean = odes_Out.HaveNaN
                                     Where If(trimNaN, Not isNaNResult, True) ' 假若不需要trim，则总是True，即返回所有数据
                                     Select odes_Out
                i += 1L
                Yield x

                If echo Then
                    Call proc.Tick()
                End If
            Next
        Else
            For Each it As Long In k.SeqIterator
                Dim odes_Out = params.Iterates(model, yinit, ps, n, a, b)
                Dim isNaNResult As Boolean = odes_Out.HaveNaN

                If If(trimNaN, Not isNaNResult, True) Then ' 假若不需要trim，则总是True，即返回所有数据
                    i += 1L
                    Yield odes_Out
                End If
                If echo Then
                    Call proc.Tick()
                End If
            Next
        End If

        If echo Then
            Call $"Bootstrapping populated {i}({Math.Round(100 * i / k, 2)}%) valid samples...".__DEBUG_ECHO
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="parms"></param>
    ''' <param name="y0"></param>
    ''' <param name="ps"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    ''' <remarks>在Linux服务器上面有内存泄漏的危险</remarks>
    <Extension>
    Public Function Iterates(parms As NamedValue(Of IValueProvider)(),
                             model As Type,
                             y0 As NamedValue(Of IValueProvider)(),
                             ps As Dictionary(Of String, Action(Of Object, Double)),
                             n%, a#, b#) As ODEsOut

        Dim odes As ODEs = DirectCast(Activator.CreateInstance(model), ODEs)
        ' Dim debug As New List(Of NamedValue(Of Double))

        For Each x As NamedValue(Of IValueProvider) In parms
            Dim value As Double = x.Value()()
            Call ps(x.Name)(odes, value)  ' 设置方程的参数的值

            'debug += New NamedValue(Of Double) With {
            '    .Name = x.Name,
            '    .x = value
            '}
        Next

        For Each y In y0
            Dim value As Double = y.Value()()
            odes(y.Name).value = value
            'debug += New NamedValue(Of Double) With {
            '    .Name = y.Name,
            '    .x = value
            '}
        Next

        ' Call debug.GetJson.__DEBUG_ECHO

        Return odes.Solve(n, a, b, incept:=True)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="model"><see cref="ODEs"/>类型</param>
    ''' <param name="vars"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SetParameters(vars As IEnumerable(Of String), model As Type) As Dictionary(Of String, Action(Of Object, Double))
        Dim ps As New Dictionary(Of String, Action(Of Object, Double))

        For Each var As String In vars
            ps(var) = model.FieldSet(Of Double)(var)
        Next

        Return ps
    End Function
End Module
