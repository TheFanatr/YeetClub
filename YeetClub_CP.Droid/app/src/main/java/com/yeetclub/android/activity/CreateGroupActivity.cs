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
public class CreateGroupActivity : AppCompatActivity {

    private const int SELECT_PHOTO = 2;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        setContentView(R.layout.activity_create_group);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        // HashSet typeface for action bar title
        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        TextView feedTitle = (TextView) findViewById(R.id.edit_profile_title);
        feedTitle.setTypeface(tf_reg);

        assert getSupportActionBar() != null;
        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        // Initiate ParseQuery
        ParseUser currentUser = getCurrentUser();
        if (currentUser == null) {
            return;
        }

        bool isOnline = NetworkHelper.isOnline(this);

        // Hide or show views associated with network state
        LinearLayout ll = (LinearLayout) findViewById(R.id.linearLayout);
        ll.setVisibility(isOnline ? View.VISIBLE : View.GONE);
        findViewById(R.id.submitProfileChanges).setVisibility(isOnline ? View.VISIBLE : View.GONE);

        EditText nameField = (EditText) findViewById(R.id.name);
        EditText bioField = (EditText) findViewById(R.id.bio);

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                if (mSwipeRefreshLayout.isRefreshing()) {
                    mSwipeRefreshLayout.setRefreshing(false);
                }

                createProfileHeader(nameField, bioField);
            }
        });

        setSubmitProfileChangesClickListener(nameField, bioField);

        setProfilePictureClickListener();

        // Populate the profile information from Parse
        createProfileHeader(nameField, bioField);
    }


    private void setSubmitProfileChangesClickListener(EditText nameField, EditText bioField) {
        findViewById(R.id.submitProfileChanges).setOnClickListener(v -> {

            // Create new group
            ParseObject newGroup = new ParseObject(ParseConstants.CLASS_GROUP);

            // HashSet standard fields
            newGroup.Add("name", nameField.getText().ToString());
            newGroup.Add("description", bioField.getText().ToString());
            newGroup.Add("private", true);

            // HashSet default secret key
            string idOne = UUID.randomUUID().ToString();
            newGroup.Add("secretKey", idOne);

            // HashSet initial admin list
            string currentUserObjectId = ParseUser.getCurrentUser().getObjectId();
            string[] currentUserAdminList = {currentUserObjectId};
            newGroup.Add(ParseConstants.KEY_GROUP_ADMIN_LIST, Array.asList(currentUserAdminList));

            // HashSet profile picture
            ImageView imageView = (ImageView) findViewById(R.id.profile_picture);
            Bitmap bitmap = ((BitmapDrawable) imageView.getDrawable()).getBitmap();
            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
            byte[] image = stream.toByteArray();
            ParseFile file = new ParseFile("profilePicture.png", image);
            newGroup.Add("profilePicture", file);

            newGroup.saveInBackground(e -> {
                        Toast.makeText(getApplicationContext(), "Club created successfully", Toast.LENGTH_SHORT).show();
                        finish();
                        Intent intent = new Intent(this, GroupsActivity.class);
                        startActivity(intent);

                        string newGroupObjectId = newGroup.getObjectId();

                        ParseUser currentUser = ParseUser.getCurrentUser();

                        // Update myGroups list
                        List<string> myGroups = currentUser.getList(ParseConstants.KEY_MY_GROUPS);
                        myGroups.Add(newGroupObjectId);
                        currentUser.Add("myGroups", myGroups);

                    }
            );
        });
    }


    private void createProfileHeader(EditText nameField, EditText bioField) {

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");

        // HashSet typefaces for text fields
        nameField.setTypeface(tf_reg);
        bioField.setTypeface(tf_reg);

        // fadeInProfilePicture();
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
                    ImageView selectedProfilePicture = (ImageView) findViewById(R.id.profile_picture);
                    selectedProfilePicture.setImageBitmap(croppedThumbnail);
                }
        }
    }


    // Relaunches UserProfileActivity
    public void RefreshGalleryActivity() {
        Toast.makeText(getApplicationContext(), "Profile picture uploaded successfully", Toast.LENGTH_SHORT).show();
        finish();
        Intent intent = new Intent(this, CreateGroupActivity.class);
        startActivity(intent);
    }


    private void fadeInProfilePicture() {
        Animation animFadeIn;
        animFadeIn = AnimationUtils.loadAnimation(getApplicationContext(), R.anim.fadein);
        ImageView profilePicture = (ImageView) findViewById(R.id.profile_picture);
        profilePicture.setAnimation(animFadeIn);
        profilePicture.setVisibility(View.VISIBLE);
    }


    // Relaunches the activity
    public void RefreshActivity() {
        Intent intent = getIntent();
        finish();
        startActivity(intent);
    }

}
}
