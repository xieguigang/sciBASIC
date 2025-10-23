#Region "Microsoft.VisualBasic::da46817d009da8c2d846ea7796fbecc1, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\MultivariateDataSeriesRepository.vb"

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

    '   Total Lines: 74
    '    Code Lines: 46 (62.16%)
    ' Comment Lines: 10 (13.51%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 18 (24.32%)
    '     File Size: 2.96 KB


    '     Class MultivariateDataSeriesRepository
    ' 
    '         Properties: Title
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEntities, GetValues, GetVariables
    ' 
    '         Sub: AddValues, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace NDtw.Examples
    Public Class MultivariateDataSeriesRepository
        Private _Title As String

        Public Property Title As String
            Get
                Return _Title
            End Get
            Private Set(value As String)
                _Title = value
            End Set
        End Property

        'values by entity and variable: Dictionary<entity, Dictionary<variable, values>>
        Private ReadOnly _valuesDict As Dictionary(Of String, Dictionary(Of String, IList(Of Double)))

        Public Sub New(title As String)
            Me.Title = title
            _valuesDict = New Dictionary(Of String, Dictionary(Of String, IList(Of Double)))()
        End Sub

        Public Sub AddValues(entity As String, variable As String, values As IList(Of Double))
            _validated = False

            If Not _valuesDict.ContainsKey(entity) Then _valuesDict(entity) = New Dictionary(Of String, IList(Of Double))()

            If _valuesDict(entity).ContainsKey(variable) Then Throw New Exception(String.Format("Values for {0}/{1} already exist.", entity, variable))

            _valuesDict(entity)(variable) = values
        End Sub

        ''' <summary>
        ''' Get values for given entity and variable.
        ''' </summary>
        Public Function GetValues(entity As String, variable As String) As IList(Of Double)
            If Not _validated Then Validate()

            Return _valuesDict(entity)(variable)
        End Function

        ''' <summary>
        ''' Get all entity keys.
        ''' </summary>
        Public Function GetEntities() As IList(Of String)
            If Not _validated Then Validate()

            Return _valuesDict.Keys.Distinct().ToList()
        End Function

        ''' <summary>
        ''' Get all variable keys.
        ''' </summary>
        Public Function GetVariables() As IList(Of String)
            If Not _validated Then Validate()

            Return _valuesDict.Values.SelectMany(Function(x) x.Keys).Distinct().ToList()
        End Function

        Private _validated As Boolean
        Private Sub Validate()
            _validated = True

            If _valuesDict Is Nothing OrElse _valuesDict.Count = 0 OrElse _valuesDict.Values.Any(Function(x) x Is Nothing OrElse x.Count = 0) Then Throw New Exception(String.Format("Dataset '{0}' is invalid (not initialized or contains entities without data).", Title))

            Dim distinctVariables = _valuesDict.Values.SelectMany(Function(x) x.Keys).Distinct()

            If _valuesDict.Any(Function(x) x.Value.Count <> distinctVariables.Count()) Then Throw New Exception(String.Format("Dataset '{0}' is invalid (at least one variable missing for one of entities).", Title))
        End Sub
    End Class
End Namespace
