using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{


/// <summary>
/// Created by Martin on 2016-12-03.
/// </summary>

public class AlertDialogFragment : DialogFragment {

    Context mContext;

    public AlertDialogFragment() {
        mContext = getActivity();
    }

    override public Dialog onCreateDialog(Bundle savedInstanceState) {
        AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(mContext);
        alertDialogBuilder.setTitle("Really?");
        alertDialogBuilder.setMessage("Are you sure?");
        alertDialogBuilder.setPositiveButton("OK", null);
        alertDialogBuilder.setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss());
        alertDialogBuilder.show();

        return alertDialogBuilder.create();
    }


}
}
