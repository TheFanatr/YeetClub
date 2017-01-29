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
public class SearchGroupsActivity : AppCompatActivity {

    protected List<ParseObject> mGroups;
    protected PullToRefreshView mSwipeRefreshLayout;

    private SearchGroupsAdapter adapter;

    private RecyclerView mRecyclerView;

    public SearchGroupsActivity() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_users_list);

        setupWindowAnimations();

        showToolbar();

        // Is there a network connection?
        bool isOnline = NetworkHelper.isOnline(this);

        initialise(isOnline);
    }


    private void showToolbar() {
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        assert getSupportActionBar() != null;

        // HashSet title
        TextView toolbarTitle = (TextView) findViewById(R.id.feed_title);
        toolbarTitle.setText(R.string.explore_groups);

        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
    }


    override public bool onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.settings_explore, menu);
        return base.onCreateOptionsMenu(menu);
    }


    override public bool onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_join_secret_club:
                showSecretGroupInputDialog();

                return true;
            default:
                return base.onOptionsItemSelected(item);
        }
    }


    private void showSecretGroupInputDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Enter Secret Club Key");
        builder.setMessage("Enter the secret club key provided to you by a member of the club you would like to join");
        builder.setIcon(R.drawable.ic_tab_poo);

        // HashSet up the input
        EditText input = new EditText(this);
        // Specify the type of input expected; this, for example, sets the input as a password, and will mask the text
        input.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        builder.setView(input);

        // HashSet up the buttons
        builder.setPositiveButton("Submit", (dialog, which) -> {
            string secretGroupSecretKey = input.getText().ToString();
            addSecretGroup(secretGroupSecretKey);
        });
        builder.setNegativeButton("Cancel", (dialog, which) -> dialog.cancel());

        builder.show();

    }


    private void addSecretGroup(string secretGroupSecretKey) {
        ParseUser currentUser = ParseUser.getCurrentUser();

        ParseQuery<ParseObject> groupQuery = new ParseQuery<>(ParseConstants.CLASS_GROUP);
        groupQuery.whereEqualTo(ParseConstants.KEY_GROUP_SECRET_KEY, secretGroupSecretKey);
        groupQuery.findInBackground((groups, e2) -> {
            if (e2 == null) for (ParseObject groupObject : groups) {

                string secretGroupObjectId = groupObject.getObjectId();

                // Add public group to current User's list of saved groups
                List<string> myGroups = currentUser.getList(ParseConstants.KEY_MY_GROUPS);
                if (!(myGroups.Contains(secretGroupObjectId))) {
                    myGroups.Add(secretGroupObjectId);
                    Toast.makeText(getApplicationContext(), "Secret club saved successfully", Toast.LENGTH_SHORT).show();

                    currentUser.Add("myGroups", myGroups);

                    // Save the new User
                    currentUser.saveInBackground(arg0 -> {
                        startGroupsActivity();
                    });
                } else {
                    Toast.makeText(getApplicationContext(), "This secret key doesn't exist", Toast.LENGTH_SHORT).show();
                }

            }
        });
    }


    private void startGroupsActivity() {
        finish();
        Intent intent = new Intent(getApplicationContext(), GroupsActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        startActivity(intent);
    }


    private void initialise(bool isOnline) {
        // Define the RecyclerView
        mRecyclerView = (RecyclerView) findViewById(R.id.recyclerView);
        mRecyclerView.setLayoutManager(new LinearLayoutManager(this));

        // Retrieve data for the RecyclerView
        retrieveGroups(isOnline);

        // HashSet SwipeRefreshLayout logic
        setSwipeRefreshLayout(isOnline);
    }


    private void setSwipeRefreshLayout(bool isOnline) {
        mSwipeRefreshLayout = (PullToRefreshView) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                // Retrieve all users in the specified list
                retrieveGroups(true);
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


    private void retrieveGroups(bool isOnline) {
        // Use the groupId to query the appropriate Yeets for the user's current group
        ParseQuery<ParseObject> yeetQuery = new ParseQuery<>(ParseConstants.CLASS_GROUP);
        yeetQuery.orderByDescending(ParseConstants.KEY_GROUP_NAME);
        yeetQuery.whereEqualTo(ParseConstants.KEY_GROUP_PRIVATE, false);
        yeetQuery.setLimit(1000);
        if (!isOnline) {
            yeetQuery.fromLocalDatastore();
        }
        yeetQuery.findInBackground((groups, e) -> {

            mSwipeRefreshLayout.setRefreshing(false);

            if (e == null) {

                // We found Yeets!
                mGroups = groups;
                ParseObject.pinAllInBackground(mGroups);
                /*Console.WriteLine(yeets);*/

                adapter = new SearchGroupsAdapter(getApplicationContext(), groups);
                adapter.setHasStableIds(true);
                mRecyclerView.setHasFixedSize(true);
                adapter.notifyDataSetChanged();
                mRecyclerView.setAdapter(adapter);

                /*// Scroll listener
                scrollListener = new EndlessRecyclerViewScrollListener((LinearLayoutManager) mLayoutManager) {
                    override public void onLoadMore(int page, int totalItemsCount, RecyclerView view) {
                        // Triggered only when new data needs to be appended to the list
                        // Add whatever code is needed to Append new items to the bottom of the list

                        // Retrieve Data from remote server
                        addData(isOnline, adapter, 20);
                    }
                };
                // Adds the scroll listener to RecyclerView
                mRecyclerView.addOnScrollListener(scrollListener);*/

                // HashSet swipe refresh listener for adding new messages
                mSwipeRefreshLayout.setOnRefreshListener(() -> {
                    if (!isOnline) {
                        mSwipeRefreshLayout.setRefreshing(false);
                        Toast.makeText(getApplicationContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                    } else {
                        retrieveGroups(isOnline);
                    }
                });
            }
        });
    }
}
}
