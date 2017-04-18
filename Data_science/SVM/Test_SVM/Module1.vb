Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.SVM
Imports Microsoft.VisualBasic.Imaging

Module Module1

    Sub Main()
        With New MainActivity
            Call .insertDefault()
            Dim result = .calculate(2)
            Dim cs = .CartesianCoordinateSystem
            With New Size(500, 500).CreateGDIDevice


                Call cs.onDraw(.Graphics, .Width, .Height)
                Call .ImageResource.SaveAs("x:\000asfsdfsd.png")
            End With
        End With
    End Sub
End Module
