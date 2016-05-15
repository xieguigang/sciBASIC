Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 只包含有对数据映射目标对象的属性读写，并不包含有文件数据的读写操作
    ''' </summary>
    ''' 
    Public Class ConfigEngine : Inherits ITextFile
        Implements IDisposable

        ''' <summary>
        ''' 所映射的数据源
        ''' </summary>
        Protected _SettingsData As IProfile
        ''' <summary>
        ''' 键名都是小写的
        ''' </summary>
        Protected ProfileItemCollection As IDictionary(Of String, BindMapping)

        ''' <summary>
        ''' List all of the available settings nodes in this profile data session.
        ''' (枚举出当前配置会话之中的所有可用的配置节点)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AllItems As BindMapping()
            Get
                Return ProfileItemCollection.Values.ToArray
            End Get
        End Property

        Protected Friend Overrides Property FilePath As String
            Get
                Return _SettingsData.FilePath
            End Get
            Set(value As String)
                _SettingsData.FilePath = value
            End Set
        End Property

        Sub New(obj As IProfile)
            _SettingsData = obj
            ProfileItemCollection = ConfigEngine.Load(Of IProfile)(obj.GetType, TargetData:=_SettingsData).ToDictionary
        End Sub

        Protected Sub New()
        End Sub

        Protected Friend Shared Function Load(Of EntityType)(Type As System.Type, TargetData As EntityType) As KeyValuePair(Of String, BindMapping)()
            Dim LQuery = From [Property] As PropertyInfo In Type.GetProperties
                         Let attributes = [Property].GetCustomAttributes(attributeType:=ProfileItemType, inherit:=False)
                         Where attributes.Length > 0
                         Let attr = DirectCast(attributes(0), ProfileItem)
                         Select BindMapping.Initialize(attr, [Property], TargetData) '
            Dim LoadLQuery = (From ProfileItem As BindMapping In LQuery
                              Select New KeyValuePair(Of String, BindMapping)(GetName(ProfileItem, ProfileItem.BindProperty), ProfileItem)).ToList

            Dim Nodes = From [property] As PropertyInfo In Type.GetProperties
                        Let attributes = [property].GetCustomAttributes(attributeType:=ProfileItemNode, inherit:=False)
                        Where attributes.Length = 1
                        Select New With {.[Property] = [property], .Entity = [property].GetValue(TargetData, Nothing)} '
            Dim lstNodes = Nodes.ToArray

            If lstNodes.Length > 0 Then
                Dim List As List(Of KeyValuePair(Of String, BindMapping)) = LoadLQuery

                For Each Item In lstNodes
                    If Item.Entity Is Nothing Then
                        Try
                            Item.Entity = Activator.CreateInstance(type:=Item.[Property].PropertyType)
                        Catch ex As Exception
                            ex = New Exception($"{Item.Property.Name} As {Item.Property.PropertyType.FullName}", ex)
                            Throw ex
                        Finally
                            Call Item.Property.SetValue(TargetData, Item.Entity, Nothing)
                        End Try
                    End If
                    Call List.AddRange(Load(Item.[Property].PropertyType, Item.Entity))
                Next

                Return List.ToArray
            End If

            Return LoadLQuery.ToArray
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
        ''' 
        <ExportAPI("Node.Exists")>
        Public Overridable Function ExistsNode(Name As String) As Boolean
            Return ProfileItemCollection.ContainsKey(Name.ToLower)
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

            If ProfileItemCollection.ContainsKey(keyFind) Then
                Call ProfileItemCollection(keyFind).Set(Value)
            Else
                Return False
            End If
            Return True
        End Function

        <ExportAPI("GetValue")>
        Public Overridable Function GetSettings(Name As String) As String
            Dim keyFind As String = Name.ToLower

            If ProfileItemCollection.ContainsKey(keyFind) Then
                Dim item = ProfileItemCollection(keyFind)
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
                Return Prints(Me.ProfileItemCollection.Values)
            Else
                Dim data = GetSettingsNode(name)
                Return data.AsOutString
            End If
        End Function

        <ExportAPI("Prints")>
        Public Shared Function Prints(data As IEnumerable(Of BindMapping)) As String
            Dim Keys As String() = (From nodeItem As BindMapping In data Select nodeItem.Name).ToArray
            Dim MaxLength As Integer = (From str As String In Keys Select Len(str)).Max
            Dim sBuilder As StringBuilder = New StringBuilder(New String("-"c, 120))

            Call sBuilder.AppendLine()

            For Each line As BindMapping In data
                Dim blank As String = New String(" "c, MaxLength - Len(line.Name) + 2)
                Dim value As String = If(String.IsNullOrEmpty(line.Value), "null", line.Value)
                Dim str As String = String.Format("  {0}{1}  = {2}", line.Name, blank, value)
                If Not String.IsNullOrEmpty(line.Description) Then
                    str &= "     // " & line.Description
                End If
                Call sBuilder.AppendLine(str)
            Next

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' 大小写不敏感的
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("GetNode")>
        Public Function GetSettingsNode(Name As String) As BindMapping
            Return ProfileItemCollection(Name.ToLower)
        End Function

        Public Overrides Function ToString() As String
            Return _SettingsData.FilePath
        End Function

        <ExportAPI("Save")>
        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Dim Xml As String = _SettingsData.GetXml
            Return Xml.SaveTo(getPath(FilePath), Encoding)
        End Function

        Protected Friend Shared ReadOnly Property ProfileItemType As Type = GetType(ProfileItem)
        Protected Friend Shared ReadOnly Property ProfileItemNode As Type = GetType(ProfileNodeItem)

#Region "IDisposable Support"
        ' IDisposable
        Protected Overrides Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。
                    Call _SettingsData.Save()
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub
#End Region

        Protected Overrides Function __getDefaultPath() As String
            Return FilePath
        End Function
    End Class
End Namespace