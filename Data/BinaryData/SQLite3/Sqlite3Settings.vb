#Region "Microsoft.VisualBasic::e315f4e23c994b3bd6cced52a0ac8f72, Data\BinaryData\SQLite3\Sqlite3Settings.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 14
    '    Code Lines: 7 (50.00%)
    ' Comment Lines: 4 (28.57%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (21.43%)
    '     File Size: 466 B


    ' Class Sqlite3Settings
    ' 
    '     Properties: blobAsBase64
    ' 
    '     Function: GetDefaultSettings
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default

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
