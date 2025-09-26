#Region "Microsoft.VisualBasic::36836e7c1139636a45284575a1f8d556, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\CharacteristicFunction.vb"

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

    '   Total Lines: 97
    '    Code Lines: 62 (63.92%)
    ' Comment Lines: 9 (9.28%)
    '    - Xml Docs: 77.78%
    ' 
    '   Blank Lines: 26 (26.80%)
    '     File Size: 3.26 KB


    '     Class CharacteristicFunction
    ' 
    '         Properties: NbPlayers
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) getValue, ToString
    ' 
    '         Sub: addDummyUser
    '         Class CharacteristicFunctionBuilder
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: addCoalition, build
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ShapleyValue

    ''' <summary>
    ''' represent the following function often written by v
    ''' 2^N to R
    ''' for each subset of the set {1.. N} is associated a value
    ''' per convention v(empty set) = 0
    ''' 
    ''' @author Franck Benault
    ''' 
    ''' </summary>
    Public Class CharacteristicFunction

        Dim v As IDictionary(Of ISet(Of Integer), Double)

        Public Overridable ReadOnly Property NbPlayers As Integer

        Private Sub New(builder As CharacteristicFunctionBuilder)
            NbPlayers = builder.nbPlayers
            v = New Dictionary(Of ISet(Of Integer), Double)()
            v = builder.v
        End Sub

        Public Overridable Function getValue(ParamArray coalition As Integer?()) As Double
            Dim coalitionSet As ISet(Of Integer) = New HashSet(Of Integer)()

            For Each player As Integer In coalition
                coalitionSet.Add(player)
            Next

            Return v(coalitionSet)
        End Function

        Public Overridable Function getValue(coalitionSet As ISet(Of Integer)) As Double
            Return v(coalitionSet)
        End Function

        Public Overrides Function ToString() As String
            Return "CharacteristicFunction [nbPlayers=" & NbPlayers.ToString() & ", v=" & v.ToString() & "]"
        End Function

        Public Class CharacteristicFunctionBuilder
            Friend ReadOnly nbPlayers As Integer
            Friend v As IDictionary(Of ISet(Of Integer), Double)

            Public Sub New(nbPlayers As Integer)
                Me.nbPlayers = nbPlayers
                v = New Dictionary(Of ISet(Of Integer), Double)()
            End Sub

            Public Overridable Function addCoalition(value As Double, ParamArray coalition As Integer?()) As CharacteristicFunctionBuilder
                Dim [set] As ISet(Of Integer) = New HashSet(Of Integer)()
                For Each player As Integer In coalition
                    [set].Add(player)
                Next
                v([set]) = value
                Return Me
            End Function

            Public Overridable Function build() As CharacteristicFunction
                Return New CharacteristicFunction(Me)
            End Function

        End Class

        Public Overridable Sub addDummyUser()
            Dim newV As IDictionary(Of ISet(Of Integer), Double) = New Dictionary(Of ISet(Of Integer), Double)()


            Dim coalitions = v.Keys
            Dim coalitionSet As HashSet(Of Integer)

            For Each coalition In coalitions

                coalitionSet = New HashSet(Of Integer)()
                coalitionSet.AddAll(coalition)
                coalitionSet.Add(NbPlayers + 1)

                newV(coalitionSet) = v(coalition)
            Next

            coalitionSet = New HashSet(Of Integer)()
            coalitionSet.Add(NbPlayers + 1)
            v(coalitionSet) = 0.0

            For Each coalition In newV.Keys
                v(coalition) = newV(coalition)
            Next

            _NbPlayers += 1
        End Sub

    End Class

End Namespace

