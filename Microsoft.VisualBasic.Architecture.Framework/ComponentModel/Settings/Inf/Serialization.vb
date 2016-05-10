Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization
Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Settings.Inf

    Public Class Serialization : Inherits ITextFile

        <XmlElement> Public Property Sections As Section()

        Public Shared Function Load(path As String) As Serialization
            Throw New NotImplementedException
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException
        End Function

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class IniMapIO : Inherits Attribute

        Public ReadOnly Property Path As String

        Sub New(path As String)
            path = PathMapper.GetMapPath(path)
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Module IOProvider

        <Extension>
        Public Function WriteProfile(Of T As Class)(x As T, path As String) As Boolean
            Dim ini As New IniFile(path)

            For Each section As PropertyInfo In __getSections(Of T)()
                Dim obj As Object = section.GetValue(x)
                Call ClassMapper.ClassDumper(obj, section.PropertyType, ini)
            Next

            Return True
        End Function

        Public Function WriteProfile(Of T As Class)(x As T) As Boolean
            Return x.WriteProfile(__getPath(Of T))
        End Function

        Private Function __getSections(Of T As Class)() As PropertyInfo()
            Dim properties As PropertyInfo() =
                GetType(T).GetProperties(bindingAttr:=
                BindingFlags.Instance Or
                BindingFlags.Public)
            properties =
                LinqAPI.Exec(Of PropertyInfo) <= From p As PropertyInfo
                                                 In properties
                                                 Let type As Type = p.PropertyType
                                                 Let attr As ClassName = p.GetAttribute(Of ClassName)
                                                 Where Not attr Is Nothing
                                                 Select p
            Return properties
        End Function

        <Extension>
        Public Function LoadProfile(Of T As Class)(path As String) As T
            Dim obj As Object = Activator.CreateInstance(Of T)
            Dim ini As New IniFile(path)

            For Each prop As PropertyInfo In __getSections(Of T)()
                Dim x As Object = ClassMapper.ClassWriter(ini, prop.PropertyType)
                Call prop.SetValue(obj, x)
            Next

            Return DirectCast(obj, T)
        End Function

        Private Function __getPath(Of T As Class)() As String
            Dim path As IniMapIO = GetType(T).GetAttribute(Of IniMapIO)
            If path Is Nothing Then
                Throw New Exception("Could not found path mapping! @" & GetType(T).FullName)
            Else
                Return path.Path
            End If
        End Function

        Public Function LoadProfile(Of T As Class)() As T
            Return __getPath(Of T).LoadProfile(Of T)
        End Function
    End Module
End Namespace