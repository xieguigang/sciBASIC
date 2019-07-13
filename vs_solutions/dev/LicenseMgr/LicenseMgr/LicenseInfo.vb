#Region "Microsoft.VisualBasic::ae74b9296db08d7df5499dbbecfb6cb2, vs_solutions\dev\LicenseMgr\LicenseMgr\LicenseInfo.vb"

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

    ' Module LicenseInfoExtensions
    ' 
    '     Properties: info
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development

Module LicenseInfoExtensions

    Dim __content As LicenseInfo

    Public Property info As LicenseInfo
        Get
            If __content Is Nothing Then
                __content = New LicenseInfo
            End If

            Return __content
        End Get
        Set(value As LicenseInfo)
            __content = value
        End Set
    End Property

End Module
