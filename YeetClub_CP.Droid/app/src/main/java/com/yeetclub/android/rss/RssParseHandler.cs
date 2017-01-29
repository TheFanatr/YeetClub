using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.rss
{



/// <summary>
/// SAX tag handler. The Type Contains a list of RssItems which is being filled while the parser is working
/// @author ITCuties
/// </summary>
public class RssParseHandler : DefaultHandler {

    // List of items parsed
    private List<RssItem> rssItems;
    // We have a local reference to an object which is constructed while parser is working on an item tag
    // Used to reference item while parsing
    private RssItem currentItem;
    // We have two indicators which are used to differentiate whether a tag title or link is being processed by the parser
    // Parsing title indicator
    private bool parsingTitle;
    // Parsing link indicator
    private bool parsingLink;

    public RssParseHandler() {
        rssItems = new ArrayList();
    }
    // We have an access method which returns a list of items that are read from the RSS feed. This method will be called when parsing is done.
    public List<RssItem> getItems() {
        return rssItems;
    }
    // The StartElement method creates an empty RssItem object when an item start tag is being processed. When a title or link tag are being processed appropriate indicators are set to true.
    override public void startElement(string uri, string localName, string qName, Attributes attributes)  {
        if ("item".Equals(qName)) {
            currentItem = new RssItem();
        } else if ("title".Equals(qName)) {
            parsingTitle = true;
        } else if ("link".Equals(qName)) {
            parsingLink = true;
        }
    }
    // The EndElement method adds the  current RssItem to the list when a closing item tag is processed. It sets appropriate indicators to false -  when title and link closing tags are processed
    override public void endElement(string uri, string localName, string qName)  {
        if ("item".Equals(qName)) {
            rssItems.Add(currentItem);
            currentItem = null;
        } else if ("title".Equals(qName)) {
            parsingTitle = false;
        } else if ("link".Equals(qName)) {
            parsingLink = false;
        }
    }
    // Characters method fills current RssItem object with data when title and link tag content is being processed
    override public void characters(char[] ch, int start, int length)  {
        if (parsingTitle) {
            if (currentItem != null)
                currentItem.setTitle(new string(ch, start, length));
        } else if (parsingLink) {
            if (currentItem != null) {
                currentItem.setLink(new string(ch, start, length));
                parsingLink = false;
            }
        }
    }
}
}
