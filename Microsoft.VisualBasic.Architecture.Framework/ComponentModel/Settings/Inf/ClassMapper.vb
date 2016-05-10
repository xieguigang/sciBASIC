Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

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
            Dim nameCLS As ClassName = type.GetAttribute(Of ClassName)
            Dim name As String

            If nameCLS Is Nothing Then
                name = type.Name
            Else
                name = nameCLS.Name
            End If

            Dim source = DataFrameColumnAttribute.LoadMapping(type)
            Dim binds As BindProperty() = source.ToArray(AddressOf BindProperty.FromHash)

            Return New NamedValue(Of BindProperty()) With {
                .Name = name,
                .x = binds
            }
        End Function
    End Module
End Namespace