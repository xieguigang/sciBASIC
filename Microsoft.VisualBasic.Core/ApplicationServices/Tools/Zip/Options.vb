#Region "Microsoft.VisualBasic::ebdf46fff322a8fbaf22989b286305b2, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Zip\Options.vb"

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

    '     Enum Overwrite
    ' 
    '         Always, IfNewer, Never
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum ArchiveAction
    ' 
    '         [Error], Ignore, Merge, Replace
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
