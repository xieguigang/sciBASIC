#Region "Microsoft.VisualBasic::b2460ab5907996636f333af293e0c332, Microsoft.VisualBasic.Core\Language\Language\C\File.vb"

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

    '     Module File
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Language.C

    Public Module File

        ''' <summary>
        ''' Specifies the beginning of a stream.(文件开头)
        ''' </summary>
        Public Const SEEK_SET As SeekOrigin = SeekOrigin.Begin
        ''' <summary>
        ''' Specifies the current position within a stream.(当前位置)
        ''' </summary>
        Public Const SEEK_CUR As SeekOrigin = SeekOrigin.Current
        ''' <summary>
        ''' Specifies the end of a stream.(文件结束)
        ''' </summary>
        Public Const SEEK_END As SeekOrigin = SeekOrigin.End
    End Module
End Namespace
