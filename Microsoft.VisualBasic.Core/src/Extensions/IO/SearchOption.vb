#If NETSTANDARD1_2_OR_GREATER Then

Namespace FileIO

    ''' <summary>
    ''' Specifies whether to search all or only top-level directories.
    ''' </summary>
    Public Enum SearchOption

        ''' <summary>
        ''' Search only the specified directory and exclude subdirectories.
        ''' </summary>
        SearchTopLevelOnly = 2
        ''' <summary>
        ''' Search the specified directory and all subdirectories within it. Default.
        ''' </summary>
        SearchAllSubDirectories = 3
    End Enum
End Namespace
#End If 