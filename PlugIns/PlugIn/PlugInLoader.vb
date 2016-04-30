Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Reflection

Public NotInheritable Class PlugInLoader

    Public Menu As MenuStrip, AssemblyPath As String

    ''' <summary>
    ''' 加载插件命令
    ''' </summary>
    ''' <returns>返回成功加载的命令的数目</returns>
    ''' <remarks></remarks>
    Public Function Load() As PlugIn.PlugInEntry
        Dim PlugInEntry = LoadMainModule(AssemblyPath)  'Get the plugin entry module.(获取插件主模块)
        If PlugInEntry Is Nothing Then Return Nothing

        Dim Initialize As EntryFlag = PlugInEntry.GetEntry(EntryTypes.Initialize)
        Dim Target As System.Windows.Forms.Form = Menu.FindForm

        If Not PlugInEntry.ShowOnMenu Then 'When the showonmenu property of this plugin entry is false, then this plugin will not load on the form menu but a initialize method is required.
            If Not Initialize Is Nothing Then
                Call PlugIn.PlugInEntry.Invoke(New Object() {Target}, Initialize.Target)

                Return PlugInEntry
            Else
                Return Nothing 'This plugin assembly have no initialize method nor name property, i really don't know how to processing it, so i treat is as nothing.
            End If
        Else
            If Not Initialize Is Nothing Then Call PlugIn.PlugInEntry.Invoke(New Object() {Target}, Initialize.Target)
        End If

        Dim IconLoader As EntryFlag = PlugInEntry.GetEntry(EntryTypes.IconLoader)
        Dim MainModule = PlugInEntry.MainModule
        Dim PluginCommandType = GetType(PlugInCommand)
        Dim LQuery = From Method As MethodInfo In MainModule.GetMethods
                     Let attributes = Method.GetCustomAttributes(PluginCommandType, False)
                     Where attributes.Count = 1
                     Let command = DirectCast(attributes(0), PlugInCommand).Initialize(Method)
                     Select command Order By command.Path Descending  'Load the available plugin commands.(加载插件模块中可用的命令)

        Dim MenuEntry = New System.Windows.Forms.ToolStripMenuItem() With {.Text = PlugInEntry.Name}   '生成入口点，并加载于UI之上
        Call Menu.Items.Add(MenuEntry)

        If IconLoader Is Nothing Then
            Dim Instance = GetIconLoader(PlugInEntry.Assembly)
            Dim Method As MethodInfo = Instance.Last
            Dim Invoke As Func(Of String, Object) = Function(Name As String) Method.Invoke(Instance.First, New Object() {Name})
            IconLoader = New EntryFlag With {.GetIconInvoke = Invoke}
        End If

        MenuEntry.Image = IconLoader.GetIcon(PlugInEntry.Icon)
        PlugInEntry.IconImage = MenuEntry.Image

        For Each Command As PlugInCommand In LQuery.ToArray  '生成子菜单命令
            Dim Item As ToolStripMenuItem = AddCommand(MenuEntry, (From s As String In Command.Path.Split("\"c) Where Not String.IsNullOrEmpty(s) Select s).ToArray, Command.Name, p:=0)
            Item.Image = IconLoader.GetIcon(Command.Icon)
            AddHandler Item.Click, Sub() Command.Invoke(Target)      '关联命令
        Next

        Return PlugInEntry
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Assembly"></param>
    ''' <returns>{Resource Manager Instanc, GetObject MethodInfo}</returns>
    ''' <remarks></remarks>
    Private Shared Function GetIconLoader(Assembly As Assembly) As Object()
        Dim LQuery = From Type In Assembly.DefinedTypes Where String.Equals(Type.Name, "Resources") Select Type '
        LQuery = LQuery.ToArray
        If LQuery.Count > 0 Then
            Dim Resources = LQuery.First
            Dim ResourceManager As Object = (From Method In Resources.DeclaredMethods Where String.Equals("get_ResourceManager", Method.Name) Select Method).First.Invoke(Nothing, New Object() {})
            Dim ResourceManagerType As System.Type = ResourceManager.GetType
            Dim GetObject = (From Method In ResourceManagerType.GetMethods Where String.Equals("GetObject", Method.Name) Select Method).First
            Return New Object() {ResourceManager, GetObject}
        Else
            Return Nothing
        End If
    End Function

    Protected Friend Shared Function LoadMainModule(AssemblyPath As String) As PlugInEntry
        If Not FileIO.FileSystem.FileExists(AssemblyPath) Then
            Return Nothing
        End If

        Dim Assembly As Assembly = Assembly.LoadFile(FileIO.FileSystem.GetFileInfo(AssemblyPath).FullName)
        Dim EntryType As Type = GetType(PlugInEntry)
        Dim EntryFlag = GetType(EntryFlag)
        Dim FindModule = From [Module] As Type In Assembly.DefinedTypes
                         Let attributes As Object() = [Module].GetCustomAttributes(EntryType, False)
                         Where attributes.Count = 1
                         Select DirectCast(attributes(0), PlugInEntry).Initialize([Module]) '
        FindModule = FindModule.ToArray
        If FindModule.Count = 0 Then
            Return Nothing
        Else
            Dim MainModule = FindModule.First
            Dim EntryFlags = From Method As MethodInfo In MainModule.MainModule.GetMethods
                             Let attributes = Method.GetCustomAttributes(EntryFlag, False)
                             Where 1 = attributes.Count
                             Select DirectCast(attributes(0), EntryFlag).Initialize(Target:=Method) '
            MainModule.AssemblyPath = AssemblyPath
            MainModule.EntryList = EntryFlags.ToArray
            MainModule.Assembly = Assembly
            Return MainModule
        End If
    End Function

    ''' <summary>
    ''' Recursive function for create the menu item for each plugin command.(递归的添加菜单项)
    ''' </summary>
    ''' <param name="MenuRoot"></param>
    ''' <param name="Path"></param>
    ''' <param name="Name"></param>
    ''' <param name="p"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function AddCommand(MenuRoot As ToolStripMenuItem, Path As String(), Name As String, p As Integer) As ToolStripMenuItem
        Dim NewItem As System.Func(Of String, ToolStripMenuItem) =
            Function(sName As String) As ToolStripMenuItem
                Dim MenuItem = New ToolStripMenuItem() With {.Text = sName}
                Call MenuRoot.DropDownItems.Add(MenuItem)
                Return MenuItem
            End Function

        If p = Path.Count Then
            Return NewItem(Name)
        Else
            Dim LQuery = From menuItem As ToolStripMenuItem In MenuRoot.DropDownItems Where String.Equals(menuItem.Text, Path(p)) Select menuItem '
            Dim Items = LQuery.ToArray, Item As ToolStripMenuItem
            If Items.Count = 0 Then Item = NewItem(Path(p)) Else Item = Items.First
            Return AddCommand(Item, Path, Name, p + 1)
        End If
    End Function
End Class
