# Syroot.IO.BinaryData

.NET library extending binary reading and writing functionality.
> + Adapted from the original work of https://github.com/Syroot/BinaryData
> + CodeProject: [&lt;A more powerful BinaryReader/Writer>](http://www.codeproject.com/Articles/1130187/A-more-powerful-BinaryReader-Writer)

## Introduction

When parsing or storing data in binary file formats, the functionality offered by the default .NET `BinaryReader` and `BinaryWriter` classes is often not sufficient. It totally lacks support for a different byte order than the one of the system and specific string or date formats (most prominently, 0-terminated strings instead of the default Int32 prefixed .NET strings).

Further, navigating in binary files is slightly tedious when it becomes required to skip to another chunk in the file and then navigate back. Also, aligning to specific block sizes might be a common task.

This NuGet package adds all this functionality by offering two new .NET 4.5 classes, `BinaryDataReader` and  `BinaryDataWriter`, which extend the aforementioned .NET reader and writer, usable in a similar way so that they are easy to implement into existing projects - in fact, they can be used directly without requiring any changes to existing code.

The usage is described in detail [on the wiki](https://github.com/Syroot/BinaryData/wiki).

## License

<a href="http://www.wtfpl.net/"><img src="./res/wtfpl.svg" height="20"/ ></a> WTFPL

    Copyright © 2016 syroot.com <admin@syroot.com>
    This work is free. You can redistribute it and/or modify it under the
    terms of the Do What The Fuck You Want To Public License, Version 2,
    as published by Sam Hocevar. See the COPYING file for more details.

## RepositoryEngine

### Repository index engine

The repository data was indexed by a binary search tree, where this binary search tree index file and the data file in structure like:

```
# index.data
key|data_jump_point|left_jump_point|right_jump_point|key2|data_jump_point2|left_jump_point2|right_jump_point2|...

# data
key_data|key2_data
```

The field key, key2, ... is the object index key in the repository system, it is a string in various length, and the ``data_jump_point``, ``data_jump_point2``, ... is the corresiponding byts offset in the repository data file, it is a fix length 8 bytes ``Long`` data type in VB.NET. The data jump point its value point to the begining offset in bytes of the correponding data object. Next is two 8 bytes fix length Long type offsets for the left/right node of current key.
