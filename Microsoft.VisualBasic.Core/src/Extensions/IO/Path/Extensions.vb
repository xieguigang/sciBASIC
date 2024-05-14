#Region "Microsoft.VisualBasic::1b9aad55631216c0693df3837adeb35e, Microsoft.VisualBasic.Core\src\Extensions\IO\Path\Extensions.vb"

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

    '   Total Lines: 50
    '    Code Lines: 41
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.99 KB


    '     Module Extensions
    ' 
    '         Function: Combine, GetFullPath, Pairs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports IOModule = System.IO.Path

Namespace FileIO.Path

    Public Module Extensions

        Public Iterator Function Pairs(ParamArray paths As NamedValue(Of String())()) As IEnumerable(Of Dictionary(Of String, String))
            Dim primary As NamedValue(Of String()) = paths(Scan0)
            Dim others = (From path As NamedValue(Of String())
                      In paths.Skip(1)
                          Select path.Name,
                          pls = (From p As String
                                 In path.Value
                                 Select pName = p.BaseName,
                                     p).ToArray).ToArray

            For Each path As String In primary.Value
                Dim q As String = path.BaseName
                Dim result As New Dictionary(Of String, String) From {{primary.Name, path}}

                For Each otherpath In others
                    For Each S In otherpath.pls
                        If InStr(q, S.pName, CompareMethod.Text) = 1 OrElse
                        InStr(S.pName, q, CompareMethod.Text) = 1 Then

                            result.Add(otherpath.Name, S.p)
                            Exit For
                        End If
                    Next
                Next

                If result.Count = paths.Length Then
                    Yield result
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFullPath(relativePath As String) As String
            Return IOModule.GetFullPath(relativePath)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Combine(destination$, name$) As String
            Return IOModule.Combine(destination, name)
        End Function
    End Module
End Namespace
