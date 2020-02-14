#Region "Microsoft.VisualBasic::24156be8c06fe9887d554576f8a068b1, Data_science\Mathematica\Math\ODE\ODEsSolver\ODEs.vb"

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

    ' Class ODEs
    ' 
    '     Properties: Parameters
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: __getY0, GetParameters, GetVariables, Solve, TimePopulator
    ' 
    '     Sub: ODEs, rungeKutta
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

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

    Dim K1, K2, K3, K4 As Vector

    ReadOnly __vars As Dictionary(Of var)
    Protected Friend vars As var()

    Public Const y0RefName As String = NameOf(__vars)

    Default Public ReadOnly Property GetVar(Name$) As var
        Get
            Return __vars(Name)
        End Get
    End Property

    Sub New()
        Dim type As TypeInfo = CType(Me.GetType, TypeInfo)
        Dim fields = type.DeclaredFields _
            .Where(Function(x) x.FieldType.Equals(var.type)) _
            .ToArray

        vars = New var(fields.Length - 1) {}

        For Each f As SeqValue(Of FieldInfo) In fields.SeqIterator
            Dim x As New var() With {
                .Name = f.value.Name,
                .Index = f.i
            }
            vars(f.i) = x

            Call f.value.SetValue(Me, x)
        Next

        __vars = New Dictionary(Of var)(vars)
    End Sub

    Protected Sub New(vars As var())
        Me.vars = vars
        Me.__vars = New Dictionary(Of var)(vars)
    End Sub

    ''' <summary>
    ''' RK4 ODEs solver
    ''' </summary>
    ''' <param name="dxn">The x initial value.(x初值)</param>
    ''' <param name="dyn">The y initial value.(初值y(n))</param>
    ''' <param name="dh">Steps delta.(步长)</param>
    ''' <param name="dynext">
    ''' Returns the y(n+1) result from this parameter.(下一步的值y(n+1))
    ''' </param>
    Private Sub rungeKutta(dxn As Double,
                           ByRef dyn As Vector,
                           dh As Double,
                           ByRef dynext As Vector)

        Call ODEs(dxn, dyn, K1)                             ' 求解K1
        Call ODEs(dxn + dh / 2, dyn + dh / 2 * K1, K2)      ' 求解K2
        Call ODEs(dxn + dh / 2, dyn + dh / 2 * K2, K3)      ' 求解K3
        Call ODEs(dxn + dh, dyn + dh * K3, K4)              ' 求解K4

        dynext = dyn + (K1 + K2 + K3 + K4) * dh / 6.0  ' 求解下一步的值y(n+1)
    End Sub

    ''' <summary>
    ''' 初值
    ''' </summary>
    ''' <returns></returns>
    Protected MustOverride Function y0() As var()

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
    ''' <returns></returns>
    Private Function __getY0(incept As Boolean) As Double()
        Return If(incept, Me.vars, Me.y0) _
            .OrderBy(Function(o) o.Index) _
            .Select(Function(o) o.value) _
            .ToArray
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
        Dim dh As Double = (b - a) / n  ' 步长
        Dim dx As Double = a
        Dim y0 As Double() = __getY0(incept)
        Dim darrayn As New Vector(y0)
        Dim darraynext As New Vector(y0.Length) ' //下一步的值,最好初始化

        K1 = New Vector(y0.Length)
        K2 = New Vector(y0.Length)
        K3 = New Vector(y0.Length)
        K4 = New Vector(y0.Length)

        Dim y As List(Of Double)() = New List(Of Double)(vars.Length - 1) {}
        Dim x As New List(Of Double)
        Dim yinit As New Dictionary(Of String, Double)

        For Each var As var In vars ' 记录y0
            yinit(var.Name) = darrayn(var.Index)
        Next

        For i As Integer = 0 To n
            Call rungeKutta(dx, darrayn, dh, darraynext)

            x += dx
            dx += dh
            darrayn = darraynext

            For Each var In vars ' y
                y(var) += darrayn(var.Index)
                var.Value = darrayn(var.Index)
            Next
        Next

        Dim out = LinqAPI.MakeList(Of NamedCollection(Of Double)) <=
 _
            From var As var
            In vars
            Select New NamedCollection(Of Double) With {
                .Name = var.Name,
                .Value = y(var)
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
            .y0 = yinit,
            .params = Parameters
        }
    End Function

    ''' <summary>
    ''' 在这里计算具体的微分方程组
    ''' </summary>
    ''' <param name="dx"></param>
    ''' <param name="dy"></param>
    Protected MustOverride Sub func(dx#, ByRef dy As Vector)

    Private Sub ODEs(dx As Double, y As Vector, ByRef k As Vector)
        ' 更新设置y的值
        For Each x As var In vars
            x.value = y(x.Index)
        Next

        Call func(dx, dy:=k)
    End Sub

    ''' <summary>
    ''' 返回的值包括<see cref="Double"/>类型的Field或者<see cref="float"/>类型的field
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Parameters() As Dictionary(Of String, Double)
        Get
            Dim type As TypeInfo = CType(Me.GetType, TypeInfo)
            Dim fields As IEnumerable(Of FieldInfo) =
                type _
                .DeclaredFields _
                .Where(Function(f) f.FieldType.Equals(GetType(Double)))
            Dim out As Dictionary(Of String, Double) =
                fields.ToDictionary(
                Function(x) x.Name,
                Function(x) DirectCast(x.GetValue(Me), Double))

            fields = type _
                .DeclaredFields _
                .Where(Function(f) (Not f.FieldType.Equals(GetType(var))) AndAlso f.FieldType.Equals(GetType(f64)))

            For Each v As FieldInfo In fields
                Call out.Add(v.Name, DirectCast(v.GetValue(Me), f64).Value)
            Next

            Return out
        End Get
    End Property

    ''' <summary>
    ''' Get function parameters name collection
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetParameters(model As Type) As IEnumerable(Of String)
        Dim fields = CType(model, TypeInfo) _
            .DeclaredFields _
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
        Dim fields = CType(model, TypeInfo) _
            .DeclaredFields _
            .Where(Function(f)
                       Return f.FieldType.Equals(GetType(var))
                   End Function)
        Return fields.Select(Function(f) f.Name)
    End Function
End Class
