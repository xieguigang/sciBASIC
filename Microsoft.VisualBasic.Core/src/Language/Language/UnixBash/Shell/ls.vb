#Region "Microsoft.VisualBasic::a81909f72bcb2869129761dbd5078445, Microsoft.VisualBasic.Core\src\Language\Language\UnixBash\Shell\ls.vb"

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

    '   Total Lines: 277
    '    Code Lines: 185
    ' Comment Lines: 57
    '   Blank Lines: 35
    '     File Size: 10.15 KB


    '     Class Search
    ' 
    '         Properties: SearchType, wildcards
    ' 
    '         Function: Clone, DoFileNameGreps, ToString
    '         Operators: (+3 Overloads) -, <, <<, (+2 Overloads) <=, >
    '                    (+2 Overloads) >=
    ' 
    '     Structure wildcardsCompatible
    ' 
    '         Function: IsMatch
    ' 
    '     Structure SearchOpt
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    '         Enum Options
    ' 
    '             Directory, Ext, LongName, None, Recursive
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Patterns
Imports SearchOptions = System.Int32

Namespace Language.UnixBash

    Public Class Search : Implements ICloneable

        ''' <summary>
        ''' The search options
        ''' </summary>
        Dim opts As New Dictionary(Of SearchOpt.Options, SearchOpt)

        Public Overrides Function ToString() As String
            Return opts.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Search With {
                .opts = New Dictionary(Of SearchOpt.Options, SearchOpt)(opts)
            }
        End Function

        ''' <summary>
        ''' Add a search options
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="l"></param>
        ''' <returns></returns>
        Public Shared Operator -(ls As Search, l As SearchOpt) As Search
            If l.opt <> SearchOpt.Options.None Then
                Dim clone As Search = DirectCast(ls.Clone, Search)
                Call clone.opts.Add(l.opt, l)
                Return clone
            Else
                Return ls
            End If
        End Operator

        Public Shared Operator <<(ls As Search, opt As SearchOptions) As Search
            If CType(opt, SearchOption) = SearchOption.SearchAllSubDirectories Then
                ls.opts.Add(SearchOpt.Options.Recursive, r)
            End If

            Return ls
        End Operator

        ''' <summary>
        ''' Add a set of wildcards patterns for path match
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="wildcards$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(ls As Search, wildcards$()) As Search
            Return ls - ShellSyntax.wildcards(wildcards$)
        End Operator

        ''' <summary>
        ''' Add wildcard pattern for path match
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="wildcards$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(ls As Search, wildcards$) As Search
            Return ls - wildcards.Split(","c)
        End Operator

        Public ReadOnly Property SearchType As FileIO.SearchOption
            Get
                Dim opt As FileIO.SearchOption = FileIO.SearchOption.SearchTopLevelOnly
                If opts.ContainsKey(SearchOpt.Options.Recursive) Then
                    Return FileIO.SearchOption.SearchAllSubDirectories
                Else
                    Return opt
                End If
            End Get
        End Property

        Public ReadOnly Property wildcards As String()
            Get
                If Not opts.ContainsKey(SearchOpt.Options.Ext) Then
                    Return Nothing
                Else
                    Return opts(SearchOpt.Options.Ext) _
                        .wildcards _
                        .ToArray
                End If
            End Get
        End Property

        ''' <summary>
        ''' Search the files in the specific directory
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <(ls As Search, DIR As String) As IEnumerable(Of String)
            Return ls <= DIR
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <=(ls As Search, directories As IEnumerable(Of String)) As IEnumerable(Of String)
            Return directories _
                .SafeQuery _
                .Select(Function(dir) ls <= dir) _
                .IteratesALL
        End Operator

        ''' <summary>
        ''' Search the files in the specific directory
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="directory"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <=(ls As Search, directory$) As IEnumerable(Of String)
            If Not directory.DirectoryExists Then
                Call $"Directory {directory} is not valid on your file system!".Warning
                Return {}
            Else
                With ls
                    Dim list As IEnumerable(Of String)

                    If .opts.ContainsKey(SearchOpt.Options.Directory) Then
                        list = directory.ListDirectory(.SearchType)
                    Else
                        list = directory.ReadDirectory(.SearchType)
                    End If

                    Return DoFileNameGreps(ls, files:=list)
                End With
            End If
        End Operator

        Public Shared Function DoFileNameGreps(ls As Search, files As IEnumerable(Of String)) As IEnumerable(Of String)
            Dim l As Boolean = ls.opts.ContainsKey(SearchOpt.Options.LongName)
            Dim wc$() = ls.wildcards
            Dim isMatch As Func(Of String, Boolean) =
                AddressOf New wildcardsCompatible With {
                    .regexp = If(wc.IsNullOrEmpty, {"*"}, wc)
                }.IsMatch

            With ls
                If .opts.ContainsKey(SearchOpt.Options.Directory) Then
                    If l Then
                        Return files.Where(isMatch)
                    Else
                        Return files.Where(isMatch) _
                            .Select(Function(s)
                                        Return s.BaseName
                                    End Function)
                    End If
                Else
                    If l Then
                        Return files.Where(isMatch)
                    Else
                        Return From path As String
                               In files
                               Where isMatch(path)
                               Let name As String = path.Replace("\", "/") _
                                                        .Split("/"c) _
                                                        .Last
                               Select name
                    End If
                End If
            End With
        End Function

        Public Shared Operator >(ls As Search, DIR As String) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator

        Public Shared Operator >=(ls As Search, DIR As String) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator

        Public Shared Operator >=(ls As Search, dirs As IEnumerable(Of String)) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator
    End Class

    ''' <summary>
    ''' Using regular expression to find a match on the file name.
    ''' </summary>
    Public Structure wildcardsCompatible

        Dim regexp As String()
        ''' <summary>
        ''' Using the regexp engine instead of the wildcard match engine?
        ''' </summary>
        Dim usingRegexp As Boolean

        ''' <summary>
        ''' Windows系统上面文件路径不区分大小写，但是Linux、Mac系统却区分大小写
        ''' 所以使用这个来保持对Windows文件系统的兼容性
        ''' </summary>
        Shared ReadOnly opt As RegexOptions =
            If(App.Platform = PlatformID.MacOSX OrElse
               App.Platform = PlatformID.Unix,
               RegexOptions.Singleline, RegexICSng)
        Shared ReadOnly pathIgnoreCase As Boolean = App.IsMicrosoftPlatform

        ''' <summary>
        ''' Linux/Mac系统不支持Windows系统的通配符，所以在这里是用正则表达式来保持代码的兼容性
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function IsMatch(path As String) As Boolean
            If regexp.IsNullOrEmpty Then
                ' 匹配的规则是空的，则默认是允许所有的路径
                Return True
            End If

            Dim name As String = path.Replace("\", "/").Split("/"c).Last

            If usingRegexp Then
                For Each r As String In regexp
                    If Regex.Match(name, r, opt).Success Then
                        Return True
                    End If
                Next
            Else
                For Each r As String In regexp
                    If WildcardsExtension.WildcardMatch(r, name, pathIgnoreCase) Then
                        Return True
                    End If
                Next
            End If

            Return False
        End Function
    End Structure

    Public Structure SearchOpt

        Dim opt As Options
        Dim value As String
        Dim wildcards As List(Of String)

        Sub New(opt As Options, s As String)
            Me.opt = opt
            Me.value = s
            Me.wildcards = New List(Of String)
        End Sub

        Sub New(opt As Options)
            Call Me.New(opt, "")
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Enum Options
            None
            Ext
            LongName
            ''' <summary>
            ''' List directories, not files listing.
            ''' </summary>
            Directory
            ''' <summary>
            ''' 递归搜索所有的文件夹
            ''' </summary>
            Recursive
        End Enum
    End Structure
End Namespace
