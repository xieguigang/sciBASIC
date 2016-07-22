#Region "Microsoft.VisualBasic::acab08b1f08c89564e1b02dcb67ddde6, ..\Microsoft.VisualBasic.Architecture.Framework\Text\StringSimilarity\BipartiteMatcher.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Text.Similarity

    '
    'Maximize the total weight of bipartite grapth 
    'Author: Thanh Ngoc Dao - Thanh.dao@gmx.net
    'Copyright (c) 2005 by Thanh Ngoc Dao.
    '

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    Public Class BipartiteMatcher
        Private _leftTokens As String(), _rightTokens As String()
        Private _cost As Single(,)
        Private leftLabel As Single(), rightLabel As Single()
        Private _previous As Integer(), _incomming As Integer(), _outgoing As Integer()
        'connect with the left and right
        Private _leftVisited As Boolean(), _rightVisited As Boolean()
        Private leftLen As Integer, rightLen As Integer
        Private _errorOccured As Boolean = False

        Public Sub New(left As String(), right As String(), cost As Single(,))
            If left Is Nothing OrElse right Is Nothing OrElse cost Is Nothing Then
                _errorOccured = True
                Return
            End If

            _leftTokens = left
            _rightTokens = right
            'swap
            If _leftTokens.Length > _rightTokens.Length Then
                Dim tmpCost As Single(,) = New Single(_rightTokens.Length - 1, _leftTokens.Length - 1) {}
                For i As Integer = 0 To _rightTokens.Length - 1
                    For j As Integer = 0 To _leftTokens.Length - 1
                        tmpCost(i, j) = cost(j, i)
                    Next
                Next

                _cost = DirectCast(tmpCost.Clone(), Single(,))

                Dim tmp As String() = _leftTokens
                _leftTokens = _rightTokens
                _rightTokens = tmp
            Else
                _cost = DirectCast(cost.Clone(), Single(,))
            End If


            MyInit()

            Make_Matching()
        End Sub

        Private Sub MyInit()
            Initialize()

            _leftVisited = New Boolean(leftLen) {}
            _rightVisited = New Boolean(rightLen) {}
            _previous = New Integer((leftLen + rightLen) + 1) {}

        End Sub

        Private Sub Initialize()
            leftLen = _leftTokens.Length - 1
            rightLen = _rightTokens.Length - 1

            leftLabel = New Single(leftLen) {}
            rightLabel = New Single(rightLen) {}
            For i As Integer = 0 To leftLabel.Length - 1
                leftLabel(i) = 0
            Next
            For i As Integer = 0 To rightLabel.Length - 1
                rightLabel(i) = 0
            Next

            'init distance
            For i As Integer = 0 To leftLen
                Dim maxLeft As Single = Single.MinValue
                For j As Integer = 0 To rightLen
                    If _cost(i, j) > maxLeft Then
                        maxLeft = _cost(i, j)
                    End If
                Next

                leftLabel(i) = maxLeft
            Next

        End Sub

        Private Sub Flush()
            For i As Integer = 0 To _previous.Length - 1
                _previous(i) = -1
            Next
            For i As Integer = 0 To _leftVisited.Length - 1
                _leftVisited(i) = False
            Next
            For i As Integer = 0 To _rightVisited.Length - 1
                _rightVisited(i) = False
            Next
        End Sub

        Private [stop] As Boolean = False
        Private Function FindPath(source As Integer) As Boolean
            Flush()
            [stop] = False
            Walk(source)
            Return [stop]

        End Function

        Private Sub Increase_Matchs(li As Integer, lj As Integer)
            Dim tmpOut As Integer() = DirectCast(_outgoing.Clone(), Integer())
            Dim i As Integer, j As Integer, k As Integer
            i = li
            j = lj
            _outgoing(i) = j
            _incomming(j) = i
            If _previous(i) <> -1 Then
                Do
                    j = tmpOut(i)
                    k = _previous(i)
                    _outgoing(k) = j
                    _incomming(j) = k
                    i = k
                Loop While _previous(i) <> -1
            End If
        End Sub


        Private Sub Walk(i As Integer)
            _leftVisited(i) = True

            For j As Integer = 0 To rightLen
                If [stop] Then
                    Return
                ElseIf Not _rightVisited(j) AndAlso (leftLabel(i) + rightLabel(j) = _cost(i, j)) Then
                    If _incomming(j) = -1 Then
                        ' if found a path
                        [stop] = True
                        Increase_Matchs(i, j)
                        Return
                    Else
                        Dim k As Integer = _incomming(j)
                        _rightVisited(j) = True
                        _previous(k) = i
                        Walk(k)
                    End If
                End If
            Next
        End Sub

