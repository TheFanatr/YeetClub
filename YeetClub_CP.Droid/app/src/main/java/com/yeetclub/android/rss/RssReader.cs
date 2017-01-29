using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.rss
{



/// <summary>
/// Type reads RSS data.
/// @author ITCuties
/// </summary>
public class RssReader {
    // Our class has an attribute which represents RSS Feed Url
    private string rssUrl;
    /// <summary>
    /// We set this Url with the constructor
    /// </summary>
    public RssReader(string rssUrl) {
        this.rssUrl = rssUrl;
    }
    /// <summary>
    /// Get RSS items. This method will be called to get the parsing process result.
    /// @return
    /// </summary>
    public List<RssItem> getItems()  {
        // At first we need to get an SAX Parser Factory object
        SAXParserFactory factory = SAXParserFactory.newInstance();
        // Using factory we create a new SAX Parser instance
        SAXParser saxParser = factory.newSAXParser();
        // We need the SAX parser handler object
        RssParseHandler handler = new RssParseHandler();
        // We call the method parsing our RSS Feed
        saxParser.parse(rssUrl, handler);
        // The result of the parsing process is being stored in the handler object
        return handler.getItems();
    }
}
}
