#Region "Microsoft.VisualBasic::25318555f5ae4dcb0820f6f699d08b62, Data\MyersDiff\DiffUtils.vb"

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

    '   Total Lines: 63
    '    Code Lines: 30 (47.62%)
    ' Comment Lines: 26 (41.27%)
    '    - Xml Docs: 84.62%
    ' 
    '   Blank Lines: 7 (11.11%)
    '     File Size: 2.77 KB


    ' Class DiffUtils
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CompareFiles, CompareLines, DiffChars, DiffFiles, DiffLines
    ' 
    ' /********************************************************************************/

#End Region

' -----------------------------------------------------------------------
' 便捷工具类：提供静态方法快速调用
' -----------------------------------------------------------------------
''' <summary>
''' 提供静态便捷方法的差异比较工具类。
''' </summary>
Public NotInheritable Class DiffUtils

    Private Sub New()
        ' 禁止实例化
    End Sub

    ''' <summary>
    ''' 比较两个文本文件并返回统一差异格式字符串。
    ''' </summary>
    ''' <param name="oldFilePath">旧文件路径。</param>
    ''' <param name="newFilePath">新文件路径。</param>
    ''' <param name="contextLines">上下文行数（默认 3）。</param>
    ''' <returns>统一差异格式字符串。</returns>
    Public Shared Function DiffFiles(oldFilePath As String, newFilePath As String,
                                      Optional contextLines As Integer = 3) As String
        Dim differ As New MyersDiff()
        Dim result As DiffResult = differ.CompareFiles(oldFilePath, newFilePath)
        Return result.ToUnifiedDiff(oldFilePath, newFilePath, contextLines)
    End Function

    ''' <summary>
    ''' 比较两个字符串数组并返回统一差异格式字符串。
    ''' </summary>
    Public Shared Function DiffLines(oldLines As String(), newLines As String(),
                                      Optional oldLabel As String = "a",
                                      Optional newLabel As String = "b",
                                      Optional contextLines As Integer = 3) As String
        Dim differ As New MyersDiff()
        Dim result As DiffResult = differ.Compare(oldLines, newLines)
        Return result.ToUnifiedDiff(oldLabel, newLabel, contextLines)
    End Function

    ''' <summary>
    ''' 比较两个字符串（字符级）并返回差异结果。
    ''' </summary>
    Public Shared Function DiffChars(oldText As String, newText As String) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.CompareChars(oldText, newText)
    End Function

    ''' <summary>
    ''' 比较两个文本文件并返回完整的差异结果对象。
    ''' </summary>
    Public Shared Function CompareFiles(oldFilePath As String, newFilePath As String) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.CompareFiles(oldFilePath, newFilePath)
    End Function

    ''' <summary>
    ''' 比较两个字符串数组并返回完整的差异结果对象。
    ''' </summary>
    Public Shared Function CompareLines(oldLines As String(), newLines As String()) As DiffResult
        Dim differ As New MyersDiff()
        Return differ.Compare(oldLines, newLines)
    End Function

End Class
