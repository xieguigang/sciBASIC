Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ComponentModel.Settings.Inf

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ClassName : Inherits Attribute

        Public ReadOnly Property Name As String

        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' 使用这个属性来标记需要进行序列化的对象属性: <see cref="DataFrameColumnAttribute"/>
    ''' </summary>
    Public Module ClassMapper

        Public Function MapParser(Of T As Class)() As NamedValue(Of BindProperty())
            Return GetType(T).MapParser
        End Function

        <Extension>
        Public Function MapParser(type As Type) As NamedValue(Of BindProperty())

        End Function
    End Module
End Namespace