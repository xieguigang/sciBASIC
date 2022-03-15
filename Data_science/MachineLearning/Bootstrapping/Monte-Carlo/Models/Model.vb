#Region "Microsoft.VisualBasic::9382ae738ab105898f408a9461cfb95c, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Monte-Carlo\Models\Model.vb"

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

    '   Total Lines: 185
    '    Code Lines: 97
    ' Comment Lines: 63
    '   Blank Lines: 25
    '     File Size: 7.65 KB


    '     Class Model
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+4 Overloads) RunTest, y0
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Emit
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MonteCarlo

    ''' <summary>
    ''' ``y`` 声明的类型为<see cref="var"/>类型的域;
    ''' ``parameter`` 声明的类型为<see cref="Double"/>类型的域
    ''' </summary>
    Public MustInherit Class Model : Inherits ODEs

        Sub New()
            Call MyBase.New()
        End Sub

        Protected Sub New(vars As var())
            Call MyBase.New(vars)
        End Sub

        ''' <summary>
        ''' 系统的初始值列表(应用于系统状态随机聚类)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function yinit() As ValueRange()
        ''' <summary>
        ''' 系统的状态列表，即方程里面的参数(应用于参数估计)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function params() As ValueRange()

        ''' <summary>
        ''' 在计算聚类的相似度的时候对y变量的特征提取
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function eigenvector() As Dictionary(Of String, Eigenvector)

        Protected Overrides Function y0() As var()
            Return {}
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
        ''' <param name="n%"></param>
        ''' <param name="a%"></param>
        ''' <param name="b%"></param>
        ''' <returns></returns>
        ''' <remarks>线程不安全的</remarks>
        Public Function RunTest(estimates As Dictionary(Of String, Double), n%, a%, b%) As ODEsOut
            Dim model As Type = Me.GetType()
            'Dim parms$() = ODEs.GetParameters(model).ToArray
            'Dim vars$() = ODEs.GetVariables(model).ToArray

            'For Each var$ In vars
            '    Me(var).value = estimates(var)
            'Next

            'For Each parm$ In parms
            '    Dim [set] As Action(Of Object, Double) =
            '        Delegates.FieldSet(Of Double)(model, parm)
            '    Call [set](Me, estimates(parm))
            'Next

            'Return Me.Solve(n, a, b, incept:=True)
            Return RunTest(model, estimates, estimates, n, a, b) ' y0也被包含在estimates之中，所以传递两个
        End Function

        ''' <summary>
        ''' 这个函数是为并行化而设计的，线程安全的
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
        ''' <param name="n%"></param>
        ''' <param name="a%"></param>
        ''' <param name="b%"></param>
        ''' <returns></returns>
        Public Shared Function RunTest(model As Type,
                                       y0 As Dictionary(Of String, Double),
                                       estimates As Dictionary(Of String, Double),
                                       n%, a%, b%,
                                       Optional ref As ODEsOut = Nothing) As ODEsOut

            Dim parms$() = ODEs.GetParameters(model).ToArray
            Dim vars$() = ODEs.GetVariables(model).ToArray
            Dim x As Model = TryCast(Activator.CreateInstance(model), Model)
            Dim var$ = Nothing
            Dim parm$ = Nothing

            Try
                For Each var$ In vars    ' 设置初始值
                    x(var).value = y0(var)
                Next
            Catch ex As Exception
                Dim msg$ = $"Model required a parameter which is named ``{var}``, but '{var}' is not exists in list: {y0.KeysJson}"
                ex = New Exception(msg, ex)
                Throw ex
            End Try

            Try
                For Each parm$ In parms  ' 设置参数值
                    Dim [set] As Action(Of Object, Double) =
                        Delegates.FieldSet(Of Double)(model, parm)
                    Call [set](x, estimates(parm))
                Next
            Catch ex As Exception
                Dim msg$ = $"Model required a parameter which is named ``{parm}``, but '{parm}' is not exists in list: {estimates.KeysJson}"
                ex = New Exception(msg, ex)
                Throw ex
            End Try

            If Not ref Is Nothing Then
                Dim partTokens As Integer = n / ref.x.Length

                TryCast(x, RefModel).RefValues = New ValueVector With {
                    .Y = ref.y,
                    .Value = Scan0
                }
                TryCast(x, RefModel).Delta = partTokens
            End If

            Return x.Solve(n, a, b, incept:=True)
        End Function

        ''' <summary>
        ''' 这个函数是为并行化而设计的，线程安全的
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="estimates">新的方程参数，这里面需要同时包括了参数和y初始值</param>
        ''' <param name="n%"></param>
        ''' <param name="a%"></param>
        ''' <param name="b%"></param>
        ''' <returns></returns>
        Public Shared Function RunTest(model As Type,
                                       y0 As Dictionary(Of String, Double),
                                       estimates As var(),
                                       n%, a%, b%,
                                       Optional ref As ODEsOut = Nothing) As ODEsOut

            Dim args As Dictionary(Of String, Double) =
                estimates _
                .ToDictionary(Function(x) x.Name,
                              Function(x) x.value)
            Return RunTest(model, y0, args, n, a, b, ref)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="estimates"></param>
        ''' <param name="n%"></param>
        ''' <param name="a%"></param>
        ''' <param name="b%"></param>
        ''' <param name="modify">修改部分数据</param>
        ''' <returns></returns>
        Public Function RunTest(estimates As Dictionary(Of String, Double)(),
                                n%, a%, b%,
                                Optional modify As Dictionary(Of String, Double) = Nothing) As ODEsOut

            Dim parms As New Dictionary(Of String, Double)

            For Each var$ In estimates(Scan0%).Keys
                Dim dist = estimates.Select(Function(x) x(var$)).Distributes
                Dim most As DoubleTagged(Of Integer) =
                    LinqAPI.DefaultFirst(Of DoubleTagged(Of Integer)) <= From x As DoubleTagged(Of Integer)
                                                                         In dist.Values
                                                                         Select x
                                                                         Order By x.Value Descending
                parms(var$) = most.Tag
            Next

            If Not modify.IsNullOrEmpty Then
                For Each var$ In modify.Keys
                    parms(var$) = modify(var$)
                Next
            End If

            Return RunTest(parms, n, a, b)
        End Function
    End Class
End Namespace
