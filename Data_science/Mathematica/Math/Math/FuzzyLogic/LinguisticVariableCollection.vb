#Region "Microsoft.VisualBasic::910649a9a40176fe21c4ef193910066b, Data_science\Mathematica\Math\Math\FuzzyLogic\LinguisticVariableCollection.vb"

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

    '   Total Lines: 51
    '    Code Lines: 19 (37.25%)
    ' Comment Lines: 24 (47.06%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 8 (15.69%)
    '     File Size: 1.77 KB


    '     Class LinguisticVariableCollection
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Find
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#Region "GNU Lesser General Public License"
'
'This file is part of DotFuzzy.
'
'DotFuzzy is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'DotFuzzy is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with DotFuzzy.  If not, see <http://www.gnu.org/licenses/>.
'

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents a collection of rules.
    ''' </summary>
    Public Class LinguisticVariableCollection : Inherits Dictionary(Of LinguisticVariable)

        Sub New()
            Call MyBase.New
        End Sub

#Region "Public Methods"

        ''' <summary>
        ''' Finds a linguistic variable in a collection.
        ''' </summary>
        ''' <param name="linguisticVariableName">Linguistic variable name.</param>
        ''' <returns>The linguistic variable, if founded.</returns>
        Public Overloads Function Find(linguisticVariableName As String) As LinguisticVariable
            If Me.ContainsKey(linguisticVariableName) Then
                Return Me(linguisticVariableName)
            Else
                Throw New Exception("LinguisticVariable not found: " & linguisticVariableName)
            End If
        End Function

#End Region
    End Class
End Namespace
