#Region "Microsoft.VisualBasic::e29877b3056274bb9eeacf5b756fd66c, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\base\Combination\Combination.vb"

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

    '   Total Lines: 60
    '    Code Lines: 46 (76.67%)
    ' Comment Lines: 4 (6.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (16.67%)
    '     File Size: 1.94 KB


    '     Class Combination
    ' 
    '         Properties: CanRun
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Execute
    '         Structure StackState
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.base

    ''' <summary>
    ''' https://github.com/coderespawn/permutations
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Friend Class Combination(Of T)

        Private data As T()
        Private selectionCount As Integer
        Private stack As New Stack(Of StackState)()

        Friend Structure StackState
            Public buffer As T()
            Public startIndex As Integer
        End Structure

        Public ReadOnly Property CanRun As Boolean
            Get
                Return stack.Count > 0
            End Get
        End Property

        Public Sub New(data As T(), selectionCount As Integer)
            Me.data = data
            Me.selectionCount = selectionCount

            stack.Push(New StackState With {
                .buffer = New T(-1) {},
                .startIndex = 0
            })
        End Sub

        Public Function Execute() As T()
            While stack.Count > 0
                Dim top = stack.Pop()

                If top.buffer.Length = selectionCount Then
                    Return top.buffer
                Else
                    Dim bufferLength = top.buffer.Length
                    Dim itemsLeft = selectionCount - bufferLength - 1
                    Dim endIndex = data.Length - itemsLeft

                    For i = top.startIndex To endIndex - 1
                        Dim nextBuffer = New List(Of T)(top.buffer)
                        nextBuffer.Add(data(i))
                        Dim nextState = New StackState With {
                            .buffer = nextBuffer.ToArray(),
                            .startIndex = i + 1
                        }
                        stack.Push(nextState)
                    Next
                End If
            End While

            Return Nothing
        End Function
    End Class
End Namespace
