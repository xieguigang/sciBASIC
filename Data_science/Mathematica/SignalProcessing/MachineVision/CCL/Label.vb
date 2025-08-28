#Region "Microsoft.VisualBasic::1266256a06af799c5205aaeabafc055e, Data_science\Mathematica\SignalProcessing\MachineVision\CCL\Label.vb"

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

    '   Total Lines: 59
    '    Code Lines: 46 (77.97%)
    ' Comment Lines: 1 (1.69%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (20.34%)
    '     File Size: 1.57 KB


    '     Class Label
    ' 
    '         Properties: Name, Rank, Root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Equals, GetHashCode, GetRoot, ToString
    ' 
    '         Sub: Join
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CCL

    Friend Class Label

        Public Property Name As Integer
        Public Property Root As Label
        Public Property Rank As Integer

        Public Sub New(Name As Integer)
            Me.Name = Name
            Root = Me
            Rank = 0
        End Sub

        Public Function GetRoot() As Label
            If Root IsNot Me Then
                Root = Root.GetRoot() ' 路径压缩
            End If
            Return Root
        End Function

        Public Sub Join(other As Label)
            Dim root1 = Me.GetRoot()
            Dim root2 = other.GetRoot()

            If root1.Name = root2.Name Then
                Return ' 已在同一集合
            End If

            ' 按秩合并
            If root1.Rank < root2.Rank Then
                root1.Root = root2
            ElseIf root1.Rank > root2.Rank Then
                root2.Root = root1
            Else
                root2.Root = root1
                root1.Rank += 1
            End If
        End Sub

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Label)

            If other Is Nothing Then
                Return False
            End If

            Return other.Name = Name
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return Name
        End Function

        Public Overrides Function ToString() As String
            Return Name.ToString()
        End Function
    End Class
End Namespace
