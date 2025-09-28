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
                Dim type As TypeInfo = CType(Me.GetType, TypeInfo)
                Dim fields As IEnumerable(Of FieldInfo) = type _
                    .DeclaredFields _
                    .Where(Function(f)
                               Return f.FieldType.Equals(GetType(Double))
                           End Function)
                Dim out As Dictionary(Of String, Double) = fields _
                    .ToDictionary(Function(x) x.Name,
                                  Function(x)
                                      Return DirectCast(x.GetValue(Me), Double)
                                  End Function)

                fields = type _
                    .DeclaredFields _
                    .Where(Function(f)
                               Return (Not f.FieldType.Equals(GetType(var))) AndAlso f.FieldType.Equals(GetType(f64))
                           End Function)

                For Each v As FieldInfo In fields
                    Call out.Add(v.Name, DirectCast(v.GetValue(Me), f64).Value)
                Next

                Return out
            End Get
        End Property

        Sub New()
            Dim type As TypeInfo = CType(Me.GetType, TypeInfo)
            Dim fields As FieldInfo() = type.DeclaredFields _
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

            __vars = vars.ToDictionary(Function(a) a.Name)
        End Sub

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
        ''' 
        ''' </summary>
        ''' <param name="incept">是否是为蒙特卡洛实验设计的？</param>
        ''' <returns></returns>
        Public Function GetY0(Optional incept As Boolean = False) As Double()
            Dim symbols As var() = If(incept, Me.vars, Me.y0)

            Return symbols _
                .OrderBy(Function(o) o.Index) _
                .Select(Function(o) o.Value) _
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

        Friend Sub ODEs(dx As Double, y As Vector, ByRef k As Vector)
            ' 更新设置y的值
            For Each x As var In vars
                x.Value = y(x.Index)
            Next

            k = k.ImputeNA(fill_as:=0.0)

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
