using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{

/// <summary>
/// Created by @santafebound on 2016-10-01.
/// </summary>

public class ResizeableImageView : ImageView {
    public ResizeableImageView(Context context, AttributeSet attrs) {
        base(context, attrs);
    }

    public ResizeableImageView(Context context) {
        base(context);
    }

    override protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        Drawable d = getDrawable();
        if (d == null) {
            base.setMeasuredDimension(widthMeasureSpec, heightMeasureSpec);
            return;
        }

        int imageHeight = d.getIntrinsicHeight();
        int imageWidth = d.getIntrinsicWidth();

        int widthSize = MeasureSpec.getSize(widthMeasureSpec);
        int heightSize = MeasureSpec.getSize(heightMeasureSpec);

        float imageRatio = 0.0F;
        if (imageHeight > 0) {
            imageRatio = imageWidth / imageHeight;
        }
        float sizeRatio = 0.0F;
        if (heightSize > 0) {
            sizeRatio = widthSize / heightSize;
        }

        int width;
        int height;
        if (imageRatio >= sizeRatio) {
            // set width to maximum allowed
            width = widthSize;
            // scale height
            height = width * imageHeight / imageWidth;
        } else {
            // set height to maximum allowed
            height = heightSize;
            // scale width
            width = height * imageWidth / imageHeight;
        }

        setMeasuredDimension(width, height);
    }
}
}
