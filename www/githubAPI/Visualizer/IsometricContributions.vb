Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI

Public Module IsometricContributions

    Public Function Plot(userName$, Optional schema$ = "Jet", Optional size$ = "1440,900", Optional padding$ = g.DefaultPadding, Optional bg$ = "white") As GraphicsData
        Dim contributions = userName.GetUserContributions
        Dim max% = contributions.Values.Max
        Dim colors As Color() = Designer.GetColors(schema, max)
    End Function
End Module
