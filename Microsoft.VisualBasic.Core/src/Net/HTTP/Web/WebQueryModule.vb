#Region "Microsoft.VisualBasic::7db4f975cd9d698eff5a21483dcafea3, Microsoft.VisualBasic.Core\src\Net\HTTP\Web\WebQueryModule.vb"

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

    '     Class WebQueryModule
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: contextPrefix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET_48 Or netcore5 = 1 Then

Imports System.Runtime.CompilerServices

Namespace Net.Http

    Public MustInherit Class WebQueryModule(Of Context) : Inherits WebQuery(Of Context)

        Sub New(<CallerMemberName>
                Optional cache$ = Nothing,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call MyBase.New(cache, interval, offline)

            Me.contextGuid = AddressOf doParseGuid
            Me.deserialization = AddressOf doParseObject
            Me.url = AddressOf doParseUrl
            Me.prefix = AddressOf contextPrefix
        End Sub

        Protected MustOverride Function doParseUrl(context As Context) As String
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
#End If
