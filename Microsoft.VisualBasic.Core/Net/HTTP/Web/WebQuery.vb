#Region "Microsoft.VisualBasic::ac6cbdb98e7f8d3cc2e1e2fa8b7354eb, Microsoft.VisualBasic.Core\Net\HTTP\Web\WebQuery.vb"

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

    '     Class WebQuery
    ' 
    '         Properties: offlineMode
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: IsNullKey, (+2 Overloads) Query, QueryCacheText, queryText
    ' 
    '         Sub: runHttpGet
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET_48 Then

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization

Namespace Net.Http

    ''' <summary>
    ''' <typeparamref name="Context"/>类型参数应该是查询的term的数据类型, 而非返回的查询结果的数据类型
    ''' </summary>
    ''' <typeparam name="Context"></typeparam>
    ''' <remarks>
    ''' 这个模块不会重复请求404状态的资源
    ''' </remarks>
    ''' 
    <FrameworkConfig(WebQuery(Of Boolean).WebQueryDebug)>
    Public Class WebQuery(Of Context)

        Friend url As Func(Of Context, String)
        Friend contextGuid As IToString(Of Context)
        Friend deserialization As IObjectBuilder
        Friend prefix As Func(Of String, String)

        ''' <summary>
        ''' 404状态的资源列表
        ''' </summary>
        Dim url404 As New Index(Of String)

        ''' <summary>
        ''' 是否是处于仅从缓存数据之中查找结果的离线模式
        ''' </summary>
        ''' <returns></returns>
        Public Property offlineMode As Boolean

        ''' <summary>
        ''' 原始请求结果数据的缓存文件夹,同时也可以用这个文件夹来存放错误日志
        ''' </summary>
        Protected cache$
        Protected sleepInterval As Integer

        Protected Shared debug As Boolean = True

        Shared ReadOnly interval As [Default](Of Integer)

        Friend Const WebQueryDebug As String = "webquery.debug"

        Shared Sub New()
            Static defaultInterval As [Default](Of String) = "3000"

            With Val(App.GetVariable("sleep") Or defaultInterval)
                ' controls of the interval by /@set sleep=xxxxx
                interval = CInt(.ByRef).AsDefault(Function(x) x <= 0)
                debug = App.GetVariable(WebQueryDebug).ParseBoolean
            End With

            If debug Then
                ' display debug info
                Call $"WebQuery download worker for query context [{GetType(Context).FullName}] thread sleep interval is {interval}ms".__INFO_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url">请注意,查询词应该是被<see cref="UrlEncode"/>所转义过的</param>
        ''' <param name="contextGuid"></param>
        ''' <param name="parser"></param>
        ''' <param name="prefix">
        ''' 如果查询的结果文件很多, 则缓存放在同一下文件夹下, 打开的效率会非常低,
        ''' 在这里使用这个函数来得到分组前缀用作为文件夹名,分组存放缓存数据
        ''' </param>
        ''' <param name="cache$"></param>
        ''' <param name="interval%"></param>
        Sub New(url As Func(Of Context, String),
                Optional contextGuid As IToString(Of Context) = Nothing,
                Optional parser As IObjectBuilder = Nothing,
                Optional prefix As Func(Of String, String) = Nothing,
                <CallerMemberName>
                Optional cache$ = Nothing,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call Me.New(cache, interval, offline)

            Me.url = url
            Me.contextGuid = contextGuid Or Scripting.ToString(Of Context)
            Me.deserialization = parser Or XmlParser
            Me.prefix = prefix
        End Sub

        Friend Sub New(cache$, interval%, offline As Boolean)
            Me.cache = cache
            Me.sleepInterval = interval Or WebQuery(Of Context).interval
            Me.offlineMode = offline

            If offlineMode AndAlso debug Then
                Call $"WebQuery of '{Me.GetType.Name}' running in offline mode!".__DEBUG_ECHO
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function IsNullKey(key As Object) As Boolean
            Return ExceptionHandle.Default(key)
        End Function

        ''' <summary>
        ''' 这个函数返回的是缓存的本地文件的路径列表
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="type">文件拓展名，可以不带有小数点</param>
        ''' <returns></returns>
        Protected Iterator Function queryText(query As IEnumerable(Of Context), type$) As IEnumerable(Of (cache$, hitCache As Boolean))
            ' 因为在这里是进行批量的数据库查询
            ' 所以在这个函数内的代码的执行效率不会被考虑在内
            For Each context As Context In query
                If IsNullKey(context) Then
                    Yield ("", True)
                End If

                Dim url = Me.url(context)
                Dim id$ = Me.contextGuid(context)
                Dim cache$
                ' 如果是进行一些分子名称的查询,可能会因为分子名称超长而导致文件系统api调用出错
                ' 所以在这里需要截短一下文件名称
                ' 因为路径的总长度不能超过260个字符,所以文件名这里截短到200字符以内,留给文件夹名称一些长度
                Dim baseName$ = Mid(id, 1, 192)
                Dim hitCache As Boolean = True

                If prefix Is Nothing Then
                    cache = $"{Me.cache}/{baseName}.{type.Trim("."c, "*"c)}"
                Else
                    cache = $"{Me.cache}/{prefix(id)}/{baseName}.{type.Trim("."c, "*"c)}"
                End If

                If Not url Like url404 Then
                    Call runHttpGet(cache, url, hitCache)
                ElseIf debug Then
                    Call $"{id} 404 Not Found!".PrintException
                End If

                Yield (cache, hitCache)
            Next
        End Function

        Private Sub runHttpGet(cache As String, url$, ByRef hitCache As Boolean)
            Dim is404 As Boolean = False

            If cache.FileLength <= 0 AndAlso Not offlineMode Then
                Call url.GET(is404:=is404).SaveTo(cache)
                Call Thread.Sleep(sleepInterval)

                If is404 Then
                    url404 += url
                    Call $"{url} 404 Not Found!".PrintException
                ElseIf debug Then
                    Call $"Worker thread sleep {sleepInterval}ms...".__INFO_ECHO
                End If

                hitCache = False
            ElseIf debug Then
                Call "hit cache!".__DEBUG_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="context"></param>
        ''' <param name="cacheType">缓存文件的文本格式拓展名</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Query(Of T)(context As Context, Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As T
            Return QueryCacheText(context, cacheType, hitCache).DoCall(Function(text) deserialization(text, GetType(T)))
        End Function

        Public Function QueryCacheText(context As Context, Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As String
            hitCache = False

            If context Is Nothing Then
                Return Nothing
            Else
                Dim result As (cache$, hitCache As Boolean) = queryText({context}, cacheType).First
                Dim cache As String = result.cache.ReadAllText(throwEx:=False)

                hitCache = result.hitCache

                Return cache
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="context"></param>
        ''' <param name="cacheType">缓存文件的文本格式拓展名</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Query(Of T)(context As IEnumerable(Of Context), Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As IEnumerable(Of T)
            Dim result = queryText(context, cacheType).ToArray
            Dim hits = result.Where(Function(pop) pop.hitCache).Count

            hitCache = (hits / result.Length) > 0.6

            Return result _
                .Select(Function(file)
                            Return file.cache _
                                .ReadAllText(throwEx:=False) _
                                .DoCall(Function(text)
                                            Return deserialization(text, GetType(T))
                                        End Function)
                        End Function) _
                .As(Of T)
        End Function
    End Class
End Namespace

#End If
