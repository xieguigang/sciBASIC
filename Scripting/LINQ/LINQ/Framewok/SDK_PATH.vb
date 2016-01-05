
''' <summary>
''' .NET Framework的Reference Assembly文件夹的位置
''' </summary>
''' <remarks></remarks>
Module SDK_PATH

    ''' <summary>
    ''' 从高版本到低版本排列，从x64到x86排列
    ''' </summary>
    ''' <remarks></remarks>
    Private ReadOnly PathList As String() =
        New String() _
        {
            "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.3",
            "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.1",
            "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5",
            "C:\Windows\Microsoft.NET\Framework\v4.0.30319",
            "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0",
            "C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Client",
            "C:\Windows\Microsoft.NET\Framework\v3.5",
            "C:\Windows\Microsoft.NET\Framework\v2.0.50727"
        }

    ''' <summary>
    ''' .NET Framework的Reference Assembly文件夹的位置
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property AvaliableSDK As String
        Get
            For Each Path As String In PathList
                If FileIO.FileSystem.DirectoryExists(Path) Then
                    Return Path
                End If
            Next

            Return ""
        End Get
    End Property
End Module
