Imports Microsoft.VisualBasic.Math.SignalProcessing.NDtw.Preprocessing

Namespace NDtw

    Public Class SeriesVariable
        Private ReadOnly _x As Double()
        Private ReadOnly _y As Double()
        Private ReadOnly _variableName As String
        Private ReadOnly _preprocessor As IPreprocessor
        Private ReadOnly _weight As Double

        Public Sub New(x As Double(), y As Double(), Optional variableName As String = Nothing, Optional preprocessor As IPreprocessor = Nothing, Optional weight As Double = 1)
            _x = x
            _y = y
            _variableName = variableName
            _preprocessor = preprocessor
            _weight = weight
        End Sub

        Public ReadOnly Property VariableName As String
            Get
                Return _variableName
            End Get
        End Property

        Public ReadOnly Property Weight As Double
            Get
                Return _weight
            End Get
        End Property

        Public ReadOnly Property OriginalXSeries As Double()
            Get
                Return _x
            End Get
        End Property

        Public ReadOnly Property OriginalYSeries As Double()
            Get
                Return _y
            End Get
        End Property

        Public Function GetPreprocessedXSeries() As Double()
            If _preprocessor Is Nothing Then Return _x

            Return _preprocessor.Preprocess(_x)
        End Function

        Public Function GetPreprocessedYSeries() As Double()
            If _preprocessor Is Nothing Then Return _y

            Return _preprocessor.Preprocess(_y)
        End Function
    End Class
End Namespace
