#Region "Microsoft.VisualBasic::10099b769f7bc7dad43ec17bc4cfca96, www\githubAPI\WebAPI\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Extensions
    ' 
    '         Function: GetAvatar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace WebAPI

    Public Module Extensions

        ''' <summary>
        ''' Download user avatar image or read from local cache
        ''' </summary>
        ''' <param name="user"></param>
        ''' <param name="noCache"></param>
        ''' <returns></returns>
        <Extension> Public Function GetAvatar(user As User, Optional noCache As Boolean = False) As Image
            Dim local$ = App.SysTemp & $"/Github/avatars/{user.login}.png"

            If noCache Then
                Call user.avatar_url.DownloadFile(save:=local)
                Return local.LoadImage
            Else
                If local.FileLength > 0 Then
                    Return local.LoadImage
                Else
                    Call user.avatar_url.DownloadFile(save:=local)
                    Return local.LoadImage
                End If
            End If
        End Function
    End Module
End Namespace
