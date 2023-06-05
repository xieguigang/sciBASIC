Imports System
Imports System.Collections.Generic

Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' An HDBSCAN* cluster, which will have a birth level, death level, stability, and constraint 
    ''' satisfaction once fully constructed.
    ''' </summary>
    Public Class Cluster
        Private _PropagatedLowestChildDeathLevel As Double, _Stability As Double, _HasChildren As Boolean
        Private ReadOnly _birthLevel As Double
        Private _deathLevel As Double
        Private _numPoints As Integer
        Private _propagatedStability As Double
        Private _numConstraintsSatisfied As Integer
        Private _propagatedNumConstraintsSatisfied As Integer
        Private _virtualChildCluster As SortedSet(Of Integer)

        Public ReadOnly Property PropagatedDescendants As List(Of Cluster)

        Public Property PropagatedLowestChildDeathLevel As Double
            Get
                Return _PropagatedLowestChildDeathLevel
            End Get
            Friend Set(value As Double)
                _PropagatedLowestChildDeathLevel = value
            End Set
        End Property
        Public ReadOnly Property Parent As Cluster

        Public Property Stability As Double
            Get
                Return _Stability
            End Get
            Friend Set(value As Double)
                _Stability = value
            End Set
        End Property

        Public Property HasChildren As Boolean
            Get
                Return _HasChildren
            End Get
            Friend Set(value As Boolean)
                _HasChildren = value
            End Set
        End Property
        Public ReadOnly Property Label As Integer
        Public Property HierarchyPosition As Integer    'First level where points with this cluster's label appear

        ''' <summary>
        ''' Creates a new Cluster.
        ''' </summary>
        ''' <param name="label">The cluster label, which should be globally unique</param>
        ''' <param name="parent">The cluster which split to create this cluster</param>
        ''' <param name="birthLevel">The MST edge level at which this cluster first appeared</param>
        ''' <param name="numPoints">The initial number of points in this cluster</param>
        Public Sub New(label As Integer, parent As Cluster, birthLevel As Double, numPoints As Integer)
            _birthLevel = birthLevel
            _deathLevel = 0
            _numPoints = numPoints
            _propagatedStability = 0
            _numConstraintsSatisfied = 0
            _propagatedNumConstraintsSatisfied = 0
            _virtualChildCluster = New SortedSet(Of Integer)()

            Me.Label = label
            HierarchyPosition = 0
            Stability = 0
            PropagatedLowestChildDeathLevel = Double.MaxValue
            Me.Parent = parent
            If Me.Parent IsNot Nothing Then Me.Parent.HasChildren = True
            HasChildren = False
            PropagatedDescendants = New List(Of Cluster)(1)
        End Sub

        ''' <summary>
        ''' Removes the specified number of points from this cluster at the given edge level, which will
        ''' update the stability of this cluster and potentially cause cluster death.  If cluster death
        ''' occurs, the number of constraints satisfied by the virtual child cluster will also be calculated.
        ''' </summary>
        ''' <param name="numPoints">The number of points to remove from the cluster</param>
        ''' <param name="level">The MST edge level at which to remove these points</param>
        Public Sub DetachPoints(numPoints As Integer, level As Double)
            _numPoints -= numPoints
            Stability += numPoints * (1 / level - 1 / _birthLevel)

            If _numPoints = 0 Then
                _deathLevel = level
            ElseIf _numPoints < 0 Then
                Throw New InvalidOperationException("Cluster cannot have less than 0 points.")
            End If
        End Sub

        ''' <summary>
        ''' This cluster will propagate itself to its parent if its number of satisfied constraints is
        ''' higher than the number of propagated constraints.  Otherwise, this cluster propagates its
        ''' propagated descendants.  In the case of ties, stability is examined.
        ''' Additionally, this cluster propagates the lowest death level of any of its descendants to its
        ''' parent.
        ''' </summary>
        Public Sub Propagate()
            If Parent IsNot Nothing Then
                'Propagate lowest death level of any descendants:
                If PropagatedLowestChildDeathLevel = Double.MaxValue Then PropagatedLowestChildDeathLevel = _deathLevel
                If PropagatedLowestChildDeathLevel < Parent.PropagatedLowestChildDeathLevel Then Parent.PropagatedLowestChildDeathLevel = PropagatedLowestChildDeathLevel

                'If this cluster has no children, it must propagate itself:
                If Not HasChildren Then
                    Parent._propagatedNumConstraintsSatisfied += _numConstraintsSatisfied
                    Parent._propagatedStability += Stability
                    Parent.PropagatedDescendants.Add(Me)
                ElseIf _numConstraintsSatisfied > _propagatedNumConstraintsSatisfied Then
                    Parent._propagatedNumConstraintsSatisfied += _numConstraintsSatisfied
                    Parent._propagatedStability += Stability
                    Parent.PropagatedDescendants.Add(Me)
                ElseIf _numConstraintsSatisfied < _propagatedNumConstraintsSatisfied Then
                    Parent._propagatedNumConstraintsSatisfied += _propagatedNumConstraintsSatisfied
                    Parent._propagatedStability += _propagatedStability
                    Parent.PropagatedDescendants.AddRange(PropagatedDescendants)
                ElseIf _numConstraintsSatisfied = _propagatedNumConstraintsSatisfied Then
                    'Chose the parent over descendants if there is a tie in stability:
                    If Stability >= _propagatedStability Then
                        Parent._propagatedNumConstraintsSatisfied += _numConstraintsSatisfied
                        Parent._propagatedStability += Stability
                        Parent.PropagatedDescendants.Add(Me)
                    Else
                        Parent._propagatedNumConstraintsSatisfied += _propagatedNumConstraintsSatisfied
                        Parent._propagatedStability += _propagatedStability
                        Parent.PropagatedDescendants.AddRange(PropagatedDescendants)
                    End If
                End If
            End If
        End Sub

        Public Sub AddPointsToVirtualChildCluster(points As SortedSet(Of Integer))
            For Each point In points
                _virtualChildCluster.Add(point)
            Next
        End Sub

        Public Function VirtualChildClusterConstraintsPoint(point As Integer) As Boolean
            Return _virtualChildCluster.Contains(point)
        End Function

        Public Sub AddVirtualChildConstraintsSatisfied(numConstraints As Integer)
            _propagatedNumConstraintsSatisfied += numConstraints
        End Sub

        Public Sub AddConstraintsSatisfied(numConstraints As Integer)
            _numConstraintsSatisfied += numConstraints
        End Sub

        ''' <summary>
        ''' Sets the virtual child cluster to null, thereby saving memory.  Only call this method after computing the
        ''' number of constraints satisfied by the virtual child cluster.
        ''' </summary>
        Public Sub ReleaseVirtualChildCluster()
            _virtualChildCluster = Nothing
        End Sub
    End Class
End Namespace
