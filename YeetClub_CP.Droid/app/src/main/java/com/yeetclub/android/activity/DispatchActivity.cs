using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.activity
{


/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class DispatchActivity : ParseLoginDispatchActivity {

    override protected Type<?> getTargetClass() {
        return MainActivity.class;
    }
}
}
