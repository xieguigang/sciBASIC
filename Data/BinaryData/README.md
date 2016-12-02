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
