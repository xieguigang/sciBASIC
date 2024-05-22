#Region "Microsoft.VisualBasic::3d8d636568111d4680c9d14dbbdc8910, Data_science\MachineLearning\VAE\GMM\1D\Datum.vb"

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

    '   Total Lines: 75
    '    Code Lines: 56 (74.67%)
    ' Comment Lines: 4 (5.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (20.00%)
    '     File Size: 2.38 KB


    '     Class Datum
    ' 
    '         Properties: dataId, max, probs
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: getProb, getVector, ToString, val
    ' 
    '         Sub: setProb
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace GMM

    Public Class Datum

        Private m_val As ClusterEntity
        Private m_probs As Double()

        Public ReadOnly Property dataId As String
            Get
                Return m_val.uid
            End Get
        End Property

        Public ReadOnly Property probs As Double()
            Get
                Return m_probs
            End Get
        End Property

        ''' <summary>
        ''' get the cluster id of current data point most likely assign to
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property max As Integer
            Get
                Return which.Max(m_probs) + 1
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(value As Double, components As Integer, index As Integer)
            Call Me.New({value}, components, index)
        End Sub

        Sub New(value As IEnumerable(Of Double), components As Integer, index As Integer)
            Call Me.New(New ClusterEntity With {.entityVector = value.ToArray, .uid = index}, components)
        End Sub

        Sub New(value As ClusterEntity, components As Integer)
            m_val = value
            m_probs = New Double(components - 1) {}

            For i = 0 To m_probs.Length - 1
                m_probs(i) = 0.0
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getVector() As Double()
            Return m_val.entityVector
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function val() As Double
            Return m_val.entityVector.Average
        End Function

        Public Overridable Sub setProb(i As Integer, val As Double)
            m_probs(i) = val
        End Sub

        Public Overridable Function getProb(i As Integer) As Double
            Return m_probs(i)
        End Function

        Public Overrides Function ToString() As String
            Return $"#{dataId}={max},  {m_val}; probs = {probs.GetJson}"
        End Function
    End Class
End Namespace
