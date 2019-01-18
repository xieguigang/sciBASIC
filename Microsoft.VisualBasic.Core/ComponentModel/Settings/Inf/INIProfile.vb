#Region "Microsoft.VisualBasic::80dfb7f3011eb71f60fd0eb678caaa29, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\INIProfile.vb"

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

    '     Module INIProfile
    ' 
    '         Function: (+2 Overloads) GetPrivateProfileString, isCommentsOrBlank, PopulateSections, readDataLines, WritePrivateProfileString
    '                   WriteProfileComments
    ' 
    '         Sub: WritePrivateProfileString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
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
        ''' 在读取的时候会将注释行以及空白行给删除掉
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function readDataLines(path As String) As IEnumerable(Of String)
            Return From line As String
                   In path.ReadAllLines
                   Let strLine As String = line.Trim
                   Where Not strLine.isCommentsOrBlank
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
        <ExportAPI("GetValue", Info:="Get profile data from the ini file which the data is stores in a specific path like: ``section/key``")>
        Public Function GetPrivateProfileString(section$, key$, path$) As String
            Return path.readDataLines _
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
            Dim values As New List(Of NamedValue)

            For Each line As String In path.readDataLines
                If r.Match(line.Trim, RegexoSectionHeader).Success Then
                    ' 找到了新的section的起始
                    ' 则将前面的数据抛出
                    If Not sectionName.StringEmpty Then
                        Yield New Section With {
                            .Name = sectionName,
                            .Items = values
                        }
                    End If

                    values *= 0
                    sectionName = line.GetStackValue("[", "]")
                ElseIf r.Match(line, RegexpKeyValueItem, RegexICSng).Success Then
                    With line.Trim.GetTagValue("=", trim:=True)
                        values += New NamedValue(.Name, .Value)
                    End With
                End If
            Next

            ' 抛出剩余的数据
            If Not sectionName.StringEmpty Then
                Yield New Section With {
                    .Name = sectionName,
                    .Items = values
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
        ''' 
        ''' </summary>
        ''' <param name="lines"></param>
        ''' <param name="section$"></param>
        ''' <param name="key$"></param>
        ''' <param name="comments">不需要添加注释符号,函数会自动添加</param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteProfileComments(lines As List(Of String), section$, key$, comments$) As String()
            Dim sectionFind As String = $"^\s*\[{section}\]\s*$"
            ' 当存在该Section的时候，则从该Index位置处开始进行key的搜索
            Dim keyFind As String = $"^\s*{key}\s*=\s*.*$"
            Dim appendSection As Boolean = True
            Dim lastLine%

            comments = ";" & comments

            For index As Integer = 0 To lines.Count - 1
                If Not r.Match(lines(index), sectionFind, RegexICSng).Success Then
                    Continue For
                End If

                ' 找到了section的起始，则下面的数据到下一个section出现之前都是需要进行查找的数据
                For i As Integer = index + 1 To lines.Count - 1
                    Dim line As String = lines(i)

                    If r.Match(line.Trim, keyFind, RegexICSng).Success Then
                        ' 找到了
                        ' 在上一行插入注释文本，然后退出循环
                        lines.Insert(i - 1, comments)
                        Exit For
                    ElseIf r.Match(line.Trim, RegexoSectionHeader).Success Then
                        ' 已经匹配到了下一个section的起始了
                        ' 没有找到，则进行新建
                        ' 然后退出循环
                        lines.Insert(i - 1, comments)
                        lines.Insert(i, $"{key}=")
                        Exit For
                    End If

                    lastLine = i
                Next

                ' 如果成功了,则会提前退出,则这段代码不会被执行
                ' 所以在这里可以直接使用
                lines.Insert(lastLine + 1, comments)
                lines.Insert(lastLine + 2, $"{key}=")
                appendSection = False

                Exit For
            Next

            If appendSection Then
                ' 没有找到section，则需要追加新的数据
                lines += $"[{section}]"
                lines += comments
                lines += $"{key}="
            End If

            Return lines
        End Function

        <Extension>
        Public Function WritePrivateProfileString(lines As List(Of String), section$, key$, value$) As String()
            Dim sectionFind As String = $"^\s*\[{section}\]\s*$"
            ' 当存在该Section的时候，则从该Index位置处开始进行key的搜索
            Dim keyFind As String = $"^\s*{key}\s*=\s*.*$"
            Dim appendSection As Boolean = True
            Dim lastLine%

            For index As Integer = 0 To lines.Count - 1
                If Not r.Match(lines(index), sectionFind, RegexICSng).Success Then
                    Continue For
                End If

                ' 找到了section的起始，则下面的数据到下一个section出现之前都是需要进行查找的数据
                For i As Integer = index + 1 To lines.Count - 1
                    Dim line As String = lines(i)

                    If r.Match(line.Trim, keyFind, RegexICSng).Success Then
                        ' 找到了
                        ' 在这里进行值替换，然后退出循环
                        lines(i) = $"{key}={value}"
                        Return lines
                    ElseIf r.Match(line.Trim, RegexoSectionHeader).Success Then
                        ' 已经匹配到了下一个section的起始了
                        ' 没有找到，则进行新建
                        ' 然后退出循环
                        lines.Insert(i - 1, $"{key}={value}")
                        Return lines
                    End If

                    lastLine = i
                Next

                ' 如果成功了,则会提前退出,则这段代码不会被执行
                ' 所以在这里可以直接使用
                lines.Insert(lastLine + 1, $"{key}={value}")
                appendSection = False
                Exit For
            Next

            If appendSection Then
                ' 没有找到section，则需要追加新的数据
                lines += $"[{section}]"
                lines += $"{key}={value}"
            End If

            Return lines
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
        <ExportAPI("SetValue", Info:="Setting profile data from the ini file which the data is stores in a specific path like: ``section/key``. If the path is not exists, the function will create new.")>
        Public Sub WritePrivateProfileString(section$, key$, value$, path$)
            Call path.ReadAllLines _
                .AsList _
                .WritePrivateProfileString(section, key, value) _
                .SaveTo(path)
        End Sub
    End Module
End Namespace
