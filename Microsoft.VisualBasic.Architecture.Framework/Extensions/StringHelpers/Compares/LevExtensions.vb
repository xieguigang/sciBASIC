#Region "Microsoft.VisualBasic::793ad88e567b4f8b825d81dabcbc270c, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\Extensions\StringHelpers\Compares\LevExtensions.vb"

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

Imports System.Runtime.CompilerServices

Public Module LevExtensions

    <Extension> Public Sub GetMatches(Of T)(edits As DistResult, ref As T(), hyp As T(), ByRef refOUT As T(), ByRef hypOUT As T())
        Dim len As Integer = edits.DistEdits.Count("m"c)
        Dim idx As Integer = -1
        Dim iiiii As Integer = 0

        refOUT = New T(len - 1) {}
        hypOUT = New T(len - 1) {}

        For j As Integer = 0 To hyp.Length   ' 参照subject画列
            For i As Integer = 0 To ref.Length  ' 参照query画行
                If edits.IsPath(i, j) Then
                    Dim ch As String = edits.DistEdits.Get(idx.MoveNext)

                    If ch = "m"c Then
                        refOUT(iiiii) = ref(i)
                        hypOUT(iiiii) = hyp(j - 1)

                        iiiii += 1
                    End If
                End If
            Next
        Next
    End Sub
End Module
