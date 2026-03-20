#Region "Microsoft.VisualBasic::8918d546f07ca6a346dcd9f61c2fb19a, Data_science\DataMining\HMM\Models\Psi.vb"

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

'   Total Lines: 21
'    Code Lines: 14 (66.67%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 7 (33.33%)
'     File Size: 458 B


'     Class Psi
' 
'         Properties: index, psi
' 
'     Class TrellisPsi
' 
'         Properties: psiArrays, trellisSequence
' 
'     Class termViterbi
' 
'         Properties: maximizedProbability, psiArrays
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Models

    Public Class Psi

        Public Property psi As Integer
        Public Property index As Integer

    End Class

    Public Class TrellisPsi

        Public Property trellisSequence As Double()()
        Public Property psiArrays As PsiArray

    End Class

    Public Class termViterbi
        Public Property maximizedProbability As Double
        Public Property psiArrays As PsiArray
    End Class

    Public Class viterbiSequence
        Public Property trellisSequence As Double()()
        Public Property terminationProbability As Double
        Public Property stateSequence As String()
    End Class

    Public Class PsiArray

        Friend ReadOnly matrix As New List(Of List(Of Integer))

        Default Public ReadOnly Property Item(i As Integer) As List(Of Integer)
            Get
                Return matrix(i)
            End Get
        End Property

        Sub New(matrix As List(Of Integer)())
            Me.matrix = matrix.AsList
        End Sub

        Public Sub Add(i As Integer, data As Integer)
            matrix(i).Add(data)
        End Sub

        Public Sub Add(data As IEnumerable(Of Integer))
            matrix.Add(data.AsList)
        End Sub

        Public Sub forEach(apply As Action(Of List(Of Integer), Integer))
            Dim i As i32 = Scan0

            For Each item As List(Of Integer) In matrix
                Call apply(item, ++i)
            Next
        End Sub
    End Class
End Namespace
