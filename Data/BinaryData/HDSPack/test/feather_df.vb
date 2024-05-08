Imports Microsoft.VisualBasic.DataStorage.FeatherFormat
Imports Microsoft.VisualBasic.Math.Quantile

Module feather_df

    Sub Main222222()
        Call writeTest()
    End Sub

    Sub writeTest()
        Using writer As New FeatherWriter("./aaa.fea1", WriteMode.Eager)
            writer.AddColumn("row.names", {"1", "2", "3"})
            writer.AddColumn("x", {2, 4, 8})
            writer.AddColumn("y", {3.1, 6.2, 9.3})
        End Using
    End Sub

    Sub readTest()
        Using untyped = FeatherReader.ReadFromFile("\GCModeller\src\runtime\sciBASIC#\Data\BinaryData\Feather\examples\r-feather-test.feather", BasisType.Zero)
            Dim typed = untyped.Map(Of Boolean, Integer, Double, String, DateTimeOffset, TimeSpan, DateTime, String)()

            Pause()
        End Using
    End Sub
End Module
