﻿Imports Microsoft.VisualBasic.Serialization.JSON

Namespace CNN.data

    ''' <summary>
    ''' This class will hold the definitions that bridge two layers.
    ''' So you can set values in one layer and use them in the next layer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class OutputDefinition

        Public Overridable Property outX As Integer
        Public Overridable Property outY As Integer
        Public Overridable Property depth As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

End Namespace