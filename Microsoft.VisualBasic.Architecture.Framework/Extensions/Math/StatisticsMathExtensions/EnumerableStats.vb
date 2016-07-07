#Region "Microsoft.VisualBasic::8aaf769945f72267f7612ceff23713da, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\StatisticsMathExtensions\EnumerableStats.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Diagnostics
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Namespace StatisticsMathExtensions

    Public Module EnumerableStats

        <Extension>
        Public Function Coalesce(Of T As Structure)(source As IEnumerable(Of System.Nullable(Of T))) As IEnumerable(Of T)
            Debug.Assert(source IsNot Nothing)
            Return source.Where(Function(x) x.HasValue).[Select](Function(x) CType(x, T))
        End Function
    End Module
End Namespace
