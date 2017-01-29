using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.application
{




/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class GlobalApplication : Application {
    /// <summary>
    /// Enum used to identify the tracker that needs to be used for tracking.
    ///
    /// A single tracker is usually enough for most purposes. In case you do need multiple trackers,
    /// storing them all in Application object helps ensure that they are created only once per
    /// application instance.
    /// </summary>

    override public void onCreate()
    {
        base.onCreate();

        Neumob.initialize(getApplicationContext(), "cH9iZERGkmqBhPUR");

        Parse.enableLocalDatastore(this);

        Parse.initialize(new Parse.Configuration.Builder(getApplicationContext())
                .applicationId(getString(R.string.parse_app_id))
                .clientKey(getString(R.string.parse_client_key))
                .server("https://parseapi.back4app.com/")
                .enableLocalDataStore()
                .build()
        );

        Parse.setLogLevel(Parse.LOG_LEVEL_DEBUG);

        ParsePush.subscribeInBackground("", e -> {
            if (e == null) {
                Log.d("com.parse.push", "successfully subscribed to the broadcast channel.");
            } else {
                Log.e("com.parse.push", "failed to subscribe for push", e);
            }
        });
    }

    public enum TrackerName {
        APP_TRACKER, // Tracker used only in this app.
        GLOBAL_TRACKER, // Tracker used by all the apps from a company. eg: roll-up tracking.
        //ECOMMERCE_TRACKER, // Tracker used by all ecommerce transactions from a company.
    }

    Dictionary<TrackerName, Tracker> mTrackers = new Dictionary<TrackerName, Tracker>();

    private const string TAG = "GlobalApplication";
    public static int GENERAL_TRACKER = 0;

    private const string PROPERTY_ID = "UA-57884954-1";

    public GlobalApplication() {
        base();
    }

    public synchronized Tracker getTracker(TrackerName trackerId) {

        if (!mTrackers.ContainsKey(trackerId)) {

            GoogleAnalytics analytics = GoogleAnalytics.getInstance(this);
            Tracker t = null;

            // Global GA Settings
            // <!-- Google Analytics SDK V4 BUG20141213 Using a GA global xml freezes the app! Do config by coding. -->
            // http://stackoverflow.com/questions/27533679/google-analytics-blocks-android-app/27542483#27542483
            analytics.setDryRun(false);

            analytics.getLogger().setLogLevel(Logger.LogLevel.INFO);
            //analytics.getLogger().setLogLevel(Logger.LogLevel.VERBOSE);

            analytics.setLocalDispatchPeriod(30);

            switch(trackerId) {
                case APP_TRACKER:
                    t = analytics.newTracker(R.xml.app_tracker);
                    break;
                case GLOBAL_TRACKER:
                    t = analytics.newTracker(PROPERTY_ID);
                    break;
            }

            // To enable Display Advertising features for Android, modify your Google Analytics tracking code to collect the advertising id.
            // Demographics and Interest Reports
            if(t != null) {
                t.enableAdvertisingIdCollection(true);
            }

            mTrackers.Add(trackerId, t);
        }

        return mTrackers[trackerId];
    }

}
}
