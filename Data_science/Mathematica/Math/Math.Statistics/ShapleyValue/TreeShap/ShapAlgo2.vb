#Region "Microsoft.VisualBasic::764afd76ebe148f00dc0fdeae0ad22e2, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\TreeShap\ShapAlgo2.vb"

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

    '   Total Lines: 206
    '    Code Lines: 141 (68.45%)
    ' Comment Lines: 33 (16.02%)
    '    - Xml Docs: 69.70%
    ' 
    '   Blank Lines: 32 (15.53%)
    '     File Size: 8.55 KB


    '     Class ShapAlgo2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: compute, copy, extend, findFirst, sumWeight
    '                   unwind, unwindTracked
    ' 
    '         Sub: recurse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ShapleyValue.TreeShape

    ''' <summary>
    ''' Rewrite of the Algorithm 2 from [1].
    ''' 
    ''' The main goal here is to get "fanatically faithful" transcript of the algo into java.
    ''' Therefore, performance and memory optimizations are neither done nor planned here.
    ''' 
    ''' Deviations from original notation:
    ''' - java list/arrays are zero-based
    ''' - tree is represented by recursive structure of nodes, each is either a leaf or a split node with yes, no, and other props
    ''' - the path (stored in variable "m") has long-named properties to make the thing more readable
    ''' 
    ''' Bugfixes:
    ''' - [bugfix1] in EXTEND, iteration misses newly added element, causing wrong computation - see [2]
    ''' - [bugfix2] in UNWIND, iteration exceeds into removed element, causing exception
    ''' 
    ''' Resources:
    ''' - [1] Consistent Individualized Feature Attribution for Tree Ensembles - https://arxiv.org/pdf/1802.03888.pdf
    ''' - [2] From local explanations to global understanding with explainable AI for trees, pages 65,66 - https://www.nature.com/articles/s42256-019-0138-9.epdf?shared_access_token=RCYPTVkiECUmc0CccSMgXtRgN0jAjWel9jnR3ZoTv0O81kV8DqPb2VXSseRmof0Pl8YSOZy4FHz5vMc3xsxcX6uT10EzEoWo7B-nZQAHJJvBYhQJTT1LnJmpsa48nlgUWrMkThFrEIvZstjQ7Xdc5g%3D%3D
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/pkozelka/treeshap
    ''' </remarks>
    Public Class ShapAlgo2
        Friend Shared DEBUG As Boolean = True
        Private indent As String = ""

        Private ReadOnly phi As Double()
        Private ReadOnly x As Double()

        Public Shared Function compute(x As Double(), tree As PkTree) As Double()
            Dim shap As New ShapAlgo2(x)
            shap.recurse(tree.root, New List(Of PathElement)(), 1.0, 1.0, Nothing)
            Return shap.phi
        End Function

        Private Sub New(x As Double())
            Me.x = x
            phi = New Double(x.Length - 1) {}
        End Sub

        Private Sub recurse(j As PkNode, m As IList(Of PathElement), pz As Double, po As Double, pi As Integer)
            If DEBUG Then
                Console.Write("{0}recurse(DC={1}, P0f={2:F}, P1f={3:F}, PFi={4:D})%n", indent, j.dataCount, pz, po, pi)
            End If
            m = extend(m, pz, po, pi)
            If j.LeafProp Then
                For i = 1 To m.Count - 1 - 1 ' note that we are skipping first element
                    Dim w = sumWeight(unwind(m, i))

                    Dim mi = m(i)
                    Dim contrib = w * (mi.oneFraction - mi.zeroFraction) * j.leafValue
                    If DEBUG Then
                        Console.Write("{0}* phi[{1,2:D}] += {2:F} ... w = {3:F}%n", indent, mi.featureIndex, contrib, w)
                    End If
                    phi(mi.featureIndex) += contrib
                Next
            Else

                Dim isHot = x(j.splitFeatureIndex) <= j.splitValue ' evaluate the decision

                Dim h = If(isHot, j.yes, j.no)

                Dim c = If(isHot, j.no, j.yes)
                Dim iz As Double = 1
                Dim io As Double = 1

                Dim k = findFirst(m, j.splitFeatureIndex)
                If k > -1 Then
                    Dim mk = m(k)
                    iz = mk.zeroFraction
                    io = mk.oneFraction
                    m = unwindTracked(m, k)
                End If

                Dim indentBackup = indent
                indent += "    "
                recurse(h, m, iz * h.dataCount / j.dataCount, io, j.splitFeatureIndex)
                recurse(c, m, iz * c.dataCount / j.dataCount, 0, j.splitFeatureIndex)
                indent = indentBackup
            End If
        End Sub

        Private Function findFirst(m As IList(Of PathElement), splitFeatureIndex As Integer) As Integer
            Dim index = 0
            For Each e As PathElement In m
                If e.featureIndex > -1 AndAlso e.featureIndex = splitFeatureIndex Then
                    Return index
                End If
                index += 1
            Next
            Return -1
        End Function

        Private Function extend(origM As IList(Of PathElement), pz As Double, po As Double, pi As Integer) As IList(Of PathElement)
            If DEBUG Then
                Console.Write("{0}(+) 0f={1:F}, 1f={2:F}, Fi={3:D}  -->  ", indent, pz, po, pi)
            End If
            ' copy original
            Dim m = copy(origM)
            ' add new item
            Dim e As New PathElement()
            e.featureIndex = pi
            e.zeroFraction = pz
            e.oneFraction = po
            e.weight = If(origM.Count = 0, 1.0, 0.0)
            m.Add(e)

            Dim sz = m.Count ' use "sz" rather than "l" which is optically too similar to "1" (one)
            ' distribute weights; note that the iteration works with "old" sz
            For i = sz - 2 To 0 Step -1 ' <-- [bugfix1] ... partly reverted, by assigning sz *after* new element is added

                Dim mi1 = m(i + 1)
                Dim mi = m(i)

                Dim ii = i + 1 ' this would be the index in one-based array

                Dim d = ii / sz
                mi1.weight = mi1.weight + po * mi.weight * d

                Dim dd = (sz - ii) / sz
                mi.weight = mi.weight * (pz * dd)
            Next
            If DEBUG Then
                Console.Write("({0:D}){1}%n", m.Count, m)
            End If
            Return m
        End Function

        ''' <summary>
        ''' Removes i-th element of the path, and redistributes all weights </summary>
        ''' <param name="origM"> elements of decision path </param>
        ''' <param name="i">  zero-based index to be removed </param>
        ''' <returns> transformed decision path </returns>
        Private Shared Function unwind(origM As IList(Of PathElement), i As Integer) As IList(Of PathElement)
            Dim sz = origM.Count ' use "sz" rather than "l" which is optically too similar to "1" (one)
            Dim n = origM(sz - 1).weight ' last weight
            ' copy elements from original, all but the last one

            Dim m = copy(origM)
            m.RemoveAt(m.Count - 1)
            ' redistribute weights

            Dim mi = m(i)
            For j = sz - 2 To 1 Step -1

                Dim mj = m(j)

                Dim jj = j + 1 ' this would be the index in one-based array
                If mi.oneFraction <> 0.0 Then

                    Dim t = mj.weight
                    mj.weight = n * sz / (jj * mi.oneFraction)

                    Dim dd = (sz - jj) / sz
                    n = t - mj.weight * mi.zeroFraction * dd
                Else

                    Dim d As Double = sz - jj
                    mj.weight = mj.weight * sz / (mj.zeroFraction * d)
                End If
            Next
            ' shift other attributes
            For j = i To sz - 2 - 1 ' <-- [bugfix2]

                Dim mj = m(j)
                Dim mj1 = m(j + 1)
                mj.featureIndex = mj1.featureIndex
                mj.zeroFraction = mj1.zeroFraction
                mj.oneFraction = mj1.oneFraction
            Next
            Return m
        End Function

        Private Function unwindTracked(origM As IList(Of PathElement), i As Integer) As IList(Of PathElement)
            If DEBUG Then
                Console.Write("{0}(-) Pi={1:D}  -->  ", indent, i)
            End If

            Dim m = unwind(origM, i)
            If DEBUG Then
                Console.Write("({0:D}){1}%n", m.Count, m)
            End If
            Return m
        End Function

        Private Shared Function copy(origM As IList(Of PathElement)) As IList(Of PathElement)
            Dim m As New List(Of PathElement)()
            For Each e As PathElement In origM
                Dim cloned As New PathElement()
                cloned.featureIndex = e.featureIndex
                cloned.zeroFraction = e.zeroFraction
                cloned.oneFraction = e.oneFraction
                cloned.weight = e.weight
                m.Add(cloned)
            Next
            Return m
        End Function

        Private Shared Function sumWeight(m As IList(Of PathElement)) As Double
            Return Aggregate e As PathElement In m Into Sum(e.weight)
        End Function
    End Class

End Namespace
