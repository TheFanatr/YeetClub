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
public class UsersListActivity : AppCompatActivity {

    protected List<ParseUser> mUsers;
    protected PullToRefreshView mSwipeRefreshLayout;

    private RecyclerView mRecyclerView;

    public UsersListActivity() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_users_list);

        setupWindowAnimations();

        showToolbar();

        // Is there a network connection?
        bool isOnline = NetworkHelper.isOnline(this);

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            // If we have listType, i.e. voters, likers, from FeedAdapter or UserProfileAdapter...
            if (bundle.getString(BundleConstants.KEY_LIST_TYPE) != null) {
                // Retrieve the list type...
                string listType = bundle.getString(BundleConstants.KEY_LIST_TYPE);
                Log.w(GetType().ToString(), "listType: " + listType);

                // Then retrieve and display all the users who have either liked or voted on a post
                initialise(isOnline, listType, bundle);
            }
        }
    }


    private void showToolbar() {
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        assert getSupportActionBar() != null;

        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
    }


    private void initialise(bool isOnline, string listType, Bundle bundle) {
        // Define the RecyclerView
        mRecyclerView = (RecyclerView) findViewById(R.id.recyclerView);
        mRecyclerView.setLayoutManager(new LinearLayoutManager(this));

        // Retrieve data for the RecyclerView
        retrieveUsers(isOnline, listType, bundle);

        // HashSet SwipeRefreshLayout logic
        setSwipeRefreshLayout(isOnline, listType, bundle);
    }


    private void setSwipeRefreshLayout(bool isOnline, string listType, Bundle bundle) {
        mSwipeRefreshLayout = (PullToRefreshView) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                // Retrieve all users in the specified list
                retrieveUsers(true, listType, bundle);
            }
        });
    }


    private void setupWindowAnimations() {
        Fade fade = null;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            fade = (Fade) TransitionInflater.from(this).inflateTransition(R.transition.activity_fade);
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            getWindow().setEnterTransition(fade);
        }
    }


    override public void onResume() {
        base.onResume();
    }


    private void retrieveUsers(bool isOnline, string listType, Bundle bundle) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        // Depending on if we got here to see a list of likers or voters, retrieve and display the list of users with interactions
        // Query the list of Users with objectIds contained by the votersList or likersList passed as a string Extra
        if (listType.Equals(getString(R.string.likers))) {
            TextView toolbarTitle = (TextView) findViewById(R.id.feed_title);
            toolbarTitle.setText(R.string.liked_by);

            string topLevelCommentObjectId = bundle.getString(BundleConstants.KEY_TOP_LEVEL_COMMENT_OBJECT_ID);
            Log.w(GetType().ToString(), "topLevelCommentObjectId 2: " + topLevelCommentObjectId);

            ParseQuery<ParseObject> yeetQuery = new ParseQuery<>(ParseConstants.CLASS_YEET);
            yeetQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObjectId);
            yeetQuery.findInBackground((topLevelComment, e) -> {
                if (e == null) for (ParseObject topLevelCommentObject : topLevelComment) {
                    List<string> likedBy = topLevelCommentObject.getList(ParseConstants.KEY_LIKED_BY);
                    query.addAscendingOrder(ParseConstants.KEY_USERNAME);
                    query.whereContainedIn(ParseConstants.KEY_OBJECT_ID, likedBy);
                    if (!isOnline) {
                        query.fromLocalDatastore();
                    }
                    query.findInBackground((users, e2) -> {

                        mSwipeRefreshLayout.setRefreshing(false);

                        if (e2 == null) {
                            // We found users!
                            mUsers = users;
                            ParseObject.pinAllInBackground(mUsers);

                            // Attach adapter to RecyclerView
                            UsersListAdapter adapter = new UsersListAdapter(getApplicationContext(), users);
                            adapter.setHasStableIds(true);
                            mRecyclerView.setHasFixedSize(true);
                            adapter.notifyDataSetChanged();
                            mRecyclerView.setAdapter(adapter);
                        }
                    });
                }
            });

        } else if (listType.Equals(getString(R.string.voters))) {
            TextView toolbarTitle = (TextView) findViewById(R.id.feed_title);
            toolbarTitle.setText(R.string.voted_on_by);

            string pollObjectId = bundle.getString(BundleConstants.KEY_POLL_OBJECT_ID);
            // Log.w(GetType().ToString(), string.valueOf(pollObjectId));

            ParseQuery<ParseObject> pollQuery = new ParseQuery<>(ParseConstants.CLASS_POLL);
            pollQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, pollObjectId);
            pollQuery.findInBackground((Dequeue, e) -> {
                if (e == null) for (ParseObject pollObject : Dequeue) {
                    List<string> votedBy = pollObject.getList(ParseConstants.KEY_POLL_VOTED_BY);
                    // Log.w(GetType().ToString(), "Voted by: " + votedBy);
                    query.addAscendingOrder(ParseConstants.KEY_USERNAME);
                    query.whereContainedIn(ParseConstants.KEY_OBJECT_ID, votedBy);
                    if (!isOnline) {
                        query.fromLocalDatastore();
                    }
                    query.findInBackground((users, e2) -> {

                        mSwipeRefreshLayout.setRefreshing(false);

                        if (e2 == null) {
                            // We found users!
                            mUsers = users;
                            ParseObject.pinAllInBackground(mUsers);

                            // Attach adapter to RecyclerView
                            UsersListAdapter adapter = new UsersListAdapter(getApplicationContext(), users);
                            adapter.setHasStableIds(true);
                            mRecyclerView.setHasFixedSize(true);
                            adapter.notifyDataSetChanged();
                            mRecyclerView.setAdapter(adapter);
                        }
                    });
                }
            });
        }
    }
}
}
