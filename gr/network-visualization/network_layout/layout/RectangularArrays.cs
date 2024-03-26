//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2020 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert Java rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
    public static float[][] RectangularFloatArray(int size1, int size2)
    {
        float[][] newArray = new float[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new float[size2];
        }

        return newArray;
    }

    public static LinkedList[][] RectangularLinkedListArray(int size1, int size2)
    {
        LinkedList[][] newArray = new LinkedList[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new LinkedList[size2];
        }

        return newArray;
    }
}