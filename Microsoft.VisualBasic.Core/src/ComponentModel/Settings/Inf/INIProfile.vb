#Region "Microsoft.VisualBasic::c769dd28bad939004cff74eb554f3b86, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\Inf\INIProfile.vb"

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

    '   Total Lines: 171
    '    Code Lines: 95
    ' Comment Lines: 59
    '   Blank Lines: 17
    '     File Size: 7.51 KB


    '     Module INIProfile
    ' 
    '         Function: (+2 Overloads) GetPrivateProfileString, isCommentsOrBlank, PopulateSections, readDataLines
    ' 
    '         Sub: WritePrivateProfileString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports r = System.Text.RegularExpressions.Regex

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Wrapper class for *.ini and *.inf configure file.
    ''' (可能文件中的注释行会受到影响，所以请尽量使用本类型中的两个静态函数来操作INI文件)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <Package("Settings.Inf", Description:="Wrapper class for *.ini and *.inf configure file.", Url:="http://gcmodeller.org", Publisher:="xie.guigang@live.com")>
    Public Module INIProfile

        Const RegexoSectionHeader$ = "^\s*\[[^]]+\]\s*$"
        Const RegexpKeyValueItem$ = "^\s*[^=]+\s*=\s*.*$"

        ''' <summary>
        ''' 在读取的时候会将空白行给删除掉
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function readDataLines(path As String, removeComments As Boolean) As IEnumerable(Of String)
            Dim isBlank = Function(strLine As String) As Boolean
                              Return strLine.StringEmpty(True) OrElse (removeComments AndAlso strLine.isCommentsOrBlank)
                          End Function

            Return From line As String
                   In path.ReadAllLines
                   Let strLine As String = line.Trim
                   Where Not isBlank(strLine)
                   Select strLine
        End Function

        ''' <summary>
        ''' Get profile data from the ini file which the data is stores in a specific path like: ``section/key``
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="key"></param>
        ''' <param name="section">
        ''' 因为这个函数是使用正则表达式进行匹配的，所以section名称不可以有正则表达式之中的特殊符号
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPrivateProfileString(section$, key$, path$) As String
            Return path.readDataLines(True) _
                .ToArray _
                .GetPrivateProfileString(section, key)
        End Function

        ''' <summary>
        ''' 解析ini配置文件数据为通用数据模型
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Iterator Function PopulateSections(path As String) As IEnumerable(Of Section)
            Dim sectionName$ = Nothing
            Dim values As New List(Of [Property])
            Dim comments As String = ""
            Dim sectionComments As String = ""

            For Each line As String In path.readDataLines(removeComments:=False)
                If r.Match(line.Trim, RegexoSectionHeader).Success Then
                    ' 找到了新的section的起始
                    ' 则将前面的数据抛出
                    If Not sectionName.StringEmpty Then
                        Yield New Section With {
                            .Name = sectionName,
                            .Items = values,
                            .Comment = sectionComments
                        }
                    End If

                    values *= 0
                    sectionName = line.GetStackValue("[", "]")
                    sectionComments = ""
                ElseIf line.First = "#"c Then
                    sectionComments = sectionComments & vbCrLf & Mid(line, 2).Trim
                ElseIf line.First = ";" Then
                    comments = comments & vbCrLf & Mid(line, 2).Trim
                ElseIf r.Match(line, RegexpKeyValueItem, RegexICSng).Success Then
                    With line.Trim.GetTagValue("=", trim:=True)
                        values += New [Property](.Name, .Value, comments)
                        comments = ""
                    End With
                End If
            Next

            ' 抛出剩余的数据
            If Not sectionName.StringEmpty Then
                Yield New Section With {
                    .Name = sectionName,
                    .Items = values,
                    .Comment = sectionComments
                }
            End If
        End Function

        ''' <summary>
        ''' Get profile data from the ini file data lines which stores in a specific path like: ``section/key``
        ''' </summary>
        ''' <param name="lines$"></param>
        ''' <param name="section$"></param>
        ''' <param name="key$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 比较轻量化的文件解析
        ''' </remarks>
        <Extension>
        Public Function GetPrivateProfileString(lines$(), section$, key$) As String
            Dim sectionFind$ = String.Format("^\s*\[{0}\]\s*$", section)
            Dim keyFind$ = String.Format("^{0}\s*=\s*.*$", key)

            For index As Integer = 0 To lines.Length - 1
                If r.Match(lines(index), sectionFind, RegexICSng).Success Then

                    ' 找到了section的起始，则下面的数据到下一个section出现之前都是需要进行查找的数据
                    For i As Integer = index + 1 To lines.Length - 1
                        Dim line As String = lines(i)

                        If r.Match(line.Trim, keyFind, RegexICSng).Success Then
                            Return line.GetTagValue("=", trim:=True).Value
                        ElseIf r.Match(line.Trim, RegexoSectionHeader).Success Then
                            ' 已经匹配到了下一个section的起始了
                            ' 没有找到，则返回空值
                            Return ""
                        End If
                    Next
                End If
            Next

            Return ""
        End Function

        ''' <summary>
        ''' 判断当前的行是否是空白或者注释行
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function isCommentsOrBlank(str As String) As Boolean
            Return String.IsNullOrEmpty(str) OrElse (str.First = ";"c OrElse str.First = "#"c)
        End Function

        ''' <summary>
        ''' Setting profile data from the ini file which the data is stores in a specific path like: ``section/key``. 
        ''' If the path is not exists, the function will create new.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="Section"></param>
        ''' <param name="key"></param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' 这个函数会保留下在配置文件之中原来的注释信息
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WritePrivateProfileString(section$, key$, value$, path$)
            Using ini As New IniFile(path)
                Call ini.WriteValue(section, key, value)
            End Using
        End Sub
    End Module
End Namespace
