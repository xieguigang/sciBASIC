#Region "Microsoft.VisualBasic::9d39cdbf9d3d1c713831c04ed8294690, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Linq\LanguageExtensions.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace Language

    Public Module LanguageExtensions

        ''' <summary>
        ''' New List From syntax supports
        ''' 
        ''' ```
        ''' {Name, value, Description?}
        ''' ```
        ''' </summary>
        <Extension> Public Sub Add(Of T)(list As List(Of NamedValue(Of T)), name$, value As T, Optional descript$ = Nothing)
            list += New NamedValue(Of T) With {
                .Name = name,
                .Value = value,
                .Description = descript
            }
        End Sub

        ''' <summary>
        ''' From {"1,100", "100,1000", "200,500"}
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="range$"></param>
        <Extension> Public Sub Add(list As List(Of IntRange), range$)
            list += range
        End Sub

        <Extension> Public Sub Add(list As List(Of DoubleRange), range$)
            list += range
        End Sub
    End Module
End Namespace
