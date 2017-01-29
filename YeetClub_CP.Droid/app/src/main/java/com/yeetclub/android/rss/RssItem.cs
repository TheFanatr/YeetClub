using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.rss
{

/// <summary>
/// Type : a list listener.
/// @author ITCuties
/// </summary>
public class RssItem {
    // item title
    private string title;
    // item link
    private string link;
    public string getTitle() {
        return title;
    }
    public void setTitle(string title) {
        this.title = title;
    }
    public string getLink() {
        return link;
    }
    public void setLink(string link) {
        this.link = link;
    }
    override public string ToString() {
        return title;
    }
}
}
