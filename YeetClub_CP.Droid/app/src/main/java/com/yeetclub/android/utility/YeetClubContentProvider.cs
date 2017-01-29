using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{



/// <summary>
/// Created by Bluebery on 10/25/2015.
/// </summary>
public class YeetClubContentProvider : ContentProvider {

    override public ParcelFileDescriptor openFile(Uri uri, string mode)  {
        File privateFile = new File(getContext().getFilesDir(), uri.getPath());
        return ParcelFileDescriptor.open(privateFile, ParcelFileDescriptor.MODE_READ_ONLY);
    }

    override public int delete(Uri uri, string selection, string[] selectionArgs) {
        // Implement this to handle requests to delete one or more rows.
        return 0;
    }

    override public string getType(Uri uri) {
        // TODO: Implement this to handle requests for the MIME type of the data
        // at the given Uri.
       // throw new UnsupportedOperationException("Not yet implemented");
        return "image/png";
    }

    override public Uri insert(Uri uri, ContentValues values) {
        // TODO: Implement this to handle requests to insert a new row.
        return null;
    }

    override public bool onCreate() {
        // TODO: Implement this to initialize your content provider on startup.
        return false;
    }

    override public Cursor query(Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder) {
        // TODO: Implement this to handle query requests from clients.
        return null;
    }

    override public int update(Uri uri, ContentValues values, string selection, string[] selectionArgs) {
        // TODO: Implement this to handle requests to update one or more rows.
        return 0;
    }
}
}
