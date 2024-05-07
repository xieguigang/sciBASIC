Imports Microsoft.VisualBasic.DataStorage.FeatherFormat

Module feather_df

    Sub Main222222()
        Using untyped = FeatherReader.ReadFromFile("G:\GCModeller\src\runtime\sciBASIC#\Data\BinaryData\Feather\examples\r-feather-test.feather", BasisType.Zero)
            Dim typed = untyped.Map(Of Boolean, Integer, Double, String, DateTimeOffset, TimeSpan, DateTime, String)()

            Pause()
        End Using
    End Sub
End Module
