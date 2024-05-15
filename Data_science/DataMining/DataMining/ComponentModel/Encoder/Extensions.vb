#Region "Microsoft.VisualBasic::f8ea19c8ec55061968ad51aee2367ee6, Data_science\DataMining\DataMining\ComponentModel\Encoder\Extensions.vb"

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

    '   Total Lines: 42
    '    Code Lines: 35
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 1.65 KB


    '     Module Extensions
    ' 
    '         Function: ClassEncoder, GetClassMapping, ToEnumsTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging

Namespace ComponentModel.Encoder

    <HideModuleName>
    Public Module Extensions

        <Extension>
        Public Function ToEnumsTable(Of T)(classes As IEnumerable(Of ColorClass)) As Dictionary(Of T, ColorClass)
            Return classes.ToDictionary(Function(c) DirectCast(CObj(c.factor), T))
        End Function

        <Extension>
        Public Iterator Function ClassEncoder(labels As IEnumerable(Of String), Optional distance As Integer = 1) As IEnumerable(Of ColorClass)
            Dim raw As String() = labels.ToArray
            Dim unique As Index(Of String) = raw.Distinct.Indexing

            With Imaging.AllDotNetPrefixColors.AsLoop
                Dim colors As String() = unique.Objects _
                    .Select(Function(a)
                                Return .Next.ToHtmlColor
                            End Function) _
                    .ToArray

                For Each label As String In raw
                    Yield New ColorClass With {
                        .factor = unique.IndexOf(label) * distance,
                        .name = label,
                        .color = colors(.factor)
                    }
                Next
            End With
        End Function

        <Extension>
        Public Function GetClassMapping(labels As IEnumerable(Of String), Optional distance As Integer = 1) As ColorClass()
            Return labels.Distinct.ClassEncoder.ToArray
        End Function
    End Module
End Namespace
