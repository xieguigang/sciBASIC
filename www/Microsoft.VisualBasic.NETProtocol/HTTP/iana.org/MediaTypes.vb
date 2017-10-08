
''' <summary>
''' Csv file reader for the csv file list on https://www.iana.org/assignments/media-types/media-types.xhtml
''' 
''' + [application](https://www.iana.org/assignments/media-types/application.csv)
''' + [audio](https://www.iana.org/assignments/media-types/audio.csv)
''' + [font](https://www.iana.org/assignments/media-types/font.csv)
''' + [example]()
''' + [image](https://www.iana.org/assignments/media-types/image.csv)
''' + [message](https://www.iana.org/assignments/media-types/message.csv)
''' + [model](https://www.iana.org/assignments/media-types/model.csv)
''' + [multipart](https://www.iana.org/assignments/media-types/multipart.csv)
''' + [text](https://www.iana.org/assignments/media-types/text.csv)
''' + [video](https://www.iana.org/assignments/media-types/video.csv)
''' </summary>
Public Class MediaTypes
    Public Property Name As String
    Public Property Template As String
    Public Property Reference As String

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
