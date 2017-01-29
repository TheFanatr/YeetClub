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
public class UserProfileActivity : AppCompatActivity {

    private const int SELECT_PHOTO = 2;
    protected List<ParseObject> mYeets;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    private RecyclerView recyclerView;
    private UserProfileAdapter adapter;

    public UserProfileActivity() {

    }


    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_profile);

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

        Bundle bundle = getIntent().getExtras();
        // If the bundle is not null then we have arrived at another user's profile
        if (bundle != null) {
            if (bundle.getString(ParseConstants.KEY_OBJECT_ID) != null) {
                string userId = bundle.getString(ParseConstants.KEY_OBJECT_ID);

                // Populate profile with Yeets from user we are visiting
                initialise(userId);

                // Enable click listener to change profile picture
                setSwipeRefreshLayout(isOnline, userId);

                // Enable click listener to change profile picture
                if (userId != null && userId.Equals(ParseUser.getCurrentUser().getObjectId())) {
                    setProfilePictureClickListener();
                }

                // HashSet up profile header for user we are visiting
                createProfileHeader(userId);
            }
        } else {
            // Came here as self, so set userId string to own ParseUser objectId
            string userId = ParseUser.getCurrentUser().getObjectId();

            // HashSet logic for swipe-to-refresh
            setSwipeRefreshLayout(isOnline, userId);

            // Initialise adapter data
            initialise(userId);

            // Enable click listener to change profile picture
            setProfilePictureClickListener();

            // Populate profile with Yeets from current user, i.e. self.
            createProfileHeader(userId);
        }

        // Floating action button
        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setBackgroundTintList(ColorStateList.valueOf(Color.parseColor("#169cee")));
        fab.setOnClickListener(view -> {

            Intent intent = new Intent(UserProfileActivity.this, YeetActivity.class);
            startActivity(intent);

        });

    }


    private bool initialise(string userId) {

        bool isOnline = NetworkHelper.isOnline(this);

        recyclerView = (RecyclerView) findViewById(R.id.recyclerView);
        recyclerView.setLayoutManager(new PreCachingLayoutManager(this));
        recyclerView.setHasFixedSize(true);

        // For image caching
        recyclerView.setItemViewCacheSize(20);
        recyclerView.setDrawingCacheEnabled(true);

        retrieveYeets(userId, isOnline);

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                createProfileHeader(userId);
                retrieveYeets(userId, true);
            }
        });

        return isOnline;
    }


    private void setProfilePictureClickListener() {
        ImageView profilePicture = (ImageView) findViewById(R.id.profile_picture);
        profilePicture.setOnClickListener(view -> {
            view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
            ChangeProfilePicture();
        });
    }


    public void ChangeProfilePicture() {
        Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
        photoPickerIntent.setType("image/*");
        startActivityForResult(photoPickerIntent, SELECT_PHOTO);
    }


    override protected void onActivityResult(int requestCode, int resultCode, Intent imageReturnedIntent) {
        base.onActivityResult(requestCode, resultCode, imageReturnedIntent);

        switch (requestCode) {
            case SELECT_PHOTO:
                if (resultCode == RESULT_OK) {
                    Uri selectedImage = imageReturnedIntent.getData();
                    StreamReader imageStream = null;
                    try {
                        imageStream = getContentResolver().openInputStream(selectedImage);
                    } catch (FileNotFoundException e) {
                        e.printStackTrace();
                    }
                    Bitmap yourSelectedImage = BitmapFactory.decodeStream(imageStream);
                    Bitmap croppedThumbnail = ThumbnailUtils.extractThumbnail(yourSelectedImage, 144, 144, ThumbnailUtils.OPTIONS_RECYCLE_INPUT);
                    ParseHelper.UploadProfilePictureToCurrentUser(croppedThumbnail);
                    RefreshGalleryActivity();
                }
        }
    }


    // Relaunches UserProfileActivity
    public void RefreshGalleryActivity() {
        Toast.makeText(getApplicationContext(), "Profile picture uploaded successfully", Toast.LENGTH_SHORT).show();
        Intent intent = new Intent(this, UserProfileActivity.class);
        finish();
        startActivity(intent);
    }


    private void setSwipeRefreshLayout(bool isOnline, string userId) {
        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                mSwipeRefreshLayout.setRefreshing(false);

                // HashSet data to adapter
                retrieveYeets(userId, true);

                // Update user profile data
                if (!(userId.IsEmpty()) && userId.Equals(ParseUser.getCurrentUser().getObjectId())) {
                    createProfileHeader(userId);
                }

            }
        });
    }


    private void createProfileHeader(string userId) {
        TextView topLevelFullName = (TextView) findViewById(R.id.fullName);
        ImageView topLevelVerified = (ImageView) findViewById(R.id.verified);
        TextView topLevelBio = (TextView) findViewById(R.id.bio);
        TextView topLevelBae = (TextView) findViewById(R.id.bae);
        TextView topLevelWebsiteLink = (TextView) findViewById(R.id.websiteLink);
        ImageView topLevelProfilePicture = (ImageView) findViewById(R.id.profile_picture);

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        Typeface tf_black = Typeface.createFromAsset(getAssets(), "fonts/Lato-Black.ttf");

        // Query the User class with the objectId that was sent with us here from the Intent bundle
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereContains(ParseConstants.KEY_OBJECT_ID, userId);
        query.findInBackground((headerUser, e) -> {

            if (e == null) {

                foreach (ParseObject headerUserObject in headerUser) {

                    // HashSet full name
                    if (headerUserObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME) != null) {
                        if (headerUserObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME).IsEmpty()) {
                            topLevelFullName.setVisibility(View.GONE);
                        }

                        string topLevelFullNameText = headerUserObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME);
                        topLevelFullName.setText(topLevelFullNameText);
                        topLevelFullName.setTypeface(tf_black);
                    } else {
                        topLevelFullName.setVisibility(View.GONE);
                    }

                    // HashSet Verified badge
                    if (headerUserObject.getBoolean(ParseConstants.KEY_VERIFIED)) {
                        topLevelVerified.setVisibility(View.VISIBLE);
                        topLevelVerified.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                            Toast.makeText(getApplicationContext(), headerUserObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME) + " is a verified user.", Toast.LENGTH_SHORT).show();
                        });
                    } else {
                        topLevelVerified.setVisibility(View.GONE);
                    }

                    // HashSet bio
                    if (headerUserObject.getString(ParseConstants.KEY_USER_BIO) != null) {
                        if (headerUserObject.getString(ParseConstants.KEY_USER_BIO).IsEmpty()) {
                            topLevelBio.setVisibility(View.GONE);
                        }

                        string headerBioText = headerUserObject.getString(ParseConstants.KEY_USER_BIO);
                        topLevelBio.setText(headerBioText);
                        topLevelBio.setTypeface(tf_reg);
                    } else {
                        topLevelBio.setVisibility(View.GONE);
                    }

                    // HashSet bae
                    if (headerUserObject.getString("bae") != null) {
                        if (headerUserObject.getString("bae").IsEmpty()) {
                            topLevelBae.setVisibility(View.GONE);
                        }

                        string headerBaeText = headerUserObject.getString("bae");
                        topLevelBae.setText(headerBaeText.ToUpper());
                        topLevelBae.Append(" " + getString(R.string.is_bae));
                        topLevelBae.setTypeface(tf_reg);
                    } else {
                        topLevelBae.setVisibility(View.GONE);
                    }

                    // HashSet website link
                    if (headerUserObject.getString("websiteLink") != null) {
                        if (headerUserObject.getString("websiteLink").IsEmpty()) {
                            topLevelWebsiteLink.setVisibility(View.GONE);
                        }

                        string headerWebsiteLinkText = headerUserObject.getString("websiteLink");
                        topLevelWebsiteLink.setText(headerWebsiteLinkText);
                        topLevelWebsiteLink.setTypeface(tf_reg);
                    } else {
                        topLevelWebsiteLink.setVisibility(View.GONE);
                    }

                    // HashSet profile picture
                    if (headerUserObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {

                        Picasso.with(getApplicationContext())
                                .load(headerUserObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl())
                                .placeholder(R.color.placeholderblue)
                                .memoryPolicy(MemoryPolicy.NO_CACHE).into(((ImageView) findViewById(R.id.profile_picture)));

                        fadeInProfilePicture();

                    } else {
                        topLevelProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }

                }

            }
        });
    }


    private void fadeInProfilePicture() {
        Animation animFadeIn;
        animFadeIn = AnimationUtils.loadAnimation(getApplicationContext(), R.anim.fadein);
        ImageView profilePicture = (ImageView) findViewById(R.id.profile_picture);
        profilePicture.setAnimation(animFadeIn);
        profilePicture.setVisibility(View.VISIBLE);
    }


    override protected void onResume() {
        base.onResume();
    }


    private void retrieveYeets(string userId, Boolean isOnline) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                // Log.w(GetType().ToString(), currentGroupObjectId);

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
                query.whereContains(ParseConstants.KEY_SENDER_AUTHOR_POINTER, userId);
                //  how only Yeets that match the current group of the user viewing the profile
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.addDescendingOrder(ParseConstants.KEY_LAST_REPLY_UPDATED_AT);
                query.findInBackground((yeets, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {

                        // We found messages!
                        mYeets = yeets;
                        ParseObject.pinAllInBackground(mYeets);

                        UserProfileAdapter adapter = new UserProfileAdapter(getApplicationContext(), yeets);
                        adapter.setHasStableIds(true);
                        /*RecyclerViewHeader header = (RecyclerViewHeader) findViewById(R.id.header);
                        header.attachTo(recyclerView);*/
                        recyclerView.setAdapter(adapter);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getApplicationContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                Date onRefreshDate = new Date();
                                /*Console.WriteLine(onRefreshDate.getTime());*/

                                createProfileHeader(userId);

                                refreshYeets(userId, onRefreshDate, adapter);
                            }
                        });

                    }
                });

            }
        });
    }


    private void refreshYeets(string userId, Date date, UserProfileAdapter adapter) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                // Log.w(GetType().ToString(), currentGroupObjectId);

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
                query.whereContains(ParseConstants.KEY_SENDER_AUTHOR_POINTER, userId);
                // Show only Yeets that match the current group of the user viewing the profile
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.orderByDescending(ParseConstants.KEY_LAST_REPLY_UPDATED_AT);
                if (date != null)
                    query.whereLessThanOrEqualTo(ParseConstants.KEY_CREATED_AT, date);
                query.setLimit(1000);
                query.findInBackground((yeets, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {
                        // We found messages!
                        mYeets.removeAll(yeets);
                        mYeets.addAll(0, yeets); //This should Append new messages to the top
                        adapter.notifyDataSetChanged();
                        ParseObject.pinAllInBackground(mYeets);

                                /*Console.WriteLine(yeets);*/
                        if (recyclerView.getAdapter() == null) {
                            adapter.setHasStableIds(true);
                            recyclerView.setHasFixedSize(true);
                            adapter.notifyDataSetChanged();
                            recyclerView.setAdapter(adapter);
                        } else {
                            adapter.notifyDataSetChanged();
                        }
                    }
                });
            }
        });
    }


    override public bool onCreateOptionsMenu(Menu menu) {

        MenuInflater inflater = getMenuInflater();

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            if (bundle.getString(ParseConstants.KEY_OBJECT_ID) != null) {
                string userId = bundle.getString(ParseConstants.KEY_OBJECT_ID);

                if (userId != null && userId.Equals(ParseUser.getCurrentUser().getObjectId())) {
                    inflater.inflate(R.menu.settings_profile, menu);
                }
            }
        } else {
            inflater.inflate(R.menu.settings_profile, menu);
        }

        return base.onCreateOptionsMenu(menu);
    }


    override public bool onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_edit_profile:
                Intent intent = new Intent(this, EditProfileActivity.class);
                startActivity(intent);
                return true;
            default:
                return base.onOptionsItemSelected(item);
        }
    }

}
}
