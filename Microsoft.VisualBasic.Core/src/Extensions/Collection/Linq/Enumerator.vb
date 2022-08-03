#Region "Microsoft.VisualBasic::8ef5e07986e9065218497a65eeada9e2, sciBASIC#\Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\Enumerator.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 681 B


    '     Class Enumerator
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Linq

    Friend Class Enumerator(Of T) : Implements IEnumerable(Of T)

        Public Enumeration As Enumeration(Of T)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return Enumeration.GenericEnumerator
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return Enumeration.GetEnumerator
        End Function
    End Class
End Namespace
