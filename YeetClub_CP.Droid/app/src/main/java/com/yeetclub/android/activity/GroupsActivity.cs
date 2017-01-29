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
public class GroupsActivity : AppCompatActivity {

    protected List<ParseObject> mYeets;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    private RecyclerView recyclerView;

    public GroupsActivity() {

    }


    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_groups);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        assert getSupportActionBar() != null;
        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        // HashSet typeface for action bar title
        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        TextView feedTitle = (TextView) findViewById(R.id.feed_title);
        feedTitle.setTypeface(tf_reg);

        bool isOnline = NetworkHelper.isOnline(this);

        /*ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
            }
        });*/

        // HashSet logic for swipe-to-refresh
        setSwipeRefreshLayout(isOnline);

        // Populate list with groups belonging to the current User
        initialise(isOnline);

        // Create current group header information
        createGroupHeader();

        // Floating action button
        showFloatingActionButton();

    }

    private void showFloatingActionButton() {
        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setBackgroundTintList(ColorStateList.valueOf(Color.parseColor("#169cee")));
        fab.setOnClickListener(view -> {

            Intent intent = new Intent(GroupsActivity.this, YeetActivity.class);
            startActivity(intent);

        });
    }


    private bool initialise(Boolean isOnline) {

        recyclerView = (RecyclerView) findViewById(R.id.recyclerView);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        recyclerView.setHasFixedSize(true);

        retrieveGroups(isOnline);

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                createGroupHeader();
                retrieveGroups(true);
            }
        });

        return isOnline;
    }


    private void setSwipeRefreshLayout(bool isOnline) {
        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                mSwipeRefreshLayout.setRefreshing(false);

                // HashSet data to adapter
                retrieveGroups(true);

                // Update user profile data
                createGroupHeader();

            }
        });
    }


    private void createGroupHeader() {
        ImageView topLevelGroupProfilePicture = (ImageView) findViewById(R.id.groupProfilePicture);
        TextView topLevelGroupName = (TextView) findViewById(R.id.groupName);
        TextView topLevelGroupDescription = (TextView) findViewById(R.id.groupDescription);

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        Typeface tf_black = Typeface.createFromAsset(getAssets(), "fonts/Lato-Black.ttf");

        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                // Use the group objectId to query the actual groupId of the queried group
                ParseQuery<ParseObject> groupQuery = new ParseQuery<>(ParseConstants.CLASS_GROUP);
                groupQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, currentGroupObjectId);
                groupQuery.findInBackground((groups, e2) -> {
                    if (e2 == null) for (ParseObject groupObject : groups) {

                        // HashSet Group name
                        if (groupObject.getString(ParseConstants.KEY_GROUP_NAME) != null) {
                            if (groupObject.getString(ParseConstants.KEY_GROUP_NAME).IsEmpty()) {
                                topLevelGroupName.setVisibility(View.GONE);
                            }

                            string topLevelGroupNameText = groupObject.getString(ParseConstants.KEY_GROUP_NAME);
                            topLevelGroupName.setText(topLevelGroupNameText);
                            topLevelGroupName.setTypeface(tf_black);
                        } else {
                            topLevelGroupName.setVisibility(View.GONE);
                        }

                        // HashSet Group description
                        if (groupObject.getString(ParseConstants.KEY_GROUP_DESCRIPTION) != null) {
                            if (groupObject.getString(ParseConstants.KEY_GROUP_DESCRIPTION).IsEmpty()) {
                                topLevelGroupDescription.setVisibility(View.GONE);
                            }

                            string topLevelGroupDescriptionText = groupObject.getString(ParseConstants.KEY_GROUP_DESCRIPTION);
                            topLevelGroupDescription.setText(topLevelGroupDescriptionText);
                            topLevelGroupDescription.setTypeface(tf_reg);
                        } else {
                            topLevelGroupDescription.setVisibility(View.GONE);
                        }

                        // HashSet Group profile picture
                        if (groupObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {

                            Picasso.with(getApplicationContext())
                                    .load(groupObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl())
                                    .placeholder(R.color.placeholderblue)
                                    .memoryPolicy(MemoryPolicy.NO_CACHE).into(((ImageView) findViewById(R.id.groupProfilePicture)));

                            fadeInProfilePicture();

                        } else {
                            topLevelGroupProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                        }

                    }
                });

            }
        });
    }


    private void fadeInProfilePicture() {
        Animation animFadeIn;
        animFadeIn = AnimationUtils.loadAnimation(getApplicationContext(), R.anim.fadein);
        ImageView profilePicture = (ImageView) findViewById(R.id.groupProfilePicture);
        profilePicture.setAnimation(animFadeIn);
        profilePicture.setVisibility(View.VISIBLE);
    }


    override public bool onCreateOptionsMenu(Menu menu) {
        // Check if current User is the admin of the currently selected group, then show edit group option
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {

                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                ParseQuery<ParseObject> groupQuery = new ParseQuery<>(ParseConstants.CLASS_GROUP);
                groupQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, currentGroupObjectId);
                groupQuery.findInBackground((groups, e2) -> {
                    // Find the single Comment object associated with the current ListAdapter position
                    if (e2 == null) for (ParseObject groupObject : groups) {
                        // Retrieve the admin List for the user's current group
                        List<string> currentUserAdminList = groupObject.getList(ParseConstants.KEY_GROUP_ADMIN_LIST);
                        string currentUserObjectId = ParseUser.getCurrentUser().getObjectId();

                        // If the current User is on that list, show the "Edit Group" menu option
                        if (currentUserAdminList.Contains(currentUserObjectId)) {
                            MenuInflater inflater = getMenuInflater();
                            inflater.inflate(R.menu.settings_groups, menu);
                        }
                    }
                });

            }
        });

        return base.onCreateOptionsMenu(menu);
    }


    override public bool onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_edit_group:
                Intent intent = new Intent(this, EditGroupActivity.class);
                startActivity(intent);
                return true;
            case R.id.action_create_group:
                Intent intent2 = new Intent(this, CreateGroupActivity.class);
                startActivity(intent2);
                return true;
            case R.id.action_search_groups:
                Intent intent3 = new Intent(this, SearchGroupsActivity.class);
                startActivity(intent3);
                return true;
            default:
                return base.onOptionsItemSelected(item);
        }
    }


    override protected void onResume() {
        base.onResume();
    }


    private void retrieveGroups(Boolean isOnline) {

        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Query the Group class to retrieve the groups that the current User belongs to
                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_GROUP);

                List<string> myGroups = userObject.getList(ParseConstants.KEY_MY_GROUPS);
                Log.w(GetType().ToString(), myGroups.ToString());

                query.whereContainedIn(ParseConstants.KEY_OBJECT_ID, myGroups);
                query.addDescendingOrder(ParseConstants.KEY_CREATED_AT);
                query.findInBackground((yeets, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {

                        // We found groups!
                        mYeets = yeets;
                        ParseObject.pinAllInBackground(mYeets);

                        GroupsAdapter adapter = new GroupsAdapter(getApplicationContext(), yeets);
                        adapter.setHasStableIds(true);
                        /*RecyclerViewHeader header = (RecyclerViewHeader) findViewById(R.id.header);
                        header.attachTo(recyclerView);*/
                        recyclerView.setAdapter(adapter);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getApplicationContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                createGroupHeader();
                                retrieveGroups(true);
                            }
                        });

                    }
                });
            }
        });


    }

}
}
