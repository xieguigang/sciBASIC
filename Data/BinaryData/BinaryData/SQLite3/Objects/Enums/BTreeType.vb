#Region "Microsoft.VisualBasic::37b6d5a7270f5537fb17e5fe853143b6, Data\BinaryData\BinaryData\SQLite3\Objects\Enums\BTreeType.vb"

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

    '     Enum BTreeType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Objects.Enums
    Friend Enum BTreeType As Byte
        InteriorIndexBtreePage = &H2
        InteriorTableBtreePage = &H5
        LeafIndexBtreePage = &HA
        LeafTableBtreePage = &HD
    End Enum
End Namespace
