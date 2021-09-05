
Imports System.Drawing
Imports Microsoft.VisualBasic.My.JavaScript

Namespace DBSCAN

    Public Class Point
        Public processed As Boolean = False
        Public reachabilityDistance As Double
        Public attribute As JavaScriptObject
        Public id As String
        Public color As Color
    End Class
End Namespace