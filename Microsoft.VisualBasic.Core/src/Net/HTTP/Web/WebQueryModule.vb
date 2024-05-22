#Region "Microsoft.VisualBasic::b1ee04b67b0e34a0d5f73e3634ce0467, Microsoft.VisualBasic.Core\src\Net\HTTP\Web\WebQueryModule.vb"

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

    '   Total Lines: 70
    '    Code Lines: 31 (44.29%)
    ' Comment Lines: 25 (35.71%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 14 (20.00%)
    '     File Size: 2.35 KB


    '     Interface IHttpGet
    ' 
    '         Function: GetText
    ' 
    '     Class WebQueryModule
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: contextPrefix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.FileIO

Namespace Net.Http

    ''' <summary>
    ''' the abstract model for the http proxy get request
    ''' </summary>
    Public Interface IHttpGet

        Function GetText(url As String) As String

    End Interface

    Public MustInherit Class WebQueryModule(Of Context) : Inherits WebQuery(Of Context)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cache">the cache directory path</param>
        ''' <param name="interval"></param>
        ''' <param name="offline"></param>
        Sub New(<CallerMemberName>
                Optional cache$ = Nothing,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call Me.New(New Directory(cache), interval, offline)
        End Sub

        Sub New(cache As IFileSystemEnvironment,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call MyBase.New(cache, interval, offline)

            Me.contextGuid = AddressOf doParseGuid
            Me.deserialization = AddressOf doParseObject
            Me.url = AddressOf doParseUrl
            Me.prefix = AddressOf contextPrefix
        End Sub

        ''' <summary>
        ''' generate url for run data query
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Protected MustOverride Function doParseUrl(context As Context) As String
        ''' <summary>
        ''' parse query text to data object
        ''' </summary>
        ''' <param name="html"></param>
        ''' <param name="schema"></param>
        ''' <returns></returns>
        Protected MustOverride Function doParseObject(html As String, schema As Type) As Object

        ''' <summary>
        ''' 生成缓存所使用的一个唯一标识符的生成函数
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Protected MustOverride Function doParseGuid(context As Context) As String

        Protected Overridable Function contextPrefix(guid As String) As String
            Return ""
        End Function

    End Class
End Namespace
