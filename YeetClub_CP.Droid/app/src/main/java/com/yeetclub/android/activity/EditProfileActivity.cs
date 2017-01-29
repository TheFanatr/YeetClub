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
public class EditProfileActivity : AppCompatActivity {

    private const int SELECT_PHOTO = 2;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        setContentView(R.layout.activity_edit_profile);

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
        ParseUser currentUser = ParseUser.getCurrentUser();
        if (currentUser == null) {
            return;
        }

        string userId = currentUser.getObjectId();

        bool isOnline = NetworkHelper.isOnline(this);

        // Hide or show views associated with network state
        LinearLayout ll = (LinearLayout) findViewById(R.id.linearLayout);
        ll.setVisibility(isOnline ? View.VISIBLE : View.GONE);
        findViewById(R.id.submitProfileChanges).setVisibility(isOnline ? View.VISIBLE : View.GONE);

        EditText fullNameField = (EditText) findViewById(R.id.fullName);
        EditText usernameField = (EditText) findViewById(R.id.username);
        EditText bioField = (EditText) findViewById(R.id.bio);
        EditText baeField = (EditText) findViewById(R.id.bae);
        EditText websiteField = (EditText) findViewById(R.id.websiteLink);

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                if (mSwipeRefreshLayout.isRefreshing()) {
                    mSwipeRefreshLayout.setRefreshing(false);
                }

                createProfileHeader(userId, fullNameField, usernameField, bioField, baeField, websiteField);
            }
        });

        setSubmitProfileChangesClickListener(fullNameField, usernameField, bioField, baeField, websiteField);

        setProfilePictureClickListener();

        // Populate the profile information from Parse
        createProfileHeader(userId, fullNameField, usernameField, bioField, baeField, websiteField);

    }


    private void setSubmitProfileChangesClickListener(EditText fullNameField, EditText usernameField, EditText bioField, EditText baeField, EditText websiteField) {
        findViewById(R.id.submitProfileChanges).setOnClickListener(v -> {

            // Update user
            ParseUser user1 = ParseUser.getCurrentUser();
            user1.Add("name", fullNameField.getText().ToString());
            user1.setUsername(usernameField.getText().ToString().ToLower().replaceAll("\\s", ""));
            user1.Add("websiteLink", websiteField.getText().ToString());
            user1.Add("bio", bioField.getText().ToString());
            user1.Add("bae", baeField.getText().ToString());

            // HashSet profile picture
            ImageView imageView = (ImageView) findViewById(R.id.profile_picture);
            Bitmap bitmap = ((BitmapDrawable) imageView.getDrawable()).getBitmap();
            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
            byte[] image = stream.toByteArray();
            ParseFile file = new ParseFile("profilePicture.png", image);
            user1.Add("profilePicture", file);

            user1.saveInBackground(e -> {
                        finish();
                        Toast.makeText(getApplicationContext(), "Profile updated successfully", Toast.LENGTH_SHORT).show();
                        Intent intent = new Intent(this, UserProfileActivity.class);
                        startActivity(intent);
                    }
            );
        });
    }


    private void createProfileHeader(string userId, EditText fullNameField, EditText usernameField, EditText bioField, EditText baeField, EditText websiteField) {

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");

        // HashSet typefaces for text fields
        fullNameField.setTypeface(tf_reg);
        usernameField.setTypeface(tf_reg);
        bioField.setTypeface(tf_reg);
        baeField.setTypeface(tf_reg);
        websiteField.setTypeface(tf_reg);

        ParseUser user = ParseUser.getCurrentUser();

        fullNameField.setText(user.getString("name"));
        usernameField.setText(user.getUsername());
        bioField.setText(user.getString("bio"));
        baeField.setText(user.getString("bae"));
        websiteField.setText(user.getString("websiteLink"));

        ParseQuery<ParseUser> query = ParseUser.getQuery();
        Log.w("User ID", userId);
        // Query the User class with the objectId that was sent with us here from the Intent bundle
        query.whereContains(ParseConstants.KEY_OBJECT_ID, userId);
        query.findInBackground((headerUser, e) -> {

            if (e == null) {

                foreach (ParseObject headerUserObject in headerUser) {

                    if (headerUserObject.getString("name") != null) {
                        string topLevelFullNameText = headerUserObject.getString("name");
                        fullNameField.setText(topLevelFullNameText);
                    }

                    if (headerUserObject.getString("bio") != null) {
                        string headerBioText = headerUserObject.getString("bio");
                        bioField.setText(headerBioText);
                        bioField.setTypeface(tf_reg);
                    }

                    if (headerUserObject.getString("bae") != null) {
                        string headerBaeText = headerUserObject.getString("bae");
                        baeField.setText(headerBaeText.ToUpper());
                        baeField.setTypeface(tf_reg);
                    }

                    if (headerUserObject.getString("websiteLink") != null) {
                        string headerWebsiteLinkText = headerUserObject.getString("websiteLink");
                        websiteField.setText(headerWebsiteLinkText);
                        websiteField.setTypeface(tf_reg);
                    }

                    if (headerUserObject.getParseFile("profilePicture") != null) {

                        Picasso.with(getApplicationContext())
                                .load(headerUserObject.getParseFile("profilePicture").getUrl())
                                .placeholder(R.color.placeholderblue)
                                .memoryPolicy(MemoryPolicy.NO_CACHE).into(((ImageView) findViewById(R.id.profile_picture)));

                        fadeInProfilePicture();
                    }

                }

            }
        });
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
        Intent intent = new Intent(this, EditProfileActivity.class);
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
