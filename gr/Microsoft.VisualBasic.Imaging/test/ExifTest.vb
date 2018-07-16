Imports Microsoft.VisualBasic.Imaging.Driver

Module ExifTest

    Sub Main()
        Dim adapter As New JpegMetadataAdapter("D:\smartnucl_integrative\dist\urine\biodeepMSMS\neg\plot\biodeepMSMS\85.0281@75_1,4-Butynediol.jpg")

        adapter.Metadata.Title = "Beach"
        adapter.Metadata.Subject = "Summer holiday 2014"
        adapter.Metadata.Rating = 4
        adapter.Metadata.Keywords.Add("beach")
        adapter.Metadata.Comments = "This is a comment."

        Console.WriteLine(adapter.Save())

        Pause()
    End Sub
End Module
