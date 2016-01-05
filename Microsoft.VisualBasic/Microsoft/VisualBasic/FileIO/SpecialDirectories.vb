Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.IO
Imports System.Security.Permissions
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.FileIO
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class SpecialDirectories
        ' Methods
        Private Shared Function GetDirectoryPath(Directory As String, DirectoryNameResID As String) As String
            If (Directory = "") Then
                Dim placeHolders As String() = New String() {Utils.GetResourceString(DirectoryNameResID)}
                Throw ExceptionUtils.GetDirectoryNotFoundException("IO_SpecialDirectoryNotExist", placeHolders)
            End If
            Return FileSystem.NormalizePath(Directory)
        End Function


        ' Properties
        Public Shared ReadOnly Property AllUsersApplicationData As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Application.CommonAppDataPath, "IO_SpecialDirectory_AllUserAppData")
            End Get
        End Property

        Public Shared ReadOnly Property CurrentUserApplicationData As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Application.UserAppDataPath, "IO_SpecialDirectory_UserAppData")
            End Get
        End Property

        Public Shared ReadOnly Property Desktop As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.Desktop), "IO_SpecialDirectory_Desktop")
            End Get
        End Property

        Public Shared ReadOnly Property MyDocuments As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.Personal), "IO_SpecialDirectory_MyDocuments")
            End Get
        End Property

        Public Shared ReadOnly Property MyMusic As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.MyMusic), "IO_SpecialDirectory_MyMusic")
            End Get
        End Property

        Public Shared ReadOnly Property MyPictures As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.MyPictures), "IO_SpecialDirectory_MyPictures")
            End Get
        End Property

        Public Shared ReadOnly Property ProgramFiles As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.ProgramFiles), "IO_SpecialDirectory_ProgramFiles")
            End Get
        End Property

        Public Shared ReadOnly Property Programs As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Environment.GetFolderPath(SpecialFolder.Programs), "IO_SpecialDirectory_Programs")
            End Get
        End Property

        Public Shared ReadOnly Property Temp As String
            Get
                Return SpecialDirectories.GetDirectoryPath(Path.GetTempPath, "IO_SpecialDirectory_Temp")
            End Get
        End Property

    End Class
End Namespace

