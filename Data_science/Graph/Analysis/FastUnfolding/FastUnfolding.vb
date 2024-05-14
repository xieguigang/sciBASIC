#Region "Microsoft.VisualBasic::ae609f7e3db50fdd235d3b0d83331196, Data_science\Graph\Analysis\FastUnfolding\FastUnfolding.vb"

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

    '   Total Lines: 116
    '    Code Lines: 78
    ' Comment Lines: 15
    '   Blank Lines: 23
    '     File Size: 3.82 KB


    '     Class FastUnfolding
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Analysis, members, rebuildMap, tagMatrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Analysis.FastUnfolding

    ''' <summary>
    ''' Fast unfolding of communities in large networks.
    ''' </summary>
    Public Class FastUnfolding

        Dim tag_dict As New Dictionary(Of String, String)
        Dim member As KeyMaps
        Dim map_dict As KeyMaps

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="map_dict">
        ''' the network data: [a -> b[]]
        ''' </param>
        Sub New(map_dict As KeyMaps)
            Me.map_dict = map_dict
            Me.tag_dict = map_dict.Keys.ToDictionary(Function(k) k)
            Me.member = members(tag_dict)
        End Sub

        Public Function members(tags As Dictionary(Of String, String)) As KeyMaps
            Dim member As New KeyMaps

            For Each i In tags.Keys
                member(tags(i)).Add(i)
            Next

            Return member
        End Function

        Friend Function tagMatrix(tag2 As Dictionary(Of String, String), map2 As KeyMaps, Q As Double) As Matrix
            For Each u As String In map2.Keys
                For Each v As String In map2(u)
                    Dim tag_dict_copy As New Dictionary(Of String, String)(tag_dict)
                    Dim tag_dict2_copy As New Dictionary(Of String, String)(tag2)

                    tag_dict2_copy(u) = tag_dict2_copy(v)

                    For Each p As String In member(u)
                        tag_dict_copy(p) = tag_dict2_copy(v)
                    Next

                    Dim Q_new As Double = tag_dict_copy.Modularity(map_dict)

                    If Q_new > Q Then
                        Q = Q_new
                        tag_dict = tag_dict_copy
                        tag2 = tag_dict2_copy
                    End If
                Next
            Next

            Return New Matrix With {
                .Q = Q,
                .tag_dict = tag_dict,
                .tag_dict2 = tag2
            }
        End Function

        ''' <summary>
        ''' 将一个社区作为一个节点重新构造图
        ''' </summary>
        ''' <param name="tag_dict"></param>
        ''' <param name="map_dict"></param>
        ''' <returns></returns>
        Private Function rebuildMap(tag_dict As Dictionary(Of String, String), map_dict As KeyMaps) As Map
            Dim map2 As New KeyMaps

            For Each u As String In map_dict.Keys
                Dim tagu = tag_dict(u)

                For Each v As String In map_dict.Keys
                    Dim tagv = tag_dict(v)

                    If tagu <> tagv AndAlso map_dict(u).Lookups(v).Count <> 0 AndAlso map2(tagu).Lookups(tagv).Count = 0 Then
                        map2(tagu).Add(tagv)
                    End If
                Next
            Next

            Return New Map With {
                .tag2 = map2.Keys.ToDictionary(Function(k) k),
                .map2 = map2
            }
        End Function

        Public Function Analysis() As (community As KeyMaps, Q As Double)
            Dim Q As Double = 0
            Dim Q_new As Double = tag_dict.Modularity(map_dict)
            Dim tag_dict2 = tag_dict
            Dim map_dict2 = map_dict

            Do While Q <> Q_new
                Q = Q_new

                With tagMatrix(tag_dict2, map_dict2, Q)
                    Q_new = .Q
                    tag_dict = .tag_dict
                    tag_dict2 = .tag_dict2
                End With

                member = members(tag_dict)

                With rebuildMap(tag_dict, map_dict)
                    tag_dict2 = .tag2
                    map_dict2 = .map2
                End With
            Loop

            Return (member, Q)
        End Function
    End Class
End Namespace
