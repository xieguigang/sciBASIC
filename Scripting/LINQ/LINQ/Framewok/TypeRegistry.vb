Namespace Framework

    ''' <summary>
    ''' Type registry table for loading the external LINQ entity assembly module. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TypeRegistry : Implements System.IDisposable

        ''' <summary>
        ''' item in the type registry table
        ''' </summary>
        ''' <remarks></remarks>
        Public Class RegistryItem

            ''' <summary>
            ''' 类型的简称或者别称，即本属性为LINQEntity自定义属性中的构造函数的参数
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Xml.Serialization.XmlAttribute> Public Property Name As String
            ''' <summary>
            ''' 建议使用相对路径，以防止移动程序的时候任然需要重新注册方可以使用
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Xml.Serialization.XmlAttribute> Public Property AssemblyPath As String
            ''' <summary>
            ''' Full type name for the target LINQ entity type.(目标LINQEntity集合中的类型全称)
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Xml.Serialization.XmlAttribute> Public Property TypeId As String

            Public ReadOnly Property AssemblyFullPath As String
                Get
                    Return IO.Path.GetFullPath(AssemblyPath)
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return String.Format("({0}) {1}!{2}", Name, AssemblyPath, TypeId)
            End Function

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="Obj">Name, TypeId, AssemblyPath, IsInnerType</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Shared Widening Operator CType(Obj As Object()) As RegistryItem
                Dim RegistryItem As RegistryItem = New RegistryItem
                RegistryItem.Name = Obj(0).ToString
                RegistryItem.TypeId = Obj(1).ToString
                RegistryItem.AssemblyPath = Obj(2).ToString

                Return RegistryItem
            End Operator
        End Class

        Public Property ExternalModules As List(Of RegistryItem)

        Dim File As String

        Public Overrides Function ToString() As String
            Return File
        End Function

        ''' <summary>
        ''' 返回包含有该类型的目标模块的文件路径
        ''' </summary>
        ''' <param name="Name">LINQ Entity集合中的元素的简称或者别称，即Item中的Name属性</param>
        ''' <returns>If the key is not exists in this object, than the function will return a empty string.</returns>
        ''' <remarks></remarks>
        Public Function FindAssemblyPath(Name As String) As String
            Dim Item = Find(Name)
            If Item Is Nothing Then
                Return ""
            Else
                Return Item.AssemblyFullPath
            End If
        End Function

        ''' <summary>
        ''' Return a registry item in the table using its specific name property.
        ''' (返回注册表中的一个指定名称的项目)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Find(Name As String) As TypeRegistry.RegistryItem
            For i As Integer = 0 To ExternalModules.Count - 1
                If String.Equals(Name, ExternalModules(i).Name, StringComparison.OrdinalIgnoreCase) Then
                    Return ExternalModules(i)
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Registry the external LINQ entity assembly module in the LINQFramework
        ''' </summary>
        ''' <param name="AssemblyPath">DLL file path</param>
        ''' <returns></returns>
        ''' <remarks>查询出目标元素的类型定义并获取信息</remarks>
        Public Function Register(AssemblyPath As String) As Boolean
            Dim Assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(IO.Path.GetFullPath(AssemblyPath)) 'Load external module
            Dim ILINQEntityTypes As System.Reflection.TypeInfo() =
                LINQ.Framework.LQueryFramework.LoadAssembly(Assembly, Reflection.LINQEntity.ILINQEntity) 'Get type define informations of LINQ entity

            If ILINQEntityTypes.Count > 0 Then
                Dim LQuery As Generic.IEnumerable(Of TypeRegistry.RegistryItem) =
                    From Type As System.Type In ILINQEntityTypes
                    Select New TypeRegistry.RegistryItem With {
                        .Name = Framework.Reflection.LINQEntity.GetEntityType(Type),
                        .AssemblyPath = AssemblyPath,
                        .TypeId = Type.FullName}        'Generate the resitry item for each external type

                For Each Item In LQuery.ToArray         'Update exists registry item or insrt new item into the table
                    Dim Item2 As RegistryItem = Find(Item.Name)         '在注册表中查询是否有已注册的类型
                    If Item2 Is Nothing Then
                        Call ExternalModules.Add(Item)  'Insert new record.(添加数据)
                    Else                                'Update exists data.(更新数据)
                        Item2.AssemblyPath = Item.AssemblyPath
                        Item2.TypeId = Item.TypeId
                    End If
                Next
                Return True
            Else                                        'I did't found any LINQ entity type define information, skip this dll assembly file
                Return False
            End If
        End Function

        Public Shared Function Load(Path As String) As TypeRegistry
            If FileIO.FileSystem.FileExists(Path) Then
                Dim TypeRegistry As TypeRegistry = Path.LoadXml(Of TypeRegistry)()
                TypeRegistry.File = Path
                Return TypeRegistry
            Else
                Return New TypeRegistry With {
                    .ExternalModules = New List(Of TypeRegistry.RegistryItem),
                    .File = Path}
            End If
        End Function

        Public Sub Save(Optional Path As String = "")
            If String.IsNullOrEmpty(Path) Then
                Path = Me.File
            End If
            Call Me.GetXml.SaveTo(Path, Text.Encoding.Unicode)
        End Sub

        Public Shared Widening Operator CType(Path As String) As TypeRegistry
            Return TypeRegistry.Load(Path)
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO:  释放托管状态(托管对象)。
                    Call Me.Save()
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose(ByVal disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(ByVal disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace