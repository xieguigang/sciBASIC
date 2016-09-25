Imports System.Reflection
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
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
    End Module
End Namespace