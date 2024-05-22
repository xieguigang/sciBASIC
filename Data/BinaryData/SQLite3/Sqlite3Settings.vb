#Region "Microsoft.VisualBasic::9b3b2d800541fbdd2bd3529c8f1458d4, Data\BinaryData\SQLite3\Sqlite3Settings.vb"

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

    '   Total Lines: 17
    '    Code Lines: 9 (52.94%)
    ' Comment Lines: 4 (23.53%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (23.53%)
    '     File Size: 553 B


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
