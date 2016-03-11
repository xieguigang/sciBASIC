Imports System.Text
Imports System.Reflection
Imports Microsoft.VisualBasic.Scripting

#If NET_40 = 0 Then

Namespace ComponentModel.Settings

    Public Class Settings(Of T As IProfile) : Inherits ConfigEngine
        Implements System.IDisposable

        Public ReadOnly Property SettingsData As T
            Get
                Return _SettingsData.As(Of T)
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(Data As T)
            Call MyBase.New(Data)
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="XmlFile">目标配置文件的Xml文件的文件名</param>
        ''' <returns>可以调用的配置项的数目，解析失败则返回0</returns>
        ''' <remarks></remarks>
        Public Shared Function LoadFile(XmlFile As String, Optional CreateSave As Action(Of T, String) = Nothing) As Settings(Of T)
            If Not XmlFile.FileExists Then
                Return __createSave(XmlFile, CreateSave)
            Else
                Dim File As T = XmlFile.LoadXml(Of T)(ThrowEx:=False)
                If File Is Nothing Then
                    Return __createSave(XmlFile, CreateSave)
                Else
                    Return Load(File.InvokeSet(NameOf(File.FilePath), XmlFile))
                End If
            End If
        End Function

        Private Shared Function __createSave(xml As String, createSave As Action(Of T, String)) As Settings(Of T)
            Dim FileObject As T = DirectCast(Activator.CreateInstance(Of T)(), T)
            FileObject.FilePath = xml
            If Not createSave Is Nothing Then
                Call createSave(FileObject, xml)
            End If
            Return Load(Data:=FileObject)
        End Function

        Public Overloads Shared Function Load(Data As T) As Settings(Of T)
            Return New ComponentModel.Settings.Settings(Of T)(Data)
        End Function

        'Private Shared Sub LoadMethods(Settings As Settings(Of T))
        '    Dim Type As System.Type = GetType(T)
        '    Dim ProfileItem As System.Type = GetType(ProfileItem)
        '    Dim LQuery = (From [Property] In Type.GetProperties
        '                  Let Attributes = [Property].GetCustomAttributes(attributeType:=ProfileItem, inherit:=False)
        '                  Where Attributes.Count > 0
        '                  Let Attr = DirectCast(Attributes(0), ProfileItem)
        '                  Select New KeyValuePair(Of ProfileItem, System.Reflection.PropertyInfo)(Attr, [Property])).ToArray

        '    Dim ItemCollection = (From Pair In LQuery Select Pair.Key).ToArray

        '    For i As Integer = 0 To ItemCollection.Count - 1
        '        Dim item = ItemCollection(i)
        '        item._PropertyInfo = LQuery(i).Value
        '        Call Settings.ProfileItemCollection.Add(GetName(item, item._PropertyInfo), item)
        '    Next
        'End Sub

        Public Overloads Shared Narrowing Operator CType(Settings As Settings.Settings(Of T)) As T
            Return Settings._SettingsData.As(Of T)
        End Operator
    End Class
End Namespace
#End If