#Region "Microsoft.VisualBasic::9b3b2d800541fbdd2bd3529c8f1458d4, Data\BinaryData\BinaryData\SQLite3\Sqlite3Settings.vb"

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

    '     Class Sqlite3Settings
    ' 
    '         Properties: blobAsBase64
    ' 
    '         Function: GetDefaultSettings
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default

Namespace ManagedSqlite.Core

    Public Class Sqlite3Settings

        ''' <summary>
        ''' 在读取数据的时候,将blob数据块转换为base64字符串,默认不进行转换,保持原样输出
        ''' </summary>
        ''' <returns></returns>
        Public Property blobAsBase64 As Boolean = False

        Public Shared Function GetDefaultSettings() As [Default](Of Sqlite3Settings)
            Return New Sqlite3Settings
        End Function
    End Class
End Namespace
