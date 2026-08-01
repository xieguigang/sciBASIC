Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace sln

    Public Class FolderWorkspace : Implements IProjectWorkspace

        ''' <summary>
        ''' the name of the folder
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            Get
                Return Path.BaseName
            End Get
        End Property

        ''' <summary>
        ''' the folder full path
        ''' </summary>
        ''' <returns></returns>
        Public Property Path As String

        Sub New()
        End Sub

        Sub New(dir As String)
            Path = dir
        End Sub

        Public Overrides Function ToString() As String
            Return Path
        End Function

        Public Function GetCompileFiles() As IEnumerable(Of String) Implements IProjectWorkspace.GetCompileFiles
            Return From file As String
                   In (ls - l - r - "*.*" <= Path)
                   Let rel As String = ProjectFiles.GetRelativePath(Path, file)
                   Where Not ProjectFiles.IsExcludedByDefault(rel)
                   Select rel
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateFs(ws As IProjectWorkspace) As FileSystemTree
            Return FileSystemTree.BuildTree(ws.GetCompileFiles)
        End Function
    End Class
End Namespace