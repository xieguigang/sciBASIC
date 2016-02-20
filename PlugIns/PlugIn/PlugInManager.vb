Imports System.Windows.Forms
Imports System.Text
Imports System.Drawing
Imports System.Xml.Serialization

Public Class PlugInManager

    ''' <summary>
    ''' The file path for the disabled plugin assembly module.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlElement> Public DisabledPlugIns As List(Of String)

    Dim XmlPath As String
    Protected Friend PlugInList As List(Of PlugInEntry) = New List(Of PlugInEntry)

    Public Shared Function LoadPlugins(Menu As MenuStrip, PluginDir As String, ProfileXml As String) As PlugInManager
        Dim PluginManager As PlugInManager = PlugIn.PlugInManager.Load(ProfileXml)
        Dim LoadFileList = (From Path As String In FileIO.FileSystem.GetFiles(PluginDir, FileIO.SearchOption.SearchTopLevelOnly, "*.dll", "*.exe")
                            Where PluginManager.DisabledPlugIns.IndexOf(Path) = -1
                            Select Path).ToArray 'Get the load plugin file list, ignore the plugin file which is in disabled plugin list.

        Call PluginManager.PlugInList.AddRange((From PlugInAssembly As String In LoadFileList Select PlugIn.PlugInEntry.LoadPlugIn(Menu, PlugInAssembly)).ToArray)
        Call PluginManager.PlugInList.AddRange((From PlugInAssembly As String In PluginManager.DisabledPlugIns Select PlugIn.PlugInLoader.LoadMainModule(PlugInAssembly)).ToArray)
        Call PluginManager.PlugInList.RemoveAll(Function(PlugInEntry As PlugIn.PlugInEntry) PlugInEntry Is Nothing)

        Return PluginManager
    End Function

    Public Function IsDisabled(AssemblyPath As String) As Boolean
        Return DisabledPlugIns.IndexOf(AssemblyPath) > -1
    End Function

    Public Sub ShowDialog(Optional ShowWarnDialog As Boolean = True)
        Call New PlugInManagerGUI() With {.PlugInManager = Me}.ShowDialog()
        Call Save()
        If ShowWarnDialog Then
            MsgBox("You should restart this program to makes the changes take effect.", MsgBoxStyle.Information)
        End If
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0}, {1} plugins is disabled by user.", XmlPath, DisabledPlugIns.Count)
    End Function

    Public Sub Save(Optional Path As String = "")
        Dim sBuilder As StringBuilder = New StringBuilder(1024)
        If String.IsNullOrEmpty(Path) Then
            Path = Me.XmlPath
        End If
        Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(Path))
        Using Stream As New System.IO.StringWriter(sb:=sBuilder)
            Call (New System.Xml.Serialization.XmlSerializer(GetType(PlugInManager))).Serialize(Stream, Me)
            Call FileIO.FileSystem.WriteAllText(Path, sBuilder.ToString, append:=False)
        End Using
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Xml">XML文件的文件路径</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Shared Function Load(Xml As String) As PlugIn.PlugInManager
        Dim PlugInManager As PlugIn.PlugInManager
        If FileIO.FileSystem.FileExists(Xml) Then
            Dim FileContent As String = FileIO.FileSystem.ReadAllText(file:=Xml)
            Using Stream As New System.IO.StringReader(s:=FileContent)
                PlugInManager = DirectCast(New System.Xml.Serialization.XmlSerializer(GetType(PlugInManager)).Deserialize(Stream), PlugInManager)
            End Using
        Else
            PlugInManager = New PlugInManager
        End If
        If PlugInManager.DisabledPlugIns Is Nothing Then PlugInManager.DisabledPlugIns = New List(Of String)
        PlugInManager.XmlPath = Xml
        Return PlugInManager
    End Function

    Public Sub LoadPlugIns(ListView As System.Windows.Forms.ListView, ImageList As ImageList)
        Dim Index As Integer
        ListView.LargeImageList = ImageList
        ListView.SmallImageList = ImageList

        For Each PlugIn As PlugIn.PlugInEntry In PlugInList
            Dim Icon = CType(PlugIn.IconImage, Bitmap)
            Dim Item As ListViewItem = New ListViewItem({PlugIn.Name, PlugIn.Description, PlugIn.AssemblyPath})
            Item.Checked = Not IsDisabled(PlugIn.AssemblyPath)

            Call ListView.Items.Add(Item)

            If Not Icon Is Nothing Then
                Call ImageList.Images.Add(Icon)
                Item.ImageIndex = Index
                Index += 1
            End If
        Next
    End Sub

    Public Shared Function GetDisabledPlugIns(ListView As ListView, PlugInManager As PlugInManager) As List(Of String)
        Dim LQuery = From item As ListViewItem In ListView.Items Where Not item.Checked Select PlugInManager.PlugInList(item.Index).AssemblyPath '
        Return LQuery.ToList
    End Function
End Class
