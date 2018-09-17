#Region "Microsoft.VisualBasic::292b1f3f0b6cec9885c42234fcc64c4f, Microsoft.VisualBasic.Core\ComponentModel\Settings\Inf\INIProfile.vb"

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
    '         Function: __isCommentsOrBlank, GetValue
    ' 
    '         Sub: SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' Wrapper class for *.ini and *.inf configure file.(可能文件中的注释行会受到影响，所以请尽量使用本类型中的两个静态函数来操作INI文件)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <Package("Settings.Inf",
                      Description:="Wrapper class for *.ini and *.inf configure file.", Url:="http://gcmodeller.org", Publisher:="xie.guigang@live.com")>
    Public Module INIProfile

        Const REGEX_SECTION_HEAD As String = "^\s*\[[^]]+\]\s*$"
        Const REGEX_KEY_VALUE_ITEM As String = "^\s*[^=]+\s*=\s*.*$"

        ''' <summary>
        ''' Get the value from a specific section/key in a file of path 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("GetValue",
                   Info:="Get profile data from the ini file which the data is stores in a specific path like:  section/key")>
        Public Function GetValue(path As String, Section As String, key As String) As String
            Dim strLines As String() = (From line As String In IO.File.ReadAllLines(path)
                                        Let strLine As String = line.Trim
                                        Where Not __isCommentsOrBlank(strLine)
                                        Select strLine).ToArray
            Dim LQuery = (From line As String In strLines
                          Where Regex.Match(line, String.Format("^\s*\[{0}\]\s*$", Section)).Success
                          Select line).ToArray
            Dim Index As Integer = If(LQuery.IsNullOrEmpty, -1, Array.IndexOf(strLines, LQuery.First))

            If Index = -1 Then Return ""

            For Index = Index + 1 To strLines.Length - 1
                Dim strLine As String = strLines(Index)
                If Regex.Match(strLine.Trim, String.Format("^\s*{0}\s*=\s*.*$", key)).Success Then
                    Dim p = InStr(strLine, "=")
                    strLine = Mid(strLine, p + 1).Trim
                    Return strLine
                ElseIf Regex.Match(strLine.Trim, REGEX_SECTION_HEAD).Success Then
                    Return ""  '没有找到，则返回空值
                End If
            Next

            Return ""
        End Function

        Private Function __isCommentsOrBlank(strLine As String) As Boolean
            Return String.IsNullOrEmpty(strLine) OrElse (strLine.First = ";"c OrElse strLine.First = "#"c)
        End Function

        <ExportAPI("SetValue",
                   Info:="Setting profile data from the ini file which the data is stores in a specific path like:  section/key. If the path is not exists, the function will create new.")>
        Public Sub SetValue(path As String, Section As String, key As String, value As String)
            Dim strLines As String() = IO.File.ReadAllLines(path)
            Dim sectionFind As String = $"^\s*\[{Section}\]\s*$"
            Dim LQuery = (From line As String In strLines
                          Let strLine As String = line.Trim
                          Where Not __isCommentsOrBlank(strLine) AndAlso Regex.Match(line, sectionFind).Success
                          Select line).ToArray
            Dim index As Integer = If(LQuery.IsNullOrEmpty, -1, Array.IndexOf(strLines, LQuery.First))

            If index = -1 Then '没有找到该Section，则进行新建
                Dim sBuilder As StringBuilder = New StringBuilder(1024)
                Call sBuilder.AppendLine()
                Call sBuilder.AppendLine($"[{Section}]")
                Call sBuilder.AppendLine($"{key}={value}")
                Call FileIO.FileSystem.WriteAllText(path, sBuilder.ToString, append:=True)
            Else            '当存在该Section的时候，则从该Index位置处开始进行key的搜索
                Dim keyFind As String = $"^\s*{key}\s*=\s*.*$"

                For index = index + 1 To strLines.Length - 1
                    Dim strLine As String = strLines(index)
                    If Regex.Match(strLine.Trim, keyFind).Success Then
                        strLines(index) = $"{key}={value}"
                        Call IO.File.WriteAllLines(path, strLines)
                    ElseIf Regex.Match(strLine.Trim, REGEX_SECTION_HEAD).Success Then
                        GoTo NEW_KEY    '没有找到，则进行新建
                    End If
                Next

NEW_KEY:        Dim List = strLines.AsList
                Call List.Insert(index, $"{key}={value}")
                Call IO.File.WriteAllLines(path, List.ToArray)

            End If
        End Sub
    End Module
End Namespace
