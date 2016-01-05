Imports Microsoft.VisualBasic.FileIO
Imports System
Imports System.ComponentModel
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.MyServices
    <EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class SpecialDirectoriesProxy
        ' Methods
        Friend Sub New()
        End Sub


        ' Properties
        Public ReadOnly Property AllUsersApplicationData As String
            Get
                Return SpecialDirectories.AllUsersApplicationData
            End Get
        End Property

        Public ReadOnly Property CurrentUserApplicationData As String
            Get
                Return SpecialDirectories.CurrentUserApplicationData
            End Get
        End Property

        Public ReadOnly Property Desktop As String
            Get
                Return SpecialDirectories.Desktop
            End Get
        End Property

        Public ReadOnly Property MyDocuments As String
            Get
                Return SpecialDirectories.MyDocuments
            End Get
        End Property

        Public ReadOnly Property MyMusic As String
            Get
                Return SpecialDirectories.MyMusic
            End Get
        End Property

        Public ReadOnly Property MyPictures As String
            Get
                Return SpecialDirectories.MyPictures
            End Get
        End Property

        Public ReadOnly Property ProgramFiles As String
            Get
                Return SpecialDirectories.ProgramFiles
            End Get
        End Property

        Public ReadOnly Property Programs As String
            Get
                Return SpecialDirectories.Programs
            End Get
        End Property

        Public ReadOnly Property Temp As String
            Get
                Return SpecialDirectories.Temp
            End Get
        End Property

    End Class
End Namespace

