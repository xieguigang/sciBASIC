Imports System.Collections.Specialized
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Module QueryBuilder

    ''' <summary>
    ''' 假若有非法字符，则需要使用<see cref="Field"/>来标记出来
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Build(Of T As Structure)(args As T) As NameValueCollection

    End Function

    ''' <summary>
    ''' 当使用<see cref="Term"/>标记的时候，表明这个属性为必须的参数并且没有名称
    ''' </summary>
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Term : Inherits Attribute

        Public Const Key As String = "<" & NameOf(Term) & ">"
    End Class
End Module
