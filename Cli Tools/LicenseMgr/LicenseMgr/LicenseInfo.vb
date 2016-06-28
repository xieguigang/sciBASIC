Module LicenseInfo

    Dim __content As SoftwareToolkits.LicenseInfo

    Public Property info As SoftwareToolkits.LicenseInfo
        Get
            If __content Is Nothing Then
                __content = New SoftwareToolkits.LicenseInfo
            End If

            Return __content
        End Get
        Set(value As SoftwareToolkits.LicenseInfo)
            __content = value
        End Set
    End Property

End Module
