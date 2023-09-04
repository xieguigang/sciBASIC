Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CNN.data

    ''' <summary>
    ''' This class will hold the definitions that bridge two layers.
    ''' So you can set values in one layer and use them in the next layer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    ''' <remarks>
    ''' width, height and depth
    ''' </remarks>
    Public Class OutputDefinition

        ''' <summary>
        ''' the image width
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property outX As Integer
        ''' <summary>
        ''' the image height
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property outY As Integer
        ''' <summary>
        ''' the data depth channel, example as 3 probably stands for rgb channels
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property depth As Integer

        Sub New()
        End Sub

        Sub New(w As Integer, h As Integer, depth As Integer)
            Me.outX = w
            Me.outY = h
            Me.depth = depth
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

End Namespace
