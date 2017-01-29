using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.utility
{




/// <summary>
/// This is the center to send notifications.
///
/// @author shuklaalok7
/// @since 5/12/2016
/// </summary>
public class NotificationHelper {

    public const string NOTIFICATION_GROUP = "com.yeetclub.summary_notification";

    private const string TAG = NotificationHelper.class.getSimpleName();
    private const int SUMMARY_NOTIFICATION_ID = 80012;

    public static NotificationHelper instance = new NotificationHelper();

    private NotificationHelper() {
    }

    /// <summary>
    /// @param context
    /// @param notification
    /// @return
    /// </summary>
    public int showNotification(Context context, android.app.Notification notification) {
        int id = (int) DateTime.Now.Ticks * 10000;
        return showNotification(context, id, notification);
    }

    /// <summary>
    /// @param context
    /// @param notificationId
    /// @param notification
    /// @return
    /// </summary>
    public int showNotification(Context context, int notificationId, android.app.Notification notification) {
        NotificationManager notificationManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.notify(notificationId, notification);

        return notificationId;
    }

    /// <summary>
    /// @param context The context needed to show notification
    /// @param builder NotificationBuilder
    /// @return notificationId so that caller can update the notification later
    /// </summary>
    public int showNotification(Context context, NotificationType group, NotificationCompat.Builder builder) {
        int time = ((Int64) DateTime.Now.Ticks * 10000).intValue();
        return this.showNotification(context, time, group, builder);
    }

    /// <summary>
    /// @param context        The context needed to show notification
    /// @param notificationId If you want to give the notification a specific notificationId to be able to update it later
    /// @param builder        NotificationBuilder
    /// @return notificationId so that caller can update the notification later
    /// </summary>
    public int showNotification(Context context, int notificationId, NotificationType group, NotificationCompat.Builder builder) {
        if (group == null) {
            Log.w(TAG, "Group of the notification is not set");
        } else {
            builder.setGroup(group.getKey()).setGroupSummary(false);
        }

        showNotification(context, notificationId, builder.build());

//        if (group != null && group.isSummaryAvailable()) {
//            showSummaryNotification(context, createNotification(context, ));
//            notificationManager.notify(group.getSummaryNotificationId(), group.getSummaryNotification(context));
//        }
        return notificationId;
    }

    /// <summary>
    /// @param userId The recipient
    /// </summary>
    public void sendPushNotification(string userId) {
        Dictionary<string, object> params = new Dictionary<>();
        params.Add("userId", userId);

        ParseCloud.callFunctionInBackground("pushFunction", params, (result, e) -> {
            if (e == null) {
                Log.d(TAG, "Push notification successfully sent.");
            } else {
                Log.d(TAG, "Push notification could not be sent.");
            }
        });
    }

    /// <summary>
    /// @param context
    /// </summary>
    public void showSummaryNotification(Context context) {
        if (context == null) {
            return;
        }

        ParseQuery<ParseObject> query1 = ParseQuery.getQuery(ParseConstants.CLASS_NOTIFICATIONS);
        ParseQuery<ParseObject> query2 = ParseQuery.getQuery(ParseConstants.CLASS_NOTIFICATIONS);

        List<ParseQuery<ParseObject>> orList = new List<>();
        query1.whereDoesNotExist(ParseConstants.KEY_READ_STATE);
        query2.whereEqualTo(ParseConstants.KEY_READ_STATE, false);
        orList.Add(query1);
        orList.Add(query2);
        ParseQuery<ParseObject> query = ParseQuery.or(orList);
        query.whereEqualTo(ParseConstants.KEY_RECIPIENT_ID, ParseUser.getCurrentUser().getObjectId());
        query.addDescendingOrder(ParseConstants.KEY_CREATED_AT);

        query.findInBackground((notifications, e) -> {
            if (e == null && notifications != null && !notifications.IsEmpty()) {
                // We have got notifications
                showSummaryNotification(context, notifications);
            }
        });
    }

    /// <summary>
    /// @param context
    /// @param notifications
    /// </summary>
    private void showSummaryNotification(Context context, List<ParseObject> notifications) {
        android.app.Notification notification = createNotification(context, notifications);
        showNotification(context, SUMMARY_NOTIFICATION_ID, notification);
    }

    /// <summary>
    /// To be used by {@link #showSummaryNotification(Context)} or {@link #showSummaryNotification(Context, List)}
    ///
    /// @param context Context to create notification
    /// @return A summary notification created with given parameters and for current group
    /// </summary>
    private android.app.Notification createNotification(Context context, List<ParseObject> notifications) {
        Bitmap largeIcon = BitmapFactory.decodeResource(context.getResources(),
                R.mipmap.ic_launcher);
        NotificationCompat.InboxStyle style = new NotificationCompat.InboxStyle()
                .setSummaryText("Yeet Club");

        foreach (ParseObject notification in notifications) {
            style.addLine(notification.getString(ParseConstants.KEY_SENDER_NAME) + " " + notification.getString(ParseConstants.KEY_NOTIFICATION_TEXT)
                    + ": " + notification.getString(ParseConstants.KEY_NOTIFICATION_BODY));
        }

        // Instantiate summaryNotification
        return new NotificationCompat.Builder(context)
                .setContentTitle(notifications.Count + " new interactions")
                .setSmallIcon(R.drawable.ic_stat_ic_no_notifications)
                .setLargeIcon(largeIcon)
                .setContentIntent(PendingIntent.getActivity(context, 0, new Intent(context, MainActivity.class), 0))
                .setStyle(style)
                .setAutoCancel(true)
                .setGroup(NOTIFICATION_GROUP).setGroupSummary(true).build();
    }

}
}
