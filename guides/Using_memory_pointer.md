## Using the memory pointer

For example, here is the code by using memory pointer to construct a grayscale image in very fast speed:

First, imports the ``Microsoft.VisualBasic.Emit`` namespace, we can using this memory pointer in a ``Using`` structure, as the ``End Using`` can release the memory block automaticly to avoid the memory leak problem.

```vbnet
Imports Microsoft.VisualBasic.Emit

' ptr parameter is a IntPtr pointer type that specific the &p location in the memory
' bytes is a Integer type to specific the length of the memory to modify
Using rgbValues As Marshal.Byte = New Marshal.Byte(ptr, bytes)
    Dim byts As Marshal.Byte = rgbValues

    ' Set every third value to 255. A 24bpp bitmap will binarization.
    Do While Not rgbValues.NullEnd(3)
        ' Get the red channel
        iR = rgbValues(2)
        ' Get the green channel
        iG = rgbValues(1)
        ' Get the blue channel
        iB = rgbValues(0)

        Dim luma = CInt(Math.Truncate(iR * 0.3 + iG * 0.59 + iB * 0.11))
        ' gray pixel
        byts(2) = luma
        byts(1) = luma
        byts(0) = luma
        byts += BinarizationStyles.Binary
    Loop
End Using
```