#Region "BreadFirst"
        '		int FindPath(int source)
        '		{
        '			int head, tail, idxHead=0;
        '			int[] visited=new int[(leftLen+rightLen)+2] , 
        '				q=new int[(leftLen+rightLen)+2] ;
        '			head=0;
        '			for (int i=0; i < visited.Length; i++) visited[i]=0;
        '			Flush ();
        '								
        '			head=-1;
        '			tail=0;
        '			q[tail]=source;
        '			visited[source]=1;
        '			leftVisited[source]=true;
        '			int nMerge=leftLen + rightLen + 1;
        '
        '			while (head <= tail)
        '			{
        '				++head;
        '				idxHead=q[head];
        '
        '
        '				for (int j=0; j <= (leftLen + rightLen + 1); j++)
        '					if(visited[j] == 0)
        '				{
        '					if (j > leftLen) //j is stay at the RightSide
        '					{
        '						int idxRight=j - (leftLen + 1);
        '						if (idxHead <= leftLen &&  (leftLabel[idxHead] + rightLabel[idxRight] == cost[idxHead, idxRight]))
        '						{
        '							++tail;
        '							q[tail]=j;
        '							visited[j]=1;
        '							previous[j]=idxHead;
        '							rightVisited[idxRight]=true;
        '							if (In[idxRight] == -1) // pretty good, found a path															
        '								return j;		
        '							
        '						}
        '					}
        '					else if ( j <= leftLen) // is stay at the left
        '					{
        '						if (idxHead > leftLen && In[idxHead - (leftLen + 1)] == j)
        '						{
        '							++tail;
        '							q[tail]=j;
        '							visited[j]=1;
        '							previous[j]=idxHead;
        '							leftVisited[j]=true;
        '						}
        '					}
        '				}
        '			}
        '
        '			return -1;//not found
        '		}
        '
        '		void Increase_Matchs(int j)
        '		{			
        '			if (previous [j] != -1)
        '				do
        '				{
        '					int i=previous[j];
        '					Out[i]=j-(leftLen + 1);
        '					In[j-(leftLen + 1)]=i;
        '					j=previous[i];
        '				} while ( j != -1);
        '		}
        '

#End Region

        Private Function GetMinDeviation() As Single
            Dim min As Single = Single.MaxValue

            For i As Integer = 0 To leftLen
                If _leftVisited(i) Then
                    For j As Integer = 0 To rightLen
                        If Not _rightVisited(j) Then
                            If leftLabel(i) + rightLabel(j) - _cost(i, j) < min Then
                                min = (leftLabel(i) + rightLabel(j)) - _cost(i, j)
                            End If
                        End If
                    Next
                End If
            Next

            Return min
        End Function

        Private Sub Relabels()
            Dim dev As Single = GetMinDeviation()

            For k As Integer = 0 To leftLen
                If _leftVisited(k) Then
                    leftLabel(k) -= dev
                End If
            Next

            For k As Integer = 0 To rightLen
                If _rightVisited(k) Then
                    rightLabel(k) += dev
                End If
            Next
        End Sub

        Private Sub Make_Matching()
            _outgoing = New Integer(leftLen) {}
            _incomming = New Integer(rightLen) {}
            For i As Integer = 0 To _outgoing.Length - 1
                _outgoing(i) = -1
            Next
            For i As Integer = 0 To _incomming.Length - 1
                _incomming(i) = -1
            Next

            For k As Integer = 0 To leftLen
                If _outgoing(k) = -1 Then
                    Dim found As Boolean = False
                    Do
                        found = FindPath(k)
                        If Not found Then
                            Relabels()

                        End If
                    Loop While Not found
                End If
            Next
        End Sub


        Private Function GetTotal() As Single
            Dim nTotal As Single = 0
            Dim nA As Single = 0
            Trace.Flush()
            For i As Integer = 0 To leftLen
                If _outgoing(i) <> -1 Then
                    nTotal += _cost(i, _outgoing(i))
                    Trace.WriteLine(_leftTokens(i) & " <-> " & _rightTokens(_outgoing(i)) & " : " & _cost(i, _outgoing(i)))
                    Dim a As Single = If(1.0F - System.Math.Max(_leftTokens(i).Length, _rightTokens(_outgoing(i)).Length) <> 0, _cost(i, _outgoing(i)) / System.Math.Max(_leftTokens(i).Length, _rightTokens(_outgoing(i)).Length), 1)
                    nA += a
                End If
            Next
            Return nTotal
        End Function

        Public Function GetScore() As Single
            Dim dis As Single = GetTotal()

            Dim maxLen As Single = rightLen + 1
            Dim l1 As Integer = 0
            Dim l2 As Integer = 0
            For Each s As String In _rightTokens
                l1 += s.Length
            Next
            For Each s As String In _leftTokens
                l2 += s.Length
            Next
            maxLen = Math.Max(l1, l2)

            If maxLen > 0 Then
                Return dis / maxLen
            Else
                Return 1.0F
            End If
        End Function


        Public ReadOnly Property Score() As Single
            Get
                If _errorOccured Then
                    Return 0
                Else
                    Return GetScore()
                End If
            End Get
        End Property

    End Class
End Namespace
