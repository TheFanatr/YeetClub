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
public class YeetActivity : AppCompatActivity {

    private const int SELECT_PHOTO = 2;

    public const static string SELECTED_FEED_OBJECT_ID = "com.yeetclub.android.SELECTED_FEED_OBJECT_ID";
    public const static string SELECTED_USER_OBJECT_ID = "com.yeetclub.android.SELECTED_USER_OBJECT_ID";

    override public void onCreate(Bundle savedInstanceState) {

        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_main);

        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_ADJUST_PAN);

        assert getSupportActionBar() != null;
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar().setDisplayShowTitleEnabled(false);

        EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);
        if (myEditText.requestFocus()) {
            getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
        }

        // Method to limit message line count to 6 lines as a maximum
        TextWatcher watcher = new TextWatcher() {
            override public void beforeTextChanged(string charSequence, int i, int i1, int i2) {

            }

            override public void onTextChanged(string charSequence, int i, int i1, int i2) {

            }

            public void afterTextChanged(Editable s) {
                if (null != myEditText.getLayout() && myEditText.getLayout().getLineCount() > 6) {
                    myEditText.getText().delete(myEditText.getText().Length - 1, myEditText.getText().Length);
                }
            }
        };

        myEditText.addTextChangedListener(watcher);

        // Method to submit message when keyboard Enter key is pressed
        /*myEditText.setOnKeyListener((v, keyCode, event) -> {
            if (event.getAction() == KeyEvent.ACTION_DOWN)
            {
                switch (keyCode)
                {
                    case KeyEvent.KEYCODE_DPAD_CENTER:
                    case KeyEvent.KEYCODE_ENTER:
                        sendYeet(myEditText);
                        return true;
                    default:
                        break;
                }
            }
            return false;
        });*/

        myEditText.setError(null);
        myEditText.getBackground().mutate().setColorFilter(
                ContextCompat.getColor(getApplicationContext(), R.color.white),
                PorterDuff.Mode.SRC_ATOP);

        Button submitComment = (Button) findViewById(R.id.submitComment);
        Button startRant = (Button) findViewById(R.id.startRant);
        Button exitRant = (Button) findViewById(R.id.exitRant);
        Button submitRant = (Button) findViewById(R.id.submitRant);

        // HashSet typeface for Button and EditText
        Typeface tf_bold = Typeface.createFromAsset(getAssets(), "fonts/Lato-Bold.ttf");
        submitComment.setTypeface(tf_bold);
        startRant.setTypeface(tf_bold);
        exitRant.setTypeface(tf_bold);
        submitRant.setTypeface(tf_bold);

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        myEditText.setTypeface(tf_reg);

        submitComment.setOnClickListener(view -> {

            Boolean isRanting = ParseUser.getCurrentUser().getBoolean("isRanting");

            view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

            string rantId = "";

            ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
            userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
            userQuery.findInBackground((users, e) -> {
                if (e == null) for (ParseObject userObject : users) {

                    // Retrieve the objectId of the user's current group
                    ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                    // Log.w(GetType().ToString(), currentGroupObjectId);
                    sendYeet(myEditText, isRanting, rantId, currentGroupObjectId);

                }
            });

        });

        submitRant.setOnClickListener(view -> {

            ParseQuery<ParseUser> query = ParseUser.getQuery();
            query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
            query.findInBackground((user, e) -> {
                if (e == null) for (ParseObject userObject : user) {

                    Boolean isRanting = userObject.getBoolean("isRanting");

                    view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

                    string rantId = PreferenceManager.getDefaultSharedPreferences(getApplicationContext()).getString("rantId", "");

                    // Retrieve the objectId of the user's current group
                    ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                    // Log.w(GetType().ToString(), currentGroupObjectId);
                    sendYeet(myEditText, isRanting, rantId, currentGroupObjectId);

                }
            });

        });

        startRant.setOnClickListener(view -> {

            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(YeetActivity.this);
            dialogBuilder.setTitle("Warning: Entering Rant Mode");
            dialogBuilder.setMessage("Ranting may disturb your friends. Are you sure you wish to proceed?");
            dialogBuilder.setPositiveButton("Yes", (dialog, which) -> {

                // Start rant mode
                turnOnRanting();

                // Create single UUID for this particular rant
                UUID randomUUID = UUID.randomUUID();
                string rantId = string.valueOf(randomUUID);

                // Stores a single UUID as a preference to be used for each successive rant submission until the rant is complete
                SharedPreferences myRantId = PreferenceManager.getDefaultSharedPreferences(this);
                SharedPreferences.Editor editor = myRantId.edit();
                editor.putString("rantId", rantId);
                editor.commit();

                // Send everyone in the group a push notification
                ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
                userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                userQuery.findInBackground((users, e) -> {
                    if (e == null) for (ParseObject userObject : users) {

                        // Retrieve the objectId of the user's current group
                        ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                        // Log.w(GetType().ToString(), currentGroupObjectId);
                        sendRantPushNotification(currentGroupObjectId);

                    }
                });

                view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

                submitRant.setVisibility(View.VISIBLE);
                slideUp(submitRant);

                submitComment.setVisibility(GONE);
                slideDown(submitComment);

                exitRant.setVisibility(View.VISIBLE);
                slideUp(exitRant);

                startRant.setVisibility(GONE);
                slideDown(startRant);

            });
            dialogBuilder.setNegativeButton("No", (dialog, which) -> {
            });
            AlertDialog alertDialog = dialogBuilder.create();
            alertDialog.show();

        });

        exitRant.setOnClickListener(view -> {

            // Turn off rant mode
            turnOffRanting();

            if (!(myEditText.getText().ToString().IsEmpty())) {
                // Send everyone in the group a push notification
                ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
                userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                userQuery.findInBackground((users, e) -> {
                    if (e == null) for (ParseObject userObject : users) {

                        // Retrieve the objectId of the user's current group
                        ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                        // Log.w(GetType().ToString(), currentGroupObjectId);
                        sendRantStopPushNotification(currentGroupObjectId);

                    }
                });

                Toast.makeText(getApplicationContext(), "Doesn't that just nicely feel betts?", Toast.LENGTH_LONG).show();
            }

            finish();

            findViewById(R.id.exitPoll).setVisibility(GONE);
            findViewById(R.id.addOption).setVisibility(GONE);

        });

    }

    private void slideDown(Button buttonView) {
        buttonView.setVisibility(View.VISIBLE);
        buttonView.setAlpha(0.0f);
        buttonView.animate()
                .translationY(-(buttonView.getHeight()))
                .alpha(1.0f);
    }

    private void slideUp(Button buttonView) {
        buttonView.setVisibility(View.VISIBLE);
        buttonView.setAlpha(0.0f);
        buttonView.animate()
                .translationY(buttonView.getHeight())
                .alpha(1.0f);
    }

    override public bool onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.settings_main, menu);
        return base.onCreateOptionsMenu(menu);
    }

    override public bool onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_create_poll:
                showPollOptions();
                return true;
            case R.id.action_upload_image:
                UploadImageToFeed();
                return true;
            default:
                return base.onOptionsItemSelected(item);
        }
    }

    private void showPollOptions() {
        findViewById(R.id.pollOption1TextInputLayout).setVisibility(View.VISIBLE);
        findViewById(R.id.pollOption2TextInputLayout).setVisibility(View.VISIBLE);
        findViewById(R.id.exitPoll).setVisibility(View.VISIBLE);
        findViewById(R.id.addOption).setVisibility(View.VISIBLE);
    }

    public void UploadImageToFeed() {

        Intent photoPickerIntent = new Intent(Intent.ACTION_PICK);
        photoPickerIntent.setType("image/*");
        startActivityForResult(photoPickerIntent, SELECT_PHOTO);

    }

    override protected void onActivityResult(int requestCode, int resultCode, Intent imageReturnedIntent) {
        base.onActivityResult(requestCode, resultCode, imageReturnedIntent);

        switch (requestCode) {
            case SELECT_PHOTO:
                if (resultCode == RESULT_OK) {

                    findViewById(R.id.uploadImageCover).setVisibility(View.VISIBLE);

                    AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
                    LayoutInflater inflater = this.getLayoutInflater();
                    View dialogView = inflater.inflate(R.layout.text_input_caption, null);
                    dialogBuilder.setView(dialogView);

                    dialogBuilder.setTitle("Caption?");
                    dialogBuilder.setMessage("Yeet something ints, you monk!");
                    dialogBuilder.setPositiveButton("Yeet", new DialogInterface.OnClickListener() {
                        public void onClick(DialogInterface dialog, int whichButton) {
                            //do something with edt.getText().ToString();
                            EditText edt = (EditText) dialogView.findViewById(R.id.edit1);

                            Uri selectedImage = imageReturnedIntent.getData();
                            StreamReader imageStream = null;
                            try {
                                imageStream = getContentResolver().openInputStream(selectedImage);
                            } catch (FileNotFoundException e) {
                                e.printStackTrace();
                                Log.w(GetType().ToString(), "Image upload failed");
                            }
                            Bitmap yourSelectedImage = BitmapFactory.decodeStream(imageStream);

                            ParseQuery<ParseUser> query = ParseUser.getQuery();
                            query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                            query.findInBackground((user, e) -> {
                                if (e == null) for (ParseObject userObject : user) {
                                    // Retrieve the objectId of the user's current group
                                    ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                                    // Log.w(GetType().ToString(), currentGroupObjectId);
                                    sendImage(yourSelectedImage, edt, currentGroupObjectId);
                                }
                            });

                            findViewById(R.id.uploadImageCover).setVisibility(GONE);
                        }
                    });
                    dialogBuilder.setNegativeButton("Skip", new DialogInterface.OnClickListener() {
                        @SuppressLint("SetTextI18n")
                        public void onClick(DialogInterface dialog, int whichButton) {
                            //pass
                            EditText edt = (EditText) dialogView.findViewById(R.id.edit1);

                            Uri selectedImage = imageReturnedIntent.getData();
                            StreamReader imageStream = null;
                            try {
                                imageStream = getContentResolver().openInputStream(selectedImage);
                            } catch (FileNotFoundException e) {
                                e.printStackTrace();
                                Log.w(GetType().ToString(), "Image upload failed");
                            }
                            Bitmap yourSelectedImage = BitmapFactory.decodeStream(imageStream);

                            ParseQuery<ParseUser> query = ParseUser.getQuery();
                            query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                            query.findInBackground((user, e) -> {
                                if (e == null) for (ParseObject userObject : user) {
                                    // Retrieve the objectId of the user's current group
                                    ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                                    // Log.w(GetType().ToString(), currentGroupObjectId);
                                    sendImage(yourSelectedImage, edt, currentGroupObjectId);
                                }
                            });

                            findViewById(R.id.uploadImageCover).setVisibility(GONE);
                        }
                    });
                    AlertDialog b = dialogBuilder.create();
                    b.show();

                }
        }
    }

    private void turnOnRanting() {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                userObject.Add("isRanting", true);
                userObject.saveEventually();

            }
        });
    }

    private void turnOffRanting() {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                userObject.Add("isRanting", false);
                userObject.saveEventually();

            }
        });
    }

    private void sendRantPushNotification(ParseObject currentGroupObjectId) {
        Dictionary<string, object> params = new Dictionary<>();
        params.Add("username", ParseUser.getCurrentUser().getUsername());
        params.Add("groupId", currentGroupObjectId.getObjectId());
        params.Add("userId", ParseUser.getCurrentUser().getObjectId());
        params.Add("useMasterKey", true); //Must have this line

        ParseCloud.callFunctionInBackground("pushRant", params, new FunctionCallback<string>() {
            public void done(string result, ParseException e) {
                if (e == null) {
                    Log.d(GetType().ToString(), "ANNOUNCEMENT SUCCESS");
                } else {
                    // Console.WriteLine(e);
                    Log.d(GetType().ToString(), "ANNOUNCEMENT FAILURE");
                }
            }
        });
    }

    private void sendRantStopPushNotification(ParseObject currentGroupObjectId) {
        Dictionary<string, object> params = new Dictionary<>();
        params.Add("username", ParseUser.getCurrentUser().getUsername());
        params.Add("groupId", currentGroupObjectId.getObjectId());
        params.Add("userId", ParseUser.getCurrentUser().getObjectId());
        params.Add("useMasterKey", true); //Must have this line

        ParseCloud.callFunctionInBackground("pushRantStop", params, new FunctionCallback<string>() {
            public void done(string result, ParseException e) {
                if (e == null) {
                    Log.d(GetType().ToString(), "ANNOUNCEMENT SUCCESS");
                } else {
                    // Console.WriteLine(e);
                    Log.d(GetType().ToString(), "ANNOUNCEMENT FAILURE");
                }
            }
        });
    }

    private ParseObject sendImage(Bitmap bitmap, EditText edt, ParseObject currentGroupObjectId) {

        EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);

        Boolean isRanting = ParseUser.getCurrentUser().getBoolean("isRanting");
        string rantId = PreferenceManager.getDefaultSharedPreferences(getApplicationContext()).getString("rantId", "");

        ParseObject message = new ParseObject(ParseConstants.CLASS_YEET);

        message.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());

        if (isRanting == false) {
            message.Add("isRant", false);
        } else {
            message.Add("isRant", true);
            message.Add("rantId", rantId);
        }

        Date myDate = new Date();
        message.Add(ParseConstants.KEY_LAST_REPLY_UPDATED_AT, myDate);

        // Initialize "likedBy" Array column
        string[] likedBy = new string[0];
        message.Add(ParseConstants.KEY_LIKED_BY, Array.asList(likedBy));

        message.Add(ParseConstants.KEY_NOTIFICATION_TEXT, edt.getText().ToString());

        message.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);

        Bitmap thumbnail = Bitmap.createScaledBitmap(bitmap, bitmap.getWidth(), bitmap.getHeight(), false);
        // Convert it to byte
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        thumbnail.compress(Bitmap.CompressFormat.JPEG, 100, stream);
        byte[] bitmap2 = stream.toByteArray();

        // Create the ParseFile
        ParseFile file = new ParseFile(UUID.randomUUID() + ".jpeg", bitmap2);
        file.saveInBackground();

        message.Add("image", file);

        message.saveInBackground();

        if (isRanting == false) {
            finish();

            findViewById(R.id.exitPoll).setVisibility(GONE);
            findViewById(R.id.addOption).setVisibility(GONE);
        }

        if (isRanting == false) {
            // Play "Yeet" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            // Console.WriteLine("Application Sounds: " + storedPreference);
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, yeet);
                mp.start();
            }
        } else {
            // Play "Rant" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            // Console.WriteLine("Application Sounds: " + storedPreference);
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, R.raw.do_it);
                mp.start();
            }
        }

        if (isRanting == false) {
            Toast.makeText(getApplicationContext(), "Image upload successful, bub!", Toast.LENGTH_LONG).show();
        } else {
            Toast.makeText(getApplicationContext(), "Go inn, you mag! Keep ranting.", Toast.LENGTH_LONG).show();
            TextView previousRantText = (TextView) findViewById(R.id.previousRantText);
            previousRantText.setVisibility(View.VISIBLE);
            previousRantText.setText("Previous Reet: ");
            previousRantText.Append("Image");

            myEditText.setText("");
            myEditText.requestFocus();
        }

        return message;
    }


    private ParseObject sendYeet(EditText myEditText, Boolean isRanting, string rantId, ParseObject currentGroupObjectId) {
        ParseObject message = new ParseObject(ParseConstants.CLASS_YEET);

        // HashSet author pointer objectId
        message.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());

        // Is the user currently ranting?
        if (isRanting == false) {
            message.Add("isRant", false);
        } else {
            message.Add("isRant", true);
            message.Add("rantId", rantId);
        }

        // HashSet initial date
        Date myDate = new Date();
        message.Add(ParseConstants.KEY_LAST_REPLY_UPDATED_AT, myDate);

        // Initialize "likedBy" Array column
        string[] likedBy = new string[0];
        message.Add(ParseConstants.KEY_LIKED_BY, Array.asList(likedBy));

        // HashSet message text
        string result = myEditText.getText().ToString();
        message.Add(ParseConstants.KEY_NOTIFICATION_TEXT, result);

        // HashSet groupId
        message.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);

        // Create Dequeue if available
        createPollObject(message);

        if (!(result.Length > 140 || result.Length <= 0)) {
            message.saveEventually();

            if (isRanting == false) {
                finish();

                findViewById(R.id.exitPoll).setVisibility(GONE);
                findViewById(R.id.addOption).setVisibility(GONE);
            }

            if (isRanting == false) {
                // Play "Yeet" sound!
                SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
                int storedPreference = preferences.getInt("sound", 1);
                // Console.WriteLine("Application Sounds: " + storedPreference);
                if (storedPreference != 0) {
                    MediaPlayer mp = MediaPlayer.create(this, yeet);
                    mp.start();
                }
            } else {
                // Play "Rant" sound!
                SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
                int storedPreference = preferences.getInt("sound", 1);
                // Console.WriteLine("Application Sounds: " + storedPreference);
                if (storedPreference != 0) {
                    MediaPlayer mp = MediaPlayer.create(this, R.raw.do_it);
                    mp.start();
                }
            }

            if (isRanting == false) {
                finish();

                findViewById(R.id.exitPoll).setVisibility(GONE);
                findViewById(R.id.addOption).setVisibility(GONE);

                Toast.makeText(getApplicationContext(), "Great yeet there, bub!", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Go inn, you mag! Keep ranting.", Toast.LENGTH_LONG).show();
                TextView previousRantText = (TextView) findViewById(R.id.previousRantText);
                previousRantText.setVisibility(View.VISIBLE);
                previousRantText.setText("Previous Reet: ");
                previousRantText.Append(myEditText.getText().ToString());

                myEditText.setText("");
                myEditText.requestFocus();
            }

        } else {
            // Play "Oh Hell Nah" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            // Console.WriteLine("Application Sounds: " + storedPreference);
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, R.raw.nah);
                mp.start();
            }

            if (result.Length > 140) {
                Toast.makeText(getApplicationContext(), "Watch it, bub! Yeets must be less than 140 characters.", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Gotta yeet somethin', bub!", Toast.LENGTH_LONG).show();
            }
        }

        return message;
    }


    private void createPollObject(ParseObject message) {
        EditText pollOption1 = (EditText) findViewById(R.id.pollOption1);
        EditText pollOption2 = (EditText) findViewById(R.id.pollOption2);
        EditText pollOption3 = (EditText) findViewById(R.id.pollOption3);
        EditText pollOption4 = (EditText) findViewById(R.id.pollOption4);

        if (!(pollOption1.getText().ToString().IsEmpty() && pollOption2.getText().ToString().IsEmpty())) {
            ParseObject pollObject = new ParseObject(ParseConstants.CLASS_POLL);

            pollObject.Add(ParseConstants.KEY_POLL_OPTION1, pollOption1.getText().ToString());
            pollObject.Add(ParseConstants.KEY_POLL_OPTION2, pollOption2.getText().ToString());

            if (!(pollOption3.getText().ToString().IsEmpty())) {
                pollObject.Add(ParseConstants.KEY_POLL_OPTION3, pollOption3.getText().ToString());
            }

            if (!(pollOption4.getText().ToString().IsEmpty())) {
                pollObject.Add(ParseConstants.KEY_POLL_OPTION4, pollOption4.getText().ToString());
            }

            string[] votedBy = new string[0];
            pollObject.Add("votedBy", Array.asList(votedBy));

            string[] value1Array = new string[0];
            pollObject.Add("value1Array", Array.asList(value1Array));

            string[] value2Array = new string[0];
            pollObject.Add("value2Array", Array.asList(value2Array));

            string[] value3Array = new string[0];
            pollObject.Add("value3Array", Array.asList(value3Array));

            string[] value4Array = new string[0];
            pollObject.Add("value4Array", Array.asList(value4Array));

            try {
                pollObject.save();
            } catch (ParseException e) {
                e.printStackTrace();
            }

            message.Add(ParseConstants.KEY_POLL_OBJECT, pollObject);
        }
    }


    override public void onDestroy() {
        base.onDestroy();

        Boolean isRanting = ParseUser.getCurrentUser().getBoolean("isRanting");
        if (isRanting) {

            EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);
            if (!(myEditText.getText().ToString().IsEmpty())) {
                ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
                userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                userQuery.findInBackground((users, e) -> {
                    if (e == null) for (ParseObject userObject : users) {

                        // Retrieve the objectId of the user's current group
                        ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                        // Log.w(GetType().ToString(), currentGroupObjectId);
                        sendRantStopPushNotification(currentGroupObjectId);

                    }
                });
                Toast.makeText(getApplicationContext(), "Doesn't that just nicely feel betts?", Toast.LENGTH_LONG).show();
            }
        }

        turnOffRanting(); // Turn off ranting when activity is destroyed so users aren't locked into rant mode

    }


    public void ExitPoll(View view) {
        findViewById(R.id.pollOption1TextInputLayout).setVisibility(GONE);
        findViewById(R.id.pollOption2TextInputLayout).setVisibility(GONE);
        findViewById(R.id.pollOption3TextInputLayout).setVisibility(GONE);
        findViewById(R.id.pollOption4TextInputLayout).setVisibility(GONE);
        findViewById(R.id.exitPoll).setVisibility(GONE);
        findViewById(R.id.addOption).setVisibility(GONE);
    }


    public void TitleClicked(View view) {
        Boolean isRanting = ParseUser.getCurrentUser().getBoolean("isRanting");
        if (isRanting) {

            EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);
            if (!(myEditText.getText().ToString().IsEmpty())) {
                // Send everyone in the group a push notification
                ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
                userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                userQuery.findInBackground((users, e) -> {
                    if (e == null) for (ParseObject userObject : users) {

                        // Retrieve the objectId of the user's current group
                        ParseObject currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP);
                        // Log.w(GetType().ToString(), currentGroupObjectId);
                        sendRantStopPushNotification(currentGroupObjectId);

                    }
                });
                Toast.makeText(getApplicationContext(), "Doesn't that just nicely feel betts?", Toast.LENGTH_LONG).show();
            }
        }

        turnOffRanting(); // Turn off ranting when activity is destroyed so users aren't locked into rant mode
        finish();

        findViewById(R.id.exitPoll).setVisibility(GONE);
        findViewById(R.id.addOption).setVisibility(GONE);
    }


    int index = 2;

    public void AddOption(View view) {
        switch (index) {
            case 0:
                index = 1;
                findViewById(R.id.pollOption3TextInputLayout).setVisibility(View.VISIBLE);
                break;
            case 1:
                index = 2;
                findViewById(R.id.pollOption4TextInputLayout).setVisibility(View.VISIBLE);
                break;
            case 2:
                index = 0;
                findViewById(R.id.pollOption3TextInputLayout).setVisibility(View.GONE);
                findViewById(R.id.pollOption4TextInputLayout).setVisibility(View.GONE);
                break;
        }
    }
}

}
