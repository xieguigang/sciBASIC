

''' <summary>
''' Panose class
''' </summary>
''' <remarks>
''' The PANOSE structure describes the PANOSE font-classification
''' values for a TrueType font. These characteristics are then
''' used to associate the font with other fonts of similar
''' appearance but different names.
''' </remarks>
Public Class WinPanose

    ''' <summary>
    ''' Panose family type
    ''' </summary>
    Public Property bFamilyType As Byte

    ''' <summary>
    ''' Panose serif style
    ''' </summary>
    Public Property bSerifStyle As Byte

    ''' <summary>
    ''' Panose weight
    ''' </summary>
    Public Property bWeight As Byte

    ''' <summary>
    ''' Panose proportion
    ''' </summary>
    Public Property bProportion As Byte

    ''' <summary>
    ''' Panose contrast
    ''' </summary>
    Public Property bContrast As Byte

    ''' <summary>
    ''' Panose stroke variation
    ''' </summary>
    Public Property bStrokeVariation As Byte

    ''' <summary>
    ''' Panose arm style
    ''' </summary>
    Public Property bArmStyle As Byte

    ''' <summary>
    ''' Panose letter form
    ''' </summary>
    Public Property bLetterform As Byte

    ''' <summary>
    ''' Panose mid line
    ''' </summary>
    Public Property bMidline As Byte

    ''' <summary>
    ''' Panose X height
    ''' </summary>
    Public Property bXHeight As Byte

    Friend Sub New(DC As FontApi)
        bFamilyType = DC.ReadByte()
        bSerifStyle = DC.ReadByte()
        bWeight = DC.ReadByte()
        bProportion = DC.ReadByte()
        bContrast = DC.ReadByte()
        bStrokeVariation = DC.ReadByte()
        bArmStyle = DC.ReadByte()
        bLetterform = DC.ReadByte()
        bMidline = DC.ReadByte()
        bXHeight = DC.ReadByte()
        Return
    End Sub
End Class
