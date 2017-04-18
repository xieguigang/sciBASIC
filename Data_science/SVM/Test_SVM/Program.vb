Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.SVM
Imports Microsoft.VisualBasic.DataMining.SVM.Model
Imports Microsoft.VisualBasic.Imaging

Module Program

    Sub Main()
        Dim cs = InsertDefault()
        Call cs.Calculate(Optimizers.GradientDescent)

        With New Size(650, 500).CreateGDIDevice
            Call cs.Draw(.Graphics, .Width, .Height)
            Call .ImageResource _
                 .SaveAs("../../../SVM-plot.png")
        End With
    End Sub

    Function InsertDefault() As CartesianCoordinateSystem
        Dim toAdd As New List(Of LabeledPoint)
        toAdd.Add(New LabeledPoint(0.4, 0.4, ColorClass.RED))
        toAdd.Add(New LabeledPoint(0.7, 0.6, ColorClass.RED))
        toAdd.Add(New LabeledPoint(0.2, 0.6, ColorClass.BLUE))
        toAdd.Add(New LabeledPoint(0.4, 0.9, ColorClass.BLUE))
        toAdd.Add(New LabeledPoint(1, 1, ColorClass.BLUE))
        toAdd.Add(New LabeledPoint(1, 0.75, ColorClass.BLUE))
        toAdd.Add(New LabeledPoint(0.6, 0.59, ColorClass.BLUE))
        toAdd.Add(New LabeledPoint(0.14, 0.5, ColorClass.RED))

        Dim line As New Line(0, 0.4, 1, 0.9)
        Dim cartesianCoordinateSystem As New CartesianCoordinateSystem(points:=toAdd) With {
            .Line = line
        }

        Return cartesianCoordinateSystem
    End Function
End Module
