#Region "Microsoft.VisualBasic::735e7947e31cef5c12dc1093ec5d51d1, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\Enumerable\ILocalSearchHandle.vb"

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

Namespace ComponentModel.Collection

    Public Interface ILocalSearchHandle

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Matches(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As ILocalSearchHandle()
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Keyword"></param>
        ''' <param name="CaseSensitive">是否大小写敏感，默认不敏感</param>
        ''' <returns></returns>
        Function Match(Keyword As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As Boolean
    End Interface
End Namespace
