#Region "Microsoft.VisualBasic::999787527675817aa205be340df17256, ..\sciBASIC#\Microsoft.VisualBasic.Core\Language\Linq\Vectorization\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Namespace Language.Vectorization

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVector(booleans As IEnumerable(Of Boolean)) As BooleanVector
            Return New BooleanVector(booleans)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVector(Of T)(list As IEnumerable(Of T)) As Vector(Of T)
            Return New Vector(Of T)(list)
        End Function
    End Module
End Namespace
