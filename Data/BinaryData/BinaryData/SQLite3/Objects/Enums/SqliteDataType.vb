#Region "Microsoft.VisualBasic::c4c5bc160e24609097fc952da45bf333, Data\BinaryData\BinaryData\SQLite3\Objects\Enums\SqliteDataType.vb"

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

    '     Enum SqliteDataType
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module DataTypeParser
    ' 
    '         Function: TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Objects.Enums

    Public Enum SqliteDataType As Byte
        Null = 0
        [Integer] = 1
        Float = 7
        Boolean0 = 8
        Boolean1 = 9
        Blob = 12
        Text = 13
    End Enum

    Module DataTypeParser

        Public Function TryParse(type As String) As SqliteDataType
            Select Case Strings.LCase(type)
                Case "integer"
                    Return SqliteDataType.Integer
                Case "float", "double"
                    Return SqliteDataType.Float
                Case "text", "blob_text"
                    Return SqliteDataType.Text
                Case "blob"
                    Return SqliteDataType.Blob
                Case Else
                    Throw New NotImplementedException(type)
            End Select
        End Function
    End Module
End Namespace
