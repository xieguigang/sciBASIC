Imports System.Reflection
Imports System.Xml.Serialization

Namespace Scripting.MetaData

    ''' <summary>
    ''' 类型信息
    ''' </summary>
    Public Class TypeInfo

        ''' <summary>
        ''' 模块文件
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property assm As String
        ''' <summary>
        ''' <see cref="Type.FullName"/>.(类型源)
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property FullIdentity As String

        ''' <summary>
        ''' 是否是已知的类型？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SystemKnownType As Boolean
            Get
                Return Not Scripting.GetType(FullIdentity) Is Nothing
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(info As Type)
            Call __infoParser(info, assm, FullIdentity)
        End Sub

        Private Shared Sub __infoParser(info As Type, ByRef assm As String, ByRef id As String)
            assm = FileIO.FileSystem.GetFileInfo(info.Assembly.Location).Name
            id = info.FullName
        End Sub

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
        Public Overloads Function [GetType](Optional knownFirst As Boolean = False) As Type
            Dim type As Type

            If knownFirst Then
                type = Scripting.GetType(FullIdentity)
                If Not type Is Nothing Then
                    Return type
                End If
            End If

            Dim assm As Assembly = LoadAssembly()
            type = assm.GetType(Me.FullIdentity)
            Return type
        End Function

        ''' <summary>
        ''' 检查a是否是指向b的类型引用的
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =(a As TypeInfo, b As Type) As Boolean
            Dim assm As String = Nothing, type As String = Nothing
            Call __infoParser(b, assm, type)
            Return String.Equals(a.assm, assm, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(a.FullIdentity, type, StringComparison.Ordinal)
        End Operator

        Public Overloads Shared Operator <>(a As TypeInfo, b As Type) As Boolean
            Return Not a = b
        End Operator
    End Class
End Namespace