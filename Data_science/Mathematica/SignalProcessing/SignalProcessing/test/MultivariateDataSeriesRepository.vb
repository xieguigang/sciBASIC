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
            Private Set(ByVal value As String)
                _Title = value
            End Set
        End Property

        'values by entity and variable: Dictionary<entity, Dictionary<variable, values>>
        Private ReadOnly _valuesDict As Dictionary(Of String, Dictionary(Of String, IList(Of Double)))

        Public Sub New(ByVal title As String)
            Me.Title = title
            _valuesDict = New Dictionary(Of String, Dictionary(Of String, IList(Of Double)))()
        End Sub

        Public Sub AddValues(ByVal entity As String, ByVal variable As String, ByVal values As IList(Of Double))
            _validated = False

            If Not _valuesDict.ContainsKey(entity) Then _valuesDict(entity) = New Dictionary(Of String, IList(Of Double))()

            If _valuesDict(entity).ContainsKey(variable) Then Throw New Exception(String.Format("Values for {0}/{1} already exist.", entity, variable))

            _valuesDict(entity)(variable) = values
        End Sub

        ''' <summary>
        ''' Get values for given entity and variable.
        ''' </summary>
        Public Function GetValues(ByVal entity As String, ByVal variable As String) As IList(Of Double)
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
