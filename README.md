This is a console application for downloading a complete site. The application is built in .Net Core 6 in Visual Studio 2022.

Url to site and download folder is setup in appsettings.json.

Publish to folder with the included publish profile to create an executable.

Pages not ending with ".html" will get the filename index.html.

Only local files will be downloaded (url's with a relative path).

HtmlAgilityPack is used for finding links and files in html documents.

The application is prepared for use with logging system such as NLog but for now logging is done to console.

Only 1 unit test is created for now.