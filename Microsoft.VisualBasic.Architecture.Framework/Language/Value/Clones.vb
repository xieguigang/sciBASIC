#Region "Microsoft.VisualBasic::24e140f55c3cf7e559838feb896ab08f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Value\Clones.vb"

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

Namespace Language.Values

    ''' <summary>
    ''' Some extension for copy a collection object.
    ''' </summary>
    Public Module Clones

        ''' <summary>
        ''' Creates a new dictionary
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="hash"></param>
        ''' <returns></returns>
        <Extension> Public Function Clone(Of T, V)(hash As IDictionary(Of T, V)) As Dictionary(Of T, V)
            Return New Dictionary(Of T, V)(hash)
        End Function

        <Extension> Public Function Clone(Of T)(list As List(Of T)) As List(Of T)
            Return New List(Of T)(list)
        End Function

        <Extension> Public Function CloneCopy(Of T)(array As T()) As T()
            Return DirectCast(array.Clone, T())
        End Function

        <Extension> Public Function Clone(s As String) As String
            Return New String(s.ToCharArray)
        End Function

        <Extension> Public Function Clone(int As int) As int
            Return New int(int.value)
        End Function

        <Extension> Public Function Clone(float As float) As float
            Return New float(float.value)
        End Function
    End Module
End Namespace
