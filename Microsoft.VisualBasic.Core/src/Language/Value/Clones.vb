#Region "Microsoft.VisualBasic::da06393bffe69bbe757e33a122958bdb, Microsoft.VisualBasic.Core\src\Language\Value\Clones.vb"

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

    '   Total Lines: 41
    '    Code Lines: 23 (56.10%)
    ' Comment Lines: 10 (24.39%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (19.51%)
    '     File Size: 1.34 KB


    '     Module Clones
    ' 
    '         Function: (+5 Overloads) Clone, CloneCopy
    ' 
    ' 
    ' /********************************************************************************/

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
        ''' <param name="table"></param>
        ''' <returns></returns>
        <Extension> Public Function Clone(Of T, V)(table As IDictionary(Of T, V)) As Dictionary(Of T, V)
            Return New Dictionary(Of T, V)(table)
        End Function

        <Extension> Public Function Clone(Of T)(list As List(Of T)) As List(Of T)
            Return New List(Of T)(list)
        End Function

        <Extension>
        Public Function CloneCopy(Of T)(array As T()) As T()
            Return DirectCast(array.Clone, T())
        End Function

        <Extension> Public Function Clone(s As String) As String
            Return New String(s.ToCharArray)
        End Function

        <Extension> Public Function Clone(int As i32) As i32
            Return New i32(int.value)
        End Function

        <Extension> Public Function Clone(float As f64) As f64
            Return New f64(float.value)
        End Function
    End Module
End Namespace
