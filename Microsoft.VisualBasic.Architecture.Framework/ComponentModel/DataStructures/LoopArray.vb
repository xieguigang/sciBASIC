#Region "Microsoft.VisualBasic::73e254a58e36e13a1a045ff26379774a, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\LoopArray.vb"

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

Namespace ComponentModel.DataStructures

    Public Class LoopArray(Of T)

        Dim __innerArray As T()
        Dim __p As Integer

        Sub New(source As IEnumerable(Of T))
            __innerArray = source.ToArray
        End Sub

        ''' <summary>
        ''' Gets the next elements in the array, is move to end, then the index will moves to the array begining position.
        ''' </summary>
        ''' <returns></returns>
        Public Function [GET]() As T
            If __p < __innerArray.Length - 1 Then
                __p += 1
            Else
                __p = 0
            End If

            Return __innerArray(__p)
        End Function
    End Class
End Namespace
