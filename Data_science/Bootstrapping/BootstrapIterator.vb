#Region "Microsoft.VisualBasic::1d5db2f2348598c905774cd3714b6bf7, ..\visualbasic_App\Data_science\Bootstrapping\BootstrapIterator.vb"

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
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.Serialization.JSON

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
                                              a As Integer,
                                              b As Integer,
                                           Optional trimNaN As Boolean = True,
                                           Optional parallel As Boolean = False) As IEnumerable(Of ODEsOut)

        Dim varList = vars.Select(Function(x) New NamedValue(Of PreciseRandom)(x, range))
        Dim y0 = yinit.Select(Function(x) New NamedValue(Of PreciseRandom)(x, range))
        Return GetType(T).Bootstrapping(varList, y0, k, n, a, b, trimNaN, parallel)
    End Function

    ''' <summary>
    ''' Bootstrapping 参数估计分析，这个函数用于生成基本的采样数据
    ''' </summary>
    ''' <param name="vars">各个参数的变化范围</param>
    ''' <param name="model">具体的求解方程组</param>
    ''' <param name="k">重复的次数</param>
    ''' <param name="yinit">``Y0``初值</param>
    ''' <param name="parallel">并行计算模式有极大的内存泄漏的危险</param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Iterator Function Bootstrapping(model As Type,
                                           vars As IEnumerable(Of NamedValue(Of PreciseRandom)),
                                          yinit As IEnumerable(Of NamedValue(Of PreciseRandom)),
                                              k As Long,
                                              n As Integer,
                                              a As Integer,
                                              b As Integer,
                                           Optional trimNaN As Boolean = True,
                                           Optional parallel As Boolean = False) As IEnumerable(Of ODEsOut)

        Dim params As NamedValue(Of PreciseRandom)() = vars.ToArray
        Dim y0 As NamedValue(Of PreciseRandom)() = yinit.ToArray
        Dim ps As Dictionary(Of String, Action(Of Object, Double)) =
            params _
            .Select(Function(x) x.Name) _
            .SetParameters(model)

        If parallel Then
            ' memory leaks on linux

            For Each x As ODEsOut In From it As Long ' 进行n次并行的采样计算
                                     In k.SeqIterator.AsParallel
                                     Let odes_Out = params.iterate(model, y0, ps, n, a, b)
                                     Let isNaNResult As Boolean = odes_Out.HaveNaN
                                     Where If(trimNaN, Not isNaNResult, True) ' 假若不需要trim，则总是True，即返回所有数据
                                     Select odes_Out
                Yield x
            Next
        Else
            For Each it As Long In k.SeqIterator
                Dim odes_Out = params.iterate(model, y0, ps, n, a, b)
                Dim isNaNResult As Boolean = odes_Out.HaveNaN
                If If(trimNaN, Not isNaNResult, True) Then ' 假若不需要trim，则总是True，即返回所有数据
                    Yield odes_Out
                End If
            Next
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="vars"></param>
    ''' <param name="yinis"></param>
    ''' <param name="ps"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    ''' <remarks>在Linux服务器上面有内存泄漏的危险</remarks>
    <Extension>
    Public Function iterate(vars As NamedValue(Of PreciseRandom)(),
                            model As Type,
                            yinis As NamedValue(Of PreciseRandom)(),
                            ps As Dictionary(Of String, Action(Of Object, Double)),
                            n As Integer,
                            a As Integer,
                            b As Integer) As ODEsOut

        Dim odes As Object = Activator.CreateInstance(model)
        ' Dim debug As New List(Of NamedValue(Of Double))

        For Each x In vars
            Dim value As Double = x.x.NextNumber
            Call ps(x.Name)(odes, value)  ' 设置方程的参数的值

            'debug += New NamedValue(Of Double) With {
            '    .Name = x.Name,
            '    .x = value
            '}
        Next

        For Each y In yinis
            Dim value As Double = y.x.NextNumber
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

