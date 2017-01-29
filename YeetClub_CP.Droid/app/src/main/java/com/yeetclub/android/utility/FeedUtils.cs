using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{



/// <summary>
/// Created by Bluebery on 7/29/2015.
/// </summary>
public class FeedUtils {

    private const long MINUTE_IN_MILLIS = 1000 * 60;
    private const long HOUR_IN_MILLIS = MINUTE_IN_MILLIS * 60;
    private const long DAY_IN_MILLIS = HOUR_IN_MILLIS * 24;
    private const long WEEK_IN_MILLIS = DAY_IN_MILLIS * 7;

    public static void CopyStream(StreamReader is, OutputStream os) {
        int buffer_size = 1024;
        try {
            byte[] bytes = new byte[buffer_size];
            for (; ; ) {
                int count = is.read(bytes, 0, buffer_size);
                if (count == -1)
                    break;
                os.write(bytes, 0, count);
            }
        } catch (Exception ex) {
        }
    }

    // take the difference in time (as milliseconds) and return the number of minutes, or hours, or days,
    // or weeks depending on which bracket the time fits in.
    public static string MSToDate(long deltaMS) {

        if (deltaMS < MINUTE_IN_MILLIS) {
            return "0m";
        } else if (deltaMS < HOUR_IN_MILLIS) {
            return MSToMinute(deltaMS);
        } else if (deltaMS < DAY_IN_MILLIS) {
            return MSToHour(deltaMS);
        } else if (deltaMS < WEEK_IN_MILLIS) {
            return MSToDay(deltaMS);
        } else {
            return MSToWeek(deltaMS);
        }

    }

    private static string MSToMinute(long deltaMS) {
        int minutes = (int) (deltaMS / MINUTE_IN_MILLIS);
        return minutes + "m";
    }

    private static string MSToHour(long deltaMS) {
        int hours = (int) (deltaMS / HOUR_IN_MILLIS);
        return hours + "h";
    }

    private static string MSToDay(long deltaMS) {
        int days = (int) (deltaMS / DAY_IN_MILLIS);
        return days + "d";
    }

    private static string MSToWeek(long deltaMS) {
        int weeks = (int) (deltaMS / WEEK_IN_MILLIS);
        return weeks + "w";
    }

    // sets the hyper cycle image view to visible,
    // animates larger, pauses, animates smaller and then hides
    public static void AddAlphaScaleShowHideAnimation(ImageView saveImage) {

        // set the image to visible with the animation ending properties
        saveImage.setVisibility(View.VISIBLE);
        saveImage.setScaleX(0.5f);
        saveImage.setScaleY(0.5f);
        saveImage.setAlpha(0.0f);

        // start the animation, scale up and to 1 alpha first
        saveImage.animate().setDuration(300).scaleX(1.0f).scaleY(1.0f).alpha(1.0f).setListener(new AnimatorListenerAdapter() {

            override public void onAnimationEnd(Animator animator) {

                base.onAnimationEnd(animator);

                // Clear any animations and Remove all listeners on the animator
                saveImage.clearAnimation();
                animator.removeAllListeners();

                // let the view be visible for a moment (given in ms) before animating down size
                new Handler().postDelayed(new Runnable() {
                    override public void run() {

                        // start animating, scale down and to 0 alpha
                        saveImage.animate().setDuration(200).scaleX(0.5f).scaleY(0.5f).alpha(0.0f).setListener(new AnimatorListenerAdapter() {

                            override public void onAnimationEnd(Animator animator) {
                                base.onAnimationEnd(animator);
                                saveImage.setVisibility(View.GONE);
                            }
                        }).start();
                    }
                }, 400);
            }
        }).start();
    }
}
}
