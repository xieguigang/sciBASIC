Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.BinaryTree

    Public Enum ComparisonDirectionPrefers
        Left
        Right
    End Enum

    ''' <summary>
    ''' wrapper for the cluster data
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    Public Class ClusterKey(Of K)

        ReadOnly members As New List(Of K)
        ReadOnly views As Func(Of K, String)

        Public ReadOnly Property NumberOfKey As Integer
            Get
                Return members.Count
            End Get
        End Property

        Default Public ReadOnly Property Item(index As Integer) As K
            Get
                Return members(index)
            End Get
        End Property

        ''' <summary>
        ''' use the first element in current cluster member set as the root element
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Root As K
            Get
                Return members(Scan0)
            End Get
        End Property

        Sub New([single] As K, views As Func(Of K, String))
            Me.views = views

            ' Add a initial single member object
            Call members.Add([single])
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(newMember As K)
            Call members.Add(newMember)
        End Sub

        Public Overrides Function ToString() As String
            If members.Count = 1 Then
                Return views(members(Scan0))
            Else
                Return views(members(Scan0)) & $", and with {members.Count} cluster members.."
            End If
        End Function

        Public Function ToArray() As K()
            Return members.ToArray
        End Function

        Private Shared Function PreferDirection(prefer As ComparisonDirectionPrefers) As Func(Of Boolean, Boolean, Integer)
            Return Function(left As Boolean, right As Boolean) As Integer
                       If prefer = ComparisonDirectionPrefers.Left Then
                           If left Then
                               Return -1
                           Else
                               Return 1
                           End If
                       Else
                           If right Then
                               Return 1
                           Else
                               Return -1
                           End If
                       End If
                   End Function
        End Function

        Private Shared Function ComparesAll(compares As Comparison(Of K), dir As Func(Of Boolean, Boolean, Integer)) As Func(Of ClusterKey(Of K), K, Integer)
            Return Function(cluster, key) As Integer
                       Dim compareVal As Value(Of Integer) = -100
                       Dim left As Boolean = False
                       Dim right As Boolean

                       For Each index As K In cluster.members
                           If (compareVal = compares(index, key)) = 0 Then
                               Return 0
                           Else
                               If compareVal.Equals(1) Then
                                   right = True
                               Else
                                   left = True
                               End If
                           End If
                       Next

                       Return dir(left, right)
                   End Function
        End Function

        Private Shared Function JustComparesRoot(compares As Comparison(Of K), dir As Func(Of Boolean, Boolean, Integer)) As Func(Of ClusterKey(Of K), K, Integer)
            Return Function(cluster, key) As Integer
                       Dim compareVal As Integer = compares(cluster.Root, key)
                       Dim left As Boolean = False
                       Dim right As Boolean

                       If compareVal = 0 Then
                           Return 0
                       ElseIf compareVal = 1 Then
                           right = True
                       Else
                           left = True
                       End If

                       Return dir(left, right)
                   End Function
        End Function

        ''' <summary>
        ''' 在这里应该是多个key比较一个query
        ''' </summary>
        ''' <param name="compares"></param>
        ''' <returns></returns>
        Public Shared Function DoComparison(compares As Comparison(Of K), prefer As ComparisonDirectionPrefers, loopAll As Boolean) As Func(Of ClusterKey(Of K), K, Integer)
            Dim dir = PreferDirection(prefer)

            If Not loopAll Then
                Return JustComparesRoot(compares, dir)
            Else
                Return ComparesAll(compares, dir)
            End If
        End Function
    End Class

End Namespace