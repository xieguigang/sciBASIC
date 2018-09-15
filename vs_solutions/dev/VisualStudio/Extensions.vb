#Region "Microsoft.VisualBasic::683691caaabf6d5e619e6261c4dc4b49, vs_solutions\dev\VisualStudio\Extensions.vb"

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

    ' Module Extensions
    ' 
    '     Function: EnumerateSourceFiles
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    ''' <summary>
    ''' Enumerate all of the vb source files in this vbproj.
    ''' </summary>
    ''' <param name="vbproj"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function EnumerateSourceFiles(vbproj As String) As IEnumerable(Of String)
        Return vbproj _
            .LoadXml(Of Project) _
            .ItemGroups _
            .Where(Function(items) Not items.Compiles.IsNullOrEmpty) _
            .Select(Function(items)
                        Return items.Compiles _
                            .Where(Function(vb)
                                       Return Not True = vb.AutoGen.ParseBoolean
                                   End Function) _
                            .Select(Function(vb)
                                        Return vb.Include.Replace("%28", "(").Replace("%29", ")")
                                    End Function)
                    End Function) _
            .IteratesALL
    End Function
End Module
