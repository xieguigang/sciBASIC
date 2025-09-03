Imports System.Text
Imports std = System.Math

Namespace ShapleyValue.TreeShape

    Public Class ShapOptimized

        Friend Shared DEBUG As Boolean = True
        Private indent As String = ""

        Private ReadOnly data As Double()
        Private ReadOnly tree As PkTree
        Private ReadOnly expectedValue As Double
        Private ReadOnly pathArrayCapacity As Integer

        ''' <summary>
        ''' temporary variables, for use inside recursion - minimizing number of passed parameters
        ''' </summary>
        Private pathArray As IList(Of PathElement)
        Private phi As Double()


        Public Sub New(data As Double(), tree As PkTree, expectedValue As Double)
            Me.data = data
            Me.tree = tree
            Me.expectedValue = expectedValue
            Dim maxd = maxDepth(tree.root) + 2
            pathArrayCapacity = maxd * (maxd + 1) / 2
        End Sub

        Private Shared Function maxDepth(node As PkNode) As Integer
            If node.LeafProp Then
                Return 0
            End If
            Return std.Max(maxDepth(node.yes) + 1, maxDepth(node.no) + 1)
        End Function

        Private Shared Function createPathArray(capacity As Integer) As List(Of PathElement)
            Dim pathArray As List(Of PathElement) = New List(Of PathElement)(capacity)
            ' note that element at index [0] seems to never be accessed (we would get NPE otherwise)
            Dim boundary = 0
            Dim cnt = 0
            For i = 0 To capacity - 1
                If cnt = boundary Then
                    ' security measure - these bounds should never be touched!
                    Dim separator As PathElement = New PathElement()
                    separator.featureIndex = -888
                    '                final PathElement separator = null;
                    pathArray.Add(separator)
                    cnt = 0
                    boundary += 1
                Else
                    pathArray.Add(New PathElement())
                    cnt += 1
                End If
            Next
            Return pathArray
        End Function

        Private Sub treeShap(node As PkNode, uniqueDepth As Integer, parentUniquePathPtr As Integer, parentZeroFraction As Double, parentOneFraction As Double, parentFeatureIndex As Integer, conditionFraction As Double)
            ' stop if we have no weight coming down to us
            If conditionFraction = 0 Then
                Return
            End If

            ' extend the unique path
            Dim uniquePathPtr = parentUniquePathPtr + uniqueDepth + 1
            If DEBUG Then
                Console.Write("{0}recurse(DC={1}, P0f={2:F}, P1f={3:F}, PFi={4:D})%n", indent, node.dataCount, parentZeroFraction, parentOneFraction, If(parentFeatureIndex < 0, Nothing, parentFeatureIndex))
            End If
            stdcopy(parentUniquePathPtr, parentUniquePathPtr + uniqueDepth, uniquePathPtr) 'TODO this +1 in second param looks to be AT LEAST useless, MAYBE even harmful. Explore when tests are working!
            extendPath(uniquePathPtr, uniqueDepth, parentZeroFraction, parentOneFraction, parentFeatureIndex)

            If node.LeafProp Then
                ' leaf node
                Dim i = 1

                While i <= uniqueDepth
                    Dim w = unwoundPathSum(uniquePathPtr, uniqueDepth, i)
                    Dim el = pathArray(uniquePathPtr + i)
                    Dim contrib = w * (el.oneFraction - el.zeroFraction) * node.leafValue * conditionFraction
                    If DEBUG Then
                        Console.Write("{0}* phi[{1,2:D}] += {2:F} ... w = {3:F}%n", indent, el.featureIndex, contrib, w)
                    End If
                    phi(el.featureIndex) += contrib
                    Threading.Interlocked.Increment(i)
                End While
            Else
                ' internal node
                Dim featureValue = data(node.splitFeatureIndex)
                Dim decision = featureValue <= node.splitValue
                Dim hotNode = If(decision, node.yes, node.no)
                Dim coldNode = If(decision, node.no, node.yes)
                Dim hotZeroFraction = hotNode.dataCount / node.dataCount
                Dim coldZeroFraction = coldNode.dataCount / node.dataCount
                Dim incomingZeroFraction As Double = 1
                Dim incomingOneFraction As Double = 1

                ' see if we have already split on this feature,
                Dim pathIndex = findFeatureSplit(uniquePathPtr, uniqueDepth, node.splitFeatureIndex)
                If pathIndex IsNot Nothing Then
                    ' if so we undo that split so we can redo it for this node
                    Dim el = pathArray(uniquePathPtr + pathIndex.Value)
                    incomingZeroFraction = el.zeroFraction
                    incomingOneFraction = el.oneFraction
                    unwindPath(uniquePathPtr, uniqueDepth, pathIndex.Value)
                    uniqueDepth -= 1
                End If

                Dim indentBackup = indent
                indent += "    "
                treeShap(hotNode, uniqueDepth + 1, uniquePathPtr, hotZeroFraction * incomingZeroFraction, incomingOneFraction, node.splitFeatureIndex, conditionFraction)

                treeShap(coldNode, uniqueDepth + 1, uniquePathPtr, coldZeroFraction * incomingZeroFraction, 0, node.splitFeatureIndex, conditionFraction)
                indent = indentBackup
            End If
        End Sub

        Private Function findFeatureSplit(uniquePathPtr As Integer, uniqueDepth As Integer, splitIndex As Integer) As Integer?
            Dim pathIndex = 0
            While pathIndex <= uniqueDepth
                Dim el = pathArray(uniquePathPtr + pathIndex)
                If el.featureIndex = splitIndex Then
                    Return pathIndex
                End If

                Threading.Interlocked.Increment(pathIndex)
            End While
            Return Nothing
        End Function

        Public Overridable Function calculateContributions() As Double()
            Dim icols = data.Length
            Dim ncolumns = icols + 1

            Dim contributions = New Double(ncolumns - 1) {}
            contributions(icols) += expectedValue
            pathArray = createPathArray(pathArrayCapacity)
            phi = contributions
            treeShap(tree.root, 0, 0, 1, 1, -1, 1)
            Return contributions
        End Function

        ''' <summary>
        ''' Ala C++ std::copy - see http://www.cplusplus.com/reference/algorithm/copy/
        ''' (Copies the elements in the range [first,last) into the range beginning at result.)
        ''' </summary>
        Private Sub stdcopy(first As Integer, last As Integer, result As Integer)
            While first <> last
                Dim src = pathArray(first)
                Dim dest = pathArray(result)
                dest.featureIndex = src.featureIndex
                dest.zeroFraction = src.zeroFraction
                dest.oneFraction = src.oneFraction
                dest.pweight = src.pweight
                Threading.Interlocked.Increment(result)
                Threading.Interlocked.Increment(first)
            End While
        End Sub

        ''' <summary>
        ''' extend our decision path with a fraction of one and zero extensions
        ''' </summary>
        ''' <param name="uniquePathPtr"></param>
        ''' <param name="uniqueDepth"></param>
        ''' <param name="zeroFraction"></param>
        ''' <param name="oneFraction"></param>
        ''' <param name="featureIndex"></param>
        Private Sub extendPath(uniquePathPtr As Integer, uniqueDepth As Integer, zeroFraction As Double, oneFraction As Double, featureIndex As Integer)
            If DEBUG Then
                Console.Write("{0}(+) 0f={1:F}, 1f={2:F}, Fi={3:D}  -->  ", indent, zeroFraction, oneFraction, If(featureIndex < 0, Nothing, featureIndex))
            End If
            Dim el = pathArray(uniquePathPtr + uniqueDepth)
            el.featureIndex = featureIndex
            el.zeroFraction = zeroFraction
            el.oneFraction = oneFraction
            el.pweight = If(uniqueDepth = 0, 1.0, 0.0)
            For i = uniqueDepth - 1 To 0 Step -1
                Dim upi1 = pathArray(uniquePathPtr + i + 1)
                Dim upi = pathArray(uniquePathPtr + i)
                upi1.pweight += oneFraction * upi.pweight * (i + 1) / (uniqueDepth + 1)
                upi.pweight = zeroFraction * upi.pweight * (uniqueDepth - i) / (uniqueDepth + 1)
            Next
            If DEBUG Then
                Console.WriteLine(track(uniquePathPtr, uniqueDepth + 1))
            End If
        End Sub

        ''' <summary>
        ''' undo a previous extension of the decision path
        ''' </summary>
        ''' <param name="uniquePathPtr"></param>
        ''' <param name="uniqueDepth"></param>
        ''' <param name="pathIndex"></param>
        Private Sub unwindPath(uniquePathPtr As Integer, uniqueDepth As Integer, pathIndex As Integer)
            If DEBUG Then
                Console.Write("{0}(-) Pi={1:D}  -->  ", indent, pathIndex)
            End If
            Dim oneFraction = pathArray(uniquePathPtr + pathIndex).oneFraction
            Dim zeroFraction = pathArray(uniquePathPtr + pathIndex).zeroFraction
            Dim nextOnePortion = pathArray(uniquePathPtr + uniqueDepth).pweight

            Dim i = uniqueDepth - 1

            While i >= 0
                Dim el = pathArray(uniquePathPtr + i)
                Dim d = (uniqueDepth - i) / (uniqueDepth + 1)
                If oneFraction <> 0 Then
                    Dim tmp = el.pweight
                    el.pweight = nextOnePortion * (uniqueDepth + 1) / ((i + 1) * oneFraction)
                    nextOnePortion = tmp - el.pweight * zeroFraction * d
                Else
                    el.pweight = el.pweight / zeroFraction / d
                End If

                Threading.Interlocked.Decrement(i)
            End While

            i = pathIndex

            While i < uniqueDepth
                Dim el = pathArray(uniquePathPtr + i)
                Dim el1 = pathArray(uniquePathPtr + i + 1)
                el.featureIndex = el1.featureIndex
                el.zeroFraction = el1.zeroFraction
                el.oneFraction = el1.oneFraction
                Threading.Interlocked.Increment(i)
            End While
            If DEBUG Then
                Console.WriteLine(track(uniquePathPtr, uniqueDepth - 1))
            End If
        End Sub

        ''' <summary>
        ''' determine what the total permutation weight would be if
        ''' we unwound a previous extension in the decision path
        ''' </summary>
        ''' <param name="uniquePathPtr"></param>
        ''' <param name="uniqueDepth"></param>
        ''' <param name="pathIndex"></param>
        ''' <returns></returns>
        Private Function unwoundPathSum(uniquePathPtr As Integer, uniqueDepth As Integer, pathIndex As Integer) As Double
            Dim oneFraction = pathArray(uniquePathPtr + pathIndex).oneFraction
            Dim zeroFraction = pathArray(uniquePathPtr + pathIndex).zeroFraction
            Dim nextOnePortion = pathArray(uniquePathPtr + uniqueDepth).pweight
            Dim total As Double = 0
            Dim i = uniqueDepth - 1

            While i >= 0
                Dim el = pathArray(uniquePathPtr + i)
                Dim tmp As Double
                Dim d = (uniqueDepth - i) / (uniqueDepth + 1)
                If oneFraction <> 0 Then
                    tmp = nextOnePortion * (uniqueDepth + 1) / ((i + 1) * oneFraction)
                    nextOnePortion = el.pweight - tmp * zeroFraction * d
                Else
                    tmp = el.pweight / zeroFraction / d
                End If
                total += tmp
                Threading.Interlocked.Decrement(i)
            End While
            Return total
        End Function

        Private Function track(uniquePathPtr As Integer, uniqueDepth As Integer) As String
            Dim sb As StringBuilder = New StringBuilder()
            For i = 0 To uniqueDepth - 1
                If sb.Length > 0 Then
                    sb.Append(", ")
                End If
                sb.Append(pathArray(uniquePathPtr + i).toStringComparable())
            Next
            Return String.Format("({0:D})[{1}]", uniqueDepth, sb)
        End Function

        ' 
        ' 		 data we keep about our decision path
        ' 		 note that pweight is included for convenience and is not tied with the other
        ' 		 attributes the pweight of the i'th path element is the permutation weight of
        ' 		 paths with i-1 ones in them
        ' 		
        Friend Class PathElement

            Public featureIndex As Integer = -777
            Public zeroFraction As Double = -777.777
            Public oneFraction As Double = -777.777
            Public pweight As Double = -777.777

            Public Overridable Function toStringComparable() As String
                Return "PE{f=" & If(featureIndex < 0, Nothing, featureIndex).ToString() & ", zf=" & zeroFraction.ToString() & ", of=" & oneFraction.ToString() & ", pw=" & pweight.ToString() & "}"c.ToString()
            End Function

            Public Overrides Function ToString() As String
                If featureIndex = -888 Then
                    Return "- - - -"
                End If
                Return "PE{" & (If(featureIndex = -777, "-", "f=" & featureIndex.ToString() & ", zf=" & zeroFraction.ToString() & ", of=" & oneFraction.ToString() & ", pw=" & pweight.ToString())) & "}"c.ToString()
            End Function
        End Class
    End Class

End Namespace
