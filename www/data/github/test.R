imports ["Html", "http", "graphquery"] from "webKit";

const demo_url = "E:\GCModeller\src\runtime\sciBASIC#\www\data\github\test.html";
const query = graphquery::parseQuery('

	followers css("div") | css(".application-main") | css(".gutter-condensed ") | css(".position-relative") [
	{
		user css("div") | css(".width-full") [{
			
			text()
			
		}]
	}]
	
');

const document = readText(demo_url);

# print("the raw html document text that request from the remote web server:");
# cat("\n");
# print(document);

# cat("\n\n");

print("data query result from the html document text:");
cat("\n");
print(graphquery::query(Html::parse(document), query));