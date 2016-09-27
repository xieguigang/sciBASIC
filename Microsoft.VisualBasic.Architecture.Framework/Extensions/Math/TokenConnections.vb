#Region "Microsoft.VisualBasic::d38e17b2708188db2a98aee8f2e24574, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\TokenConnections.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq

Namespace Mathematical.Analysis

    Public Interface ITokenCount
        Property Token As String
        Property Id As Integer
        Property Count As Integer
    End Interface

    Public Interface ILink
        Property source As Integer
        Property target As Integer
        Property Count As Integer
    End Interface

    Public Module TokenConnections

        Public Sub Analysis(Of T, Tnode As ITokenCount)(
                               data As IEnumerable(Of T),
                           getValue As Func(Of T, String),
                            nodeNew As Func(Of String, Integer, Tnode),
                        ByRef nodes As Dictionary(Of String, Tnode),
                   Optional ignores As String() = Nothing)

            If ignores Is Nothing Then
                ignores = {}
            Else
                ignores = ignores _
                .Where(Function(s) Not s.IsBlank) _
                .ToArray(Function(s) s.ToLower)
            End If

            For Each x As T In data
                Dim tokens As String() = getValue(x).ToLower.Split
                tokens = tokens.Where( ' 前面已经用ToLower转换为小写了，所以在这里直接使用indexof来判断
                Function(s) Array.IndexOf(ignores, s) = -1 AndAlso
                    Regex.Match(s, "\d+:?").Value <> s).ToArray

                For Each s As String In tokens
                    If Not nodes.ContainsKey(s) Then
                        nodes(s) = nodeNew(s, nodes.Count + 1)
                    End If
                    nodes(s).Count += 1
                Next
            Next
        End Sub

        Public Sub Analysis(Of T, Tnode As ITokenCount,
                              Tlink As ILink)(
                               data As IEnumerable(Of T),
                           getValue As Func(Of T, String),
                            nodeNew As Func(Of String, Integer, Tnode),
                        ByRef nodes As Dictionary(Of String, Tnode),
                   Optional linkNew As Func(Of Integer, Integer, Tlink) = Nothing,
                   Optional ByRef links As Dictionary(Of String, Tlink) = Nothing,
                   Optional ignores As String() = Nothing)

            If ignores Is Nothing Then
                ignores = {}
            Else
                ignores = ignores _
                .Where(Function(s) Not s.IsBlank) _
                .ToArray(Function(s) s.ToLower)
            End If

            For Each x As T In data
                Dim tokens As String() = getValue(x).ToLower.Split
                tokens = tokens.Where( ' 前面已经用ToLower转换为小写了，所以在这里直接使用indexof来判断
                Function(s) Array.IndexOf(ignores, s) = -1 AndAlso
                    Regex.Match(s, "\d+:?").Value <> s).ToArray

                For Each s As String In tokens
                    If Not nodes.ContainsKey(s) Then
                        nodes(s) = nodeNew(s, nodes.Count + 1)
                    End If
                    nodes(s).Count += 1
                Next

                If linkNew Is Nothing Then
                    Continue For
                End If

                For Each s As String In tokens
                    For Each tt As String In tokens

                        If s = tt Then
                            Continue For ' 自己和自己不需要被统计
                        End If

                        Dim o As String = {s, tt}.OrderBy(Function(ss) ss).JoinBy(" --> ")
                        If Not links.ContainsKey(o) Then
                            links(o) = linkNew(nodes(s).Id, nodes(tt).Id)
                        End If
                        links(o).Count += 1
                    Next
                Next
            Next
        End Sub

        ''' <summary>
        ''' 枚举出在<paramref name="start"/>到<paramref name="ends"/>这个时间窗里面的所有日期
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="ends"></param>
        ''' <returns>返回值里面包含有起始和结束的日期</returns>
        Public Iterator Function DateSeq(start As Date, ends As Date) As IEnumerable(Of Date)
            Do While start <= ends
                ' Yield $"{start.Year}-{If(start.Month < 10, "0" & start.Month, start.Month)}-{If(start.Day < 10, "0" & start.Day, start.Day)}"
                Yield start
                start = start.AddDays(1)
            Loop
        End Function

        ''' <summary>
        ''' yyyy-mm-dd
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function YYMMDD(x As Date) As String
            Return $"{x.Year}-{If(x.Month < 10, "0" & x.Month, x.Month)}-{If(x.Day < 10, "0" & x.Day, x.Day)}"
        End Function
    End Module
End Namespace
