Imports System.ComponentModel

Namespace sln

    ''' <summary>
    ''' Known Visual Studio project type GUIDs.
    ''' </summary>
    Public Enum TypeId
        <Description("2150E333-8FDC-42A3-9474-1A3956D46DE8")> FolderGroup
        <Description("F184B08F-C81C-45F6-A57F-5ABD9991F28F")> VBProject
        <Description("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")> NjsProject
        <Description("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")> CSharpProject
        <Description("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942")> CppProject
        <Description("F135691A-BF7E-435D-8960-F99683D2D49C")> WebApplication
        <Description("E24C65DC-7377-472B-9ABA-BC803B73C61A")> WebSite
        <Description("3AC096D0-A1C2-E12C-1390-A8335801FDAB")> TestProject
        <Description("VC60C7D7-84AE-47E4-8DED-D1B4E0554DBB")> SolutionFolderAlt
        <Description("Unknown")> Unknown
    End Enum

    ''' <summary>
    ''' A project or solution folder inside the solution.
    ''' </summary>
    Public Class Project
        ''' <summary>
        ''' The project type, derived from the project type GUID.
        ''' </summary>
        Public Property NodeType As TypeId
        ''' <summary>
        ''' The project type GUID as it appears in the solution file.
        ''' </summary>
        Public Property TypeGuid As String
        ''' <summary>
        ''' The project GUID.
        ''' </summary>
        Public Property Guid As String
        ''' <summary>
        ''' The node display name.
        ''' </summary>
        Public Property Name As String
        ''' <summary>
        ''' Includes virtual solution folder and project file path.
        ''' </summary>
        Public Property TreePath As String
        ''' <summary>
        ''' Relative path of the project file (e.g. a ``.vbproj``) to the solution file.
        ''' Empty for solution folders.
        ''' </summary>
        Public Property RelativePath As String
        ''' <summary>
        ''' Resolved full path of the project file. Empty for solution folders.
        ''' </summary>
        Public Property FullPath As String
        ''' <summary>
        ''' The GUID of the parent solution folder, or empty for top-level nodes.
        ''' </summary>
        Public Property ParentGuid As String

        ''' <summary>
        ''' True when this node is a solution folder (no physical project file).
        ''' </summary>
        Public ReadOnly Property IsFolder As Boolean
            Get
                Return NodeType = TypeId.FolderGroup OrElse NodeType = TypeId.SolutionFolderAlt
            End Get
        End Property
    End Class
End Namespace