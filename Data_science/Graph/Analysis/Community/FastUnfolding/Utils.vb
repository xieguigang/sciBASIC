#Region "Microsoft.VisualBasic::1ddf2b0271c7c6277d858c5709b4b337, Data_science\Graph\Analysis\FastUnfolding\Utils.vb"

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

    '   Total Lines: 48
    '    Code Lines: 31 (64.58%)
    ' Comment Lines: 8 (16.67%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 9 (18.75%)
    '     File Size: 1.46 KB


    '     Module Utils
    ' 
    '         Function: Modularity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Analysis.FastUnfolding

    Module Utils

        ''' <summary>
        ''' 根据tag和图的连接方式计算模块度
        ''' </summary>
        ''' <param name="tags"></param>
        ''' <param name="map_dict"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Modularity(tags As Dictionary(Of String, String), map_dict As KeyMaps) As Double
            Dim m As Double = 0
            Dim community As New KeyMaps
            Dim Q As Double = 0
            Dim sum_in As Double = 0
            Dim sum_tot As Double = 0

            ' 同属一个社群的人都有谁
            For Each key As String In map_dict.Keys
                m += map_dict(key).Count
                community(tags(key)).Add(key)
            Next

            For Each com As String In community.Keys
                sum_in = 0
                sum_tot = 0

                For Each u As String In community(com)
                    sum_tot += map_dict(u).Count

                    For Each v As String In map_dict(u)
                        If tags(v) = tags(u) Then
                            sum_in += 1
                        End If
                    Next
                Next

                Q += (sum_in / m - (sum_tot / m) ^ 2)
            Next

            Return Q
        End Function
    End Module
End Namespace
