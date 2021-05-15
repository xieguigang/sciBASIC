# https://www.codeproject.com/Articles/1264613/GraphQuery-Powerful-Text-Query-Language-3

graphquery
{
    bookID css("book")|attr("id")
    title css("title")
    isbn xpath("//isbn")
    quote css("quote")
    language css("title")|attr("lang")
    author css("author") {
        name css("name")
        born css("born")
        dead css("dead")
    }
    character xpath("//character") [{
        name css("name")
        born css("born")
        qualification xpath("qualification")
    }]
}