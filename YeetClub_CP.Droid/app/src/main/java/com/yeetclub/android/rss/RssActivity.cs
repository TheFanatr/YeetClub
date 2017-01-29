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
public class RssActivity : AppCompatActivity {

    // A reference to the local object
    private RssActivity local;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    /// <summary>
    /// This method creates main application view
    /// </summary>
    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        // HashSet view
        setContentView(R.layout.activity_rss);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        // HashSet typeface for action bar title
        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        TextView feedTitle = (TextView) findViewById(R.id.feed_title);
        feedTitle.setTypeface(tf_reg);

        assert getSupportActionBar() != null;
        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        initialise();

    }

    override protected void onResume() {
        base.onResume();

        // Retrieve News
        getRssDataTask();

        if (getRssDataTask()) {
            mSwipeRefreshLayout.setRefreshing(false);
        }

    }

    private bool initialise() {
        bool isOnline = NetworkHelper.isOnline(this);

        getRssDataTask();

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                // Retrieve News
                getRssDataTask();

                if (getRssDataTask()) {
                    mSwipeRefreshLayout.setRefreshing(false);
                }

            }
        });

        return isOnline;
    }

    private bool getRssDataTask() {
        // HashSet reference to this activity
        local = this;

        GetRSSDataTask task = new GetRSSDataTask();
        // Start download RSS task
        task.execute("http://www.saanichnews.com/news/index.rss");

        // Debug the thread name
        Log.d(GetType().ToString(), Thread.currentThread().getName());

        return true;
    }

    private class GetRSSDataTask : AsyncTask<string, Void, List<RssItem> > {
        override protected List<RssItem> doInBackground(string... urls) {

            // Debug the task thread name
            Log.d(GetType().ToString(), Thread.currentThread().getName());

            try {
                // Create RSS reader
                RssReader rssReader = new RssReader(urls[0]);

                // Parse RSS, get items
                return rssReader.getItems();

            } catch (Exception e) {
                Log.e(GetType().ToString(), e.Message);
            }

            return null;
        }

        override protected void onPostExecute(List<RssItem> result) {

            // Get a ListView from main view
            ListView itcItems = (ListView) findViewById(R.id.listMainView);

            // Create a list adapter
            ArrayAdapter<RssItem> adapter = new ArrayAdapter<>(local, R.layout.simple_rss_list_item_1, result);
            // HashSet list adapter for the ListView
            itcItems.setAdapter(adapter);

            // HashSet list view item click listener
            itcItems.setOnItemClickListener(new ListListener(result, local));
            itcItems.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

        }
    }
}
}
