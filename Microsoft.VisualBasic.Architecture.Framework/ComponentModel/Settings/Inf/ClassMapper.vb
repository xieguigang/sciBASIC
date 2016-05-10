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

        ''' <summary>
        ''' Read data from ini file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="ini"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ClassWriter(Of T As Class)(ini As IniFile) As T
            Dim maps As NamedValue(Of BindProperty()) = MapParser(Of T)()
            Dim obj As Object = Activator.CreateInstance(Of T)

            For Each map In maps.x
                Dim key As String = map.Column.Name
                Dim value As String = ini.ReadValue(maps.Name, key)
                Dim o As Object = Scripting.CTypeDynamic(value, map.Type)
                Call map.SetValue(obj, o)
            Next

            Return DirectCast(obj, T)
        End Function

        <Extension>
        Public Sub ClassDumper(Of T As Class)(x As T, ini As IniFile)
            Dim maps As NamedValue(Of BindProperty()) = MapParser(Of T)()

            For Each map In maps.x
                Dim key As String = map.Column.Name
                Dim value As String = Scripting.ToString(map.GetValue(x))
                Call ini.WriteValue(maps.Name, key, value)
            Next
        End Sub

        <Extension>
        Public Function LoadIni(Of T As Class)(path As String) As T
            Return New IniFile(path).ClassWriter(Of T)
        End Function

        <Extension>
        Public Function WriteClass(Of T As Class)(x As T, ini As String) As Boolean
            Try
                Call x.ClassDumper(New IniFile(ini))
            Catch ex As Exception
                ex = New Exception(ini, ex)
                ex = New Exception(GetType(T).FullName, ex)
                Call ex.PrintException
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function
    End Module
End Namespace