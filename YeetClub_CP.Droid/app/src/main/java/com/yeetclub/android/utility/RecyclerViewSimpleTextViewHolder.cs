using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{


/// <summary>
/// Created by santafebound on 4.7.2016.
/// </summary>

public class RecyclerViewSimpleTextViewHolder : RecyclerView.ViewHolder {

    private TextView label;

    public RecyclerViewSimpleTextViewHolder(View itemView) {
        base(itemView);
        label = (TextView) itemView.findViewById(android.R.id.text1);
    }

    public TextView getLabel() {
        return label;
    }

    public void setLabel(TextView label) {
        this.label = label;
    }
}
}
