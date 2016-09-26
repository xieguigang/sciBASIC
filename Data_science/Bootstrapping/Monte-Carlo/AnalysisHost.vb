Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq

Namespace MonteCarlo

    Public Module AnalysisHost

        ''' <summary>
        ''' 加载dll文件之中的计算模型
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <returns></returns>
        Public Function DllParser(dll As String) As Type
            Dim assem As Assembly = Assembly.LoadFile(dll.GetFullPath)
            Dim types As Type() = assem.GetTypes

            Return LinqAPI.DefaultFirst(Of Type) <= From type As Type
                                                    In types
                                                    Where type.IsInheritsFrom(GetType(Model)) AndAlso
                                                    Not type.IsAbstract
                                                    Select type
        End Function

        <Extension>
        Public Function Gety0(def As Type) As NamedValue(Of PreciseRandom)()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.yinit
        End Function

        <Extension>
        Public Function GetRandomParameters(def As Type) As NamedValue(Of PreciseRandom)()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.params
        End Function

        <Extension>
        Public Function GetEigenvector(def As Type) As Dictionary(Of String, Eigenvector)
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.eigenvector
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

        <Extension>
        Public Function Sampling(data As IEnumerable(Of ODEsOut), eigenvector As Dictionary(Of String, Eigenvector), Optional partN As Integer = 20) As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double)))
            Return data.Select(Function(x) x.Sampling(eigenvector, partN))
        End Function

        <Extension>
        Public Function Sampling(x As ODEsOut, eigenvector As Dictionary(Of String, Eigenvector), Optional partN As Integer = 20) As VectorTagged(Of Dictionary(Of String, Double))
            Dim vector As New List(Of Double)

            For Each var As String In eigenvector.Keys
                Dim y As Double() = x.y(var).x
                Dim n As Integer = y.Length / partN

                For Each block As Double() In Parallel.Linq.SplitIterator(y, n)
                    vector += eigenvector(var)(block)
                Next
            Next

            Return New VectorTagged(Of Dictionary(Of String, Double)) With {
                .Tag = vector.ToArray,   ' 所提取采样出来的特征向量
                .value = x.params  ' 生成原始数据的参数列表
            }
        End Function
    End Module
End Namespace