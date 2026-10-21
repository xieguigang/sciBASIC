#Region "Microsoft.VisualBasic::5deb090abc5b72a0d46c1467c9acc904, Data_science\Mathematica\Math\ODE\Dynamics\ODEs.vb"

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

    '   Total Lines: 240
    '    Code Lines: 128 (53.33%)
    ' Comment Lines: 77 (32.08%)
    '    - Xml Docs: 77.92%
    ' 
    '   Blank Lines: 35 (14.58%)
    '     File Size: 8.62 KB


    '     Class ODEs
    ' 
    '         Properties: Parameters
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: CreateOutput, GetParameters, GetVariables, GetY0, Solve
    '                   TimePopulator
    ' 
    '         Sub: ODEs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Concurrent
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Dynamics

    ''' <summary>
    ''' Solving ODEs in R language, as example for test this class:
    ''' 
    ''' ```R
    ''' func &lt;- function(t, x, parms) {
    '''    with(as.list(c(parms, x)), {
    '''    
    '''       dP &lt;- a * P      - b * C * P
    '''       dC &lt;- b * P * C  - c * C
    '''      
    '''       list(c(dP, dC))
    '''    })
    ''' }
    '''
    ''' y0    &lt;- c(P = 2, C = 1)
    ''' parms &lt;- c(a = 0.1, b = 0.1, c = 0.1)
    ''' out   &lt;- ode(y = y0, times = 0:100, func, parms = parms)
    ''' 
    ''' head(out)
    ''' plot(out)
    ''' ```
    ''' </summary>
    Public MustInherit Class ODEs

        ReadOnly __vars As Dictionary(Of String, var)
        Protected Friend vars As var()

        Public Const y0RefName As String = NameOf(__vars)

        ''' <summary>
        ''' The reflection cache of the parameter fields of each ODEs model type, the
        ''' <see cref="Parameters"/> property is evaluated in every <see cref="Solve"/> call,
        ''' so the expensive reflection metadata should be cached by the model type.
        ''' </summary>
        Private Shared ReadOnly parameterFields As New ConcurrentDictionary(Of Type, FieldInfo())
        ''' <summary>
        ''' The reflection cache of the state variable fields of each ODEs model type.
        ''' </summary>
        Private Shared ReadOnly variableFields As New ConcurrentDictionary(Of Type, FieldInfo())
        ''' <summary>
        ''' The parameter names cache which is required by the Monte Carlo fitting.
        ''' </summary>
        Private Shared ReadOnly parameterNames As New ConcurrentDictionary(Of Type, String())
        ''' <summary>
        ''' The state variable names cache.
        ''' </summary>
        Private Shared ReadOnly variableNames As New ConcurrentDictionary(Of Type, String())

        ''' <summary>
        ''' The number of the state variables in current ODEs system.(当前方程组的状态变量个数)
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property Size As Integer
            Get
                Return If(vars Is Nothing, 0, vars.Length)
            End Get
        End Property

        ''' <summary>
        ''' get variable by given unique name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetVar(name As String) As var
            Get
                Return __vars(name)
            End Get
        End Property

        ''' <summary>
        ''' 返回的值包括<see cref="Double"/>类型的Field或者<see cref="float"/>类型的field
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Parameters() As Dictionary(Of String, Double)
            Get
                Dim fields As FieldInfo() = parameterFields.GetOrAdd(Me.GetType, AddressOf GetParameterFields)
                Dim out As New Dictionary(Of String, Double)(fields.Length)

                For i As Integer = 0 To fields.Length - 1
                    Dim f As FieldInfo = fields(i)
                    Dim value As Object = f.GetValue(Me)

                    If f.FieldType.Equals(GetType(Double)) Then
                        out.Add(f.Name, CDbl(value))
                    Else
                        out.Add(f.Name, DirectCast(value, f64).Value)
                    End If
                Next

                Return out
            End Get
        End Property

        ''' <summary>
        ''' Get the parameter fields of the given ODEs model type: the <see cref="Double"/>
        ''' typed fields and the <see cref="f64"/> typed fields, but the <see cref="var"/>
        ''' typed state variables are excluded.
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Private Shared Function GetParameterFields(model As Type) As FieldInfo()
            Dim fields As FieldInfo() = CType(model, TypeInfo).DeclaredFields.ToArray
            Dim doubleFields = fields.Where(Function(f) f.FieldType.Equals(GetType(Double)))
            Dim f64Fields = fields.Where(Function(f)
                                             Return (Not f.FieldType.Equals(var.type)) AndAlso f.FieldType.Equals(GetType(f64))
                                         End Function)

            Return doubleFields.Concat(f64Fields).ToArray
        End Function

        Sub New()
            Dim fields As FieldInfo() = variableFields.GetOrAdd(Me.GetType, AddressOf GetVariableFields)

            If fields.Length = 0 Then
                ' 没有声明任何状态变量的方程组：给一个空数组而不是抛出异常
                vars = New var() {}
            Else
                vars = New var(fields.Length - 1) {}
            End If

            For Each f As SeqValue(Of FieldInfo) In fields.SeqIterator
                Dim x As New var() With {
                    .Name = f.value.Name,
                    .Index = f.i
                }
                vars(f.i) = x

                Call f.value.SetValue(Me, x)
            Next

            __vars = vars.ToDictionary(Function(a) a.Name)
        End Sub

        ''' <summary>
        ''' Get the state variable fields of the given ODEs model type.
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns></returns>
        Private Shared Function GetVariableFields(model As Type) As FieldInfo()
            Return CType(model, TypeInfo).DeclaredFields _
                .Where(Function(x) x.FieldType.Equals(var.type)) _
                .ToArray
        End Function

        Protected Sub New(vars As var())
            Me.vars = vars
            Me.__vars = vars.ToDictionary(Function(a) a.Name)
        End Sub

        ''' <summary>
        ''' 初值
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function y0() As var()

        ''' <summary>
        ''' Get the initial values of the state variables, the value array is indexed by
        ''' the <see cref="var.Index"/> of each state variable.
        ''' </summary>
        ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
        ''' <returns></returns>
        Public Function GetY0(Optional incept As Boolean = False) As Double()
            Dim symbols As var() = If(incept, Me.vars, Me.y0)

            If symbols Is Nothing OrElse symbols.Length = 0 Then
                Return New Double() {}
            End If

            Dim values As Double() = New Double(symbols.Length - 1) {}

            For i As Integer = 0 To symbols.Length - 1
                Dim v As var = symbols(i)

                values(v.Index) = v.Value
            Next

            Return values
        End Function

        ''' <summary>
        ''' Populates the data of <see cref="ODEsOut.x"/>
        ''' </summary>
        ''' <param name="n%"></param>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <returns></returns>
        Public Shared Iterator Function TimePopulator(n%, a#, b#) As IEnumerable(Of Double)
            Dim dh As Double = (b - a) / n  ' 步长
            Dim dx As Double = a

            For i As Integer = 0 To n
                Yield dx
                dx += dh
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n">A larger value of this parameter, will makes your result more precise.</param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function Solve(n As Integer, a As Double, b As Double, Optional incept As Boolean = False) As ODEsOut
            Dim y0 As Double() = GetY0(incept)
            Dim yinit As New Dictionary(Of String, Double)
            Dim x As Double() = Nothing
            Dim y As List(Of Double)() = Nothing

            For Each var As var In vars
                ' 记录y0
                yinit(var.Name) = y0(var.Index)
            Next

            Call New RungeKutta4(Me).Solve(y0, n, a, b).GetResult(x, y)

            Return CreateOutput(Me, yinit, x, y)
        End Function

        Public Shared Function CreateOutput(system As ODEs, y0 As Dictionary(Of String, Double), x As Double(), y As List(Of Double)()) As ODEsOut
            Dim out = LinqAPI.MakeList(Of NamedCollection(Of Double)) _
                                                                      _
                () <= From var As var
                      In system.vars
                      Select New NamedCollection(Of Double) With {
                          .name = var.Name,
                          .value = y(var).ToArray
                      }

            ' 强制进行内存回收，以应对在蒙特卡洛分析的时候的内存泄漏
            'GC.SuppressFinalize(K1)
            'GC.SuppressFinalize(K2)
            'GC.SuppressFinalize(K3)
            'GC.SuppressFinalize(K4)
            'GC.SuppressFinalize(darrayn)
            'GC.SuppressFinalize(darraynext)
            'GC.SuppressFinalize(vars)

            Return New ODEsOut With {
                .x = x,
                .y = out.ToDictionary,
                .y0 = y0,
                .params = system.Parameters
            }
        End Function

        ''' <summary>
        ''' 在这里计算具体的微分方程组
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected MustOverride Sub func(dx#, ByRef dy As Vector)

        ''' <summary>
        ''' Evaluate the right-hand side function of the ODEs system: the state variables
        ''' are synchronized from the state vector first, then the user overrided
        ''' <see cref="func"/> will be invoked for computing the derivatives.
        ''' (方程组的右端函数求值：先从状态向量同步各个变量的数值，再调用用户重写的 func 计算导数)
        ''' </summary>
        ''' <param name="dx">The current integration time.</param>
        ''' <param name="y">The state vector.</param>
        ''' <param name="k">The derivative vector buffer, it will be cleared before the evaluation.</param>
        Friend Sub ODEs(dx As Double, y As Vector, ByRef k As Vector)
            Dim values As Double() = y.Array
            Dim symbols As var() = vars

            ' 更新设置y的值
            For i As Integer = 0 To symbols.Length - 1
                Dim symbol As var = symbols(i)

                symbol.Value = values(symbol.Index)
            Next

            ' 导数缓冲清零：避免用户函数没有显式赋值的分量残留上一阶段的数值
            Dim derivatives As Double() = k.Array
            System.Array.Clear(derivatives, 0, derivatives.Length)

            Call func(dx, dy:=k)
        End Sub

        ''' <summary>
        ''' Get function parameters name collection
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetParameters(model As Type) As IEnumerable(Of String)
            Dim fields = CType(model, TypeInfo).DeclaredFields _
                .Where(Function(f)
                           Return (Not f.IsLiteral) AndAlso f.FieldType.Equals(GetType(Double))
                       End Function)

            Return fields.Select(Function(f) f.Name)
        End Function

        ''' <summary>
        ''' Get Y names
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetVariables(model As Type) As IEnumerable(Of String)
            Dim fields = CType(model, TypeInfo).DeclaredFields _
                .Where(Function(f)
                           Return f.FieldType.Equals(GetType(var))
                       End Function)

            Return fields.Select(Function(f) f.Name)
        End Function
    End Class
End Namespace
