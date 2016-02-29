Imports System.Reflection

Namespace Scripting.MetaData

    ''' <summary>
    ''' 类型信息
    ''' </summary>
    Public Class TypeInfo

        ''' <summary>
        ''' 模块文件
        ''' </summary>
        ''' <returns></returns>
        Public Property assm As String
        ''' <summary>
        ''' 类型源
        ''' </summary>
        ''' <returns></returns>
        Public Property FullIdentity As String

        Public Overrides Function ToString() As String
            Return $"{assm}!{FullIdentity}"
        End Function

        Public Function LoadAssembly() As Assembly
            Dim path As String = App.HOME & "/" & Me.assm
            Dim assm As Assembly = Assembly.LoadFile(path)
            Return assm
        End Function

        ''' <summary>
        ''' Get mapping type information.
        ''' </summary>
        ''' <returns></returns>
        Public Overloads Function [GetType]() As Type
            Dim assm As Assembly = LoadAssembly()
            Dim type As Type = assm.GetType(Me.FullIdentity)
            Return type
        End Function
    End Class
End Namespace