#Region "Microsoft.VisualBasic::5f4f9cddb13ef03d682594b7938ebda9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\C\Vector.vb"

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

Namespace Language.C

    Public Module Vector

        <Extension>
        Public Sub Resize(Of T)(ByRef list As List(Of T), len%, Optional fill As T = Nothing)
            Call list.Clear()

            For i As Integer = 0 To len - 1
                Call list.Add(fill)
            Next
        End Sub
    End Module
End Namespace
