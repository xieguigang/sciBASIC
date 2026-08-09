Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln.File

Namespace sln

    Public Class SolutionWorkspace : Implements IProjectWorkspace

        Public ReadOnly Property Name As String Implements IProjectWorkspace.Name
            Get
                Return Sln.FilePath.BaseName
            End Get
        End Property

        Public ReadOnly Property Sln As Solution

        Sub New(sln As Solution)
            _Sln = sln
        End Sub

        Public Overrides Function ToString() As String
            Return Sln.FilePath.FileName
        End Function

        Public Iterator Function GetCompileFiles() As IEnumerable(Of String) Implements IProjectWorkspace.GetCompileFiles
            For Each proj As Project In Sln.Projects
                For Each file As String In DirectCast(proj, IProjectWorkspace).GetCompileFiles
                    Yield file
                Next
            Next
        End Function
    End Class
End Namespace