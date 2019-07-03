Namespace ApplicationServices.Zip

    ''' <summary>
    ''' Used to specify what our overwrite policy
    ''' is for files we are extracting.
    ''' </summary>
    Public Enum Overwrite
        Always
        IfNewer
        Never
    End Enum

    ''' <summary>
    ''' Used to identify what we will do if we are
    ''' trying to create a zip file and it already
    ''' exists.
    ''' </summary>
    Public Enum ArchiveAction
        Merge
        Replace
        [Error]
        Ignore
    End Enum
End Namespace