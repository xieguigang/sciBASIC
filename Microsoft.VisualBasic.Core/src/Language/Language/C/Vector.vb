#Region "Microsoft.VisualBasic::5f4f9cddb13ef03d682594b7938ebda9, Microsoft.VisualBasic.Core\src\Language\Language\C\Vector.vb"

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

    '   Total Lines: 16
    '    Code Lines: 12 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (25.00%)
    '     File Size: 390 B


    '     Module Vector
    ' 
    '         Sub: Resize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.C

    ''' <summary>
    ''' std::vector helper for C++ language
    ''' </summary>
    Public Module Vector

        ''' <summary>
        ''' Resize a vector list
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="len">the new size of the target <paramref name="list"/></param>
        ''' <param name="fill">
        ''' fill this default value is the given list its original size is smaller than the <paramref name="len"/> new size.
        ''' </param>
        ''' <param name="preserved">
        ''' preserved the original element values inside the list?
        ''' </param>
        <Extension>
        Public Sub Resize(Of T)(ByRef list As List(Of T), len%, Optional fill As T = Nothing, Optional preserved As Boolean = False)
            If Not preserved Then
                Call list.Clear()

                ' resize a list with no value preserved.
                For i As Integer = 0 To len - 1
                    Call list.Add(fill)
                Next
            Else
                If list.Count > len Then
                    Call list.RemoveRange(len, list.Count - len)
                ElseIf list.Count < len Then
                    ' fill with the value to a new bigger size
                    For i = list.Count To len - 1
                        Call list.Add(fill)
                    Next
                End If
            End If
        End Sub
    End Module
End Namespace
