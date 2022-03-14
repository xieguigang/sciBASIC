Namespace Analysis.FastUnfolding

    ''' <summary>
    ''' Fast unfolding of communities in large networks.
    ''' </summary>
    Public Class FastUnfolding

        Public Function com_member(tag_dict As Dictionary(Of String, String)) As KeyMaps
            Dim member As New KeyMaps

            For Each i In tag_dict.Keys
                member(tag_dict(i)).Add(i)
            Next

            Return member
        End Function

        Public Function modularity(tag_dict As Dictionary(Of String, String), map_dict As KeyMaps) As Double
            ' 根据tag和图的连接方式计算模块度
            Dim m As Double = 0
            Dim community_dict As New KeyMaps
            '同属一个社群的人都有谁

            For Each key In map_dict.Keys
                m += map_dict(key).Count
                community_dict(tag_dict(key)).Add(key)
            Next

            Dim Q As Double = 0

            For Each com In community_dict.Keys
                Dim sum_in As Double = 0
                Dim sum_tot As Double = 0

                For Each u In community_dict(com)
                    sum_tot += map_dict(u).Count

                    For Each v In map_dict(u)
                        If tag_dict(v) = tag_dict(u) Then
                            sum_in += 1
                        End If
                    Next
                Next

                Q += (sum_in / m - (sum_tot / m) ^ 2)
            Next

            Return Q
        End Function

        Dim tag_dict As New Dictionary(Of String, String)
        Dim member As KeyMaps
        Dim map_dict As KeyMaps

        Public Function changeTagRound(tag_dict2 As Dictionary(Of String, String),
                                       map_dict2 As KeyMaps,
                                       Q As Double) As (Q As Double, tag_dict As Dictionary(Of String, String), tag_dict2 As Dictionary(Of String, String))

            For Each u In map_dict2.Keys
                For Each v In map_dict2(u)
                    Dim tag_dict_copy = tag_dict.ToDictionary
                    Dim tag_dict2_copy = tag_dict2.ToDictionary
                    tag_dict2_copy(u) = tag_dict2_copy(v)

                    For Each p In member(u)
                        tag_dict_copy(p) = tag_dict2_copy(v)
                    Next

                    Dim Q_new = modularity(tag_dict_copy, map_dict)

                    If Q_new > Q Then
                        Q = Q_new
                        tag_dict = tag_dict_copy
                        tag_dict2 = tag_dict2_copy
                    End If
                Next
            Next

            Return (Q, tag_dict, tag_dict2)
        End Function

        Public Function rebuildMap(tag_dict As Dictionary(Of String, String), map_dict As KeyMaps) As (tag2 As Dictionary(Of String, String), map2 As KeyMaps)
            ' 将一个社区作为一个节点重新构造图
            Dim map2 As New KeyMaps

            For Each u In map_dict.Keys
                Dim tagu = tag_dict(u)

                For Each v In map_dict.Keys
                    Dim tagv = tag_dict(v)

                    If tagu <> tagv AndAlso map_dict(u).Lookups(v).Count <> 0 AndAlso map2(tagu).Lookups(tagv).Count = 0 Then
                        map2(tagu).Add(tagv)
                    End If
                Next
            Next

            Dim tag2 = map2.Keys.ToDictionary(Function(k) k)

            Return (tag2, map2)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="map_dict">
        ''' the network data: [a -> b[]]
        ''' </param>
        ''' <returns></returns>
        Public Function Analysis(map_dict As KeyMaps) As (KeyMaps, Double)
            Dim Q As Double = 0

            Me.map_dict = map_dict
            Me.tag_dict = map_dict.Keys.ToDictionary(Function(k) k)
            Me.member = com_member(tag_dict)

            Dim Q_new As Double = modularity(tag_dict, map_dict)
            Dim tag_dict2 = tag_dict
            Dim map_dict2 = map_dict

            Do While Q <> Q_new
                Q = Q_new
                Dim x = changeTagRound(tag_dict2, map_dict2, Q)

                Q_new = x.Q
                tag_dict = x.tag_dict
                tag_dict2 = x.tag_dict2
                member = com_member(tag_dict)

                Dim y = rebuildMap(tag_dict, map_dict)

                tag_dict2 = y.tag2
                map_dict2 = y.map2
            Loop

            Return (member, Q)
        End Function
    End Class
End Namespace