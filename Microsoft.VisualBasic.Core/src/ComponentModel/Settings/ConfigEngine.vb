#Region "Microsoft.VisualBasic::a533c58b8a26ecb2e234987b65f9c5d9, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\ConfigEngine.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 316
    '    Code Lines: 203 (64.24%)
    ' Comment Lines: 68 (21.52%)
    '    - Xml Docs: 63.24%
    ' 
    '   Blank Lines: 45 (14.24%)
    '     File Size: 12.78 KB


    '     Class ConfigEngine
    ' 
    '         Properties: AllItems, FilePath, MimeType, ProfileItemNode, ProfileItemType
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) [Set], ExistsNode, GetName, GetSettings, GetSettingsNode
    '                   Load, (+2 Overloads) Prints, (+2 Overloads) Save, ToString, View
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 只包含有对数据映射目标对象的属性读写，并不包含有文件数据的读写操作
    ''' </summary>
    ''' 
    Public Class ConfigEngine : Implements ISaveHandle, IFileReference
        Implements IDisposable

        ''' <summary>
        ''' 所映射的数据源
        ''' </summary>
        Protected profilesData As IProfile
        ''' <summary>
        ''' 键名都是小写的
        ''' </summary>
        Protected profileItemCollection As IDictionary(Of String, BindMapping)

        ''' <summary>
        ''' List all of the available settings nodes in this profile data session.
        ''' (枚举出当前配置会话之中的所有可用的配置节点)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AllItems As BindMapping()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return profileItemCollection _
                    .Values _
                    .ToArray
            End Get
        End Property

        Public Property FilePath As String Implements IFileReference.FilePath
            Get
                Return profilesData.FilePath
            End Get
            Set(value As String)
                profilesData.FilePath = value
            End Set
        End Property

        Sub New(obj As IProfile)
            profilesData = obj
            profileItemCollection = ConfigEngine.Load(Of IProfile)(
                obj.GetType,
                obj:=profilesData
            ).ToDictionary(Function(x) x.Name,
                           Function(x) x.Value)
        End Sub

        Protected Sub New()
        End Sub

        Protected Friend Shared Function Load(Of EntityType)(type As Type, obj As EntityType) As NamedValue(Of BindMapping)()
            Dim LQuery As IEnumerable(Of BindMapping) =
 _
                From [Property] As PropertyInfo
                In type.GetProperties
                Let attributes = [Property].GetCustomAttributes(attributeType:=ProfileItemType, inherit:=False)
                Where attributes.Length > 0
                Let attr = DirectCast(attributes(0), ProfileItem)
                Select BindMapping.Initialize(attr, [Property], obj) '

            Dim out As List(Of NamedValue(Of BindMapping)) =
                LinqAPI.MakeList(Of NamedValue(Of BindMapping)) <=
 _
                    From ProfileItem As BindMapping
                    In LQuery
                    Let name As String = GetName(ProfileItem, ProfileItem.BindProperty)
                    Select New NamedValue(Of BindMapping) With {
                        .Name = name,
                        .Value = ProfileItem
                    }

            Dim Nodes = From [property] As PropertyInfo
                        In type.GetProperties
                        Let attributes = [property].GetCustomAttributes(attributeType:=ProfileItemNode, inherit:=False)
                        Where attributes.Length = 1
                        Select New With {
                            .[Property] = [property],
                            .Entity = [property].GetValue(obj, Nothing)
                        } ' 在这里是用匿名类型而不是直接使用Linq的匿名类型的原因是在后面还需要进行赋值操作，而Linq的匿名类型的属性是ReadOnly的

            Dim innerNodes = Nodes.ToArray

            If innerNodes.Length > 0 Then
                For Each x In innerNodes

                    If x.Entity Is Nothing Then
                        Try
                            x.Entity = Activator.CreateInstance(type:=x.[Property].PropertyType)
                        Catch ex As Exception
                            Dim view As String =
                                $"{x.Property.Name} As {x.Property.PropertyType.FullName}"
                            ex = New Exception(view, ex)

                            Throw ex
                        Finally
                            Call x.Property.SetValue(obj, x.Entity, Nothing)
                        End Try
                    End If

                    out += Load(x.[Property].PropertyType, x.Entity)
                Next

                Return out.ToArray
            End If

            Return out.ToArray
        End Function

        Protected Shared Function GetName(ProfileItem As ProfileItem, [Property] As PropertyInfo) As String
            If String.IsNullOrEmpty(ProfileItem.Name) Then
                ProfileItem.Name = [Property].Name.ToLower
            End If
            Return ProfileItem.Name.ToLower
        End Function

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("Node.Exists")>
        Public Overridable Function ExistsNode(Name As String) As Boolean
            Return profileItemCollection.ContainsKey(Name.ToLower)
        End Function

        ''' <summary>
        ''' 请注意，<paramref name="name"/>必须是小写的
        ''' </summary>
        ''' <param name="Name">The name of the configuration entry should be in lower case.</param>
        ''' <param name="Value"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("SetValue")>
        Public Overridable Function [Set](Name As String, Value As String) As Boolean
            Dim keyFind As String = Name.ToLower

            If profileItemCollection.ContainsKey(keyFind) Then
                Call profileItemCollection(keyFind).Set(Value)
            Else
                Return False
            End If

            Return True
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Set](var As NamedValue(Of String)) As Boolean
            Return [Set](var.Name, var.Value)
        End Function

        <ExportAPI("GetValue")>
        Public Overridable Function GetSettings(Name As String) As String
            Dim keyFind As String = Name.ToLower

            If profileItemCollection.ContainsKey(keyFind) Then
                Dim item = profileItemCollection(keyFind)
                Dim result = item.Value
                Return result
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        '''假若函数参数<paramref name="name"/>为空，则函数输出所有的变量的值，请注意，这个函数并不在终端上面显示任何消息
        ''' </summary>
        ''' <param name="name">假若本参数为空，则函数输出所有的变量的值，大小写不敏感的</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("View")>
        Public Overridable Function View(Optional name As String = "") As String
            If String.IsNullOrEmpty(name) Then
                Return Prints(Me.profileItemCollection.Values)
            Else
                Return GetSettingsNode(name).AsOutString
            End If
        End Function

        <ExportAPI("Prints")>
        Public Shared Function Prints(data As IEnumerable(Of BindMapping)) As String
            Dim source As NamedValue(Of String)() =
                LinqAPI.Exec(Of NamedValue(Of String)) <=
 _
                From x As BindMapping
                In data
                Let value As String =
                    If(String.IsNullOrEmpty(x.Value), "null", x.Value)
                Select New NamedValue(Of String) With {
                    .Name = x.Name,
                    .Value = value,
                    .Description = x.Description
                }

            Return Prints(source)
        End Function

        <ExportAPI("Prints")>
        Public Shared Function Prints(data As IEnumerable(Of NamedValue(Of String))) As String
            Dim keys As String() = data.Select(Function(x) x.Name).ToArray
            Dim maxLen As Integer = keys.Select(AddressOf Len).Max
            Dim sb As New StringBuilder(New String("-"c, 120))

            Call sb.AppendLine()

            For Each line As NamedValue(Of String) In data
                Dim blank As String = New String(" "c, maxLen - Len(line.Name) + 2)
                Dim str As String = String.Format("  {0}{1}  = {2}", line.Name, blank, line.Value)

                If Not String.IsNullOrEmpty(line.Description) Then
                    str &= "     // " & line.Description
                End If

                Call sb.AppendLine(str)
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 大小写不敏感的
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <ExportAPI("GetNode")>
        Public Function GetSettingsNode(Name As String) As BindMapping
            Return profileItemCollection(Name.ToLower)
        End Function

        Public Overrides Function ToString() As String
            Return profilesData.FilePath
        End Function

        ''' <summary>
        ''' save the settings data in xml file format
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding"></param>
        ''' <returns></returns>
        <ExportAPI("Save")>
        Public Function Save(FilePath$, Encoding As Encoding) As Boolean Implements ISaveHandle.Save
            ' 20221022
            '
            ' due to the reason of profilesData object is 
            ' an interface, then getxml on this interface 
            ' will throw exceptions, change the getxml extension
            ' to the object general function to fix 
            ' this error
            Dim xml As String = XmlExtensions.GetXml(profilesData, profilesData.GetType(), throwEx:=False)
            Return Xml.SaveTo(FilePath Or Me.FilePath.When(FilePath.StringEmpty), Encoding)
        End Function

        Protected Friend Shared ReadOnly Property ProfileItemType As Type = GetType(ProfileItem)
        Protected Friend Shared ReadOnly Property ProfileItemNode As Type = GetType(ProfileNodeItem)

        Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {MIME.UnknownType}
            End Get
        End Property

        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call profilesData.Save()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
